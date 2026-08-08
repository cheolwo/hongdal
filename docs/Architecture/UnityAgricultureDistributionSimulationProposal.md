# 살뜰 Unity 농업·유통·경영 시뮬레이션 제안서

> 상태: 제품 방향과 단계별 구현 기준, 데이터 코어 1차 구현
>
> 구현 상태: `Ssalddel.Unity`의 engine-independent 데이터 package와 golden 감자 시나리오를 구현했으며 Unity Editor project, GameObject, server API는 아직 만들지 않음
>
> 상위 제품·네트워크·Prefab 기준: [살뜰 Unity 생산·유통·협력 경험 플랫폼 종합 제안서](UnityCooperativeExperiencePlatformProposal.md)
>
> WorldManager·DataManager·UseCase·원장·GameObject 책임 기준: [살뜰 비즈니스 도메인의 Unity World·원장 투영 아키텍처](UnityWorldLedgerProjectionArchitectureProposal.md)
>
> 우선 기준: [0.0 집중 로드맵](../Versions/v0.0/focus-roadmap.md), [커뮤니티 0.0 기반 제품 원칙](CommunityFoundationV0Policy.md), [업무 실행 책임 모델](BusinessWorkflowResponsibilityModel.md), [입력·데이터 수집 처리 파이프라인](DataInputCollectionProcessingPipelineProposal.md)

## 0. 현재 구현 상태

2026-08-05 기준 첫 데이터 준비 slice를 구현했다.

| 구분 | 현재 상태 | 근거 |
| --- | --- | --- |
| Unity data package | 구현 | `Ssalddel.Unity/package.json`, `Ssalddel.Unity.Data.asmdef` |
| 독립 빌드 | 검증 | `netstandard2.1`, UnityEngine·서버 contract 참조 없음 |
| API model·Mapper | 구현 | 시장가격 관측 transport model과 Unity game snapshot 분리 |
| provenance·stable ID·단위·hash | 구현 | 시나리오 validator와 SHA-256 package 봉인 |
| DataManager | 구현 | `ReadyLive`, `ReadyCached`, `ReadyFixture`, `Invalid`, `Failed` 분리 |
| 첫 농업 계산 | 구현 | 심기, 물주기, 7일 성장, 수확, 생산비와 판매 방식 비교 |
| golden fixture | 구현·검증 | 감자 1종, 날씨 7일, 가상 KAMIS 형태 가격, 12개 Command |
| Unity Editor·scene·GameObject | 미구현 | 현재 환경에 Unity Editor가 없음 |
| 살뜰 server API | 미구현 | 실제 provider 호출과 운영 상태 변경 없음 |
| 실제 공공데이터 | 미연결 | 현재 값은 모두 `SIMULATED FIXTURE` |

현재 구현은 제안서의 데이터 준비 gate를 코드와 headless test로 닫는 범위다. Unity Test Runner와 실제 scene 실행은 Editor project가 정해진 뒤 별도 검증한다.

## 1. 제안 목적

살뜰의 공공데이터와 원장(Business Case) 개념을 활용해 농업·유통·경영 구조를 체험하는 Unity 교육형 시뮬레이션을 만든다.

이 게임의 목표는 단순히 농장을 꾸미거나 작물을 수확하는 데 있지 않다. 사용자가 한 작물의 생산부터 판매까지를 직접 선택하면서 다음 관계를 이해하게 하는 것이 핵심이다.

- 날씨·토양·재배 방식이 생산량과 품질에 미치는 영향
- 산지·도매·소매 가격이 서로 다른 시장 단계의 관측값이라는 사실
- 인건비·장비·운송·보관 비용을 포함해야 실제 수익을 판단할 수 있다는 점
- 일반판매와 공동판매가 가격뿐 아니라 노동, 시간, 위험과 역할 분담에서 어떻게 다른지
- 공공데이터의 출처, 기준 시각, 지역, 단위와 한계를 읽는 방법
- 관심, 참여 의사, 공동 원장과 실제 실행이 서로 다른 상태라는 점

제품의 한 줄 정의는 다음과 같다.

> **데이터를 기반으로 현실의 농업·유통·협력을 이해하는 가상세계**

## 2. 제품 지위와 현재 살뜰 범위의 관계

Unity 게임은 살뜰 서버의 새 운영 주체가 아니라 **읽기 중심의 시뮬레이션 클라이언트**다. 현재 기본 공개 범위인 `0.0` 커뮤니티·공공데이터 기반을 존중하며, `0.5` 이후의 주문·공동구매·운송·창고 기능을 게임 안에서 곧바로 운영 기능으로 열지 않는다.

초기 버전은 다음 경계를 지킨다.

| 구분 | 초기 Unity 게임 | 살뜰 운영 기능 |
| --- | --- | --- |
| 공공데이터 | 서버가 검증·저장한 공개 projection 조회 | 수집, 정규화, 출처·품질 관리 |
| 작물·시장 계산 | 로컬 시뮬레이션 | 운영 가격·계약 확정 아님 |
| 가상 참여자 | `SIMULATED`로 표시한 고정 또는 생성 시나리오 | 실제 사용자·공급자 아님 |
| 원장 | 로컬 학습 원장 또는 별도 학습 기록 | 실제 가원장·공동 원장과 분리 |
| 공동판매 | 역할·비용·위험을 배우는 비교 시나리오 | 모집·주문·계약·정산을 실행하지 않음 |
| 외부 효과 | 없음 | 명시적 동의와 운영 gate가 있는 서버 UseCase만 가능 |

게임 결과를 실제 관심, 참여 의사, 주문 또는 공동 원장으로 자동 전환하지 않는다. 향후 살뜰 계정과 연결하더라도 별도 화면에서 목적·공개 범위·동의를 다시 확인해야 한다.

## 3. 핵심 개발 원칙

### 3.1 데이터가 먼저다

Unity를 데이터를 기반으로 가상세계를 표현하는 엔진으로 사용한다. 그래픽 에셋이나 애니메이션보다 먼저 데이터 계약, 계산 규칙, 상태 전이와 검증 가능한 시나리오를 설계한다.

### 3.2 서버는 근거를, Unity는 표현과 시뮬레이션을 맡는다

```text
살뜰 공공데이터 원본·정규화본·공개 projection
  → 읽기 전용 API
  → Unity API model
  → Mapper
  → Unity game model
  → DataManager
  → FarmTile · Crop · Market · UI
```

- 살뜰 서버는 공공데이터의 수집, 출처, 기준 시각, 단위, 지역, 갱신 주기, 품질과 공개 범위를 관리한다.
- Unity는 서버의 운영 Entity나 공유 DTO를 참조하지 않고 게임에 필요한 값만 변환한다.
- GameObject는 원본 데이터 저장소가 아니다. 현재 장면에 필요한 표현 상태와 식별자만 가진다.
- 서버 데이터가 없거나 오래되었을 때 최신값처럼 꾸미지 않는다. 마지막 관측 시각과 제한을 표시하거나 명시적 fixture 모드로 전환한다.

### 3.3 실제 관측과 게임 규칙을 분리한다

공공데이터 관측값은 현실의 근거이고, 성장률·수확량·판매 결과는 게임 규칙의 계산값이다. 둘을 한 DTO나 한 숫자로 합치지 않는다.

예를 들어 KAMIS 도매 관측가격을 게임의 판매가격 계산에 사용했다면 다음을 함께 보존한다.

- 원 관측의 source key, 품목, 등급, 시장 단계, 지역, 조사일, 통화와 단위
- 게임이 적용한 환산 규칙과 rule version
- 난이도, 품질, 가상 수급 같은 시뮬레이션 보정값
- 최종 계산값이 실제 견적이나 가격 보장이 아니라는 안내

### 3.4 실행 모드는 공통 경계를 따른다

게임의 외부 효과 경계도 `SsalddelExecution:Mode`의 `Simulation`과 `Operational` 구분을 따른다. 초기 Unity 클라이언트는 `Simulation`만 지원한다. 화면마다 별도의 임의 실행 모드를 만들지 않는다.

## 4. DTO와 모델 분리

살뜰 서버 계약 assembly를 Unity project가 직접 참조하지 않는다. Unity에는 다음 두 계층을 별도로 둔다.

```text
HTTP JSON
  → Unity ApiModels       서버 응답을 받는 transport 전용 model
  → ApiToGameMapper       누락값·단위·품질·호환성 처리
  → Unity GameModels      게임 계산과 표현에 필요한 model
```

`ApiModels`도 서버 DTO와 소스 코드를 공유하지 않는다. JSON 호환성만 계약 test로 확인한다. 이 구조는 서버 변경이 Unity scene과 GameObject에 직접 전파되는 것을 막는다.

예시 책임은 다음과 같다.

| 계층 | 예시 | 포함 | 제외 |
| --- | --- | --- | --- |
| 서버 공개 응답 | `공개시장가격관측응답` | source, observedAt, marketStage, region, unit, value, limitation | 내부 Entity, 수집 credential |
| Unity API model | `MarketPriceObservationApiModel` | JSON 수신에 필요한 nullable field | 게임 성장·수익 계산 |
| Mapper | `MarketPriceObservationMapper` | schema version, 필수값, 단위, freshness 검사 | 장면 조작 |
| Unity game model | `MarketPriceSnapshot` | 품목 key, 정규화 가격, 게임 기준일, provenance 요약 | 서버 저장 구조 전체 |

Mapper는 오류를 조용히 기본값으로 바꾸지 않고 `Mapped`, `UnsupportedSchema`, `MissingRequiredField`, `IncompatibleUnit`, `Stale` 같은 결과를 반환한다.

## 5. 데이터 영역

모든 데이터는 stable key, schema/rule version과 출처 유형을 가진다. 초기에는 한 작물에 필요한 최소 필드부터 만들고, 다음 영역으로 확장한다.

| 영역 | 주요 데이터 | 초기 적용 | 확장 방향 |
| --- | --- | --- | --- |
| 농업 | 작물, 씨앗, 성장 단계, 재배 방법, 병충해 | 작물 1종, 4개 성장 단계 | 품종, 윤작, 방제 선택 |
| 환경 | 날씨, 기온, 강수량, 토양 | 일별 기온·강수 fixture | 기상청 관측·예보, 토양 성분 |
| 시장 | 산지가, 공판장, 도매가, 소매가, 수입·수출 | KAMIS 관측가격 1개 시장 단계 | 시장 단계별 비교, 무역 통계 |
| 경영 | 인건비, 생산비, 장비, 운송비, 세금, 수익 | 종자·노동·운송 원가 | 감가상각, 자금 흐름, 세금 규칙 |
| 제조 | 가공, 공정, 제조원가, 부산물 | 초기 범위 제외 | 원물 등급별 가공과 수율 |
| 물류 | 창고, 운송, 배송, 연료 | 판매 선택의 가상 운송비 | 온도대, 거리, 보관 손실 |
| 살뜰 | 관심, 참여 의사, 공동 원장, 공개 여부 | 학습용 공동판매 원장 | 별도 동의가 있는 실제 화면 연결 |
| 공공데이터 | KAMIS, 농림축산식품부, 기상청, 통계청 등 | 저장된 KAMIS 공개 projection | source별 adapter와 품질 표시 |

공개 사실은 공급 가능성, 재고, 계약 의사나 실시간 위치를 뜻하지 않는다. 기관·시설·가격 관측이 공개되어 있어도 게임은 이를 실제 거래 상대나 확정 가격으로 표현하지 않는다.

## 6. 데이터 중심 아키텍처

### 6.1 데이터 종류와 권위

데이터를 한 저장소나 한 model에 모으지 않고 의미와 변경 주기에 따라 분리한다.

| 데이터 종류 | 예시 | 권위 있는 위치 | 변경 방식 |
| --- | --- | --- | --- |
| 기준정보 `Master Data` | 작물·품종·지역·시장 단계·단위 code | versioned game catalog 또는 서버 공개 catalog | 검토된 version 배포 |
| 현실 관측 `Observation` | 기온, 강수량, KAMIS 조사 가격 | 살뜰 서버의 출처 보존 snapshot | source별 수집·정정 |
| 게임 규칙 `Rule Set` | 성장률, 수분 감소, 품질, 원가·판매 계산 | Unity versioned rule package | balance version 배포 |
| 시나리오 `Scenario` | 작물·기간·지역·난이도·사용 snapshot | immutable scenario package | 새 scenario version 생성 |
| 실행 상태 `Runtime State` | tile 수분, 작물 성장, 재고, 게임 clock | 현재 game session/save | Command와 tick으로 변경 |
| 학습 원장 `Simulation Ledger` | 선택, 계산 결과, 사용 근거, 회고 | session event log와 완료 snapshot | append 후 완료 시 봉인 |
| 표현 상태 `Presentation State` | 선택 tile, 열린 panel, animation | scene과 ViewModel | 언제든 재생성 가능 |

현실 관측을 game balance에 맞게 수정하지 않는다. 게임 난이도를 조절해야 할 때는 별도 rule parameter를 적용하고 관측값, 보정값과 결과를 함께 남긴다.

### 6.2 stable ID와 명칭

표시 이름을 관계 key로 사용하지 않는다. 모든 관계는 변경되지 않는 stable ID로 연결한다.

```text
crop:kr:potato
crop-variety:kr:potato:sumi
region:kr:41
market-stage:wholesale
source:kamis:daily-price
unit:mass:kg
scenario:potato-basic-kr-001
ruleset:farming-basic:1.0.0
```

- stable ID에는 화면 언어와 가격 같은 가변 값을 넣지 않는다.
- 한국어·영어 표시명은 locale별 label catalog에서 읽는다.
- KAMIS 품목 code, 통계청 분류 code와 게임 작물 key는 동일하다고 가정하지 않는다.
- 외부 code 연결은 `ExternalCodeMapping`으로 분리하고 mapping version, 품질, 검토 상태와 근거를 가진다.
- 하나의 외부 품목이 여러 게임 작물 후보와 연결되면 임의로 하나를 고르지 않고 `Ambiguous`로 보류한다.

### 6.3 네 개의 시간축

현실 시간과 게임 시간을 하나의 `DateTime`으로 합치지 않는다.

| 시간 | 의미 | 예시 |
| --- | --- | --- |
| `ObservedAt` 또는 `SurveyDate` | 현실에서 값이 관측된 시각·기간 | KAMIS 조사일 |
| `IngestedAt` | 살뜰 서버가 값을 수집·저장한 시각 | API 수집 완료 시각 |
| `ScenarioEffectiveAt` | 시나리오가 현실 snapshot을 기준으로 삼는 시각 | 2026-07 데이터 기준 |
| `GameTime` | 게임 안에서 흐르는 가상 시각 | 봄 8일 06:00 |

파생 계산에는 `CalculatedAtGameTime`과 사용한 관측 기간을 함께 남긴다. 게임 날짜가 바뀐다고 현실 API를 매 tick 다시 호출하지 않으며, 한 시즌 중에는 선택한 scenario snapshot을 고정한다.

### 6.4 단위와 수치 정밀도

- 금액은 `float`를 사용하지 않는다. 통화 code와 최소 화폐 단위의 정수 또는 `decimal`을 사용한다.
- 생산량·무게·면적·연료량은 값과 unit code를 한 쌍으로 보관한다.
- 계산 내부의 기준 단위를 정하되 원 관측 단위와 원 값을 지우지 않는다.
- `개`, `상자`, `망`을 kg으로 바꿀 때는 품목·포장별 명시적 conversion rule과 version이 있어야 한다.
- 환율, 물가 보정, 수분 감모와 폐기율은 서로 다른 rule이며 한 `Multiplier`에 합치지 않는다.
- 반올림은 화면이 아니라 계산 경계에서 정책명과 자릿수를 정하고 중간 단계 원 값을 보존한다.

초기 권장 기준 단위는 다음과 같다.

| 의미 | 내부 기준 | 함께 보존할 값 |
| --- | --- | --- |
| 금액 | `KRW` 최소 단위 정수 | 원 통화, 환율과 기준일 |
| 질량 | `kg` decimal | 원 값과 원 단위 |
| 면적 | `m2` decimal | 평·ha 등 입력 단위 |
| 온도 | `degC` decimal | 관측 장비·시각 |
| 강수량 | `mm` decimal | 관측 기간 |
| 비율 | 0~1 decimal | 화면용 백분율은 파생 |

### 6.5 공통 provenance envelope

모든 외부 관측과 중요한 파생값은 provenance를 선택 필드가 아니라 필수 envelope로 가진다.

```text
EvidenceId
SourceKey / SourceRecordId
DatasetKey / DatasetVersion
ObservedAt / IngestedAt
RegionKey / MarketStageKey
OriginalValue / OriginalUnit / CurrencyCode
NormalizedValue / NormalizedUnit
QualityCode / FreshnessCode
LicenseOrTermsReference
Limitations[]
PayloadHash
```

파생값에는 추가로 다음 lineage를 기록한다.

```text
DerivedValueId
InputEvidenceIds[]
RuleSetKey / RuleSetVersion
RuleParametersHash
RandomSeed
CalculatedAtGameTime
ResultValue / ResultUnit
ExplanationCodes[]
```

이 구조를 통해 “왜 이 수익이 나왔는가”를 입력 관측과 규칙까지 역추적할 수 있어야 한다.

### 6.6 불변 scenario package

플레이 도중 live 데이터가 갱신되면 같은 행동의 결과가 달라질 수 있다. 따라서 게임 시작 시 필요한 데이터를 하나의 immutable package로 고정한다.

```text
ScenarioManifest
  ScenarioKey / ScenarioVersion
  SchemaVersion
  RuleSetReferences[]
  CatalogVersions[]
  ObservationSnapshotIds[]
  DefaultRandomSeed
  ExpectedFileHashes[]
  CreatedAt / EffectiveAt
  Mode = SIMULATED
```

- 새 관측이나 balance 수정은 기존 package를 덮어쓰지 않고 새 version을 만든다.
- save file은 package 전체를 복사하지 않고 manifest key/version, input hash와 runtime event를 기록한다.
- 로드할 때 동일 package를 찾지 못하면 자동으로 최신 version으로 올리지 않고 migration 가능 여부를 판정한다.
- 교육용 fixture도 실제 snapshot과 같은 schema를 사용하되 `Fixture` source type을 반드시 가진다.

### 6.7 결정적 시뮬레이션과 Event 기록

그래픽 없이도 동일한 scenario, rule version, 사용자 Command와 random seed로 같은 결과가 나와야 한다.

```text
SimulationState(n)
  + PlayerCommand(n+1)
  + EnvironmentTick(n+1)
  + RuleSetVersion
  + RandomSeed
  → SimulationState(n+1)
  + DomainEvents[]
  + DerivedEvidence[]
```

- 성장, 병충해, 품질과 가상 수요의 난수는 전역 `UnityEngine.Random`에 의존하지 않는다.
- 난수 stream을 농장·날씨·시장 등 용도별 key로 분리해 UI animation의 난수가 결과에 영향을 주지 않게 한다.
- 사용자의 행동은 `PlantCrop`, `WaterTile`, `HarvestCrop`, `ChooseSalesChannel` 같은 Command로 기록한다.
- 결과는 `CropGrowthAdvanced`, `HarvestCompleted`, `CostAccrued`, `SalesCompared` Event로 설명한다.
- autosave는 현재 상태 snapshot과 마지막 적용 Event sequence를 함께 기록한다.

Event 기록은 운영 Event/Outbox가 아니라 로컬 학습 재현용이다. 향후 서버 저장을 검토하더라도 별도 contract와 동의 없이 운영 Event bus에 발행하지 않는다.

### 6.8 데이터 품질과 결측 처리

`0`, 빈 문자열과 “자료 없음”을 같은 값으로 취급하지 않는다.

| 상태 | 의미 | 기본 처리 |
| --- | --- | --- |
| `Valid` | 현재 scenario 조건에 사용 가능 | 계산 허용 |
| `Stale` | freshness 기준 초과 | 경고 후 정책에 따라 허용 |
| `Missing` | source에 값 없음 | 계산 중단 또는 명시적 대안 선택 |
| `NotApplicable` | 해당 품목·지역에 적용되지 않음 | 계산 대상에서 제외 |
| `IncompatibleUnit` | 안전한 단위 변환 불가 | 비교 금지 |
| `AmbiguousMapping` | 외부 code와 game key 연결 불확실 | 사람 검토 전 사용 금지 |
| `Rejected` | 범위·품질·보안 검증 실패 | 격리하고 미사용 |
| `Fixture` | 학습용 고정 자료 | `SIMULATED FIXTURE` 표시 후 허용 |

결측값을 평균이나 0으로 자동 대체하지 않는다. 교육 목적상 보간값이 필요하면 `Estimated` 파생값으로 만들고 입력, 방법, 신뢰 범위와 rule version을 표시한다.

### 6.9 DataManager 내부 경계

`DataManager`가 모든 데이터 책임을 가진 거대한 singleton이 되지 않게 내부 port를 먼저 나눈다.

```text
DataManager
  ├─ IScenarioPackageRepository
  ├─ IPublicObservationRepository
  ├─ IGameCatalogRepository
  ├─ IRuleSetRepository
  ├─ ISimulationSaveRepository
  └─ IDataStatusProvider
```

초기에는 한 class가 여러 port를 구현해도 되지만 소비 코드는 구체 class가 아니라 좁은 interface를 사용한다. `FarmManager`는 가격 API를 직접 호출하지 않고 필요한 crop rule과 환경 snapshot만 요청한다.

데이터 로딩 상태는 최소한 다음 상태 기계로 표현한다.

```text
NotLoaded → Loading → ReadyLive | ReadyCached | ReadyFixture
                    ↘ Invalid | Failed
```

`ReadyLive`, `ReadyCached`, `ReadyFixture`를 모두 단순 `Ready=true`로 축약하지 않는다.

### 6.10 저장, cache와 migration

- public snapshot cache와 player save를 다른 경로와 보존 정책으로 관리한다.
- cache key는 endpoint URL만이 아니라 dataset key/version, query 조건과 schema version을 포함한다.
- cache entry에는 fetched time, ETag 또는 payload hash, expiration과 provenance를 둔다.
- save file에는 개인정보, API token, 서버 원장 원문을 저장하지 않는다.
- schema migration은 원본 save의 backup 또는 복구 가능한 복사본을 만든 뒤 수행한다.
- 지원할 수 없는 새 major schema는 부분 로드로 성공을 가장하지 않고 명확히 차단한다.
- 개발용 reset은 player save와 public cache를 따로 지울 수 있어야 한다.

## 7. 첫 시제품 데이터 청사진

첫 시제품은 GameObject를 만들기 전에 다음 dataset을 완성한다.

| dataset | 최소 record | 핵심 관계 |
| --- | --- | --- |
| `CropCatalog` | 작물 1종, 품종 1종 | crop → growth profile |
| `GrowthStageCatalog` | 4단계 | stage → 필요 누적 성장량 |
| `CultivationRuleSet` | 온도·수분·성장·수확 rule | rule → crop/stage |
| `WeatherSnapshot` | 7~14 game day | day → temperature/rainfall |
| `SoilCatalog` | 토양 1종 | soil → 보수력·배수 |
| `InputCostCatalog` | 종자·노동·수확 비용 | cost item → 발생 event |
| `TransportRuleSet` | 거리·중량 기반 가상 운송비 | sales channel → cost rule |
| `MarketObservationSnapshot` | KAMIS 관측 1건 이상 | external item mapping → crop |
| `SalesChannelRuleSet` | 일반·공동판매 각 1개 | channel → 가격·노동·위험 rule |
| `ScenarioManifest` | 시나리오 1개 | 위 version과 hash 고정 |
| `GoldenSimulationCase` | 표준 Command sequence 1개 | 입력 → 기대 Event·최종 수익 |

### 7.1 데이터 사전

각 field에는 다음 항목을 기록한 data dictionary가 있어야 한다.

- 업무 의미와 교육 목적
- data type, nullable 여부와 허용 범위
- stable ID 또는 참조 대상
- 원 단위와 내부 기준 단위
- source와 갱신 책임
- 현실 관측, 게임 규칙, runtime 상태 또는 파생값 중 분류
- 개인정보·공개 범위
- 결측·오류 처리
- schema version에서 추가·변경·폐기된 이력

같은 이름의 `Price`, `Date`, `Status`를 문맥 없이 만들지 않는다. 예를 들어 가격은 `ObservedWholesalePrice`, `SimulatedExpectedSalePrice`, `ActualRevenue`처럼 시장 단계와 값의 성격을 이름에 드러낸다.

### 7.2 파일과 authoring 형식

| 용도 | 권장 형식 | 기준 |
| --- | --- | --- |
| API contract sample | UTF-8 JSON | server 응답 호환성의 golden fixture |
| scenario manifest | UTF-8 JSON | diff·hash·CI 검증 가능 |
| 대량 catalog 원본 | CSV 또는 JSON | schema와 stable ID 검증 후 import |
| Unity editor authoring | ScriptableObject | 사람이 편집하는 rule·catalog adapter |
| runtime model | plain C# object | Unity scene lifecycle과 분리 |
| player save | versioned JSON 또는 binary envelope | schema, manifest, sequence, checksum 포함 |

ScriptableObject는 편집 편의를 위한 asset이지 공공데이터 원본이나 runtime 진행 상태의 권위가 아니다. Scene과 Prefab에는 stable ID와 표현 설정만 두고 농업·시장 원본 record를 복제하지 않는다.

### 7.3 데이터 준비 완료 gate

다음 조건을 충족하기 전에는 FarmTile의 본 구현과 그래픽 에셋 구매를 시작하지 않는다.

- 첫 시나리오의 모든 stable ID와 관계가 유효하다.
- 필수 field, 범위, enum, 단위와 참조 무결성 검증이 통과한다.
- KAMIS 원 관측과 게임용 정규화값을 함께 추적할 수 있다.
- scenario manifest의 모든 version과 hash가 재현된다.
- golden Command sequence가 headless 계산에서 기대 결과를 만든다.
- 결측, stale, 단위 불일치와 mapping 모호성 test가 통과한다.
- 데이터 사전만 읽고도 각 값의 소유자와 변경 방법을 알 수 있다.

## 8. Unity 런타임 구조

### 8.1 최소 Manager

초기에는 전역 Manager를 다음 세 개로 제한한다.

| Manager | 책임 |
| --- | --- |
| `GameManager` | 게임 clock, 시즌, pause, 시나리오 시작·종료 |
| `DataManager` | API 조회, cache, fixture, Mapper, provenance와 데이터 상태 제공 |
| `FarmManager` | 농장 tile 상태, 심기·물주기·성장 tick·수확 조율 |

`WeatherManager`, `MarketManager`, `InventoryManager`, `UIManager`는 책임이 실제로 커질 때 분리한다. 처음부터 모든 영역을 singleton Manager로 만들지 않는다.

### 8.2 GameObject의 책임

| GameObject | 보유해도 되는 상태 | DataManager에서 읽는 값 |
| --- | --- | --- |
| `FarmTile` | tile ID, 수분 상태, 현재 crop instance ID | 토양 규칙, 날씨 영향 |
| `Crop` | 작물 key, 심은 날, 성장 단계, 건강 상태 | 작물 정의, 성장 규칙 |
| `Player` | 현재 위치, 선택 도구, 입력 상태 | 소유 자원과 행동 가능 조건 |
| `NPC` | 시나리오 역할 key, 현재 대화 단계 | 가상 참여자 정의와 학습 대화 |
| `Market` | 표시 중인 시장 key | 관측가격, 시장 단계, 출처와 시각 |

가격 원본, 전체 작물 catalog, 공공데이터 응답, 원장 원문은 GameObject에 복제하지 않는다.

## 9. 첫 번째 세로 기능 슬라이스

첫 구현 목표는 **한 작물·한 시즌·두 판매 방식 비교**다.

### 9.1 사용자 여정

```text
작물 한 종 선택
  → 밭 한 칸에 심기
  → 날씨와 물 상태를 보며 4단계 성장
  → 수확량과 생산비 확인
  → 서버가 제공한 KAMIS 관측가격의 출처·단위·조사일 확인
  → 일반판매 또는 공동판매 시뮬레이션 선택
  → 매출·비용·노동·위험·역할 분담 비교
  → 학습 원장으로 한 시즌의 선택과 근거 회고
```

### 9.2 최소 데이터

- 작물 1종과 성장 단계 `Seed`, `Sprout`, `Growing`, `Harvestable`
- 게임 시간 7~14일 분량의 고정 날씨 fixture
- 토양 수분과 물주기 규칙
- 종자비, 일별 노동비, 수확비, 운송비
- 저장된 KAMIS 관측 1종 또는 동일 schema의 명시적 fixture
- 일반판매와 공동판매의 규칙 기반 계산
- `SIMULATED` label, source, 조사일, 단위, 시장 단계, rule version

### 9.3 학습 원장

첫 버전의 원장은 Unity 로컬의 `SimulationRunRecord`다. 실제 `커뮤니티원장Dto`를 복사하거나 서버 원장 저장 API를 호출하지 않는다.

```text
RunId
ScenarioKey / ScenarioVersion
CropKey
StartedAtGameTime / CompletedAtGameTime
Decisions[]
PublicDataEvidence[]
CostBreakdown
SalesComparison
LearningSummary
Mode = SIMULATED
```

공동판매 결과에는 예상 가격만 보여 주지 않고 참여 인원, 공동 선별·포장 노동, 목표 미달 위험, 운송 분담과 미정 조건을 함께 표시한다.

## 10. API 제안

첫 단계에는 운영 상태를 바꾸지 않는 좁은 bootstrap API 하나를 우선 검토한다.

```http
GET /api/v1/simulation/agriculture/scenarios/{scenarioKey}/bootstrap
```

응답은 게임 전체 database가 아니라 해당 시나리오에 필요한 공개 snapshot만 제공한다.

- scenario key와 version
- 작물·지역의 stable key
- 공개 가격 관측과 provenance
- 데이터 freshness와 limitation
- 서버가 지원하는 schema version
- fixture 여부와 execution mode

작물 성장 공식, 난이도 보정과 가상 참여자 행동은 초기에는 Unity versioned rule asset으로 관리한다. 공공데이터와 게임 밸런스 rule을 서버 응답 하나에 섞지 않는다.

API가 실패하면 다음 중 하나만 허용한다.

1. 마지막 성공 snapshot을 시각과 함께 `CACHED`로 표시해 사용
2. 게임에 포함된 fixture를 `SIMULATED FIXTURE`로 표시해 사용
3. 데이터가 필요한 시나리오 시작을 중단하고 재시도 제공

운영 조회 실패를 이름 없는 sample 값으로 대체하지 않는다.

## 11. 권장 project 구조

Unity project의 실제 저장 위치와 repository 분리는 첫 착수 시 결정한다. 같은 repository에 둔다면 다음과 같이 서버 project와 물리적으로 분리하는 안을 권장한다.

```text
Ssalddel.Unity/
  Assets/SsalddelGame/
    Runtime/
      ApiModels/
      Mapping/
      GameModels/
      Data/
        Catalogs/
        Observations/
        Provenance/
        Repositories/
        Validation/
      Farming/
      Simulation/
      Presentation/
    DataSchemas/
    ScenarioPackages/
    ScriptableObjects/
      Crops/
      Rules/
      Scenarios/
    Tests/
      EditMode/
      PlayMode/
  Packages/
  ProjectSettings/
```

Unity 전용 assembly definition을 계층별로 두고 `Ssalddel.Contracts`와 서버 project reference는 추가하지 않는다. API schema 호환성은 저장된 JSON contract fixture와 양쪽 test로 확인한다.

## 12. 단계별 구현 계획

| 단계 | 결과물 | 완료 기준 |
| --- | --- | --- |
| 0. 결정 | Unity version, render pipeline, 지원 platform, repo 위치, 첫 작물 확정 | Architecture Decision Record 승인 |
| 1. 데이터 계약 | stable ID, data dictionary, schema, provenance, 단위 정책 | schema·참조·단위 validator 통과 |
| 2. 재현 기반 | scenario manifest, rule package, golden dataset·Command sequence | headless 결과와 hash 재현 |
| 3. 데이터 접근 | ApiModels, Mapper, repository port, cache, fixture | live·cached·fixture·invalid test 통과 |
| 4. 농장 핵심 | `GameManager`, `DataManager`, `FarmManager`, `FarmTile` | 심기부터 수확까지 headless test 가능 |
| 5. 플레이 | Player 이동·선택·물주기, 성장 feedback | PlayMode에서 한 시즌 완료 |
| 6. 경영 | 생산비, 재고, 일반판매 계산 | 비용 합계와 수익식 회귀 test 통과 |
| 7. 협력 학습 | 공동판매 비교, 가상 역할, 학습 원장 | 실제 참여·주문 없이 비교·회고 완료 |
| 8. 서버 연결 | 읽기 전용 bootstrap API와 cache | live·cached·fixture 상태가 구분됨 |
| 9. UI | 데이터 근거 panel, 원가표, 판매 비교, 접근성 | 출처·단위·시각·제한이 화면에 보임 |
| 10. 그래픽 | 검증된 Asset Store 자산 적용 | 시스템 동작을 유지하며 scene 완성 |
| 11. 확장 | 날씨·토양·제조·물류·통계 시나리오 | 영역별 독립 rule·test·source gate 보유 |

초기 구현 순서는 `데이터 구조 → Mapper → DataManager → FarmTile → Player → 성장 → 판매 → 서버 공공데이터 → UI → 그래픽`을 유지한다.

## 13. 검증 전략

### 13.1 데이터·schema test

- stable ID 중복, 끊어진 참조와 금지된 순환 참조 검사
- data dictionary와 실제 schema의 field 일치
- scenario manifest의 version, 파일 hash와 필수 dataset 검사
- master data, observation, rule, runtime state 사이의 금지 field 검사
- 같은 external code의 ambiguous mapping과 mapping version 검사
- golden dataset을 이용한 이전 rule version 결과 회귀

### 13.2 Unity EditMode test

- API JSON의 schema version별 역직렬화와 Mapper 결과
- 필수 출처·기준 시각·단위 누락 시 거부
- kg, g, 개수 단위 환산과 호환 불가 단위 차단
- 작물 성장, 수분, 날씨 영향의 결정적 계산
- 생산비·운송비·매출·수익 합계
- 일반판매와 공동판매 비교의 rule version 재현성
- 같은 seed와 Command sequence의 Event·최종 state hash 일치
- 금액 계산 경로에서 binary floating-point 미사용

### 13.3 Unity PlayMode test

- 심기부터 수확·판매·회고까지 한 시즌 완주
- scene reload 뒤 허용된 로컬 진행만 복구
- API 실패 시 cached/fixture/error 표시
- GameObject 파괴·재생성 뒤 DataManager 원본 상태 유지

### 13.4 서버·계약 test

- Unity bootstrap API가 공개 허용 필드만 반환
- source, observedAt, unit, currency, region, marketStage, limitation 보존
- 내부 수집 정보, credential, 개인 원장, 연락처, 정밀 위치 미노출
- 같은 scenario/version 응답의 호환성과 cache 정책
- Unity contract fixture를 이용한 소비자 호환성 test

### 13.5 교육 효과 확인

게임 성공 여부뿐 아니라 사용자가 다음을 구분할 수 있는지 확인한다.

- 관측가격과 실제 판매 확정가격
- 매출과 순수익
- 개인판매와 공동판매의 비용·노동·위험
- 공개데이터와 공급·재고·계약 사실
- 게임 원장과 실제 살뜰 원장

## 14. 그래픽과 외부 자산 전략

농장 환경, 2.5D 배경, 캐릭터, 애니메이션, UI와 이펙트는 Asset Store 활용을 우선한다. 구매 전에는 다음을 확인한다.

- 선택한 Unity/LTS version과 render pipeline 호환성
- desktop·mobile 목표 platform 지원
- 라이선스와 팀·배포 범위
- source file, prefab, animation controller의 수정 가능성
- 성능 예산, shader 의존성, input system과 UI framework 충돌
- 장기 업데이트 여부와 vendor 종속 위험

에셋 구매와 그래픽 개선은 첫 세로 슬라이스의 데이터·계산·테스트가 완성된 뒤 진행한다. 임시 primitive와 placeholder UI로도 전체 학습 loop를 먼저 검증한다.

## 15. 범위에서 제외하는 항목

초기 제안은 다음을 구현 완료로 간주하지 않는다.

- 실제 농가·공급자·구매자의 가용성, 재고, 계약 의사 표시
- 실제 주문, 결제, 계약, 발주, 정산, 운송 의뢰와 자동 배차
- 게임 결과의 실제 관심·참여·공동 원장 자동 등록
- 기상·가격 예측을 확정 사실처럼 제공하는 기능
- 모든 작물·지역·공공기관 데이터를 한 번에 수집하는 작업
- Asset Store 구매, 유료 provider 호출, credential 추가
- Unity project 생성 자체만으로 교육 효과나 서버 연동 완료를 선언하는 일

## 16. 착수 전 결정할 사항

구현 전 다음 네 가지를 짧은 ADR로 확정한다.

1. Unity LTS version과 2.5D render pipeline
2. 1차 지원 platform: Windows 우선 또는 Windows·Android 동시
3. Unity project를 현재 monorepo에 둘지 별도 repository로 둘지
4. 첫 작물, 첫 지역, 사용할 KAMIS 관측 종류와 고정 fixture 기간

## 17. 1차 완료 정의

다음 조건을 모두 만족할 때 첫 시제품을 완료로 본다.

- 사용자가 작물 하나를 심고 관리해 수확한다.
- 모든 성장·수익 계산은 같은 입력과 rule version에서 재현된다.
- 첫 scenario package, data dictionary, golden dataset과 Command sequence가 version 관리된다.
- 같은 package, seed와 Command sequence가 같은 최종 state hash를 만든다.
- KAMIS 가격의 출처, 조사일, 지역, 시장 단계, 단위와 제한을 볼 수 있다.
- 관측가격과 게임 보정값이 분리되어 설명된다.
- 일반판매와 공동판매의 매출뿐 아니라 비용, 노동, 위험과 역할이 비교된다.
- 결과는 `SIMULATED` 학습 원장에만 남고 실제 주문·참여·원장을 만들지 않는다.
- server unavailable, cached, fixture 상태가 사용자와 test에서 구분된다.
- EditMode·PlayMode·서버 계약 test가 통과한다.
- 실제 플레이 화면과 최종 실행 환경을 별도로 검증한다.

이 첫 슬라이스가 닫힌 뒤에만 작물·지역 확대, 실시간 날씨, 제조, 물류와 살뜰 계정 연결을 다음 단계로 연다.
