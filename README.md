# JoltPhysicsSharp for Unity

JoltPhysicsSharp를 Unity용 UPM 패키지로 이식한다. 위치·방향·회전의 기본 타입은 `UnityEngine.Vector3`와 `UnityEngine.Quaternion`이다. `MonoBehaviour`나 `GameObject` 없이 월드와 물리 바디를 생성하고, 호출자가 고정 시간 간격으로 시뮬레이션을 진행하는 API를 목표로 한다.

현재는 **macOS Editor에서 월드·바디를 실행할 수 있는 초기 구현 버전**이다. 초기화·종료, 테이블 기반 충돌 레이어, Box·StaticCompound 형상, 바디 생성·추가·제거·파괴, 키네마틱 이동, 속도 설정과 자세 조회를 제공한다. Unity 6000.4.8f1 arm64 Editor에서 씬 없는 상자·Compound 예제를 실행했다. 쿼리·접촉 이벤트와 Windows·IL2CPP 지원은 아직 완료되지 않았다.

## 패키지 경계

패키지는 초기화, 월드·바디·형상 수명 관리, 시뮬레이션, 자세 조회, 충돌 쿼리와 이벤트를 담당한다. 게임의 `Playfield`, `Piece`, 세션, 프리팹 매핑과 렌더링은 사용하는 프로젝트에서 구현한다. 원본의 `JoltPhysicsSharp` 네임스페이스와 API 의미를 가능한 한 유지한다.

개발 저장소는 [ciart/JoltPhysicsUnity](https://github.com/ciart/JoltPhysicsUnity)다. 저장소 루트가 UPM 패키지 루트이며, 네이티브 플러그인과 Unity 생성 메타데이터를 함께 보관한다. 공개 저장소이므로 다운로드에 GitHub 인증이 필요하지 않다.

Pentricat의 로컬 준비 위치는 `LocalPackages/com.ciart.joltphysics`다. 다른 프로젝트로 패키지 폴더 전체를 옮길 수도 있다. 현재 Pentricat에는 설치하지 않았다.

## 원본 기준

| 항목 | 고정 기준 |
| --- | --- |
| 원본 | [amerkoleci/JoltPhysicsSharp](https://github.com/amerkoleci/JoltPhysicsSharp) |
| 관리 코드 버전 | 2.22.0 |
| 커밋 | `77a5be2dd30d587c1981dfcaf15851f18041b39c` |
| 원본의 네이티브 의존성 | JoltPhysics.Native 1.1.0을 참조 기준으로 삼고, 고정한 joltc·Jolt 소스에서 직접 빌드 |
| 포함한 네이티브 라이브러리 | macOS arm64/x86_64 공용, 단정밀도, 오브젝트 레이어 32비트 |
| 개발 대상 | Unity 6000.4.8f1, .NET Standard 2.1, C# 9 문법을 기준으로 준비 |

원본 소스와 빌드 설정의 변경되지 않은 사본은 `Upstream~/JoltPhysicsSharp-2.22.0.tar.gz`에 보관한다. 출처와 해시는 [upstream.lock.json](upstream.lock.json)에 기록한다. `Upstream~`는 Unity가 임포트하지 않는 비교용 자료다.

네이티브 기준은 joltc 커밋 `886e088675bae3a086f8318c7803f8ee962c2f2c`와 Jolt 5.6.0 커밋 `e77f175595e64cb44218cc9d9d56fc365ad0e36a`다. C API 원본 사본, 실제 빌드 옵션과 배포 파일 해시도 고정했다.

## 문서와 작업 규칙

- [구현 요구사항](Documentation~/Implementation.md): 구현 범위, 순서, 완료 조건, 지원 플랫폼.
- [원본 대비 변경 기록](Documentation~/UpstreamChanges.md): 구현별 원본 파일, 변경 이유와 검증 결과.
- [작업 규칙](AGENTS.md): 코드 변경과 변경 기록을 함께 갱신하는 필수 제약.
- [버전 변경 기록](CHANGELOG.md): 패키지 버전별 변경 요약.
- [사용 계약과 네이티브 빌드](Documentation~/Usage.md): 초기화·소유권·스텝 호출과 빌드 재현 방법.

## 설치

원본 NuGet JoltPhysicsSharp와 동일한 타입 이름을 사용하므로, 설치 대상 프로젝트에서 원본 관리 DLL과 이 패키지를 동시에 참조하지 않도록 정리해야 한다. 현재 지원 확인 환경은 macOS arm64 Editor다.

Unity Package Manager에서 **Install package from git URL**을 선택하고 다음 URL을 입력한다. 설치할 컴퓨터에는 Git이 설치되어 있어야 한다.

```text
https://github.com/ciart/JoltPhysicsUnity.git
```

프로젝트에서 사용하는 소스를 고정하려면 URL 뒤에 `#<커밋 SHA>`를 붙인다. 태그와 GitHub Release는 아직 발행하지 않았으며, 패키지 버전은 `0.1.0-preview.1`이다. 상세 설치 방식은 [Unity의 Git URL 설치 안내](https://docs.unity3d.com/6000.4/Documentation/Manual/upm-ui-giturl.html)를 따른다.

로컬 개발 시에는 **Install package from disk**로 이 폴더의 `package.json`을 선택한다.

Unity에서 생성한 `.meta`와 macOS Editor·Standalone 플러그인 설정을 보존했다. Package Manager에서 **Data World** 샘플을 임포트하면 `JoltPhysicsSharp.Samples.BoxSimulation.Run()`으로 시뮬레이션 결과를 조회할 수 있다. 플레이어 실행, Intel Editor와 하위 Unity 버전 호환성은 아직 확인하지 않았다.

## 라이선스 고지

MIT 라이선스의 원저작자 고지와 전문을 `LICENSE.md`, `Native~/Licenses` 및 [제삼자 고지](Third%20Party%20Notices.md)에 보존한다. 이 패키지를 포함한 게임을 배포할 때도 해당 고지 전문을 배포물에 동봉한다.
