# 세척·선별·포장 블록

@spatial-knowledge h2-candidate:farm-wash-sort-pack
@hierarchy H2
@state IdeaInventory
@required-h1 h1-stock:farm-harvest-staging
@required-h1 h1-stock:farm-washing
@required-h1 h1-stock:farm-sorting
@required-h1 h1-stock:farm-work-yard
@connector HarvestInput
@connector ShippingOutput
@evidence RoadNetwork
@evidence WaterPresence
@evidence BlockBoundary

## 존재 이유

수확물 적치에서 세척·선별·포장까지 이어지는 후처리 레시피다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
