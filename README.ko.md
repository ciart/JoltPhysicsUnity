# JoltPhysicsUnity

[English](README.md) | 한국어

[JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp)를 Unity용 UPM 패키지로 포팅한 프로젝트입니다. `UnityEngine.Vector3`와 `UnityEngine.Quaternion`을 사용하며, 씬 객체 없이 C# 데이터에서 물리 월드를 생성하고 스텝을 진행할 수 있습니다.

**이 프로젝트의 Unity 포팅 작업은 OpenAI GPT(Codex)가 수행했습니다.** 원본 물리 엔진과 래퍼의 출처·저작권 고지는 그대로 보존합니다.

현재 버전은 **`0.1.0-preview.1`**입니다. 월드·바디의 핵심 기능을 구현한 개발 단계이며, 원본 API 전체를 제공하지는 않습니다.

## 설치

설치할 컴퓨터에 Git이 필요합니다. 기존 NuGet `JoltPhysicsSharp`를 사용 중이라면 먼저 제거해야 합니다. 두 패키지는 같은 네임스페이스와 타입 이름을 사용합니다.

Unity Package Manager에서 **＋ → Install package from git URL**을 선택하고 다음 URL을 입력합니다.

```text
https://github.com/ciart/JoltPhysicsUnity.git
```

특정 소스로 고정하려면 URL 뒤에 `#<커밋 SHA>`를 붙입니다. 로컬 개발 시에는 **Install package from disk**에서 저장소 루트의 `package.json`을 선택합니다. [Unity 설치 안내](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-ui-giturl.html)

## 샘플

Package Manager에서 **Data World** 샘플을 임포트한 뒤, C# 메서드에서 다음 코드를 호출합니다.

```csharp
var result = JoltPhysicsSharp.Samples.BoxSimulation.Run(steps: 120);
UnityEngine.Debug.Log($"Box 위치: {result.Position}, Compound 위치: {result.CompoundPosition}");
```

샘플은 바닥과 동적 Box·Compound를 만들고 60 Hz로 120 스텝을 실행합니다. 초기화부터 자원 정리까지 포함하며, 씬에 GameObject를 만들지 않습니다. 결과에는 위치·회전과 Compound의 로컬 질량 중심이 들어 있습니다.

직접 월드를 구성하는 코드는 [샘플 소스](Samples~/DataWorld/BoxSimulation.cs)를, 초기화·소유권·고정 스텝 계약은 [사용 가이드](Documentation~/Usage.md)를 참고하세요. 프리팹 생성과 자세 적용은 사용하는 게임에서 처리합니다.

## 구현 범위

현재 제공하는 기능:

- 런타임 초기화·종료, 네이티브 자원의 `Dispose`와 수명 관리
- 테이블 기반 충돌 레이어, 물리 월드, 중력과 고정 스텝
- Box·StaticCompound 형상과 바디 생성·추가·제거·파괴
- 바디 자세·질량 중심 조회, 속도 설정과 키네마틱 이동

RayCast·CollideShape·CastShape, 접촉·활성화 이벤트, 질량 설정과 행렬 API는 아직 구현하지 않았습니다. 상세 상태는 [구현 요구사항](Documentation~/Implementation.md)에 기록합니다.

## 실행 확인 환경

개발 기준은 Unity **6000.4.8f1**, .NET Standard 2.1, C# 9입니다. 포함된 네이티브 라이브러리는 단정밀도이며, macOS arm64/x86_64 공용 바이너리입니다.

| 환경 | 현재 상태 |
| --- | --- |
| macOS arm64 Editor | 로컬 UPM 임포트, Box·Compound 120 스텝과 자세 조회·자원 정리 확인 |
| macOS Intel Editor·플레이어 | 바이너리 포함, 실행 미확인 |
| macOS arm64 플레이어 | 바이너리 포함, 실행 미확인 |
| Windows·Linux·모바일·WebGL | 해당 플랫폼 바이너리 미포함 |
| IL2CPP | 미확인 |

[예제 실행 결과](Documentation~/DataWorldResult.json)를 보존합니다. 플랫폼 간 결정성과 성능은 아직 확인하지 않았습니다.

## 원본과 변경 기록

| 구성 | 원본 기준 |
| --- | --- |
| C# 래퍼 | [JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp) 2.22.0 |
| 네이티브 C API | [joltc](https://github.com/amerkoleci/joltc) |
| 물리 엔진 | [Jolt Physics](https://github.com/jrouwe/JoltPhysics) 5.6.0 |

네이티브 플러그인은 고정한 joltc·Jolt 소스에서 직접 빌드했습니다. 정확한 커밋·빌드 옵션·SHA-256은 [upstream.lock.json](upstream.lock.json)에, 원본 대비 수정 이유와 검증 결과는 [변경 기록](Documentation~/UpstreamChanges.md)에 보관합니다. 원본 관리 코드와 C API 소스 사본은 `Upstream~`에 있습니다.

- [변경 이력](CHANGELOG.md)
- [네이티브 빌드 재현](Documentation~/Usage.md#네이티브-빌드-재현)
- [개발 작업 규칙](AGENTS.md)

## 라이선스

MIT 라이선스입니다. 원본 고지 전문은 [LICENSE.md](LICENSE.md), [joltc 라이선스](Native~/Licenses/Joltc.txt), [Jolt Physics 라이선스](Native~/Licenses/JoltPhysics.txt)에 보존합니다. 이 패키지를 포함한 배포물에도 해당 고지 전문을 동봉해야 합니다. [제삼자 고지](Third%20Party%20Notices.md)
