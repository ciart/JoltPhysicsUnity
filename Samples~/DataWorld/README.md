# 데이터 월드 예제

패키지와 이 샘플을 임포트한 뒤 `JoltPhysicsSharp.Samples.BoxSimulation.Run()`을 호출한다. 씬과 GameObject 없이 바닥, 상자와 두 자식 형상을 가진 Compound 바디를 생성하고 60 Hz로 120 스텝을 진행한다. 결과에는 두 바디의 `Vector3` 위치와 `Quaternion` 회전, Compound의 로컬 질량 중심이 들어 있다.

Compound의 자식은 Z축으로 90도 회전하며 서로 다른 X 오프셋을 가진다. 자식 Shape 래퍼를 Dispose한 뒤에도 Compound가 유지하는 네이티브 참조로 시뮬레이션한다. 바디 원점 위치와 로컬 질량 중심을 구분하는 사용 예이기도 하다.

런타임 객체는 생성 순서의 역순으로 Dispose하고 마지막에 Shutdown한다. 월드로 소유권을 넘긴 필터는 월드가 함께 파괴하므로, 그 이후 필터의 Dispose는 아무 작업도 하지 않는다. 형상과 BodyCreationSettings는 네이티브 참조 수를 사용하므로 바디 수명과 따로 Dispose할 수 있다.

반환된 자세를 프리팹에 적용하는 코드는 사용하는 게임에서 작성한다. 스텝 호출과 프레임 사이 보간도 호출자가 결정한다.
