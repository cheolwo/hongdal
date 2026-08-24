# 지역 사건·Nature 위협·지역 발전 모듈 경계

## 목적

업무 영역의 잘못된 선택, Nature의 위협과 전투, 플레이어가 선택하는 공간 발전을 한 상태 전이로 뭉치지 않는다. 각 모듈은 한 종류의 질문에만 답하고, 다음 모듈에는 안정된 고유 식별자와 상태 사본으로 인계한다.

이 문서는 새 기능의 완료 증거가 아니다. 현재 구현을 정리하고 [Nature 위협 대응에서 Farm 공간 발전으로 돌아오는 작업 명세](../../eng/execution-ledgers/work-orders/nature-farm-regional-development.e9-work-order.json)를 구현할 때 지켜야 할 책임 경계다.

## 플레이어가 경험하는 흐름과 권위 모듈

```text
Farm에서 안전하지 않은 선택
  → 업무 영역 사건 원장
  → 지역 인과와 경로별 압력 계산
  → Nature 경고·조우 투영
  → 플레이어 전투 결과
  → 즉시 경로 안전 + 사건별 발전 기회
  → Farm 귀환
  → 플레이어가 발전 프로젝트 선택
  → 재료·노동·시간을 들여 H1 조립
  → 세 H1 완료 뒤 H2 독립 준비
```

플레이어 화면에서는 한 폐루프로 보이지만 상태 권위는 아래처럼 나눈다.

| 모듈 | 답하는 질문 | 소유 상태 | 소유하지 않는 것 |
| --- | --- | --- | --- |
| `RegionalIncident` | 어느 업무 선택이 어떤 원인 사건을 만들었는가? | 사건, 원인 WI, 기한, 남은 심각도, 응답 | Nature 적 생성, 전투, 발전 프로젝트 |
| `RegionalCausality` | 확정 결과가 위협·회복 계보에 무엇을 더했는가? | 원인 변화, 위협·회복 점수 | 사건 해결, 공간 배치 |
| `NatureThreatPressurePolicy` | 현재 원장으로 경로 압력이 얼마인가? | 없음. 순수 계산 결과만 반환 | 조우 생성, 상태 변경 |
| `NatureThreatProjection` | 압력을 어떤 경고·조우로 보여야 하는가? | 조우와 World Event 투영 | 업무 사건 응답, 전투 판정 |
| `NatureEncounterOutcome` | 권위 있는 전투 결과가 무엇을 즉시 바꾸는가? | 전투 결과 멱등성, 즉시 안전 인계 | 원인 WI 완료, Farm 프로젝트 시공 |
| `RegionalDevelopment` | 어떤 원인 사건이 어느 업무 영역에 발전 기회를 만들었는가? | 기회 발급·예약·소비, 영역 준비도, 연결 후보 | 재료·노동 재고 자체, Unity 배치 |
| `FarmDevelopmentProject` | 플레이어가 어떤 Farm H1을 어디에 조립하는가? | Preview·Confirm, 자원 예약, Task, Operational 결과 | Nature 압력 직접 변경, 다른 영역 자동 발전 |

## 코드 소유 위치

공개 형식 이름과 JSON 필드는 호환을 위해 유지한다. 파일 분리는 소유 책임을 드러내기 위한 것이며 API 이름 변경이 아니다.

```text
Ssalddel.Simulation.Contracts/UnityPackage/Runtime/
├─ SimulationRegionalIncidentContracts.cs
├─ SimulationRegionalCausalityContracts.cs
├─ SimulationNatureInteractionContracts.cs
├─ SimulationNatureThreatContracts.cs
└─ SimulationRegionalDevelopmentContracts.cs       # 다음 구현

Ssalddel.Simulation.Domain/UnityPackage/Runtime/
├─ SimulationRegionalIncidents.cs
├─ SimulationRegionalCausality.cs
├─ SimulationNatureThreatPressurePolicy.cs
├─ SimulationNatureThreatProjection.cs
├─ SimulationNatureEncounterOutcome.cs
├─ SimulationRegionalDevelopment.cs                # 다음 구현
└─ SimulationFarmDevelopmentProjects.cs            # 다음 구현
```

`Simulation.Server`와 `LocalSimulationRuntime`은 이 규칙을 다시 구현하지 않는다. 둘은 같은 공통 Core를 각각 `RemoteHost`와 `LocalProcess`에서 실행한다. Unity `GameObject`는 상태를 확정하지 않고 `SimulationWorldShell`에서 상태 사본과 Preview·Confirm을 투영한다.

## 전이 불변 조건

1. 업무 사건의 남은 심각도는 원인 WI 또는 명시적인 업무 영역 규칙만 변경한다.
2. Nature 전투 승리는 업무 사건을 해결하지 않는다.
3. 전투 승리는 같은 경로의 즉시 안전과 발전 기회 발급의 원인이 될 수 있다.
4. 같은 원인 사건에서는 발전 기회를 한 번만 발급한다.
5. 발전 기회는 건설 비용을 대신하지 않는다. 프로젝트 시작 자격으로만 사용한다.
6. 기회는 Confirm에서 예약하고, 취소하면 반환하며, H1이 `Operational`이 될 때 소비한다.
7. 세 Farm H1이 모두 `Operational`일 때만 `h2-candidate:farm-incident-containment`를 `IndependentReady`로 판정한다.
8. H2 준비는 Nature↔Farm 연결 후보를 `Available`로 만들 뿐, 영역 간 실제 시설·운송·업무를 자동 생성하지 않는다.
9. Town과 City/Hub는 Farm 결과를 시작 조건으로 삼지 않는다. 각 영역의 독립 폐루프가 준비된 뒤 같은 공통 계약을 별도 구현한다.
10. 기존 저장 판본의 명령 의미는 재생 중 조용히 바꾸지 않는다. 새 전이는 새 저장 판본과 명시적 migration 경계에서만 활성화한다.

## 첫 Farm 프로젝트 집합

| H 깊이 | 고유 식별자 | 플레이어가 얻는 새 선택 |
| --- | --- | --- |
| H1 | `h1-stock:farm-exposure-inspection` | `WI-FARM-07`로 사건 원인·기한·필요 WI를 점검한다. |
| H1 | `h1-stock:farm-incident-quarantine` | `WI-FARM-08`로 해당 사건의 타 경로 확산 기여만 격리한다. |
| H1 | `h1-stock:farm-weather-protection` | `WI-FARM-09`로 해당 사건 기한을 한 번 연장한다. |
| H2 | `h2-candidate:farm-incident-containment` | 세 H1의 실제 완료를 묶어 독립 준비 상태를 판정한다. |

세 프로젝트의 세부 재료량과 작업 시간은 구현 전 시험 Fixture에서 확정한다. 문서에 이름이 있다고 H1/H2가 Actual E5에 배치되거나 E7 플레이 증거가 생긴 것은 아니다.

## 구현 재개 순서

1. 현재 파일 분리 뒤 지역 사건 집중 시험과 저장·재생 회귀를 통과시킨다.
2. `RegionalDevelopment` 계약과 빈 상태 사본을 추가하되 승리 규칙은 아직 바꾸지 않는다.
3. 신규 저장 판본과 구판 재생 정책을 먼저 고정한다.
4. `NatureEncounterOutcome`에서 즉시 안전과 단일 기회 발급을 구현한다.
5. Farm 프로젝트 Preview·Confirm·취소·완료를 기존 배치·Task 경계에 연결한다.
6. WI-FARM-07~09 효과를 각각 독립 시험한다.
7. Local·Remote 결과 동일성과 Save/Replay hash를 검증한다.
8. 마지막에만 기존 Farm 배치 팔레트와 `SimulationWorldShell` 표현을 연결한다.

E1~E7 코드와 시험이 생겨도 실제 Play Mode·Game View 폐루프가 없으면 E7로 승격하지 않는다. NPC 생활세계와 실제 변화 적응 기준선이 없으므로 E8·E9 승격도 하지 않는다.
