# Hongdal 1.0 Scope

| 기능 | 포함 여부 | 관련 앱/모듈 | 메모 |
| --- | --- | --- | --- |
| 국내 화물/용달 운송 의뢰 | 포함 | `ShipperApp`, `Hongdal` | 1.0 핵심 |
| 화물/용달 기사 배차 | 포함 | `DriverApp`, `CargoYongdalDispatchEngine` | 1.0 핵심 |
| 상차/하차 사진 증빙 | 포함 | `DriverApp`, 파일 업로드 서비스 | 필수 업무 흐름 |
| 수령자 인수 확인 | 포함 | 운송 상세 DTO | 노출 범위 관리 필요 |
| 운송 상태 전이 | 포함 | `기사운송상태전이Service` | 배차대기/상차지도착/상차완료/하차지도착/인수완료 |
| 결제/정산 기본 폐쇄 루프 | 포함 | 결제/정산 서비스, 인수증 문서, `TransportPaymentSettlementPolicy` | 홍달은 플랫폼 이용료를 무료로 둘 수 있지만 운송료는 등록 PG/에스크로/정산사를 통해 보증하고, 하차 완료 후 정산 후보가 남아야 함 |
| 수령자 정보 단계별 마스킹 | 포함 | 운송 상세 DTO, DriverApp | 배차 전/수락 후/하차 직전/완료 후 구분 |
| 커뮤니티 보조 모드 | 제한 포함 | `Hongdal.Ui.Common` | 업무 흐름 보조 |
| 창고 입고/출고 고도화 | 보류 | `WarehouseManagerApp` | 1.5 |
| 통관/HS 데이터 | 보류 | `Hongdal.WebApp`, `HongdalAdmin` | 2.0 |
| 음식점 일반 음식 배달 | 보류 | `FDriverApp`, `FoodDeliveryDispatchEngine` | 3.0 |
| 홍달마트 즉시배송 | 보류 | `FDriverApp`, `FoodDeliveryDispatchEngine`, `WarehouseManagerApp` | 3.5 |
