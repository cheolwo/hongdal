# SsalddelApp-P06-2-6-1 - 피킹 task Action

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [피킹 목록](../SsalddelApp-P06-2-6/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/picking/{TaskId:long}` |
| 소스 | [OrderFulfillmentPickingTask.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentPickingTask.razor) |
| 경계 | Simulation · 로컬 task만 변경 |
| 검증 | positive task ID·route·Windows build 확인 |

## 단일책임

주소의 정확한 task ID 한 건에만 적재함·상품 스캔, 보류 또는 취소를 적용하고 같은 Simulation 원장을 다시 읽는다. 존재하지 않는 ID를 다른 task로 자동 대체하지 않는다.
