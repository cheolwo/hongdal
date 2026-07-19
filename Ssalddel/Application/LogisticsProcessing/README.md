# LogisticsProcessing

서버의 물류 처리 영역 Application 경계다.

물류 처리 영역은 계약 조건과 인사 권한을 확인한 뒤 실제 업무 상태를 바꾸는 흐름을 담당한다.

## 포함 대상

- 입고, 입고완료, 재고 생성
- 마켓 주문 예약, 피킹, 포장
- 배차대기, 배차 추천, 배차 확정
- 기사 운행 상태 변경
- 음식 배달 큐 진입과 배달 진행
- 통관 진행 상태 변경

## 현재 연결 코드

- `Ssalddel/Application/Warehouse`
- `Ssalddel/Application/Driver`
- `Ssalddel/Application/Admin/Inbound`
- `Ssalddel/Services/Dispatch`
- `Ssalddel/Services/LogisticsProcessing`

## 이동 기준

업무 상태를 직접 바꾸는 Command/Handler/Service는 이 영역으로 둔다.
계약 조건 자체를 정의하는 코드는 `ContractManagement`에 두고, 사람/권한/역할 코드는 `HumanResources`에 둔다.
