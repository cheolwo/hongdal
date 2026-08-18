# 농장 사고 수확물 격리 공간

@spatial-knowledge h1-stock:farm-incident-quarantine
@hierarchy H1
@state IdeaInventory
@gameplay FarmCargoQuarantine
@gameplay IncidentHold
@gameplay ReleaseOrDiscard
@role FarmIncidentQuarantineArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.ExclusiveOccupancy
@capability Spatial.TemporaryStorage
@predecessor h1-stock:farm-exposure-inspection
@successor h1-stock:farm-loss-recovery
@successor h1-stock:farm-restoration-supply
@connector InspectionHoldInput
@connector RecoveryOutput
@connector DisposalOutput
@grammar farm:헛간 작업마당
@grammar farm:농산물 집하·직판장

## 존재 이유

노출·오염이 의심되는 수확물을 정상 집하·포장 화물과 분리해 해결 선택을 기다리는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
