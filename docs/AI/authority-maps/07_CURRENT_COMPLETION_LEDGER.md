# 현재 플레이 폐루프 완결 원장

> 이 문서는 `eng/execution-ledgers/playable-loops.json`와 `eng/execution-ledgers/evidence-packages.json`에서 자동 생성된다. 직접 수정하지 않는다.
> CoreClosed는 E5 핵심 폐루프, ExtendedClosed는 선택형 확장 E5, PlayClosed는 E7 실제 플레이 폐루프, WorldClosed는 E8 NPC 판단→행동→결과→다음 판단 폐루프를 뜻한다.

- 플레이 폐루프 대장: `ssalddel-playable-loop-catalog.r6`
- 증거 묶음 대장: `ssalddel-evidence-package-catalog.r6`
- 플레이 단위·집계: `21`
- 등록 증거 묶음: `11`

## 판정 기준

| 기준 | 판정 |
| --- | --- |
| 핵심 완결 | 모든 필수 Core 자식이 E5 CoreClosed 이상이어야 영역 집계가 닫힘 |
| 확장 완결 | 건물·타로·배치 같은 선택형 자식은 ExtendedClosed로 별도 기록하며 Core를 막지 않음 |
| 플레이 완결 | 실제 입력·화면·귀환까지 E7이 확인된 자식만 PlayClosed |
| 세계 완결 | 필수 NPC 판단→행동→결과→다음 판단까지 E8이 확인된 단위만 WorldClosed |
| 독립 영역 우선 | 플레이어 연속성 기준 Nature→Farm→Hub→Town→City의 내부 폐루프를 먼저 닫고 영역 간 연결은 뒤로 미룸 |

## 전체 상태

| 우선 | 폐루프 | 수준·등급 | WI | 현재→다음/최종 E | 완결 | 상태 | 증거 | 다음 행동 |
| ---: | --- | --- | ---: | --- | --- | --- | ---: | --- |
| 1 | `playable-loop:nature-survival-homestead.v1` Nature 생존 생활거점 영역 집계 | AreaAggregate/Aggregate | 0 | E4→E5/E8 | Open | InProgress | 2 | 생활 거점 기초와 밤→Day2 자식 폐루프를 각각 E5로 닫은 뒤 영역 CoreClosed를 파생 판정한다. |
| 2 | `playable-loop:nature-shelter-foundation.v1` Nature 도끼·벌목·오두막 기초 | PlayableUnit/Core | 7 | E6→E7/E7 | CoreClosed | InProgress | 4 | WI-NATURE-05 E7의 범위 밖 DailyWork 연결 오류를 제거하고 실제 Game View 수동 완주·청음 증거를 보강한다. |
| 3 | `playable-loop:nature-twilight-return.v1` Nature 황혼 위협 대응·귀환 | PlayableUnit/Core | 5 | E5→E5/E7 | CoreClosed | InProgress | 2 | CoreClosed를 유지하면서 E6 체감 정제와 E7 실제 입력·Hosted 동등성을 별도 증거로 만든다. |
| 4 | `playable-loop:nature-night-day2.v1` Nature 보관·수면·Day2 반환 | PlayableUnit/Core | 3 | E4→E5/E7 | Open | InProgress | 1 | 세 WI와 save.v17 복원을 하나의 결정적 첫날 종료 Fixture로 묶는다. |
| 5 | `playable-loop:nature-workbench-foundation.v1` Nature 작업대 기반 | PlayableUnit/Core | 2 | E5→E6/E7 | CoreClosed | InProgress | 1 | 작업대 Preview·Confirm·취소·완료·귀환을 canonical Scene 실제 입력과 Hosted hash로 검증한다. |
| 6 | `playable-loop:nature-field-supply-return.v1` Nature 현장 성과·거점 제작·다음 원정 왕복 | PlayableUnit/Core | 6 | E4→E5/E8 | Open | InProgress | 1 | WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다. |
| 7 | `playable-loop:nature-building-learning.v1` Nature 건물 발전·배움 확장 | PlayableUnit/Extension | 2 | E5→E5/E8 | ExtendedClosed | InProgress | 1 | 기존 StableId를 유지한 채 배움터 Extension의 NPC 생활 주기를 별도 E9→E1 작업 명세에서 정의한다. |
| 20 | `playable-loop:farm-internal-production.v1` Farm 독립 생산·보관 영역 집계 | AreaAggregate/Aggregate | 0 | E3→E5/E7 | Open | InProgress | 1 | 생산 주기와 포장·내부 보관 반환을 별도 Fixture로 닫은 뒤 영역 집계를 올린다. |
| 21 | `playable-loop:farm-crop-cycle.v1` Farm 경작·성장·수확 | PlayableUnit/Core | 4 | E3→E5/E7 | Open | InProgress | 1 | 감자 독립 Fixture로 경작→수확 Lot과 재시도 상태를 닫는다. |
| 22 | `playable-loop:farm-pack-store-return.v1` Farm 집하·포장·내부 보관 반환 | PlayableUnit/Core | 2 | E3→E5/E7 | Open | InProgress | 1 | 새 WI를 성급히 만들지 않고 WI-FARM-06 완료 결과에 내부 보관·재생산 반환 계약을 먼저 확정한다. |
| 23 | `playable-loop:farm-player-placement.v1` Farm 플레이어 배치 확장 | PlayableUnit/Extension | 2 | E3→E5/E7 | Open | InProgress | 1 | Farm 핵심 생산 폐루프와 분리해 배치 Preview·Confirm·Tick·Save 계보를 전용 증거로 만든다. |
| 30 | `playable-loop:hub-internal-warehouse.v1` Hub 독립 창고 영역 집계 | AreaAggregate/Aggregate | 0 | E2→E5/E8 | Open | Defined | 0 | 입고·적치 자식의 독립 Fixture부터 E3로 만든다. |
| 31 | `playable-loop:hub-inbound-putaway.v1` Hub 입고·검수·적치 | PlayableUnit/Core | 2 | E2→E5/E8 | Open | Defined | 0 | Hub 자체 입고 Lot과 용량·검수 실패·재선택 계약을 E3로 구현한다. |
| 32 | `playable-loop:hub-outbound-ready-return.v1` Hub 출고 준비·작업 반환 | PlayableUnit/Core | 3 | E3→E4/E8 | Open | InProgress | 1 | Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다. |
| 40 | `playable-loop:town-resident-market.v1` Town 주민 생활복구 영역 집계 | AreaAggregate/Aggregate | 0 | E4→E5/E8 | Open | InProgress | 1 | 타로 유무와 무관한 주문→소비→다음 욕구 E5 Fixture부터 닫는다. |
| 41 | `playable-loop:town-order-consume-return.v1` Town 주문·소비·다음 욕구 | PlayableUnit/Core | 7 | E4→E5/E8 | Open | InProgress | 1 | 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다. |
| 42 | `playable-loop:town-arcana-context.v1` Town 메이저 아르카나 문맥 확장 | PlayableUnit/Extension | 1 | E4→E5/E7 | Open | InProgress | 1 | 핵심 주문 폐루프와 분리된 확장 Fixture에서 방향 판정·단일 전개·저장 복원을 발현한다. |
| 50 | `playable-loop:city-urban-logistics.v1` City 독립 도심 서비스 영역 집계 | AreaAggregate/Aggregate | 0 | E1→E5/E8 | Open | Defined | 0 | 도심 수요→재고 배정→서비스→결과 확인 자식을 E2/E3로 구현한다. |
| 51 | `playable-loop:city-demand-service-return.v1` City 수요·서비스·결과 반환 | PlayableUnit/Core | 4 | E1→E5/E8 | Open | Defined | 0 | 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다. |
| 90 | `playable-loop:solo-world-day.v1` Solo 하루 세계 집계 | WorldAggregate/Aggregate | 1 | E1→E5/E7 | Open | Defined | 0 | 영역 간 운송을 선행시키지 말고 각 영역 CoreClosed 뒤 하루 선택 집계를 조립한다. |
| 100 | `playable-loop:nature-farm-roundtrip.v1` Nature↔Farm 선택적 왕복 | WorldAggregate/Aggregate | 5 | E1→E7/E7 | Open | Deferred | 0 | 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다. |

## 영역 집계와 자식 상태

- **Nature 생존 생활거점 영역 집계** `Open`
  - 필수 Core: `playable-loop:nature-shelter-foundation.v1` — E6, CoreClosed
  - 필수 Core: `playable-loop:nature-twilight-return.v1` — E5, CoreClosed
  - 필수 Core: `playable-loop:nature-night-day2.v1` — E4, Open
  - 필수 Core: `playable-loop:nature-workbench-foundation.v1` — E5, CoreClosed
  - 필수 Core: `playable-loop:nature-field-supply-return.v1` — E4, Open
  - 선택 Extension: `playable-loop:nature-building-learning.v1` — E5, ExtendedClosed
- **Farm 독립 생산·보관 영역 집계** `Open`
  - 필수 Core: `playable-loop:farm-crop-cycle.v1` — E3, Open
  - 필수 Core: `playable-loop:farm-pack-store-return.v1` — E3, Open
  - 선택 Extension: `playable-loop:farm-player-placement.v1` — E3, Open
- **Hub 독립 창고 영역 집계** `Open`
  - 필수 Core: `playable-loop:hub-inbound-putaway.v1` — E2, Open
  - 필수 Core: `playable-loop:hub-outbound-ready-return.v1` — E3, Open
- **Town 주민 생활복구 영역 집계** `Open`
  - 필수 Core: `playable-loop:town-order-consume-return.v1` — E4, Open
  - 선택 Extension: `playable-loop:town-arcana-context.v1` — E4, Open
- **City 독립 도심 서비스 영역 집계** `Open`
  - 필수 Core: `playable-loop:city-demand-service-return.v1` — E1, Open

## 현재 닫힌 E5 단위

- `playable-loop:nature-shelter-foundation.v1` — CoreClosed (E6)
- `playable-loop:nature-twilight-return.v1` — CoreClosed (E5)
- `playable-loop:nature-workbench-foundation.v1` — CoreClosed (E5)
- `playable-loop:nature-building-learning.v1` — ExtendedClosed (E5)

## 열린 경계

- `playable-loop:nature-night-day2.v1`
  - 보관→수면→새벽 계획→Day2Ready를 한 E5 반환 Fixture로 판정한 증거가 없다.
- `playable-loop:nature-field-supply-return.v1`
  - 직접 제작과 NPC 위임 Core 자동시험 뒤에도 실제 H1 작업대 입력, LocalProcess·RemoteHost revision별 hash 동등성과 Unity 결과 피드백이 필요하다.
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
  - 지속 NPC 생활세계 E8은 범위 밖이다.
- `playable-loop:town-order-consume-return.v1`
  - 배터리 경쟁·대체 물품·소비 Effect 뒤 다음 목표까지 한 E5 생활세계 Fixture로 판정하지 않았다.
- `playable-loop:town-arcana-context.v1`
  - 활성화 Snapshot은 시험됐지만 Town 핵심 폐루프에 적용된 최종 E5 계보와 Unity 설명 화면이 없다.
- `playable-loop:city-demand-service-return.v1`
  - 계약만 있으며 권위 Core·Save/Replay·Fixture가 없다.

## 보류 또는 독립 준비 후 통합

- `playable-loop:nature-farm-roundtrip.v1`: 두 독립 영역이 E7로 닫힐 때까지 대표 다음 작업으로 선택하지 않는다.

## 현재 증거 묶음

| 증거 | 종류 | 결과·상태 | 대상 E | 범위 | 제외 |
| --- | --- | --- | --- | --- | --- |
| `evidence:hub-npc-routine-core-20260825` Hub 운영 유래 WI NPC 루틴 Core | AutomatedTest | Passed/Current | E3 | 독립 Hub 재고 Fixture, 전용 출고 NPC·정책, WI-HUB-03~05 결정적 WorldTick, 플레이어 직접 Confirm 차단, 정책·담당자 차단, 취소 반환, Local·Hosted 읽기 모델, simulation-save.v21 및 관련 FarmSupply·AreaBuilding·WI 성숙도 회귀 35/35 | Hub PickingWorkArea·OutboundStagingArea H 공간 결속 / WI-HUB-06 차량 상차와 외부 운송 / Unity Play Mode·Game View / 지속 NPC 생활세계 E8 / 운영 Provider와 운영 DB |
| `evidence:nature-building-core-20260825` Nature 건물 발전·배움 확장 Core | AutomatedTest | Passed/Current | E3, E4, E5 | Nature r3 작업대·배움터 건설, 비용·선행·발자국·취소 반환·NPC 학습 방문과 simulation-save.v19의 결정적 Fixture 집중 시험 5/5 | Farm·Town·Hub·City 실제 건설 상태 전이 / Unity Play Mode 배치 입력과 Game View / Hosted RemoteHost hash 동등성 / 외부 Provider와 운영 DB |
| `evidence:nature-dual-combat-core-20260825` Nature 관찰 운영·직접 개입 Core와 LocalProcess 폐루프 | AutomatedTest | Passed/Current | E3, E4, E5 | 관찰 방식 잠금·카드 hash·자동 행동·단일 비상 개입, 직접 S/A/B 성과, Nature 서버 전용 추가 보상, HTTP 없는 LocalProcess 전투, simulation-save.v17 읽기 호환과 v18 슬롯 복원, 세션 API route 회귀 57/57 | 실제 Unity Play Mode 입력과 Game View / LocalProcess·RemoteHost WorldRevision별 hash 동등성 / E6 난도·보상 체감 정제 / 운영 Provider와 운영 DB |
| `evidence:nature-dual-combat-unity-editmode-20260825` Nature 이중 전투 Unity 컴파일과 공식 Scene 배선 | UnityEditMode | Passed/Current | E3, E4 | 공통 RuntimeCore와 Local battle Adapter 스크립트 컴파일, 현장전투 입력 번역과 저장 SimulationWorldShell 배선 4/4 | 관찰 일시정지·비상 카드의 실제 Input System 조작 / 1인칭 직접 전투 완주 / Play Mode·Game View·Console / Hosted RemoteHost |
| `evidence:nature-field-supply-core-20260825` Nature 현장 성과·거점 제작·다음 원정 왕복 Core | AutomatedTest | Passed/Current | E3, E4 | Nature r4 직접 제작 WI-NATURE-16을 보존하고 npc-routine-control.r3 정책 비활성·활성, 적격 NPC 결정, 입력 없는 4초 WI-NATURE-17 위임 제작, 직접 제작 경합, 취소 반환, 보급 꾸러미 원정 준비·패배 보호와 simulation-save.v20·v21 결정적 재생 집중 시험 및 Nature 생활핵 CraftingWorkArea 문맥 결속 | Unity Play Mode 작업대 입력과 Game View / Hosted RemoteHost revision별 hash 동등성 / 별도 다중 Lot 재고 원장 / 외부 Provider와 운영 DB |
| `evidence:nature-playmode-20260824` Nature Solo LocalProcess PlayMode | UnityPlayMode | Passed/Stale | E7 | 실제 Input System의 도끼 획득·C 취소·B 배치·E 퇴장·R 후퇴와 저장·Scene 재진입 복원 1/1 | 수동 Game View 시각 수용 / 오류 없는 Console 캡처 / Hosted RemoteHost / 전체 후속 경로의 E5 Manifested 판정 |
| `evidence:nature-r2-core-20260825` Nature 첫날 r2 Core와 Unity 컴파일 | AutomatedTest | Passed/Stale | E3 | WI-NATURE-13~15, r2 보관·위협·WorldLocal 전투 후퇴 인계·전투 결과·수면·Day2 계획·simulation-save.v17 집중 시험 31/31, Simulation 빌드, Unity 스크립트 컴파일 | 실제 직접 전투의 LocalProcess 권위 연결 / Hosted RemoteHost revision hash 동등성 / Play Mode 전체 첫날 완주 / 수동 Game View와 Console 수용 |
| `evidence:nature-shelter-hosted-parity-20260825` Nature 생활거점 LocalProcess·RemoteHost 동등성 | HostedParity | Passed/Current | E5, E7 | 취소·재수확·오두막 건설·입장·퇴장·저장·Replay verification 명령 15개를 LocalProcess와 RemoteHost에 적용해 최종 revision 15, simulation-save.v23과 replay hash 83a5582724e76d95f8a3344bf86ee7ea56703edbe708aaaf1fa920a131ede0e0 동등성을 확인했다. | canonical Scene의 사람 수동 완주 / 실제 음향 청취와 최종 화면 캡처 / 운영 Provider와 운영 DB |
| `evidence:nature-shelter-playmode-20260825` Nature 생활거점 실제 입력 E7 후보 | UnityPlayMode | Partial/Current | E5, E6, E7 | 실제 Input System으로 도끼 획득, 벌목 취소·재시도, 세 나무 벌목, 오두막 배치·건설, 입장·퇴장, HUD 저장과 Scene 재진입 복원을 자동 PlayMode 1/1로 검증하고 직접 Game View에서 시점·조준·실패 피드백·저장 입력을 확인했다. | 사람이 도끼 획득부터 저장 복원까지 전 과정을 수동 완주한 증거 / 실제 음향 청취와 승인 Nature Ambient·BGM / 재로드 때 발생하는 기존 Scene 직렬화 경고 분리 / 운영 Provider와 운영 DB |
| `evidence:simulation-task-20260824` Simulation 전체 Task 회귀 | AutomatedTest | Passed/Stale | E3 | Simulation 솔루션 빌드, 코드 지도·E 책임 지도와 전체 Simulation 시험 764/764 | Unity 실제 입력 / Game View와 Console / Hosted RemoteHost 동등성 / 운영 Provider와 운영 DB |
| `evidence:town-arcana-core-20260825` Town 활성화별 아르카나와 NPC 생활복구 Core 문맥 | AutomatedTest | Passed/Current | E3, E4 | 활성화별 방향 가변·활성 중 불변, 고정 정밀도 51%, 단일 Fan-out, 물품 경쟁·대체 선택·소비·다음 욕구와 simulation-save.v16 집중 시험 8/8 | ORDER 01~07 전체 E5 세계 발현 / Town H1 이동과 실제 소비 표현 / Unity Play Mode·Game View / LocalProcess·RemoteHost hash 동등성 |

## 증거 경계

- 개별 WI 구현 E, PlayableUnit E와 영역 집계 E를 서로 대신하지 않는다.
- H 공간 조립은 필요한 공간 능력의 조건부 증거이며 E5·E7을 자동 승격하지 않는다.
- 자동 시험, Unity Play Mode, Game View, Hosted 동등성과 운영 효과를 별도 EvidencePackage로 기록한다.
- 증거가 `Stale`이면 과거 범위는 보존하되 현재 완결 판정의 단독 근거로 확대하지 않는다.

## 현재 최우선 실행 순서

1. **Nature 보관·수면·Day2 반환** — 세 WI와 save.v17 복원을 하나의 결정적 첫날 종료 Fixture로 묶는다.
2. **Nature 현장 성과·거점 제작·다음 원정 왕복** — WI-NATURE-16·17의 실제 공간 발현과 Hosted 동등성을 검증한 뒤 E4·E5를 판정한다.
3. **Farm 경작·성장·수확** — 감자 독립 Fixture로 경작→수확 Lot과 재시도 상태를 닫는다.
4. **Farm 집하·포장·내부 보관 반환** — 새 WI를 성급히 만들지 않고 WI-FARM-06 완료 결과에 내부 보관·재생산 반환 계약을 먼저 확정한다.
5. **Farm 플레이어 배치 확장** — Farm 핵심 생산 폐루프와 분리해 배치 Preview·Confirm·Tick·Save 계보를 전용 증거로 만든다.
6. **Hub 입고·검수·적치** — Hub 자체 입고 Lot과 용량·검수 실패·재선택 계약을 E3로 구현한다.
7. **Hub 출고 준비·작업 반환** — Hub H 공간 능력을 결속한 뒤 WI-HUB-03~05의 E4 실행 문맥과 E5 세계 발현을 검증한다.
8. **Town 주문·소비·다음 욕구** — 타로 배율을 끈 기준선에서도 욕구→경쟁→소비→다음 욕구가 결정적으로 닫히게 한다.
9. **Town 메이저 아르카나 문맥 확장** — 핵심 주문 폐루프와 분리된 확장 Fixture에서 방향 판정·단일 전개·저장 복원을 발현한다.
10. **City 수요·서비스·결과 반환** — 기존 도심마트 파생 엔진을 상태 권위로 오인하지 않고 독립 City 명령 Aggregate부터 만든다.
