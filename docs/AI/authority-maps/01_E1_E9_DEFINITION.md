# E1~E10 증거 성숙도 정의

> 기계 권위: [`eng/execution-ledgers/evidence-stages.json`](../../../eng/execution-ledgers/evidence-stages.json) `simulation-evidence-stages.r13`
> E는 기능 목록이나 개발 순서가 아니라 선정된 판정 단위가 실제로 확보한 증거 깊이다.

## 전체 계단

| 단계 | 이름 | 판정 단위 | 무엇이 존재해야 하는가 |
| --- | --- | --- | --- |
| E0 | 후보 | WI·PlayableUnit 후보 | 실행 후보와 목적이 식별됨 |
| E1 | 계약·결정 완료 | WI·PlayableUnit | 행위·자료·권위 경계와 완료 조건 확정 |
| E2 | 코드 준비 | WI·PlayableUnit | Core·Application·Adapter·Unity 소비 코드 준비 |
| E3 | 자동 시험 통과 | WI·PlayableUnit | 규칙·저장·재생 관련 자동 시험 통과 |
| E4 | WI 실행 문맥 결속 | WI | 발생원·주체·대상·자원·시간과 조건부 H 문맥 결속 |
| E5 | WI 세계 발현 | WI·PlayableUnit | 권위 상태 전이·Task·Effect·결과·후속/복귀 WI 발현 |
| E6 | 플레이 전 정제·필요 근거 결속 | PlayableUnit | 의미·인과·표현·권위·배치·필요 근거 정제 |
| E7 | 핵심 플레이 폐루프 | PlayableUnit | Logic·Presentation 중 낮은 값 기준으로 실제 입력·성공·실패 회복·귀환 확인 |
| E8 | 개별 플레이 폐루프 안정 | PlayableUnitStabilityCampaign | 한 E7 폐루프의 반복 결정성·저장 재진입·권위 위치·실제 입력 안정 확인 |
| E9 | 영역 폐루프 조화·사람 승인 | AreaHarmonySet | 같은 영역의 안정 Core 둘 이상이 조화를 이루고 사람이 후보를 승인 |
| E10 | 제한 운영 검증 | LimitedOperationWindow | 승인된 불변 후보를 판본화된 관찰창에서 안전하게 운영 |

## 단계별 핵심 질문

```text
E1  무엇을 왜 만들며 누가 확정하는가?
 ↓
E2  그 계약을 실행할 코드가 존재하는가?
 ↓
E3  규칙·저장·재생이 자동으로 재현되는가?
 ↓
E4  WI의 발생원과 실행 문맥이 빠짐없이 결속됐는가?
 ↓
E5  권위 세계에서 결과와 다음 선택으로 이어지는가?
 ↓
E6  실제 플레이 전에 의미·배치·피드백·필요 근거를 정제했는가?
 ↓
E7  사람이 실제 입력으로 성공·실패·회복·귀환을 완주하는가?

E7 한 단위 → E8  같은 후보를 반복해도 안정적인가?
E8 Core 둘 이상 → E9  영역에서 조화롭고 사람이 승인하는가?
E9 승인 후보 → E10  제한 운영 관찰창에서도 안전한가?
```

## E와 G의 관계

| 주 전이 | 관리 체계 | 대표 증거 |
| --- | --- | --- |
| E1→E6 | G1 세계 성립 관리 | WI·H·Revision·Hash·Lineage·정제·필요 근거 |
| E6→E7 | G2 플레이어 경험 관리 | 입력·UI·카메라·`SimulationWorldShell`·Play Mode·Game View |
| E7→E8 | G3 개별 폐루프 안정 관리 | 반복 결정성·Save/Replay·Local/Remote·실제 입력 재진입 |
| E8→E9 | G4 영역 조화·사람 승인 관리 | Core 인계·NPC 연속성·공간/입력 조화·사람 평가와 승인 |
| E9→E10 | G5 제한 운영 관리 | 불변 build·관찰 일수·완주·rollback·계속 운영 승인 |

G 작업이 끝났다는 사실은 해당 E를 자동 승격하지 않는다. 변경 영향·Migration·호환·회귀는 특정 E 번호가 아니라 전 구간 교차 책임이다.

## 작업 방향

새 기능은 `PlayableUnit E7 약속 → E1 계약`으로 영향을 검토하고 가장 낮은 미완료 의존성을 구현한 뒤 `E1 → E7`로 검증한다. E7을 통과하면 E8 안정성 캠페인으로 인계하고, 같은 영역의 E8 Core가 둘 이상일 때만 E9 후보를 연다. 모든 PlayableUnit을 E8·E9로 일괄 승격하지 않는다.

과거 `legacy-change-adaptive.r10`의 E8 NPC 생활세계·E9 변화 적응 정의와 `.e9-work-order.json`은 읽기 호환 자료다. 현재 승격에는 [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](../../Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md)를 사용한다.
