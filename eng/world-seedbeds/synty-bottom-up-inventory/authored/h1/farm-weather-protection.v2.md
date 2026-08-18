# 농장 기상 보호 적치 공간

@spatial-knowledge h1-stock:farm-weather-protection
@hierarchy H1
@state ExploratoryInventory
@gameplay WeatherProtectedStaging
@gameplay HarvestDelay
@gameplay MaterialShelter
@role FarmWeatherProtectedStagingArea
@capability Spatial.CargoAccessible
@capability Spatial.TemporaryStorage
@capability Spatial.WeatherShelter
@predecessor h1-stock:farm-harvest-staging
@successor h1-stock:farm-work-yard
@successor h1-stock:farm-loading-gate
@connector HarvestInput
@connector PackingOutput
@connector LoadingOutput
@grammar farm:헛간 작업마당
@grammar farm:시설하우스 단동

## 존재 이유

비·강풍·한파로 집하나 상차를 진행하기 어려울 때 수확물과 작업 자재를 임시 보호하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
