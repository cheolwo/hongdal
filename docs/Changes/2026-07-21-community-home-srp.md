# 커뮤니티 홈 단일책임 화면 조립

## 변경 기록

| 변경 축 | 화면 변경 여부 | 책임 경계 |
| --- | --- | --- |
| 커뮤니티 홈 route shell | 화면 유지 | 736줄에서 313줄로 줄이고 게시판·상세·원장 선택·요약·다이어그램·업무 mode 중 표시할 화면만 결정 |
| 홈 머리글 | 화면 유지 | 게시판 제목·설명·현재 mode와 글쓰기·원장·다이어그램·상점·업무 진입을 담당 |
| 공개 글 feed | 화면 유지 | loading·empty·선택 글/seed 글 상세와 앱 공지를 독립적으로 표현 |
| 다이어그램 stage | 화면 유지 | desktop/mobile canvas, 연결선 편집, 창고 대행, diagram chat과 글쓰기 전환을 담당 |
| 생활 원장 초안 | 화면 유지 | 원함 분석, 현재 원장, 인라인 다이어그램, API metadata와 초안 전환을 담당 |
| 업무 workspace | 화면 유지 | 게시판 관리·업무 hub·원장 초안·글쓰기·근거 도구·지원 영역을 조립 |
| surface adapter | 간접 확인 | 각 화면이 필요한 상태와 사용자 event만 노출하고 상위 Home의 private workflow 세부사항을 감춤 |

## 조립 구조

```text
PlatformCommunityHome (313줄 route/mode shell)
├─ PlatformCommunityHomeHero
├─ PlatformCommunityHomeFeed
├─ PlatformCommunityHomeDiagramStage
│  └─ DiagramStageSurface
├─ PlatformCommunityHomeWorkspace
│  ├─ PlatformCommunityHomeLedgerDraft
│  │  └─ LedgerDraftSurface
│  └─ WorkspaceSurface
└─ 기존 게시판·상세·원장 선택·mobile navigation component
```

`PlatformCommunityHomePageViewModel`의 공개 게시판과 명시적으로 여는 연결 도구 수명은 그대로 유지한다. 새 surface adapter는 API를 호출하거나 상태 전이를 새로 만들지 않고, 기존 Home workflow를 화면별 입력과 event로 제한해 전달한다.

## 유지한 동작

- 기본 `/community` 진입은 전통 게시판 header·게시판 shelf·공개 글 목록 흐름을 유지한다.
- 업무 workspace와 다이어그램은 기존 조건 또는 사용자의 명시적 행동으로만 열린다.
- 게시글 loading·empty·실제 글/seed 글 선택·판매 문의·참여·원장 재사용 event 계약을 유지한다.
- 원함 분석, 현재 원장 선택, 연결선 편집, API 경로 준비와 글쓰기 전환은 기존 ViewModel/Command 경계를 그대로 사용한다.
- 하위 화면은 전달받은 상태를 표현하며 서버 응답 전에 게시·원장·운송·창고 상태를 확정하지 않는다.
- 기존 CSS class와 stylesheet를 유지해 의도적인 시각 변경을 만들지 않았다.

## 화면

간접 확인 — clean 격리 worktree에서 실제 `/community` route가 HTTP 200으로 응답했다. 앱 내 브라우저 WebView가 두 차례 연결되지 않아 desktop·390px mobile DOM과 PNG 캡처는 만들지 못했다. 기존 markup과 CSS class를 책임별 component로 이동했고 WebApp 및 Windows MAUI 소비 빌드로 Razor 계약을 확인했다.

## 검증

- clean 격리 worktree `Ssalddel.Ui.Common` build 경고 0개·오류 0개
- clean 격리 worktree `Ssalddel.WebApp` build 경고 0개·오류 0개
- clean 격리 worktree `SsalddelApp` Windows build 경고 0개·오류 0개
- 커뮤니티 홈 책임 조립·기존 Home ViewModel 관련 테스트 18개 통과
- 실제 `/community` HTTP 200 확인
