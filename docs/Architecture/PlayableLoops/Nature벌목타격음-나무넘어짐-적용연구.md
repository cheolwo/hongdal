# Nature 벌목 타격음·나무 넘어짐 적용 연구

## 1. 기준선과 판정

- 연구: `study:nature-woodcutting:impact-tree-fall.r1`, 판본 r1, 2026-08-30.
- 상태: **Accepted — 10절 A1 정적 소유 복사본의 계층/LOD/TRS 보존 helper·격리시험 기준만. A2 실제 pivot/낙하 방향/시간/지면·Audio 설정/청취와 B Blender 가공은 Draft/미승인 유지.** 최신 hash의 명세 재결속과 전용 경로 배분 전 코드 착수 금지.
- 수용 추록 1: 검토자 개발 `01a02198-8b2a-7491-ac93-366b30ff474c`, 2026-08-30. 수용 대상은 최초 Draft SHA256 `400906C3112F5491089534FDBBF2F42E56F3F8AD1614F93226F481860F080726`와 개발의 이번 A1 한정 수용 메시지다. 개발이 당시 보고 `829DF58E1EDAB2F418EEA71E5F37F9DC64C2DFD9545438AFA4D9D9C3917E7A85` 및 source-baseline 21/21 hash를 직접 대조했다고 반환했다.
- 이력: 최초 제출 상태는 `Draft — 파일 조사·기술 제안만`이었다. 아래 2~9절은 해당 조사·제안 이력으로 보존하며, 현재 수용 범위/순서는 10절만 적용한다. 7절의 A 전체 또는 8절의 전체 품질 시험이 Accepted된 것은 아니다. 최초 인계 보고와 source-baseline은 수정하지 않는다.
- 승인: [D379 원문](Nature벌목타격음과나무넘어짐.md), `nature-woodcutting-impact-tree-fall.design.r1` / ApprovedDirectionAndResearchScope.
- 승인 SHA256: `A65E52225B18B7E358785320EA9ADC1AC29F2891EC9D4F5FDB38C12AEF444E24` 전체 읽기·직접 일치 확인.
- 배분 시 명세: `nature-woodcutting-animation.e7-work-order.json` SHA `9ACA5F1EAD1270CD0B9C4BE5298D893405272A592770CA543B071C7810B45076`, Goal r80, `supportImplementation.impactTreeFallFollowup`. 이후 개발의 입력 시험 인수 문서 개정은 이 연구 승인으로 간주하지 않는다.
- 기존 [D359 연구](Nature벌목동작-애니메이션적용연구.md) SHA `01150E4ECEF8D281284AC3B3213ACFF0670C026F4202B4E4C0A6A7841387586B` 불변. 5.1 A만 수용된 범위와 B 신규 키프레임/bake/AI 가공 미확인 상태를 유지한다.
- 반환: 애니메이션 → 개발 통합 검토 → 기획. 이 문서는 권위 WI-NATURE-06의 4초 Task·보상·취소·Save를 변경하지 않는다.

**결론:** 타격/낙하음과 완료 후 낙하 복사본은 이미 존재한다. 새로운 음원이나 나무 전체 재제작보다 기존 완료 소비 사슬을 유지한 채 접촉 동기·3D 출력·LOD/크기/회전·밑동/지면 검증을 먼저 닫는 것이 적합하다. Blender 객체 애니메이션은 비교 대상이지만 제작 착수나 이용조건 확인 완료가 아니다.

## 2. 조사 방법과 증거 한계

읽은 것은 코드, Prefab YAML, importer meta, 시각 자산 대장, 오디오 요구사항 대장이다. D379를 위한 Editor·청취·녹음·음원 생성·Clip/.blend/FBX 생성·반입·재import는 하지 않았다. 같은 시간대 별도 D359 입력 수정의 승인된 컴파일/118 시험은 **D379 품질/청취/나무 실제 재생 증거가 아니다**.

Unity 루트는 `C:/Users/user/ssalddel`. 이하 `Assets/...`는 이 루트 기준이다. 21개 파일의 절대경로·SHA256·길이는 `C:/Users/user/ssalddel/artifacts/local/validation/nature-woodcutting-d359/impact-tree-fall-d379/source-baseline.json`에 고정했다. 바이너리 FBX의 내장 Clip·정점·축을 직접 판독하지 않았으므로 존재나 빈 clipAnimations로 Clip 부재/호환을 확정하지 않는다.

## 3. 기존 오디오 재고와 연결

[오디오 요구사항 대장](오디오요구사항대장.md)의 두 요구사항은 모두 **PrototypeBound / 절차형 Cue 연결 / 실제 청취 미검증**이다. 새 요구사항 ID를 중복 생성하지 않는다.

| 요구사항 | 실제 코드 재고 | 파일로 확인한 연결 | 남은 검증 |
| --- | --- | --- | --- |
| `audio:nature:axe:impact.r1` | `Nature_AxeImpact_ProceduralFallback`, 0.18초, 24kHz mono | AxeImpact → 대상 AudioEmitter의 PlayOneShot 배율0.6, 접촉 토큰 소비 경로 | 목재에 날이 박히는 질감, 접촉 시점, 장치 출력, 거리·음량 |
| `audio:nature:tree:fall.r1` | `Nature_TreeFall_ProceduralFallback`, 0.9초, 24kHz mono | 확정 최종 접촉 뒤 TreeFall → 배율0.52, 먼지/낙하 복사본 | 기울기·지면 도달과 소리 대응, 중복·잔향·시각 결과 연속성 |

근거: `Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs`의 BuildAudioRouting(337행 부근), PlayAudio(489행 부근), CreateProceduralClip(613행 부근).

- 기존 파형은 런타임 AudioClip.Create/SetData로 생성한다. impact는 92Hz sine+필터 잡음, fall은 54Hz sine+더 강한 저역 필터 잡음이다. 둘 다 sin(진행률×π) envelope라 첫 샘플부터 강한 transient를 갖는 구조는 아니다. **청취 없이 “알림음 같다/목재음이다”라고 판정하지 않는다.**
- 소스 코드 hash는 생성식 기준선이며 생성된 PCM/가공 음원 hash와 같지 않다. 별도 WAV/Ogg 원본·가공본을 이번 조사에서 확보하거나 생성하지 않았다.
- 정상 경로는 spatialBlend=1, Linear rolloff, minDistance1/maxDistance24. 해당 대상 spatialSources가 없으면 UI 2D 배율0.35로 fallback한다. 이 fallback 성공을 D379의 3D 접촉음 성공으로 올리지 않는다.
- AudioListener는 비활성 포함 검색 결과가 0일 때만 생성한다. 활성 Listener/장치/Editor 음소거 여부는 파일만으로 보장되지 않는다.
- RebindWoodcuttingAnchors의 나무 음향 위치는 marker.position+up×0.9, FX는 up×0.8이다. 도끼날의 실제 접촉점 측정값이 아니다.
- Nature 코드에 효과음 전용 volume/mute 사용자 설정 연결은 확인하지 못했다(Assets/Ssalddel C# 검색 범위). 기존 전역/장치 음소거가 없다고 단정하지 않는다. D379의 “음량/끄기 보존”을 통과시키려면 현재 실제 설정 소유자/접근 경로를 개발이 확인해야 한다. 누락이면 별도 최소 결속 제안으로 반환한다.

## 4. 완료와 접촉 단일 소비

현재 `Nature감각표현Presenter.벌목.cs`의 RefreshWoodcutting → 완료근거선택 → Project → Apply/Reconstruct → 단일 Tick → TryConsumeContact → ConsumeWoodcuttingContact를 유지한다.

1. 선택기는 이전 Working 작업, 동일 epoch/Session/Actor/OriginCommand/Target 및 `previous.Revision < ResultRevision <= current.Revision`의 근거를 고른다. EvidenceStableId/RecordHash가 비어 있으면 선택하지 않고 동일 내용 중복은 한 건, 충돌은 거부한다. hash는 계보 값으로 보존할 뿐 선택기가 원장 진위를 재인증하지 않는다.
2. 과거 작업을 본 적 없는 초기 Stump/Load나 명시 근거 없는 null 작업은 성공 낙하의 근거가 아니다. Projector가 상태·근거를 함께 판정한다. Core/원장 생성·권위 판단을 애니메이션에 옮기지 않는다.
3. ConsumeWoodcuttingContact는 관측세대·Session·Actor를 검사하고 cue.StableId를 **효과 호출 전** 소비한다. 진행/마지막 접촉 모두 impact/woodchip을 먼저 호출한다.
4. Completed + ProgressToken==RequiredSeconds + 일치 종료 근거/세대/OriginCommand + 비어 있지 않은 recordHash + 미소비 EvidenceStableId일 때만 TreeFall/fallDust를 이어 호출한다.
5. 작업 중 원본이 살아 있을 때 PrepareTreeFallVisual로 소유 복사본을 숨겨 준비한다. 완료 결과 적용으로 원본이 사라진 뒤 새로 복제하려 하지 않는다.
6. Load/새 세대는 Reconstruct로 과거 큐·대기 완료·준비/활성 낙하를 폐기하고 앵커의 재생음/FX를 정리한다. 이동/중단은 준비/완료를 폐기하고 복귀하며 이동 자체가 권위 취소라는 뜻은 아니다.
7. Audio/FX 예외는 독립 처리한다. 이미 소비한 마지막 접촉을 재시도해 성공 채널을 중복 재생하지 않는다. 준비 복사본이 없으면 `CompletedTreeVisualUnavailable`; 소리만 나온 것을 낙하 성공으로 세지 않는다.

`Nature감각표현Models.cs`의 legacy Stump 전환 추론과 달리 권위 PlayerStableId가 있는 새 경로는 legacy 벌목 진행/완료/취소 Cue를 억제한다. D379는 이 차단을 유지한다. 새 Animation Event·별도 Update·AudioSource로 완료를 중복 생산하지 않는다. 기존 연결 시험의 본문은 위 순서를 검사하지만 실제 접촉/청취/지면 관통 검증을 대신하지 않는다.

## 5. 나무 하나의 후보·원본·실제 연결

첫 **모델 후보**는 `Assets/Synty/PolygonNature/Prefabs/Trees/SM_Tree_Pine_01.prefab` 하나다. 실제 Play의 특정 나무 instance/Target ID를 이번 파일 조사로 고정한 것은 아니다.

| 연결 요소 | 고정 근거 |
| --- | --- |
| Prefab | GUID `fc9f550802bde56499ac8b64cac565f0`, root GameObject fileID `1182756067964316` |
| Prefab SHA256 | `1331E2EF3F3E7D98C7F124C876B976D48ACD75FF659A37B397FFE32011FC6E0E` |
| LOD0 mesh | `Models/SM_Tree_Pine_01.fbx`, GUID `5514696c048ea514c8e0bbd59d370247`, fileID4300000 |
| LOD1 mesh | `Models/LODS/SM_Tree_Pine_01_LOD_01.fbx`, GUID `3fb50d0818667374aab581a3799635a9`, fileID4300000 |
| LOD0 materials | `9815abc3682332c43bb766decd901afb` / `feab95a1ad51daa4caeb40403608aa12` |
| LOD1 material slots | 두 슬롯 모두 `32a7d9275f65e894c84e0dcdcf05e4e0` |
| 대장 | `Assets/Ssalddel/Resources/Nature생존VisualCatalog.asset`의 TreePrefab이 같은 GUID/root fileID 참조 |

Prefab 루트(identity TRS)에 LODGroup+CapsuleCollider, 자식 `SM_Tree_Pine_01`과 `SM_Tree_Pine_01_LOD_01` 각각 MeshFilter+MeshRenderer가 있다. 두 자식은 active1/enabled1, identity TRS다. LOD0/1 renderer fileID는 23694049802177168 / 23171540964131236, 화면 비율 경계는 0.29754394 / 0.018657776. LODGroup size5.683202는 직렬화된 LOD 기준이지 실측 높이가 아니다.

CapsuleCollider는 enabled/비Trigger, Y축, radius0.33469224, height3.907669, center(0,1.6459765,0). Collider 밑점이나 통합 Renderer bounds.min을 실제 줄기 절단면으로 확정하지 않는다. Prefab에는 Animator/Animation/Rigidbody/Clip 참조가 직렬화되어 있지 않다.

두 FBX importer는 `importAnimation:0`, `animationType:0`, `clipAnimations:[]`, `referencedClips:[]`, `globalScale:1`, useFileUnits1/useFileScale1, bakeAxisConversion0이다. 현재 나무는 캐릭터 Humanoid 재타깃 대상이 아니다. **FBX 바이너리 내부 애니메이션의 존재 여부·실제 Unity subasset·mesh bounds·Blender 축 왕복은 미검증**이다.

연결은 두 경로에서 확인했다.

- 기존 Controller.RebuildVisuals: 분리 World 배치를 쓰지 않는 경우 TreePrefab로 ResourceNode 표현을 만들고 HarvestTree marker를 단다.
- 분리 World: `World공간표현조립Coordinator.ResolvePrefab`의 `Nature.Tree.Standing` → 동일 대장 TreePrefab. ConfigureNatureMarker는 Category NatureResourceNode+Standing만 HarvestTree로 설정하고 SourceChangeStableIds 첫 값(없으면 PlacementStableId)을 marker StableId로 쓴다. instance.localScale은 placement.UniformScale을 적용한다.

따라서 장식용 Pine만 있다는 주장은 아니며 실제 수확 표현으로 연결될 코드가 있다. 다만 현재 세션이 어느 배치·scale·yaw·지면·Target을 사용 중인지는 후속 승인 관찰에서 동결해야 한다. 동일 Prefab을 쓰는 장식 나무를 자동 수확 대상으로 선택하지 않는다.

## 6. 현재 TreeFall의 보존과 결손

| 확인된 구현 | 의미와 남은 문제 |
| --- | --- |
| 원본 sharedMesh/sharedMaterials 읽기 + 새 Transform/MeshRenderer 복사 | 공급사 파일·원본 Controller/Collider를 수정하지 않는 방향은 유지할 수 있음 |
| 모든 MeshRenderer를 비활성 포함 순회, LODGroup/active/enabled 미복제 | 이 후보의 LOD0/1을 둘 다 활성 복사해 한꺼번에 그릴 수 있음. 단순 “Renderer2 있음”은 정상 LOD 증거가 아님 |
| 자식 scale을 source.lossyScale로 나누고 복사 root는 기본scale1 | 배치 UniformScale 및 부모 scale을 완전히 재현하지 못할 위험. identity 후보 YAML만으로 실제 크기 보존 통과 금지 |
| pivot 위치=(전체 Renderer bounds.center.x, min.y, center.z), 초기rotation=marker.rotation | 밑동/절단점 실측이 아니고 비활성 LOD·수관 bounds까지 포함 |
| AnimateOwnedTreeFall이 localRotation=Euler(SmoothStep(0,84),0,0)로 대입 | 초기 로컬 회전과 합성하지 않아 원래 yaw/부모 회전을 잃거나 첫 프레임 자세가 뛸 수 있음 |
| 0.9초 기울기 뒤 소유 복사본 Destroy | 지면 도달·잠깐의 잔류·결과 통나무와의 연속성을 보장하지 않음. 지면 검사/주변 장애물 검사 없음 |
| owned 낙하는 ClockPaused 시 elapsed 정지 | 권위 시간을 새로 만들지 않는 장점. legacy 낙하는 별도 unscaled 경로이므로 새 권위 경로와 혼합 금지 |
| 복사본은 Collider/Rigidbody/업무 marker를 생성하지 않음 | 낙하 피해·길 막힘·추가 자원 없이 시각 전용 유지. 원본/결과 Collider 수명은 개발·공간에서 별도 확인 |

근거 함수는 Presenter.cs의 TryCreateTreeFallVisual/BuildStaticVisualClone/RendererBounds와 partial의 PrepareTreeFallVisual/AnimateOwnedTreeFall이다. 위 결손은 코드 구조에 대한 정적 판정이며 현재 Game View에서 실제로 모두 발생했다고 주장하지 않는다.

## 7. 방식 비교와 권리 공백

### A. 기존 소유 복사본·절차형 재생 보완 — 우선 제안

새 Clip/FBX/.blend 없이 기존 원본을 읽기 참조하고 완료 소비/취소/복원 계약을 유지한다. 개선 후보는 원래 월드 자세·배치 크기 보존, 명시적인 밑동 기준점, LOD 계층 보존 또는 대상 1개에서 검증된 단일 LOD 선택, 밑동 회전과 부모 회전 합성, 기존 지면을 읽는 종료 자세 검증이다. 시각 복사본만 단일 작성자가 움직인다.

단일 LOD 고정은 임의 최적화 확정이 아니라 비교안이다. 거리 전환 품질과 원본 외형을 확인해야 한다. 0.9초/84도는 현행값일 뿐 새 품질 보장값이 아니다. 지면과 불일치하면 shrink/terrain 수정 대신 대상 선택·앵커·방향·종료 각도 문제로 반환한다.

A 보완도 **D379 Accepted/소유 경로 배분 전 코드 변경 금지**다. 기존 D359 A 승인만으로 이 후속 구현을 승인받았다고 해석하지 않는다.

### B. Blender 객체 애니메이션·전용 복사본 — 조건부 비교

사용자 의도대로 Blender 작업 경로를 검토한다. 나무 전체의 rigid object 기울기는 Humanoid/새 뼈 리깅이 필수인 동작이 아니다. 전용 복사본에 밑동 pivot/객체 transform 트랙을 두고 수관·줄기·LOD가 같은 기준으로 움직이게 만드는 안이다. 이를 택하더라도 완료/취소/Load/음향 소비는 Unity의 기존 권위 관측 경계가 맡는다.

장점은 동작을 분리해 재열기·곡선 검토하기 쉽다는 점이다. 비용은 FBX 단위/축/계층/LOD/재질 연결 왕복, 복사본 원본 계보, 클립 binding/시간 정지·중단, 실제 지면별 방향 조정 검증이 추가된다는 점이다. 고정 Clip 자체는 지면 관통이나 원장 중복을 해결하지 않는다.

향후 경로 제안(예약/쓰기 승인이 아님): Unity Assets 밖 `ArtSource/Blender/source/NatureImpactTreeFallD379/`에 편집 원본, `exports/NatureImpactTreeFallD379/`에 Unity 전달 FBX, `validation/NatureImpactTreeFallD379/`에 왕복 기록을 분리. Unity 전용 전달/시험 경로와 공유 Presenter 수정은 개발이 별도 확정한다. 공급사 폴더·기존 D359/D286 전달 폴더 덮어쓰기 없음.

### 권리 판정

기존 D359 연구6절의 구매조건/채널 판본 미확정과 B 보류를 그대로 인용하는 **프로젝트 승인 경계**이며 이번에 법률 조건을 새로 인증한 것이 아니다. Nature meta의 Store/productId120152/패키지1.2.0/upload915458는 자산 유래이지 해당 구매 영수증/적용 EULA 판본 증명이 아니다.

- 기존 프로젝트 내 참조·상태 연결·파일 분석은 계속 가능하다.
- 수동 로컬 Blender 편집과 AI가 새 키프레임을 생성하거나 bake/자동 재가공하는 방식은 구별한다. D379가 후자의 미확인 권리를 해제하지 않는다.
- 정확한 구매 채널·적용 조건·제작 방식이 결속되기 전 신규가공은 착수하지 않는다. 불명확한 가공 방식만 보류하고 기존 재사용 연구 전체를 중단하지 않는다.
- 외부 AI 업로드·모델 생성 서비스·유료 도구/새 음원 구매·공급사 원본 수정 없음. 새 음원을 택할 때는 별도 출처/사용조건/원본과 가공 hash/청취를 기록한다.

## 8. 수용 시험과 후속 승인 단위

| 검증 단위 | 합격에 필요한 근거 | 현재 |
| --- | --- | --- |
| 실제 대상 하나 | Scene/Session/Actor/Target/OriginCommand/원본GUID/hash/배치scale·yaw·부모·지면·LOD 고정 | 모델 후보/코드 경로만 확인 |
| 도끼 접촉음 | 정상 4초 작업의 시각 접촉 프레임과 cue/token/3D 위치·출력 시각 대응, 시작·허공·중단·복원에서 오발행0 | 접촉 토큰 연결 존재, 실제 접촉·청취 미검증 |
| 마지막 타격→낙하 | 이전 Working+확정 완료 근거→마지막 impact→TreeFall 순서1회, current보다 낮은 유효 ResultRevision도 허용 | 코드/기존 Fixture 근거, 실제 입력 미검증 |
| 부정·중복 경계 | 근거누락/충돌·다른Actor·다른Origin·동일/낮은revision·Load epoch·이미Stump·빠른 중단/재시작에서 과거 효과0 | 새 D379 집중시험 미실행 |
| 외형·LOD | 회전 전 원본과 동일 외형/크기/재질, 근·원거리에서 LOD 중복0, 첫 프레임 yaw jump0 | 정적 결손 발견 |
| 밑동·지면 | 같은 밑동에서 줄기/수관 동기 회전, 기존 지지면 쪽 낙하, 지면·주변/카메라 관통과 결과 pop 확인 | 실측 미검증 |
| 소유·수명 | source파일/hash 불변, 원본/결과 Collider 보존, 복사본 Collider/권위명령0, pause·cancel·Load·Disable에서 자기 재생만 정리 | 기존 구조 있음, D379 검증 미실행 |
| 음량·접근성 | 실제 설정/장치로 음량0·끄기·거리감쇠 확인, 소리 꺼도 접촉/낙하/결과 판독 가능 | 설정 결속·청취 미검증 |
| Blender 선택 시만 | .blend 저장/재열기→별도 FBX→Unity 축/단위/pivot/계층/LOD/material/clip 재생·중단 검증 | 미제작, 방식 권리 확인 필요 |
| 권위 불변 | 표현 없이도 같은 완료/보상/4초Task/Save, 낙하가 피해/새통나무/길봉쇄를 만들지 않음 | 의미 변경 제안 없음 |

다음 순서: 개발이 A/B 범위·소유·이 연구 hash를 검토 → Accepted와 명세 재결속 → 기존 대상 하나의 읽기/격리 평가 슬롯 → 필요한 전용 구현 → 집중시험 → 공간 담당의 동결된 실제 입력/연속 화면과 별도 장치 청취 → 개발 통합 검토. 청취/영상 캡처는 이번 문서 제출로 자동 승인되지 않는다.

새 손도끼 키프레임, 피격/컵 등 다른 동작, 신규 권위 규칙, 자산 전체 재제작은 제외한다. 입력 문제가 남으면 실제 실행을 차단해 기록하며 테스트 직접 호출로 대체하지 않는다.

## 9. 무효화와 인계

원본/meta/material/대장/선택기·Projector·Presenter/배치 Coordinator의 변경, 다른 나무 모델 선택, actual Target/LOD/scale 변경, 음원 교체, 완료/Load 계약 변경 시 관련 부분을 재대조한다. 별도 입력수정 Controller hash는 동결 당시값으로만 기록한다. E 승격·제작 완료·청취 완료 없음.

개발 인계와 검증 기록은 [D379 연구 인계](../../Reports/Nature벌목타격음-나무넘어짐-D379-연구인계-2026-08-30.md)로 분리한다.

## 10. A1 한정 수용 — 정적 소유 복사본·격리시험 기준

### 10.1 수용 범위와 착수 관문

개발의 2026-08-30 수용 메시지는 **정적 소유 복사본의 원본 계층·LOD·활성 상태·TRS 보존 helper와 집중 격리시험 기준만 Accepted**로 한정한다. 새 helper/fixture를 별도 준비할 수 있는 기술 기준선이며, 현재 배분은 이 연구 문서의 수정과 최신 hash 반환뿐이다. 개발이 최신 hash를 명세에 재결속하고 정확한 전용 staging 경로를 다시 배분하기 전에는 코드·시험 초안도 작성하지 않는다.

기존 `BuildStaticVisualClone`의 1인칭/3인칭 도끼 호출 두 곳은 보존한다. 기존 `TryCreateTreeFallVisual`의 legacy 낙하와 권위 사전복사 양쪽 경로에도 이번 단위에서 연결하지 않는다. 공유 Presenter patch, 후속 실제 Pine fixture, 실제 낙하 연결은 개발의 별도 배분이다. 기존 메서드 전체 동작을 새 helper에 복제하지 않는다.

### 10.2 입력·출력과 허용 구성

- 입력은 원본 `Transform`과 **개발이 생성한** `destinationParent`다. helper가 새 pivot이나 지면 기준점을 정하지 않는다.
- 출력은 비활성인 소유 wrapper와 그 안의 정적 시각 계층이다. wrapper의 사전 숨김 상태와 내부 원본의 `activeSelf`를 구별하고, 내부 원본 local 계층/TRS·MeshFilter/MeshRenderer 설정을 보존한다.
- mesh/material은 기존 자산을 읽기 참조하며 정점·곡선·메시를 변형하거나 bake하지 않는다. LODGroup의 설정과 각 단계 Renderer 참조는 원본→복사 매핑으로 재결속한다. 복사 LODGroup이 원본 Renderer를 참조해서는 안 된다.
- 복사본의 Collider/Rigidbody/Animator/업무 컴포넌트는 0개다. 원본에 있는 해당 컴포넌트를 제거·비활성화하지 않고 복사에서 제외한다. 완전한 Prefab Instantiate 후 구성요소를 제거하는 방식은 사용하지 않는다.
- SkinnedMeshRenderer, 지원하지 않는 동적 Renderer, source 계층 밖의 LOD Renderer 참조, 누락 mesh는 **명시 거부**한다. 지원하지 않는 부분을 생략한 불완전 복사본을 성공으로 반환하지 않는다.
- source와 destinationParent의 기존 상태는 불변이다. 성공 시 허용되는 변경은 destinationParent 아래 새 소유 wrapper의 추가뿐이며, 부모/원본의 TRS·설정·기존 자식은 변경하지 않는다. 실패 시 이번 호출이 생성한 소유 조각만 정리하고 기존 객체/자산은 건드리지 않는다.

### 10.3 TRS 무보정 보존과 거부 기준

원본과 destinationParent 사이 상대행렬을 `destinationParent.worldToLocalMatrix * source.localToWorldMatrix`로 검토한다. 단순 worldRotation/lossyScale 복사로 보존했다고 판정하지 않는다. 유한 값, 비특이성, 양의 scale, 축 직교성, TRS 재구성 행렬의 일치를 확인한다. 내부 자식 계층은 원래 local TRS를 보존한다.

- shear, zero scale, negative scale/reflection은 A1 미지원으로 명시 거부한다. 새로운 중간 pivot, 메시 보정/bake 또는 축소로 통과시키지 않는다.
- 비균일 scale 자체를 무조건 shear로 취급하지 않는다. 양의 비균일 scale이라도 부모 회전과 결합한 상대행렬이 검사를 통과하는 경우와 실제 shear인 경우를 분리한다.
- 개발이 지정한 초기 허용오차: 정규화된 서로 다른 축의 dot 절댓값 `<= 1e-5`; 재구성 행렬의 각 원소는 `abs(reconstructed - original) <= 1e-4 * max(1, abs(original))`.
- 이 수치는 행렬 보존 검증의 시작 기준이며 발 접지·손잡이·지면 접촉 오차가 아니다. 격리시험에서 합리적인 수정이 필요하면 변경값·근거·영향을 기록해 개발에 반환하고, 기존 숫자를 조용히 바꾸거나 허용 범위를 성공 수에 맞춰 완화하지 않는다.

### 10.4 집중 격리시험 기준과 증거 상한

향후 배분할 신규 helper/fixture만으로 다음 경계를 검증한다. 이번에는 실행하지 않았다.

1. identity, 부모 회전+균일 scale, 표현 가능한 양의 비균일 scale에서 local 계층과 복사 후 월드 행렬을 대조한다. 계층을 평탄화해서 외형만 비슷하게 만들지 않는다.
2. 회전/비균일 scale 조합으로 생긴 shear, zero/negative/reflection, 비유한 값과 비특이성 실패를 명시 거부한다. 실패 후 부모/원본과 기존 자식을 보존한다.
3. 중첩된 비활성 자식, disabled MeshRenderer, MeshFilter/sharedMesh·material 참조와 지원 Renderer 설정, LODGroup 단계/설정/Renderer 재매핑을 검사한다. 원본과 복사 객체의 참조 혼입 및 성공으로 처리된 누락이 없어야 한다.
4. Skinned/지원외 동적 Renderer, 외부 LOD 참조, 누락 mesh를 음성 사례로 검사한다. 전체 Prefab 인스턴스화나 업무 컴포넌트 실행에 기대지 않는다.
5. wrapper 비활성, 복사 Collider/Rigidbody/Animator/업무 컴포넌트0, 원본 무변경, 실패 시 자기 생성분만 정리하는 수명을 확인한다. 성공 결과의 후속 활성화/낙하 수명은 이번 helper가 소유하지 않는다.

시험 개수·실제 통과·실물 Pine 호환을 사전에 선언하지 않는다. 이 기준의 수용은 실제 Pine/나무 낙하 성공이 아니며, A2 pivot·방향·시간·지면·Audio 설정/청취와 B Blender 가공은 계속 Draft/미승인이다. 완료 소비·소리·권위 상태·4초 Task·보상·Save는 변경하지 않는다.

현재 공간 r82의 마우스 관찰 슬롯을 침범하지 않는다. 이번 수정에는 Editor/Assets/staging 코드 쓰기·컴파일·시험·Blender 작업이 없으며, 문서 전용 검증 뒤 최신 hash만 개발에 반환한다.

## 11. A2a Draft — 권위 낙하 경로의 정적 복사 연결과 초기 회전 합성 후보

### 11.1 상태·이력·제외

2026-08-31, 개발의 후속 파일검토 배분에 따른 **Draft / 미수용 / 코드 착수 전** 추록이다. 기존 1~10절을 수정하지 않는다. 상단 Accepted와 10절은 A1만을 뜻하며 이 11절을 승인하지 않는다. A2a Accepted/hash·명세·소유 경로 재결속 전 구현하지 않는다. 기존 D379 인계 보고는 당시 이력으로 보존한다.

선행 A1은 정상 컴파일과 합성 정확 class 45/45 정식 시험을 거쳤다. Factory SHA `E8C7503E5A27C07949743BE7B49D5527B4E1F422A2F5C824EB72F605BC6AEF66`, Tests SHA `ED12C5AB4D39519C59AE97C12686E427D9897D2DAA3026606943510AC85903BF`, 정식 XML SHA `1739504E518D4DFAC8A1DB8F3A730531437EC5A26A6275F8D0410D2951ADA0CD`. 검증 기록은 Unity `artifacts/local/validation/nature-woodcutting-d359/impact-tree-fall-d379/a1-static-clone/validation/검증인계.md`다. **합성 정적 보존 시험이지 실제 Pine·낙하·Audio·Play 증거가 아니다.** 10절의 당시 미실행 서술은 역사로 유지한다.

A2a 후보는 **권위 작업 중 사전 복사와 owned 낙하 경로만** 별도 메서드로 연결한다. 기존 Presenter.cs의 `TryCreateTreeFallVisual`/`AnimateTreeFall` legacy 경로와 도끼용 `BuildStaticVisualClone` 호출 두 곳은 그대로 둔다. 새 Clip/피봇 위치/방향축/각도/시간/지면높이/Collider/음량·권위·Save 변경은 범위 밖이다. A2의 실제 지면·접촉 품질 및 Audio, B Blender/키프레임/bake/FBX 권리는 별도 미승인 그대로다.

### 11.2 실제 Pine의 파일 근거와 아직 필요한 입력 확인

`Assets/Synty/PolygonNature/Prefabs/Trees/SM_Tree_Pine_01.prefab` SHA `1331E2EF3F3E7D98C7F124C876B976D48ACD75FF659A37B397FFE32011FC6E0E`를 다시 확인했다. GUID/mesh/material/LOD 식별자는 5절을 따른다. 파일상 root/자식 StaticEditorFlags=0, identity TRS, active1, MeshRenderer2, 내부 LOD2, fadeMode0/animateCrossFading0, probeAnchor/proxy 참조0이다. 이 값은 현재 Play 인스턴스의 값으로 간주하지 않는다.

- Nature생존VisualCatalog.asset의 treePrefab은 위 GUID를 가리킨다. `World공간표현조립Coordinator.Configure`는 placement 회전/UniformScale → 지면 정렬 → HarvestTree marker → 방향광 정책을 적용한다. 따라서 실제 원본은 prefab YAML이 아니라 사전 복사 시점의 marker 계층이다.
- `방향광표현.cs`의 Renderer 정책은 shadowCastingMode/receiveShadows를 Runtime에서 바꾼다. A1은 이 현재 설정을 읽어 복사해야 한다. 자산 기본값으로 되돌리지 않는다.
- Presentation/Bootstrap 및 PolygonNature C#의 좁은 파일 검색에서 이 나무 경로의 MPB/ForceLOD/custom bounds writer를 확인하지 못했다. 다른 컴포넌트·패키지·실행 중 작성자의 부재를 증명한 것은 아니다.
- **실물 Pine 읽기 평가 선행:** 정확 prefab/mesh/material hash, 지원 컴포넌트·Renderer·LOD 내부참조·직렬화 설정을 별도 승인된 Editor 읽기 슬롯에서 확인한다. prefab 존재/identity YAML/합성45만으로 A1 호환 성공을 선언하지 않는다.
- **실제 marker preflight 선행:** Session/Actor/OriginCommand/Target/관측세대·권위 revision과 marker/source 식별, 현재 부모 사슬·행렬/scale·활성·StaticFlags·MPB·probe·LOD 참조·mesh/material·bounds를 사전 복사 시점에 확인한다. A1 명시 거부를 우회하거나 지원외 구성요소를 조용히 누락하지 않는다.
- ForceLOD 강제선택 이력, 진행 중 crossfade, 현재 계산값과 같은 custom worldBounds override, source 밖 LODGroup의 역참조는 부분트리 공개 API만으로 검출 완료를 주장할 수 없다. source를 준비부터 낙하 종료까지 움직이거나 렌더 설정을 쓰는 외부 작성자와 숨은 override가 없다는 입력 소유 근거가 필요하다. 미확인 입력을 지원 완료로 승격하지 않는다.

### 11.3 별도 권위 메서드와 명시적인 활성·실패 수명

개발 partial에 `TryPrepareOwnedTreeFallVisual` 같은 별도 private 메서드를 두는 후보이며, 공개 API 확정이나 구현은 아니다. 기존 shared legacy 메서드를 내부적으로 바꾸어 두 경로를 동시에 교체하지 않는다.

1. 아직 원본 Standing marker가 유효한 Working 관측에서만 준비한다. 기존 관측세대·작업 교체/중단·Load 폐기 조건을 유지한다. 완료 뒤 사라진 원본을 다시 복구하거나 대체 나무를 고르지 않는다.
2. 개발이 새 소유 pivot/container를 생성해 **비활성**으로 두고 현재 `RendererBounds`의 `(center.x,min.y,center.z)` 위치와 marker 월드 회전이라는 기존 휴리스틱을 적용한다. 부모 사슬과 현재 TRS를 고정·확인한다. 원본·기존 부모를 재부모화하거나 scale 보정하지 않는다.
3. A1 `TryCreate(marker.transform, ownedPivot, out ownedWrapper, out diagnostic)`를 호출한다. 반환된 wrapper는 비활성이다. source 초기 월드행렬과 복사 계층의 행렬/LOD 재매핑·외형 설정을 대조하며, 이후 clone에 다시 `SetPositionAndRotation`/lossyScale 나눗셈을 적용하지 않는다.
4. 성공하면 **pivot을 숨긴 채 wrapper만 활성 준비**한다. wrapper.activeSelf=true여도 pivot.activeSelf=false이므로 내부 계층은 화면에 나타나지 않는다. 원본 내부 activeSelf는 바꾸지 않는다. pivot만 켜고 wrapper는 계속 비활성으로 남기는 누락을 시험한다.
5. 최종 접촉·동일 작업의 확정 완료 근거를 기존 단일 소비 경계에서 확인한 뒤에만 준비 pivot을 활성화해 owned 낙하에 넘긴다. 마지막 impact/woodchip → 완료 fall/먼지의 기존 순서와 중복 차단을 유지한다. 별도 Animation Event·Update·Audio 경로를 추가하지 않는다.
6. A1 false/예외/원본 상실이면 진단을 보존하고 이번 준비의 소유 조각·pivot만 정리한다. 기존 평탄화 helper로 조용히 fallback하거나 부분 복사를 성공으로 채택하지 않는다. Play의 Destroy 지연 동안은 먼저 비활성화하고, 즉시 제거된 것으로 보고하지 않는다. 완료 근거와 다른 효과의 소비를 되감아 중복 재생하지 않는다.
7. 중단/취소/Load/세대·작업 교체/Disable/Destroy는 준비물과 자기 활성 낙하만 정리한다. 다른 작성자/Graph/원본 Collider·결과 통나무·저장 상태는 건드리지 않는다.

destinationParent는 **개발 소유 비활성 복사 container, 계층 변경 callback/외부 writer 없음**이 전제다. 비활성만으로 callback 부재를 입증하지 않는다. SetParent가 기존 부모의 OnTransformChildrenChanged를 유발할 수 있으므로 임의 부모에서 무부작용인 범용 복사로 확대하지 않는다. 기존 Presenter 아래 pivot을 붙이는 소유 경계 역시 이 조건을 별도로 확인해야 한다.

### 11.4 owned 회전 합성 — 기존 yaw 소실의 제한적인 교정 후보

현재 두 Animate는 `pivot.localRotation = Quaternion.Euler(기울기,0,0)`로 대입한다. A1이 생성 시 원본 월드행렬을 보존하더라도 이 대입은 초기 yaw/부모 회전 관계를 잃게 만들 수 있다.

A2a는 owned 경로에서만 활성 직전의 `initialLocalRotation`을 한 번 고정하고, 매 프레임 **`initialLocalRotation * localX 기울기 delta`**로 합성하는 후보를 명시한다. localX는 기존 Euler X 기울기 의미를 기준 자세에 결합하는 후보이지 새 월드 낙하축이나 플레이어 반대 방향의 선택 규칙이 아니다. 시간0의 delta=identity에서 초기 자세를 유지하고, 매 프레임 이전 결과에 누적 곱하지 않는다. 초기 위치/scale와 상대 계층을 재계산하거나 변경하지 않는다.

기울기 함수의 기존 SmoothStep 0→84도, 기존 .9초, owned의 ClockPaused일 때 elapsed 정지, 끝난 뒤 기존 제거 의미는 유지 후보다. legacy의 별도 unscaled 시간 경로는 변경하지 않는다. **이는 기존 yaw 소실 결함을 고치는 제안이며 exact 현재 화면/궤적 보존과 동일한 약속이 아니다.** 회전된 부모·비균일 scale의 상대행렬이 A1 지원 조건을 벗어나면 거부하며, 새 보정/bake로 통과시키지 않는다. .9초/84도가 지면에 맞는 품질값이라고 인증하지 않는다.

### 11.5 밑동·지면·낙하 방향의 미제공 근거

기존 `RendererBounds`는 비활성 포함 전체 Renderer의 월드 AABB를 합친다. `(center.x,min.y,center.z)`는 계속 **기존 pivot 휴리스틱**이며 줄기 절단점/실제 밑동/지지면 접촉점의 측정값이 아니다. 새로운 피봇값을 이번 Draft에서 만들지 않는다.

`World공간표현조립Coordinator.AlignAndValidate/SurfaceWorldY`에는 Standing 배치의 높이표본과 visibleBottom 정렬 검사가 존재한다. 그러나 현재 TreeFall 경로에는 해당 표면 판본·표본/법선·검증 receipt, 쓰러질 전체 궤적의 지면 높이·주변 장애물·최종 접촉 근거가 전달되지 않는다. Standing 정렬 성공은 낙하 궤적/지면 관통 없음의 증거가 아니다.

향후 개발·공간이 실제 대상 placement/Target/원본hash·현재 부모TRS·표면 판본/표본/기준점·실패 사유를 동결해 반환해야 한다. 이번 A2a에서 새 지면 API/높이/Collider/피해/길 봉쇄/추가 자원/음량을 결정하지 않는다. 실제 낙하 접촉·관통·결과 통나무와의 연속 판독은 공간 후속 증거로 남긴다.

### 11.6 정확한 수정 후보 소유와 선행 검증

다음은 **후속 승인 시의 경로 제안**이며 현재 파일 쓰기 배분이 아니다.

| 담당·경로 | 후보 범위 / 보존 |
| --- | --- |
| 개발: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.벌목.cs | 권위 사전복사 별도 private 메서드, wrapper/pivot 활성 수명, owned 초기회전 합성과 실패진단·자기정리만 |
| 개발: Assets/Ssalddel/Tests/EditMode/Nature벌목표현연결Tests.cs | 기존 권위 완료/중단/Load·세대·단일효과 소비 회귀에 준비실패/활성전이/초기회전 사례 추가 후보 |
| 전문: Assets/Ssalddel/Editor/NatureWoodcuttingValidation/Tests/벌목정적시각Pine호환Tests.cs (+meta) | 새 전용 실물 Pine 읽기/참조·설정 대조 후보. 정확 실행 범위·Editor 소유 배분 후만 작성/실행 |
| 보존: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs | 도끼 두 호출·legacy TryCreateTreeFallVisual/AnimateTreeFall·기존 audio/bounds helper 그대로. 필요 변경 발견 시 별도 반환 |
| 보존: Presentation/NatureWoodcutting/벌목정적시각복사Factory.cs 및 Editor/NatureWoodcuttingValidation/Tests/벌목정적시각복사Tests.cs | A1 기준선/45시험 유지. 지원 계약 확장·asmdef 변경 없음 |
| 개발·공간 별도 결정 | Coordinator/표면 계약·실제 marker 관찰·Game View. 이 Draft로 해당 파일 쓰기나 Editor 점유를 승인하지 않음 |

합성 회귀는 (a) 준비 직후 pivot=false/wrapper=true/activeInHierarchy=false 및 초기행렬·LOD 참조 보존, (b) 최종 접촉 전 비노출/완료 1회 후 활성·타 작업 근거 거부, (c) 시간0 초기회전 동일·회전된 부모에서 고정 baseline×delta·scale불변·pause/기존 .9초·84도, (d) A1 거부/생성예외/원본 소실에서 fallback0·자기정리·중복AudioFX0, (e) 취소/Load/중단/Disable 뒤 자기 소유만 해제, (f) legacy/도끼 경로의 소스·소비 불변으로 나눈다. 예상 수나 실제 통과를 사전 선언하지 않는다. MonoBehaviour 본문 격리 시험과 실제 Play 생명주기도 구별한다.

실물 Pine 읽기 평가와 실제 marker preflight는 합성 회귀로 대체할 수 없다. 준비 성공 뒤만 owned 회전 시험을 진행하고, 미지원 설정을 제거해 성공 표본을 만들지 않는다. 실제 화면·낙하·청취 성공은 별도 범위다. 개발이 11절 전체 읽기 → Accepted A2a/hash·명세 재결속 → 정확 파일/읽기·시험 슬롯 배분 전에는 코드/Editor/Assets/Blender 작업을 시작하지 않는다.

## 12. 수용 추록 2 — Accepted A2a 한정 기술 기준

### 12.1 수용 대상과 현재 권한

2026-08-31, 검토자 **개발 `01a02198-8b2a-7491-ac93-366b30ff474c`**. 개발은 11절 전체와 제출본 SHA256 **`67D9329F04F765A9548AB87305FFF27C9163D5415FF040BD58AF4B9BB2EAA962`**를 직접 읽고, 아래 A2a 한정 기술 기준을 수용한다고 반환했다. 해당 전체 파일 hash가 수용 대상 판본이며 기존 A1의 선행 수용과 구분한다.

**현재 연구 상태: A1 Accepted 유지 + 이 12절 범위의 A2a 기술 기준만 Accepted.** 11절의 Draft 원문과 기존 1~10절은 당시 이력으로 그대로 보존한다. 상단 및 과거 절의 Draft/미승인 서술을 조용히 덮어쓰지 않으며, A2 전체·실제 지면·청취·Blender B가 수용된 것으로 해석하지 않는다.

이번 배분은 **같은 연구에 이 수용 추록을 추가하고 최신 hash를 개발에 반환하는 문서 작업뿐**이다. 최신 hash의 명세 재결속과 정확한 파일·Editor/시험 슬롯의 명시 시작 통보 전에는 코드/Assets/Editor 작업을 하지 않는다. 기존 보고·원장·작업 명세는 이 담당이 수정하지 않는다.

### 12.2 수용한 변경과 반드시 보존할 경계

- 권위 Working 중 사전 복사와 owned 낙하만 별도 메서드에서 A1 helper로 연결한다. 개발 소유 비활성 pivot → A1의 비활성 wrapper → pivot이 숨겨진 동안 wrapper 활성 준비 → 기존 최종 완료 접촉 소비 뒤 pivot 활성이라는 명시 수명을 따른다. 준비 실패는 진단과 자기 조각 정리로 끝내고 기존 평탄화 helper로 fallback하지 않는다.
- source 초기 월드행렬·계층·LOD 참조/설정 보존을 검증한다. 원본/기존 부모의 TRS나 메시·재질을 보정하지 않는다. destination은 개발 소유 비활성 container이며 계층 변경 callback/외부 writer가 없다는 입력 전제를 확인한다. 비활성만으로 임의 부모의 무부작용을 보장하지 않는다.
- owned 회전은 한 번 고정한 `initialLocalRotation * localX 기울기 delta`로 합성해 시간0의 초기 자세를 유지한다. 이 변경은 **기존 yaw 소실 결함의 제한적인 교정**이며 exact 기존 화면/궤적 보존과 동일하지 않다. 현재 결과를 유지했다는 표현으로 교정을 숨기지 않는다.
- 기존 bounds의 center.x/min.y/center.z 피봇 휴리스틱, SmoothStep 0→84도, .9초, owned ClockPaused 정지, 완료·접촉·AudioFX 단일 소비와 중단/Load/소유 정리 의미를 유지한다. 새로운 월드축·각도·시간·높이·Collider·권위·음량을 결정하지 않는다.
- `Nature감각표현Presenter.cs`의 도끼 두 호출과 legacy `TryCreateTreeFallVisual`/`AnimateTreeFall`은 변경하지 않는다. A1 Factory/합성 Tests 소스 및 기존 asmdef도 불변이다. 개발 partial/연결 회귀와 전문 실물 Pine 읽기 시험의 정확 경로는 11.6 후보를 바탕으로 개발이 별도 배분한다.

### 12.3 실물 선행 관문과 미확인 override

**실물 Pine 읽기 평가 및 실제 marker preflight는 실행 연결 전 선행 관문**이다. 합성45 통과나 prefab YAML만으로 실제 호환을 승인하지 않는다. 승인된 읽기 슬롯에서 원본 GUID/hash·mesh/material·LOD/Renderer 설정을 대조하고, 실제 관측의 Session/Actor/OriginCommand/Target/세대/revision·source/부모 현재행렬·활성·StaticFlags·MPB·probe·LOD 내부참조·bounds를 확인해야 한다. 원본을 제거한 뒤 새 대체 나무로 관문을 통과시키지 않는다.

현재 검출 가능한 지원외 설정은 A1 진단으로 명시 거부한다. **ForceLOD 강제선택 이력, 진행 중 crossfade, 계산값과 우연히 같은 custom worldBounds override, source 외부 LODGroup 역참조**는 부분트리 공개 조회만으로 검출·보존 완료를 주장하지 않는다. 입력 소유/외부 작성자 부재가 확인되지 않은 경우 미확인을 유지해 개발에 반환하며, 이번 수용으로 해당 지원을 추가하거나 실패 보호를 완화하지 않는다.

Standing 배치의 표면 정렬은 실제 줄기 절단점·낙하 궤적·지면 접촉·장애물 비관통의 근거가 아니다. 기존 피봇 휴리스틱을 실제 밑동/ground로 인증하지 않는다. 실제 지면·낙하 판독·청취와 Blender B/키프레임/bake/FBX 가공은 별도 미승인이다. 이 문서 수용 자체는 실물 Pine·실제 Play·낙하 성공이나 E 승격이 아니다.
