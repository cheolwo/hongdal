# OrdererApp-P04-2-1 - 마트 공개 상품 상세

[전체 화면 문서](../../README.md) / [OrdererApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | OrdererApp, Ssalddel.WebApp |
| 페이지 ID / 제목 | OrdererApp-P04-2-1 - 마트 공개 상품 상세 |
| canonical route | `/food/mart/products/{ProductId}` |
| Web legacy alias | `/orderer/mart/products/{ProductId}` |
| 공용 화면 | `MartProductDetailScreen` |
| capability | Beta / ReadOnly / 익명 공개 |
| 캡처 상태 | stable-ID 실제 렌더링 완료, [변경 기록](../../../../Changes/2026-07-22-mart-product-route-srp.md) |

## 한 가지 책임

stable product ID 한 건의 공개 설명, 판매가, 마지막 재고 투영, 완료 원장 근거와 공개 후기를 읽기 전용으로 표시합니다. 후기 입력과 주문 요청 입력은 이 화면에 두지 않고 각각 별도 route로 이동합니다.

없는·비공개 상품 ID를 첫 상품이나 sample 상품으로 대체하지 않습니다. 상세 조회만으로 구매 의향, 재고 예약·차감, 결제, 피킹·포장과 배송을 생성하지 않습니다.

## API와 이동

- `GET /api/v1/orderer/mart/products/{productId}`
- 후기 작성: `/food/mart/reviews/{ProductId}`
- 비구속 주문 요청: `/food/mart/order/{ProductId}`
- 목록 검색·조건·페이지와 안전한 `from` 문맥을 세 route 사이에 보존합니다.

## 현재 검증

- 상세 Screen에 후기 저장 ViewModel과 `작성Async`가 없음을 구조 테스트로 고정
- 정확한 ID not-found 무대체 ViewModel 테스트 통과
- clean commit 기준 전체 테스트 2,528개와 Web·Orderer·통합·Admin 소비 빌드 통과
- `/food/mart/products/1` 직접 진입, 공개 설명·재고 투영·구매 근거 읽기와 horizontal overflow 없음 실제 확인
