# ShipperApp-P10-2 - 꾸미기 FakePG 결제

[전체 화면 문서](../../README.md) / [ShipperApp 화면 목록](../README.md) / [통합 클라이언트 가이드](../../../unified-community-client.md)

## 화면 캡처

<img src="../../../assets/app-pages/ShipperApp/ShipperApp-P10-2.png" alt="ShipperApp-P10-2 개발용 FakePG 결제" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/community/decorations/{ProductKey}/checkout` |
| 내비게이션 단계 | 3단계 보조 기능 처리 페이지 |
| 소스 | [CommunityDecorationCheckoutPage.razor](../../../../../ShipperApp/Components/Pages/CommunityDecorationCheckoutPage.razor) |
| 분류 | 개발·확장 |
| 캡처 | 완료, Android 1080×2400 |

## 왜 필요한가

실제 결제 시스템을 연결하기 전에 상품 확인, 수단 선택, 동의, 승인, 보유, 적용으로 이어지는 UX를 검증한다.

## 사용자와 화면 책임

가상 카드 또는 가상 간편결제를 선택하고 개발 결제 안내에 동의하면 현재 앱 실행 상태에 보유권을 추가하고 첫 자산을 적용한다. 다른 상품 탐색과 세부 자산 선택은 상점·상세 페이지로 돌려보낸다.

## 결제·보안 점검

- 실제 금액 청구, 환불, 창작자 정산을 수행하지 않는다.
- 현재 화면은 로컬 상태만 변경한다.
- 서버의 `POST api/v1/community/node-sticker-store/fake-pg/confirm`은 인증 및 Development 환경 검사가 있지만 아직 이 화면과 연결되지 않았다.
- 운영 환경에서는 클라이언트 금액·보유 상태를 신뢰하지 않고 서버 승인 결과로 갱신해야 한다.

## 다른 화면과의 관계

- 이전·완료 후 자산 선택: [ShipperApp-P10-1](../ShipperApp-P10-1/)
- 계속 탐색: [ShipperApp-P10](../ShipperApp-P10/)
