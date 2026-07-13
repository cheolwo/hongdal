# ShipperApp-P10-1 - 꾸미기 상품 상세

[전체 화면 문서](../../README.md) / [ShipperApp 화면 목록](../README.md) / [통합 클라이언트 가이드](../../../unified-community-client.md)

## 화면 캡처

<img src="../../../assets/app-pages/ShipperApp/ShipperApp-P10-1.png" alt="ShipperApp-P10-1 꾸미기 상품 상세" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/community/decorations/{ProductKey}` |
| 내비게이션 단계 | 3단계 보조 기능 상세 페이지 |
| 소스 | [CommunityDecorationDetailPage.razor](../../../../../ShipperApp/Components/Pages/CommunityDecorationDetailPage.razor) |
| 분류 | 확장 |
| 캡처 | 완료, Android 1080×2400 |

## 왜 필요한가

사용자가 상품의 창작자, 사용 위치, 가격, 포함 자산과 실제 적용 모습을 확인한 뒤 구매 또는 적용을 선택하게 한다.

## 사용자와 화면 책임

상품을 찾고 자산별 미리보기를 제공한다. 무료 또는 보유 상품은 적용하고, 유료 미보유 상품은 결제 페이지로 보낸다. 목록 탐색과 결제 승인은 별도 페이지의 책임이다.

## 권리·접근성 점검

크리에이터 상품은 검수 상태와 사용권을 확인해야 한다. 이미지가 표시되지 않거나 사용자가 꾸미기를 끈 경우에도 기호, 대체 텍스트, 기본 업무 아이콘으로 기능을 유지한다.

## 다른 화면과의 관계

- 이전: [ShipperApp-P10 상점](../ShipperApp-P10/)
- 유료 구매: [ShipperApp-P10-2 FakePG](../ShipperApp-P10-2/)
- 적용 결과: [ShipperApp-P00 통합 홈](../ShipperApp-P00/) 또는 다이어그램
