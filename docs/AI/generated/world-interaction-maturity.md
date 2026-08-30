# WI E4·E5 성숙도 대장

> 이 문서는 `eng/execution-ledgers/world-interaction-maturity.json`와 105개 WI 정의에서 자동 생성된다. 직접 수정하지 않는다.

- 대장 개정: `world-interaction-maturity.r5`
- 전체 WI: `105`
- 1차 Runtime 결속 WI: `24`
- 공간은 WI가 Required일 때만 E4 문맥과 E5 추가 증거로 사용한다.

| 한국어 기능명 · 고유 식별자 | 대장 순번 | 허용 발생원 | 실제 결속 | 공간 | E4 | E5 |
| --- | --- | --- | --- | --- | --- | --- |
| 행위자 공통 물품·장착 · 물품 획득 · `WI-ACTOR-01` | 1 | PlayerDriven, NpcDriven | PlayerDriven | NotRequired | ContextBound | Manifested |
| 행위자 공통 물품·장착 · 장착 상태 변경 · `WI-ACTOR-02` | 2 | PlayerDriven, NpcDriven | PlayerDriven | NotRequired | ContextBound | Manifested |
| 행위자 공통 물품·장착 · 지식 습득 · `WI-ACTOR-03` | 3 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 행위자 공통 물품·장착 · 물품 섭취 · `WI-ACTOR-CONSUME` | 4 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 행위자 공통 물품·장착 · 개인 계획 설정 · `WI-ACTOR-PLAN-SET` | 5 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 메이저 아르카나 · 현재 세계의 메이저 아르카나 활성화 · `WI-CARD-01` | 1 | PlayerDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 도심 운영 · 도심 서비스 수요 확정 · `WI-CITY-01` | 1 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 도심 운영 · 도심 서비스용 지역 재고 배정 · `WI-CITY-02` | 2 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 도심 운영 · 도심 주민 서비스 처리 · `WI-CITY-03` | 3 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 도심 운영 · 도심 서비스 결과 확인 · `WI-CITY-04` | 4 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 방문자 임시 체류 결정 · `WI-COMMUNITY-VISITOR-STAY` | 1 | PlayerDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 공동체 협력 제안 · `WI-COMMUNITY-COOPERATION-PROPOSE` | 2 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 공동체 출입 정책 설정 · `WI-COMMUNITY-ENTRANCE-POLICY-SET` | 3 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · NPC 고용 확정 · `WI-COMMUNITY-HIRE` | 4 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 공동체 정식 편입 확정 · `WI-COMMUNITY-MEMBERSHIP-CONFIRM` | 5 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 원격 응대 지시 확정 · `WI-COMMUNITY-REMOTE-RESPONSE` | 6 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 공동 지원 임무 참여 · `WI-COMMUNITY-SUPPORT-MISSION-JOIN` | 7 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공동체 방문·관계 · 손님 활동 권한 설정 · `WI-GUEST-PERMISSION-SET` | 8 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 영역 건설 · 영역 건물 건설 확정 · `WI-CON-01` | 1 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | Manifested |
| 영역 건설 · 건설 청사진 배치 · `WI-CON-BLUEPRINT-PLACE` | 2 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 영역 건설 · 건설물 해체 · `WI-CON-DEMOLISH` | 3 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 영역 건설 · 건설 재료 투입 · `WI-CON-MATERIAL-DEPOSIT` | 4 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 영역 건설 · 건설 시공 기여 · `WI-CON-WORK-CONTRIBUTE` | 5 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 농장 생산 · 경작지 밭갈이 · `WI-FARM-01` | 1 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · 경작지 씨앗 파종 · `WI-FARM-02` | 2 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · 농작물 생육 관리 · `WI-FARM-03` | 3 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · 익은 농작물 수확 · `WI-FARM-04` | 4 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 농장 생산 · 수확물 집하장 모으기 · `WI-FARM-05` | 5 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 농장 생산 · 출하 물량 포장 · `WI-FARM-06` | 6 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 농장 생산 · 방위 분대 소집 · `WI-FARM-DEFENSE-MOBILIZE` | 7 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · 경비 초소 분대 배정 · `WI-SQUAD-ASSIGN` | 8 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · 경비 분대 식량·장비 보급 · `WI-SQUAD-SUPPLY` | 9 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · Farm 방어 성공 결과 발현 · `WI-FARM-DEFENSE-RESOLVE` | 10 | WorldDerived | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 농장 생산 · Farm 방위 분대 초소 귀환 인계 · `WI-FARM-DEFENSE-RETURN` | 11 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 농장 생산 · 밭 경계 확정 · `WI-FARM-FIELD-BOUNDARY-CONFIRM` | 12 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 농장 생산 · 토양 개량 · `WI-FARM-SOIL-AMEND` | 13 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 농장 생산 · 농업 용수 이송 · `WI-FARM-WATER-TRANSFER` | 14 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · 입고 화물 검수 · `WI-001` | 1 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · 검수 완료 화물 창고 적재 · `WI-002` | 2 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · 출고 대상 재고 요청 · `WI-HUB-03` | 3 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · 출고 대상 재고 피킹 · `WI-HUB-04` | 4 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · 피킹 화물 포장 · `WI-HUB-05` | 5 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · 출고 차량 상차 · `WI-HUB-06` | 6 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · Hub 수요 재고 할당 · `WI-HUB-DEMAND-ALLOCATE` | 7 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 물류 거점 창고 · Hub 조달 과제 수락 · `WI-HUB-SUPPLY-TASK-ACCEPT` | 8 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 영역 간 화물 이동 · 출하 차량 상차 확정 · `WI-LOG-01` | 1 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 영역 간 화물 이동 · 농장에서 출발 · `WI-LOG-02` | 2 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 영역 간 화물 이동 · 농장에서 물류 거점으로 화물 이동 · `WI-LOG-03` | 3 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 영역 간 화물 이동 · 물류 거점 도착 화물 하차 · `WI-LOG-04` | 4 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 영역 간 화물 이동 · 물류 거점 도착 화물 인수 · `WI-LOG-05` | 5 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · 물류 거점에서 마트로 운송 · `WI-MARKET-01` | 1 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · 마트 도착 화물 인수 · `WI-MARKET-02` | 2 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · 마트 입고 상품 검수 · `WI-MARKET-03` | 3 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · 검수 상품 후방 창고 적재 · `WI-MARKET-04` | 4 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · 매장 진열대 상품 보충 · `WI-MARKET-05` | 5 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · Town 납품 검수 · `WI-TOWN-DELIVERY-INSPECT` | 6 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · Town 납품 인수 · `WI-TOWN-DELIVERY-RECEIVE` | 7 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · Town 후방 재고 적재 · `WI-TOWN-STOCK-PUTAWAY` | 8 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · Town 재고 보충 주문 · `WI-TOWN-STOCK-REPLENISH` | 9 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 마트 입고·진열 · Town 공급 운송 출발 확정 · `WI-TOWN-SUPPLY-DISPATCH` | 10 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 자연 탐사·생활 거점 · 자연 지역 위험 징후 확인 · `WI-NATURE-01` | 1 | PlayerDriven, NpcDriven, WorldDerived | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 안전 거점으로 긴급 후퇴 · `WI-NATURE-02` | 2 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 훼손된 자연 경로 복원 · `WI-NATURE-03` | 3 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 탐사대 안전 회복 · `WI-NATURE-04` | 4 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 벌목 도끼 획득 · `WI-NATURE-05` | 5 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 나무 벌목 작업 시작 · `WI-NATURE-06` | 6 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 오두막을 지을 터 선정 · `WI-NATURE-07` | 7 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 오두막 건설 작업 시작 · `WI-NATURE-08` | 8 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 오두막 안으로 들어가기 · `WI-NATURE-09` | 9 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 오두막 밖으로 나가기 · `WI-NATURE-10` | 10 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 황혼 위협 대응 방식 확정 · `WI-NATURE-11` | 11 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 진행 중 작업 취소 · `WI-NATURE-12` | 12 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 획득 자원 거점 보관 · `WI-NATURE-13` | 13 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 오두막에서 수면·새벽 맞기 · `WI-NATURE-14` | 14 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 다음 날 거점 확장 계획 선택 · `WI-NATURE-15` | 15 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 현장 보급 꾸러미 제작 · `WI-NATURE-16` | 16 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 현장 보급 제작 업무 위임 · `WI-NATURE-17` | 17 | PlayerDriven, NpcDriven | NpcDriven | Bound | ContextBound | ManifestationPartial |
| 자연 탐사·생활 거점 · 벌목 통나무 줍기 · `WI-NATURE-18` | 18 | PlayerDriven, NpcDriven | PlayerDriven | Bound | ContextBound | Manifested |
| 자연 탐사·생활 거점 · 배합물 달이기 · `WI-CRAFT-BREW` | 19 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 자연 탐사·생활 거점 · 열원 상태 변경 · `WI-HEAT-SOURCE-STATE-CHANGE` | 20 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 자연 탐사·생활 거점 · 약초 채집 · `WI-NATURE-HERB-GATHER` | 21 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 자연 탐사·생활 거점 · 자연 흔적 조사 · `WI-NATURE-TRACE-INVESTIGATE` | 22 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주민 주문 확정 · `WI-ORDER-01` | 1 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주문 상품 재고 예약 · `WI-ORDER-02` | 2 | WorldDerived | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주문 상품 피킹 · `WI-ORDER-03` | 3 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주문 상품 포장 · `WI-ORDER-04` | 4 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주문 상품 수령 준비 · `WI-ORDER-05` | 5 | WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주민 주문 상품 수령 · `WI-ORDER-06` | 6 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 주민 주문·소비 · 주민 상품 소비 · `WI-ORDER-07` | 7 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 거점 성찰 · 승인 자료로 거점 성찰 확정 · `WI-REFLECT-01` | 1 | PlayerDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 업무 검토 · NPC 업무 결과 검토 확정 · `WI-REVIEW-01` | 1 | PlayerDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · NPC에게 반복 업무 배정 · `WI-WORLD-01` | 1 | NpcDriven, WorldDerived | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · NPC에게 업무 역량 위임 · `WI-WORLD-02` | 2 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 진행 중 세계 업무 취소 · `WI-WORLD-03` | 3 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 손상된 시설 수리 · `WI-WORLD-04` | 4 | PlayerDriven, NpcDriven | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 새로운 지역 발견 · `WI-WORLD-05` | 5 | PlayerDriven, NpcDriven, WorldDerived | 미결속 | RequiredMissing | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 일행 역할 카드 장착 · `WI-WORLD-06` | 6 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 세계 활동 상태 변경 · `WI-WORLD-07` | 7 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 하루 운영 턴 마감 · `WI-WORLD-08` | 8 | PlayerDriven, NpcDriven, WorldDerived | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 직접 전투 조종 전환 · `WI-COMBAT-DIRECT-CONTROL-SET` | 9 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 분대 전술 명령 확정 · `WI-COMBAT-TACTICAL-COMMAND` | 10 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 탐사 임무 파견 · `WI-EXPEDITION-DISPATCH` | 11 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 목표 비축 미달 판매 확정 · `WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM` | 12 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 생존 배급 정책 설정 · `WI-SURVIVAL-RATION-POLICY-SET` | 13 | PlayerDriven, NpcDriven | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
| 공통 세계 운영 · 세계 자원 재생 · `WI-WORLD-RESOURCE-REGENERATE` | 14 | WorldDerived | 미결속 | NotApplicable | ContextUnbound | ManifestationMissing |
