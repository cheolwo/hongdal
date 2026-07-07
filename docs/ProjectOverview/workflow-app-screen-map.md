# 워크플로우 앱 화면 지도

이 문서는 홍달의 워크플로우가 실제 앱 화면에서 어떻게 성립되는지 정리한다. 워크플로우는 하나의 API나 하나의 화면이 아니라, 여러 앱의 화면이 같은 업무 원장을 보고 단계적으로 호출되면서 완성되는 흐름이다.

기본 정책은 [workflow-api-policy.md](workflow-api-policy.md)를 따른다. 이 문서는 그 정책을 화면, 앱, 부족한 페이지 후보 관점으로 풀어쓴다.

## 읽는 방법

| 구분 | 의미 |
| --- | --- |
| 현재 화면 | 프로젝트 안에 이미 존재하는 앱 페이지 또는 라우트 |
| 보완 후보 | 워크플로우를 더 자연스럽게 성립시키기 위해 추가하거나 강화할 화면 |
| 주 사용자 | 해당 절차를 가장 자주 직접 조작하는 사용자 |
| 보조 참여자 | 같은 워크플로우 안에서 확인, 보정, 승인, 작업을 맡는 참여자 |
| 인계 | 한 워크플로우의 결과가 다음 워크플로우의 입력이 되는 지점 |

## 앱별 기본 역할

| 앱 | 주 사용자 | 현재 역할 |
| --- | --- | --- |
| `OrdererApp` | 주문자, 주문자 집단 대표 | 음식/마트 주문, 화물 주문, 수입 공동구매, 주문 내역, 커뮤니티 진입 |
| `ShipperApp` | 화주, 판매자, 물류 의뢰자 | 운송 의뢰, 대량 의뢰, 창고 재고, 판매채널, HS 코드 검토, 국제 운송 계획 |
| `DriverApp` | 용달 기사, 배달 기사 | 운행 시작, 추천 의뢰 확인, 배차 수락/거절, 상차/하차 증빙, 정산 |
| `WarehouseManagerApp` | 창고 관리자, 현장 작업자 | 입고, 검수, 스캔, 피킹, 포장, 출고, 홍달마트 작업 |
| `HongdalAdmin` | 플랫폼 운영자 | 배차 대기, 운송 진행, 기사/화주 관리, 문서, 정산, HS 코드, HR, 운영 정책 |
| `CustomsBrokerApp` | 관세사 | 통관·무역 데이터 검토와 보정의 사용자 접점 |

## 전체 연결 그림

```mermaid
flowchart LR
    O["OrdererApp<br/>주문자·주문자 집단"]
    S["ShipperApp<br/>화주·판매자"]
    D["DriverApp<br/>기사"]
    W["WarehouseManagerApp<br/>창고"]
    A["HongdalAdmin<br/>플랫폼 운영"]
    C["CustomsBrokerApp<br/>관세사"]

    O -->|공동주문·음식·마트 주문| A
    O -->|공동수입 상태 조회| A
    S -->|운송 의뢰·판매채널 주문| A
    S -->|창고 입출고 요청| W
    A -->|추천·배차·운송 상태| D
    D -->|상차·하차·증빙| A
    C -->|HS/통관 보정| A
    A -->|입고·출고 작업 생성| W
    W -->|재고·출고 결과| S
    W -->|마트 피킹·기사 인계| D
```

## 국내 화물 운송

국내 화물 운송은 홍달 1.0에서 시작한 핵심 실행 워크플로우다. 공동주문 수입, 창고 입출고, 판매채널 출고, 음식 배달, 홍달마트도 실제 상차와 하차가 필요하면 이 흐름으로 합류한다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 화주 | `ShipperApp` `/shipper/request`, `/shipper/request/bulk`, `/shipper/public-cargo` | `api/v1/shipper/requests` |
| 기사 | `DriverApp` `/driver/work/start`, `/driver/recommendations`, `/driver/recommendations/{의뢰Id}`, `/driver/recommendations/{의뢰Id}/decision` | `api/v1/driver/recommendations`, `api/v1/driver/dispatch-actions` |
| 기사 | `DriverApp` `/driver/transports/current`, `/driver/transports/{운송Id}/pickup`, `/driver/transports/{운송Id}/dropoff` | `api/v1/driver/transports`, `api/v1/files` |
| 플랫폼 운영자 | `HongdalAdmin` `/dispatch/wait`, `/transports`, `/drivers/operating`, `/documents`, `/settlements` | `api/v1/admin/transports`, `api/v1/transport-events`, `api/v1/admin/documents` |

```mermaid
sequenceDiagram
    participant Shipper as ShipperApp 화주 화면
    participant Admin as Hongdal 서버/관리자
    participant Driver as DriverApp 기사 화면

    Shipper->>Admin: 운송 의뢰 등록
    Admin->>Driver: 추천 의뢰 노출
    Driver->>Admin: 수락, 보류, 거절
    Admin->>Driver: 진행 중 운송 생성
    Driver->>Admin: 상차 증빙, 인수증 서명/생략 사유
    Driver->>Admin: 하차 증빙, 완료 처리
    Admin->>Shipper: 진행 상태와 증빙 공개
    Admin->>Driver: 정산 후보 생성
```

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 운송 의뢰 상세/분쟁 상세 | `ShipperApp` | 화주가 비용, 증빙, 정산 예정, 분쟁 상태를 한 화면에서 확인해야 한다. |
| 업무 유형이 강조된 추천 상세 | `DriverApp` | 일반 화물, 공동주문 세대 배송, 음식, 마트 인계를 기사에게 명확히 구분해 보여야 한다. |
| 수령자 확인 화면 | `OrdererApp` 또는 별도 공개 화면 | 하차 완료 후 수령자가 확인, 이의 제기, 사진 확인을 할 수 있어야 한다. |
| 운송 워크플로우 운영 보드 | `HongdalAdmin` | 모든 운송 건을 워크플로우 단계 기준으로 모아 보고 예외를 처리해야 한다. |

## 공동주문 수입

공동주문 수입은 주문자 집단이 해외 상품을 공동으로 구매하고, 해외 선적, 통관, 국내 반출, 국내 운송 또는 3PL 입고까지 이어지는 흐름이다. 주문자 집단이 사업자 주체가 아닐 수 있으므로 플랫폼 위임, 운영 주체, 비용 정산 경계를 화면에서 분명히 보여야 한다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 주문자, 주문자 집단 대표 | `OrdererApp` `/group-purchase` | `api/v1/orderer/group-purchase-logistics-workflows`, `api/v1/orderer/group-purchase-overseas-shipments` |
| 주문자 | `OrdererApp` `/orders` | 공동주문 상태와 개인 주문 확인 |
| 플랫폼 운영자 | `HongdalAdmin` `/dashboard`, `/documents`, `/activity-logs` | `api/v1/admin/orderer/group-purchase-*`, `api/v1/admin/documents` |
| 관세사 | `CustomsBrokerApp` `/` | `api/v1/customs`, `api/v1/admin/hs-codes` |
| 화주·판매자 | `ShipperApp` `/shipper/customs/hs-reviews`, `/shipper/international/fcl-lcl` | HS 검토, FCL/LCL 계획 |
| 기사 | `DriverApp` 국내 화물 운송 화면 | 국내 보세구역 반출 이후 운송 |
| 창고 관리자 | `WarehouseManagerApp` `/work/inbound`, `/work-board` | 3PL 입고 또는 판매채널 출고 준비 |

```mermaid
flowchart TD
    A["OrdererApp: 공동주문 생성/참여"] --> B["공동주문 원장"]
    B --> C{"판매자 출처"}
    C -->|국내| D["국내 판매자 포장/상차 인계"]
    C -->|해외| E["해외 판매자 또는 배송대행지<br/>BL/AWB, 문서관리번호 등록"]
    E --> F["통관·무역 데이터<br/>UNI-PASS, HS 코드, 관세사 검토"]
    F --> G["국내 보세구역 반입"]
    G --> H{"국내 처리 방식"}
    H -->|기본| I["국내 화물 운송<br/>용달 기사 세대 배송"]
    H -->|선택| J["창고 입출고<br/>3PL 입고"]
    J --> K["판매채널 출고<br/>스마트스토어/쿠팡 주문 이행"]
    I --> L["커뮤니티 신뢰<br/>후기, 활동 신호, 투표"]
    K --> L
```

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 공동주문 투표/결정 화면 | `OrdererApp` | 운송 방식, 세대 배송 범위, 3PL 입고 여부, 비용 부담 방식을 집단이 합의해야 한다. |
| 공동주문 수입 원장 콘솔 | `HongdalAdmin` | BL/AWB, 문서관리번호, 통관 상태, 보세구역, 국내 운송 인계를 운영자가 한 곳에서 봐야 한다. |
| 해외 판매자/배송대행지 업로드 화면 | 별도 파트너 화면 또는 `ShipperApp` 확장 | 라벨, 상품 정보 스티커, 포장 단위, 선적 증빙을 외부 참여자가 입력할 접점이 필요하다. |
| 기사 세대 배송 상세 | `DriverApp` | 송장 필수 배송과 품목 스티커만 필요한 배송을 구분하고, 동/호수 분배 책임을 표시해야 한다. |
| 공동주문 수령/분배 체크리스트 | `OrdererApp` | 주문자 집단이 세대별 수령, 미수령, 파손, 추가 비용을 확인해야 한다. |

## 창고 입출고

창고 입출고는 물품이 창고에 들어와 재고화되고, 피킹, 포장, 출고 배치로 이어지는 흐름이다. 공동주문 수입과 판매채널 출고가 모두 이 워크플로우를 호출할 수 있다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 창고 관리자 | `WarehouseManagerApp` `/work/inbound`, `/work/outbound`, `/work/packing`, `/scan`, `/work-board` | `api/v1/warehouse-operations` |
| 창고 관리자 | `WarehouseManagerApp` `/work/inbound/products`, `/work/inbound/inspection`, `/work/{ProcessCode}/workbench` | 입고 검수, 작업대 스캔 |
| 화주·판매자 | `ShipperApp` `/shipper/warehouse/workspace`, `/shipper/warehouse/inventory`, `/shipper/warehouse/scan` | 재고, 입고 상태 확인 |
| 화주·판매자 | `ShipperApp` `/shipper/warehouse/work/inbound`, `/shipper/warehouse/work/outbound`, `/shipper/warehouse/work/packing` | 창고 작업 진입 |

```mermaid
flowchart LR
    A["공동주문 수입 또는 판매채널 주문"] --> B["입고/출고 요청"]
    B --> C["WarehouseManagerApp 작업 보드"]
    C --> D["입고 검수"]
    D --> E["재고 로트"]
    E --> F["피킹"]
    F --> G["포장"]
    G --> H["국내 화물 운송 인계"]
```

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 공동수입 입고 로트 상세 | `WarehouseManagerApp` | BL/AWB, 통관 단위, 세대 배송 단위, 냉장/냉동 조건을 창고 입고 단위와 연결해야 한다. |
| 출고 배치 상세 | `WarehouseManagerApp` 또는 `ShipperApp` | 출고 배치 엔진이 어떤 창고와 재고를 선택했는지 사람이 검토할 수 있어야 한다. |
| 보관 조건 적합성 확인 | `WarehouseManagerApp` | 냉장, 냉동, 상온, 유통기한 조건에 맞는 창고만 선택되도록 보여야 한다. |

## 판매채널 출고

판매채널 출고는 공동수입 또는 일반 입고 상품을 스마트스토어, 쿠팡, 아마존 같은 판매채널 주문으로 연결하는 흐름이다. 현재는 국내 채널과 출고 배치의 골격을 중심으로 보고, 해외 수출 채널은 관세사 검토와 수출 통관 판단이 붙는 확장 흐름으로 본다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 판매자 | `ShipperApp` `/shipper/sales/channels` | `api/v1/sales-channels` |
| 판매자 | `ShipperApp` `/shipper/sales/listings` | 상품 출품 후보와 채널 상품 연결 |
| 판매자 | `ShipperApp` `/shipper/sales/orders` | 판매채널 주문과 창고 출고 연결 |
| 창고 관리자 | `WarehouseManagerApp` `/work-board`, `/work/outbound`, `/work/packing` | 출고, 피킹, 포장 |
| 기사 | `DriverApp` 국내 화물 운송 화면 | 출고 후 국내 배송 |

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 채널 주문 상세 | `ShipperApp` | 주문별 재고 배분, 출고 배치, 배송 인계를 추적해야 한다. |
| 상품 상세 이미지 생성/검수 화면 | `ShipperApp` 또는 `HongdalAdmin` | 물류 이력과 후기를 판매 상세 이미지로 활용하려면 검수 접점이 필요하다. |
| 수출 채널 준비 화면 | `ShipperApp` | 아마존 등 해외 채널로 보낼 때 수출 통관, 관세사 검토, 금지 품목 판단을 연결해야 한다. |

## 통관·무역 데이터

통관·무역 데이터는 공동주문 수입과 수출 채널 확장의 판단 근거다. HS 코드, 국가, 월 기준 수출입 평균 단가, BL/AWB, 항구/공항, 관할 세관, 보세구역 코드를 정규화해서 보여주는 방향이 맞다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 화주·판매자 | `ShipperApp` `/shipper/customs/hs-reviews`, `/shipper/international/fcl-lcl` | HS 코드 검토, FCL/LCL 판단 |
| 관세사 | `CustomsBrokerApp` `/` | 관세사 검토와 보정 |
| 플랫폼 운영자 | `HongdalAdmin` `/customs/hs-codes` | HS 코드 운영 |
| 주문자 | `OrdererApp` `/group-purchase` | 공동수입 상태와 예상 단가 확인 |

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| BL/AWB 조회 상세 | `OrdererApp` 또는 공개 조회 화면 | 주문자가 자신의 공동수입 화물이 어디 있는지 문서관리번호로 확인해야 한다. |
| 항구/공항/세관/보세구역 코드 브라우저 | `HongdalAdmin` | 공공 데이터를 정규화해 통관 응답값과 연결해야 한다. |
| 관세사 작업 보드 | `CustomsBrokerApp` | HS 코드 보정, 수입 가능성 검토, 보류 사유를 관세사 업무 단위로 처리해야 한다. |

## 커뮤니티 신뢰

커뮤니티 신뢰는 별도 버전의 부가 기능이 아니라 모든 워크플로우 위에 얹히는 성장 트랙이다. 공개 가능한 활동 신호, 후기, 투표, 관계 기록을 개인정보 보호 범위 안에서 공유해 다음 거래 판단을 돕는다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 커뮤니티 참여자 | 각 앱 홈의 커뮤니티 모드 | `api/v1/community/posts`, `api/v1/community/activity-signals` |
| 주문자 집단 | `OrdererApp` `/group-purchase`, 홈 커뮤니티 | `api/v1/community/votes` |
| 플랫폼 운영자 | `HongdalAdmin` `/common-contents`, `/activity-logs` | 콘텐츠와 활동 로그 관리 |

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 워크플로우 활동 신호 뷰어 | 각 앱 또는 `HongdalAdmin` | 어떤 업무 행동이 커뮤니티에 공개 가능한지 확인해야 한다. |
| 투표와 전자문서 화면 | `OrdererApp` | 집단 결정, 참여자 서명, 제출 가능한 문서 생성을 연결해야 한다. |
| 공개 범위 설정 | 각 앱 | 개인정보와 영업정보를 보호하면서 신뢰 신호만 공개해야 한다. |

## 참여 인력 관리

참여 인력 관리는 주문자 집단, 창고, 단지 내 분배, 경비·관리 보조 같은 일을 실제 사람의 역할과 계약, 보상으로 연결하는 흐름이다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 플랫폼 운영자 | `HongdalAdmin` `/dashboard` | `api/v1/admin/hr-roles`, `api/v1/admin/hr-employment-contracts` |
| 플랫폼 운영자 | `HongdalAdmin` `/dashboard` | `api/v1/admin/hr-participation-benefits`, `api/v1/admin/hr-social-insurance-filings` |
| 주문자 집단 | `OrdererApp` `/group-purchase` | 주문자 집단 운영 주체와 참여 가능 역할 확인 |

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| HR 운영 콘솔 | `HongdalAdmin` | 역할, 계약, 보상, 4대보험 신고 준비 상태를 분리해서 관리해야 한다. |
| 입주민 참여 신청 화면 | `OrdererApp` | 같은 단지 주민이 분류, 배분, 관리 보조 업무에 지원할 접점이 필요하다. |
| 계약/신고 준비 체크리스트 | `HongdalAdmin` | API만 있으면 운영자가 누락 상태를 파악하기 어렵다. |

## 음식 배달

음식 배달은 주문자 앱의 음식 주문과 기사 앱의 픽업/전달 흐름이 국내 운송 구조를 가볍게 호출하는 형태다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 주문자 | `OrdererApp` `/food`, `/food/restaurants` | `api/v1/orderer/restaurant-search-policy` |
| 기사 | `DriverApp` `/driver/recommendations`, `/driver/transports/current` | 기사 추천, 픽업/전달 |
| 플랫폼 운영자 | `HongdalAdmin` `/restaurant-search-policy`, `/food/operations` | 음식점 검색 정책과 운영 상태 |

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 음식점 주문 관리 화면 | 별도 음식점 앱 또는 `HongdalAdmin` | 조리 상태, 픽업 준비, 품절 처리를 음식점이 입력해야 한다. |
| 음식 픽업 전용 상세 | `DriverApp` | 일반 화물 상차/하차와 다른 조리 완료, 픽업, 고객 전달 상태가 필요하다. |

## 홍달마트

홍달마트는 도심 재고를 기반으로 주문, 피킹, 포장, 기사 인계가 빠르게 이어지는 흐름이다.

| 참여자 | 현재 화면 | 주요 API |
| --- | --- | --- |
| 주문자 | `OrdererApp` `/food/mart` | 마트 상품 주문 |
| 창고 관리자 | `WarehouseManagerApp` `/mart`, `/mart/work-board`, `/mart/work/{ProcessCode}` | 마트 입고, 보충, 피킹, 기사 인계 |
| 기사 | `DriverApp` `/driver/recommendations`, `/driver/transports/current` | 마트 배송 추천과 진행 |

보완 후보:

| 화면 후보 | 앱 | 이유 |
| --- | --- | --- |
| 마트 상품/재고 운영 화면 | `HongdalAdmin` 또는 `WarehouseManagerApp` | 판매 가능 재고, 품절, 가격, 보충 정책을 운영해야 한다. |
| 마트 주문 상세 | `OrdererApp` | 피킹, 포장, 기사 인계, 배송 완료 상태를 주문자가 봐야 한다. |
| 반품/취소 처리 화면 | `OrdererApp` 또는 `HongdalAdmin` | 신선식품과 일반상품의 취소 가능 시점을 다르게 관리해야 한다. |

## 화면 보완 우선순위

| 순위 | 보완 항목 | 이유 |
| --- | --- | --- |
| 1 | `HongdalAdmin` 워크플로우 운영 보드 | 여러 워크플로우가 서로 인계되는 상태를 운영자가 먼저 볼 수 있어야 한다. |
| 2 | 공동주문 수입 원장 콘솔 | BL/AWB, 통관, 보세구역, 국내 운송, 3PL 입고가 하나의 원장으로 이어져야 한다. |
| 3 | `DriverApp` 추천 상세의 업무 유형 강화 | 일반 화물과 공동주문 세대 배송, 음식, 마트 배송은 기사 책임 범위가 다르다. |
| 4 | 공동주문 투표/결정/전자문서 화면 | 주문자 집단의 합의와 책임 경계를 기록해야 이후 분쟁을 줄일 수 있다. |
| 5 | 창고 입고 로트와 출고 배치 상세 | 공동수입, 냉장/냉동, 판매채널 출고를 창고 작업자가 이해할 수 있어야 한다. |
| 6 | 통관 코드/보세구역 데이터 브라우저 | BL/AWB 응답값과 플랫폼 내부 원장을 안정적으로 연결해야 한다. |
| 7 | HR 참여 인력 콘솔 | 주문자 집단의 고용, 보상, 4대보험 준비 상태는 API보다 운영 화면이 먼저 필요하다. |

## 화면 설계 원칙

1. 화면은 버전 번호보다 워크플로우 이름을 먼저 보여준다.
2. 추천, 주문, 입고, 통관, 정산 같은 카드에는 현재 워크플로우와 다음 인계 워크플로우를 함께 표시한다.
3. 사용자에게는 "내가 지금 책임지는 단계"와 "다음 단계의 책임자"를 분리해서 보여준다.
4. `GET /api/v1/version-feature-flags`의 `Workflows`, `Screens`, `WorkflowRelations` 값을 메뉴, 비활성 안내, 운영 보드의 기준 데이터로 사용한다.
5. 공동주문, 통관, HR, 커뮤니티처럼 민감한 흐름은 공개 가능한 정보와 비공개 정보를 화면 단위에서 나눈다.

## 용어 정리

| 용어 | 정의 |
| --- | --- |
| 워크플로우 | 하나의 업무 절차를 완성하기 위해 여러 API와 화면이 묶인 단위 |
| 화면 경계 | 한 화면에서 사용자가 직접 책임지고 처리할 수 있는 업무 범위 |
| 인계 지점 | 한 워크플로우의 결과가 다른 워크플로우의 시작 조건이 되는 지점 |
| 원장 | 업무 상태, 증빙, 문서 번호, 책임 주체를 계속 추적하기 위한 기록 |
| 활동 신호 | 거래나 작업에서 발생한 기록 중 커뮤니티에 공개 가능한 요약 정보 |
| 보완 후보 | 현재 화면만으로는 워크플로우를 설명하거나 운영하기 부족해 추가가 필요한 화면 |
