# 세계 상호작용 단위 대장

> 이 문서는 `eng/execution-ledgers/world-interactions.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 대장 개정: `simulation-world-interactions.r8`
- 증거 단계 개정: `simulation-evidence-stages.r10`
- WI 발생원 개정: `world-interaction-trigger-sources.r1`
- 마지막 확인일: `2026-08-24`
- 기본 구현 완료선: `E3 자동 시험 통과`
- 실제 공간·공공데이터·Unity 통합 목표선: `E7 실제 플레이 폐루프`
- 전체 항목: `49`

## 읽는 법

WI는 새 업무 엔티티가 아니라 발생원·행위자·대상·자료·자원·미리보기·확정·작업·효과·저장/재생을 관통하는 구현·검증 단위다. 자료를 읽은 사실만으로 WI가 되지 않으며 의미 있는 권위 상태 전이가 생길 때 WI가 된다. `Command`만 독립 확정을 가지며, `AutomaticTransition`은 부모 명령의 계보와 Tick으로 진행되고, `SharedPolicy`는 여러 WI가 함께 쓰는 판정 규칙이다.

## 분류 요약

| 분류 | 수 |
| --- | ---: |
| 명시적 명령 | 38 |
| 자동 상태 전이 | 10 |
| 공유 정책 | 1 |

## FARM 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-FARM-01` 밭갈기 | 명시적 명령 | PlayerDriven, NpcDriven | Untilled → Tilled | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-FARM-02` 파종 | 명시적 명령 | PlayerDriven, NpcDriven | Tilled → Growing | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-FARM-03` 관수·재배 관리 | 명시적 명령 | PlayerDriven, NpcDriven | Growing → Growing, HarvestReady | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-FARM-04` 수확 | 명시적 명령 | PlayerDriven, NpcDriven | HarvestReady → Harvested, HarvestedAtField | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-FARM-05` 수확물 집하 | 명시적 명령 | PlayerDriven, NpcDriven | HarvestedAtField → CollectedAtYard | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-FARM-06` 출하 준비·포장 | 명시적 명령 | PlayerDriven, NpcDriven | CollectedAtYard → PackedForShipment, PreparedForShipment | 완료 · `E3→E3` | 진행 중 · `E6→E7` |

## HUB 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-001` 진부 Hub 입고검수 | 명시적 명령 | PlayerDriven, NpcDriven | ArrivedAtDestination, PendingInspection → StorageEligible, Received | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-002` 진부 Hub 창고 적재 | 명시적 명령 | PlayerDriven, NpcDriven | StorageEligible → PutAwayCompleted | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-HUB-03` 출고 요청 | 명시적 명령 | PlayerDriven, NpcDriven | PutAwayCompleted → OutboundRequested | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-HUB-04` 피킹 | 자동 상태 전이 | WorldDerived | OutboundRequested → Picked | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-HUB-05` 출고 준비 | 자동 상태 전이 | WorldDerived | Picked → OutboundReady | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-HUB-06` Hub 차량 상차 | 명시적 명령 | PlayerDriven, NpcDriven | OutboundReady → Reserved | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## LOG 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-LOG-01` 차량 상차 확정 | 명시적 명령 | PlayerDriven, NpcDriven | PreparedForShipment → Reserved | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-LOG-02` Farm 출발 | 자동 상태 전이 | WorldDerived | Reserved → InTransit | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| `WI-LOG-03` Farm→Hub 화물 이동 | 자동 상태 전이 | WorldDerived | InTransit → InTransit, ArrivedAtDestination | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-LOG-04` Hub 하차 | 자동 상태 전이 | WorldDerived | InTransit → ArrivedAtDestination | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-LOG-05` Hub 인수 | 자동 상태 전이 | WorldDerived | ArrivedAtDestination, PendingInspection → Received, StorageEligible | 완료 · `E3→E3` | 진행 중 · `E4→E7` |

## MARKET 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-MARKET-01` Hub→마트 운송 | 명시적 명령 | PlayerDriven, NpcDriven | Reserved → ArrivedAtDestination | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-MARKET-02` 마트 하차·인수 | 명시적 명령 | PlayerDriven, NpcDriven | ArrivedAtDestination → MarketReceived | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-MARKET-03` 마트 입고검수 | 명시적 명령 | PlayerDriven, NpcDriven | MarketReceived → MarketStorageEligible | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-MARKET-04` 마트 후방 적재 | 명시적 명령 | PlayerDriven, NpcDriven | MarketStorageEligible → MarketBackroomStored | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-MARKET-05` 진열 보충 | 명시적 명령 | PlayerDriven, NpcDriven | MarketBackroomStored → Displayed | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## NATURE 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-NATURE-01` 자연권 위협 관찰 | 명시적 명령 | PlayerDriven, NpcDriven, WorldDerived | Stable, Warning, Threatened, Infested → ThreatObserved | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-NATURE-02` 자연권 긴급 후퇴 | 명시적 명령 | PlayerDriven, NpcDriven | ThreatObserved, EncounterActive → RetreatedToSafeCore | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-NATURE-03` 자연권 복원 | 명시적 명령 | PlayerDriven, NpcDriven | ThreatObserved, CauseResolved → NatureRouteRestored | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-NATURE-04` 파티 회복 | 명시적 명령 | PlayerDriven, NpcDriven | RetreatedToSafeCore, NatureRouteRestored → PartyRecovered | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-NATURE-05` 도끼 획득 | 명시적 명령 | PlayerDriven, NpcDriven | AxeAvailable, PlayerWithoutAxe → AxeOwned | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-06` 벌목 시작 | 명시적 명령 | PlayerDriven, NpcDriven | AxeOwned, TreeStanding, PlayerIdle → HarvestWorkScheduled | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-07` 오두막 위치 배치 | 명시적 명령 | PlayerDriven, NpcDriven | CabinPlanned, BuildingSiteAvailable → CabinBlueprintPlaced | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-08` 오두막 건설 시작 | 명시적 명령 | PlayerDriven, NpcDriven | CabinBlueprintPlaced, TimberAvailable, PlayerIdle → CabinBuildScheduled | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-09` 오두막 입장 | 명시적 명령 | PlayerDriven, NpcDriven | CabinOperational, PlayerOutsideCabin → PlayerInsideCabin | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-10` 오두막 퇴장 | 명시적 명령 | PlayerDriven, NpcDriven | PlayerInsideCabin → PlayerOutsideCabin | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-11` 황혼 조우 대응 | 명시적 명령 | PlayerDriven, NpcDriven | EncounterPending → EncounterResolved, BattleHandoffRequested, PlayerRetreated | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| `WI-NATURE-12` 진행 작업 취소 | 명시적 명령 | PlayerDriven, NpcDriven | WorkActive → WorkCancelled, SafeChoiceAvailable | 완료 · `E3→E3` | 진행 중 · `E4→E7` |

## ORDER 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-ORDER-01` 주문 확정 | 명시적 명령 | PlayerDriven, NpcDriven | DemandCandidate → OrderConfirmed | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-ORDER-02` 주문 재고 예약 | 자동 상태 전이 | WorldDerived | OrderConfirmed → StockReserved | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-ORDER-03` 주문 피킹 | 자동 상태 전이 | WorldDerived | StockReserved → Picked | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-ORDER-04` 주문 포장 | 자동 상태 전이 | WorldDerived | Picked → Packed | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-ORDER-05` 수령 준비 | 자동 상태 전이 | WorldDerived | Packed → ReadyForPickup | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-ORDER-06` 주민 수령 | 명시적 명령 | PlayerDriven, NpcDriven | ReadyForPickup → Fulfilled | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-ORDER-07` 주민 소비 | 명시적 명령 | PlayerDriven, NpcDriven | Fulfilled → Consumed | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## WORLD 작업군

| WI | 종류 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | --- | --- | --- | --- | --- |
| `WI-WORLD-01` NPC 작업 배정 | 공유 정책 | NpcDriven, WorldDerived | Available → Assigned | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-02` NPC 역량 위임 | 명시적 명령 | PlayerDriven, NpcDriven | NotGranted → Granted | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-03` 작업 취소 | 명시적 명령 | PlayerDriven, NpcDriven | Scheduled, Blocked → Cancelled | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-04` 시설 수리 | 명시적 명령 | PlayerDriven, NpcDriven | Damaged → Repaired | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-05` 지역 발견 | 명시적 명령 | PlayerDriven, NpcDriven, WorldDerived | Undiscovered → Discovered | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-06` 역할 카드 장착 | 명시적 명령 | PlayerDriven, NpcDriven | Unequipped → Equipped | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-07` 활동 시작·종료 | 명시적 명령 | PlayerDriven, NpcDriven | Available, Active → Active, Completed | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| `WI-WORLD-08` 턴 마감 | 명시적 명령 | PlayerDriven, NpcDriven, WorldDerived | TurnOpen → TurnClosed | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## 첫 WI 실행 문맥·세계 발현 공급선

```text
WI-FARM-04 수확 (100㎡ × 3kg/㎡ = 300kg)
→ WI-FARM-05 집하
→ WI-FARM-06 출하 준비·포장
→ WI-LOG-01 상차 확정
→ WI-LOG-02 출발 [자동]
→ WI-LOG-03 Farm→Hub 이동 [자동]
→ WI-LOG-04 Hub 하차 [자동]
→ WI-LOG-05 Hub 인수 [WI-001 안의 자동 전이]
→ WI-001 입고검수
→ WI-002 창고 적재
```

## 증거 경계

- E3는 계약·코드·자동 시험의 구현 완료선이다.
- Scenario 공간으로 통과한 E3는 실제 LandscapeGraph 또는 공공 공간자료 증거가 아니다.
- E4는 WI의 허용 발생원·주체·대상·자료·자원·시간과 공간 적용 여부가 결속되는 단계다.
- E5는 실제 Simulation 세계에서 WI가 발생해 권위 상태·Task·Effect·결과·후속 경로로 발현되는 단계다.
- H는 공간 포함 계층이며 공간 WI의 E4·E5 입력 증거다. AreaSet·Graph가 존재해도 WI 발현이 없으면 E5가 아니다.
- 실제 서버와 저장 Scene에서 사람이 조작한 Play Mode·Game View·Console 증거가 있어야 E7이다.
- Unity 애니메이션이나 GameObject 상태가 Task 완료를 확정하지 않는다.
