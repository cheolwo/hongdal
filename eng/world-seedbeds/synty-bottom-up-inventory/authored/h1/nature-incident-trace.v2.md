# 자연권 사건 흔적 조사 구역

@spatial-knowledge h1-stock:nature-incident-trace
@wi WI-NATURE-01
@hierarchy H1
@state CandidateForReview
@gameplay IncidentTraceInvestigation
@gameplay CauseIdentification
@gameplay ThreatTracking
@role NatureIncidentTraceArea
@capability Spatial.Traversable
@capability Spatial.InvestigationArea
@capability Spatial.ThreatMonitoringArea
@capacity InvestigationWorkArea
@capacity IncidentTraceTarget
@predecessor h1-stock:nature-threat-watch
@successor h1-stock:nature-restoration-site
@successor h1-stock:nature-emergency-retreat
@connector ObservationInput
@connector CauseRouteOutput
@connector RetreatOutput
@grammar nature:숲 빈터·고사목
@grammar nature:산길·바위 길목

## 존재 이유

Farm·Town·City/Hub 사건이 자연권에 남긴 오염·파손·이동 흔적을 조사하는 위치 독립 공간 후보다.

## 설계 상태

- 재고 상태: `CandidateForReview`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- E2에서 관찰 WI 안의 조사 단계와 원인 계보 판정 단위를 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
