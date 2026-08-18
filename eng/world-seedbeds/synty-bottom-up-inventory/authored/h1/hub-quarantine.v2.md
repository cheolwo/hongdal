# Hub 검역·격리 공간

@spatial-knowledge h1-stock:hub-quarantine
@hierarchy H1
@state IdeaInventory
@gameplay CargoQuarantine
@gameplay QualityHold
@gameplay ReleaseOrReject
@role HubQuarantineArea
@capability Spatial.CargoAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.ExclusiveOccupancy
@predecessor h1-stock:hub-receiving-storage
@successor h1-stock:hub-temporary-staging
@successor h1-stock:hub-returns
@connector InspectionHoldInput
@connector ReleaseOutput
@connector RejectOutput
@grammar city:화물 대기 야드
@grammar city:상하차 Dock

## 존재 이유

검수 중 이상이 발견된 화물을 일반 보관 흐름과 분리해 임시 격리하는 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
