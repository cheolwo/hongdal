# Graph Map 기획 인계 상태

> 이 문서는 `eng/world-seedbeds/graph-map-planning-handoffs.json`에서 생성한다. 직접 수정하지 않는다.

- 원장 판본: mirror-graph-map-planning-handoffs.r7
- 전체: 7 / 반영: 1 / 차단: 0 / 영향 없음: 0
- 기획은 승인 판본과 인계만 소유하고, Graph Map 작업은 레벨 1·2·3 반영과 최종 반환을 소유한다.
- 이 상태는 Unity Scene·Prefab·실제 입력·Game View·E 승격을 뜻하지 않는다.

| 인계 | 상태 | 영향 | 기획 판본 | 대상 Graph Map | 요청 레벨 | 기획 질문 |
| --- | --- | --- | --- | --- | --- | --- |
| graph-map-handoff:northern-life-hub-discovery:r4 | Superseded | UpdateExisting | northern-life-hub-discovery-graph-map-proposal.r4 | graph-map:mirror:northern-life-hub-discovery.v1 | Federation, Level1, Level2, Level3 | 없음 |
| graph-map-handoff:northern-life-hub-discovery:r5 | Superseded | UpdateExisting | main-story-mirror-flow.r58 | graph-map:mirror:northern-life-hub-discovery.v1 | Federation, Level1, Level2, Level3 | 필요 |
| graph-map-handoff:northern-life-hub-discovery:r6 | Superseded | UpdateExisting | main-story-mirror-flow.r58 | graph-map:mirror:northern-life-hub-discovery.v1 | Federation, Level1, Level2, Level3 | 필요 |
| graph-map-handoff:northern-life-hub-discovery:r7 | Superseded | UpdateExisting | graph-map-planning-integration.r1 | graph-map:mirror:northern-life-hub-discovery.v1 | Federation, Level1, Level2, Level3 | 없음 |
| graph-map-handoff:northern-life-hub-discovery:r8 | Superseded | UpdateExisting | graph-map-normalization-hans.r1 | graph-map:mirror:northern-life-hub-discovery.v1 | Level1, Level2, Level3 | 없음 |
| graph-map-handoff:northern-life-hub-discovery:r9 | Superseded | UpdateExisting | dual-possession-alchemist-succession.r63 | graph-map:mirror:northern-life-hub-discovery.v1 | Level1, Level2, Level3 | 없음 |
| graph-map-handoff:northern-life-hub-discovery:r10 | Integrated | UpdateExisting | planning-index.current-2026-09-02 | graph-map:mirror:northern-life-hub-discovery.v1<br>mirror-graph-map-plan.northern-life-hub-discovery.r10 | Federation, Level1, Level2, Level3 | 없음 |

## graph-map-handoff:northern-life-hub-discovery:r4

- 기획: [docs/AI/북부생활권-첫그래프맵-상세제안-2026-09-01.md](../../../docs/AI/북부생활권-첫그래프맵-상세제안-2026-09-01.md) / northern-life-hub-discovery-graph-map-proposal.r4 / SHA-256 8ac5dec5109c6bd469c7460f9261b833859c355e0d696aacc61f0eadcb7e24d4
- 상태·영향: Superseded / UpdateExisting
- 반영: federation:graph-map:mirror:northern-life-hub-discovery.v1, level1:nodes:11, level1:edges:10, level2:constraints:9, level2:placement-rule-profiles:5, level2:placement-rules:21, level2:placement-rule-bound-constraints:3, level3:bindings:6
- 미반영: gm-node:yodong-defense-gateway actual AreaSet/Graph/WI
- 차단: 없음
- 검증: GraphMapCheckPassed, GraphMapRegression144Passed, ScopedFastPassed
- 기획 반환: r4 결과는 역사로 보존하며 D-529·D-530을 반영한 r5 인계가 현재 Graph Map 판본을 대체한다.
- 다음 기획 초점: 현행 결과는 r5 인계에서 읽는다.

## graph-map-handoff:northern-life-hub-discovery:r5

- 기획: [docs/AI/메인스토리-거울의흐름-기획-2026-09-01.md](../../../docs/AI/메인스토리-거울의흐름-기획-2026-09-01.md) / main-story-mirror-flow.r58 / SHA-256 058e2e3d77c6179d6a4a0b75e8dd53b679a8ce7d0cfd372fa480e0108b02c13e
- 상태·영향: Superseded / UpdateExisting
- 반영: level1:gm-node:hans-permitted-tree, level1:gm-node:hans-broken-fence, level1:gm-node:hans-first-trust, level1:gm-edge:farm-edge-to-hans-permitted-tree, level1:gm-edge:hans-timber-to-fence-repair, level1:gm-edge:hans-fence-repair-to-first-trust, level2:gm-constraint:hans-first-trust-causal-lineage, level2:gm-constraint:hans-first-trust-spatial-values-unresolved, overlay:gm-overlay:decision-card-authority-time, level3:unbound:6
- 미반영: D-529 exact tree/distance/fence/tool/relation stage; D-529 Hans witness/trust approved WI and authority contract; D-530 slowdown ratio/transition/UI/accessibility/Hosted authority contract
- 차단: Planning: D-529 exact spatial values and relationship stage; PlanningAndDevelopment: D-530 slowdown and Hosted policy
- 검증: GraphMapCheckPassed, GraphMapRegression144Passed, NoSceneOrEvidencePromotion
- 기획 반환: r5 결과와 당시 r51 hash는 역사로 보존하며 D-531~D-537과 현행 r58을 반영한 r6 인계가 현재 판본을 대체한다.
- 다음 기획 초점: 현행 결과는 r6 인계에서 읽는다.

## graph-map-handoff:northern-life-hub-discovery:r6

- 기획: [docs/AI/메인스토리-거울의흐름-기획-2026-09-01.md](../../../docs/AI/메인스토리-거울의흐름-기획-2026-09-01.md) / main-story-mirror-flow.r58 / SHA-256 058e2e3d77c6179d6a4a0b75e8dd53b679a8ce7d0cfd372fa480e0108b02c13e
- 상태·영향: Superseded / UpdateExisting
- 반영: level1:reused:gm-node:hans-permitted-tree, level1:reused:gm-node:hans-broken-fence, level1:reused:gm-node:hans-first-trust, level1:new:gm-node:hans-house-timber-stockpile, level1:new:gm-node:hans-house-repair, level1:new:gm-node:hans-mission-life-base, level1:new-edges:3, level2:new-constraints:4, overlay:gm-overlay:decision-card-authority-time, level3:new-unbound:6
- 미반영: D-531/D-532 exact axe ownership, tree distance/count, stockpile position and inventory contract; D-533 exact Hans schedule and discovery contract; D-535/D-536 exact Farmhouse, repair-state assets, Blender environment, collider and bounds; D-537 exact rest, recovery, storage, companion waiting, danger closure and mission contract
- 차단: PlanningAndDevelopment: exact tool, stockpile, schedule, repair-state and life-base contracts; PresentationE4: D-536 candidate and Blender tool unavailable
- 검증: GraphMapCheckPassed, GraphMapRegressionPassed, PlanningHandoffRegressionPassed, DevelopmentHandoffRegressionPassed, NoSceneOrEvidencePromotion
- 기획 반환: r6 결과는 역사로 보존하며 분리 기획 ID를 입력으로 통합한 r7 인계가 현행 Graph Map 판본을 대체한다.
- 다음 기획 초점: 현행 결과는 r7 인계에서 읽는다.

## graph-map-handoff:northern-life-hub-discovery:r7

- 기획: [docs/AI/GraphMap-분리기획통합-인계-2026-09-01.md](../../../docs/AI/GraphMap-분리기획통합-인계-2026-09-01.md) / graph-map-planning-integration.r1 / SHA-256 00aa5495d72f2c9d49ea35cb05457fc863d82c51677f94f87fa6421c6090e86c
- 상태·영향: Superseded / UpdateExisting
- 반영: level1:nodes:30, level1:edges:31, level2:constraints:21, level2:placement-rules:21, federation:subgraphs:5, federation:ports:10, federation:connectors:5, overlays:4, level3:bindings:6, level3:unbound-nodes:20, level3:unbound-edges:22
- 미반영: 새 13개 기획 노드와 15개 엣지의 승인 Unity 계약·Prefab·Scene·Runtime 결속; 마수·한스 검·공방·화물 사건의 정확 공간·자산·수량·시간
- 차단: 없음
- 검증: GraphMapCheckPassed, GraphMapRegressionPassed, PlanningHandoffRegressionPassed, DevelopmentHandoffRegressionPassed, NoUnityOrEvidencePromotion
- 기획 반환: r7 분리 기획 통합 결과는 역사 판본으로 보존하며 현재 Graph Map은 한스 정규화 표본을 추가한 r8이 대체한다.
- 다음 기획 초점: 새 기획은 PLAN ID·revision·hash와 Graph Map 영향만 후속 인계하고 실제 Unity 결속은 작은 개발 slice로 별도 승인한다.

## graph-map-handoff:northern-life-hub-discovery:r8

- 기획: [docs/Architecture/GraphMap기획인계순환체계.md](../../../docs/Architecture/GraphMap기획인계순환체계.md) / graph-map-normalization-hans.r1 / SHA-256 5f6dae8935ba8f48530ab6e472bcd9d3375548ade1598082e9d8b4cd02806c51
- 상태·영향: Superseded / UpdateExisting
- 반영: level1:legacy-nodes:30-preserved, level1:legacy-edges:31-preserved, normalization:elements:9, normalization:relations:7, normalization:compatibility-aliases:2, normalization:blocked:1, level2:constraint-lineage-preserved, level3:binding-lineage-preserved
- 미반영: 한스 목격·발견·수리·허락의 승인 Runtime/WI 계약; 정확 허용 나무·울타리·집·적재 지점과 실제 공간 수치; 집 수리 상태의 실제 Prefab·Scene 결속; 한스 검 단서의 정확 대상·위치·관찰 계약
- 차단: 없음
- 검증: GraphMapCheckPassed, GraphMapRegressionPassed, PlanningHandoffRegressionPassed, DevelopmentHandoffRegressionPassed, NoUnityOrEvidencePromotion
- 기획 반환: 한스 Actor와 집을 안정 대상으로 분리하고 7개 관계 계약을 추출했으며 기존 사건형 ID와 일곱 칸은 호환 조회로 보존했다.
- 다음 기획 초점: Runtime/WI·정확 공간·Prefab 결속이 승인된 작은 slice만 별도 개발 인계하고 Blocked 검 단서는 근거가 생길 때 다시 검토한다.

## graph-map-handoff:northern-life-hub-discovery:r9

- 기획: [docs/AI/소가주-연금술-가주승계-메인스토리기획-2026-09-01.md](../../../docs/AI/소가주-연금술-가주승계-메인스토리기획-2026-09-01.md) / dual-possession-alchemist-succession.r63 / SHA-256 1b0bda2d9859372cf6d4e4f42c358be18f7cd5512b9f8c61515c7f49311ffa7d
- 상태·영향: Superseded / UpdateExisting
- 반영: level1:gm-node:first-logging-reflection-preparation, level2:gm-constraint:first-logging-reflection-lineage, level3:Simulation첫벌목성찰SeedEngine, test:SimulationFocusMeditationTests
- 미반영: 실제 제품 한스 집 안전 휴식 공급자; 성찰 진행 상태의 제품 Save 소비 경로; P1 명상 UI·염체 그래프; Unity Scene·실제 입력·Game View
- 차단: 없음
- 검증: GraphMapCheckPassed, FirstLoggingReflectionTestsPassed13Of13, NoNewGrowthAmount, NoUnityOrEvidencePromotion
- 기획 반환: 첫 벌목 ActionRecord와 한스 집 안전 휴식을 결속하는 P0 통합 결과는 보존하며, 현행 21기획·레이어 통합 r10이 현재 Graph Map 판본을 대체한다.
- 다음 기획 초점: 현행 Graph Map 결과는 r10 인계에서 읽고 P0 제품 연결 공백은 그대로 유지한다.

## graph-map-handoff:northern-life-hub-discovery:r10

- 기획: [docs/AI/PLANNING.md](../../../docs/AI/PLANNING.md) / planning-index.current-2026-09-02 / SHA-256 d9af1bbf5c21d6dca531da4b724153d2c50de78b3cb86eaa0ae3c577faeb7836
- 상태·영향: Integrated / UpdateExisting
- 반영: planning-assessments:21, level1:nodes:34, level1:edges:34, level2:constraints:28, federation:subgraphs:6, layers:6, overlays:9, overlay-edge-effects:4
- 미반영: 장거리 경로의 실제 좌표·분절·병렬 경로·비용·용량 fixture; 한스 정밀 손도끼의 실제 아이템·수리·NPC·표현 계약; 4업무영역 상세 경계·좌표·Prefab 승인; Unity Scene·실제 입력·Game View
- 차단: 없음
- 검증: GraphMapCheckPassed, GraphMapRegression185Passed, PlanningImpactCoverage21Passed, NoUnityOrEvidencePromotion
- 기획 반환: 현행 21기획을 분류하고 기존 Graph Map r10에 한스 정밀 손도끼 하위 그래프와 6레이어 경로 계약을 통합했다.
- 다음 기획 초점: 장거리 경로의 정확 fixture와 한스 정밀 손도끼 실제 계약은 각각 승인된 후속 판본에서 가장 작은 단위로 다시 연다.
- 결과 Graph Map: [eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json](../../../eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json) / mirror-graph-map-plan.northern-life-hub-discovery.r10 / SHA-256 b2f1288125e4974371e808a8ae59b2cde4d5a9222fe3a8c43bcbd4e0686b8adc
- 결과 보고: [docs/Reports/GraphMap-현행기획-레이어통합-2026-09-02.md](../../../docs/Reports/GraphMap-현행기획-레이어통합-2026-09-02.md)
