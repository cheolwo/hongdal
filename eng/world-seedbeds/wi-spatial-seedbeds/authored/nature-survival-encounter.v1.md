# Nature 생존 탐사·조우 공간 모판

- 공간 모판: `wi-spatial-seedbed:nature-survival-encounter.v1`
- 포함 WI: `WI-NATURE-06`, `WI-NATURE-11`
- H 결속: 벌목·조우 H1 모판 → `h2-candidate:nature-encounter-route` → `h3-candidate:nature-home-encounter-defense`
- 검토 상태: `ApprovedForSimulation`

플레이어가 나무를 골라 벌목 시작을 확정하는 순간과 황혼 조우에서 싸움·후퇴를 고르는 순간을 WI로 관리한다. 벌목의 4초 진행, 황혼 조우의 자동 발생, 나무·통나무 결과는 각각 `Task` 또는 자동 상태 전이·`Effect`다.

`ResourceNode`, `WorkArea`, `EncounterArea`, `RetreatRoute` 용량은 공간 충돌과 취소 규칙을 구현하기 위한 H1 계약이다. 진행 중인 벌목 취소는 생활핵 모판의 `WI-NATURE-12`로 인계해 기존 예약을 안전하게 해제한다. 이 모판은 위치 독립 E4 기준이며 실제 Nature AreaSet의 H2·H3 배치나 Play Mode 증거를 주장하지 않는다.
