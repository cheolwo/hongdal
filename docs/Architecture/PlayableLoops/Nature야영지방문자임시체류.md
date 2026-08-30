# Nature 야영지 방문자 임시 체류

## 식별과 근거

- 주제 고유 식별자: `topic:nature-camp-visitor-stay.v1`
- PlayableLoop 고유 식별자: `playable-loop:nature-camp-visitor-stay.v1`
- 기획 revision: `nature-camp-visitor-stay.design.r3`
- 원천 문답: [공동체 편입·손님·원격 응대 문답](PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md)
- 반영 문답: Q-199~Q-219 가운데 Q-202의 임시 손님 수용 원칙, Q-205의 초기 플레이어 결정 원칙, Q-207~Q-209의 공간 성장·완충 위치·짧은 마중 원칙
- 승인 상태: `Approved`
- 승인 근거: 원래 E3/E4 개발 인계에 이어 2026-08-30 [기존 WI 세계 발현 E5 개발 계획](../기존WI세계발현E5개발계획.md)의 사용자 구현 승인으로 Session·저장·실제 배치·Game View까지 E5 상한으로 확장한다. 현재 증거 E4는 실행 검증 전 유지한다.

## 플레이어 약속과 재미

플레이어는 야영지 입구에서 대기하는 방문자를 보고, 현재 남은 손님 수용 여력을 확인한 뒤 임시 체류를 수용하거나 거절한다. 수용은 환대를, 거절은 공동체 경계를 지키는 선택을 남기며 어느 쪽도 정식 편입을 자동 확정하지 않는다.

## 반복 폐루프

`방문자 대기 → 수용 여력 확인 → 수용/거절 Preview → Confirm → 방문자 상태와 공동체 마음 계보 기록 → 후속 체류 검토 대기`

첫 구현은 `WI-COMMUNITY-VISITOR-STAY` 하나만 소유한다.

## 선택·대가·성공·실패·회복

- 선택: `AcceptTemporaryStay` 또는 `Reject` 중 하나를 명시적으로 Confirm한다.
- 대가: 수용하면 손님 수용 칸 하나를 사용한다. 거절하면 수용 칸은 변하지 않는다.
- 성공: 대기 방문자의 상태가 임시 체류 또는 거절로 한 번만 바뀌고 같은 revision의 행위 기록과 공동체 마음 계보가 남는다.
- 실패: revision 불일치, 알 수 없는 방문자, 이미 결정된 방문자, 알 수 없는 선택, 수용 여력 부족은 상태 변경 없이 거부한다.
- 회복: 최신 상태 재조회·유효 방문자 선택·수용 여력 확인 후 재시도하거나 거절할 수 있다. 체류 기간·연장·외부 야영·다른 거점 이동·정식 편입은 후속 WI가 소유한다.

## WI 단일 책임 후보

`WI-COMMUNITY-VISITOR-STAY`는 대기 중 방문자 한 명에 대한 임시 체류 수용 또는 거절 결정을 확정한다.

- Preview: 상태와 수용 여력을 검사하고 예상 방문자 상태·마음 계보를 보여 주지만 상태를 바꾸지 않는다.
- Confirm: 방문자 상태와 사용 중인 수용 칸, 공동체 마음 계보, WorldRevision, 행위 기록을 원자적으로 바꾼다.
- 멱등성: 같은 Command와 같은 방문자·선택은 결과를 재사용한다. 같은 Command의 다른 payload는 거부한다.
- 마음 계보: 수용은 `HospitalityAffirmed`, 거절은 `BoundaryProtected`를 남긴다. 첫 WI에서는 점수나 선악 판정을 부여하지 않는다.

## 논리·표현 요구

### Logic E1~E3

- E1: 방문자·대기/임시 체류/거절 상태, 수용 한도, 선택, 마음 계보와 WorldRevision 계약을 정의한다.
- E2: 순수 Domain Aggregate와 Application 저장소·Service가 Query·Preview·Confirm을 같은 규칙으로 실행한다.
- E3: Preview 무변경, Confirm 원자성, 멱등, revision·방문자·재결정·수용 한도 거부와 결정적 hash를 집중 시험한다.

### Presentation E1~E4

- E1: 플레이어는 방문자 상태, 남은 수용 칸, 선택 결과의 의미를 읽어야 한다.
- E2: 권위 상태 사본에서 읽기 전용 방문자 응대 카드를 만든다.
- E3: 카드 고유 식별자·정렬·SourceWorldRevision·결정 상태를 결정적으로 검증한다.
- E4: 같은 revision의 방문자 카드를 상태별 H 기준점, VisualKey, 보유 Synty Actor·침상·대기 소품과 AnimationRole 후보에 결속한다. 표현 준비 투영은 권위 상태를 변경하지 않는다.

## H 공간과 자산 요구

E3 묶음은 공간과 실제 자산을 사용하지 않는다. E4 후보 기준선은 [방문자 임시 체류 E4 표현 연구](Nature야영지방문자임시체류-E4표현연구.r1.md)에 결속한다. `Spatial.VisitorWaitingAnchor`, `Spatial.GuestRestAnchor`, `Spatial.VisitorDepartureAnchor`는 위치 독립 후보이며 실제 H 승격이나 좌표가 아니다.

r3의 실제 배치는 [E5 적용 연구 r2](Nature야영지방문자임시체류-E5표현연구.r2.md)를 따른다. 원래 E4 연구는 원천 후보 기록으로 보존하고 실제 공간·이동·저장 검증 기준은 r2에 결속한다.

## 전문 심화 연구 판정과 재결속

| 분야 | 필요성 | 연구 문서 | 상태 | 이번 판본의 결론 |
| --- | --- | --- | --- | --- |
| 건물 | `NotRequired` | 기존 `h1-stock:nature-shelter`를 후보 host로 읽으며 새 건물 외피를 만들지 않는다. | `NotRequired` | 실제 손님 숙소 건물 성장은 후속 WI다. |
| 공간 | `Required` | `study:nature-camp-visitor-stay:presentation.r2` | `Accepted` | 실제 기존 야영지의 안전 경계·대기·휴식·이탈 경로를 결속한다. |
| 배치 | `Required` | `study:nature-camp-visitor-stay:presentation.r2` | `Accepted` | 실제 Actor·손님 침상·Bounds·통로를 검증한다. |
| 애니메이션 | `Required` | `study:nature-camp-visitor-stay:presentation.r2` | `Accepted` | Starter 남성 Actor 하나의 Idle/Walk, 선택적 인사, 중단·복귀 및 비권위 이동을 검증한다. |

## 저장·권위·외부 경계

- r3는 기존 Application을 Session에 귀속시키는 Local/Remote 실행 포트와 Save/Replay 결속을 허용한다. 다음 미사용 Save 판본은 개발이 지식 습득과 공통 예약하며 과거 판본 읽기·hash를 보존한다. 별도 자동 시간 진행은 만들지 않는다.
- 방문자 결정·수용 여력·마음 계보·행위 기록·멱등 정보는 저장 후에도 같아야 한다. Restore에서 방문자와 침상이 중복 생성되거나 수용 칸이 다시 차감되지 않도록 한다.
- 운영 서버·외부 Provider를 호출하지 않는다.
- 체류 기간과 정식 구성원 전환을 추론하거나 자동 확정하지 않는다.

## 제외 범위와 승인

- 제외: 체류 기간, 연장·외부 협력, 정식 편입, 공동체 마음 수치, 신규 자율 NPC 생활, 새 건물 성장, E6/E7 승격.
- 현재 전달 목표: Logic E5 / Presentation E5 / 통합 E5. 현재 확인된 E4를 자동 승격하지 않는다. 실제 Session 결과·같은 revision 표현·저장 재진입과 Scene 증거가 필요하다.
- 허용: 기존 안전 야영지 H 기준점·보유 Prefab·Renderer·Collider·Bounds·해당 Actor Rig를 `SimulationWorldShell`에 결속하고 기존 이동 Adapter로 짧은 휴식/이탈 이동을 표현한다. 실제 입력/자동 실행·Game View·Console 확인을 포함한다.
