# DriverApp-P07-1 - 기사 업무 허브/요약

[전체 화면 문서](../../README.md) / [DriverApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

<img src="../../../assets/app-pages/DriverApp/DriverApp-P07-1.png" alt="DriverApp-P07-1 화면 캡처" width="720">

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | DriverApp |
| 페이지 ID / 제목 | DriverApp-P07-1 - 기사 업무 허브/요약 |
| 라우트 | /driver/home/summary |
| 소스 파일 | [DriverApp/Components/Pages/Driver/Home/기사홈Page.razor](../../../../../DriverApp/Components/Pages/Driver/Home/기사홈Page.razor) |
| 분류 | 보조 |
| 1.0 필수 연결 | [DriverApp-P07-1 - 기사 업무 허브/요약](../../../ssalddel-v1-required-pages.md) |
| 캡처 상태 | 완료 |

## 왜 필요한가

이 화면은 기사 업무 허브/요약을 담당하므로, 주 업무 화면을 보조하고 사용자가 다음 행동으로 이동할 수 있게 합니다.

## 사용자와 참여자

주 사용자: 기사 / 보조 참여자: 화주, 관리자, 창고 또는 현장 담당자

이 화면은 살뜰 1.0 국내 화물 운송 워크플로우 안에서 기사 업무 허브/요약 책임을 갖습니다. 화면 하나가 너무 많은 결정을 떠안지 않도록, 이 문서에서는 이 화면의 주 책임과 다른 화면으로 넘겨야 할 책임을 구분해 관리합니다.

## 화면에서 다루는 일

- 주 책임: 기사 업무 허브/요약
- 사용자가 확인해야 하는 것: 이 화면에서 상태, 입력값, 다음 행동이 명확히 보이는지 확인합니다.
- 사용자가 조작해야 하는 것: 버튼, 입력, 선택, 업로드, 조회 같은 조작이 이 화면의 책임 안에 머무는지 확인합니다.
- 화면 밖으로 넘길 일: 다른 앱이나 관리자 화면에서 처리해야 하는 상태 변경은 이 화면에 과하게 넣지 않습니다.

## 다른 화면과의 관계

- 이전 화면: [DriverApp-P07 - 지도 홈, 추천 배너, 현재 운송 진입](../DriverApp-P07/)
- 다음 화면: [DriverApp-P08 - 추천 목록](../DriverApp-P08/)
- 상위 화면: [DriverApp-P07 - 지도 홈, 추천 배너, 현재 운송 진입](../DriverApp-P07/)
- 하위 화면: 없음

상호작용 관점에서는 다음 흐름을 우선 봅니다. 기사의 수락, 거절, 상차, 하차, 증빙, 정산 관련 조작은 화주 상세와 관리자 원장에 상태 변경으로 반영됩니다.

## API 경로와 코드 연결

- 화면 소스: [DriverApp/Components/Pages/Driver/Home/기사홈Page.razor](../../../../../DriverApp/Components/Pages/Driver/Home/기사홈Page.razor)
- 클라이언트 서비스/계약: 화면 파일에서 직접 확인하거나 정적/라우팅 화면으로 본다.

| 구분 | 메서드 | API 경로 | 클라이언트/문서 근거 | 서버 근거 |
| --- | --- | --- | --- | --- |
| 화면 직접 | - | `api/v1/driver/home` | [DriverApp/Components/Pages/Driver/Home/기사홈Page.razor](../../../../../DriverApp/Components/Pages/Driver/Home/기사홈Page.razor) | `GET api/v1/driver/home` [Ssalddel/Controllers/Driver/00_Profile/기사홈Controller.cs](../../../../../Ssalddel/Controllers/Driver/00_Profile/기사홈Controller.cs) |

검증할 때는 이 화면이 직접 메모리 데이터만 보는지, 위 API 응답을 받아 상태를 표시하는지, 실패했을 때 사용자가 다음 행동을 알 수 있는지 확인합니다.

## 보안과 개인정보 점검

위치, 주소, 운행 상태는 최소 공개와 최신성 표시가 필요합니다.

## 캡처와 문서 상태

현재 캡처는 문서용 캡처 호스트 또는 기존 캡처 파일 기준으로 확인한 화면입니다.

이미지 파일을 다시 생성하면 이 README는 같은 경로의 이미지를 참조하므로 자동으로 최신 캡처를 보여줍니다.

## 보완 메모

- 화면 설명이 실제 구현과 달라지면 이 문서와 app-page-catalog.md를 함께 갱신합니다.
- 화면이 1.0 필수 워크플로우에 포함되면 ssalddel-v1-required-pages.md에도 반영합니다.
- 렌더링이 깨지거나 내용이 잘리면 캡처 스크립트와 실제 화면 레이아웃을 같이 확인합니다.