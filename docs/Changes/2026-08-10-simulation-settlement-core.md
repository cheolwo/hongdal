# SETTLEMENT-CORE-1 정착지 graph와 최소 경제·식량 snapshot

## 변경

- Simulation session 생성 요청에 선택적 `SimulationSettlementInitialStateRequest`를 추가했다.
- scenario 입력으로 District·Facility graph, 재정, 노동 capacity, storage, 상품별 시장 공급, 비축 StockLot, 주민·주둔군 식량 수요와 source lineage를 구성한다.
- `LaborAvailable = LaborCapacityTotal - LaborReserved`와 `StorageAvailable = StorageCapacity - StorageOccupied`를 계산한다.
- outbound 예약을 제외한 FoodEquivalent와 주민·주둔군 Tick 수요로 Fixture `FoodSecurityDays`를 계산한다.
- Confirm된 미완료 Task만 정착지 `ActiveTaskStableIds`에 투영하고 완료 Tick 뒤 제거한다.
- 정착지 생성 입력과 snapshot 전체를 `simulation-save.v1` replay hash와 deep clone에 포함했다.

## 무결성

- Facility는 존재하는 District만 참조한다.
- `Storage`와 `Market` Facility가 각각 하나 이상 필요하다.
- 비축 Lot은 `Storage` Facility만 참조하며 storage 단위가 일치해야 한다.
- 비축 Lot 수량 합은 `StorageOccupied`를 초과할 수 없다.
- 노동 예약은 전체 capacity를, storage occupied는 capacity를 초과할 수 없다.
- District·Facility·StockLot stable ID와 상품별 시장 공급은 중복될 수 없다.
- 주민과 주둔군 수·수요의 불일치를 거부한다.

## 경계

- 정착지 값은 scenario가 명시한 Simulation Fixture이며 Unity 화면 객체나 운영 서버에서 추정하지 않는다.
- 정착지 입력이 없는 기존 session은 `Settlement = null`로 호환한다.
- FoodEquivalent와 FoodSecurityDays는 실제 영양 처방이 아니며 rule revision과 단위를 보존한다.
- DECISION-WORK Effect는 아직 재정·노동·storage·시장·비축 값을 변경하지 않는다.
- 실제 판로별 allocation과 Lot 중복 배정 차단은 `SETTLEMENT-ECONOMY-1` 범위다.
- Unity Scene·Game View 변경은 없다.

## 검증

- `SimulationSettlementCoreTests` 15/15 통과
- 정착지+save 집중 회귀 25/25 통과
- `Ssalddel.Simulation.Tests` 전체 138/138 통과
- Fixture 1,200 FoodEquivalent / 120 demand per Tick = FoodSecurityDays 10 확인
- graph·capacity·중복·단위·수요 오류 거부, active Task, deep clone과 save/replay 동일 hash 확인
- scoped Fast: `git diff --check`·`Ssalddel.v0.0.slnx` build 통과 (`artifacts/local/validation/20260810-210347`)
- scoped Task: 같은 build는 통과했고 전체 서버 test는 기존 비관련 metadata·WebApp·UI 7건으로 4,482/4,489 통과 (`artifacts/local/validation/20260810-210402`)

## 화면

화면 없음. 이번 단계는 Simulation Contracts·Domain과 save/replay 입력만 변경했다.
