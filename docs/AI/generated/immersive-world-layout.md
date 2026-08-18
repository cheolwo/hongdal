# Nature 중심 3인칭 몰입 세계 대장

> 이 문서는 `immersive-world-layout.v1.json`에서 결정적으로 생성한다. 후보 공간은 공식 H·E·AreaSet 권위를 만들지 않는다.

- 실행 진입점: `SimulationWorldShell`
- 기본 시점: `TacticalThirdPerson`
- 배경 Simulation 지속: `True`
- Apocalypse: `WaitingForApocalypseAssetPack` · 대체 자산 `Forbidden`

## 경관 인스턴스

| 인스턴스 | 주축 팩 | 구성 | WI | 현장 방문 |
| --- | --- | --- | ---: | --- |
| `immersive-instance:nature-home` Nature 생활·탐색 세계 | `Nature` | Nature 90%, NetworkTransition 10% | 2 | `False` |
| `immersive-instance:farm` Farm 생산·출하 경관 | `Farm` | Farm 70%, Nature 20%, NetworkTransition 10% | 4 | `True` |
| `immersive-instance:town` Town 생활·시장 경관 | `Town` | Town 70%, Nature 20%, NetworkTransition 10% | 6 | `True` |
| `immersive-instance:city-hub` City 물류 Hub 경관 | `City` | City 70%, Nature 20%, NetworkTransition 10% | 4 | `True` |

## Nature 위험 단계

| 단계 | 의미 | 몬스터 표현 | 연결 지점 |
| --- | --- | --- | --- |
| `SafeCore` | 안전 생활 중심부 | `False` |  |
| `WarningBand` | 위협 징후 외곽 | `False` | WarningSignal, RetreatRoute |
| `EncounterBand` | 조우 위험 외곽 | `True` | ThreatSpawn, CombatClearing, RetreatRoute |

실제 몬스터 Prefab 연결은 `POLYGON Apocalypse` 설치·감사 전까지 금지한다. 현재 Generic 해골로 자동 대체하지 않는다.
