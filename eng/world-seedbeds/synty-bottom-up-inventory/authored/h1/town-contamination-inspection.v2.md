# 생활권 재고 오염 점검 공간

@spatial-knowledge h1-stock:town-contamination-inspection
@hierarchy H1
@state ExploratoryInventory
@gameplay MarketContaminationInspection
@gameplay StockSafetyAssessment
@gameplay SaleHoldDecision
@role TownContaminationInspectionArea
@capability Spatial.WorkerAccessible
@capability Spatial.CargoAccessible
@capability Spatial.InspectionWorkArea
@predecessor h1-stock:town-market-receiving
@predecessor h1-stock:town-market-display
@successor h1-stock:town-contamination-quarantine
@successor h1-stock:town-market-display
@connector StockInput
@connector SafeDisplayOutput
@connector QuarantineOutput
@grammar town:읍내 상점 전면
@grammar city:도심 마트 앞마당

## 존재 이유

마트 입고 또는 진열 재고의 오염 의심 물량을 정상 판매 흐름에 넣기 전에 확인하는 후방 점검 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
