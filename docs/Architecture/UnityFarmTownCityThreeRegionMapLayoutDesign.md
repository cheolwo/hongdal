# Unity Farm·Town·City 3개 독립 Region Map 구성 설계

## 1. 목적과 상태

이 문서는 Farm·Town·City를 일렬로 이어진 하나의 긴 경관이 아니라 각각 독립적으로 발전할 수 있는 세 개의 Macro Region으로 구성하고, Region 사이를 사람과 화물차량이 실제로 오가는 모습을 표현하는 Map 기준을 정의한다.

- 기준일: 2026-08-09
- 상태: 구현 전 설계
- 이번 작업에서는 Unity 코드·prefab·catalog·Scene을 변경하지 않는다.
- 기존 WORLD-0~WORLD-5, FARM-2, 농장 풍경 Composition 24개와 Cargo Journey를 재사용한다.
- Region은 Unity Presentation 공간이다. 운영 서버나 Simulation 서버의 canonical Zone·조직·주소·행정구역을 자동으로 새로 만들지 않는다.

## 2. 핵심 결정

기존의 주된 시각 흐름은 다음과 같았다.

```text
Farm → Transition → Town → Logistics → Market → Residential
```

이를 세 주 Region과 공용 지역 물류허브 구조로 바꾼다.

```text
 [Farm Region] ── 농산물 집하 ─┐
       │                         ▼
       │ 생활도로       [Regional Logistics Hub] ── City 배송 ─→ [City Region]
       │                         ▲                                  ▲
 [Town Region] ── 지역 집배송 ──┘──── 사람·통근도로 ────────────────┘
```

세 Region은 각각 내부 도로·건물·업무 공간과 독립된 확장 방향을 가진다. 여러 Farm·Town 화물은 Town과 City 사이의 공용 `Regional Logistics Hub`를 기본 경유하고, 사람 이동은 Hub의 Dock·yard와 분리된 생활·통근도로를 사용한다.

이 구조의 목표는 다음과 같다.

1. Farm은 City의 배경 구역이 아니라 생산 경영 영역으로 독립 발전한다.
2. Town은 Farm과 City 사이의 단순 장식 Transition이 아니라 주거·생활상권·지역 배송을 가진 독립 영역이 된다.
3. City는 도심 분배·도심마트·공동주택·공공정보 시설을 가진 고밀도 영역으로 독립 발전한다.
4. 지역 물류허브는 여러 Farm·Town의 화물을 입고·검수·보관·분류하고 City로 재출하하는 공유 운영 거점이 된다.
5. 세 Region의 발전 결과가 사람·차량·Cargo Journey를 통해 서로 영향을 주는 모습이 Map에서 읽힌다.
6. Region·Hub 외형과 이동 animation이 운영 상태를 임의로 확정하지 않는다.

## 3. 권장 Macro 배치

기본 World 회전에서는 Farm을 북서쪽, City를 북동쪽, Town을 남서쪽, Regional Logistics Hub를 Town과 City 사이의 중앙~동쪽에 둔다.

```text
                         북쪽 / 화면 배경

        ┌──────────────────┐               ┌──────────────────┐
        │   Farm Region    │               │   City Region    │
        │ 밭·시설하우스·출하│               │ 마트·공동주택·분배│
        └───────┬──────────┘               └────────▲─────────┘
                │ 농산물 집하                        │ City 배송
                │                                    │
        ┌───────┴──────────┐      ┌─────────────────┴──────┐
        │   Town Region    │─────▶│ Regional Logistics Hub │
        │ 주택·중심가·배송 │ 집배송│ 입고·검수·보관·출고   │
        └──────────────────┘      └────────────────────────┘

        Town↔City 사람·통근도로는 Hub freight yard와 분리

                         남쪽 / Camera
```

### 3.1 이 배치를 권장하는 이유

- 세 Region이 서로의 뒤에 숨지 않고 World Overview에서 동시에 보인다.
- 낮은 Town 건물이 전경·중경을 만들고, Farm의 Barn·Silo와 City skyline이 좌우 배경 landmark가 된다.
- Farm·Town에서 들어오는 화물이 Hub로 모이고 City로 재출하되는 hub-and-spoke 흐름이 보인다.
- Farm↔Town 생활 이동과 Town↔City 통근 이동을 화물차량 노선과 별도로 읽을 수 있다.
- 한 Region이 커져도 Map 바깥 방향으로 확장하여 Hub Gate와 중앙 이동망을 보존할 수 있다.

정확한 좌표·거리·Region·Hub 크기는 prefab bounds·camera·NavMesh 실측 뒤 확정한다. 현재 단계에서는 Region과 Hub 사이에 최소 한두 개 Composition footprint의 경관 완충과 이동 가시 공간을 확보한다.

## 4. Region별 책임과 내부 구성

### 4.1 Farm Region — 생산·농장 운영

```text
FarmRegionRoot
├─ FarmCore
│  ├─ 실제 감자 6×6
│  ├─ 환경 작물밭
│  └─ 농부 작업 공간
├─ FieldDistrict
├─ GreenhouseDistrict
├─ PaddyBlockoutDistrict?
├─ FarmYard
├─ ProduceStand
├─ FarmInternalRoads
├─ FarmToTownGate
├─ FarmToHubFreightGate
└─ FarmExpansionSockets
```

- 내부 발전: 밭·시설하우스·작기·농기계·출하장 확장
- 독립된 핵심 지표 후보: 생산능력, 작업 진행, 출하 준비와 농업시설 상태
- 외부 연결: Town 생활권 방문과 Regional Logistics Hub 출하
- 확장 방향: Map의 서쪽·북쪽
- 고정할 안쪽 경계: Town Gate와 Hub Freight Gate

### 4.2 Town Region — 저밀도 주거·생활상권·지역 배송

```text
TownRegionRoot
├─ TownNeighborhood
├─ TownMainStreet
├─ LocalShopDistrict
├─ CommunityPark
├─ RegionalDeliveryPocket
├─ TownInternalRoads
├─ TownToFarmGate
├─ TownToHubCollectionGate
├─ TownToCityPassengerGate
└─ TownExpansionSockets
```

- 내부 발전: 단독주택·정원·중심가·동네상점·놀이터·생활배송 거점 확장
- 독립된 핵심 지표 후보: 생활 편의, 지역 상권, 공동체 활동과 지역 배송 연결성
- 외부 연결: Farm 방문·직판 이용, City 통근·시장 방문, Hub 지역 집배송
- 확장 방향: Map의 남쪽·남서쪽
- 고정할 안쪽 경계: Farm Gate, Hub Collection Gate와 City Passenger Gate

Town의 주택 수와 보이는 NPC 수는 실제 세대·인구가 아니다. authorized aggregate 또는 Simulation scenario가 별도로 제공될 때만 수치로 표시한다.

### 4.3 City Region — 고밀도 시장·도심 분배·공동주거

```text
CityRegionRoot
├─ CityDistributionGate
├─ UrbanLastMileDistributionDistrict
├─ UrbanMarketDistrict
├─ ApartmentDistrict
├─ ResidentialPickupDistrict
├─ PublicDataAndOfficeDistrict
├─ CityInternalRoads
├─ CityToTownPassengerGate
├─ CityToHubDistributionGate
└─ CityExpansionSockets
```

- 내부 발전: 도심 분배·마트·상업가로·공동주택·공동수령 시설 확장
- 독립된 핵심 지표 후보: 도심 배송, 시장 공급, 주문 충족과 공동수령 상태
- 외부 연결: Hub outbound Cargo와 Town 주민·통근 이동
- 확장 방향: Map의 동쪽·북동쪽
- 고정할 안쪽 경계: Town Passenger Gate와 Hub Distribution Gate

### 4.4 Regional Logistics Hub — 공유 물류 운영 거점

```text
RegionalLogisticsHubRoot
├─ FarmInboundGate
├─ TownCollectionInboundGate
├─ InboundDockDistrict
├─ InspectionDistrict
├─ StorageDistrict
├─ SortingAndConsolidationDistrict
├─ OutboundStagingDistrict
├─ CityOutboundGate
└─ HubExpansionSockets
```

- 내부 발전: inbound lane·Dock·검수·보관·분류·출고 capacity의 Presentation 확장
- 외부 연결: 여러 Farm·Town origin과 City·다른 destination
- 배치 위치: Town 외곽과 City 진입부 사이
- 주의: 네 번째 생활 Region이 아니라 기존 물류·창고 canonical Projection을 표시하는 공유 운영 Area

세부 Hub flow와 다중 origin 기준은 [Farm·Town·City 지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md)를 따른다.

## 5. Region과 canonical Zone의 관계

`FarmRegion`, `TownRegion`, `CityRegion`은 Map과 camera를 위한 Presentation Region이다. 기존 canonical 업무 Zone을 그대로 대체하지 않는다.

| Presentation Region | 포함할 수 있는 기존 canonical 의미 |
| --- | --- |
| Farm Region | Farm Production, Farm Yard, 생산·출하 View |
| Town Region | Residential 저밀도 표현, Community·Market 접점, 지역 배송 Presentation |
| Regional Logistics Hub | Urban Logistics Center, Warehouse inbound·inspection·storage·outbound Presentation |
| City Region | Urban Market, 도심 분배, Residential Pickup, Public Data·Office Presentation |

Town을 보인다는 이유로 운영 서버에 `Town` Entity나 실제 주민 주소를 만들지 않는다. 추후 Town 고유 업무 원장이 필요해질 때만 별도 contract를 검토한다.

## 6. Region Gate 계약

Region 내부 구현이 독립적으로 바뀌어도 Gate의 위치와 의미가 유지되어야 한다.

| Gate code 후보 | 연결 | 주 이동 |
| --- | --- | --- |
| `region-gate:farm-town` | Farm↔Town | 농부·주민 방문, Pickup, 지역 배송 |
| `region-gate:farm-hub` | Farm↔Hub | Tractor/Trailer 인계, 화물차량, 농산물 Cargo |
| `region-gate:town-hub` | Town↔Hub | 지역 집배송차량, Shop cargo, 기사 |
| `region-gate:hub-city` | Hub↔City | outbound cargo, 대형·중형 배송차량 |
| `region-gate:town-city-passenger` | Town↔City | 주민·대표·통근 차량, Bus |

Gate는 다음 signature를 가진다.

```text
RegionGateSignature
├─ GateCode
├─ RegionCode
├─ ConnectedRouteCode
├─ VehicleConnector
├─ PedestrianConnector?
├─ CargoHandoffSocket?
├─ NpcTransferSocket?
├─ CameraFocusAnchor
└─ DetailTierAnchor
```

- 각 Route 양끝에는 정확히 하나의 대응 Gate가 있어야 한다.
- Region 내부 도로가 바뀌어도 Gate connector 위치·방향은 유지한다.
- 사람용 connector와 화물차량용 connector를 억지로 하나로 합치지 않는다.
- Gate는 출발·도착 위치이지 입고·검수·주문·수령 완료를 확정하는 장치가 아니다.

## 7. 지역 간 이동망

### 7.1 Farm↔Town 농촌 생활도로

```text
Farm internal dirt road
  → FarmToTownGate
  → 흙길·grass verge·driveway adapter
  → TownToFarmGate
  → Town neighborhood road
```

표현할 이동:

- 농부 또는 작업자가 Town 상점·커뮤니티 공간을 방문
- Town 주민이 Produce Stand나 Farm 체험 공간을 방문
- Pickup·소형 Delivery Truck이 농산물 상자나 생활물품을 운반
- Tractor는 Town 중심가까지 상시 진입하지 않고 Farm Gate·공유마당에서 인계

### 7.2 Town↔City 사람·통근 간선도로

```text
Town Main Street
  → TownToCityGate
  → Regional Road·Bus Stop·roadside buffer
  → CityToTownGate
  → City residential/market road
```

표현할 이동:

- Town 주민·대표의 마트·공공정보관·공동수령 거점 방문
- Bus·승용차·대표·방문자 이동
- City 직원·서비스 차량의 Town 방문
- Hub Staff Entrance로 향하는 직원·기사 분기

화물차량은 이 passenger route를 기본 배송노선으로 사용하지 않고 Hub freight route를 사용한다.

### 7.3 Farm·Town→Hub 집하·집배송 노선

```text
Farm Yard → FarmToHubFreightGate ─┐
                                  ├→ Hub Inbound Gate → Dock
Town Delivery Pocket → TownToHub ─┘
```

표현할 이동:

- 기존 cargo stable ID를 가진 Farm 농산물 화물차량
- Town 상점·지역배송 거점에서 출발하는 집배송차량
- 계획·출발·이동·Hub Gate 도착·Dock 대기 상태

### 7.4 Hub→City 배송노선

```text
Hub Storage·Outbound Staging
  → Hub City Outbound Gate
  → City Distribution Road
  → City Hub Distribution Gate
  → Market·Warehouse·Residential 목적 route
```

기본 화물은 `Origin→Hub→City`를 따른다. Farm→Town 직판·소량 지역배송은 허용하지만 Farm→City 직송은 명시적 계약·긴급·대체 route가 있을 때만 사용하는 예외다.

## 8. 사람 이동 Presentation

사람이 Region 사이를 오가는 모습은 다음 세 층으로 분리한다.

```text
Region 내부 이동
  → Gate 접근·출발
  → Inter-Region Journey
  → 목적 Region Gate 도착
  → 목적지 내부 이동
```

첫 대표 Journey 후보는 다음과 같다.

| Journey | 출발→도착 | 보이는 의미 | 자동으로 발생시키지 않는 것 |
| --- | --- | --- | --- |
| 농장 작업자 생활방문 | Farm→Town | 상점·쉼터 방문 | 구매·작업 완료 |
| Town 주민 직판장 방문 | Town→Farm | Produce Stand 탐색 | 주문·결제 |
| 공동주택 대표 시장방문 | Town 또는 City Residential→Urban Market | 수요 확인·조건 문의 | 주민 주문·계약·발주 |
| 지역 배송 작업자 이동 | Town→City Logistics | 배송·인계 과정 | 입고·검수 완료 |

개인 주소·실명·가족관계·종교·학교와 실제 이동 경로를 World에 노출하지 않는다. 실제 사용자 위치가 아니라 Simulation NPC 또는 공개·권한 검증된 집계 Presentation만 사용한다.

## 9. 화물차량과 Cargo Journey

화물 이동은 차량 skin이 아니라 cargo lineage를 기준으로 한다.

```text
InterRegionCargoJourney
├─ JourneyStableId
├─ CargoStableId
├─ ProductStableId
├─ OriginRegionCode
├─ DestinationRegionCode
├─ ViaHubCode?
├─ RouteCode
├─ JourneyPhase
├─ VehicleVisualKey
├─ Revision
└─ SourceLineage
```

Journey phase 후보:

```text
Planned
ReadyAtOrigin
Departing
InTransit
WaitingAtHubOrDestinationGate
ArrivedAtDestinationZone
```

`ArrivedAtDestinationZone`은 해당 leg의 목적 Region 또는 Hub에 도착했다는 Presentation이다. Hub 입고·검수·재고 반영·출고와 City 마트 진열은 각각 기존 canonical 상태가 있어야 표시한다.

기존 WORLD-4 Cargo Journey를 첫 `Farm→Hub` inbound leg에 재사용한다. Hub operation이 accepted storage와 명시적 outbound allocation을 제공한 뒤 `Hub→City` outbound leg를 별도 Journey로 만든다. Town 배송도 같은 모델의 별도 Journey로 추가하되 같은 cargo를 동시에 두 차량이 소유하지 않도록 revision과 handoff를 검증한다.

## 10. 상태가 있는 Journey와 분위기용 Traffic 분리

모든 움직이는 차량과 NPC에 업무 stable ID를 부여하지 않는다.

| 종류 | 목적 | 데이터 |
| --- | --- | --- |
| Stateful Journey | 실제 Simulation 또는 authorized Projection의 이동 표현 | journey·actor/cargo stable ID, revision, route, lineage |
| Ambient Traffic | World에 생활감 제공 | presentation profile·loop route만 보유, 업무 상태 없음 |

Ambient Traffic은 다음 제한을 따른다.

- `ambient` root 아래에서만 생성한다.
- Cargo·주문·계약·재고·주민 수를 암시하는 숫자로 사용하지 않는다.
- 화면 밀도와 성능을 위한 고정 presentation profile로만 제어한다.
- Stateful 차량과 같은 Gate·Dock·pickup socket을 점유하지 않는다.
- player가 선택했을 때 환경 설명은 가능하지만 상품·가격·업무 카드는 열지 않는다.

## 11. 독립 발전을 위한 Scene·Root 경계

초기 구현에서는 하나의 Integration Preview Scene 안에 Region root를 분리해 connector와 camera를 검증한다. 계약이 안정된 뒤 다음 additive Scene 구조로 분리할 수 있다.

```text
ThreeRegionWorldShell
├─ SharedCameraAndLighting
├─ RegionRegistry
├─ InterRegionJourneyRoot
├─ InterRegionRouteRoot
├─ FarmRegionScene          additive
├─ TownRegionScene          additive
├─ RegionalLogisticsHubScene additive
├─ CityRegionScene          additive
└─ WorldUiRoot
```

### 11.1 World Shell의 책임

- camera와 World/Region/Corridor/Journey focus
- 공통 lighting·Volume·quality tier
- Region·Hub·Gate·Route registry
- Stateful Journey의 Region↔Hub↔Region handoff
- ambient traffic budget
- Scene loading state와 오류 표시

### 11.2 Region·Hub Scene의 책임

- 해당 Region의 환경 Composition과 내부 route
- Region 내부 focus anchor
- Region Gate와 expansion socket
- 해당 Region에 연결된 업무 View의 VisualRoot

Region·Hub Scene은 다른 Scene 내부 object를 직접 reference하지 않는다. `GateCode`, `RouteCode`, stable ID와 registry를 통해서만 연결한다.

Scene additive loading과 streaming은 첫 구현의 필수 조건이 아니다. 먼저 한 Scene의 분리된 root로 검증하고, 독립 저장·빌드 필요성이 확인된 뒤 Scene 분리를 수행한다.

## 12. Region 독립 확장 규칙

각 Region은 `RegionFootprint`와 안쪽 Gate를 고정하고 Map 바깥 방향으로 확장한다.

| Region | 우선 확장 방향 | 보존할 안쪽 면 |
| --- | --- | --- |
| Farm | 서쪽·북쪽 | Town 생활 Gate와 Hub Freight Gate |
| Town | 남쪽·남서쪽 | Farm Gate·Hub 집배송 Gate·City Passenger Gate |
| Regional Logistics Hub | 남동쪽 또는 실측 여유 방향 | Farm·Town Inbound와 City Outbound Gate |
| City | 동쪽·북동쪽 | Town Passenger Gate·Hub Distribution Gate |

확장 시 지킬 규칙:

1. Region catalog와 builder revision은 서로 독립적으로 올릴 수 있다.
2. 다른 Region을 다시 생성하지 않고 한 Region Preview를 검증할 수 있어야 한다.
3. Gate signature가 같으면 Region 내부 A/B/C·District Recipe를 교체할 수 있다.
4. Region 성장 때문에 지역 간 도로를 밀어내거나 끊지 않는다.
5. 실제 업무 수치가 없는 건물 증축은 환경 발전 Presentation으로만 취급한다.

## 13. Camera와 이동 가시성

기존 World·Zone·Object Focus를 다음 단계로 확장한다.

```text
World Overview
Region Focus
Corridor Focus
Journey Follow
Object Focus
```

- World Overview: 세 Region silhouette, 중간 Hub와 사람·화물 노선, 이동 중 차량을 함께 본다.
- Region Focus: 한 Region 내부 발전과 Gate 출입을 본다.
- Corridor Focus: 두 Region 사이의 사람·차량 흐름을 본다.
- Journey Follow: 선택한 Stateful NPC나 화물차량을 출발 Gate부터 목적 Gate까지 추적한다.
- Object Focus: 실제 업무 object·상품·가격 Card를 본다.

Overview에서는 사람을 작은 silhouette 또는 제한된 대표 actor로 표시하고, Region·Corridor Focus에서만 개별 NPC와 차량 detail을 활성화한다.

Journey actor의 동작은 [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다. Map·Gate·route를 먼저 검증한 뒤 Farm·Town·City 대표 Humanoid에 공용 in-place Idle/Walk를 리타기팅하고, 차량은 route follower와 절차형 바퀴·조향 표현부터 연결한다. clip 누락은 fallback으로 드러내며 Synty 제공 animation으로 오표기하지 않는다.

## 14. Composition Set 사용 방식 변경

세 Pack을 전체 Map에서 계속 섞지 않는다.

| 위치 | Composition 원칙 |
| --- | --- |
| Region 내부 70~90% | 해당 Pack이 명확하게 dominant |
| Region·Hub Gate | 인접 Pack과 Logistics asset의 제한된 Boundary Set 사용 |
| 지역 간 Corridor | passenger/freight route·vegetation·vehicle 중심, 대형 landmark 최소화 |
| 목적 Region 진입부 | 도착 Region Pack 비율을 점진적으로 높임 |

Farm↔Town·Town↔City 혼합 Set은 독립 Region 사이의 관문 문법이다. Farm 전체가 Town처럼 보이거나 Town 전체가 City의 전이 구역처럼 보이지 않게 한다. Hub 진입부는 Farm/Town origin 특성을 accent로 남기되 물류 기능과 차량 route가 dominant다.

기본 화물 Route Recipe는 `Farm 또는 Town Gate→Hub Inbound→Dock·검수·보관·출고→City Distribution Gate`다. Farm→City 대형 건물을 직접 섞지 않으며 직송은 명시적 예외 route로만 남긴다.

## 15. 첫 Map 구현 수직 슬라이스

첫 구현은 세 Region 전체를 완성하지 않는다.

1. primitive 또는 검증된 A형으로 세 Region과 Regional Logistics Hub footprint·Gate를 배치한다.
2. Farm→Hub, Town→Hub, Hub→City freight route와 Farm↔Town·Town↔City 사람 route를 연결한다.
3. 기존 실제 감자 6×6·Farm Yard·Logistics Facility Overview·Urban Market·Residential Pickup을 각 Region과 Hub에 연결한다.
4. Town에는 기본주택·동네상점·생활배송 거점 A형만 배치한다.
5. Stateful 이동 세 leg를 먼저 연결한다.
   - 기존 감자 cargo inbound 화물차량: Farm→Hub
   - 명시적 Hub outbound plan 뒤 감자 cargo 배송차량: Hub→City
   - 공동주택 대표 또는 Town 주민: Town→City Market
6. Ambient 이동 한 개를 추가한다.
   - Farm↔Town Pickup 또는 주민 왕복 loop
7. World Overview·세 Region·Hub Focus, inbound/outbound Corridor와 Journey Follow를 캡처한다.

첫 완료 문장은 다음과 같다.

> Farm·Town·City가 서로 독립된 Region으로 읽히고, 여러 origin 화물이 Town과 City 사이 지역 물류허브에 입고된 뒤 명시적 출고계획으로 재출하되 사람 이동은 freight yard와 분리된다.

## 16. 검증 규칙

### Map·Region

1. 세 Region과 Regional Logistics Hub root·footprint가 겹치지 않는다.
2. 각 Region은 다른 Region 없이 독립 Preview가 가능하다.
3. Region 내부 확장이 안쪽 Gate를 이동시키지 않는다.
4. Town을 제거해도 Farm→Hub→City 기본 화물망이 유지된다.
5. Farm↔Town·Town↔City 사람 route와 Farm/Town→Hub→City 화물 route가 각자 독립적으로 유효하다.

### Route·Gate

6. 모든 Route leg는 두 개의 유효 Region 또는 Hub Gate와 연결된다.
7. 차량·보행 connector 종류와 방향이 일치한다.
8. dangling route, 중복 Gate code와 순환 없는 고립 Region을 거부한다.
9. Stateful Journey와 ambient traffic이 같은 업무 socket을 점유하지 않는다.
10. Region Scene 교체 뒤에도 Gate signature가 유지된다.

### 업무·개인정보

11. NPC 도착·animation이 주문·계약·작업·수령을 자동 완료하지 않는다.
12. Cargo 차량의 Hub 도착이 입고·검수·재고 반영을, Hub 출발이 City 입고를 자동 완료하지 않는다.
13. Town 외형으로 실제 거주지·가족관계·종교·학교를 추론하지 않는다.
14. Ambient traffic 수를 인구·수요·물동량으로 사용하지 않는다.
15. Region·Route·Journey Presentation이 서버·Simulation의 stable ID·revision·source lineage를 임의로 만들지 않는다.

### 화면·성능

16. World Overview에서 세 Region, Hub, passenger route와 freight route가 구분된다.
17. Region·Hub Focus에서 Gate 출입과 내부 주요 업무 공간이 가려지지 않는다.
18. Journey Follow에서 Scene 또는 Region 경계 순간에 actor가 중복되거나 사라지지 않는다.
19. PC·Android의 active vehicle·NPC·renderer·shadow·NavMesh 비용을 분리 측정한다.
20. 최종 Game View와 library·route preview 증거를 구분한다.

## 17. 기존 구현 순서에 미치는 영향

[Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)의 CMP 단계는 다음처럼 해석한다.

- `CMP1`: 공통 Composition 계약에 `Region`, `Hub`, `Gate`, `Route`, `Journey` signature를 포함한다.
- `CMP2`: Pack bounds뿐 아니라 세 Region·Hub footprint와 Gate 방향을 실측한다.
- `CMP3`: Farm→Hub, Town→Hub, Hub→City freight leg와 사람 route A형을 검증한다.
- `CMP4`: 각 Region과 Hub의 독립 Preview가 가능한 최소 A형을 만든다.
- `CMP4-A`: animation source validator와 공용 Humanoid locomotion adapter를 연결한다.
- `CMP5`: 세 Region·Hub Map과 Farm→Hub→City Cargo, Town→City 사람 Journey를 연결한다.
- `CMP7~CMP8`: 각 Region을 독립 확장하고 혼합 Set은 Gate subset부터 B/C로 확장한다.
- `CMP11`: Farm/Town→Hub inbound, Hub→City outbound, Town↔City passenger Corridor Focus와 Journey Follow 증거를 추가한다.

상품·가격 카드 `CMP6`은 Region 구조와 무관하게 기존 `ProductStableId`·HS mapping·PriceObservation 경계를 유지한다.

## 18. 관련 문서

- [Unity Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)
- [Unity Farm·Town·City 지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md)
- [Unity Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)
- [Unity Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)
- [Unity POLYGON Town 반복 배치 Composition Set 조사](UnityPolygonTownCompositionSetResearch.md)
- [Unity POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)
- [Unity City 주거단지·십자형 도로 Modular Composition 설계](UnityCityResidentialRoadModularCompositionDesign.md)
- [Unity Farm 시설하우스·밭·논 단지 Modular Composition 설계](UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md)
- [Unity 입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
- [Unity 서버 상태와 3D World Projection 설계](UnityServerStateToWorldProjectionDesign.md)
