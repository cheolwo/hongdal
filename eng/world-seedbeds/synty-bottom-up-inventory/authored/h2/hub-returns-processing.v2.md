# Hub 반품 처리 블록

@spatial-knowledge h2-candidate:hub-returns-processing
@hierarchy H2
@state IdeaInventory
@required-h1 h1-stock:hub-returns
@required-h1 h1-stock:hub-quarantine
@optional-h1 h1-stock:hub-temporary-staging
@connector ReturnInput
@connector RestockOutput
@connector DisposalOutput
@evidence RoadNetwork
@evidence BuildingFootprint
@evidence BlockBoundary

## 존재 이유

반품 접수에서 격리·재입고·폐기 인계까지 분기하는 회수 물류 레시피다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
