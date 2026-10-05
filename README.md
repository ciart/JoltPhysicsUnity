# JoltPhysicsUnity

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A Unity port of [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp), maintained at [ciart/JoltPhysicsUnity](https://github.com/ciart/JoltPhysicsUnity). This fork preserves upstream Git history and modifies the original files under `src/JoltPhysicsSharp`.

The intended runtime uses Unity math types and lets plain C# code own and step physics data. GameObjects, prefab creation and Transform synchronization remain the responsibility of the game. The C# namespace stays **JoltPhysicsSharp**; the UPM package ID is **com.ciart.joltphysics**.

**Development package: limited box runtime verified on macOS arm64 Editor.** Current preview: `0.1.0-preview.2`. The original files now provide initialization, box shapes, basic body creation/pose/velocity access, fixed stepping and closest-hit raycasts. The complete JoltPhysicsSharp API is still being ported; unsupported Unity sources are excluded from compilation.

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

To install this preview explicitly, use its version tag:

```text
https://github.com/ciart/JoltPhysicsUnity.git#v0.1.0-preview.2
```

A full commit hash can also be used after `#` to pin a development revision. No `?path=` parameter is needed. This preview provides the limited API described below; the complete Unity runtime port remains unfinished.

Refer to Unity's [Git installation instructions](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-ui-giturl.html) and [Git revisions](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-git.html#revision) for the URL format.

## Current implementation

| Area | Verified in this fork |
| --- | --- |
| UPM metadata | Root manifest, runtime asmdef, Unity-generated metadata and six OS/CPU-specific PluginImporter configurations; embedded import verified |
| Source compatibility | 66 original files compile in the Unity runtime scope; 111 unported files are excluded from Unity, preserving their .NET source |
| Math types | Unity geometry, box/body vectors and quaternions; initial position and yaw round trips pass; advanced native matrices pending |
| Structure layout | 22 type sizes and 29 field offsets match the pinned C header in a macOS arm64 inspection hosted on .NET 9 |
| Unity runtime assembly | Enabled box-runtime assembly and embedded package import verified; full API port incomplete |
| Native execution | Actual Editor box/floor simulation, closest-hit raycast and 5 repeated cleanup cycles pass; player builds remain unverified |

Native checks ran against this fork and its unchanged upstream macOS binary. Results from the previous standalone repository are not used as validation. Compound shapes, contact callbacks, custom query filters, complete body APIs, Windows, IL2CPP and performance remain outside the verified scope.

See the [implementation requirements](Unity~/Documentation/Implementation.md), [changes from upstream](Unity~/Documentation/UpstreamChanges.md), and [pinned source baseline](Unity~/upstream.json) for scope and remaining work. The manifest starts at `0.1.0-preview.1`; its version will not be incremented for each implementation step.

## Native platforms

| Target | Native package configuration | Execution verified |
| --- | --- | --- |
| macOS arm64 / x64 | Universal single-precision library; Editor and player enabled | arm64 Editor only |
| Windows x64 / ARM64 | Separate single-precision DLLs; matching Editor OS/CPU and player CPU enabled | No |
| Linux x64 | Single-precision library; Linux Editor and player enabled | No |
| Android ARM64 / x64 | Separate single-precision libraries; Android CPU selected; 16 KiB ELF load-segment alignment inspected | No |
| Android ARMv7 | Upstream library retained and disabled; 32-bit ABI/runtime verification pending | No |
| Linux ARM64 | Upstream library retained and disabled; separate Embedded Linux target configuration required | No |
| Windows / Linux x86 | No upstream native artifact | No |
| iOS / tvOS / visionOS | Native artifacts and static-link interop not prepared | No |
| WebGL / WebGPU | WebAssembly artifact and browser interop not prepared | No |
| UWP / consoles | Target-specific artifacts and integration not prepared | No |

All eight upstream release artifacts retain their original bytes and export the 71 entry points used by the Unity runtime. Debug and double-precision libraries remain disabled. Enabling an importer does not establish target execution, IL2CPP support or CPU instruction requirements. See the [native audit](Unity~/Documentation/NativePlatforms.json) for hashes, source commits, dependencies and remaining gaps, and Unity's [plug-in settings](https://docs.unity3d.com/6000.4/Documentation/Manual/plug-in-inspector.html) for OS/CPU selection.

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

The example uses the macOS Editor layout. Supply your own Editor path. The geometry scope compiles 48 files through Unity and non-Unity paths. Use `--scope runtime --variant unity` for the 66-file box runtime. The compiler does not install packages or run physics. Unsupported original files are guarded out in Unity until ported.

For import metadata, use Unity on a writable embedded package, not its immutable Git cache. See the [metadata generation workflow](Unity~/Documentation/Implementation.md#box-runtime-verification-2026-10-04). Unity creates the GUIDs, and [GenerateImportMetadata.cs](Unity~/Tools/GenerateImportMetadata.cs) configures the native plugin. Keep all generated source and plugin meta files in version control.

## Credits and license

The Unity port is written and modified using **GPT (Codex)**. This attribution applies to the Unity adaptation, not to the original engine or bindings.

- [Jolt Physics](https://github.com/jrouwe/JoltPhysics): Jorrit Rouwe and contributors.
- [joltc](https://github.com/amerkoleci/joltc) and [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp): Amer Koleci and contributors.

The original MIT license and copyright notices are preserved. See [LICENSE](LICENSE). For the original .NET/NuGet project and installation instructions, refer to the [upstream README](https://github.com/amerkoleci/JoltPhysicsSharp#readme). You can support the original bindings through [Amer Koleci's sponsorship page](https://github.com/sponsors/amerkoleci).
