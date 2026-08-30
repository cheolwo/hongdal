# WI 상하위 관계와 신규 후보 등록 결과

> world-interaction-registration-relations.json에서 생성한다. 직접 수정하지 않는다.

- 기존 72개 + 신규 행동 33개 = 전체 WI 105개. 41개 후보 중 특화 프로필 2개·상위 분류 5개·결과 투영 1개는 실행 WI로 중복 등록하지 않는다.
- 등록 당시 신규 33개는 E0였다. 현재 미착수 30 개; 아래 실제 구현 상태는 WI 대장에서 읽는다. 등록 자체는 구현·Save·API·공간·성장 승인이 아니다.
- 상위 분류는 실행하지 않는다. 특화는 의미 관계이며 부모를 재실행하지 않는다. 작업 순서는 world-interaction-flows.json이 소유한다.
- 기존 WI ID와 공개 계약을 바꾸지 않는다. 특화 프로필의 옛 후보 ID는 이 문서와 질문 추적에 보존하며 런타임 별칭으로 자동 해석하지 않는다.
- Town 독립 보충 주문과 Hub/기존 마트 연결은 주체·원장·시작 조건이 달라 이름만으로 통합하지 않았다. 사상자 대응·안전화 같은 복합 활동은 하위 책임 추가 승인이 필요하다.

## 상위 분류와 하위 행동

기존 `wi-family:meditation`은 독립된 횡단 성장 축으로 유지한다. 아래는 행동 책임 분류이며 신규 등록을 명상 보상에 자동 결속하지 않는다.

- 물품 섭취 (`wi-family:consumption`): WI-ACTOR-CONSUME, WI-ORDER-07
- 개인 계획 (`wi-family:planning`): WI-ACTOR-PLAN-SET, WI-NATURE-15
- 전투 지휘 (`wi-family:combat`): WI-COMBAT-DIRECT-CONTROL-SET, WI-COMBAT-TACTICAL-COMMAND
- 공동체 관계 (`wi-family:community`): WI-COMMUNITY-COOPERATION-PROPOSE, WI-COMMUNITY-ENTRANCE-POLICY-SET, WI-COMMUNITY-HIRE, WI-COMMUNITY-MEMBERSHIP-CONFIRM, WI-COMMUNITY-REMOTE-RESPONSE, WI-COMMUNITY-SUPPORT-MISSION-JOIN, WI-COMMUNITY-VISITOR-STAY, WI-GUEST-PERMISSION-SET, WI-WORLD-02
- 건설 활동 (`wi-family:construction`): WI-CON-01, WI-CON-BLUEPRINT-PLACE, WI-CON-DEMOLISH, WI-CON-MATERIAL-DEPOSIT, WI-CON-WORK-CONTRIBUTE, WI-NATURE-07, WI-NATURE-08, WI-WORLD-04
- 조합 제작 (`wi-family:crafting`): WI-CRAFT-BREW
- 탐사 활동 (`wi-family:expedition`): WI-EXPEDITION-DISPATCH, WI-NATURE-TRACE-INVESTIGATE
- 농업 활동 (`wi-family:farming`): WI-FARM-01, WI-FARM-02, WI-FARM-03, WI-FARM-04, WI-FARM-FIELD-BOUNDARY-CONFIRM, WI-FARM-SOIL-AMEND, WI-FARM-WATER-TRANSFER
- 열원 관리 (`wi-family:heat-source`): WI-HEAT-SOURCE-STATE-CHANGE
- 물품 공급·보관 (`wi-family:logistics`): WI-001, WI-002, WI-HUB-DEMAND-ALLOCATE, WI-HUB-SUPPLY-TASK-ACCEPT, WI-MARKET-02, WI-MARKET-03, WI-MARKET-04, WI-MARKET-05, WI-TOWN-DELIVERY-INSPECT, WI-TOWN-DELIVERY-RECEIVE, WI-TOWN-STOCK-PUTAWAY, WI-TOWN-STOCK-REPLENISH, WI-TOWN-SUPPLY-DISPATCH
- 생존 경제 (`wi-family:economy`): WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM, WI-SURVIVAL-RATION-POLICY-SET
- 자원 획득 (`wi-family:gathering`): WI-ACTOR-01, WI-NATURE-05, WI-NATURE-18, WI-NATURE-HERB-GATHER
- 세계 자원 재생 (`wi-family:regeneration`): WI-WORLD-RESOURCE-REGENERATE
- 사상자 대응 (`wi-family:casualty-response`): 
- 농지 조성 (`wi-family:land-improvement`): WI-FARM-01
- 위협 핵 정리 (`wi-family:threat-core-clear`): WI-HEAT-SOURCE-STATE-CHANGE
- 회랑 안전화 (`wi-family:route-safety`): WI-EXPEDITION-DISPATCH, WI-NATURE-03, WI-WORLD-04
- 비전투 위협 대응 (`wi-family:noncombat-response`): WI-NATURE-02, WI-NATURE-TRACE-INVESTIGATE

## 공통 행동의 특화 관계

- `WI-ACTOR-CONSUME` → `WI-ORDER-07`: 물품 소비 의미의 주민 생활 욕구 특화. 기존 주문·소비·욕구 원장 계약은 유지하며 부모를 추가 실행하지 않음
- `WI-ACTOR-01` → `WI-NATURE-05`: 물품 획득의 벌목 도끼 특화; 기존 계약 유지
- `WI-ACTOR-01` → `WI-NATURE-HERB-GATHER`: 획득 의미 특화. 약초 노드 잔량·권한·비용은 후속 승인에서 검증하며 부모 실행을 호출하지 않음
- `WI-ACTOR-PLAN-SET` → `WI-NATURE-15`: 개인 계획의 다음날 확장 목표 특화. 기존 저장·호출 계약은 수정하지 않음
- `WI-CON-BLUEPRINT-PLACE` → `WI-NATURE-07`: 청사진 배치의 오두막 특화. 등록 관계만 추가하고 기존 실행은 유지

## 후보 41개 판정

| 후보·한국어 이름 | 판정 | 등록 대상 | 책임·중복 판정 이유 | 질문 |
| --- | --- | --- | --- | --- |
| `WI-ACTOR-CONSUME` 물품 섭취 | RegisterAction | `WI-ACTOR-CONSUME` | 섭취 가능한 소유 물품 한 묶음을 소비한다. 주문 이행이나 치료 판정 전체는 소유하지 않는다. | Q-045, Q-046, Q-047, Q-048, Q-049, Q-050, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296 |
| `WI-ACTOR-PLAN-SET` 개인 계획 설정 | RegisterAction | `WI-ACTOR-PLAN-SET` | 플레이어 개인 계획 하나를 설정한다. 계획 완료나 성장 보상을 즉시 확정하지 않는다. | Q-040, Q-041, Q-042, Q-043, Q-044 |
| `WI-COMBAT-CASUALTY-RESPONSE` 사상자 대응 | MetadataFamily | `wi-family:casualty-response` | 후송·응급처치·재편성·모집은 서로 다른 결과이므로 하나의 실행 WI로 등록하지 않는다. | Q-191, Q-192, Q-193, Q-230 |
| `WI-COMBAT-DIRECT-CONTROL-SET` 직접 전투 조종 전환 | RegisterAction | `WI-COMBAT-DIRECT-CONTROL-SET` | 자기 Actor의 자동 조종 보류·재개 상태만 전환한다. 카메라 전환은 표현이고 피해·승패는 별도다. | Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190 |
| `WI-COMBAT-TACTICAL-COMMAND` 분대 전술 명령 확정 | RegisterAction | `WI-COMBAT-TACTICAL-COMMAND` | 대상 분대의 전술 명령 하나를 확정한다. 이동·방어·후퇴는 명령 종류이며 실제 NPC 전투 결과를 대행하지 않는다. | Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190 |
| `WI-COMMUNITY-COOPERATION-PROPOSE` 공동체 협력 제안 | RegisterAction | `WI-COMMUNITY-COOPERATION-PROPOSE` | 협력 제안 하나를 기록한다. 제안만으로 수락·위임·현장 작업을 확정하지 않는다. | Q-236, Q-238 |
| `WI-COMMUNITY-ENTRANCE-POLICY-SET` 공동체 출입 정책 설정 | RegisterAction | `WI-COMMUNITY-ENTRANCE-POLICY-SET` | 입구 생활·경비 운영 정책을 설정한다. 실제 쉼터 건설은 건설 WI가 수행한다. | Q-218, Q-219 |
| `WI-COMMUNITY-HIRE` NPC 고용 확정 | RegisterAction | `WI-COMMUNITY-HIRE` | 고용 합의와 고용 관계를 확정한다. 작업 배정이나 역량 부여를 대신하지 않는다. | Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118 |
| `WI-COMMUNITY-MEMBERSHIP-CONFIRM` 공동체 정식 편입 확정 | RegisterAction | `WI-COMMUNITY-MEMBERSHIP-CONFIRM` | 당사자 동의와 조건을 확인해 정식 소속 관계를 확정한다. 임시 방문자 체류와 별도 관계다. | Q-200, Q-205, Q-206 |
| `WI-COMMUNITY-REMOTE-RESPONSE` 원격 응대 지시 확정 | RegisterAction | `WI-COMMUNITY-REMOTE-RESPONSE` | 응대 담당자에게 지시 하나를 확정한다. 지시의 현장 수행 결과는 담당 WI가 기록한다. | Q-210, Q-211, Q-212, Q-213, Q-214 |
| `WI-COMMUNITY-SUPPORT-MISSION-JOIN` 공동 지원 임무 참여 | RegisterAction | `WI-COMMUNITY-SUPPORT-MISSION-JOIN` | 승인된 지원 임무의 참여를 확정한다. 협력 제안이나 전체 유지관리 작업 완료와 다르다. | Q-175, Q-176, Q-177, Q-178, Q-179 |
| `WI-CON-BLUEPRINT-PLACE` 건설 청사진 배치 | RegisterAction | `WI-CON-BLUEPRINT-PLACE` | 검증된 건설 청사진 배치를 확정한다. 실제 시공·자재 소비·건물 완공은 별도 행동이다. | Q-054, Q-055, Q-056 |
| `WI-CON-DEMOLISH` 건설물 해체 | RegisterAction | `WI-CON-DEMOLISH` | 대상 건설물 하나를 해체한다. 재료 회수는 승인 규칙의 원자적 부수 결과로만 추가할 수 있다. | Q-054, Q-055, Q-056 |
| `WI-CON-MATERIAL-DEPOSIT` 건설 재료 투입 | RegisterAction | `WI-CON-MATERIAL-DEPOSIT` | 소유 재료를 대상 건설 원장에 투입한다. 시공 기여나 완공을 대신하지 않는다. | Q-054, Q-055, Q-056 |
| `WI-CON-WORK-CONTRIBUTE` 건설 시공 기여 | RegisterAction | `WI-CON-WORK-CONTRIBUTE` | 대상 건설 작업에 유효 시공 기여를 기록한다. 작업 시간 경과만으로 다른 책임을 실행하지 않는다. | Q-054, Q-055, Q-056, Q-058, Q-059, Q-060 |
| `WI-CRAFT-BREW` 배합물 달이기 | RegisterAction | `WI-CRAFT-BREW` | 확정한 배합 배치 하나를 열원에서 달여 완성한다. 지식 습득·재료 채집·완성품 섭취는 제외한다. | Q-061, Q-062, Q-063, Q-064, Q-068, Q-069, Q-070, Q-071, Q-142, Q-150, Q-157, Q-279, Q-280, Q-281, Q-282, Q-283, Q-284, Q-285, Q-286, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296 |
| `WI-DEFENSE-SEGMENT-REPAIR` 방어 구간 수리 | ReuseProfile | `WI-WORLD-04` | 손상 SegmentStableId를 가진 시설의 내구도 수리이므로 기존 시설 수리에 대상 프로필을 연결한다. | Q-149 |
| `WI-EXPEDITION-DISPATCH` 탐사 임무 파견 | RegisterAction | `WI-EXPEDITION-DISPATCH` | 승인한 탐사 임무의 인력·목적지·보급 파견을 확정한다. 보고·교전 결과는 별도 책임이다. | Q-119, Q-121 |
| `WI-FARM-FIELD-BOUNDARY-CONFIRM` 밭 경계 확정 | RegisterAction | `WI-FARM-FIELD-BOUNDARY-CONFIRM` | 검증된 밭 경계와 관리 통로 범위를 확정한다. 경작·파종·개간을 동시에 완료하지 않는다. | Q-087, Q-088, Q-089, Q-091, Q-103, Q-104, Q-105, Q-109, Q-110 |
| `WI-FARM-LAND-IMPROVE` 농지 조성 | MetadataFamily | `wi-family:land-improvement` | 장애물 제거·평탄화·개간은 다른 권위 상태를 바꾸므로 비실행 상위 분류로 등록한다. | Q-082, Q-092, Q-095, Q-096, Q-097, Q-106, Q-107, Q-136 |
| `WI-FARM-SOIL-AMEND` 토양 개량 | RegisterAction | `WI-FARM-SOIL-AMEND` | 승인 자재로 토양 상태를 개량한다. 생육 중인 작물 관리나 경계 확정과 다른 대상 원장이다. | Q-083, Q-084, Q-085, Q-086, Q-098 |
| `WI-FARM-WATER-TRANSFER` 농업 용수 이송 | RegisterAction | `WI-FARM-WATER-TRANSFER` | 수원에서 지정 수용처로 용수를 이송한다. 강수 생성·관수 설비 건설은 포함하지 않는다. | Q-099, Q-100, Q-101, Q-102, Q-143, Q-144, Q-146, Q-147, Q-155 |
| `WI-GUEST-PERMISSION-SET` 손님 활동 권한 설정 | RegisterAction | `WI-GUEST-PERMISSION-SET` | 특정 손님의 허용 활동 범위를 설정한다. 고용 관계나 NPC 업무 역량을 생성하지 않는다. | Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118 |
| `WI-HEAT-SOURCE-STATE-CHANGE` 열원 상태 변경 | RegisterAction | `WI-HEAT-SOURCE-STATE-CHANGE` | 같은 열원의 점화·연료 보충·소화를 작업 코드로 구분한다. 범위는 열원 상태와 승인된 자원 비용이다. | Q-026, Q-027, Q-028, Q-032, Q-033, Q-034, Q-053, Q-153, Q-156, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174 |
| `WI-HUB-DEMAND-ALLOCATE` Hub 수요 재고 할당 | RegisterAction | `WI-HUB-DEMAND-ALLOCATE` | Hub 내부 재고를 선택한 수요에 할당한다. City 서비스 배정이나 외부 수요 충족을 직접 확정하지 않는다. | Q-245 |
| `WI-HUB-DEMAND-REMAINDER-RETURN` Hub 미충족 수요 환류 | ResultProjection | `module:hub-demand-remainder` | 부분 할당 뒤 부족분 유지·지연 통지·대안 조회는 결과 투영이며 새로운 사용자 확정 행동이 아니다. | Q-246 |
| `WI-HUB-SUPPLY-TASK-ACCEPT` Hub 조달 과제 수락 | RegisterAction | `WI-HUB-SUPPLY-TASK-ACCEPT` | 기한·위험·대가를 확인한 조달 과제 하나를 수락한다. 실패 해결 정책 Q250은 결정하지 않는다. | Q-247 |
| `WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM` 목표 비축 미달 판매 확정 | RegisterAction | `WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM` | 비축 미달 위험을 재확인하고 판매 거래를 확정한다. 비축 경고 자체는 별도 WI로 만들지 않는다. | Q-260 |
| `WI-NATURE-HERB-GATHER` 약초 채집 | RegisterAction | `WI-NATURE-HERB-GATHER` | 접근 가능한 약초 노드에서 허용량을 채집한다. 일반 물품 획득 의미를 특화하지만 노드 잔량 검증을 별도로 요구한다. | Q-269, Q-270, Q-275, Q-276, Q-277, Q-278, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296 |
| `WI-NATURE-THREAT-CORE-CLEAR` 위협 핵 정리 | MetadataFamily | `wi-family:threat-core-clear` | 뿌리 절단·소각·잔불 정리의 절차를 단일 WI로 만들지 않고 작업 조합을 분류한다. | Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174 |
| `WI-NATURE-TRACE-INVESTIGATE` 자연 흔적 조사 | RegisterAction | `WI-NATURE-TRACE-INVESTIGATE` | 현장 흔적 관찰 결과를 조사 원장에 기록한다. 승인 Recipe 지식 추가와 다른 관찰 기록이다. | Q-164, Q-165, Q-166 |
| `WI-ROUTE-SAFETY-IMPROVE` 회랑 안전화 | MetadataFamily | `wi-family:route-safety` | 괴물 제거·도로 정비·조명·순찰은 개별 원인과 유지 책임이 달라 비실행 상위 분류로 등록한다. | Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-159, Q-160 |
| `WI-SURVIVAL-RATION-POLICY-SET` 생존 배급 정책 설정 | RegisterAction | `WI-SURVIVAL-RATION-POLICY-SET` | 소비·배급 정책을 확정한다. 재고 이동이나 실제 소비는 각 실행 WI가 수행한다. | Q-255 |
| `WI-THREAT-NONCOMBAT-RESOLVE` 비전투 위협 대응 | MetadataFamily | `wi-family:noncombat-response` | 관찰·회피·협상·시설·서식지 분리는 서로 다른 행동이다. 보상 Q195는 보류하고 상위 분류만 등록한다. | Q-194, Q-195 |
| `WI-TOWN-DELIVERY-INSPECT` Town 납품 검수 | RegisterAction | `WI-TOWN-DELIVERY-INSPECT` | Town 독립 보충 주문의 도착 물품 수량·품질을 검수한다. 기존 Hub 출고 연계 마트 검수와 자동 동일시하지 않는다. | Q-151 |
| `WI-TOWN-DELIVERY-RECEIVE` Town 납품 인수 | RegisterAction | `WI-TOWN-DELIVERY-RECEIVE` | Town 보충 주문에 대한 도착 화물 인수를 확정한다. 주민 주문 수령이나 Hub 화물 인계와 다른 원장이다. | Q-151 |
| `WI-TOWN-STOCK-PUTAWAY` Town 후방 재고 적재 | RegisterAction | `WI-TOWN-STOCK-PUTAWAY` | Town에서 검수된 재고를 승인 후방 슬롯에 적재한다. 검수나 진열을 대신하지 않는다. | Q-151 |
| `WI-TOWN-STOCK-REPLENISH` Town 재고 보충 주문 | RegisterAction | `WI-TOWN-STOCK-REPLENISH` | 운영자가 부족한 상점 재고의 보충 주문을 확정한다. 주민 소비 주문·기존 진열 이동과 다르다. | Q-137, Q-138, Q-145, Q-148 |
| `WI-TOWN-SUPPLY-DISPATCH` Town 공급 운송 출발 확정 | RegisterAction | `WI-TOWN-SUPPLY-DISPATCH` | Town 보충 주문의 운송 출발을 확정한다. 회랑 진행·도착·인수는 이후 책임이다. | Q-158 |
| `WI-WORLD-PATTERN-PLACEMENT-CONFIRM` 공간 패턴 청사진 확정 | ReuseProfile | `WI-CON-BLUEPRINT-PLACE` | H2/H3 패턴의 동결 배치 계획도 청사진 확정의 입력 프로필이다. 두 Confirm을 중첩하지 않는다. | Q-329, Q-330, Q-331, Q-332 |
| `WI-WORLD-RESOURCE-REGENERATE` 세계 자원 재생 | RegisterAction | `WI-WORLD-RESOURCE-REGENERATE` | 권위 WorldTick에서 판본화된 자원 재생 조건을 만족한 노드의 재생만 확정한다. 수치·재생 주기는 미승인이다. | Q-036, Q-037 |

## 원문 근거

### 신규 33개 실제 구현 상태

| WI | 논리 구현 | 통합 E | 구현 파일 수 |
| --- | --- | --- | --- |
| `WI-ACTOR-CONSUME` 물품 섭취 | E0 / NotStarted | E0 | 0 |
| `WI-ACTOR-PLAN-SET` 개인 계획 설정 | E3 / Done | E1 | 4 |
| `WI-COMBAT-DIRECT-CONTROL-SET` 직접 전투 조종 전환 | E0 / NotStarted | E0 | 0 |
| `WI-COMBAT-TACTICAL-COMMAND` 분대 전술 명령 확정 | E0 / NotStarted | E0 | 0 |
| `WI-COMMUNITY-COOPERATION-PROPOSE` 공동체 협력 제안 | E0 / NotStarted | E0 | 0 |
| `WI-COMMUNITY-ENTRANCE-POLICY-SET` 공동체 출입 정책 설정 | E0 / NotStarted | E0 | 0 |
| `WI-COMMUNITY-HIRE` NPC 고용 확정 | E0 / NotStarted | E0 | 0 |
| `WI-COMMUNITY-MEMBERSHIP-CONFIRM` 공동체 정식 편입 확정 | E0 / NotStarted | E0 | 0 |
| `WI-COMMUNITY-REMOTE-RESPONSE` 원격 응대 지시 확정 | E0 / NotStarted | E0 | 0 |
| `WI-COMMUNITY-SUPPORT-MISSION-JOIN` 공동 지원 임무 참여 | E0 / NotStarted | E0 | 0 |
| `WI-CON-BLUEPRINT-PLACE` 건설 청사진 배치 | E0 / NotStarted | E0 | 0 |
| `WI-CON-DEMOLISH` 건설물 해체 | E0 / NotStarted | E0 | 0 |
| `WI-CON-MATERIAL-DEPOSIT` 건설 재료 투입 | E0 / NotStarted | E0 | 0 |
| `WI-CON-WORK-CONTRIBUTE` 건설 시공 기여 | E0 / NotStarted | E0 | 0 |
| `WI-CRAFT-BREW` 배합물 달이기 | E0 / NotStarted | E0 | 0 |
| `WI-EXPEDITION-DISPATCH` 탐사 임무 파견 | E0 / NotStarted | E0 | 0 |
| `WI-FARM-FIELD-BOUNDARY-CONFIRM` 밭 경계 확정 | E0 / NotStarted | E0 | 0 |
| `WI-FARM-SOIL-AMEND` 토양 개량 | E0 / NotStarted | E0 | 0 |
| `WI-FARM-WATER-TRANSFER` 농업 용수 이송 | E0 / NotStarted | E0 | 0 |
| `WI-GUEST-PERMISSION-SET` 손님 활동 권한 설정 | E0 / NotStarted | E0 | 0 |
| `WI-HEAT-SOURCE-STATE-CHANGE` 열원 상태 변경 | E3 / Done | E1 | 4 |
| `WI-HUB-DEMAND-ALLOCATE` Hub 수요 재고 할당 | E0 / NotStarted | E0 | 0 |
| `WI-HUB-SUPPLY-TASK-ACCEPT` Hub 조달 과제 수락 | E0 / NotStarted | E0 | 0 |
| `WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM` 목표 비축 미달 판매 확정 | E0 / NotStarted | E0 | 0 |
| `WI-NATURE-HERB-GATHER` 약초 채집 | E0 / NotStarted | E0 | 0 |
| `WI-NATURE-TRACE-INVESTIGATE` 자연 흔적 조사 | E0 / NotStarted | E0 | 0 |
| `WI-SURVIVAL-RATION-POLICY-SET` 생존 배급 정책 설정 | E0 / NotStarted | E0 | 0 |
| `WI-TOWN-DELIVERY-INSPECT` Town 납품 검수 | E0 / NotStarted | E0 | 0 |
| `WI-TOWN-DELIVERY-RECEIVE` Town 납품 인수 | E0 / NotStarted | E0 | 0 |
| `WI-TOWN-STOCK-PUTAWAY` Town 후방 재고 적재 | E0 / NotStarted | E0 | 0 |
| `WI-TOWN-STOCK-REPLENISH` Town 재고 보충 주문 | E0 / NotStarted | E0 | 0 |
| `WI-TOWN-SUPPLY-DISPATCH` Town 공급 운송 출발 확정 | E0 / NotStarted | E0 | 0 |
| `WI-WORLD-RESOURCE-REGENERATE` 세계 자원 재생 | E3 / Done | E1 | 4 |

- `docs/Architecture/PlayableLoops/Nature기초약초회복.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/생존경제/survival-economy.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/Farm병영방위/farm-barracks-defense.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/Hub수요분배/hub-demand-allocation.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/nature-night-day2.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md`
- `docs/Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md`
- `eng/execution-ledgers/world-interactions.json`
