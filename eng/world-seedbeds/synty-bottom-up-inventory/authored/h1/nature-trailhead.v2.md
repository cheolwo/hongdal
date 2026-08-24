# 자연 탐색 출발지

@spatial-knowledge h1-stock:nature-trailhead
@hierarchy H1
@state ExploratoryInventory
@wi WI-WORLD-05
@wi WI-WORLD-07
@wi WI-NATURE-05
@gameplay TrailStart
@gameplay RouteCheck
@gameplay ExplorationBriefing
@role NatureTrailheadArea
@capability Spatial.Traversable
@capability Spatial.WorkerAccessible
@capability Spatial.InformationArea
@capability Spatial.PlayerAccessible
@capability Spatial.ToolPickupPoint
@capacity Actor
@capacity Tool
@predecessor h1-stock:road-facility-access
@successor h1-stock:nature-lookout
@successor h1-stock:nature-shelter
@connector RoadAccess
@connector TrailOutput
@grammar nature:산길·바위 길목
@grammar nature:숲 가장자리

## 존재 이유

산길·숲 탐색을 시작하고 안전 상태와 경로를 확인하는 진입 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
