# Simulation 수출 Cargo 배송대행지 인계

## 결과

준비 완료된 수출 Cargo를 배송대행지가 넘겨받는 Simulation 인계를 별도 Preview·Confirm·WorldTick으로 분리했다.

## 흐름

```text
Cargo ReadyForHandoff
  → 배송대행지 인계 Preview
  → Confirm
  → 인계 Task
  → HandedOffInSimulation
  → 별도 물류 이동 결정 대기
```

## 경계

- Cargo 준비 목적 시설과 실제 인계 요청 시설이 같아야 한다.
- 준비 완료 전 또는 같은 Cargo의 중복 인계를 차단한다.
- Simulation 인계 완료는 실제 사업자의 인수 증빙이나 계약이 아니다.
- 배차, 차량 지정, 상차, 출발과 물류 이동을 생성하지 않는다.
- 수출신고와 통관을 생성하지 않는다.

## 검증

- 수출 준비·재작업·Cargo 준비·인계 집중 테스트: 22/22 통과
- Simulation 전체 테스트: 218/218 통과
- scoped Fast 통과
- scoped Task 빌드 통과, 전체 테스트는 기존 비관련 7건 실패로 4,501/4,508 통과
- 화면 변경 없음
