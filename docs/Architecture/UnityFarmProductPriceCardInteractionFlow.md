# Unity Farm 상품·가격 카드 상호작용 흐름

## 1. 목적과 상태

이 문서는 POLYGON Farm asset으로 배치한 작물·과수·수확물·상자를 사용자가 선택했을 때 상품 정보와 가격 정보를 공통 Concept Card로 확인하는 읽기 흐름을 정의한다.

```text
농장 Object를 본다
  → 선택한다
  → 어떤 상품인지 확인한다
  → 가격 연결 상태를 확인한다
  → 국내 가격과 근거를 카드로 읽는다
  → 조건이 맞으면 국가별 가격 카드로 확장한다
```

기준일은 2026-08-09이다. 이번 범위는 문서화뿐이며 Unity prefab·Scene·catalog, 서버 API와 client 코드를 변경하지 않는다.

현재 재사용 가능한 기반과 아직 없는 연결은 다음과 같다.

| 구분 | 현재 상태 |
| --- | --- |
| Farm 식품 asset·HS·가격 대응 조사 | 29개 품목군을 직접·대표가격·추가 판정 상태로 분류 완료 |
| World 선택 상태 | `SelectionStateStore`가 authorization scope와 `WorldStableId` 선택·해제를 제공 |
| 공통 카드 계약 | `ConceptCardDeckPresentationModel`과 Concept·Status·Reason·Action 카드 구현 |
| 공통 카드 View | Urban Market sample의 `ConceptCardDeckView`, `ConceptCardView`, asset-neutral skin 존재 |
| 국내 가격 서버 API | `GET /api/v1/agricultural-fisheries/items/{hsCode}/domestic-price` 존재 |
| 국가별 가격 서버 API | `GET /api/v1/agricultural-fisheries/items/{hsCode}/country-price-card` 존재 |
| Farm 상품 선택 wrapper | 후속 구현 필요 |
| Unity 가격 API model·repository·interpreter | 후속 구현 필요 |
| Farm 상품·가격 Concept Card projector와 Scene wiring | 후속 구현 필요 |

따라서 이 문서는 이미 구현됐다는 보고가 아니라, 기존 선택·카드·가격 API를 Farm World에 연결하기 위한 기준 설계다.

## 2. 핵심 원칙

### 2.1 보이는 asset은 조회의 입구다

Synty prefab 이름이나 mesh는 상품, HS, 가격의 권위가 아니다. 클릭은 vendor prefab 내부가 아니라 이를 감싼 Ssalddel World View wrapper에서 받는다.

```text
Synty VisualRoot
  ↑ 외형만 제공

Farm World Object View
  ├─ WorldObjectRef
  ├─ ProductStableId?
  ├─ CultivationCycleStableId?
  ├─ CargoStableId?
  ├─ VisualRoleCode
  └─ InteractionPolicyCode
```

- `ProductStableId`가 없는 환경 작물은 경관용이며 상품·가격 카드를 열지 않는다.
- 동일 상품은 밭 작물, 수확물, 상자와 Produce Stand에서 다른 외형으로 보일 수 있다.
- 재배체를 클릭해도 “현재 이 밭에서 생산 중인 상품”을 보여줄 뿐 수확량이나 재고를 asset 개수로 계산하지 않는다.
- 상자를 클릭할 때 실제 cargo stable ID가 있으면 상품 정보와 함께 해당 화물의 현재 위치·상태를 연결할 수 있다.

### 2.2 상품과 가격은 서버·Simulation snapshot에서 읽는다

```text
FarmVisualKey
  → WorldObjectRef 선택
  → ProductStableId 해석
  → authorized 상품 snapshot 조회
  → 검토된 HS mapping 조회
  → 가격 API 조회
  → Interpretation
  → Concept Card projection
```

Unity View는 prefab 이름에서 HS 코드를 만들거나 KAMIS 품목을 고르지 않는다. 가격을 다시 계산하지 않고 서버 응답의 품목, 범위, 단위, 조사일, 표본 수와 주의를 표시한다.

### 2.3 정보 조회와 업무 실행을 분리한다

첫 카드 흐름은 읽기 전용이다. asset 클릭이나 카드 열기는 계약, 발주, 결제, 수확, 출하 또는 Simulation Tick을 만들지 않는다. 후속 Action Card가 추가되더라도 `Preview → 명시적 확인 → Command → canonical 재조회` 경계를 따라야 한다.

## 3. 선택 가능한 Object 분류

| Object 유형 | 예 | 클릭 결과 | 가격 카드 |
| --- | --- | --- | --- |
| 실제 재배 Object | 실제 Simulation 감자 6×6 중 한 tile·작기 | 상품 카드와 재배 상태 카드 | 연결된 상품과 HS가 있을 때 가능 |
| 상품 수확물 | 감자·토마토·사과 낱개·묶음 | 상품 기본 카드 | 직접·대표 연결 상태에 따라 가능 |
| 출하 상자 | Potato Box, Apple Box | 상품 카드, cargo가 있으면 출하 상태 카드 | 상품 연결이 있을 때 가능 |
| Produce Stand 진열 | 판매 또는 출하 대기 상품 | 상품 카드와 판매·출하 맥락 | 상품 연결이 있을 때 가능 |
| 환경 작물 | 밀·옥수수·해바라기 등 풍경 밀도용 배치 | 선택 안 함 또는 짧은 환경 label | 가격 카드 없음 |
| 연결 검토 Object | Bean, Cabbage, Orange처럼 동일성이 불명확한 asset | 상품 외형 설명과 `연결 검토 필요` 카드 | 조회하지 않음 |

Scene authoring 단계에서 `환경 전용`, `상품 연결`, `Simulation 연결`, `Operational 연결`을 명시한다. 동일 prefab을 사용해도 Scene instance의 binding이 다르면 상호작용 결과가 달라질 수 있다.

## 4. 사용자 상호작용 흐름

### 4.1 기본 흐름

```text
[Idle]
  사용자가 작물·수확물·상자를 클릭
        ↓
[SelectedLoading]
  선택 highlight + 상품명 placeholder + loading
        ↓
  WorldObjectRef와 ProductStableId 확인
        ↓
  authorized 상품·HS·가격 snapshot 조회
        ↓
[Ready | Partial | MappingRequired | DataUnavailable | Stale]
  오른쪽 screen-space Card Deck 표시
        ↓
  상품 / 국내가격 / 가격근거 / 국가별가격 카드 탐색
```

첫 클릭은 Object 선택과 Deck 열기를 함께 수행한다. Deck 안의 카드를 다시 클릭하면 선택한 카드의 상세 evidence를 펼친다. 다른 Object를 클릭하면 이전 요청을 취소하고 새 선택으로 바꾸며, 빈 지면 클릭·닫기 버튼·`Esc`는 선택과 Deck을 닫는다.

PC와 Mobile 입력은 다음처럼 분리한다.

- PC: 짧은 좌클릭은 선택, drag threshold를 넘으면 camera pan으로 처리한다.
- Mobile: 짧은 tap은 선택, 이동한 touch는 pan·pinch gesture로 처리한다.
- vendor prefab의 여러 collider가 맞더라도 가장 가까운 Ssalddel wrapper 하나로 선택을 정규화한다.
- 새 선택이 먼저 끝났는데 이전 요청이 늦게 도착하는 race를 막기 위해 selection revision 또는 request token을 비교한다.

### 4.2 카드 배치

기본 Deck은 화면 오른쪽의 screen-space panel로 둔다. World-space 3D Text는 줌·가림·해상도에 따라 가격 단위와 출처가 읽히지 않을 수 있으므로 핵심 정보 표시로 사용하지 않는다.

선택 Object에는 다음만 남긴다.

- 얇은 outline 또는 바닥 ring
- 짧은 상품명 label
- Deck과 연결된 anchor indicator

Deck은 camera 이동 중에도 화면에 고정하되 선택 Object가 화면 밖으로 나가면 indicator만 화면 가장자리로 제한하거나 Deck 상단에 `선택 대상 화면 밖` 상태를 표시한다.

## 5. 기본 Card Deck

### 5.1 카드 구성

| 순서 | 카드 종류 | 카드 제목 예 | 목적 |
| --- | --- | --- | --- |
| 1 | Concept | `감자는 어떤 상품인가` | 상품명, 품목군, HS mapping 수준과 현재 시각 역할 설명 |
| 2 | Status | `국내 가격` | 도매·소매 평균/최저/최고, 단위, 최근 조사일과 표본 수 표시 |
| 3 | Reason | `이 가격이 연결된 이유` | HS→KAMIS 연결 품질, 포함 품종, 제외 원산지, 대표가격 여부와 한계 설명 |
| 4 | Status | `국가별 가격 자료` | 확정 HS6와 자료가 있을 때만 국가별 관측 상태 표시 |
| 5 | Action | `자세한 가격 정보 보기` | 권한과 route가 있을 때 기존 정보 화면으로 이동하는 읽기 행동 |

Action Card는 발주나 계약 행동이 아니다. 첫 단계에서는 `상세 정보 열기`, `출처 보기`, `기간 변경` 같은 읽기 intent만 제공한다.

### 5.2 상품 Concept Card

필수 표시 항목:

- 사용자 표시명과 canonical `ProductStableId`
- Object의 시각 역할: 재배 중, 수확물, 출하 상자, 직판대 진열
- HS code 또는 prefix와 scheme
- mapping 상태: `직접`, `대표가격`, `연결 검토 필요`, `연결 없음`
- `정보 제공용` 표시
- Simulation이면 scenario·mode label, Operational이면 authorized snapshot 기준임을 표시

개발용 VisualKey와 vendor prefab 파일명은 일반 사용자 카드의 기본 정보로 노출하지 않는다. 진단 모드에서만 별도 표시한다.

### 5.3 국내 가격 Status Card

`AgriculturalFisheriesDomesticPriceResponse`와 `AtDomesticFoodPriceLookupResult`에서 다음을 투영한다.

| 카드 영역 | 표시 값 |
| --- | --- |
| 주요 값 | 도매 또는 소매 평균 `KRW/kg` |
| 가격 범위 | 최저~최고 `KRW/kg` |
| 조사 구분 | 도매 조사·소매 조사 |
| 기준 시각 | `LatestSurveyDate`, 조회 시작일·종료일 |
| 표본 | `SampleCount` |
| 품목 | KAMIS item name과 품종 범위 |
| 출처 | 한국농수산식품유통공사(aT) 일별 도·소매 가격정보 |
| 주의 | origin 상태, 제외 품종·원산지, `InformationOnly` |

도매와 소매가 모두 있으면 두 행으로 분리한다. 평균만 크게 표시하고 최저·최고·표본 수를 숨기지 않는다. 도매와 소매의 차이를 마진이나 절감액으로 해석하지 않는다.

### 5.4 가격 연결 Reason Card

가격 숫자보다 연결 근거를 먼저 검증할 수 있게 한다.

```text
입력       ProductStableId: product:potato
입력       HS prefix: 0701
조정       국내 조사 품종·원산지 필터
결과       KAMIS 감자 도매·소매 대표 범위
한계       실제 거래가격·계약가격·생산자 수취가격이 아님
```

- 직접 연결: asset 품목명과 현재 crosswalk 품목이 일치함을 표시한다.
- 대표가격: 호박·스쿼시처럼 여러 품종을 묶은 대표가격임을 카드 상단 badge와 caution에 표시한다.
- 후보: HS 후보는 보여줄 수 있지만 가격을 조회하거나 숫자를 표시하지 않는다.
- 연결 없음: 상품명만 표시하고 `가격 연결 자료 없음`으로 끝낸다.

### 5.5 국가별 가격 Status Card

국가별 가격 API는 숫자 6자리 HS6가 확인됐을 때만 활성화한다. `0701` 감자처럼 현재 연결이 4자리 prefix뿐이면 국내 가격은 조회할 수 있어도 국가별 가격 카드 행동은 `HS6 확정 필요`로 차단한다.

카드에는 다음 경계를 유지한다.

- 국내 KAMIS: 국내시장 조사가격
- 관세 수입통계: 수입 통계단가
- 국가별 `Observed`, `NoData`, `Unavailable` 상태
- 통화, 단위, 기준월, 관측 수, 계산 근거
- `AllowsComparisonWithinGroup`이 참인 같은 비교군 내부에서만 비교

시장 단계·통화·중량 단위·품종·등급·원산지·관측 시점이 정렬되지 않으면 순위, 더 싸다/비싸다, 절감률을 만들지 않는다.

## 6. 연결 상태별 Deck 동작

| 연결 상태 | 상품 카드 | 국내 가격 카드 | 국가별 가격 카드 | 사용자 문구 |
| --- | --- | --- | --- | --- |
| 직접 | 표시 | 조회 가능 | 확정 HS6일 때 가능 | `검토된 품목 연결` |
| 대표가격 | 표시 | 대표 badge와 함께 조회 | 확정 HS6일 때 제한적으로 가능 | `여러 품종을 포함한 대표가격` |
| 후보 | 표시 | 숨김 또는 차단 | 차단 | `상품 상태·품종 확인 필요` |
| 연결 없음 | 표시 | 없음 | 없음 | `연결된 가격 자료 없음` |
| 환경 전용 | 기본적으로 선택 불가 | 없음 | 없음 | 카드 없음 |

`대표가격`과 `후보`를 같은 노란색 하나로 뭉치지 않는다. 대표가격은 검토된 가격 연결이 존재하고, 후보는 아직 숫자를 보여주면 안 되는 상태다.

## 7. Loading·오류·최신성 상태

| 상태 | 발생 조건 | 표시 원칙 |
| --- | --- | --- |
| `Idle` | 선택 없음 | Deck 숨김 |
| `SelectedLoading` | 선택 뒤 조회 중 | 상품명 placeholder와 loading, 이전 상품 숫자 숨김 |
| `Ready` | 상품·가격 조회 성공 | 정상 Deck 표시 |
| `Partial` | 국내 가격은 있으나 일부 시장·국가 자료 없음 | 있는 자료만 표시하고 `자료 없음`을 개별 행에 표시 |
| `MappingRequired` | 가격 crosswalk 없음 | 상품 카드는 유지하고 가격 숫자는 표시하지 않음 |
| `DataUnavailable` | 원천 또는 서버 조회 실패 | 마지막 성공 자료가 없으면 오류·재시도 표시 |
| `Stale` | refresh 실패, 이전 성공 snapshot 존재 | 이전 자료 유지, `최신 조회 실패`와 마지막 성공 시각 표시 |
| `Unauthorized` | 현재 Perspective에서 조회 불가 | 비공개 내용을 남기지 않고 Deck 제거 또는 제한 카드 표시 |
| `ObjectRemoved` | refresh 뒤 선택 stable ID가 사라짐 | 선택 해제와 Deck 닫기 |

`MappingRequired`를 임의의 sample 가격으로 대체하지 않는다. 운영 조회 실패를 Simulation fixture로 바꾸지 않으며, stale 값에는 최신 자료처럼 보이지 않는 badge가 필요하다.

## 8. 데이터 흐름과 책임

```text
Pointer/Tap
  ↓
Farm Product Interaction View
  ↓ WorldObjectRef
SelectionStateStore
  ↓ ProductStableId
Farm Product Query UseCase
  ├─ Product Repository
  ├─ HS Mapping Repository
  └─ Price Repository
       ├─ domestic-price
       └─ country-price-card (confirmed HS6 only)
  ↓ authorized data snapshot
Farm Product Price Interpreter
  ↓ direct / representative / candidate / unavailable
Farm Product Price Card Projector
  ↓ ConceptCardDeckPresentationModel
ConceptCardDeckView
```

### Data

- API response, source key, 조회 기준일·기간과 HTTP 상태를 보존한다.
- cache key에는 authorization scope, `ProductStableId`, HS, 기준일/기간을 포함한다.
- public-data API key나 secret을 Unity client·Scene·ScriptableObject에 넣지 않는다.

### Interpretation

- `FoodPriceCrosswalkCatalog`의 mapping 품질과 원산지 상태를 읽는다.
- 가격의 시장 단계·단위·통화·최신성·비교 가능성을 판정한다.
- ProductStableId와 asset 이름이 충돌하면 asset을 믿지 않고 DataAttention 상태로 보낸다.

### Presentation

- 카드 제목, 강조 값, evidence 행, caution과 활성화된 읽기 행동을 결정한다.
- View는 숫자 변환 외에 평균·절감액·순위·HS 후보를 계산하지 않는다.
- 카드와 Object selection은 presentation revision으로 reconcile한다.

## 9. 감자 예시

### 9.1 감자밭 클릭

```text
사용자 → 실제 감자밭 tile 클릭
WorldObjectRef → farm-plot/cultivation-cycle/product 관계 확인
ProductStableId → product:potato
HS mapping → 0701, 직접 연결
국내 가격 → 감자 도매·소매 조사 범위
Deck → 상품 / 국내 가격 / 연결 근거 / 상세 정보 행동
```

카드 예시 문구:

```text
[상품] 감자
현재 표현: 재배 중
HS prefix: 0701 · 검토된 품목 연결

[국내 가격]
도매 평균  ○○원/kg   범위 ○○~○○원/kg
소매 평균  ○○원/kg   범위 ○○~○○원/kg
최근 조사일 · 표본 수 · aT 출처

[가격 근거]
국내 유통가격 대표 범위입니다.
이 농장의 판매가·생산자 수취가격·계약가격은 아닙니다.

[국가별 가격]
HS6가 확정되지 않아 조회할 수 없습니다.
```

여기서 `○○`는 runtime API 응답 자리이며 문서나 prefab에 고정값을 넣지 않는다.

### 9.2 감자 상자 클릭

같은 `product:potato` 상품·가격 Deck을 열되, 실제 cargo가 연결돼 있으면 별도의 Status Card를 추가한다.

- 현재 위치: Farm Yard
- 상태: 출하 대기
- cargo stable ID와 snapshot revision
- 상품 수량과 단위

상자 mesh의 개수나 scale로 수량을 추정하지 않는다. cargo 카드와 공공 가격 카드는 source lineage를 분리한다.

## 10. 개인정보·권한·운영 경계

- 공개 상품·가격 카드에 생산자 개인 이름, 연락처, 상세주소, 계약 단가와 비공개 재고를 섞지 않는다.
- 농장 소유자·관리자에게만 허용된 실제 재배·수확·계약 정보는 authorized Perspective가 제공할 때만 추가한다.
- 공공 가격은 정보 제공용이며 실제 매입가·판매가·계약가·정산가가 아니다.
- Simulation 상품과 Operational 상품이 같은 Synty prefab을 사용해도 mode badge와 stable ID namespace를 분리한다.
- 카드 열기, camera focus, NPC 이동과 animation은 운영 상태 변경 근거가 아니다.

## 11. 후속 구현 단위와 완료 기준

### FPC0 — 선택·binding 계약

- 환경 전용과 상품 연결 Object를 구분한다.
- wrapper가 `WorldObjectRef`와 `ProductStableId`를 제공한다.
- vendor prefab을 직접 수정하지 않는다.
- 새 선택·선택 해제·Object 제거와 authorization scope 변경을 검증한다.

### FPC1 — 가격 Data·Interpretation

- Unity용 API model·mapper·repository를 추가한다.
- 국내 가격의 `Success`, `MappingRequired`, `DataUnavailable`과 stale last-success를 구분한다.
- 국가별 가격은 확정 HS6가 있을 때만 조회한다.
- direct·representative·candidate mapping 회귀를 검증한다.

### FPC2 — Card Projector

- 상품·국내 가격·연결 근거·국가별 가격을 공통 Concept Card 계약으로 투영한다.
- source lineage, 기준 시각, 단위, 통화, 시장 단계와 `InformationOnly`를 보존한다.
- 값 없음, 일부 자료와 비교 불가를 정상 상태로 표현한다.

### FPC3 — Farm Scene 연결

- 감자 재배체·수확물·상자·Produce Stand 네 anchor에 연결한다.
- PC click과 Mobile tap/drag 충돌을 검증한다.
- Object highlight, screen-space Deck, 카드 상세 선택과 닫기를 Game View에서 확인한다.
- 대표 Game View PNG와 변경 기록을 코드·Scene과 같은 변경 맥락에 포함한다.

완료 기준은 “감자 asset을 클릭하면 가격 숫자가 뜬다”가 아니다. 사용자가 `어떤 상품인지 → 어떤 HS 연결을 사용했는지 → 어느 시장 단계의 무슨 단위 가격인지 → 기준 시각과 출처가 무엇인지 → 무엇과 비교하면 안 되는지`를 한 Deck에서 확인할 수 있어야 한다.

## 12. 관련 기준 문서

- [Unity Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)
- [Unity POLYGON Farm 식품 Asset·HS·가격 연결 조사](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)
- [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)
- [Unity Data·World Interpretation·Perspective·Presentation 기준 아키텍처](UnityDataInterpretationPresentationArchitecture.md)
- [Unity 서버 상태와 3D World Projection 설계](UnityServerStateToWorldProjectionDesign.md)
- [농수산 정보 통합 모듈](AgriculturalFisheriesInformationModule.md)
