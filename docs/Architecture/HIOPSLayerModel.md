# 이전 HIOPS 층위 문서

`HIOPS`와 `하위 OS`는 초기 설계 이력과 기존 설정·식별자에 남아 있는 호환 용어다. 새 코드와 문서의 기술 책임 이름으로는 사용하지 않는다.

현재 기준은 [업무 실행 책임 모델](BusinessWorkflowResponsibilityModel.md)이다.

- `원장` → `Business Case`
- `원장 블록` → `Case Section`
- `하위 OS` → 실제 책임에 따라 `ProcessManager`, `WorkflowCoordinator`, `Scheduler`, `BackgroundService`
- `엔진` → 영속 상태를 바꾸지 않는 순수 계산 경계에서만 `Engine`
- 실제 상태 변경 → `API`, `UseCase`, `Command`, `ApplicationService`

기존 링크를 깨지 않기 위해 이 파일 경로는 호환 문서로 유지한다.
