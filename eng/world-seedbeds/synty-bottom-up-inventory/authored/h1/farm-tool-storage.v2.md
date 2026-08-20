# 농기구 보관 공간

@spatial-knowledge h1-stock:farm-tool-storage
@hierarchy H1
@state CandidateForReview
@wi WI-WORLD-04
@gameplay ToolCheckout
@gameplay ToolReturn
@gameplay NextTaskPreparation
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

농장 작업 전후에 도구와 소형 장비를 보관·반환하고 다음 작업을 준비하는 생활 거점 공간이다.

## 설계 상태

- 재고 상태: `CandidateForReview`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 도구별 재고 원장은 후속 Simulation 계약이며 현재는 공간 수용 능력만 승인한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
