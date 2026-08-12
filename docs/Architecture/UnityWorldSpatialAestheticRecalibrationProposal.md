# Unity World 공간·미감 재조정 제안

> 상태: 제안
>
> 작성일: 2026-08-10
>
> 적용 대상: `ThreeRegionHubJourney`, `FarmHeroShowcase`, 감자 생산·유통 Lifecycle Scene과 이후 Farm·Town·Hub·City 통합 World
>
> 관련 기준: [Unity World Visual Art Direction·ART Pass 제안](UnityVisualArtDirectionAndPassProposal.md), [Unity World 시간대별 미술·색감 전환 제안](UnityWorldTimeOfDayVisualDirectionProposal.md), [공통 품목과 POLYGON Farm 식품 asset·HS·가격 연결표](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)

## 1. 제안 요약

현재 프로젝트는 서버·Simulation·Unity Presentation의 권위 경계, Farm–Town–Hub–City의 약 300m 간격, 시간대 변화, 감자 생산·포장·운송·검수·분기 Lifecycle과 60개 canonical 상품의 Farm asset catalog까지 확보했다. 반면 공간과 화면은 서로 다른 구현 Gate가 누적된 결과 다음 두 극단을 동시에 가진다.

- World Overview에서는 Region이 넓은 단색 지면 위에 작은 섬처럼 흩어져 보인다.
- Lifecycle 화면에서는 하단·우측 정보 패널이 World를 크게 가려 공간 체험보다 상태 dashboard가 주역이 된다.

따라서 다음 미술 작업은 오브젝트 추가나 Post-processing 강화가 아니라 아래 순서의 **공간·미감 재조정**이어야 한다.

```text
논리적 300m 거리 유지
  → Overview·Region·Task 공간 스케일 분리
  → 지형과 도로를 하나의 경관 회랑으로 재구성
  → Region별 silhouette·밀도·색 관계 정리
  → Lifecycle UI를 점진 공개 방식으로 축소
  → 서버·Simulation 상태를 공간 미술과 결합
  → 고정 카메라·모바일 성능으로 마감
```

목표 화면은 다음 한 문장으로 고정한다.

> **서로 떨어진 지역이 길과 지형으로 자연스럽게 이어지고, 가까이 들어가면 생산·생활·물류 상태가 읽히며, 데이터 UI가 World를 가리지 않는 따뜻한 로우폴리 지역사회.**

## 2. 현재 프로젝트 기준 진단

### 2.1 유지할 성과

- Farm·Town·Hub·City가 독립 Region이며 약 300m 간격의 route로 연결된다.
- Hub가 생산지와 생활권의 화물을 받아 City로 보내는 공유 결절점 역할을 가진다.
- Farm Hero의 3/4 구도, 따뜻한 조명, 작물 흔들림과 트랙터 움직임은 Region Focus 화면의 기준이 된다.
- Dawn·Morning·Midday·Afternoon·Golden Dusk·Night의 여섯 시간 앵커와 연속 보간이 존재한다.
- 감자 Lifecycle은 재배→수확→포장→운송→Hub 검수→처분→직접 판매 준비까지 stable ID와 lineage를 유지한다.
- canonical 상품 60개와 Farm asset 대응은 `Direct 18`, `Representative 10`, `Unmapped 32`로 분리된다.
- Synty prefab은 `VisualRoot → VisualKey/Catalog → prefab` 뒤에 있고 서버·Simulation authority가 아니다.

### 2.2 재조정이 필요한 문제

| 화면 층위 | 현재 관찰 | 사용자에게 생기는 문제 |
| --- | --- | --- |
| World Overview | Region 사이 거리는 확보됐지만 지형과 object 밀도가 거리에 비해 작다 | 넓은 World가 아니라 작은 시설을 올린 검증 보드처럼 보인다 |
| Region 경계 | Farm·Town·Hub·City 지면이 큰 직사각형 색면으로 나뉜다 | 서로 다른 Pack을 한 판에 붙인 느낌이 강하다 |
| 연결 도로 | 길고 곧은 road strip과 얇은 노선 표시가 중심이다 | 실제 이동 회랑보다 데이터 선 또는 임시 connector처럼 보인다 |
| 지형 | 굴곡은 추가됐지만 Overview silhouette와 도로 흐름을 바꿀 정도로 읽히지 않는다 | 전경·중경·배경의 깊이가 약하고 평면성이 남는다 |
| Hub | route가 모이지만 주변 landform·yard·건물군의 질량이 약하다 | 기능적으로는 중심이지만 시각적 중심으로 읽히지 않는다 |
| Lifecycle | 하단 action panel과 우측 가격 card가 화면의 큰 비율을 차지한다 | World object와 actor가 상태표의 배경으로 밀린다 |
| 데이터 미술 | 정확한 lineage와 상태가 주로 text panel에 집중된다 | 공간을 보고 업무 흐름을 이해하기 어렵다 |
| 상품 다양성 | 60개 catalog가 생겼지만 대표 공간은 여전히 감자 단일품목 중심이다 | 서버 다품목 구조가 World의 경관 다양성으로 이어지지 않는다 |

이 진단은 데이터·Simulation 구현의 완성도를 부정하지 않는다. 현재 화면은 각 기능 Gate의 정확성을 입증하는 데 성공했고, 이제 그 결과를 하나의 World 경험으로 다시 편집해야 한다는 뜻이다.

## 3. 핵심 결정: 300m는 유지하고 보이는 방식을 바꾼다

Region 간 약 300m는 실제 route 길이·차량 이동·공간 분리의 기준으로 유지한다. 문제를 해결하기 위해 좌표를 다시 붙이지 않는다. 대신 **논리 거리**, **카메라 가시 거리**, **경관 밀도**를 분리한다.

```text
Logical Distance
  서버·Simulation route, travel duration, stable anchor 관계

Visible Distance
  현재 카메라가 보여 주는 구간과 원근·fog·occlusion

Perceived Distance
  수목대·주택·교차로·경사·랜드마크를 통과하며 느끼는 여정
```

- World Overview는 네 Region의 관계와 전체 route만 읽힌다.
- Region Focus는 현재 Region과 다음 목적지 방향만 보여 준다.
- Task Focus는 40~80m 안팎의 업무 cluster와 actor를 주역으로 삼는다.
- 먼 Region은 작은 detail을 렌더링하지 않고 silhouette와 landmark만 남긴다.
- 도로가 300m를 한 번에 노출하지 않도록 완만한 곡선, 높이차, 수목대와 건물군으로 구간을 나눈다.

이 방식은 거리를 속이는 것이 아니라 같은 300m를 사람에게 읽을 수 있는 여러 장면으로 번역하는 Presentation 전략이다.

## 4. 세 가지 공간·카메라 스케일

### 4.1 World Overview

목적은 시설 상세가 아니라 관계 파악이다.

- Farm·Town·Hub·City가 각각 하나의 큰 silhouette로 보여야 한다.
- Hub는 네 방향 route가 수렴·분기하는 가장 명확한 결절점이어야 한다.
- 도로 폭과 roadside mass는 현재보다 Overview에서 읽힐 만큼 커져야 한다.
- World 외곽은 단색 plane의 끝이 아니라 산림대·낮은 구릉·도시 skyline으로 닫는다.
- 작은 World Text, 개별 상자, NPC detail은 끄고 선택 Region·route·상태만 절제된 표시로 남긴다.

### 4.2 Region Focus

목적은 장소의 성격과 다음 행동의 방향을 이해하는 것이다.

- 선택 Region이 화면의 주된 면적을 차지하고 인접 Region은 배경 landmark로만 보인다.
- `Large → Medium → Small` 덩어리 계층을 Region마다 고정한다.
- entrance·업무 anchor·출구 route가 한 화면에서 서로 겹치지 않아야 한다.
- 카메라 이동 때 전경 수목이나 건물이 업무 object를 가리면 renderer tier 또는 cutaway를 적용한다.

### 4.3 Task Focus

목적은 재배·포장·상차·검수·판매 준비 같은 한 행동을 공간 안에서 수행하는 것이다.

- 업무 object와 actor가 화면의 주역이고 UI는 상태를 보조한다.
- 기본 화면에서는 요약 bar와 선택 card만 보이며 상세 근거·lineage는 펼쳤을 때 나타난다.
- 패널을 닫아도 선택 object, 다음 가능한 행동과 제한 상태가 World marker로 구분돼야 한다.
- Animation 완료는 서버·Simulation 상태를 확정하지 않으며 canonical 재조회 또는 Tick 결과를 다시 표현한다.

## 5. Region별 공간 재구성

### 5.1 Farm — 생산 경관의 큰 리듬

```text
낮은 구릉·수목대
  → 여러 크기의 밭 cluster
  → Farmhouse·Barn·Yard
  → 집하 농로와 Town/Hub 출구
```

- 하나의 거대한 직사각 밭보다 2~4개의 crop block과 휴경·수로·농로를 조합한다.
- Farmhouse와 Barn을 흩어진 object가 아니라 마당을 공유하는 건물군으로 묶는다.
- 과수 asset은 road edge와 언덕선의 orchard belt로 사용해 전경·배경을 만든다.
- 60품목 catalog는 모든 품목을 동시에 전시하지 않고 season·scenario별 3~5개 cluster로 제한한다.
- `Representative` 품목은 진단·카드에서 대표 표현임을 표시하고 `Unmapped` 품목에는 잘못된 작물 prefab을 사용하지 않는다.

### 5.2 Town — 생활권의 연속된 frontage

- 주택을 독립 점으로 놓지 않고 Main Street·골목·작은 광장 주변에 군집시킨다.
- 현관·driveway·울타리·정원·우편함이 도로와 집 사이의 중간 스케일을 만든다.
- Farm 쪽에는 저밀도 농가와 나무, City 쪽에는 상점과 포장도로 비율을 점진적으로 늘린다.
- 주민 route와 화물 route가 같은 차선을 무조건 공유하지 않도록 보행·생활 pocket을 분리한다.

### 5.3 Regional Logistics Hub — 시각적 결절점

- 입고 Dock, 검수 canopy, 보관 block, outbound yard가 하나의 U자 또는 ㄷ자 질량을 만든다.
- 차량 회차와 대기 pocket이 route 흐름을 보여 주고 pallet·cone은 경계를 강조하는 작은 detail로만 쓴다.
- Hub 바닥은 큰 회색 사각형 한 장이 아니라 concrete apron, service road, drainage/edge와 완충 수목대로 분리한다.
- `ArrivedAtHub`, `Accepted`, `LossRecorded`, `OutboundCandidate`는 각각 다른 socket·marker에서 보이되 물리 도착으로 다음 상태를 자동 확정하지 않는다.

### 5.4 City — 도심 외곽에서 마트까지의 밀도 상승

- Hub에서 City로 들어갈 때 창고·주차·소형 상가→공동주택→마트 frontage 순으로 밀도를 높인다.
- 건물 수보다 skyline의 높이차와 교차로·보도의 중간 덩어리를 먼저 잡는다.
- 마트는 큰 간판 하나보다 loading rear, public entrance, pickup pocket의 서로 다른 면을 가져야 한다.
- 도시 object가 같은 높이·간격으로 반복되지 않도록 2~3개의 block rhythm을 만든다.

## 6. Region 사이 전환 회랑

| 회랑 | 공간 문법 | 배치할 중간 landmark |
| --- | --- | --- |
| Farm→Town | 농로, 배수로, 과수·방풍림, 드문 농가 | 풍차, 작은 다리, 농산물 표지, 버스 정류장 |
| Farm→Hub | 넓어지는 집하도로, 차량 대피 pocket, 창고 전조 | 급수탑, weigh/inspection 표지, roadside shed |
| Town→Hub | 생활도로에서 service road로 변화 | 주유·정비 성격 조형물, 소형 상가, 안전등 |
| Hub→City | 산업 완충지에서 도시 가로로 변화 | 물류 sign, 가로등 리듬, 창고 edge, 첫 공동주택 |
| Town→City | 주민 이동 간선과 생활 frontage | 공원 pocket, 횡단 지점, 버스 정류장, 연속 주택 |

중간 집과 조형물은 빈 공간을 채우기 위한 랜덤 장식이 아니라 다음 공간의 성격을 예고하는 표지다. 각 회랑은 `출발 성격 40% → 중립 전환 20% → 도착 성격 40%`의 점진적 변화로 구성한다.

## 7. 지형과 도로 재조정

### 7.1 지형

- Farm은 완만한 구릉과 밭 단차를 주고, Town은 작은 생활 plateau, Hub는 작업을 위한 평탄지, City는 높이차 있는 urban terrace로 구분한다.
- 지형 굴곡은 object 아래 장식이 아니라 도로의 곡선, 건물 배치와 camera occlusion을 결정해야 한다.
- 높은 terrain mesh 한 장으로 막지 않고 모바일에서 culling 가능한 low-poly cluster로 구성한다.
- World edge는 산·수목·도시 silhouette로 닫되 플레이 가능한 공간처럼 오인시키지 않는다.

### 7.2 도로

- connector용 직선 strip을 최종 road mesh·shoulder·ditch·sidewalk의 조합으로 치환한다.
- 장거리 도로는 한 번 이상 완만하게 휘고, 80~120m마다 landmark 또는 밀도 변화가 있어야 한다.
- route data line은 도로와 경쟁하지 않도록 선택·진행 중일 때만 나타나고 road surface와 명도 차이를 제한한다.
- 폭·회전 반경은 차량 presentation 경로와 맞추되 NavMesh나 follower가 업무 상태를 결정하지 않는다.

## 8. 미감 재조정

### 8.1 큰 면적부터 정리한다

```text
Ground·Terrain
  → Road·Building mass
  → Tree·Crop·Roof rhythm
  → Cargo·Sign·Interaction accent
```

- 현재 단색 녹색 base를 Region별 ground family와 transition blend로 분해한다.
- Farm의 흙·초록, Town의 cream·brick, Hub의 concrete·charcoal, City의 slate·blue-gray는 서로 다르되 같은 하늘·태양·명도 범위 안에 둔다.
- Synty 원본 material은 수정하지 않고 wrapper, ground, 전용 variant와 `MaterialPropertyBlock`으로 조정한다.
- Bloom·Fog·Color Grading은 큰 형태와 조명이 정리된 뒤 마지막 통합 수단으로만 사용한다.

### 8.2 빛과 시간

- World 전체는 같은 태양 방향과 시간 source를 공유한다.
- Farm은 지면과 작물의 따뜻한 반사, Town은 창문·정원의 생활감, Hub는 기능 조명, City는 점진적 인공조명으로 같은 시간에 다르게 반응한다.
- Night는 화면 전체를 파랗게 만드는 방식보다 road light·window·dock light의 계층으로 장소를 유지한다.
- 데이터 상태 색은 시간대 색보정 뒤에도 읽히도록 별도 luminance·shape 검증을 한다.

### 8.3 움직임

- Farm: 작물 sway, 느린 작업 차량, 제한된 농부 동선
- Town: 주민 Idle/Walk, 생활차량, 작은 환경 motion
- Hub: 명시된 cargo 상태에 맞춘 Van·forklift·작업등
- City: 보행·교통의 낮은 밀도, 마트 pickup activity

Ambient motion은 화면을 살리는 역할만 하며 exact 수량·업무 완료·위험 발생을 의미하지 않는다.

## 9. Lifecycle UI와 데이터 미술 재편

현재 Lifecycle 화면의 정보 정확성은 유지하되 표현을 세 층으로 나눈다.

```text
Always Visible
  현재 단계, 상태, 다음 허용 행동, Operational/Simulation 구분

Context Card
  선택 object의 수량·가격·제한·freshness 요약

Expanded Evidence
  lineage, source, revision, 후보 관계, 계산 근거
```

- 하단 전체 폭 panel은 기본적으로 한 줄 action rail과 작은 상태 card로 축소한다.
- 우측 가격 card는 선택 시 열고, 이동·관찰 중에는 compact badge로 접는다.
- `Reset`, `Confirm`, `Tick` 같은 개발 제어는 일반 사용자 action과 시각적으로 분리하거나 진단 mode에 둔다.
- HarvestLot→PackageLot→Cargo→Inspection→Disposition lineage는 긴 text 한 줄 대신 선택 object 사이의 강조 route와 단계 chip으로 표현한다.
- 정확한 수량은 card에 유지하고, World의 상자·작물 밀도는 `Exact`인지 `BoundedSymbolic`인지 PresentationModel에서 구분한다.
- UI를 모두 닫았을 때에도 선택 object, 제한 상태, 다음 destination이 공간 안에서 읽혀야 한다.

## 10. 서버·Simulation·60품목 catalog 연결

```text
Server / Simulation Snapshot
  → CanonicalProductStableId·state·revision·lineage
  → Perspective / PresentationModel
  → FarmProductVisualCatalog
  → Direct | Representative | Unmapped
  → VisualKey / Prefab / marker / card
```

- 60개 상품을 Scene에 미리 모두 생성하지 않는다. 현재 scenario와 카메라에 필요한 품목만 resolve한다.
- `Direct`는 같은 품목군 시각을 사용할 수 있다는 뜻이며 HS·가격 관계의 Confirmed를 뜻하지 않는다.
- `Representative`는 World와 card에 대표 표현 표시를 유지한다.
- `Unmapped`는 generic crate·label 같은 중립 placeholder만 사용하고 다른 작물로 가장하지 않는다.
- 가격·생산량·재고 변화가 작물·상자 수에 반영될 때 exact/representative scale과 상한을 명시한다.
- Simulation Tick과 운영 snapshot은 같은 미술 adapter를 사용할 수 있지만 상태 badge와 source 표시는 분리한다.

## 11. 단계별 구현 계획

| 단계 | 작업 | 완료 Gate |
| --- | --- | --- |
| `WORLD-R0` 증거 기준선 | World·Farm·Town·Hub·City와 대표 Lifecycle 고정 카메라, 화면 점유율·가림·draw 범위 기록 | 같은 카메라의 Before PNG와 문제 목록이 고정됨 |
| `WORLD-R1` 카메라·스케일 | Overview/Region/Task camera family, focus 전환, detail tier, UI 기본 접힘 | Overview 관계와 Task object가 각각 주역으로 읽힘 |
| `WORLD-R2` 지형·도로 | Region 지형 silhouette, 곡선 회랑, shoulder·ditch·sidewalk, World edge | 단색 보드와 긴 strip 인상이 사라지고 300m 경로가 연속 경관으로 읽힘 |
| `WORLD-R3` 공간 밀도 | Region별 Large/Medium/Small mass와 회랑 landmark 배치 | UI 없이도 네 Region과 전환 구간을 구분할 수 있음 |
| `WORLD-R4` 색·조명 | ground palette, 공통 태양, shadow/contact, 여섯 시간 반응 통합 | 같은 World가 시간에 따라 변하되 Region 정체성과 데이터 색을 보존함 |
| `WORLD-R5` Lifecycle UI | action rail, compact context card, expanded evidence, 진단 제어 분리 | 기본 Task 화면에서 World가 주된 화면 면적을 유지함 |
| `WORLD-R6` 데이터 미술 | 60품목 mapping, cargo/route/inspection 상태, exact·symbolic density | panel을 닫아도 흐름과 상태를 읽고 오인 가능성을 테스트로 차단함 |
| `WORLD-R7` 성능 마감 | distance culling, LOD/renderer tier, shadow·FX tier, 실제 모바일 측정 | 대표 기기 측정값과 품질 tier가 기록되고 시각 Gate를 유지함 |

`WORLD-R0~R2`를 첫 재조정 묶음으로 진행하고, 그 결과가 통과하기 전에는 대량 prop 추가나 강한 Post-processing 작업을 시작하지 않는다.

## 12. 첫 실행 범위

첫 구현은 별도 기능을 더 만드는 작업이 아니라 기존 `ThreeRegionHubJourney`와 대표 감자 Lifecycle 한 장면을 대상으로 한다.

1. 현재 Game View를 Before 기준선으로 고정한다.
2. Overview·Farm·Hub·Task 네 카메라를 동일 해상도에서 비교한다.
3. 논리 anchor와 route는 그대로 두고 카메라·지형·도로·큰 mass만 조정한다.
4. Farm→Hub 한 회랑에만 곡선 도로, 높이차, 수목대, 중간 landmark를 완성한다.
5. 감자 재배 또는 Hub 검수 화면 하나에서 UI를 compact/expanded로 분리한다.
6. 동일 카메라 After PNG와 side-by-side contact sheet로 판단한다.

권장 Unity 작업 브랜치는 `codex/world-spatial-art-recalibration`이다. 기존 증거 Scene과 PNG는 유지하고, 첫 Gate가 통과한 뒤 canonical integration Scene에 반영한다.

## 13. 검증 기준

### 공간·미감

- Overview에서 Farm·Town·Hub·City와 route 방향이 3초 안에 구분되는가.
- 각 Region Focus에서 dominant landmark가 하나이며 다른 Region과 경쟁하지 않는가.
- 도로가 thin line이 아니라 이동 가능한 경관 회랑으로 보이는가.
- World edge와 큰 직사각 ground 경계가 카메라에서 노출되지 않는가.
- 조명과 fog를 꺼도 Large/Medium mass가 유지되는가.

### 데이터·업무

- UI를 접어도 현재 선택·상태·다음 목적지가 읽히는가.
- Representative·Unmapped 품목이 Direct로 오인되지 않는가.
- NPC·차량·animation 완료가 canonical 상태를 바꾸지 않는가.
- exact quantity와 symbolic density가 Presentation 계약에서 구분되는가.
- Operational·Simulation·Candidate 상태가 시간대와 관계없이 구분되는가.

### 기술·성능

- World·Region·Task 고정 Game View PNG를 남긴다.
- Scene/prefab/material 변경은 EditMode와 PlayMode를 모두 확인한다.
- draw call, renderer, shadow caster, Animator, particle, frame time을 대표 카메라별로 기록한다.
- 성능 목표치는 측정 전 임의로 선언하지 않고 대표 Windows·Android 기기 기준선 뒤 확정한다.
- Synty 원본 prefab/material을 직접 수정하지 않는다.

## 14. 이번 제안의 비대상

- Region 간 논리 거리를 다시 축소하는 것
- 새 운영 주문·계약·배차·재고 상태를 Unity에서 생성하는 것
- 60품목 모두를 한 Scene에 배치하는 것
- `Unmapped` 품목을 비슷한 Farm asset으로 자동 대체하는 것
- 실제 사용자 주소나 실제 화물 위치를 도로변 주택·차량에 연결하는 것
- 새 asset 구매로 현재 구도 문제를 먼저 덮는 것
- Bloom·Fog·Color Grading만으로 완성도를 올리는 것

## 15. 완료 정의

공간·미감 재조정은 오브젝트 수가 늘어났을 때가 아니라 다음 조건을 모두 만족할 때 완료된다.

1. 300m 거리와 서버·Simulation route 관계가 보존된다.
2. Overview·Region·Task가 서로 다른 정보 밀도와 카메라 문법을 가진다.
3. Farm·Town·Hub·City가 지형·도로·silhouette·색으로 구분되면서 하나의 World로 보인다.
4. 대표 Lifecycle 화면에서 World가 dashboard의 배경으로 밀리지 않는다.
5. 60품목 mapping 상태와 cargo lineage가 시각적으로 표현되지만 자산이 Domain authority가 되지 않는다.
6. 고정 Game View, EditMode·PlayMode와 실제 기기 성능 증거가 함께 남는다.

