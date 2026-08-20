# Hub 피킹·출고준비 작업 블록

@spatial-knowledge h2-candidate:hub-fulfillment
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:hub-outbound-staging
@required-h1 h1-stock:hub-temporary-staging
@optional-h1 h1-stock:hub-long-term-storage
@optional-h1 h1-stock:hub-vehicle-yard
@connector StorageInput
@connector PickingLoop
@connector OutboundStagingOutput

## 존재 이유

보관 화물을 피킹하고 임시 적치한 뒤 차량 상차 공간으로 넘기는 City/Hub 출고 작업 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
