# Unity 외부 Reference Pattern 선별 도입 제안

> 상태: 제안
>
> 적용 대상: `Ssalddel.Unity`의 engine-independent 계약과 `C:\Users\user\ssalddel` Unity Presentation project
>
> 기준 아키텍처: [Unity Data·World Interpretation·Perspective·Presentation 기준 아키텍처](UnityDataInterpretationPresentationArchitecture.md)
>
> 관련 구현 순서: [Unity Composition Set 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md), [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)

## 1. 제안 요약

다섯 외부 프로젝트 중 하나를 fork하거나 통째로 이식하지 않는다. 각 프로젝트에서 검증 가치가 있는 패턴만 추출해 기존 Ssalddel 흐름 아래에 둔다.

```text
Ssalddel Server / Simulation
  → authorized Data Snapshot
  → Shared World Interpretation
  → Perspective Interpretation
  → Presentation Model
  → Ssalddel View wrapper / VisualRoot
  → Synty Character·Vehicle·Facility adapter
  → Animator·NavMeshAgent·RouteFollower·WheelCollider·GameObject
```

우선 표준화할 것은 다음 세 가지다.

1. `SyntyCharacterVisualAdapter`: 논리 actor와 Synty 외형·Avatar·Animator 연결
2. `NpcMovementPresenter`: 확정된 이동 intent를 waypoint·NavMesh·Animator로 연출
3. `SyntyVehicleVisualAdapter`: 차량 의미와 Synty mesh·wheel·movement 구현 분리

공간 실측과 Facility 표현은 별도 Synty Domain이 아니라 기존 Composition descriptor, connector, socket, `VisualRoot` 체계를 확장한다. `Synty기사`, `Synty농장`, `Synty창고`, `Synty트럭` 같은 업무 type은 만들지 않는다.

## 2. 현재 기준선과 이 제안의 위치

이 제안은 새 아키텍처로 교체하는 문서가 아니다. 현재 확인된 다음 기반을 재사용한다.

- `Data → Shared/Perspective Interpretation → Presentation` 경계
- stable ID, revision, source lineage와 `Operational`/`Simulation` 분리
- Ssalddel wrapper의 `VisualRoot`와 `VisualKey/Catalog`
- Farm·Town·City Composition descriptor, connector, socket과 source 측정 catalog
- 공용 Idle/Walk intent·catalog·adapter와 procedural fallback
- `ThreeRegionHubJourney`의 사람 Journey와 allocation-gated 화물 차량 route follower
- FARM-2의 Preview → Confirm → Simulation Tick → canonical snapshot → reconcile 폐루프

따라서 외부 프로젝트가 제공하는 것은 업무 상태나 새 Domain이 아니라 Presentation 구현 선택지다. 현재 동작하는 scene·catalog·adapter를 대체하지 않고, 빠진 계약과 검증을 additive하게 보강한다.

## 3. Reference 프로젝트별 채택 범위

아래 평가는 사용자가 제공한 조사 결과를 제안 입력으로 사용한다. 실제 코드 도입 전에는 각 upstream 저장소의 URL, 고정 commit, 파일별 license, Unity·package 버전과 유료 asset 제외 상태를 별도 inventory로 다시 확인한다.

| Reference | 채택할 패턴 | Ssalddel 적용 | 채택하지 않을 것 |
| --- | --- | --- | --- |
| `DialogueDreamland` | 논리 NPC와 runtime visual model 분리, model 생성 후 Avatar·Animator 배선, 카메라 책임 분리 | Character visual factory와 adapter 생명주기, 간소화한 탑다운 camera input | 미완성 NPC AI, 프로젝트 전체 runtime framework, 그대로 복사한 third/first-person camera |
| `UnityThirdPersonSandbox` | 명시적 state enter/exit, waypoint, NavMeshAgent, locomotion parameter | snapshot이 지시한 이동을 수행하는 local presentation state machine | combat·chase 의미, Unity state가 업무 상태를 결정하는 구조 |
| `BasicVehiclesControl_ForSyntyCity` | WheelCollider와 Synty vehicle hierarchy·wheel 설정, Unity 6 Input 참고 | `VehicleProfile` 기반 prefab 차이 흡수와 선택적 player-control mode | 모델별 controller class 증식, W/A/S/D 입력을 물류 기본 권위로 사용 |
| `EdTechALL` | 실제 치수·지도와 Unity 좌표·prefab 배치 분리, vendor/custom 폴더 분리 | spatial anchor·scale·origin·measurement provenance와 composition socket | mini-game domain, asset·code 일괄 복사 |
| `EscapeRoomGame` | Town Pack의 실내 밀도·동선·URP 구성 사례 | 선택적 Town interior composition의 시각 reference | escape-room rule과 scene architecture |

참고 순위는 구현 순서가 아니다. NPC 이동 패턴의 참고 가치가 가장 높더라도 먼저 Ssalddel Presentation 계약과 source/license inventory를 고정해야 한다.

## 4. 지켜야 할 권위 경계

### 4.1 서버와 Simulation이 소유하는 것

- actor·vehicle·facility·cargo의 stable ID와 revision
- 현재 업무 상태와 허용된 다음 interaction
- 배차·상차·배송·검수·하차·복귀의 canonical 전이
- route 목적과 업무상 출발지·목적지
- 권한·개인정보 공개 범위와 source lineage
- `Operational` 또는 `Simulation` mode

### 4.2 Interpretation이 소유하는 것

- 여러 snapshot 사이의 actor·vehicle·cargo·facility 관계
- 현재 관점에서 보여줄 이동·대기·작업 intent
- route semantic, destination anchor와 표현 제한
- stale·blocked·unknown 상태와 fallback 허용 여부

### 4.3 Presentation이 소유하는 것

- prefab, Avatar, RuntimeAnimatorController와 Animator parameter
- NavMesh 경로, 보간, 회전, wheel pose와 카메라 focus
- material, shader, LOD, VFX, SFX와 시각 fallback
- scene-local waypoint와 world-coordinate binding

NPC 도착, vehicle collider 진입, animation event 또는 VFX 완료는 Command를 실행하거나 canonical 상태를 바꾸지 않는다. Presentation은 도착을 로컬로 표시한 뒤 다음 snapshot을 기다리거나 명시적 Application interaction을 제공한다.

## 5. 공통 Presentation 계약

Core에는 Unity·Synty 이름이 없는 의미 계약만 둔다.

```text
ActorPresentationModel
  ActorStableId
  Revision
  ActorRoleCode
  VisualArchetypeCode
  MovementIntentCode
  ActivityIntentCode
  RouteStableId
  DestinationAnchorStableId
  DataMode
  Lineage

VehiclePresentationModel
  VehicleStableId
  Revision
  VehicleKindCode
  VisualArchetypeCode
  MovementIntentCode
  RouteStableId
  CargoStableIds
  ControlPolicyCode
  DataMode
  Lineage
```

`VisualArchetypeCode`는 `City_Male_01`이나 prefab path가 아니라 `resident-adult`, `market-worker`, `delivery-driver`, `delivery-van` 같은 제품 의미 key다. 실제 Synty asset은 Presentation catalog가 이 key에 매핑한다.

```text
VisualArchetypeCode
  → Presentation Visual Catalog
  → VisualKey
  → Synty prefab or project-owned fallback
```

Synty asset이 없거나 mapping이 잘못됐을 때도 actor의 stable ID와 업무 상태는 유지하고 primitive 또는 project-owned fallback을 표시한다.

## 6. `SyntyCharacterVisualAdapter`

### 6.1 책임

- `VisualArchetypeCode`로 character visual을 resolve한다.
- `VisualRoot` 아래에만 instance를 생성하거나 교체한다.
- Humanoid Avatar, 검증된 Animator Controller와 renderer를 배선한다.
- 모델의 pivot·scale·forward axis를 profile로 보정한다.
- 교체 전후에도 wrapper의 stable ID, selection, interaction과 route binding을 유지한다.
- addressable 또는 비동기 생성이 필요하면 cancellation·중복 요청·scene unload를 처리한다.

### 6.2 금지 사항

- 원본 Synty prefab 수정
- prefab 이름으로 actor role·권한·업무 상태 추론
- animator state를 canonical actor state로 역기록
- runtime visual 생성 실패를 actor 데이터 부재로 처리

### 6.3 권장 구성

```text
기사View
  ├─ StableIdBinding
  ├─ SelectionSocket
  ├─ NpcMovementPresenter
  └─ VisualRoot
       └─ SyntyCharacterVisualAdapter
            ├─ CharacterVisualProfile
            ├─ Animator
            └─ Avatar / Renderer
```

`CharacterVisualProfile`은 Presentation 전용 asset이며 최소한 visual key, prefab reference, scale, pivot offset, forward axis, Avatar compatibility, animator controller key와 verified source kind를 기록한다.

## 7. `NpcMovementPresenter`

외부 state machine의 `Enter → SetDestination → Walk → Arrive → Wait` 구조는 재사용하되 업무 state machine으로 승격하지 않는다. 로컬 상태 이름은 표현 책임을 드러낸다.

```text
Hidden
ResolvingRoute
Moving
WaitingAtAnchor
BlockedPresentation
Reconciling
```

예를 들어 `상차장 이동`은 서버·Simulation snapshot의 업무 의미이고, `Moving`은 그 의미를 보여주는 scene-local 상태다.

```text
Snapshot revision N: 이동 intent + destination anchor
  → route/anchor resolve
  → NavMeshAgent.SetDestination()
  → Animator locomotion intent
  → local arrival
  → WaitingAtAnchor
  → revision N+1 snapshot 수신
  → reconcile 후 다음 표현
```

필수 동작은 다음과 같다.

- 같은 stable ID·revision은 재시작하지 않는다.
- 높은 revision이 경로 중 도착지를 바꾸면 현재 이동을 취소하고 새 intent로 reconcile한다.
- 낮은 revision과 중복 stable ID는 거부한다.
- NavMesh path 실패는 업무 실패로 바꾸지 않고 `BlockedPresentation`으로 표시한다.
- teleport는 명시적 spawn/reset/recovery policy에서만 허용한다.
- root motion은 기본적으로 끄고 위치 권위는 NavMeshAgent 또는 route follower 한 곳에 둔다.
- animation event는 발소리·접촉 FX 같은 국소 효과에만 사용한다.

## 8. `SyntyVehicleVisualAdapter`

차량은 외형, 이동 model과 control source를 분리한다.

```text
차량PresentationModel
  → VehicleMovementPresenter
       → Movement implementation
            ├─ RouteFollower        기본 자동 연출
            ├─ NavMeshVehicle       제한된 저속 공간
            └─ WheelColliderDrive   물리 운전 검증이 필요한 mode
  → SyntyVehicleVisualAdapter
       → VehicleVisualProfile
       → body / wheel visual / lights / cargo socket
```

`WheelCollider`는 모든 물류 차량의 기본 조건이 아니다. 탑다운 World에서 서버·Simulation route를 안정적으로 재현하는 첫 기준은 deterministic route follower다. WheelCollider는 회전 반경, 경사, suspension, 직접 운전이 실제 경험 가치나 검증 요구를 가질 때 선택한다.

`VehicleVisualProfile` 후보 필드:

- visual key와 prefab reference
- body scale·pivot·forward axis
- wheel transform mapping과 wheel radius
- steering·drive axle 설정
- mass·center of mass·suspension preset
- cargo socket과 door socket
- route-follower / WheelCollider 호환 여부
- PC·Mobile LOD와 collider policy

차종마다 `Van01Controller`, `Van02Controller`를 만들지 않고 profile 차이만 둔다. Player Control은 별도 input adapter가 `ControlPolicyCode=PlayerAssisted`일 때만 연결하며, 기본 물류 상태는 `SnapshotDriven`이다.

## 9. Facility와 실제 공간 mapping

EdTechALL에서 참고할 핵심은 지도 자체가 아니라 측정된 현실 공간과 Unity 표현의 중간 계약이다. 기존 Composition source measurement와 connector 체계를 다음 정보로 확장한다.

```text
SpatialReferenceProfile
  ReferenceStableId
  SourceKind
  SourceRevision
  RealWidthMeters / RealLengthMeters
  UnityOriginAnchor
  YawAlignment
  ScalePolicy
  PrecisionCode
  Limitation
```

실제 밭 800㎡를 반드시 1:1 축척으로 렌더링한다는 뜻은 아니다. 실측 source, 적용한 축척·단순화 규칙과 표현 한계를 보존한다는 뜻이다. 위치·주소·소유 경계처럼 민감한 공간 정보는 서버가 허용한 정밀도보다 Unity에서 더 정밀하게 복원하지 않는다.

Facility는 기존 업무 object wrapper 아래에 둔다.

```text
농장 / 창고 / 마트 / 물류센터
  → FacilityPresentationModel
  → FacilityView wrapper
  → facility VisualRoot
  → SyntyFacilityVisualAdapter 또는 fallback
```

환경용 집·나무·울타리·도로에는 무조건 stable ID를 부여하지 않는다. 선택, 상태, interaction, cargo lineage 또는 source evidence를 가진 object만 canonical wrapper에 연결한다.

## 10. Asset과 소스 격리

권장 물리 구조는 vendor asset, 외부 reference source와 Ssalddel 코드를 분리한다.

```text
Assets/
  Synty/                         구매 원본, 직접 수정 금지
  ThirdPartyReference/           실제 도입이 승인된 외부 코드만
    <Project>/<PinnedCommit>/
      NOTICE.md
      LICENSE.*
  Ssalddel/
    Runtime/Presentation/
      Characters/
      Movement/
      Vehicles/
      Facilities/
      Spatial/
    Settings/Presentation/
      CharacterVisualProfiles/
      VehicleVisualProfiles/
      SpatialReferenceProfiles/
    Experiments/SyntyIntegration/
```

외부 코드를 직접 도입할 때는 최소한 다음을 기록한다.

- upstream URL과 고정 commit/tag
- 도입 파일 목록과 변경 내용
- license와 attribution 의무
- 포함되지 않은 유료 asset·sample data
- Unity·URP·Input System·Cinemachine·Addressables package 호환성
- Ssalddel wrapper 또는 adapter가 소유하는 경계

패턴만 재구현한 경우에도 조사 출처와 “코드 복사 없음”을 기록해 provenance를 명확히 한다.

## 11. Reference Sandbox 범위

`SsalddelSyntyIntegrationSandbox`는 새 제품 World나 거대한 showcase가 아니라 adapter 회귀검증 scene이다. 현재 `ThreeRegionHubJourney`와 `FarmCityGraphicalShowcase`가 이미 World 구성과 관통 Journey를 검증하므로 이를 복제하지 않는다.

```text
SsalddelSyntyIntegrationSandbox
  ├─ TestRoadAndNavMesh
  ├─ TestFacilityAnchors
  │    ├─ FarmAnchor
  │    ├─ LogisticsDockAnchor
  │    ├─ MarketAnchor
  │    └─ ResidentialAnchor
  ├─ CharacterCases
  │    ├─ Resident
  │    ├─ MarketWorker
  │    └─ DeliveryDriver
  ├─ VehicleCases
  │    └─ DeliveryVan
  ├─ SnapshotScenarioControls
  └─ TopDownCameraRig
```

Sandbox는 다음 네 가지를 재현 가능하게 검증한다.

1. actor snapshot 변경에 따라 한 NPC가 route를 이동하고 VisualRoot 교체 뒤에도 identity가 유지된다.
2. vehicle snapshot 변경에 따라 Van 한 대가 route follower로 이동하며 cargo lineage가 유지된다.
3. 진행 중 높은 revision, stale revision, path failure와 visual load failure가 정해진 정책으로 처리된다.
4. 탑다운 3/4 camera에서 character·vehicle·facility의 가독성과 PC/Mobile 비용을 측정한다.

기존 scene에서 위 항목을 독립적으로 안정되게 검증할 수 있다면 새 scene을 만들지 않고 test fixture와 validation menu만 추가한다.

## 12. 구현 단계와 완료 Gate

현재 CMP6 우선순위와 FARM-3 복귀 계획을 깨지 않도록 다음 단위로 진행한다.

| 단계 | 내용 | 완료 Gate |
| --- | --- | --- |
| `REF0` | 다섯 upstream URL·commit·license·package compatibility inventory | 채택·재구현·보류가 파일 단위로 구분되고 유료 asset이 source 도입 범위에 없음 |
| `SYN-CHAR-1` | character profile·factory·`SyntyCharacterVisualAdapter` 표준화 | 세 Pack 대표 Humanoid와 fallback이 같은 wrapper에서 교체되고 missing asset이 안전하게 표시됨 |
| `SYN-MOVE-1` | `NpcMovementPresenter`의 revision·cancel·replan·blocked 정책 | snapshot-driven 이동, stale 거부, 도착 비권위와 Animator locomotion이 EditMode/PlayMode에서 확인됨 |
| `FARM-3 / ANIM3` | 확정된 농부 작업 한 종류 연결 | FARM-2 canonical task를 이동·작업으로 표현하되 animation 완료가 Tick을 발생시키지 않음 |
| `SYN-VEH-1` | 공통 vehicle profile과 route follower adapter | 한 Van이 visual profile과 무관하게 같은 route/cargo model을 소비함 |
| `SYN-VEH-2` | 필요할 때만 WheelCollider profile pilot | route follower와 별도 mode로 한 차량의 물리 설정·입력·복귀 정책 검증 |
| `SYN-SPATIAL-1` | spatial reference profile과 facility anchor 연결 | meter·origin·scale·precision·limitation이 composition 결과와 함께 검증됨 |
| `SYN-QA-1` | Sandbox 또는 기존 Journey scene 회귀 suite | 저장 Scene reload, Play Mode, 최종 Game View PNG, Console error 0과 성능 수치 기록 |

`REF0` 조사 때문에 이미 계획된 CMP6 구현을 막을 필요는 없다. 다만 외부 코드를 복사하거나 새 package 의존성을 추가하는 시점 전에는 반드시 통과해야 한다.

## 13. 테스트 전략

### Core/headless

- Presentation model에 UnityEngine·Synty asset path가 들어가지 않는 architecture test
- stable ID·revision·mode·lineage validator
- 같은 snapshot의 idempotency와 stale revision 거부
- 이동·차량 intent projector의 deterministic 결과

### Unity EditMode

- visual profile catalog의 key 중복·missing prefab·Avatar 호환성
- 원본 Synty prefab 변경 없음과 wrapper/VisualRoot 규칙
- wheel mapping, cargo socket, scale·pivot·forward axis 검사
- scene anchor와 spatial profile의 단위·precision 검증

### Unity PlayMode/Game View

- snapshot N→N+1 이동·취소·재계획
- visual 비동기 교체 중 identity·selection·route 유지
- NavMesh/path failure와 visual fallback
- 차량 route follower, 선택적 WheelCollider와 camera follow
- 최종 1600×900 Game View, Console error, renderer·draw call·triangle·memory 기준

PlayMode 통과, Game View 증거, PC 성능, Android 성능은 각각 별도 사실로 기록한다.

## 14. 수용 기준

다음 조건을 모두 만족할 때 외부 Reference Pattern 도입 1차를 완료로 본다.

1. Domain·Data·Interpretation에 Synty prefab 이름, Animator parameter와 WheelCollider 설정이 없다.
2. 캐릭터·차량 visual을 fallback 또는 다른 asset으로 교체해도 stable ID와 업무 흐름이 유지된다.
3. NPC·차량 이동은 snapshot을 표현하며 도착·animation event가 업무 전이를 확정하지 않는다.
4. `Operational`과 `Simulation` snapshot·UI 표시·interaction gate가 섞이지 않는다.
5. 외부 코드와 asset의 URL·commit·license·attribution·변경 이력이 추적된다.
6. 세 대표 actor와 Van 한 대가 같은 공통 adapter를 사용하고 모델별 controller class가 생기지 않는다.
7. 실제 공간 mapping은 source·단위·축척·정밀도·한계를 보존한다.
8. Unity 저장 Scene·Play Mode·Game View와 자동화 검증 결과가 각각 기록된다.

## 15. 보류 사항

- 범용 NPC planner, behaviour tree와 combat AI
- 차량별 전용 controller class
- 기본 player driving과 first/third-person camera
- Synty 전용 Domain·server contract·stable ID
- 모든 Town interior와 모든 City/Farm prefab의 일괄 배치
- 외부 프로젝트 package·scene·settings의 통째 이식
- 검증되지 않은 animation clip 또는 controller를 Synty 제공 자산으로 표기

이 제안의 목표는 Synty 사용량을 최대화하는 것이 아니라, Ssalddel의 권위 경계를 유지한 채 캐릭터·이동·차량·공간 표현을 반복해서 안전하게 교체하고 검증할 수 있게 하는 것이다.
