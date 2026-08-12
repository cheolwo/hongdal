# Unity Farm 시설하우스·밭·논 단지 Modular Composition 설계

## 1. 목적과 상태

이 문서는 POLYGON Farm으로 다음 농업 공간을 반복 구성하기 위한 구체적인 module 문법을 정의한다.

- 비닐하우스 또는 온실이 병렬로 놓인 시설재배 단지
- 밭고랑·작물열·밭머리·농로가 연결된 밭 단지
- 논길·논둑·농수로와 사각 필지가 반복되는 논 단지
- 실제 Simulation 농장구획·재배작기·차량·화물을 환경 경관 안에 연결하는 socket

기준일은 2026-08-09이다. 이번 범위는 문서화뿐이며 Unity prefab·Scene·catalog·builder, Simulation·서버 코드와 vendor asset은 변경하지 않는다.

## 2. 실제 Asset 조사 결과

### 2.1 시설하우스

직접 확인된 건물은 두 개다.

- `SM_Bld_Greenhouse_01`
- `SM_Bld_Greenhouse_Large_01`

두 prefab에는 문과 투명 외피 child가 포함돼 있다. 다만 Synty의 asset 이름은 `Greenhouse`이며 실제 재질이 한국 농촌의 비닐 피복을 정확히 표현하는지는 이번 파일 조사만으로 확정하지 않았다.

따라서 문서와 내부 catalog에서는 `시설하우스`를 기본 한국어 이름으로 사용한다. Game View에서 외피가 비닐하우스로 충분히 읽히는 것이 확인된 뒤 사용자-facing 이름을 `비닐하우스`로 좁힌다.

### 2.2 밭·작물열

| Asset family | 확인 수 | 역할 |
| --- | ---: | --- |
| Dirt·Dirt Row 계열 | 9 | 평평한 흙, 두둑·고랑, 중앙·끝·mound·skirt 경계 |
| 환경 채소열 `Vege_Rows` | 3 | 멀리서 반복 농지로 읽히는 비권위 환경 작물 |
| 흙길 `Road_Dirt` | 9 | 직선·모서리·끝·교차·T자·swerve 농로 |
| Generic Ground | 12 | 평지·흙·밀·언덕·tile 배경 지형 |
| Corn plant | 4 | 옥수수 생육 크기 variant |
| Wheat plant·ground·FX | 다수 | 밀밭과 수확 상태의 환경 표현 |
| Ground plant 3계열 | 12 | 품목을 확정하지 않는 일반 작물열 |
| 과수 | 8종 | 사과·살구·체리·레몬·오렌지·복숭아·배·자두 과수원 후보 |

실제 품목·가격과 연결되는 asset 범위는 [Unity POLYGON Farm 식품 Asset·HS·가격 연결 조사](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)를 따른다. 환경 작물열과 generic plant는 상품 stable ID를 만들지 않는다.

### 2.3 관수·급수와 농장 설비

- `SM_Prop_Sprinkler_01`
- `SM_Prop_Sprinkler_Hose_01`
- `FX_Sprinkler_01`, `FX_Sprinkler_Large_01`
- `SM_Prop_Watering_Can_01`
- `SM_Prop_Well_01`
- `SM_Bld_WaterTower_01`
- `SM_Prop_Windmill_01`
- `SM_Veh_Attach_Trailer_Tank_01`

이 asset은 시설하우스·밭의 관수 분위기를 만들 수 있다. FX 재생이나 물 object의 존재가 실제 관수 실행·토양수분·작업 완료를 뜻하지 않는다.

### 2.4 논 표현의 공백

Farm Pack prefab 이름에서는 다음 전용 asset을 찾지 못했다.

- `Rice`, `Paddy` 작물
- 담수된 논 수면
- 논둑·논두렁 전용 module
- 농수로·배수로·수문
- 모내기·벼 생육·수확 단계

따라서 Farm Pack만으로 `실제 벼가 자라는 담수 논`을 완성할 수 있다고 보지 않는다. Wheat나 generic crop을 벼라고 부르지 않고, Watermelon 같은 이름에 포함된 `Water`를 물 환경 asset으로 오인하지 않는다.

논 단지는 두 단계로 분리한다.

| 단계 | 가능한 범위 | 표시 이름 |
| --- | --- | --- |
| 논 공간 Blockout | 사각 필지, 논길, 논둑 자리, 농수로 자리와 급수시설 socket | `논 단지 Blockout` |
| 논 전용 Visual | 담수면, 논둑, 농수로, 벼 생육 단계가 추가된 상태 | `논 단지` |

Blockout 단계에는 쌀 `1006` 상품·가격 카드를 연결하지 않는다.

## 3. Farm Grid 기준

City Pack은 collider에서 5m grid를 확인했지만 Farm의 Dirt Row·Dirt Road는 같은 방식의 BoxCollider bounds가 없어 이번 조사에서 exact 크기를 확정하지 못했다.

World 연결을 위해 다음 논리 단위를 후보로 둔다.

```text
F = Farm grid unit
초기 후보 F = 5m
```

- `F=5m`는 City grid와 도로 접속을 쉽게 하기 위한 authoring 후보이지 검증 완료 수치가 아니다.
- RR0과 같은 실제 bounds·pivot 조사 뒤 `F`를 확정한다.
- 긴 시설하우스와 밭고랑은 1F cell에 억지로 scale하지 않고 여러 cell을 점유한다.
- 원본 prefab scale은 유지하고 footprint가 grid에 맞지 않으면 wrapper에 여백을 포함한다.

## 4. Cell 문법

| Cell code | 한국어 이름 | 역할 |
| --- | --- | --- |
| `D` | 밭흙 cell | 평평한 흙 또는 두둑·고랑 바닥 |
| `C` | 환경 작물 cell | generic crop·Vege Row·옥수수·밀 환경 표현 |
| `A` | 실제 재배 cell | `농장구획`·`재배작기`가 연결되는 상태 socket 영역 |
| `H` | 시설하우스 cell | Greenhouse footprint와 출입 여백 |
| `R` | 농로 cell | Tractor·작업차가 이동하는 흙길 |
| `B` | 경계 cell | 두렁·울타리·skirt·식생 완충 |
| `I` | 관수·농수로 예정 cell | hose·sprinkler 또는 future irrigation module |
| `W` | 작업띠 cell | 밭머리, 자재·수확물·회차 공간 |
| `G` | 수목·녹지 cell | 방풍·완충 수목과 과수 배경 |
| `P` | 논필지 예정 cell | 논 전용 Visual을 나중에 넣는 Blockout 영역 |

`A`와 `C`를 구분하는 것이 중요하다. `C`는 넓은 농지로 보이게 하는 환경이고, `A`만 실제 stable ID·revision·생육 상태를 가진다.

## 5. Connector 계약

```text
농로Connector
├─ 방향: 북 / 동 / 남 / 서
├─ 폭: 1F 또는 2F
├─ 차량 허용: Tractor / Trailer / 일반 작업차
└─ 회차·교행 가능 여부

필지Connector
├─ 밭 경계
├─ 작물열 방향
├─ 밭머리 방향
└─ 인접 필지 결합 가능 여부

관수Connector
├─ 급수 유입 / 배수 유출
├─ hose·sprinkler / future canal
├─ 높이·방향
└─ 실제 관측·작업 socket 연결 여부
```

같은 A/B/C 변형은 footprint, 농로·관수 connector와 실제 재배 socket 위치를 유지한다. 작물 종류와 소품만 바뀌어 주변 단지 연결이 깨지는 구조는 허용하지 않는다.

## 6. 농로 Composition Set

밭·시설하우스·논 단지가 같은 route 문법을 사용하도록 농로 6종을 먼저 둔다.

| 세트 이름 | 후보 footprint | connector | 주요 source | 용도 |
| --- | --- | --- | --- | --- |
| 농로 직선 | `2F × 6F` | 북·남 또는 동·서 | Road Dirt Straight | 필지 사이 긴 통과로 |
| 농로 모서리 | `4F × 4F` | 인접 두 방향 | Road Dirt Corner 01·02 | 필지 corner 회전 |
| 농로 T자교차로 | `4F × 4F` | 세 방향 | Road Dirt T Section | 작업동·필지 분기 |
| 농로 십자교차로 | `4F × 4F` | 네 방향 | Road Dirt Intersection | 단지 중심 route node |
| 농로 완만굽이 | `4F × 6F` | 두 방향 | Road Dirt Swerve 01·02 | 자연스러운 전경·경계 연결 |
| 농로 끝·회차장 | `4F × 4F` | 한 방향 | Road Dirt End 01·02 + 빈 흙 | Tractor 회차·작업 대기 |

후보 footprint는 connector 여백을 포함한 논리 크기다. 실제 prefab bounds를 측정하기 전 원본 mesh 크기라고 단정하지 않는다.

## 7. 시설하우스 Composition Set

첫 시설하우스 kit는 6종×A/B/C, 총 18개 후보로 구성한다.

| 세트 이름 | 후보 footprint | 구성 | 상태 socket 후보 |
| --- | --- | --- | --- |
| 시설하우스 단동 | `2F × 8F` = 약 10m×40m | Greenhouse 01 + 양측 서비스 여백 | 시설·작기·농부·출입구 |
| 시설하우스 대형동 | `2F × 10F` = 약 10m×50m | Greenhouse Large 01 + 출입·후면 작업띠 | 시설·작기·농부·출입구 |
| 시설하우스 병렬단지 | `8F × 10F` = 약 40m×50m | 단동 또는 대형동 3열 + 사이 작업로 | 시설들·작기들·농부·차량 |
| 시설하우스 관수동 | `4F × 6F` = 약 20m×30m | Greenhouse + sprinkler·hose·water tower 시각 연결 | 센서·관수작업·시설·농부 |
| 시설하우스 육묘·작업장 | `4F × 6F` | Greenhouse + 일반 plant·wheelbarrow·상자 | 작기·농부·화물·interaction |
| 시설하우스 출하마당 | `6F × 6F` = 약 30m×30m | 출입구, 농로 끝·회차, pallet·상자·Trailer | 차량·화물·농부·출하 interaction |

### 7.1 A/B/C 변형

| 세트 | A | B | C |
| --- | --- | --- | --- |
| 시설하우스 단동 | Greenhouse 01, 출입구 1 | hose·작업소품 추가 | 측면 수목·상자와 후면 작업띠 |
| 시설하우스 대형동 | Large 1동 | 관수 socket·Trailer 접근 | 배경 WaterTower·Windmill과 서비스 진입 |
| 시설하우스 병렬단지 | 같은 크기 3동 | 소형·대형 혼합 3동 | 2동+빈 확장부로 반복 완화 |
| 관수동 | sprinkler 1계통 | hose·급수탑 연결 | 관수 시설과 작업자·센서 focus |
| 출하마당 | 상자·차량 socket 최소 | pallet·상자와 농부 대기 | Trailer 회차·화물 분리 공간 |

병렬 배치에서는 모든 하우스 출입구가 서비스 농로를 향하게 한다. 구조물 사이에 농부·wheelbarrow가 지나갈 작업로와 관수 hose가 겹치지 않을 여백을 둔다.

## 8. 밭 Composition Set

첫 밭 kit는 8종×A/B/C, 총 24개 후보로 구성한다.

| 세트 이름 | 후보 footprint | 환경 구성 | 상태 socket 후보 |
| --- | --- | --- | --- |
| 밭고랑 단일필지 | `6F × 8F` = 약 30m×40m | Dirt Row center·end·skirt | 농장구획·재배작기 |
| 밭고랑 연속필지 | `12F × 8F` = 약 60m×40m | 두 필지+중간 경계·작업로 | 구획들·작기들·농부 |
| 혼합작물 필지 | `8F × 8F` = 약 40m×40m | Corn·Wheat·Vege Row·generic crop 분할 | 실제 작기 socket은 별도 한정 |
| 실제 감자밭 필지 | 기존 6×6 tile 영역+환경 경계 | Dirt Row·울타리·식생, 중앙은 비워 둠 | 실제감자밭·농부·센서 |
| 밭머리 작업띠 | `6F × 2F` = 약 30m×10m | wheelbarrow·도구·상자·빈 흙 | 농부·차량·화물·작업 |
| 관수설비 포켓 | `4F × 4F` = 약 20m×20m | sprinkler·hose·well·water tower | 센서·관수작업·interaction |
| 과수원 블록 | `8F × 8F` | 같은 과수의 규칙적 열+경계 수목 | 과수 구획·작기·농부 |
| 휴경·준비 필지 | `6F × 8F` | Dirt·mound·skirt, 작물 최소 | 구획·준비작업 |

### 8.1 밭고랑 방향

- 한 필지 안의 row 방향은 모두 일치시킨다.
- row 끝은 밭머리 작업띠를 향한다.
- Tractor route는 row 중심을 가로지르지 않고 밭머리에서 진입한다.
- 인접 필지는 A/B/C에서 row 방향을 90도 바꿀 수 있지만 같은 필지 내부에서 섞지 않는다.
- Dirt Row 환경 renderer 개수를 실제 재식 수량으로 사용하지 않는다.

### 8.2 혼합작물 필지

```text
B C C C W C C C B
B C C C W C C C B
B C C C W C C C B
B W W W W W W W B
B C C C W C C C B
B C C C W C C C B

B = 경계
C = 환경 작물
W = 작업띠
```

혼합작물은 경관 밀도를 위한 환경 구성이다. 실제 Simulation 감자·토마토 등의 stable ID가 필요한 경우 해당 구역을 `A` socket으로 비우고 별도 View를 연결한다.

## 9. 논 단지 Blockout Set

논 전용 Visual이 없으므로 첫 단계는 6종의 공간 Blockout 후보만 정의한다.

| 세트 이름 | 후보 footprint | 현재 표현 | 추가 asset Gate |
| --- | --- | --- | --- |
| 논필지 사각형 | `8F × 8F` = 약 40m×40m | Flat Dirt/Ground와 경계 자리 | 담수면·벼·논둑 |
| 논필지 긴형 | `6F × 12F` = 약 30m×60m | 긴 사각 Blockout | 담수면·벼·논둑 |
| 논두렁 경계 | `1F` 폭 strip | Dirt Skirt·Grass 경계 후보 | 실제 논둑 mesh·보행 폭 |
| 농수로 예정띠 | `1F` 폭 strip | 빈 connector·표식만 | 수로·물·수문·배수 |
| 논길 직선·교차 | 농로 module 재사용 | Dirt Road route | 논길 폭·교행 검증 |
| 양수·관리 포켓 | `4F × 4F` | Well·WaterTower·작업 anchor | 실제 양수·관개 설비 표현 |

### 9.1 논 단지 cell 예시

```text
B P P P I P P P B
B P P P I P P P B
B P P P I P P P B
R R R R R R R R R
B P P P I P P P B
B P P P I P P P B
B P P P I P P P B

P = 논필지 예정 영역
B = 논두렁 예정 경계
I = 농수로 예정 connector
R = 논길·농로
```

Blockout에서는 `P`에 Wheat·Corn·generic crop을 채우지 않는다. 물과 벼가 없는 상태를 숨기기 위해 다른 작물을 넣으면 논·밭·상품 identity가 섞인다.

## 10. 단지 Recipe

### 10.1 시설하우스 병렬단지

```text
수목 완충 ┃ 시설하우스 A ┃ 작업로 ┃ 시설하우스 B ┃ 작업로 ┃ 시설하우스 C
           ┗━━━━━━ 출입구가 같은 서비스 농로를 향함 ━━━━━━┛
                        밭머리·출하마당
                              │
                         농로 T자교차로
```

구성 규칙:

- 긴 축을 같은 방향으로 정렬한다.
- 하우스 사이 작업로와 외곽 차량 농로를 구분한다.
- 관수동은 단지 중앙 또는 급수 source 쪽에 둔다.
- 출하마당은 하우스 문 바로 앞을 막지 않고 단지 끝에 둔다.
- 높은 WaterTower·Windmill은 배경에 두고 camera 전경의 하우스를 가리지 않는다.

### 10.2 십자형 밭 단지

```text
┌──────────────┬──────────┬──────────────┐
│ 감자 실제필지│ 북쪽 농로│ 혼합작물 필지│
│ 밭머리 작업띠│          │ 관수 포켓    │
├──────────────┼──────────┼──────────────┤
│ 서쪽 농로    │ 농로십자 │ 동쪽 농로    │
├──────────────┼──────────┼──────────────┤
│ 과수원 블록  │ 남쪽 농로│ 휴경 준비필지│
│ 수목 완충    │          │ 출하마당     │
└──────────────┴──────────┴──────────────┘
```

- 실제 감자 6×6은 한 corner의 명확한 `A` 영역에만 둔다.
- 다른 필지는 Farm 전체가 넓어 보이게 하는 환경이지만 상품·상태를 소유하지 않는다.
- 중앙 농로 십자는 Tractor·Trailer route를 연결한다.
- 밭머리·출하마당은 서로 인접시키되 실제 cargo lineage는 별도 View로 유지한다.

### 10.3 논 Blockout 단지

```text
[논필지 A]┃농수로 예정┃[논필지 B]
━━━━━━━━━━ 논길 ━━━━━━━━━━
[논필지 C]┃농수로 예정┃[양수 관리포켓]
```

- 농수로 connector는 낮은 쪽으로 일관된 방향을 갖게 한다.
- 논길이 농수로를 가로지르는 곳에는 future culvert·bridge socket을 둔다.
- 실제 물 흐름이나 수위를 Simulation하지 않는다.
- 논 전용 Visual이 준비되기 전 최종 홍보 Game View에 완성된 논으로 사용하지 않는다.

### 10.4 혼합 농업 생활권

```text
수목·Farmhouse 배경
  → 시설하우스 단지
  → 밭 단지
  → 논 Blockout 또는 future 논 단지
  → Farm Yard·Produce Stand
  → Rural Road
```

시설하우스·밭·논이 한 화면에 모두 보이더라도 실제 Simulation 대상은 선택된 좁은 vertical slice만 연결한다.

## 11. 한국어 Set 이름과 Key 후보

```text
농로.직선.A
농로.십자교차로.B
시설하우스.단동.A
시설하우스.병렬단지.C
시설하우스.출하마당.B
밭.밭고랑단일필지.A
밭.실제감자밭필지.B
밭.과수원블록.C
논Blockout.사각필지.A
논Blockout.농수로예정띠.B
```

- 표시 이름은 `시설하우스 병렬단지 A`처럼 한국어로 둔다.
- `논Blockout`은 논 전용 Visual 완료 전까지 key와 화면 badge에서 숨기지 않는다.
- vendor 파일명은 builder source allowlist에만 둔다.
- Composition key를 농장구획·재배작기·센서·작업 stable ID로 사용하지 않는다.

## 12. Stateful Socket Schema 후보

```text
농업단지StatefulSockets
├─ 농장구획Sockets[]
├─ 재배작기Sockets[]
├─ 실제감자밭Socket?
├─ 농업센서Sockets[]
├─ 농부NpcSockets[]
├─ 차량Sockets[]
├─ 농기계Sockets[]
├─ 화물Sockets[]
├─ 시설출입구Sockets[]
├─ 관수작업Socket?
├─ 공동작업InteractionSockets[]
└─ 상품가격CardAnchorSockets[]
```

- Greenhouse 외형이 재배작기나 시설 운영상태를 소유하지 않는다.
- Sprinkler FX가 켜졌다는 이유로 관수 작업을 완료하지 않는다.
- crop renderer 수로 수확량을 계산하지 않는다.
- 상품·가격 카드는 실제 `ProductStableId`가 연결된 anchor에서만 연다.

## 13. 성능·표현 규칙

- Overview에서는 개별 작물 renderer 대신 Vege Row·optimized Wheat와 큰 필지 pattern을 우선한다.
- Zone Focus에서는 실제 재배 View와 밭고랑·경계·관수 설비를 표시한다.
- Object Focus에서만 작은 작물·hose·상자·도구와 sensor detail을 활성화한다.
- 같은 Greenhouse·Dirt Row를 연속 복사할 때 A/B/C, 180도 방향, 작업띠·수목 여백을 조합한다.
- 작물 row는 무작위 회전하지 않고 필지의 긴 축과 일치시킨다.
- 투명 Greenhouse가 겹치는 구도와 Mobile overdraw를 별도로 측정한다.
- 물·투명 외피·sprinkler FX는 Mobile detail tier에서 독립적으로 끌 수 있어야 한다.
- LOD, instancing, renderer budget은 실제 Unity 측정 전 수치로 확정하지 않는다.

## 14. 구현 전 조사·구현 Gate

### GF0 — Bounds·pivot·분류 조사

- Dirt·Dirt Row·Road Dirt·Greenhouse의 실제 bounds와 pivot을 Editor에서 측정한다.
- base·edge·end·overlay·complete building을 source catalog에서 구분한다.
- `F=5m` 후보와 City Transition 접속 오차를 확인한다.

### GF1 — 농로·밭 최소 kit

- 농로 직선·모서리·T자·십자 A형을 만든다.
- 밭고랑 단일필지·실제 감자필지·밭머리 작업띠 A형을 만든다.
- connector, row 방향, 실제 감자 socket과 environment authority 부재를 검증한다.

### GF2 — 시설하우스 최소 kit

- 단동·대형동·병렬단지·출하마당 A형을 만든다.
- 출입구, 작업로, 차량 회차와 transparent overdraw를 검증한다.
- `Greenhouse`가 실제 시설·작기 상태를 소유하지 않음을 고정한다.

### GF3 — 논 Blockout

- 사각필지·논두렁 예정·농수로 예정·논길 A형을 primitive와 명시적 `Blockout` badge로 검증한다.
- rice·water visual 없이 상품·가격·생육 상태가 나타나지 않는지 확인한다.

### GF4 — 논 전용 Visual Gate

- 벼 생육 단계, 담수면, 논둑, 농수로·수문 asset을 선정한다.
- shader·수면 overdraw·Android 성능과 계절 표현을 검증한다.
- 이 Gate 뒤에만 `논 단지` 이름과 쌀 상품 anchor를 허용한다.

### GF5 — A/B/C와 District Preview

- 농로 6종·시설하우스 6종·밭 8종을 A/B/C로 확장한다.
- 논은 Blockout과 전용 Visual 상태를 별도 catalog로 유지한다.
- 시설하우스·밭·논 recipe를 preview에서 검사한 뒤 실제 Farm Zone에 필요한 것만 배치한다.

이번 요청에서는 GF0~GF5를 구현하지 않는다.

## 15. 완료 기준 후보

1. Farm grid와 실제 source bounds·pivot의 오차가 문서화된다.
2. 농로 직선·모서리·T자·십자가 connector graph로 연결된다.
3. 밭고랑 방향, 밭머리와 Tractor 진입 방향이 일치한다.
4. 시설하우스 출입구가 작업로·출하마당으로 연결된다.
5. A/B/C는 같은 footprint·connector·상태 socket 위치를 유지한다.
6. 실제 감자 6×6과 환경 작물열이 분리된다.
7. Greenhouse·작물·FX가 시설 상태·수확량·관수 완료를 소유하지 않는다.
8. Wheat나 generic crop을 Rice로 표시하지 않는다.
9. 논 전용 asset 전에는 `논 단지 Blockout` 상태를 숨기지 않는다.
10. 쌀 상품·가격 카드는 실제 논·작기 ProductStableId가 연결된 뒤에만 활성화한다.
11. 원본 Farm prefab·material을 수정하지 않는다.
12. Overview·Farm Zone·Object Focus별 detail tier를 구분한다.
13. 투명 하우스·작물 반복·물·FX의 PC/Android 성능을 측정한다.
14. Library Preview와 최종 Game View 증거를 구분한다.

## 16. 관련 문서

- [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
- [City·Farm World P0 기준선과 Asset Inventory](UnityCityFarmWorldP0Inventory.md)
- [Unity POLYGON Farm 식품 Asset·HS·가격 연결 조사](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)
- [Unity Farm 상품·가격 카드 상호작용 흐름](UnityFarmProductPriceCardInteractionFlow.md)
- [Unity City 주거단지·십자형 도로 Modular Composition 설계](UnityCityResidentialRoadModularCompositionDesign.md)

