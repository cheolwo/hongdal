# Unity World Visual Art Direction·ART Pass 제안

> 상태: 제안
>
> 적용 대상: Farm·Town·City·Regional Logistics Hub를 포함하는 Ssalddel Unity World Presentation
>
> 현재 기준 화면: `C:\Users\user\ssalddel\Documentation\Changes\2026-08-10-three-region-hub-journey\three-region-hub-journey-playmode.png`
>
> 상위 구조: [Unity Data·World Interpretation·Perspective·Presentation 기준 아키텍처](UnityDataInterpretationPresentationArchitecture.md)
>
> 공간 구성 기준: [Unity Composition Set 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md), [Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)

## 1. 제안 요약

현재 Unity World는 Data·Simulation·Presentation 권위 경계와 Farm→Town→Hub→City 관통 구조를 검증하는 데 성공했지만, 최종 미술 화면으로는 아직 부족하다. 다음 시각 작업은 효과를 하나씩 추가하는 방식이 아니라 아래 의존 순서의 별도 `ART` 트랙으로 진행한다.

```text
ART0 Visual Direction
  → ART1 Composition
  → ART2 Color Relationships
  → ART3 Lighting
  → ART4 Atmosphere and Post-processing
  → ART5 Motion and Life
  → ART6 Data-integrated Art
  → ART7 Camera, Accessibility and Performance Finish
```

한 줄 기준은 다음과 같다.

> **따뜻하고 평온한 지역사회 안에서 생산·생활·유통 데이터가 조용히 살아 움직이는, 깨끗하고 읽기 쉬운 로우폴리 미니어처 World**

`ART0~ART3`이 통과하기 전에는 Fog·Bloom·강한 Color Grading으로 화면을 완성된 것처럼 보정하지 않는다. Post-processing은 공간·색·빛을 대신하는 기능이 아니라 이 셋을 마지막으로 묶는 마감재다.

## 2. 현재 화면에 대한 사실 기반 진단

2026-08-10 `ThreeRegionHubJourney` Play Mode Overview 한 장을 기준으로 확인한 내용이다. 이 평가는 해당 화면의 시각 완성도에 대한 것이며 CMP5의 데이터·경로·권위 경계 구현 성과를 부정하지 않는다.

### 유지할 강점

- Farm·Town·Hub·City anchor가 분리되어 전체 관계를 시험할 수 있다.
- 감자밭, 주택, Hub, 공동주택과 차량이 각 영역의 의미를 최소한 전달한다.
- 사람과 화물 route가 기능적으로 연결되어 이후 움직임·데이터 연출의 기반이 있다.
- Synty asset과 Ssalddel wrapper·stable ID·lineage 경계가 분리되어 있다.

### 시각적으로 부족한 점

- 넓은 단색 지면 위에 object가 서로 고립되어 `World`보다 검증용 배치판처럼 보인다.
- 도로·보도·농로가 연속적인 공간망이 아니라 색이 다른 긴 strip과 connector로 읽힌다.
- Farm의 밭을 제외하면 큰 덩어리·중간 덩어리·작은 detail의 계층이 약하다.
- Zone별 dominant landmark와 silhouette 차이가 작고, Hub가 World의 시각적 결절점으로 충분히 보이지 않는다.
- 전경·중경·배경의 겹침이 약해 카메라 깊이와 이동 거리가 평면적으로 느껴진다.
- 나무·울타리·가로등·작업 소품·주변 건물 같은 경관 문법이 적어 생활 밀도가 낮다.
- 접지 그림자와 명암 분리가 약해 차량·건물·작물이 바닥 위에 놓인 모형처럼 뜬다.
- 하늘과 World 경계의 큰 빈 면적이 시선을 분산시키며 화면 중심이 약하다.
- 작은 World text는 Overview에서 읽히지 않으면서 경관의 정돈감도 해친다.

이 화면은 `CMP5 A형 관통 구조 기준선`으로는 유효하지만 `ART1 Composition` 완료 화면으로 분류하지 않는다.

## 3. ART 트랙과 기존 트랙의 관계

ART는 Domain·Simulation·Data 계약을 만드는 트랙이 아니다. 이미 결정된 World 의미를 카메라에서 아름답고 읽기 쉽게 만드는 Presentation 품질 트랙이다.

```text
CMP: 공간 모듈·connector·anchor·route 구조
ANIM: animation source·intent·adapter
FARM/SC/FPC: 업무 vertical slice와 데이터 의미
ART: 위 결과를 하나의 일관된 게임 화면으로 구성하는 시각 규칙과 품질 Gate
```

의존 관계는 다음과 같다.

- `ART0`은 즉시 시작할 수 있고 모든 후속 시각 작업의 기준이 된다.
- `ART1`은 CMP5 A형 World를 사용해 시작하되, CMP7~CMP8의 실제 사용처를 결정하는 입력이 된다.
- `ART2~ART3`은 대표 A형 구도가 안정된 뒤 진행한다.
- `ART5`는 ANIM2·ANIM3와 vehicle adapter를 소비한다.
- `ART6`은 CMP6/FPC 감자 카드와 cargo·inventory Presentation을 소비한다.
- `ART7`은 CMP11의 최종 품질·성능 증거와 함께 닫는다.

CMP와 ART를 완전히 순차적으로 끝내지 않는다. `CMP → 고정 카메라 ART 검토 → 필요한 CMP 수정 → 다음 ART Gate`의 짧은 반복으로 진행한다. 다만 뒤 단계 효과로 앞 단계 실패를 숨기지는 않는다.

## 4. ART0 — Visual Direction 고정

### 4.1 미술 핵심 문장

```text
따뜻한 지역사회
  + 깨끗한 Synty low-poly silhouette
  + 생산·생활·유통이 연결된 미니어처 공간
  + 출처 있는 데이터가 절제된 움직임과 상태로 드러나는 World
```

### 4.2 다섯 가지 Art Pillar

| Pillar | 화면에서 보여야 하는 것 | 피해야 하는 것 |
| --- | --- | --- |
| 따뜻한 공동체 | 사람·집·농장·상점이 도로와 생활 소품으로 연결됨 | 비어 있는 산업 simulation board |
| 읽기 쉬운 미니어처 | 명확한 silhouette, 3/4 시점, 큰 형태 우선 | 작은 prop과 text에 의존한 설명 |
| 살아 있는 생산·유통 | 작물·사람·차량·cargo의 느리고 목적 있는 움직임 | 무의미한 과밀 NPC·FX |
| 절제된 데이터 미술 | 상태가 공간 object·route·빛·motion과 결합됨 | World 위를 덮는 dashboard wall |
| 교체 가능한 Synty 표현 | vendor asset은 `VisualRoot` 아래에서 조합됨 | Synty 이름·색·animation을 Domain 권위로 사용 |

### 4.3 정서와 시간대

첫 대표 시간대는 맑은 날의 늦은 오전 또는 이른 오후로 고정한다.

- 밤·비·황혼보다 형태와 색을 읽기 쉬운 기준 시간대를 먼저 만든다.
- 지나치게 노란 sunset이나 차가운 cinematic teal-orange를 기본 정서로 사용하지 않는다.
- 위험·긴급·오류를 날씨 전체 변화로 과장하지 않는다.
- Operational과 Simulation 구분은 전역 색조가 아니라 명시적 UI·token·label로 유지한다.

### 4.4 ART0 산출물

- 한 문장 Visual Direction
- Art Pillar 5개와 금지 예시
- Farm·Town·Hub·City reference board
- 기준 시간대와 camera family
- 고정 Before 카메라 5개: World, Farm, Town, Hub, City
- `Must Preserve`, `May Change`, `Do Not Use` 목록

완료 Gate: 새 화면을 보지 않은 작업자도 같은 장면을 구성할 수 있을 정도로 방향이 구체적이며, “현실적”, “예쁘게”, “Synty 느낌” 같은 단어만으로 끝나지 않는다.

## 5. ART1 — Composition과 공간 밀도

가장 먼저 고쳐야 할 단계다. 조명과 효과를 끈 상태에서도 공간이 읽혀야 한다.

### 5.1 화면 계층

각 카메라는 다음 세 크기의 덩어리를 가져야 한다.

```text
Large: Region silhouette·주요 밭·건물군·도로 방향
Medium: 작업장·마당·주택군·Dock·교차로·수목군
Small: cargo·상자·농기구·표지·가로등·작물·NPC
```

작은 detail을 늘려 Large·Medium 구도의 실패를 가리지 않는다.

### 5.2 Region별 dominant landmark

| Region | Large landmark | Medium rhythm | Small accent |
| --- | --- | --- | --- |
| Farm | 감자밭+Barn/Farmhouse 군 | 농로·Farm Yard·Greenhouse·수목대 | 작물열·상자·농기구·농부 |
| Town | 주택군+Main Street | driveway·정원·작은 상점·공원 | 우편함·벤치·생활차량·주민 |
| Hub | 입출고 Hall+Dock yard | canopy·pallet lane·차량 대기 포켓 | cone·pallet·forklift·작업등 |
| City | 공동주택 skyline+도심마트 | 교차로·보도·상점 frontage | 가로등·차량·수령 point·보행자 |

한 카메라에서 대형 landmark 네 개가 같은 크기와 대비로 경쟁하지 않게 한다. Zone Focus는 해당 Zone landmark를 주역으로, 다른 Region은 depth와 방향을 제공하는 배경으로 둔다.

### 5.3 연결 공간

- Farm dirt road → Town driveway/asphalt → Hub industrial apron → City road가 실제 mesh 흐름으로 이어져야 한다.
- connector 검증용 색 strip은 최종 Game View에서 도로·보도·경계 경관으로 치환한다.
- road 주변에는 fence, ditch, tree cluster, lamp, sign, cargo 또는 작업 여유 공간 중 하나의 이유가 있어야 한다.
- Region 사이 빈 공간은 단순 공백이 아니라 전환 경관·시야 휴식·성능 buffer 중 하나로 정의한다.
- Hub는 지도 중앙에 놓였다는 사실뿐 아니라 여러 route가 수렴하고 다시 분기하는 형태로 보여야 한다.

### 5.4 전경·중경·배경

- 전경: 낮은 crop·fence·tree canopy edge·parked vehicle 일부로 화면 진입점을 만든다.
- 중경: 현재 선택 Zone의 업무 anchor와 actor를 둔다.
- 배경: 다음 Region의 silhouette와 route 방향을 보여 주되 detail을 낮춘다.
- 카메라 앞을 완전히 가리는 나무·건물은 focus 전환 시 cutaway 또는 renderer tier로 제어한다.

### 5.5 밀도 규칙

밀도는 object 수가 아니라 의미 있는 cluster와 간격의 리듬이다.

- 랜드마크 주변: 높은 밀도
- 업무 route와 entrance: 중간 밀도, 이동 가독성 우선
- Region 전환: 낮은 밀도에서 다음 cluster로 점진 증가
- 안전한 작업 여유 공간: 비워 두되 fence·ground variation·marking으로 의도된 공백임을 표시
- World 외곽: 수목대·terrain edge·배경 silhouette로 보드의 끝처럼 보이지 않게 처리

완료 Gate: UI·text·post-processing을 꺼도 각 Region과 Hub를 구별할 수 있고, 생산→집하→분류→도시 배송의 공간 흐름을 한 장의 Game View로 설명할 수 있다.

## 6. ART2 — Color Relationship과 Palette

Synty 원본 material을 무작정 recolor하지 않는다. Region의 색 성격은 asset 선택 비율, ground·road, 공통 빛, Ssalddel 전용 variant와 작은 accent로 만든다.

| 영역 | 주된 색 성격 | 보조 색 | 피할 것 |
| --- | --- | --- | --- |
| Farm | 작물 초록·따뜻한 흙·목재 | barn red, straw gold | 전체가 같은 녹색 plane으로 합쳐짐 |
| Town | cream·brick·정원 초록 | 따뜻한 roof·생활차량 | Farm과 구분 없는 흙색, City식 회색 과다 |
| Hub | neutral concrete·charcoal | safety yellow/orange, cargo blue | 화면 전체를 경고색으로 채움 |
| City | slate·asphalt·절제된 blue-gray | 상점·차량의 따뜻한 accent | 채도 낮은 회색 덩어리 또는 과한 neon |
| Shared World | 같은 하늘·햇빛·명도 범위 | cargo·route 공통 accent | Pack마다 다른 게임처럼 보이는 tonality |

### 색 적용 우선순위

1. ground·road·building mass의 큰 면적 비율
2. 수목·crop·roof·vehicle의 중간 면적 리듬
3. sign·cargo·interaction의 작은 accent
4. 상태 token과 selection feedback

상태 색은 원본 material을 바꾸지 않고 marker, outline, emissive detail 또는 `MaterialPropertyBlock`으로 적용한다. 빨강·초록만으로 상태를 구분하지 않고 icon·shape·text를 함께 사용한다.

데이터 값의 많고 적음을 장식 object 수로 표현할 때는 exact quantity인지 bounded symbolic density인지 Presentation 계약에 명시한다. 임의로 cargo를 많이 쌓아 실제 재고가 많다고 오인하게 하지 않는다.

완료 Gate: grayscale에서도 형태 계층이 유지되고, color 화면에서는 label 없이 Region 성격이 구분되며, 상태 accent가 환경 palette보다 먼저 눈에 띄되 World 전체를 지배하지 않는다.

## 7. ART3 — Lighting과 Shadow

좋은 빛 하나로 Synty silhouette, 높이, 도로와 접지감을 동시에 살린다.

### 7.1 기준 조명

- `WorldBootstrap`이 공통 Sun·environment·shadow 기준을 소유한다.
- Farm·Town·Hub·City는 같은 시간대와 그림자 방향을 공유한다.
- Directional Light는 카메라 정면과 완전히 같은 방향을 피하고, 건물 옆면과 지붕이 분리되는 사선 방향을 사용한다.
- shadow 길이는 높이와 거리감을 보여 주되 업무 route와 interaction surface를 검게 덮지 않게 한다.
- ambient는 그늘 정보를 없애지 않는 범위에서 Synty의 어두운 면을 읽히게 한다.

### 7.2 접지와 깊이

- 건물·차량·NPC·crop은 contact shadow 또는 검증된 대체 표현을 가져야 한다.
- 밭 이랑은 Overview에서도 반복 방향이 보이는 grazing light를 검토한다.
- 투명 Greenhouse, tree canopy와 큰 City 건물의 shadow cost·가림을 별도 측정한다.
- shadow caster 수와 distance는 PC/Mobile quality tier로 분리한다.

### 7.3 조명 비교 방법

같은 고정 카메라·같은 scene state에서 다음을 비교한다.

1. unlit에 가까운 neutral 기준
2. Sun 방향·강도만 적용
3. ambient/environment 적용
4. shadow softness·distance 적용

이 단계에서는 Fog·Bloom·강한 Color Grading을 끈다.

완료 Gate: object가 바닥에 붙어 보이고, roof·wall·ground가 분리되며, Farm 작물과 City 도로가 같은 날씨 안에 있으면서도 각각 읽힌다.

## 8. ART4 — Atmosphere와 Post-processing

ART1~ART3 결과를 묶는 단계다.

### 권장 순서

1. Sky와 horizon/background color
2. 먼 Region만 뒤로 미는 제한적 depth fog
3. 형태 접촉을 보조하는 낮은 Ambient Occlusion
4. 전체 명도·대비를 묶는 Tonemapping과 미세한 Color Adjustments
5. 실제 밝은 emissive·야간 light가 있을 때만 제한적 Bloom

### 금지 사항

- 빈 공간을 Fog로 숨김
- 약한 구도를 vignette로 강제
- 모든 밝은 표면이 번지는 Bloom
- Farm·Town·City의 원래 색 관계를 없애는 강한 LUT
- mobile 검증 없이 SSAO·transparent FX·shadow를 누적
- test component가 섞인 기존 Volume profile 재사용

완료 Gate: post-processing을 꺼도 화면이 성립하고, 켰을 때 depth·통일감·초점만 개선되며 데이터 marker의 색·가독성이 변질되지 않는다.

## 9. ART5 — Motion과 생활감

움직임은 화면의 생명감을 높이지만 업무 권위와 분리한다.

| 종류 | 예시 | 상태 연결 |
| --- | --- | --- |
| Ambient motion | 작물·수목의 미세 흔들림, chimney smoke | 환경 Presentation, canonical 업무 상태 없음 |
| Route motion | 주민·농부·직원 Walk, Van·Pickup 이동 | snapshot의 이동 intent 표현 |
| Work motion | 밭갈이·검수·상하차·진열 | 확정된 Task 상태 표현, 완료 권위 없음 |
| Feedback motion | 선택 pulse·route trace·card transition | Presentation interaction |
| Time/condition motion | 작업등·비·관수·먼지 | 허용된 weather/task snapshot 표현 |

모든 것을 동시에 움직이지 않는다. World Overview는 큰 route·주요 vehicle 위주, Zone Focus는 NPC·설비, Object Focus는 작업과 feedback detail을 강화한다.

`NpcMovementPresenter`, vehicle route follower와 Synty adapter는 [Unity 외부 Reference Pattern 선별 도입 제안](UnityExternalReferencePatternAdoptionProposal.md)의 경계를 따른다. animation·particle 완료는 Simulation Tick이나 operational Command를 발생시키지 않는다.

완료 Gate: 정지 화면과 비교해 생활감이 증가하지만 시선이 분산되지 않고, 움직임을 모두 꺼도 데이터와 공간 의미가 유지된다.

## 10. ART6 — 데이터와 미술의 통합

데이터 표현은 World 위에 추가된 별도 dashboard가 아니라 공간·object·motion·절제된 UI의 조합이어야 한다.

```text
Data Snapshot
  → Interpretation
  → Presentation token / symbolic density / route intent
  → World object state + small marker + optional detail card
```

| 데이터 의미 | World 표현 후보 | 반드시 보존할 제한 |
| --- | --- | --- |
| 감자 재배 상태 | crop visual stage·field edge marker | renderer 수로 실제 생산량 계산 금지 |
| cargo 수량 | bounded crate cluster·cargo socket fill | exact/symbolic 표현 구분, unit·revision 보존 |
| 이동 중 | 얇은 route trace·moving vehicle | 도착으로 배송 완료 확정 금지 |
| Hub 단계 | Dock light·zone occupancy·작업 actor | 입고·검수·보관 상태를 합치지 않음 |
| 마트 재고 부족 | shelf/facility 위 작은 상태 marker | 공개 가능 수량과 내부 재고 혼동 금지 |
| stale/offline | 낮은 채도·clock/source badge | 오래된 값을 정상으로 보정 금지 |
| blocked | 정지 motion·block icon·reason card | 빨간색만으로 이유 대체 금지 |

### UI 면적 원칙

- Overview에서는 label과 card를 최소화하고 Region·route·상태의 큰 관계만 보여 준다.
- Zone Focus에서는 선택 대상과 주변 업무 상태만 표시한다.
- Object Focus에서 source·as-of·unit·revision·limitation을 가진 상세 card를 연다.
- Screen-space UI는 landmark와 이동 actor를 가리지 않는 safe area를 사용한다.
- World-space text는 카메라 distance에 따라 숨기거나 icon으로 축약한다.

완료 Gate: UI를 숨겨도 데이터 흐름의 방향과 활성 지점이 보이고, UI를 켜면 출처·단위·상태 제한을 정확히 설명하며, 장식이 새로운 업무 사실을 만들지 않는다.

## 11. ART7 — Camera·접근성·성능 마감

최종 판단은 Scene View가 아니라 실제 Game View와 target device에서 한다.

### 11.1 고정 카메라 세트

| Camera | 목적 |
| --- | --- |
| World Overview | 네 Region·Hub·주요 route와 silhouette |
| Farm Focus | 밭·Barn/Farmhouse·Farm Yard·농부 |
| Town Focus | 주택군·Main Street·생활 route |
| Hub Focus | 다중 origin 수렴·Dock·cargo·차량 |
| City Focus | 공동주택·마트·도시 배송 |
| Journey Follow | actor/vehicle 이동과 foreground occlusion |
| Object Focus | 감자·cargo·card·상호작용 상세 |

- 기본 perspective는 45~55도 3/4 시점을 유지하되 target별 높이·FOV·focus distance를 고정한다.
- camera pan·zoom·90도 회전 후에도 landmark와 route가 가려지지 않는지 확인한다.
- foreground cutaway는 renderer를 무작위로 숨기지 않고 focus policy를 따른다.
- Android safe area와 작은 화면에서 World text·marker·card를 재검증한다.

### 11.2 Quality tier

| 항목 | PC 기준 | Mobile 기준 |
| --- | --- | --- |
| Shadow | 주요 building·vehicle·actor, 높은 distance/quality | focus 주변 우선, distance·cascade 축소 |
| AO | 낮은 강도의 검증된 SSAO 후보 | 기본 off 또는 매우 제한적 |
| Transparent | Greenhouse·선택 FX 허용 | 동시 노출과 overdraw 제한 |
| Environment detail | Zone/Object Focus에서 확장 | distance·focus tier로 비활성화 |
| Motion/FX | 주요 ambient+업무 motion | 동시 actor·particle budget 제한 |

수치는 Editor 순간값으로 확정하지 않고 Windows Player와 Android Player를 분리 측정한다.

### 11.3 최종 질문

1. 첫눈에 아름답고 들어가 보고 싶은가?
2. Farm·Town·Hub·City와 이동 방향이 설명 없이 읽히는가?
3. 업무 actor와 데이터 상태가 배경에 묻히지 않는가?
4. UI와 effect가 Synty 공간을 가리지 않는가?
5. 모바일에서도 같은 계층과 의미가 유지되는가?

완료 Gate: 고정 카메라 전체의 대표 PNG, Play Mode, Windows Player, Android Player 결과와 renderer·shadow·transparent·draw call·triangle·memory·frame time을 별도 기록한다.

## 12. 구현 순서와 현재 우선순위 제안

현재 상태에서는 `ART0 → ART1-A → ART2-A → ART3-A`를 작은 첫 미술 묶음으로 수행할 가치가 높다. CMP6 감자 상품·가격 Card의 데이터 계약을 막지는 않지만, Card의 최종 색·배치·motion은 ART2와 ART6 기준을 소비하게 한다.

| 단계 | 지금 수행할 범위 | 이번에 하지 않을 것 |
| --- | --- | --- |
| `ART0` | Visual Direction·Pillar·고정 camera·Before capture | Scene 대량 변경 |
| `ART1-A` | CMP5 Overview의 Region cluster, 연속 road, landmark, edge·depth blockout | 작은 prop 대량 배치 |
| `ART2-A` | Farm·Town·Hub·City ground/building 비율과 공통 명도 기준 | vendor material 일괄 recolor |
| `ART3-A` | 한 Sun 방향, ambient, contact shadow 기준 | Fog·Bloom·LUT |
| `CMP6/FPC` | 감자 데이터·카드 의미와 anchor 연결 | 미술 기준 없는 최종 카드 skin |
| `ART4` | ART1~3 통과 뒤 depth fog·AO·tone 마감 | 결함 은폐용 효과 |
| `ART5~6` | ANIM3·vehicle·cargo·감자 상태의 살아 있는 표현 | 무근거 군중·재고·업무 성공 연출 |
| `ART7/CMP11` | 카메라·접근성·PC/Android 최종 증거 | Editor 수치만으로 완료 선언 |

첫 시각 개선 절단선의 완료 문장은 다음과 같다.

> 같은 CMP5 데이터와 actor·cargo lineage를 유지한 채, Farm·Town·Hub·City가 연속된 도로와 경관 cluster로 연결되고, 고정 Overview와 Zone Focus에서 Region별 silhouette·색 관계·공통 조명이 읽히는 첫 Art Direction 기준선을 만들었다.

## 13. Presentation 전용 설정 구조 후보

반복 가능한 미술 규칙은 Scene object마다 임의로 조정하지 않고 Presentation 전용 profile로 모은다.

```text
WorldArtDirectionProfile
  MoodCode
  ReferenceCameraSet
  SharedValueRange
  GlobalAccentPolicy

RegionVisualProfile
  RegionCode
  Dominant / Support / Accent policy
  LandmarkKeys
  Ground / Road / Vegetation family
  Density tier

WorldLightingProfile
  Sun rotation / intensity
  Ambient / environment
  Shadow tier

WorldAtmosphereProfile
  Sky / fog / tonemapping
  AO / bloom quality tier

CameraArtProfile
  Focus kind
  FOV / pitch / distance
  Occlusion / detail tier
  UI safe area
```

이 profile은 Presentation 설정이며 server contract·Domain·Simulation에 넣지 않는다. Synty 원본 material·prefab을 직접 수정하지 않고 project-owned variant, catalog와 `MaterialPropertyBlock`만 사용한다.

## 14. 검증과 변경 기록

각 ART 단계는 같은 scene state와 camera transform의 Before/After 쌍으로 검증한다.

### 공통 증거

- World·Farm·Town·Hub·City Game View PNG
- camera transform·FOV·resolution·quality tier
- 적용한 profile revision과 변경한 visual catalog key
- active renderer·shadow caster·transparent renderer·particle·animator 수
- Console error·missing script·missing material
- 원본 Synty prefab·material dirty 여부

### 단계별 비교

- ART1: silhouette·negative space·landmark·route continuity overlay
- ART2: color image와 grayscale/value 비교
- ART3: shadow/contact·overexposure·underexposure 비교
- ART4: post-processing off/on 비교
- ART5: 정지 capture와 짧은 runtime sequence
- ART6: UI off/on, normal/stale/blocked/simulation 상태 비교
- ART7: PC와 Android built-player 비교

화면 변경은 `Documentation/Changes`와 장기 보존 대표 PNG에 기록한다. 중간 capture·profiling·raw log는 `artifacts/local`에 둔다.

## 15. 중단 조건

다음 조건에서는 뒤 ART 단계로 넘어가지 않는다.

- ART1 실패를 Fog·Bloom·vignette로 숨김
- connector·NavMesh·cargo socket을 시각 구도 때문에 끊음
- Synty 원본 prefab·material을 직접 수정함
- status color가 canonical 상태나 권한을 추론하게 함
- 장식 crate·NPC 수를 실제 수량·참여자 수로 오인하게 함
- animation·차량 도착·FX 완료가 업무 Command 또는 Simulation Tick을 발생시킴
- Overview의 unreadable World text를 계속 유지함
- 기준 camera가 달라 Before/After 비교가 불가능함
- Game View 없이 Scene View나 Inspector 수치만으로 통과 선언함
- Android built-player 검증 없이 mobile 완료를 선언함

## 16. 최종 수용 기준

ART 트랙은 다음 조건을 모두 만족할 때 완료한다.

1. UI를 끈 Overview만으로 Farm·Town·Hub·City의 성격과 연결 방향이 읽힌다.
2. 모든 주요 카메라에 Large·Medium·Small 형태와 전경·중경·배경이 존재한다.
3. Region은 색 성격이 다르지만 같은 하늘·빛·명도 범위 안의 하나의 World로 보인다.
4. 건물·차량·NPC·작물이 접지되고 route와 업무 공간이 그림자에 묻히지 않는다.
5. Post-processing을 꺼도 화면이 성립한다.
6. 최소한 한 사람 Journey, 한 차량 Journey와 한 업무 동작이 자연스럽게 보인다.
7. 감자·cargo·Hub·마트 상태가 World 미술과 결합되면서 source·revision·mode·limitation을 보존한다.
8. 카메라·UI·접근성·PC/Android 성능 검증이 각각 남아 있다.
9. Synty asset은 교체 가능한 Presentation 자산으로 남고 업무 권위를 소유하지 않는다.

이 제안의 목적은 “효과가 많은 화면”이 아니라, Ssalddel의 데이터와 공동체 의미가 공간·색·빛·움직임을 통해 자연스럽게 읽히는 완성도 높은 게임 World를 만드는 것이다.
