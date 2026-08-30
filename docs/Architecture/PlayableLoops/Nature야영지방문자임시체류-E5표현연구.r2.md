# Nature 야영지 방문자 임시 체류 E5 표현 연구

## 식별·승인·한계

- 연구 ID: `study:nature-camp-visitor-stay:presentation.r2`
- revision: `nature-camp-visitor-stay.presentation-study.r2`
- 대상: `playable-loop:nature-camp-visitor-stay.v1` / `WI-COMMUNITY-VISITOR-STAY`
- 상태: `Accepted` (적용 기준 승인, 실제 Rig·배치·씬 통과 아님)
- 근거: [원천 E4 연구](Nature야영지방문자임시체류-E4표현연구.r1.md), [개발 통합 재조사](../../Reports/전문산출물-개발통합검토-2026-08-30.md), 2026-08-30 [E5 계획](../기존WI세계발현E5개발계획.md) 사용자 구현 승인.

## 질문·대안·선택

대기·수용·거절이 카드뿐 아니라 실제 야영지에서도 구별되고, 저장 후 같은 결정으로 복원되는가를 검증한다. 새 숙소를 건설하는 안은 범위를 넓히므로 제외하고, 기존 야영지에 손님 Slot 하나를 결속하는 안을 선택한다. primitive Actor만 두는 안은 진단 fallback이며 E5 완료안이 아니다.

## 현재 자산과 H 기준선

원천 r1의 후보 목록·의미 키를 재사용한다. 이번 직접 파일 확인에서 Starter `SM_Chr_Male_01` SHA256 `0CCA4FF0779B9D40A106B09F8038360C92BC25C4BEC39A6BE89CDB5C3C465BD1`, Farm `SM_Prop_Bed_01` `53BC5CAB842052C0E0F30935462708AAF35531974E17B0CA625A586AADE82DEC`, Farm `SM_Prop_Bench_01` `0BA1C30D549163F8491AF089B463C8D9882DCC5B7C66C566BDD8C1D114EDBAF3`가 기존 기록과 일치한다. meta·imported Rig·실제 치수·Renderer·Collider는 실행 담당이 재검증한다.

- 대상 Actor/Rig는 **Starter 남성 하나**로 고정한다. 기존 여성은 대체 재고이며 자동 교체하지 않는다.
- `AwaitingDecision`: 실제 안전 야영지 경계 안, 입구와 생활 중심 사이 `Spatial.VisitorWaitingAnchor`에 배치한다. 기존 주 통행선은 비운다.
- `TemporaryStay`: `Spatial.GuestRestAnchor`의 별도 손님 침상 Slot으로 이동한다. Player 수면 Slot·배치·권위 수용 칸을 공유하거나 덮어쓰지 않는다.
- `Rejected`: `Spatial.VisitorDepartureAnchor`로 기존 통행 표면을 따라 짧게 이동한다. 강제 순간이동·영구 배회·추가 호감/처벌 규칙은 없다.
- H1 기존 기능 공간을 사용하고 H2/H3 성장·`h1-stock:nature-shelter` IdeaInventory의 승격을 요구하지 않는다. 실제 안전 host가 없으면 해당 표현 작업만 차단한다.

## 측정과 배치 선택

실측 전 임의 좌표를 기획 수치로 만들지 않는다. 현재 야영지 생성 고유 식별자/안전 경계/기존 Player 통행 Collider를 읽고, 기존 배치 후보를 StableId 순으로 검사한다. Actor·침상 활성 Renderer Bounds를 측정하여 기존 표면 정렬 여유값과 이동 Adapter의 반경·높이를 사용한다. 경계 안에서 문·Player Slot·통행 보호 범위를 침범하지 않는 첫 후보를 선택한다. 후보가 없으면 겹치게 배치하지 않는다.

배치 검증 기록에는 실제 값, 가져온 설정 경로, 표면·Anchor·배치 ID, 후보 fingerprint, 계획 hash를 남긴다. 이는 측정·결정 절차를 승인한 것이며 실측 완료 주장이 아니다.

## 애니메이션·입력·접촉·복귀 수용 기준

- AnimationRole은 기존 `VisitorArrival`을 재사용하고 상태 Cue는 r1의 `Visitor.Waiting.Greet`, `Visitor.State.IdleOrDepart`를 유지한다. 이동/대기는 공용 Idle/Walk 의미 키를 사용한다.
- 1/3인칭 카메라 어디에서도 같은 Actor 상태를 읽으며 카메라 강제 전환·플레이어 입력 잠금은 없다. Greet는 선택적 짧은 표현으로 Confirm/이동을 기다리게 하지 않는다.
- 주 Actor에 실제 imported Avatar/Clip/Controller를 결속해 검증한다. FBX 이름·meta 선언·vendor Controller 존재는 실행 증거가 아니다. 저장된 Walk/Run Rig 오류는 먼저 재현 여부를 확인하고 대상 설정만 좁게 처리한다.
- root motion은 끈다. 기존 이동 Adapter만 위치를 작성하고 Animator/절차형 fallback은 뼈만 표현한다. 동시 뼈 작성자를 두지 않는다. 실제 걸음이 없으면 기존 절차형 Walk를 사용할 수 있지만 바닥 접촉·이동 상태 구별·중단 복귀를 확인해야 한다.
- 발바닥은 이동 지지면을 관통하지 않고 Actor/침상은 Player 경로를 막지 않는다. 첫 범위는 서서 대기·손님 침상 곁 휴식이므로 앉기/눕기·양손 물체 접촉 IK를 추가하지 않는다.
- 수용/거절 확정·Scene 해제·Load 시 이전 Greet/Walk/절차형 동작을 중단하고 새 권위 상태에 맞게 Idle/목표를 재구성한다. 애니메이션 Event가 수용 칸·WorldRevision·보상을 변경하면 안 된다.
- Clip 실패 시 실제 정적 Actor+카드로 응대 기능은 보존한다. 인사 생략은 허용하지만 필수 이동/상태 판독이 성립하지 않으면 해당 Presentation E5는 차단한다. primitive는 실제 Actor를 대체한 완료 증거가 아니다.

## Logic·Save 영향과 검증

권위 방문자 상태·점유·마음 계보·행위 기록·Command 멱등 정보는 기존 규칙대로 Session에 귀속해 저장한다. 이동은 표현이며 거절 확정을 애니메이션 종료까지 미루지 않는다. Reload는 저장된 상태에서 Actor를 한 번만 재구성한다. 새 생활 Tick·체류 종료 규칙은 없다.

시험은 상태별 Anchor/VisualKey/같은 revision, 수락/거절/용량부족/중복/충돌, 경로·표면·Bounds·Actor 중복, 중단/복원, 저장 재진입을 검증한다. 실제 Scene 실행과 대표 Game View/Console를 남긴다. 공간·배치·애니메이션 Required는 이 r2에 결속하고 새 건물 연구는 NotRequired다.

원천 후보·권위 상태·안전 경계·Player Slot·이동/카메라 계약 변경 또는 실제 Rig/배치 실패는 해당 가장 이른 E를 다시 연다. 기술 결함 수정은 개발이 처리하되 새로운 비용·회원 정책·동작 약속 변경은 기획으로 반환한다.
