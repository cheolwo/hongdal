# 생활권 폐기물 처리 공간

@spatial-knowledge h1-stock:town-waste
@hierarchy H1
@state IdeaInventory
@gameplay WasteSorting
@gameplay WasteStorage
@gameplay WasteCollection
@role TownWasteHandlingArea
@capability Spatial.WorkerAccessible
@capability Spatial.TemporaryStorage
@capability Spatial.ServiceVehicleAccessible
@predecessor h1-stock:town-market-display
@predecessor h1-stock:town-returns
@successor h1-stock:road-facility-access
@connector WasteInput
@connector ServiceVehicleOutput
@grammar transition:Road–BuildingFront 전환
@grammar town:정원·담장 경계

## 존재 이유

판매·반품·생활 활동에서 발생한 폐기물을 임시 집하하고 외부 처리로 인계하는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
