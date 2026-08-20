# Farm 계절 생산·출하 순환 경관

@spatial-knowledge h3-candidate:farm-seasonal-production-loop
@hierarchy H3
@state ExploratoryInventory
@required-h2 h2-candidate:farm-irrigation-service
@required-h2 h2-candidate:farm-harvest-throughput
@required-h2 h2-candidate:farm-processing-shipping
@optional-h2 h2-candidate:farm-worker-support
@connector SeasonInput
@connector ProductionLoop
@connector FarmShippingGate

## 존재 이유

관수·재배 관리에서 집중 수확과 후처리·출하로 이어지는 계절 생산 흐름을 Farm 단독 블록으로 묶는다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H3`
- 실제 지역 권위: 없음

## 미해결

- 실제 AreaSet과 공공데이터 근거를 적용하기 전까지 조립 후보로 유지한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
