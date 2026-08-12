# Unity 통합 모판·전시관 Object 리팩토링 계획

## 1. 목적과 현재 판정

이 문서는 현재 `EXH-0~EXH-5`로 구현된 통합 모판·전시관을 **완성된 업무 장면의 전시장**이 아니라 **각 실제 Scene에 개별 배치할 Object를 연구·비교·검증·승격하는 공간**으로 리팩토링하는 계획이다.

현재 구현은 stable ID, source lineage, 공개범위, 업무 checkpoint와 코드·test·Runtime·운영 연결 증거를 보존한다는 점에서 재사용 가치가 높다. 반면 Unity `통합전시관Builder.BuildWorld`가 건물·차량·인물·상자·marker와 업무 이야기를 하나의 Scene 좌표계에 직접 조립하므로, 개별 Object를 다른 Scene에 심는 모판으로는 아직 분리되지 않았다.

따라서 이 계획의 결론은 다음과 같다.

> 기존 EXH 업무 계보는 `Story`로 보존하고, Scene에 심을 수 있는 최소 시각 단위는 `Seedbed Object`로 분리하며, 실제 Scene 좌표와 연결 결과는 `Scene Placement`로 따로 기록한다.

2026-08-12 `OBJ-0~OBJ-5`, `OBJ-6A Hub Gate pilot`, `OBJ-6B 물류 Object 분해`, `OBJ-6C 배송 차량 이식`, `OBJ-6D-1 공용 pallet 이식`, `OBJ-6D-2 농장 출하 crate 이식`, `OBJ-7A EXH-4 Object 분해`, `OBJ-7B 도심마트 Shop 이식`, `OBJ-7C 집단수요 Cart Table 이식`을 구현했다. 현재 additive `Stories`, `SeedbedObjects`, `ScenePlacements` 계약과 열다섯 Object의 wrapper prefab·Visual Catalog·footprint·bounds·socket validator, 독립 Preview 모판이 있다. 감자 수확 상자·Hub 입고 Gate·화물 배송 차량·공용 화물 Pallet·농장 출하 Pallet Crate·도심마트 Shop·집단수요 Cart Table은 `SimulationWorldShell`에 이식해 O6 receipt까지 검증했고, 나머지 여덟 Object는 O5를 유지한다.

## 2. 정정된 개념 모델

### 2.1 전시관, 모판, Object, Scene의 관계

```text
통합 모판·전시관 Scene
  ├─ Object 후보를 카탈로그에서 불러온다
  ├─ 후보별 source·의미·배치 조건·증거를 보여 준다
  ├─ 여러 Object를 임시로 나란히 놓아 Story를 설명한다
  └─ 검증된 Object만 대상 Scene 이식 후보로 승격한다

Seedbed Object
  ├─ 건물, 조리대, 선반, Dock, Gate, 차량, 화물, marker 등
  ├─ 독립 stable ID와 VisualKey를 가진다
  ├─ Actor·Vehicle·Cargo·Interaction Socket을 가진다
  └─ 주문·배차·재고·수령 상태를 스스로 소유하지 않는다

대상 Scene
  ├─ 필요한 Object만 선택해 배치한다
  ├─ Scene 전용 Placement stable ID와 좌표를 가진다
  └─ 서버/Simulation snapshot을 Object Presenter에 주입한다
```

전시관에서 음식배달 관계를 한 줄로 설명할 수는 있지만, `음식배달 장면 전체`를 하나의 배치 모듈로 만들지 않는다. 음식점 준비대, 픽업 인계점, 기사 대기점, 차량, 전달 지점과 수령 확인 표식은 각각 독립 Object 후보다.

### 2.2 세 가지 identity

| identity | 예시 | 소유자 | 바뀌어도 되는 것 |
| --- | --- | --- | --- |
| 업무 record stable ID | `food-order:...`, `cargo:...` | 서버/Simulation | 권위 있는 상태 전이와 revision에 따라서만 변경 |
| 모판 Object stable ID | `seedbed-object:food.pickup-counter.city.a` | 전시관 catalog | 새 revision·사람 검토를 거쳐 의미와 호환 조건 변경 |
| Scene Placement stable ID | `placement:city-district-01:food-pickup-counter-01` | Unity Scene composition | 대상 Scene의 좌표·회전·variant 교체 |

세 identity를 이름, 배열 순서 또는 prefab 경로로 서로 추정하지 않는다. 업무 record와 Object의 관계는 `DataBindingKey`, Object와 Placement의 관계는 명시적 stable ID reference로만 연결한다.

## 3. 현재 구현에서 유지할 것과 분리할 것

### 3.1 그대로 유지할 기반

- `통합전시관ManifestResponse`의 읽기 전용 원칙
- source plan, canonical relation, expected target revision
- `Fixture`, `Uncollected`, `Live`, `Failed` 데이터 상태 구분
- `Research`, `ReadOnly`, `Simulation`, `OperationalHandoff` mode 구분
- 공개범위와 역할별 authorization 경계
- 코드·집중 test·Runtime·운영 연결의 네 evidence 축
- 화물, 주문자 집단·마트, 음식배달의 업무 `Story`와 checkpoint
- `VisualKey → wrapper/prefab/material/FX` 방향과 Synty 원본 비권위 원칙
- 기존 EXH-2~5 Game View와 변경 기록

### 3.2 분리해야 할 결합

| 현재 결합 | 문제 | 목표 |
| --- | --- | --- |
| `Exhibit`가 Story와 Object 후보를 동시에 표현 | 전체 업무 묶음이 배치 단위처럼 보임 | `StoryDefinition`과 `SeedbedObjectDefinition` 분리 |
| `ZoneStableId`와 Object 목록이 Exhibit에 고정 | 다른 Scene·Zone 재사용 관계를 표현하기 어려움 | Object의 compatible zone과 별도 Placement manifest |
| `BuildWorld`가 prefab path·위치·크기를 한 메서드에 하드코딩 | 개별 Object 검증·교체·이식 불가 | Object prefab/descriptor와 Scene placement 조립 분리 |
| primitive checkpoint와 beacon이 Story 상태를 직접 시각화 | marker 자체가 재사용 Object인지 단순 증거 UI인지 불명확 | `EvidenceMarker`, `WorkflowMarker`, 업무 Object 분류 |
| Exhibit-level completion state만 존재 | Object 하나의 준비 상태와 Story 연결 상태가 섞임 | Object Gate와 Story completion을 별도 관리 |
| Scene Game View만 Runtime 증거로 사용 | 개별 Object가 다른 Scene에서도 성립하는지 알 수 없음 | Object preview와 대상 Scene placement 증거 분리 |

## 4. 목표 계약

### 4.1 StoryDefinition

기존 EXH-3~5의 업무 계보를 보존하는 읽기 전용 설명 단위다.

- `StoryStableId`
- `DisplayName`
- `WorkflowKey`
- `CanonicalRecordRelations`
- `WorkflowCheckpoints`
- `ReferencedObjectStableIds`
- `DisclosureScopeCodes`
- `SourcePlan`
- `Evidence`

Story는 Object를 나란히 강조할 수 있지만 Scene 배치 좌표, prefab 경로 또는 운영 Command를 소유하지 않는다.

### 4.2 SeedbedObjectDefinition

실제 Scene에 독립적으로 심을 수 있는 최소 후보 단위다.

| 필드 | 의미 |
| --- | --- |
| `ObjectStableId` | 모판 Object의 호환 식별자 |
| `DisplayName` | 한국어 기본 표시 이름 |
| `SemanticRoleCode` | `RestaurantPreparation`, `PickupHandoff`, `InboundDock` 등 업무 표현 역할 |
| `ObjectKindCode` | Building, Facility, Furniture, Vehicle, ActorVisual, CargoVisual, Marker, Surface |
| `VisualVariantKeys` | Pack·표현 variant 후보. prefab path가 아님 |
| `PackRoleCodes` | Farm, Town, City, Shared |
| `CompatibleZoneRoleCodes` | 배치 가능한 Scene/Zone 역할 |
| `PlacementProfileKey` | footprint·방향·지면·clearance를 찾는 Unity catalog key |
| `RequiredSocketCodes` | Actor, Vehicle, Cargo, Interaction, Entry, Exit, Label, CameraFocus |
| `DataBindingKeys` | 표현 가능한 server/Simulation projection 종류 |
| `PresentationStateCodes` | Normal, Selected, Blocked, Stale 등 표현 상태 |
| `GateStateCode` | 후보부터 Scene 승격까지의 Object 전용 Gate |
| `BlockedReasonCodes` | 누락된 source, socket, binding, Runtime 증거 등 |
| `Evidence` | 코드·test·Object Preview·대상 Scene placement 증거 |

서버/shared contract에는 의미와 compatibility만 둔다. 실제 prefab GUID/path, Bounds, Vector3 좌표, material과 renderer 목록은 Unity `전시ObjectVisualCatalog`가 가진다.

### 4.3 Unity PlacementProfile

Unity 전용 catalog가 다음 정보를 관리한다.

- source asset GUID와 `VisualVariantKey`
- local bounds와 footprint
- 허용 회전 단위와 정면 방향
- ground contact, slope, clearance, occlusion 기준
- Entry·Exit·Actor·Vehicle·Cargo·Interaction·Label·CameraFocus socket transform
- required/optional renderer·collider·Animator·FX
- PC/Android presentation tier
- 원본 asset revision과 wrapper prefab revision

PlacementProfile은 생산량, 재고, 주문 상태, 배차 성공이나 업무 우선순위를 계산하지 않는다.

### 4.4 SceneObjectPlacement

대상 Scene에서 Object를 실제로 사용한 기록이다.

- `PlacementStableId`
- `SceneStableId`와 `ZoneStableId`
- `ObjectStableId`
- `VisualVariantKey`와 `PlacementProfileRevision`
- Scene local position·rotation·scale
- 연결된 Scene anchor/socket key
- `DataBindingKey`
- placement validation 결과
- 대표 Game View와 Runtime test evidence
- 승격을 승인한 revision 또는 변경 기록 reference

Scene 좌표는 운영 원장에 넣지 않는다. Placement가 삭제되어도 업무 record를 삭제하거나 완료시키지 않는다.

## 5. Object prefab 공통 구조

```text
SeedbedObjectRoot
  ├─ VisualRoot
  ├─ SocketRoot
  │   ├─ EntrySocket
  │   ├─ ExitSocket
  │   ├─ ActorSocket
  │   ├─ VehicleSocket
  │   ├─ CargoSocket
  │   ├─ InteractionSocket
  │   ├─ LabelSocket
  │   └─ CameraFocusSocket
  ├─ PresentationRoot
  ├─ PlacementProbeRoot
  └─ DebugEvidenceRoot
```

모든 socket을 모든 Object에 강제하지 않는다. `RequiredSocketCodes`에 선언한 socket만 필수 검증한다. `DebugEvidenceRoot`와 모판 label은 대상 제품 Scene에서 끌 수 있어야 하며 업무 상태를 대신하지 않는다.

## 6. Object 승격 Gate

턴 카드 C0~C6와 같은 원칙을 Object에 맞게 적용한다.

| Gate | 상태 | 통과 조건 | 실패 시 |
| --- | --- | --- | --- |
| O0 | `Indexed` | source asset GUID와 license·Pack 확인 | 후보 등록 금지 |
| O1 | `MeaningMapped` | 한국어 이름, SemanticRole, 알 수 있음/없음 검토 | `Unlinked` |
| O2 | `VisualResolved` | VisualVariant와 wrapper prefab 연결 | `Blocked:VisualMissing` |
| O3 | `PlacementValidated` | bounds, footprint, 방향, 필수 socket 검증 | `Blocked:PlacementInvalid` |
| O4 | `BindingValidated` | DataBinding contract와 개인정보·권위 경계 검증 | `Blocked:BindingInvalid` |
| O5 | `RuntimeVerified` | 독립 Preview Scene에서 test·Game View 확인 | `Blocked:RuntimeUnverified` |
| O6 | `PromotedToScene` | 특정 대상 Scene placement와 대표 Game View 승인 | `Promoted` |

O5 통과만으로 모든 Scene에 자동 배치하지 않는다. O6은 `ObjectStableId + SceneStableId + PlacementProfileRevision` 조합의 명시적 placement receipt가 있어야 한다. 한 Scene의 O6 통과는 다른 Scene의 승격 증거가 아니다.

## 7. EXH-0~EXH-5 마이그레이션 지도

### 7.1 EXH-0 현황 대장

- 현재 Exhibit 후보 표는 `Story 후보 대장`으로 보존한다.
- 별도 `Object 후보 대장`을 추가해 O0~O6를 기록한다.
- 코드·test·Runtime·운영 evidence와 Object Preview·Scene Placement evidence를 구분한다.

### 7.2 EXH-1 공통 manifest

- 기존 `Exhibits`는 호환 기간 동안 Story aggregate로 유지한다.
- additive하게 `Stories`, `SeedbedObjects`, `ScenePlacements`를 추가한다.
- 기존 `ObjectStableIds`는 `ReferencedObjectStableIds` adapter로 읽는다.
- 기존 consumer가 새 필드를 몰라도 EXH-0~5 manifest를 계속 읽을 수 있게 한다.

### 7.3 EXH-2 로비·자료관·Farm

| 현재 Scene 요소 | 리팩토링 Object 후보 | 분류 |
| --- | --- | --- |
| 출처·권위 beacon | `seedbed-object:shared.authority-beacon.a` | Marker |
| 공공데이터 관측 marker/table | `seedbed-object:shared.public-observation-surface.a` | Surface |
| Farm 온실 | `seedbed-object:farm.greenhouse.a` | Building |
| 감자 Dirt Row | `seedbed-object:farm.potato-row.a` | Facility |
| 감자 plant | `seedbed-object:farm.potato-plant-visual.a` | CargoVisual이 아닌 PlantVisual |
| 감자 수확 상자 | `seedbed-object:farm.potato-harvest-box.a` | CargoVisual |
| 모판 연구대 | `seedbed-object:shared.study-plinth.a` | Furniture |

CityHall·TownHouse처럼 현재 구역 배경 역할만 하는 대형 건물은 곧바로 업무 Object로 승격하지 않는다. `EnvironmentBackdrop` 후보로 따로 분류하고 DataBinding을 요구하지 않는다.

### 7.4 EXH-3 화물·Hub·창고

| Story 역할 | Object 후보 |
| --- | --- |
| 화주 의뢰 후보 표시 | `shared.request-candidate-marker.a` |
| Cargo 이동 | `town.delivery-truck.a` |
| Cargo 표시 | `shared.cargo-pallet.a`, `farm.pallet-crate.a` |
| Hub 접근·입고 | `town.hub-inbound-gate.a` |
| 검수 | `shared.inspection-station.a` |
| 창고 인계 | `shared.warehouse-handoff-marker.a` |

Cargo relation과 seven checkpoint는 Story에 남는다. Truck이나 pallet의 위치 변화가 checkpoint 상태를 변경하지 않는다.

### 7.5 EXH-4 주문자 집단·도심마트

| Story 역할 | Object 후보 |
| --- | --- |
| 개인 의향 | `town.individual-intent-marker.a` |
| 집단화 Preview | `town.grouping-cart-table.a` |
| 주민 관점 | `town.resident-visual.a` |
| 마트 공개상품 | `city.public-product-shelf.a` |
| 운영 재고 | `city.operator-inventory-shelf.a` |
| 진열 작업 후보 | `city.shelf-task-marker.a` |
| 마트 운영자 | `city.market-operator-visual.a` |

주민·운영자 actor visual은 사람 identity나 권한을 소유하지 않는다. Perspective가 허용할 때만 역할을 표현하는 교체 가능한 Visual Object다.

### 7.6 EXH-5 음식배달

| Story 역할 | Object 후보 |
| --- | --- |
| 음식점 준비 | `city.restaurant-preparation.a` |
| 픽업대기 표지 | `city.food-pickup-ready-sign.a` |
| 기사 후보 권역 | `shared.driver-offer-zone-marker.a` |
| 확정 기사 | `town.food-driver-visual.a` |
| 기사 차량 | `town.food-delivery-vehicle.a` |
| 음식 픽업 인계 | `shared.food-pickup-handoff-box.a` |
| 전달 지점 | `shared.food-delivery-point.a` |
| 주문자 수령 확인 | `shared.orderer-receipt-marker.a` |

여덟 음식배달 checkpoint와 공개범위는 Story에 남는다. 각 Object는 여러 City·Town·주거·마트 Scene에 개별 배치할 수 있으며 `전달완료`와 `수령확인` 상태를 자체 저장하지 않는다.

## 8. 단계별 리팩토링 순서

| 순서 | 단계 | 구현 범위 | 완료 Gate |
| --- | --- | --- | --- |
| 1 | `OBJ-0 현재 Object 대장` | BuildWorld 시각물·primitive·배경을 Object/Marker/Backdrop으로 분류 | 모든 현재 요소가 유지·분리·폐기 후보 중 하나로 판정됨 |
| 2 | `OBJ-1 공통 계약` | Story, SeedbedObject, Placement reference와 O0~O6 code 추가 | 기존 EXH manifest 호환 test와 새 중복·누락 검증 통과 |
| 3 | `OBJ-2 Unity descriptor/catalog` | prefab path를 Unity catalog로 이동하고 공통 root/socket validator 추가 | engine-independent core가 Unity GUID/path를 참조하지 않음 |
| 4 | `OBJ-3 독립 Preview 모판` | Object 하나를 선택·회전·variant 비교하고 evidence를 보는 Preview bay | Object 단독 EditMode·Game View 통과 |
| 5 | `OBJ-4 EXH-2 분해` | Farm·자료관·모판 Object를 prefab/descriptor로 분리 | 기존 EXH-2 Story 화면과 시각 회귀 유지 |
| 6 | `OBJ-5 첫 대상 Scene 이식` | 감자 수확 상자를 Farm HarvestLot 위치의 wrapper로 교체 | 단일 placement receipt, Runtime test, 대표 Game View 확인 |
| 7 | `OBJ-6 EXH-3 분해·이식` | truck·pallet·Hub·검수·handoff를 분리하고 Hub Gate pilot 연결 | 동일 Cargo lineage와 별도 인수 경계 유지 |
| 8 | `OBJ-7 EXH-4 분해` | 주민·cart·shop·shelf·operator 분리 | 네 공개범위와 공개수량/후방재고 분리 유지 |
| 9 | `OBJ-8 EXH-5 분해` | 음식점·표지·기사·차량·상자·전달·수령 marker 분리 | 기사 후보/확정 기사 공개범위와 전달/수령 분리 유지 |
| 10 | `OBJ-9 placement 확대·BuildWorld 축소` | 검증된 Object를 개별 이식하고 hardcoded 조립을 catalog-driven placement로 교체 | stable placement와 중복 방지, Story 회귀 유지 |

첫 pilot은 업무 전체를 옮기지 않는다. `감자 수확 상자` 하나를 먼저 이식하고, 이후 `Hub 입고 Gate`, `음식 픽업 인계 상자`처럼 권위 경계가 분명한 Object를 각각 별도 receipt와 Game View로 추가한다.

## 9. 호환·마이그레이션 원칙

- 기존 Exhibit stable ID, wire field, canonical relation, error code를 rename하지 않는다.
- 기존 `ObjectStableIds`를 즉시 삭제하지 않고 새 Story reference로 adapter한다.
- 새 Object stable ID를 기존 prefab 이름이나 배열 순서에서 자동 생성하지 않는다.
- Synty 원본 prefab과 material은 수정하지 않고 wrapper prefab을 만든다.
- Unity `.meta` GUID는 `AssetDatabase` 이동으로 보존한다.
- 기존 EXH-2~5 Scene과 Game View는 migration baseline으로 유지한다.
- Object 분해 중 기존 Story test가 깨지면 새 구조를 우선하지 않고 해당 단계에서 중단한다.
- Scene Placement를 운영 원장, 재고, 주문 또는 업무 완료 증거로 해석하지 않는다.
- 마이그레이션이 끝날 때까지 legacy `BuildWorld`와 catalog-driven builder를 feature flag가 아닌 별도 Editor 경로로 병행한다.

## 10. 검증 계획

### 10.1 공통 계약

- Story/Object/Placement stable ID 중복 거부
- 존재하지 않는 Object를 참조하는 Story 거부
- 존재하지 않는 Object를 참조하는 Placement 거부
- `PromotedToScene`인데 O0~O5 증거가 빠진 Object 거부
- server/shared contract에 `Assets/`, prefab GUID, Vector3가 들어오면 architecture test 실패
- 권한 범위 밖 DataBinding 거부

### 10.2 Unity Object

- 필수 root와 socket 존재
- bounds·footprint·ground contact·forward 방향 검증
- 원본 Synty asset mutation 없음
- wrapper prefab missing script·missing material 없음
- 동일 Object를 두 Scene에 배치해도 업무 stable ID가 복제·변경되지 않음
- DebugEvidenceRoot 비활성 상태에서도 본체 의미가 유지됨

### 10.3 Story 회귀

- EXH-3: 도착, 입고, 검수, 보관 상태 분리
- EXH-4: 본인 비공개, 개인정보 제거 집계, 주문자 공개, 운영자 전용 분리
- EXH-5: 기사 후보 권역 축약, 확정 기사 전용, 전달 완료와 수령 확인 분리
- generic Confirm과 운영 Command 없음

### 10.4 Runtime·Game View

증거를 두 종류로 나눈다.

1. `Object Preview Evidence`: 모판에서 단독 선택·회전·variant·socket 확인
2. `Scene Placement Evidence`: 실제 대상 Scene에서 주변 도로·건물·actor와 함께 배치 확인

Scene View만으로 어느 쪽도 완료 처리하지 않는다.

## 11. 예상 변경 경로

### Hongdal/shared

- `Ssalddel.Contracts/Common/WorldProjection/통합전시관Dtos.cs`
- `Ssalddel/Application/WorldProjection/통합전시관Projector.cs`
- `Ssalddel.Unity/Runtime/Exhibition/통합전시관Models.cs`
- 대응 server/portable Unity test

### 별도 Unity 프로젝트

- `Assets/Ssalddel/Presentation/ExhibitionObjects/`
- `Assets/Ssalddel/Presentation/World/전시ObjectDescriptor.cs`
- `Assets/Ssalddel/Editor/전시ObjectCatalogBuilder.cs`
- `Assets/Ssalddel/Editor/전시ObjectPlacementValidator.cs`
- `Assets/Ssalddel/Scenes/통합모판전시관.unity`
- `Documentation/Changes/<object-gate>/`

경로와 타입 이름은 `OBJ-1` 구현 직전 가까운 `AGENTS.md`와 현재 assembly 경계를 다시 확인한 뒤 확정한다.

## 12. 완료 기준과 비범위

리팩토링 완료는 다음을 모두 만족해야 한다.

- 전시관이 개별 Object 후보를 선택·비교·검증한다.
- Story가 Object의 관계를 설명하지만 배치 단위를 결정하지 않는다.
- Object 하나를 둘 이상의 대상 Scene에 독립 배치할 수 있다.
- Scene 좌표와 prefab 교체가 업무 record·상태를 바꾸지 않는다.
- Object 승격은 O0~O6 evidence와 명시적 placement receipt를 가진다.
- 기존 EXH-3~5의 권한·상태 경계 test가 그대로 통과한다.
- Object Preview와 대상 Scene Game View를 별도 증거로 보존한다.

다음은 이 리팩토링의 비범위다.

- 실제 주문·배차·결제·창고·수령 Command 실행
- Unity 배치 결과의 운영 DB 저장
- prefab 자동 추천을 통한 무승인 Scene 변경
- 모든 Synty asset 1,535개의 일괄 wrapper 생성
- 한 번에 모든 기존 Scene을 새 catalog로 변환
- Object 배치 위치로 거주자·생산량·재고·영업 상태 추정

## 13. 현재 구현 결과와 즉시 다음 단계

`OBJ-0~OBJ-5`, `OBJ-6A Hub Gate pilot`, `OBJ-6B 물류 Object 분해`, `OBJ-6C 배송 차량 이식`, `OBJ-6D-1 공용 pallet 이식`, `OBJ-6D-2 농장 출하 crate 이식`, `OBJ-7A EXH-4 Object 분해`, `OBJ-7B 도심마트 Shop 이식`, `OBJ-7C 집단수요 Cart Table 이식` 완료 범위는 다음과 같다.

1. [OBJ-0 Object 대장](UnityIntegratedSeedbedExhibitionObjectInventory.md)에서 현재 Scene 요소를 Object, Marker, Backdrop으로 분류했다.
2. 기존 `Exhibits` 호환성을 유지한 additive `Stories`, `SeedbedObjects`, `ScenePlacements` contract를 추가했다.
3. 기존 열 Object에 `주민 관점 Visual`, `집단수요 Cart Table`, `도심마트 Shop`, `운영자 전용 재고 Shelf`, `마트 운영자 Visual`을 더해 열다섯 Object를 O5 `RuntimeVerified` 이상으로 등록했다.
4. server projector와 engine-independent Unity mapper에서 중복·Story 참조·Gate evidence·prefab path 누출을 검증한다.
5. 별도 Unity 프로젝트에 열다섯 wrapper prefab과 `통합전시관ObjectVisualCatalog`, `SeedbedObjectRoot`, 실제 socket Transform·footprint·bounds 검증을 추가했다. 자료관 시청·관측 Marker·모판 연구대는 업무 Object가 아니므로 Backdrop·Marker·Preview 가구로 유지했다.
6. `통합Object모판` Scene은 열다섯 Object 중 하나만 선택·회전하며 descriptor와 O5/O6 경계를 표시한다. 2열 선택 UI로 모든 Object를 한 화면에서 고를 수 있고 업무 상태나 운영 Command를 소유하지 않는다.
7. server 집중 16/16, portable Unity mapper 16/16, 실제 Unity EditMode 3/3, batch Scene 생성, Play Mode Console 오류 0건과 1600×900 Game View를 통과했다.
8. OBJ-4까지는 모든 O6 Scene Placement가 `Unverified`였으며, OBJ-5·OBJ-6A·OBJ-6C·OBJ-6D-1·OBJ-6D-2·OBJ-7A·OBJ-7B·OBJ-7C 뒤 아직 배치하지 않은 여덟 Object는 `TargetScenePlacementNotPromoted` 차단 사유를 유지한다.
9. 감자 수확 상자의 기존 Farm vendor 표현을 wrapper prefab으로 교체하고 `scene-placement:simulation-world-shell.farm.potato-harvest-box.a` receipt를 추가했다.
10. 물류 District의 기존 일반 Hub 건물 표현을 `Hub 입고 Gate` wrapper로 교체하고 `scene-placement:simulation-world-shell.logistics.hub-inbound-gate.a` receipt를 추가했다. Van·pallet·cargo와 기존 Cargo navigation은 유지하며 `HubReceiving:hub-receiving:sim.potato`를 결합점으로 사용한다.
11. 실제 Unity 저장 Scene placement 2/2, `SimulationWorldShellTests` 10/10, 서버 projector와 portable mapper 각각 18/18, Logistics District 1600×900 Game View를 통과했다. Play Mode Console에는 배치와 무관한 로컬 턴마감 서버 미실행 `ConnectionError` 1건이 남았다.
12. EXH-3의 배송 차량·공용 pallet·농장 출하 crate를 서로 다른 stable ID·binding·socket의 wrapper로 분리했다. Unity Object 모판 4/4와 Play Mode Console 오류 0건, 배송 차량 1600×900 Game View, 서버 projector 18/18과 portable mapper 19/19를 통과했다.
13. Logistics District의 기존 Van 표현을 배송 차량 wrapper로 교체하고 세 번째 placement receipt를 추가했다. Unity placement 3/3·기존 Scene 10/10, 서버 projector 19/19·portable mapper 20/20과 Cargo Object Focus 1600×900 Game View를 통과했다. Play Mode Console에는 로컬 턴마감 서버 미실행 `ConnectionError` 1건이 남았다.
14. Logistics District의 기존 outbound pallet 표현을 공용 pallet wrapper로 교체하고 네 번째 placement receipt를 추가했다. Unity placement 4/4·기존 Scene 10/10, 서버 projector 20/20·portable mapper 21/21과 Logistics District 1600×900 Game View를 통과했다. Play Mode Console에는 로컬 턴마감 서버 미실행 `ConnectionError` 1건이 남았다.
15. Farm District의 출하 대기 위치에 농장 Pallet Crate wrapper를 수확 상자와 별도 배치하고 다섯 번째 placement receipt를 추가했다. `farm.outbound.pallet-crate` anchor는 `CanonicalProductHarvestCargo:cargo:sim.potato.20260407.r3`만 참조하며 실제 상차·운송을 확정하지 않는다. Unity placement 5/5·기존 Scene 10/10, 서버 projector 21/21·portable mapper 22/22와 Harvest Lot 1600×900 Game View를 통과했다. Play Mode Console에는 로컬 턴마감 서버 미실행 `ConnectionError` 1건이 남았다.
16. EXH-4의 주민·Cart Table·Shop·운영 Shelf·운영자 Visual을 공개범위별 wrapper로 분리했다. Shop은 `MartPublicProduct`, Shelf는 `MarketInventory`에만 결속하며 actor Visual은 identity·권한을 소유하지 않는다. Unity Object 모판 5/5, Play Mode Console 오류 0건과 15개 2열 선택 UI·도심마트 Shop 1600×900 Game View, 서버 projector 22/22·portable mapper 23/23을 통과했다.
17. Market District의 공개상품 위치에 도심마트 Shop wrapper를 개별 배치하고 여섯 번째 placement receipt를 추가했다. `market.public-products.shop` anchor는 `MartPublicProduct:mart-product:sim.potato.public`만 참조하고 `MarketInventory`를 노출하지 않는다. Unity placement 6/6·기존 Scene 10/10, 서버 projector 23/23·portable mapper 24/24와 Market District 1600×900 Game View를 통과했다. Play Mode Console에는 로컬 턴마감 서버 미실행 `ConnectionError` 1건이 남았다.
18. Town District의 집단화 위치에 Cart Table wrapper를 개별 배치하고 일곱 번째 placement receipt를 추가했다. `town.orderer-group.grouping-cart-table` anchor는 `GroupingPreview:grouping-preview:sim.potato.town`만 참조하고 개인 의향 원문·자동 참여·주문 Command를 노출하지 않는다. Unity placement 7/7·기존 Scene 10/10, 서버 projector 24/24·portable mapper 25/25와 Town District 1600×900 Game View를 통과했다. Play Mode Console에는 로컬 턴마감 서버 미실행 `ConnectionError` 1건이 남았다.

다음은 배치 객체를 더 늘리는 `OBJ-7D`가 아니라 `OBJ-STUDY-1 감자 재배체 심층 연구`다. [배치 객체 심층 연구 우선순위](UnitySeedbedObjectDeepStudyPriority.md)의 10개 질문으로 현실 존재 이유, 업무 앞뒤, 데이터 연결, 금지 권한, 거리별 인지, 상태별 시각 변화, 주변 구성, 연결 관계, 사용자 행동과 실제 World 필요성을 먼저 판정한다. 감자 이후 14개 객체는 [배치 객체 수평 연구 대장](UnitySeedbedObjectHorizontalStudyMatrix.md)의 동일한 열을 사용해 책임 중복과 누락을 비교한다. `OBJ-7D 주민 관점 Visual Scene 이식`은 삭제하지 않고 연구 결과가 쌓일 때까지 보류한다.
