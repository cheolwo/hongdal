# 자연 위협 추적·대피 블록

@spatial-knowledge h2-candidate:nature-threat-response
@hierarchy H2
@state CandidateForReview
@required-h1 h1-stock:nature-threat-watch
@required-h1 h1-stock:nature-incident-trace
@required-h1 h1-stock:nature-emergency-retreat
@optional-h1 h1-stock:nature-lookout
@optional-h1 h1-stock:nature-shelter
@connector SafeCoreInput
@connector ThreatBandContinuation
@connector EmergencyExit
@connector RecoveryHandoff

## 존재 이유

위협 감시에서 사건 흔적 추적과 긴급 후퇴까지 이어지는 자연권 최우선 대응 레시피다.

## 설계 상태

- 재고 상태: `CandidateForReview`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- E2에서 위협 관찰 결과가 후퇴·복원 분기로 전달되는 상태 계약을 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
