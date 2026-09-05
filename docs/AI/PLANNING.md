# Mirror 기획 목차

> 현행 스토리 기준 `story-inspiration.r1` (2026-09-05): [스토리 영감과 플레이 진행 분리](../Architecture/스토리영감과플레이진행분리.md). 인물·사건·선택을 중심으로 기획하고 괘·효는 영감과 참조 이력으로 사용한다. 아래 이전 판본의 64캠페인·육효 순서·제작 커서는 새 기획 관문이 아니다. 캠페인 복원 기획은 `hexagram-campaign-reset.r2`를 따른다.

> 게임 기획의 현재 판본과 상·하위 관계를 찾는 시작점이다. 세부 본문은 각 기획 문서가 소유하며 이 목차는 내용을 복제하지 않는다. 운영 기준은 [기획 문서 독립 관리 체계](../Architecture/기획문서독립관리체계.md), 단계적 경로 통합은 [현행 기획 정본 경로](Planning/README.md)를 따른다.

## 읽는 순서

1. 공통 기획 방식
2. 메인 스토리와 하위 이야기
3. 현재 사건·플레이 기획
4. World·Graph Map 기획
5. 자료·표현 인계 기획

## 현재 문답 우선순위

- 메인 스토리 문답은 [영감 중심 기획 r16](Planning/스토리/PLAN-STORY-HEXAGRAM-SEQUENCE-001/README.md)에 따라 인물의 목적·갈등·선택·대가와 사건 인과부터 구성한다. 괘·효는 선택적 참고 자료이며 기존 이야기·효사 배정과 큰 줄기 제안은 참고 이력으로 보존한다. 다음 사건은 효 번호가 아니라 현재 이야기의 미정과 플레이 필요로 선택한다.
- 한스 농장의 첫 밭갈기 표본은 한스 집에서 보이는 가장 가까운 비통행 허용 구획으로 확정했다. 첫 NPC 학습 중점은 비울 수 있는 주 슬롯 한 칸, 초·중·후반 세 구간, 다음 구간 적용, 결속된 플레이어 행위의 이해도 `+1`만 E3로 동결했다. 관계 기반 취득·플레이어 멘토 공유·실제 카드 UI는 후속 범위다.
- Farm 수확 Lot은 물류 입구 목으로 이어지고, 입고·정돈 화→적재 완료 토→주문 배치·재고 할당 금(`토생금`)→포장·출고 인계·운송 수(`금생수`)로 순환한다. 도착 화물은 목적지·주문·수량·봉인/파손 상태의 인수 관문을 통과한 뒤에만 `수생목`으로 새 입고 작업을 연다. 상품별 인수 기준은 후속 Profile로 미뤘다.
- H2 오행 순환의 다음 질문은 Nature `자연 복원·안전 회복 블록`에서 재탐색을 열 수 있는 최소 복원선을 정하는 것이다.
- Hub 첫 이용은 플레이어가 이미 인지한 자기 필요를 해결하려고 주도적으로 찾아가는 것으로 확정했다. 실제 공간 배치 전에 Farm↔Hub 관계를 Graph Map에서 먼저 구체화하며, 플레이어 왕복 경로와 기존 화물 운송 엣지의 분리 질문은 다음 수평 순환으로 보존한다. 첫 대표 비료 수요는 경로 형태 뒤로 미뤘고 미도착 화물은 선택적 후속 사건으로 내렸다.
- 한 절기는 초반·중반·후반 세 구간과 두 내부 전략 분기로 운용한다. 방어를 기본 압력으로 두되 정찰·차단·전초 공격 같은 공세가 다음 방어 조건을 바꿀 수 있고, 상대 성 점령은 여러 절기에 걸친 별도 상위 캠페인으로 둔다. 첫 공세 상한은 다음 질문이다.
- 다음 수평 순환은 Town 첫 공방 제작 실패 → 요동성 첫 위협 전달 → City 첫 공공 문제 순서의 후보로 둔다. 앞 답변이 주변 기획을 바꾸면 순서를 다시 계산한다.
- 수치·임계값·정확 UI·Prefab·Clip·Collider·코드·Unity 검증과 원작·소실 원문 복구는 사용자 문답이 아니라 `P3`, `Hold`, `NotAQuestion`으로 분리한다.
- 전수 현행화에는 분야 우선순위를 두지 않는다. 전체 기준과 후속 큐는 [기존 기획 현행화·문답 우선순위 r9](전체기획-네관점순환이관-2026-08-31.md#11-전수-목록과-문답-큐의-역할을-분리한다)가 소유한다.

## 공통 기획 방식

| 기획 ID | 현재 문서 | 상태 | 역할 |
| --- | --- | --- | --- |
| `PLAN-GAME-COMMON-PURPOSE-001` | [나·상대 오행 순환과 광복기 상위 목적 r5](게임상위목적-오행순환과광복기-기획-2026-09-02.md) | `ApprovedPlanningBaseline / PurposeGatePropagatedToPlanningGraphDevelopmentWorkflow / LogisticsMirrorGoalAdded / ExactProfilesPending` | 나와 상대의 회복 순환을 상위 목적으로 유지하면서, 현실의 창고·상하차·배달을 사람·차량·행동으로 비추는 `Mirror 물류 표현`을 첫 대표 구현 목표로 사용 |
| `PLAN-PLANNING-PLAYER-CONTEXT-001` | [시간·공간·플레이어·대상 WI 기획 r3](시간공간플레이어대상-WI기획정리-2026-08-31.md) | `ApprovedPlanningBaseline / ContextFitActionDefined` | 지금·여기·나·너 속에서 현재 목적에 가장 적중하는 `이렇게`를 대표 추천하되 복수의 유효 행동과 실제 결과 판정을 보존하는 문답 기준 |
| `PLAN-PLANNING-WI-GWAE-001` | [WI 괘성 분류 체계 r10](../Architecture/WI괘성분류체계.md) | `ReviewedMetadata / IndependentFiveElementAccumulationConfirmed / RelativeDistributionProjectionConfirmed / FiveElementMeditationRelationEstablished / SubjectFirstSequentialPlanningEstablished / AllWorldObjectRoleActionE5GateEstablished` | Actor의 오행 활동값은 원소별로 독립 누적하고, UI에서만 선택 기간의 100% 상대 분포로 표현한다. 행위 발자국은 명상·숙련·이데아·회복·위협을 대체하지 않는다. |
| `PLAN-PLANNING-MIGRATION-001` | [기존 기획 현행화·문답 우선순위 r9](전체기획-네관점순환이관-2026-08-31.md) · [전수 목록 r7](기존기획-현행화-전수목록-2026-09-02.md) · [정본 경로](Planning/README.md) | `InProgress / FullInventoryBounded / CanonicalPathTransitionDefined / QuestionPrioritySeparated` | 현행 기획 45개의 계보와 경로를 우선순위 없이 전수 현행화하고, 실제 사용자 문답은 별도 우선순위 큐에서 한 질문씩 수평 순환 |
| `PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001` | [운영 서버 0.0~3.5에서 Mirror Unity로의 이관 r2](Planning/시스템/PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001/README.md) | `ApprovedForHandoff / InventoryImplemented / LogisticsMirrorGoalBound / HubFirstSliceApproved / PresentationE5Blocked` | 서버 업무를 사실·플레이 선택·세계 표현으로 번역해 창고 작업자와 화물, 도심 오토바이 배송이 살아 움직이는 현실 물류의 거울로 만든다. Hub 입고·검수·적치를 첫 독립 표본으로 유지하고 도심 배송을 다음 독립 표본 후보로 둔다. |
| `PLAN-PLANNING-DECISION-READING-001` | [결정 원장 기획 시작점 안내](결정원장-기획시작점-읽기안내.md) | `ReadyForReview` | 과거 D 이력에서 기획 시작점을 찾는 안내 |

## 메인 스토리

| 기획 ID | 현재 문서·판본 | 상태 | 상위·하위 관계 |
| --- | --- | --- | --- |
| `PLAN-STORY-MIRROR-MAIN-001` | [Mirror 메인 스토리 r83](메인스토리-거울의흐름-기획-2026-09-01.md) | `SourcePending / ReadyForReview / HexagramProductionHierarchyBound / RuntimePlayOrderDeferred` | 최상위 이야기. 64괘→육효 이야기를 제작 주계층으로 사용하고 기존 Act·Chapter는 호환 묶음으로 보존한다. 실제 플레이 순서와 흑막상인 원작 정사는 별도 승인 전 미확정 |
| `PLAN-STORY-HEXAGRAM-SEQUENCE-001` | [역경에서 영감을 얻는 스토리 기획 r16](Planning/스토리/PLAN-STORY-HEXAGRAM-SEQUENCE-001/README.md) | `ApprovedPlanningBaseline / InspirationReferenceOnly` | 이야기·사건이 주 탐색이며 괘·효는 선택적 영감·원문 대조 이력이다. 고정 캠페인 수·육효 진행·순차 제작 관문을 대체한다. |
| `PLAN-STORY-HEXAGRAM-CAMPAIGN-RESET-001` | [캠페인 실패·진입 복귀 r2](Planning/스토리/PLAN-STORY-HEXAGRAM-CAMPAIGN-RESET-001/README.md) | `ApprovedForHandoff / LogicE3Validation / PresentationDeferred` | 이야기 단계 수를 명시하고 기존 6단계 저장과 진입 복원·재시도·성과 보존을 호환한다. |
| `PLAN-STORY-IDEA-MAP-LEARNING-001` | [수뢰둔→산수몽 이데아 맵 학습 r2](Planning/스토리/PLAN-STORY-IDEA-MAP-LEARNING-001/README.md) | `ApprovedPlanningBaseline / MengContextLogicE3Implemented / PresentationDeferred` | 산수몽을 독립 괘상 맥락 카드로 제안·수락·보류·해제하고 실제 행위에만 NPC 학습과 별도 가산 보정을 적용한다. 지리는 효별 강제 순서가 아닌 횡단 학습 배경이다. |
| `PLAN-STORY-HEX03-LINE-001` | [수뢰둔 초구 효사 기획 r3](Planning/스토리/PLAN-STORY-HEX03-LINE-001/README.md) | `PrototypeReference / StoryApproved / RequirementsUnresolved / NotReadyForDevelopment` | 손님 체류 선택과 더불어 첫 유효 플레이어 행위가 장소와 무관하게 기초 이데아 맵을 연다. |
| `PLAN-STORY-HEX03-LINE-002` | [수뢰둔 육이 효사 기획 r2](Planning/스토리/PLAN-STORY-HEX03-LINE-002/README.md) | `PrototypeReference / StoryApproved / RequirementsUnresolved / NotReadyForDevelopment` | 한스와 농장 경계를 공동 순찰해 피해를 줄이고 행동→결과→관계의 첫 계보를 읽는다. |
| `PLAN-STORY-HEX03-LINE-003` | [수뢰둔 육삼 효사 기획 r1](Planning/스토리/PLAN-STORY-HEX03-LINE-003/README.md) | `PrototypeReference / StoryApproved / RequirementsUnresolved / NotReadyForDevelopment` | 피해 흔적의 관찰 사실과 원인 추정을 분리하고 안내 없는 깊은 숲 추격을 중단한다. |
| `PLAN-STORY-HEX03-LINE-004` | [수뢰둔 육사 효사 기획 r2](Planning/스토리/PLAN-STORY-HEX03-LINE-004/README.md) | `PrototypeReference / StoryApproved / RequirementsUnresolved / NotReadyForDevelopment` | 제한 보호권을 권한→책임→돌봄 대상으로 읽고 숲닭 무리를 임시 보호한다. |
| `PLAN-STORY-HEX03-LINE-005` | [수뢰둔 구오 효사 기획 r2](Planning/스토리/PLAN-STORY-HEX03-LINE-005/README.md) | `PrototypeReference / StoryApproved / RequirementsUnresolved / NotReadyForDevelopment` | 반복 돌봄과 명상으로 후보 관계를 부분 검증하되 작은 질서를 성급히 일반화하지 않는다. |
| `PLAN-STORY-HEX03-LINE-006` | [수뢰둔 상육 효사 기획 r2](Planning/스토리/PLAN-STORY-HEX03-LINE-006/README.md) | `PrototypeReference / StoryApproved / RequirementsUnresolved / NotReadyForDevelopment` | 부분 손실을 복기해 운영·방어 학습 필요를 분리하되 권위 WI 전에는 조회기가 이를 추측 생성하지 않는다. |
| `PLAN-STORY-HEX04-LINE-001..006` | [산수몽 초육](Planning/스토리/PLAN-STORY-HEX04-LINE-001/README.md)부터 [상구](Planning/스토리/PLAN-STORY-HEX04-LINE-006/README.md) | `Locked / PrototypeReference / ResonanceArchetype / NotReadyForDevelopment` | 규칙 수용, 멘토 교정, 무원리 모방, 막힌 반복, 안내된 재시도, 보호 적용을 순서 없는 공명 원형으로 사용한다. 한 활성화에서 전부 경험할 필요가 없다. |
| `PLAN-STORY-DUAL-PROTAGONIST-001` | [두 빙의 대상·연금술·가주 승계 r71](소가주-연금술-가주승계-메인스토리기획-2026-09-01.md) | `ApprovedPlanningBaseline / DistinctCombatScaleConfirmed / SingleControlledProtagonistConfirmed / CommonActiveDefenseFoundationConfirmed / ScopedYoungLordCommandAuthorityConfirmed / HierarchicalDefenseBlueprintConfirmed / LumberjackAxeTransferConfirmed / SourceCanonPending` | 두 시작은 같은 능동 방어 기반과 H2·H3 선행 청사진→H1 점진 가동 구조를 공유한다. 소가주는 담당 영지·부대 안의 지휘권을 처음부터 가지며, 모험가는 작은 허가 거점에서 관계·소유·공적으로 계획 범위를 넓힌다. |
| `PLAN-STORY-YODONG-DEFENSE-001` | [요동성 방어 r77](요동성방어-메인스토리기획-2026-09-01.md) | `ApprovedPlanningBaseline / SourceCanonPending` | 한스 농장 첫 행동과 정밀 작업 도끼 수리·미정 과거 단서, 역할 분리·감사 사유를 필요한 관계자에게만 공개하는 보급 기획 |
| `PLAN-STORY-HUB-DISCOVERY-001` | [허브 발견 r27](허브발견-3인칭관찰과광역노드-기획-2026-09-01.md) | `QuestionActive / PlayerNeedFirstEntryConfirmed / GraphBeforePlacementConfirmed / PlayerTravelEdgeSeparationAsked / FirstDemandDeferred / DevelopmentHandoffDeferred` | 실제 배치 전에 Farm↔Hub Graph Map을 구체화한다. 플레이어 왕복 경로와 기존 화물 운송 엣지의 분리 여부를 현재 문답하며 비료 수요와 미도착 화물은 후속으로 보존 |
| `PLAN-STORY-TOWN-DISCOVERY-001` | [Town 첫 발견 r5](Planning/스토리/PLAN-STORY-TOWN-DISCOVERY-001/README.md) | `PausedForHorizontalRotation / SeasonalMarketPricingConfirmed / AudioPreparationRequired / DevelopmentHandoffDeferred` | 비제철 상품은 재고 소진·저수요 때 할인되고 고수요·저공급·보존/운송 부담 때 비싸질 수 있으며 가격 사유를 조회 가능하게 함 |
| `PLAN-STORY-CITY-DISCOVERY-001` | [City 첫 발견 r5](Planning/스토리/PLAN-STORY-CITY-DISCOVERY-001/README.md) | `PausedForHorizontalRotation / OnDemandMainQuestRouteConfirmed / DevelopmentHandoffDeferred` | 주거권 생활 동선과 메인 안내 골격을 두고, 기본 화면은 방향·짧은 목표만 표시하며 상세 경로는 지도에서 요청할 때 알려진 길 범위로 제공 |

## 현재 플레이·사건 기획

| 기획 ID | 현재 문서 | 상태 | 현재 초점 |
| --- | --- | --- | --- |
| `PLAN-GAMEPLAY-FIRST-PERSON-FOCUS-001` | [1인칭 선택형 집중 타이밍](1인칭-선택형집중타이밍-기획보완-2026-08-31.md) | `AcceptedPlanningDirection` | 선택한 작업의 반복 집중과 추가 효과 |
| `PLAN-GAMEPLAY-MULTI-AREA-CHOICE-001` | [다영역 선택형 플레이 r2](다영역-선택형플레이와병행개발-2026-08-31.md) | `ConfirmedDirection / CurrentStructureReviewed` | Nature·Farm·Town·Hub·City를 강제 직선 해금 없이 선택적으로 발견하고, 독립 영역 개발과 세계 조화를 구분 |
| `PLAN-GAMEPLAY-PERSPECTIVE-ROLES-001` | [직접 탐험에서 넓은 운영으로 r3](탐험과운영-시점역할과현실업무연결-2026-08-31.md) | `AcceptedPlanningDirection` | 1인칭 직접 탐험과 전술 3인칭·광역 운영의 역할·WI 스케일 구분 |
| `PLAN-GAMEPLAY-PROGRESSION-CLUSTERS-001` | [문답 기반 발전 군집·테크트리 r13](문답기반-발전군집과테크트리-2026-08-31.md) | `AcceptedPlanningDirection / OnePrimaryLearningSlotConfirmed / InitialNpcLearningE3Bounded` | 한스 NPC 카드 두 장만 첫 E3 기준선으로 사용하고 관계 기반 취득·멘토 공유·직접 효율 보정은 후속으로 분리한다. |
| `PLAN-GAMEPLAY-MEDITATION-ACTION-001` | [플레이어 내면·명상 문답 r53](../Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md) | `Refining / OnePrimaryLearningSlotConfirmed / InitialNpcLearningE3Bounded` | 결속된 플레이어 ActionRecord에만 이해도 +1을 멱등 적용하고 취소·NPC 수행·무관한 WI를 제외한다. |
| `PLAN-TIME-SEASONAL-001` | [24절기·제철·복장·식생·생활 작업 r9](24절기-제철자료-조사와기획연결-2026-08-31.md) | `ApprovedPlanningBaseline / SeasonalWorkSurveyIntegrated / Conditional` | 대표 농사·운송·수리의 정적 후보와 전용 Clip·접촉·중단/귀환·말풍선/대화창 anchor 공백을 구분 |
| `PLAN-TIME-SOLAR-TERM-TAROT-TURN-001` | [절기 전략 턴·타로 카드 r14](절기전략턴-타로카드-기획-2026-09-02.md) | `ApprovedForHandoff / ThreeLearningCadencesPerTermConfirmed / TwoInternalStrategyBranchesConfirmed / DefenseBaselineOffenseOptionConfirmed / OnePrimaryLearningSlotConfirmed / InitialNpcLearningE3Bounded` | 초반·중반·후반 사이의 두 내부 분기에서 방어·회복·제한 공세를 재배정하고, 결과를 다음 방어 조건과 절기 마감에 합류한다. |
| `PLAN-STORY-FIRST-FARM-DISCOVERY-001` | [한스 농장 첫 벌목·울타리 수리 r27](한스농장-첫벌목과울타리수리-기획-2026-09-02.md) | `ApprovedPlanningBaseline / FirstFenceRestorationE5BoundaryFrozen / LogicE5Validated / PresentationE4Blocked / AnimationDeferredToE6` | 부러진 농장 도끼 줍기 → 개인 도끼로 나무 1그루 벌목 → 목재 2개 획득 → 손상 울타리 3구간 일괄 수리를 첫 폐루프로 동결했다. 논리 E5는 검증했고, 최소 World 표현 후보는 준비했으나 현재 Unity 컴파일 차단 때문에 표현 E5는 승격하지 않는다. 실제 벌목 애니메이션은 E6 책임이다. |

세부 문답 기획은 [PlanningSessions 목차](../Architecture/PlayableLoops/PlanningSessions/README.md)와 [문답 정리 상태판](../Architecture/PlayableLoops/PlanningSessions/문답정리상태판.md)에서 조회한다.

## 주제 문답 기획

| 기획 ID | 현재 문서·판본 | 상태 | 현재 초점 |
| --- | --- | --- | --- |
| `PLAN-PLACEMENT-CROSS-AREA-BUILDING-001` | [영역별 건물·공간·배치 r3](../Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) | `Open` | Q-077~Q-339의 영역별 H·시설·통행·배치 문답과 현행 배치 맵 계보 |
| `PLAN-GAMEPLAY-FARM-CROP-LIFE-001` | [수확 가능 직후의 여유 r1](../Architecture/PlayableLoops/PlanningSessions/건물공간배치/harvest-ready-grace-window.inquiry.r1.md) | `Asked` | 수확 가능 뒤 손실이 시작되기 전 여유 시간 |
| `PLAN-GAMEPLAY-COMMUNITY-VISITOR-001` | [공동체 편입·손님·원격 응대 r10](../Architecture/PlayableLoops/PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md) | `Synthesizing` | 편입·방문·체류·원격 응대와 첫 적용 Area |
| `PLAN-GAMEPLAY-SURVIVAL-ECONOMY-001` | [생존경제·생산·소비·비축 r10](../Architecture/PlayableLoops/PlanningSessions/생존경제/survival-economy.inquiry.r1.md) | `Synthesizing / SeasonalTownPricingConfirmed` | 생존 가능 일수·조달·비축·가격 전망과 비제철 재고의 할인·희소 프리미엄 및 현실 자료 경계 |
| `PLAN-GAMEPLAY-DELEGATION-001` | [Solo 업무 위임·예외 r3](../Architecture/PlayableLoops/PlanningSessions/솔로업무위임/solo-work-delegation.inquiry.r1.md) | `Synthesizing` | NPC 역량·권한·도구·경로와 실패·예외의 플레이어 반환 |
| `PLAN-GAMEPLAY-HERBAL-CRAFTING-001` | [약초·Recipe·조합 제작 r30](../Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md) | `Refining / SourceRecoveryGapQ272To274` | 약초 식별·채집·달이기·음용과 소실 질문 3개 |
| `PLAN-SYSTEM-SAVE-REENTRY-001` | [저장·Load·재진입 r1](../Architecture/PlayableLoops/PlanningSessions/저장재진입/save-load-runtime.inquiry.r1.md) | `Refining` | 저장·중단·Load·같은 판본 재진입 |
| `PLAN-GAMEPLAY-REGIONAL-MONSTER-001` | [지역 오행 몬스터·개척 r5](../Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) | `Refining / SuitableCreatureAssetPending` | 지역 속성·흔적·준비·동물형 첫 경계 마수 |
| `PLAN-GAMEPLAY-FIRST-EXPERIENCE-001` | [첫 플레이 체감·반복 r10](../Architecture/PlayableLoops/PlanningSessions/첫플레이체감/first-play-experience.inquiry.r1.md) | `ConfirmedDirection / RuntimeEvidenceSeparate` | 강제 시작 경로가 아닌 첫 발견·선택과 한스 농장 계보 |
| `PLAN-GAMEPLAY-NATURE-SHELTER-001` | [Nature 거점·수면·날씨·방어 r4](../Architecture/PlayableLoops/PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md) | `PausedForHorizontalRotation / CabinSleepStorageConfirmed` | 자연 쉼터 H1의 3단계 작은 오두막에서 조건부 안전 수면과 제한 보관을 함께 열되 별도 상태·행동으로 판정 |
| `PLAN-GAMEPLAY-NATURE-RESOURCE-CONSTRUCTION-001` | [Nature 자원·LandUse·건설 r6](../Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) | `Refining / NatureRestorationReopenQuestionAsked` | 재생·LandUse·청사진·재료 투입·단계 건설과 자연 복원·안전 회복 H2의 최소 재탐색 관문 |
| `PLAN-GAMEPLAY-TOWN-ORDER-001` | [Town 주문 수령·소비·귀환 r1](../Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) | `Refining` | 주문 확인·수령·소비·귀환의 독립 Town 폐루프 |
| `PLAN-GAMEPLAY-FARM-DEFENSE-001` | [Farm 병영·방위 r1](../Architecture/PlayableLoops/PlanningSessions/Farm병영방위/farm-barracks-defense.inquiry.r1.md) | `ReadyForSynthesis` | 농민 소집·전문병·분대·초소·귀환·치료 |
| `PLAN-GAMEPLAY-HUB-DEMAND-001` | [Hub 수요·분배·출고 준비 r1](../Architecture/PlayableLoops/PlanningSessions/Hub수요분배/hub-demand-allocation.inquiry.r1.md) | `Synthesizing / Q250Deferred` | 입지·수요·희소 재고·부족분·출고 준비 |
| `PLAN-GAMEPLAY-PLAYER-STAMINA-001` | [플레이어 행동 체력 회복·성장 r32](../Architecture/PlayableLoops/플레이어행동체력회복과성장.md) | `Draft / NumericProfilePending` | 행동 체력·휴식·포션·최대치 성장과 별도 회복 개념 |

## World·Graph Map 기획

| 기획 ID | 현재 문서 | 상태 | Graph Map 관계 |
| --- | --- | --- | --- |
| `PLAN-WORLD-FOUR-AREAS-001` | [월드맵 네 업무영역 제안](월드맵-4업무영역-자연경계와자산선정제안-2026-08-31.md) | `ReadyForReview` | Farm·Town·Hub·City의 자연 경계 |
| `PLAN-GRAPH-NORTHERN-LIFE-001` | [북부 생활권 첫 Graph Map](북부생활권-첫그래프맵-상세제안-2026-09-01.md) | `ApprovedForHandoff / StaleRevision` | 기존 r4 제안. 현행 Graph Map r6과 최신 스토리의 재결속 필요 |
| `PLAN-GRAPH-NORTHERN-LIFE-REVIEW-001` | [Graph Map 현행 검증·구체화](그래프맵-현행검증과구체화-기획-2026-09-01.md) | `ReadyForReview` | 현재 판본 결함과 첫 경계 순찰 확장 후보 |
| `PLAN-GRAPH-PLANNING-INTEGRATION-001` | [분리 기획 기반 Graph Map 통합 인계 r1](GraphMap-분리기획통합-인계-2026-09-01.md) | `ApprovedForHandoff` | 현행 기획 판본을 기존 Graph Map과 증분 통합하고 순환 결함을 먼저 복구 |
| `PLAN-GRAPH-LONG-ROUTE-ENCOUNTER-001` | [거점 간 장거리 경로·위험 조우·보급로 r3](거점간장거리경로-위험조우-기획-2026-09-02.md) | `ApprovedPlanningDirection / NumericThresholdPending` | 기준 공간 위 기상·운송·위협·물류·선택 레이어, 다중 비용·용량 엣지와 대체 보급로 |
| `PLAN-GRAPH-HUB-LOGISTICS-CIRCULATION-001` | [허브 물류 H1~H4 순환 경로 r4](허브물류-H1-H4-순환경로-기획-2026-09-02.md) | `ApprovedForHandoff / NumericAndWorldBindingPending` | H1~H4 입출고 순환, 도로 공사·파손·보수, 권한·업무 위임, NPC 사건 성장·성향 단서와 비권위 로컬 LLM 대사 |
| `PLAN-GRAPH-LAYER-FIRST-WORKFLOW-001` | [Graph Map 레이어 중심 설계·개발 우선순위 r11](GraphMap-레이어중심-설계개발우선순위-2026-09-02.md) | `ActivePlanningPriority / SubjectNodeInteractionEdgeConfirmed / RelationPossibilityBoundaryConfirmed / GraphGapQuestioningConfirmed / FirstDefenseTargetPathConfirmed / JointBreachDefenseConfirmed / CommonActiveDefenseFoundationConfirmed / YoungLordCommandAuthorityConfirmed / HierarchicalDefenseBlueprintConfirmed / PlayerOnlyGhostBlueprintConfirmed / ResourceFreeBlueprintConfirmed / ArchitectMediatedBlueprintUnlockConfirmed / PlacementEngineCandidateGenerationConfirmed / SavedBlueprintContinuityConfirmed` | 건축가 관계가 끝나도 저장된 청사진은 열람·표시·현재 조건에서 착공 요청할 수 있다. 새 자동 설계와 구조 변경은 잠기지만 실제 착공 사전 검사는 현재 World 상태로 항상 다시 수행해 오래된 설계의 무효 조건을 차단한다. |
| `PLAN-PLACEMENT-FOREST-EDGE-FARM-001` | [숲 경계 농장 H1·H2 배치 맵 r25](숲경계농장-H1-H2-배치맵-기획-2026-09-02.md) | `PausedForHorizontalRotation / BalancedDraft / ToolObservationBeforePermissionConfirmed / DefenseChokepointR2PendingVisualApproval / DevelopmentHandoffDeferred` | 4m 통과부와 좌우 2.5m 울타리 연장을 하나의 방어 병목 H1 r2 후보로 준비했다. 사용자 시각 승인 전에는 후보 채택·Graph 권위·Unity 배치·E5로 넘기지 않는다 |

## 자료·표현 인계 기획

| 기획 ID | 현재 문서 | 상태 | 경계 |
| --- | --- | --- | --- |
| `PLAN-DATA-GAMEOBJECT-ASSET-001` | [농수산 품목·시각 자산 대응](농수산품목-시각자산대응-기획과개발인계-2026-08-31.md) | `ApprovedDirection` | 게임 객체·레코드·시각 자산 관계 |
| `PLAN-DATA-REALITY-MYSQL-001` | [현실 자료 서버·MySQL 축적](현실자료-서버MySQL축적-기획과개발인계-2026-08-31.md) | `ApprovedDirection` | 자료 수집·검토·비공개 저장 경계 |
| `PLAN-PRESENTATION-SYNTY-SURVEY-001` | [최근 기획 Synty Prefab 조사 r2](최근기획-SyntyPrefab조사-개발인계-2026-09-01.md) | `InProgressByDevelopment` | 후보 조사 인계. 실제 자산 채택·E5가 아님 |
| `PLAN-PRESENTATION-E4-POOL-001` | [Presentation E4 후보 풀·상태 변화 표현 r2](Planning/표현/PLAN-PRESENTATION-E4-POOL-001/README.md) | `ApprovedPlanningBaseline / BroadE4CandidatePoolConfirmed / StateTransitionVisualGateConfirmed / E5SelectionPolicyPending` | WI의 시작·진행·결과·중단/회복을 자연스럽게 판독할 최소 표현을 E4에서 준비하고, Unity 조립으로 부족한 부분만 Blender 파생형으로 보완. 후보 등록은 E5 성취가 아님 |
| `PLAN-PRESENTATION-H1-SYNTY-STATE-001` | [H1 Synty 표현 배당·상태 변화 전수 조사 r44](Planning/표현/PLAN-PRESENTATION-H1-SYNTY-STATE-001/README.md) | `ImplementedE4SurveyBoundary / DrainCompositionPrepared / WoodcuttingRigPoseR3Prepared / E5Blocked` | 배수 입구→경로→방류구를 조감·절개 시안으로 연결했다. 벌목 r1은 초벌 이력으로 낮추고 같은 팩 실제 리그의 골반 회전·무릎 굽힘·보폭을 사용한 r3 백스윙·타격 자세를 만들었다. 최종 벌목 Clip·실제 도끼 결속·Unity 배치와 E5는 별도다 |

## 앞으로 갱신하는 법

- 기획 스레드 응답은 `[기획 · 분야 · PLAN-* · 판본]` 아래에 `지금·여기·나·너·이렇게`, `오행 관계`, `추천·이유·대가`를 차례로 두고 마지막에 `확정 / 미정 / 다음 질문 하나`를 표시한다.
- 기획 문답이 깊어지면 해당 기획 문서의 판본과 이 목차의 현재 판본·상태만 갱신한다.
- 새 기획과 이관 완료 기획은 `Planning/<분야>/<PLAN-ID>/README.md`를 정본으로 사용한다. 현재 45개 가운데 기존 경로 정본 43개는 개별 이관이 검증되기 전까지 현재 경로를 유지하고, Town·City 첫 발견 2개는 표준 경로 정본을 유지한다.
- 장면 하나를 정할 때마다 새 D를 만들지 않는다.
- 여러 기획이 함께 따라야 할 장기 원칙이 새로 생길 때만 `DECISIONS.md`에 요약 결정 추가를 검토한다.
- Graph Map 인계 전에는 기획 ID·판본·상태·SHA-256·영향·제외 범위를 동결한다.
- 개발 결과는 완결·차단 때만 해당 기획의 인계 상태에 반영한다.
