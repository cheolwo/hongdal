# 생활권 정화·폐기 인계 공간

@spatial-knowledge h1-stock:town-cleanup-transfer
@hierarchy H1
@state IdeaInventory
@gameplay ContaminatedWasteTransfer
@gameplay MarketCleanup
@gameplay ServiceVehicleHandoff
@role TownCleanupTransferArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.VehicleAccessible
@capability Spatial.WasteHandlingArea
@predecessor h1-stock:town-contamination-quarantine
@predecessor h1-stock:town-recall-service
@successor h1-stock:town-waste
@successor h1-stock:hub-returns
@connector CleanupInput
@connector ServiceVehicleOutput
@connector HubReturnOutput
@grammar town:정원·담장 경계
@grammar city:화물 대기 야드

## 존재 이유

오염 재고와 정화 작업 폐기물을 주민 동선에서 분리해 서비스 차량 또는 Hub 회수 흐름으로 넘기는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
