# 자연 전망·관찰 공간

@spatial-knowledge h1-stock:nature-lookout
@hierarchy H1
@state IdeaInventory
@wi WI-WORLD-05
@gameplay LandscapeObservation
@gameplay ThreatObservation
@role NatureLookoutArea
@capability Spatial.Traversable
@capability Spatial.ObservationArea
@predecessor h1-stock:nature-trailhead
@successor h1-stock:nature-exploration-buffer
@connector TrailInput
@connector TrailOutput
@grammar nature:산 능선
@grammar nature:고지대 노출지

## 존재 이유

경관과 위험·길목을 관찰하고 지역 발견을 확정하기 전 정보를 제공하는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
