# Nature 벌목 동작 — 애니메이션 적용 연구

- 식별: `study:nature-woodcutting:animation.r1`
- 판정: `Required`; 상태: `Accepted` — **5.1 A(기존 imported Clip 소비)만**. B 신규 가공 방식은 미승인/미착수이며 접촉·발고정 목표 미통과, D359 전체 제작 미완료.
- 검토자: 개발 통합 담당 `01a02198-8b2a-7491-ac93-366b30ff474c`. 2026-08-30 개발 반환에서 직전 SHA `175B6F724125310A7F8A8282748BD9CE7F269CF0DF8CB3450617C5382F3A0B1F` 및 5.1 전체를 대조하고 A만 수용했다. 이번 상태 표기 이후 최종 hash를 개발 명세/작업목록에 결속한 뒤 A 코드 착수 배분을 받는다.
- 대상: `playable-loop:nature-shelter-foundation.v1` / `WI-NATURE-06`; 취소는 기존 `WI-NATURE-12`.
- 승인: [D359 원문](Nature벌목동작-Blender제작승인.md), `nature-woodcutting-animation.design.r1 / Approved`, SHA256 `A4A7E942D025372EC4063E0C8152A6ABB31633E92D22DAFD336A40D4B1DF6642` 전체 읽기·직접 일치 확인.
- 명세: [D359 지원 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json). 기존 루프 E7 사본은 이 변경의 완료 증거가 아니다. 첫 전달 상한 E5, 승격 false 유지.
- 부모 기획: [Nature 도끼·벌목·오두막 기초](Nature도끼벌목오두막기초.md) `nature-shelter-foundation.design.r1 / Approved`(D360) 전체 읽음. 기존 planningGate 형식 결손 복원이며 Required 연구를 자동 Accepted로 만들지 않는다.
- 기준일: 2026-08-30. 전문 조사→개발 수용 기준 검토·결속→제작/통합→공간 화면 검증→개발→기획 순서.

## 1. 질문과 결론

플레이어가 나무 앞에서 도끼를 들고 접촉시킨 뒤 자세를 회복하며, 이동/피격 취소 후 곧바로 다른 행동을 할 수 있어야 한다. 한 스윙은 한 그루의 성공이 아니다.

**현재 실제 Actor는 Farm 남성이다. 기존 검술 HeavyCombo01A의 들기 부분과 별도 ReturnToIdle을 첫 보정 후보로 선택한다.** 실제 Humanoid Clip과 Farm Avatar에서 포즈 평가까지 확인했으나, 현재 검술 동작 그대로는 두 손 손잡이 결합·발 고정·나무 접촉을 충족하지 않는다. B/C보다 전체 발 이동이 작은 A를 우선 평가한다. 새 외형/도끼/구매 대신 기존 동작의 필요한 구간을 활용한다. 최종 제작물·Blender 왕복·Unity WI 연결은 아직 없다.

공통 재사용 원칙은 [Synty 재사용 설계](../UnitySyntyAnimationReuseAndRetargetDesign.md), 연구 양식은 [전문 연구 체계](../PlayableLoop전문심화연구분기재결속체계.md)를 따른다. 공통 설계의 2026-08-09 Clip/Controller 0개 표는 과거 재고이며 현재 추가 팩의 실제 내장 Clip 관측을 대체하지 않는다. `synty-asset-functional-modules.json`의 AnimationSourceNeedsReview 프로필도 재생 인증이 아니다.

## 2. 실제 배치와 원본 기준선

아래 Unity 상대 경로의 루트는 `C:/Users/user/ssalddel/`이다. 바이너리 저장 Scene을 텍스트 배선으로 추정하지 않고, 승인된 Editor 읽기로 현재 열린 객체를 확인했다.

- `LegalWorldFarmPlayer` → `VisualRoot_역할Character` → `SyntyRoleCharacterVisual_농부플레이어`.
- 대응 원본은 `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Male_01.prefab`. 플레이어와 배우의 lossyScale 모두 `(1,1,1)`.
- Animator Avatar: `Assets/Synty/PolygonFarm/Models/Characters.fbx`, `CharactersAvatar`, GUID `0125309e38bea8a48bc578cf8f220634`, fileID `9000000`, 실제 `isHuman=true/isValid=true`.
- 열린 Scene의 Animator는 `enabled=true`, `applyRootMotion=false`, controller 없음. 공용 Adapter는 `enabled=true`, `SourceKindCode=procedural-fallback`, intent `idle`. 이는 stopped 상태 관측이며 Play Mode 재생 결과가 아니다.
- Farm FBX importer는 Humanoid/Avatar 생성, `importAnimation:0`, `clipAnimations:[]`; 실제 가져온 Clip 객체도 0개다. Farm 외형 자체에서 벌목 Clip이 나오는 것이 아니다.
- `Assets/Ssalddel/Resources/Nature생존VisualCatalog.asset:18`의 axePrefab은 GUID `c037a3d7cf0b14449a2efc99e7a8f759`, fileID `936837361358885588`이다. 원본은 Generic `SM_Gen_Wep_Axe_01.prefab`.
- 도끼 Mesh 원본 `Assets/Synty/PolygonGeneric/Models/SM_Gen_Wep_Axe_01.fbx`, GUID `e5bb741599bcc324b80d0fa4450e6d38`, Prefab Mesh fileID `595298860125206120`. Mesh 로컬 bounds 중심 `(0.0722,0.1934,0.0003)`, 크기 `(0.2667,0.7370,0.0781)`m. 이 값은 손잡이/날의 접촉 좌표가 아니다.

### 원본 hash

아래 키는 후속 표에서도 사용한다. 각 hash는 이번 직접 파일 읽기값이다. 공급사 파일 변경 없음.

| 키·경로 | SHA256 | `.meta` SHA256 |
| --- | --- | --- |
| F-P `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Male_01.prefab` | `D176B368838CBED5024D89CB953B65FC8A0D3630E3FE11F214A0108B262E2343` | `6ED96896ADBEA6C5C7BB08DDAAAF5C89084D86CBE84DC91B5172DAE3C34E3266` |
| F-M `Assets/Synty/PolygonFarm/Models/Characters.fbx` | `EA7A567F52F593024854AD2947A64B7C4540985B2C71E93F79FF6574F2DB810F` | `195FC2247303913F44D3AC767683985288617E5FB15C3B4AEF51AE7A87A7625D` |
| X-P `Assets/Synty/PolygonGeneric/Prefabs/Weapons/SM_Gen_Wep_Axe_01.prefab` | `272EE343E4D5F39E51DF957E6F0B71B1C53DE652B4B5F662E7DA374674CBB8A1` | `EE9BBB3091CBDBBB36246433D7BFE45808C4309644CD7D7EE09302E2F7DCD1EA` |
| X-M `Assets/Synty/PolygonGeneric/Models/SM_Gen_Wep_Axe_01.fbx` | `762CD95751102EED6F9A3AB4FFA81E84452F07B701A62AEDFE970B3FD3F05CF9` | `BAFF258DF1DC588FAFEB1F884832F5CD1733FAF7E27B3685E1D0E903185421B5` |
| A `Assets/Synty/AnimationSwordCombat/Animations/Polygon/Attack/HeavyCombo01/A_Attack_HeavyCombo01A_Sword.fbx` | `58E5D031CB15D58965599BFD0545B3FE56E2E9743A4737C252D5D30325A6B32B` | `49842410E318C6F8F0671CA274C66E2D1CFF8E342A571BFA0640D35D66772ECD` |
| B 같은 폴더 `A_Attack_HeavyCombo01B_Sword.fbx` | `D5A410BE4263191BAF37DC990F6D4876DA0904CD6230E78F6CE4DF773360AF0B` | `1D1443E03F8F88BCBE8CCD47C969F34D98E6B81FA1507B62192DE188EBEC797F` |
| C 같은 폴더 `A_Attack_HeavyCombo01C_Sword.fbx` | `4B7EBCEA43DFE89EAF830EB161257F41F8B486C0C2424569434857C0BD1F9839` | `B8421E4802F296E8056FF5829598180010728ABA7AED1917CE004A6BD9D75DE2` |
| R 같은 폴더 `A_Attack_HeavyCombo01A_ReturnToIdle_Sword.fbx` | `BF3745902F8E39AEF42B8A0013184014CC5C6161D7B0FF63E91090F90F2DBCDA` | `DFA9C140379E5A0B81BF0D99F93493F736CF5262F4B743F1B11EC046DEFD3C92` |
| I `Assets/Synty/AnimationBaseLocomotion/Animations/Polygon/Masculine/Idle/A_Idle_Standing_Masc.fbx` | `76DC31C92FD3CA69F28E92F1F530EAC6F60EE71EA0F25D490A633F49D2088DD0` | `28B56AAE249214D9D69E94E1C6E97DA77EF269E2E3C964B5763637F25FD94BF9` |

F-P GUID `3c3a8236be548bb4c892cf39a5abadfe`; A `f3800f1bd189abc469fcc7c374371a91`; B `94770028b07a9ce44b555d283c113a86`; C `2511e6dc0955bde4395e6b369d7d8707`; R `20aa50f251da63047ab4ac1045839566`; I `a4b8f2deec6a99945987cfc59b1b4e54`.

## 3. 실제 내장 Clip과 격리 평가

`AssetDatabase.LoadAllAssetsAtPath`의 AnimationClip 객체를 읽었다. `__preview__`는 후보에서 제외했다. 큰 fileID는 JSON 숫자 반올림을 피하려고 문자열로 다시 조회했다. 아래 Clip은 모두 30fps, `humanMotion=true`이다.

| 키/Clip | fileID | 길이 s | 곡선 수 | 판정 |
| --- | --- | ---: | ---: | --- |
| A `A_Attack_HeavyCombo01A_Sword` | `-8709411803423658444` | 2.03333354 | 196 | Farm 격리 평가됨; 보정 후보 |
| A `_WindUp_Sword` | `9216853843792190869` | 0.966666639 | 196 | 같은 FBX의 준비 구간 실제 존재 |
| A `_Hit_Sword` | `114646337594296348` | 0.200000048 | 196 | 같은 FBX 접촉 후보 구간; 나무 접촉 인증 아님 |
| A `_FollowThrough_Sword` | `6240718372927078310` | 0.8666668 | 196 | 같은 FBX 후속 구간 실제 존재 |
| B `A_Attack_HeavyCombo01B_Sword` | `7713716175428577925` | 1.666667 | 196 | 평가됨; A보다 큰 발 이동으로 후순위 |
| C `A_Attack_HeavyCombo01C_Sword` | `-2621343224683262629` | 1.60000014 | 196 | 평가됨; 연속 검술 보류 |
| R `A_Attack_HeavyCombo01A_ReturnToIdle_Sword` | `-7354800201745137062` | 0.933333635 | 196 | 실제 Clip 존재; A와 복귀 연결은 미검증 |
| I `A_Idle_Standing_Masc` | `1827226128182048838` | 1.76666677 | 130 | loop=true; 손잡이를 쥔 Idle 적합성 별도 |

A/R은 loop=false, root curves 있음, AnimationEvent 0. 따라서 파일명에 RootMotion이 없다는 이유로 루트/발 고정을 보증하지 않는다. A importer의 원본 구간은 frame 1~62, R은 62~90이다.

실측 방법: 새 소유 PreviewScene에 F-P만 복제하고 Animator root motion/foot IK/playable IK를 끈다. 수동 PlayableGraph로 A/B/C 각각 `t=length*i/30 (i=0..30)` 31지점 평가. 배우 로컬 좌표의 Hand/Foot 위치를 읽고 Graph·소유 객체·PreviewScene을 finally 정리했다. 스킨 시각 확인·연속 렌더링·Runner 시험·Play Mode는 아니다.

| 후보 | 왼발 최대 변위 m | 오른발 최대 변위 m | 주요 관측 |
| --- | ---: | ---: | --- |
| A | 0.3396995 | 0.232391238 | 0.678s에서 손 높이 약 1.645/1.502m. 표본 양손 간격 0.135~0.899m로 고정 그립 아님 |
| B | 0.762517035 | 0.7035228 | 앞 콤보 종료 자세로 시작, A보다 큰 발 이동 |
| C | 0.9900433 | 0.525689244 | 연속 공격의 자세 변화가 크고 벌목 기본복귀가 아님 |

변위는 **각 Clip 첫 Foot 위치 대비 전체 구간 거리**다. 접지 구간을 나누지 않았으므로 이것을 발 미끄러짐 수치로 부르지 않는다. 세 평가 모두 객체 루트 위치는 `(0,0,0)`이나 발은 움직였다. Farm Avatar에서 실제 뼈 변화가 있었다는 좁은 증거이며 손-도끼/나무 접촉과 품질 통과는 아니다.

## 4. 기존 연결과 부족점

`Assets/Ssalddel/Presentation/World/` 아래:

- `Nature감각표현Presenter.cs:327`은 카탈로그의 도끼 Mesh를 1인칭 카메라와 3인칭 RightHand에 복제한다. 오른손 오프셋 `(0.03,0.04,0.02)`, 회전 `(15,0,90)`; 왼손 그립 제약 없음. 1인칭 `(0.32,-0.35,0.7)`, 회전 `(8,-20,-35)`는 별도 도구 표현이므로 반드시 회귀 대상.
- 같은 파일 `:373`은 Working 중 `Repeat((unscaledTime-start)*1.55,1)`로 도구/양팔/척추를 쓴다. `Nature감각표현Models.cs:45`의 접촉 Audio/FX는 `CompletedSeconds` 증가로 생성되어 현재 스윙과 시계가 다르다.
- `공용AnimationAdapter.cs:139`/`:164`의 전신 절차형 LateUpdate도 같은 팔/척추/다리를 쓴다. `Configure` fallback은 controller를 비우고, `PrepareAnimatorOwnership`은 조건 충족 시 Animator를 끈다. 현재 stopped의 enabled=true 관측과 코드의 실행 시 경로를 구별한다.
- `플레이어경관Controller.cs:506`의 CharacterController.Move와 `:519`/`:525`의 ApplyLocomotion은 기존 이동 소유다. 새 Adapter는 루트 위치를 쓰지 않는다.
- `Nature감각표현Models.cs:133`의 CueStableId에는 revision이 포함된다. 단순 revision 증가를 새 타격으로 해석하지 않는다. WorkState의 Kind/Target/RequiredSeconds/CompletedSeconds에는 별도 TaskID가 없다는 개발 확인을 따른다.
- `Ssalddel.WorkflowRules/UnityPackage/Runtime/NatureSurvivalRules.cs:18`의 4초·목재2 유지. Domain `SimulationNatureSurvival.cs:730/:914/:934`의 시작/진행/완료와 `:812` 취소를 표현에서 호출하지 않는다.

연결 원본 hash: 공용 Adapter `CCFDE217940F3493E907A469123540A2423DE36C7F7EA2C1C88153FB4AA198BF`, Presenter `F08A8E2753CB671D8745E7373313AFD59E89E959C4EEAB2DBD1EEDE90F07C404`, Models `413E473C6783AFE7E5A230537609E73DF04875299B991DF9C07B726255A52C62`, VisualCatalog `E9E3E012D519877427AF5D2AFE76A362B4CA80D005BB2D0A7E67A43C1738870A`.

## 5. 선택 기준과 기술 조정안

| 대안 | 장단점 | 선택 |
| --- | --- | --- |
| 기존 절차형 그대로 | 연결은 있으나 그립/타격 위상/뼈 작성 경쟁 미해결 | 실패 시 명시적 fallback만 |
| A 준비/접촉 + R 복귀 + 정지 하체/그립 보정 | 기존 구매 곡선 활용, 발 이동 억제와 그립 보정 필요 | 우선 기준선 |
| B/C 또는 점프/찌르기 | 벌목과 무관한 이동/연속 전투 자세가 큼 | 첫 제작 제외 |
| 전신/외형 신규 생성 | 승인 범위와 비용/약관 위험 확대 | 제외 |

제작 조정 시작값: 30fps, 1회 30프레임(약1초), 준비0~0.35·내리치기0.35~0.55·접촉0.55~0.65·복귀0.65~1.0. 기존 4초 Task에서 정상 진행 시 최대4회 표현하는 시작안이다. A 전체를 단순 2배속하는 확정안이 아니라 준비/접촉/R의 필요한 부분을 축약하는 보정 입력이다. 최종 프레임·속도·위상은 평가 결과와 개발 시간 소비 계약에 맞춰 기록하며 숫자별 사용자 재승인은 요구하지 않는다.

권위 동기 제안:

1. 개발이 Session/Actor/Target + 로컬 관측 작업 세대(재시작/Load 식별)를 제공한다. 동일 revision 재조회는 재시작하지 않는다. 같은 Target 재작업은 새 세대로 구별한다. 전문 Adapter가 영속 TaskID를 만들지 않는다.
2. `CompletedSeconds` 변화와 완료/취소만 권위 진행이다. 보간 시간은 포즈에만 쓰며 다음 미확인 완료 초를 생성하지 않는다. 진행 토큰이 없으면 준비 말미까지만 접근하고 실제 접촉 Cue는 보류한다.
3. 개발이 확인한 진행 토큰을 해당 스윙 접촉 Window에서 한 번 소비한다. Cue 중복 키는 세대+완료초 기반으로 유지하고 revision만 바뀐 재조회는 새 타격이 아니다. 여러 초가 건너뛰면 지난 스윙/소리를 몰아 재생하지 않고 최신 상태로 합류한다.
4. 마지막 ActiveWork가 사라질 때 실제 Stump 완료인지 취소인지 개발이 구분해 전달한다. 완료 때 남은 접촉/나무넘어짐 표시 순서는 개발이 한 번만 배치하며 보상을 다시 호출하지 않는다. 취소는 미소비 접촉을 폐기하고 0.15초 안쪽 블렌드 시작값으로 기존 이동/Idle에 반환한다.
5. Load/재구성은 과거 타격을 재생하지 않는 baseline 동작이다. 진행값이 감소하거나 Session/세대가 달라지면 보간·대기 Cue를 비우고 현재 상태에서 시작한다. 일시정지/네트워크 지연 중 unscaledTime으로 반복 타격하지 않는다.

이 프로토콜의 시간 지연 체감과 마지막 접촉 처리는 아직 통합 미검증이며 개발이 모델/Presenter에 결속한다. 작업 완료를 애니메이션 종료까지 늦추지 않는다. 취소/보상/저장 규칙 변경 없음.

단일 작성자: 작업 중 전용 Animator 평가(필요 시 같은 평가 사슬 안의 IK)가 전신/상체 최종 자세를 소유하고 기존 공용/Presenter의 해당 뼈 쓰기를 개발이 배제한다. 하체는 발 고정 기준 자세를 유지한다. 종료 시 소유를 원래 이동 표현에 넘긴다. Animator와 독립 LateUpdate IK를 동시에 추가하는 방식은 선택하지 않는다. root motion=false 유지.

제작 검증 목표값(관측값 아님): 고정 그립 구간 손-손잡이 목표 이탈 ≤2cm, 날-나무 표면 접촉 오차 ≤3cm, 의도적 디딤을 제외한 접지발 수평 이동 ≤2cm. 관절 늘어남/도구 관통/비정상 비틀림은 수치 외 정면·측면·기존 시점 연속 화면으로 판정한다. 실제 그립점·나무 접촉점 좌표는 bounds로 추정하지 않고 소유 제작 복제에서 측정한다. 기존 ActorWork anchor의 나무 뒤 1.5m를 실제 도끼 도달 거리로 보증하지 않는다. 강제 이동/새 카메라 전환 없이 개발의 기존 상호작용 범위와 일치시킨다.

### 5.1 이번 Accepted 검토 요청 범위: 기존 Clip 연결만

개발의 2026-08-30 후속 검토에 따라 **아래 A범위만 Accepted**다. 앞 절의 그립·발고정·Blender 보정 목표는 B범위이며 함께 승인된 것으로 해석하지 않는다.

| 범위 | 허용 작업 | 완료 표현 |
| --- | --- | --- |
| A — 기존 imported Clip 소비 | A의 기존 WindUp/Hit/FollowThrough, R, I를 읽기 참조. 재생 구간/속도·블렌드 가중치·중단 상태를 프로젝트 Adapter에서 제어. 공급사 FBX/meta와 Clip 곡선/이벤트는 불변 | 기존 Clip 재생/상태 소비 준비. 실제 벌목 접촉 미통과 |
| B — 실제 벌목 자세 보정 | 새 키프레임, bake된 Clip/FBX, 공급사 뼈/형상의 자동 수정, 그립/발 고정용 신규 자세 생성 | 구매조건/방식 확인과 별도 개발 배분 전 미착수 |

A의 블렌딩은 Unity가 기존 곡선을 평가하는 표현 동작이다. `AnimationUtility.SetEditorCurve`, 새 AnimationClip 곡선 작성, AnimationEvent 삽입, FBX 재저장, AI 기반 Pose 생성은 사용하지 않는다. A에서는 `.blend`/새 `.fbx`를 만들지 않는다. Blender 산출물이 없다는 사실을 인계에 적고 D359 전체 제작 완료로 보고하지 않는다. 1초는 조정 가능한 재생 목표값일 뿐 원본 Clip을 30프레임으로 재작성하지 않는다. 빠른 재생이 부자연스러우면 원본 구간 가중/속도만 기술적으로 조정하고 접촉 품질 미통과를 유지한다.

독립 전달물 제안(실제 생성은 개발 Accepted/hash 결속 후):

- `Assets/Ssalddel/Presentation/NatureWoodcutting/`에 전용 상태 소비 모델·Adapter. 외부에서 실제 대상 Animator와 위 고정 Clip 참조를 공급받는다. 기존 Scene 자동검색/자동부착·Bootstrap·Shared Adapter 수정 없음.
- 입력 계약 후보: 관측 작업 세대/Session/Actor/Target, 관측 revision, RequiredSeconds/CompletedSeconds, Working/Completed/Cancelled, baseline/Load 구분, 개발이 부여한 진행 토큰, 복귀 의도 Idle/Walk. Adapter가 authoritative 세대/TaskID를 만들어 저장하지 않는다.
- API 후보: `ConfigureSources`(GUID/fileID 및 런타임 참조 확인), `ApplySnapshot`(동일/역행 revision·세대 구분), `TickPresentation`(표현 시간만), `InterruptToIdle` 또는 개발이 준 복귀 의도, `Reconstruct`(과거 Cue 억제), `ReadState`(현재 구간/관측revision/진행토큰/진단 읽기), 해제 시 소유 Graph만 정리. 공개 명칭은 코드 명명 지침에 맞춰 구현 시 확정한다.
- 출력은 현재 구간·정규화 시각·미소비/소비 토큰·단 한 번의 접촉 가능 알림뿐이다. 보상/WorldTick/Save/나무 변경 API를 보유하지 않는다. 접촉 알림은 타격 위치 검증을 대신하지 않으며 Audio/FX 재생 여부와 완료 표시 순서는 개발이 연결한다.
- 이동 Transform writer 없음. Graph/Animator는 개발이 기존 뼈 writer를 배제하고 소유를 넘긴 경우에만 활성화한다. 소유권 인계 없이 동시 활성화하지 않는다. 종료/비활성/재구성에서 이전 참조를 훼손하거나 다른 담당 Graph를 정리하지 않는다.
- 개발 추가 결속: 공용 Adapter.OnEnable이 현재 포즈를 기준자세로 다시 수집하고 fallback Animator를 비활성화하므로 `enabled=false/true` 왕복으로 소유를 전환하지 않는다. 개발의 명시 lease가 원래 기준자세/이동 의도를 보존하며 fallback 뼈쓰기만 중지한다. 전문 `Configure`는 Graph를 활성화하지 않고 명시 bool/token 인계의 `Acquire/Release`만으로 소유 Graph를 활성/해제한다. 객체 교체/Disable/Dispose는 자신 Graph만 해제하며 다른 Controller나 Animator 상태를 임의 초기화하지 않는다. 공용 lease 구현은 개발 소유다.
- `Assets/Ssalddel/Editor/NatureWoodcuttingValidation/`에 전용 검증. source GUID/fileID/hash drift, 누락/잘못된 Clip, 같은 revision 반복, 오래된 revision, 진행 건너뜀, 세대/Target 변경, Load baseline, Working→Cancelled/Completed, disable/dispose를 집중 시험한다. 접촉 토큰은 중복 발행하지 않으며 소스 불일치는 진단과 안전한 미재생으로 반환한다.

검증 층을 분리한다: 순수 상태 소비 시험 → 소유 격리 Animator 평가(기존 Clip 불변/루트미작성/Graph 해제) → 개발의 Shared writer 및 실제 Task 연결 → 공간의 연속 View. 앞 두 층만 성공해도 최종 두 손 접촉·발 고정·실제 벌목 품질은 미통과다. 실제 코드 소유 경로는 기존 7절/명세와 같고, 공용 파일은 개발 단독 소유다.

## 6. 라이선스와 제작 접근

D359 사용자가 기존 구매 사실을 확인했다. F-P meta에는 Store/productId146192/Farm1.7.2, X-P에는 Construction productId168036/1.5.2 출처가 있다. 이는 패키지 유래이지 구매 영수증/계약 판본 증명이 아니다. 선택 Sword FBX meta에는 구매 채널 증빙이 없고 로컬 Synty 파일명 검색에서 EULA/License 문서를 찾지 못했다.

[Synty FAQ](https://syntystore.com/community/faq)는 프로젝트용 일반 3D 편집을 안내한다. [Unity Asset Store EULA](https://unity.com/legal/as-terms)의 2.2.1(e)는 허용 제품에 관련된 수정, 2.2.1.1(g)는 AI/ML 이용 제한을 별도로 규정한다. [Synty One-Time EULA](https://syntystore.com/pages/one-time-purchase-licence)는 일반 수정과 생성형 AI 3D 모델 생성/외부 생성 서비스 업로드 제한을 구별하며 Unity Asset Store를 적용 Store에서 제외한다. 채널·구매 당시 판본·추가 약관은 미확정이다.

따라서 **기존 Clip 로컬 재생/평가·프로젝트 내 통상 리타깃/상태 연결 조사**를 계속한다. 로컬 수동 Blender 편집은 적용 구매 조건 확인 아래 준비한다. 반면 AI가 공급사 뼈/형상을 입력으로 새 키프레임·모델을 자동 생성/보정하는 방식의 허용은 이번 구매 확인만으로 확정하지 않으며 그 방식만 별도 반환한다. 외부 AI 업로드·새 구매·원본 덮어쓰기 없음. 현재 수행한 것은 기존 Clip의 메모리 평가이며 신규 자산 가공/학습이 아니다.

## 7. 소유 경로·시험·무효화

개발 제안 예약 경로는 시작 시 모두 부재/충돌 없음. 이번 작성은 이 연구 문서뿐이며 다음 경로는 Accepted 결속 뒤 사용한다.

- 전문: `ArtSource/Blender/source/NatureWoodcuttingD359/`, `exports/NatureWoodcuttingD359/`, `validation/NatureWoodcuttingD359/`.
- 전문 전달: `Assets/Ssalddel/Presentation/NatureWoodcutting/`와 meta, `Assets/Ssalddel/Editor/NatureWoodcuttingValidation/`와 meta. Unity 실제 반입은 개발과 조율한다.
- 전문 인계: `docs/Reports/Nature벌목동작-D359-제작인계-2026-08-30.md` — 아직 작성/제작 완료 없음.
- 개발: 공용 Adapter/기존 Nature Presenter/Bootstrap·원장·Scene 연결. 전문이 수정하지 않는다.
- 공간: 실제 입력/접촉/중단 연속 Game View·보조 시점과 Console. 정지 PNG 한 장으로 동작 통과시키지 않는다.

후속 시험: `.blend` 저장/재열기→FBX 뼈/단위/Clip 구간 왕복→실제 Farm Avatar의 도끼 접촉/발 고정→Task 시작·동일revision·진행 건너뜀·완료·이동/피격취소·재작업·Load/해제→동일 권위 revision/보상/시간 불변→공간의 연속 화면. 기존 D358 도형 시험/D286 직접호출 시험은 이 동작의 성공 증거가 아니다.

Editor 슬롯은 개발 승인·공간 무점유 확인 후 사용했다. 직전/직후 stopped, compilefalse, canonical `SimulationWorldShell` dirtyfalse/root2, selection[], 기존 previewCount1 유지. 저장 Scene SHA256 `B270E1CC1DDAAE219D56D3F2D15B127A05743718DC0903D93D833DB17FF1F849` 전후 일치. 소유 Preview/Graph/객체만 정리했고 잔여 `D359_OwnedPoseProbe` 0개, 슬롯 해제를 개발/공간에 통보했다. 기존 dirtytrue 이력을 이번 현재값으로 재사용하지 않는다. Scene 저장·재import·Play Mode·View 캡처·Packages 변경 없음.

조회식 2건은 폐기 API/using 문법 때문에 실행 전 컴파일 진단으로 실패했고 수정식은 성공했다. 프로젝트 코드 컴파일/Runner 실패로 혼동하지 않는다. 자동시험·Blender 제작·실제 WI 연결은 아직 미실시.

무효화: Actor/원본/Clip/meta/공용 쓰기 경로 hash 변경, Avatar invalid, 실제 접촉 불가, 이중 뼈 작성 재발, 기존 Task/취소/Save 의미 변경. 기술 수치 보정은 승인 안에서 재기록하며 플레이 의미 변경만 기획에 반환한다. 개발이 수용한 A 기준선의 최종 hash 결속을 기다리며 담당이 독자 범위 확장/E 승격하지 않는다.
