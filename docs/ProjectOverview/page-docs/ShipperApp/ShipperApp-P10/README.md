# ShipperApp-P10 - 꾸미기 상점

[전체 화면 문서](../../README.md) / [ShipperApp 화면 목록](../README.md) / [통합 클라이언트 가이드](../../../unified-community-client.md)

## 화면 캡처

<img src="../../../assets/app-pages/ShipperApp/ShipperApp-P10.png" alt="ShipperApp-P10 꾸미기 상점" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/community/decorations` |
| 내비게이션 단계 | 3단계 보조 기능 페이지 |
| 소스 | [CommunityDecorationStorePage.razor](../../../../../ShipperApp/Components/Pages/CommunityDecorationStorePage.razor) |
| 분류 | 확장 |
| 캡처 | 완료, Android 1080×2400 |

## 왜 필요한가

괘상과 다이어그램 노드 이미지를 홈의 임시 패널에 섞지 않고 독립적인 탐색 페이지에서 비교하기 위해 필요하다.

## 사용자와 화면 책임

`플랫폼 기본`, `크리에이터`, `내 보유` 탭을 제공하고 상품 카드를 상세 페이지로 연결한다. 결제 승인이나 실제 적용은 이 페이지에서 수행하지 않는다.

## 상태·보안 점검

현재 상품과 보유 상태는 `PlatformCommunityDecorationStateService`의 앱 실행 상태다. 가격과 보유권을 보안 근거로 신뢰해서는 안 되며 서버 연동 시 계정 기준으로 다시 조회해야 한다.

## 다른 화면과의 관계

- 상품 선택: [ShipperApp-P10-1](../ShipperApp-P10-1/)
- 직접 만들기: [ShipperApp-P10-3](../ShipperApp-P10-3/)
- 돌아가기: [ShipperApp-P00](../ShipperApp-P00/)
