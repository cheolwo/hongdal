# Unity Farm·Town·City 혼합 Composition 조화 설계

## 1. 목적과 상태

이 문서는 POLYGON Farm·Town·City를 한 World 안에서 조화롭게 연결하는 혼합 Composition Set을 정의한다.

```text
생산 경관을 담당하는 Farm
  + 저밀도 생활권을 담당하는 Town
  + 고밀도 유통·도시 경관을 담당하는 City
  = 생산부터 생활·유통·공동수령까지 이어지는 Ssalddel World
```

이번 범위는 문서 설계뿐이다. 혼합 prefab·catalog·builder·Scene을 생성하지 않고 기존 Farm Composition 24개, WORLD Scene과 vendor asset을 수정하지 않는다.

## 2. Pack별 책임

| Pack | 주된 시각 책임 | 현재 부족한 부분 | 다른 Pack의 보완 |
| --- | --- | --- | --- |
| Farm | 밭·시설하우스·Barn·Silo·농기계·농산물·흙길 | 단독주택·생활상권·도시형 도로 부족 | Town 주택·정원·동네상점·생활차량 |
| Town | 단독주택·driveway·정원·놀이터·동네상점·생활배송 | 대규모 농지·고밀도 skyline·전문 물류 부족 | Farm 생산 경관, City 공동주택·물류·도심마트 |
| City | 공동주택·Office·도심상점·교차로·물류·대중교통 | 단독주택·농촌 생활 완충 부족 | Town 저층 생활권, Farm 생산 경관 |

Town을 Farm·City asset을 섞는 임시 배경으로 쓰지 않는다. Town은 두 Pack 사이의 독립적인 저밀도 생활 문법을 맡는다.

## 3. 확인된 기술적 공통점과 제한

| 항목 | 확인 결과 | 설계 영향 |
| --- | --- | --- |
| Shader | 세 Pack이 `PolygonGeneric/Shaders/Generic_Basic` 공유 | 같은 조명·후처리 아래 통합 가능 |
| Town·City grid | Road·Sidewalk collider가 약 5m cell | 직접 connector adapter 구성 가능 |
| Farm grid | 5m는 후보이나 Dirt Road exact bounds 미측정 | Farm↔Town adapter에서 여백·offset 필요 |
| Material | 각 Pack 고유 palette 유지 | vendor material 수정 대신 배치 비율·조명으로 조화 |
| LOD | Town 702 prefab에 LODGroup 없음 | Town interior·item의 detail tier 필수 |
| 업무 상태 | 모든 vendor prefab에 없음 | Ssalddel wrapper·stable ID·socket 유지 |

## 4. 갱신된 3개 Region World 구조

Farm·Town·City는 하나의 긴 선형 경관 안에서 앞뒤 단계로만 존재하지 않는다. 각각 독립적으로 발전하는 Presentation Region으로 두고 세 개의 지역 간 Route로 연결한다.

```text
 [Farm Region] ── 농산물 집하 ─┐
       │                        ▼
       │ 생활도로      [Regional Logistics Hub] ── 배송 ─→ [City Region]
       │                        ▲                              ▲
 [Town Region] ── 지역 집배송 ─┘──── 사람·통근도로 ────────────┘
```

- Farm Region: 생산·시설하우스·밭·Farm Yard·Produce Stand
- Town Region: 단독주택·생활상권·커뮤니티·지역 배송
- Regional Logistics Hub: 여러 Farm·Town 화물의 입고·검수·보관·분류·출고
- City Region: 도심 분배·도심마트·공동주택·공동수령
- Farm↔Town: 주민·농부·Pickup 중심의 생활 이동
- Town↔City: 주민·대표·Bus 중심의 통근·시장 방문
- Farm/Town→Hub: origin별 집하·집배송 화물차량
- Hub→City: accepted·allocated cargo의 outbound 배송

canonical Zone을 바로 늘리지 않는다. 세 Region은 Map·camera·Composition을 위한 Presentation 경계다.

| Presentation subzone | canonical 의미 |
| --- | --- |
| 농촌주거 접경 | Farm Production/Farm Yard 주변 환경 |
| Town Neighborhood | Residential의 저밀도 표현 또는 Transition |
| Town Main Street | Market·Community로 들어가기 전 생활상권 표현 |
| Regional Road | 기존 Transport route의 경관 표현 |

실제 업무 경계가 필요해질 때만 별도 Zone contract를 검토한다.

세부 footprint·Gate·Route·사람과 차량 Journey 기준은 [Farm·Town·City 3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md), 다중 origin 입출고 기준은 [지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md)를 따른다.

## 5. 혼합 Set 구조

```text
혼합풍경CompositionSet
├─ DominantPackRoot
├─ SupportPackRoot
├─ AccentPackRoot?
├─ SharedRoad·Ground Connectors
├─ Occlusion·Detail Roots
└─ StatefulSockets
```

세 Pack을 같은 비율로 한 세트에 넣지 않는다.

- Dominant: 공간 정체성과 silhouette의 60~80%를 결정한다.
- Support: 다음 공간으로 넘어가는 단서 20~40%를 제공한다.
- Accent: 상자·표지·수목·차량 같은 소수 연결 요소만 사용한다.

이 비율은 asset 개수의 정확한 계산식이 아니라 배치 판단 기준이다.

## 6. Farm↔Town 혼합 Set

| 세트 이름 | Dominant / Support | 주요 구성 | 상태 socket 후보 |
| --- | --- | --- | --- |
| 농장마을 진입부 | Farm / Town | Farm 흙길·울타리·수목 + Town driveway·letterbox·주택 | 차량·주민·방문자 |
| 농촌주택 텃밭 | Town / Farm | Town House·gardenbox + Farm 작물·상자·도구 | 주민·작기·상품카드 |
| 시설하우스 주거접경 | Farm / Town | Greenhouse·농로 + Town 주택·정원·생활차량 | 시설·농부·주민·차량 |
| 농산물 직판 생활상가 | Farm / Town | Produce Stand·농산물 상자 + Town Shop·sidewalk | 상품·가격카드·판매자·고객 |
| 농기계·생활차량 공유마당 | Farm / Town | Tractor·Trailer + Pickup·driveway·shed | 농기계·차량·작업·화물 |
| 농촌 커뮤니티 쉼터 | Town / Farm | Town bench·playground·garden + Farm 수목·fence | 주민·농부·커뮤니티 interaction |

### A/B/C 전이

- A: Farm 80 / Town 20 — 흙길·농업시설 중심, 첫 주택이 보임
- B: Farm 55 / Town 45 — 주택·텃밭·직판이 균형
- C: Farm 25 / Town 75 — asphalt·sidewalk와 Town 주택이 중심, Farm은 상자·수목 accent

같은 세트의 connector와 footprint는 유지한다.

## 7. Town↔City 혼합 Set

| 세트 이름 | Dominant / Support | 주요 구성 | 상태 socket 후보 |
| --- | --- | --- | --- |
| 저층주거 도시전환 | Town / City | Town House·driveway + City Apartment 배경·가로등 | 주민·대표·출입구 |
| 소도시 중심가 도시전환 | Town / City | Town Shop·Main Street + City Shop·corner·traffic | 상점·고객·상품카드 |
| 지역 집배송 거점 | Town / City | Delivery Truck·boxes + City pallet·Van·Station | 차량·화물·handoff·직원 |
| 공동수령 생활거점 | Town / City | Town 주택·주차포켓 + City cargo·공동수령 View | 대표·주민·차량·화물 |
| 대중교통 환승가로 | Town / City | SchoolBus·Bus + City BusStop·Taxi·traffic light | 차량 route·대기 NPC |
| 소도시공원 도시녹지 | Town / City | Town playground·garden + City path·bench·planter | 주민·방문자·커뮤니티 interaction |

### A/B/C 전이

- A: Town 80 / City 20 — 단독주택·낮은 Shop 중심, City 가로등·도로 accent
- B: Town 50 / City 50 — 낮은 상권과 공동주택 배경이 공존
- C: Town 20 / City 80 — City Apartment·Shop 중심, Town garden·주택 한두 동이 기억층으로 남음

## 8. 세 Pack 관통 Set

| 세트 이름 | Farm 역할 | Town 역할 | City 역할 | 연결할 업무 |
| --- | --- | --- | --- | --- |
| 생산·직판·동네상점 흐름 | 농작물·Produce Stand | Shop·counter·고객 공간 | 상품·가격 Card skin | 생산상품 탐색·정보 조회 |
| 농장출하·지역집배송·도심물류 | crate·Tractor/Trailer | Delivery Truck·배송포켓 | pallet·Van·Station/Dock | 같은 cargo lineage handoff |
| 농촌주택·Town주거·공동주택 skyline | Farmhouse·수목 | 단독주택·정원 | Apartment·pickup point | 저밀도→고밀도 Residential |
| 지역정보·커뮤니티 생활거점 | 농업·가격 source anchor | Town Shop·소광장 | PublicData marker·Concept Card | 공개정보·커뮤니티 탐색 |

세 Pack 관통 Set은 하나의 거대한 prefab이나 선형 Map으로 만들지 않는다. 각 독립 Region의 업무 anchor, Region Gate와 지역 간 Route를 조합하고 shared stable ID·Journey가 지나가게 하는 District Recipe다.

## 9. Road·Ground 전환 문법

### 9.1 Farm→Town

```text
Farm Dirt Road
  → Dirt Road End·grass/fence shoulder
  → Town DirtPatch·Driveway
  → Town Road + Sidewalk
```

- Farm road exact bounds가 확정되기 전 adapter wrapper가 offset을 소유한다.
- 갑자기 흙길 전체가 asphalt로 바뀌지 않게 driveway·grass verge를 중간에 둔다.
- Tractor route와 Town vehicle route는 semantic route로 연결하고 mesh 접촉만으로 연결됐다고 보지 않는다.

### 9.2 Town→City

```text
Town Road·Sidewalk 5m grid
  → Town Main Street
  → Town/City 공통 5m connector
  → City Road·Sidewalk·Traffic Light
```

- Town 쪽은 driveway·garden·street sign 비율이 높다.
- City 쪽은 crossing·median·traffic light·parking meter와 높은 facade 비율을 높인다.
- road material 차이는 patch·parking·shadow와 수목 경계로 흡수하고 vendor material을 덮어쓰지 않는다.

## 10. 건물 높이·밀도 전환

```text
Barn·Farmhouse·Greenhouse
  → Town 단층·2층 House
  → Town Shop·Church landmark
  → City 저층 Shop·Old Office
  → City Apartment·Office skyline
```

- 높은 City 건물을 Farm 바로 옆에 두지 않는다.
- Town House의 roof·tree가 중간 높이층을 만든다.
- 전경은 Farm fence·Town garden·City planter처럼 낮은 object로 이어간다.
- camera 기본 방향에서 높이 변화가 계단처럼 읽히되 기계적으로 동일 간격이 되지 않게 한다.

## 11. Palette·조명·소품 조화

- 같은 `Generic_Basic` shader를 유지하고 세 Pack material을 직접 합치거나 recolor하지 않는다.
- 공통 Sun, shadow, ambient, fog와 Global Volume으로 시간대·대기감을 통일한다.
- Farm의 따뜻한 토양·목재, Town의 주택·정원, City의 asphalt·회색 facade가 순서대로 바뀌게 한다.
- pack boundary에서 tree·fence·box·vehicle처럼 의미가 이어지는 object를 두세 종류 반복한다.
- sign·광고는 vendor 의미를 상품·기관의 canonical label로 사용하지 않고 별도 Ssalddel sign layer를 둔다.
- 같은 화면에서 각 Pack 대형 landmark를 하나씩 경쟁시키지 않는다. Zone별 dominant landmark는 하나로 제한한다.

## 12. 상품·Cargo·NPC 경계

### 상품

- Farm produce, Town garden plant·shop item, City shelf는 각각 외형이다.
- `ProductStableId`와 검토된 HS mapping이 있을 때만 상품·가격 카드를 연다.
- 같은 Tomato 외형이 여러 Pack에 있어도 prefab 이름으로 같은 상품이라고 합치지 않는다.

### Cargo

```text
기본 출하: Farm crate → Farm→Hub 화물차량 → Hub 입고·검수·보관·출고 → City 배송차량·Dock
Town 집배송: Town cargo → Town→Hub delivery truck → Hub 분류·outbound → City Van·목적지
예외 직송: 명시적 계약·긴급·대체 route가 있는 Farm→City
```

각 leg는 별도 Route·Journey이며 같은 cargo stable ID·revision과 hub operation lineage가 있을 때만 업무상 이어진다. 여러 origin lot을 자동 병합하지 않고, 외형이 이동했다고 입고·검수·보관·출고·진열을 완료하지 않는다.

### NPC

- Farmer·Town family skin·City business character는 역할 skin이다.
- 역할·권한·가족관계·거주지·계약 관계를 외형에서 추정하지 않는다.
- 개인 주택 내부에는 자동 NPC를 생성하지 않는다.
- NPC 도착·animation은 주문·계약·작업 Command를 실행하지 않는다.

## 13. Performance·Detail Tier

| Focus | Farm | Town | City |
| --- | --- | --- | --- |
| World Overview | 큰 밭 pattern·Barn·Silo | House preset·도로·큰 수목 | Apartment·Office·Station silhouette |
| Zone Focus | 작물열·Greenhouse·차량 | driveway·garden·Shop·놀이터 | Dock·shelf·공동수령·교차로 |
| Object Focus | 실제 재배·상자·도구 | House/Shop interior·생활 item | Market interior·Concept Card·cargo detail |

- Town interior 340개 prop와 72개 item을 Overview에 남기지 않는다.
- transparent Greenhouse, Town interior와 City cutaway를 동시에 활성화하지 않는다.
- 세 Pack 합산 renderer·shadow caster·transparent overdraw를 PC/Android에서 측정한다.
- LOD·instancing·renderer budget은 실제 profiling 뒤 확정한다.

## 14. 구현 순서 후보

### MIX0 — 세 Pack Inventory·Connector 기준

- Town bounds·pivot·5m connector를 검증한다.
- Farm road의 실제 grid와 Town adapter offset을 측정한다.
- shared shader·material·scale·camera 비교표를 만든다.
- Farm·Town·City Region과 Regional Logistics Hub footprint·Gate 방향을 고정한다.

### MIX1 — Farm↔Town 3종

- 농장마을 진입부, 농촌주택 텃밭, 농산물 직판 생활상가 A형을 만든다.
- dirt→driveway→asphalt, Farm/Town scale과 camera depth를 확인한다.

### MIX2 — Town↔City 3종

- 저층주거 도시전환, 지역 집배송 거점, 공동수령 생활거점 A형을 만든다.
- 5m road connector, cargo·NPC socket과 skyline 전환을 확인한다.

### MIX3 — A/B/C와 District Recipe

- Farm↔Town 6종, Town↔City 6종을 A/B/C로 확장한다.
- 혼합 Set을 Region 내부 전체가 아니라 각 Gate와 진입부에 배치한다.
- Farm/Town inbound·Hub Dock·City outbound의 Logistics Hub subset을 추가한다.
- 세 Pack 관통 4개 recipe에서 생산→배송→판매→수령이 읽히는지 확인한다.

### MIX4 — Game View·성능 Gate

- World Overview·세 Region·Hub Focus, Farm↔Town·Town↔City passenger와 Farm/Town→Hub→City freight Corridor Focus를 캡처한다.
- 대표 사람 Journey와 화물차량 Journey Follow를 검증한다.
- PC/Android 성능과 occlusion을 검증한 뒤 필요한 subset만 제품 World에 배치한다.

이번 요청에서는 MIX0~MIX4를 구현하지 않는다.

## 15. 완료 기준 후보

1. Farm·Town·City가 각각 dominant 역할을 유지한다.
2. Farm↔Town·Town↔City 사람 route와 Farm/Town→Hub→City 화물 route의 Gate·connector가 끊기지 않는다.
3. 세 Region이 각각 독립된 내부 구조와 바깥쪽 확장 방향을 가진다.
4. 저층 House에서 Apartment skyline으로 높이·밀도가 점진적으로 변한다.
5. 각 혼합 세트의 A/B/C가 같은 footprint·connector·socket을 유지한다.
6. vendor material·prefab을 수정하지 않는다.
7. 상품·Cargo·NPC의 stable ID와 authority를 asset 이름에서 만들지 않는다.
8. 실제 가족·종교·주소·개별 주문 개인정보를 Town 외형에 연결하지 않는다.
9. World·Region·Corridor·Journey·Object Focus detail tier가 구분된다.
10. 사람·화물차량 이동이 보이되 도착·animation으로 업무 상태를 확정하지 않는다.
11. Preview와 최종 Game View, PC와 Android 성능 증거를 각각 남긴다.

## 16. 관련 문서

- [Unity Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)
- [Unity Farm·Town·City 3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md)
- [Unity Farm·Town·City 지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md)
- [Unity POLYGON Town 반복 배치 Composition Set 조사](UnityPolygonTownCompositionSetResearch.md)
- [Unity Farm 시설하우스·밭·논 단지 Modular Composition 설계](UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md)
- [Unity POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)
- [Unity City 주거단지·십자형 도로 Modular Composition 설계](UnityCityResidentialRoadModularCompositionDesign.md)
- [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
