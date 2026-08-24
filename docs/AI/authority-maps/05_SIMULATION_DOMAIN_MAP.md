# Simulation Domain 실제 구조와 권위 소유 지도

> 기준일: 2026-08-23
> 이 문서는 개념 설계가 아니라 현재 `Ssalddel.Simulation.*` 코드의 소유 위치를 요약한다. 세부 계약은 [`simulation-unity.json`](../../../eng/work-areas/simulation-unity.json)과 [생성 코드 지도](../generated/simulation-unity-code-map.md)에서 소스로 이동해 확인한다.

## 실행 구조

```text
Simulation Server API
  → Session 생성·조회
  → Command + ExpectedRevision
  → Application 처리
  → 경영SimulationSessionAggregate
      ├─ Decision
      ├─ Task
      ├─ Effect
      ├─ WorldTick
      └─ Revision
  → Simulation 전용 저장소
  → Save Package / Command Log / Replay Hash
```

운영 업무 DB와 Simulation DB는 분리된다. Simulation 결과나 Unity 표현은 운영 주문·배차·정산의 완료 증거가 아니다.

## 최상위 Aggregate

`경영SimulationSessionAggregate`가 Session의 핵심 일관성 경계를 소유한다.

| 소유 상태 | 의미 |
| --- | --- |
| SessionStableId, ScenarioId·Version·Seed | 실행 정체성과 결정적 시작 조건 |
| RuleRevision | 적용 규칙 판본 |
| CurrentTick·WorldTick, DurationTicks | 큰 세계 시간과 실행 범위 |
| Faction, Territory, Settlement | 기본 세계·세력 상태 |
| Revision | 낙관적 동시성·Confirm 기준 |

기능별 partial 파일이 같은 Aggregate의 상태 전이와 규칙을 확장한다. 별도 클래스가 보인다고 별도 최종 권위로 해석하지 않는다.

## 주제별 실제 소유 위치

| 주제 | 현재 소유 위치 | 현재 가능한 것 | 아직 주장하지 않는 것 |
| --- | --- | --- | --- |
| Player | Session의 Actor·Faction·Control 관련 상태 | actor 행동·전투 제어·Command | 완성된 플레이어 생활 정체성 Aggregate |
| NPC | `SimulationNpcWorkforce`의 조직·NPC actor·역량·정책·배정·업무 기록 | 업무 후보·배정·역량·재고 연계 | 지속 욕구·기억·자율 생활 폐루프 E8 |
| Facility | `SimulationIntegratedWorld`의 정의·runtime 시설 | 상태·용량·제조·건설·수리 | Unity GameObject가 시설 권위라는 주장 |
| Lot / Cargo | 통합 actor·lot, 재고·예약·cargo movement | 생산 Lot, 적재·운송·수령 상태 | 운영 화물 원장과 동일하다는 주장 |
| Formation | 통합 세계의 formation·commitment, 전투 상태 | 편성·명령·전투 연결 | 표현 계층이 편성을 확정하는 것 |
| Area / Space | LH World Save/Replay의 AreaSet·layout provenance·delta와 공간 계약 | 결정적 배치 사본·변경 재생 | H 설계 카드 자체가 runtime 사실이라는 주장 |
| Threat / Recovery | local combat, injury·repair·world effect·Nature WI | 위협 관찰·전투·부상·복구 Effect | Farm 원인을 Nature가 대체하는 것 |
| Card | 역할·행동 선택 계약과 UI projection | 안정 카드 ID에 의한 선택·표현 | 카드 UI가 서버 결과 modifier를 계산하는 것 |

## 상태 묶음

`SimulationIntegratedWorld`가 현재 다음 사전을 Session 안에서 소유한다.

- 시설 정의와 runtime 시설
- 통합 actor와 Lot
- 제조법·청사진·제약
- 제조 작업과 건설 프로젝트
- 편성·commitment·부상·예약
- world effect의 대기·적용 receipt
- 수리 작업과 cargo movement

`SimulationNpcWorkforce`는 조직, NPC actor, capability grant, 정책, 배정, 실행 기록, 시설 재고를 소유한다. 이는 G3 구현 재료지만 E8 증거 자체는 아니다.

## 전투 경계

`SimulationBattleInstanceState`는 지역 전투의 actor·행동·control mode와 100ms `BattleTick`을 별도 상태 객체로 관리한다. 전투 결과는 다음 WorldTick의 Session 규칙과 Effect를 통해 세계 상태로 합류해야 하며, Unity 애니메이션이 피해나 승패를 확정하지 않는다.

## 시간과 개정 경계

`WorldRevision`은 Confirm·시계 명령·Tick 등 권위 상태가 바뀐 판본이고, `WorldTick`은 NPC·Task·생산·물류·사건 같은 시간 의존 세계 규칙을 진행하는 큰 경계다. 둘은 같은 값이나 같은 의미가 아니다.

기본 경영 Session은 명시적인 `TickAdvance`와 현재 `OneTickOneDay` 달력을 사용한다. `nature-survival.realtime.r1`은 정규화한 경과 초를 권위 명령으로 받아 1,200초 주기 경계에서만 큰 WorldTick을 진행한다. 전투는 별도 BattleTick에서 진행하고 완료 결과를 이후 WorldTick에 합류시킨다.

상세 기준은 [WorldTick과 실시간 실행 경계](../../Architecture/WorldTick과실시간실행경계.md)를 따른다.

## Save / Replay

```text
초기 Seed·RuleRevision
 + 순서가 보존된 Command Log
 + ExpectedRevision 검증
 + WorldTick
 → Save Package
 → Replay
 → Canonical State Hash 비교
```

현재 Nature 실시간 상태와 시계 명령까지 포함한 Save schema는 `simulation-save.v13`이다. `SimulationLhWorldSaveReplay`는 AreaSet·세계 배치의 출처·판본·delta를 별도로 보존한다. 새 변화는 기존 Save migration과 replay hash 회귀를 함께 다뤄야 E9 후보가 된다.

## Host mode와 권위 모드

일반 서버 SessionMode의 안정 값은 `Solo`와 `HostedMultiplayer`다. 서버 실행에서는 둘 다 Simulation Session이 최종 권위다.

Unity 쪽의 `Server`와 `ReviewFixture`는 일반 권위 클라이언트 선택이다. `ReviewFixture`는 검토·시험 근거이지 실제 플레이 권위가 아니다. Nature 기능 한정 `SoloLocal`은 이제 `Nature생존LocalEngine`이 공유 결정 규칙과 로컬 상태 개정을 소유하는 명시적 권위 모드다. 이 값을 일반 SessionMode 또는 다른 기능의 로컬 권위로 확대하지 않는다.

```text
일반 Solo 서버 플레이
 = SessionMode.Solo + Unity AuthorityClient.Server

Nature SoloLocal
 = Nature생존LocalEngine + 공유 결정 규칙 + 로컬 상태 사본

ReviewFixture
≠ SoloLocal
≠ E7 실제 서버 증거
```

## 변경 판단 규칙

1. 상태 소유자를 먼저 찾고 Command가 그 Aggregate를 통과하게 한다.
2. Preview는 후보만, Confirm은 안정 ID와 ExpectedRevision만 보낸다.
3. Unity Presentation은 projection·입력·피드백을 담당하고 WorldTick·Revision을 직접 바꾸지 않는다. 기능 한정 로컬 권위는 Nature `SoloLocal`처럼 모드·규칙·개정·저장 경계를 별도로 선언한다.
4. 새 Aggregate는 기존 Session 경계로 표현할 수 없는 독립 일관성·수명주기가 있을 때만 추가한다.
5. 저장 형식이나 Effect가 바뀌면 Save/Replay·결정성·이전 판본 migration 영향까지 추적한다.
