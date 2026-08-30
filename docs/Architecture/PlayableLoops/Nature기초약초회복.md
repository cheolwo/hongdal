# Nature 기초 약초 회복

## 식별과 근거

- 주제 고유 식별자: `topic:nature-basic-herbal-recovery.v1`
- PlayableLoop 고유 식별자: `playable-loop:nature-basic-herbal-recovery.v1`
- 기획 revision: `nature-basic-herbal-recovery.design.r5`
- 원천 기획 문서:
  - `docs/Architecture/PlayableLoops/PlanningSessions/nature-night-day2.inquiry.r1.md`
- 마지막으로 반영한 문답 revision: `Q-089` 개발 전환 요청. 이 주제의 직접 결정은 Q-045~Q-050, Q-061~Q-064, Q-068~Q-071이다.
- 문답에서 남은 미정: 약초 종류·조합·정밀 수치, 실제 조리 배치와 애니메이션은 범위 밖 후속 기획이다. r5는 `WI-ACTOR-03` 하나의 Session·실제 책/카드·저장·복원·씬 검증을 E5 상한까지 승인한다. 전체 약초 폐루프의 승격 승인이 아니다.
- r5 승인 근거: 2026-08-30 [기존 WI 세계 발현 E5 개발 계획](../기존WI세계발현E5개발계획.md)의 사용자 구현 승인. 아래 r1~r4 인계 제한은 역사 기록이며 이번 WI의 r5 범위를 차단하지 않는다.

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
- 플레이어가 표현에서 식별해야 할 대상: 처방 기록, 읽을 수 있음, 이미 아는 처방, 현재 차단 상태. E2~E3에서는 카드 서랍이 소비할 결정적 관점별 조회 결과를 만들고, E4에서는 물리 기록 후보를 `ReadableKnowledgeSource`·의미 기반 `VisualKey`·fallback·Preview 앵커에 결속한다.
- 결과가 같은 revision임을 보여줄 피드백: 지식 원장과 Preview의 `WorldRevision`이 다르면 투영을 거부하고, 카드 가족 상태 사본의 `SourceRevision`은 해당 권위 revision과 같아야 한다.
- 공통 표현 검증 모듈 외 조건 모듈: 기존 E3/E4 검사를 유지하고 E5에서 지면·실내 지지면·카메라 가림 조건 모듈을 적용한다. 실제 GameObject·입력·Game View는 r5 범위에 포함한다.
- Presentation E4 준비 후보: 플레이어가 물리적인 처방 기록을 식별하는 순간을 기준으로 `ReadableKnowledgeSource`, 의미 기반 `Knowledge.Recipe.Record.OpenBook`, Town 열린 책 주 후보와 City·Generic 종이 및 Construction 클립보드 대체 후보를 조사하고 코드 계약으로 고정했다. 실제 위치·Collider·시인성이 미검증이므로 E5 준비 상태는 `Conditional`이며 이 E4 준비 계약만으로 E5를 승격하지 않는다.

## H 공간과 자산 요구

- 필요한 H1~H5 능력: 이번 E4 표현 준비에는 `ReadableKnowledgeSource`, 후속 채집에는 `HerbGatheringArea`가 필요하다.
- 실외·실내 배치 요구: [E5 공간·배치 연구](Nature지식습득-E5공간배치연구.r1.md)의 폐야영지 지지면·접근·기존 상호작용 거리 기준을 사용하고 실제 측정 결과를 배치 증거로 동결한다.
- Synty 자산 후보와 대체 표현: 첫 지식 출처의 주 후보는 Town `SM_Prop_BookOpen_01`, 대체 후보는 City·Generic 종이와 Construction `SM_Item_Clipboard_01`, 문맥 후보는 Nature `SM_Prop_CampFire_01`과 범용 상자·탁자다. 이후 약초·조리에는 Nature 식물·Generic 병/머그/냄비를 의미 기반 `VisualKey` 후보로만 둔다. 자산은 권위 상태를 바꾸지 않는다.
- Traversal, Collider, NavMesh 요구: 기존 플레이어 이동 표면·통로를 보존하고 책 선택 Collider와 지지면을 확인한다. 이 WI만을 위해 새로운 NavMesh/이동 규칙을 만들지 않는다.

## 전문 심화 연구 판정과 재결속

| 분야 | 필요성 | 연구 문서 참조 또는 NotRequired 사유 | 상태 | 기획서 반영 항목 |
| --- | --- | --- | --- | --- |
| 건물 | `NotRequired` | 기존 폐야영지와 지지 소품을 재사용하며 건물 외피·건설 규칙을 추가하지 않는다. | `NotRequired` | 새 숙소 불필요 |
| 공간 | `Required` | `study:nature-player-knowledge:spatial-placement.r1` | `Accepted` | 폐야영지 접근·기존 이동과 상호작용 거리 보존 |
| 배치 | `Required` | `study:nature-player-knowledge:spatial-placement.r1` | `Accepted` | 열린 책·지지면·Collider·같은 revision 결과 |
| 애니메이션 | `NotRequired` | 손으로 책을 집거나 페이지를 넘기는 연출 없이 실제 책과 카드로 판독한다. | `NotRequired` | 기존 이동/시점 유지, 조리 동작 제외 |

- `requiredDetailStudyRefs`의 모든 `Required` 연구가 `Accepted`인지: E3 카드 상태 사본 인계에는 `Required` 연구가 없다.
- 연구 결과로 다시 연 Logic E와 이유: 없음.
- 연구 결과로 다시 연 Presentation E와 이유: 실제 배치와 조건 모듈 결속을 위해 E4 준비를 재확인한 뒤 E5를 구현한다. 기존 통과 근거는 삭제하지 않는다.
- 연구끼리 충돌한 사항과 기획 판단: 없음.
- 개발 인계 기준: `RecipeStableId` 멱등 추가, Preview 무변경, 같은 Session revision의 행위 기록·카드, 결정적 정렬, 접근 실패 복구를 유지한다. 실제 Synty 책·지지면·fallback 기준은 연결된 r1 연구를 따른다. 약효 수치는 정하지 않는다.

## 저장·권위·외부 경계

- Simulation 권위 상태: 플레이어별 `KnownRecipeStableIds`, 지식 습득 행위 기록, WorldRevision.
- Save/Replay에 고정할 값: r5는 Session·Actor 귀속, KnownRecipeStableIds, 행위/명령 기록·멱등 정보와 복원 상태를 기존 Save에 포함한다. 다음 미사용 판본을 개발이 공통 예약하고 과거 판본 읽기·hash를 유지한다. 종료·재진입 후 지식이 사라지거나 중복 지급되지 않아야 한다.
- LocalProcess/RemoteHost 동등성: 두 번째 인계는 같은 Application 서비스를 `LocalSimulationRuntime`과 `RemoteHost` Adapter가 호출하도록 연결하고, 같은 Preview·Confirm 입력이 같은 상태 사본과 WorldRevision을 만드는지 검증한다. 규칙을 Host별로 복제하지 않는다.
- 외부 Provider 또는 운영 효과 제외: 기상청 API, 상품 API, 운영 DB, 멀티플레이 지식 전수는 제외한다. 실제 보유 Synty 배치는 r5에 포함한다.

## 제외 범위와 승인

- 이번 판본에서 하지 않는 것: 실제 약초 채집·달이기·섭취, 약효 수치, 심한 질병 치료, Recipe 전수/작성, 새 읽기 애니메이션, 새 자동 저장 정책, E6/E7 승격. 카드 Confirm은 유효한 물리 출처 접근을 권위가 확인할 때만 허용한다.
- 검토할 사람 또는 근거: `nature-night-day2.inquiry.r1`의 Q-045~Q-050·Q-061~Q-064·Q-068~Q-071과 2026-08-28 Q-089 뒤 E3 개발 전환 요청.
- 승인 근거 참조: 원천 문답과 `docs/Architecture/기존WI세계발현E5개발계획.md`의 2026-08-30 사용자 구현 승인
- 승인 상태: `Approved`

## 다섯 번째 개발 인계 상한 — 현행 r5

- 대상은 `WI-ACTOR-03` 하나, 전달 상한은 Logic E5 / Presentation E5다. 현재 E4 증거를 실행 전에 올리지 않는다.
- 실제 책 접근→Preview→명시적 Confirm→지식/행위 기록→같은 revision 카드→재조회/다음 탐색을 닫는다. 미지 처방·접근 불가·revision 불일치는 무변경, 최신 상태에서 재시도한다.
- Session 실행·Save/Replay·Local/Remote 동일 Core·Unity Builder/Prefab/InteractionAnchor·실제 Scene/대표 Game View·Console·저장 재진입까지 허용한다.
- 아래 r1~r4 상한과 완료 수치는 역사 기록이다. r5의 승인과 실제 E5 통과는 다르며 아직 E5 실행 증거는 없다.

## 과거 첫 개발 인계 결과

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

## 네 번째 개발 인계 상한

- 활성 WI: `WI-ACTOR-03` 지식 습득을 유지한다.
- 목표: 기존 Logic E4와 Presentation E3를 보존하면서 물리 처방 기록의 Presentation E4 준비 계약을 추가해 통합 에비던스 `E4`까지 올린다.
- 허용: 같은 revision의 Recipe 지식 카드 상태를 `ReadableKnowledgeSource`, 의미 기반 `VisualKey`, 승인 후보 fingerprint, 명시적 primitive fallback과 Preview 전용 `InteractionAnchor`로 결정적으로 투영하는 코드와 집중 자동시험.
- 금지: 실제 Prefab 로드·Scene 좌표·Renderer·Collider·입력·카드 Confirm·Play Mode·Game View, Save/Replay, 약초 채집·달이기·섭취·약효, Logic 또는 Presentation E5 이상 승격 주장.
- 완료 표현: `Logic E4 / Presentation E4 / 통합 E4`. 실제 공간 발현을 뜻하지 않으며 E5 준비 상태는 `Conditional`로 유지한다.
