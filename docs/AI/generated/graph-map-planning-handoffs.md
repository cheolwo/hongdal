# Graph Map 기획 인계 상태

> 이 문서는 `eng/world-seedbeds/graph-map-planning-handoffs.json`에서 생성한다. 직접 수정하지 않는다.

- 원장 판본: mirror-graph-map-planning-handoffs.r1
- 전체: 1 / 반영: 1 / 차단: 0 / 영향 없음: 0
- 기획은 승인 판본과 인계만 소유하고, Graph Map 작업은 레벨 1·2·3 반영과 최종 반환을 소유한다.
- 이 상태는 Unity Scene·Prefab·실제 입력·Game View·E 승격을 뜻하지 않는다.

| 인계 | 상태 | 영향 | 기획 판본 | 대상 Graph Map | 요청 레벨 | 기획 질문 |
| --- | --- | --- | --- | --- | --- | --- |
| graph-map-handoff:northern-life-hub-discovery:r3 | Integrated | UpdateExisting | northern-life-hub-discovery-graph-map-proposal.r3 | graph-map:mirror:northern-life-hub-discovery.v1<br>mirror-graph-map-plan.northern-life-hub-discovery.r3 | Federation, Level1, Level2, Level3 | 없음 |

## graph-map-handoff:northern-life-hub-discovery:r3

- 기획: [docs/AI/북부생활권-첫그래프맵-상세제안-2026-09-01.md](../../../docs/AI/북부생활권-첫그래프맵-상세제안-2026-09-01.md) / northern-life-hub-discovery-graph-map-proposal.r3 / SHA-256 1d28414b3fae2dc090ebd4c2a8b19af3148a189294d25e90783eea582f56ad8d
- 상태·영향: Integrated / UpdateExisting
- 반영: federation:graph-map:mirror:northern-life-hub-discovery.v1, level1:nodes:11, level1:edges:10, level2:constraints:9, level3:bindings:6
- 미반영: gm-node:yodong-defense-gateway actual AreaSet/Graph/WI
- 차단: 없음
- 검증: GraphMapCheckPassed, GraphMapRegression130Passed, ScopedFastPassed
- 기획 반환: 첫 Graph Map의 레벨 1·2·3과 federation을 구조화했으며 요동성은 미해결 외부 관문으로 보존했다.
- 다음 기획 초점: 새 기획이 공간 관계를 바꾸면 기존 안정 ID와 대조해 별도 인계 판본을 등록한다.
- 결과 Graph Map: [eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json](../../../eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json) / mirror-graph-map-plan.northern-life-hub-discovery.r3 / SHA-256 665b1b34ca6545d1b34a69af5fec8c6470da01eae836b4c0f65de053e0eb2e0b
- 결과 보고: [docs/Reports/그래프맵-프로젝트재검토와첫구현-2026-09-01.md](../../../docs/Reports/그래프맵-프로젝트재검토와첫구현-2026-09-01.md)
