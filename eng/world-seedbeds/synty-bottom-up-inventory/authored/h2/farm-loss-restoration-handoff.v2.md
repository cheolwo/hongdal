# 농장 손실 회복·복원 인계 블록

@spatial-knowledge h2-candidate:farm-loss-restoration-handoff
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:farm-incident-quarantine
@required-h1 h1-stock:farm-loss-recovery
@required-h1 h1-stock:farm-restoration-supply
@optional-h1 h1-stock:farm-work-yard
@optional-h1 h1-stock:nature-farm-edge
@connector IncidentInput
@connector RecoveredCargoOutput
@connector NatureRestorationOutput

## 존재 이유

격리된 손실을 회복 처리하고 자연권 복원 물자를 인계하는 농장과 자연권 사이의 회복 레시피다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
