# H2 사람 검토 대기열

> 이 문서는 `eng/world-seedbeds/synty-bottom-up-inventory/h2-human-review-queue.v1.json`에서 생성된다. 직접 수정하지 않는다.

- 대기 항목: 6개
- 검토 단위: H2 하나
- 필수 화면: H2당 5시점
- 판단: `ApproveCandidate` / `NeedsRevision` / `Hold`

| 순위 | H2 후보 | 기준 플레이 | 검토 질문 | 상태 |
| ---: | --- | --- | --- | --- |
| 1 | `h2-candidate:nature-threat-response` | `reference-play:nature-farm-day.v1` | 위협 관찰 지점에서 흔적을 읽고 비상 출구로 후퇴하는 흐름이 한눈에 구분되는가? | `AwaitingHumanReview` |
| 2 | `h2-candidate:nature-restoration-recovery` | `reference-play:nature-farm-day.v1` | 복원 작업 지점과 안전 회복 생활핵, 다음 탐색 출구가 자연스럽게 이어지는가? | `AwaitingHumanReview` |
| 3 | `h2-candidate:farm-incident-containment` | `reference-play:nature-farm-day.v1` | 노출 점검, 사건 격리, 기상 보호의 순서와 안전 경계가 읽히는가? | `AwaitingHumanReview` |
| 4 | `h2-candidate:farm-loss-restoration-handoff` | `reference-play:nature-farm-day.v1` | 격리 결과가 손실 회복과 Nature 복원 물자 인계로 이어지는 출구를 가지는가? | `AwaitingHumanReview` |
| 5 | `h2-candidate:town-contamination-control` | `reference-play:hub-town-market-day.v1` | 생활 동선과 오염 점검·격리·정화 인계 동선이 충돌하지 않는가? | `AwaitingHumanReview` |
| 6 | `h2-candidate:town-recall-relief` | `reference-play:hub-town-market-day.v1` | 주민 회수 안내와 생활 서비스, Nature 구호 인계가 하나의 생활권 흐름으로 읽히는가? | `AwaitingHumanReview` |

## 판단 경계

- 입구·목표·위험·출구와 게임 플레이 흐름을 H2 조합 전체로 판단한다.
- H1 이미지는 부품 존재 확인용이며 H2 승인 화면을 대신하지 않는다.
- 후보 승인은 AreaSet 배치, WI E단계, 공공데이터 또는 Runtime 완료가 아니다.
