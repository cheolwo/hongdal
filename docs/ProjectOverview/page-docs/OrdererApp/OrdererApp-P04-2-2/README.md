# OrdererApp-P04-2-2 - 마트 상품 구매후기 작성

[전체 화면 문서](../../README.md) / [OrdererApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

390px에서 후기 전용 제목, 2열 navigation과 익명·완료 원장 미확인 disabled 상태를 실제 확인했습니다.

<img src="../../../../assets/changes/2026-07-22-mart-product-route-srp/mart-product-review-mobile.png" alt="OrdererApp-P04-2-2 mobile" width="390">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | OrdererApp, Ssalddel.WebApp |
| 페이지 ID / 제목 | OrdererApp-P04-2-2 - 마트 상품 구매후기 작성 |
| canonical route | `/food/mart/reviews/{ProductId}` |
| Web legacy alias | `/orderer/mart/reviews/{ProductId}` |
| 공용 화면 | `MartProductReviewScreen` |
| capability | Beta / PlatformPersistence / 인증 필요 |
| 캡처 상태 | 390px 실제 렌더링 완료, [변경 기록](../../../../Changes/2026-07-22-mart-product-route-srp.md) |

## 한 가지 책임

완료된 구매 원장에 접근할 수 있는 로그인 사용자가 공개 가능한 후기 한 건을 명시적으로 저장합니다. 서버가 사용자와 원장 접근 권한을 다시 검증하고, 성공 뒤 목록 전체가 아니라 같은 product ID 상세 한 건만 다시 조회합니다.

주소·연락처·결제정보를 입력하지 않도록 안내하며, 자격이 없거나 익명인 사용자를 임시 후기 저장으로 우회하지 않습니다. 후기 저장은 주문, 재고, 결제, 피킹·포장과 배송 상태를 변경하지 않습니다.

## API와 검증

- `GET /api/v1/orderer/mart/products/{productId}`
- `POST /api/v1/orderer/mart/products/{productId}/reviews`
- 작성 성공 뒤 같은 product ID를 두 번째로 조회하고 목록 API를 호출하지 않는 자동 테스트 통과
- capability에서 상세 ReadOnly와 후기 PlatformPersistence·인증 필요 경계를 분리
- 390px navigation 2열·63px, horizontal overflow 없음과 final console 오류 0 실제 확인
- 후기 저장 Command는 실행하지 않음
