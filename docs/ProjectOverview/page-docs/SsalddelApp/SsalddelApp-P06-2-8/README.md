# SsalddelApp-P06-2-8 - 입고 알림 정책

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [상위 허브](../SsalddelApp-P06-2/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/restock-policy` |
| 소스 | [OrderFulfillmentRestockPolicy.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentRestockPolicy.razor) |
| 경계 | Simulation · 로컬 정책만 변경 |
| 검증 | route·capability·Windows build 확인 |

## 단일책임

판매자별 관리자 허용·수신 동의·내부 알림과 SKU override를 로컬 정책으로 편집한다. 저장은 실제 카카오톡이나 다른 메시지를 발송하지 않는다.
