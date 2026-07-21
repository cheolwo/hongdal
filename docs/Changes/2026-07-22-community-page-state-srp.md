# 커뮤니티 WebApp 페이지 상태 책임 분리

## 변경 요약

- `CommunityBoardPage.razor`에서 게시판 조회·현재 게시판 판정·검색·필터·오류·페이징 상태를 `CommunityBoardPageViewModel`로 옮겼다.
- 과거 게시판 이름은 기존 서버 호환 category로 보존하고, 카탈로그 게시판은 정규 `boardKey`로 조회한다.
- `CommunityWorkspacePage.razor`의 글쓰기·업무 공간·게시글 상세·다이어그램 route 판정을 `CommunityWorkspaceRouteContext`로 분리했다.
- Razor 페이지는 route parameter와 navigation callback 조립만 담당한다.

## 화면 영향

화면 없음 — 기존 게시판과 업무 공간 표시를 유지하는 내부 책임 분리다. 네트워크 실패 시 핵심 게시판 탐색 구조를 유지하고 재시도 상태를 표시하는 계약을 단위 테스트로 확인했다.

## 검증

- `CommunityBoardPageViewModelTests`
- `CommunityWorkspaceRouteContextTests`
- `dotnet build Ssalddel.WebApp/Ssalddel.WebApp.csproj --no-restore`
