# 주문 피킹·포장·수령 블록

@spatial-knowledge h2-candidate:town-order-fulfillment
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:town-market-display
@required-h1 h1-stock:town-order-packing
@required-h1 h1-stock:town-resident-pickup
@optional-h1 h1-stock:town-neighborhood-service
@connector OrderStockInput
@connector PickingPackingLoop
@connector ResidentPickupOutput

## 존재 이유

마트 진열·피킹, 주문 포장, 주민 수령 공간을 순서대로 묶어 주문 이행을 수용하는 Town 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
