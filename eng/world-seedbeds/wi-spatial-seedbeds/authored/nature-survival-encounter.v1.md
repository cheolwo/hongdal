# Nature 생존 탐사·조우 공간 모판

- 공간 모판: `wi-spatial-seedbed:nature-survival-encounter.v1`
- 포함 WI: `WI-NATURE-01`, `WI-NATURE-06`, `WI-NATURE-18`, `WI-NATURE-11`
- H 결속: 벌목·조우 H1 모판 → `h2-candidate:nature-encounter-route` → `h3-candidate:nature-home-encounter-defense`
- 검토 상태: `ApprovedForSimulation`

플레이어가 위협 방향과 압력을 관찰하는 순간, 나무를 골라 벌목 시작을 확정하는 순간, 도끼 장착을 바꾸는 순간, 황혼 조우에서 싸움·후퇴를 고르는 순간을 서로 다른 WI로 관리한다. `WI-NATURE-18`은 벌목 장소에서 요구되는 명시적 장착 문맥을 제공하지만 장착 자체가 벌목 결과를 만들지는 않는다. `threat-watch`는 관찰과 1 slot 예약만 소유하며 원인 조사·위협 해결·보상 판정을 수행하지 않는다. 벌목의 4초 진행, 황혼 조우의 자동 발생, 나무·통나무 결과는 각각 `Task` 또는 자동 상태 전이·`Effect`다.

`ResourceNode`, `WorkArea`, `EncounterArea`, `RetreatRoute` 용량은 공간 충돌과 취소 규칙을 구현하기 위한 H1 계약이다. 진행 중인 벌목 취소는 생활핵 모판의 `WI-NATURE-12`로 인계해 기존 예약을 안전하게 해제한다. 조우 해결 뒤에는 생활핵의 `WI-NATURE-17` 위임 보급 정책으로 돌아갈 수 있고, 완성된 보급은 다시 `WI-NATURE-06` 원정 선택으로 인계된다. 이 모판은 위치 독립 E4 기준이며 실제 Nature AreaSet의 H2·H3 배치나 Play Mode 증거를 주장하지 않는다.
