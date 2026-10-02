# JoltPhysicsUnity

English | [한국어](README.ko.md)

A Unity UPM port of [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp). It uses `UnityEngine.Vector3` and `UnityEngine.Quaternion` and lets C# data code create and step a physics world without scene objects.

**The Unity port in this project was implemented by OpenAI GPT (Codex).** The original physics engine and wrapper retain their upstream attribution and copyright notices.

The current version is **`0.1.0-preview.1`**. This is a development preview with core world and body functionality; it does not yet provide the full upstream API.

## Installation

Git must be installed on your computer. If your project already uses the NuGet `JoltPhysicsSharp` package, remove it first. Both packages use the same namespace and type names.

In Unity Package Manager, select **＋ → Install package from git URL** and enter:

```text
https://github.com/ciart/JoltPhysicsUnity.git
```

Append `#<commit SHA>` to pin a specific revision. For local development, use **Install package from disk** and select `package.json` at the repository root. [Unity installation guide](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-ui-giturl.html)

## Sample

Import the **Data World** sample in Package Manager, then call the following code inside a C# method:

```csharp
var result = JoltPhysicsSharp.Samples.BoxSimulation.Run(steps: 120);
UnityEngine.Debug.Log($"Box position: {result.Position}, Compound position: {result.CompoundPosition}");
```

The sample creates a floor, a dynamic Box, and a dynamic Compound, then runs 120 simulation steps at 60 Hz. It handles initialization and resource cleanup and creates no scene GameObjects. The result contains positions, rotations, and the Compound's local center of mass.

See the [sample source](Samples~/DataWorld/BoxSimulation.cs) to create your own world, and the [usage guide (Korean)](Documentation~/Usage.md) for initialization, ownership, and fixed-step contracts. Your game handles prefab creation and applies the resulting poses.

## Implemented features

Currently available:

- Runtime initialization and shutdown, native resource disposal, and lifetime management
- Table-based collision layers, physics worlds, gravity, and fixed-step simulation
- Box and StaticCompound shapes; body creation, addition, removal, and destruction
- Body pose and center-of-mass queries, velocity control, and kinematic movement

RayCast, CollideShape, CastShape, contact and activation events, mass configuration, and matrix APIs are not implemented yet. See the [implementation requirements (Korean)](Documentation~/Implementation.md) for detailed status.

## Validation and platforms

Development targets Unity **6000.4.8f1**, .NET Standard 2.1, and C# 9. The included native library uses single precision and is a universal macOS arm64/x86_64 binary.

| Environment | Current status |
| --- | --- |
| macOS arm64 Editor | Local UPM import, 120-step Box/Compound simulation, pose queries, and resource cleanup verified |
| macOS Intel Editor and player | Native binary included; execution not verified |
| macOS arm64 player | Native binary included; execution not verified |
| Windows, Linux, mobile, WebGL | Platform binaries not included |
| IL2CPP | Not verified |

The [sample execution results](Documentation~/DataWorldResult.json) are preserved. Cross-platform determinism and performance have not been verified.

## Upstream sources and changes

| Component | Upstream baseline |
| --- | --- |
| C# wrapper | [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp) 2.22.0 |
| Native C API | [joltc](https://github.com/amerkoleci/joltc) |
| Physics engine | [Jolt Physics](https://github.com/jrouwe/JoltPhysics) 5.6.0 |

The native plugin was built directly from pinned joltc and Jolt sources. Exact commits, build options, and SHA-256 hashes are recorded in [upstream.lock.json](upstream.lock.json). Reasons for changes from upstream and validation results are documented in the [porting log (Korean)](Documentation~/UpstreamChanges.md). Original managed code and C API source archives are kept in `Upstream~`.

- [Changelog (Korean)](CHANGELOG.md)
- [Native build instructions (Korean)](Documentation~/Usage.md#네이티브-빌드-재현)
- [Development guidelines (Korean)](AGENTS.md)

## License

MIT. Full upstream notices are preserved in [LICENSE.md](LICENSE.md), the [joltc license](Native~/Licenses/Joltc.txt), and the [Jolt Physics license](Native~/Licenses/JoltPhysics.txt). Include these notices in distributions that contain this package. [Third-party notices (Korean)](Third%20Party%20Notices.md)
