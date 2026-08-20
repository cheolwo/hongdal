# 마트 후방 입고·검수 블록

@spatial-knowledge h2-candidate:town-market-receiving
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:road-facility-access
@required-h1 h1-stock:town-market-receiving
@optional-h1 h1-stock:town-staff-rest
@optional-h1 h1-stock:town-market-display
@connector DeliveryAccessInput
@connector ReceivingInspectionLoop
@connector BackroomOutput

## 존재 이유

도로–시설 진입부와 마트 후방 입고 공간을 연결해 하차·인수·검수·후방 적재를 수용하는 Town 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
