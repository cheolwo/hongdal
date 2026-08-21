# Farm AreaSet 구성 패턴

Farm은 생산, 후처리, 출하, 사고 복구가 같은 생활권에서 이어지는 업무 영역이다. 기준안은 플레이어가 공간을 걸으며 감자가 상품과 화물로 바뀌는 과정을 이해하도록 만든다.

## 기준안 — 고지대 생산·후처리형

```text
고지대 생산
  ↓
농가·작업자 준비·후처리
  ↓
사고 격리·복구
  ↓
계절 생산·출하
  ↓
생산 구역 복귀
```

| 역할 슬롯 | 선택 H3 | 플레이 의미 |
| --- | --- | --- |
| HighlandProduction | `highland-farm` | 재배·수확 |
| HomeProcessing | `farm-processing-campus` | 집하·세척·선별·포장 |
| IncidentRecovery | `farm-incident-recovery` | 격리·수리·생산 복귀 |
| SeasonalDispatch | `farm-seasonal-production-loop` | 임시 적치·상차·Farm Gate |

## 변형안 — 계절 출하 집중형

후처리와 출하 슬롯을 확대하고 사고 복구 슬롯을 우회 가능한 보조 구역으로 둔다. 같은 Farm 문법을 사용하되 수확기 물량 흐름을 우선한다.

## 관련 WI

`WI-FARM-01~06`, `WI-LOG-01~02`, `WI-WORLD-04`를 수용한다. H2 교체는 역할 슬롯과 연결 능력이 같은 후보만 허용한다.
