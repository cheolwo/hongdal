# Unity World Shell·정착지 Scene 우선순위 재정렬

## 변경 요약

| 항목 | 내용 |
| --- | --- |
| 변경일 | 2026-08-10 |
| 변경 유형 | Architecture·priority 문서 |
| 화면 변경 | 없음 |
| 시각 증거 | 없음 — Unity 코드·Scene·prefab·camera·UI를 변경하지 않음 |

현재 서버 권위 순서의 다음 Gate를 `SETTLEMENT-ECONOMY-1`로 유지하면서, 그 전에 `WORLD-SHELL-0 → SETTLEMENT-SCENE-0`을 하나의 읽기 전용 Presentation milestone으로 한정 수행하도록 순서를 재정렬했다.

## 결정한 순서

```text
WORLD-SHELL-0
  → SETTLEMENT-SCENE-0
  → SETTLEMENT-ECONOMY-1
  → WORLD-SETTLEMENT-NAV-0
  → BRANCH-ADAPTER-1
  → SETTLEMENT-VISUAL-BASE-0
  → SETTLEMENT-INTERACTION-0
```

Shell과 첫 정착지 blockout은 같은 `SimulationSession·WorldTick·WorldRevision·SettlementStableId`를 World Map과 Settlement Interior가 공유하는 구조만 증명한다. 경제 계산, 판로 Confirm, Tick 자동 진행, NPC·차량 완료와 군량·분쟁은 포함하지 않는다.

기존 공공데이터 `WorldBootstrapScene`은 공개지도 surface로 유지한다. Simulation용 `SimulationWorldShell`은 별도로 추가하며 첫 구현은 한 Scene 안의 `WorldMapRoot`와 `SettlementInteriorRoot` 전환으로 상태 보존을 검증한다.

## 문서 변경

- `docs/Architecture/UnityWorldShellSettlementSceneFoundationProposal.md`
- `docs/Architecture/UnityRealtimeTerritoryManagementConflictSimulationProposal.md`
- `docs/Architecture/UnityWorldImplementationPriority.md`
- `docs/AI/DECISIONS.md`의 D-042
- `docs/AI/CURRENT_WORK.md`
- `docs/Changes/README.md`

## 검증 경계

이번 변경은 문서에만 해당한다. Unity EditMode·PlayMode와 Game View는 실행하지 않았으며, 실제 `WORLD-SHELL-0` 구현 때 Scene hierarchy validator, 관련 EditMode test, Console 확인과 최종 Play Mode Game View를 별도로 남겨야 한다.

