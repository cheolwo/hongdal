# 농장 자연권 복구 자재 인계 공간

@spatial-knowledge h1-stock:farm-restoration-supply
@hierarchy H1
@state ExploratoryInventory
@gameplay RestorationSupplyHandoff
@gameplay RecoveredMaterialTransfer
@gameplay NatureRouteSupport
@role FarmRestorationSupplyHandoffArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.LoadingWorkArea
@predecessor h1-stock:farm-incident-quarantine
@predecessor h1-stock:farm-loss-recovery
@successor h1-stock:nature-restoration-site
@successor h1-stock:nature-farm-edge
@connector FarmRecoveryInput
@connector NatureRestorationOutput
@connector ReturnMaterialInput
@grammar farm:농산물 집하·직판장
@grammar nature:숲 가장자리

## 존재 이유

Farm 사건을 해결한 뒤 자연권 정화·복구에 필요한 자재와 회수물을 Nature 경계로 인계하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
