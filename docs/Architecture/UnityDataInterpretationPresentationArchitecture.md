# Unity Data·World Interpretation·Perspective·Presentation 기준 아키텍처

## 1. 문서 상태와 목적

이 문서는 Ssalddel Unity 클라이언트가 인구, 주문, 센서, 재고, 운송, 가격과 공공데이터를 공통 방식으로 처리하기 위한 기본 읽기 흐름을 정의한다.

기존 [Unity 클라이언트 계층 구조 설계](UnityClientLayeredArchitecture.md)의 API Client, Mapper, Repository, UseCase, Presenter, SceneController와 View를 폐기하지 않는다. 각 구성요소가 다루는 책임을 다음 세 변환 단계에 명시적으로 배치해 Zone마다 같은 판단이 반복되는 것을 막는다.

```text
Ssalddel Server
       ↓ authorized API / stream

Data Layer
  실제로 어떤 값과 근거가 들어왔는가
       ↓
Interpretation Layer
  그 값과 관계가 World에서 무엇을 의미하는가
       ↓
Presentation Layer
  허용된 의미를 현재 관점에서 어떻게 보여줄 것인가
```

이 문서는 2026-08-08 현재 P0~P7 코드의 Architecture Migration Map도 함께 제공한다. 전체 파일 이동이나 public type rename은 수행하지 않으며, Warehouse W1부터 호환 facade를 둔 점진 migration을 적용한다.

현재 적용 상태:

| 단계 | 상태 | 구현 범위 |
| --- | --- | --- |
| DIP1 | 구현·headless 검증 | `DataRevisionSet`, quality code, interpretation lineage, presentation revision과 deterministic calculator |
| DIP2 | 구현·Unity Editor 검증 | Warehouse `DataMapper/DataRepository → WorldInterpreter → Presenter → View`, 기존 Mapper·Repository constructor facade 유지 |
| Warehouse W2 | 구현·Unity Editor 검증 | authorized Warehouse data에 inbound handoff를 추가하고 `inbound-task` 관계로 차량·화물·NPC·Dock 점유를 결합 |
| DIP3 | 구현·headless 검증 | engine-independent `StableIdReconciler<T>`와 change set·policy를 추가하고 WorldProjection·Warehouse·PublicData·Community facade에 적용 |
| DIP4 | 구현·headless 검증 | P0·P4 authorized role query와 Presentation Perspective 분리, NPC route와 transport corridor Interpreter lineage·Presenter·Presentation target 추가 |
| DIP5 | 구현·headless 검증 | P1 PublicData와 P2 Community의 Data Snapshot·World Interpreter·Presentation Model을 분리하고 sample Controller/View 입력 전환 |
| DIP5R | 진행 중·headless 검증 | identity·typed graph/index, Shared/Perspective Interpretation Runtime과 Selection store 구현 완료; surface별 pilot은 후속 |
| DIP6 이후 | 진행 중·headless 검증 | 도심마트 UM0~UM5 Data·typed graph·전역 할당·다중 원천 SourcePlan·평면 관리자 Perspective와 독립 surface 완료; manager Scene·Game View와 공동수령·농장 migration은 미검증·미구현 |

## 2. 최상위 원칙

1. 서버는 권한, 공개 범위, canonical 운영 상태와 업무 판정의 최종 권위다.
2. Data Layer는 서버가 허용한 사실, source, 단위, 시각, 정밀도와 revision을 보존한다.
3. Interpretation Layer는 여러 Data Snapshot을 연결해 World 의미, 관계, 공간 의미와 simulation 파생값을 만든다.
4. Interpretation은 서버의 권한·운영 판정을 뒤집거나 raw 값으로 새로운 운영 사실을 만들지 않는다.
5. Presentation Layer는 이미 허용되고 해석된 상태를 역할·장면·장치에 맞게 표현한다.
6. 권한 관점인 `Authorized Perspective`와 표현 관점인 `Presentation Perspective`를 분리한다.
7. source에서 presentation까지 identity lineage를 관통시키되 `SourceStableId`, `WorldStableId`, `PresentationStableId`의 역할을 구분한다.
8. 사용자 Command는 읽기 흐름을 역으로 수정하지 않고 별도 Application 경계를 통해 서버에 제출한다.
9. Command 성공 뒤 canonical data를 다시 조회해 세 층을 순서대로 재계산한다.
10. `Simulation` 결과는 입력과 rule revision을 보존하고 `Operational` 상태와 합치지 않는다.

## 3. 세 계층의 책임

### 3.1 Data Layer

Data Layer의 질문은 다음 하나다.

> 서버가 이 사용자에게 허용한 데이터 중 실제로 무엇이 들어왔는가?

포함한다.

- API route와 query contract
- Unity 전용 ApiModel과 JSON transport
- 명시적 Mapper와 schema·code·unit 검증
- Repository port와 UnityWebRequest adapter
- canonical 또는 public observation을 나타내는 immutable Data Snapshot
- source, evidence as-of, collected-at, precision, freshness와 limitation
- 서버가 승인한 role, viewer scope와 allowed interaction

포함하지 않는다.

- `수요가 높음`, `혼잡`, `위험`, `검토 가치 있음` 같은 client 파생 판단
- Material, color, icon, prefab, Animator parameter
- Unity `Vector3`와 Scene Transform
- 권한이 없어 서버가 보내지 않은 데이터의 추론
- API 오류를 fixture 성공으로 바꾸는 fallback

예시는 다음과 같다.

```csharp
public sealed record 지역인구DataSnapshot
{
    public required string RegionStableId { get; init; }
    public int RegisteredPopulation { get; init; }
    public int RegisteredHouseholdCount { get; init; }
    public required string SourceKey { get; init; }
    public DateTimeOffset EvidenceAsOfUtc { get; init; }
    public required string SpatialPrecisionCode { get; init; }
    public required string DataRevision { get; init; }
}
```

`DataSnapshot`은 서버 Entity 복사본이 아니다. Unity가 해석에 필요한 최소 허용 사실을 담는다.

#### 3.1.1 Data Context와 Data Runtime

Data Layer는 Data 종류뿐 아니라 조회 실행 문맥을 소유한다.

```text
Login / server session
  → UserSessionContext
  → WorldContext
  → DataAuthorizationContext
  → WorldDataContext
  → WorldDataQueryContext
  → Authorized Data Query
  → Data Snapshot
```

`UserSessionContext`의 identity handle은 서버가 발급한 불투명 값이다. Unity가 임의의 UserId나 Role을 넣어 권한을 확대하는 근거로 사용하지 않는다. `DataAuthorizationContext`의 role·capability·authorization revision도 서버가 승인해 내려준 범위만 보존한다. 실제 API는 token과 route에서 서버 authorization을 다시 검증해야 한다.

`WorldDataContext`는 다음 경계를 묶는다.

- 불투명 session scope
- 현재 `WorldContextId`와 World revision
- 서버 승인 authorization scope·role·capability revision
- `Operational` 또는 `Simulation` Data mode

동일한 `warehouse:42`라도 `world-a`와 `world-b`에서는 다른 참조다. Stable ID 문자열에 World ID를 억지로 결합하지 않고 `WorldObjectRef(WorldContextId, WorldStableId)`로 scope와 identity를 합성한다.

#### 3.1.2 Data scope와 cache

Data query는 다음 네 scope 중 하나를 명시한다.

| Scope | 예시 | World 전환 | logout |
| --- | --- | --- | --- |
| `Global` | 전국 인구, 공공 가격 | 유지 | 유지 |
| `World` | World 건물, simulation state | 폐기 | 폐기 |
| `AuthorizedUser` | 내 profile, 사용자 공통 설정 | 유지 | 폐기 |
| `AuthorizedUserWorld` | 이 World의 내 주문·농장·창고 업무 | 폐기 | 폐기 |

cache lookup key는 `Scope + Mode + Dataset + 해당 scope의 Session/World/Authorization`으로 만든다. Snapshot revision은 key에 넣지 않고 cache entry에 보존한다. 그래야 같은 scoped dataset의 새 revision이 이전 entry를 교체할 수 있다. 특정 과거 revision 자체가 query 대상이면 revision을 dataset/query variant에 명시한다.

`WorldDataContextRuntime`은 서버가 승인한 context의 수명만 관리하고 권한을 새로 만들지 않는다. Session·World·authorization·mode 전환을 구분해 `ContextScopedSnapshotCache`, `SelectionStateStore`와 이후 private WorldState store에 폐기 신호를 보낸다.

World 전환은 `ReinterpretPerspective()`가 아니다.

```text
Zone Runtime stop
  → World-scoped cache / selection / private state clear
  → server-authorized WorldDataContext activate
  → contextual RefreshDataAsync
  → Data → Shared World → Perspective → Presentation
```

VContainer 수명은 장기적으로 `ApplicationLifetime → SessionScope → WorldScope → ZoneScope`로 구성한다. 현재 core는 이 수명 구조가 소비할 context·transition·cache 계약을 제공하며 실제 dynamic child scope 생성은 composition 단계에서 별도 적용한다.

### 3.2 Interpretation Layer

Interpretation Layer의 질문은 다음이다.

> 하나 이상의 Data Snapshot이 World에서 어떤 의미와 관계를 갖는가?

Interpretation은 역할별로 전체를 복제하지 않고 두 단계로 구성한다.

```text
Authorized Data Snapshot
  → Shared World Interpretation
  → SharedWorldState
  → Perspective Interpretation + PerspectiveContext
  → PerspectiveWorldState
  → Presentation Projector
```

포함한다.

- 상태 분류와 derived metric
- stable-ID 기반 관계 graph
- semantic location, waypoint와 route 의미
- source 간 시간·지역·단위 정렬
- freshness·quality·suppression의 의미
- 역할별 업무 의미의 조합
- World object 종류와 상태
- deterministic simulation과 후보 비교
- rule, evidence와 input lineage

포함하지 않는다.

- 서버 권한 필터 대체
- 실제 주문·배차·입출고·센서 판정의 임의 변경
- GameObject 생성·삭제
- localized label, color, animation과 화면 배치
- 누락된 값을 0 또는 정상 상태로 보정

서버가 이미 판정한 값은 Interpretation의 입력 사실로 취급한다. 예를 들어 센서의 `ConditionCode=Dry`가 서버 rule revision과 함께 왔다면 Unity가 토양수분 raw value를 다시 임계값 판정하지 않는다. Interpretation은 `Dry`가 현재 Farm World에서 `관수 검토 상태`와 어떤 작물·작업 관계를 갖는지 연결할 수 있다.

```csharp
public sealed record 지역수요WorldState
{
    public required string RegionStableId { get; init; }
    public required string DemandLevelCode { get; init; }
    public required string DeliveryBurdenCode { get; init; }
    public decimal? OrderDensity { get; init; }
    public required string ReviewCandidateCode { get; init; }
    public required InterpretationLineage Lineage { get; init; }
}
```

#### Shared World Interpretation

Shared 단계의 질문은 다음이다.

> 현재 허용된 현실에서 어떤 상태·관계·경로·제약과 가능성이 존재하는가?

역할에 상관없이 재사용할 다음 결과를 만든다.

- state classification과 derived metric
- typed World graph와 spatial relation
- route graph와 constraint result
- candidate set과 ranking input
- 가능하지만 아직 확정되지 않은 possibility graph
- source/rule/catalog/evaluation lineage

Candidate와 ranking은 서버 배차·주문·계약의 확정이 아니다. authorized Data에 근거한 client 해석 또는 명시적 Simulation 결과이며 Command 실행 권한을 만들지 않는다.

#### Perspective Interpretation

Perspective 단계의 질문은 다음이다.

> 이 Shared World가 현재 역할과 목적, Zone과 주대상에게 어떤 의미인가?

Perspective는 `Role + Intent + Current Context`다. 공통 입력 계약은 다음 의미를 가진다.

```csharp
public sealed record InterpretationPerspectiveContext
{
    public required string RoleCode { get; init; }
    public required string IntentCode { get; init; }
    public required string ZoneCode { get; init; }
    public WorldStableId? FocusWorldId { get; init; }
    public required WorldInterpretationMode Mode { get; init; }
}
```

예를 들어 같은 배송 가능성 graph를 주문자에게는 배송·공동수령 선택지로, 기사에게는 권역·적재 제약을 만족하는 배송 후보로, 창고 관리자에게는 picking·packing·outbound 작업과 병목으로 축약한다. 내부 기사 후보나 배차 세부정보가 authorized Data에 없으면 관점 전환으로 생성하거나 추론하지 않는다.

`Interpretation Perspective`는 의미를 축약하고 다음 행동 후보를 구성한다. `Presentation Perspective`는 그 의미를 지도 강조, Heatmap, chart, route, NPC와 panel로 표현한다. 두 관점을 같은 Presenter 조건문으로 합치지 않는다.

#### 비교·파생 해석 규칙

Interpreter는 값을 계산하기 전에 비교 가능성을 먼저 검증한다.

1. 품목·등급·규격과 지역 범위가 같은가
2. 통화와 중량·수량 단위를 정규화할 수 있는가
3. 관측일·집계기간·timezone이 허용 오차 안에 있는가
4. source의 가격 정의가 생산자 수취·산지·도매·소매 중 무엇인지 명확한가
5. suppressed·missing·estimated 값을 실제 관측값처럼 사용하지 않는가

비교할 수 없으면 차이를 0으로 만들지 않고 `NotComparable`, `Incomplete`, `SuppressedInput` limitation을 가진 WorldState를 반환한다. 비교할 수 있을 때만 원본 단계 가격과 파생된 단계간 가격차를 분리해 보존한다.

```text
Price Data Snapshot
  생산자 수취가격 1,200 KRW/kg
  도매가격        1,850 KRW/kg
  소매가격        3,100 KRW/kg
       ↓ DistributionPriceInterpreter
유통가격WorldState
  생산자→도매 단계간 가격차   +650
  도매→소매 단계간 가격차   +1,250
  전체 단계간 가격차        +1,900
  비교 가능성, source/as-of/unit, rule lineage
```

`단계간 가격차`는 운송비, 보관비, 선별·포장비, 폐기 위험, 수수료와 인건비가 분해되지 않은 관측 차이다. 비용·수익 자료가 없는 상태에서 이를 `유통마진`, `이익` 또는 특정 참여자의 수취액으로 이름 붙이지 않는다. 실제 비용 source가 추가되면 원본 가격과 별도의 cost component WorldState로 해석한다.

해석된 가격 WorldState는 그래프 종류, 색, 축 label을 포함하지 않는다. 동일 WorldState를 막대그래프, 시계열 그래프, 가격표, 유통경로 World object와 DetailPanel이 재사용한다.

### 3.3 Presentation Layer

Presentation Layer의 질문은 다음이다.

> 해석된 World 상태를 현재 사용자의 표현 관점과 장치에서 어떻게 보여줄 것인가?

포함한다.

- Presentation Perspective
- Presenter와 PresentationModel
- runtime status를 안전한 UI 모델로 바꾸는 Status Projector
- stable-ID create/update/remove reconcile
- View, GameObject, prefab socket와 Inspector wiring
- map, building, NPC, heatmap, marker와 Detail Panel
- NavMeshAgent, Animator, Renderer, VFX, SFX
- localization, accessibility label와 legend

포함하지 않는다.

- 인증·인가 결정
- raw API code 해석
- canonical 운영 상태 변경
- 여러 Data Snapshot의 업무 join
- simulation 결과를 운영 결과로 승격

같은 `배송작업WorldState`를 주문자에게는 `배송 중`, 운송자에게는 `다음 하차 작업`, 창고 관리자에게는 `출고 Dock 예정`으로 표현할 수 있다. 세 표현은 같은 stable ID와 Interpretation revision을 참조한다.

Presentation은 내부적으로 다시 세 단계로 나눈다.

```text
WorldState + PresentationContext
  → Presentation Projector
  → surface별 PresentationSnapshot
  → surface별 Reconciler
  → StableIdChangeSet
  → Unity Applicator
```

- `Projector`는 순수 C#이며 marker/heatmap/label/detail/highlight와 semantic socket·motion code를 결정한다.
- `Reconciler`는 이전·다음 Presentation 항목만 비교하며 Data나 World 의미를 다시 해석하지 않는다.
- `Applicator`만 `GameObject`, `Transform`, `Animator`, `NavMeshAgent`와 실제 UI를 조작한다.
- Marker, Heatmap, Legend, Detail처럼 수명과 변경 빈도가 다른 surface는 별도 Snapshot과 change set을 가진다.

Presentation surface의 기본 분류는 다음과 같다.

```text
World Surface        Building / NPC / Vehicle / Cargo
Map Surface          Marker / Heatmap / Route
Chart Surface        PriceStage / PriceHistory / Population / Demand
Information Surface  DetailPanel / Legend / EvidenceCard
```

Chart Projector는 WorldState의 값과 lineage를 축·point·label·단위·출처·기준시각 모델로 투영한다. 가격차나 수요 점수를 Chart View가 직접 계산하지 않는다.

### 3.4 현재 구현에서 확인된 보완 대상

| 현재 구현 | 부족한 점 | 목표 조치 |
| --- | --- | --- |
| `PublicWorldMapLayerData.Color`, `MarkerShape` | Data Snapshot에 visual policy가 남음 | legacy wire field로만 보존하고 새 `PublicWorldState`에는 의미 layer만, 색·shape는 PublicMap Projector에서 결정 |
| `PublicWorldMapSnapshot` | 이름과 payload가 Data/World/Presentation 중간 형태 | 호환 facade로 유지하고 새 출력은 `PublicWorldState`로 고정 |
| `CommunityMarketSquareSnapshot.Items` | Board/Post/Activity/Ledger가 문자열 `Kind` item으로 평탄화됨 | typed World node와 `Contains`, `HasActivity`, `ContributesTo` relation graph 도입 |
| `WarehouseWorldObject.SourceStableId` | 재고→작업→NPC 단일 체인만 표현 | typed relation graph로 교체하고 기존 필드는 facade 입력으로만 사용 |
| feature별 `StableId` 문자열 | source/world/presentation identity가 우연히 동일하다고 가정 | 세 identity와 명시적 lineage 도입 |
| `*LoadCoordinator` | 조회 lifecycle, last-success와 Presentation 결과가 한 객체에 결합 | Application Runtime status/store와 surface Presentation channel 분리 |
| 단일 snapshot reconcile | legend 하나의 변경도 전체 surface 갱신 가능 | surface별 item revision과 reconciler 적용 |
| 단일 package assembly | namespace 규칙만으로 Unity 의존 금지를 보장 | 계층별 asmdef와 architecture test를 점진 도입 |

## 4. 세 계층 옆의 Application 축

세 계층은 모든 클래스를 강제로 셋 중 하나의 폴더에 넣는 규칙이 아니라 **읽기 데이터의 변환 단계**다. 다음 요소는 세 단계의 옆에서 흐름을 조율한다.

### 4.1 Query Application

```text
SceneController
  → Query UseCase
  → Data Repository
  → Interpreter
  → Presenter
  → View
```

Application Runtime과 Query UseCase는 다음을 조율한다.

- 필요한 Repository 호출
- 여러 Data Snapshot의 수집과 cancellation
- Interpreter 실행 조건
- 부분 자료·stale·no-access 정책
- 결과를 Presenter에 전달할 단일 World State 구성
- `Idle`, `InitialLoading`, `Ready`, `Refreshing`, `InitialError`, `RefreshError`와 last-success 보존

UseCase가 Data 의미를 직접 판정하거나 View state code를 만들기 시작하면 각각 Interpreter와 Presenter로 분리한다.

Runtime은 세 갱신 경로를 구분한다.

```text
RefreshDataAsync       Data 재조회 → Shared → Perspective → Project → Reconcile
ReinterpretShared      기존 Data → Shared → Perspective → Project → Reconcile
ReinterpretPerspective 기존 SharedWorld → Perspective → Project → Reconcile
Reproject              기존 PerspectiveWorld → Project → Reconcile
```

표현 theme·locale만 바뀌면 `Reproject`한다. role·intent·Zone·focus만 바뀌고 authorization scope가 같으면 `ReinterpretPerspective`를 사용한다. 공통 rule/catalog가 바뀌면 `ReinterpretShared`를 사용한다. 역할 변경으로 authorization scope가 달라지면 기존 cache를 재사용하지 않고 반드시 `RefreshDataAsync`로 새 authorized Data를 조회한다.

Runtime 결과도 두 채널로 분리한다.

```text
World Presentation Channel  object/NPC/marker/heatmap/detail snapshot
Runtime Status Channel      loading/refresh/error/last-success 상태
```

Data validation부터 Presentation validation과 change set 계산까지 모두 성공한 뒤에만 성공 snapshot을 교체한다. 중간 실패에서는 현재 Unity 표현을 건드리지 않는다.

### 4.2 Command Application

```text
Presentation interaction
  → preview
  → explicit confirmation
  → Command UseCase
  → server authorization + expected revision
  → canonical persistence/event
  → canonical re-query
  → Data → Interpretation → Presentation
```

Command 결과로 Data Snapshot, World State나 ViewModel을 client에서 직접 수정하지 않는다. optimistic feedback이 필요하면 `PendingConfirmation` 같은 Presentation 상태로만 두고 서버 재조회 결과와 구분한다.

### 4.3 Composition

VContainer `LifetimeScope`는 다음 구현을 조립한다.

- Data: API Client, Repository, cache
- Interpretation: Interpreter, resolver, rule catalog
- Application: Query/Command UseCase
- Presentation: Presenter, SceneController, View

Simulation·Operational 선택은 Composition에서 이루어지며 Controller가 concrete 구현을 `new`하지 않는다.

## 5. Authorized Perspective와 Presentation Perspective

### 5.1 Authorized Perspective

서버가 결정한다.

```text
authenticated session
  + assigned role
  + organization/ownership/task relation
  + current disclosure policy
      ↓
authorized projection
```

예:

- 주문자: 자기 공동수령 상품
- 운송자: 자신에게 배정된 하차 대상
- 생산자: 자신이 소유한 농장
- 창고 관리자: 권한이 있는 창고의 재고·작업
- 공공 관찰자: 공개 source와 공개 정밀도만

Unity가 역할 code를 요청하는 것은 권한 증명이 아니다. 보안상 숨겨야 할 값은 서버 응답과 Unity process memory에 들어오지 않아야 한다.

### 5.2 Presentation Perspective

Unity가 이미 허용받은 World State의 강조와 표현을 선택한다.

```text
PresentationPerspective
  PerspectiveCode
  EmphasisRuleRevision
  VisiblePanelCodes
  VisualPriorityRules
  AccessibilityProfile
```

Presentation Perspective는 허용된 정보를 덜 보여줄 수는 있지만, 숨김을 보안 경계로 사용하지 않는다. `RoleExperienceCoordinator`라는 현재 이름은 두 관점을 혼동할 수 있으므로 migration 시 다음 두 책임으로 분리한다.

```text
AuthorizedRoleProjectionQuery
  서버가 승인한 role projection을 Data Snapshot으로 조회

RolePresentationPerspectiveCoordinator
  승인된 World State에 표현 강조와 panel policy 적용
```

## 6. Stable ID와 층별 revision

### 6.1 Stable ID

동일 문자열을 세 층에서 무조건 재사용하지 않는다. 각 identity의 목적을 분리하고 lineage로 연결한다.

```text
SourceStableId       서버 원본·공공 관측 항목
  region:myeonmok-2:population
        ↓ interpreted from
WorldStableId        표현과 독립적인 World 실체
  region:myeonmok-2
        ↓ projected as
PresentationStableId Zone·surface별 표현 실체
  public-map:heatmap:region:myeonmok-2
  logistics-panel:region:myeonmok-2
```

원본과 World 실체가 실제로 1:1이면 같은 문자열 값을 사용할 수 있지만 타입과 의미까지 같다고 간주하지 않는다. Presentation 항목은 자신의 `PresentationStableId`와 하나 이상의 `SourceWorldIds`를 가진다. 생성·갱신·제거는 Presentation ID로 수행하며 label, array index, SKU 또는 Scene hierarchy 이름으로 identity를 추론하지 않는다.

### 6.2 Data revision

`DataRevision`은 서버 Snapshot 또는 외부 관측의 내용 revision이다.

- 운영 데이터는 서버가 발급한 revision을 그대로 보존한다.
- 여러 source를 조합하면 단일 문자열로 덮지 않고 `DataRevisionSet`을 사용한다.
- cache가 최신인 것처럼 새 revision을 발급하지 않는다.

```csharp
public sealed record DataRevisionReference(
    string SourceStableId,
    string Revision,
    DateTimeOffset? EvidenceAsOfUtc);
```

### 6.3 Interpretation revision

`InterpretationRevision`은 다음 입력으로 결정적으로 계산한다.

```text
ordered DataRevisionSet
  + InterpreterContractVersion
  + RuleSetRevision
  + CatalogRevision
  + EvaluationTimeBucket(시간 의존 rule에만)
  + normalized parameters
  = InterpretationRevision
```

같은 입력인데 결과가 달라지면 test가 실패해야 한다. rule이 바뀌면 Data가 같아도 Interpretation revision은 바뀐다.

### 6.4 Presentation revision

`PresentationRevision`은 운영 상태 revision이 아니라 표현 계약의 revision이다.

```text
InterpretationRevision
  + PresentationPerspectiveCode
  + VisualRuleRevision
  + PresentationContractVersion
  + LayoutRevision
  + ThemeRevision
  + LocaleRevision
  + QualityTier
  = PresentationRevision
```

카메라 frame, hover와 animation progress마다 revision을 올리지 않는다. visual policy, perspective 또는 표시 모델 계약이 달라질 때만 변경한다.

전체 Snapshot revision과 별도로 항목별 Presentation revision을 둔다. 지역 한 곳의 라벨 변경이 모든 marker를 `Updated`로 만들지 않아야 한다. 현재 `WorldDataFlowRevisionCalculator`는 기존 호환 입력을 유지하고 DIP5R에서 선택형 catalog/layout/theme/locale 입력을 받는 새 overload로 확장한다.

### 6.5 Lineage 계약

```csharp
public sealed record InterpretationLineage
{
    public required DataRevisionReference[] Inputs { get; init; }
    public required string InterpreterContractVersion { get; init; }
    public required string RuleSetRevision { get; init; }
    public required string InterpretationRevision { get; init; }
    public required string[] EvidenceCardIds { get; init; }
    public required string[] LimitationCodes { get; init; }
}
```

사용자가 `왜 이렇게 보이나요?`를 선택하면 Presentation은 다음을 연결할 수 있어야 한다.

```text
PresentationRevision
  → InterpretationRevision + rule/evidence
  → DataRevisionSet + source/as-of/unit/precision
```

## 7. 여러 Data Snapshot의 교차 해석

Interpretation Layer는 단순한 `API 하나 → Interpreter 하나` 구조에 제한되지 않는다.

```text
PopulationData ──────┐
OrderData ───────────┤
TransportData ───────┤
WarehouseData ───────┼─→ LogisticsDemandInterpreter
RoadData ────────────┤       ↓
FacilityCostData ────┘   LogisticsSiteWorldState
```

교차 해석 전에 다음을 명시적으로 맞춘다.

| 정렬 축 | 필수 처리 |
| --- | --- |
| identity | 내부 stable ID 또는 검증된 crosswalk 사용 |
| geography | 행정동·법정동·SGIS code와 boundary version 분리 |
| time | 관측시각, 집계기간, timezone과 freshness 비교 |
| unit | 수량, 무게, 통화, 거리와 밀도 단위 정규화 |
| disclosure | suppression·masking·viewer scope 보존 |
| quality | missing, stale, estimated, noisy와 invalid 구분 |

정렬에 실패한 source는 임의 join하지 않는다. Interpreter는 `Incomplete`, `NotComparable`, `SuppressedInput`과 limitation을 결과에 포함한다.

인구는 잠재 수요 기반이고 주문은 실제 관측 수요다. 둘을 합친 단일 `수요값`을 만들지 않고 각각의 metric과 기여도를 lineage에 남긴다. 세부 지역 수요 설계는 [지역 인구·수요 World Layer 제안](RegionalPopulationDemandWorldLayerProposal.md)을 따른다.

### 7.1 Typed World graph

Interpretation 결과는 다목적 `SourceStableId` 체인이 아니라 typed node와 relation으로 관계를 표현한다.

```csharp
public enum WorldRelationKind
{
    Contains,
    AssignedTo,
    Carries,
    LocatedAt,
    Targets,
    HandoffTo,
    DerivedFrom,
    HasActivity,
    ContributesTo,
}

public sealed record WorldRelation(
    WorldStableId From,
    WorldStableId To,
    WorldRelationKind Kind);
```

`WorldGraphIndex<TNode>`는 `NodesById`, `Outgoing`, `Incoming`을 Snapshot 생성 시 한 번 구성하고 dangling relation과 중복 node를 거부한다. 공통 envelope와 graph는 공유하지만 Inventory, Task, NPC, Cargo, Board, Post처럼 업무 payload는 typed model로 유지하며 문자열 `Kind` switch를 새 코드에 추가하지 않는다.

선택된 ID 자체는 서버 Data도 World 사실도 아니므로 Zone-scoped `SelectionStateStore`가 소유한다. Interpretation graph가 관련 World ID를 계산하고 Selection Projector가 강조할 Presentation ID와 Detail 모델을 만든다. refresh 후 World ID가 남아 있으면 재투영하고 사라졌으면 선택을 해제한다.

## 8. 목표 물리 구조

장기 목표는 다음과 같다.

```text
Ssalddel.Unity/Runtime/
  Shared/
    Identity/
    Revisions/
    Provenance/
    Quality/
  Data/
    PublicData/
    Community/
    Warehouse/
    Transport/
    UrbanMarket/
    ResidentialPickup/
    Farm/
  Interpretation/
    WorldProjection/
    Spatial/
    Relations/
    Perspectives/
    Npcs/
    Simulation/
    PublicData/
    Warehouse/
    Transport/
    Farm/
  Application/
    Queries/
    Commands/
  PresentationContracts/
    Models/
    Reconciliation/
    Targets/

Unity presentation project or Samples~/
  <Zone>/Runtime/
    Presenters/
    SceneControllers/
    Views/
    Input/
    Animation/
    LifetimeScopes/
```

이 구조를 한 번에 이동하지 않는다. Unity `.meta`와 assembly reference, 외부 import path를 보존하기 위해 먼저 namespace와 책임을 분리하고, 실제 이동은 slice별 compile·test와 함께 수행한다.

engine-independent `PresentationContracts`에는 Unity type이 없는 PresentationModel, target port와 reconcile 결과만 둔다. `MonoBehaviour`, `Renderer`, `Animator`와 `NavMeshAgent`는 presentation project 또는 `Samples~`에만 둔다.

장기 Assembly 경계는 다음과 같다. 초기에는 package 분리를 강제하지 않고 namespace와 test로 먼저 경계를 고정한 뒤 slice별 asmdef로 이동한다.

```text
Ssalddel.Unity.Core
  ↑ Ssalddel.Unity.Data
  ↑ Ssalddel.Unity.Interpretation
  ↑ Ssalddel.Unity.Presentation

Ssalddel.Unity.Application       → Data/Interpretation/Presentation ports
Ssalddel.Unity.Infrastructure    → Data ports 구현
Ssalddel.Unity.Presentation.Unity→ Presentation + UnityEngine
Ssalddel.Unity.Composition       → 전체 조립
```

Data·Interpretation·순수 Presentation assembly는 `UnityEngine`을 참조하지 않는다. `Presentation.Unity`는 Infrastructure 구현을 참조하지 않고 port와 PresentationModel만 소비한다.

## 9. 현재 공통 클래스 Migration Map

| 현재 클래스·파일 | 현재 책임 | 목표 위치 | 조치 |
| --- | --- | --- | --- |
| `*ApiModel`, `*ApiRoutes` | wire contract | Data | 유지, feature별 Data namespace로 이동 |
| `*Mapper` | wire 검증과 일부 World 변환 | Data + Interpretation | `ApiModel→DataSnapshot`과 `DataSnapshot→WorldState`로 분리 |
| `*ApiRepository`, Repository port | source 조율 | Data | 유지 |
| `DataManager` | source 상태와 snapshot 보관 | Data/Shared | source·cache 상태만 유지 |
| `*QueryUseCase` | 조회 조율 | Application/Queries | Repository와 Interpreter를 조합하도록 명시 |
| `*LoadCoordinator` | load 상태와 마지막 성공 | Application/Queries 또는 PresentationContracts | Data cache와 화면 load state 책임 분리 |
| `WorldProjectionReconciler`, feature `*Reconciler` | stable-ID change set | PresentationContracts/Reconciliation | interpreted PresentationModel을 비교하도록 일반화 |
| `PageWorldProjectionCatalog` | route→World navigation 분류 | Interpretation/WorldProjection | 유지 |
| `WarehouseLocationCatalog` | semantic 위치 해석 | Interpretation/Spatial | 이름을 `WarehouseLocationResolver`로 수렴 |
| `WarehouseWorldSelectionService` | stable-ID 관계 + 선택 결과 | Interpretation/Relations + Presentation | relation graph 계산과 현재 선택 UI state 분리 |
| `ZoneNpcRouteCatalog` | Zone route 의미 | Interpretation/Npcs | 유지, Transform 참조 금지 |
| `TransportCorridorProjector` | 화물 인계→corridor 의미 | Interpretation/Transport | 유지 |
| `SensorProjectionResolver` | 상태→시각 code | Interpretation + Presentation | sensor 업무 의미와 visual policy로 분리 |
| `농업SimulationEngine` | derived simulation | Interpretation/Simulation | 입력 revision·rule lineage 추가 |
| `RoleExperienceCoordinator` | authorized 조회와 target 적용 | Data/Application + Presentation | authorized query와 presentation coordinator로 분리 |
| `*Applicator`, `I*Target` | core→View target 적용 | PresentationContracts | 유지하되 Presenter output만 받도록 변경 |
| `*ScreenModel`, validator | View-ready 계약 | PresentationContracts/Models | `PresentationModel` naming으로 점진 수렴 |
| `*SceneController` | lifecycle과 query 실행 | Presentation | 유지, Interpreter·Repository 직접 접근 금지 |
| `*View`, `*NpcView`, panel | Unity 표현 | Presentation | 유지 |
| `*LifetimeScope` | DI composition | Composition | layer별 registration group으로 정리 |

## 10. P0~P7 Architecture Migration Map

### P0. 도심 물류센터

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `RolePerspectiveApiModel`, `NpcMovementApiModel`, `CargoWarehouseHandoffApiModel`, 각 Mapper·Repository·UnityWebRequest adapter | authorized snapshot과 source 상태만 반환 |
| Interpretation | `ZoneNpcRouteCatalog`, `TransportCorridorProjector`, cargo handoff state mapping | role target, semantic route와 corridor 관계를 하나의 `UrbanLogisticsWorldState`로 조합 |
| Presentation | `도심물류센터SceneController`, `도심물류센터View`, `LogisticsRoleTargetView`, `TransportCorridorTruckView`, `LogisticsInteractionPanelView` | Presenter를 추가하고 View가 Data Snapshot을 받지 않게 함 |

`RoleExperienceCoordinator`는 authorized projection 조회와 View target 적용을 함께 하므로 우선 분리 대상이다.

### P1. 공공데이터 정보관

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `PublicWorldMap*ApiModel`, `PublicWorldMapMapper`, API Client·Repository, `PublicWorldMapSnapshot` | source·metric·위치·freshness를 보존하는 Data Snapshot으로 명명 |
| Interpretation | dataset/layer 의미와 향후 지역수요 Interpreter | point observation, region metric과 quality 의미를 World State로 변환 |
| Presentation | `PublicWorldMapReconciler`, `PublicDataHallLoadCoordinator`, `PublicDataHallSceneController`, `PublicDataHallView`, `PublicObservationMarkerView` | marker/heatmap PresentationModel, legend와 stable-ID reconcile 분리 |

현재 `PublicWorldMapMapper`가 transport 검증과 marker용 model 생성을 함께 하므로 지역 인구 Layer 전에 분리한다.

### P2. 커뮤니티·시장 광장

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `CommunitySquare*ApiModel`, Mapper·Repository, authorized/public snapshot | 게시판·게시글·활동·원장 요약 사실 보존 |
| Interpretation | board/post/activity/ledger의 World object 의미와 관계 | 공개 상태와 공동행동 의미를 stable-ID graph로 구성 |
| Presentation | `CommunityMarketSquareReconciler`, load coordinator, SceneController, Square View와 Item View | board·item·panel PresentationModel로 변환 |

게시글 본문·사용자 정보를 client 해석으로 복원하지 않는다.

### P3. 창고

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `WarehouseWorld*ApiModel`, API Client·Repository | `WarehouseDataSnapshot`으로 inventory, task, NPC 사실 보존 |
| Interpretation | 현재 `WarehouseWorldMapper`의 object 생성, `WarehouseLocationCatalog`, `WarehouseWorldSelectionService` 관계 계산 | `WarehouseWorldInterpreter`, `WarehouseLocationResolver`, `WarehouseRelationResolver`, `WarehouseWorldState`로 분리 |
| Presentation | feature reconciler/load coordinator, `WarehouseWorldSceneController`, `WarehouseWorldView`, object/NPC/detail View | `WarehousePresenter`와 `WarehousePresentationModel` 추가, View의 relation·상세 문자열 계산 제거 |

현재 `WarehouseWorldView`가 위치 resolution, relation selection, detail 문자열과 GameObject 생명주기를 함께 담당한다. Warehouse W1을 migration pilot으로 삼아 이 책임을 먼저 분리한다.

### P4. 운송 World

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `CargoWarehouseHandoffSnapshot`, `NpcMovementSnapshot`의 API 경계 | 운송·입고 canonical projection을 개별 Data Snapshot으로 보존 |
| Interpretation | `CargoWarehouseHandoffMapper`의 상태 의미, `TransportCorridorProjector`, NPC route 의미 | transport, cargo, handoff, waypoint를 `TransportWorldState` graph로 조합 |
| Presentation | `TruckMovementApplicator`, `NpcMovementApplicator`, `TransportCorridorTruckView`, `NpcMovementView`, `CargoWarehouseHandoffView` | Presenter가 route visual state와 action code를 결정 |

NPC 도착은 Presentation event이며 Command trigger가 아니다.

### P5. 도심마트

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `도심마트상품ApiModel`, `도심마트ApiMapper`, Repository, simulation fixture | 공개 판매정보와 authorized 마트 운영정보를 분리하고 각각의 DataSnapshot에 source·기준시각·origin·revision 보존 |
| Interpretation | 현재 별도 층 없음 | 상품·위치별 재고·진열대·작업 typed graph와 진열 보충 후보를 Shared World로 구성한 뒤 관리자 Perspective로 재해석 |
| Presentation | `도심마트ScreenModel`, validator, SceneController, 마트·진열대·가격표·재고 View | surface별 PresentationSnapshot과 visual policy를 만들고 View의 색·상자 수·상세 문구 판단 제거 |

상세 기준은 [Unity 도심마트 운영자 3계층 재정비 설계](UrbanMarketOperatorDataInterpretationPresentationRedesign.md)를 따른다. 현재 `api/v1/orderer/mart/products`의 `판매가능수량`은 내부 창고·진열 재고가 아닌 주문자용 공개 투영이므로 관리자 재고나 물리 선반 수량으로 해석하지 않는다.

### P6. 주거공동체 공동수령

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `ResidentialPickup*ApiModel`, Mapper·Repository·UseCase | 주문자/운송자별 authorized Data Snapshot 유지 |
| Interpretation | `ResidentialPickupPerspectiveApplicator` 앞의 역할별 object 의미 | 동일 수령 point와 task 관계를 `ResidentialPickupWorldState`로 통합 |
| Presentation | SceneController, `ResidentialPickupView`, Point View, Role Switch View | role switch는 Presentation Perspective만 바꾸고 권한 재조회 필요 여부를 명시 |

Unity role switch로 다른 역할의 데이터가 새로 허용되는 것으로 간주하지 않는다.

### P7. 농장·생산자

| 층 | 현재 코드 | 보완 방향 |
| --- | --- | --- |
| Data | `Farm*ApiModel`, `FarmSensorObservationApiModel`, Mapper·Repository, `CropReference*` | farm/plot/cultivation/sensor observation과 public crop reference를 별도 Data Snapshot으로 유지 |
| Interpretation | 서버 `ConditionCode`를 입력으로 작물·센서·작업 관계 구성, `ZoneNpcRouteCatalog` | `FarmWorldInterpreter`, sensor/crop/task relation과 evidence lineage 추가 |
| Presentation | `FarmProducerPerspectiveApplicator`, Farm SceneController/View, FarmTile·Crop·Sensor·FarmWorker View | Farm Presenter가 visual state를 만들고 `SensorView.ConditionColor` 같은 code mapping을 View에서 제거 |

농사로 작물 기준과 실제 생육 상태는 Interpretation에서도 같은 상태로 합치지 않는다.

### 공통 지원 slice. 전통시장·공개 물류거점

현재 `전통시장물류거점ScreenModel`과 simulated UseCase가 Presentation 계약을 직접 만든다. operational 연결 전 `TraditionalMarketHubDataSnapshot → TraditionalMarketHubWorldState → PresentationModel` 경계를 추가한다.

## 11. Migration 실행 순서

### DIP0. 기준 문서와 분류 고정

- 이 문서와 결정 기록 승인
- P0~P7 class inventory를 review checklist로 고정
- 새 코드가 어느 층인지 PR 설명에 기록
- 아직 파일 이동·rename 없음

### DIP1. 공통 계약 추가

2026-08-08 구현 완료. `Runtime/Data/WorldDataFlowRevisionModels.cs`에 공통 계약을 추가했으며 입력 순서 결정성, rule·perspective 변경과 중복 source 거부를 headless test로 고정했다.

- `DataRevisionReference`, `DataRevisionSet`
- `InterpretationLineage`
- `PresentationRevisionReference`
- `DataQualityCode`, `LimitationCode`
- generic `IInterpreter<TData, TWorld>`는 두 번째 실제 구현에서만 도입

처음부터 모든 feature를 위한 범용 base class나 event bus를 만들지 않는다.

### DIP2. Warehouse W1 migration pilot

2026-08-08 구현 완료. 기존 API route·JSON·`WarehouseWorldMapper.Map`과 `WarehouseWorldQueryUseCase(IWarehouseWorldRepository)`는 호환 facade로 유지하고, 기본 VContainer 경로는 `IWarehouseDataRepository → WarehouseWorldInterpreter → WarehousePresenter`를 사용한다.

```text
Warehouse ApiModel
  → WarehouseDataMapper
  → WarehouseDataSnapshot
  → WarehouseWorldInterpreter
     ├─ WarehouseLocationResolver
     └─ WarehouseRelationResolver
  → WarehouseWorldState
  → WarehousePresenter
  → WarehousePresentationModel
  → WarehouseWorldView
```

호환성 원칙:

- server route와 JSON contract를 변경하지 않음
- 현재 public class를 즉시 삭제·rename하지 않음
- adapter 또는 facade로 기존 test를 유지
- stable ID와 기존 server revision을 보존
- W1 operational refresh와 last-success 의미를 유지
- `WarehouseWorldView`에서 relation 계산과 상세 문자열 조립만 이동

### DIP3. 공통 stable-ID reconcile 정리

- feature별 change set의 공통 계약 추출
- Data revision 비교와 Presentation revision 비교 분리
- initial/refresh 오류와 object reconcile 분리
- 동일 revision에서 불필요한 View 재생성 방지

구현 상태(2026-08-08): `PresentationContracts/Reconciliation`에 Unity type이 없는 `StableIdChangeSet<T>`, `StableIdReconciliationPolicy<T>`와 `StableIdReconciler<T>`를 추가했다. 공통 계산기는 입력 순서와 무관한 stable-ID 정렬, add/update/remove/unchanged 계산, 중복 ID 거부, 낮은 Data revision 거부와 Presentation revision 동일 시 기존 instance 유지를 담당한다. 기존 `WorldProjectionReconciler`, `WarehouseWorldReconciler`, `PublicWorldMapReconciler`, `CommunityMarketSquareReconciler`는 공개 계약과 feature별 오류 의미를 보존하는 facade로 남겼다. Initial/refresh 오류와 last-success는 기존 load coordinator가 계속 담당하므로 object reconcile과 분리되어 있다. headless Unity core test 104건이 통과했으며 실제 Unity Editor import·Scene refresh는 이번 단계에서 재검증하지 않았다.

### DIP4. P0·P4 Role/NPC/Transport 분리

- authorized query와 Presentation Perspective 분리
- route/corridor Interpreter 출력 고정
- Applicator는 Presenter output만 소비
- NPC arrival과 Command 경계 test 유지

구현 상태(2026-08-08): 기존 `RoleExperienceCoordinator`는 호환 facade로 유지하고 실제 도심 물류센터 경로에는 `AuthorizedRoleProjectionQuery → RolePresentationPresenter → RolePresentationPerspectiveCoordinator → IRolePresentationTarget`을 추가했다. 서버가 허용한 Snapshot 조회와 Unity의 강조·panel 표현 적용이 별도 호출이 되었으며 authorization decision과 role·Zone 일치 검증은 기존 Repository 경계에 남는다. NPC는 `NpcMovementSnapshot → NpcMovementInterpreter → NpcMovementWorldState → NpcMovementPresenter → NpcMovementApplicator` 경로를, 운송은 canonical handoff와 movement revision을 가진 corridor lineage 뒤 `TransportCorridorPresenter → TruckMovementApplicator` 경로를 사용한다. 기존 Snapshot target overload는 다른 Zone의 점진 migration을 위한 호환 경로로 유지한다. NPC 도착 action은 `ArrivalAnimationCode`라는 Presentation 입력일 뿐 Command를 호출하지 않는다. headless Unity core 107건과 열린 Unity Editor의 package core 재컴파일은 통과했으나 `Samples~/UrbanLogisticsCenter`를 별도 import한 sample assembly·Scene reload는 이번 단계에서 재검증하지 않았다.

### DIP5. P1·P2 공개 World 적용

- PublicData와 Community Data Snapshot 분리
- region/marker/item World Interpreter 추가
- 지역 인구·수요 RD0는 이 단계가 완료된 계약 위에서 시작

구현 상태(2026-08-08): P1은 `PublicWorldMapApiModel → PublicWorldMapDataMapper → PublicWorldMapDataSnapshot → PublicWorldMapInterpreter → PublicWorldMapSnapshot → PublicDataHallPresenter → PublicDataHallPresentationModel` 경로를 추가했다. Data Snapshot은 layer·metric·관측값과 source·기준시각·공간 정밀도·freshness를 보존하고, Interpreter가 공개 World marker 의미와 input/rule lineage를 만든다. P2는 board·post·activity·ledger 배열을 `CommunitySquareDataSnapshot`에 보존한 뒤 `CommunitySquareWorldInterpreter`가 stable-ID World item과 관계 검증·lineage를 만들고 `CommunitySquarePresenter`가 label·detail·visual state를 결정한다. 두 sample Controller는 DataFlow load coordinator를 조회하고 View에는 Presenter output만 전달한다. 기존 Mapper·Repository·LoadCoordinator 공개 계약은 다른 소비자의 점진 migration을 위한 facade로 유지한다. refresh 실패 시 마지막 성공 Presentation을 유지하는 정책과 stable-ID reconcile 의미를 회귀 검증했다. headless Unity core 111건과 열린 Unity Editor의 package core 재컴파일은 통과했지만 `Samples~/PublicDataHall`, `Samples~/CommunityMarketSquare`의 별도 sample assembly import와 Scene reload는 이번 단계에서 재검증하지 않았다. 이 계약을 기반으로 지역 수요 RD0를 시작할 수 있다.

### DIP5R. 3계층 책임 보강

지역 수요 RD0보다 먼저 다음 기반을 좁은 호환 변경으로 보완한다.

1. `SourceStableId`, `WorldStableId`, `PresentationStableId`와 identity lineage 계약
2. `WorldRelationKind`, typed node, `WorldGraphIndex<TNode>`와 graph validation
3. Shared World Interpreter와 Perspective Interpreter·Context 분리
4. `SelectionStateStore`와 Selection Interpretation/Projector 분리
5. Application Runtime의 `RefreshDataAsync/ReinterpretShared/ReinterpretPerspective/Reproject`와 Runtime Status channel
6. Session·World·Authorization·DataScope를 묶는 Data Context/Runtime과 context-scoped cache
7. surface별 PresentationSnapshot·item revision·Reconciler
8. PublicData visual metadata의 Data→Presentation 이동과 `PublicWorldState` 도입
9. Community typed graph 도입과 평면 `Items` 계약의 facade화
10. Warehouse `SourceStableId` chain을 typed relation graph로 변환하는 compatibility adapter
11. 계층 의존 architecture tests와 선택적 asmdef 분리

이 단계에서는 기존 JSON, route, public facade와 Scene wiring을 삭제하거나 rename하지 않는다. 새 계약을 병행 추가하고 PublicData 한 surface와 Warehouse selection 한 경로를 pilot으로 전환한 뒤 Community에 적용한다.

구현 상태(2026-08-08): DIP5R-1로 세 identity, lineage와 `WorldGraphIndex<TNode>`를 추가했다. DIP5R-2 Runtime은 `ISharedWorldInterpreter`와 `IPerspectiveInterpreter`를 분리하고 `InterpretationPerspectiveContext`에 role·intent·Zone·focus·Operational/Simulation mode를 명시했다. `WorldReadRuntime`은 `RefreshDataAsync`, `ReinterpretShared`, `ReinterpretPerspective`, `Reproject`를 구분하며 모든 단계와 diff 성공 뒤에만 last-success를 교체한다. 동일 authorization scope의 refresh 실패에서만 이전 표현을 유지하고, scope 변경 시 private cache와 selection을 먼저 제거하며 cancellation은 오류로 변환하지 않는다.

DIP5R-3에서는 PublicData를 다음 실행 경로로 전환했다.

```text
Authorized Public Data Snapshot
  → PublicSharedWorldInterpreter
  → PublicWorldState
  → PublicWorldPerspectiveInterpreter
  → PublicWorldPerspectiveState
  → PublicDataHallSurfaceProjector
  ├─ Marker Surface
  ├─ Legend Surface
  ├─ Heatmap Surface
  └─ Detail Surface
  → surface별 StableIdChangeSet
  → PublicDataHallView marker applicator
```

서버 wire의 `Color`와 `MarkerShape`는 기존 contract 호환을 위해 Data Snapshot에 보존하지만 Shared World State에는 포함하지 않는다. 실제 색·형태는 `PublicDataHallVisualPolicy`가 Presentation 단계에서 결정한다. Marker·Legend·Heatmap·Detail은 서로 다른 item revision과 change set을 사용하므로 한 surface의 변경이 다른 surface의 Unity object 갱신을 유발하지 않는다. 지역 geometry가 없는 현재 Heatmap은 수치를 꾸며내지 않고 `RegionGeometryMissing` 제한 상태를 명시한다.

`PublicDataHallSurfaceRuntimeCoordinator`는 기존 `PublicDataHallDataFlowLoadCoordinator`를 즉시 삭제하지 않는 병행 adapter다. Sample Controller는 새 Runtime을 기본 경로로 사용하고 VContainer 설정의 operational/simulation mode와 authorization scope를 명시한다. 오류 원문은 View에 넘기지 않으며 refresh 실패에서는 마지막 성공 marker를 유지한다.

Warehouse selection pilot은 기존 `Kind`·`SourceStableId` 문자열 체인을 `WarehouseWorldGraphBuilder`가 typed relation으로 변환한 뒤 `WarehouseRelationResolver`가 outgoing/incoming index의 연결 요소를 탐색하도록 바꿨다. 기존 snapshot과 selection facade는 호환을 위해 유지한다. PublicData surface, Warehouse typed graph와 Runtime 통합을 포함한 Unity core headless test 134건이 통과했다. 새 PublicData sample 코드는 작성했지만 Unity Editor sample assembly import·Scene runtime은 아직 재검증하지 않았다.

DIP5R Data Context 보강으로 `UserSessionContext`, `WorldContext`, `DataAuthorizationContext`, `WorldDataContext`와 네 종류 `DataScopeKind`를 추가했다. `ContextScopedSnapshotCache`는 World 전환, authorization 변경과 logout에서 scope별로 entry를 폐기하고 Global public entry는 유지한다. `WorldReadRuntime`에는 기존 문자열 scope API를 유지하면서 `IContextualWorldDataQuery`와 `WorldDataQueryContext`를 받는 overload를 추가했다. PublicDataHall은 이 경로의 첫 Global Data 소비자로 전환했다. Data Context 전환·cache 격리·selection 해제와 contextual query를 포함한 Unity core headless test 140건이 통과했다.

### DIP6. P5~P7 적용

- 도심마트는 [운영자 3계층 재정비 설계](UrbanMarketOperatorDataInterpretationPresentationRedesign.md)의 UM0~UM3 → UM3R → UM4~UM5 순서로 공개 상품 호환 경로와 관리자 simulation 경로를 분리
- 도심마트 fixture→Data Snapshot→Shared World→Manager Perspective→surface Presentation 전환
- 첫 업무는 진열 보충 후보·차단 사유의 read-only/simulation proof이며 operational Command는 서버 canonical 진열·위치별 재고·작업 계약 이후 연결
- 공동수령 authorized/presentation perspective 분리
- Farm sensor/crop/task Interpreter와 Presenter 분리
- View의 status code→color/text switch를 visual policy로 이동

구현 상태(2026-08-09): 도심마트 UM0~UM1에서 공개 주문자 route의 `ApiModel → 도심마트공개상품DataSnapshot` 경로, Data validator, operational repository와 simulation query를 추가했다. Data 계약은 `OrdererPublic` audience와 `ProjectedSaleAvailability` 수량 의미를 강제하고 보관·진열·예약 재고를 만들지 않는다. 기존 ScreenModel 경로는 compatibility adapter로 유지했다.

UM2는 공개 상품 World에 물리 재고·진열대 node를 만들지 않고, 별도 관리자 Simulation Data에서만 상품·위치·재고·진열대·작업 typed graph를 구성한다. UM3 `도심마트진열보충Interpreter`는 목표 진열률과 rule revision을 입력으로 보충 후보 수량, 입고 필요, 활성 작업 중복, 데이터 불충분과 server capability 차단을 계산하되 Command를 호출하지 않는다. UM3R은 모든 비종료 allocation을 원천 재고별로 집계해 `OnHand / Allocated / Available`을 만들고, 명시적 allocation이 없는 기존 작업은 legacy 한 건으로 정규화한다. UM4는 무결성 검증된 Shared World의 모든 진열 상태를 `NeedCode`·차단 사유·허용 interaction·SourcePlan과 함께 보존하며 우선순위 점수나 업무 queue를 만들지 않는다. UM5 Runtime은 shelf·task·source-plan·detail surface를 stable-ID change set으로 갱신하고 refresh 실패 시 마지막 성공 화면을 유지한다.

### 이후 기능 순서

```text
DIP0~DIP2
  → Warehouse W2
  → DIP3~DIP5
  → DIP5R 공통 identity/graph/runtime 보강
  → 지역 수요 RD0·RD1·RD2
  → Warehouse W3와 다른 Zone 심화
  → DIP6를 해당 기능 변경과 함께 완료
```

모든 feature를 먼저 일괄 이동한 뒤 기능 개발을 재개하지 않는다. Warehouse pilot로 기준을 검증하고, 다음에 수정하는 slice에 적용하는 점진 migration을 사용한다.

## 12. 검증 전략

| 계층 | 단위 검증 |
| --- | --- |
| Data | JSON fixture, 필수 field, unknown code, unit, source, precision, suppression, cancellation |
| Repository | initial/refresh failure, last-success, no operational→fixture fallback |
| Interpretation | 동일 입력 결정성, relation integrity, 시간·지역·단위 불일치, missing/stale/suppressed input |
| Lineage | input revision·rule revision 변화가 Interpretation revision에 반영됨 |
| Presentation | perspective별 PresentationModel, legend, label, accessibility와 no-access 상태 |
| Reconcile | stable-ID add/update/remove, 동일 revision 유지, selection 복원 |
| Command | preview·confirm·expected revision·server success·canonical re-query 순서 |
| Unity EditMode | View socket과 Presenter binding, Scene reload wiring |
| Unity PlayMode | lifecycle, refresh, object 유지, animation·NavMesh 표현 |

Architecture test 후보:

- Data namespace가 `UnityEngine`과 Presentation namespace를 참조하지 않는다.
- Interpretation namespace가 `UnityEngine`, MonoBehaviour와 API Client 구현을 참조하지 않는다.
- Presentation View가 ApiModel·Repository를 참조하지 않는다.
- SceneController가 Mapper와 Repository를 직접 조합하지 않는다.
- Operational Interpreter가 simulation fixture를 fallback으로 선택하지 않는다.
- `PresentationRevision`이 canonical expected revision으로 사용되지 않는다.

## 13. Code review 판단표

| 질문 | Yes일 때 위치 |
| --- | --- |
| JSON field·HTTP·cache·source 사실을 다루는가? | Data |
| 여러 snapshot을 join하거나 상태·관계·공간 의미를 만드는가? | Interpretation |
| label·color·icon·animation·GameObject·panel을 결정하는가? | Presentation |
| 여러 단계와 cancellation을 조율하는가? | Query Application |
| 운영 상태 변경을 요청하는가? | Command Application |
| 구현 선택과 lifetime을 구성하는가? | Composition |

다음 징후는 분리 신호다.

- Mapper가 `WorldObject`, color 또는 label을 만든다.
- View가 source code, freshness, 업무 relation을 판정한다.
- SceneController가 두 Repository 결과를 직접 join한다.
- UseCase가 Renderer용 상태 code를 반환한다.
- role switch가 authorization filter처럼 동작한다.
- simulation 결과가 server revision 없이 operational object를 갱신한다.

## 14. 완료 정의

3계층 migration은 단순히 폴더 세 개가 생겼을 때 완료되지 않는다. 다음 질문에 코드와 revision으로 답할 수 있어야 한다.

> 어떤 허용 데이터가 들어왔는가?
>
> 어떤 rule과 관계로 이 World 의미가 만들어졌는가?
>
> 어떤 표현 관점과 visual rule로 현재 모습이 선택됐는가?
>
> 사용자의 행동이 운영 상태를 바꿨다면 어떤 Command와 canonical 재조회가 있었는가?

Warehouse W1 pilot에서 이 추적이 가능하고 기존 operational refresh 동작이 유지되면, 이후 Zone과 지역 인구·수요 Layer의 기본 아키텍처로 확장한다.
