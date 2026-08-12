# Unity Synty Animation·FX 재사용과 리타기팅 설계

## 1. 목적과 상태

이 문서는 Farm·Town·City Region과 Regional Logistics Hub에서 Synty 캐릭터·차량·설비·FX를 가능한 범위까지 재사용하되, animation 완료를 업무 상태의 권위로 오인하지 않기 위한 Presentation 설계다.

- 기준일: 2026-08-09
- 상태: 구현 전 설계와 실제 import inventory 기록
- 확인 대상: `C:\Users\user\ssalddel\Assets\Synty`
- 이번 작업에서 Unity controller·clip·prefab·Scene은 생성하거나 수정하지 않았다.

## 2. 실제 import 상태

현재 가져온 Synty 폴더를 파일과 `.meta` 기준으로 조사한 결과는 다음과 같다.

| 항목 | 확인 결과 | 구현 판단 |
| --- | --- | --- |
| 독립 `AnimationClip` (`.anim`) | 0개 | 바로 재생할 Synty 제공 clip이 없음 |
| `AnimatorController` (`.controller`) | 0개 | 공용 controller를 별도로 구성해야 함 |
| `AnimatorOverrideController` | 0개 | variant override 기반도 아직 없음 |
| FBX | 2,099개 | mesh·rig 원본은 충분하지만 animation 포함 여부는 별도 판정 |
| character FBX embedded clip | Farm·Town·City·Generic·Starter 모두 `clipAnimations: []`, `importAnimation: 0` | character FBX에서 걷기·작업 clip을 가져올 수 없음 |
| Humanoid character rig | 각 Pack character FBX에서 `animationType: 3` 확인 | Synty 외형과 Avatar에 검증된 Humanoid clip을 리타기팅할 수 있음 |
| Town character controller 참조 | 8개 character prefab이 GUID `5dee6d9587d12df4daaf4452fb7387e5`를 참조하지만 대응 controller asset을 찾지 못함 | 해소 전에는 사용 가능한 Synty controller로 간주하지 않음 |
| Synty ParticleSystem prefab | Farm 11개, City 2개, Generic 17개 확인 | 업무 의미와 일치하는 FX를 우선 재사용 가능 |

프로젝트 전체 `Assets`에서 확인된 유일한 `.controller`는 Ssalddel이 생성한 `UrbanMarketRepresentativePrimitive.controller`이며 Synty 제공 animation 자산이 아니다. 따라서 “Synty animation을 최대한 사용한다”는 목표는 현재 import 상태에서는 **존재하지 않는 clip을 가정하는 것**이 아니라 다음 두 가지로 해석한다.

1. Synty Humanoid 외형·Avatar·rig를 공용 animation adapter 뒤에서 최대한 재사용한다.
2. Synty가 실제 제공한 ParticleSystem과 이후 추가로 확인되는 정식 clip·controller만 출처를 기록하고 우선 사용한다.

## 3. 재사용 우선순위

Animation source는 다음 순서로 선택한다.

1. `SyntyProvided`: 현재 또는 이후 import에서 실제 파일·license·clip이 확인된 Synty animation
2. `Retargeted`: 사용권과 출처가 확인된 Humanoid clip을 Synty Avatar에 리타기팅한 animation
3. `Procedural`: 차량 이동·바퀴 회전·문·리프트·설비처럼 Transform·constraint·간단한 curve로 표현하는 동작
4. `Fallback`: clip 누락·Avatar 불일치·성능 tier 제한 시 사용하는 정지 pose 또는 최소 이동 표현

`SyntyProvided`가 존재하지 않는데 이름이나 demo 영상만 보고 있다고 기록하지 않는다. `Retargeted`와 `Procedural`은 Synty 외형을 사용하더라도 source kind를 Synty 제공으로 표시하지 않는다.

## 4. Presentation 경계

```text
Canonical 또는 Simulation state
  → AnimationPresentationModel
  → AnimationIntent
  → AnimationKey / FxKey
  → Animation Catalog
  → Animator adapter / procedural adapter / FX adapter
  → Synty VisualRoot의 Avatar·Renderer·ParticleSystem
```

Domain·Simulation·server contract에는 clip 이름, controller GUID, Animator parameter, Synty prefab 경로를 넣지 않는다. View는 다음과 같은 의미 intent만 받는다.

```text
Idle
Walk
Carry
Inspect
Work
Drive
Load
Unload
Wait
Talk
```

Catalog entry에는 최소한 다음 값을 둔다.

- `AnimationKey`, `SourceKind`, source asset reference와 출처·license 기록
- 대상 actor role과 허용 `AnimationIntent`
- Humanoid·Generic·procedural requirement
- loop, in-place/root motion, speed, transition duration
- 필요 hand·tool·vehicle socket
- PC·Android detail tier와 fallback key
- 검증한 Avatar·controller·clip compatibility와 검사 시각

## 5. Region별 최소 intent

| 영역 | 첫 수직 슬라이스 | 후속 intent | 우선 표현 방식 |
| --- | --- | --- | --- |
| Farm | 농부 `Idle→Walk→Inspect/Work→Idle`, Tractor 이동 | 밭갈이·파종·관수·수확·상자 적재 | Synty Humanoid rig+리타기팅, 농기계는 절차형 이동·회전, Farm FX |
| Town | 주민·상점주 `Idle→Walk→Wait/Talk`, 배송자 이동 | 장보기·택배 전달·승하차 | Synty Humanoid rig+리타기팅, 차량은 절차형 |
| Hub | 운송자 `Walk→Inspect`, 차량 `Arrive→Dock→Depart` | 하역·분류·적재·신호 | 사람은 리타기팅, 차량·Dock 설비는 절차형, 상태 일치 FX |
| City | 주민·대표·관리자 `Walk→Wait/Talk`, 배송차량 이동 | 진열 보충·공동수령·마트 업무 | Synty Humanoid rig+리타기팅, 차량은 절차형 |

첫 공용 locomotion은 Farm·Town·City의 Synty Humanoid 세 종류에 같은 in-place `Idle/Walk` 계약을 적용해 Avatar 호환성을 검증한다. 농부 작업이나 하역처럼 도구·손 접촉이 중요한 clip은 locomotion이 통과한 뒤 Zone별로 한 동작씩 추가한다.

## 6. Root motion과 Journey

- Region 내부·지역 간 Journey 위치는 `NavMeshAgent` 또는 검증된 route follower가 소유하고 root motion은 기본적으로 끈다.
- animation은 in-place clip으로 속도와 방향을 표현한다.
- 짧은 국소 작업 동작만 anchor·warp·중단 복귀를 검증한 뒤 제한적으로 root motion을 허용한다.
- canonical revision이 바뀌면 진행 중 transition을 취소하거나 blend해 새 `AnimationIntent`로 조정한다.
- clip 또는 controller load 실패 시 actor를 제거하지 않고 fallback pose와 진단 상태를 사용한다.
- NPC·차량 도착, animation event와 ParticleSystem 완료는 Command·Tick·입고·검수·판매·수령을 자동 실행하지 않는다.

## 7. Town의 해소되지 않은 controller 참조

Town의 Father 01/02, Mother 01/02, SchoolBoy, SchoolGirl, ShopKeeper, Son prefab에서 대응 asset을 찾지 못한 controller GUID를 확인했다.

- 원본 Town prefab YAML이나 material을 직접 고치지 않는다.
- source catalog 검사에서 missing controller reference를 오류로 보고한다.
- Ssalddel wrapper의 `VisualRoot`와 공용 adapter가 검증된 controller를 명시적으로 공급한다.
- 원본 참조가 복구되더라도 clip·license·동작을 다시 조사하기 전에는 `SyntyProvided`로 승격하지 않는다.
- unresolved controller가 있는 prefab은 정적 외형으로는 사용할 수 있지만 animation 완료 품질 증거에는 포함하지 않는다.

## 8. Synty FX 활용

현재 확인한 주요 후보는 다음과 같다.

- Farm: 바람 먼지, 꽃가루, 비, 분무기, 스프링클러, 차량 먼지, 밀 수집·비산, 불
- City: 비, 증기
- Generic: 안개, 잎, 눈, 연기, 햇빛, 바람, 폭포 거품 등

Farm 관수에는 `FX_Sprinkler_*` 또는 `FX_Sprayer`, 농기계 이동에는 `FX_Vehicle_Dust_01`, 수확 연출에는 검증된 crop 의미 범위에서 `FX_Wheat_*`를 사용할 수 있다. 다만 밀 전용 FX를 감자 수확량이나 상품 종류의 증거로 사용하지 않는다. 불·피·연기처럼 상황 의미가 강한 FX는 단지 자산이 있다는 이유로 기본 경관에 사용하지 않는다.

## 9. 구현 Gate와 우선순위

| Gate | 구현 내용 | 선행 조건 | 완료 기준 |
| --- | --- | --- | --- |
| ANIM0 | animation·Avatar·FX inventory와 missing-reference validator | CMP0 | source kind가 사실대로 분류되고 Town missing controller를 검출 |
| ANIM1 | `AnimationIntent`, `AnimationKey`, catalog와 adapter 계약 | CMP1~CMP2 | Domain에 asset 참조 없이 Synty/retarget/procedural/fallback 교체 가능 |
| ANIM2 | 공용 Humanoid Idle/Walk 리타기팅 | CMP3 route·CMP4 actor socket | Farm·Town·City 대표 actor 1명씩이 같은 계약으로 이동 |
| ANIM3 | Farm 작업 1종과 Tractor 표현 | FARM-3 | FARM-2 Task를 표현하되 animation이 Tick을 발생시키지 않음 |
| ANIM4 | Hub 차량 Dock과 운송자 작업 | CMP5 Hub Journey | inbound/outbound 상태와 시각 동작을 분리 |
| ANIM5 | Town·City 생활·서비스 동작 | CMP5 이후 | privacy-safe actor role과 허용 intent만 표현 |
| ANIM6 | FX·전환·중단·Android tier 품질 | CMP11 | fallback, interruption, renderer·Animator·FX 비용 증거 확보 |

Animation 작업은 Map footprint·Gate·route 실측보다 앞서 대량 제작하지 않는다. `ANIM0~ANIM1`은 공통 기반 단계에, `ANIM2`는 최소 actor socket이 생긴 직후, Zone별 작업 동작은 실제 vertical slice와 함께 구현한다.

## 10. 완료 기준

1. Synty에서 실제 제공한 asset과 리타기팅·절차형 결과가 catalog에서 구분된다.
2. 원본 vendor prefab을 직접 수정하지 않는다.
3. Farm·Town·City 대표 Humanoid가 하나의 intent 계약과 controller adapter를 공유한다.
4. actor role별 허용 동작과 fallback이 명시되어 있다.
5. 차량·농기계·설비 동작은 route·socket과 맞고 업무 상태를 확정하지 않는다.
6. FX는 상태 의미와 맞을 때만 켜지고 수량·품질·성공의 근거로 사용되지 않는다.
7. Town missing controller와 clip 누락이 build·catalog 검사에서 조용히 통과하지 않는다.
8. PC와 Android에서 Animator·skinned mesh·ParticleSystem 비용을 각각 측정한다.

## 11. 관련 문서

- [Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)
- [Farm·Town·City 3개 독립 Region Map 구성 설계](UnityFarmTownCityThreeRegionMapLayoutDesign.md)
- [Farm·Town·City 지역 물류허브 Map·Flow 설계](UnityFarmTownCityRegionalLogisticsHubDesign.md)
- [입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)
- [City·Farm World P0 기준선과 Asset Inventory](UnityCityFarmWorldP0Inventory.md)

