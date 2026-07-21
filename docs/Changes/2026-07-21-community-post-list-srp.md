# 커뮤니티 글 목록 단일책임 분리

## 변경 기록

| 변경 축 | 화면 변경 여부 | 책임 경계 |
| --- | --- | --- |
| 글 목록 조립 shell | 화면 유지 | 546줄에서 97줄로 줄이고 목록형·카드형 전환과 하위 영역 조립만 담당 |
| 도구막대 | 화면 유지 | 게시판·글 수, 추천/공지 필터, 목록/카드 보기, 글쓰기 진입을 담당 |
| 표 목록 | 화면 유지 | 실제 글·추천 seed 글의 행, 작성자 공개 범위, 선택 event와 빈 상태를 담당 |
| 카드 목록 | 화면 유지 | 카드 본문·판매 요약·작성자 공개 범위·지표·빈 상태를 담당 |
| 검색 footer | 화면 유지 | 검색어 입력·초기화와 상세 진입 안내를 담당 |
| 표현 규칙 | 간접 확인 | 언어별 이름, 선택/공지/자동글 class, 날짜·가격·수량, 신고 익명화, YouTube 표시 판정을 순수 함수로 분리 |
| 격리 CSS | 화면 유지 | 기존 판매 badge·요약 style이 하위 component에도 적용되도록 root `::deep` 범위로 명시 |

## 조립 구조

```text
PlatformCommunityPostList (97줄 조립 shell)
├─ PlatformCommunityPostListToolbar
├─ PlatformCommunityPostTable
├─ PlatformCommunityPostCards
├─ PlatformCommunityPostSearchFooter
└─ PlatformCommunityPostListPresentation
```

조회·loading·오류 상태는 기존처럼 `PlatformCommunityPostIndex`와 상위 페이지 ViewModel이 맡는다. 이 component는 전달받은 공개 글·추천 글을 표현하고 사용자 선택 event를 반환할 뿐 API를 직접 호출하거나 게시 상태를 바꾸지 않는다.

## 유지·보강한 동작

- 목록형과 카드형 모두 실제 글, 추천 seed 글, 선택 상태와 빈 상태를 유지한다.
- 비공개 신고 관계자는 계속 `익명 신고자`로 표시하고 활동 국가를 노출하지 않는다.
- 시스템 글·운영자 공지·인기 글·음식 YouTube 글·참여 momentum의 표시 우선순위를 유지한다.
- 판매글은 상태, 통화별 가격, 잔여 수량, 공동구매 제안 가능 여부와 첫 첨부 썸네일을 유지한다.
- 한국어/영어 게시판·필터 이름과 검색 접근성 문구를 유지한다.
- 외부 영상 아이콘은 기존처럼 `youtu.be`와 `youtube.com` 절대 URL만 허용한다.

## 화면

간접 확인 — clean worktree에서 `/community`와 `/ko/community`가 모두 HTTP 200으로 응답했다. 같은 세션의 내장 브라우저 WebView 연결 제한 때문에 desktop·mobile PNG는 만들지 못했다. 기존 class·문구·DOM 영역을 그대로 옮겼고, 생성된 격리 CSS 번들에서 root scope가 하위 판매 요소에 적용되는 selector를 확인했다.

## 검증

- clean worktree `Ssalddel.Ui.Common` build 경고 0개·오류 0개
- clean worktree `Ssalddel.WebApp` build 경고 0개·오류 0개
- clean worktree `SsalddelApp` Windows build 경고 0개·오류 0개
- 글 목록 표현 규칙·책임 조립·기존 목록 ViewModel·공용 component 계약 테스트 17개 통과
- 생성 CSS에서 `[scope] .platform-community-sales-label`과 `[scope] .platform-community-sales-summary` 확인
