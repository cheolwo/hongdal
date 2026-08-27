# PlayableLoop 설계 문서 관리

이 폴더는 플레이어 약속, 반복 이유, 선택·대가·결과·귀환과 완료 조건처럼 **상태와 무관한 설계 기준**을 폐루프별로 보관한다.

- 현재 단계·차단·다음 WI: `eng/execution-ledgers/playable-loops.json`
- Codex Goal 순서·WIP: `eng/execution-ledgers/codex-playable-loop-goals.json`
- 사람이 읽는 현재 완결 상태판: `docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md`
- 새 작업 작성 틀: `docs/ProjectOverview/templates/게임개발작업단위템플릿.md`

같은 현재 상태를 개별 설계 문서에 복사하지 않는다. 신규 또는 의미가 크게 바뀐 폐루프는 대장의 `designDocumentationPolicy.requiredDetailedDesignLoopStableIds`에 등록하고 다음을 검증한다.

1. `designDocumentRef`가 실제 파일을 가리킨다.
2. `sourcePlanningDocumentRefs`가 통합 기획 근거를 가리킨다.
3. 선행 폐루프는 실행 의존이며 영역 간 자동 종속을 만들지 않는다.
4. 모든 PlayableUnit의 수직 목표는 E7이다. 한 폐루프의 반복 안정은 E8 캠페인, NPC 연속성을 포함한 둘 이상의 Core 조화와 사람 승인은 E9 캠페인에서 판정한다.
5. `Validated` 또는 Goal `Completed`는 유효한 EvidencePackage와 반환 상태 없이는 사용할 수 없다.

## 상세 설계

- [Nature 거점 성찰·다음 원정 준비](nature-base-reflection.v1.md)

기존 폐루프의 상세 문서는 의미 변경 작업을 열 때 점진적으로 이 폴더로 옮긴다. 이전 Architecture 문서는 출처 기획으로 보존하며 조용히 삭제하거나 최신 상태 원장으로 취급하지 않는다.
