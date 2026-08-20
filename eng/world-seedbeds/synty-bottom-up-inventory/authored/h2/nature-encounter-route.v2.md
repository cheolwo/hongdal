# 자연 몬스터 조우·이탈 블록

@spatial-knowledge h2-candidate:nature-encounter-route
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:nature-threat-watch
@required-h1 h1-stock:nature-incident-trace
@required-h1 h1-stock:nature-emergency-retreat
@optional-h1 h1-stock:nature-lookout
@connector EncounterInput
@connector ThreatContinuation
@connector RetreatOutput

## 존재 이유

위협 관찰과 사건 흔적 조사, 긴급 후퇴를 하나의 접근·이탈 경로로 묶는 Nature 단독 조우 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
