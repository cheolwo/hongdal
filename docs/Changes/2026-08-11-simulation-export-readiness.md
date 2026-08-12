# Simulation 수출 서류·검사 준비성 검토

## 결과

항만 준비시설 인수가 완료된 수출 Cargo의 서류 묶음과 후속 검사 준비 여부를 별도 Preview·Confirm·Task·WorldTick으로 검토한다. 준비가 부족하면 누락 코드를 남기고, 보완 뒤 새 stable ID로 재검토할 수 있다.

## 흐름

```text
ReceivedAtPortStaging
  → 준비성 검토 Preview
  → 서류·검사 준비 입력과 Cargo 계보 확인
  → 준비성 검토 Confirm
  → 검토 Task
  → WorldTick
  ├─ ReadyCandidate
  └─ ActionRequired
       → 보완
       → 부모 검토를 보존한 재검토
```

## 경계

- 검토 입력은 자기 진술형 Simulation 값이며 공공기관 확인 자료가 아니다.
- 항만 인수 전 Cargo와 다른 시설의 검토를 차단한다.
- 준비 후보 완료 뒤 같은 항만 인수의 중복 검토를 차단한다.
- 기존 출고 예약 300kg과 HarvestLot부터 Cargo까지의 계보를 유지한다.
- 실제 수출신고·공식 검사·검역 승인·통관·선복 예약·선적을 생성하지 않는다.

## 검증

- 수출 준비부터 준비성 재검토까지 집중 테스트: 41/41 통과
- Simulation 전체 테스트: 237/237 통과
- scoped Fast 통과
- scoped Task 빌드 통과, 전체 테스트는 기존 비관련 7건 실패로 4,501/4,508 통과
- 화면 변경 없음
