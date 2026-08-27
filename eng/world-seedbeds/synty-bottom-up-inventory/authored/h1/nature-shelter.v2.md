# 자연 임시 대피 공간

@spatial-knowledge h1-stock:nature-shelter
@hierarchy H1
@state IdeaInventory
@wi WI-WORLD-07
@wi WI-NATURE-07
@wi WI-NATURE-08
@wi WI-NATURE-09
@wi WI-NATURE-10
@wi WI-NATURE-13
@wi WI-NATURE-14
@wi WI-NATURE-15
@wi WI-CON-01
@gameplay TemporaryShelter
@gameplay WeatherWait
@gameplay Recovery
@role NatureTemporaryShelterArea
@capability Spatial.Traversable
@capability Spatial.RestArea
@capability Spatial.WeatherShelter
@capability Spatial.PlayerAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.BuildingSite
@capability Spatial.ShelterConstructionWorkArea
@capability Spatial.ShelterEntrance
@capability Spatial.ShelterInterior
@capability Spatial.ShelterStorage
@capability Spatial.StorageInteractionAnchor
@capability Spatial.ShelterSleep
@capability Spatial.SleepInteractionAnchor
@capability Spatial.DawnPlanChoice
@capability Spatial.DawnPlanChoiceAnchor
@capability Spatial.AreaBuildingFootprint
@capability Spatial.AreaBuildingPlacementAllowed
@capability Spatial.FootprintAvailable
@capability Spatial.CraftingWorkArea
@capability Spatial.ActiveWorkReservationContext
@capacity BuildingSite
@capacity WorkArea
@capacity Material
@capacity ShelterOccupancy
@capacity ContainerCapacity
@predecessor h1-stock:nature-trailhead
@successor h1-stock:nature-exploration-buffer
@connector TrailInput
@connector TrailOutput
@grammar nature:숲 빈터·고사목
@grammar nature:산길·바위 길목

## 존재 이유

기상·위험·피로로 탐색을 계속하기 어려울 때 머물며 획득 자원을 보관하고 안전하게 수면한 뒤 다음 확장 계획을 선택하는 생활 거점 공간이다. 보관량·이동 결과·수면 시간 배율·새벽 도달·계획 확정은 Simulation이 판정하고 H1은 보관·수면·새벽 계획 Anchor와 점유·용량 문맥만 제공한다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
