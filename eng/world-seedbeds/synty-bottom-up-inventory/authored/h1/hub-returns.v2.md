# Hub 반품 처리 공간

@spatial-knowledge h1-stock:hub-returns
@hierarchy H1
@state IdeaInventory
@gameplay ReturnReceiving
@gameplay ReturnInspection
@gameplay RestockOrDispose
@role HubReturnsProcessingArea
@capability Spatial.CargoAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.InspectionWorkArea
@predecessor h1-stock:hub-market-transfer
@successor h1-stock:hub-quarantine
@successor h1-stock:hub-temporary-staging
@connector ReturnInput
@connector RestockOutput
@connector DisposeOutput
@grammar city:상하차 Dock
@grammar city:화물 대기 야드

## 존재 이유

반송된 화물을 접수하고 재입고·폐기·격리 경로로 분기하는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
