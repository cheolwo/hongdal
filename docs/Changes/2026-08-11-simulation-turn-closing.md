# Simulation 경영일 턴 마감

## 결과

- 경영 중에는 게임 날짜를 자동 진행하지 않고 턴 마감 Preview와 명시적 Confirm에서만 다음 WorldTick으로 넘어간다.
- 현재 경영일, 미완료 Task 수와 선택 가능한 카드 catalog를 조회한다.
- 카드 없이 넘기거나 바보·전차 Fixture 중 한 장을 선택할 수 있다.
- 선택한 철학 카드 효과는 끝난 날이 아니라 다음 경영일에만 활성화된다.
- 턴 마감 Command, 선택 카드, 턴 기록과 활성 효과를 save/replay에 포함한다.

## API

- `GET api/simulation/v1/sessions/{sessionStableId}/turn-closing-context`
- `POST api/simulation/v1/sessions/{sessionStableId}/turn-closing-previews`
- `POST api/simulation/v1/sessions/{sessionStableId}/turn-closings/confirm`

기존 `POST .../ticks`는 호환용으로 유지하며 플레이 UI의 기본 진행 경로는 아니다.

## 카드 경계

초기 두 카드는 `evening-hakdang.fixture-r1`이다. 승인 publication이나 실제 문화 자료가 아니며 Unity나 LLM이 효과 수치를 만들지 않는다. 문화 카드는 종류 code만 예약했고 지역·기간·출처·달력 revision과 효과 규칙이 정해진 뒤 별도 slice에서 추가한다.

## Unity adapter와 WorldShell

`Ssalddel.Unity`에는 context·카드·Preview·Confirm 최소 wire model과 `I턴마감AuthorityClient`, `턴마감Coordinator`를 추가했다. Coordinator는 catalog 밖 카드와 Preview 없는 Confirm을 차단하고 Confirm 응답의 session·revision·완료 Tick·다음 턴 카드 effect가 일치할 때만 마지막 성공 session을 교체한다.

별도 Unity 저장소의 `SimulationWorldShell`에는 카드 없음·바보·전차와 Preview·Confirm 버튼을 실제로 연결했다. Preview는 기존 Tick과 Revision을 유지하고 Confirm 성공 뒤에만 HUD snapshot을 교체한다. 현재 Scene은 `턴마감FixtureAuthorityClient`를 사용하므로 서버 API의 portable adapter와 실제 Unity HTTP transport 결합은 후속 작업이다.

## 검증

- `SimulationTurnClosingTests`: 8/8 통과
- `Ssalddel.Simulation.Tests`: 267/267 통과
- `턴마감CoordinatorTests`: 4/4 통과
- `Ssalddel.Unity.Tests`: 362/362 통과
- scoped Fast: `git diff --check`, v0.0 build 통과 (`artifacts/local/validation/20260811-141303`)
- scoped Task: build 통과, 전체 테스트는 기존 비관련 7건 실패로 4,501/4,508 통과 (`artifacts/local/validation/20260811-141322`)
- 별도 Unity `턴마감Tests`: 3/3 통과
- 별도 Unity 전체 EditMode: 212/213 통과. 실패 1건은 기존 연구 Scene 기대 개수 27과 현재 28의 불일치
- `TURN-CARD-UI-1B` Scene wiring validation 통과
- Play Mode 바보 카드 Preview→Confirm 뒤 HUD `04-13 · Tick 13 · Revision 13`, `DAY 14 시작`, `BeginnerMind` 활성화와 새 Console 오류 0건 확인
- Game View: `C:/Users/user/ssalddel/Assets/Documentation/Changes/2026-08-11-turn-card-ui-1b/turn-closing-fool-preview.png`, `turn-closing-next-day.png`
- 운영 live 호출·승인 publication·문화 카드·commit·push 없음
