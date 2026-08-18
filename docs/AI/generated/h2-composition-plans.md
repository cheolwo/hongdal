# P1~P3 H2 조립안

이 문서는 H1을 상대 위치·관계·연결구로 조립한 위치 독립 H2 설계안이다. 실제 도로·경계·AreaSet·경관 그래프 권위가 아니다.

## 농장 사건 점검·격리 블록

- 후보: `h2-candidate:farm-incident-containment`
- 위상: `ModifiedGrid`
- 기준 크기: `240m × 180m`
- 설계 상태: `ReadyForPlanningReview`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:farm-exposure-inspection` | -66 / 8 | 0° |
| `h1-stock:farm-incident-quarantine` | 12 / 34 | 20° |
| `h1-stock:farm-weather-protection` | 48 / -46 | 180° |

연결구: `HarvestInput`, `SafeCargoOutput`, `RecoveryOutput`

## 농장 손실 회복·복원 인계 블록

- 후보: `h2-candidate:farm-loss-restoration-handoff`
- 위상: `Linear`
- 기준 크기: `260m × 180m`
- 설계 상태: `ReadyForPlanningReview`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:farm-incident-quarantine` | -78 / 24 | 10° |
| `h1-stock:farm-loss-recovery` | -4 / 0 | 0° |
| `h1-stock:farm-restoration-supply` | 82 / -24 | 345° |

연결구: `IncidentInput`, `RecoveredCargoOutput`, `NatureRestorationOutput`

## 자연 복원·안전 회복 블록

- 후보: `h2-candidate:nature-restoration-recovery`
- 위상: `Organic`
- 기준 크기: `220m × 180m`
- 설계 상태: `ReadyForPlanningReview`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:nature-restoration-site` | -28 / 30 | 10° |
| `h1-stock:nature-safe-recovery-camp` | 64 / -34 | 190° |

연결구: `IncidentRouteInput`, `SafeCoreOutput`, `RestoredRouteOutput`

## 자연 위협 추적·대피 블록

- 후보: `h2-candidate:nature-threat-response`
- 위상: `ContourAdaptive`
- 기준 크기: `240m × 200m`
- 설계 상태: `ReadyForPlanningReview`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:nature-threat-watch` | 0 / 0 | 20° |
| `h1-stock:nature-incident-trace` | 62 / 48 | 35° |
| `h1-stock:nature-emergency-retreat` | -72 / -58 | 215° |

연결구: `SafeCoreInput`, `ThreatBandContinuation`, `EmergencyExit`

## 생활권 오염 점검·정화 블록

- 후보: `h2-candidate:town-contamination-control`
- 위상: `ModifiedGrid`
- 기준 크기: `240m × 180m`
- 설계 상태: `ReadyForPlanningReview`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:town-contamination-inspection` | -68 / 26 | 0° |
| `h1-stock:town-contamination-quarantine` | 4 / 36 | 10° |
| `h1-stock:town-cleanup-transfer` | 70 / -38 | 315° |

연결구: `MarketStockInput`, `SafeDisplayOutput`, `ServiceVehicleOutput`

## 생활권 회수 안내·자연권 구호 블록

- 후보: `h2-candidate:town-recall-relief`
- 위상: `Cluster`
- 기준 크기: `240m × 190m`
- 설계 상태: `ReadyForPlanningReview`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:town-recall-service` | -66 / 8 | 0° |
| `h1-stock:town-neighborhood-service` | 0 / 36 | 15° |
| `h1-stock:town-nature-relief` | 72 / -32 | 330° |

연결구: `ResidentInput`, `ReturnOutput`, `NatureReliefOutput`
