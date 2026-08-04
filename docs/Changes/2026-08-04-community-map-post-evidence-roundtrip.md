# 지도 관측·게시글 근거 재조회 완결

- 날짜: 2026-08-04
- 화면: `/community/home`, 커뮤니티 게시글 상세
- 범위: 공개 관측에서 질문 초안·게시글을 만들고 같은 stable ID와 게시 당시 version을 지도에서 다시 확인

## 변경 내용

- 지도 관측에서 질문 초안을 만들 때 observation stable ID, dataset, 국가, 레이어, snapshot revision과 source version을 하나의 구조화된 공개 근거로 묶습니다.
- 게시글에 영속되는 공개 근거에 source version과 지도 재조회 경로를 함께 저장합니다.
- 게시글 상세의 `지도 근거 다시 보기`는 연결 자료 상세 주소가 아니라 정확한 지도 deep link를 사용합니다.
- 새 `MapHref`가 없는 기존 게시글도 저장된 공개 근거 필드로 같은 지도 deep link를 재구성합니다.
- 지도 deep link는 `snapshot`과 `sourceVersion`을 받아 현재 관측과 비교합니다.
- source version이 같으면 게시 당시 근거가 유지됨을, source version 또는 snapshot이 달라졌으면 최신 자료가 게시 당시와 다름을 표시합니다.
- `연결 자료 상세`와 `공식 원문 열기`는 지도 재조회와 별도 동작으로 유지합니다.

이 흐름은 공개정보의 출처와 시점을 재확인하는 기능입니다. 질문 초안 생성이나 지도 재조회만으로 관심·참여·가원장·주문·계약·배차가 생성되지 않습니다.

## 검증

- 경로 계약, 질문 UseCase, 게시글 근거 패널, 지도 조립, version 비교 대상 test: 72개 통과
- 게시글 생성 전 확인, source version·map deep link 영속 JSON, 기존 근거의 URL 복원, 현재/게시 당시 version 비교를 test로 확인했습니다.
- scoped Fast와 Task의 `Ssalddel.v3.5.slnx` build와 `git diff --check`는 통과했습니다.
- 두 validator의 자동 확장 관련 test는 이번 지도 변경과 무관한 `OfficialFoodIngredientJourneyTests.공식재료화면은_재료이름과_좁은폭동작영역을_실제값으로연결한다` 1개가 기존 공식 재료 CSS의 `min-height: 44px` 누락으로 실패했습니다. 각각 451개가 통과했고 결과는 `artifacts/local/validation/20260804-123729/`, `artifacts/local/validation/20260804-123851/`에 있습니다.
- 실제 API 런타임은 개발 DB의 기존 암호화 개인정보가 참조하는 Data Protection key 불일치 때문에 시작되지 않아 이번 근거 왕복 화면은 직접 렌더링하지 못했습니다.
- WebApp 단독 fallback 브라우저 실행은 지도 query 복원만 증명하며, 서버 observation·게시글 근거 패널의 version 비교를 증명하지 않으므로 화면 상태는 `간접 확인`으로 기록합니다.

## 화면

간접 확인 — 이번 slice의 새 화면은 별도 PNG를 만들지 않았습니다. 기존 지도 선택 deep link와 게시글 공개 근거 패널의 시각 자료는 각각 [지도 선택 stable deep link](2026-08-04-community-map-deep-link-state.md), [게시글 상세 공개 근거 패널](2026-08-04-community-post-source-evidence-panel.md)에 있습니다.
