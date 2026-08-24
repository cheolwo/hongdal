# 코드 탐색 메타데이터

## 목적

`SsalddelCodeMetadataAttribute`는 기능 하나가 계약, 화면, API, Application, 저장소와 외부 adapter를 어떻게 통과하는지 소스에서 바로 찾기 위한 코드 지도다. [API 업무 의미 분류](ApiBusinessClassification.md)의 업무 영역·사용자·업무 동작·Workflow를 대체하지 않고, 개별 구현 타입의 책임과 부수효과를 더 자세히 설명한다. 제품 버전은 현재 기능 분류가 아니라 최초 도입 이력으로만 해석한다.

## 필드

| 필드 | 의미 |
| --- | --- |
| `FeatureKey` | 같은 세로 기능 흐름을 묶는 안정적인 검색 키 |
| `Layer` | `Contract`, `Api`, `Application`, `Domain`, adapter, `ViewModel`, `View` 중 실제 책임 계층 |
| `Responsibility` | 해당 타입이 맡는 한 가지 주 책임 |
| `ContractType` | 구현이 따르는 주 interface 또는 ViewModel 타입 |
| `StepKey` | 기능 안에서 한 단계를 가리키는 안정적인 식별자 |
| `DependsOnStepKeys` | 코드 지도를 따라갈 때 바로 앞에서 읽을 단계 식별자 |
| `FlowOrder` | 사용자의 입력에서 외부 경계까지 탐색할 때의 대략적인 순서 |
| `ExecutionStage` | 정의, 조회, 미리보기, 확인, Tick, 투영, 저장, 표현 중 현재 단계 |
| `ReadsFrom`, `WritesTo` | 운영 상태, 공유 공공데이터, Simulation 상태, 파생 World, 클라이언트 표현 중 데이터 권위 경계 |
| `Effects` | 네트워크, 외부 API, 영속화, object storage, 일반·UI 상태 변경, 비용 발생 가능성 |
| `Boundary` | 호출 전에 알아야 할 권한·비용·개인정보·증빙·상태 변경 경계 |

`Effects`는 직접 작성한 코드 한 줄만이 아니라 그 타입의 공개 동작이 하위 서비스에 위임해 발생시키는 효과까지 표시한다. 예를 들어 API Controller가 Application service에 생성을 위임하더라도 외부 비용 가능성을 숨기지 않는다.

## 적용 기준

- 새 기능은 여러 프로젝트를 통과해 찾아야 할 때만 하나의 `FeatureKey`를 만든다.
- 하나의 타입이 여러 기능을 실제로 조율하면 특성을 여러 개 붙일 수 있지만, 단순 참조만으로 기능 소유권을 표시하지 않는다.
- `Responsibility`에는 구현 방법을 나열하지 말고 타입이 소유한 결과를 한 문장으로 적는다.
- 외부 호출, 영속 상태 변경, 비용, 개인정보 전송, 법적 증빙 오인 가능성은 `Effects`와 `Boundary`에 명시한다.
- 순수 계산기는 `Effects = None`으로 두고 순수성 경계를 적는다.
- `ReadsFrom/WritesTo`는 데이터 권위를 나타낸다. Simulation·Unity는 `OperationalState`를 쓰지 않고, 공유 공공데이터는 읽기 전용이며, Unity의 쓰기는 `ClientPresentation`으로 제한한다.
- `StepKey`는 같은 기능 안에서 중복하지 않고 의존 단계보다 큰 `FlowOrder`를 사용한다. 여러 병렬 단계는 같은 `FlowOrder`를 사용할 수 있다.
- 기능을 분리하거나 이름을 바꾸면 특성, reader 검증 테스트와 관련 아키텍처 문서를 함께 갱신한다.

## E 성숙도 책임 메타데이터

`SsalddelEvidenceResponsibilityAttribute`는 게임 코드가 E1~E9 중 어느
성숙도 질문에 주로 답해야 하는지를 표시한다. 이것은
`SsalddelCodeMetadataAttribute`의 기능 흐름을 대체하거나 확장한 필드가 아니다.
기능 흐름 메타데이터는 "이 호출이 어디를 지나가는가"를, E 책임 메타데이터는
"이 구성 요소를 어느 E 검토에서 다시 읽어야 하는가"를 답한다.

가장 중요한 경계는 다음과 같다.

- Attribute는 **책임 참여**를 뜻하며 현재 E 증거 통과나 승격을 뜻하지 않는다.
- 검사 후보 타입은 대표 `Primary` 책임을 정확히 하나 가진다. 실제로 두 번째
  단계의 책임도 수행할 때만 `Secondary`를 추가한다.
- `Boundary`에는 그 타입만으로 완료를 주장할 수 없는 경계를 반드시 적는다.
- WI의 실제 `Preview`·`Confirm`처럼 단계 판정에 중요한 공개 메서드만 선택해
  메서드 Attribute를 붙인다. 메서드도 대표 책임은 하나다.
- `WorldInteractionIds`는 기존 `WI-*` 고유 식별자를 참조할 뿐 WI 정의를 새로
  소유하지 않는다. H1~H5 역시 Attribute 문자열로 새 계층을 만들지 않는다.
- 호환 facade, 기술 보조 타입, 생성·외부 코드, Sample·Experiment처럼 검사에서
  빠져야 하는 공개 후보는 `SsalddelEvidenceCoverageExclusionAttribute`와 구체적인
  사유를 남긴다. 사유 없는 누락은 허용하지 않는다.

### E1~E3 사람용 하위 모듈

E1·E2·E3은 적용 범위가 넓으므로 `SubmoduleKey`로 한 번 더 나눠 탐색한다.
이 값은 새 E 단계나 물리 프로젝트를 만드는 분류가 아니다. 대표 E 책임 안에서
구성 요소가 맡는 역할을 사람이 바로 알아볼 수 있게 하는 안정 key다.

| E | 사람용 하위 모듈 |
| --- | --- |
| E1 핵심 계약 | `E1세션권위계약Module`, `E1세계상호작용계약Module`, `E1공간계약Module`, `E1저장재생계약Module`, `E1전투위협계약Module` |
| E2 실행 경계 | `E2세션실행Module`, `E2세계상호작용실행Module`, `E2로컬권위AdapterModule`, `E2원격HostAdapterModule`, `E2Unity권위ClientModule` |
| E3 회귀 증거 | `E3계약회귀Module`, `E3결정성검증Module`, `E3저장재생검증Module`, `E3로컬원격동등성Module`, `E3Unity소비자회귀Module` |

정의의 기계 기준은 `SsalddelEvidenceSubmoduleDefinitionCatalog`이며 현재 결속
대상과 개수는 자동 생성되는
[E 책임 코드 지도](../AI/generated/evidence-responsibility-code-map.md#e1e3-사람용-하위-모듈)에서 확인한다.
Attribute에는 다음처럼 안정 key를 지정한다.

```csharp
[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E2,
    "Farm·Nature WI Preview·Confirm 실행 포트를 제공한다.",
    Boundary = "플레이 완료나 E2 증거 승격을 단독으로 주장하지 않는다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
```

- `SubmoduleKey`의 E 번호는 Attribute의 대표 E와 같아야 한다.
- 새로 만들거나 의미 있게 수정하는 E1~E3 핵심 구성 요소는 정확한 하위 모듈이
  있을 때 지정한다.
- 기존 구성 요소는 기능 단위로 옮긴다. 뜻이 불분명한 코드를 개수 맞추기 위해
  억지로 분류하지 않으며 미지정 개수를 생성 지도에 드러낸다.
- 하위 모듈 결속과 코드 지도 통과는 책임 탐색 증거일 뿐 E1~E3 완료나 상위 E
  승격 증거가 아니다.

E 단계·G 관리 체계·모듈 이름의 단일 기계 기준은
`eng/execution-ledgers/e9-refactor-module-catalog.json`이다. 공통 코드의
`SsalddelEvidenceStageDefinitionCatalog`와 생성 도구는 이 대장과 일치하는지
검사한다. E 단계 자체의 완료 질문과 반복 왕복 절차는
[E9↔E1 반복 왕복 구현 체계](E9하향식수직구현체계.md)가 소유한다.

현재 강제 범위는 게임 경계인 Simulation 계약·Domain·Application·저장·Server,
공유 Unity adapter, 실제 Unity 제품 코드와 검증 코드다. 일반 Operations 코드는
게임 E 책임 전수 검사에 포함하지 않는다. Unity의 Sample·Experiment 어셈블리는
제품 후보가 아니지만 각 어셈블리의 제외 Attribute와 사유는 지도에 남긴다.

저장소 쪽 전수 지도는 다음 명령으로 갱신하고 검사한다.

```powershell
dotnet run --project eng/Ssalddel.EvidenceMap -- --write --strict
dotnet run --project eng/Ssalddel.EvidenceMap -- --strict
```

- 생성 결과는 `docs/AI/generated/evidence-responsibility-code-map.md`와 JSON이며,
  JSON schema `v2`부터 E1~E3 하위 모듈 정의·결속 수·미지정 수를 함께 기록한다.
- `eng/validate-changes.ps1`은 관련 게임 코드나 메타데이터가 바뀌면 strict 검사를
  실행하며 후보 타입의 무사유 누락을 오류로 처리한다.
- 실제 Unity 제품은 `Ssalddel/검증/E 책임 코드 지도 갱신` 메뉴와
  `UnityEvidenceResponsibilityMetadataTests`로 별도 전수 검사한다. 생성 결과는
  Unity 저장소의 `Documentation/Generated/`에 둔다.
- 코드 지도 통과는 컴파일·Attribute 정합 증거다. 저장 Scene, Play Mode,
  Game View, 실제 서버 또는 E 단계 승격 증거를 대신하지 않는다.

## 코드 명명 언어

이 절은 저장소 코드 명명 언어의 단일 기준이다. 기술 책임을 나타내는 용어는
영어로 유지하고 업무 의미는 한국어로 적는다.

| 구분 | 표기 | 예 |
| --- | --- | --- |
| 기술 책임 | 영어 | `Controller`, `API`, `DTO`, `Command`, `Event`, `Handler`, `UseCase`, `ApplicationService`, `ProcessManager`, `WorkflowCoordinator`, `Repository`, `Store`, `Client`, `Options`, `BackgroundService`, `Outbox` |
| 업무 개념 | 한국어 | `공동구매수요`, `생산자연결`, `운송의뢰`, `창고입고`, `재고관리`, `정산` |
| 외부 표준·고유명 | 원 표기 | `YouTube`, `HSK`, `HTSUS`, `JWT`, `OAuth`, `KAMIS` |

따라서 `DomesticGroupPurchaseNegotiationsController`보다
`국내공동구매협의Controller`, `PublicDataLookupController`보다
`공공데이터조회Controller`를 사용한다. 기술 역할을 번역한
`컨트롤러`, `서비스`, `이벤트처리기` 같은 접미사는 만들지 않는다.

이 기준은 class, method, property, field, parameter와 file 이름에 적용한다.
`국내공동구매협의Controller`, `생산자후보검색`,
`_공공데이터조회UseCase`처럼 `한국어 업무명 + 영어 기술 역할`로 조합한다.
새 코드와 수정하는 코드에는 이 기준을 적용한다. 기존 이름을 넓게 바꾸는 작업은
기능 단위로 나누고 호출부와 외부·영속 contract의 호환성을 함께 검증한다.

코드 이름 변경이 HTTP Route, query 이름, JSON 필드, Event code, DB 식별자 또는
원장에 저장된 API 식별자의 변경을 뜻하지는 않는다. 이미 노출된 API metadata 이름은
`SsalddelApiContractNameAttribute`로 보존하고 새 코드는 한국어 업무 이름을 사용한다.
attribute, mapping 또는 adapter로 기존 contract를 명시적으로 보존하고 회귀 test를 둔다.
외부 계약 자체를 바꿔야 할 때는 별도 migration과 호환 기간을 둔다.

### 일반 아키텍처 역할 이름

저장소 고유의 상위 개념을 기술 접미사로 만들지 않는다. `HIOPS`와 `OS`는
설계 이력이나 호환 식별자에는 남을 수 있지만 새 class, interface, field,
parameter 또는 file 이름의 기술 역할로 사용하지 않는다. 실제 책임에 따라
다음 이름을 선택한다.

| 실제 책임 | 기술 역할 |
| --- | --- |
| 여러 상태 전이와 장기 실행 업무를 조율 | `ProcessManager`, `Saga` |
| 여러 UseCase와 외부 adapter의 호출 순서를 조율 | `WorkflowCoordinator`, `Orchestrator` |
| 일정에 따라 작업을 시작 | `Scheduler`, `BackgroundService`, `Job` |
| 후보 계획·배분 | `Planner` |
| 후보 매칭·선택 | `Matcher`, `Selector`, `Strategy` |
| 수치 계산·추정 | `Calculator`, `Estimator` |
| 분류·판정 | `Classifier`, `Evaluator` |
| 외부 AI 호출 | `AiClient`, `AiService` |

`Engine`은 입력을 받아 영속 상태를 바꾸지 않고 계산 결과만 반환하는
순수 알고리즘 경계에서만 허용한다. DB 저장, 권한 확인, 상태 전이,
Event/Outbox 발행을 수행하는 타입은 `UseCase`, `ApplicationService`,
`ProcessManager` 중 실제 책임에 맞는 이름을 사용한다.

기존 route, JSON 필드, 설정 section, Event code와 저장 식별자에 `os` 또는
`engine`이 들어 있다면 호환 계약으로 분리해 유지할 수 있다. 내부 타입을 먼저
일반 용어로 바꾸고, 외부 계약 변경은 별도 버전과 호환 기간을 둔다.

## 탐색 방법

기능 키를 한 번 검색하면 관련 타입과 흐름 순서를 바로 확인할 수 있다.

```powershell
rg -n "SsalddelCodeFeatureKeys.CommunityAuthoringImage" Ssalddel.Contracts Ssalddel.Ui.Common SsalddelAdminApp Ssalddel
```

특성이 적용된 기능 전체를 찾을 때는 다음 명령을 쓴다.

```powershell
rg -n "SsalddelCodeMetadata\(" -g "*.cs" -g "*.razor"
```

런타임 또는 테스트에서는 `SsalddelCodeMetadataReader.ReadFeature`에 관련 assembly를 전달하면 `FlowOrder` 순서의 descriptor를 얻는다. 현재 기준 구현인 `community-authoring-image`는 `View -> ViewModel -> client port -> 관리자 HTTP adapter -> API -> 문맥 planner -> 생성 orchestration -> Gemini Nano Banana adapter`로 이어진다.

Simulation·Unity는 다음 명령으로 생성된 한국어 트리와 JSON을 함께 사용한다.

```powershell
dotnet run --project eng/Ssalddel.CodeMap -- --feature simulation-parallel-battle
dotnet run --project eng/Ssalddel.CodeMap -- --check
```

- `eng/work-areas/simulation-unity.json`은 읽을 문서·소스 범위·기능 관계·필수 단계를 정의한다.
- `docs/AI/generated/simulation-unity-code-map.md`는 사람이 읽는 트리, 같은 이름의 JSON은 기계 판독 자료다.
- 소스 특성과 manifest가 원본이며 생성 파일은 직접 수정하지 않는다. 필수 단계·권위 위반·오래된 생성 파일은 검증을 실패시키고, 아직 표기하지 않은 일반 공개 타입은 프로젝트별 경고 요약으로 남긴다.

## 경계

이 특성은 권한 검사, 트랜잭션, validation 또는 보안 통제를 대신하지 않는다. 코드가 실제로 수행하는 효과와 특성이 다르면 코드를 기준으로 즉시 특성을 고치고 테스트로 차이를 드러낸다. secret, 실제 개인정보, Prefab·Material의 원본 경로는 메타데이터 문자열에 기록하지 않는다.

## 제품 모듈 특성

`SsalddelModuleAttribute`는 여러 타입의 출시 묶음과 릴리즈 단계를 기록한다. 현재 업무 책임은 API 업무 의미 분류를 따르고, Module의 제품 버전은 출시 이력으로 사용한다. 커뮤니티 0.0에는 파생 특성인 `[SsalddelCommunityV0Module]`을 사용해 다음 값을 공통으로 고정한다.

- `ProductVersion`: `0.0`
- `FeatureFlag`: `CommunityTrustWorkflow`
- `WorkflowKey`: `CommunityTrust`
- `DefaultEnabled`: `true`

각 적용 지점은 `ModuleKey`, `Kind`, `ReleaseStage`, `Responsibility`, `Boundary`를 추가로 기록한다. `SsalddelModuleMetadataReader.ReadVersion`으로 관련 assembly를 조회하면 UI 조립부터 API, Application, 원장 영속화와 background 처리까지 모듈별로 확인할 수 있다. API 모듈은 기존 `[SsalddelApiVersion(V0_0)]`도 함께 유지하며 테스트에서 두 메타데이터의 일치를 검사한다.
