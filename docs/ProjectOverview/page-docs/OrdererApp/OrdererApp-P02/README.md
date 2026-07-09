# OrdererApp-P02 - 공동구매 의사 표시/집단화

[전체 화면 문서](../../README.md) / [OrdererApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

<img src="../../../assets/app-pages/OrdererApp/OrdererApp-P02.png" alt="OrdererApp-P02 화면 캡처" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | OrdererApp |
| 페이지 ID / 제목 | OrdererApp-P02 - 공동구매 의사 표시/집단화 |
| 라우트 | /group-purchase |
| 소스 파일 | [OrdererApp/Components/Pages/GroupPurchaseIntent.razor](../../../../../OrdererApp/Components/Pages/GroupPurchaseIntent.razor) |
| 분류 | 확장 |
| 1.0 필수 연결 | 직접 연결 없음 |
| 캡처 상태 | 완료 |

## 왜 필요한가

이 화면은 공동구매 의사 표시/집단화을 담당하므로, 1.0 이후의 해외 물류, 창고, 주문자 집단, 판매채널 확장을 화면 단위로 검증하기 위해 필요합니다.

## 사용자와 참여자

주 사용자: 주문자, 공동구매 참여자 / 보조 참여자: 집단 운영자, 화주 역할 대행자, 관리자

이 화면은 공동주문/주문자 집단 워크플로우 안에서 공동구매 의사 표시/집단화 책임을 갖습니다. 화면 하나가 너무 많은 결정을 떠안지 않도록, 이 문서에서는 이 화면의 주 책임과 다른 화면으로 넘겨야 할 책임을 구분해 관리합니다.

## 화면에서 다루는 일

- 주 책임: 공동구매 의사 표시/집단화
- 사용자가 확인해야 하는 것: 이 화면에서 상태, 입력값, 다음 행동이 명확히 보이는지 확인합니다.
- 사용자가 조작해야 하는 것: 버튼, 입력, 선택, 업로드, 조회 같은 조작이 이 화면의 책임 안에 머무는지 확인합니다.
- 화면 밖으로 넘길 일: 다른 앱이나 관리자 화면에서 처리해야 하는 상태 변경은 이 화면에 과하게 넣지 않습니다.

## 다른 화면과의 관계

- 이전 화면: [OrdererApp-P01 - 주문자 홈](../OrdererApp-P01/)
- 다음 화면: [OrdererApp-P03 - 주문자 화물 주문](../OrdererApp-P03/)
- 상위 화면: 없음
- 하위 화면: 없음

상호작용 관점에서는 다음 흐름을 우선 봅니다. 주문자의 의사 표시와 주문 상태는 공동주문 집단화, 해외 물류, 국내 운송, 판매채널 이행 흐름으로 이어질 수 있습니다.

## API 경로와 코드 연결

- 화면 소스: [OrdererApp/Components/Pages/GroupPurchaseIntent.razor](../../../../../OrdererApp/Components/Pages/GroupPurchaseIntent.razor)
- 클라이언트 서비스/계약: [OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs](../../../../../OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs)

| 구분 | 메서드 | API 경로 | 클라이언트/문서 근거 | 서버 근거 |
| --- | --- | --- | --- | --- |
| 워크플로우 문서 | - | `api/v1/admin/documents` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/documents/policies` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`PUT api/v1/admin/documents/policies/{documentCode}` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`GET api/v1/admin/documents` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`GET api/v1/admin/documents/logs` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs) |
| 워크플로우 문서 | - | `api/v1/admin/orderer/group-purchase-` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | - |
| 워크플로우 문서 | - | `api/v1/community/votes` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/community/votes` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs)<br>`GET api/v1/community/votes/{voteId:guid}` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs)<br>`POST api/v1/community/votes` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs)<br>`POST api/v1/community/votes/{voteId:guid}/votes` [Hongdal/Controllers/Common/커뮤니티투표Controller.cs](../../../../../Hongdal/Controllers/Common/커뮤니티투표Controller.cs) |
| 워크플로우 문서 | - | `api/v1/orderer/group-purchase-logistics-workflows` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/orderer/group-purchase-logistics-workflows/resolve` [Hongdal/Controllers/Orderer/공동구매물류워크플로우Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매물류워크플로우Controller.cs) |
| 워크플로우 문서 | - | `api/v1/orderer/group-purchase-overseas-shipments` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/orderer/group-purchase-overseas-shipments/lookup` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs)<br>`GET api/v1/orderer/group-purchase-overseas-shipments/import-logistics-references` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs)<br>`POST api/v1/orderer/group-purchase-overseas-shipments/import-logistics-normalization-simulation` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs)<br>`GET api/v1/orderer/group-purchase-overseas-shipments/lookup-normalized` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs) |
| 클라이언트 서비스 | GET | `api/v1/orderer/group-purchase-overseas-shipments/lookup?documentManagementNumber={encodedNumber}` | [OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs](../../../../../OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs) | `GET api/v1/orderer/group-purchase-overseas-shipments/lookup` [Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매해외선적추적Controller.cs) |
| 클라이언트 서비스 | POST | `api/v1/orderer/group-purchase-auto-groups/demands` | [OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs](../../../../../OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs) | `POST api/v1/orderer/group-purchase-auto-groups/demands` [Hongdal/Controllers/Orderer/공동구매자동집단화Controller.cs](../../../../../Hongdal/Controllers/Orderer/공동구매자동집단화Controller.cs) |
| 클라이언트 서비스 | POST | `api/v1/orderer/public-data/customs/hs-country-import-unit-price-simulation` | [OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs](../../../../../OrdererApp/Services/GroupPurchaseShipmentTrackingService.cs) | `POST api/v1/orderer/public-data/customs/hs-country-import-unit-price-simulation` [Hongdal/Controllers/Orderer/PublicDataLookupController.cs](../../../../../Hongdal/Controllers/Orderer/PublicDataLookupController.cs) |

검증할 때는 이 화면이 직접 메모리 데이터만 보는지, 위 API 응답을 받아 상태를 표시하는지, 실패했을 때 사용자가 다음 행동을 알 수 있는지 확인합니다.

## 보안과 개인정보 점검

개인정보와 업무 상태가 섞이는 화면인지 확인하고, 화면 권한과 로그 정책을 함께 점검합니다.

## 캡처와 문서 상태

현재 캡처는 문서용 캡처 호스트 또는 기존 캡처 파일 기준으로 확인한 화면입니다.

이미지 파일을 다시 생성하면 이 README는 같은 경로의 이미지를 참조하므로 자동으로 최신 캡처를 보여줍니다.

## 보완 메모

- 화면 설명이 실제 구현과 달라지면 이 문서와 app-page-catalog.md를 함께 갱신합니다.
- 화면이 1.0 필수 워크플로우에 포함되면 hongdal-v1-required-pages.md에도 반영합니다.
- 렌더링이 깨지거나 내용이 잘리면 캡처 스크립트와 실제 화면 레이아웃을 같이 확인합니다.