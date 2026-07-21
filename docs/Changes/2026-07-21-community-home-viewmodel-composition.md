# 커뮤니티 홈 ViewModel 책임 조립 분리

## 변경

- 공개 게시판의 목록·글쓰기·참여·원장 선택 책임을 `PlatformCommunityPublicBoardViewModel`로 묶었다.
- 사용자가 명시적으로 여는 음식 발견·다이어그램·서원·근거 그래프·창고 연결 도구를 `PlatformCommunityConnectedToolsViewModel`로 분리했다.
- `PlatformCommunityHomePageViewModel`은 두 조립 ViewModel의 수명과 페이지 상태만 관리하고 기존 하위 ViewModel 접근 계약은 유지한다.
- 공통 UI DI에 두 조립 ViewModel을 독립 transient 서비스로 등록했다.

## 화면 변화

간접 확인 — 렌더링 컴포넌트와 사용자 문구는 바꾸지 않고 ViewModel 생성·수명·책임 경계만 분리했다.

## 검증

- 커뮤니티 홈 조립·하위 상태 전달 ViewModel 테스트
- 공통 UI DI 등록 회귀 테스트
