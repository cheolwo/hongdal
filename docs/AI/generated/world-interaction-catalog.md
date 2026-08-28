# 세계 상호작용 단위 대장

> 이 문서는 `eng/execution-ledgers/world-interactions.json`와 참조된 단일 책임·조립 흐름·음양 사분면 대장에서 자동 생성된다. 직접 수정하지 않는다.

- 대장 개정: `simulation-world-interactions.r31`
- 증거 단계 개정: `simulation-evidence-stages.r13`
- WI 발생원 개정: `world-interaction-trigger-sources.r7`
- WI 단일 책임 개정: `simulation-world-interaction-responsibilities.r5`
- WI 조립 흐름 개정: `simulation-world-interaction-flows.r3`
- WI 음양·수행주체 사분면 개정: `world-interaction-polarity-quadrants.r5`
- 마지막 확인일: `2026-08-28`
- 기본 구현 완료선: `E3 자동 시험 통과`
- 실제 공간·공공데이터·Unity 통합 목표선: `E7 실제 플레이 폐루프`
- 전체 항목: `66`

## 읽는 법

WI는 한 행위자의 한 의도와 하나의 주요 권위 결과를 관통하는 구현·검증 단위다. 플레이어와 NPC가 같은 의도·결과를 만들면 같은 WI를 사용할 수 있으며 행위자 종류는 실행 문맥에 결속한다. Preview·Confirm·Task·Effect는 별도 절차 WI가 아니라 같은 책임의 실행 생명주기다. 여러 WI의 순서·분기·반복은 별도 조립 흐름 대장이 소유하고 WI의 정체성이나 필수 선행 조건이 되지 않는다.

음양은 행동 목적, 두 번째 부호는 실제 Actor를 뜻한다. `++`·`+-`·`-+`·`--`는 선악이나 배율이 아니며 실행 인스턴스의 설명 좌표다. 순수 자동 전이는 사분면 밖에 두고 Contextual WI는 승인 PlayableLoop 문맥이 있을 때만 Yang 또는 Yin으로 고정한다.

`sequence`는 대장 안에서 찾기 위한 순번일 뿐 게임 실행 단계가 아니다. 효과가 여러 개여도 하나의 주요 결과를 원자적으로 지키는 부수 효과라면 한 WI로 유지할 수 있다. 서로 독립적으로 실패·취소·재시도할 수 있는 복합 책임은 `복합 책임·분리 필요`로 차단한다. 독립 의도가 없는 자동 절차는 Task/Effect로 내리고, 실제 피킹·포장처럼 Actor가 수행하는 일은 NPC 행동 책임으로 전환한다.

## 분류 요약

| 분류 | 수 |
| --- | ---: |
| 명시적 명령 | 54 |
| 자동 상태 전이 | 11 |
| 공유 정책 | 1 |

## 행위자 공통 물품·장착 작업군 (`ACTOR`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 물품 획득 · `WI-ACTOR-01` | 1 | 행위자 의도 | 단일 책임 | 양(陽) | `ItemAcquired` | PlayerOrNpc | PlayerDriven, NpcDriven | WorldItemAvailable, ActorDoesNotOwnItem → ItemOwnedInInventory | 완료 · `E3→E3` | 진행 중 · `E5→E7` |
| 장착 상태 변경 · `WI-ACTOR-02` | 2 | 행위자 의도 | 단일 책임 | 음(陰) | `ItemEquipmentChanged` | PlayerOrNpc | PlayerDriven, NpcDriven | ItemOwnedInInventory, ItemEquipped → EquipmentStateChanged | 완료 · `E3→E3` | 진행 중 · `E5→E7` |
| 지식 습득 · `WI-ACTOR-03` | 3 | 행위자 의도 | 단일 책임 | 음(陰) | `RecipeKnowledgeAdded` | PlayerDirect | PlayerDriven, NpcDriven | ReadableKnowledgeSourceAvailable, RecipeNotKnown → RecipeKnown | 완료 · `E3→E3` | 진행 중 · `E5→E7` |

## 메이저 아르카나 작업군 (`CARD`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 현재 세계의 메이저 아르카나 활성화 · `WI-CARD-01` | 1 | 행위자 의도 | 원자적 부수 효과 | 음(陰) | `MajorArcanaActivated` | PlayerDirect | PlayerDriven | MajorArcanaChoiceAvailable → MajorArcanaActivationFrozen, TownLifeChoiceAvailable | 완료 · `E3→E3` | 진행 중 · `E4→E7` |

## 도심 운영 작업군 (`CITY`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 도심 서비스 수요 확정 · `WI-CITY-01` | 1 | 행위자 의도 | 단일 책임 | 음(陰) | `CityDemandConfirmed` | NpcRoutine | PlayerDriven, NpcDriven | CityDemandChoiceAvailable → CityDemandConfirmed | 미착수 · `E1→E3` | 미선정 · `E1→E7` |
| 도심 서비스용 지역 재고 배정 · `WI-CITY-02` | 2 | 권위 상태 전이 | 배타적 결과 묶음 | 사분면 제외 | `CityInventoryAllocated` | WorldAutomatic | WorldDerived | CityDemandConfirmed → CityInventoryAllocated, CityInventoryShortage | 미착수 · `E1→E3` | 미선정 · `E1→E7` |
| 도심 주민 서비스 처리 · `WI-CITY-03` | 3 | 행위자 의도 | 배타적 결과 묶음 | 음(陰) | `CityServiceCompleted` | NpcRoutine | PlayerDriven, NpcDriven | CityInventoryAllocated, CityInventoryShortage → CityServiceCompleted, CityServiceDeferred | 미착수 · `E1→E3` | 미선정 · `E1→E7` |
| 도심 서비스 결과 확인 · `WI-CITY-04` | 4 | 행위자 의도 | 단일 책임 | 음(陰) | `CityServiceChoiceAvailable` | NpcRoutine | PlayerDriven, NpcDriven | CityServiceCompleted, CityServiceDeferred → CityServiceChoiceAvailable | 미착수 · `E1→E3` | 미선정 · `E1→E7` |

## 영역 건설 작업군 (`CON`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 영역 건물 건설 확정 · `WI-CON-01` | 1 | 행위자 의도 | 단일 책임 | 실행 문맥 판정 | `AreaBuildingOperational` | PlayerDirect | PlayerDriven, NpcDriven | Available → Building, Operational | 완료 · `E3→E3` | 완료 · `E7→E7` |

## 농장 생산 작업군 (`FARM`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 경작지 밭갈이 · `WI-FARM-01` | 1 | 행위자 의도 | 단일 책임 | 양(陽) | `SoilTilled` | PlayerOrNpc | PlayerDriven, NpcDriven | Untilled → Tilled | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 경작지 씨앗 파종 · `WI-FARM-02` | 2 | 행위자 의도 | 단일 책임 | 양(陽) | `CultivationStarted` | PlayerOrNpc | PlayerDriven, NpcDriven | Tilled → Growing | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 농작물 생육 관리 · `WI-FARM-03` | 3 | 행위자 의도 | 단일 책임 | 양(陽) | `CropCareApplied` | PlayerOrNpc | PlayerDriven, NpcDriven | Growing → Growing, HarvestReady | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 익은 농작물 수확 · `WI-FARM-04` | 4 | 행위자 의도 | 원자적 부수 효과 | 양(陽) | `HarvestLotCreated` | PlayerOrNpc | PlayerDriven, NpcDriven | HarvestReady → Harvested, HarvestedAtField | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 수확물 집하장 모으기 · `WI-FARM-05` | 5 | 행위자 의도 | 단일 책임 | 양(陽) | `HarvestLotCollected` | PlayerOrNpc | PlayerDriven, NpcDriven | HarvestedAtField → CollectedAtYard | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 출하 물량 포장 · `WI-FARM-06` | 6 | 행위자 의도 | 원자적 부수 효과 | 양(陽) | `PackageLotCreated` | PlayerOrNpc | PlayerDriven, NpcDriven | CollectedAtYard → PackedForShipment, PreparedForShipment | 완료 · `E3→E3` | 진행 중 · `E6→E7` |

## 물류 거점 창고 작업군 (`HUB`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 입고 화물 검수 · `WI-001` | 1 | 행위자 의도 | 복합 책임·분리 필요 | 음(陰) | `StorageEligible` | NpcRoutine | PlayerDriven, NpcDriven | ArrivedAtDestination, PendingInspection → StorageEligible, Received | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 검수 완료 화물 창고 적재 · `WI-002` | 2 | 행위자 의도 | 원자적 부수 효과 | 양(陽) | `PutAwayCompleted` | NpcRoutine | PlayerDriven, NpcDriven | StorageEligible → PutAwayCompleted | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 출고 대상 재고 요청 · `WI-HUB-03` | 3 | 행위자 의도 | 단일 책임 | 음(陰) | `OutboundRequested` | NpcRoutine | PlayerDriven, NpcDriven | PutAwayCompleted → OutboundRequested | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 출고 대상 재고 피킹 · `WI-HUB-04` | 4 | 권위 상태 전이 | 실제 Actor 행동으로 전환 필요 | 양(陽) | `StockPicked` | WorldAutomatic | WorldDerived | OutboundRequested → Picked | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 피킹 화물 포장 · `WI-HUB-05` | 5 | 권위 상태 전이 | 실제 Actor 행동으로 전환 필요 | 양(陽) | `OutboundCargoPrepared` | WorldAutomatic | WorldDerived | Picked → OutboundReady | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 출고 차량 상차 · `WI-HUB-06` | 6 | 행위자 의도 | 단일 책임 | 양(陽) | `HubCargoLoaded` | NpcRoutine | PlayerDriven, NpcDriven | OutboundReady → Reserved | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## 영역 간 화물 이동 작업군 (`LOG`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 출하 차량 상차 확정 · `WI-LOG-01` | 1 | 행위자 의도 | 단일 책임 | 양(陽) | `CargoTransportReserved` | NpcRoutine | PlayerDriven, NpcDriven | PreparedForShipment → Reserved | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 농장에서 출발 · `WI-LOG-02` | 2 | 권위 상태 전이 | 절차 단계·Task/Effect로 이동 필요 | 사분면 제외 | `CargoDeparted` | WorldAutomatic | WorldDerived | Reserved → InTransit | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 농장에서 물류 거점으로 화물 이동 · `WI-LOG-03` | 3 | 권위 상태 전이 | 절차 단계·Task/Effect로 이동 필요 | 사분면 제외 | `CargoRouteProgressed` | WorldAutomatic | WorldDerived | InTransit → InTransit, ArrivedAtDestination | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 물류 거점 도착 화물 하차 · `WI-LOG-04` | 4 | 권위 상태 전이 | 절차 단계·Task/Effect로 이동 필요 | 사분면 제외 | `CargoUnloaded` | WorldAutomatic | WorldDerived | InTransit → ArrivedAtDestination | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 물류 거점 도착 화물 인수 · `WI-LOG-05` | 5 | 권위 상태 전이 | 절차 단계·Task/Effect로 이동 필요 | 사분면 제외 | `FreightReceived` | WorldAutomatic | WorldDerived | ArrivedAtDestination, PendingInspection → Received, StorageEligible | 완료 · `E3→E3` | 진행 중 · `E4→E7` |

## 마트 입고·진열 작업군 (`MARKET`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 물류 거점에서 마트로 운송 · `WI-MARKET-01` | 1 | 행위자 의도 | 단일 책임 | 양(陽) | `MarketCargoArrived` | NpcRoutine | PlayerDriven, NpcDriven | Reserved → ArrivedAtDestination | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 마트 도착 화물 인수 · `WI-MARKET-02` | 2 | 행위자 의도 | 단일 책임 | 음(陰) | `MarketFreightReceived` | NpcRoutine | PlayerDriven, NpcDriven | ArrivedAtDestination → MarketReceived | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 마트 입고 상품 검수 · `WI-MARKET-03` | 3 | 행위자 의도 | 단일 책임 | 음(陰) | `MarketStorageEligible` | NpcRoutine | PlayerDriven, NpcDriven | MarketReceived → MarketStorageEligible | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 검수 상품 후방 창고 적재 · `WI-MARKET-04` | 4 | 행위자 의도 | 단일 책임 | 양(陽) | `MarketBackroomStored` | NpcRoutine | PlayerDriven, NpcDriven | MarketStorageEligible → MarketBackroomStored | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 매장 진열대 상품 보충 · `WI-MARKET-05` | 5 | 행위자 의도 | 단일 책임 | 양(陽) | `DisplayStockReplenished` | NpcRoutine | PlayerDriven, NpcDriven | MarketBackroomStored → Displayed | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## 자연 탐사·생활 거점 작업군 (`NATURE`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 자연 지역 위험 징후 확인 · `WI-NATURE-01` | 1 | 행위자 의도 | 단일 책임 | 양(陽) | `NatureThreatObserved` | PlayerDirect | PlayerDriven, NpcDriven, WorldDerived | Stable, Warning, Threatened, Infested → ThreatObserved | 완료 · `E3→E3` | 완료 · `E7→E7` |
| 안전 거점으로 긴급 후퇴 · `WI-NATURE-02` | 2 | 행위자 의도 | 단일 책임 | 양(陽) | `PartyRetreatedToSafeCore` | PlayerDirect | PlayerDriven, NpcDriven | ThreatObserved, EncounterActive → RetreatedToSafeCore | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 훼손된 자연 경로 복원 · `WI-NATURE-03` | 3 | 행위자 의도 | 단일 책임 | 실행 문맥 판정 | `NatureRouteRestored` | PlayerDirect | PlayerDriven, NpcDriven | ThreatObserved, CauseResolved → NatureRouteRestored | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 탐사대 안전 회복 · `WI-NATURE-04` | 4 | 행위자 의도 | 단일 책임 | 음(陰) | `PartyRecovered` | PlayerDirect | PlayerDriven, NpcDriven | RetreatedToSafeCore, NatureRouteRestored → PartyRecovered | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 벌목 도끼 획득 · `WI-NATURE-05` | 5 | 행위자 의도 | 단일 책임 | 양(陽) | `AxeAcquired` | PlayerDirect | PlayerDriven, NpcDriven | AxeAvailable, PlayerWithoutAxe → AxeOwnedInInventory | 완료 · `E3→E3` | 완료 · `E7→E7` |
| 나무 벌목 작업 시작 · `WI-NATURE-06` | 6 | 행위자 의도 | 원자적 부수 효과 | 양(陽) | `TreeFelled` | PlayerDirect | PlayerDriven, NpcDriven | WoodcuttingCapabilityEquipped, TreeStanding, PlayerIdle → HarvestWorkScheduled | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 오두막을 지을 터 선정 · `WI-NATURE-07` | 7 | 행위자 의도 | 단일 책임 | 실행 문맥 판정 | `CabinBlueprintPlaced` | PlayerDirect | PlayerDriven, NpcDriven | CabinPlanned, BuildingSiteAvailable → CabinBlueprintPlaced | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 오두막 건설 작업 시작 · `WI-NATURE-08` | 8 | 행위자 의도 | 원자적 부수 효과 | 실행 문맥 판정 | `CabinOperational` | PlayerDirect | PlayerDriven, NpcDriven | CabinBlueprintPlaced, TimberAvailable, PlayerIdle → CabinBuildScheduled | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 오두막 안으로 들어가기 · `WI-NATURE-09` | 9 | 행위자 의도 | 단일 책임 | 양(陽) | `PlayerEnteredCabin` | PlayerDirect | PlayerDriven, NpcDriven | CabinOperational, PlayerOutsideCabin → PlayerInsideCabin | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 오두막 밖으로 나가기 · `WI-NATURE-10` | 10 | 행위자 의도 | 단일 책임 | 양(陽) | `PlayerLeftCabin` | PlayerDirect | PlayerDriven, NpcDriven | PlayerInsideCabin → PlayerOutsideCabin | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 황혼 위협 대응 방식 확정 · `WI-NATURE-11` | 11 | 행위자 의도 | 복합 책임·분리 필요 | 양(陽) | `EncounterResolved` | PlayerDirect | PlayerDriven, NpcDriven | EncounterPending, CombatActive → EncounterResolved, BattleHandoffRequested, PlayerRetreated, PlayerDefeated | 완료 · `E3→E3` | 완료 · `E7→E7` |
| 진행 중 작업 취소 · `WI-NATURE-12` | 12 | 행위자 의도 | 원자적 부수 효과 | 음(陰) | `WorkCancelled` | PlayerDirect | PlayerDriven, NpcDriven | WorkActive → WorkCancelled, SafeChoiceAvailable | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 획득 자원 거점 보관 · `WI-NATURE-13` | 13 | 행위자 의도 | 원자적 부수 효과 | 음(陰) | `TimberStored` | PlayerDirect | PlayerDriven, NpcDriven | CabinOperational, PlayerInsideCabin, TimberCarried → TimberStored | 완료 · `E3→E3` | 완료 · `E7→E7` |
| 오두막에서 수면·새벽 맞기 · `WI-NATURE-14` | 14 | 행위자 의도 | 복합 책임·분리 필요 | 음(陰) | `DawnReached` | PlayerDirect | PlayerDriven, NpcDriven | Night, PlayerInsideCabin, EncounterResolved → Sleeping, DawnReached | 완료 · `E3→E3` | 진행 중 · `E5→E7` |
| 다음 날 거점 확장 계획 선택 · `WI-NATURE-15` | 15 | 행위자 의도 | 원자적 부수 효과 | 음(陰) | `ExpansionPlanSelected` | PlayerDirect | PlayerDriven, NpcDriven | Dawn, PlanUnselected → Day2Ready, ExpansionPlanSelected | 완료 · `E3→E3` | 진행 중 · `E6→E7` |
| 현장 보급 꾸러미 제작 · `WI-NATURE-16` | 16 | 행위자 의도 | 복합 책임·분리 필요 | 실행 문맥 판정 | `NatureFieldSupplyPackAdded` | PlayerDirect | PlayerDriven, NpcDriven | Day2Ready, NatureWorkbenchOperational, PlayerInsideCabin → NatureFieldSupplyPackAdded, FieldExpeditionChoiceAvailable | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 현장 보급 제작 업무 위임 · `WI-NATURE-17` | 17 | 행위자 의도 | 복합 책임·분리 필요 | 음(陰) | `NpcFieldSupplyPolicySelected` | NpcRoutine | PlayerDriven, NpcDriven | Day2Ready, NatureWorkbenchOperational, NpcFieldSupplyPolicyEnabled → NatureFieldSupplyPackAdded, FieldExpeditionChoiceAvailable | 완료 · `E3→E3` | 진행 중 · `E4→E7` |
| 벌목 통나무 줍기 · `WI-NATURE-18` | 18 | 행위자 의도 | 원자적 부수 효과 | 양(陽) | `TimberCollected` | PlayerDirect | PlayerDriven, NpcDriven | DroppedTimberAvailable, InventoryCapacityAvailable → DroppedTimberCollected, TimberCarried | 완료 · `E3→E3` | 완료 · `E7→E7` |

## 주민 주문·소비 작업군 (`ORDER`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 주민 주문 확정 · `WI-ORDER-01` | 1 | 행위자 의도 | 단일 책임 | 음(陰) | `OrderConfirmed` | NpcRoutine | PlayerDriven, NpcDriven | DemandCandidate → OrderConfirmed | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 주문 상품 재고 예약 · `WI-ORDER-02` | 2 | 권위 상태 전이 | 절차 단계·Task/Effect로 이동 필요 | 사분면 제외 | `OrderStockReserved` | WorldAutomatic | WorldDerived | OrderConfirmed → StockReserved | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 주문 상품 피킹 · `WI-ORDER-03` | 3 | 권위 상태 전이 | 실제 Actor 행동으로 전환 필요 | 양(陽) | `OrderStockPicked` | WorldAutomatic | WorldDerived | StockReserved → Picked | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 주문 상품 포장 · `WI-ORDER-04` | 4 | 권위 상태 전이 | 실제 Actor 행동으로 전환 필요 | 양(陽) | `OrderPacked` | WorldAutomatic | WorldDerived | Picked → Packed | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 주문 상품 수령 준비 · `WI-ORDER-05` | 5 | 권위 상태 전이 | 절차 단계·Task/Effect로 이동 필요 | 사분면 제외 | `OrderReadyForPickup` | WorldAutomatic | WorldDerived | Packed → ReadyForPickup | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 주민 주문 상품 수령 · `WI-ORDER-06` | 6 | 행위자 의도 | 단일 책임 | 양(陽) | `OrderFulfilled` | NpcRoutine | PlayerDriven, NpcDriven | ReadyForPickup → Fulfilled | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 주민 상품 소비 · `WI-ORDER-07` | 7 | 행위자 의도 | 단일 책임 | 음(陰) | `ResidentConsumed` | NpcRoutine | PlayerDriven, NpcDriven | Fulfilled → Consumed | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## 거점 성찰 작업군 (`REFLECT`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 승인 자료로 거점 성찰 확정 · `WI-REFLECT-01` | 1 | 행위자 의도 | 단일 책임 | 음(陰) | `InnerLearningPending` | PlayerDirect | PlayerDriven | ReturnedToBase, NatureSafeChoiceAvailable, ReflectionChoiceAvailable → InnerLearningPending | 완료 · `E3→E3` | 미선정 · `E3→E7` |

## 업무 검토 작업군 (`REVIEW`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| NPC 업무 결과 검토 확정 · `WI-REVIEW-01` | 1 | 행위자 의도 | 원자적 부수 효과 | 음(陰) | `NpcWorkReviewConfirmed` | PlayerDirect | PlayerDriven | NpcWorkCompleted, ReviewPending → NpcWorkReviewConfirmed | 진행 중 · `E2→E3` | 미선정 · `E1→E7` |

## 공통 세계 운영 작업군 (`WORLD`)

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 책임 종류 | 단일 책임 판정 | 음양 정의 | 주요 결과 | 조작 정책 | 허용 발생원 | 시작 → 완료 | 구현 | 통합 |
| --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| NPC에게 반복 업무 배정 · `WI-WORLD-01` | 1 | 공유 판정 규칙 | 단일 책임 | 사분면 제외 | `NpcAssigned` | WorldAutomatic | NpcDriven, WorldDerived | Available → Assigned | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| NPC에게 업무 역량 위임 · `WI-WORLD-02` | 2 | 행위자 의도 | 단일 책임 | 음(陰) | `NpcCapabilityGranted` | NpcRoutine | PlayerDriven, NpcDriven | NotGranted → Granted | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 진행 중 세계 업무 취소 · `WI-WORLD-03` | 3 | 행위자 의도 | 원자적 부수 효과 | 음(陰) | `TaskCancelled` | PlayerDirect | PlayerDriven, NpcDriven | Scheduled, Blocked → Cancelled | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 손상된 시설 수리 · `WI-WORLD-04` | 4 | 행위자 의도 | 단일 책임 | 실행 문맥 판정 | `FacilityRepaired` | PlayerOrNpc | PlayerDriven, NpcDriven | Damaged → Repaired | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 새로운 지역 발견 · `WI-WORLD-05` | 5 | 행위자 의도 | 단일 책임 | 양(陽) | `RegionDiscovered` | PlayerDirect | PlayerDriven, NpcDriven, WorldDerived | Undiscovered → Discovered | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 일행 역할 카드 장착 · `WI-WORLD-06` | 6 | 행위자 의도 | 단일 책임 | 음(陰) | `TeamRoleCardEquipped` | PlayerDirect | PlayerDriven, NpcDriven | Unequipped → Equipped | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 세계 활동 상태 변경 · `WI-WORLD-07` | 7 | 행위자 의도 | 복합 책임·분리 필요 | 음(陰) | `TeamActivityStarted` | PlayerDirect | PlayerDriven, NpcDriven | Available, Active → Active, Completed | 완료 · `E3→E3` | 미선정 · `E1→E7` |
| 하루 운영 턴 마감 · `WI-WORLD-08` | 8 | 행위자 의도 | 단일 책임 | 음(陰) | `TurnClosed` | PlayerDirect | PlayerDriven, NpcDriven, WorldDerived | TurnOpen → TurnClosed | 완료 · `E3→E3` | 미선정 · `E1→E7` |

## WI 조립 흐름

아래 연결은 WI 정의가 소유하는 필수 절차가 아니다. 독립 행동을 특정 플레이 폐루프에서 조립하는 선택 가능한 흐름이며, 같은 WI는 다른 흐름에도 참여할 수 있다.

### 농장 내부 생산 흐름 (`wi-flow:farm-production-internal.r1`)

- 흐름 종류: 독립 영역 흐름
- 경작지 밭갈이 (`WI-FARM-01`) → 경작지 씨앗 파종 (`WI-FARM-02`)
- 경작지 씨앗 파종 (`WI-FARM-02`) → 농작물 생육 관리 (`WI-FARM-03`)
- 농작물 생육 관리 (`WI-FARM-03`) → 익은 농작물 수확 (`WI-FARM-04`)
- 익은 농작물 수확 (`WI-FARM-04`) → 수확물 집하장 모으기 (`WI-FARM-05`)
- 수확물 집하장 모으기 (`WI-FARM-05`) → 출하 물량 포장 (`WI-FARM-06`)

### 농장과 물류 거점 선택 운송 흐름 (`wi-flow:farm-hub-transport-optional.r1`)

- 흐름 종류: 선택형 영역 연결
- 출하 물량 포장 (`WI-FARM-06`) → 출하 차량 상차 확정 (`WI-LOG-01`)
- 출하 차량 상차 확정 (`WI-LOG-01`) → 농장에서 출발 (`WI-LOG-02`)
- 농장에서 출발 (`WI-LOG-02`) → 농장에서 물류 거점으로 화물 이동 (`WI-LOG-03`)
- 농장에서 물류 거점으로 화물 이동 (`WI-LOG-03`) → 물류 거점 도착 화물 하차 (`WI-LOG-04`)
- 물류 거점 도착 화물 하차 (`WI-LOG-04`) → 물류 거점 도착 화물 인수 (`WI-LOG-05`)
- 물류 거점 도착 화물 인수 (`WI-LOG-05`) → 입고 화물 검수 (`WI-001`)

### 물류 거점 내부 창고 흐름 (`wi-flow:hub-warehouse-internal.r1`)

- 흐름 종류: 독립 영역 흐름
- 입고 화물 검수 (`WI-001`) → 검수 완료 화물 창고 적재 (`WI-002`)
- 검수 완료 화물 창고 적재 (`WI-002`) → 출고 대상 재고 요청 (`WI-HUB-03`)
- 출고 대상 재고 요청 (`WI-HUB-03`) → 출고 대상 재고 피킹 (`WI-HUB-04`)
- 출고 대상 재고 피킹 (`WI-HUB-04`) → 피킹 화물 포장 (`WI-HUB-05`)

### 물류 거점과 마트 선택 연결 흐름 (`wi-flow:hub-market-connector-optional.r1`)

- 흐름 종류: 선택형 영역 연결
- 피킹 화물 포장 (`WI-HUB-05`) → 출고 차량 상차 (`WI-HUB-06`)
- 출고 차량 상차 (`WI-HUB-06`) → 물류 거점에서 마트로 운송 (`WI-MARKET-01`)

### 마트 내부 입고·진열 흐름 (`wi-flow:market-stock-internal.r1`)

- 흐름 종류: 독립 영역 흐름
- 물류 거점에서 마트로 운송 (`WI-MARKET-01`) → 마트 도착 화물 인수 (`WI-MARKET-02`)
- 마트 도착 화물 인수 (`WI-MARKET-02`) → 마트 입고 상품 검수 (`WI-MARKET-03`)
- 마트 입고 상품 검수 (`WI-MARKET-03`) → 검수 상품 후방 창고 적재 (`WI-MARKET-04`)
- 검수 상품 후방 창고 적재 (`WI-MARKET-04`) → 매장 진열대 상품 보충 (`WI-MARKET-05`)

### 마트 진열과 주민 주문 선택 연결 흐름 (`wi-flow:market-order-connector-optional.r1`)

- 흐름 종류: 선택형 영역 연결
- 매장 진열대 상품 보충 (`WI-MARKET-05`) → 주민 주문 확정 (`WI-ORDER-01`)

### 주민 주문·소비 내부 흐름 (`wi-flow:resident-order-consumption.r1`)

- 흐름 종류: 독립 영역 흐름
- 주민 주문 확정 (`WI-ORDER-01`) → 주문 상품 재고 예약 (`WI-ORDER-02`)
- 주문 상품 재고 예약 (`WI-ORDER-02`) → 주문 상품 피킹 (`WI-ORDER-03`)
- 주문 상품 피킹 (`WI-ORDER-03`) → 주문 상품 포장 (`WI-ORDER-04`)
- 주문 상품 포장 (`WI-ORDER-04`) → 주문 상품 수령 준비 (`WI-ORDER-05`)
- 주문 상품 수령 준비 (`WI-ORDER-05`) → 주민 주문 상품 수령 (`WI-ORDER-06`)
- 주민 주문 상품 수령 (`WI-ORDER-06`) → 주민 상품 소비 (`WI-ORDER-07`)

### 자연 위협 관찰·대응·회복 흐름 (`wi-flow:nature-threat-recovery.r1`)

- 흐름 종류: 분기·반복 영역 흐름
- 자연 지역 위험 징후 확인 (`WI-NATURE-01`) → 안전 거점으로 긴급 후퇴 (`WI-NATURE-02`)
- 자연 지역 위험 징후 확인 (`WI-NATURE-01`) → 훼손된 자연 경로 복원 (`WI-NATURE-03`)
- 자연 지역 위험 징후 확인 (`WI-NATURE-01`) → 황혼 위협 대응 방식 확정 (`WI-NATURE-11`)
- 안전 거점으로 긴급 후퇴 (`WI-NATURE-02`) → 탐사대 안전 회복 (`WI-NATURE-04`)
- 훼손된 자연 경로 복원 (`WI-NATURE-03`) → 탐사대 안전 회복 (`WI-NATURE-04`)
- 황혼 위협 대응 방식 확정 (`WI-NATURE-11`) → 탐사대 안전 회복 (`WI-NATURE-04`)

### 자연 생활 거점 생존 흐름 (`wi-flow:nature-home-survival.r1`)

- 흐름 종류: 분기·반복 영역 흐름
- 벌목 도끼 획득 (`WI-NATURE-05`) → 장착 상태 변경 (`WI-ACTOR-02`)
- 장착 상태 변경 (`WI-ACTOR-02`) → 나무 벌목 작업 시작 (`WI-NATURE-06`)
- 나무 벌목 작업 시작 (`WI-NATURE-06`) → 벌목 통나무 줍기 (`WI-NATURE-18`)
- 나무 벌목 작업 시작 (`WI-NATURE-06`) → 나무 벌목 작업 시작 (`WI-NATURE-06`)
- 나무 벌목 작업 시작 (`WI-NATURE-06`) → 오두막을 지을 터 선정 (`WI-NATURE-07`)
- 나무 벌목 작업 시작 (`WI-NATURE-06`) → 진행 중 작업 취소 (`WI-NATURE-12`)
- 오두막을 지을 터 선정 (`WI-NATURE-07`) → 오두막 건설 작업 시작 (`WI-NATURE-08`)
- 오두막 건설 작업 시작 (`WI-NATURE-08`) → 오두막 안으로 들어가기 (`WI-NATURE-09`)
- 오두막 건설 작업 시작 (`WI-NATURE-08`) → 진행 중 작업 취소 (`WI-NATURE-12`)
- 오두막 건설 작업 시작 (`WI-NATURE-08`) → 획득 자원 거점 보관 (`WI-NATURE-13`)
- 오두막 안으로 들어가기 (`WI-NATURE-09`) → 오두막 밖으로 나가기 (`WI-NATURE-10`)
- 오두막 안으로 들어가기 (`WI-NATURE-09`) → 획득 자원 거점 보관 (`WI-NATURE-13`)
- 오두막 밖으로 나가기 (`WI-NATURE-10`) → 오두막 안으로 들어가기 (`WI-NATURE-09`)
- 황혼 위협 대응 방식 확정 (`WI-NATURE-11`) → 오두막에서 수면·새벽 맞기 (`WI-NATURE-14`)
- 황혼 위협 대응 방식 확정 (`WI-NATURE-11`) → 현장 보급 제작 업무 위임 (`WI-NATURE-17`)
- 획득 자원 거점 보관 (`WI-NATURE-13`) → 황혼 위협 대응 방식 확정 (`WI-NATURE-11`)
- 획득 자원 거점 보관 (`WI-NATURE-13`) → 오두막에서 수면·새벽 맞기 (`WI-NATURE-14`)
- 획득 자원 거점 보관 (`WI-NATURE-13`) → 현장 보급 제작 업무 위임 (`WI-NATURE-17`)
- 오두막에서 수면·새벽 맞기 (`WI-NATURE-14`) → 다음 날 거점 확장 계획 선택 (`WI-NATURE-15`)
- 다음 날 거점 확장 계획 선택 (`WI-NATURE-15`) → 영역 건물 건설 확정 (`WI-CON-01`)
- 현장 보급 제작 업무 위임 (`WI-NATURE-17`) → 나무 벌목 작업 시작 (`WI-NATURE-06`)
- 벌목 통나무 줍기 (`WI-NATURE-18`) → 나무 벌목 작업 시작 (`WI-NATURE-06`)
- 벌목 통나무 줍기 (`WI-NATURE-18`) → 오두막을 지을 터 선정 (`WI-NATURE-07`)

### 도심 서비스 내부 흐름 (`wi-flow:city-service-internal.r1`)

- 흐름 종류: 독립 영역 흐름
- 도심 서비스 수요 확정 (`WI-CITY-01`) → 도심 서비스용 지역 재고 배정 (`WI-CITY-02`)
- 도심 서비스용 지역 재고 배정 (`WI-CITY-02`) → 도심 주민 서비스 처리 (`WI-CITY-03`)
- 도심 주민 서비스 처리 (`WI-CITY-03`) → 도심 서비스 결과 확인 (`WI-CITY-04`)

## 증거 경계

- E3는 계약·코드·자동 시험의 구현 완료선이다.
- Scenario 공간으로 통과한 E3는 실제 LandscapeGraph 또는 공공 공간자료 증거가 아니다.
- E4는 WI의 허용 발생원·주체·대상·자료·자원·시간과 공간 적용 여부가 결속되는 단계다.
- E5는 실제 Simulation 세계에서 WI가 발생해 권위 상태·Task·Effect·결과·후속 경로로 발현되는 단계다.
- H는 공간 포함 계층이며 공간 WI의 E4·E5 입력 증거다. AreaSet·Graph가 존재해도 WI 발현이 없으면 E5가 아니다.
- 실제 서버와 저장 Scene에서 사람이 조작한 Play Mode·Game View·Console 증거가 있어야 E7이다.
- Unity 애니메이션이나 GameObject 상태가 Task 완료를 확정하지 않는다.
