# SIM-WORLD-0 공통 Simulation World 문맥과 날짜

## 변경

- Simulation session 생성 요청에 `FactionStableId`, `TerritoryStableId`, `SettlementStableId`를 요구한다.
- UTC 자정 `GameDateStartsOn`과 `1 Tick = 1 Game Day` 규칙을 추가했다.
- snapshot에 `WorldTick`, `WorldRevision`, 현재 `GameDate`와 `CalendarRuleCode`를 제공한다.
- 기존 `CurrentTick`·`Revision`은 호환을 위해 유지하며 World 값과 항상 동일하게 projection한다.
- 같은 ClientRequest의 멱등 payload 비교에 World context를 포함한다.
- Tick 재시도 snapshot의 World context를 deep clone해 caller 변경이 저장 결과를 오염시키지 않도록 했다.

## 경계

- Pause·1x·2x·4x는 client의 Tick 요청 빈도이며 별도 서버 시간 상태가 아니다.
- PresentationTimeOfDay와 현실 자료 ReferenceDate를 GameDate에 합치지 않는다.
- 운영 서버·운영 DB·Unity Scene·Game View를 변경하지 않았다.
- 현재 store는 process-local in-memory라 재시작 뒤 session을 복원하지 못한다.

## 검증

- `경영SimulationSessionTests` 17/17 통과
- `Ssalddel.Simulation.Tests` 전체 102/102 통과
- 연도 경계 2026-12-30 + 3 Tick = 2027-01-02 확인
- scoped Fast: `git diff --check`·`Ssalddel.v0.0.slnx` build 통과 (`artifacts/local/validation/20260810-202149`)
- scoped Task: 같은 build는 통과했고 전체 서버 test는 기존 비관련 metadata·WebApp·UI 7건으로 4,482/4,489 통과 (`artifacts/local/validation/20260810-202258`)

## 화면

화면 없음. 이번 단계는 Simulation Contracts·Domain·Server의 공통 세계 문맥만 변경했다.
