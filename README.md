# JoltPhysicsUnity

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A Unity port of [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp), maintained at [ciart/JoltPhysicsUnity](https://github.com/ciart/JoltPhysicsUnity). This fork preserves upstream Git history and modifies the original files under `src/JoltPhysicsSharp`.

The intended runtime uses Unity math types and lets plain C# code own and step physics data. GameObjects, prefab creation and Transform synchronization remain the responsibility of the game. The C# namespace stays **JoltPhysicsSharp**; the UPM package ID is **com.ciart.joltphysics**.

**Development package: runtime port incomplete.** A UPM manifest is available at the repository root as `package.json`, version `0.1.0-preview.1`. The complete Unity assembly, native interop and Unity import are not yet verified. Adding the Git dependency does not yet provide a working physics runtime.

## Adding to Unity

The target is **Unity 6000.4.8f1**, **C# 9**, **.NET Standard 2.1**, and UnityEngine `Vector3`, `Quaternion` and `Matrix4x4` APIs. The first native execution target is single precision on macOS arm64 Editor. Windows and IL2CPP are not yet verified.

1. Install Git and ensure Unity can find its executable through `PATH`.
2. Open **Window > Package Management > Package Manager**.
3. Select **+ > Install package from git URL**.
4. Enter the development package URL below, then select **Install**.

The main-branch package URL is:

```text
https://github.com/ciart/JoltPhysicsUnity.git
```

To pin a specific development revision, append its full commit hash to the repository URL. A future published Unity release can also be selected by tag:

```text
https://github.com/ciart/JoltPhysicsUnity.git#<tag-or-full-commit-hash>
```

Replace `<tag-or-full-commit-hash>` with an existing revision that contains the root package manifest. No `?path=` parameter is needed. No complete Unity runtime release is available yet.

Refer to Unity's [Git installation instructions](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-ui-giturl.html) and [Git revisions](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-git.html#revision) for the URL format.

## Current implementation

| Area | Verified in this fork |
| --- | --- |
| UPM metadata | Manifest, README and MIT license at the repository root; JSON checked, Unity import not yet verified |
| Source compatibility | 48 original foundational, enum, geometry and query-result files compile against C# 9 / .NET Standard 2.1 in Unity and .NET paths |
| Math types | Unity types applied to the ported geometry and result types; full rotation and native matrix paths pending |
| Structure layout | 22 type sizes and 29 field offsets match the pinned C header in a macOS arm64 inspection hosted on .NET 9 |
| Complete Unity assembly and UPM import | Not verified |
| Native world/body execution and player builds | Not verified |

Compilation and layout inspection do not establish native execution or package usability. Results from the previous standalone repository are not treated as validation of this fork.

See the [implementation requirements](Unity~/Documentation/Implementation.md), [changes from upstream](Unity~/Documentation/UpstreamChanges.md), and [pinned source baseline](Unity~/upstream.json) for scope and remaining work. The manifest starts at `0.1.0-preview.1`; its version will not be incremented for each implementation step.

## Working on the port

Clone the repository outside your Unity project's `Assets` folder:

```sh
git clone https://github.com/ciart/JoltPhysicsUnity.git
cd JoltPhysicsUnity
```

To compile the currently ported source scope, install Python 3 and the .NET 9 runtime used for the recorded checks, then point the tool at a local Unity Editor installation:

```sh
python3 'Unity~/Tools/compile-scope.py' \
  --unity-editor '/path/to/Unity.app/Contents' \
  --dotnet dotnet \
  --scope geometry
```

The example uses the macOS Editor layout. Supply your own Editor path. The tool compiles the 48-file scope through Unity and non-Unity paths; it does not install a package or run physics. Do not copy the complete source folder into `Assets`: the unported files still require unsupported language features and runtime APIs.

## Credits and license

The Unity port is written and modified using **GPT (Codex)**. This attribution applies to the Unity adaptation, not to the original engine or bindings.

- [Jolt Physics](https://github.com/jrouwe/JoltPhysics): Jorrit Rouwe and contributors.
- [joltc](https://github.com/amerkoleci/joltc) and [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp): Amer Koleci and contributors.

The original MIT license and copyright notices are preserved. See [LICENSE](LICENSE). For the original .NET/NuGet project and installation instructions, refer to the [upstream README](https://github.com/amerkoleci/JoltPhysicsSharp#readme). You can support the original bindings through [Amer Koleci's sponsorship page](https://github.com/sponsors/amerkoleci).
