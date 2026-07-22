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

주력 가능성이 높은 모바일을 기준 화면으로 둔다. 390px 단일 열, stable-ID deep link, 뒤로가기 문맥 복원, loading·retry·중복 탭 방지와 48px 주 행동 터치 영역을 먼저 만족시키고, desktop은 같은 Screen과 상태를 넓은 폭에서 나란히 조합한다. Razor 화면 재사용은 현재 수단일 뿐이며 Contracts·UseCase·ViewModel은 이후 native mobile UI에서도 사용할 수 있도록 플랫폼 UI에 종속시키지 않는다.

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
| `P0-4` 완료 | 커뮤니티 화면 복귀 문맥 | 변경 전에는 board query가 화면 상태 전체를 표현하지 못하고 diagram node에서 3단계 업무 화면으로 이동한 뒤 돌아갈 공용 계약이 없었음 | 공용 `PageNavigationContext`, `CommunityBoardNavigationContext`와 Web·모바일 route adapter | 게시판의 board·검색·필터·보기·focus와 다이어그램의 원장·node·zoom·filter·출발 page를 URL로 복원하고, 3단계 Screen이 안전한 local `from`으로 원래 화면에 돌아가며 deep link test를 통과함 |
| `P1-1` 완료 | 운송 요청 작성 | 변경 전에는 Web 한 화면 606줄, Web mode route 4개, 모바일 997줄 복합 Panel이 서로 다른 workflow를 가졌음 | 화물, 운송, 절차, 최종 확인 공용 Screen | 같은 draft와 validation을 공유하고 Web은 adaptive layout, 모바일은 단계 navigation만 담당하며 diagram·커뮤니티 출발 문맥을 단계 사이에 보존함 |
| `P1-2` 완료 | 운송 요청 상세 | 변경 전에는 Web 828줄과 모바일 988줄의 독립 monolith가 요약·진행·결제·증빙을 함께 수행했음 | 요약, 진행 이력, 결제, 증빙 Screen | 같은 request ID와 서버 원본을 사용하고 결제·증빙은 명시적 별도 route에서만 실행함 |
| `P1-3` 완료 | 입고 요청 | 변경 전에는 `SsalddelInboundRequestManager` 한 route가 창고 등록·입고 신청·목록·상세·완료를 함께 수행했음 | 목록, 신규 신청, 상세, 입고 완료와 별도 창고 등록 Screen | Web·모바일이 같은 route 계약·공용 Screen을 사용하고 성공 뒤 같은 inbound ID를 재조회하며 desktop·390px 실제 검증을 통과함 |
| `P1-4` 완료 | 창고·판매 master-detail-action | 변경 전 입고 검수와 피킹, 마트 상품은 한 query 화면에 목록·상세·후기 저장을 함께 배치했고 판매 주문은 Web 읽기 원장과 모바일 Simulation 의미가 달랐음 | List, Detail, Action Screen과 adaptive desktop composition | 영속 판매 주문은 Web·모바일 공통 목록·stable-ID 상세로 통합하고, 로컬 Simulation도 허브·조회·stable-key Action route로 분리해 Command 뒤 같은 원장을 재조회함 |
| `P2-1` 완료 | 개인 공간·꾸미기 | 변경 전에는 Web 개인 route multiplexer가 꾸미기 상점·상품까지 해석하고 모바일만 별도 상점·상세·checkout을 가졌음 | 개인 관리, 꾸미기 상점, stable-key 상품 상세, FakePG checkout 공용 Screen | Web·모바일이 같은 canonical route·Screen을 사용하고 구매·보유권 생성은 로그인 후 checkout에만 남김 |
| `P2-2` | `/shipper` 홈 | Web은 링크 디렉터리, 모바일은 커뮤니티 홈·인증·dashboard를 수행함 | 공용 화주 허브 Screen과 플랫폼 shell | 같은 route의 주된 목표가 같고 1.0 이후 기능은 flag 뒤에 유지됨 |
| `P3` 보존 | 기사 추천·정산·통관 등 1.0 이후 화면 | 큰 Web 전용 화면이 남아 있으나 0.0 범위가 아님 | 재활성화 시 List·Detail·Action Screen으로 분리 | 0.0 기본 비노출, 추천·자동 배차·결제·정산 운영 효과 확장 금지 |

## 커뮤니티 목표 route 지도

| 단계 | 사용자 목표 | Route |
| --- | --- | --- |
| 1단계 | 공개 커뮤니티·게시판 탐색 | `/community`, `/community/boards?boardKey=...&q=...&filter=...&view=...&focus=...` |
| 2단계 | 업무·원장 목적지 탐색 | `/community/workspace` |
| 2단계 | 공동 원장 초안 작성 | `/community/ledgers/new` |
| 2단계 | 게시판 개설·관리 | `/community/boards/manage` |
| 2단계 | 참여자·업무 관계 확인 | `/diagram?ledgerTemplate=...&node=...&zoom=...&filter=...&from=...` |
| 3단계 | 새 글 작성 | `/community/write?board=...&from=...` |
| 3단계 | 추천 글 목록 | `/community/posts/recommended?board=...&from=...` |
| 3단계 | 추천 sample 글 상세 | `/community/posts/recommended/detail?seed=...&from=...` |
| 3단계 | 영속 게시글 상세 | `/community/posts/{PostId:long}?from=...` |
| 3단계 | 공동구매 목록·개설 | `/community/group-purchase`, `/community/group-purchase/new` |
| 3단계 | 공동구매 상세·참여·공급자·협의 | `/community/group-purchase/{CampaignId:guid}`와 하위 `/participation`, `/suppliers`, `/negotiation` |
| 3단계 | 공동구매 이의·결의·서명 | campaign 하위 `/objections`, `/resolution`, `/signature` |
| 3단계 | 공동구매 배송 정보·이행 초안 | campaign 하위 `/delivery-options`, `/fulfillment-draft` |

## 운송 의뢰 작성 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| Web adaptive 조립 | 네 책임을 한 문맥에서 최종 조립 | `/shipper/request` |
| 화물 정보 | 품목·적재·수량·중량·부피 입력 | `/shipper/request/cargo` |
| 운송 정보 | 상차·하차·연락 대상·서비스 조건 입력 | `/shipper/request/transport` |
| 절차·결제 정보 | 차량·운임·부가비용·알선 경계 검토 | `/shipper/request/procedure` |
| 최종 확인 | 공통 validation·실행 경계 확인과 명시적 저장·등록 | `/shipper/request/review` |
| legacy alias | 기존 최종 요약 링크의 같은 의미 호환 | `/shipper/request/summary` |
| CSV 일괄등록 | 단건 작성과 분리된 여러 의뢰 준비 | `/shipper/request/bulk` |

`SsalddelApp`의 `/shipper/request`는 query 문맥을 보존해 화물 정보 단계로 호환 이동한다. 네 단계 Route Page는 공용 Screen과 scoped draft만 조립하며 서버/sample adapter 호출은 앱 PageViewModel이 맡는다.

## 운송 의뢰 상세 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| 요약 | 의뢰·경로·비용과 현재 상태 확인 | `/shipper/request/{RequestId}` |
| 진행 이력 | 결제·배차·수락·상차·하차·정산 순서 확인 | `/shipper/request/{RequestId}/timeline` |
| 결제 | 수납 조건 확인과 허용된 개발 환경의 명시적 FakePG 실행 | `/shipper/request/{RequestId}/payment` |
| 증빙 | 상차·하차/POD·인수증·세무 증빙 연결 상태 확인 | `/shipper/request/{RequestId}/proofs` |
| legacy 조회 | query ID를 stable-ID 요약 route로 호환 연결 | `/shipper/request/detail?id=...` |

네 Route Page는 같은 `ShipperRequestDetailPresentation`과 request ID를 사용한다. Web PageViewModel은 인증·기능 플래그·서버 원장 조회만 조율하고, 모바일 PageViewModel은 기존 transport adapter·원장 observer와 명시적인 payment route의 FakePG 개발 흐름만 조율한다. 증빙 Screen은 원본 저장소의 연결 상태를 읽기 전용으로 표시하며 조회만으로 증빙을 생성하거나 완료 처리하지 않는다.

## 입고 요청 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| 목록 | 입고 원장 검색·필터와 대상 선택 | `/shipper/inbound/requests` |
| 신규 신청 | 계약 기반 입고 예정 한 건 검토·등록 | `/shipper/inbound/requests/new` |
| 상세 | stable inbound ID의 원장·계약 스냅샷 재조회 | `/shipper/inbound/requests/{InboundId:long}` |
| 입고 완료 | 실제 수량·불량·보관 위치 확인 후 명시적 재고 전환 | `/shipper/inbound/requests/{InboundId:long}/complete` |
| 창고 등록 | 입고 신청과 분리된 창고 기본정보 한 건 등록 | `/shipper/warehouses/new` |

다이어그램 창고 후보는 신규 신청 route에 초안만 전달하며 API를 직접 실행하지 않는다. 입고 신청과 완료 성공 뒤에는 같은 inbound ID를 구성된 adapter에서 다시 조회한다. 현장 임시 입고는 안내 동의·멱등 요청 ID가 있는 입고상품 수령 화면, 주문 자동 입고 예정은 주문 workflow에 남겨 일반 신청 route가 우회 생성하지 못하게 한다.

## 입고 검수 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| 목록 | 접근 가능한 검수 대상 검색·필터와 선택 | `/work/inbound/inspection` |
| 상세 | stable inbound item ID의 입고·재고·검수 상태 읽기 | `/work/inbound/inspection/{InboundItemId:long}` |
| 검수 실행 | 실제 수량·불량 수량과 네 가지 현장 확인 뒤 명시적 저장 | `/work/inbound/inspection/{InboundItemId:long}/record` |
| legacy Web alias | 통합 Web의 기존 경로 호환 | `/warehouse/work/inbound/inspection[/...]` |
| legacy query | query ID를 stable-ID 상세 route로 호환 연결 | `/work/inbound/inspection?inboundItemId=...` |

목록과 상세 Screen은 `I입고검수페이지Service`의 Command를 호출하지 않는다. 검수 실행 Screen만 `입고검수실행ViewModel`을 통해 저장하고 성공 뒤 같은 inbound item ID를 다시 조회한다. 목록 검색·상태·페이지는 상세와 실행 route에 보존되며 Web과 `WarehouseManagerApp`이 같은 공용 Screen을 조립한다.

실제 Web 검증에서 목록 2건의 stable-ID 상세 이동, 실행 route의 네 확인 항목 저장 gate, desktop·390px horizontal overflow 없음과 mobile navigation 2열·58px를 확인했다. 추적 설정의 기능 플래그 기본값은 유지하고 검증 프로세스에서만 `WarehouseFulfillmentWorkflow`를 활성화했으며 실제 저장 Command는 실행하지 않았다. 대표 PNG와 상세 결과는 [입고 검수 Route·공용 Screen 단일책임 분리](../../Changes/2026-07-22-inbound-inspection-route-srp.md)에 기록했다.

## 피킹 작업 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| 목록 | 접근 가능한 피킹 작업 검색·상태 필터와 대상 선택 | `/work/picking-batch` |
| 상세 | stable task key의 상품·수량·적재대·담당·현재 상태 읽기 | `/work/picking-batch/{TaskKey}` |
| 피킹 실행 | 대기 작업 시작 또는 진행 중 작업의 적재대·상품·전체 수량 확인 뒤 완료 | `/work/picking-batch/{TaskKey}/execute` |
| legacy Web alias | 통합 Web의 기존 경로 호환 | `/warehouse/work/picking-batch[/...]` |
| legacy query | query key를 stable-key 상세 route로 호환 연결 | `/work/picking-batch?taskKey=...` |

목록 Screen은 `피킹작업목록ViewModel`, 상세 Screen은 `피킹작업상세ViewModel`만 사용해 Command 입력을 소유하지 않는다. 실행 Screen만 `피킹작업실행ViewModel`을 통해 시작·완료 Command를 호출하고 성공 뒤 목록 전체가 아니라 같은 task key 상세 한 건만 다시 조회한다. 목록 검색·상태·페이지와 안전한 `from`은 상세와 실행 route에 보존되며 Web과 `WarehouseManagerApp`이 같은 공용 Screen을 조립한다.

실제 Web 검증에서 stable-key 상세와 실행 route, legacy query·Web alias, desktop·390px horizontal overflow 없음과 mobile navigation 2열·58px를 확인했다. 실행 화면은 확인 폼을 작업 요약보다 먼저 표시하고 적재대·상품·전체 수량 조건이 모두 충족된 뒤에만 완료 버튼을 활성화했다. 추적 설정의 기능 플래그 기본값은 유지하고 검증 프로세스에서만 `WarehouseFulfillmentWorkflow`를 활성화했으며 실제 시작·완료 Command는 실행하지 않았다. 대표 PNG와 상세 결과는 [피킹 작업 Route·공용 Screen 단일책임 분리](../../Changes/2026-07-22-picking-task-route-srp.md)에 기록했다.

## 마트 공개 상품 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| 목록 | 공개 상품 검색·판매 가능 조건·서버 페이징 | `/food/mart` |
| 상세 | stable product ID 한 건의 공개 설명·재고 투영·구매 근거 읽기 | `/food/mart/products/{ProductId}` |
| 후기 작성 | 완료 원장 참여자의 명시적 공개 구매후기 저장 | `/food/mart/reviews/{ProductId}` |
| 비구속 주문 요청 | 한 공개 상품의 주문 의향 저장과 같은 요청 ID 재조회 | `/food/mart/order/{ProductId}` |
| legacy Web alias | 통합 Web의 기존 목록·상세·후기·주문 경로 호환 | `/orderer/mart[/...]` |
| legacy query | query ID를 stable-ID 상세·주문 route로 호환 연결 | `/food/mart?productId=...`, `/food/mart/order?productId=...` |

409줄 markup과 148줄 code-behind의 기존 `OrdererMartCatalogWorkspace`는 제거했다. 목록 Screen은 `마트공개상품목록ViewModel`, 상세 Screen은 `마트공개상품상세ViewModel`만 사용해 저장 입력을 소유하지 않는다. 후기 Screen만 `마트공개상품후기PageViewModel`을 통해 후기를 저장하고 성공 뒤 목록이 아니라 같은 product ID 상세 한 건만 다시 조회한다. 기존 주문 요청 Screen은 공용 접근 frame 아래에서 인증·한 상품 작성·같은 request ID 영수증만 조립한다.

Web과 `OrdererApp`은 같은 canonical route와 공용 Screen을 사용하고 기존 `/orderer/mart`·query 링크를 보존한다. route 계약, 안전한 `from`, 검색·판매 가능 조건·페이지 복귀, capability의 ReadOnly·PlatformPersistence 분리, route→Screen 구성과 같은 ID 재조회 테스트를 포함한 clean commit 기준 전체 테스트 2,528개가 통과했다. `Ssalddel.WebApp`, `OrdererApp`, `SsalddelApp`, `SsalddelAdminApp` 소비 빌드도 경고·오류 없이 통과했다.

실제 Web 검증에서 1280px 목록, stable-ID 상세·후기·주문 요청, legacy query redirect와 390px 후기·주문 로그인 경계를 확인했다. 390px navigation은 2열·63px, 마트 주 행동은 48px 이상이고 horizontal overflow와 final console 오류가 없었다. 주문 로그인 터치 영역도 최소 48px로 보완했다. 대표 PNG와 상세 결과는 [마트 공개 상품 Route·공용 Screen 단일책임 분리](../../Changes/2026-07-22-mart-product-route-srp.md)에 기록했다. 공통 Web 언어 전환의 31px 버튼은 다음 셸 모바일 감사 항목이다.

## 판매 주문 목표 route 지도

| 화면 | 사용자 목표 | Route |
| --- | --- | --- |
| 영속 주문 목록 | 인증된 판매 주문 원장 검색·상태·동기화 범위·페이지 확인 | `/shipper/sales/orders` |
| 영속 주문 상세 | stable order ID 한 건의 주문·출고 투영 읽기 | `/shipper/sales/orders/{OrderId:long}` |
| legacy query | query ID와 목록 문맥을 stable-ID 상세 route로 호환 연결 | `/shipper/sales/orders?orderId=...&q=...&scope=...&status=...&page=...` |
| 로컬 이행 허브 | 상태를 바꾸지 않고 각 Simulation 목표로 이동 | `/shipper/sales/fulfillment` |
| 샘플 반영 | 외부 수집 없이 비식별 주문을 로컬 원장에 명시적으로 준비 | `/shipper/sales/fulfillment/samples` |
| Simulation 주문 목록 | 검색·범위·상태로 로컬 주문 후보 확인 | `/shipper/sales/fulfillment/orders` |
| Simulation 주문 상세 | opaque stable 주문 key 한 건만 읽고 목록 문맥으로 복귀 | `/shipper/sales/fulfillment/orders/{OrderKey}` |
| 재고·입고 신호 | 로컬 재고 snapshot과 입고 필요 검토 신호만 읽기 | `/shipper/sales/fulfillment/inventory` |
| 피킹 목록 | task 목록을 읽고 정확한 task ID 화면 선택 | `/shipper/sales/fulfillment/picking` |
| 피킹 Action | stable task ID 한 건에만 스캔·보류·취소 적용 | `/shipper/sales/fulfillment/picking/{TaskId:long}` |
| 포장 목록 | task 목록을 읽고 정확한 task ID 화면 선택 | `/shipper/sales/fulfillment/packing` |
| 포장 Action | stable task ID 한 건에만 시작·완료 적용 | `/shipper/sales/fulfillment/packing/{TaskId:long}` |
| 입고 알림 정책 | 판매자별 로컬 동의·발송 의도만 변경 | `/shipper/sales/fulfillment/restock-policy` |

기존에는 Web의 `/shipper/sales/orders`가 인증된 영속 읽기 원장이지만 `SsalddelApp`의 같은 route는 `InMemoryShipperStore` 기반 Simulation Command 화면이었다. 이제 두 앱의 canonical 주문 route는 같은 `ShipperSalesOrderWorkspace`를 목록 또는 상세 mode로 조립하고, 모바일의 실행성 Simulation은 capability까지 분리된 `/shipper/sales/fulfillment`에 둔다. 목록 검색·동기화 범위·상태·페이지는 상세의 안전한 `from`에 보존되며 기존 query ID는 replace redirect로 stable-ID route에 연결된다.

실제 Web 검증에서 1280px 목록과 390px stable-ID 상세의 인증 경계, legacy query redirect와 목록 복귀 문맥을 확인했다. 390px에서 horizontal overflow가 없고 새 뒤로가기 동작은 48px 터치 영역을 만족한다. 인증된 실제 주문 데이터나 Command는 사용하지 않았다. 대표 PNG와 상세 결과는 [판매 주문 조회 Route 의미·단일책임 정렬](../../Changes/2026-07-22-sales-order-mobile-route-srp.md)에 기록한다. 공통 Web 언어 전환의 31px 버튼은 별도 셸 후속 항목이다.

`/shipper/sales/fulfillment`는 이제 상태를 바꾸지 않는 목표 허브다. 주문·재고·피킹·포장·정책은 독립 Route Page를 가지며, 목록 route는 읽기 전용이고 피킹·포장 Command는 주소의 stable task ID 화면에서만 실행한다. 존재하지 않는 key는 다른 주문이나 task로 자동 대체하지 않는다. 실제 MAUI Windows 앱에서 desktop과 플랫폼 최소 501px 좁은 창을 확인했고, 좁은 창의 route 이동이 이전 스크롤 위치를 이어받던 문제도 공용 상단 복원으로 보완했다. Android·iOS 실기기 확인은 배포 대상이 정해질 때 별도 수행한다.

## 개인 공간·꾸미기 목표 route 지도

| 화면 | 사용자 목표 | Route | 효과 경계 |
| --- | --- | --- | --- |
| 개인 공간 | 내 글·참여·원장·알림·사용 설정 관리 | `/community/me`, `/community/me/{SectionKey}` | 로그인 기반 개인 관리 |
| 내 꾸미기 관리 | 현재 보유·적용 상태 확인과 상점 이동 | `/community/me/decorations` | 기존 보유 항목의 로컬 적용 선택 |
| 꾸미기 상점 | 공개 상품 분류·탐색 | `/community/decorations` | 읽기 전용 |
| 상품 상세 | stable product key의 미리보기·가격·사용 범위 확인, 이미 보유한 항목의 적용·해제 | `/community/decorations/products/{ProductKey}` | 로컬 선택 저장, 구매·보유권 생성 없음 |
| FakePG checkout | 개발 결제 조건에 동의하고 한 상품의 보유권 기록 | `/community/decorations/checkout/{ProductKey}` | 로그인 필요, Simulation, 실제 승인·청구·정산 없음 |
| legacy 호환 | 기존 상품·checkout 링크를 canonical route로 교체 이동 | `/community/decorations/{ProductKey}`와 `/{ProductKey}/checkout` | 새 효과 없음 |

Web 개인 Page는 더 이상 현재 URL을 해석해 상점 또는 상품 상세로 mode 전환하지 않는다. Web과 `SsalddelApp`의 얇은 Route Page는 `Ssalddel.Ui.Common`의 상점·상품·checkout Screen을 동일하게 조립한다. 상점은 checkout adapter를 알지 못하고, 상품 상세는 보유권을 생성하지 않으며, checkout만 플랫폼별 `ICommunityDecorationPurchaseClient`를 호출한다.

Web sample adapter와 모바일 adapter는 로그인하지 않은 사용자의 보유권을 기록하지 않는다. Web은 브라우저 세션에만 개발용 보유 상태를 남기고, 모바일은 서버 지원 노드 스티커만 기존 FakePG API·계정 보유권과 동기화한다. 지원되지 않은 테마 상품은 앱 실행 동안의 로컬 Simulation에 머물며 실제 카드 승인·청구·창작자 정산은 실행하지 않는다.

실제 Web 검증에서 390px 상점·상품·checkout의 단일 열, 48px 이상 주 행동, 가로 넘침 없음과 legacy replace redirect를 확인했다. 상품 상세는 모바일에서 큰 미리보기보다 제목·가격·결제 행동을 먼저 배치하고 desktop 상점은 같은 Screen을 3열로 확장한다. 대표 PNG와 세부 결과는 [개인 공간·꾸미기 Route 의미·단일책임 정렬](../../Changes/2026-07-22-community-decoration-route-srp.md)에 기록했다. 공통 Web 모바일 머리말의 제목 줄바꿈은 `P2-2` 셸 감사에서 함께 다룬다.

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

`P1-4`의 첫 수직 단위인 입고 검수는 목록, stable-ID 상세와 검수 실행 Route Page로 분리해 전체 완료 게이트를 통과했다.

두 번째 수직 단위인 피킹 작업은 기존 query 선택과 복합 Workspace를 제거하고 목록, stable-key 상세, 실행 Route Page로 분리했다. 목록·상세는 읽기 전용이며 실행 route만 시작·완료 Command를 호출하고 같은 task key 한 건을 재조회한다. 자동 테스트, 소비 앱 빌드와 실제 desktop·390px 완료 게이트를 통과했다.

세 번째 수직 단위인 마트 공개 상품은 기존 query master-detail-review Workspace를 제거하고 목록, stable-ID 상세, 후기 작성과 기존 주문 요청 Route Page로 분리했다. 목록·상세는 읽기 전용이며 후기 route만 공개 후기 저장 뒤 같은 product ID를 다시 조회하고, 주문 route는 비구속 요청 뒤 같은 request ID를 다시 조회한다. 자동 테스트, 소비 앱 빌드, 실제 desktop·390px와 PNG 완료 게이트를 통과했다.

네 번째 수직 단위인 판매 주문은 Web의 영속 읽기 원장과 모바일의 로컬 Simulation이 같은 URL을 사용하던 충돌을 제거했다. `/shipper/sales/orders`와 stable-ID 상세는 Web·모바일 공용 읽기 Screen이며, 기존 모바일 Simulation은 `/shipper/sales/fulfillment`로 이동했다. route 계약, legacy query, 안전한 목록 복귀와 capability를 자동 테스트로 고정하고 실제 desktop·390px 인증 경계를 확인했다.

다섯 번째 수직 단위인 로컬 판매 주문 이행 Simulation은 복합 탭을 상태 변경 없는 허브, 주문·재고 목록, stable 주문 상세, 피킹·포장 목록과 stable task Action, 알림 정책 route로 분리했다. route 계약, capability, 개인정보 비노출, 48px 주 행동과 새 route 상단 복원을 자동 테스트로 고정하고 실제 desktop·501px MAUI Windows 화면을 확인했다. 외부 주문 수집, 실제 재고 예약·차감, 메시지 발송, 출고·운송·결제·정산은 계속 비활성이다.

여섯 번째 수직 단위인 개인 공간·꾸미기는 Web 개인 route multiplexer에서 상점·상품 해석을 제거하고, Web·모바일의 상점·stable-key 상품·FakePG checkout을 같은 canonical route와 공용 Screen으로 통합했다. 상품 상세의 로컬 적용 선택과 checkout의 로그인·Simulation 보유권 생성 경계를 분리하고 기존 주소는 replace redirect로 보존했다.

다음 우선순위는 `P2-2` `/shipper` 홈이다. Web 링크 디렉터리와 모바일 커뮤니티 홈·인증·dashboard가 같은 주소에서 서로 다른 목표를 갖는 문제를 공용 화주 허브 Screen과 플랫폼 shell로 정렬한다. 이때 공통 Web 모바일 머리말의 제목 줄바꿈과 48px 미만 언어 전환도 셸 책임으로 함께 감사한다.
