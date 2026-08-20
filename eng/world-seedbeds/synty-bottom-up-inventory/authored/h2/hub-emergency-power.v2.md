# Hub 비상 전력·보관 유지 블록

@spatial-knowledge h2-candidate:hub-emergency-power
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:hub-cold-storage
@required-h1 h1-stock:hub-long-term-storage
@required-h1 h1-stock:hub-service-maintenance
@optional-h1 h1-stock:hub-temporary-staging
@connector StorageInput
@connector EmergencyService
@connector SafeReleaseOutput

## 존재 이유

저온 보관과 장기 보관, 시설 정비 공간을 묶어 전력 이상 시 보관 연속성을 관리하는 City 단독 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
