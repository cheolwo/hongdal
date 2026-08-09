# 지역 인구·수요 World Layer 제안

## 1. 문서 상태와 목적

이 문서는 Ssalddel의 공공 세계지도에 지역 인구, 실제 수요, 물류 접근성과 가상 물류센터 후보를 서로 구분해 표현하기 위한 설계 제안이다. 구현 완료 보고가 아니며, API 신청·실제 공급자 호출·운영 DB 집계·Unity runtime 검증을 수행했다는 뜻도 아니다.

목표는 다음 흐름을 좁은 vertical slice로 만드는 것이다.

```text
공식 인구통계             Ssalddel 운영 원장             교통·거리 관측
  잠재 수요의 기반          관측된 실제 수요                물류 접근성
          \                    |                    /
           \                   |                   /
            +------ 지역 단위 서버 Projection ------+
                               |
                     Unity Regional World Layer
                               |
                 조회·비교·비구속 Simulation
```

이 기능은 인구를 주문으로 예측하는 시스템이 아니다. 공공 통계와 실제 운영 관측을 같은 지도에서 비교할 수 있게 하고, 물류센터 후보를 검토할 때 어떤 근거를 사용했는지 추적 가능하게 만드는 것이 첫 목적이다.

Unity 내부의 공통 변환과 층별 revision은 [Unity Data·Interpretation·Presentation 기준 아키텍처](UnityDataInterpretationPresentationArchitecture.md)를 따른다. 이 문서의 public statistics와 authorized demand는 Data Snapshot이고, 지역 수요·후보지 비교는 Interpretation 결과이며, heatmap·marker·panel은 Presentation이다.

## 2. 프로젝트에 적용할 핵심 결정

1. 인구·세대수는 `잠재 수요 기반`이고 실제 주문이나 매출의 대체값이 아니다.
2. 실제 주문·공동구매·배송은 Ssalddel 원장에서 서버가 권한과 공개 범위를 적용해 지역 단위로 집계한다.
3. Unity는 외부 공공 API나 주문 DB를 직접 조회하지 않고 서버가 만든 Projection만 읽는다.
4. 기존 `PublicWorldMapSnapshot`은 위도·경도 기반 개별 관측 marker에 유지한다. 지역 polygon과 다중 지표는 별도 `RegionalDemandWorldSnapshot`으로 추가한다.
5. 행정구역 `RegionId`, 외부 행정코드, 경계 기준연도는 기존 [한국·미국 행정구역 기반 농수산물 지도 제안](KoreaUnitedStatesAdministrativeRegionMapProposal.md)의 공통 지역 원장을 재사용한다.
6. 작은 지역의 운영 수요는 서버에서 억제하거나 더 큰 지역·기간으로 합친다. Unity에 개인 주소, 이름, 연락처, 주문 ID를 전달하지 않는다.
7. 공공 공급자가 `N/A`, 마스킹 또는 노이즈가 적용된 값을 주면 `0`으로 바꾸지 않고 품질·억제 상태를 보존한다.
8. 가상 물류센터는 항상 `Simulation`이며 배치·점수 계산이 계약, 발주, 배차, 투자 또는 시설 건립을 만들지 않는다.
9. 초기 점수는 설명 가능한 비교용 heuristic이다. AI 추천이나 사업 타당성 판정으로 표현하지 않는다.

## 3. 현재 저장소와의 접점

### 3.1 재사용할 기반

현재 Unity Public Data 계층에는 다음 기반이 있다.

- `GET api/v1/community/world-map/observations`
- `PublicWorldMapApiModel → Mapper → Repository → UseCase`
- `Revision`과 `StableId` 기반 증분 reconcile
- `Idle / Loading / Success / InitialLoadError / Refreshing / RefreshError`
- 최초 실패 시 빈 World, refresh 실패 시 마지막 성공 Snapshot 유지
- source, evidence 기준시각, freshness, 위치 정밀도와 boundary notice 표현
- Public Data Hall의 simulated/operational VContainer 분기와 primitive View

이 로딩·오류·출처 원칙은 지역 Layer에서도 재사용한다. 다만 현재 모델은 좌표 하나를 가진 관측 marker 중심이므로 다음 내용을 한 observation에 넣지 않는다.

- 행정구역 polygon 또는 grid geometry
- 한 지역의 인구·세대·실제 주문·배송 등 서로 다른 성격의 지표
- 공개 통계의 마스킹과 운영 집계의 개인정보 억제
- 비교 기간과 후보지 Simulation 입력

### 3.2 새로 필요한 경계

```text
PublicWorldMapSnapshot
  point observation과 공개 근거 marker

RegionalDemandWorldSnapshot
  행정구역별 지표와 공개·권한·품질 상태

RegionalGeometryCatalog
  기준연도를 가진 단순화 경계 mesh와 대표 내부점

LogisticsSiteScenario
  특정 Snapshot revision을 입력으로 한 비구속 비교 결과
```

세 모델은 stable ID로 연결하되 수명과 갱신 주기가 다르므로 하나의 거대한 DTO로 합치지 않는다.

## 4. 데이터 의미를 먼저 분리한다

| 데이터 분류 | 예 | 의미 | 금지할 해석 |
| --- | --- | --- | --- |
| `PublicPotentialBasis` | 주민등록인구, 세대수, 인구밀도 | 지역 수요를 이해하기 위한 공공 기반 | 인구 수만큼 주문이 발생한다고 단정 |
| `ObservedOperationalDemand` | 주문 건수, 주문 상품 수량 | 허용된 기간·지역에서 실제 관측된 Ssalddel 활동 | 전체 지역 소비 또는 시장점유율로 확대 해석 |
| `ObservedCommunityIntent` | 비구속 공동구매 관심·참여 | 아직 주문이 아닌 참여 신호 | 확정 주문·매출로 합산 |
| `LogisticsAccessibility` | 거점 간 거리, 시간대별 교통량 | 운송 접근성의 일부 관측 | 개별 배송시간이나 운임을 자동 확정 |
| `SimulationDerived` | 후보지 점수, 예상 담당 수요 | 선택한 입력·가중치에 따른 비교 결과 | 사업성·투자·허가가 검증됐다고 표현 |

`지역수요Snapshot` 하나에 단일 `데이터종류`만 두면 혼합된 지표의 출처와 성격을 잃는다. 따라서 각 metric이 자신의 분류, 출처, 단위, 기간과 품질을 가져야 한다.

## 5. 공식 데이터 공급자 제안

| 공급자 | 우선 활용 | 프로젝트 경계 |
| --- | --- | --- |
| [행정안전부 주민등록 인구통계](https://jumin.mois.go.kr/index.jsp) | 행정동별 인구·세대수, 연령별 인구 | 주민등록 기준이며 외국인을 제외한다는 원천 경계를 화면과 근거에 표시한다. |
| [SGIS OpenAPI](https://sgis.kostat.go.kr/developer/html/openApi/api/intro.html) | 인구·가구·주택·사업체 공간통계와 행정구역 경계 | 인증키, 기준연도, 좌표계와 코드 체계를 기록한다. 소지역 비밀보호 결과를 실제 0으로 해석하지 않는다. |
| [SGIS 소지역 통계 이용 매뉴얼](https://sgis.kostat.go.kr/html/attachFiles/%EA%B0%9C%EC%A0%95%ED%8C%90%20SGIS%20%EC%86%8C%EC%A7%80%EC%97%AD%20%ED%86%B5%EA%B3%84%20%EC%9D%B4%EC%9A%A9%EB%A7%A4%EB%89%B4%EC%96%BC.pdf) | 격자·집계구 자료의 비밀보호 규칙 확인 | 속성값 `N/A`, 작은 격자값의 확률 대체와 노이즈를 `QualityCode`와 `SuppressionCode`로 보존한다. |
| [KOSIS OpenAPI](https://kosis.kr/openapi/index/index.jsp) | 장기 시계열, 통계표·메타데이터 | 표 ID, 항목, 단위, 기준기간과 갱신일을 보존하고 호출·셀 제한에 맞춰 서버에서 수집한다. |
| [한국도로공사 시간대별 교통량](https://www.data.go.kr/data/15076797/openapi.do) | 전국·고속도로 규모의 물류 접근성 보조 자료 | 도심 마지막 구간의 실제 이동시간을 대신하지 않는다. 물류 Layer 후속 단계에서 사용한다. |

[서울시 생활인구·생활이동 데이터](https://data.seoul.go.kr/together/notice/boardView.do?seq=721010a1522630fbf7a78d381a8326ee)는 2026-06-09 공지에서 일부 기존 소규모 통신데이터의 현행화 중지와 250m 격자 기반 개편을 안내했다. 그러므로 초기 필수 공급자로 고정하지 않고 `Disabled` 상태의 교체 가능한 provider 후보로 둔다.

외부 API key는 서버의 .NET User Secrets 또는 운영 비밀 저장소에서만 관리한다. Unity, tracked 설정, 문서, log와 capture에는 key를 넣지 않는다.

## 6. 권장 서버 구조

```text
MOIS / SGIS / KOSIS provider
       ↓
Public Data Client / Collector
       ↓
출처·기간·hash를 가진 공공 통계 observation
       ┐
       │          Canonical Order / Community Intent / Delivery
       │                         ↓
       │             Authorization + Regional Aggregation
       └─────────────────────────┤
                                 ↓
                    Regional Demand Projection UseCase
                                 ↓
               Public 또는 Authorized Projection API
                                 ↓
                      Unity Repository / UseCase
```

### 6.1 지역 원장 재사용

새 `RegionCode` 문자열 하나로 모든 공급자를 통합하지 않는다.

- 내부 식별: `AdministrativeRegion.RegionId`
- 공개 식별: 개인정보가 없는 `RegionStableId` 또는 `PublicRegionKey`
- 외부 코드: `RegionCodeAssignment`에 `KR-MOIS-HADM`, `KR-SGIS-HADM` 등 scheme과 유효기간 저장
- 경계: `RegionBoundary`에 공급자, 기준연도, geometry reference와 단순화 수준 저장
- 교차: `RegionCrosswalk`에 match 방법, 신뢰도와 검토 근거 저장

행정동, 법정동과 SGIS 코드를 prefix나 문자열 길이만으로 결합하지 않는다.

### 6.2 공개 API와 권한 API 분리

첫 계약 후보는 다음과 같다.

```text
GET /api/v1/community/world-map/regional-statistics
  ?countryCode=KR
  &regionLevel=Sigungu
  &period=2026-07
  &layers=population,household

GET /api/v1/logistics/world-map/regional-demand
  ?regionLevel=Sigungu
  &periodStart=2026-07-01
  &periodEnd=2026-07-31
  &layers=order,community-intent,delivery
```

- 첫 API는 공개 가능한 공공 통계만 반환한다.
- 둘째 API는 인증·역할·업무 범위를 확인한 집계만 반환한다.
- 권한 실패나 운영 API 실패를 공개 통계 또는 simulation fixture로 대체하지 않는다.
- 실제 route 이름은 구현 전 기존 Controller route와 authorization policy를 다시 대조해 확정한다.

한 API로 합치더라도 서버 내부와 contract에서 공개 지표와 운영 지표의 권한 판정을 분리해야 한다. 초기 slice는 잘못된 정보 노출을 막기 위해 route 자체를 분리하는 편을 권장한다.

## 7. Projection 계약 초안

기술 역할은 영어, 업무 의미는 한국어로 명명하는 저장소 규칙을 따른다.

```csharp
public sealed record 지역수요WorldSnapshotResponse
{
    public required string SnapshotStableId { get; init; }
    public required string Revision { get; init; }
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public required string CountryCode { get; init; }
    public required string RegionLevelCode { get; init; }
    public required string BoundaryVersion { get; init; }
    public required 기간Response Period { get; init; }
    public required 지역수요RegionResponse[] Regions { get; init; }
}

public sealed record 지역수요RegionResponse
{
    public required string RegionStableId { get; init; }
    public required string PublicRegionKey { get; init; }
    public required string DisplayName { get; init; }
    public required string GeometryStableId { get; init; }
    public required 지역수요MetricResponse[] Metrics { get; init; }
}

public sealed record 지역수요MetricResponse
{
    public required string MetricStableId { get; init; }
    public required string MetricKindCode { get; init; }
    public decimal? Value { get; init; }
    public required string UnitCode { get; init; }
    public required string DataClassCode { get; init; }
    public DateTimeOffset? EvidenceAsOfUtc { get; init; }
    public DateTimeOffset? PeriodStartUtc { get; init; }
    public DateTimeOffset? PeriodEndUtc { get; init; }
    public required string SourceKey { get; init; }
    public required string SourceName { get; init; }
    public required string SourceVersion { get; init; }
    public required string FreshnessCode { get; init; }
    public required string QualityCode { get; init; }
    public required string SuppressionCode { get; init; }
    public required string BoundaryNotice { get; init; }
}
```

`Value`가 `null`이고 `SuppressionCode`가 설정된 경우는 값이 없거나 보호된 상태다. mapper와 View는 이를 0으로 바꾸지 않는다.

### 7.1 첫 metric code

```text
RegisteredPopulation
RegisteredHouseholdCount
PopulationDensity

ObservedOrderCount
ObservedOrderedItemQuantity
ObservedCommunityIntentCount
CompletedDeliveryCount
```

공동구매 관심, 참여, 주문과 배송 완료는 서로 다른 상태이므로 합산 지표 하나로 축약하지 않는다.

## 8. 개인정보와 공개 범위

```text
Canonical Order / Delivery
  주소·이름·연락처·개별 주문
            ↓
서버 Authorization
            ↓
행정구역 + 기간 집계
            ↓
최소 집계 기준 / 지역 확대 / 기간 확대 / 억제
            ↓
Regional Demand Projection
```

반환 가능한 값은 다음으로 제한한다.

- 공개 지역 stable ID와 표시명
- 명시된 기간의 집계 건수·수량
- 데이터 기준시각, 단위, 출처, 공개 정밀도
- 억제 여부와 이유

반환하지 않는 값은 다음과 같다.

- 고객·생산자·기사 이름과 내부 사용자 ID
- 전화번호, 상세 주소와 정확한 배송 좌표
- 개별 주문·운송·결제·계약 ID
- 개인별 구매 품목 또는 행동 이력
- 소수 집계를 역산할 수 있는 분할 조합

운영 수요의 최소 집계 기준은 코드에 임의 숫자로 고정하지 않는다. 개인정보·업무 정책에서 승인한 값과 지역·기간 확대 규칙을 서버 policy로 정의한다. 억제된 값은 `0건`이 아니라 `보호로 인해 표시하지 않음`으로 표현한다.

## 9. Unity World 구조

기존 Public Data Hall과 World Map을 확장하며 새 Scene을 먼저 만들지 않는다.

```text
PublicWorldMap
├─ PopulationLayerView
│  ├─ RegisteredPopulation
│  ├─ HouseholdCount
│  └─ PopulationDensity
├─ DemandLayerView
│  ├─ ObservedOrder
│  ├─ CommunityIntent
│  └─ CompletedDelivery
├─ LogisticsLayerView
│  ├─ Warehouse
│  ├─ UrbanLogisticsCenter
│  ├─ TraditionalMarketHub
│  └─ AggregatedRoute
└─ SimulationLayerView
   └─ LogisticsSiteCandidate
```

권장 Presentation 구성은 다음과 같다.

```text
RegionalDemandLayerController : MonoBehaviour
├─ RegionalDemandLayerView
│  ├─ RegionalLayerToggleView
│  ├─ RegionCellView[]
│  ├─ RegionalDemandLegendView
│  └─ RegionalDemandDetailPanel
└─ LogisticsSiteSimulationController : MonoBehaviour
   └─ SimulationLogisticsCenterView
```

- `RegionalDemandLayerController`는 조회, loading/error 상태와 revision 적용을 조율한다.
- `RegionCellView`는 이미 판정된 구간, 품질과 선택 상태만 표현한다.
- `RegionalLayerToggleView`는 지표 선택을 요청할 뿐 권한을 만들지 않는다.
- `SimulationLogisticsCenterView`는 언제나 `SIMULATION` badge와 입력 revision을 표시한다.
- Controller는 Material, mesh vertex와 개별 label을 직접 조작하지 않고 View 계약을 호출한다.

### 9.1 geometry와 수치 Snapshot 분리

지역 경계는 매 refresh마다 수치와 함께 내려보내지 않는다.

```text
RegionalGeometryCatalog
  GeometryStableId
  RegionStableId
  BoundarySourceCode
  BoundaryVersion
  CoordinateReferenceCode
  SimplificationLevel
  Mesh 또는 Addressable reference

RegionalDemandWorldSnapshot
  RegionStableId
  GeometryStableId
  Metrics
```

Unity용 geometry는 공식 경계를 출처·기준연도와 함께 서버 또는 build pipeline에서 단순화한 파생 asset으로 만든다. raw 대용량 GeoJSON을 매 조회마다 Unity로 보내지 않는다. Snapshot이 알 수 없는 geometry를 참조하면 임의 위치에 표시하지 않고 `UnassignedGeometry` 목록과 Detail Panel에 격리한다.

### 9.2 시각화 규칙

- 인구, 실제 주문과 물류 접근성은 서로 다른 legend와 단위를 사용한다.
- 현재 viewport 또는 동일 기간 안에서 사용한 정규화 범위를 legend에 표시한다.
- 색상만으로 상태를 전달하지 않고 pattern, 높이, icon과 text를 함께 사용한다.
- `N/A`, 억제, stale과 수집 실패를 같은 회색으로 뭉개지 않는다.
- 지역 면적이 큰 이유만으로 중요도가 커 보이는 choropleth 왜곡을 Detail Panel의 원값·단위로 보완한다.
- 인구 Layer에서 `수요 높음`이라고 쓰지 않고 `주민등록인구 높음`처럼 관측 이름을 그대로 표시한다.

## 10. 물류센터 후보 Simulation

### 10.1 상태와 효과 경계

첫 slice는 Unity session 안의 비영속 후보만 만든다.

```text
DraftLocal
  → Compared
  → Discarded
```

후속 단계에서만 명시적 확인을 거쳐 다음 비구속 상태로 보낼 수 있다.

```text
Compared
  → 관심 등록
  → 공동체 논의
  → 별도 사업 검토
```

어느 단계도 운영 물류센터, 계약, 부지 확보, 발주, 배차 또는 결제를 자동 생성하지 않는다. 실제 효과가 필요한 기능은 `SsalddelExecution:Mode=Operational`과 별도 서버 UseCase·권한·확인을 통과한 뒤 canonical 상태를 재조회해야 한다.

### 10.2 첫 점수 모델

점수는 정규화한 지표의 투명한 가중합으로 시작할 수 있다.

```text
CandidateScore =
  ObservedDemandScore       × W1
  + PublicPotentialScore   × W2
  + DistanceScore          × W3
  + AccessibilityScore     × W4
  + FacilityConstraintScore× W5
```

단, `40/20/20/10/10` 같은 값은 연구 근거가 확정된 기본값이 아니라 비교 예시로만 취급한다. 각 결과에는 다음을 함께 표시한다.

- 사용한 Snapshot revision과 기간
- 포함·제외된 metric
- 가중치와 정규화 방법
- 누락·stale·억제된 지역 수
- 거리 계산 방식과 교통 자료 범위
- 결과가 비교용 Simulation이라는 경계

시설 비용, 처리량, 차량 수와 30분 배송권역은 실제 입력 자료와 계산 규칙이 생기기 전에는 숫자를 생성하지 않는다. 알 수 없는 값을 그럴듯한 기본값으로 채우지 않는다.

### 10.3 도심마트 Demand Scenario handoff

지역 인구 Layer는 공급계약 Engine에 주문을 직접 전달하지 않는다. 첫 연결은 다음처럼 명시적인 Simulation handoff를 사용한다.

```text
지역공공통계DataSnapshot
  → 지역잠재수요WorldState
  → 도심마트수요시나리오DataSnapshot
  → 도심마트주문SimulationDataSnapshot
```

`도심마트수요시나리오DataSnapshot`에는 상품 선택률, Simulation 점유율, 계절·요일·행사, 기간별 수요 범위, seed, `PopulationBasisRevision`과 `DemandRuleRevision`을 명시한다. 인구나 세대가 10% 변했다는 사실만으로 주문을 10% 바꾸지 않는다. 어떤 비례·탄력성 가정도 scenario rule에 드러나야 한다.

Simulation 주문은 synthetic 객체이며 실제 사람·주소·계정·운영 주문 ID를 포함하지 않는다. 반대로 운영 주문은 이 Layer의 Simulation 값을 사용하지 않고 운영 서버가 권한·집계·억제 규칙을 적용한 canonical Projection만 사용한다. 상세 주문 생성·할당·충족 기준은 [도심마트 지역 수요·주문 Simulation 설계](UrbanMarketDemandOrderSimulationDesign.md)를 따른다.

## 11. 권장 vertical slice와 우선순위

### RD0. 계약과 지역 기준 고정

- 첫 공간 단위를 `시·군·구`로 고정
- 공통 `RegionId`, MOIS·SGIS code assignment와 boundary version 확인
- public/authorized route와 metric code 확정
- 억제·품질·freshness code 확정
- DTO·validator·contract test 작성

시·군·구를 먼저 권장하는 이유는 전국 비교가 가능하고, 행정동보다 geometry·코드 변경과 운영 수요 노출 위험을 작게 시작할 수 있기 때문이다.

### RD1. 공공 인구 Layer

- 공급자 한 곳의 인구·세대수 두 지표부터 수집
- source, period, unit, hash와 collected time 보존
- 서버 public Projection API
- Unity ApiModel·Mapper·Repository·UseCase
- RegionCell primitive, legend, Detail Panel과 layer toggle

첫 공급자는 최신 월별 주민등록 기준을 명확히 설명할 수 있는 MOIS를 권장한다. SGIS는 경계와 인구밀도·사업체 등 공간통계의 두 번째 provider로 연결한다.

### RD2. 실제 수요 Aggregate

- canonical 주문·공동구매 의향·배송 상태를 각각 집계
- 역할·업무 범위와 최소 집계 policy 적용
- 주소를 RegionId로 변환하는 서버 내부 crosswalk
- authorized API와 no-fallback 오류 처리
- Unity에서 Public과 Operational badge·legend 분리

### RD3. 물류 World 연결

- 기존 Warehouse, Urban Logistics Center와 Traditional Market Hub stable ID 연결
- 실제 시설과 simulation 후보를 다른 object type으로 표현
- 지역 선택 시 관련 Zone·집계된 흐름 highlight
- 개인 운송 route가 아닌 지역 간 집계 flow만 표시

### RD4. 후보지 비교 Simulation

- 지도 후보 배치·이동·삭제
- 투명한 가중치와 deterministic score
- 입력 revision, coverage와 limitation 표시
- 저장·발주·배차 없는 local comparison

### RD5. 접근성과 시계열 확장

- 도로·교통 공급자 adapter
- 시간대·거리 기반 접근성
- KOSIS 장기 시계열
- 인구 감소·1인 가구·주문 증가 시나리오
- 시나리오 자료와 현재 관측을 시각적으로 분리

### 현재 Zone 심화 순서와의 관계

이 제안 작성은 현재 Warehouse 구현 우선순위를 대체하지 않는다. 권장 실행 순서는 다음과 같다.

```text
Warehouse W2
  cargo handoff와 입고 Dock의 canonical relation 결합
        ↓
RD0
  지역·contract·privacy 기준 고정
        ↓
RD1
  public population vertical slice
        ↓
RD2
  authorized actual demand aggregate
        ↓
RD3~RD4
  물류 Zone 연결과 후보지 Simulation
```

Warehouse W3의 운영 Command 폐루프는 별도 Warehouse 업무 축이다. RD0~RD2와 파일·계약 충돌을 확인해 한 slice씩 진행하며, 지역 Layer 때문에 운영 Command 구현 범위를 암묵적으로 넓히지 않는다.

## 12. 1차 MVP 전체 범위

첫 완료 기준은 다음으로 제한한다.

- 대한민국 시·군·구 한 단계
- 공공 지표: 주민등록인구, 세대수
- 운영 지표: 주문 건수, 주문 상품 수량
- 비교 기간 하나와 이전 기간 하나
- 단순화한 지역 geometry catalog
- Population / Demand 두 layer toggle
- 지역 선택 Detail Panel
- source, 기준시각, 단위, freshness, suppression과 boundary version 표시
- stable ID 증분 갱신과 마지막 성공 Snapshot 유지
- simulation 후보 1개를 놓고 두 후보를 비교하는 비영속 기능

첫 범위에서 제외한다.

- AI 입지 추천
- 개인 주소·주문 marker
- 실제 시설 비용·임대료·부지 규제
- 정확한 차량 수·창고 용량·처리량 산정
- 자동 배차·노선 최적화
- 계약·투자·발주·결제
- 행정동·집계구·100m/250m 격자의 공개 운영 수요
- 특정 서울 생활인구 API에 대한 필수 의존

## 13. 검증 기준

| 영역 | 필수 검증 |
| --- | --- |
| 계약 | stable ID·revision·period·unit·source·boundary version 누락 거부 |
| 지역 | 외부 code와 내부 RegionId 혼합 방지, 폐지·변경 이력, 알 수 없는 geometry 격리 |
| 품질 | `N/A`·masked·noise·stale·missing을 0과 구분 |
| 개인정보 | 이름·연락처·상세 주소·개별 ID가 Projection JSON에 없음 |
| 권한 | 공개 API에서 운영 수요가 나오지 않고 authorized 실패 시 fallback하지 않음 |
| 집계 | 기간 경계, 상태별 중복, 취소 주문과 비구속 의향의 별도 처리 |
| Unity mapping | 중복 RegionStableId, 알 수 없는 metric·geometry, 잘못된 단위 거부 |
| 갱신 | 최초 실패는 빈 Layer, refresh 실패는 마지막 성공 region object 유지 |
| 시각화 | legend·단위·기간·출처·억제 상태가 선택 panel에 표시 |
| Simulation | 동일 입력·revision은 동일 결과, 누락 입력은 limitation으로 노출 |
| 부작용 | 후보 배치·비교가 DB·주문·배차·계약에 write하지 않음 |

runtime proof는 다음을 서로 구분해 보고한다.

1. contract·mapper·집계 단위 test
2. 공급자 fixture 기반 수집 test
3. 승인된 key를 사용한 실제 공급자 호출
4. 운영 DB 권한 집계 test
5. Unity compile·Scene reload wiring
6. 실제 Game View의 layer toggle·선택·refresh

## 14. 예상 파일 배치

구현 시 실제 기존 namespace와 가까운 `AGENTS.md`를 다시 확인한 뒤 확정한다.

```text
Ssalddel.Contracts/Common/WorldProjection/
  지역수요WorldDtos.cs

Ssalddel/Application/PublicData/
  지역공공통계조회UseCase.cs

Ssalddel/Application/WorldProjection/
  지역운영수요ProjectionUseCase.cs

Ssalddel/Controllers/Common/
  지역공공통계WorldController.cs

Ssalddel/Controllers/.../Logistics/
  지역운영수요WorldController.cs

Ssalddel.Unity/Runtime/RegionalDemand/
  RegionalDemandWorldModels.cs
  RegionalDemandWorldData.cs
  RegionalDemandWorldReconciler.cs

Ssalddel.Unity/Samples~/PublicDataHall/
  Runtime/RegionalDemandLayerController.cs
  Runtime/RegionalDemandLayerView.cs
  Runtime/RegionCellView.cs
  Runtime/RegionalDemandDetailPanel.cs
```

공공 통계 수집과 권한이 필요한 운영 수요 집계는 서로 다른 Application 경계에 둔다. Unity에서는 하나의 World 경험으로 조합하되 서버의 공개 범위를 합쳐 버리지 않는다.

## 15. 구현 전 확정할 질문

1. 첫 지역 범위를 전국 시·군·구로 할지 서울·수도권 일부 시·군·구로 할지
2. 실제 수요 Layer를 허용할 최초 역할과 조직 범위
3. 주문 집계의 기준 상태, 취소·환불 처리와 기간 timezone
4. 운영 수요 최소 집계 기준과 지역·기간 확대 규칙
5. geometry catalog의 공급자·기준연도·단순화 수준
6. MOIS 자료의 수집 방식과 이용 조건, SGIS key 승인 상태
7. 후보지 점수에서 실제로 사용할 수 있는 거리·시설·교통 자료

이 질문이 확정되기 전에도 RD0 contract와 fixture는 만들 수 있다. 다만 미확정 값을 실제 운영 수치나 추천 결과처럼 표시해서는 안 된다.

## 16. 완료 정의

이 제안의 첫 vertical slice는 사용자가 Unity 지도에서 다음을 분명히 구별할 수 있을 때 완료다.

> 이 지역의 공식 주민등록인구와 세대수는 얼마인가?
> 같은 기간 Ssalddel에서 권한상 볼 수 있는 실제 주문은 몇 건인가?
> 값의 출처·기준시각·단위·공간 정밀도와 보호 여부는 무엇인가?
> 가상 물류센터 후보 점수는 어떤 입력과 가중치에서 나온 것인가?
> 이 비교가 현실의 계약·발주·배차를 만들지 않는 Simulation임이 명확한가?

이를 만족하면 공공데이터 정보관, 도심 물류센터, 창고와 운송 Zone을 지역 수요라는 공통 축으로 연결할 수 있다. 그다음에야 더 세밀한 격자, 교통, 미래 시나리오와 사업 검토 handoff를 확장한다.
