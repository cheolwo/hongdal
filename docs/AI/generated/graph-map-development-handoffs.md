# Graph Map 개발 인계 상태

> 이 문서는 `eng/world-seedbeds/graph-map-development-handoffs.json`에서 생성한다. 직접 수정하지 않는다.

- 원장 판본: mirror-graph-map-development-handoffs.r1
- 전체: 1 / 개발 준비: 1 / 개발 수용: 0 / 진행: 0 / 통합: 0 / 차단: 0
- ReadyForDevelopment는 개발 수용·자동 활성화·코드 변경을 뜻하지 않는다.
- 실제 구현 상태는 Goal·work item·작업 명세·코드·시험·EvidencePackage가 소유한다.

| 인계 | 상태 | Graph Map slice | Loop / WI | 후보 work item | 목표·상한 |
| --- | --- | --- | --- | --- | --- |
| graph-map-development-handoff:farm-production-work-yard:r1 | ReadyForDevelopment | graph-map-dev-slice:farm-production-work-yard.v1 | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | work:farm-crop-cycle:landscape-binding-guard | E4<br>PresentationE4Preparation |

## graph-map-development-handoff:farm-production-work-yard:r1

- Graph Map: [eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json](../../../eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json) / mirror-graph-map-plan.northern-life-hub-discovery.r3 / SHA-256 665b1b34ca6545d1b34a69af5fec8c6470da01eae836b4c0f65de053e0eb2e0b
- 선택 노드: gm-node:farm-production, gm-node:farm-work-yard
- 선택 엣지: gm-edge:farm-production-to-work-yard
- 선택 제약: gm-constraint:actual-reference-identity, gm-constraint:farm-flow-separation, gm-constraint:asset-candidate-not-assignment
- 코드 결속: gm-code:actual-e5-network-pipeline, gm-code:landscape-runtime-realization, gm-code:farm-h-placement-plan, gm-code:wi-seedbed-editor-preview, gm-code:h-spatial-rule-editor
- 개발 후보: work:farm-crop-cycle:landscape-binding-guard / 현재 Active / 자동 활성화 아님
- 작업 명세: [eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json) / SHA-256 cef233d99c8fcf6a588999bf85b9aa359cbeef71c14d073830cad423c886f6e7
- 현재 결과: 아직 개발 수용 전이며 코드·시험·Unity 결과가 없다.
- 다음 행동: 개발 담당이 현재 Goal·작업 명세·후보 work item의 소유와 겹침을 다시 확인한 뒤 수용 또는 차단을 반환한다.
