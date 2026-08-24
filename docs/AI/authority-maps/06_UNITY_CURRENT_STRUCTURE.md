# Unity 현재 구성도

> 기준일: 2026-08-23
> Unity 프로젝트: `C:\Users\user\ssalddel`
> 증거 수준: Builder·정책·시험 소스 확인 완료. 이번 작성에서는 Unity Editor가 연결되지 않아 저장 Scene hierarchy와 Play Mode를 실시간 재확인하지 않았다.

## 공식 진입점

`SimulationWorldShell`은 유일한 canonical Play Scene이다. 검토·실험 Scene은 보존하되 Build Settings의 공식 진입점으로 늘리지 않는다.

```text
SimulationWorldShell
├─ ShellRuntimeRoot
│  ├─ SimulationWorldShellPresenter
│  ├─ SettlementInteraction 구성
│  ├─ LogisticsMovement 구성
│  ├─ TurnClosing 구성
│  ├─ CardDrawer 구성
│  ├─ JinbuInbound UI 구성
│  └─ Nature 생존 모듈(runtime 조립)
├─ WorldMapRoot
├─ SettlementInteriorRoot
├─ CameraSystem
│  └─ PlayerCameraRig
│     └─ CameraPivot
│        └─ Main Camera
├─ Lighting
├─ PersistentUI
│  └─ SimulationWorldHud
└─ EventSystem
```

초기에는 WorldMap이 활성이고 SettlementInterior는 비활성이다. 카메라에는 `DioramaTopDownCameraRig`와 전략 조작기가 연결된다.

## 조립 책임

| 구성 요소 | 실제 책임 |
| --- | --- |
| `SimulationWorldShellBuilder` | canonical root, 세계·정착지, 카메라, 조명, HUD와 runtime 기능 조립 |
| `통합SimulationWorldBuilder` | 기본 Shell 위에 대한민국 법정동 World, mode navigation, 통합 정책 조립·검증 |
| `통합WorldScenePolicy` | canonical Scene 경로와 Build Settings 단일 진입점 정책 |
| `SimulationWorldShellPresenter` | 서버·fixture 조회 결과를 Scene 표현으로 투영 |
| `PersistentUI` 계층 | HUD, 카드 서랍, 모드·입고 UI의 지속 표시 |

통합 Builder의 검증은 Player, inbound UI, mode presenter, 공식 projection, Farm 완료 영역, 하단 버튼, 최소 HUD 등 필수 배선을 검사한다.

## 권위 경계

```text
Unity 입력
 → AuthorityClient Preview
 → 플레이어 명시 Confirm
 → Simulation Server Command
 → 서버 Decision / Task / Effect / Tick / Revision
 → 재조회 projection
 → Unity 표현·피드백
```

Unity는 카메라·선택·이동 표현·UI를 소유하지만 Session의 최종 상태를 확정하지 않는다. `ReviewFixture`는 검토용이고 `Server`가 실제 E7 경로다. 서버 `SessionMode`의 `Solo`/`HostedMultiplayer`와 Unity의 `Server`/`ReviewFixture`를 하나의 enum처럼 섞지 않는다.

## 실시간과 WorldTick

```text
표현 실시간
  Unity Update / deltaTime
  → 이동·카메라·애니메이션·경로 표현
  → WorldTick·WorldRevision 불변

권위 실시간
  Nature SoloLocal 또는 Hosted 서버 시계
  → 정수 경과 초·CommandId·ExpectedRevision
  → 짧은 작업·주기 위상 갱신
  → 1,200초 주기 경계에서 WorldTick 진행
```

전투는 100ms `BattleTick`을 별도로 사용하고 결과를 이후 WorldTick에 합류시킨다. `WorldRevision`은 Tick뿐 아니라 Confirm과 권위 실시간 명령으로 상태가 바뀔 때도 증가할 수 있다. 상세 기준은 [WorldTick과 실시간 실행 경계](../../Architecture/WorldTick과실시간실행경계.md)를 따른다.

## H 공간 조립 위치

H1~H5 설계 재고는 Unity asset inventory이고, 저장 Scene의 runtime 배치는 공간 조립 호환 출력과 Builder가 결정적으로 조립해야 한다. 이 출력은 WI E5 판정의 조건부 입력이지 E5 완료 증거가 아니다.

```text
H 설계 카드
 → 승인·선택 H1~H4
 → AreaSet 공간 조립 호환 JSON
 → WorldComposition / Builder
 → 저장 SimulationWorldShell runtime root
 → 공간 검증 기록
```

H 설계 카드가 Approved여도 Scene에 배치됐다는 뜻이 아니며, Scene에 시각 객체가 있어도 WI 실행 문맥과 권위 전이·Task/Effect·결과·후속 선택이 닫히지 않으면 E5 세계 발현이 아니다.

## 현재 검증 상태

- 이전 작업 근거에는 최소 HUD의 실제 Play Mode·Game View 검증이 있다.
- Simulation 소스와 관련 시험은 별도 서버 저장소에서 검증된다.
- Nature 생존 모듈은 조립 코드와 Solo 로컬 엔진·서버 계약이 구현됐지만 저장 Scene, 실제 Play Mode 입력, Game View와 실제 서버 HTTP는 아직 검증되지 않았다.
- 이번 작성 시점에는 Editor/Pipeline 연결이 없어 현재 저장 hierarchy, Console, 실제 입력을 재확인하지 않았다.
- 따라서 이 파일은 **현재 조립 코드 지도**이며 새로운 E7 runtime 증거가 아니다.

## 다음 E7 조립 순서

1. Nature 생존 모듈을 저장 `SimulationWorldShell`에 조립하고 실제 입력 배선을 확인한다.
2. `SoloLocal`에서 도끼→벌목→오두막→황혼→후퇴·회복을 실행한다.
3. Hosted Simulation Server에서 같은 규칙의 Preview·Confirm·실시간 시계·주기 WorldTick·재조회를 실행한다.
4. Save 후 재실행·Replay Hash와 Solo/Hosted 결과의 결정성을 확인한다.
5. Game View, Console, 서버 응답과 저장 증거를 같은 실행 기록으로 남긴다.
