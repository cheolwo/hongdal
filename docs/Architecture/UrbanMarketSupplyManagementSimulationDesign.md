# 도심마트 공급 계약 경영 Simulation 설계

## 1. 문서 상태와 목표

- 상태: 수요·주문 경계까지 설계 확정, 공급계약 Data 구현 전
- 기준일: 2026-08-09
- 첫 Playable: 감자 한 품목의 4주 공급 포트폴리오
- 실행 경계: 첫 구현은 `Management Simulation` 전용이며 실제 계약·발주·결제·입고를 만들지 않는다.

이 설계는 UM0~UM4의 진열 보충 운영을 폐기하지 않는다. 기존 30초 관리자 queue를 매일 아침의 운영 브리핑으로 유지하고, 지역 수요에서 생성된 주문의 미충족 원인을 공급 제안·계약·납품·현금·노동까지 거슬러 올라가 결정하는 상위 경영 loop를 추가한다.

```text
지역 인구·세대 공공 Data
  → 잠재수요 Interpretation
  → 명시적 Demand Scenario
  → Simulation 주문과 주문 재고할당
  → 공급 포트폴리오 결정
  → 납품·입고·진열 보충
  → 주문 충족·미충족·폐기·현금·작업 부담
  → UM4 queue와 다음 계약 조정
```

작업명은 `도심마트: 계약이 진열대를 만든다`를 유지한다. 게임 설명은 **지역 수요를 읽고 마트를 운영하는 점장 Simulation**으로 확장한다. 수요·주문 계약의 단일 기준은 [도심마트 지역 수요·주문 Simulation 설계](UrbanMarketDemandOrderSimulationDesign.md)다. 공동주택 대표·기존 같이 주문 원장·공동수령의 연결은 [도심마트 공동주택 주문자 집단 통합 설계](UrbanMarketResidentialOrdererGroupIntegrationDesign.md)를 따른다.

## 2. 현재 저장소 기준선

### 2.1 재사용하는 구현

- UM0~UM4의 운영 Data, typed graph, 전역 allocation, 다중 원천 `SourcePlan`
- `DataAttention / UrgentActions / PendingActions / InProgress` 30초 관리자 queue
- shelf, task, detail, source-plan Presentation surface
- stable ID, revision, source lineage와 Simulation/Operational 구분
- 서버의 `플랫폼공급조건계약 → 공급계약이용등록 → 조직개별공급발주` 원장과 UseCase

서버의 플랫폼 공급계약은 이미 구현된 canonical 운영 경계다. 플랫폼은 공급조건의 관리·중개자이며 판매자나 재판매자가 아니고, 음식점과 살들마트는 계약을 이용등록한 뒤 각 발주를 별도로 확인한다. 이 원장을 이름만 바꿔 Unity Simulation 계약으로 복제하지 않는다.

### 2.2 아직 없는 것

- 공급처별 제안과 deterministic counteroffer 계약
- 납품 주기·lead time·품질 기준·결제 기한을 구조화한 Simulation 조건
- 명시적 4주 수요 시나리오와 seed
- Demand Scenario에서 생성되는 synthetic 주문·주문 재고할당·충족 상태 원장
- 공급·재고·폐기·현금·작업 부담을 함께 계산하는 Simulation Engine
- 공급계약 관리자 Perspective, queue와 Unity surface
- Simulation 결과를 기존 입고·진열 보충 fixture로 넘기는 폐루프

현재 서버의 `정산조건`과 `반품조건`은 문자열이며 계약 품목에는 단가·최소/최대 발주수량이 있다. 첫 Playable의 납품 횟수, lead time, 결제 일수와 품질 수명은 이 운영 원장을 조용히 확장하지 않고 Simulation 전용 Data로 명시한다. 운영 계약 확장은 SC9에서 별도 서버 vertical slice로 수행한다.

## 3. 플레이어 역할과 시간 범위

역할 코드는 계속 `MarketManager`를 사용하되 Intent와 Interpreter를 분리한다.

| 시간 범위 | 플레이어 업무 | 구현 경계 |
| --- | --- | --- |
| 매일 | 진열 부족, 입고 필요, 진행 작업 확인 | UM0~UM5 |
| 매주 | 공급 제안 비교, 물량 배분, 납품·가격·결제 조건 조정 | SC1~SC6 |
| 매월·계절 | 갱신, 공급처 분산, 품목 구성과 이행 평가 | SC8 이후 |

```text
ReviewReplenishment
  → ReviewDemandAndOrders
  → ReviewOrderFulfillmentRisk
  → ReviewSupplyPortfolio
  → CompareSupplyOffers
  → NegotiateSupplyContract
  → PreviewSupplyPlan
  → ConfirmSimulationContract
  → AdvanceSimulationTime
  → ReviewContractPerformance
```

진열 보충 Perspective에 계약 판단을 합치지 않는다. 같은 역할이라도 `마트관리자진열보충PerspectiveInterpreter`와 `마트관리자공급계약PerspectiveInterpreter`를 독립적으로 유지한다.

## 4. 첫 Playable: 감자 4주 공급 포트폴리오

### 4.1 공급 방식

| 공급 방식 | 기본 성격 | 장점 | 부담 |
| --- | --- | --- | --- |
| 지역 생산자 협동조합 | 중간 가격, 주 2회, 소량 주문 가능 | 신선도와 유연성 | 공급 가능량 상한 |
| 대형 도매 공급처 | 낮은 단가, 주 1회, 높은 최소주문량 | 물량과 가격 안정 | 보관 공간·폐기·입고 집중 |
| Simulation 현물 시장 | 가격·가용량 변동, 계약 기본 물량 없음 | 긴급 대응 | 공급·가격 불확실성 |

모든 수치는 `SimulatedFixture`와 `ScenarioStableId`, seed, rule revision을 가진다. 실제 공급자 이름, 실제 가격 또는 운영 가용량처럼 표시하지 않는다.

플레이어는 한 공급처를 고르는 대신 기본·보조·긴급 물량의 포트폴리오를 구성할 수 있다. 공급 비율 합계가 100%라는 이유만으로 실행 가능하다고 보지 않고, 각 계약의 최소수량·최대공급량·납품일·현금·보관·작업 제약을 함께 검증한다.

첫 fixture의 비교 기준 예시는 `지역 협동조합 70% + 대형 도매 20% + 현물 시장 10%`다. 이는 정답 비율이 아니라 공급 집중도·최소수량·긴급 대응을 동시에 관찰하기 위한 시작안이며 플레이어가 변경할 수 있다.

### 4.2 성공 조건

단일 종합 점수로 승패를 결정하지 않는다. 다음 결과를 독립적으로 표시하고 시나리오 목표가 요구하는 여러 제약을 함께 만족시킨다.

- 기간별 상품 충족률
- 구매·운송 비용
- 결제일별 현금 잔액과 음수 여부
- 초과 공급 및 폐기 노출
- 입고·검수·진열 작업 횟수와 capacity 초과
- 공급처별 물량 비중과 집중도
- 추가 주문·취소 가능 범위
- 실제 Simulation 이행 기록에 근거한 주의 항목
- 계약 물량·결제·검수 의무의 위반 여부와 미결제 일정

낮은 단가·대량 구매와 높은 단가·소량 다빈도 전략이 서로 다른 trade-off로 목표를 달성할 수 있어야 한다. 비용 절감만으로 노동·폐기·현금 위험을 숨기지 않는다.

## 5. Data 계약과 수명

하나의 Snapshot에 운영·계약·예측을 모두 넣지 않는다.

```text
도심마트운영DataSnapshot
  - 현재 재고·진열대·작업·allocation

지역인구DataSnapshot
  - 공공 인구·세대 사실

도심마트수요시나리오DataSnapshot
  - 지역 잠재수요를 입력으로 명시한 기간별 Simulation 가정

도심마트주문SimulationDataSnapshot
  - Tick별 synthetic 주문·상태·충족량

도심마트공급경영SimulationDataSnapshot
  - 가상 공급처·Offer·Simulation 계약안·납품·결제 fixture
```

Operational 연결 시에는 별도의 authorized `도심마트공급계약OperationalDataSnapshot`을 사용한다. 이 Projection은 기존 `플랫폼공급조건계약`, 조직 이용등록과 개별 발주 원장을 권한에 맞게 투영하며 Simulation 계약 ID를 운영 계약 ID로 승격하지 않는다.

### 5.1 Simulation Data 후보

- `도심마트공급처SimulationData`
- `도심마트공급제안SimulationData`
- `도심마트공급계약안SimulationData`
- `도심마트계약상품조건SimulationData`
- `도심마트납품약정SimulationData`
- `도심마트계약이행SimulationData`
- `도심마트대금일정SimulationData`
- `도심마트수요시나리오Data`
- `도심마트주문SimulationData`
- `도심마트주문재고할당SimulationData`

각 Snapshot은 독립 `DataRevision`, 기준시각, mode와 source lineage를 가진다. 다른 주기로 갱신되어도 한 revision으로 뭉개지 않는다.

### 5.2 사실과 해석

Data에는 제시 단가, 최소수량, 납품일, 실제 Simulation 도착량, 검수 통과량과 결제 예정일 같은 사실만 둔다. 공급 공백, 과잉 물량, 현금 압박, 폐기 노출, 집중도, 갱신 필요와 이행 주의는 Interpretation에서 rule revision과 계산 근거를 붙여 만든다.

수요가 없을 때 판매속도·품절 예상·매출 영향을 추론하지 않는다. `도심마트수요시나리오DataSnapshot`이 있을 때만 synthetic 주문을 만들고, 주문 객체의 할당·부분충족·미충족 원장을 거쳐 결과를 계산한다. 인구를 주문으로 직접 변환하지 않으며 항상 Simulation 표시와 basis·rule lineage·한계를 노출한다.

## 6. Shared World와 관계

```text
도심마트경영SharedWorldState
├─ 공급처WorldState
├─ 공급제안WorldState
├─ 공급계약안WorldState
├─ 계약상품조건WorldState
├─ 납품약정WorldState
├─ 납품실적WorldState
├─ 대금일정WorldState
├─ 수요시나리오WorldState
├─ Simulation주문WorldState
└─ 주문재고할당WorldState
```

필요 관계는 다음 의미를 갖는다.

```text
공급처 → 공급제안       Provides
공급제안 → 상품         Targets
공급계약안 → 공급처     ProvidedBy
공급계약안 → 상품       Covers
납품약정 → 공급계약안   DerivedFrom
입고요청 → 납품약정     Fulfills
입고재고 → 입고요청     DerivedFrom
진열보충 → 입고재고     Targets
수요시나리오 → 지역      BasedOn
Simulation주문 → 수요시나리오 GeneratedFrom
주문재고할당 → Simulation주문 DerivedFrom
주문재고할당 → 판매가능재고 Allocates
```

`ProvidedBy`, `Covers`, `Fulfills`, `BasedOn`, `GeneratedFrom`, `Allocates`를 `AssignedTo`로 대체하지 않는다. SC1에서 첫 typed graph를 구현할 때 공통 `WorldRelationKind` 확장과 기존 소비자 호환 test를 함께 추가한다. 실제 relation 소비 없이 enum만 미리 늘리지 않는다.

## 7. 공급 포트폴리오와 4주 Engine

`도심마트공급포트폴리오Interpreter`는 제안·계약안을 비교하고, 순수 C# `도심마트공급경영SimulationEngine`은 4주 Tick 결과를 계산한다.

필수 입력:

- 시작 재고, 현금, 보관 capacity와 작업 capacity
- 공급처별 단가·최소수량·최대 추가량
- 납품 요일·횟수, lead time과 운송비
- 결제 기한과 품질별 판매 가능 기간
- 기간별 수요 시나리오, seed와 rule revision
- Tick별 synthetic 주문 생성 규칙, 주문 기한과 주문 재고할당 정책
- 플레이어가 선택한 공급처별 물량 배분과 계약안 revision

Tick의 최소 순서는 `주문 생성 → 현재 재고 1차 할당 → 납품 도착·검수·입고 → 진열 보충 → 미처리 주문 재할당 → 충족 마감 → 폐기 → 결제`로 고정하고, 같은 seed·입력 revision·rule revision에서는 같은 결과를 만든다. 입고 도착만으로 판매 가능 또는 주문 충족으로 계산하지 않으며 검수와 작업 capacity를 모두 통과해야 한다.

출력은 하나의 점수가 아니라 기간별 inventory, delivery, cash, waste, workload와 supplier share ledger다. 각 metric은 계산 rule과 source revision을 추적할 수 있어야 한다.

## 8. 협상과 Counteroffer

협상은 숨겨진 호감도나 무작위 성공률을 사용하지 않는다. 공급 가능량, 차량·배송 횟수, 최소 수익 조건, 기존 Simulation 계약 점유량, 결제·검수 이행과 변경 횟수를 입력으로 하는 deterministic policy다.

```text
PlayerProposal
  → constraint validation
  → Accepted | RejectedWithReasons | Counteroffer
  → reason codes + rule revision + changed terms
```

첫 version의 조정 항목은 단가, 최소 주문량, 기본 공급량, 납품 횟수, lead time, 결제 기한과 품질 기준으로 제한한다. 한 조건을 완화하면 다른 조건이나 운송비가 어떻게 바뀌는지 이유를 함께 표시한다.

신뢰도는 국적·언어·가족 형태·경제력 같은 배경 정보가 아니라 Simulation 또는 권한이 허용된 운영 이행 기록만 사용한다.

## 9. 관리자 Perspective와 queue

`마트관리자공급계약PerspectiveInterpreter`는 다음 Intent를 분리한다.

- `ReviewSupplyPortfolio`
- `CompareSupplyOffers`
- `NegotiateSupplyContract`
- `ReviewContractRenewal`
- `ReviewDeliveryException`
- `ReviewDemandAndOrders`
- `ReviewOrderFulfillmentRisk`

| Queue | 의미 |
| --- | --- |
| `DataAttention` | 가격·단위·기간·상품·revision 관계가 불완전 |
| `DecisionRequired` | 만료 또는 명시적 공급 공백 때문에 결정 필요 |
| `RiskAttention` | 납품 이행, 집중도, 현금 또는 작업 capacity 검토 |
| `Opportunity` | 현재 목표에서 더 유연하거나 낮은 비용의 대안이 있으나 자동 교체 금지 |
| `Negotiating` | Counteroffer 또는 Simulation 내부 승인 대기 |
| `ActiveContracts` | 정상 이행 중인 계약안 |
| `NoActionNeeded` | 기본 action queue에서 숨김 |

`PriorityScore`를 예상 이익 하나로 만들지 않는다. `PriorityReasonCodes`, rule revision, source lineage와 metric별 근거를 보존하며 Stable ID는 같은 우선순위 안의 tie-breaker로만 사용한다.

첫 reason code는 `ContractExpiring`, `SupplyCoverageGap`, `MinimumVolumeExcess`, `PaymentSchedulePressure`, `SupplierConcentrationHigh`, `DeliveryPerformanceAttention`, `BetterFlexibleOfferAvailable`, `ContractDataIncomplete`, `OrderBacklogPresent`, `OrderFulfillmentGap`, `InboundTooLateForDueTick`, `DemandOrOrderDataIncomplete`로 제한한다. 각 code는 해당 Data와 계산 rule을 찾을 수 있어야 하며 단순 설명 문구로만 만들지 않는다.

## 10. Unity 공간과 Presentation

| 공간 | 책임 |
| --- | --- |
| 매장 Floor | UM4 shelf·task·detail·SourcePlan으로 계약 결과 표시 |
| 관리자 사무실 | 주간 공급 일정, 포트폴리오, Offer 비교, 갱신 달력, 현금·작업 Preview |
| 공급처 미팅 테이블 | 제안 조건·거절 이유·Counteroffer 표시 |
| 입고 Dock | 예정·실제 도착·검수·부분 납품과 후방 재고 이동 표시 |

3D 공간은 상태와 인과관계를 보여주고, 조건 비교·표·현금 흐름은 2D panel로 유지한다. NPC 도착과 animation은 계약 체결·납품 수락·검수 완료가 아니다.

`도심마트공급계약PresentationSnapshot`은 최소한 다음 독립 surface를 가진다.

- `SupplyPortfolioBoardSurface`
- `SupplyOfferCardSurface`
- `ContractCalendarSurface`
- `ManagementPreviewSurface`
- `CashScheduleSurface`
- `DeliveryCommitmentSurface`
- `NegotiationPanelSurface`
- `DemandAndOrderBriefingSurface`
- `ConceptCardDeckSurface`

주문 브리핑은 현재 주문·미처리량·현재 가용재고·예정 입고·향후 7일 수요·계약 공급량을 함께 보여준다. `즉시 충족`, `입고·검수·진열 뒤 충족 가능`, `기한 내 충족 불가`를 분리하고 예정 입고를 현재 재고처럼 합산하지 않는다.

`ConceptCardDeckSurface`는 위 업무 surface를 대체하지 않고 대표 NPC, 진열대, 공급처와 입고 Dock에서 관련 surface로 들어가는 학습·설명 layer다. `Concept / Status / Reason / Action`을 분리하고 첫 대표 deck에서는 의향 410kg, 확정 385kg, 공동수령, 공급 상태, 부족 근거와 공급 검토 행동을 연결한다. 공통 계약과 asset 중립 경계는 [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)을 따른다.

## 11. Simulation과 Operational 실행 경계

### Management Simulation

`Management Simulation`은 새 실행 mode가 아니라 저장소 공통 `SsalddelExecution:Mode=Simulation` 안의 도심마트 시나리오 profile이다. 화면이나 API에 세 번째 실행 mode를 추가하지 않는다.

```text
계약안 선택
  → 4주 결과 Preview
  → 명시적 확인
  → Simulation 계약 원장 확정
  → Tick 진행
  → 새 Simulation Snapshot
```

Simulation 확정은 실제 계약, 조직 이용등록, 개별 발주, 결제, 입고와 재고를 만들지 않는다.

### Operational

```text
authorized 계약·발주 Projection 조회
  → preview
  → 사용자 명시적 확인
  → 기존 server UseCase / Command
  → 권한·동의·expected revision·멱등성 검증
  → canonical 재조회
```

운영 공급계약은 [플랫폼 공급조건 계약과 개별 발주 중개](PlatformSupplyBrokerage.md)를 따른다. Unity가 운영 계약 조건을 직접 확정하거나 `ConfirmSimulationContract`를 운영 Command로 재사용하지 않는다.

## 12. 재정렬된 구현 순서

운영 서버와 게임 상태를 섞지 않기 위한 `SS0~SS1`을 먼저 완료했다. UM5는 공급 제안·계약·납품처럼 자주 바뀌는 surface의 공통 기반이므로 다음으로 완료한다.

| 단계 | 구현 내용 | 완료 근거 |
| --- | --- | --- |
| SS0 | 기존 운영 서버와 별도 Simulation authority·dependency·database 경계 | D-027과 독립 project reference test |
| SS1 | Simulation session·scenario lineage·seed·revision·멱등 Tick API | 집중 test와 전용 solution build |
| UM5-A 완료 | 관리자 surface의 context Runtime·reconcile·selection·last-success 전환 | headless change set, refresh failure, selection 회귀 |
| UM5-B 완료 | manager surface applicator와 별도 sample Controller/View 전환 | 실제 Unity 프로젝트 package/sample compile·EditMode 16/16; Scene·Game View 미실행 |
| SC0 완료 | 공급처·Offer·계약안 + 수요·주문·주문할당 계약 | 독립 revision·lineage, Simulation/운영 분리와 참조·단위·수량 무결성 11건 |
| SC1-A 완료 | 감자 1품목, 3공급처 Simulation fixture와 typed graph | 10 node·15 relation, 조건 차이·lineage·semantic relation 검증 7건 |
| SC1-B 완료 | 지역 인구 → 잠재수요 → 4주 Demand Scenario | 인구 자동 비례 없음, 4주 명시 가정과 basis·rule lineage 검증 8건 |
| RG0 완료 | 기존 같이 주문·대표·공동수령·마트 상품·공급계약 재사용 조사 | ExistingServerReuseMap과 canonical gap; 코드 변경 없음 |
| SC1-C 완료 | 기본 방문 Demand Scenario → Simulation Order Stream | 4주 56건, 일별 2건, 기대수요 합계·same-seed·lineage 검증 8건 |
| RG1 완료 | 공동주택 주문자 집단 Simulation fixture | synthetic 대표의 사회적 context·canonical role·NPC identity, 의향 410kg·확정 385kg·pickup 후보; 집중 8건 |
| RG2 완료 | 주문자 집단 수요 Data와 typed graph | 기존 원장 복제 없이 intent/confirmed source·관계 무결성; 집중 7건 |
| RG3 완료 | 기본·집단의향·집단확정 수요 Composition | 기본 1,720kg + 확정 385kg만 hard demand 2,105kg; 의향 410kg 제외; 집중 8건 |
| SC2 완료 | 합성 hard demand의 주문 할당·충족 + 공급 Engine | 28 Tick, 재고·수요·현금 보존식, 검수/입고/진열 통합 작업 capacity, 폐기·운송비·공급처 비중; 집중 9건 |
| RG4 완료 | 대표·주민·마트 관리자 Perspective와 inquiry/dialogue state | 역할별 projection, capability 제거, 문의 초안 비노출, 개인정보·자동 Command 제외; 집중 11건 |
| RG4-NPC-A 완료 | 공동주택 대표 NPC 방문 core | 공통 NpcMovement에 주거/마트 route leg를 additive하게 추가하고 visit state·무효과 arrival action 검증 8건 |
| SC3~SC5 headless 완료 | 공급 위험 Perspective·브리핑·계약 surface 입력 | 공백·현금·작업·집중도 근거와 주문/재고/입고·Preview·현금·납품 독립 모델; 집중 8건 |
| RG4-NPC-B + SC5 Unity binding code 완료 | 대표 NPC View·manager desk 대화·surface mapper/applicator | local package core Unity compile 완료; imported sample·Scene wiring·NavMesh·Animator·Game View 검증 잔여 |
| CC0 완료 | 공통 Concept·Status·Reason·Action 카드 문법 | 공통 계층, 개인정보·권한·asset 중립 경계와 첫 대표 deck 확정 |
| CC1 완료 | 공통 카드 계약·Projector | identity·revision·mode·lineage, 선택과 미승인 Action 제거 검증 |
| CC2 완료 | 대표 NPC 7-card adapter | 집단 확정 385kg·전체 hard demand 2,105kg·현재 공급 부족 75kg을 source별로 분리 투영 |
| CC3 + RG4-NPC-C | 카드 View·skin과 Unity runtime wiring | imported sample·Scene·NavMesh·Animator에서 대표 선택→카드 탐색 Game View 검증 |
| SC6 | Preview → Confirm Simulation → Tick → 새 Snapshot 폐루프 | 같은 seed 재현·명시 확인·멱등 Simulation command |
| SC7 | 납품→재고→진열→주문 충족→UM4 queue·대표 결과 전달 | 계약 결정에 따른 주문 상태·NPC 전달 상태·UM4 queue 변화 |
| SC8 | 다품목·다중 공급처·협동조합 공동구매 확장 | 품목·조직 경계와 공정성 회귀 |
| RG5 | 기존 공동구매·group-order authorized Projection | 기존 원장을 source로 하는 privacy-safe operational adapter |
| RG6 | 기존 ResidentialPickup 연결 | 확정 fulfillment 뒤 출고·운송 stable ID 인계 |
| RG7 + SC9 | 대표 문의와 공급계약 Operational Command 폐루프 | 실제 capability만 권한·동의·revision·멱등·canonical 재조회 |

기존 UM6~UM9를 삭제하지 않는다. UM6 operational 마트 aggregate는 SC9의 선행 서버 경계, UM7 NPC는 SC7의 납품·입고 표현, UM8 입고·발주는 SC7/SC9의 handoff로 재배치한다. UM9의 판매속도·유통기한·재고 차이는 Simulation 수요 시나리오와 운영 canonical metric을 섞지 않고 각각 별도 rule로 유지한다.

## 13. 첫 Playable 완료 정의

플레이어가 10분 안에 다음 폐루프를 수행할 수 있어야 한다.

1. 지역 인구·세대 사실과 Simulation 수요 가정을 구분해 확인한다.
2. 오늘의 감자 주문, 미처리량, 현재 재고와 예정 입고를 확인한다.
3. 세 공급처의 제안과 주문 충족 위험을 비교한다.
4. 둘 이상의 공급처에 기본·보조·긴급 물량을 배분한다.
5. 7개 상업 조건을 조정하고 counteroffer 이유를 확인한다.
6. 4주 주문 충족·재고·폐기·현금·작업 부담·집중도 Preview를 본다.
7. Simulation 계약임을 확인하고 명시적으로 확정한다.
8. Tick을 진행해 주문→할당→납품→진열→충족 상태 변화를 본다.
9. 계약 선택에 따라 주문 미충족과 UM4 queue가 달라지는 것을 확인한다.
10. 같은 scenario seed와 rule revision에서 같은 주문·결과를 재현한다.

완료 기준은 플레이어가 주문 미충족과 진열 부족의 원인을 지역 수요 가정과 계약 결정까지 추적하고, 다른 계약안을 선택해 후속 운영 결과를 바꿀 수 있는 것이다. 실제 운영 주문·계약·결제·발주 성공은 첫 Playable의 완료 조건이 아니다.

## 14. 검증 matrix

| 범위 | 최소 검증 |
| --- | --- |
| Data | 중복·누락 ID, 단위·통화·기간·revision·mode·source 경계 |
| Graph | 공급 관계와 `BasedOn/GeneratedFrom/Allocates` 의미, dangling relation 거부 |
| Demand·Order | 인구 직접 변환 금지, 같은 seed 주문 결정성, 총수요·충족 보존 |
| Engine | 주문→할당→납품→진열→재할당 Tick 순서, 재고·현금 보존, 음수·capacity 차단 |
| Negotiation | 숨은 확률 없음, constraint별 거절·counteroffer reason |
| Interpretation | metric별 계산 근거, 단일 이익 점수 금지, 수요 없는 예측 금지 |
| Perspective | Intent 분리, queue 우선순위, focus와 authorization 비확대 |
| Presentation | surface별 stable ID·revision, 표·공간 책임 분리 |
| Simulation command | preview·confirm 분리, 중복 confirm 멱등, 실제 원장 무변경 |
| Downstream | 납품→입고→재고→UM4 queue 인과관계와 lineage |
| Operational | 기존 공급중개 권한·조직 동의·개별 확인·canonical 재조회 |

headless engine, Unity Editor compile, Scene wiring, Game View, 서버 operational runtime을 각각 별도 증거로 보고한다.
