# Ssalddel Research-Grounded World Projection Project Structure

> Unity API Client·Repository·UseCase·Presenter·SceneController·View·Prefab·Inspector의 단일 기준은 [Unity 클라이언트 계층 구조 설계](../docs/Architecture/UnityClientLayeredArchitecture.md)다. 이 문서는 package-local 구조와 현재 구현 상태만 요약한다.

## 제품 정의

Ssalddel Unity는 연구자료와 실제 데이터를 권위 서버에서 관리하고 그 상태를 탑다운 월드, 센서와 업무 오브젝트로 체험하게 만드는 연구 근거 기반 World Projection Client다.

Unity는 운영 상태의 최종 권위가 아니다. 실제 주문, 참여, 운송, 검수와 원장 상태는 서버 UseCase가 검증하고 저장한다. Unity의 애니메이션이나 GameObject 상태만으로 운영 효과를 확정하지 않는다.

## 계층 구조

```text
Ssalddel server
  ├─ public evidence projection
  ├─ authorized ledger projection
  └─ validated command API
       ↓
Unity transport and P2 runtime
  ├─ UnityWebRequest API Client
  ├─ repository adapter and state store
  ├─ use case and presenter
  └─ scene controller and View
       ↓
Ssalddel.Unity engine-independent core
  ├─ ApiModels
  ├─ Mapping
  ├─ Data
  ├─ Simulation
  ├─ WorldProjection
  ├─ Evidence
  ├─ Sensors
  └─ Interactions
       ↓
Unity presentation assembly
  ├─ WorldBootstrapScene
  ├─ CommunityMarketSquare
  ├─ Farm
  ├─ VisualRoot wrappers
  ├─ top-down input and camera
  └─ panels, animation, VFX and SFX
```

현재 체크아웃에는 engine-independent core가 있다. 사용자가 확인한 P2 `UnityWebRequest → Repository → UseCase → Scene Controller → Presenter → WorldBootstrapScene` 소스는 이 체크아웃에서 발견되지 않았으므로, 이 구조는 그 코드를 대체하지 않고 결합 지점을 제공한다. DTO 호환성 판정은 Repository에 섞지 않고 기존의 명시적 Mapper에 유지한다.

## Runtime 폴더 책임

| 폴더 | 책임 | 금지 |
| --- | --- | --- |
| `ApiModels` | 서버 JSON transport model | 게임 규칙과 GameObject 조작 |
| `Mapping` | API model을 검증된 game snapshot으로 변환 | 오류를 이름 없는 기본값으로 숨김 |
| `Data` | stable ID, provenance, cache·fixture·invalid 상태 | UnityEngine과 서버 assembly 참조 |
| `Simulation` | 결정적 학습 시나리오와 계산 | 실제 주문·원장 생성 |
| `WorldProjection` | page-to-world catalog, world object snapshot, stable-ID reconcile | Scene과 Prefab 직접 생성 |
| `Evidence` | 연구 주장, 제품 해석, 시각 번역과 한계 분리 | 연구 주장과 예술 표현 혼합 |
| `Sensors` | Sensor 상태의 외부 장비 projection | raw 값으로 새 판정 생성 |
| `Interactions` | preview, 확인, 서버 명령, canonical 재조회 상태 계약 | 애니메이션 완료를 업무 성공으로 간주 |

## Page-to-World 구조

`PageWorldProjectionCatalog`는 전체 route를 한 번에 복제하지 않고 대표 route부터 다음 정보로 분류한다.

```text
RoutePattern
BusinessName
RoleCodes
WorldZoneCode
ProjectionTypeCodes
WorldObjectKey
InteractionCode
PanelCode
StableIdPrefix
ViewerScopeCode
InteractionEffectCode
ProjectionStageCode
RequiresExplicitConfirmation
RequiresCanonicalStateRefresh
```

현재 대표 18개 route가 들어 있다. 운영 command는 명시적 확인과 canonical state 재조회 없이는 catalog validation을 통과하지 못한다. 계좌 같은 민감 페이지는 `KeepWeb`과 `WebHandoff`로 남긴다.

## World Zone

```text
community-market-square
public-data-hall
farm
cooperative-hall
market-order
urban-logistics-center
warehouse
personal-meditation
```

첫 Unity presentation slice는 `community-market-square`다. 운영 지도, 커뮤니티 게시판, 공공데이터 정보대, 지역문화 전시대와 다른 zone portal을 primitive로 표현한다.

## Stable-ID 증분 표현

`WorldProjectionReconciler`는 snapshot을 stable ID로 비교한다.

```text
새 ID       → Added
기존 ID 변경 → Updated
사라진 ID    → Removed
동일 상태    → Unchanged
중복 ID      → snapshot 거부
낮은 revision → snapshot 거부
```

P2 관측 마커 presenter는 기존 자체 reconcile을 유지하거나 이 공통 change set으로 수렴할 수 있다. 초기 로드 실패와 refresh 실패의 화면 유지 정책은 transport/use-case 계층에 남고, reconciler는 유효 snapshot 간 차이만 계산한다.

## 센서 Projection

`농장SensorState`가 측정값, 기준시각, 데이터 상태, 상위 판정 결과와 근거 reference를 보존한다.

```text
농장SensorState
  ├─ SensorId / Revision
  ├─ source / value / unit / observedAt
  ├─ ConditionCode
  └─ RuleVersion / EvidenceCardIds / confidence / limitation
       ↓
SensorProjection
  └─ 외부SensorVisualState
```

Sensor projection은 측정값을 재해석하지 않는다. 연구 근거와 rule version을 통해 이미 판정된 `Normal`, `Dry`, `Critical`, `Waterlogged`, `Stale`, `Offline`을 장비 상태, 표시등과 material 상태로 번역한다. 표현 결과는 원본 SensorId, revision과 evidence card를 보존한다.

## 연구 근거

`연구근거Card`는 다음 세 층을 반드시 분리한다.

1. 자료가 직접 뒷받침하는 주장
2. Ssalddel의 제품 판정 규칙 해석
3. Unity의 시각적 번역

source, scope, limitation과 version이 없으면 validator가 거부한다. Zotero와 Notion은 authoring·review 도구로 사용할 수 있지만, 제품에 승인된 reference와 rule version만 Git의 package/data에 들어온다.

## Synty 결합 경계

Synty 원본 Prefab에 Ssalddel 업무 로직을 직접 추가하지 않는다.

```text
SsalddelSensorView
  ├─ Sensor stable ID binding
  ├─ revision guard
  ├─ click and evidence-card binding
  └─ VisualRoot
       ├─ 현재: Cube placeholder
       └─ 이후: Synty model or project visual
```

동일한 wrapper 구조를 `CommunityBoardView`, `TransportTruckView`, `WarehousePalletView`에도 적용한다. 외형 교체가 server contract, rule과 tests를 변경하지 않아야 한다.

## 다음 구현 순서

1. 도심마트 기준 slice와 전통시장·공개 물류거점 slice를 고정한다.
2. 공공데이터 정보대와 커뮤니티 게시판을 공개 projection으로 연결한다.
3. 입고·분류·보관·출고·운송 인계를 묶는 `도심물류센터` Zone을 구성한다.
4. 창고·재고 slice와 운송·배송 slice를 별도 authorized snapshot으로 연결한다.
5. 협동조합 원장 board와 주문 board로 인계 상태를 연결한다.
6. canonical server source가 생기기 전까지 농장·sensor·작물은 `SimulatedFixture`로만 구성한다.
7. 실제 Unity project 위치와 composition root가 확정되면 Repository·Mapper adapter를 연결한다.
8. 필요할 때 Windows·Android PlayMode와 build 성능을 검증하고 Synty 구매 대상을 산정한다.

## 도심마트 Presentation Sample

`Samples~/UrbanMarket`은 첫 View→Controller vertical slice다.

```text
Simulated도심마트조회UseCase
  → 도심마트ScreenModel
  → 도심마트SceneController
  → 도심마트View
      ├─ 상품진열대View[3]
      ├─ 상품상자View
      ├─ 가격표View
      ├─ 재고상태View
      ├─ 정보키오스크View
      └─ InteractionSocket
```

fixture는 감자 20kg 35,000 KRW·12상자와 쌀·양파를 포함하며 모든 source를 `SimulatedFixture`로 표시한다. Editor builder는 primitive scene과 Inspector reference를 생성한다. 실제 API 연결은 `I도심마트조회UseCase` 구현을 composition root에서 Controller에 주입하는 방식으로 교체한다.

## 전통시장·물류거점 Presentation Sample

`Samples~/TraditionalMarketHub`는 두 번째 View→Controller vertical slice다.

```text
Simulated전통시장물류거점조회UseCase
  → 전통시장물류거점ScreenModel
  → 전통시장물류거점SceneController
  → 전통시장물류거점View
      ├─ 시장건물View
      ├─ 물류거점View
      ├─ 입고·픽업 Dock
      └─ 상세 panel
```

공개 projection에서 허용되는 `Pilot`, `Active` 상태와 검증된 위치 정밀도만 표현한다. fixture는 `SimulatedFixture`로 표시하고 실제 API adapter는 server DTO를 명시적 Mapper로 ScreenModel에 변환한다.

## 검증 경계

- `dotnet build`와 `dotnet test`는 engine-independent 계약을 검증한다.
- Unity EditMode·PlayMode는 GameObject, Scene reload와 UI 표현을 검증한다.
- built player는 실제 target platform의 렌더링과 성능을 검증한다.
- 실제 API 검증과 Game View capture는 서로 다른 증거로 기록한다.
- commit, push, 배포와 Asset Store 구매는 별도 사용자 승인이 필요하다.
