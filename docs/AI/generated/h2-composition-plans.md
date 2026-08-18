# P1 H2 조립안

이 문서는 H1을 상대 위치·관계·연결구로 조립한 위치 독립 H2 설계안이다. 실제 도로·경계·AreaSet·경관 그래프 권위가 아니다.

## 자연 복원·안전 회복 블록

- 후보: `h2-candidate:nature-restoration-recovery`
- 위상: `Organic`
- 기준 크기: `220m × 180m`
- 근거 상태: `WaitingForRoadBoundaryEvidence`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:nature-restoration-site` | -28 / 30 | 10° |
| `h1-stock:nature-safe-recovery-camp` | 64 / -34 | 190° |

연결구: `IncidentRouteInput`, `SafeCoreOutput`, `RestoredRouteOutput`

## 자연 위협 추적·대피 블록

- 후보: `h2-candidate:nature-threat-response`
- 위상: `ContourAdaptive`
- 기준 크기: `240m × 200m`
- 근거 상태: `WaitingForRoadBoundaryEvidence`

| H1 노드 | 로컬 X/Z | 회전 |
| --- | ---: | ---: |
| `h1-stock:nature-threat-watch` | 0 / 0 | 20° |
| `h1-stock:nature-incident-trace` | 62 / 48 | 35° |
| `h1-stock:nature-emergency-retreat` | -72 / -58 | 215° |

연결구: `SafeCoreInput`, `ThreatBandContinuation`, `EmergencyExit`
