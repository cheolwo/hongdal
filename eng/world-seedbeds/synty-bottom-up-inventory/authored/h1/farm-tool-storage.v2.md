# 농기구 보관 공간

@spatial-knowledge h1-stock:farm-tool-storage
@hierarchy H1
@state ExploratoryInventory
@wi WI-WORLD-04
@gameplay ToolCheckout
@gameplay ToolReturn
@role FarmToolStorageArea
@capability Spatial.Storage
@capability Spatial.WorkerAccessible
@successor h1-stock:farm-seed-preparation
@successor h1-stock:farm-maintenance-yard
@connector WorkerAccess
@connector MaintenanceHandoff
@grammar farm:헛간 작업마당
@grammar farm:시설하우스 단동

## 존재 이유

농장 작업 전후에 도구와 소형 장비를 보관·반환하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
