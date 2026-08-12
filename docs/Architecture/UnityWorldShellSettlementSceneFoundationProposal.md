# Unity Simulation World Shell·정착지 Scene 기반 재정렬 제안서

> 상태: 구현 완료 — `WORLD-SHELL-0 + SETTLEMENT-SCENE-0 + SETTLEMENT-ECONOMY-1 + WORLD-SETTLEMENT-NAV-0 + BRANCH-ADAPTER-1 + SETTLEMENT-VISUAL-BASE-0 + SETTLEMENT-INTERACTION-0`, 다음 Gate는 `LOGISTICS-MOVEMENT-1`
>
> 작성일: 2026-08-10
>
> 상위 Simulation 순서: [Unity 실시간 정착지 경제·영지 경영·분쟁 Simulation 재정렬 제안서](UnityRealtimeTerritoryManagementConflictSimulationProposal.md)
>
> 기존 Unity 순서: [Unity World 구현 현황과 우선순위](UnityWorldImplementationPriority.md)
>
> 권위 경계: [D-027 운영 서버와 게임 Simulation 서버를 물리 분리한다](../AI/DECISIONS.md#d-027-운영-서버와-게임-simulation-서버를-물리-분리한다)

## 1. 결정 요약

공식 Simulation 권위 순서는 바꾸지 않는다.

```text
SIM-WORLD-0
  → DECISION-WORK-0
  → SAVE-REPLAY-0
  → SETTLEMENT-CORE-1
  → HARVEST-IMPACT-1 + STORAGE-1
  → SETTLEMENT-ECONOMY-1
```

위 Simulation 권위 Gate와 같은 snapshot의 관찰 규모를 연결하는 navigation, 판로 adapter, 정착지 visual base와 HarvestLot 선택부터 authoritative Effect가 반영된 snapshot 재조회까지 잇는 `SETTLEMENT-INTERACTION-0`을 완료했다. 첫 경제 playable 절단선은 닫혔으며 다음 활성 Gate는 기존 Cargo/Journey를 공통 WorldTick Task에 합류시키는 `LOGISTICS-MOVEMENT-1`이다.

다만 Unity Presentation이 개별 기능 Scene의 집합으로 굳어지는 것을 막기 위해, 다음 서버 Gate를 완성하기 전에 한 번만 범위를 제한해 장기 재사용 골격을 먼저 만든다.

```text
완료
  1. WORLD-SHELL-0
  2. SETTLEMENT-SCENE-0
  3. SETTLEMENT-ECONOMY-1
  4. WORLD-SETTLEMENT-NAV-0
  5. BRANCH-ADAPTER-1

현재 다음
  6. SETTLEMENT-VISUAL-BASE-0
  7. SETTLEMENT-INTERACTION-0
```

`WORLD-SHELL-0`과 `SETTLEMENT-SCENE-0`은 **읽기 전용 Presentation 기반**이다. 이 두 단계에서는 경제 값을 계산하거나 판로 Confirm을 실행하지 않고, 하나의 Simulation snapshot을 World Map과 정착지 내부에서 동일하게 관찰할 수 있는 구조만 증명한다.

## 2. 왜 지금 두 Presentation 단계를 먼저 두는가

기존 Farm·Town·City·Hub, 감자 생애주기, 판로 카드와 Journey는 좁은 vertical slice로 이미 많은 사실을 증명했다. 그러나 각각의 Scene과 wrapper를 계속 추가하면 다음 문제가 생긴다.

- World Map, 정착지, Farm, Market이 서로 다른 게임 상태처럼 보일 수 있다.
- Scene 진입 때 fixture가 다시 생성되어 WorldTick·재고·선택이 초기화될 수 있다.
- 기능마다 별도 manager와 bootstrap이 생겨 하나의 Simulation World에 수렴하기 어려워진다.
- 정착지 기능이 늘어난 뒤 WorldShell을 만들면 기존 Scene의 생명주기와 카메라를 다시 뜯어야 한다.

반대로 서버 경제 구현을 멈추고 대규모 World 미술부터 만드는 것도 맞지 않는다. 그래서 이번 선행 범위는 다음 두 가지에만 한정한다.

1. 같은 snapshot을 공유하는 영구 Presentation shell
2. 이후 시설을 끼워 넣을 수 있는 첫 정착지 blockout과 district socket

이 범위를 넘는 카드 상호작용, 경제 효과, NPC 생활, 물류 완료, 성벽·군단·전투는 각 권위 Gate가 준비된 뒤에만 연결한다.

## 3. 현재 프로젝트에서 확인한 재사용 기반과 충돌 지점

### 3.1 그대로 재사용할 기반

| 현재 기반 | 재사용 책임 |
| --- | --- |
| `DioramaTopDownCameraRig`와 World·Zone·Object Focus | World Map→Settlement→District→Object 관찰 이동 |
| `WorldVisualCatalog`, semantic `VisualKey`, `VisualRoot` | District와 시설 외형을 vendor prefab과 분리 |
| Farm·Town·Hub·City Composition과 `ThreeRegionHubJourney` | 정착지와 외부 Region의 공간 구성 재료 |
| Farm 24개 Composition Set | 첫 FarmDistrict의 교체 가능한 시각 모듈 |
| FARM-3·HARVEST-CHOICE-1·COOP-1·DIRECT-1 | 이후 Interaction adapter의 검증된 실행 명세 |
| CARGO-1·JOURNEY-1·HUB-1/2 | 이후 WorldTick 물류와 route 표현의 실행 명세 |
| Simulation session·save/replay·Settlement snapshot | WorldShell이 읽을 장기 권위와 재현 기준 |

### 3.2 이름과 책임을 분리해야 하는 기존 요소

현재 Unity의 `WorldBootstrapScene`은 공공 관측을 보여주는 `PublicWorldMapPresenter`와 `CommunityWorldMapRepository` 중심의 공개지도 surface다. 이 Scene을 Simulation World authority나 정착지 경영 bootstrap으로 조용히 바꾸지 않는다.

구분은 다음처럼 고정한다.

| 대상 | 책임 | 금지 |
| --- | --- | --- |
| 기존 `WorldBootstrapScene` | 공공데이터·커뮤니티 World Map Presentation | Simulation session 소유, 정착지 경제 초기화 |
| 신규 `SimulationWorldShell` | Simulation snapshot 관찰 규모와 Presentation surface 조율 | 경제 계산, Tick 자동 진행, 운영 데이터 혼합 |
| `WorldManager`·기존 bootstrap use case | 기존 generic World data 로딩 계약 | 새 Simulation singleton으로 암묵적 승격 |

공공지도에서 마커·카메라·surface 패턴은 재사용할 수 있지만, 그 repository나 DTO를 Simulation World의 authority로 재사용하지 않는다.

## 4. 두 개의 작업 열과 의존성

### 4.1 Authority Track

```text
SETTLEMENT-ECONOMY-1
  → BRANCH-ADAPTER-1
  → LOGISTICS-MOVEMENT-1
  → MARKET-CONSUMPTION-1
  → FOOD-SECURITY-1
  → ARMY-SUPPLY-1
  → INVASION-1
```

이 열은 서버가 실제 값을 계산하고 저장하는 순서다. 재정·노동·재고·비축·시장 공급·Task·Effect의 최종 권위는 여기에만 있다.

### 4.2 Presentation Foundation Track

```text
WORLD-SHELL-0
  → SETTLEMENT-SCENE-0
  → WORLD-SETTLEMENT-NAV-0
  → SETTLEMENT-VISUAL-BASE-0
  → SETTLEMENT-INTERACTION-0
```

이 열은 같은 authoritative snapshot을 다른 관찰 규모로 보여주는 순서다. 화면 전환, 카메라, 선택, District 배치와 VisualKey를 담당한다.

### 4.3 합류 규칙

| Presentation 단계 | 필요한 Authority Gate | 이유 |
| --- | --- | --- |
| `WORLD-SHELL-0` | `SIM-WORLD-0`, `SAVE-REPLAY-0` | session·Tick·revision을 초기화하지 않고 유지해야 함 |
| `SETTLEMENT-SCENE-0` | `SETTLEMENT-CORE-1` | 정착지·District·Facility stable ID를 Scene이 만들지 않아야 함 |
| `WORLD-SETTLEMENT-NAV-0` | 위와 동일 | 선택과 관찰 규모만 바꾸며 snapshot은 유지 |
| `SETTLEMENT-VISUAL-BASE-0` | `SETTLEMENT-CORE-1` | 공간 역할은 읽되 경제 결과는 아직 표현만 함 |
| `SETTLEMENT-INTERACTION-0` | `SETTLEMENT-ECONOMY-1`, `BRANCH-ADAPTER-1` | Preview·Confirm·Task·Tick·Effect를 실제 권위 원장과 왕복해야 함 |

따라서 Shell과 blockout은 지금 만들 수 있지만, 판로 카드의 최종 폐루프는 경제 원장과 adapter보다 앞설 수 없다.

## 5. 재정렬된 통합 구현 순서

| 순서 | 단계 | 이번 단계의 산출물 | 완료 Gate 뒤 다음으로 넘어가는 이유 |
| ---: | --- | --- | --- |
| 0 | 문서·계약 기준 고정 — 현재 | shell 책임, view state, hierarchy, 금지 사항, 검증 계획 | 구현 중 Scene authority가 생기는 것을 방지 |
| 1 | `WORLD-SHELL-0` — 완료 | 신규 Simulation 전용 shell, WorldMap/Settlement root, 공통 HUD·카메라·선택 상태 | 같은 snapshot을 두 surface가 공유함을 먼저 증명 |
| 2 | `SETTLEMENT-SCENE-0` — 완료 | 정착지 blockout, 8개 District socket, Road·anchor | 경제 기능 없이도 장기 공간 뼈대를 고정 |
| 3 | `SETTLEMENT-ECONOMY-1` — 완료 | 300kg 단일 allocation, cash·labor·stock 적용과 중복 차단 | 화면이 보여줄 실제 결과를 서버에 완성 |
| 4 | `WORLD-SETTLEMENT-NAV-0` — 완료 | World→Settlement→District→Object→Back | 동일 revision과 선택 규칙을 실제 카메라 경험으로 검증 |
| 5 | `BRANCH-ADAPTER-1` — 완료 | 조합·직판·보관·외부 교역 준비를 서버 Preview 입력과 후보 Task로 연결 | 기존 판로 slice와 서버 권위의 의미를 연결 |
| 6 | `SETTLEMENT-VISUAL-BASE-0` | 환경·도로·건물·수목·낮밤의 1차 미술 | Interaction 전 기준 Game View 가독성을 확보 |
| 7 | `SETTLEMENT-INTERACTION-0` | Lot 선택→Preview→Confirm→Task→Tick→Effect | 첫 정착지 경제 playable 폐루프 완성 |
| 8 | `LOGISTICS-MOVEMENT-1` 이후 | Cargo·시장·식량·군량·분쟁 확장 | 같은 shell과 snapshot에 기능을 누적 |

순서 1~2는 하나의 짧은 Presentation milestone으로 묶는다. 이 milestone이 완료되면 추가 미술·NPC·시설 구현으로 퍼지지 않고 반드시 순서 3의 서버 경제 Gate로 돌아간다.

## 6. WORLD-SHELL-0 최소 설계

### 6.1 책임

`SimulationWorldShell`은 다음만 책임진다.

- 현재 Simulation session과 마지막 성공 snapshot을 읽는다.
- `SessionStableId`, `WorldTick`, `WorldRevision`, `SettlementStableId`를 같은 관찰 문맥으로 유지한다.
- WorldMap과 SettlementInterior Presentation root 중 하나를 활성화한다.
- 현재 관찰 규모와 stable-ID 선택을 보존하거나 명시적 규칙에 따라 해제한다.
- 공통 카메라, 조명, HUD, 입력 surface의 생명주기를 조율한다.
- snapshot 갱신 실패 시 마지막 성공 화면과 실패 상태를 구분해 표시한다.

다음은 책임지지 않는다.

- Treasury·Labor·FoodSecurityDays 계산
- WorldTick 자동 진행
- Preview·Confirm Command 발행
- Lot·Cargo·Task 완료 확정
- 운영 서버 상태 fallback
- District별 별도 manager 생성

### 6.2 첫 구현은 root 전환으로 제한한다

첫 버전에서는 Unity Scene을 실제로 unload/load하지 않는다. 하나의 신규 `SimulationWorldShell` Scene 안에 `WorldMapRoot`와 `SettlementInteriorRoot`를 두고 활성 root만 전환한다.

이 방식의 목적은 다음 세 사실을 가장 작게 증명하는 것이다.

1. 전환 전후 `SessionStableId·WorldTick·WorldRevision`이 같다.
2. 카메라와 선택은 Presentation 규칙에 따라 유지되거나 해제된다.
3. surface가 비활성화돼도 Simulation snapshot은 다시 생성되지 않는다.

콘텐츠가 커져 additive Scene loading이 필요해지면 shell 뒤의 surface loader만 교체한다. snapshot과 view state 계약은 바꾸지 않는다.

### 6.3 런타임 상태 모델

권장하는 최소 Presentation 상태는 다음 의미를 가진다.

```text
SimulationWorldViewState
  ObservationScaleCode
    WorldMap | Settlement | District | Object
  SessionStableId
  SnapshotWorldRevision
  SnapshotWorldTick
  SelectedSettlementStableId?
  SelectedDistrictStableId?
  SelectedObjectStableId?
  CameraFocusKey?
```

이 모델은 GameObject reference나 prefab path를 저장하지 않는다. save package에도 Presentation 카메라를 Simulation authority처럼 넣지 않는다. 필요하면 별도 사용자 Presentation preference로 저장한다.

### 6.4 선택 보존 규칙

- World Map에서 정착지를 선택하면 `SelectedSettlementStableId`를 설정한다.
- Settlement 진입 뒤 같은 snapshot에 해당 정착지가 있으면 선택을 유지한다.
- District나 Object가 새 snapshot에 없으면 하위 선택만 해제한다.
- Settlement가 사라지거나 권한·scenario 경계가 바뀌면 Settlement 이하 선택을 모두 해제하고 World Map으로 돌아간다.
- GameObject destroy 여부로 canonical 선택 존재를 판정하지 않는다.
- 카메라 전환 실패는 Simulation 선택이나 Tick을 바꾸지 않는다.

## 7. 첫 Scene hierarchy

```text
SimulationWorldShell
├─ ShellRuntimeRoot
├─ WorldMapRoot
│  ├─ TerrainRoot
│  ├─ TerritoryRoot
│  ├─ SettlementMarkers
│  ├─ RegionMarkers
│  ├─ RouteRoot
│  ├─ CargoPresentationRoot
│  ├─ ThreatPresentationRoot        [inactive placeholder]
│  └─ CameraAnchors
├─ SettlementInteriorRoot
│  ├─ Terrain
│  ├─ Roads
│  ├─ Districts
│  │  ├─ FarmDistrict
│  │  ├─ TownDistrict
│  │  ├─ MarketDistrict
│  │  ├─ StorageDistrict
│  │  ├─ LogisticsDistrict
│  │  ├─ ResidentialDistrict
│  │  ├─ GarrisonDistrict           [presentation placeholder]
│  │  └─ GateDistrict               [presentation placeholder]
│  ├─ WorldObjects
│  ├─ NpcVisuals
│  ├─ CargoVisuals
│  ├─ InteractionAnchors
│  └─ CameraAnchors
├─ CameraSystem
├─ Lighting
├─ PersistentUI
└─ EventSystem
```

District root는 서버 Entity를 새로 만드는 선언이 아니다. `SETTLEMENT-CORE-1` snapshot의 District/Facility를 배치하는 Presentation socket이다. Garrison과 Gate는 기능이 없음을 명확히 표시한 blockout이며, 주둔군·방어·성문 Command를 암시하지 않는다.

## 8. 첫 정착지 blockout 기준

첫 정착지는 완성된 도시가 아니라 경제 인과를 읽는 축소판이다.

```text
                 Forest / Hill

              Gate placeholder
                     │
              Garrison placeholder

 Farm ───── Settlement Center ───── Market
                     │
                 Residential
                     │
                   Storage
                     │
              Logistics District
                     │
                    Road
                     │
               External City marker
```

최소 콘텐츠는 다음으로 제한한다.

| 범위 | 수량/상태 |
| --- | --- |
| Faction·Territory·Settlement | 각 1 |
| Farm Region·Town Region·City Region·Hub | 각 1 marker 또는 공간 socket |
| Farm | 2개 blockout, 첫 playable은 1개만 활성 |
| Market·Storage·Logistics | 각 1 |
| Residential | 1 cluster |
| Garrison·Gate | 기능 없는 placeholder 각 1 |
| Route | Farm→Settlement→Hub/City를 읽는 최소 연결 |
| 첫 Lot | 감자 HarvestLot 300kg, 읽기 전용 선택 후보 |

빈 공간은 실패가 아니라 확장 socket이다. 다만 inactive 기능은 HUD·카드·오브젝트 상태에서 placeholder임을 숨기지 않는다.

## 9. 카메라와 관찰 규모

관찰 규모는 별도 Simulation이 아니라 같은 snapshot의 Perspective다.

```text
World Map
  → Settlement Overview
  → District Focus
  → Object Focus
  → Back
```

기존 `DioramaTopDownCameraRig`의 World·Zone·Object focus를 재사용하고 다음 매핑을 둔다.

| 관찰 규모 | 기존 Focus | 의미 |
| --- | --- | --- |
| World Map | World | Territory·Settlement·Route 전체 |
| Settlement Overview | World 또는 Settlement 전용 anchor | 첫 정착지 전체 |
| District | Zone | Farm·Market·Storage 등 공간 역할 |
| Object | Object | HarvestLot·시설·Cargo·NPC Presentation |

전환 animation은 카메라 표현일 뿐 WorldTick 경과가 아니다. World Map으로 돌아가는 행위도 save reload가 아니다.

## 10. HUD 최소 범위

첫 Shell HUD는 snapshot의 값을 읽기만 한다.

- `GameDate`
- Pause / Speed의 **표시와 입력 socket**
- `Treasury`
- `Labor Available / Reserved`
- `Market Food Supply`
- `Reserve Food`
- `FoodSecurityDays`
- `Active Tasks`

`WORLD-SHELL-0`에서는 Pause/Speed 버튼이 존재하더라도 Simulation Tick을 발행하지 않는 disabled 또는 explicit-not-connected 상태로 둘 수 있다. 가짜로 시간이 흐르는 것처럼 표시하지 않는다. 실제 입력 연결은 command와 revision 처리가 준비된 별도 Gate에서 수행한다.

## 11. Synty와 Composition 적용 규칙

적용 순서는 고정한다.

```text
Simulation stable ID / Presentation role
  → District socket
  → semantic VisualKey
  → VisualRoot
  → catalog entry
  → Synty prefab
```

예를 들어 `GateDistrict`는 `gate.main`, `wall.main`, `tower.guard` 같은 semantic key를 먼저 갖고, 그 뒤 적합한 prefab을 연결한다. prefab 이름·GUID·`Assets/` 경로를 Simulation DTO, Domain, save package에 넣지 않는다.

기존 Farm·Town·City·Hub wrapper와 CompositionSet을 우선 배치한다. 새 asset이 없으면 primitive blockout을 사용하고, 그 사실을 placeholder 상태로 남긴다.

## 12. 단계별 최소 구현과 완료 Gate

### 12.1 WORLD-SHELL-0 — 완료

구현:

- 신규 Simulation 전용 Scene과 composition root
- shell runtime과 read-only snapshot port
- `WorldMapRoot`, `SettlementInteriorRoot`
- 공통 camera·lighting·HUD root
- observation scale과 stable-ID selection state
- 마지막 성공 snapshot과 오류 상태 분리

완료 Gate:

- World Map과 Settlement root를 왕복해도 같은 session·Tick·revision을 표시한다.
- 전환만으로 session 생성, Tick, Confirm, 재고 변경이 일어나지 않는다.
- 기존 공개지도 `WorldBootstrapScene` 동작과 build 설정을 임의로 대체하지 않는다.
- plain C# test로 root 전환, 선택 보존·해제, snapshot 불변을 검증한다.
- 실제 Scene을 변경했으므로 Play Mode Game View와 변경 기록을 남긴다.

구현 결과:

- 별도 `Assets/Ssalddel/Scenes/SimulationWorldShell.unity`와 재현 가능한 builder를 추가했다.
- engine-independent 상태기계가 WorldMap·Settlement 전환 동안 같은 snapshot instance와 Tick 12·Revision 12를 유지한다.
- HUD는 명시적 `SimulationFixture` 값을 읽고 Pause·Speed는 `미연결` 비활성으로 표시한다.
- 기존 공공데이터 `WorldBootstrapScene`과 Build Settings는 변경하지 않았다.

### 12.2 SETTLEMENT-SCENE-0 — 완료

구현:

- 하나의 SettlementInterior blockout
- Farm·Town·Market·Storage·Logistics·Residential district
- Garrison·Gate placeholder
- Roads, interaction anchor, camera anchor
- 기존 wrapper와 semantic VisualKey 연결

완료 Gate:

- 3/4 Overview 한 장에서 생산→저장→운반→판매→생활→방어 예정 공간이 읽힌다.
- District마다 manager나 독립 Simulation을 추가하지 않는다.
- 오브젝트 수·상자 수로 수량을 계산하지 않는다.
- Scene 생성 코드와 저장 Scene의 hierarchy가 validator에서 일치한다.
- 최종 Play Mode Game View와 Console·EditMode 결과를 남긴다.

구현 결과:

- Farm·Town·Market·Storage·Logistics·Residential 6개 활성 District와 Garrison·Gate placeholder 2개를 같은 Settlement root에 배치했다.
- `SimulationWorldDistrictView`는 stable ID·semantic VisualKey·placeholder 여부만 가진 Presentation socket이다.
- 전용 5/5, 기존 Diorama camera 4/4, Unity 기본 EditMode assembly 전체 44/44가 통과했다.
- 최종 World Map·Settlement Interior Play Mode Game View와 Console 오류 0건을 `C:\Users\user\ssalddel\Assets\Documentation\Changes\2026-08-10-world-shell-settlement`에 남겼다.

### 12.3 WORLD-SETTLEMENT-NAV-0

구현:

- Settlement marker 선택
- Settlement Overview 진입
- District·Object focus와 Back
- 새 snapshot reconcile 뒤 선택 복원/해제

완료 Gate:

- 모든 전환에서 같은 snapshot identity를 유지한다.
- 삭제된 stable ID의 하위 선택만 결정적으로 해제한다.
- Scene/GameObject reference가 선택 권위가 아니다.

구현 결과:

- Settlement marker 1개, District 8개와 감자 HarvestLot 1개를 stable-ID target으로 연결했다.
- Object→District→Settlement→World Map Back과 breadcrumb·선택 강조를 추가했다.
- 전용 EditMode 8/8과 Play Mode Tick 12·Revision 12 유지, Console 오류 0건을 확인했다.

### 12.4 BRANCH-ADAPTER-1 — 완료

구현:

- 네 판로 choice와 workflow code를 서버 계약과 동일하게 정렬
- 기존 Decision·HarvestLot stable ID, revision, 상품·수량·단위와 source lineage 보존
- 서버 HarvestDisposition Impact Preview 입력 envelope 구성
- deterministic 후보 Task ID·type·input Lot·output candidate 구성
- 비용·노동·기간·시설·Effect는 envelope에서 생성하지 않고 서버 재계산 경계 표시

완료 Gate:

- 조합·직판·보관·외부 교역 준비 네 분기가 서버 workflow와 정확히 대응한다.
- 결정되지 않은 Lot, 잘못된 actor stable ID와 canonical workflow 불일치를 거부한다.
- adapter 호출은 정착지 원장, Cargo, 판매와 외부 실행을 변경하지 않는다.

구현 결과:

- engine-independent `HarvestDispositionBranchAdapter`와 Preview request·Task candidate 모델을 추가했다.
- .NET 판로·adapter 집중 테스트 15/15와 Unity package 전체 328/328, Unity EditMode adapter 6/6과 기본 assembly 전체 55/55를 통과했다.
- 서버의 기존 `SimulationHarvestDispositionImpactTests` 23/23으로 네 분기 정책 계약과 계속 일치함을 확인했다.
- Scene과 Game View는 변경하지 않았으며 기존 3버튼 실험 카드는 후속 `SETTLEMENT-INTERACTION-0`에서 4개 action surface로 교체한다.

### 12.5 SETTLEMENT-VISUAL-BASE-0

구현:

- foreground·midground·background
- 도로·수목·지형·건물 배치
- 기존 Day/Night Presentation 재사용
- semantic VisualKey 기반 Synty 교체

완료 Gate:

- 한 장의 Overview에서 district 관계가 텍스트 없이도 대체로 읽힌다.
- asset showcase가 아니라 정착지 경제 공간으로 보인다.
- 기능 없는 Gate·Garrison은 placeholder 상태를 유지한다.

구현 결과:

- 기존 District socket·도로·navigation target을 유지하고 Farm/Urban/Environment catalog의 semantic VisualKey로 45개 이상의 vendor-prefab wrapper를 연결했다.
- 감자 경작지·HarvestLot, Market 판매대, Storage/Logistics pallet·cargo box·van, Residential 건물·수목을 한 SettlementInterior에 배치했다.
- Gate·Garrison은 기능 없는 primitive placeholder로 유지했다.
- 오후 15:00 시간대는 Presentation에만 적용하며 Simulation Tick 12·Revision 12를 바꾸지 않는다.
- Unity 기본 EditMode 57/57, 구형 판로 카드 호환 4/4, Play Mode Console 오류 0건과 Overview·Farm·Market Game View를 확인했다.

### 12.6 SETTLEMENT-INTERACTION-0

선행 조건:

- `SETTLEMENT-ECONOMY-1` 완료
- `BRANCH-ADAPTER-1` 완료

구현:

```text
HarvestLot 선택
  → 공동 출하 / 직접 판매 / 보관 / 외부 교역 준비
  → Preview
  → Confirm
  → Task
  → WorldTick
  → Effect
  → 새 snapshot reconcile
```

완료 Gate:

- Preview 전후 snapshot은 같다.
- Confirm과 Tick은 expected revision을 검증한다.
- 같은 300kg은 하나의 allocation만 가진다.
- 정착지 HUD와 Lot 카드가 새 snapshot을 다시 읽어 함께 갱신된다.
- animation·NPC·차량은 Task나 Cargo 도착을 확정하지 않는다.

구현 결과:

- 300kg HarvestLot 선택 시 조합·직판·비축·외부 교역 준비 네 action과 정책 Preview를 표시한다.
- Production authority repository는 공식 session GET, 판로 impact Preview·Confirm, Tick 경로와 expected revision을 사용한다.
- Game View의 `SimulationFixtureAuthority`는 운영 fallback이 아닌 test double이며 Preview 무변경, Confirm 예약, 완료 Tick Effect 경계를 재현한다.
- 비축 경로는 revision 12·Tick 12 Preview, revision 13 예약, revision 14·Tick 13 Effect 적용과 HUD reconcile을 확인했다.
- 집중 8/8, 기본 EditMode 65/65, 서버 판로 영향 23/23, .NET adapter 6/6과 Play Mode Console 오류 0건을 통과했다.

## 13. 검증 전략

### 13.1 코드와 계약

- Shell 상태 전환은 engine-independent plain C# test를 우선한다.
- 같은 snapshot identity의 WorldMap/Settlement projection 결과를 비교한다.
- 선택 stable ID가 존재할 때 유지되고 사라졌을 때만 해제되는지 검증한다.
- root 전환이 Simulation Command port를 호출하지 않는지 검증한다.
- fixture를 사용하면 `SimulationFixture`임을 type·HUD·문서에 명시하고 운영 fallback으로 사용하지 않는다.

### 13.2 Scene과 시각 증거

- hierarchy validator
- missing script·prefab·shader 검사
- Unity EditMode 집중 test와 관련 전체 test
- Play Mode에서 World Map, Settlement Overview, Farm/Market/Storage focus 캡처
- 전환 전후 같은 GameDate·WorldTick·revision이 보이는 증거
- `docs/Changes` 기록과 대표 Game View PNG

Scene View는 배치 확인의 보조 증거일 뿐 최종 Game View를 대신하지 않는다.

### 13.3 서버 합류 뒤

- `SETTLEMENT-ECONOMY-1` 집중 test
- save/replay 동일 hash
- Unity adapter contract test
- Preview 무변경, Confirm·Tick revision, allocation 보존식
- 새 snapshot을 통한 HUD·Lot·Task 동시 reconcile

## 14. 명시적 비목표

이번 두 선행 단계에서는 다음을 하지 않는다.

- 실제 판로 Confirm·Tick 연결
- 재정·노동·재고·FoodSecurityDays 계산
- NPC 생활 schedule
- 차량 이동으로 Cargo 완료
- 여러 정착지와 territory 확장
- 성벽·성문 gameplay
- 군단·침공·공성·ConflictView
- 대규모 streaming 또는 Addressables 전환
- District별 Scene·manager
- 기존 Scene 삭제·이름 변경·build 진입점 교체
- 새 Synty Pack에 맞춘 Domain 변경

## 15. 중단 조건

다음 중 하나가 발생하면 시각 확장을 멈추고 경계를 바로잡는다.

- WorldMap과 SettlementInterior가 서로 다른 fixture/session을 만든다.
- surface 전환으로 WorldTick·revision·재고가 초기화된다.
- shell이 Treasury·Labor·FoodSecurityDays를 계산한다.
- District GameObject가 canonical stable ID를 새로 발급한다.
- 기존 공공지도 repository가 Simulation authority로 승격된다.
- Garrison·Gate placeholder가 실제 방어 capability처럼 노출된다.
- 하나의 District 추가마다 manager·Scene·save가 하나씩 늘어난다.
- Shell milestone 중 상호작용·NPC·전투 범위가 계속 확장된다.

## 16. 최종 권고

다음 구현은 아래처럼 진행한다.

```text
[WORLD-SHELL-0 완료]
  → [SETTLEMENT-SCENE-0 완료]
  → 시각 확장 중단
  → [SETTLEMENT-ECONOMY-1 완료]
  → [WORLD-SETTLEMENT-NAV-0 완료]
  → [BRANCH-ADAPTER-1 완료]
  → SETTLEMENT-VISUAL-BASE-0
  → SETTLEMENT-INTERACTION-0
```

이 재정렬은 Simulation 우선순위를 폐기한 것이 아니다. 이미 준비된 session·save/replay·정착지 graph를 Unity의 장기 Presentation 골격이 정확히 받아볼 수 있도록 최소 shell과 공간 socket을 먼저 놓는 것이다.

첫 두 단계가 성공했다는 기준도 풍부한 콘텐츠가 아니다. **같은 Simulation snapshot을 World Map과 Settlement Interior가 공유하고, 한 번 정한 공간·카메라·선택 계약 위에 이후 경제·물류·생활·군량·분쟁을 계속 끼워 넣을 수 있는가**가 유일한 기준이다.
