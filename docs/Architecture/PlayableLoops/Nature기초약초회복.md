# Nature 기초 약초 회복

## 식별과 근거

- 주제 고유 식별자: `topic:nature-basic-herbal-recovery.v1`
- PlayableLoop 고유 식별자: `playable-loop:nature-basic-herbal-recovery.v1`
- 기획 revision: `nature-basic-herbal-recovery.design.r3`
- 원천 기획 문서:
  - `docs/Architecture/PlayableLoops/PlanningSessions/nature-night-day2.inquiry.r1.md`
- 마지막으로 반영한 문답 revision: `Q-089` 개발 전환 요청. 이 주제의 직접 결정은 Q-045~Q-050, Q-061~Q-064, Q-068~Q-071이다.
- 문답에서 남은 승인 차단 미정: 약초 종류·조합·정밀 수치, 실제 조리 배치와 애니메이션은 후속 기획 대상으로 남긴다. 이번 판본은 기존 `WI-ACTOR-03`의 읽기 전용 Recipe 지식 카드 투영만 E3까지 추가 승인한다.

## 플레이어 약속과 재미

- 플레이어가 처한 상황: Nature에서 추위와 질병 위험이 누적되고, 주변 식물과 버려진 기록에서 회복 방법을 찾아야 한다.
- 플레이어가 원하는 것: 발견한 처방을 자기 지식으로 남기고, 맞는 약초를 모아 따뜻한 차를 만들어 초기 질병과 체온을 회복한다.
- 반복해도 재미있어야 하는 핵심 선택: 위험을 감수해 지식을 찾을지, 이미 아는 처방의 재료를 모을지, 지금 마실지 더 심한 상황에 대비할지 선택한다.
- 짧은 플레이어 약속 한 문장: 기록에서 기초 처방을 배우고 약초를 모아 따뜻한 차를 달여 마심으로써 체온과 질병 위험을 관리한다.

## 반복 폐루프

`처방 단서 발견 → 읽기 Preview → 지식 습득 Confirm → 약초 채집 → 달이기 → 마시기 → 체온·초기 질병 회복 → 다음 탐색 또는 휴식`

- 진입 상태: 플레이어가 Nature에서 이동 가능하며, 처방 기록 또는 지역 기초 처방 단서에 접근할 수 있다.
- 종료 뒤 다시 열리는 선택: 같은 처방의 재료를 다시 모으거나, 다른 영역에서 상위 처방을 찾거나, 휴식·탐색을 선택한다.

## 선택·대가·성공·실패·회복

- 선택지: 처방을 지금 읽기, 나중에 읽기, 이미 아는 처방은 건너뛰기.
- 자원·시간·위험 대가: 첫 인계 범위인 지식 습득은 자원을 소비하지 않는다. 후속 채집·달이기는 시간·연료·재료를 소비한다.
- 성공 결과: 승인된 `RecipeStableId`가 플레이어 지식 원장에 한 번 기록되고 같은 revision의 행위 기록이 남는다.
- 실패 결과: 접근 불가, 알 수 없는 처방, 예상 revision 불일치에서는 상태가 바뀌지 않는다.
- 실패 뒤 회복 경로: 올바른 기록에 접근하거나 최신 상태 사본을 다시 읽은 뒤 Preview부터 재시도한다.

## WI 단일 책임 후보

| 순서 | WI 후보 | 한 번에 바꾸는 권위 상태 | 주체 | 비고 |
| --- | --- | --- | --- | --- |
| 1 | `WI-ACTOR-03` 지식 습득 | 플레이어 지식 원장에 `RecipeStableId` 하나를 멱등 추가 | Player | 이번 첫 개발 인계의 유일한 활성 WI |
| 2 | `WI-NATURE-HERB-GATHER` 후보 | 승인 식물 노드의 수량을 줄이고 약초 소지량을 늘림 | Player | 후속 문답·WI 번호 승인 전 구현 금지 |
| 3 | `WI-CRAFT-BREW` 후보 | 재료·연료를 소비하고 약초차 인스턴스를 생성 | Player | 후속 문답·WI 번호 승인 전 구현 금지 |
| 4 | `WI-ACTOR-CONSUME` 후보 | 약초차를 소비하고 체온·질병 상태에 효과 적용 | Player | 후속 문답·WI 번호 승인 전 구현 금지 |

## 논리·표현 요구

- 논리적으로 반드시 성립할 상태와 규칙: 지식 습득 Preview는 무변경이며 Confirm만 지식 원장·행위 기록·WorldRevision을 변경한다. 같은 처방의 재확정은 중복 항목을 만들지 않는다.
- 플레이어가 표현에서 식별해야 할 대상: 처방 기록, 읽을 수 있음, 이미 아는 처방, 현재 차단 상태. E2~E3에서는 실제 화면을 만들지 않고 카드 서랍이 소비할 결정적 관점별 조회 결과를 만든다.
- 결과가 같은 revision임을 보여줄 피드백: 지식 원장과 Preview의 `WorldRevision`이 다르면 투영을 거부하고, 카드 가족 상태 사본의 `SourceRevision`은 해당 권위 revision과 같아야 한다.
- 공통 표현 검증 모듈 외 조건 모듈: E3에서는 Recipe 지식 카드 가족의 결정성·읽기 전용 경계·상태 구분 자동시험을 적용한다. 실제 GameObject·입력·Game View는 E4 이후 별도 승인한다.

## H 공간과 자산 요구

- 필요한 H1~H5 능력: 후속 표현 단계에서 `ReadableKnowledgeSource`와 `HerbGatheringArea`가 필요하다.
- 실외·실내 배치 요구: 이번 첫 인계에는 좌표·배치를 정의하지 않는다.
- Synty 자산 후보와 대체 표현: Nature 식물·Generic 병/머그/냄비·Town 책/종이를 의미 기반 `VisualKey` 후보로만 둔다. 자산은 권위 상태를 바꾸지 않는다.
- Traversal, Collider, NavMesh 요구: 이번 첫 인계에는 해당 없음.

## 전문 심화 연구 판정과 재결속

| 분야 | 필요성 | 연구 문서 참조 또는 NotRequired 사유 | 상태 | 기획서 반영 항목 |
| --- | --- | --- | --- | --- |
| 건물 | `NotRequired` | E3 Recipe 지식 카드 투영은 건물 구조를 만들지 않는다. | `NotRequired` | Presentation E4 전에 다시 판정 |
| 공간 | `NotRequired` | E3 관점별 조회 결과는 실제 배치·접근 공간을 다루지 않는다. | `NotRequired` | Presentation E4 전에 다시 판정 |
| 배치 | `NotRequired` | 처방 기록과 식물의 실제 배치는 E3 범위 밖이다. | `NotRequired` | Presentation E4 전에 다시 판정 |
| 애니메이션 | `NotRequired` | E3는 읽기·달이기 애니메이션을 구현하지 않는다. | `NotRequired` | Presentation E4 전에 다시 판정 |

- `requiredDetailStudyRefs`의 모든 `Required` 연구가 `Accepted`인지: E3 카드 상태 사본 인계에는 `Required` 연구가 없다.
- 연구 결과로 다시 연 Logic E와 이유: 없음.
- 연구 결과로 다시 연 Presentation E와 이유: Presentation E4를 시작할 때 네 분야를 다시 판정한다.
- 연구끼리 충돌한 사항과 기획 판단: 없음.
- 개발 인계에 고정할 측정값·자산 fallback·검증법: `RecipeStableId` 멱등 추가, Preview 무변경, Confirm revision 증가, 카드의 권위 revision 일치, 입력 순서와 무관한 결정적 카드 정렬, 읽기 가능·이미 앎·차단 구분 자동시험. 약효 수치와 Synty 표현은 고정하지 않는다.

## 저장·권위·외부 경계

- Simulation 권위 상태: 플레이어별 `KnownRecipeStableIds`, 지식 습득 행위 기록, WorldRevision.
- Save/Replay에 고정할 값: 첫 구현에서는 새 Save 판본을 만들지 않는다. 지식 원장의 저장 계약은 후속 작업 명세에서 별도 승인받는다.
- LocalProcess/RemoteHost 동등성: 두 번째 인계는 같은 Application 서비스를 `LocalSimulationRuntime`과 `RemoteHost` Adapter가 호출하도록 연결하고, 같은 Preview·Confirm 입력이 같은 상태 사본과 WorldRevision을 만드는지 검증한다. 규칙을 Host별로 복제하지 않는다.
- 외부 Provider 또는 운영 효과 제외: 기상청 API, 상품 API, 운영 DB, 멀티플레이 지식 전수, 실제 Synty 배치는 제외한다.

## 제외 범위와 승인

- 이번 주제에서 하지 않는 것: 실제 약초 채집·달이기·섭취, 약효 수치, 심한 질병 치료, Recipe 전수, 카드에서 Confirm 실행, Unity Scene·배치·애니메이션·Play Mode·Game View, Save 판본 증가, 자동 저장 구현.
- 검토할 사람 또는 근거: `nature-night-day2.inquiry.r1`의 Q-045~Q-050·Q-061~Q-064·Q-068~Q-071과 2026-08-28 Q-089 뒤 E3 개발 전환 요청.
- 승인 근거 참조: `docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md`
- 승인 상태: `Approved`

## 첫 개발 인계 결과

- 활성 WI: `WI-ACTOR-03` 지식 습득
- 완료: `Logic E3 / Presentation E1 / 통합 E1`
- 증거: 지식 원장·Preview 무변경·Confirm 멱등 추가·행위 기록·거부 경계와 집중 시험 `8/8`

## 두 번째 개발 인계 상한

- 활성 WI: `WI-ACTOR-03` 지식 습득
- 목표: Logic 에비던스 `E4`만 추가 검증
- 허용: 기존 Application 서비스의 `LocalSimulationRuntime` Adapter, 동일 서비스에 연결되는 RemoteHost API/Adapter, Query·Preview·Confirm 계약, LocalProcess/RemoteHost 결과 동등성 집중시험
- 금지: Save 판본·자동 저장, Presentation E2 이상, Unity Scene·자산·배치·애니메이션, 약초 채집·달이기·섭취, Recipe 카드 UI, Logic E5 이상 승격 주장
- 완료 표현: `Logic E4 / Presentation E1 / 통합 E1` 이하로만 기록한다.

## 세 번째 개발 인계 상한

- 활성 WI: `WI-ACTOR-03` 지식 습득을 유지한다.
- 목표: 기존 Logic E4를 보존하면서 Presentation E2~E3를 추가해 통합 에비던스 `E3`까지 올린다.
- 허용: `Simulation플레이어지식LedgerSnapshot`과 같은 revision의 `Simulation지식습득PreviewSnapshot`을 읽는 Recipe 지식 카드 가족 투영, 읽기 가능·이미 앎·차단 상태, 결정적 정렬, 카드 서랍 공통 계약 소비, 집중 자동시험.
- 금지: 카드에서 지식 습득 Confirm 실행, Unity Scene·GameObject·실제 UI 배선, 실제 입력, Play Mode·Game View, Save 판본, 약초 채집·달이기·섭취·약효.
- 완료 표현: `Logic E4 / Presentation E3 / 통합 E3` 이하로 기록한다.
