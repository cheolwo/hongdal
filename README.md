# Hongdal

Hongdal은 **화주가 운송을 의뢰하고, 기사님이 추천을 받아 운송하고, 관리자가 진행 상태를 확인하는 화면들**을 중심으로 만든 물류 플랫폼입니다.

이 README는 **홍달 1.0에서 실제로 볼 수 있는 화면**을 먼저 보여줍니다. 기술 구조, OS, 엔진, AI 같은 설명은 앞에 두지 않고 [첨부 문서 목차](docs/ProjectOverview/00-첨부문서목차.md) 뒤쪽에 따로 정리합니다.

## 먼저 볼 화면

아래 화면들은 링크를 눌러 들어가지 않아도 README에서 바로 보이는 대표 캡처입니다. 전체 화면 목록과 나머지 캡처는 [코드 프로젝트별 전체 페이지 카탈로그](docs/ProjectOverview/app-page-catalog.md)에서 봅니다.

### 대표 화면: 기사 지도 홈

기사님이 운행을 시작하고 추천 배너와 현재 운송으로 들어갑니다.

<img src="docs/ProjectOverview/assets/app-pages/DriverApp/DriverApp-P07.png" alt="DriverApp 기사 지도 홈 화면" width="360">

### 화주: 의뢰 상세

화주가 결제, 배차, 수락, 상차, 하차, 정산 상태를 한 화면에서 확인합니다.

<img src="docs/ProjectOverview/assets/app-pages/ShipperApp/ShipperApp-P03.png" alt="ShipperApp 의뢰 상세 화면" width="280">

### 기사: 추천 상세

기사님이 추천받은 운송 의뢰의 상차지, 하차지, 운임, 제한 시간을 확인합니다.

<img src="docs/ProjectOverview/assets/app-pages/DriverApp/DriverApp-P09.png" alt="DriverApp 추천 상세 화면" width="280">

### 기사: 상차와 하차 증빙

기사님이 상차와 하차 사진을 남기고 운송 상태를 다음 단계로 넘깁니다.

<img src="docs/ProjectOverview/assets/app-pages/DriverApp/DriverApp-P12.png" alt="DriverApp 상차 증빙 화면" width="260">
<img src="docs/ProjectOverview/assets/app-pages/DriverApp/DriverApp-P13.png" alt="DriverApp 하차 증빙 화면" width="260">

### 관리자: 운송 원장

운영자가 의뢰, 배차, 증빙, 정산 흐름을 점검합니다.

<img src="docs/ProjectOverview/assets/app-pages/HongdalAdmin/HongdalAdmin-P22.png" alt="HongdalAdmin 운송 원장 화면" width="320">

### 통합 클라이언트: 사람과 업무가 이어지는 커뮤니티

같은 앱에서 역할을 바꾸고, 게시판과 세로 다이어그램을 보다가 화주·창고 업무로 이어갑니다. 괘상과 노드 이미지는 독립 상점에서 선택하거나 직접 만들 수 있습니다.

<img src="docs/ProjectOverview/assets/app-pages/ShipperApp/ShipperApp-P00.png" alt="Hongdal 역할 기반 통합 커뮤니티 홈" width="360">

[통합 커뮤니티 클라이언트와 꾸미기 상점 문서](docs/ProjectOverview/unified-community-client.md)에서 역할 전환, 모바일 다이어그램, 후천 사방 이동판, 상점·상세·FakePG·제작 흐름을 봅니다.

## 통합 클라이언트 화면 구조

```mermaid
flowchart LR
    A["1단계 · 사방괘"] --> B["2단계 · 다이어그램"]
    B --> C["3단계 · 구체 데이터 페이지"]
```

사방괘에서 업무 영역을 고르고, 다이어그램에서 창고·주문·사람·운송 같은 노드의 관계와 상태를 파악한 뒤, 노드의 행동 메뉴에서 입고 내역·재고·운송 상세 같은 구체 페이지를 엽니다. 음식 주문·배달은 주문 1건에 배달 회차 0..N건을 연결하는 묶음으로, 공동구매는 수요·수입 결정·선적/통관·입고/분배를 조정하는 복합 원장으로 표시합니다. 데스크톱은 오른쪽 클릭 또는 `⋮`, 모바일은 길게 누르기 또는 `⋮`를 사용합니다. 상세 기준은 [통합 클라이언트 3단계 내비게이션](docs/Architecture/ThreeStageClientNavigation.md)에 둡니다.

## 홍달 1.0 흐름

```mermaid
flowchart LR
    A["화주 의뢰 화면"] --> B["기사 추천 화면"]
    B --> C{"수락 또는 거절"}
    C -->|수락| D["상차 증빙 화면"]
    C -->|거절/만료| B
    D --> E["운송 진행 화면"]
    E --> F["하차 증빙 화면"]
    F --> G["관리자 확인/정산 화면"]
```

홍달 1.0은 화면 기준으로 보면 단순합니다. 화주는 의뢰를 만들고 상태를 확인합니다. 기사님은 추천을 받고, 수락하거나 거절하고, 상차와 하차 사진을 남깁니다. 관리자는 그 과정에서 막힌 지점, 증빙 누락, 정산 대기 상태를 확인합니다.

## 코드 프로젝트별 화면 묶음

아래 프로젝트명은 사용자에게 별도 앱을 강제하는 내비게이션 분류가 아니라, 현재 코드와 라우트가 들어 있는 위치를 찾기 위한 물리 색인입니다.

| 코드 프로젝트 | 화면 수 | 1.0에서 먼저 보는 화면 |
| --- | ---: | --- |
| `ShipperApp` | 30 | 통합 커뮤니티 홈, 역할 전환, 운송 업무, 꾸미기 상점, 의뢰 타임라인 |
| `DriverApp` | 23 | 운행 시작, 지도 홈, 추천, 수락/거절, 상차/하차 증빙, 정산 |
| `HongdalAdmin` | 42 | 배차 대기, 운송 원장, 문서/POD, 결제/정산, 운영 점검 |
| `WarehouseManagerApp` | 13 | 창고 작업 보드, 입고, 스캔, 피킹 배치, 마트 피킹/포장 |
| `OrdererApp` | 8 | 주문자 홈, 공동구매, 음식/마트 주문, 주문 이력 |
| `RestaurantDeskApp` | 5 | 음식점/매장 운영 화면 |

## 첨부 문서

화면과 화면 사이의 더 자세한 흐름은 README 본문에 길게 펼치지 않고 첨부 문서로 둡니다.

| 문서 | 용도 |
| --- | --- |
| [첨부 문서 목차](docs/ProjectOverview/00-첨부문서목차.md) | 화면 문서부터 기술 문서까지 읽는 순서 |
| [커밋별 시각 변경 기록](docs/Changes/README.md) | 커밋 해시별 화면 변화와 실제 렌더링 캡처 |
| [통합 커뮤니티 클라이언트](docs/ProjectOverview/unified-community-client.md) | 역할 전환, 모바일 다이어그램, 후천 사방 이동, 꾸미기 상점 |
| [통합 클라이언트 3단계 내비게이션](docs/Architecture/ThreeStageClientNavigation.md) | 사방괘 → 다이어그램 → 구체 데이터 페이지 구성 원칙 |
| [코드 프로젝트별 전체 페이지 카탈로그](docs/ProjectOverview/app-page-catalog.md) | 통합 클라이언트 화면의 실제 코드 위치와 인라인 캡처 |
| [홍달 1.0 필수 페이지 기준](docs/ProjectOverview/hongdal-v1-required-pages.md) | 1.0 운송 흐름에 꼭 필요한 화면 |
| [렌더링/캡처 검증 요약](docs/ProjectOverview/hongdal-v1-render-capture-summary.md) | 화면 캡처 방식과 검증 결과 |

## 실행과 검증

```powershell
dotnet build Hongdal.slnx /p:UseSharedCompilation=false
dotnet test Hongdal.Tests\Hongdal.Tests.csproj /p:UseSharedCompilation=false
```

## 문서 작성 배경

루트 README는 저자 이윤석의 [『논스톱 보고서』](https://product.kyobobook.co.kr/detail/S000218640179)에서 영향을 받아 1페이지 보고서식 요약을 지향합니다.
