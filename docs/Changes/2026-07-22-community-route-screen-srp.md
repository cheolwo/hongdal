# 커뮤니티 Route·공용 Screen 단일책임 분리

## 변경 요약

- 하나의 `CommunityWorkspacePage.razor`가 맡던 작업공간, 글쓰기, 추천 목록, 추천 sample 상세와 영속 게시글 상세 route를 각각 독립 Route Page로 분리했다.
- 실제 렌더링에서 작업공간에 게시판 관리·원장 초안·글 작성이 함께 남은 것을 확인한 뒤, 작업공간은 탐색 허브로 축소하고 `/community/boards/manage`와 `/community/ledgers/new`를 추가했다.
- 각 Route Page는 `Ssalddel.Ui.Common`의 전용 Screen 하나와 Web 전용 navigation frame만 조립한다.
- `CommunityPageRoutes`에 게시판, 글쓰기, 상세, 추천 sample 상세, 작업공간, 게시판 관리, 원장 초안과 다이어그램 route builder를 모았다.
- 과거 `/community/posts/recommended?seed=...` 링크는 `/community/posts/recommended/detail?seed=...`로 replace 이동해 기존 deep link를 보존한다.
- 관리자 페이지 카탈로그와 page capability 경계를 새 소유 파일과 route 의미에 맞게 갱신했다.

## 책임 경계

| Route | 주된 사용자 목표 | 공용 Screen |
| --- | --- | --- |
| `/community/workspace` | 업무·원장 목적지 탐색 | `CommunityWorkspaceScreen` |
| `/community/boards/manage` | 게시판 개설 신청·권한별 검토 | `CommunityBoardManagementScreen` |
| `/community/ledgers/new` | 공동 원장 초안 작성 | `CommunityLedgerDraftScreen` |
| `/community/write` | 게시글 작성 | `CommunityPostComposeScreen` |
| `/community/posts/recommended` | 추천 글 목록 탐색 | `CommunityRecommendedPostListScreen` |
| `/community/posts/recommended/detail` | 추천 sample 글 한 건 읽기 | `CommunityRecommendedPostDetailScreen` |
| `/community/posts/{PostId:long}` | 영속 게시글 한 건 읽기·참여 | `CommunityPostDetailScreen` |

desktop과 모바일은 같은 Screen을 소비하고 Route Page가 플랫폼 shell만 맡는다. 작업공간 허브의 원장 유형별 `원장 초안`은 선택한 template key를 `/community/ledgers/new`로 넘기며, 허브 안에서 편집기를 열지 않는다.

## 화면 확인

- 로컬 WebApp에서 1280px 기준 글쓰기, 추천 목록, 추천 sample 상세, 영속 글 상세와 legacy 추천 링크 이동을 확인했다.
- 390×844 기준 글쓰기와 추천 목록에서 horizontal overflow가 없음을 확인했다.
- 확인한 route의 browser console error는 없었다.
- 작업공간에서 추가 책임 혼합을 발견해 게시판 관리와 원장 초안을 한 번 더 분리했다. 이 마지막 허브 축소는 build와 구성 테스트로 검증했으며 최종 화면 PNG 재캡처는 commit 단계에 남아 있다.

## 검증

- `dotnet test Ssalddel.Tests/Ssalddel.Tests.csproj --no-restore --filter "FullyQualifiedName~CommunityWorkspaceRouteContextTests|FullyQualifiedName~CommunityWorkspacePageCompositionTests|FullyQualifiedName~CommunityPageRoutesTests|FullyQualifiedName~PlatformCommunityHomeCompositionTests|FullyQualifiedName~PageRouteInventoryTests"`
  - 46개 통과
- `dotnet build Ssalddel.WebApp/Ssalddel.WebApp.csproj --no-restore`
  - 경고 0, 오류 0
- `dotnet build SsalddelApp/SsalddelApp.csproj -f net10.0-windows10.0.19041.0 --no-restore`
  - 경고 0, 오류 0
- `dotnet build SsalddelAdminApp/SsalddelAdminApp.csproj -f net10.0-windows10.0.19041.0 --no-restore`
  - 경고 0, 오류 0

## 다음 단계

국내 공동구매의 목록·신규 제안·campaign 상세·참여·협상·이의제기·결의·서명을 stable campaign ID 기반 Route Page와 공용 Screen으로 순차 분리한다. 0.0에서는 생산자·기사 추천, 계약 대리, 자동 배차와 결제를 활성화하지 않는다.
