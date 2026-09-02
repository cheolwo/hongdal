# D-554 최근 기획 Synty Prefab 조사 — P0 개발 통합

- 기준 결정: `D-554`
- 기준 문서: [최근 기획 Synty Prefab 조사 — 개발 인계](../AI/최근기획-SyntyPrefab조사-개발인계-2026-09-01.md)
- 기준 SHA-256: `6458F755E027A1AB68EF8879616D8612F8412E99F45CC46A1F80D2EA3DC0DA70`
- 결과 상태: `P0FileEvidenceReviewed / PresentationE4CandidateOnly`
- 실행하지 않음: Unity Editor, AssetDatabase, Prefab load, Preview, Play Mode, Scene, Game View, Blender, 구매·다운로드, 원본 수정, 실제 배치, E5, commit, push

## 1. 수행 범위와 증거

개발은 D-437에서 저장한 로컬 MySQL `world_visual_inventory_snapshots`를 읽기 전용으로 재사용했다. 모집단을 다시 스캔하거나 DB에 다시 반입하지 않았다. P0 역할 이름으로 좁힌 Prefab 검색 결과는 집 계열 168건, 수리 소품 계열 91건, 검·농기구 계열 116건, 몬스터·생물 계열 69건이다. 이 수는 이름·경로 검색 모집단이며 적합 후보 수가 아니다.

전문 담당은 다음 비중첩 파일 조사 결과를 반환했다.

| 담당 | 산출물 | SHA-256 | 증거 상한 |
| --- | --- | --- | --- |
| 월드·공간·배치 | `C:/Users/user/.codex/worktrees/cba3/Hongdal/spatial-support/synty-prefab-d554-p0/space-review.md` | `53D2E85A12F28061398D5573540A06ADCB2054D59D1B178DF1669E1CDFD83225` | Prefab/meta/YAML 구성·외형 역할 후보. 실제 Bounds·지지·통행·Anchor·이미지 미검증 |
| 애니메이션 | `C:/Users/user/ssalddel/artifacts/local/validation/synty-prefab-d554-p0/animation-review.md` | `C48EF63402E1BDB146A1D7F343CF3FCFFA700D1445647E1598A3853C5A7B5F92` | Actor/Avatar/Clip/Controller 정적 결속. 실제 import·재생·그립·접촉·귀환 미검증 |

공간 담당이 고른 13개 Prefab/meta는 현재 파일 지문과 D-437 입력 지문이 모두 일치했다. 그러나 D-437 `imageState`는 모두 `NotAcquiredOrNotSurveyed`이므로 이름·YAML만으로 외형 적합성을 통과시키지 않았다.

## 2. P0 통합 판정

### 2.1 한스 집: 파손 → 수리 중 → 수리됨

| 역할 | 정확 후보 | GUID | Prefab SHA-256 | 통합 판정 |
| --- | --- | --- | --- | --- |
| 수리 중 비계 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Scaffold_Preset_01.prefab` | `763a9fdc43a797e4eaf803854c430de3` | `7A383CCBBEE354B7E02F36948503D5371AA79B0D740222BDF5036206270930F2` | `연결·설정 보완` |
| 수리 중 사다리 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Ladder_01.prefab` | `3abf514a6823faa48aa619aa6670c586` | `F758559E76860C54E3D7676049440CAE34D6EE19AFAAEBADB9FB2F6AFE7DAF50` | `연결·설정 보완` |
| 보강 목재 | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Plank_Long_Stack_01.prefab` | `53c406acd6feb9b409da749a8baad33f` | `2894264141824D2A6A182F8A047F573C8CBFBB4F90FA2404643D773F37CAF48B` | `연결·설정 보완` |
| 완료형 비교 | `Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Farmhouse_01.prefab` | `72c07751c5ab1274e907d46aa40a3134` | `D5D615A089F8CB8900113A78FB0CE8E7D647D26088CA0F24AA55FE93EA7D6B36` | `미검사·미확보` |

비계·사다리·목재는 기존 한스 집을 교체하지 않는 상태 덧입힘 후보로 좁힐 수 있다. Farmhouse01은 완성 주택 파일일 뿐 기존 한스 집의 외곽·실내·문·생활 구역과 호환된다는 근거가 없다. 동일 주택의 파손/수리/완료 variant 쌍은 확보하지 못했다. 따라서 파손 구멍을 꾸며내거나 Farmhouse로 교체하지 않고, 기존 집을 보존한 제한 A/B 측정이 다음 단계다.

### 2.2 한스의 검과 첫 순찰 현장 도구

| 역할 | 정확 후보 | GUID | Prefab SHA-256 | 통합 판정 |
| --- | --- | --- | --- | --- |
| 집 안 관리된 검 | `Assets/Synty/PolygonDungeonRealms/Prefabs/Weapons/SM_Wep_Sword_Medium_01.prefab` | `94d77be6c53fb7c4d926452da7783d66` | `0918E11EAF001647C414F8E0CAEE07400A10892722212D6839DA607CBFD4009B` | 정적 단서 `연결·설정 보완`, 손 장착 별도 |
| 비전투 받침 | `Assets/Synty/PolygonDungeonRealms/Prefabs/Props/SM_Prop_Dwarf_Weapon_Rack_01.prefab` | `81e22d8c731e13b48b241af82760eca8` | `9BA02AE840CA36C6D62DA1EB664F48E772DC96262500D3B5853F598B238A11BD` | `격리 가공 필요` 후보 |
| 첫 순찰 도구 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Tool_Pitchfork_01.prefab` | `d5ac20042826928479fc7af3e4a17a05` | `7E0D05C7D4092D65973E26402A398E3096663763846494346C2CA2B69FDA08E6` | 외형 `그대로 재사용`, 장착 `연결·설정 보완` |

갈퀴·괭이도 평범한 Farm 도구의 외형 후보로 재사용 가능하다. 검집·칼집 이름의 Prefab은 좁은 보유 목록에서 찾지 못했다. 검 본체와 받침은 별도 Prefab이어서 집 안 결합 TRS·소유권·반출 금지·탈착 Anchor가 필요하다.

한스의 기본 사람형 대기·보행은 기존 `SM_Chr_Male_01`·`CharactersAvatar`·`방문자Wrapper.controller` 결속을 후보로 재사용할 수 있다. 그러나 한스 역할과 순찰 권위 상태는 연결되지 않았다. 농기구 손 socket/TRS·한손/양손 그립·장착 수명과 전용 행동 Clip도 확보되지 않았다. 따라서 첫 순찰은 “도구를 지니고 걷기”조차 실제 그립 검증 전에는 통과가 아니다.

SwordCombat의 Idle·Draw·Sheathe·AttackReturn·Hit 파일은 존재하지만 importer events가 모두 비어 있다. 공격 접촉과 검의 손/검집 전환 시점을 Clip에서 새 권위로 만들 수 없다. 집 안 환경 단서, 손 장착, 첫 순찰 미사용, 후속 중대 위기 사용은 계속 별도 상태로 유지한다.

### 2.3 첫 경계 마수 무리와 이동 흔적

| 역할 | 정확 후보 | GUID | Prefab SHA-256 | 통합 판정 |
| --- | --- | --- | --- | --- |
| 마수 외형 주 후보 | `Assets/Synty/PolygonDungeonRealms/Prefabs/Characters/Chr_BR_Demon_01.prefab` | `13087e211b7388945b22bab7dd93cb8f` | `6A58DA44CC145C92C7571485AA62F8B9FC24C3101135C0F5BE6AC7F4E640536E` | `사람형 범주 대체 / 미검사` |
| 사람형 fallback | `Assets/Synty/PolygonDungeonRealms/Prefabs/Characters/Chr_Undead_Knight_01.prefab` | `cb4ff29f6f657bc4d94ed49c50a70be2` | `48381B49F09FA10FFD30A63D940B9C54B815456DC1CCED6AAD919BAA1D220EDC` | `사람형 범주 대체 / 미검사` |
| 정적 위협 흔적 | `Assets/Synty/PolygonNature/Prefabs/Props/SM_Prop_Skeleton_Ground_01.prefab` | `8ec29696c3f5cc04faf6c4c6a5ffb0ea` | `2C03370A9F55ABA3F1546679BD0EA6BAD74E1B5BE68F1D56055E08213F289BD7` | 외형 `그대로 재사용`, 배치 `연결·설정 보완` |
| 손상 영역 표식 후보 | `Assets/Synty/PolygonDungeonRealms/Prefabs/Props/SM_Prop_Banner_Damaged_01.prefab` | `e22f16695df2a7146b185b1caf2307e4` | `23E01946307568B85971DD8C3759BB5D81652D71DF424863E7F2025606CEC629` | `격리 가공 필요`; 부서진 표지판 대체 아님 |
| 부상 흔적 FX | `Assets/Synty/PolygonGeneric/Prefabs/FX/FX_Blood_Splatter_01.prefab` | `4011e98ad166a054eba33d59b5d530d0` | `98FDB8A758CA1A002E3AD2D2BE9047123755DC51F14FA8ED06764939A2BE3905` | `연결·설정 보완` |

Demon과 Undead는 모두 Humanoid 계열이다. 현재 설치된 BaseLocomotion·EmotesAndTaunts·SwordCombat의 제한 검색에서도 동물형 Claw/Bite/Unarmed Clip을 확보하지 못했다. 따라서 “더 깊은 숲에서 밀려 내려온 굶주린 동물형 무리”의 주 후보로 확정하지 않고 사람형 범주 대체로만 남긴다.

Undead/Skeleton Prefab이 참조하는 Controller GUID의 소유 meta와 BaseLocomotion/SwordCombat의 copy-source Avatar GUID가 현재 Assets에서 해소되지 않았다. 이는 재생 불가 확정이 아니라 파일 증거의 결손이다. 실제 import·Avatar 호환·이동/공격/피격·중단 귀환은 별도 격리 평가가 필요하다.

발자국·pawprint·trackmark와 부서진 표지판 이름 Prefab은 좁은 검색에서 0건이었다. 다른 이름의 Mesh·Texture·Decal까지 전수 부재를 증명한 것은 아니므로, 신규 제작 판단 전에 제한 텍스처·데칼 재조사가 가능하다.

## 3. 중복 제거와 현재 선택 상태

- D-437의 14,461개/Prefab 4,221개 저장 사본을 검색 모집단으로 재사용했고 재반입·전수 재스캔을 하지 않았다.
- D-385 냄비·방문자 Actor·LS01 경관 이미지는 P0 집/검/마수의 외형 증거로 중복 사용하지 않았다.
- `CatalogMapped`는 기능 대장에 대응 행이 있다는 뜻이며 역할 적합·선정·Unity 연결 완료가 아니다.
- 현재 P0의 주·대체·fallback은 **후보 목록**이다. 실제 Synty 선택, `presentationE4Preparation` 동결, E5 배치는 아직 없다.

## 4. 가장 이른 다음 단계

1. **한스 집 상태 덧입힘 격리 측정**: 기존 집의 실제 Bounds·문앞 보호 통로·지면과 비계/사다리/목재 native Bounds·pivot을 읽어 교체 없는 A/B만 비교한다.
2. **검 단서 격리 조립**: Sword01+Rack01의 외형·결합 TRS·소유/반출 금지·분리 수명을 검증한다. 검집은 미확보로 유지한다.
3. **한스 도구 장착 격리 평가**: 기존 남성 Actor의 손 socket과 Pitchfork 장착·Idle/Walk·중단 복귀를 확인한다. 전용 작업 Clip이나 검 사용으로 확대하지 않는다.
4. **첫 경계 흔적 격리 조립**: GroundSkeleton·DamagedBanner·BloodFX의 접지·겹침·통로를 확인한다.
5. **마수 역할 재결정 관문**: 사람형 범주 대체를 수용할지, 동물형 판독이 필수인지 기획·연구 기준으로 확정한 뒤 정확 Actor/Rig/Clip 슬롯을 연다.

이 결과는 P0 파일 조사 마감이다. 실제 이미지, Bounds, Collider 적합성, 손 그립, Animation 재생, Scene/Game View가 없으므로 P0 Presentation E4 완료나 E5 승격으로 보고하지 않는다. P1 Town/Hub 조사와 P2 강적/분할 정복 조사는 독립 후속 묶음이다.
