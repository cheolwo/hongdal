# SsalddelApp-P06-2-7-1 - 포장 task Action

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [포장 목록](../SsalddelApp-P06-2-7/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/packing/{TaskId:long}` |
| 소스 | [OrderFulfillmentPackingTask.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentPackingTask.razor) |
| 경계 | Simulation · 로컬 task만 변경 |
| 검증 | positive task ID·route·Windows build 확인 |

## 단일책임

주소의 정확한 task ID 한 건에만 포장 시작·완료를 적용하고 같은 Simulation 원장을 다시 읽는다. 실제 출고·운송 인계를 만들지 않고 존재하지 않는 ID를 다른 task로 대체하지 않는다.
