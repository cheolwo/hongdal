# 실시간 정착지 경제·영지 경영·분쟁 Simulation 우선순위 재정렬

## 변경

- 첨부된 영지 경영·전쟁 Simulation 방향을 현재 구현 상태와 운영 제품 경계에 맞춰 다시 설계했다.
- 기존 FARM-3·HARVEST-CHOICE-1·COOP-1·DIRECT-1·CARGO-1·JOURNEY-1·Hub·Urban Market 기반은 폐기하지 않는다.
- 다음 구현을 `EXPORT-1`에서 `SIM-WORLD-0 → DECISION-WORK-0 → SAVE-REPLAY-0 → SETTLEMENT-CORE-1`로 재정렬했다.
- 첫 필수 playable을 군단·공성전이 아니라 감자 300kg 판로가 재정·노동·시장·비축·FoodSecurityDays를 바꾸는 정착지 경제 폐루프로 정의했다.
- 영지·군단·침공은 운영 제품과 DB를 공유하지 않는 별도 Simulation World로 고정했다.
- 온라인·수출·영주·성 같은 시대별 표현은 scenario profile이 소유하고 core domain은 시대 중립적으로 유지한다.

## 기준 문서

- [Unity 실시간 정착지 경제·영지 경영·분쟁 Simulation 재정렬 제안서](../Architecture/UnityRealtimeTerritoryManagementConflictSimulationProposal.md)
- [D-038 정착지 경영·분쟁 Simulation은 공통 World와 경제 인과를 먼저 닫는다](../AI/DECISIONS.md#d-038-정착지-경영분쟁-simulation은-공통-world와-경제-인과를-먼저-닫는다)

## 화면

화면 없음. 이번 변경은 제안서·결정·현재 작업 snapshot의 재작성만 수행했으며 코드, Unity Scene과 Game View를 변경하지 않았다.

## 검증

- docs Fast validation
- `git diff --check`
- 신규·변경 문서 상대 링크 확인
