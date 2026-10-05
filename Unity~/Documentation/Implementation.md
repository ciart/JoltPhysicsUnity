# Unity port requirements

The goal is an independent UPM package, `JoltPhysicsUnity`, providing Unity math types by modifying the original JoltPhysicsSharp files. The baseline is `3ab66bf9e970b10c2b22b8f5b88dfdc2c765a7fb` (managed API 2.22.0). Preserve its Git history. Execution results from the previous standalone repository do not establish completion in this fork.

| ID | Implementation and completion criteria | Current status in this fork |
| --- | --- | --- |
| SRC-01 | Preserve fork history, baseline and source paths; separate commits and upstream change records | 486 upstream commits preserved; baseline and working rules recorded |
| LANG-01 | Compile actual sources against C# 9 / .NET Standard 2.1; replace implicit usings, primary constructors and file-scoped namespaces | 66 original files compile in the Unity runtime scope; the 48-file geometry scope retains its .NET check. Other 111 files are excluded from Unity pending their port |
| MATH-01 | Use Unity Vector3, Quaternion, Vector4 and Matrix4x4; review exact equality, hashing, rotation defaults and column layout | Unity vectors/quaternions used by geometry, box creation, bodies and queries; initial yaw/position round trips pass. Advanced matrix paths pending |
| ABI-01 | Replace LibraryImport with Unity-compatible calls; compare signatures, bool widths, layout and calling conventions with the pinned header | Editor box-runtime P/Invoke uses Cdecl DllImport and byte bool returns; native settings size 48 / first pointer offset 24 and job config size 12 checked in Unity. Advanced interop pending |
| INIT-01 | Define initialization, shutdown, multiple worlds and shutdown with live objects | Reference-counted initialization and live-resource shutdown guard implemented; 6 initialization/shutdown cycles pass. Concurrent multiworld behavior not verified |
| WORLD-01 | Provide world settings, gravity, JobSystem and caller-driven fixed steps without GameObjects | Box-world settings, gravity, thread pool and fixed steps pass; each world uses an explicit malloc temp allocator |
| SHAPE-01 | Modify original Box and StaticCompound files; preserve half extents, child transforms and shared shape lifetime | Original Box/Shape/ConvexShape files adapted; box half extents and disposal exercised. StaticCompound pending |
| BODY-01 | Create, add, remove, destroy, activate, read/write pose and velocity, and move kinematic bodies; retain ID generation bits | Create/add/remove/destroy and pose/velocity reads exercised; ID generation constants retained. Complete body API and kinematic movement pending |
| QUERY-01 | RayCast, CollideShape, CastShape and layer/body filters; preserve collector return types and result lifetime | Closest-hit unfiltered RayCast ID/fraction pass. Unity exposes the two-argument overload; custom filters/collectors and other queries pending |
| CONTACT-01 | Added/Persisted/Removed and activation events; handle worker threads, exceptions and pointer lifetime | Not ported |
| LIFE-01 | IDisposable, ownership, GCHandles, callback rooting, concurrent stepping/shutdown, duplicate release and disposed access | Filter ownership transfers to the world and wrappers are invalidated after native deletion. Bodies are tracked for world cleanup. Repeated box cycles pass; advanced callbacks/concurrency and leak profiling pending |
| NATIVE-01 | Record single-precision native source, options, hashes and platform PluginImporter settings | Eight upstream release artifacts audited; six macOS/Windows/Linux/Android 64-bit OS/CPU configurations saved and checked through Unity APIs. Only macOS arm64 Editor execution verified; other targets pending. No binary rebuilt; original build options not reconstructed |
| PKG-01 | UPM manifest at the repository root, original managed source paths preserved, runtime assembly, licenses and standalone example; verify Unity import | Root manifest/version unchanged; runtime asmdef and 263 Unity-generated meta files added. Upstream .NET samples/tests disabled for Unity. Embedded import and the temporary validation scene pass in an isolated Editor project; immutable Git import pending |
| AOT-01 | Static callbacks, MonoPInvokeCallback, delegate rooting, stripping support and IL2CPP execution | Foundation trace/assert delegates rooted with static MonoPInvokeCallback methods; IL2CPP and stripping remain unverified |
| PERF-01 | Measure allocations and call costs during repeated stepping and pose queries | Not measured |

The first execution target is Unity 6000.4.8f1 on macOS arm64 Editor, single precision. Update Windows and IL2CPP support only after separate execution checks. Distinguish the complete source/API port from the required subset actually exercised.

Positions and directions use Unity Vector3; rotations use Quaternion. Preserve exact equality and hash contracts rather than Unity's approximate equality operators. Convert matrices according to native columns and translation placement. Do not mix double precision into the first execution milestone.

RayCast and CastShape displacement vectors include length; fractions retain their upstream meaning. Keep float-returning collectors separate from void callbacks. Deferred contact events contain copied values rather than native pointers. Do not invent positions or normals absent from Removed callbacks. Validation and filters requiring immediate returns cannot be deferred into queues.

GameObjects, MonoBehaviours, prefab mapping and network ticks are outside the package. Callers control stepping and scene synchronization. Burst/Unity Jobs integration and cross-platform determinism are outside the first completion criteria.

The current fork is [ciart/JoltPhysicsUnity](https://github.com/ciart/JoltPhysicsUnity). Its UPM package root is the repository root, with package ID `com.ciart.joltphysics` and initial development version `0.1.0-preview.1`. The Git dependency URL is `https://github.com/ciart/JoltPhysicsUnity.git`, without a path query. Managed sources remain in `src/JoltPhysicsSharp`. The active box-runtime Unity assembly and embedded package import are verified. This is a limited API surface, not the complete 177-file port. Unported sources are guarded out in Unity while preserving the original .NET path; .NET sample/test asmdefs are disabled in Unity. Single-precision macOS, Windows x64/ARM64, Linux x64 and Android ARM64/x64 plugins have OS/CPU-specific importer settings. Native execution is verified only on macOS arm64 Editor. Do not describe the development package as a validated runtime release or increment its version for implementation steps.

## Current compilation checks

`Unity~/Tools/compile-scope.py` compiles actual files listed in `compile-scopes.json` through two paths. It does not run native physics. The runtime scope builds only the currently enabled Unity API surface. Supply the Unity Editor installation and dotnet executable paths.

```sh
python3 'Unity~/Tools/compile-scope.py' --unity-editor '<Unity Editor path>/Unity.app/Contents' --dotnet '<dotnet executable>' --scope foundations
```

Use `--scope geometry` to include geometry and result types (48 original files). [GeometryLayout.json](GeometryLayout.json) records the current layout comparison: C++ sizeof/offsetof from the pinned header versus Marshal.SizeOf/OffsetOf on actual compiled outputs hosted in .NET 9. This is distinct from Unity Editor or native physics execution.

## Box-runtime verification (2026-10-04)

Unity 6000.4.8f1, macOS arm64 Editor, C# 9, .NET Standard 2.1, single precision: the 66-original-file runtime scope and enabled Unity assembly compile. A real Play mode scene, using the unchanged upstream native binary, passed 300 1/60-second steps, initial position and 35-degree yaw round trips, finite normalized poses, floor collision/rest height, and closest-hit raycast ID/fraction checks. Five additional complete initialization, world creation, simulation, removal, disposal and shutdown cycles passed. Scene visuals contain no Unity Rigidbody or Collider. The temporary host scene is outside this package.

The original native binary is universal, but only macOS arm64 Editor execution is verified. Player builds, Intel Editor, Windows, compound shapes, contact events, custom filters, advanced body APIs, IL2CPP, determinism and performance remain unverified or unported.

```sh
python3 'Unity~/Tools/compile-scope.py' --unity-editor '<Unity Editor root>' --dotnet dotnet --scope runtime --variant unity
```

`GenerateImportMetadata.cs` must run from Assets/Editor in a separate project with this writable checkout embedded under Packages/com.ciart.joltphysics. Unity generates source GUIDs; PluginImporter APIs write platform settings. Merely adding a file dependency did not generate metadata in this environment. Do not hand-write GUIDs or treat a completed package request as proof that metadata exists.

## Native platform audit (2026-10-05)

[NativePlatforms.json](NativePlatforms.json) records all eight upstream single-precision release artifacts. Inspect PE/ELF headers, exported symbols, dynamic dependencies and ELF load-segment alignment; inspect both Mach-O architectures. All artifacts retain the baseline bytes and provide all 71 entry points declared by the current Unity runtime. Android libraries have 16 KiB load-segment alignment; this alone does not validate an Android build or device.

Unity 6000.4.8f1 saves and reads back six configurations: universal macOS; Windows x64 and ARM64; Linux x64; Android ARM64 and x64. Editor settings select an explicit OS and CPU; player settings select a target and matching CPU. Android and Linux libraries sharing the same filename are kept on distinct targets. All native GUIDs remain unchanged; debug and double-precision artifacts remain disabled.

Android ARMv7 remains disabled pending 32-bit ABI/runtime verification. Linux ARM64 remains disabled pending its separate Embedded Linux configuration. Windows/Linux x86, Apple mobile, Web, UWP and consoles lack the required target-specific artifacts or interop integration. No target execution, IL2CPP build, binary rebuild or performance test was performed in this audit. The host project and its package dependency were not changed.
