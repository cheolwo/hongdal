# WI 전체 게임플레이 그래프

> 기준일: 2026-08-25
> 기계 권위: [`world-interactions.json`](../../../eng/execution-ledgers/world-interactions.json) `simulation-world-interactions.r16`
> 단일 책임 권위: [`world-interaction-responsibilities.json`](../../../eng/execution-ledgers/world-interaction-responsibilities.json) — 한 의도·주요 결과·다중 효과 감사와 복합 책임 migration을 판정한다.
> 조립 흐름 권위: [`world-interaction-flows.json`](../../../eng/execution-ledgers/world-interaction-flows.json) — 여러 WI의 선택 가능한 순서·분기·반복을 소유한다.
> 음양·수행주체 권위: [`world-interaction-polarity-quadrants.json`](../../../eng/execution-ledgers/world-interaction-polarity-quadrants.json) — 행동 목적과 실제 Actor를 실행별 사분면으로 분류하며 수치 효과를 만들지 않는다.
> 폐루프 권위: [`playable-loops.json`](../../../eng/execution-ledgers/playable-loops.json) — 여러 WI의 성공·실패·회복·귀환과 폐루프 전체 E를 별도로 판정한다.
> 현재 재고: 60개 WI. 이 중 City 4개는 E1 계약이고 나머지 구현·통합 단계는 기계 대장의 항목별 상태를 따른다. E4 실행 문맥·E5 세계 발현 상태는 별도 결속과 EvidencePackage에서 판정한다.

## 읽는 순서

```text
플레이 목적
  → WI 상태 전이
  → Preview / Confirm / Task / Effect
  → 필요한 H 공간
  → 해당 WI의 E 증거
  → 필요한 G 관리 작업
```

WI 자체는 선행·후속 절차를 소유하지 않는다. 연결선은 별도 흐름 대장이 조립한 선택 가능한 경로다. Farm→Hub→Town 연결은 **통합 후보**이지 개발 우선순위가 아니다. Farm·Hub·Town은 각각 독립 내부 폐루프와 독립 Fixture·Save/Replay를 먼저 가져야 한다.

사람이 읽는 이름은 한국어 기능명을 먼저 쓰고 안정 고유 식별자를 뒤에 붙인다. 예시는 `자연 탐사·생활 거점 · 오두막을 지을 터 선정 (WI-NATURE-07)`이다. 대장 순번은 실행 단계가 아니므로 이름에 붙이지 않는다. 정확한 60개 이름과 단일 책임 판정은 [세계 상호작용 단위 대장](../generated/world-interaction-catalog.md)을 따른다.

## 첫 기준 플레이: Nature 생존 생활거점

```text
Nature 안전 빈터
 → WI-NATURE-05 도끼 획득
 → WI-ACTOR-02 MainHand 장착
 → WI-NATURE-06 수확 허용 resource node 벌목 시작
 → 통나무 6개 확보
 → WI-NATURE-07 오두막 위치 배치
 → WI-NATURE-08 오두막 건설 시작
 → WI-NATURE-09/10 오두막 입장·퇴장
 → 황혼 소음 기반 조우
 → WI-NATURE-11 싸움·후퇴 선택
 → WI-NATURE-12 진행 중 벌목·건설 취소
 → WI-NATURE-04 회복·다음 날 재출발
 → WI-NATURE-13 오두막 보관
 → WI-NATURE-14 수면
 → WI-NATURE-15 다음 확장 계획
 → 선택형 WI-CON-01 영역 건물 건설
```

플레이어가 의도적으로 선택하고 권위 상태를 바꾸는 도끼·벌목·오두막·조우 대응·진행 작업 취소는 정식 WI다. 벌목 4초, 건설 30초, 황혼 조우 발생과 완공 결과는 WI가 아니라 Task·자동 상태 전이·Effect로 관리한다. 취소는 진행률만 멈추는 입력이 아니라 점유를 해제하고 예약 자원을 반환하는 권위 전이이므로 별도 WI다.

## 선택적 기준 통합: Nature↔Farm 왕복

```text
Nature 생활핵에서 준비·회복
  ├─ WI-NATURE-01 위협 관찰
  │    ├─ WI-NATURE-02 안전핵 후퇴 → WI-NATURE-04 회복
  │    └─ WI-NATURE-03 원인 해결·경로 복구 → WI-NATURE-04 회복
  └─ Farm으로 이동
       WI-FARM-01 밭갈기
        → 02 파종
        → 03 관수·재배
        → 04 수확
        → 05 집하
        → 06 포장·출하 준비
       → Farm 내부 보관·다음 생산 조건으로 귀환
  → Nature 생활핵으로 귀환·다음 날 선택
```

현재 Farm 공간 자료는 일부 WI에서 E6 정제까지 연결됐지만 이것만으로 WI E6를 주장하지 않는다. 결속 대장 범위의 Farm 04~06은 E4 문맥과 E5 부분 발현이 결속돼 있다. `WI-ACTOR-01`·`02`는 보편 행위라 공간이 `NotApplicable`이고 E5 권위 발현까지 자동 검증됐다. Nature 01~04는 E4 부분 결속·E5 부분 발현이고, Nature 특화 WI는 필요한 경우 승인 H와 실제 E5 배치에 결속한다. 기존 canonical Scene E7 증거는 도끼 자동 장착 기준이므로 명시적 장착 추가 뒤 실제 입력·Game View를 다시 검증해야 한다.

## Farm 생산 폐루프

| WI | 상태 전이 | 대표 결과·Effect | 연결 H1 | 다음 WI | 증거 |
| --- | --- | --- | --- | --- | --- |
| WI-FARM-01 밭갈기 | Untilled→Tilled | SoilTilled | farm-production | FARM-02 | E3 / 공간 E6 |
| WI-FARM-02 파종 | Tilled→Growing | CultivationStarted | farm-production | FARM-03 | E3 / 공간 E6 |
| WI-FARM-03 관수·재배 | Growing→Growing·HarvestReady | CropCareApplied | farm-production | FARM-04 | E3 / 공간 E6 |
| WI-FARM-04 수확 | HarvestReady→HarvestedAtField | HarvestLotCreated, CultivationHarvested | farm-production | FARM-05 | E3 / 공간 E6 |
| WI-FARM-05 집하 | HarvestedAtField→CollectedAtYard | HarvestLotCollected | farm-work-yard | FARM-06 | E3 / 공간 E6 |
| WI-FARM-06 포장 | CollectedAtYard→PackedForShipment | PackageLotCreated, CargoPrepared | farm-work-yard | LOG-01 | E3 / 공간 E6 |

Farm의 첫 독립 완료는 LOG-01로 반드시 넘어가는 것이 아니다. 포장 Lot을 Farm 내부에 저장하고 다음 생산·복구 선택으로 되돌아오는 반환 계약을 E7 전에 명시해야 한다.

## 선택적 영역 간 물류 통합

| WI | 상태 전이 | 대표 Effect | H 참조 | 다음 WI | 증거 |
| --- | --- | --- | --- | --- | --- |
| WI-LOG-01 차량 적재 | Prepared→Reserved | CargoTransportReserved | farm-loading-gate | LOG-02 | E3 / 공간 E6 |
| WI-LOG-02 출발 | Reserved→InTransit | CargoDeparted | farm-loading-gate | LOG-03 | E3 / 공간 E6 |
| WI-LOG-03 노선 이동 | InTransit→InTransit·Arrived | CargoRouteProgressed | farm-hub-corridor | LOG-04 | E3 / 공간 E4 |
| WI-LOG-04 Hub 하역 | InTransit→Arrived | CargoUnloaded | hub-receiving-storage | LOG-05 | E3 / 공간 E4 |
| WI-LOG-05 입고 인계 | Arrived·PendingInspection→Received·StorageEligible | FreightReceived, StorageEligible | hub-receiving-storage | WI-001 | E3 / 공간 E4 |

## Hub 독립 업무와 후속 연결

| WI | 행위 | 다음 WI | 공간 통합 |
| --- | --- | --- | --- |
| WI-001 | 입고 검수·작업공간 해제 | WI-002 | hub-receiving-storage / E4 |
| WI-002 | 적치·용량 소비 | WI-HUB-03 | hub-receiving-storage / E4 |
| WI-HUB-03 | 출고 요청 | WI-HUB-04 | 미결 / E1 |
| WI-HUB-04 | 피킹 | WI-HUB-05 | 미결 / E1 |
| WI-HUB-05 | 출고 준비 | WI-HUB-06 | 미결 / E1 |
| WI-HUB-06 | 상차 | WI-MARKET-01 | 미결 / E1 |

Hub의 독립 폐루프는 입고·검수·적치·내부 재배치·보관 복구·독립 출고 Fixture로 닫아야 한다. Town 연결은 별도 통합 slice다.

## Town 시장·주문 후보 사슬

```text
WI-MARKET-01 시장 운송 도착
 → 02 하역·수령
 → 03 검사
 → 04 후방 보관
 → 05 진열
 → WI-ORDER-01 주문 확정
 → 02 재고 예약
 → 03 피킹
 → 04 포장
 → 05 수령 준비
 → 06 이행
 → 07 소비
```

`WI-CARD-01` 메이저 아르카나 활성화는 이 사슬의 선택형 상위 문맥이다. 타로가 없어도 ORDER 01~07 Core는 Neutral 문맥으로 실행돼야 하며, 기존 `WI-WORLD-06` 역할 카드 장착과 혼동하지 않는다.

## City 독립 서비스 계약

```text
WI-CITY-01 도심 수요 확정
 → WI-CITY-02 지역 재고 배정
 → WI-CITY-03 서비스 처리 또는 부족 복구
 → WI-CITY-04 결과 확인·다음 운영 선택
```

City 4개 WI는 현재 E1 계약이다. 기존 도심마트 파생 자료를 권위 상태로 오인하지 않고 City 독립 명령 Aggregate·Save/Replay·Fixture를 E2/E3부터 구현한다.

이 12개 WI는 자동 시험 E3이지만 공간 통합은 E1이다. Town의 실제 H 결속과 독립 시장 폐루프 없이 Farm 성과를 재사용해 완료로 판정하지 않는다.

## 행위자 공통 물품·장착

```text
WI-ACTOR-01 물품 획득
  → Inventory
  → WI-ACTOR-02 장착 상태 변경(Equip/Unequip/Swap)
  → 장착 ItemDefinition의 Capability 파생
  → 도끼 벌목·후속 삽 평탄화·곡괭이 채굴 같은 구체 WI
```

`WI-NATURE-05`는 `WI-ACTOR-01`의 도끼·공간 특화다. 획득 뒤 도끼는 인벤토리에만 있고 `WI-ACTOR-02`로 `MainHand`에 장착해야 `capability:woodcutting`이 생긴다. 역할 카드 `WI-WORLD-06`과 전투 편성은 같은 장착 어휘를 쓰지만 물리 물품 원장과 분리한다.

## Nature 생존 생활거점과 심리 회복

| WI | 행위 | 상태·Effect | 다음 WI | 현재 빈칸 |
| --- | --- | --- | --- | --- |
| WI-NATURE-01 | 자연 지역 위험 징후 확인 | NatureThreatObserved | NATURE-02 또는 03 | WI 대장의 H 참조 |
| WI-NATURE-02 | 안전 거점으로 긴급 후퇴 | PartyRetreatedToSafeCore | NATURE-04 | H 참조·E7 실행 |
| WI-NATURE-03 | 훼손된 자연 경로 복원 | NatureRouteRestored | NATURE-04 | H 참조·E7 실행 |
| WI-NATURE-04 | 탐사대 안전 회복 | PartyRecovered | 다음 탐험·Farm 선택 | 반환 계약·E7 실행 |
| WI-NATURE-05 | 벌목 도끼 확보 | AxeAcquired, ItemAcquired | ACTOR-02 | 명시적 장착 뒤 E7 재검증 |
| WI-NATURE-06 | 나무 벌목 작업 시작 | TreeFelled, TimberCreated | 반복 또는 NATURE-07 | MainHand 벌목 능력·작업영역 예약·취소 |
| WI-NATURE-07 | 오두막을 지을 터 선정 | CabinBlueprintPlaced | NATURE-08 | 부지 취소·재배치 |
| WI-NATURE-08 | 오두막 건설 작업 시작 | CabinOperational | NATURE-09 또는 NATURE-12 | Hosted·수동 Game View |
| WI-NATURE-09 | 오두막 안으로 들어가기 | PlayerEnteredCabin | NATURE-10 | 점유 충돌 검사 |
| WI-NATURE-10 | 오두막 밖으로 나가기 | PlayerLeftCabin | NATURE-09 | 점유 용량 반환 |
| WI-NATURE-11 | 황혼 위협 대응 방식 확정 | BattleHandoffRequested 또는 PlayerRetreated | 전투·회복 | 복합 책임 분할·실제 전투 인계·E7 실행 |
| WI-NATURE-12 | 진행 중 작업 취소 | WorkCancelled, SpatialReservationReleased, ReservedMaterialReturned | Nature 안전 선택 | Hosted·수동 Game View |
| WI-NATURE-13 | 획득 자원 거점 보관 | TimberStored | 수면·제작·다음 선택 | 실제 보관 UI |
| WI-NATURE-14 | 오두막에서 수면·새벽 맞기 | DawnReached | NATURE-15 | 실제 수면 피드백 |
| WI-NATURE-15 | 다음 날 거점 확장 계획 선택 | Day2Ready | CON-01 또는 제작 선택 | 실제 계획 UI |
| WI-NATURE-16 | 현장 보급 꾸러미 제작 | NatureFieldSupplyPackAdded, ExpeditionPrepared | NATURE-06 | 제작·사용 효과 분할·작업대 실제 입력·Hosted 동등성 |
| WI-NATURE-17 | 현장 보급 제작 업무 위임 | NpcFieldSupplyPolicySelected | NATURE-06 | 위임·NPC 실제 제작 분할·Hosted 동등성 |

Nature의 1차 플레이는 생존·채집·건설·방어이고 심리 회복은 그 결과에 결합한다. Nature는 여전히 Farm 업무 원인의 대체 공간이 아니며 Farm의 생산 실패·복구 원인은 Farm 업무 상태로 남긴다.

## 공통 World 역량 WI

| WI | 역할 |
| --- | --- |
| WI-WORLD-01 | NPC 업무 배정 |
| WI-WORLD-02 | 역량 부여 |
| WI-WORLD-03 | 예약 해제 포함 작업 취소 |
| WI-WORLD-04 | 시설 수리 |
| WI-WORLD-05 | 발견 |
| WI-WORLD-06 | 역할 카드 장착 |
| WI-WORLD-07 | 활동 시작·완료 |
| WI-WORLD-08 | 턴 종료 |

이 WI들은 현재 E3 구현·E1 공간 통합이다. NPC 관련 계약이 있다는 사실만으로 NPC PlayableUnit E7, 그 단위의 E8 반복 안정성, 여러 Core의 E9 생활 조화를 주장하지 않는다.

## Preview→Confirm 공통 계약

각 WI 구현은 아래 최소 추적성을 가져야 한다.

```text
입력과 ExpectedRevision
 → Preview: 후보·비용·영향만 계산
 → Confirm: 안정 ID와 ExpectedRevision으로 서버 결정 요청
 → Decision
 → Task
 → Effect
 → WorldTick
 → 같은 Session 재조회
 → Save / Replay Hash
```

클라이언트는 운임·중량·결과 modifier를 확정하거나 Effect를 직접 적용하지 않는다.

## 현재 가장 중요한 미결 연결

1. `WI-NATURE-05 → WI-ACTOR-02 → WI-NATURE-06`을 canonical Scene 실제 입력으로 완주하고 장착·해제·저장 재진입의 E7을 다시 닫는다.
2. Farm 포장 뒤 Farm 내부 반환 경로를 계약화해 독립 폐루프를 닫는다.
3. Nature↔Farm 이동·활동·회복·다음 날을 같은 Session에서 Save/Replay한다.
4. 저장 `SimulationWorldShell`에서 실제 서버 Preview·Confirm·Tick·재조회로 E7을 증명한다.
5. Hub·Town은 각각 독립 내부 폐루프를 닫은 후에만 영역 간 사슬을 통합 증거로 승격한다.

## 64개 WI 식별자 재고

| 그룹 | 안정 식별자 |
| --- | --- |
| ACTOR | WI-ACTOR-01, WI-ACTOR-02 |
| FARM | WI-FARM-01, WI-FARM-02, WI-FARM-03, WI-FARM-04, WI-FARM-05, WI-FARM-06 |
| LOG | WI-LOG-01, WI-LOG-02, WI-LOG-03, WI-LOG-04, WI-LOG-05 |
| HUB | WI-001, WI-002, WI-HUB-03, WI-HUB-04, WI-HUB-05, WI-HUB-06 |
| MARKET | WI-MARKET-01, WI-MARKET-02, WI-MARKET-03, WI-MARKET-04, WI-MARKET-05 |
| ORDER | WI-ORDER-01, WI-ORDER-02, WI-ORDER-03, WI-ORDER-04, WI-ORDER-05, WI-ORDER-06, WI-ORDER-07 |
| NATURE | WI-NATURE-01, WI-NATURE-02, WI-NATURE-03, WI-NATURE-04, WI-NATURE-05, WI-NATURE-06, WI-NATURE-07, WI-NATURE-08, WI-NATURE-09, WI-NATURE-10, WI-NATURE-11, WI-NATURE-12, WI-NATURE-13, WI-NATURE-14, WI-NATURE-15, WI-NATURE-16, WI-NATURE-17, WI-NATURE-18 |
| REFLECT | WI-REFLECT-01 |
| CARD | WI-CARD-01 |
| CON | WI-CON-01 |
| CITY | WI-CITY-01, WI-CITY-02, WI-CITY-03, WI-CITY-04 |
| WORLD | WI-WORLD-01, WI-WORLD-02, WI-WORLD-03, WI-WORLD-04, WI-WORLD-05, WI-WORLD-06, WI-WORLD-07, WI-WORLD-08 |
