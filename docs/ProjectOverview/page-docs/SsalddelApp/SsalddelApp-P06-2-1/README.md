# SsalddelApp-P06-2-1 - 판매 주문 원장 목록

[전체 화면 문서](../../README.md) / [SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

![판매 주문 원장 공용 Screen의 Web desktop 인증 경계](../../../../assets/changes/2026-07-22-sales-order-mobile-route-srp/sales-orders-desktop.png)

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | SsalddelApp·Ssalddel.WebApp 공용 Screen |
| 페이지 ID / 제목 | SsalddelApp-P06-2-1 - 판매 주문 원장 목록 |
| 라우트 | `/shipper/sales/orders` |
| 소스 파일 | `SsalddelApp/Components/Pages/SalesOrders.razor` |
| 공용 Screen | `ShipperSalesOrderWorkspace`의 `List` mode |
| 분류 | 확장·읽기 전용 |
| 캡처 상태 | 공용 Screen의 Web host 인증 경계 확인 |

## 단일책임

인증된 사용자가 영속 판매 주문 원장을 검색하고 동기화 범위, 상태와 페이지를 바꿔 정확한 주문 한 건으로 이동하는 일만 담당한다. 피킹·포장·재고 변경과 알림 정책 Command는 실행하지 않는다.

상세 링크는 목록 상태를 안전한 local `from`으로 보존한다. 기존 `?orderId=...` 링크는 같은 검색·필터·페이지 문맥을 유지한 stable-ID 상세 route로 호환 이동한다.

## 검증 경계

Web과 모바일이 같은 공용 Screen과 navigation contract를 사용한다. 실제 캡처는 통합 Web host에서 인증되지 않은 상태의 공개 경계를 확인한 것으로, `SsalddelApp` shell 자체의 실기기 캡처는 아니다. 개인정보나 영속 주문 데이터, Command는 사용하지 않았다.
