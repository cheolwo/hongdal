# 커뮤니티 글쓰기 단일책임 분리

## 변경 기록

| 변경 축 | 화면 변경 여부 | 책임 경계 |
| --- | --- | --- |
| 글쓰기 조립 shell | 화면 유지 | 794줄에서 215줄로 줄이고 저장·임시저장·닫기·판매 전환과 하위 영역 조립만 담당 |
| 머리글·게시판 | 화면 유지 | 게시판 탭, 작성 조건, 임시저장·등록·닫기 행동을 담당 |
| 상태·초안 폐기 | 화면 유지 | 저장 상태와 2단계 초안 비우기 확인을 담당 |
| 본문 | 화면 유지 | 제목·본문 입력과 blur 자동저장 event만 담당 |
| 판매 정보 | 화면 유지 | 상품·수량·가격·통화·협의 가능 결제·판매 상태 입력을 담당 |
| 첨부 도구 | 화면 유지 | 선택 사진, 원장·자료·다이어그램·근거·이미지 도구 진입을 담당 |
| 문맥·게시 설정 | 화면 유지 | 현재 업무·역할·국가·원장·예약 상태와 상세 게시 설정을 각각 담당 |
| 표현 규칙 | 간접 확인 | 작성 권한 안내, 상태 우선순위, 등록 action 문구, 탭 class를 순수 함수로 분리 |

## 조립 구조

```text
PlatformCommunityPostComposer (215줄 조립 shell)
├─ PlatformCommunityComposerHeader
├─ PlatformCommunityComposerFeedback
├─ PlatformCommunityComposerBodyFields
├─ PlatformCommunityComposerSalesEditor
├─ PlatformCommunityComposerAttachmentTools
├─ PlatformCommunityComposerContextBar
├─ PlatformCommunityComposerSettings
└─ PlatformCommunityComposerPresentation
```

최상위 component는 기존 `CommunityPostComposerViewModel`과 저장 결과 event 계약을 유지한다. 하위 component는 같은 ViewModel의 필요한 화면 영역만 표현하며 API를 직접 호출하거나 게시 상태를 확정하지 않는다.

## 유지한 동작

- 비로그인 익명 게시판은 자동 익명 닉네임을 사용하고 로그인 작성자는 계정 공개 닉네임을 사용한다.
- ViewModel 검증·저장 상태가 외부 글쓰기 도구 상태보다 우선한다.
- 제목·본문 blur 임시저장, 명시적 임시저장, 초안 비우기, 닫기 전 자동저장 흐름을 유지한다.
- 판매 정보를 켜면 판매 게시판으로 분류하고 비어 있는 상품명에 현재 제목을 복사한다.
- 저장 성공 뒤에만 상위 component로 `Saved` event를 전달한다.
- CSS와 기존 class 이름을 바꾸지 않아 사용자에게 보이는 배치와 문구를 유지한다.

## 화면

간접 확인 — clean worktree의 실제 `/ko/community` route가 HTTP 200으로 응답하는 것까지 확인했다. 현재 세션의 내장 브라우저 WebView가 열리지 않아 desktop·mobile DOM과 PNG 캡처는 만들지 못했다. CSS와 기존 DOM class 계약은 변경하지 않았으며 이 제한을 숨기지 않고 후속 `PlatformCommunityPostList` 작업의 실제 렌더링 검증에서 함께 재확인한다.

## 검증

- clean worktree `Ssalddel.Ui.Common` build 경고 0개·오류 0개
- clean worktree `Ssalddel.WebApp` build 경고 0개·오류 0개
- 글쓰기 표현 규칙·책임 조립·기존 ViewModel·공용 component 계약 테스트 35개 통과
- `SsalddelApp` Windows 소비 빌드에서 기존 `ShipperSalesService`의 계정 상세조회 계약 누락을 발견했으며 별도 fix 맥락에서 보완 후 경고 0개·오류 0개 확인
