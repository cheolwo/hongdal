# Hongdal 1.5 Scope

| 기능 | 포함 여부 | 관련 모듈 | 메모 |
| --- | --- | --- | --- |
| 입고 관리 | 포함 | `WarehouseManagerApp` | 계약 기반, 현장 임시, 주문 자동 입고 예정 |
| 적재 관리 | 포함 | `WarehouseManagerApp`, 재고 이력 | 입고 묶음 바코드와 적재함 위치 바코드 기반 재고 이동 |
| 출고 배치 엔진 | 포함 | `OutboundBatchEngine`, `SalesChannelOrderSyncService`, `WarehouseManagerApp` | 판매채널 주문과 출고 요청을 입고상품 재고 기준 단일/복수 창고 출고 계획으로 변환 |
| 판매채널 주문 출고 연결 | 포함 | `SalesChannelOrderSyncService`, `판매상품`, `채널출품`, `출고예정` | 판매채널 주문을 판매상품/입고상품과 매핑한 뒤 출고 배치 엔진 결과로 출고예정 생성 |
| 출고 관리 | 포함 | `WarehouseManagerApp`, `OrdererApp` | 주문 출고 알림, 출고 예약, 피킹, 포장 |
| 창고 출고 연계 운송 | 포함 | `CargoYongdalDispatchEngine` | 1.0 화물/용달 운송 흐름 재사용 |
| 작업자/작업대 검증 | 포함 | 창고 앱 공통 진입 | 휴대폰 뒤 8자리 + 작업대 바코드 + 역할 확인 |
| 통관/HS 데이터 | 보류 | `CustomsBrokerApp` | 2.0 |
| 음식점 일반 음식 배달 | 보류 | `Deliver`, `FoodDeliveryDispatchEngine` | 3.0 |
| 홍달마트 도심 즉시배송 | 보류 | `Deliver`, `WarehouseManagerApp` | 3.5 |
