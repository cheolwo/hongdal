# 자연권 긴급 후퇴 길목

@spatial-knowledge h1-stock:nature-emergency-retreat
@hierarchy H1
@state ExploratoryInventory
@gameplay EmergencyRetreat
@gameplay ThreatEvacuation
@gameplay SafeCoreReturn
@role NatureEmergencyRetreatArea
@capability Spatial.Traversable
@capability Spatial.EmergencyAccess
@capability Spatial.PlayerEscapeRoute
@predecessor h1-stock:nature-incident-trace
@predecessor h1-stock:nature-threat-watch
@successor h1-stock:nature-shelter
@successor h1-stock:nature-safe-recovery-camp
@connector ThreatBandInput
@connector BufferOutput
@connector SafeCoreOutput
@grammar nature:산길·바위 길목
@grammar nature:숲 가장자리

## 존재 이유

조우 위험대에서 경계 완충대와 안전 생활핵으로 돌아가는 단방향 우선 후퇴 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
