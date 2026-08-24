# 자연 탐색·완충 공간

@spatial-knowledge h1-stock:nature-exploration-buffer
@hierarchy H1
@state ExploratoryInventory
@wi WI-WORLD-05
@wi WI-WORLD-07
@wi WI-NATURE-06
@gameplay TimberHarvest
@role NatureExplorationBuffer
@capability Spatial.Traversable
@capability Spatial.WorkerAccessible
@capability Spatial.HarvestResourceWorkArea
@capacity WorkArea
@capacity ResourceNode
@capacity Actor
@capacity Tool
@connector HomeInput
@connector HomeReturn
@grammar nature:숲 빈터·고사목
@grammar nature:산길·바위 길목

## 존재 이유

기존 상향식 재고 v1에서 이관한 자연 탐색·완충 공간 설계 지식이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 ResourceNode별 점유와 재생성 해제는 Simulation Core가 검증한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
