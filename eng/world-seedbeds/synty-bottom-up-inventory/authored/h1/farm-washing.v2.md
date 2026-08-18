# 농산물 세척 공간

@spatial-knowledge h1-stock:farm-washing
@hierarchy H1
@state IdeaInventory
@gameplay ProduceWashing
@gameplay WaterUse
@gameplay WasteWaterHandling
@role FarmProduceWashingArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.WaterAccessible
@capability Spatial.WashingWorkArea
@predecessor h1-stock:farm-harvest-staging
@successor h1-stock:farm-sorting
@connector CargoInput
@connector WetProcessOutput
@grammar farm:헛간 작업마당
@grammar nature:수변 완충지

## 존재 이유

수확물의 흙과 이물질을 제거해 선별·포장으로 넘기는 습식 작업 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
