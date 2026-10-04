# JoltPhysicsUnity porting rules

This repository is an upstream GitHub fork of JoltPhysicsSharp. All repository content must be English, including documentation, source comments, tool messages, change records and commit messages. Do not add Korean text or a Korean README. Conversation with the user remains Korean. The user's latest instructions override earlier documentation preferences.

The README must disclose that GPT (Codex) is used to write and modify the Unity port. Keep that attribution separate from the authors of Jolt Physics, joltc and JoltPhysicsSharp, and preserve their MIT notices.

## Modify the original source

- Pin the baseline in `Unity~/upstream.json`. Preserve upstream Git history and the original `src/JoltPhysicsSharp` paths. Modify the original files directly; do not replace them with a subset copied from the previous standalone repository.
- Commit each logical change separately unless the user explicitly requests consolidation. Do not create orphan history or reset upstream history. Rewriting published port commits requires explicit user authorization and a push protected by an explicit expected remote commit.
- The public name is `JoltPhysicsUnity`, the UPM ID is `com.ciart.joltphysics`, and the C# namespace remains `JoltPhysicsSharp`. Update the repository URL only after verifying its actual rename.
- Keep package.json at the repository root so the Git installation URL needs no path parameter. Preserve the original managed source paths independently of the package root.
- Unity APIs use `UnityEngine.Vector3`, `Quaternion`, and where needed `Vector4` and `Matrix4x4`. Retain existing .NET paths through conditional compilation where practical. Review equality, normalization, defaults, matrix layout and rotation semantics explicitly.
- Keep game sessions, Playfield, Piece, prefabs and automatic Update outside the package. Callers control stepping and scene synchronization.
- Do not add comments that restate what the code already expresses. Record change rationale in the upstream change log. Preserve original copyright notices.

## Required change records

**Every managed, native, build or packaging change must update `Unity~/Documentation/UpstreamChanges.md` in the same commit.**

Record the change ID, date, original commit and files, modified files, reason, API/ABI/behavior impact, actual verification results and remaining limitations. Mark new files as new. Do not record plans as completed work. Update implementation status in `Unity~/Documentation/Implementation.md`.

Previous standalone-repository results do not validate this fork. Mark support as verified only for the sources, binaries and environment actually checked in this fork.

## Native boundaries and lifetime

- Compare C ABI, structure sizes, offsets, alignment, bool widths, calling conventions and float/void collector returns with the pinned header. Compilation alone does not establish ABI compatibility.
- The first execution milestone uses single precision. Record binary source commits, build options, architectures and SHA-256 hashes. Do not mix precision modes or binaries from different wrappers.
- IL2CPP callbacks require static methods, `AOT.MonoPInvokeCallback` and rooted delegates. Prevent exceptions from crossing the native boundary.
- Do not manipulate Unity objects in native worker-thread callbacks. Copy deferred contact data; do not retain pointers valid only during callbacks.
- Define ownership and destruction order for bodies, shapes, filters, worlds and runtime shutdown. Preserve upstream and third-party MIT notices.

## Validation and release

The initial development manifest uses `0.1.0-preview.1`. Do not increment the version for implementation steps or conversation turns; choose subsequent versions for actual releases. Install into the host game or check in host UVCS changes only when separately requested.

Compile against C# 9 and .NET Standard 2.1. Distinguish compilation, ABI inspection, Editor execution, player execution and performance measurements. Do not claim IL2CPP, Windows or cross-platform determinism without verification. Add or run tests only when the user requests them.

Do not invent `.meta` GUIDs. Preserve Unity-generated metadata and PluginImporter settings when completing the package. Do not hardcode developer-specific absolute paths in distribution files.
