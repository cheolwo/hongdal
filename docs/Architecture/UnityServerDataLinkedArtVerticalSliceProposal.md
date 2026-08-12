# Unity 서버 데이터 연계 미술 수직 슬라이스 모듈 제안

## 1. 목적

현재 `ThreeRegionHubJourney`에는 Farm·Town·Hub·City, 약 300m 구간, 도로변 생활 경관, 시간대, 차량·NPC와 야간 Simulation 연출이 있다. 다음 단계는 오브젝트를 더 배치하는 작업이 아니라, 서버가 알고 있는 한 품목의 상태가 이 공간의 미술·움직임·정보 카드로 일관되게 읽히도록 만드는 것이다.

첫 대상은 `감자`로 제한한다.

```text
서버의 감자 재배·가격·화물·창고 상태
  → 출처와 권한이 보존된 World Slice Snapshot
  → Unity Data 검증
  → Interpretation
  → ART5 움직임 + ART6 데이터 미술
  → Farm → Hub → City에서 같은 감자 여정을 읽는 경험
```

이 문서는 기존 서버 계약과 Unity 기반을 재사용해 첫 수직 슬라이스 모듈을 형성하기 위한 구현 기준이다. 2026-08-10 현재 `PVS0~PVS6` Gate를 구현해 서버 read projection, Unity Data·Interpretation·Farm 카드, 인증 HTTP adapter와 관계 검증형 Farm→Hub Simulation route까지 연결했다. 운영 서버에는 감자 재배/상품과 운송 화물을 잇는 canonical stable-ID 관계가 아직 없으므로 운영 Van 이동은 닫혀 있다.

## 2. 첫 슬라이스의 사용자 경험

사용자는 같은 감자를 세 공간에서 서로 다른 업무 의미로 본다.

1. Farm의 감자밭을 선택해 재배 상태와 공개 작물 기준, 최신 센서 판정을 본다.
2. Farm Yard의 감자 상자를 선택해 상품·국내 가격·가격 근거 카드를 본다.
3. 서버가 실제 화물 연결을 제공하거나 Simulation scenario가 선택된 경우에만 Van과 감자 상자가 Hub로 이동한다.
4. Hub에서는 입고 상태·보관 위치·작업 상태가 팔레트, Dock, 직원 동작으로 표현된다.
5. City에서는 서버가 공개한 판매 가능 상품과 가격 정보만 진열·카드에 표시한다.
6. 사용자가 어느 anchor를 선택해도 `상품`, `가격 관측`, `업무 화물`, `시각 asset`의 출처가 서로 구분된다.

첫 완료 장면은 “감자 상자가 움직인다”가 아니라 다음 질문에 답해야 한다.

- 이 감자는 어떤 서버 상태를 표현하는가?
- 가격은 어느 출처·시점·단위·시장 단계의 관측인가?
- Farm의 재배와 Hub의 화물이 실제로 연결됐는가, 아니면 Simulation인가?
- 현재 사용자가 볼 수 있는 정보와 실행할 수 있는 행동은 무엇인가?

## 3. 현재 재사용할 수 있는 기반

| 영역 | 현재 기반 | 이번 슬라이스에서의 역할 |
| --- | --- | --- |
| 생산자 농장 | `FarmProducerPerspectiveResponse` | 농장·구획·재배·센서의 authorized projection |
| 국내 가격 | `FoodPriceCrosswalkCatalog`, domestic price API | 감자 `0701`과 KAMIS 감자 관측 연결 |
| 화물 인계 | `CargoWarehouseHandoffResponse` | 운송중·도착·입고 완료와 NPC movement |
| 창고 | `WarehouseWorldSnapshotResponse` | 공개되지 않은 내부 정보를 제외한 재고·작업·입고 상태 |
| 도시 판매 | 기존 주문자용 공개 마트 상품 projection | 판매 가능 수량·판매가·기준 시각의 읽기 전용 표현 |
| Unity 선택 | `SelectionStateStore`, stable-ID wrapper | vendor prefab이 아닌 World Object 선택 |
| 카드 | `ConceptCardDeckPresentationModel` | Concept·Status·Reason·Action 카드 |
| 공간·미술 | `ThreeRegionHubJourney`, 시간대 Presenter | Farm·Hub·City anchor와 ART5·ART6 적용 대상 |
| 이동 | Cargo Journey·NPC·Vehicle presentation | snapshot이 결정한 상태를 NavMesh·Animator로 연출 |

현재 계약 사이에는 “이 Farm 재배작기의 감자가 이 Cargo와 이 Warehouse inventory를 거쳐 이 City 상품이 됐다”는 canonical 관계가 항상 존재하지 않는다. 이름이 모두 `감자`라는 이유만으로 하나의 실제 여정으로 합치면 안 된다.

### 3.1 PVS0 조사와 PVS1~PVS2 구현 결과

| 확인 항목 | 2026-08-10 결과 |
| --- | --- |
| Farm 재배작기 | `CropReferenceStableId`와 source key는 있으나 공통 `ProductStableId`가 없음 |
| Cargo·Warehouse | stable ID와 task 관계는 있으나 Farm 재배작기와 같은 상품임을 증명하는 관계가 없음 |
| 가격 | 감자 `0701`을 `ExactCommodity`로 조회할 수 있고 source·기간·단위·시장 단계를 보존 가능 |
| read 계약 | `감자생산유통WorldResponse`와 source lineage·linkage·limitation 구현 |
| 순수 Projection | `ProductOnly`, `Unverified`, 명시적 `SimulationLinked` Gate와 composite revision 구현 |
| authorized API | `GET api/v1/common/world/slices/potato-journey` 구현, `[Authorize]`와 Farm perspective scope 재사용 |
| 현재 operational 결과 | 재배 선택이 없으면 `ProductOnly`, 승인 범위의 재배 stable ID를 선택하면 `Unverified` |
| 의도적으로 비운 블록 | canonical 관계가 생기기 전까지 Cargo·Warehouse·Market은 `null` |

따라서 PVS2는 “감자라는 이름이 같으니 연결했다”는 성공 화면을 만들지 않는다. 현재 서버가 증명할 수 있는 상품·가격과 사용자가 선택할 수 있는 본인 재배작기를 같은 응답에 넣되, 둘의 관계가 아직 검증되지 않았다는 사실을 계약으로 보존한다.

## 4. 권위와 연결 원칙

### 4.1 네 종류의 사실을 분리한다

```text
Product identity       product:potato
Cultivation state      farm / plot / cultivation snapshot
Price observation      source / observedAt / unit / market stage
Operational journey    cargo / task / handoff / inventory revision
```

- Synty prefab 이름, 상자 개수, 차량 위치는 어느 사실도 확정하지 않는다.
- 상품명이 같아도 canonical relation ID가 없으면 운영 여정으로 연결하지 않는다.
- 가격 관측은 재고 가치, 계약 단가, 판매가 또는 농가 수취가격으로 재해석하지 않는다.
- Unity의 도착·충돌·animation 완료는 서버 Command나 Simulation Tick을 자동 발생시키지 않는다.

### 4.2 연결 상태를 데이터로 드러낸다

첫 조합 Projection은 다음 상태를 명시한다.

| `LinkageStatusCode` | 의미 | 화면 처리 |
| --- | --- | --- |
| `CanonicalLinked` | 서버 원장이 stable ID 관계를 제공 | 실제 업무 상태로 표시 |
| `SimulationLinked` | scenario·seed·revision이 연결 | `SIMULATION` 표식과 함께 연출 |
| `ProductOnly` | 상품·가격만 연결 | 화물 이동과 재고 수량 숨김 |
| `Unverified` | 이름·외형만 유사하고 근거 없음 | 환경 Object로만 표시 |
| `Unavailable` | 권한·오류·누락으로 확인 불가 | 마지막 정상값과 혼동하지 않는 상태 카드 |

운영 API 실패를 Simulation fixture로 대체하지 않는다. 두 모드는 동일 인터페이스를 사용할 수 있지만 source mode와 표시를 분리한다.

## 5. 제안 서버 모듈

첫 모듈의 가칭은 `감자생산유통World`다. 여러 원장을 Unity가 직접 임의 조인하지 않도록 서버가 authorized read projection을 제공한다.

```text
GET api/v1/common/world/slices/potato-journey
  → 인증·viewer scope 확인
  → Farm perspective 조회
  → product/HS crosswalk 조회
  → 국내 가격 observation 조회
  → 명시적 관계가 있는 Cargo/Warehouse/Market projection 조회
  → source별 revision과 linkage 판정
  → 감자생산유통WorldResponse
```

권장 코드 단위는 다음과 같다.

```text
Ssalddel.Contracts/Common/WorldProjection/
  감자생산유통WorldDtos.cs

Ssalddel/Application/WorldProjection/
  감자생산유통World조회UseCase.cs
  감자생산유통WorldProjector.cs

Ssalddel/Controllers/Common/
  감자생산유통WorldController.cs
```

새 Entity나 통합 원장을 먼저 만들지 않는다. 기존 source를 읽어 하나의 응답으로 조합하되, canonical relation이 없는 블록은 독립 source로 남긴다.

### 5.1 응답 골격

```text
감자생산유통WorldResponse
├─ StableId / Revision / GeneratedAt
├─ AuthorizedRoleCode / ViewerScopeCode
├─ SourceModeCode
├─ LinkageStatusCode / Limitations[]
├─ Product
│  ├─ ProductStableId / DisplayName
│  ├─ HsPrefix / MappingQualityCode
│  └─ MappingEvidence
├─ Farm
│  ├─ Farm / Plot / Cultivation stable IDs
│  ├─ GrowthStatusCode
│  └─ Sensor observations[]
├─ PriceObservations[]
│  ├─ MarketStageCode
│  ├─ ValueRange / UnitCode / CurrencyCode
│  ├─ ObservedAt / FreshnessStatusCode
│  └─ SourceKey / SourceRevision / Limitation
├─ CargoJourney?
│  ├─ Cargo / TransportTask / InboundTask stable IDs
│  ├─ HandoffStateCode
│  └─ Movements[]
├─ Warehouse?
│  ├─ Inventory / Task stable IDs
│  └─ Authorized quantities / location / status
├─ Market?
│  ├─ PublicProductStableId
│  ├─ SalePrice / AvailableQuantity
│  └─ InventoryObservedAt
└─ SourceLineage[]
```

`Revision`은 각 원장의 revision을 잃어버리는 단일 숫자가 아니라 composite revision 또는 source revision 집합을 검증할 수 있어야 한다. 캐시도 source lineage가 같은 경우에만 재사용한다.

### 5.2 서버가 반환하지 않을 것

- Synty prefab path, material, Animator parameter
- 실제 주소·좌표·연락처·생산자 개인 식별 정보
- 근거 없는 수확량·예상 매출·우선순위 점수
- KAMIS 가격을 계약 단가·마트 판매가로 바꾼 값
- Unity에서 계산한 도착·약탈·재고 변화
- 자동 발주·배차·결제·입고 완료 Command

## 6. Unity 수직 슬라이스 모듈

Unity는 서버 DTO를 곧바로 GameObject에 적용하지 않는다.

```text
PotatoJourney API model
  → Mapper + Validator
  → PotatoJourneyDataSnapshot
  → PotatoJourneyInterpreter
  → PotatoJourneyPresentationModel
  → Farm / Cargo / Hub / Market presenters
  → Synty visual adapters
```

권장 package 구조는 다음과 같다.

```text
Ssalddel.Unity/Runtime/PotatoJourney/
├─ Data/
│  ├─ PotatoJourneyApiModels.cs
│  ├─ PotatoJourneyDataSnapshot.cs
│  └─ PotatoJourneyMapper.cs
├─ Interpretation/
│  ├─ PotatoJourneyInterpreter.cs
│  └─ PotatoJourneyInterpretationModels.cs
├─ Presentation/
│  ├─ PotatoJourneyPresentationProjector.cs
│  └─ PotatoJourneyPresentationModels.cs
└─ Application/
   ├─ PotatoJourneyRepository.cs
   └─ PotatoJourneyQueryUseCase.cs
```

실제 Unity 프로젝트에는 다음 wrapper만 둔다.

```text
PotatoJourneySliceRoot
├─ FarmPotatoAnchorView
├─ FarmYardCargoAnchorView
├─ CargoVehiclePresenter
├─ HubInboundAnchorView
├─ HubInventoryPresenter
├─ CityMarketAnchorView
└─ ConceptCardDeckView
```

`SyntyPotato`, `SyntyVan`, `SyntyWarehouse` 같은 Domain 모델은 만들지 않는다. visual adapter가 `VisualKey`를 실제 prefab과 연결한다.

## 7. ART5·ART6 적용 방식

### 7.1 ART5 — 상태가 만드는 움직임

| 서버·Simulation 상태 | 허용되는 움직임 | 금지되는 추론 |
| --- | --- | --- |
| 재배 진행중 | 작물의 미세 흔들림, 농부 순찰 | 흔들림으로 생육률 계산 |
| 센서 `Dry` | 절제된 토양 상태 indicator, 점검 NPC route | 자동 관수 Command |
| 화물 `InTransit` | Van route 이동, cargo 고정 | NavMesh 도착으로 배송 완료 |
| `ArrivedAtWarehouse` | Dock 정차, 직원 접근·하차 연출 | 상자 충돌로 입고 처리 |
| `ReceivingCompleted` | 보관 위치로 팔레트·직원 이동 | 화면 상자 수로 재고 계산 |
| 판매 가능 | City 진열대의 제한된 생활 motion | KAMIS 관측을 판매가로 표시 |

### 7.2 ART6 — 데이터를 미술 언어로 번역

데이터는 네모난 UI만으로 표시하지 않고 공간에도 절제되게 반영한다.

- Farm: 선택된 감자 필지에 얇은 contour와 sensor condition marker를 표시한다.
- Farm Yard: 실제 cargo가 연결된 상자만 작은 seal·route marker를 가진다.
- Road: 활성 Cargo Journey에만 짧은 방향 pulse를 표시한다.
- Hub: quantity를 상자 수에 1:1로 대응하지 않고 `Empty/Low/Medium/High`처럼 서버가 허용한 표현 bucket으로 투영한다.
- City: 공개 판매 가능 상태만 진열 밀도와 카드에 반영하고 내부 예약·보관 수량은 숨긴다.
- Card: 출처·시각·단위·통화·시장 단계·mapping 품질을 항상 가격 숫자와 함께 표시한다.

시간대 색감은 데이터 의미색을 덮지 않는다. Night에서도 `Simulation`, stale, blocked, source badge의 명도 대비를 유지하고 Bloom이나 emissive로 수치의 중요도를 과장하지 않는다.

## 8. 상호작용과 실행 경계

첫 버전은 읽기 전용이다.

```text
World anchor 선택
  → selection revision 증가
  → slice 조회
  → snapshot 검증
  → 카드와 공간 highlight 적용
  → 다른 anchor 선택 시 이전 요청 취소
```

초기 Action Card는 `근거 보기`, `Farm/Hub/City 위치 보기`, `새로고침`만 제공한다. 수확·출하·배차·입고·발주·구매는 포함하지 않는다.

후속 실행 행동은 반드시 다음 경계를 별도 Gate로 통과한다.

```text
Preview
  → allowed interaction과 expected revision 표시
  → 사용자 명시적 확인
  → 서버 Command
  → canonical snapshot 재조회
  → Unity reconcile
```

## 9. 단계적 구현 순서

| 단계 | 범위 | 완료 Gate |
| --- | --- | --- |
| `PVS0` 기준선 | 현재 감자 asset·anchor·서버 route·권한·source 관계 inventory | 실제 연결과 추정 연결을 표로 분리 |
| `PVS1` 계약 | slice DTO, source lineage, linkage·freshness·error 상태 | contract test와 직렬화 호환성 통과 |
| `PVS2` 서버 Projection | authorized UseCase·Controller, 기존 source 조합 | 권한 밖 정보 미노출, 이름 기반 조인 없음 |
| `PVS3` Unity Data | API model·mapper·validator·repository | malformed·stale·revision 역행 거부 |
| `PVS4` Interpretation | 카드·anchor·route·quantity bucket 결정 | prefab·material을 참조하지 않는 headless test |
| `PVS5` Farm Vertical | 감자밭·상자 선택과 상품/가격 Deck | Farm 같은 카메라의 loading·ready·partial 증거 |
| `PVS6` Hub Journey | 연결된 경우에만 Van·Dock·입고 Presentation | 상태 변화가 snapshot 재조회로만 발생 |
| `PVS7` City 연결 | 공개 마트 상품 anchor와 가격·가용성 분리 | KAMIS 가격과 판매가가 명확히 구분 |
| `PVS8` ART5·ART6 마감 | motion, data marker, time-of-day 대비, 모바일 최적화 | Day/Night Game View·짧은 sequence·성능 결과 |

구현은 `PVS0 → PVS5`로 Farm 읽기 흐름을 먼저 닫고, canonical cargo 연결이 검증된 뒤 `PVS6~PVS7`을 연다. 서버 관계가 준비되지 않으면 Hub·City는 `SimulationLinked` fixture로만 검증한다.

### 9.1 2026-08-10 구현 결과

| 단계 | 구현 결과 | 검증 |
| --- | --- | --- |
| `PVS3` | 서버 응답 mirror model, source·stable ID·권한·revision·linkage validator, repository와 query use case 구현 | .NET headless 집중 테스트 9/9 |
| `PVS4` | 상품·가격·관계 근거를 `ConceptCardDeckPresentationModel`로 해석하고 Farm/상자 anchor와 `SimulationLinked`·`ProductOnly` 강조를 분리 | UnityEngine·Synty 참조 없는 headless test 포함 |
| `PVS5` | 별도 `PotatoJourneyFarmVerticalSlice` Scene에 실제 POLYGON Farm 감자 식재·감자 상자, 필지/상자 선택, 화면 카드와 source lineage 연결 | Unity EditMode 3/3, 연결 Editor Play Mode, Game View PNG |
| `PVS6` | Bearer 인증 `UnityWebRequest` adapter, nullable을 보존하는 Newtonsoft wire JSON 변환, loading·ready·partial·stale·error/last-success 상태와 cargo route Gate 구현. canonical 관계가 없으므로 별도 Scene의 Van은 `SimulationLinked` fixture로만 이동 | Unity Core 감자 집중 14/14, Unity EditMode 9/9, 연결 Editor Play Mode Game View |

PVS5의 기본 선택은 재배 필지 `SimulationLinked`다. 상자를 선택하면 같은 가격 관측을 보더라도 `ProductOnly`로 바뀌며, canonical cargo·inventory 관계가 없는 상태에서 route나 입고 완료를 만들어 내지 않는다. Scene의 presenter는 운영 실패를 fixture로 대체하지 않고 `SimulationFixture` source mode를 명시한다.

PVS6에서는 기존 운송–입고 handoff 원장이 존재하더라도 그것만으로 감자 상품과 연결하지 않는다. `CargoStableId`, `TransportTaskStableId`, `InboundTaskStableId`, handoff 상태가 들어 있고 linkage가 `CanonicalLinked` 또는 `SimulationLinked`일 때만 Hub route를 연다. 현재 Play Mode route는 `cargo:simulation-potato-hub-1`이며 화면에 `CANONICAL CARGO RELATION: NOT AVAILABLE`을 함께 표시한다. 인증 adapter의 wire parser는 검증했지만 실제 로그인 token과 실행 서버를 사용한 live 호출은 아직 검증하지 않았다.

### 9.2 BOOT0 게임 시작 메모리 준비

게임 시작 시 서버 DB 전체나 모든 Zone 상세를 적재하지 않고, 현재 authorization scope에서 허용된 `감자생산유통World` snapshot 한 개를 첫 bootstrap 단위로 사용한다.

```text
감자생산유통World API
  → PotatoJourneyMapper 검증
  → PotatoProductionDistributionWorldBootstrapLoader
  → PotatoProductionDistributionWorldMemoryStore
  → stable-ID node / relation index
  → semantic VisualKey
  → FarmVisualCatalog / UrbanVisualCatalog
  → Presenter가 필요할 때 Prefab reference 소비
```

- 메모리에는 Product·Cultivation·CargoVehicle·Warehouse·Market·PublicMarketProduct node와 명시적 relation만 둔다.
- `VisualKey`는 `farm.cargo.potato-box`, `urban.vehicle.van` 같은 Unity semantic key이며 서버 응답에는 Prefab 이름·`Assets/` 경로가 들어가지 않는다.
- 같은 authorization boundary의 같은 node는 instance를 유지하고, 변경·삭제만 change set으로 제공한다.
- 더 오래된 `GeneratedAt`과 같은 시각의 상충 revision은 snapshot을 적용하기 전에 거부한다.
- session·role·authorization boundary가 바뀌면 이전 메모리 instance를 재사용하지 않는다.
- Unity `PotatoProductionDistributionBootstrapPresenter`는 명시적으로 주입된 authenticated API client만 `Start`에서 사용한다. client가 없을 때 Simulation fixture로 대체하지 않는다.
- Farm·Urban catalog의 여섯 VisualKey가 실제 Prefab으로 resolve되는지는 Unity EditMode에서 검증한다.

### 9.3 DATA-ID0 공통 상품 identity

`product:potato`를 서버 내부 `CanonicalProductStableId`로 유지하고 KAMIS·HS·USDA AMS·농사로 코드를 출처별 relation으로 분리한다. KAMIS `100/152`는 확인된 source code, HS4 `0701`과 AMS `Potatoes`는 후보, 농사로 품목구분Code는 공식 근거가 없어 `Unlinked`다. Unity는 외부 코드나 품목명을 이용해 node를 합치지 않고 `CanonicalProductStableId`만 World product identity로 사용한다.

첫 구현은 `common-food-product-identity.v1`과 `api/v1/agriculture/products/common-identities` 조회 API다. `agri_common_food_product_identities`, `agri_common_food_product_code_relations`, `agri_common_food_product_code_relation_reviews` 원장에 relation의 `Confirmed`·`Candidate`·`Unlinked`, source key, code scheme, match quality, revision과 검토 근거를 보존한다. API는 DB projection만 읽고 코드 catalog는 migration 초기 seed와 World bootstrap 상수에 한정한다. 이름 기반 자동 확장과 검토 이력 없는 관계 승격은 금지한다.

### 9.4 DATA-ID2 기존 데이터 대조

`reconciliation-preview`는 현재 연도의 KAMIS 관측을 기준축으로 기존 HS crosswalk와 실제 AMS 연도별 Commodity catalog를 대조한다. 결과를 `CanonicalLinked`, `CandidateOnly`, `Unmapped`, `SourceConflict`로 분리하며 Preview 자체는 상품 identity나 relation을 저장하지 않는다. 같은 KAMIS 분류·품목코드에 여러 이름이 있으면 canonical 연결을 차단하고, 농사로 공식 품목구분Code crosswalk가 없으면 전 품목을 `Unlinked`로 유지한다.

2026년 로컬 DB read-only 검증에서는 KAMIS 96개 중 감자 1개만 `product:potato`에 canonical 연결됐고, 기존 HS·AMS 후보가 있는 59개는 `CandidateOnly`, 36개는 `Unmapped`, 코드·이름 충돌은 0개였다. 쌀 `111→HS 1006`, 양파 `245→HS 070310 / AMS Onions, Dry`, 사과 `411→HS 080810 / AMS Apples`는 후보이며 아직 canonical 상품이 아니다.

## 10. 테스트와 증거

### 10.1 서버·Core

- 요청 역할과 서버 승인 역할·viewer scope 불일치 거부
- Farm·Product·Cargo·Warehouse stable ID와 source revision 보존
- 이름이 같은 감자 record의 자동 조인 금지
- domestic price의 source·단위·통화·시장 단계·기준 시각 필수
- stale·partial·unavailable 응답의 명시적 상태
- Operational 실패 시 Simulation fallback 금지
- 개인정보·내부 계약·정산·주소 필드 부재 확인

### 10.2 Unity headless/EditMode

- 잘못된 stable ID, 중복 source, revision 역행 거부
- `CanonicalLinked`, `SimulationLinked`, `ProductOnly`별 Presentation 차이
- 상자 수·색·route가 PresentationModel에서만 결정됨
- Data/Interpretation assembly가 UnityEngine과 vendor asset을 참조하지 않음
- 선택 race와 취소, loading·partial·stale·error 상태
- 기존 Farm·Cargo·Warehouse·Concept Card 회귀

### 10.3 Runtime 시각 증거

같은 카메라와 해상도에서 다음을 남긴다.

1. Farm 감자 필지 선택 전·후
2. 국내 가격 Ready와 stale/partial 카드
3. Farm Yard cargo가 없는 `ProductOnly` 상태
4. `SimulationLinked` 또는 `CanonicalLinked` Van 이동
5. Hub 도착·입고 완료 두 snapshot
6. City 공개 판매 가능 상태
7. Day와 Night에서 동일 데이터 badge의 가독성

Scene View나 테스트 통과만으로 완료 처리하지 않고 Play Mode Game View와 실제 선택 동작을 함께 확인한다.

## 11. 성능 기준

- slice 응답은 현재 선택된 품목과 필요한 anchor만 포함한다.
- World 전체 prefab을 데이터 변화마다 다시 생성하지 않고 stable-ID reconcile을 사용한다.
- 가격·센서 polling 주기는 source 갱신 주기보다 빠르게 두지 않는다.
- 선택되지 않은 Card Deck과 원거리 route FX는 비활성화한다.
- cargo·marker·FX는 pooling하고 `MaterialPropertyBlock`으로 원본 material을 수정하지 않는다.
- 모바일에서는 data marker와 실제 shadow/light 비용을 별도 측정한다.

## 12. 완료 정의

첫 감자 수직 슬라이스는 다음 조건을 모두 만족할 때 완료다.

1. 한 서버 응답이 source별 provenance와 linkage 상태를 보존한다.
2. Farm 감자 anchor 선택에서 상품·가격·근거 카드가 열린다.
3. 실제 canonical 관계 또는 명시적 Simulation이 있을 때만 화물 여정이 보인다.
4. Hub·City의 표현이 각각 authorized 창고 상태와 공개 판매 상태를 넘지 않는다.
5. Unity 이동·animation·asset 수가 서버 상태를 변경하지 않는다.
6. Day/Night 미술 변화 속에서도 데이터 의미와 source badge가 유지된다.
7. 서버·Unity targeted test와 Play Mode Game View 증거가 남는다.
8. commit·push·배포·운영 데이터 연결 여부를 각각 분리해 보고한다.

## 13. 권장 첫 실행 범위

바로 Farm→Hub→City 전체를 구현하지 않고 첫 작업 묶음을 다음으로 제한한다.

```text
PVS0 inventory
  → PVS1 read contract
  → PVS2 server projection with ProductOnly + SimulationLinked fixtures
  → PVS3/PVS4 Unity headless flow
  → PVS5 Farm 감자밭·상자·가격 카드
```

이 Gate를 통과하면 현재 확보한 미술 자산이 단순 배경이 아니라 서버 데이터의 읽을 수 있는 표현 체계가 된다. 그 뒤 실제 canonical Cargo relation이 확인되는 순서대로 Hub와 City를 추가하면, 서버 구조를 왜곡하지 않으면서 ART5의 생활감과 ART6의 데이터 미술을 같은 수직 슬라이스 안에서 확장할 수 있다.
