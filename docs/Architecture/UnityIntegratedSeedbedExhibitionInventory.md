# Unity 통합 모판·전시관 EXH-0 현황 대장

## 1. 목적과 판정 기준

이 문서는 [Unity 통합 모판·전시관 제안](UnityIntegratedSeedbedExhibitionProposal.md)의 `EXH-0 현황 대장`이다. 2026-08-11 현재 코드와 보존된 검증 기록을 기준으로 전시 후보를 다음 네 증거 축에서 따로 판정한다.

현재 표의 Exhibit는 업무 관계를 설명하는 Story 후보로 유지한다. 실제 Scene에 심을 개별 Object의 O0~O6 판정은 [Object 리팩토링 계획](UnityIntegratedSeedbedExhibitionObjectRefactoringPlan.md)에 따라 별도 대장으로 분리할 예정이다.

개별 Object의 현재 판정은 [OBJ-0 Object 대장](UnityIntegratedSeedbedExhibitionObjectInventory.md)을 따른다.

| 증거 축 | `Verified` | `Partial` | `Unverified` | `NotApplicable` |
| --- | --- | --- | --- | --- |
| 코드 | 후보를 표현하거나 실행하는 계약·구현이 확인됨 | 일부 계층 또는 일부 상태만 구현됨 | 공통 구현을 확인하지 못함 | 해당 후보에 코드가 필요하지 않음 |
| 집중 test | 현재 후보의 핵심 경계를 검사한 통과 기록이 있음 | test는 있으나 최신 실행이 막혔거나 일부만 통과 | test 또는 통과 기록을 확인하지 못함 | 순수 조사 항목 |
| Runtime | Play Mode·Game View에서 핵심 동선을 확인함 | Scene·Game View는 있으나 최종 상호작용 일부가 미확인 | compile·headless 외 runtime 증거 없음 | 화면이 없는 계약 |
| 운영 연결 | 인증·실제 저장·provider·canonical 재조회까지 확인됨 | API 일부 또는 read-only projection만 연결 | Fixture/Simulation 전용이거나 live 연결 미확인 | 연구 전용 후보 |

`코드 Verified`는 전체 업무가 완성됐다는 뜻이 아니다. `Runtime Verified`도 운영 연결을 증명하지 않는다. 과거 검증 기록은 현재 재실행 결과와 구분해 `Recorded`로 설명한다.

## 2. 전시 후보 대장

| Exhibit stable ID | 구역·후보 | 코드 | 집중 test | Runtime | 운영 연결 | 다음 차단점 |
| --- | --- | --- | --- | --- | --- | --- |
| `exhibit:authority-lobby` | 출처·권위 로비 | `Partial` | `Partial` | `Partial` | `Unverified` | 여러 sample의 mode·source·revision 표시를 공통 manifest로 통합해야 함 |
| `exhibit:asset-lab:synty` | 신티 에셋 연구소 | `Verified` | `Verified/Recorded` | `Verified/Recorded` | `NotApplicable` | 원본 GUID 연구 기록을 업무 stable ID manifest 후보로 변환하는 adapter |
| `exhibit:turn-card-seedbed` | 턴 카드 모판 | `Verified` | `Verified/Recorded` | `Verified/Recorded` | `Unverified` | 사람 승인 publication과 C5 snapshot 0건 |
| `exhibit:public-data:hall` | 공공데이터 자료관 | `Verified` | `Verified/Recorded` | `Partial/Recorded` | `Partial` | 최신 Unity sample wiring·Game View와 provider별 live 상태 재확인 |
| `exhibit:public-data:potato-observation` | 감자 KAMIS·토양·기상 관측 | `Partial` | `Verified/Recorded` | `Verified/Recorded` | `Uncollected` | 승인된 실제 표본과 source revision; 미수집을 실패나 Fixture로 표시하지 않음 |
| `exhibit:farm:potato-lifecycle` | 감자 6×6 재배·수확 | `Verified` | `Verified/Recorded` | `Verified/Recorded` | `Unverified` | Fixture 작기 profile을 실제 농업 권고나 운영 재배 원장으로 승격하지 않음 |
| `exhibit:mobility:potato-cargo` | HarvestLot·Cargo Journey | `Verified` | `Verified/Recorded` | `Verified/Current` | `Partial` | EXH-3 Fixture 계보는 연결됨. authorized 운영 Cargo snapshot은 아직 미적재 |
| `exhibit:town:individual-intent` | 개별 의향·개별주문 | `Verified` | `Verified/Recorded` | `Partial/Recorded` | `Unverified` | 운영 주문 호출 없이 만든 Simulation 규칙과 실제 주문 projection 분리 |
| `exhibit:town:orderer-group` | 주문자 집단·같이주문 | `Verified` | `Verified/Recorded` | `Partial/Recorded` | `Partial` | privacy-safe 집계와 사람 승인·철회 상태의 Unity aggregate |
| `exhibit:hub:shipper-request` | 화주 운송 의뢰 | `Verified` | `Verified/Current` | `Verified/Current` | `Partial` | EXH-3의 request candidate는 Fixture이며 실제 화주 요청을 만들지 않음 |
| `exhibit:mobility:freight-transport` | 기사 추천·화물운송 | `Verified` | `Verified/Recorded` | `Verified/Recorded` | `Partial` | authorized Unity HTTP projection, 실제 배정·상하차·인수 재조회 |
| `exhibit:hub:warehouse` | 입고·검수·적재·피킹·포장·출고 | `Verified` | `Verified/Current` | `Verified/Current` | `Partial` | Fixture checkpoint는 연결됨. 운영 DB의 실제 handoff·Command·canonical 재조회는 미연결 |
| `exhibit:city:urban-market` | 도심마트 공개상품·후방재고·진열 | `Partial` | `Verified/Recorded` | `Verified/Recorded` | `Partial` | canonical shelf·location·task·allocation 운영 projection |
| `exhibit:town-city:orderer-group-urban-market` | 주문자 집단·도심마트 공개범위 composite | `Verified` | `Verified/Current` | `Verified/Current` | `Partial` | EXH-4 Fixture 경계는 연결됨. 별도 참여 동의·authorized 운영 마트 snapshot·Command는 미실행 |
| `exhibit:city:food-delivery` | 음식점·기사·주문자 인계 | `Verified` | `Verified/Current` | `Verified/Current` | `Partial` | EXH-5 Fixture 경계는 연결됨. authorized 운영 snapshot·Command·canonical 재조회는 미실행 |
| `exhibit:city:residential-pickup` | 주거공동체 수령 | `Verified` | `Verified/Recorded` | `Verified/Recorded` | `Partial` | 별도 pickup-point canonical reference와 privacy-safe group summary |
| `exhibit:evidence:lineage-room` | 계보·검증실 | `Partial` | `Partial` | `Unverified` | `NotApplicable` | 코드/test/runtime/운영 증거를 한 manifest에서 읽는 공통 projector |

## 3. 코드 근거 지도

### 3.1 EXH-3 composite 판정

`exhibit:logistics:cargo-hub-warehouse`는 위의 화주 의뢰·감자 Cargo·창고 후보를 새 원장으로 합친 것이 아니라, 기존 계약의 연결 가능성을 보여 주는 `Fixture/Simulation` composite다.

- 동일 Cargo stable ID를 7개 workflow checkpoint가 공유한다.
- canonical relation 5개가 화주 의뢰 후보에서 Warehouse World snapshot까지 단방향 chain을 이룬다.
- 모든 relation은 target revision과 expected target revision을 따로 가진다.
- `ArrivedAtHub`, `ArrivedAtWarehouse`, `ReceivingCompleted`는 서로 다른 상태이며 도착 animation으로 다음 상태를 확정하지 않는다.
- 운영 `CargoWarehouseHandoffProjectionBuilder`와 권한 있는 `WarehouseWorldSnapshot` 계약은 코드·test 근거로만 연결했다. 실제 운영 Cargo snapshot은 이번 단계에서 읽지 않았다.

현재 판정은 코드 `Verified`, 집중 test `Verified/Current`, Runtime `Verified/Current`, 운영 연결 `Partial`이다.

### 3.2 EXH-4 composite 판정

`exhibit:town-city:orderer-group-urban-market`는 개인 주문 원장과 마트 운영 원장을 합친 새 원장이 아니다. 기존 공개 계약의 인계 가능성과 공개범위 단절을 보여 주는 `Fixture/Simulation` composite다.

- `IndividualIntent`는 `OwnerPrivate`이며 본인 소유·철회 가능 상태를 유지한다.
- `GroupingPreview`와 `OrdererGroupSummary`는 `PrivacySafeAggregate`이며 Preview만으로 참여시키거나 확정하지 않는다.
- `MartPublicProduct`는 `OrdererPublic`, `MarketInventory`와 `ShelfTask`는 `MarketOperatorAuthorized`다. 공개 판매 가능 수량과 물리 후방재고 stable ID를 공유하지 않는다.
- KAMIS 관측과 마트 판매가 사이 관계는 `ComparedWithNotUsedAsSalePrice`다. 현재 KAMIS 표본은 미수집이며 판매가의 원천으로 주장하지 않는다.
- 운영 도심마트 Data 계약은 코드 근거로만 연결했다. 실제 authorized snapshot, 진열 작업 Command와 canonical 재조회는 이번 단계에서 실행하지 않았다.

현재 판정은 코드 `Verified`, 집중 test `Verified/Current`, Runtime `Verified/Current`, 운영 연결 `Partial`이다.

### 3.3 EXH-5 음식배달 composite 판정

`exhibit:city:food-delivery`는 화물운송이나 창고 인계의 상태 machine을 재사용하지 않는다. 기존 음식 주문·음식점·기사·주문자 계약을 하나의 stable-ID 계보로 읽는 `Fixture/Simulation` composite다.

- `FoodOrder → RestaurantPreparation → FoodDispatchQueue → DriverOffer → DriverAssignment → FoodPickupHandoff → FoodDeliveryHandoff → OrdererReceipt`의 단방향 관계를 유지한다.
- 기사 수락 전 `DriverOffer`는 `DriverCandidateApproximate`로 전달 권역만 제공하고, 수령인 상세는 노출하지 않는다.
- 기사 본인 수락 뒤 `DriverAssignment`, `픽업완료`, `전달완료`만 `AssignedDriverAuthorized`다.
- `전달완료`와 주문자의 `수령확인`은 서로 다른 canonical record와 별도 Confirm이다.
- 실제 authorized 음식배달 snapshot, 배차·픽업·전달·수령 Command와 canonical 재조회는 이번 단계에서 실행하지 않았다.

현재 판정은 코드 `Verified`, 집중 test `Verified/Current`, Runtime `Verified/Current`, 운영 연결 `Partial`이다.

### 3.4 공통 World·전시 기반

- [World Projection 모델](../../Ssalddel.Unity/Runtime/WorldProjection/WorldProjectionModels.cs)
- [Public Data surface](../../Ssalddel.Unity/Runtime/PublicData/PublicWorldSurfaceModels.cs)
- [역할 Perspective](../../Ssalddel.Unity/Runtime/Perspectives/RolePerspectiveModels.cs)
- [Concept Card](../../Ssalddel.Unity/Runtime/PresentationContracts/LearningCards/ConceptCardPresentationModels.cs)
- 별도 Unity 프로젝트의 `에셋원본Index`, `에셋연구Catalog`, `에셋공공관측Catalog`, `턴카드모판CatalogData`

### 3.5 Farm·상품·이동

- [감자 생산유통 서버 Projector](../../Ssalddel/Application/WorldProjection/감자생산유통WorldProjector.cs)
- [감자 재배 lifecycle](../../Ssalddel.Unity/Runtime/Farm/CanonicalProductCultivationLifecycleModels.cs)
- [감자 수확 Cargo lifecycle](../../Ssalddel.Unity/Runtime/Farm/CanonicalProductHarvestCargoLifecycleModels.cs)
- [Potato Journey](../../Ssalddel.Unity/Runtime/PotatoJourney/PotatoCargoJourneyLifecycleModels.cs)
- [Cargo presentation](../../Ssalddel.Unity/Runtime/PresentationContracts/Cargo/CargoJourneyPresentationModels.cs)
- [Cargo·창고 운영 handoff Projector](../../Ssalddel/Application/WorldProjection/CargoWarehouseHandoffProjectionBuilder.cs)
- [권한 있는 창고 World snapshot UseCase](../../Ssalddel/Application/Warehouse/창고WorldSnapshot조회UseCase.cs)

### 3.6 주문·집단·운송·창고·마트·배달

- [공동구매 자동집단화 Controller](../../Ssalddel/Controllers/Orderer/공동구매자동집단화Controller.cs)
- [주문자 집단 운영주체 Controller](../../Ssalddel/Controllers/Orderer/주문자집단운영주체Controller.cs)
- [화주 운송 의뢰 Controller](../../Ssalddel/Controllers/Shipper/01_Request/화주운송의뢰Controller.cs)
- [기사 운송 진행 Controller](../../Ssalddel/Controllers/Driver/05_Settings/기사운송진행Controller.cs)
- [창고 World snapshot contract](../../Ssalddel.Contracts/Common/WorldProjection/WarehouseWorldSnapshotDtos.cs)
- [창고 Unity 모델](../../Ssalddel.Unity/Runtime/Warehouse/WarehouseWorldModels.cs)
- [도심마트 Unity 모델](../../Ssalddel.Unity/Runtime/UrbanMarket/UrbanMarketModels.cs)
- [음식 주문 Controller](../../Ssalddel/Controllers/Food/음식주문Controller.cs)
- [음식배달 기사 업무 Controller](../../Ssalddel/Controllers/Driver/Food/음식배달기사업무Controller.cs)

## 4. EXH-1에 전달할 최소 계약

P0 판정 결과, 공통 manifest에는 다음 필드가 반드시 필요하다.

- `ExhibitStableId`, `ExhibitKindCode`, 한국어 표시 이름
- workflow·제품 버전·Perspective·authorization scope
- World·Zone·Object stable ID
- canonical record relation, 양쪽 revision과 expected target revision
- state machine·state·권위·별도 Confirm 경계를 보존하는 workflow checkpoint
- checkpoint별 본인 비공개·개인정보 제거 집계·주문자 공개·운영자 권한 공개범위
- source plan, source revision, projection revision, 기준 시각
- `Live`, `Cached`, `Fixture`, `Uncollected`, `Invalid`, `Failed` 데이터 상태
- `Research`, `ReadOnly`, `Simulation`, `OperationalHandoff` 경험 mode
- `Candidate`, `Linked`, `Verified`, `Blocked`, `Promoted` 완료 상태
- 허용 interaction intent와 block reason
- VisualKey·Pack role
- 코드·집중 test·Runtime·운영 연결의 네 evidence entry

`Uncollected`는 `Failed`와 다르다. 아직 승인·표본 수집을 하지 않은 공공데이터를 실패로 과장하지 않고, 호출 실패를 미수집으로 완화하지 않기 위해 별도 코드로 둔다.

## 5. EXH-0 완료 판정

`EXH-0`는 다음 범위에서 완료다.

- 16개 전시 후보를 stable ID로 등록했다.
- 모든 후보를 코드·집중 test·Runtime·운영 연결의 네 축에서 따로 판정했다.
- 현재 코드 근거와 다음 단절 지점을 기록했다.
- `EXH-1` 공통 manifest의 최소 필드를 도출했다.

이 완료는 기존 test 재실행이나 Unity runtime 재검증을 뜻하지 않는다. `Recorded` 증거는 이후 각 EXH 단계에서 현재 환경으로 다시 검증한다.
