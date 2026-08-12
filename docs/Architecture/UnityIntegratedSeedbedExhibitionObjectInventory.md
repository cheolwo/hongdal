# Unity 통합 모판·전시관 OBJ-0 Object 대장

## 1. 목적과 판정 기준

이 문서는 [Object 리팩토링 계획](UnityIntegratedSeedbedExhibitionObjectRefactoringPlan.md)의 `OBJ-0` 결과다. 현재 별도 Unity 프로젝트의 `통합전시관Builder.BuildWorld`가 생성하는 요소를 다음 세 종류로 분류한다.

| 분류 | 의미 | Scene 이식 |
| --- | --- | --- |
| `Object` | 다른 Scene에서 독립적으로 사용할 업무·표현 후보 | O0~O6 Gate를 거쳐 개별 배치 |
| `Marker` | 공개범위·checkpoint·상태·근거를 설명하는 보조 표현 | 업무 Object와 분리하며 필요 Scene에서만 선택 배치 |
| `Backdrop` | 전시관 바닥·배경·로비·구역 구획 | 대상 업무 Scene 이식 기본 대상 아님 |

`Object` 판정은 현재 prefab이 곧바로 O6 승격됐다는 뜻이 아니다. source, 의미, Visual, placement와 binding을 확인한 뒤 독립 Preview와 대상 Scene 배치 증거를 별도로 통과해야 한다.

## 2. 현재 BuildWorld 분류

| 현재 요소 | 분류 | 후보 stable ID 또는 처리 | 현재 Gate | 다음 작업 |
| --- | --- | --- | --- | --- |
| `WorldBase` | Backdrop | 전시관 전용 유지 | 해당없음 | catalog-driven builder에서도 전시관 shell로 유지 |
| 자료관·농장·모판·화물Hub·TownCity마트·음식배달 Ground | Backdrop | 전시 구역 layout | 해당없음 | 업무 Object prefab에 포함하지 않음 |
| `MainPath` | Backdrop | 전시용 이동 회랑 | 해당없음 | 실제 Scene 도로 module과 분리 |
| 자료관 시청 | Backdrop 후보 | `environment-backdrop:city.civic-hall.a` | O0 미등록 | 공공데이터 업무 Object로 오해하지 않도록 DataBinding 없음 |
| Farm 온실 | Object | `seedbed-object:farm.greenhouse.a` | **O5 RuntimeVerified** | 대상 Scene Placement 미검증 |
| 모판 마을집 | Backdrop 후보 | `environment-backdrop:town.house.a` | O0 미등록 | 거주자·가구 상태를 추정하지 않음 |
| 감자 Dirt Row | Object | `seedbed-object:farm.potato-row.a` | **O5 RuntimeVerified** | 6×6 field 전체가 아닌 단일 토양 표면 Visual |
| 감자 plant visual | Object | `seedbed-object:farm.potato-plant-visual.a` | **O5 RuntimeVerified** | 생육 상태 비권위 Visual variant로만 사용 |
| 감자 수확 상자 | Object | `seedbed-object:farm.potato-harvest-box.a` | **O6 PromotedToScene** | `SimulationWorldShell/FarmDistrict` 단일 배치 검증 |
| 화물 Hub 창고/garage | Object | `seedbed-object:town.hub-inbound-gate.a` | **O6 PromotedToScene** | `SimulationWorldShell/LogisticsDistrict` 개별 배치 검증 |
| Cargo Journey truck | Object | `seedbed-object:town.delivery-truck.a` | **O6 PromotedToScene** | `SimulationWorldShell/LogisticsDistrict` Cargo Journey 개별 배치 검증 |
| City pallet | Object | `seedbed-object:shared.cargo-pallet.a` | **O6 PromotedToScene** | `SimulationWorldShell/LogisticsDistrict` Warehouse Handoff 개별 배치 검증 |
| Farm pallet crate | Object | `seedbed-object:farm.pallet-crate.a` | **O6 PromotedToScene** | `SimulationWorldShell/FarmDistrict` Farm outbound·Harvest Cargo 개별 배치 검증 |
| City 도심마트 shop | Object | `seedbed-object:city.urban-market-building.a` | **O6 PromotedToScene** | `SimulationWorldShell/MarketDistrict` 공개상품·수요 신호 개별 배치 검증 |
| 운영자 전용 재고 진열대 | Object | `seedbed-object:city.operator-inventory-shelf.a` | **O5 RuntimeVerified** | 운영자 권한의 재고·ShelfTask만 결속하며 공개상품과 분리 |
| Town 집단수요 cart | Object | `seedbed-object:town.grouping-cart-table.a` | **O6 PromotedToScene** | `SimulationWorldShell/TownDistrict` 개인정보 제거 Preview 개별 배치 검증 |
| 비공개 개별의향 주민 | Object | `seedbed-object:town.resident-visual.a` | **O5 RuntimeVerified** | 실제 사람 identity·의향을 actor prefab이 소유하지 않음 |
| 마트 운영자 | Object | `seedbed-object:city.market-operator-visual.a` | **O5 RuntimeVerified** | 권한은 Perspective가 제공하며 Visual이 소유하지 않음 |
| 음식점 조리·픽업 building | Object 후보 | `seedbed-object:city.restaurant-preparation.a` | O0 후보 | preparation state와 building Visual 분리 |
| Pizza 표지 | Marker 후보 | `seedbed-marker:city.food-pickup-ready-sign.a` | O0 후보 | 조리·픽업대기 상태의 읽기 표현만 허용 |
| 음식배달 기사 차량 | Object 후보 | `seedbed-object:town.food-delivery-vehicle.a` | O0 후보 | 화물운송 차량 상태 machine 재사용 금지 |
| 확정 음식배달 기사 | Object 후보 | `seedbed-object:town.food-driver-visual.a` | O0 후보 | 기사 후보·확정 기사 공개범위 분리 |
| 음식 픽업 인계 상자 | Object | `seedbed-object:shared.food-pickup-handoff-box.a` | **O5 RuntimeVerified** | 대상 Scene Placement 미검증 |
| TownCity 공개범위 checkpoint 6개 | Marker 후보 | `seedbed-marker:story.orderer-market-disclosure.*` | O0 후보 | Story overlay로 분리하고 대상 Scene 기본 배치 금지 |
| 화물 checkpoint 7개 | Marker 후보 | `seedbed-marker:story.cargo-handoff.*` | O0 후보 | Cargo 상태 권위를 소유하지 않음 |
| 음식배달 checkpoint 8개 | Marker 후보 | `seedbed-marker:story.food-delivery.*` | O0 후보 | 전달완료·수령확인 marker를 별도 유지 |
| 모판 연구대 4개 | Backdrop | 전시관 Preview 가구 | 해당없음 | 업무 의미·DataBinding이 없어 대상 Scene 이식 금지 |
| 자료관 관측 marker 3개 | Marker 후보 | `seedbed-marker:shared.public-observation.a` | O0 후보 | 실제 관측 미수집·실패 상태 표현 |
| Lobby 기둥·beam | Backdrop | 전시관 전용 shell | 해당없음 | 대상 Scene 이식 금지 |
| Lobby authority core | Marker 후보 | `seedbed-marker:shared.authority-core.a` | O0 후보 | mode·source·revision 표시 전용 |
| 전시별 상태 beacon 6개 | Marker 후보 | `seedbed-marker:shared.story-state-beacon.*` | O0 후보 | Story 선택 강조이며 업무 상태가 아님 |

## 3. OBJ-1~7C 등록·Preview·Scene 검증 Object

현재 shared/server manifest에는 다음 열다섯 Object를 등록했다. 자료관 시청은 공공데이터 업무 Object가 아닌 Backdrop이고 관측 구체는 Marker이며, 모판 연구대는 Preview 전용 가구이므로 이 목록에 포함하지 않는다. 기존 감자 토양 모판에서 사용한 관수 스프링클러는 실제 Scene에서 독립 배치할 설비 Visual이라 OBJ-4에 포함했고, OBJ-6B에서는 EXH-3의 차량·pallet·crate를, OBJ-7A에서는 EXH-4의 주민·cart·shop·shelf·operator를 권위와 분리된 Object로 추가했다.

| Object stable ID | 의미 | Pack | binding | Gate | 차단 사유 |
| --- | --- | --- | --- | --- | --- |
| `seedbed-object:farm.potato-harvest-box.a` | 감자 수확 상자 | Farm | HarvestLot·HarvestCargo | O6 | `scene-placement:simulation-world-shell.farm.potato-harvest-box.a` |
| `seedbed-object:town.hub-inbound-gate.a` | Hub 입고 Gate | Town | CargoJourney·HubReceiving·WarehouseHandoff | O6 | `scene-placement:simulation-world-shell.logistics.hub-inbound-gate.a` |
| `seedbed-object:shared.food-pickup-handoff-box.a` | 음식 픽업 인계 상자 | Shared | RestaurantPreparation·DriverAssignment·FoodPickupHandoff | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:farm.greenhouse.a` | 농장 온실 | Farm | CultivationEnvironment·FarmEnvironmentalGrowthTurn | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:farm.potato-row.a` | 감자 밭고랑 | Farm | FarmSoilTile·SoilObservation | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:farm.potato-plant-visual.a` | 감자 재배체 | Farm | CanonicalProductCultivation·FarmEnvironmentalGrowthTurn | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:farm.irrigation-sprinkler.a` | 밭 관수 스프링클러 | Farm | FarmEnvironmentalGrowthTurn·AgriculturalWeatherObservation | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:town.delivery-truck.a` | 화물 배송 차량 | Town | CargoJourney·TransportTask·ShipperRequestCandidate | O6 | `scene-placement:simulation-world-shell.logistics.delivery-truck.a` |
| `seedbed-object:shared.cargo-pallet.a` | 공용 화물 Pallet | Shared | Cargo·HubReceiving·WarehouseHandoff | O6 | `scene-placement:simulation-world-shell.logistics.cargo-pallet.a` |
| `seedbed-object:farm.pallet-crate.a` | 농장 출하 Pallet Crate | Farm | CanonicalProductHarvestCargo·CargoJourney·HubReceiving | O6 | `scene-placement:simulation-world-shell.farm.pallet-crate.a` |
| `seedbed-object:town.resident-visual.a` | 주민 관점 Visual | Town | IndividualIntent·OwnerAuthorizedPerspective | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:town.grouping-cart-table.a` | 집단수요 Cart Table | Town | GroupingPreview·OrdererGroupSummary | O6 | `scene-placement:simulation-world-shell.town.grouping-cart-table.a` |
| `seedbed-object:city.urban-market-building.a` | 도심마트 Shop | City | MartPublicProduct·MarketDemandSignal | O6 | `scene-placement:simulation-world-shell.market.urban-market-shop.a` |
| `seedbed-object:city.operator-inventory-shelf.a` | 운영자 전용 재고 Shelf | City | MarketInventory·ShelfTask | O5 | 대상 Scene Placement 미검증 |
| `seedbed-object:city.market-operator-visual.a` | 마트 운영자 Visual | City | MarketInventory·ShelfTask·MarketOperatorPerspective | O5 | 대상 Scene Placement 미검증 |

이 열다섯 Object의 `VisualVariantKey`와 `PlacementProfileKey`는 semantic key다. shared/server contract에는 `Assets/` path, `.prefab`, Unity GUID, Bounds나 Vector3를 넣지 않는다.

## 4. 완료와 미완료

OBJ-0~4는 다음 범위에서 완료다.

- 현재 `BuildWorld`의 생성 요소를 Object, Marker, Backdrop으로 분류했다.
- 열다섯 Object의 stable ID, semantic role, Pack, zone, socket과 DataBinding 후보를 등록했다.
- Story가 존재하지 않는 Object를 참조하지 못하게 했다.
- O4 Object가 O0~O4 Verified evidence를 모두 가지도록 검증했다.
- prefab path와 로컬 Unity asset locator가 shared/server contract로 새는 것을 차단했다.
- 열다섯 Object의 wrapper prefab, `SeedbedObjectRoot`, Visual Catalog를 만들었다.
- required socket Transform, footprint와 실측 bounds를 검증했다.
- 독립 `통합Object모판`에서 항상 Object 하나만 선택하도록 하고 Game View를 확인했다.
- O5 `ObjectPreview` 증거를 `unity-change:2026-08-11-integrated-object-seedbed-obj4`로 갱신했다.
- `SimulationWorldShell/FarmDistrict`의 기존 vendor 감자 상자 표현을 wrapper prefab으로 교체하고 `harvest-lot:potato-001` navigation·camera anchor를 유지했다.
- 감자 수확 상자 하나만 O6 `PromotedToScene`으로 승격하고 placement stable ID, Scene·Zone, profile revision, anchor와 DataBinding을 receipt로 등록했다.
- `SimulationWorldShell/LogisticsDistrict`의 기존 일반 물류센터 건물 표현을 Hub 입고 Gate wrapper로 교체하고 차량·pallet·cargo와 기존 Cargo navigation을 유지했다.
- Hub 입고 Gate를 두 번째 O6 Object로 승격하고 `HubReceiving:hub-receiving:sim.potato` DataBinding과 Entry·Exit·Vehicle·Cargo socket을 개별 receipt로 검증했다.
- 배송 차량·공용 pallet·농장 출하 crate를 wrapper로 분리하고 10개 Object를 선택하는 독립 모판에서 footprint·bounds·고유 socket과 O5 Preview를 검증했다.
- Logistics District의 기존 Van 표현을 배송 차량 wrapper로 교체하고 Cargo Object Focus와 물류 이동 Preview·Confirm·Tick 경계를 유지했다.
- 배송 차량을 세 번째 O6 Object로 승격해 `CargoJourney:cargo-journey:sim.potato.farm-hub` DataBinding과 Driver·Cargo·RouteEntry·RouteExit socket을 개별 receipt로 검증했다.
- Logistics District의 기존 outbound pallet 표현을 공용 화물 Pallet wrapper로 교체하고 위의 Cargo box·차량·Hub 표현을 유지했다.
- 공용 pallet을 네 번째 O6 Object로 승격해 `WarehouseHandoff:cargo-handoff:sim.potato.20260407.r3.inbound-91` DataBinding과 Cargo·Forklift socket을 개별 receipt로 검증했다.
- Farm District의 출하 대기 위치에 농장 Pallet Crate wrapper를 수확 상자와 별도 배치하고 기존 Harvest Lot navigation을 유지했다.
- 농장 출하 crate를 다섯 번째 O6 Object로 승격해 `CanonicalProductHarvestCargo:cargo:sim.potato.20260407.r3` DataBinding과 Cargo·Forklift socket을 개별 receipt로 검증했다. 이 배치는 실제 상차·운송 확정이 아니다.
- EXH-4의 주민·집단수요 Cart Table·도심마트 Shop·운영자 전용 Shelf·마트 운영자를 다섯 wrapper로 분리하고 15개 Object를 선택하는 2열 독립 모판에서 footprint·bounds·고유 socket과 O5 Preview를 검증했다.
- 주민/운영자 actor가 identity·권한을 소유하지 않고, Cart가 참여를 확정하지 않으며, Shop의 공개상품과 Shelf의 후방재고가 서로 다른 DataBinding을 유지하도록 서버·portable mapper test를 추가했다.
- Market District의 기존 도심마트 표현을 보존하면서 Shop wrapper를 공개상품 anchor에 개별 배치하고 여섯 번째 placement receipt를 추가했다.
- 도심마트 Shop을 O6로 승격해 `MartPublicProduct:mart-product:sim.potato.public` DataBinding과 Entry·PublicProduct·DemandSignal socket을 검증했다. `MarketInventory`는 공개 placement에 포함하지 않았고 주문·재고·운영 확정 권위는 서버에 남는다.
- Town District에 집단수요 Cart Table wrapper를 개별 배치하고 일곱 번째 placement receipt를 추가했다.
- Cart Table을 O6로 승격해 `GroupingPreview:grouping-preview:sim.potato.town` DataBinding과 IntentInput·AggregateOutput·ConsentBoundary socket을 검증했다. 개인 의향 원문·자동 참여·주문 확정 Command는 placement에 포함하지 않았다.

아직 완료하지 않은 범위는 다음과 같다.

- 나머지 여덟 Object의 대상 Scene placement와 O6 승격 receipt
- 물류·음식배달의 나머지 Object 후보 O0 이상 등록
- legacy `BuildWorld`의 catalog-driven builder 전환
