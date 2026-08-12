# Unity POLYGON City 반복 배치 Composition Set 조사

## 1. 목적과 범위

- 기준일: 2026-08-09
- 조사 대상: `C:\Users\user\ssalddel\Assets\Synty\PolygonCity\Prefabs`
- 목적: POLYGON City의 단일 prefab을 반복 가능한 작은 도시 풍경 세트로 묶기 위한 후보 catalog를 정의한다.
- 이번 결과: 문서화만 수행하며 Unity prefab·Scene·catalog·builder와 vendor asset은 변경하지 않는다.

Farm Pack의 `농장풍경CompositionSet`과 같은 Presentation 계층을 City Pack에도 적용할 수 있다. 다만 이 문서의 세트명, 조합과 socket은 구현 후보이며 실제 prefab, asset catalog와 preview Scene이 만들어졌다는 뜻이 아니다.

## 2. 실제 Inventory 결론

실제 Unity project에서 확인한 POLYGON City prefab은 총 335개다.

| 분류 | 수 | 주요 구성 |
| --- | ---: | --- |
| 건물 | 76 | 공동주택, 상점, 구형·원형·사각·팔각 사무실, 역·시설, 시청, 지붕·계단·비상계단 module |
| 환경 | 65 | 도로, 인도, 길·잔디길, 교량, 수변, 지하철 입구, 나무·꽃·울타리 |
| 소품 | 174 | 매장 집기, 상자·팔레트, 간판, 신호등·가로등, 벤치, 화분, 설비, 쓰레기·공사 소품 |
| 차량 | 9 | 승용차 7종, Van 1종, steering wheel 소품 1종 |
| 캐릭터 | 9 | 사무직·일반 시민·경찰 외형 |
| FX | 2 | 비, 증기 |

세트 구성에 특히 유용한 asset family는 다음과 같다.

| Asset family | 수 | 세트 활용 |
| --- | ---: | --- |
| Apartment | 25 | 공동주택 block, 생활마당, 공동수령 장소 |
| Shop | 13 | 전환 상가, 도심마트, 먹거리 골목 |
| Office | 26 | 관리 사무실, 공공정보관, 도시 배경 block |
| Station | 3 | 물류센터 또는 공공시설의 교체 가능한 외형 |
| Road | 15 | 진입로, 교차로, 주차·하역 lane |
| Sidewalk | 환경 14 + 소품 5 | 상점 전면, 보행 동선, 정류장 |
| Path·GrassPath | 8 | 공원, 수변, 완충 공간 |
| ShopInterior | 11 | 진열대, display, desk, table, cafe 구성 |
| CardboardBox·Pallet | 5 | 입고·적치·공동수령 화물 외형 |
| 실제 차량 외형 | 8 | Van, 택시, 일반 차량, 경찰차, 구급차 |
| Character | 9 | 관리자·방문자·주민·경찰의 교체 가능한 외형 |

이 정도 inventory면 상가·물류·마트·주거·교통·공공 공간은 조합할 수 있다. 그러나 다음 한계가 있다.

- 물류 전용 대형 truck, trailer, forklift와 warehouse rack은 없다.
- `Station`은 물류센터 업무 의미가 아니라 교체 가능한 건물 외형이다.
- Shop prefab과 음식 모양 대형 간판은 실제 판매 상품·재고·가격을 뜻하지 않는다.
- Shop interior 소품은 있지만 완성된 cutaway 매장 prefab은 별도로 조립해야 한다.
- 캐릭터에는 resident·manager·worker 같은 canonical 역할이 내장돼 있지 않다.
- City Pack에 실제 AnimationClip·AnimatorController가 없다는 기존 검증 결과를 유지한다.
- 한국어 상호·도로 표지와 Ssalddel 고유 branding은 별도 Presentation layer가 필요하다.

## 3. City Composition Set 구조

```text
POLYGON City 원본 prefab
  → 도시풍경CompositionSet A/B/C
      ├─ EnvironmentRoot
      ├─ OcclusionRoot?
      ├─ InteriorRoot?
      ├─ RoadConnectorSockets
      └─ StatefulSockets
  → City Zone 배치
  → 상태가 필요한 socket에만 Ssalddel View 연결
```

### EnvironmentRoot

건물, 도로, 인도, 가로등, 화분, 벤치와 작은 생활 소품처럼 경관을 만드는 object를 둔다. 환경 object는 stable ID, 권한, 재고, 가격, 차량 상태와 업무 완료 여부를 소유하지 않는다.

### OcclusionRoot와 InteriorRoot

- camera를 가리는 건물 전면·지붕은 명시적 occlusion 대상으로 분리한다.
- 매장 내부 세트는 Object Focus일 때만 `InteriorRoot`를 활성화할 수 있게 한다.
- cutaway는 View의 시각 상태이며 건물 폐쇄·영업·입고 상태를 뜻하지 않는다.

### RoadConnectorSockets

`도로진입`, `도로진출`, `인도연결`, `보행진입`, `서비스진입`처럼 세트끼리 접속할 위치와 방향만 제공한다. 차선 방향, 화살표, 정지선, 횡단보도와 신호등은 회전 후에도 서로 맞아야 한다.

### StatefulSockets

실제 업무 상태를 표현할 대상만 별도 socket으로 둔다.

- 관리자·직원·주민·대표·방문자 NPC
- 차량과 운송 task
- 화물·pallet·입고·공동수령 object
- 진열대·상품·가격 카드 anchor
- 출입구·Dock·interaction anchor
- 공공정보 marker·커뮤니티 board

socket은 위치 anchor이며 상태나 stable ID 자체가 아니다. 실제 View가 연결될 때만 canonical 또는 Simulation stable ID를 가진다.

## 4. 첫 City Library 후보

첫 library는 12종을 A/B/C 세 변형으로 준비하는 구성이 적절하다. 구현한다면 총 36개 후보 prefab이지만 이번 조사에서는 생성하지 않는다.

| 세트 이름 | 공간 역할 | 주요 City asset family | 상태 연결 socket 후보 |
| --- | --- | --- | --- |
| 농촌도시 전환 상가 | Rural Road에서 도시 상업가로로 밀도를 높이는 전환 | 작은 Shop, OfficeOld Small, sidewalk, grass path, tree, planter, 일반 간판 | 상점·방문자·차량·상호작용 |
| 도시 진입 교차로 | Transition에서 Urban Zone으로 들어오는 방향성과 안전시설 | road, crossing, median, arrow, traffic light, light pole, 도로 표지 | 차량 route·보행 route |
| 물류센터 입고장 | 차량 접근→Dock→입고의 핵심 작업 공간 | Station, Van, pallet, cardboard box, barrier, cone, parking line, security camera | 차량·화물·Dock·직원·상호작용 |
| 물류센터 적치·출하장 | 검수 뒤 임시 적치와 다음 운송 인계 | Station, cover, pallet, cardboard box, Van, light pole, service road | 화물·차량·직원·출하 task |
| 도심마트 앞마당 | 도로에서 매장 입구·입고·고객 동선으로 연결 | Shop, ShopCover, sidewalk, planter, sign, parking line, box | 관리자·고객·화물·매장 입구 |
| 도심마트 매장 내부 | 진열·가격 확인·관리 업무를 읽는 Object Focus 공간 | shelf, display, desk, table, chair, cafe, cardboard box | 진열대·상품·가격 카드·관리자·고객·후방재고 |
| 먹거리 상점 골목 | 작은 음식점·직판 상점이 모인 보행 상권 | Shop, cafe·pizza·noodle sign, hotdog stand, umbrella, table, soda, planter | 상점·판매자·방문자·상품 카드 |
| 공동주택 생활마당 | 공동주택의 일상성과 대표·주민 동선을 표현 | Apartment, stairs, planter, washing line, bench, mailbox, tree | 주민·대표·방문자·출입구 |
| 공동수령 장소 | 차량 하차→임시 적치→주민 인계 공간 | Apartment, cover, sidewalk dip, Van, pallet, boxes, bench, light | 차량·화물·공동수령·대표·주민·interaction |
| 사무·공공정보관 앞 | 관리자 업무와 공개정보 진입점을 표현 | CityHall 또는 Office, entrance sign, ATM, poster, security camera, business character | 관리자·방문자·공공정보 marker·interaction |
| 대중교통 정류장 | 보행과 차량 route가 만나는 도시 생활 node | bus stop, taxi, sidewalk, road, Bustop sign, light pole, bench, parking meter | 승객·차량·정류장·보행 route |
| 도시 공원 쉼터 | 건물 밀도 사이의 수목·휴식·커뮤니티 완충 공간 | grass path, path, tree, flower, bench, picnic table, umbrella, trash can | 주민·방문자·커뮤니티 interaction |

## 5. A/B/C 변형 원칙

모든 세트의 A/B/C를 단순 색상 차이로 만들지 않는다.

| 변형 | 공간 성격 | 구성 원칙 |
| --- | --- | --- |
| A — 기본형 | 좁은 footprint와 명확한 기능 | 건물 또는 핵심 시설 1개, route 1개, 큰 소품 최소화 |
| B — 작업형 | 사람·차량·화물이 머무는 중간 밀도 | 상태 socket을 늘리고 작업 소품·보행 공간을 함께 둠 |
| C — 모서리·배경형 | corner, 깊이와 skyline 연결 | corner 건물·sidewalk, 높은 건물·간판 또는 수목을 후방에 배치 |

예시:

| 세트 | A | B | C |
| --- | --- | --- | --- |
| 물류센터 입고장 | Van 1대 접근과 Dock 1개 | pallet·상자 적치와 작업자 공간 | corner service road와 배경 facility |
| 도심마트 앞마당 | 작은 Shop 전면 | 입고 상자와 고객 동선이 공존 | corner Shop과 측면 서비스 진입 |
| 공동주택 생활마당 | 작은 현관·우편함 | 벤치·화분·빨랫줄의 생활 밀도 | corner apartment와 배경 수목 |
| 도시 진입 교차로 | 직선 진입 | T자 또는 횡단보도 | 교차·median과 corner building 연결 |

공동주택 단지와 직선·모서리·T자·십자형 도로를 5m grid, connector graph와 block recipe까지 구체화한 후속 기준은 [Unity City 주거단지·십자형 도로 Modular Composition 설계](UnityCityResidentialRoadModularCompositionDesign.md)를 따른다.

## 6. 후속 Library 후보

다음 6종은 첫 36개가 실제 World에서 읽힌 뒤 확장하는 편이 좋다. 구현한다면 각각 A/B/C, 총 18개 후보가 된다.

| 세트 이름 | 목적 | 주요 asset |
| --- | --- | --- |
| 주거 뒷골목 | 공동주택의 서비스 면과 생활감 | fire escape, washing line, trash, skip, pipe, aircon, fence |
| 옥상 설비 군집 | 높은 건물의 배경 silhouette와 설비 밀도 | roof access, aircon, vent, pipe, skylight, satellite dish, billboard |
| 커뮤니티 광장 | 게시·대화·모임의 공개 공간 | CityHall/Office 전면, bench, poster, newspaper, planter, tree |
| 수변 산책로 | 도시 외곽의 완충과 배경 깊이 | water edge, ocean tile, path, tree, bench, deckchair |
| 교량 진입부 | 물류·도시 route의 큰 landmark | bridge edge·wall·support·pillar·underside, road, divider, light |
| 비상 대응 거점 | 실제 authorized 공공 역할이 있을 때만 쓰는 대응 공간 | 경찰 캐릭터·경찰차·구급차, hydrant, barrier, police/fire/hospital sign |

`비상 대응 거점`은 경찰차나 표지가 있다는 이유로 운영 기관이나 긴급상황을 만들어서는 안 된다. 실제 역할·event가 없으면 background set으로도 사용하지 않는 것이 안전하다.

## 7. 세트별 상세 구성 후보

### 7.1 농촌도시 전환 상가

주요 후보:

- `SM_Bld_Shop_01`, `SM_Bld_Shop_02`
- `SM_Bld_OfficeOld_Small_01`, `SM_Bld_OfficeOld_Small_02`
- `SM_Env_Sidewalk_Straight_01`, `SM_Env_GrassPath_Straight_01`
- `SM_Env_Tree_01`~`03`, `SM_Prop_Planter_01`~`02`
- 작은 Cafe·Entrance·Parking sign

Farm Yard 바로 옆에 고층 Office를 배치하지 않는다. 낮은 Shop·구형 소형 Office·수목을 먼저 섞고 도시 방향으로 sidewalk와 가로등 비율을 높인다.

### 7.2 물류센터 입고·출하

주요 후보:

- `SM_Bld_Station_01`~`03`
- `SM_Veh_Car_Van_01`
- `SM_Prop_Pallet_01`
- `SM_Prop_CardboardBox_01`~`04`
- `SM_Prop_Barrier_01`, `SM_Prop_Cone_01`~`02`
- `SM_Env_Road_ParkingLines_01`, light pole와 security camera

City Pack에는 대형 화물차·지게차·rack이 없으므로 세트가 대규모 물류단지를 과장하지 않게 한다. 현재는 소형 도심 물류센터, cross-dock 또는 last-mile 인계 공간의 시각 규모가 적합하다.

### 7.3 도심마트 외부·내부

외부 후보:

- `SM_Bld_Shop_03`~`06`, corner와 cover variant
- sidewalk, parking line, planter, sign, cardboard box

내부 후보:

- `SM_Prop_ShopInterior_Shelf_01`~`04`
- `SM_Prop_ShopInterior_Display_01`~`02`
- `SM_Prop_ShopInterior_Desk_01`~`02`
- `SM_Prop_ShopInterior_Table_01`, `Chair_01`, `Cafe_01`

매장 외부와 내부를 하나의 항상 활성 prefab으로 합치지 않는다. Overview에서는 외부를 유지하고 Object Focus에서만 필요한 interior detail을 표시한다. 진열대와 상품 상자는 실제 `ProductStableId`, 재고·가격 카드 anchor가 연결될 때만 업무 object가 된다.

### 7.4 공동주택 생활·공동수령

주요 후보:

- Apartment complete·corner·door·stairs·planter variant
- washing line, mailbox, bench, planter, pot plant, tree
- 공동수령용 Van, pallet, cardboard box, sidewalk dip와 cover

공동수령 세트의 상자와 Van은 기본 환경으로 수량·도착을 표현하지 않는다. 실제 `ResidentialPickup`, unloading task와 cargo View가 socket에 연결됐을 때만 상태를 표시한다.

### 7.5 도로·정류장·공원

도로는 `Road`, `Sidewalk`, `TrafficLight`, `LightPole`, `Sign` family를 함께 사용한다. 정류장은 `BusStop`, Taxi, bench와 sidewalk를 묶는다. 공원은 grass path·tree·flower·bench·picnic table을 사용하되 도시 전역에 같은 A형을 연속 배치하지 않는다.

## 8. 반복 배치 규칙

### 8.1 연결과 방향

- 도로·인도 connector는 공통 grid와 snap 단위를 사용한다.
- corner 건물은 실제 corner에만 두고 출입구가 인도를 향하게 한다.
- road arrow, stop·give-way sign과 traffic light는 90도 회전 뒤 차선과 일치하는지 검증한다.
- text·상호·그림이 있는 sign은 무조건 좌우 반전하지 않는다.
- service entrance와 고객·보행 진입을 같은 socket으로 합치지 않는다.

### 8.2 반복 감춤

- 같은 완성 건물을 바로 이웃하게 두지 않고 A/B/C와 다른 building family를 교차한다.
- 동일 가로등·화분·상자·쓰레기 소품은 위치·회전·밀도를 제한된 seed로 바꾼다.
- 큰 음식 간판과 billboard는 시선 landmark로만 제한하고 모든 상점에 반복하지 않는다.
- 높은 Office·Apartment는 배경층, Shop·정류장은 중경, 벤치·화분·상자는 전경층에 둔다.

### 8.3 업무 가독성

- 차량 route, NPC route, Dock와 매장 입구를 환경 소품으로 막지 않는다.
- 물류 pallet·상자와 마트 상품 상자는 색만으로 구분하지 않고 stable ID View와 focus 상태를 사용한다.
- 환경 NPC와 업무 NPC를 같은 위치에 겹치지 않는다.
- 카드·interaction anchor 앞에는 최소 선택 여백을 남긴다.
- camera 기본 방향과 90도 회전 네 방향에서 occlusion을 확인한다.

## 9. Farm→City 연결

```text
농장 작업마당
  → 농산물 직판장
  → 농촌 도로
  → 농촌도시 전환 상가 A/B/C
  → 도시 진입 교차로
  → 물류센터 입고장
  → 도심마트
  → 공동주택 생활마당·공동수령 장소
```

- Transition에는 Farm의 수목·상자·낮은 건물과 City의 sidewalk·Shop·가로등을 함께 사용한다.
- City로 갈수록 직선 road, sidewalk, corner building과 signage 비율을 높인다.
- Farm palette에서 City palette로 바뀌더라도 cargo stable ID와 route lineage는 끊지 않는다.
- Transition 세트는 별도 업무 Zone이 아니라 Farm과 Urban 사이의 Presentation subzone으로 유지한다.

## 10. 상품·간판·가격 카드 경계

City Pack의 Burger·Hotdog Stand·Soda·Cafe·Pizza·Noodles 및 음식 모양 대형 간판은 상권 분위기를 만드는 외형이다. 이것만으로 판매 상품, HS 코드, 원산지, 재고 또는 가격을 만들지 않는다.

- 실제 상품 카드는 `ProductStableId`가 있는 진열대·상자·interaction anchor에서 연다.
- 간판 클릭은 기본적으로 상점 focus 또는 환경 설명만 제공한다.
- Farm 상품이 마트에 도착했을 때는 Farm asset 이름이 아니라 cargo→inventory→shelf lineage로 연결한다.
- 상품·가격 카드 흐름은 [Unity Farm 상품·가격 카드 상호작용 흐름](UnityFarmProductPriceCardInteractionFlow.md)의 Data·Interpretation·Presentation 경계를 재사용한다.

## 11. 구현 전 Gate

문서 뒤 바로 54개 prefab을 만드는 것이 아니라 다음을 순서대로 확인한다.

1. 첫 12종 중 실제 WORLD-5에 필요한 세트와 footprint 범위를 선택한다.
2. complete building과 modular part의 pivot·bounds·door 방향을 측정한다.
3. road·sidewalk connector grid와 lane 방향을 확정한다.
4. 각 세트의 EnvironmentRoot, OcclusionRoot, InteriorRoot와 socket schema를 고정한다.
5. A형 두세 종으로 camera·NavMesh·선택·draw call을 먼저 검증한다.
6. 검증된 규칙으로 B/C를 생성하고 library preview에서 중복·누락을 확인한다.
7. 최종 City Zone에 필요한 세트만 배치하고 Game View와 PC/Android 성능을 검증한다.

이번 조사에서는 pivot·mesh bounds·collider·NavMesh·LOD·draw call을 측정하지 않았다. 따라서 footprint 수치, 세트당 renderer budget과 Mobile detail tier는 아직 확정하지 않는다.

## 12. 완료 기준 후보

City Composition Library를 실제 구현할 때는 다음을 만족해야 한다.

1. 세트 이름은 한국어 경관·업무 의미로 탐색할 수 있다.
2. 첫 12종에 A/B/C 변형이 빠짐없이 존재하고 key가 중복되지 않는다.
3. 모든 source는 원본 City prefab을 nested reference로 사용한다.
4. 원본 prefab과 material을 수정하지 않는다.
5. 환경 object와 상태 socket이 분리된다.
6. 세트 prefab·catalog에 업무 상태, stable ID, 권한, 수량과 가격을 저장하지 않는다.
7. road·sidewalk connector와 출입구 방향이 검증된다.
8. 물류센터·마트·공동수령 route가 소품이나 건물에 막히지 않는다.
9. Overview에서는 interior·소형 소품을 줄이고 Object Focus에서 필요한 detail만 표시한다.
10. Farm→Transition→Logistics→Market→Residential이 서로 다른 demo Scene의 연결처럼 보이지 않는다.
11. Preview Scene은 library 검사 증거이고 최종 Game View 증거와 구분한다.
12. PC와 Android에서 draw call·shadow·memory를 측정한 뒤 detail tier를 확정한다.

## 13. 관련 문서

- [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
- [City·Farm World P0 기준선과 Asset Inventory](UnityCityFarmWorldP0Inventory.md)
- [Synty POLYGON City Pack 도입 검토표](UnitySyntyCityPackAdoptionChecklist.md)
- [Unity POLYGON Town 반복 배치 Composition Set 조사](UnityPolygonTownCompositionSetResearch.md)
- [Unity Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)
- [Unity 서버 상태와 3D World Projection 설계](UnityServerStateToWorldProjectionDesign.md)
- [Unity Farm 상품·가격 카드 상호작용 흐름](UnityFarmProductPriceCardInteractionFlow.md)
