# 생활권 회수 안내·자연권 구호 블록

@spatial-knowledge h2-candidate:town-recall-relief
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:town-recall-service
@required-h1 h1-stock:town-nature-relief
@required-h1 h1-stock:town-neighborhood-service
@optional-h1 h1-stock:town-resident-pickup
@optional-h1 h1-stock:town-living-square
@connector ResidentInput
@connector ReturnOutput
@connector NatureReliefOutput
@evidence RoadNetwork
@evidence BuildingFootprint
@evidence BlockBoundary
@evidence MarketContext

## 존재 이유

주민 회수 안내와 생활 서비스, 자연권 구호 물자 인계를 하나의 지역 대응 거점으로 묶는 레시피다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
