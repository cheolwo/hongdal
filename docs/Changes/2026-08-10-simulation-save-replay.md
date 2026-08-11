# SAVE-REPLAY-0 versioned save와 Command replay

## 변경

- `simulation-save.v1` schema의 `SimulationSessionSavePackage`를 추가했다.
- package는 session 생성 요청, 저장 시점 snapshot, 순서화된 Confirm/Tick Command log와 SHA-256 replay hash를 보존한다.
- Save는 expected revision을 검증하지만 session Tick·revision·업무 원장을 변경하지 않는다.
- restore는 snapshot을 aggregate에 직접 주입하지 않고 생성 요청과 Command log를 새 aggregate에 재실행한다.
- package 자체 hash와 replay 결과 hash·WorldTick·WorldRevision이 모두 일치할 때만 복원 session을 store에 등록한다.
- 멱등 Command 재시도는 log 항목을 추가하지 않고 save package와 반환 snapshot은 deep clone한다.

## 거부 조건

- stale save revision
- 같은 SaveStableId에 다른 session 상태 저장
- 지원하지 않는 schema 또는 hash algorithm
- Command sequence·kind·payload 변조
- snapshot 또는 replay hash 변조
- replay 결과 Tick·revision 불일치
- 이미 활성인 같은 session 덮어쓰기

실패한 restore는 임시 aggregate를 session store에 등록하지 않는다.

## 경계

- `ISimulationSessionSaveStore` restore port는 구현했다.
- 기본 `InMemorySimulationSessionSaveStore`는 process-local 개발 adapter다.
- 실제 프로세스 재시작 뒤의 영속성, schema migration, 백업·보존 정책과 외부 durable adapter는 아직 구현하지 않았다.
- 운영 서버 DB·계약·주문·결제 원장과 save를 공유하지 않는다.
- Unity Scene·Game View 변경은 없다.

## 검증

- `SimulationSaveReplayTests` 10/10 통과
- `Ssalddel.Simulation.Tests` 전체 123/123 통과
- 새 session store에 같은 Command를 replay한 뒤 동일 hash·stable ID·완료 Task·Applied Effect 확인
- 같은 저장점의 다른 SaveStableId가 동일 replay hash를 갖는지 확인
- deep clone, 멱등 log, 변조 거부와 실패 원자성 확인
- scoped Fast·Task: 각각 `git diff --check`, `Ssalddel.v0.0.slnx` build와 자동 targeted 81/81 통과 (`artifacts/local/validation/20260810-205018`, `artifacts/local/validation/20260810-205046`)

## 화면

화면 없음. 이번 단계는 Simulation save·replay 계약, Domain, API와 in-memory port만 변경했다.
