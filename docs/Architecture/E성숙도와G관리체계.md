# E 성숙도와 G 관리 체계

## 목적

이 문서는 완료 증거인 `E`, 다음 증거를 만들기 위한 관리 체계인 `G`, 공간 포함 깊이인 `H`를 분리한다. 현재 상세 기준은 [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](E1-E7수직폐루프와E8-E10수평증거체계.md)가 소유한다.

```text
E = 실제 증거의 깊이. E8부터는 판정 단위가 바뀐다.
G = 다음 E 증거를 만들기 위해 관리하는 작업 종류
H = 공간이 조립되는 포함 깊이
```

## 수직과 수평

```text
PlayableUnit: E1 → E2 → E3 → E4 → E5 → E6 → E7
  Logic·Presentation 각각 하향 영향 검토 ↔ 상향 조립·검증

PlayableUnitStabilityCampaign: E7 → E8
  한 폐루프의 반복 결정성·Save 재진입·Host 동등성·실제 입력 안정

AreaHarmonySet: E8 Core 둘 이상 → E9
  영역 인계·조건부 NPC 연속성·사람 평가·후보 승인

LimitedOperationWindow: E9 → E10
```

E1~E7은 하나의 플레이어 선택 폐루프를 계약부터 실제 입력까지 세로로 닫는다. E8은 그 한 후보를 반복해도 동일하고 복원 가능한지 검증한다. E9는 같은 영역의 안정 Core 둘 이상이 논리·표현에서 조화로운지 확인하고 사람이 후보를 승인한다. E10은 승인된 불변 후보를 제한 운영창에서 관찰한다.

E 승격은 [`evidence-stages.json`](../../eng/execution-ledgers/evidence-stages.json)의 완료 관문과 실제 증거를 통과해야 한다. G의 기계 기준은 [`evidence-management-systems.json`](../../eng/execution-ledgers/evidence-management-systems.json)이며, G 작업 완료는 E 승격과 같지 않다.

## 플레이어 관점

| 범위 | 핵심 질문 |
| --- | --- |
| E1~E6 | 이 플레이어 선택을 성립시키는 계약·실행·결정성·WI·H·정제가 준비됐는가? |
| E7 | 사람이 저장 Scene에서 실제 입력으로 성공·실패·회복·귀환을 끝낼 수 있는가? |
| E8 | 같은 E7 폐루프가 규정 횟수의 반복, 저장 재진입, Local/Remote에서 안정적인가? |
| E9 | 같은 영역의 안정 Core 둘 이상이 자연스럽게 이어지고 사람 평가에서 승인 기준을 넘는가? |
| E10 | 승인된 불변 후보가 제한 운영 관찰창에서 안전하게 지속되는가? |

## G1~G5

| G | 주 적용 구간 | 핵심 책임 |
| --- | --- | --- |
| G1 세계 성립 관리 | E1→E6 | WI·Simulation·H·결정성·정제·필요 근거 |
| G2 플레이어 경험 관리 | E6→E7 | 입력·카메라·피드백·실제 Play Mode·Game View |
| G3 개별 폐루프 안정 관리 | E7→E8 | 결정적 3회 논리 실행, Save/Restore/Replay, Local/Remote, 실제 입력 2회와 Scene 재진입 |
| G4 영역 조화·사람 승인 관리 | E8→E9 | 안정 Core 인계, 자원·시간·공간·회복·조건부 NPC 연속성, 사람 평가와 후보 승인 |
| G5 제한 운영 관리 | E9→E10 | 불변 후보, 관찰창, 저장·Replay·rollback, 계속 운영 승인 |

뒤 단계에서 발견한 결함은 같은 판정 단위를 유지한 채 가장 이른 E를 다시 연다. 수정된 revision에는 기존 안정성·조화·사람 승인을 자동 재사용하지 않는다. 변경 영향·Migration·호환·회귀는 특정 E 번호가 아니라 전 단계의 교차 책임이다.

## NPC 생활 연속성

NPC의 정체성·판단·이동·예약·WI·결과·다음 판단은 독립 `PlayableUnit`이면 E1~E7로 먼저 닫는다. 여러 Core의 영역 조화에 NPC가 관여하면 E9의 필수 조건 모듈로 다시 검증한다. NPC가 관련되지 않은 E9 묶음만 근거가 있는 `NotApplicable`을 허용한다. 과거 E8 NPC 생활세계 정의는 호환 문서이며 현재 E8 승격 의미가 아니다.

## 축과 호환 원칙

- `E0~E10`은 증거 코드에만 사용한다.
- `G1~G5`는 관리 체계에만 사용한다.
- `H1~H5`는 공간 포함 깊이에만 사용한다.
- 모든 `PlayableUnit` Goal의 수직 목표는 E7이다.
- E7을 통과한 각 단위는 자기 E8 안정성 캠페인을 하나씩 가진다.
- E9 후보는 같은 영역의 E8 Core 둘 이상을 요구한다. Town·City처럼 Core가 하나뿐이면 기존 Core를 억지로 쪼개지 않고 후보를 보류한다.
- 과거 `legacy-change-adaptive.r10`의 E8 NPC·E9 변화 적응 문자열은 판본과 함께 읽고 현재 증거에 합산하지 않는다.

## 현재 판정

이 체계를 도입한 사실은 E8·E9·E10 증거가 아니다. 현재 Goal·활성 WI·두 궤적 단계는 [`CURRENT_WORK.md`](../AI/CURRENT_WORK.md)와 기계 원장을 확인한다. 현재 16개 `PlayableUnit`은 모두 Goal 대기열과 E8 안정성 캠페인에 정확히 한 번 들어가며, E9 후보는 Nature 2개·Farm 1개·Hub 1개다. Town·City는 두 번째 안정 Core가 생길 때까지 보류한다.
