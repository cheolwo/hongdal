# 자연 임시 대피 공간

@spatial-knowledge h1-stock:nature-shelter
@hierarchy H1
@state IdeaInventory
@wi WI-WORLD-07
@gameplay TemporaryShelter
@gameplay WeatherWait
@gameplay Recovery
@role NatureTemporaryShelterArea
@capability Spatial.Traversable
@capability Spatial.RestArea
@capability Spatial.WeatherShelter
@predecessor h1-stock:nature-trailhead
@successor h1-stock:nature-exploration-buffer
@connector TrailInput
@connector TrailOutput
@grammar nature:숲 빈터·고사목
@grammar nature:산길·바위 길목

## 존재 이유

기상·위험·피로로 탐색을 계속하기 어려울 때 잠시 머무는 회복 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
