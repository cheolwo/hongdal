# Nature 생존 생활거점 세로 조각

> 상태: `r5 Logic E7 / Shelter·Twilight Presentation E7 / Night·Workbench Presentation E6`
> 규칙 개정: `nature-survival.realtime.r5` (`r1~r4` Session·즉시 통나무 지급 호환 유지)
> 기준일: 2026-08-26

## 목적

Nature를 단순한 회복 화면이 아니라 플레이어가 처음 도착해 도구를 얻고, 나무를 베고, 생활 거점을 만들며, 황혼의 위협을 견디는 첫 생활 공간으로 사용한다. 심리 회복 의미는 유지하지만 생존·채집·건설·방어가 현재 플레이의 1차 폐루프다.

```text
안전 빈터 등장
  → 도끼 획득
  → 수확 허용 구역의 나무 벌목
  → 지면 통나무 묶음 생성
  → 통나무 묶음 직접 획득·휴대
  → 오두막 도면 배치
  → 누르기 건설
  → 여분 통나무를 오두막에 직접 보관
  → 황혼 소음 기반 확정 첫 조우
  → 3인칭 관찰 운영 또는 1인칭 직접 개입, 또는 안전 후퇴
  → 오두막 수면과 밤 6배 진행
  → 새벽 확장 계획 선택
  → Day2Ready 저장
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
| 첫 주기 조우 | 소음 1 이상이면 확정 |
| 두 번째 주기 이후 | 결정론적 650/1000 |
| 소음 단계 | 1~2 낮음, 3~4 보통, 5 이상 높음 |
| 오두막 방어 | 유효 위협 1단계 감소 |
| 조우 적 수 | 유효 위협에 따라 1~3명 |
| 수면 중 밤 진행 | 6배, 새벽 자동 해제 |

## r4 플레이어 활동 갈래와 현장 보급 반환

플레이어를 별도 직업이나 고정 모드에 가두지 않고, 현재 행동의 목적만 세 갈래로 분류한다.

| 활동 갈래 | 뜻 | Nature 첫 결속 |
| --- | --- | --- |
| `FieldExpedition` | 영역 바깥 위험에 직접 나가 재료·위협·기회를 다룬다 | 벌목·조우·기본 원정과 보급 원정 |
| `AreaOperation` | 영역 안에서 건물·보관·방어·NPC 흐름을 운영한다 | 작업대·오두막·보관대 운영 |
| `AreaManufacturing` | 확보한 재료를 목적이 있는 물품으로 조립한다 | `WI-NATURE-16` 현장 보급 꾸러미 제작 |

세 갈래는 서로 배타적인 역할이 아니다. 같은 플레이어가 상황에 따라 오가며, 기본 원정은 보급 꾸러미가 없어도 항상 가능하다. 다만 작업대가 운영 중이고 소지 또는 오두막 보관에 통나무 2개와 재건 부품 1개가 있을 때 4초 제작을 선택할 수 있다. 확정 시 재료를 예약하고, 취소하면 같은 원장으로 전량 반환하며, 완료하면 `supply:nature-field-pack` 한 개를 만든다.

다음 벌목 원정에서 `UseFieldSupplyPack`을 고르면 꾸러미를 한 번 소비하고 `ExpeditionPrepared`를 고정한다. 이 준비는 패배 시 별도 Lot 원장이 없는 현재 인벤토리에서 고유 식별자 순으로 고른 소지 `material:*` 한 묶음을 절반 손실에서 보호한 뒤 종료된다. 수익·전투 승리·추가 생산량을 자동 보장하지 않으며, 추후 실제 Lot 보호로 확장하기 전까지의 제한된 첫 계약이다.

플레이어 기회 조회는 지금 선택할 수 있는 행동과 차단 이유를, 영역 수요 조회는 작업대·재료·보급 준비의 부족 상태를 보여 준다. HTTP와 Solo `LocalProcess`가 같은 Simulation Core 조회를 사용하며 조회 자체는 외부 Provider나 운영 DB를 호출하지 않는다.

첫 주기는 소음이 하나 이상이면 조우가 확정된다. 두 번째 주기부터 `ScenarioSeed + SessionStableId + CycleIndex + NoiseEventCount`를 SHA-256으로 정규화한 결정적 65% roll을 사용한다. Skeleton은 `placeholder:synty-generic-skeleton`로 명시하며 최종 몬스터 자산이나 시각 완성 증거가 아니다.

## 첫날 손익과 반환

- `Fight`는 r2에서 조우를 즉시 해결하지 않고 연결 전투 식별자와 `CombatActive` 상태를 연다.
- 전투 중 Nature 시계는 멈추며 기존 `SimulationLocalCombat`의 100ms `BattleTick`만 진행해야 한다.
- 승리는 적 수와 같은 기본 재건 부품 1~3개를 지급한다. `DirectAction`은 Simulation이 확정한 성과 등급에 따라 최대 2개를 추가하며, `ObserverOperation`은 동일 기본 보상만 받는다.
- 후퇴는 자원 손실 없이 완성 오두막 또는 안전 빈터로 돌아간다.
- 패배는 도끼·건물·보관 자원을 유지하고 소지 중인 각 `material:*` 수량의 `floor(50%)`만 잃는다.
- 전투 결과는 동일 조우에 정확히 한 번만 적용한다.
- 밤에 오두막 안에서 수면하면 새벽까지 6배로 진행하고 자동 해제한다.
- 새벽에는 `Workbench`, `StorageRack`, `Palisade` 중 하나를 선택해 `Day2Ready`로 저장한다. 비용은 다음 날 목표 표시용이며 첫날 선택 시 즉시 소비하거나 효과를 적용하지 않는다.

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

Unity 자동 조립은 저장 Scene을 덮어쓰지 않고 `SimulationWorldShell` Play 진입 뒤 기존 H의 `NatureHome` 기준점에 로컬 모듈을 붙인다. 전용 생활핵 H2가 없으면 canonical H 구성 대장의 Nature 기준점을 사용하며 Farm 원점을 대체값으로 사용하지 않는다. 수확 나무·도끼·통나무·Garden Shed 오두막·Skeleton·Table Saw는 판본화된 `Nature생존VisualCatalog`의 Synty Prefab을 사용한다.

## 서버 계약

- 세션 생성: `경영SimulationSession생성Request.NatureSurvival`
- 상태 사본: `경영SimulationSessionSnapshot.NatureSurvival`
- 조회: `GET /api/simulation/v1/sessions/{sessionStableId}/nature-survival`
- 검토: `POST .../nature-survival/previews`
- 행동 확정: `POST .../nature-survival/commands`
- 실시간 진행: `POST .../nature-survival/clock/advance`
- 저장·재생: r5 지면 통나무 상태는 `simulation-save.v24`; 기존 r1~r4 저장 의미와 hash 읽기 호환 유지
- 관찰 개입: `POST .../battles/{battleStableId}/observer-interventions/confirm`

도끼와 획득 완료 통나무는 별도 중복 수량이 아니라 기존 `SimulationWorldInventorySnapshot.Players[].Items` 원장에 들어간다. r5 벌목 완료는 먼저 `NatureDroppedTimber`를 만들고 `WI-NATURE-18` Confirm이 묶음 전체를 원자적으로 인벤토리로 옮긴다. 오두막 완공 시 기존 세계 소지품 원장에 용량 20의 `container:nature-cabin:storage`를 만들며 `WI-NATURE-13` Confirm만 가능한 소지 통나무를 모두 옮기고 Transfer를 남긴다.

## 입력과 전투 연결

Unity 입력 우선순위는 `기존 현장 전투 → Farm 배치 → Nature 문맥 작업`이다. Nature에서 왼쪽 버튼은 도끼 획득, 나무 벌목 시작, 진행 중 작업 누르기에 쓰인다. `B`는 오두막 도면 배치, 조우 중 `F`는 기존 현장 전투 요청, `R`은 안전 생활핵 또는 완성 오두막 후퇴다.

전투 참여 방식은 카메라 이름이 아니라 권위 상태로 한 번 확정하고 전투 동안 잠근다.

| 참여 방식 | 기본 표현 | 입력과 규칙 |
| --- | --- | --- |
| `ObserverOperation` | 3인칭 관찰 운영 | 동결된 관찰 카드 3칸과 결정적 자동 행동을 사용한다. `Space`는 전투당 한 번의 전술 일시정지, `3`은 첫 사용 가능 비상 카드, `4` 또는 `Escape`는 개입 건너뛰기다. |
| `DirectAction` | 1인칭 직접 개입 | 기존 이동·공격·방어·회피를 직접 입력한다. 피격·회피·소요 BattleTick으로 성과를 계산하고 S/A/B 등급의 제한된 추가 보상만 지급한다. |

`FocusedAssault`, `CautiousDefense`, `WeaknessObservation`, `CabinCover`, `FieldRecovery`, `SafeRetreat`는 카드 정의 고유 식별자다. 전투 시작 시 카드 장비 hash를 동결하며 이후 원본 편성 변경은 현재 전투를 바꾸지 않는다. 자동 행동·피해·성과·보상은 Simulation Core가 계산하고 Unity는 방식·카드 고유 식별자·기대 개정만 보낸다. 전투 승리·후퇴·패배는 기존 전투 권위가 Nature 조우에 정확히 한 번 인계한다.

## 현재 증거와 제한

- Nature r5 집중 자동 시험 `45/45`: 별도 지면 통나무, 용량 차단, 멱등 획득, r1~r4 호환, `simulation-save.v24`, Replay와 LocalProcess·RemoteHost 동등성을 포함한다.
- Unity EditMode 집중 시험 `14/14`: Local Engine, Simulation Core Adapter와 Synty 시각 자료 묶음 결속을 확인했다.
- Unity PlayMode는 전체 실행에서 저장 외 경로 `5/5`를 통과했고, 저장 직후 실시간 시계 revision과 실제 저장 revision을 구분하도록 수정한 뒤 저장·복원 집중 시험 `1/1`을 통과했다.
- Game View에서 Synty 통나무 세 묶음의 실제 왼쪽 버튼 획득, Garden Shed 오두막, 황혼 Skeleton과 Table Saw를 확인했다.
- 도끼·벌목·오두막과 황혼 위협 반환은 논리·표현 E7이다. 보관·수면·새벽은 상태별 공간 변화와 카메라 가림, 작업대는 건설 중·운영 중 작업 구역 차이가 부족해 표현 E6이다.
- 실제 Provider·운영 DB·새 공식 Scene·원본 Synty Prefab은 변경하거나 호출하지 않았다.
- Unity Console에는 시험 도구 실행 중 별도 Job lock 진단이 반복돼 전체 Console 무오류 증거로 사용하지 않는다. 스크립트 재컴파일 결과는 오류 `0`이다.

기존 `WI-NATURE-01~17`은 호환을 유지한다. r5는 `WI-NATURE-18 벌목 통나무 줍기`를 추가하며 벌목 시작과 획득 책임을 분리한다. 벌목·건설 시간 진행, 황혼 조우 자동 발생, BattleTick과 새벽 자동 수면 해제는 독립 WI가 아니라 Task·자동 상태 전이·Effect다.

폐루프의 현재 판정은 [플레이 폐루프 논리·시각 이중 순환 체계](플레이폐루프논리시각이중순환체계.md)와 EvidencePackage를 따른다. 코드 통과와 Game View 식별 가능성을 서로 대신 사용하지 않는다.
