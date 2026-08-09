# Unity 도심마트 운영자 3계층 재정비 설계

## 1. 문서 상태와 목적

- 상태: UM0~UM3R 구현·headless 검증, UM4 이후 미구현
- 기준일: 2026-08-09
- 대상: `Ssalddel.Unity/Runtime/UrbanMarket`, `Ssalddel.Unity/Samples~/UrbanMarket`과 향후 마트 운영 Projection
- 첫 업무 slice: **진열 보충**

이 문서는 도심마트를 상품 진열 샘플에서 **마트 운영자가 재고·진열·작업을 이해하고 다음 업무를 검토하는 World**로 발전시키기 위한 feature 기준 문서다. 공통 계층·identity·runtime 규칙은 [Unity Data·Interpretation·Presentation Architecture](UnityDataInterpretationPresentationArchitecture.md)를 따르고, 이 문서는 도심마트에 필요한 구체 계약과 migration 순서만 정의한다.

이번 단계에서는 MVVM, `ObservableObject`, 새 UI framework, 외부 3D asset과 대규모 그래픽 개선을 도입하지 않는다. 기존 public class, API route와 Scene wiring은 즉시 삭제하거나 rename하지 않고 compatibility facade로 유지한다.

2026-08-09 구현 상태: UM0~UM1에서 `도심마트공개상품DataSnapshot`, Data Mapper·validator, operational Data Repository와 simulation Data Query를 추가했다. `ProjectionAudienceCode=OrdererPublic`, `QuantityMeaningCode=ProjectedSaleAvailability`와 공개 수량 안내를 필수로 검증하며 물리 보관·진열·예약 재고 필드는 Data 계약에 넣지 않았다. 기존 `도심마트ApiMapper.Map`, `Simulated도심마트조회UseCase`와 ScreenModel 소비자는 새 Data 경로 뒤 compatibility adapter로 유지한다.

UM2에서는 공개 상품 World가 상품 node만 가지도록 제한하고, 관리자용 상품·위치·재고·진열대·작업은 별도의 `MarketOperatorAuthorized` Simulation DataSnapshot과 `WorldGraphIndex`로 구성했다. UM3에서는 목표 진열률이 명시된 simulation rule로 보충 후보 수량, 입고 필요, 활성 작업 중복, 데이터 불충분과 capability 차단 사유를 계산한다. 이 결과는 preview 후보이며 서버 작업을 만들지 않는다. UrbanMarket targeted 28건과 Unity core 전체 157건이 통과했으며 Unity Editor·Scene runtime은 이번 단계에서 실행하지 않았다.

다만 UM3의 현재 구현은 같은 상품을 여러 진열대가 공유할 때 상품의 모든 후방재고를 합산하면서 진행 작업은 현재 진열대 대상만 차감한다. 또한 작업 하나가 `SourceInventoryStableId` 하나만 가리켜 여러 후방 위치에서 나누어 가져오는 계획을 표현하지 못한다. 따라서 현 상태는 **신뢰 가능한 진열 보충 판단의 초기 proof**이지 마트 전체 운영관리 또는 재고 예약 무결성의 완료가 아니다. UM4 관리자 우선순위보다 먼저 UM3R에서 전역 할당과 다중 원천 계획을 보강한다.

UM3R-A에서는 기존 단일 source 작업 계약을 호환 유지하면서 모든 비종료 진열 보충 작업을 원천 재고별로 한 번 집계하는 `도심마트재고가용성WorldState`를 추가했다. 각 재고와 진열 보충 결과가 `OnHand / Allocated / Available`을 구분하며, 다른 진열대 작업도 가용 수량에서 차감한다. 할당 초과는 `InventoryOversubscribed`로 차단하고, 수량은 존재하지만 전부 다른 작업에 할당된 경우는 물리 재고 없음과 구분해 `AvailableQuantityInsufficient`로 표시한다. targeted 9건과 Unity core 전체 160건이 통과했으며 다중 원천 SourcePlan과 확대 무결성 matrix는 아직 미구현이다.

UM3R-B/C에서는 `도심마트운영작업재고할당Data`와 typed allocation World node를 추가했다. 명시적 allocation이 없는 기존 작업은 legacy source 한 건으로 정규화하고, 명시적 allocation이 있으면 legacy source를 중복 계산하지 않는다. 보충 후보는 여러 후방 위치의 실제 Available 수량을 Stable ID 순서의 deterministic fallback으로 배분한 SourcePlan을 가지며 합계가 후보와 일치해야 preview할 수 있다. 완료·해제 할당 제외, 다중 원천, 수량 합계·단위·원천·중복 ID 검증을 포함한 targeted 22건과 Unity core 전체 168건이 통과했다.

## 2. 현재 저장소에서 확인된 기준선

### 2.1 현재 Unity 흐름

현재 도심마트는 다음 흐름으로 동작한다.

```text
도심마트상품ApiModel
  → 도심마트ApiMapper
  → 도심마트ScreenModel
  → I도심마트조회UseCase
  → 도심마트SceneController
  → 도심마트View / 상품진열대View / 가격표View / 재고상태View
```

Simulation도 `Simulated도심마트조회UseCase`가 `도심마트ScreenModel`을 직접 만든다. 이 구조는 primitive presentation 검증에는 적합했지만 다음 책임이 섞여 있다.

| 현재 위치 | 섞인 책임 |
| --- | --- |
| `도심마트ApiMapper` | wire mapping과 판매 가능 상태 분류, ScreenModel 생성 |
| `Simulated도심마트조회UseCase` | 사실 fixture와 표현 상태·문구 생성 |
| `상품진열대View` | 재고 상태에서 표시 상자 수 계산, 상품명에서 색상 결정 |
| `재고상태View` | 상태 code에서 색상 결정 |
| `도심마트View` | 상품 상세 문구 조립 |
| `도심마트SceneController` | 조회·검증·표시 조율은 수행하지만 공통 `WorldReadRuntime`과 last-success reconcile을 아직 사용하지 않음 |

### 2.2 현재 서버 Projection의 정확한 의미

현재 operational Unity route인 `api/v1/orderer/mart/products`는 마트 관리자 API가 아니다. `마트공개상품조회UseCase`는 주문자에게 공개된 상품과 별도로 투영된 `판매가능수량`만 제공하며 다음 안내를 명시한다.

> 표시 수량은 내부 창고 재고를 직접 공개한 값이 아니라 마지막 투영 시각 기준의 판매 가능 수량입니다.

따라서 이 응답으로 다음 값을 만들면 안 된다.

- 보관 재고
- 진열 재고
- 예약 재고
- 실제 선반의 상자 수
- 진열 보충 가능 수량
- 마트 직원 작업과 배정

현재 `SsalddelContext`에는 마트의 `마트공개상품`, `마트주문요청`, `마트주문`, `마트주문상품`과 창고의 `입고요청`, `입고상품`, `재고이력`, `출고예정`, `피킹포장작업`, `재고이동` 등이 존재한다. 그러나 마트 운영에 필요한 `진열대`, `진열재고`, `마트작업`, `마트직원배정` canonical source는 확인되지 않았다. 창고 객체를 마트 객체로 간주하지 않고, 향후 서버 aggregate가 관계를 명시적으로 투영해야 한다.

## 3. 재설계의 핵심 결정

도심마트 조회를 두 개의 의미로 분리한다.

| Projection | 사용자 | 보존할 의미 | 금지 |
| --- | --- | --- | --- |
| 공개 상품 Projection | 주문자·공공 관찰 | 가격, 판매 가능 수량, 출처, 기준시각 | 물리 진열·내부 재고·직원 업무 추론 |
| 마트 운영 Projection | 권한 있는 관리자·직원 | 위치별 재고, 진열대, 작업, 배정, capability, revision | 공개 route를 내부 운영 원장처럼 사용 |

기존 route는 공개 상품 Projection으로 유지한다. 관리자 World는 별도의 authorized server Projection이 생기기 전까지 `Simulation`으로만 검증하며, operational로 위장하지 않는다.

## 4. 목표 읽기 흐름

```text
Authorized Market Operation API
  → 도심마트운영ApiModel
  → 도심마트운영DataMapper
  → 도심마트운영DataSnapshot
  → 도심마트SharedWorldInterpreter
  → 도심마트SharedWorldState + WorldGraphIndex
  → 마트관리자PerspectiveInterpreter
  → 마트관리자PerspectiveWorldState
  → 도심마트PresentationProjector
  → 도심마트PresentationSnapshot
  → StableIdChangeSet
  → 도심마트ViewApplicator
  → MonoBehaviour View / NPC / UI
```

Application 생명주기는 기존 `WorldReadRuntime`을 사용한다.

```text
RefreshDataAsync       Data 재조회부터 전체 실행
ReinterpretShared      기존 Data를 새 업무 rule로 재해석
ReinterpretPerspective 같은 authorized World를 다른 역할·목적으로 재해석
Reproject              theme·layout·surface만 다시 투영
```

authorization scope가 바뀌면 기존 private last-success와 selection을 폐기하고 새 Data를 조회한다.

## 5. Data 계층

### 5.1 목표 계약

`도심마트운영DataSnapshot`은 서버가 허용한 사실만 보존한다.

```text
도심마트운영DataSnapshot
├─ MarketStableId / WorldId
├─ DataOrigin / AuthorizationScope
├─ DataRevision / AsOf / Source
├─ 상품Data[]
├─ 위치별재고Data[]
├─ 진열대Data[]
├─ 작업재고할당Data[]
├─ 입고요청Data[]
├─ 마트주문Data[]
├─ 마트작업Data[]
└─ 직원배정Data[]
```

모든 배열이 첫 버전에 필요한 것은 아니다. 첫 진열 보충 slice에서는 최소한 다음 사실이 서버 Projection 또는 명시적 simulation fixture에 있어야 한다.

- 상품 stable ID와 단위
- 보관 위치 stable ID와 보관 수량
- 진열대 stable ID, 대상 상품과 진열 수량·수용량
- 진행 중 진열 보충 작업과 대상·수량
- 모든 비종료 작업이 점유한 원천 재고 할당과 위치별 수량
- 작업 가능한 직원 또는 서버가 허용한 배정 후보
- source, 기준시각, Data revision, Operational/Simulation origin

Data Mapper는 wire field, null, 단위, 시간과 identity만 검증한다. `부족`, `긴급`, `보충 필요` 또는 색상 같은 의미를 만들지 않는다.

### 5.2 공개 상품 호환 경로

현재 공개 상품 route는 다음 별도 경로로 유지한다.

```text
마트공개상품ApiModel
  → 마트공개상품DataSnapshot
  → 공개상품SharedWorldState
  → 주문자 또는 공공관찰 Perspective
  → 기존 진열 sample Presentation
```

이 호환 경로의 상자 수는 실제 선반 재고가 아니라 `판매 가능 상태를 축약한 시각적 placeholder`라고 명시한다. 관리자 진열 보충 판단에는 사용하지 않는다.

## 6. Shared Interpretation 계층

### 6.1 World node

마트 Shared World는 다음 typed node를 사용한다.

- `도심마트WorldNode`
- `상품WorldState`
- `재고WorldState`
- `보관위치WorldState`
- `진열대WorldState`
- `마트주문WorldState`
- `마트작업WorldState`
- `마트직원WorldState`

각 node는 `WorldStableId`, `InterpretationRevision`과 source lineage를 가진다. 하나의 거대한 nullable payload로 합치지 않는다.

### 6.2 기존 typed graph 재사용

새 graph framework를 만들지 않고 기존 `WorldGraphIndex<TNode>`와 `WorldRelationKind`를 사용한다.

| 마트 의미 | 공통 relation |
| --- | --- |
| 마트가 상품·진열대·작업을 포함 | `Contains` |
| 재고가 보관 위치 또는 진열대에 있음 | `LocatedAt` |
| 진열 보충·피킹 작업이 상품·재고·진열대를 대상으로 함 | `Targets` |
| 작업에 직원이 배정됨 | `AssignedTo` |
| 작업이 주문·입고 요청에서 파생됨 | `DerivedFrom` |

relation의 방향은 feature contract에서 고정하고, View가 LINQ로 관계를 다시 구성하지 않는다.

### 6.3 진열 보충 해석

Shared Interpreter가 각 진열대에 대해 다음 결과를 만든다.

```text
진열보충WorldState
├─ ShelfWorldId
├─ ProductWorldId
├─ DisplayQuantity
├─ DisplayCapacity
├─ BackroomOnHandQuantity
├─ BackroomAllocatedQuantity
├─ BackroomAvailableQuantity
├─ ActiveTaskWorldIds[]
├─ SourcePlan[]
├─ NeedCode
├─ CandidateQuantity
├─ BlockReasonCodes[]
└─ RuleRevision / SourceWorldIds[]
```

판정 예시는 다음과 같다.

- 진열량이 rule의 목표 이하이고 보관 재고가 있음: `ReplenishmentCandidate`
- 진열량이 부족하지만 보관 재고가 없음: `InboundRequired`
- 동일 진열대의 진행 작업이 있음: `TaskAlreadyActive`
- source가 오래됐거나 위치 관계가 불완전함: `DataInsufficient`
- 진열량이 목표 범위임: `NoActionNeeded`

`CandidateQuantity`는 진열 수용량, **전역 할당을 차감한 보관 가용 수량**과 기존 진행 작업을 고려한 후보값이다. 실제 이동 가능 수량과 상태 전이는 서버 Command가 현재 revision을 다시 검증해 확정한다.

### 6.4 재고 할당 무결성

진열대별 후보를 계산하기 전에 원천 재고마다 다음 수량을 한 번 계산한다.

```text
OnHandQuantity
  = 서버 또는 simulation 원장에 기록된 위치별 물리 수량

AllocatedQuantity
  = 같은 원천 재고를 점유한 모든 비종료 작업 할당 수량

AvailableQuantity
  = max(0, OnHandQuantity - AllocatedQuantity)
```

첫 slice의 `AllocatedQuantity`에는 모든 진열대의 활성 진열 보충 작업을 포함한다. 주문 예약, 피킹, 위치 이동과 폐기 보류가 canonical Data에 추가되면 같은 원천 재고 할당 원장에 명시적으로 합류시킨다. Unity가 공개 판매 가능 수량이나 작업 대상만 보고 숨은 예약을 추론하지 않는다.

현재 단일 `SourceInventoryStableId`는 호환 facade로 유지하되 새 기본 계약은 작업별 할당 배열을 사용한다.

```text
도심마트작업재고할당Data
├─ AllocationStableId
├─ TaskStableId
├─ InventoryStableId
├─ LocationStableId
├─ Quantity / Unit
├─ State
└─ Revision / Source lineage
```

하나의 보충 후보는 여러 위치를 포함하는 결정적 원천 계획을 가질 수 있다.

```text
SourcePlan
├─ backroom:A → 3개
└─ backroom:B → 3개
```

계획의 합은 `CandidateQuantity`와 같아야 한다. 위치 우선순위나 이동비용 데이터가 없으면 Stable ID 정렬은 재현 가능한 fallback일 뿐 최적 동선으로 설명하지 않는다. 다음 상태에서는 후보를 실행 가능으로 표시하지 않는다.

- 같은 원천 재고의 할당 합이 OnHand를 초과함: `InventoryOversubscribed`
- 작업 할당이 존재하지 않는 원천 재고를 가리킴: `AllocationSourceUnknown`
- 재고와 할당 단위가 다름: `AllocationUnitMismatch`
- 후보 수량을 원천 위치에 완전히 배분할 수 없음: `SourcePlanIncomplete`
- 전역 할당 차감 후 가용 수량이 부족함: `AvailableQuantityInsufficient`

Operational에서는 서버가 canonical allocation을 투영하고 Command 시점에 다시 예약해야 한다. Simulation에서는 동일 계약의 deterministic allocation ledger를 사용하되 운영 예약으로 표현하지 않는다.

### 6.5 권한과 가능성의 구분

해석층은 업무 필요성과 후보를 계산할 수 있지만 권한을 새로 만들지 않는다.

```text
NeedCode                  Unity 해석: 보충이 필요한가
CandidateQuantity         Unity 해석: 어떤 후보가 가능한가
ServerCapabilityCode      서버 Projection: 이 사용자에게 요청 권한이 있는가
Command validation        서버: 현재 상태에서 실제로 생성 가능한가
```

`ServerCapabilityCode`가 없으면 operational 버튼을 활성화하지 않고 read-only 또는 simulation preview로 남긴다.

## 7. 관리자 Perspective Interpretation

`마트관리자PerspectiveInterpreter`는 같은 Shared World를 다음 문맥으로 읽는다.

```text
Role   = MarketManager
Intent = ReviewReplenishment | ReviewReceiving | ReviewOrders
Zone   = urban-market
Focus  = 선택 상품·진열대·작업
Mode   = Operational | Simulation
```

첫 slice 출력인 `마트관리자PerspectiveWorldState`는 관리자가 출근 후 30초 안에 확인할 수 있는 운영 문제 queue를 만든다.

- `UrgentActions`: 현재 진열 수량이 0이거나 후방 가용재고가 없어 입고 검토가 필요한 항목
- `PendingActions`: 전역 가용재고와 완전한 SourcePlan이 있어 보충 검토 가능한 항목
- `InProgress`: 이미 작업·할당이 존재해 새 요청보다 진행 확인이 필요한 항목
- `DataAttention`: 중복·단위·관계·할당 오류로 판단할 수 없는 항목
- `NoActionNeeded`: 정상 상태이며 기본 작업 queue에서는 숨기는 항목
- 현재 focus와 graph로 연결된 상품·재고·위치·작업·직원
- 서버가 허용한 interaction intent

각 문제에는 `PriorityClass`, `PriorityScore`, `PriorityReasonCodes`, `RuleRevision`과 source lineage를 둔다. Stable ID는 같은 우선순위 안에서만 결정적 tie-breaker로 사용한다. 우선순위는 최소한 다음 순서를 따른다.

1. 데이터 무결성 오류와 할당 초과: 잘못된 실행을 막기 위한 확인 대상
2. 진열 품절 또는 후방 가용재고 없음: 입고·대체 업무 검토 대상
3. 가용재고와 SourcePlan이 있는 진열 보충 후보: 실행 검토 대상
4. 진행 중 작업: 지연 여부를 아직 판단하지 않는 모니터링 대상
5. 정상 상태: action queue에서 제외

판매속도·수요 시간창이 없는 상태에서 `곧 품절`, `몇 시간 후 품절` 또는 매출 영향 점수를 만들지 않는다. 해당 해석은 서버의 판매 시계열과 `DemandWindow`, 계산 rule이 추가된 뒤 별도 `StockoutRiskWorldState`로 확장한다. 유통기한·폐기, 재고 실사 차이도 별도 canonical Data가 생기기 전에는 추론하지 않는다.

Perspective는 다른 직원의 개인정보나 내부 계약 원문을 추론하지 않는다. 역할 변경이 authorization scope 변경을 동반하면 `ReinterpretPerspective`가 아니라 새 authorized Data query를 사용한다.

## 8. Presentation 계층

### 8.1 Presentation Snapshot

Presentation Projector는 다음 surface를 만든다.

```text
도심마트PresentationSnapshot
├─ ManagerSummarySurface
├─ PriorityQueueSurface
├─ ShelfSurface
├─ ProductCrateSurface
├─ TaskMarkerSurface
├─ WorkerNpcSurface
├─ StatusPanelSurface
├─ SourcePlanSurface
└─ DetailPanelSurface
```

각 item은 `PresentationStableId`, item revision과 source `WorldStableId` lineage를 가진다. 색, label, 표시 상자 수, highlight와 상세 문구는 Projector 또는 visual policy가 결정한다.

### 8.2 기존 View에서 이동할 책임

| 현재 View 책임 | 목표 위치 |
| --- | --- |
| 상태별 표시 상자 수 | `도심마트PresentationProjector` 또는 `도심마트VisualPolicy` |
| 상품명별 primitive 색상 | `도심마트VisualPolicy` |
| 재고 상태별 색상 | `도심마트VisualPolicy` |
| 상세 panel 문자열 | `도심마트DetailPresentationProjector` |
| 관련 상품·작업 탐색 | Shared graph + Perspective Interpreter |
| Instantiate·SetActive·Transform·Text·Material | 기존 MonoBehaviour View/Applicator |

View는 최종적으로 `도심마트상품ScreenModel`이나 DataSnapshot을 받지 않고 surface item 또는 change set만 적용한다.

## 9. Stable ID, refresh와 selection

- Source ID, World ID와 Presentation ID를 구분한다.
- 상품 하나가 shelf, detail panel과 task marker에 동시에 나타나도 서로 다른 Presentation ID를 사용한다.
- 기존 `StableIdReconciler<T>`로 Added/Updated/Removed/Unchanged를 계산한다.
- 모든 Data·Shared·Perspective·Presentation validation과 diff가 성공한 뒤에만 last-success를 교체한다.
- 최초 실패는 빈 상태와 재시도를 표시한다.
- refresh 실패는 같은 authorization scope의 마지막 성공 World를 유지하고 safe error code만 별도 표시한다.
- 선택은 `SelectionStateStore`에 World ID로 보존하고, refresh 뒤 대상이 사라졌으면 해제한다.

## 10. 진열 보충 interaction과 Command 경계

```text
Shared Interpretation: ReplenishmentCandidate
  → Manager Perspective: 요청 검토 가능
  → Presentation: 진열대·보관 위치 강조, 후보 수량 panel
  → 사용자 선택
  → Preview
  → 명시적 확인
  → 서버 진열보충작업 Command
  → 권한·재고·진열 수용량·중복 작업·expected revision 재검증
  → canonical 작업 생성
  → 마트 운영 Projection 재조회
  → 새 작업과 NPC 이동 Presentation
```

첫 구현은 다음 두 단계로 나눈다.

1. **Simulation/read-only proof**: Data→Shared→Manager Perspective→Presentation과 후보·차단 사유를 검증한다. 서버 상태는 바꾸지 않는다.
2. **Operational closure**: 서버 canonical 진열대·위치별 재고·작업·capability와 Command가 준비된 뒤 연결한다.

직원 NPC의 보관 위치→진열대 이동과 진열 animation은 canonical 작업 상태의 표현이다. NPC 도착은 Presentation Event이며 자동으로 서버 작업을 완료하지 않는다.

## 11. 기존 클래스 Migration Map

| 현재 클래스 | 조치 |
| --- | --- |
| `도심마트상품ApiModel`, `도심마트목록ApiModel` | 공개 상품 wire model로 유지 |
| `도심마트ApiMapper` | Data Mapper와 공개 상품 Interpreter/Projector로 분리, 기존 `Map`은 facade 유지 |
| `I도심마트Repository` | 점진적으로 DataSnapshot 반환 port 추가, 기존 ScreenModel 반환은 호환 유지 |
| `Simulated도심마트조회UseCase` | simulation DataSnapshot fixture/query로 이동 |
| `도심마트ScreenModel` | 기존 공개 sample Presentation facade로 유지 후 새 surface 모델로 대체 |
| `도심마트ScreenModelValidator` | Data·Shared·Perspective·Presentation validator로 분리 |
| `도심마트SceneController` | `WorldReadRuntime` 호출, runtime status와 change set 적용만 조율 |
| `도심마트View` | full model render 대신 surface applicator와 interaction event 담당 |
| `상품진열대View`, `재고상태View`, `가격표View` | 최종 Presentation item 적용만 담당 |
| `도심마트LifetimeScope` | Query, Shared Interpreter, Manager Perspective, Projector, Reconciler, Selection과 Controller 등록 |

## 12. 서버 선행 계약

Operational 진열 보충을 시작하기 전에 서버에서 다음을 확정해야 한다.

| 필요 계약 | 현재 상태 | 완료 조건 |
| --- | --- | --- |
| 마트·World scope와 관리자 authorization | 공개 주문자 route만 확인 | 권한 있는 aggregate route와 capability 제공 |
| 진열대와 진열 위치 | canonical source 미확인 | stable ID, 상품 relation, 수용량, revision |
| 보관 위치별 재고 | 공개 판매 가능 수량만 존재 | 위치별 수량·예약·진행 작업을 중복 계산 없이 투영 |
| 작업별 재고 할당 | canonical source 미확인 | OnHand·Allocated·Available과 작업별 다중 원천 할당을 투영 |
| 진열 보충 작업 | canonical source 미확인 | 상태, 대상, 수량, 다중 원천 계획, 배정, expected revision |
| 작업 Command | 미구현 | preview와 confirm 분리, 멱등 Command ID, 권한·revision 재검증 |
| 직원 배정 | 마트 source 미확인 | 개인정보 최소화된 worker stable ID와 작업 relation |

창고의 입고·피킹·재고 객체는 연계 가능한 source 후보이지만 마트 내부 위치·작업과 동일 객체로 간주하지 않는다. 서버 Projection에서 `DerivedFrom`, handoff 또는 별도 relation으로 연결한다.

## 13. 구현 우선순위

### UM0. 계약·호환 기준 고정 — 완료

- 공개 상품과 관리자 운영 Projection 분리
- 현재 클래스 migration test 작성
- existing route·JSON·sample facade 유지

### UM1. Data 분리 — 완료

- 공개 상품 `ApiModel → DataSnapshot` 전환
- simulation fixture도 DataSnapshot 생성
- source·as-of·origin·revision validation

### UM2. Shared World 기반 — 완료

- typed market node와 기존 `WorldGraphIndex` 적용
- 공개 상품에서는 판매 가능 의미만 해석
- 관리자 simulation fixture에는 위치별 재고·진열대·작업 관계 추가

### UM3. 진열 보충 Interpretation — 완료

- 필요·후보 수량·차단 사유·중복 작업 판정
- rule lineage와 deterministic headless test

### UM3R. 재고 할당 무결성 보강 — 완료

- **UM3R-A 완료**: `OnHand / Allocated / Available`을 원천 재고별로 전역 계산
- **UM3R-A 완료**: 모든 진열대의 비종료 작업 할당을 후보 계산 전에 차감하고 초과 할당 차단
- **UM3R-B 완료**: 단일 source facade를 유지하면서 명시적 작업 allocation과 다중 원천 `SourcePlan` 계약 추가
- **UM3R-C 완료**: 같은 상품·여러 진열대, 여러 작업, 완료·해제 제외, 초과 할당, 단위·수량·원천·중복 ID 회귀 test
- **UM3R-C 완료**: integrity validation과 완전한 SourcePlan을 통과한 후보만 preview 허용

### UM4. 관리자 Perspective와 Presentation — 완료

- 30초 요약과 `UrgentActions / PendingActions / InProgress / DataAttention` queue
- rule·reason·lineage가 있는 관리자 우선순위
- shelf/task/detail surface projector
- 원천 위치별 수량 계획 surface
- View의 색·상자 수·문구 판단 제거

구현은 `마트관리자PerspectiveInterpreter`와 `도심마트PresentationProjector`에 두었다. 첫 slice는 Simulation/read-only이며 `DataAttention → UrgentActions → PendingActions → InProgress` 순서로 action queue를 만들고, 같은 priority 안에서만 Stable ID를 tie-breaker로 사용한다. `ManagerSummary`, priority queue, shelf, task, detail과 source-plan surface는 각각 Presentation Stable ID와 source World lineage를 보존한다. 기존 `도심마트SceneController`와 View의 실제 surface 적용 전환은 UM5로 남긴다.

### UM5. Runtime·reconcile 전환

- **UM5-A 완료**: `WorldReadRuntime`과 `AuthorizedUserWorld` context-scoped query 사용
- **UM5-A 완료**: initial/refresh 오류와 last-success 회귀 검증
- **UM5-A 완료**: selection 유지·해제와 surface별 change set
- **UM5-B 완료**: 기존 공개 상품 compatibility Controller/View는 유지하고 별도 manager Controller/View가 summary·queue·shelf·task·source-plan·detail change set을 적용하도록 전환
- **UM5-B 완료**: manager shelf selection을 `도심마트ManagerRuntime` focus로 되돌리고 refresh 실패 시 마지막 성공 surface를 유지
- **UM5-B 검증 경계**: 실제 Unity 프로젝트에 sample과 VContainer 1.18.0을 가져와 EditMode compile·16/16을 확인했으며, manager Scene builder 실행·Scene 저장·Game View는 수행하지 않음

### UM6. Operational server vertical slice

- authorized 마트 운영 Projection
- canonical 진열대·위치별 재고·진열 보충 작업
- preview·confirm·Command·canonical re-query

### UM7. NPC 표현

- 서버 작업 상태에서 semantic route 생성
- 보관 위치→진열대 이동과 action Presentation
- 도착 Event와 서버 완료 Command 분리

### UM8. 입고·발주 연결

- 공급처·입고 요청·예정 수량·ETA·상태의 개인정보 최소 Projection
- `InboundRequired`를 문제 알림에서 입고 검토 흐름으로 연결
- 발주·입고 Command는 preview·confirm·canonical 재조회로만 반영

### UM9. 운영 지표 확장

- 판매속도와 명시적 시간창 기반 품절 위험
- 유통기한·폐기 예정과 재고 실사 차이
- 별도 rule revision과 근거가 있는 병목·지연 해석

### 후속 업무

1. [도심마트 공급 계약 경영 Simulation](UrbanMarketSupplyManagementSimulationDesign.md)의 SC1-C Demand Scenario→deterministic Simulation Order Stream
2. SC2~SC7 단일 마트 playable
3. 입고 필요 후보 → Simulation/운영 입고 요청 → 입고 → 보관 재고 증가
4. 주문 → 예약 → 피킹 → 포장·출고 준비 → 출고
5. 본사·공급처와 여러 마트의 보충 수요 aggregate

UM0~UM5는 기존 route를 깨지 않는 Unity migration이다. UM5 뒤 공급 계약 경영 track은 기존 진열 보충을 대체하지 않고 원인과 결과의 상류를 추가한다. UM3R의 operational 할당과 SC9의 운영 계약 연결은 각각 서버 canonical reservation Projection과 기존 공급중개 권한·동의·개별 발주 UseCase가 필요하므로 Simulation proof와 운영 보장을 구분한다.

## 14. 테스트와 검증

| 범위 | 최소 검증 |
| --- | --- |
| Data | null·중복 ID·단위·as-of·origin·낮은 revision 거부 |
| Shared Interpretation | 진열 충분, 보충 후보, 보관 재고 없음, 전역 활성 작업 차감, stale data |
| Allocation | 같은 상품·여러 진열대, 여러 원천 위치, 초과 할당, 단위 불일치, 불완전 SourcePlan |
| Graph | unknown node·중복 relation 거부, 정·역방향 탐색 |
| Perspective | 관리자 intent별 우선순위, focus relation, authorization 비확대 |
| Presentation | 색·문구·표시 수량이 View 밖에서 결정됨, item revision 안정성 |
| Runtime | initial error, refresh last-success, cancellation, scope 변경 시 cache·selection 폐기 |
| Command | expected revision, 중복 Command 멱등성, 서버 거부 뒤 화면 미변경, 성공 후 canonical 재조회 |
| Unity | package compile, sample Scene wiring, View applicator, NPC는 NavMesh·Animator 별도 runtime proof |

headless test, Unity Editor compile, Scene wiring, 실제 서버 연결과 operational Command를 각각 별도 증거로 보고한다.

## 15. 완료 정의

첫 재정비 완료는 다음을 모두 만족할 때다.

1. 공개 판매정보와 관리자 운영정보가 계약상 분리된다.
2. ApiMapper와 fixture가 ScreenModel을 직접 만들지 않는 기본 경로가 생긴다.
3. 마트 상품·재고·진열대·작업 관계가 Shared Interpretation에 존재한다.
4. 진열 보충 필요·후보·차단 사유가 deterministic하게 계산된다.
5. 모든 비종료 작업을 반영한 `OnHand / Allocated / Available`과 완전한 다중 원천 계획이 존재한다.
6. 관리자 Perspective가 근거와 rule revision이 있는 긴급·대기·진행·판단 불가 queue를 만든다.
7. Presentation Projector가 색·문구·표시 상자 수를 결정하고 View는 적용만 한다.
8. stable-ID reconcile과 refresh last-success 정책을 유지한다.
9. operational 실행은 서버 Command와 canonical 재조회로만 반영된다.
10. NPC 도착과 animation이 서버 업무 완료로 취급되지 않는다.
11. 기존 공개 route와 primitive sample 소비자가 compatibility facade 아래에서 유지된다.

이 상태가 된 뒤 도심마트는 상품 진열 샘플이 아니라, 관리자가 현재 업무의 의미와 다음 검토 대상을 이해하는 운영 World로 확장할 수 있다.
