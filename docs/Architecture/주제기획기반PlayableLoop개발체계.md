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
8. 저장·권위·외부 경계
9. 제외 범위와 승인

구현 세부가 아직 정해지지 않았으면 빈칸을 숨기지 않고 `미정`으로 남긴다. `ReadyForReview`와 `Approved`에는 미정인 플레이어 약속이나 폐루프 핵심이 없어야 한다.

## 운영 순서

1. 주제를 고르고 템플릿으로 기획서를 작성한다.
2. 기존 기획·결정 문서를 `sourcePlanningDocumentRefs`로 연결한다.
3. 검토 가능한 상태가 되면 `ReadyForReview`로 올린다.
4. 명시적 승인 근거와 hash를 남겨 `Approved`로 바꾼다.
5. Goal을 활성화하고 E7→E1 영향 검토와 E1→E7 조립을 진행한다.
6. 논리·표현 결과를 검증하며 부족하면 가장 이른 E로 돌아간다.
7. E7 뒤 반복 안정성은 E8, 같은 영역의 조화는 E9, 제한 운영은 E10에서 별도 검증한다.

## 자동 검증

`eng/execution-ledgers/manage-playable-loop-topic-planning.ps1`가 문서 경로, 필수 절, 주제 중복, 판본 hash, 승인 근거, 활성 Goal 관문과 이전 예외를 검사한다. `Write` 모드는 `docs/AI/generated/playable-loop-topic-planning.md`를 생성한다.
