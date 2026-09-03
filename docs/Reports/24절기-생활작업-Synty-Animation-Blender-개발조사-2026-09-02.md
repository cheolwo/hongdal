# 24절기 생활 작업 Synty·Animation·Blender 개발 조사

## 1. 결론

- 기준 기획은 `PLAN-TIME-SEASONAL-001`, `solar-term-seasonal-food-research.plan.r8`, SHA-256 `9C83AB17280A4F6F9ACC3A4FF681DA93B1D583A01688CBE3F2D5B10CD65F2725`다.
- 대표 장면은 농사 `WI-FARM-01 밭갈이`, 운송 `Town 주문 차량 도착 뒤 손수레 하역`, 수리 `기존 한스 집의 비계·사다리·목재 보수`로 좁혔다.
- 세 장면 모두 Actor와 정적 소품은 보유하고 있다. 그러나 밭갈이·적재/하역·망치질 전용 Clip, 손·도구·대상 접촉점, 안전 중단·귀환, 실제 NPC 선택 기준점은 제품에 결속되어 있지 않다.
- 농사는 전용 작업 Clip 계획이 필요하고, 운송은 기존 Walk를 하체 이동에 재사용하는 Unity 조립을 먼저 시험하며, 수리는 망치질 동작 계획이 필요하다. 정적 소품 존재를 작업 애니메이션이나 업무 완료로 세지 않는다.
- 계절 복장 교체는 작업 pose와 도구 소유를 해제한 안전한 작업 단위·외출·교대 경계 뒤에만 수행한다. Farm과 Construction 완성형 Actor의 Skeleton·Avatar를 한 Actor에 섞지 않는다.
- 일반 생활 설명은 Actor 가까이의 별도 말풍선 기준점, 실제 퀘스트·중요 선택은 별도 대화창 진입 기준점으로 분리해야 한다. 둘 다 현재 Actor Prefab에는 확인되지 않았고 열람만으로 작업·퀘스트·보상을 변경할 수 없다.
- 이번 결과는 파일·meta·대장·기존 보고의 정적 조사다. 코드·UI·대사·음성 작성, Unity Editor, Scene, Play Mode, Game View, Blender 실행, 신규 Clip/Prefab 제작, 구매, E 승격, commit/push는 수행하지 않았다.

## 2. 공통 Actor·복장·애니메이션 경계

### 2.1 Actor와 계절 복장

| 역할 | 정확 파일 | GUID | 파일 SHA-256 | 판정 |
| --- | --- | --- | --- | --- |
| Farm 주민 | `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Male_01.prefab` | `3c3a8236be548bb4c892cf39a5abadfe` | `D176B368838CBED5024D89CB953B65FC8A0D3630E3FE11F214A0108B262E2343` | 기본 몸체 그대로 재사용 |
| Farm Avatar 원본 | `Assets/Synty/PolygonFarm/Models/Characters.fbx` | `0125309e38bea8a48bc578cf8f220634` | `EA7A567F52F593024854AD2947A64B7C4540985B2C71E93F79FF6574F2DB810F` | 재사용. 이번 조사에서 fresh import/재생은 미검증 |
| Construction 주민 | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Builder_Male_01.prefab` | `0c4f2b2dbeb9b1f40ae35319fd9995e6` | `442D00142FCEB83C1A063778AE7F40D3E8928C94C91C641040019B01C039027B` | 운송·수리 Actor 후보, Unity 조립 보완 |
| 작업복 Actor | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Builder_Overalls_01.prefab` | `d0a9ec5dc1484dc4bad93811a9c0ab65` | `AA542DC4D6962E90AA548E3013D7243F90F0540E3FA3749E2C60D2836241A75B` | 전신 Actor 교체 후보. Farm 의복 부품 아님 |
| 우비 Actor | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Builder_Raincoat_01.prefab` | `d3088af5cb703ce42947223d4f444d97` | `F5B5769FBC165D6259BBB22AF1B910B392F5D7213866ED52FE8FE3BD735978C4` | 우천 작업 후보, 실제 계절 전환 미검증 |
| Construction Avatar 원본 | `Assets/Synty/PolygonConstruction/Models/Characters.fbx` | `fc8c23e5ebff8b54ebeb0799b9cbed72` | `4B4BE793EE38D24B943A8CA2AFA9C1932896B3B7A07C0E0C961B49F20904120C` | Humanoid 정적 몸체. 자체 Clip 없음 |

- Farm Actor 기본값은 `m_ApplyRootMotion: 1`이다. 작업 wrapper가 권위 이동과 충돌하지 않도록 root motion을 명시적으로 끄고 읽기 확인해야 한다.
- Construction 세 Actor는 같은 Construction Avatar 계보지만 독립된 전신 Actor다. Farm 몸체 위에 Overalls/Raincoat SkinnedMesh를 단순 부착하는 근거가 없다.
- Construction 공급사 Controller GUID `a064967857b33594ba417763a7738412`는 참조되지만 조사 범위에서 소유 `.meta`를 찾지 못했다. Controller 존재를 재생 가능성으로 세지 않는다.
- Farm과 Construction의 atlas 재질은 팩 전체가 공유한다. 원본 shared material 색을 바꾸지 않고 정확 Renderer/material slot을 가진 프로젝트 소유 복사본만 써야 한다.
- 독립 장갑·신발/장화 및 여름 경량복·겨울 보온복은 현재 조사 범위에서 **후보 없음**이다.

### 2.2 공통 이동 후보와 전용 작업 Clip 공백

| 용도 | 정확 파일 | GUID | 파일 SHA-256 | 관찰 |
| --- | --- | --- | --- | --- |
| Walk | `Assets/Synty/AnimationBaseLocomotion/Animations/Polygon/Masculine/Locomotion/Walk/A_Walk_F_Masc.fbx` | `eead3eb07cb89cb4eb25b3550df5e84b` | `B2081DF84B35040DD5105AAAE175E5E84AB0C8C97C1E9A5AEE1AB45C1E82E78D` | loop, Humanoid, XZ/Y bake. copied-avatar pelvis/Hips 경고 보존 |
| Idle | `Assets/Synty/AnimationBaseLocomotion/Animations/Polygon/Masculine/Idle/A_Idle_Standing_Masc.fbx` | `a4b8f2deec6a99945987cfc59b1b4e54` | `76DC31C92FD3CA69F28E92F1F530EAC6F60EE71EA0F25D490A633F49D2088DD0` | 접근 뒤 대기·귀환 후보 |
| Crouching | `Assets/Synty/AnimationBaseLocomotion/Animations/Polygon/Masculine/Idle/A_Idle_Crouching_Masc.fbx` | `52a2eb3158000934eb33267c807b8b15` | `44FACA900C154C18CE7473D26342295DB972E0D35573E46122DB48CA55F8BBF0` | 굽힘 자세 참고, 작업 접촉 Clip 아님 |

BaseLocomotion·Emotes·SwordCombat의 선언 Clip 이름에서 `Harvest/Plant/Water/Sow/Dig/Lift/Pick/Pickup/Carry/Interact/Grab/Pour` 및 제한된 운송·수리 이름 후보를 찾지 못했다. 다른 이름 take까지 전부 없다는 뜻은 아니지만, 이번 세 작업의 전용 Clip은 **확인되지 않았다**.

현재 `공용AnimationContract` intent는 Idle/Walk/Run/Guard/Attack/Stagger이고, `Npc업무행동View`는 Navigating만 Walk, 나머지는 Idle로 표현한다. `Tilling`, `CargoUnloading`, `FacilityRepair`용 등록 `AnimationRole`·`ActionCue`는 없다.

## 3. 농사 대표 장면 — 봄 밭 준비·밭갈이

### 3.1 선정 근거와 자산

`WI-FARM-01/Tilling`의 농장 주민이 Dirt Rows 작업 구역으로 접근하여 괭이질을 반복하고, 권위 Task가 끝난 뒤 같은 revision의 `Untilled → Tilled` 결과를 다시 표현하는 장면이다.

| 역할 | 정확 파일 | GUID | 파일 SHA-256 | 판정 |
| --- | --- | --- | --- | --- |
| 봄 모자 | `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Attach_Buckethat_01.prefab` | `60fa9e0c6567acd45b844ed4537a6d72` | `D6A84D8E2221D728058D4535A397A71B7D104D4B96BD593E7F92F36B2E63C6B1` | Unity 조립 보완 |
| 괭이 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Tool_Hoe_01.prefab` | `5a142f078c588454ca16909367867308` | `8D70CB70A685491497CF60E2D7F460D2CC8EBB6C2883B0114CC6AE1768571A78` | 정적 외형 재사용, 양손 Slot·접촉 보완 |
| 밭고랑 | `Assets/Synty/PolygonFarm/Prefabs/Environments/SM_Env_Dirt_Rows_01.prefab` | `c738b62174ab09f4eb2d7b486398a6ed` | `1C9876DE7FE7B6F3800617ECB0B23C2206152D4AF1B7C6065193E7774356E15A` | 형상 재사용, 상태 binding 필요 |
| 배경 헛간 | `Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Barn_01.prefab` | `1135f5bcd489da04cb5f4fcb4355006b` | `727ED17B903B0D7BBD1F25A2D7636F377637B77C606FC2EF0A70696B76813C59` | 외관 후보일 뿐 작업 완료 근거 아님 |

Bucket hat과 괭이는 자체 MeshCollider가 있어 Actor 자식 조립 시 비활성화 또는 충돌 layer 분리가 필요하다. 머리 socket, 양손 grip, 괭이날-토양 contact marker는 없다.

### 3.2 동작·중단·귀환 판정

- 제안 의미명 `FarmTillingWork`와 접근→반복→중단 요청→완료/취소→Idle은 후속 계약 후보이지 등록된 ID가 아니다.
- 전용 밭갈이 Clip은 **후보 없음**이다. 기존 Sword HeavyCombo는 칼/벌목 자세여서 양손 괭이 grip, 토양 접점, 지지발, 반복을 증명하지 못한다.
- Unity retarget/IK로 접촉과 반복을 만족하지 못할 때에만 **Blender 계획 필요**로 연다.
- 입력은 선택된 Farm Skeleton, 출력은 프로젝트 소유 Humanoid 작업 Clip으로 한정한다. Skeleton, Avatar, root, 재질과 원 Actor를 보존하고 실패 시 static Actor+권위 진행 표시 fallback으로 복귀한다.
- 가장 작은 안전 단위는 괭이 한 타가 아니라 Confirm으로 시작된 Task 하나다. `WorldTick 완료+동일 revision 재조회+예약 해제` 또는 `권위 취소+Untilled 유지+예약 해제` 뒤에만 절기 복장과 다음 업무를 교체한다.

## 4. 운송 대표 장면 — Town 주문 차량 도착·손수레 하역

도착→Confirm→하차→검수→적재→귀환이 비교적 구체적인 기존 Town 주문 수령 문맥을 쓴다. 정차 차량과 화물 외형은 재사용하되 이동·하역은 별도 표현 조립이다.

| 역할 | 정확 파일 | GUID | 파일 SHA-256 | 판정 |
| --- | --- | --- | --- | --- |
| 배달 트럭 | `Assets/Synty/PolygonTown/Prefabs/Vehicles/SM_Veh_Truck_Delivery_01.prefab` | `22659212538d64441aa7af56c6a17ce0` | `68ED88EA3CA300998D71573451FDD6A8C3B0432D93FF4A5D9FF690E9D73E1DBD` | 정차 외형 재사용; 이동·문·바퀴 보완 |
| 손수레 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_HandTrolley_01.prefab` | `ba0da51144c6c4248a066ae612caaef2` | `F9E89D17A0491D01CFA83B79AB7410C7FE59633F66DC8608B56DBF58A175A157` | Unity 조립 보완 |
| 팔레트 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Pallet_01.prefab` | `15aefff89fb17ca4d9574be8a0ed390a` | `AB4D307C7875EF600682752AEB5DAD6633086C7EE22EF690C9587BFB9D0872A6` | 정적 외형 재사용 |
| 상자 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Crate_01.prefab` | `a39495696f707e84a96ff152d750caa2` | `638DA6352E67B7E8830879E42775F91021C55067DF91AB00DD5773C7A916C1F5` | 정적 외형 재사용 |

- 트럭에는 Collider 8개가 있지만 Animator/LOD와 cargo/seat/hand 기준점이 없다. 손수레에도 grip/load 기준점과 wheel Clip이 없다.
- 제안 의미명 `CargoUnloadingWorker`와 Approach/Inspect→PushOrCarryLoop→PlaceContact→SafePause→ReturnIdle은 미등록 후보다.
- Walk는 하체 이동의 **Unity 조립 보완** 후보일 뿐이다. 권위 route/CharacterController가 이동을 소유하고 visual root motion은 꺼야 한다.
- 양손 손잡이, 화물 attach/release, 바퀴 회전, Walk↔Idle, 작업 revision을 하나의 소유 wrapper에서 묶어야 한다. upper-body push가 layer/IK로 해결되지 않을 때만 해당 동작을 Blender 계획으로 올린다.
- 안전 중단은 화물이 지지면에 놓이고 손수레와 차량이 정지한 뒤다. 이동·화물 접촉 중 선택은 경로를 끊지 않고 busy 상태만 반환해야 한다.

## 5. 수리 대표 장면 — 한스 집의 비계·사다리·목재 보수

기존 한스 집을 보존하고 수리 중 소품을 덧입히는 장면이다. 완성 Farmhouse를 한스 집 대체로 사용하지 않는다.

| 역할 | 정확 파일 | GUID | 파일 SHA-256 | 판정 |
| --- | --- | --- | --- | --- |
| 비계 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Scaffold_Preset_01.prefab` | `763a9fdc43a797e4eaf803854c430de3` | `7A383CCBBEE354B7E02F36948503D5371AA79B0D740222BDF5036206270930F2` | 정적 외형 재사용, 배치 보완 |
| 사다리 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Ladder_01.prefab` | `3abf514a6823faa48aa619aa6670c586` | `F758559E76860C54E3D7676049440CAE34D6EE19AFAAEBADB9FB2F6AFE7DAF50` | 정적 외형 재사용, 접촉 보완 |
| 목재 더미 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Plank_Long_Stack_01.prefab` | `53c406acd6feb9b409da749a8baad33f` | `2894264141824D2A6A182F8A047F573C8CBFBB4F90FA2404643D773F37CAF48B` | 그대로 재사용 후보 |
| 허리 망치 | `Assets/Synty/PolygonConstruction/Prefabs/Characters/Attachments/SM_Chr_Attach_BeltLoop_Hammer_01.prefab` | `b35adbc5dd80bdb4fbbe2710a7f5a296` | `B1F873A0DA6FC2C1246F575AF9CA8D284EAD0A03398D90A62C396B8F15FD47C8` | BeltSlot 외형 후보 |
| 손 망치 | `Assets/Synty/PolygonConstruction/Prefabs/Tools/SM_Tool_Hammer_Claw.prefab` | `981d75f5d4ad7be4bbf819361ad1fa3d` | `79266D2EDDE77767FA0E2B148F76979F43DEAF50BBD5BCCAA93B94EBC061530B` | HandSlot 외형 후보 |

- 위 소품에는 Animator/LOD 및 손잡이·타격·사다리 손발 기준점이 없다.
- Belt hammer와 handheld hammer는 작업 상태에 따라 상호배타적으로 관리해야 한다. 동시에 보이면 이중 도구 충돌이다.
- 제안 의미명 `FacilityRepairWorker`와 Approach/Inspect→HammerLoop→ImpactContact→SafePause→ReturnIdle은 미등록 후보다.
- 검증된 망치질/수리/사다리 Clip은 **후보 없음**이다. Sword HeavyCombo를 직접 재사용하지 않는다.
- 한손 grip, 반대손 안정, 망치머리-수리 대상 접점, 지지발, 반복과 어느 구간에서도 안전한 귀환을 갖춘 작업 Clip은 **Blender 계획 필요**다. 실제 Clip 제작 승인은 별도다.
- 소품 덧입힘으로 상태가 읽히지 않아 동일 건물의 damaged/repaired mesh pair가 꼭 필요하다고 재승인될 때만 건물 Blender 계획을 연다. 입력은 프로젝트 소유 한스 집 사본, 변경은 국소 파손/보수, 출력은 상태 variant이며 pivot/scale/문·실내/Collider/material slot/LOD를 보존한다.
- 안전 중단은 타격 follow-through 뒤 망치를 대상에서 거두고, 사다리 승강 중이 아니며, 필요 시 HandSlot→BeltSlot 복귀가 끝난 시점이다.

## 6. NPC 선택·말풍선·대화창 충돌 감사

현재 조사한 Farm/Construction Actor Prefab은 Collider가 없고 NPC 선택용 `Selectable` 또는 `InteractionAnchor` 결속도 확인되지 않았다. `Npc업무행동View.InteractionPointKey/InteractionPoint`는 NPC가 향하는 작업 지점이며 플레이어가 주민을 선택하는 기준점이 아니다.

| 진입 | 최소 기준점 | 작업 동작과의 경계 | 현재 판정 |
| --- | --- | --- | --- |
| 일반 생활 말풍선 | ActorStableId에 결속된 presentation-only selection proxy와 머리 위 말풍선 anchor | 짧은 선택 요청만 기록. 안전 반복 구간이면 작업을 바꾸지 않고 1~2문장 표시; 위험 구간이면 busy와 후속 가능 시점만 표시 | **후보 없음 / Unity 조립 보완 필요** |
| 실제 퀘스트 대화창 | 실제로 열린 Quest/중요 선택 상태와 결속된 별도 dialogue-entry anchor | 제안·조건·대가·수락·거절을 보여도 명시 선택 전 상태 불변. 평상시 생활 말풍선과 동일 anchor로 자동 승격 금지 | **상태 공급·UI 진입 미검증** |

- 선택 proxy는 작업 InteractionPoint, 차량·도구 Collider, WI Preview/Confirm, Quest marker와 분리한다.
- 말풍선은 `현재 하는 일 + 이 장소에서 직접 관측한 절기/날씨 맥락`까지만 담는 후보이며, 정확 대사는 작성하지 않았다.
- 상세 설명은 플레이어가 다시 선택했을 때 해당 NPC가 아는 출처·일정·보유 물자·작업 이유만 읽는 후보이다. 미발견 지역, 도시 전체 물류, 다른 NPC의 숨은 상태를 노출하지 않는다.
- 농사는 Task terminal/도구 안전 높이/ReturnIdle 뒤, 운송은 화물 내려놓기와 정차 뒤, 수리는 망치 수납 및 비승강 상태 뒤에 상세 진입이 가능하다.
- 말풍선과 대화창 모두 퀘스트 수락, 작업 중단, 업무 완료, 보상, 권위 revision 쓰기를 수행하지 않는다. 실제 퀘스트가 없으면 표식을 만들지 않는다.
- 말풍선 유지 시간·거리·겹침·이동 중 위치, 음성, 세계 시간 정지/감속, 접근성 대체 표현은 미정이다.

## 7. 네 등급 통합 판정

| 장면 | 그대로 재사용 | Unity 조립 보완 | Blender 계획 필요 | 후보 없음 |
| --- | --- | --- | --- | --- |
| 농사 | Farmer 몸체/Avatar, Walk/Idle, Dirt Rows, Barn 외관 | root motion off, 모자·괭이 Slot/Collider, WorkArea/revision, 선택·말풍선 anchor | IK/retarget으로 부족할 때 양손 괭이질 Clip | 전용 Tilling Clip, 승인 Role/Cue, 접촉점, 안전 중단 계약 |
| 운송 | 정차 트럭·팔레트·상자 외형 | 손수레 grip/load, 바퀴, 화물 수명, Walk/Idle, route·revision, 선택·말풍선 anchor | upper-body push가 Unity 조립으로 부족할 때 동작 부분 | Carry/Lift/Load/Unload/Push/Place Clip, cargo/seat/hand anchor |
| 수리 | 비계·사다리·목재·망치 외형 | 기존 집 덧입힘, 반경/Collider, Belt/Hand Slot, 선택·말풍선 anchor | 망치질 작업 Clip; 재승인 시 국소 damaged/repaired 건물 variant | 검증된 Repair/Hammer/Ladder Clip, 접촉점, 안전 귀환 |

## 8. 다음 담당과 가장 이른 재개

1. 개발: 세 작업의 권위 Task revision, terminal/cancel, Actor/WorkArea 예약 해제와 `SafeSeasonalWorkBoundary` 후보를 기존 계약에 재결속한다.
2. 애니메이션: 장면별 단일 Actor/Avatar를 정하고, 접근·반복·접촉·중단·귀환 및 도구 Slot을 정적/격리 시험한다. 기존 Clip으로 부족함을 확인한 뒤에만 작업 Clip 제작 계획을 연다.
3. 월드·공간·배치: 작업 지면·차량/하역 지지면·수리 반경, NPC selection proxy, 말풍선/대화창 anchor가 통행과 충돌하지 않는지 격리 검토한다.
4. 기획: 말풍선 정확 수명·거리·겹침·시간 처리와 퀘스트 대화창의 실제 상태 공급을 후속 선택으로 남긴다.
5. 실제 Unity·E5는 위 계약과 자산 후보가 동결된 뒤 별도 작업 명세와 단독 Editor 검증으로 연다.

## 9. 검증 범위

- 개발이 농사, 운송·수리 자산, 공통 Animation/Rig·상호작용의 세 읽기 작업을 비중첩 배분하고 최종 결과를 통합했다.
- 기준 기획: [24절기·제철 자료 조사와 기획 연결](../AI/24절기-제철자료-조사와기획연결-2026-08-31.md).
- 선행 조사: [24절기 주민 복장·식생 개발 조사](24절기-주민복장-식생-Synty-Blender보완-개발조사-2026-09-02.md), [WI 애니메이션 보유 자산 대조](WI애니메이션-보유자산대조와제작우선순위-2026-08-30.md).
- Synty 기능 대장 SHA-256은 `9366C4F8708EB3170184491BC7B3D52D32DD45BA277E74A5D328E68C8A48717B`, human taxonomy SHA-256은 `FF9C656AEC6E3A48E9A68337036248C66F6010C617D462FAB29B3E1A87C68770`으로 확인했다.
- 실제 테스트·컴파일·Unity/Blender는 실행하지 않았으며, 문서 범위 검증만 별도로 수행한다.
