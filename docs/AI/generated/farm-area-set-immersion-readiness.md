# Farm AreaSet E6 정밀 몰입 판정

- AreaSet: area-set:sim:pyeongchang:farm-production.v1
- 공간 성숙도: E5Qualified
- 몰입 성숙도: ImmersionQualified
- 최신성: Current
- GIS 결속: NotApplied
- E7 시작 관문: Open
- 판정 해시: a27d751827c2a6d45b5dd2dd6be7689fcba30a224acbb8bcb105dbe01b933954

## H3 정밀 조사

- h3-candidate:farm-processing-campus - ImmersionQualified, questions 3, H2 5, H1 11
- h3-candidate:highland-farm - ImmersionQualified, questions 3, H2 3, H1 6
- h3-candidate:farm-seasonal-production-loop - ImmersionQualified, questions 3, H2 3, H1 6
- h3-candidate:farm-incident-recovery - ImmersionQualified, questions 3, H2 2, H1 5

## AreaSet 교차 H3 폐루프

- closure:farm:harvest-to-processing.r1 - Pass, HarvestReadyCropUnit to HarvestLot
- closure:farm:incident-to-production-recovery.r1 - Pass, FacilityRestricted to ProductionCapabilityRestored
- closure:farm:processing-to-shipping.r1 - Pass, HarvestLot to CargoDeparted

## 권위 경계

공공자료는 장소와 작업의 현실 문맥을 설명하는 근거다. 이 판정은 H5 좌표를 이동하거나 생산량·수익성·Simulation 규칙을 자동 변경하지 않는다. 라이브 Provider 호출, Unity Play Mode, Game View와 실제 E7 완료는 수행하지 않았다.
