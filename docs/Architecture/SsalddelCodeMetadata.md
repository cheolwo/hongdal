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
| `FlowOrder` | 사용자의 입력에서 외부 경계까지 탐색할 때의 대략적인 순서 |
| `Effects` | 네트워크, 외부 API, 영속화, object storage, UI 상태 변경, 비용 발생 가능성 |
| `Boundary` | 호출 전에 알아야 할 권한·비용·개인정보·증빙·상태 변경 경계 |

`Effects`는 직접 작성한 코드 한 줄만이 아니라 그 타입의 공개 동작이 하위 서비스에 위임해 발생시키는 효과까지 표시한다. 예를 들어 API Controller가 Application service에 생성을 위임하더라도 외부 비용 가능성을 숨기지 않는다.

## 적용 기준

- 새 기능은 여러 프로젝트를 통과해 찾아야 할 때만 하나의 `FeatureKey`를 만든다.
- 하나의 타입이 여러 기능을 실제로 조율하면 특성을 여러 개 붙일 수 있지만, 단순 참조만으로 기능 소유권을 표시하지 않는다.
- `Responsibility`에는 구현 방법을 나열하지 말고 타입이 소유한 결과를 한 문장으로 적는다.
- 외부 호출, 영속 상태 변경, 비용, 개인정보 전송, 법적 증빙 오인 가능성은 `Effects`와 `Boundary`에 명시한다.
- 순수 계산기는 `Effects = None`으로 두고 순수성 경계를 적는다.
- 기능을 분리하거나 이름을 바꾸면 특성, reader 검증 테스트와 관련 아키텍처 문서를 함께 갱신한다.

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

## 경계

이 특성은 권한 검사, 트랜잭션, validation 또는 보안 통제를 대신하지 않는다. 코드가 실제로 수행하는 효과와 특성이 다르면 코드를 기준으로 즉시 특성을 고치고 테스트로 차이를 드러낸다. secret이나 실제 개인정보는 메타데이터 문자열에 기록하지 않는다.

## 제품 모듈 특성

`SsalddelModuleAttribute`는 여러 타입의 출시 묶음과 릴리즈 단계를 기록한다. 현재 업무 책임은 API 업무 의미 분류를 따르고, Module의 제품 버전은 출시 이력으로 사용한다. 커뮤니티 0.0에는 파생 특성인 `[SsalddelCommunityV0Module]`을 사용해 다음 값을 공통으로 고정한다.

- `ProductVersion`: `0.0`
- `FeatureFlag`: `CommunityTrustWorkflow`
- `WorkflowKey`: `CommunityTrust`
- `DefaultEnabled`: `true`

각 적용 지점은 `ModuleKey`, `Kind`, `ReleaseStage`, `Responsibility`, `Boundary`를 추가로 기록한다. `SsalddelModuleMetadataReader.ReadVersion`으로 관련 assembly를 조회하면 UI 조립부터 API, Application, 원장 영속화와 background 처리까지 모듈별로 확인할 수 있다. API 모듈은 기존 `[SsalddelApiVersion(V0_0)]`도 함께 유지하며 테스트에서 두 메타데이터의 일치를 검사한다.
