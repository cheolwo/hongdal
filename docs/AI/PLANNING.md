# Mirror 기획 목차

> 게임 기획의 현재 판본과 상·하위 관계를 찾는 시작점이다. 세부 본문은 각 기획 문서가 소유하며 이 목차는 내용을 복제하지 않는다. 운영 기준은 [기획 문서 독립 관리 체계](../Architecture/기획문서독립관리체계.md), 단계적 경로 통합은 [현행 기획 정본 경로](Planning/README.md)를 따른다.

## 읽는 순서

1. 공통 기획 방식
2. 메인 스토리와 하위 이야기
3. 현재 사건·플레이 기획
4. World·Graph Map 기획
5. 자료·표현 인계 기획

## 현재 문답 우선순위

- 한스 농장의 직전 `P0`는 **첫 장면에서 판독·접근 가능한 손상 울타리 전체를 한 번에 수리**하는 것으로 확정했다.
- Farm 수확 Lot은 물류 입구 목으로 이어지고, 입고·정돈 화→적재 완료 토→주문 배치·재고 할당 금(`토생금`)→포장·출고 인계·운송 수(`금생수`)로 순환한다. 도착 화물은 목적지·주문·수량·봉인/파손 상태의 인수 관문을 통과한 뒤에만 `수생목`으로 새 입고 작업을 연다. 상품별 인수 기준은 후속 Profile로 미뤘다.
- H2 오행 순환의 다음 질문은 Nature `자연 복원·안전 회복 블록`에서 재탐색을 열 수 있는 최소 복원선을 정하는 것이다.
- Hub 첫 이용은 플레이어가 이미 인지한 자기 필요를 해결하려고 주도적으로 찾아가는 것으로 확정했다. 실제 공간 배치 전에 Farm↔Hub 관계를 Graph Map에서 먼저 구체화하며, 현재 `P0` 질문은 플레이어 왕복 경로를 기존 화물 운송 엣지와 분리할지다. 첫 대표 비료 수요는 경로 형태 뒤로 미뤘고 미도착 화물은 선택적 후속 사건으로 내렸다.
- 다음 수평 순환은 Town 첫 공방 제작 실패 → 요동성 첫 위협 전달 → City 첫 공공 문제 순서의 후보로 둔다. 앞 답변이 주변 기획을 바꾸면 순서를 다시 계산한다.
- 수치·임계값·정확 UI·Prefab·Clip·Collider·코드·Unity 검증과 원작·소실 원문 복구는 사용자 문답이 아니라 `P3`, `Hold`, `NotAQuestion`으로 분리한다.
- 전수 현행화에는 분야 우선순위를 두지 않는다. 전체 기준과 후속 큐는 [기존 기획 현행화·문답 우선순위 r9](전체기획-네관점순환이관-2026-08-31.md#11-전수-목록과-문답-큐의-역할을-분리한다)가 소유한다.

## 공통 기획 방식

| 기획 ID | 현재 문서 | 상태 | 역할 |
| --- | --- | --- | --- |
| `PLAN-GAME-COMMON-PURPOSE-001` | [나·상대 오행 순환과 광복기 상위 목적 r4](게임상위목적-오행순환과광복기-기획-2026-09-02.md) | `ApprovedPlanningBaseline / PurposeGatePropagatedToPlanningGraphDevelopmentWorkflow / ExactProfilesPending` | 나의 오행 회복으로 확보한 역량·지식·물자·관계를 상대의 실제 필요에 적용하고, 상대의 회복이 새 선택·안전·관계로 환류하는 사슬을 기획 판본·Graph Map 레벨 1·개발 slice의 공통 목적 관문으로 사용 |
| `PLAN-PLANNING-PLAYER-CONTEXT-001` | [시간·공간·플레이어·대상 WI 기획 r3](시간공간플레이어대상-WI기획정리-2026-08-31.md) | `ApprovedPlanningBaseline / ContextFitActionDefined` | 지금·여기·나·너 속에서 현재 목적에 가장 적중하는 `이렇게`를 대표 추천하되 복수의 유효 행동과 실제 결과 판정을 보존하는 문답 기준 |
| `PLAN-PLANNING-WI-GWAE-001` | [WI 괘성 분류 체계 r4](../Architecture/WI괘성분류체계.md) | `ReviewedMetadata / H2FiveElementSurveyEstablished / SequentialQuestionReviewActive` | 현행 H2 38개를 우선 문답 15·부분 관계 검토 14·구조 우선 9로 나누고, Farm 물류의 도착·인수 `수생목`을 닫은 뒤 Nature→Hub→Town 순환에서 질문 하나씩 오행 관계를 확정해 E1~E4 인계 문맥을 보완 |
| `PLAN-PLANNING-MIGRATION-001` | [기존 기획 현행화·문답 우선순위 r9](전체기획-네관점순환이관-2026-08-31.md) · [전수 목록 r7](기존기획-현행화-전수목록-2026-09-02.md) · [정본 경로](Planning/README.md) | `InProgress / FullInventoryBounded / CanonicalPathTransitionDefined / QuestionPrioritySeparated` | 현행 기획 45개의 계보와 경로를 우선순위 없이 전수 현행화하고, 실제 사용자 문답은 별도 우선순위 큐에서 한 질문씩 수평 순환 |
| `PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001` | [운영 서버 0.0~3.5에서 Mirror Unity로의 이관 r1](Planning/시스템/PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001/README.md) | `ApprovedForHandoff / InventoryImplemented / HubFirstSliceApproved / PresentationE5Blocked` | 페이지 기능·EF Core·MongoDB·기존 Unity 경로를 전수 대장으로 만들고 `PlayableAction / ReadOnlyContext / AmbientSimulation / ServerOnly`로 선별한다. H1/H2는 후보 대응만 하며 Hub 입고·검수·적치를 첫 독립 표본으로 결속하되 실제 World 배치 전 E5를 선언하지 않는다. |
| `PLAN-PLANNING-DECISION-READING-001` | [결정 원장 기획 시작점 안내](결정원장-기획시작점-읽기안내.md) | `ReadyForReview` | 과거 D 이력에서 기획 시작점을 찾는 안내 |

## 메인 스토리

| 기획 ID | 현재 문서·판본 | 상태 | 상위·하위 관계 |
| --- | --- | --- | --- |
| `PLAN-STORY-MIRROR-MAIN-001` | [Mirror 메인 스토리 r82](메인스토리-거울의흐름-기획-2026-09-01.md) | `SourcePending / ReadyForReview / PlayOrderDisplayDefined` | 최상위 이야기. 메인 위치·필수 선행·권장 순서·선택/병행·조건부 귀환을 함께 보여 주되 정확 정사 장 순서는 원작 결속 전 미확정 |
| `PLAN-STORY-DUAL-PROTAGONIST-001` | [두 빙의 대상·연금술·가주 승계 r66](소가주-연금술-가주승계-메인스토리기획-2026-09-01.md) | `ApprovedPlanningBaseline / SourceCanonPending` | 독립 계절 나무꾼 빙의, 자발적 벌목 뒤 남은 목재를 활용하는 선택형 울타리 수리, 별도 정밀 작업 도끼·한스 수리, 명상 튜토리얼, 자유 구매와 플레이 성향별 조달 우선순위, 소가주·연금술·가주 승계 |
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
| `PLAN-GAMEPLAY-PROGRESSION-CLUSTERS-001` | [문답 기반 발전 군집·테크트리 r1](문답기반-발전군집과테크트리-2026-08-31.md) | `AcceptedPlanningDirection` | 확정 문답의 발전 수단을 강제 선행이 아닌 군집·선택·발견 계보로 조회 |
| `PLAN-GAMEPLAY-MEDITATION-ACTION-001` | [플레이어 내면·명상 문답 r38](../Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md) | `Refining / SharedRecoveryThreatModelConfirmed / DirectIndirectRecoveryPathConfirmed / RecoveryClarityConfirmed` | 특정 NPC에 대한 실제 도움은 빠르고 크게, 같은 시설·파티·공동체의 다른 NPC에게는 바뀐 안전·물자·관계·업무 경로를 통해 느리고 작게 영향을 주며 중복 지급을 막음 |
| `PLAN-TIME-SEASONAL-001` | [24절기·제철·복장·식생·생활 작업 r9](24절기-제철자료-조사와기획연결-2026-08-31.md) | `ApprovedPlanningBaseline / SeasonalWorkSurveyIntegrated / Conditional` | 대표 농사·운송·수리의 정적 후보와 전용 Clip·접촉·중단/귀환·말풍선/대화창 anchor 공백을 구분 |
| `PLAN-TIME-SOLAR-TERM-TAROT-TURN-001` | [절기 전략 턴·타로 카드 r2](절기전략턴-타로카드-기획-2026-09-02.md) | `ApprovedForHandoff / TimingAndBalancePending` | 실시간·일 WorldTick·절기 마감 3층, 카드 3장 제안·1장 선택·버림 더미, 기존 RecoveryShare51 방향 동결 |
| `PLAN-STORY-FIRST-FARM-DISCOVERY-001` | [한스 농장 첫 벌목·울타리 수리 r20](한스농장-첫벌목과울타리수리-기획-2026-09-02.md) | `ApprovedPlanningBaseline / E5FocusedQuestionMode / FirstTillingSliceRecommended / ExactTillingTargetAsked` | 새 기능 확장을 잠시 멈추고 기존 WI-FARM-01 한 구획 밭갈기의 정확 대상·전후 상태·기준점·표현 후보를 차례로 동결해 개발의 실제 Presentation E5 인계까지 닫음 |

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
| `PLAN-GRAPH-LAYER-FIRST-WORKFLOW-001` | [Graph Map 레이어 중심 설계·개발 우선순위 r1](GraphMap-레이어중심-설계개발우선순위-2026-09-02.md) | `ActivePlanningPriority` | 새 기획을 레이어·노드·엣지 영향으로 정밀화하고 닫힌 작은 부분 그래프만 개발 인계 |
| `PLAN-PLACEMENT-FOREST-EDGE-FARM-001` | [숲 경계 농장 H1·H2 배치 맵 r24](숲경계농장-H1-H2-배치맵-기획-2026-09-02.md) | `PausedForHorizontalRotation / BalancedDraft / ToolObservationBeforePermissionConfirmed / DevelopmentHandoffDeferred` | 운영 단서와 허락 전 관찰을 확정한 뒤 수평 문답으로 전환한다. 대표 농기구 종류·허락 뒤 사용 범위는 재개 질문으로 보존하고 개발 인계는 후속 묶음까지 미룬다 |

## 자료·표현 인계 기획

| 기획 ID | 현재 문서 | 상태 | 경계 |
| --- | --- | --- | --- |
| `PLAN-DATA-GAMEOBJECT-ASSET-001` | [농수산 품목·시각 자산 대응](농수산품목-시각자산대응-기획과개발인계-2026-08-31.md) | `ApprovedDirection` | 게임 객체·레코드·시각 자산 관계 |
| `PLAN-DATA-REALITY-MYSQL-001` | [현실 자료 서버·MySQL 축적](현실자료-서버MySQL축적-기획과개발인계-2026-08-31.md) | `ApprovedDirection` | 자료 수집·검토·비공개 저장 경계 |
| `PLAN-PRESENTATION-SYNTY-SURVEY-001` | [최근 기획 Synty Prefab 조사 r2](최근기획-SyntyPrefab조사-개발인계-2026-09-01.md) | `InProgressByDevelopment` | 후보 조사 인계. 실제 자산 채택·E5가 아님 |
| `PLAN-PRESENTATION-E4-POOL-001` | [Presentation E4 후보 풀 r1](Planning/표현/PLAN-PRESENTATION-E4-POOL-001/README.md) | `ApprovedPlanningBaseline / BroadE4CandidatePoolConfirmed / E5SelectionPolicyPending` | 여러 영역의 표현 후보를 E4까지 넓게 준비하고 실제 결속 조건이 갖춰진 후보를 E5 실행 묶음으로 선택. 후보 등록은 E5 성취가 아님 |

## 앞으로 갱신하는 법

- 기획 스레드 응답은 `[기획 · 분야 · PLAN-* · 판본]` 아래에 `지금·여기·나·너·이렇게`, `오행 관계`, `추천·이유·대가`를 차례로 두고 마지막에 `확정 / 미정 / 다음 질문 하나`를 표시한다.
- 기획 문답이 깊어지면 해당 기획 문서의 판본과 이 목차의 현재 판본·상태만 갱신한다.
- 새 기획과 이관 완료 기획은 `Planning/<분야>/<PLAN-ID>/README.md`를 정본으로 사용한다. 현재 45개 가운데 기존 경로 정본 43개는 개별 이관이 검증되기 전까지 현재 경로를 유지하고, Town·City 첫 발견 2개는 표준 경로 정본을 유지한다.
- 장면 하나를 정할 때마다 새 D를 만들지 않는다.
- 여러 기획이 함께 따라야 할 장기 원칙이 새로 생길 때만 `DECISIONS.md`에 요약 결정 추가를 검토한다.
- Graph Map 인계 전에는 기획 ID·판본·상태·SHA-256·영향·제외 범위를 동결한다.
- 개발 결과는 완결·차단 때만 해당 기획의 인계 상태에 반영한다.
