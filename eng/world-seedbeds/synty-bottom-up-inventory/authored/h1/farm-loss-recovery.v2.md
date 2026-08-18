# 농장 손실 복구·재작업 공간

@spatial-knowledge h1-stock:farm-loss-recovery
@hierarchy H1
@state IdeaInventory
@gameplay CropLossAssessment
@gameplay ProduceRework
@gameplay RecoveryPacking
@role FarmLossRecoveryWorkArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.SortingWorkArea
@capability Spatial.PackingWorkArea
@predecessor h1-stock:farm-incident-quarantine
@successor h1-stock:farm-work-yard
@successor h1-stock:farm-restoration-supply
@connector QuarantineInput
@connector RecoveredCargoOutput
@connector LossOutput
@grammar farm:농산물 집하·직판장
@grammar farm:헛간 작업마당

## 존재 이유

사건 처리 뒤 회수 가능한 수확물을 재선별·재포장하고 손실량을 분리하는 복구 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
