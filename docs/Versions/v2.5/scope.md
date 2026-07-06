# Hongdal 2.5 Scope

| 기능 | 포함 여부 | 관련 앱/모듈 | 메모 |
| --- | --- | --- | --- |
| 공동주택 주소/단지 후보 조회 | 포함 | `OrdererApp`, 주소/공동주택 식별 서비스 | 외부 API 후보 활용 |
| 사용자 단지 소속 확인 | 포함 | 가입/프로필/커뮤니티 | 상세주소 직접 공개 금지 |
| 공동 주문 모집 | 포함 | `OrdererApp`, 커뮤니티 모드 | 주민 구매 의사 수집 |
| 화주 대량 구매 공개 | 포함 | `ShipperApp` | 수입/국내 대량 구매 모두 고려 |
| FCL 가능 조건 계산 | 포함 | `ShipperApp`, 화물 계획 서비스 | 목표 수량/부피/중량 기준 |
| 단지 대표 입고 | 포함 | `WarehouseManagerApp`, `CargoYongdalDispatchEngine` | 단지 입고지로 운송 연결 |
| 단지 내 분류/배분 | 포함 | 단지 분류 작업 서비스 | 동/수령 지점 단위 |
| 홍달마트 즉시배송 | 보류 | `Deliver`, `FoodDeliveryDispatchEngine` | 3.0 |
| 관리사무소 공식 승인 자동화 | 보류 | Admin/외부 협약 | 운영 정책 확정 후 |
