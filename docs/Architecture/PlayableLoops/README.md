# PlayableLoop 설계 문서 관리

이 폴더는 플레이어 약속, 반복 이유, 선택·대가·결과·귀환과 완료 조건처럼 **상태와 무관한 설계 기준**을 폐루프별로 보관한다.

- 현재 단계·차단·다음 WI: `eng/execution-ledgers/playable-loops.json`
- Codex Goal 순서·WIP: `eng/execution-ledgers/codex-playable-loop-goals.json`
- 사람이 읽는 현재 완결 상태판: `docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md`
- 새 작업 작성 틀: `docs/ProjectOverview/templates/게임개발작업단위템플릿.md`
- 새 주제 기획 틀: `docs/ProjectOverview/templates/PlayableLoop주제기획서템플릿.md`
- 문답 정밀화 기록 틀: `docs/ProjectOverview/templates/PlayableLoop문답정밀화기록템플릿.md`
- 전문 심화 연구 틀: `docs/ProjectOverview/templates/PlayableLoop전문심화연구템플릿.md`
- 효과음·환경음·배경음·음성 요구 재고: [PlayableLoop 오디오 요구사항 대장](오디오요구사항대장.md)
- 현재 승인 전달: [기존 WI 세계 발현 E5 계획](../기존WI세계발현E5개발계획.md) — 지식 습득·방문자 응대의 실행/저장/실제 배치와 [Farm 경작 E5](Farm경작세계발현E5.md)를 병행한다. 계획 승인은 증거 승격이 아니다.

같은 현재 상태를 개별 설계 문서에 복사하지 않는다. 신규 또는 의미가 크게 바뀐 폐루프는 대장의 `designDocumentationPolicy.requiredDetailedDesignLoopStableIds`에 등록하고 다음을 검증한다.

1. `designDocumentRef`가 실제 파일을 가리킨다.
2. `sourcePlanningDocumentRefs`가 통합 기획 근거를 가리킨다.
3. 선행 폐루프는 실행 의존이며 영역 간 자동 종속을 만들지 않는다.
4. 모든 PlayableUnit의 수직 목표는 E7이다. 한 폐루프의 반복 안정은 E8 캠페인, NPC 연속성을 포함한 둘 이상의 Core 조화와 사람 승인은 E9 캠페인에서 판정한다.
5. `Validated` 또는 Goal `Completed`는 유효한 EvidencePackage와 반환 상태 없이는 사용할 수 없다.
6. 신규 또는 의미 변경 revision은 건물·공간·배치·애니메이션 연구 필요성을 판정하고 모든 `Required` 연구를 `Accepted`로 재결속한 뒤 승인한다.

전문 심화 연구는 [PlayableLoop 전문 심화 연구 분기·재결속 체계](../PlayableLoop전문심화연구분기재결속체계.md)를 따른다. 공통 기준 문서는 여러 폐루프가 공유하는 문법을, 폐루프 적용 연구는 해당 WI의 측정값·자산·검증 기준을 소유한다. 연구 승인만으로 Logic·Presentation E가 오르지 않는다.

짧은 정차·대기 중의 기획은 [PlayableLoop 문답 정밀화 체계](../PlayableLoop문답정밀화체계.md)를 따른다. 한 번에 질문 하나씩 답하고 확인된 해석만 문답 기록에 남긴 뒤 기획서로 합성한다. 원문 대화나 기억만으로 개발 스레드에 인계하지 않는다.

## 상세 설계

- [Nature 거점 성찰·다음 원정 준비](nature-base-reflection.v1.md)

## 진행 중 문답 정밀화

- [Q-001~268 전체 문답 정리 상태판](PlanningSessions/문답정리상태판.md)
- [문답 기록 routing과 주제별 색인](PlanningSessions/README.md)
- [Nature 거점·수면·날씨·방어](PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md)
- [플레이어 내면·명상·계획](PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md)
- [Nature 자원·LandUse·건설](PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md)
- [약초·Recipe·조합 제작](PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md)
- [저장·Load·재진입](PlanningSessions/저장재진입/save-load-runtime.inquiry.r1.md)
- [기존 Nature Night Day2 통합 기록](PlanningSessions/nature-night-day2.inquiry.r1.md)은 동결 호환 아카이브다.

기존 폐루프의 상세 문서는 의미 변경 작업을 열 때 점진적으로 이 폴더로 옮긴다. 이전 Architecture 문서는 출처 기획으로 보존하며 조용히 삭제하거나 최신 상태 원장으로 취급하지 않는다.
