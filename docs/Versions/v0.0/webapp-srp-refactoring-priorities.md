# WebApp 페이지 단일책임 리팩터링 우선순위

기준일: 2026-07-22

이 문서는 WebApp의 화면을 모바일 앱의 기반으로 다시 사용할 수 있도록 **route와 사용자 목표 단위**로 재평가한 작업 순서다. 파일 길이와 내부 component 분리는 책임 혼합을 찾는 보조 신호일 뿐이며, 완료 판단은 [0.0 집중 로드맵](./focus-roadmap.md)의 사용자 여정과 [통합 클라이언트 3단계 내비게이션](../../Architecture/ThreeStageClientNavigation.md)을 따른다.

## 완료 단위

이 문서에서 사용하는 단위는 다음과 같다.

| 단위 | 책임 |
| --- | --- |
| `Route Page` | URL parameter와 query를 읽고 플랫폼 navigation shell과 공용 Screen 하나를 조립한다. |
| `Screen` | 사용자가 완료하려는 하나의 주된 목표를 표현하며 `Ssalddel.Ui.Common`에 둔다. |
| `ViewModel/UseCase` | 조회, 입력 검증, 상태 전이와 서버 재조회 workflow를 맡는다. |
| `Navigation Contract` | Web과 모바일이 공유할 page key, route builder, diagram 복귀 문맥과 deep link를 정의한다. |

페이지 단일책임은 카드나 입력 항목마다 route를 만드는 뜻이 아니다. loading, empty, error, retry, 인증 안내와 목표 수행에 필요한 읽기 전용 요약은 같은 Screen의 보조 상태로 둘 수 있다. 반면 목록 탐색, 상세 조회, 작성, 결제, 증빙, 승인, 예외 보고처럼 사용자가 별도로 시작·취소·완료할 수 있는 목표는 별도 Screen으로 분리한다.

1단계 허브와 2단계 다이어그램은 여러 목적지의 관계를 보여 줄 수 있지만 3단계 업무의 입력·결제·승인·완료 Command를 대신 실행하지 않는다. desktop은 여러 Screen을 split pane으로 조립할 수 있고, 모바일은 같은 Screen을 navigation stack, drawer 또는 bottom sheet로 배치한다.

## 2026-07-22 감사 기준선

- WebApp은 87개 Razor route 파일에 119개 route 선언이 있다.
- 통합 모바일 앱은 38개 route 파일에 41개 route 선언이 있다.
- 두 앱에서 문자열이 같은 route 35개 중 같은 최상위 공용 Screen을 명시적으로 조립하는 경로는 15개다.
- WebApp에는 150줄 이상이면서 공용 feature Screen을 직접 조립하지 않는 route page가 19개 있다.
- `Ssalddel.Ui.Common`에도 업무 route literal이 남아 있어 앱별 route 의미와 deep link가 어긋날 수 있다.

이 수치는 우선순위 신호다. 줄 수를 줄이거나 내부 component 파일을 늘리는 것만으로 완료 처리하지 않는다.

## Route·Screen 실행 순서

| 우선순위 | 문맥 | 현재 신호 | 목표 route·Screen | 완료 조건 |
| --- | --- | --- | --- | --- |
| `P0-0` 진행 | 공용 navigation 계약 | WebApp·모바일 route catalog와 `Ui.Common`의 URL literal이 분산됨 | 공용 `CommunityPageRoutes`, 이후 page key·diagram return context | 공용 Screen이 플랫폼 namespace를 참조하지 않고 Web·모바일 deep link가 같은 의미로 해석됨 |
| `P0-1` 완료 | 커뮤니티 기본 흐름 | 한 `CommunityWorkspacePage.razor`가 작업공간·게시판 관리·원장 초안·글쓰기·추천 목록·추천 상세·일반 상세를 mode로 전환했음 | 허브, 게시판 관리, 원장 초안, 글쓰기, 추천 목록·상세, 영속 글 상세 전용 Route Page와 공용 Screen | route 파일마다 하나의 의미와 공용 Screen 하나만 가지며 작업공간은 탐색 허브만 맡고 기존 `?seed=` 링크가 새 추천 상세 route로 호환 이동함 |
| `P0-2` 완료 | 국내 공동구매 대표 파일럿 | 변경 전에는 내부 component가 나뉘어도 한 route가 제안·목록·상세·공급자·배송 정보·이행 초안·협상·단계 전이·이의제기를 모두 실행했음 | 목록, 개설, 캠페인 상세, 참여, 공급자, 협상, 이의제기, 결의, 서명, 배송 가능 정보, 이행 초안 Screen | Web·모바일이 같은 공용 Screen을 조립하고 각 단계가 stable campaign ID와 직접 URL을 가지며 저장 뒤 같은 ID를 재조회하고 추천·자동 배차·결제·계약을 확정하지 않음 |
| `P0-3` 완료 | 다이어그램 | 변경 전에는 Web 전용 `DiagramWorkbenchPage`가 palette·preset·canvas를 함께 소유하고 모바일 route가 없었음 | 공용 diagram Screen, desktop sidebar, mobile bottom sheet | Web·모바일 `/diagram`이 같은 Screen을 조립하고 선택 node·zoom·filter·출발 page를 복원하며 구체 업무 Command는 3단계 Screen으로 이동함 |
| `P0-4` | 커뮤니티 화면 복귀 문맥 | board query는 있으나 diagram node와 return context 계약이 없음 | 공용 `PageNavigationContext`와 route adapter | Web·모바일에서 게시판·다이어그램으로 돌아갈 때 선택 상태가 복원되고 deep link test를 통과함 |
| `P1-1` | 운송 요청 작성 | Web 한 화면 606줄, Web 단계 route 4개, 모바일 wizard가 서로 다른 workflow를 가짐 | 화물, 운송, 절차, 최종 확인 공용 Screen | 같은 draft와 validation을 공유하고 Web은 adaptive layout, 모바일은 단계 navigation만 담당함 |
| `P1-2` | 운송 요청 상세 | Web 828줄과 모바일 988줄의 독립 monolith | 요약, 진행 이력, 결제, 증빙 Screen | 같은 request ID와 서버 원본을 사용하고 결제·증빙은 명시적 별도 route에서만 실행함 |
| `P1-3` | 입고 요청 | `SsalddelInboundRequestManager` 한 route에서 창고 빠른 등록·입고 신청·목록·완료 처리를 수행함 | 목록, 신규 신청, 상세, 입고 완료 Screen | 목록과 Command가 분리되고 성공 뒤 같은 inbound ID를 재조회함 |
| `P1-4` | 창고·판매 master-detail-action | 입고 검수·피킹·마트 상품·판매 주문이 desktop 한 화면에 목록·상세·행동을 함께 배치함 | List, Detail, Action Screen과 adaptive desktop composition | 모바일에서 각 Screen을 독립 route로 열고 desktop split pane은 선택적 조립으로만 제공함 |
| `P2-1` | 개인 공간·꾸미기 | Web 개인 route multiplexer와 모바일 전용 꾸미기 route의 의미가 다름 | 개인 개요와 꾸미기 상점·상품·checkout Screen 분리 | 같은 route가 플랫폼마다 다른 기능을 뜻하지 않으며 FakePG 경계를 유지함 |
| `P2-2` | `/shipper` 홈 | Web은 링크 디렉터리, 모바일은 커뮤니티 홈·인증·dashboard를 수행함 | 공용 화주 허브 Screen과 플랫폼 shell | 같은 route의 주된 목표가 같고 1.0 이후 기능은 flag 뒤에 유지됨 |
| `P3` 보존 | 기사 추천·정산·통관 등 1.0 이후 화면 | 큰 Web 전용 화면이 남아 있으나 0.0 범위가 아님 | 재활성화 시 List·Detail·Action Screen으로 분리 | 0.0 기본 비노출, 추천·자동 배차·결제·정산 운영 효과 확장 금지 |

## 커뮤니티 목표 route 지도

| 단계 | 사용자 목표 | Route |
| --- | --- | --- |
| 1단계 | 공개 커뮤니티·게시판 탐색 | `/community`, `/community/boards` |
| 2단계 | 업무·원장 목적지 탐색 | `/community/workspace` |
| 2단계 | 공동 원장 초안 작성 | `/community/ledgers/new` |
| 2단계 | 게시판 개설·관리 | `/community/boards/manage` |
| 2단계 | 참여자·업무 관계 확인 | `/diagram?ledgerTemplate=...&node=...&zoom=...&filter=...&from=...` |
| 3단계 | 새 글 작성 | `/community/write` |
| 3단계 | 추천 글 목록 | `/community/posts/recommended` |
| 3단계 | 추천 sample 글 상세 | `/community/posts/recommended/detail?seed=...` |
| 3단계 | 영속 게시글 상세 | `/community/posts/{PostId:long}` |
| 3단계 | 공동구매 목록·개설 | `/community/group-purchase`, `/community/group-purchase/new` |
| 3단계 | 공동구매 상세·참여·공급자·협의 | `/community/group-purchase/{CampaignId:guid}`와 하위 `/participation`, `/suppliers`, `/negotiation` |
| 3단계 | 공동구매 이의·결의·서명 | campaign 하위 `/objections`, `/resolution`, `/signature` |
| 3단계 | 공동구매 배송 정보·이행 초안 | campaign 하위 `/delivery-options`, `/fulfillment-draft` |

## 기존 component 리팩터링의 재분류

아래 작업은 유효한 내부 책임 분리이며 되돌리지 않는다. 다만 route 하나가 여러 사용자 목표를 계속 수행하면 **페이지 SRP 완료**가 아니라 공용 Screen을 만들기 위한 기반 완료로 본다.

| 기존 작업 | 재평가 |
| --- | --- |
| `PlatformCommunityPostComposer` 794줄 → 215줄 shell | 글쓰기 Screen 내부 component 책임 분리 완료 |
| `PlatformCommunityPostList` 546줄 → 97줄 shell | 목록 Screen 내부 표현 책임 분리 완료 |
| `PlatformCommunityHome` 736줄 → 약 313줄 mode shell | 내부 surface 분리 완료, route별 공용 Screen 경계 추가 후 mode 제거는 후속 작업 |
| `CommunityGroupPurchaseWorkspace` root shell 축소 | 내부 component 분리 완료, route 단위 제안·목록·상세·단계 행동 분리는 `P0-2` |
| `SsalddelInboundReceivingWorkspace` 438줄 → 43줄 shell | 입고 수령 Screen 내부 책임 분리 완료 |
| 주문자·판매·창고의 기존 조립 shell | desktop 조립 기반 완료, 모바일용 List·Detail·Action route 감사 필요 |

## 페이지별 완료 게이트

각 페이지는 다음을 모두 만족한 뒤 완료로 표시한다.

- 하나의 의미 route만 소유한다. 여러 `@page` 선언은 동일 화면의 언어·legacy alias처럼 의미가 완전히 같을 때만 허용한다.
- Route Page는 query·route parameter와 플랫폼 navigation shell만 조립하고 API 호출, `try/catch`, 업무 검증과 상태 전이를 직접 소유하지 않는다.
- Route Page는 `Ssalddel.Ui.Common`의 feature Screen 하나를 주 콘텐츠로 사용한다.
- Screen은 하나의 주된 사용자 목표를 가지며 loading, empty, error, retry, disabled와 인증 필요 상태를 제공한다.
- 공용 Screen은 `Ssalddel.WebApp`, `SsalddelApp` 또는 전문 앱 namespace와 서비스를 참조하지 않는다.
- 업무 route는 공용 route catalog 또는 navigation intent를 사용하고 `Ui.Common`에 새 URL literal을 추가하지 않는다.
- stable 업무 ID로 직접 열 수 있고, diagram·목록에서 넘어온 context와 돌아갈 위치를 복원한다.
- ViewModel/UseCase가 조회·검증·상태 전이를 맡고 View는 서버 성공 응답 전 상태를 확정하지 않는다.
- 공용 contract/UI 변경은 서버뿐 아니라 WebApp과 해당 모바일 소비 project를 빌드한다.
- route 유일성, route→Screen 구성, Web·모바일 의미 parity와 핵심 상태 전이를 자동 테스트로 고정한다.
- desktop과 390px mobile에서 overflow, fixed navigation, drawer/dialog/bottom sheet와 터치 영역을 실제 확인한다.
- 화면 변경은 PNG와 함께 `docs/Changes`에 남기고, 구조만 바뀌어 화면이 같으면 `간접 확인`과 미검증 범위를 명시한다.
- 운송·창고·판매 운영 효과는 기능 플래그와 `SsalddelExecution:Mode` 경계를 통과하며 0.0 기본값에서는 비활성이다.

## 다음 작업

`P0-3`에서 Web 전용 다이어그램 작업대를 `Ssalddel.Ui.Common`의 공용 Screen과 Web·모바일 `/diagram` Route Page로 분리했다. desktop은 palette sidebar, 모바일은 bottom sheet를 사용하고 `ledgerTemplate`, 선택 node, zoom, filter와 출발 page를 URL 문맥으로 복원한다. 과거 `/community/workspace?diagram=true` 링크는 공용 route로 호환 이동하며, 다이어그램은 관계 탐색만 맡고 구체 입력·승인·처리는 기존 3단계 업무 Screen으로 이동한다. 다음 수직 단위는 `P0-4` 커뮤니티 화면 복귀 문맥으로, 게시판과 다이어그램에서 3단계 Screen을 왕복할 때 선택·검색·스크롤 문맥을 공용 계약으로 정리한다.
