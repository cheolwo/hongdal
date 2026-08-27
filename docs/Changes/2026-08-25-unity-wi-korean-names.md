# Unity WI 한국어 기능명·단일 책임 표시

- 검증 수준: 간접 확인
- 대상: canonical `SimulationWorldShell`의 작업 계획 표시와 WI 공간 모판 검토 화면
- 변경: `WI-NATURE-07` 같은 번호형 고유 식별자를 단독 표시하지 않고 `자연 탐사·생활 거점 · 오두막을 지을 터 선정 (WI-NATURE-07)`처럼 한국어 의미를 먼저 표시한다. 대장 순번은 실행 절차가 아니므로 `7단계`처럼 표시하지 않는다.
- 원본: `world-interactions.json`의 한국어 이름, `world-interaction-responsibilities.json`의 주요 결과·단일 책임 판정, `world-interaction-flows.json`의 선택 가능한 조립 흐름에서 C# 이름 카탈로그를 결정적으로 생성한다.
- 호환: 저장·재생·API·공간 결속에 쓰는 WI 고유 식별자는 변경하지 않았다.
- 자동 검증: Simulation 집중시험과 Unity EditMode `세계상호작용한국어이름Tests`가 통과했다.
- 미검증: Play Mode 수동 조작, 실제 Game View 줄바꿈·가독성, 대표 PNG와 Console 수동 확인은 수행하지 않았다.
