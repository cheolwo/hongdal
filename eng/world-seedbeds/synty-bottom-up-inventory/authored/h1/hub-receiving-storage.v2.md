# Hub 입고·검수·보관 공간

@spatial-knowledge h1-stock:hub-receiving-storage
@hierarchy H1
@state ApprovedReference
@wi WI-LOG-04
@wi WI-LOG-05
@wi WI-001
@wi WI-002
@role HubUnloadingArea
@role HubInspectionArea
@role HubStorageArea
@capability Spatial.CargoAccessible
@capability Spatial.InspectionWorkArea
@capability Spatial.LoadingWorkArea
@capability Spatial.Storage
@capability Spatial.UnloadingWorkArea
@capability Spatial.WorkerAccessible
@connector vehicle
@grammar city:물류 Station 진입부
@grammar city:상하차 Dock
@grammar city:화물 대기 야드

## 존재 이유

기존 상향식 재고 v1에서 이관한 Hub 입고·검수·보관 공간 설계 지식이다.

## 설계 상태

- 재고 상태: `ApprovedReference`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결


이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
