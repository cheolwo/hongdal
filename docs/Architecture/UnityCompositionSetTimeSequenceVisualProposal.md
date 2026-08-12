# Unity City·Town·Farm Composition Set 시간 순차 시각 변화 제안

## 1. 목적

이 문서는 City·Town·Farm Pack으로 구성한 Composition Set이 하루의 시간 순서에 따라 다음 세 요소를 일관되게 변화시키는 방법을 정의한다.

```text
시간의 흐름
  → 태양 위치와 그림자
  → World 밝기와 대기 명도
  → 표면·텍스처의 색·반사·emissive 반응
  → 지역 인공조명
```

목표는 시간대별로 prefab과 material을 복제하는 것이 아니라, 동일한 Composition과 동일한 데이터 상태가 시간에 맞는 미술 반응만 보이게 만드는 것이다.

이 문서는 [Unity World 시간대별 미술·색감 전환 제안](UnityWorldTimeOfDayVisualDirectionProposal.md)의 World-level 시간 구조를 Composition-level renderer·material·shadow 규칙으로 구체화한다.

## 2. 현재 상태와 적용 범위

현재 저장소에서 구현·catalog·검증이 확인되는 24개는 Farm의 8개 경관군×A/B/C 변형이다.

| Farm 경관군 | A/B/C | 합계 |
| --- | --- | --- |
| 감자밭 두렁 | 3 | 3 |
| 혼합 작물밭 | 3 | 3 |
| 헛간 작업마당 | 3 | 3 |
| 농기계 대기장 | 3 | 3 |
| 농산물 직판장 | 3 | 3 |
| 수확물 집하장 | 3 | 3 |
| 농로 교차로 | 3 | 3 |
| 수목 완충지 | 3 | 3 |
| **합계** |  | **24** |

City와 Town은 source 조사, 후보 Composition, 도로·Gate와 최소 A형 기반이 존재하지만 Farm 24개와 같은 전체 catalog가 모두 생성됐다는 뜻은 아니다. 첫 구현은 Farm 24개를 회귀 기준으로 삼고, City·Town의 검증된 subset과 향후 catalog가 동일한 시간 Presentation 계약을 소비하도록 한다.

따라서 이 제안의 적용 순서는 다음과 같다.

1. Farm 24개 전체 binding·회귀 검증
2. Town 대표 주택·생활도로·상가 subset
3. City 대표 공동주택·도로·마트·물류 subset
4. 검증된 Composition이 늘어날 때 profile catalog만 확장

## 3. 핵심 설계 원칙

1. **24개 prefab을 시간대 수만큼 복제하지 않는다.** 하나의 Composition prefab이 시간 Presentation 값을 받는다.
2. **A/B/C는 공간 변형이지 시간 변형이 아니다.** 같은 경관군의 A/B/C는 같은 시간 profile family를 공유한다.
3. **그림자를 먼저 계산한다.** 밝기와 색감은 태양 위치·그림자 구도가 확정된 뒤 조절한다.
4. **원본 텍스처를 직접 교체하지 않는다.** Synty 원본 material·texture를 수정하지 않고 project-owned profile과 `MaterialPropertyBlock`을 사용한다.
5. **표면 종류별로 반응한다.** 흙·작물·목재·아스팔트·콘크리트·금속·유리·간판이 같은 tint를 받지 않는다.
6. **날씨와 시간을 분리한다.** 시간만으로 흙이 젖거나 작물이 마르지 않는다. wetness·snow·rain은 별도 condition snapshot의 책임이다.
7. **시간은 업무 권위가 아니다.** 밤이 됐다고 영업 종료·배송 완료·주민 귀가를 확정하지 않는다.

## 4. 시간 순서와 여섯 시각 구간

World-level 앵커는 Dawn·Day·GoldenHour·Night를 유지하되, Composition 검증에서는 그림자와 표면 변화가 잘 드러나도록 여섯 구간으로 관찰한다.

| 순서 | 구간 | 대표 시각 | 그림자 | 밝기·ambient | 표면 반응 |
| ---: | --- | --- | --- | --- | --- |
| 1 | `Dawn` | 05:30 | 길고 부드러움, 서쪽 방향 | 낮은 명도, 청회색 fill | 흙·목재는 차분하고 잎 edge에 약한 따뜻함 |
| 2 | `Morning` | 08:30 | 중간 길이, 비교적 선명 | 밝기 상승, 깨끗한 하늘색 | 작물·주택 local color가 점진적으로 복귀 |
| 3 | `Midday` | 12:30 | 가장 짧음 | 가장 중립적이고 밝음 | 텍스처·상품·상태색 검증 기준 |
| 4 | `Afternoon` | 16:00 | 동쪽으로 길어짐 | 약간 따뜻한 key | 목재·벽·곡물의 warmth 증가 |
| 5 | `GoldenDusk` | 18:30 | 길고 부드러움, silhouette 강조 | 주황 key + 서늘한 fill | 지붕·수목·차량 edge, 창문 emissive 시작 |
| 6 | `Night` | 21:00 | 낮은 대비의 moon shadow 또는 제한적 shadow | 남청색 ambient, landmark fill | 유리·창문·간판·Dock emissive 중심 |

실제 runtime은 여섯 값을 계단식으로 교체하지 않고 연속 보간한다. 대표 시각은 미술 검증용 기준점이며 계절·위도·실제 천체 계산을 의미하지 않는다.

## 5. 프레임별 적용 순서

시간이 바뀔 때 모든 값을 임의 순서로 갱신하면 노출과 그림자가 튈 수 있으므로 다음 순서를 고정한다.

```text
1. TimeOfDayPresentationSnapshot 수신
2. 태양 azimuth·elevation 평가
3. Directional Light와 shadow parameter 적용
4. ambient·sky·fog·exposure 적용
5. Composition surface parameter block 적용
6. emissive·local light activation 적용
7. marker·route 대비 variant 적용
8. 변경 완료 frame 기록
```

전체 renderer를 매 frame 다시 검색하지 않는다. Composition이 생성될 때 semantic surface binding을 cache하고, 시간 profile 값이 유의미하게 변했을 때만 property block을 갱신한다.

## 6. 그림자 변화 규칙

### 6.1 태양과 그림자 방향

- Farm·Town·City는 하나의 World Sun을 공유한다.
- Composition을 회전해 배치해도 그림자는 World 방향을 따른다.
- 태양 고도가 낮아질수록 그림자가 길어지지만 top-down 가독성을 위해 시각적 최대 길이와 opacity를 제한한다.
- Noon 그림자는 짧고 비교적 선명하게, Dawn·GoldenDusk는 길지만 부드럽게 만든다.
- Night는 현실적인 완전 암흑 대신 낮은 강도의 moon key 또는 shadow off tier를 품질별로 선택한다.

### 6.2 Composition별 그림자 중요도

| 종류 | 주된 그림자 대상 | 보호할 요소 |
| --- | --- | --- |
| 작물·밭 | 작물 줄, 울타리, 수목 | 필지 경계·선택 tile·sensor marker |
| 작업마당 | Barn, Silo, 농기계 | 작업 socket·차량 route·actor |
| 도로·교차로 | 건물·수목·가로등 | 차선·Gate·횡단·route trace |
| 주택·상가 | 지붕·처마·수목 | 현관·보행로·선택 marker |
| 공동주택·마트 | 고층 facade·간판 | 출입구·공동수령·상품 card anchor |
| 물류거점 | canopy·vehicle·cargo | Dock 번호·입출고 Gate·cargo socket |

긴 그림자가 보호 요소를 가리면 태양 방향을 세트마다 바꾸지 않고 shadow strength, contact shadow, local fill 또는 marker backplate로 해결한다.

## 7. 밝기와 명도 변화 규칙

시간대 미술은 Light intensity만 바꾸지 않는다. 다음 값을 하나의 curve set으로 관리한다.

- Sun intensity와 color temperature
- ambient sky·equator·ground intensity
- exposure compensation
- fog 명도·색·거리
- shadow strength
- saturation·contrast의 제한적 보정
- local emissive activation

### 명도 계층

모든 시간대에서 다음 순서를 유지한다.

```text
선택 actor·vehicle·data marker
  > active landmark·route
  > 주요 건물·작업 공간
  > 환경 detail
  > distant background
```

Night에도 ground와 road를 같은 검정으로 만들지 않는다. Farm 흙길, Town 생활도로, City 아스팔트는 서로 구분되는 value band를 유지한다. Screen-space UI는 World exposure 영향을 받지 않게 하고 World-space marker에는 시간대별 outline·backplate를 적용한다.

## 8. 표면·텍스처 반응 체계

### 8.1 텍스처 교체보다 표면 parameter

시간대별 texture set을 24개 Composition에 복제하면 memory·batching·유지보수 비용이 커진다. 첫 구현은 albedo texture 자체를 바꾸지 않고 다음 parameter를 표면 종류별로 변화시킨다.

- base color multiplier
- brightness/value multiplier
- saturation multiplier
- smoothness/specular response
- emission color·intensity
- optional rim 또는 edge response
- fog·distance fade participation

실제 texture swap은 창문 emissive mask, 간판 on/off mask처럼 시간 의미가 분명하고 재사용 가능한 경우에만 허용한다.

### 8.2 Semantic Surface Group

```text
CompositionTimeSurfaceBinding
  ├─ GroundSoil
  ├─ CropLeaf
  ├─ Wood
  ├─ Asphalt
  ├─ Concrete
  ├─ Roof
  ├─ Metal
  ├─ GlassWindow
  ├─ Signage
  └─ EmissiveFixture
```

| Surface | Dawn | Midday | GoldenDusk | Night |
| --- | --- | --- | --- | --- |
| 흙·밭 | 낮은 채도, 서늘한 shadow | 원래 갈색 기준 | 따뜻한 적갈색 강조 | 형태가 남는 낮은 명도 |
| 작물·수목 | 청록 fill, 약한 warm edge | local green 기준 | 황록 edge와 따뜻한 rim | 과도한 청색화 금지, silhouette 유지 |
| 목재 | 차분한 갈색 | texture 기준 | warmth·grain 대비 증가 | 창문·등 주변만 국소적으로 읽힘 |
| 아스팔트·콘크리트 | 서늘한 중명도 | 중립 회색 기준 | warm key·cool shadow | route와 curb가 분리되는 value 유지 |
| 금속·차량 | 낮은 specular | 명확한 형태 | edge highlight | local light reflection을 제한적으로 표현 |
| 유리·창문 | sky reflection | 낮은 emission | emission fade-in | Town·City·Farmhouse의 점등 hierarchy |
| 간판·상태 socket | 비활성 장식 | local color 기준 | emissive 준비 | 업무 상태색과 구분되는 제한적 emission |

시간대가 바뀌어도 감자 상태, cargo 수량, 건물 영업 상태와 같은 실제 정보는 texture 반응으로 추론하지 않는다.

## 9. 24개 Farm Composition 적용 규칙

### 9.1 경관군별 profile family

24개 각각에 독립 값을 넣지 않고 8개 경관군별 family profile과 A/B/C 공통 binding schema를 둔다.

| 경관군 | 우선 surface | 시간대 핵심 표현 |
| --- | --- | --- |
| 감자밭 두렁 | GroundSoil·CropLeaf·Wood | 작물 줄 그림자와 필지 읽기 |
| 혼합 작물밭 | GroundSoil·CropLeaf | 작물군별 local color 유지 |
| 헛간 작업마당 | Wood·Roof·GroundSoil·Fixture | Barn 긴 그림자, 야간 출입구 조명 |
| 농기계 대기장 | Metal·GroundSoil·Fixture | 차량 silhouette와 제한적 작업등 |
| 농산물 직판장 | Wood·Signage·Fixture | GoldenDusk 온기, Night 간판·매대 범위 |
| 수확물 집하장 | GroundSoil·Wood·Metal·Fixture | cargo socket을 가리지 않는 shadow·light |
| 농로 교차로 | GroundSoil·Wood·Signage | 시간대 전체에서 connector·route 보존 |
| 수목 완충지 | CropLeaf·Wood | Dawn/GoldenDusk silhouette, background depth |

### 9.2 A/B/C 공유 규칙

- A/B/C는 같은 `SurfaceProfileFamilyKey`를 사용한다.
- renderer 수와 배치는 달라도 semantic group key는 동일하게 유지한다.
- 변형별 누락 surface는 허용하지만 잘못된 group과 중복 binding은 거부한다.
- A/B/C 중 하나만 시간 반응이 빠지는 경우 catalog validation 실패로 처리한다.

## 10. Town·City 확장 규칙

### Town

- 주택·driveway·정원·main street는 `Wood`, `Roof`, `GlassWindow`, `Asphalt`, `Fixture` 중심으로 binding한다.
- Night는 모든 창문을 켜지 않고 seed 기반 장식 pattern 또는 명시적 Presentation 입력을 사용한다.
- 창문 수를 주민 수·재택 여부로 해석하지 않는다.

### City

- 공동주택·마트·도로·도심 물류는 `Concrete`, `GlassWindow`, `Signage`, `Asphalt`, `Metal`, `Fixture`를 우선한다.
- 고층 그림자가 마트·공동수령·Dock을 가리지 않도록 World Sun 기준은 유지하고 local fill·marker 대비로 보정한다.
- 차량 light와 도로 light는 실제 배차·운행·영업 상태가 아니라 Presentation token일 때만 켠다.

Town·City catalog가 확정되면 Farm과 동일한 `CompositionTimeSurfaceBinding` 검증을 적용하고 pack별 별도 시간 시스템을 만들지 않는다.

## 11. Presentation 계약 후보

```text
CompositionTimeProfileCatalog
  ProfileRevision
  SurfaceProfiles[]
  CompositionBindings[]

CompositionTimeSurfaceProfile
  SurfaceProfileKey
  SurfaceKind
  TimeCurves
  QualityTierOverrides

CompositionTimeBinding
  CompositionKey
  ProfileFamilyKey
  RendererBindings[]
  EmissiveSockets[]
  LocalLightProxySockets[]

CompositionTimePresenter
  Apply(WorldTimeOfDayPresentationModel)
  ValidateWiring()
```

`CompositionKey`와 A/B/C signature는 기존 catalog를 재사용한다. 시간 profile에는 stable ID, revision, 가격, 수량, 업무 상태를 저장하지 않는다.

## 12. 구현 단계

기존 `TOD0~TOD6` 아래에 Composition 전용 `TCS` 작업을 둔다.

| 단계 | 구현 범위 | 완료 Gate |
| --- | --- | --- |
| `TCS0` Inventory | Farm 24개의 renderer·material·surface semantic inventory와 Day baseline | 24개 key·A/B/C 누락 없이 현재 화면 보존 |
| `TCS1` Shadow | 여섯 시각의 Sun·shadow curve와 Composition shadow budget | 그림자 방향 연속, socket·route 가림 없음 |
| `TCS2` Brightness | ambient·exposure·fog·value hierarchy curve | 여섯 시각 모두 silhouette·ground·road 판독 가능 |
| `TCS3` Surface | 8개 family의 surface binding·property block | 원본 material 변경 없이 24개 모두 반응 |
| `TCS4` Emissive | Farmhouse·Barn·stand·yard fixture, Town·City 후보 socket | 점등이 데이터 상태와 경쟁하지 않음 |
| `TCS5` Continuous | 시간 scrubber·Simulation clock 입력과 연속 보간 | 하루 경계·anchor 전환 pop 없음 |
| `TCS6` Expansion | Town·City 검증 subset, PC·Android tier | 같은 시간·Sun 공유, 성능 budget 통과 |

### 권장 첫 절단선

```text
TCS0 Farm 24 inventory
  → TCS1 여섯 시각 그림자
  → TCS2 밝기·명도
  → TCS3 표면 반응
  → Farm 24 contact sheet + 대표 Play Mode 전환
```

인공조명은 그림자·밝기·표면 반응이 통과한 뒤 붙인다. City·Town 전체 prefab 생성을 시간대 작업 때문에 앞당기지 않는다.

## 13. 검증과 시각 증거

24개×6시간대의 144개 화면을 개별 수작업으로 판단하지 않고 자동 capture와 contact sheet를 사용한다.

### 전체 검사

- 24개 Composition의 `Midday` contact sheet
- 24개 Composition의 `Night` contact sheet
- A/B/C 전체의 surface binding·missing renderer 자동 검사
- 그림자 caster·active light·material instance·property block 수집

### 대표 시간 흐름 검사

- 8개 경관군 A형×6시간대 = 48칸 contact sheet
- 감자밭·작업마당·직판장·농로의 고정 카메라 원본 PNG
- Farm Hero의 Dawn→Morning→Midday→Afternoon→GoldenDusk→Night Play Mode sequence
- 후속 Town 주택·City 공동주택/마트 대표 세트의 동일 시간 matrix

### 자동 테스트

- 24개 Composition key와 A/B/C profile family 완전성
- semantic surface kind 유효성·renderer 중복 binding 거부
- 시간 curve 정렬·하루 경계 연속성·NaN/범위 검사
- 원본 Synty material·texture dirty 여부
- Presenter의 Command·Simulation Tick·업무 Domain 참조 금지
- Mobile tier의 realtime light·shadow caster·material instance budget

정지 contact sheet는 시간대별 결과의 비교 증거이고, 전환의 부드러움은 Play Mode sequence와 frame timing으로 별도 검증한다.

## 14. 성능 원칙

- World Directional Light 1개를 모든 Composition이 공유한다.
- 24개 세트에 개별 Sun이나 shadow light를 두지 않는다.
- material clone 대신 shared material + `MaterialPropertyBlock`을 우선한다.
- time parameter는 global shader property와 surface family property를 조합한다.
- local light는 focus zone과 quality tier로 제한하며 Mobile은 emissive·fake light pool을 우선한다.
- renderer binding은 시작 시 cache하고 전체 계층 검색을 반복하지 않는다.
- 시간 curve는 낮은 주기로 평가하고 시각적으로 필요한 경우에만 매 frame 보간한다.

## 15. 중단 조건

- 24개 prefab을 시간대별로 복제함
- A/B/C마다 서로 다른 시간 색감을 임의 설정함
- Synty 원본 texture·material을 직접 수정함
- 시간대만으로 wetness·작물 생육·영업·배송 상태를 추론함
- GoldenDusk 그림자가 route·socket·marker를 가림
- Night에 ground·road·building이 같은 검정으로 뭉침
- texture swap 또는 material instance가 세트 수에 비례해 폭증함
- Composition마다 다른 Sun·shadow 방향을 가짐
- contact sheet·Game View 없이 Inspector 값만으로 완료 선언함
- Android Player 측정 없이 Mobile 완료를 선언함

## 16. 최종 수용 기준

1. Farm 24개가 동일한 시간 source를 소비하고 A/B/C 누락 없이 반응한다.
2. 그림자 → 밝기 → 표면 반응 → 인공조명 순서가 모든 시간 전환에서 유지된다.
3. 여섯 대표 시각 사이의 그림자 방향·명도·색감이 연속적으로 변한다.
4. Midday에는 원래 Synty local color와 데이터 상태색이 정확히 읽힌다.
5. Dawn·GoldenDusk·Night에도 connector·route·actor·socket·marker가 가려지지 않는다.
6. 원본 Synty prefab·material·texture는 수정되지 않는다.
7. 시간 Presentation은 업무·Simulation 권위를 소유하지 않는다.
8. Town·City subset이 Farm과 같은 World Sun·profile 계약으로 확장된다.
9. PC·Android에서 material·light·shadow·frame time 증거가 남는다.

## 17. 권장 다음 작업

제안 승인 후 `TCS0 Farm 24 Time Surface Inventory`부터 시작한다. 24개 prefab의 renderer를 semantic surface로 분류하고 현재 Midday 화면을 회귀 기준으로 고정한다. 그다음 `TCS1 Shadow`, `TCS2 Brightness`, `TCS3 Surface` 순으로 적용해 8개 경관군×6시간대 contact sheet와 Farm Hero Play Mode 전환을 먼저 확인한다. 이 기준이 통과한 뒤 Town·City 대표 Composition으로 확장한다.
