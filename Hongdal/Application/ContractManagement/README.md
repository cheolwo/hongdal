# ContractManagement

서버의 계약 영역 Application 경계다.

계약 영역은 실제 물류 상태를 직접 바꾸기보다, 비즈니스 실행 전에 확인해야 하는 조건을 확정한다.

## 포함 대상

- 입고 계약 유형과 계약 스냅샷
- 위탁판매, 보관대행, 마켓 풀필먼트 계약
- 운임, 배송료, 배달 수수료, 플랫폼 수수료 정책
- 통관 필요 여부, 정산 주기, 계약 유효기간

## 현재 연결 코드

- `Hongdal.Contracts/Common/Inbound/InboundContractDtos.cs`
- `Hongdal.Domain/창고/입고요청.cs`
- `Hongdal.Domain/창고/입고상품.cs`
- `Hongdal/Services/LogisticsProcessing/Warehouse/WarehouseOperationService.cs`

## 이동 기준

계약 조건을 계산하거나 검증하는 Command, Query, Policy는 이 폴더 아래로 둔다.
입고, 피킹, 배차처럼 실제 상태를 변경하는 처리는 `LogisticsProcessing`에 둔다.
