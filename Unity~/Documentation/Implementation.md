# Unity port requirements

The goal is an independent UPM package, `JoltPhysicsUnity`, providing Unity math types by modifying the original JoltPhysicsSharp files. The baseline is `3ab66bf9e970b10c2b22b8f5b88dfdc2c765a7fb` (managed API 2.22.0). Preserve its Git history. Execution results from the previous standalone repository do not establish completion in this fork.

| ID | Implementation and completion criteria | Current status in this fork |
| --- | --- | --- |
| SRC-01 | Preserve fork history, baseline and source paths; separate commits and upstream change records | 486 upstream commits preserved; baseline and working rules recorded |
| LANG-01 | Compile actual sources against C# 9 / .NET Standard 2.1; replace implicit usings, primary constructors and file-scoped namespaces | 48 foundational, enum, geometry and result files compile in Unity and .NET paths; full assembly incomplete |
| MATH-01 | Use Unity Vector3, Quaternion, Vector4 and Matrix4x4; review exact equality, hashing, rotation defaults and column layout | Unity types applied to Triangle, BoundingBox, Ray, MathUtil, query results and ContactSettings; Ray uses MultiplyPoint3x4. Native rotation/matrix paths incomplete |
| ABI-01 | Replace LibraryImport with Unity-compatible calls; compare signatures, bool widths, layout and calling conventions with the pinned header | 22 type sizes and 29 offsets in three structures match; P/Invoke and callbacks pending |
| INIT-01 | Define initialization, shutdown, multiple worlds and shutdown with live objects | Not ported |
| WORLD-01 | Provide world settings, gravity, JobSystem and caller-driven fixed steps without GameObjects | Not ported |
| SHAPE-01 | Modify original Box and StaticCompound files; preserve half extents, child transforms and shared shape lifetime | Not ported |
| BODY-01 | Create, add, remove, destroy, activate, read/write pose and velocity, and move kinematic bodies; retain ID generation bits | Not ported |
| QUERY-01 | RayCast, CollideShape, CastShape and layer/body filters; preserve collector return types and result lifetime | Not ported |
| CONTACT-01 | Added/Persisted/Removed and activation events; handle worker threads, exceptions and pointer lifetime | Not ported |
| LIFE-01 | IDisposable, ownership, GCHandles, callback rooting, concurrent stepping/shutdown, duplicate release and disposed access | Not ported |
| NATIVE-01 | Record single-precision native source, options, hashes and platform PluginImporter settings | No artifacts added |
| PKG-01 | UPM package at the original src path, runtime assembly, licenses and standalone example; verify Unity import | Manifest 0.1.0-preview.1, package README and MIT license added at src/JoltPhysicsSharp; runtime assembly, example and Unity import pending |
| AOT-01 | Static callbacks, MonoPInvokeCallback, delegate rooting, stripping support and IL2CPP execution | Not ported |
| PERF-01 | Measure allocations and call costs during repeated stepping and pose queries | Not measured |

The first execution target is Unity 6000.4.8f1 on macOS arm64 Editor, single precision. Update Windows and IL2CPP support only after separate execution checks. Distinguish the complete source/API port from the required subset actually exercised.

Positions and directions use Unity Vector3; rotations use Quaternion. Preserve exact equality and hash contracts rather than Unity's approximate equality operators. Convert matrices according to native columns and translation placement. Do not mix double precision into the first execution milestone.

RayCast and CastShape displacement vectors include length; fractions retain their upstream meaning. Keep float-returning collectors separate from void callbacks. Deferred contact events contain copied values rather than native pointers. Do not invent positions or normals absent from Removed callbacks. Validation and filters requiring immediate returns cannot be deferred into queues.

GameObjects, MonoBehaviours, prefab mapping and network ticks are outside the package. Callers control stepping and scene synchronization. Burst/Unity Jobs integration and cross-platform determinism are outside the first completion criteria.

The current fork is [ciart/JoltPhysicsUnity](https://github.com/ciart/JoltPhysicsUnity). Its UPM package directory is `src/JoltPhysicsSharp`, with package ID `com.ciart.joltphysics` and initial development version `0.1.0-preview.1`. The README documents its Git dependency URL and Package Manager workflow. Metadata availability is distinct from a working runtime: the complete Unity assembly, native interop and Unity import are still pending. Do not describe the development package as a validated runtime release or increment its version for implementation steps.

## Current compilation checks

`Unity~/Tools/compile-scope.py` compiles actual files listed in `compile-scopes.json` through two paths. It does not validate the full library or native execution. Supply the Unity Editor installation and dotnet executable paths.

```sh
python3 'Unity~/Tools/compile-scope.py' --unity-editor '<Unity Editor path>/Unity.app/Contents' --dotnet '<dotnet executable>' --scope foundations
```

Use `--scope geometry` to include geometry and result types (48 original files). [GeometryLayout.json](GeometryLayout.json) records the current layout comparison: C++ sizeof/offsetof from the pinned header versus Marshal.SizeOf/OffsetOf on actual compiled outputs hosted in .NET 9. This is distinct from Unity Editor or native physics execution.
