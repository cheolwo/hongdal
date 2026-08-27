# 실제 E5 4영역 공간·WI 결속

이 문서는 이론 H2·H3를 작성 Scenario 좌표의 실제 AreaSet·Graph·Network에 결정적으로 결속한 결과다.

- AreaSet: `4`
- 내부 Graph: `16` · Network 경로 Graph: `3`
- Network 관계: `8`
- 이론 보류 Graph: `1` (정책 승격 전 실제 E5에서 제외)
- AreaSet 구성 패턴: `simulation-world-area-set-composition-plans.r1`
- WI: 직접 `42` · 문맥 `6` · 비공간 `9` · E5 배치 대기 `7`

| 영역 | 구성 패턴 | 실제 AreaSet | Graph | 적재 정책 |
| --- | --- | --- | ---: | --- |
| NatureHome | `NATURE-ASET-COMP-01` | `area-set:sim:pyeongchang:nature-home.v1` | 3 | `Persistent` |
| Farm | `FARM-ASET-COMP-01` | `area-set:sim:pyeongchang:farm-production.v1` | 4 | `OnDemand` |
| CityHub | `CITYHUB-ASET-COMP-01` | `area-set:sim:pyeongchang:logistics-hub.v1` | 4 | `OnDemand` |
| Town | `TOWN-ASET-COMP-01` | `area-set:sim:pyeongchang:town-market.v1` | 5 | `OnDemand` |

작성 Scenario 근거는 E5 공간 결속이며 공공데이터 E6나 실제 서버·Unity E7 증거가 아니다.
