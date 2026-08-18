# 생활권 회수·안내 창구

@spatial-knowledge h1-stock:town-recall-service
@hierarchy H1
@state ExploratoryInventory
@gameplay ResidentRecallNotice
@gameplay ContaminatedReturn
@gameplay ReplacementPickup
@role TownRecallServiceArea
@capability Spatial.CustomerAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.ReturnsWorkArea
@capability Spatial.InformationArea
@predecessor h1-stock:town-contamination-quarantine
@predecessor h1-stock:town-resident-pickup
@successor h1-stock:town-returns
@successor h1-stock:town-cleanup-transfer
@connector ResidentInput
@connector ReturnOutput
@connector ReplacementOutput
@grammar town:읍내 상점 전면
@grammar town:생활 공공광장

## 존재 이유

주민에게 오염 재고 회수와 대체 수령 절차를 알리고 반품 물량을 접수하는 생활 서비스 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
