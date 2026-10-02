# JoltPhysicsUnity

A Unity port of [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp), developed directly in an upstream GitHub fork. The original Git history and `src/JoltPhysicsSharp` paths are retained; changes are made to the original source files. The public name is **JoltPhysicsUnity**, the C# namespace remains **JoltPhysicsSharp**, and the planned UPM ID is **com.ciart.joltphysics**.

The Unity port is written and modified using **GPT (Codex)**. The original Jolt Physics engine is by Jorrit Rouwe and contributors; joltc and JoltPhysicsSharp are by Amer Koleci and contributors. Their MIT licenses and copyright notices are retained.

**Porting is in progress. This fork is not yet an installable Unity Package Manager release.** The target is Unity 6000.4.8f1, C# 9, .NET Standard 2.1, and UnityEngine Vector3, Quaternion and Matrix4x4 APIs. The first execution milestone is a single-precision world with Box and Compound bodies without GameObjects. UPM import, native execution, Windows and IL2CPP have not yet been validated in this fork.

See the [implementation requirements](Unity~/Documentation/Implementation.md), [upstream change log](Unity~/Documentation/UpstreamChanges.md), and [pinned baseline](Unity~/upstream.json). Validation from the previous standalone repository does not count as validation of this fork. The release version and installation URL will be published when the package is ready.

Currently, 48 original source files covering foundational types, enums, geometry and query results compile against C# 9 and .NET Standard 2.1 in both Unity and .NET paths. Selected native structure sizes and field offsets match the pinned C header. Native calls, world execution and the complete assembly are still being ported; these checks do not establish an installable package.

## Upstream .NET project

The following information describes the original .NET project and its NuGet packages.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/amerkoleci/JoltPhysicsSharp/blob/main/LICENSE)
[![Build status](https://github.com/amerkoleci/JoltPhysicsSharp/workflows/Build/badge.svg)](https://github.com/amerkoleci/JoltPhysicsSharp/actions)
[![NuGet](https://img.shields.io/nuget/v/JoltPhysicsSharp.svg)](https://www.nuget.org/packages/JoltPhysicsSharp)

Cross platform modern **.net9.0** and **.net10.0** bindings for [JoltPhysics](https://github.com/jrouwe/JoltPhysics) using [joltc](https://github.com/amerkoleci/joltc).

# Installation - [Nuget](https://www.nuget.org/packages/JoltPhysicsSharp)
```
dotnet add package JoltPhysicsSharp --version [VERSION]
```

If you want to debug the native libraries add the debug dependency
```
dotnet add package JoltPhysics.Native.Debug --version 1.0.1
```

## Sponsors
Please consider [SPONSOR](https://github.com/sponsors/amerkoleci) me to further help development and to allow faster issue triaging and new features to be implemented.
**_NOTE:_** **any feature request** would require a [sponsor](https://github.com/sponsors/amerkoleci) in order to allow faster implementation and allow this project to continue.
