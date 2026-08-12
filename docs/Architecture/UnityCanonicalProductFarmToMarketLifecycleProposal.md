# Unity canonical 품목별 Farm→Town·City 생애주기 제안서

> 상태: 부분 구현 — `CALENDAR-0 + FARM-3 + HARVEST-CHOICE-1 + COOP-1 + DIRECT-1 + CARGO-1 + WORLD-6 + JOURNEY-1 + HUB-1 + HUB-2/WORLD-8` 완료, 수출 후속 workflow·실제 공식 재배 달력·운영 저장과 Hub→City 출발 이후는 미구현
>
> 상위 구현 우선순위: [Unity 실시간 정착지 경제·영지 경영·분쟁 Simulation 재정렬 제안서](UnityRealtimeTerritoryManagementConflictSimulationProposal.md). 이 문서는 canonical 품목 생애주기 subsystem의 계약과 완료 상태를 유지하며, 전체 프로젝트의 다음 구현 순서는 상위 문서를 따른다.
>
> 제안일: 2026-08-10
>
> 상위 제품 방향: [Unity 생산·유통·협력 경험 플랫폼 종합 제안서](UnityCooperativeExperiencePlatformProposal.md)
>
> 권위와 World 투영 기준: [Unity World·원장 투영 아키텍처 제안서](UnityWorldLedgerProjectionArchitectureProposal.md)
>
> 상품 identity·asset 경계: [D-036·D-037](../AI/DECISIONS.md#d-036-공통-상품-stable-id와-출처별-품목코드를-분리한다), [POLYGON Farm 식품 asset·HS·가격 대응표](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)
>
> 이 문서의 책임: canonical 품목 하나가 파종·생육·수확·상차·이동·진열·판매로 이어지는 게임 경험과 단계별 구현 Gate를 제안한다. 실제 파종·수확 날짜, 품종별 재배법, 운영 거래 규칙을 확정하지 않는다.

## 1. 제안 요약

현재 기반은 서버의 canonical 식품 60개와 Unity `FarmProductVisualCatalog`의 `Direct 18 / Representative 10 / Unmapped 32` 분류까지 연결됐다. 다음 단계에서는 asset을 더 배치하는 데 그치지 않고, 각 품목이 다음 여정을 갖도록 구체화한다.

```text
품목을 고른다
  → 지역·재배 방식에 맞는 파종·정식·수확 가능 시기를 확인한다
  → 밭을 준비하고 명시적으로 파종·정식한다
  → 게임 달력과 작업을 따라 생육 단계를 경험한다
  → 수확해 품목별 lot를 만든다
  → 수확물과 상호작용해 조합 출하·직접 판매·수출대행 중 판로를 고른다
  → 선택한 판로에 맞춰 상자·pallet·차량 또는 상품 등록 준비로 분기한다
  → Farm에서 Town 또는 Hub·City·배송대행지로 이동한다
  → 검수·입고·진열한다
  → 주민·고객에게 판매된 결과를 원장에서 확인한다
```

핵심은 **같은 `CanonicalProductStableId`가 외형을 바꾸며 여행하되, 품목·수량·소유·상태는 GameObject나 prefab이 아니라 Data와 원장이 소유한다**는 점이다.

첫 완성 목표는 감자 한 품목의 `파종 → 수확 → 상차 → Farm→City 이동 → 마트 입고 → Simulation 판매` 폐루프다. 감자에서 계약과 표현 패턴을 닫은 뒤 양파·토마토·딸기·사과로 재배 유형을 넓히고, 이후 Direct 18개 전체로 확장한다.

## 2. 현재 출발점과 아직 없는 연결

### 2.1 2026-08-10 첫 구현 상태

`CALENDAR-0 + FARM-3`의 첫 headless vertical slice를 `Ssalddel.Unity`에 구현했다. 실제 농업 권고로 오인되지 않는 감자 `Fixture`를 사용하며, 재배 달력 profile은 지역·작형·파종/수확 window·source·quality·revision·limitation만 소유한다. 생육 단계별 Simulation 일수와 300kg 기준 수확량은 별도 `재배SimulationRuleSnapshot`에 둬 현실 기준정보와 게임 규칙을 분리했다.

기존 FARM-2의 6×6 토양 snapshot에서 `Tilled` 타일을 골라 파종 Preview/Confirm/Tick으로 `Sown`과 pinned calendar revision을 가진 재배작기를 만들고, 명시적 날짜 진행 Command로 `Emerged → Vegetative → Bulking → HarvestReady`를 결정적으로 계산한다. 수확도 Preview/Confirm/Tick을 통과한 뒤에만 `Harvested` 타일과 `product:potato → CultivationStableId → HarvestLotStableId`, 300kg, `kg`, source command lineage를 가진 Simulation 수확 Lot을 만든다.

Unity View Gate에서는 기존 감자 Farm vertical slice 위에 별도 `PotatoCultivationLifecycle` Scene과 presenter·builder를 추가했다. 왼쪽 lifecycle panel의 8개 명시적 action으로 파종 검토·확인·Tick·날짜 진행·수확 검토·확인·Tick을 실행하며, 생육 단계는 감자 식생 scale로 투영하고 `Harvested`에서는 밭 식생을 숨긴 뒤 기존 상자와 300kg HarvestLot marker를 표시한다. 오른쪽 PVS5 카드는 `POTATO IDENTITY · PRICE EVIDENCE / SERVER PRODUCT DATA · READ ONLY`로 경계를 명확히 해 재배 Command나 재고·판매 권위로 오인되지 않게 했다. Unity EditMode는 신규 FARM-3 4/4와 기존 PVS5 3/3을 통과했고, Play Mode `Harvested` revision 5 Game View PNG를 남겼다.

`HARVEST-CHOICE-1`은 수확 직후 농사짓는 사용자가 판로를 결정하는 첫 분기 Gate다. FARM-3의 동일 300kg HarvestLot marker와 상호작용하면 카드가 열리고, `생산자 조합에 출하`, `온라인 마켓 직접 판매`, `수출대행 준비`를 각각 Preview한 뒤 Confirm과 Simulation Tick으로 결정한다. 결과 원장은 HarvestLot stable-ID·수량·source lineage를 보존하고 선택에 따라 `CooperativeIntakeCandidate`, `ProducerPackingCandidate`, `ExportReadinessCandidate` 중 하나만 만든다. 이는 후속 업무 후보이므로 조합 인수·정산, 상품 등록·주문·결제·택배, 수출계약·검사·통관·운송은 발생하지 않는다. headless 집중 8/8, Unity EditMode 4/4, Unity core 전체 305/305와 Play Mode 직접판매 결정 Game View를 확인했다.

기존 `CARGO-1 → WORLD-6 → JOURNEY-1` 구현은 계속 유효하다. COOP-1 adapter가 `CooperativeIntakeCandidate`를 명시적으로 수락한 뒤 CARGO-1 포장 검토까지만 연결하며, 판로 선택만으로 PackageLot이나 Cargo를 만들지 않는다. 직접판매와 수출대행도 각자의 포장·표시·인계 규칙을 별도 Gate에서 닫는다.

`COOP-1`은 `CooperativeShipment` 결정만 입력으로 받아 조합 인수 Preview/Confirm/Tick을 수행한다. Tick 뒤 300kg `생산자조합인수LotSimulationData`와 `PotatoHarvestCargoLifecycle` 후속 후보를 만들며 HarvestLot·판로 결정·인수 Command lineage를 함께 보존한다. `CooperativeHarvestCargoAdapter`는 인수 승인 전 연결을 거부하고 승인 뒤에는 같은 HarvestLot과 조합 인수 Lot을 source로 가진 CARGO-1 초기 snapshot만 연다. 이 시점의 PackageLot과 Cargo는 null이므로 포장 Preview를 다시 명시적으로 수행해야 한다. 집중 headless 8/8, Unity EditMode 4/4, Unity core 전체 313/313과 Play Mode `AcceptedForPreparation / CARGO-1 PACKING PREVIEW READY` Game View를 확인했다.

`DIRECT-1`은 `DirectOnlineSale` 결정만 입력으로 받아 생산자 소포장 Preview/Confirm/Tick을 수행한다. Simulation Fixture의 5kg `ParcelBox` 규칙으로 300kg을 60개로 수량 보존하고 `OnlineMarketListingDraft` 후보를 만든다. 후보 이전에는 등록 초안을 열 수 없으며, adapter가 만든 초안도 `IsPublished=false`, `UnitPrice=null`, `OrderCount=0`이다. 따라서 상품 공개·주문·결제·택배 접수는 발생하지 않는다. 집중 headless 8/8, Unity EditMode 4/4, Unity core 전체 321/321과 Play Mode `PackedForListing / UNPUBLISHED / PRICE — / ORDERS 0` Game View를 확인했다.

`CARGO-1`은 이 HarvestLot을 입력으로 받는 별도 Simulation 원장을 추가했다. 포장 Preview/Confirm/Tick은 300kg을 20kg 상자 15개의 PackageLot으로 만들고, 상차 Preview/Confirm/Tick은 400kg 차량 용량 안에서 300kg Cargo를 만든다. 각 단계는 별도 stable-ID와 revision을 가지며 `product:potato → HarvestLot → PackageLot → Cargo` source lineage를 보존한다. 포장 전 상차, 수량 변조, 차량 용량 초과, stale Preview·Command는 거부한다. Unity `PotatoHarvestCargoLifecycle` Scene은 7개 명시적 action, Package·Cargo marker와 전체 lineage를 투영하며 headless 6/6, Unity EditMode 4/4, Unity core 전체 276/276과 Play Mode `Loaded` revision 3 Game View를 확인했다.

이 구현은 운영 Farm·Cargo 저장과 실제 농업·포장·운송 규칙을 추가하지 않았다. 달력 날짜·300kg 수확량·20kg 상자·400kg 차량 용량은 계속 `Simulation/Fixture`이며 운영 실패 fallback이 아니다.

`WORLD-6/PVS6 adapter`는 기존 Presenter의 하드코딩된 `cargo:simulation-potato-hub-1`을 제거하고 CARGO-1의 `cargo:sim.potato.20260407.r3` snapshot을 직접 투영한다. Farm→Hub Scene에서 15개 상자·300kg·400kg 용량과 `HarvestLot → PackageLot → Cargo` lineage가 같은 ID로 보이며 focused headless 7/7, Unity route EditMode 3/3, Unity core 전체 278/278과 Play Mode Game View를 확인했다. 이 adapter는 Cargo 원장 상태를 `Loaded`로 유지한다. 화면의 Van 이동만으로 `InTransit`, 도착 또는 인수를 만들지 않으며 실제 Journey 상태 전이는 다음 explicit Command/Tick Gate의 책임이다.

`JOURNEY-1/WORLD-7`은 별도 Journey Simulation 원장에서 Dispatch Preview/Confirm/Tick으로만 `Loaded → InTransit`을 만들고, 3회 route Tick과 게임 날짜 진행으로 `ArrivedAtHub`에 도달한다. 각 Tick은 Data·Cargo revision을 올리되 Cargo stable-ID, 15개 상자, 300kg, HarvestLot·PackageLot lineage를 보존한다. Unity Scene은 자동 왕복 follower를 비활성화하고 원장의 normalized progress로 Van 위치를 결정한다. headless 6/6, Unity EditMode 4/4, Unity core 전체 284/284와 Play Mode `ArrivedAtHub / 2026-04-10 / revision 3` Game View를 확인했다. endpoint 도착은 검수·입고·재고·판매를 확정하지 않는다.

`HUB-1`은 도착 Cargo를 Receiving Review/Confirm/Tick으로 `Inspection`에 넣고, Inspection Review/Confirm/Tick으로 300kg을 288kg 합격과 12kg 손실로 보존한다. 손실 이유 `DamageFixture`와 received·accepted·rejected 수량 합계를 별도 inspection result에 기록하며 Cargo·HarvestLot·PackageLot lineage를 유지한다. 합격은 아직 HubStockLot이나 City outbound Cargo가 아니고 손실도 숨겨 폐기하지 않는다. headless 6/6, Unity EditMode 4/4, Unity core 전체 290/290과 Play Mode `Accepted / Data revision 3 / Cargo revision 5` Game View를 확인했다.

`HUB-2/WORLD-8`은 Accepted inspection result에서만 Lot 분리 Preview/Confirm/Tick을 허용해 288kg `AcceptedForOutbound` Lot과 12kg `LossRecorded` Lot을 함께 만든다. 두 Lot은 inspection result와 source Cargo를 보존하며 합이 300kg인지 검증한다. 다음 City outbound Preview/Confirm/Tick은 합격 Lot만 288kg `CandidateOnly`의 source로 연결하고 손실 Lot이 candidate lineage에 들어오면 거부한다. 화면에는 Hub의 두 Lot marker와 Hub→City 후보 route를 표시하지만 이는 출발 Cargo·Hub 재고·City 입고가 아니다. 집중 headless 7/7, Unity EditMode 4/4, Unity core 전체 297/297, Unity 전체 EditMode 150/150과 Play Mode `OutboundCandidate / Data revision 3` Game View를 확인했다.

| 영역 | 현재 확인된 기반 | 이번 제안이 채울 공백 |
| --- | --- | --- |
| 상품 identity | 서버 DB에 canonical 60개와 출처별 code relation | 품목과 재배 기준·재배작기의 명시적 관계 |
| Farm asset | Unity 60개 전수 분류, 28개 prefab 연결 | 생육 단계별 Visual variant와 field layout |
| Farm 상호작용 | 6×6 tile의 선택→Preview→Confirm→Tick→Reconcile 밭갈이 | 파종·정식·생육·수확 폐루프 |
| 감자 World | Farm read projection, 상품·가격 카드, Simulation Hub route | canonical 수확 lot와 cargo 관계 |
| World 이동 | Farm·Town·Hub·City Region과 route·vehicle 표현 | 품목 lot의 상차·운송·도착 상태 전이 |
| City·Market | 마트 surface와 공개 상품 read 경계 | 입고 lot·진열 수량·Simulation 판매 원장 |
| 시간 연출 | Dawn·Day·GoldenHour·Night Presentation | 날짜·계절·작형을 포함하는 게임 달력 |

따라서 현재 `Prefab 연결됨`은 품목별 게임 플레이가 완성됐다는 뜻이 아니다. 다음 Gate는 visual catalog에 규칙을 넣는 것이 아니라 canonical 관계와 상태 전이를 먼저 마련하는 것이다.

## 3. 제품 경험 원칙

### 3.1 품목 하나가 하나의 이야기 단위다

사용자는 품목 카드를 보고 끝나는 것이 아니라 같은 품목의 생산과 이동을 이어서 본다.

- 밭에서는 재배작기, 작업, 예상 수확 시기와 근거를 본다.
- 수확장에서는 수확 lot, 수량, 품질 판정 상태와 포장 단위를 본다.
- 트럭에서는 어떤 lot가 어느 route로 이동하는지 본다.
- Hub·Town·City에서는 검수, 보관, 진열과 판매 가능 여부를 본다.
- 판매 뒤에는 매출만이 아니라 생산비·운송비·손실·노동·남은 재고를 함께 본다.

하나의 큰 Scene에 60개 품목을 동시에 늘어놓기보다, 동일한 World와 시스템 안에서 품목별 시나리오를 선택해 반복 경험하게 한다.

### 3.2 현실의 기준과 게임의 규칙을 구분한다

현실의 파종·수확 가능 시기는 지역, 품종, 노지·시설, 육묘·직파, 기상과 재배법에 따라 달라진다. 그러므로 `감자는 3월 파종, 6월 수확` 같은 단일 월을 상품 속성으로 고정하지 않는다.

```text
CanonicalProductStableId
  └─ CropVariantStableId
       └─ CultivationCalendarProfileStableId
            ├─ RegionCode
            ├─ CultivationMethodCode
            ├─ Source/Evidence/Revision
            └─ 권장 작업 window

FarmPlotStableId
  └─ CultivationCycleStableId
       ├─ 선택한 calendar profile
       ├─ 실제 파종·정식·수확 일자
       └─ 현재 생육·작업·판정 상태
```

calendar profile은 참고 가능한 기준이고, 실제 재배작기는 사용자가 선택하고 서버 또는 명시적 Simulation Command가 만든 원장이다. profile이 수정돼도 이미 진행 중인 재배작기의 과거 기록을 조용히 바꾸지 않는다.

### 3.3 World 행동은 상태를 표현하고 자동 확정하지 않는다

- 씨앗을 뿌리는 animation이 끝났다고 파종 완료가 되지 않는다.
- 작물이 크게 보인다고 수확 가능 수량이 생기지 않는다.
- 상자가 트럭 mesh 안에 들어갔다고 상차가 확정되지 않는다.
- 차량이 City Gate에 도착했다고 입고·검수·판매가 완료되지 않는다.
- NPC가 상품을 집었다고 운영 판매나 결제가 발생하지 않는다.

각 행동은 `Preview → 명시적 Confirm → authoritative Tick/Command → 새 revision 조회 → Reconcile`을 통과한다. Simulation 판매는 `SIMULATION`으로 표시하고 운영 주문·결제·소유권 이전과 연결하지 않는다.

## 4. 품목 생애주기와 원장 경계

### 4.1 canonical node

| node | 소유하는 의미 | 주요 식별 관계 |
| --- | --- | --- |
| `공통식품품목Identity` | 여러 World가 공유하는 상품 identity | `CanonicalProductStableId` |
| `작물품종기준` | 실제 재배 대상을 구분하는 품종·작물 기준 | Product와 1:N 가능 |
| `재배달력Profile` | 지역·작형별 권장 작업 window와 근거 | Variant·region·method·revision |
| `재배작기` | 특정 농장·구획에서 실제 진행하는 재배 | Plot·Variant·CalendarProfile |
| `수확Lot` | 한 재배작기에서 나온 추적 가능한 수확물 | CultivationCycle·Product |
| `포장단위` | 상자·bag·crate·pallet의 수량 단위 | HarvestLot·quantity·unit |
| `화물Lot` | 운송 인계가 가능한 화물 묶음 | Package lots·cargo revision |
| `운송Journey` | Farm→Town/Hub/City 이동과 인계 | Cargo·vehicle·route·handoff |
| `시장재고Lot` | 검수 뒤 판매 장소가 보유한 재고 | Cargo/Harvest lineage |
| `Simulation판매원장` | 가상 고객 판매와 잔여 재고·손실 | MarketStockLot·game time |

`CanonicalProductStableId` 하나에 여러 품종·작형·재배작기가 연결될 수 있다. 반대로 여러 수확 lot를 한 cargo에 합칠 때도 원래의 cultivation·harvest lineage를 잃지 않는다.

### 4.2 상태 전이

```text
Planning
  → SoilPrepared
  → Sown | Transplanted
  → Emerged
  → Vegetative
  → Flowering/Fruiting 또는 Bulking
  → HarvestReady
  → Harvested
  → Graded
  → Packed
  → Loaded
  → InTransit
  → Arrived
  → Inspected
  → Stored
  → Displayed
  → PartiallySold
  → SoldOut | Withdrawn | LossRecorded
```

모든 품목이 같은 생육 단계를 갖는 것은 아니다. 곡류·엽채·근채·과채·과수별 `GrowthStageProfile`을 두고, 지원되지 않는 단계를 억지로 채우지 않는다. 물류 이후 상태는 작물 생육과 분리된 lot 상태다.

### 4.3 이벤트와 확인

| 사용자 의도 | Preview | Confirm 뒤 결과 |
| --- | --- | --- |
| 파종/정식 | 날짜 window, 토지·입력·예상 비용·차단 사유 | 재배작기 revision 생성 또는 변경 |
| 수확 | 수확 가능 판정, 예상량 범위, 노동·상자 필요량 | 수확 lot 생성 |
| 상차 | lot·차량 용량·목적지·담당·인계 조건 | cargo와 loading handoff 생성 |
| 출발 | route·예상 시간·source mode·차단 사유 | Journey 시작 |
| 입고 | 도착 cargo·검수 필요·보관 위치 | 시장재고 후보 생성 |
| 진열 | 판매 가능량·가격 근거·손실 위험 | display allocation 생성 |
| Simulation 판매 | 고객군·시간대·가격·수요 가정 | 판매 원장과 잔여 재고 revision 생성 |

실제 판매가, 공공 가격 관측, Simulation 가격은 같은 필드로 합치지 않는다. 공공 가격은 근거 카드, 판매가는 해당 시장의 제안 또는 운영 값, Simulation 가격은 시나리오 입력이다.

## 5. 게임 달력 설계

### 5.1 세 개의 시간을 동시에 보존한다

| 시간 | 용도 | 예시 |
| --- | --- | --- |
| `ReferenceDate` | 현실 기준정보와 출처의 날짜 | 농업기술 자료 기준일, 가격 관측일 |
| `SimulationDate` | 게임 안에서 진행되는 날짜 | Year 1, 04-12 |
| `PresentationTimeOfDay` | Dawn·Day·GoldenHour·Night 연출 | 16:40 GoldenHour |

날짜가 하루 진행됐다는 사실과 해가 저무는 연출은 관련되지만 같은 시스템이 아니다. `WorldTimeOfDayProfile`은 빛·안개·조명을 표현하고, `SimulationClock`은 재배·운송·판매 규칙의 시간을 제공한다.

### 5.2 달력 화면

게임 달력은 월간 달력 하나와 선택 품목의 timeline을 결합한다.

- 월간 영역: 현재 Simulation 날짜, 계절, 기상 관측 또는 Simulation 날씨, 예정 작업
- 품목 lane: 파종, 육묘, 정식, 관리, 예상 수확의 **기간 window**
- 실제 기록: 사용자가 수행한 날짜와 현재 revision
- 근거 표시: source, 지역, 재배 방식, 기준일, 제한
- 경고: window 밖 작업, 자료 없음, stale, 지역 불일치
- 비교: 노지/시설 또는 조생/중생/만생 profile을 같은 품목 안에서 전환

window 밖 작업을 무조건 금지하지 않는다. authoritative rule이 금지를 규정하지 않았다면 생산성·위험 preview를 보여주고 사용자가 Simulation에서 선택하게 한다. 운영 농장 작업은 별도 권한과 확인을 요구한다.

### 5.3 시간 배율

- 기본은 `1 Simulation day`를 사용자가 읽고 작업을 선택할 수 있는 단위로 둔다.
- `Pause / 1x / 4x / 다음 작업까지`를 제공하되, Confirm이 필요한 작업을 자동 수행하지 않는다.
- 고속 진행 중 차단·수확 가능·cargo 도착 같은 결정 지점에서는 멈추거나 명시적 알림을 낸다.
- offline 경과나 실제 벽시계가 공유 World를 임의로 진행시키지 않는다.
- deterministic seed, scenario revision과 command log로 같은 결과를 재생할 수 있어야 한다.

## 6. 품목별 시각 구체화

### 6.1 하나의 품목에 필요한 Visual role

```text
Product Visual Set
  ├─ Field/Growing: 밭·지주·과수에 붙은 재배체
  ├─ HarvestedLoose: 수확 직후 낱개·묶음
  ├─ Packed: 상자·bag·crate
  ├─ Cargo: pallet·truck socket에 실린 묶음
  ├─ MarketDisplay: 진열대·바구니·가격표
  └─ CustomerCarry: 장바구니·봉투의 제한된 표현
```

같은 prefab을 모든 role에 쓰지 않는다. 현재 catalog가 plant group만 제공하는 품목은 `Field/Growing`만 구현된 것으로 보고, `Packed`나 `MarketDisplay`는 별도 semantic VisualKey와 검증된 asset이 준비될 때 연결한다.

### 6.2 생육 단계 표현

첫 구현에서는 품목마다 수십 개 mesh를 만들지 않는다. 다음 우선순위를 사용한다.

1. 전용 단계 prefab이 있으면 `StageVisualKey`로 직접 연결한다.
2. 같은 plant prefab의 scale·밀도·색을 제한적으로 바꾸되 실제 품종 차이를 뜻하지 않게 한다.
3. 흙 표면, row marker, 지주, 꽃·열매 socket처럼 품목군 공통 composition을 사용한다.
4. 오해 없이 표현할 수 없으면 label·card만 표시하고 잘못된 작물 prefab으로 대체하지 않는다.

수량은 renderer 개수나 상자 개수로 계산하지 않는다. 대량 lot는 대표 상자 수와 `DisplayedQuantity/ActualQuantity`를 함께 보여준다.

### 6.3 28개 연결 품목의 적용 순서

현재 catalog 기준 적용 대상은 다음과 같다.

| 묶음 | 품목 | 제안 |
| --- | --- | --- |
| 첫 폐루프 | 감자 | 기존 Farm·Hub·City 수직 슬라이스와 상자 asset을 재사용해 전체 생애주기 완성 |
| 일반화 1차 | 양파, 토마토, 딸기, 사과 | 근채/구근, 과채, 시설재배 후보, 과수의 서로 다른 구조 검증 |
| Direct 밭작물 | 콩, 양배추, 상추, 수박, 오이, 당근, 브로콜리 | 검증된 공통 단계·lot·cargo·market adapter로 확대 |
| Direct 과수 | 배, 복숭아, 바나나, 오렌지, 레몬, 체리 | 지역·시설·수입/국내 생산 경계를 확인한 profile만 활성화 |
| Representative | 배추, 얼갈이배추, 호박, 풋고추, 붉은고추, 피망, 파프리카, 알배기배추, 방울토마토, 감귤 | 카드와 World에 `대표 외형`을 명시하고 품종·가격 동일성으로 사용하지 않음 |

`Unmapped 32`는 게임 대상에서 삭제하지 않는다. 달력·Data·원장 흐름은 구현할 수 있지만 World에는 잘못된 작물 대신 정보 marker 또는 정직한 placeholder를 사용한다. 전용 asset이 확보되면 catalog revision만 올려 Presentation을 교체한다.

## 7. Farm에서 Town·City까지의 World 구성

### 7.1 Farm

- 품목 선택 보드와 계절별 planting window
- 토양 준비, 파종/정식, 관리와 수확 interaction socket
- 생육 단계가 다른 plot을 동시에 읽을 수 있는 3/4 composition
- 수확장, 선별대, 빈 상자·채운 상자, loading yard
- product·cultivation·harvest lot Concept Card

### 7.2 차량과 route

- `CargoVisualRoot` 아래 품목별 `PackedVisualKey`를 배치한다.
- 차량은 실제 cargo 수량을 소유하지 않고 load ratio와 대표 visual만 표현한다.
- Farm→Town은 소규모 직거래·공동수령 시나리오, Farm→Hub→City는 집하·검수·대량 유통 시나리오로 구분한다.
- route 선택에는 시간·비용·용량·손실 위험·담당과 source를 함께 보여준다.
- 교통 animation과 route follower는 Journey 상태의 consumer다.

### 7.3 Town

Town은 City의 축소판이 아니다.

- Farm stand, 전통시장 또는 공동수령 거점에서 소량 lot를 다룬다.
- 생산자 직거래와 공동 수령은 비용·노동·보관 책임을 따로 보여준다.
- 주민 NPC는 시장의 생활감을 만들지만 개인정보나 실제 개인 구매 이력을 표현하지 않는다.
- 판매는 Simulation 수요 집단 단위로 집계하고 종교·국적·언어·가족 형태·경제력을 구매 성향 proxy로 쓰지 않는다.

### 7.4 City

- Hub 도착, 검수, 보관, 마트 입고, 진열을 서로 다른 상태로 표현한다.
- 진열대의 상품 수는 `MarketStockLot`과 allocation projection에서 읽는다.
- 고객 이동·집기·계산 animation은 Simulation 판매 결과에 맞춰 재생하되 결과를 만들지 않는다.
- 품절·폐기·할인 후보는 원장 상태와 근거가 있을 때만 표시하며 임의 urgency queue를 만들지 않는다.

## 8. Data와 코드 경계 제안

### 8.1 서버·공통 contract

추가 또는 확장이 필요한 최소 계약은 다음과 같다.

```text
재배달력ProfileDto
  ProfileStableId
  CanonicalProductStableId
  CropVariantStableId
  RegionCode / CultivationMethodCode
  ActivityWindows[]
  SourceReferences[] / Revision / EffectiveOn

재배작기SnapshotDto
  CultivationCycleStableId / Revision
  FarmStableId / PlotStableId
  Product / Variant / CalendarProfile relation
  SimulationDateRange / ActualActivityRecords[]
  GrowthStageCode / AllowedInteractionIntentCodes[]

수확유통LotSnapshotDto
  HarvestLotStableId / Product / Cultivation lineage
  Quantity / Unit / GradeStatus
  PackageLots[] / CargoRelation? / MarketStockRelation?
  SourceMode / Revision / GeneratedAt
```

재배달력은 공공 기준정보 projection, 재배작기·lot·cargo·market stock은 업무 또는 Simulation 원장이다. 한 DTO에 모두 평탄화해 어느 값이 근거이고 어느 값이 실제 상태인지 흐리지 않는다.

### 8.2 Unity core

```text
API model
  → Mapper/Validator
  → DataManager world memory
  → Simulation preview/engine
  → Perspective interpreter
  → Presentation model
  → World wrapper / VisualRoot
  → semantic VisualKey catalog
  → prefab/material/FX
```

Unity mirror model은 서버 DTO를 그대로 공유하지 않고 nullable·source·revision·limitation을 보존한다. `FarmProductVisualCatalog`는 Presentation 끝단에 남으며 calendar rule, yield, price, cargo quantity를 가지지 않는다.

### 8.3 제안하는 semantic VisualKey

```text
farm.product.{crop}.stage.{stage}
farm.product.{crop}.harvested.loose
farm.product.{crop}.package.{packageType}
cargo.product.{crop}.load.{fillBand}
market.product.{crop}.display.{displayType}
```

key는 prefab 경로가 아니다. Direct·Representative·Unmapped 판정과 evidence note를 각 role별로 보존한다.

## 9. 단계적 구현안

| 단계 | 범위 | 완료 Gate |
| --- | --- | --- |
| `CALENDAR-0` | Product→Variant→지역·작형 calendar profile 계약, source·revision·window validator | 날짜를 상품명으로 추정하지 않고 fixture 하나를 source와 함께 읽음 |
| `FARM-3` | 감자 파종 Preview/Confirm, 생육 Tick, HarvestReady, 수확 lot 생성 | 밭갈이 뒤 감자 재배작기 revision과 수확 lot가 결정적으로 재현됨 |
| `HARVEST-CHOICE-1` | HarvestLot 상호작용, 조합 출하·직판·수출대행 판로 Preview/Confirm/Tick | 선택 하나만 후속 업무 후보가 되고 실제 거래 효과는 발생하지 않음 |
| `COOP-1` | 조합 인수 Lot과 CARGO-1 포장 검토 후보 생성 | 조합 선택만 허용하고 인수 전 연결 차단, PackageLot·Cargo 미생성 검증 |
| `DIRECT-1` | 생산자 소포장 Lot과 비공개 온라인 등록 초안 생성 | 300kg 수량 보존, 가격·주문·결제·택배 미생성 검증 |
| `CARGO-1` | 감자 선별·포장·상차, canonical cargo relation | 상자 animation과 무관하게 lot 수량·lineage·차량 용량 검증 통과 |
| `WORLD-6` | Farm→Hub→City 또는 Farm→Town Journey | 차량 이동 전후 같은 cargo stable ID와 handoff revision 보존 |
| `MARKET-1` | 검수·입고·진열·Simulation 고객 판매 | public price·판매가·Simulation price 분리, 잔여 재고 수렴 |
| `PRODUCT-1` | 양파·토마토·딸기·사과 일반화 | 네 재배 유형이 같은 계약을 쓰되 단계 profile은 독립적 |
| `PRODUCT-2` | Direct 18개 확대 | 품목별 calendar·stage·package role 지원표와 회귀 테스트 |
| `PRODUCT-3` | Representative 10개 확대 | 모든 카드와 World 선택에 대표 외형 표시, 동일성 오인 없음 |
| `PRODUCT-4` | Unmapped 32개 Data-ready | 잘못된 대체 asset 없이 profile·원장·placeholder 정책 검증 |

### 9.1 첫 절단선 완료와 판로 분기

첫 구현인 `CALENDAR-0 + FARM-3`를 닫은 뒤 `HARVEST-CHOICE-1`까지 확장했다.

```text
감자 canonical identity
  → 지역·작형이 명시된 calendar fixture 1개
  → Game Calendar의 파종·수확 window
  → 기존 6×6 tile 선택
  → 파종 Preview/Confirm
  → 결정적 Simulation 날짜 진행
  → 생육 단계 Reconcile
  → HarvestReady와 수확 Preview
  → 300kg HarvestLot 생성
  → HarvestLot 상호작용
  → 조합 출하 / 온라인 직판 / 수출대행 준비 중 하나를 결정
```

판로 결정 자체는 PackageLot·Cargo·상품 등록·주문을 만들지 않는다. 다음 단계는 세 선택마다 별도 후속 Gate를 두고, 기존 `CARGO-1`은 먼저 조합 출하 분기와 명시적으로 연결한다.

## 10. 품목 지원 manifest

60개를 코드 switch와 Scene hierarchy에 흩어 놓지 않고 revision이 있는 manifest로 관리한다.

| 필드 | 의미 |
| --- | --- |
| `CanonicalProductStableId` | 서버 canonical identity |
| `CultivationSupportCode` | `Playable`, `CalendarOnly`, `NotApplicable`, `Unknown` |
| `CalendarProfileIds` | 지역·작형별 profile 목록 |
| `GrowthStageProfileId` | 품목군 또는 품종별 생육 단계 |
| `FieldVisualStatusCode` | Direct·Representative·Unmapped |
| `HarvestVisualStatusCode` | 수확물 외형 지원 상태 |
| `PackageVisualStatusCode` | 상자·bag·crate 지원 상태 |
| `MarketVisualStatusCode` | 진열 외형 지원 상태 |
| `CargoPolicyCode` | 온도·취급·용량 규칙의 지원 여부 |
| `EvidenceRevision` | 판정 근거와 catalog revision |

`NotApplicable`은 수산물처럼 Farm 재배 calendar가 맞지 않는 품목에 사용한다. 이런 품목을 억지로 밭에 심지 않고 향후 어업·양식·수입 유통 World로 routing한다.

## 11. 검증 계획

### 11.1 계약·데이터

- Product·Variant·CalendarProfile·CultivationCycle stable ID 중복과 참조 무결성
- 지역·작형이 다른 calendar window의 혼합 금지
- window의 시작·종료, 월경계·연도경계와 윤년 처리
- source·기준일·revision·limitation 결측 거부
- profile 갱신 뒤 진행 중 재배작기의 pinned revision 보존
- Product→HarvestLot→Cargo→MarketStock lineage 왕복 검증
- 동일 Confirm 재처리의 멱등성과 expected revision 충돌
- Direct·Representative·Unmapped role별 catalog 검증

### 11.2 Simulation

- 같은 seed·scenario·command log에서 동일 생육·수확·판매 결과
- Pause·배속·다음 작업 이동에서 Confirm 작업 자동 실행 금지
- 수확 가능 전 수확, 용량 초과 상차, 미검수 진열, 재고 초과 판매 차단
- 손실·폐기·부분 판매 뒤 질량 보존 또는 명시적 loss ledger
- 가격 관측 부재를 임의 판매가 fallback으로 숨기지 않음

### 11.3 Unity Editor·Game View

- 같은 plot에서 파종 전·초기·중기·수확 가능 상태의 Play Mode PNG
- 감자 loose/box/truck/market display가 같은 Product와 서로 다른 role로 선택됨
- Farm·route·Hub/City의 동일 cargo journey 연속 캡처
- 고객 animation 전후 판매 원장 revision과 잔여 재고 카드 확인
- Representative 품목의 `대표 외형` label과 Unmapped placeholder 확인
- Dawn·Day·GoldenHour·Night에도 calendar·상태·source card 가독성 유지

자동 테스트, Editor compile, Scene Builder 실행, Play Mode, Game View PNG는 서로 독립된 증거로 기록한다.

## 12. 완료 기준

품목 하나의 생애주기가 완료됐다고 말하려면 다음을 모두 만족해야 한다.

1. canonical Product와 region·method·source가 있는 calendar profile이 연결된다.
2. 파종 또는 정식과 수확이 명시적 Command·revision으로 기록된다.
3. 생육 단계와 수확 가능 판정이 prefab scale이나 animation에서 나오지 않는다.
4. 수확 lot가 재배작기와 수량·단위·source lineage를 보존한다.
5. cargo가 같은 lot를 Farm에서 Town·Hub·City까지 운반한다.
6. 검수·입고·진열·판매가 서로 다른 상태이고 잔여 재고가 수렴한다.
7. 판매 결과가 Simulation인지 Operational인지 화면과 Data에 명시된다.
8. 해당 품목의 Direct·Representative·Unmapped 표현이 모든 role에서 정직하게 표시된다.
9. headless test와 Unity EditMode가 통과하고 최종 Play Mode Game View PNG가 남는다.

## 13. 명시적 비범위와 중단 조건

이번 제안은 다음을 자동으로 허용하지 않는다.

- 공공 재배 정보를 실제 농업 처방이나 안전 보장으로 사용
- 상품명만으로 지역·품종·파종·수확 날짜 생성
- NPC animation으로 운영 주문·결제·소유권 이전 확정
- 공공 가격을 실제 판매가나 재고로 사용
- 수요가 높다는 이유로 자동 재배·배차·가격 변경
- asset이 있다는 이유로 국내 재배 가능성 또는 상품 동일성 확정
- 수산물·축산물을 Farm 작물 lifecycle에 억지로 포함
- prefab 이름·경로를 서버 contract나 Simulation rule에 저장

다음 중 하나가 발생하면 품목 확대를 멈추고 앞 Gate를 보완한다.

- calendar source와 region·method가 불명확함
- 수확 lot와 cargo의 수량·lineage가 끊김
- Representative 외형이 Direct처럼 표시됨
- 상자 수와 실제 재고 수량이 불일치하는데 설명이 없음
- 차량 도착 animation이 입고를 자동 확정함
- Simulation 판매가 실제 판매 또는 운영 가격처럼 보임
- 품목 추가마다 별도 Scene·전용 manager·중복 switch가 늘어남

## 14. 제안 결론

다음 발전 방향은 **28개 asset 연결을 28개의 장식으로 늘리는 것**이 아니라, 한 품목이 계절과 달력을 따라 생산되고 같은 identity와 provenance를 유지한 채 사람에게 도달하는 공통 시스템을 만드는 것이다.

이 subsystem 안의 의존 순서는 다음과 같다. 전체 프로젝트 우선순위에서는 공통 Simulation World·Decision/Task/Effect·정착지 경제가 먼저이며, 이후 각 판로와 물류 단계를 이 순서에 맞춰 연결한다.

```text
감자 calendar·재배·수확 폐루프
  → 수확물 판로 선택
  → 조합 인수 / 생산자 직판 포장 / 수출 준비
  → 판로별 lot·운송·시장 판매 폐루프
  → 양파·토마토·딸기·사과로 구조 일반화
  → Direct 18개
  → Representative 10개
  → Unmapped 32개의 정직한 Data-ready 상태
```

이 subsystem은 사용자가 원하는 “각 품목이 밭에서 길러지고, 트럭에 실리고, Town·City로 이동하고, 사람들에게 팔리는 World”의 생산·유통 기반이다. 다음 구현에서는 분기 수를 먼저 늘리지 않고 이 생애주기를 정착지 재정·노동·시장·비축과 공통 WorldTick에 연결해 영지 경영 Simulation의 실제 인과로 승격한다.
