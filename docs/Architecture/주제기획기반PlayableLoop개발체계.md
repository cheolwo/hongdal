# 주제 기획 기반 PlayableLoop 개발 체계

## 목적

게임 기능은 구현 가능한 코드 조각이 아니라 플레이어가 반복해서 경험할 하나의 주제에서 시작한다. 주제 기획서는 재미와 의도를 소유하고, `PlayableLoop` 원장은 현재 성숙도와 검증 상태를 소유한다. 두 문서는 서로를 대신하지 않는다.

## 기본 관계

`주제 1개 → PlayableLoop 1개 → Codex Goal 1개 → 활성 WI 1개`를 기본 단위로 사용한다.

- 주제 기획서는 플레이어 상황, 욕구, 선택, 대가, 성공·실패, 회복·귀환, 다음 선택을 설명한다.
- PlayableLoop는 논리·표현 E와 증거, 차단, 재개 단계를 기록한다.
- Goal은 한 시점에 하나의 PlayableUnit만 소유한다.
- WI는 Goal 안에서 한 번에 하나만 활성화한다.
- AreaAggregate와 WorldAggregate는 주제 기획 관문의 대상이 아니며 자식 결과에서 파생한다.

## 기획 스레드와 개발 스레드

기획과 구현은 같은 저장소 권위 자료를 사용하지만 책임을 분리한다.

### 기획 스레드

- 구현 대기 PlayableLoop를 조사하고 플레이어 약속·재미·선택·대가·성공·실패·회복·귀환을 구체화한다.
- 짧은 정차·대기 시간의 반복 대화는 [PlayableLoop 문답 정밀화 체계](PlayableLoop문답정밀화체계.md)에 따라 한 번에 질문 하나씩 진행한다. 답변을 확정 후보·미정·영향·다음 질문으로 정리하고 판본화된 문답 기록을 기획서의 원천 근거로 연결한다.
- WI 후보, Logic·Presentation 요구, H 공간 능력, 저장·권위·외부 경계와 제외 범위를 정리한다.
- 건물·공간·배치·애니메이션처럼 구체 설계가 플레이 경험을 좌우하면 [전문 심화 연구 분기·재결속 체계](PlayableLoop전문심화연구분기재결속체계.md)에 따라 필요한 연구를 분기하고 검토된 기준선을 기획서에 다시 결속한다.
- 기획서를 `Draft → ReadyForReview → Approved`로 올리고 승인 revision·hash·근거를 `planningGate`에 남긴다.
- 현재 E나 시험 통과를 추정하지 않고 구현 원장과 실제 증거를 읽어 기획의 전제만 갱신한다.
- 별도 요청이 없으면 제품 코드를 구현하거나 활성 Goal을 교체하지 않는다.

### 개발 스레드

- `Approved` 기획서와 현재 활성 Goal의 E7 작업 명세를 구현 입력으로 사용한다.
- 현재 WI에 `Required`로 연결된 전문 연구가 모두 `Accepted`인지 확인하고, 그 기준선과 기획서 재결속 결과를 함께 구현한다.
- Goal WIP 1, WI WIP 1을 유지하고 현재 WI의 가장 이른 미완료 의존성부터 처리한다.
- 기획서에 없는 플레이어 약속·핵심 선택·비용·실패·회복 규칙을 임의로 추가하지 않는다.
- 구현에서 기획 충돌이나 미정 사항을 발견하면 `openFeedbackItems`와 재개 E를 남기고 기획 스레드로 돌려보낸다.
- 코드·시험·Play Mode·Game View 결과는 기획서가 아니라 원장과 `EvidencePackage`에 기록한다.

두 스레드는 대화 내용을 직접 인계 자료로 사용하지 않는다. 기획 대화는 문답 정밀화 기록과 기획서로 합성한 뒤에만 인계한다. 저장소의 승인 기획서, 연결된 문답·전문 연구, Goal 원장, 작업 명세와 생성 상태판이 유일한 공용 인계면이다.

### 장기 기획 Goal과 실제 개발 스레드 인계

기획 스레드는 하나의 장기 `PlanningThreadGoal`을 유지할 수 있다. 이 Goal의 목적은 Unity 게임 완성 구성요소 대장을 균형 있게 채우고, 구현 가능한 묶음을 개발 스레드에 보내며, 구현 피드백을 다시 기획에 결속하는 것이다.

`PlanningThreadGoal`은 저장소의 `Codex PlayableLoop Goal`과 다르다.

- `PlanningThreadGoal`: 여러 문답 회차와 구현 인계를 계속 관리하는 대화 운영 목표
- `Codex PlayableLoop Goal`: 개발 스레드가 한 번에 소유하는 PlayableUnit 하나, WI WIP 1

기획 스레드는 질문을 계속할 수 있지만 이미 개발 스레드에 보낸 기획 revision의 플레이어 약속·WI 책임·권위·H·저장 의미를 조용히 바꾸지 않는다. 새 답변이 전달 범위를 바꾸면 다음 revision으로 기록하고, 진행 중 구현에는 `FeedbackRequired` 또는 재승인 인계로 알려야 한다.

실제 스레드 메시지는 다음 조건을 모두 만족할 때만 보낸다.

1. 개발할 범위가 Unity 게임 완성 구성요소 대장에서 `확정후보` 이상이며 핵심 미정이 없다.
2. 문답이 기획서에 합성되고 `designRevision`·hash·승인 근거가 `Approved`다.
3. 모든 `Required` 전문 연구가 `Accepted`이고 승인 기획서에 재결속됐다.
4. 첫 활성 WI와 E7 작업 명세, 엔진·표현 파이프라인, Save/Replay·Local/Remote·Game View 완료 조건이 준비됐다.
5. 현재 개발 스레드의 Goal WIP·WI WIP와 충돌하지 않는다. 충돌하면 명령하지 않고 대기열에 둔다.

조건이 부족하면 기획 스레드가 구현 가능하다고 추정해 대화 요약만 보내지 않는다. 문답은 계속 진행하되 인계 상태를 `NotReady`로 유지한다.

## 기획 승인 관문

PlayableUnit의 `planningGate`는 다음 값을 가진다.

| 항목 | 의미 |
| --- | --- |
| `topicStableId` | 한 주제를 식별하는 고유 식별자 |
| `designDocumentRef` | `docs/Architecture/PlayableLoops/` 아래 기획서 |
| `designRevision` | 승인 또는 검토 대상 기획 판본 |
| `designHashSha256` | 해당 판본 파일의 SHA-256 |
| `statusCode` | `NotStarted`, `Draft`, `ReadyForReview`, `Approved`, `LegacyActiveMigration` |
| `approvalEvidenceRef` | 승인 근거 참조 |

`Approved`만 새 Goal을 활성화할 수 있다. 승인 뒤 문서 내용이 달라져 hash가 맞지 않으면 승인은 자동으로 무효다. 문서가 없거나 필수 절이 없거나 참조가 끊긴 상태도 활성화할 수 없다.

현재 이미 활성화된 Goal 하나에는 `LegacyActiveMigration`을 한시적으로 허용한다. 이 상태는 다른 루프로 이전할 수 없고 현재 Goal을 완료하기 전에 반드시 정식 기획서와 승인 근거를 갖춘 `Approved`로 바꾼다. 다음 대기 Goal부터 예외는 없다.

## 기획 변경과 E 재개

- 플레이어 약속이 달라지면 기존 주제를 조용히 수정하지 않고 새 `topicStableId`, PlayableLoop, Goal을 만든다.
- 같은 약속을 정밀하게 보완하면 기획 revision을 올리고 다시 승인한다.
- 보완으로 기존 계약이나 표현 가정이 틀렸음이 드러나면 같은 Goal에서 가장 이른 책임 E를 다시 연다.
- 기획서는 현재 E, 시험 통과 수, 활성 WI 상태를 완료 사실처럼 소유하지 않는다. 이 값은 원장과 생성 상태판에서만 읽는다.

## 주제 기획서 필수 절

모든 승인 후보는 템플릿의 다음 절을 유지한다.

1. 식별과 근거
2. 플레이어 약속과 재미
3. 반복 폐루프
4. 선택·대가·성공·실패·회복
5. WI 단일 책임 후보
6. 논리·표현 요구
7. H 공간과 자산 요구
8. 전문 심화 연구 판정과 재결속
9. 저장·권위·외부 경계
10. 제외 범위와 승인

구현 세부가 아직 정해지지 않았으면 빈칸을 숨기지 않고 `미정`으로 남긴다. `ReadyForReview`와 `Approved`에는 미정인 플레이어 약속이나 폐루프 핵심이 없어야 한다. 전문 심화 연구 절에서는 건물·공간·배치·애니메이션 각각을 `Required` 또는 사유 있는 `NotRequired`로 판정하고, 모든 `Required` 문서는 `Accepted` 상태로 재결속해야 한다. 이 절은 다음 신규 기획서와 의미가 바뀌어 revision을 올리는 기존 기획서부터 필수로 적용하며, 현재 활성 Goal의 이미 승인된 범위를 소급해 무효화하지 않는다.

## 운영 순서

1. 주제를 고르고 문답 정밀화 기록을 개설한다.
2. 안전하게 정차·대기할 때 질문 하나씩 답하고 해석을 확인한다.
3. 확인된 결정과 미정을 템플릿 기획서로 합성한다.
4. 문답 기록과 기존 기획·결정 문서를 `sourcePlanningDocumentRefs`로 연결한다.
5. 검토 가능한 상태가 되면 `ReadyForReview`로 올린다.
6. 필요한 전문 심화 연구를 `Accepted`로 만들고 선택한 기준선을 기획서에 재결속한다.
7. 명시적 승인 근거와 hash를 남겨 `Approved`로 바꾼다.
8. Goal을 활성화하고 E7→E1 영향 검토와 E1→E7 조립을 진행한다.
9. 논리·표현 결과를 검증하며 부족하면 가장 이른 E, 문답 가정 또는 잘못된 연구 기준선을 다시 연다.
10. E7 뒤 반복 안정성은 E8, 같은 영역의 조화는 E9, 제한 운영은 E10에서 별도 검증한다.

## 개발 인계 묶음

기획 스레드는 다음 항목이 모두 준비된 경우에만 개발 가능 상태로 인계한다.

1. `playable-loop:*`와 1:1인 `topicStableId`
2. 필수 절을 채운 `designDocumentRef`
3. 승인된 `designRevision`, `designHashSha256`, `approvalEvidenceRef`
4. 단일 플레이어 약속과 반복 폐루프
5. 활성화할 첫 WI와 WI별 단일 책임·허용 발생원
6. 성공·실패·취소·회복·귀환 상태
7. Logic·Presentation E7→E1 영향 검토 범위
8. 필요한 H Capability와 엔진·파이프라인 인계
9. 분야별 `Required | NotRequired` 판정과 모든 `Accepted` 전문 연구 참조
10. 연구에서 확정한 치수·시간·거리·밀도·접촉점·자산 fallback과 재검증 조건
11. Save/Replay, LocalProcess/RemoteHost, 외부 자료 경계
12. 실제 입력·Play Mode·Game View를 포함한 E7 완료 조건

실제 스레드 인계 메시지는 위 묶음을 다시 서술하는 긴 자유문이 아니라 다음 참조를 가진 짧은 실행 지시로 작성한다.

```text
dispatchStableId
PlayableLoop / topic / Goal / 활성 WI
designDocumentRef + revision + SHA-256
Accepted 전문 연구 참조
E7 workOrderRef
이번 구현 범위와 제외 범위
Unity 완성 구성요소 대상 행
필수 검증과 완료 증거
기획 충돌 발견 시 되돌릴 feedback 위치와 가장 이른 재개 E
```

### 인계·피드백 순환

```text
기획 문답
  -> 기획 합성·연구 승인
  -> dispatch 준비 검사
  -> 개발 스레드에 실행 지시
  -> 코드·시험·Unity Runtime 작업
  -> 완료 증거 또는 FeedbackRequired
  -> 기획 스레드가 결과 읽기
  -> 다음 질문·revision·재인계
```

- 개발 스레드는 인계를 받으면 현재 Goal 상태와 승인 hash를 다시 검증하고 일치할 때만 구현한다.
- 기획 스레드는 구현이 진행되는 동안 비중첩 구성요소의 문답을 계속할 수 있다.
- 같은 WI·같은 기획 절을 바꾸는 문답은 개발 결과가 돌아오거나 구현이 명시적으로 중단될 때까지 다음 revision 후보로만 보존한다.
- 개발 완료 보고는 코드 변경, 자동시험, 저장 Scene, Play Mode, Game View, Console, Build 증거를 구분한다.
- 구현 중 발견한 설계 충돌은 개발 스레드가 임의로 해결하지 않고 `openFeedbackItems`, 관련 구성요소, 가장 이른 재개 E와 함께 돌려보낸다.
- 기획 스레드는 개발 완료를 대화만으로 승인하지 않고 원장·상태판·EvidencePackage와 실제 변경을 읽은 뒤 다음 인계 여부를 결정한다.

개발 스레드는 시작할 때 다음 순서를 지킨다.

1. `docs/AI/generated/codex-playable-loop-goals.md`에서 활성 Goal과 WI를 읽는다.
2. `playable-loops.json`의 `planningGate`가 `Approved`인지 확인한다.
3. `manage-playable-loop-topic-planning.ps1 -Mode Validate`로 문서 hash와 필수 절을 검사한다.
4. 승인 기획서와 `sourcePlanningDocumentRefs` 중 현재 WI에 필요한 근거를 읽는다.
5. 현재 WI에 `Required`로 연결된 전문 연구가 모두 `Accepted`인지 확인하고 기준선·revision·무효화 조건을 읽는다.
6. Goal의 `workOrderRef`와 엔진·표현 파이프라인 프로필을 읽는다.
7. 기획 범위·전문 연구와 현재 코드가 다르면 코드를 먼저 늘리지 않고 차이를 원장에 기록한다.
8. 현재 WI 하나만 구현하고 Logic·Presentation 증거를 분리해 갱신한다.

승인 기획서는 구현 방법을 모든 줄까지 고정하지 않는다. 내부 클래스 분리, 시험 보조 코드, 성능 최적화처럼 플레이어 약속과 권위 경계를 바꾸지 않는 기술 선택은 개발 스레드가 결정할 수 있다. 반대로 플레이어 선택·대가·보상·실패·회복·귀환, WI 책임, H 필수 능력, 저장 의미, Local/Remote 권위 또는 `Accepted` 연구의 측정 기준이 달라지면 연구와 기획 revision 재승인이 필요하다.

## 자동 검증

`eng/execution-ledgers/manage-playable-loop-topic-planning.ps1`가 문서 경로, 필수 절, 주제 중복, 판본 hash, 승인 근거, 활성 Goal 관문과 이전 예외를 검사한다. `Write` 모드는 `docs/AI/generated/playable-loop-topic-planning.md`를 생성한다.
