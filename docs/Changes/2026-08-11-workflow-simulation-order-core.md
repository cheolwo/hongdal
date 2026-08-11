# 운영 업무 규칙과 Simulation 개별주문 기반

## 범위

- `WORKFLOW-RULES-0`: 음식배달·화물운송·개별주문·같이주문의 상태·전이·수량 보존과 운영 제외 효과
- `SIM-ORDER-CORE-1`: Simulation 주문·재고예약 원장과 공통 결정·작업·효과 연결
- `SIM-INDIVIDUAL-ORDER-1`: 감자 300kg 중 주민 주문 20kg의 예약·포장·수령준비·완료 전 취소

## 결과

운영 API를 Simulation에서 호출하거나 운영 DB를 공유하지 않는다. 공통 규칙은 source capability와 contract/rule revision을 보존하고, Simulation adapter는 같은 업무 의미를 가상 시장재고·노동·WorldTick에 적용한다.

```text
감자 시장재고 300kg
  → 주문 Preview 20kg (무변경)
  → Confirm (재고·노동 예약)
  → 포장 Task
  → WorldTick
  → 수령준비·잔여 280kg

또는 완료 전 취소
  → 포장 Task·재고 Effect 취소
  → 예약 재고·노동 반환
```

실제 결제, 실제 주문 등록, 기사 배차, GPS, 주소와 개인 알림은 수행하지 않는다.

## 검증

- 공통 규칙 parity·수량 보존 집중 테스트: 7/7 통과
- Simulation 개별주문 집중 테스트: 6/6 통과
- Simulation 전체 회귀 테스트: 172/172 통과
- scoped Fast: `git diff --check`, `Ssalddel.v0.0.slnx` build 통과
- scoped Task: build 통과, 전체 4,505개 중 기존 비관련 route·metadata·CSS 기대 7건 실패
- 화면 변화: 없음
- Unity runtime 검증: 대상 아님
