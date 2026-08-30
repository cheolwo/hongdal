# 기존 WI 세계 발현 E5 개발 계획

## 승인과 목표

- 계획 판본: `existing-wi-world-manifestation-e5.plan.r1`
- 상태: `Approved` (실행 범위 승인, 증거 통과 아님)
- 승인 근거: 2026-08-30 기획 스레드 `01a025cf-0772-7251-b842-156a20e7483e`에서 준비된 WI와 Farm 병행, 실제 씬 배치·실행 확인, 핵심 상태 저장 포함을 선택한 뒤 사용자가 `Implement the proposed plan.`으로 승인했다.
- 목적: 새 E3 골격 확대보다 기존 WI를 실제 Session·공간·결과·저장에 연결한다. 최종 E7 목표를 삭제하지 않고 이번 전달 목표와 상한을 E5로 둔다.
- 현재 조사 기준: WI r43의 105개 중 구현 E3 68개/E4 2개. 이것은 논리·표현 통합 E나 실제 플레이 완료 수가 아니다. 최신 사실은 [개발 통합 상태판](../AI/개발통합상태판.md)과 원장·증거를 함께 읽는다.

## 첫 승인 범위

| 우선순위 | PlayableUnit / WI | 개발 완료 조건 | 이번에 열지 않는 것 |
| --- | --- | --- | --- |
| A, 독립 진행 | `playable-loop:nature-basic-herbal-recovery.v1` / `WI-ACTOR-03` | 실제 폐야영지 책 → Query/Preview/Confirm → 지식 카드와 같은 revision 결과 → 저장·복원·재방문 | 채집·달이기·섭취·약효·전체 약초 루프 E5 |
| B, A의 완료를 기다리지 않음 | `playable-loop:nature-camp-visitor-stay.v1` / `WI-COMMUNITY-VISITOR-STAY` | 실제 방문자·대기/휴식/이탈 기준점 → 수락/거절 → 점유·응대 결과 → 저장·복원 | 체류 기간·정식 가입·고용·마음 수치·자동 NPC 생활 |
| 병행 준비 후 자체 통합 | `playable-loop:farm-crop-cycle.v1` / `WI-FARM-01~04` | 기존 감자 Fixture와 밭 하나에서 경작→파종→성장→수확→다음 생산 선택, 강변 H2 배치·저장·재진입 | 자유형 개간·고급 관수·공공 API 실호출·Farm→Hub 선행 의존 |

각 주제의 승인 원문은 [지식 습득 r5](PlayableLoops/Nature기초약초회복.md), [방문자 r3](PlayableLoops/Nature야영지방문자임시체류.md), [Farm r1](PlayableLoops/Farm경작세계발현E5.md)이다. 이 계획은 여러 폐루프를 하나의 Goal로 합치는 문서가 아니다. 기존 주제 1:1 Goal과 WI별 작업·증거를 유지한다.

## 공통 구현 계약

1. 기존 규칙을 재사용한다. 지식 Runtime 포트는 확장하고 방문자에는 같은 방식의 좁은 실행 포트를 연결한다. 하위 원장은 Session·Actor·방문자 고유 식별자에 귀속시킨다.
2. 독립 메모리 원장의 revision을 Session revision으로 이름만 바꾸지 않는다. Session 명령 경계에서 검증, 상태 변경, ActionRecord, CommandLog 및 같은 WorldRevision 결과 투영이 함께 성공해야 한다. Preview는 무변경이고 Confirm 실패·중복은 추가 효과를 만들지 않는다.
3. 두 주제의 지식/방문자 상태, 점유, 중복 실행 방지와 복원에 필요한 생성 계보를 기존 Save/Replay에 추가한다. 개발이 착수 시 최신 판본을 확인하고 다음 판본 하나를 예약한다(조사 시 v29). v1~v29 읽기·hash 의미를 보존한다. 별도 UI 저장 파일이나 최신 Catalog에 의존한 조용한 재생성은 금지한다.
4. LocalProcess/RemoteHost는 같은 Core를 호출한다. 변경한 공통 계약의 HTTP/Unity 소비자를 검증하되 실제 멀티플레이 화면·서버 배포·운영 DB는 열지 않는다.
5. Presentation E4의 자산·H·상호작용점·fallback 기준선을 동결한 후 Logic E5 결과를 실제 Prefab/Renderer/Collider/Bounds와 결속한다. 기존 지도·Sky·실내외 배치·LH 순서 프로필을 유지하고 표현은 권위 상태를 바꾸지 않는다.
6. 공식 Scene은 `SimulationWorldShell` 하나다. 독립 Prefab/Builder를 먼저 만들고 개발이 Scene 쓰기와 Editor 시험 시간을 조율한다. 현재 입력·1/3인칭 전환을 재사용하고 새 카메라 체계를 만들지 않는다.

## Farm·애니메이션 통합 선택

- Farm net10 후보는 공통 Runtime의 직접 ProjectReference로 넣지 않는다. 필요한 순수 알고리즘·변환을 netstandard2.1/C#9 호환 소스로 이식하고 기존 표면 계산 경계를 재사용한다. Cell/Local 좌표/H 소유/CompositionKey/pivot/scale을 명시적으로 매핑한다.
- 후보 JSON hash는 원본 근거로 보존하고 현행 계획의 정규형 hash를 별도로 생성한다. 동결된 배치를 다른 Seed나 Compose로 다시 추첨하지 않는다. 기존 Partition/LH 소비 계약을 사용한다.
- 실제 자산 치수·통로 검증 전 후보는 `UnapprovedCandidate`다. 기술 변환 승인과 실제 패턴·사람 시각 승인은 구분한다.
- 애니메이션 첫 대상은 방문자 r3 연구의 기존 Starter 남성 Actor 하나다. Idle/Walk를 우선하고 Greet는 선택 사항이다. imported Clip/Avatar/Adapter 재생을 검증하며 FBX 존재만으로 완료하지 않는다.
- Editor 읽기 검사와 해당 Actor의 프로젝트 소유 Wrapper/Controller/Adapter 구현을 승인한다. vendor 원본을 일괄 재import·변경하지 않는다. Rig 오류가 재현되면 프로젝트 소유 복제 설정/Wrapper로 좁게 해결하고 실제 뼈 작성자는 하나로 조율한다. 기존 Nature 벌목과 검 공격을 바꾸거나 전체 팩을 리타기팅하지 않는다.

## 순서와 병렬 소유

`기획 승인·연구 → E4 결속 → 논리 E5 실행·저장 → 같은 revision 표현 E5 → 실제 씬·저장 재진입 → 개발 통합 반환`을 각 작업이 독립적으로 따른다.

- 기획: 본 계획·주제 기획·Accepted 연구·장기 결정. 작업 명세 입력과 승인 hash를 개발에 인계한다.
- 개발 `01a02198-8b2a-7491-ac93-366b30ff474c`: 공통 Runtime/Save, 작업 명세·Goal/전달/Loop/WI/문답·증거·생성물, 상태판과 최종 Scene 통합의 단일 writer.
- 공간 `01a04fb7-7c73-75a3-b7c2-a29c64766c26`: 개발이 예약한 Farm 호환 변환·기하 검증·측정·독립 배치 모듈. 최신 저장소 기준선으로 진행하고 원본 후보 hash는 보존한다.
- 애니메이션 `01a04676-8d10-7480-b851-707fbd655d46`: 개발이 예약한 Actor·동작·Wrapper 및 시험. Scene/공통 Adapter 겹침은 먼저 조율한다.
- 전역/담당별 WIP1을 다시 적용하지 않는다. `writePaths`, 공유 계약, baseline hash, 명세 hash, 실제 의존성으로 판정한다. 한 작업의 차단으로 다른 작업을 멈추지 않는다.

전체 105개 WI의 재분류는 기존 실행 우선순위·문답 구현 원장을 갱신하는 방식으로 진행한다. 새 평행 원장을 만들지 않는다. 구현 E3+, 승인 기획, 독립 입출력, 공간 준비 순서로 후속 후보를 정하고 기존 공간 E5/E6을 WI 전체 통합 E로 잘못 재사용하지 않는다. 미정·보류 WI의 신규 권위 구현을 자동 승인하지 않는다. 기존 자원 재생 E3 인계 완료와 실제 Tick/Save 연결, 흔적 조사 승인 대기는 별개로 유지한다.

## 검증과 보고

- 기존 회귀 두 건(모판 고정 revision·하위 모듈 고정 개수)은 개발이 실제 기준과 비교해 수정한다. 숫자만 통과하도록 바꾸지 않는다.
- 성공/거부/접근 불가/용량 부족/중복/ExpectedRevision, 순서 불변 hash, 상태 무변경, Session 생성·저장·복원·재생, Local/Remote 결과 동등성을 시험한다.
- 배치의 경사·겹침·문·통로·지지면·실제 Bounds·지면 관통/부유, LH 해제/재활성 및 중복 객체 방지를 확인한다. 기존 공통 배치 반복 검토 도구도 재사용한다.
- 코드·관련 자동시험과 소비 빌드 후 실제 canonical Scene에서 세 범위를 각각 실행하고 저장·재진입한다. 상태 전후 대표 Game View와 Console 결과를 보존한다. 원장·표현·엔진 인계 검증도 통과해야 한다.
- Logic/Presentation은 개별 판정하고 통합 E는 낮은 값이다. WI-ACTOR-03 일부 증거를 전체 약초 루프 E5로 올리지 않는다. 정적 fallback 때문에 필수 시각 조건이 부족하면 그 Presentation만 차단한다.
- 개발은 실제 통합/미통합 준비물/검증/기획 결정/다음 작업 형식으로 기획에 반환한다. Accepted 통합 기록 전에는 workItem을 Integrated로 바꾸지 않는다. 사용자 요청 없는 commit/push는 하지 않는다.
