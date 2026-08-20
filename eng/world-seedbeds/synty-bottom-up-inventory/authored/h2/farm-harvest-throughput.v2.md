# 농장 집중 수확·집하 블록

@spatial-knowledge h2-candidate:farm-harvest-throughput
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:farm-production
@required-h1 h1-stock:farm-harvest-staging
@required-h1 h1-stock:farm-work-yard
@optional-h1 h1-stock:farm-worker-waiting
@connector ProductionInput
@connector HarvestFlow
@connector ProcessingOutput

## 존재 이유

생산구획에서 수확물 임시 적치와 집하 작업마당으로 이어지는 집중 수확 흐름을 수용하는 Farm 단독 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
