# 지도 뉴스 출처별 RSS 검토 후보 연결

- 날짜: 2026-08-04
- 화면: `/community/home`
- 범위: 국가별 뉴스 출처 마커의 feed 지원 상태와 명시 선택한 별도 공식뉴스 RSS 검토 후보

## 구현 내용

- 연합뉴스·AP·신화통신·ABC News Australia 마커마다 언론사 자체 feed의 현재 연결 상태를 제공합니다.
- `sourceKey` 없는 최초 상태 조회는 외부 RSS 요청을 실행하지 않습니다.
- 대한민국 연합뉴스 마커에서는 농림축산식품부 보도자료·설명자료와 식품의약품안전처 보도자료를 `같은 국가의 별도 공식자료`로 구분해 제공합니다.
- 후속 [공식뉴스 검토 결정 영속 원장](2026-08-04-official-news-review-ledger.md) 적용 뒤 공개 지도는 특정 원천을 눌러도 외부 RSS를 직접 호출하지 않고, MongoDB 검토 원장에서 승인된 snapshot만 조회합니다.
- 공개 후보에는 제목, 300자 이하 요약, 발행·수집 시각, 승인 상태와 공식 원문 link만 포함합니다.
- 검토 전·제외 후보와 원천 실패는 공개 기사처럼 표시하지 않습니다.
- 후보 조회는 게시글·참여·원장·주문·계약·배차를 만들지 않습니다. 사용자가 별도 질문 작성 흐름에서 확인해야만 게시글 하나가 생성됩니다.

## 언론사 feed 판정

- 연합뉴스: 공식 홈페이지는 연결하지만 검증된 공개 RSS endpoint를 등록하지 않았습니다.
- AP: 공식 문서는 feed를 AP Media API의 라이선스 콘텐츠로 설명하고 API key와 계약 조건을 요구하므로 공개 RSS 원천으로 등록하지 않았습니다. [AP Media API 시작 안내](https://api.ap.org/media/v/docs/Getting_Started_API.htm)
- 신화통신: 공식 홈페이지는 연결하지만 검증된 공개 RSS endpoint를 등록하지 않았습니다.
- ABC News Australia: ABC가 뉴스 RSS feed를 더 이상 갱신하지 않는다고 안내하므로 수집 원천으로 등록하지 않았습니다. [ABC RSS 중단 안내](https://help.abc.net.au/hc/en-us/articles/6147104938383-Why-are-RSS-feeds-no-longer-being-updated)

공개 RSS를 찾지 못한 상태는 `수집 성공`이나 `지원됨`으로 표시하지 않습니다. 이후 공식 feed가 확인되더라도 이용 조건·저작권·호출 제한을 검토한 뒤 source별로 별도 등록해야 합니다.

## 검증

- 지도 뉴스 후보 UseCase, 기존 공식뉴스 RSS parser, 지도 UI 조립 대상 test: 35개 통과
- `Ssalddel.v0.0.slnx` build 통과
- scoped Fast·Task의 `Ssalddel.v3.5.slnx` build, `git diff --check`, 자동 관련 test 36개가 모두 통과했습니다. 상세 결과는 `artifacts/local/validation/20260804-125746/`, `artifacts/local/validation/20260804-125858/`에 있습니다.
- 실제 외부 RSS 호출과 게시 실행은 하지 않았습니다.
- 로컬 API의 기존 Data Protection key 불일치가 남아 있어 이번 패널은 코드·test·build로 간접 확인했습니다.

## 화면

간접 확인 — 실제 API 연결 화면을 렌더링하지 못해 새 PNG를 만들지 않았습니다. 기존 언론사 마커 화면은 [국가별 언론·뉴스 출처 마커](2026-08-04-community-map-news-publisher-markers.md)에서 확인할 수 있습니다.
