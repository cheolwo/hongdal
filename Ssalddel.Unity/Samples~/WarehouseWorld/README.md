# Warehouse World primitive

권한 필터가 적용된 재고·적재·피킹 snapshot을 팔레트, 작업 표식, DockWorker와 Picker NPC로 투영한다.

- Operational 모드는 `WarehouseManager` 인증 token을 메모리 provider로만 전달한다.
- 작업자 이름, 주문 참조, 연락처, 주소와 계약·정산 정보는 Unity 계약에 포함하지 않는다.
- semantic waypoint를 Scene Transform으로 변환하며 NPC 도착은 서버 작업을 완료하지 않는다.
- 갱신 실패 시 마지막 성공 snapshot과 기존 object를 유지한다.
- `Ssalddel/Samples/Create Warehouse World Primitive`에서 primitive Scene을 생성한다.
