#!/usr/bin/env python3
"""Report fused multiply-add instructions in a native library and decide whether any of
them can come from Jolt or joltc.

ELF and Mach-O libraries must contain none. Windows libraries statically link the MSVC
runtime, so every hit in a PE file is attributed through the PDB: the section contribution
that contains the address names the object file and library it came from. Hits inside
Microsoft runtime libraries are reported with their function; any other origin fails.
"""

import argparse
import bisect
import re
import subprocess
import sys

X86_FMA = re.compile(r"^vfn?m(add|sub)(sub|add)?\d+(ps|ss|pd|sd)$")
ARM_SCALAR_FMA = re.compile(r"^fn?m(add|sub)$")
ARM_VECTOR_FMA = re.compile(r"^fml[as]$")
INSTRUCTION = re.compile(r"^\s*([0-9a-f]+):\s+(\S+)\s*(.*)$")
DIRECT_BRANCH = re.compile(r"^(callq?|jmpq?|bl|b)$")
BRANCH_TARGET = re.compile(r"^(?:0x)?([0-9a-f]+)\b")
RUNTIME_LIBRARY = re.compile(r"(?i)(^|[\\/])(libcmt|libvcruntime|libucrt|libcpmt|libconcrt|msvcrt|oldnames)[^\\/]*\.lib$")


def run(*args):
    return subprocess.run(args, check=True, capture_output=True, text=True).stdout


def classify(mnemonic, operands):
    match = X86_FMA.match(mnemonic)
    if match:
        return {"ps": "single vector", "ss": "single scalar", "pd": "double vector", "sd": "double scalar"}[match.group(3)]
    if ARM_VECTOR_FMA.match(mnemonic):
        return "vector"
    if ARM_SCALAR_FMA.match(mnemonic):
        register = operands.strip()[:1]
        return {"s": "single scalar", "d": "double scalar", "h": "half scalar"}.get(register, "scalar")
    return None


def disassemble(library):
    hits = []
    callers = {}
    for line in run("llvm-objdump", "-d", "--no-show-raw-insn", library).splitlines():
        match = INSTRUCTION.match(line)
        if not match:
            continue
        address = int(match.group(1), 16)
        mnemonic, operands = match.group(2), match.group(3)
        kind = classify(mnemonic, operands)
        if kind:
            hits.append((address, mnemonic, kind))
        elif DIRECT_BRANCH.match(mnemonic):
            target = BRANCH_TARGET.match(operands.strip())
            if target:
                callers.setdefault(int(target.group(1), 16), []).append(address)
    return hits, callers


def section_addresses(library):
    addresses = {}
    for line in run("llvm-objdump", "-h", library).splitlines():
        fields = line.split()
        if len(fields) >= 4 and fields[0].isdigit():
            addresses[int(fields[0]) + 1] = int(fields[3], 16)
    return addresses


def load_pdb(pdb, sections):
    modules = {}
    current = None
    for line in run("llvm-pdbutil", "dump", "--modules", pdb).splitlines():
        match = re.match(r"^\s*Mod (\d+) \| `(.*)`:", line)
        if match:
            current = int(match.group(1))
            modules[current] = {"name": match.group(2), "obj": ""}
            continue
        match = re.match(r"^\s*Obj: `(.*)`:", line)
        if match and current is not None:
            modules[current]["obj"] = match.group(1)

    contributions = []
    pattern = re.compile(r"mod = (\d+), (\d+):(\d+), size = (\d+)")
    for line in run("llvm-pdbutil", "dump", "--section-contribs", pdb).splitlines():
        match = pattern.search(line)
        if match:
            section = int(match.group(2))
            if section in sections:
                start = sections[section] + int(match.group(3))
                contributions.append((start, start + int(match.group(4)), int(match.group(1))))
    contributions.sort()

    functions = []
    name = None
    for line in run("llvm-pdbutil", "dump", "--publics", pdb).splitlines():
        match = re.search(r"S_PUB32 \[size = \d+\] `(.*)`", line)
        if match:
            name = match.group(1)
            continue
        match = re.search(r"addr = (\d+):(\d+)", line)
        if match and name is not None:
            section = int(match.group(1))
            if section in sections:
                functions.append((sections[section] + int(match.group(2)), name))
            name = None
    functions.sort()

    if not modules or not contributions:
        for option in ("--modules", "--section-contribs", "--publics"):
            print(f"--- llvm-pdbutil dump {option} (first lines)")
            print("\n".join(run("llvm-pdbutil", "dump", option, pdb).splitlines()[:40]))
        raise SystemExit(f"{pdb}: no module or section contribution data")
    return modules, contributions, functions


def function_start(address, functions):
    index = bisect.bisect_right(functions, (address, "\uffff")) - 1
    return functions[index] if index >= 0 else (None, "?")


def is_runtime(module, obj):
    return obj is not None and (RUNTIME_LIBRARY.search(obj) is not None or module == "* Linker *")


def outside_callers(address, callers, modules, contributions, functions, depth=6):
    """Follow direct calls and jumps backwards from the function containing address until
    code outside the Microsoft runtime is reached. Returns each outside caller with the chain
    of runtime functions leading to the reported function. Indirect calls are not visible."""
    start, name = function_start(address, functions)
    chains = {start: [name]}
    frontier = [start]
    found = {}
    for _ in range(depth):
        next_frontier = []
        for target in frontier:
            for site in callers.get(target, []):
                module, obj, function = attribute(site, modules, contributions, functions)
                if is_runtime(module, obj):
                    caller_start, caller_name = function_start(site, functions)
                    if caller_start not in chains:
                        chains[caller_start] = [caller_name] + chains[target]
                        next_frontier.append(caller_start)
                else:
                    found.setdefault(f"{function} [{module.split(chr(92))[-1]}]", " <- ".join(reversed(chains[target])))
        frontier = next_frontier
    return found


def attribute(address, modules, contributions, functions):
    index = bisect.bisect_right(contributions, (address, float("inf"), 0)) - 1
    if index < 0 or not contributions[index][0] <= address < contributions[index][1]:
        return None, None, "?"
    module = modules.get(contributions[index][2], {"name": "?", "obj": "?"})
    function_index = bisect.bisect_right(functions, (address, "￿")) - 1
    function = functions[function_index][1] if function_index >= 0 and functions[function_index][0] >= contributions[index][0] else "?"
    return module["name"], module["obj"], function


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("library")
    parser.add_argument("--pdb")
    args = parser.parse_args()

    hits, callers = disassemble(args.library)
    if not args.library.lower().endswith(".dll"):
        print(f"{args.library}: {len(hits)} fused multiply-add instructions")
        for address, mnemonic, kind in hits[:20]:
            print(f"  FAIL {address:#x} {mnemonic} ({kind})")
        return 1 if hits else 0

    if not args.pdb:
        raise SystemExit(f"{args.library}: a PDB is required to attribute Windows hits")

    modules, contributions, functions = load_pdb(args.pdb, section_addresses(args.library))
    groups = {}
    failed = False
    for address, mnemonic, kind in hits:
        module, obj, function = attribute(address, modules, contributions, functions)
        runtime = is_runtime(module, obj)
        failed |= not runtime
        key = ("runtime" if runtime else "FAIL", obj or "unattributed", module or "?", function, kind)
        groups.setdefault(key, []).append(address)

    print(f"{args.library}: {len(hits)} fused multiply-add instructions")
    for (verdict, obj, module, function, kind), addresses in sorted(groups.items()):
        print(f"  {verdict} {len(addresses)} {kind} in {function} [{module} from {obj}] first {addresses[0]:#x}")
        if verdict == "runtime":
            reached = outside_callers(addresses[0], callers, modules, contributions, functions)
            print(f"    callers outside the runtime: {len(reached)}")
            for caller, chain in sorted(reached.items())[:12]:
                print(f"      {caller} -> {chain}")
            if len(reached) > 12:
                print(f"      ... {len(reached) - 12} more")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
