# 원본 대비 변경 기록

기준 원본은 JoltPhysicsSharp 2.22.0, 커밋 `77a5be2dd30d587c1981dfcaf15851f18041b39c`다. 변경되지 않은 비교 자료와 SHA-256은 `Upstream~`와 `upstream.lock.json`에 보존한다. 이후 모든 구현 작업은 이 문서에 원본 대비 차이를 함께 남긴다.

## U001 — 독립 UPM 개발 구조와 원본 보존

- 날짜: 2026-10-01.
- 원본: 고정 커밋의 `src/JoltPhysicsSharp`, `Directory.Build.props`, `src/JoltPhysics.Native/JoltPhysics.Native.csproj`, 라이선스와 README. 패키지 설정·개발 문서는 신규.
- 변경 파일: `package.json`, `Runtime/JoltPhysicsSharp.Unity.asmdef`, `AGENTS.md`, `README.md`, `Documentation~`, `CHANGELOG.md`, `upstream.lock.json`, `Upstream~`, 라이선스 고지.
- 이유와 구분: 원본의 NuGet 배포 구조를 Unity용 UPM 개발 구조로 준비한 호환성 작업. 게임 코드 없이 분리해 개발하고, 구현마다 원본과 변경 이유를 추적하기 위한 작업 규칙을 추가했다.
- 영향: 원본 네임스페이스는 유지한다. 런타임 어셈블리 이름은 `JoltPhysicsSharp.Unity`다. 원본 사본은 Unity 임포트 대상에서 제외한다. 아직 네이티브 DLL이나 전체 물리 API를 제공하지 않는다.
- 검증: 고정 커밋 체크아웃과 원본 라이선스를 확인했다. 사본 해시·JSON·문서 링크의 확인 결과는 아래 검증 기록에 남긴다.
- 제한: 호스트 게임에 설치하지 않았다. Unity 임포트와 `.meta`, PluginImporter 및 플랫폼 실행 검증은 후속 단계다.

## U002 — 기초 값 타입의 Unity 호환 이식

- 날짜: 2026-10-01.
- 원본 파일: `src/JoltPhysicsSharp/BodyID.cs`, `Triangle.cs`, `MotionType.cs`, `Activation.cs`, `BackFaceMode.cs`.
- 변경 파일: 같은 이름의 `Runtime/*.cs` 5개.
- 이유와 구분: 파일 범위 네임스페이스, 기본 생성자, implicit usings 의존성을 C# 9 문법과 명시적 using으로 바꾸는 호환성 작업. `Triangle`의 수학 타입을 Unity Vector3로 교체하여 정한 API 방향의 구현을 시작했다.
- API 영향: BodyID의 상수·ID·세대 번호·연산자와 열거형 값은 유지한다. Triangle의 생성자와 꼭짓점 프로퍼티는 `System.Numerics.Vector3` 대신 `UnityEngine.Vector3`를 받는다.
- 동작 영향: Triangle 동일성은 Unity Vector3의 근사 `==`를 사용하지 않고 정확한 `Equals` 비교로 구현해 원본의 비교 의도를 유지한다. `!=`는 `==`의 부정으로 통일했다.
- ABI 영향: BodyID와 Triangle에 Sequential 배치를 명시했다. 실제 네이티브 호출과 구조체 크기·오프셋 검증은 아직 수행하지 않았다.
- 검증: Unity 6000.4.8f1의 컴파일러와 .NET Standard 2.1·UnityEngine 참조로 컴파일하는 결과를 아래에 기록한다. 동작 테스트와 네이티브 실행은 수행하지 않았다.
- 제한: Quaternion·행렬·월드·바디·쿼리 이식은 아직 완료되지 않았다.

## 이번 준비 작업의 검증 기록

2026-10-01에 다음을 확인했다.

- `package.json`, 런타임 asmdef, `upstream.lock.json`의 JSON 구문과 어셈블리·네임스페이스 일치: 통과.
- 원본 사본 183개 파일(관리 C# 소스 177개 포함): 고정 커밋 체크아웃과 바이트 단위 일치. 아카이브 SHA-256은 lock 파일과 일치.
- 패키지 LICENSE.md와 원본 LICENSE 일치, 이식한 소스의 원본 저작권 고지 보존: 통과.
- 문서의 상대 파일 링크와 게임 코드·System.Numerics 런타임 의존성 제외: 통과.
- Unity 6000.4.8f1에 포함된 Roslyn 컴파일러, 해당 Editor의 .NET Standard 2.1 참조 및 UnityEngine.CoreModule을 사용한 C# 9 라이브러리 컴파일: 기초 타입 5개 통과, 경고 없음. 결과 DLL은 임시 폴더에 두었으며 패키지에 넣지 않았다.

이 확인은 Unity Editor의 패키지 임포트나 네이티브 ABI·실제 시뮬레이션 검증이 아니다. 동작 테스트, 플랫폼별 플레이어와 IL2CPP 실행은 수행하지 않았다. UVCS 상태 조회는 사용자 설정 파일의 접근 제한으로 확인하지 못했다. 호스트 게임 코드와 패키지 설치 설정은 편집하지 않고 신규 패키지 폴더에서만 작업했다.

## U003 — 네이티브 기준 고정과 단정밀도 Unity interop

- 날짜: 2026-10-01. 상태: 구현, macOS arm64 Editor 실행 확인.
- 원본: 관리 코드 기준 커밋의 `JoltApi.cs`; joltc 커밋 `886e088675bae3a086f8318c7803f8ee962c2f2c`의 `include/joltc.h`, `src/joltc.cpp`, `CMakeLists.txt`; Jolt 5.6.0 커밋 `e77f175595e64cb44218cc9d9d56fc365ad0e36a`. 배치 확인 파일과 빌드 도구는 신규.
- 변경 파일: `Runtime/JoltApi.cs`, `Runtime/Plugins/macOS/libjoltc.dylib` 및 Unity 생성 메타데이터, `Native~`, `Tools~/build_macos.py`, 네이티브 원본 사본과 `upstream.lock.json`, 라이선스 고지.
- 이유와 구분: 원본 LibraryImport·최신 .NET 로더를 Unity에서 지원하는 DllImport와 플러그인 임포트로 대체하는 호환성 작업. 바이너리를 다른 Unity 래퍼에서 가져오지 않고 고정한 C API와 엔진 소스로 직접 빌드했다. 네이티브 원본 소스는 수정하지 않았다.
- API·ABI 영향: 62개 C API 선언에 Cdecl과 정확한 심볼 이름을 명시한다. C bool은 U1, ObjectLayer는 uint32, BroadPhaseLayer는 uint8이다. 단정밀도 위치는 Unity Vector3, 회전은 Unity Quaternion을 사용한다. PhysicsSystemSettings의 명시적 padding과 64비트 포인터 배치를 보존한다. 배정밀도는 지원하지 않는다.
- 빌드 영향: macOS arm64/x86_64 공용, OS 배포 타깃 11.0, Distribution, 오브젝트 레이어 32비트, 결정성 옵션과 CPU 옵션을 lock 파일에 고정했다. 결정성 옵션은 플랫폼 간 결정성 지원 보장이 아니다.
- 검증: C 헤더의 bool·레이어·ID·벡터·Quaternion·Triangle·설정 배치 static_assert 컴파일 통과. 선언 62개의 고정 헤더 존재와 두 아키텍처의 export 심볼 확인. Mach-O 두 아키텍처와 OS 타깃 11.0 확인. 실제 빌드·아카이브 SHA-256 보존.
- 빌드 관찰: AppleClang이 원본의 `-ffp-model=precise`와 결정성 옵션의 `-ffp-contract=off` 조합에 경고를 출력했으나 링크는 성공했다. GPU 셰이더용 dxc·Vulkan 발견 실패 메시지는 있었으며 이 작업에서 해당 기능을 사용하지 않았다.
- 제한: Intel 실행, macOS 11 실행, Windows·플레이어·IL2CPP 미확인. 전체 관리 API 및 콜백 ABI는 후속 범위다.
- 요구사항: ABI-01, NATIVE-01, MATH-01.

## U004 — 초기화·네이티브 소유권과 바디 수명 보호

- 날짜: 2026-10-01. 상태: 구현, 예제의 정리·종료 확인.
- 원본 파일: `Foundation.cs`, `NativeObject.cs`, `PhysicsSystem.cs`, `BodyInterface.cs`, `Body.cs`, 테이블 기반 필터 파일과 joltc의 `JPH_PhysicsSystem_Create/Destroy`, `JPH_PhysicsSystem_Update`.
- 변경 파일: `Runtime/Foundation.cs`, `NativeObject.cs`, `CollisionLayers.cs`, `PhysicsSystem.cs`, `BodyInterface.cs`, `MathValidation.cs`(신규).
- 이유와 구분: joltc의 월드 파괴는 세 충돌 필터도 delete한다. 관리 필터 래퍼가 이후 원본 Destroy를 다시 호출하거나 다른 월드에서 같은 필터를 사용하지 못하도록 소유권 이전을 명시한 동작 개선이다. 전역 TempAllocator와 전체 런타임 수명도 관리 코드에서 보호한다.
- API·동작 영향: Init 호출 수와 살아 있는 네이티브 객체 수를 추적한다. 마지막 Shutdown은 살아 있는 객체가 있으면 거부한다. ObjectVsBroadPhase 생성 이후 테이블 구성을 고정하고, 월드 생성 성공 시 필터 소유권을 이전한다. 월드 종료는 바디를 제거·파괴하고 필터 래퍼를 무효화한다. Dispose 중복 호출은 이미 파괴된 객체에서 아무 작업도 하지 않는다. finalizer는 명시적 Dispose 누락 시 보조 정리이며 정상 사용은 Dispose를 요구한다.
- 바디 영향: BodyInterface가 월드를 참조하고 월드는 생성한 세대 ID와 포인터를 추적한다. 삭제한 바디 ID와 종료된 월드의 접근을 거부한다. Body는 월드 소유 조회 객체이며 원본 NativeObject 상속·독립 Dispose와 원시 포인터 변환 API를 제공하지 않는다. 파괴는 BodyInterface로 수행한다.
- 스레드 영향: 스텝·관리 API·Dispose를 공통 잠금으로 보호한다. 현재 API의 여러 월드 동시 Update는 직렬화한다. 네이티브 JobSystem 내부 작업은 병렬로 실행된다.
- 검증: Box 예제 종료 및 이후 Compound 예제 실행에서 월드·바디·필터·형상·설정·JobSystem 정리와 마지막 Shutdown 성공. 강제 GC/finalizer 순서, 동시 호출과 여러 월드 조합에 대한 별도 동작 테스트는 수행하지 않았다.
- 제한: 이벤트·필터 델리게이트 수명은 아직 구현하지 않았다. 도메인 리로드·플레이 모드 반복과 AOT 확인도 남아 있다.
- 요구사항: INIT-01, LIFE-01, WORLD-01, BODY-01.

## U005 — Unity 수학 타입의 월드·Box·Compound·바디 API

- 날짜: 2026-10-01. 상태: 구현, Box·Compound 예제 실행 확인.
- 원본 파일: `PhysicsSystem.cs`, `PhysicsUpdateError.cs`, `JobSystem.cs`, `JobSystemThreadPool.cs`, `ObjectLayer.cs`, `BroadPhaseLayer.cs`, 충돌 테이블 파일, `Shape/Shape.cs`, `BoxShape.cs`, `CompoundShape.cs`, `StaticCompoundShape.cs`, `BodyCreationSettings.cs`, `BodyInterface.cs`.
- 변경 파일: 같은 기능을 담당하는 `Runtime` 파일 13개와 관련 interop 선언. 여러 원본 파일은 기능별 파일로 묶었다.
- 이유와 구분: implicit usings·기본 생성자·파일 범위 네임스페이스 의존성을 C# 9로 바꾸고, System.Numerics Vector3·Quaternion을 UnityEngine 타입으로 교체한 호환성 작업. 수학 값과 레이어·바디 상태를 네이티브 호출 전에 확인하는 보호를 추가했다.
- API 영향: PhysicsSystemSettings의 C# 9 기본값은 0이며, 용량 0은 네이티브 기본값을 사용한다. 명시적 기본값은 `PhysicsSystemSettings.Default`로 제공한다. 구현 범위는 중력·Update·최적화, Box·StaticCompound, 바디 생성·추가·제거·파괴, 자세·질량 중심·선형 속도·키네마틱 이동·활성화다. 원본의 전체 프로퍼티·오버로드를 제공하지 않는다.
- 동작 영향: Quaternion은 정규화를 요구하며 자동 축 반전이나 회전 보정은 하지 않는다. Compound 자식 오프셋과 회전을 그대로 전달한다. 형상을 만든 Compound 설정을 다시 변경하지 못하게 하여 원본 캐시의 오래된 형상을 재사용하지 않게 한다. ShapeSettings 기반 바디 설정 및 Compound 자식 추가는 Shape를 먼저 생성해 오류를 드러내고 생성 시점의 형상을 참조한다.
- 검증: 런타임 C# 18개 파일의 Unity 참조·C# 9 컴파일 통과. Unity 6000.4.8f1 arm64 Editor에서 Static 바닥과 Dynamic Box·Compound 120 스텝 진행, 바디 원점 Vector3·Quaternion 및 Compound 로컬 질량 중심 조회 성공. 자식 Shape 래퍼를 먼저 Dispose한 Compound도 실행됐다.
- 관찰 결과: Box Y 약 0.480, Compound Y 약 0.230, Compound 로컬 질량 중심 X 0.125. 이 결과는 실행한 예제의 관찰이며 범용 정확도·성능 보장이 아니다.
- 제한: 질량 설정·MutableCompound·행렬·쿼리·접촉 이벤트 미구현. 키네마틱 이동·속도·활성화 API는 컴파일과 원본 대조만 수행했으며 별도 실행 예제에 포함하지 않았다.
- 요구사항: MATH-01, WORLD-01, SHAPE-01, BODY-01.

## U006 — Unity 임포트·샘플·플러그인 메타데이터 보존

- 날짜: 2026-10-01. 상태: 구현, 별도 임시 프로젝트의 임포트·예제 실행 확인.
- 원본: 샘플·문서 신규. 관리·네이티브 기준은 위와 동일하다.
- 변경 파일: `Samples~/DataWorld`, `package.json`, Unity가 생성한 `.meta`, `README.md`, `Documentation~/Usage.md`, 요구사항 상태·변경 이력.
- 이유와 구분: GameObject 없이 데이터 중심으로 사용할 수 있는 샘플을 제공하고, 실제 Unity의 컴파일·플러그인 로딩 경로를 확인했다. GUID는 Unity가 생성한 값을 보존했다.
- 임포트 영향: PluginImporter에서 Any 플랫폼을 끄고 macOS Editor와 StandaloneOSX, 두 CPU를 대상으로 설정했다. 샘플은 UPM의 명시적 임포트 대상이며 호스트 게임에 자동으로 추가하지 않는다.
- 검증: Client.Add로 별도 프로젝트에 로컬 UPM 설치 성공, Unity CLI 예제 실행 두 번 모두 종료 코드 0. Box 예제와 확장한 Compound 예제에서 실제 네이티브 스텝·자세 조회·정리가 성공했다. 테스트 프레임워크나 단위 테스트는 추가·실행하지 않았다.
- 실행 환경: 첫 샌드박스 실행은 Unity UPM 로컬 소켓 제한으로 실패했다. 임시 프로젝트만 승인된 샌드박스 밖 실행으로 재시도하여 완료했다. 라이선스 토큰 갱신 및 종료 시 Unity 자체 로그 메시지는 있었으나 임포트와 예제는 성공 결과를 반환했다.
- 추가 확인: 네이티브 빌드 재현 도구 실행과 배치 컴파일도 성공했다. UVCS에서 패키지 폴더의 신규 변경 상태를 읽었다. 앞 준비 단계에서 조회가 막혔던 상태와 달리 이번 조회는 성공했다.
- 최종 정적 확인: 런타임 18개 파일의 C# 9 컴파일, 각 아키텍처의 62개 export, 원본 사본·플러그인 해시와 재현 도구 출력 바이너리 일치, Unity 생성 메타데이터 34개의 GUID 중복·누락 검사, 문서 상대 링크 확인 통과. 실제 예제 결과는 `Documentation~/DataWorldResult.json`에 보존했다.
- 제한: 호스트 게임에 설치하지 않았으며 게임 코드·기존 NuGet 구성도 변경하지 않았다. 변경 사항은 체크인하지 않았다.
- 요구사항: PKG-01, NATIVE-01, WORLD-01, LIFE-01.

## U007 — 미배포 버전과 변경 이력 정리

- 날짜: 2026-10-01. 상태: 반영.
- 원본: 패키지 메타데이터·작업 규칙 신규. 관리·네이티브 원본 기준은 변경하지 않았다.
- 변경 파일: `package.json`, `CHANGELOG.md`, `AGENTS.md`, 이 문서.
- 문제와 이유: 구현 작업 한 번을 배포로 간주해 `0.1.0-preview.2`로 올렸으나 실제 배포는 없었다. 준비 버전을 `0.1.0-preview.1`로 되돌리고 모든 미배포 변경을 `Unreleased`로 모았다. 앞으로 구현 단계마다 버전을 증가시키지 않는 규칙을 추가했다.
- API·ABI·동작 영향: 메타데이터와 문서만 변경했다. 런타임·네이티브 바이너리·고정 원본은 그대로다. 이전 로컬 검증 당시의 버전 표기와 실행 증거는 실제 수행 기록으로 보존한다.
- 검증: package.json JSON 구문과 준비 버전, 변경 이력의 단일 Unreleased 항목 확인.
- 제한: 이번 변경에서 Unity 실행·네이티브 빌드를 반복하지 않았다. 기존 구현의 미완료 범위와 검증 제한은 U003~U006을 따른다.
- 요구사항: PKG-01.

## U008 — 독립 Git 저장소 공유 준비

- 날짜: 2026-10-02. 상태: 배포 준비 반영.
- 원본: UPM 저장소 메타데이터·Git 설치 안내·ignore 설정 신규. 관리·네이티브 원본 기준은 변경하지 않았다.
- 변경 파일: `package.json`, `README.md`, `CHANGELOG.md`, `.gitignore`, 이 문서.
- 이유와 구분: 사용자가 ciart GitHub 조직으로 푸시를 요청하고 저장소 이름을 `JoltPhysicsUnity`로 지정했다. `ciart/JoltPhysicsUnity`의 저장소 루트를 패키지 루트로 구성하고 Git URL 설치 방법과 비공개 저장소 인증 조건을 기록했다. 정식 릴리스와 개발 소스 공유를 구분해 기존 버전을 유지했다.
- API·ABI·동작 영향: 코드·네이티브 바이너리·Unity 생성 GUID는 변경하지 않았다. `.gitignore`는 로컬 임시 파일만 제외하며 배포 플러그인·원본 사본·메타데이터는 포함한다. MIT 고지 전문의 게임 배포물 동봉 조건도 설치 안내에 명시했다.
- 검증: 기존 UVCS 패키지 범위와 전체 파일 목록을 확인했다. 업로드 대상 75개 파일의 별도 임시 복사본과 원본이 일치한다. 고정 원본 사본·플러그인 SHA-256, JSON, Unity 생성 GUID 34개의 중복·메타데이터 누락과 문서 상대 링크 확인 통과. Git 원격 조회·푸시의 실제 결과는 작업 완료 보고에서 구분한다.
- 제한: 이번 작업은 저장소 공유이며 추가 Unity 실행·새 플랫폼 확인을 수행하지 않는다. 태그·GitHub Release는 발행하지 않고, 호스트 게임의 패키지 설치·NuGet 제거·UVCS 체크인은 수행하지 않는다.
- 요구사항: PKG-01, SRC-01, NATIVE-01.

## U009 — 공개 저장소 전환과 설치 안내 갱신

- 날짜: 2026-10-02. 상태: 공개 전환 확인, 설치 안내 반영.
- 원본: Git 저장소 공개 설정·설치 안내 신규. 관리·네이티브 원본 기준은 변경하지 않았다.
- 변경 파일: `README.md`, `CHANGELOG.md`, 이 문서. GitHub의 `ciart/JoltPhysicsUnity` 공개 설정도 변경했다.
- 이유와 구분: 사용자가 공개 저장소 전환을 요청했다. GitHub 인증 없이 Git URL로 가져올 수 있도록 공개로 전환하고 README의 비공개 인증 조건을 제거했다. U008은 처음 비공개로 준비한 작업의 기록으로 보존한다.
- API·ABI·동작 영향: 런타임·네이티브 바이너리·Unity 메타데이터·라이선스·버전은 변경하지 않았다. 버전은 `0.1.0-preview.1`을 유지한다.
- 검증: GitHub API의 isPrivate=false 확인. 사용자 Git 설정·credential helper·인증 프롬프트를 제외한 환경에서 공개 HTTPS URL의 main 브랜치 조회 성공. 문서 변경은 같은 패키지의 Git 작업 폴더로 복사하여 원본과 일치하는지 확인하고 푸시한다.
- 제한: 설치 안내와 원격 접근만 확인하며 Unity 실행·새 플랫폼 확인은 반복하지 않는다. 호스트 게임 설치·NuGet 제거·UVCS 체크인은 수행하지 않는다.
- 요구사항: PKG-01.

## 다음 변경의 기록 양식

각 변경을 아래 형식으로 추가한다. 계획을 기록할 때는 `계획` 상태를 명시하고 구현 완료 항목과 구분한다.

```text
Uxxx — 변경 제목
날짜 / 상태:
원본 버전·커밋 / 원본 파일 또는 신규:
변경 파일:
문제와 변경 이유 / 호환성 수정 또는 동작 개선:
API·ABI·동작 영향:
실제 수행한 검증 / 환경 / 결과:
수행하지 않은 검증과 남은 제한:
연결한 요구사항 ID:
```
