# D386 최소 모듈·D387 조사 책임·D388 첫 자산 조사 — 기술 통합

## 결과

D386의 준비 코드·관리 연결·근거/생성 상태판 정합성을 개발이 인수했다. **실제 Farm UI·권위 조회·Preview/Confirm·Scene은 E5 미연결**이며 E 승격은 없다. 전체 개발시스템 검사는 과거 명세 판본 문제로 차단되므로 전체 관리 체계 완료로 보고하지 않는다. r129/야간/다른 전문 작업은 계속 중지다.

승인 [Presentation 단계별 최소 모듈](../AI/Presentation단계별최소모듈-2026-08-31.md)의 SHA256 `602306949E897AFBD59B066A9547A76D611A8CD7001476B7564DC294FB1CEF53`을 재확인했다. 기존 [Farm 소비 구현보고](Presentation최소모듈-Farm소비-2026-08-31.md)는 근거 묶음이 참조하는 당시 기록이므로 SHA `FCA73E01DE576A78C8C39C9E381313C78C8892B501447ADF3A41D4D79E833213` 그대로 보존한다. 그 보고의 생성 지도 불일치/기획 통합 대기는 아래 후속 검증 결과로 대체되며, 최초 실패 기록 자체는 유지한다.

## 인수 범위

- 기존 `StableIdReconciler` 최초/삭제 항목의 누락 표현 판본 거부와 첫 Farm `Farm수확상태PresentationPreparation` 소비자. 새 범용 프레임워크·상태 권위·게임 규칙을 만들지 않았다.
- 표현 대장 v2/r9를 인수하고 D387 안내를 r10에 반영했다. E1~E7 18모듈, 공통8/조건10은 유지한다. `outputs/implementationRefs/testRefs`는 책임·경로이지 실제 동작 성공 표시가 아니다.
- `presentation-module-bindings.ps1`와 표현/E7 관리자: 기존 대장 v1·확장 없는 명세 읽기 호환, Passed의 후보/단계/폐루프/WI/파일 hash 검증, E5 이상 Logic E5, E7 Play+GameView 근거 요구. 합성 시험 자료를 실제 근거로 등록하지 않는다.
- EvidencePackage `subjectRefs`는 폐루프를 유지하고 선택적 `presentationModuleScope`로 모듈/WI를 좁힌다. D386 준비 묶음의 12파일 hash가 현재 디스크와 모두 일치한다.
- D386 인수 당시 Farm SHA `9A00CBDBB9839D9AE8F8FD64991D18EA1B94FF6485401701B46085F69C717B2C`와 Goal r130의5참조가 일치했다. D387 안내 후 Farm SHA `88E0CF3B83431CAFED26A8C7912C5364347D7873CEC3BAFCD779749F35E1793B`를 Goal r131의 같은5참조에 동기화했다. WI-FARM-04 한 재배 준비 지원은 기존 활성 WI-FARM-01을 대체하지 않는다. Logic E3/Presentation E1/통합 E1·상한 E5·승격false 유지.
- Farm 모듈은 Unverified/Blocked다. 부분 E2/E3 근거가 전체 객체수명·자산·렌더 시험을 대신하지 않으며 E5~E7은 Blocked다.

## 검증과 한계

| 검증 | 확인 결과 |
| --- | --- |
| 기획 인계 Task `20260831-073511` | TRX와 빌드 로그를 개발이 직접 읽고 결속 hash 대조. Unity.slnx 빌드 경고0/오류0, 독립 .NET **587/587**, 실패/건너뜀0. 집중50을 포함하며 합산하지 않는다. |
| 개발 재실행 관리 회귀 | **42/42 사례 통과**, 합성 근거 전용. `presentation-minimum-integration-d386/development-module-bindings.log`. |
| 개발 scoped Fast `20260831-074631` | D386 관련16경로, 코드/E 책임 지도 check·두 프로젝트 빌드(각 경고0/오류0)·집중 **50/50**, 실패/건너뜀0 통과. |
| D387 후속 scoped Fast `20260831-074943` | D387 기획/DECISIONS 포함14경로, 지도 check·두 프로젝트 빌드·집중50 통과. 구조 회귀24/관리42 재실행도 각각 통과(`d387-survey.log`/`d387-module-bindings.log`). 새 자산 실행 없음. |
| Farm/표현/Goal 관리자 | 실제 Unity 경로를 명시한 파일 참조 검사, Farm 명세 검사, 생성 표현 상태판 Validate와 Goal Check 통과. Editor 실행은 아니다. |
| E 책임 지도 | 인계 Task 및 개발 Fast의 check 성공 확인. 생성문서의 767/764/제외3/미분류0을 직접 읽었으며 이전 변경까지 포함한다. 지도 diff 전체를 D386 성과로 세지 않는다. |
| 전체 E7 suite | 개발 재실행에서 등록 명세 순회 중 `E7VerticalWorkOrderInvalid:ProtocolRevisionInvalid`로 실패. 이전 `legacy-work-order-suite.log`는 0바이트라 실행 성공 근거로 사용하지 않았다. |
| 전체 개발시스템 Validate | 같은 오류로 중단. v1/v2 schema 수용 보완 뒤 남은 별개 판본 차단이다. Write/전체 생성 완료 주장 없음. |

과거 판본 차단을 직접 확인한 두 파일은 다음과 같다. 현재 프로토콜 r5에 비해 둘은 r4이며 이번에 변경하지 않았다.

| 파일 | 보존 SHA256 |
| --- | --- |
| `eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json` | `E44926D17843859169671A154A0BAF6AF15CD2B244AD6D66C752DDCA90D2B2BE` |
| `eng/execution-ledgers/work-orders/nature-tactical-self-navigation.e7-work-order.json` | `43BD2F0BD59A370A9191615D745C38E445C16E1AB3E367ABFFD0EAF21EE64420` |

과거 승인 자료/해시 자동 변경이나 protocol 검사 완화는 하지 않았다. 이 두 명세의 후속 이관은 해당 승인·연구 결속을 별도로 검토해야 한다. 전체 순회가 중단되므로 다른 모든 명세까지 정상이라는 주장도 하지 않는다.

## D387 추가 기획 결속

[단계별 Synty 자산 조사](../AI/Presentation단계별Synty자산조사-2026-08-31.md), `presentation-synty-survey.design.r1`, SHA `F42ABB24C5C3EBD0B6C2E302F593C5E73B46A48987AC6FEDF7E63B0E5A4CD9F7` 및 D387을 전체 읽었다. 기존 [Synty 표현 모듈 체계](../Architecture/플레이폐루프Synty표현모듈체계.md)와 대장을 그대로 참조한다.

- E2 `presentation-projection-lifecycle`: 기존 자산/동작 목록·Adapter의 가벼운 파일 조회와 잠정 후보. 전체 팩 실측을 코드 작성 선행조건으로 만들지 않는다.
- E4 `presentation-binding`: 기존 `presentationE4Preparation`에 필요한 후보 적합성·근거·판본과 **그대로 재사용 / 연결·설정 보완 / 가공 필요 / 신규 제작 필요 / 미검사**를 명시하는 책임. 동일 자산·판본·문맥 근거 재사용/변경분 확인, 파일·격리·실제 대상 증거 구분, 비자산 NotApplicable를 안내한다.
- E5 `visual-source-bounds`: 실제 상태·World·Prefab/대체 표현 결속과 Renderer/Collider/Bounds 증거. E4 조사나 파일 존재는 E5 성공이 아니다. 기존 E6~E10 의미는 바꾸지 않았다.
- 기존 대장의 입력/출력·참조와 템플릿/Farm의 Presentation E2/E4 요약·열린 결함만 보완했다. 새 자산 대장·모듈·필수 schema 필드·게임 코드/API·조사 결과를 만들지 않았다. 가공/신규 제작 필요는 승인이나 실행이 아니다.
- 새 `eng/tests/presentation-synty-survey.ps1` **24사례 통과**는 문서·대장·명세·생성 안내의 구조 회귀다. 실제 자산 경로/GUID/기하/애니메이션 적합성 검증이 아니며 기존42 관리/587 .NET 시험과 구분한다. 표현/Goal 생성물은 기존 관리자로 갱신했다.
- 기획 원문 상대링크2/2, DECISIONS의 로컬 링크220/220, 이 보고 링크6/6 존재를 확인했다. D386/D387 두 원문 SHA와 D386 근거12파일 hash는 최종 재대조에서도 일치한다. Goal 변경은 r130→r131 및 Farm hash5치환 외 전체 문자열 동일을 확인했다.

## 변경 소유와 남은 작업

최종 D386 인수에서는 추가 실행 코드 수정을 하지 않았고, 이어 받은 D387 범위에서 위 기술 안내·시험·Farm hash/Goal 동기화·생성물을 변경했다. [CURRENT_WORK](../AI/CURRENT_WORK.md)와 [개발 통합 상태판](../AI/개발통합상태판.md)의 최상단에 D386/D387과 사용자 중지를 명시하고, 과거 진행 문구를 실행 권한 없는 이력으로 구분했다. 기존 근거로 결속된 Farm 보고/12파일/두 승인 원문은 보존한다.

실제 Farm 입력 포트·상태 사본 공급·UI/Preview/명시 Confirm/결과 재조회와 Scene의 9밭 누락 컴포넌트·표시·시점은 해결하지 않았다. 준비 입력 실패를 실제 성공으로 바꾸거나 Session/권위 상태를 주입하지 않았다. .NET 시험은 Unity Editor/API·실제 화면 검증을 대신하지 않는다.

r129는 공간 반환에 따라 loader 전 Compile0/Run0/Load0/Render0, `slot-r129/disk-preflight.json`만 보존한 중단점이다. 마지막 직접 Editor 관측은 04:58:51경이며 이번에 새로 조회하지 않았다. Scene/Save/Packages/Assets 가공·Play·캡처·야간/다른 전문/자동화 재개·commit/push 없음.

## D388 첫 파일 조사 묶음 — 조사 완료와 제품 적용 분리

[Logic 선행 기능의 표현 준비 균형 계획](../AI/Logic선행기능-Presentation균형계획-2026-08-31.md), `logic-presentation-asset-balance.design.r1`, SHA `BD8532C58EE625B8563AAAD939450A80CB27BB003C7A691FA6160E831FE5C463` 및 D388을 전체 읽고 적용했다. **Farm 한 재배 상태의4후보 조사 + 약초 용기/방문자 Actor의 기존 근거 재사용·연결 공백 확인**을 완료했다. 기존 세 작업 명세의 `presentationE4Preparation.assetSurvey`에 구체 신원과 결과를 추가했으며 새 권위 대장·E축·자동 상태 주입·E 승격은 없다. 이는 선택적 준비 기록이지 새 관리 필수 schema나 실제 Prefab 소비 구현이 아니다.

### 기능·증거 범위 비교

2026-08-31의 [폐루프 원장](../../eng/execution-ledgers/playable-loops.json), [WI 원장](../../eng/execution-ledgers/world-interactions.json), [증거 묶음](../../eng/execution-ledgers/evidence-packages.json), 각 명세/소스를 대조했다. 아래 시험 수는 **이전 묶음에 기록된 범위**이며 이번 재실행 결과가 아니다.

| Loop / WI | 현재 Logic / Presentation / 통합 | 실제 근거 범위와 이번 준비 |
| --- | --- | --- |
| `farm-crop-cycle.v1` / WI-FARM-04(활성WI01 유지) | E3 / E1 / E1 | `evidence:simulation-task-20260824`의 worktree:2026-08-24-simulation-task, 전체764시험 기록은 실제 입력/화면/Hosted를 제외. WI04에는 별도 E3 구현과 과거 E6 질문·배치 닫힘 기록이 있으나 현재 Loop E6가 아니다. D386 준비 근거는 E2/E3 한 재배·판본 거부 범위. 이번은 정확4후보+연결 공백 조사. |
| `nature-basic-herbal-recovery.v1` / WI-ACTOR-03, 지원 HB01 | E4 / E4 / E4(부모 지식 범위) | `evidence:nature-basic-herbal-recovery-logic-e4-20260828`, design.r2/worktree-20260828: Query/Preview/Confirm·알려진 처방·Local/Remote 동등성. P4는 처방 기록·카드 준비. **채집·달이기·섭취/약효·Save/Scene은 제외**. HB01 순수 내용물44시험을 부모 전체 완주로 승격하지 않음. |
| `nature-camp-visitor-stay.v1` / WI-COMMUNITY-VISITOR-STAY | E4 / E4 / E4, E5Pending | design.r3, 기존 E3/E4 묶음·Accepted E5 연구 r2. 독립 원장·준비 투영 및 격리 포즈가 있으나 실제 Session/Actor 권한·원자적 SessionRevision·Save/배치/입력은 별개. 이번은 남성Actor/Avatar/3Clip/Controller 재사용 확인. |
| `nature-field-supply-return.v1` / WI-NATURE-06/11/12/13/16/17 | E4 / E1 / E1 | `evidence:nature-field-supply-core-20260825`, nature-play-flow-cycle-r2: 직접/위임 제작·취소·꾸러미·Save v20/v21 기록. 실제 작업대 입력/화면·Hosted 제외. **이번 후보 조사 미실시**, 후속 목록 유지. |
| `town-order-consume-return.v1` / WI-ORDER-01~07 | E4 / E1 / E1 | `evidence:town-arcana-core-20260825`, town-arcana-npc-life-r1: 선택·경쟁·소비·다음 욕구/Save v16 집중8시험. ORDER전체 E5·Town 이동/실제 소비·Local/Remote 제외. **이번 후보 조사 미실시**. |

Logic가 E3 이상인 다른 PlayableUnit도 목록에서 제외하지 않는다. 이번 세 조사 외 다음 항목은 **원장 단계 조회만** 했고 개별 자산/전체 근거 재검증은 하지 않았다: `nature-regional-threat-recovery` E3/P1, `nature-base-reflection` E3/P1, `town-arcana-context` E4/P1, `farm-pack-store-return` E3/P1, `farm-player-placement` E3/P1, `hub-outbound-ready-return` E3/P1, `farm-barracks-defense` E3/P3. 더 높은 Logic의 `nature-building-learning` E5/P1·`nature-workbench-foundation` E7/P6도 후속 공백 대상으로 유지한다. `nature-tactical-self-navigation`·`nature-shelter-foundation`·`nature-twilight-return` E7/P7 역시 과거 등록값이지 모든 후보가 현재 적합하다는 새 인증이 아니다. 전수 문답/팩 조사·전체 기능 준비 완료를 주장하지 않는다.

### Farm — 한 밭 수확 상태의 첫 완결 조사

기준은 [승인 기획 r1](../Architecture/PlayableLoops/Farm경작세계발현E5.md)(SHA FF9D86E6…477FFC), [Accepted 공간 연구 r1](../Architecture/PlayableLoops/Farm경작세계발현E5-공간배치연구.r1.md)(SHA EADA84A0…57711B0) 및 기존 r2다. r1이 허용한 **정적 Actor/진행 UI/결과 표현 대체**를 사용 준비에 재사용하며, 이번 첫 상태 비교에는 Clip/Rig가 필요하지 않다. 농작업 접촉·이동·도구 그립·새 Clip은 미검증이다.

[Farm 명세](../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json)에 아래 후보의 전체 Prefab/meta SHA·GUID·과거 의존 fingerprint를 기록했다. Unity 자산 경로 기준은 `C:/Users/user/ssalddel/`이며 식물은 `Assets/Synty/PolygonFarm/Prefabs/Plants/`, 밭은 `.../Prefabs/Environments/`다.

| 플레이어 판독 순간 / 기존 Slot | 정확 후보 | GUID | 과거 native Renderer 크기 x/y/z / 판정 |
| --- | --- | --- | --- |
| 밭 기반(수확한 동일 밭을 잃지 않음), farm.crop.prepare는 기존 준비 Slot | SM_Env_Dirt_Rows_01.prefab | c738b62174ab09f4eb2d7b486398a6ed | 4.990549/.198393449/5.06839371. 비중심 pivot, 연결·설정 보완. 25.294㎡ 외곽은100㎡ 생산 면적 아님. |
| Growing / farm.crop.grow + 성장 라벨 | SM_Prop_Plant_Potato_01_S.prefab | 53e5ab917382c9749a58810d6e170537 | .186849356/.3714474/.262023926, minY -.0622976124, Collider0. 정적 성장 후보, 자동 성장률/수량 추론 없음. |
| HarvestReady / farm.crop.grow + 수확 가능 라벨 | SM_Prop_Plant_Potato_01_L.prefab | e48b8d820d122d64484926ce5e8f6e8c | 1.1754415/1.00500667/1.12609553, minY -.02854991, Collider0. 준비 상태와 식물 크기의 표시 결속 필요. |
| Harvested + HarvestedAtField Lot / farm.crop.harvest | SM_Prop_Box_Potato_01.prefab | 2131bc3845099584ebe0cb30614e96f4 | .755736053/.385134041/.452614427, Collider1. 밭의 수확 Lot 표상이지 Packed/Collected 완료가 아님. |

측정은 [기존 실측 보고](FarmH2-실측준비-2026-08-30.md)와 `C:/Users/user/ssalddel/artifacts/local/validation/farm-h2-measurement/20260830T030719482Z/measurements.json`(SHA 99F8B557D4DC8C2E6044D7D7D688B043CBF502F86C5370418F44D6EE09BE9CC8)을 재사용했다. 후보4개의 Prefab/meta8파일과 GUID 일치, 해당 기록에 연결된 **고유 의존48파일/meta hash 일치**를 직접 확인했다. 이 집합은 이전 조사 집합이며 **현재 AssetDatabase 전이 의존 재조회/Library 재실측이 아니다**. 최초 대조의 Packages→PackageCache 경로 구분자 오류는 읽기 경로만 정정했고 자산 drift로 세지 않았다.

이번 좁은 YAML 판독에서 밭/상자는 MeshRenderer·MeshFilter·MeshCollider, 식물은 Renderer·Filter이고 업무 MonoBehaviour/Animator/Rigidbody 레코드를 발견하지 못했다. 실제 imported hierarchy/콜백 무부작용을 새로 인증한 것은 아니다. 음수 minY를 자동 lift하거나 Collider를 추가하지 않았다.

**실재 연결 공백은 세 가지다.**

1. `Ssalddel.Unity/Runtime/Farm/Farm수확상태PresentationPreparation.cs`의 `TryPrepare(SimulationFarmSurvivalStateSnapshot,...)`는 같은 Session/규칙/밭/재배·판본을 검사해 Growing/HarvestReady/Harvested와 Lot을 준비한다. null이면 `FarmSnapshotMissing_E5Unlinked`다. 조사 범위 `Ssalddel.Unity`와 Unity `Assets/Ssalddel`의 C#에서 제품 호출은 발견되지 않았다. 기존 오늘작업 CompositionRoot도 LocalScope에서 capability waiting으로 중단한다. 준비 API 존재는 실제 상태 공급/수확 Preview·Confirm 성공이 아니다.
2. `업무영역플레이폐루프Synty표현Modules.cs`의 기존 farm.crop.grow/harvest는 각각 `CropGrowing/CropHarvested` 상태와 `synty-family:farm:plants:potato / props:crate` 계열을 사용한다. 정확 후보 재고 계열은 `plants:potato-s / potato-l / box-potato`이며 밭은 `environments:dirt-rows`다. `Synty전체자산ModuleCatalog.FindCandidates`는 **family 정확 일치**를 요구한다. 따라서 기존 Slot 명칭/후보 목록만으로 위 Prefab이 선택된다고 할 수 없다. 기존 대장 fingerprint·과거 파일 의존 fingerprint·Prefab SHA는 서로 다른 값이다.
3. 지지면/예약 외곽/접근/선택 표식·소유 조립/해제와 현재 상태→정확 후보 변환은 미연결이다. 과거 **9밭 null component**·11대상 비활성·시점/허용Root 문제는 별개의 실제 통합 차단이다. 조사로 수리되었다고 하지 않으며 새 Editor/버튼/Play/Scene 변경은 없다.

다음 담당은 **개발, Presentation E4**: 기존 상태 이름·정확 후보·단일 표시 소비 경로 계약을 좁혀 연결해야 한다. Logic E5 상태 사본/Session·Save 준비가 충족되기 전 E5 실제 World 적용으로 넘어가지 않는다. 공간은 그 계약이 있는 별도 승인에서 지지·접근/배치를 검증한다. 신규 가공·제작 필요는 현재 확정되지 않았고 “파일 존재→그대로 제품 사용”으로 판정하지 않았다.

### 약초 — 기존 외형 선정 재사용, 지식 E4와 달이기 분리

[HB01 명세](../../eng/execution-ledgers/work-orders/nature-herbal-hb01-contents.e7-work-order.json)와 [냄비 교체 연구](../Architecture/PlayableLoops/Nature약초차-냄비표현교체-적용연구.md)(SHA C46404610AACD697A3AFF4967214DC51E2BB8B05B2BA74FFAB3D26273DD8D9F5)의 제한 수용을 재사용했다. `selectedPresentationReference`는 기존 D385 선정이고 제품 연결 완료가 아니다. HB01의 첫 순수 계산 `NotApplicable`는 보존하면서 전체 용기 표현 조사 범위를 선택 필드로 분리했다.

| 역할 / 판독 순간 | 정확 경로(Assets/Synty/ 아래) | GUID / 판정 |
| --- | --- | --- |
| Survival.Brew.Vessel / 종류·내용물·양·온도·차단 이유를 받을 조리용기 | PolygonTown/Prefabs/Props/SM_Prop_CookingPot_01.prefab | 28522495c7273dc4fa67b894ae25ff11 / 기존 외형 선정, 연결·설정 보완 |
| Survival.Brew.Cup / 옮김·음용 상태를 받을 컵 | PolygonGeneric/Prefabs/Props/SM_Gen_Prop_Mug_01.prefab | d23c715168b31804ba6fa370a6502b8f / 기존 후보 유지, 손 접촉·음용 미검사 |
| 제외된 원냄비 | PolygonDungeonRealms/Prefabs/Props/SM_Prop_Camp_Pot_01.prefab | 29fc435e87f624c4aa417f109a26bdf2 / 병 외형으로 냄비 역할 제외, 삭제 없음 |
| Survival.Camp.HeatSource / 열원 후보 | PolygonNature/Prefabs/Props/SM_Prop_CampFire_01.prefab | 1fbdd99ef1d1e2b4dac24a8d8ef04741 / 파일 존재, 냄비 지지·접근/열원 외형 미검사 |

주요4원본/meta8파일 hash/GUID 일치, 기존 D385 **22/22**와 용기 **20/20** 산출물 길이/hash 일치다. 전체 값은 명세에 있다. 과거 CookingPot 전체 크기 .6340182/.3077426/.338767081, Mug .196044713/.163742334/.116531953(Collider0)는 재사용 관찰값이며 새 실측/내부 용량이 아니다. 원냄비 뚜껑 닫힘을 보존하고 숨김은 과거 소유 복제본 진단만이다.

CookingPot/Mug 재고 참조는 있으나 조사 범위에서 HB01 `Survival.Brew.Vessel/Cup` 상태 소비자는 확인되지 않았다. 수면의 `sleepCampfirePrefab` 경로는 .65배·PointLight·AlignGround 장식으로, 열원/냄비 받침 계약이 아니다. 열원 `Accessible`도 지지면·위치·접근 기준점 계산이 아니다. 내용물 계약의1000/500/200mL는 승인 시험값이지 mesh 측정값이 아니다.

다음은 개발 Presentation E4 소비 경로·지지/접근 입력, **HB01 Logic E3의 Confirm 원자성/가열·섭취 잔여**, Session/Save의 Logic E1 영향 검토다. 공간·애니의 실제 용기/손/중단 복귀는 해당 상태 계약과 별도 승인이 필요하다. 정적 용기에는 Clip/Rig가 필요하지 않으며 음용 Clip/접촉 적합성은 미검사다. r129는 여전히 loader 전 중지이며 기존 실행 절의 `executionAllowed:true`가 남아 있어도 재개 권한으로 사용하지 않는다.

### 방문자 — 같은 Actor/Clip 재사용과 제품 연결 공백

[방문자 명세](../../eng/execution-ledgers/work-orders/nature-camp-visitor-stay.e7-work-order.json), [Accepted E5 연구 r2](../Architecture/PlayableLoops/Nature야영지방문자임시체류-E5표현연구.r2.md)(SHA 847D2A57CA215CD9B3310B2AD70331CF3A8F2D7586492E1964621E98346C67E0), [기존 동작 인계](방문자애니메이션-E5-인계-2026-08-30.md)·[상태 결속 검토](방문자애니메이션-상태결속검토-2026-08-30.md)를 대조했다.

| 역할 | 정확 자산(Assets/ 아래) | GUID |
| --- | --- | --- |
| VisitorArrival 남성 Actor | Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Male_01.prefab | f11fc98cf1e8d5547a9b2ec85cc9c664 |
| 동일 대상 Avatar, fileID9000000 | Synty/PolygonStarter/Models/Characters.fbx | 2dc7b382d25903545b405802eb2198ab |
| Idle | Synty/AnimationBaseLocomotion/Animations/Polygon/Masculine/Idle/A_Idle_Standing_Masc.fbx | a4b8f2deec6a99945987cfc59b1b4e54 |
| Walk | Synty/AnimationBaseLocomotion/Animations/Polygon/Masculine/Locomotion/Walk/A_Walk_F_Masc.fbx | eead3eb07cb89cb4eb25b3550df5e84b |
| 선택 Greet | Synty/AnimationEmotesAndTaunts/Animations/Polygon/Masculine/Greet/A_POLY_EMOT_Greet_Wave_Masc.fbx | 93358076c2333114f95ea9ed86e831e8 |
| Controller(별도 Wrapper Prefab 아님) | Ssalddel/Presentation/VisitorAnimation/방문자Wrapper.controller | e41e1adabad620b41828be1189ffd5c9 |

전용 인계15/15 hash·Actor/Avatar/3Clip10원본/meta 기존 기준 일치, 위 Controller 포함12파일을 새 명세 값과 재대조했다. Prefab의 Controller는0이며 Controller 자산의 존재가 자동 소비를 뜻하지 않는다. 과거8/8은 NUnit 본문 직접호출이고, PNG12는 Animator 평가→BakeMesh/임시Unlit 포즈 표본이다. 이번 새 재생·화면·정식 Runner는 없다. PNG12에 과거 개별hash 기준이 없어 과거와12/12 일치라고 하지 않는다.

| 상태 / VisualKey | 필요한 읽힘과 실제 공백 |
| --- | --- |
| AwaitingDecision / Community.Visitor.Stay.AwaitingDecision | 안전 경계 입구~생활 중심의 VisitorWaitingAnchor, 1회 선택 Greet→Idle. 같은 상태 재공급/복원 때 인사 중복을 막는 제품 소비 필요. |
| TemporaryStay / Community.Visitor.Stay.TemporaryStay | 남은 수용칸·명시 Confirm 후 별도 GuestRestAnchor로 이동·도착Idle. 기존 Player 침상과 구분, 단일 이동 작성자/경로 미연결. |
| Rejected / Community.Visitor.Stay.Rejected | 권위 거절 후 VisitorDepartureAnchor 이탈. 애니 종료가 거절 확정/관계 벌점이 아님. |

`방문자AnimationAdapter`의 확인된 소비는 Editor Builder/Validation/Tests이며 준비 `방문자체류VisualBinding`의 실제 제품 연결도 발견되지 않았다. Preparation은 외부 Role/Cue/Clip 문자열을 복사하며 시험의 같은 Greet 문자열이 모든 상태의 동작 구현 증거는 아니다. 기존 NPC 공용 Adapter와 중복 뼈/이동 작성자를 붙이지 않는다. 실제 Session/Actor 권한·SessionRevision 원자성·Save/Replay·지지면/통행/손님Slot이 남아 있다.

벤치·침상·여성 대체·Shelter·Town 침상·표지판의 기존 정확 후보 파일도 존재/GUID/hash를 읽었지만 이번 첫 Actor 조사 밖의 **배치·실측은 미검사**다. 여성 후보를 고정된 남성 Actor 대신 자동 선택하지 않는다. Walk meta에는 pelvis/Hips 불일치 기록이 남아 과거 Library 평가를 fresh import 성공으로 확대하지 않는다.

기존 E4 연구의 fallback `A_POLY_EMOT_Base_Idle_Masc.fbx` SHA는 끝 BDF가 빠진61자리다. 현재 파일 SHA **E0BFDB66BCFB88BD76DAA7B77054210D100E4AA669AE58447E5C57D6CCEF0BDF**, GUID f3ebe68afe1718744bba319137c3ee8f를 확인했다. 이는 **과거 기록 불완전**이며 파일 변경 증거가 아니다. 실제 Wrapper의 주 Idle과 다른 자산이고 동결 연구 원문은 고치지 않았다. 후속 연구 정정이 필요하면 새 판본으로 처리한다.

판정은 **기존 후보·격리 자료 재사용/제품 연결·설정 보완**이다. 새 가공·제작 필요는 확인되지 않았다. 개발 Presentation E4 상태/cue/단일 이동 결속, Logic E5 전 Session·권한/Save 영향 검토가 먼저다. 필요한 fresh import/Rig 및 공간 실행은 별도 승인에만 배분한다.

### D388 변경·검증 경계

Farm/HB01/방문자 세 명세의 기존 E·상한·활성 WI·연구·과거 실행 절을 보존하고 조사 내용만 추가했다. 기존 후보 경로/GUID를 바꾸거나 재고 계열을 몰래 교체하지 않았다. Goal은 해당 현재 명세 hash 참조와 조사 인계만 동기화하며, 기존 Active 표시는 중지 해제 권한이 아니다. 신규 실제 실행/가공/Scene·Save·Logic 코드·commit/push는 없다.

검증: 주요14자산 원본/meta28파일·GUID와 D386 근거12파일 재대조 통과. 구조24/관리42사례, Farm/HB01/방문자 명세 검사·표현 생성/검사·Goalr132 생성/검사 통과. Fast080048 문서9경로 통과(빌드/시험 생략). 별도 파일검증 스크립트는 오류문자열의 PowerShell 변수 구분자 구문오류를 실행 전 파싱 단계에서 발견·정정 후 통과했으며 최초 오류를 자산 실패로 세지 않았다. 로그/결과는 artifacts/local/validation/presentation-balance-d388/verification.json이다. 원장 판정/EvidencePackage 승격은 없다.
