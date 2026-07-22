# SsalddelApp-P06-2-5 - 재고·입고 신호

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [상위 허브](../SsalddelApp-P06-2/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/inventory` |
| 소스 | [OrderFulfillmentInventory.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentInventory.razor) |
| 경계 | ReadOnly |
| 검증 | route·capability·Windows build 확인 |

## 단일책임

로컬 마켓 재고 snapshot과 안전재고 이하 입고 검토 신호만 읽는다. 입고 요청, 구매, 창고 예약 또는 공급자 연락을 자동 실행하지 않는다.
