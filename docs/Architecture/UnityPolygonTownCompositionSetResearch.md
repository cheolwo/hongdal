# Unity POLYGON Town 반복 배치 Composition Set 조사

## 1. 목적과 상태

- 기준일: 2026-08-09
- Unity project: `C:\Users\user\ssalddel`
- 조사 대상: `Assets/Synty/PolygonTown`
- 확인 package: `POLYGON - Town Pack - Art by Synty` 1.9.1
- 이번 결과: 실제 import asset 조사와 문서 설계만 수행하며 prefab·Scene·catalog·builder는 만들지 않는다.

Town Pack의 주된 역할은 Farm과 City 사이를 장식으로 메우는 것이 아니다. Farm Pack에 부족한 단독주택·생활정원·동네상점·통학·생활배송을 제공해 `농촌 마을 → 저밀도 Town → 고밀도 City`가 실제 생활권처럼 이어지게 하는 것이다.

## 2. 실제 Inventory

POLYGON Town에는 prefab 702개와 material 25개가 import되어 있다.

| 분류 | 수 | 주요 구성 |
| --- | ---: | --- |
| 건물·건물 Preset | 143 | House module·preset, Shop, Church, Garden Shed, chimney, skylight |
| 환경 | 97 | road, sidewalk, driveway, path, garden, fence, hedge, tree, 생활 텃밭 작물 |
| 소품 | 340 | 집 안 가구, 주방·욕실·세탁·차고, 정원·수영장·놀이터, 상점·주유·생활 소품 |
| 손에 들거나 진열할 Item | 72 | 음식·음료·공구·생활용품·정원 도구 |
| Generic 배경 | 33 | ground, tree, cloud, mountain, ocean, grass |
| 캐릭터 | 9 | 부모·자녀 외형, 학생 외형, 상점 주인 |
| 차량 | 8 | bus, school bus, pickup, delivery truck, 일반 truck, garbage truck, firetruck, convertible |

주요 family:

| Asset family | 확인 수 | 용도 |
| --- | ---: | --- |
| House module·preset | 131 | 단독주택 외부·내부·차고·deck·roof와 완성 주택 |
| 완성 House preset | 12 | 빠른 대지 조합 기준선 |
| Shop | 5 | 동네상점 외부와 sign |
| Road | 8 | 소도시 도로·주차·횡단·speed bump |
| Sidewalk | 18 | 직선·corner·driveway·crossing |
| Driveway | 8 | 일반·wide와 좌우 path 조합 |
| 생활 텃밭 작물 | 9 | cabbage, carrot, corn, lettuce, onion, pumpkin, tomato, generic vegetable |
| Truck 계열 | 3 | 일반·delivery·garbage truck |

Town material 25개는 현재 Farm·City와 같은 `PolygonGeneric/Shaders/Generic_Basic` shader를 참조한다. palette는 서로 다르지만 Render Pipeline과 shader 계열이 같아 세 Pack을 하나의 조명·후처리 아래 결합하기에 유리하다.

전체 702개 prefab YAML에서 `LODGroup` component는 확인되지 않았다. 대량 배치 전에 wrapper detail tier, renderer 수와 Mobile 성능을 별도로 검증해야 한다.

## 3. 현재 Ssalddel World에서의 역할

Town은 새 canonical 업무 Zone을 만들지 않는다. 우선 다음 Presentation subzone을 제공한다.

```text
Farm Core
  → Farm Yard
  → Rural Residential Edge
  → Town Neighborhood
  → Town Main Street
  → Regional Road
  → Urban Logistics
  → Urban Market
  → City Residential
```

| Town 공간 | 기존 World와의 관계 |
| --- | --- |
| 농촌주거 접경 | Farm Yard·Rural Road와 Town 단독주택의 전환 |
| 소도시 주거지 | City 공동주택 이전의 저밀도 Residential Presentation |
| 소도시 중심가 | Farm Produce Stand와 Urban Market 사이의 생활상권 |
| 생활배송 거점 | Farm cargo와 Urban Logistics 사이의 last-mile·지역배송 표현 |
| 근린 커뮤니티 | 공동주택 대표·지역 주민·상점의 공개 생활 공간 |

운영 주문·계약·배송·공동수령의 권위는 기존 서버 stable ID와 authorized Projection에 남는다. Town GameObject는 그 상태를 표현하는 anchor일 뿐이다.

## 4. Grid와 Module 기준

Town의 일반 Road·Sidewalk·Driveway BoxCollider는 약 `5m × 5m`다. City의 5m road grid와 직접 맞출 수 있다.

```text
T = 5m Town grid unit
도로·인도·진입로 기본 cell = 1T × 1T
주거도로 기본 corridor = 2T
단독주택 대지 후보 = 4T × 6T
주거 block 후보 = 8T × 8T 이상
```

House preset의 정확한 mesh bounds·door 방향은 Editor 측정 전이므로 대지 footprint는 후보값이다. 원본 집을 scale해 대지에 억지로 맞추지 않고 대지 wrapper가 잔디·앞마당·후면 여백을 흡수한다.

## 5. Town Composition 구조

```text
POLYGON Town 원본 prefab
  → 소도시풍경CompositionSet A/B/C
      ├─ EnvironmentRoot
      ├─ HouseExteriorRoot?
      ├─ HouseInteriorRoot?
      ├─ OcclusionRoot?
      ├─ Road·Sidewalk Connectors
      └─ StatefulSockets
  → Town Presentation subzone
```

- House interior는 Object Focus일 때만 활성화하는 후보로 둔다.
- 지붕·전면 벽 cutaway는 Presentation 상태이고 실제 출입·거주 상태가 아니다.
- 환경용 가족·학생 캐릭터는 실제 가족관계·나이·학교·주민 identity를 뜻하지 않는다.
- 실제 NPC는 별도 View와 privacy-safe stable ID를 가진다.

## 6. 소도시 도로 Set

첫 도로 kit는 6종×A/B/C, 총 18개 후보다.

| 세트 이름 | 후보 footprint | connector | 주요 source | 역할 |
| --- | --- | --- | --- | --- |
| 소도시도로 직선 | `4T × 4T` | 북·남 또는 동·서 | Road 01, Sidewalk Straight | 주거 가로 반복 |
| 소도시도로 모서리·끝 | `6T × 6T` | 인접 두 방향 또는 한 방향 | Road Corner End, sidewalk corner/end | block 모서리·막다른길 |
| 소도시도로 T자교차로 | `6T × 6T` | 세 방향 | Road base·crossing overlay·sidewalk corner | 주택가 분기 |
| 소도시도로 십자교차로 | `8T × 8T` | 네 방향 | Road base·crossing·street sign·sidewalk | 생활권 중심 교차로 |
| 주택 진입로 | `2T × 4T` | 도로 1·대지 1 | Driveway·Sidewalk Driveway | 차고·현관 접근 |
| 도로변 주차·배송 포켓 | `4T × 4T` | 도로 측면 1 | Road Parking, pickup·delivery truck socket | 주차·택배·공동수령 대기 |

Town에는 이름상 완성된 T자·십자 도로 prefab이 없으므로 base와 crossing·sidewalk를 조합한다. line·crossing 중첩, z-fighting과 차선 방향은 구현 Gate에서 확인한다.

## 7. Town 생활권 Set

첫 생활권 kit는 12종×A/B/C, 총 36개 후보다.

| 세트 이름 | 주된 구성 | 주요 source | 상태 socket 후보 |
| --- | --- | --- | --- |
| 단독주택 기본대지 | 완성 House+앞마당+현관 path | House Preset, grass, path, letterbox | 주민·방문자·출입구 |
| 차고형 단독주택 | House Garage+driveway+차량 여백 | Garage Preset·garage wall·wide driveway | 주민·차량·차고·배송 |
| 정원형 단독주택 | House+garden·hedge·flower·tree | House Preset, Garden, hedge, pot plant | 주민·정원작업·방문자 |
| 텃밭형 단독주택 | House+작은 생활 텃밭 | gardenbox, cabbage·carrot·lettuce·tomato 등 | 주민·텃밭·상품카드 후보 |
| 뒷마당 생활주택 | deck·washing line·barbeque·shed | House deck, Garden Shed, outdoor props | 주민·생활 interaction |
| 동네상점 전면 | Shop+sidewalk+sign+주차 | Shop 01~03·Concrete·Sign | 상점·상점주인·고객·배송 |
| 동네상점 내부 | counter·shelf·fridge·checkout | ShopCounter·Shelf·Fridge·Checkout | 상품·가격카드·상점주인·고객 |
| 생활택배 거점 | delivery truck+boxes+현관·상점 | Delivery Truck, cardboard box, driveway | 차량·화물·배송 task·수령자 |
| 통학버스 정류장 | school bus+sidewalk+street sign | SchoolBus, sidewalk, streetlamp | 차량·대기 NPC·route |
| 근린 놀이터 | playground+grass+path+bench | Playground 5종, sandpit, hoop, tree | 주민·방문자·커뮤니티 interaction |
| 교회·소광장 | Church+path+fountain·bench | Church, Church props, fountain, garden | 방문자·공개모임 interaction |
| 주거 서비스 골목 | rubbish·laundry·airvent·utility | garbage truck, bin, trash bag, airvent | 관리작업·수거차량·서비스출입 |

## 8. A/B/C 변형

| 변형 | 성격 | 원칙 |
| --- | --- | --- |
| A — 소형 기본형 | 좁은 대지와 낮은 소품 밀도 | House/기능 1개, connector 최소 |
| B — 생활 활동형 | 정원·차량·배송·NPC socket 증가 | 앞·뒤마당과 생활 동선 분리 |
| C — 모서리·깊이형 | corner road와 배경 수목·높이 변화 | 시야 깊이와 인접 block 연결 강화 |

같은 set의 A/B/C는 footprint, road·sidewalk connector와 House entrance 방향을 유지한다. House preset·정원·소품만 달라야 한다.

## 9. 단독주택 단지 Recipe

### 9.1 십자형 소도시 주거지

```text
┌──────────────┬──────────┬──────────────┐
│ 정원주택 A   │ 북쪽도로 │ 차고주택 B   │
│ 텃밭·우편함  │          │ 배송포켓     │
├──────────────┼──────────┼──────────────┤
│ 서쪽도로     │ 십자교차 │ 동쪽도로     │
├──────────────┼──────────┼──────────────┤
│ 생활주택 C   │ 남쪽도로 │ 놀이터 D     │
│ 뒷마당       │          │ 수목완충     │
└──────────────┴──────────┴──────────────┘
```

- House entrance·letterbox·driveway는 sidewalk를 향한다.
- delivery pocket은 교차로 중심이 아니라 block 측면에 둔다.
- 놀이터와 차량 서비스 골목을 같은 corner에 두지 않는다.
- 실제 가구 수·세대 수·주민 수를 visible house 수로 계산하지 않는다.

### 9.2 소도시 중심가

```text
[동네상점 A] [소광장] [동네상점 B]
═════════ Town Main Street ═════════
[주택 C]   [배송포켓] [근린놀이터]
```

- Shop interior는 Object Focus에서만 표시한다.
- 배송차량이 고객 출입구와 보행로를 막지 않게 service socket을 분리한다.
- Town Shop의 음식·음료·진열 item은 실제 판매상품이나 가격을 뜻하지 않는다.

### 9.3 농촌주거 접경

```text
Farm fence·수목·작은 밭
  → 텃밭형 Town 주택
  → 정원형 Town 주택
  → Town sidewalk·직선도로
  → Town 중심가
```

이 recipe가 기존 Semi-Urban Transition의 저밀도 구간을 맡는다.

## 10. House Interior 후보

Town Pack에는 상세 실내 asset이 많지만 처음부터 모든 집 내부를 채우지 않는다. Object Focus용 후속 6종을 별도 catalog로 둔다.

| 실내 세트 | 주요 source | 경계 |
| --- | --- | --- |
| 현관·거실 | couch, TV, cabinet, rug, lamp | 주민 개인정보 없이 환경 표현 |
| 주방·식당 | kitchen, fridge, table, chair, cutlery | 실제 식재료·재고와 분리 |
| 침실 | bed, wardrobe, dresser, bedside table | 개인 room identity 생성 금지 |
| 욕실 | bath, shower, toilet, sink, towel | Object Focus 외 비활성 |
| 세탁실 | laundry, washing machine, bin, washing line | 관리작업 상태와 분리 |
| 차고·작업실 | workbench, toolboard, shelf, tool item | 실제 차량정비·작업 Command와 분리 |

실제 사용자 이름, 가족관계, 연락처, 상세주소, 구매내역을 House interior에 투영하지 않는다.

## 11. 상품·역할·민감정보 경계

- `Mother`, `Father`, `Son`, `Daughter`, `SchoolBoy`, `SchoolGirl`은 vendor 외형 이름일 뿐 실제 가족관계·성별 역할·연령·학교를 정하지 않는다.
- 자동으로 household를 만들거나 주택에 사람을 배정하지 않는다.
- Church asset은 환경 landmark 후보다. 주민의 종교·국적·언어를 추론하거나 가입·노출·신뢰·역할 자격에 사용하지 않는다.
- SchoolBus·Firetruck·GarbageTruck은 vehicle skin이며 통학·긴급·수거 업무 완료를 발생시키지 않는다.
- garden plant와 food item은 외형이다. `ProductStableId`가 없으면 HS·가격·재고 카드를 열지 않는다.
- Gas pump·propane은 운영 주유·위험물 시설이 아니다.

## 12. 성능과 Presentation

- 702개 prefab에 LODGroup이 없으므로 Overview에 House interior·작은 item을 남기지 않는다.
- Overview: House preset, road, large tree와 Shop silhouette 중심.
- Zone Focus: driveway, garden, playground, delivery vehicle와 주요 NPC socket.
- Object Focus: interior·작은 생활소품·상품카드 anchor.
- 같은 House preset을 이웃하게 반복하지 않고 preset·roof·driveway·garden A/B/C를 교차한다.
- shared shader를 유지하고 vendor material을 직접 수정하지 않는다.
- Town의 생활소품 밀도가 Farm·City보다 과도하게 높아지지 않도록 detail budget을 surface별로 둔다.

## 13. 구현 전 Gate

### TOWN0 — Inventory·bounds

- House preset 12개, Road·Sidewalk·Driveway의 bounds·pivot·door 방향을 Editor에서 측정한다.
- base·overlay·complete preset·modular part를 source catalog에서 구분한다.
- 5m grid와 City road connector 호환을 확인한다.

### TOWN1 — 도로·주택 최소 kit

- 도로 직선·모서리·T자·십자, 기본주택·차고주택·정원주택 A형을 만든다.
- road graph, driveway, entrance와 camera occlusion을 검증한다.

### TOWN2 — 생활상권·배송

- 동네상점 전면·내부, 생활택배 거점과 놀이터 A형을 만든다.
- 상품·화물·NPC socket과 environment authority 부재를 검증한다.

### TOWN3 — A/B/C와 Preview

- 도로 6종·생활권 12종×A/B/C, 총 54개 후보를 검토한다.
- 실제 World에 필요한 subset만 구현하고 library preview와 최종 Game View를 구분한다.

### TOWN4 — House Interior

- 선택된 한두 House만 Object Focus interior를 검증한다.
- privacy·occlusion·renderer budget을 통과한 뒤 확장한다.

이번 요청에서는 TOWN0~TOWN4를 구현하지 않는다.

## 14. 완료 기준 후보

1. Town 702 prefab의 source category와 사용 금지 대체가 기록된다.
2. Road·Sidewalk·Driveway 5m connector가 검증된다.
3. 도로 6종과 생활권 12종의 A/B/C footprint·connector가 일치한다.
4. House entrance, driveway와 sidewalk가 연결된다.
5. visible House·NPC 수를 실제 세대·주민 수로 해석하지 않는다.
6. Shop item·garden crop에 임의 상품·가격을 연결하지 않는다.
7. 차량·NPC·animation이 업무 Command를 실행하지 않는다.
8. 개인 House interior에 실제 개인정보를 넣지 않는다.
9. vendor prefab·material을 수정하지 않는다.
10. Overview·Zone·Object Focus detail tier와 PC/Android 성능을 검증한다.

## 15. 관련 문서

- [Unity POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)
- [Unity City 주거단지·십자형 도로 Modular Composition 설계](UnityCityResidentialRoadModularCompositionDesign.md)
- [Unity Farm 시설하우스·밭·논 단지 Modular Composition 설계](UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md)
- [Unity Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)
- [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
