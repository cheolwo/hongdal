# Hongdal

Hongdal은 화주, 기사, 창고, 운영자가 같은 물류 흐름을 공유하도록 만드는 .NET 10 기반 플랫폼입니다.
현재 개발의 중심은 **1.0 국내 화물/용달 운송 정보 서비스**입니다.

## 핵심 방향

- 1.0은 화주, 용달기사, 수령자 사이의 국내 운송 정보를 정확히 연결하는 데 집중합니다.
- 앱은 Super App 하나로 키우기보다 역할별 앱으로 분리합니다.
- 화면은 각 사용자의 "지금 처리할 일"을 먼저 보여주고, 상세 정보는 필요할 때 펼치게 합니다.
- 서버는 Command, 상태 변경, Event/Outbox 흐름을 기준으로 정리합니다.
- 운영 리스크가 큰 기능은 Admin 설정, 승인, 보류, 노출 제어를 둡니다.

## 1.0 범위

| 참여자 | 핵심 기능 | 주요 앱/화면 |
| --- | --- | --- |
| 화주 | 운송 의뢰, 상차/하차 정보, 화물 정보, 결제/정산 조건, 배차 상태 확인 | `ShipperApp` |
| 용달기사 | 추천 운송, 수락/거절, 진행 중 운송, 상차/하차 완료 증빙, 정산 확인 | `DriverApp` |
| 수령자 | 하차 예정 정보, 인수 확인, 결제 또는 인수증 관련 정보 | 운송 상세/하차 흐름 |
| 운영자 | 의뢰, 결제, 배차, 취소/환불, 기능 노출 정책 관리 | `HongdalAdmin` |

1.0에서 직접 도움이 되지 않는 음식 배달, 홍달마트, 국제 통관, HS 코드, 공동주문 기능은 이후 버전 범위로 둡니다. 구현이 일부 존재하더라도 기본 노출은 1.0 운송 흐름을 해치지 않는 수준으로 제한합니다.

## 버전 방향

| 버전 | 목표 |
| --- | --- |
| `1.0` | 국내 화물/용달 운송 정보 서비스 안정화 |
| `1.5` | 판매 물류와 창고 기반 입고/출고/재위탁 확장 |
| `2.0` | 국제 물류, 통관, HS 코드 데이터 기반 확장 |
| `2.5` | 공동주택 기반 공동 주문과 FCL/대량 입고 |
| `3.0` | 음식점 일반 음식 배달 운영 |
| `3.5` | 홍달마트와 도심 즉시배송 운영 |

세부 릴리즈 기준은 [docs/Versions](docs/Versions/README.md), [릴리즈 게이트](docs/Versions/release-gates.md), [기능 플래그 정책](docs/Versions/feature-flags.md)을 따릅니다.

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
| `HumanResourcesManagerApp` | 인력 관리자 앱 |

## 실행과 검증

```powershell
dotnet build Hongdal.slnx /p:UseSharedCompilation=false
dotnet test Hongdal.Tests\Hongdal.Tests.csproj /p:UseSharedCompilation=false
```

개발 DB는 MySQL을 사용합니다. 로컬 실행 중 `Unable to connect to any of the specified MySQL hosts`가 발생하면 Docker의 MySQL 컨테이너 실행 상태와 연결 문자열을 먼저 확인합니다.

## 문서

- 로드맵과 업무 흐름: [docs/ProjectOverview](docs/ProjectOverview/README.md)
- 버전별 범위: [docs/Versions](docs/Versions/README.md)
- 화면/컨트롤러 매핑: [docs/ViewControllerMapping](docs/ViewControllerMapping/README.md)
- Command/Event 원칙: [docs/Architecture/CommandEvent리팩토링원칙.md](docs/Architecture/CommandEvent리팩토링원칙.md)
- 참여자 중심 설계: [docs/Architecture/참여자중심설계원칙.md](docs/Architecture/참여자중심설계원칙.md)

## 개발 원칙

1. Command와 Event 책임을 분리한다.
2. 1.0 기능은 화주, 용달기사, 수령자 운송 흐름을 닫는지 먼저 본다.
3. 앱 화면은 다음 행동, 상태, 금액, 증빙을 우선 노출한다.
4. 상세 설계와 긴 흐름은 `Docs/`에 두고 README는 핵심 요약만 유지한다.
