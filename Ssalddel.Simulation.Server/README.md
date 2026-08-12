# Ssalddel Simulation Server

Unity 경영 Simulation의 session, scenario clock와 deterministic command 권위를 기존 운영 서버에서 분리하는 별도 ASP.NET Core host다.

이 서버는 실제 운영 전 게임 세계와 업무 규칙을 반복 검증하는 예행연습 서버 역할을 함께 맡는다. 개발 기본 주소는 `http://localhost:5204`이며, 기존 `Ssalddel` 운영 서버의 개발 주소와 분리한다. 예행연습 결과는 실제 운영 원장으로 자동 승격하지 않는다.

현재 첫 slice는 다음만 제공한다.

- Simulation session 생성·조회
- expected revision 기반 Tick 진행
- `CommandId` 멱등 재시도
- scenario seed, Data revision과 rule revision 보존
- 세력·영지·정착지 stable ID를 가진 공통 World context
- `1 Tick = 1 Game Day` 규칙의 `WorldTick`, `WorldRevision`, `GameDate`
- 상태를 바꾸지 않는 Decision Preview와 명시적 Confirm
- `Decision → Task → Effect` 인과 원장과 Tick 기반 작업 완료
- `simulation-save.v1` snapshot·append-only Command log·SHA-256 replay hash
- snapshot 직접 주입 없이 Command를 재실행하는 restore port
- scenario가 명시한 정착지 District·Facility graph와 재정·노동·창고·시장·비축 snapshot
- outbound 예약 식량을 제외한 Fixture `FoodSecurityDays` 계산
- 수확물의 조합 출하·온라인 직판·수출 대행·비축 판로별 영향 Preview와 Confirm
- 창고 capacity·2% Fixture 감모·FoodEquivalent 근거에 따른 비축·FoodSecurityDays 후보
- Confirm 시 수확 Lot 단일 allocation과 labor·treasury·storage capacity 예약
- Task 완료 Tick의 비용·Simulation 수입·시장 공급·비축 Stock Lot 원자적 반영
- 수확 Lot 중복 배정 차단과 판로 전용 Command save/replay
- Cargo stable ID·HarvestLot·PackageLot lineage를 보존하는 물류 이동 Preview와 Confirm
- 원천 allocation 재고 예약과 공통 WorldTick 기반 출발·진행·도착
- 도착 뒤 Hub 검수 전 `DestinationStockCandidate` 경계와 물류 Command save/replay
- 같은 Cargo에 결합된 화물운송 의뢰·배차 후보·가상 차량 용량과 상차·운송·하차 상태 이력
- 목적지 도착 뒤 별도 Preview·Confirm·WorldTick을 요구하는 화물 인수 완료
- 참여자별 명시적 의향·수량·동의를 보존하는 같이주문 모집 결과 Preview와 Confirm
- 목표 충족의 `확정`, 목표 미달의 `모집종료목표미달` WorldTick 전이
- 감자 시장재고 300kg을 기준으로 한 개별주문 20kg Preview·재고/노동 예약·포장 Task·수령준비
- 수령준비 전 주문 취소의 원래 Task/Effect 취소와 재고·노동 예약 반환
- 최대 duration을 넘는 Tick 차단

이 서버는 `Ssalddel`, `Ssalddel.Contracts`, `Ssalddel.Domain`, `Ssalddel.Infrastructure`와 `Ssalddel.Unity`를 참조하지 않는다. 실제 계약·발주·결제·입고·재고를 만들지 않으며 두 서버 사이에 공유 DB도 없다.

API는 기본 비활성이다. 승인된 Simulation 환경에서만 `SimulationServer:Enabled=true`로 켜며 `SsalddelExecution:Mode=Simulation`이 아니면 host 시작을 거부한다. 현재 session store와 save store는 모두 프로세스 수명에 한정된 in-memory 구현이다. restore port는 구현됐지만 외부 durable adapter를 연결하지 않은 현재 host는 실제 프로세스 재시작 뒤 save를 읽을 수 없다.

```text
POST /api/simulation/v1/sessions
GET  /api/simulation/v1/sessions/{sessionStableId}
POST /api/simulation/v1/sessions/{sessionStableId}/ticks
POST /api/simulation/v1/sessions/{sessionStableId}/decision-previews
POST /api/simulation/v1/sessions/{sessionStableId}/decisions/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/harvest-disposition-impact-previews
POST /api/simulation/v1/sessions/{sessionStableId}/harvest-disposition-impacts/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/logistics-movement-previews
POST /api/simulation/v1/sessions/{sessionStableId}/logistics-movements/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/freight-transport-previews
POST /api/simulation/v1/sessions/{sessionStableId}/freight-transports/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/freight-receipt-previews
POST /api/simulation/v1/sessions/{sessionStableId}/freight-receipts/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/group-order-previews
POST /api/simulation/v1/sessions/{sessionStableId}/group-orders/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/individual-order-previews
POST /api/simulation/v1/sessions/{sessionStableId}/individual-orders/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/individual-order-cancellation-previews
POST /api/simulation/v1/sessions/{sessionStableId}/individual-order-cancellations/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/saves
POST /api/simulation/v1/sessions/restores
GET  /health
```

Preview의 예상값은 카드용 Interpretation이며 원장을 변경하지 않는다. 일반 Decision Confirm은 `Confirmed Decision`, `Scheduled Task`, `Pending Effect`를 분리해 기록한다. 수확 판로 Confirm은 여기에 `HarvestLotAllocation=Reserved`를 더하고 labor·treasury와 비축 선택의 storage capacity를 예약한다. 실제 Tick에서 Task·Effect와 allocation을 완료하면서 비용과 Simulation 수입, 시장 공급 또는 비축 Stock Lot을 한 aggregate lock 안에서 적용하고 예약을 해제한다.

save package는 session 생성 요청, 저장 시점 snapshot과 Confirm/Tick Command log를 함께 보존한다. restore는 새 aggregate에 Command를 순서대로 재실행하고 package 자체 hash와 replay 결과 hash가 모두 일치할 때만 session store에 등록한다. 실패 복원은 활성 session을 만들지 않으며 이미 활성인 같은 session도 덮어쓰지 않는다.

정착지 초기 상태는 선택적 session 생성 입력이다. 제공된 scenario에서만 별도 재정·노동·storage·시장 공급·비축 Lot·주민/주둔군 수요를 투영하며 화면의 상자·NPC·건물 크기로 수량을 만들지 않는다. `TreasuryReserved/Available`, `StorageReserved/Available`, `LaborReserved/Available`은 확정된 미완료 작업의 capacity를 포함한다. `FoodSecurityDays`는 명시적 Fixture 환산 rule revision에 따른 게임 지표이며 실제 영양 처방이 아니다.

수확 판로 영향은 기존 Unity choice code와 결정 stable ID·revision을 보존하되 서버가 비용·노동·기간·예상 수입·시장·식량 후보를 다시 계산한다. 네 판로를 점수화하거나 자동 선택하지 않는다. 같은 HarvestLot은 최초 Confirm 이후 다른 판로로 다시 배정할 수 없다. 조합·수출은 비용과 Simulation 수입을, 직판은 여기에 시장 공급을, 비축은 감모 뒤 Stock Lot과 FoodEquivalent를 완료 Tick에 반영한다. 이는 Simulation 결과이며 실제 계약·판매·수출·정산을 뜻하지 않는다.

물류 이동 Preview는 정착지와 원천 HarvestLot allocation을 검증하되 상태를 바꾸지 않는다. Confirm은 같은 Cargo 300kg을 원천 allocation에 예약하고 공통 Task를 생성하며, WorldTick만 `Reserved → InTransit → ArrivedAtDestination`을 확정한다. 도착 결과는 검수 전 재고 후보이므로 실제 Hub 재고·운송·입고·정산을 만들지 않는다.

화물운송은 이 물류 이동을 복제하지 않고 같은 Cargo에 운송 의뢰와 가상 배차·차량 용량을 결합한다. 이동 Tick은 상차·운송·하차 도착 상태 이력을 남기지만 `ArrivedAtDestination`만으로 인수완료를 만들지 않는다. 별도 인수 Preview·Confirm 뒤 Task 완료 Tick에서만 `인수완료`가 된다. 실제 기사·GPS·운임 정산·알림은 수행하지 않는다.

같이주문 Preview는 각 참여자의 명시적 동의와 의향 수량을 보존하고 목표 충족 여부를 계산하지만 상태를 바꾸지 않는다. 모집 결과 Confirm은 공통 Task를 만들고, 완료 WorldTick에서만 목표 충족 모집을 `확정`, 미달 모집을 `모집종료목표미달`로 전이한다. 같은 참여자의 중복 의향은 차단하고 실제 주문·결제·자동 동의는 수행하지 않는다.

다음 slice는 `SIM-FOOD-DELIVERY-1`로, 조리·픽업·전달·수령 확인을 실제 주소·기사·결제 없이 Simulation 주문 원장과 WorldTick에 연결한다. 그 뒤 `MARKET-CONSUMPTION-1`로 진행한다. 인증된 session scope, scenario package validator, 외부 durable save adapter와 migration은 별도 후속 Gate다.
