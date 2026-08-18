# 농산물 선별 공간

@spatial-knowledge h1-stock:farm-sorting
@hierarchy H1
@state IdeaInventory
@gameplay ProduceSorting
@gameplay QualityGrading
@role FarmProduceSortingArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.SortingWorkArea
@predecessor h1-stock:farm-washing
@predecessor h1-stock:farm-harvest-staging
@successor h1-stock:farm-work-yard
@connector CargoInput
@connector PackingHandoff
@grammar farm:농산물 집하·직판장
@grammar farm:헛간 작업마당

## 존재 이유

세척 또는 집하된 농산물을 등급·상태별로 분류해 포장으로 넘기는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
