# 살뜰 운영 서버 0.0~3.5에서 Mirror Unity로의 이관

- 기획 ID: `PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001`
- 기획 분야: 시스템·운영 기능 이관
- 기획 판본: `operations-unity-transfer.r1`
- 상태: `ApprovedForHandoff`
- 상위 기획: `PLAN-GAME-COMMON-PURPOSE-001`
- 관련 하위 기획: `PLAN-GRAPH-HUB-LOGISTICS-CIRCULATION-001`, `PLAN-PRESENTATION-E4-POOL-001`
- 관련 결정: 운영·Simulation·Unity 권위 분리, Farm·Hub·City 독립 영역 우선
- 관련 WI·PlayableLoop: `WI-001`, `WI-002`, `playable-loop:hub-inbound-putaway.v1`
- 플레이 순서: 독립 영역별. Hub 창고를 첫 기술 표본으로 삼되 게임의 필수 첫 방문 순서로 고정하지 않는다.
- Graph Map 영향: Hub 입고·검수·적치 노드와 내부 작업 엣지의 기존 안정 ID를 재사용한다.
- 다음 인계 상태: `ReadyForGraphMap`; Goal 자동 활성화·Unity Scene 변경·Evidence 자동 승격은 금지한다.

## 1. 목적

살뜰 운영 서버가 0.0 커뮤니티·공공데이터부터 3.5 마트·도심 물류까지 보유한 업무 기능을 빠짐없이 조사하고, Mirror에서 플레이어가 실제로 다룰 부분과 서버에 남길 부분을 같은 대장으로 관리한다.

전수 조사 모수는 Unity 객체 수가 아니다. 페이지·API·저장 개체를 그대로 H1로 복제하지 않고, 서버의 권위 있는 업무 상태를 플레이어 과업과 공간으로 번역한다.

## 2. 확정된 이관 등급

모든 운영 기능은 다음 기본 등급 하나를 가진다.

| 등급 | 의미 | Unity 기본 표현 |
| --- | --- | --- |
| `PlayableAction` | 플레이어가 선택하고 명시적으로 확인할 수 있는 업무 | H1 상호작용·선택 UI. 운영 변경은 서버 Command 뒤 재조회 |
| `ReadOnlyContext` | 상태·근거·이력·가격처럼 읽는 것이 중심인 정보 | World Object에서 여는 Panel 또는 선택형 상세 |
| `AmbientSimulation` | 차량·NPC·재고 흐름처럼 세계의 움직임을 설명하는 가상 상태 | 권위 상태 사본을 읽는 환경 표현. 운영 저장 금지 |
| `ServerOnly` | 금융·급여·개인정보·관리자·내부 Run·게임 내 실행이 승인되지 않은 외부 효과 | World Object로 만들지 않고 필요하면 권한 있는 Web으로 인계 |

등급은 서버 실행 권한을 부여하지 않는다. `PlayableAction`도 기존 권한·revision·Preview·Confirm·canonical 재조회를 통과해야 한다.

## 3. H1·H2 번역 규칙

### H1

H1은 플레이어가 식별하고 접근할 수 있는 **과업·작업 공간·상호작용 지점**이다. DB 행, 이력 행, Outbox, 수집 Run, 관리자 설정은 H1이 아니다.

한 H1은 여러 페이지·UseCase·저장 개체를 소비할 수 있고, 같은 서버 기능도 관점에 따라 여러 H1에서 읽을 수 있다. 관계는 다대다로 관리한다.

### H2

H2는 둘 이상의 H1을 시작·선택·결과·회복 또는 귀환으로 연결하는 독립 업무 블록이다. Farm·Hub·City의 H2는 다른 영역의 화물을 필수 시작 상태로 요구하지 않는다. 영역 간 운송은 양쪽 독립 폐루프가 준비된 뒤 별도 통합 엣지로 연다.

| 서버 업무 | 재사용 H1 | 재사용 H2 |
| --- | --- | --- |
| 창고 입고·검수·적치 | `h1-stock:hub-receiving-storage` | `h2-candidate:hub-inbound-storage`, `h2-candidate:hub-internal-warehouse` |
| 창고 피킹·포장·출고 준비 | `h1-stock:hub-outbound-staging`, `h1-stock:hub-temporary-staging` | `h2-candidate:hub-fulfillment` |
| 운송 상하차·차량 대기 | `h1-stock:hub-vehicle-yard`, `h1-stock:hub-town-corridor` | `h2-candidate:hub-outbound-vehicle`, `h2-candidate:hub-town-corridor` |
| 마트 주문·수령 | `h1-stock:town-market-display`, `h1-stock:town-order-packing`, `h1-stock:town-resident-pickup` | `h2-candidate:market-life-commerce`, `h2-candidate:town-order-fulfillment` |

커뮤니티·공공데이터·공동구매·음식점처럼 현행 H 정의가 충분하지 않은 기능은 기존 H를 억지로 재사용하지 않고 `HMappingRequired`로 남긴다.

## 4. 전수 이관 대장

기계 판독 대장은 다음을 각각 독립 배열로 보존한다.

- 0.0~3.5 페이지 기능 규칙 전체
- EF Core `DbSet` 전체와 MongoDB collection 전체
- 현행 Unity 대표 Page-to-World 경로와 표현 구현 근거
- 페이지별 canonical 업무 기능, 이관 등급, 표시 방식, H1/H2·WI·PlayableLoop 후보
- 권한·개인정보·외부 효과와 가장 이른 Evidence 재개 단계
- 판본·파일 hash와 대장 불일치 진단

페이지 별칭은 canonical 기능 ID로 묶는다. 다만 자동 정규화로 의미를 확정하지 않으며, 검증된 별칭 규칙만 같은 ID를 공유한다.

## 5. 첫 독립 표본: Hub 입고·검수·적치

### 지금·여기·나·너·이렇게

- 지금: Hub 내부 입고 상태 사본이 조회되었고 검수·적치 작업을 고를 수 있는 시점
- 여기: `area-set:sim:pyeongchang:logistics-hub.v1` 안의 입고·검수·보관 작업 공간
- 나: 창고 관리자 관점 또는 독립 Hub Simulation의 허용된 행위 주체
- 너: 입고 화물, 검수 상태, 보관 위치와 이를 처리하는 NPC 작업
- 이렇게: `WI-001`로 화물을 검수하고 `WI-002`로 적치한 뒤 `HubWorkChoiceAvailable`로 돌아간다.

### 기존 계약 재사용

- 서버 계약: `WarehouseWorldSnapshotRoutes.AuthorizedSnapshot`
- 서버 조회: `창고WorldSnapshot조회UseCase`
- Unity 소비: `WarehouseWorldApiRepository`, `WarehouseWorldInterpreter`, `WarehousePresenter`
- H1: `h1-stock:hub-receiving-storage`
- H2: `h2-candidate:hub-inbound-storage`, 상위 조립 후보 `h2-candidate:hub-internal-warehouse`

새 Entity별 API나 별도 공식 Scene을 만들지 않는다. 첫 구현은 권한 있는 Snapshot 조회·해석·표현 준비를 재사용한다.

### 현재 Evidence와 차단

- 서버 Snapshot 계약과 Unity 읽기·해석·표현 코드: 존재
- PlayableLoop 기획 관문: 이 판본으로 승인 가능
- H2 세부 연결구·내부 도달 가능성: 미검증
- 실제 Prefab·World 배치·활성 Renderer/Collider/Bounds·입력: 미검증
- 운영 Command 연결과 canonical 재조회: 조회 표본 뒤 별도 WI에서 검증
- 현행 Presentation은 E1이며 이 이관 대장의 목표 상한은 E4 준비다. E5는 실제 배치 증거 전까지 금지한다.

## 6. 영역별 후속 순서

1. Hub 창고 입고·검수·적치
2. Hub 출고 준비와 내부 작업 반환
3. 운송·배차 독립 폐루프
4. 음식점 주문·조리·픽업 독립 폐루프
5. 마트 주문·피킹·수령 독립 폐루프
6. 커뮤니티·공공데이터·공동구매의 선택형 조회·협의
7. 양쪽 영역이 독립 준비된 뒤 창고→운송, 음식점→배달, 마트→도시 배송 통합

이 순서는 제품 출시 판본의 선후행 의존성을 뜻하지 않는다.

## 7. E4에서 E5로 가는 관문

다음 항목이 같은 판본으로 결속된 H1만 E5 실행 후보가 된다.

1. Logic E5 상태 사본 또는 명시적인 읽기 전용 권위 Projection
2. 플레이어 판독 순간과 `VisualKey`
3. 동결된 주·대체·fallback 자산 후보
4. Graph Map 레벨 1~3과 배치 맵의 시야·통행·접근·간격 제약
5. `InteractionAnchor`, 입력, 결과, 취소·해제·귀환 조건
6. active Renderer·Collider·Bounds와 동일 revision의 실제 World 관측
7. 운영 변경이면 Preview→Confirm→server Command→canonical 재조회

정적 Fixture, 컴파일, 단위 시험, 보조 Scene 또는 이미지 후보만으로 E5를 선언하지 않는다.

## 8. 상위 목적 정렬

- 나의 막힘: 방대한 운영 기능을 그대로 World Object로 옮기면 무엇을 해야 하는지 판독하기 어렵다.
- 회복 행동과 대가: 플레이어 과업 단위로 선별 번역하되 정밀 관리·민감 업무는 Web에 남긴다.
- 기여 자원: 기존 0.0~3.5 기능, 권위 Snapshot, WI, H1/H2, Graph Map, Synty 표현 후보
- 상대의 실제 필요: 창고·운송·음식점·마트·공동체 각 영역이 독립적으로 읽히고 작동해야 한다.
- 권위 결과: 서버 또는 Simulation Core가 판정하고 Unity는 같은 revision을 표현한다.
- 환류: 성공 결과를 재조회해 다음 과업과 H1 상태를 갱신한다.
- 보조 층: 공공데이터 근거, 상세 이력, Web 인계, 환경 NPC·차량 표현

## 9. 확정·미정

### 확정

- 네 등급 선별 이관
- 영역별 독립 폐루프 우선
- 첫 기술 표본은 Hub 입고·검수·적치
- H1은 과업 공간, H2는 독립 업무 블록
- Unity는 운영 상태를 직접 쓰지 않음

### 미정 또는 후속 검증

- 서버 기능별 최종 H 신규 생성 여부
- 음식점 전용 H1/H2의 정확한 공간 구성
- Hub H2 연결구·동선·자산과 실제 E5 판본
- 운영 Command를 게임 내에서 허용할 개별 WI와 권한

## 10. 구현된 관리 도구

- 정책 원본: `eng/execution-ledgers/operational-unity-transfer-policy.json`
- 생성·조회: `eng/execution-ledgers/manage-operational-unity-transfer-catalog.ps1`
- 기계 대장: `docs/AI/generated/operational-unity-transfer-catalog.json`
- 사람이 읽는 대장: `docs/AI/generated/operational-unity-transfer-catalog.md`
- 구조 회귀: `eng/tests/operational-unity-transfer-catalog.ps1`
- 첫 표본 상세 기획: `docs/Architecture/PlayableLoops/Hub입고검수적치.md`

대장은 현재 코드에서 페이지 기능, EF Core `DbSet`, MongoDB `GetCollection` 사용 지점과 Unity 대표 경로를 다시 읽는다. H 대응은 검토 후보이며 중앙 H 대장의 선언 수와 실제 안정 ID 수가 다르면 수정하지 않고 진단으로 반환한다.
