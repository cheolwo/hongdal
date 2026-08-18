# 생활권 자연권 지원 인계점

@spatial-knowledge h1-stock:town-nature-relief
@hierarchy H1
@state ExploratoryInventory
@gameplay NatureReliefCollection
@gameplay RestorationSupplyHandoff
@gameplay CommunitySupport
@role TownNatureReliefHandoffArea
@capability Spatial.CustomerAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.CollectionWorkArea
@predecessor h1-stock:town-recall-service
@predecessor h1-stock:town-neighborhood-service
@successor h1-stock:nature-safe-recovery-camp
@successor h1-stock:nature-restoration-site
@connector CommunityInput
@connector NatureReliefOutput
@connector ReturnReportInput
@grammar town:생활 공공광장
@grammar town:버스 정류장·보행 쉼터

## 존재 이유

시장 사건을 해결한 뒤 Nature 생활권 복구에 필요한 식량·약품·정화 자재를 주민과 작업자가 모아 넘기는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
