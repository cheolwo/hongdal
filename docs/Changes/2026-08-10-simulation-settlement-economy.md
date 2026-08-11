# Simulation 정착지 경제 원장 적용

- 날짜: 2026-08-10
- 범위: `SETTLEMENT-ECONOMY-1`
- 화면: 없음 — Simulation Server 계약·Domain·save/replay·test 변경

## 변경

- 수확 Lot마다 하나의 `HarvestLotAllocation`을 만들고 `Reserved → Applied` 상태를 보존한다.
- Confirm에서 labor·treasury와 비축 선택의 storage capacity를 예약한다.
- 같은 HarvestLot의 두 번째 판로 Confirm을 차단한다.
- Task 완료 Tick에서 비용·Simulation 수입·시장 공급 또는 비축 Stock Lot·FoodEquivalent를 반영하고 예약을 해제한다.
- 수확 판로 Confirm을 전용 Command payload로 저장하고 restore 때 같은 명령을 재실행한다.

## 경계

- Preview는 원장을 바꾸지 않는다.
- 실제 계약·판매·수출·입고·정산을 만들지 않는다.
- Unity의 차량·NPC·상자 수는 Task 완료나 수량의 권위가 아니다.
- Unity Scene과 Game View는 변경하지 않았다.

## 검증

- 수확 판로·정착지·save/replay 집중 test 48/48 통과
- `Ssalddel.Simulation.Tests` 전체 161/161 통과
- scoped Fast: `git diff --check`, `Ssalddel.v0.0.slnx` build 통과
- scoped Task: 같은 build 통과 후 기존 비관련 7건 실패, 4,482/4,489 통과
