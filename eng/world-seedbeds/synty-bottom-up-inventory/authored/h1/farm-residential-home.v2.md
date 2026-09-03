# 농장 생활 주택

@spatial-knowledge h1-stock:farm-residential-home
@hierarchy H1
@state ExploratoryInventory
@gameplay ResidentialBuildingIdentity
@gameplay ResidenceAccessBoundary
@gameplay ResidentialObservationSightline
@role FarmResidentialHome
@capability Spatial.ResidentialBuildingIdentity
@capability Spatial.ResidentialFootprint
@capability Spatial.ResidentialEntrance
@capability Spatial.ResidentialInteriorBoundary
@capability Spatial.ResidentialObservationSightline
@capability Spatial.ResidentialUsePermissionBoundary
@capacity BuildingFootprint
@capacity ResidenceOccupancy
@connector FarmHomeIngress
@connector FarmHomeObservation
@connector ResidentPermissionBoundary
@grammar farm:헛간 작업마당
@grammar town:저층 주택 블록

## 존재 이유

농장 안에서 영속적인 생활 주택의 건물 정체성, footprint, 출입구, 내외부 경계, 관찰 시야와 소유·이용 허락 경계를 제공한다. 이 H1은 주택이 농장 전체나 생활 행동의 권위가 되지 않도록 건물 경계만 맡는다.

## 책임 분리

- 귀환·휴식·작업자 대기: `h1-stock:farm-worker-waiting`
- 농기구 보관·반환: `h1-stock:farm-tool-storage`
- 생산: `h1-stock:farm-production`
- 수확·집하 작업: `h1-stock:farm-work-yard`

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음
- 보유 Synty 적합 후보: 미동결

## 미해결

- 정확 주택 Prefab, footprint 크기, 출입구 방향과 관찰 시야 수치는 개별 배치 맵과 Presentation E4에서 동결한다.
- 한스 집은 일반 H1 정의가 아니라 별도 배치 프로필의 특정 인스턴스로 결속한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
