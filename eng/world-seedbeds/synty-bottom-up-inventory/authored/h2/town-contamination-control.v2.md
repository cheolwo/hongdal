# 생활권 오염 점검·정화 블록

@spatial-knowledge h2-candidate:town-contamination-control
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:town-contamination-inspection
@required-h1 h1-stock:town-contamination-quarantine
@required-h1 h1-stock:town-cleanup-transfer
@optional-h1 h1-stock:town-market-receiving
@connector MarketStockInput
@connector SafeDisplayOutput
@connector ServiceVehicleOutput
@evidence RoadNetwork
@evidence BuildingFootprint
@evidence BlockBoundary
@evidence MarketContext

## 존재 이유

시장 재고 점검과 오염 격리, 정화 폐기 인계를 후방 서비스 동선으로 묶는 레시피다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
