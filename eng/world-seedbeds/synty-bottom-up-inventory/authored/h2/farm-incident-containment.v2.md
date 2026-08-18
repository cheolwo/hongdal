# 농장 사건 점검·격리 블록

@spatial-knowledge h2-candidate:farm-incident-containment
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:farm-exposure-inspection
@required-h1 h1-stock:farm-incident-quarantine
@required-h1 h1-stock:farm-weather-protection
@optional-h1 h1-stock:farm-harvest-staging
@connector HarvestInput
@connector SafeCargoOutput
@connector RecoveryOutput
@evidence RoadNetwork
@evidence BuildingFootprint
@evidence BlockBoundary
@evidence WeatherContext

## 존재 이유

외부 노출 점검과 오염 격리, 기상 보호를 생산 흐름에서 분리해 피해 확산을 막는 레시피다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
