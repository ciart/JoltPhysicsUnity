# 변경 이력

## Unreleased

현재 `0.1.0-preview.1`을 개발 중이며, 태그와 GitHub Release는 아직 발행하지 않았다. 개발 중 변경은 이 항목에 누적한다.

- 독립 UPM 개발 구조, UnityEngine 수학 타입 사용 방향, 단계별 구현 요구사항을 준비했다.
- JoltPhysicsSharp 2.22.0의 원본 커밋과 비교용 소스 사본을 고정했다.
- 코드 변경마다 원본 대비 이유와 검증을 기록하는 작업 규칙을 추가했다.
- BodyID, Triangle, MotionType, Activation, BackFaceMode를 이식했다. Triangle은 UnityEngine.Vector3를 사용한다.
- 고정한 joltc·Jolt 소스에서 macOS arm64/x86_64 공용 네이티브 플러그인을 직접 빌드했다.
- UnityEngine Vector3·Quaternion 기반 월드·바디·Box·StaticCompound와 고정 스텝, 자세·속도·키네마틱 이동 API를 추가했다.
- 필터 소유권을 월드로 이전하고, 살아 있는 객체의 전체 런타임 종료 및 파괴한 바디 ID 접근을 제어한다.
- 원본의 LibraryImport 대신 단정밀도 C ABI의 DllImport 62개를 제공하고 핵심 배치를 확인한다.
- Unity가 생성한 메타데이터와 macOS 플러그인 설정, 데이터 월드 샘플과 빌드 재현 도구를 보존했다.
- Unity 6000.4.8f1 macOS arm64 Editor에서 씬 없는 Box·Compound 예제 120 스텝과 자세 조회·정리를 확인했다.
- 구현 단계마다 버전을 올리지 않고 배포 단위로 버전을 정하는 작업 규칙을 추가했다.
- `ciart/JoltPhysicsUnity` 독립 Git 저장소의 루트에서 설치할 수 있도록 저장소 메타데이터와 Git URL 설치 안내를 추가했다.
- 사용자의 요청에 따라 저장소를 공개로 전환하고 GitHub 인증 없이 설치하는 안내로 갱신했다.

쿼리·접촉 이벤트, Windows 및 IL2CPP 지원은 아직 완료하지 않았다. 세부 차이와 검증 범위는 `Documentation~/UpstreamChanges.md`에 기록한다.
