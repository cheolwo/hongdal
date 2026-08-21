# Nature AreaSet 구성 패턴

Nature는 플레이어가 계속 돌아오는 심리 영역이다. 기준안은 생활핵에서 출발해 위협을 관찰하고 탐색길을 지나 다시 생활핵으로 돌아오는 폐루프를 만든다.

## 기준안 — 숲·하천 생활핵 순환형

```text
생활핵·조우·방어
  ↓
위협·회복
  ↓
탐색길·대피망
  ↓
생활핵 복귀
```

| 역할 슬롯 | 선택 H3 | 플레이 의미 |
| --- | --- | --- |
| HomeEncounterDefense | `nature-home-encounter-defense` | 시작·안전·방어·복귀 |
| ThreatRecovery | `nature-threat-recovery` | 위협 대응과 회복 선택 |
| TrailNetwork | `nature-trail-network` | 다리·여울·숲길을 통한 탐색 |

하천과 식생 수량은 Synty 표현층이다. 구성 권위는 시작점, H3 순서, 연결 지점, 선택과 복귀 동선이다.

## 변형안 — 위협 고조·대피 우회형

생활핵에서 안전한 직행로 대신 탐색·대피망을 먼저 거쳐 위협·회복 구역에 접근한다. 이 패턴은 Runtime 위협 상태가 아니라 서버가 선택할 수 있는 불변 설계 후보다.

## 관련 WI

`WI-WORLD-05`, `WI-WORLD-07`, `WI-NATURE-01~04`를 수용한다. Unity 표현은 이 WI의 성공이나 공간 선택을 확정하지 않는다.
