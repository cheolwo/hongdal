# SsalddelApp-P06-2-2 - 판매 주문 원장 상세

[전체 화면 문서](../../README.md) / [SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

![판매 주문 공용 Screen의 Web 390px stable-ID 상세 인증 경계](../../../../assets/changes/2026-07-22-sales-order-mobile-route-srp/sales-order-detail-mobile.png)

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | SsalddelApp·Ssalddel.WebApp 공용 Screen |
| 페이지 ID / 제목 | SsalddelApp-P06-2-2 - 판매 주문 원장 상세 |
| 라우트 | `/shipper/sales/orders/{OrderId:long}` |
| 소스 파일 | `SsalddelApp/Components/Pages/SalesOrderDetail.razor` |
| 공용 Screen | `ShipperSalesOrderWorkspace`의 `Detail` mode |
| 분류 | 확장·읽기 전용 |
| 캡처 상태 | 공용 Screen의 Web host 390px 인증 경계 확인 |

## 단일책임

stable order ID 한 건의 주문과 출고 투영을 읽는 일만 담당한다. 목록 검색·동기화 범위·상태·페이지를 `from`에 보존하고 뒤로가기에서 그대로 복원한다. 조회 화면은 피킹·포장 또는 외부 주문 동기화 Command를 노출하지 않는다.

## 모바일 기준

통합 Web host의 390px에서 가로 넘침 없이 단일 열로 표시되며 새 목록 복귀 동작은 48px 터치 영역을 갖는다. `SsalddelApp` shell 자체의 실기기 캡처는 아니며, 공용 Screen의 인증 경계를 확인한 화면으로 영속 주문 데이터나 개인정보는 포함하지 않는다.
