# Codex PlayableLoop Goal 상태

> 이 문서는 `eng/execution-ledgers/codex-playable-loop-goals.json`에서 자동 생성된다. 직접 수정하지 않는다.

- Goal 원장 개정: `codex-playable-loop-goals.r27`
- Goal WIP: `1/1`
- WI WIP: `1/1`
- 주제 기획 관문: `Approved` / `topic:nature-basic-herbal-recovery.v1`
- 우선순위: `CoreFirstPlayerContinuity` / `Nature → Farm → Hub → Town → City`

## 현재 /goal 입력

```text
목표:
playable-loop:nature-basic-herbal-recovery.v1의 플레이어 약속을
E7 PlayClosed까지 닫는다.

플레이어 약속:
기록에서 기초 처방을 배우고 약초를 모아 따뜻한 차를 달여 마심으로써 체온과 질병 위험을 관리한다.

현재 기준:
- 현재 폐루프 증거 단계: E1
- 현재 WI 증거 단계: E5
- 현재 성숙도 궤적: Logic
- 현재 작업 WI: WI-ACTOR-03 지식 습득
- 파이프라인 관문: Logic Passed / Presentation Blocked / 통합 Blocked
- 파이프라인 재개 E: E1
- 기준 revision: world-interaction-delivery-priorities.r26 / nature-basic-herbal-recovery.design.r2

운영 규칙:
- 동시에 하나의 WI만 구현한다.
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
| `WI-ACTOR-03` 지식 습득 | 폐루프 E1 / WI E5 → E7 | evidence:nature-basic-herbal-recovery-logic-e3-20260828<br>evidence:nature-basic-herbal-recovery-logic-e4-20260828 | Presentation E2 이상, Save/Replay, Unity, 약초 채집·달이기·섭취는 후속 승인 전 구현하지 않는다. | `WI-ACTOR-03 E5` |

## Goal 대기열

| 순서 | 영역 | 역할 | PlayableLoop | 목표 | 상태 | 다음 WI |
| ---: | --- | --- | --- | --- | --- | --- |
| 1 | Nature | Core | Nature 도끼·벌목·오두막 기초<br>`playable-loop:nature-shelter-foundation.v1` | E7 PlayClosed | Completed | `WI-ACTOR-02` |
| 2 | Nature | Core | Nature 황혼 위협 대응·귀환<br>`playable-loop:nature-twilight-return.v1` | E7 PlayClosed | Completed | `WI-NATURE-11` |
| 3 | Nature | Core | Nature 전술 자기 캐릭터 선택·이동<br>`playable-loop:nature-tactical-self-navigation.v1` | E7 PlayClosed | Completed | `WI-NATURE-05` |
| 4 | Nature | Core | Nature 보관·수면·Day2 반환<br>`playable-loop:nature-night-day2.v1` | E7 PlayClosed | Deferred | `WI-NATURE-15` |
| 5 | Nature | Core | Nature 작업대 기반<br>`playable-loop:nature-workbench-foundation.v1` | E7 PlayClosed | Queued | `WI-CON-01` |
| 6 | Nature | Core | Nature 현장 성과·거점 제작·다음 원정 왕복<br>`playable-loop:nature-field-supply-return.v1` | E7 PlayClosed | Queued | `WI-NATURE-16` |
| 7 | Nature | Core | Nature 기초 약초 회복<br>`playable-loop:nature-basic-herbal-recovery.v1` | E7 PlayClosed | Active | `WI-ACTOR-03` |
| 8 | Farm | Core | Farm 경작·성장·수확<br>`playable-loop:farm-crop-cycle.v1` | E7 PlayClosed | Queued | `WI-FARM-01` |
| 9 | Farm | Core | Farm 집하·포장·내부 보관 반환<br>`playable-loop:farm-pack-store-return.v1` | E7 PlayClosed | Queued | `WI-FARM-05` |
| 10 | Hub | Core | Hub 입고·검수·적치<br>`playable-loop:hub-inbound-putaway.v1` | E7 PlayClosed | Queued | `WI-001` |
| 11 | Hub | Core | Hub 출고 준비·작업 반환<br>`playable-loop:hub-outbound-ready-return.v1` | E7 PlayClosed | Queued | `WI-HUB-03` |
| 12 | Town | Core | Town 주문·소비·다음 욕구<br>`playable-loop:town-order-consume-return.v1` | E7 PlayClosed | Queued | `WI-ORDER-01` |
| 13 | City | Core | City 수요·서비스·결과 반환<br>`playable-loop:city-demand-service-return.v1` | E7 PlayClosed | Queued | `WI-CITY-01` |
| 14 | Nature | Extension | Nature 거점 성찰·다음 원정 준비<br>`playable-loop:nature-base-reflection.v1` | E7 PlayClosed | Queued | `WI-REFLECT-01` |
| 15 | Nature | Extension | Nature 건물 발전·배움 확장<br>`playable-loop:nature-building-learning.v1` | E7 PlayClosed | Queued | `WI-CON-01` |
| 16 | Nature | Extension | Nature 지역 위협 후퇴·복원·회복<br>`playable-loop:nature-regional-threat-recovery.v1` | E7 PlayClosed | Queued | `WI-NATURE-02` |
| 17 | Farm | Extension | Farm 플레이어 배치 확장<br>`playable-loop:farm-player-placement.v1` | E7 PlayClosed | Queued | `WI-WORLD-03` |
| 18 | Town | Extension | Town 메이저 아르카나 문맥 확장<br>`playable-loop:town-arcana-context.v1` | E7 PlayClosed | Queued | `WI-CARD-01` |

PlayableUnit Goal은 E7에서 끝난다. 각 PlayableUnit의 E8 반복 안정성, 둘 이상의 안정 Core를 묶는 E9 영역 조화·사람 승인, E10 제한 운영은 별도 캠페인에서 파생한다.
