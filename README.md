# Hongdal

Hongdal은 화물 운송, 음식 배달, 판매 물류를 하나의 운영 모델로 다루는 .NET 10 기반 솔루션입니다.  
현재는 **Command → 상태 변경 → Event/Outbox** 리듬과 앱별 역할 분리를 중심으로 구조를 정리하고 있습니다.


# DriverApp 화면예시
<img width="514" height="1153" alt="DriverApp 사용자메뉴" src="https://github.com/user-attachments/assets/bc74f20b-b676-4390-9e2d-da213e46d865" />

<img width="509" height="760" alt="DirverApp 운행" src="https://github.com/user-attachments/assets/cde136dd-e1e7-4e7a-b46d-3e1c601a7fdd" />

<img width="487" height="744" alt="DriverApp Home 화면" src="https://github.com/user-attachments/assets/9cecb7cd-bd58-4981-902e-28fdf24d6572" />

# ShipperApp 메뉴
<img width="809" height="1009" alt="ShipperApp 메뉴" src="https://github.com/user-attachments/assets/094b6cb5-2388-48c4-86e9-bb82b73786dc" />

# WarehouseManager App 화면
<img width="1106" height="751" alt="창고 App 화면" src="https://github.com/user-attachments/assets/52cccc67-5d58-4405-9c09-a8b79191ece7" />

<img width="1135" height="766" alt="포장 화면" src="https://github.com/user-attachments/assets/9bf11f36-03c7-48ea-9718-9cbcc24345d4" />

<img width="1125" height="762" alt="출고 화면" src="https://github.com/user-attachments/assets/1b6efd10-be59-40f5-b1eb-bd488709c293" />

<img width="1126" height="764" alt="입고화면" src="https://github.com/user-attachments/assets/f3c34968-620a-4cd1-bc5c-21c933c91879" />


## 핵심 방향

- 운영 레인: 계약 영역 / 인사 영역 / 비즈니스 실행 영역
- 업무 축: 화주 운송 / 음식 배달 / 판매 물류
- 앱 전략: 기능 과밀한 Super App 대신 역할별 앱 분리
- 운영 전략: 자동 계산 + Admin 승인/보류/노출 제어

## 현재 솔루션 구성 (Hongdal.slnx)

| 프로젝트 | 역할 | TFM |
| --- | --- | --- |
| `Hongdal` | ASP.NET Core API Host, Controller, Application 조립 | `net10.0` |
| `Hongdal.Domain` | 핵심 도메인 모델(사용자, 계약, 물류, 설정 등) | `net10.0` |
| `Hongdal.Contracts` | 서버/클라이언트 공용 계약 DTO | `net10.0` |
| `Hongdal.Infrastructure` | EF Core/Identity/Persistence/보안 | `net10.0` |
| `Hongdal.Ui.Common` | 공통 UI/백오피스 영역 컴포넌트 | `net10.0` |
| `HongdalAdmin` | 관리자 앱(운영 제어) | `net10.0` |
| `Hongdal.BackOffice.Ui` | 백오피스 UI 호스트 | `net10.0` |
| `Hongdal.BackOffice.Client` | 백오피스 클라이언트 계층 | `net10.0` |
| `Hongdal.FoodApi` | 음식 도메인 API 분리 영역 | `net10.0` |
| `DriverApp` | 기사 앱 (.NET MAUI Android) | `net10.0-android` |
| `ShipperApp` | 화주/판매자 앱 (.NET MAUI Android) | `net10.0-android` |
| `WarehouseManagerApp` | 창고 현장 앱 (.NET MAUI Android) | `net10.0-android` |

## 서버 구조 가이드

서버 코드(`Hongdal`)는 레인 기준으로 점진 정리 중입니다.

| 레인 | 주요 위치 |
| --- | --- |
| 인사 영역 | `Application/HumanResources`, `Services/HumanResources`, `Controllers/Admin/HumanResources` |
| 계약 영역 | `Application/ContractManagement`, `Services/ContractManagement`, `Controllers/Admin/ContractManagement` |
| 물류 처리 영역 | `Application/LogisticsProcessing`, `Services/LogisticsProcessing`, `Controllers/Admin/LogisticsProcessing` |
| 정산/환원 영역 | `Application/*/Settlement`, `Services/Settlement`, `Controllers/Admin/Settlement` |

## 최근 반영 관점

- DriverApp: 지도 중심 홈 + 배차/운행 흐름 집중
- ShipperApp: 화주/판매자 운영 허브 역할 유지
- WarehouseManagerApp: 입고/출고/포장/스캔 현장 공정 분리
- HR/권한: 역할 + 근무시간 + 작업장 IP 기준 강화
- Admin: Command 기능 설정, Event 후속처리, 알림/정산/노출 정책 제어

## 참고 문서

- [CommandEvent리팩토링원칙.md](../Docs/Architecture/CommandEvent리팩토링원칙.md)
- [참여자중심설계원칙.md](../Docs/Architecture/참여자중심설계원칙.md)
- [배차큐_진행현황_2026-07-02.md](../Docs/DispatchQueue/배차큐_진행현황_2026-07-02.md)
- [ViewControllerMapping](../Docs/ViewControllerMapping/README.md)

## 개발 원칙

1. Command와 Event 책임을 분리한다.
2. 운영 리스크가 큰 흐름은 Admin 승인 지점을 둔다.
3. 앱은 각 참여자의 "지금 처리할 일"을 우선 노출한다.
4. 상세 설계/기록은 `Docs/`로 분리하고 README는 항상 최신 요약으로 유지한다.
