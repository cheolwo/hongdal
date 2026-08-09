# Unity 개념 카드 Presentation 패턴

## 1. 목적과 상태

Unity World는 데이터를 3D 공간에 표시하는 데서 끝나지 않고, 사용자가 업무 개념과 판단 근거를 공간 안에서 학습하고 다음 행동을 선택할 수 있게 해야 한다.

```text
보인다
  → 선택한다
  → 카드로 의미를 이해한다
  → 현재 상태와 근거를 확인한다
  → 허용된 행동을 Preview한다
  → 명시적으로 확인한다
```

이 문서는 이를 위한 공통 `Concept Card` Presentation 패턴을 확정한다. 현재 CC1 공통 contract·Projector와 CC2 대표 NPC 7-card deck adapter까지 구현됐으며 View와 Unity Scene 연결은 아직 구현되지 않았다.

## 2. 계층과 책임

```text
Authorized Data Snapshot
  → Shared World Interpretation
  → Role + Intent Perspective WorldState
  → Learning Card Projector
  → ConceptCardDeckPresentationModel
  → ConceptCardView / CardDeckView
  → VisualSkinAdapter
  → Synty INTERFACE 또는 다른 UI asset
```

- Interpretation은 업무 의미, 상태, 판단 근거와 행동 후보를 만든다.
- Learning Card Projector는 사용자에게 보일 문구, 값, 근거 행, 주의와 행동 항목을 결정한다.
- View는 이미 결정된 PresentationModel을 그리며 수량, 위험, 권한을 다시 계산하지 않는다.
- `VisualSkinAdapter`는 색, icon, prefab, animation과 layout을 교체 가능한 표현으로 한정한다.
- NPC, 진열대, 공급처와 입고 Dock은 카드의 공간 진입점이지 Data authority가 아니다.

따라서 `ConceptCard`는 특정 Synty prefab 이름이 아니다. asset을 교체해도 stable ID, source lineage, revision, 의미와 capability 경계는 유지한다.

## 3. 네 종류의 카드

### 3.1 Concept Card

`확정 수요`, `의향 수요`, `공급계약`, `최소주문량`, `공동수령`, `Lead Time`처럼 “이 개념은 무엇인가?”에 답한다.

- 한 줄 정의와 현재 맥락의 예시를 함께 제공한다.
- 비슷하지만 다른 개념과의 경계를 명시한다.
- `의향 수요 != 확정 수요`, `대표성 != 개별 주문 권한` 같은 불변식을 숨기지 않는다.

### 3.2 Status Card

선택한 대상의 현재 사실을 보여준다.

- 기준 시각, revision, unit과 `Simulation / Operational` mode를 표시한다.
- 현재 재고와 예정 입고, 의향과 확정, 실제 상태와 Preview를 합쳐 표현하지 않는다.
- stale last-success를 표시할 수 있지만 최신 상태인 것처럼 보이게 하지 않는다.

### 3.3 Reason Card

“왜 이런 상태 또는 판단인가?”에 답한다.

- 입력, 조정, 결과와 한계를 계산 행으로 분리한다.
- 각 행에 source stable ID와 가능한 경우 rule revision을 보존한다.
- 근거 없는 예상, 매출 영향, 품절 시간 또는 단일 종합 점수를 만들지 않는다.

### 3.4 Action Card

현재 사용자가 다음에 검토할 수 있는 행동을 보여준다.

- 서버 또는 Simulation session이 허용한 intent만 표시한다.
- 사용할 수 없는 행동은 숨겨진 권한으로 추측하지 않고 block reason을 제공한다.
- `Preview → 명시적 확인 → Command → canonical 재조회` 순서를 유지한다.
- 카드 클릭, NPC 도착, 대화 완료와 animation은 Command 성공이 아니다.

## 4. 공통 Presentation 계약 후보

첫 구현에서는 아래 의미를 최소 공통 계약으로 둔다. 실제 C# 이름은 주변 공통 Presentation contract와 정합성을 확인한 뒤 확정한다.

```text
ConceptCardDeckPresentationModel
├─ DeckStableId
├─ AnchorWorldObjectRef
├─ RoleCode / IntentCode / ModeCode
├─ PresentationRevision
├─ SelectedCardStableId?
└─ Cards[]

ConceptCardPresentationModel
├─ StableId
├─ CardKindCode: Concept | Status | Reason | Action
├─ ConceptStableId
├─ TitleText / SummaryText / PrimaryValueText?
├─ SimulationLabel?
├─ EvidenceRows[]
├─ Cautions[]
├─ RelatedConceptRefs[]
├─ ActionItems[]
├─ SourceLineage[]
└─ PresentationRevision

ConceptCardEvidenceRow
├─ LabelText / ValueText
├─ CalculationRoleCode: Input | Adjustment | Result | Limitation
├─ SourceStableId?
└─ RuleRevision?

ConceptCardActionItem
├─ IntentCode / LabelText / EffectCode
├─ IsAvailable
└─ BlockReasonCodes[]
```

`PrimaryValueText`와 표시 순서는 Projector가 결정한다. Deck, Card, World Object와 canonical 업무 객체는 서로 다른 identity level을 사용한다.

## 5. 선택, 이동과 갱신

대표 NPC에서 시작한 첫 탐색은 다음처럼 이어진다.

```text
공동주택 대표 NPC
  → 공동주택 주문 상태
  → 확정 수요의 의미
  → 의향 수요와의 차이
  → 공급 부족의 근거
  → 공급 검토 행동
```

- 관련 개념 이동은 `RelatedConceptRefs`로 표현하고 대상 Perspective 범위를 벗어나면 authorized source를 다시 조회한다.
- surface reconcile은 deck/card stable ID와 presentation revision을 사용한다.
- 권한 또는 역할이 바뀌면 기존 선택과 비공개 deck을 즉시 제거한다.
- refresh 실패 시 last-success deck을 stale로 유지할 수 있지만 Simulation fixture로 대체하지 않는다.

## 6. 도심마트 첫 카드 Deck

첫 anchor는 합성 대표 NPC `npc:sim:residential-group-representative:1`이다.

| 순서 | 카드 | 핵심 내용 |
| --- | --- | --- |
| 1 | 공동주택 감자 주문 상태 | 의향 410kg·67명, 확정 385kg·61명, 문의 상태 |
| 2 | 확정 수요 Concept | 주민별 최종 확인이 끝나 hard demand에 반영되는 수요 |
| 3 | 의향 수요 Concept | 공급 검토 참고값이며 주문과 hard demand가 아님 |
| 4 | 공동수령 Concept | 확정 fulfillment 이후 연결되는 수령 후보 |
| 5 | 공급 상태 | 현재 재고, 처리 가능한 예정 입고, 공급 충족과 부족 |
| 6 | 공급 부족 Reason | 입력과 차감 근거를 행 단위로 설명 |
| 7 | 공급 검토 Action | 공급처 추가 검토, 납품 일정 조정, 대표에게 조건 제안 |

공동주택 확정 수요 `385kg`과 기본 Simulation 수요까지 합친 전체 hard demand `2,105kg`을 같은 값처럼 표시하지 않는다. 카드마다 수요 source를 명시한다.

## 7. 공간별 anchor

| 공간 진입점 | 기본 카드 Deck |
| --- | --- |
| 공동주택 대표 NPC | 집단 주문 상태, 의향/확정 수요, 공동수령, 문의 행동 |
| 감자 진열대 | 진열·후방 재고, 연결 수요, 다음 납품, 보충 행동 |
| 공급처 NPC·미팅 테이블 | 공급처, Offer, 가격·최소주문량·납품 조건, 이행 상태 |
| 입고 Dock | 약정 수량, 실제 도착·검수, 지연·부분 납품, 후방재고 인계 |
| 관리자 사무실 | 수요 브리핑, 공급 포트폴리오, 현금, 계약 Preview |

각 anchor는 동일한 카드 문법을 사용하지만 role과 intent가 다른 deck을 연다. GameObject가 같은 canonical 정보를 무권한으로 확장하지 않는다.

## 8. 다른 World로의 확장

- 농장: 토양수분, 작기, 관측값, 출처와 관측 한계
- 가격: 도매가격, 생산자 수취가격, 판매가와 서로 다른 기준 시각
- 물류: 배차, 입고, 피킹, 출고, 하차와 역할별 행동 경계
- 공동구매: 관심, 의향, 확정, 가원장, 실원장과 공동수령
- 공공데이터: 출처, 기준 시각, 갱신 주기, 공간 정밀도와 이용 한계

이 확장은 공통 카드 View를 복제하는 작업이 아니라 각 도메인의 Interpretation과 Projector가 같은 Presentation 문법을 사용하는 작업이다.

## 9. 개인정보와 설명 가능성

- 마트 관리자와 대표 deck에 다른 주민의 이름, 사용자 ID, 연락처, 상세주소, 동·호수, 주민별 수량과 결제 상세를 넣지 않는다.
- source, unit, 기준 시각, 정밀도, 갱신 주기와 한계를 카드가 숨기지 않는다.
- Simulation 값에는 mode, scenario, seed와 rule revision을 추적할 수 있어야 한다.
- 이유를 설명할 수 없는 위험 점수나 계산 불일치는 정상 카드가 아니라 `DataAttention`으로 보낸다.

## 10. 구현 우선순위

| 단계 | 구현 | 완료 조건 |
| --- | --- | --- |
| CC0 | 공통 카드 문법과 결정 확정 | 이 문서와 D-031이 기준 문서에 연결됨 |
| CC1 완료 | 공통 deck/card/evidence/action 계약과 Projector helper | identity, revision, mode, lineage와 권한 제거 집중 9건·Unity core 전체 207건 통과 |
| CC2 완료 | 대표 NPC 7-card deck adapter | RG4·SC3~SC5의 집단 수요·브리핑 값을 재계산 없이 카드로 투영; 집중 25건·Unity core 전체 217건 통과 |
| CC3-A + RG4-NPC-C1 완료 | `ConceptCardView`, asset-neutral skin, imported sample과 임시 Scene wiring | 실제 Unity EditMode 3/3에서 대표 NPC·7장 카드·선택·NavMeshData·Mecanim parameter 검증; Scene 저장 없음 |
| CC3-B + RG4-NPC-C2 | City Pack `VisualRoot`·Humanoid Animator 교체와 실제 Scene wiring | 대표 NPC 선택부터 관련 카드 탐색과 이동까지 Game View 검증 |
| SC6~SC7 | Action Card의 Preview·Confirm·Tick과 UM4 하류 연결 | 명시 확인, 멱등성, canonical/simulation 재조회와 결과 카드 갱신 |
| CC4 | 진열대·공급처·Dock 및 다른 World 확장 | 같은 문법으로 확장하고 도메인별 권한·출처 회귀 없음 |

첫 완료는 prefab import가 아니다. 사용자가 `의향과 확정의 차이 → 현재 확정량 → 공급 부족 이유 → 허용된 공급 검토`를 카드의 근거와 한계까지 따라갈 수 있어야 한다.
