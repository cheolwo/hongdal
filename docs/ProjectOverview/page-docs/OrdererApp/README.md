# OrdererApp 화면 문서

[전체 화면 문서](../README.md) / [앱 전체 카탈로그](../../app-page-catalog.md)

이 문서는 OrdererApp 에 속한 화면별 README를 모은 색인입니다. 각 화면 문서는 캡처 이미지, 화면 책임, 사용자와 참여자, API/서버 연계, 보안 점검을 별도로 설명합니다.

| 페이지 ID / 제목 | 라우트 | 분류 | 화면 책임 | 캡처 |
| --- | --- | --- | --- | --- |
| [OrdererApp-P01 - 주문자 홈](OrdererApp-P01/) | / | 보조 | 주문자 홈 | 완료 |
| [OrdererApp-P02 - 공동구매 의사 표시/집단화](OrdererApp-P02/) | /group-purchase | 확장 | 공동구매 의사 표시/집단화 | 완료 |
| [OrdererApp-P03 - 주문자 화물 주문](OrdererApp-P03/) | /cargo | 확장 | 주문자 화물 주문 | 완료 |
| [OrdererApp-P04 - 음식 주문 홈](OrdererApp-P04/) | /food | 확장 | 음식 주문 홈 | 완료 |
| [OrdererApp-P04-1 - 음식점 주문](OrdererApp-P04-1/) | /food/restaurants | 확장 | 음식점 주문 | 완료 |
| [OrdererApp-P04-2 - 마트 공개 상품 목록](OrdererApp-P04-2/) | `/food/mart` | 확장 | 공개 상품 검색·판매 가능 조건·서버 페이징 | desktop 완료 |
| [OrdererApp-P04-2-1 - 마트 공개 상품 상세](OrdererApp-P04-2-1/) | `/food/mart/products/{ProductId}` | 확장 | stable product ID 공개 설명·재고 투영·구매 근거 읽기 | stable-ID 실제 확인 |
| [OrdererApp-P04-2-2 - 마트 상품 구매후기 작성](OrdererApp-P04-2-2/) | `/food/mart/reviews/{ProductId}` | 확장 | 완료 원장 참여자의 명시적 공개 후기 저장과 같은 상품 재조회 | 390px 완료 |
| [OrdererApp-P04-3 - 마트 비구속 주문 요청](OrdererApp-P04-3/) | `/food/mart/order/{ProductId}` | 확장 | 공개 상품 한 건의 주문 요청 저장과 같은 요청 ID 재조회 | stable-ID·390px 로그인 경계 확인 |
| [OrdererApp-P05 - 주문 이력](OrdererApp-P05/) | /orders | 보조 | 주문 이력 | 완료 |
| [OrdererApp-P99 - 미발견 페이지](OrdererApp-P99/) | /not-found | 시스템 | 미발견 페이지 | 완료 |
