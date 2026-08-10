# Unity Farm·Town·City 지역 물류허브 Map·Flow 설계

## 1. 목적과 상태

이 문서는 Town과 City 사이에 `지역 물류허브 영역`을 배치하고, 여러 Farm 또는 여러 Town에서 출하된 상품이 허브에 입고·검수·보관·분류된 뒤 City와 각 수요처로 재출하되는 Map과 Cargo 흐름을 정의한다.

- 기준일: 2026-08-09
- 상태: 구현 전 설계
- 이번 작업에서는 Unity 코드·prefab·catalog·Scene을 변경하지 않는다.
- 기존 Urban Logistics Center View, Warehouse handoff, WORLD-4 Cargo Journey와 stable ID·revision·source lineage를 재사용한다.
- 차량 도착이나 animation만으로 입고·검수·재고·출고를 완료하지 않는다.

## 2. 핵심 결정

Farm·Town·City는 계속 독립적으로 발전하는 세 개의 주 Region이다. 다만 화물망은 세 Region을 단순 직통 연결하지 않고 Town과 City 사이의 공용 운영 거점인 `Regional Logistics Hub`를 기본 경유한다.

```text
 [Farm Region A] ─┐
 [Farm Region B] ─┼─ 농산물 집하노선 ─┐
 [Farm Region …] ─┘                   │
                                      ▼
                             [Regional Logistics Hub]
                                      │
 [Town Region A] ─┐                   ├─ City 배송노선 ─→ [City Region]
 [Town Region B] ─┼─ 지역 집배송노선 ─┤
 [Town Region …] ─┘                   └─ 필요 시 타 Region 재출하

 사람 이동: Farm↔Town, Town↔City 생활·통근도로
 화물 이동: Origin Region→Regional Logistics Hub→Destination Region
```

지역 물류허브는 네 번째 생활권이나 새로운 도시가 아니다. 여러 생산·생활 Region의 화물을 모아 처리하는 공유 Operational Presentation Area다.

## 3. 권장 Macro Map 배치

기본 Map은 Farm 북서, Town 남서, Logistics Hub 중앙~동쪽, City 북동에 둔다.

```text
                         북쪽 / 화면 배경

      ┌────────────────┐               ┌────────────────┐
      │  Farm Region   │               │  City Region   │
      │ 밭·시설·출하장 │               │ 마트·공동주택  │
      └───────┬────────┘               └───────▲────────┘
              │ 농산물 집하노선                 │ City 배송노선
              │                                  │
              └──────────┐              ┌────────┘
                         ▼              │
                   ┌────────────────────┴─┐
                   │ Regional Logistics Hub│
                   │ 입고·검수·보관·출고  │
                   └──────────▲───────────┘
                              │ Town 집배송노선
                     ┌────────┴────────┐
                     │   Town Region   │
                     │ 주택·상점·배송  │
                     └─────────────────┘

       Town↔City 사람·통근도로는 물류 Dock을 통과하지 않고 별도 연결
```

### 3.1 배치 원칙

- Logistics Hub는 Town 외곽과 City 진입부 사이에 둔다.
- Town 주거·중심가와 Hub의 대형 화물차량 동선을 직접 겹치지 않는다.
- Town↔City 사람 이동도로는 Hub 외곽을 지나되 입고·출고 yard를 관통하지 않는다.
- Farm 화물은 Town 중심가를 지나지 않고 Hub의 Farm Inbound Gate로 접근한다.
- Hub→City 배송차량은 City 물류 진입로에서 마트·공동수령·창고 목적 route로 분기한다.
- 정확한 거리·도로 폭·회차 반경은 prefab bounds와 NavMesh·vehicle path 실측 뒤 확정한다.

## 4. 물류허브의 World 책임

```text
RegionalLogisticsHubRoot
├─ HubApproachRoads
├─ FarmInboundGate
├─ TownCollectionInboundGate
├─ OtherOriginInboundGates[]
├─ VehicleWaitingYard
├─ InboundDockDistrict
├─ InspectionDistrict
├─ ExceptionHoldingArea
├─ StorageDistrict
├─ SortingAndConsolidationDistrict
├─ OutboundStagingDistrict
├─ CityOutboundGate
├─ OtherDestinationOutboundGates[]
├─ StaffAndServiceArea
├─ HubFocusAnchors
├─ StatefulCargoSockets
└─ AmbientEnvironmentRoot
```

### 4.1 반드시 분리할 공간

| 공간 | 표현할 상태 | 표현하지 않는 것 |
| --- | --- | --- |
| 차량 접근·대기 | 도착 예정·Gate 대기 | 입고 완료 |
| Inbound Dock | 하차·인계 작업 후보 | 검수·재고 반영 완료 |
| Inspection | 검사 중·통과·보류의 canonical Projection | 외형만으로 품질 판정 |
| Exception Holding | 불일치·보류·반송 대기 | 자동 폐기·반송 확정 |
| Storage | 허용된 재고 위치·수량 Projection | pallet 개수로 재고 계산 |
| Sorting·Consolidation | 명시적 allocation·분류 계획 | 비슷한 상품 자동 합치기 |
| Outbound Staging | 출고 계획·상차 대기 | 차량 출발만으로 출고 확정 |
| Outbound Gate | Hub 출발·Region 간 이동 | 목적지 입고 완료 |

## 5. 다중 Farm·Town 연결 모델

현재 첫 Map에는 대표 Farm Region과 Town Region을 하나씩 배치하되, Gate·Route 계약은 복수 origin을 수용한다.

```text
OriginRegionRegistration
├─ RegionInstanceCode
├─ RegionKind              Farm | Town
├─ DisplayName
├─ HubRouteCode
├─ OriginGateCode
├─ HubInboundGateCode
├─ SupportedVehicleKinds
├─ CargoCapabilityCodes
└─ PresentationRevision
```

- `RegionInstanceCode`는 Map instance 식별자이며 실제 농장 조직·주소를 자동 의미하지 않는다.
- 실제 조직 Farm이나 Town 집배송 거점과 연결할 때는 authorized server Projection의 stable ID를 별도로 받는다.
- 새 Farm·Town을 추가할 때 Hub 전체를 다시 만들지 않고 origin registration과 route를 추가한다.
- 모든 origin을 한 Dock에 겹치지 않고 inbound gate·waiting lane·time window 후보로 분산한다.
- Map의 보이는 Farm·Town 수를 실제 공급처 수나 계약 수로 계산하지 않는다.

## 6. 기본 화물 흐름

### 6.1 Farm→Hub 입고

```text
Farm Yard 출하 준비
  → Farm Outbound Gate
  → 농산물 집하노선
  → Hub Farm Inbound Gate
  → Vehicle Waiting
  → Inbound Dock
  → Inspection
  → Accepted / Held / Rejected canonical 결과
  → Storage 또는 Cross-dock 후보
```

### 6.2 Town→Hub 집배송

```text
Town Shop·Regional Delivery Pocket
  → Town Collection Gate
  → 지역 집배송노선
  → Hub Town Inbound Gate
  → Inbound Dock
  → Inspection·분류
  → Storage 또는 Outbound Staging
```

Town에서 들어오는 화물은 Farm 생산물만이 아닐 수 있다. `ProductStableId`, cargo kind, unit과 source lineage가 명시되지 않으면 농산물로 간주하지 않는다.

### 6.3 Hub→City 출하

```text
Storage / Cross-dock
  → 명시적 Outbound Allocation
  → Sorting·Consolidation
  → Outbound Staging
  → Loading
  → Hub City Outbound Gate
  → City 배송노선
  → City Logistics Gate
  → Market / Warehouse / Residential Pickup 목적 route
```

City Region 도착 뒤에도 물류센터 입고, 마트 후방재고, 진열과 공동수령은 각각 기존 canonical 상태를 따라야 한다.

## 7. Cargo identity·분할·통합 원칙

여러 Farm·Town의 화물이 한 허브에 모인다고 cargo identity를 하나로 합치지 않는다.

```text
InboundCargoLot A ─┐
InboundCargoLot B ─┼─ explicit allocation ─→ OutboundShipment X
InboundCargoLot C ─┘
```

통합 출하가 허용되려면 최소한 다음이 명시적으로 검증되어야 한다.

- ProductStableId 또는 허용된 상품 grouping rule
- 단위와 환산 rule
- 품질·등급·보관조건의 호환 여부
- 사용 가능한 수량과 기존 allocation
- origin·lot·inspection source lineage
- Simulation/Operational mode
- outbound plan revision

분할·통합 뒤에도 다음 보존식을 검증한다.

```text
각 Inbound lot의 allocated quantity ≤ accepted available quantity
Outbound shipment quantity = 연결된 allocation quantity 합계
Rejected·Held quantity는 available inventory와 outbound에 포함하지 않음
```

보이는 pallet·상자·차량 수로 수량을 역산하지 않는다.

## 8. Journey와 업무 상태 경계

기존 `InterRegionCargoJourney`는 Route 이동을 표현한다. Hub 업무는 별도 Projection을 연결한다.

```text
Cargo Journey
  Planned → Departing → InTransit → WaitingAtHubGate → ArrivedAtHubZone

Hub Operation Projection
  InboundTask → Inspection → Accepted/Held/Rejected
  → Storage/Allocation → OutboundTask

Outbound Cargo Journey
  ReadyAtHub → Departing → InTransitToCity → ArrivedAtCityZone
```

- `ArrivedAtHubZone`은 입고 완료가 아니다.
- Inbound Dock animation 완료는 Inspection 통과가 아니다.
- Storage 위치에 pallet이 보인다는 이유로 판매 가능 재고가 되지 않는다.
- `ReadyAtHub`는 명시적 outbound plan과 allocation이 있을 때만 표시한다.
- Hub 출발 차량이 City에 보였다는 이유로 마트 재고를 늘리지 않는다.

## 9. 기본 경유와 예외 직송

### 기본 경로

```text
Farm/Town Origin → Regional Logistics Hub → City 또는 다른 Destination
```

### 허용할 수 있는 예외

- Farm→Town Produce Stand 현장 판매·소량 지역배송
- 같은 Town 내부 동네상점 배송
- 명시적 직송 계약·긴급 출하·cross-dock 정책이 있는 Farm→City
- Hub 장애 또는 Simulation scenario의 대체 route

예외 직송은 Map에 도로가 있다는 이유로 자동 선택하지 않는다. route decision source, 계약·Simulation rule, 시간·거리·비용·service area 같은 근거가 있어야 한다. 첫 구현에서는 Hub 경유를 기본으로 하고 직송은 `Disabled` 또는 명시적 scenario만 허용한다.

## 10. 사람 이동과 화물 동선 분리

Town과 City 사이 사람 이동은 유지하되 Hub yard를 통과하지 않는다.

```text
Town Main Street ── Passenger/Regional Road ── City Residential·Market
                          │
                          └─ Hub Staff Entrance 분기
```

- 주민·대표·Bus·일반 차량은 Passenger Road를 사용한다.
- 화물차량은 Hub Freight Gate와 Freight Road를 사용한다.
- 물류 직원·기사만 Staff Entrance·Driver Waiting Area로 분기한다.
- pedestrian NavMesh와 truck route의 교차점은 횡단·대기 anchor로 명시한다.
- NPC 도착은 근무·검수·인계 완료를 자동 실행하지 않는다.

Hub의 운송자·검수자·기사 동작과 차량·Dock 설비 표현은 [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다. 사람은 공용 Humanoid locomotion 뒤 `Inspect/Load/Unload/Signal` intent를 필요한 순서로 추가하고, 차량 도착·Dock·출발과 설비는 절차형 adapter를 우선한다.

## 11. Logistics Hub Composition Set 후보

첫 library는 A형만 필요한 subset으로 시작한다.

| 세트 이름 | 역할 | 주요 socket |
| --- | --- | --- |
| 지역물류 농장입고 진입부 | Farm route→Hub gate | 차량·cargo·대기 |
| 지역물류 Town집배송 진입부 | Town route→Hub gate | 배송차량·cargo·기사 |
| 지역물류 입고대기장 | Gate 대기·회차 | 차량 대기·호출 |
| 지역물류 입고 Dock | 하차·handoff | 차량·cargo·inbound task |
| 지역물류 검수마당 | 검사·보류 분리 | cargo·검수·보류 |
| 지역물류 보관블록 | accepted storage 표현 | inventory lot·location |
| 지역물류 분류통합장 | outbound allocation 표현 | lot·allocation·worker |
| 지역물류 출고대기장 | 상차·출발 준비 | outbound cargo·vehicle |
| 지역물류 City출고 진입부 | Hub→City route | 차량·cargo·route |
| 지역물류 직원서비스 구역 | 기사·직원 동선 | NPC·interaction |

A/B/C는 pallet·차량·소품 밀도와 building variant만 바꾸고 footprint, Gate·Dock connector와 핵심 socket 위치를 유지한다.

## 12. Scene·Root 구조

```text
ThreeRegionWorldShell
├─ FarmRegionRoot
├─ TownRegionRoot
├─ RegionalLogisticsHubRoot
├─ CityRegionRoot
├─ PassengerRouteRoot
├─ FreightRouteRoot
├─ StatefulJourneyRoot
└─ AmbientTrafficRoot
```

Hub는 첫 검증에서 별도 root로 두고, 계약이 안정되면 `RegionalLogisticsHubScene`으로 additive 분리할 수 있다.

- Hub Scene은 Farm·Town·City 내부 object를 직접 참조하지 않는다.
- Region·Hub Gate와 Route registry로만 연결한다.
- Stateful cargo actor는 World Shell 또는 Journey root가 소유해 Scene 경계에서 중복 생성되지 않게 한다.
- Hub Scene 실패 시 마지막 성공 상태·오류 surface를 유지하고 cargo를 임의로 목적지에 순간 이동시키지 않는다.

## 13. Camera·화면 구성

### World Overview

- Farm·Town에서 Hub로 들어오는 두 방향의 freight route가 보인다.
- Hub에서 City로 나가는 outbound route가 보인다.
- Town↔City passenger route는 freight yard와 시각적으로 구분된다.
- Hub는 Town보다 산업적이고 City skyline보다 낮은 중간 높이 landmark로 둔다.

### Hub Focus

- Inbound Gate→대기→Dock→검수→보관→출고의 방향이 한 화면에서 읽힌다.
- Farm inbound와 Town inbound를 서로 다른 lane·sign·camera side로 구분한다.
- 상태 Card를 숨겨도 차량·cargo가 어디에서 들어와 어디로 나가는지 읽힌다.

### Journey Follow

- Origin Region 출발부터 Hub Gate 도착까지 추적한다.
- Hub Operation 중에는 cargo focus로 전환한다.
- Outbound Journey가 생성된 뒤 Hub→City 차량을 별도 leg로 추적한다.

## 14. 첫 구현 수직 슬라이스

첫 구현은 대표 Farm 1개, Town 1개, City 1개와 Hub 1개만 사용한다.

1. Farm·Town·Hub·City footprint와 Gate를 blockout한다.
2. Farm→Hub, Town→Hub, Hub→City freight route를 연결한다.
3. Town↔City passenger route를 별도로 연결한다.
4. 기존 감자 cargo를 Farm→Hub inbound Journey에 연결한다.
5. 기존 Logistics Facility Overview의 차량 접근·입고 Dock·검수·보관 View를 Hub에 재사용한다.
6. 첫 canonical 또는 Simulation fixture 범위에서 accepted cargo만 outbound staging 후보로 투영한다.
7. Hub→City outbound Journey는 별도 stable ID·revision과 source allocation을 요구한다.
8. Town origin은 첫 단계에서 environment delivery 또는 별도 sample cargo로 두고 감자 cargo와 자동 병합하지 않는다.
9. Overview, Hub Focus, Farm→Hub와 Hub→City Journey Follow를 캡처한다.

첫 완료 문장은 다음과 같다.

> 여러 Origin을 수용할 수 있는 지역 물류허브에서 Farm 화물이 입고·검수·보관 상태를 거쳐 명시적 출고계획으로 City에 재출하되고, 차량 이동과 Hub 업무 상태의 권위가 분리되어 있다.

## 15. 검증 규칙

### Map·Route

1. Logistics Hub가 Town과 City 사이에 독립 root와 footprint를 가진다.
2. Farm·Town inbound route와 City outbound route가 서로 다른 Gate에 연결된다.
3. Town↔City passenger route가 freight Dock·yard를 관통하지 않는다.
4. 복수 origin registration의 Region·Route·Gate code가 중복되지 않는다.
5. 한 origin route 장애가 다른 origin cargo를 임의로 목적지에 이동시키지 않는다.

### Cargo·재고

6. 동일 cargo가 inbound와 outbound 차량에 동시에 연결되지 않는다.
7. Held·Rejected cargo는 storage available과 outbound allocation에 포함되지 않는다.
8. allocation 합계가 accepted available quantity를 초과하지 않는다.
9. 단위·상품·품질·mode 불일치 lot의 자동 통합을 거부한다.
10. split·merge 뒤 origin·lot·inspection lineage를 추적할 수 있다.
11. vehicle·pallet·box renderer 수를 수량으로 사용하지 않는다.

### 업무 권위

12. Hub Gate 도착은 입고 완료가 아니다.
13. Dock·검수 animation 완료는 canonical task 완료가 아니다.
14. 차량 출발은 outbound 확정이나 City 입고 완료가 아니다.
15. Simulation cargo를 Operational inventory·shipment로 승격하지 않는다.
16. 직송은 명시적 route decision 없이 활성화하지 않는다.

### 화면·성능

17. Overview에서 origin→Hub→City 방향과 passenger road를 구분할 수 있다.
18. Hub Focus에서 접근·입고·검수·보관·출고 영역이 가려지지 않는다.
19. Stateful truck과 ambient truck이 같은 Dock socket을 점유하지 않는다.
20. PC·Android에서 active vehicle, cargo renderer, shadow, NavMesh와 Hub interior detail을 분리 측정한다.

## 16. 기존 설계에 미치는 영향

- Farm↔City 직통 화물회랑은 기본 노선에서 예외 직송 후보로 변경한다.
- `Farm→Hub→City`가 첫 감자 Cargo Journey의 기본 Map 경로가 된다.
- Town은 `Town→Hub` 지역 집배송과 `Town↔City` 사람 이동을 별도 Route로 가진다.
- Logistics Hub는 Town이나 City의 장식 Transition이 아니라 공유 운영 거점이다.
- 기존 City Region의 `UrbanLogisticsDistrict`는 Hub의 하류 City 진입·도심 분배 거점으로 축소하거나 역할을 구분한다.
- 3 Region 독립 발전 원칙은 유지하되 Region 사이 화물망의 중심은 Hub-and-spoke 구조로 바뀐다.

## 17. 관련 문서

- [Unity Farm·Town·City 3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md)
- [Unity Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)
- [Unity Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)
- [Unity Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)
- [Unity 입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
- [Unity Zone별 Domain 심화 설계](UnityZoneDomainDeepeningDesign.md)
- [살뜰마트 도심 창고 앱 설계](SsalddelMartUrbanWarehouseApp.md)
- [Unity 서버 상태와 3D World Projection 설계](UnityServerStateToWorldProjectionDesign.md)
