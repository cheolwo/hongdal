# Simulation 수출 Cargo 물류 이동 연결

## 결과

배송대행지 Simulation 인계가 완료된 Cargo를 기존 물류 이동 Preview·Confirm에 연결했다. 수출 준비 단계에서 확보한 300kg 예약을 다시 더하지 않고 그대로 승계한다.

## 흐름

```text
HandedOffInSimulation
  → 물류 이동 Preview
  → 인계·Cargo·Lot·수량·출발 시설 검증
  → 물류 이동 Confirm
  → Cargo 출발
  → 경로 진행
  → 항만 준비시설 도착 후보
  → 별도 목적지 인수 결정 대기
```

## 경계

- 인계 완료 전 Cargo는 이동할 수 없다.
- 기존 출고 예약 300kg을 중복 예약하지 않는다.
- 화물운송 binding이 없으면 운송사·차량·배차를 만들지 않는다.
- 차량 연출은 Cargo 출발이나 도착을 확정하지 않는다.
- 목적지 도착은 재고 확정이나 수출신고·통관 완료가 아니다.

## 검증

- 수출 준비부터 물류 이동까지 집중 테스트: 28/28 통과
- Simulation 전체 테스트: 224/224 통과
- scoped Fast 통과
- scoped Task 빌드 통과, 전체 테스트는 기존 비관련 7건 실패로 4,501/4,508 통과
- 화면 변경 없음
