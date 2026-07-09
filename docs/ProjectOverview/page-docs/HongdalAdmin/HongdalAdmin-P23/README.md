# HongdalAdmin-P23 - 관리자 활동 로그

[전체 화면 문서](../../README.md) / [HongdalAdmin 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

<img src="../../../assets/app-pages/HongdalAdmin/HongdalAdmin-P23.png" alt="HongdalAdmin-P23 화면 캡처" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | HongdalAdmin |
| 페이지 ID / 제목 | HongdalAdmin-P23 - 관리자 활동 로그 |
| 라우트 | /activity-logs |
| 소스 파일 | [HongdalAdmin/Components/Pages/ActivityLogs.razor](../../../../../HongdalAdmin/Components/Pages/ActivityLogs.razor) |
| 분류 | 운영 |
| 1.0 필수 연결 | 직접 연결 없음 |
| 캡처 상태 | 인증 필요 |

## 왜 필요한가

이 화면은 관리자 활동 로그을 담당하므로, 운영자가 막힌 상태와 정책 변경 지점을 확인하기 위해 필요합니다.

## 사용자와 참여자

주 사용자: 관리자, 운영자 / 보조 참여자: 화주, 기사, 파트너, 문서 담당자

이 화면은 보조 또는 확장 업무 워크플로우 안에서 관리자 활동 로그 책임을 갖습니다. 화면 하나가 너무 많은 결정을 떠안지 않도록, 이 문서에서는 이 화면의 주 책임과 다른 화면으로 넘겨야 할 책임을 구분해 관리합니다.

## 화면에서 다루는 일

- 주 책임: 관리자 활동 로그
- 사용자가 확인해야 하는 것: 이 화면에서 상태, 입력값, 다음 행동이 명확히 보이는지 확인합니다.
- 사용자가 조작해야 하는 것: 버튼, 입력, 선택, 업로드, 조회 같은 조작이 이 화면의 책임 안에 머무는지 확인합니다.
- 화면 밖으로 넘길 일: 다른 앱이나 관리자 화면에서 처리해야 하는 상태 변경은 이 화면에 과하게 넣지 않습니다.

## 다른 화면과의 관계

- 이전 화면: [HongdalAdmin-P22-3 - 운송 정산 상세](../HongdalAdmin-P22-3/)
- 다음 화면: [HongdalAdmin-P24 - 화면/기능 노출 정책](../HongdalAdmin-P24/)
- 상위 화면: 없음
- 하위 화면: 없음

상호작용 관점에서는 다음 흐름을 우선 봅니다. 관리자는 여러 앱에서 발생한 상태 변경을 모아 보고, 막힌 배차·증빙·정산·문서 문제에 개입합니다.

## API 경로와 코드 연결

- 화면 소스: [HongdalAdmin/Components/Pages/ActivityLogs.razor](../../../../../HongdalAdmin/Components/Pages/ActivityLogs.razor)
- 클라이언트 서비스/계약: [HongdalAdmin/Services/ActivityLogService.cs](../../../../../HongdalAdmin/Services/ActivityLogService.cs), [HongdalAdmin/Services/관리자인증세션Service.cs](../../../../../HongdalAdmin/Services/관리자인증세션Service.cs)

| 구분 | 메서드 | API 경로 | 클라이언트/문서 근거 | 서버 근거 |
| --- | --- | --- | --- | --- |
| 워크플로우 문서 | - | `api/v1/admin/documents` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | `GET api/v1/admin/documents/policies` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`PUT api/v1/admin/documents/policies/{documentCode}` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`GET api/v1/admin/documents` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs)<br>`GET api/v1/admin/documents/logs` [Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs](../../../../../Hongdal/Controllers/Admin/04_증빙/문서관리Controller.cs) |
| 워크플로우 문서 | - | `api/v1/admin/orderer/group-purchase-` | [docs/ProjectOverview/workflow-app-screen-map.md](../../../workflow-app-screen-map.md) | - |
| 클라이언트 서비스 | GET | `api/v1/admin/activity-logs/{id}` | [HongdalAdmin/Services/ActivityLogService.cs](../../../../../HongdalAdmin/Services/ActivityLogService.cs) | `GET api/v1/admin/activity-logs` [Hongdal/Controllers/Admin/사용자행위로그Controller.cs](../../../../../Hongdal/Controllers/Admin/사용자행위로그Controller.cs)<br>`GET api/v1/admin/activity-logs/{id:long}` [Hongdal/Controllers/Admin/사용자행위로그Controller.cs](../../../../../Hongdal/Controllers/Admin/사용자행위로그Controller.cs) |
| 클라이언트 서비스 | GET | `api/v1/admin/activity-logs/trace/{traceId}` | [HongdalAdmin/Services/ActivityLogService.cs](../../../../../HongdalAdmin/Services/ActivityLogService.cs) | `GET api/v1/admin/activity-logs` [Hongdal/Controllers/Admin/사용자행위로그Controller.cs](../../../../../Hongdal/Controllers/Admin/사용자행위로그Controller.cs)<br>`GET api/v1/admin/activity-logs/trace/{traceId}` [Hongdal/Controllers/Admin/사용자행위로그Controller.cs](../../../../../Hongdal/Controllers/Admin/사용자행위로그Controller.cs) |

검증할 때는 이 화면이 직접 메모리 데이터만 보는지, 위 API 응답을 받아 상태를 표시하는지, 실패했을 때 사용자가 다음 행동을 알 수 있는지 확인합니다.

## 보안과 개인정보 점검

관리자 권한, 감사 로그, 민감 운영 정보 접근 통제가 필요합니다. 인증 필요 화면은 현재 로그인 장벽 캡처로 남겨 둡니다.

## 캡처와 문서 상태

이 화면은 현재 인증 장벽까지 캡처되어 있으므로, 실제 운영 화면 설명은 인증 세션을 붙인 재캡처 뒤 보완해야 합니다.

이미지 파일을 다시 생성하면 이 README는 같은 경로의 이미지를 참조하므로 자동으로 최신 캡처를 보여줍니다.

## 보완 메모

- 화면 설명이 실제 구현과 달라지면 이 문서와 app-page-catalog.md를 함께 갱신합니다.
- 화면이 1.0 필수 워크플로우에 포함되면 hongdal-v1-required-pages.md에도 반영합니다.
- 렌더링이 깨지거나 내용이 잘리면 캡처 스크립트와 실제 화면 레이아웃을 같이 확인합니다.