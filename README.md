# Hongdal

Hongdal은 주문자, 화주, 기사, 창고, 관세사, 운영자가 같은 업무 원장을 보면서 주문, 운송, 창고, 통관, 판매채널, 커뮤니티, 참여 인력 흐름을 이어가도록 만드는 .NET 10 기반 물류 플랫폼입니다.

제품 버전은 기능이 처음 정리된 시점을 기록합니다. 실제 화면 노출, 권한, 운영 가능 여부는 버전보다 **워크플로우**와 **HIOPS** 기준으로 관리합니다.

**HIOPS**는 Hongdal Integrated Operations & Policy System의 약자입니다. 홍달의 최상위 운영 체제 이름이며, 단순한 기술 계층이 아닙니다. 여러 사람이 각자의 입장에서 움직일 때 서로 간의 입장, 책임, 시간, 비용, 증빙을 잘 조율할 수 있도록 돕는 운영 기준입니다. 엔진과 워크플로우는 그 조율을 실제 업무 안에서 실행하기 위한 도구입니다.

## 현재 초점

지금 가장 먼저 안정화할 기준은 **홍달 1.0 국내 화물/용달 운송 OS**입니다.

국내 화물 운송 OS는 화주 운송 의뢰, 창고 출고품, 공동주문 국내 운송, 홍달마트 출고처럼 실제 이동이 필요한 대상을 `배차대기`로 모으고, 운송 의뢰 배차 엔진을 통해 기사 추천, 수락, 상차, 하차, POD, 정산 후보 흐름으로 연결합니다.

상세 기준은 [국내 화물 운송 OS](docs/Architecture/DomesticCargoTransportOS.md)를 봅니다.

## 큰 구조

| 구분 | 역할 |
| --- | --- |
| HIOPS | 홍달의 최상위 운영 체제입니다. 여러 참여자의 입장, 책임, 시간, 비용, 증빙을 조율하고 그 목적에 맞게 하위 OS, 워크플로우, 엔진을 조합합니다. |
| 워크플로우 | 여러 API와 앱 화면이 이어져 하나의 업무 절차를 완성하는 단위입니다. |
| 엔진 | OS가 호출하는 판단 도구입니다. 집단화, 출고 배치, 피킹 배치, 운송 의뢰 배차를 담당합니다. |
| 앱 화면 | 사용자가 현재 해야 할 일을 처리하고 상태를 서버에 반영하는 접점입니다. |

대표 OS:

| OS | 주요 역할 |
| --- | --- |
| 국내 화물 운송 OS | 운송 의뢰를 기사에게 배차하고 상차·하차·증빙·정산 후보까지 연결 |
| 창고·커머스 이행 OS | 입고, 재고, 출고, 피킹, 포장, 판매채널 주문 처리 |
| 공동주문 수입 OS | 주문자 수요 집단화, 해외 선적, 통관, 국내 반출, 분배 또는 3PL 입고 |
| 음식 배달 OS | 음식 주문의 조리·픽업·고객 전달 시간창 기준 배차 |
| 홍달마트 도심 물류 OS | 도심 창고 재고, 피킹·포장 통합, 기사 인계 |

전체 OS/엔진/스케줄링 정책은 [HIOPS와 주요 엔진 정리](docs/Architecture/EngineOverview.md)에 둡니다.

## 솔루션 구성

| 프로젝트 | 역할 |
| --- | --- |
| `Hongdal` | ASP.NET Core API Host |
| `Hongdal.Domain` | 핵심 도메인 모델 |
| `Hongdal.Contracts` | 서버/클라이언트 공용 DTO |
| `Hongdal.Infrastructure` | EF Core, Identity, Persistence, 보안 |
| `Hongdal.Ui.Common` | 공통 UI 컴포넌트 |
| `HongdalAdmin` | 관리자 앱 |
| `DriverApp` | 기사 앱 |
| `ShipperApp` | 화주/판매자 앱 |
| `WarehouseManagerApp` | 창고 현장 앱 |
| `OrdererApp` | 주문자 앱 |

## 실행과 검증

```powershell
dotnet build Hongdal.slnx /p:UseSharedCompilation=false
dotnet test Hongdal.Tests\Hongdal.Tests.csproj /p:UseSharedCompilation=false
```

개발 DB는 MySQL을 사용합니다. 로컬 실행 중 `Unable to connect to any of the specified MySQL hosts`가 발생하면 Docker의 MySQL 컨테이너 실행 상태와 연결 문자열을 먼저 확인합니다.

## 주요 문서

| 문서 | 내용 |
| --- | --- |
| [국내 화물 운송 OS](docs/Architecture/DomesticCargoTransportOS.md) | 홍달 1.0 기준 OS의 큐, 스케줄링 정책, 엔진 호출, 화면 반영 기준 |
| [HIOPS와 엔진](docs/Architecture/EngineOverview.md) | HIOPS, 하위 OS, 엔진, 스케줄링 정책 카탈로그 |
| [워크플로우 API 정책](docs/ProjectOverview/workflow-api-policy.md) | API를 버전보다 워크플로우와 액터 기준으로 관리하는 기준 |
| [워크플로우 앱 화면 지도](docs/ProjectOverview/workflow-app-screen-map.md) | 여러 앱 화면이 하나의 업무 흐름을 완성하는 방식 |
| [배차 흐름](docs/ProjectOverview/dispatch-flows.md) | 화물/용달 배차와 음식 배달 배차의 경계 |
| [출고 배치 엔진](docs/Architecture/OutboundBatchEngine.md) | 출고 배치와 피킹 배치 판단 기준 |
| [주문자 집단 공동주문/커머스](docs/ProjectOverview/orderer-group-commerce-flows.md) | 공동주문 수입, 물류대행 입고, 판매채널 출고 흐름 |
| [용어집](docs/ProjectOverview/glossary.md) | POD, BL, 3PL, 출고 배치 같은 주요 용어 정의 |

전체 문서 목록은 [프로젝트 문서 안내](docs/ProjectOverview/README.md)를 봅니다.

## 개발 원칙

1. README는 핵심 방향과 문서 링크만 둡니다.
2. 상세 설계, Mermaid 다이어그램, 코드 예시는 `docs/`에 둡니다.
3. Command와 Event 책임을 분리합니다.
4. API와 화면은 제품 버전보다 OS, 워크플로우, 책임 경계를 먼저 봅니다.
5. 앱 화면은 다음 행동, 상태, 금액, 증빙, 다음 인계 대상을 우선 노출합니다.
