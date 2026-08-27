# City/Hub 보관·피킹·상차 운영 경관

@spatial-knowledge h3-candidate:hub-fulfillment-operations
@hierarchy H3
@state ExploratoryInventory
@required-h2 h2-candidate:hub-longterm-cold-storage
@required-h2 h2-candidate:hub-fulfillment
@required-h2 h2-candidate:hub-outbound-vehicle
@optional-h2 h2-candidate:hub-maintenance-yard
@optional-h2 h2-candidate:hub-internal-warehouse
@connector StorageCargoInput
@connector FulfillmentLoop
@connector HubLoadingOutput

## 존재 이유

장기·저온 보관 블록에서 출고 화물을 피킹·준비하고 차량 상차까지 넘기는 City/Hub 출고 운영 구역이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H3`
- 실제 지역 권위: 없음

## 미해결

- 실제 AreaSet과 공공데이터 근거를 적용하기 전까지 조립 후보로 유지한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
