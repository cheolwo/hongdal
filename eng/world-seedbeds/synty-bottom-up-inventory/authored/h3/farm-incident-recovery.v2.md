# 농장 사건 격리·회복 경관

@spatial-knowledge h3-candidate:farm-incident-recovery
@hierarchy H3
@state ExploratoryInventory
@required-h2 h2-candidate:farm-incident-containment
@required-h2 h2-candidate:farm-loss-restoration-handoff
@optional-h2 h2-candidate:farm-processing-shipping
@optional-h2 h2-candidate:forest-edge-farm
@connector ProductionIncidentInput
@connector RecoveredProductionOutput
@connector NatureRestorationHandoff
@connector FarmExternalGate

## 존재 이유

생산·수확 흐름에서 사건을 점검·격리하고 손실 회복과 자연권 복원 물자 인계까지 이어지는 Farm 생존 경관이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H3`
- 실제 지역 권위: 없음

## 미해결

- 실제 AreaSet과 공공데이터 근거를 적용하기 전까지 조립 후보로 유지한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
