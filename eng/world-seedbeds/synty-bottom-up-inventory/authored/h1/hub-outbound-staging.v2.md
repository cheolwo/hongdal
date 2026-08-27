# Hub 피킹·출고 준비 공간

@spatial-knowledge h1-stock:hub-outbound-staging
@hierarchy H1
@state ApprovedReference
@wi WI-HUB-03
@wi WI-HUB-04
@wi WI-HUB-05
@role HubPickingArea
@role HubOutboundStagingArea
@capability Spatial.CargoAccessible
@capability Spatial.OutboundStagingArea
@capability Spatial.PickingWorkArea
@capability Spatial.Storage
@capability Spatial.WorkerAccessible
@capacity WorkArea
@connector cargo-handoff
@grammar city:화물 대기 야드
@grammar city:상하차 Dock

## 존재 이유

기존 상향식 재고 v1에서 이관한 Hub 피킹·출고 준비 공간 설계 지식이다.

## 설계 상태

- 재고 상태: `ApprovedReference`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 승인 근거

`wi-spatial-seedbed:hub-outbound-staging.v1`이 피킹과 차량 상차 전 출고 대기 공간을 분리하고, `WI-HUB-03~05`에 필요한 작업영역과 화물 인계 관계를 제공한다.

이 문서는 승인된 공간 의미를 기록하지만 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
