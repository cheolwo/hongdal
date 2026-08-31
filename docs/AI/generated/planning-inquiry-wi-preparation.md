# WI 전체 — 지금·여기·나·너·이렇게와 E4 준비 조회

공식 대장 simulation-world-interactions.r43, 105개. 기존 필드를 읽기 단위로 투영했다. 전체 의미 검토/코드·시험·Prefab 준비 완료가 아니며 모든 행의 실행 권한을 false로 유지한다. 높은 기존 E를 낮추거나 원장 단계와 Loop 두 궤적을 합성하지 않는다.

| WI | 이름 | 원장 구현 / 통합(과거 분류) | 직접 Loop 문맥 수 | 코드/시험 참조 수 |
| --- | --- | --- | --- | --- |
| WI-ACTOR-CONSUME | 물품 섭취 | E0 / E0 | 0 | 0 |
| WI-ACTOR-PLAN-SET | 개인 계획 설정 | E3 / E1 | 1 | 4 |
| WI-COMBAT-DIRECT-CONTROL-SET | 직접 전투 조종 전환 | E0 / E0 | 0 | 0 |
| WI-COMBAT-TACTICAL-COMMAND | 분대 전술 명령 확정 | E0 / E0 | 0 | 0 |
| WI-COMMUNITY-COOPERATION-PROPOSE | 공동체 협력 제안 | E0 / E0 | 0 | 0 |
| WI-COMMUNITY-ENTRANCE-POLICY-SET | 공동체 출입 정책 설정 | E0 / E0 | 0 | 0 |
| WI-COMMUNITY-HIRE | NPC 고용 확정 | E0 / E0 | 0 | 0 |
| WI-COMMUNITY-MEMBERSHIP-CONFIRM | 공동체 정식 편입 확정 | E0 / E0 | 0 | 0 |
| WI-COMMUNITY-REMOTE-RESPONSE | 원격 응대 지시 확정 | E0 / E0 | 0 | 0 |
| WI-COMMUNITY-SUPPORT-MISSION-JOIN | 공동 지원 임무 참여 | E0 / E0 | 0 | 0 |
| WI-CON-BLUEPRINT-PLACE | 건설 청사진 배치 | E0 / E0 | 0 | 0 |
| WI-CON-DEMOLISH | 건설물 해체 | E0 / E0 | 0 | 0 |
| WI-CON-MATERIAL-DEPOSIT | 건설 재료 투입 | E0 / E0 | 0 | 0 |
| WI-CON-WORK-CONTRIBUTE | 건설 시공 기여 | E0 / E0 | 0 | 0 |
| WI-CRAFT-BREW | 배합물 달이기 | E0 / E0 | 0 | 0 |
| WI-EXPEDITION-DISPATCH | 탐사 임무 파견 | E0 / E0 | 0 | 0 |
| WI-FARM-FIELD-BOUNDARY-CONFIRM | 밭 경계 확정 | E0 / E0 | 0 | 0 |
| WI-FARM-SOIL-AMEND | 토양 개량 | E0 / E0 | 0 | 0 |
| WI-FARM-WATER-TRANSFER | 농업 용수 이송 | E0 / E0 | 0 | 0 |
| WI-GUEST-PERMISSION-SET | 손님 활동 권한 설정 | E0 / E0 | 0 | 0 |
| WI-HEAT-SOURCE-STATE-CHANGE | 열원 상태 변경 | E3 / E1 | 1 | 4 |
| WI-HUB-DEMAND-ALLOCATE | Hub 수요 재고 할당 | E0 / E0 | 0 | 0 |
| WI-HUB-SUPPLY-TASK-ACCEPT | Hub 조달 과제 수락 | E0 / E0 | 0 | 0 |
| WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM | 목표 비축 미달 판매 확정 | E0 / E0 | 0 | 0 |
| WI-NATURE-HERB-GATHER | 약초 채집 | E0 / E0 | 0 | 0 |
| WI-NATURE-TRACE-INVESTIGATE | 자연 흔적 조사 | E0 / E0 | 0 | 0 |
| WI-SURVIVAL-RATION-POLICY-SET | 생존 배급 정책 설정 | E0 / E0 | 0 | 0 |
| WI-TOWN-DELIVERY-INSPECT | Town 납품 검수 | E0 / E0 | 0 | 0 |
| WI-TOWN-DELIVERY-RECEIVE | Town 납품 인수 | E0 / E0 | 0 | 0 |
| WI-TOWN-STOCK-PUTAWAY | Town 후방 재고 적재 | E0 / E0 | 0 | 0 |
| WI-TOWN-STOCK-REPLENISH | Town 재고 보충 주문 | E0 / E0 | 0 | 0 |
| WI-TOWN-SUPPLY-DISPATCH | Town 공급 운송 출발 확정 | E0 / E0 | 0 | 0 |
| WI-WORLD-RESOURCE-REGENERATE | 세계 자원 재생 | E3 / E1 | 1 | 4 |
| WI-ACTOR-01 | 물품 획득 | E3 / E5 | 1 | 3 |
| WI-ACTOR-02 | 장착 상태 변경 | E3 / E5 | 1 | 4 |
| WI-ACTOR-03 | 지식 습득 | E3 / E4 | 1 | 10 |
| WI-COMMUNITY-VISITOR-STAY | 방문자 임시 체류 결정 | E4 / E4 | 1 | 6 |
| WI-FARM-DEFENSE-MOBILIZE | 방위 분대 소집 | E4 / E4 | 1 | 6 |
| WI-SQUAD-ASSIGN | 경비 초소 분대 배정 | E3 / E3 | 1 | 4 |
| WI-SQUAD-SUPPLY | 경비 분대 식량·장비 보급 | E3 / E3 | 1 | 4 |
| WI-FARM-DEFENSE-RESOLVE | Farm 방어 성공 결과 발현 | E3 / E3 | 1 | 4 |
| WI-FARM-DEFENSE-RETURN | Farm 방위 분대 초소 귀환 인계 | E3 / E3 | 1 | 4 |
| WI-FARM-01 | 경작지 밭갈이 | E3 / E6 | 1 | 1 |
| WI-FARM-02 | 경작지 씨앗 파종 | E3 / E6 | 1 | 0 |
| WI-FARM-03 | 농작물 생육 관리 | E3 / E6 | 1 | 0 |
| WI-FARM-04 | 익은 농작물 수확 | E3 / E6 | 2 | 1 |
| WI-FARM-05 | 수확물 집하장 모으기 | E3 / E6 | 2 | 0 |
| WI-FARM-06 | 출하 물량 포장 | E3 / E6 | 2 | 1 |
| WI-LOG-01 | 출하 차량 상차 확정 | E3 / E6 | 0 | 2 |
| WI-LOG-02 | 농장에서 출발 | E3 / E6 | 0 | 1 |
| WI-LOG-03 | 농장에서 물류 거점으로 화물 이동 | E3 / E4 | 0 | 1 |
| WI-LOG-04 | 물류 거점 도착 화물 하차 | E3 / E4 | 0 | 1 |
| WI-LOG-05 | 물류 거점 도착 화물 인수 | E3 / E4 | 0 | 1 |
| WI-001 | 입고 화물 검수 | E3 / E4 | 1 | 2 |
| WI-002 | 검수 완료 화물 창고 적재 | E3 / E4 | 1 | 2 |
| WI-HUB-03 | 출고 대상 재고 요청 | E3 / E1 | 1 | 2 |
| WI-HUB-04 | 출고 대상 재고 피킹 | E3 / E1 | 1 | 2 |
| WI-HUB-05 | 피킹 화물 포장 | E3 / E1 | 1 | 2 |
| WI-HUB-06 | 출고 차량 상차 | E3 / E1 | 0 | 1 |
| WI-MARKET-01 | 물류 거점에서 마트로 운송 | E3 / E1 | 0 | 1 |
| WI-MARKET-02 | 마트 도착 화물 인수 | E3 / E1 | 0 | 0 |
| WI-MARKET-03 | 마트 입고 상품 검수 | E3 / E1 | 0 | 0 |
| WI-MARKET-04 | 검수 상품 후방 창고 적재 | E3 / E1 | 0 | 0 |
| WI-MARKET-05 | 매장 진열대 상품 보충 | E3 / E1 | 0 | 0 |
| WI-ORDER-01 | 주민 주문 확정 | E3 / E1 | 1 | 1 |
| WI-ORDER-02 | 주문 상품 재고 예약 | E3 / E1 | 1 | 1 |
| WI-ORDER-03 | 주문 상품 피킹 | E3 / E1 | 1 | 0 |
| WI-ORDER-04 | 주문 상품 포장 | E3 / E1 | 1 | 0 |
| WI-ORDER-05 | 주문 상품 수령 준비 | E3 / E1 | 1 | 1 |
| WI-ORDER-06 | 주민 주문 상품 수령 | E3 / E1 | 1 | 1 |
| WI-ORDER-07 | 주민 상품 소비 | E3 / E1 | 1 | 1 |
| WI-NATURE-01 | 자연 지역 위험 징후 확인 | E3 / E7 | 2 | 3 |
| WI-NATURE-02 | 안전 거점으로 긴급 후퇴 | E3 / E1 | 2 | 2 |
| WI-NATURE-03 | 훼손된 자연 경로 복원 | E3 / E1 | 1 | 3 |
| WI-NATURE-04 | 탐사대 안전 회복 | E3 / E1 | 2 | 2 |
| WI-NATURE-05 | 벌목 도끼 획득 | E3 / E7 | 2 | 4 |
| WI-NATURE-06 | 나무 벌목 작업 시작 | E3 / E4 | 2 | 4 |
| WI-NATURE-07 | 오두막을 지을 터 선정 | E3 / E4 | 1 | 2 |
| WI-NATURE-08 | 오두막 건설 작업 시작 | E3 / E4 | 1 | 2 |
| WI-NATURE-09 | 오두막 안으로 들어가기 | E3 / E4 | 1 | 2 |
| WI-NATURE-10 | 오두막 밖으로 나가기 | E3 / E4 | 1 | 2 |
| WI-NATURE-11 | 황혼 위협 대응 방식 확정 | E3 / E7 | 2 | 2 |
| WI-NATURE-12 | 진행 중 작업 취소 | E3 / E4 | 4 | 4 |
| WI-NATURE-13 | 획득 자원 거점 보관 | E3 / E7 | 2 | 2 |
| WI-NATURE-14 | 오두막에서 수면·새벽 맞기 | E3 / E5 | 1 | 2 |
| WI-NATURE-15 | 다음 날 거점 확장 계획 선택 | E3 / E6 | 1 | 2 |
| WI-NATURE-16 | 현장 보급 꾸러미 제작 | E3 / E4 | 1 | 4 |
| WI-NATURE-17 | 현장 보급 제작 업무 위임 | E3 / E4 | 1 | 5 |
| WI-NATURE-18 | 벌목 통나무 줍기 | E3 / E7 | 1 | 3 |
| WI-REFLECT-01 | 승인 자료로 거점 성찰 확정 | E3 / E3 | 1 | 3 |
| WI-CARD-01 | 현재 세계의 메이저 아르카나 활성화 | E3 / E4 | 1 | 2 |
| WI-CON-01 | 영역 건물 건설 확정 | E3 / E7 | 3 | 2 |
| WI-CITY-01 | 도심 서비스 수요 확정 | E1 / E1 | 1 | 1 |
| WI-CITY-02 | 도심 서비스용 지역 재고 배정 | E1 / E1 | 1 | 1 |
| WI-CITY-03 | 도심 주민 서비스 처리 | E1 / E1 | 1 | 1 |
| WI-CITY-04 | 도심 서비스 결과 확인 | E1 / E1 | 1 | 0 |
| WI-WORLD-01 | NPC에게 반복 업무 배정 | E3 / E1 | 0 | 1 |
| WI-WORLD-02 | NPC에게 업무 역량 위임 | E3 / E1 | 0 | 1 |
| WI-WORLD-03 | 진행 중 세계 업무 취소 | E3 / E1 | 1 | 1 |
| WI-WORLD-04 | 손상된 시설 수리 | E3 / E1 | 0 | 1 |
| WI-WORLD-05 | 새로운 지역 발견 | E3 / E1 | 0 | 1 |
| WI-WORLD-06 | 일행 역할 카드 장착 | E3 / E1 | 0 | 1 |
| WI-WORLD-07 | 세계 활동 상태 변경 | E3 / E1 | 0 | 1 |
| WI-WORLD-08 | 하루 운영 턴 마감 | E3 / E1 | 1 | 1 |
| WI-REVIEW-01 | NPC 업무 결과 검토 확정 | E2 / E1 | 0 | 2 |

## WI-ACTOR-CONSUME — 물품 섭취

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"섭취 가능한 소유 물품 한 묶음을 소비한다. 주문 이행이나 치료 판정 전체는 소유하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ItemConsumed"],"effectCodes":["ItemConsumed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Nature기초약초회복.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-045, Q-046, Q-047, Q-048, Q-049, Q-050, Q-061, Q-062, Q-063, Q-064, Q-068, Q-069, Q-070, Q-071, Q-131, Q-133, Q-142, Q-150, Q-157, Q-269, Q-270, Q-271, Q-272, Q-273, Q-274, Q-275, Q-276, Q-277, Q-278, Q-279, Q-280, Q-281, Q-282, Q-283, Q-284, Q-285, Q-286, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296, Q-340, Q-341, Q-342, Q-343, Q-344, Q-345, Q-346, Q-347, Q-348, Q-349, Q-350, Q-351, Q-352, Q-353, Q-360, Q-361, Q-362, Q-363, Q-364, Q-365, Q-366, Q-367, Q-368, Q-369, Q-370, Q-371, Q-372, Q-373, Q-374, Q-375, Q-376, Q-377. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ACTOR-PLAN-SET — 개인 계획 설정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / personal-plan.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 설정이며 진척·완료·회복 수치 Task를 생성하지 않는다.","cancellationPolicy":"거부는 무변경. 수정은 새 Command와 현재 revision으로 요청한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["현재 플레이어 본인"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["PersonalPlanPolicyReady"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어 개인 계획 하나를 설정한다. 계획 완료나 성장 보상을 즉시 확정하지 않는다.","previewRule":"권한·계획 슬롯·목표·본문·예상 revision을 무변경 검증한다.","confirmRule":"동일 Command 재시도는 최초 결과를 재사용. 설정·최초 안정 자격 근거·행위 기록·revision을 원자적으로 확정한다.","blockReasonCodes":["PersonalPlanActorNotAuthorized","PersonalPlanUnknown","PersonalPlanObjectiveUnknown","PersonalPlanDescriptionInvalid","PersonalPlanExpectedRevisionMismatch","PersonalPlanUnchanged","PersonalPlanCommandPayloadConflict","PersonalPlanRevisionExhausted"]} |
| 결과 | {"completionStateCodes":["PersonalPlanSet"],"effectCodes":["PersonalPlanSet"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"거부는 무변경. 수정은 새 Command와 현재 revision으로 요청한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/Simulation개인계획Service.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/Simulation개인계획Contracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/Simulation개인계획.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/Simulation개인계획Tests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-night-day2.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-personal-plan-logic-e3-20260830","evidence:nature-heat-source-logic-e3-20260830","evidence:nature-night-day2-wi13-hosted-parity-20260826","evidence:nature-night-day2-wi14-hosted-parity-20260826","evidence:nature-night-day2-wi15-hosted-parity-20260826"],"blockers":["세계 자원 재생 E1→E3 구현 중. 개인 계획·열원 E3 증거는 보존.","열원 변경 경로 독립 Core E3까지만 시험. 이전 다른 WI의 증거는 보존하되 열원으로 전이하지 않는다.","v28 행위 원장·분야 성장 계보가 포함된 실제 입력과 LocalProcess·RemoteHost 반복 증거를 다시 확보해야 한다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Blocked","evidencePackageRefs":["evidence:nature-night-day2-wi13-playmode-20260826","evidence:nature-night-day2-wi14-playmode-20260826","evidence:nature-night-day2-wi15-playmode-20260826","evidence:nature-dual-loop-game-view-20260826","evidence:nature-night-day2-presentation-e7-20260826"],"blockers":["재생 성장 단계·채집 가능 상태의 판독 요구 E1만 정의.","표현 엔진의 행위 원장 cursor 소비와 같은 Revision Game View 증거가 없다.","Day2 계획판의 세 선택·비용·다음 행동 판독과 오두막 주변 구도, 사람 직접 입력·청음 수용이 남았다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 행위 원장·분야 성장 공통 관문 소급으로 기존 E6/E7 증거가 무효화됐다.; Unity 표현 엔진의 cursor 소비와 현재 Game View 증거를 다시 검증해야 한다.. 기존 다음 작업은 자동 실행 지시가 아니다: 세계 자원 재생 Logic E3 검증 후 캠페인 다음 WI로 전환.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMBAT-DIRECT-CONTROL-SET — 직접 전투 조종 전환

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"자기 Actor의 자동 조종 보류·재개 상태만 전환한다. 카메라 전환은 표현이고 피해·승패는 별도다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ActorDirectControlChanged"],"effectCodes":["ActorDirectControlChanged"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMBAT-TACTICAL-COMMAND — 분대 전술 명령 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"대상 분대의 전술 명령 하나를 확정한다. 이동·방어·후퇴는 명령 종류이며 실제 NPC 전투 결과를 대행하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["SquadTacticalOrderConfirmed"],"effectCodes":["SquadTacticalOrderConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-COOPERATION-PROPOSE — 공동체 협력 제안

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"협력 제안 하나를 기록한다. 제안만으로 수락·위임·현장 작업을 확정하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["CooperationProposed"],"effectCodes":["CooperationProposed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Farm병영방위/farm-barracks-defense.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-ENTRANCE-POLICY-SET — 공동체 출입 정책 설정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"입구 생활·경비 운영 정책을 설정한다. 실제 쉼터 건설은 건설 WI가 수행한다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["EntrancePolicySet"],"effectCodes":["EntrancePolicySet"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-HIRE — NPC 고용 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"고용 합의와 고용 관계를 확정한다. 작업 배정이나 역량 부여를 대신하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["NpcEmploymentConfirmed"],"effectCodes":["NpcEmploymentConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-MEMBERSHIP-CONFIRM — 공동체 정식 편입 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"당사자 동의와 조건을 확인해 정식 소속 관계를 확정한다. 임시 방문자 체류와 별도 관계다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["CommunityMembershipConfirmed"],"effectCodes":["CommunityMembershipConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-REMOTE-RESPONSE — 원격 응대 지시 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"응대 담당자에게 지시 하나를 확정한다. 지시의 현장 수행 결과는 담당 WI가 기록한다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["RemoteResponseOrderConfirmed"],"effectCodes":["RemoteResponseOrderConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-SUPPORT-MISSION-JOIN — 공동 지원 임무 참여

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"승인된 지원 임무의 참여를 확정한다. 협력 제안이나 전체 유지관리 작업 완료와 다르다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["SupportMissionParticipationConfirmed"],"effectCodes":["SupportMissionParticipationConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CON-BLUEPRINT-PLACE — 건설 청사진 배치

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"검증된 건설 청사진 배치를 확정한다. 실제 시공·자재 소비·건물 완공은 별도 행동이다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ConstructionBlueprintPlaced"],"effectCodes":["ConstructionBlueprintPlaced"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/world-interactions.json) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CON-DEMOLISH — 건설물 해체

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"대상 건설물 하나를 해체한다. 재료 회수는 승인 규칙의 원자적 부수 결과로만 추가할 수 있다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ConstructionDemolished"],"effectCodes":["ConstructionDemolished"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/world-interactions.json) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CON-MATERIAL-DEPOSIT — 건설 재료 투입

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"소유 재료를 대상 건설 원장에 투입한다. 시공 기여나 완공을 대신하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ConstructionMaterialDeposited"],"effectCodes":["ConstructionMaterialDeposited"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/world-interactions.json) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CON-WORK-CONTRIBUTE — 건설 시공 기여

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"대상 건설 작업에 유효 시공 기여를 기록한다. 작업 시간 경과만으로 다른 책임을 실행하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ConstructionWorkContributed"],"effectCodes":["ConstructionWorkContributed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/world-interactions.json) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CRAFT-BREW — 배합물 달이기

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"확정한 배합 배치 하나를 열원에서 달여 완성한다. 지식 습득·재료 채집·완성품 섭취는 제외한다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["BrewBatchCompleted"],"effectCodes":["BrewBatchCompleted"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-045, Q-046, Q-047, Q-048, Q-049, Q-050, Q-061, Q-062, Q-063, Q-064, Q-068, Q-069, Q-070, Q-071, Q-131, Q-133, Q-142, Q-150, Q-157, Q-269, Q-270, Q-271, Q-272, Q-273, Q-274, Q-275, Q-276, Q-277, Q-278, Q-279, Q-280, Q-281, Q-282, Q-283, Q-284, Q-285, Q-286, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296, Q-340, Q-341, Q-342, Q-343, Q-344, Q-345, Q-346, Q-347, Q-348, Q-349, Q-350, Q-351, Q-352, Q-353, Q-360, Q-361, Q-362, Q-363, Q-364, Q-365, Q-366, Q-367, Q-368, Q-369, Q-370, Q-371, Q-372, Q-373, Q-374, Q-375, Q-376, Q-377. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-EXPEDITION-DISPATCH — 탐사 임무 파견

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"승인한 탐사 임무의 인력·목적지·보급 파견을 확정한다. 보고·교전 결과는 별도 책임이다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["ExpeditionDispatched"],"effectCodes":["ExpeditionDispatched"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-FIELD-BOUNDARY-CONFIRM — 밭 경계 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"검증된 밭 경계와 관리 통로 범위를 확정한다. 경작·파종·개간을 동시에 완료하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["FieldBoundaryConfirmed"],"effectCodes":["FieldBoundaryConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-SOIL-AMEND — 토양 개량

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"승인 자재로 토양 상태를 개량한다. 생육 중인 작물 관리나 경계 확정과 다른 대상 원장이다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["SoilAmended"],"effectCodes":["SoilAmended"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-WATER-TRANSFER — 농업 용수 이송

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"수원에서 지정 수용처로 용수를 이송한다. 강수 생성·관수 설비 건설은 포함하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["IrrigationWaterTransferred"],"effectCodes":["IrrigationWaterTransferred"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-GUEST-PERMISSION-SET — 손님 활동 권한 설정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"특정 손님의 허용 활동 범위를 설정한다. 고용 관계나 NPC 업무 역량을 생성하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["GuestPermissionSet"],"effectCodes":["GuestPermissionSet"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HEAT-SOURCE-STATE-CHANGE — 열원 상태 변경

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / heat-source-state.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 상태 변경만 수행. 연소 Tick·수면·명상 제외.","cancellationPolicy":"거부 무변경·소화 환불 없음.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["신뢰된 단일 플레이어·열원 접근 권한","점화 시 기초 생존 능력"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["Off","Smoldering","Burning"],"resourceRequirements":["신뢰된 초기화의 연료 UnitEnergy·재고·Capacity. 출시 수치 제외"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"같은 열원의 점화·연료 보충·소화를 작업 코드로 구분한다. 범위는 열원 상태와 승인된 자원 비용이다.","previewRule":"권한·접근·revision·작업·상태·연료·용량을 무변경 검사한다.","confirmRule":"복사 후보의 연료·열원·행위 기록을 동일 revision으로 원자 교체. Command 멱등·입력 충돌 검사.","blockReasonCodes":["HeatExpectedRevisionMismatch","HeatActorNotAuthorized","HeatSourceUnknown","HeatSourceInaccessible","HeatBasicSurvivalRequired","HeatFuelNotApproved","HeatFuelQuantityInvalid","HeatFuelInsufficient","HeatCapacityExceeded","HeatIgnitionRequired","HeatAlreadyOff","HeatAlreadyBurning","HeatExtinguishPayloadInvalid","HeatOperationInvalid","HeatRevisionExhausted","HeatCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["HeatSourceStateChanged"],"effectCodes":["HeatSourceStateChanged"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"거부 무변경·소화 환불 없음.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Nature열원관리E3.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/nature-night-day2.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/Simulation열원상태Service.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/Simulation열원상태Contracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/Simulation열원상태.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/Simulation열원상태Tests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-night-day2.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-personal-plan-logic-e3-20260830","evidence:nature-heat-source-logic-e3-20260830","evidence:nature-night-day2-wi13-hosted-parity-20260826","evidence:nature-night-day2-wi14-hosted-parity-20260826","evidence:nature-night-day2-wi15-hosted-parity-20260826"],"blockers":["세계 자원 재생 E1→E3 구현 중. 개인 계획·열원 E3 증거는 보존.","열원 변경 경로 독립 Core E3까지만 시험. 이전 다른 WI의 증거는 보존하되 열원으로 전이하지 않는다.","v28 행위 원장·분야 성장 계보가 포함된 실제 입력과 LocalProcess·RemoteHost 반복 증거를 다시 확보해야 한다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Blocked","evidencePackageRefs":["evidence:nature-night-day2-wi13-playmode-20260826","evidence:nature-night-day2-wi14-playmode-20260826","evidence:nature-night-day2-wi15-playmode-20260826","evidence:nature-dual-loop-game-view-20260826","evidence:nature-night-day2-presentation-e7-20260826"],"blockers":["재생 성장 단계·채집 가능 상태의 판독 요구 E1만 정의.","표현 엔진의 행위 원장 cursor 소비와 같은 Revision Game View 증거가 없다.","Day2 계획판의 세 선택·비용·다음 행동 판독과 오두막 주변 구도, 사람 직접 입력·청음 수용이 남았다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 행위 원장·분야 성장 공통 관문 소급으로 기존 E6/E7 증거가 무효화됐다.; Unity 표현 엔진의 cursor 소비와 현재 Game View 증거를 다시 검증해야 한다.. 기존 다음 작업은 자동 실행 지시가 아니다: 세계 자원 재생 Logic E3 검증 후 캠페인 다음 WI로 전환.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HUB-DEMAND-ALLOCATE — Hub 수요 재고 할당

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Hub 내부 재고를 선택한 수요에 할당한다. City 서비스 배정이나 외부 수요 충족을 직접 확정하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["HubDemandInventoryAllocated"],"effectCodes":["HubDemandInventoryAllocated"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Hub수요분배/hub-demand-allocation.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HUB-SUPPLY-TASK-ACCEPT — Hub 조달 과제 수락

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"기한·위험·대가를 확인한 조달 과제 하나를 수락한다. 실패 해결 정책 Q250은 결정하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["HubSupplyTaskAccepted"],"effectCodes":["HubSupplyTaskAccepted"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Hub수요분배/hub-demand-allocation.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM — 목표 비축 미달 판매 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"비축 미달 위험을 재확인하고 판매 거래를 확정한다. 비축 경고 자체는 별도 WI로 만들지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["BelowReserveSaleConfirmed"],"effectCodes":["BelowReserveSaleConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/생존경제/survival-economy.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-HERB-GATHER — 약초 채집

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"접근 가능한 약초 노드에서 허용량을 채집한다. 일반 물품 획득 의미를 특화하지만 노드 잔량 검증을 별도로 요구한다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["HerbGathered"],"effectCodes":["HerbGathered"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-045, Q-046, Q-047, Q-048, Q-049, Q-050, Q-061, Q-062, Q-063, Q-064, Q-068, Q-069, Q-070, Q-071, Q-131, Q-133, Q-142, Q-150, Q-157, Q-269, Q-270, Q-271, Q-272, Q-273, Q-274, Q-275, Q-276, Q-277, Q-278, Q-279, Q-280, Q-281, Q-282, Q-283, Q-284, Q-285, Q-286, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296, Q-340, Q-341, Q-342, Q-343, Q-344, Q-345, Q-346, Q-347, Q-348, Q-349, Q-350, Q-351, Q-352, Q-353, Q-360, Q-361, Q-362, Q-363, Q-364, Q-365, Q-366, Q-367, Q-368, Q-369, Q-370, Q-371, Q-372, Q-373, Q-374, Q-375, Q-376, Q-377. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-TRACE-INVESTIGATE — 자연 흔적 조사

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"현장 흔적 관찰 결과를 조사 원장에 기록한다. 승인 Recipe 지식 추가와 다른 관찰 기록이다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["NatureTraceInvestigated"],"effectCodes":["NatureTraceInvestigated"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-SURVIVAL-RATION-POLICY-SET — 생존 배급 정책 설정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"소비·배급 정책을 확정한다. 재고 이동이나 실제 소비는 각 실행 WI가 수행한다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["RationPolicySet"],"effectCodes":["RationPolicySet"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/생존경제/survival-economy.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-TOWN-DELIVERY-INSPECT — Town 납품 검수

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Town 독립 보충 주문의 도착 물품 수량·품질을 검수한다. 기존 Hub 출고 연계 마트 검수와 자동 동일시하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["TownDeliveryInspected"],"effectCodes":["TownDeliveryInspected"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-TOWN-DELIVERY-RECEIVE — Town 납품 인수

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Town 보충 주문에 대한 도착 화물 인수를 확정한다. 주민 주문 수령이나 Hub 화물 인계와 다른 원장이다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["TownDeliveryReceived"],"effectCodes":["TownDeliveryReceived"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-TOWN-STOCK-PUTAWAY — Town 후방 재고 적재

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Town에서 검수된 재고를 승인 후방 슬롯에 적재한다. 검수나 진열을 대신하지 않는다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["TownStockPutAway"],"effectCodes":["TownStockPutAway"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-TOWN-STOCK-REPLENISH — Town 재고 보충 주문

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"운영자가 부족한 상점 재고의 보충 주문을 확정한다. 주민 소비 주문·기존 진열 이동과 다르다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["TownReplenishmentOrderConfirmed"],"effectCodes":["TownReplenishmentOrderConfirmed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-TOWN-SUPPLY-DISPATCH — Town 공급 운송 출발 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / registration-only.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"작업 시작·중단·완료 및 원자성 규칙은 승인 기획과 시험으로 확정한다. 등록만으로 Task를 실행하지 않는다.","cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["승인된 실행 주체·대상 권한 필요"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["RegistrationOnly:PreconditionsRequireApprovedDesign"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Town 보충 주문의 운송 출발을 확정한다. 회랑 진행·도착·인수는 이후 책임이다.","previewRule":"등록 기준: 권한·예상 revision·대상 조건을 읽기 전용 평가한다. 실제 규칙은 미구현이다.","confirmRule":"등록 기준: ExpectedRevision·Command 멱등성을 검증하고 한 책임의 결과·행위 기록을 같은 WorldRevision에 반영한다. 실제 실행은 미승인이다.","blockReasonCodes":["ApprovedDesignRequired","ImplementationNotStarted"]} |
| 결과 | {"completionStateCodes":["TownSupplyDispatched"],"effectCodes":["TownSupplyDispatched"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미착수 등록. 취소·재시도·회복 정책은 구현 전 승인해야 한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-RESOURCE-REGENERATE — 세계 자원 재생

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / resource-regeneration.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"Profile 성장 단계와 Tick당 결정적 빈셀 생성 상한. 자연 외 토지 제외.","cancellationPolicy":"거부된 Tick은 상태를 변경하지 않고 동일 TransitionId 재전송은 멱등 처리한다. 실제 Session 취소·복구 연결은 보류다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["권위 WorldTick 평가"],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["TrustedResourcePolicyReady","ConsecutiveWorldTickAvailable"],"resourceRequirements":["비용·수량·공간 조건은 후속 승인 기획에서 확정"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"신뢰된 Tick 입력으로 판본화된 자원 재생 조건을 평가해 노드·시계·행위 기록을 원자적으로 반영한다. 실제 Session 연결과 출시 수치·재생 주기는 보류다.","previewRule":"PreviewTick 내부 진단은 무변경. 플레이어 Preview/Confirm 미노출.","confirmRule":"신뢰된 ApplyTick만 연속 Tick·권한·ExpectedRevision·TransitionId 멱등성을 검증해 노드·시계·행위 기록을 원자적으로 반영한다.","blockReasonCodes":["ResourceRegenerationAuthorityRequired","ResourceRegenerationExpectedRevisionMismatch","ResourceRegenerationNextTickRequired","ResourceRegenerationRevisionExhausted","ResourceRegenerationTickOverflow","ResourceRegenerationTransitionConflict","ResourceRegenerationNodeCollision"]} |
| 결과 | {"completionStateCodes":["ResourceAvailabilityRestored","ResourceAvailabilityUnchanged"],"effectCodes":["ResourceAvailabilityRestored","ResourceAvailabilityUnchanged"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"거부된 Tick은 상태를 변경하지 않고 동일 TransitionId 재전송은 멱등 처리한다. 실제 Session 취소·복구 연결은 보류다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Nature세계자원재생E3.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/Simulation세계자원재생Service.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/Simulation세계자원재생Contracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/Simulation세계자원재생.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/Simulation세계자원재생Tests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-night-day2.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-personal-plan-logic-e3-20260830","evidence:nature-heat-source-logic-e3-20260830","evidence:nature-night-day2-wi13-hosted-parity-20260826","evidence:nature-night-day2-wi14-hosted-parity-20260826","evidence:nature-night-day2-wi15-hosted-parity-20260826"],"blockers":["세계 자원 재생 E1→E3 구현 중. 개인 계획·열원 E3 증거는 보존.","열원 변경 경로 독립 Core E3까지만 시험. 이전 다른 WI의 증거는 보존하되 열원으로 전이하지 않는다.","v28 행위 원장·분야 성장 계보가 포함된 실제 입력과 LocalProcess·RemoteHost 반복 증거를 다시 확보해야 한다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Blocked","evidencePackageRefs":["evidence:nature-night-day2-wi13-playmode-20260826","evidence:nature-night-day2-wi14-playmode-20260826","evidence:nature-night-day2-wi15-playmode-20260826","evidence:nature-dual-loop-game-view-20260826","evidence:nature-night-day2-presentation-e7-20260826"],"blockers":["재생 성장 단계·채집 가능 상태의 판독 요구 E1만 정의.","표현 엔진의 행위 원장 cursor 소비와 같은 Revision Game View 증거가 없다.","Day2 계획판의 세 선택·비용·다음 행동 판독과 오두막 주변 구도, 사람 직접 입력·청음 수용이 남았다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 행위 원장·분야 성장 공통 관문 소급으로 기존 E6/E7 증거가 무효화됐다.; Unity 표현 엔진의 cursor 소비와 현재 Game View 증거를 다시 검증해야 한다.. 기존 다음 작업은 자동 실행 지시가 아니다: 세계 자원 재생 Logic E3 검증 후 캠페인 다음 WI로 전환.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ACTOR-01 — 물품 획득

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / actor-equipment.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 즉시 완료하며 획득과 장착을 분리한다.","cancellationPolicy":"소유권 이전 전에는 확정하지 않으며 완료 뒤에는 별도 버리기 WI 없이 되돌리지 않는다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["물품을 획득할 수 있는 Player 또는 NPC"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["WorldItemAvailable","ActorDoesNotOwnItem"],"resourceRequirements":["획득 가능한 고유 물품 인스턴스"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"행위자가 세계에 놓인 고유 물품 인스턴스의 소유권을 얻어 인벤토리에 넣는다.","previewRule":"행위자·물품 인스턴스·현재 위치·중복 소유·장착 원장 개정을 검사하고 상태를 바꾸지 않는다.","confirmRule":"ExpectedEquipmentRevision으로 물품 인스턴스를 WorldPickup에서 Inventory로 한 번만 이전한다.","blockReasonCodes":["ActorEquipmentExpectedRevisionMismatch","ActorEquipmentItemInstanceNotFound","ActorEquipmentItemNotInWorld"]} |
| 결과 | {"completionStateCodes":["ItemOwnedInInventory"],"effectCodes":["ItemAcquired"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"소유권 이전 전에는 확정하지 않으며 완료 뒤에는 별도 버리기 WI 없이 되돌리지 않는다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationActorEquipmentService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationActorEquipment.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationActorEquipmentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-ACTOR-01 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-ACTOR-01 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ACTOR-02 — 장착 상태 변경

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / actor-equipment.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 즉시 완료하며 능력 코드는 장착 완료 상태에서만 파생한다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 완료 뒤에는 반대 장착 명령을 새로 확정한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["장착 원장을 가진 Player 또는 NPC"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["ItemOwnedInInventory","ItemEquipped"],"resourceRequirements":["소유한 고유 물품 인스턴스","허용 장착 슬롯"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"행위자가 소유한 고유 물품 인스턴스를 허용 슬롯에 장착·해제·교체하고 그 결과로 사용 능력을 얻거나 잃는다.","previewRule":"소유·허용 슬롯·현재 점유·교체 대상·장착 원장 개정을 검사하고 상태를 바꾸지 않는다.","confirmRule":"Equip·Unequip·Swap 중 하나를 ExpectedEquipmentRevision으로 원자적으로 확정한다.","blockReasonCodes":["ActorEquipmentExpectedRevisionMismatch","ActorEquipmentItemNotInInventory","ActorEquipmentSlotNotAllowed","ActorEquipmentSlotOccupied"]} |
| 결과 | {"completionStateCodes":["EquipmentStateChanged"],"effectCodes":["ItemEquipmentChanged"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 완료 뒤에는 반대 장착 명령을 새로 확정한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- 로컬 참조: `C:/Users/user/ssalddel/Assets/Ssalddel/Presentation/World/Nature생존Controller.cs` (이 조회에서 미검사)
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationActorEquipmentService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationActorEquipment.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationActorEquipmentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-001, Q-002, Q-003, Q-004, Q-005, Q-023, Q-024, Q-025, Q-026, Q-027, Q-028, Q-029, Q-030, Q-031, Q-032, Q-033, Q-034, Q-035, Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-132, Q-136, Q-141, Q-143, Q-144, Q-146, Q-147, Q-149, Q-153, Q-155, Q-156, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-ACTOR-02 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-ACTOR-02 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ACTOR-03 — 지식 습득

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / player-knowledge.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 즉시 완료하며 채집·달이기·섭취·약효를 포함하지 않는다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 이미 아는 처방의 재확정은 무변경으로 재사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["플레이어 지식 원장을 가진 Player"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["ReadableKnowledgeSourceAvailable","RecipeNotKnown"],"resourceRequirements":["접근 가능한 승인 지식 출처","승인된 RecipeStableId"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 접근 가능한 승인 지식 출처에서 RecipeStableId 하나를 자기 지식 원장에 멱등하게 추가한다.","previewRule":"플레이어·WorldRevision·지식 출처 접근 가능성·승인 RecipeStableId를 검사하며 상태를 바꾸지 않는다.","confirmRule":"ExpectedWorldRevision으로 RecipeStableId를 한 번만 추가하고 같은 revision의 행위 기록을 남긴다.","blockReasonCodes":["PlayerKnowledgeExpectedRevisionMismatch","PlayerKnowledgePlayerMismatch","PlayerKnowledgeRecipeUnknown","PlayerKnowledgeSourceUnavailable"]} |
| 결과 | {"completionStateCodes":["RecipeKnown"],"effectCodes":["RecipeKnowledgeAdded"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 이미 아는 처방의 재확정은 무변경으로 재사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Nature기초약초회복.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/nature-basic-herbal-recovery.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/LocalSimulationRuntime.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationPlayerKnowledgeService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationPlayerKnowledgeContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationPlayerKnowledge.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Server/Controllers/SimulationPlayerKnowledgeController.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationPlayerKnowledgeTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity.Tests/Simulation처방기록PresentationPreparationTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity.Tests/Simulation처방지식CardFamilyTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity/Runtime/Cards/Simulation처방기록PresentationPreparation.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity/Runtime/Cards/Simulation처방지식CardFamily.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-045, Q-046, Q-047, Q-048, Q-049, Q-050, Q-061, Q-062, Q-063, Q-064, Q-068, Q-069, Q-070, Q-071, Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-131, Q-133, Q-136, Q-142, Q-143, Q-144, Q-146, Q-147, Q-150, Q-155, Q-157, Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-269, Q-270, Q-271, Q-272, Q-273, Q-274, Q-275, Q-276, Q-277, Q-278, Q-279, Q-280, Q-281, Q-282, Q-283, Q-284, Q-285, Q-286, Q-287, Q-288, Q-289, Q-290, Q-291, Q-292, Q-293, Q-294, Q-295, Q-296, Q-340, Q-341, Q-342, Q-343, Q-344, Q-345, Q-346, Q-347, Q-348, Q-349, Q-350, Q-351, Q-352, Q-353, Q-360, Q-361, Q-362, Q-363, Q-364, Q-365, Q-366, Q-367, Q-368, Q-369, Q-370, Q-371, Q-372, Q-373, Q-374, Q-375, Q-376, Q-377, Q-378, Q-379, Q-380, Q-383, Q-384, Q-385, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-basic-herbal-recovery.v1 / PlayableUnit / 통합 E4, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-basic-herbal-recovery-logic-e4-20260828"],"blockers":["Session·Save 영향 경로 재검증 필요"]},"presentation":{"trackCode":"Presentation","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-basic-herbal-recovery-presentation-e3-20260828","evidence:nature-basic-herbal-recovery-presentation-e4-20260829"],"blockers":["실제 자산·통행·입력·Game View 미검증"]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-basic-herbal-recovery.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=9B974AFE21BB8F5983B9F8A036187738A288B5E3B81451D50192F389F34A6F6F. 후보 상세는 -Wi -Id WI-ACTOR-03 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 플레이어가 물리적인 처방 기록에 접근해 읽을 수 있음·이미 아는 처방·현재 차단 상태를 구분한다.; VisualKey: Knowledge.Recipe.Record.OpenBook, Knowledge.Recipe.Record.LoosePaper. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_BookOpen_01.prefab; 대체: Assets/Synty/PolygonCity/Prefabs/Props/SM_Prop_Paper_01.prefab, Assets/Synty/PolygonGeneric/Prefabs/Props/SM_Gen_Prop_Papers_01.prefab, Assets/Synty/PolygonConstruction/Prefabs/Items/SM_Item_Clipboard_01.prefab; fallback: Primitive:ReadableKnowledgeSourceMarker, Card:RecipeKnowledge.
  - 배치/Anchor: Q-133에 따라 첫 읽기 기록은 폐야영지의 지지면 위에서 3인칭 플레이어 접근 방향을 향하고 통행과 조준을 막지 않는다. Farm·Town 진입은 선행 조건이 아니다. / ReadableKnowledgeSource 접근 범위가 WI-ACTOR-03 Query·Preview·Confirm을 호출하되 Prefab은 Simulation 권위를 변경하지 않는다.. 준비 상태: Conditional.
  - 열린 준비: 실제 지지면·활성 Bounds·통행·입력·저장 재진입 검증 필요.
  - 기존 차단: E5 Session/Save/실제 World 결속 검증 미완료. 기존 다음 작업은 자동 실행 지시가 아니다: E5 승인 범위: Session·Save·같은 Core·실제 자산 발현을 검증한다. 현재 E는 보존한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-COMMUNITY-VISITOR-STAY — 방문자 임시 체류 결정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / community-visitor-stay.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 즉시 결정까지만 완료하며 체류 기간·연장·정식 편입은 후속 WI로 분리한다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 결정 뒤 재검토는 후속 관계 WI가 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Spatial.VisitorWaitingAnchor","Spatial.GuestRestAnchor","Spatial.VisitorDepartureAnchor"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 야영지 공동체를 응대할 수 있는 Player"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["NatureCampVisitorAwaitingDecision"],"resourceRequirements":["대기 방문자","수용 선택일 때 남은 손님 수용 칸"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 Nature 야영지 입구에서 대기하는 방문자 한 명의 현재 임시 체류를 수용하거나 거절한다.","previewRule":"WorldRevision·방문자 대기 상태·선택 코드·수용 여력을 검사하고 예상 상태와 마음 계보만 반환한다.","confirmRule":"ExpectedWorldRevision으로 수용 또는 거절을 한 번 확정하고 방문자 상태·수용 칸·공동체 마음 계보·행위 기록을 같은 revision에 남긴다.","blockReasonCodes":["CommunityVisitorStayExpectedRevisionMismatch","CommunityVisitorUnknown","CommunityVisitorAlreadyDecided","CommunityVisitorCapacityUnavailable","CommunityVisitorDecisionInvalid","CommunityVisitorStayCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["CommunityVisitorTemporaryStayAccepted","CommunityVisitorRejected"],"effectCodes":["CommunityVisitorStayDecisionRecorded","CommunityMindTraceRecorded"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 결정 뒤 재검토는 후속 관계 WI가 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Nature야영지방문자임시체류.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/nature-camp-visitor-stay.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationCommunityVisitorStayService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationCommunityVisitorStayContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationCommunityVisitorStay.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationCommunityVisitorStayTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity.Tests/Simulation방문자체류PresentationPreparationTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity/Runtime/Cards/Simulation방문자체류PresentationPreparation.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-199, Q-200, Q-201, Q-202, Q-203, Q-204, Q-205, Q-206, Q-207, Q-208, Q-209, Q-210, Q-211, Q-212, Q-213, Q-214, Q-215, Q-216, Q-217, Q-218, Q-219, Q-354, Q-355, Q-356, Q-357, Q-358, Q-359. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-camp-visitor-stay.v1 / PlayableUnit / 통합 E4, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-camp-visitor-stay-e3-20260829","evidence:nature-camp-visitor-stay-e4-20260830"],"blockers":["Session·Save 영향 경로 재검증 필요"]},"presentation":{"trackCode":"Presentation","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-camp-visitor-stay-e3-20260829","evidence:nature-camp-visitor-stay-e4-20260830"],"blockers":["실제 자산·통행·입력·Game View 미검증"]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-camp-visitor-stay.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=D32C405875A4D3663E9A71D600910DEFCF7512769F9E36CEAD6A7E1314F3C2B4. 후보 상세는 -Wi -Id WI-COMMUNITY-VISITOR-STAY 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 플레이어가 방문자 카드에서 결정 대기·임시 체류·거절, 남은 수용 칸과 선택 가능 여부를 읽는 순간이다.; VisualKey: Community.Visitor.Stay.AwaitingDecision, Community.Visitor.Stay.TemporaryStay, Community.Visitor.Stay.Rejected. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Male_01.prefab, Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Bench_01.prefab, Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Bed_01.prefab; 대체: Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Female_01.prefab, Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Shelter_01.prefab, Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_Bed_Single_01.prefab; fallback: Primitive:CommunityVisitorMarker, Card:CommunityVisitorStay.
  - 배치/Anchor: Q-207~Q-209에 따라 결정 대기 방문자는 Nature 안전 경계 안쪽이면서 출입구와 생활 중심부 사이의 완충 위치에 두고, 임시 체류 방문자는 기존 플레이어 침상과 다른 손님 Slot을 사용하며, 거절 상태는 출입구 방향 이탈 기준점만 사용한다. / 상태별 H 기준점은 방문자 카드 조회와 WI-COMMUNITY-VISITOR-STAY Preview 진입만 제공하며 Prefab·Animator는 Confirm과 WorldRevision을 변경하지 않는다.. 준비 상태: Conditional.
  - 열린 준비: 실제 지지면·활성 Bounds·통행·입력·저장 재진입 검증 필요; Starter 남성 Actor Idle/Walk 재생·취소 복귀 검증 필요.
  - 제한 준비 결과 d396StateBindingPreparation: StateBindingValidationAnd21FixtureTestsPassed_ProductUnlinked. [기술보고](../../../docs/Reports/WI전체-E4-방문자상태대응-D396-2026-08-31.md); 명세의 writePaths/validation 참조. 이 결과를 개별 WI 전체 또는 E 달성으로 합성하지 않는다.
  - 기존 차단: E5 Session/Save/실제 World 결속 검증 미완료. 기존 다음 작업은 자동 실행 지시가 아니다: E5 승인 범위: Session·Save·같은 Core·실제 자산 발현을 검증한다. 현재 E는 보존한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-DEFENSE-MOBILIZE — 방위 분대 소집

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / farm-defense-mobilization.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 소집 확정까지만 즉시 완료하며 전투 결과·부상·보급·귀환은 후속 WI로 분리한다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 출동 뒤 복귀는 WI-FARM-DEFENSE-RETURN이 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Spatial.FarmDefenseWatchAnchor","Spatial.FarmDefenseMusterAnchor"],"hRefs":["h1-stock:farm-worker-waiting"],"placementVerified":false} |
| 나 | {"actorRequirements":["준비 상태의 Farm 방위 분대"],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["FarmDefenseThreatApproaching","FarmDefenseSquadReady"],"resourceRequirements":["접근 중인 위협","분대에 배정된 작업자"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"접근 중인 위협에 대응할 준비 분대 하나를 출동시키고 배정 작업자의 Farm 생산 기여를 중단한다.","previewRule":"WorldRevision·분대 준비 상태·위협 존재·기존 출동 여부를 검사하고 상태를 바꾸지 않는다.","confirmRule":"ExpectedWorldRevision으로 분대 출동과 배정 작업자의 생산 기여 중단을 한 번에 확정하고 같은 revision의 행위 기록을 남긴다.","blockReasonCodes":["FarmDefenseMobilizationExpectedRevisionMismatch","FarmDefenseSquadUnknown","FarmDefenseSquadNotReady","FarmDefenseThreatUnknown","FarmDefenseSquadAlreadyMobilized","FarmDefenseMobilizationCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["FarmDefenseSquadMobilized","FarmProductionContributionSuspended"],"effectCodes":["FarmDefenseSquadMobilized","FarmProductionContributionSuspended"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 출동 뒤 복귀는 WI-FARM-DEFENSE-RETURN이 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Farm병영방위.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/farm-barracks-defense.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationFarmDefenseMobilizationService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationFarmDefenseMobilizationContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationFarmDefenseMobilization.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationFarmDefenseMobilizationTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity.Tests/SimulationFarm방위소집PresentationPreparationTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Unity/Runtime/Cards/SimulationFarm방위소집PresentationPreparation.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-223, Q-224, Q-225, Q-226, Q-227, Q-228, Q-229, Q-230, Q-231, Q-232, Q-233, Q-234, Q-235, Q-236, Q-237, Q-238, Q-239. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-barracks-defense.v1 / PlayableUnit / 통합 E3, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey는 승인되지 않았다."]},"presentation":{"trackCode":"Presentation","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN은 결정적 귀환 카드까지만 검증했고 실제 이동·초소·치료·생산 재합류 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-DEFENSE-MOBILIZE는 통합 E4, 분대 배정·보급·결과 발현은 통합 E3에서 대기하고, 활성 WI-FARM-DEFENSE-RETURN은 통합 E3 상한에서 E4 승인을 기다린다.. 기존 다음 작업은 자동 실행 지시가 아니다: 다음 승인 revision에서 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey를 동결하거나 치료·생산 재합류 후속 WI를 별도 승인한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-SQUAD-ASSIGN — 경비 초소 분대 배정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / farm-defense-squad-assignment.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 분대 배정 확정까지만 즉시 완료하며 편성·훈련·보급·영웅 조작·출동은 후속 WI로 분리한다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 배정 해제·교체는 별도 후속 WI가 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Spatial.FarmDefenseMusterAnchor"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["Farm 방위 편성 권한이 있는 Host Player","미배정 Farm 방위 분대"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["FarmDefenseOutpostSlotEmpty","FarmDefenseSquadUnassigned"],"resourceRequirements":["등록된 Farm 경비 초소","빈 초소 배치 슬롯","미배정 분대"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 Farm 경비 초소의 빈 배치 슬롯 하나에 미배정 방위 분대 하나를 배정한다.","previewRule":"ExpectedWorldRevision과 초소·슬롯·분대 존재, 슬롯 점유와 기존 분대 배정을 검사하고 상태를 바꾸지 않는다.","confirmRule":"ExpectedWorldRevision으로 빈 초소 슬롯과 미배정 분대를 1:1로 결속하고 같은 revision의 행위 기록을 한 번 남긴다.","blockReasonCodes":["FarmSquadAssignmentExpectedRevisionMismatch","FarmDefenseOutpostUnknown","FarmDefenseOutpostSlotUnknown","FarmDefenseSquadUnknown","FarmDefenseSquadAlreadyAssigned","FarmDefenseOutpostSlotOccupied","FarmSquadAssignmentCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["FarmDefenseSquadAssignedToOutpostSlot"],"effectCodes":["FarmDefenseSquadAssigned"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-DEFENSE-MOBILIZE"],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 배정 해제·교체는 별도 후속 WI가 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Farm병영방위.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/farm-barracks-squad-assignment.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationFarmSquadAssignmentService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationFarmSquadAssignmentContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationFarmSquadAssignment.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationFarmSquadAssignmentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-barracks-defense.v1 / PlayableUnit / 통합 E3, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey는 승인되지 않았다."]},"presentation":{"trackCode":"Presentation","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN은 결정적 귀환 카드까지만 검증했고 실제 이동·초소·치료·생산 재합류 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-DEFENSE-MOBILIZE는 통합 E4, 분대 배정·보급·결과 발현은 통합 E3에서 대기하고, 활성 WI-FARM-DEFENSE-RETURN은 통합 E3 상한에서 E4 승인을 기다린다.. 기존 다음 작업은 자동 실행 지시가 아니다: 다음 승인 revision에서 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey를 동결하거나 치료·생산 재합류 후속 WI를 별도 승인한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-SQUAD-SUPPLY — 경비 분대 식량·장비 보급

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / farm-defense-squad-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 보급 확정까지만 즉시 완료하며 개별 장비 수리·훈련·출동·자동 재보급은 후속 WI로 분리한다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 확정 뒤 소비 취소나 재보급은 별도 후속 규칙이 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Spatial.FarmDefenseSupplyAnchor"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["Farm 방위 보급 권한이 있는 Host Player","보급이 필요한 Farm 방위 분대"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["FarmDefenseSquadSupplyRequired"],"resourceRequirements":["분대별 식량 필요량","분대별 장비 내구도 복구 필요량"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 초기 권위 상태에 기록된 필요량만큼 식량과 장비 내구도 복구 능력을 소비해 Farm 경비 분대 하나를 보급 완료로 만든다.","previewRule":"ExpectedWorldRevision·분대 존재·기보급 여부와 식량·내구도 복구 능력 충족 여부를 검사하고 상태를 바꾸지 않는다.","confirmRule":"초기 권위 상태가 정한 필요량만큼 두 자원을 원자적으로 소비하고 분대를 보급 완료로 바꾸며 같은 revision의 행위 기록을 남긴다.","blockReasonCodes":["FarmSquadSupplyExpectedRevisionMismatch","FarmSquadSupplySquadUnknown","FarmSquadSupplySquadAlreadySupplied","FarmSquadSupplyFoodInsufficient","FarmSquadSupplyDurabilityRestoreInsufficient","FarmSquadSupplyCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["FarmDefenseSquadSupplied"],"effectCodes":["FarmDefenseSquadSupplied"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-DEFENSE-MOBILIZE"],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 확정 뒤 소비 취소나 재보급은 별도 후속 규칙이 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Farm병영방위.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/farm-barracks-squad-supply.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationFarmSquadSupplyService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationFarmSquadSupplyContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationFarmSquadSupply.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationFarmSquadSupplyTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-barracks-defense.v1 / PlayableUnit / 통합 E3, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey는 승인되지 않았다."]},"presentation":{"trackCode":"Presentation","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN은 결정적 귀환 카드까지만 검증했고 실제 이동·초소·치료·생산 재합류 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-DEFENSE-MOBILIZE는 통합 E4, 분대 배정·보급·결과 발현은 통합 E3에서 대기하고, 활성 WI-FARM-DEFENSE-RETURN은 통합 E3 상한에서 E4 승인을 기다린다.. 기존 다음 작업은 자동 실행 지시가 아니다: 다음 승인 revision에서 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey를 동결하거나 치료·생산 재합류 후속 WI를 별도 승인한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-DEFENSE-RESOLVE — Farm 방어 성공 결과 발현

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / farm-defense-resolution.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 발현하며 전투 승패·피해·보상 수치를 계산하지 않고 부상·치료·귀환을 후속 WI에 남긴다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 발현 뒤 결과 취소·보정은 별도 권위 명령이 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["전투 권위가 확정한 결과","결과를 만든 Farm 방위 분대"],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["FarmDefenseResultConfirmed"],"resourceRequirements":["확정 위협 감소","확정 안전 종료 Tick","확정 생산/회복 보정","확정 전리품"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"전투 권위가 확정한 Farm 방어 성공 결과 묶음을 위협 감소·안전 기간·생산/회복 보정·전리품으로 한 번 발현한다.","previewRule":"ExpectedWorldRevision·조우 존재·미발현 여부·성공 결과 여부를 검사하고 확정 결과 수치를 읽기만 한다.","confirmRule":"확정 성공 결과를 위협·안전 기간·생산/회복 보정·전리품에 원자적으로 발현하고 같은 revision의 행위 기록을 남긴다.","blockReasonCodes":["FarmDefenseResolutionExpectedRevisionMismatch","FarmDefenseResolutionEncounterUnknown","FarmDefenseResolutionEncounterAlreadyResolved","FarmDefenseResolutionResultNotSuccessful","FarmDefenseResolutionCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["FarmDefenseResultManifested"],"effectCodes":["FarmDefenseResolved","FarmThreatReduced","FarmSafePeriodExtended","FarmProductionRecoveryModified","FarmDefenseLootAdded"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-DEFENSE-RETURN"],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 발현 뒤 결과 취소·보정은 별도 권위 명령이 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Farm병영방위.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/farm-barracks-defense-resolution.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationFarmDefenseResolutionService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationFarmDefenseResolutionContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationFarmDefenseResolution.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationFarmDefenseResolutionTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-223, Q-224, Q-225, Q-226, Q-227, Q-228, Q-229, Q-230, Q-231, Q-232, Q-233, Q-234, Q-235, Q-236, Q-237, Q-238, Q-239. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-barracks-defense.v1 / PlayableUnit / 통합 E3, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey는 승인되지 않았다."]},"presentation":{"trackCode":"Presentation","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN은 결정적 귀환 카드까지만 검증했고 실제 이동·초소·치료·생산 재합류 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-DEFENSE-MOBILIZE는 통합 E4, 분대 배정·보급·결과 발현은 통합 E3에서 대기하고, 활성 WI-FARM-DEFENSE-RETURN은 통합 E3 상한에서 E4 승인을 기다린다.. 기존 다음 작업은 자동 실행 지시가 아니다: 다음 승인 revision에서 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey를 동결하거나 치료·생산 재합류 후속 WI를 별도 승인한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-DEFENSE-RETURN — Farm 방위 분대 초소 귀환 인계

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / farm-defense-return.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 판본은 귀환 인계까지만 즉시 완료하며 치료·휴식·생산 재합류와 이동 표현은 별도 WI가 소유한다.","cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 귀환 뒤 치료·재합류 취소나 재배정은 후속 권위 규칙이 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Spatial.FarmDefenseReturnOutpostAnchor"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["결과가 확정된 Farm 방위 분대"],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["FarmDefenseResultResolved","FarmDefenseReturnPending"],"resourceRequirements":["귀환 정의","치료 필요 Actor","생산 재합류 후보 Actor"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"결과가 확정된 Farm 방위 분대를 지정 초소로 귀환 완료시키고 부상자와 생존 작업자를 치료·생산 재합류 후속 대기열에 인계한다.","previewRule":"ExpectedWorldRevision·귀환 정의·결과 확정·기귀환 여부를 검사하고 후속 인계 건수를 읽기만 한다.","confirmRule":"분대를 초소 귀환 완료로 바꾸고 치료·생산 재합류 후보를 서로 겹치지 않는 대기열에 한 번 인계하며 같은 revision의 행위 기록을 남긴다.","blockReasonCodes":["FarmDefenseReturnExpectedRevisionMismatch","FarmDefenseReturnUnknown","FarmDefenseReturnResultNotResolved","FarmDefenseReturnAlreadyReturned","FarmDefenseReturnCommandPayloadConflict"]} |
| 결과 | {"completionStateCodes":["FarmDefenseSquadReturned"],"effectCodes":["FarmDefenseSquadReturned"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"확정 전 Preview는 폐기할 수 있고 귀환 뒤 치료·재합류 취소나 재배정은 후속 권위 규칙이 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/PlayableLoops/Farm병영방위.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/execution-ledgers/work-orders/farm-barracks-defense-return.e7-work-order.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationFarmDefenseReturnService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationFarmDefenseReturnContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationFarmDefenseReturn.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationFarmDefenseReturnTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-223, Q-224, Q-225, Q-226, Q-227, Q-228, Q-229, Q-230, Q-231, Q-232, Q-233, Q-234, Q-235, Q-236, Q-237, Q-238, Q-239. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-barracks-defense.v1 / PlayableUnit / 통합 E3, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey는 승인되지 않았다."]},"presentation":{"trackCode":"Presentation","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:farm-barracks-defense-e3-20260829","evidence:farm-barracks-defense-e4-20260830","evidence:farm-barracks-squad-assignment-e3-20260830","evidence:farm-barracks-squad-supply-e3-20260830","evidence:farm-barracks-defense-resolution-e3-20260830","evidence:farm-barracks-defense-return-e3-20260830"],"blockers":["활성 WI-FARM-DEFENSE-RETURN은 결정적 귀환 카드까지만 검증했고 실제 이동·초소·치료·생산 재합류 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-DEFENSE-MOBILIZE는 통합 E4, 분대 배정·보급·결과 발현은 통합 E3에서 대기하고, 활성 WI-FARM-DEFENSE-RETURN은 통합 E3 상한에서 E4 승인을 기다린다.. 기존 다음 작업은 자동 실행 지시가 아니다: 다음 승인 revision에서 WI-FARM-DEFENSE-RETURN의 E4 판독 순간·InteractionAnchor·VisualKey를 동결하거나 치료·생산 재합류 후속 WI를 별도 승인한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-01 — 경작지 밭갈이

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / farm-survival.scenic-season.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"확정 뒤 Tick에 따라 밭갈기 작업을 진행한다.","cancellationPolicy":"완료 전 예약만 계보에 따라 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["WorkerAccessible","TillingWorkArea"],"hRefs":["h1-stock:farm-production"],"placementVerified":false} |
| 나 | {"actorRequirements":["Player 또는 농장 작업 NPC"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["Untilled"],"resourceRequirements":["노동 또는 체력"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"행위자가 농지 작업 구획을 경작 가능한 상태로 만든다.","previewRule":"행위자·대상 토양·공간·노동을 검사하고 상태를 바꾸지 않는다.","confirmRule":"결정·작업·행위자와 작업 영역 예약을 원자적으로 만든다.","blockReasonCodes":["SimulationFarmSoilTileNotFound","SimulationFarmActorBusy"]} |
| 결과 | {"completionStateCodes":["Tilled"],"effectCodes":["SoilTilled"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-02"],"cancellationPolicy":"완료 전 예약만 계보에 따라 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/spatial-capabilities.v1.json) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFarmSurvival.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-136, Q-143, Q-144, Q-146, Q-147, Q-155, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-crop-cycle.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["Session·Save 영향 경로 재검증 필요"]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":[],"blockers":["실제 자산·통행·입력·Game View 미검증"]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=CEF233D99C8FCF6A588999BF85B9AA359CBEEF71C14D073830CAD423C886F6E7. 후보 상세는 -Wi -Id WI-FARM-01 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 밭갈이·파종·관리·수확 결과와 다음 파종을 판독한다.; VisualKey: farm.crop.prepare, farm.crop.plant, farm.crop.grow, farm.crop.harvest. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: Assets/Synty/PolygonFarm/Prefabs/Environments/SM_Env_Dirt_Rows_01.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_S.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab; 대체: Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Barn_02.prefab; fallback: 기존 지면+상태 Outline/UI: Synty 실물 증거 아님.
  - 배치/Anchor: D-320 추가 승인 연구 r2에 따라 Barn/Player native 비율을 보존하고 실측 회전 외곽·Player 접근 여유로 새 후보의 예약부지·마당·통로를 조정한다. r1 후보 및 거부 회귀는 보존한다. D390: approvedStudyRefs r1/r2의 적용 공간 기준을 소비하되 한 재배 대상이 소비하는 지지·접근·통행·소유 조건을 확인한다. 구후보/확대연구/현 Scene 계보 차이는 미검증이며 전체 Farm 시설 활성화를 현재 준비의 선행조건으로 추가하지 않는다. / 기존 Player의 실제 접근과 Preview/Confirm 연결. 배치 엔진은 권위를 변경하지 않는다.. 준비 상태: Conditional.
  - 열린 준비: 실측9자산·구후보16/16 예상 거부 확인. 확대 후보 생성·통로/지지면 검증·실제 Scene 결속은 미완료.; D390 필수/현재 대상: Logic E5 사본·Session/판본·상태→정확 family/제품 소비·필수 컴포넌트(null9 과거관측)·지지/접근/통행·입력/결과·표시/해제 소유 결손은 차단 유지. 미관측은 Conditional이지 면제 아님. 개발 Logic E1/E5·Presentation E4, 실제 공간 관측은 별도 승인.; D390 간섭/후속: 현재 대상의 통로 차단·카메라 가림·자원/상태 간섭은 가장 이른 원인 E의 차단이며 E9로 이관해 우회하지 않는다. 주변 장식의 실제 영향은 미검증이다. 데이터 전용 D386/D389는 Scene/다른 WI/Renderer를 읽거나 쓰지 않는 코드·순수 시험 범위에서만 다른 장식 완성의 비선행 근거가 있다; 실제 E5로 일반화 금지. 공간 영향 조사 후 무영향이 확인된 범위만 후속 품질/조화로 분리. 담당 개발·공간, 검토 위치 이 openGapRefs와 기술 보고.; D390 변경 영향: 후보/기준 공간·대상·Session/판본·배치/소유 계약 변경 시 해당 소비자/근거만 재검토. D389 문맥 지문 불일치는 재검사 차단, 영향 미확인은 미검증 유지. 논리와 무관한 미감 변경으로 관계없는 논리 증거를 자동 폐기하지 않으며 실제 E9는 E8 Core 둘 이상/AreaHarmonySet·사람 승인 유지.; Session·새 밭 두 주기·저장 재진입 미검증; D388: 기존4후보/Accepted 연구·과거 의존48파일을 대조해 연결·설정 보완으로 분류했다. 실제 imported 기하/지지/접근/제품 소비는 미검사이며 가공·신규 제작 필요는 미확정이다. 상태명과 정확 family 불일치·9밭 null 수리는 별개. 전체 팩 재조사·새 촬영·가공을 자동 실행하지 않는다..
  - 제한 준비 결과 d396VisualCandidatePreparation: PurePreparationAndFocusedTestsPassed_ProductUnlinked. [기술보고](../../../docs/Reports/WI전체-E4-Farm시각후보준비-D396-2026-08-31.md); 명세의 writePaths/validation 참조. 이 결과를 개별 WI 전체 또는 E 달성으로 합성하지 않는다.
  - 기존 차단: E5 Session/Save/실제 World 결속 검증 미완료. 기존 다음 작업은 자동 실행 지시가 아니다: D-320 연구 r2 추가 승인으로 Barn native 실측 수용 후보·통로 검증을 공간 담당에 분담한다. 개발은 Session/Save/자연 회복 연결을 병행하고 실제 Scene/화면은 후보 검토 후 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-02 — 경작지 씨앗 파종

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.farm.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 Tick에 재배 단위를 Growing으로 만든다.","cancellationPolicy":"작업 전 예약을 반환하며 이미 소비된 종자는 별도 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CropProduction","WorkerAccessible"],"hRefs":["h1-stock:farm-production","h1-stock:farm-seed-preparation"],"placementVerified":false} |
| 나 | {"actorRequirements":["파종 가능 행위자"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["Tilled"],"resourceRequirements":["종자","노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"경작된 농지에 작물 재배 단위를 시작한다.","previewRule":"경작 상태·종자·행위자·농지 수용 가능성을 검사한다.","confirmRule":"대상 농지와 종자를 예약하고 파종 작업을 만든다.","blockReasonCodes":["CultivationParcelUnavailable","SeedInsufficient"]} |
| 결과 | {"completionStateCodes":["Growing"],"effectCodes":["CultivationStarted"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-03"],"cancellationPolicy":"작업 전 예약을 반환하며 이미 소비된 종자는 별도 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/spatial-capabilities.v1.json) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-136, Q-143, Q-144, Q-146, Q-147, Q-155, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-crop-cycle.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["Session·Save 영향 경로 재검증 필요"]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":[],"blockers":["실제 자산·통행·입력·Game View 미검증"]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=CEF233D99C8FCF6A588999BF85B9AA359CBEEF71C14D073830CAD423C886F6E7. 후보 상세는 -Wi -Id WI-FARM-02 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 밭갈이·파종·관리·수확 결과와 다음 파종을 판독한다.; VisualKey: farm.crop.prepare, farm.crop.plant, farm.crop.grow, farm.crop.harvest. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: Assets/Synty/PolygonFarm/Prefabs/Environments/SM_Env_Dirt_Rows_01.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_S.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab; 대체: Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Barn_02.prefab; fallback: 기존 지면+상태 Outline/UI: Synty 실물 증거 아님.
  - 배치/Anchor: D-320 추가 승인 연구 r2에 따라 Barn/Player native 비율을 보존하고 실측 회전 외곽·Player 접근 여유로 새 후보의 예약부지·마당·통로를 조정한다. r1 후보 및 거부 회귀는 보존한다. D390: approvedStudyRefs r1/r2의 적용 공간 기준을 소비하되 한 재배 대상이 소비하는 지지·접근·통행·소유 조건을 확인한다. 구후보/확대연구/현 Scene 계보 차이는 미검증이며 전체 Farm 시설 활성화를 현재 준비의 선행조건으로 추가하지 않는다. / 기존 Player의 실제 접근과 Preview/Confirm 연결. 배치 엔진은 권위를 변경하지 않는다.. 준비 상태: Conditional.
  - 열린 준비: 실측9자산·구후보16/16 예상 거부 확인. 확대 후보 생성·통로/지지면 검증·실제 Scene 결속은 미완료.; D390 필수/현재 대상: Logic E5 사본·Session/판본·상태→정확 family/제품 소비·필수 컴포넌트(null9 과거관측)·지지/접근/통행·입력/결과·표시/해제 소유 결손은 차단 유지. 미관측은 Conditional이지 면제 아님. 개발 Logic E1/E5·Presentation E4, 실제 공간 관측은 별도 승인.; D390 간섭/후속: 현재 대상의 통로 차단·카메라 가림·자원/상태 간섭은 가장 이른 원인 E의 차단이며 E9로 이관해 우회하지 않는다. 주변 장식의 실제 영향은 미검증이다. 데이터 전용 D386/D389는 Scene/다른 WI/Renderer를 읽거나 쓰지 않는 코드·순수 시험 범위에서만 다른 장식 완성의 비선행 근거가 있다; 실제 E5로 일반화 금지. 공간 영향 조사 후 무영향이 확인된 범위만 후속 품질/조화로 분리. 담당 개발·공간, 검토 위치 이 openGapRefs와 기술 보고.; D390 변경 영향: 후보/기준 공간·대상·Session/판본·배치/소유 계약 변경 시 해당 소비자/근거만 재검토. D389 문맥 지문 불일치는 재검사 차단, 영향 미확인은 미검증 유지. 논리와 무관한 미감 변경으로 관계없는 논리 증거를 자동 폐기하지 않으며 실제 E9는 E8 Core 둘 이상/AreaHarmonySet·사람 승인 유지.; Session·새 밭 두 주기·저장 재진입 미검증; D388: 기존4후보/Accepted 연구·과거 의존48파일을 대조해 연결·설정 보완으로 분류했다. 실제 imported 기하/지지/접근/제품 소비는 미검사이며 가공·신규 제작 필요는 미확정이다. 상태명과 정확 family 불일치·9밭 null 수리는 별개. 전체 팩 재조사·새 촬영·가공을 자동 실행하지 않는다..
  - 제한 준비 결과 d396VisualCandidatePreparation: PurePreparationAndFocusedTestsPassed_ProductUnlinked. [기술보고](../../../docs/Reports/WI전체-E4-Farm시각후보준비-D396-2026-08-31.md); 명세의 writePaths/validation 참조. 이 결과를 개별 WI 전체 또는 E 달성으로 합성하지 않는다.
  - 기존 차단: E5 Session/Save/실제 World 결속 검증 미완료. 기존 다음 작업은 자동 실행 지시가 아니다: D-320 연구 r2 추가 승인으로 Barn native 실측 수용 후보·통로 검증을 공간 담당에 분담한다. 개발은 Session/Save/자연 회복 연결을 병행하고 실제 Scene/화면은 후보 검토 후 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-03 — 농작물 생육 관리

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.farm.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 효과가 생육 진행도를 반영한다.","cancellationPolicy":"미사용 예약만 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CropProduction","WaterAccessible","WorkerAccessible"],"hRefs":["h1-stock:farm-production"],"placementVerified":false} |
| 나 | {"actorRequirements":["재배 관리 가능 행위자"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["Growing"],"resourceRequirements":["물","노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Growing 재배 단위의 생육 조건을 관리한다.","previewRule":"생육 상태와 물·노동·공간 접근을 검사한다.","confirmRule":"재배 관리 작업과 필요한 자원을 예약한다.","blockReasonCodes":["CropStateInvalid","WaterUnavailable"]} |
| 결과 | {"completionStateCodes":["Growing","HarvestReady"],"effectCodes":["CropCareApplied"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-04"],"cancellationPolicy":"미사용 예약만 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../eng/world-seedbeds/area-sets/pyeongchang-farm-hub-town.v1/spatial-capabilities.v1.json) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-136, Q-143, Q-144, Q-146, Q-147, Q-155, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-crop-cycle.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["Session·Save 영향 경로 재검증 필요"]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":[],"blockers":["실제 자산·통행·입력·Game View 미검증"]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=CEF233D99C8FCF6A588999BF85B9AA359CBEEF71C14D073830CAD423C886F6E7. 후보 상세는 -Wi -Id WI-FARM-03 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 밭갈이·파종·관리·수확 결과와 다음 파종을 판독한다.; VisualKey: farm.crop.prepare, farm.crop.plant, farm.crop.grow, farm.crop.harvest. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: Assets/Synty/PolygonFarm/Prefabs/Environments/SM_Env_Dirt_Rows_01.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_S.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab; 대체: Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Barn_02.prefab; fallback: 기존 지면+상태 Outline/UI: Synty 실물 증거 아님.
  - 배치/Anchor: D-320 추가 승인 연구 r2에 따라 Barn/Player native 비율을 보존하고 실측 회전 외곽·Player 접근 여유로 새 후보의 예약부지·마당·통로를 조정한다. r1 후보 및 거부 회귀는 보존한다. D390: approvedStudyRefs r1/r2의 적용 공간 기준을 소비하되 한 재배 대상이 소비하는 지지·접근·통행·소유 조건을 확인한다. 구후보/확대연구/현 Scene 계보 차이는 미검증이며 전체 Farm 시설 활성화를 현재 준비의 선행조건으로 추가하지 않는다. / 기존 Player의 실제 접근과 Preview/Confirm 연결. 배치 엔진은 권위를 변경하지 않는다.. 준비 상태: Conditional.
  - 열린 준비: 실측9자산·구후보16/16 예상 거부 확인. 확대 후보 생성·통로/지지면 검증·실제 Scene 결속은 미완료.; D390 필수/현재 대상: Logic E5 사본·Session/판본·상태→정확 family/제품 소비·필수 컴포넌트(null9 과거관측)·지지/접근/통행·입력/결과·표시/해제 소유 결손은 차단 유지. 미관측은 Conditional이지 면제 아님. 개발 Logic E1/E5·Presentation E4, 실제 공간 관측은 별도 승인.; D390 간섭/후속: 현재 대상의 통로 차단·카메라 가림·자원/상태 간섭은 가장 이른 원인 E의 차단이며 E9로 이관해 우회하지 않는다. 주변 장식의 실제 영향은 미검증이다. 데이터 전용 D386/D389는 Scene/다른 WI/Renderer를 읽거나 쓰지 않는 코드·순수 시험 범위에서만 다른 장식 완성의 비선행 근거가 있다; 실제 E5로 일반화 금지. 공간 영향 조사 후 무영향이 확인된 범위만 후속 품질/조화로 분리. 담당 개발·공간, 검토 위치 이 openGapRefs와 기술 보고.; D390 변경 영향: 후보/기준 공간·대상·Session/판본·배치/소유 계약 변경 시 해당 소비자/근거만 재검토. D389 문맥 지문 불일치는 재검사 차단, 영향 미확인은 미검증 유지. 논리와 무관한 미감 변경으로 관계없는 논리 증거를 자동 폐기하지 않으며 실제 E9는 E8 Core 둘 이상/AreaHarmonySet·사람 승인 유지.; Session·새 밭 두 주기·저장 재진입 미검증; D388: 기존4후보/Accepted 연구·과거 의존48파일을 대조해 연결·설정 보완으로 분류했다. 실제 imported 기하/지지/접근/제품 소비는 미검사이며 가공·신규 제작 필요는 미확정이다. 상태명과 정확 family 불일치·9밭 null 수리는 별개. 전체 팩 재조사·새 촬영·가공을 자동 실행하지 않는다..
  - 제한 준비 결과 d396VisualCandidatePreparation: PurePreparationAndFocusedTestsPassed_ProductUnlinked. [기술보고](../../../docs/Reports/WI전체-E4-Farm시각후보준비-D396-2026-08-31.md); 명세의 writePaths/validation 참조. 이 결과를 개별 WI 전체 또는 E 달성으로 합성하지 않는다.
  - 기존 차단: E5 Session/Save/실제 World 결속 검증 미완료. 기존 다음 작업은 자동 실행 지시가 아니다: D-320 연구 r2 추가 승인으로 Barn native 실측 수용 후보·통로 검증을 공간 담당에 분담한다. 개발은 Session/Save/자연 회복 연결을 병행하고 실제 Scene/화면은 후보 검토 후 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-04 — 익은 농작물 수확

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.farm-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"Player 1 Tick, NPC 2 Tick 뒤 수확 Lot을 만든다.","cancellationPolicy":"완료 전 재배 단위와 작업 영역 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CropProduction","WorkerAccessible","CargoAccessible","HarvestWorkArea"],"hRefs":["h1-stock:farm-harvest-staging","h1-stock:farm-production"],"placementVerified":false} |
| 나 | {"actorRequirements":["FarmHarvest"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["HarvestReady"],"resourceRequirements":["노동 또는 체력","수확 대상 재배 단위"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"수확 가능한 감자 재배 단위를 수확 Lot으로 전환한다.","previewRule":"100㎡·3kg/㎡와 승인된 계수를 적용해 300kg 후보를 계산하고 상태는 바꾸지 않는다.","confirmRule":"재배 단위·행위자·수확 작업 영역을 예약한다.","blockReasonCodes":["CultivationUnitNotHarvestReady","SimulationSpatialCapabilityMissing","SimulationSpatialReservationConflict"]} |
| 결과 | {"completionStateCodes":["Harvested","HarvestedAtField"],"effectCodes":["HarvestLotCreated","CultivationHarvested"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-05"],"cancellationPolicy":"완료 전 재배 단위와 작업 영역 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/Simulation감자생산Contracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/Simulation감자생산규칙.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-267, Q-268, Q-381, Q-382, Q-386. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-crop-cycle.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["Session·Save 영향 경로 재검증 필요"]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":[],"blockers":["실제 자산·통행·입력·Game View 미검증"]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=CEF233D99C8FCF6A588999BF85B9AA359CBEEF71C14D073830CAD423C886F6E7. 후보 상세는 -Wi -Id WI-FARM-04 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 밭갈이·파종·관리·수확 결과와 다음 파종을 판독한다.; VisualKey: farm.crop.prepare, farm.crop.plant, farm.crop.grow, farm.crop.harvest. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: Assets/Synty/PolygonFarm/Prefabs/Environments/SM_Env_Dirt_Rows_01.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_S.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab, Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab; 대체: Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Barn_02.prefab; fallback: 기존 지면+상태 Outline/UI: Synty 실물 증거 아님.
  - 배치/Anchor: D-320 추가 승인 연구 r2에 따라 Barn/Player native 비율을 보존하고 실측 회전 외곽·Player 접근 여유로 새 후보의 예약부지·마당·통로를 조정한다. r1 후보 및 거부 회귀는 보존한다. D390: approvedStudyRefs r1/r2의 적용 공간 기준을 소비하되 한 재배 대상이 소비하는 지지·접근·통행·소유 조건을 확인한다. 구후보/확대연구/현 Scene 계보 차이는 미검증이며 전체 Farm 시설 활성화를 현재 준비의 선행조건으로 추가하지 않는다. / 기존 Player의 실제 접근과 Preview/Confirm 연결. 배치 엔진은 권위를 변경하지 않는다.. 준비 상태: Conditional.
  - 열린 준비: 실측9자산·구후보16/16 예상 거부 확인. 확대 후보 생성·통로/지지면 검증·실제 Scene 결속은 미완료.; D390 필수/현재 대상: Logic E5 사본·Session/판본·상태→정확 family/제품 소비·필수 컴포넌트(null9 과거관측)·지지/접근/통행·입력/결과·표시/해제 소유 결손은 차단 유지. 미관측은 Conditional이지 면제 아님. 개발 Logic E1/E5·Presentation E4, 실제 공간 관측은 별도 승인.; D390 간섭/후속: 현재 대상의 통로 차단·카메라 가림·자원/상태 간섭은 가장 이른 원인 E의 차단이며 E9로 이관해 우회하지 않는다. 주변 장식의 실제 영향은 미검증이다. 데이터 전용 D386/D389는 Scene/다른 WI/Renderer를 읽거나 쓰지 않는 코드·순수 시험 범위에서만 다른 장식 완성의 비선행 근거가 있다; 실제 E5로 일반화 금지. 공간 영향 조사 후 무영향이 확인된 범위만 후속 품질/조화로 분리. 담당 개발·공간, 검토 위치 이 openGapRefs와 기술 보고.; D390 변경 영향: 후보/기준 공간·대상·Session/판본·배치/소유 계약 변경 시 해당 소비자/근거만 재검토. D389 문맥 지문 불일치는 재검사 차단, 영향 미확인은 미검증 유지. 논리와 무관한 미감 변경으로 관계없는 논리 증거를 자동 폐기하지 않으며 실제 E9는 E8 Core 둘 이상/AreaHarmonySet·사람 승인 유지.; Session·새 밭 두 주기·저장 재진입 미검증; D388: 기존4후보/Accepted 연구·과거 의존48파일을 대조해 연결·설정 보완으로 분류했다. 실제 imported 기하/지지/접근/제품 소비는 미검사이며 가공·신규 제작 필요는 미확정이다. 상태명과 정확 family 불일치·9밭 null 수리는 별개. 전체 팩 재조사·새 촬영·가공을 자동 실행하지 않는다..
  - 제한 준비 결과 d396VisualCandidatePreparation: PurePreparationAndFocusedTestsPassed_ProductUnlinked. [기술보고](../../../docs/Reports/WI전체-E4-Farm시각후보준비-D396-2026-08-31.md); 명세의 writePaths/validation 참조. 이 결과를 개별 WI 전체 또는 E 달성으로 합성하지 않는다.
  - 기존 차단: E5 Session/Save/실제 World 결속 검증 미완료. 기존 다음 작업은 자동 실행 지시가 아니다: D-320 연구 r2 추가 승인으로 Barn native 실측 수용 후보·통로 검증을 공간 담당에 분담한다. 개발은 Session/Save/자연 회복 연결을 병행하고 실제 Scene/화면은 후보 검토 후 검증한다.

- playable-loop:nature-farm-roundtrip.v1 / WorldAggregate / 통합 E1, 궤적 null. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: Nature와 Farm 독립 영역 집계가 모두 E7 PlayClosed가 아니다.. 기존 다음 작업은 자동 실행 지시가 아니다: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-05 — 수확물 집하장 모으기

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.farm-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 Tick에 Lot 위치 상태를 CollectedAtYard로 바꾼다.","cancellationPolicy":"완료 전 Lot·작업 영역 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CollectionWorkArea","WorkerAccessible","CargoAccessible"],"hRefs":["h1-stock:farm-harvest-staging","h1-stock:farm-work-yard"],"placementVerified":false} |
| 나 | {"actorRequirements":["FarmCollection"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["HarvestedAtField"],"resourceRequirements":["수확 Lot","노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"밭의 수확 Lot을 농장 집하장으로 옮긴다.","previewRule":"Lot 상태·행위자·집하 공간을 검사한다.","confirmRule":"Lot과 집하 작업 영역을 예약한다.","blockReasonCodes":["HarvestLotStateInvalid","SimulationSpatialReservationConflict"]} |
| 결과 | {"completionStateCodes":["CollectedAtYard"],"effectCodes":["HarvestLotCollected"]} |
| 다음 선택 | {"successorWiIds":["WI-FARM-06"],"cancellationPolicy":"완료 전 Lot·작업 영역 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-pack-store-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["내부 보관·재생산 반환 상태 계약과 E5 Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["집하·포장·보관·다음 생산 반환의 Runtime 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-07~09 예약 번호를 재사용하지 않는 내부 보관·반환 상태 계약과 E5 Fixture가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 새 WI를 성급히 만들지 않고 WI-FARM-06 완료 결과에 내부 보관·재생산 반환 계약을 먼저 확정한다.

- playable-loop:nature-farm-roundtrip.v1 / WorldAggregate / 통합 E1, 궤적 null. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: Nature와 Farm 독립 영역 집계가 모두 E7 PlayClosed가 아니다.. 기존 다음 작업은 자동 실행 지시가 아니다: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-FARM-06 — 출하 물량 포장

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.farm-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 Tick에 PackageLot과 Cargo를 동일 수량으로 만든다.","cancellationPolicy":"완료 전 Lot·작업 영역 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["PackingWorkArea","WorkerAccessible","CargoAccessible"],"hRefs":["h1-stock:farm-work-yard"],"placementVerified":false} |
| 나 | {"actorRequirements":["FarmPacking"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["CollectedAtYard"],"resourceRequirements":["수확 Lot","포장 노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"집하된 수확 Lot을 포장 Lot과 운송 Cargo로 만든다.","previewRule":"Lot 상태·수량·포장 공간을 검사하고 Cargo 계보 후보를 제시한다.","confirmRule":"Lot과 포장 작업 영역을 예약한다.","blockReasonCodes":["HarvestLotStateInvalid","PackingWorkAreaUnavailable"]} |
| 결과 | {"completionStateCodes":["PackedForShipment","PreparedForShipment"],"effectCodes":["PackageLotCreated","CargoPrepared"]} |
| 다음 선택 | {"successorWiIds":["WI-LOG-01"],"cancellationPolicy":"완료 전 Lot·작업 영역 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Contracts/SimulationLogisticsMovementContracts.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-pack-store-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["내부 보관·재생산 반환 상태 계약과 E5 Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["집하·포장·보관·다음 생산 반환의 Runtime 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-FARM-07~09 예약 번호를 재사용하지 않는 내부 보관·반환 상태 계약과 E5 Fixture가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 새 WI를 성급히 만들지 않고 WI-FARM-06 완료 결과에 내부 보관·재생산 반환 계약을 먼저 확정한다.

- playable-loop:nature-farm-roundtrip.v1 / WorldAggregate / 통합 E1, 궤적 null. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: Nature와 Farm 독립 영역 집계가 모두 E7 PlayClosed가 아니다.. 기존 다음 작업은 자동 실행 지시가 아니다: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-LOG-01 — 출하 차량 상차 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.logistics.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"확정된 운송 작업이 후속 자동 전이를 소유한다.","cancellationPolicy":"출발 전 모든 예약을 계보로 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["VehicleAccessible","CargoAccessible","LoadingWorkArea"],"hRefs":["h1-stock:farm-loading-gate"],"placementVerified":false} |
| 나 | {"actorRequirements":["운송 담당 행위자"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["PreparedForShipment"],"resourceRequirements":["Cargo","차량 용량"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Farm 출하 Cargo를 차량과 Farm 상차 공간에 예약한다.","previewRule":"Cargo·차량 용량·원점 상차 공간·경로·도착 하차 공간을 함께 검사한다.","confirmRule":"하나의 물류 결정으로 Cargo·차량·공간을 예약한다.","blockReasonCodes":["FreightVehicleCapacityExceeded","SimulationSpatialReservationConflict"]} |
| 결과 | {"completionStateCodes":["Reserved"],"effectCodes":["CargoTransportReserved"]} |
| 다음 선택 | {"successorWiIds":["WI-LOG-02"],"cancellationPolicy":"출발 전 모든 예약을 계보로 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFreightTransport.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationLogisticsMovement.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-LOG-02 — 농장에서 출발

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.logistics.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"ScheduledStartTick 도달 시 결정적으로 전이한다.","cancellationPolicy":"출발 이후 취소 규칙은 부모 운송 작업이 소유한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["OriginLoading"],"hRefs":["h1-stock:farm-loading-gate"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["Reserved"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"상차 예약된 Cargo가 운송 시작 Tick에 Farm을 출발한다.","previewRule":"독립 Preview가 없고 WI-LOG-01 Preview 결과를 사용한다.","confirmRule":"독립 Confirm이 없고 WI-LOG-01 Command 계보를 사용한다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["InTransit"],"effectCodes":["CargoDeparted"]} |
| 다음 선택 | {"successorWiIds":["WI-LOG-03"],"cancellationPolicy":"출발 이후 취소 규칙은 부모 운송 작업이 소유한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationLogisticsMovement.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-LOG-03 — 농장에서 물류 거점으로 화물 이동

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.logistics.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"WorldTick에 따라 CompletedRouteTicks를 결정적으로 갱신한다.","cancellationPolicy":"부모 운송 작업 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CargoRoute"],"hRefs":["h1-stock:farm-hub-corridor"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["InTransit"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Cargo가 승인된 Farm–Hub 경로를 따라 이동 진행도를 갱신한다.","previewRule":"독립 Preview 없이 부모 명령에서 경로 접근을 검사한다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["InTransit","ArrivedAtDestination"],"effectCodes":["CargoRouteProgressed"]} |
| 다음 선택 | {"successorWiIds":["WI-LOG-04"],"cancellationPolicy":"부모 운송 작업 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationLogisticsMovement.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-LOG-04 — 물류 거점 도착 화물 하차

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.logistics.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"ExpectedEndTick에 도착·하차 상태로 전이한다.","cancellationPolicy":"부모 운송 작업 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["UnloadingWorkArea","CargoAccessible"],"hRefs":["h1-stock:hub-receiving-storage"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["InTransit"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"도착 Cargo가 Hub 하차 작업 영역을 사용하고 도착 후보가 된다.","previewRule":"부모 Preview가 도착 공간을 선검사한다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["ArrivedAtDestination"],"effectCodes":["CargoUnloaded"]} |
| 다음 선택 | {"successorWiIds":["WI-LOG-05"],"cancellationPolicy":"부모 운송 작업 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFreightTransport.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-LOG-05 — 물류 거점 도착 화물 인수

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.hub.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"WI-001 Task 완료 시 같은 계보로 전이한다.","cancellationPolicy":"WI-001 취소 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["InspectionWorkArea"],"hRefs":["h1-stock:hub-receiving-storage"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["ArrivedAtDestination","PendingInspection"],"resourceRequirements":["도착 Cargo"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Hub 입고검수 완료가 운송 Cargo의 인수 완료를 함께 확정한다.","previewRule":"독립 Preview가 없고 WI-001 입고검수 Preview에 포함된다.","confirmRule":"독립 Confirm이 없고 WI-001 Command에 포함된다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["Received","StorageEligible"],"effectCodes":["FreightReceived","StorageEligible"]} |
| 다음 선택 | {"successorWiIds":["WI-001"],"cancellationPolicy":"WI-001 취소 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFreightTransport.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-001 — 입고 화물 검수

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.hub.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"배정→이동→작업→완료 상태를 서버 Tick이 확정한다.","cancellationPolicy":"이 Task가 만든 공간·NPC·임시 재고만 계보로 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CargoAccessible","WorkerAccessible","InspectionWorkArea"],"hRefs":["h1-stock:hub-receiving-storage","h1-stock:hub-temporary-staging"],"placementVerified":false} |
| 나 | {"actorRequirements":["WarehouseInspection"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["ArrivedAtDestination","PendingInspection"],"resourceRequirements":["도착 Cargo","NPC 노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"도착 Cargo를 적격 NPC와 검수 공간에서 검수한다.","previewRule":"Cargo·NPC·검수 공간·개정·예약 충돌을 검사하며 상태는 바꾸지 않는다.","confirmRule":"Decision·Task·NPC 배정·작업 영역 예약을 원자적으로 만든다.","blockReasonCodes":["SimulationSpatialCapabilityMissing","SimulationSpatialReservationConflict"]} |
| 결과 | {"completionStateCodes":["StorageEligible","Received"],"effectCodes":["InspectionWorkAreaReleased","StorageEligible","FreightReceived"]} |
| 다음 선택 | {"successorWiIds":["WI-002"],"cancellationPolicy":"이 Task가 만든 공간·NPC·임시 재고만 계보로 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/Simulation공간상호작용.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFreightTransport.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- [원문/소스](../../../Ssalddel.Simulation.Tests/Simulation공간상호작용Tests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:hub-inbound-putaway.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E2","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":[],"blockers":["독립 Fixture·Save/Replay·결정적 적치 시험이 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["입고·검수·적치의 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 다른 영역 화물과 무관한 Fixture·Save/Replay·결정적 적치 시험이 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: Hub 자체 입고 Lot과 용량·검수 실패·재선택 계약을 E3로 구현한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-002 — 검수 완료 화물 창고 적재

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.hub.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 Tick에 예약 용량을 실제 점유량으로 전환한다.","cancellationPolicy":"완료 전 보관 용량·작업 영역·NPC 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Storage","CargoAccessible","WorkerAccessible","LoadingWorkArea"],"hRefs":["h1-stock:hub-long-term-storage","h1-stock:hub-receiving-storage"],"placementVerified":false} |
| 나 | {"actorRequirements":["WarehousePutAway"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["StorageEligible"],"resourceRequirements":["StorageEligible 재고","NPC 노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"검수 완료 재고를 창고 용량과 적재 작업 영역을 사용해 적재한다.","previewRule":"남은 보관 용량과 작업 영역·행위자·개정을 검사한다.","confirmRule":"재고 수량만큼 보관 용량과 작업 영역을 원자적으로 예약한다.","blockReasonCodes":["SimulationSpatialCapacityInsufficient","SimulationSpatialReservationConflict"]} |
| 결과 | {"completionStateCodes":["PutAwayCompleted"],"effectCodes":["SpatialStorageCapacityConsumed","SpatialWorkAreaReleased","PutAwayCompleted"]} |
| 다음 선택 | {"successorWiIds":["WI-HUB-03"],"cancellationPolicy":"완료 전 보관 용량·작업 영역·NPC 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/Simulation공간상호작용.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationWarehousePutAway.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- [원문/소스](../../../Ssalddel.Simulation.Tests/Simulation공간상호작용Tests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:hub-inbound-putaway.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E2","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":[],"blockers":["독립 Fixture·Save/Replay·결정적 적치 시험이 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["입고·검수·적치의 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 다른 영역 화물과 무관한 Fixture·Save/Replay·결정적 적치 시험이 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: Hub 자체 입고 Lot과 용량·검수 실패·재선택 계약을 E3로 구현한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HUB-03 — 출고 대상 재고 요청

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.hub.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"후속 피킹 자동 전이의 부모 작업이다.","cancellationPolicy":"피킹 전 재고 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Storage","WorkerAccessible"],"hRefs":["h1-stock:hub-outbound-staging"],"placementVerified":false} |
| 나 | {"actorRequirements":["출고 권한 행위자"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["PutAwayCompleted"],"resourceRequirements":["창고 재고"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"창고 재고에 대해 목적지 출고를 요청한다.","previewRule":"재고·목적지·권한을 검사한다.","confirmRule":"출고 결정과 대상 재고 예약을 만든다.","blockReasonCodes":["WarehouseStockInsufficient"]} |
| 결과 | {"completionStateCodes":["OutboundRequested"],"effectCodes":["OutboundRequested"]} |
| 다음 선택 | {"successorWiIds":["WI-HUB-04"],"cancellationPolicy":"피킹 전 재고 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/NPC루틴WI통제정책.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNpcRoutineWork.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Server/Controllers/경영Simulation물류창고Controller.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-240, Q-241, Q-242, Q-243, Q-244, Q-245, Q-246, Q-247, Q-248, Q-249, Q-250. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:hub-outbound-ready-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:hub-npc-routine-core-20260825"],"blockers":["H 공간 결속과 실제 WI 발현이 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["플레이어 정책 개입과 NPC 업무 표현의 Game View 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: PickingWorkArea·OutboundStagingArea의 승인 H 공간 결속과 실제 WI Manifestation이 없다.; SimulationWorldShell의 플레이어 정책 개입·NPC 업무 표현과 Game View 증거가 없다.; 다른 안정 Core와의 E9 NPC 생활 조화는 현재 범위 밖이다.. 기존 다음 작업은 자동 실행 지시가 아니다: Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HUB-04 — 출고 대상 재고 피킹

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.hub.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"부모 Task 진행 중 자동 상태로 기록한다.","cancellationPolicy":"부모 작업 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Storage","WorkerAccessible","PickingWorkArea"],"hRefs":["h1-stock:hub-outbound-staging","h1-stock:hub-temporary-staging"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["OutboundRequested"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"출고 요청 재고를 창고에서 피킹한다.","previewRule":"부모 출고 요청이 피킹 가능성을 검사한다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["Picked"],"effectCodes":["StockPicked"]} |
| 다음 선택 | {"successorWiIds":["WI-HUB-05"],"cancellationPolicy":"부모 작업 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/NPC루틴WI통제정책.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNpcRoutineWork.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationSupplyChainWork.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:hub-outbound-ready-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:hub-npc-routine-core-20260825"],"blockers":["H 공간 결속과 실제 WI 발현이 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["플레이어 정책 개입과 NPC 업무 표현의 Game View 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: PickingWorkArea·OutboundStagingArea의 승인 H 공간 결속과 실제 WI Manifestation이 없다.; SimulationWorldShell의 플레이어 정책 개입·NPC 업무 표현과 Game View 증거가 없다.; 다른 안정 Core와의 E9 NPC 생활 조화는 현재 범위 밖이다.. 기존 다음 작업은 자동 실행 지시가 아니다: Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HUB-05 — 피킹 화물 포장

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.hub.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"피킹 뒤 결정적으로 출고 준비 상태가 된다.","cancellationPolicy":"부모 작업 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["PackingWorkArea","CargoAccessible"],"hRefs":["h1-stock:hub-outbound-staging","h1-stock:hub-temporary-staging"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["Picked"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"피킹 재고를 출고 가능한 Cargo 후보로 만든다.","previewRule":"부모 출고 요청 Preview에 포함된다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["OutboundReady"],"effectCodes":["OutboundCargoPrepared"]} |
| 다음 선택 | {"successorWiIds":["WI-HUB-06"],"cancellationPolicy":"부모 작업 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/NPC루틴WI통제정책.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNpcRoutineWork.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationSupplyChainWork.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:hub-outbound-ready-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:hub-npc-routine-core-20260825"],"blockers":["H 공간 결속과 실제 WI 발현이 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["플레이어 정책 개입과 NPC 업무 표현의 Game View 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: PickingWorkArea·OutboundStagingArea의 승인 H 공간 결속과 실제 WI Manifestation이 없다.; SimulationWorldShell의 플레이어 정책 개입·NPC 업무 표현과 Game View 증거가 없다.; 다른 안정 Core와의 E9 NPC 생활 조화는 현재 범위 밖이다.. 기존 다음 작업은 자동 실행 지시가 아니다: Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-HUB-06 — 출고 차량 상차

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.market-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"Hub→Town 운송의 선행 작업이다.","cancellationPolicy":"출발 전 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["LoadingWorkArea","VehicleAccessible","CargoAccessible"],"hRefs":["h1-stock:hub-vehicle-yard"],"placementVerified":false} |
| 나 | {"actorRequirements":["운송 담당 행위자"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["OutboundReady"],"resourceRequirements":["Cargo","차량"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"출고 준비 Cargo를 Town행 차량에 상차한다.","previewRule":"Cargo·차량·Hub 상차 공간을 검사한다.","confirmRule":"운송 예약을 원자적으로 만든다.","blockReasonCodes":["FreightVehicleCapacityExceeded"]} |
| 결과 | {"completionStateCodes":["Reserved"],"effectCodes":["HubCargoLoaded"]} |
| 다음 선택 | {"successorWiIds":["WI-MARKET-01"],"cancellationPolicy":"출발 전 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFreightTransport.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-MARKET-01 — 물류 거점에서 마트로 운송

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.market-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"Tick 기반 운송 하위 상태를 진행한다.","cancellationPolicy":"운송 상태에 따른 부모 취소 규칙을 사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CargoRoute","VehicleAccessible"],"hRefs":["h1-stock:hub-market-transfer","h1-stock:hub-town-corridor","h1-stock:hub-vehicle-yard"],"placementVerified":false} |
| 나 | {"actorRequirements":["운송 담당 행위자"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["Reserved"],"resourceRequirements":["Cargo","차량"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"Hub 출고 Cargo를 Town 마트로 운송한다.","previewRule":"경로·차량·목적지 접근을 검사한다.","confirmRule":"운송 작업을 확정한다.","blockReasonCodes":["CargoRouteUnavailable"]} |
| 결과 | {"completionStateCodes":["ArrivedAtDestination"],"effectCodes":["MarketCargoArrived"]} |
| 다음 선택 | {"successorWiIds":["WI-MARKET-02"],"cancellationPolicy":"운송 상태에 따른 부모 취소 규칙을 사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFreightTransport.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-MARKET-02 — 마트 도착 화물 인수

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.market-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 Tick에 마트 인수 상태가 된다.","cancellationPolicy":"완료 전 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["UnloadingWorkArea","CargoAccessible","WorkerAccessible"],"hRefs":["h1-stock:hub-market-transfer","h1-stock:town-market-receiving"],"placementVerified":false} |
| 나 | {"actorRequirements":["마트 입고 담당"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["ArrivedAtDestination"],"resourceRequirements":["도착 Cargo"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"도착 Cargo를 마트 하차 공간에서 인수한다.","previewRule":"도착 상태·담당자·하차 공간을 검사한다.","confirmRule":"인수 작업과 공간을 예약한다.","blockReasonCodes":["MarketReceivingUnavailable"]} |
| 결과 | {"completionStateCodes":["MarketReceived"],"effectCodes":["MarketFreightReceived"]} |
| 다음 선택 | {"successorWiIds":["WI-MARKET-03"],"cancellationPolicy":"완료 전 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-MARKET-03 — 마트 입고 상품 검수

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.market-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 시 후방 적재 가능 상태가 된다.","cancellationPolicy":"완료 전 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["InspectionWorkArea","WorkerAccessible"],"hRefs":["h1-stock:town-market-receiving"],"placementVerified":false} |
| 나 | {"actorRequirements":["마트 검수 담당"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["MarketReceived"],"resourceRequirements":["마트 인수 재고"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"마트 인수 재고의 품질과 수량을 검수한다.","previewRule":"재고·담당자·검수 공간을 검사한다.","confirmRule":"검수 작업을 확정한다.","blockReasonCodes":["MarketInventoryStateInvalid"]} |
| 결과 | {"completionStateCodes":["MarketStorageEligible"],"effectCodes":["MarketStorageEligible"]} |
| 다음 선택 | {"successorWiIds":["WI-MARKET-04"],"cancellationPolicy":"완료 전 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-MARKET-04 — 검수 상품 후방 창고 적재

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.market-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 시 후방 점유량이 증가한다.","cancellationPolicy":"완료 전 용량 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Storage","LoadingWorkArea","WorkerAccessible"],"hRefs":["h1-stock:town-market-receiving"],"placementVerified":false} |
| 나 | {"actorRequirements":["마트 적재 담당"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["MarketStorageEligible"],"resourceRequirements":["마트 적재 가능 재고"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"검수 완료 마트 재고를 후방 창고에 적재한다.","previewRule":"후방 용량과 작업 영역을 검사한다.","confirmRule":"용량·작업 영역·행위자를 예약한다.","blockReasonCodes":["SimulationSpatialCapacityInsufficient"]} |
| 결과 | {"completionStateCodes":["MarketBackroomStored"],"effectCodes":["MarketBackroomStored"]} |
| 다음 선택 | {"successorWiIds":["WI-MARKET-05"],"cancellationPolicy":"완료 전 용량 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-MARKET-05 — 매장 진열대 상품 보충

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.market-supply.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 시 판매 가능한 진열 재고가 된다.","cancellationPolicy":"완료 전 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["DisplayArea","CustomerAccessible","WorkerAccessible"],"hRefs":["h1-stock:town-market-display"],"placementVerified":false} |
| 나 | {"actorRequirements":["진열 담당"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["MarketBackroomStored"],"resourceRequirements":["후방 재고"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"후방 재고를 고객 진열 공간으로 옮긴다.","previewRule":"후방 재고와 진열 용량을 검사한다.","confirmRule":"진열 공간과 재고를 예약한다.","blockReasonCodes":["DisplayCapacityInsufficient"]} |
| 결과 | {"completionStateCodes":["Displayed"],"effectCodes":["DisplayStockReplenished"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-01"],"cancellationPolicy":"완료 전 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-01 — 주민 주문 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"주문 예약 자동 전이의 부모 원인이다.","cancellationPolicy":"예약·이행 단계에 따라 주문 취소 규칙을 적용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["주문자"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["DemandCandidate"],"resourceRequirements":["상품·수량·가격 후보"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"주민의 수요 후보를 명시적 주문으로 확정한다.","previewRule":"수요와 주문을 구분하고 주문 조건을 검사한다.","confirmRule":"명시적 주문만 생성한다.","blockReasonCodes":["OrderTermsInvalid"]} |
| 결과 | {"completionStateCodes":["OrderConfirmed"],"effectCodes":["OrderConfirmed"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-02"],"cancellationPolicy":"예약·이행 단계에 따라 주문 취소 규칙을 적용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationIndividualOrder.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-02 — 주문 상품 재고 예약

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"독립 Task가 아니다.","cancellationPolicy":"주문 취소 시 미이행 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["OrderConfirmed"],"resourceRequirements":["판매 재고"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"확정 주문 수량을 시장 재고에서 예약한다.","previewRule":"주문 Preview에서 재고 가능성을 계산한다.","confirmRule":"주문 Confirm과 같은 원자적 변경으로 예약한다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["StockReserved"],"effectCodes":["OrderStockReserved"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-03"],"cancellationPolicy":"주문 취소 시 미이행 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationIndividualOrder.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-03 — 주문 상품 피킹

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"주문 이행 Task의 하위 상태다.","cancellationPolicy":"부모 주문 이행 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["PickingWorkArea"],"hRefs":["h1-stock:town-market-display"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["StockReserved"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"예약 재고를 주문별로 피킹한다.","previewRule":"부모 주문 이행 Preview에 포함된다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["Picked"],"effectCodes":["OrderStockPicked"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-04"],"cancellationPolicy":"부모 주문 이행 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-04 — 주문 상품 포장

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"주문 이행 Task의 하위 상태다.","cancellationPolicy":"부모 주문 이행 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["PackingWorkArea"],"hRefs":["h1-stock:town-order-packing"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["Picked"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"피킹된 주문 재고를 수령 단위로 포장한다.","previewRule":"부모 주문 이행 Preview에 포함된다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["Packed"],"effectCodes":["OrderPacked"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-05"],"cancellationPolicy":"부모 주문 이행 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-05 — 주문 상품 수령 준비

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"포장 완료 뒤 자동 전이한다.","cancellationPolicy":"부모 주문 상태 규칙을 따른다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CustomerAccessible"],"hRefs":["h1-stock:town-resident-pickup"],"placementVerified":false} |
| 나 | {"actorRequirements":[],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["Packed"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"포장 주문을 주민 수령 가능 상태로 만든다.","previewRule":"부모 주문 이행 Preview에 포함된다.","confirmRule":"독립 Confirm이 없다.","blockReasonCodes":[]} |
| 결과 | {"completionStateCodes":["ReadyForPickup"],"effectCodes":["OrderReadyForPickup"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-06"],"cancellationPolicy":"부모 주문 상태 규칙을 따른다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationIndividualOrder.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-06 — 주민 주문 상품 수령

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 시 주문 이행 상태가 된다.","cancellationPolicy":"수령 확정 전 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CustomerAccessible","PickupArea"],"hRefs":["h1-stock:town-resident-pickup"],"placementVerified":false} |
| 나 | {"actorRequirements":["주문자 또는 승인된 수령자"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["ReadyForPickup"],"resourceRequirements":["수령 준비 주문"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"주민이 준비된 주문을 수령한다.","previewRule":"수령자·주문·수령 공간을 검사한다.","confirmRule":"수령 행위를 명시적으로 확정한다.","blockReasonCodes":["OrderNotReadyForPickup"]} |
| 결과 | {"completionStateCodes":["Fulfilled"],"effectCodes":["OrderFulfilled"]} |
| 다음 선택 | {"successorWiIds":["WI-ORDER-07"],"cancellationPolicy":"수령 확정 전 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationIndividualOrder.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-135, Q-137, Q-138, Q-145, Q-148, Q-151, Q-152, Q-154, Q-158, Q-159, Q-160. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-ORDER-07 — 주민 상품 소비

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.order.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 효과는 주민 상태만 바꾸고 시장 재고를 재차감하지 않는다.","cancellationPolicy":"소비 완료 뒤 취소하지 않는다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":["h1-stock:town-living-square"],"placementVerified":false} |
| 나 | {"actorRequirements":["주문 수령 주민"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["Fulfilled"],"resourceRequirements":["주민 보유 상품"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"주민이 이미 이행된 주문 상품을 소비 상태로 전환한다.","previewRule":"이행 계보와 중복 소비 여부를 검사한다.","confirmRule":"소비 행위를 확정한다.","blockReasonCodes":["OrderNotFulfilled","ConsumptionAlreadyApplied"]} |
| 결과 | {"completionStateCodes":["Consumed"],"effectCodes":["ResidentConsumed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"소비 완료 뒤 취소하지 않는다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationMarketConsumption.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-order-consume-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["욕구·경쟁·소비·다음 목표의 E5 Fixture가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["주문·수령·소비·다음 욕구의 화면 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-01 — 자연 지역 위험 징후 확인

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.nature.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"관찰 작업은 경로별 위협 사본과 원인 계보를 기록하고 후퇴 또는 복원 판단을 연다.","cancellationPolicy":"완료 전 관찰 공간과 행위자 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","ObservationArea","ThreatMonitoringArea"],"hRefs":["h1-stock:nature-incident-trace","h1-stock:nature-threat-watch"],"placementVerified":false} |
| 나 | {"actorRequirements":["탐사 또는 관찰 가능 행위자"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Stable","Warning","Threatened","Infested"],"resourceRequirements":["현재 자연권 위협 상태 사본"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 전문 경관에서 번진 자연권 위협의 경로와 현재 압력을 관찰한다.","previewRule":"자연권 위협 경로·관찰 공간·행위자와 현재 개정을 검사하고 관찰 가능한 경고만 제시한다.","confirmRule":"관찰 대상 경로를 명시적으로 확정하되 압력이나 사건 결과를 변경하지 않는다.","blockReasonCodes":["NatureThreatRouteUnavailable","NatureObservationAreaUnavailable"]} |
| 결과 | {"completionStateCodes":["ThreatObserved"],"effectCodes":["NatureThreatObserved"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-02","WI-NATURE-03","WI-NATURE-11"],"cancellationPolicy":"완료 전 관찰 공간과 행위자 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/지역사건-자연권위협-규칙.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationNatureThreatObservation.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationRegionalIncidents.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationRegionalIncidentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-385. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-twilight-return.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-dual-combat-core-20260825","evidence:nature-twilight-wi11-hosted-parity-20260826"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-dual-combat-unity-editmode-20260825","evidence:nature-twilight-wi11-playmode-20260826","evidence:nature-dual-loop-game-view-20260826"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: 완료 상태를 유지하되 H3 ThreatInput·전투 상태·Skeleton 표현 계약 변경 시 논리와 표현 증거를 각각 재검증한다.

- playable-loop:nature-regional-threat-recovery.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-regional-threat-core-20260826"],"blockers":["실제 Nature 경관 Graph와 귀환·회복 입력 결속이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["위협·후퇴·복원·회복의 표현 계약만 있고 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-NATURE-02~04의 Scenario 공간을 실제 Nature 경관 Graph와 canonical SimulationWorldShell 귀환·회복 입력으로 닫지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: Nature 핵심 첫날 폐루프와 분리된 Extension Goal로 선택될 때 WI-NATURE-02부터 E4→E7을 진행한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-02 — 안전 거점으로 긴급 후퇴

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.nature.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"후퇴 작업은 안전 생활핵 도달 상태를 만들지만 원인 사건이나 위협 압력을 해결하지 않는다.","cancellationPolicy":"후퇴 시작 전 경로와 행위자 예약을 반환하고 시작 뒤에는 별도 중단 규칙을 사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","EmergencyAccess","PlayerEscapeRoute"],"hRefs":["h1-stock:nature-emergency-retreat"],"placementVerified":false} |
| 나 | {"actorRequirements":["후퇴 가능한 플레이어 또는 파티"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["ThreatObserved","EncounterActive"],"resourceRequirements":["유효한 안전 생활핵 연결"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어와 동료가 조우 위험대에서 경계 완충대를 거쳐 안전 생활핵으로 후퇴한다.","previewRule":"현재 조우·후퇴 경로·안전 생활핵 연결과 예약 충돌을 검사한다.","confirmRule":"선택한 후퇴 경로와 파티를 명시적으로 확정한다.","blockReasonCodes":["NatureEmergencyRouteUnavailable","NatureSafeCoreUnavailable"]} |
| 결과 | {"completionStateCodes":["RetreatedToSafeCore"],"effectCodes":["PartyRetreatedToSafeCore"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-04"],"cancellationPolicy":"후퇴 시작 전 경로와 행위자 예약을 반환하고 시작 뒤에는 별도 중단 규칙을 사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/지역사건-자연권위협-규칙.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationNatureEmergencyRetreat.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationRegionalIncidentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-regional-threat-recovery.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-regional-threat-core-20260826"],"blockers":["실제 Nature 경관 Graph와 귀환·회복 입력 결속이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["위협·후퇴·복원·회복의 표현 계약만 있고 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-NATURE-02~04의 Scenario 공간을 실제 Nature 경관 Graph와 canonical SimulationWorldShell 귀환·회복 입력으로 닫지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: Nature 핵심 첫날 폐루프와 분리된 Extension Goal로 선택될 때 WI-NATURE-02부터 E4→E7을 진행한다.

- playable-loop:nature-farm-roundtrip.v1 / WorldAggregate / 통합 E1, 궤적 null. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: Nature와 Farm 독립 영역 집계가 모두 E7 PlayClosed가 아니다.. 기존 다음 작업은 자동 실행 지시가 아니다: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-03 — 훼손된 자연 경로 복원

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.nature.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 효과만 해당 경로의 복원 상태를 갱신하며 다른 경로 압력을 변경하지 않는다.","cancellationPolicy":"미사용 자재와 작업 공간·행위자 예약을 원인 계보별로 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["WorkerAccessible","RestorationWorkArea","CargoAccessible"],"hRefs":["h1-stock:nature-restoration-site"],"placementVerified":false} |
| 나 | {"actorRequirements":["복원 작업 가능 행위자"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["ThreatObserved","CauseResolved"],"resourceRequirements":["해결된 원인 계보","복원 자재"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"원인 사건이 해결된 자연권 경로에서 정화·복구 작업을 수행한다.","previewRule":"원인 해결 계보·남은 압력·복원 공간·자재·행위자와 현재 개정을 검사한다.","confirmRule":"복원 작업과 필요한 자재·공간을 원자적으로 예약한다.","blockReasonCodes":["NatureIncidentCauseUnresolved","NatureRestorationMaterialInsufficient","NatureRestorationAreaUnavailable"]} |
| 결과 | {"completionStateCodes":["NatureRouteRestored"],"effectCodes":["NatureRouteRestored"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-04"],"cancellationPolicy":"미사용 자재와 작업 공간·행위자 예약을 원인 계보별로 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/지역사건-자연권위협-규칙.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationNatureRestoration.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationRegionalIncidents.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationRegionalIncidentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-385. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-regional-threat-recovery.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-regional-threat-core-20260826"],"blockers":["실제 Nature 경관 Graph와 귀환·회복 입력 결속이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["위협·후퇴·복원·회복의 표현 계약만 있고 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-NATURE-02~04의 Scenario 공간을 실제 Nature 경관 Graph와 canonical SimulationWorldShell 귀환·회복 입력으로 닫지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: Nature 핵심 첫날 폐루프와 분리된 Extension Goal로 선택될 때 WI-NATURE-02부터 E4→E7을 진행한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-04 — 탐사대 안전 회복

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.nature.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"회복 작업은 파티 준비 상태만 갱신하고 지역 사건이나 자연권 압력을 자동 변경하지 않는다.","cancellationPolicy":"완료 전 회복 공간과 미사용 보급 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","RestArea","SafeCore"],"hRefs":["h1-stock:nature-safe-recovery-camp"],"placementVerified":false} |
| 나 | {"actorRequirements":["회복 대상 플레이어 또는 파티"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["RetreatedToSafeCore","NatureRouteRestored"],"resourceRequirements":["회복 시간 또는 보급"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"안전 생활핵에서 플레이어와 동료가 위협 대응 뒤 다음 행동을 준비한다.","previewRule":"안전 생활핵·회복 공간·파티 상태·보급과 현재 예약을 검사한다.","confirmRule":"회복 대상과 공간을 명시적으로 확정한다.","blockReasonCodes":["NatureSafeCoreUnavailable","PartyRecoveryUnavailable"]} |
| 결과 | {"completionStateCodes":["PartyRecovered"],"effectCodes":["PartyRecovered"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"완료 전 회복 공간과 미사용 보급 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/지역사건-자연권위협-규칙.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationNaturePartyRecovery.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationRegionalIncidentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-385. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-regional-threat-recovery.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-regional-threat-core-20260826"],"blockers":["실제 Nature 경관 Graph와 귀환·회복 입력 결속이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["위협·후퇴·복원·회복의 표현 계약만 있고 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-NATURE-02~04의 Scenario 공간을 실제 Nature 경관 Graph와 canonical SimulationWorldShell 귀환·회복 입력으로 닫지 않았다.. 기존 다음 작업은 자동 실행 지시가 아니다: Nature 핵심 첫날 폐루프와 분리된 Extension Goal로 선택될 때 WI-NATURE-02부터 E4→E7을 진행한다.

- playable-loop:nature-farm-roundtrip.v1 / WorldAggregate / 통합 E1, 궤적 null. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: Nature와 Farm 독립 영역 집계가 모두 E7 PlayClosed가 아니다.. 기존 다음 작업은 자동 실행 지시가 아니다: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-05 — 벌목 도끼 획득

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r5. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 완료 명령이며 장착은 후속 WI-ACTOR-02가 담당한다.","cancellationPolicy":"소유권 이전 전에는 확정하지 않으며 이전 완료 뒤에는 취소하지 않는다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","PlayerAccessible","ToolPickupPoint"],"hRefs":["h1-stock:nature-trailhead"],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["AxeAvailable","PlayerWithoutAxe"],"resourceRequirements":["획득 가능한 기본 도끼"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 안전 빈터의 도구 획득 지점에서 기본 도끼 인스턴스의 소유권을 얻어 인벤토리에 넣는다.","previewRule":"플레이어·도구 지점·중복 소유 여부와 현재 개정을 검사하며 상태를 바꾸지 않는다.","confirmRule":"WI-ACTOR-01 권위 전이를 사용해 도끼 인스턴스를 플레이어 인벤토리로 한 번만 이전한다.","blockReasonCodes":["SimulationNatureSurvivalActionBlocked","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["AxeOwnedInInventory"],"effectCodes":["AxeAcquired"]} |
| 다음 선택 | {"successorWiIds":["WI-ACTOR-02"],"cancellationPolicy":"소유권 이전 전에는 확정하지 않으며 이전 완료 뒤에는 취소하지 않는다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationNatureSurvivalService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationActorEquipment.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationActorEquipmentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-tactical-self-navigation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-tactical-axe-hosted-parity-20260828"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-tactical-axe-playmode-20260828"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-tactical-self-navigation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=E2627217CB75232042D4BF79185EA35232F197FF46F7C2AB5918E43C584AFD76. 후보 상세는 -Wi -Id WI-NATURE-05 조회. 존재만으로 적합성 통과 아님.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: E7 PlayClosed 상태를 유지하고 다음 승인 Nature 내부 폐루프의 가장 이른 미완료 증거를 연다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-05 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-05 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-06 — 나무 벌목 작업 시작

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r5. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"실시간 입력이 유지되는 동안 Task를 진행하고 완료 시 나무를 그루터기로 바꾸며 통나무를 생성한다.","cancellationPolicy":"완료 전 WI-NATURE-12로 취소하면 진행률과 Actor·Tool·ResourceNode·WorkArea 점유를 해제하고 나무와 인벤토리는 변경하지 않는다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","WorkerAccessible","HarvestResourceWorkArea"],"hRefs":["h1-stock:nature-exploration-buffer"],"placementVerified":false} |
| 나 | {"actorRequirements":["capability:woodcutting을 장착 상태에서 가진 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["WoodcuttingCapabilityEquipped","TreeStanding","PlayerIdle"],"resourceRequirements":["서 있는 나무","장착된 벌목 도구"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 MainHand에 장착한 벌목 도구와 서 있는 나무를 선택해 벌목 작업을 시작한다.","previewRule":"나무 상태·장착된 벌목 능력·진행 중 작업·현재 개정을 검사하며 나무나 인벤토리를 바꾸지 않는다.","confirmRule":"대상 나무를 참조하는 4초 벌목 Task를 생성한다.","blockReasonCodes":["SimulationNatureResourceNodeNotFound","SimulationNatureResourceNodeUnavailable","SimulationNatureAxeRequired","SimulationNatureSurvivalActionBlocked"]} |
| 결과 | {"completionStateCodes":["HarvestWorkScheduled"],"effectCodes":["TreeFelled","TimberCreated"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-18","WI-NATURE-06","WI-NATURE-07","WI-NATURE-12"],"cancellationPolicy":"완료 전 WI-NATURE-12로 취소하면 진행률과 Actor·Tool·ResourceNode·WorkArea 점유를 해제하고 나무와 인벤토리는 변경하지 않는다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationNatureSurvivalService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationActorEquipment.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationActorEquipmentTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-06 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-06 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

- playable-loop:nature-field-supply-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-field-supply-core-20260825"],"blockers":["실제 H1 발현과 LocalProcess·RemoteHost 동등성이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["직접 제작·NPC 위임·다음 원정 준비의 Unity 결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.. 기존 다음 작업은 자동 실행 지시가 아니다: WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-07 — 오두막을 지을 터 선정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"배치는 즉시 완료되며 건설 시간과 자재 소비는 후속 WI가 담당한다.","cancellationPolicy":"건설 시작 전 재배치·취소 계약은 아직 없으며 E4 후속 규칙으로 남긴다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","PlayerAccessible","BuildingSite"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["CabinPlanned","BuildingSiteAvailable"],"resourceRequirements":["오두막 설계안"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 안전 생활핵 안에서 오두막을 지을 위치와 방향을 선택한다.","previewRule":"생활핵 허용 범위·기존 오두막 상태·현재 개정을 검사하고 배치 좌표만 제안한다.","confirmRule":"선택한 위치와 방향을 오두막 설계 상태에 기록한다.","blockReasonCodes":["SimulationNatureSurvivalActionBlocked","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["CabinBlueprintPlaced"],"effectCodes":["CabinBlueprintPlaced"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-08"],"cancellationPolicy":"건설 시작 전 재배치·취소 계약은 아직 없으며 E4 후속 규칙으로 남긴다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-07 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-07 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-08 — 오두막 건설 작업 시작

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"실시간 입력이 유지되는 동안 Task를 진행하고 완료 시 오두막을 사용 가능 상태로 전환한다.","cancellationPolicy":"완료 전 WI-NATURE-12로 취소하면 Actor·BuildingSite·WorkArea 점유를 해제하고 예약 통나무 6개를 인벤토리에 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","WorkerAccessible","BuildingSite","ShelterConstructionWorkArea"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["CabinBlueprintPlaced","TimberAvailable","PlayerIdle"],"resourceRequirements":["통나무 6개","배치된 오두막 설계안"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 배치된 오두막 설계안에 통나무를 예약하고 건설 작업을 시작한다.","previewRule":"설계안·통나무 수량·진행 중 작업·현재 개정을 검사하며 자재를 소비하지 않는다.","confirmRule":"통나무 6개를 예약·차감하고 30초 건설 Task를 생성한다.","blockReasonCodes":["SimulationNatureCabinBlueprintRequired","SimulationNatureTimberInsufficient","SimulationNatureSurvivalActionBlocked"]} |
| 결과 | {"completionStateCodes":["CabinBuildScheduled"],"effectCodes":["CabinOperational","CabinRecoveryEnabled","CabinDefenseEnabled"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-09","WI-NATURE-12","WI-NATURE-13"],"cancellationPolicy":"완료 전 WI-NATURE-12로 취소하면 Actor·BuildingSite·WorkArea 점유를 해제하고 예약 통나무 6개를 인벤토리에 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-08 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-08 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-09 — 오두막 안으로 들어가기

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 공간 상태를 바꾸며 이동 애니메이션은 표현 계층에서 처리한다.","cancellationPolicy":"입장 완료 뒤 되돌리기는 WI-NATURE-10 퇴장으로 수행한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","PlayerAccessible","ShelterInterior","ShelterEntrance"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["CabinOperational","PlayerOutsideCabin"],"resourceRequirements":["사용 가능한 오두막"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 완성된 오두막 내부 점유를 선택한다.","previewRule":"오두막 완공·입구 접근·내부 점유 상태와 현재 개정을 검사한다.","confirmRule":"플레이어의 현재 H1과 실내 점유 상태를 오두막으로 전환한다.","blockReasonCodes":["SimulationNatureSurvivalActionBlocked","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["PlayerInsideCabin"],"effectCodes":["PlayerEnteredCabin"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-10","WI-NATURE-13"],"cancellationPolicy":"입장 완료 뒤 되돌리기는 WI-NATURE-10 퇴장으로 수행한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-09 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-09 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-10 — 오두막 밖으로 나가기

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 공간 상태를 바꾸며 이동 애니메이션은 표현 계층에서 처리한다.","cancellationPolicy":"퇴장 완료 뒤 되돌리기는 WI-NATURE-09 입장으로 수행한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","ShelterInterior","ShelterEntrance","PlayerAccessible"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["오두막 내부의 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["PlayerInsideCabin"],"resourceRequirements":["사용 가능한 오두막 출입구"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 오두막 내부 점유를 해제하고 안전 빈터로 돌아간다.","previewRule":"플레이어의 현재 실내 상태와 출입구 사용 가능성 및 현재 개정을 검사한다.","confirmRule":"실내 점유를 해제하고 플레이어의 현재 H1을 안전 빈터로 전환한다.","blockReasonCodes":["SimulationNatureSurvivalActionBlocked","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["PlayerOutsideCabin"],"effectCodes":["PlayerLeftCabin"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-09"],"cancellationPolicy":"퇴장 완료 뒤 되돌리기는 WI-NATURE-09 입장으로 수행한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-10 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-10 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-11 — 황혼 위협 대응 방식 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r2. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"전투 활성 동안 Nature 시계를 정지하고 기존 100ms BattleTick 결과만 수용한다.","cancellationPolicy":"전투 결과 적용 뒤 같은 조우는 새 명령으로 재확정할 수 없다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable","EncounterDecisionArea","RetreatRoute"],"hRefs":["h1-stock:nature-emergency-retreat"],"placementVerified":false} |
| 나 | {"actorRequirements":["조우 중인 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["EncounterPending","CombatActive"],"resourceRequirements":["대기 중인 조우","Fight 또는 Retreat 선택"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 황혼 조우에서 싸움 또는 후퇴를 선택하고 기존 현장 전투의 승리·후퇴·패배 결과를 한 번만 인계한다.","previewRule":"조우 대기 또는 전투 활성 상태, 선택 코드와 현재 개정을 검사하며 결과를 바꾸지 않는다.","confirmRule":"Fight는 연결 전투 식별자만 열고 승리·후퇴·패배 결과 인계 또는 직접 Retreat만 조우를 한 번 해결한다.","blockReasonCodes":["SimulationNatureEncounterNotPending","SimulationNatureSurvivalActionBlocked","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["EncounterResolved","BattleHandoffRequested","PlayerRetreated","PlayerDefeated"],"effectCodes":["BattleHandoffRequested","EncounterVictoryRewarded","PlayerRetreated","CarriedMaterialsLost","EncounterResolved"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-04","WI-NATURE-14","WI-NATURE-17"],"cancellationPolicy":"전투 결과 적용 뒤 같은 조우는 새 명령으로 재확정할 수 없다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-001, Q-002, Q-003, Q-004, Q-005, Q-023, Q-024, Q-025, Q-026, Q-027, Q-028, Q-029, Q-030, Q-031, Q-032, Q-033, Q-034, Q-035, Q-132, Q-141, Q-149, Q-153, Q-156. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-twilight-return.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-dual-combat-core-20260825","evidence:nature-twilight-wi11-hosted-parity-20260826"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-dual-combat-unity-editmode-20260825","evidence:nature-twilight-wi11-playmode-20260826","evidence:nature-dual-loop-game-view-20260826"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: 완료 상태를 유지하되 H3 ThreatInput·전투 상태·Skeleton 표현 계약 변경 시 논리와 표현 증거를 각각 재검증한다.

- playable-loop:nature-field-supply-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-field-supply-core-20260825"],"blockers":["실제 H1 발현과 LocalProcess·RemoteHost 동등성이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["직접 제작·NPC 위임·다음 원정 준비의 Unity 결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.. 기존 다음 작업은 자동 실행 지시가 아니다: WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-12 — 진행 중 작업 취소

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 취소 효과를 적용하고 원래 WI 발현 기록과 취소 WI 기록을 같은 개정 전이에 결속한다.","cancellationPolicy":"취소 명령 자체는 멱등하며 새 작업은 별도 Preview와 Confirm으로 다시 시작한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["ActiveWorkReservationContext"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["진행 작업을 소유한 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["WorkActive"],"resourceRequirements":["진행 중인 Nature 작업"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 진행 중인 벌목 또는 오두막 건설을 취소하고 점유와 예약 자원을 안전하게 반환한다.","previewRule":"진행 중 작업과 대상 일치, 현재 개정 및 원래 작업의 공간 문맥을 검사하며 상태를 바꾸지 않는다.","confirmRule":"원래 작업을 취소 상태로 닫고 점유를 해제하며 건설 예약 통나무를 인벤토리로 반환한다.","blockReasonCodes":["SimulationNatureActiveWorkRequired","SimulationNatureSurvivalActionBlocked","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["WorkCancelled","SafeChoiceAvailable"],"effectCodes":["WorkCancelled","SpatialReservationReleased","ReservedMaterialReturned"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"취소 명령 자체는 멱등하며 새 작업은 별도 Preview와 Confirm으로 다시 시작한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/Nature생존생활거점세로조각.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationNatureSurvivalService.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/LocalSimulationRuntimeTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-12 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-12 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

- playable-loop:nature-workbench-foundation.v1 / PlayableUnit / 통합 E6, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-building-core-20260825","evidence:nature-workbench-wi-con-01-hosted-parity-20260826"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E6","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-workbench-wi-con-01-playmode-20260826","evidence:nature-dual-loop-game-view-20260826"],"blockers":["Synty Table Saw는 식별되지만 건설 중·운영 중 상태 차이와 목재·상자·조명으로 구성된 작업 공간은 아직 부족하다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 작업대 상태별 공간 조립과 운영 가능성의 시각 E7 증거가 부족하다.. 기존 다음 작업은 자동 실행 지시가 아니다: Table Saw·목재·상자·조명을 배치 통제 계층에서 하나의 작업 구역으로 조립하고 건설·취소·운영 화면 차이를 재검증한다.

- playable-loop:nature-building-learning.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E5","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-building-core-20260825"],"blockers":["Hosted 동등성과 NPC 생활 주기 검증이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["배움터의 배치·학습 방문·결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배움터 실제 Unity 배치·Hosted 동등성과 NPC 판단→이동→학습 결과→다음 판단의 E7 폐루프 증거가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 기존 StableId를 유지한 채 배움터 Extension의 NPC 생활 주기를 E7→E1로 검토하고 가장 낮은 미완료 의존성부터 구현한다.

- playable-loop:nature-field-supply-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-field-supply-core-20260825"],"blockers":["실제 H1 발현과 LocalProcess·RemoteHost 동등성이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["직접 제작·NPC 위임·다음 원정 준비의 Unity 결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.. 기존 다음 작업은 자동 실행 지시가 아니다: WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-13 — 획득 자원 거점 보관

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r2. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"첫 구현은 가능한 수량 모두를 즉시 옮긴다.","cancellationPolicy":"확정 전에는 무변경이며 확정된 입고는 별도 인출 WI 전까지 유지한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["ShelterInterior","StorageInteractionAnchor"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["오두막에 접근한 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["CabinOperational","PlayerInsideCabin","TimberCarried"],"resourceRequirements":["소지 통나무","남은 보관 용량"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 완성된 오두막 보관함과 상호작용해 소지 통나무 중 가능한 수량을 직접 보관한다.","previewRule":"오두막 완공·접근·소지량·용량과 현재 개정을 검사하고 가능한 이동량만 계산한다.","confirmRule":"Confirm에서만 소지 통나무를 오두막 Container로 옮기고 입고 Transfer를 남긴다.","blockReasonCodes":["SimulationNatureCabinRequired","SimulationNatureCabinAccessRequired","SimulationNatureCabinStorageFull","SimulationNatureTimberNotCarried","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["TimberStored"],"effectCodes":["TimberStored","CabinStorageTransferRecorded"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-11","WI-NATURE-14","WI-NATURE-17"],"cancellationPolicy":"확정 전에는 무변경이며 확정된 입고는 별도 인출 WI 전까지 유지한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/Nature생존생활거점세로조각.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-night-day2.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-personal-plan-logic-e3-20260830","evidence:nature-heat-source-logic-e3-20260830","evidence:nature-night-day2-wi13-hosted-parity-20260826","evidence:nature-night-day2-wi14-hosted-parity-20260826","evidence:nature-night-day2-wi15-hosted-parity-20260826"],"blockers":["세계 자원 재생 E1→E3 구현 중. 개인 계획·열원 E3 증거는 보존.","열원 변경 경로 독립 Core E3까지만 시험. 이전 다른 WI의 증거는 보존하되 열원으로 전이하지 않는다.","v28 행위 원장·분야 성장 계보가 포함된 실제 입력과 LocalProcess·RemoteHost 반복 증거를 다시 확보해야 한다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Blocked","evidencePackageRefs":["evidence:nature-night-day2-wi13-playmode-20260826","evidence:nature-night-day2-wi14-playmode-20260826","evidence:nature-night-day2-wi15-playmode-20260826","evidence:nature-dual-loop-game-view-20260826","evidence:nature-night-day2-presentation-e7-20260826"],"blockers":["재생 성장 단계·채집 가능 상태의 판독 요구 E1만 정의.","표현 엔진의 행위 원장 cursor 소비와 같은 Revision Game View 증거가 없다.","Day2 계획판의 세 선택·비용·다음 행동 판독과 오두막 주변 구도, 사람 직접 입력·청음 수용이 남았다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 행위 원장·분야 성장 공통 관문 소급으로 기존 E6/E7 증거가 무효화됐다.; Unity 표현 엔진의 cursor 소비와 현재 Game View 증거를 다시 검증해야 한다.. 기존 다음 작업은 자동 실행 지시가 아니다: 세계 자원 재생 Logic E3 검증 후 캠페인 다음 WI로 전환.

- playable-loop:nature-field-supply-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-field-supply-core-20260825"],"blockers":["실제 H1 발현과 LocalProcess·RemoteHost 동등성이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["직접 제작·NPC 위임·다음 원정 준비의 Unity 결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.. 기존 다음 작업은 자동 실행 지시가 아니다: WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-14 — 오두막에서 수면·새벽 맞기

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r2. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"새벽 1110초에 자동 해제하며 WorldTick 의미를 바꾸지 않는다.","cancellationPolicy":"첫 구현은 새벽 자동 해제만 허용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["ShelterInterior","SleepInteractionAnchor"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["오두막 내부의 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Night","PlayerInsideCabin","EncounterResolved"],"resourceRequirements":["완성된 오두막"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 전투가 끝난 밤에 완성된 오두막 안에서 수면을 선택한다.","previewRule":"밤·오두막 내부·전투 종료·현재 개정을 검사한다.","confirmRule":"수면 상태를 확정하고 이후 실시간 진행만 밤 구간에서 6배로 계산한다.","blockReasonCodes":["SimulationNatureCabinRequired","SimulationNatureCabinAccessRequired","SimulationNatureNightRequired","SimulationNatureSurvivalActionBlocked"]} |
| 결과 | {"completionStateCodes":["Sleeping","DawnReached"],"effectCodes":["SleepStarted","DawnReached","SleepReleased"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-15"],"cancellationPolicy":"첫 구현은 새벽 자동 해제만 허용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/Nature생존생활거점세로조각.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/SkyEngine세계대기표현계층.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-night-day2.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-personal-plan-logic-e3-20260830","evidence:nature-heat-source-logic-e3-20260830","evidence:nature-night-day2-wi13-hosted-parity-20260826","evidence:nature-night-day2-wi14-hosted-parity-20260826","evidence:nature-night-day2-wi15-hosted-parity-20260826"],"blockers":["세계 자원 재생 E1→E3 구현 중. 개인 계획·열원 E3 증거는 보존.","열원 변경 경로 독립 Core E3까지만 시험. 이전 다른 WI의 증거는 보존하되 열원으로 전이하지 않는다.","v28 행위 원장·분야 성장 계보가 포함된 실제 입력과 LocalProcess·RemoteHost 반복 증거를 다시 확보해야 한다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Blocked","evidencePackageRefs":["evidence:nature-night-day2-wi13-playmode-20260826","evidence:nature-night-day2-wi14-playmode-20260826","evidence:nature-night-day2-wi15-playmode-20260826","evidence:nature-dual-loop-game-view-20260826","evidence:nature-night-day2-presentation-e7-20260826"],"blockers":["재생 성장 단계·채집 가능 상태의 판독 요구 E1만 정의.","표현 엔진의 행위 원장 cursor 소비와 같은 Revision Game View 증거가 없다.","Day2 계획판의 세 선택·비용·다음 행동 판독과 오두막 주변 구도, 사람 직접 입력·청음 수용이 남았다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 행위 원장·분야 성장 공통 관문 소급으로 기존 E6/E7 증거가 무효화됐다.; Unity 표현 엔진의 cursor 소비와 현재 Game View 증거를 다시 검증해야 한다.. 기존 다음 작업은 자동 실행 지시가 아니다: 세계 자원 재생 Logic E3 검증 후 캠페인 다음 WI로 전환.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-15 — 다음 날 거점 확장 계획 선택

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r2. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"다음 날 구현에서 계획 비용과 효과를 별도 WI로 실행한다.","cancellationPolicy":"첫날 저장 기준선에서는 선택을 고정하며 변경은 다음 날 별도 규칙으로 다룬다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["ShelterInterior"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["첫날을 마친 Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Dawn","PlanUnselected"],"resourceRequirements":["계획 비용 표시용 소지·보관 자원"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 새벽에 작업대·보관대·방책 중 다음 날 목표 하나를 선택한다.","previewRule":"새벽·미선택 상태·안정 계획 코드를 검사하고 부족 재료는 차단 대신 목표로 표시한다.","confirmRule":"계획 코드와 Day2Ready만 고정하며 비용이나 향후 효과를 즉시 적용하지 않는다.","blockReasonCodes":["SimulationNatureExpansionPlanInvalid","SimulationNatureExpansionPlanAlreadySelected","SimulationNatureSurvivalActionBlocked"]} |
| 결과 | {"completionStateCodes":["Day2Ready","ExpansionPlanSelected"],"effectCodes":["ExpansionPlanSelected","Day2Ready"]} |
| 다음 선택 | {"successorWiIds":["WI-CON-01"],"cancellationPolicy":"첫날 저장 기준선에서는 선택을 고정하며 변경은 다음 날 별도 규칙으로 다룬다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/Nature생존생활거점세로조각.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-001, Q-002, Q-003, Q-004, Q-005, Q-023, Q-024, Q-025, Q-026, Q-027, Q-028, Q-029, Q-030, Q-031, Q-032, Q-033, Q-034, Q-035, Q-132, Q-141, Q-149, Q-153, Q-156. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-night-day2.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-personal-plan-logic-e3-20260830","evidence:nature-heat-source-logic-e3-20260830","evidence:nature-night-day2-wi13-hosted-parity-20260826","evidence:nature-night-day2-wi14-hosted-parity-20260826","evidence:nature-night-day2-wi15-hosted-parity-20260826"],"blockers":["세계 자원 재생 E1→E3 구현 중. 개인 계획·열원 E3 증거는 보존.","열원 변경 경로 독립 Core E3까지만 시험. 이전 다른 WI의 증거는 보존하되 열원으로 전이하지 않는다.","v28 행위 원장·분야 성장 계보가 포함된 실제 입력과 LocalProcess·RemoteHost 반복 증거를 다시 확보해야 한다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Blocked","evidencePackageRefs":["evidence:nature-night-day2-wi13-playmode-20260826","evidence:nature-night-day2-wi14-playmode-20260826","evidence:nature-night-day2-wi15-playmode-20260826","evidence:nature-dual-loop-game-view-20260826","evidence:nature-night-day2-presentation-e7-20260826"],"blockers":["재생 성장 단계·채집 가능 상태의 판독 요구 E1만 정의.","표현 엔진의 행위 원장 cursor 소비와 같은 Revision Game View 증거가 없다.","Day2 계획판의 세 선택·비용·다음 행동 판독과 오두막 주변 구도, 사람 직접 입력·청음 수용이 남았다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 행위 원장·분야 성장 공통 관문 소급으로 기존 E6/E7 증거가 무효화됐다.; Unity 표현 엔진의 cursor 소비와 현재 Game View 증거를 다시 검증해야 한다.. 기존 다음 작업은 자동 실행 지시가 아니다: 세계 자원 재생 Logic E3 검증 후 캠페인 다음 WI로 전환.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-16 — 현장 보급 꾸러미 제작

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r4. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"입력 유지 4초 뒤 현장 보급 꾸러미 1개를 지급하며 취소하면 이 제작 작업의 예약 재료만 반환한다.","cancellationPolicy":"완료 전 WI-NATURE-12가 예약 재료를 반환하고 완료된 꾸러미는 다음 WI-NATURE-06에서 선택·소비한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["ShelterInterior","CraftingWorkArea","ActiveWorkReservationContext"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 생활 거점의 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Day2Ready","NatureWorkbenchOperational","PlayerInsideCabin"],"resourceRequirements":["소지 또는 보관 통나무 2","소지 재건 부품 1","운영 중인 Nature 작업대"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 운영 중인 Nature 작업대에서 거점의 통나무와 소지 재건 부품을 조립해 다음 현장 원정용 보급 꾸러미를 만든다.","previewRule":"r4 세션, 작업대 운영, 오두막 H1 접근, 재료, 진행 작업과 현재 개정을 검사하며 상태를 바꾸지 않는다.","confirmRule":"통나무 2와 재건 부품 1을 예약·소비하고 4초 권위 제작 작업을 만든다.","blockReasonCodes":["SimulationNatureWorkbenchRequired","SimulationNatureCabinAccessRequired","SimulationNatureFieldSupplyTimberInsufficient","SimulationNatureFieldSupplyRebuildPartInsufficient","SimulationNatureSurvivalActionBlocked"]} |
| 결과 | {"completionStateCodes":["NatureFieldSupplyPackAdded","FieldExpeditionChoiceAvailable"],"effectCodes":["NatureFieldSupplyPackAdded","ExpeditionPrepared","OneCarriedMaterialStackProtected"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"완료 전 WI-NATURE-12가 예약 재료를 반환하고 완료된 꾸러미는 다음 WI-NATURE-06에서 선택·소비한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이폐루프와증거묶음개발체계.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationNatureSurvivalContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/LocalSimulationRuntimeTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationAreaBuildingProgressionTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-036, Q-037, Q-038, Q-039, Q-051, Q-052, Q-053, Q-054, Q-055, Q-056, Q-057, Q-058, Q-059, Q-060, Q-134, construction-cancel-material-preview, construction-cancel-material-state, construction-cancel-normal-refund, construction-cancel-refund-difficulty. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-field-supply-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-field-supply-core-20260825"],"blockers":["실제 H1 발현과 LocalProcess·RemoteHost 동등성이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["직접 제작·NPC 위임·다음 원정 준비의 Unity 결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.. 기존 다음 작업은 자동 실행 지시가 아니다: WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-17 — 현장 보급 제작 업무 위임

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / npc-routine-control.r3. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"NPC 작업은 입력 유지 없이 Nature 권위 시계 4초 동안 진행하고 꾸러미 한 개가 있으면 추가 제작하지 않는다.","cancellationPolicy":"완료 전 WI-NATURE-12가 이 작업의 예약 재료만 반환하며 정책 중지는 새 작업만 막고 진행 작업을 암묵적으로 취소하지 않는다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["CraftingWorkArea","ActiveWorkReservationContext"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["현장보급제작 역량을 가진 Nature 거점 NPC"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["Day2Ready","NatureWorkbenchOperational","NpcFieldSupplyPolicyEnabled"],"resourceRequirements":["소지 또는 보관 통나무 2","소지 재건 부품 1","운영 중인 Nature 작업대","활성 NPC 정책"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 원정 보급 우선 정책을 활성화하면 적격 Nature 거점 NPC가 작업대와 재료를 예약해 다음 현장 원정용 보급 꾸러미를 만든다.","previewRule":"r4 세션과 npc-routine-control.r3, 작업대·재료·정책·담당 NPC·진행 작업·현재 재고 목표를 읽고 차단 이유만 반환한다.","confirmRule":"플레이어가 작업 완료 명령을 보내지 않으며 정책 활성 뒤 Nature 권위 시계가 적격 NPC를 결정적으로 선택하고 재료를 한 번 예약한다.","blockReasonCodes":["SimulationNpcRoutineNatureRevisionRequired","SimulationNpcRoutinePolicyMissing","SimulationNpcAutomationDisabled","SimulationNpcAutoDelegationDisabled","SimulationNpcEligibleActorMissing","SimulationNatureWorkbenchRequired","SimulationNatureFieldSupplyTimberInsufficient","SimulationNatureFieldSupplyRebuildPartInsufficient","SimulationNatureFieldSupplyAlreadyAvailable","SimulationNatureSurvivalActionBlocked"]} |
| 결과 | {"completionStateCodes":["NatureFieldSupplyPackAdded","FieldExpeditionChoiceAvailable"],"effectCodes":["NpcFieldSupplyPolicySelected","NatureFieldSupplyPackAdded","FieldExpeditionChoiceAvailable"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-06"],"cancellationPolicy":"완료 전 WI-NATURE-12가 이 작업의 예약 재료만 반환하며 정책 중지는 새 작업만 막고 진행 작업을 암묵적으로 취소하지 않는다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/NPC루틴WI통제정책.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationNatureSurvivalContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNaturePlayFlowCycle.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNpcRoutineWork.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNaturePlayFlowCycleTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-field-supply-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-field-supply-core-20260825"],"blockers":["실제 H1 발현과 LocalProcess·RemoteHost 동등성이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["직접 제작·NPC 위임·다음 원정 준비의 Unity 결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.. 기존 다음 작업은 자동 실행 지시가 아니다: WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-NATURE-18 — 벌목 통나무 줍기

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / nature-survival.realtime.r5. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 상태 전이하며 벌목 Task나 월드 시계를 대신 진행하지 않는다.","cancellationPolicy":"Confirm 전에는 상태가 변하지 않으며 Confirm 뒤 동일 묶음은 다시 획득할 수 없다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["WorkerAccessible","DroppedTimberPickupAnchor"],"hRefs":["h1-stock:nature-exploration-buffer"],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 생존 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["DroppedTimberAvailable","InventoryCapacityAvailable"],"resourceRequirements":["수집 가능한 지면 통나무 묶음","묶음 전체를 담을 인벤토리 여유"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 벌목으로 지면에 생성된 통나무 묶음을 선택해 소지 인벤토리로 옮긴다.","previewRule":"r5 세션, 지면 통나무 존재·가용 상태, 현재 개정과 인벤토리 잔여 용량을 검사하며 상태를 바꾸지 않는다.","confirmRule":"대상 통나무 묶음을 한 번 Collected로 전이하고 동일 수량을 플레이어 인벤토리에 원자적으로 더한다.","blockReasonCodes":["SimulationNatureDroppedTimberNotFound","SimulationNatureDroppedTimberUnavailable","SimulationWorldInventoryPlayerCapacityExceeded","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["DroppedTimberCollected","TimberCarried"],"effectCodes":["TimberCollected","DroppedTimberRemoved"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-06","WI-NATURE-07","WI-NATURE-13"],"cancellationPolicy":"Confirm 전에는 상태가 변하지 않으며 Confirm 뒤 동일 묶음은 다시 획득할 수 없다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이폐루프논리시각이중순환체계.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이폐루프와증거묶음개발체계.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationNatureSurvivalContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationNatureSurvival.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationNatureSurvivalTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-shelter-foundation.v1 / PlayableUnit / 통합 E7, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-hosted-parity-20260825","evidence:nature-first-evening-equipment-logic-20260827"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-shelter-playmode-20260825","evidence:nature-dual-loop-game-view-20260826","evidence:nature-shelter-explicit-equipment-e7-20260827"],"blockers":[]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/actor-item-equipment.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=False, 명세 hash=02E9220109579DF43EBC2DEACBF2D200C088A9B215C31E3E823D654826F54EE5. 후보 상세는 -Wi -Id WI-NATURE-18 조회. 존재만으로 적합성 통과 아님.
  - [기존 명세](../../../eng/execution-ledgers/work-orders/nature-woodcutting-animation.e7-work-order.json) / SourceAvailableNotValidated, E4 후보 항목 존재=True, 명세 hash=FEFB471028C5788E34C1DD5B0CC6D0FD7A7A16158A967A80997FCE63E8A72E7A. 후보 상세는 -Wi -Id WI-NATURE-18 조회. 존재만으로 적합성 통과 아님.
  - 명세 문맥의 판독 순간: 기존 나무 벌목 준비·접촉·반복·완료 및 취소 후 이동/Idle 복귀; VisualKey: animation:nature:axe-swing. 개별 WI 적용 범위는 원명세를 다시 확인한다.
  - 주 후보: 현재 Nature 플레이어/장착 도끼: 적용 연구에서 원본 GUID/hash 동결; 대체: ; fallback: Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs.
  - 배치/Anchor: 기존 canonical Scene/Actor/나무 배치 유지 / 기존 나무 접촉과 손도끼 결합; 세부 기술값 연구 결속. 준비 상태: Blocked.
  - 열린 준비: WoodcuttingContactQualityUnverified; SingleBoneWriterBindingPending.
  - 기존 차단: . 기존 다음 작업은 자동 실행 지시가 아니다: stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-REFLECT-01 — 승인 자료로 거점 성찰 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / base-reflection.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"다음 활동 경계에서 허용된 Awareness+1/BeginnerMind 또는 Resolve+1/IntegratedProgress만 영구 적용하고 다음 발산 선택으로 돌아간다.","cancellationPolicy":"Confirm 전에는 상태를 바꾸지 않으며 Confirm 뒤 지급 계보를 제자리 수정하지 않고 후속 보정 명령을 사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["ReflectionInteractionAnchor","ShelterInterior"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["Nature 거점에 귀환한 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["ReturnedToBase","NatureSafeChoiceAvailable","ReflectionChoiceAvailable"],"resourceRequirements":["선택적인 승인 학습 Publication","해당 일차의 미사용 성찰 기회"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 거점 귀환 뒤 그냥 휴식하거나 세션 시작 때 동결된 승인 자료를 골라 오늘 행동을 성찰하고 다음 활동에 적용할 내면 학습 하나를 확정한다.","previewRule":"현재 개정, 플레이어, 일차, 승인 Publication stable ID·revision·hash, 하루 한 번과 캐릭터별 revision 한 번 지급을 검사하고 다음 활동 효과만 보여준다.","confirmRule":"CommandId·ExpectedRevision과 Preview stable ID를 검증하고 영상 시청 정보 없이 InnerLearningPending 지급 계보를 한 번 만든다.","blockReasonCodes":["SimulationApprovedLearningMaterialUnavailable","SimulationReflectionDailyLimitReached","SimulationReflectionPublicationAlreadyGranted","SimulationReflectionPreviewMismatch","SimulationExpectedRevisionMismatch"]} |
| 결과 | {"completionStateCodes":["InnerLearningPending"],"effectCodes":["InnerLearningPending"]} |
| 다음 선택 | {"successorWiIds":["WI-NATURE-06"],"cancellationPolicy":"Confirm 전에는 상태를 바꾸지 않으며 Confirm 뒤 지급 계보를 제자리 수정하지 않고 후속 보정 명령을 사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/PlayableLoops/nature-base-reflection.v1.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Contracts/UnityPackage/Runtime/SimulationBaseReflectionContracts.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationBaseReflection.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationBaseReflectionTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-006, Q-007, Q-008, Q-009, Q-010, Q-011, Q-012, Q-013, Q-014, Q-015, Q-016, Q-017, Q-018, Q-019, Q-020, Q-021, Q-022, Q-040, Q-041, Q-042, Q-043, Q-044, Q-065, Q-066, Q-067, Q-122, Q-123, Q-124, Q-125, Q-196, Q-197, Q-198, Q-398. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-base-reflection.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-base-reflection-e3-20260826"],"blockers":["WI-REFLECT-01의 E4 결속과 주 세션 Adapter가 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["성찰 선택·근거·내면 성장 결과를 읽는 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: WI-REFLECT-01과 H1 ReflectionInteractionAnchor의 E4 결속, 주 세션 저장·LocalProcess·RemoteHost Adapter, SimulationWorldShell 실제 입력 증거가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 공용 E1~E3 계약·시험을 기준선으로 삼아 WI-REFLECT-01과 기존 Nature 오두막 H1을 결속한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CARD-01 — 현재 세계의 메이저 아르카나 활성화

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / arcana-town-life.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"활성화 수명 동안 방향을 다시 계산하지 않고 승인된 하위 카드 영향에 한 번만 전개한다.","cancellationPolicy":"해제·교체는 기존 활성화를 종료하고 다음 활성화에서 새 방향을 판정하며 과거 계보는 수정하지 않는다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["메이저 아르카나 선택 권한 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["MajorArcanaChoiceAvailable"],"resourceRequirements":["승인된 메이저 아르카나","Town 회복비중 상태 사본"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 메이저 아르카나를 선택하면 Simulation이 활성화 시점의 권위 세계 상태로 이번 활성화의 정·역방향을 한 번 판정하고 하위 카드 영향 상태 사본을 만든다.","previewRule":"카드 고유 식별자, 현재 개정, 회복비중과 활성화 중복을 검사하고 방향 후보와 근거만 반환한다.","confirmRule":"새 MajorArcanaActivationStableId를 만들고 고정 정밀도 51% 규칙으로 방향·근거·정책 revision을 동결한다.","blockReasonCodes":["SimulationMajorArcanaInvalid","SimulationMajorArcanaActivationRevisionConflict","SimulationMajorArcanaRecoveryEvidenceInvalid"]} |
| 결과 | {"completionStateCodes":["MajorArcanaActivationFrozen","TownLifeChoiceAvailable"],"effectCodes":["MajorArcanaActivated","ArcanaOrientationFrozen","ArcanaInfluenceSnapshotsCreated"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"해제·교체는 기존 활성화를 종료하고 다음 활성화에서 새 방향을 판정하며 과거 계보는 수정하지 않는다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/게임기획통합기준.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/플레이폐루프완결로드맵.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationArcanaTownLife.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationArcanaTownNpcLifeTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-006, Q-007, Q-008, Q-009, Q-010, Q-011, Q-012, Q-013, Q-014, Q-015, Q-016, Q-017, Q-018, Q-019, Q-020, Q-021, Q-022, Q-040, Q-041, Q-042, Q-043, Q-044, Q-065, Q-066, Q-067, Q-122, Q-123, Q-124, Q-125, Q-196, Q-197, Q-198, Q-398. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:town-arcana-context.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E4","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:town-arcana-core-20260825"],"blockers":["Town 핵심 폐루프에 적용된 최종 E5 계보가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["정·역방향 근거와 적용 결과의 Unity 설명 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 활성화 Snapshot은 시험됐지만 Town 핵심 폐루프에 적용된 최종 E5 계보와 Unity 설명 화면이 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 핵심 주문 폐루프와 분리된 확장 Fixture에서 방향 판정·단일 전개·저장 복원을 발현한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CON-01 — 영역 건물 건설 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / area-building-tech-tree.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"권위 시간이 끝나면 Operational로 전이하고 취소 시 이 작업의 예약만 반환한다.","cancellationPolicy":"완료 전 WI-NATURE-12 또는 공통 작업 취소가 재료·발자국 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["AreaBuildingPlacementAllowed","FootprintAvailable"],"hRefs":["h1-stock:nature-shelter"],"placementVerified":false} |
| 나 | {"actorRequirements":["해당 영역 건설 권한 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Available"],"resourceRequirements":["청사진별 재료","권위 작업 시간"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 영역 청사진을 선택하고 재료·시간·배치 조건을 통과해 H1 건물을 운영 상태로 만든다.","previewRule":"청사진 revision/hash, 선행 건물, 비용, 배치 반경과 발자국 겹침을 검사하고 상태를 바꾸지 않는다.","confirmRule":"CommandId·ExpectedRevision으로 재료와 발자국을 예약하고 Building 상태를 만든다.","blockReasonCodes":["SimulationAreaBuildingBlueprintLocked","SimulationAreaBuildingTimberInsufficient","SimulationAreaBuildingPlacementOutsideHome","SimulationAreaBuildingPlacementOverlap"]} |
| 결과 | {"completionStateCodes":["Building","Operational"],"effectCodes":["AreaBuildingOperational"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"완료 전 WI-NATURE-12 또는 공통 작업 취소가 재료·발자국 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/영역별건물발전테크트리계획.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationAreaBuildingProgression.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Tests/SimulationAreaBuildingProgressionTests.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: Q-036, Q-037, Q-038, Q-039, Q-051, Q-052, Q-053, Q-054, Q-055, Q-056, Q-057, Q-058, Q-059, Q-060, Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-134, Q-136, Q-143, Q-144, Q-146, Q-147, Q-155, Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-199, Q-200, Q-201, Q-202, Q-203, Q-204, Q-205, Q-206, Q-207, Q-208, Q-209, Q-210, Q-211, Q-212, Q-213, Q-214, Q-215, Q-216, Q-217, Q-218, Q-219, Q-354, Q-355, Q-356, Q-357, Q-358, Q-359, Q-378, Q-379, Q-380, Q-383, Q-384, Q-385, construction-cancel-material-preview, construction-cancel-material-state, construction-cancel-normal-refund, construction-cancel-refund-difficulty, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:nature-workbench-foundation.v1 / PlayableUnit / 통합 E6, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E7","targetStage":"E7","statusCode":"Validated","evidencePackageRefs":["evidence:nature-building-core-20260825","evidence:nature-workbench-wi-con-01-hosted-parity-20260826"],"blockers":[]},"presentation":{"trackCode":"Presentation","currentStage":"E6","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-workbench-wi-con-01-playmode-20260826","evidence:nature-dual-loop-game-view-20260826"],"blockers":["Synty Table Saw는 식별되지만 건설 중·운영 중 상태 차이와 목재·상자·조명으로 구성된 작업 공간은 아직 부족하다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 작업대 상태별 공간 조립과 운영 가능성의 시각 E7 증거가 부족하다.. 기존 다음 작업은 자동 실행 지시가 아니다: Table Saw·목재·상자·조명을 배치 통제 계층에서 하나의 작업 구역으로 조립하고 건설·취소·운영 화면 차이를 재검증한다.

- playable-loop:nature-building-learning.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E5","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:nature-building-core-20260825"],"blockers":["Hosted 동등성과 NPC 생활 주기 검증이 남아 있다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["배움터의 배치·학습 방문·결과 표현 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 배움터 실제 Unity 배치·Hosted 동등성과 NPC 판단→이동→학습 결과→다음 판단의 E7 폐루프 증거가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 기존 StableId를 유지한 채 배움터 Extension의 NPC 생활 주기를 E7→E1로 검토하고 가장 낮은 미완료 의존성부터 구현한다.

- playable-loop:farm-player-placement.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["Farm 전용 배치 규칙·Save 계보·권위 Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["Preview·Confirm·배치 결과의 Farm Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 현재 건물 WI의 실제 건설 상태 전이는 Nature 전용이며 Farm 배치는 별도 규칙·Scene 증거가 섞여 있다.. 기존 다음 작업은 자동 실행 지시가 아니다: Farm 핵심 생산 폐루프와 분리해 배치 Preview·Confirm·Tick·Save 계보를 전용 증거로 만든다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CITY-01 — 도심 서비스 수요 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / city-independent-service.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"확정된 수요를 지역 재고 배정으로 인계한다.","cancellationPolicy":"배정 전 수요 슬롯을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["UrbanDemandArea"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["City 운영 플레이어"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["CityDemandChoiceAvailable"],"resourceRequirements":["City 독립 수요 Fixture"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 City 내부 수요 Fixture에서 이번 처리 대상을 확정한다.","previewRule":"수요·지역 재고·현재 개정을 검사한다.","confirmRule":"StableId와 ExpectedRevision으로 수요를 확정한다.","blockReasonCodes":["SimulationCityDemandUnavailable"]} |
| 결과 | {"completionStateCodes":["CityDemandConfirmed"],"effectCodes":["CityDemandConfirmed"]} |
| 다음 선택 | {"successorWiIds":["WI-CITY-02"],"cancellationPolicy":"배정 전 수요 슬롯을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이폐루프완결로드맵.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/도심마트수요CompositionModels.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:city-demand-service-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["권위 Core·Save/Replay·Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["도심 수요·배정·서비스 결과의 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 계약만 있으며 권위 Core·Save/Replay·Fixture가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CITY-02 — 도심 서비스용 지역 재고 배정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / city-independent-service.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"StableId 정렬과 재고량으로 배정·부족을 확정한다.","cancellationPolicy":"후속 처리 전 예약 재고를 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["UrbanSortingArea"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["Simulation Core"],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["CityDemandConfirmed"],"resourceRequirements":["City 지역 재고"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"확정 수요에 City 독립 재고를 결정적으로 배정한다.","previewRule":"확정 수요와 지역 재고를 읽는다.","confirmRule":"WI-CITY-01 계보 안에서만 실행한다.","blockReasonCodes":["SimulationCityInventoryUnavailable"]} |
| 결과 | {"completionStateCodes":["CityInventoryAllocated","CityInventoryShortage"],"effectCodes":["CityInventoryAllocated","CityInventoryShortage"]} |
| 다음 선택 | {"successorWiIds":["WI-CITY-03"],"cancellationPolicy":"후속 처리 전 예약 재고를 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이폐루프완결로드맵.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/도심마트공급경영SimulationEngine.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:city-demand-service-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["권위 Core·Save/Replay·Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["도심 수요·배정·서비스 결과의 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 계약만 있으며 권위 Core·Save/Replay·Fixture가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CITY-03 — 도심 주민 서비스 처리

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / city-independent-service.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"처리 결과와 미충족 수요를 별도 상태로 남긴다.","cancellationPolicy":"서비스 시작 전 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["LastMileHandoffArea"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["City 운영 플레이어"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["CityInventoryAllocated","CityInventoryShortage"],"resourceRequirements":["배정 재고 또는 부족 복구 선택"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 배정된 재고로 공동수령 또는 매장 서비스를 처리한다.","previewRule":"배정·부족 상태와 서비스 후보를 검사한다.","confirmRule":"서비스 또는 부족 복구 선택을 확정한다.","blockReasonCodes":["SimulationCityServiceChoiceInvalid"]} |
| 결과 | {"completionStateCodes":["CityServiceCompleted","CityServiceDeferred"],"effectCodes":["CityServiceCompleted","CityServiceDeferred"]} |
| 다음 선택 | {"successorWiIds":["WI-CITY-04"],"cancellationPolicy":"서비스 시작 전 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이폐루프완결로드맵.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/도심마트공급경영SimulationEngine.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:city-demand-service-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["권위 Core·Save/Replay·Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["도심 수요·배정·서비스 결과의 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 계약만 있으며 권위 Core·Save/Replay·Fixture가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-CITY-04 — 도심 서비스 결과 확인

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / city-independent-service.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 반환 상태로 전이한다.","cancellationPolicy":"결과 확인은 멱등 처리한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["UrbanRecoveryRoute"],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["City 운영 플레이어"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["CityServiceCompleted","CityServiceDeferred"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 완료·유예 결과를 확인하고 다음 City 운영 선택으로 돌아간다.","previewRule":"미확인 결과와 현재 개정을 검사한다.","confirmRule":"결과 확인과 다음 선택 가능 상태를 확정한다.","blockReasonCodes":["SimulationCityServiceResultUnavailable"]} |
| 결과 | {"completionStateCodes":["CityServiceChoiceAvailable"],"effectCodes":["CityServiceChoiceAvailable"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"결과 확인은 멱등 처리한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이폐루프완결로드맵.md) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:city-demand-service-return.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["권위 Core·Save/Replay·Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["도심 수요·배정·서비스 결과의 표현 계약 이후 Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 계약만 있으며 권위 Core·Save/Replay·Fixture가 없다.. 기존 다음 작업은 자동 실행 지시가 아니다: 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-01 — NPC에게 반복 업무 배정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"독립 업무 Task를 만들지 않는다.","cancellationPolicy":"소비 Task 취소 계보로 배정을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":["h1-stock:farm-worker-waiting"],"placementVerified":false} |
| 나 | {"actorRequirements":["ActionCode별 NPC capability"],"controlPolicy":"WorldAutomatic"} |
| 너 | {"startStateCodes":["Available"],"resourceRequirements":["노동 용량"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"여러 WI가 적격 NPC를 결정적으로 선택하고 배정한다.","previewRule":"요구 역량·정책·현재 배정을 검사한다.","confirmRule":"소비 WI의 Confirm 안에서 원자적으로 배정한다.","blockReasonCodes":["SimulationNpcCapabilityMissing","SimulationNpcUnavailable"]} |
| 결과 | {"completionStateCodes":["Assigned"],"effectCodes":["NpcAssigned"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"소비 Task 취소 계보로 배정을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationNpcWorkforce.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-135, Q-136, Q-137, Q-138, Q-143, Q-144, Q-145, Q-146, Q-147, Q-148, Q-151, Q-152, Q-154, Q-155, Q-158, Q-159, Q-160, Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-378, Q-379, Q-380, Q-383, Q-384, Q-385, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-02 — NPC에게 업무 역량 위임

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 상태 전이 또는 정책 Task를 따른다.","cancellationPolicy":"철회는 별도 명령과 개정 검사를 사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["위임 권한 주체"],"controlPolicy":"NpcRoutine"} |
| 너 | {"startStateCodes":["NotGranted"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"승인된 주체가 NPC에 특정 업무 역량을 위임한다.","previewRule":"권한·대상·중복 위임을 검사한다.","confirmRule":"역량 부여 원장을 갱신한다.","blockReasonCodes":["NpcCapabilityGrantUnauthorized"]} |
| 결과 | {"completionStateCodes":["Granted"],"effectCodes":["NpcCapabilityGranted"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"철회는 별도 명령과 개정 검사를 사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationNpcWorkforce.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-135, Q-136, Q-137, Q-138, Q-143, Q-144, Q-145, Q-146, Q-147, Q-148, Q-151, Q-152, Q-154, Q-155, Q-158, Q-159, Q-160, Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-378, Q-379, Q-380, Q-383, Q-384, Q-385, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-03 — 진행 중 세계 업무 취소

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"새 업무 Task를 만들지 않고 대상 Task를 취소한다.","cancellationPolicy":"취소 명령 자체는 멱등 처리한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["취소 권한 주체"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Scheduled","Blocked"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"진행 전 작업의 계보 소유 예약과 임시 상태를 취소한다.","previewRule":"현재 Task lifecycle과 취소 가능성을 검사한다.","confirmRule":"CommandId·ExpectedRevision으로 취소를 확정한다.","blockReasonCodes":["SimulationTaskCancellationNotAllowed"]} |
| 결과 | {"completionStateCodes":["Cancelled"],"effectCodes":["TaskCancelled","ReservationsReleased"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"취소 명령 자체는 멱등 처리한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/Simulation작업취소.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-136, Q-143, Q-144, Q-146, Q-147, Q-155, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:farm-player-placement.v1 / PlayableUnit / 통합 E1, 궤적 {"logic":{"trackCode":"Logic","currentStage":"E3","targetStage":"E7","statusCode":"InProgress","evidencePackageRefs":["evidence:simulation-task-20260824"],"blockers":["Farm 전용 배치 규칙·Save 계보·권위 Fixture가 없다."]},"presentation":{"trackCode":"Presentation","currentStage":"E1","targetStage":"E7","statusCode":"Waiting","evidencePackageRefs":[],"blockers":["Preview·Confirm·배치 결과의 Farm Runtime 증거가 없다."]}}. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 현재 건물 WI의 실제 건설 상태 전이는 Nature 전용이며 Farm 배치는 별도 규칙·Scene 증거가 섞여 있다.. 기존 다음 작업은 자동 실행 지시가 아니다: Farm 핵심 생산 폐루프와 분리해 배치 Preview·Confirm·Tick·Save 계보를 전용 증거로 만든다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-04 — 손상된 시설 수리

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 시 내구도와 회복 가능 손상을 갱신한다.","cancellationPolicy":"미사용 자재와 공간을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["WorkerAccessible","RepairWorkArea"],"hRefs":["h1-stock:farm-maintenance-yard","h1-stock:farm-tool-storage","h1-stock:farm-work-yard","h1-stock:hub-service-maintenance","h1-stock:road-facility-access"],"placementVerified":false} |
| 나 | {"actorRequirements":["수리 가능 행위자"],"controlPolicy":"PlayerOrNpc"} |
| 너 | {"startStateCodes":["Damaged"],"resourceRequirements":["수리 자재","노동"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"손상된 시설의 회복 가능한 내구도를 수리한다.","previewRule":"손상·자재·행위자·공간을 검사한다.","confirmRule":"수리 작업과 자재를 예약한다.","blockReasonCodes":["RepairMaterialInsufficient"]} |
| 결과 | {"completionStateCodes":["Repaired"],"effectCodes":["FacilityRepaired"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"미사용 자재와 공간을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationFarmSurvival.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-135, Q-136, Q-137, Q-138, Q-143, Q-144, Q-145, Q-146, Q-147, Q-148, Q-151, Q-152, Q-154, Q-155, Q-158, Q-159, Q-160, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-05 — 새로운 지역 발견

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"완료 효과가 지역 발견 상태를 남긴다.","cancellationPolicy":"완료 전 행위자 예약을 반환한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":["Traversable"],"hRefs":["h1-stock:nature-exploration-buffer","h1-stock:nature-farm-edge","h1-stock:nature-lookout","h1-stock:nature-trailhead","h1-stock:town-living-square","h1-stock:town-neighborhood-service"],"placementVerified":false} |
| 나 | {"actorRequirements":["탐사 가능 행위자"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Undiscovered"],"resourceRequirements":["탐사 시간"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"행위자가 아직 발견되지 않은 지역을 탐사한다.","previewRule":"접근·발견 상태·행위자를 검사한다.","confirmRule":"탐사 활동을 확정한다.","blockReasonCodes":["RegionNotTraversable"]} |
| 결과 | {"completionStateCodes":["Discovered"],"effectCodes":["RegionDiscovered"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"완료 전 행위자 예약을 반환한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationWorldExploration.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-077, Q-078, Q-079, Q-080, Q-081, Q-082, Q-083, Q-084, Q-085, Q-086, Q-087, Q-088, Q-089, Q-090, Q-091, Q-092, Q-093, Q-094, Q-095, Q-096, Q-097, Q-098, Q-099, Q-100, Q-101, Q-102, Q-103, Q-104, Q-105, Q-106, Q-107, Q-108, Q-109, Q-110, Q-111, Q-112, Q-113, Q-114, Q-115, Q-116, Q-117, Q-118, Q-119, Q-120, Q-121, Q-126, Q-127, Q-128, Q-129, Q-130, Q-136, Q-143, Q-144, Q-146, Q-147, Q-155, Q-378, Q-379, Q-380, Q-383, Q-384, harvest-ready-grace-window. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-06 — 일행 역할 카드 장착

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"즉시 상태 전이한다.","cancellationPolicy":"해제는 별도 명령으로 처리한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["팀 역할 관리 권한"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Unequipped"],"resourceRequirements":["보유 카드"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"팀 역할 카드를 명시적으로 장착한다.","previewRule":"소유·슬롯·중복 상태를 검사한다.","confirmRule":"ExpectedRevision으로 장착 상태를 확정한다.","blockReasonCodes":["TeamRoleCardUnavailable"]} |
| 결과 | {"completionStateCodes":["Equipped"],"effectCodes":["TeamRoleCardEquipped"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"해제는 별도 명령으로 처리한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationTeamRoleCards.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-385. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-07 — 세계 활동 상태 변경

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"활동 상태를 서버가 소유한다.","cancellationPolicy":"활동 규칙에 따른 종료 명령을 사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":["h1-stock:nature-exploration-buffer","h1-stock:nature-shelter","h1-stock:nature-trailhead","h1-stock:town-staff-rest"],"placementVerified":false} |
| 나 | {"actorRequirements":["활동 관리 권한"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["Available","Active"],"resourceRequirements":["활동 조건"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"팀 활동을 시작하거나 종료한다.","previewRule":"활동 상태·조건·중복을 검사한다.","confirmRule":"시작 또는 종료 명령을 확정한다.","blockReasonCodes":["TeamActivityStateInvalid"]} |
| 결과 | {"completionStateCodes":["Active","Completed"],"effectCodes":["TeamActivityStarted","TeamActivityEnded"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"활동 규칙에 따른 종료 명령을 사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationTeamRoleCards.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: Q-161, Q-162, Q-163, Q-164, Q-165, Q-166, Q-167, Q-168, Q-169, Q-170, Q-171, Q-172, Q-173, Q-174, Q-175, Q-176, Q-177, Q-178, Q-179, Q-180, Q-181, Q-182, Q-183, Q-184, Q-185, Q-186, Q-187, Q-188, Q-189, Q-190, Q-191, Q-192, Q-193, Q-194, Q-195, Q-385. 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-WORLD-08 — 하루 운영 턴 마감

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / world-interaction.shared-policy.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"턴 효과와 후속 상태를 결정적으로 계산한다.","cancellationPolicy":"확정된 턴 마감은 취소하지 않고 후속 보정 명령을 사용한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["턴 마감 권한"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["TurnOpen"],"resourceRequirements":[],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"현재 턴의 결정·효과를 마감하고 다음 세계 상태를 연다.","previewRule":"미해결 필수 작업과 마감 조건을 검사한다.","confirmRule":"ExpectedRevision으로 턴 마감을 확정한다.","blockReasonCodes":["TurnClosingBlocked"]} |
| 결과 | {"completionStateCodes":["TurnClosed"],"effectCodes":["TurnClosed"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"확정된 턴 마감은 취소하지 않고 후속 보정 명령을 사용한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/세계상호작용단위중심공간Simulation통합.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- 참조 파일 없음: `Ssalddel.Simulation.Domain/SimulationTurnClosing.cs`. 원장 경로를 보존하며 구현 부재로 단정하지 않는다.

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

- playable-loop:solo-world-day.v1 / WorldAggregate / 통합 E1, 궤적 null. Loop 문맥이지 개별 WI 달성 판정이 아니다.
  - 기존 차단: 다섯 독립 영역 집계가 CoreClosed가 아니며 Solo 하루의 영역 선택·저장 반환 계약도 아직 E1이다.. 기존 다음 작업은 자동 실행 지시가 아니다: 영역 간 운송을 선행시키지 말고 각 영역 CoreClosed 뒤 하루 선택 집계를 조립한다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.

## WI-REVIEW-01 — NPC 업무 결과 검토 확정

원문: [공식 WI 대장](../../../eng/execution-ledgers/world-interactions.json) / player-domain-progress.r1. ExistingCatalogProjectionNotFullMeaningReview. 주제 Q 연결은 정확 질문별 구현 증거가 아니다.

| 읽기 항목 | 기존 기록 |
| --- | --- |
| 지금 | {"taskRule":"검토는 즉시 완료하며 NPC의 실제 작업 결과를 다시 실행하지 않는다.","cancellationPolicy":"Confirm 전에는 상태를 바꾸지 않으며 완료된 검토는 같은 CommandId로 멱등 조회한다.","authorityReview":"NotReviewed"} |
| 여기 | {"requirements":[],"hRefs":[],"placementVerified":false} |
| 나 | {"actorRequirements":["위임을 발의한 플레이어"],"controlPolicy":"PlayerDirect"} |
| 너 | {"startStateCodes":["NpcWorkCompleted","ReviewPending"],"resourceRequirements":["위임 행위 기록","NPC 완료 행위 기록"],"identityReview":"NotReviewed"} |
| 이렇게 | {"worldAction":"플레이어가 자신이 위임한 NPC 업무의 완료 결과와 계보를 검토해 운영 학습 근거를 확정한다.","previewRule":"플레이어·위임·NPC 완료 기록의 연결과 중복 검토 여부를 읽기 전용으로 검사한다.","confirmRule":"ExpectedRevision과 CommandId로 검토 결과와 운영 숙련 효과를 한 번만 확정한다.","blockReasonCodes":["NpcWorkReviewLineageInvalid","NpcWorkReviewUnauthorized","NpcWorkReviewAlreadyConfirmed"]} |
| 결과 | {"completionStateCodes":["NpcWorkReviewConfirmed"],"effectCodes":["NpcWorkReviewConfirmed","PlayerOperationalProficiencyChanged"]} |
| 다음 선택 | {"successorWiIds":[],"cancellationPolicy":"Confirm 전에는 상태를 바꾸지 않으며 완료된 검토는 같은 CommandId로 멱등 조회한다.","meaning":"RecordedRelationsNotMandatoryRoute"} |

원문/코드·시험 참조(현재 실행 검증 아님):
- [원문/소스](../../../docs/Architecture/플레이어중심게임개발업무구조.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../docs/Architecture/WI단일책임원칙.md) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationActionManifestationLedger.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님
- [원문/소스](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationPlayerDomainProficiency.cs) / 파일 존재·hash 확인, 구현/시험 검증 아님

문답 주제 문맥: . 빈 목록은 직접 근거 부재 확정이 아닌 미연결이다.

이번 준비 판정: NotAssessed. 명세·승인·Required 연구·실제 코드/시험/후보의 동일 판본 대조가 남았다. Scene/Unity 검증을 수행하지 않았다.
