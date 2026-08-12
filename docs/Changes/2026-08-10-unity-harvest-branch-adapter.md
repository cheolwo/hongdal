# Unity 수확 판로 공통 Branch Adapter

- 날짜: 2026-08-10
- 범위: `BRANCH-ADAPTER-1`
- 화면: 없음 — Data/adapter/test만 변경

## 변경

- 기존 조합 출하·온라인 직판·수출대행 준비에 정착지 비축 보관을 추가해 서버와 같은 네 판로 code를 사용한다.
- 각 판로를 canonical workflow code와 일대일로 연결한다.
- 기존 `HarvestDispositionDecision`과 `HarvestLot`의 stable ID, revision, 상품, 수량, 단위와 source lineage로 서버 Preview 입력 envelope를 구성한다.
- 서버와 같은 형식의 deterministic 후보 Task ID·type·input Lot·output candidate를 제공한다.

## 권위 경계

- adapter는 비용, 노동, 기간, 시설, 예상 수입, 감모, FoodSecurityDays와 Effect를 계산하지 않는다.
- envelope는 서버 Preview와 명시적 Confirm이 필요하고 자체적으로 정착지 원장, Cargo, 판매 또는 외부 실행을 변경하지 않는다고 표시한다.
- 기존 조합·직판 lifecycle은 후속 workflow의 fixture/Presentation 명세로 보존한다.
- 기존 3버튼 실험 카드는 아직 비축 보관 action을 노출하지 않으며, 4개 action surface는 `SETTLEMENT-INTERACTION-0`에서 WorldShell에 연결한다.

## 검증

- `Ssalddel.Unity.Tests`: 판로 선택·adapter 집중 15/15 통과
- `Ssalddel.Unity.Tests`: 전체 328/328 통과
- Unity EditMode `HarvestDispositionBranchAdapterTests`: 6/6 통과
- Unity 기본 EditMode assembly 전체: 55/55 통과
- `SimulationHarvestDispositionImpactTests`: 23/23 통과
- Unity recompile: 오류 0건
- scoped Fast: build와 `git diff --check` 통과
- scoped Task: build 통과, 전체 test는 기존 비관련 7건 실패로 4,482/4,489 통과
- Scene·prefab·material·Game View 변경 없음
