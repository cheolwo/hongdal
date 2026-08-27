# WorldTick과 실시간 실행 경계

> 상태: `Accepted`
> 기준일: 2026-08-23
> 적용 범위: `Ssalddel.Simulation.*`, Unity `SimulationWorldShell`, Nature 생존, 현장 전투

## 한 줄 기준

Ssalddel은 **이동·카메라·애니메이션과 짧은 작업 과정은 실시간으로 보여 주고, 시간에 따른 세계의 큰 결과는 WorldTick 경계에서 결정적으로 정리한다.** 다만 모든 권위 변경이 WorldTick에서만 일어나는 것은 아니다. Confirm, 실시간 시계 명령과 전투 명령도 상태를 바꿀 수 있으며, 그 변경 여부는 `WorldRevision`으로 구분한다.

```text
실시간 입력·경과
  → 표현만 갱신하거나
  → 권위 명령으로 정규화
  → Confirm / 실시간 시계 / BattleTick
  → 필요한 세계 결과를 WorldTick에 합류
  → canonical 상태 재조회
  → Unity 표현 갱신
```

## 네 가지 시간·변경 축

| 축 | 현재 단위 | 소유자 | 바꾸는 것 | 바꾸지 않는 것 |
| --- | --- | --- | --- | --- |
| Unity 표현 시간 | 프레임과 `Time.deltaTime` | Unity Presentation | 위치 표현, 카메라, 애니메이션, 입력 피드백 | 서버 `WorldTick`, `WorldRevision`, 재고·Task·Effect |
| 권위 실시간 시계 | 정수 경과 초 명령 | Nature `SoloLocal` 엔진 또는 Simulation Session | 작업 진행 초, 주기 위상, 조우 후보, 상태 개정 | 벽시계 timestamp, 종료 중 따라잡기 |
| WorldTick | 명시적 Tick 수 또는 완료된 Nature 주기 수 | `경영SimulationSessionAggregate` | NPC 업무, Task, 생산·물류, 사건, 생존, 통합 세계의 시간 의존 결과 | 프레임별 이동·카메라 |
| BattleTick | 현재 100ms 전투 박자 | `SimulationBattleInstanceState` | 전투 배치·지원 도착·전투 진행과 결과 후보 | 즉시 세계 재고·시설·지역 결과 확정 |

`실시간`은 하나의 권위 계층 이름이 아니다. 화면이 부드럽게 움직이는 표현 시간일 수도 있고, Nature처럼 검증된 경과 시간을 권위 명령으로 보내는 실행 시간일 수도 있다.

## WorldTick과 WorldRevision은 다르다

현재 기본 Session은 다음 계약으로 Tick을 진행한다.

```text
경영SimulationTick진행Request
├─ CommandId
├─ ExpectedRevision
└─ TickCount

검증
  → CurrentTick += TickCount
  → NPC·Decision/Task·Farm 생존·지역 사건·통합 세계 계산
  → Revision += 1
  → Command log와 상태 사본 저장
```

현재 기본 달력 규칙은 `OneTickOneDay`이므로 `GameDate = GameDateStartsOn + CurrentTick 일`이다. 이는 현재 경영 Session의 달력 Profile이지, WorldTick이라는 개념이 항상 현실 1일 또는 현실 1초라는 뜻은 아니다.

다음과 같은 권위 변경은 WorldTick을 올리지 않고도 `WorldRevision`을 올릴 수 있다.

- Preview 뒤 Confirm으로 Decision·예약·작업을 생성한다.
- Nature에서 도끼를 얻거나 작업을 시작한다.
- Nature 실시간 시계가 작업 진행 초와 낮·황혼·밤 위상을 갱신한다.
- 별도 전투 원장에서 배치·전술 명령을 확정한다.

따라서 현재 프로젝트의 정확한 표현은 다음과 같다.

> `WorldRevision`은 권위 상태가 바뀐 판본이고, `WorldTick`은 시간 의존 세계 규칙을 진행하는 큰 경계다.

## 일반 경영 세계의 흐름

감자 상차를 예로 들면 다음과 같다.

```text
플레이어가 창고까지 실시간 이동
  → 상차 Preview: 후보만 계산, 상태 불변
  → Confirm: Lot·작업·수량 예약, Revision 증가 가능
  → WorldTick: 상차 Task 완료와 재고·Cargo·작업자 결과 반영
  → Session 재조회
  → Unity 차량·상자·HUD 표현 갱신
```

차량이나 NPC가 목적지에 도착해 보이는 것만으로 상차·운송·인수 완료가 되지 않는다. 서버가 반환한 Tick·Task·Effect와 최신 상태 사본이 완료 근거다.

## Nature 생존의 권위 실시간 시계

`nature-survival.realtime.r1`은 기본 경영 Tick에 실시간 주기 입력을 연결한 현재의 명시적 예외다.

| 항목 | 현재 계약 |
| --- | --- |
| 한 주기 | 실제 경과 1,200초 |
| 서버 입력 | `ElapsedRealtimeSeconds` 0~60, `CommandId`, `ExpectedRevision` |
| 짧은 작업 | 벌목 4초, 오두막 건설 30초 |
| WorldTick 진행 | 완료된 1,200초 주기 수만큼 `AdvanceWorldState` 호출 |
| Solo 일시정지 | 메뉴·응용프로그램 비활성 사유일 때 정지 |
| Hosted | 개별 클라이언트 메뉴와 무관하게 서버 시계 진행 |
| 종료 중 경과 | 저장하지 않고 따라잡지 않음 |

Unity `Nature생존Controller`는 `Time.unscaledDeltaTime`을 1초 단위로 모아 `Nature생존CoreRuntimeAdapter`에 전달한다. Solo에서는 Unity 프로세스 안의 공통 `LocalSimulationRuntime`이 Session Aggregate와 Nature 상태의 권위다. 기존 `Nature생존LocalEngine`은 호환 시험·규칙 대조용으로 남는다. Hosted에서는 같은 Application·Domain을 실행하는 Simulation 서버의 `POST .../nature-survival/clock/advance`가 권위다.

실시간 1초가 들어올 때마다 일반 WorldTick이 오르는 것은 아니다. 작업 진행, 위상과 `WorldRevision`은 주기 중에도 바뀔 수 있지만, 큰 세계 시간은 1,200초 주기 경계를 넘을 때만 1 Tick 진행한다.

이 기능의 상세 규칙과 현재 증거는 [Nature 생존 생활거점 세로 조각](Nature생존생활거점세로조각.md)을 따른다.

## 전투의 BattleTick

현장 전투는 일반 WorldTick을 프레임마다 올리지 않는다. 현재 전투 계약은 `CombatStepMilliseconds = 100`인 별도 `BattleTick`과 별도 전투 개정을 사용한다.

```text
World 상태 사본·대상 예약
  → 전투 생성 Confirm
  → 100ms BattleTick 단위 전투 진행
  → 전투 결과 Completed
  → 다음 WorldTick에서 Reconcile
  → 예약 해제·Effect 적용·세계 상태 재조회
```

전투 중 플레이어 조작과 부대 표현은 빠르게 갱신할 수 있지만, 최종 피해·예약 해제·세계 Effect는 전투 결과가 World Session에 합류한 뒤 권위가 된다.

## Unity `SimulationWorldShell`의 역할

Unity의 다음 요소는 기본적으로 표현 전용 실시간 계층이다.

- 플레이어 1인칭·3인칭 이동
- 전략 카메라 이동·회전·확대
- 선택 원·목적지 표식·드래그 입력
- NPC와 차량의 경로 표현
- 걷기·작업·전투 애니메이션
- 실시간 입력 피드백과 HUD 전환
- 공간 Streaming·LOD·가시성 갱신

실제 PlayMode 시험도 플레이어 이동 전후 `SimulationWorldShellPresenter.WorldTick`과 `WorldRevision`이 유지되는지를 검사한다.

Unity 프로세스 안에서 상태 권위를 실행해야 하는 경우에도 단순 `Update()` 부수효과로 처리하지 않는다. Nature Solo의 `LocalSimulationRuntime`처럼 권위 위치, 공유 Application·Domain, 상태 사본, 개정, 저장·재생 경계를 명시해야 한다. 실행 위치의 전체 기준은 [Solo 우선 Simulation Runtime](SoloFirstSimulationRuntime.md)을 따른다.

## Save / Replay 원칙

| 입력 종류 | 저장·재생 기준 |
| --- | --- |
| 카메라 흔들림·보간 프레임 | 일반적으로 저장하지 않음 |
| 플레이어 표현 위치 | 공간·게임 규칙상 필요할 때 별도 계약으로 저장 |
| Confirm | 안정 ID·ExpectedRevision·CommandId를 Command log에 보존 |
| Tick 진행 | `TickCount`와 적용 순서를 보존 |
| Nature 실시간 | 정규화된 정수 초, 작업 입력, 일시정지 사유를 보존 |
| BattleTick | 전투 명령·박자·전투 개정과 결과를 전투 재생 경계에 보존 |

프레임 수나 장치 성능이 달라도 같은 정규화 명령과 규칙 판본이 같은 canonical 결과를 만들어야 한다. 현재 Nature 상태와 시계 명령은 `simulation-save.v13`에 포함된다.

## E7~E9에서의 판정

```text
E7 플레이어
실시간 이동·입력
  → Preview·Confirm 또는 권위 실시간 명령
  → WorldTick·BattleTick 결과
  → 서버/로컬 권위 상태 재조회
  → Game View에서 같은 결과 확인

NPC PlayableUnit E1~E7
목표·행동 후보 선택
  → 실시간 이동 표현
  → 권위 작업·자원 예약
  → WorldTick 결과와 다음 목표 갱신
  → Save/Replay 뒤에도 생활 연속성 유지

E8 개별 안정
같은 폐루프를 결정적으로 반복
  → Save/Restore/Replay와 Local/Remote 비교
  → 실제 입력·Scene 재진입 확인

E9 영역 조화
안정 Core 둘 이상을 이어 플레이
  → 시간·자원·공간·회복·조건부 NPC 인계 확인
  → 사람 평가와 후보 승인
```

NPC가 걷는 애니메이션만 반복하면 NPC PlayableUnit의 E7이나 E9 NPC 연속성 증거가 아니다. 목표 선택, 공간·자원 경합, Task·Effect, 다음 행동과 기억이 권위 상태로 이어져야 한다.

## 구현 판단 규칙

새 시간 기반 기능은 구현 전에 다음을 정한다.

1. 이것은 부드러운 표현인가, 권위 상태 변화인가.
2. 권위 변화라면 `WorldRevision`, WorldTick, BattleTick 중 어느 경계를 사용하는가.
3. 실시간 경과를 받는다면 초를 어떻게 정규화하고 한 요청의 최대 범위를 얼마로 제한하는가.
4. Solo 일시정지, Hosted 지속, 종료 중 따라잡기를 어떻게 처리하는가.
5. Save/Replay에 프레임이 아니라 어떤 명령 입력을 남기는가.
6. Unity가 서버 결과를 재계산하지 않고 어떤 canonical 상태를 재조회하는가.

이 여섯 항목이 정해지지 않은 상태에서 `Update()`나 타이머가 재고·성장·작업 완료·피해·World Effect를 직접 확정하게 만들지 않는다.

## 용어 구분

- **실시간 실행**: 경과 시간과 입력을 따라 계속 진행되는 게임 과정.
- **실시간 표현**: Unity 프레임에서만 움직이는 카메라·위치·애니메이션.
- **권위 실시간 시계**: 정규화된 경과 시간을 Command로 받아 상태를 변경하는 시계.
- **WorldTick**: 시간 의존 세계 규칙을 묶어 진행하는 큰 Simulation 경계.
- **BattleTick**: 전투 안에서만 사용하는 빠른 결정적 박자.
- **WorldRevision**: WorldTick 여부와 관계없이 권위 상태가 바뀔 때 증가하는 판본.
- **실시간 통신**: SignalR 등 상태 전달 방식으로, 위 실행 시간축과는 별개다.
- **실시간 공공데이터**: 자료 최신성을 뜻하며 게임 시계 권위와는 별개다.
