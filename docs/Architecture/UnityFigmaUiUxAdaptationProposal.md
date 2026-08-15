# 기존 Figma UI/UX의 Unity World 적용 제안

## 1. 제안 목적과 상태

Ssalddel에는 `01~09` 역할별 모바일 화면, 역할 간 상태 흐름, 주문·운송·창고·공동행동의 검토 UI가 Figma와 실제 MAUI 구현 기록으로 축적되어 있다. 이 자산을 현재 Unity World Projection에 활용하면 Unity가 새 UI 문법을 다시 발명하지 않고도 다음 강점을 이어받을 수 있다.

- 역할별로 무엇을 먼저 보여 주는지 정리된 정보 우선순위
- `정상·주의·차단·재시도·예시 데이터`를 구분하는 상태 표현
- 카드, 상태 chip, 근거 행, 하단 CTA로 이어지는 판단 흐름
- 하나의 stable ID를 여러 역할이 다시 조회하는 폐쇄 루프
- 비교·Preview·별도 동의·실행을 섞지 않는 UX 경계

다만 적용 단위는 Web route나 모바일 화면의 3D 복제가 아니다. Figma는 **사용자 여정·정보 계층·표현 토큰의 근거**, 서버와 Simulation은 **상태와 실행의 권위**, Unity는 **공간 탐색·선택·상황 인식·Preview와 확인의 Presentation**을 맡는다.

이 문서는 제안서다. Figma node, Unity prefab·Scene·코드, 서버 계약을 이번 문서 작업에서 변경하지 않았다. 현재 Figma 파일의 live node 상태도 다시 검사하지 않았으며, 아래 자산 현황은 저장소에 보존된 실제 확인 기록을 기준으로 한다.

## 2. 현재 근거

### 2.1 Figma·MAUI에 이미 있는 것

| 자산 | 검증된 설계 의미 | Unity에서의 우선 활용 |
| --- | --- | --- |
| `01 Community` | 생활/업무 탐색, 게시판 홈과 전체 디렉터리의 분리, 개인정보 안내 | Town·Community Market의 발견 지점, 공개 정보 board, 긴 글로 이동하는 진입점 |
| `02 Orderer` | 상품 탐색, KAMIS 비교, 개별 주문/같이 주문 비교, 의향·확정·별도 동의, 원장 | Farm 상품·가격 카드, 수확 판로 Preview, Town 수요와 City 판매 연결 |
| `03 Shipper` | 운송의뢰 초안, 화물 조건, 입고·판매 인계, 계약 전 실행 차단 | Cargo 선택 상세, Farm/Hub/City handoff, 운송 Preview |
| `04 Driver` | 추천·만료·재연결, 현재 운송, 상차·하차·POD, 정산 근거 | 차량/NPC 상호작용, 경로 상태 HUD, 허용된 운송 Task |
| `05 Warehouse` | 입고 예정, 작업 보드, 검수·재고·피킹·출고, 하차지 오류 차단 | Hub Dock·검수대·보관 Zone·출고 Gate의 공간 카드 |
| 역할 간 Flow | 같은 주문·운송 ID의 Command 뒤 역할별 canonical 재조회 | 한 World 변화가 Farm·Hub·City surface에 다시 반영되는 상태 전이 설명 |
| Seller/Admin | 수요·재고·상품·운영 개요와 권한·재시도 | City Market 관리자 desk와 후속 Settlement 운영 시점 |

보존된 주요 설계 근거는 다음과 같다.

- Figma 파일 `0KhuQLc1MleUBIQnARC21Z`
- `02 Orderer` 페이지 `2030:65`와 검증된 `02A → 02B → 02C → 02D → 02E → 02G` 흐름
- 주문 방식 비교 화면 `2233:177`
- 음식 주문 역할 간 상태 화면 `2269:177`
- 서버·클라이언트 수렴 구역의 Orderer `2425:301`, Shipper `2438:64`, Driver `2438:455`, Warehouse `2438:791`
- 실제 MAUI 렌더로 확인한 Orderer, Shipper, Driver, Warehouse의 역할별 Shell과 대표 상태 화면

### 2.2 현재 Unity에 이미 있는 것

Unity는 다음 기반을 이미 갖고 있다.

- Farm·Town·Hub·City의 3/4 시점 World와 Region/Hub Journey
- stable ID, revision, source lineage, `Simulation`/`Operational` 구분
- `ConceptCardDeckPresentationModel`과 Concept·Status·Reason·Action 카드 문법
- 감자 Farm 상품·공공가격 카드와 PVS0~PVS6 read projection
- 밭갈이 `선택 → Preview → 명시적 Confirm → Simulation Tick → 재조회` 폐루프
- 수확 판로 선택, 조합 인수, 생산자 직판 준비, 포장·Cargo·Hub 입고·Lot 분리의 좁은 slice
- Warehouse·Urban Market sample의 View, selection, last-success와 Game View 기준선

따라서 새 UI 체계를 별도로 만들기보다, Figma에서 검증된 UX를 현재 `Concept Card + World Anchor + Preview/Confirm` 구조에 접합하는 편이 적절하다.

## 3. 적용 원칙

### 3.1 재사용하는 것

1. **정보 계층**: 제목, 핵심 값, 보조 설명, 근거, 제한, 다음 행동의 순서
2. **상태 문법**: loading, empty, stale, error, blocked, preview, confirmed, simulation
3. **역할 문법**: 역할 accent와 업무 진입점은 사용자의 현재 Perspective를 설명하는 보조 신호로 사용
4. **비교 문법**: 비용·시간·수량·위험·출처를 같은 기준으로 나란히 표시
5. **폐쇄 루프**: 행동 성공 뒤 같은 stable ID와 더 높은 revision을 다시 읽어 모든 surface를 갱신
6. **접근성 원칙**: 색만으로 상태를 전달하지 않고 icon·label·shape·문구를 함께 사용

### 3.2 그대로 옮기지 않는 것

- `393×852` 모바일 화면 전체를 World Space Canvas로 붙이지 않는다.
- AppBar·drawer·bottom navigation을 Zone마다 복제하지 않는다.
- Figma frame 번호를 Scene, prefab, stable ID 또는 업무 계약으로 사용하지 않는다.
- 화면에 있던 예시 수치를 operational data처럼 표시하지 않는다.
- Unity가 역할 accent를 근거로 권한을 추론하지 않는다.
- 긴 입력, 대형 표, 주소·계좌·증빙·계약 서명과 관리자 대량 작업을 Unity 안에서 재구현하지 않는다.

## 4. Figma 문법을 Unity 문법으로 변환

| Figma·MAUI 요소 | Unity 적용 형태 | 책임 경계 |
| --- | --- | --- |
| AppBar의 역할·화면 제목 | 화면 상단의 얇은 `Perspective / Zone / Mode` Context HUD | 서버가 허용한 Perspective만 표시 |
| 핵심 지표 카드 | 선택 대상의 Status Card 또는 Zone Summary | Projector가 문구와 값 결정 |
| 상태 chip | `SIMULATION`, `STALE`, `PREVIEW`, `BLOCKED`, source freshness badge | 색과 문구를 함께 사용 |
| 카드 왼쪽 accent | 카드 종류·주의 수준의 asset-neutral visual token | 역할 색과 업무 상태 색을 분리 |
| bottom navigation | World Map, 현재 Task, Ledger/History의 3개 전역 진입 | Web route 목록을 복제하지 않음 |
| 하단 CTA | 선택 대상 근처의 Action Card와 화면 하단 Confirm Dock | 먼저 Preview, 이후 별도 Confirm |
| 화면 간 화살표 | World path, Task stepper, Cargo route highlight | 이동 animation이 상태 성공을 의미하지 않음 |
| 비교 화면 | 2열 또는 전환형 Compare Overlay | 단위·통화·기준 시각·source 보존 |
| 오류·재시도 카드 | last-success world를 유지하는 Refresh Banner | fixture 성공으로 대체하지 않음 |
| drawer의 전체 업무 | 현재 허용 Interaction을 보여 주는 Task Journal | 숨은 권한이나 미구현 기능을 만들지 않음 |
| 긴 입력·표·증빙 | `Web에서 자세히 검토` handoff | 서버 발급 context ID만 전달하고 민감정보를 World에 노출하지 않음 |

## 5. 권장 Unity UI 구조

```text
World View
├─ Context HUD
│  └─ Perspective / Zone / Operational-or-Simulation / freshness
├─ Selection Reticle
│  └─ Farm crop / Cargo / NPC / Dock / Shelf / Ledger anchor
├─ Concept Card Deck
│  ├─ Concept
│  ├─ Status
│  ├─ Reason
│  └─ Action
├─ Compare Overlay
│  └─ source, unit, 기준 시각이 보존된 두 선택지
├─ Confirm Dock
│  └─ Preview summary / cautions / Confirm / Cancel
├─ Task Journal
│  └─ 현재 허용된 단계와 block reason
└─ Web Handoff
   └─ 긴 입력·민감 업무·전문가 검토
```

기본 화면은 World를 가리지 않는 낮은 밀도로 유지한다. 상세 카드는 선택할 때만 열고, Confirm Dock은 실행 가능 Action을 Preview한 뒤에만 나타나게 한다. World Space label은 가까운 대상의 짧은 상태에 한정하고, 긴 설명과 근거 행은 Screen Space overlay로 읽게 한다.

## 6. 첫 적용 vertical slice

### `FUX1 · 감자 한 상자의 Farm → Hub → City UX`

현재 코드와 Scene에 가장 잘 맞는 첫 대상은 새로운 기능이 아니라 이미 존재하는 감자 한 상자의 여정을 하나의 UX 문법으로 잇는 것이다.

```text
Farm 감자/수확물 선택
  → 02 Orderer 기반 상품·가격·판로 Compare
  → 판로 Action Preview와 Confirm
  → 03 Shipper 기반 Cargo handoff 상태
  → 05 Warehouse 기반 Hub 입고·검수·보관·분할
  → 04 Driver 기반 허용된 이동 Task와 경로 상태
  → City 판매/공급 상태 재조회
```

#### Farm

- `02.01`의 개별/같이 주문 비교 문법을 `조합 인수 / 생산자 직판 / 보관` 판로 비교에 재사용한다.
- 금액만 강조하지 않고 수량, 노동, 처리 시간, capacity, 감모·위험, 근거 source를 함께 보여 준다.
- 추천이나 기본 선택을 만들지 않고 각 선택의 block reason과 Simulation 영향을 Preview한다.

#### Hub

- `05 Warehouse`의 `홈·입고·작업·출고` 계층을 Dock, Inspection, Storage, Outbound 네 공간 anchor에 대응한다.
- 선택한 Cargo stable ID를 유지하면서 예상 입고, 검수 결과, 보관 Lot, City outbound 후보를 순차적으로 표시한다.
- 하차지·수량·revision이 맞지 않으면 빨간 성공 연출 대신 차단 카드와 재조회 행동을 제공한다.

#### Cargo Journey

- `03 Shipper`의 초안/조건/인계와 `04 Driver`의 추천 만료/상차/하차 문법을 합쳐 Cargo Status Card를 만든다.
- 이동 경로는 공간에 표시하되 운송 확정은 canonical task와 revision을 기준으로 한다.
- 네트워크 갱신 실패 시 차량을 삭제하지 않고 마지막 성공 위치·상태를 stale로 표시한다.

#### City

- `02 Orderer`의 수요·가격 정보와 Seller/Admin의 재고·상품 요약 문법을 Concept Card로 투영한다.
- Farm/Hub에서 발생한 Simulation 결과와 실제 판매가능 operational projection을 합치지 않는다.
- 긴 상품 등록, 계약, 정산은 Web handoff로 남긴다.

## 7. 단계별 실행안

| Gate | 범위 | 산출물 | 완료 기준 |
| --- | --- | --- | --- |
| `FUX0` | 자산 inventory와 대응표 | Figma node/screenshot → Unity anchor/card/state 매핑 | live Figma 재확인, 숨김 참고본과 실제 사용 frame 구분 |
| `FUX1` | 공통 token과 Context HUD | 역할 accent, 상태 색, spacing, typography, mode/freshness badge | 색 외 label 포함, 1600×900과 목표 모바일 해상도 가독성 확인 |
| `FUX2` | Farm Compare·Confirm | 판로 비교 Overlay와 Preview/Confirm Dock | 기존 수확 판로 slice에 연결, 자동 선택 없음, Game View 확인 |
| `FUX3` | Hub Warehouse UX | Dock·검수·보관·출고 anchor와 Task Journal | 동일 Cargo ID/revision 유지, 오류·stale·blocked 상태 확인 |
| `FUX4` | Cargo Journey UX | Shipper/Driver 상태 카드와 route highlight | 이동 연출과 canonical task 성공 분리, reconnect 확인 |
| `FUX5` | City와 Web handoff | 판매/수요 요약, 상세 Web 진입 | 민감정보 미노출, context ID·return path와 권한 재검증 |
| `FUX6` | Figma 역동기화 | 전용 `Unity World UX` page와 component variants | Game View 기준 frame, 실제 구현과 차이 기록 |

`FUX0`에서 전용 Figma page를 먼저 새로 만드는 것은 피한다. 기존 node와 보존 PNG를 다시 확인하고 실제 Unity 화면에 필요한 component inventory를 확정한 뒤 `FUX1~FUX5`의 구현 결과를 `FUX6`에서 Figma 기준으로 정리하는 순서가 낭비가 적다.

### 2026-08-14 첫 적용 상태

- `FUX0`: Figma `05P1 Warehouse`의 역할 띠, 상태 배지, 요약·근거·다음 단계, 보조·주요 행동 문법을 재확인했다.
- `FUX1`: `figma-maui-warehouse.v1` 의미 프로필과 Unity Theme Catalog를 추가했다. 역할 색과 상태 색은 별도 축이다.
- `FUX3`: 진부면 물류 거점의 검수→적재 정보판을 `SimulationWorldShell`에 배치하고 Preview·Confirm·WorldTick·재조회 버튼을 실제 기능에 연결했다.
- 저장 Scene은 서버 기준이며 fixture는 EditMode·PlayMode와 Game View 증거 생성에서만 사용한다. stale·blocked의 자동화 검증과 Figma 역동기화는 후속 범위다.

## 8. 디자인 토큰 제안

역할 색과 상태 색은 다른 축으로 유지한다.

| 축 | 예시 | 용도 |
| --- | --- | --- |
| 역할 accent | Community neutral, Orderer violet/blue, Shipper blue, Driver teal, Warehouse orange | Perspective와 업무 영역 식별 |
| 상태 semantic | success, attention, blocked, stale, unavailable | 상태 의미 |
| 실행 mode | Simulation, Operational, Preview | 권위와 실행 경계 |
| source quality | Live, Cached, Fixture, Invalid, Failed | 데이터 provenance와 freshness |

예를 들어 Warehouse의 주황색은 `입고 완료`를 뜻하지 않는다. 완료는 별도의 icon·문구·semantic token으로 표현한다. `Simulation`도 역할 색과 무관한 고정 label과 패턴을 사용한다.

## 9. 검증 기준

### 구조·데이터

- DTO를 View에 직접 전달하지 않고 기존 Mapper·Interpretation·Projector를 통과한다.
- stable ID, revision, source lineage, unit, 기준 시각과 mode를 PresentationModel에 보존한다.
- refresh 실패 후 last-success와 stale 상태를 함께 유지한다.
- 역할 또는 authorization context가 바뀌면 기존 선택과 비공개 card를 제거한다.

### UX

- 선택 전, 선택 후, Preview, Confirm 대기, 성공 재조회, blocked, stale, error 상태를 각각 확인한다.
- 키보드/게임패드/터치 가운데 목표 platform 입력 경로를 명시한다.
- 카메라 이동 중 HUD가 World의 주요 시각 요소와 겹치지 않는지 확인한다.
- 한국어 긴 제목, 단위, 날짜와 source 문구의 잘림을 검사한다.
- 색각 차이와 작은 화면에서도 label과 icon으로 상태를 구분할 수 있어야 한다.

### 실행·시각 증거

- headless/core test, Unity EditMode, Play Mode interaction, Game View PNG를 서로 다른 검증 사실로 기록한다.
- UI가 달라지는 각 Gate는 최종 Play Mode Game View PNG와 `docs/Changes/` 기록을 남긴다.
- Figma는 live node metadata와 screenshot, Unity는 실제 Game View로 각각 확인한다.
- Figma mockup, Unity Scene View 또는 compile 성공만으로 완료 처리하지 않는다.

## 10. 위험과 대응

| 위험 | 대응 |
| --- | --- |
| 화면 수만큼 Unity panel이 늘어남 | route가 아니라 공통 card/state/action 문법을 구현 |
| 모바일 UI가 World를 가림 | Context HUD는 얇게, 상세는 선택 시 overlay, 긴 업무는 Web handoff |
| 역할 색이 상태나 권한으로 오해됨 | 역할·상태·mode token을 분리하고 label 병기 |
| 예시 데이터가 실데이터처럼 보임 | Fixture/Simulation badge와 source lineage를 항상 표시 |
| animation이 업무 완료처럼 보임 | Command·Tick 결과 재조회 뒤에만 confirmed 상태 적용 |
| Figma와 Unity가 다시 분기됨 | FUX6에서 구현 Game View를 기준으로 전용 Figma component를 역동기화 |
| 기존 dirty worktree와 시각 작업이 섞임 | Gate별 named path와 변경 기록을 분리하고 별도 검증 |

## 11. 권고 결론

기존 Figma UI/UX는 충분히 활용 가치가 있다. 특히 현재 Unity의 감자 Farm→Hub→City vertical slice와 `Concept Card` 구조는 Figma의 주문자 비교, 화주 인계, 기사 운송 상태, 창고 입출고 흐름을 받아들이기 좋은 단계다.

첫 구현은 새 메인 메뉴나 전체 역할 Shell이 아니라 다음 세 요소로 제한하는 것이 좋다.

1. Figma 상태 문법을 반영한 공통 Context HUD와 badge
2. Farm 판로 선택용 Compare Overlay와 Preview/Confirm Dock
3. 같은 Cargo stable ID를 따라가는 Hub 입고·검수·보관 Task Journal

이 세 가지가 닫히면 기존 Figma 자산은 단순한 과거 모바일 시안이 아니라 Web·MAUI·Unity가 같은 업무 의미를 공유하게 하는 UX 기준층이 된다.

## 관련 문서

- [Figma-MAUI 화면 호환성 정책](FigmaMauiCompatibilityPolicy.md)
- [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)
- [Unity Farm 상품·가격 카드 상호작용 흐름](UnityFarmProductPriceCardInteractionFlow.md)
- [Unity 서버 데이터 연계 미술 수직 슬라이스 모듈 제안](UnityServerDataLinkedArtVerticalSliceProposal.md)
- [Figma 서버·클라이언트 수렴](../Changes/2026-07-28-figma-code-convergence.md)
- [Figma 역할 화면 우선순위 정렬](../Changes/2026-07-30-role-priority-figma-alignment.md)
- [MAUI Orderer 02 Figma 근접 구현](../Changes/2026-07-24-maui-orderer-figma-02.md)
- [MAUI Warehouse 05 Figma 근접 구현](../Changes/2026-07-24-maui-warehouse-figma-05.md)
