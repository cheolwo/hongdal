# 24절기 주민 복장·식생 Synty/Blender 보완 개발 조사

## 1. 결론

- 기준 기획은 `solar-term-seasonal-food-research.plan.r4`, SHA-256 `8127B72EF5710066961CCAF089E9EAE6D05645D59F86D04F8DD602D9FF89ACAB`이다.
- 현재 판정은 **Conditional**이다. 보유 Synty 자산으로 4계절 기본군을 구성할 수 있지만, 24벌 세트나 24개 식생 Prefab을 만드는 방식은 필요하지 않다.
- 주민 기본 몸체·일부 모자·작업도구·Birch/Pine/관목/풀/꽃/작물은 재사용 후보가 있다. 그러나 주민 겉옷 조합, 장갑·신발, 겨울 낙엽수 실루엣, 수종별 LOD 재질 전환은 기존 자산만으로 완결되지 않았다.
- 권위 절기·최근 날씨 상태 사본에서 결정적 표현 Profile을 만들고, 주민 복장은 다음 외출/작업 경계에서 교체하며 식생은 별도 Presentation adapter가 소비해야 한다. 이 연결은 현재 제품 코드에 없다.
- 점진 보간은 Save/Replay 및 Local/Hosted 결정성을 먼저 확보해야 한다. 첫 구현 후보는 승인된 fallback인 `절기 턴 마감 → 상태 사본과 목표 Profile hash 고정 → 다음 턴/안전한 재진입에서 원자적 교체`이다.
- 이 조사는 파일·대장·계약의 읽기 검토다. Unity Editor, Scene, Preview, Game View, Blender 가공, 실제 연결, E 승격은 수행하지 않았다.

## 2. 증거 수준

| 구분 | 이번에 확인한 것 | 확인하지 않은 것 |
| --- | --- | --- |
| 자산 | Prefab 경로, GUID, 파일 hash, 구성 단서 | 실제 로드·렌더·착용·LOD 연속성 |
| 코드 | 상태 사본, 날씨 연속성 검사, 계절 표현 소비 구조 | 24절기·최근 날씨에서 실제 주민/식생까지 이어지는 제품 연결 |
| 결정성 | 기존 World atmosphere의 Save/Replay·Local/Hosted 선례 | 새 절기 표현 Profile의 동일 화면 복원 |
| Blender | 필요한 경우의 입력·출력·보존·복구 계획 | 제작 승인, 실제 FBX 생성·가져오기 |

`Prefab 존재`, `대장 등록`, `정적 시험`은 E5나 실제 화면 증거가 아니다.

## 3. 주민 복장·작업 소품 후보

### 3.1 그대로 재사용 후보

| 역할 | 정확 Prefab | GUID | Prefab SHA-256 | 판정 |
| --- | --- | --- | --- | --- |
| Farm 남성 주민 기본 몸체 | `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Male_01.prefab` | `3c3a8236be548bb4c892cf39a5abadfe` | `D176B368838CBED5024D89CB953B65FC8A0D3630E3FE11F214A0108B262E2343` | 기본 몸체 재사용 후보. Animator 1, SkinnedMeshRenderer 6, controller null, Farm Avatar. 계절 주민 실제 연결은 미검증 |
| Farm 여성 주민 기본 몸체 | `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Female_01.prefab` | `827a26e9b261b7847ac6cc3df54f2677` | `097EDCD8D8E1ABB7ED5991D0A973B00F56B4572D98B0BBABBA7D614FF53A45C9` | 기본 몸체 재사용 후보. Runtime 미검증 |
| 괭이 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Tool_Hoe_01.prefab` | `5a142f078c588454ca16909367867308` | `8D70CB70A685491497CF60E2D7F460D2CC8EBB6C2883B0114CC6AE1768571A78` | 작업 소품 후보. 손 Slot·작업 상태 결속 필요 |
| 물뿌리개 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Tool_WateringCan_01.prefab` | `6bb3792295be6374db9d296a6a1e5dab` | `EA8FE128D573DD8B792BF7A62A1DAE7C639BAEA191539A1ED7425E9943B30D21` | 작업 소품 후보. 급수 능력의 권위 근거는 아님 |
| 망치 벨트 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Tool_Belt_Hammer_01.prefab` | `b35adbc5dd80bdb4fbbe2710a7f5a296` | `B1F873A0DA6FC2C1246F575AF9CA8D284EAD0A03398D90A62C396B8F15FD47C8` | 작업 역할 후보. Farm 채택은 별도 기획 결속 필요 |

### 3.2 Unity 조립 보완 후보

| 역할 | 정확 Prefab | GUID | Prefab SHA-256 | 필요한 보완 |
| --- | --- | --- | --- | --- |
| 햇빛/봄·여름 모자 | `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Hat_Bucket_01.prefab` | `60fa9e0c6567acd45b844ed4537a6d72` | `D6A84D8E2221D728058D4535A397A71B7D104D4B96BD593E7F92F36B2E63C6B1` | Actor 뼈·머리 Slot·Collider 소유 확인 후 소유 wrapper에서 조립 |
| 일반 모자 | `Assets/Synty/PolygonFarm/Prefabs/Characters/SM_Chr_Farmer_Hat_Cap_01.prefab` | `b2eed9c07e9096a44a25870919024434` | `FB2E187F10E8A5F5200089086D9343B4B3B3A76D5CB6939DC8E57618AA55874B` | 같은 Slot·수명 관문 필요 |
| 비·작업 겉옷 후보 | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Construction_Raincoat_01.prefab` | `d3088af5cb703ce42947223d4f444d97` | `F5B5769FBC165D6259BBB22AF1B910B392F5D7213866ED52FE8FE3BD735978C4` | Farm 얼굴·몸체·Avatar를 유지하는 조합 가능성 미검증 |
| 작업복 후보 | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Construction_Overalls_01.prefab` | `d0a9ec5dc1484dc4bad93811a9c0ab65` | `AA542DC4D6962E90AA548E3013D7243F90F0540E3FA3749E2C60D2836241A75B` | 다른 팩 Skeleton/Avatar/재질 계보 대조 필요 |
| 방한 귀마개 후보 | `Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Construction_Earmuffs_01.prefab` | `ee9aa59915456024e8b21928e173ef49` | `9F49E692049EE2728053025AAB7AA12938B5B16E12109862B191A9BC3814CA04` | 머리 Slot과 모자 동시 사용 규칙 미정 |
| City 여성 코트 참고 | `Assets/Synty/PolygonCity/Prefabs/Characters/SM_Chr_City_Female_Coat_01.prefab` | `83139230f1d12e941a1a4fe3717b6561` | `67E5AD8486F9C9B57335CE8651B4855E5AA60E593B061CB326EE900E1168AF8A` | Farm 주민 정체성 유지 여부 미검증 |
| City 남성 재킷 참고 | `Assets/Synty/PolygonCity/Prefabs/Characters/SM_Chr_City_Male_Jacket_01.prefab` | `d461c345ada76d14f94fe72087cec0f9` | `3A5AEA9067DCA7AB784306B2C5C82D9C2CC8251DC20F9F37305E447452540046` | Farm 주민 정체성 유지 여부 미검증 |

공유 atlas/material을 직접 바꾸면 다른 인물까지 영향을 받을 수 있다. 원자산 수정 대신 프로젝트 소유 wrapper·재질 복사본·명시 Slot을 사용해야 한다.

### 3.3 후보 없음과 Blender 조건부 계획

- 파일명과 현재 Synty 대장에서 장갑 및 신발/장화의 식별 가능한 독립 후보는 찾지 못했다. 이는 전체 보유 팩에 절대 없다는 판정이 아니라 **현재 조사 범위에서 후보 없음**이다.
- 동일 Farm 주민 얼굴·몸체·Avatar를 유지하면서 코트·우비·장갑·신발을 독립 교체해야 하고 Unity 조립으로 해결되지 않을 때만 Blender 계획을 연다.
- 입력 후보: Farm Characters FBX와 Construction/City 의복 참고 자산.
- 제안 소유 원본 경로: `Assets/Ssalddel/ArtSource/Residents/Seasonal/`.
- 제안 제품 출력 경로: `Assets/Ssalddel/Presentation/Characters/Residents/Seasonal/`.
- 보존 조건: Skeleton, bind pose, bone 이름, root scale, Rig/Avatar, 재질 계보, Collider 소유권.
- 복구: 계절 wrapper를 해제하고 원 Farm Prefab으로 복귀. 위 경로와 가공은 아직 승인된 구현 경로가 아니다.

## 4. 식생·작물 후보

| 계절 역할 | 대표 후보 | 정확 근거 | 판정 |
| --- | --- | --- | --- |
| 봄 새싹 | Grass/Flowers 및 작물 소형 단계 | 현재 tree-specific bud 후보 없음 | **Unity 조립 보완**. 작은 식생 활성·밀도 Profile은 가능하나 수종 새싹으로 위장 금지 |
| 여름 밀도 | `SM_Plant_Bush_Leaves_01`, `SM_Plant_Grass_01`, Flowers | Bush GUID `47d0da1ba5336ef4bb50355db2307fe0`, Grass GUID `6f62c6f9e92a8ed44878e929e7e8db3f`, Flowers GUID `d7bb738e4e0288b42b577570c010f6df` | **Unity 조립 보완**. 활성·밀도·프로젝트 소유 재질 Profile 필요 |
| 가을 단풍 | Birch Base/Yellow/Brown/Pink 재질군 | Birch Prefab GUID `1ea71f643417d124d91e53eae7c666a6`, SHA `78AFFC39F7CE1453DB6ADAAA07CD023A47CF673BED393991B18068686F1166E1` | **Unity 조립 보완**. 수종·Renderer·material-slot·LOD별 binding 필요 |
| 겨울 상록 | `SM_Tree_Pine_01.prefab` | GUID `fc9f550802bde56499ac8b64cac565f0`, SHA `1331E2EF3F3E7D98C7F124C876B976D48ACD75FF659A37B397FFE32011FC6E0E` | 기본 실루엣 **그대로 재사용 후보**, 눈 표현은 날씨 조건 조립 필요 |
| 겨울 낙엽수 | Birch dormant | `SM_Tree_Birch_Dead_01`은 죽은 나무 의미 | **후보 없음**. 휴면 나무로 오인 금지; 필요하면 Blender 계획 |
| 절기 작물 | 감자 S/M/L, Wheat 01~04 | 단계별 geometry 후보 존재 | **그대로 재사용 후보 + Unity 조립 보완**. 성장 권위·절기 상태 연결은 별도 |

대표 식생 파일의 추가 식별 정보:

- Birch LOD0 mesh GUID `5b8619cd77cebea4e8da3a08467ce1a9`, SHA `08E2A431BDA58C368C770B0613CEEA8DDDAE73B32B0E84DC3E8EE695F64B77E0`.
- Birch LOD1 mesh GUID `dfd02d6fb40caa342b25650fd2b303ec`, SHA `6E1FC03007D9B5F464A418778B9952E2DD419C63600ECA7D7061CD2AA890BF8F`.
- Birch LOD 전환 단서는 `.185624`와 `.026555998`, Collider는 radius `.2049985`, height `3.907669`이다. 실제 Game View 연속성은 미검증이다.
- Bush SHA `77229DE94B75EEEB4528053744B08231FE86B64EEE173BDC233F49FB87BF37D8`, Grass SHA `816D284B3A7D7866412DD145E83CD22CE74005F8098A8925830C1B43BAB3EE3B`, Flowers SHA `D1456EED296B68CD5E5345322020C38A4EF8299DFC869D6BA5A8D33690B5BFAE`.

재질 대안은 Material Variant 상속이 아니라 독립 재질이고 여러 수목·풀·돌·건물과 공유될 수 있다. 현재 `자연경관SeasonPresentationController`의 문자열 이름 필터와 단일 tint만으로는 수종별 단풍, 상록/낙엽 분리, LOD 연속성을 보장할 수 없다.

겨울 Birch Blender 계획이 필요한 경우 입력은 기존 Birch LOD0/LOD1과 Dead 변형 비교로 한정한다. 제안 출력은 `Assets/Ssalddel/Presentation/World/SeasonalVegetation/BirchDormant/` 아래 소유 FBX·wrapper·재질이며, 줄기/pivot/scale, Collider, LOD 임계값, material slot, UV/normal을 보존한다. 제작 실패 시 원 Birch Profile로 복귀한다. 이 역시 계획일 뿐 제작 승인이 아니다.

## 5. 기존 상태·표현 계약 감사

### 5.1 재사용 가능한 부분

- `일별날씨Snapshot`은 `GameDay`, 평균기온, 강수량 및 evidence를 갖고, `농업ScenarioValidator`가 중복·날짜 연속성·단위와 정규화 값을 검사한다.
- `SimulationNatureWeatherProfileFreezeCandidateSnapshot`은 하루 시작/새 세계 경계, source hash, 규칙 판본, 날씨 Profile을 고정하는 후보 구조다.
- `WorldTick`은 경영 Session의 GameDate를 하루 단위로 결정하는 권위 선례다.
- `SimulationWorldAtmosphere`는 현재/다음 날씨와 `TransitionProgressPermille`를 결정적으로 계산하고 Save/Replay·Local/Hosted parity 시험의 선례가 있다.
- `SimulationSeparatedLhCellContentSource`는 base hash와 `SeasonCode`/`SeasonRuleVersion` 표현 hash를 분리한다.
- `자연경관SeasonPresentationController`는 프로젝트 소유 재질 복사본과 원 `sharedMaterials` 복원의 수명 패턴을 제공한다.

### 5.2 그대로 재사용할 수 없는 부분

- 농업 일별 날씨는 바람이 없고 현재 서버 권위 절기·지역·발견 상태와 같은 revision으로 묶이지 않았다.
- 날씨 freeze 구조는 `PlanningCandidate`이며 Save/Replay binding과 unavailable fallback이 미해결이고 실제 날씨를 적용하지 않는다.
- LH `SeasonProgress01`은 `simulation-season.28-day.r1`의 4×28일 모델이다. 한국 24절기와 현실 하루 비고정 원칙의 시간 권위로 재사용할 수 없다.
- 기존 자연경관 controller는 4계절을 즉시 바꾸고, 이름 필터·단일 tint·FX 활성만 사용한다. 점진 보간, 정확 species/Renderer/LOD binding, 주민 복장은 지원하지 않는다.
- Broadleaf/Conifer/Mountain 표현 key는 계약에 있으나 실제 자산 resolve 소비가 확인되지 않았다.

## 6. 최소 연결 계약

1. **권위 입력 사본**: `Session`, `WorldRevision`, `WorldTick`, 절기 code·규칙 판본, 지역, 발견 여부, 최근 일별 날씨 hash·품질을 한 revision으로 고정한다.
2. **결정적 목표 Profile resolver**: 4계절 기본군에 절기 진행, 기온·강수·바람, 주민 역할·실내외·작업, 식생 종류를 조합해 `TargetProfileId`와 hash를 만든다. 24벌/24Prefab 목록을 강제하지 않는다.
3. **주민 adapter**: 안정된 outfit/부품 Slot과 실제 자산 inventory를 사용하고 다음 외출·작업 일정 경계에서만 교체한다. Actor 권위, 작업 능력, 장착 도구 상태는 바꾸지 않는다.
4. **식생 adapter**: species/family, instance, Renderer, material slot, LOD binding을 명시하고 소유 재질·밀도 표현만 바꾼다. 이름 검색이나 전역 shader만으로 적용하지 않는다.
5. **표현 Profile 대장**: 후보 path/GUID/hash, 수명, Preview 여부, 실제 Prefab/Scene binding, fallback, 무효화 조건을 분리한다.

## 7. 전환과 결정성 판정

### 7.1 우선 fallback

`절기 턴 마감 → 권위 상태 사본과 목표 Profile hash 고정 → 다음 턴 시작 또는 안전한 재진입에서 주민 조합과 식생 Profile 원자적 교체`

- 날씨가 없거나 품질이 부족하면 절기 기본 Profile을 사용한다.
- 후보 조회·binding·복원에 실패하면 기존 4계절 표현 또는 변화 없음 상태를 유지한다.
- 로딩/짧은 전환 연출은 Presentation 전용이며 WorldTick·절기·날씨를 발생시키지 않는다.
- 주민 교체 시 이전 outfit을 성공 전까지 보유하고 stale revision 또는 실패에서 그대로 유지한다.

### 7.2 점진 보간을 열기 위한 조건

- 권위 Tick과 동결한 전환 epoch에서 진행률을 결정적으로 재구성한다.
- 시작 Profile, 목표 Profile, revision, transition epoch, adapter version을 Save/Replay에 포함한다.
- LocalProcess/RemoteHost가 같은 입력에서 같은 Profile/hash와 재진입 화면을 만드는지 시험한다.
- 셀 해제·재진입, LOD 전환, 동시 writer, 중간 저장·불러오기에서 정확한 재질/Renderer 복원을 검증한다.

프레임 시간 기반 보간은 Hosted 지연과 reload 시점에 따라 중간 화면이 달라질 수 있으므로 현재 단계에서 채택하지 않는다.

## 8. 남은 담당과 가장 이른 단계

| 항목 | 다음 담당 | 가장 이른 재개 |
| --- | --- | --- |
| outfit Slot·Farm/Construction/City Rig 호환 | 애니메이션 + 개발 | Presentation E4 연구·정적/격리 검증 |
| 주민 계절 Profile resolver와 일정 경계 | 개발 | Logic 상태 사본 계약 확인 후 Presentation E4 |
| Birch 수종·LOD material slot과 소유 복원 | 월드·공간·배치 + 개발 | Presentation E4 격리 Preview |
| 휴면 Birch geometry | 공간/Blender, 별도 승인 | Unity 조립으로 부족함이 확인된 뒤 제작 준비 |
| 절기·최근 날씨 권위 snapshot 및 Save/Replay | 개발 | Logic E4~E5 계약 설계; 기존 규칙 변경은 기획 재결속 |
| 실제 Prefab/World binding과 화면 | 월드·공간·배치 | 상태 사본·후보·수명 계약이 동결된 뒤 Presentation E5 이상 |

## 9. 변경·검증 범위

- 개발 통합이 주민/애니메이션, 식생/공간, 대장·상태·결정성의 비중첩 읽기 검토를 배분하고 결과를 합쳤다.
- 이번 변경은 이 보고서 한 파일이다.
- Unity Editor, AssetDatabase, Scene, Play Mode, Game View, Blender, 빌드, 게임 시험, 자산 가공, 원본 변경, commit, push는 수행하지 않았다.
- 기준 기획: [24절기·제철 자료 조사와 기획 연결](../AI/24절기-제철자료-조사와기획연결-2026-08-31.md).
- 관련 기존 조사: [절기 경관 D405 기존 소비 경로 조사](절기경관-D405-기존소비경로조사-2026-08-31.md).
