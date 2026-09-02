# Graph Map 개발 인계 상태

> 이 문서는 `eng/world-seedbeds/graph-map-development-handoffs.json`에서 생성한다. 직접 수정하지 않는다.

- 원장 판본: mirror-graph-map-development-handoffs.r6
- 전체: 2 / 개발 준비: 1 / 개발 수용: 0 / 진행: 0 / 통합: 1 / 차단: 0
- ReadyForDevelopment는 개발 수용·자동 활성화·코드 변경을 뜻하지 않는다.
- 실제 구현 상태는 Goal·work item·작업 명세·코드·시험·EvidencePackage가 소유한다.

| 인계 | 상태 | Graph Map slice | Loop / WI | 후보 work item | 목표·상한 |
| --- | --- | --- | --- | --- | --- |
| graph-map-development-handoff:farm-production-work-yard:r1 | ReadyForDevelopment | graph-map-dev-slice:farm-production-work-yard.v1 | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | work:farm-crop-cycle:landscape-binding-guard | E4<br>PresentationE4Preparation |
| graph-map-development-handoff:first-logging-reflection:r1 | Integrated | graph-map-dev-slice:first-logging-reflection.v1 | playable-loop:nature-shelter-foundation.v1<br>WI-NATURE-06 | work:nature-shelter:first-logging-reflection-seed | E3<br>LogicE3Preparation |

## graph-map-development-handoff:farm-production-work-yard:r1

- Graph Map: [eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json](../../../eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json) / mirror-graph-map-plan.northern-life-hub-discovery.r10 / SHA-256 b2f1288125e4974371e808a8ae59b2cde4d5a9222fe3a8c43bcbd4e0686b8adc
- 선택 노드: gm-node:farm-production, gm-node:farm-work-yard
- 선택 엣지: gm-edge:farm-production-to-work-yard
- 선택 제약: gm-constraint:actual-reference-identity, gm-constraint:farm-flow-separation, gm-constraint:asset-candidate-not-assignment
- 배치 규칙: gm-placement-rule:farm:production-processing-separation
- 코드 결속: gm-code:actual-e5-network-pipeline, gm-code:landscape-runtime-realization, gm-code:farm-h-placement-plan, gm-code:wi-seedbed-editor-preview, gm-code:h-spatial-rule-editor
- 개발 후보: work:farm-crop-cycle:landscape-binding-guard / 현재 Active / 자동 활성화 아님
- 작업 명세: [eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json) / SHA-256 cef233d99c8fcf6a588999bf85b9aa359cbeef71c14d073830cad423c886f6e7
- 현재 결과: 아직 개발 수용 전이며 코드·시험·Unity 결과가 없다.
- 다음 행동: 개발 담당이 현재 Goal·작업 명세·후보 work item의 소유와 겹침을 다시 확인한 뒤 수용 또는 차단을 반환한다.

## graph-map-development-handoff:first-logging-reflection:r1

- Graph Map: [eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json](../../../eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json) / mirror-graph-map-plan.northern-life-hub-discovery.r10 / SHA-256 b2f1288125e4974371e808a8ae59b2cde4d5a9222fe3a8c43bcbd4e0686b8adc
- 선택 노드: gm-node:first-logging-reflection-preparation
- 선택 엣지: 없음
- 선택 제약: gm-constraint:first-logging-reflection-lineage
- 배치 규칙: 없음
- 코드 결속: 없음
- 개발 후보: work:nature-shelter:first-logging-reflection-seed / 현재 Integrated / 자동 활성화 아님
- 작업 명세: [eng/execution-ledgers/work-orders/nature-logging-focus-meditation.e7-work-order.json](../../../eng/execution-ledgers/work-orders/nature-logging-focus-meditation.e7-work-order.json) / SHA-256 3ef0d631cb0e1fee47e178ac664eccd9b3dd403fcb9dd0a7f4cd066ece1094e5
- 현재 결과: 첫 벌목 성찰 P0 계약·도메인·13개 집중 시험을 통합했으며 제품 휴식·진행 저장·UI는 연결하지 않았다.
- 다음 행동: 제품 한스 집 안전 휴식 공급자와 성찰 진행 Save 소비 경로를 별도 승인 판본에서 연결한다.
