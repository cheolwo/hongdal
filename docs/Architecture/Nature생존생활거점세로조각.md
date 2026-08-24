# Nature 생존 생활거점 세로 조각

> 상태: `Implemented / runtime evidence pending`
> 규칙 개정: `nature-survival.realtime.r1`
> 기준일: 2026-08-24

## 목적

Nature를 단순한 회복 화면이 아니라 플레이어가 처음 도착해 도구를 얻고, 나무를 베고, 생활 거점을 만들며, 황혼의 위협을 견디는 첫 생활 공간으로 사용한다. 심리 회복 의미는 유지하지만 생존·채집·건설·방어가 현재 플레이의 1차 폐루프다.

```text
안전 빈터 등장
  → 도끼 획득
  → 수확 허용 구역의 나무 벌목
  → 통나무 휴대
  → 오두막 도면 배치
  → 누르기 건설
  → 황혼 소음 기반 위협 후보
  → 전투 또는 후퇴
  → 오두막 회복·보관·방어 / 나무 재생
  → 다음 날 재출발
```

Farm·Hub·Town·City의 독립 업무 폐루프와 선택적 영역 간 연결은 이 흐름의 필수 선행·후행이 아니다.

## 권위와 시간

| 실행 방식 | Simulation 권위 | 시간 규칙 |
| --- | --- | --- |
| Solo | Unity 프로세스 안 `LocalSimulationRuntime` | 메뉴·응용프로그램 비활성 중 정지 |
| Hosted | Simulation 서버 Session | 개별 클라이언트 메뉴와 관계없이 계속 진행 |

- 한 주기는 실제 경과 1,200초다. `낮 0~599 → 황혼 600~749 → 밤 750~1109 → 새벽 1110~1199` 순서를 사용한다.
- 새 프로필을 명시한 Session만 주기 경계에서 기존 `WorldTick`을 1회 진행한다. 프로필이 없는 기존 Session은 바뀌지 않는다.
- 벽시계 timestamp나 종료 시각을 저장하지 않으므로 게임을 끈 동안의 시간을 따라잡지 않는다.
- 외부 Provider와 운영 DB는 세션 생성·실시간 진행·Unity 조회 중 호출하지 않는다.

## 결정적 규칙

| 항목 | 현재 값 |
| --- | ---: |
| 벌목 누르기 시간 | 4초 |
| 나무 한 그루 통나무 | 2개 |
| 오두막 비용 | 통나무 6개 |
| 오두막 누르기 건설 | 30초 |
| 나무 재생 | 3주기 뒤 |
| 오두막 보관 용량 | 20단위 |
| 첫 황혼 조우 기본 확률 | 650/1000 |

황혼 조우는 `ScenarioSeed + SessionStableId + CycleIndex + NoiseEventCount`를 SHA-256으로 정규화한 결정적 roll이다. Skeleton은 `placeholder:synty-generic-skeleton`로 명시하며 최종 몬스터 자산이나 시각 완성 증거가 아니다.

## H 공간 결속

새 H 계층이나 별도 공식 Scene을 만들지 않는다.

| 플레이 역할 | 기존 공간 근거 |
| --- | --- |
| H4 Nature | `area-set:sim:pyeongchang:nature-home.v1` |
| H3 생활·조우·방어 폐루프 | `h3-candidate:nature-home-encounter-defense` |
| H2 안전 생활핵·오두막 | `h2-candidate:nature-home-core` |
| H2 벌목·위협 접근 | `h2-candidate:nature-encounter-route` |
| H1 등장·도끼 획득 | `h1-stock:nature-trailhead` |
| H1 오두막 | `h1-stock:nature-shelter` |
| H1 수확 표본 | `h1-stock:nature-exploration-buffer` 안의 결정적 resource node |
| H1 생활핵 WI 모판 | `wi-spatial-seedbed:nature-survival-home.v1` |
| H1 탐사·조우 WI 모판 | `wi-spatial-seedbed:nature-survival-encounter.v1` |

Unity 자동 조립은 저장 Scene을 덮어쓰지 않고 `SimulationWorldShell` Play 진입 뒤 기존 H3의 `h2-nature-home-core` 기준점에 로컬 모듈을 붙인다. 현재 수확 나무·도끼·Skeleton은 Editor에서 기존 Synty Prefab을 불러오며 오두막은 단계 상태를 보여 주는 blockout이다.

## 서버 계약

- 세션 생성: `경영SimulationSession생성Request.NatureSurvival`
- 상태 사본: `경영SimulationSessionSnapshot.NatureSurvival`
- 조회: `GET /api/simulation/v1/sessions/{sessionStableId}/nature-survival`
- 검토: `POST .../nature-survival/previews`
- 행동 확정: `POST .../nature-survival/commands`
- 실시간 진행: `POST .../nature-survival/clock/advance`
- 저장·재생: `simulation-save.v13`

도끼와 통나무는 별도 중복 수량이 아니라 기존 `SimulationWorldInventorySnapshot.Players[].Items` 원장에 들어간다. 오두막 재료는 건설 시작 때 예약 의미로 소비되고, 누르기 작업 완료 뒤 회복·보관·방어 기능이 열린다.

## 입력과 전투 연결

Unity 입력 우선순위는 `기존 현장 전투 → Farm 배치 → Nature 문맥 작업`이다. Nature에서 왼쪽 버튼은 도끼 획득, 나무 벌목 시작, 진행 중 작업 누르기에 쓰인다. `B`는 오두막 도면 배치, 조우 중 `F`는 기존 현장 전투 요청, `R`은 안전 생활핵 또는 완성 오두막 후퇴다. 전투 승리는 기존 전투 권위가 반환한 종료 Event를 받은 뒤 Nature 조우를 해결한다.

## 현재 증거와 제한

- 서버 Domain·Server 프로젝트 빌드: 오류 0개.
- 서버 집중 자동 시험: 벌목·재고·오두막·일시정지·WorldTick·결정적 조우·Save/Replay·HTTP `8/8`; 기존 저장·재생 포함 회귀 `20/20`.
- Unity 전체 솔루션 빌드: 경고·오류 `0/0`.
- Unity 신규 EditMode 시험은 코드가 컴파일됐지만 같은 프로젝트를 연 Editor 때문에 별도 batch 실행이 차단됐다.
- 저장 `SimulationWorldShell`, 실제 Play Mode 입력, Game View, Console과 독립 프로세스의 실제 서버 HTTP 왕복은 아직 검증하지 않았다. 시험 HTTP host 왕복은 통과했다.
- Unity 로컬 상태를 기존 범용 저장 파일에 넣는 연결, 오두막 최종 Synty 모듈 조립, 전투 후 체력·회복 수치 결속은 후속 범위다.

기존 `WI-NATURE-01~04`는 위협 관찰·후퇴·복원·회복 계약으로 보존한다. `simulation-world-interactions.r8`에서는 플레이어가 선택해 권위 상태를 바꾸는 도끼 획득·벌목 시작·오두막 배치/건설/입장/퇴장·황혼 조우 대응과 진행 작업 취소를 `WI-NATURE-05~12`로 등록한다. 벌목·건설 시간 진행, 황혼 조우 자동 발생과 완료 효과는 독립 WI가 아니라 Task·자동 상태 전이·Effect다. `WI-NATURE-12`는 진행률을 멈추는 입력이 아니라 점유 해제와 예약 자원 반환을 확정하는 권위 전이다.

WI-NATURE-05~11은 Nature Actual E5의 H1·H2·H3·LandscapeGraph에 직접 결속했고, WI-NATURE-12는 원래 작업의 공간·예약 문맥을 이어받는 Contextual 결속으로 닫았다. 따라서 E4는 `ContextBound`, E5는 `ManifestationPartial`이며 공간 대기 항목은 없다. 공통 Local Runtime의 취소·통나무 반환·V15 Save/Replay와 canonical `SimulationWorldShell`의 C 취소·E 퇴장·R 후퇴·저장·Scene 재진입 PlayMode 시험은 통과했다. 다만 Hosted 동등성, 전체 후속·복귀의 `Manifested`, 수동 Game View·Console, E6 정제와 E8 NPC 생활세계는 별도 증거로 남는다.
