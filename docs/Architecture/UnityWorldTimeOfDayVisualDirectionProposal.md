# Unity World 시간대별 미술·색감 전환 제안

## 1. 제안 목적

현재 `FarmHeroShowcase`는 늦은 오전·이른 오후에 가까운 따뜻한 낮 화면을 기준으로 `ART0~ART4` 구도·색·조명·분위기·기초 움직임을 확보했다. 다음 시각 개선은 같은 Farm·Town·Hub·City가 시간의 흐름에 따라 서로 다른 정서를 가지면서도 하나의 World, 하나의 데이터 상태로 읽히게 만드는 것이다.

목표는 단순한 낮·밤 토글이나 태양 회전이 아니다.

> 시간의 흐름이 하늘·빛·그림자·공기·인공조명·생활 움직임을 함께 변화시키되, 업무 상태와 데이터 의미는 훼손하지 않는 Presentation 체계를 만든다.

이 제안은 기존 [Unity World Visual Art Direction·ART Pass 제안](UnityVisualArtDirectionAndPassProposal.md)의 ART3 Lighting, ART4 Atmosphere, ART5 Motion, ART6 Data Art, ART7 Camera·Performance를 시간축으로 연결하는 횡단 설계다.

## 2. 핵심 원칙

1. **기준 낮 화면을 먼저 보존한다.** 현재 Farm Hero 낮 화면을 회귀 기준으로 삼고 시간대 기능 때문에 기존 가독성이 나빠지지 않게 한다.
2. **시간은 Presentation 입력이다.** 태양·안개·노출·창문 불빛이 업무 상태를 결정하지 않는다.
3. **World 전체가 같은 시각을 공유한다.** Farm·Town·Hub·City의 태양 방향과 하늘은 하나이며 Region별 재질 반응과 인공조명만 다르게 설계한다.
4. **연속적으로 변한다.** 몇 개의 미술 앵커를 정의하되 실제 화면은 곡선 보간해 갑작스러운 LUT·조명 교체를 피한다.
5. **밤도 읽혀야 한다.** 현실적인 암흑보다 모바일 top-down에서 형태·길·actor·상태 marker가 읽히는 stylized night를 우선한다.
6. **데이터 색은 보호한다.** 상태색·선택색·경고색은 시간대 색보정에 묻히지 않고 모양·icon·문구와 함께 표현한다.
7. **모바일 비용을 먼저 제한한다.** 시간대 수만큼 실시간 Light를 늘리지 않고 하나의 Sun, 환경광, emissive·light proxy와 제한된 지역 조명을 사용한다.

## 3. 시간 권위와 실행 모드

시각 시간과 업무 시간은 같은 값으로 가정하지 않는다.

```text
Clock source
  → TimeOfDayPresentationSnapshot
  → TimeOfDayInterpreter
  → WorldTimeOfDayPresentationModel
  → Lighting / Atmosphere / Emissive / Motion Presenter
  → Synty World
```

### 3.1 허용할 시간 Source

| 모드 | 시간 출처 | 목적 | 운영 효과 |
| --- | --- | --- | --- |
| `FixedReference` | profile에 저장한 고정 시각 | 미술 제작·Before/After·회귀 테스트 | 없음 |
| `PreviewScrub` | Editor 또는 개발 UI slider | 전환 구간 미리보기 | 없음 |
| `SimulationClock` | Simulation session의 명시적 시각·tick | 가상 하루·save·replay | Simulation 내부만 |
| `OperationalObservation` | 서버가 제공한 기준 시각·timezone | 실제 시각을 읽기 전용으로 반영 | 업무 Command 없음 |

클라이언트의 `DateTime.Now`를 운영 상태나 배송·작업 상태의 권위로 사용하지 않는다. 운영 시각을 반영할 경우 서버 snapshot의 `ObservedAt`, timezone, revision과 freshness를 보존한다. 시간 연출 완료, 해가 뜸, 차량이 도착해 보임, 조명이 켜짐은 업무 완료나 Simulation Tick을 발생시키지 않는다.

## 4. 미술 시간대 앵커

첫 버전은 계절·날씨와 분리해 맑은 날의 네 앵커만 만든다. 숫자는 최종 물리값이 아니라 초기 Art Direction 범위다.

| 앵커 | 대표 시각 | 정서 | 빛·색의 핵심 | World 가독성 |
| --- | --- | --- | --- | --- |
| `Dawn` | 05:30~07:00 | 조용한 시작, 서늘함 | 청회색 ambient + 낮은 고도 복숭아색 Sun, 옅은 ground fog | silhouette와 길 가장자리를 먼저 보존 |
| `Day` | 10:00~14:30 | 깨끗함, 활동, 데이터 기준 | 중립에 가까운 따뜻한 Sun, 청록 하늘, 가장 정확한 local color | 현재 Farm Hero 기준 화면 |
| `GoldenHour` | 16:30~18:30 | 따뜻한 공동체, 하루의 결실 | 주황빛 low-angle Sun, 긴 그림자, 약간 낮춘 하늘 채도 | 얼굴·차량·길이 그림자에 잠기지 않게 fill 유지 |
| `Night` | 19:30~04:30 | 차분한 생활, 거점의 존재감 | 남청색 ambient, 낮은 moon key, 창문·가로등·Dock 작업등 | 완전한 검정 금지, landmark와 route 유지 |

### 4.1 Region별 반응

| Region | Dawn | Day | Golden Hour | Night |
| --- | --- | --- | --- | --- |
| Farm | 옅은 밭 안개, 차가운 잎색 | 작물·흙의 local color 기준 | 곡물·목재·흙의 황금빛 강조 | 농가 창문·Barn 작업등, 밭은 낮은 채도 유지 |
| Town | 굴뚝·주택 윤곽 | 따뜻한 주택색과 녹지 | 지붕·목재 facade의 온기 강화 | 창문 군집과 main street가 생활 중심 형성 |
| Hub | 금속·Dock의 차가운 시작 | 산업색과 cargo 구분 | 차량·cargo edge light | Dock·Gate·작업구역의 제한적 기능 조명 |
| City | 청회색 건물 silhouette | 도로·건물 대비 기준 | facade와 긴 도로 그림자 | 도로·마트·공동주택의 점진적 light hierarchy |

Region별 차이는 서로 다른 하늘을 쓰는 방식이 아니라 같은 빛에 대한 표면·인공조명의 반응 차이로 만든다.

## 5. 전환 시스템

### 5.1 앵커 보간

시간대를 네 개의 독립 Scene이나 Volume으로 만들지 않는다. 하루의 정규화된 값 `0..1`을 기반으로 인접 앵커를 보간한다.

```text
Normalized time
  → solar elevation / azimuth curve
  → anchor blend weights
  → lighting parameters
  → atmosphere parameters
  → local-light activation weights
  → motion/FX presentation weights
```

보간 대상:

- Sun rotation, color, intensity
- ambient sky·equator·ground color와 intensity
- sky gradient 또는 skybox exposure
- fog color·density·start/end distance
- exposure·contrast·saturation·white balance
- shadow strength·distance tier
- window·street·Dock emissive intensity
- 허용된 ambient particle과 생활 motion 밀도

Discrete 전환이 필요한 항목도 즉시 켜고 끄지 않고 activation window와 fade duration을 둔다. 예를 들어 가로등은 일몰 직후 0%에서 100%로 서서히 올라오며, 업무 상태와 무관한 Presentation 반응으로만 동작한다.

### 5.2 Color Grading 규칙

- 시간대마다 강한 LUT를 교체하지 않고 기본 local color 위에 white balance·contrast·saturation을 제한적으로 보간한다.
- `Day`는 상품·작물·상태색 검증의 기준이므로 가장 중립적으로 유지한다.
- `GoldenHour`는 shadow까지 주황색으로 덮지 않고 따뜻한 key와 서늘한 fill의 대비를 쓴다.
- `Night`는 전체를 파랗게 tint하지 않고 ground·vegetation·building의 명도 계층을 남긴다.
- Bloom은 달빛이 아니라 실제 emissive와 밝은 fixture에만 제한한다.
- exposure 변화로 screen-space UI와 상태 marker의 색·명도는 바뀌지 않게 별도 camera/UI 정책을 둔다.

## 6. 조명 구성

### 6.1 공통 World Light

- 실시간 Directional Light는 기본적으로 하나를 유지한다.
- 밤의 moon은 별도 두 번째 shadow light보다 Sun profile의 색·강도·방향 전환으로 우선 표현한다.
- ambient는 시간대 profile에서 통제하고 Scene별 임의값을 금지한다.
- shadow cascade와 distance는 PC·Mobile tier로 나눈다.

### 6.2 지역 인공조명

인공조명은 실제 Light보다 emissive material variant와 제한된 light proxy를 우선한다.

```text
FacilityVisualRoot
  ├─ Vendor model
  ├─ Emissive sockets
  └─ LocalLightProxyGroup
       ├─ PC: bounded realtime/mixed lights
       └─ Mobile: emissive + baked/fake pool
```

- Farm: 농가 창문, Barn 출입구, 작은 yard lamp
- Town: main street, 현관, 선택된 생활 landmark
- Hub: Dock 번호, Gate, 상하차 구역
- City: 마트 sign, 도로 교차점, 공동주택 일부 창문

모든 창문을 동시에 밝히지 않는다. 점등 pattern은 장식용 seed 또는 명시적 occupancy Presentation 입력으로 결정하고 실제 주민 수·영업 여부를 추론하지 않는다.

## 7. 데이터·UI 가독성 보호

시간대는 데이터 의미를 바꿀 수 없으므로 다음을 고정한다.

1. `Normal`, `Stale`, `Blocked`, `Selected` 등의 token은 시간대 profile과 분리한다.
2. 상태는 색만 쓰지 않고 icon·shape·label을 함께 사용한다.
3. World-space marker는 어두운 배경에서 outline·backplate를 자동 선택한다.
4. route trace는 낮·밤 각각 최소 명도 대비를 만족하는 variant를 사용하지만 같은 상태 의미를 유지한다.
5. detail card의 source·as-of·unit·revision·limitation은 시간 연출과 무관하게 동일하게 읽혀야 한다.
6. 야간 장식등이 warning·selection·route 색과 경쟁하면 장식등을 낮춘다.

## 8. Presentation 설정 구조

```text
WorldTimeOfDayProfile
  ProfileRevision
  TimezonePolicy
  AnchorProfiles[]
  TransitionCurves
  QualityTierOverrides

TimeOfDayAnchorProfile
  AnchorCode
  NormalizedTime
  Sun / Ambient / Sky
  Fog / Exposure / ColorAdjustments
  ShadowPolicy
  EmissivePolicy
  MotionFxPolicy

WorldTimeOfDayPresentationModel
  SourceMode
  SourceTimestamp
  Timezone
  NormalizedTime
  Previous / Next Anchor
  BlendWeight
  Freshness / Limitation
```

이 구조는 Presentation 전용이다. server Domain과 업무 contract에 Sun color·fog density·Volume profile key를 넣지 않는다. Synty 원본 prefab·material도 수정하지 않고 project-owned profile, material variant, `MaterialPropertyBlock`, wrapper socket을 사용한다.

## 9. 단계적 구현 제안

기존 ART 번호를 다시 정의하지 않고 `TOD` 트랙을 추가한다.

| 단계 | 구현 범위 | 완료 Gate |
| --- | --- | --- |
| `TOD0` 기준선·계약 | 현재 Farm Hero `Day` 고정 profile, 시간 source mode와 Presentation model, 동일 카메라 기준 캡처 | 기능 off와 `Day` profile 화면이 사실상 동일 |
| `TOD1` 네 앵커 | Dawn·Day·GoldenHour·Night profile과 Editor preview menu | 같은 Farm 카메라의 4장 PNG에서 정서가 다르고 공간은 동일하게 읽힘 |
| `TOD2` 연속 전환 | 보간 curve, scrubber, 개발용 가속 하루 | 전환 중 pop·노출 급변·shadow jump 없음 |
| `TOD3` 인공조명 | Farmhouse·Barn·road·Dock의 emissive/light proxy | 밤 landmark·route가 읽히고 장식등이 상태색을 침범하지 않음 |
| `TOD4` 생활감 연결 | 시간대별 제한된 연기·작물 motion·차량/actor 빈도 표현 | canonical 업무 상태 변화 없이 생활감만 변화 |
| `TOD5` 데이터 미술 | normal·selected·stale·blocked·route marker의 시간대 variant | 네 시간대 모두 UI off/on과 상태 구분 통과 |
| `TOD6` 카메라·성능 | World/Farm/Town/Hub/City 카메라, PC·Android tier·profiling | 대표 PNG, Play Mode, Windows·Android 수치와 회귀 테스트 기록 |

### 첫 구현 절단선

우선 `TOD0~TOD2`만 Farm Hero에 적용하는 것이 적절하다.

```text
현재 Farm Hero Day
  → Day 회귀 profile 고정
  → Dawn / GoldenHour / Night 3개 anchor 추가
  → Editor scrubber로 연속 보간
  → 같은 카메라 4장 + 전환 runtime 검증
```

Town·Hub·City의 인공조명과 시간대별 움직임은 이 기준이 통과한 뒤 `TOD3~TOD4`에서 확장한다. 날씨·계절·실시간 천체 계산은 첫 절단선에 포함하지 않는다.

## 10. 검증 계획

### 10.1 자동 검증

- anchor code·normalized time 중복과 정렬 검사
- 하루 경계 `23:59 → 00:00` 연속성
- 모든 profile 값의 허용 범위와 NaN 검사
- `FixedReference`, `PreviewScrub`, `SimulationClock`, `OperationalObservation` source 구분
- operational timestamp 결측·오래됨·timezone 불명확 상태의 명시적 제한
- 시간 Presenter가 Command·Simulation Tick·업무 Domain type을 참조하지 않는지 검사
- Synty 원본 prefab·material이 dirty하지 않은지 검사
- PC·Mobile quality tier에서 활성 Light·shadow·particle budget 검사

### 10.2 시각 검증

동일 Scene state와 동일 camera transform에서 다음을 남긴다.

- Farm의 Dawn·Day·GoldenHour·Night 4장
- 후속 단계의 World·Town·Hub·City 4시간대 matrix
- Color와 grayscale/value 비교
- UI off/on, normal·selected·stale·blocked 비교
- Post-processing off/on 비교
- 전환 구간을 확인할 짧은 runtime sequence
- PC와 Android built-player 비교

정지 PNG는 시간대별 최종 구도·색·가독성 증거이며, 전환의 부드러움은 Play Mode sequence와 frame timing으로 별도 확인한다.

## 11. 성능 예산 방향

- Directional shadow light: 기본 1개
- local realtime shadow light: Mobile 기본 0, PC도 focus zone에 제한
- 창문·sign: emissive variant와 batching 우선
- Night particle: focus와 distance tier로 제한
- Volume component: 시간대마다 여러 Volume을 중첩하기보다 단일 runtime profile 값을 갱신
- material: 인스턴스 대량 생성 대신 shared variant와 `MaterialPropertyBlock` 우선
- 시간 갱신: 매 frame 전체 renderer를 순회하지 않고 일정 주기 profile 평가 + 변경된 group만 적용

정확한 수치는 구현 뒤 Windows Player와 Android Player에서 renderer·draw call·shadow caster·transparent·particle·memory·frame time을 측정해 확정한다.

## 12. 중단 조건

다음 조건에서는 다음 TOD 단계로 넘어가지 않는다.

- Night가 단순한 파란 overlay 또는 검은 화면이 됨
- Golden Hour의 긴 그림자가 actor·route·데이터 marker를 가림
- 시간 전환 때 sky·fog·exposure가 튀거나 material이 깜박임
- 지역마다 태양 방향이나 그림자 방향이 다름
- 가로등·창문 수가 실제 주민·영업·업무 상태로 오인됨
- 시간 연출이 업무 Command, 배차, 배송 완료, Simulation Tick을 발생시킴
- 상태색을 LUT 하나로 보정해 의미가 달라짐
- Synty 원본 material·prefab을 직접 수정함
- Editor Game View만 보고 모바일 완료를 선언함

## 13. 최종 수용 기준

1. 같은 World가 네 시간대에 서로 다른 정서를 가지면서 지형·landmark·route 관계는 동일하게 읽힌다.
2. Dawn·Day·GoldenHour·Night 사이가 시각적으로 연속적이며 하루 경계도 튀지 않는다.
3. Farm·Town·Hub·City가 하나의 Sun·Sky·시간을 공유한다.
4. Night에도 actor·vehicle·시설·길과 데이터 marker가 구분된다.
5. 시간대에 관계없이 source·revision·mode·limitation과 상태 token이 정확히 보존된다.
6. 시간·조명·FX Presenter는 운영 업무와 Simulation 결과의 권위를 갖지 않는다.
7. PC와 Android에서 품질 tier별 시각·성능 증거가 남는다.
8. 현재 `Day` Farm Hero 화면이 회귀 기준으로 유지된다.

## 14. 권장 다음 작업

제안 승인 후 첫 구현은 `TOD0~TOD2 Farm Hero Time-of-Day Prototype`으로 제한한다. 현재 Farm Hero Scene에 Day 회귀 profile, 네 앵커와 연속 scrubber를 추가하고 동일 카메라의 4시간대 PNG 및 Play Mode 전환 증거를 만든다. 이 결과가 통과한 뒤에만 Town·Hub·City 인공조명과 시간대별 생활 움직임으로 확장한다.
