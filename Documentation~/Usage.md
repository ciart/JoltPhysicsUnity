# 월드·바디 사용 계약과 네이티브 빌드

현재 패키지는 macOS arm64 Unity 6000.4.8f1 Editor에서 사용 예제 실행을 확인한 초기 버전이다. 공개 API 이름은 JoltPhysicsSharp를 따른다. 전체 원본 API를 제공하는 버전은 아니며 쿼리·접촉 이벤트는 후속 구현 대상이다.

## 초기화와 소유권

`Foundation.Init()`의 성공 호출마다 마지막에 `Foundation.Shutdown()`을 대응시킨다. 단정밀도만 지원한다. 마지막 Shutdown은 살아 있는 월드, 필터, 형상, 형상·바디 설정 또는 JobSystem이 있으면 예외를 발생시키며 초기화 상태를 유지한다. 먼저 해당 객체를 Dispose한 뒤 다시 종료할 수 있다.

충돌 테이블은 PairFilter의 충돌 조합 설정, BroadPhase의 모든 레이어 매핑, ObjectVsBroadPhase 필터 생성 순서로 만든다. ObjectVsBroadPhase 필터를 생성하면 기반 테이블의 구성이 고정된다. `PhysicsSystem` 생성 성공 시 세 필터의 네이티브 소유권을 월드로 넘긴다. 이 필터를 다른 월드에 재사용하거나 먼저 Dispose하면 예외가 발생한다. 월드 Dispose가 네이티브 필터를 함께 파괴한 뒤 해당 래퍼의 Dispose는 아무 작업도 하지 않는다. 생성에 실패한 경우 소유권을 넘기지 않은 필터는 호출자가 정리한다.

형상은 네이티브 참조 수를 사용한다. 바디나 Compound 설정이 형상을 참조한 뒤에는 호출자의 Shape 래퍼를 Dispose해도 그 참조가 유지된다. ShapeSettings·BodyCreationSettings 역시 따로 Dispose한다. StaticCompound 설정에서 형상을 생성한 뒤에는 자식을 추가할 수 없으며, 다른 구조가 필요하면 새로운 설정을 만든다.

`Body`는 월드가 관리하는 바디를 가리키는 조회 객체다. `BodyInterface.RemoveAndDestroyBody` 또는 제거 후 `DestroyBody`로 파괴한다. 파괴된 ID, 다른 월드의 Body 객체 및 종료된 월드의 BodyInterface 접근은 거부한다. ID는 월드 안에서만 의미가 있으므로 서로 다른 월드의 숫자 ID를 섞지 않는다. 공개 네이티브 Handle을 외부에서 파괴하면 이 수명 계약을 유지할 수 없다.

## 시뮬레이션과 자세

`PhysicsSystem.Update(deltaTime, collisionSteps, jobSystem)`은 호출자가 정한 고정 스텝으로 실행한다. 유한한 양수 deltaTime과 양수 collisionSteps를 요구한다. 반환하는 `PhysicsUpdateError`는 용량 초과를 나타내므로 확인해야 한다. 패키지가 프레임이나 프리팹을 자동으로 갱신하지 않는다.

Box 생성 인수는 각 축의 half extent이며 모두 양수여야 한다. convex radius는 0 이상이고 가장 작은 half extent 이하로 제한한다. Quaternion은 정규화된 값을 전달한다. Vector3와 Quaternion의 성분은 C API에 그대로 전달하며 자동 축 반전이나 회전 보정을 하지 않는다.

`GetPositionAndRotation`은 바디 원점 자세를 반환한다. `GetCenterOfMassPosition`은 월드 공간의 질량 중심이고, `Shape.CenterOfMass`는 로컬 질량 중심이다. Compound 자식 오프셋이 비대칭이면 이 값들은 서로 다를 수 있다. 프리팹 오프셋과 렌더링 보간은 호출자에서 처리한다.

`SetPositionAndRotation`은 자세를 직접 바꾸며, `MoveKinematic`은 키네마틱 바디에 목표 자세와 deltaTime을 전달해 이동 속도를 계산한다. 정적 바디에는 속도를 설정하지 않는다. 바디를 월드에 추가한 뒤에는 추가·제거 상태를 중복 변경하지 않는다.

현재 joltc의 기본 Update는 전역 TempAllocator를 공유한다. 모든 관리 API 호출과 Dispose를 공통 잠금으로 보호하므로 여러 월드를 병렬로 스텝하지 않는다. 네이티브 JobSystemThreadPool의 내부 작업은 병렬로 실행된다. 병렬 월드나 Unity Jobs 연동은 별도 구현과 확인이 필요하다.

## 예제 실행

Unity Package Manager에서 **Data World** 샘플을 임포트하고 `JoltPhysicsSharp.Samples.BoxSimulation.Run()`을 호출한다. 바닥, 동적 Box, 회전한 두 자식 형상의 Compound를 만들어 60 Hz로 120 스텝을 진행한다. 반환값에 위치·회전과 Compound의 로컬 질량 중심이 들어 있다. 씬 객체는 만들지 않는다.

실제 확인 환경에서는 Box 원점 Y가 약 0.480, Compound 원점 Y가 약 0.230, Compound 로컬 질량 중심 X가 0.125였다. [실행 결과](DataWorldResult.json)를 보존했다. 이 값은 해당 예제의 관찰 결과이며 모든 형상·빌드에 대한 보장 수치가 아니다.

## 네이티브 빌드 재현

고정한 [joltc 커밋](https://github.com/amerkoleci/joltc/tree/886e088675bae3a086f8318c7803f8ee962c2f2c)과 [Jolt 5.6.0 커밋](https://github.com/jrouwe/JoltPhysics/tree/e77f175595e64cb44218cc9d9d56fc365ad0e36a)을 각각 체크아웃한다. 패키지의 `Tools~/build_macos.py`에 두 소스 폴더, 빌드 폴더와 출력 폴더를 전달한다. 스크립트는 커밋과 원본 변경 여부를 확인하며 자동 다운로드하지 않는다.

```sh
python3 'Tools~/build_macos.py' \
  --joltc-source /path/to/joltc \
  --jolt-source /path/to/JoltPhysics \
  --build-dir /path/to/build \
  --output-dir /path/to/output
```

빌드 옵션은 lock 파일에서 읽는다. macOS arm64/x86_64, 단정밀도, 오브젝트 레이어 32비트, Distribution과 CPU 옵션을 고정한다. `Native~/abi_layout.c`는 C 헤더의 bool·벡터·회전·ID와 핵심 설정 배치를 컴파일 시 확인한다. 다시 빌드한 파일을 배포하려면 새 해시·컴파일러·빌드 결과와 변경 기록을 갱신해야 한다. 기존 플러그인의 Unity 생성 메타데이터는 보존한다.

최소 OS 빌드 타깃은 11.0이다. 이는 Mach-O에 기록한 배포 타깃이며 macOS 11이나 Intel 환경에서 실행 확인한 결과는 아니다. Windows 바이너리, 플레이어 빌드와 IL2CPP 역호출은 현재 지원 확인 범위에 포함하지 않는다.
