# 현재 플레이 폐루프 완결 원장

> 이 문서는 `eng/execution-ledgers/playable-loops.json`와 `eng/execution-ledgers/evidence-packages.json`에서 자동 생성된다. 직접 수정하지 않는다.
> CoreClosed는 E5 핵심 폐루프, ExtendedClosed는 선택형 확장 E5, PlayClosed는 E7 핵심 플레이 폐루프, WorldClosed는 E8 개별 안정성을 거친 둘 이상의 Core가 E9 영역 조화·사람 승인을 통과한 집계를 뜻한다.
> Nature 시범 루프는 논리·시각 성숙도를 별도로 기록하며 통합 E는 두 축 중 낮은 단계다. WI 음양 분류는 개발 성숙도 축과 무관하다.

- 플레이 폐루프 대장: `ssalddel-playable-loop-catalog.r18`
- 증거 묶음 대장: `ssalddel-evidence-package-catalog.r24`
- 플레이 단위·집계: `23`
- 등록 증거 묶음: `32`

## 판정 기준

| 기준 | 판정 |
| --- | --- |
| 핵심 완결 | 모든 필수 Core 자식이 E5 CoreClosed 이상이어야 영역 집계가 닫힘 |
| 확장 완결 | 건물·타로·배치 같은 선택형 자식은 ExtendedClosed로 별도 기록하며 Core를 막지 않음 |
| 플레이 완결 | 실제 입력·화면·귀환까지 E7이 확인된 자식만 PlayClosed |
| 이중 순환 | Nature 시범 루프는 논리 E와 시각 E가 모두 E7이고 열린 환류가 없어야 PlayClosed |
| 개별 안정 | E7 PlayableUnit이 반복 결정성·Save/Replay·Local/Remote·실제 입력 재진입을 통과하면 별도 E8 캠페인으로 기록 |
| 세계 완결 | E8을 통과한 둘 이상의 Core가 영역 조화와 사람 승인을 E9에서 통과한 집계만 WorldClosed |
| 독립 영역 우선 | 플레이어 연속성 기준 Nature→Farm→Hub→Town→City의 내부 폐루프를 먼저 닫고 영역 간 연결은 뒤로 미룸 |

## 전체 상태

| 우선 | 폐루프 | 수준·등급 | WI | 논리 E | 시각 E | 통합→다음/최종 E | 완결 | 상태 | 증거 | 다음 행동 |
| ---: | --- | --- | ---: | --- | --- | --- | --- | --- | ---: | --- |
| 1 | `playable-loop:nature-survival-homestead.v1` Nature 생존 생활거점 영역 집계 | AreaAggregate/Aggregate | 0 | - | - | E1→E2/E9 | Open | InProgress | 2 | 생활 거점 기초와 밤→Day2 자식 폐루프를 각각 E5로 닫은 뒤 영역 CoreClosed를 파생 판정한다. |
| 2 | `playable-loop:nature-shelter-foundation.v1` Nature 도끼·벌목·오두막 기초 | PlayableUnit/Core | 10 | E7 | E7 | E7→E7/E7 | PlayClosed | Validated | 8 | stability:nature-shelter-foundation.v1 E8 통과를 유지하고 stability:nature-twilight-return.v1의 고정 후보를 검증한다. |
| 3 | `playable-loop:nature-twilight-return.v1` Nature 황혼 위협 대응·귀환 | PlayableUnit/Core | 2 | E7 | E7 | E7→E7/E7 | PlayClosed | Validated | 5 | 완료 상태를 유지하되 H3 ThreatInput·전투 상태·Skeleton 표현 계약 변경 시 논리와 표현 증거를 각각 재검증한다. |
| 4 | `playable-loop:nature-night-day2.v1` Nature 보관·수면·Day2 반환 | PlayableUnit/Core | 3 | E7 | E6 | E6→E7/E7 | Open | InProgress | 9 | SkyEngineTests와 canonical Scene 다섯 날씨 상태·오두막 내부 강수 차폐를 검증한다. |
| 5 | `playable-loop:nature-workbench-foundation.v1` Nature 작업대 기반 | PlayableUnit/Core | 2 | E7 | E6 | E6→E7/E7 | Open | InProgress | 4 | Table Saw·목재·상자·조명을 배치 통제 계층에서 하나의 작업 구역으로 조립하고 건설·취소·운영 화면 차이를 재검증한다. |
| 6 | `playable-loop:nature-field-supply-return.v1` Nature 현장 성과·거점 제작·다음 원정 왕복 | PlayableUnit/Core | 6 | E4 | E1 | E1→E2/E7 | Open | InProgress | 1 | WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다. |
| 7 | `playable-loop:nature-base-reflection.v1` Nature 거점 성찰·다음 원정 준비 | PlayableUnit/Extension | 1 | E3 | E1 | E1→E2/E7 | Open | Defined | 1 | 공용 E1~E3 계약·시험을 기준선으로 삼아 WI-REFLECT-01과 기존 Nature 오두막 H1을 결속한다. |
| 8 | `playable-loop:nature-building-learning.v1` Nature 건물 발전·배움 확장 | PlayableUnit/Extension | 2 | E5 | E1 | E1→E2/E7 | Open | InProgress | 1 | 기존 StableId를 유지한 채 배움터 Extension의 NPC 생활 주기를 E7→E1로 검토하고 가장 낮은 미완료 의존성부터 구현한다. |
| 9 | `playable-loop:nature-regional-threat-recovery.v1` Nature 지역 위협 후퇴·복원·회복 | PlayableUnit/Extension | 4 | E3 | E1 | E1→E2/E7 | Open | Defined | 1 | Nature 핵심 첫날 폐루프와 분리된 Extension Goal로 선택될 때 WI-NATURE-02부터 E4→E7을 진행한다. |
| 20 | `playable-loop:farm-internal-production.v1` Farm 독립 생산·보관 영역 집계 | AreaAggregate/Aggregate | 0 | - | - | E1→E2/E9 | Open | InProgress | 1 | 생산 주기와 포장·내부 보관 반환을 별도 Fixture로 닫은 뒤 영역 집계를 올린다. |
| 21 | `playable-loop:farm-crop-cycle.v1` Farm 경작·성장·수확 | PlayableUnit/Core | 4 | E3 | E1 | E1→E2/E7 | Open | InProgress | 1 | 감자 독립 Fixture로 경작→수확 Lot과 재시도 상태를 닫는다. |
| 22 | `playable-loop:farm-pack-store-return.v1` Farm 집하·포장·내부 보관 반환 | PlayableUnit/Core | 2 | E3 | E1 | E1→E2/E7 | Open | InProgress | 1 | 새 WI를 성급히 만들지 않고 WI-FARM-06 완료 결과에 내부 보관·재생산 반환 계약을 먼저 확정한다. |
| 23 | `playable-loop:farm-player-placement.v1` Farm 플레이어 배치 확장 | PlayableUnit/Extension | 2 | E3 | E1 | E1→E2/E7 | Open | InProgress | 1 | Farm 핵심 생산 폐루프와 분리해 배치 Preview·Confirm·Tick·Save 계보를 전용 증거로 만든다. |
| 30 | `playable-loop:hub-internal-warehouse.v1` Hub 독립 창고 영역 집계 | AreaAggregate/Aggregate | 0 | - | - | E1→E2/E9 | Open | Defined | 0 | 입고·적치 자식의 독립 Fixture부터 E3로 만든다. |
| 31 | `playable-loop:hub-inbound-putaway.v1` Hub 입고·검수·적치 | PlayableUnit/Core | 2 | E2 | E1 | E1→E2/E7 | Open | Defined | 0 | Hub 자체 입고 Lot과 용량·검수 실패·재선택 계약을 E3로 구현한다. |
| 32 | `playable-loop:hub-outbound-ready-return.v1` Hub 출고 준비·작업 반환 | PlayableUnit/Core | 3 | E3 | E1 | E1→E2/E7 | Open | InProgress | 1 | Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다. |
| 40 | `playable-loop:town-resident-market.v1` Town 주민 생활복구 영역 집계 | AreaAggregate/Aggregate | 0 | - | - | E1→E2/E9 | Open | InProgress | 1 | 타로 유무와 무관한 주문→소비→다음 욕구 E5 Fixture부터 닫는다. |
| 41 | `playable-loop:town-order-consume-return.v1` Town 주문·소비·다음 욕구 | PlayableUnit/Core | 7 | E4 | E1 | E1→E2/E7 | Open | InProgress | 1 | 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다. |
| 42 | `playable-loop:town-arcana-context.v1` Town 메이저 아르카나 문맥 확장 | PlayableUnit/Extension | 1 | E4 | E1 | E1→E2/E7 | Open | InProgress | 1 | 핵심 주문 폐루프와 분리된 확장 Fixture에서 방향 판정·단일 전개·저장 복원을 발현한다. |
| 50 | `playable-loop:city-urban-logistics.v1` City 독립 도심 서비스 영역 집계 | AreaAggregate/Aggregate | 0 | - | - | E1→E5/E9 | Open | Defined | 0 | 도심 수요→재고 배정→서비스→결과 확인 자식을 E2/E3로 구현한다. |
| 51 | `playable-loop:city-demand-service-return.v1` City 수요·서비스·결과 반환 | PlayableUnit/Core | 4 | E1 | E1 | E1→E2/E7 | Open | Defined | 0 | 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다. |
| 90 | `playable-loop:solo-world-day.v1` Solo 하루 세계 집계 | WorldAggregate/Aggregate | 1 | - | - | E1→E5/E9 | Open | Defined | 0 | 영역 간 운송을 선행시키지 말고 각 영역 CoreClosed 뒤 하루 선택 집계를 조립한다. |
| 100 | `playable-loop:nature-farm-roundtrip.v1` Nature↔Farm 선택적 왕복 | WorldAggregate/Aggregate | 5 | - | - | E1→E7/E9 | Open | Deferred | 0 | 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다. |

## 영역 집계와 자식 상태

- **Nature 생존 생활거점 영역 집계** `Open`
  - 필수 Core: `playable-loop:nature-shelter-foundation.v1` — E7, PlayClosed
  - 필수 Core: `playable-loop:nature-twilight-return.v1` — E7, PlayClosed
  - 필수 Core: `playable-loop:nature-night-day2.v1` — E6, Open
  - 필수 Core: `playable-loop:nature-workbench-foundation.v1` — E6, Open
  - 필수 Core: `playable-loop:nature-field-supply-return.v1` — E1, Open
  - 선택 Extension: `playable-loop:nature-base-reflection.v1` — E1, Open
  - 선택 Extension: `playable-loop:nature-building-learning.v1` — E1, Open
  - 선택 Extension: `playable-loop:nature-regional-threat-recovery.v1` — E1, Open
- **Farm 독립 생산·보관 영역 집계** `Open`
  - 필수 Core: `playable-loop:farm-crop-cycle.v1` — E1, Open
  - 필수 Core: `playable-loop:farm-pack-store-return.v1` — E1, Open
  - 선택 Extension: `playable-loop:farm-player-placement.v1` — E1, Open
- **Hub 독립 창고 영역 집계** `Open`
  - 필수 Core: `playable-loop:hub-inbound-putaway.v1` — E1, Open
  - 필수 Core: `playable-loop:hub-outbound-ready-return.v1` — E1, Open
- **Town 주민 생활복구 영역 집계** `Open`
  - 필수 Core: `playable-loop:town-order-consume-return.v1` — E1, Open
  - 선택 Extension: `playable-loop:town-arcana-context.v1` — E1, Open
- **City 독립 도심 서비스 영역 집계** `Open`
  - 필수 Core: `playable-loop:city-demand-service-return.v1` — E1, Open

## 현재 닫힌 E5 단위


## 열린 경계

- `playable-loop:nature-night-day2.v1`
  - Unity Editor의 기존 Job lock 해소 뒤 Sky Engine 실제 Test Runner·Play Mode·Game View·Console을 재검증해야 한다.
- `playable-loop:nature-workbench-foundation.v1`
  - 작업대 상태별 공간 조립과 운영 가능성의 시각 E7 증거가 부족하다.
- `playable-loop:nature-field-supply-return.v1`
  - 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.
- `playable-loop:nature-base-reflection.v1`
  - WI-REFLECT-01과 H1 ReflectionInteractionAnchor의 E4 결속, 주 세션 저장·LocalProcess·RemoteHost Adapter, SimulationWorldShell 실제 입력 증거가 없다.
- `playable-loop:nature-building-learning.v1`
  - 배움터 실제 Unity 배치·Hosted 동등성과 NPC 판단→이동→학습 결과→다음 판단의 E7 폐루프 증거가 없다.
- `playable-loop:nature-regional-threat-recovery.v1`
  - WI-NATURE-02~04의 Scenario 공간을 실제 Nature 경관 Graph와 canonical SimulationWorldShell 귀환·회복 입력으로 닫지 않았다.
- `playable-loop:farm-crop-cycle.v1`
  - 독립 Fixture의 E4 주체·자원·시간 결속과 E5 성공·복구 반환 판정이 없다.
- `playable-loop:farm-pack-store-return.v1`
  - WI-FARM-07~09 예약 번호를 재사용하지 않는 내부 보관·반환 상태 계약과 E5 Fixture가 없다.
- `playable-loop:farm-player-placement.v1`
  - 현재 건물 WI의 실제 건설 상태 전이는 Nature 전용이며 Farm 배치는 별도 규칙·Scene 증거가 섞여 있다.
- `playable-loop:hub-inbound-putaway.v1`
  - 다른 영역 화물과 무관한 Fixture·Save/Replay·결정적 적치 시험이 없다.
- `playable-loop:hub-outbound-ready-return.v1`
  - PickingWorkArea·OutboundStagingArea의 승인 H 공간 결속과 실제 WI Manifestation이 없다.
  - SimulationWorldShell의 플레이어 정책 개입·NPC 업무 표현과 Game View 증거가 없다.
  - 다른 안정 Core와의 E9 NPC 생활 조화는 현재 범위 밖이다.
- `playable-loop:town-order-consume-return.v1`
  - 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.
- `playable-loop:town-arcana-context.v1`
  - 활성화 Snapshot은 시험됐지만 Town 핵심 폐루프에 적용된 최종 E5 계보와 Unity 설명 화면이 없다.
- `playable-loop:city-demand-service-return.v1`
  - 계약만 있으며 권위 Core·Save/Replay·Fixture가 없다.

## 보류 또는 독립 준비 후 통합

- `playable-loop:nature-farm-roundtrip.v1`: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

## 현재 증거 묶음

| 증거 | 축 | 종류 | 결과·상태 | 대상 E | 범위 | 제외 |
| --- | --- | --- | --- | --- | --- | --- |
| `evidence:hub-npc-routine-core-20260825` Hub 운영 유래 WI NPC 루틴 Core | Logic | AutomatedTest | Passed/Current | E3 | 독립 Hub 재고 Fixture, 전용 출고 NPC·정책, WI-HUB-03~05 결정적 WorldTick, 플레이어 직접 Confirm 차단, 정책·담당자 차단, 취소 반환, Local·Hosted 읽기 모델, simulation-save.v21 및 관련 FarmSupply·AreaBuilding·WI 성숙도 회귀 35/35 | Hub PickingWorkArea·OutboundStagingArea H 공간 결속 / WI-HUB-06 차량 상차와 외부 운송 / Unity Play Mode·Game View / 지속 NPC 생활세계 E8 / 운영 Provider와 운영 DB |
| `evidence:nature-base-reflection-e3-20260826` Nature 거점 성찰 승인자료·결정성 집중 자동 시험 | Logic | AutomatedTest | Passed/Current | E1, E2, E3 | 원문 관측→해석 후보→사람 승인 Publication, Publication hash와 파생 원장 멱등 동기화, 세션 불변 사본, 하루 한 번·Publication revision 한 번 제한, 원문 열기 무보상, 다음 활동 내면 효과 적용, 상태 hash 복원·변조 거부와 동일 입력 계산 동등성 7/7을 검증했다. | 실제 YouTube·Apify Provider 호출과 운영 DB / 주 세션 simulation-save.v24 및 HTTP LocalProcess·RemoteHost Adapter / Unity H1 배치, Play Mode, Game View |
| `evidence:nature-building-core-20260825` Nature 건물 발전·배움 확장 Core | Logic | AutomatedTest | Passed/Current | E3, E4, E5 | Nature r3 작업대·배움터 건설, 비용·선행·발자국·취소 반환·NPC 학습 방문과 simulation-save.v19의 결정적 Fixture 집중 시험 5/5 | Farm·Town·Hub·City 실제 건설 상태 전이 / Unity Play Mode 배치 입력과 Game View / Hosted RemoteHost hash 동등성 / 외부 Provider와 운영 DB |
| `evidence:nature-dual-combat-core-20260825` Nature 관찰 운영·직접 개입 Core와 LocalProcess 폐루프 | Logic | AutomatedTest | Passed/Current | E3, E4, E5 | 관찰 방식 잠금·카드 hash·자동 행동·단일 비상 개입, 직접 S/A/B 성과, Nature 서버 전용 추가 보상, HTTP 없는 LocalProcess 전투, simulation-save.v17 읽기 호환과 v18 슬롯 복원, 세션 API route 회귀 57/57 | 실제 Unity Play Mode 입력과 Game View / LocalProcess·RemoteHost WorldRevision별 hash 동등성 / E6 난도·보상 체감 정제 / 운영 Provider와 운영 DB |
| `evidence:nature-dual-combat-unity-editmode-20260825` Nature 이중 전투 Unity 컴파일과 공식 Scene 배선 | Presentation | UnityEditMode | Passed/Current | E3, E4 | 공통 RuntimeCore와 Local battle Adapter 스크립트 컴파일, 현장전투 입력 번역과 저장 SimulationWorldShell 배선 4/4 | 관찰 일시정지·비상 카드의 실제 Input System 조작 / 1인칭 직접 전투 완주 / Play Mode·Game View·Console / Hosted RemoteHost |
| `evidence:nature-dual-loop-game-view-20260826` Nature 논리·시각 이중 순환 Synty 표현과 실제 입력 | Presentation | UnityPlayMode | Passed/Stale | E6, E7 | Nature H 영역 기준으로 Synty Garden Shed·통나무·Skeleton·Table Saw를 배치하고 진단 HUD를 기본 접힘으로 바꿨다. EditMode 14/14, PlayMode 전체 실행의 비저장 경로 5/5와 저장 revision 경계 수정 후 집중 복원 1/1을 통과했다. Game View에서 세 통나무 묶음과 실제 왼쪽 버튼 획득, 오두막, 황혼 Skeleton, 작업대를 확인했다. 이 증거는 오두막·벌목과 황혼 루프의 표현 E7, 보관·수면·Day2와 작업대 루프의 표현 E6만 뒷받침한다. | 사람 수동 조작에 의한 최종 미감·청음 수용 / 보관·수면·새벽 상태별 공간 변화와 카메라 가림 해소 / 목재·상자·조명을 포함한 완성 작업 구역 / RemoteHost Unity 실행과 운영 Provider·운영 DB |
| `evidence:nature-field-supply-core-20260825` Nature 현장 성과·거점 제작·다음 원정 왕복 Core | Logic | AutomatedTest | Passed/Current | E3, E4 | Nature r4 직접 제작 WI-NATURE-16을 보존하고 npc-routine-control.r3 정책 비활성·활성, 적격 NPC 결정, 입력 없는 4초 WI-NATURE-17 위임 제작, 직접 제작 경합, 취소 반환, 보급 꾸러미 원정 준비·패배 보호와 simulation-save.v20·v21 결정적 재생 집중 시험 및 Nature 생활핵 CraftingWorkArea 문맥 결속 | Unity Play Mode 작업대 입력과 Game View / Hosted RemoteHost revision별 hash 동등성 / 별도 다중 Lot 재고 원장 / 외부 Provider와 운영 DB |
| `evidence:nature-first-evening-equipment-logic-20260827` Nature 첫 저녁 명시적 장착·생활거점 LocalProcess·RemoteHost 논리 증거 | Logic | HostedParity | Passed/Current | E3, E5, E7 | WI-ACTOR-01 획득과 WI-ACTOR-02 장착·해제·Swap, 장착 상태에서만 생기는 벌목 능력, simulation-save.v27 복원·Replay와 Nature 생활거점 LocalProcess·RemoteHost revision/hash 동등성을 묶은 집중 시험 10/10을 검증했다. | Unity 실제 I 입력·상태창 버튼·손 도끼와 Game View / 열린 Unity Editor의 Job Lock 오류와 전경 지형 가림 / 운영 Provider와 운영 DB |
| `evidence:nature-night-day2-presentation-e7-20260826` Nature 보관·수면·새벽 계획 표현 E7 실제 입력과 Game View | Presentation | UnityPlayMode | Passed/Stale | E6, E7 | Logic E7 기준선을 바꾸지 않고 WI-NATURE-13의 실제 G 입력 후 오두막 옆 Synty 목재 더미와 보관 수량, WI-NATURE-14의 실제 T 입력 후 야간 차광·침상·캠프파이어 자리와 권위 시간에 따른 새벽 계획판 전환, WI-NATURE-15의 실제 숫자 1 입력 후 작업대 계획 확정·건설 위치 선택 상태를 같은 canonical Scene에서 검증했다. 집중 PlayMode 3/3과 시각 자산 대장 EditMode 1/1이 통과했다. 전체 Nature E7 PlayMode 6건 중 현재 세 WI 3건과 작업대 1건은 통과했고, 기존 이동 제한과 황혼 상태 문구 2건은 별도 회귀로 남겼다. | 사람 수동 조작에 의한 최종 미감·실제 청음 수용 / 작업대 건설 중·운영 중 작업 구역의 표현 E7 / 전체 Nature PlayMode 묶음의 기존 이동 제한·황혼 상태 문구 실패 2건 / RemoteHost Unity 실행과 운영 Provider·운영 DB |
| `evidence:nature-night-day2-wi13-hosted-parity-20260826` Nature 획득 자원 거점 보관 LocalProcess·RemoteHost 동등성 | Logic | HostedParity | Passed/Current | E3, E5, E7 | nature-survival.realtime.r2에서 도끼 획득, 나무 4개 벌목, 오두막 배치·건설·입장과 통나무 2개 보관을 동일 명령열로 실행했다. LocalProcess와 RemoteHost의 최종 revision, simulation-save.v23 Replay hash, 오두막 Container와 입고 Transfer가 일치했고 Local slot 복원이 같은 보관 상태를 되살렸다. 전용 동등성 1/1이 통과했다. | 별도 UnityPlayMode EvidencePackage가 소유하는 실제 G 입력과 Game View / WI-NATURE-14~15 밤→Day2 전체 폐루프 / 운영 Provider와 운영 DB |
| `evidence:nature-night-day2-wi13-playmode-20260826` Nature 획득 자원 거점 보관 실제 입력 | Presentation | UnityPlayMode | Passed/Superseded | E6, E7 | 완성 오두막에 들어간 상태에서 실제 G 입력으로 소지 통나무 2개를 오두막 보관함으로 옮겼다. Simulation 상태 사본은 소지 2→0, 보관 0→2를 확정했고 Unity는 결과만 표시했다. 전용 PlayMode 1/1(3.40초), 결과 Game View PNG와 마지막 시험 이후 Console 오류 0건을 확인했다. | WI-NATURE-14 수면과 WI-NATURE-15 새벽 계획 선택 / 선택형 보관 애니메이션·효과음 실제 수용 확인 / 운영 Provider와 운영 DB |
| `evidence:nature-night-day2-wi14-hosted-parity-20260826` Nature 오두막 수면·새벽 맞기 LocalProcess·RemoteHost 동등성 | Logic | HostedParity | Passed/Current | E3, E5, E7 | r2 도끼·벌목·오두막·보관·황혼 후퇴 전제에서 SleepInCabin과 권위 실시간 60초를 동일하게 실행했다. LocalProcess와 RemoteHost는 정확히 1110초 Dawn, Sleeping=false, 보관 통나무 2개, 최종 revision과 simulation-save.v23 Replay hash가 일치했고 Local slot 복원이 같은 새벽 상태를 되살렸다. 전용 동등성 1/1이 통과했다. | 별도 UnityPlayMode EvidencePackage가 소유하는 실제 T 입력과 Game View / WI-NATURE-15 계획 선택과 Day2Ready / 운영 Provider와 운영 DB |
| `evidence:nature-night-day2-wi14-playmode-20260826` Nature 오두막 수면·새벽 맞기 실제 입력 | Presentation | UnityPlayMode | Passed/Superseded | E6, E7 | 오두막 보관과 황혼 후퇴를 준비한 뒤 밤에 실제 T 입력으로 수면을 시작했다. Simulation 상태 사본은 Sleeping=true를 확정했고 권위 실시간 60초를 6배 밤 진행으로 적용해 정확히 1110초 DawnReached·Sleeping=false로 자동 기상했다. 전용 PlayMode 1/1(3.43초), 새벽 Game View와 Console 오류 0건을 확인했다. | WI-NATURE-15 새벽 확장 계획 실제 선택 / 선택형 수면 애니메이션·환경음 실제 수용 확인 / 운영 Provider와 운영 DB |
| `evidence:nature-night-day2-wi15-hosted-parity-20260826` Nature Day2 확장 계획 LocalProcess·RemoteHost 동등성 | Logic | HostedParity | Passed/Current | E3, E5, E7 | r2 도끼·벌목·오두막·보관·황혼 후퇴·수면·새벽 전제에서 Workbench 계획 선택을 동일하게 실행했다. LocalProcess와 RemoteHost는 ExpansionPlanSelected=Workbench, Day2Ready=true, 최종 revision과 simulation-save.v23 Replay hash가 일치했고 Local slot 복원이 같은 Day2 상태를 되살렸다. WI-NATURE-13~15를 포함한 생활 거점 동등성 회귀 4/4가 통과했다. | 별도 UnityPlayMode EvidencePackage가 소유하는 실제 숫자 1 입력과 Game View / WI-CON-01 작업대 건설과 이후 Day2 실행 / 운영 Provider와 운영 DB |
| `evidence:nature-night-day2-wi15-playmode-20260826` Nature Day2 확장 계획 실제 입력 | Presentation | UnityPlayMode | Passed/Stale | E6, E7 | 오두막 보관·황혼 후퇴·수면·새벽을 권위 상태로 준비한 뒤 실제 숫자 1 입력으로 작업대 확장 계획을 선택했다. Simulation 상태 사본은 SelectedExpansionPlanCode=Workbench와 Day2Ready=true를 확정했고 Unity는 Day 2 계획 고정 결과만 표시했다. 전용 PlayMode 1/1(3.53초), Day2Ready Game View와 마지막 시험 이후 Console 오류 0건을 확인했다. | Day2 작업대 건설 WI-CON-01 실행 / 선택형 계획 카드 애니메이션·효과음 실제 수용 확인 / 운영 Provider와 운영 DB |
| `evidence:nature-playmode-20260824` Nature Solo LocalProcess PlayMode | Presentation | UnityPlayMode | Passed/Stale | E7 | 실제 Input System의 도끼 획득·C 취소·B 배치·E 퇴장·R 후퇴와 저장·Scene 재진입 복원 1/1 | 수동 Game View 시각 수용 / 오류 없는 Console 캡처 / Hosted RemoteHost / 전체 후속 경로의 E5 Manifested 판정 |
| `evidence:nature-r2-core-20260825` Nature 첫날 r2 Core와 Unity 컴파일 | Logic | AutomatedTest | Passed/Stale | E3 | WI-NATURE-13~15, r2 보관·위협·WorldLocal 전투 후퇴 인계·전투 결과·수면·Day2 계획·simulation-save.v17 집중 시험 31/31, Simulation 빌드, Unity 스크립트 컴파일 | 실제 직접 전투의 LocalProcess 권위 연결 / Hosted RemoteHost revision hash 동등성 / Play Mode 전체 첫날 완주 / 수동 Game View와 Console 수용 |
| `evidence:nature-r5-logic-20260826` Nature r5 지면 통나무·저장·동등성 논리 증거 | Logic | AutomatedTest | Passed/Current | E3, E5, E7 | WI-NATURE-18 지면 통나무 생성·용량 차단·멱등 획득, r1~r4 즉시 지급 호환, simulation-save.v24 복원·Replay hash, LocalProcess·RemoteHost 상태 동등성을 포함한 Nature 집중 시험 45/45를 검증했다. | 운영 Provider와 운영 DB / 별도 Presentation 증거가 소유하는 Unity 실제 입력과 Game View |
| `evidence:nature-regional-threat-core-20260826` Nature 지역 위협 후퇴·복원·회복 Core 계약 | Logic | AutomatedTest | Passed/Current | E1, E2, E3 | 지역 위협 관찰 뒤 EmergencyRetreat의 경로·Actor 예약과 취소 반환, NatureRestoration의 원인 계보·자재, PartyRecovery의 안전 생활핵 조건, Save/Replay와 HTTP 계약을 자동 시험한다. 황혼 조우 ResolveEncounter Retreat와는 Stable ID·상태·공간 책임이 다름을 E1에서 분리했다. | 실제 Nature AreaSet 경관 Graph 배치 / canonical SimulationWorldShell 실제 이동·회복 입력과 Game View / 운영 Provider와 운영 DB |
| `evidence:nature-shelter-e8-logic-20260827` Nature 생활거점 고정 후보 E8 논리 반복 안정성 | Logic | HostedParity | Passed/Current | E8 | 동일한 actor-equipment.r1/simulation-save.v27 후보에서 SimulationActorEquipmentTests와 SimulationNature생활거점동등성Tests를 세 차례 실행해 매회 10/10 통과했다. Save/Restore/Replay canonical hash와 LocalProcess·RemoteHost WorldRevision 동등성을 같은 시험 묶음에서 확인했다. | Unity 실제 입력·Game View 반복 안정성 / 사람의 재미·몰입·완성도 승인 / 운영 Provider와 운영 DB |
| `evidence:nature-shelter-e8-presentation-20260827` Nature 생활거점 고정 후보 E8 실제 입력·재진입 안정성 | Presentation | UnityPlayMode | Passed/Current | E8 | 동일 Unity 코드와 저장 판본에서 실제 Input System I·포인터 입력, 장착·해제·미장착 벌목 차단·재장착, 오두막 보관, 저장과 Scene 재진입을 세 차례 통과했다. 마지막 실행 전 Console을 비우고 cursor 3930부터 3979까지 차단 오류 0건을 확인했다. | 사람 직접 I 장착 반복과 미감·청음 승인 / RemoteHost Unity 프로세스의 실제 네트워크 실행 / E9 영역 조화와 사람 승인 |
| `evidence:nature-shelter-explicit-equipment-e7-20260827` Nature 명시적 장착·생활거점 canonical PlayMode E7 | Presentation | UnityPlayMode | Passed/Superseded | E5, E6, E7 | canonical SimulationWorldShell에서 실제 Input System I 입력과 장착 상태창 버튼으로 MainHand 장착·해제·미장착 벌목 차단·재장착을 거쳐 벌목·지면 통나무 획득·오두막·보관·저장·Scene 재진입을 완주했다. Nature생존E7폐루프PlayModeTests 전체 6/6, Nature 감각 표현 1/1, H 계층 실증 EditMode 2/2가 통과했다. 사람 직접 Game View에서는 Nature 전환과 도끼 획득 및 HUD 권위 전이를 확인했고, 자동화 화상 키보드가 자기 창에 입력을 소비해 사람 직접 I 장착 조작은 별도 수용 증거로 남겼다. | 사람 직접 I 장착·해제 반복과 최종 미감·실제 청음 수용 / RemoteHost Unity 프로세스의 실제 네트워크 실행 / E8 동일 후보 revision 반복 안정성 캠페인 / 운영 Provider와 운영 DB |
| `evidence:nature-shelter-hosted-parity-20260825` Nature 생활거점 LocalProcess·RemoteHost 동등성 | Logic | HostedParity | Passed/Current | E5, E7 | 현재 Simulation revision에서 취소 전 2초 진행, 취소, 재수확, 나무 3개 벌목, 오두막 건설, 입장·퇴장과 저장·복원을 LocalProcess와 RemoteHost HTTP에 동일하게 적용했다. 두 실행 위치는 최종 revision 15, simulation-save.v23, 저장 revision과 Replay hash가 일치했고 Local slot 복원과 RemoteHost replay-verifications도 같은 완료 상태를 복원했다. 전용 전체 동등성 1/1과 Nature·Local Runtime 집중 회귀 44/44가 통과했다. | 별도 UnityPlayMode EvidencePackage가 소유하는 사람 수동 완주와 Game View / 선택형 실제 음향 청취 / 운영 Provider와 운영 DB |
| `evidence:nature-shelter-playmode-20260825` Nature 생활거점 실제 입력·수동 완주 E7 | Presentation | UnityPlayMode | Passed/Superseded | E5, E6, E7 | 실제 Editor Play Mode에서 도끼 획득, 첫 벌목 취소·자원 무변경, 재시도와 세 나무 벌목, 오두막 도면 배치·건설, 입장·퇴장, HUD 저장과 Play Mode 재진입 복원을 사람이 수동 완주했다. 복원 Game View는 Day 2, 도끼 보유, 통나무 0, 그루터기 3개와 오두막 완성을 기록했고 Console 오류는 0개였다. 같은 Unity revision에서 실제 Input System 전체 폐루프 PlayMode 1/1(27.49초), 감각 표현 PlayMode 1/1(2.92초), 하단 UI 입력 경계 EditMode 1/1이 통과했다. 최신 Simulation revision에서는 Nature·Local Runtime 집중 회귀 44/44와 취소·재수확·오두막·저장·복원 전체 LocalProcess·RemoteHost 동등성 1/1이 통과했다. 오두막 우선 접근은 이미 가까운 플레이어를 외벽 밖 안전 반경까지 이격한다. 음향 진단은 48 kHz Stereo, Listener 활성과 절차형 행동 효과음 4개 결속을 기록했다. | 현재 연결된 Windows 재생 종단점이 없어 수행하지 못한 선택형 실제 음향 청취 / 승인 Nature Ambient·BGM 선택 채널 / 운영 Provider와 운영 DB |
| `evidence:nature-twilight-e8-logic-20260827` Nature 황혼 대응·귀환 고정 후보 E8 논리 반복 안정성 | Logic | HostedParity | Passed/Current | E8 | ObserverOperation과 DirectAction 두 황혼 대응 명령열을 세 차례 실행해 매회 2/2 통과했다. 각 실행은 전투 결과·Battle Replay hash·최종 WorldRevision·simulation-save.v27 저장 hash의 LocalProcess/RemoteHost 동등성과 RemoteHost 패키지의 Local slot 복원을 검증했다. | Unity 실제 전투·후퇴 입력 / 사람의 논리·표현 조화와 재미·몰입·완성도 승인 / 운영 Provider와 운영 DB |
| `evidence:nature-twilight-e8-presentation-20260827` Nature 황혼 후퇴·직접 개입 E8 실제 입력·재진입 안정성 | Presentation | UnityPlayMode | Passed/Current | E8 | 실제 O/R 후퇴와 O/F/좌클릭 직접 개입 두 경로를 네 차례 각각 2/2 통과했다. 두 경로 모두 결과 저장 뒤 canonical Scene을 다시 열어 같은 SessionStableId·WorldRevision·EncounterResolved·LastCombatResultCode를 복원했고, 마지막 cursor 4300~4400 구간의 신규 Console 오류는 0건이었다. | ObserverOperation 전술 일시정지·비상 카드 사람 조작 / 사람의 최종 미감·청음과 E9 승인 / RemoteHost Unity 프로세스 실제 네트워크 실행 |
| `evidence:nature-twilight-wi11-hosted-parity-20260826` Nature 황혼 이중 참여 LocalProcess·RemoteHost 동등성 | Logic | HostedParity | Passed/Current | E3, E5, E7 | ObserverOperation과 DirectAction 각각에 동일한 황혼 관찰·Fight·전투 명령열을 적용해 결과 코드, 최종 World revision, Battle Replay hash, simulation-save.v23 Replay hash를 일치시켰다. RemoteHost 저장 묶음을 Local slot 실제 복원 경로로 읽어 Battle Store와 Nature EncounterResolved·결과 상태를 함께 복원했다. 전용 동등성 2/2와 Nature 회귀 40/40이 통과했다. | 별도 UnityPlayMode EvidencePackage가 소유하는 실제 입력과 Game View / 운영 Provider와 운영 DB |
| `evidence:nature-twilight-wi11-playmode-20260826` Nature 황혼 관찰 운영·직접 개입 실제 입력과 거점 귀환 | Presentation | UnityPlayMode | Passed/Superseded | E6, E7 | 관찰 경로는 실제 O/F 입력 뒤 ObserverOperation 전투를 33.82초에 완주했고, 직접 경로는 실제 O/F/좌클릭 입력 뒤 DirectAction 전투를 9.26초에 완주했다. 별도 실제 O/R 입력은 조우를 Retreat로 해결하고 안전 거점 선택으로 돌아왔다. 세 경로 모두 Simulation 권위 결과를 Nature EncounterResolved와 거점 다음 선택 피드백으로 인계했다. 결과 Game View PNG 세 장과 마지막 시험 이후 Console 오류 0개를 확인했다. | 관찰 운영의 선택형 전술 일시정지·비상 카드 수동 조작 / 선택형 실제 음향 청취 / 운영 Provider와 운영 DB |
| `evidence:nature-workbench-wi-con-01-hosted-parity-20260826` Nature 작업대 건설 LocalProcess·RemoteHost 동등성 | Logic | HostedParity | Passed/Current | E3, E5, E7 | 동일한 도끼·벌목 6회·오두막·전투 승리·수면·Day2 작업대 계획 기준선에서 WI-CON-01 건설 시작, WI-NATURE-12 취소 반환, 동일 위치 재시도와 권위 20초 완료를 실행했다. LocalProcess와 RemoteHost는 h1:nature:workbench Operational, 최종 revision, simulation-save.v23 schema와 Replay hash가 일치했다. 전용 동등성 1/1이 통과했다. | 별도 UnityPlayMode EvidencePackage가 소유하는 실제 입력과 Game View / WI-NATURE-16 현장 보급 제작의 실제 입력과 다른 안정 Core와의 E9 NPC 생활 조화 / 운영 Provider와 운영 DB |
| `evidence:nature-workbench-wi-con-01-playmode-20260826` Nature 작업대 배치·취소·재시도·운영 실제 입력 | Presentation | UnityPlayMode | Passed/Superseded | E6, E7 | Day2Ready와 작업대 계획 이후 실제 숫자 1 입력으로 배치 모드에 들어가 지면 클릭으로 Preview·Confirm하고, 실제 C 입력으로 예약 통나무·재건 부품 반환을 확인한 뒤 재배치와 HUD 작업 유지 입력으로 권위 20초 건설을 완료했다. Simulation 상태 사본은 h1:nature:workbench의 Operational과 ActiveWork 해제를 확정했고 Unity는 Synty Construction 작업대와 HUD를 표현했다. 전용 PlayMode 1/1(22.99초), Game View PNG, 마지막 재컴파일 이후 Console 오류 0건을 확인했다. | 수동 사람 조작에 의한 미세 시각 마감 평가 / 작업대 제작 애니메이션·효과음의 실제 수용 확인 / 운영 Provider와 운영 DB |
| `evidence:simulation-task-20260824` Simulation 전체 Task 회귀 | Logic | AutomatedTest | Passed/Stale | E3 | Simulation 솔루션 빌드, 코드 지도·E 책임 지도와 전체 Simulation 시험 764/764 | Unity 실제 입력 / Game View와 Console / Hosted RemoteHost 동등성 / 운영 Provider와 운영 DB |
| `evidence:town-arcana-core-20260825` Town 활성화별 아르카나와 NPC 생활복구 Core 문맥 | Logic | AutomatedTest | Passed/Current | E3, E4 | 활성화별 방향 가변·활성 중 불변, 고정 정밀도 51%, 단일 Fan-out, 물품 경쟁·대체 선택·소비·다음 욕구와 simulation-save.v16 집중 시험 8/8 | ORDER 01~07 전체 E5 세계 발현 / Town H1 이동과 실제 소비 표현 / Unity Play Mode·Game View / LocalProcess·RemoteHost hash 동등성 |

## 증거 경계

- 개별 WI 구현 E, PlayableUnit E와 영역 집계 E를 서로 대신하지 않는다.
- H 공간 조립은 필요한 공간 능력의 조건부 증거이며 E5·E7을 자동 승격하지 않는다.
- 자동 시험, Unity Play Mode, Game View, Hosted 동등성과 운영 효과를 별도 EvidencePackage로 기록한다.
- 증거가 `Stale`이면 과거 범위는 보존하되 현재 완결 판정의 단독 근거로 확대하지 않는다.

## 현재 최우선 실행 순서

1. **Nature 보관·수면·Day2 반환** — SkyEngineTests와 canonical Scene 다섯 날씨 상태·오두막 내부 강수 차폐를 검증한다.
2. **Nature 작업대 기반** — Table Saw·목재·상자·조명을 배치 통제 계층에서 하나의 작업 구역으로 조립하고 건설·취소·운영 화면 차이를 재검증한다.
3. **Nature 현장 성과·거점 제작·다음 원정 왕복** — WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.
4. **Nature 거점 성찰·다음 원정 준비** — 공용 E1~E3 계약·시험을 기준선으로 삼아 WI-REFLECT-01과 기존 Nature 오두막 H1을 결속한다.
5. **Nature 건물 발전·배움 확장** — 기존 StableId를 유지한 채 배움터 Extension의 NPC 생활 주기를 E7→E1로 검토하고 가장 낮은 미완료 의존성부터 구현한다.
6. **Nature 지역 위협 후퇴·복원·회복** — Nature 핵심 첫날 폐루프와 분리된 Extension Goal로 선택될 때 WI-NATURE-02부터 E4→E7을 진행한다.
7. **Farm 경작·성장·수확** — 감자 독립 Fixture로 경작→수확 Lot과 재시도 상태를 닫는다.
8. **Farm 집하·포장·내부 보관 반환** — 새 WI를 성급히 만들지 않고 WI-FARM-06 완료 결과에 내부 보관·재생산 반환 계약을 먼저 확정한다.
9. **Farm 플레이어 배치 확장** — Farm 핵심 생산 폐루프와 분리해 배치 Preview·Confirm·Tick·Save 계보를 전용 증거로 만든다.
10. **Hub 입고·검수·적치** — Hub 자체 입고 Lot과 용량·검수 실패·재선택 계약을 E3로 구현한다.
11. **Hub 출고 준비·작업 반환** — Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다.
12. **Town 주문·소비·다음 욕구** — 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.
13. **Town 메이저 아르카나 문맥 확장** — 핵심 주문 폐루프와 분리된 확장 Fixture에서 방향 판정·단일 전개·저장 복원을 발현한다.
14. **City 수요·서비스·결과 반환** — 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다.
