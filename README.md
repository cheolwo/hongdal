# Hongdal

Hongdal은 주문자, 화주, 기사, 창고, 관세사, 운영자가 같은 업무 원장을 보면서 주문, 운송, 창고, 통관, 판매채널, 커뮤니티, 참여 인력 흐름을 이어가도록 만드는 .NET 10 기반 물류 플랫폼입니다.

이 README는 처음 읽는 사람이 방향과 문서 이동 경로를 빠르게 잡을 수 있도록 핵심만 요약합니다. 세부 설계와 판단 근거는 번호가 붙은 첨부 문서에서 확인합니다.

## 한 장 요약

| 항목 | 내용 |
| --- | --- |
| 최상위 관점 | **HIOPS**: Hongdal Integrated Operations & Policy System. 여러 참여자의 입장, 책임, 시간, 비용, 증빙을 조율하는 홍달의 운영 체제입니다. |
| 현재 초점 | **홍달 1.0 국내 화물/용달 운송 OS**. 운송 의뢰를 기사 추천, 수락, 상차, 하차, POD, 정산 후보 흐름으로 연결합니다. |
| 판단 방식 | 절대 조건은 규칙 기반으로 걸러내고, 비용/시간/대기/경로 변경 이점은 계산합니다. AI는 참여자 입장 해석, 추천 사유 설명, 예외 대응 보조부터 붙입니다. |
| 핵심 엔진 | 집단화 엔진, 출고 배치 엔진, 피킹 배치 엔진, 운송 의뢰 배차 엔진. OS가 업무 목적에 맞게 엔진을 호출합니다. |
| 앱 역할 | `DriverApp`은 기사 운송 수행, `ShipperApp`은 화주/판매자 업무, `WarehouseManagerApp`은 창고 현장 작업, `OrdererApp`은 주문자 흐름, `HongdalAdmin`은 운영 관리입니다. |

## 홍달 1.0 중심 흐름

```mermaid
flowchart LR
    A[운송 의뢰/출고 예정] --> B[운송 의뢰 배차 엔진]
    B --> C[기사 추천]
    C --> D[수락/거절]
    D --> E[상차 사진 증빙]
    E --> F[운송 중 경로/추가 추천 판단]
    F --> G[하차 사진 증빙]
    G --> H[POD/정산 후보]
```

국내 화물 운송 OS는 화주 운송 의뢰, 창고 출고품, 공동주문 국내 운송, 홍달마트 출고처럼 실제 이동이 필요한 대상을 `배차대기`로 모읍니다. 이후 운송 의뢰 배차 엔진이 차량 적합성, 위치, 시간창, 대기, 경로 변경 이점, 증빙 상태를 보고 기사님에게 추천합니다.

## 첨부 문서

세부 문서는 번호가 붙은 첨부 문서 목차에서 봅니다.

| 번호 | 문서 | 내용 |
| --- | --- | --- |
| 00 | [첨부 문서 목차](docs/ProjectOverview/00-첨부문서목차.md) | 프로젝트 문서를 읽는 순서와 문서별 위치 |
| 01 | [국내 화물 운송 OS](docs/Architecture/DomesticCargoTransportOS.md) | 홍달 1.0의 큐, 스케줄링 정책, 엔진 호출, 화면 반영 기준 |
| 02 | [HIOPS와 엔진](docs/Architecture/EngineOverview.md) | OS, 워크플로우, 엔진, 스케줄링 정책 카탈로그 |
| 03 | [HIOPS AI](docs/Architecture/HIOPSAI.md) | 참여자 입장 해석 AI와 국내 화물 운송 배차 조율 AI의 역할 |
| 04 | [워크플로우 앱 화면 지도](docs/ProjectOverview/workflow-app-screen-map.md) | 여러 앱 화면이 하나의 업무 절차를 완성하는 관계 |
| 05 | [워크플로우 API 정책](docs/ProjectOverview/workflow-api-policy.md) | API를 버전보다 업무 절차와 액터 기준으로 관리하는 기준 |
| 06 | [배차 흐름](docs/ProjectOverview/dispatch-flows.md) | 화물/용달 배차와 음식 배달 배차의 경계 |
| 07 | [출고 배치 엔진](docs/Architecture/OutboundBatchEngine.md) | 출고 배치와 피킹 배치 판단 기준 |
| 08 | [공동주문/커머스 흐름](docs/ProjectOverview/orderer-group-commerce-flows.md) | 공동주문 수입, 국내 운송, 판매채널 출고 흐름 |
| 09 | [HIOPS AI 판단 사례집](docs/ProjectOverview/hiops-ai-judgment-cases.md) | AI 판단 보조를 위한 상황별 사례와 사용자 판정 기록 |
| 10 | [용어집](docs/ProjectOverview/glossary.md) | POD, BL, 3PL, 레그, RAG 같은 주요 용어 정의 |

## 솔루션 구성

| 프로젝트 | 역할 |
| --- | --- |
| `Hongdal` | ASP.NET Core API Host |
| `Hongdal.Domain` | 핵심 도메인 모델 |
| `Hongdal.Contracts` | 서버/클라이언트 공용 DTO |
| `Hongdal.Infrastructure` | EF Core, Identity, Persistence, 보안 |
| `Hongdal.Ui.Common` | 공통 UI 컴포넌트 |
| `DriverApp` | 기사 앱 |
| `ShipperApp` | 화주/판매자 앱 |
| `WarehouseManagerApp` | 창고 현장 앱 |
| `OrdererApp` | 주문자 앱 |
| `HongdalAdmin` | 관리자 앱 |

## 실행과 검증

```powershell
dotnet build Hongdal.slnx /p:UseSharedCompilation=false
dotnet test Hongdal.Tests\Hongdal.Tests.csproj /p:UseSharedCompilation=false
```

개발 DB는 MySQL을 사용합니다. 로컬 실행 중 `Unable to connect to any of the specified MySQL hosts`가 발생하면 Docker의 MySQL 컨테이너 실행 상태와 연결 문자열을 먼저 확인합니다.

## 개발 원칙

1. README는 핵심 방향과 문서 링크만 둡니다.
2. 상세 설계, Mermaid 다이어그램, 코드 예시는 `docs/`에 둡니다.
3. 절대 조건은 규칙 기반으로 유지하고, AI는 설명, 예외 대응, 판단 보조부터 적용합니다.
4. API와 화면은 제품 버전보다 OS, 워크플로우, 책임 경계를 먼저 봅니다.
5. 앱 화면은 다음 행동, 상태, 금액, 증빙, 다음 인계 대상을 우선 노출합니다.

## 문서 작성 배경

루트 README는 저자 이윤석의 [『논스톱 보고서』](https://product.kyobobook.co.kr/detail/S000218640179)에서 영향을 받아 1페이지 보고서식 요약을 지향합니다.
