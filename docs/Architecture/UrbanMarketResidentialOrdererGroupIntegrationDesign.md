# 도심마트 공동주택 주문자 집단 통합 설계

## 1. 문서 상태와 결정 범위

- 상태: `RG0` 기존 서버 재사용 조사 완료, 구현 전
- 기준일: 2026-08-09
- 목적: 공동주택 대표가 조율하는 같이 주문 수요를 기존 공동구매 원장과 도심마트 공급 경영 Simulation 사이에 연결한다.
- 비목적: 공동주택 전용 주문 원장, 대표의 일괄 주문 권한, Simulation 결과의 운영 원장 자동 승격

공동주택은 새 주문 도메인이 아니다. 기존 주문자 집단의 배송권·생활권·공동수령 문맥이며, 대표는 주민별 주문을 대신 확정하는 권한자가 아니라 집단 조건과 후속 인계를 조율하는 역할이다.

```text
기존 individual-demand 원장
  → 기존 공동구매 자동집단
  → 기존 공동구매 원장과 대표 역할
  → 주민별 기존 order 원장
  → 기존 group-order 집계 원장
  → 권한·개인정보가 제거된 집단 수요 Projection
  → 별도 도심마트 공급 Simulation
  → 확정 이행 뒤 기존 출고·운송·ResidentialPickup
```

이 문서는 [공동구매 수요 모집 Process Manager](GroupPurchaseDemandProcessManager.md), [주문자 같이 주문 흐름](../ProjectOverview/orderer-group-commerce-flows.md), [도심마트 지역 수요·주문 Simulation 설계](UrbanMarketDemandOrderSimulationDesign.md), [도심마트 공급 계약 경영 Simulation 설계](UrbanMarketSupplyManagementSimulationDesign.md)를 연결하는 기준 문서다.

## 2. RG0 ExistingServerReuseMap

### 2.1 개별 참여 의향

- 기존 타입: `공동구매자동수요등록Command`, `공동구매자동수요응답`, `공동구매내원함응답`
- 기존 원장: `CommunityLedgerTemplateKeys.IndividualDemand`
- 기존 Service/UseCase: `I공동구매개별원함원장Service`, `I공동구매자동집단화UseCase`
- 기존 상태: `InterestOnly`, `PaidReservation`, `Active`, `Withdrawn`
- 기존 API: `PUT|DELETE api/v1/orderer/group-purchase-auto-groups/demands/{demandSourceKey}`
- 재사용 방식: 주민 본인이 자신의 비구속 의향을 멱등 key와 `개별원함기대Revision`으로 저장·철회한다. 대표나 마트가 주민 대신 변경하지 않는다.
- 부족한 부분: 마트 관리자가 볼 수 있는 상품·단위·희망기간별 privacy-safe 집계 Projection은 없다.

### 2.2 주문자 집단과 공동구매 원장

- 기존 타입: `공동구매자동집단응답`, `공동구매자동집단요약응답`, `같이주문공개상세응답`
- canonical ID: `자동집단Id`; 원장 관계는 `커뮤니티원장Id`, `SourceGroupPurchaseLedgerId + AutomaticGroupId`
- 기존 상태: `CollectingDemand`, `ReadyToConfirm`, `Confirmed`, `RecruitmentClosedTargetNotReached`
- 기존 UseCase: `I공동구매자동집단화UseCase`, `I공동구매수요모집ProcessManager`
- 기존 원장: `CommunityLedgerTemplateKeys.GroupPurchase`
- 기존 API: `GET api/v1/orderer/group-purchase-auto-groups`, 관리자 OS `api/v1/admin/orderer/group-purchase-demand-os`
- 재사용 방식: 상품·배송권·온도·거래유형·가격표시기준이 맞는 기존 자동집단과 공동구매 원장을 사용한다. 공동주택은 배송권과 fulfillment context로 연결한다.
- 부족한 부분: `Confirmed`는 주민별 주문 확정이 아니라 모집 결과의 후속 원장 인계 승인이다. 이 상태만으로 hard demand를 만들 수 없다.

### 2.3 주민별 확정 주문과 같이 주문 집계

- 기존 타입/원장: `CommunityLedgerTemplateKeys.Order`, `CommunityLedgerTemplateKeys.GroupOrder`
- 기존 Service: `I공동구매개별주문원장Service`
- canonical ID: 개별 주문 원장 ID와 `SourceGroupPurchaseLedgerId + AutomaticGroupId` 기반 같이 주문 원장 ID
- 기존 집계: `ConfirmedIndividualOrderCount`, `ConfirmedOrdererCount`, `TotalRequestedQuantity`, `QuantityUnit`, `TotalReservedPaymentAmount`
- 재사용 방식: hard demand는 `GroupOrderRequiresIndividualOrders` 규칙을 통과한 유효 개별 주문 집계에서만 읽는다. 집계 수량을 별도 필드로 사람이 입력하지 않는다.
- 부족한 부분: 마트/Unity용 authorized aggregate와 revision·source lineage 계약이 없다.

### 2.4 대표와 역할 슬롯

- 기존 역할: `CommunityLedgerTemplateKeys.GroupPurchase`의 `공동구매 대표`
- 기존 권한: 합의된 조건을 주문집계로 인계하기 위한 `ChangeState`, `AttachEvidence`
- 기존 운영주체 문맥: `주문자집단운영주체Dto`, `주문자집단운영주체유형코드.관리사무소위임`
- 기존 공개 API: `GET api/v1/orderer/orderer-group-operating-entities/{ordererGroupScopeKey}`; 대표 User ID·이름은 공개 DTO에서 제외된다.
- 재사용 방식: 공동구매 원장의 역할 배정이 조율 capability의 기준이고, 운영주체는 해당 배송권의 생활권·법적 운영 문맥만 보조한다.
- 부족한 부분: 현재 일반 사용자 API에는 `공동구매 대표` 역할과 특정 자동집단을 검증해 마트 문의를 허용하는 capability가 없다. 운영주체의 `대표UserId`만으로 권한을 부여하지 않는다.

`Representation != IndividualOrderAuthority`를 불변식으로 둔다. `ChangeState`가 있어도 다른 주민의 개별 주문 원장을 확인·변경·결제할 권한으로 확장하지 않는다.

대표는 다음 세 층을 분리한다.

```text
사회적 World Context
  주민자치 대표 / 입주자대표회의 대표 / 관리사무소 조정자
  = 세계 안에서 누구를 대표해 설명하고 조율하는가

Canonical 업무 Role
  기존 GroupPurchase 원장의 공동구매 대표
  = 서버가 어떤 조회·초안·인계 capability를 허용하는가

Unity Presentation Role
  ResidentialGroupRepresentative NPC
  = 승인된 집단 상태를 어떤 캐릭터·이동·대화로 표현하는가
```

사회적 명칭만으로 권한을 만들지 않는다. `주민자치 대표`, `입주자대표회의 대표`, `관리사무소 조정자`는 verified context가 있을 때 표시할 수 있는 label이며, 모두 별도의 공동구매 역할 배정과 서버 capability 검증이 필요하다. 첫 Simulation fixture는 실제 법적 직함을 주장하지 않는 `ResidentialCommunityRepresentative` context와 표시명 `주민자치 대표`를 사용한다.

### 2.5 집단 Projection

- 기존 공개 contract: `공동구매자동집단요약응답`, `같이주문공개상세응답`
- 기존 본인 contract: `공동구매자동집단사용자응답`, `공동구매자동본인수요응답`
- 재사용 방식: 공개 집계의 상품·배송권·참여자 수·총희망수량과 같이 주문 원장의 확정 집계를 source로 사용한다.
- 부족한 부분: 공개 Projection은 마트 업무 권한, 희망 fulfillment window, pickup stable ID, 확정 주문 집계 revision과 source lineage를 함께 제공하지 않는다. 기존 DTO를 마트 관리용으로 과다 확장하지 않고 최소 authorized Projection을 additive하게 둔다.

### 2.6 마트 상품

- 기존 API: `GET api/v1/orderer/mart/products`
- 기존 UseCase: `I마트공개상품조회UseCase`, `마트공개상품조회UseCase`
- 기존 contract: `마트공개상품조회Dtos`
- 재사용 방식: 대표가 주문자에게 공개된 상품, 판매가, 판매 가능 수량과 재고 기준시각을 탐색하는 시작점으로만 사용한다.
- 경계: `판매가능수량`은 주문자용 공개 Projection이며 내부 보관재고·진열재고·예약재고·선반 상자 수가 아니다. 마트 관리자의 공급 판단 원본으로 사용하지 않는다.

### 2.7 공동수령

- 기존 업무 API: `GET api/v1/unloading-perspectives/orderer|transport|community-ledgers/{communityLedgerId}`
- 기존 Service: `IUnloadingPerspectiveReadService`, `UnloadingPerspectiveReadService`
- 기존 Unity Projection: `IResidentialPickupPerspectiveUseCase`, `ResidentialPickupPerspectiveResponse`
- 역할별 route: `api/v1/orderer/world/zones/residential-pickup/perspective`, `api/v1/driver/world/zones/residential-pickup/perspective`
- Pickup stable ID: `residential-pickup:{출고예정Id}`
- canonical task stable ID: `unloading-task:{운송원장Id}.{출고예정Id}`
- 권한 필터: 주문자는 `출고예정.주문자UserId`, 운송자는 화주·추천 기사·확정 기사와 운송원장 관계, 공동원장 관점은 원장 접근 검사를 통과해야 한다.
- 재사용 방식: 확정된 fulfillment가 출고·운송 원장에 연결된 뒤에만 공동수령 object를 만든다.
- 부족한 부분: 현재 label은 `공동 수령지` 또는 `지정 수령지`이고 별도 pickup-point canonical ID를 직접 보존하지 않는다. RG6에서 기존 출고·운송 관계를 깨지 않는 명시적 pickup reference가 필요한지 검토한다.

### 2.8 공급계약

- canonical 원장: `플랫폼공급조건계약 → 공급계약이용등록 → 조직개별공급발주`
- 기존 UseCase: `플랫폼공급계약관리UseCase`, `I조직개별공급발주UseCase`, `조직개별공급발주UseCase`
- 기존 API: `api/v1/supply-brokerage/agreements`, `.../participations`, `api/v1/supply-brokerage/orders`
- 멱등성: 이용등록과 발주의 `클라이언트요청Id`
- 동시성 경계: 철회·공급자 응답의 `기대상태코드`; 계약 문서 버전 일치 검증
- 재사용 방식: Operational 마트만 서버 Claim으로 확인된 조직 범위에서 이용등록하고 발주마다 별도 확인한다.
- 경계: Simulation 계약 확정은 이 원장들을 생성하지 않는다. 운영 연결은 기존 UseCase 호출 뒤 canonical 목록/상세 재조회로 닫는다.

### 2.9 기존 생산자 문의·제안 초안

- 기존 API: `api/v1/orderer/domestic-group-purchases/{campaignId}/producer-connections`
- 기존 Service: `IDomesticGroupPurchaseProducerConnectionService`
- 기존 기능: 생산자 연락 요청 초안, 공급 제안 초안, 적합성 preview, 공동구매 원장 block 기록
- 재사용 가능한 부분: 실제 발송·연락처 공개·자동구매를 하지 않는 초안 경계와 원장 증거 연결 방식
- 직접 재사용하지 않는 부분: 현재 contract는 생산자↔공동구매 대표 방향이며 마트 문의 authority, expected revision, 대표 역할 검증을 제공하지 않는다. 이를 공동주택-마트 견적 Command로 간주하지 않는다.

## 3. 수요 의미와 canonical source

세 수요를 한 값으로 합치지 않는다.

| 수요 | source | 허용 용도 | 금지 |
| --- | --- | --- | --- |
| 공공 잠재수요 | `지역인구SimulationBasisDataSnapshot → 지역잠재수요WorldState` | Simulation scenario 선택 근거 | 인구·세대를 상품 kg으로 직접 환산 |
| 집단 의향 수요 | 활성 `individual-demand`와 자동집단 집계 | 문의·모집·공급 가능성 preview 신호 | 확정 주문 또는 hard demand 처리 |
| 집단 확정 수요 | 유효 개별 `order` 원장을 합산한 `group-order` | Operational 집단 수요와 Simulation hard demand 입력 | 대표·관리자가 독립 수량 입력 |

따라서 공급 Simulation의 합성은 다음으로 고정한다.

```text
BaseScenarioDemand      = 명시적 4주 Simulation 가정
GroupIntentDemand       = 비구속 의향, 별도 표시
GroupConfirmedDemand    = 유효 개별 주문 집계

HardDemand = BaseScenarioDemand + GroupConfirmedDemand
```

`GroupIntentDemand`에 conversion rate를 적용해 자동으로 확정 수요를 만들지 않는다. `공동구매자동집단상태코드.확정`도 hard demand 조건으로 쓰지 않는다.

## 4. 최소 Data와 개인정보 경계

RG2에서 필요할 때만 `도심마트주문자집단수요DataSnapshot` 같은 Simulation/authorized Projection 계약을 추가한다. canonical 공동구매 원장을 복제하지 않으며 다음 최소 필드만 고려한다.

- `GroupStableId`, `GroupTypeCode`, `ProductStableId`, `StatusCode`
- `IntentParticipantCount`, `IntentQuantity`
- `ConfirmedParticipantCount`, `ConfirmedQuantity`, `QuantityUnitCode`
- `RequestedFulfillmentWindow`, optional `PickupPointStableId`
- `RepresentativeRoleState`, `AvailableActionCodes`
- `Revision`, `GeneratedAt`, `ViewerScope`, `SourceLineage`, `Mode`

마트 관리자와 Unity 계약에는 다른 주민의 user ID, 이름, 연락처, 상세주소, 동·호수, 주민별 수량, 개별 결제 상세와 공동 원장 원문을 넣지 않는다. 대표 Perspective도 집계와 본인의 action만 제공하며 대표라는 이유로 주민 개인정보를 확대하지 않는다.

## 5. Shared World 결합

기존 공급 graph의 relation semantic을 확장해 주문자 집단을 억지로 넣지 않는다.

```text
도심마트주문자집단WorldGraph
  OrdererGroup → DemandRequest   RaisesDemand
  DemandRequest → Product       RequestsProduct
  DemandRequest → PickupPoint?  RequestsFulfillmentAt

도심마트공급SimulationWorldGraph
  Supplier → Offer              Provides
  Offer → Product               Targets
  ContractDraft → Supplier      ProvidedBy
  ContractDraft → Product       Covers

상위 Shared World 결합 key
  ProductStableId + QuantityUnitCode + 기간
```

relation 이름은 실제 소비자가 생기는 RG2에서만 추가하고, node-kind 조합·dangling·duplicate relation 검증을 함께 넣는다.

## 6. 역할별 Perspective와 Command 경계

### 주민

- 본인 의향·본인 확정 주문·본인 수령 요약만 조회한다.
- 저장·철회·확인은 본인 인증과 expected revision을 통과한다.

### 공동주택 대표

- 집계 의향·확정 수량, 마트 제안 상태, 공동수령 후보와 대표 action을 조회한다.
- 문의 초안, 조건 전달과 수령지 조율은 가능하지만 다른 주민의 의향·주문·결제는 변경하지 않는다.
- `ConfirmAllMembers`와 동등한 Command는 만들지 않는다.

### 마트 관리자

- 상품·단위·희망 기간·의향/확정 집계·fulfillment 요약만 조회한다.
- `ReviewOrdererGroupDemand`는 수요 확정 Command가 아니라 공급 가능성 판단 Intent다.
- 공급계약 선택과 발주는 기존 마트 조직 권한 및 공급중개 UseCase를 통과한다.

### 공동주택 대표 NPC

공동주택 대표는 RG1부터 명시적인 Simulation actor로 존재하고 RG4-NPC에서 Unity Presentation vertical slice를 갖는다. 후보 계약 이름은 다음과 같다.

- 사회적 context: `ResidentialCommunityRepresentative`
- canonical role code: 기존 `GroupPurchaseRepresentative`
- NPC actor role code: `ResidentialGroupRepresentative`
- synthetic NPC stable ID: `npc:sim:residential-group-representative:1`
- View 후보: `공동주택대표NpcView`

NPC는 집단 수요 확인, 문의 준비, 마트 방문, 관리자 검토 대기, 조건 전달, 공동수령 조율을 시각화한다. NPC GameObject나 Animator가 권한 source가 되지 않으며, 주민·대표 User ID나 실제 이름을 Unity stable ID와 label에 넣지 않는다.

## 7. 공동주택 대표 NPC 이동과 대화

### 7.1 기존 공통 계약 재사용

새 이동 framework를 만들지 않는다. 다음 기존 흐름을 그대로 사용한다.

```text
명시적 Simulation actor state 또는 authorized operational task
  → NpcMovementApiModel
  → NpcMovementMapper
  → NpcMovementSnapshot
  → NpcMovementInterpreter
  → NpcMovementPresenter
  → 공동주택대표NpcView
  → semantic waypoint Transform
  → NavMeshAgent + Animator
```

`NpcMovementSnapshot`은 하나의 `WorldZoneCode`를 가지므로 공동주택과 마트 waypoint를 한 route에 섞지 않는다. 상위 `RepresentativeVisitStableId`가 다음 두 leg를 묶는다.

```text
residential-group-representative-briefing
  residential.community-office
    → residential.community-board
    → residential.departure-point

market-group-representative-consultation
  market.entrance
    → market.manager-desk
    → market.exit
```

실제 route code와 waypoint는 RG4-NPC에서 `ZoneNpcRouteCatalog`에 추가하며 기존 `residential-orderer-pickup`, `residential-distribution-round`, `market-orderer-browse`, `market-stock-clerk-round`를 변경하지 않는다. Zone 간 전환은 두 movement snapshot 사이의 journey stage 전환으로 표현하고 서버가 Unity `Vector3`를 보내지 않는다.

### 7.2 업무 상태와 Presentation 상태

| 업무/Perspective 상태 | NPC 표현 | 금지되는 자동 효과 |
| --- | --- | --- |
| 집단 수요 검토 | community board에서 집계 확인 | 주민 의향·주문 변경 |
| 문의 초안 준비 | community office에서 문서 준비 animation | 마트 문의 제출 |
| 마트 검토 요청 | departure point에서 market entrance leg로 전환 | 주문·계약 확정 |
| 관리자 검토 대기 | manager desk에서 대기·대화 animation | 공급계약 선택·발주 |
| 조건 전달 | community board로 돌아와 결과 안내 | 주민 일괄 확인 |
| 공동수령 조율 | pickup point에서 수령 안내 | 하차·수령 완료 Command |

`ArrivalActionCode`는 `ReviewDemandBoard`, `WaitForManagerReview`, `RelayOffer`, `CoordinatePickup` 같은 animation 입력이다. 도착 event, 대화 종료 event 또는 Animator event는 server/simulation Command를 호출하지 않는다.

첫 playable에서 마트 관리자는 플레이어 역할이다. 대표 NPC는 `market.manager-desk`에서 플레이어에게 문의를 제시하며, 별도 점장 NPC를 중복 생성하지 않는다. 자동 시연 모드에서 점장 캐릭터를 NPC로 표현하더라도 같은 `MarketManager` Perspective의 Presentation일 뿐 별도 권한자가 아니며 대화 완료가 계약 확정이 되지 않는다.

### 7.3 대화와 명시적 확인

```text
대표 NPC 도착
  → 문의 dialogue와 Concept Card Deck 열기 가능
  → 플레이어가 의향·확정 수요와 source lineage 검토
  → Preview
  → 사용자 명시적 확인
  → Simulation state transition 또는 기존 Operational capability
  → 새 canonical/Simulation snapshot 재조회
  → 다음 NPC movement snapshot
```

역할이 해제되거나 authorization decision이 바뀌면 다음 refresh에서 대표 Perspective와 허용 action을 제거한다. 이미 보이는 NPC는 stale 권한으로 Command를 실행하지 못하며 stable-ID reconcile로 숨기거나 비활성 상태로 전환한다.

첫 대표 deck은 집단 주문 상태, 확정 수요, 의향 수요, 공동수령, 공급 상태, 공급 부족 근거와 공급 검토 행동의 일곱 카드를 제공한다. 대표 NPC는 이 deck의 공간 anchor이며 주민 개인정보나 개별 주문 권한의 source가 아니다. 공통 카드 구조는 [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)을 따른다.

## 8. Simulation fixture

첫 fixture는 실제 공동주택·주민 데이터를 쓰지 않는다.

```text
Mode                         Simulation
OrdererGroupStableId         orderer-group:residential:potato:1
ContextCode                  ResidentialCommunity
ProductStableId              product:potato
IntentParticipantCount       67
IntentQuantity               410 kg
ConfirmedParticipantCount    61
ConfirmedQuantity            385 kg
RequestedPickupPointStableId pickup-point:residential:sample-1
RepresentativeRoleState      AssignedSimulatedCoordinator
RepresentativeContextCode    ResidentialCommunityRepresentative
RepresentativeDisplayLabel   주민자치 대표
RepresentativeNpcStableId    npc:sim:residential-group-representative:1
RepresentativeVisitStableId  representative-visit:sim:potato:1
```

필요하면 `participant:sim:001` 형태의 synthetic ID만 사용한다. fixture는 실제 공동구매·주문·결제·공급계약 ID로 승격되지 않는다.

## 9. 재정렬된 구현 순서

기존 SC1-C를 폐기하지 않고 기본 방문 수요 stream으로 범위를 명확히 한다. 공동주택 수요는 그 뒤 별도 source로 합성한다.

| 순서 | 단계 | 결과 |
| --- | --- | --- |
| 완료 | RG0 | 이 문서의 ExistingServerReuseMap과 canonical gap 확정 |
| 완료 | SC1-C | 기본 방문 Demand Scenario를 4주 56건 deterministic synthetic order stream으로 변환 |
| 완료 | RG1 | 감자 공동주택 집단 fixture, 대표 사회적 context·canonical role·NPC identity |
| 완료 | RG2 | 집단 수요 Data·typed graph; intent/confirmed source 분리 |
| 완료 | RG3 | `BaseScenarioDemand`, `GroupIntentDemand`, `GroupConfirmedDemand` composition |
| 완료 | SC2 | 합성 hard demand를 소비하는 4주 주문·공급 Engine |
| 완료 | RG4 | 대표·주민·마트 관리자 Perspective와 inquiry/dialogue state |
| 완료 | RG4-NPC-A + SC3~SC5 headless | 대표 NPC 두 Zone leg·visit state와 브리핑·계약 board 입력 모델 |
| 코드 완료 | RG4-NPC-B + SC5 Unity binding | `공동주택대표NpcView`, manager desk 대화, surface mapper/applicator; imported sample·Scene wiring 잔여 |
| 완료 | CC0 | 공통 카드 문법, 계층, 개인정보·권한·asset 경계 확정 |
| 완료 | CC1 | 공통 카드 계약·Projector와 권한·lineage·selection 검증 |
| 완료 | CC2 | 대표 NPC 7-card adapter와 집단/공급 source·권한 검증 |
| 1 | CC3 + RG4-NPC-C | 기존 Unity project sample 재가져오기·카드/Scene wiring·NavMesh/Animator 검증 |
| 2 | SC6 | Simulation 계약 confirm·tick·새 Snapshot 폐루프 |
| 9 | SC7 | 납품·재고·진열·주문 충족·UM4 queue와 대표의 결과 전달 연결 |
| 10 | RG5 | 기존 공동구매/group-order의 authorized Operational Projection adapter |
| 11 | RG6 | fulfillment에서 기존 ResidentialPickup stable ID로 인계 |
| 12 | RG7 + SC9 | 실제 존재하는 대표 문의/마트 공급 Command만 preview→confirm→canonical 재조회로 연결 |

RG5~RG7은 첫 Simulation playable을 막지 않는다. 다만 Operational adapter가 준비되기 전 Unity가 운영 API 실패를 fixture로 대체해서는 안 된다.

## 10. Canonical gap과 구현 차단선

현재 조사로 확인된 gap은 다음과 같다.

1. 공동구매 대표 역할을 특정 자동집단의 마트 문의 capability로 검증하는 일반 사용자 UseCase가 없다.
2. 마트 관리자에게 의향·확정 수요를 개인정보 없이 제공하는 authorized Projection이 없다.
3. 공개 자동집단 Projection에는 확정 개별 주문 합계와 그 source revision이 없다.
4. `ResidentialPickup` stable ID는 현재 출고예정 기반이며 별도 pickup-point canonical reference를 직접 보존하지 않는다.
5. 마트 관리자용 진열대·위치별 재고·작업·allocation operational Projection은 아직 없다.
6. 현재 `ZoneNpcRouteCatalog`에는 주문자·분배 담당 route만 있고 `ResidentialGroupRepresentative`의 공동주택/마트 route leg와 representative visit state는 없다.

이 gap은 기존 원장 복제의 근거가 아니다. RG5~RG7에서 기존 source ledger와 authorization을 보존하는 얇은 Projection·capability로만 닫는다.

## 11. 완료 검증

- 기존 `individual-demand`, `GroupPurchase`, `Order`, `GroupOrder` 원장을 우회한 중복 원장이 없다.
- 대표는 주민별 의향·주문·결제 원장을 변경할 수 없다.
- auto-group `Confirmed`가 hard demand에 포함되지 않는다.
- group-order의 유효 개별 주문 합계만 `GroupConfirmedDemand`가 된다.
- 마트/Unity contract에 주민 식별·연락·주소·개별 수량·결제 상세가 없다.
- 같은 fixture, seed, data revision과 rule revision은 같은 결과를 만든다.
- Simulation confirm은 운영 주문·결제·공급계약·발주를 생성하지 않는다.
- Operational 실패는 Simulation fixture로 fallback하지 않는다.
- 공동수령은 확정 fulfillment 뒤에만 `residential-pickup:{출고예정Id}`와 `unloading-task:{운송원장Id}.{출고예정Id}`로 연결된다.
- 사회적 대표 context를 변경해도 canonical 공동구매 capability가 자동 확대되지 않는다.
- 대표 NPC route는 기존 Zone route와 중복되지 않고 actor role·Zone·waypoint semantic을 검증한다.
- Simulation NPC는 `CanonicalTaskStableId`를 주장하지 않고 같은 fixture·seed·revision에서 같은 movement snapshot을 만든다.
- NPC 도착·대화·Animator event는 문의·주문·계약·발주·수령 완료 Command를 호출하지 않는다.
- 대표 역할 해제 또는 authorization 변경 뒤 Perspective action과 NPC interaction이 함께 제거된다.
