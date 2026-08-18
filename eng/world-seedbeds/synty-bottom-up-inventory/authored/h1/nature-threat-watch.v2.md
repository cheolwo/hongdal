# 자연권 위협 관찰 초소

@spatial-knowledge h1-stock:nature-threat-watch
@hierarchy H1
@state ExploratoryInventory
@gameplay RegionalThreatObservation
@gameplay NatureRouteWarning
@gameplay EncounterForecast
@role NatureThreatWatchArea
@capability Spatial.Traversable
@capability Spatial.ObservationArea
@capability Spatial.ThreatMonitoringArea
@predecessor h1-stock:nature-trailhead
@predecessor h1-stock:nature-lookout
@successor h1-stock:nature-emergency-retreat
@successor h1-stock:nature-safe-recovery-camp
@connector SafeCoreInput
@connector ThreatBandOutput
@connector WarningRelay
@grammar nature:산 능선
@grammar nature:고지대 노출지

## 존재 이유

전문 경관에서 번진 위험의 방향과 강도를 관찰하고 안전 생활핵에 경고를 전달하는 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
