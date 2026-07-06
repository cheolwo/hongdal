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

- 1.0 범위: 국내 용달 운송에서 화주, 용달기사, 수령자 사이의 운송 정보를 정확히 연결
- 운영 레인: 계약 영역 / 인사 영역 / 비즈니스 실행 영역
- 업무 축: 화주 운송 / 음식 배달 / 판매 물류
- 앱 전략: 기능 과밀한 Super App 대신 역할별 앱 분리
- 운영 전략: 자동 계산 + Admin 승인/보류/노출 제어

## Hongdal 1.0 우선순위

1.0은 넓은 물류 플랫폼 전체를 한 번에 완성하기보다, 국내 화물/용달 운송의 핵심 참여자인 화주, 용달기사, 수령자 사이에서 필요한 정보를 정확히 제공하는 것을 최우선으로 둡니다.

| 참여자 | 1.0에서 제공해야 하는 핵심 정보 | 주요 앱/화면 |
| --- | --- | --- |
| 화주 | 의뢰 등록, 상차/하차 정보, 화물 정보, 결제/정산 조건, 배차 상태 | `ShipperApp`, 화물운송의뢰 등록, 공개 화물정보 |
| 용달기사 | 추천 운송, 상차지/하차지, 화물 제원, 결제 방식, 상차/하차 완료 증빙 | `DriverApp`, 추천 목록, 진행 중 운송, 상차/하차 화면 |
| 수령자 | 하차 예정 정보, 수령/인수 확인, 결제 또는 인수증 관련 정보 | 수령자 정보 DTO, 운송 상세, 하차 완료 흐름 |

1.0의 커뮤니티 모드는 별도 목적지가 아니라 이 세 참여자 사이의 신뢰와 소통을 보조하는 레이어입니다. 업무 모드에서 발생한 운송 경험, 문의, 후기, 문제 제기, 관계 기록이 커뮤니티 모드로 자연스럽게 이어지도록 두되, 핵심 업무 처리를 방해하지 않는 범위에서 노출합니다.

```mermaid
flowchart TD
    A["화주: 운송 의뢰 등록"] --> B["Hongdal 1.0 운송 정보 허브"]
    B --> C["용달기사: 추천 / 수락 / 운행"]
    B --> D["수령자: 하차 / 인수 / 결제 확인"]
    C --> E["상차 완료 사진 / 상태 변경"]
    E --> F["하차 완료 사진 / 인수 확인"]
    F --> G["정산 / 기록 / 관계 스냅샷"]
    G --> H["커뮤니티 모드: 후기, 문의, 신뢰 기록"]
    H --> B
```

음식 배달, 홍달마트, 창고 고도화, 해외/통관, HS 코드, 공동구매는 1.0 이후 확장 축으로 유지합니다. 다만 코드 구조는 이미 분리 가능하게 두고, 1.0에서는 `CargoYongdalDriverApp`, `CargoYongdalDispatchEngine`, 화물 배달 건 정리를 중심으로 우선 완성도를 높입니다.

## 단계별 형상 관리안

프로젝트는 기능을 한 번에 넓히기보다 버전별로 안정화 대상을 분리합니다. 각 버전은 코드 브랜치, 문서, 앱 노출 범위, DB 마이그레이션을 같은 기준으로 맞춥니다.

형상 관리의 기본 원칙은 **소스 프로젝트는 앱/도메인 기준으로 유지하고, 버전별 관리는 문서/브랜치/태그/마이그레이션/노출 정책으로 분리**하는 것입니다. `DriverApp`, `ShipperApp`, `WarehouseManagerApp`, `Hongdal.Domain` 같은 실행 코드를 `v1.0`, `v1.5` 폴더로 복제하지 않습니다. 대신 버전별 목표와 체크리스트는 [Docs/Versions](Docs/Versions/README.md)에서 관리합니다.

| 관리 대상 | 기준 |
| --- | --- |
| 소스 프로젝트 | 앱/도메인 경계 기준으로 유지합니다. 버전 때문에 프로젝트를 복제하지 않습니다. |
| 버전 문서 | `Docs/Versions/v1.0`, `v1.5`, `v2.0`, `v2.5`, `v3.0`에서 목표, 범위, 체크리스트, 마이그레이션을 관리합니다. |
| 브랜치 | 안정화는 `release/{version}`, 기능 개발은 `feature/{version}-{domain}-{topic}` 형태로 관리합니다. |
| 태그 | 검증 가능한 기준점은 `v1.0.0`, `v1.5.0`, `v2.0.0`, `v2.5.0`, `v3.0.0`처럼 남깁니다. |
| 기능 노출 | 구현되어 있어도 해당 버전 범위가 아니면 기본 UI 노출이나 운영 플래그를 끕니다. |
| DB 변경 | 다음 버전 실험 테이블이 이전 버전 운영 흐름을 깨지 않게 migration notes에 기록합니다. |

| 버전 | 목표 | 우선 앱/모듈 | 포함 범위 | 보류 범위 |
| --- | --- | --- | --- | --- |
| `1.0` | 국내 화물/용달 운송 정보 서비스 안정화 | `ShipperApp`, `DriverApp`, `HongdalAdmin`, `CargoYongdalDispatchEngine` | 화주 의뢰, 용달기사 추천/수락/거절, 상차/하차 증빙, 수령자 정보, 결제/정산 기본, 커뮤니티 보조 모드 | 음식 배달 실운영, 홍달마트 실운영, 해외/통관 자동화, HS 데이터 유료 공유 |
| `1.5` | 판매 물류와 창고 기반 출고/재위탁 확장 | `WarehouseManagerApp`, `ShipperApp`, `OrdererApp` | 판매상품 재고, 입고/적재/출고, 주문 출고 알림, 재위탁 운송, 창고 작업자 검증, 판매자 물류 운영 | 홍달마트 도심 즉시배송, 해외/통관 자동화 |
| `2.0` | 국제 물류/통관/HS 데이터 기반 확장 | `CustomsBrokerApp`, HS 코드 DB, 해외/통관 서비스, `CargoYongdalDispatchEngine` | HS 코드 DB, 통관/수입 대행 조회, 관세사 전용 앱, FCL/LCL 판단, 수입 예정 수요 확인, 통관 데이터 공개/결제 정책 | 홍달마트 즉시배송 실운영 |
| `2.5` | 공동주택 기반 공동 주문과 FCL/대량 입고 | `OrdererApp`, `ShipperApp`, `WarehouseManagerApp`, 공동 주문 서비스 | 공동주택 단지 식별, 주민 공동 주문 모집, 화주 대량 구매 공개, FCL 가능 조건 계산, 단지 대표 입고, 동/수령 지점별 분류/배분 | 홍달마트 즉시배송 실운영, 관리사무소 공식 승인 자동화 |
| `3.0` | 홍달마트와 도심 즉시배송 운영 | `WarehouseManagerApp`, `OrdererApp`, `FoodDeliveryDispatchEngine`, `Deliver` | 홍달마트 주문, 도심 재고 보충, 피킹/포장, 음식 배달 기사 픽업, 묶음 배달, 도심 마트 커뮤니티 운영 | 1.0 핵심 운송 흐름을 흔드는 대규모 구조 변경 |

### 브랜치와 릴리스

| 구분 | 규칙 |
| --- | --- |
| 기본 개발 | `main`은 항상 빌드 가능한 상태를 유지합니다. 실험성 기능은 기능 브랜치에서 진행합니다. |
| 버전 안정화 | `release/1.0`, `release/1.5`, `release/2.0`, `release/2.5`, `release/3.0` 브랜치를 두고, 해당 버전 범위의 버그 수정과 문서 보강만 반영합니다. |
| 기능 브랜치 | `feature/dispatch-cargo-*`, `feature/customs-*`, `feature/warehouse-*`, `feature/mart-*`처럼 버전 축과 도메인이 보이게 이름을 붙입니다. |
| 태그 | 실제 기준점은 `v1.0.0`, `v1.5.0`, `v2.0.0`, `v2.5.0`처럼 태그로 남깁니다. |
| 마이그레이션 | DB 마이그레이션은 버전 범위와 맞춰 설명을 남기고, 1.0 안정화 후에는 1.5 실험 테이블이 1.0 운영 흐름을 깨지 않게 합니다. |

### 버전별 판단 기준

```mermaid
flowchart TD
    A["새 기능 아이디어"] --> B{"화주-용달기사-수령자 국내 운송 안정화에 직접 필요한가"}
    B -->|예| C["1.0 후보"]
    B -->|아니오| F{"판매 물류/창고 출고/재위탁인가"}
    F -->|예| G["1.5 후보"]
    F -->|아니오| D{"국제 물류/통관/HS 데이터인가"}
    D -->|예| E["2.0 후보"]
    D -->|아니오| O{"공동주택 공동 주문/FCL 단지 입고인가"}
    O -->|예| P["2.5 후보"]
    O -->|아니오| H{"홍달마트/도심 즉시배송인가"}
    H -->|예| L["3.0 후보"]
    H -->|아니오| M["보류 또는 커뮤니티 아이디어로 기록"]
    C --> I["1.0 release 브랜치 안정화 기준으로 검토"]
    E --> J["2.0 통관/HS 데이터 모델부터 정리"]
    G --> K["1.5 창고/판매 물류 기능 브랜치에서 실험"]
    P --> Q["2.5 공동주택/공동 주문/FCL 입고 모델 실험"]
    L --> N["3.0 도심 운영 설계 후 실험"]
```

1.0에서는 기능을 줄이는 판단도 적극적으로 합니다. 화주, 용달기사, 수령자 사이의 국내 운송 정보가 더 명확해지지 않는 기능은 구현되어 있더라도 기본 노출에서 숨기거나 2.0 이후 기능으로 문서화합니다.

## 현재 솔루션 구성 (Hongdal.slnx)

| 프로젝트 | 역할 | TFM |
| --- | --- | --- |
| `Hongdal` | ASP.NET Core API Host, Controller, Application 조립 | `net10.0` |
| `Hongdal.Domain` | 핵심 도메인 모델(사용자, 계약, 물류, 설정 등) | `net10.0` |
| `Hongdal.Contracts` | 서버/클라이언트 공용 계약 DTO | `net10.0` |
| `Hongdal.Infrastructure` | EF Core/Identity/Persistence/보안 | `net10.0` |
| `Hongdal.Ui.Common` | 공통 UI/백오피스 영역 컴포넌트 | `net10.0` |
| `HongdalAdmin` | 관리자 앱(운영 제어) | `net10.0` |
| `Hongdal.BackOffice.Client` | 백오피스 클라이언트 계층 | `net10.0` |
| `Hongdal.FoodApi` | 음식 도메인 API 분리 영역 | `net10.0` |
| `DriverApp` | 기사 앱 (.NET MAUI Android) | `net10.0-android` |
| `ShipperApp` | 화주/판매자 앱 (.NET MAUI Android) | `net10.0-android` |
| `WarehouseManagerApp` | 창고 현장 앱 (.NET MAUI Android) | `net10.0-android` |
| `HumanResourcesManagerApp` | 인력 관리자 앱 (.NET MAUI Windows) | `net10.0-windows10.0.19041.0` |

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

## 기사 앱 경계

기사 앱은 공통 지도, 운행 시작, 추천/수락/거절, 정산, 알림 기능을 공유하되 앱 경계는 두 가지로 나눕니다. 화물과 용달은 같은 앱에서 처리하고, 음식 배달은 별도 앱 흐름으로 분리합니다.

| 구분 | 앱 식별자 | 포함 업무 | 우선 확인 정보 |
| --- | --- | --- | --- |
| 화물/용달 기사 | `CargoYongdalDriverApp` | `CargoTransport`, `YongdalTransport` | 차량 제원, FCL/LCL, 상하차 조건, 현재 위치, 픽업 거리, 현장 결제 |
| 음식 배달 기사 | `FoodDeliveryDriverApp` | `FoodDelivery` | 조리/픽업 시간, 고객 도착 시간, 묶음 배달 가능 여부 |

현재 `DriverApp`은 공통 기사 앱 껍데기와 화물/용달 기사 흐름을 기본값으로 사용합니다. 이후 음식 배달 기사 앱은 같은 공통 컴포넌트를 공유하되 추천 목록, 진행 중 업무 카드, 정산 문구, 알림 정책을 `FoodDeliveryDriverApp` 식별자 기준으로 분리합니다.

## 배차 엔진 경계

배차는 주문 유입 경로보다 실제 운송 성격을 기준으로 엔진을 나눕니다. 음식 주문은 음식 배달 배차 엔진으로 보내고, 화물 운송 의뢰나 주문자가 직접 만든 화물/공산품 운송 요청은 화물/용달 배차 엔진으로 보냅니다.

| 엔진 | 배차업무유형 | 주요 대상 | 우선 판단 기준 |
| --- | --- | --- | --- |
| `CargoYongdalDispatchEngine` | `용달운송` | 화주 운송 의뢰, 주문자 화물/공산품 운송 요청, FCL/LCL 연계 운송 | 차량 적합성, 상하차 조건, 거리/복귀지, 운임/비용, 일정 삽입 가능성 |
| `FoodDeliveryDispatchEngine` | `음식배달` | 음식점 주문, 홍달마트 주문, 즉시 픽업 배달 | 조리/픽업 시간, 고객 도착 시간, 묶음 배달 가능성, 짧은 반경 기사 위치 |

```mermaid
flowchart TD
    A["주문 / 운송 의뢰 유입"] --> B{"운송 성격 판정"}
    B -->|음식점 / 홍달마트 즉시 배달| C["FoodDeliveryDispatchEngine"]
    B -->|화물 / 용달 / 공산품 / 수입화물 운송| D["CargoYongdalDispatchEngine"]
    C --> E["음식배달배차업무정책"]
    D --> F["용달운송배차업무정책"]
    E --> G["배달기사 후보 선정"]
    F --> H["화물/용달기사 후보 선정"]
    G --> I["배차추천 / 공개배차 / 확정"]
    H --> I
```

현재 코드에서는 `배차추천후보선정Service`가 `배차대기.배차업무유형`을 보고 `I배차엔진`을 선택합니다. 엔진은 다시 세부 `I배차업무정책`으로 후보 선정 알고리즘을 위임합니다. 이 구조 덕분에 음식 배달은 짧은 시간창과 묶음 배달 중심으로, 화물/용달은 차량 제원과 상하차 조건 중심으로 독립적으로 발전시킬 수 있습니다.

### 화물 배달 건 정리

화물 배달 건은 “누가 주문했는가”보다 “어떤 운송 단위인가”를 먼저 봅니다. 같은 화주 운송 의뢰라도 독차/FCL, 혼적/LCL, 수입 통관 연계, 창고 출고 연계에 따라 배차 판단 기준이 달라집니다.

| 흐름 | 원본의뢰유형 | 배차 시작 조건 | 배차 시 우선 확인 |
| --- | --- | --- | --- |
| 화주 운송 의뢰 | `CargoTransport` | 결제 완료, 후불 승인, 현장지급 승인 후 `배차대기` 생성 | 상차지, 하차지, 화물 제원, 운임, 결제/정산 조건 |
| 주문자 화물/공산품 운송 | `OrdererCargoOrder` | 주문자가 운송 요청을 확정하고 결제/승인 조건이 충족됨 | 주문자 연락 가능 여부, 상품 크기, 파손 주의, 픽업/하차 주소 |
| FCL/독차 운송 | `FclCargoTransport` | 컨테이너 또는 차량 단위 운송 조건이 확정됨 | 차량 제원, 팔레트 수, 중량, 상하차 장비, 시간창 |
| LCL/혼적 운송 | `LclCargoTransport` | 혼적 가능 조건과 경유 가능 시간이 확인됨 | 온도/파손 민감도, 하차 순서, 경유 가능 시간, 합짐 가능성 |
| 수입/통관 연계 운송 | `ImportCargoTransport` | 통관 또는 반출 가능 상태가 확인됨 | 통관 상태, 보세/창고 위치, 반출 가능 시각, HS 코드 위험 태그 |
| 창고 출고 연계 운송 | `WarehouseOutboundCargo` | 피킹/포장 또는 출고예정 상태가 확인됨 | 출고 준비 상태, 적재 위치, 상차 가능 시각, 하차지 결제 조건 |

```mermaid
flowchart TD
    A["화물 운송 요청 유입"] --> B{"원본의뢰유형 / 운송방식 판정"}
    B -->|CargoTransport| C["화주 운송 의뢰"]
    B -->|OrdererCargoOrder| D["주문자 화물/공산품 운송"]
    B -->|FCL / 독차| E["FCL/독차 운송"]
    B -->|LCL / 혼적| F["LCL/혼적 운송"]
    B -->|ImportCargoTransport| G["수입/통관 연계 운송"]
    B -->|WarehouseOutboundCargo| H["창고 출고 연계 운송"]
    C --> I["결제/승인 확인"]
    D --> I
    E --> J["차량 제원 / 팔레트 / 중량 확인"]
    F --> K["혼적 가능성 / 시간창 확인"]
    G --> L["통관 / 반출 가능 상태 확인"]
    H --> M["피킹 / 포장 / 출고예정 확인"]
    I --> N["CargoYongdalDispatchEngine"]
    J --> N
    K --> N
    L --> N
    M --> N
    N --> O["용달운송배차업무정책"]
    O --> P["차량 적합성 + 거리 + 복귀지 + 일정 삽입 + 예상수익 평가"]
    P --> Q["화물/용달 기사 추천"]
```

코드에서는 `화물용달배차흐름Resolver`가 `배차대기.원본의뢰유형`과 `화주운송의뢰.운송방식`을 해석합니다. 현재는 배차 동작을 막지는 않고, `CargoYongdalDispatchEngine`이 후보 선정 전에 흐름 컨텍스트를 확인합니다. 이후에는 흐름별로 FCL/LCL 요율, 통관 완료 게이트, 창고 출고 완료 게이트, 혼적 가능성 평가를 각각 독립적으로 고도화합니다.

### 음식 배달 배차 하위 흐름

음식 배달 엔진 안에서도 배차 시작 시점은 두 갈래입니다.

| 흐름 | 원본의뢰유형 | 배차 시작 조건 |
| --- | --- | --- |
| 음식점 즉시 배달 | `RestaurantFoodOrder`, `FoodOrder` | 결제 승인과 조리 접수 후 바로 배차 가능 |
| 홍달마트 준비 중 배달 | `HongdalMartOrder`, `MartFoodOrder` | 재고 확인, 피킹, 포장 완료 전에는 배차 보류 |
| 홍달마트 포장 완료 배달 | `HongdalMartPackedOrder` | 포장 완료 후 배달기사 픽업 배차 가능 |

```mermaid
flowchart TD
    A["음식 주문 결제 승인"] --> B{"주문 출처"}
    B -->|음식점| C["조리 접수 / 픽업 예상시각 산정"]
    C --> D["FoodDeliveryDispatchEngine 즉시 배차"]
    B -->|홍달마트| E["창고 주문 생성"]
    E --> F["재고 확인"]
    F --> G["피킹 작업"]
    G --> H["포장 작업"]
    H --> I["HongdalMartPackedOrder 배차대기 생성"]
    I --> J["FoodDeliveryDispatchEngine 배차"]
    D --> K["배달기사 추천 / 수락 / 픽업"]
    J --> K
```

코드에서는 `음식배달배차흐름Resolver`가 `배차대기.원본의뢰유형`을 해석합니다. 홍달마트 주문이 아직 `HongdalMartPackedOrder`가 아니면 `음식배달배차엔진`은 후보 선정을 하지 않고, 창고의 피킹/포장 완료를 기다립니다.

## 화면 기능 처리 흐름

화면에 보이는 버튼과 카드가 내부적으로 어떻게 처리되는지 문서 기준을 둡니다. 현재 일부 화면은 샘플 서비스로 상태를 갱신하고 있으며, 서버 연동 시에는 같은 지점에서 Command/API 호출로 바꿉니다.

### 공통 홈 모드 전환

`PlatformCommunityHome`은 앱별 홈을 커뮤니티 모드와 업무 모드로 나눕니다. 모드 버튼은 공통 `PlatformHomeModeStateService`의 `IsWorkMode` 값을 바꾸고, 각 앱은 같은 상태값을 보고 커뮤니티 콘텐츠 또는 업무 콘텐츠를 렌더링합니다.

```mermaid
flowchart TD
    A["사용자: 커뮤니티/업무 모드 버튼 선택"] --> B["PlatformModeBar / NavMenu"]
    B --> C["PlatformHomeModeStateService.SetWorkMode"]
    C --> D["Changed 이벤트 발행"]
    D --> E["PlatformCommunityHome 상태 동기화"]
    E --> F{"IsWorkMode"}
    F -->|false| G["커뮤니티 게시판 / 공지 / 공유글 표시"]
    F -->|true| H["앱별 업무 대시보드 표시"]
```

### DriverApp 추천과 배차 처리

DriverApp 홈의 추천 목록, 추천 상세, 배차 처리 화면은 같은 추천 의뢰 데이터를 기준으로 움직입니다. 현재 앱 내부에서는 `DriverRecommendationDecisionService`가 수락/보류/거절 상태를 샘플 상태로 저장합니다. 서버 연동 시에는 이 위치가 `배차추천수락Command`, `배차추천보류Command`, `배차추천거절Command` 같은 Command 호출 지점이 됩니다.

```mermaid
flowchart TD
    A["DriverApp 홈 / 추천 목록"] --> B["IDriverSampleDataService.추천의뢰목록"]
    B --> C["추천 상세 화면"]
    C --> D["배차 처리 화면"]
    D --> E{"기사 선택"}
    E -->|수락| F["Accept: 배차상태=수락, 상태=수락완료"]
    E -->|보류| G["Hold: 배차상태=보류, 상태=검토중"]
    E -->|거절| H["Reject: 배차상태=거절, 상태=추천제외"]
    F --> I["Changed 이벤트 / 화면 갱신"]
    G --> I
    H --> I
    I --> J["서버 전환 시 Command 처리와 배차 이벤트 발행"]
```

### DriverApp 상차/하차 완료 사진 처리

상차 완료와 하차 완료 화면은 모바일 카메라에서 사진을 받은 뒤 완료 처리와 연결됩니다. 샘플 구현은 업로드 대상 경로를 계산하고, HTTP 구현은 파일 업로드 후 운송 완료 API를 호출합니다.

```mermaid
flowchart TD
    A["상차 화면 또는 하차 화면"] --> B["카메라 촬영 / 사진 선택"]
    B --> C["DriverTransportCompletionPhoto 생성"]
    C --> D["IDriverTransportCompletionPhotoService.CompleteWithPhotoAsync"]
    D --> E{"구현 방식"}
    E -->|Sample| F["driver-transports/{id}/pickup-complete 또는 dropoff-complete 경로 계산"]
    E -->|HTTP| G["POST api/v1/files/upload"]
    G --> H["commandName + referenceId + 파일 저장"]
    H --> I{"사진 종류"}
    I -->|상차| J["POST api/v1/driver/transports/{id}/pickup-complete"]
    I -->|하차| K["POST api/v1/driver/transports/{id}/complete"]
    J --> L["운송 상태 갱신 / 이벤트 후속처리"]
    K --> L
```

### WarehouseManagerApp 작업 진입

창고 앱의 입고와 포장 흐름은 먼저 휴대폰 번호 뒤 8자리로 작업자를 확인하고, 다음 화면에서 작업대 바코드를 확인합니다. 입고는 작업대 확인 후 상품 바코드 스캔과 검수 화면으로 이어지고, 포장은 작업 보드로 이어집니다.

```mermaid
flowchart TD
    A["입고/출고/포장 작업 시작 화면"] --> B["휴대폰 번호 뒤 8자리 입력"]
    B --> C["IWarehouseWorkEntryGateService.VerifyAsync"]
    C --> D{"작업자/역할 검증"}
    D -->|실패| X["오류 표시 / 다음 화면 차단"]
    D -->|성공| E{"공정 유형"}
    E -->|입고| F["입고 작업대 바코드 화면"]
    E -->|포장| G["포장 작업대 바코드 화면"]
    E -->|출고/기타| H["작업 보드 이동"]
    F --> I["WB:IN-* 작업대 확인"]
    G --> J["WB:PK-* 작업대 확인"]
    I --> K["상품 바코드 스캔"]
    K --> L["입고 예정 매칭 / 현장 임시 입고 편입"]
    L --> M["입고 검수 화면"]
    J --> H
```

## 창고 업무 흐름

입고, 적재, 출고는 같은 재고를 다루지만 책임 지점이 다릅니다. 입고는 물건을 시스템에 편입하는 흐름이고, 적재는 입고된 물건을 실제 보관 위치에 배치하는 흐름이며, 출고는 주문이나 운송 요청에 맞춰 재고를 예약하고 밖으로 내보내는 흐름입니다.

### 입고 흐름

```mermaid
flowchart TD
    A["입고 발생"] --> B{"입고 흐름 유형"}
    B -->|계약 기반 입고| C["계약 DB / 입고계약스냅샷 확인"]
    B -->|현장 임시 입고| D["입고 관리자 수기 등록"]
    B -->|주문 자동 입고 예정| E["주문/구매 이벤트로 입고예정 생성"]
    C --> F["입고요청 저장"]
    D --> F
    E --> F
    F --> G["작업자 확인"]
    G --> H["입고 작업대 바코드 확인"]
    H --> I["상품 바코드 스캔"]
    I --> J{"입고 예정 매칭"}
    J -->|매칭됨| K["예정 수량 / 실제 수량 확인"]
    J -->|미매칭| L["현장 임시 입고로 편입"]
    K --> M["입고 확인 상태 변경"]
    L --> M
    M --> N["검수 작업으로 이동"]
```

### 적재 흐름

```mermaid
flowchart TD
    A["입고 확인 완료"] --> B["검수 작업"]
    B --> C{"검수 결과"}
    C -->|정상| D["적재 대상 재고 확정"]
    C -->|수량 차이 / 파손| E["검수 사유 기록"]
    E --> D
    D --> F["보관 구역 / 랙 / 적재함 선택"]
    F --> G["위치 바코드 스캔"]
    G --> H["입고상품 보관위치 갱신"]
    H --> I["재고이력 / 재고이동 기록"]
    I --> J["판매 / 재위탁 / 출고 가능 재고로 노출"]
```

### 출고 흐름

```mermaid
flowchart TD
    A["주문 또는 운송 요청 발생"] --> B["판매상품 / 입고상품 매핑"]
    B --> C{"계약상 판매 가능 여부"}
    C -->|불가| X["출고 보류 / 운영 확인"]
    C -->|가능| D["가용 재고 조회"]
    D --> E{"재고 충분"}
    E -->|부족| Y["재고 부족 알림 / 입고 필요 후보"]
    E -->|충분| F["재고 예약"]
    F --> G["피킹 작업 생성"]
    G --> H["상품 / 적재함 바코드 스캔"]
    H --> I["포장 작업"]
    I --> J["출고예정 상태 변경"]
    J --> K["배차 / 배송 / 재위탁 운송 연결"]
```

### 주문 발생 시 창고 알림 흐름

```mermaid
flowchart TD
    A["주문 결제 완료"] --> B["주문결제완료 이벤트 발행"]
    B --> C["판매자 기본 출고창고 확인"]
    B --> D{"주문자가 플랫폼 참여자인가"}
    C --> E["출고예정 생성"]
    D -->|예| F["주문자 기본 입고창고 확인"]
    D -->|아니오| M["주문자 입고 예정 / 알림 생략"]
    F --> N["주문 자동 입고 예정 생성"]
    E --> G["창고 출고 알림 생성"]
    N --> H["입고 예정 목록에 노출"]
    G --> I["창고 관리자 앱 알림"]
    I --> J["업무 모드 / 작업 보드 표시"]
    J --> K["피킹 / 포장 / 출고 작업 진입"]
    H --> L["입고 관리자 확인 대상"]
```

주문 결제 완료 후에는 판매자 창고에 출고 준비 알림이 생성됩니다. 다만 주문자가 플랫폼 참여자로 확인될 때만 주문자 기본 입고창고와 자동 입고 예정이 생성됩니다. 외부 주문자라면 주문자 입고 예정 알림은 생략하고, 판매자 창고의 피킹/포장/출고 흐름만 진행합니다.

### 외부 주문자 가입 후 연결 후보 흐름

```mermaid
flowchart TD
    A["외부 주문자 회원가입"] --> B["이메일 / 연락처 / 표시이름 / 주문참조번호 입력"]
    B --> C["가입 온보딩 후보 조회"]
    C --> D["출고예정 / 입고요청 / 주문참조번호 검색"]
    D --> E{"가입자 단서와 주문자 식별값 일치"}
    E -->|불일치| X["후보 미노출 / 고객센터 확인"]
    E -->|일치| F["관련 판매자 / 창고 / 통관 후보 구성"]
    F --> G["개인정보는 마스킹해서 후보 표시"]
    G --> H{"사용자가 연결 요청 선택"}
    H -->|선택 안 함| Y["가입만 완료"]
    H -->|선택| I["인연연결요청 생성"]
    I --> J["상대방 수락 / 거절"]
    J --> K["수락 시 필요한 범위만 연락처 공개"]
```

가입자가 과거에는 외부 주문자였더라도, 회원가입 후 주문참조번호와 본인 단서가 맞으면 관련 판매자나 업무 참여자 후보를 확인할 수 있습니다. 이때 플랫폼은 곧바로 연결하지 않고, 후보를 마스킹해서 보여준 뒤 사용자가 선택한 경우에만 기존 `인연연결요청` 흐름으로 이어갑니다.

서버에서는 이 후보 조회를 회원가입/인증 온보딩 흐름으로 보고 `POST /api/v1/auth/onboarding/connection-candidates`에서 처리합니다. 컨트롤러는 얇게 유지하고, 실제 후보 검색은 `I가입온보딩인연후보Service`로 분리합니다.

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
