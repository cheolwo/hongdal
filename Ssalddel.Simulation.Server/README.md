# Ssalddel Simulation Server

Unity 경영 Simulation의 session, scenario clock와 deterministic command 권위를 기존 운영 서버에서 분리하는 별도 ASP.NET Core host다.

현재 첫 slice는 다음만 제공한다.

- Simulation session 생성·조회
- expected revision 기반 Tick 진행
- `CommandId` 멱등 재시도
- scenario seed, Data revision과 rule revision 보존
- 최대 duration을 넘는 Tick 차단

이 서버는 `Ssalddel`, `Ssalddel.Contracts`, `Ssalddel.Domain`, `Ssalddel.Infrastructure`와 `Ssalddel.Unity`를 참조하지 않는다. 실제 계약·발주·결제·입고·재고를 만들지 않으며 두 서버 사이에 공유 DB도 없다.

API는 기본 비활성이다. 승인된 Simulation 환경에서만 `SimulationServer:Enabled=true`로 켜며 `SsalddelExecution:Mode=Simulation`이 아니면 host 시작을 거부한다. 현재 store는 프로세스 수명에 한정된 in-memory 구현이므로 재시작 시 session이 사라진다.

```text
POST /api/simulation/v1/sessions
GET  /api/simulation/v1/sessions/{sessionStableId}
POST /api/simulation/v1/sessions/{sessionStableId}/ticks
GET  /health
```

다음 slice에서 인증된 session scope, durable snapshot store, scenario package validator와 공급 계약 4주 Engine을 추가한다.
