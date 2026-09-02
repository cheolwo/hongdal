# Codex PlayableLoop Goal 상태

> 이 문서는 `eng/execution-ledgers/codex-playable-loop-goals.json`에서 자동 생성된다. 직접 수정하지 않는다.

- Goal 원장 개정: `codex-playable-loop-goals.r141`
- Goal WIP: `5/상한 없음`
- WI WIP: `6/상한 없음`
- 담당별 강제 상한 없음. 승인·의존성·쓰기 소유권으로 실행 가능 여부를 판정한다.
- 주제 기획 관문: `Approved` / `topic:nature-night-day2.v1`
- 우선순위: `CoreFirstPlayerContinuity` / `Nature → Farm → Hub → Town → City`

## 대표 작업 /goal 입력 (전체 실행 목록 아님)

```text
목표:
playable-loop:nature-night-day2.v1의 플레이어 약속을
E7 PlayClosed까지 닫는다.

플레이어 약속:
오두막에 자원을 보관하고 수면한 뒤 새벽에 다음 확장 계획을 선택한다.

현재 기준:
- 현재 폐루프 증거 단계: E4
- 현재 WI 증거 단계: E4
- 현재 성숙도 궤적: Logic
- 현재 작업 WI: WI-WORLD-RESOURCE-REGENERATE 세계 자원 재생
- 파이프라인 관문: Logic Passed / Presentation Passed / 통합 Passed
- 파이프라인 재개 E: E5
- 기준 revision: world-interaction-delivery-priorities.r40 / nature-resource-regeneration.design.r1

운영 규칙:
- 승인된 독립 작업은 병렬로 구현한다. 같은 파일·계약 변경은 소유자를 조율한다.
- E7→E1로 폐루프 영향을 검토하고 가장 낮은 미완료 의존성을 고른다.
- 구현 후 E1→E7 방향으로 수직 증거를 검증한다.
- H 전체가 아니라 현재 폐루프에 필요한 공간 능력만 사용한다.
- Scene·Synty 배치·문서·EditMode만으로 E7을 선언하지 않는다.
- Solo LocalProcess와 Hosted RemoteHost는 같은 Simulation Core 계약을 사용한다.
- 플레이어 의도, Simulation 권위, Unity 표현을 분리한다.
- 권위 변경은 행위 원장 기록을 남기고 분야 성장은 적용 또는 사유 있는 NotApplicable로 판정한다.

완료 조건:
- 필수 WI가 모두 필요한 증거 단계를 충족한다.
- 성공·실패·회복·귀환 경로가 닫힌다.
- Save/Restore/Replay 결과가 결정적이다.
- E7 실제 입력·Play Mode·Game View 증거가 유효하다.
- EvidencePackage가 유효하며 미해결 차단 항목이 없다.

중지 조건:
새 권위, 외부 Provider·운영 쓰기, 범위 밖 폐루프 또는 플레이어 약속 변경이 필요하면 사용자 결정을 요청한다.
```

## 현재 상태 보고

| 현재 WI | 현재 E | 현재 증거 | 남은 차단 | 다음 최저 의존성 |
| --- | --- | --- | --- | --- |
| `WI-WORLD-RESOURCE-REGENERATE` 세계 자원 재생 | 폐루프 E4 / WI E4 → E7 | evidence:nature-personal-plan-current-e4-20260901<br>evidence:nature-heat-source-current-e4-20260901<br>evidence:nature-night-day2-six-wi-e4-20260901<br>evidence:nature-personal-plan-logic-e3-20260830<br>evidence:nature-heat-source-logic-e3-20260830<br>evidence:nature-r2-core-20260825<br>evidence:nature-night-day2-wi13-playmode-20260826<br>evidence:nature-night-day2-wi13-hosted-parity-20260826<br>evidence:nature-night-day2-wi14-playmode-20260826<br>evidence:nature-night-day2-wi14-hosted-parity-20260826<br>evidence:nature-night-day2-wi15-playmode-20260826<br>evidence:nature-night-day2-wi15-hosted-parity-20260826<br>evidence:nature-dual-loop-game-view-20260826<br>evidence:nature-night-day2-presentation-e7-20260826 | 여섯 WI의 Logic·Presentation E4 준비는 현재 코드·자동 회귀·정확 후보 fingerprint와 r5 작업 명세로 결속됐지만 E5 실제 제품 연결은 미검증이다.<br>과거 Unity PlayMode·Game View 패키지는 Superseded/Stale이므로 Current E5로 재사용하지 않는다. 이번 범위에서는 Editor·Scene·Play·E5를 열지 않는다. | `WI-WORLD-RESOURCE-REGENERATE E5` |

## 병렬 작업과 통합 인계

| 작업 | Goal / WI | 궤적 / 목표 | 담당 | 상태 | 실제 차단 |
| --- | --- | --- | --- | --- | --- |
| work:farm-crop-cycle:d396-visual-candidate-preparation | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Presentation / E4 | 01a02198-8b2a-7491-ac93-366b30ff474c | Integrated | IntegrationReceiptInvalid |
| work:farm-crop-cycle:display-focus-repair | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Presentation / E4 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:farm-crop-cycle:e5-logic | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Logic / E5 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:farm-crop-cycle:fb01-harvest-delegation | playable-loop:farm-crop-cycle.v1<br>WI-FARM-04 | Logic / E3 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:farm-crop-cycle:landscape-binding-guard | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Presentation / E4 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:farm-crop-cycle:landscape-ls01-study | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Presentation / E4 | 01a04fb7-7c73-75a3-b7c2-a29c64766c26 | Active |  |
| work:farm-crop-cycle:spatial-expansion-r2 | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Presentation / E4 | 01a04fb7-7c73-75a3-b7c2-a29c64766c26 | Active |  |
| work:farm-crop-cycle:stamina-natural-recovery | playable-loop:farm-crop-cycle.v1<br>WI-FARM-01 | Logic / E5 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:nature-basic-herbal-recovery:d416-visual-key-validation | playable-loop:nature-basic-herbal-recovery.v1<br>WI-ACTOR-03 | Presentation / E4 | 01a02198-8b2a-7491-ac93-366b30ff474c | Integrated |  |
| work:nature-basic-herbal-recovery:e5-logic | playable-loop:nature-basic-herbal-recovery.v1<br>WI-ACTOR-03 | Logic / E5 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:nature-basic-herbal-recovery:hb01-contents | playable-loop:nature-basic-herbal-recovery.v1<br>WI-ACTOR-03 | Logic / E3 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:nature-basic-herbal-recovery:isolated-vessel-review | playable-loop:nature-basic-herbal-recovery.v1<br>WI-ACTOR-03 | Presentation / E4 | 01a04fb7-7c73-75a3-b7c2-a29c64766c26 | Active |  |
| work:nature-camp-visitor-stay:d396-state-binding-preparation | playable-loop:nature-camp-visitor-stay.v1<br>WI-COMMUNITY-VISITOR-STAY | Presentation / E4 | 01a02198-8b2a-7491-ac93-366b30ff474c | Integrated |  |
| work:nature-camp-visitor-stay:e5-logic | playable-loop:nature-camp-visitor-stay.v1<br>WI-COMMUNITY-VISITOR-STAY | Logic / E5 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:nature-camp-visitor-stay:local-npc-dialogue | playable-loop:nature-camp-visitor-stay.v1<br>WI-COMMUNITY-VISITOR-STAY | Presentation / E3 | 01a02198-8b2a-7491-ac93-366b30ff474c | ReadyForIntegration |  |
| work:nature-resource-regeneration:logic | playable-loop:nature-night-day2.v1<br>WI-WORLD-RESOURCE-REGENERATE | Logic / E5 | 01a02198-8b2a-7491-ac93-366b30ff474c | ReadyForIntegration |  |
| work:nature-shelter:first-logging-reflection-seed | playable-loop:nature-shelter-foundation.v1<br>WI-NATURE-06 | Logic / E3 | 01a02198-8b2a-7491-ac93-366b30ff474c | Integrated | IntegrationReceiptInvalid |
| work:nature-shelter:locomotion-comparison-preparation | playable-loop:nature-shelter-foundation.v1<br>WI-NATURE-06 | Presentation / E4 | 01a04676-8d10-7480-b851-707fbd655d46 | Active |  |
| work:nature-shelter:locomotion-synchronous-render-preparation | playable-loop:nature-shelter-foundation.v1<br>WI-NATURE-06 | Presentation / E4 | 01a04fb7-7c73-75a3-b7c2-a29c64766c26 | Active |  |
| work:nature-shelter:woodcutting-animation-integration | playable-loop:nature-shelter-foundation.v1<br>WI-NATURE-06 | Presentation / E4 | 01a02198-8b2a-7491-ac93-366b30ff474c | Active |  |
| work:nature-shelter:woodcutting-animation-production | playable-loop:nature-shelter-foundation.v1<br>WI-NATURE-06 | Presentation / E4 | 01a04676-8d10-7480-b851-707fbd655d46 | Active |  |

개발 스레드가 최종 통합한다. 공간 후보·코드 시험을 공식 Scene 승인이나 Evidence 승격으로 해석하지 않는다.

## Goal 대기열

| 순서 | 영역 | 역할 | PlayableLoop | 목표 | 상태 | 다음 WI |
| ---: | --- | --- | --- | --- | --- | --- |
| 1 | Nature | Core | Nature 도끼·벌목·오두막 기초<br>`playable-loop:nature-shelter-foundation.v1` | E7 PlayClosed | Active | `WI-NATURE-06` |
| 2 | Nature | Core | Nature 황혼 위협 대응·귀환<br>`playable-loop:nature-twilight-return.v1` | E7 PlayClosed | Completed | `WI-NATURE-11` |
| 3 | Nature | Core | Nature 전술 자기 캐릭터 선택·이동<br>`playable-loop:nature-tactical-self-navigation.v1` | E7 PlayClosed | Completed | `WI-NATURE-05` |
| 4 | Nature | Core | Nature 보관·수면·Day2 반환<br>`playable-loop:nature-night-day2.v1` | E7 PlayClosed | Active | `WI-WORLD-RESOURCE-REGENERATE` |
| 5 | Nature | Core | Nature 작업대 기반<br>`playable-loop:nature-workbench-foundation.v1` | E7 PlayClosed | Queued | `WI-CON-01` |
| 6 | Nature | Core | Nature 현장 성과·거점 제작·다음 원정 왕복<br>`playable-loop:nature-field-supply-return.v1` | E7 PlayClosed | Queued | `WI-NATURE-16` |
| 7 | Nature | Core | Nature 기초 약초 회복<br>`playable-loop:nature-basic-herbal-recovery.v1` | E7 PlayClosed | Active | `WI-ACTOR-03` |
| 8 | Nature | Core | Nature 야영지 방문자 임시 체류<br>`playable-loop:nature-camp-visitor-stay.v1` | E7 PlayClosed | Active | `WI-COMMUNITY-VISITOR-STAY` |
| 9 | Farm | Core | Farm 경작·성장·수확<br>`playable-loop:farm-crop-cycle.v1` | E7 PlayClosed | Active | `WI-FARM-01` |
| 10 | Farm | Core | Farm 집하·포장·내부 보관 반환<br>`playable-loop:farm-pack-store-return.v1` | E7 PlayClosed | Queued | `WI-FARM-05` |
| 11 | Farm | Core | Farm 병영·방위·분대 운영<br>`playable-loop:farm-barracks-defense.v1` | E7 PlayClosed | Deferred | `WI-FARM-DEFENSE-RETURN` |
| 12 | Hub | Core | Hub 입고·검수·적치<br>`playable-loop:hub-inbound-putaway.v1` | E7 PlayClosed | Queued | `WI-001` |
| 13 | Hub | Core | Hub 출고 준비·작업 반환<br>`playable-loop:hub-outbound-ready-return.v1` | E7 PlayClosed | Queued | `WI-HUB-03` |
| 14 | Town | Core | Town 주문·소비·다음 욕구<br>`playable-loop:town-order-consume-return.v1` | E7 PlayClosed | Queued | `WI-ORDER-01` |
| 15 | City | Core | City 수요·서비스·결과 반환<br>`playable-loop:city-demand-service-return.v1` | E7 PlayClosed | Queued | `WI-CITY-01` |
| 16 | Nature | Extension | Nature 거점 성찰·다음 원정 준비<br>`playable-loop:nature-base-reflection.v1` | E7 PlayClosed | Queued | `WI-REFLECT-01` |
| 17 | Nature | Extension | Nature 건물 발전·배움 확장<br>`playable-loop:nature-building-learning.v1` | E7 PlayClosed | Queued | `WI-CON-01` |
| 18 | Nature | Extension | Nature 지역 위협 후퇴·복원·회복<br>`playable-loop:nature-regional-threat-recovery.v1` | E7 PlayClosed | Queued | `WI-NATURE-02` |
| 19 | Farm | Extension | Farm 플레이어 배치 확장<br>`playable-loop:farm-player-placement.v1` | E7 PlayClosed | Queued | `WI-WORLD-03` |
| 20 | Town | Extension | Town 메이저 아르카나 문맥 확장<br>`playable-loop:town-arcana-context.v1` | E7 PlayClosed | Queued | `WI-CARD-01` |

PlayableUnit Goal은 E7에서 끝난다. 각 PlayableUnit의 E8 반복 안정성, 둘 이상의 안정 Core를 묶는 E9 영역 조화·사람 승인, E10 제한 운영은 별도 캠페인에서 파생한다.
