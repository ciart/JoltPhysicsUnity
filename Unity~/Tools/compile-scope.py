#!/usr/bin/env python3

import argparse
import json
from pathlib import Path
import shutil
import subprocess
import tempfile


def main():
    parser = argparse.ArgumentParser(description="Compile an original-source scope against Unity C# 9 references.")
    parser.add_argument("--unity-editor", required=True, type=Path,
                        help="Unity.app/Contents or the Windows/Linux Editor root")
    parser.add_argument("--dotnet", default="dotnet", help="dotnet executable")
    parser.add_argument("--scope", default="foundations")
    parser.add_argument("--output", type=Path,
                        help="output directory; defaults to a system temporary directory")
    args = parser.parse_args()

    repository = Path(__file__).resolve().parents[2]
    scopes = json.loads((repository / "Unity~/compile-scopes.json").read_text())
    if args.scope not in scopes:
        parser.error("Unknown compilation scope: " + args.scope)

    scripting = args.unity_editor / "Resources/Scripting"
    compiler = scripting / "DotNetSdkRoslyn/csc.dll"
    refs = sorted((scripting / "NetStandard/ref/2.1.0").glob("*.dll"))
    unity_core = scripting / "Managed/UnityEngine/UnityEngine.CoreModule.dll"
    if not compiler.is_file() or not refs or not unity_core.is_file():
        parser.error("Unity compiler, .NET Standard 2.1 references or CoreModule not found")
    dotnet = shutil.which(args.dotnet)
    if dotnet is None:
        parser.error("dotnet executable not found")

    output = args.output or Path(tempfile.mkdtemp(prefix="jolt-unity-compile-"))
    output = output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    sources = [repository / "src/JoltPhysicsSharp" / name for name in scopes[args.scope]]
    for source in sources:
        if not source.is_file():
            parser.error("Source file not found: " + str(source))

    for variant in ("netstandard", "unity"):
        options = ["-nologo", "-target:library", "-langversion:9.0",
                   "-nullable:enable", "-warnaserror+", "-nostdlib+", "-unsafe+",
                   "-out:" + str(output / (args.scope + "." + variant + ".dll"))]
        options.extend("-r:" + str(ref) for ref in refs)
        if variant == "unity":
            options.extend(["-define:UNITY_5_3_OR_NEWER", "-r:" + str(unity_core)])
        options.extend(str(source) for source in sources)
        response = output / (variant + ".rsp")
        response.write_text("\n".join('"' + option + '"' for option in options) + "\n")
        subprocess.run([dotnet, "--roll-forward", "Major", str(compiler),
                        "@" + str(response)], check=True)
        print(args.scope + ": " + variant + " compilation passed (" + str(len(sources)) + " sources)")
    print("Output: " + str(output))


if __name__ == "__main__":
    main()
