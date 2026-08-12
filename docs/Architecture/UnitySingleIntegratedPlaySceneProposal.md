# Unity 단일 통합 플레이 Scene 재편 제안서

## 1. 제안 결론

현재 Unity 프로젝트에는 새 통합 Scene을 하나 더 만들지 않는다.

기존 `Assets/Ssalddel/Scenes/SimulationWorldShell.unity`를 장기적인 **유일한 실제 플레이 Scene**으로 승격한다. 현재 빌드 진입점인 `WorldBootstrapScene.unity`의 공공 세계지도 기능은 삭제하거나 Simulation 권위와 합치지 않고, 읽기 전용 `공공세계지도Surface`로 `SimulationWorldShell` 안에 이식한다.

최종 목표는 다음과 같다.

```text
Unity 실행
  → SimulationWorldShell.unity 한 개 로드
  → 공공 세계지도 관찰
  → Simulation World Map
  → 정착지 Overview
  → District
  → Object·Lot·Cargo·NPC
  → Preview
  → 명시적 Confirm
  → Task·WorldTick·Effect
  → canonical snapshot 재조회
  → 같은 Scene의 표현 갱신
```

농사, 판로, 화물, 창고, 시장, 같이주문, 개별주문, 음식배달, 턴 카드는 더 이상 각각의 실제 플레이 Scene이 아니다. 같은 Scene에서 authoritative snapshot과 사용자의 관찰 위치가 바뀌면서 드러나는 **업무 흐름과 Presentation 상태**다.

이 문서는 기존 [Unity Simulation World Shell·정착지 Scene 기반 재정렬 제안서](UnityWorldShellSettlementSceneFoundationProposal.md)와 [통합 모판·전시관 Object 리팩토링 계획](UnityIntegratedSeedbedExhibitionObjectRefactoringPlan.md)을 폐기하지 않는다. 앞 문서가 정한 WorldShell 권위 경계와 뒤 문서가 정한 Object 이식 단위를 이용해, 현재 흩어진 Scene을 한 실제 플레이 무대로 수렴시키는 후속 계획이다.

---

## 2. 현재 프로젝트에서 확인한 사실

### 2.1 빌드 설정

2026-08-11 현재 `ProjectSettings/EditorBuildSettings.asset`에는 다음 Scene 하나만 활성화되어 있다.

```text
Assets/Ssalddel/Scenes/WorldBootstrapScene.unity
```

따라서 형식상 빌드 진입 Scene은 이미 하나다. 그러나 이 Scene은 `WorldBootstrapSceneCompositionRoot`를 통해 공공데이터·커뮤니티 세계지도 marker를 읽고 상세 관찰 Panel을 여는 공개지도 Surface다. Farm·정착지·판로·물류·턴 마감의 실제 플레이 Shell은 아니다.

### 2.2 Scene 분포

`Assets/Ssalddel` 아래에는 `.unity` Scene이 총 35개 있다.

| 구분 | 수량 | 현재 역할 |
| --- | ---: | --- |
| `Experiments - 연구` | 29 | 에셋 연구, 시각 품질, 감자 생산·유통 단계, 생산자 판로 등 검증 |
| `Scenes` | 6 | 공개지도 진입, Simulation WorldShell, primitive, 모판·전시관 |

`Assets/Ssalddel/Scenes`의 현재 파일은 다음과 같다.

- `WorldBootstrapScene.unity`
- `SimulationWorldShell.unity`
- `UrbanLogisticsCenterPrimitive.unity`
- `UrbanMarketManagerPrimitive.unity`
- `턴카드모판.unity`
- `통합모판전시관.unity`

문제는 Scene 개수 자체보다 **어느 Scene이 실제 게임이고 어느 Scene이 연구·검증용인지 경계가 Project 창만으로는 완전히 닫히지 않았다는 점**이다.

### 2.3 이미 통합 플레이 Scene에 가까운 기반

`SimulationWorldShell.unity`에는 이미 다음 기반이 있다.

- `WorldMapRoot`
- `SettlementInteriorRoot`
- Farm·Town·Market·Storage·Logistics·Residential District
- Gate·Garrison placeholder
- 공통 `DioramaTopDownCameraRig`
- 공통 Lighting과 Persistent UI
- HarvestLot 판로 선택
- 물류 이동
- 턴 마감
- World·Zone·Object Focus
- 같은 session·WorldTick·WorldRevision을 유지하는 Shell 상태기계

`SimulationWorldShellBuilder`도 Shell, 정착지, 시각 기반, 정착지 상호작용, 물류 이동, 턴 마감을 같은 Scene에 조립한다. 따라서 새 통합 Scene을 만드는 것보다 이 Scene을 기준 플레이 무대로 승격하는 편이 기존 검증과 참조를 가장 많이 보존한다.

### 2.4 아직 분리된 두 세계

현재는 다음 두 흐름이 물리적으로 나뉘어 있다.

```text
WorldBootstrapScene
  └─ 운영 서버의 공공데이터·커뮤니티 세계지도 관찰

SimulationWorldShell
  └─ Simulation session·정착지·경제·판로·물류·턴 플레이
```

이 둘을 한 Scene에 둔다고 해서 원장과 authority까지 하나로 합치는 것은 아니다. **한 화면 무대 안에서 서로 다른 자료 계층을 함께 관찰**하되, 공공 관측과 Simulation 상태는 별도 repository·use case·snapshot·오류 상태를 유지해야 한다.

### 2.5 현재 작업 트리 경계

확인 시점의 별도 Unity 저장소에는 `SimulationWorldShell`, 턴 카드, 통합 전시관, `Experiments - 연구` 이동과 에셋 관련 변경이 함께 존재한다. 따라서 이 제안의 구현을 시작할 때는 현재 변경을 통째로 이동·삭제·재생성하지 않는다.

1. 관련 경로와 소유 맥락을 다시 확인한다.
2. `OBJ-2 + OBJ-3` 변경과 Scene 통합 변경을 별도 맥락으로 나눈다.
3. Builder가 생성하는 파일과 사용자가 배치한 파일을 구분한다.
4. Build Settings 전환은 통합 Scene의 Play Mode 검증 뒤 독립 변경으로 수행한다.
5. 기존 연구 Scene 삭제는 별도 승인 없이는 수행하지 않는다.

### 2.6 기존 결정과의 관계

현재 D-045는 공개지도 `WorldBootstrapScene`과 `SimulationWorldShell`을 물리적으로 분리해, 공개지도 repository가 Simulation authority로 암묵적으로 승격되는 일을 막는다. 이 문서는 그 당시의 권위 경계를 유지하면서 **물리적 Scene만 하나의 플레이 무대로 수렴**시키자는 후속 제안이다.

아직 구현 결정으로 확정된 것은 아니다. 이 제안이 승인되어 `SINGLE-SCENE-1`에 들어갈 때는 다음 새 결정을 `DECISIONS.md`에 기록해야 한다.

- D-045의 authority 분리는 계속 유효하다.
- `WorldBootstrapScene`의 실제 빌드 진입 책임만 `SimulationWorldShell`로 대체한다.
- 공공 관측과 Simulation은 같은 `.unity` 안에서도 별도 composition·snapshot·오류 상태를 가진다.
- 기존 결정을 조용히 덮어쓰지 않고 물리 Scene 정책만 대체 관계로 명시한다.

---

## 3. 목표 Scene과 파일 정책

### 3.1 canonical 실제 플레이 Scene

첫 통합 완료 시점의 canonical 경로는 다음으로 고정한다.

```text
Assets/Ssalddel/Scenes/SimulationWorldShell.unity
```

지금 `SsalddelWorld.unity` 같은 새 이름으로 옮기지 않는다. 현재 Scene, Builder, Test와 `.meta` GUID를 보존하는 것이 우선이다. 제품 이름으로 바꿀 필요가 생기면 통합 완료 뒤 별도 리팩토링에서 Unity `AssetDatabase` 이동과 전체 참조 검증을 거친다.

### 3.2 최종 Build Settings

통합 Gate를 모두 통과한 뒤 Build Settings는 다음 하나만 활성화한다.

```text
Assets/Ssalddel/Scenes/SimulationWorldShell.unity
```

`WorldBootstrapScene`, 모판, 전시관, primitive와 연구 Scene은 Build Settings에서 제외한다. 파일을 즉시 삭제하지 않으며 Editor 검증과 회귀 비교에 계속 사용한다.

### 3.3 첫 버전은 물리적으로도 한 Scene

첫 통합 버전에서는 gameplay를 additive Scene으로 분할하지 않는다.

- World Map을 별도 Scene으로 로드하지 않는다.
- Farm·Market·Storage·Hub를 별도 Scene으로 로드하지 않는다.
- 턴 카드나 판로 선택을 별도 Scene으로 열지 않는다.
- District 전환은 root 활성화와 camera focus로 처리한다.
- UI는 공통 Canvas 아래 Panel 전환으로 처리한다.

향후 메모리·빌드 시간 때문에 streaming이 필요해져도 분할 기준은 업무 기능이 아니라 terrain·lighting·대형 visual chunk여야 한다. 그때도 사용자에게는 하나의 World이고 Simulation snapshot은 바뀌지 않는다.

---

## 4. 목표 hierarchy

기존 직렬화 이름은 호환을 위해 한 번에 바꾸지 않는다. 아래 이름은 장기 책임을 설명하는 목표 구조이며, 새 C# type은 `한국어 업무 의미 + 영어 기술 역할` 원칙을 따른다.

```text
SimulationWorldShell
├─ ShellRuntimeRoot                         [기존 유지]
│  ├─ 통합세계CompositionRoot
│  ├─ 공공세계지도CompositionRoot
│  ├─ Simulation세계CompositionRoot
│  ├─ 세계PresentationCoordinator
│  ├─ 정착지상호작용Presenter
│  ├─ 물류이동Presenter
│  └─ 턴마감Presenter
│
├─ WorldMapRoot                             [기존 유지]
│  ├─ TerrainRoot
│  ├─ TerritoryRoot
│  ├─ 공공관측PresentationRoot
│  │  ├─ CommunityMarkers
│  │  └─ ObservationDetailAnchors
│  ├─ SimulationPresentationRoot
│  │  ├─ SettlementMarkers
│  │  ├─ RegionMarkers
│  │  ├─ RouteRoot
│  │  ├─ CargoPresentationRoot
│  │  └─ ThreatPresentationRoot             [placeholder]
│  └─ CameraAnchors
│
├─ SettlementInteriorRoot                   [기존 유지]
│  ├─ Terrain
│  ├─ Roads
│  ├─ Districts
│  │  ├─ FarmDistrict
│  │  ├─ TownDistrict
│  │  ├─ MarketDistrict
│  │  ├─ StorageDistrict
│  │  ├─ LogisticsDistrict
│  │  ├─ ResidentialDistrict
│  │  ├─ GarrisonDistrict                   [placeholder]
│  │  └─ GateDistrict                       [placeholder]
│  ├─ WorldObjects
│  ├─ NpcVisuals
│  ├─ CargoVisuals
│  ├─ InteractionAnchors
│  └─ CameraAnchors
│
├─ CameraSystem
├─ Lighting
├─ PersistentUI
│  ├─ 공통WorldHud
│  ├─ 공공관측Panel
│  ├─ HarvestLot판로Panel
│  ├─ 물류이동Panel
│  ├─ 턴마감Panel
│  ├─ 주문·배달PanelRoot                    [초기 비활성]
│  └─ 공통오류상태Panel
│
├─ DevelopmentFixtureRoot                   [개발 빌드에서만 명시적 활성]
└─ EventSystem
```

`통합세계CompositionRoot`는 실제 경제 상태를 소유하지 않는다. 두 authority client의 생명주기와 Presentation 연결만 조율한다.

---

## 5. 한 Scene 안에서 지켜야 할 authority 경계

### 5.1 공공데이터 관찰

```text
운영 서버 공개 API
  → CommunityWorldMapRepository
  → LoadPublicWorldMapUseCase
  → 공공세계지도 Scene state
  → 공공관측PresentationRoot
```

- 읽기 전용이다.
- source·기준 시각·공개 범위·오류 상태를 보존한다.
- Simulation fixture로 실패를 숨기지 않는다.
- marker 선택은 Simulation Task나 WorldTick을 만들지 않는다.

### 5.2 Simulation 플레이

```text
Simulation Server
  → session·WorldTick·WorldRevision
  → Decision Preview
  → 명시적 Confirm
  → Task·Effect
  → canonical session 재조회
  → SimulationWorldShell snapshot 교체
```

- Scene과 GameObject는 session을 발급하지 않는다.
- Treasury·Labor·Stock·FoodSecurityDays를 UI에서 계산하지 않는다.
- 차량 도착, NPC animation, 상자 개수로 Task 완료나 수량을 확정하지 않는다.
- 서버 연결 실패를 Fixture 성공으로 대체하지 않는다.

### 5.3 공통 Presentation 상태

한 Scene에서 공통으로 소유해도 되는 것은 다음뿐이다.

- 현재 관찰 규모
- 선택한 World·Settlement·District·Object stable ID
- 열려 있는 Panel
- camera focus와 breadcrumb
- 마지막 성공 snapshot의 표시 상태
- 로딩·오류·재시도 상태

이 상태는 save 원장이나 업무 authority가 아니다.

---

## 6. Scene 대신 Prefab·Object·상태로 분리할 단위

현재 진행 중인 `OBJ-2 + OBJ-3`을 통합 Scene보다 먼저 완료하는 이유는, 연구 Scene 전체를 복사하지 않고 검증된 개별 Object만 안전하게 이식하기 위해서다.

### 6.1 Scene으로 남기지 않을 단위

| 현재 보이는 기능 | 통합 뒤 단위 |
| --- | --- |
| 감자 수확 상자 | wrapper Prefab + Object descriptor + Lot binding |
| Hub 입고 Gate | wrapper Prefab + socket + Cargo/Task binding |
| 음식 픽업 인계 상자 | wrapper Prefab + 공개범위·handoff state binding |
| 판로 선택 화면 | 공통 Canvas의 Panel Prefab + Coordinator |
| 물류 이동 화면 | Cargo visual + route Presenter + Panel |
| 턴 카드 모판 | 카드 catalog 검증 Scene은 유지, 실제 사용은 Panel Prefab |
| 통합 모판·전시관 | Object 연구·승격 검증 Scene으로 유지 |

### 6.2 Object 공통 구조

```text
업무ObjectRoot
├─ IdentityView
├─ BindingView
├─ InteractionAnchor
├─ StatePresentationRoot
├─ SocketRoot
└─ VisualRoot
   └─ Synty 또는 primitive prefab
```

- 업무 record stable ID, 모판 Object stable ID, Scene Placement stable ID를 구분한다.
- prefab path·GUID·좌표는 Unity catalog와 placement가 관리한다.
- 서버와 shared contract에는 semantic role, zone, binding, Gate와 evidence만 둔다.
- Scene에 이식하는 단위는 업무 Story 전체가 아니라 독립 Object다.

### 6.3 Scene Placement

각 Object는 `SceneObjectPlacement`를 통해 다음을 명시해야 한다.

- 대상 Scene stable ID
- 대상 District·socket
- Object stable ID
- Placement stable ID와 revision
- 위치·회전·크기
- binding 대상 record 종류
- 필요한 Presenter
- O6 승격 Runtime test와 Game View evidence

독립 Preview를 통과했다는 이유만으로 `SimulationWorldShell`에 자동 배치하지 않는다.

---

## 7. 사용자 경험 흐름

### 7.1 실행과 진입

```text
게임 실행
  → SimulationWorldShell 로드
  → 공통 HUD와 카메라 준비
  → 공공 관측과 Simulation 연결 상태를 각각 초기화
  → World Map 표시
```

공공 API가 실패해도 Simulation이 정상이라면 Simulation World는 계속 볼 수 있어야 한다. 반대로 Simulation 연결이 실패해도 공개 가능한 공공 관측을 볼 수 있다. 단, 어느 한쪽의 Fixture로 다른 쪽의 실패를 성공처럼 보이지 않는다.

### 7.2 World에서 정착지로

```text
World Map
  → 정착지 marker 선택
  → Settlement Overview
  → Farm District
  → HarvestLot Object
```

전환 동안 같은 `SimulationSession`, `WorldTick`, `WorldRevision`, `SettlementStableId`를 유지한다. Scene을 다시 로드하지 않는다.

### 7.3 수확물 판로

```text
HarvestLot 선택
  → [공동 출하]
     [온라인 직접 판매]
     [비축 보관]
     [외부 교역 준비]
  → Preview
  → 비용·노동·기간·수량·위험·예상 영향 표시
  → Confirm
  → Task 생성
  → 턴 마감 또는 명시적 진행
  → Effect 반영
  → 같은 Scene의 창고·시장·화물 표현 갱신
```

### 7.4 주문·운송·배달

개별주문, 같이주문, 음식배달과 화물운송도 새 Scene으로 이동하지 않는다.

- 시장 Object를 선택하면 주문·재고 Panel을 연다.
- Hub·Cargo를 선택하면 운송·인수 Panel을 연다.
- 음식점·픽업 Object를 선택하면 조리·인계 계보를 연다.
- Task가 진행되면 같은 World의 marker, actor, Cargo visual과 HUD가 snapshot을 다시 받아 갱신된다.

---

## 8. 기존 Scene의 장기 역할

### 8.1 `SimulationWorldShell.unity`

- 역할: 유일한 실제 플레이 Scene
- Build Settings: 최종 활성
- 변경 방식: Builder로 재현 가능하게 유지
- 권위: 없음. 서버 snapshot을 조율·표현

### 8.2 `WorldBootstrapScene.unity`

- 현재 역할: 공공 세계지도 단독 실행과 회귀 검증
- 통합 뒤 역할: 공개지도 Surface의 검증 Scene
- Build Settings: 통합 완료 뒤 비활성
- 삭제 시점: 장기간 회귀 가치가 사라지고 Prefab/Presenter 검증으로 대체된 뒤 별도 승인

### 8.3 `턴카드모판.unity`, `통합모판전시관.unity`

- 역할: 카드와 Object의 연구·검증·승격
- 실제 플레이: 아님
- Build Settings: 비활성
- 산출물: 검증된 catalog, wrapper Prefab, descriptor, placement 후보

### 8.4 primitive Scene

- 역할: Logistics·Market primitive의 독립 회귀 또는 이전 호환
- 통합 뒤: 사용 참조를 조사해 Preview Prefab으로 대체 가능하면 `Experiments - 연구`로 이동 후보
- 즉시 삭제: 금지

### 8.5 `Experiments - 연구` 29개

- 역할: vertical slice, Builder 재생성, 시각 비교, 자료 연구
- 실제 플레이: 아님
- Build Settings: 계속 제외
- 정책: 신규 gameplay 기능 때문에 연구 Scene을 계속 복제하지 않는다. 새로운 연구 질문이나 독립 시각 검증이 있을 때만 만든다.

---

## 9. 우선순위 재정렬

현재 공식 다음 작업인 `OBJ-2 + OBJ-3`을 훼손하지 않고 단일 Scene 수렴을 그 뒤에 연결한다.

```text
완료
  WORLD-SHELL-0
  SETTLEMENT-SCENE-0
  WORLD-SETTLEMENT-NAV-0
  SETTLEMENT-VISUAL-BASE-0
  SETTLEMENT-INTERACTION-0
  LOGISTICS-MOVEMENT-1
  TURN-CARD-UI-1B
  OBJ-0 + OBJ-1

현재
  OBJ-2 + OBJ-3

다음
  SINGLE-SCENE-0
  → SINGLE-SCENE-1
  → SINGLE-SCENE-2
  → SINGLE-SCENE-3
  → SINGLE-SCENE-4
```

### 9.1 `OBJ-2` — 이식 가능한 Object 만들기

구현:

- 첫 세 Object wrapper Prefab
- descriptor·Visual Catalog
- socket validator
- prefab path가 contract로 새지 않는지 검증

완료 Gate:

- 감자 수확 상자, Hub 입고 Gate, 음식 픽업 인계 상자가 Scene 없이도 identity·binding·socket을 검증할 수 있다.
- `.meta` GUID와 기존 Story evidence를 보존한다.

### 9.2 `OBJ-3` — 독립 Preview

구현:

- Object별 Preview bay
- 상태별 VisualRoot 표현
- missing binding·socket·catalog entry 차단

완료 Gate:

- O5 Object Preview를 통과한다.
- 아직 `SimulationWorldShell` 배치 성공으로 주장하지 않는다.

### 9.3 `SINGLE-SCENE-0` — Scene 역할 대장과 생성 동결

구현:

- 35개 Scene을 `PlayEntry`, `Verification`, `Research`, `LegacyCandidate`로 분류
- 새 gameplay Scene 생성 금지 규칙
- `SimulationWorldShell` canonical Scene stable ID
- Build Settings 전환 전 회귀 목록

완료 Gate:

- 모든 Scene에 역할과 유지 이유가 있다.
- 어느 Scene이 실제 플레이인지 문서·Editor 메뉴·검증에서 하나로 식별된다.

### 9.4 `SINGLE-SCENE-1` — 공공 세계지도 Surface 이식

구현:

- `WorldBootstrapScene`의 marker·상태 View·상세 Panel을 Prefab 또는 재현 가능한 Builder 단위로 추출
- `SimulationWorldShell/WorldMapRoot/공공관측PresentationRoot`에 배치
- 공공 API composition과 Simulation composition 분리
- 공통 loading/error Panel에서 두 연결 상태를 별도로 표시

완료 Gate:

- Shell 한 Scene에서 공공 marker와 Simulation 정착지 marker를 구분해 볼 수 있다.
- public repository가 Simulation snapshot을 만들거나 바꾸지 않는다.
- 한쪽 실패가 다른 쪽 Fixture 성공으로 숨겨지지 않는다.

### 9.5 `SINGLE-SCENE-2` — 실제 플레이 기능 수렴

구현:

- 판로·물류·턴 카드 Panel을 Persistent UI의 기능 Panel로 정리
- OBJ-2/3 첫 Object를 명시적 Scene Placement로 배치
- 개별주문·같이주문·음식배달은 우선 읽기 projection과 entry action만 연결
- World→Settlement→Object→Panel→Back 흐름 통일

완료 Gate:

- 감자 HarvestLot 판로와 턴 마감이 Scene 이동 없이 이어진다.
- Cargo·Market·Storage 표현이 같은 canonical snapshot revision으로 갱신된다.
- 각 Object는 O6 Scene 승격 evidence를 갖는다.

### 9.6 `SINGLE-SCENE-3` — Build Settings 전환

구현:

- `SimulationWorldShell.unity`만 활성화
- 개발·제품 실행 모두 canonical Scene으로 시작
- `WorldBootstrapScene` 직접 실행 회귀 test 유지
- Builder와 CI의 Scene path 단일화

완료 Gate:

- 새 실행에서 다른 Scene을 수동으로 열지 않아도 핵심 플레이에 진입한다.
- 빌드와 Play Mode에서 같은 entry를 사용한다.
- 서버 모드와 명시적 Fixture 모드가 구분된다.

### 9.7 `SINGLE-SCENE-4` — 회귀 비교와 연구 Scene 정리

구현:

- 기존 vertical slice와 통합 Scene의 상태·시각 parity 비교
- 대체 완료 Scene을 `Experiments - 연구` 아래 유지 또는 별도 승인 뒤 정리
- 중복 Builder·primitive 참조 정리

완료 Gate:

- 기존 검증 근거와 GUID·catalog 참조가 보존된다.
- 삭제할 Scene이 있다면 대상, 대체 증거, 복구 가능성을 별도 변경으로 명시한다.

---

## 10. `SimulationWorldShellBuilder` 재편 방향

현재 Builder는 Scene 전체를 새로 만들고 각 기능을 같은 파일에 조립하는 좋은 출발점이다. 다만 기능이 늘수록 하나의 큰 method 집합으로 굳지 않도록 다음처럼 분리한다.

```text
SimulationWorldShellBuilder
  ├─ BuildShellStructure
  ├─ BuildWorldMapSurface
  ├─ BuildSettlementSurface
  ├─ BuildPersistentUI
  └─ ValidateIntegratedScene

공공세계지도SurfaceBuilder
정착지DistrictBuilder
판로PanelBuilder
물류PanelBuilder
턴카드PanelBuilder
SceneObjectPlacementBuilder
```

분리된 Builder는 서로 Scene을 열고 저장하지 않는다. 활성 `SimulationWorldShell`의 지정 root에만 조립하며, 최종 저장과 전체 validator 실행은 상위 Builder가 한 번 수행한다.

Builder가 기존 root를 무조건 파괴하고 재생성하기보다 다음 정책을 가져야 한다.

1. stable root와 수동 배치 보존 범위를 명시한다.
2. 생성 소유 object에는 생성 marker와 revision을 둔다.
3. 중복 stable ID와 socket을 fail-closed 처리한다.
4. 미등록 수동 object를 조용히 삭제하지 않는다.
5. Scene 저장 전 wiring·missing script·catalog·binding을 검증한다.

---

## 11. 검증 계획

### 11.1 코드·계약

- 공공 관측과 Simulation assembly·repository 경계
- Preview 무변경과 Confirm 뒤 canonical 재조회
- session·Tick·revision 불변 및 증가 규칙
- Object·Lot·Cargo·Placement stable ID 중복 차단
- prefab path·GUID·GameObject 이름의 server/shared contract 누출 차단
- Fixture와 server mode의 명시적 분리

### 11.2 Scene 구조

- Build Settings 활성 Scene 정확히 1개
- canonical entry path 검사
- hierarchy와 필수 root 검사
- missing script·prefab·shader·catalog entry 검사
- EventSystem·Camera·Audio Listener 중복 검사
- 비활성 Panel과 placeholder가 gameplay capability처럼 노출되지 않는지 검사

### 11.3 Play Mode 골든 패스

최소 한 번에 다음을 검증한다.

```text
실행
  → World Map
  → 공공 marker 관찰
  → 정착지 진입
  → Farm focus
  → 감자 HarvestLot 선택
  → 판로 Preview
  → Confirm
  → 턴 마감
  → WorldTick·revision 증가
  → Storage 또는 Cargo 상태 갱신
  → World Map 복귀
```

전 과정에서 Scene load/unload가 없어야 하고 같은 session stable ID가 유지되어야 한다.

### 11.4 Game View 증거

실제 Scene이 바뀌는 각 단계는 다음 대표 PNG를 남긴다.

- 통합 World Map
- Settlement Overview
- HarvestLot 선택과 판로 Preview
- 턴 마감 뒤 상태 변화
- 물류 또는 Storage 반영

Scene View는 hierarchy 확인의 보조 증거이며 Game View를 대신하지 않는다.

### 11.5 성능 기준선

한 Scene에 모든 root가 존재해도 항상 모두 활성화할 필요는 없다.

- 현재 Perspective 밖의 고비용 actor·FX·Panel 비활성
- 반복 Object pooling 후보 측정
- renderer·material·draw call 기준선
- Windows와 목표 모바일 해상도에서 CPU·GPU·메모리 측정
- 측정 전 Addressables나 additive Scene을 선제 도입하지 않음

---

## 12. 금지 사항

- 새 기능마다 `.unity` Scene을 추가한다.
- `WorldBootstrapScene`의 repository를 Simulation authority로 재사용한다.
- `SimulationWorldShell`이 운영 상태와 Simulation 상태를 하나의 DTO로 합친다.
- Scene 전환 또는 root 활성화가 session·Tick·재고를 초기화한다.
- District마다 manager·save·Simulation을 만든다.
- 연구 Scene 전체 hierarchy를 통합 Scene에 복사한다.
- 업무 Story 전체를 하나의 prefab으로 만든다.
- Synty prefab 이름과 path를 Domain·server contract에 저장한다.
- NPC·차량 animation 완료로 업무 Task를 확정한다.
- 상자나 renderer 개수로 실제 수량을 계산한다.
- Builder가 사용자의 수동 배치나 다른 작업을 무표시로 삭제한다.
- 통합 검증 전에 현재 Scene을 삭제하거나 `.meta` GUID를 잃는다.
- 화면 통합을 이유로 Simulation·Interpretation 규칙을 Unity 안에 복제한다.

---

## 13. 첫 통합 완료의 정의

다음 조건을 모두 만족해야 “하나의 통합된 `.unity`에서 게임이 돌아간다”고 판정한다.

1. Build Settings의 활성 Scene은 `SimulationWorldShell.unity` 하나다.
2. 게임 실행 뒤 다른 Scene을 수동으로 열 필요가 없다.
3. 공공 세계지도와 Simulation 정착지를 같은 Scene에서 관찰할 수 있다.
4. World Map→Settlement→District→Object→Panel→Back이 Scene 로드 없이 동작한다.
5. 감자 HarvestLot의 판로 Preview·Confirm·Task·턴 마감·Effect가 한 플레이 흐름으로 이어진다.
6. Cargo·Storage·Market 표현이 canonical snapshot 재조회 결과로 갱신된다.
7. 공공 관측과 Simulation의 authority, 오류, Fixture 상태가 구분된다.
8. 기존 연구·모판 Scene은 실제 플레이가 아니라 검증 자산으로 분류된다.
9. 새로 이식한 Object는 O6 Scene Placement 검증과 Game View evidence를 갖는다.
10. 관련 EditMode·PlayMode·builder validation과 실제 Game View 골든 패스를 통과한다.

---

## 14. 최종 권고

현재는 통합 Scene을 새로 만드는 시점이 아니라, 이미 만들어진 `SimulationWorldShell`을 **유일한 플레이 무대로 완성하는 시점**이다.

즉시 순서는 다음이 바람직하다.

```text
OBJ-2
  → wrapper Prefab·descriptor·Visual Catalog·socket validator

OBJ-3
  → 독립 Object Preview

SINGLE-SCENE-0
  → Scene 역할 대장·신규 gameplay Scene 생성 동결

SINGLE-SCENE-1
  → WorldBootstrap의 공공지도 Surface를 Shell 안에 이식

SINGLE-SCENE-2
  → 검증된 Object와 기존 판로·물류·턴 Panel을 한 플레이 흐름으로 수렴

SINGLE-SCENE-3
  → Build Settings를 SimulationWorldShell 하나로 전환

SINGLE-SCENE-4
  → 기존 Scene과 parity 확인 뒤 연구 자산 정리
```

이 순서는 현재 진행 중인 Object 모듈화를 우회하지 않으면서도, 그 결과가 또 다른 전시 Scene으로 끝나지 않고 실제 하나의 World에 들어오게 한다. 장기적으로 바뀌는 것은 Scene이 아니라 server·Simulation snapshot, 사용자의 Perspective, Object state와 Presentation이어야 한다.
