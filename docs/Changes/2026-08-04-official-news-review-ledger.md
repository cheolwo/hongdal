# 공식뉴스 검토 결정 영속 원장

- 날짜: 2026-08-04
- 서버 범위: 관리자 검토 API, MongoDB `official_news_review_ledgers`, 공개 지도 승인 후보 조회
- 화면: `/community/home`

## 구현 내용

- 서버관리자는 기존 공식뉴스 RSS 후보를 다시 조회한 뒤 `Approved` 또는 `Excluded` 결정을 기록합니다.
- 원장은 candidate key를 stable ID로 사용하고 source key, 후보 metadata JSON snapshot, 현재 상태, revision과 생성·수정 시각을 저장합니다.
- 각 결정 이력에는 idempotency key, 결정 상태·사유, 내부 검토자 ID, 표시 이름, 결정 시각과 revision을 남깁니다.
- 공개 응답의 결정 이력에는 내부 검토자 ID를 포함하지 않습니다.
- 같은 candidate와 idempotency key의 재시도는 revision과 이력을 늘리지 않습니다.
- 기존 원장을 변경할 때는 `ExpectedRevision`이 일치해야 하며 오래된 검토 화면의 결정은 `409 Conflict`로 거부합니다.
- 승인·제외 전에 source가 공식뉴스 원천인지 확인하고, RSS 후보를 다시 찾을 수 없거나 원천 조회가 실패하면 결정 성공으로 숨기지 않습니다.
- 공개 지도 API는 외부 RSS를 직접 호출하지 않고 원장에서 `Approved`인 snapshot만 읽습니다. 이후 제외로 전환된 후보는 공개 목록에서 빠집니다.

검토 승인은 기사 정확성·정치적 중립성·가격·재고·공급 가능성을 보증하지 않으며 게시글·참여·주문·계약·배차를 자동 생성하지 않습니다.

## API

- `GET /api/v1/admin/content/information/official-news/review-ledgers`
- `POST /api/v1/admin/content/information/official-news/candidates/{candidateKey}/review-decisions`
- 두 관리자 route는 기존 `서버관리자전용` 정책을 그대로 사용합니다.
- 공개 `GET /api/v1/community/world-map/observations/{stableId}/news-candidates`는 승인 원장의 읽기 전용 projection입니다.

## 검증

- 후보 snapshot 저장, 내부 검토자 ID 비공개, 멱등 재시도, 승인→제외 이력, revision 충돌, 승인 목록 필터를 test로 확인했습니다.
- 지도 source 선택이 RSS를 다시 호출하지 않고 승인 원장만 읽는 것을 test로 확인했습니다.
- 직접 대상 test 40개와 scoped Fast·Task의 `Ssalddel.v3.5.slnx` build, `git diff --check`는 통과했습니다.
- Fast·Task 자동 확장 test는 각각 78개 중 77개가 통과했고, 이번 변경과 무관한 기존 `농수산정보Controller.Hs식품국가가격Card조회`의 명명 규칙 test 1개가 실패했습니다. 결과는 `artifacts/local/validation/20260804-132849/`, `artifacts/local/validation/20260804-133021/`에 있습니다.
- 실제 MongoDB와 외부 RSS, 관리자 결정 API는 호출하지 않았습니다.
- 로컬 API의 기존 Data Protection key 불일치 때문에 화면과 재시작 후 실제 Mongo 재조회는 미확인입니다.

## 화면

간접 확인 — 관리자 화면은 추가하지 않았고 공개 지도는 승인 후보가 있을 때만 기존 RSS 검토 카드 영역에 표시합니다.
