# Ssalddel 문서 작업 지침

이 폴더에서는 저장소 루트 `AGENTS.md`와 함께 아래 문서 원칙을 적용한다.

- 제품 설명과 코드가 다르면 실제 route, contract, test, 실행 설정을 확인하고 차이를 명시한다.
- 같은 정책을 여러 문서에 복제하지 말고 기준 문서 하나와 링크를 유지한다.
- 게임 기획 문답은 개별 `PLAN-*` 문서의 분야·판본에 기록하고 `PLANNING.md`에는 현재 판본·상태·관계만 둔다. 일반 기획 선택을 새 `D-###`로 만들지 않으며, 잘못 기록된 기존 D는 삭제 대신 현행 기획을 가리키는 호환 이력으로 둔다.
- Unity·서버 통합 문서의 사람이 읽는 용어와 작업 보고는 [Unity 프로젝트 한국어 중심 용어·출력 지침](AI/UnityKoreanTerminologyGuide.md)을 따른다. 코드 식별자와 고유 기술명은 유지하고 프로젝트 개념은 한국어를 먼저 쓴다.
- 버전·기능 플래그·`Simulation`/`Operational` 경계를 현재 코드와 맞춘다.
- 공공 데이터에는 출처, 기준 시각, 단위, 통화, 지역, 갱신 주기와 제한을 함께 기록한다.
- 화면 변경 기록은 `docs/Changes/README.md`의 형식을 따르고 실제 PNG를 `docs/assets/changes/`에 둔다.
- 문서만 바뀌어도 link와 경로를 확인하고 `git diff --check`를 실행한다.
- 임시 조사 결과와 raw capture는 `artifacts/local/`에 두고 장기 보존할 결과만 문서 자산으로 이동한다.
