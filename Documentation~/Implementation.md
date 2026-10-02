# Unity 이식 구현 요구사항

목표는 JoltPhysicsSharp의 물리 API를 Unity 수학 타입으로 제공하는 독립 UPM 패키지다. 게임의 순수 C# 데이터가 바디를 소유하고, 별도의 씬 컴포넌트가 그 자세를 프리팹에 적용할 수 있어야 한다. 패키지 자체는 씬이나 `MonoBehaviour`를 요구하지 않는다.

현재 기준은 JoltPhysicsSharp 2.22.0, 커밋 `77a5be2dd30d587c1981dfcaf15851f18041b39c`다. 원본은 .NET 9/10, C# 14와 `System.Numerics`를 사용한다. Unity용 런타임은 .NET Standard 2.1, C# 9 문법과 UnityEngine 수학 타입을 기준으로 이식한다. 원본 API 전체를 한 번에 옮기지 않고 아래 필수 범위를 먼저 완성한다.

## 필수 구현과 완료 조건

| ID | 요구사항 | 완료 조건 | 현재 상태 |
| --- | --- | --- | --- |
| PKG-01 | 독립 UPM 구조 | 호스트 코드 의존성 없는 매니페스트·런타임 어셈블리·문서와 변경 기록 | 별도 Unity 프로젝트의 로컬 UPM 임포트·컴파일 확인 |
| SRC-01 | 원본 고정 | 커밋, 관리 코드 사본, 라이선스와 SHA-256 보존 | 완료 |
| MATH-01 | Unity 수학 타입 | Vector3·Quaternion·Matrix4x4 사용; 비교·회전·행렬·배치 의미 검토 | Vector3·Quaternion ABI와 Compound 자식 회전 확인; 행렬 API 미구현 |
| ABI-01 | Unity 호환 네이티브 호출 | LibraryImport와 최신 .NET 전용 API 교체; 고정 헤더와 시그니처·배치 대조 | 62개 DllImport, 핵심 배치・심볼 확인; 전체 원본 API 이식은 미완료 |
| INIT-01 | 초기화·종료 | 반복 초기화, 여러 월드, 마지막 월드 종료와 콜백 정리 순서 정의 | 참조 수·살아 있는 객체의 종료 방지 구현, 예제의 정리·종료 확인; 콜백은 후속 단계 |
| WORLD-01 | 월드와 고정 스텝 | 설정·중력·JobSystem·Update 제공; GameObject 없이 호출 가능 | 구현, macOS arm64 Editor에서 120 스텝 실행 확인 |
| SHAPE-01 | 기본 형상 | Box와 Compound 형상, 자식 위치·회전, 질량과 공유 형상 수명 | Box·StaticCompound 구현, 회전·오프셋·질량 중심과 자식 참조 수명 예제 확인; 질량 설정 API 미구현 |
| BODY-01 | 바디 수명·제어 | 생성·추가·제거·파괴, Static/Kinematic/Dynamic, 활성화·자세·속도 | 생성·수명·자세·속도·키네마틱 이동 구현; 예제는 Static/Dynamic 실행 확인 |
| QUERY-01 | 충돌 쿼리·필터 | RayCast, CollideShape, CastShape 및 바디·레이어 필터; 결과와 반환형 일치 | 미구현 |
| CONTACT-01 | 충돌·활성화 이벤트 | 바디 ID와 필요한 접촉 정보 전달; 스레드·포인터 수명 안전성 | 미구현 |
| LIFE-01 | 자원 수명 | IDisposable, 델리게이트·GCHandle·네이티브 참조 해제, 파괴 후 접근 제어 | IDisposable·필터 소유권 이전·바디 ID 수명 검사 구현; 콜백 관련 자원은 미구현 |
| NATIVE-01 | 플러그인 배포 | 지원 플랫폼의 일치하는 바이너리·빌드 기준·해시·임포트 설정 | macOS universal 직접 빌드·해시·원본·메타데이터 보존; 다른 플랫폼은 미포함 |
| AOT-01 | IL2CPP | 정적 역호출·MonoPInvokeCallback·스트리핑 대응과 플레이어 확인 | 미구현 |
| PERF-01 | 데이터 중심 사용 | 시뮬레이션과 반복 자세 조회에서 불필요한 할당·객체 생성 방지 | 미확인 |

단계 상태는 코드 작성만으로 완료로 바꾸지 않는다. 해당 완료 조건을 확인한 환경과 결과를 변경 기록에 남긴다. 현재는 초기 월드·바디 실행까지 가능하며, 필수 범위 전체가 완료된 상태는 아니다.

## 수학 타입과 ABI

공개 API의 위치와 방향은 `UnityEngine.Vector3`, 회전은 `UnityEngine.Quaternion`을 사용한다. 원본이 Vector4나 행렬을 사용하는 API도 필요할 때 UnityEngine 타입으로 옮긴다. 단정밀도를 첫 범위로 삼으며 원본의 `RVector3`와 배정밀도 경로는 이 범위에서 제외한다.

벡터를 교체할 때 멤버 이름뿐 아니라 연산 의미를 검토한다. Unity Vector3의 `==`는 근사 비교이므로, 원본의 정확한 동일성 비교와 해시 계약을 유지할 위치에서는 정확한 비교를 사용한다. Quaternion의 기본값, 정규화 조건, 회전 합성 순서도 API별로 정한다. 행렬은 행·열 순서와 평행이동 위치를 명시적으로 변환하고 메모리를 그대로 재해석하지 않는다. 축과 회전 변환은 단순 축 이동과 비대칭 형상 회전을 기준으로 확인한다.

`LibraryImport`, `UnmanagedCallersOnly`, 함수 포인터 별칭, `InlineArray`, 파일 범위 네임스페이스, 기본 생성자와 implicit usings 등은 Unity에서 지원하는 표현으로 변경한다. 일괄 변환 후 컴파일 성공만으로 ABI가 맞다고 판단하지 않는다. `DllImport`의 CallingConvention, bool의 실제 너비, 포인터·배열·출력 인수와 구조체 정렬을 네이티브 헤더와 대조한다.

## 월드·바디·형상 계약

호출자가 스텝 시점과 `deltaTime`을 결정한다. 자동 `Update`·`FixedUpdate` 실행이나 프리팹 생성을 라이브러리에 넣지 않는다. 초기 JobSystem은 원본의 네이티브 스레드 풀을 기준으로 이식한다. Burst·Unity Jobs 연동은 첫 완료 조건에 포함하지 않는다.

바디 ID에는 인덱스와 세대 번호를 유지한다. 생성·월드 추가·월드 제거·파괴를 구분하고, 제거 후 재사용 및 파괴 후 접근 정책을 문서화한다. 자세 API는 바디 원점과 질량 중심을 구분해 이름과 의미를 유지한다. 키네마틱 이동과 위치 강제 변경은 속도·접촉에 미치는 의미를 따로 제공한다.

Box는 반경이 아닌 각 축의 half extent를 받는 원본 의미를 유지한다. Compound는 자식 형상의 위치·회전과 질량 중심을 반영한다. 형상 공유·참조 수와 바디 생성 설정의 소유권을 명시한다. 프리팹 스케일과 시각적 오프셋은 호출자가 처리하며, 패키지가 Transform을 따라 형상을 자동 변경하지 않는다.

전체 런타임 초기화와 월드 수명을 구분한다. 살아 있는 월드나 콜백이 남은 상태에서 전체 런타임을 종료하지 않는다. 진행 중인 스텝과 Dispose가 충돌하지 않게 하고, 중복 Dispose 및 바디 파괴 이후의 호출을 제어한다. Editor의 도메인·플레이 모드 재진입도 확인 대상이다.

## 쿼리·필터·이벤트 계약

RayCast의 이동 벡터와 CastShape의 이동 벡터는 길이를 포함하는 원본 의미를 유지한다. 히트 비율, 바디·서브형상 ID, 접촉 위치와 법선을 제공한다. `CollideShapeResult`와 `ShapeCastResult`를 구분하고, collector가 반환하는 float와 void 콜백을 바꾸어 연결하지 않는다.

BodyFilter, ObjectLayerFilter, BroadPhaseLayerFilter와 레이어 충돌 설정을 실제로 구현한다. 자기 바디 제외와 지정 레이어 포함·제외가 가능해야 한다. 필터와 쿼리 콜백의 수명을 네이티브 호출 기간에 맞춰 보장한다. 결과 정렬·최근접 선택·출력 버퍼 용량과 초과 처리 방식을 명시한다.

접촉 이벤트는 Added, Persisted, Removed를 구분한다. 이벤트가 제공할 바디 ID, 서브형상 ID 및 접촉 데이터는 각 네이티브 콜백에서 실제 제공되는 범위에 맞춘다. Removed 이벤트에 존재하지 않는 위치나 법선을 만들어 내지 않는다. 활성화·비활성화 이벤트도 바디 ID를 제공한다.

접촉 콜백은 워커 스레드에서 호출될 수 있으며 바디 잠금 상태도 고려해야 한다. 콜백 안에서 Unity API를 사용하거나 동일 바디 잠금을 다시 획득하지 않는다. 유효한 값을 복사해 큐에 보관하고, 스텝 이후 호출자 스레드에서 전달한다. 큐가 가득 찬 경우를 드러내고 이벤트를 조용히 유실하지 않는다. 즉시 반환이 필요한 필터·검증 콜백은 큐로 미루지 않으며, 예외 발생 시 네이티브 ABI에 맞는 안전한 반환과 사후 오류 보고를 제공한다.

## 플랫폼과 배포

| 환경 | 순서 | 현재 검증 |
| --- | --- | --- |
| Unity 6000.4.8f1 macOS arm64 Editor | 첫 네이티브 실행 대상 | 로컬 UPM 임포트, Box·Compound 예제 120 스텝과 자세 조회·정리 확인 |
| macOS arm64/x86_64 플레이어 | macOS 플러그인 설정·배포 확인 | universal 바이너리와 임포트 설정 준비; 플레이어 미실행 |
| Windows x86_64 Editor·플레이어 | 두 번째 네이티브 대상 | 미확인 |
| IL2CPP 플레이어 | 역호출과 스트리핑 확인 | 미확인 |
| Linux·Android·iOS·WebGL | 별도 요구와 배포 조건 확정 후 | 첫 범위에서 제외 |

원본 의존성 JoltPhysics.Native 1.1.0은 참조 기준이다. 포함한 바이너리는 고정한 joltc와 Jolt 5.6.0에서 직접 빌드했으며, 정밀도·오브젝트 레이어 폭·컴파일러·CPU 옵션과 해시는 `upstream.lock.json`에 기록했다. 다른 Unity 래퍼의 로딩 결과를 이 패키지의 검증으로 재사용하지 않는다. 결정성 빌드 옵션을 켰더라도 플랫폼 간 결정성이 확인된 것은 아니다.

플러그인은 OS·CPU별 경로와 PluginImporter 설정을 사용한다. 지원하지 않는 환경에는 설명 가능한 로딩 오류를 제공한다. 공개 배포 전에 Unity에서 생성한 `.meta`, 플러그인 설정, 라이선스와 해시를 보존한다. 최소 샘플은 GameObject 없이 월드와 바디를 생성하고 스텝 후 Vector3·Quaternion 자세를 조회하는 흐름을 보여야 한다.

## 구현 순서와 후속 검증

1. 패키지 구조, 원본 고정과 변경 기록을 준비한다. 기초 값 타입부터 Unity 문법·수학 타입으로 이식한다.
2. 단정밀도 네이티브 ABI와 로딩, 초기화·종료를 macOS arm64에서 연결한다.
3. 월드, Box·Compound와 바디 수명, 고정 스텝과 자세 조회를 구현한다.
4. 쿼리·필터·충돌 이벤트, 자원 정리를 구현한다.
5. 별도 Unity 검증 프로젝트에서 임포트, 씬 없는 실행, 수명 반복, 쿼리 반환형·필터와 이벤트를 확인한다. Windows와 IL2CPP 검증 후 지원 상태를 갱신한다.

검증 항목은 컴파일, 네이티브 ABI 대조, 실제 시뮬레이션과 플레이어 실행을 구분한다. 성능은 워밍업 이후 반복 스텝·자세 조회의 할당과 호출 비용을 측정한다. 허용 수치와 측정 조건은 구현 시 근거를 기록하며 현재 단계에서 성능 개선을 주장하지 않는다.

네트워크 입력 틱, 명령 대기 시간, 게임 규칙·프리팹 매핑은 호스트 게임의 책임이다. 플랫폼 간 결정성도 현재 지원 범위에 포함하지 않는다. 이후 락스텝을 요구한다면 고정 스텝·입력·생성 순서와 빌드 옵션을 고정하고, 대상 플랫폼 간 리플레이 상태 해시 비교를 별도 완료 조건으로 추가한다.

## 근거

- [고정한 원본 프로젝트](https://github.com/amerkoleci/JoltPhysicsSharp/tree/77a5be2dd30d587c1981dfcaf15851f18041b39c): 보관한 csproj·Directory.Build.props·JoltApi.cs가 원본 런타임·문법·interop 기준이다.
- [Unity 패키지 레이아웃](https://docs.unity3d.com/6000.4/Documentation/Manual/cus-layout.html): Runtime, Documentation~ 및 메타데이터 배치.
- [Unity .NET 프로파일](https://docs.unity3d.com/6000.4/Documentation/Manual/dotnet-profile-support.html): Unity의 .NET Standard 2.1 API 기준.
- [Unity 스크립팅 제한](https://docs.unity3d.com/6000.4/Documentation/Manual/scripting-restrictions.html): AOT 플랫폼의 정적 네이티브 역호출과 MonoPInvokeCallback 요구.
