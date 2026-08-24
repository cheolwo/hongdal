# 현재 작업 진입점

이 파일은 전달 묶음의 고정 번호를 위한 진입점이다. 최신 작업 snapshot의 단일 기준은 [`docs/AI/CURRENT_WORK.md`](../CURRENT_WORK.md)다. 이 파일에 별도 진행 기록을 복제하지 않는다.

## Codex 시작 순서

1. [게임 기획 최상위 트리](00_GAME_DESIGN_TREE.md)에서 플레이 목적과 코어·확장을 확인한다.
2. [WI 전체 그래프](04_WI_GAMEPLAY_GRAPH.md)에서 상태 전이와 선후행을 찾는다.
3. [H 현재 트리](03_H1_H5_CURRENT_TREE.md)에서 필요한 공간의 선택·실행 상태를 확인한다.
4. [E 정의](01_E1_E9_DEFINITION.md)에서 필요한 증거 관문을 고른다.
5. [G 관리 체계](02_G1_G4_MANAGEMENT.md)로 작업 종류를 분류한다.
6. [Simulation](05_SIMULATION_DOMAIN_MAP.md)과 [Unity](06_UNITY_CURRENT_STRUCTURE.md)에서 권위 소유자와 조립 위치를 찾는다.
7. [완료 원장](07_CURRENT_COMPLETION_LEDGER.md)과 [최신 작업 snapshot](../CURRENT_WORK.md)을 대조한다.
8. [게임 개발 업무 순서 기준](../../Architecture/게임개발업무순서기준.md)과 [작업 단위 템플릿](../../ProjectOverview/templates/게임개발작업단위템플릿.md)으로 현재 목표·범위·증거와 다음 판단을 정한다.
9. [확정 결정](09_DECISIONS.md)과 가까운 `AGENTS.md`를 확인한 뒤 좁은 vertical slice를 실행한다.

## 계획 문장 형식

새 작업은 가능하면 다음 한 문장으로 시작한다.

```text
[플레이 목적]을 위해 [WI]를 [H 공간]에서 실행하고,
[권위 소유자]의 상태를 [G 관리 체계]로 변경·검증하여
[E 단계]의 [구체적 증거]를 만든다.
```

예:

```text
Nature↔Farm 왕복 플레이를 위해 WI-NATURE-04와 WI-FARM-01을
safe-recovery-camp와 farm-production에서 실행하고,
서버 Session의 상태를 G1·G2로 연결·검증하여
E7의 실제 서버 응답, Save/Replay, Game View 증거를 만든다.
```
