# WarehouseManagerApp-P04-3 - 출고예정 운송 전 검토

- 경로: `/warehouse/general/outbound-plan-review`
- 통합 웹 경로: `/warehouse/general/outbound-plan-review`
- 통합 웹 별칭: `/work/outbound/plans`
- 상태: 확장 · 실제 캡처
- 실행 경계: `Beta / ReadOnly`

준비된 출고예정 원장의 포장·수량·출발 창고 근거와 운송의뢰 생성 전에 필요한 입력을 한 화면에서 확인하는 읽기 전용 페이지다. 목록과 검색·상태 조건, 사용자가 명시한 정확한 `outboundPlanId` 상세, 페이지 조정, host 인증·capability 책임을 각각 분리한다.

포장 완료 여부는 후속 원장 투영이 재고의 현재 상태를 정규화하더라도 사라지지 않는 포장 이력을 기준으로 판정한다. 출고 상태, 포장 이력, 출고·가용 수량 일치, 활성 출발 창고와 주소 등록은 준비 조건으로 확인하고 하차지·희망 일정·운송의뢰는 별도 작성 단계의 입력 필요 항목으로 표시한다.

이 페이지에는 생성·저장 Command가 없고 재고 예약·차감, 운송의뢰, 배차, 계약, 결제와 정산을 변경하지 않는다. `초안 입력 가능`은 별도 운송의뢰 작성 페이지로 이동할 수 있다는 서버 계산 결과일 뿐 실제 외부 효과를 실행하지 않는다.

![출고예정 운송 전 검토 화면](../../../../assets/changes/2026-07-20-warehouse-outbound-plan-review/desktop.png)
