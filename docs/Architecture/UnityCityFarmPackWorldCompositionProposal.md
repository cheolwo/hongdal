# Unity 입체 탑다운 City·Farm World 구성 제안

## 1. 제안의 중심

Ssalddel Unity World는 **따뜻하고 풍성한 Farm, 저밀도 생활권 Town, 고밀도 물류·시장 City가 각각 독립적으로 발전하고 사람과 화물차량이 세 Region 사이를 오가는 입체 탑다운 World**로 구성한다. 공급망 기능을 일렬로 늘어놓은 도식이 아니라, 각 Region의 생활과 경영이 독립적으로 읽히면서 이동망을 통해 생산·운송·물류·판매·공동수령이 연결되어야 한다.

완전한 수직 탑다운 지도도 아니고, NPC 눈높이의 3인칭 World도 아니다. 기본 카메라는 건물의 높이, 밭의 이랑, 차량의 이동과 작업 공간의 깊이를 읽을 수 있는 3/4 시점이다.

```text
                         Camera
                            ↘
                      pitch 45~55°

 [Farm Region] ── 집하 ─┐
       │                 ▼
       │ 생활이동 [Regional Logistics Hub] ── 배송 ─→ [City Region]
       │                 ▲                               ▲
 [Town Region] ── 집배송 ┘────── 주민·통근 이동 ─────────┘
```

구매·import한 `POLYGON Farm`은 생산 공간과 첫 화면의 정서적 중심을, `POLYGON Town`은 단독주택·정원·동네상점·생활배송으로 저밀도 생활권을, `POLYGON City`는 고밀도 도시 생활과 유통 공간을 맡는다. 각 Pack은 Region 내부에서 주된 정체성을 유지하고, 혼합 asset은 Farm↔Town·Town↔City 사람 Gate와 Farm/Town→Hub·Hub→City 화물 Gate의 진입부에서만 제한적으로 사용한다.

먼저 전체 World의 배경과 주요 object anchor를 배치하고 카메라에서 읽히는 공간 구조를 확정한다. 그 뒤 FARM-2 밭갈이부터 코드 vertical slice로 돌아간다.

### 1.1 초기 Visual Prototype의 품질 목표

WORLD 단계는 단순히 prefab이 깨지지 않고 배치되는지를 확인하는 단계가 아니다. 구매한 세 Pack이 제공하는 shader·material·palette·FX를 적극 활용해, 실제 Game View를 처음 보는 사람도 완성될 게임의 분위기와 공간적 매력을 이해할 수 있는 수준의 대표 화면을 먼저 만든다.

```text
안전한 asset 연결
  + Synty shader/material의 시각적 특성 유지
  + 통합된 조명·그림자·ambient·Global Volume
  + 제한적이지만 효과적인 FX
  + 입체 탑다운 카메라 구도
  + 전경·중경·배경이 겹치는 풍성한 생활 경관
  = 실제로 들어가 보고 싶은 농장·도시 경영 World Prototype
```

초기부터 primitive 수준으로 재질을 평탄화하거나 Android 최저 사양을 가정해 시각 품질을 과도하게 줄이지 않는다. 먼저 PC Game View에서 목표 품질을 확정하고, 이후 실제 profiling 근거로 mobile quality tier를 분리한다.

이 시각적 목표는 Data·Simulation·서버를 Synty에 종속시키지 않는다. 적극적인 그래픽 활용은 `VisualRoot → VisualKey → Presentation catalog → prefab/material/FX` 경계 안에서만 수행한다.

### 1.2 Visual Target — POLYGON Farm Showcase 수준의 살아 있는 농장 World

초기 Farm World의 목표 화면은 단순한 기능 배치도가 아니다. 이번 제안에 함께 제공된 POLYGON Farm Showcase 네 장을 다음과 같이 목표 화면 레퍼런스로 사용한다.

| 레퍼런스 | Ssalddel 적용 장면 | 핵심 품질 |
| --- | --- | --- |
| Photo 1 | Farm Yard / Produce Stand Object Focus | 농산물·상자·간판·NPC가 겹치는 생활 밀도와 생산→판매 연결 |
| Photo 2 | Farm 작업 Simulation Zone Focus | Barn·Silo·Tractor·차량·농부가 함께 만드는 실제 작업장 느낌 |
| Photo 3 | Farm 작업 Simulation Object Focus | 반복 작물 사이를 통과하는 Tractor와 전경 작물의 깊이 |
| Photo 4 | World / Farm Zone Overview | 여러 작물밭·농로·건물·수목이 하나의 넓은 농장으로 읽히는 구성 |

Farm 대표 화면에는 다음 요소를 한두 개씩 고립해 놓지 않고, 서로 겹치고 이어지는 하나의 농촌 경관으로 구성한다.

- 반복되는 감자·옥수수·밀·해바라기 또는 일반 작물밭
- 농로와 흙길, 울타리와 Farm gate
- Barn·Farmhouse·Silo·풍차와 급수 시설
- Tractor·Trailer·농기계와 작업 중인 농부 NPC
- 나무·건초·상자·농기구·농산물 같은 생활 소품
- 생산과 출하를 연결하는 Produce Stand와 Farm Yard
- 전경·핵심 작업 공간·중경·배경이 겹치는 레이어드 구도

플레이어가 UI와 Card를 모두 숨긴 첫 Game View를 보았을 때 `데이터를 표시하기 위해 asset을 배치한 Scene`이 아니라 `이미 운영되고 있는 농장을 위에서 내려다보고 있다`고 느끼는 것을 Visual Prototype의 핵심 품질 기준으로 한다.

## 2. 플레이어가 보게 될 World

기본 화면은 독립적으로 발전하는 Farm·Town·City 세 Region과 그 사이의 이동망을 조망하는 전략 화면이다.

```text
┌──────────────────────────────────────────────────────────────┐
│ [Farm Region] ── 집하 ─┐           ┌── 배송 ─→ [City Region] │
│ 밭·시설·Farm Yard      ▼           │          마트·공동주택  │
│       │        [Regional Logistics Hub]                      │
│       │        입고·검수·보관·출고                           │
│       ▼                  ▲                                   │
│ [Town Region] ── 집배송 ─┘──── 주민·통근도로 ───────────────▶│
│ 주택·중심가·지역 배송                                       │
│                                                              │
│ 세 Region은 독립 발전하고 화물은 중간 Hub를 기본 경유함     │
└──────────────────────────────────────────────────────────────┘
```

플레이어는 화면을 이동하고 확대하면서 다음 질문을 공간에서 따라간다.

```text
왜 진열대가 비었는가?
  → 마트 후방재고가 부족하다
  → 물류센터 입고가 늦었다
  → 농장 출하량이 부족했다
  → 일부 감자밭의 작업이 완료되지 않았다
```

반대로 주민 공동주문의 변화도 도시에서 농장 방향으로 추적할 수 있다.

```text
공동주택 확정 주문 증가
  → 마트 공급 검토
  → 물류센터 처리 계획
  → 농장 출하와 다음 작기 검토
```

플레이어는 한 Region의 내부 발전만 보는 것이 아니라 사람과 화물차량을 선택해 Region 사이의 Journey를 따라갈 수 있다. 이동 actor의 도착은 Presentation이며 주문·계약·입고·검수·수령을 자동 확정하지 않는다.

## 3. 카메라는 핵심 인터페이스다

### 3.1 기본 카메라

권장 기본값은 다음 범위에서 실제 Game View로 조정한다.

| 항목 | 제안 |
| --- | --- |
| Projection | Perspective |
| Pitch | 45~55도 하향 |
| Yaw | 기본 대각 방향, 90도 단위 회전 |
| Field of View | 낮은 왜곡의 25~35도 후보 |
| 이동 | 지면 평면 Pan |
| 확대 | 제한된 Zoom |
| 회전 | 자유 회전 대신 90도 단계 회전 |
| 기준점 | 선택 object 또는 Zone anchor |

완전한 Orthographic은 보드게임처럼 정돈되지만 건물 깊이와 거리감이 약하다. 기본은 좁은 FOV의 Perspective를 사용해 입체감을 유지한다. 필요하면 가장 먼 전략 조망 단계만 Orthographic에 가까운 시각으로 보정한다.

기본 구도는 단순히 전체 Zone을 프레임 안에 넣는 데서 끝나지 않는다. 화면 가까이의 큰 작물·울타리·상자 일부를 전경에 걸치고, 감자밭·Tractor·Produce Stand를 핵심 공간에 두며, Farmhouse·다른 작물밭·풍차를 중경에, Barn·Silo·수목을 배경에 배치한다.

```text
[수목 / Silo / Barn]                              배경

       [Farmhouse]   [다른 작물밭]   [풍차]       중경

   [Tractor] ─ [감자밭 6×6] ─ [Produce Stand]    핵심 공간

 [해바라기] [울타리] [상자] [농기구]             전경

                         Camera ↗
```

전경 object는 깊이와 현장감을 만들되 선택 object나 업무 route를 완전히 가리지 않는다. 필요한 경우 기존 foreground occlusion 정책으로 투명화한다.

### 3.2 세 단계 조망

#### World Overview

- 농장부터 공동주택까지 전체 공급망을 본다.
- 차량 이동, Zone 상태 marker와 큰 흐름만 표시한다.
- 개별 타일 수치나 작은 카드 문구는 숨긴다.

#### Zone Focus

- 농장, 물류센터, 마트 같은 한 Zone으로 확대한다.
- 작업 영역, NPC, cargo와 주요 상태 카드를 표시한다.
- 다른 Zone은 낮은 detail이나 silhouette로 유지한다.

#### Object Focus

- 밭 타일, 감자 상자, Dock, 진열대 또는 대표 NPC를 선택한다.
- 선택 outline, detail card와 가능한 행동을 표시한다.
- 카메라가 object를 중앙에 강제로 고정하기보다 주변 업무 관계도 함께 남긴다.

```text
World Overview
    ↓ Zone 선택
Zone Focus
    ↓ Object 선택
Object Focus + Concept/Status/Reason/Action Card
    ↓ Back
이전 조망 단계 복귀
```

### 3.3 가림 처리

입체 탑다운에서 건물과 높은 object가 업무 공간을 가리지 않도록 명시적 occlusion 정책을 둔다.

- 마트·물류센터 선택 시 지붕과 전면 상부를 fade하거나 cutaway한다.
- 카메라와 선택 object 사이의 큰 나무·벽은 일시적으로 투명화한다.
- 공동주택 내부 전체를 항상 노출하지 않고 공동수령 공간만 별도 anchor로 둔다.
- 멀리 있는 건물의 내부 UI와 작은 text는 숨긴다.
- Scene View에서 잘 보이는지가 아니라 최종 Game View에서 읽히는지를 기준으로 한다.

```text
일반 조망
  마트 외관 + 지붕

마트 Zone Focus
  지붕 fade
  → 후방재고·진열대·관리자 desk 표시
```

## 4. 실제 구매 Asset 상태

Unity 프로젝트 `C:\Users\user\ssalddel`에서 Farm·Town·City Pack의 실제 Import를 확인했다.

| Pack | 경로 | Prefab 구성 |
| --- | --- | --- |
| POLYGON City | `Assets/Synty/PolygonCity` | 건물 76, 캐릭터 9, 환경 65, 소품 174, 차량 9 |
| POLYGON Farm | `Assets/Synty/PolygonFarm` | 건물 17, 캐릭터·부착물 14, 환경 67, FX 11, 식물 173, 소품 166, 차량 11 |
| POLYGON Town | `Assets/Synty/PolygonTown` | Prefab 702개, Material 25개; 세부 Composition 분류는 Town 조사 문서 기준 |

세 Pack의 대표 건물·캐릭터·작물은 현재 프로젝트에서 Synty 계열 shader와 palette를 사용한다. Farm·Town·City character FBX는 Humanoid rig를 제공한다.

감자 playable에 필요한 Farm asset도 실제로 존재한다.

- 평탄 토양과 Dirt Row center/end/skirt 변형
- 감자 식물 S/M/L
- 낱개 감자와 감자 group
- 감자 상자
- Farmer Male·Female·Old
- Tractor, Plough, Planter, Harvester와 trailer
- Barn, Farmhouse, Garage, Greenhouse, Silo, Produce Stand

현재 Synty 폴더에는 standalone `.anim`·`.controller`가 없고 각 Pack character FBX도 embedded clip import가 꺼져 있다. Town character prefab 8개에는 대응 asset을 찾지 못한 controller GUID가 남아 있다. 따라서 외형과 Humanoid rig는 재사용하되 걷기·밭갈이·파종·수확 동작은 검증된 clip의 리타기팅과 별도 animation adapter가 필요하다.

## 5. Pack별 공간 책임

### Farm Pack — 생산 World

- 토양, 이랑, 밭 경계와 농로
- 감자 생육 단계와 수확물
- 농장주택, Barn, 온실, Silo와 출하대
- 농부와 농장 작업자 외형
- 트랙터, 쟁기, 파종기, 수확기
- 관수, 비, 먼지와 수확 FX 후보
- 여러 종류의 반복 작물, 수목·울타리·건초·상자·농기구를 이용한 비권위 환경 경관

Farm의 모든 시각 object가 stable ID를 가질 필요는 없다. 실제 상태·선택·상호작용·cargo lineage를 담당하는 object만 Ssalddel wrapper와 `VisualRoot`에 연결하고, 반복 작물·수목·울타리·건초 같은 환경 object는 장면 구성과 성능 정책 아래 별도 Environment root에서 관리한다.

### City Pack — 유통·생활 World

- 도심 도로와 보행 공간
- 물류센터 외곽과 차량 접근 공간
- 도심마트 외관과 내부 판매 공간
- 공동주택과 공동수령 생활권
- 관리자, 공동주택 대표와 도시 운송자 외형
- van과 도시 차량

### Town Pack — 저밀도 생활·지역 연결 World

- 단독주택·차고·정원과 뒷마당
- Town Neighborhood와 Main Street
- 동네상점·놀이터·생활 커뮤니티 공간
- 생활택배 거점과 지역 배송차량
- Farm 방문과 City 통근·시장 방문을 표현할 주민·대표 외형
- Farm↔Town 생활도로와 Town↔City 지역 간선도로의 Gate

### 세 Pack의 연결부

| 연결 지점 | 출발·도착 | 대표 이동 | 공유 의미 |
| --- | --- | --- | --- |
| Farm↔Town 생활도로 | Farm Yard·Produce Stand↔Town Neighborhood | 농부·주민·Pickup | 방문·직판·생활배송 Presentation |
| Town↔City 사람 간선도로 | Town Main Street↔City Residential·Market | 주민·대표·Bus | 통근·시장방문 Presentation |
| Farm/Town→Hub inbound | Origin Gate↔Hub Inbound Dock | 집하·집배송 화물차량·Cargo | Cargo/handoff·Inbound Journey |
| Hub→City outbound | Hub Outbound↔City Distribution Gate | 배송차량·pallet·Cargo | allocation·Outbound Journey |
| City 내부 공급 | Distribution Gate↔Market↔Residential Pickup | Van·pallet·주민 | Inventory·fulfillment Presentation |

Farm 상자나 City pallet이 canonical cargo가 되는 것은 아니다. Ssalddel World의 cargo stable ID가 기준이고, 각 Region의 View가 같은 cargo를 적절한 외형으로 표현한다. NPC와 차량 도착도 업무 상태를 자동 확정하지 않는다.

## 6. 탑다운 맵 배치 원칙

### 6.1 세 Region과 안쪽 Gate를 고정한다

기본 회전에서 Farm을 북서쪽, City를 북동쪽, Town을 남서쪽, Regional Logistics Hub를 Town과 City 사이 중앙~동쪽에 둔다. 세 Region의 바깥쪽은 독립 확장에 사용하고, 서로 마주보는 안쪽 면에는 Gate와 지역 간 Route를 고정한다.

```text
 Farm Region ── 집하 ─┐
      │                ▼
      │ 생활도로  Regional Logistics Hub ── 배송 ─→ City Region
      │                ▲                                ▲
 Town Region ── 집배송 ┘────── 사람·통근도로 ───────────┘
```

90도 회전 후에도 세 Region, 각 Gate와 도로 연결이 읽혀야 한다. 건물을 예쁘게 정면 배치하는 것보다 route와 작업 anchor가 가려지지 않는 것이 우선이다.

### 6.2 Region 내부와 Corridor를 구분한다

- 각 Region은 바닥 재질, 도로, fence와 높이 차이로 독립된 정체성을 가진다.
- Farm 내부는 기능별 빈칸으로 분리하지 않고 밭·농로·울타리·식생·작업 소품으로 자연스럽게 연결한다.
- 카메라가 Region을 선택했을 때 해당 영역과 주요 Gate가 한 화면에 들어오는 여백을 둔다.
- Farm↔Town 생활도로와 Town↔City 통근도로, Farm/Town→Hub inbound와 Hub→City outbound route는 장식 배경이 아니라 사람·차량 Journey를 보여주는 공간으로 남긴다.
- 혼합 Composition은 Region 전체가 아니라 Gate·진입부에 집중한다. Town은 Farm과 City 사이의 장식 Transition이 아니라 독립 생활권으로 유지한다.
- 빈 공간은 무조건 채우지 않지만, 남겨 둘 때는 차량 회전·작업 안전·밭 접근·시야 확보처럼 공간의 이유가 읽혀야 한다.

구체적인 Region footprint·Gate·Route·Journey 구조는 [Farm·Town·City 3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md), 허브 입고·검수·보관·출고와 다중 origin 기준은 [지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md)를 따른다.

### 6.3 주요 object는 실루엣으로 식별한다

- 농장: 밭의 반복 패턴, Barn과 Silo
- 출하장: Produce Stand, 감자 상자와 농장 차량
- 물류센터: 큰 facility, Dock와 pallet
- 마트: Shop facade, 후방과 진열대
- 공동주택: Apartment와 pickup marker

Overview에서는 text를 읽지 않아도 각 Zone을 구분할 수 있어야 한다.

## 7. Scene과 Hierarchy 구조

모든 asset을 한 대형 Scene에 넣지 않는다. 공통 카메라·조명·Runtime과 Zone 표현을 분리한다.

```text
ThreeRegionWorldShell
├─ IsometricCameraRig
├─ GlobalLightingAndVolume
├─ SharedPresentationCanvas
├─ WorldSelectionCoordinator
├─ RegionAndJourneyFocusCoordinator
├─ RegionRegistry
├─ FarmRegionRoot
│  ├─ FarmProduction
│  ├─ FarmYard
│  └─ FarmRegionGates
├─ TownRegionRoot
│  ├─ TownNeighborhood
│  ├─ TownMainStreet
│  └─ TownRegionGates
├─ RegionalLogisticsHubRoot
│  ├─ InboundDockDistrict
│  ├─ InspectionAndStorageDistrict
│  ├─ OutboundStagingDistrict
│  └─ HubGates
├─ CityRegionRoot
│  ├─ UrbanLastMileDistribution
│  ├─ UrbanMarket
│  ├─ ResidentialCommunity
│  └─ CityRegionGates
├─ InterRegionRouteRoot
│  ├─ FarmTownLocalRoute
│  ├─ TownCityPassengerRoute
│  ├─ FarmHubInboundRoute
│  ├─ TownHubCollectionRoute
│  └─ HubCityOutboundRoute
├─ StatefulJourneyRoot
├─ AmbientTrafficRoot
└─ RuntimeCompositionRoot
```

초기에는 복잡한 streaming framework를 만들지 않는다. 한 Integration Scene 안에서 세 Region을 명시적 root로 분리하고 Gate·Route·camera를 먼저 검증한다. 계약이 안정되면 World Shell과 Farm·Town·City Region Scene을 additive 구조로 분리할 수 있다.

## 8. 감자 6×6 밭의 입체 표현

기존 `FarmSoilTileCellView`가 좌표·stable ID·선택과 상태를 계속 소유한다. Farm prefab은 하위 `VisualRoot`에만 배치한다.

Simulation이 6×6이라는 이유로 화면에 보이는 농장 전체를 6×6 실험장처럼 만들지 않는다. 6×6 감자밭은 더 넓은 농촌 풍경 안에 자연스럽게 포함된 실제 Simulation 대상이며, 주변의 다른 작물밭·울타리·농로·Barn·Farmhouse·Silo·풍차·수목·농기계·소품은 공간의 규모와 생활감을 만드는 환경이다.

```text
농장 전체 풍경
├─ 옥수수밭 / 밀밭 / 해바라기 / 일반 작물밭  ← Environment
├─ Barn / Farmhouse / Silo / 풍차
├─ Tractor / Trailer / 농기계
├─ Produce Stand / Farm Yard
├─ 나무 / 울타리 / 건초 / 상자 / 농기구
└─ 실제 Simulation 감자밭 6×6
   └─ tile 상태·stable ID·선택·작업 연결
```

| 상태 | 입체 표현 |
| --- | --- |
| Untilled | 평탄 Dirt |
| TillingPreview | Dirt + selection outline/바닥 overlay |
| Tilled | Dirt Row center/end 조합 |
| Seeded | 이랑 유지 + seed 상태 marker |
| Sprout | Potato S |
| Growing | Potato M |
| Mature | Potato L |
| HarvestReady | Potato L + readiness marker |
| Harvested | 빈 이랑 + 감자 group 또는 출하 상자 |

S/M/L prefab 이름을 생육 상태 코드로 저장하지 않는다.

```text
CropStageCode
  → FarmPresentationProjector
  → FarmVisualKey
  → FarmVisualCatalog
  → prefab instance
```

36개 타일은 멀리서 하나의 밭 패턴으로 읽히고, 확대하면 각 tile의 상태와 선택이 구분되어야 한다. 모든 tile 위에 text를 띄우지 않고 선택 tile과 주의 tile만 marker를 표시한다.

## 9. 공간과 Concept Card의 역할 분담

탑다운 World는 상태와 흐름을 보여주고, Card는 의미와 근거를 설명한다.

```text
공간
  어디에서 무엇이 움직이는가

Concept Card
  이 개념은 무엇인가

Status Card
  지금 수량과 상태는 무엇인가

Reason Card
  왜 이런 상태인가

Action Card
  무엇을 Preview할 수 있는가
```

Overview에서는 큰 상태 marker만 표시한다. Zone Focus에서 관련 card deck을 열고 Object Focus에서 상세 근거와 Action을 제공한다. 카드는 화면 중앙을 상시 가리지 않고 가장자리 panel 또는 선택 object 주변 anchor에 배치한다.

## 10. 코드와 Asset 경계

```text
Data / Simulation / Shared World
  → Perspective WorldState
  → PresentationModel
  → View wrapper
  → VisualKey adapter
  → City/Farm prefab
```

Presentation 전용 catalog 후보:

- `UrbanVisualCatalog`
- `FarmVisualCatalog`
- `TransitionVisualCatalog`

catalog에는 prefab reference, scale, rotation, pivot/anchor와 material variant만 둔다. 상품 수량, 토양 상태, 권한, 작업 가능 여부, source lineage와 revision은 넣지 않는다.

원본 Synty prefab은 수정하지 않는다. Ssalddel wrapper의 `VisualRoot` 아래에 scene instance 또는 project prefab variant를 배치한다.

### 10.1 반복 가능한 농장 풍경 Composition Set

단일 Synty prefab과 최종 Farm Zone 사이에 반복 배치 가능한 `농장풍경CompositionSet` 계층을 둔다. 세트는 원본 prefab을 중첩 참조하는 Presentation 전용 prefab이며, 환경 경관과 실제 상태 object가 연결될 socket을 분리한다.

```text
Synty 단일 prefab
  → 농장풍경CompositionSet A/B/C
  → Farm Zone 배치
  → 상태가 필요한 socket에만 VisualRoot 연결
```

첫 library는 다음 8종을 각각 A/B/C 세 변형으로 제공한다.

| 풍경 세트 | 주된 역할 | 상태 연결 socket 후보 |
| --- | --- | --- |
| 감자밭 두렁 | 실제 6×6 감자밭 주변 울타리·식생·경계 | 실제감자밭 |
| 혼합 작물밭 | 옥수수·밀·채소가 만드는 환경 농지 | 없음 |
| 헛간 작업마당 | Barn·농기계·건초·도구 작업 군집 | 농부·차량·화물 |
| 농기계 대기장 | Garage/Shelter와 Tractor attachment 군집 | 차량·농기계 |
| 농산물 직판장 | Produce Stand·상자·간판·판매 소품 | 농부·화물·상호작용 |
| 수확물 집하장 | pallet crate·상자·grain bag·wheelbarrow | 화물·차량 |
| 농로 교차로 | 교차·T·곡선 농로와 표지·울타리·식생 | 차량 |
| 수목 완충지 | 수목·과수·꽃·풀·바위의 전경·배경 군집 | 없음 |

세트 이름과 변형은 한국어 업무·경관 의미로 탐색하고, 개별 vendor 파일명은 builder와 catalog의 Presentation 내부에만 남긴다. 세트 prefab 전체에 stable ID를 부여하지 않으며, 실제 감자밭·농부·차량·화물처럼 canonical 또는 Simulation 상태를 표현하는 대상만 socket 아래에서 별도 View와 stable ID를 가진다.

초기 구현은 `농장풍경CompositionSetBuilder`가 실제 Synty prefab 83종을 조합해 24개 prefab, catalog asset과 library preview Scene을 재현 가능하게 생성한다. 최종 Farm World에는 catalog에서 필요한 세트만 선택해 배치하고, 같은 세트를 반복할 때는 A/B/C 변형·90도 회전·전후 반전을 조합하되 업무 route와 camera occlusion을 다시 검증한다.

현재 8종 풍경 세트 아래에서 시설하우스·밭·논을 실제 단지처럼 조립하기 위한 하위 module 기준은 [Unity Farm 시설하우스·밭·논 단지 Modular Composition 설계](UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md)를 따른다. 시설하우스와 밭은 실제 Farm prefab으로 구성 가능하지만, 논은 Rice·담수면·논둑·농수로 asset이 없어 전용 Visual Gate 전까지 `논 단지 Blockout`으로만 다룬다.

### 10.2 반복 가능한 도시 풍경 Composition Set 후보

City Pack도 단일 건물·도로·소품을 최종 Zone에 직접 나열하지 않고, `도시풍경CompositionSet` 후보 계층을 둔다. 첫 후보는 농촌도시 전환 상가·도시 진입 교차로·물류센터 입고장·물류센터 적치출하장·도심마트 앞마당·도심마트 매장 내부·먹거리 상점 골목·공동주택 생활마당·공동수령 장소·사무공공정보관 앞·대중교통 정류장·도시 공원 쉼터 12종이며 각 A/B/C를 검토한다.

현재는 조사·설계 상태이고 City 세트 prefab, catalog, builder와 preview Scene을 구현하지 않았다. 실제 335개 prefab inventory, 세트별 source family·socket·반복 규칙·한계와 구현 전 Gate는 [Unity POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)를 기준으로 한다.

금지 사항:

- Data contract에 Synty prefab 이름 저장
- prefab 이름으로 상품·작물 stable ID 생성
- Animator event나 NPC 도착으로 작업 완료 Command 실행
- Farm demo Scene script를 Simulation authority로 사용
- asset 로드 실패를 Simulation fixture로 대체
- 카메라 선택으로 서버 권한 확대

## 11. 시각적 통일 기준

### Synty 그래픽 적극 활용

- 초기 Visual Prototype에서는 City/Farm의 실제 shader·material·palette를 최종 렌더링 기준으로 사용한다.
- 원본 asset의 색 분할, 표면 대비와 low-poly silhouette를 불필요하게 단색 primitive처럼 평탄화하지 않는다.
- Pack의 demo Scene을 제품 Scene으로 복사하지는 않지만, lighting·material·FX 구성이 좋은 부분은 분석해 Ssalddel 공통 환경에 맞게 재구성한다.
- World Overview, Farm, Logistics, Market의 대표 화면은 단순 asset 나열이 아니라 카메라 구도와 전경·중경·배경이 있는 게임 화면처럼 구성한다.
- 상태 marker와 Card가 World 그래픽을 가리는 면적을 제한해 구매 asset의 공간 표현이 실제로 보이게 한다.

### Lighting

- `WorldBootstrap`이 하나의 global lighting과 volume을 소유한다.
- City/Farm demo Scene의 skybox와 post-processing을 각각 사용하지 않는다.
- Farm과 City가 같은 시간대와 그림자 방향을 공유한다.
- Directional Light의 각도·강도·shadow softness와 resolution을 입체 탑다운 Game View에서 조정한다.
- ambient/environment lighting으로 건물의 그늘과 밭 이랑의 깊이가 모두 읽히게 한다.
- Global Volume의 color adjustment, bloom, ambient occlusion과 tone mapping은 과장보다 형태 분리를 목표로 사용한다.
- Farm의 흙·작물과 City의 도로·건물·차량이 같은 날씨와 시간대 안에 있는 하나의 World처럼 보여야 한다.

### Material

- 두 Pack이 공유하는 Synty shader와 palette를 우선 유지한다.
- 상태 색은 원본 material 수정이 아니라 marker, outline과 MaterialPropertyBlock으로 표현한다.
- 선택·오류·Preview 색은 기존 Presentation token에서 결정한다.
- 원본 material을 직접 수정하지 않고 Ssalddel 전용 variant와 MaterialPropertyBlock으로 필요한 변화만 확장한다.
- Farm의 dirt·crop, City의 asphalt·building·vehicle material이 조명 아래에서 충분한 색·명암 차이를 갖는지 실제 Game View로 비교한다.

### Shadow와 Depth

- 건물, 차량, 농부와 작물의 contact shadow가 바닥에서 떠 보이지 않게 한다.
- 밭의 Dirt Row가 멀리서도 반복 패턴과 높이 차이로 읽히게 한다.
- 큰 건물 shadow가 업무 영역 전체를 검게 덮으면 light 방향이나 cutaway 상태를 조정한다.
- 그림자는 장식이 아니라 입체 탑다운에서 높이·거리·공간 경계를 설명하는 정보로 취급한다.

### FX

- Pack에 포함된 dust, rain, sprinkler, pollen과 harvest FX 중 현재 업무 상태에 맞는 것을 적극 활용한다.
- FX는 항상 켜진 장식이 아니라 작업·날씨·차량 이동 같은 Presentation 상태에 반응한다.
- Overview에서는 FX 밀도를 줄이고 Zone/Object Focus에서 필요한 효과를 강화한다.
- FX 완료 event가 Simulation Tick이나 operational Command를 확정하지 않는다.

### Animation

- Synty가 실제 제공한 clip·controller가 확인되면 source와 license를 기록하고 가장 먼저 재사용한다.
- 현재 import에서는 Synty animation clip이 확인되지 않았으므로 Farm·Town·City Humanoid Avatar에 검증된 in-place Idle/Walk clip을 공용 adapter로 리타기팅한다.
- `SyntyProvided`, `Retargeted`, `Procedural`, `Fallback`을 catalog에서 구분하며 리타기팅 결과를 Synty 제공 animation으로 오표기하지 않는다.
- 사람 Journey의 위치는 NavMesh/route follower가 소유하고 root motion은 기본적으로 끈다. 농기계·차량·문·Dock 설비는 절차형 이동·회전부터 구현한다.
- Town의 해소되지 않은 Animator Controller 참조는 validator가 오류로 검출하고 원본 vendor prefab은 직접 수정하지 않는다.
- 공용 Idle/Walk는 도로·Gate와 actor socket이 검증된 직후 구현하고, 밭갈이·파종·수확·하역·진열 보충은 해당 업무 vertical slice에서 한 동작씩 추가한다.
- animation event·도착·전환 완료는 Simulation Tick이나 operational Command를 확정하지 않는다.

세부 inventory, source policy, Region별 intent와 `ANIM0~ANIM6` Gate는 [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다.

### Scale

- 캐릭터를 공통 기준으로 건물·차량·밭의 상대 scale을 맞춘다.
- prefab별 scale·rotation·pivot 보정은 catalog 또는 wrapper에 둔다.
- Data의 tile 좌표와 거리 단위를 asset 크기에 맞춰 변경하지 않는다.

### Detail

- Overview에서는 넓은 농지의 인상을 만드는 반복 작물은 유지하되, 개별 renderer·작은 소품과 interior detail은 batching·LOD·비활성화로 줄인다.
- Zone Focus에서 필요한 object만 활성화하거나 높은 detail을 사용한다.
- 선택하지 않은 Zone의 FX와 Animator는 제한한다.

### 공간 밀도와 생활감

- 핵심 Farm Game View 한 장에는 밭뿐 아니라 Barn·Silo·Farmhouse·농기계·수목·울타리·소품이 함께 보이게 한다.
- 밭·건물·차량·NPC·소품을 균등한 격자로 흩뿌리지 않고, 농로·작업 동선·건물 출입구·출하 흐름을 따라 작은 군집으로 구성한다.
- 반복 작물은 넓은 농지의 규모감을 만들고, 해바라기·울타리·상자 같은 큰 전경 요소는 화면 가장자리에 깊이를 만든다.
- Tractor와 농부 NPC는 장식용 정지 표본이 아니라 현재 작업 또는 이동 상태를 표현하되 Simulation 완료 권위를 갖지 않는다.
- UI와 Card를 숨긴 상태에서도 화면의 중심, 깊이, 이동 방향과 생활감이 유지되어야 한다.

## 12. 기존 WORLD-0~5 기준선과 후속 Region 재구성

WORLD-0~WORLD-5는 현재 구현된 선형 공급망 기준선의 이력이다. 이를 삭제하거나 같은 Scene에서 무리하게 변형하지 않는다. 후속 Composition Track은 기존 Scene·test·Cargo Journey를 보존한 별도 Integration Preview에서 3개 독립 Region Map을 검증한다.

### WORLD-0 — Camera Prototype

- primitive World에서 pitch, FOV, pan, zoom과 90도 회전을 먼저 결정한다.
- World/Zone/Object Focus 전환을 검증한다.
- 지붕 fade와 foreground occlusion의 최소 방식을 정한다.

완료 기준: Farm·물류센터·마트를 각각 확대했을 때 업무 object가 가려지지 않는다.

### WORLD-1 — Macro World Blockout

- 현재 구현된 선형 기준선에서는 Farm Core→Farm Yard→Rural Road→Semi-Urban Transition→Logistics→Market→Residential의 위치와 높이를 배치한다.
- 도로, Zone 입구, 차량 route와 camera focus anchor를 먼저 만든다.
- 건물 내부와 장식물은 아직 최소화한다.

완료 기준: text 없이도 생산에서 소비까지 공간 흐름을 설명할 수 있다.

### WORLD-2 — Synty Visual Quality Prototype

- Farm: 여러 반복 작물밭, 실제 감자 6×6 밭, Barn, Farmhouse, Silo, 풍차, Produce Stand, 농로, 울타리, 수목, 작업 소품, 농부 NPC와 농장 차량
- City: 물류센터, 도로, 마트와 공동주택
- Transition: 농촌 도로 주변의 작은 상점·창고·주택으로 Farm에서 City로 palette와 건축 밀도를 점진적으로 전환
- City/Farm의 실제 shader·material·palette를 유지한 상태에서 공통 lighting·scale을 적용한다.
- Directional Light, shadow, ambient/environment lighting과 Global Volume을 실제 Game View에서 함께 조정한다.
- Farm dirt·crop·building과 City road·building·vehicle이 하나의 시간대와 광원 아래 있는 World처럼 보이게 한다.
- Pack FX가 장면과 업무 상태에 맞으면 제한적으로 활성화해 먼지·관수·수확·차량 이동의 생동감을 만든다.
- Overview, Farm, Logistics와 Market 네 구도를 빠르게 캡처·비교해 화면 완성도가 가장 낮은 Zone을 보정한다.

Farm 대표 Game View의 완료 기준은 다음과 같다.

1. 제공된 POLYGON Farm Showcase와 유사한 공간 밀도와 깊이를 갖는다.
2. 한 화면에 밭만 존재하지 않고 Barn·Silo·Farmhouse·농기계·수목·울타리·소품이 함께 보인다.
3. 전경·핵심 작업 공간·중경·배경이 명확히 존재한다.
4. 반복 작물이 넓은 농지라는 인상을 만든다.
5. 실제 Simulation 대상인 감자 6×6 밭이 주변 환경 안에 자연스럽게 포함된다.
6. Tractor와 농부 NPC가 Scene에 생활감을 만든다.
7. Produce Stand/Farm Yard가 생산과 출하의 시각적 연결점이 된다.
8. 빈 공간을 단순히 남기지 않고 농로·식생·울타리·작업 소품 또는 실제 작업 여유 공간으로 이유를 만든다.
9. UI와 Card를 모두 숨겨도 하나의 완성된 농장 경영 게임 화면처럼 보인다.
10. 화면 캡처만 보아도 `Synty Farm asset을 테스트 중`이 아니라 `Ssalddel의 실제 농장 Zone`으로 인식된다.

전체 WORLD-2 완료 기준은 두 Pack이 서로 다른 게임처럼 보이지 않고, Overview/Farm/Logistics/Market 네 대표 캡처가 `asset을 배치한 테스트 Scene`이 아니라 `입체 탑다운 경영 Simulation의 실제 게임 화면`으로 읽히는 것이다.

### WORLD-3 — 업무 Object와 VisualRoot

- 6×6 Dirt/Dirt Row
- Potato S/M/L와 수확 상자
- FarmWorker Humanoid
- 차량·cargo·Dock·마트 후방·진열대
- 공동주택 대표·관리자와 pickup point

완료 기준: 외형을 primitive fallback으로 되돌려도 같은 stable ID와 View wiring이 유지된다.

### WORLD-4 — Farm→City Handoff

- 감자 상자가 Farm Yard, 차량, 물류센터, 마트로 이어지는 anchor를 배치한다.
- Produce Stand/Farm Yard에서 현장 판매, 출하 대기, 농산물 상태 확인과 도시 공급계약 물량 분리를 서로 다른 Presentation 상태로 표현할 수 있는 공간 anchor를 둔다.
- Rural Road와 Semi-Urban Transition을 통과하며 Farm Pack 중심 경관이 City Pack 중심 경관으로 갑자기 끊기지 않게 한다.
- 같은 cargo lineage의 Zone별 표현을 확인한다.
- 차량 이동과 NPC 도착이 업무 완료를 만들지 않는 경계를 유지한다.

완료 기준: 두 Pack이 배경 테마가 아니라 하나의 공급 흐름으로 연결되어 보인다.

### WORLD-5 — Game View 품질 Gate와 성능 기준

- World Overview, Farm Zone, Logistics Zone, Market Zone의 최종 Game View를 남긴다.
- 각 화면에서 카메라 구도, 조명, 그림자, material 대비, FX 밀도와 UI 가림을 함께 검토한다.
- Console error 0건과 shader 오류 없음은 최소 조건이며, 시각적 완성도 검토를 대신하지 않는다.
- 대표 화면이 Synty prefab overview처럼 보이지 않고 실제 업무 공간과 공급 흐름을 전달하는지 확인한다.
- 대표 renderer, active Animator/FX, camera draw 범위를 기록한다.
- PC/Windows에서 목표 품질 기준을 먼저 고정한다.
- Android는 같은 구성의 실제 profiling 결과를 바탕으로 shadow distance, FX density, material variant와 active Zone 범위를 단계적으로 낮춘다.
- mobile 가능성을 이유로 PC Visual Prototype의 조명·그림자·material 품질을 처음부터 제거하지 않는다.

완료 기준: 저장소의 [커밋별 시각 변경 기록](../Changes/README.md)에 Overview/Farm/Logistics/Market의 게임 화면 수준 대표 PNG, 품질 판단과 성능 측정 기록이 있고 배경 확장을 중단할 수 있다. 대표 PNG는 `docs/assets/changes/`에 둔다.

### CMP-REGION — Farm·Town·City 3개 독립 Region 재구성

- 기존 WORLD-5와 Cargo Journey를 보존한다.
- 별도 Preview에서 Farm·Town·City Region root, Regional Logistics Hub와 passenger·freight Route·Gate를 배치한다.
- Farm→Hub에는 기존 감자 Cargo Journey를, Hub→City에는 명시적 outbound Journey를, Town↔City에는 대표 주민 Journey를 연결한다.
- Farm↔Town에는 상태 없는 ambient 이동과 첫 생활방문 후보를 구분한다.
- 검증 뒤에만 Region별 Scene 분리와 A/B/C 확장을 진행한다.

상세 순서는 [Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md), Map 계약은 [3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md)를 따른다.

## 13. 배경 구성 뒤 코드 복귀 순서

WORLD-5와 FARM-2는 완료됐다. 최신 사용자 요청에 따른 제한된 Composition Track을 다음 순서에 삽입하고 합의된 Gate 뒤 생산 Simulation으로 복귀한다.

1. **FARM-2 밭갈이 폐루프**
   - tile 선택
   - Preview
   - 명시적 Confirm
   - Simulation Tick
   - 새 snapshot
   - Dirt에서 Dirt Row로 표현 갱신

2. **Farm·Town·City 3 Region Composition Track**
   - Region·Gate·Route 계약과 실측
   - 세 Region 최소 A형
   - Farm↔Town·Town↔City 사람 이동망과 Farm/Town→Hub→City 화물망
   - 사람·Cargo Journey와 감자 상품가격 Card
   - 사용처가 검증된 subset만 A/B/C 확장

3. **FARM-3 농부 NPC 작업 표현**
   - task target tile
   - semantic waypoint
   - 이동·정지·회전
   - animation은 Presentation이며 Tick 권위가 아님

4. **FARM-4 파종·생육**
   - SeedLot
   - Seeded→S→M→L
   - deterministic seed와 rule revision

5. **FARM-5 수확·출하**
   - Harvest Preview/Confirm/Tick
   - potato box cargo
   - Farm Yard handoff

6. **FARM-6~SC7 공급망 연결**
   - 농장 출하
   - 운송
   - 물류센터 입고
   - 마트 후방재고
   - 진열 보충
   - 공동주택 주문 결과

## 14. 성능과 저장소 원칙

- 초기 PC Visual Prototype은 Synty shader·material·shadow·FX를 충분히 활용한 목표 화면을 먼저 만든다.
- 성능 최적화는 profiler와 renderer·draw call·메모리 측정 뒤 수행하며, 추측으로 시각 품질을 선제적으로 제거하지 않는다.
- Android에는 별도 quality tier를 두되 같은 PresentationModel과 VisualKey를 유지한다.
- City/Farm demo Scene 전체를 제품 World에 포함하지 않는다.
- 첫 playable allowlist에 든 prefab만 배치한다.
- 반복 작물은 shared material과 가능한 instancing을 사용한다.
- 36 tile은 stable-ID reconcile로 유지하고 매 Tick 전체를 파괴·재생성하지 않는다.
- 멀리 있는 Zone의 interior, FX와 Animator를 비활성화한다.
- vendor 원본과 Ssalddel wrapper·catalog·Scene 변경을 커밋에서 구분한다.
- 구매 asset을 원격 저장소에 넣기 전 repository 공개 범위, license/seat와 Git LFS 정책을 확인한다.
- 시각 변경 커밋에는 최종 Game View PNG와 변경 기록을 같은 맥락으로 포함한다.

## 15. 하지 않을 것

- 수직 90도 카메라로 입체감을 제거
- 자유 회전 카메라로 사용자가 방향을 잃게 함
- NPC 눈높이 3인칭을 기본 조작으로 사용
- 두 Pack의 모든 prefab을 한 Scene에 배치
- 건물 정면을 보여주기 위해 업무 route를 가림
- Farm demo Scene을 제품 World의 시작점으로 사용
- 6×6 Simulation 감자밭만 고립시켜 농장 전체를 실험장처럼 보이게 함
- Farm 옆에 대도시 건물을 바로 붙여 시각적 전환을 끊음
- 환경 object 모두에 stable ID와 업무 상태를 억지로 부여
- 장식 작업을 이유로 FARM-2 코드 복귀를 계속 연기
- 농기계 도착을 밭갈이 완료로 처리
- 식물 크기로 생육 rule을 역산
- 초기 단계에서 계절·날씨·낮밤·대규모 open world까지 확장

## 16. 첫 완료 기준

1. 첫 Farm Game View가 UI 없이도 따뜻하고 풍성한 농장 경영 게임 화면으로 읽힌다.
2. 기본 카메라에서 Farm·Town·City 세 Region과 세 지역 간 Route가 동시에 읽힌다.
3. World/Zone/Object Focus가 일관된 pan·zoom·90도 회전 규칙을 사용한다.
4. Farm·Town·City가 각각 독립 Region으로 구분되고 내부 핵심 공간과 Gate가 읽힌다.
5. 감자 6×6 밭이 더 넓은 환경 농지 안에 포함되며 Untilled/Tilled/S/M/L/Harvested 표현이 기존 상태 계약과 대응한다.
6. 감자 cargo가 Farm→Hub 입고와 Hub→City 출고에서 같은 lineage로 이어지고 사람 Journey가 Farm↔Town 또는 Town↔City에서 보인다.
7. Produce Stand/Farm Yard가 생산·출하·현장 판매의 시각적 연결점으로 읽힌다.
8. 지붕과 전경 object가 선택한 업무 공간을 가리지 않는다.
9. City/Farm asset 이름이 Data·Simulation·server contract에 나타나지 않는다.
10. 원본 prefab을 수정하지 않고 wrapper와 catalog만 사용한다.
11. primitive fallback으로 핵심 test를 계속 실행할 수 있다.
12. Play Mode Console error와 shader 오류가 없다.
13. Overview/Farm/Logistics/Market 대표 화면이 조명·그림자·material·FX와 레이어드 구도를 포함한 게임 화면 수준으로 읽힌다.
14. 대표 Zone별 최종 Game View PNG와 시각 품질 판단이 변경 기록에 포함된다.
15. PC 목표 품질과 Android quality tier 후보가 측정 근거로 분리된다.
16. 제한된 3 Region Composition Track의 합의된 Gate 뒤 실제 우선순위가 FARM-3 생산 표현으로 돌아간다.
17. Synty 제공·리타기팅·절차형·fallback animation이 source별로 구분되고 Farm·Town·City 대표 actor가 공용 locomotion 계약으로 이동한다.

최종 목표는 Farm Scene과 City Scene을 따로 감상하거나 공급망을 도식처럼 공간화하는 것이 아니다. 플레이어가 먼저 실제로 들어가 보고 싶은 농촌과 도시 World를 경험하고, 그 세계 자체를 따라가며 감자가 밭에서 생산되어 출하·운송·입고·진열·주민 수령으로 이어지는 원인과 결과를 이해하고 조작하게 만드는 것이다.

WORLD 기반 위에 아직 문서 상태인 Town·City 주거도로·Farm 농업단지·상품가격 카드와 혼합 경관을 실제 구현하는 순서는 [Unity Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)를 따른다. 사람·차량·설비 동작의 source와 적용 순서는 [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다.
