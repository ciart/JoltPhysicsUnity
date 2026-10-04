# Changes from upstream

Baseline: JoltPhysicsSharp 2.22.0, `3ab66bf9e970b10c2b22b8f5b88dfdc2c765a7fb`.

## UNITY-001 — Restart on the upstream fork (2026-10-03)

- Original: baseline `README.md` and `.gitignore`; other documents and rules are new.
- Changed: `AGENTS.md`, `README.md`, the former `README.ko.md`, `.gitignore`, `Unity~/upstream.json`, `Unity~/Documentation/Implementation.md` and this log.
- Reason: replace the standalone repository approach with direct modification of original sources in an upstream GitHub fork. Pin Unity naming, math types, completion criteria and required change records.
- Impact: no managed API or native ABI changes. Preserve upstream .NET installation information in a separate section. Disclose GPT (Codex) use for Unity porting. Add an exception only for the Unity documentation folder to the upstream `*~` ignore rule.
- Verification: baseline `git rev-list --count HEAD` returned 486; parent commits and 177 original C# files were inspected. Upstream history and original paths are retained.
- Limits: UPM import and Unity native execution were not checked. Previous standalone-repository binaries and results do not validate this fork.

## UNITY-002 — C# 9 and Unity compatibility for original foundational types (2026-10-03)

- Original: baseline `src/JoltPhysicsSharp/{BodyID,ObjectLayer,BroadPhaseLayer,SubShapeID,CollisionGroupID,CollisionSubGroupID,CharacterID,Bool8,Bool32,Triangle,Activation,MotionType,BackFaceMode}.cs`.
- Changed: those 13 files directly; new `Unity~/compile-scopes.json` and `Unity~/Tools/compile-scope.py`; implementation status updated.
- Reason: Unity C# 9 source compilation cannot use primary constructors, file-scoped namespaces or implicit usings. Use explicit constructors, block namespaces and System imports while preserving files and types. Remove Bool32's unused JoltApi reference.
- API/ABI impact: retain ID names, values, conversions, generation bits and constants, and byte/uint bool fields. Make nullable context explicit. Triangle uses UnityEngine.Vector3 for Unity and System.Numerics.Vector3 for .NET. Use exact Equals rather than approximate Unity `==`; define inequality as its negation. No native declarations changed.
- Verification: compile the actual 13 files with Unity 6000.4.8f1 Roslyn, C# 9, .NET Standard 2.1 references and warnings as errors. Both the Unity path (`UNITY_5_3_OR_NEWER` with actual CoreModule references) and non-Unity path passed without errors or warnings. The scope compiler reproduces these checks.
- Limits: full 177-file compilation, native structure layout, Triangle execution and UPM import were not verified in this change. Foundational compilation does not establish package usability.

## UNITY-003 — Port original geometry, query results and enums (2026-10-03)

- Original: 35 baseline files under `src/JoltPhysicsSharp`. The exact list is the `geometry` scope minus `foundations` in `compile-scopes.json`. Geometry/results/settings: `BoundingBox.cs`, `Ray.cs`, `MathUtil.cs`, `BroadPhaseCastResult.cs`, `RayCastResult.cs`, `CollidePointResult.cs`, `CollideShapeResult.cs`, `ShapeCastResult.cs`, `SubShapeIDPair.cs`, `IndexedTriangle.cs`, `IndexedTriangleNoMaterial.cs`, `ContactSettings.cs`. The other 23 files contain standalone enums.
- Changed: modify those original files directly; update scope lists, status, baseline metadata and both then-existing READMEs; add `Unity~/Documentation/GeometryLayout.json`.
- Compatibility: use block namespaces and explicit imports/nullable context. Select UnityEngine Vector3/Matrix4x4 for Unity. Ray.Transform uses affine MultiplyPoint3x4, preserving displacement length by subtracting transformed endpoints. MathUtil retains upstream arithmetic and zero-vector behavior. Retain the .NET path.
- Behavior improvements: make BoundingBox operators use the same exact equality as Equals, avoiding a Unity approximate-equality/hash mismatch. Correct Ray.Direction documentation to describe displacement with length. Match CollideShapeResult face counts to native uint32_t and use checked conversion for Span lengths, preventing oversized counts from wrapping negative. Field sizes and layout are unchanged.
- API/ABI impact: retain public type/field/enum names and enum values. Unity vector/matrix parameters and results use UnityEngine types. Only internal count signedness and overflow handling change. Suppress CS0649 only for native-written internal CollideShapeResult fields.
- Compilation: actual 48 original files passed Unity 6000.4.8f1 Roslyn / .NET Standard 2.1 / C# 9 with warnings as errors in both Unity and non-Unity paths.
- Layout inspection: compile the actual header from joltc `886e088675bae3a086f8318c7803f8ee962c2f2c`, SHA-256 `5b6e2ca74170bb0c7760463e965f9958691a15bbf7b80ffd005886c9895759a7`, with C++. On macOS arm64, single precision and 8-byte pointers, compare sizeof/offsetof against Marshal.SizeOf/OffsetOf on both managed outputs. All 22 type sizes and 29 field offsets across CollideShapeResult, ShapeCastResult and ContactSettings match. Sizes: Unity Vector3 12, Quaternion 16, Triangle 40, CollideShapeResult 80, ShapeCastResult 60 and ContactSettings 52 bytes. Full results are in GeometryLayout.json.
- Limits: managed layout inspection ran on .NET 9. Unity Mono/IL2CPP execution, native calls, Ray transformation execution, face-array lifetime/release and event delivery were not checked. Full original API compilation is incomplete.

## UNITY-004 — Record source-comment preference (2026-10-03)

- Original: AGENTS from UNITY-001 and CollideShapeResult modified in UNITY-003.
- Changed: require omission of new comments that restate code; remove the internal-field explanation added in the preceding change. Preserve original comments and copyright notices.
- Reason: user requested that change rationale be recorded in documents without unnecessary source comments.
- Impact/verification: comments and rules only; no compilation tokens, API, ABI or behavior changes. No additional compilation performed.

## UNITY-005 — English-only repository content (2026-10-03)

- Original: AGENTS, README, Implementation, UpstreamChanges and compile-scope.py added or modified above; the former Korean README.
- Changed: translate repository rules, requirements, change records and tool messages into English. Remove README.ko.md and its link from README.md. Require English for all future repository content, including commit messages.
- Reason: the user's latest instruction prohibits Korean anywhere in repository content and supersedes the earlier bilingual README/development-record preference. Conversation remains Korean.
- Impact: no managed API, native ABI or release-version changes. Prior change IDs, source mappings, verification results and limitations remain recorded.
- Verification: scan tracked text for Hangul across the complete working tree, including hidden and ignored files outside .git; no occurrences remain. Run the modified scope compiler for geometry; both 48-file compilation paths pass.
- Limits: this is a working-tree content change. Upstream and existing Git commit history remain intact.

## UNITY-006 — Consolidate the initial port commits (2026-10-03)

- Original: the five port commits after the pinned upstream baseline, ending at `a3bcd9e2a8774522459927ccfed9afb94230f487`; AGENTS and this log.
- Changed: consolidate those commits into one commit directly after the upstream baseline. Update the history rule to allow user-authorized consolidation with an explicit expected remote commit. Preserve the logical change records above.
- Reason: the user explicitly requested commit consolidation and authorized the force push.
- Impact: no managed source, API, ABI, tool or release-version changes. The 486 upstream commits remain ancestors of the consolidated commit. A local backup reference retains the previous port history.
- Verification: remote main matched the inspected previous commit before rewriting. The consolidated commit has the pinned baseline as its sole parent. All files match the previous main tree except AGENTS and this change log; the prior compilation and layout results still apply. English-only repository content and a clean working tree were checked.
- Publication: use force-with-lease with the inspected previous remote commit as the explicit expected value.

## UNITY-007 — Update the repository address and Unity installation guidance (2026-10-04)

- Original: README.md, Unity~/upstream.json and Unity~/Documentation/Implementation.md after UNITY-006; local origin configuration.
- Changed: point origin to `https://github.com/ciart/JoltPhysicsUnity.git`, record the current Unity fork URL, and rewrite the README around the Unity port. Add Package Manager steps, the planned `?path=/src/JoltPhysicsSharp` URL, release-revision syntax and the existing source compilation workflow. Retain GPT (Codex) attribution, original author credits and MIT notices. Link upstream .NET installation instructions instead of presenting NuGet commands as Unity setup.
- Reason: the user moved the fork and requested README updates with Unity installation instructions.
- Impact: documentation and repository metadata only; no managed source, API, ABI, native artifact or release-version changes. Keep forkRepositoryAtStart as historical provenance.
- Verification: GitHub reports a public fork at ciart/JoltPhysicsUnity with amerkoleci/JoltPhysicsSharp as its parent. Fetch confirms local and remote main match before editing. Inspect the repository for package.json and assembly definitions; neither is present. Check the documented Package Manager workflow, package-subfolder requirement and path-before-revision syntax against Unity 6.4 official documentation. Check local links, JSON validity, English-only content and the final diff.
- Limits: the current main branch is not an installable Unity package. URLs are documented as future installation instructions, not verified installation results. No host package installation or additional runtime execution was performed for this documentation change.

## UNITY-008 — Add the initial UPM manifest (2026-10-04)

- Original: package manifest and package README are new. The package LICENSE is an unchanged copy of the upstream root LICENSE. Existing README, AGENTS, Unity~/upstream.json and Implementation.md follow UNITY-007.
- Changed: add `src/JoltPhysicsSharp/package.json` for `com.ciart.joltphysics`, display name JoltPhysicsUnity, initial version `0.1.0-preview.1`, Unity baseline 6000.4 and English documentation/license links. Include README and MIT license inside the package subfolder. Update the root installation instructions, working rules and package metadata to reflect the actual manifest path.
- Reason: the user requested package.json so the Git dependency URL can target a UPM manifest. Preserve original source paths instead of moving or replacing source files.
- Impact: packaging and documentation only; no C# source, API, ABI, native binary, workflow or host project changes. The initial development version is not a new runtime release or an increment from the previous preview. No assembly definition or invented meta GUIDs are added.
- Verification: parse the manifest and metadata, validate the required name/version fields and matching package ID/version/path, compare package LICENSE byte-for-byte with the original, verify local documentation links and English-only content, and check the diff. The documented package path contains its manifest. Follow Unity 6.4 official package-manifest and Git URL requirements.
- Limits: Unity package resolution, the full assembly, native execution and CI execution have not been run for this packaging change. The manifest alone does not make the unported source a working Unity runtime; README and package description retain that status.

## UNITY-009 — Use the repository root as the UPM package root (2026-10-04)

- Original: src/JoltPhysicsSharp/package.json, package README and license copy from UNITY-008; root README, AGENTS, Unity~/upstream.json and Implementation.md.
- Changed: move the manifest unchanged to root package.json, use the existing root README and LICENSE, and remove the redundant subfolder copies. Set packagePath to a relative dot and document `https://github.com/ciart/JoltPhysicsUnity.git` without a path query, including full commit-hash pinning. Record the root-package rule in AGENTS.
- Reason: the user requested a repository-root package and a simpler Git URL. A subfolder query is only needed when the manifest lives in a subfolder; the managed sources can retain their original paths under a root package.
- Impact: package-root selection changes; package ID, version, C# source paths, upstream history, API and ABI remain unchanged. The root package includes the broader repository tree, including original native, sample and test directories. Existing runtime and Unity-import limitations remain explicit.
- Verification: compare the root manifest byte-for-byte with the previous subfolder manifest, validate its JSON and package metadata, verify root README/LICENSE and local documentation links, confirm no C# source or native files changed, scan for Hangul and check the diff. The bare Git URL format matches Unity 6.4 official documentation.
- Limits: Unity resolution, native importer settings, sample/test assembly isolation and full runtime execution are not validated by moving the manifest. No host project installation or package-version increment was performed.
