# Unity World 구현 현황과 우선순위

## 0. 2026-08-10 Simulation World Shell 우선순위 부록

기존 P0~P9와 Composition Track은 지금까지 완료한 기반과 검증 이력으로 보존한다. 현재 활성 순서는 개별 기능 Scene을 더 늘리는 대신 [Unity Simulation World Shell·정착지 Scene 기반 재정렬 제안서](UnityWorldShellSettlementSceneFoundationProposal.md)를 따른다.

```text
읽기 전용 Presentation 선행 — 완료
[WORLD-SHELL-0]
  → [SETTLEMENT-SCENE-0]

서버 권위 복귀 — 완료
[SETTLEMENT-ECONOMY-1]

첫 playable 조립
[WORLD-SETTLEMENT-NAV-0] — 완료
  → [BRANCH-ADAPTER-1] — 완료
  → [SETTLEMENT-VISUAL-BASE-0] — 완료
  → [SETTLEMENT-INTERACTION-0] — 완료
  → LOGISTICS-MOVEMENT-1
```

Shell과 첫 정착지 Scene 완료 뒤 `SETTLEMENT-ECONOMY-1`로 수확 Lot 단일 allocation과 완료 Tick의 경제 반영을 구현하고, navigation·branch adapter·visual base를 거쳐 `SETTLEMENT-INTERACTION-0`에서 HarvestLot 선택→네 판로 Preview→Confirm→Task 예약→WorldTick→Effect→새 snapshot reconcile을 닫았다. Production repository는 공식 Simulation API 경로와 expected revision을 사용하며 Game View fixture는 명시적으로 분리된다. 이제 활성 Gate는 `LOGISTICS-MOVEMENT-1`이고, Unity adapter, NPC·차량 animation이나 Scene object는 계속 Task 완료와 수량 권위를 갖지 않는다.

기존 공공데이터 `WorldBootstrapScene`은 공개지도 surface로 유지했다. Simulation용 Scene은 별도 `SimulationWorldShell`로 추가하고, 첫 버전은 하나의 Scene 안에서 `WorldMapRoot`와 `SettlementInteriorRoot`를 전환해 같은 Tick 12·Revision 12 보존을 증명했다. 실제 additive Scene loading은 콘텐츠 규모가 필요성을 증명한 뒤 shell 뒤의 loader로 추가한다.

## 1. 목적

이 문서는 서버 상태를 Unity World로 투영하는 현재 구현을 한곳에 모으고, 다음 작업을 **기술 의존성**, **현재 slice 완결 비용**, **제품 공개 우선순위**, **권한·개인정보 위험**을 기준으로 정렬한다.

P0~P7의 기본 배선 이후 각 공간의 현실 업무·상태 변화·NPC·interaction을 깊게 구현하는 순서와 첫 창고 slice는 [Unity Zone 업무 심화 설계](UnityZoneDomainDeepeningDesign.md)를 따른다.

새 데이터 종류를 더 연결하기 전 읽기 변환은 [Unity Data·Interpretation·Presentation 기준 아키텍처](UnityDataInterpretationPresentationArchitecture.md)에 따라 분리한다. 현재 W1을 첫 migration pilot으로 삼고 기존 route·JSON·stable ID·refresh 동작을 보존한 뒤 W2로 진행한다.

사용자가 World에서 낯선 업무 의미를 학습하는 공통 진입 문법은 [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)을 따른다. Scene asset 연결 전에 `Concept / Status / Reason / Action` 계약과 Projector를 먼저 안정화한다.

장소와 역할은 별도 Scene 복제 기준이 아니다.

```text
Canonical server state
  → 권한별 Projection API
  → Unity Repository / UseCase
  → Zone Controller + Role Experience
  → World View / Role View / Detail View
  → NPC movement presentation
```

Zone Controller는 장소의 현재 상태를 조율하고, Role Experience는 같은 장소에서 현재 사용자에게 허용된 대상과 행동을 강조한다. NPC는 서버 상태를 설명하는 시각적 투영이며 도착만으로 주문·배차·상차 같은 운영 Command를 발생시키지 않는다.

## 2. 현재 구현 기준선

| 영역 | 현재 상태 | 다음 완결 조건 |
| --- | --- | --- |
| 공통 데이터 계층 | stable ID, revision, provenance, Repository, UseCase, reconcile 구현 | 실제 제품 Unity 프로젝트의 HTTP adapter와 composition root 연결 |
| 도심마트 | UM0~UM5, SC0~SC5와 RG0~RG4·RG4-NPC-A/B code, CC0~CC2 완료 | CC3 + RG4-NPC-C Card View·Scene runtime wiring |
| 전통시장·공개 물류거점 | 공개 위치·출처 기반 simulated slice와 primitive builder 구현 | 기존 공개 세계지도 aggregate와 연결 |
| Role Perspective | 생산자·주문자·운송자 공통 계약과 applicator 구현 | 생산자·주문자 server aggregate 추가 |
| 도심 물류센터 운송자 | 인증된 현재 배정 운송 기반 server Role/NPC API 구현 | 실제 UnityWebRequest adapter, NavMesh bake와 runtime 확인 |
| NPC | Zone route catalog, NavMeshAgent·Animator socket, stable-ID 적용 구현 | Zone별 canonical API 확대와 실제 Animator Controller |
| 작물 | 농사로 기준정보 server→Unity read-only 흐름 구현 | Farm·Plot·Cultivation·Sensor canonical 운영 contract |
| Synty | WORLD-5 뒤 사용자 요청의 별도 Farm·City 그래픽 Showcase Scene 구현 | Test Runner 복구·최종 증거 뒤 Visual 범위 재중단 |
| Farm 밭갈이 | FARM-2 Preview→Confirm→Tick→새 snapshot·reconcile 완료 | FARM-3 농부 작업 Presentation |

## 3. 기존 World 기반의 구현·검증 순서

기존 SS0~SS1, UM5-B, SC0~SC5, RG1~RG4, CC0~CC3-A와 FARM-0~FARM-1 기반은 유지한다. [Unity 입체 탑다운 City·Farm World 구성 제안](UnityCityFarmPackWorldCompositionProposal.md)의 WORLD-0~WORLD-5와 FARM-2를 닫은 뒤, 2026-08-09 사용자 요청에 따라 기존 Farm·City Pack만 사용하는 한정 Showcase와 [Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)의 Composition Track, FARM-3 이후 생애주기 slice를 진행했다. 아래 표는 그 구현·검증 이력을 보존하며 현재 다음 순서는 위 0절의 Simulation World Shell 부록이 우선한다.

| 우선순위 | 구현 묶음 | 재사용하거나 추가할 핵심 | 완료 Gate |
| --- | --- | --- | --- |
| P0 | 사전 조사와 기준선 고정 | 실제 Unity project·Scene, City/Farm prefab allowlist, URP Asset·Renderer·Quality, 기존 View·builder·VContainer wiring, baseline test·Console | 중복 생성 금지 목록, 수정 대상 목록, 선택 asset과 현재 오류가 기록됨 |
| P1 | WORLD-0 Camera Prototype | 기존 Camera가 없을 때만 `DioramaTopDownCameraRig`, World/Zone/Object focus anchor와 coordinator, pan·zoom·90도 회전, 최소 roof/foreground occlusion | primitive Farm·Logistics·Market에서 focus와 가림을 Game View로 확인 |
| P2 | WORLD-1 Macro World Blockout | 하나의 Bootstrap과 Farm→Farm Yard→Transport→Logistics→Market→Residential Zone root, route와 vehicle/focus anchor | text 없이 생산→수령 방향이 읽히며 interior·장식은 아직 없음 |
| P3 | WORLD-2 Presentation Catalog·Synty 품질 | 실제 inventory 기반 Farm/Urban/Transition VisualKey·catalog, wrapper scale·pivot 보정, 공통 lighting·Volume·URP Renderer 영향 확인 | Overview/Farm/Logistics/Market 비교 화면에서 두 Pack이 한 World로 읽힘 |
| P4 | WORLD-3 기존 업무 View 연결 | `FarmSoilTileCellView`, `TransportCorridorTruckView`, `LogisticsFacilityOverviewView`, 도심마트 surface/Card, Residential pickup의 기존 `VisualRoot` | Synty 외형을 primitive로 되돌려도 stable ID·선택·Presentation test가 유지됨 |
| P5 | WORLD-4 Cargo Handoff | prefab 중립 cargo stable ID·lineage PresentationModel, Zone anchor와 Zone별 VisualKey, Farm Yard→차량→물류센터→마트 표현 | 같은 감자 cargo의 lineage가 전 구간에서 유지되고 도착 event는 상태를 확정하지 않음 |
| P6 | WORLD-5 품질·성능·증거 Gate | Console·shader·prefab 검사, renderer·Animator·FX·draw range와 기본 profiling, PC target과 Android 후보 분리, 대표 Game View 4종 | `docs/Changes`와 `docs/assets/changes`에 최종 PNG·측정·제한을 기록하고 Visual 확장을 중단 |
| P7 | FARM-2 밭갈이 폐루프 | 기존 FARM-0~FARM-1 6×6 snapshot·Projector·View 위에 Preview, explicit Confirm, Simulation Command/Tick, 새 snapshot과 reconcile | 선택→Preview→Confirm→Tick→Dirt Row가 실제 state 변화로 왕복하고 animation·NPC는 권위를 갖지 않음 |
| P7.5 | 사용자 요청 Farm·City 그래픽 Showcase | 기존 WORLD-5를 보존한 별도 Scene, Presentation 전용 Environment key/catalog, Farm·City vendor prefab의 전경·중경·원경 배치 | 최종 테스트·Console·Overview/Farm/Logistics/Market·profiling 증거를 남기고 범위를 다시 중단 |
| P7.6 | Farm·Town·City Composition Track | 기존 Farm 24개와 WORLD 기반을 유지하고 `CMP0` 기준선→공통 계약·실측→도로/Gate A형→Pack·Hub 최소 A형→공용 Humanoid locomotion→다중 origin Journey→감자 가격 카드 순으로 구현 | 후보 전체 일괄 생성 없이 Farm/Town→Hub→City 물류와 사람 이동을 분리한 수직 슬라이스를 닫음 |
| P8 | FARM-3~FARM-5 생산 표현 | 농부 semantic waypoint·최소 animation, 파종·S/M/L 생육, 수확·감자 cargo | deterministic seed·rule revision과 cargo lineage를 보존한 작은 vertical slice별 test |
| P9 | FARM-6 이후 공급망 폐루프 | 농장 출하→운송→입고→후방재고→진열→공동수령, 이후 필요한 Operational projection | Simulation과 Operational을 섞지 않고 각 canonical 재조회·권한·오류 경계를 검증 |

### P0에서 반드시 남길 작업 분류

```text
재사용
  Farm 6×6 Tile·선택·Projector
  Logistics facility·truck VisualRoot
  Urban Market surface·Concept Card
  Residential pickup View

새 구현 후보
  통합 Diorama camera/focus
  Macro World Bootstrap과 Zone anchor
  asset inventory 기반 Presentation catalog
  cross-zone cargo lineage presentation

후속으로 유지
  FARM-2 Confirm/Tick
  NPC animation 완성
  Android 최적화
  Operational command 연결
```

P0~P6은 시각적 World 기반을 닫는 작업이며 Domain·Simulation 구조를 asset에 맞게 바꾸지 않는다. P6 완료 뒤 계절·낮밤·대규모 날씨·streaming·추가 interior를 시작하지 않고 P7 FARM-2로 이동한다. 도심마트 Simulation과 공동주택 세부 계약은 [도심마트 공급 계약 경영 Simulation 설계](UrbanMarketSupplyManagementSimulationDesign.md)와 [도심마트 공동주택 주문자 집단 통합 설계](UrbanMarketResidentialOrdererGroupIntegrationDesign.md)를 계속 따른다.

### 현재 Gate 상태

- P0 완료: [City·Farm World P0 기준선과 Asset Inventory](UnityCityFarmWorldP0Inventory.md)에 실제 Unity project·Scene·City/Farm allowlist·URP/Renderer/Quality·Console 기준선을 기록했다.
- P1 WORLD-0 완료: asset-neutral camera state, Perspective rig, World/Zone/Object focus, pan·zoom·90도 회전과 명시적으로 표시된 foreground cutaway를 구현했다. 저장하지 않은 primitive prototype에서 Overview/Farm/Logistics/Market Game View를 확인했고 Unity EditMode 전체 29/29가 통과했다.
- P2 WORLD-1 완료: 별도 `CityFarmMacroWorldBlockout` Scene에 Farm Production→Farm Yard→Transport→Logistics→Market→Residential 6개 Presentation Zone과 5개 route, World/Zone focus anchor를 저장했다. Farm Production과 Farm Yard는 기존 canonical `farm`을 공유하며 Presentation 공간만 분리한다. 대표 Game View 4종을 확인했고 Unity EditMode 전체 33/33이 통과했다.
- P3 WORLD-2 완료: Farm·Urban·Transition `WorldVisualCatalog`와 vendor-neutral VisualKey, `WorldVisualInstanceView/VisualRoot` wrapper를 추가하고 별도 `CityFarmSyntyWorldPrototype` Scene에 allowlist 21종을 연결했다. 전용 Global Volume profile을 사용하되 기존 PC/Mobile URP Asset·Renderer는 수정하지 않았다. 대표 Game View 4종과 shader/prefab reference를 확인했고 Unity EditMode 전체 36/36이 통과했다.
- P4 WORLD-3 완료: 별도 `CityFarmBusinessViewIntegration` Scene에서 기존 Farm 6×6 Tile, Logistics facility, Urban Market shelf·Concept Card, Residential pickup View를 WORLD-2 wrapper에 연결했다. `WorldPresentationFallbackView`는 Synty child와 primitive만 교체하며 업무 View와 stable ID는 유지한다. 전용 5/5와 Unity EditMode 전체 41/41이 통과했다.
- P5 WORLD-4 완료: 별도 `CityFarmCargoJourney` Scene에서 기존 handoff의 `cargo:transport-71`을 Farm Yard·Transport·Urban Logistics·Urban Market 네 anchor가 공유한다. origin·product·cargo·handoff·transport task·inbound task의 6개 source lineage를 보존하며, handoff가 물류센터 도착까지만 증명하므로 Market은 `Planned`로 유지한다. 전용 6/6과 Unity EditMode 전체 47/47이 통과했다.
- P6 WORLD-5 완료: 별도 `CityFarmVisualQualityGate` Scene에서 Zone distance를 Game View 비교 근거로 26으로 확정하고, 읽히지 않는 3D evidence text를 숨긴 뒤 동일 Cargo Journey를 읽는 camera-space HUD로 대체했다. shader·vendor prefab·missing script 검사, 대표 PNG 4종, PC/Mobile URP 차이와 Editor 기본 profiling을 기록했다. 전용 5/5와 Unity EditMode 전체 52/52가 통과했다.
- Visual 강제 중단 예외: 사용자 요청으로 기존 Farm·City Pack만 사용하는 P7.5를 한정 수행했다. 계절·낮밤·날씨·streaming·추가 interior·새 Zone은 여전히 시작하지 않는다.
- P7 FARM-2 완료: 기존 6×6 snapshot·Projector·stable-ID View를 재사용해 선택→Preview→명시적 Confirm→Simulation Tick→새 Snapshot→Reconcile→Dirt Row를 연결했다. Preview·Confirm은 revision 1을 유지하고 Tick만 revision 2의 새 snapshot과 `Tilled` 상태를 반환한다. core 10/10, Farm View 6/6, Unity EditMode 전체 55/55가 통과했다.
- P7.5 완료: 별도 `FarmCityGraphicalShowcase` Scene과 Environment Catalog의 Farm 263·City 88 Wrapper를 다시 열어 검증했다. 전용 4/4·기존 전체 64/64, 대표 캡처 4종, Environment 351 instance·370 renderer와 Console Error 0·Scene dirty false를 확인했다.
- P7.6 CMP1·ANIM0 완료: additive 공통 Composition descriptor·connector·socket·A/B/C signature validator와 기존 Farm adapter를 추가하고, Synty clip/controller 0·Humanoid rig 5·Town missing controller 8·FX 11/2/17을 코드로 검출했다. 집중 8/8과 열린 Editor 전체 EditMode 72/72가 통과했다.
- P7.6 CMP2 완료: Town House 12개를 포함한 세 Pack source 42개의 bounds·pivot·긴 축·문 방향·shader·scale·collider·LOD를 Editor 검사기로 측정했다. Town·City는 5m grid를 확인했고 Farm Dirt Road 직선 11.9106m를 10m grid에 연결할 때 오차 1.9106m와 중심 adapter offset `(0, 0, -0.9553)`을 기록했다. 결합 mesh에서 문 방향을 확인할 수 없는 Town House 12개는 `unknown`으로 보존했다. 집중 4/4와 전체 EditMode 76/76이 통과했다.
- P7.6 CMP3 완료: 세 Pack 도로 12개와 Region/Hub Gate 10개, 총 A형 prefab 22개를 생성했다. 사람·차량 경계 4쌍과 Farm/Town→Hub→City 화물 3쌍을 route signature로 분리하고, builder 2회 실행 `22 → 22`, 90도 회전·tile 중첩·nested Synty prefab·Farm offset을 검증했다. 집중 6/6·전체 EditMode 82/82, Console Error 0과 Preview Game View를 확인했다.
- P7.6 CMP4 완료: Farm 실제 감자 6×6 필지·Town 기본주택·City 공동주택 가로형·Regional Logistics Hub Dock A형 각 1종을 생성하고 CMP3 도로/Gate와 반대 방향·동일 route signature로 접속했다. 실제 감자밭은 environment 36칸과 simulation-target socket만 가지며 기존 상태 View를 복제하지 않는다. 출입구 원본/설계 방향, 차량 회전반경, occlusion root, actor·vehicle·cargo·interaction socket을 고정했고 builder 3회 결과 `4 → 4 → 4`, 집중 6/6·전체 EditMode 88/88과 Preview Game View를 확인했다.
- P7.6 CMP4-A·ANIM1 및 ANIM2 fallback 기준선 완료: asset-neutral Idle/Walk key·intent·source kind·fallback catalog와 adapter를 추가했다. Farm·Town·City 대표 Humanoid 각 1명은 같은 계약으로 별도 route follower를 따라 이동하며 root motion은 꺼져 있다. 실제 clip/controller 0과 Town missing controller 8개를 진단하고 procedural fallback을 명시했으므로 이를 검증된 Synty clip 리타기팅으로 간주하지 않는다. 집중 6/6·전체 EditMode 94/94, Console Error 0과 Play Mode Game View를 확인했다.
- P7.6 CMP5·ANIM4 차량 이동 기준선 완료: Farm·Town·City·Hub A형 anchor 4개와 CMP3 Gate 10개, 사람 Journey 2개, 화물 Journey 2개를 한 저장 Scene에 조립했다. 기존 감자 cargo identity와 6개 lineage는 Hub 보관까지 재사용하고, 별도 Town cargo만 명시적 outbound allocation source가 있을 때 City 차량 이동을 허용한다. 위치·animation tick은 stage나 lineage를 바꾸지 않는다. 집중 7/7·전체 EditMode 101/101과 Play Mode Overview를 확인했으며 최종 경관 밀도·연속 도로 품질은 아직 미완료다.
- 다음 Gate: `CMP6`에서 실제 감자 한 품목만 선택해 Farm·Hub·City anchor의 상품·가격 Concept Card를 연결한다. prefab은 가격·수량을 계산하지 않으며 source·시각·단위·통화·mapping 상태를 API projection에서 받아야 한다.

## 4. 통합 구현 우선순위

### P0. 현재 도심 물류센터 slice를 실제 연결까지 닫기

이미 server Role/NPC projection과 Unity Repository·UseCase, primitive Zone 배선이 있으므로 가장 적은 추가 비용으로 전체 아키텍처를 검증할 수 있다.

1. Role Perspective와 NPC movement용 `UnityWebRequest` adapter를 작성한다.
2. API base URL, 인증과 cancellation을 application `LifetimeScope`에 등록한다.
3. server JSON과 Unity ApiModel의 직렬화 호환 test를 추가한다.
4. 물류센터 NavMesh를 bake하고 기본 Animator Controller를 연결한다.
5. canonical 상태 재조회 시 같은 NPC stable ID가 새 route로 증분 갱신되는지 확인한다.

2026-08-08 현재 1~3의 코드 기반은 구현됐다. 실제 제품 Unity 프로젝트가 저장소에 없어 login session 공급과 4~5의 runtime 확인은 남아 있다. operational adapter는 인증 실패나 API 실패를 simulated fixture로 대체하지 않는다.

완료 기준은 실제 서버의 배정 운송 하나가 물류센터 Role target과 NPC 이동으로 함께 표시되고, NPC 도착이 운영 상태를 변경하지 않는 것이다.

현재 코드에는 이 slice의 다음 연결도 포함되어 있다.

```text
운송중
  → 운송 NPC: 물류센터 거점 → 창고 거점
하차지도착 + 입고 운송중
  → 운송 NPC + 창고 입고작업자 NPC: warehouse.inbound-dock 집결
입고완료
  → 운송 NPC: warehouse.vehicle-exit
  → 입고작업자 NPC: warehouse.storage-zone
```

화물은 `cargo:transport-{id}`, 업무는 `transport-task:{id}`와 `inbound-task:{id}`로 분리해 추적한다.

### P1. 공공데이터 정보관

제품의 기본 공개 범위인 0.0과 가장 잘 맞고 개인정보 위험이 낮다. 기존 `community/world-map/observations` aggregate와 123개 마커 검증 경험을 재사용한다.

지역 인구·실제 수요·물류 접근성을 별도 Layer로 확장하는 계약과 순서는 [지역 인구·수요 World Layer 제안](RegionalPopulationDemandWorldLayerProposal.md)을 따른다. 인구통계와 운영 주문을 같은 의미로 합치지 않고, 운영 수요는 권한과 개인정보 집계를 통과한 별도 Projection으로 둔다.

- 공개 관측 ScreenModel과 Repository adapter
- marker freshness·source·기준시각 표현
- 정보키오스크와 상세 근거 panel
- initial load/refresh failure 및 stable-ID reconcile

2026-08-08 현재 Unity ApiModel·Mapper·Repository·UseCase, stable-ID reconcile, 마지막 성공 유지 정책과 primitive 정보관 View까지 구현됐다. 실제 제품 Unity 프로젝트에서 공개 API marker 수와 위치 표현을 확인하는 runtime 검증이 남아 있다.

### P2. 커뮤니티·시장 광장

공개 정보가 공동행동으로 이어지는 제품 중심 공간이다. 게시판 글을 개별 3D 객체로 대량 복제하지 않고 게시판·원장 보드·상세 panel로 압축한다.

- 공개 게시판 projection
- 커뮤니티 활동 공개 범위
- 관심·참여·실행 상태의 명시적 분리
- 민감 상세와 실행은 Web handoff 또는 확인된 server Command

2026-08-08 현재 기존 공개 게시판·게시글·비식별 활동 신호를 결합하는 server aggregate와 Unity ApiModel·Mapper·Repository·UseCase, stable-ID reconcile, primitive 광장 View까지 구현됐다. 공개 계약에는 작성자 식별자·연락처·댓글 본문·원장 ID·담당자·실행 행동이 포함되지 않는다. 실제 제품 Unity 프로젝트에서 operational API 표시를 확인하는 runtime 검증은 남아 있다.

### P3. 창고·재고

서버에 canonical Entity와 업무 상태가 존재하고 도심 물류센터의 Dock·팔레트·NPC 구조를 가장 많이 재사용할 수 있다.

- authorized warehouse snapshot
- 팔레트·상품상자·입고·피킹·출고 View
- picker와 dock worker NPC
- 재고수량, 작업상태, viewer scope의 서버 판정

2026-08-08 현재 기존 `재고현황UseCase`, `적재작업UseCase`, `피킹작업UseCase`의 권한 필터 결과를 결합하는 `WarehouseManager` 전용 server aggregate와 Unity ApiModel·Mapper·Repository·UseCase가 구현됐다. 재고·작업·NPC 참조 무결성, stable-ID reconcile, 마지막 성공 유지 정책과 팔레트·작업 표식·DockWorker·Picker primitive socket까지 연결했다. 실제 제품 Unity 프로젝트의 NavMesh bake와 operational API runtime 검증은 남아 있다.

### P4. 운송 World 연결

물류센터 안의 NPC에서 농장·창고·마트·공동수령지 사이의 노드 이동으로 확장한다. 초기에는 실제 도로 시뮬레이션 대신 server가 결정한 출발·목적지와 semantic route만 표시한다.

- transport corridor node와 TruckView
- 운송자 다음 작업·상차·하차 Role View
- canonical 재조회와 stale 처리
- 주소·연락처는 배정 업무에 필요한 서버 projection 범위만 사용

2026-08-08 현재 기존 `warehouse-handoff` API의 `InTransit` 상태와 Transporter movement를 재사용해 `TransportCorridorSnapshot`으로 투영한다. `TruckMovementApplicator`는 truck stable ID와 revision을 검사하고, 도심 물류센터 sample은 `network.logistics-center`에서 `network.warehouse`로 향하는 NavMeshAgent TruckView와 cargo VisualRoot를 제공한다. 도착·입고 완료는 Unity가 확정하지 않으며 서버 handoff 재조회 결과가 운송중이 아니면 TruckView를 숨긴다. 임시 Unity 6 프로젝트에서 script compile, primitive scene 생성과 scene reload 후 truck wiring을 확인했다. 실제 제품 Unity 프로젝트의 NavMesh bake와 operational runtime 검증은 남아 있다.

### P5. 도심마트 operational + 주문자 관점

현재 primitive View를 살리되 상품·가격·재고를 각각 독립 API로 조립하지 않고 공개 가능한 마트 aggregate를 먼저 정의한다.

- 마트 상품·가격·재고·출처 snapshot
- 주문자 Role target과 상세 panel
- 주문 생성은 확인 panel → server UseCase → canonical 재조회

2026-08-08 현재 기존 공개 aggregate `GET api/v1/orderer/mart/products`를 Unity ApiModel·Mapper·Repository·operational UseCase로 연결했다. 서버가 공개한 판매가·판매 가능 수량·재고 기준시각을 그대로 사용하고 내부 창고 원문을 추론하지 않는다. 주문자 관점은 판매 가능 상품 탐색과 읽기 전용 상세 panel까지만 허용하며 주문 Command는 포함하지 않는다. VContainer에서 simulation/operational 모드를 명시적으로 선택하고 operational 실패를 fixture로 대체하지 않는다. 임시 Unity 6 프로젝트에서 operational adapter compile, primitive scene 생성과 reload wiring을 확인했으며 실제 API runtime 표시는 남아 있다.

### P6. 주거공동체 수령

주문자와 운송자가 같은 장소를 다르게 보는 첫 개인정보 민감 slice다. 다른 세대 정보, 상세 주소와 연락처가 client 필터에 의존하지 않도록 server projection을 먼저 고정한다.

- 공동수령지 World View
- 주문자 본인 수령 상태
- 운송자의 현재 배정 하차 대상
- 배송 증빙과 완료 Command의 권한·revision 검증

2026-08-08 현재 기존 `unloading-perspectives`의 주문자 본인·운송담당자 배정 관계 필터를 재사용하고, Unity에는 공동수령 point·상품 요약·상태·canonical task만 제공하는 `ResidentialPickupPerspective`를 추가했다. 주문자와 운송자는 역할 선택 파라미터가 없는 별도 인증 route를 사용한다. 주소·상세주소·연락처·사용자 ID·주문번호·결제·계약 정보는 계약에서 제외했다. Unity는 같은 point를 `내 수령 상품` 또는 `내 하차 대상`으로 표현하며 현재 interaction은 읽기 전용이다. 임시 Unity 6 프로젝트에서 operational adapter compile, 역할 전환 socket, primitive scene 생성과 reload wiring을 확인했다. 실제 API runtime과 수령 확인·하차 완료 Command는 남아 있다.

공동주택 같이 주문 playable에서는 P6 앞단에 `ResidentialGroupRepresentative` NPC를 추가한다. 이는 주민자치 대표 등 사회적 context의 표현이며 기존 `공동구매 대표` 역할 검증을 대체하지 않는다. 현재 route catalog에는 해당 actor가 없으므로 RG4-NPC에서 다음 두 leg를 additive하게 추가한다.

- 주거공동체: `community office → community board → departure point`
- 도심마트: `market entrance → manager desk → exit`

두 Zone을 하나의 `NpcMovementSnapshot` route로 합치지 않고 representative visit state가 leg를 연결한다. 도착·대화·Animator event는 문의·주문·계약·발주·수령 Command를 실행하지 않는다.

### P7. 농장·생산자 관점

운영 Farm·Plot·Cultivation·Sensor Projection은 실제 농장 관측과 작업 권위를 유지한다. 타일 기반 농사 playable은 이 계약을 좌표 격자로 변형하지 않고 별도 `Simulation` snapshot으로 구성한다.

1. FARM-0: 타일 stable ID·좌표·토양 profile·수분 관측 상태·경작 상태·작업 참조 계약과 무결성 검증
2. FARM-1: 6×6 asset-neutral 토양 Grid, stable-ID 선택과 Projector 결정 color token·상세 설명
3. FARM-2: 밭갈이 Preview → 명시적 Confirm → 작업 생성 → Simulation Tick → 타일 상태 갱신
4. FARM-3: 기존 생산자 NPC waypoint·NavMeshAgent에 밭갈이 작업 표현 연결. NPC 도착은 완료 권위가 아님
5. FARM-4: SeedLot 재고, 파종 Preview·Confirm과 경작·수량 무결성
6. FARM-5: scenario seed·rule revision 기반 발아·생육·관수·수확 결정적 Tick
7. FARM-6: 수확물 → 농장 포장장 → 화물 → 물류센터 handoff 연결
8. FARM-7: Farm asset `VisualRoot`, Humanoid Animator, 실제 Scene·Game View와 성능 검증

공공·운영 토양 관측을 Simulation 수확량으로 직접 환산하지 않는다. Operational API 실패 시 Simulation fixture fallback도 금지한다.

2026-08-09 현재 `농장` root 아래 `농장구획`, `재배작기`, `농업센서`, `농업센서관측`과 `농장작업` canonical aggregate 및 EF migration을 추가했다. 생산자 API는 인증 사용자가 소유한 농장만 반환하며 공개 작물 기준 ID·출처와 실제 생육 상태를 분리한다. 센서는 원시값·단위·기준시각과 서버 판정 상태·규칙 revision·근거 card·한계를 함께 제공하고 위치·주소·소유자 ID는 제외한다. Unity에는 Repository·UseCase, FarmTile·Crop·Sensor View, canonical 농장작업을 참조하는 생산자 NavMeshAgent socket과 VContainer primitive sample을 연결했다. FARM-0~FARM-1에서 별도 감자 Simulation 6×6 토양 타일 계약·무결성·Projector와 선택 View를 추가하고 실제 imported sample EditMode를 검증했다. 실제 sensor ingestion, 운영 DB migration 적용, 인증 API runtime, 밭갈이 작업 폐루프, NavMesh bake와 Animator Controller는 남아 있다.

### P8. 협동조합·공동원장 공간

커뮤니티에서 형성된 공동행동을 원장 보드와 회의 테이블로 투영한다. 참여 의향, 가원장, 실원장과 실행 권한을 한 상태로 합치지 않는다.

### P9. Synty 교체와 성능 최적화

모든 주요 View는 `VisualRoot`, `Animator`, `Renderer`, 장착점 같은 socket을 유지한다. 최소 환경·캐릭터 팩으로 URP, 스케일, Windows·Android 성능을 검증한 뒤 placeholder 외형만 교체한다.

## 5. 병렬로 보지 말아야 할 세 순서

- **개발 완결 순서**는 P0 도심 물류센터가 먼저다. 이미 구현된 server→Unity→NPC 흐름을 실제 adapter와 runtime까지 닫아 공통 패턴을 증명한다.
- **현재 도심마트 playable 순서**는 SC1-C → RG1~RG3 → SC2 → RG4+SC3~SC5 → SC6~SC7이다. Operational은 그 뒤 RG5~RG7+SC9에서 기존 원장을 재사용한다.
- **제품 공개 순서**는 P1 공공데이터 정보관과 P2 커뮤니티 광장이 먼저다. 현재 0.0 공개 범위와 제품 중심에 맞기 때문이다.

따라서 물류센터를 먼저 기술적으로 완결한다고 해서 3.5 기능을 기본 공개하거나 유상 운송·자동 배차를 활성화하는 것은 아니다.

## 6. 공통 완료 기준

- server가 권한·공개 범위·canonical 상태의 최종 권위다.
- Unity API model은 server DTO assembly를 직접 공유하지 않는다.
- stable ID, revision, source, 기준시각과 operational/simulation 구분이 보존된다.
- Controller는 DTO, NavMesh 좌표와 개별 Renderer를 직접 해석하지 않는다.
- Role View는 server가 허용한 대상만 강조한다.
- NPC 이동과 animation은 Presentation이며 운영 Command가 아니다.
- 상태 전이는 명시적 확인과 server 검증 뒤 같은 aggregate를 재조회한다.
- placeholder에서 계약과 runtime을 확인한 뒤 외부 asset을 도입한다.
