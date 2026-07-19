# SsalddelAdmin-P40 - 개발용 Fake PG/정산 콘솔

[전체 화면 문서](../../README.md) / [SsalddelAdmin 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

현재 전용 캡처 대기 상태다.

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/development/fake-payment-settlement` |
| 내비게이션 단계 | 3단계 운영·개발 처리 페이지 |
| 소스 | [FakePaymentSettlementConsole.razor](../../../../../SsalddelAdmin/Components/Pages/FakePaymentSettlementConsole.razor) |
| 분류 | 개발 |
| 캡처 | 대기 |

## 왜 필요한가

실결제 없이 운송 원장의 결제 확보, 상차, 하차/POD, 정산, 보류, 보류 해제, 환불 상태 전이를 운영자가 검증한다.

## 화면 책임과 보안

`FakePaymentSettlementSimulationService`의 메모리 시나리오만 조작한다. 실결제·실정산·실제 운송계약을 생성하지 않는다는 경고를 항상 표시하며, 운영 환경 노출은 별도 개발 기능 정책으로 차단해야 한다.

## 다른 화면과의 관계

검증한 상태 정의는 운송 원장, 결제, 정산 화면과 맞춰야 하지만 이 콘솔의 메모리 데이터가 운영 원장에 반영되어서는 안 된다.
