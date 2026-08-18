# 마트 반품 접수 공간

@spatial-knowledge h1-stock:town-returns
@hierarchy H1
@state IdeaInventory
@gameplay CustomerReturn
@gameplay ReturnTriage
@gameplay ReturnHandoff
@role MarketReturnsArea
@capability Spatial.CustomerAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.InspectionWorkArea
@predecessor h1-stock:town-resident-pickup
@successor h1-stock:hub-returns
@successor h1-stock:town-waste
@connector CustomerReturnInput
@connector HubReturnOutput
@connector WasteOutput
@grammar city:도심 마트 앞마당
@grammar town:읍내 상점 전면

## 존재 이유

주민 반품을 접수해 재진열·Hub 반송·폐기로 분기하는 생활권 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
