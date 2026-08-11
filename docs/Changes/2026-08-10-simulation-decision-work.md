# DECISION-WORK-0 공통 Decision·Task·Effect 인과 원장

## 변경

- Simulation session에 Decision, Task, Effect snapshot 배열을 추가했다.
- Decision Preview는 예상 비용·예상 효과·불확실성·차단 사유와 작업 계획을 반환하지만 session revision과 원장을 변경하지 않는다.
- 명시적 Confirm은 expected revision과 멱등 Command를 검증하고 `Confirmed Decision`, `Scheduled Task`, `Pending Effect`를 별도 원장으로 만든다.
- 세계 Tick만 Task를 `InProgress` 또는 `Completed`로 진행하고 완료 Tick에 Effect를 `Applied`로 전이한다.
- Decision은 작업 진행 중에도 Confirm 당시 revision과 원인을 유지한다.
- source lineage, stable ID, 작업 capacity·기간, Before + Delta = After 보존식을 검증한다.

## 경계

- 예상 효과는 Preview용 Interpretation이며 적용된 Effect와 같은 상태가 아니다.
- 현재 Applied Effect는 인과 기록이다. 아직 정착지 재정·노동·재고 원장의 실제 값을 변경하지 않는다.
- 차단 사유가 있는 Preview는 조회할 수 있지만 Confirm할 수 없다.
- Animation, NPC 이동과 Unity View는 Confirm이나 Tick을 자동 발생시키지 않는다.
- store는 여전히 process-local in-memory이며 save·restore는 다음 Gate다.

## 검증

- `SimulationDecisionWorkTests` 11/11 통과
- `Ssalddel.Simulation.Tests` 전체 113/113 통과
- Preview 무변경, Confirm/Tick 상태 분리, 중간 Tick, 멱등 deep clone 검증
- stale revision, blocked Preview, 중복 stable ID, Command payload·종류 충돌, 수치 보존식 오류 거부
- scoped Fast: `git diff --check`·`Ssalddel.v0.0.slnx` build 통과 (`artifacts/local/validation/20260810-203921`)
- scoped Task: 같은 build는 통과했고 전체 서버 test는 기존 비관련 metadata·WebApp·UI 7건으로 4,482/4,489 통과 (`artifacts/local/validation/20260810-203944`)

## 화면

화면 없음. 이번 단계는 Simulation Contracts·Domain·Server API의 인과 원장만 변경했다.
