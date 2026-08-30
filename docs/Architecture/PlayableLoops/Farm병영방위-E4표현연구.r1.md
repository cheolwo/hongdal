# Farm 병영 방위 E4 표현 연구 r1

- 상태: `Accepted`
- 연구 revision: `farm-barracks-defense.presentation-study.r1`
- 대상: `playable-loop:farm-barracks-defense.v1 / WI-FARM-DEFENSE-MOBILIZE`
- 증거 상한: Presentation E4. 실제 Prefab 배치·Renderer·Collider·Rig·입력은 E5 이후다.

## 판독 순간과 H 후보

| 권위 상태 | 판독 순간 | H 능력 후보 | VisualKey |
| --- | --- | --- | --- |
| `Stationed` | 준비 분대와 생산 인원 투입 대가를 확인 | `Spatial.FarmDefenseMusterAnchor` | `Farm.Defense.Squad.Stationed` |
| `Mobilized` | 접근 위협·출동 분대·생산 기여 중단을 확인 | `Spatial.FarmDefenseWatchAnchor` | `Farm.Defense.Squad.Mobilized` |

초소 H2는 Farm 외곽 접근로에 놓이는 후보이며 이 연구만으로 실제 H 위치를 승인하지 않는다. 카드의 Preview 기준점은 `InteractionAnchor.WI-FARM-DEFENSE-MOBILIZE.Preview`이고 Confirm 권위는 Simulation에만 있다.

## 실제 보유 Synty 후보

| 역할 | 주 후보 | SHA-256 | 대체 후보 |
| --- | --- | --- | --- |
| 집결 쉼터 | `Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Shelter_01.prefab` | `4995f85c342736747bad94477782895cd75e56e4ee2839f7d38aa945ea1644c7` | Farm 목재 문·울타리 |
| 장비 준비 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Rack_01.prefab` | `e72a2b9fad7bc77c11628ff2ffeddf42da36f5206754a7073531e4ebda3253e7` | Construction Barrier |
| 출동 분대 Actor | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Builder_Male_01.prefab` | `442d00142fceb83c1a063778ae7f40d3e8928c94c91c641040019b01c039027b` | Starter Actor |
| 경계 신호 | `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_Flag_Wall_01.prefab` | `d30cb5c15ca83cb8d54b5569297e34eab6a3afb7c809c355d2313743a86b7620` | primitive 경보 표식 |

전용 군인 Prefab이나 검증된 출동 Animator 결속은 확인하지 않았다. E4에서는 `FarmDefenseSquad / Squad.Mobilize` 역할과 이동 Clip 후보만 동결하고 실제 Rig·Avatar·root motion·중단·귀환은 E5에서 검증한다. 후보 결속이 없으면 `Primitive.FarmDefenseSquadMarker`와 정적 상태 Cue를 사용한다.

## 자동 검증

`SimulationFarm방위소집PresentationPreparationTests 7/7`은 동일 revision, 상태별 H·VisualKey, 생산 중단 판독, fallback, StableId 정렬과 hash, 혼합 revision·중복 결속 거부를 검증한다. 이는 실제 초소·분대·전투 또는 Game View 증거가 아니다.
