# 적재 완료 재고를 잇는 창고 포장 작업

## 변경

- WarehouseManagerApp `/work/outbound/packing`과 통합 웹 `/warehouse/work/outbound/packing`에 같은 공용 포장 작업 component를 연결했다.
- 목록, 정확한 `inboundItemId` 상세, 포장 완료 Command, 완료 후 같은 ID 재조회 책임을 service와 ViewModel로 분리했다.
- 창고 소유자 또는 배정 사용자 범위의 `적재완료` 재고만 조회·처리한다.
- 주문 참조, 적재 이력, 보관 위치와 수량을 확인하고 두 현장 확인을 마친 경우에만 전체 가용수량을 포장 완료한다.
- 동일 수량·유형 재시도는 멱등 처리하고, 부분 포장과 완료 뒤 수량·유형 변경은 별도 업무로 분리한다.
- `Beta / Simulation` 경계에서 재고 차감, 출고 확정, 운송, 계약, 결제, 정산을 실행하지 않는다.

## 실제 확인

- `shipper1` 로그인
- 입고상품 `#1` 검수 완료
- `PA-01-01` 적재 완료
- `냉장포장` 29개 포장 완료
- 완료 후 URL과 상세가 같은 `inboundItemId=1` 유지
- 완료 필터 목록과 상세의 서버 상태 일치
- 브라우저 콘솔 오류 없음
- 검증 뒤 개발 시드 재실행으로 초기 상태 원복, 테스트 서버 종료

## 화면

![창고 포장 완료 화면](../assets/changes/2026-07-20-warehouse-packing/desktop.png)
