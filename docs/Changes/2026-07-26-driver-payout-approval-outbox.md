# 기사 운송대금 지급 승인·Outbox

## 결과

- 읽기 전용 지급 준비 결과를 서버관리자가 다시 확인한 뒤 내부 승인 기록과 Outbox를 같은 저장 단위로 남기도록 보완했다.
- 완료 운송, 원천 의뢰 ID, 기사 지급 예정 금액, 화주 수납, 확인된 기사 정산계좌를 서버에서 다시 대조한다.
- 운송별 1개 승인과 요청 멱등 키의 고유 인덱스를 두고, 같은 키·같은 자료 재시도는 기존 결과를 반환하며 같은 키·다른 자료는 거부한다.
- Outbox payload와 감사 필드에는 계좌번호와 예금주명을 넣지 않는다.

## 실행 경계

- `Simulation`
  - Outbox 처리는 `SimulationVerified`로 기록한다.
  - 실제 송금 완료로 표시하지 않으며 응답의 `IsActualTransferCompleted`는 `false`다.
- `Operational`
  - 실제 지급 Provider가 아직 없으므로 `OperationalProviderNotConfigured`로 차단한다.
  - 외부 송금을 시도하거나 화주 수납을 기사 지급 완료로 바꾸지 않는다.
- 일시 오류만 `RetryScheduled`로 두고 시도 횟수, 다음 시도 시각, 결과 코드와 비민감 오류 메시지를 남긴다.

## API와 저장

- `GET /api/v1/admin/driver-payouts?year=&month=&driverId=`
- `POST /api/v1/admin/driver-payouts/approve`
- `기사운송대금지급요청`
- `기사지급_Outbox`
- migration: `20260726111538_AddDriverPayoutApprovalOutbox`

## 화면

- 화면 없음 — 관리자 지급 운영 UI와 실제 지급 Provider는 이번 범위에 포함하지 않았다.
- 기사 화면에서는 기존 지급 준비 조회만 유지하며, Simulation 검증을 실제 입금 완료로 표시하지 않는다.

## 검증

- 관리자 권한·조건 재검증·금액 불일치·멱등 재시도·민감정보 비포함을 검증했다.
- Simulation 검증 완료, Operational Provider 미구성 차단, 일시 오류 재시도 감사를 검증했다.
- migration은 생성만 했고 실제 DB에는 적용하지 않았다.
