# Town 마트 입고·주문이행 경관

@spatial-knowledge h3-candidate:town-market-fulfillment
@hierarchy H3
@state ExploratoryInventory
@required-h2 h2-candidate:town-market-receiving
@required-h2 h2-candidate:market-life-commerce
@required-h2 h2-candidate:town-order-fulfillment
@optional-h2 h2-candidate:town-resident-service
@connector TownDeliveryInput
@connector MarketFulfillmentLoop
@connector ResidentPickupOutput

## 존재 이유

마트 후방 입고·검수에서 전면 진열과 주문 피킹·포장·주민 수령까지 이어지는 Town 시장 운영 구역이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H3`
- 실제 지역 권위: 없음

## 미해결

- 실제 AreaSet과 공공데이터 근거를 적용하기 전까지 조립 후보로 유지한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
