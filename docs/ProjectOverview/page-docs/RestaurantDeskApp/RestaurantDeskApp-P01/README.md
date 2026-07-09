# RestaurantDeskApp-P01 - 음식점 데스크 홈

[전체 화면 문서](../../README.md) / [RestaurantDeskApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

<img src="../../../assets/app-pages/RestaurantDeskApp/RestaurantDeskApp-P01.png" alt="RestaurantDeskApp-P01 화면 캡처" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | RestaurantDeskApp |
| 페이지 ID | RestaurantDeskApp-P01 |
| 라우트 | / |
| 소스 파일 | [RestaurantDeskApp/Components/Pages/Home.razor](../../../../../RestaurantDeskApp/Components/Pages/Home.razor) |
| 분류 | 보조 |
| 1.0 필수 연결 | 직접 연결 없음 |
| 캡처 상태 | 완료 |

## 왜 필요한가

이 화면은 음식점 데스크 홈을 담당하므로, 주 업무 화면을 보조하고 사용자가 다음 행동으로 이동할 수 있게 합니다.

## 사용자와 참여자

주 사용자: 음식점 또는 매장 운영자 / 보조 참여자: 주문자, 배달 기사, 관리자

이 화면은 음식/마트 주문 및 배달 워크플로우 안에서 음식점 데스크 홈 책임을 갖습니다. 화면 하나가 너무 많은 결정을 떠안지 않도록, 이 문서에서는 이 화면의 주 책임과 다른 화면으로 넘겨야 할 책임을 구분해 관리합니다.

## 화면에서 다루는 일

- 주 책임: 음식점 데스크 홈
- 사용자가 확인해야 하는 것: 이 화면에서 상태, 입력값, 다음 행동이 명확히 보이는지 확인합니다.
- 사용자가 조작해야 하는 것: 버튼, 입력, 선택, 업로드, 조회 같은 조작이 이 화면의 책임 안에 머무는지 확인합니다.
- 화면 밖으로 넘길 일: 다른 앱이나 관리자 화면에서 처리해야 하는 상태 변경은 이 화면에 과하게 넣지 않습니다.

## 다른 화면과의 관계

- 이전 화면: 없음
- 다음 화면: [RestaurantDeskApp-P02](../RestaurantDeskApp-P02/)
- 상위 화면: 없음
- 하위 화면: 없음

상호작용 관점에서는 다음 흐름을 우선 봅니다. 매장 또는 음식점 업무 상태는 주문자 화면, 배달 기사 추천, 관리자 음식 운영 화면과 연결될 수 있습니다.

## API 경로와 코드 연결

- 화면 소스: [RestaurantDeskApp/Components/Pages/Home.razor](../../../../../RestaurantDeskApp/Components/Pages/Home.razor)
- 클라이언트 서비스/계약: [RestaurantDeskApp/Services/I주문알림Service.cs](../../../../../RestaurantDeskApp/Services/I주문알림Service.cs), [RestaurantDeskApp/Services/RestaurantDeskSampleService.cs](../../../../../RestaurantDeskApp/Services/RestaurantDeskSampleService.cs), [RestaurantDeskApp/Services/주문알림Service.cs](../../../../../RestaurantDeskApp/Services/주문알림Service.cs)

| 구분 | 메서드 | API 경로 | 클라이언트/문서 근거 | 서버 근거 |
| --- | --- | --- | --- | --- |
| 워크플로우 문서 | - | `api/v1/admin/documents` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/documents/policies` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`PUT api/v1/admin/documents/policies/{documentCode}` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`GET api/v1/admin/documents` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`GET api/v1/admin/documents/logs` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs) |
| 워크플로우 문서 | - | `api/v1/admin/hr-employment-contracts` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/hr-employment-contracts` [Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs)<br>`GET api/v1/admin/hr-employment-contracts/{contractId:guid}` [Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs)<br>`POST api/v1/admin/hr-employment-contracts` [Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs)<br>`POST api/v1/admin/hr-employment-contracts/{contractId:guid}/sign` [Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrEmploymentContractsController.cs) |
| 워크플로우 문서 | - | `api/v1/admin/hr-participation-benefits` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/hr-participation-benefits` [Hongdal/Controllers/Admin/HumanResources/HrParticipationBenefitsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrParticipationBenefitsController.cs)<br>`POST api/v1/admin/hr-participation-benefits/transfer` [Hongdal/Controllers/Admin/HumanResources/HrParticipationBenefitsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrParticipationBenefitsController.cs) |
| 워크플로우 문서 | - | `api/v1/admin/hr-roles` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/hr-roles` [Hongdal/Controllers/Admin/HumanResources/HrRolesController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrRolesController.cs)<br>`POST api/v1/admin/hr-roles` [Hongdal/Controllers/Admin/HumanResources/HrRolesController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrRolesController.cs)<br>`DELETE api/v1/admin/hr-roles/{assignmentId:guid}` [Hongdal/Controllers/Admin/HumanResources/HrRolesController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/HrRolesController.cs) |
| 워크플로우 문서 | - | `api/v1/admin/hr-social-insurance-filings` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/hr-social-insurance-filings` [Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs)<br>`GET api/v1/admin/hr-social-insurance-filings/{id:guid}` [Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs)<br>`POST api/v1/admin/hr-social-insurance-filings/assess` [Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs)<br>`POST api/v1/admin/hr-social-insurance-filings` [Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs](../../../../../Hongdal/Controllers/Admin/HumanResources/SocialInsuranceFilingsController.cs) |
| 워크플로우 문서 | - | `api/v1/admin/hs-codes` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/hs-codes` [Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs](../../../../../Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs)<br>`PUT api/v1/admin/hs-codes/{entryId:long}/business-category` [Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs](../../../../../Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs)<br>`POST api/v1/admin/hs-codes/{entryId:long}/risk-tags` [Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs](../../../../../Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs)<br>`PUT api/v1/admin/hs-codes/risk-tags/{tagId:long}` [Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs](../../../../../Hongdal/Controllers/Admin/Customs/HS코드운영Controller.cs) |
| 워크플로우 문서 | - | `api/v1/admin/orderer/group-purchase-` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | - |
| 워크플로우 문서 | - | `api/v1/admin/transports` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/transports` [Hongdal/Controllers/Admin/03_진행/운송진행관리Controller.cs](../../../../../Hongdal/Controllers/Admin/03_진행/운송진행관리Controller.cs)<br>`GET api/v1/admin/transports/events` [Hongdal/Controllers/Admin/03_진행/운송진행관리Controller.cs](../../../../../Hongdal/Controllers/Admin/03_진행/운송진행관리Controller.cs) |
| 워크플로우 문서 | - | `api/v1/community/activity-signals` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/community/activity-signals` [Hongdal/Controllers/Common/커뮤니티활동신호Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티활동신호Controller.cs) |
| 워크플로우 문서 | - | `api/v1/community/posts` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/community/posts` [Hongdal/Controllers/Common/커뮤니티게시글Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티게시글Controller.cs)<br>`POST api/v1/community/posts` [Hongdal/Controllers/Common/커뮤니티게시글Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티게시글Controller.cs)<br>`GET api/v1/community/posts/{id:long}` [Hongdal/Controllers/Common/커뮤니티게시글Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티게시글Controller.cs)<br>`POST api/v1/community/posts/{id:long}/attachments` [Hongdal/Controllers/Common/커뮤니티게시글Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티게시글Controller.cs) |
| 워크플로우 문서 | - | `api/v1/community/votes` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/community/votes` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs)<br>`GET api/v1/community/votes/{voteId:guid}` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs)<br>`POST api/v1/community/votes` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs)<br>`POST api/v1/community/votes/{voteId:guid}/votes` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs) |
| 워크플로우 문서 | - | `api/v1/customs` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `POST api/v1/customs/consents` [Hongdal/Controllers/Common/통관연동Controller.cs](../../../../../Hongdal/Controllers/Common/통관연동Controller.cs) |
| 워크플로우 문서 | - | `api/v1/driver/dispatch-actions` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `POST api/v1/driver/dispatch-actions/{requestId}/accept` [Hongdal/Controllers/Driver/03_Action/기사배차액션Controller.cs](../../../../../Hongdal/Controllers/Driver/03_Action/기사배차액션Controller.cs)<br>`POST api/v1/driver/dispatch-actions/{requestId}/reject` [Hongdal/Controllers/Driver/03_Action/기사배차액션Controller.cs](../../../../../Hongdal/Controllers/Driver/03_Action/기사배차액션Controller.cs)<br>`POST api/v1/driver/dispatch-actions/{requestId}/cancel-acceptance` [Hongdal/Controllers/Driver/03_Action/기사배차액션Controller.cs](../../../../../Hongdal/Controllers/Driver/03_Action/기사배차액션Controller.cs) |
| 워크플로우 문서 | - | `api/v1/driver/recommendations` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/driver/recommendations` [Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs](../../../../../Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs)<br>`GET api/v1/driver/recommendations/idle` [Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs](../../../../../Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs)<br>`GET api/v1/driver/recommendations/driving` [Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs](../../../../../Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs)<br>`GET api/v1/driver/recommendations/search` [Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs](../../../../../Hongdal/Controllers/Driver/02_Recommendation/기사배차추천Controller.cs) |
| 워크플로우 문서 | - | `api/v1/driver/transports` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/driver/transports` [Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs](../../../../../Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs)<br>`GET api/v1/driver/transports/current` [Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs](../../../../../Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs)<br>`GET api/v1/driver/transports/{id:long}` [Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs](../../../../../Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs)<br>`POST api/v1/driver/transports/{id:long}/arrive-pickup` [Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs](../../../../../Hongdal/Controllers/Driver/05_Settings/기사운송진행Controller.cs) |
| 워크플로우 문서 | - | `api/v1/files` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `POST api/v1/files/upload` [Hongdal/Controllers/Common/파일업로드Controller.cs](../../../../../Hongdal/Controllers/Common/파일업로드Controller.cs) |
| 워크플로우 문서 | - | `api/v1/orderer/group-purchase-logistics-workflows` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/orderer/group-purchase-logistics-workflows/resolve` [Hongdal/Controllers/Orderer/공동구매물류워크플로우Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매물류워크플로우Controller.cs) |
| 워크플로우 문서 | - | `api/v1/orderer/group-purchase-overseas-shipments` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/orderer/group-purchase-overseas-shipments/lookup` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs)<br>`GET api/v1/orderer/group-purchase-overseas-shipments/import-logistics-references` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs)<br>`POST api/v1/orderer/group-purchase-overseas-shipments/import-logistics-normalization-simulation` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs)<br>`GET api/v1/orderer/group-purchase-overseas-shipments/lookup-normalized` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs) |

추가 API 후보 6개는 관련 서비스 파일에서 계속 확인합니다. 이 화면 README는 화면 책임 판단에 필요한 대표 경로만 먼저 노출합니다.

검증할 때는 이 화면이 직접 메모리 데이터만 보는지, 위 API 응답을 받아 상태를 표시하는지, 실패했을 때 사용자가 다음 행동을 알 수 있는지 확인합니다.

## 보안과 개인정보 점검

위치, 주소, 운행 상태는 최소 공개와 최신성 표시가 필요합니다.

## 캡처와 문서 상태

현재 캡처는 문서용 캡처 호스트 또는 기존 캡처 파일 기준으로 확인한 화면입니다.

이미지 파일을 다시 생성하면 이 README는 같은 경로의 이미지를 참조하므로 자동으로 최신 캡처를 보여줍니다.

## 보완 메모

- 화면 설명이 실제 구현과 달라지면 이 문서와 app-page-catalog.md를 함께 갱신합니다.
- 화면이 1.0 필수 워크플로우에 포함되면 hongdal-v1-required-pages.md에도 반영합니다.
- 렌더링이 깨지거나 내용이 잘리면 캡처 스크립트와 실제 화면 레이아웃을 같이 확인합니다.