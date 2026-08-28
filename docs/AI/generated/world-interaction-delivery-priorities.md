# WI 행동 단위 E7 실행 우선순위

> 이 문서는 `eng/execution-ledgers/world-interaction-delivery-priorities.json`와 WI·폐루프·H 상태 대장에서 자동 생성된다. 직접 수정하지 않는다.

- 실행 우선순위 개정: `world-interaction-delivery-priorities.r26`
- 전체 WI: `66`
- 진행 방식: `SingleWorldInteractionVertical` / 작업 중 한도 `1`
- 현재 활성 WI: `WI-ACTOR-03` / `E5` → `E7`
- Synty H1 설계 재고: `84`
- E7은 최신 PlayMode·Game View·Hosted 동등성 증거가 있을 때만 승격한다.

## 현재 단일 WI 증거 관문

| E 단계 | 판정 | 정제·검증 요약 |
| --- | --- | --- |
| E4 | Passed | LocalSimulationRuntime과 RemoteHost HTTP Adapter가 같은 Application 서비스를 호출하며 Query·Preview·Confirm 결과가 일치한다. |
| E5 | Pending | 지식 출처의 실제 H 결속과 같은 revision 표현은 후속 승인 범위다. |
| E6 | Pending | 플레이어 피드백 정제는 표현 문답과 자산 연구 뒤 수행한다. |
| E7 | Pending | 실제 입력·Play Mode·Game View는 이번 인계에서 수행하지 않는다. |

## D1 Nature 행동 폐루프

- 진입: 현재 E5 황혼 대응부터 시작해 생활거점·Day2·현장 보급을 순서대로 닫는다.
- 완료: Nature 필수 Core가 E7 PlayClosed이고 선택 Extension의 상태가 별도로 표시된다.

| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 물품 획득<br>`WI-ACTOR-01` | Queued | Core | E3 | E5 | E6 | Conditional | NotApplicable | Shared | playable-loop:nature-shelter-foundation.v1 |
| 2 | 장착 상태 변경<br>`WI-ACTOR-02` | Queued | Core | E3 | E5 | E6 | Conditional | NotApplicable | Shared | playable-loop:nature-shelter-foundation.v1 |
| 3 | 벌목 도끼 획득<br>`WI-NATURE-05` | E7Closed | Core | E3 | E7 | Complete | NotApplicable | EstablishedH1 | Nature | playable-loop:nature-shelter-foundation.v1<br>playable-loop:nature-tactical-self-navigation.v1 |
| 4 | 나무 벌목 작업 시작<br>`WI-NATURE-06` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Nature | playable-loop:nature-shelter-foundation.v1 |
| 5 | 오두막을 지을 터 선정<br>`WI-NATURE-07` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Construction | playable-loop:nature-shelter-foundation.v1 |
| 6 | 오두막 건설 작업 시작<br>`WI-NATURE-08` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Construction | playable-loop:nature-shelter-foundation.v1 |
| 7 | 진행 중 작업 취소<br>`WI-NATURE-12` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Construction | playable-loop:nature-shelter-foundation.v1 |
| 8 | 오두막 안으로 들어가기<br>`WI-NATURE-09` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Nature | playable-loop:nature-shelter-foundation.v1 |
| 9 | 오두막 밖으로 나가기<br>`WI-NATURE-10` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Nature | playable-loop:nature-shelter-foundation.v1 |
| 10 | 자연 지역 위험 징후 확인<br>`WI-NATURE-01` | E7Closed | Core | E3 | E7 | Complete | NotApplicable | EstablishedH3 | Nature | playable-loop:nature-twilight-return.v1 |
| 11 | 황혼 위협 대응 방식 확정<br>`WI-NATURE-11` | E7Closed | Core | E3 | E7 | Complete | NotApplicable | EstablishedH3 | Nature | playable-loop:nature-twilight-return.v1 |
| 12 | 안전 거점으로 긴급 후퇴<br>`WI-NATURE-02` | Queued | Extension | E3 | E1 | E2 | NotApplicable | CandidateLineage | Nature | playable-loop:nature-regional-threat-recovery.v1 |
| 13 | 훼손된 자연 경로 복원<br>`WI-NATURE-03` | Queued | Extension | E3 | E1 | E2 | NotApplicable | CandidateLineage | Nature | playable-loop:nature-regional-threat-recovery.v1 |
| 14 | 탐사대 안전 회복<br>`WI-NATURE-04` | Queued | Extension | E3 | E1 | E2 | NotApplicable | CandidateLineage | Nature | playable-loop:nature-regional-threat-recovery.v1 |
| 15 | 획득 자원 거점 보관<br>`WI-NATURE-13` | E7Closed | Core | E3 | E7 | Complete | NotApplicable | EstablishedH3 | Nature | playable-loop:nature-night-day2.v1 |
| 16 | 오두막에서 수면·새벽 맞기<br>`WI-NATURE-14` | Queued | Core | E3 | E5 | E6 | NotApplicable | EstablishedH3 | Nature | playable-loop:nature-night-day2.v1 |
| 17 | 다음 날 거점 확장 계획 선택<br>`WI-NATURE-15` | Queued | Core | E3 | E6 | E7 | NotApplicable | EstablishedH3 | Nature | playable-loop:nature-night-day2.v1 |
| 18 | 영역 건물 건설 확정<br>`WI-CON-01` | E7Closed | Shared | E3 | E7 | Complete | NotApplicable | EstablishedH3 | Construction | playable-loop:nature-workbench-foundation.v1<br>playable-loop:nature-building-learning.v1<br>playable-loop:farm-player-placement.v1 |
| 19 | 현장 보급 꾸러미 제작<br>`WI-NATURE-16` | Queued | Core | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Nature | playable-loop:nature-field-supply-return.v1 |
| 20 | 현장 보급 제작 업무 위임<br>`WI-NATURE-17` | Queued | Core | E3 | E4 | E5 | Required | EstablishedH1 | Nature | playable-loop:nature-field-supply-return.v1 |
| 21 | 승인 자료로 거점 성찰 확정<br>`WI-REFLECT-01` | Queued | Extension | E3 | E3 | E4 | NotApplicable | MissingRequired | Nature | playable-loop:nature-base-reflection.v1 |
| 22 | 벌목 통나무 줍기<br>`WI-NATURE-18` | E7Closed | Core | E3 | E7 | Complete | NotApplicable | EstablishedH3 | Nature | playable-loop:nature-shelter-foundation.v1 |
| 23 | 지식 습득<br>`WI-ACTOR-03` | Active | Core | E3 | E5 | E6 | NotApplicable | NotApplicable | Shared | playable-loop:nature-basic-herbal-recovery.v1 |

## D2 Farm 독립 생산 폐루프

- 진입: Nature Core E7 뒤 기존 E6 Farm 공간 결속을 실제 생산 입력으로 검증한다.
- 완료: 경작부터 포장·내부 보관 반환까지 Farm Core가 E7 PlayClosed다.

| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 경작지 밭갈이<br>`WI-FARM-01` | Queued | Core | E3 | E6 | E7 | Conditional | EstablishedH3 | Farm | playable-loop:farm-crop-cycle.v1 |
| 2 | 경작지 씨앗 파종<br>`WI-FARM-02` | Queued | Core | E3 | E6 | E7 | Conditional | EstablishedH3 | Farm | playable-loop:farm-crop-cycle.v1 |
| 3 | 농작물 생육 관리<br>`WI-FARM-03` | Queued | Core | E3 | E6 | E7 | Conditional | EstablishedH3 | Farm | playable-loop:farm-crop-cycle.v1 |
| 4 | 익은 농작물 수확<br>`WI-FARM-04` | Queued | Core | E3 | E6 | E7 | Conditional | EstablishedH3 | Farm | playable-loop:farm-crop-cycle.v1 |
| 5 | 수확물 집하장 모으기<br>`WI-FARM-05` | Queued | Core | E3 | E6 | E7 | Conditional | EstablishedH3 | Farm | playable-loop:farm-pack-store-return.v1 |
| 6 | 출하 물량 포장<br>`WI-FARM-06` | Queued | Core | E3 | E6 | E7 | Conditional | EstablishedH3 | Farm | playable-loop:farm-pack-store-return.v1 |

## D3 Hub 독립 창고 NPC 폐루프

- 진입: Farm과 화물을 연결하지 않고 Hub 자체 300 KGM 입고 Fixture를 사용한다.
- 완료: 검수·적재·피킹·포장·출고 준비와 실제 NPC 통행이 E7에서 닫힌다.

| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 입고 화물 검수<br>`WI-001` | Queued | Core | E3 | E4 | E5 | Required | EstablishedH1 | Hub | playable-loop:hub-inbound-putaway.v1 |
| 2 | 검수 완료 화물 창고 적재<br>`WI-002` | Queued | Core | E3 | E4 | E5 | Required | EstablishedH1 | Hub | playable-loop:hub-inbound-putaway.v1 |
| 3 | 출고 대상 재고 요청<br>`WI-HUB-03` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Hub | playable-loop:hub-outbound-ready-return.v1 |
| 4 | 출고 대상 재고 피킹<br>`WI-HUB-04` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Hub | playable-loop:hub-outbound-ready-return.v1 |
| 5 | 피킹 화물 포장<br>`WI-HUB-05` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Hub | playable-loop:hub-outbound-ready-return.v1 |

## D4 Town 주민 생활복구 폐루프

- 진입: 타로가 없는 주문·소비 Core를 먼저 닫고 카드 문맥은 선택 확장으로 분리한다.
- 완료: 욕구·주문·경쟁·소비·다음 욕구 Core와 카드 Extension의 E7 상태가 분리된다.

| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 주민 주문 확정<br>`WI-ORDER-01` | Queued | Core | E3 | E1 | E2 | Required | NotApplicable | Town | playable-loop:town-order-consume-return.v1 |
| 2 | 주문 상품 재고 예약<br>`WI-ORDER-02` | Queued | Core | E3 | E1 | E2 | NotApplicable | NotApplicable | Town | playable-loop:town-order-consume-return.v1 |
| 3 | 주문 상품 피킹<br>`WI-ORDER-03` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Town | playable-loop:town-order-consume-return.v1 |
| 4 | 주문 상품 포장<br>`WI-ORDER-04` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Town | playable-loop:town-order-consume-return.v1 |
| 5 | 주문 상품 수령 준비<br>`WI-ORDER-05` | Queued | Core | E3 | E1 | E2 | NotApplicable | CandidateLineage | Town | playable-loop:town-order-consume-return.v1 |
| 6 | 주민 주문 상품 수령<br>`WI-ORDER-06` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Town | playable-loop:town-order-consume-return.v1 |
| 7 | 주민 상품 소비<br>`WI-ORDER-07` | Queued | Core | E3 | E1 | E2 | Required | CandidateLineage | Town | playable-loop:town-order-consume-return.v1 |
| 8 | 현재 세계의 메이저 아르카나 활성화<br>`WI-CARD-01` | Queued | Extension | E3 | E4 | E5 | NotApplicable | NotApplicable | Town | playable-loop:town-arcana-context.v1 |

## D5 City 독립 주민 서비스 폐루프

- 진입: 누락된 City H1 네 종류와 권위 명령 Aggregate를 먼저 만든다.
- 완료: 수요·배정·서비스·결과 확인이 독립 City Fixture로 E7 PlayClosed다.

| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 도심 서비스 수요 확정<br>`WI-CITY-01` | Queued | Core | E1 | E1 | E2 | Required | MissingRequired | City | playable-loop:city-demand-service-return.v1 |
| 2 | 도심 서비스용 지역 재고 배정<br>`WI-CITY-02` | Queued | Core | E1 | E1 | E2 | NotApplicable | MissingRequired | City | playable-loop:city-demand-service-return.v1 |
| 3 | 도심 주민 서비스 처리<br>`WI-CITY-03` | Queued | Core | E1 | E1 | E2 | Required | MissingRequired | City | playable-loop:city-demand-service-return.v1 |
| 4 | 도심 서비스 결과 확인<br>`WI-CITY-04` | Queued | Core | E1 | E1 | E2 | Required | MissingRequired | City | playable-loop:city-demand-service-return.v1 |

## D6 후속 세계·운송 통합

- 진입: 관련 독립 영역이 모두 E7 PlayClosed일 때만 통합 경로를 연다.
- 완료: 영역 간 운송·마트·공통 세계 운영을 별도 통합 PlayableLoop로 검증한다.

| 순서 | 한국어 행동명 · 고유 식별자 | 작업 | 역할 | 현재 구현 | 현재 통합 | 다음 관문 | NPC E8 | H 상태 | Synty | 폐루프 |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 출고 차량 상차<br>`WI-HUB-06` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | CandidateLineage | Hub | 후속 정의 |
| 2 | 출하 차량 상차 확정<br>`WI-LOG-01` | Deferred | DeferredIntegration | E3 | E6 | E7 | Required | EstablishedH3 | Integration | 후속 정의 |
| 3 | 농장에서 출발<br>`WI-LOG-02` | Deferred | DeferredIntegration | E3 | E6 | E7 | NotApplicable | EstablishedH3 | Integration | 후속 정의 |
| 4 | 농장에서 물류 거점으로 화물 이동<br>`WI-LOG-03` | Deferred | DeferredIntegration | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Integration | 후속 정의 |
| 5 | 물류 거점 도착 화물 하차<br>`WI-LOG-04` | Deferred | DeferredIntegration | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Integration | 후속 정의 |
| 6 | 물류 거점 도착 화물 인수<br>`WI-LOG-05` | Deferred | DeferredIntegration | E3 | E4 | E5 | NotApplicable | EstablishedH1 | Integration | 후속 정의 |
| 7 | 물류 거점에서 마트로 운송<br>`WI-MARKET-01` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | CandidateLineage | Town | 후속 정의 |
| 8 | 마트 도착 화물 인수<br>`WI-MARKET-02` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | CandidateLineage | Town | 후속 정의 |
| 9 | 마트 입고 상품 검수<br>`WI-MARKET-03` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | CandidateLineage | Town | 후속 정의 |
| 10 | 검수 상품 후방 창고 적재<br>`WI-MARKET-04` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | CandidateLineage | Town | 후속 정의 |
| 11 | 매장 진열대 상품 보충<br>`WI-MARKET-05` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | CandidateLineage | Town | 후속 정의 |
| 12 | NPC에게 반복 업무 배정<br>`WI-WORLD-01` | Deferred | DeferredIntegration | E3 | E1 | E2 | NotApplicable | CandidateLineage | None | 후속 정의 |
| 13 | NPC에게 업무 역량 위임<br>`WI-WORLD-02` | Deferred | DeferredIntegration | E3 | E1 | E2 | Required | NotApplicable | None | 후속 정의 |
| 14 | 진행 중 세계 업무 취소<br>`WI-WORLD-03` | Deferred | DeferredIntegration | E3 | E1 | E2 | NotApplicable | NotApplicable | Construction | playable-loop:farm-player-placement.v1 |
| 15 | 손상된 시설 수리<br>`WI-WORLD-04` | Deferred | DeferredIntegration | E3 | E1 | E2 | Conditional | CandidateLineage | Construction | 후속 정의 |
| 16 | 새로운 지역 발견<br>`WI-WORLD-05` | Deferred | DeferredIntegration | E3 | E1 | E2 | NotApplicable | CandidateLineage | Integration | 후속 정의 |
| 17 | 일행 역할 카드 장착<br>`WI-WORLD-06` | Deferred | DeferredIntegration | E3 | E1 | E2 | NotApplicable | NotApplicable | None | 후속 정의 |
| 18 | 세계 활동 상태 변경<br>`WI-WORLD-07` | Deferred | DeferredIntegration | E3 | E1 | E2 | NotApplicable | CandidateLineage | None | 후속 정의 |
| 19 | 하루 운영 턴 마감<br>`WI-WORLD-08` | Deferred | DeferredIntegration | E3 | E1 | E2 | NotApplicable | NotApplicable | None | playable-loop:solo-world-day.v1 |
| 20 | NPC 업무 결과 검토 확정<br>`WI-REVIEW-01` | Deferred | DeferredIntegration | E2 | E1 | E2 | NotApplicable | NotApplicable | None | 후속 정의 |
