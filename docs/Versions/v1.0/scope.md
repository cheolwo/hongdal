# Hongdal 1.0 Scope

| 기능 | 포함 여부 | 관련 앱/모듈 | 메모 |
| --- | --- | --- | --- |
| 국내 화물/용달 운송 의뢰 | 포함 | `ShipperApp`, `Hongdal` | 1.0 핵심 |
| 화물/용달 기사 배차 | 포함 | `DriverApp`, `CargoYongdalDispatchEngine` | 1.0 핵심 |
| 상차/하차 사진 증빙 | 포함 | `DriverApp`, 파일 업로드 서비스 | 필수 업무 흐름 |
| 수령자 인수 확인 | 포함 | 운송 상세 DTO | 노출 범위 관리 필요 |
| 커뮤니티 보조 모드 | 제한 포함 | `Hongdal.Ui.Common` | 업무 흐름 보조 |
| 창고 입고/출고 고도화 | 보류 | `WarehouseManagerApp` | 1.5 |
| 통관/HS 데이터 | 보류 | `CustomsBrokerApp` | 2.0 |
| 홍달마트 즉시배송 | 보류 | `Deliver`, `FoodDeliveryDispatchEngine` | 3.0 |
