# Platform Community Home

## Operating Policy

- The community surface is free by default. General posts, comments, image comments, recommendations, report-board posts, suggestions, and coordination posts are treated as basic communication infrastructure.
- The platform should charge only for optional tools that help users perform work more easily, such as legal/document helpers, business exports, promoted posts, advanced matching, bulk notifications, scheduled posts, or customs/HS-code support workflows.
- Required work features stay outside this optional fee layer. For example, transport completion photos, required file uploads, warehouse state changes, and legally necessary audit records must keep working regardless of auxiliary feature settings.
- Admin configuration should distinguish free communication, required work, optional utility, and paid utility features.
- See [Community Operating Policy](Architecture/CommunityOperatingPolicy.md) for the operating model.

## Role Profile Badges

- Community author and commenter displays use a lightweight role-profile badge.
- The badge shows a small avatar ring and role chip so readers can quickly understand whether a participant mainly appears as a driver, shipper, orderer, seller, warehouse operator, customs broker, HR manager, operator, or general participant.
- The current implementation infers the role from app/context/category/nickname because community DTOs do not yet store a primary role field.
- Report-board posts keep masked participants as anonymous role badges.
- Later, when participant/profile tables become the source of truth, the same component should receive the user's declared or system-derived primary role instead of relying on context inference.

## Photo Attachments

- Community posts can include image attachments so the hub can handle field photos, notices, reference screenshots, and lightweight visual reports.
- Attachments are uploaded after the post is created or edited, and the post password is checked again before each upload.
- The server stores attachment metadata in `platform_community_post_attachments` and cascades deletion when the parent post is deleted.
- Image files are uploaded through `IGoogleCloudStorageService`; the root folder, maximum file size, attachment count, and allowed content types are controlled by `CommunityPostStorage` in `appsettings`.
- The default storage folder is `community/posts`, while development uses `community/dev-posts`.

## Promotion And Engagement

- Public users can share text, image attachments, and external links such as video URLs.
- Public create/update APIs do not accept direct top-pinning authority.
- Operator-pinned posts are controlled through `POST api/v1/community/posts/{id}/operator-pin` and require the `서버관리자전용` authorization policy.
- Normal posts rise in the list through server-side engagement signals: recommendation count, comment count, and latest engagement time.
- Recommendations are recorded in `platform_community_post_recommendations` with a per-post recommender key to avoid repeated recommendations from the same client session.
- Comments are stored in `platform_community_post_comments`; the post keeps `CommentCount` and `LastEngagedAtUtc` so active conversations can appear higher without becoming operator-pinned notices.
- `PlatformCommunityHome` now exposes inline recommendation and comment entry controls on each post card.
- Admin hosts can pass `CanManageCommunityPosts="true"` to show operator pin/unpin controls; ordinary app hosts leave this disabled.
- Image attachments support their own comment stream, stored in `platform_community_post_attachment_comments`.
- Attachment comments update the parent post engagement timestamp so active image discussions can also raise the post in the community feed.
- Each image card shows its recent image comments and provides an inline nickname/password/comment form.
- Post comments and image comments can be deleted with the original comment password.
- Post comments and image comments can be reported; report counts are stored on the comment records.
- Operators can hide post comments and image comments through server-admin-only moderation endpoints. Hidden comments are excluded from community reads without deleting the underlying record.

## Report Board Privacy

- Community posts can be marked as report-board posts with `IsReportBoardPost`.
- Report-board posts store `ReporterDisplayName` and `ReportedDisplayName` separately from the public post nickname.
- Public/community list responses use observer-safe labels for report subjects by default: reporter and reported party are masked.
- The UI shows report-board posts with a warning notice and displays report subjects as anonymous observer labels.
- The intended visibility split is:
  - observers see masked report subjects and masked participant labels;
  - reporter, reported party, and operators may receive a future role-checked response with the minimum necessary identity range;
  - operator moderation remains separate from normal top-pinning and engagement sorting.
- Server-side masking is the primary rule. Client-side masking is kept as an additional display guard.

## Community Hub Posts

- 플랫폼 홈에는 로그인 역할과 무관하게 쓸 수 있는 커뮤니티 글 영역을 둔다.
- 작성자는 글마다 닉네임과 비밀번호를 입력한다.
- 비밀번호는 서버에 평문 저장하지 않고 BCrypt 해시로 저장한다.
- 글 수정과 삭제는 작성 시 입력한 비밀번호가 맞을 때만 허용한다.
- 1차 API는 `api/v1/community/posts`이며, 목록 조회와 작성은 공개하고 수정/삭제는 게시글 비밀번호로 보호한다.

Hongdal 앱들의 기본 홈 화면은 업무 대시보드 하나로 고정하지 않고, 플랫폼 커뮤니티와 공지 흐름을 먼저 보여주는 방향으로 둔다.

## 기본 원칙

- 기본 홈(`/`)은 공지, 운영 공유, 개선 제안, 정책 변경처럼 플랫폼 전체 참여자가 같이 보는 내용을 우선한다.
- 앱별 핵심 업무는 홈 안의 빠른 실행과 좌측 메뉴에서 바로 진입한다.
- 관리자가 설정하는 화면 정책에서는 홈을 기본 노출값으로 유지하고, 개별 업무 화면은 역할별로 조정한다.
- 특정 역할 전용 앱은 예외적으로 로그인 전 화면이나 핵심 업무 화면을 먼저 둘 수 있지만, 플랫폼 소속감이 필요한 앱은 커뮤니티 홈을 기본값으로 삼는다.

## 현재 반영

- `Hongdal.Ui.Common`에 `PlatformCommunityHome` 공통 컴포넌트를 추가했다.
- `HongdalAdmin`의 `/` 홈은 플랫폼 커뮤니티 홈으로 전환했다.
- `WarehouseManagerApp`의 `/` 홈은 플랫폼 커뮤니티 홈으로 전환하고, 입고/출고/포장/스캔/작업 보드를 빠른 실행으로 배치했다.
- 서버관리자 로그인 후 기본 이동은 `/drivers/operating`이 아니라 `/`로 조정했다.

## 다음 단계

- 커뮤니티 게시글을 정적 샘플에서 서버 API 기반 게시판으로 전환한다.
- 앱별/역할별 게시글 공개 범위를 설계한다.
- 화주 앱과 기사 앱의 홈 전환은 기존 업무 시작 UX와 충돌하지 않게 설정값으로 분리한다.
