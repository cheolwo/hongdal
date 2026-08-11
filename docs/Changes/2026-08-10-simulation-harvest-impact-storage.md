# HARVEST-IMPACT-1·STORAGE-1 수확 판로 영향과 비축 후보

## 변경

- 기존 Unity의 `CooperativeShipment`, `DirectOnlineSale`, `ExportAgent` choice code와 `harvest-disposition:sim.potato.20260407.r1` 결정 stable ID·revision을 Simulation 서버 계약에 보존했다.
- 네 번째 `ReserveStorage → ReserveStockLotCandidate` 선택을 추가했다.
- 판로별 비용·노동·기간·예상 수입·시장 또는 반출 영향·위험·차단 사유를 서버 `harvest-impact:fixture-r1` 정책으로 계산하는 전용 Preview API를 추가했다.
- Confirm은 클라이언트 예상값을 받지 않고 같은 입력에서 공통 `Decision → Task → Effect` Preview를 다시 계산한다.
- HarvestLot과 판로 결정 revision을 공통 Decision source lineage에 포함했다.

## 비축 후보

- 300kg 입력과 2% Fixture 감모로 6kg 감모, 294kg 예상 비축량을 계산한다.
- 기존 감자 비축 Lot의 1.2 FoodEquivalent/kg 근거로 352.8 FoodEquivalent 추가 후보를 만든다.
- 현재 1,200 FoodEquivalent와 120/Tick 수요에서 `FoodSecurityDays`를 10에서 12.94로 바꾸는 후보를 표시한다.
- 창고 가용 capacity가 300kg보다 작거나 FoodEquivalent 근거가 없으면 block reason을 반환하고 Confirm을 거부한다.

## 경계

- Preview는 session revision, Decision, Task, Effect와 정착지 값을 변경하지 않는다.
- Confirm과 Tick은 후보 Decision·Task·Effect의 상태만 진행한다. Effect가 `Applied`여도 재정·노동·시장·창고·비축·FoodSecurityDays 실제 값은 아직 변경하지 않는다.
- 실제 StockLot 생성, HarvestLot allocation, 비용·수입 반영과 같은 300kg의 중복 배정 차단은 `SETTLEMENT-ECONOMY-1` 범위다.
- 보관은 즉시 군량이 아니며 운영 수확·판매·수출·계약·결제 효과를 만들지 않는다.
- 판로 추천 점수와 자동 선택은 없다.
- Unity Scene·Game View 변경은 없다.

## API

```text
POST /api/simulation/v1/sessions/{sessionStableId}/harvest-disposition-impact-previews
POST /api/simulation/v1/sessions/{sessionStableId}/harvest-disposition-impacts/confirm
```

## 검증

- `SimulationHarvestDispositionImpactTests` 18/18 통과
- `Ssalddel.Simulation.Tests` 전체 156/156 통과
- `Ssalddel.Simulation.slnx` build 경고 0·오류 0
- 네 판로별 정책, Preview 무변경, capacity 차단, 비축 계산, Confirm/Task/Effect, 원장 불변과 save/replay 동일 hash 확인
- scoped Fast: `git diff --check`·`Ssalddel.v0.0.slnx` build 통과 (`artifacts/local/validation/20260810-211843`)
- scoped Task: 같은 build는 통과했고 전체 서버 test는 기존 비관련 metadata·WebApp·UI 7건으로 4,482/4,489 통과 (`artifacts/local/validation/20260810-211900`)

## 화면

화면 없음. 이번 단계는 Simulation Contracts·Domain·Server API와 headless test만 변경했다.
