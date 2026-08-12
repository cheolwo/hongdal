# Unity Farm·Town·City Composition 통합 구현 순서

## 1. 목적과 상태

이 문서는 지금까지 조사·설계만 하고 구현하지 않은 Farm·Town·City Composition, 농식품 상품·가격 카드와 혼합 경관 작업을 하나의 구현 순서로 통합한다.

- 기준일: 2026-08-10
- 상태: CP0·CMP1·CMP2·CMP3·CMP4·CMP4-A·CMP5·ANIM0·ANIM1 구현 완료, ANIM2 fallback·ANIM4 차량 이동 기준선 완료, CMP6 감자 가격 Card 대기
- CP0에서는 기존 저장 Scene과 asset을 재사용했고 CMP1에서는 additive 공통 계약·adapter·validator만 추가했다. 새 Town·City·Hub prefab은 아직 생성하지 않았다.
- 실제 구현 대상 Unity project는 `C:\Users\user\ssalddel`이다.
- 기존 운영 서버·Simulation 서버·Unity Presentation의 권위 경계를 변경하지 않는다.

이 문서는 개별 조사 문서의 세트 목록과 세부 규칙을 복제하지 않는다. 무엇을 먼저 만들고 어떤 Gate를 통과한 뒤 다음 단계로 이동할지를 정하는 실행 순서의 단일 기준이다.

## 2. 먼저 구분할 현재 상태

### 2.1 이미 구현되어 재사용할 기반

| 기반 | 현재 상태 | 후속 작업에서의 사용 |
| --- | --- | --- |
| WORLD-0~WORLD-5 | Camera·Macro World·Synty catalog·업무 View·Cargo Journey·품질 Gate 구현 완료 | 새 Composition을 검증할 camera·Zone·cargo 기반으로 재사용 |
| Farm 6×6 감자밭 | stable ID 선택과 FARM-2 Preview→Confirm→Simulation Tick→reconcile 완료 | 실제 상품·가격 anchor와 농업단지 내부 Simulation 구역으로 재사용 |
| 농장 풍경 Composition Library | 8종×A/B/C, 24 prefab·catalog·preview 구현 완료 | 새 공통 계층의 회귀 기준이며 다시 생성하지 않음 |
| `WorldVisualCatalog`·`WorldVisualInstanceView` | Farm·Urban·Transition VisualKey와 vendor-neutral wrapper 구현 완료 | 원본 Synty prefab과 업무 View를 분리하는 기존 경계로 유지 |
| Cargo Journey | Farm Yard→Transport→Logistics→Market의 cargo stable ID·lineage 표현 구현 완료 | 혼합 District에서 같은 cargo가 이어지는지 검증할 기준 |
| Concept Card 공통 문법 | Concept·Status·Reason·Action Presentation 계약 구현 기반 존재 | 상품·가격 Deck의 View 문법으로 재사용 |
| 공통 Composition 계약 | pack/source, footprint·cell, root, detail tier, connector·socket, journey kind와 A/B/C signature validator 구현 | 기존 Farm adapter와 Town·City·Hub 신규 module의 공통 검사 기준 |
| Synty animation inventory | clip/controller 0, Humanoid rig 5, Town missing controller 8, ParticleSystem Farm 11·City 2·Generic 17을 Editor validator로 검출 | ANIM1 catalog와 후속 retarget source 선택 기준 |
| 세 Pack source 실측 | Town House 12개와 Farm·Town·City 도로·보도·밭·온실·공동주택 등 42개를 같은 Editor 검사 코드로 측정 | CMP3 builder가 사용하는 5m grid·pivot·bounds·Farm adapter 기준 |

따라서 새 작업은 WORLD 기반이나 기존 농장 풍경 24개를 다시 만드는 작업이 아니다. 기존 직렬화 asset과 Scene을 깨지 않는 additive 확장이어야 한다.

`FarmCityGraphicalShowcase`는 2026-08-10 저장 Scene 재로드·검증 메뉴·대표 캡처와 profiling을 다시 확인했고 전용 4/4, 전체 EditMode 64/64 기준선을 통과했다. CMP1 추가 뒤 열린 Editor 전체 EditMode는 72/72 통과했다.

### 2.2 아직 문서에만 있는 구현 대상

| 구현 대상 | 문서상 후보 규모 | 상태와 주의점 |
| --- | ---: | --- |
| Town 도로·생활권 | 도로 6종+생활권 12종×A/B/C = 최대 54개 | 실제 World에 필요한 subset부터 구현 |
| City 풍경 | 첫 12종×A/B/C = 36개, 후속 6종×A/B/C = 18개 | Zone 경관 계층이며 아래 도로·주거 module과 중복 계산 금지 |
| City 주거도로·공동주택 | 도로 6종+주거 block 8종×A/B/C = 최대 42개 | City 풍경을 조립하는 하위 module |
| Farm 농업단지 | 농로 6종·시설하우스 6종·밭 8종×A/B/C = 최대 60개 | 기존 농장 풍경 24개의 하위·인접 module로 추가 |
| 논 단지 | Blockout 6종 후보 | Rice·담수면·논둑·농수로 전용 asset 전에는 완성 세트로 승격하지 않음 |
| Farm↔Town·Town↔City 혼합 | 경계 12종×A/B/C = 최대 36개, 관통 District Recipe 4개 | 거대한 단일 prefab이 아니라 경계 세트와 route의 조합 |
| 농식품 상품·가격 | 식별 품목군 29개 | 직접 10개·대표가격 2개·추가 판정 17개를 구분 |
| 상품·가격 카드 | FPC0~FPC3 | 감자 한 품목의 수직 슬라이스 뒤 확장 |
| Synty animation·FX | Humanoid rig와 Farm 11·City 2·Generic 17 FX는 확인, Synty clip·controller는 확인되지 않음 | 실제 source를 검증하고 공용 intent·리타기팅 adapter부터 구현 |
| Town House interior | 선택적 Object Focus | 첫 World 연결과 성능 Gate 뒤 한두 채만 검증 |

위 숫자는 구현 quota가 아니라 후보 상한이다. A형 최소 세트와 실제 District를 먼저 검증하지 않고 모든 후보 prefab을 일괄 생성하지 않는다.

## 3. 최종 조립 계층

경관 구현은 다음 계층을 건너뛰지 않는다.

```text
Synty 원본 prefab
  → Source Catalog와 측정값
  → 도로·밭·주택 같은 Module Set
  → Farm·Town·City Zone Composition Set
  → Farm↔Town·Town↔City Boundary Set
  → Farm·Town·City 독립 Region
  → Regional Logistics Hub
  → Region·Hub Gate와 Passenger·Freight Route
  → 세 Pack 관통 District·Journey Recipe
  → 기존 World Scene에 선택 배치
```

상품 정보는 별도 계보를 사용한다.

```text
선택 가능한 World Object
  → WorldObjectRef
  → ProductStableId
  → 검토된 HS·가격 mapping
  → 출처·시각·단위·통화·시장단계가 있는 PriceObservation
  → Concept Card Deck
```

두 흐름은 `ProductStableId`가 있는 명시적 상태 socket에서만 만난다. Synty prefab 이름이나 모양으로 상품·HS·가격·재고를 만들지 않는다.

사람·차량·설비 animation은 세 번째 Presentation 계보를 사용한다.

```text
Canonical 또는 Simulation state
  → AnimationPresentationModel
  → AnimationIntent
  → AnimationKey / FxKey
  → Synty·Retargeted·Procedural·Fallback adapter
  → VisualRoot의 Animator·차량·설비·ParticleSystem
```

현재 import에는 Synty 제공 standalone·embedded clip이 없으므로 Synty Humanoid rig를 우선 재사용하고 검증된 clip을 리타기팅한다. 실제 source 판정과 세부 Gate는 [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다.

## 4. 구현 공통 원칙

1. 원본 Synty prefab·material·shader를 직접 수정하지 않는다.
2. 기존 `농장풍경CompositionCatalog`·24개 prefab과 저장 Scene의 직렬화를 깨는 이름 변경을 하지 않는다.
3. 공통 footprint·connector·detail tier 계약은 additive type으로 추가하고 기존 Farm catalog는 adapter 또는 기존 View를 통해 호환한다.
4. 세트 prefab은 환경 외형과 상태 socket만 소유한다. stable ID, revision, 권한, 수량, 가격과 업무 상태는 연결되는 View가 소유한다.
5. Builder는 같은 입력에서 같은 key·prefab·catalog·preview를 재현하고 두 번 실행해도 중복 asset을 만들지 않아야 한다.
6. A/B/C는 외형 밀도만 다르고 footprint·connector signature와 핵심 socket 위치는 같아야 한다.
7. Library Preview는 module 검사 증거이고 최종 World Game View와 구분한다.
8. visible building·NPC·작물 수를 실제 세대·주민·수확량·재고로 해석하지 않는다.
9. NPC 도착·차량 이동·animation·FX는 Command 성공이나 업무 완료를 발생시키지 않는다.
10. PC와 Android 실측 전 renderer·shadow·LOD·overdraw 예산을 임의 숫자로 확정하지 않는다.
11. Synty가 실제 제공하지 않은 clip·controller를 `SyntyProvided`로 표시하지 않고, 리타기팅·절차형·fallback source를 catalog에서 구분한다.

## 5. 구현 우선순위 등급

문서 후보 수가 아니라 **다음 단계가 의존하는 정도**, **기존 구현 회귀 위험**, **사용자에게 보이는 관통 가치**, **업무 권위 오류 위험** 순으로 우선순위를 정한다.

| 등급 | 먼저 구현할 것 | 해당 Gate | 뒤로 미루는 것 | 이유 |
| --- | --- | --- | --- | --- |
| 최우선 CP0 | 기존 Showcase 검증 복구 | CMP0 | 모든 새 prefab | 기준선이 깨진 상태에서는 새 오류와 기존 오류를 구분할 수 없음 |
| 기반 CP1 | 공통 계약·실측·Source Catalog·도로/Gate A형·animation inventory | CMP1~CMP3, ANIM0~ANIM1 | B/C 변형, interior, 품목 확대 | 이후 모든 Region·Journey가 공유하는 골격이며 재작업 비용이 가장 큼 |
| 관통 CP2 | Pack·Hub 최소 A형, 공용 Humanoid 이동, 3 Region·Hub Journey | CMP4~CMP5, ANIM2·ANIM4 | 생활소품 밀도와 다수 작업 animation | 가장 작은 범위로 실제 World 구조와 사람/화물 이동을 검증 |
| 업무 CP3 | 감자 상품·가격 카드 한 품목, FARM-3 농부 작업 한 종류 | CMP6, FARM-3·ANIM3 | 29개 품목과 농작업 동작 일괄 연결 | 기존 6×6·cargo lineage·가격 근거와 확정 Task를 각각 끝까지 잇는 첫 정보·작업 수직 슬라이스 |
| 확장 CP4 | 사용처가 확인된 Zone subset과 A/B/C, 직접 연결 품목 | CMP7~CMP9, ANIM5 | 미사용 세트, 보류 품목 | 관통 화면에서 필요한 조합만 늘려 자산 폭증을 방지 |
| 후순위 CP5 | 논 완성, 선택적 Town Interior, 고밀도 detail, 최종 최적화 | CMP10~CMP11, ANIM6 | 근거 없는 장식 확장 | 전용 asset·성능·상태 socket이 안정된 뒤 품질을 올려야 함 |

`CP0~CP5`는 [Unity World 구현 현황과 우선순위](UnityWorldImplementationPriority.md)의 전체 P0~P9와 혼동하지 않기 위한 **P7.6 Composition Track 내부 등급**이다.

따라서 첫 구현 목표는 “Farm·Town·City 세트를 많이 만드는 것”이 아니라 `CP0→CP1→CP2`를 닫아 **감자 cargo 한 건과 대표 사람 한 명이 올바른 Gate를 통해 이동하는 최소 World**를 만드는 것이다. `CP3` 가격 카드는 이 공간 anchor가 고정된 직후 붙인다.

## 6. 통합 구현 순서

### CMP0 — 현재 기준선 닫기

먼저 기존 `FarmCityGraphicalShowcase`의 미완료 검증을 끝낸다.

- Unity compile과 Test Runner 고착 원인을 확인한다.
- 기존 전체 EditMode 회귀를 통과시킨다.
- Showcase Overview·Farm·Logistics·Market을 다시 캡처한다.
- 현재 renderer·shadow caster·transparent renderer·Console 기준을 기록한다.
- Scene과 vendor asset이 dirty하지 않은지 확인한다.

완료 Gate: 새 Composition을 추가하기 전의 테스트·화면·성능 기준선이 하나로 고정된다. 이 단계가 실패하면 새 prefab 생성으로 넘어가지 않는다.

구현 결과(2026-08-10): 저장 Scene과 24개 Farm prefab이 유지됨을 확인하고 Showcase 전용 4/4·기존 전체 64/64를 통과했다. World·Farm·Logistics·Market 캡처와 Environment 기준 `351 instances / 370 renderers / 370 shadow casters / 7 transparent renderers / Animator 0 / ParticleSystem 0`, Console Error 0, Scene dirty false를 기록했다. CLI 전체 test는 결과 XML 저장 뒤 Unity 6.5 InputSystem/TextCore native crash가 한 번 발생했으나, 다시 연 Editor Pipeline에서 전체 test가 정상 종료됐다.

### CMP1 — 공통 Composition 계약을 additive로 확장

기존 Farm 전용 구현을 삭제하거나 대규모 rename하지 않고 Town·City·혼합 Set이 공유할 최소 계약을 추가한다.

- footprint와 cell size
- 북·동·남·서 차량·보행·농기계 connector
- 출입구와 route anchor
- `EnvironmentRoot`, `OcclusionRoot`, `InteriorRoot`
- 상태 socket code와 socket category
- World·Zone·Object Focus detail tier
- source prefab reference와 pack/source kind
- A/B/C variant code와 동일 signature 검증
- Region footprint·expansion socket
- Region·Hub Gate·차량/보행 connector·Route signature
- Stateful Journey와 ambient traffic 구분

Pack별 builder·catalog의 업무 이름은 분리한다.

```text
기존 농장풍경 Composition       유지
농업단지 Composition            새 하위 module
소도시풍경 Composition          새 Town 계층
도시주거도로 Composition        새 City 하위 module
도시풍경 Composition            새 City Zone 계층
혼합풍경 Composition            새 boundary 계층
```

완료 Gate: 기존 Farm 24개 test가 그대로 통과하고, 공통 validator가 중복 key·잘못된 connector·A/B/C signature 불일치를 거부한다.

구현 결과(2026-08-10): `월드CompositionDescriptor`, connector·socket·pack/source/detail/journey code와 validator를 추가하고 기존 `농장풍경CompositionCatalog`를 수정하지 않는 adapter를 연결했다. 공통 계약 집중 6/6, animation inventory 2/2와 전체 EditMode 72/72가 통과했다. 다음 Gate는 CMP2의 실제 bounds·pivot·door/긴 축·connector 측정값 고정이다.

### CMP2 — 세 Pack 실측과 Source Catalog 고정

문서의 논리 footprint를 곧바로 prefab 좌표로 사용하지 않는다. Editor 측정 도구 또는 재현 가능한 검사 코드로 실제 값을 먼저 기록한다.

- Town House preset 12개와 Road·Sidewalk·Driveway의 bounds·pivot·door 방향
- City Road·Sidewalk·Apartment module의 bounds·pivot·entrance 방향
- Farm Dirt·Dirt Row·Dirt Road·Greenhouse의 bounds·pivot·긴 축 방향
- base·overlay·complete building·modular part·accent 분류
- Town↔City 5m connector와 Farm↔Town adapter offset
- Farm·Town·City Region과 Regional Logistics Hub footprint, 안쪽 Gate 방향과 바깥쪽 확장 방향
- 공통 shader, scale, collider, NavMesh 후보와 LODGroup 부재
- `.anim`·`.controller`·embedded clip·Avatar·Animator·ParticleSystem inventory와 missing controller reference

측정 결과는 builder 내부의 흩어진 상수가 아니라 source catalog 또는 명시적 measurement report에 둔다.

완료 Gate: 5m Town·City grid가 실제 값으로 확인되고, Farm 접속 오차와 adapter offset이 기록된다. 확인하지 못한 asset에는 임의 footprint를 확정하지 않는다.

구현 결과(2026-08-10): `SyntyCompositionSourceMeasurementCatalog`에 세 Pack 42개 source와 `base / overlay / complete-building / modular-part / accent` 역할을 고정하고, Editor 검사기가 root-local bounds·pivot offset·긴 축·문 방향·shader·scale·collider·NavMesh 후보·LODGroup을 재현 가능하게 측정하도록 했다. Town·City 도로/보도 후보는 5m grid 오차 허용치 0.05m 안에서 모두 일치했다. Farm Dirt Road 직선은 긴 축 11.9106m로 측정되어 10m Town grid 접속 기준 오차 1.9106m, 중심 기준 adapter offset `(0, 0, -0.9553)`을 명시했다. Town House preset 12개는 결합 mesh에 문 이름이 보존되지 않아 방향을 임의 확정하지 않고 `unknown`으로 남겼다. 측정 집중 4/4와 열린 Editor 전체 EditMode 76/76이 통과했고, report는 `C:\Users\user\ssalddel\artifacts\CMP2\SyntyCompositionSourceMeasurements.json`에 생성된다.

### CMP3 — 도로 Connector Backbone A형

주택·상가·밭보다 먼저 길을 만든다. 모든 이후 세트가 route와 camera 공간을 공유하기 때문이다.

1. Town 직선·모서리·T자·십자 도로 A형
2. City 직선·모서리·T자·십자 도로 A형
3. Farm 농로 직선·모서리·T자·십자 A형
4. Farm↔Town 농촌 생활도로와 두 Region Gate A형
5. Town↔City 사람·통근도로와 두 Region Gate A형
6. Farm→Hub 농산물 집하노선과 Gate A형
7. Town→Hub 지역 집배송노선과 Gate A형
8. Hub→City 배송노선과 Gate A형

각 A형에서 90도 회전, connector graph, lane·row 방향, sidewalk 연결, overlap과 z-fighting을 검증한다. 차량·보행·농기계 semantic route가 mesh 접촉과 별도로 이어져야 한다.

완료 Gate: Farm↔Town·Town↔City 사람 route와 Farm/Town→Hub→City 화물 route의 각 leg가 유효한 Gate 쌍을 가지며, dangling connector는 명시적 World 경계나 막다른길에만 존재한다.

구현 결과(2026-08-10): `도로GateCompositionCatalog`와 idempotent builder를 추가해 Farm·Town·City의 직선·모서리·T자·십자 도로 12개, Farm↔Town·Town↔City 사람·차량 Gate 4개, Farm→Hub·Town→Hub·Hub→City 화물 Gate 6개 등 A형 prefab 22개를 생성했다. 도로 connector는 `vehicle / pedestrian / farm-machine` kind와 pack별 route signature를 분리하고, Gate 외부 connector는 `boundary.*` 사람·차량 4쌍과 `freight.*` 화물차 3쌍으로 북·남 방향이 정확히 대응한다. Builder를 연속 두 번 실행해 prefab 수 `22 → 22`를 확인했으며, Town 십자도로 tile 수평 면적 중첩 없음과 90도 회전 시 connector 동반 회전을 검사했다. 집중 EditMode 6/6·전체 82/82, Console Error 0, Preview Scene dirty false와 Game View를 확인했다.

### CMP4 — Pack별 최소 생활·생산 Module A형

도로가 검증된 뒤 각 Pack의 정체성을 만드는 최소 module만 추가한다.

#### Farm 최소 A형

- 밭고랑 단일필지
- 실제 감자 6×6 포함 필지
- 밭머리 작업띠
- 단동 시설하우스
- 대형 시설하우스
- 시설하우스 출하마당

#### Town 최소 A형

- 기본주택
- 차고주택
- 정원주택
- 동네상점 전면
- 생활택배 거점
- 근린 놀이터

#### City 최소 A형

- 공동주택 가로형 블록
- 공동주택 모서리 블록
- 공동주택 생활마당
- 공동수령 포켓
- 도심마트 앞마당

#### Regional Logistics Hub 최소 A형

- Farm inbound 진입부
- Town 집배송 inbound 진입부
- 입고 Dock
- 검수마당
- 보관블록
- 출고대기장·City outbound 진입부

완료 Gate: 각 Pack의 Preview에서 source가 nested reference이고, 환경 외형에 stable ID·권한·수량·가격이 없으며, 출입구·차량 회차·camera occlusion이 검증된다.

구현 결과(2026-08-10): 다음 관통 단계가 직접 사용하는 A형 4종만 먼저 생성했다. `실제 감자 6×6 필지`는 36개 Dirt Row 환경 prefab과 단 하나의 `farm.socket.potato-field` simulation-target을 가지며 기존 `FarmSoilTileGridView`를 복제하지 않는다. `타운 기본주택`은 결합 mesh 때문에 source 문 방향을 `unknown`으로 보존하고 남측 설계 출입구를 별도로 두었다. `시티 공동주택 가로형`은 source 동측 출입구를 90도 회전해 남측 생활도로에 연결한다. `지역 물류허브 Dock`은 차량 Gate·입고·검수·보관·출고대기·cargo handoff socket을 분리한다. 네 prefab은 CMP3의 Farm/Town/City 직선도로 또는 Hub Gate와 반대 방향·동일 route signature로 연결되며, 차량 회전반경과 Town·City·Hub camera occlusion root를 명시한다. Builder 반복 실행 결과는 `4 → 4 → 4`, 집중 EditMode 6/6·전체 88/88이며 Preview Game View를 저장했다. 환경 prefab에는 Controller·UseCase·Repository·Simulation 권위를 넣지 않았다.

### CMP4-A — 공용 animation adapter 최소 기반

Journey를 최종 조립하기 전에 animation을 대량 제작하지 않고, 실제 actor socket을 이용한 최소 공용 기반만 만든다.

1. `AnimationIntent`, `AnimationKey`, source kind와 fallback을 asset-neutral 계약으로 추가한다.
2. Town의 해소되지 않은 controller GUID를 catalog 검사에서 오류로 검출한다.
3. 검증된 in-place Idle/Walk clip 한 세트를 Farm·Town·City Synty Humanoid에 리타기팅한다.
4. 사람 위치는 NavMesh/route follower가 소유하고 root motion은 기본적으로 끈다.
5. 차량은 route 이동과 바퀴·조향 같은 절차형 표현부터 시작한다.
6. clip을 확보하지 못해도 route·state 검증은 fallback으로 계속하되 CMP11 시각 완료로 승격하지 않는다.

완료 Gate: 세 Region의 대표 actor 1명씩이 같은 intent 계약으로 이동하고, source 누락·Avatar 불일치 시 fallback과 진단이 작동하며 animation event가 업무 Command나 Simulation Tick을 발생시키지 않는다.

구현 결과(2026-08-10): asset-neutral `공용AnimationKey`, `Idle / Walk` intent, `SyntyProvided / Retargeted / ProceduralFallback` source kind와 세 Pack catalog를 추가했다. Farm 농부·Town 주민·City 주민 대표 actor 각 1명은 동일한 `locomotion.idle.v1 / locomotion.walk.v1` 계약을 사용한다. `공용ActorRouteFollower`만 위치·회전·대기 전이를 소유하고 `공용AnimationAdapter`는 root motion을 끈 채 intent 표현만 담당한다. 실제 Synty character clip과 controller가 0이고 Town character 8종의 controller GUID가 해소되지 않는 상태를 숨기지 않으며, catalog는 세 Pack 모두 `humanoid.procedural-locomotion.v1` fallback과 `animation.clip-unavailable` 진단을 기록한다. 따라서 ANIM1 계약과 ANIM2의 route/fallback 기준선은 완료했지만 검증된 clip 리타기팅 완료나 CMP11 시각 완료로 승격하지 않는다. 집중 EditMode 6/6·전체 94/94, Console Error 0, 저장 Preview Scene과 Play Mode Game View를 확인했다.

### CMP5 — 첫 3개 Region·물류허브 Map·Journey 수직 슬라이스

모든 library를 확장하기 전에 A형만으로 세 Region과 지역 간 이동망을 만든다.

```text
 [Farm Region] ── 감자 집하 ──┐
       │                       ▼
       │ 생활도로      [Regional Logistics Hub] ── City 배송 ─→ [City Region]
       │                       ▲                                  ▲
 [Town Region] ── 집배송 ──────┘──── 사람·통근도로 ────────────────┘
```

이 단계에서는 기존 WORLD-4 Cargo Journey의 cargo stable ID와 lineage를 `Farm→Hub` inbound leg에 재사용한다. Hub의 입고·검수·보관 Projection과 명시적 outbound allocation 뒤에만 `Hub→City` Journey를 만든다. Town은 새 canonical Zone이 아니라 독립 Presentation Region이며 저밀도 Residential·Community·지역 배송 의미를 조합한다.

첫 slice는 다음을 연결한다.

- Farm: 실제 감자 6×6·Farm Yard·Produce Stand
- Town: 기본주택·동네상점·생활택배 거점
- Hub: inbound Gate·Dock·검수·보관·outbound staging
- City: 도심 분배 Gate·도심마트·공동주택 공동수령 포켓
- Farm↔Town: 농장마을 진입부와 Pickup 또는 주민 ambient loop
- Town↔City: 저층주거 도시전환과 대표/주민 Stateful Journey
- Farm→Hub: 기존 감자 Cargo inbound Journey
- Town→Hub: 지역 집배송 route와 별도 sample cargo 또는 environment delivery
- Hub→City: accepted·allocated cargo만 별도 outbound Journey

완료 Gate: 세 Region이 각각 독립 영역으로 읽히고 여러 origin 화물이 중간 Hub에 입고된 뒤 명시적 출고계획으로 City에 재출하되, 사람 이동은 freight yard와 분리되고 외형 이동만으로 입고·검수·판매·수령 완료가 발생하지 않는다.

구현 결과(2026-08-10): `ThreeRegionHubJourney` 저장 Scene에 Farm 실제 감자 6×6, Town 기본주택, City 공동주택 가로형, Regional Logistics Hub Dock A형을 독립 anchor로 배치하고 CMP3 Gate prefab 10개를 passenger·freight 경계에 사용했다. Farm↔Town과 Town↔City 사람 Journey 2개는 Hub freight yard 남쪽 corridor와 공용 Idle/Walk fallback을 사용한다. 화물은 Farm과 Town 두 origin이 Hub에 모인다. Farm 화물은 WORLD-4의 `cargo:transport-71`, `product:potato`와 6개 lineage를 그대로 보존하고 `hub-stored`에서 멈춘다. Town sample 화물만 `outbound-allocation:town-delivery-01.city-01` source가 있을 때 `city-outbound` 차량 follower가 활성화된다. Allocation 없는 outbound model은 거부되며 차량 이동을 수동 tick해도 stage와 lineage는 변하지 않는다. 집중 EditMode 7/7·전체 101/101과 Play Mode Overview를 확인했다. 이 Scene은 A형 관통 구조 기준선이므로 최종 경관 밀도·연속 도로·카메라 품질 완료로 간주하지 않는다.

### CMP6 — 감자 상품·가격 카드 수직 슬라이스

공간 anchor가 고정된 뒤 상품·가격을 연결한다. 첫 품목은 이미 실제 6×6과 cargo lineage가 있는 감자 하나로 제한한다.

1. FPC0: 재배체·수확물·상자·Produce Stand wrapper에 `WorldObjectRef`와 `ProductStableId` binding
2. FPC1: Unity API model·mapper·repository와 `Success`·`MappingRequired`·`DataUnavailable`·stale last-success 구분
3. FPC2: 상품·국내가격·HS 연결 근거·국가별 가격을 공통 Concept Card Deck으로 투영
4. FPC3: PC click·Mobile tap, highlight, screen-space Deck, 상세·닫기와 authorization scope 변경 검증

가격 카드에는 출처, 기준 시각, 단위, 통화, 시장 단계, 직접/대표 mapping과 `InformationOnly`를 표시한다. 국가별 가격은 검토된 HS6가 있을 때만 조회한다.

완료 Gate: 감자의 네 anchor가 같은 상품을 가리키되 수량과 상태를 prefab에서 계산하지 않고, 일부 자료·비교 불가·갱신 실패도 정상적인 Card 상태로 표현한다.

### CMP7 — 필요한 Zone Composition만 확장

관통 slice에서 실제로 필요한 조합이 확인된 뒤 Pack별 상위 세트를 추가한다.

1. Town: 중심가·농촌주거 접경·생활상권·배송에 사용된 생활권 subset
2. City: 첫 12종 중 물류센터·마트·공동수령·주거·공원에 실제 필요한 subset
3. Farm: 시설하우스·밭 단지 recipe에 필요한 subset
4. Hub: 입고·검수·보관·분류·출고에 실제 필요한 subset
5. 혼합: Farm↔Town·Town↔City Gate와 Farm/Town→Hub·Hub→City 진입부 subset

완료 Gate: 세트 수를 늘리기 전 각 세트가 어느 District Recipe와 Game View에 사용되는지 추적할 수 있다. 사용처 없는 후보는 생성하지 않는다.

### CMP8 — A/B/C와 District Recipe 확장

A형과 최종 사용처가 통과한 세트만 B/C로 확장한다.

- A/B/C의 footprint·connector·핵심 socket signature 동일성 test
- 같은 preset·roof·작물열·수목의 연속 반복 검사
- 90도 회전과 전후 반전 허용 여부를 세트별로 제한
- Farm↔Town 6종과 Town↔City 6종의 dominance 전이 확인
- 생산·직판·동네상점, 농장출하·지역집배송·도심물류, 저밀도→고밀도 주거, 지역정보·커뮤니티의 District Recipe 4개 조립

완료 Gate: A/B/C 교체가 길과 상태 socket을 끊지 않고, Farm·Town·City가 각각 생산·저밀도 생활·고밀도 유통의 dominant 역할을 유지한다.

### CMP9 — 품목 확장과 보류 품목 Gate

감자 slice가 통과한 뒤에만 가격 연결 품목을 늘린다.

1. 직접 연결 10개 품목을 한 품목씩 추가한다.
2. Pumpkin·Squash는 `대표가격` badge와 서로 다른 상품 identity를 유지한다.
3. 추가 판정 17개는 환경 object 또는 `연결 검토 필요`로만 표현한다.
4. 쌀·고구마·마늘 등 전용 asset 공백은 다른 작물 외형으로 대체하지 않는다.
5. cargo·inventory·shelf 연결은 prefab 이름이 아니라 같은 product·cargo lineage로 검증한다.

완료 Gate: direct·representative·candidate가 UI와 test에서 혼동되지 않고, 유사 asset을 이용한 잘못된 HS·가격 연결이 거부된다.

### CMP10 — 논·Interior·고밀도 Detail은 별도 후순위

#### 논

- 먼저 사각필지·논두렁 예정·농수로 예정·논길을 명시적 Blockout으로만 만든다.
- Rice·담수면·논둑·농수로·수문 asset과 Android shader·overdraw를 검증한 뒤에만 완성 논 단지와 쌀 상품 anchor를 허용한다.

#### Town Interior

- 대표 House와 Shop 각 한두 개만 Object Focus 대상으로 선택한다.
- 실제 개인정보를 넣지 않고, roof·wall occlusion과 renderer budget을 검증한다.
- World Overview에서는 interior와 작은 생활소품을 비활성화한다.

완료 Gate: 논 Blockout과 완성 Visual이 catalog에서 구분되고, interior를 끄더라도 World 흐름과 업무 object가 유지된다.

### CMP11 — 최종 World 배치와 품질·성능 증거

- 별도 library preview를 보존하고 검증된 subset만 제품 World 후보 Scene에 배치한다.
- World Overview, Farm·Town·Hub·City Focus, passenger/inbound/outbound Corridor Focus와 대표 Journey Follow를 캡처한다.
- World·Zone·Object Focus별 renderer, shadow caster, transparent overdraw와 memory를 측정한다.
- PC와 Android Player 증거를 구분한다.
- Game View 변경 기록과 대표 PNG를 저장소 기준 경로에 남긴다.
- Console, missing script, vendor prefab reference, material·shader, duplicate key와 dangling connector를 최종 검사한다.

완료 Gate: UI와 Card를 숨겨도 Farm·Town·City가 독립 발전하고 중간 Hub에서 여러 origin 화물이 모여 재출하되는 World로 보이며, 상품·Cargo·NPC·건물 외형이 업무 권위를 소유하지 않는다.

## 7. 내일 구현 권장 절단선

하루에 모든 후보 prefab을 만드는 것을 목표로 하지 않는다. 첫날 권장 순서는 다음과 같다.

1. `CMP0` 기존 Showcase 검증 복구와 기준선 고정
2. `CMP1` 공통 additive 계약과 기존 Farm 24개 회귀 test
3. `CMP2` Town·City·Farm bounds·pivot·connector와 animation·Avatar·FX source 측정
4. `CMP3` 사람 route와 Farm/Town→Hub→City 화물 route·Gate A형 연결
5. `CMP4`의 Farm 실제 감자필지·Town 기본주택·City 공동주택·Hub Dock 각 1종
6. 시간이 남으면 `CMP4-A`의 source catalog·missing controller validator까지만 구현

첫날 완료 목표는 “많은 세트 수”가 아니라 다음 문장이다.

> 기존 Farm Composition을 깨지 않고, Farm·Town·City 독립 Region과 중간 물류허브, 사람 route와 Hub 경유 화물 route의 최소 A형을 재현 가능하게 생성하고 검증했다.

첫날에 animation clip 탐색·작업 동작·FX를 한꺼번에 붙이지 않는다. `CMP4-A` 공용 Idle/Walk는 actor socket이 검증된 뒤 둘째 변경 묶음에서 닫고, `CMP5` 관통 World에 사용한다. `CMP6` 가격 카드와 B/C 대량 확장은 그 기준선이 통과한 다음 변경 묶음으로 분리한다.

## 8. 변경 묶음 권장 단위

구현은 되돌릴 수 있도록 다음 맥락으로 나눈다.

| 변경 묶음 | 포함 범위 | 포함하지 않는 것 |
| --- | --- | --- |
| 1. 기준선 복구 | Test Runner·Showcase 증거 | 새 asset |
| 2. 공통 계약·실측 | schema·validator·measurement test/report | 제품 World 배치 |
| 3. 도로 A형 | connector backbone·preview | 주택·상가 대량 배치 |
| 4. Pack별 최소 A형 | Farm·Town·City 최소 module | B/C 전체 |
| 5. animation 최소 기반 | source validator·intent/catalog·Idle/Walk retarget | Zone별 작업 동작 전체 |
| 6. 관통 District | boundary A형·기존 cargo lineage·대표 actor/vehicle 이동 | 가격 API |
| 7. 감자 가격 카드 | FPC0~FPC3 감자 slice | 29개 품목 일괄 확장 |
| 8. 선택적 확장 | 사용처가 확인된 B/C·District | 미사용 후보 |
| 9. 후순위 detail | 논 Gate·Interior·작업 animation·성능 최적화 | 가짜 쌀·개인정보 |

각 변경 묶음은 code·test·생성 asset·Scene·Game View 변경 기록을 같은 맥락으로 검증한다. commit과 push는 별도 명시 요청이 있을 때만 수행한다.

## 9. 중단 조건

다음 중 하나라도 발생하면 후보 수를 늘리지 않고 해당 Gate에서 멈춘다.

- 기존 Farm 24개 prefab 또는 WORLD Scene 직렬화 회귀
- 원본 Synty prefab·material dirty 발생
- connector mismatch, overlap, z-fighting 또는 출입구 단절
- 환경 prefab에 stable ID·권한·가격·수량 저장
- Town NPC·House 외형에서 가족·거주·종교·역할을 추론
- crop renderer 수에서 수확량·재고 계산
- cargo 이동·NPC 도착·FX 완료를 업무 성공으로 처리
- 공공가격과 실제 계약·매입·판매·정산가격 혼동
- 비교 불가능한 단위·통화·시장 단계의 자동 비교
- Test Runner·Console·Game View 증거 없이 다음 library 확장
- Android 실측 없이 성능 수치 확정
- 누락 controller·clip을 조용히 대체하거나 리타기팅 결과를 Synty 제공 animation으로 오표기

## 10. 문서별 실행 위치

| 기준 문서 | 이 통합 순서에서 사용되는 단계 |
| --- | --- |
| [POLYGON Farm 식품 Asset·HS·가격 연결 조사](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md) | CMP6·CMP9·CMP10 |
| [Farm 상품·가격 카드 상호작용 흐름](UnityFarmProductPriceCardInteractionFlow.md) | CMP6·CMP9 |
| [POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md) | CMP2·CMP4·CMP7·CMP8 |
| [City 주거단지·십자형 도로 Modular 설계](UnityCityResidentialRoadModularCompositionDesign.md) | CMP2·CMP3·CMP4·CMP8 |
| [Farm 시설하우스·밭·논 단지 Modular 설계](UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md) | CMP2·CMP3·CMP4·CMP8·CMP10 |
| [POLYGON Town 반복 배치 Composition Set 조사](UnityPolygonTownCompositionSetResearch.md) | CMP2·CMP3·CMP4·CMP7·CMP10 |
| [Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md) | CMP2·CMP3·CMP5·CMP7·CMP8·CMP11 |
| [Farm·Town·City 3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md) | CMP1~CMP5·CMP7~CMP8·CMP11 |
| [Farm·Town·City 지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md) | CMP1~CMP5·CMP7~CMP8·CMP11 |
| [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md) | CMP2·CMP4-A·CMP5·CMP11, FARM-3 |
| [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md) | 기존 WORLD 기반과 CMP11 최종 품질 기준 |
| [City·Farm World P0 기준선과 Asset Inventory](UnityCityFarmWorldP0Inventory.md) | CMP0·CMP2 재사용 기준선 |
| [Unity World 구현 현황과 우선순위](UnityWorldImplementationPriority.md) | CMP0 이전 상태와 Composition 뒤 FARM-3 복귀 순서 |

## 11. Composition Track 종료 뒤 복귀

Composition Track은 배경을 무한히 확장하기 위한 새 상시 작업축이 아니다. `CMP11` 또는 합의한 중간 절단선의 증거가 완료되면 기존 Simulation 우선순위로 복귀한다.

다음 Simulation Gate는 기존 계획대로 FARM-3 농부 작업 Presentation이다. 이때 `CMP4-A/ANIM2`의 공용 locomotion을 재사용하고 농부 작업 animation 한 종류만 `ANIM3`로 추가한다. 농부 이동·정지·회전·animation은 FARM-2의 확정된 Simulation Task를 표현하며, 도착이나 animation 완료가 상태 확정 권위를 갖지 않는다.
