# Simulation 문화 턴 카드

## 결과

- 턴 마감 canonical catalog에 첫 `Culture` 카드인 `culture:kr-seoul.living-culture-question.2026`을 추가했다.
- 특정 행사나 지역 대표성을 사실처럼 만들지 않고 주민의 현재 경험과 공식 지역문화 원천을 함께 확인하는 질문 카드로 제한했다.
- Simulation game date 2026년 범위에서만 노출하며 Preview는 상태를 바꾸지 않고 Confirm 뒤 다음 턴에만 효과를 활성화한다.
- `LocalContextAwareness`와 `CommunityInsight +1`은 `culture-local-context-awareness:r1` 서버 규칙에서만 정한다.

## 근거 계약

문화카드는 다음 값이 모두 있어야 catalog에 들어온다.

- `RegionKey`: `kr-seoul`
- 유효 game date: `2026-01-01` ~ `2026-12-31`
- `CalendarRevision`: `simulation-culture-calendar:kr-seoul:2026.r1`
- `EffectRuleRevision`: `culture-local-context-awareness:r1`
- `SourceStableId`: `source:kr-regional-culture-promotion-agency`
- HTTPS source URL
- `EvidenceCheckedAtUtc`: `2026-07-26T00:00:00Z`

공식 원천은 저장소의 기존 지역문화 공공기관 seed를 재사용했다. 이는 지역문화진흥원의 관계기관 정보 확인 근거이며 서울의 특정 행사·생활양식·대표성을 증명하는 자료는 아니다.

## Unity

실제 `SimulationWorldShell` 턴 마감 패널에 `문화 · 서울 질문` 버튼을 추가했다. 카드 본문에서 지역, calendar revision, 지역문화진흥원과 근거 확인일을 보여주며 Confirm 후에만 Day 14·Tick 13·Revision 13과 `LocalContextAwareness`로 전환한다.

Game View는 `C:/Users/user/ssalddel/Assets/Documentation/Changes/2026-08-11-culture-card-0/`에 보존한다.

## 검증

- `SimulationTurnClosingTests`: 11/11 통과
- `Ssalddel.Simulation.Tests`: 270/270 통과
- `턴마감CoordinatorTests`: 6/6 통과
- `Ssalddel.Unity.Tests`: 364/364 통과
- 실제 Unity 턴 마감 EditMode: 4/4 통과
- 실제 Unity 전체 EditMode: 213/214 통과. 실패 1건은 기존 연구 Scene 기대 개수 27과 현재 28의 불일치
- scoped Fast build 통과 (`artifacts/local/validation/20260811-170723`)
- scoped Task build 통과, 전체는 기존 비관련 API metadata·분류·CSS·WebApp route 기대 7건으로 4,501/4,508 통과 (`artifacts/local/validation/20260811-170840`)
- Play Mode 문화카드 Preview→Confirm과 해당 구간 새 Console 오류 0건 확인
- 운영 live 호출·현재 원천 재확인·실제 행사 publication·commit·push 없음
