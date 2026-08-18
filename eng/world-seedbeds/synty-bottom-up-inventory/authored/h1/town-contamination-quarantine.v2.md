# 생활권 오염 재고 격리 공간

@spatial-knowledge h1-stock:town-contamination-quarantine
@hierarchy H1
@state IdeaInventory
@gameplay MarketStockQuarantine
@gameplay RecallHold
@gameplay ReleaseOrDispose
@role TownContaminationQuarantineArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.ExclusiveOccupancy
@capability Spatial.TemporaryStorage
@predecessor h1-stock:town-contamination-inspection
@successor h1-stock:town-recall-service
@successor h1-stock:town-cleanup-transfer
@connector InspectionHoldInput
@connector RecallOutput
@connector DisposalOutput
@grammar town:읍내 상점 전면
@grammar city:상하차 Dock

## 존재 이유

오염이 의심되는 재고를 후방 정상 재고와 분리해 회수·폐기·재검사 결정을 기다리는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
