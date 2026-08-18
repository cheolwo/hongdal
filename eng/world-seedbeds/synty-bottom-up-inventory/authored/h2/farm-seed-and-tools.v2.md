# 종자·농기구 준비 블록

@spatial-knowledge h2-candidate:farm-seed-and-tools
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:farm-tool-storage
@required-h1 h1-stock:farm-seed-preparation
@optional-h1 h1-stock:farm-worker-waiting
@connector ProductionOutput
@connector WorkerAccess
@evidence RoadNetwork
@evidence BlockBoundary

## 존재 이유

농기구 보관과 종자 준비, 작업자 대기를 생산구획 앞에서 묶는 작업 시작 레시피다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
