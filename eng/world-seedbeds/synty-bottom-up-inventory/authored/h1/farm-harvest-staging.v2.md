# 수확물 임시 적치 공간

@spatial-knowledge h1-stock:farm-harvest-staging
@hierarchy H1
@state ExploratoryInventory
@wi WI-FARM-04
@wi WI-FARM-05
@gameplay TemporaryCropStorage
@gameplay WaitForPacking
@gameplay WaitForVehicle
@role FarmHarvestStagingArea
@capability Spatial.CargoAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.TemporaryStorage
@predecessor h1-stock:farm-production
@successor h1-stock:farm-work-yard
@successor h1-stock:farm-loading-gate
@connector HarvestInput
@connector PackingHandoff
@connector LoadingHandoff
@grammar farm:농산물 집하·직판장
@grammar farm:헛간 작업마당

## 존재 이유

수확물을 포장·상차 전에 잠시 적치하고 다음 작업의 용량을 기다리는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
