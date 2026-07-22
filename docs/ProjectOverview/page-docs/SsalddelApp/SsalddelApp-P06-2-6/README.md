# SsalddelApp-P06-2-6 - 피킹 task 목록

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [상위 허브](../SsalddelApp-P06-2/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/picking` |
| 소스 | [OrderFulfillmentPicking.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentPicking.razor) |
| 경계 | ReadOnly |
| 검증 | route·touch target·Windows build 확인 |

## 단일책임

로컬 피킹 task 목록을 읽고 사용자가 정확한 stable task ID Action 화면을 고르게 한다. 목록에서 스캔·보류·취소를 실행하지 않으며 수령인과 주소를 표시하지 않는다.
