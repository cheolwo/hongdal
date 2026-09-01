# Graph Map 계획 조회

> 이 문서는 파일 기반 계획 그래프의 생성 조회다. ReferenceAvailable은 기존 실제 E5 공간 사본의 식별자를 확인했다는 뜻이며, 이번 작업의 Unity Scene 배치·Play Mode 이동·입력·결과 또는 E5/E6 승격 증거가 아니다.

- 그래프 맵: graph-map:mirror:northern-life-hub-discovery.v1
- 판본: mirror-graph-map-plan.northern-life-hub-discovery.r3
- 원본: [eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json](../../../eng/world-seedbeds/graph-maps/northern-life-hub-discovery.v1.json)
- 원본 SHA-256: 665b1b34ca6545d1b34a69af5fec8c6470da01eae836b4c0f65de053e0eb2e0b
- 기준 공간 사본: simulation-world-actual-e5-spatial-output.r6 / AreaSet 4 / Graph 19 / 직접 결속 42
- 기준 WI: simulation-world-interactions.r43 / 105개
- federation: 하위 맵 5 / port 8 / connector 4
- 이동 능력 프로필: 6 / 오버레이 2
- 레벨 3 코드 결속: 6 / 소스 파일 13 / 실제 결속 미검증 대상 2
- 이번 실제 Runtime 검증: false

## 규모 계층 — 하위 맵과 연결 포트

| 하위 맵 | 책임 | 노드 | 내부 엣지 | 제약 | 포트 |
| --- | --- | ---: | ---: | ---: | ---: |
| gm-subgraph:nature-discovery<br>Nature 발견·Farm 경계 | NatureWorldPlanning / AreaBoundary | 2 | 1 | 0 | 1 |
| gm-subgraph:farm-production<br>Farm 생산·집하·상차 | FarmWorldPlanning / IndependentArea | 3 | 2 | 1 | 2 |
| gm-subgraph:hub-logistics<br>Hub 입고·차량·출고 | HubWorldPlanning / IndependentArea | 3 | 2 | 1 | 3 |
| gm-subgraph:town-life<br>Town 시장·생활 | TownWorldPlanning / IndependentArea | 2 | 1 | 0 | 1 |
| gm-subgraph:yodong-gateway<br>요동성 방비 관문 | YodongPlanning / UnresolvedExternal | 1 | 0 | 0 | 1 |

| connector | from → to | Graph Map 엣지 | 필요 능력 | 상태 |
| --- | --- | --- | --- | --- |
| gm-connector:nature-farm | gm-port:nature-farm-edge:to-farm → gm-port:farm-production:from-nature | gm-edge:farm-edge-to-production | Discovery | ReferenceAvailable |
| gm-connector:farm-hub | gm-port:farm-loading:to-hub → gm-port:hub-receiving:from-farm | gm-edge:farm-loading-to-hub-receiving | Cargo | ReferenceAvailable |
| gm-connector:hub-town | gm-port:hub-outbound:to-town → gm-port:town-market:from-hub | gm-edge:hub-outbound-to-town-receiving | Cargo, TownMarket | ReferenceAvailable |
| gm-connector:hub-yodong | gm-port:hub-outbound:to-yodong → gm-port:yodong:from-hub | gm-edge:hub-outbound-to-yodong-gateway | ExternalUnresolved | Unresolved |

## 레벨 1 — 플레이 관계

```mermaid
flowchart LR
    N0["Nature 숲길 입구"]
    N1["숲 가장자리·농장 외곽"]
    N2["Farm 생산 구역"]
    N3["Farm 작업마당"]
    N4["Farm 상차 관문"]
    N5["Hub 입고·보관 접점"]
    N6["Hub 차량 마당"]
    N7["Hub 출고 접점"]
    N8["Town 시장 입고 접점"]
    N9["Town 생활 광장"]
    N10{{"요동성 방비 외부 관문<br/>미해결"}}
    N0 <-->|Traversal| N1
    N1 <-->|DiscoverySightline| N2
    N2 <-->|WorkHandoff| N3
    N3 <-->|Logistics| N4
    N4 -->|Logistics| N5
    N5 <-->|Traversal| N6
    N6 <-->|WorkHandoff| N7
    N7 -->|Logistics| N8
    N8 <-->|WorkHandoff| N9
    N7 -.->|ExternalGateway| N10
```

| 노드 | 역할 | 실현 상태 | WI | 실제 공간 참조 |
| --- | --- | --- | --- | --- |
| gm-node:nature-trailhead<br>Nature 숲길 입구 | DiscoveryEntry | ExistingActualGraphRef / ReferenceAvailable | WI-NATURE-01, WI-NATURE-02 | landscape-graph:sim:pyeongchang:nature-trail-network.v1<br>node:actual-e5:nature-trail-network:space:nature-trail-shelter:nature-trailhead |
| gm-node:nature-farm-edge<br>숲 가장자리·농장 외곽 | NatureFarmThreshold | ExistingActualGraphRef / ReferenceAvailable | WI-NATURE-01, WI-NATURE-04 | landscape-graph:sim:pyeongchang:highland-farm.v1<br>node:actual-e5:highland-farm:space:forest-edge-farm:nature-farm-edge |
| gm-node:farm-production<br>Farm 생산 구역 | CropProduction | ExistingActualGraphRef / ReferenceAvailable | WI-FARM-01, WI-FARM-02, WI-FARM-03, WI-FARM-04 | landscape-graph:sim:pyeongchang:highland-farm.v1<br>node:actual-e5:highland-farm:space:forest-edge-farm:farm-production |
| gm-node:farm-work-yard<br>Farm 작업마당 | HarvestCollectionAndPacking | ExistingActualGraphRef / ReferenceAvailable | WI-FARM-05, WI-FARM-06 | landscape-graph:sim:pyeongchang:farm-processing-campus.v1<br>node:actual-e5:farm-processing-campus:space:farm-wash-sort-pack:farm-work-yard |
| gm-node:farm-loading-gate<br>Farm 상차 관문 | FarmCargoExit | ExistingActualGraphRef / ReferenceAvailable | WI-LOG-01, WI-LOG-02, WI-LOG-03 | landscape-graph:sim:pyeongchang:farm-processing-campus.v1<br>node:actual-e5:farm-processing-campus:space:farm-processing-shipping:farm-loading-gate |
| gm-node:hub-receiving-storage<br>Hub 입고·보관 접점 | HubInboundContact | ExistingActualGraphRef / ReferenceAvailable | WI-LOG-04, WI-LOG-05, WI-HUB-03, WI-HUB-04 | landscape-graph:sim:pyeongchang:jinbu-hub.v1<br>node:actual-e5:jinbu-hub:space:hub-inbound-storage:hub-receiving-storage |
| gm-node:hub-vehicle-yard<br>Hub 차량 마당 | HubObservationAndVehicleAccess | ExistingActualGraphRef / ReferenceAvailable | WI-HUB-06 | landscape-graph:sim:pyeongchang:hub-fulfillment-operations.v1<br>node:actual-e5:hub-fulfillment-operations:space:hub-outbound-vehicle:hub-vehicle-yard |
| gm-node:hub-outbound-staging<br>Hub 출고 접점 | HubOutboundContact | ExistingActualGraphRef / ReferenceAvailable | WI-HUB-05, WI-HUB-06, WI-MARKET-01 | landscape-graph:sim:pyeongchang:hub-fulfillment-operations.v1<br>node:actual-e5:hub-fulfillment-operations:space:hub-outbound-vehicle:hub-outbound-staging |
| gm-node:town-market-receiving<br>Town 시장 입고 접점 | TownMarketInbound | ExistingActualGraphRef / ReferenceAvailable | WI-MARKET-02, WI-MARKET-03, WI-MARKET-04 | landscape-graph:sim:pyeongchang:town-market-fulfillment.v1<br>node:actual-e5:town-market-fulfillment:space:town-market-receiving:town-market-receiving |
| gm-node:town-living-square<br>Town 생활 광장 | TownResidentContact | ExistingActualGraphRef / ReferenceAvailable | WI-MARKET-05, WI-ORDER-01, WI-ORDER-06 | landscape-graph:sim:pyeongchang:town-market-fulfillment.v1<br>node:actual-e5:town-market-fulfillment:space:market-life-commerce:town-living-square |
| gm-node:yodong-defense-gateway<br>요동성 방비 외부 관문 | FutureStoryGateway | PlanningGateway / Unresolved |  | 없음 |

| 엣지 | 종류·의도 | 이동 능력 | 상태 | 방향 | 이유 |
| --- | --- | --- | --- | --- | --- |
| gm-edge:nature-trail-to-farm-edge<br>gm-node:nature-trailhead → gm-node:nature-farm-edge | Traversal / Required | gm-capability:walk-discovery | ReferenceAvailable | 양방향 | 발견 장면의 접근과 압박 없는 복귀를 함께 보존한다. |
| gm-edge:farm-edge-to-production<br>gm-node:nature-farm-edge → gm-node:farm-production | DiscoverySightline / Optional | gm-capability:discovery-sightline | ReferenceAvailable | 양방향 | Farm을 발견해도 반드시 진입하거나 소유할 필요는 없다. |
| gm-edge:farm-production-to-work-yard<br>gm-node:farm-production → gm-node:farm-work-yard | WorkHandoff / Required | gm-capability:work-handoff | ReferenceAvailable | 양방향 | 수확 결과와 집하·포장 준비를 같은 공간으로 오인하지 않고 인계한다. |
| gm-edge:farm-work-yard-to-loading-gate<br>gm-node:farm-work-yard → gm-node:farm-loading-gate | Logistics / Required | gm-capability:local-cargo | ReferenceAvailable | 양방향 | 작업마당·정비 여유·상차 관문을 순서 있는 화물 동선으로 읽는다. |
| gm-edge:farm-loading-to-hub-receiving<br>gm-node:farm-loading-gate → gm-node:hub-receiving-storage | Logistics / Optional | gm-capability:inter-area-cargo | ReferenceAvailable | 단방향 | Farm과 Hub는 독립 실행을 유지하며 승인된 화물이 있을 때만 선택적으로 연결한다. |
| gm-edge:hub-receiving-to-vehicle-yard<br>gm-node:hub-receiving-storage → gm-node:hub-vehicle-yard | Traversal / Required | gm-capability:walk-discovery | ReferenceAvailable | 양방향 | Hub의 입구·접점·출구를 한 화면에 뭉개지 않고 현장 이동과 광역 조회를 연결한다. |
| gm-edge:hub-vehicle-yard-to-outbound<br>gm-node:hub-vehicle-yard → gm-node:hub-outbound-staging | WorkHandoff / Required | gm-capability:work-handoff | ReferenceAvailable | 양방향 | 차량 접근과 실제 출고 대기 상태를 분리한다. |
| gm-edge:hub-outbound-to-town-receiving<br>gm-node:hub-outbound-staging → gm-node:town-market-receiving | Logistics / Optional | gm-capability:inter-area-cargo | ReferenceAvailable | 단방향 | Hub와 Town의 독립 업무를 유지하면서 확정된 출고만 운송 관계로 넘긴다. |
| gm-edge:town-receiving-to-living-square<br>gm-node:town-market-receiving → gm-node:town-living-square | WorkHandoff / Optional | gm-capability:work-handoff | ReferenceAvailable | 양방향 | 후방 입고와 주민이 보는 시장·생활 접점을 구분한다. |
| gm-edge:hub-outbound-to-yodong-gateway<br>gm-node:hub-outbound-staging → gm-node:yodong-defense-gateway | ExternalGateway / Unknown | gm-capability:unresolved-external | Unresolved | 단방향 | 보급·제작·전투 기록이 요동성 방비에 이어지는 방향만 있으며 실제 공간·WI·경로는 아직 없다. |

### 이동 능력 프로필

| 프로필 | Actor | 화물 | 차량 | 권위 근거 | 귀환 정책 |
| --- | --- | --- | --- | --- | --- |
| gm-capability:walk-discovery<br>도보 발견·복귀 | Player, Npc | None | None | False | RequiredWhenTraversalIsRequired |
| gm-capability:discovery-sightline<br>시야 단서 기반 발견 | PlayerView | None | None | False | NotApplicable |
| gm-capability:work-handoff<br>작업 인계 | Player, Npc, Worker | HandCarryCandidate | None | True | WorkResultCanReturnToSource |
| gm-capability:local-cargo<br>Area 내부 화물 이동 | Player, Npc, Worker | CargoRequired | OptionalCandidate | True | ReturnOrHoldRequired |
| gm-capability:inter-area-cargo<br>Area 사이 선택형 화물 이동 | Worker, Vehicle | CargoRequired | RequiredCandidate | True | IndependentAreasRemainRunnable |
| gm-capability:unresolved-external<br>미해결 외부 관문 |  | Unknown | Unknown | True | Unresolved |

## 레벨 2 — 배치 전 제약

| 제약 | 분류 | 심각도 | 집행 | 필요 E | 실패 코드 | 규칙 |
| --- | --- | --- | --- | --- | --- | --- |
| gm-constraint:actual-reference-identity | Provenance | Blocking | Static | E4 | ActualReferenceIdentityInvalid | ExistingActualGraphRef 노드는 같은 AreaSet·Graph 안의 실제 Node ID를 가져야 하며 판본이 사라지면 검토를 중단한다. |
| gm-constraint:required-traversal-return | Traversal | Blocking | Static | E4 | RequiredTraversalReturnMissing | 필수 플레이어 이동은 양방향이거나 별도 귀환 엣지를 가져야 한다. 화물의 단방향 흐름을 플레이어 귀환으로 대체하지 않는다. |
| gm-constraint:unresolved-never-verified | Evidence | Blocking | Static | E4 | UnresolvedTargetPromoted | 미해결 관문과 연결은 실제 이동·배치·WI가 결속되기 전 ReferenceAvailable이나 Verified로 올리지 않는다. |
| gm-constraint:farm-flow-separation | WorkAndCargo | Blocking | StaticAndHumanReview | E5 | FarmFlowSeparationInvalid | 생산, 집하·포장, 상차를 서로 다른 역할로 유지하고 완료 상태와 물리 위치를 같은 것으로 취급하지 않는다. |
| gm-constraint:hub-entry-contact-exit | Readability | Blocking | StaticAndHumanReview | E5 | HubReadabilityInvalid | Hub의 입구·접점·출구 내역을 구분해 읽을 수 있어야 하며 3인칭과 광역 시점이 같은 상태 사본을 소비해야 한다. |
| gm-constraint:route-capability-separation | RouteCapability | Blocking | StaticAndPlayMode | E5 | RouteCapabilityInvalid | 보행·화물·차량 접근 능력을 분리하고 그래프 연결을 실제 Collider 통행이나 차량 운행 성공으로 확대하지 않는다. |
| gm-constraint:season-does-not-rewrite-topology | TimeAndPresentation | Advisory | Static | E4 | SeasonTopologyMutation | 절기·날씨·Sky 표현은 발견 난도와 후보 표현에 영향을 줄 수 있지만 승인된 WI·경로·권위 상태를 조용히 바꾸지 않는다. |
| gm-constraint:asset-candidate-not-assignment | PresentationCandidate | Blocking | Static | E4 | CandidatePromotedWithoutBinding | Synty Prefab과 이미지 후보는 노드 역할을 설명하는 후보이며 E4 지문·실측·배치 검증 전 실제 할당으로 기록하지 않는다. |
| gm-constraint:no-whole-map-prerequisite | IndependentArea | Blocking | Static | E4 | IndependentAreaPrerequisiteInvalid | Farm·Hub·Town은 독립 폐루프를 먼저 유지하며 연결 엣지가 없거나 미완료여도 각 영역의 독립 검증을 막지 않는다. |

### 시간·날씨 오버레이

| 오버레이 | 계기 | 대상 하위 맵 | 효과 범주 | 토폴로지·권위 변경 |
| --- | --- | --- | --- | --- |
| gm-overlay:spring-equinox<br>춘분 기본 검토 오버레이 | SeasonalTerm | gm-subgraph:nature-discovery, gm-subgraph:farm-production | DiscoveryReadability, LandscapePaletteCandidate, CropAvailabilityContext | false / false |
| gm-overlay:weather-discovery-visibility<br>날씨에 따른 발견 판독 오버레이 | WeatherState | gm-subgraph:nature-discovery | DiscoveryDifficulty, SightlineReadability, SkyAndLandscapePresentation | false / false |

## 레벨 3 — Unity 코드·Component 결속

> 레벨 3은 코드 본문을 복제하지 않는다. 공용 코드 결속 대장에서 파일·assembly·SHA-256·심볼을 한 번만 관리하고, 이 맵은 대상 selector만 소유한다. SourceAndSymbolVerified는 Scene wiring, Play Mode 실행, Game View 또는 E5 성립을 뜻하지 않는다.

- 코드 대장: eng/world-seedbeds/graph-maps/unity-code-bindings.v1.json / mirror-graph-map-code-binding-catalog.r1 / SHA-256 c1a2360271bbcfd76f0b29ea2291894625cb57050cf8b97d16582a4b62b1a0b0
- 소스 루트 SsalddelUnity: 관측 HEAD 094f225d55f94f16de0f8bc3edbdaf2471e19147 / canonical Scene Assets/Ssalddel/Scenes/SimulationWorldShell.unity / Scene SHA-256 D1D31BFDD9A727D1744B888D2AE25D7C275CC9E7F9A6D21EF5FB5CCBDD243271

### 실제 E5 AreaSet 네트워크 조회·전환·HUD 파이프라인

- 결속 ID: gm-code:actual-e5-network-pipeline
- 단계·사용·관계: SourceKnown / Runtime / SharedNetworkProjectionPipeline
- 대상 선택: AllResolvedNodesAndEdges / 19개
- 대상: gm-node:nature-trailhead, gm-node:nature-farm-edge, gm-node:farm-production, gm-node:farm-work-yard, gm-node:farm-loading-gate, gm-node:hub-receiving-storage, gm-node:hub-vehicle-yard, gm-node:hub-outbound-staging, gm-node:town-market-receiving, gm-node:town-living-square, gm-edge:nature-trail-to-farm-edge, gm-edge:farm-edge-to-production, gm-edge:farm-production-to-work-yard, gm-edge:farm-work-yard-to-loading-gate, gm-edge:farm-loading-to-hub-receiving, gm-edge:hub-receiving-to-vehicle-yard, gm-edge:hub-vehicle-yard-to-outbound, gm-edge:hub-outbound-to-town-receiving, gm-edge:town-receiving-to-living-square

| assembly | 소유 | 파일 | 심볼 |
| --- | --- | --- | --- |
| Ssalddel.Unity.Runtime | WorldPresentationRuntime | Assets/Ssalddel/Runtime/World/실제E5AreaSetNetworkModels.cs | 실제E5AreaSetNetworkCodes, 실제E5AreaSetNetworkData, 실제E5NetworkRelationData |
| Ssalddel.Unity.Runtime | WorldPresentationRuntime | Assets/Ssalddel/Runtime/World/실제E5AreaSetNetworkStreaming.cs | I실제E5AreaSetNetworkRepository, 실제E5AreaSetNetworkStreamingSession |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/실제E5AreaSetNetworkController.cs | 실제E5AreaSetNetworkController, InitializeAsync, SwitchAreaAsync |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/실제E5AreaSetNetworkHudPresenter.cs | 실제E5AreaSetNetworkHudPresenter, ShowRegionalCausality |

### 노드의 지형·타일·VisualKey 실제화 준비 파이프라인

- 결속 ID: gm-code:landscape-runtime-realization
- 단계·사용·관계: SourceKnown / Runtime / SharedLandscapeRealizationPipeline
- 대상 선택: AllResolvedNodes / 10개
- 대상: gm-node:nature-trailhead, gm-node:nature-farm-edge, gm-node:farm-production, gm-node:farm-work-yard, gm-node:farm-loading-gate, gm-node:hub-receiving-storage, gm-node:hub-vehicle-yard, gm-node:hub-outbound-staging, gm-node:town-market-receiving, gm-node:town-living-square

| assembly | 소유 | 파일 | 심볼 |
| --- | --- | --- | --- |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/공간문법LandscapeRuntimeAssembler.cs | 공간문법LandscapeRuntimeAssembler, 공간문법PlacementInstanceView, CommitAtomic |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/공간TileStreamingController.cs | 공간TileStreamingController, ConfigureLandscapeAssembly, TryGetTrackedWorldBounds |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/WorldVisualCatalog.cs | WorldVisualCatalog, WorldVisualCatalogEntry, Resolve |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/WorldVisualInstanceView.cs | WorldVisualInstanceView, ValidateWiring |

### Farm H1 후보와 실외 배치 계획

- 결속 ID: gm-code:farm-h-placement-plan
- 단계·사용·관계: SourceKnown / Runtime / ExactHStableIdConsumer
- 대상 선택: ExplicitRefs / 4개
- 대상: gm-node:farm-production, gm-node:farm-work-yard, gm-node:farm-loading-gate, gm-constraint:farm-flow-separation

| assembly | 소유 | 파일 | 심볼 |
| --- | --- | --- | --- |
| Ssalddel.Unity.Runtime | WorldPresentationRuntime | Assets/Ssalddel/Runtime/World/공간실외자산배치Planning.cs | I공간실외자산배치PlanProvider, 결정적공간실외자산배치PlanProvider, h1-stock:farm-loading-gate |
| Ssalddel.Unity.Runtime | WorldPresentationRuntime | Assets/Ssalddel/Runtime/World/공간LHWorldModels.cs | I공간LHWorldRepository, 로컬공간LHWorldEngine, h1-stock:farm-production |

### Nature 숲길 입구 감각 표현

- 결속 ID: gm-code:nature-trail-expression
- 단계·사용·관계: SourceKnown / Runtime / ExactHStableIdConsumer
- 대상 선택: ExplicitRefs / 1개
- 대상: gm-node:nature-trailhead

| assembly | 소유 | 파일 | 심볼 |
| --- | --- | --- | --- |
| Ssalddel.Unity.Presentation | WorldPresentationRuntime | Assets/Ssalddel/Presentation/World/Nature감각표현Presenter.cs | Nature감각표현Presenter, h1-stock:nature-trailhead, ValidateWiring |

### Farm·Hub H1 모판 Editor 검토 진입점

- 결속 ID: gm-code:wi-seedbed-editor-preview
- 단계·사용·관계: SourceKnown / EditorOnly / EditorPreviewOnly
- 대상 선택: ExplicitRefs / 4개
- 대상: gm-node:farm-production, gm-node:farm-work-yard, gm-node:farm-loading-gate, gm-node:hub-receiving-storage

| assembly | 소유 | 파일 | 심볼 |
| --- | --- | --- | --- |
| Ssalddel.Unity.Editor | WorldSpatialEditor | Assets/Ssalddel/Editor/WI공간모판검토실Builder.cs | WI공간모판검토실Builder, ShowFarmProduction, ShowFarmWorkYard, ShowFarmLoadingGate, ShowHubReceivingStorage |

### Farm H 배치 규칙 Editor 검사

- 결속 ID: gm-code:h-spatial-rule-editor
- 단계·사용·관계: SourceKnown / EditorOnly / EditorConstraintInspector
- 대상 선택: ExplicitRefs / 3개
- 대상: gm-node:farm-production, gm-node:farm-work-yard, gm-constraint:farm-flow-separation

| assembly | 소유 | 파일 | 심볼 |
| --- | --- | --- | --- |
| Ssalddel.Unity.Editor | WorldSpatialEditor | Assets/Ssalddel/Editor/H공간배치규칙EditorEngine.cs | H공간배치규칙EditorEngine, farm-production, farm-work-yard |

### 아직 Unity 코드와 결속하지 않은 대상

| 대상 | 사유 |
| --- | --- |
| gm-node:yodong-defense-gateway | NoApprovedUnityBinding — 요동성 방비 관문은 기획 방향만 있고 승인된 AreaSet·H·Unity Controller 결속이 없다. |
| gm-edge:hub-outbound-to-yodong-gateway | NoApprovedUnityBinding — Hub에서 요동성으로 이어지는 실제 경로·이동 능력·Unity 소비 코드가 아직 승인되지 않았다. |

## 현재 미해결

- 미해결 노드: 1
- 미해결 엣지: 1
- 요동성 방비 관문은 기획 방향만 있으며 실제 WI·AreaSet·Graph·경로가 없다.
- 최신 공간 사본 자체가 runtimeValidated=false이므로 실제 이동·Collider·Game View 근거로 확대하지 않는다.
- Synty 후보, 지면·통로 실측, InteractionAnchor, 입력·결과, 적용·해제는 후속 작은 실행 범위에서 별도 검증한다.
