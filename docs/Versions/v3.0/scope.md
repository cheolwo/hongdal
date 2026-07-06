# Hongdal 3.0 Scope

| 기능 | 포함 여부 | 관련 앱/모듈 | 메모 |
| --- | --- | --- | --- |
| 홍달마트 주문 | 포함 | `OrdererApp` | 도심 마트 상품 주문 |
| 도심 재고 관리 | 포함 | `WarehouseManagerApp` | 마트형 재고 |
| 피킹/포장 | 포함 | `WarehouseManagerApp` | 배차 전 완료 조건 |
| 음식 배달 배차 | 포함 | `FoodDeliveryDispatchEngine`, `Deliver` | 포장 완료 후 진행 |
| 묶음 배달 | 포함 | 음식 배달 정책 | 거리/시간 기준 |
| 공동주택 공동 주문 | 참조 | `OrdererApp`, 공동 주문 서비스 | 2.5 흐름과 운영 경계 유지 |
| 국내 화물/용달 핵심 변경 | 보류 | `DriverApp`, `ShipperApp` | 1.0 안정성 유지 |
