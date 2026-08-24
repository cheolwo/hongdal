# 운영·Simulation·Unity 작업 흐름 분리

## 목적

`codex/rename-ssalddel`에는 이름 전환, 운영 업무, 게임 Simulation과 공통 문서의 장기 개발 이력이 함께 있다. 이 이력을 다시 작성하거나 폐기하지 않고 **과거 통합 기준선**으로 보존하되, 이후 변경은 주 책임을 운영·Simulation·Unity로 먼저 나누고 필요한 공유 경계만 Integration으로 관리한다.

이 구분은 E·G·H·WI를 대신하지 않는다. E·G·H·WI는 게임의 성숙도·관리·공간·상호작용을 설명하고, 이 문서는 그 작업을 어느 저장소·브랜치·커밋·검증에서 다룰지 정한다.

## 저장소와 네 책임

| 책임 흐름 | 저장소 | 소유 상태와 역할 | 소유하지 않는 것 |
| --- | --- | --- | --- |
| `Operations` | `cheolwo/ssalddel` | 실제 사용자·조직, 권한·동의, 공개 범위, 계약·발주·입고·재고·결제, 운영 DB·Event·Outbox | 게임 Session·가상 시간·Save/Replay, Unity Scene |
| `Simulation` | `cheolwo/ssalddel` | 게임 규칙, Session Aggregate, Preview·Confirm, Task·Effect, 권위 실시간·WorldTick, Revision, Save/Replay, Local·Hosted 공통 Core | 실제 계약·결제·운영 원장, Unity 표현 상태 |
| `Unity` | `cheolwo/unity` | `SimulationWorldShell`, 입력·카메라·공간·UI, 상태 사본 표현, Local/Remote Runtime 소비 | 운영 원장, GameObject만으로 확정한 Simulation 결과 |
| `Integration` | 주 계약 저장소 | 호환 계약, 직렬화, Adapter, 운영 자료의 읽기 전용 파생, 저장 판본·migration, 소비자 회귀 | 독립된 네 번째 상태 원장이나 만능 공통 계층 |

운영과 Simulation은 같은 저장소를 사용하지만 같은 상태를 소유하지 않는다. Unity는 별도 저장소이므로 백엔드 push에 포함되지 않는다. Git은 폴더가 아니라 커밋을 전송하므로, 한 커밋에 여러 책임을 섞으면 push 시 분리할 수 없다.

## 기존 통합 기준선

- `codex/rename-ssalddel`의 기존 커밋은 과거 통합 이력으로 유지한다.
- 과거 커밋을 대규모로 재작성해 Operations와 Simulation으로 나누지 않는다.
- 이 브랜치 이름을 새 작업의 일반 목적 브랜치로 계속 사용하지 않는다.
- 현재 미완료 변경은 삭제·이동·복원을 일괄 처리하지 않고, 실제 책임과 검증 범위를 확인한 작은 후속 커밋으로 정돈한다.
- 전환 기준 커밋이나 태그를 고정하는 일은 현재 변경이 검증된 뒤 별도 작업으로 수행한다. 이 문서 자체는 새 기준선 확정이나 `main` 승격을 의미하지 않는다.

## 새 작업 분류 순서

```text
플레이 또는 운영 목표
  → 실제로 바뀌는 권위 상태 확인
  → 주 책임 하나 선택
  → 필요한 공개 계약과 소비자 확인
  → 짧은 브랜치와 단일 책임 커밋
  → 책임별 검증
  → 필요한 경우에만 통합 검증
```

주 책임은 파일 수가 아니라 **어떤 상태를 확정하는가**로 판단한다.

- 실제 주문이나 동의 원장을 바꾸면 `Operations`다.
- 가상 세계의 Session, Tick이나 Save를 바꾸면 `Simulation`이다.
- 같은 상태 사본을 화면·공간·입력으로 표현하면 `Unity`다.
- 두 책임이 함께 사용하는 계약·Adapter·호환 판본만 바꾸면 `Integration`이다.

한 작업이 여러 책임을 통과하더라도 먼저 하나의 작업 ID와 호환 방향을 정한 다음 책임별 커밋과 검증을 나눈다. 단순히 파일이 여러 프로젝트에 있다는 이유로 `Integration`으로 보내지 않는다.

## 브랜치와 커밋

### `cheolwo/ssalddel`

| 브랜치 | 사용 범위 | 커밋 scope 예시 |
| --- | --- | --- |
| `operations/<작업명>` | 운영·비즈니스 상태와 제품 기능 | `feat(operations):` |
| `simulation/<작업명>` | Simulation Core·Host·저장·시험 | `feat(simulation):` |
| `integration/<작업명>` | 공개 계약·Adapter·호환·교차 회귀 | `refactor(integration):` |
| `docs/<작업명>` | 실행 의미를 바꾸지 않는 기준 문서 | `docs(architecture):` |

### `cheolwo/unity`

| 브랜치 | 사용 범위 | 커밋 scope 예시 |
| --- | --- | --- |
| `unity/<작업명>` | Unity Scene·입력·표현·Runtime 소비 | `feat(unity):` |

`main`은 각 저장소의 검증된 통합 기준선으로 사용한다. 진행 중 변경을 `main`에 직접 누적하지 않고, 브랜치 하나는 가능한 한 한 책임과 한 작은 결과만 가진다.

## 교차 작업 예

Nature 오두막 세로 조각은 하나의 플레이 작업이지만 다음처럼 나뉜다.

```text
작업 ID: WI-NATURE-SHELTER

simulation/nature-shelter
├─ Preview·Confirm
├─ 자원 예약과 Task
├─ 권위 시간·WorldTick
└─ Save/Replay 시험

unity/nature-shelter
├─ 배치 입력
├─ Preview 표현
├─ 상태 사본 재표시
└─ SimulationWorldShell 검증
```

운영 원장을 사용하지 않으면 `operations/*` 브랜치를 만들지 않는다. 운영에서 승인한 자료가 필요하면 원자료·권한·출처는 Operations가 소유하고, Simulation에는 명시적인 읽기 전용 파생 계약과 세션 동결 상태만 Integration을 거쳐 전달한다.

## 커밋 전 확인

1. 작업 ID와 주 책임이 적혀 있는가?
2. 변경 파일이 선택한 책임의 `sourceRoots` 안에 있는가?
3. 다른 책임의 상태를 직접 쓰고 있지 않은가?
4. 공유 계약 변경이라면 생산자와 소비자 호환 시험이 있는가?
5. Operations·Simulation·Unity 증거를 한 완료 주장으로 합치지 않았는가?
6. 이번 커밋과 무관한 기존 dirty 변경을 stage하지 않았는가?

기계 판독 기준은 `eng/work-areas/responsibility-workstreams.json`과 연결된 네 manifest를 따른다. 기존 `simulation-unity.json`은 코드 지도 생성을 위한 호환 집계로 유지하며 새 작업의 책임 분류는 세부 manifest를 먼저 사용한다.
