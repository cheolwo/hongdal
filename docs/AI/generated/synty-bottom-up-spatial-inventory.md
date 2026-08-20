# Synty 상향식 공간 설계 재고

> 이 문서는 `eng/world-seedbeds/synty-bottom-up-inventory/catalog.v1.json`에서 결정적으로 생성된다. 직접 수정하지 않는다.

- 재고 개정: `simulation-world-synty-bottom-up-inventory.r2`
- 원본 Prefab: `1,762개`
- H1 의미 재고: `19개` — 승인 참조 5개, 검토 후보 14개
- H1 최소 A/B/C 표현 슬롯: `54개`
- 기준 경관 문법 후보 연결: `117개`
- H2 블록 후보: `10개 × 3 크기 = 30개 배치안`
- H3 조립 후보: `6개`

## 팩 재고

| 팩 | Prefab | Unity 상대 경로 |
| --- | ---: | --- |
| Nature | 227 | `Assets/Synty/PolygonNature` |
| Farm | 498 | `Assets/Synty/PolygonFarm` |
| Town | 702 | `Assets/Synty/PolygonTown` |
| City | 335 | `Assets/Synty/PolygonCity` |

## H1 의미 재고

| 상태 | 그룹 | 공간 재고 | E3 WI | 경관 문법 후보 |
| --- | --- | --- | --- | --- |
| 승인 H1 참조 | `FarmNature` | `h1-stock:farm-production` 농업 생산구획 | WI-FARM-01, WI-FARM-02, WI-FARM-03, WI-FARM-04 | farm:감자밭 두렁, farm:혼합 작물밭 |
| 승인 H1 참조 | `FarmNature` | `h1-stock:farm-work-yard` 수확·집하 작업마당 | WI-FARM-05, WI-FARM-06, WI-WORLD-04 | farm:농산물 집하·직판장, farm:헛간 작업마당 |
| 승인 H1 참조 | `FarmNature` | `h1-stock:farm-loading-gate` 농장 상차·출입 공간 | WI-LOG-01, WI-LOG-02 | farm:헛간 작업마당, transition:Farm–Hub 전환 |
| 검토 후보 | `FarmNature` | `h1-stock:farm-maintenance-yard` 농장 시설 정비 공간 | WI-WORLD-04 | farm:헛간 작업마당, farm:시설하우스 단동 |
| 검토 후보 | `FarmNature` | `h1-stock:nature-farm-edge` 숲 경계형 농장 전환 공간 | WI-WORLD-05 | nature:숲 가장자리, transition:Nature–Farm 전환 |
| 검토 후보 | `FarmNature` | `h1-stock:nature-exploration-buffer` 자연 탐색·완충 공간 | WI-WORLD-05, WI-WORLD-07 | nature:숲 빈터·고사목, nature:산길·바위 길목 |
| 승인 H1 참조 | `HubCity` | `h1-stock:hub-receiving-storage` Hub 입고·검수·보관 공간 | WI-LOG-04, WI-LOG-05, WI-001, WI-002 | city:물류 Station 진입부, city:상하차 Dock, city:화물 대기 야드 |
| 검토 후보 | `HubCity` | `h1-stock:hub-outbound-staging` Hub 피킹·출고 준비 공간 | WI-HUB-03, WI-HUB-04, WI-HUB-05 | city:화물 대기 야드, city:상하차 Dock |
| 검토 후보 | `HubCity` | `h1-stock:hub-vehicle-yard` Hub 차량 상차·대기 공간 | WI-HUB-06, WI-MARKET-01 | city:상하차 Dock, city:화물 대기 야드 |
| 검토 후보 | `HubCity` | `h1-stock:hub-market-transfer` Hub–시장 화물 인계 공간 | WI-MARKET-01, WI-MARKET-02 | transition:Town–Hub 전환, city:물류 Station 진입부 |
| 검토 후보 | `HubCity` | `h1-stock:hub-service-maintenance` Hub 시설 정비 공간 | WI-WORLD-04 | city:화물 대기 야드, transition:Road–BuildingFront 전환 |
| 검토 후보 | `TownMarket` | `h1-stock:town-market-receiving` 마트 후방 입고 공간 | WI-MARKET-02, WI-MARKET-03, WI-MARKET-04 | city:도심 마트 앞마당, transition:Road–BuildingFront 전환 |
| 검토 후보 | `TownMarket` | `h1-stock:town-market-display` 마트 진열·판매 공간 | WI-MARKET-05, WI-ORDER-03 | town:읍내 상점 전면, city:먹거리 상점 골목 |
| 검토 후보 | `TownMarket` | `h1-stock:town-order-packing` 주문 포장 작업공간 | WI-ORDER-04 | town:읍내 상점 전면, city:먹거리 상점 골목 |
| 검토 후보 | `TownMarket` | `h1-stock:town-resident-pickup` 주민 수령 공간 | WI-ORDER-05, WI-ORDER-06 | town:버스 정류장·보행 쉼터, town:읍내 상점 전면 |
| 검토 후보 | `TownMarket` | `h1-stock:town-living-square` 생활권 작은 광장 | WI-WORLD-05, WI-ORDER-07 | town:생활 공공광장, town:근린 놀이터 |
| 승인 H1 참조 | `CorridorTransition` | `h1-stock:farm-hub-corridor` Farm–Hub 화물 회랑 | WI-LOG-03 | network:농촌도로 직선, transition:Farm–Hub 전환 |
| 검토 후보 | `CorridorTransition` | `h1-stock:hub-town-corridor` Hub–Town 물류 회랑 | WI-MARKET-01 | network:타운도로 직선, transition:Town–Hub 전환 |
| 검토 후보 | `CorridorTransition` | `h1-stock:road-facility-access` 도로–시설 진입 전환 공간 | WI-WORLD-04 | transition:Road–BuildingFront 전환, network:타운도로 T자 |

## H2 블록 후보

| 후보 | 위상 | 포함 H1 | 설계 상태 |
| --- | --- | --- | --- |
| `h2-candidate:highland-production` 고지대 생산 블록 | `ModifiedGrid` | h1-stock:farm-production, h1-stock:nature-farm-edge | 위치 독립 설계 |
| `h2-candidate:farm-processing-shipping` 농장 작업·출하 블록 | `Linear` | h1-stock:farm-work-yard, h1-stock:farm-maintenance-yard, h1-stock:farm-loading-gate | 위치 독립 설계 |
| `h2-candidate:forest-edge-farm` 숲 경계 농장 블록 | `ContourAdaptive` | h1-stock:nature-farm-edge, h1-stock:nature-exploration-buffer, h1-stock:farm-production | 위치 독립 설계 |
| `h2-candidate:hub-inbound-storage` Hub 입고·창고 블록 | `ModifiedGrid` | h1-stock:hub-receiving-storage, h1-stock:hub-service-maintenance | 위치 독립 설계 |
| `h2-candidate:hub-outbound-vehicle` Hub 출고·차량 블록 | `Linear` | h1-stock:hub-outbound-staging, h1-stock:hub-vehicle-yard, h1-stock:hub-market-transfer | 위치 독립 설계 |
| `h2-candidate:lowrise-residential` 저층 주거 블록 | `Grid` | h1-stock:town-living-square, h1-stock:town-resident-pickup | 위치 독립 설계 |
| `h2-candidate:market-life-commerce` 마트·생활상권 블록 | `ModifiedGrid` | h1-stock:town-market-receiving, h1-stock:town-market-display, h1-stock:town-order-packing, h1-stock:town-resident-pickup, h1-stock:town-living-square | 위치 독립 설계 |
| `h2-candidate:farm-hub-corridor` Farm–Hub 회랑 블록 | `Linear` | h1-stock:farm-loading-gate, h1-stock:farm-hub-corridor | 위치 독립 설계 |
| `h2-candidate:hub-town-corridor` Hub–Town 회랑 블록 | `Linear` | h1-stock:hub-market-transfer, h1-stock:hub-town-corridor, h1-stock:road-facility-access | 위치 독립 설계 |
| `h2-candidate:nature-water-buffer` 산림·수변 완충 블록 | `Organic` | h1-stock:nature-exploration-buffer, h1-stock:nature-farm-edge | 위치 독립 설계 |

## H3 조립 후보

| 후보 | 위상 | H2 후보 | 외부 연결 역할 |
| --- | --- | --- | --- |
| `h3-candidate:highland-farm` 고지대 농장 경관 | `ContourAdaptive` | h2-candidate:highland-production, h2-candidate:farm-processing-shipping, h2-candidate:forest-edge-farm | FarmExternalGate |
| `h3-candidate:farm-hub-logistics` 농장–물류 거점 연결 경관 | `Linear` | h2-candidate:farm-processing-shipping, h2-candidate:farm-hub-corridor, h2-candidate:hub-inbound-storage | FarmGate, HubInboundGate |
| `h3-candidate:jinbu-hub` 진부형 물류 Hub 경관 | `ModifiedGrid` | h2-candidate:hub-inbound-storage, h2-candidate:hub-outbound-vehicle | HubInboundGate, HubOutboundGate |
| `h3-candidate:hub-town-logistics` Hub–Town 연결 경관 | `Linear` | h2-candidate:hub-outbound-vehicle, h2-candidate:hub-town-corridor, h2-candidate:market-life-commerce | HubOutboundGate, TownReceivingGate |
| `h3-candidate:lowrise-market-town` 저층 생활·시장 경관 | `ModifiedGrid` | h2-candidate:lowrise-residential, h2-candidate:market-life-commerce | TownReceivingGate, TownLocalRoad |
| `h3-candidate:nature-exploration-buffer` Nature 탐색·완충 경관 | `Organic` | h2-candidate:nature-water-buffer, h2-candidate:forest-edge-farm | NatureTrail, FarmEdge |

## 권위 경계

- 검토 후보는 공식 H1 정의 수에 포함하지 않는다.
- H2 후보는 실제 도로·경계·지형 근거와 결정적 경계 hash가 생기기 전까지 H2가 아니다.
- H3 조립 후보는 공식 LandscapeGraph StableId, 실제 Node·Edge·좌표를 소유하지 않는다.
- Synty Prefab·GUID·Material·Scene 경로는 표현 대장에서만 연결하며 공간·Simulation 권위를 갖지 않는다.
