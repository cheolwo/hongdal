# 도심마트 지역 수요·주문 Simulation 설계

## 1. 목적과 상태

이 문서는 공공 지역 인구·세대 사실이 도심마트 경영 Simulation의 주문과 공급계약 판단으로 이어지는 계약을 정의한다. 현재는 설계 기준이며 공공 공급자 호출, 운영 주문 집계, Simulation Order Engine 또는 Unity surface 구현 완료를 뜻하지 않는다.

첫 Playable의 정체성은 **지역 수요를 읽고 마트를 운영하는 점장 Simulation**이다. 인구 숫자를 주문으로 직접 바꾸지 않고 다음 계보를 반드시 보존한다.

```text
지역인구DataSnapshot                 공공 Data 사실
  → 지역잠재수요WorldState           Interpretation
  → 도심마트수요시나리오DataSnapshot  명시적 Simulation 입력
  → 도심마트주문SimulationDataSnapshot deterministic 주문 객체
  → 주문 재고할당·충족 원장           Simulation 결과
  → 관리자 주문 브리핑                Presentation
  → 공급계약·추가조달 판단             사용자 결정
```

상세 지역 Data·개인정보 기준은 [지역 인구·수요 World Layer 제안](RegionalPopulationDemandWorldLayerProposal.md), 공급처·계약·납품 기준은 [도심마트 공급 계약 경영 Simulation 설계](UrbanMarketSupplyManagementSimulationDesign.md)를 따른다. 기존 같이 주문의 의향·확정 수요를 합성하는 기준은 [도심마트 공동주택 주문자 집단 통합 설계](UrbanMarketResidentialOrdererGroupIntegrationDesign.md)를 따른다.

## 2. 절대 경계

1. 인구·세대수는 잠재 소비 기반이며 주문량이 아니다.
2. `지역잠재수요WorldState`는 Interpretation이다. 공공 DataSnapshot에 상품별 구매량을 써 넣지 않는다.
3. Simulation 주문은 `ScenarioStableId + Seed + RuleRevision + DemandRevision`에서만 생성한다.
4. Simulation 주문에는 실제 사람, 주소, 연락처, 계정 또는 운영 주문 ID를 넣지 않는다.
5. Simulation 주문 Stable ID를 실제 주문 Stable ID로 승격하거나 운영 Command에 재사용하지 않는다.
6. 운영 주문은 기존 운영 서버가 권한·지역·기간·최소 집계 기준을 적용한 canonical Projection만 사용한다.
7. 실제 운영 집계가 없을 때 Simulation 주문으로 fallback하지 않고, Simulation 실패를 실제 주문으로 보완하지 않는다.
8. 모든 예상 수요·주문·품절·미충족 값에는 Simulation 표시, input revision, rule revision과 한계를 노출한다.
9. `공동구매자동집단상태코드.확정`은 모집 결과의 후속 인계 승인이지 주민별 확정 주문 합계가 아니다.
10. 집단 hard demand는 유효 개별 주문 원장을 합산한 기존 `group-order`에서만 읽고, 비구속 의향과 분리한다.

## 3. Data 계약과 수명

Snapshot은 갱신 주기와 권위가 다르므로 다음처럼 분리한다.

```text
지역인구DataSnapshot                    공공 사실
도심마트수요시나리오DataSnapshot         Simulation 가정
도심마트주문SimulationDataSnapshot       synthetic 주문 원장
도심마트공급경영SimulationDataSnapshot   공급처·계약·납품·결제
도심마트운영SimulationDataSnapshot       재고·진열·작업
```

서로 다른 Snapshot의 revision을 하나로 합치지 않고 Engine 실행 입력에 각각 기록한다.

### 3.1 지역인구DataSnapshot

공식 인구·세대·지역·기준시각·경계 기준연도·출처·단위·품질 상태만 보존한다. 공공 공급자가 마스킹·억제·결측으로 제공한 값은 0으로 바꾸지 않는다.

### 3.2 지역잠재수요WorldState

공공 사실을 상품 주문으로 확정하지 않고, 특정 지역에 소비 기반이 존재한다는 해석을 제공한다.

- `RegionStableId`
- `PopulationBasisRevision`
- `PotentialDemandBandCode` 또는 명시적 범위
- 사용한 metric과 가중치
- `InterpretationRuleRevision`
- 품질·공간 정밀도·한계

실제 주문량·매출·시장점유율은 포함하지 않는다.

### 3.3 도심마트수요시나리오DataSnapshot

Simulation이 사용할 명시적 가정이다.

- `ScenarioStableId`
- `ProductStableId`
- `RegionStableId`
- 기간별 수요량 또는 수요 범위
- 수량 단위와 기간 timezone
- `ScenarioSeed`
- `DemandRuleRevision`
- `PopulationBasisRevision`
- 상품 선택률·Simulation 점유율·계절·요일·행사 가정
- 생성 근거와 limitation codes
- Simulation 전용 표시

인구 변화율과 수요 변화율이 우연히 같더라도 자동 비례 규칙으로 숨기지 않고 위 가정에 명시한다.

### 3.4 도심마트주문SimulationDataSnapshot

하나의 Tick 실행에서 생성되거나 상태가 바뀐 synthetic 주문을 보존한다.

```text
도심마트주문SimulationData
├─ OrderStableId
├─ ScenarioStableId
├─ DemandScenarioRevision
├─ ProductStableId
├─ RegionStableId
├─ CreatedTick
├─ FulfillmentDueTick
├─ RequestedQuantity / Unit
├─ AllocatedQuantity
├─ FulfilledQuantity
├─ UnfulfilledQuantity
├─ StateCode
├─ GenerationRuleRevision
└─ SourceDemandSegmentStableId
```

첫 상태 code는 `Pending`, `Allocated`, `PartiallyFulfilled`, `Fulfilled`, `Unfulfilled`, `Cancelled`로 제한한다. `Cancelled`는 Engine 임의 실패가 아니라 scenario에 명시된 취소 event가 있을 때만 사용한다.

### 3.5 주문재고할당SimulationData

주문과 어떤 판매 가능 Simulation 재고가 연결됐는지 명시한다.

- `AllocationStableId`
- `OrderStableId`
- `InventoryStableId`
- `Quantity / Unit`
- `StateCode = Reserved | Consumed | Released`
- `AllocationRevision`

한 재고를 여러 주문이 중복 소비하지 않도록 전역 가용량을 계산한다. 주문 할당과 UM3R 진열 보충 작업 할당은 목적이 다른 별도 원장이며 같은 allocation ID를 공유하지 않는다.

## 4. Shared World와 관계

```text
지역WorldState → 잠재수요WorldState          InterpretedAs
수요시나리오WorldState → 지역WorldState      BasedOn
Simulation주문WorldState → 수요시나리오      GeneratedFrom
Simulation주문 → 상품                        Targets
주문재고할당 → Simulation주문                DerivedFrom
주문재고할당 → 판매가능재고                  Allocates
납품약정 → 공급계약안                        DerivedFrom
입고재고 → 납품실적                          DerivedFrom
진열보충 → 후방재고                          Targets
```

`InterpretedAs`, `BasedOn`, `GeneratedFrom`, `Allocates`는 기존 관계에 억지로 대입하지 않는다. SC1에서 실제 graph 소비가 생길 때 공통 관계 확장과 호환 test를 함께 추가한다.

## 5. deterministic 주문 생성

동일한 `ScenarioStableId`, demand revision, seed, rule revision과 Tick 범위에서는 같은 주문 순서·수량·기한을 생성한다.

```text
Demand segment
  → scenario rule validation
  → deterministic quantity partition
  → OrderStableId = scenario + tick + sequence
  → synthetic order stream
```

주문 건수와 주문별 수량 분포는 명시적 scenario parameter다. 수요 총량만 주어졌는데 임의의 가구 구성이나 소득 수준을 추론하지 않는다. 주문 분할은 총수요 보존을 만족해야 하며 반올림 잔량도 deterministic 규칙으로 배분한다.

## 6. 주문 충족과 Tick 순서

SC2의 하루 Tick 순서는 다음으로 고정한다.

1. 해당 Tick의 Simulation 주문 생성
2. 현재 판매 가능 재고로 1차 주문 할당
3. 예정 납품 도착·검수·후방 입고
4. 작업 capacity 안에서 진열 보충
5. 새 판매 가능 재고로 미처리 주문 재할당
6. 기한 도달 주문을 `Fulfilled / PartiallyFulfilled / Unfulfilled`로 마감
7. 유통기한 경과·폐기
8. 계약·운송·구매 대금 일정 반영
9. inventory·order·cash·waste·workload ledger 확정

같은 Tick에서 입고된 물량이 주문을 충족하려면 검수와 진열 작업 capacity를 모두 통과해야 한다. 입고 도착만으로 판매 가능 재고나 주문 충족으로 계산하지 않는다.

필수 보존식:

- 주문별 `Requested = Fulfilled + Unfulfilled + 아직 유효한 Pending`
- 재고별 `OnHand = Available + Reserved`를 원장 의미에 맞게 보존
- `Consumed` 주문 allocation 합계는 `FulfilledQuantity`와 일치
- 음수 재고·음수 현금·capacity 초과는 숨기지 않고 차단 또는 별도 위험 결과로 기록

## 7. 관리자 Perspective

공급계약 관점에 다음 Intent를 추가한다.

- `ReviewDemandAndOrders`
- `ReviewOrderFulfillmentRisk`

주문 미충족은 공급계약 변경의 한 근거이지 자동 긴급발주 명령이 아니다. Perspective reason code는 다음부터 시작한다.

- `OrderBacklogPresent`
- `OrderFulfillmentGap`
- `InboundCanCoverBacklog`
- `InboundTooLateForDueTick`
- `ShelfWorkCapacityLimited`
- `DemandAboveScenarioBand`
- `SupplyCoverageGap`
- `DemandOrOrderDataIncomplete`

`InboundCanCoverBacklog`은 수량뿐 아니라 도착 Tick·검수·진열 capacity와 주문 기한을 모두 만족할 때만 사용한다.

## 8. DemandAndOrderBriefingSurface

`도심마트공급계약PresentationSnapshot`에 독립 surface로 추가한다.

```text
DemandAndOrderBriefingSurface
├─ TodayOrderCount / TodayRequestedQuantity
├─ PendingOrderQuantity
├─ Fulfilled / PartiallyFulfilled / Unfulfilled
├─ CurrentAvailableInventory
├─ TodayScheduledInbound
├─ DemandScenarioBand / CurrentDemand
├─ Next7DayDemand
├─ ContractedSupplyBySupplier
├─ CoverageGapWindows
├─ ReasonCodes
├─ SimulationLabel / LimitationText
└─ SourceLineage / PresentationRevision
```

현재 가용재고와 오늘 입고 예정량을 단순 합산해 즉시 충족 가능으로 표시하지 않는다. `현재 즉시 충족`, `입고·검수·진열 뒤 충족 가능`, `기한 내 충족 불가`를 분리한다.

이 값은 숫자 panel만으로 노출하지 않는다. `의향 수요`와 `확정 수요`의 개념 차이, 현재 집단 수요 상태, 공급 부족 계산 근거와 허용된 다음 행동을 `Concept / Status / Reason / Action` 카드로 분리한다. 공통 카드 계약과 첫 대표 NPC deck은 [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)을 따른다.

## 9. Operational 주문과의 분리

```text
Public population → Potential demand → Simulation demand → Simulation orders

Operational Order API → authorized order Projection → Operational manager view
```

Operational 화면은 실제 주문 aggregate와 최소 집계 기준을 따르며, 개별 주문 상세는 해당 업무 권한이 있을 때만 운영 서버가 제공한다. Public Population Data와 실제 주문을 client에서 결합해 개인 또는 소지역 행동을 역추정하지 않는다.

## 10. 재정렬된 구현 순서

| 단계 | 내용 | 완료 기준 |
| --- | --- | --- |
| UM5-B 완료 | manager surface applicator·sample wiring | 실제 Unity project sample compile·EditMode 검증 |
| SC0 완료 | 공급처·Offer·계약안 + demand·order·allocation 계약 | 독립 revision·lineage, Simulation 경계와 참조·단위·합계 검증 |
| SC1-A 완료 | 감자 1품목·3공급처 fixture | 공급 graph·단위·계약 조건과 deterministic relation 검증 |
| SC1-B 완료 | 지역 인구 → 잠재수요 → 4주 Demand Scenario | 억제·결측 거부, 숨은 비례 없음과 basis·assumption lineage 검증 |
| RG0 완료 | 기존 같이 주문·대표·공동수령·공급계약 조사 | ExistingServerReuseMap과 canonical gap |
| SC1-C 완료 | 기본 방문 Demand Scenario → Simulation Order Stream | 4주 56건, 일별 2건, 기대수요 합계·same-seed·stable ID 보존 |
| RG1~RG2 완료 | 공동주택 fixture와 집단 수요 typed graph | 의향/확정 source·대표 role·pickup 후보 및 graph relation 분리 |
| RG3 완료 | 기본·집단의향·집단확정 수요 Composition | 기본 1,720kg + 확정 group-order 385kg만 hard demand, 의향 410kg 제외 |
| SC2 완료 | 합성 hard demand의 주문 할당·공급 Engine | 28 Tick, 재고·수요·현금 보존식과 capacity·폐기·공급처 비중 golden scenario |
| RG4 완료 | 주민·대표·마트 관리자 집단 수요 Perspective | 개인정보 제외, inquiry/dialogue와 capability 비확대 |
| RG4-NPC-A + SC3~SC5 headless 완료 | 대표 방문 core·집단 수요 Interpretation·surface 입력 | 두 Zone route leg, queue·브리핑·계약 위험 근거 |
| RG4-NPC-B + SC5 Unity binding code 완료 | 대표 NPC View와 주문/계약 surface mapper/applicator | package core compile 완료, imported sample·Scene·NavMesh·Animator·Game View 검증 잔여 |
| CC0 완료 | 수요·주문 학습 카드 방향 확정 | 의향/확정·상태·근거·행동을 서로 다른 카드로 표현 |
| CC1 완료 | 공통 카드 계약·Projector | mode·revision·lineage와 미승인 Action·사라진 선택 제거 검증 |
| CC2 완료 | 대표 NPC deck | 385kg 집단 확정 수요, 2,105kg 전체 hard demand와 75kg 현재 부족을 source별로 분리 표시 |
| CC3 + RG4-NPC-C | 카드 View·skin·Scene 연결 | 대표 NPC 선택에서 공급 검토 Action Card까지 runtime 확인 |
| SC6 | 계약 Confirm → Tick → 새 Snapshot | 멱등 command·same-seed replay |
| SC7 | 납품 → 재고 → 진열 → 주문 충족 → UM4 queue | 전체 인과계보와 하류 변화 |
| RG5~RG7 | 기존 group-order Projection·ResidentialPickup·Command 폐루프 | 기존 authority·stable ID·canonical 재조회 |

SC8 다품목·협동조합 확장과 SC9 운영 canonical 연결 순서는 유지한다.

## 11. 검증 기준

| 범위 | 최소 검증 |
| --- | --- |
| Population handoff | 공공 사실에 주문량이 포함되지 않음, region·period·quality 보존 |
| Demand scenario | seed·rule·basis revision 필수, 숨은 인구 비례 금지 |
| Order generation | 같은 입력 결정성, 총수요 보존, 중복 ID·음수 수량 거부 |
| Allocation | 재고 중복 소비 금지, 단위 불일치·초과 예약 차단 |
| Fulfillment | 부분충족·미충족 보존식, deadline과 상태 전이 검증 |
| Tick | 주문→할당→납품→진열→재할당 순서 golden test |
| Privacy | 실제 개인·주소·연락처·운영 주문 ID 없음 |
| Presentation | 즉시/입고 후/기한 내 불가 분리, Simulation 표시와 lineage |
| Boundary | Simulation 주문이 운영 계약·발주·결제·주문을 만들지 않음 |
