# Mirror 기획 목차

> 게임 기획의 현재 판본과 상·하위 관계를 찾는 시작점이다. 세부 본문은 각 기획 문서가 소유하며 이 목차는 내용을 복제하지 않는다. 운영 기준은 [기획 문서 독립 관리 체계](../Architecture/기획문서독립관리체계.md)를 따른다.

## 읽는 순서

1. 공통 기획 방식
2. 메인 스토리와 하위 이야기
3. 현재 사건·플레이 기획
4. World·Graph Map 기획
5. 자료·표현 인계 기획

## 공통 기획 방식

| 기획 ID | 현재 문서 | 상태 | 역할 |
| --- | --- | --- | --- |
| `PLAN-PLANNING-PLAYER-CONTEXT-001` | [시간·공간·플레이어·대상 WI 기획](시간공간플레이어대상-WI기획정리-2026-08-31.md) | `ApprovedPlanningBaseline` | 지금·여기·나·너·이렇게와 WI를 함께 읽는 기준 |
| `PLAN-PLANNING-WI-GWAE-001` | [WI 괘성 분류 체계](../Architecture/WI괘성분류체계.md) | `ReviewedMetadata` | 행위 주체의 행위괘·작용괘·대상괘·보조괘로 기존 WI를 탐색하는 비권위 메타데이터 |
| `PLAN-PLANNING-MIGRATION-001` | [전체 기획 네 관점 순환 이관](전체기획-네관점순환이관-2026-08-31.md) | `InProgress` | 기존 문답을 공통 관점으로 재정리 |
| `PLAN-PLANNING-DECISION-READING-001` | [결정 원장 기획 시작점 안내](결정원장-기획시작점-읽기안내.md) | `ReadyForReview` | 과거 D 이력에서 기획 시작점을 찾는 안내 |

## 메인 스토리

| 기획 ID | 현재 문서·판본 | 상태 | 상위·하위 관계 |
| --- | --- | --- | --- |
| `PLAN-STORY-MIRROR-MAIN-001` | [Mirror 메인 스토리 r76](메인스토리-거울의흐름-기획-2026-09-01.md) | `SourcePending / ReadyForReview` | 최상위 이야기. 아래 세 기획을 묶음 |
| `PLAN-STORY-DUAL-PROTAGONIST-001` | [두 빙의 대상·연금술·가주 승계 r65](소가주-연금술-가주승계-메인스토리기획-2026-09-01.md) | `ApprovedPlanningBaseline / SourceCanonPending` | 독립 계절 나무꾼 빙의, 개인 손도끼 벌목과 별도 정밀 작업 도끼·한스 수리, 명상 튜토리얼, 자유 구매와 플레이 성향별 조달 우선순위, 소가주·연금술·가주 승계 |
| `PLAN-STORY-YODONG-DEFENSE-001` | [요동성 방어 r76](요동성방어-메인스토리기획-2026-09-01.md) | `ApprovedPlanningBaseline / SourceCanonPending` | 한스 농장 첫 행동과 정밀 작업 도끼 수리·미정 과거 단서, 역할 분리·감사 사유를 필요한 관계자에게만 공개하는 보급 기획 |
| `PLAN-STORY-HUB-DISCOVERY-001` | [허브 발견 r21](허브발견-3인칭관찰과광역노드-기획-2026-09-01.md) | `ReadyForDevelopmentHandoff` | 허브 관찰·미도착 화물·회복·명상 |

## 현재 플레이·사건 기획

| 기획 ID | 현재 문서 | 상태 | 현재 초점 |
| --- | --- | --- | --- |
| `PLAN-GAMEPLAY-FIRST-PERSON-FOCUS-001` | [1인칭 선택형 집중 타이밍](1인칭-선택형집중타이밍-기획보완-2026-08-31.md) | `AcceptedPlanningDirection` | 선택한 작업의 반복 집중과 추가 효과 |
| `PLAN-GAMEPLAY-MEDITATION-ACTION-001` | [플레이어 내면·명상 문답 r28](../Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md) | `Refining` | NPC 역할 분리·복원과 필요 최소 공개 계보를 잇는 염체 그래프 |
| `PLAN-TIME-SEASONAL-001` | [24절기·제철 자료 연결](24절기-제철자료-조사와기획연결-2026-08-31.md) | `ApprovedPlanningBaseline` | 절기와 농수산물·경관·자료 근거 |
| `PLAN-STORY-FIRST-FARM-DISCOVERY-001` | [북부 춘분 굶주린 농장 발견](북부춘분-굶주린농장발견-기획보완-2026-08-31.md) | `SupersededInPartByMainStory` | 초기 발견형 골격. 현행 한스 흐름은 메인 스토리가 소유 |

세부 문답 기획은 [PlanningSessions 목차](../Architecture/PlayableLoops/PlanningSessions/README.md)와 [문답 정리 상태판](../Architecture/PlayableLoops/PlanningSessions/문답정리상태판.md)에서 조회한다.

## World·Graph Map 기획

| 기획 ID | 현재 문서 | 상태 | Graph Map 관계 |
| --- | --- | --- | --- |
| `PLAN-WORLD-FOUR-AREAS-001` | [월드맵 네 업무영역 제안](월드맵-4업무영역-자연경계와자산선정제안-2026-08-31.md) | `ReadyForReview` | Farm·Town·Hub·City의 자연 경계 |
| `PLAN-GRAPH-NORTHERN-LIFE-001` | [북부 생활권 첫 Graph Map](북부생활권-첫그래프맵-상세제안-2026-09-01.md) | `ApprovedForHandoff / StaleRevision` | 기존 r4 제안. 현행 Graph Map r6과 최신 스토리의 재결속 필요 |
| `PLAN-GRAPH-NORTHERN-LIFE-REVIEW-001` | [Graph Map 현행 검증·구체화](그래프맵-현행검증과구체화-기획-2026-09-01.md) | `ReadyForReview` | 현재 판본 결함과 첫 경계 순찰 확장 후보 |
| `PLAN-GRAPH-PLANNING-INTEGRATION-001` | [분리 기획 기반 Graph Map 통합 인계 r1](GraphMap-분리기획통합-인계-2026-09-01.md) | `ApprovedForHandoff` | 현행 기획 판본을 기존 Graph Map과 증분 통합하고 순환 결함을 먼저 복구 |
| `PLAN-GRAPH-LONG-ROUTE-ENCOUNTER-001` | [거점 간 장거리 경로·위험 조우·보급로 r3](거점간장거리경로-위험조우-기획-2026-09-02.md) | `ApprovedPlanningDirection / NumericThresholdPending` | 기준 공간 위 기상·운송·위협·물류·선택 레이어, 다중 비용·용량 엣지와 대체 보급로 |
| `PLAN-GRAPH-LAYER-FIRST-WORKFLOW-001` | [Graph Map 레이어 중심 설계·개발 우선순위 r1](GraphMap-레이어중심-설계개발우선순위-2026-09-02.md) | `ActivePlanningPriority` | 새 기획을 레이어·노드·엣지 영향으로 정밀화하고 닫힌 작은 부분 그래프만 개발 인계 |

## 자료·표현 인계 기획

| 기획 ID | 현재 문서 | 상태 | 경계 |
| --- | --- | --- | --- |
| `PLAN-DATA-GAMEOBJECT-ASSET-001` | [농수산 품목·시각 자산 대응](농수산품목-시각자산대응-기획과개발인계-2026-08-31.md) | `ApprovedDirection` | 게임 객체·레코드·시각 자산 관계 |
| `PLAN-DATA-REALITY-MYSQL-001` | [현실 자료 서버·MySQL 축적](현실자료-서버MySQL축적-기획과개발인계-2026-08-31.md) | `ApprovedDirection` | 자료 수집·검토·비공개 저장 경계 |
| `PLAN-PRESENTATION-SYNTY-SURVEY-001` | [최근 기획 Synty Prefab 조사 r2](최근기획-SyntyPrefab조사-개발인계-2026-09-01.md) | `InProgressByDevelopment` | 후보 조사 인계. 실제 자산 채택·E5가 아님 |

## 앞으로 갱신하는 법

- 기획 문답이 깊어지면 해당 기획 문서의 판본과 이 목차의 현재 판본·상태만 갱신한다.
- 장면 하나를 정할 때마다 새 D를 만들지 않는다.
- 여러 기획이 함께 따라야 할 장기 원칙이 새로 생길 때만 `DECISIONS.md`에 요약 결정 추가를 검토한다.
- Graph Map 인계 전에는 기획 ID·판본·상태·SHA-256·영향·제외 범위를 동결한다.
- 개발 결과는 완결·차단 때만 해당 기획의 인계 상태에 반영한다.
