# WarehouseManagerApp-P04-4 - 운송의뢰 로컬 초안

- 경로: `/warehouse/general/transport-request-draft`
- 통합 웹 경로: `/warehouse/general/transport-request-draft`
- 통합 웹 별칭: `/work/outbound/transport-request-draft`
- 상태: 확장 · 실제 캡처
- 실행 경계: `Beta / Simulation`

출고예정 검토를 통과한 정확한 `outboundPlanId` 한 건을 다시 조회해 하차지 요약, 희망 상차·도착 일시, 차량 유형과 취급 메모를 구성하는 페이지다. 원장 조회, 입력 상태와 교차검증, 페이지 조정, host 인증·capability 책임을 각각 분리한다.

출고 원장의 포장·수량·출발 창고 근거는 이전 검토 결과를 재사용하고, 이 페이지는 운송에 새로 필요한 입력만 맡는다. 도착 일시는 상차 일시보다 뒤여야 하고 지원 차량 유형과 상품·수량 확인을 검증한다. 출발지 상세 주소는 서버 창고 설정을 사용하되 화면에는 노출하지 않고, 입력 메모에는 연락처·계좌 같은 개인정보를 넣지 않도록 안내한다.

`입력값 로컬 검토`는 메모리에만 `OUT-{outboundPlanId}-LOCAL` 결과를 만들며 서버 저장, 재고 예약·차감, 운송의뢰 ID 생성, 배차, 계약, 결제와 정산을 실행하지 않는다.

![운송의뢰 로컬 초안 화면](../../../../assets/changes/2026-07-20-warehouse-transport-request-draft/desktop.png)
