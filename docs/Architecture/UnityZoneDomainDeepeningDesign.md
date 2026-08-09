# Unity Zone 업무 심화 설계

## 1. 목적

이 문서는 P0~P7의 `MonoBehaviour` Controller, primitive Scene과 VContainer 배선을 출발점으로, 각 Zone을 실제 업무가 살아 움직이는 공간으로 심화하는 기준과 구현 순서를 정의한다.

이 단계의 개발 단위는 Controller나 Prefab 하나가 아니라 **현실 업무 하나를 끝까지 투영하는 Zone vertical slice**다.

```text
현실 업무와 제약
  → canonical Domain / Ledger / UseCase
  → 권한별 server Projection
  → Unity ApiModel / Mapper / Repository
  → Zone Snapshot / Role Perspective
  → SceneController
  → World View / Role View / Detail View
  → NPC / 물품 / 경로 / animation
  → 명시적 interaction
  → server Command 성공과 canonical 재조회
```

게임 목표, 제한 시간과 점수를 먼저 만들지 않는다. 현실 업무를 정확히 공간화한 뒤 반복 관찰에서 드러나는 선택, 제약, 병목과 피드백을 게임 후보로 수집한다.

공통 데이터·권한 원칙은 [서버 상태에서 Unity World Projection으로의 설계](UnityServerStateToWorldProjectionDesign.md), 현재 구현 위치는 [Unity World 구현 현황과 우선순위](UnityWorldImplementationPriority.md)를 따른다. 이 문서는 두 기준을 반복하지 않고 **Zone을 얼마나 깊게 구현할 것인가**를 다룬다.

## 2. 현재 기준선과 범위

2026-08-08 현재 P0~P7에는 Zone별 `MonoBehaviour` SceneController, View socket, primitive Scene Builder와 VContainer LifetimeScope가 있다. 따라서 다음 병목은 GameObject 생성 여부가 아니라 아래 질문에 답할 수 있는가다.

1. 이 공간에서 실제로 어떤 업무가 발생하는가?
2. 서버에서 그 업무의 현재 상태를 권위 있게 확인할 수 있는가?
3. 사용자의 역할과 업무 관계에 따라 어떤 정보와 행동이 허용되는가?
4. 상태 변화가 공간, 물품, NPC와 경로 변화로 어떻게 보이는가?
5. 사용자가 관찰·탐색·확인·실행한 뒤 무엇을 다시 조회해야 하는가?
6. 반복되는 실제 의사결정에서 어떤 플레이 가능성이 발견되는가?

이 문서는 실제 제품 Unity 프로젝트에 Scene을 배치했다는 증거가 아니다. `Samples~`의 importable Presentation code와 현재 server contract를 기준으로 다음 구현 slice를 설계한다.

## 3. Zone 심화 단계

각 Zone은 아래 단계를 순서대로 통과한다. 뒤 단계가 앞 단계의 권위와 검증을 대신하지 않는다.

| 단계 | 이름 | 완료 조건 |
| --- | --- | --- |
| Z0 | 배선 | Controller, LifetimeScope, primitive View와 wiring validation이 있다. |
| Z1 | 현실 관측 | 실제 권한 projection이 World object, 상태, 출처와 기준시각으로 보인다. fixture는 명확히 구분된다. |
| Z2 | 흐름 표현 | 최소 한 업무가 시작·진행·완료 상태에 따라 물품, marker, NPC route와 공간 점유 변화로 보인다. |
| Z3 | 역할 경험 | 같은 object를 역할별로 다르게 강조하고, 허용 interaction과 민감 정보 범위를 서버가 결정한다. |
| Z4 | 실행 폐루프 | 확인 panel → server Command → 성공 snapshot 재조회가 연결되고 충돌·권한 오류·stale을 표현한다. |
| Z5 | 시간 수렴 | refresh 또는 event 뒤 stable ID와 revision으로 여러 object와 NPC가 같은 canonical 상태에 수렴한다. |
| Z6 | 게임성 발견 | 실제 병목·선택·자원·협력 패턴을 관찰 자료로 설명하고, 별도 simulation 또는 운영 보조 기능 후보로 승인한다. |

Z6는 점수 UI를 추가했다는 뜻이 아니다. 현실 근거가 없는 목표·보상·패널티는 Z6로 인정하지 않는다.

## 4. 심화 우선순위

### 4.1 평가 기준

각 항목을 1~5로 평가한다.

| 기준 | 질문 |
| --- | --- |
| Canonical readiness | 실제 Entity, 원장, UseCase와 권한 조회가 이미 존재하는가? |
| State dynamics | 상태 전이가 여러 단계이며 관찰 가치가 있는가? |
| Spatial legibility | object 이동·점유·대기·경로를 공간에서 이해하기 쉬운가? |
| Cross-zone reuse | 다른 Zone과 NPC·화물·상품·원장을 연결할 수 있는가? |
| Closure cost | 현재 코드에서 Z2~Z4 한 흐름을 닫는 비용이 낮은가? |
| Safety clarity | 개인정보·실행 효과 경계를 서버에서 명확히 제한할 수 있는가? |

### 4.2 개발 심화 순서

| 순위 | Zone | 핵심 이유 | 첫 심화 목표 |
| ---: | --- | --- | --- |
| 1 | 창고 | 재고·적재·피킹·출고 canonical 업무와 NPC socket이 이미 존재한다. | 입고 Dock에서 보관 위치, 피킹과 출고 대기까지 하나의 물품 흐름을 표현한다. |
| 2 | 도심 물류센터 | 운송 원장·화물 인계·Transporter NPC가 창고와 직접 연결된다. | 차량 도착, 하차, 입고 인계와 다음 운송을 거점 흐름으로 만든다. |
| 3 | 농장 | Farm aggregate와 센서·재배·농장작업 계약이 생겼고 시각적 변화가 크다. | 실제 sensor ingestion과 서버 판정이 작물·작업자 표현을 바꾸게 한다. |
| 4 | 도심마트 | 공개 상품·가격·재고가 있고 창고·주문 수요와 연결 가능하다. | 진열 상태와 판매 가능 수량 변화가 보이는 매장 운영 흐름을 만든다. |
| 5 | 주거공동체 수령 | 주문자·운송자 권한 경계가 고정되어 마지막 구간을 검증하기 좋다. | 도착→공동수령지 대기→권한 있는 수령/하차 확인을 표현한다. |
| 6 | 전통시장·물류거점 | 공개 시장·거점은 있으나 운영 집하·공동배송 원장이 상대적으로 얕다. | 공개 공간 위에 집하 상태를 별도 operational projection으로 겹친다. |
| 7 | 공공데이터 정보관 | 공개·출처 경계가 안정적이지만 업무 실행보다 탐색 경험 중심이다. | 여러 출처·시점·지역을 비교하고 근거를 추적하는 탐색 경험을 깊게 한다. |
| 8 | 커뮤니티·시장 광장 | 제품 중심 공간이지만 사람·참여·공개 범위와 실행 동의가 복잡하다. | 공개 관찰에서 관심·참여 의향까지를 안전하게 연결하고 실행은 분리한다. |

이 순서는 **개발 심화 순서**다. 제품 기본 공개 순서는 여전히 공공데이터 정보관과 커뮤니티·시장 광장이 우선이다. 창고를 먼저 깊게 만든다고 유상 물류, 자동 배차나 운영 Command를 기본 공개하지 않는다.

P8 협동조합·공동원장 공간은 별도 빈 공간으로 먼저 만들지 않는다. 앞 Zone에서 공통 원장 요구가 실제로 확인될 때 생성한다.

## 5. Zone 하나를 설계하는 공통 양식

각 Zone 작업은 아래 12개 항목을 한 문서 또는 work item에서 채운 뒤 구현한다.

| 번호 | 항목 | 반드시 답할 내용 |
| ---: | --- | --- |
| 1 | 현실 업무 | 실제 시작 조건, 단계, 완료 조건과 예외는 무엇인가? |
| 2 | Actor | 누가 관찰하고 누가 실행하며 누가 승인하는가? |
| 3 | Server Data | canonical aggregate, 이력, 외부 관측과 권한 출처는 무엇인가? |
| 4 | Perspective | Public, OwnerOnly, ParticipantOnly, AuthorizedRole 중 무엇인가? |
| 5 | Snapshot | object, 상태, revision, 기준시각, source와 relation을 무엇으로 압축하는가? |
| 6 | Scene Object | 지속 object, 상태 component, panel, Web handoff를 어떻게 나누는가? |
| 7 | NPC Behavior | 어떤 canonical task가 어느 semantic route와 arrival animation을 만드는가? |
| 8 | Interaction | 관찰, 선택, preview, 확인, Command, 재조회를 어디까지 허용하는가? |
| 9 | State Change | 어떤 server 상태 변화가 어떤 object 추가·갱신·제거를 만드는가? |
| 10 | Feedback | loading, stale, conflict, 실패, 완료와 불확실성을 어떻게 보이는가? |
| 11 | Runtime Proof | server test, Unity core test, Scene wiring, 실제 API, NavMesh/Animator 중 무엇을 검증했는가? |
| 12 | Game Candidate | 현실의 어떤 선택·제약·병목이 발견됐으며 아직 추정인 것은 무엇인가? |

### 5.1 설계 산출물

Zone 심화 slice마다 최소 다음 산출물을 남긴다.

- 현실 업무 흐름과 canonical source 표
- 권한별 정보 공개 표
- World Snapshot contract와 stable-ID 관계
- Scene object tree와 semantic waypoint 목록
- 상태→시각·NPC·animation mapping
- interaction 폐루프와 실패 상태
- 검증 matrix
- 게임성 후보와 보류 근거

## 6. 1순위: 창고 Zone 심화 설계

### 6.1 현실 업무

```text
입고 예정·차량 도착
  → Dock 인계
  → 수량·상태 확인
  → 입고상품 생성·가용/불량 수량 반영
  → 보관 위치 지정·적재
  → 재고 보관·예약
  → 피킹 작업
  → 포장·출고 묶음
  → 출고 대기
  → 운송 인계
```

현재 서버가 이 흐름 전체를 하나의 Unity projection으로 제공하는 것은 아니다. 기존 `WarehouseWorldSnapshot`은 권한이 적용된 **재고, 대기 적재, 피킹과 파생 NPC**를 결합한다. 입고 차량 도착, 검수 과정, 포장, 출고 묶음과 Dock 점유는 다음 slice에서 canonical source를 추가 결합해야 한다.

### 6.2 Actor와 관점

| Actor | 중요한 정보 | 가능한 행동 | Unity에서 제외할 정보 |
| --- | --- | --- | --- |
| WarehouseManager | 전체 재고, 미배정 위치, 작업 대기열, 병목 | 작업 확인·배정·운영 상세 진입 | 불필요한 개인 연락처와 결제·정산 원문 |
| DockWorker | 현재 배정 입고·적재 대상과 Dock | 수량 확인, 적재 시작·완료 요청 | 다른 창고와 미배정 작업자 정보 |
| Picker | 배정 피킹 순서, 위치, 수량, outbound staging | 피킹 시작·완료 요청 | 주문자 주소·연락처·결제 정보 |
| Transporter | 현재 배정된 하차/상차 대상과 Dock | 도착·하차·인계 확인 요청 | 창고 전체 재고와 타 운송 업무 |
| Shipper/Seller | 본인 소유 재고와 출고 준비 상태 | 조회, 필요한 Web 업무로 인계 | 다른 소유자의 재고·작업 |

역할은 Unity가 `if role`로 원본 목록을 숨겨 만드는 것이 아니다. 서버 UseCase가 인증·창고 접근·업무 배정을 확인한 최소 projection을 반환한다.

### 6.3 현재 canonical source와 gap

| 현실 개념 | 현재 source | 현재 Unity 사용 | gap |
| --- | --- | --- | --- |
| 창고 접근 | `창고`, `창고사용자`와 기존 권한 UseCase | `warehouseId` 기반 authorized snapshot | 활성 역할과 authorization decision을 snapshot에 명시할 필요 |
| 입고 요청 | `입고요청` | 화물 인계 workflow에서 일부 연결 | Warehouse snapshot에는 도착 예정·Dock 상태가 없음 |
| 입고 상품·재고 | `입고상품`, `재고이력`, `재고현황UseCase` | `InventoryItem` object | 물리 pallet/lot 수와 1:1이라고 주장할 수 없음 |
| 보관 위치 | `입고상품.보관위치` | `StorageLocation`, pallet label | 위치 code→Scene socket catalog와 rack capacity source가 없음 |
| 적재 | `적재작업UseCase` | `PutAway` task와 DockWorker NPC | 시작·완료 Command 후 재조회와 object 이동 animation 미연결 |
| 피킹 | `피킹포장작업`, `피킹작업UseCase` | `Picking` task와 Picker NPC | 포장 단계·선후 작업·outbound batch 연결이 snapshot에서 축약됨 |
| 포장 | `피킹포장작업`의 포장 유형, `포장작업UseCase` | 미표현 | Package/Bundle object와 packing station 필요 |
| 출고 | `출고예정`, `출고묶음`, `출고인계준비UseCase` | handoff 일부만 표현 | outbound staging queue와 transport handoff relation 필요 |
| 재고 이동 | `재고이동` | 직접 표현하지 않음 | object 이동 이력·원인 panel과 revision relation 필요 |
| Dock·Rack 물리 용량 | canonical source 없음 | primitive waypoint만 존재 | 운영 값처럼 표현 금지; layout config 또는 별도 aggregate 필요 |

### 6.4 World Snapshot 목표 구조

아래는 다음 심화 contract의 목표다. 기존 contract를 한 번에 교체하지 않고 additive version 또는 별도 detail projection으로 확장한다.

```text
WarehouseZoneSnapshot
├─ StableId / Revision / GeneratedAt / ViewerScope
├─ WarehouseSummary
│  ├─ available / reserved / unassigned
│  └─ queue counts by canonical state
├─ InventoryLots[]
│  ├─ stable ID / product / SKU / quantity
│  ├─ storage location code
│  └─ source inbound item / freshness
├─ WorkTasks[]
│  ├─ PutAway / Picking / Packing / OutboundHandoff
│  ├─ source object relation
│  ├─ status / can execute / expected revision
│  └─ current / destination semantic location
├─ CargoHandoffs[]
│  ├─ inbound or outbound task relation
│  └─ transport relation and state
└─ NpcMovements[]
   ├─ NPC stable ID / canonical task stable ID
   ├─ current / destination waypoint
   └─ movement / arrival action
```

`InventoryLot`은 실제 lot 또는 pallet aggregate가 서버에 없으면 `InventoryItemProjection`으로 명명한다. Unity에 상자 모양으로 표시하더라도 물리 pallet 한 개라고 설명하지 않는다.

### 6.5 Scene 구조

```text
WarehouseZone
├─ WorldRoot
│  ├─ InboundDock
│  ├─ InspectionArea
│  ├─ StorageZone
│  │  └─ RackSocket[]
│  ├─ PickingAisle
│  ├─ PackingStation
│  ├─ OutboundStaging
│  └─ VehicleExit
├─ ObjectRoot
│  ├─ InventoryItemView[]
│  ├─ WorkTaskMarkerView[]
│  ├─ PackageOrBundleView[]
│  └─ CargoHandoffView[]
├─ NpcRoot
│  ├─ DockWorkerView[]
│  ├─ PickerView[]
│  └─ PackingWorkerView[]
├─ RoleOverlayRoot
│  ├─ MyTaskHighlight
│  ├─ AllowedInteractionBadge
│  └─ QueueWarning
└─ PanelRoot
   ├─ WarehouseSummaryPanel
   ├─ InventoryDetailPanel
   ├─ TaskConfirmationPanel
   └─ ProvenanceAndHistoryPanel
```

Scene의 Transform은 `warehouse.inbound-dock`, `warehouse.inspection`, `warehouse.storage-zone`, `warehouse.rack-zone`, `warehouse.packing-station`, `warehouse.outbound-staging`, `warehouse.vehicle-exit` 같은 semantic key에 연결한다. server는 Unity 좌표를 보내지 않는다.

### 6.6 상태와 시각 표현

| Canonical 상태 | World object | NPC·경로 | feedback |
| --- | --- | --- | --- |
| 입고 예정 | Cargo marker를 Dock 밖에 표시 | Transporter가 inbound gate 대기 | 예정 시각과 source task 표시 |
| 창고 도착 | Cargo를 inbound Dock에 표시 | Transporter와 DockWorker가 Dock으로 이동 | 하차·인계 가능 여부 badge |
| 적재 대기 | Inventory projection을 inspection/storage 경계에 표시 | DockWorker가 storage destination을 받음 | 위치 미배정이면 warning, 임의 rack 선택 금지 |
| 적재 진행 | object를 source와 destination 사이 이동 표현 | DockWorker 이동·carrying animation | server task는 아직 진행중으로 유지 |
| 보관중 | object를 location socket에 고정 | worker는 다음 task 또는 대기 | available/reserved 수량 분리 |
| 피킹 대기 | 대상 location과 outbound staging 연결 강조 | Picker가 rack zone으로 이동 | 작업 우선순위는 server 값만 표현 |
| 피킹 진행 | quantity 변화 preview를 별도 표시 | Picker가 outbound staging으로 이동 | 확정 수량은 Command 성공 전 변경 금지 |
| 포장·출고 대기 | Package/Bundle을 staging에 표시 | PackingWorker 또는 DockWorker 이동 | 운송 task relation과 준비 상태 표시 |
| 인계 완료 | staging object 제거 또는 transport cargo로 전환 | Transporter가 vehicle exit로 이동 | canonical 재조회 뒤 전환 |
| stale / refresh 실패 | 마지막 성공 object 유지 | 새로운 route 적용 중지 | 데이터 기준시각·stale banner 표시 |

### 6.7 Interaction 폐루프

첫 심화에서는 관찰과 상세 조회를 먼저 완성한다.

```text
object 선택
  → 권한 적용 DetailPanel
  → 현재 상태 / 수량 / 위치 / source task / 기준시각 확인
```

실행 기능을 연결할 때는 공통 폐루프를 지킨다.

```text
task 선택
  → server projection의 CanExecute와 expected revision 확인
  → 영향 preview와 확인 panel
  → 기존 server UseCase / Command 호출
  → 권한·현재 상태·revision 재검증
  → 성공 시 WarehouseZoneSnapshot 재조회
  → stable-ID reconcile
  → object / NPC / panel 갱신
```

NavMesh 도착, animation event, drag-and-drop과 object collision은 적재·피킹 완료의 권위가 아니다.

### 6.8 창고 세부 구현 순서

#### W1. 현재 snapshot을 공간적으로 정직하게 표현

- `StorageLocation` code→Scene Transform catalog를 추가한다.
- 위치가 없거나 알 수 없는 object는 `UnassignedArea`에 둔다.
- inventory projection을 물리 pallet 개수로 오인하지 않도록 label과 detail panel을 분리한다.
- task와 inventory relation, NPC와 source task relation을 선택 시 함께 강조한다.
- 실제 API에서 loading, last-success refresh error와 stable-ID reconcile을 검증한다.

구현 상태(2026-08-08): 위치 catalog, `UnassignedArea`, 명시적 재고↔적재 작업↔NPC 선택 관계, highlight와 DetailPanel까지 코드·Unity 6 Scene 배선 검증을 마쳤다. 피킹 목록에는 재고 원본 ID가 없으므로 SKU 일치만으로 관계를 만들지 않는다. 실제 서버 snapshot을 UnityWebRequest로 두 번 조회한 refresh와 단절 후 마지막 성공 snapshot 유지도 Unity 6 EditMode에서 검증했다. Game View 클릭 확인은 남아 있다.

완료 기준: 사용자가 하나의 재고 object를 선택하면 현재 수량·예약 수량·보관 위치·연결 적재/피킹 task와 NPC를 추적할 수 있다.

#### W2. 입고 Dock 인계

- 기존 `warehouse-handoff`와 `입고요청`을 Warehouse detail projection에 결합한다.
- 차량·화물·Transporter·DockWorker가 같은 canonical handoff를 참조한다.
- 도착 전, Dock 도착, 입고 완료를 서로 다른 공간 점유로 표현한다.
- 주소·연락처·운임과 불필요한 주문 식별자는 제외한다.

완료 기준: 하나의 실제 handoff 상태가 도심 물류센터, transport corridor와 창고에서 같은 task relation으로 수렴한다.

구현 상태(2026-08-08): 기존 Warehouse authorized snapshot에 권한 필터된 `InboundHandoffs`를 additive contract로 포함하고, 적재 작업에는 `inbound-task:{입고요청Id}` canonical 참조를 추가했다. 기사 관점과 창고 관점은 공통 `CargoWarehouseHandoffProjectionBuilder`를 사용하므로 상태·차량·화물·NPC stable ID가 갈라지지 않는다. Unity는 이를 `Approach → InboundDock → StorageZone/VehicleExit` 공간 점유로 해석하고 차량·화물·Transporter·DockWorker 선택 관계를 같은 canonical relation으로 결합한다. 서버 접근 필터, Unity headless 101건, Unity 6 scene wiring과 VContainer EditMode 2건을 검증했다. 활성 handoff가 존재하는 실제 운영 DB 응답의 Game View 확인은 남아 있다.

#### W3. 적재 작업 폐루프

- 위치 미배정과 위치 지정 상태를 분리한다.
- 적재 시작/완료에 기존 UseCase를 재사용하고 expected revision을 추가 검토한다.
- 성공 전에는 preview 이동, 성공 후에는 canonical 위치 이동으로 구분한다.
- 중복 실행, 권한 상실과 stale 충돌을 panel에서 설명한다.

완료 기준: DockWorker animation이 아니라 server Command 성공과 재조회가 object의 보관 위치를 바꾼다.

#### W4. 피킹·포장·출고 연결

- 피킹과 포장을 별도 task kind와 선후 relation으로 표현한다.
- outbound bundle/package stable ID를 정의한다.
- 예약 수량, 피킹 수량과 실제 출고 수량의 차이를 보존한다.
- outbound staging에서 운송 인계로 전환되는 relation을 연결한다.

완료 기준: 재고 object에서 출발한 수량이 picking→packing→outbound handoff로 추적되며 중복 차감되지 않는다.

#### W5. 운영 관찰과 게임성 후보 수집

- task별 대기시간, 이동 구간, 미배정 위치, staging 체류와 queue 길이를 서버 또는 검증된 projection에서 관찰한다.
- 자동 점수나 경쟁 순위를 만들지 않고 병목과 선택 근거를 설명한다.
- layout simulation은 운영 상태와 분리된 scenario에서 수행한다.

게임 후보는 다음 질문에 실제 데이터가 답할 수 있을 때만 제안한다.

- 위치 선택에 따라 실제 또는 simulation 이동거리가 달라지는가?
- 긴급도·유통기한·출고 시간창처럼 서버가 권위 있게 제공하는 우선순위가 있는가?
- Dock, worker, rack 또는 staging capacity가 canonical 또는 승인된 scenario input으로 존재하는가?
- 개인 작업자를 감시·서열화하지 않고 팀 단위 흐름 개선으로 피드백할 수 있는가?

## 7. 나머지 Zone 심화 방향

### 7.1 도심 물류센터

```text
운송 도착 → 하차 → 입고 인계 → 분류/보관 목적지 → 상차 → 다음 거점 출발
```

- 현재 source: `운송원장`, `운송이벤트`, 현재 기사 배정, 상·하차 관점, warehouse handoff.
- World object: gate, inbound/outbound Dock, cargo, truck, loading bay, route board.
- Actor: Transporter, DockWorker, 물류센터 운영자, Shipper.
- 먼저 깊게 할 것: 한 cargo가 운송중→도착→창고 인계로 바뀔 때 TruckView·CargoView·두 NPC와 역할 panel이 같은 revision에 수렴하게 한다.
- canonical gap: Dock queue, 분류 line, 차량 bay capacity와 작업자 배정 원장은 현재 확인되지 않았다. fixture 값을 운영 값처럼 표현하지 않는다.
- 게임 후보: Dock 혼잡, 차량 도착 시간창, 상하차 순서와 거점 간 연결. 자동 배차·운임·계약 확정은 운영 준비 전 제외한다.

### 7.2 농장

```text
환경 관측 → 서버 판정 → 작업 필요성 → 농장작업 → 재배 상태 변화 → 재관측
```

- 현재 source: `농장`, `농장구획`, `재배작기`, `농업센서`, `농업센서관측`, `농장작업`, 공개 농사로 작물 기준.
- World object: FarmTile, CropView, SensorView, FarmWorker, 작업 marker와 근거 panel.
- 먼저 깊게 할 것: 실제 sensor ingestion과 승인 rule이 `ConditionCode`, freshness, rule revision과 evidence card를 생성하고 같은 snapshot에서 작물·센서·작업 강조를 갱신하게 한다.
- canonical gap: sensor 수집 adapter, 보정·설치 정보, 재배 규칙 실행, 운영 seed와 실제 DB migration 적용이 남아 있다.
- interaction: 관측·근거 확인을 먼저 제공하고 작업 시작/완료는 canonical `농장작업` Command가 생긴 뒤 연결한다.
- 게임 후보: 센서 배치·최신성, 작업 우선순위, 관수 판단과 생육 단계별 관찰. raw value만으로 Unity가 건조·과습을 판정하지 않는다.

### 7.3 도심마트

```text
판매 가능 상품 → 진열 → 가격·재고 기준시각 → 주문 수요 → 보충 또는 품절
```

- 현재 source: 주문자용 공개 마트 상품 aggregate의 판매가·판매 가능 수량·재고 기준시각. 이 Projection은 내부 운영 재고가 아니다.
- World object: Shelf, ProductCrate, PriceTag, StockBadge, Kiosk와 detail panel.
- 먼저 깊게 할 것: [Unity 도심마트 운영자 3계층 재정비 설계](UrbanMarketOperatorDataInterpretationPresentationRedesign.md)에 따라 공개 판매정보와 관리자 운영정보를 분리하고, 첫 업무를 `진열 보충`으로 한정한다. 상품·위치별 재고·진열대·작업을 Shared World graph로 구성한 뒤 모든 진열대의 작업 할당을 반영한 `OnHand / Allocated / Available`과 다중 원천 계획을 먼저 확정한다. 관리자 Perspective는 그 무결성 검증을 통과한 상태만 긴급·대기·진행·판단 불가 queue로 만들고 Presentation이 강조와 NPC 표현을 담당한다.
- canonical gap: 진열 위치·진열 수량·보충 task와 매장 worker 업무 source가 없다. 공개 가용 수량을 선반 위 물리 상자 수로 그대로 해석하지 않는다.
- interaction: 주문자는 탐색·상세·출처 확인부터 시작한다. 주문은 확인 panel과 기존 server 주문 UseCase 뒤 canonical 주문 재조회가 필요하다.
- 게임 후보: 진열·보충, 품절 대응, 수요와 재고 균형. 가격 조작이나 실제 주문을 simulation 행동으로 발생시키지 않는다.

### 7.4 주거공동체 수령

```text
운송 도착 → 공동수령지 하차 → 수령 대기 → 권한 있는 사용자 확인 → 완료
```

- 현재 source: 기존 unloading perspective의 주문자 본인·배정 운송자 필터.
- World object: PickupPoint, delivery package projection, arrival marker, role badge와 proof panel.
- 먼저 깊게 할 것: 같은 object를 주문자에게는 `내 수령 상품`, 운송자에게는 `내 하차 대상`으로 유지하면서 도착·대기·완료 상태를 canonical 재조회로 갱신한다.
- 개인정보: 다른 세대, 상세 주소, 연락처, 주문번호와 증빙 원문은 Unity aggregate에서 제외한다.
- canonical gap: 수령 확인·배송 증빙 Command와 Unity interaction 폐루프는 미연결이다.
- 게임 후보: 마지막 구간 도착 순서, 공동수령지 체류와 분배 협력. 개인의 수령 지연을 공개 점수화하지 않는다.

### 7.5 전통시장·공개 물류거점

```text
시장 탐색 → 상점·품목 발견 → 공개 물류거점 확인 → 집하·공동배송 가능성 검토
```

- 현재 source: Traditional Market catalog, logistics hub, 공공 출처와 위치 정밀도, 생활권 협의.
- World object: 시장 건물, 상점 cluster, 집하 Dock, pickup marker와 provenance panel.
- 먼저 깊게 할 것: 공개 marker와 operational 집하 task를 서로 다른 source type과 View layer로 겹친다.
- canonical gap: 상점별 실시간 재고, 집하 화물, 공동배송 task와 상인 NPC 업무는 공개 catalog에서 추론할 수 없다.
- interaction: 공개 탐색·정보 확인과 참여 의향을 분리하고 계약·배차는 Web/서버 권한 경계로 넘긴다.
- 게임 후보: 여러 상점의 집하 시간창, 공동배송과 거점 활용. 공개 위치만으로 자동 참여자 선택을 하지 않는다.

### 7.6 공공데이터 정보관

```text
관측 발견 → 출처·기준시각 확인 → 지역·시점·품목 비교 → 관련 World Zone 탐색
```

- 현재 source: 공개 세계지도 observation, layer, 위치 정밀도, freshness와 provenance.
- World object: marker, layer table, time board, comparison panel와 source kiosk.
- 먼저 깊게 할 것: 동일 품목·지역의 여러 시점을 비교하고 stale·누락·출처 차이를 공간과 panel에서 명확히 표시한다.
- interaction: filter, compare, 근거 열기와 관련 Zone portal. 공공 가격을 판매 제안·재고·계약 의사로 해석하지 않는다.
- 게임 후보: 탐색, 패턴 발견, 출처 신뢰도 학습과 데이터 갱신 이해. 정답 맞히기보다 불확실성을 읽는 경험을 우선한다.

### 7.7 커뮤니티·시장 광장

```text
공개 정보·이야기 발견 → 관심 → 대화·참여 의향 → 별도 동의 → 공동행동 검토
```

- 현재 source: 공개 게시판, 게시글 요약, 비식별 활동 신호와 권한 적용 원장 요약.
- World object: CommunityBoard, post summary, activity signal, LedgerBoard, meeting portal과 detail panel.
- 먼저 깊게 할 것: 공개 관찰, 관심, 참여 의향과 실제 실행을 서로 다른 상태·색·interaction으로 표현한다.
- 개인정보: 작성자 식별자, 연락처, 댓글 본문, 담당자, 내부 원장 ID와 실행 권한을 공개 aggregate에 넣지 않는다.
- canonical gap: Unity에서 관심·참여 의향을 변경하는 Command 폐루프와 역할별 상세 projection이 미연결이다.
- 게임 후보: 공동 목표 발견, 역할 분담과 합의 과정. 인기 경쟁, 신뢰 점수화와 민감 특성 기반 추천을 만들지 않는다.

## 8. P8 협동조합·공동원장 Zone 생성 조건

다음 조건을 충족하기 전에는 P8 Scene을 만들지 않는다.

1. 두 개 이상의 기존 Zone이 같은 community/cooperative ledger stable ID를 참조한다.
2. 참여 의향, draft ledger와 실제 실행 ledger 상태가 서버에서 분리되어 있다.
3. 구성원·역할·열람 범위와 연락처 공개 동의를 서버가 검증한다.
4. 비용, 노동, 위험, 담당 역할과 결정 근거를 함께 투영할 수 있다.
5. 투표·결정·작업·비용의 history가 재처리 가능한 canonical event 또는 ledger에 남는다.

조건을 충족하면 P8은 앞 Zone을 대체하는 관리 화면이 아니라 관계를 설명하는 공간이 된다.

```text
농장 생산 object
  ├─ 시장 판매·주문 relation
  ├─ 물류·창고 task relation
  ├─ 공동수령 relation
  └─ Cooperative Ledger
      ├─ 참여와 역할
      ├─ 의사결정
      ├─ 비용·노동·위험
      └─ 결과와 회고
```

## 9. 실행 로드맵

### 단계 A. Warehouse W1

- 현재 snapshot과 Scene location catalog를 정직하게 연결한다.
- inventory↔task↔NPC relation 선택 강조와 detail panel을 완성한다.
- 실제 API refresh와 last-success 정책을 runtime에서 검증한다.

### 단계 B. Warehouse W2~W3

- cargo handoff와 입고 Dock을 결합한다.
- 적재 Command와 canonical 재조회 폐루프를 연결한다.

### 단계 C. Warehouse W4와 Logistics Center

- picking→packing→outbound handoff를 연결한다.
- 같은 cargo/task relation이 창고와 물류센터·transport corridor에서 수렴하게 한다.

### 단계 D. Farm과 Urban Market

- 실제 sensor ingestion→판정→농장작업 표현을 연결한다.
- 생산·창고·마트 사이 상품/재고 relation을 공개 범위에 맞게 연결한다.

### 단계 E. Residential·Traditional Market

- 마지막 구간의 권한 있는 완료 폐루프를 검증한다.
- 공개 시장과 operational 집하 layer를 분리해 결합한다.

### 단계 F. Public Data·Community

- 비교·근거 탐색과 관심·참여 의향 경험을 깊게 한다.
- 앞 Zone에서 쌓인 공동 원장 요구로 P8 생성 여부를 판단한다.

## 10. Zone 심화 완료 기준

Zone 하나를 심화 완료로 표시하려면 다음을 모두 기록한다.

- 실제 업무 한 흐름의 시작·진행·완료와 예외가 canonical source에 연결됨
- 권한별 server projection과 정보 최소화 test
- stable ID relation과 revision/freshness validation
- Unity Repository·UseCase·Controller·View의 역할 분리
- object·NPC·semantic waypoint와 상태 mapping
- initial load, refresh error, stale, conflict와 권한 실패 표현
- 실행 기능이 있으면 확인→Command→재조회 폐루프
- Unity 6 compile과 Scene reload wiring
- 실제 API runtime 여부, NavMesh bake·Animator·PlayMode 여부를 별도 보고
- 게임성 후보가 현실 근거와 simulation/operational 경계를 설명함

이 기준을 통과하기 전에는 외형의 완성도나 미니게임 수로 Zone 완성을 판단하지 않는다.
