# 농장 수확물 노출 점검 공간

@spatial-knowledge h1-stock:farm-exposure-inspection
@hierarchy H1
@state ExploratoryInventory
@gameplay HarvestExposureInspection
@gameplay ContaminationAssessment
@gameplay SafeHandoffDecision
@role FarmExposureInspectionArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.InspectionWorkArea
@predecessor h1-stock:farm-production
@predecessor h1-stock:farm-harvest-staging
@successor h1-stock:farm-incident-quarantine
@successor h1-stock:farm-work-yard
@connector HarvestInput
@connector SafeCargoOutput
@connector QuarantineOutput
@grammar farm:농산물 집하·직판장
@grammar farm:헛간 작업마당

## 존재 이유

수확 직후 비·야생동물·오염에 노출된 수확물을 정상 집하 흐름에 넣기 전에 확인하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
