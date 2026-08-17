# 2026-08-15 Simulation·Unity 리팩토링 전 상태 요약

이 문서는 `CURRENT_WORK.md`가 누적 일지처럼 커진 상태를 정리하면서 남긴 이전 작업군 색인이다. 세부 구현 이력과 당시 시험 수치는 Git 이력과 각 Architecture·Changes 문서를 기준으로 조회한다.

## 주요 작업군

- Simulation World: 행정·법정동, L0/L1/L2 타일, 공공 공간자료, 건물·사업장, 파생 DB와 Unity 공간 산출물
- Unity 통합 World: `SimulationWorldShell`, 1인칭·전술 3인칭, 시야 기반 타일 준비, Synty 표현 대장
- 농장 생존: 노동·위협·전투 박자·영웅 성과·전술 분대·카드 보상
- 팀 협력: 역할 카드, 팀 관전, 카메라 권한과 조작 권한 분리
- 경영 Simulation: 수확, 수출, 물류, 창고, 주문, 배달, 소비, 턴 마감과 Save/Replay
- 공공데이터·업무 UI: World UI Projection, 입고 검수 수직 단위, Figma·MAUI 의미를 Unity에 투영하는 규칙

## 정리 시점의 최신 기준

- 진행 중 기능: 전투 인스턴스를 `simulation-save.v1`에 포함하는 Save/Restore 수직 단위
- 보고된 코드 시험 기준선: Simulation 551개, Unity 공통 469개
- 제품 전체 시험: 별도 기준선 실패 7건이 있었으며 Simulation·Unity 결과와 분리해 보고
- 실제 Scene·Prefab·Game View 변경은 이번 구조 리팩토링 범위에 포함하지 않음
- commit·push·배포는 별도 사용자 요청이 있을 때만 수행

## 추적 방법

- 장기 권위 결정: [DECISIONS.md](../../DECISIONS.md)
- 현재 상태: [CURRENT_WORK.md](../../CURRENT_WORK.md)
- 병렬 전투 구조: [SimulationParallelManagementBattleInstances.md](../../../Architecture/SimulationParallelManagementBattleInstances.md)
- 개별 화면 변경: [Changes](../../../Changes/README.md)
