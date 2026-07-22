# SsalddelApp-P06-2-3 - Simulation 샘플 반영

[SsalddelApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md) / [상위 허브](../SsalddelApp-P06-2/)

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/shipper/sales/fulfillment/samples` |
| 소스 | [OrderFulfillmentSamples.razor](../../../../../SsalddelApp/Components/Pages/OrderFulfillmentSamples.razor) |
| 경계 | Simulation · 로컬 메모리만 변경 |
| 검증 | route·capability·Windows build 확인 |

## 단일책임

외부 판매채널을 호출하지 않고 비식별 샘플 주문을 로컬 원장에 명시적으로 반영한 뒤 같은 원장을 다시 읽는 일만 담당한다. 실제 주문 수집, 재고 차감, 출고·운송·결제·정산은 실행하지 않는다.
