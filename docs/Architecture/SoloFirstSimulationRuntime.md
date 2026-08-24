# Solo 우선 Simulation Runtime

## 결정

게임 세계의 규칙과 상태 전이는 `Simulation Core`가 소유한다. `Simulation.Server`는 Core 자체가 아니라 Hosted Multiplayer에서 Core를 실행하는 Host다.

```text
Simulation Core
├─ Domain / Rules
├─ Session Aggregate
├─ Preview / Confirm
├─ Task / Effect
├─ WorldTick / WorldRevision
└─ Save / Replay
        │
        ├─ Solo: LocalSimulationRuntime → Unity 프로세스 → 로컬 저장 슬롯
        └─ Hosted: RemoteSimulationRuntime → HTTP/Network → 서버 저장소
```

Solo의 권위는 Unity `GameObject`나 `Update()`가 아니라 Unity 프로세스 안에서 실행되는 `LocalSimulationRuntime`에 있다. Hosted에서는 같은 Application·Domain을 서버 Host가 실행한다. 규칙을 로컬용과 서버용으로 복제하지 않는다.

운영 서버의 실제 사용자 권한·계약·발주·결제·입고 원장 권위는 이 결정으로 바뀌지 않는다. 게임 Simulation의 `AuthorityLocation`과 운영 실행의 `SsalddelExecution:Mode`는 별도 축이다.

## 공통 실행 경계

`ISimulationRuntime`은 실행 위치를 숨기고 기능별 하위 경계를 제공한다. 현재 구현은 Session 생명주기, Nature 생존과 Session Aggregate 기반 턴 마감·Farm 판로 선택·화물 배차·물류 명령 경계를 포함한다.

- `LocalSimulationRuntime`: 기존 Application 서비스와 Session Aggregate를 프로세스 안에서 직접 실행한다.
- `SimulationAuthorityLocation.LocalProcess`: 네트워크 없이 실행되는 Solo 권위다.
- `SimulationAuthorityLocation.RemoteHost`: Hosted Adapter가 사용할 실행 위치다. 통합 Adapter는 후속 구현이다.
- `ReviewFixture`: 시험·검토 입력이며 Solo 또는 Hosted 실행 위치와 같은 개념이 아니다.

명령은 `ExpectedRevision`을 검증하고, 결과는 상태 사본으로 Unity에 돌아온다. Unity 표현 코드는 재고·작업·Tick·개정을 직접 변경하지 않는다.

## 로컬 저장

Solo 저장 슬롯은 기존 `simulation-save.v13` 패키지와 재생 hash를 그대로 보존한다. 파일 Adapter는 슬롯 이름을 검증하고 임시 파일 쓰기와 백업 교체를 사용하며, 읽기 전 checksum과 Replay를 검증한다. 저장 위치가 로컬 파일로 바뀌어도 Command log, `WorldTick`, `WorldRevision`, 규칙 판본과 canonical hash의 의미는 바뀌지 않는다.

## 현재 이관 범위

첫 이관 대상은 `nature-survival.realtime.r1`이다. canonical `SimulationWorldShell`의 Nature 조립은 `Nature생존CoreRuntimeAdapter`를 통해 공통 `LocalSimulationRuntime`을 실행한다. 이어 턴 마감, Farm 판로 선택과 물류 이동도 `SimulationWorldLocalRuntimeScope`의 같은 Runtime·Session을 사용한다. 물류의 후보 점수·추천·확정과 예약은 공통 배차·물류 규칙이 계산하며 Unity는 결과만 투영한다. 로컬 물류 Preview는 고정 Cargo Fixture를 권위 상태처럼 쓰지 않고 같은 Session에서 실제로 포장 완료된 Lot과 적용된 수확 배분을 찾아 요청을 만든다. 포장 Lot이 없으면 `LocalSimulationLogisticsCargoNotReady`로 멈춘다. 기존 `Nature생존LocalEngine`은 호환 시험과 규칙 대조용으로 남아 있지만 공식 조립의 권위는 아니다.

Hub 입고, 현장 전투, Nature 탐험 조우와 실제 E5 Network는 아직 공통 Local Runtime으로 이관되지 않았다. Solo 조립에서는 이들이 HTTP를 시도하거나 Fixture 성공으로 후퇴하지 않고 `LocalSimulationCapabilityWaiting` 상태로 멈춘다. 물류는 로컬 조립과 상태 조회까지 이관됐지만 Farm 수확→판로 선택→포장→배차→운송의 실제 입력 폐루프는 아직 완주하지 않았다. 따라서 Console 네트워크 오류 0개를 남은 기능 또는 물류 플레이 완료로 확대 해석하지 않는다.

## 이관 관문

이 리팩토링의 하향 영향과 상향 재조립 상태는 [`solo-first-simulation-runtime.e9-work-order.json`](../../eng/execution-ledgers/work-orders/solo-first-simulation-runtime.e9-work-order.json)을 기준으로 관리한다. 기능별 Adapter를 수평으로 계속 추가하는 대신 `E9→E1` 영향 분석을 먼저 끝내고, 가장 낮은 미완료 단계부터 `E1→E9` 순서로 다시 검증한다. 현재는 E3까지 통과했고 다음 관문은 영향받은 WI–H1 결속과 Farm 내부 반환 계약을 닫는 E4다.

1. 기능의 Application·Domain이 ASP.NET·HTTP·서버 환경 변수 없이 실행된다.
2. 기존 HTTP 경로는 같은 Core를 호출하는 Remote Adapter로 유지한다.
3. Local Adapter는 동일 명령·개정·상태 사본·Save/Replay를 사용한다.
4. 기능별 집중 시험에서 Local과 Hosted의 canonical 결과가 일치한다.
5. `SimulationWorldShell`에서 서버 프로세스 없이 새 게임→플레이→저장→종료→복원을 검증한다.
6. 남은 네트워크 호출과 서버 전용 모듈이 0개가 된 뒤에만 전체 Solo 오프라인 완료를 선언한다.
