# Ssalddel Unity 클라이언트 계층 구조 설계

> 상태: 기준 설계와 현재 저장소 적용 상태
>
> 적용 대상: `Ssalddel.Unity` engine-independent package와 향후 Unity presentation project
>
> 상위 제품 기준: [Unity 생산·유통·협력 경험 플랫폼](UnityCooperativeExperiencePlatformProposal.md)
>
> World·원장 책임 기준: [Unity World·원장 투영 아키텍처](UnityWorldLedgerProjectionArchitectureProposal.md)
>
> 데이터·시뮬레이션 기준: [Unity 농업·유통 시뮬레이션](UnityAgricultureDistributionSimulationProposal.md)

## 1. 목적

이 문서는 서버 중심의 기존 데이터와 업무 도메인을 Unity 클라이언트로 확장할 때 사용하는 코드 계층, 의존성 방향, Prefab·Inspector 결합 경계와 자동화 검증 기준을 정의한다.

Ssalddel Unity의 제품 지위는 일반적인 독립 농장 게임이나 Web 화면의 3D 복제가 아니다.

> **연구자료와 실제 데이터를 서버에서 권위 있게 관리하고, Unity에서 그 상태와 관계를 공간·센서·업무 오브젝트로 체험하게 하는 World Projection Client**

Unity는 서버 응답을 GameObject에 직접 넣지 않는다. transport, mapping, repository, UseCase, presentation model, scene orchestration과 View를 단계적으로 통과시킨다. View는 외부 3D asset을 연결하는 명시적 socket 계약을 제공하며, 향후 Editor Script와 Unity CLI가 반복적인 Prefab 생성·배선·검사를 수행할 수 있게 한다.

## 2. 현재 저장소 적용 상태

2026-08-08 현재 이 체크아웃에서 확인된 상태다.

| 영역 | 상태 | 근거와 제한 |
| --- | --- | --- |
| engine-independent Unity package | 구현 | `Ssalddel.Unity`, `netstandard2.1`, C# 9 |
| UnityEngine 격리 | 구현 | `Ssalddel.Unity.Data.asmdef`의 `noEngineReferences=true` |
| ApiModel·Mapper·DataManager | 구현 | 서버 contract assembly를 직접 참조하지 않음 |
| 결정적 농업 simulation | 구현·headless 검증 | golden fixture와 .NET tests |
| World projection core | 구현 | page catalog, stable-ID/revision reconcile |
| 연구 근거·센서 계약 | 초기 구현 | engine-independent model과 validator |
| UnityWebRequest API Client | 현재 체크아웃에서 미확인 | 사용자가 P2 실행을 검증한 별도 Unity runtime 소스 위치를 먼저 확정해야 함 |
| VContainer | 채택·sample 적용 | 1.18.0, Zone `LifetimeScope`, method injection, Unity 6 Editor compile 검증 |
| UniTask·Unity Newtonsoft·Input System | 미선언 | 현재 core·sample 필수 의존성이 아님 |
| Unity presentation sample | 구현·Editor 검증 | Urban Market·Traditional Market Hub Scene 생성·reload wiring; PlayMode·built player 미검증 |
| Synty asset | 미도입 | 구매·license·실제 import 검증 없음 |

따라서 이 문서는 이미 확인된 P2 runtime을 다시 구현했다고 주장하지 않는다. 현재 core와 별도 Unity runtime을 결합할 기준을 제공한다.

## 3. 핵심 경계

### 3.1 서버

서버가 최종 권위를 갖는 값:

- 공개데이터 원본·정규화·출처·기준 시각·단위·지역·품질
- 사용자·조직 권한과 공개 범위
- 농장·주문·공동구매·운송·창고 원장과 revision
- 실제 참여, 주문, 배차, 검수, 입출고와 상태 전이
- Event·Outbox, 멱등성, 감사 기록

서버가 알지 않아야 하는 값:

- Synty Prefab 이름과 경로
- Animator state·parameter
- Material, Renderer, ParticleSystem과 AudioClip
- Unity Scene hierarchy와 Inspector reference
- client 전용 색상·motion·VFX 선택

### 3.2 Unity

Unity가 맡는 책임:

- API 조회와 인증 token 전달
- transport model의 역직렬화
- schema·단위·freshness·호환성 검사
- cache·마지막 성공 snapshot·fixture 상태 관리
- 사용자의 preview와 명시적 확인
- server-confirmed snapshot을 World object와 panel로 투영
- 입력, 카메라, 이동, animation, VFX, SFX와 접근성 feedback

Unity의 animation 완료나 GameObject 위치만으로 운영 상태를 확정하지 않는다.

```text
Unity interaction
  → preview
  → explicit confirmation
  → server Command
  → server authorization and revision validation
  → persistence and event
  → canonical snapshot re-query
  → Unity presentation update
```

## 4. 의존성 방향과 실행 흐름

의존성 방향과 runtime 요청 흐름을 구분한다.

### 4.1 코드 의존성 방향

```text
Unity Presentation
  ├─ SceneController / Input / View / Prefab adapters
  └─ Composition Root
          ↓
Unity Application
  ├─ UseCases
  ├─ Presenter
  └─ repository ports
          ↓
Ssalddel.Unity Core
  ├─ ApiModels
  ├─ Mapping
  ├─ GameModels / ProjectionModels
  ├─ Evidence / Sensors / Simulation
  └─ stable ID, revision and validation
```

Infrastructure adapter가 core의 repository port를 구현한다. core는 VContainer, UniTask, UnityWebRequest와 UnityEngine을 참조하지 않는다.

### 4.2 조회 실행 흐름

```text
SceneController
  → Load UseCase
  → Repository port
  → UnityWebRequest API Client
  → ApiModel
  → explicit Mapper
  → GameModel / ProjectionSnapshot
  → repository state store
  → UseCase result
  → Presenter
  → ScreenModel / PresentationCommand
  → View
  → Prefab VisualRoot
```

Repository가 mapping 정책을 직접 소유하지 않는다. Repository는 API Client, Mapper, cache와 state store를 조율하고, DTO→GameModel 호환성 판정은 독립 Mapper가 담당한다.

## 5. 권장 물리 구조

현재 `Ssalddel.Unity`는 engine-independent local UPM package로 유지한다.

```text
Ssalddel.Unity/
  Runtime/
    ApiModels/
    Mapping/
    Data/
    Simulation/
    WorldProjection/
    Evidence/
    Sensors/
    Interactions/
  DataSchemas/
  package.json
  Ssalddel.Unity.Data.asmdef
```

Unity project가 현재 monorepo 또는 별도 repository 중 어디에 위치할지는 ADR로 확정한다. 어느 쪽이든 presentation project는 다음 경계를 갖는다.

```text
Assets/Ssalddel/
  Runtime/
    Transport/
      ApiClients/
      Serialization/
      Authentication/
    Infrastructure/
      Repositories/
      Cache/
      Realtime/
    Application/
      UseCases/
      Presenters/
      ScreenModels/
    Presentation/
      SceneControllers/
      Views/
      Input/
      Camera/
      Animation/
      Effects/
    Composition/
      LifetimeScopes/
    Prefabs/
      Placeholders/
      Farm/
      Community/
      Logistics/
      Warehouse/
  Editor/
    PrefabAutomation/
    Validation/
  Tests/
    EditMode/
    PlayMode/
```

외부 asset은 별도 vendor 경계에 둔다.

```text
Assets/ThirdParty/Synty/
```

Ssalddel code가 vendor 내부 파일을 수정하거나 vendor component type을 core contract에 노출하지 않는다.

## 6. 계층별 책임

### 6.1 API Client

API Client는 HTTP transport만 담당한다.

- base URL과 route 구성
- method, header와 bearer token
- request timeout과 cancellation
- JSON deserialize
- HTTP status와 network 오류 분류
- correlation ID 같은 허용된 진단 정보 전달

API Client가 하지 않는 일:

- cache fallback 결정
- domain 상태 판정
- GameObject 생성
- 오류를 sample 성공으로 변환
- raw exception·token·개인정보 로그 출력

인터페이스 return type은 Unity 전용 adapter assembly가 선택한다. engine-independent core는 `Task`/`CancellationToken` 또는 결과 계약을 유지하고, Unity adapter에서 `Awaitable`이나 승인된 UniTask bridge를 사용할 수 있다.

### 6.2 ApiModel

ApiModel은 JSON wire contract를 표현한다.

- 서버 DTO source를 공유하지 않는다.
- nullable과 unknown code를 허용하고 Mapper가 호환성을 판정한다.
- JSON field name과 schema version을 고정한다.
- contract fixture로 server 응답과 소비 호환성을 검증한다.

ApiModel을 View, MonoBehaviour와 Prefab에 직접 전달하지 않는다.

### 6.3 Mapper

Mapper가 담당하는 변환:

- 필수 field와 schema version 확인
- wire code→client code 변환
- source·observedAt·unit·region·freshness 보존
- 단위 호환성과 명시적 normalization
- unknown·missing·stale·ambiguous 상태 반환
- server DTO와 Unity GameModel의 결합 차단

Mapper는 오류를 임의 기본값으로 숨기지 않는다.

```text
Mapped
UnsupportedSchema
MissingRequiredField
UnknownStateCode
IncompatibleUnit
Stale
InvalidStableId
```

### 6.4 Repository와 state store

Repository는 데이터 출처와 조회 정책을 숨긴다.

- API Client 호출
- Mapper 호출
- 마지막 성공 snapshot 저장
- cache와 fixture 조회
- refresh 중 기존 snapshot 유지
- stable ID/revision 기반 병합
- cancellation과 retry 가능 오류 전달

초기 로드와 refresh 실패를 구분한다.

| 전이 | visible data | 결과 |
| --- | --- | --- |
| `Idle → Loading → Success` | 새 snapshot | 정상 표시 |
| `Idle → Loading → InitialLoadError` | 없음 | empty/error와 retry |
| `Success → Refreshing → Success` | 기존 snapshot 유지 후 갱신 | 증분 반영 |
| `Success → Refreshing → RefreshError` | 마지막 성공 snapshot 유지 | stale/error badge와 retry |

중복 stable ID, 낮은 revision과 목적 범위를 벗어난 snapshot은 적용하지 않는다.

### 6.5 Unity GameModel·ProjectionModel

Unity 내부 model은 서버 domain Entity의 복사본이 아니다. Unity가 계산하고 표현하는 데 필요한 최소 immutable snapshot이다.

포함 가능한 값:

- stable ID와 revision
- 상태 code
- semantic location/zone/node ID
- source·unit·observedAt·evidence reference
- 허용된 interaction code

포함하지 않는 값:

- `GameObject`, `MonoBehaviour`, `Transform`, `Animator`, `Renderer`
- 서버 EF Entity와 범용 ledger dictionary
- access token·credential
- 허용되지 않은 주소·연락처·정밀 위치
- Synty Prefab 이름

서버가 `farmerX/Y/Z` 같은 Unity 좌표를 일반 업무 DTO로 보내는 방식은 피한다. 공동 World 좌표가 정말 canonical data라면 `CoordinateSystemCode`, `WorldId`, `PositionRevision`이 있는 별도 spatial contract로 정의한다. 초기에는 `FarmZoneKey`, `WaypointKey`, `RouteNodeKey` 같은 semantic location을 Unity layout에 매핑한다.

### 6.6 UseCase

UseCase는 사용자의 하나의 의도를 표현한다.

- 농장 snapshot 불러오기
- 시장가격 조회
- 센서 상태 조회
- 작업 preview
- 명시적으로 확인된 Command 제출
- 성공 뒤 canonical snapshot 재조회

단순 조회가 한 화면에만 있고 조합·검증이 없다면 얇은 UseCase로 시작할 수 있다. 다만 SceneController가 Repository와 정책을 직접 조합하기 시작하면 즉시 UseCase로 분리한다.

### 6.7 Presenter와 ScreenModel

Presenter는 domain/projection 결과를 View가 즉시 소비할 표현 계약으로 바꾼다.

```text
FarmSnapshot
  → FarmPresenter
  → FarmScreenModel
     ├─ title
     ├─ status labels
     ├─ visual state codes
     ├─ interaction availability
     ├─ evidence summary
     └─ loading/error/conflict state
```

Presenter는 Renderer, Material과 Animator를 직접 만지지 않는다. 지역화된 문구를 Presenter가 만들지 View가 lookup할지는 하나의 정책으로 통일하되 stable state code는 유지한다.

### 6.8 SceneController

Unity SceneController는 서버 Controller와 다른 presentation coordinator다.

- Unity lifecycle entry
- UseCase 실행과 취소 token 연결
- loading·refresh·error state를 Presenter/View에 전달
- 사용자 input event를 UseCase intent로 변환
- Scene 전체의 View binding과 해제

금지:

- `Update()`에서 서버 API 호출
- `async void` 예외를 방치
- response DTO를 View에 직접 전달
- animation event로 서버 성공 확정
- Scene reload 시 canonical state를 GameObject에서 복구

`Start()`는 예외 경계가 있는 명시적 초기화 method를 시작하고, object destroy·scene unload와 application exit cancellation을 전달한다.

#### 6.8.1 Zone Controller와 Role Experience Controller

하나의 공유 World에 여러 역할 관점을 겹칠 때 SceneController를 역할별 Scene으로 복제하지 않는다.

- Zone Controller는 장소 snapshot, stable-ID object 생성·갱신·제거와 공통 World View를 소유한다.
- Role Experience Controller는 서버가 승인한 `RolePerspective` 조회, 역할 전환과 Role View socket 적용을 조율한다.
- 두 Controller는 GameObject 참조가 아니라 stable ID로 결합한다.
- 역할 전환은 World View를 유지하고 Role View와 Detail View만 clear·replace한다.

Presentation assembly의 Role Experience Controller는 `역할관점조회UseCase`와 `RolePerspectiveApplicator`를 주입받고, Zone View가 제공하는 `IRolePerspectiveTarget` 목록과 `IRoleInteractionSink`에 결과를 적용한다. Controller가 역할별 권한표, 주소 masking이나 상차 가능 여부를 자체 계산해서는 안 된다. 서버 응답의 `AllowedInteractions`에 없는 action은 표시하거나 실행하지 않으며, 운영 Command는 실행 endpoint에서 다시 권한과 revision을 검증한다.

### 6.9 View

View는 결정된 ScreenModel 또는 PresentationCommand를 Unity component로 표현한다.

허용:

- Animator parameter 적용
- Renderer·Material variant 선택
- NavMeshAgent 목적지 설정
- Transform·Collider·UI·VFX·Audio 제어
- 접근성 상태와 focus 적용

금지:

- 서버 호출
- 가격·생육·권한 규칙 계산
- 원장 저장
- raw DTO 보관
- source provenance 삭제

## 7. Prefab과 Inspector socket 계약

Prefab은 서버 데이터 저장소가 아니라 **배선된 표현 template**이다. Inspector는 실제 서버 값을 입력하는 곳이 아니라 Unity resource와 component reference를 연결하는 도구다.

```text
SsalddelFarmerView
  ├─ Animator socket
  ├─ movement adapter socket
  ├─ body Renderer socket
  ├─ right-hand attachment socket
  ├─ interaction anchor socket
  └─ VisualRoot
       └─ Synty or placeholder visual
```

View socket은 기능 요구를 정의하고 vendor asset 이름을 요구하지 않는다.

각 View는 다음 metadata를 제공하는 방향으로 확장한다.

```text
ViewContractId
ContractVersion
RequiredSockets[]
OptionalSockets[]
AllowedComponentTypes[]
AnimatorParameterContract[]
ValidationSeverity
```

필수 socket은 `OnValidate` 또는 Editor validator에서 검사한다. runtime `Awake`에서도 fail-safe 검사를 할 수 있지만, production 실행 시 매 프레임 reflection scan을 하지 않는다.

## 8. 이동과 animation

### NPC

```text
semantic route/waypoint
  → Unity route mapper
  → NavMeshAgent destination
  → actual movement
  → Animator speed and action state
```

NavMeshAgent는 client 표현 이동을 담당한다. 실제 배차·경로·도착 완료는 서버 상태와 분리한다.

NPC의 서버·Unity 경계는 다음과 같다.

```text
canonical task or explicit simulation fixture
  → NpcMovementApiModel
  → NpcMovementMapper
  → NpcMovementSnapshot
  → ZoneNpcMovementController
  → stable-ID NpcMovementView
  → semantic waypoint Transform
  → NavMeshAgent + Animator
```

서버는 `Vector3` 대신 `RouteCode`, `CurrentWaypointKey`, `DestinationWaypointKey`와 `ArrivalActionCode`를 제공한다. Zone layout이 waypoint key를 실제 Transform에 연결한다. 운영 snapshot에는 `CanonicalTaskStableId`가 필수이며 simulation snapshot은 canonical task를 가질 수 없다.

| Zone | 초기 NPC 역할 | 대표 semantic route |
| --- | --- | --- |
| 농장 | 생산자 | 입구 → 밭 → sensor → 선별·출고 준비 |
| 마트 | 주문자, 재고 담당 | 입구 → 진열대 → 주문대 / stockroom → 진열대 → loading door |
| 주거공동체 | 주문자, 분배 담당 | 세대·게시판 → 공동수령지 / loading point → 수령지 → 관리실 |
| 전통시장 | 상인, 운송자 | 점포 → 저장공간 → 상차지 / 입구 → 상차지 → 출구 |
| 도심 물류센터 | Dock 작업자, 운송자 | 입고 Dock → 분류 → 출고 Dock / 차량 gate → loading bay → 출구 |
| 창고 | picker | 작업대 → rack → 포장 Zone → 출고 Dock |
| 커뮤니티·공공데이터·협동 공간 | 구성원, 안내자, 진행자 | 입구 → 핵심 board·kiosk·table |
| 개인 공간 | 없음 | 자동 NPC를 기본 배치하지 않음 |

`NpcMovementView.Update()`는 이동 속도와 도착 animation만 처리한다. 도착 event 또는 Animator event에서 서버 Command를 호출하지 않는다. 운영 상태 전이는 별도 interaction Controller가 명시적 확인을 받고 서버 성공 뒤 canonical snapshot을 다시 조회한다.

### Player

```text
Unity Input System
  → input adapter
  → player movement controller
  → CharacterController or approved physics body
  → Animator
```

NPC와 Player는 이동 구현이 달라도 공통 presentation state와 Animator contract를 사용할 수 있다.

## 9. DI와 비동기 기술 결정

### 9.1 VContainer

VContainer 1.18.0을 Unity presentation composition root로 채택한다.

- pure C# service는 constructor injection
- Scene별 `LifetimeScope`
- MonoBehaviour는 최소 method injection 또는 registered component binding
- View의 Unity resource reference는 `[SerializeField]` 유지
- core assembly에는 VContainer attribute/reference를 넣지 않음

`도심마트LifetimeScope`와 `전통시장물류거점LifetimeScope`가 현재 Zone 단위 조립의 기준 구현이다. Controller는 concrete UseCase를 `new`하거나 simulation·operational을 선택하지 않고 `[Inject]` method로 주입받는다. 실제 Unity project의 `Packages/manifest.json`에 공식 Git dependency를 고정하며, 재사용 package의 `package.json`에 Git 의존성을 선언하지 않는다.

Application 공통 Scope와 nested Zone Scope는 인증·session·API Client가 실제로 공유될 때 추가한다. 현재는 필요한 Zone Scope만 두어 수명 구조를 선행 일반화하지 않는다.

### 9.2 Task, Unity Awaitable과 UniTask

현재 core는 .NET `Task`와 `CancellationToken`을 사용한다. 이를 유지한다.

Unity 6에는 Unity lifecycle에 맞춘 pooled `Awaitable`이 있고, UniTask는 `WhenAll`, PlayerLoop timing과 Unity integration을 제공한다. 따라서 UniTask를 즉시 전 계층 표준으로 정하지 않고 다음 기준으로 ADR을 작성한다.

| 위치 | 기본 방향 |
| --- | --- |
| engine-independent core | `Task`/`ValueTask`와 `CancellationToken` |
| Unity native async adapter | Unity `Awaitable` 검토 |
| 복합 비동기·PlayerLoop·성능 요구 | UniTask 검토 |
| public interface | 구현 package를 불필요하게 노출하지 않는 return type 우선 |

어떤 방식을 선택해도 object destroy, scene unload와 application exit cancellation을 연결하고 unobserved exception 정책을 검증한다.

### 9.3 JSON

현재 Unity project package manifest가 없으므로 Newtonsoft Json 채택은 미확정이다. 선택 전 다음을 contract fixture로 비교한다.

- `System.Text.Json` 사용 가능 범위
- Unity용 Newtonsoft package 호환성
- IL2CPP/AOT와 code stripping
- `DateOnly`, nullable, decimal과 unknown field 처리
- server JSON과 exact casing

## 10. 첫 농장 Vertical Slice

문서 예시의 `GET /api/farms/{id}`는 현재 구현 route로 확인되지 않았으므로 개념 예시로만 취급한다. 실제 구현은 기존 route와 public projection을 조사한 뒤 확정한다.

첫 slice는 서버 전체 농장 aggregate를 한 번에 복제하지 않는다.

```text
1. 지역 농수산 marker projection
2. KAMIS 감자 가격 observation
3. 농사로 감자 작목·농작업 일정 projection
4. versioned weather fixture 또는 승인된 관측
5. 대표 soil profile
6. SIMULATED soil-moisture sensor
7. Farm screen model
8. primitive Farm View
9. sensor external view and emanation view
10. evidence-card Web handoff
```

Prefab 구조:

```text
FarmRoot
  ├─ FarmView
  ├─ FarmStatusPanel
  ├─ FarmerView
  │   └─ VisualRoot
  ├─ WarehouseView
  │   └─ VisualRoot
  ├─ CropPlotView[]
  │   └─ CropVisualRoot
  └─ SensorView[]
      └─ SensorVisualRoot
```

첫 단계에는 placeholder primitive를 사용한다. Synty 적용 후에도 View contract, UseCase와 data tests가 변하지 않아야 한다.

## 11. Editor·CLI·AI 자동화

자동화의 권위는 View contract와 deterministic Editor Script에 있다. AI가 Unity asset을 직접 임의 수정하는 구조로 시작하지 않는다.

```text
asset inventory
  → candidate analysis
  → dry-run wiring plan
  → deterministic Editor Script
  → isolated Prefab generation
  → socket validation
  → EditMode tests
  → PlayMode smoke
  → human visual review
  → approved Prefab promotion
```

Editor Script가 수행할 수 있는 작업:

- 허용된 vendor folder에서 Prefab 검색
- component·bone·socket 후보 조사
- Ssalddel wrapper GameObject 생성
- required component 추가
- `SerializedObject`를 이용한 reference 연결
- Prefab 저장과 validation report 생성

안전 기준:

- 원본 vendor asset을 수정하지 않음
- asset GUID와 generated Prefab path를 manifest에 기록
- ambiguous socket은 자동 선택하지 않고 실패
- dry-run report 없이 대량 쓰기 금지
- 생성 범위를 전용 output folder로 제한
- 실행 전·후 version control diff 확인
- asset license와 raw file 공유 범위 준수
- AI 입력으로 asset 원본을 외부 provider에 업로드하지 않음

Unity CLI는 `-batchmode`, `-projectPath`, `-executeMethod`, `-quit`을 이용할 수 있지만, 실제 Editor version과 license 환경을 고정하고 exit code·Editor log·test result를 함께 보존한다.

## 12. 검증 전략

| 계층 | 검증 |
| --- | --- |
| ApiModel·Mapper | JSON contract fixture, schema, unknown field, unit, provenance |
| Repository | initial/refresh failure, last-success retention, cancellation, retry classification |
| UseCase | 권한 입력, expected revision, preview/confirm/re-query 순서 |
| Presenter | 모든 state의 ScreenModel과 accessibility label |
| View socket | missing·ambiguous reference, Animator parameter와 vendor-independent binding |
| EditMode | Prefab contract, serializer, mapper, Editor automation dry-run |
| PlayMode | Scene load, input, marker reconcile, View state, destroy cancellation |
| built player | Windows·Android HTTP, IL2CPP stripping, performance와 실제 렌더 |
| external runtime | 실제 API source·count·URL·visible state |

최소 화면 상태:

```text
Idle
Loading
Success
InitialLoadError
Refreshing
RefreshError
Cached
Fixture
Stale
Invalid
NoAccess
Conflict
```

build/test, Unity Editor, built player, 실제 API, visual capture, commit, push와 deploy는 각각 별도 증거다.

## 13. 보완된 구현 순서

1. Unity Editor version, render pipeline, platform, project 위치 ADR
2. 공통 data envelope와 evidence/rule reference 확정
3. 지역 농수산 marker·KAMIS·농사로를 Unity ApiModel/Mapper로 이관
4. 실제 P2 Unity runtime source 위치와 assembly dependency 확인
5. API Client·Repository port/adapter와 load-state contract 통합
6. 첫 Farm UseCase·Presenter·ScreenModel 작성
7. primitive SceneController·View·Prefab socket 작성
8. EditMode·PlayMode와 built player smoke 구축
9. DI·async·JSON 후보를 측정 후 ADR로 선택
10. sensor와 evidence-card handoff 연결
11. 무료 또는 단일 asset pack으로 wrapper compatibility 검증
12. 필요한 Synty asset 구매 범위 확정
13. Editor automation dry-run과 deterministic Prefab generator 작성
14. 사람의 최종 visual·accessibility·performance 검토

## 14. 변경된 제안 사항 요약

원안에서 그대로 채택한 내용:

- server authority와 Unity presentation 분리
- API Client→Repository→UseCase→Scene/View 계층
- MonoBehaviour의 business logic 최소화
- View socket과 Prefab·Inspector 배선
- Synty를 교체 가능한 presentation resource로 취급
- CLI·Editor Script를 이용한 반복 조립 자동화

보완한 내용:

- DTO mapping을 Repository 내부 구현 세부로 숨기지 않고 독립 Mapper로 유지
- server domain과 구분하기 위해 Unity model을 GameModel/ProjectionModel로 명명
- `Task` 기반 core와 Unity Awaitable/UniTask adapter를 분리
- `async void Start` 대신 cancellation·exception boundary가 있는 초기화 사용
- server가 Unity `Vector3`를 일반 DTO로 전달하지 않고 semantic location을 사용
- stable ID, schema, revision, provenance, unit, freshness와 limitation을 필수화
- initial load와 refresh failure 정책 분리
- 운영 Command 뒤 canonical snapshot 재조회 의무화
- AI 자동 배선을 dry-run·deterministic Editor Script·검증·사람 승인 흐름으로 제한

## 15. 참고 자료

- [Unity Awaitable 비동기 프로그래밍](https://docs.unity3d.com/kr/6000.0/Manual/async-await-support.html)
- [Unity Input System](https://docs.unity3d.com/ja/current/Manual/com.unity.inputsystem.html)
- [Unity Test Framework](https://docs.unity3d.com/kr/current/Manual/com.unity.test-framework.html)
- [Unity command-line arguments](https://docs.unity3d.com/es/current/Manual/CommandLineArguments.html)
- [VContainer repository and documentation](https://github.com/hadashiA/VContainer)
- [UniTask repository and documentation](https://github.com/Cysharp/UniTask)

외부 package의 구체 version은 이 문서에 고정하지 않는다. Unity Editor version과 compatibility를 확정하는 ADR에서 lock file과 함께 기록한다.
