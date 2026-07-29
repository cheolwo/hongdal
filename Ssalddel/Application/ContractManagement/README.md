# ContractManagement

서버의 계약 영역 Application 경계다.

계약 영역은 실제 물류 상태를 직접 바꾸기보다, 비즈니스 실행 전에 확인해야 하는 조건을 확정한다.

## 포함 대상

- 입고 계약 유형과 계약 스냅샷
- 위탁판매, 보관대행, 마켓 풀필먼트 계약
- 운임, 배송료, 배달 수수료, 플랫폼 수수료 정책
- 통관 필요 여부, 정산 주기, 계약 유효기간
- 공급자와 플랫폼의 공통 공급조건 계약
- 음식점·살들마트의 계약 이용 동의와 자기 명의 개별 발주 중개

## 현재 연결 코드

- `Ssalddel.Contracts/Common/Inbound/InboundContractDtos.cs`
- `Ssalddel.Domain/창고/입고요청.cs`
- `Ssalddel.Domain/창고/입고상품.cs`
- `Ssalddel/Services/LogisticsProcessing/Warehouse/WarehouseOperationService.cs`
- `Ssalddel.Contracts/Common/ContractManagement/플랫폼공급중개Dtos.cs`
- `Ssalddel.Domain/공급중개/플랫폼공급중개원장.cs`
- `Ssalddel/Application/ContractManagement/플랫폼공급계약관리UseCase.cs`
- `Ssalddel/Application/ContractManagement/조직개별공급발주UseCase.cs`

공급중개 경계의 기준 문서는
`docs/Architecture/PlatformSupplyBrokerage.md`다. 플랫폼은 공급조건 계약과
발주 전달을 중개하지만 개별 거래의 판매자·재판매자 또는 매수인이 아니다.

## 이동 기준

계약 조건을 계산하거나 검증하는 Command, Query, Policy는 이 폴더 아래로 둔다.
입고, 피킹, 배차처럼 실제 상태를 변경하는 처리는 `LogisticsProcessing`에 둔다.
