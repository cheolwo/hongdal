# Graph Map 기획 인계 순환 체계

## 목적

기획 스레드는 플레이어 약속과 선택을 계속 심화하고, Graph Map 작업은 승인된 기획 판본을 레벨 1·2·3의 구조로 옮겨 검증한다. 두 작업을 분리하되 파일 판본과 반환 결과로 이어서, 기획이 쌓일수록 Graph Map이 갱신되고 Graph Map에서 발견한 공백이 다음 기획 질문으로 돌아오게 한다.

이 체계는 Graph Map을 새 상태 권위나 월드맵으로 만들지 않는다. Graph Map은 실제 E5 배치 전에 기획 관계·공간 제약·코드 결속을 함께 검토하는 도구다. Unity Scene, Prefab 적용, 실제 입력·보행·Game View와 E 승격은 별도 승인과 증거가 필요하다.

## 한 줄 흐름

```text
단일 질문 심화
  → 승인된 기획 판본과 SHA-256 동결
  → Graph Map 영향 판정·인계 등록
  → 기존 맵 대조와 레벨 1·2·3 반영
  → 구조·참조·신선도 검증
  → Integrated / Blocked / NoImpact 최종 반환
  → 다음 기획 질문 또는 개발 인계
```

## 소유권

| 역할 | 소유하는 것 | 하지 않는 것 |
| --- | --- | --- |
| 기획 스레드 | 지금·여기·나·너·이렇게, 결과·다음 선택, 승인 판본, Graph Map 영향과 제외 범위, 인계 요청 | Graph Map JSON·생성물·검사 도구 수정, Unity 구현, 중간 실행 대기 |
| Graph Map 작업 | 기존 맵 대조, 레벨 1·2·3 구조화, 하위 맵·포트·connector·overlay 갱신, 기계 검증, 최종 반환 | 누락된 플레이 선택·대가·실패·회복을 임의 창작, Scene/Prefab 적용, E 자동 승격 |
| 개발 통합 | Graph Map 결과를 작업 명세·Logic/Presentation·실제 E5 실행 범위와 연결 | Graph Map의 계획 관계를 운영·Simulation 사실로 간주 |
| 월드·공간·배치 | 승인된 실제 실행 범위의 Scene·Prefab·입력·Game View 검증 | 기획 또는 Graph Map 계획만으로 월드 적용 주장 |

기획 스레드는 인계한 뒤 Graph Map 작업의 중간 단계를 기다리지 않는다. 다른 기획을 이어가며, Graph Map 작업이 `Integrated`, `Blocked`, `NoImpact` 가운데 하나로 끝났을 때 결과를 인수한다. 같은 Graph Map 파일을 여러 작업이 동시에 수정할 때는 Graph Map 담당이 쓰기 소유를 직렬화한다.

## 기획 판본의 필수 인계 정보

승인된 기획 문서는 다음 항목을 사람에게 읽히는 형태로 가진다. 인계 원장은 문서 본문을 복제하지 않고 절·앵커와 SHA-256으로 참조한다.

1. 기획 판본 코드와 승인 상태
2. 원문 문서 경로와 SHA-256
3. 관련 결정 ID와 기존 WI ID
4. `지금·여기·나·너·이렇게→결과→다음 선택` 일곱 칸
5. Graph Map 영향: `NoImpact`, `UpdateExisting`, `CreateSubgraph`, `CreateGraphMap`
6. 레벨 1에 필요한 플레이 관계
7. 레벨 2에 필요한 배치 제약과 미정 수치
8. 레벨 3에서 조사할 코드·컴포넌트 역할
9. 바꾸지 않을 권위·Scene·규칙과 검증 상한

핵심 칸이나 플레이어 대가·실패·회복·귀환이 미정이라면 Graph Map 작업은 그 의미를 채우지 않는다. 구조화 가능한 부분만 반영하고 질문이 필요한 항목을 `Blocked` 또는 미연결 반환으로 남긴다.

## Graph Map 영향 판정

| 판정 | 사용 조건 | 결과 |
| --- | --- | --- |
| `NoImpact` | 공간·관계·표현·코드 결속에 변화가 없는 수치·문구 정제 | 맵을 고치지 않고 근거와 이유만 반환 |
| `UpdateExisting` | 기존 노드·엣지·제약·결속의 의미나 상태를 보완 | 기존 안정 ID를 유지하고 판본을 올림 |
| `CreateSubgraph` | 기존 federation 안에서 독립 책임·규모·소유 경계가 새로 필요 | 하위 맵과 port/connector를 추가하고 전체 맵과 연결 |
| `CreateGraphMap` | 기존 맵과 플레이·공간 책임이 독립이고 연결은 외부 관문으로 충분 | 새 Graph Map을 만들고 명시적 connector만 둠 |

유사한 이름, 같은 Area, 같은 자산 팩만으로 새 노드나 관계를 추론하지 않는다. 기존 WI·결정·H·AreaSet·Graph의 안정 ID가 있으면 먼저 재사용한다.

## 레벨별 반영 책임

### 레벨 1 — 플레이 관계

- 노드: 발견·생산·입고·회복처럼 플레이어가 판독하는 역할과 선택 지점
- 엣지: 이동·발견·작업 인계·물류·외부 관문
- 각 노드의 일곱 칸, 관련 WI·결정, 실제 참조와 미해결 계획의 구분
- 필수·선택·분리·미정 연결과 귀환 가능성

레벨 1은 “무엇을 왜 이어서 플레이하는가”를 답한다. 좌표·간격·Prefab 이름을 플레이 의미 대신 쓰지 않는다.

### 레벨 2 — 배치 제약

- 통행·접근·지지·차폐·간격·가시선·방향·용량·안전 경계
- 노드 내부와 노드 사이의 제약 소유
- 확정 수치, 연구 기준, 미정 수치의 구분
- 기존 배치·LH·월드맵 엔진에 맡길 집행 책임

레벨 2는 “어떤 조건을 만족해야 자연스럽고 실행 가능한가”를 답한다. 계획 제약 통과는 실제 Scene 통과가 아니다.

### 레벨 3 — 코드·컴포넌트 결속

- 관련 assembly, 파일 경로, 심볼, SHA-256
- 읽기·검사·조립·표현·Editor 전용 역할
- 레벨 1·2 대상 selector와 미결속 대상
- canonical Scene과 Runtime 증거 경계

레벨 3은 코드 본문을 복제하지 않고 “어디에서 무엇을 소비·검사하는가”만 연결한다. 파일 존재·hash 일치는 실제 배선·Play Mode·Game View를 증명하지 않는다.

## 인계 상태

```text
Draft
  → ApprovedForHandoff
  → AcceptedByGraphMap
  → Integrated | Blocked | NoImpact
  → Superseded
```

- `Draft`: 기획이 아직 바뀔 수 있어 Graph Map 쓰기를 시작하지 않는다.
- `ApprovedForHandoff`: 기획 판본·hash·범위가 동결됐다.
- `AcceptedByGraphMap`: 담당이 신선도와 쓰기 소유를 확인했다.
- `Integrated`: 요청한 레벨이 반영되고 검사·반환 근거가 있다.
- `Blocked`: 기존 판본 충돌, 누락된 플레이 결정, 실제 참조 공백 등으로 안전하게 반영할 수 없다.
- `NoImpact`: 검토 결과 Graph Map 변경이 필요하지 않다.
- `Superseded`: 새 기획 판본이 이전 인계를 대체한다.

중간 실패를 `Integrated`로 올리지 않는다. `Blocked`는 차단 항목, 가장 이른 책임 레벨, 기획 질문 여부를 함께 반환한다.

## 기계 판독 원장과 생성 조회

- 인계 원본: [`graph-map-planning-handoffs.json`](../../eng/world-seedbeds/graph-map-planning-handoffs.json)
- 관리 도구: [`manage-graph-map-planning-handoffs.ps1`](../../eng/world-seedbeds/manage-graph-map-planning-handoffs.ps1)
- 생성 JSON: [`graph-map-planning-handoffs.v1.json`](../../eng/world-seedbeds/generated/graph-map-planning-handoffs.v1.json)
- 사람용 상태판: [`graph-map-planning-handoffs.md`](../AI/generated/graph-map-planning-handoffs.md)
- 회귀시험: [`graph-map-planning-handoffs.ps1`](../../eng/tests/graph-map-planning-handoffs.ps1)

원장은 기획 의미의 단일 원본이 아니다. 기획 문서와 결정은 의미를 소유하고, 원장은 판본·대상·상태·반환을 연결한다. 생성 문서는 직접 수정하지 않는다.

## 반환 형식

Graph Map 작업은 최종 반환에서 다음을 구분한다.

- 반영: 추가·수정·재사용한 레벨 1/2/3 식별자
- 미반영: 이유와 원문 위치
- 차단: 기획 선택 필요 / 기술 조사 필요 / 실제 E5 실행 필요
- 검증: Graph Map Check, 회귀시험, 범위 Fast
- 증거 상한: Scene·Prefab·입력·Game View·E 승격 여부
- 결과 판본: Graph Map revision·hash와 보고서

기획은 플레이어 선택이 필요한 차단만 다음 단일 질문으로 이어간다. 기술 공백은 개발 또는 전문 담당의 후속으로 보내며 같은 질문을 반복하지 않는다.

## 개발로 이어지는 다음 인계

Graph Map이 `Integrated`됐다고 전체 맵을 곧바로 개발하지 않는다. [Graph Map 개발 인계 체계](GraphMap개발인계체계.md)에 따라 한 PlayableLoop·대표 WI·검증 가능한 노드/엣지/제약 묶음을 작은 slice로 고르고, 기존 Goal·E7 작업 명세·work item 후보와 결속해 `ReadyForDevelopment`로 넘긴다. 개발이 판본·소유·정확 쓰기 경로·검증 상한을 확인하기 전에는 자동 활성화하지 않는다.

## 첫 적용

[북부 생활권 첫 Graph Map 상세 제안](../AI/북부생활권-첫그래프맵-상세제안-2026-09-01.md)을 첫 `Integrated` 사례로 등록한다. Nature→Farm→Hub→Town과 요동성 외부 관문은 레벨 1, 배치 전 제약은 레벨 2, 공용 Unity 코드 결속은 레벨 3에 반영됐다. 이 기록은 기존 결과를 새 Unity 실행이나 E5 성립으로 소급 승격하지 않는다.
