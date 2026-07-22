# SsalddelApp-P06-2-4-1 - Simulation 주문 상세

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [주문 목록](../SsalddelApp-P06-2-4/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/orders/{OrderKey}` |
| 소스 | [OrderFulfillmentOrderDetail.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentOrderDetail.razor) |
| 경계 | ReadOnly |
| 검증 | stable key round-trip·route·Windows build 확인 |

## 단일책임

주소의 opaque stable 주문 key가 가리키는 로컬 주문 한 건만 읽는다. 잘못된 key를 다른 주문으로 추측하지 않으며 목록 검색·국내외·상태 문맥으로 돌아간다.
