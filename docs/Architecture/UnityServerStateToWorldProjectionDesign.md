# 서버 상태에서 Unity World Projection으로의 설계

## 1. 목적

이 문서는 Ssalddel의 실제 EF `DbSet`, MongoDB 원장·문서와 서버 상태를 조사하고, 이를 Unity의 World Object, UseCase, SceneController와 View로 변환하는 기준을 정의한다.

핵심 결론은 다음과 같다.

> `DbSet`과 Unity Controller를 1:1로 대응하지 않는다. 서버의 영속 객체는 Unity에 표현할 현실 실체와 상태의 출발점이며, 서버가 권한과 공개 범위에 맞는 projection API를 만든 뒤 Unity가 사용자 과업과 World Zone 단위로 묶어 표현한다.

Web route는 Unity navigation과 panel handoff를 찾는 보조 자료다. World의 실체와 상태는 route 수가 아니라 canonical server state, ledger와 공개 projection에서 출발한다.

## 2. 조사 범위와 확인 결과

2026-08-08 현재 체크아웃의 직접 선언을 기준으로 조사했다. migration snapshot과 `bin`, `obj`는 제외했다.

| 저장 경계 | 명시적 객체 수 | 위치 | 비고 |
| --- | ---: | --- | --- |
| `SsalddelContext` | 148 DbSet | `Ssalddel.Infrastructure/Persistence/SsalddelContext.cs` | Identity 기반 주 업무 RDB Context |
| `AgriculturalFisheriesDbContext` | 33 DbSet | `Ssalddel.Infrastructure/Persistence/AgriculturalFisheries/AgriculturalFisheriesDbContext.cs` | 농수산 가격·공식 음식·재료 연구 데이터 |
| `TraditionalMarketDbContext` | 5 DbSet | `Ssalddel.Infrastructure/Persistence/TraditionalMarkets/TraditionalMarketDbContext.cs` | 전통시장·물류 거점·생활권 협의 |
| MongoDB store | 27 물리 collection | `Ssalddel/Services/**` | 공동 원장, 대화, 투표, 공동구매 초안·계획, 증적과 내부 log |

명시적 EF `DbSet`은 총 **186개**다. `ApplicationUser`와 ASP.NET Core Identity의 내부 table은 이 숫자에 포함하지 않는다.

### P7에서 추가된 농장 canonical 객체

초기 조사 시점에는 농장·센서·작물 운영 객체가 없었으나 2026-08-08 P7 slice에서 다음 canonical aggregate와 migration이 추가됐다.

- `농장`
- `농장구획`
- `재배작기`
- `농업센서`
- `농업센서관측`
- `농장작업`

생산자 관점 API는 인증 사용자가 소유한 농장만 반환하고 Unity `FarmTileView`, `CropView`, `SensorView`와 `FarmWorkerView`에 연결된다. 다만 다음 운영 경계는 아직 남아 있다.

1. 생성된 migration을 실제 운영 DB에 적용한다.
2. 실제 sensor ingestion과 보정·최신성 정책을 연결한다.
3. 승인 rule이 판정 상태·rule revision·근거 card와 한계를 생성하게 한다.
4. 농장작업 Command와 실제 인증 API runtime을 검증한다.

농사로 작목기술·품종·농작업일정은 여전히 `작물 기준정보`의 공개 근거일 뿐 특정 농장의 현재 재배상태를 뜻하지 않는다. `재배작기`는 공개 작물 기준 stable ID·source key와 실제 생육 상태를 별도 필드로 유지한다.

## 3. 변환 단위

### 3.1 금지하는 1:1 구조

```text
DbSet<재고이력> → 재고이력Controller
DbSet<입고상품> → 입고상품Controller
DbSet<피킹포장작업> → 피킹포장작업Controller
```

이 방식은 persistence 구조를 Scene에 노출하고, 사용자 과업 하나를 여러 Controller가 경쟁해 조율하게 만든다.

### 3.2 권장 구조

```text
DbSet / Mongo ledger / external observation
  → server aggregate and authorization
  → public or authorized Projection API
  → ApiModel
  → explicit Mapper
  → Repository and state store
  → Projection Model
  → task-oriented UseCases
  → Presenter and ScreenModel
  → Zone SceneController
  → View hierarchy
```

예를 들어 창고는 여러 영속 객체를 하나의 과업 중심 snapshot으로 조합한다.

```text
창고 + 입고요청 + 입고상품 + 재고이력
     + 출고묶음 + 출고예정 + 피킹포장작업 + 재고이동
        ↓ server authorization and projection
WarehouseOperationSnapshot
        ↓
창고상태조회UseCase / 입출고현황조회UseCase / 피킹상태조회UseCase
        ↓
창고SceneController
        ↓
WarehouseView
  ├─ WarehousePalletView
  ├─ ProductCrateView
  ├─ LoadingZoneView
  └─ StatusPanel
```

## 4. Projection 분류

각 서버 객체는 아래 중 하나 이상으로 분류한다.

| 코드 | 의미 | Unity 처리 |
| --- | --- | --- |
| `WorldObject` | 공간에 지속적으로 존재하는 실체 | stable-ID 기반 GameObject/View |
| `StateComponent` | 다른 실체의 상태·관계·이벤트 | parent object의 badge, animation, route 또는 status |
| `Panel` | 표·이력·상세처럼 2D가 적합한 정보 | World object에서 여는 ScreenModel |
| `WebHandoff` | 민감하거나 정밀 입력·운영이 필요한 업무 | 권한 있는 Web route로 인계 |
| `InternalOnly` | Outbox, 수집 Run, 처리 기록과 내부 log | Unity API에 직접 노출하지 않음 |
| `NoCanonicalSource` | 필요한 실체의 권위 있는 서버 모델이 없음 | simulation 또는 contract 설계 전까지 operational 표현 금지 |

Projection에는 별도로 공개 범위를 부여한다.

```text
Public
AuthorizedRole
ParticipantOnly
OwnerOnly
AdminOnly
Internal
```

`DbSet`이 존재한다는 사실은 Unity 공개 권한을 의미하지 않는다. 위치, 연락처, 결제, 계약, 급여, 개인정보 동의와 열람권은 projection 전에 서버에서 제거하거나 권한별로 분리한다.

## 5. Zone Controller 설계 원칙

SceneController는 Entity 종류가 아니라 사용자가 한 공간에서 보는 상태와 수행하는 과업을 조율한다.

| Zone Controller | 묶는 UseCase 예시 | 책임 |
| --- | --- | --- |
| `커뮤니티시장광장SceneController` | 공개게시판조회, 시장요약조회, 지역정보조회, 원장요약조회 | 여러 도메인의 공개 진입점과 portal 조율 |
| `공공데이터정보관SceneController` | 가격관측조회, 음식·재료근거조회, 지역경계조회 | provenance가 있는 관측·근거 표현 |
| `농장SceneController` | 농장조회, 센서조회, 작물상태조회, 근거조회 | canonical Farm/Sensor 계약이 생긴 뒤 농장 aggregate 표현 |
| `협동조합공간SceneController` | 공동원장조회, 투표조회, 역할조회, 공동구매계획조회 | 참여 범위에 맞는 원장·협의 상태 표현 |
| `시장주문SceneController` | 상품조회, 주문요약조회, 공급조건조회 | 상품, 주문과 공급 흐름 조율 |
| `도심물류센터SceneController` | 입고·분류·보관·출고·운송인계조회 | 물류센터 Zone과 권한별 운영 snapshot 조율 |
| `창고SceneController` | 창고조회, 입출고조회, 재고조회, 피킹조회 | 창고 운영 snapshot 표현 |
| `운송SceneController` | 기사·차량조회, 배차조회, 운송조회, 경로상태조회 | 물류센터와 다른 Zone 사이의 승인된 운송 projection 표현 |

하나의 SceneController가 모든 API를 직접 호출하지 않는다. 각 UseCase와 Repository port를 조합하고 Presenter가 View에 필요한 ScreenModel을 만든다.

### 5.1 공유 World와 Role Perspective

장소와 역할을 같은 축으로 모델링하지 않는다.

```text
Shared World
  ├─ Farm Zone
  ├─ Market Zone
  ├─ Residential Community Zone
  └─ Urban Logistics Center Zone
          │
          └─ server-authorized Role Perspective
              ├─ Producer
              ├─ Orderer
              └─ Transporter
```

장소는 stable-ID 기반 World Object와 canonical 상태를 소유한다. 역할은 같은 object 위에 강조, 역할별 label, 상세 panel 진입점과 허용 interaction을 겹친다. 역할을 바꿀 때 Scene이나 World View를 다시 만들지 않고 기존 Role View 상태를 지운 뒤 새 snapshot을 적용한다.

Controller 책임도 두 축으로 나눈다.

| Controller 종류 | 질문 | 책임 | 금지 |
| --- | --- | --- | --- |
| World/Zone Controller | 이 장소에 지금 무엇이 존재하고 어떤 상태인가? | object 생명주기, canonical Zone snapshot, loading/error, stable-ID reconcile | 사용자 역할을 근거로 개인정보나 권한을 추론 |
| Role Experience Controller | 현재 승인된 역할에 무엇이 중요하고 무엇을 할 수 있는가? | 역할별 조회 UseCase, object 강조, panel·interaction 목록 교체 | GameObject를 운영 권한의 근거로 사용하거나 서버에 없는 행동 생성 |

두 Controller는 stable ID와 서버 projection에서 만난다. 예를 들어 운송자의 배정 화물이 `package:123`이면 Role Experience Controller가 임의로 상차 가능 여부를 계산하지 않는다. 서버 snapshot이 `package:123` 강조와 허용된 `confirm-pickup` interaction을 함께 내려주고, 물류센터 Controller가 소유한 같은 stable-ID View socket에 이를 적용한다.

View는 세 층으로 구분한다.

| View 층 | 내용 | 역할 전환 시 처리 |
| --- | --- | --- |
| World View | 건물, 밭, 트럭, pallet, 상품상자처럼 누구에게나 존재하는 공간 실체 | 유지 |
| Role View | 내 작물·주문·배송 대상 강조, 상하차 marker, 다음 작업 badge | 전체 clear 후 승인 snapshot 적용 |
| Detail View | 가격, 근거, 원장, 재고, 배송·작업 상세 | 현재 snapshot과 viewer scope에 맞는 panel로 교체 |

Role Perspective 조회 흐름은 다음과 같다.

```text
authenticated session + requested role + current zone
  → server verifies active role assignment and task scope
  → authorized RolePerspective projection
  → Unity ApiModel
  → Mapper
  → Repository checks requested role and zone match
  → 역할관점조회UseCase
  → Role Experience Controller
  → RolePerspectiveApplicator
  → existing stable-ID Zone View sockets
```

요청한 role code는 권한 증명이 아니다. 권한이 없으면 서버가 거부해야 하며 다른 역할의 데이터로 조용히 대체하지 않는다. 응답에는 최소한 `AuthorizedRoleCode`, `WorldZoneCode`, `ViewerScope`, `AuthorizationDecisionId`, `Revision`, `GeneratedAt`, object 강조와 `AllowedInteractions`가 필요하다. 주소, 연락처, 다른 세대 주문과 정밀 위치는 projection 전에 서버가 제거한다. 운영 Command는 이 조회 결과만 믿지 않고 실행 시 권한과 expected revision을 다시 검증한다.

현재 engine-independent 계약은 `Ssalddel.Unity/Runtime/Perspectives/RolePerspectiveModels.cs`에 구현되어 있다. 첫 서버 adapter는 `GET api/v1/driver/world/zones/urban-logistics-center/perspective`이며 기존 기사 인증과 현재운송 조회를 재사용한다. 현재 배정 운송의 ID·상태·상차/하차 역할만 반환하고 주소·연락처·운임을 제외한다. 기존 `loading-perspectives`, `unloading-perspectives`, `warehouse-perspectives` 업무 API를 대체하지 않고 Unity용 최소 aggregate로 조합한다. UnityWebRequest adapter와 MonoBehaviour Role Experience Controller는 후속 연결 대상이다.

## 6. 서버 실체에서 Unity 표현으로의 1차 대응표

이 표는 개별 Entity의 최종 API를 확정하는 표가 아니라, 어떤 aggregate projection을 먼저 설계할지 결정하는 상위 카탈로그다.

| 서버 실체·상태군 | Unity 필요도 | 권장 Projection | Zone Controller | 대표 View | 기본 처리 |
| --- | --- | --- | --- | --- | --- |
| 농수산 가격 관측·비교 snapshot | 높음 | `AgriculturalPriceObservationProjection` | 공공데이터정보관 | `ObservationMarkerView`, `PriceBoardView` | Public panel/object; source·unit·observedAt 필수 |
| 공식 음식·재료·recipe·근거 | 높음 | `OfficialFoodEvidenceProjection` | 공공데이터정보관, 커뮤니티시장광장 | `InformationKioskView`, `EvidencePanel` | Public panel; 연구 주장과 제품 해석 분리 |
| 전통시장·물류 hub·생활권 협의 | 높음 | `TraditionalMarketWorldProjection` | 커뮤니티시장광장, 도심물류센터 | `MarketBuildingView`, `LogisticsHubView` | 시장은 object, sync run은 internal |
| 지역문화 source·행정구역·boundary | 높음 | `RegionalCultureMapProjection` | 공공데이터정보관, 커뮤니티시장광장 | `CultureExhibitView`, `MapTableView` | 공개 source와 지역 경계만 표현 |
| 공개 커뮤니티 post·comment·추천 | 높음 | `CommunityBoardProjection` | 커뮤니티시장광장 | `CommunityBoardView`, `PostPanel` | 공개 범위 projection 사용; 원문 Entity 직접 노출 금지 |
| 커뮤니티 원장·상태 이벤트·공개 projection | 높음 | `CommunityLedgerWorldProjection` | 협동조합공간, 커뮤니티시장광장 | `LedgerBoardView`, `WorkflowNodeView` | 참여자·공개 projection 분리 |
| 공동구매 계획·협상·운영 주체·물류 workflow | 높음 | `CollectiveActionProjection` | 협동조합공간, 시장주문 | `PlanningTableView`, `RoleSlotView` | draft와 확정 상태를 분리 |
| 업체·참여자·역할·업무 관계 | 중간 | `WorldParticipantProjection` | 관련 모든 zone | `ParticipantAvatarView`, `RoleBadgeView` | 최소 식별자·역할만; 연락처·민감 속성 제외 |
| 판매상품·채널출품·상품물류자산 | 높음 | `ProductWorldProjection` | 시장주문, 창고 | `ProductCrateView`, `ProductPanel` | 상품 master와 물류 상태를 분리 |
| 음식점·메뉴·마트 공개상품 | 높음 | `FoodMarketProjection` | 시장주문, 커뮤니티시장광장 | `MarketStallView`, `MenuBoardView` | 공개 profile·상품만 object/panel |
| 음식·마트 주문과 상태 이력 | 높음 | `OrderFlowProjection` | 시장주문 | `OrderBoardView`, `OrderStatusPanel` | ParticipantOnly; 상세 입력·결제는 Web handoff |
| 기사·차량·배차·운송원장·운송이벤트 | 높음 | `TransportOperationProjection` | 도심물류센터, 운송 | `DriverView`, `TruckView`, `RouteView` | 승인된 위치와 상태만; 자동 배차 확정 금지 |
| 창고·입고·재고·출고·피킹·이동 | 높음 | `WarehouseOperationProjection` | 창고 | `WarehouseView`, `PalletView`, `LoadingZoneView` | AuthorizedRole aggregate |
| 통관·HS code·공급 계약·발주 | 중간 | `TradeReadinessProjection` | 시장주문, 창고 | `DocumentDeskView`, `TradeStatusPanel` | 상태 요약만; 계약·신고는 Web handoff |
| 교육과정·학습 card·공통 콘텐츠 | 중간 | `LearningContentProjection` | 공공데이터정보관 또는 별도 학습 공간 | `ExhibitView`, `LearningPanel` | 공개 범위와 보상 상태 분리 |
| 금융·결제·정산·급여·수익 환류 | 낮음 | `FinancialStatusSummary` | 관련 zone의 제한 panel | `StatusBadge`, Web handoff | 금액 요약도 권한 필요; 수정은 Web |
| 사용자 설정·개인정보·연락처 동의 | 낮음 | `ViewerPreferenceProjection` | 전체 UI shell | 접근성·표시 설정 | 개인 공간 또는 Web; World object로 만들지 않음 |
| Outbox·수집 Run·처리 log·access log | 없음 | 없음 | 없음 | 없음 | `InternalOnly`; 운영 관측 도구에서만 사용 |
| 농장·sensor·작물 | 목표는 높음 | `FarmOperationProjection` | 농장 | `FarmView`, `SensorView`, `CropView` | 현재 `NoCanonicalSource`; contract 전에는 simulation만 |

## 7. API Projection 계약

Unity API가 EF Entity 또는 Mongo document를 그대로 직렬화하지 않는다. Zone 또는 과업 snapshot은 다음 공통 정보를 가져야 한다.

```text
ProjectionId
ProjectionType
StableId
Revision
ViewerScope
SourceType
SourceReferences[]
ObservedAt or EffectiveAt
FreshnessStatus
DataStatus
DisplayState
EvidenceCardIds[]
AllowedInteractions[]
CanonicalDetailHandoff
```

예시:

```text
WarehouseOperationSnapshot
  ├─ WarehouseSummary
  ├─ InventoryUnitProjection[]
  ├─ InboundProjection[]
  ├─ OutboundProjection[]
  ├─ PickingTaskProjection[]
  ├─ AllowedInteractions[]
  └─ Revision / EffectiveAt / ViewerScope
```

관계 table, 이력, join Entity와 Outbox를 각각 Unity object로 만들지 않는다. 이들은 aggregate snapshot의 상태, 관계, revision 또는 내부 동기화 근거가 된다.

## 8. UseCase와 Controller 경계

조회 UseCase는 사용자 질문 단위로 만든다.

- `창고상태조회UseCase`
- `창고입출고현황조회UseCase`
- `시장상품조회UseCase`
- `주문진행상태조회UseCase`
- `운송경로상태조회UseCase`
- `공동원장요약조회UseCase`
- `공공가격관측조회UseCase`

SceneController는 다음만 조율한다.

- Scene 진입과 취소 token
- 필요한 UseCase의 병렬 또는 순차 실행
- `Loading`, `Success`, `InitialLoadError`, `Refreshing`, `RefreshError`
- Presenter 결과를 View에 적용
- object 선택과 panel 열기
- 운영 action의 preview·확인·server Command·canonical 재조회

SceneController가 `DbContext`, EF Entity, Mongo document, raw DTO 또는 Synty Prefab 경로를 참조하면 경계 위반이다.

## 9. 구현 우선순위

각 단계는 `ScreenModel + Validator → UseCase → SceneController → View socket → PrimitiveSceneBuilder → headless 계약 test`를 하나의 완료 단위로 삼는다. PlayMode와 최종 graphic은 현재 코드 vertical slice의 필수 완료 조건이 아니다.

| 순위 | vertical slice | 첫 구현 범위 | 제외 범위 |
| ---: | --- | --- | --- |
| 0 | 도심마트 | 진열대 3개, 가격·재고·출처·상세 panel | 주문·결제 |
| 1 | 전통시장·공개 물류거점 | 시장 1개, `Pilot/Active` 거점 1개, 검증된 위치·기능·출처 | 동기화·관리자 상태 변경 |
| 2 | 공공데이터 정보대 | 관측 layer 1종, marker·값·단위·freshness | 내부 수집 Run |
| 3 | 커뮤니티 게시판 | 공개 post 3개와 상세 panel | 연락처·참여자 전용 정보 |
| 4 | 도심 물류센터 | 입고 Dock, 분류 Zone, 출고 대기, 운송 인계 요약 | 자동 확정 Command |
| 5 | 창고·재고 | 창고 1개, pallet 3개, 입·출고·피킹 상태 | 재고 수정·피킹 확정 |
| 6 | 운송·배송 | truck 1대, 승인된 경로 node, pickup·dropoff 상태 | 정밀 위치·자동 배차 |
| 7 | 협동조합 원장 board | 원장 1개, workflow node·relation·역할 요약 | Mongo 원본 직접 노출 |
| 8 | 주문 board | 주문·창고·운송 인계 상태 | 결제·주소·정산 |
| 9 | 농장·sensor·작물 simulation | FarmTile 4개, sensor 1개, 작물 1종 | operational 표시 |
| 10 | 농장 operational | 승인된 canonical server contract와 Repository adapter | contract 없는 추정 데이터 |

`도심 물류센터`는 Zone이다. `창고`, `상차장`, `차량 대기 Bay`는 그 안의 업무 object이거나 연결된 하위 공간이다. `차고`는 정비·차량 보관이 독립 과업이 될 때만 별도 object로 추가한다.

운영 interaction은 위 read-only slice가 완료된 뒤, 서버 Command와 권한·revision 검증이 존재하는 상태 전이만 연결한다. 성공 후에는 같은 canonical aggregate를 다시 조회하며 animation은 확정 결과만 표현한다.

## 10. EF DbSet 전수 목록

아래 목록은 Unity object 목록이 아니라 조사 누락을 막기 위한 persistence inventory다.

### 10.1 `SsalddelContext` — 142개

| 상태군 | 수 | DbSet Entity |
| --- | ---: | --- |
| 기사·업체·탐색 | 12 | `업체`, `배달기사`, `용달기사`, `기사근무`, `기사위치기록`, `기사월정산`, `기사운송대금지급요청`, `차량제원`, `탐색캠페인`, `탐색캠페인대상자`, `탐색캠페인응답`, `기사화주관계집계` |
| 배차·배달권 | 4 | `배차계획신청`, `기사배차`, `플랫폼배달권`, `원장배달권투영` |
| 운송 | 5 | `화주운송의뢰`, `화물요구조건`, `운송원장`, `운송이벤트`, `운송의뢰상품연결` |
| 운임·결제 | 3 | `운임구성`, `차량단가`, `결제` |
| platform 설정·Outbox·asset | 11 | `사용자Command기능설정`, `Command알림Outbox`, `배차추천알림Outbox`, `결제승인완료Outbox`, `기사지급Outbox`, `음식마트원장동기화Outbox`, `플랫폼View정책`, `사용자View설정`, `사용자행위로그`, `생성이미지작업`, `앱문맥이미지자산` |
| 참여자·HR·관계·동의 | 11 | `주문자프로필`, `살뜰참여자`, `살뜰참여자역할`, `HrRoleAssignmentRecord`, `HrRoleApplicationRecord`, `HrEmploymentContractRecord`, `HrPayrollScheduleRecord`, `WorkRelationshipSnapshotRecord`, `친구요청`, `연락처공개동의`, `관세사프로필` |
| 창고·재고·통관·HS | 17 | `창고`, `창고사용자`, `입고요청`, `입고상품`, `재고이력`, `출고묶음`, `출고예정`, `피킹포장작업`, `재고이동`, `통관절차`, `통관수임`, `통관조회연동`, `HsCodeCatalogVersion`, `HsCodeEntry`, `HsCodeEntryRiskTag`, `HsCodeClassificationCase`, `HsCodePlatformAgencyExperience` |
| 음성·영상·학습·지역·교육 | 27 | `Typecast음성`, `Typecast음성모델`, `Typecast음성용도`, `YouTube감시채널`, `YouTube채널영상`, `YouTube영상상품후보`, `HongikHakdangCardCollection`, `HongikHakdangCard`, `HongikHakdangCardCollectionItem`, `HongikHakdangCardImageVariant`, `HongikHakdangCardDeliveryPreference`, `HongikHakdangDailyCardSelection`, `HongikHakdangCardDeliveryOutbox`, `지역문화이미지Prompt`, `지역문화공공기관Source`, `지역농수산Map행정구역`, `지역농수산Map행정구역CodeAssignment`, `지역농수산Map행정구역Boundary`, `지역농수산Map지역Crosswalk`, `SsalddelMobilePushInstallation`, `교육과정`, `교육과정과목`, `교육과정양식`, `교육과정신청`, `교육과정등록`, `교육과정참석기록`, `교육과정과제제출` |
| 판매·음식·마트·공급 | 23 | `판매채널계정`, `판매상품`, `채널출품`, `상품식별코드맵`, `상품물류자산`, `상품상세이미지생성작업`, `상품판매이미지초안`, `감사메시지`, `음식점공개프로필`, `음식점메뉴`, `음식주문`, `음식주문상품`, `음식주문상태이력`, `음식점리뷰`, `음식운영정책`, `마트공개상품`, `마트주문요청`, `마트주문`, `마트주문상품`, `플랫폼공급조건계약`, `플랫폼공급조건계약품목`, `공급계약이용등록`, `조직개별공급발주` |
| 콘텐츠·수익·커뮤니티 | 29 | `살뜰공통콘텐츠`, `살뜰콘텐츠보상정책`, `살뜰콘텐츠시청세션`, `살뜰콘텐츠보상지급`, `PlatformRevenueEntryRecord`, `PlatformProfitReturnPolicyRecord`, `PlatformProfitReturnScheduleRecord`, `PlatformCommunityPost`, `PlatformCommunityPostTranslation`, `PlatformCommunityBoardRequest`, `PlatformCommunityPostAttachment`, `PlatformCommunityPostAttachmentComment`, `PlatformCommunityPostComment`, `PlatformCommunityPostRecommendation`, `CommunityPostEmailNotificationOutbox`, `CommunityKeywordSubscription`, `PlatformCommunityPostKeywordScan`, `CommunityKeywordNotification`, `CommunityKeywordNotificationDelivery`, `PlatformCommunityPostAudio`, `PlatformCommunityPostAudioSegment`, `PlatformCommunityPostAudioAccessLog`, `커뮤니티원장상태이벤트`, `커뮤니티활동공개Projection`, `커뮤니티활동처리기록`, `커뮤니티활동유료상세`, `커뮤니티활동상세열람권`, `커뮤니티활동상세구매`, `커뮤니티활동상세구매상태이력` |

### 10.2 `AgriculturalFisheriesDbContext` — 33개

| 상태군 | 수 | DbSet Entity |
| --- | ---: | --- |
| 가격·비교·포장 관측 | 20 | `UsdaNassPriceCollectionRun`, `UsdaNassPriceObservation`, `HsUsdaCommodityMapping`, `KamisPriceCollectionRun`, `KamisPriceObservation`, `Bls평균소매가격수집Run`, `Bls평균소매가격관측`, `국제농수산가격수집Run`, `국제농수산가격관측`, `UsdaAms시장가격수집Run`, `UsdaAms시장가격관측`, `UsdaAms연도상품Catalog`, `UsdaAms공개사업체수집Run`, `UsdaAms공개사업체Profile`, `UsdaAms공개사업체취급품목`, `주간국가농수산물비교Snapshot`, `주간국가농수산물비교항목`, `농수산물포장Fcl분석Snapshot`, `국내농산물경락가격수집Run`, `국내농산물경락가격관측` |
| 공식 음식·재료 | 9 | `OfficialFoodRecipeSource`, `OfficialFoodDish`, `OfficialFoodRecipeVariant`, `OfficialFoodRecipeCollectionRun`, `OfficialFoodIngredientCategory`, `OfficialFoodIngredient`, `OfficialFoodIngredientPriceMapping`, `OfficialFoodIngredientHsMapping`, `OfficialFoodRecipeIngredient` |
| 재료 업체 연구 | 4 | `OfficialFoodIngredientCompanyResearchRun`, `OfficialFoodIngredientCompanyProfile`, `OfficialFoodIngredientCompanyEvidence`, `OfficialFoodIngredientCompanySourceObservation` |

### 10.3 `TraditionalMarketDbContext` — 5개

| 상태군 | 수 | DbSet Entity |
| --- | ---: | --- |
| 전통시장·동기화·물류·협의 | 5 | `TraditionalMarket`, `TraditionalMarketSyncRun`, `TraditionalMarketLogisticsHub`, `전통시장생활권협의체`, `전통시장교역안건` |

## 11. MongoDB collection 전수 목록

MongoDB는 유연한 원장 원본과 draft·event·증적을 포함한다. 같은 document type을 세 개의 draft collection이 공유하므로 document type 수와 물리 collection 수가 다르다.

| 상태군 | 물리 collection 또는 document | Unity 기본 처리 |
| --- | --- | --- |
| 커뮤니티 원장 | `community_ledgers` (`커뮤니티원장문서`) | 권한별 ledger projection의 source |
| 원장 권한·공유·공개 | `community_ledger_role_access_policies`, `community_ledger_sharing_policies`, `community_ledger_disclosure_requests` | `ViewerScope` 판정 source; 직접 object 금지 |
| 원장 서명·투표 | `community_ledger_order_signatures`, `community_votes` | ledger state component 또는 제한 panel |
| 커뮤니티 대화 | `community_conversations`, `community_messages` | 참여자 panel; 메시지를 world object로 대량 생성하지 않음 |
| 공동구매 계획·협상 | `collective_procurement_plans`, `orderer_group_purchase_negotiations` | 협동조합 planning projection |
| 공동구매 운영·집단화 | `orderer_group_operating_entities`, `orderer_group_purchase_auto_groups` | role·group state component |
| 공동구매 이행 | `orderer_group_purchase_logistics_workflows`, `orderer_group_purchase_overseas_shipments`, `orderer_group_purchase_commerce_fulfillment_plans` | 물류·시장 workflow projection |
| 공동구매 draft | `orderer_group_purchase_producer_contact_drafts`, `orderer_group_purchase_producer_supply_offer_drafts`, `orderer_group_purchase_fulfillment_order_drafts` (`GroupPurchaseDraftDocument`) | draft badge/panel; 확정 상태와 분리 |
| 공급자 관심 | `orderer_supplier_interest_subscription_drafts` | OwnerOnly panel; 자동 연결 근거로 사용 금지 |
| dispatch log | `dispatch_recommendation_logs`, `dispatch_acceptance_logs` | 내부 감사 또는 제한 이력; 추천을 확정으로 표현 금지 |
| 탐색 event | `exploration_campaign_events` | campaign 상태 component; 개별 대상자 노출 금지 |
| 개인정보 동의 증적 | `application_privacy_consent_evidence` | `InternalOnly` 또는 Web handoff |
| 판매 page draft | `sales_page_drafts` | OwnerOnly panel; 공개 상품 object와 분리 |
| 콘텐츠 검토 | `official_news_review_ledgers`, `community_youtube_post_workspaces` | 검토자 Web panel 또는 승인된 공개 결과만 projection |
| 교육 제출 | `education_field_experience_submissions` | ParticipantOnly panel |

## 12. Page-to-World와의 관계

`PageWorldProjectionCatalog`는 폐기하지 않는다. 역할을 다음처럼 제한한다.

```text
Server-state catalog
  → 무엇이 World에 존재하고 어떤 상태를 갖는가

Page-to-World catalog
  → 사용자가 그 상태를 어디서 보고 어떤 panel/Web으로 이동하는가
```

server-state catalog가 object identity와 projection source를 결정하고, page catalog는 navigation, interaction entry와 handoff를 결정한다. 둘을 결합할 때도 stable ID의 원천은 route가 아니라 canonical server object 또는 projection이다.

## 13. 첫 구현 slice 권고

현재 바로 구현 가능한 첫 slice는 Farm/Sensor가 아니라 **이미 aggregate contract와 API가 있는 공개 세계 지도 관측**이다.

권장 순서:

1. `GET api/v1/community/world-map/observations`와 `커뮤니티세계지도SnapshotDto`를 기준 contract로 선택한다.
2. snapshot의 `Revision`과 observation의 `StableId`, source, evidence 시각, 위치 정밀도, freshness와 boundary notice를 Unity ApiModel에 보존한다.
3. explicit Mapper가 `커뮤니티세계지도ObservationDto`를 `WorldObjectProjection`으로 변환한다.
4. dataset·layer 선택은 Repository query 또는 조회 UseCase의 입력으로 두고 SceneController에 source별 분기를 넣지 않는다.
5. `공공데이터정보관SceneController`가 공개관측조회 UseCase 하나를 실행한다.
6. primitive `ObservationMarkerView` 또는 `InformationKioskView`로 표시한다.
7. initial load와 refresh failure, 중복 ID와 낮은 revision을 검증한다.

전통시장만 별도로 필요한 경우에도 이미 `전통시장MapMarkerReader`가 공개 가능 hub와 검증된 위치만 선별해 이 세계 지도 snapshot에 합류시키므로, Unity가 `TraditionalMarket`, `TraditionalMarketLogisticsHub`를 직접 조합하지 않는다. 이 slice로 DB·공공 source→서버 aggregate API→Unity Projection→Controller→View 경계를 검증한 뒤 Farm/Sensor canonical contract 설계로 넘어간다.

## 14. 완료 기준

- 새 DbSet 추가 시 inventory 누락을 탐지할 수 있다.
- 하나의 Entity가 반드시 하나의 Controller를 만들지 않는다는 원칙이 tests와 review 기준에 반영된다.
- 모든 Unity projection이 canonical stable ID, revision, viewer scope와 provenance를 가진다.
- SceneController는 DbContext·Entity·Mongo document를 참조하지 않는다.
- 공개, 참여자, 소유자, 관리자와 내부 projection이 분리된다.
- `InternalOnly`와 `WebHandoff` 객체가 World Object로 생성되지 않는다.
- Farm/Sensor는 canonical source가 생기기 전 operational 상태로 표시되지 않는다.
