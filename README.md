# Hongdal

Hongdal은 주문자, 화주, 기사, 창고, 관세사, 운영자가 같은 업무 원장을 보면서 주문, 운송, 창고, 통관, 판매채널, 커뮤니티, 참여 인력 흐름을 이어가도록 만드는 .NET 10 기반 플랫폼입니다.

제품 버전은 기능이 처음 들어온 시점을 기록합니다. 실제 화면 노출, 권한, 운영 가능 여부는 버전보다 **워크플로우** 기준으로 관리합니다.

## 최근 변경 기록

2026-07-08 기준 최신 반영본은 홍달 1.0 국내 화물/용달 운송을 중심축으로 두고, 공동주문 수입, 창고 입출고, 판매채널 출고, 커뮤니티 신뢰 흐름이 어떤 유스케이스와 액터로 성립되는지 코드와 문서에 함께 드러나도록 정리했습니다.

- `HongdalUseCaseAttribute`, `HongdalUseCaseActorAttribute`를 추가해 유스케이스의 워크플로우, 주 액터, 보조 액터, 표시 이름을 메타데이터로 기록합니다.
- `HongdalUseCaseRelationAttribute`를 추가해 유스케이스 사이의 `Include`/`Extend` 관계를 기록하고, `GET /api/v1/version-feature-flags` 응답에서 워크플로우별 `UseCases[].Relations`로 조회할 수 있게 했습니다.
- 인증, 화주 운송 의뢰, 기사 추천, 기사 프로필, 기사 알림, 창고 작업, 판매채널, 커뮤니티 게시판, 커뮤니티 게시글, 커뮤니티 투표, 커뮤니티 활동 신호, 공동구매 자동 집단화, 공동구매 커머스 이행 계획, 공동구매 해외 선적 추적을 유스케이스 중심 구조로 정리했습니다.
- 커뮤니티 게시글, 첨부, 댓글, 추천, 신고, 운영자 고정/숨김 처리는 `커뮤니티게시글UseCase`로 옮겨 컨트롤러가 HTTP 라우팅과 응답 변환만 맡도록 정리했습니다.
- 한글 도메인 명명 규칙에 따라 커뮤니티 유스케이스와 컨트롤러는 `커뮤니티게시판UseCase`, `커뮤니티게시글Controller`, `커뮤니티투표UseCase`, `커뮤니티활동신호UseCase`처럼 도메인 명사는 한글로 두고 `UseCase`, `Service`, `Controller` 같은 기술 접미사는 유지합니다.
- 공공 데이터 조회, 파일 업로드, 파일 POD, 문서 관리, 샘플 이미지 작업, ISMS-P 전송 보호도 유스케이스로 분리해 컨트롤러가 HTTP 경계만 맡도록 정리했습니다.
- 관리자 보조 기능 설정은 `보조기능설정UseCase`로 분리해 전역/사용자별 기능 설정, 초기화, 감사 로그 기록을 컨트롤러 밖에서 처리하도록 정리했습니다.
- HS 코드 운영은 `HS코드운영UseCase`로 분리해 코드 목록 조회, 업무 분류 보정, 통관 주의 태그 저장, 관세사/운영자 감사 로그 기록을 컨트롤러 밖에서 처리하도록 정리했습니다.
- 참여 인력 관리 흐름은 `HR참여운영UseCase`, `사회보험신고UseCase`, `인연스냅샷조회UseCase`, `플랫폼수익환급UseCase`로 나누어 계약, 급여 스케줄, 4대보험 신고 준비, 참여 보상 환급을 컨트롤러 밖에서 처리합니다.
- `VersionFeatureFlagsController`의 워크플로우/유스케이스 조립 로직은 `버전워크플로우UseCase`로 이동해 버전 플래그 API도 같은 유스케이스 메타데이터 규칙을 따릅니다.
- 공동주문 수입이 보세구역 반출 뒤 국내 화물 운송이나 3PL 입고로 이어질 수 있도록 배차대기, 기사 추천 표시, 관리자/주문자 앱 샘플 데이터를 연결했습니다.
- 공동구매, 주문자 집단, 해외 선적, 수입 물류, 커머스 이행 관련 도메인 파일명을 한글 중심으로 정리하고 기존 테스트도 같은 기준으로 맞췄습니다.
- 주요 엔진 정리는 `docs/Architecture/EngineOverview.md`, 워크플로우/유스케이스/액터 정책은 `docs/ProjectOverview/workflow-api-policy.md`에 둡니다.

## 핵심 방향

- 현재 개발 판단의 1순위는 **홍달 1.0 국내 화물/용달 운송**입니다.
- 하나의 워크플로우는 하나의 앱 화면이 아니라 여러 앱 화면이 순서대로 호출되면서 완성됩니다.
- 1.0 국내 화물 운송은 낮은 버전의 범위가 아니라, 공동주문 수입, 창고 입출고, 판매채널 출고, 음식 배달, 홍달마트가 필요할 때 합류하는 공통 실행 중심축입니다.
- 앱은 Super App 하나로 키우기보다 역할별 앱으로 분리합니다.
- 화면은 각 사용자의 "지금 처리할 일"을 먼저 보여주고, 상세 정보는 필요할 때 펼치게 합니다.
- 서버는 Command, 상태 변경, Event/Outbox 흐름을 기준으로 정리합니다.
- 운영 리스크가 큰 기능은 Admin 설정, 승인, 보류, 노출 제어를 둡니다.

## 1.0 중심축

홍달은 여러 워크플로우를 갖지만, 첫 번째로 닫아야 할 흐름은 국내 화물/용달 운송입니다. 다른 확장 흐름은 독립적으로 펼쳐 놓기보다, 어떤 시점에 1.0 운송 중심축으로 들어오고 어떤 상태를 되돌려 받는지 기준으로 봅니다.

```mermaid
flowchart TD
    Shipper["ShipperApp<br/>/shipper/request<br/>화주 운송 의뢰"]
    AdminWait["HongdalAdmin<br/>/dispatch/wait<br/>배차 대기"]
    DriverReco["DriverApp<br/>/driver/recommendations<br/>기사 추천"]
    DriverDecision["DriverApp<br/>/driver/recommendations/{의뢰Id}/decision<br/>수락·보류·거절"]
    DriverRun["DriverApp<br/>/driver/transports/current<br/>진행 중 운송"]
    PickupDropoff["DriverApp<br/>상차·하차·증빙"]
    AdminTrans["HongdalAdmin<br/>/transports, /documents, /settlements<br/>운송·증빙·정산 관리"]

    Shipper --> AdminWait --> DriverReco --> DriverDecision --> DriverRun --> PickupDropoff --> AdminTrans
    AdminTrans --> Shipper
```

### 1.0 유스케이스 관계

아래 다이어그램은 홍달 1.0 국내 화물/용달 운송에 직접 적용되는 유스케이스 관계만 추린 것입니다. 화살표는 코드에 기록된 `HongdalUseCaseRelationAttribute`의 `Source UseCase -> Target UseCase` 방향을 따릅니다.

```mermaid
flowchart LR
    ShipperActor(["화주"])
    DriverActor(["기사"])
    RecipientActor(["수령자"])
    OperatorActor(["플랫폼 운영자"])

    subgraph Core["운송 실행"]
        Request["화주운송의뢰UseCase<br/>의뢰·결제·인수증"]
        Recommendation["기사배차추천UseCase<br/>추천 목록·상세"]
        DriverProfile["용달기사프로필UseCase<br/>기사 등록·차량·역할"]
        DriverNotification["기사알림UseCase<br/>푸시토큰·알림 설정"]
    end

    subgraph Evidence["증빙·문서"]
        FileUpload["파일업로드UseCase<br/>사진·서명·첨부 업로드"]
        Document["문서관리UseCase<br/>인수증·POD 문서·감사 로그"]
        FilePod["파일POD관리UseCase<br/>POD 파일 상태 관리"]
        AuditLog["사용자행위로그조회UseCase<br/>다운로드·조회 이력"]
    end

    subgraph Operation["운영·화면·보안"]
        ViewSetting["View설정UseCase<br/>사용자 화면 설정"]
        AdminViewPolicy["관리자View정책UseCase<br/>화면 노출 정책"]
        AuxiliaryFeature["보조기능설정UseCase<br/>Command 보조 기능"]
        TransportSecurity["ISMSP전송보호UseCase<br/>전송 공개키"]
        WorkflowVersion["버전워크플로우UseCase<br/>워크플로우·관계 조회"]
    end

    ShipperActor --> Request
    RecipientActor --> Request
    DriverActor --> DriverProfile
    DriverActor --> Recommendation
    DriverActor --> DriverNotification
    OperatorActor --> FilePod
    OperatorActor --> Document
    OperatorActor --> AdminViewPolicy
    OperatorActor --> AuxiliaryFeature
    OperatorActor --> TransportSecurity
    OperatorActor --> WorkflowVersion

    Recommendation -. "<<include>>" .-> DriverProfile
    DriverNotification -. "<<include>>" .-> DriverProfile
    DriverProfile -. "<<extend>>" .-> DriverNotification
    DriverNotification -. "<<extend>>" .-> Recommendation

    Request -. "<<extend>>" .-> Document
    Request -. "<<extend>>" .-> FileUpload
    Document -. "<<include>>" .-> FileUpload
    Document -. "<<include>>" .-> AuditLog
    FilePod -. "<<include>>" .-> FileUpload
    FilePod -. "<<extend>>" .-> Document

    ViewSetting -. "<<include>>" .-> AdminViewPolicy
    ViewSetting -. "<<extend>>" .-> AuxiliaryFeature
    AdminViewPolicy -. "<<extend>>" .-> ViewSetting
    AdminViewPolicy -. "<<extend>>" .-> AuxiliaryFeature
    AuxiliaryFeature -. "<<include>>" .-> AdminViewPolicy
```

## 확장 워크플로우의 합류 지점

```mermaid
flowchart LR
    DT["홍달 1.0 국내 화물/용달 운송<br/>추천 → 수락 → 상차 → 하차 → 증빙 → 정산"]
    GP["공동주문 수입<br/>보세구역 반출 후 세대 배송 또는 3PL 운송"]
    CT["통관·무역 데이터<br/>수입 가능성·통관 상태 판단"]
    WH["창고 입출고<br/>입고/출고 물량 운송 요청"]
    SC["판매채널 출고<br/>주문 발생 후 출고·배송 인계"]
    FD["음식 배달<br/>픽업·전달형 운송"]
    HM["홍달마트<br/>피킹 후 기사 인계"]
    CM["커뮤니티 신뢰<br/>운송 완료 후기·활동 신호"]
    HR["참여 인력 관리<br/>분류·배분·보조 업무"]

    GP --> CT
    CT --> GP
    GP -->|국내 반출 후 직접 배송| DT
    GP -->|3PL 입고 선택| WH
    WH -->|출고 물량 배송| DT
    SC --> WH
    SC -->|바로 배송 인계| DT
    FD --> DT
    HM --> WH
    HM --> DT
    DT -.완료 후기·활동 신호.-> CM
    GP -.단지 내 분류·배분.-> HR
```

## 주요 엔진

홍달의 서버 판단 로직은 현재 세 엔진을 중심으로 본다. 집단화 엔진은 같은 배송권의 주문자 수요를 묶고, 출고 배치 엔진은 창고와 재고를 기준으로 출고 작업을 계획하며, 운송 의뢰 배차 엔진은 실제 기사 추천과 상하차 흐름을 책임진다.

```mermaid
flowchart LR
    Grouping["집단화 엔진<br/>같은 상품·배송권 수요 묶음"]
    Outbound["출고 배치 엔진<br/>창고·재고·피킹·포장 계획"]
    Dispatch["운송 의뢰 배차 엔진<br/>기사 추천·수락·상하차·증빙"]

    Grouping -->|공동주문 확정 후보| Outbound
    Grouping -->|직접 세대 배송 또는 3PL 입고 운송| Dispatch
    Outbound -->|출고 완료 화물| Dispatch
```

세 엔진의 역할, 입력/출력, 경계, 인계 방식은 [홍달 주요 엔진 정리](docs/Architecture/EngineOverview.md)에 둔다. 출고 배치의 상세 판단 기준은 [출고 배치 엔진](docs/Architecture/OutboundBatchEngine.md)을 따른다.

## 1.0 앱 화면 협업 지도

```mermaid
flowchart TD
    S1["ShipperApp<br/>/shipper/request<br/>운송 의뢰"]
    A1["HongdalAdmin<br/>/dispatch/wait<br/>배차 대기"]
    D1["DriverApp<br/>/driver/recommendations<br/>추천 목록"]
    D2["DriverApp<br/>/driver/recommendations/{의뢰Id}<br/>추천 상세"]
    D3["DriverApp<br/>/driver/recommendations/{의뢰Id}/decision<br/>수락·보류·거절"]
    D4["DriverApp<br/>/driver/transports/current<br/>진행 중 운송"]
    D5["DriverApp<br/>상차·하차·증빙"]
    A2["HongdalAdmin<br/>/transports, /documents, /settlements<br/>운송·증빙·정산"]
    G1["공동주문·창고·판매채널·음식·마트<br/>확장 의뢰"]

    S1 --> A1 --> D1 --> D2 --> D3 --> D4 --> D5 --> A2
    A2 --> S1
    G1 -.의뢰 유형을 붙여 합류.-> D1
```

### 대표 상태 전파 예시

```mermaid
sequenceDiagram
    participant Shipper as ShipperApp /shipper/request
    participant AdminWait as HongdalAdmin /dispatch/wait
    participant DriverReco as DriverApp /driver/recommendations
    participant DriverRun as DriverApp /driver/transports/current
    participant AdminTrans as HongdalAdmin /transports

    Shipper->>AdminWait: 운송 의뢰 등록
    AdminWait->>DriverReco: 추천 후보 노출
    DriverReco->>AdminWait: 기사 수락
    AdminWait->>DriverRun: 진행 중 운송 생성
    DriverRun->>AdminTrans: 상차 완료, 인수증/서명/사진 제출
    AdminTrans->>Shipper: 상차 완료 상태와 증빙 반영
    DriverRun->>AdminTrans: 하차 완료
    AdminTrans->>Shipper: 운송 완료와 정산 후보 상태 반영
```

## 워크플로우와 앱 화면

| 우선순위 | 워크플로우 | 주 사용자 | 현재 주요 화면 | 1.0 운송과의 관계 |
| --- | --- | --- | --- | --- |
| 1 | 국내 화물 운송 | 화주, 기사, 운영자 | `ShipperApp` `/shipper/request`, `DriverApp` `/driver/recommendations`, `/driver/transports/current`, `HongdalAdmin` `/transports` | 중심축 |
| 2 | 공동주문 수입 | 주문자 집단, 운영자, 관세사 | `OrdererApp` `/group-purchase`, `HongdalAdmin` 공동주문 원장 API, `CustomsBrokerApp` `/` | 보세구역 반출 후 세대 배송 또는 3PL 운송으로 합류 |
| 3 | 창고 입출고 | 창고 관리자, 판매자 | `WarehouseManagerApp` `/work-board`, `/work/inbound`, `ShipperApp` `/shipper/warehouse/inventory` | 입고/출고 물량이 운송 요청으로 합류 |
| 4 | 판매채널 출고 | 판매자, 창고 관리자 | `ShipperApp` `/shipper/sales/channels`, `/shipper/sales/listings`, `/shipper/sales/orders` | 주문 이행 후 창고 출고 또는 직접 배송 인계로 합류 |
| 5 | 통관·무역 데이터 | 관세사, 운영자, 판매자 | `ShipperApp` `/shipper/customs/hs-reviews`, `/shipper/international/fcl-lcl`, `HongdalAdmin` `/customs/hs-codes` | 공동주문 수입이 국내 운송으로 넘어갈 수 있는지 판단 |
| 6 | 음식 배달 | 주문자, 배달 기사, 운영자 | `OrdererApp` `/food/restaurants`, `DriverApp` 추천/진행 화면, `HongdalAdmin` `/food/operations` | 픽업·전달형 운송으로 합류 |
| 7 | 홍달마트 | 주문자, 창고 관리자, 기사 | `OrdererApp` `/food/mart`, `WarehouseManagerApp` `/mart`, `/mart/work-board` | 피킹 후 기사 인계로 합류 |
| 보조 | 커뮤니티 신뢰 | 모든 참여자 | 각 앱 홈의 커뮤니티 모드, `HongdalAdmin` `/activity-logs` | 운송 완료 후기와 공개 가능한 활동 신호를 받음 |
| 보조 | 참여 인력 관리 | 운영자, 참여 인력 | `HongdalAdmin` HR API, `OrdererApp` 공동주문 화면 | 공동주문 분류·배분·보조 업무를 지원 |

상세한 화면 관계, 상태 전파 시퀀스, 보완할 페이지 후보는 [워크플로우 앱 화면 지도](docs/ProjectOverview/workflow-app-screen-map.md)에 둡니다. 주문자 집단 공동주문, 해외 선적/통관 조회, 국내 물류대행 입고, 판매채널 출품, 출고 배치, 입주민 우선 고용 흐름은 [주문자 집단 공동주문/커머스 흐름](docs/ProjectOverview/orderer-group-commerce-flows.md)을 기준으로 관리합니다.

## 버전 방향

| 버전 | 목표 |
| --- | --- |
| `1.0` | 국내 화물/용달 운송 정보 서비스 안정화 |
| `1.5` | 판매 물류와 창고 기반 입고/출고/재위탁 확장 |
| `2.0` | 국제 물류, 통관, HS 코드 데이터 기반 확장 |
| `2.5` | 주문자 집단 기반 공동 주문과 FCL/대량 입고 |
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

## 주요 용어

- 온보딩: 새 사용자가 서비스에 처음 들어왔을 때 계정, 역할, 기본 설정, 기존 주문/창고/거래 단서를 연결해 실제 업무를 시작할 수 있게 만드는 초기 절차입니다. 예를 들어 외부 주문자가 회원가입한 뒤 주문참조번호와 본인 단서로 기존 출고예정이나 관계 후보를 찾는 과정이 온보딩에 포함됩니다.
- 출고 예약: 재고가 충분한 주문이나 운송 요청에 대해, 실제 피킹/포장 전에 해당 물량을 출고 대상으로 잡아두는 상태입니다.
- 주문자 집단: 같은 주소, 생활권, 공동주택, 초대코드 같은 단서를 공유해 공동 주문이나 공동 입고를 함께 할 수 있는 사용자 묶음입니다.
- 물류대행사/3PL: 보관, 입고, 피킹, 포장, 출고 같은 물류 업무를 대신 수행하는 외부 물류 업체입니다.
- 출고 배치: 주문이 들어왔을 때 어느 창고의 어떤 재고를 어떤 수량으로 출고할지 정하는 계획입니다.
- BL: Bill of Lading의 약자로, 선박 운송에서 화물이 선적되었음을 증명하는 선하증권입니다.

## 실행과 검증

```powershell
dotnet build Hongdal.slnx /p:UseSharedCompilation=false
dotnet test Hongdal.Tests\Hongdal.Tests.csproj /p:UseSharedCompilation=false
```

개발 DB는 MySQL을 사용합니다. 로컬 실행 중 `Unable to connect to any of the specified MySQL hosts`가 발생하면 Docker의 MySQL 컨테이너 실행 상태와 연결 문자열을 먼저 확인합니다.

## 문서

- 로드맵과 업무 흐름: [docs/ProjectOverview](docs/ProjectOverview/README.md)
- 버전별 범위: [docs/Versions](docs/Versions/README.md)
- 워크플로우 API 정책: [docs/ProjectOverview/workflow-api-policy.md](docs/ProjectOverview/workflow-api-policy.md)
- 워크플로우 앱 화면 지도: [docs/ProjectOverview/workflow-app-screen-map.md](docs/ProjectOverview/workflow-app-screen-map.md)
- 화면 처리 흐름: [docs/ProjectOverview/screen-flows.md](docs/ProjectOverview/screen-flows.md)
- 주문자 집단 공동주문/커머스 흐름: [docs/ProjectOverview/orderer-group-commerce-flows.md](docs/ProjectOverview/orderer-group-commerce-flows.md)
- 주요 엔진 정리: [docs/Architecture/EngineOverview.md](docs/Architecture/EngineOverview.md)
- 개인정보/계약 ISMS-P 준비도: [docs/Compliance/ISMS-P-readiness.md](docs/Compliance/ISMS-P-readiness.md)
- ISMS-P 보호 데이터 흐름: [docs/Compliance/ISMS-P-protected-data-flow.md](docs/Compliance/ISMS-P-protected-data-flow.md)
- Command/Event 원칙: [docs/Architecture/CommandEvent리팩토링원칙.md](docs/Architecture/CommandEvent리팩토링원칙.md)
- 참여자 중심 설계: [docs/Architecture/참여자중심설계원칙.md](docs/Architecture/참여자중심설계원칙.md)

## 개발 원칙

1. Command와 Event 책임을 분리한다.
2. API와 화면은 제품 버전보다 워크플로우와 책임 경계를 먼저 본다.
3. 앱 화면은 다음 행동, 상태, 금액, 증빙, 다음 인계 대상을 우선 노출한다.
4. 상세 설계와 긴 흐름은 `Docs/`에 두고 README는 핵심 요약만 유지한다.
