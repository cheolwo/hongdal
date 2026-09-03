# Presentation E4 후보 풀과 E5 선택 기획

- 기획 ID: `PLAN-PRESENTATION-E4-POOL-001`
- 분야: 표현
- 판본: `presentation-e4-candidate-pool.plan.r1`
- 상태: `ApprovedPlanningBaseline / BroadE4CandidatePoolConfirmed / E5SelectionPolicyPending`
- 승인 근거: 2026-09-03 사용자가 여러 후보를 먼저 Presentation E4까지 넓게 준비하는 방향을 확정했다.

## 지금·여기·나·너·이렇게

| 관점 | 내용 |
| --- | --- |
| 지금 | 여러 기능의 논리 준비 속도에 비해 실제 Unity 배치가 필요한 Presentation E5가 오래 걸리는 단계다. |
| 여기 | Nature·Farm·Town·Hub·City의 독립 PlayableLoop와 각 WI의 표현 준비 경계다. |
| 나 | 플레이어가 다음 행동과 결과를 화면에서 판독해야 하는 주체다. |
| 너 | Actor·사물·시설·환경·UI와 이를 표현할 H 공간, Graph Map, 배치 맵, Synty 후보 또는 fallback이다. |
| 이렇게 | E5 하나에 장기간 머물지 않고 여러 대상의 Presentation E4를 먼저 넓게 준비한 뒤, 실제 결속 조건이 가장 잘 갖춰진 후보를 선택해 E5로 올린다. |

## 확정 원칙

1. Presentation E4 후보 준비는 실제 E5 작업과 분리해 여러 영역에서 병행할 수 있다.
2. 후보 풀은 새 Evidence 단계나 권위 대장이 아니다. 기존 작업 명세와 표현 검증의 `Ready / Conditional / Blocked`를 조회하는 기획·인계 목록이다.
3. E4 후보마다 플레이어 판독 순간, 대상 WI·PlayableLoop, 기획 판본, H 적용 여부, `VisualKey`, 주·대체·fallback 후보, 배치·`InteractionAnchor` 의도, 후보 revision·fingerprint를 남긴다.
4. 공간·배치 대상은 Graph Map 관계와 배치 맵 ID·revision·hash, 상대 구도·통행·가시성 제약을 함께 준비한다. 비공간·비시각 대상만 사유 있는 `NotApplicable`을 사용한다.
5. Synty 후보는 그대로 재사용, 연결·설정 보완, Blender 가공 계획 필요, 신규 제작 필요, 미검사를 구분한다. 조사나 격리 미리보기만으로 채택·Scene 배치·E5를 선언하지 않는다.
6. `Ready`는 E5 착수 후보라는 뜻일 뿐 E5 완료가 아니다. `Conditional`은 미확보 관측을, `Blocked`는 확인된 불일치·결손과 가장 이른 재개 E를 기록한다.
7. E5 선택은 하위 E1~E4를 같은 Goal·Session·객체·Revision으로 소비할 수 있고, 실제 World 결속·입력·결과 재조회·Save/Replay·Game View를 좁게 검증할 수 있는 후보를 우선한다.
8. E5가 막혀도 다른 E4 후보의 준비를 중단하지 않는다. 차단 후보는 원인을 보존하고 해당하는 가장 이른 E로 돌아가며, 준비된 다른 후보가 다음 선택 대상이 된다.

## 후보 풀 조회 항목

| 항목 | E4에서 필요한 내용 |
| --- | --- |
| 플레이 약속 | 무엇을 언제 알아보고 어떤 행동으로 이어지는지 |
| 권위 문맥 | Logic 단계, 상태 사본, Session·객체·Revision과 열린 결손 |
| 표현 문맥 | H·Graph Map·배치 맵 또는 비적용 사유 |
| 자산 후보 | 주·대체·fallback과 GUID·fingerprint·사용 조건 |
| 상호작용 | `InteractionAnchor`, 예상 입력, 결과 판독 순간 |
| 기술 준비 | Presenter·Adapter·Catalog·Animator 등 기존 재사용 경로와 누락 |
| 승격 조건 | 실제 Prefab/fallback, Renderer·Collider·Bounds, 통행·가시성, 입력·재조회, Save/Replay, Game View |
| 실패 처리 | `Conditional` 또는 `Blocked`, 원인, 담당, 가장 이른 재개 E |

## 현재 첫 E5 실행 묶음과의 관계

현재 개발 스레드에 인계된 Nature의 밤→Day2, 기초 약초 회복, 방문자 체류 세 폐루프·다섯 WI는 이 원칙을 적용하는 첫 E5 실행 묶음이다. 이 숫자는 프로젝트 전체의 고정 WIP 상한이 아니다. 개발은 실제 소유권과 Unity 단독 점유를 확인해 순서를 조정하며, 기획은 그 결과를 기다리지 않고 다른 영역의 E4 후보를 계속 준비할 수 있다.

## 미정

- 다음 E4 후보 풀을 영역별로 한 항목씩 수평 확장할지, 현재 첫 플레이 순서에 가까운 Nature·Farm을 먼저 채울지
- 후보 간 우선순위 점수와 E5 한 회차의 실행 개수
- 후보 풀의 별도 생성 상태판·관리 도구 구현 여부

## 경계

이 기획은 새 WI·Goal·자산·공개 API·게임 규칙·Scene을 만들지 않는다. 후보 풀 등록은 코드 구현, Unity 실행, 실제 배치, Game View 검증, E5 승격, commit·push를 뜻하지 않는다. E5 결과는 기존 원장·작업 명세·증거 묶음이 계속 소유한다.
