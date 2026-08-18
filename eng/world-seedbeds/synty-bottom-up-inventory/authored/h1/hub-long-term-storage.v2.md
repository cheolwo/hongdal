# Hub 장기 보관 공간

@spatial-knowledge h1-stock:hub-long-term-storage
@hierarchy H1
@state ExploratoryInventory
@wi WI-002
@gameplay LongTermStorage
@gameplay StorageAging
@gameplay CapacityPlanning
@role HubLongTermStorageArea
@capability Spatial.Storage
@capability Spatial.CargoAccessible
@capability Spatial.WorkerAccessible
@predecessor h1-stock:hub-temporary-staging
@successor h1-stock:hub-outbound-staging
@connector StorageInput
@connector PickingOutput
@grammar city:화물 대기 야드
@grammar city:물류 Station 진입부

## 존재 이유

즉시 출고하지 않는 화물을 장기간 보관하고 점유 용량을 추적하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
