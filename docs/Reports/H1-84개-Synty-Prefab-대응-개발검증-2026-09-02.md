# H1 84개 Synty Prefab 대응 개발 검증

- 기준일: 2026-09-02
- 범위: 파일·GUID·팩·fingerprint·기존 이미지 계보 대조와 결손 가공 계획
- 기준 catalog: `eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json`
- catalog SHA256: `2FF340257EDABC08CA93AFE9E7649C4E5EC5A25BC12571B6973926E39F945108`
- 기존 사람용 목록: [H1 84개 문답 대응 개발 검증](H1-84개-문답대응-개발검증-2026-08-31.md)
- 기존 시각 목록: [H1 84개 시각 목록](H1-84개-시각목록-2026-08-31.md)

## 1. 결론

이번 조사는 84개 목록 자체를 복제하는 새 대장이 아니다. 위 두 기존 보고서의 1~84 번호와 stable ID를 그대로 사용하고, 그 위에 자산 조사 결과만 덧붙이는 증분 대응표다.

- 기능·상호작용 H1 52개와 표현 카드 32개, 합계 84개를 다시 대조했다. stable ID 중복과 표현 카드의 깨진 `supportsInteractionH1Refs`는 0건이다.
- 우선 10개를 심층 대조했다. `CompositeRequired` 8개, `NeedsReview` 2개, `DirectUse`·`DedicatedAssetRequired` 확정은 각각 0개다.
- 나머지 74개는 `Uninspected`다. 이는 자산 부재나 신규 제작 필요 확정이 아니다.
- 정확 H1 ID를 가진 기존 격리 조립 이미지는 #8과 #15 두 기능 H1뿐이다. #30·#73·#37·#29에는 기존 문법 조립 이미지가 있지만 기능 연결이나 제품 Scene 증거는 아니다.
- 모든 후보는 Presentation 후보다. H 승인, WI 연결, 제품 Scene 배치, E5, 실제 Game View를 증명하지 않는다.
- Blender, Unity Editor, Scene, Prefab 저장, 신규 캡처, 구매, 패키지 변경, commit, push는 수행하지 않았다.

판정 코드는 다음 의미로만 사용한다.

| 판정 | 의미 |
| --- | --- |
| `DirectUse` | 한 보유 Prefab이 역할·상태·접근 조건까지 직접 충족한다는 파일 근거가 있음 |
| `CompositeRequired` | 여러 보유 Prefab 또는 기존 조립물을 함께 써야 함 |
| `StateVariantPossible` | 기존 자산 교체로 상태 차이를 표현할 가능성이 있으나 실제 상태 연결은 미검증 |
| `DedicatedAssetRequired` | 보유 조합으로 해결되지 않는다고 확인되어 전용 가공이 필요함 |
| `NeedsReview` | 보유 후보는 있으나 역할·구조·판본 결손 때문에 위 셋 중 하나로 아직 고정할 수 없음 |
| `Uninspected` | 이번 우선 묶음에서 Prefab 수준 대조를 하지 않음 |

## 2. 전체 84개 대응 범위

전체 84개 각각의 사람용 이름·역할·상태·원문 경로·WI 또는 지원 H1 연결은 기존 [문답 대응 표](H1-84개-문답대응-개발검증-2026-08-31.md#31-h1-84개-목록)와 [시각 목록](H1-84개-시각목록-2026-08-31.md#4-h1-84개-시각-목차)이 소유한다. 이번 대응 결과는 다음과 같이 그 번호에 조인한다.

| H1 번호 | 결과 | 비고 |
| --- | --- | --- |
| #8 | `CompositeRequired` + `StateVariantPossible` | 농업 생산구획. 혼합 작물 조립과 감자 S/M/L 상태 후보는 있으나 상태 사본 연결 미검증 |
| #12 | `NeedsReview` | 농기구 보관. Rack·ToolBox·Crate·도구 조합 후보, 전용 보관/반환 Anchor 없음 |
| #15 | `CompositeRequired` | 수확·집하 작업마당. 기존 격리 모판은 있으나 제품 작업 동선 미검증 |
| #16 | `NeedsReview` | 작업자 대기. Bench·Table·Chair 조합 후보, 대기·휴식·집결 Anchor 없음 |
| #29 | `CompositeRequired` | 자연 탐색·완충 공간. 숲 빈터·고사목 조립 후보, 자원 상태·진입·복귀 미결속 |
| #30 | `CompositeRequired` | 숲 경계형 농장 전환. Nature/Farm 혼합 조립 후보, 전환·접근 미결속 |
| #37 | `CompositeRequired` | 자연 탐색 출발지. 산길·바위 길목 후보, 통행·안내·도구 수령 미결속 |
| #65 | `CompositeRequired` | 헛간 작업마당 표현 카드. A/B/C 조립 후보, 기능 H1과 별도 |
| #66 | `CompositeRequired` | 혼합 작물밭 표현 카드. A/B/C 조립 후보, #8의 제품 상태 증거 아님 |
| #73 | `CompositeRequired` | 숲 가장자리 표현 카드. 표현 후보이며 #30 완성 증거 아님 |
| #1~#7, #9~#11, #13~#14, #17~#28, #31~#36, #38~#64, #67~#72, #74~#84 | `Uninspected` | 74개. 기존 정의·문답·시각 자료 유무는 두 기준 보고서에 유지 |

이 범위 표는 84개 전부를 포함한다. 번호 범위 행의 각 항목은 개별적으로 `Uninspected`이며, 묶음 판정이나 자산 부재 판정이 아니다.

## 3. Nature 우선 묶음

| 번호·H1 | 현행 정의 | 정확 주 후보 | 파일 식별 | 기존 시각 근거 | 판정·결손 |
| --- | --- | --- | --- | --- | --- |
| #30 `h1-stock:nature-farm-edge` | 숲 경계형 농장 전환 공간, `ExploratoryInventory` r2 | `NatureFarm전환_A.prefab` | `Assets/Ssalddel/Presentation/World/Generated/평창군법정동World/PackCompositionSets/mixed/NatureFarm전환_A.prefab`; GUID `057c4979f94645040b091ea8078960b8`; composition fingerprint `c0cd0a54bc3c3e6fe1883f150416d193caa4faf8d7fb88f4ebeba2b387223287`; Prefab SHA `0BD38446928FA6D1FDE0D406FE6DBC264462621715FD61C9E5CF5C7FF5C1E2DD` | `transition/07-NatureFarm전환_A/FrontThreeQuarter.png`; SHA `CCE6E767A82A6B84EBF7798737CBBAB5B3470E247B85A2558B250C24CB49970D`; manifest `151A2BD5D74AD90661AEF9D158EA3C4760BBE0D69B087506B3248797A21FF5E6` | `CompositeRequired`. 흙길·울타리·나무는 있으나 `RoadAccess`, Nature↔Farm 전환, 통행 기능이 없음 |
| #73 `h1-expression:nature:숲-가장자리` | 숲 가장자리 표현 카드, `ExploratoryInventory` r1 | `숲가장자리_A.prefab` | `Assets/Ssalddel/Presentation/World/Generated/평창군법정동World/NatureCompositionSets/숲가장자리_A.prefab`; GUID `87003de14a002e2479f8c3c975731a2b`; fingerprint `213a173ac366bc92b06a23701893ab124322d64254dba42847d4207b23d1548e`; Prefab SHA `0D8C6F99346F8414301D463C97733554820C24DECE84C21009E2849E7FA0BE3C` | `nature/19-숲가장자리_A/FrontThreeQuarter.png`; SHA `CE1997CABAE744F3FC894D642616E3C231428E8AA6A2360F49A9FBEC286A9EDB`; manifest `A72C824E35AAE628E0D44746B271FCCC889CD8FD7CBAA40E742016DF12226674` | `CompositeRequired`. 관목·하층 식생·풀 조합이나 기능 H1과 독립이고 촬영상 성김 |
| #37 `h1-stock:nature-trailhead` | 자연 탐색 출발지, `ExploratoryInventory` r1 | `산길·바위길목_A.prefab` | `Assets/Ssalddel/Presentation/World/Generated/평창군법정동World/NatureCompositionSets/산길·바위길목_A.prefab`; GUID `cbd8c63a11a8c5542966e8c4c01792db`; fingerprint `9e555de34454451af580b151d8e0a197f6ba0713ae24cfec980e8dca247c8733`; Prefab SHA `E4A14CE9459D18F59D9F6D066BA0ADC823F60CC142804D088A36BF0149D949A4` | `nature/10-산길·바위길목_A/FrontThreeQuarter.png`; SHA `70A94E9BC3EFEEC1A7E556B0DD306CE8C7934D72755EB25F9B78F8A2BB2F5410`; manifest `5178FAD005EF72E9E8BEF1ED42D35907A0805F97F14005975DEC4C89742C939F` | `CompositeRequired`. 실제 통행 입구, `RoadAccess`, `TrailOutput`, 안내·도구 수령 지점 없음 |
| #29 `h1-stock:nature-exploration-buffer` | 자연 탐색·완충 공간, `ExploratoryInventory` r2 | `숲빈터·고사목_A.prefab` | `Assets/Ssalddel/Presentation/World/Generated/평창군법정동World/NatureCompositionSets/숲빈터·고사목_A.prefab`; GUID `e0abc6039606dc549a3ede603fd9b3b7`; fingerprint `afb47737e4a94a1c73f4d3e10214d7c253ef316cfef1798076370c1aad5bfecc`; Prefab SHA `4B3607E0EF123CA9FAF410E59CFD3C09E6D2F56678843ABC6FA3BCA5576A824A` | `nature/22-숲빈터·고사목_A/FrontThreeQuarter.png`; SHA `E4F6DF9AB399807126D127CA469E609FBE60FF8214C28CC443D4B00BFBB6ECA1`; manifest `3F0C20BACA030E5293639382CBF356DBF3AB121EE90DA1940B70529C3F229869` | `CompositeRequired`. `HarvestResourceWorkArea`, 자원 상태·재생·해제, 진입·복귀 미결속 |

#30 조립의 주요 vendor 구성은 Farm 흙길 GUID `20f6bd523ebdaf04fa332e6ce565fc03`, 울타리 GUID `8157d7b10c2fa75449c20daa36513d38`, Nature 나무 GUID `dafad5d52cd284f4b8229a896f9a966f`다. Module Catalog에서 구성 자산은 모두 `needs-review`, `presentationOnly`다.

## 4. Farm 우선 묶음

| 번호·H1 | 기존·주 후보 | 정확 파일 근거 | 판정·결손 |
| --- | --- | --- | --- |
| #8 `h1-stock:farm-production` | 기존 격리 H1 모판 + #66 혼합 작물 조립 + 감자 S/M/L 상태 후보 | 기존 Front PNG SHA `B5E214C44FD5CE74DD5EA36137D3F9DE1C982C8A87C15EFD46B3D71888AB76EE`, manifest `9708E9C39D1565D9B238D0B92A8FA6087792105C43C7ADABD8A535AFBEFA92D6` | `CompositeRequired`; 작물 교체만 `StateVariantPossible`. 과거 모판은 현행 상태·접근·E5 증거가 아님 |
| #66 `h1-expression:farm:혼합-작물밭` | A/B/C 각 5 Renderer 조합 | A `.../CompositionSets/Farm/혼합작물밭_A.prefab`, GUID `ff464c590478d884c871fd43251402ca`, SHA `4DCCA16D85344BE0FA2852A1B05EB5478025290E6E719BE137F4AF20E8928E72`; B GUID `07bc0d8d3d0fbfd4783aa51f76b03545`; C GUID `67a65e907d73b95459aa2c9ee83c8eb7` | `CompositeRequired`. A/B Collider 0, C Collider 2, socket/connector 비어 있음. D443은 A의 정적 준비만 수용 |
| #15 `h1-stock:farm-work-yard` | 기존 격리 H1 모판 + #65 헛간/작업마당 조립 | Front PNG SHA `68258384A4CEA13786049E65387253404438568E921DBB7C3F053BCCE3D68BB9`, manifest `D8A1906F3954E4A63CB4F1362A261A2FE809329A7146B57EB6119DB30225662A` | `CompositeRequired`. 과거 모판과 현재 grammar/후보 동등성, 수확·포장 동선 미확인 |
| #65 `h1-expression:farm:헛간-작업마당` | A/B/C 헛간·차량·농기구 조합 | A `.../Farm/헛간작업마당_A.prefab`, GUID `6414d7a141f19df49bc9b5784ef0ee73`, SHA `9C4C3FDD8954E1DA8BD270CADDFB87845C77983509B60C09DE7E8F09853B5617`; B GUID `9907822b5d1d7314ca730f4d03d70559`; C GUID `16a6a20a94e8b8546ae928d93b7f6b0e` | `CompositeRequired`. 14~20 Renderer, 13~17 Collider와 farmer/vehicle/cargo socket 후보가 있으나 기능 H1 상태는 없음 |
| #12 `h1-stock:farm-tool-storage` | Barn A + Rack01 + ToolBox01 + Crate01 + Rake/Pitchfork/Hoe | Rack GUID `39adf611006c395439df5f752c1e4d3e`; ToolBox GUID `2d0d15b5c59b9944a994e08dcc2f55b4`; Crate GUID `17f62eefbfea0bf44a08d7be65339372` | `NeedsReview`. 저장된 검토 Scene은 Barn A를 재사용하지만 Storage·ToolCheckout·Return Anchor 없음. 방위 장비 보관으로 확대 금지 |
| #16 `h1-stock:farm-worker-waiting` | Barn/shelter + Bench01 + Table01 + Chair01 | Bench GUID `74dd79ffd92ff194d85f9c443415d31e`; Table GUID `0504778a38044d446a030c9203219e0d`; Chair GUID `0af4776ee237da14ab4a893e536ee150` | `NeedsReview`. Rest·Briefing·DefenseMuster Anchor와 제품 소비가 없음 |

주요 vendor 후보는 Module Catalog에서 모두 Farm pack의 `needs-review`다. fingerprint와 Prefab 바이트 SHA는 다른 값이다. 예를 들어 Barn01 GUID `1135f5...006b`, SHA `727ED1...3C59`, fingerprint `8e6335...5c69`; Barn02 GUID `cd3a9b...4a1c`, SHA `F8E58C...CB1A`, fingerprint `f180c0...3ae2`; Tractor01 GUID `a4d902...79a6`, SHA `830DC4...BA4F`, fingerprint `797579...dec76`이다. 전체 정확 값은 현행 Module Catalog 행과 Prefab/meta가 권위이며 이 보고서의 줄임표를 복사 식별자로 사용하지 않는다.

## 5. 기존 소비와 시각 증거 경계

- #8/#15의 2026-08-19 4시점 이미지는 `ExistingSavedScene`·`LocalPresentationEvidenceOnly`인 격리 H1 조립 검토다. 실제 Game View가 아니다.
- #30/#73/#37/#29 이미지는 canonical grammar inventory의 격리 조립 검토다. 기능 H1·제품 Scene·E5 증거가 아니다.
- `AreaSetCompositionPatternReview.unity`에는 #12/#16의 exact H1 이름과 Barn A source가 있으나 검토 Scene이다. Barn socket만으로 보관·대기 역할이 성립하지 않는다.
- #15 저장 검토 장면의 일부 표시 이름과 실제 source Prefab 사이에 판본 차이가 있다. 자동 재생성·수리하지 않고 `NeedsReview`로 남긴다.
- `평창공간문법CompositionCatalog.asset`과 결정적 배치 후보 코드가 #65/#66 경로를 읽는 사실은 제품 Scene 소비나 E5가 아니다.

## 6. 결손별 가공 계획 후보

현재 `DedicatedAssetRequired`로 확정한 항목은 없다. 모두 보유 조립을 먼저 검증할 수 있다.

1. 원본 보존: vendor Prefab의 GUID, meta hash, Prefab hash, catalog fingerprint를 동결한다.
2. 전용 복사본: 제품용 안정 H1이 필요할 때만 `Assets/Ssalddel/...` 소유 경로에 조립 복사본을 만든다. vendor 원본은 수정하지 않는다.
3. 최소 조립 보완: 배치 간격, 이름, 역할별 Anchor 자식, Collider·Bounds만 우선한다. Mesh 가공을 먼저 선택하지 않는다.
4. 상태 표현: #8은 기존 Potato S/M/L, #29는 통나무·고사목·그루터기 변형을 상태 사본과 연결할 수 있는지 먼저 검토한다.
5. 전용 가공 승격 조건: 조립 후에도 기능 판독·통행·지지·접근·상태 차이가 충족되지 않을 때만 `DedicatedAssetRequired`로 재분류한다.
6. 그 경우의 최소 순서: 원본 GUID/hash 보존 → 전용 복사본 → 필요한 부분만 분리·pivot·구멍/파손·LOD·Collider·재질 계획 → Unity 재반입 → 기존 조립 fallback 보존.
7. 검증 상한: 정적 의존성 → 격리 Prefab load/type/null → Renderer/Collider/Bounds/Anchor → 통행·접근·지지 → 상태 사본 연결 → cleanup → 실제 Game View. 앞 단계 성공을 뒤 단계 성공으로 확대하지 않는다.

## 7. 자료 정합성과 실제 자산 결손의 분리

- H1 counts 52/32/84는 실제 배열과 일치한다.
- H2/H3/H4 요약 counts `18/10/5`와 실제 배열 `38/20/6`은 드리프트다. 이번 자산 조사에서 수리하지 않았다.
- `farm-worker-waiting`은 catalog row revision 2와 정의 내부 revision 3, 정의·설명 hash 재결속 차이가 있다. 이는 자산 부재 증거가 아니다.
- `farm-tool-storage`의 원바이트 차이는 LF 정규화 뒤 기대값과 일치하므로 손상으로 판정하지 않는다.
- 표현 카드 9개의 `supportsInteractionH1Refs`가 비어 있다. 무용·삭제·Prefab 부재가 아니라 관계 미검토다.
- Q089 상태 차이와 Q272~274 원문 소실은 기존 보고의 검토 필요 상태를 유지한다.

## 8. 최종 상태

`Integrated` — 파일 기반 전체 84개 기준선과 우선 10개 자산 대응·결손 계획을 통합했다.

다만 이 `Integrated`는 조사 보고 통합만 뜻한다. 74개 Prefab 심층 대조, 우선 후보의 실제 Unity 로드·통행·상태 연결, 제품 Scene, E5, Game View는 `Blocked/Unverified`다. 후속 Unity 조회가 필요하면 대상 H1과 exact Prefab, 단독 Editor 슬롯, 원본·Scene·Preview·Console 보호 범위를 별도 명세해야 한다.
