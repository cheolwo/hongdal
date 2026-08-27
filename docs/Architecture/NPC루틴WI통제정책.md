# NPC 루틴 WI 통제 정책

## 목적

운영 서버에서 유래한 창고·주문·운송 업무 의미는 Simulation에서도 재사용하지만, 그것을 플레이어가 업무 화면에서 하나씩 직접 완료하는 게임으로 만들지는 않는다. 기본 게임 구조는 **NPC가 반복 업무를 수행하고 플레이어는 정책·우선순위·예외를 결정하는 운영 개입**이다.

이 정책은 운영 시스템을 Simulation으로 복제하거나 실제 운영 효과를 실행한다는 뜻이 아니다. 운영 유래 코드는 세계 상호작용(WI)의 업무 의미 후보를 제공할 뿐이고, 순서·분기·반복은 별도 WI 조립 흐름이 소유한다. 모든 실행 상태는 Simulation 전용 Fixture와 Simulation Core가 소유한다.

## 다섯 분류 축

WI 정의와 실행 인스턴스는 다음 축을 섞지 않는다.

| 축 | 질문 | 예 |
| --- | --- | --- |
| `originCode` | 이 업무 의미는 어디에서 유래했는가? | `OperationsDerived`, `SimulationNative`, `Hybrid` |
| `controlPolicyCode` | 게임에서 누가 어떤 수준으로 통제하는가? | `NpcRoutine`, `PlayerOrNpc`, `PlayerDirect`, `WorldAutomatic` |
| `triggerSourceCode` | 이번 실행은 무엇 때문에 시작됐는가? | `NpcDriven`, `PlayerDriven`, `WorldDerived`, `DataDriven` |
| `ActorBinding` | 실제로 시간·공간·자원을 사용해 수행한 주체는 누구인가? | `PlayerActor`, `NpcActor`, `NotApplicable` |
| `ActorActionPurpose` | 이번 행동은 직접 개입인가, 상태 수렴인가? | `Yang`, `Yin`, `Contextual`, `NotApplicable` |

따라서 운영 유래 WI가 언제나 NPC 발생이라는 뜻은 아니며, 같은 Farm 수확 WI는 플레이어와 NPC가 함께 사용할 수 있다. 반대로 Hub의 반복 출고 준비는 `OperationsDerived + NpcRoutine`으로 분류하고 실제 실행 인스턴스의 출고 요청은 `NpcDriven`, 뒤따르는 피킹·출고 준비 전이는 `WorldDerived`로 기록한다.

같은 의도·대상·주요 결과를 Player와 NPC가 수행한다는 이유만으로 WI를 둘로 복제하지 않는다. WI는 Actor와 무관한 atomic 책임을 소유하고 Actor는 실행 인스턴스에 결속한다. 반대로 “NPC에게 위임한다”와 “NPC가 실제 작업한다”는 서로 독립적으로 실패·취소될 수 있으므로 별도 책임이다. 상세 판정은 [WI 단일 책임 원칙](WI단일책임원칙.md)을 따른다.

음양 사분면의 둘째 부호는 `triggerSourceCode`가 아니라 실제 `ActorBinding`을 읽는다. Player가 정책을 선택한 `PlayerDriven` 실행이어도 NPC가 작업하면 NPC 부호이며, Actor 없는 자동 전이는 NPC로 위장하지 않는다. 상세 기준은 [WI 음양·수행주체 사분면 체계](WI음양수행주체사분면체계.md)를 따른다.

## 플레이어가 하는 일

플레이어는 NPC 업무 결과를 관찰하고 다음 운영 결정을 내린다.

- 자동 배정과 자동 위임을 켜거나 끈다.
- 업무 정책의 우선순위와 선호 담당자를 바꾼다.
- 완료 전 작업을 취소하거나 차단 원인을 해소한다.
- 시설 용량·작업 공간·인력 역량을 개선한다.
- 어떤 영역과 업무 묶음에 자원을 우선 배분할지 선택한다.

플레이어가 해서는 안 되는 일은 NPC의 피킹·포장·적치 완료 상태를 직접 주입하거나, Unity 애니메이션으로 업무 완료를 확정하거나, 운영 서버의 실제 주문·재고·운송 상태를 바꾸는 것이다.

## 호환 프로필

`npc-routine-control.r1`을 세션 생성 시 명시한 경우에만 새 통제 정책을 적용한다. 프로필을 지정하지 않은 기존 세션은 과거 직접 Confirm 경로와 `simulation-save.v20` 이하의 정규 hash 의미를 유지한다.

새 프로필의 목표 계약은 `NpcRoutine`으로 분류된 WI의 플레이어 직접 Confirm을 거부하는 것이다. 내부적으로 신뢰된 NPC 루틴과 재생기만 기존 상태 전이 Core를 사용할 수 있다. 현재 실제 차단과 NPC 실행이 함께 연결된 첫 세로 조각은 Hub 내부 출고 준비이며, 다른 운영 유래 WI는 분류를 마쳤지만 각 직접 API 차단과 NPC 폐루프를 아직 연결하지 않았다. 분류만으로 차단 완료를 주장하지 않는다.

## 첫 세로 조각: Hub 내부 출고 준비

외부 Farm 화물이나 차량 운송 없이 Hub 자체 Fixture로 닫는다.

```text
Hub 저장 재고
  → 전용 출고 준비 정책 평가
  → 결정적 NPC 담당자 선택
  → WI-HUB-03 출고 요청 (NpcDriven)
  → WI-HUB-04 피킹 완료 (WorldDerived)
  → WI-HUB-05 출고 준비 완료 (WorldDerived)
  → 다음 Hub 업무 선택 가능
```

`WI-HUB-06` 차량 상차와 외부 운송은 이 폐루프에 포함하지 않는다. 후보가 여럿이면 정책 우선순위, 재고 갱신 Tick, 고유 식별자 순으로 선택한다. 한 Tick에는 한 후보만 시작하며, 자동화가 꺼져 있거나 적격 NPC가 없거나 재고가 부족하면 작업을 조작해 성공시키지 않고 차단 사유를 읽기 모델에 남긴다.

## 권위·조회·저장

- Solo는 `LocalSimulationRuntime`, Hosted는 `Simulation.Server`에서 같은 Simulation Core를 실행한다.
- Hosted 조회는 `GET /api/simulation/v1/sessions/{sessionStableId}/npc-routine-work?areaCode=Hub`를 사용한다.
- Unity에는 원운영 자료나 쓰기 권한 대신 업무 단계, 담당 NPC, 재고, 발생원, 통제 정책, 차단 사유와 허용 개입을 읽기 전용으로 제공한다.
- `simulation-save.v21`은 프로필, NPC 루틴 실행 계보와 상태를 정규 hash에 포함한다.
- v21 복원은 저장된 실행 계보를 최신 정책으로 다시 만들어 바꾸지 않는다. 기존 v20 이하 hash에는 새 필드를 넣지 않는다.
- 새 음양·수행주체 상태 사본이 있는 실행은 `simulation-save.v23`을 사용하고 판정 revision·근거와 사분면을 hash에 포함한다. v21·v22 복원은 원 판본과 정규 hash를 유지한다.
- Provider 호출, 운영 DB 쓰기와 운영 효과는 Session 생성·조회·`WorldTick` 경로에 존재하지 않는다.

## 현재 증거와 한계

현재 증거는 계약·Core·결정성·Save/Replay·Hosted HTTP와 Local Adapter의 자동 시험인 E3까지다. Hub의 실제 `PickingWorkArea`와 `OutboundStagingArea` H 결속, `SimulationWorldShell`의 NPC 이동·업무 설명 화면, Play Mode·Game View, 다른 Core와의 장기 NPC 생활 조화는 검증하지 않았다. 따라서 결정적 Fixture가 동작해도 E5 세계 발현, 소속 PlayableUnit E7, E8 반복 안정성이나 E9 영역 조화로 승격하지 않는다.

관련 기계 판독 원장은 다음과 같다.

- [`world-interactions.json`](../../eng/execution-ledgers/world-interactions.json)
- [`world-interaction-trigger-sources.json`](../../eng/execution-ledgers/world-interaction-trigger-sources.json)
- [`world-interaction-responsibilities.json`](../../eng/execution-ledgers/world-interaction-responsibilities.json)
- [`world-interaction-flows.json`](../../eng/execution-ledgers/world-interaction-flows.json)
- [`playable-loops.json`](../../eng/execution-ledgers/playable-loops.json)
- [`hub-npc-routine-outbound.e9-work-order.json`](../../eng/execution-ledgers/work-orders/hub-npc-routine-outbound.e9-work-order.json)
