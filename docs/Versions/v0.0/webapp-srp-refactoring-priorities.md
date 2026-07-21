# WebApp 페이지 단일책임 리팩터링 우선순위

기준일: 2026-07-21

이 문서는 주문자, 커뮤니티, 창고, 운송, 판매 앱의 현재 페이지를 단일책임 원칙과 0.0 릴리즈 범위로 다시 평가한 작업 순서다. 파일 길이는 책임 혼합을 찾는 신호일 뿐이며, 실제 우선순위는 [0.0 집중 로드맵](./focus-roadmap.md)의 사용자 여정과 운영 경계를 먼저 따른다.

## 판단 기준

1. `게시 -> 직접 검색·선택 -> 상호 동의 -> 공동 원장 -> 완료 사례` 흐름을 완성하는가
2. 한 페이지가 화면 조립, 조회, 입력 검증, 상태 전이, 외부 효과를 함께 맡고 있는가
3. loading, empty, error, retry, disabled와 인증 전환 상태를 독립적으로 검증할 수 있는가
4. `SsalddelExecution:Mode`와 기능 플래그 없이 운송·보관·결제 같은 운영 효과를 만들지 않는가
5. 공용 UI 변경 뒤 WebApp과 MAUI 소비 앱을 함께 빌드하고 desktop·390px mobile에서 확인할 수 있는가
6. 현재 작업 트리의 다른 변경과 겹치지 않고 맥락별 commit으로 되돌릴 수 있는가

## 실행 순서

| 우선순위 | 문맥·현재 페이지 | 현재 신호 | 분리할 책임 | 완료 조건 |
| --- | --- | --- | --- | --- |
| `P0-1` 완료 | 커뮤니티 `PlatformCommunityPostComposer.razor` | 변경 전 794줄에서 조립 shell 215줄로 축소 | 머리글·게시판 조건, 상태/초안 폐기, 제목·본문, 판매 정보, 첨부 도구, 현재 문맥, 게시 설정, 표현 규칙 | 기존 저장·임시저장·첨부·예약·판매 전환 계약 유지, 공용 UI/WebApp/MAUI 빌드, desktop/mobile 렌더링 |
| `P0-2` | 커뮤니티 `PlatformCommunityPostList.razor` | 546줄, 깨끗한 작업 대상 | 조회 상태, 필터/정렬, 목록 행, 페이지 이동, 사용자 행동 | 공개 목록이 후속 업무 모듈 없이 동작하고 오류를 sample fallback으로 숨기지 않음 |
| `P0-3` | 커뮤니티 `PlatformCommunityHome.razor` | 736줄, ViewModel 수명은 이미 분리됨 | 공개 게시판 shell, 선택 문맥, 글쓰기 진입, 명시적으로 여는 연결 도구 | 기본 진입이 전통 게시판 목록을 유지하고 업무 도구가 자동 노출되지 않음 |
| `P0-4` | 커뮤니티 `CommunityGroupPurchaseWorkspace.razor` | 1,170줄, 현재 다른 작업과 겹침 | 모집 개요, 조건 협의, 참여 의사, 연락 동의, 가원장 전환 | 국내 공동구매 대표 파일럿의 직접 협의가 영속화되고 플랫폼 추천·거래 대리가 없음 |
| `P1-1` | 주문자 `OrdererRestaurantWorkspace.razor` | 303줄, 깨끗한 작업 대상 | 공개 탐색 조건, 검색 결과, 선택 상세, 로그인 보호 요청 | 초기 공개 정보와 보호 정보를 분리하고 수동 공급 요청만 생성 |
| `P1-2` | 주문자 `OrdererMartOrderRequestWorkspace.razor` | 260줄, 깨끗한 작업 대상 | 주문 입력, 금액/수량 요약, 제출 상태, 실패 복구 | 서버가 소유권과 현재 상태를 재검증하고 성공 뒤 같은 주문을 재조회 |
| `P1-3` | 판매 `OrderFulfillment.razor` | 631줄, 깨끗한 작업 대상 | 주문 목록/필터, 선택 상세, 이행 상태, 사용자 Command | 기능 플래그 뒤에서 수동 상태 전이만 제공하고 결제·정산을 활성화하지 않음 |
| `P1-4` | 판매 `ProductListings.razor` | 209줄, 깨끗한 작업 대상 | 판매상품 선택, 채널 계정 확인, 출품 초안, 결과 상태 | 정확한 계정 ID를 조회하고 외부 출품 효과는 명시적 실행 경계로 제한 |
| `P2-1` | 창고 `WorkBoard.razor` | 313줄, 깨끗한 작업 대상 | 작업 대기열, 필터, 선택 상세, 단계 handoff | 합의된 원장 참조만 읽고 Simulation에서 상태·오류·재시도를 검증 |
| `P2-2` | 창고 `SsalddelInboundReceivingWorkspace.razor` | 438줄, 깨끗한 작업 대상 | 예정 조회, 수령 확인, 불일치 입력, 저장 결과 | 입고 ID·권한·멱등성을 서버가 검증하고 같은 ID를 재조회 |
| `P2-3` | 운송 `DriverTransportDropoffPage.razor` | 429줄, 깨끗한 작업 대상 | 운송 요약, 하차 증빙, 예외, 완료 이동 | 기존 상차·통합 증빙 컴포넌트를 재사용하고 Operational 모드 없이 실행 효과 없음 |
| `P3` 보존 | 운송 `DriverRecommendations.razor` | 673줄이나 1.0 이후 추천 흐름 | 후보 표시와 설명 책임은 향후 분리 | 0.0에서 기본 비노출, 상대 추천·순위·자동 배차 확장 금지 |

## 현재 겹치는 파일의 처리

다음 파일은 이미 다른 맥락의 변경이 진행 중이므로, 그 변경이 commit되거나 인계되기 전에는 같은 파일을 다시 구조 변경하지 않는다.

| 문맥 | 파일 | 현재 처리 |
| --- | --- | --- |
| 커뮤니티 | `CommunityGroupPurchaseWorkspace.razor` | 현재 변경을 먼저 검증·commit한 뒤 `P0-4` 분리 |
| 주문자 | `OrdererFoodOrderWorkspace.razor` | 목록·상세·로그인·검색 분리가 진행 중이므로 해당 맥락에서 완료 |
| 주문자 | `OrdererMartCatalogWorkspace.razor` | 현재 카탈로그 변경 완료 뒤 책임 재감사 |
| 창고 | `SsalddelInboundRequestManager.razor` | 입고 요청 변경 완료 뒤 입력·목록·상세 책임 재감사 |
| 판매 | `SsalddelSalesPageComposer.razor` | 현재 작성기 변경 완료 뒤 wrapper와 저장 책임 재감사 |

## 페이지별 배포 게이트

각 페이지는 다음을 모두 만족한 뒤 완료로 표시한다.

- route 또는 최상위 component는 화면 영역과 workflow만 조립하며, 세부 입력과 표시 규칙을 직접 소유하지 않는다.
- ViewModel/UseCase가 조회·검증·상태 전이를 맡고 View는 서버 응답 전 상태를 확정하지 않는다.
- loading, empty, error, retry, disabled, 인증 필요 상태를 제공한다.
- 공용 contract/UI 변경은 서버 project뿐 아니라 WebApp과 해당 MAUI/전문 앱 소비 project를 빌드한다.
- 핵심 상태 전이와 책임 조립을 자동 테스트로 고정한다.
- desktop과 390px mobile에서 실제 렌더링, overflow, dialog/drawer, 터치 영역을 확인한다.
- 화면 변경 PNG와 검증 결과를 `docs/Changes`에 남긴다.
- 운송·창고·판매 운영 효과는 기능 플래그와 `SsalddelExecution:Mode` 경계를 통과하고 0.0 기본값에서는 비활성이다.

## 다음 작업

다음 깨끗한 세로 단위는 `PlatformCommunityPostList.razor`다. 목록 조회 상태와 필터·목록 행·행동을 분리하고, `/community`와 `/ko/community`의 공개 게시판 기본형을 실제 렌더링으로 다시 확인한다. 현재 겹치는 파일이 먼저 정리되면 국내 공동구매 대표 파일럿을 `P0-4`로 당겨 0.0-C 흐름을 이어간다.
