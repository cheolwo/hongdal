# Simulation 시장 주문·주민 소비·잔여재고 수렴

## 범위

- `MARKET-CONSUMPTION-1`
- 개별주문·소진된 재고예약·시장 잔여재고 연결
- 주민 수령·소비 Preview·Confirm·WorldTick
- 품목별 주민 소비 누계와 save/replay

## 결과

시장재고는 주문 이행 완료 시 한 번만 차감하고 주민 소비 단계에서는 이중 차감하지 않는다.

```text
시장 감자 300kg
  → 주문 Confirm: 20kg 예약
  → 포장 완료 Tick: 예약 소진 + 시장 잔여 280kg
  → 소비 Preview: 주민 소비 20kg, 시장 잔여 280kg
  → 소비 Confirm: Task 예약
  → 완료 Tick: 주민 소비 누계 20kg·1건, 시장 잔여 280kg
```

수령준비 전 소비, 다른 주문자의 확인, 수량 불일치와 같은 주문의 중복 소비를 차단한다. 품목별 소비량은 주문에 근거한 Simulation 누계이며 실제 개인정보·결제·영양 환산·비축 소비를 뜻하지 않는다.

## 검증

- Simulation 시장 소비 집중 테스트: 6/6 통과
- Simulation 전체 회귀 테스트: 196/196 통과
- scoped Fast: `git diff --check`, `Ssalddel.v0.0.slnx` build 통과
- scoped Task: build 통과, 기존 비관련 route·metadata·CSS 기대 7건으로 전체 4,501/4,508 통과
- 화면 변화: 없음
- Unity runtime 검증: 대상 아님
- 운영 API 호출·실제 주문·결제: 수행하지 않음
- commit·push: 수행하지 않음
