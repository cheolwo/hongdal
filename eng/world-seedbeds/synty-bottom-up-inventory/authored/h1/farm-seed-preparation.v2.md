# 종자 준비 공간

@spatial-knowledge h1-stock:farm-seed-preparation
@hierarchy H1
@state ExploratoryInventory
@wi WI-FARM-02
@gameplay SeedInspection
@gameplay SeedBatchPreparation
@role FarmSeedPreparationArea
@capability Spatial.WorkerAccessible
@capability Spatial.MaterialPreparationArea
@predecessor h1-stock:farm-tool-storage
@successor h1-stock:farm-production
@connector MaterialHandoff
@connector WorkerAccess
@grammar farm:헛간 작업마당
@grammar farm:시설하우스 단동

## 존재 이유

파종 전에 종자와 소모품을 확인하고 작업 단위로 준비하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
