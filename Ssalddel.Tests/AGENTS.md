# Ssalddel 테스트 작업 지침

이 폴더에서는 저장소 루트 `AGENTS.md`와 함께 아래 검증 원칙을 적용한다.

- 변경 위험에 맞는 최소 테스트를 추가하고, 관련 production project와 함께 build한다.
- 반복 작업에서는 전체 suite보다 변경된 test class 또는 관련 namespace의 `FullyQualifiedName` filter를 우선한다.
- `powershell -NoProfile -ExecutionPolicy Bypass -File eng/validate-changes.ps1 -Level Fast`는 직접 영향 project와 발견된 관련 test를 검증한다.
- 작업 완료 전에는 같은 명령의 `-Level Task`, release 직전이나 명시 요청 시에만 `-Level Release`를 사용한다.
- unrelated 변경이 많은 작업 트리에서는 `-Paths`로 이번 작업 파일만 넘긴다.
- `--no-build`와 `--no-restore`는 선행 build/restore가 확인된 경우에만 사용한다.
- 상세 console output은 대화에 복사하지 않고 TRX와 `artifacts/local/validation/` log를 보존한다. 실패 보고에는 실패 test 이름과 첫 원인만 포함한다.
- 상태 전이와 동기화 테스트는 원장 저장, RDB 투영, Event 멱등 재처리, 권한, 다른 client의 재조회를 함께 다룬다.
- test filter가 좁아 소비자 compile 호환성을 놓칠 수 있으면 관련 버전 `.slnx` build를 추가한다.
