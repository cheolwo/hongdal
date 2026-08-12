# Unity City 주거단지·십자형 도로 Modular Composition 설계

## 1. 목적과 상태

이 문서는 POLYGON City로 다음 공간을 반복 생성하기 위한 구체적인 module 문법을 정의한다.

- 실제 도시처럼 이어지는 직선·모서리·T자·십자형 도로
- 도로 주변의 공동주택 block과 생활마당
- 공동수령·주차·보행·서비스 공간
- City Pack만으로 가능한 저층 주거 가로
- 여러 module을 조합한 하나의 주거 생활권

기준일은 2026-08-09이다. 이번 범위는 문서화뿐이며 Unity prefab·Scene·catalog·builder·NavMesh와 vendor asset은 변경하지 않는다.

이 문서에서 제안하는 grid, footprint와 connector는 실제 prefab 조사를 바탕으로 한 첫 설계값이다. 구현 전에 Unity Editor에서 mesh bounds, pivot, 출입구 방향과 camera scale을 다시 검증한다.

## 2. 실제 Asset에서 확인한 기준

### 2.1 도로와 인도

- `SM_Env_Road_*` 15개
- `SM_Env_Sidewalk_*` 14개
- 일반 Road·Sidewalk prefab의 BoxCollider는 대부분 `5m × 5m` 평면이다.
- `SM_Env_Road_ParkingLines_01`과 `SM_Env_Sidewalk_Merger_01`은 약 `10m × 5m`다.
- Road Patch와 일부 Path는 MeshCollider라 같은 방식의 직사각 bounds로 확정하지 않았다.
- `SM_Env_Road_Crossing_01`의 `Crossing`은 보행 횡단 표현 후보이지 십자 교차로 완성 prefab으로 간주하지 않는다.

따라서 첫 도로 문법은 `5m`를 한 grid unit으로 삼는 것이 자연스럽다.

```text
U = 5m
1U × 1U = 기본 Road·Sidewalk cell
2U × 1U = ParkingLines·SidewalkMerger 계열
```

### 2.2 공동주택

공동주택 관련 prefab은 25개다.

- 기본 외형: `Apartment_01`~`03`
- 모서리: `Apartment_Corner_01`~`03`
- 출입구: `Apartment_Door_01`~`02`, Door Corner
- 지붕: Roof·Roof Corner
- 적층: `Apartment_Stack_01`~`03`
- 계단·화분: Stairs·Stairs Planter·Stairs Corner

해석 가능한 collision bounds 기준으로 기본 apartment module의 평면은 대체로 약 `5m × 5m`, 한 층 module 높이는 약 `3m`, Stack 높이는 약 `9m`다. 즉 도로와 건물 모두 5m grid에 맞춰 조립할 수 있는 구조다.

공동주택 prefab 25개의 YAML에서는 `LODGroup` component가 확인되지 않았다. 실제 renderer 수와 Mobile 성능은 구현 전 별도로 측정한다.

### 2.3 단독주택 한계

City Pack prefab 이름에는 `House`, `Home`, `Residential`, `Villa`, `Townhouse` 계열이 없다. 따라서 다음을 구분한다.

| 목표 | City Pack만으로 가능한가 | 처리 |
| --- | --- | --- |
| 중층 공동주택 단지 | 가능 | Apartment Stack·Corner·Door·Stairs 조합 |
| 저층 공동주택 가로 | 가능 | 기본 Apartment 층 module·Door·Roof로 낮게 구성 |
| 연립·도시형 저층 주거 | 제한적으로 가능 | 같은 5m module 반복을 줄이고 출입구·지붕·생활 소품으로 구분 |
| 단독주택 단지 | 직접 대응 불가 | Office·Shop을 주택으로 속이지 않음 |

현재 import된 Town Pack에는 House module·preset 131개가 있으므로 진짜 단독주택 경관은 Town Composition이 맡는다. Farm Pack의 `SM_Bld_Farmhouse_01`, `SM_Bld_Farmhouse_02`는 농촌주거 접경에만 제한적으로 사용한다. Town 단독주택, City 공동주택과 Farmhouse가 이어지는 기준은 [Unity Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)를 따른다.

## 3. Module 계층

```text
Level 0  5m Cell
  Road / Sidewalk / Lot / Green / Service
        ↓
Level 1  도로·주거 Composition Set
  직선·모서리·T자·십자 / 가로형·중정형·모서리형 block
        ↓
Level 2  Residential Block
  건물 + 생활마당 + 출입구 + 도로 접속
        ↓
Level 3  District Recipe
  여러 block + 연속 도로 graph + 공동수령·공원
```

Level 0 cell은 Scene에 무조건 개별 GameObject로 남겨야 한다는 뜻이 아니다. builder와 검증기가 공유하는 논리적 정렬 단위다.

[POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)의 첫 도시 풍경 12종은 Zone에 놓는 기능·경관 단위다. 이 문서의 도로 6종과 주거 block 8종은 그 아래에서 `공동주택 생활마당`, `공동수령 장소`, `도시 진입 교차로` 등을 조립하는 하위 module이다. 두 catalog의 이름과 계층을 섞지 않는다.

## 4. 5m Cell 문법

| Cell code | 한국어 이름 | 역할 |
| --- | --- | --- |
| `R` | 도로 cell | 차량이 통과하는 기본 도로 바닥 |
| `X` | 교차로 중심 cell | 차선 overlay를 줄인 교차 영역 |
| `W` | 인도 cell | 보행·가로등·화분·출입구 앞 여백 |
| `L` | 건축 대지 cell | Apartment module 또는 생활 소품 배치 |
| `G` | 녹지 cell | 나무·꽃·벤치·완충 공간 |
| `P` | 주차·하차 cell | ParkingLines와 차량 socket |
| `S` | 서비스 cell | 쓰레기·설비·후면 출입과 작업 여백 |

Road와 Sidewalk mesh가 각각 5m cell 전체를 점유하므로, 실제 구현에서는 같은 위치에 무조건 중첩하지 않는다. base와 line·arrow·crossing처럼 overlay 용도인 prefab과 독립 cell prefab을 source catalog에서 먼저 구분해야 한다.

## 5. Connector 계약

모든 도로 module은 footprint와 connector signature를 가진다.

```text
도로Connector
├─ 방향: 북 / 동 / 남 / 서
├─ 종류: 차량도로 / 단지진입 / 서비스도로
├─ 폭: 1U 또는 2U
├─ 진행방향: 양방향 / 진입 / 진출
└─ 높이·grid offset

보행Connector
├─ 방향
├─ 인도 폭
├─ 횡단보도 연결 여부
└─ 단지 출입구 연결 여부
```

첫 주거 도로는 `2U`, 즉 약 10m corridor를 기본으로 한다. 이는 두 개의 5m road cell을 나란히 사용하는 논리 폭이다. 실제 mesh가 표현하는 차선 폭은 Game View에서 다시 확인한다.

같은 세트의 A/B/C는 다음 조건을 공유한다.

- footprint 동일
- 도로·보행 connector 위치와 방향 동일
- 건물 출입구·차량 진입 가능 영역 동일
- 다른 것은 건물 variant, 수목·생활 소품과 상태 socket 수

따라서 `주거도로 십자교차로 A`를 B나 C로 바꿔도 주변 도로가 끊기지 않아야 한다.

## 6. 주거 도로 Composition Set

첫 도로 kit는 6종×A/B/C, 총 18개 후보로 구성한다. 이번에는 생성하지 않는다.

| 세트 이름 | 논리 footprint | 도로 connector | 주요 source | 변형 차이 |
| --- | --- | --- | --- | --- |
| 주거도로 직선 | `4U × 4U` = 약 20m×20m | 북·남 또는 동·서 | Road, Lines, Sidewalk Straight, light pole | 가로등·가로수·횡단시설 밀도 |
| 주거도로 모서리 | `6U × 6U` = 약 30m×30m | 인접한 두 방향 | Road Bare/base, Lines, Sidewalk Corner | 안쪽 녹지·바깥쪽 건물 접속 차이 |
| 주거도로 T자교차로 | `6U × 6U` | 세 방향 | Road Bare/base, Lines, Crossing, traffic light, Sidewalk Corner | 횡단보도 수와 안전시설 |
| 주거도로 십자교차로 | `8U × 8U` = 약 40m×40m | 북·동·남·서 | Road Bare/base, Lines, Crossing, traffic light, light pole | 신호형·무신호형·수목형 corner |
| 주거단지 출입로 | `4U × 6U` = 약 20m×30m | 외부 도로 1, 단지 내부 1 | Road, Sidewalk Dip, Sidewalk Merger, Entrance sign | 문주 대신 화분·표지·차량 대기 공간 |
| 주거단지 주차포켓 | `4U × 4U` | 도로 측면 접속 1 | ParkingLines, Sidewalk Dip, parking meter, 일반 차량 | 일반 주차·하차·공동수령 대기 socket |

### 6.1 직선 도로 cell 예시

아래 모양을 90도 회전해 동서 도로로도 사용한다.

```text
W R R W
W R R W
W R R W
W R R W

W = 인도
R = 차량도로
북·남에 2U 도로 connector
인도는 북·남의 보행 connector로 계속 연결
```

### 6.2 십자형 도로 cell 예시

```text
L L W R R W L L
L L W R R W L L
W W W R R W W W
R R R X X R R R
R R R X X R R R
W W W R R W W W
L L W R R W L L
L L W R R W L L
```

- 중심 `2U × 2U`의 `X`는 교차 영역이다.
- 네 방향에 폭 `2U`의 도로 connector가 있다.
- 네 corner의 `L`에는 건물을 넣지 않고 시야 triangle, 신호등, 화분·가로수와 인도 회전 공간을 둔다.
- 횡단보도는 중심 바로 바깥의 `R`에 배치한다.
- 신호등·도로 화살표는 진행 방향을 확인한 뒤 배치하며 좌우 반전으로 복제하지 않는다.

City Pack에는 이름상 완성된 `Road_Cross_Junction` prefab이 없다. 십자교차로는 Road base와 line·crossing overlay를 조합하는 후보이며, 겹침·z-fighting과 차선 연결은 구현 Gate에서 실제 Game View로 확인해야 한다.

## 7. 공동주택 Composition Set

주거 block kit는 8종×A/B/C, 총 24개 후보로 구성한다.

| 세트 이름 | 논리 footprint | 건물 구성 | 공간 구성 | 상태 socket 후보 |
| --- | --- | --- | --- | --- |
| 공동주택 가로형 블록 | `6U × 6U` = 약 30m×30m | Stack 2~3동 또는 floor 조합 | 도로 한 면, 후면 생활마당 | 주민·대표·출입구 |
| 공동주택 모서리 블록 | `6U × 6U` | Corner·Door Corner·Roof Corner | 인접한 두 도로와 corner 인도 | 주민·방문자·출입구 |
| 공동주택 중정형 블록 | `8U × 8U` = 약 40m×40m | Stack·일반 module을 ㄷ자 배치 | 중앙 `4U × 4U` 생활 중정 | 주민·대표·커뮤니티 interaction |
| 공동주택 생활마당 | `4U × 4U` | 인접 apartment의 inset | 벤치·화분·빨랫줄·우편함·수목 | 주민·대표·방문자 |
| 공동주택 공동수령 포켓 | `4U × 4U` | Apartment door 또는 cover 인접 | Sidewalk Dip·Van·상자 임시 위치 | 차량·화물·공동수령·대표·주민 |
| 저층 공동주택 가로 | `6U × 4U` = 약 30m×20m | 기본 floor·Door·Roof를 낮게 조합 | 작은 출입구와 앞마당 반복 | 주민·방문자·출입구 |
| 주거단지 녹지 완충 | `6U × 2U` = 약 30m×10m | 건물 없음 | tree·flower·bench·grass path | 보행·휴식 interaction |
| 주거단지 서비스 골목 | `6U × 2U` | apartment 후면·fire escape 인접 | washing line·trash·pipe·aircon·fence | 관리 작업·서비스 출입 |

### 7.1 A/B/C 구체화

| 세트 | A | B | C |
| --- | --- | --- | --- |
| 공동주택 가로형 블록 | Stack 2동, 출입구 1 | Stack 3동, 생활마당·우편함 | 높이 혼합, corner 연결과 후면 수목 |
| 공동주택 모서리 블록 | 낮은 Corner module | Stack Corner와 두 출입구 | 높은 배경동+낮은 전면동으로 camera 깊이 확보 |
| 공동주택 중정형 블록 | ㄴ자 2면 | ㄷ자 3면과 중앙 벤치 | 높이 혼합 ㄷ자와 공동수령 inset |
| 저층 공동주택 가로 | 동일 높이 3호 | Door·Roof variant로 4호 구분 | corner 1호와 작은 녹지·주차포켓 결합 |
| 공동수령 포켓 | 상자·대표 anchor 중심 | Van·화물·주민 대기 분리 | cover와 service entrance 포함 |

Apartment module 개수를 세대 수로 해석하지 않는다. 창문·문·건물 수는 환경 표현이며 실제 세대·주민·주문 수는 authorized aggregate가 별도로 제공한다.

## 8. District Recipe

Composition Set은 완성된 도시 전체 prefab이 아니라 교체 가능한 block이다. 여러 block을 다음 recipe로 조합한다.

### 8.1 십자형 공동주택 생활권

```text
┌──────────────┬──────────┬──────────────┐
│ 공동주택 A   │ 북쪽 직선│ 공동주택 B   │
│ 생활마당     │ 도로     │ 주차포켓     │
├──────────────┼──────────┼──────────────┤
│ 서쪽 직선도로│ 십자교차로│ 동쪽 직선도로│
├──────────────┼──────────┼──────────────┤
│ 중정형 C     │ 남쪽 직선│ 공동수령 D   │
│ 녹지완충     │ 도로     │ 서비스골목   │
└──────────────┴──────────┴──────────────┘
```

구성 규칙:

- 중앙에는 `주거도로 십자교차로` 하나를 둔다.
- 네 방향 직선도로는 같은 2U connector로 연결한다.
- 네 corner 주거 block의 출입구는 인도를 향한다.
- 공동수령 포켓은 교차로 중심이 아니라 한 block의 측면 도로에 둬 차량 정차가 교차로를 막지 않게 한다.
- 생활마당과 녹지 완충은 서로 다른 corner에 둬 화면 한쪽에만 수목이 몰리지 않게 한다.
- 서비스 골목은 camera 전경보다 후면에 두되 interaction이 필요하면 Object Focus route를 확보한다.

### 8.2 가로형 저층 주거 생활권

```text
[저층 주거 A][저층 주거 B][녹지]
════════ 주거도로 직선 ════════
[저층 주거 C][생활마당][주차포켓]
```

- City Pack만 사용할 때는 `일반 단독주택`이 아니라 `저층 공동주택 가로`로 명명한다.
- 3~4개 건물을 동일 간격으로 복사하지 않고 Door·Roof·Stairs와 setback을 바꾼다.
- 중층 Stack은 배경 끝에 한두 동만 배치해 Semi-Urban에서 Urban으로 높이가 점진적으로 올라가게 한다.

### 8.3 공동수령 중심 단지

```text
공동주택 가로형 ─ 생활마당 ─ 공동주택 모서리형
        │
   주거단지 출입로
        │
공동수령 포켓 ─ 주차포켓 ─ 외부 주거도로
```

- `ResidentialPickup`과 cargo가 있을 때만 공동수령 socket을 활성화한다.
- 환경 Van·상자와 실제 cargo View를 겹치지 않는다.
- 주민 대기 공간과 차량 하차 공간을 인도로 분리한다.

### 8.4 혼합밀도 주거단지

Farm→City 방향으로 다음 순서를 사용한다.

```text
Farmhouse 농촌주거 접경
  → Town 단독주택 생활권
  → 저층 공동주택 가로
  → 공동주택 가로형 block
  → 십자형 공동주택 생활권
  → 높은 Apartment Stack 배경
```

Farmhouse와 Town House가 사용되는 구간은 City-only catalog가 아니라 Farm↔Town 또는 Town↔City Transition catalog로 분리한다.

## 9. 도로 Graph 검증 규칙

도로를 시각적으로만 맞춰 놓지 않고 connector graph로 검증한다.

1. 내부 도로 connector는 반대 방향 connector와 정확히 한 번 연결된다.
2. 연결된 두 connector는 종류, 폭, 높이와 진행방향이 호환된다.
3. 연결되지 않은 connector는 World 경계, 명시적 막다른길 또는 미구현 확장 지점이어야 한다.
4. 십자교차로는 북·동·남·서 네 connector가 모두 존재한다.
5. 차량 route graph와 보행 route graph를 분리한다.
6. 단지 출입구는 인도 connector와 연결되고 차량 출입구는 Sidewalk Dip을 통과한다.
7. 주차포켓·공동수령 포켓은 통과 차선 위에 socket을 두지 않는다.
8. road arrow·yellow line·median·traffic light가 connector 진행방향과 충돌하지 않는다.
9. 도로 cell 중복, 틈, 서로 다른 높이와 overlay z-fighting을 검사한다.
10. 90도 회전 뒤에도 같은 검증을 통과해야 한다.

## 10. Building 배치 검증 규칙

- building pivot을 5m cell corner 또는 center 중 하나로 정규화한다.
- MeshCollider가 도로·인도 cell을 침범하지 않는다.
- Door·Stairs가 인도 또는 생활마당을 향한다.
- Corner prefab은 두 도로가 만나는 대지에만 사용한다.
- Apartment Stack을 전경에 연속 배치해 Farm·Market·공동수령을 가리지 않는다.
- camera 0·90·180·270도에서 전면·지붕 occlusion group을 확인한다.
- 전면 facade와 지붕을 숨기는 cutaway는 Presentation 상태이며 입주·영업 상태가 아니다.
- Apartment 수, 창문 수와 visible NPC 수로 실제 세대·인구를 추정하지 않는다.

## 11. 한국어 세트 이름과 Key 후보

사용자가 Hierarchy와 catalog에서 vendor 파일명을 보지 않고 목적을 찾을 수 있게 한다.

```text
주거도로.직선.A
주거도로.모서리.B
주거도로.T자교차로.C
주거도로.십자교차로.A
주거단지.출입로.B
공동주택.가로형블록.A
공동주택.중정형블록.C
공동주택.공동수령포켓.B
저층주거.공동주택가로.A
```

- 표시 이름은 `주거도로 십자교차로 A`처럼 완전한 한국어로 둔다.
- vendor prefab 이름은 builder의 source allowlist에만 남긴다.
- key는 경관 분류와 connector 의미를 나타내며 업무 stable ID로 사용하지 않는다.

## 12. Stateful Socket Schema 후보

```text
주거단지StatefulSockets
├─ 주민NpcSockets[]
├─ 대표NpcSocket?
├─ 방문자NpcSockets[]
├─ 차량접근Socket?
├─ 차량정차Sockets[]
├─ 공동수령화물Socket?
├─ 공동수령InteractionSocket?
├─ 공동주택출입구Sockets[]
├─ 관리작업Socket?
└─ 카드AnchorSocket?
```

socket이 비어 있어도 세트는 환경 경관으로 유효하다. 실제 View가 연결되면 해당 View가 stable ID, revision, mode와 source lineage를 소유한다.

## 13. 구현 순서 후보

### RR0 — 실제 bounds·pivot 조사

- Road·Sidewalk·Apartment 25개의 bounds와 pivot을 Editor에서 측정한다.
- source를 base cell, overlay, building module, accent로 분류한다.
- 5m grid 가정과 2U 도로 폭을 Game View에서 확인한다.

### RR1 — 도로 4종 최소 kit

- 직선·모서리·T자·십자 module A형만 만든다.
- connector graph, 90도 회전과 cell overlap을 EditMode에서 검증한다.
- 임시 평면에서 차량·보행 route가 이어지는지 확인한다.

### RR2 — 공동주택 4종 최소 kit

- 가로형·모서리형·중정형·저층 가로 A형을 만든다.
- 출입구·인도·building collider·camera occlusion을 확인한다.
- 건물 수가 canonical 세대 수로 사용되지 않는 architecture test를 둔다.

### RR3 — 십자형 생활권 Preview

- 네 주거 block과 중앙 십자교차로를 조합한다.
- 생활마당·주차포켓·공동수령 포켓을 배치한다.
- preview는 library·connector 검사 증거이며 최종 제품 Scene과 구분한다.

### RR4 — A/B/C 확장

- connector와 footprint를 유지하면서 14종×A/B/C, 총 42개 후보로 확장한다.
- 같은 source 조합의 중복과 연속 반복을 검사한다.
- PC/Android renderer·shadow·memory 측정 뒤 detail tier를 확정한다.

이번 요청에서는 RR0~RR4를 구현하지 않는다.

## 14. 완료 기준 후보

1. 5m grid와 실제 prefab bounds·pivot의 오차가 문서화된다.
2. 직선·모서리·T자·십자 도로가 connector graph로 연결된다.
3. 십자교차로 네 방향에 dangling connector가 없다.
4. road base·line·crossing overlay가 중복되거나 z-fighting하지 않는다.
5. 공동주택 block의 출입구가 보행로에 연결된다.
6. 공동수령과 주차 vehicle socket이 통과 차선을 막지 않는다.
7. A/B/C가 같은 footprint·connector signature를 가진다.
8. 환경 Apartment 수를 세대·주민 수로 해석하지 않는다.
9. City Pack만 사용한 저층 가로를 단독주택 단지라고 부르지 않는다.
10. camera 네 방향과 World/Zone/Object Focus에서 건물 occlusion을 검증한다.
11. 원본 City prefab·material을 수정하지 않는다.
12. 세트 prefab·catalog에는 업무 상태·stable ID·권한을 저장하지 않는다.
13. Preview Scene과 최종 Game View 증거를 구분한다.
14. Android 실측 전 renderer budget과 LOD 수치를 확정하지 않는다.

## 15. 관련 문서

- [Unity POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)
- [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
- [City·Farm World P0 기준선과 Asset Inventory](UnityCityFarmWorldP0Inventory.md)
- [Unity 도심마트 공동주택 주문자 집단 통합 설계](UrbanMarketResidentialOrdererGroupIntegrationDesign.md)
- [Unity 서버 상태와 3D World Projection 설계](UnityServerStateToWorldProjectionDesign.md)
