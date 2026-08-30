# 약초·Recipe·조합 제작 문답

## 식별

- 문답 고유 식별자: `inquiry:herbal-recipe-crafting.r1`
- 대상 PlayableLoop: `playable-loop:nature-basic-herbal-recovery.v1`
- 이관 질문: `Q-045~Q-050`, `Q-061~Q-064`, `Q-068~Q-071`, `Q-131`, `Q-133`, `Q-142`, `Q-150`, `Q-157`, `Q-269~Q-296`, `Q-340~Q-346`
- 상세 원문·조사 계보: [동결 통합 아카이브](../nature-night-day2.inquiry.r1.md)
- 상태: `Refining`
- 마지막 확인 질문: `Q-약초제작-D3-06 · 전체 Q-346` / 선택 `1` / `2026-08-30`
- 다음 질문: `Q-약초제작-D3-07 · 전체 Q-347` / 물 운반 용기의 첫 범위 / `Asked`

## 이 문서가 소유하는 질문

- 위험 수면 뒤 체온 안정과 약초 예방·치료
- Recipe 발견·학습·Multiplayer 전수
- 건강·위험 누적·초기 질병·심한 질병
- 미학습 Recipe 추론과 자동 관찰 카드
- 정체불명 혼합물, 재료 소비, 집중력 감소와 위협 상승
- 통합 카드 서랍의 Recipe 탭
- 3×3 자유 추론과 5×5·7×7·9×9 정식 Recipe 요구
- 플레이어 자유 Recipe 작성 보류

## 현재 확정 기준

- Solo와 Multiplayer 모두 3×3 조합을 기억·추론할 수 있다.
- Recipe 카드는 제작 허가가 아니라 정보 관리와 빠른 재사용을 돕지만, 5×5 이상 고등 조합에는 정식 Recipe 지식이 필요하다.
- 추론 성공은 `SelfExperiment` 관찰 카드를 만들고 효능·부작용은 사용·분석으로 채운다.
- 실패는 재료를 소비해 정체불명 혼합물을 만들고 집중력을 낮추며 위협을 높인다.
- Recipe·청사진·타로·역할/수집은 하나의 카드 서랍 UI를 공유하되 권위와 효과를 분리한다.
- 플레이어 자유 Recipe 작성·효과 정의·공유는 후속 revision까지 보류한다.
- 첫 따뜻한 약초차는 체온 회복·질병 위험 감소·초기 질병 치료를 제공하고 심한 질병에는 보조 효과만 제공한다.
- `Q-346`: 첫 약초차용 물 한 번 분량은 폐야영지에 남아 있고, 이후에는 주변 수원에서 직접 구한다. 이는 기획 결정이며 물 재고·수질·채수 WI 구현이나 E 승격을 뜻하지 않는다.
- `Q-131`: 첫 `WI-ACTOR-03 지식 습득`의 E4 발생 문맥은 물리적인 기록 발견을 기준 후보로 삼는다. 열린 책·낱장 처방전·현장 기록판은 `ReadableKnowledgeSource`의 표현 후보이며 Synty Prefab 자체는 권위 상태를 바꾸지 않는다.
- `Q-133`: 첫 물리 Recipe 기록은 폐야영지에 둔다. Nature 탐색 중 독립적으로 발견해 `지식 습득 → 약초 재료 탐색 → 회복 준비`로 이어지며 Farm·Town 진입을 선행 조건으로 요구하지 않는다.
- `Q-142`: 3×3 조합은 작업대 선택 시 현재 세계가 계속 보이는 반투명 패널로 연다. `Q-341` 이후 약초 폐루프의 기본 1인칭 시점을 강제로 3인칭으로 바꾸지 않는다. 키보드·마우스는 Drag & Drop, 게임패드는 같은 Slot 구조의 Focus 이동을 사용한다.
- `Q-150`: 조합 패널을 닫아도 작업대별 3×3 초안을 보존한다. 초안은 재료를 예약·소비하거나 제작 Task·WorldRevision을 만들지 않는 Presentation 작업 상태다.
- `Q-157`: 주변에 위협 징후만 있을 때는 Recipe 조회·Preview·초안 편집을 허용하되 제작 Confirm을 차단한다. 실제 전투가 생성되면 패널을 닫고 전투 조작으로 인계하며 작업대별 초안은 보존한다.

## Q-142 3×3 조합 입력·표현 기준

- 진입: 플레이어가 유효한 작업대 `InteractionAnchor`에 접근해 조합 행동을 선택한다.
- 화면: 현재 1인칭 카메라와 주변 위험·Actor 상태를 완전히 가리지 않는 반투명 3×3 패널을 사용한다. 플레이어가 직접 3인칭으로 전환한 상태도 허용하지만 패널이 시점을 강제로 전환하지 않는다.
- 키보드·마우스: 재료 원본에서 Slot으로 Drag & Drop하고, 우클릭 또는 취소 입력으로 원위치시킨다.
- 게임패드: 재료 목록과 3×3 Slot을 Focus 이동하고 선택·배치·회수 입력을 같은 명령 의미에 연결한다.
- Preview: 배치 중에는 재료를 소비하지 않고 조합 후보·알려진 정보·불확실성과 차단 이유만 갱신한다.
- Confirm: 별도 제작 Confirm에서 현재 Slot 배치와 ExpectedRevision을 검증한 뒤에만 재료 예약·소비 또는 제작 Task를 시작한다.
- 중단: `Q-150`에 따라 패널을 닫으면 해당 WorkbenchStableId의 미확정 Slot 배치를 초안으로 보존한다. Simulation 재료 상태는 바뀌지 않는다.
- E5~E6 확인: 3인칭 가시성, 16:9·울트라와이드 해상도, Drag 목표 크기, 게임패드 Focus 순서, 위험 중 입력 잠금과 접근성을 실제 Game View에서 검증한다.

## Q-150 작업대별 조합 초안

- 초안 식별: `PlayerStableId + WorkbenchStableId + DraftStableId`로 구분하고 Slot별 재료 참조와 배치 순서만 가진다.
- 비권위 경계: 초안 저장은 재료 수량, 예약, Recipe 지식, 제작 결과, WorldTick 또는 WorldRevision을 변경하지 않는다.
- 다시 열기: 같은 플레이어가 같은 작업대를 열면 현재 보유 재료와 권위 revision을 다시 조회한 뒤 유효한 Slot만 복원한다.
- 불일치: 초안 작성 뒤 재료가 소비·이동·손실됐다면 해당 Slot을 `Unavailable`로 표시하고 Confirm을 차단하며 조용히 다른 재료로 대체하지 않는다.
- 범위: 다른 작업대나 다른 플레이어와 초안을 자동 공유하지 않는다.
- 저장 정책 미정: 게임 종료·Load 뒤에도 초안을 유지할지는 Save 권위 상태와 분리된 사용자 편의 저장소의 판본·손상 복구 기준을 정한 뒤 결정한다.
- 폐기: 플레이어가 명시적인 초안 초기화를 선택하면 이 Presentation 초안만 지우고 실제 재료 상태는 유지한다.

## Q-157 위협 중 Recipe 입력 경계

- `ThreatWarning`: Recipe 카드 조회, 재료 정보 확인, 3×3 초안 편집과 Preview는 허용한다.
- `ThreatWarning` 차단: 재료 예약·소비 또는 제작 Task를 시작하는 Confirm은 차단하고 현재 위협과 안전 조건을 안내한다.
- `CombatLinked`: 연결 전투 식별자가 생성되면 조합 패널을 닫고 이동·회피·전투 입력으로 즉시 전환한다.
- 초안 보존: 전투 인계로 패널이 닫혀도 Q-150의 작업대별 초안은 유지하며 재료는 예약하지 않는다.
- 전투 후: 같은 작업대를 다시 열면 현재 재료·작업대·위협·ExpectedRevision을 다시 조회한 뒤 유효 Slot만 복원한다.
- 진행 중 제작: 이미 Confirm된 제작 Task가 있다면 UI 패널 종료만으로 취소하지 않고 해당 WI의 위험 중단·취소 정책을 따른다.
- 권위 경계: Unity가 패널을 닫거나 HUD를 전환해도 위협·전투·제작 상태를 독자적으로 변경하지 않는다.

## Q-131 자산 조사와 E4 준비

- 주 후보: `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_BookOpen_01.prefab`
- 대체 후보: Town 닫힌 책·책 묶음, City `Paper_01~05`, Generic `Papers_01~06`, Construction `Clipboard_01`
- 문맥 후보: Nature `CampFire_01`, 범용 상자·탁자, Construction `Rubbish_Papers_01~03`
- 현재 판정: 후보 존재는 확인했지만 실제 크기·Collider·시인성·배치와 Game View는 미검증이므로 E5 준비 상태는 `Conditional`이다.
- 분류 결함: 현재 자동 분류에서 `book`은 `interior-loose-item`에 들어가지만 `paper`·`clipboard`는 지식 출처 역할로 자동 분류되지 않는다. E5 전에 수동 승인 또는 분류 토큰 보완이 필요하다.
- 경계: 이 조사는 Presentation E4 인계 준비이며 실제 Scene 배치나 통합 E4·E5 승격 증거가 아니다.
- 배치 문맥: 첫 기록은 폐야영지의 통행을 막지 않는 지지면에 놓고, 3인칭 접근 방향에서 열린 책 또는 낱장 기록이 보이게 한다.

## 기존 개발 인계

- `WI-ACTOR-03 지식 습득`: `Logic E4 / Presentation E3 / 통합 E3`
- 같은 WorldRevision의 지식 원장과 Preview를 `Known / Readable / Blocked` 처방 카드 상태로 결정적으로 투영하는 읽기 사본까지 구현했다.
- Save/Replay·실제 Unity Scene 배선·Recipe UI 조작·Play Mode·Game View·채집·달이기·섭취·약효는 아직 미구현이다.

## 다음 질문 후보

- 집중력 부족 시 Preview·Confirm 제한
- 공식 Recipe의 발견·분석·전수 UI

## Q-269~Q-296 공식 약초·처방·획득 지식 확장

이 구간은 문답에서 확정된 기획 재료이며 아직 `nature-basic-herbal-recovery.design.r5`에 승인·결속되지 않았다. 기존 활성 Goal의 `Logic E4 / Presentation E4 / 통합 E4`를 승격시키지 않는다.

| 질문 | 확인된 결정 | 반영 점검 |
| --- | --- | --- |
| Q-269 | 공식 약초는 실명을 사용하되 게임 효과는 독립 규칙으로 분리한다. | 기획 기록, 기획서 미결속, 구현 미착수 |
| Q-270 | 첫 약초차 효과는 체온·질병 위험을 단계형으로 판정한다. | 기획 기록, 기획서 미결속, 구현 미착수 |
| Q-271 | 감국+향유 고정 조합은 폐기하고 Q-284의 공식 자료 검토 방식으로 대체한다. | `Superseded` |
| Q-272~Q-274 | 중단된 응답에는 선택 결과만 남고 질문 본문이 남지 않았다. | `NeedsSourceRecovery`; 추측 금지 |
| Q-275 | 관찰 정보와 Recipe 지식을 대조한 뒤 실명을 식별한다. | 기획 기록, 구현 미착수 |
| Q-276 | 게임 설명을 먼저, 상세 화면에는 학명·사용 부위·공식 출처를 표시한다. | 기획 기록, 구현 미착수 |
| Q-277 | 하루 경계에서 계절·서식지·채집 압력에 따라 결정적으로 일부 재생한다. | 기획 기록, 구현 미착수 |
| Q-278 | 첫 채집은 2초 누르기 Task이며 승인된 이용 부위만 획득한다. | 기획 기록, 구현 미착수 |
| Q-279 | 기초 3×3 조합은 중앙의 물과 인접한 약초 재료 배치를 사용한다. | 기획 기록, 구현 미착수 |
| Q-280 | 완성은 솥의 김·따뜻한 색·컵 아이콘·짧은 소리로 함께 판독한다. | 기획 기록, Runtime 미검증 |
| Q-281 | 약초는 서식 조건에 맞는 H2에서 군집 후보를 생성한다. | 기획 기록, H 결속 미구현 |
| Q-282 | 어린잎·꽃은 맨손, 뿌리·줄기는 후속 도구 Capability를 요구한다. | 기획 기록, 구현 미착수 |
| Q-283 | 미확인 위험 재료는 관찰 정보와 위험 신호만 제공하고 정답은 숨긴다. | 기획 기록, 구현 미착수 |
| Q-284 | 첫 실제 처방은 공식 전통 처방 자료를 조사해 Fixture 후보를 선정한다. | 기획 기록, 전문 자료 동결 필요 |
| Q-285 | Nature에서 불완전한 실제 처방 기록을 발견하고 여러 영역에서 완성한다. | 기획 기록, 영역 통합은 후속 |
| Q-286 | 카드 첫 화면은 게임 설명·재료, 상세 탭은 원문 구성·용량·출처를 표시한다. | 기획 기록, UI 미구현 |
| Q-287 | 부족 약재는 Nature 탐험·Farm 재배·Town 가공·Hub 거래로 획득 경로를 분리한다. | 기획 기록, 각 영역 독립 폐루프 선행 |
| Q-288 | 손상된 기록은 확인·미확인 약재와 출처 단서를 Slot으로 보여 주며 복원한다. | 기획 기록, 구현 미착수 |
| Q-289 | 공식 g 단위는 상세 카드에 보존하고 제작에서는 같은 비율의 배합 단위로 변환한다. | 기획 기록, 계산 Profile 미동결 |
| Q-290 | 약재는 `Fresh / Dried / Processed`로 구분하고 공식 처방은 요구 상태를 검사한다. | 기획 기록, 구현 미착수 |
| Q-291 | 공식 자료의 전통적 용도와 현재 게임 효과를 별도 영역으로 표시한다. | 기획 기록, 구현 미착수 |
| Q-292 | 자동 대체는 금지하고 승인된 변형 Recipe만 플레이어가 선택한다. | 기획 기록, 구현 미착수 |
| Q-293 | 획득처는 탐험·의뢰·교류로 얻은 단서만 카드와 지도에 기록한다. | 기획 기록, 구현 미착수 |
| Q-294 | 획득 단서는 넓은 환경→지역→정확한 장소 순으로 구체화한다. | 기획 기록, 구현 미착수 |
| Q-295 | 알아낸 획득처는 플레이어 지식 원장과 Save/Replay에 보존한다. | 기획 기록, 다음 Save 판본 필요 |
| Q-296 | 미확인 재료는 실루엣과 꽃·뿌리·잎 같은 넓은 분류부터 보여 준다. | 기획 기록, Presentation 미구현 |

### 현실 자료와 게임 권위 경계

- 공식 명칭·학명·약재 구성·원문 용량·출처는 승인 ReferenceCatalog가 소유한다.
- 체온·질병·회복 수치는 Simulation의 판본화된 게임 Profile이 소유한다.
- 외부 자료 갱신은 기존 Save의 Recipe·효과·획득 지식을 자동 변경하지 않는다.
- 의료 효능을 주장하지 않으며 정보 카드에서 공식 전통 용례와 게임 효과를 명확히 분리한다.

## Q-340~Q-345 첫 약초 회복 폐루프 압축

이 구간은 `처방 발견 → 약초 식별·채집 → 달이기 → 섭취 → 회복 → 다음 탐험 또는 휴식`을 한 번의 짧은 플레이로 닫기 위한 후속 확정 후보다. 아직 `nature-basic-herbal-recovery.design.r5`와 E7 작업 명세에 승인·결속되지 않았으며 현재 E를 승격하지 않는다.

| 질문 | 확인된 결정 | 반영 점검 |
| --- | --- | --- |
| Q-340 | 첫 폐루프는 폐야영지에서 처방 기록·냄비·컵을 발견하고 주변 약초를 찾는 구성으로 시작한다. | 기획 기록, H1 실제 배치 미검증 |
| Q-341 | 발견·식별·채집·불 피우기·달이기는 월드 행동으로 수행하고 Recipe 카드는 정보 확인과 재료 정리에 사용한다. 약초 폐루프는 기본 1인칭이며 패널이 3인칭 전환을 강제하지 않는다. | 기존 Q-142의 강제 3인칭 표현을 대체, 입력·카메라 미구현 |
| Q-342 | 잘못 달이면 재료를 소비한 `정체불명 혼합물`이 생기고 개인 위협·정신력에 작은 손실이 발생한다. 재채집 후 다시 시도할 수 있어 폐루프를 막지 않는다. | 규칙 Profile·수치 미동결 |
| Q-343 | Nature 약초 탐색은 기본 1인칭으로 시작하며 플레이어가 원할 때 3인칭으로 전환할 수 있다. | 1·3인칭 전환·멀미 접근성 미검증 |
| Q-344 | 가까운 식물을 조준하면 약한 윤곽과 관찰 표시가 나타난다. 지식이 있는 약초만 이름과 효능 단서를 보여 주고 미확인 식물은 넓은 외형 분류로 남긴다. | 관찰 거리·윤곽 강도·색각 접근성 미동결 |
| Q-345 | 첫 채집은 약 2초 누르기 Task로 유지한다. 1인칭 손 또는 도구, 식물 흔들림·꺾임, 채집음과 획득 피드백을 함께 사용한다. | 전용 손 동작·접촉 프레임·오디오 자산 미확보 |

### 보유 Synty 표현 자산 조사

기준 프로젝트는 `C:/Users/user/ssalddel`이며 아래 후보의 Prefab과 `.meta` GUID 존재를 직접 확인했다. 원상품명은 표현 계보일 뿐 `VisualKey`, WI 상태, 약초 실명이나 Simulation 효과의 권위가 아니다.

| 표현 역할 | 의미 기반 후보 | 확인한 Synty 후보 | GUID | 판정 |
| --- | --- | --- | --- | --- |
| 미확인 잎 식물 | `Nature.Herb.Unknown.Leafy` | `Assets/Synty/PolygonNature/Prefabs/Plants/SM_Plant_01.prefab` | `2535f726e7be4c44cbf8998490f36b8a` | 사용 가능 후보 |
| 꽃 약초 외형 | `Nature.Herb.Unknown.Flowering` | `Assets/Synty/PolygonNature/Prefabs/Plants/SM_Plant_Flowers_01.prefab` | `d7bb738e4e0288b42b577570c010f6df` | 사용 가능 후보 |
| 보라색 꽃 약초 외형 | `Nature.Herb.Unknown.PurpleFlowering` | `Assets/Synty/PolygonNature/Prefabs/Plants/SM_Plant_PurpleFlower_01.prefab` | `e5ebf37e1cf2d85439deb462748a7d29` | 사용 가능 후보 |
| 고지대 꽃 군집 | `Nature.Herb.AlpineFlowerCluster` | `Assets/Synty/PolygonNatureBiomes/PNB_Alpine_Mountain/Prefabs/SM_Env_Flowers_01.prefab` | `42c2aba14d57fbd4cb4c6e92b06686ec` | 서식 Profile 후보 |
| 폐야영지 열원 | `Survival.Camp.HeatSource` | `Assets/Synty/PolygonNature/Prefabs/Props/SM_Prop_CampFire_01.prefab` | `1fbdd99ef1d1e2b4dac24a8d8ef04741` | 사용 가능 후보 |
| 달이기 용기 | `Survival.Brew.Vessel` | `Assets/Synty/PolygonDungeonRealms/Prefabs/Props/SM_Prop_Camp_Pot_01.prefab` | `29fc435e87f624c4aa417f109a26bdf2` | 사용 가능 후보 |
| 음용 컵 | `Survival.Brew.Cup` | `Assets/Synty/PolygonGeneric/Prefabs/Props/SM_Gen_Prop_Mug_01.prefab` | `d23c715168b31804ba6fa370a6502b8f` | 사용 가능 후보 |
| 물리 처방 기록 | `Knowledge.Recipe.PhysicalRecord` | `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_BookOpen_01.prefab` | `94b71dbe7c75f5c46b056c1ceb10909e` | 기존 E4 주 후보 |

- Synty 식물 외형을 감국·향유 같은 실제 약초 종과 일대일로 단정하지 않는다. 승인 ReferenceCatalog의 실명·서식 정보와 별도로 `VisualProfile`을 연결한다.
- PolygonNature의 식물·꽃·버섯·양치·갈대와 Alpine Mountain의 꽃·꽃 관목은 여러 서식 표현 후보를 제공한다. 첫 폐루프에서는 위 동결 후보 몇 개만 검증하고 전체 자산을 한꺼번에 Scene에 넣지 않는다.
- Dungeon Realms의 `Camp_Pot_01~07`, Generic 머그·병, Nature 모닥불을 결합하면 폐야영지의 H1 `ReadableKnowledgeSource + HerbGatheringArea + BrewHeatSource + BrewVessel + DrinkAnchor`를 표현할 재료는 충분하다.

### 애니메이션 조사와 적용 경계

- `AnimationBaseLocomotion`에는 721개 FBX와 174개 Animator Controller가 있으며, `A_Stand_ToCrouch_*`, `A_Idle_Crouching_*`, `A_Crouch_ToStand_*`의 실제 임포트 clip 설정을 확인했다. 이는 웅크리기 진입·유지·복귀와 3인칭 몸·그림자 표현 후보로 사용할 수 있다.
- `AnimationEmotesAndTaunts` 283개 FBX와 `AnimationSwordCombat` 242개 FBX·Controller 1개도 존재하지만, 파일명·임포트 메타데이터 조사에서 약초 채집·손 뻗기·줄기 뽑기·냄비 젓기·따르기·마시기 전용 clip은 확인되지 않았다.
- 첫 1인칭 채집은 `Crouch 진입 → 시선 대상 고정 → 손/도구 Reach → 접촉 프레임 → 식물 상태 전환 → 손 복귀 → Crouch 해제`의 `AnimationIntent` 계약이 별도로 필요하다. Base Locomotion만으로 전용 손 동작 완성을 주장하지 않는다.
- `Assets/Synty/Tools/SyntyPropBoneTool`은 획득한 약초·머그 같은 소품을 손 뼈에 결속하는 후보 도구로 사용할 수 있다. 실제 손 위치, clipping, 남녀/Sidekick rig 호환과 1인칭 카메라 근접 시인성은 별도 시험이 필요하다.
- Animation Rigging 패키지는 현재 Unity `Packages/manifest.json`에서 확인되지 않았다. 초기 Prototype은 기존 `공용AnimationAdapter`의 절차형 대체 동작 또는 전용 손 Root 보간을 사용할 수 있지만, Presentation E6 이상에서는 접촉 프레임·손가락/손목·도구 Socket·취소 복귀를 실제 Game View로 검증해야 한다.

### 오디오 조사와 적용 경계

- 발견된 약초 채집·달이기·섭취 Cue의 장기 상태는 [PlayableLoop 오디오 요구사항 대장](../../오디오요구사항대장.md)의 `audio:nature:herb:*`, `audio:nature:brew:*` 항목으로 관리한다. 이 문서는 Q-340~345에서 확인한 플레이어 의미를 소유하고 중앙 대장은 생성·구매·결속·청취 상태를 소유한다.

- 현재 Unity `Assets` 전체에서 `.wav`, `.mp3`, `.ogg`, `.aif`, `.aiff` 파일은 0개다. Synty 팩에 약초 채집이나 조리 오디오가 포함됐다고 기록하지 않는다.
- 기존 `Nature감각표현Presenter`에는 3D `AudioSource`, cue routing과 절차형 대체 효과음 생성 경로가 있고, `SkyEnginePresenter`에는 절차형 비·천둥 오디오가 있다. 첫 Prototype은 같은 표현 경계를 재사용해 `foliage-rustle`, `herb-pluck`, `gather-confirm`, `fire-crackle`, `water-pour`, `brew-bubble`, `mug-contact`, `drink`, `warmth-success` cue를 임시 절차음으로 구분할 수 있다.
- 최종 자연스러운 숲 채집 감각에는 라이선스가 명확한 실녹음 또는 별도 오디오 팩이 필요하다. 절차형 beep만으로 Presentation E6·E7의 청각 완성도를 주장하지 않는다.
- 실제 승인 조건은 1인칭 거리에서 잎 스침→접촉→채집 완료의 시간 동기화, 모닥불·끓임의 공간감, 반복 피로도, 음량·효과음 끄기 접근성, Console 오류 0과 사람의 청취 확인이다.

### 이번 조사 판정

- `ConfirmedAvailable`: 식물·꽃·고지대 꽃 군집, 모닥불, Camp Pot, 머그, 열린 책, 웅크리기 이동/Idle clip, 소품 손 결속 도구
- `PrototypeFallbackAvailable`: 기존 절차형 Animation·3D Audio cue 경계
- `AdditionalAuthoringRequired`: 1인칭 손 뻗기·뽑기·젓기·따르기·마시기 동작, 실제 숲·식물·불·물·컵·음용 오디오
- `NotEvidenceYet`: 실제 Prefab Bounds·Collider·InteractionAnchor·손 Socket·Scene 배치·Play Mode·Game View·청취 검증을 하지 않았으므로 이 조사는 다음 기획 revision의 Presentation E4 준비 자료일 뿐 E5 증거가 아니다.

## Q-346 첫 약초차 물 확보

- 질문: `Q-약초제작-D3-06 · 전체 Q-346` — 첫 약초차를 달일 물은 어떻게 마련하는가?
- 확인일·답변: `2026-08-30`, 사용자 선택 `1`.
- 확인된 결정: 첫 1회분은 폐야영지에 남아 있고, 이후에는 주변 수원에서 직접 구한다.
- 의도: 첫 회복 폐루프를 물 탐색으로 막지 않고, 반복 플레이부터 수원 탐색·운반으로 확장한다.
- 이번에 정하지 않은 것: 물의 정확한 양·수질·정화 조건, 채수 시간·비용, 운반 용기, 수원 거리와 실제 배치. 기존 물 관련 WI가 있으면 먼저 재사용 여부를 확인한다.
- 오디오 참조: [중앙 오디오 대장](../../오디오요구사항대장.md)의 `audio:nature:water:collect.r1`(새 발견), `audio:nature:brew:water-pour.r1`(재사용 후보).
- 구현 경계: 현재 승인된 기획·작업 명세·Q-001~339 구현 원장은 변경하지 않았다. 문답 확인은 Logic·Presentation Evidence가 아니다.

### 다음 질문 — Q-347 물 운반 용기

- 식별: `Q-약초제작-D3-07 · 전체 Q-347`
- 상태: `Asked`, 사용자 답변 없음.
- 추천 후보: 첫 반복에서는 별도 물통 없이 폐야영지에서 찾은 냄비로 물을 떠 와 달이고, 컵은 마시는 데 사용한다.
- 질문: 물 운반용 새 도구를 먼저 요구하지 않고 기존 냄비·컵으로 첫 반복을 닫는 방향이 적절한가?
- 근거·제한: Q-340~345에서 냄비·컵 Prefab 후보는 확인했지만 물 담김·운반 손 동작·수량 상태는 미구현·미검증이다. 이번 추천을 확정 결정으로 취급하지 않는다.
