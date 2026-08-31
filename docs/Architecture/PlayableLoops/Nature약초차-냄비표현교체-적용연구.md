# Nature 약초차 냄비 표현 교체 적용 연구

- 판본: `herbal-pot-visual-replacement-study.r1` / 2026-08-31.
- 상태: **Draft**. D385가 승인한 교체 방향에 대한 Required 후속 연구 초안이며, 적합 Prefab 선정 완료·구현·실제 게임 연결·E 승격을 뜻하지 않는다.
- 작성/실측 담당: 월드·공간·배치. 연구 검토·명세 결속·통합은 개발 담당.
- 승인 출처: [D385 원문](../../AI/약초차-냄비표현교체-2026-08-31.md), SHA-256 `ACBE115C09D4C9792D628C2A82FB2C85814CB8F06D1DD457D7278FCD69845F90`.
- 기존 기준: [HB01 개발 승인 보완](Nature약초차-HB01개발승인보완.md), [약초 제작 문답](PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md). 기존 Required 연구·게임 규칙·E 상한을 유지한다.
- 이번 범위: 디스크 파일 읽기, 후보 한 개와 후속 검증 설계, 이 문서 작성 및 문서 전용 Fast. Editor 조회/AssetDatabase 로드/렌더/Play/Assets·Scene·원장 수정은 하지 않았다.

## 1. 결론과 선택 경계

우선 검토 후보는 Town의 `SM_Prop_CookingPot_01` **한 개**다. 조리 용기 이름에 더해 본체와 뚜껑이 별도 메시/자식으로 연결되고, Prefab 구성과 충돌 Mesh의 직렬화 구조를 확인했다. 이는 격리 외형 검토를 먼저 할 근거이지, 열린 냄비로 적합하다는 판정이 아니다. 기본 뚜껑은 활성 상태이므로 넓은 입구·내부 면 존재가 핵심 미검증이다.

재고 이름 조사에는 Town/Farm/Generic의 Pot 및 DungeonRealms CampPot 계열이 포함됐지만 수를 채우기 위해 추가 후보를 올리지 않았다. `SM_Gen_Prop_Pot_01`은 아래 미해결 스크립트 위험으로 보류한다. CampPot02~07은 파일명과 단일 메시 구조만으로 열린 형상을 판별할 수 없어 이번 우선 목록에 넣지 않는다. D385에 따라 기존 CampPot01을 약초차 냄비 역할로 다시 통과시키지 않는다.

적합성 확인 후 개발이 후보와 근거를 명세에 결속한다. 같은 선택을 사용자에게 다시 묻지 않는다. 다만 형상 가공, 새 게임 의미, 권리 미확인, 기존 공간 기준 변경이 필요하면 해당 부분을 분리해 반환한다.

## 2. 원후보와 01:35 실측 이력 보존

기존 자료 위치는 `C:/Users/user/ssalddel/artifacts/local/validation/overnight-cross-domain-visual-d382/herbal-vessel-review/`다. 2026-08-31 01:35:37 KST, 소유 PreviewRenderUtility로 25도/65도·768×768 PNG 네 장을 확보했다. **격리 자산 외형이며 실제 Game View/음용/달이기 증거가 아니다.** 이번 연구에서 해당 파일은 수정하지 않는다.

- 인계문 `handoff.md` SHA-256: `81AA0E264065069208E35715FE784388EFBF70E254C8500EBADA0A6757D51ED7`.
- 구성 대장 `manifest.json` SHA-256: `3D956C03502A40305B35864C18E8BECFBEE6F44F89F634C050D712AF8D706BF3` (개발 인수 20/20).
- CampPot01: 좁고 긴 목/둥근 몸통으로 판독. Renderer 외곽 크기 약 `(0.508692265, 0.5952618, 0.508692145)`, native Scale1. D385에서 냄비 역할 제외, 다른 용도의 적합성은 미결정.
- Mug01: 열린 입구·내부 벽·손잡이 판독. Renderer 외곽 크기 약 `(0.196044713, 0.163742334, 0.116531953)`, native Scale1, Collider0. **교체 대상 아님**, 음용/손 접촉·충돌 가능 판정은 미검증.
- 위 값은 당시 Renderer Bounds이며 실물 리터/열 안전/게임상 용량이 아니다. 개별 카메라 맞춤으로 두 자산 이미지의 화면 크기도 실물 비율 비교로 쓰지 않는다.

| 유지 대상 | GUID | Prefab SHA-256 | meta SHA-256 |
| --- | --- | --- | --- |
| `Assets/Synty/PolygonDungeonRealms/Prefabs/Props/SM_Prop_Camp_Pot_01.prefab` | `29fc435e87f624c4aa417f109a26bdf2` | `BECD94A3B590B40AFBA2D39211878F0BE8474B78C5C9A322AB95733FB5CBC7AC` | `079CD71101C5B2A7D730C7A129730BBEB4B2C62544ADE26209DC593F802DD5E8` |
| `Assets/Synty/PolygonGeneric/Prefabs/Props/SM_Gen_Prop_Mug_01.prefab` | `d23c715168b31804ba6fa370a6502b8f` | `2609BF1E13F7500548CA4B76C3C63B03F198F9D2E527CC921DB7963A39CC3357` | `F920F854D7AFD7022C80F5B1B30C187D7CAA15B6C413C650724C6086B93E9973` |

## 3. 우선 후보의 파일 근거

Unity 경로 기준은 `C:/Users/user/ssalddel/`다. 후보 Prefab은 `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_CookingPot_01.prefab`이며 정확 GUID/hash는 부록에 기록한다. meta의 패키지 출처는 POLYGON Town Pack 1.9.1 / product121115 / upload917042다. 패키지 이름은 공간 역할이나 새 권리 판정이 아니다.

| 항목 | 디스크에서 확인한 값 | 한계 |
| --- | --- | --- |
| 구조 | GameObject2, Transform2, MeshFilter2, MeshRenderer2, MeshCollider1. 본체 아래 `SM_Prop_CookingPot_Lid_01` 자식 | 실제 로드 후 null 및 동적 타입 관문은 별도 |
| 초기 표시 | 본체/뚜껑 activeSelf1, Renderer enabled1 | 열린 입구가 노출된 형상인지 미확인 |
| 원본 변환 | 본체 위치0/회전 identity/Scale1. 뚜껑 위치 `(-0.0000032520943,0.19968094,-0.00000027739011)`, 회전 identity/Scale1 | 좌표 단위의 실제 렌더 크기·지지점 측정 필요 |
| 본체/뚜껑 Mesh | 모델 GUID `6978b0bcac9cb234aa7fdac20e1a7f4b`, fileID `4300000` / `4300002` | FBX 형상을 이번에 해독하거나 로드하지 않음 |
| ModelImporter | globalScale1/useFileUnits1/useFileScale1, importAnimation0/animationType0/clip0/importBlendShapes0/addColliders0 | import 과정 전체의 무부작용을 보장하지 않음 |
| 충돌 | 본체 MeshCollider enabled1/convex1/isTrigger0. Mesh fileID `43885486107449866` | 실제 물리 배치·열원과의 충돌은 미검증 |
| 충돌 파일 | `!u!43 Mesh` 직접 저장, 16정점/84인덱스/부분 메시1. 스크립트 컨테이너 없음 | Renderer 형상/내부/통행 충돌 성공과 별개 |
| 재질 | 두 Renderer 모두 `PolygonTown_01_A.mat` 한 슬롯씩 | 실제 셰이더 지원/색/내부 면/그림자 미검증 |
| Prefab 실행 구성 | YAML에 MonoBehaviour/Animator/Rigidbody 없음, 구성요소 참조와 블록 존재 확인 | 파일 참조가 0이 아니어도 실제 로드 null 부재를 뜻하지 않음 |

충돌 Mesh에 저장된 AABB 중심은 `(-0.15334737,0.099550225,0.0017896742)`, 반길이는 `(0.3109455,0.104527734,0.1599229)`, 전체 크기는 `(0.621891,0.209055468,0.3198458)`다. **충돌 메시 파일 값**일 뿐 Renderer 실측/native 외형 크기가 아니다. 중심이 pivot에서 치우친 이유나 손잡이 존재는 이 수치만으로 추정하지 않는다.

### 재질 메타데이터와 로드 위험

`PolygonTown_01_A.mat`은 ShaderGraph `Generic_Basic`을 참조하고 ALPHATEST/TransparentCutout 및 AlphaClip1 설정이 있다. normal 텍스처는 null 설정이며 emission 텍스처는 연결됐지만 Enable_Emission0이다. 선택적 normal 설정을 손상 자산으로 단정하지 않으며 실제 렌더 판독은 후속 관문이다.

재질 내부 두 `!u!114` 객체는 GUID를 실제 패키지 소스에 대조했다.

- `d0353a89b1f911e48b9e16bdc9f2e058`: `UnityEditor.Rendering.Universal.AssetVersion`, version10.
- `639247ca83abc874e893eb93af2b5e44`: `UnityEditor.Rendering.BuiltIn.AssetVersion`, version0.

읽은 두 클래스는 ScriptableObject 상속과 `public int version` 필드만 선언하며 로드 콜백 메서드를 선언하지 않는다. 이는 **이 두 타입**의 파일 근거이며 Unity/ShaderImporter/패키지 전체 로드 콜백이 없다는 보장이 아니다. 실제 persistent 타입·null·의존 자산의 유효성은 Editor 점유를 별도 배분받은 후 확인한다. 현재 승인 없이 로드하지 않았다.

### GenericPot01 보류 근거

`Assets/Synty/PolygonGeneric/Models/Collision/SM_Gen_Prop_Pot_01_Convex.asset` SHA-256 `B0D5AEEC554023239F9E2C5D8A7F621490FADF4E282574A3D82D43437EEA1B1D`에는 Mesh 외에 ScriptableObject 직렬화 객체가 있다. scriptGUID `5b71ad40e238046238f9b0c6f33c3791`, EditorClassIdentifier 비어 있음, ConvexMeshes/HashOfSourceMeshes 필드가 있으나 정확 타입과 콜백을 해결하지 못했다. Assets/Packages/Library PackageCache의 meta 및 관련 소스 검색으로 타입을 찾지 못한 범위만 확인했다. 내부 Mesh 참조가 있다는 이유로 컨테이너를 무시하거나 안전 타입으로 판정하지 않는다.

## 4. 적합성·열원 접근 수용 조건

1. 위/옆 격리 nativeScale1 화면에서 넓은 입구, 내부 바닥/벽, 물·약초를 담는 조리 용기로 읽히는지 확인한다. 화분·병·막힌 소품이면 거부한다.
2. **초기 뚜껑 활성 상태를 먼저 보존·촬영**한다. 몸체 입구 확인을 위해 소유 복제본의 뚜껑만 일시 숨기는 진단이 필요하면 대상/원복을 다음 배분에 명시한다. vendor 자식 삭제/원본 SetActive/메시 가공은 하지 않는다. 별도 뚜껑 메시 존재는 뚜껑 애니메이션이나 플레이 기능 승인이 아니다.
3. 실제 persistent 타입/null/mesh/material을 검증하고 Renderer 및 Collider local/world Bounds, pivot, 단위·모델 배율을 함께 기록한다. 자동 축소/크기 맞춤·자동 lift로 통과시키지 않는다.
4. 기존 열원 후보는 문답의 `Assets/Synty/PolygonNature/Prefabs/Props/SM_Prop_CampFire_01.prefab`, GUID `1fbdd99ef1d1e2b4dac24a8d8ef04741`, 시각 키 `Survival.Camp.HeatSource`다. 이번에 해당 자산을 로드/실측하지 않았으며 실제 열원 공급/배치 기준점 연결도 확인하지 않았다.
5. 열원 위 실제 지지면과 냄비 바닥의 접촉, Renderer/Collider 외곽, 겹침·매몰·부유, Player 접근 및 상호작용점 도달을 각각 검증한다. 지지면 또는 접근 계약이 없으면 숫자0 평면/임의 여유/합성 성공으로 채우지 않는다. 콜라이더/뚜껑이 접근을 막는지도 별도 확인한다.
6. 달이기의 시간·연료·실제 불 상태·내용물·용량은 기존 HB01 계약으로 판정한다. 메시에서 리터나 열 안전을 역산하지 않는다. Q349의 달이기 시작 후 다른 활동 가능, Q376의 실제 불·연료 기준을 유지하며 냄비 옆 대기/입력 유지라는 새 규칙을 만들지 않는다.
7. Mug/원기획/기존 자료는 유지한다. 실제 상태 연결이 없는 정지 배치나 격리 외형은 내용물/가열/완료/음용 성공을 뜻하지 않는다.

## 5. 후속 실행·소유 경로 제안

현재는 연구 작성만 허용된다. 아래는 **개발의 연구 수용 및 다음 배분을 위한 제안**이며 즉시 점유/구현/저장 허가가 아니다.

- 공간 담당: 후보 한 개의 persistent 읽기 관문과 소유 PreviewRenderUtility, 위/옆 PNG 및 바이트 인코딩·크기/hash 직접 확인. 제안 출력은 `C:/Users/user/ssalddel/artifacts/local/validation/overnight-cross-domain-visual-d382/herbal-pot-replacement-d385/` 하위다. 새 Assets 코드를 만들지 않고 가능한 기존 격리 절차를 재사용한다.
- 시작/종료: stopped/compilefalse·Scene/dirty/selection·비소유 Preview/Graph·전체 객체, Scene/Packages/Save/원본 의존 hash 기준선을 보존한다. 새 대화/불명 스크립트/다른 dirty이면 해당 후보만 중지한다. 소유 미리보기와 임시 텍스처만 정리한다.
- 개발 담당: 적합 후보 선정 근거와 Required 연구/hash를 HB01 명세에 결속하고 **실제 소비하는 정확한 참조/대장/조립 경로**를 지정한다. 조사한 Unity `Assets/Ssalddel`, Hongdal `Ssalddel.Unity` 및 `eng`의 cs/json/asset 범위에서는 `Survival.Brew.Vessel`/CampPot01에 대해 작업 명세 밖의 실제 소비 연결을 확정하지 못했다. 이것을 런타임 연결이 전혀 없다는 증명으로 쓰지 않는다.
- 교체 적용: 기존 `Survival.Brew.Vessel`의 의미와 상태 식별자를 보존하고 표현 참조만 바꿀 수 있는지 먼저 대조한다. 실제 연결점이 없으면 필요한 좁은 소비 경로를 개발이 별도 결속한다. 기존명세·원장·CURRENT_WORK는 공간 담당이 변경하지 않는다.
- 실제 배치/Game View: 열원·지지·접근 및 현재 상태 연결을 통과한 계획만 별도 단독 슬롯으로 검증한다. canonical Scene의 변경은 정확 소유 항목/보존·저장 조건을 등록한 뒤 수행한다. 이번 파일 조사에 Scene 저장이나 표현 교체를 포함하지 않는다.

## 6. 검증 상태와 반환 조건

| 항목 | 상태 | 근거/다음 조건 |
| --- | --- | --- |
| D385 원문/hash | Passed | 승인 방향과 범위를 파일로 대조 |
| 후보/직접 의존 파일 및 meta 9쌍 | Passed | 경로·GUID·SHA-256, 아래 부록 |
| Prefab 직렬 구조/기본 뚜껑/충돌 Mesh/재질 메타데이터 타입 | Passed(파일 범위) | 실제 로드·활성화 검증과 구분 |
| 실제 persistent null/타입/셰이더 지원 | NotRun | 다음 Editor 슬롯 필요 |
| 열린 입구·내부 면·위/옆 PNG 및 native Renderer 크기 | NotRun | 뚜껑 초기 상태를 먼저 관찰 |
| 열원 지지·Player 접근·실제 참조 교체 | NotRun | 대상/소비 경로 결속 필요 |
| Game View/가열·달이기·음용/청취 | NotRun | 외형 연구로 대체하지 않음 |

문서 검증은 이 파일만 지정한 Fast 계획에서 GuidanceOnly=true, 코드/E 지도 검사=false, BuildTargets/TestPlans 빈 목록을 확인했다. 2026-08-31 01:56:38 실행에서 git diff --check가 통과했고 빌드·시험은 실행하지 않았다. 로그는 `C:/Users/user/source/repos/Hongdal/artifacts/local/validation/20260831-015638/`다. 파일/meta 18/18 hash도 재대조 일치했다. 최종 연구 hash는 개발에 별도 반환한다. 기존 후보/실측/승인 원문의 hash를 새 후보 hash로 덮어쓰지 않는다. 적합성이 확인되지 않으면 이유와 필요한 좁은 검증만 반환하며, 후보 없음도 허용한다.

## 부록. 후보 및 확인한 의존 파일 기준선

아래는 디스크에서 확인한 직접 자산 및 재질 메타데이터 타입 소스의 기준선이다. **AssetDatabase 전체 전이 의존성 목록이나 실행 Assembly 목록을 대신하지 않는다.** 경로는 Unity 프로젝트 기준이며 Library 경로는 당시 실제 패키지 캐시다.

### 자산 1

- 경로: `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_CookingPot_01.prefab`
- GUID: `28522495c7273dc4fa67b894ae25ff11`
- 파일 SHA-256: `3BB5376B0AFE523B03238122C117A45D4A7B286FD2AC8F3CBBB0C44629976E3B`
- meta SHA-256: `8AF03FFAACDD8D8C9EAE7E30930DC3F348F4375661FA9DE8D785AEF538547A22`

### 자산 2

- 경로: `Assets/Synty/PolygonTown/Models/Props/SM_Prop_CookingPot_01.fbx`
- GUID: `6978b0bcac9cb234aa7fdac20e1a7f4b`
- 파일 SHA-256: `FF147394938F08009BA8B37363789B1D6E093151AA9805B308F8A5A27B135A9F`
- meta SHA-256: `D603F32C790411907F341E3ACFAAC391A8DBC1F325AC6B82DD43B0A9C499248E`

### 자산 3

- 경로: `Assets/Synty/PolygonTown/Models/Collision/Convex/SM_Prop_CookingPot_01_Convex.asset`
- GUID: `507ec5ff47e69c94f92bef51a933595e`
- 파일 SHA-256: `DB0CE1557BAFC31F94C9ABB4397EA9531718C652FEEFDDBE8119332BA67FC8F3`
- meta SHA-256: `09099D647E1C39B65C415845CB88FAE4FEB0AAFD8571619E46E1EC7BEBABB0EF`

### 자산 4

- 경로: `Assets/Synty/PolygonTown/Materials/Alts/PolygonTown_01_A.mat`
- GUID: `28646867ddcf90a4989276acd313ea43`
- 파일 SHA-256: `FB455CB94E82E88E6CCB02EACBEB29C42DD287A5CA6EC39DAA4F628A2809E4C0`
- meta SHA-256: `4F9BD362C5F3769C4EF836448B5BA56BA7AB7D0F92B2CCF4569467E6E432320E`

### 자산 5

- 경로: `Assets/Synty/PolygonGeneric/Shaders/Generic_Basic.shadergraph`
- GUID: `0730dae39bc73f34796280af9875ce14`
- 파일 SHA-256: `43330CDD0B0074AB148F5785DBB3512D176864816C1AD08D2485521A1EAB0182`
- meta SHA-256: `725F61C0FF5DA996D097268F89CD8CACDDEA950A720BBAA455D56EA826693E06`

### 자산 6

- 경로: `Assets/Synty/PolygonTown/Textures/Alts/PolygonTown_01_A.png`
- GUID: `3ff3041d26e84b247b1a900206562e65`
- 파일 SHA-256: `141B1AD20BDC9DDA0B187378FB9F55B1C00939358283D47A8E6EDDD0BD07B07C`
- meta SHA-256: `821D2F5346BF62CB7F1EAFCCC1FCCCC4D611489ECF4178C84605453BFD080DEA`

### 자산 7

- 경로: `Assets/Synty/PolygonTown/Textures/Misc/PolygonTown_01_Emissive.png`
- GUID: `efa34d710ef93124aa8baa1a8e6708d0`
- 파일 SHA-256: `A4A2CE4BF43F085B3D48E02C0971245C7C9AC540DDA2CB8B22A2529BD6DF0BDE`
- meta SHA-256: `E8B5D98CAF017261CA389EAAC6CCF55B4A36AED77DA2CA68DFADC63C9BBFED38`

### 자산 8

- 경로: `Library/PackageCache/com.unity.render-pipelines.universal@73b4c4ff130e/Editor/AssetVersion.cs`
- GUID: `d0353a89b1f911e48b9e16bdc9f2e058`
- 파일 SHA-256: `96ED27E15286CDA1FDADA6923E804887B6447A905B38360E0F5561F79CD0344C`
- meta SHA-256: `E7EC88783B56AE3A5ADBE60D3AB71FD3B6FE269932AF20218D4CC00EB3879868`

### 자산 9

- 경로: `Library/PackageCache/com.unity.shadergraph@8d7376ec43ee/Editor/Generation/Targets/BuiltIn/Editor/AssetVersion.cs`
- GUID: `639247ca83abc874e893eb93af2b5e44`
- 파일 SHA-256: `EE787C6EF844C6D4732FB1E914E43CFD11F1E8E49125B5DEB98652CDA85E8E06`
- meta SHA-256: `47DF80C771E6A4097BE3B7E755BA51D266900327FE261FFE8210BCA7C794E470`

## 7. 개발 제한 수용 — 외형 실측 관문

개발 통합 담당 01a02198-8b2a-7491-ac93-366b30ff474c가 위 Draft 전체 SHA256 `3298D5441862056F1A49FFF142067E49AC816AADF12FD0133533E72DCDF2F026`를 읽고 수용한다. 기존 1~6절과 부록은 조사 이력이며, **이번 Accepted는 Town CookingPot01 한 후보의 persistent 읽기·격리 외형 실측 절차만**이다. 최종 냄비 선정·열원 배치·실제 참조 교체/게임 연결은 아직 미확정이다.

- 정확 후보와 의존 기준선, 원본 무변경, 자기 Preview 정리, 시작/종료 Scene·파일·비소유 Preview/Graph 보존을 유지한다.
- 기본 뚜껑 포함 native1 위/옆 화면을 먼저 촬영한다. 소유 복제본에서 정확 Lid 자식의 Renderer만 일시 숨긴 진단 화면을 별도로 허용한다. 숨김 전후 본체/TRS/Collider·재질 불변을 확인하고 해당 Renderer를 원복한다. 원본 활성/자식 삭제·메시 변경·새 뚜껑 동작 제작은 없다.
- 입구/내부 면이 없거나 불명확하면 적합 판정을 하지 않는다. 실제 열원/지지·Player 접근 계약을 공급하지 않은 이번 미리보기는 해당 적합성 미검증으로 남긴다. 외형 적합성의 부분판정과 실제 배치 적합성을 분리한다.
- 실행은 HB01 명세의 정확 후보/hash·소유 출력경로와 단독 Editor 슬롯 시작 통보 이후다. D383 평가와 Editor 점유를 겹치지 않는다. 용량·연료·시간·음용·Mug01·E 변경 없음.

## 8. 개발 외형 선정 — Town CookingPot01

개발 통합 담당 01a02198-8b2a-7491-ac93-366b30ff474c는 7절까지의 SHA256 `BFEFC1FA6B638DD096E7A8EF0CE091FCB44750A47300D820ECBD2CF0D63010F9`를 보존하고, D385 승인에 따라 **Survival.Brew.Vessel의 현재 외형 선정 참조를 Town CookingPot01로 변경한다**. 이는 문서·작업 명세의 선정이며 아직 제품 Scene/실행 카탈로그 연결을 뜻하지 않는다. 원래 CampPot01은 냄비 역할에서 제외하고 원본/이전 조사 기록은 보존한다.

- 정확 경로/GUID/Prefab·meta hash는 2절 및 부록의 `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_CookingPot_01.prefab` / `28522495c7273dc4fa67b894ae25ff11` 그대로다. 원본의 기본 뚜껑 닫힘 상태를 유지한다. 소유 복제본의 뚜껑 숨김 화면은 내부 검토 진단이며 제품의 뚜껑 동작이나 기본 열린 상태 승인으로 확대하지 않는다.
- 개발이 기본/뚜껑 숨김 각각25·65도 PNG4장을 직접 열람했다. 넓은 입구·내부 벽/바닥·긴 손잡이로 조리 냄비임을 판독할 수 있어 좁은 목 병 형태의 이전 후보보다 적합하다. 각 기본/진단 쌍은 동일 카메라/native1이며 22/22 산출물 hash·길이를 직접 대조했다.
- 본체 Renderer 외곽 X/Y/Z `0.6271664 / 0.203881681 / 0.3254663`, 뚜껑 포함 `0.6340182 / 0.3077426 / 0.338767081`, Collider 외곽 `0.621891 / 0.209055468 / 0.3198458` Unity units다. 손잡이 포함 외곽은 내부 용량이나 열원 지지 면적이 아니다.
- 근거: Unity `artifacts/local/validation/overnight-cross-domain-visual-d382/herbal-pot-replacement-d385/handoff.md` SHA `358139922458218EB896B56B17593B22D3DD90D88F4DF25CC0558401F22779D4`, manifest SHA `972C79F1FFC7FF7FC09DA675CA080782A89F6A8CCA16A21D29F612B27EFD15EF`. 실제 열원/지지·Player 접근/손 접촉·내용물/달이기/섭취는 NotRun이다.
- 공간 종료02:00:49의 공식4701객체 직렬 비교/파일5·Assembly4/의존32경로 보존, Preview개수1/Graph0/Scopefalse/envnull/Console새0을 인수한다. 비소유 Preview 내부 전체 직렬값·비직렬 캐시 보존은 확인 범위 밖이다. 평가식 API/래퍼 관문 실패와 수정 후 촬영을 구분한다.
- 후속은 정확 소비 참조를 파일 조사하여 명세의 선정과 실제 제품 연결을 구분한다. 새 Scene 저장·Prefab 수정·열원/용량 규칙 변경·Mug 교체·E 승격은 이 선정에 포함하지 않는다.

## 9. Draft — CampFire01 단일 자산 외형·의존 사전검사 준비

이 절은 D385 후속 파일 조사와 개발의 단일 자산 준비 요청에 따른 **미수용 Draft**다. 기존 1~8절 및 부록 전체 바이트 접두부 SHA-256 `E36CBC863A771170B0EB6CD5DFCA1369989A41E0C3B5E914E8112E8B813F21B3`를 보존한다. 8절의 Town CookingPot01 외형 선정, 원 CampPot01 역할 제외/원본 보존, Mug 불변은 바꾸지 않는다. [D385 원문](../../AI/약초차-냄비표현교체-2026-08-31.md) SHA `ACBE115C09D4C9792D628C2A82FB2C85814CB8F06D1DD457D7278FCD69845F90`를 따른다. 이번 쓰기 허가는 이 연구의 9절과 문서 전용 Fast 신규 로그뿐이다. 아래의 자산 로드·미리보기·준비 코드 작성은 **아직 실행 허가가 없다**.

### 9.1 파일 조사에서 확인한 소비 경계와 다음 입력

- HB01 작업 명세 `eng/execution-ledgers/work-orders/nature-herbal-hb01-contents.e7-work-order.json`의 조사 당시 SHA는 `EDA8144493B3326BB4713276C3ADBCF12829EBFF181D7C30E2CB30ED3766474E`다. 실제 선정 필드는 `d385PotVisualReplacement.selectedPresentationReference`이며 `selectedVisualBinding`이라는 별도 필드로 간주하지 않는다. 이는 문서 선정과 제품 연결을 구별하기 위한 당시 기준선이다.
- Unity `Assets/Ssalddel/Resources/Nature생존VisualCatalog.asset`의 `sleepCampfirePrefab`은 아래 CampFire01을 참조한다(SHA `E9E3E012D519877427AF5D2AFE76A362B4CA80D005BB2D0A7E67A43C1738870A`). `Assets/Ssalddel/Presentation/World/Nature생존Controller.cs`(SHA `BC781031E3187208DE92D07A702A08429168401B4A38D11FFFCC8266E4C14FB0`)의 `RenderNightAndDawn`은 수면 중 `Nature_Cabin_SleepWarmLight`를 만들고 0.65배, PointLight, 오두막 상대 위치, `AlignGroundVisual`을 적용한다. 이는 **수면 장식 소비**이며 달이기 열원이나 냄비 받침 계약이 아니다. 해당 생성/정렬/fallback 경로는 이번 준비·후속 미리보기에서 호출하지 않는다.
- [열원 상태 계약](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/Simulation열원상태Contracts.cs)(SHA `E828486B23012AD6EB64C39256472CFD365CF58B1E0F0B960539AEB8B3F85A50`)에는 `HeatSourceStableId`, `Accessible`, 연료/에너지/버전이 있으나 지지 면, 위치, 접근 기준점이나 거리 계산 입력은 없다. `Accessible`을 실측 접근 성공으로 해석하지 않는다. [열원 서비스](../../../Ssalddel.Simulation.Application/RuntimeCore/Simulation열원상태Service.cs)(SHA `84B1CAEC75E44CBC78A96E11E271C6CA09985FC719984BD717584BB525069110`)의 Preview/Confirm은 이번 실행 대상이 아니다.
- 조사 범위 Unity `Assets/Ssalddel`의 cs/json/asset 및 Hongdal `Ssalddel.Unity`·`eng`에서 선정 냄비와 이 열원 서비스의 실제 표시 소비 연결은 확인하지 못했다. 전역 부재 판정은 아니다. 따라서 다음 가장 작은 입력은 **아래 정확 CampFire01 하나의 정적 외형·의존 기록**이다. 냄비 상대 TRS나 접촉 면적을 임의로 정하기 전에 별도의 지지 표면/접근 계약이 필요하다.

### 9.2 정확 후보와 파일 기준선

Unity 기준 경로 `Assets/Synty/PolygonNature/Prefabs/Props/SM_Prop_CampFire_01.prefab`, GUID `1fbdd99ef1d1e2b4dac24a8d8ef04741` 하나만 대상이다. 대체 자산이나 수 채우기 후보는 없다. 다음은 **디스크 직접 참조 조사**이며 AssetDatabase 전체 전이 의존성 검사가 끝났다는 뜻이 아니다.

- Prefab: `Assets/Synty/PolygonNature/Prefabs/Props/SM_Prop_CampFire_01.prefab`
  - 파일 SHA-256: `7D2BF08438AEFD66C31669A9E2BA4569D32A200EEA116E79D2DCAF04F7340E32`
  - meta SHA-256: `4BBF8BBAA1BA01CBDC4937603285F754EAE245B5C3E63FA10C2707653DD63AF2`
- Mesh: `Assets/Synty/PolygonNature/Models/SM_Prop_CampFire_01.fbx`
  - 파일 SHA-256: `010E0E9556EA92E2A58DCD9696026B3049B5592B4425A8B9C3B9DAE6148F4137`
  - meta SHA-256: `1E75D9AD70D644E2BD4CDB70D41F01B51CBAD27E238FA2ABA33E496A6D0BBB78`
- 재질: `Assets/Synty/PolygonNature/Materials/Alts/PolygonNature_01.mat`
  - 파일 SHA-256: `641360FC6B4BE65CCF91A3B52CF4C42385C4F499A120E1E8A3618160745D3BE2`
  - meta SHA-256: `DACFEACC73EE501A8B2A41F77787F589ACFC2EB8AE63FA2416A652985A3FB36A`
- 재질의 Albedo 텍스처: `Assets/Synty/PolygonNature/Textures/PolygonNature_01.png`
  - 파일 SHA-256: `A80BCB7F349D8A3AFF034481BE51FB1205FF03F91DFF949C37AD61811716616F`
  - meta SHA-256: `5C19DAB67B2099757245428C1307FA3F95231B6EE04B0865EBA8CE2320CFA17B`
- 재질의 ShaderGraph: `Assets/Synty/PolygonGeneric/Shaders/Generic_Basic.shadergraph`
  - 파일 SHA-256: `43330CDD0B0074AB148F5785DBB3512D176864816C1AD08D2485521A1EAB0182`
  - meta SHA-256: `725F61C0FF5DA996D097268F89CD8CACDDEA950A720BBAA455D56EA826693E06`

Prefab의 Mesh 참조는 fileID `4300000` / GUID `5f9f6ec19ed9ec548a73b9775e5a7154`, 재질은 fileID `2100000` / GUID `b72e16591230315448c77c827b522ae1`이다. 재질은 Shader GUID `0730dae39bc73f34796280af9875ce14`, Albedo GUID `2edd58f7c433e934db9029c375173b44`를 참조한다. 파일명으로 실제 열린 형상·받침면을 확정하지 않는다.

재질의 직렬 ScriptableObject 두 개는 부록 자산 8·9의 `UnityEditor.Rendering.Universal.AssetVersion`(script GUID `d0353a89b1f911e48b9e16bdc9f2e058`)와 `UnityEditor.Rendering.BuiltIn.AssetVersion`(GUID `639247ca83abc874e893eb93af2b5e44`)로 소스 식별된다. 해당 선언은 version 필드만 가지며 선언된 로드 콜백은 없다. 이것은 Unity 전체 로드·임포터 콜백 부재를 보증하지 않는다. 실행 준비 시 소스/meta hash와 실제 로드 타입을 다시 대조하고 새 미해결 타입은 차단한다.

### 9.3 현재 파일 사실과 이후 관찰 관문

| 구분 | 지금 파일로 확인한 사실 | 이후 승인된 읽기·격리 관찰에서 요구할 값 |
| --- | --- | --- |
| 계층·정적 타입 | GameObject/Transform/MeshFilter/MeshRenderer/SphereCollider 각 1, 자식 없음. Prefab YAML상 MonoBehaviour/Animator/Rigidbody/Light/ParticleSystem 없음 | persistent 객체의 정확 전체 타입·null 슬롯·mesh/material 참조, 예상 수와 동적 타입 일치. SkinnedMeshRenderer/Actor/Graph는 허용하지 않음 |
| 원형 좌표 | root 위치 0, 회전 identity, scale 1. ModelImporter globalScale=1, useFileUnits=1, useFileScale=1, importAnimation=0 | X 오른쪽/Y 위/Z 앞, Unity native units로 local/world TRS·pivot 및 mesh/Renderer 외곽을 각각 기록. 모델 제작 단위나 실제 미터 치수는 별도 근거 없이 확정하지 않음 |
| 렌더·표면 | Renderer.enabled=1, material 1개. AlphaClip/TransparentCutout, Enable_Emission=0 | submesh/slot/mesh 존재, 유한 외곽·native 크기, 보이는 장작·돌 등 실제 형상은 촬영 후 판독. 활성 Renderer 외곽은 지지 면적이 아님 |
| 충돌 형상 | SphereCollider.enabled=1, trigger=0, center=(0,-0.11,0), radius=0.56896544 | Collider 중심/반경·local/world 외곽을 렌더 외곽과 따로 기록. 최고점 Y=0.45896544는 구의 산술값일 뿐 받침면이 아님 |
| mesh 읽기 | ModelImporter isReadable=0 | 읽기 불가 형상/삼각형 자료는 미지원으로 기록. importer/readability 수정·reimport·대체 mesh·Sphere 기반 평면 보충 금지 |

persistent 자산의 `activeInHierarchy`는 열린 Scene의 활성 증거로 쓰지 않는다. Prefab root 안의 `activeSelf` 조상 경로와 `Renderer.enabled`로 검사한다. persistent 원본 SetActive나 참조 수정은 없다. 전체 직접/전이 의존 경로·GUID·fileID·파일/meta hash를 후속 입력 사본으로 고정하며, 해석 불가 script/누락 파일/null 컴포넌트/동적 타입 불일치가 있으면 제거·보정하지 않고 해당 후보를 중단한다. 새 사용자 스크립트의 초기화 콜백이 발견되면 로드·복제 진행 전에 개발에 반환한다.

### 9.4 미리보기 준비 계약 — 원 PNG 2장만

개발의 연구 수용·작업 명세 재결속·단독 Editor 슬롯 배분 이후에만 다음 절차를 구현·실행한다.

1. stopped/compilefalse/updatingfalse, 열린 Scene·dirty·선택·Scope/env·기존 Preview 신원·Graph 목록·Console cursor를 확인한다. Scene/Packages2/Save2 파일 5개, 자산 의존 파일/meta와 소비 Assembly, 전체 대상 객체 직렬 상태를 시작/종료 같은 식으로 비교한다. 예상외 dirty·대화·비소유 변경이 있으면 조작하지 않고 반환한다. 과거 객체 수나 Graph0을 현재 관찰로 대체하지 않는다.
2. persistent 관문 통과 뒤 자기 PreviewRenderUtility의 Scene 한 개에서 정확 Prefab 복제본 하나만 만든다. 정적 Mesh만 허용하며 Actor/skin/Animator/Graph 조작은 없다. 원본과 복제본의 native TRS/활성/Renderer·Collider·재질을 비교한다. 정렬·자동 lift·축소·원본 설정 제거는 하지 않는다.
3. native scale 1을 유지하고 동일 중심/직교 크기·yaw 35도·중립 조명에서 위쪽 관찰각 25도와 65도 각 768×768 PNG 한 장을 저장하는 준비안이다. 중심/직교 크기는 최초 유한 Renderer 외곽에서 한 번 결정해 두 장에 고정한다. 실측값·카메라/빛·시각을 남긴다. 잘림/단색/오류 발생 뒤 임의 카메라 재맞춤이나 추가 촬영은 없다. 기존 냄비 화면과 같은 이미지 축척이라고 주장하지 않는다.
4. 필요 priming은 복제본 생성 전 빈 자기 Preview에서 1회 수행하고 PNG로 저장하지 않는다. 본 촬영은 정확 원 PNG 2장만이며 crop/합성/색보정/내용물·냄비 추가/동작은 없다. 두 PNG를 직접 열어 형상·잘림을 판독하고 PNG 인코딩/크기/hash를 기록한다. 원형에서 보이지 않는 표면은 미확정이다.
5. 자기 복제본/Preview/texture와 자기 로그 관찰만 finally에서 정리하고 정리 결과를 기록한다. 비소유 Scene/Preview/Graph 전체 정리, 선택 변경, Play/Save/복구/재시작은 없다. 실패·부분 PNG·기존 실패 코드/관찰은 보존한다. 정리 실패는 임의 강제 정리나 재시도 없이 현재 상태와 함께 반환한다.

### 9.5 기존 렌더 코드의 제한적 재사용과 오류 관문

읽기 기준 소스는 Unity `artifacts/local/validation/overnight-cross-domain-visual-d382/herbal-pot-replacement-d385/inspect-preview-r3.cs`(SHA `D2B5B496E7DF225C2CA82DAD1895AB365AB5D2565DF3354001B92494CA0CE3A4`)와 `artifacts/local/validation/overnight-cross-domain-visual-d382/animation-locomotion/visual-review/render-target-repair-r113/이동연속Capture.cs`(SHA `46E75E8BC6C7F44B692DFBFFE745AF76B28A46C86CECD379AD7BD1ED720862B2`)다. 두 원본 모두 불변이며 새 스크립트를 지금 작성하지 않는다.

- 옛 `inspect-preview-r3.cs`는 Begin→Render→End 직렬 호출이므로 Render 예외 시 End 짝을 보장하지 않고 Console 오류 감지도 없다. 파일 저장도 Exists 확인 뒤 WriteAllBytes이므로 원자적인 새 파일 생성 조건이 아니다. 예전 촬영 성공이 이 안전 계약까지 검증했다는 뜻은 아니다.
- 새 준비판은 r113의 `RenderOwnedTexture`·`ObserveRenderLog`·`RequireNonUniformPixels`의 **정적 렌더 수명 관문만** 좁게 재사용한다. 이동 평가/Actor/Graph/연속 372장·A/B 출력 구조는 가져오지 않는다.
- 자기 PRU 카메라의 `targetTexture`를 이전 null/다른 값으로 복원하지 않는다. 그 연결/해제는 PRU 소유다. Begin 성공 직후 target nonnull·768×768과 실제 RawValue를 확인한다. 외부 `RenderTexture.active`는 원래 값으로 finally 복원한다.
- 고정된 이름의 내부 로그 관찰을 Begin/Render/End 구간에만 연결한다. 첫 Error/Exception/Assert 또는 예외 뒤 Encode/PNG 쓰기·다음 뷰를 금지한다. Begin 성공이면 오류 여부와 무관하게 finally에서 End를 정확히 1회 시도하고, End 실패까지 원래 실패와 함께 보존한다. 로그 관찰 해제와 반환 texture 정리도 finally로 보장하고 관찰 가능한 정리 결과를 남긴다.
- 본 촬영의 픽셀 RGBA가 정확히 모두 같으면 거부한다(허용오차/비율 기준 없음, 빈 priming 제외). 단색이 아니라는 사실은 대상 표시·형상·지지 적합성 통과가 아니며 직접 열람은 별도다.
- 파일 저장은 `FileMode.CreateNew`만 사용하고 원파일 덮어쓰기를 거부한다. 출력 절대 경로/조상 reparse·기존 파일을 확인한다. 준비 상한은 PNG 각 4MiB, 두 장 합계 8MiB, 기록 32MiB, 여유 1GiB, 전체 협력 시간 60초로 제안한다. 단일 Render의 선점 중단을 보장하지 않는다. 첫 실패/시간 초과 뒤 자동 수정·재실행은 없으며 상한은 게임 규칙이 아니다.

### 9.6 소유 제안·수용 조건·미검증

미래 출력 제안은 Unity `artifacts/local/validation/overnight-cross-domain-visual-d382/herbal-pot-replacement-d385/heat-support-preflight/`다. **현재 디렉터리 생성·쓰기 허가가 아니다.** 개발이 검토 후 정확 두 외부 준비 파일(검사/촬영 코드 1개와 동결 입력 계획 1개)의 파일명·hash·소유 경로 및 이후 실행 슬롯을 별도로 배분한다. 공통 Runtime/카탈로그/Service/Assets/Scene/원본 Prefab은 이번 소유가 아니다.

수용 대상은 단일 자산 식별·지원 타입·보존·격리 외형 촬영 절차다. 현재 완료한 것은 파일 읽기와 이 Draft뿐이며 AssetDatabase 로드/실측/PNG/컴파일/시험은 NotRun이다. 후속에도 다음은 별도 미검증으로 남긴다.

- 냄비와 모닥불 조립, 상대 TRS·지지 접촉면/법선·안정성·매몰/부유 판단.
- Player 접근/상호작용 기준점/거리, 열원 배치 계보와 실제 표면 공급.
- 불/연료/가열·용량·시간·달이기/음용, Service Confirm, 실제 Session/제품 표시 소비 연결.
- Game View·청취·E 승격. 결과 라벨은 **격리 정적 자산 외형 — 실제 Game View/냄비 지지 성공 아님**이다.

기존 수면불 0.65배·AlignGround 호출, Sphere 최고점의 받침면 대체, 누락 계약의 합성 값 보충은 금지한다. 후속 지지 검증에는 승인된 표면 의미·접촉 형상과 냄비 상대 배치 입력이 별도로 필요하며 이번 외형 조사에서 새 규칙으로 확정하지 않는다.

## 10. 개발 제한 수용 — CampFire01 외형 사전검사 준비

개발 통합 담당 `01a02198-8b2a-7491-ac93-366b30ff474c`는 9절까지 전체 SHA256 `FE6ABEB966CBC6E15377F85FBE0CDAA0FCABAA83D23F61C4A84D692109D5759F`의 새9절을 직접 읽고 기존 D385/HB01 승인 및 열원 계약·서비스와 대조했다. 기존 1~9절은 원문 그대로 보존한다. **Accepted 범위는 9절의 정확 CampFire01 단일 정적 자산의 의존·타입 관문과 native 외형 PNG2장 준비 절차뿐**이다.

- 공간에게 외부 `heat-support-preflight/probe.cs`와 `input-plan.json` 두 준비 파일만 배분한다. 코드 전체 검토·hash/실행 명세 재결속·단독 슬롯 통보 전 Editor/컴파일/자산 로드/렌더는 없다. 연구의 준비 수용을 실행 완료나 지지 성공으로 세지 않는다.
- 원본 불변, 자기 Preview 수명, 첫 렌더 오류/단색 중단, Begin/End/texture·로그/외부RT 정리, CreateNew와 60초/PNG8MiB/기록32MiB 상한을 유지한다. 비소유 Preview는 개수만 아니라 신원·직렬 상태를 가능한 같은 읽기식으로 대조하며 미계측 캐시는 보존을 주장하지 않는다.
- Mesh가 읽기 불가면 삼각형·지지면 자료는 미검증으로 남긴다. import/readability 변경·자동 lift·구형 Collider를 평면으로 바꾸기·냄비 조립·열원/Actor/Graph/Service 호출은 없다. 본 촬영 전 빈 priming 최대1회는 폐기하며 실제 산출물은 25/65도 원PNG2장뿐이다.
- 기존 Town CookingPot01 선정/Mug01 및 수면 연출 경로는 변경하지 않는다. 실제 받침면·냄비 상대 TRS·접근 기준점과 같은 상태판본의 열원 연결은 후속 책임이며 이번 외형만으로 결정하지 않는다. 게임 규칙·새 제작·SceneSave·E 승격·commit/push는 없다.

## 11. Draft — r125 추가 패키지 의존 식별과 분리 관문 제안

이 절은 **미수용 Draft**이며 의존 허용 목록 확대나 재실행 승인이 아니다. r127 HB01 명세 `eng/execution-ledgers/work-orders/nature-herbal-hb01-contents.e7-work-order.json` SHA-256 `208B9D69F862B8BB2D66678C1D4B5EBF78CC269E63CB9D6F9484393158A02F9D`의 `d385PotVisualReplacement.heatDependencyReviewPreparation`에 따라 작성한다. 기존 1~10절·부록의 전체 바이트 접두부 37,419 bytes / SHA-256 `CD67FE45CFDECD8D7A8C9ED98AFDA8C47B7AD9D5C3FE06402320CED268877012`는 불변이다. 쓰기는 이 새 절과 이 문서만 지정한 Fast 신규 로그에 한정한다.

### 11.1 실제 실패와 판본 이력

원자료 기준 디렉터리 `R`은 `C:/Users/user/ssalddel/artifacts/local/validation/overnight-cross-domain-visual-d382/herbal-pot-replacement-d385/heat-support-preflight/`다. r125의 `R/capture-r124/dependencies.json`은 직접 의존 2개(model·PolygonNature_01.mat), 전이 의존 14개를 기록한다. 기존 계획의 허용 7개와 차집합은 아래 추가 7개다. 첫 차단은 `UnreviewedDependency:Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/UniversalMetadata.cs`였다.

- r125는 메모리 Compile1/정확 Assembly Run1 이후 **Prefab Load 전 차단**됐다. persistent 관찰/복제/PRU/priming/Render/PNG는 모두 0 또는 NotRun이다. 결과의 `originalObservationEqual=false`는 원본 미관측이며 원본 변경 증거가 아니다.
- 실행 명세 SHA `884403B048E4C89D8D9655F94C31C5E2B9F1F5059B35D90D8714CB024C7733CC`는 당시 실행 근거다. 종료 후 결과 결속 r126 SHA `B1A4BDF31DEA51F564E798F2C033FB71142DDB40E0B0A60A4A5617F1707E550A`와 현재 r127 문서 준비 판본을 실행 중 파일 변경으로 합산하지 않는다. 실행 권한은 소진됐으며 이번 절에서 복구하지 않는다.
- 원자료 `R/slot-r125/handoff.md` SHA `4D929A6E2DC43043D330785A740BBDDF138E7D9C4885924C5927F1F42B224200`, `manifest.json` SHA `1B054E473E0E37DE2321FD88F620244E6E1EEEB6D53FD062CE6E4174E58D51D6`, 27파일 hash·길이 일치를 보존한다. 전후 객체/Scene/Graph/선택/파일/Assembly/창 비교와 Console 신규0은 해당 읽기 범위의 보존이며 비직렬 캐시 전체 불변 증명이 아니다.
- 원 `probe.cs` SHA `BB40B4D2D7297108632E70CA6DC14740938A46CE4291BBE645970F9C4E97BC11`, `input-plan.json` SHA `E691BF5D131A3DC80BA31C2AB8431CC8B428DB547670213A1FDA86DA240263D6`, `execution-loader.cs` SHA `7957410A1FF4F80D5E9A37497D93635A582C6A5CBE093EA9825FA0D82EE5A7A7` 및 원27파일은 수정하지 않는다.

### 11.2 정확 14 AssetPath 집합과 실제 패키지 위치

다음 목록은 r125 원 관찰을 그대로 분류한 **후속 동결 입력 제안**이다. 지금 실행 허용 목록에 반영하지 않는다. 파일 열거 순서와 집합 일치는 별도로 검사하며, 동일 basename이나 패키지 디렉터리 전체 허용으로 대체하지 않는다.

| 구분 | 정확 AssetPath |
| --- | --- |
| 기존 1 | `Assets/Synty/PolygonGeneric/Shaders/Generic_Basic.shadergraph` |
| 기존 2 | `Assets/Synty/PolygonNature/Materials/Alts/PolygonNature_01.mat` |
| 기존 3 | `Assets/Synty/PolygonNature/Models/SM_Prop_CampFire_01.fbx` |
| 기존 4 | `Assets/Synty/PolygonNature/Prefabs/Props/SM_Prop_CampFire_01.prefab` |
| 기존 5 | `Assets/Synty/PolygonNature/Textures/PolygonNature_01.png` |
| 기존 6 | `Packages/com.unity.render-pipelines.universal/Editor/AssetVersion.cs` |
| 기존 7 | `Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/AssetVersion.cs` |
| 추가 1 | `Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/UniversalMetadata.cs` |
| 추가 2 | `Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat` |
| 추가 3 | `Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader` |
| 추가 4 | `Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/BuiltInMetadata.cs` |
| 추가 5 | `Packages/com.unity.shadergraph/Editor/Importers/ShaderGraphImporter.cs` |
| 추가 6 | `Packages/com.unity.shadergraph/Editor/Importers/ShaderGraphIndexedData.cs` |
| 추가 7 | `Packages/com.unity.shadergraph/Editor/Importers/ShaderGraphMetadata.cs` |

기존 7개의 파일/meta 기준선은 9.2절 및 부록 자산 8·9와 원 입력 계획을 유지한다. 아래 추가 7쌍과 합쳐 14개 각각의 AssetPath·물리 경로·파일/meta hash·GUID를 별도로 결속할 준비안이다.

| 논리 패키지 루트 | 파일 읽기로 확인한 물리 루트 | package.json 판본 |
| --- | --- | --- |
| `Packages/com.unity.render-pipelines.universal/` | `C:/Users/user/ssalddel/Library/PackageCache/com.unity.render-pipelines.universal@73b4c4ff130e/` | version `17.5.0`, unity `6000.5` |
| `Packages/com.unity.shadergraph/` | `C:/Users/user/ssalddel/Library/PackageCache/com.unity.shadergraph@8d7376ec43ee/` | version `17.5.0`, unity `6000.5` |

물리 파일 경로는 위 논리 루트를 물리 루트로 치환한 정확 경로다. 캐시 접미사는 패키지 version이 아니다. `unity=6000.5`는 패키지 선언값이며 이 문서 작성 중 Editor 버전을 새로 조회한 결과가 아니다. URP는 core/shadergraph/universal-config 17.5.0, ShaderGraph는 core17.5.0/searcher4.9.3 의존을 선언한다. 이 선언은 관련 패키지 전체를 검토·허용했다는 뜻이 아니다.

### 11.3 추가 7개의 파일·meta fingerprint

번호는 11.2절의 추가 번호다. 아래 GUID는 각 파일의 meta에서 읽은 값이다. 패키지 소스/meta를 디스크로 읽었으며 AssetDatabase 조회·로드·임포트는 수행하지 않았다.

| 추가 | GUID | 파일 SHA-256 | meta SHA-256 |
| --- | --- | --- | --- |
| 1 UniversalMetadata | `d2654ad3c17ab8a4ebb9fd0719795d21` | `88083201BA4B354BDE926DECEDD11354A32B41A69E49EFCD08EABE095CB15A5A` | `665C03A7A47EB649A052B5FCE4B70FC20CDE2177E6FBD7026F67E9A888A3B5D4` |
| 2 Lit.mat | `31321ba15b8f8eb4c954353edc038b1d` | `A79F6EE52F6C31FC482ABF73B04F915EA3DFF0CBFE8EE7351E4F5A57AA94E8F1` | `A96F8B5C6B76D1FEFE7B4CD7D75120B07CB97338810776D2161D291EB2886EA8` |
| 3 Lit.shader | `933532a4fcc9baf4fa0491de14d08ed7` | `D012FADD60A3E5C19A57D501A3E010AD0EE6067465FEE135A421AFEA08B4DA45` | `F0DB005307FFE5480C40A402CA5BFB0B3726259031A86E255E49626228CEB008` |
| 4 BuiltInMetadata | `44dc38476d77dd54d91833b3d57ee8b8` | `8D0C765CE2D02DAF1A6D9C4F475C57A4E7CA62AE5C319350C784BC8BFDB9BAF2` | `F06197F0B2412A26B4400A4853820A318B77B597269E39031D3A970FDCEEAC12` |
| 5 ShaderGraphImporter | `625f186215c104763be7675aa2d941aa` | `4E4B9D5808FAB134E1D25B096439238D00C65D81B514EBEF3420A37976476B6C` | `73774735365002E56ECAA55327A951B262672984C5041E493087B9A2FA1A448A` |
| 6 ShaderGraphIndexedData | `6c86adf534154b62940167041ccb0bee` | `2D2D607D8C1FA5A76E50B1CD13DDE3B703891B4EAE7E8E63ACEA9D376143A130` | `329EB4DEC32E44218FC2215B883D82734B46157A2F4A5EBE5376E19E596C88C5` |
| 7 ShaderGraphMetadata | `b64ab828cd6c5b3479a4c575ca6617d5` | `C86716F1ECC1257883B2C67DDCB00D612D7B302E215E8BBAEB8E919F16202385` | `1D971D0D921DD2ABACD6FC997B2B7B787B69F5F2A67988217324ADEA16B8CF64` |

### 11.4 타입·역할·콜백과 조사 한계

| 추가 | 소스에서 식별한 타입/역할 | 콜백 및 한계 |
| --- | --- | --- |
| 1 | `UnityEditor.Rendering.Universal.ShaderGraph.UniversalMetadata`, sealed ScriptableObject. ShaderID/SurfaceType/AlphaMode/그림자/material override 등 target metadata 직렬필드/get-set | 전체 선언에서 자체 OnEnable/OnValidate/OnBeforeSerialize/OnAfterDeserialize 선언 없음. 기반 클래스·Unity 임포트 전체의 무콜백 보장 아님 |
| 2 | NativeFormatImporter의 Material `Lit`, opaque queue2000, texture 참조 fileID0. shader GUID는 추가3과 일치 | 포함된 `!u!114`는 기존 `AssetVersion` scriptGUID `d0353a89b1f911e48b9e16bdc9f2e058`/version10이며 Scene MonoBehaviour 아님. Material에 C# 콜백 선언이 없다는 사실로 임포터/후처리 안전을 단정하지 않음 |
| 3 | `Universal Render Pipeline/Lit`, ShaderImporter. ForwardLit/ShadowCaster/GBuffer/DepthOnly/DepthNormals/Meta/Universal2D/MotionVectors/XRMotionVectors pass | HLSL include, FallbackError, `UnityEditor.Rendering.Universal.ShaderGUI.LitShader` CustomEditor가 있음. 관련 구간/검색 판독이며 HLSL·CustomEditor·전체 shader 기능 안전 인증 아님 |
| 4 | `UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInMetadata`, sealed ScriptableObject. ShaderID metadata | 전체 선언에서 자체 lifecycle/직렬화 콜백 없음. 기반/네이티브 동작은 별도 |
| 5 | `UnityEditor.ShaderGraph.ShaderGraphImporter : ScriptedImporter`, `[ScriptedImporter(133, Extension,-902)]`, Extension=`shadergraph` | **실제 `OnImportAsset` 콜백 존재**. 관련 생성/의존 구간을 읽었으며 전체 VFX/Generator/target 기능 검증 아님 |
| 6 | `UnityEditor.ShaderGraph.ShaderGraphIndexedData : ScriptableObject`, 검색용 DataBag metadata | 전체 선언에서 자체 lifecycle/직렬화 콜백 없음. DataBag 내부 구현은 이번 범위 밖 |
| 7 | `UnityEditor.ShaderGraph.ShaderGraphMetadata : ScriptableObject`, outputNodeTypeName/assetDependencies/categoryDatas | 전체 선언에서 자체 lifecycle/직렬화 콜백 없음. 동파일 일반 자료형의 입력 처리/정렬은 Scene 동작이 아니며, export 의존 참조를 보존하는 역할 |

Importer의 `GatherDependenciesFromSourceFile`(109행 부근)은 최소 graph 의존을 모으며, `OnImportAsset`(226행 부근)은 역직렬화 후 `GraphData.OnEnable()`/ValidateGraph와 shader 생성을 호출한다. 이 `GraphData.OnEnable()` 호출을 Importer 자신의 MonoBehaviour OnEnable 선언으로 오해하지 않는다. `BuildAllShaders`에는 Generator/ShaderUtil.CreateShaderAsset, Material 및 shader 하위 자산 생성 경로가 있다.

287~294행의 target metadata, 303~318행의 ShaderGraphMetadata와 export용 assetDependencies(`AssetDatabase.LoadAssetAtPath` 포함), 376~384행의 category/index metadata, 391/408행의 source/artifact 의존 등록은 **Editor metadata/export 참조도 의존 목록에 들어갈 수 있는 역할 근거**다. 14개 의존을 Prefab 실행 컴포넌트 14개로 해석하지 않는다. 자체 OnBeforeSerialize/OnAfterDeserialize 선언이 검색되지 않는다는 사실로 `OnImportAsset`나 다른 대상/기반 클래스 콜백 부재를 주장하지 않는다.

직접 model·PolygonNature_01.mat 참조와 Lit.mat→Lit.shader GUID 연결은 확인했다. 그러나 **이 특정 graph에 기본 URP Lit.mat가 유입되는 중간 target/generator/cache 연결은 미확정**이다. 실제 CampFire 할당재질을 Lit.mat로 대체할 근거가 아니다. r125 GetDependencies 또는 향후 cached load가 내부적으로 import를 일으키는지, 네이티브 캐시를 바꾸는지는 이번 파일 읽기로 입증하지 못했다. 전체 콜백 없음·캐시 무변경·전체 셰이더 안전을 일괄 인증하지 않는다.

### 11.5 후속 준비안 — 컴포넌트 허용과 의존 fingerprint의 분리

다음은 개발의 제한 수용·새 명세·정확 외부 파일 배분이 필요한 설계 제안이다. 현재 code/plan/loader/허용 목록에는 적용하지 않는다.

1. **Prefab 구성 관문**은 기존 정확 4타입 `UnityEngine.Transform`, `UnityEngine.MeshFilter`, `UnityEngine.MeshRenderer`, `UnityEngine.SphereCollider` 및 예상 개수·null/동적 타입 거부를 그대로 유지한다. 패키지 metadata 4종이나 ScriptedImporter를 허용 component로 추가하지 않는다. GameObject 컨테이너와 Component 타입 수도 구분한다. skin/Actor/Animator/Graph 작성 권한을 추가하지 않는다.
2. **의존 파일 관문**은 11.2의 정확 14 AssetPath 집합과 각각의 파일/meta SHA-256·GUID, 실제 물리 경로, 두 패키지 version/unity 선언 및 package.json fingerprint를 후속 입력에 결속한다. 기존 Packages manifest/lock 보호도 유지한다. 이 계층은 파일 신원 검토이지 해당 타입의 임의 생성/호출 권한이 아니다. 패키지 전체·접미사·확장자 wildcard 허용은 금지한다.
3. 실행 전/후 같은 집합·각 fingerprint를 대조하여 **추가/누락/drift/중복·해석 불가 경로를 거부**한다. 경로 재해석 실패나 버전 차이를 현재 값으로 자동 갱신하지 않는다. 실제 의존 재조회는 새 실행 승인 후에만 가능하며 이번 문서 준비에서는 수행하지 않는다. 암묵 import나 캐시 변화가 관찰되면 실패/관찰 한계로 남기고 재시도·자동 복구하지 않는다.
4. 원본의 실제 material·sharedMesh/sharedMaterial 등 직접 참조, native TRS/활성·Renderer/Collider·읽기 가능성 검사는 완화하지 않는다. Lit.material fallback, metadata 인스턴스 생성, 직접 `ImportAsset`/`Refresh`/reimport, importer/readability 변경은 금지한다. 참조를 새 자산으로 치환해 관문을 통과시키지 않는다.
5. Scene/전체 객체·선택·비소유 Preview 신원/Graph·Scope/env·파일5/의존/Assembly/기존 자동 PNG·Console 전후 보존, 첫 오류 중단, 자기 PRU/texture/log 수명, CreateNew, PNG IHDR·768²·정확 단색 거부와 60초/PNG8MiB/기록32MiB 및 외부 slot 예산 분리를 유지한다. 미계측 내부 캐시/암묵 callback은 이 보존 비교의 통과 범위에 끼워 넣지 않는다.

### 11.6 이번 반환과 다음 승인 경계

이번 완료 범위는 r125 원자료 판독·추가 7개 파일/meta 및 패키지 선언 조사·새11절 Draft·문서 정적 검증뿐이다. 원27파일/Probe/plan/loader/기존1~10절은 보존하며, 새 코드/입력 계획/Editor 조회/Load/Render/실행 승인·허용 목록 수정은 0이다. 최종 문서 hash와 기존 접두부 hash·문서 전용 Fast 결과를 개발에 반환한다.

개발이 이 Draft 전체를 검토하여 제한 수용 여부를 판단하고 명세/hash를 재결속한 뒤에만 새 준비 파일과 이후 단독 실행을 별도로 배분할 수 있다. 현재 외형 PNG는 여전히 0이며, 실제 CampFire 형상·냄비 받침면·상대 TRS·접근·불/연료/가열·서비스/Session 연결·Game View·청취·E 승격은 미검증으로 유지한다.

## 12. 개발 제한 수용 — 의존 파일 검사와 정적 구성요소 검사 분리

개발 통합 담당 `01a02198-8b2a-7491-ac93-366b30ff474c`는 11절까지 SHA256 `56F0C62BEF691D335B5CACDBE0F733487CFA129DB10FFEF6B3B1ED7AE3049EFB`의 새11절 전체와 r125 원보고를 읽었다. 기존 37,419bytes 접두부가 `CD67FE45CFDECD8D7A8C9ED98AFDA8C47B7AD9D5C3FE06402320CED268877012`와 같고, 실제14개 파일/meta28개 hash가 표/기존 입력과 일치함을 직접 확인했다. **Accepted 범위는 아래 외부 준비 코드의 검사 분리 설계뿐**이며 이 수용으로 r125를 통과 처리하거나 재촬영하지 않는다. 기존 1~11절 원문은 보존한다.

- 정확14개 AssetPath의 집합과 파일/meta·GUID·물리 경로를 고정한다. URP/ShaderGraph 두 package.json의 name/version/unity 및 파일 SHA도 보호한다. 현재 package.json SHA는 각각 `0DEA509A00D1A574E51BD706A99ED926DA5545C31FAFDA76FF3C531811F37488`, `871DDE0EA8F9AF4DFBB400B86E527A5C8F9DFF5501EEAD7A62D2F0997E951ADB`다. 기존 Packages manifest/lock 및 자산 보호는 유지한다. package wildcard/단순 경로 포함 검사로 대체하지 않는다.
- 정적 Prefab의 Transform/MeshFilter/MeshRenderer/SphereCollider 정확4타입·개수, null/동적 타입 거부, 원본 실제 mesh/material/native TRS와 지원 설정은 그대로다. 패키지 의존 허용을 metadata/Importer 인스턴스 생성이나 재질 대체 권한으로 쓰지 않는다.
- 후속 단일 호출에서 최초 의존 검사를 통과한 경우에만 종료 의존 사본을 한 번 더 수집하는 준비안을 허용한다. 종료 조회는 소유 객체 정리와 다른 보존 검사를 건너뛰게 하지 않으며, 고정 오류 관찰 구간 안에서 수행한다. 최초 검사 실패 시 같은 검사를 반복하거나 목록을 확장하지 않는다. 추가/누락/중복/경로·GUID·파일/판본 drift는 실패로 반환한다.
- ShaderGraphImporter의 OnImportAsset와 graph/target 생성 경로가 존재한다. 암묵 import·캐시 변경의 완전한 부재, Lit.mat 유입 중간 경로, HLSL/CustomEditor 전체 안전을 인증한 것은 아니다. 직접 ImportAsset/Refresh/reimport/metadata 생성·원본/Packages 수정은 하지 않는다. 후속에서 관측한 변화는 실패 또는 명시 한계로 남긴다.
- 새 준비 파일은 별도 `heat-support-preflight/dependency-fingerprint-r128/probe.cs`와 `input-plan.json` 두 개만으로 한정한다. 원 probe/plan/loader 및 r12527파일은 변경하지 않는다. 새 코드·계획 전체 검토와 실행 명세·단독 슬롯 통보 전 Compile/Run/Load/Render는 없다.
- 기존 원PNG2장·카메라/native·첫 오류/단색 중단·수명·예산·Scene/Preview/Graph/Scope/env/파일/Console 보호와 가열/지지/접근/Game View/E 상한은 불변이다. 패키지 파일의 신원 검토는 새로운 게임 기능·조립 배치 승인이 아니다.
