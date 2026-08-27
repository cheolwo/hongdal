# 게임 기획 주도 H 공간 재고와 우선순위

H 재고는 게임 기획 묶음에 속해야 하며, WI 또는 예상 플레이와 연결되지 않은 카드는 공식 승격하지 않는다.

## 감사 결과

- 상호작용 H1: 52장
- 팩 표현 H1: 32장
- H2/H3/H4: 38/20/6
- 맥락·계보 위반: 0건
- 게임 기획 연결 전 격리된 팩 표현 H1: 9장

## 게임 기획 묶음

| 순위 | 게임 기획 | 핵심 동사 | H1/H2/H3/H4 범위 |
| --- | --- | --- | --- |
| P1 | Nature 생활·탐험 AreaSet (NatureHomeThreatRecovery) | Explore → AcquireTool → Harvest → PlaceShelter → BuildShelter → EnterShelter → RespondEncounter → ObserveThreat → Retreat → Restore → Recover | 16/11/5/2 |
| P2 | Farm 생산·생존 AreaSet (FarmProductionSurvival) | Cultivate → Harvest → ContainIncident → RecoverLoss → Ship | 17/10/4/1 |
| P3 | Town 생활·시장 AreaSet (TownLivingMarketSafety) | ReceiveGoods → Display → Order → Pack → Pickup → RelieveResidents | 22/12/6/1 |
| P4 | City/Hub 물류 AreaSet (CityHubLogisticsResilience) | Unload → Inspect → Store → Pick → Stage → Load | 10/9/4/1 |

## H 확장 순서

1. **H-P0 맥락 누락과 필수 공간 누락 제거** — 고아 H를 격리하고 공간 필수 WI에 빠진 H1을 먼저 보완한다.
1. **H-P1 Nature 상시 생활·위협·회복 폐루프** — 플레이어가 가장 오래 머무는 자연권의 탐색·위협·후퇴·복원 공간을 우선 완결한다.
1. **H-P2 Farm 생산·사건 대응·출하** — 재배에서 사건 격리와 출하까지 이어지는 생산 공간을 완결한다.
1. **H-P3 Town 생활·시장 안전** — 입고·진열·주문·포장·수령과 오염 대응 공간을 완결한다.
1. **H-P4 City/Hub 물류 회복력** — 입고·검수·보관·피킹·출고와 품질 대응 공간을 완결한다.

## 게임플레이 기반 H2·H3 수요

| 순위 | 게임플레이 수요 | 종류 | 신규 H2/H3 | 상태 |
| --- | --- | --- | --- | --- |
| P1 | City/Hub 피킹·출고준비 공간 분리 | `SpatialInventoryGap` | h2-candidate:hub-fulfillment, h3-candidate:hub-fulfillment-operations | `SatisfiedByTheoryInventory` |
| P2 | Town 마트 후방 입고·검수 공간 분리 | `SpatialInventoryGap` | h2-candidate:town-market-receiving, h3-candidate:town-market-fulfillment | `SatisfiedByTheoryInventory` |
| P3 | Town 주문 피킹·포장·수령 공간 분리 | `SpatialInventoryGap` | h2-candidate:town-order-fulfillment, h3-candidate:town-market-fulfillment | `SatisfiedByTheoryInventory` |
| P4 | Farm–Hub–Town 화물 인과선 의미 폐쇄 | `Reuse` | h3-candidate:hub-fulfillment-operations, h3-candidate:town-market-fulfillment | `SatisfiedByTheoryInventory` |
| P5 | Nature 위협·후퇴·회복 의미 개정 | `RevisionExpansion` | h3-candidate:nature-threat-recovery | `Queued` |

## 플레이 가능한 완성 단위

H 조립 상태, 게임플레이 추적, E 증거와 사람이 완주하는 완성 상태는 서로 독립이다.

| 기준 플레이 | 게임 기획 | 현재 | 목표 | 이론 공간 | 실제 공간 |
| --- | --- | --- | --- | --- | --- |
| Nature↔Farm 수확과 회복의 하루 (`reference-play:nature-farm-day.v1`) | NatureHomeThreatRecovery, FarmProductionSurvival | `SpatiallyComposed` | `PlayableSliceComplete` | `E5TheoryQualified` | `ActualE5Bound` |

- 아직 기준 플레이가 없는 경고 전용 기획: CityHubLogisticsResilience, TownLivingMarketSafety

## WI의 E 채움 순서

1. **E-P1 Farm 수확·출하 공간 실행** — `Integration` 현재 E6 → 목표 E6
1. **E-P2 Hub 입고·검수·보관 공간 실행** — `Integration` 현재 E4 → 목표 E5
1. **E-P3 Town 입고·주문·수령 공간 실행** — `Integration` 현재 E1 → 목표 E4
1. **E-P4 Nature 생활·탐사·사건 공간 모판 재검증** — `Integration` 현재 E1, E4, E6, E7 → 목표 E7
1. **E-P5 선정 WI 공공데이터 계보** — `Integration` 현재 선정 뒤 판정 → 목표 E6

## 경계

- 맥락 없는 H는 삭제하지 않고 IdeaInventory 격리 대상으로 보고한다.
- 팩 표현 카드는 상호작용 H1과 게임 기획에 연결되기 전에는 공식 작업공간이 아니다.
- 실제 AreaSet 배치는 E5, 필요한 공공데이터 계보는 E6에서만 수행한다.
