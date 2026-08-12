# Simulation 수출 Cargo 항만 준비시설 인수

## 결과

항만 준비시설에 도착한 수출 Cargo를 별도 Preview·Confirm·Task·WorldTick으로 인수한다. 수출 준비에서 확보한 300kg 예약과 HarvestLot부터 Cargo까지의 계보는 그대로 유지한다.

## 흐름

```text
ArrivedAtDestination
  → 항만 인수 Preview
  → 인계·Cargo·Lot·수량·목적 시설 검증
  → 항만 인수 Confirm
  → 인수 Task
  → WorldTick
  → ReceivedAtPortStaging
  → 별도 수출 준비 여부 결정 대기
```

## 경계

- 이동 중이거나 목적 시설이 다른 Cargo는 인수할 수 없다.
- 기존 출고 예약 300kg을 해제하거나 중복 예약하지 않는다.
- 항만 인수는 수출신고·공식 검사·검역·통관·선적이 아니다.
- 하역 차량과 NPC 연출은 인수 완료를 확정하지 않는다.

## 검증

- 수출 준비부터 항만 인수까지 집중 테스트: 34/34 통과
- Simulation 전체 테스트: 230/230 통과
- scoped Fast 통과
- scoped Task 빌드 통과, 전체 테스트는 기존 비관련 7건 실패로 4,501/4,508 통과
- 화면 변경 없음
