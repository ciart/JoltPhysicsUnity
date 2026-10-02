#!/usr/bin/env python3
"""고정한 원본 소스로 macOS 단정밀도 라이브러리를 빌드한다. 원본을 자동 다운로드하지 않는다."""

import argparse
import json
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--joltc-source", type=Path, required=True)
    parser.add_argument("--jolt-source", type=Path, required=True)
    parser.add_argument("--build-dir", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--jobs", type=int, default=6)
    args = parser.parse_args()
    package = Path(__file__).resolve().parents[1]
    lock = json.loads((package / "upstream.lock.json").read_text())
    for source, commit in [(args.joltc_source, lock["native"]["sourceCommit"]),
                           (args.jolt_source, lock["native"]["engine"]["commit"])]:
        actual = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=source, text=True).strip()
        if actual != commit:
            raise SystemExit(f"원본 커밋 불일치: {source}: {actual}")
        subprocess.run(["git", "diff", "--exit-code", "HEAD", "--"], cwd=source, check=True)
    if args.jobs <= 0:
        raise SystemExit("--jobs는 양수여야 합니다.")
    options = lock["native"]["build"]["cmakeOptions"]
    command = ["cmake", "-S", str(args.joltc_source.resolve()), "-B", str(args.build_dir.resolve()),
               "-G", "Ninja", "-DJOLT_PHYSICS_ROOT=" + str(args.jolt_source.resolve())]
    command.extend("-D" + key + "=" + str(value) for key, value in options.items())
    subprocess.run(command, check=True)
    subprocess.run(["cmake", "--build", str(args.build_dir.resolve()), "--parallel", str(args.jobs)], check=True)
    subprocess.run(["clang", "-std=c11", "-fsyntax-only", "-I" + str(args.joltc_source.resolve() / "include"),
                    str(package / "Native~/abi_layout.c")], check=True)
    args.output_dir.mkdir(parents=True, exist_ok=True)
    target = args.output_dir / "libjoltc.dylib"
    shutil.copy2(args.build_dir / "lib/libjoltc.dylib", target)
    print(target)


if __name__ == "__main__":
    main()
