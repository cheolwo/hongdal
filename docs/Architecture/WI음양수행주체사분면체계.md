# WI 음양·수행주체 사분면 체계

## 목적

WI 단일 책임을 유지하면서 실행의 행동 목적과 실제 수행 주체를 서로 다른 축으로 읽는다. 이 분류는 게임플레이 설명·조회·증거를 위한 좌표이며 선악, 성별, 성공·실패, 버프·디버프 또는 보상 배율이 아니다.

| 기호 | 행동 목적 | 실제 수행 주체 | 한국어 이름 |
| --- | --- | --- | --- |
| `++` | 양(陽) | Player | 양·플레이어 |
| `+-` | 양(陽) | NPC | 양·NPC |
| `-+` | 음(陰) | Player | 음·플레이어 |
| `--` | 음(陰) | NPC | 음·NPC |

첫 번째 부호는 `ActorActionPurpose`, 두 번째 부호는 실행 인스턴스의 실제 `ActorBinding`이다. `TriggerSource`는 실행이 무엇 때문에 시작됐는지를 설명할 뿐 두 번째 부호를 결정하지 않는다. Player가 정책을 선택한 뒤 NPC가 실제 작업하면 발생원은 `PlayerDriven`일 수 있어도 수행 주체 부호는 NPC다.

## 음양 판정

- `Yang`: 탐사·전투·채집·이동·운반·물리적 작업처럼 행위자가 세계에 직접 개입한다.
- `Yin`: 관찰 결과 검토·정책·배정·보관·회복·취소·계획처럼 상태를 수렴하고 다음 행동을 결정한다.
- `Contextual`: 제조·건설·복원처럼 발산과 수렴을 잇는 행동이다. 승인된 PlayableLoop 구간의 목적을 읽어 이번 실행의 Yang 또는 Yin을 정한다.
- `NotApplicable`: 실제 Player/NPC Actor가 없는 순수 DataDriven·WorldDerived 자동 전이다.
- `Unclassified`: Contextual WI에 승인 문맥이 없거나 Actor 결속을 신뢰할 수 없는 상태다. 이번 기준선에서는 실행을 막지 않고 분류만 비워 둔다.

한 번 판정한 값은 Preview→Confirm→Task→Effect가 끝날 때까지 고정한다. Tick·진행률·Effect가 바뀌어도 같은 실행의 사분면을 다시 계산하지 않는다. 같은 WI도 다른 실행에서 Player 또는 NPC가 수행하면 다른 사분면이 될 수 있다.

## 단일 책임 및 권위 경계

사분면은 WI를 네 벌로 복제하지 않는다. 예를 들어 `WI-FARM-04`의 의도와 주요 결과는 수확으로 동일하며 Player 실행은 `++`, NPC 실행은 `+-`다. Preview·Confirm·Task·Effect는 한 WI의 생명주기이고 순서·분기·반복은 [별도 WI 조립 흐름](../../eng/execution-ledgers/world-interaction-flows.json)이 소유한다.

Simulation Core만 분류를 확정한다. Unity와 클라이언트는 사분면·실제 수행 주체·판정 규칙을 Command에 넣지 않고 읽기 전용 상태 사본만 소비한다. 분류는 생산량·전투 피해·보상·재고·타로 방향·카드 배율·심리 기간을 변경하지 않는다.

## 대장과 초기 분류

기계 판독 기준은 [`world-interaction-polarity-quadrants.json`](../../eng/execution-ledgers/world-interaction-polarity-quadrants.json)이다. 60개 WI를 고정 Yang 25개, 고정 Yin 21개, 실행 문맥 판정 6개, 사분면 제외 8개로 전수 분류한다.

Actor 책임 전환 대상 `WI-HUB-04~05`, `WI-ORDER-03~04`는 의미상 Yang으로 등록한다. 그러나 현재 WorldDerived 전이는 실제 NPC 행동 migration이 완료되기 전까지 `NotApplicable`로 남긴다.

Contextual 기준선은 다음과 같다.

- Nature 오두막 터 선정·건설은 `nature-shelter-foundation`에서 Yin이다.
- Nature 경로 복원은 `nature-twilight-return`에서 Yang이다.
- Nature 건물 발전과 현장 보급 제작은 각각 승인된 영역 운영·보급 반환 폐루프에서 Yin이다.
- 등록되지 않은 Contextual 조합은 이름이나 장소만으로 추정하지 않는다.

## 저장·재생과 표시

새 실행은 판정된 음양, 실제 수행 주체, 사분면 코드·기호와 규칙 revision을 `WorldInteractionInvocation.v2` 상태 사본으로 보존한다. 사분면 상태가 있는 새 세션만 `simulation-save.v23`을 사용하며 v22 이하 hash는 새 필드를 읽지 않는다.

안정 코드는 `YangPlayer`, `YangNpc`, `YinPlayer`, `YinNpc`, `NotApplicable`, `Unclassified`를 사용한다. `++`, `+-`, `-+`, `--`는 사람이 읽는 기호다. Unity는 한국어 이름·기호·판정 근거를 표시할 수 있지만 GameObject나 애니메이션으로 분류를 확정하지 않는다.

## 검증 기준

- 동일한 수확 WI가 Player 실행에서 `++`, NPC 실행에서 `+-`가 된다.
- Player가 시작해 NPC가 수행하는 위임은 TriggerSource가 아니라 Actor 기준으로 두 번째 부호를 정한다.
- Player의 확장 계획 선택은 `-+`, NPC의 주문 관리는 `--`다.
- 순수 자동 WI는 사분면 밖에 있고 NPC로 위장되지 않는다.
- Contextual 판정은 승인 PlayableLoop 근거가 없으면 `Unclassified`다.
- 저장·복원·Replay와 LocalProcess·RemoteHost가 같은 분류와 hash를 반환한다.
- 분류 전후의 전투·생산·재고·타로 계산 결과는 동일하다.

## 관련 기준

- [WI 단일 책임 원칙](WI단일책임원칙.md)
- [플레이어 중심 게임 개발 업무 구조](플레이어중심게임개발업무구조.md)
- [NPC 루틴 WI 통제 정책](NPC루틴WI통제정책.md)
- [세계 상호작용 단위 중심 공간·Simulation 통합](세계상호작용단위중심공간Simulation통합.md)
