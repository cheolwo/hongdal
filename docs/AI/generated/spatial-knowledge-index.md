# Codex 공간 설계 지식 색인

> 항목별 JSON·Markdown에서 결정적으로 생성된다. 직접 수정하지 않는다.

- H1 작업공간 지식: `52개`
- H2 블록 조립법: `24개`
- H3 지역 유형 청사진: `13개`

## H1

| 상태 | 고유 식별자 | 이름 | 검색 단서 |
| --- | --- | --- | --- |
| `ApprovedReference` | `h1-stock:farm-hub-corridor` | Farm–Hub 화물 회랑 | WI-LOG-03, Spatial.CargoRoute, Spatial.VehicleAccessible, Network, Transition |
| `ApprovedReference` | `h1-stock:farm-loading-gate` | 농장 상차·출입 공간 | WI-LOG-01, WI-LOG-02, Spatial.CargoAccessible, Spatial.LoadingWorkArea, Spatial.VehicleAccessible, Spatial.WorkerAccessible, Farm, Transition |
| `ApprovedReference` | `h1-stock:farm-production` | 농업 생산구획 | WI-FARM-01, WI-FARM-02, WI-FARM-03, WI-FARM-04, Spatial.CargoAccessible, Spatial.CropCareWorkArea, Spatial.CropProduction, Spatial.HarvestWorkArea, Spatial.SowingWorkArea, Spatial.TillingWorkArea, Spatial.WaterAccessible, Spatial.WorkerAccessible, Farm |
| `ApprovedReference` | `h1-stock:farm-work-yard` | 수확·집하 작업마당 | WI-FARM-05, WI-FARM-06, WI-WORLD-04, Spatial.CargoAccessible, Spatial.CollectionWorkArea, Spatial.PackingWorkArea, Spatial.WorkerAccessible, Farm |
| `ApprovedReference` | `h1-stock:hub-receiving-storage` | Hub 입고·검수·보관 공간 | WI-LOG-04, WI-LOG-05, WI-001, WI-002, Spatial.CargoAccessible, Spatial.InspectionWorkArea, Spatial.LoadingWorkArea, Spatial.Storage, Spatial.UnloadingWorkArea, Spatial.WorkerAccessible, City |
| `CandidateForReview` | `h1-stock:nature-emergency-retreat` | 자연권 긴급 후퇴 길목 | WI-NATURE-02, EmergencyRetreat, ThreatEvacuation, SafeCoreReturn, Spatial.Traversable, Spatial.EmergencyAccess, Spatial.PlayerEscapeRoute, Nature |
| `CandidateForReview` | `h1-stock:nature-incident-trace` | 자연권 사건 흔적 조사 구역 | WI-NATURE-01, IncidentTraceInvestigation, CauseIdentification, ThreatTracking, Spatial.Traversable, Spatial.InvestigationArea, Spatial.ThreatMonitoringArea, Nature |
| `CandidateForReview` | `h1-stock:nature-restoration-site` | 자연권 정화·복구 작업 공간 | WI-NATURE-03, NatureRestoration, ContaminationCleanup, RouteRecovery, Spatial.WorkerAccessible, Spatial.RestorationWorkArea, Spatial.CargoAccessible, Nature |
| `CandidateForReview` | `h1-stock:nature-safe-recovery-camp` | 자연권 안전 회복 야영지 | WI-NATURE-04, PartyRecovery, ThreatDebrief, NextActionPreparation, Spatial.Traversable, Spatial.RestArea, Spatial.SafeCore, Spatial.NpcWorkArea, Nature |
| `CandidateForReview` | `h1-stock:nature-threat-watch` | 자연권 위협 관찰 초소 | WI-NATURE-01, RegionalThreatObservation, NatureRouteWarning, EncounterForecast, Spatial.Traversable, Spatial.ObservationArea, Spatial.ThreatMonitoringArea, Nature |
| `ExploratoryInventory` | `h1-stock:farm-exposure-inspection` | 농장 수확물 노출 점검 공간 | HarvestExposureInspection, ContaminationAssessment, SafeHandoffDecision, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.InspectionWorkArea, Farm |
| `ExploratoryInventory` | `h1-stock:farm-harvest-staging` | 수확물 임시 적치 공간 | WI-FARM-04, WI-FARM-05, TemporaryCropStorage, WaitForPacking, WaitForVehicle, Spatial.CargoAccessible, Spatial.WorkerAccessible, Spatial.TemporaryStorage, Farm |
| `ExploratoryInventory` | `h1-stock:farm-maintenance-yard` | 농장 시설 정비 공간 | WI-WORLD-04, Farm |
| `ExploratoryInventory` | `h1-stock:farm-restoration-supply` | 농장 자연권 복구 자재 인계 공간 | RestorationSupplyHandoff, RecoveredMaterialTransfer, NatureRouteSupport, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.LoadingWorkArea, Farm, Nature |
| `ExploratoryInventory` | `h1-stock:farm-seed-preparation` | 종자 준비 공간 | WI-FARM-02, SeedInspection, SeedBatchPreparation, Spatial.WorkerAccessible, Spatial.MaterialPreparationArea, Farm |
| `ExploratoryInventory` | `h1-stock:farm-tool-storage` | 농기구 보관 공간 | WI-WORLD-04, ToolCheckout, ToolReturn, Spatial.Storage, Spatial.WorkerAccessible, Farm |
| `ExploratoryInventory` | `h1-stock:farm-weather-protection` | 농장 기상 보호 적치 공간 | WeatherProtectedStaging, HarvestDelay, MaterialShelter, Spatial.CargoAccessible, Spatial.TemporaryStorage, Spatial.WeatherShelter, Farm |
| `ExploratoryInventory` | `h1-stock:hub-long-term-storage` | Hub 장기 보관 공간 | WI-002, LongTermStorage, StorageAging, CapacityPlanning, Spatial.Storage, Spatial.CargoAccessible, Spatial.WorkerAccessible, City |
| `ExploratoryInventory` | `h1-stock:hub-market-transfer` | Hub–시장 화물 인계 공간 | WI-MARKET-01, WI-MARKET-02, City, Transition |
| `ExploratoryInventory` | `h1-stock:hub-outbound-staging` | Hub 피킹·출고 준비 공간 | WI-HUB-03, WI-HUB-04, WI-HUB-05, City |
| `ExploratoryInventory` | `h1-stock:hub-service-maintenance` | Hub 시설 정비 공간 | WI-WORLD-04, City, Transition |
| `ExploratoryInventory` | `h1-stock:hub-temporary-staging` | Hub 임시 적치 공간 | WI-001, WI-HUB-04, WI-HUB-05, InboundStaging, OutboundStaging, Spatial.CargoAccessible, Spatial.WorkerAccessible, Spatial.TemporaryStorage, City |
| `ExploratoryInventory` | `h1-stock:hub-town-corridor` | Hub–Town 물류 회랑 | WI-MARKET-01, Network, Transition |
| `ExploratoryInventory` | `h1-stock:hub-vehicle-yard` | Hub 차량 상차·대기 공간 | WI-HUB-06, WI-MARKET-01, City |
| `ExploratoryInventory` | `h1-stock:nature-exploration-buffer` | 자연 탐색·완충 공간 | WI-WORLD-05, WI-WORLD-07, Nature |
| `ExploratoryInventory` | `h1-stock:nature-farm-edge` | 숲 경계형 농장 전환 공간 | WI-WORLD-05, Nature, Transition |
| `ExploratoryInventory` | `h1-stock:nature-trailhead` | 자연 탐색 출발지 | WI-WORLD-05, WI-WORLD-07, TrailStart, RouteCheck, ExplorationBriefing, Spatial.Traversable, Spatial.WorkerAccessible, Spatial.InformationArea, Nature |
| `ExploratoryInventory` | `h1-stock:road-facility-access` | 도로–시설 진입 전환 공간 | WI-WORLD-04, Network, Transition |
| `ExploratoryInventory` | `h1-stock:town-contamination-inspection` | 생활권 재고 오염 점검 공간 | MarketContaminationInspection, StockSafetyAssessment, SaleHoldDecision, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.InspectionWorkArea, City, Town |
| `ExploratoryInventory` | `h1-stock:town-living-square` | 생활권 작은 광장 | WI-WORLD-05, WI-ORDER-07, Town |
| `ExploratoryInventory` | `h1-stock:town-market-display` | 마트 진열·판매 공간 | WI-MARKET-05, WI-ORDER-03, City, Town |
| `ExploratoryInventory` | `h1-stock:town-market-receiving` | 마트 후방 입고 공간 | WI-MARKET-02, WI-MARKET-03, WI-MARKET-04, City, Transition |
| `ExploratoryInventory` | `h1-stock:town-nature-relief` | 생활권 자연권 지원 인계점 | NatureReliefCollection, RestorationSupplyHandoff, CommunitySupport, Spatial.CustomerAccessible, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.CollectionWorkArea, Town |
| `ExploratoryInventory` | `h1-stock:town-neighborhood-service` | 근린 서비스 거점 | WI-WORLD-05, LocalInformation, NeighborhoodTaskStart, Spatial.CustomerAccessible, Spatial.WorkerAccessible, Spatial.InformationArea, Town |
| `ExploratoryInventory` | `h1-stock:town-order-packing` | 주문 포장 작업공간 | WI-ORDER-04, City, Town |
| `ExploratoryInventory` | `h1-stock:town-recall-service` | 생활권 회수·안내 창구 | ResidentRecallNotice, ContaminatedReturn, ReplacementPickup, Spatial.CustomerAccessible, Spatial.WorkerAccessible, Spatial.ReturnsWorkArea, Spatial.InformationArea, Town |
| `ExploratoryInventory` | `h1-stock:town-resident-pickup` | 주민 수령 공간 | WI-ORDER-05, WI-ORDER-06, Town |
| `IdeaInventory` | `h1-stock:farm-incident-quarantine` | 농장 사고 수확물 격리 공간 | FarmCargoQuarantine, IncidentHold, ReleaseOrDiscard, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.ExclusiveOccupancy, Spatial.TemporaryStorage, Farm |
| `IdeaInventory` | `h1-stock:farm-loss-recovery` | 농장 손실 복구·재작업 공간 | CropLossAssessment, ProduceRework, RecoveryPacking, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.SortingWorkArea, Spatial.PackingWorkArea, Farm |
| `IdeaInventory` | `h1-stock:farm-sorting` | 농산물 선별 공간 | ProduceSorting, QualityGrading, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.SortingWorkArea, Farm |
| `IdeaInventory` | `h1-stock:farm-washing` | 농산물 세척 공간 | ProduceWashing, WaterUse, WasteWaterHandling, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.WaterAccessible, Spatial.WashingWorkArea, Farm, Nature |
| `IdeaInventory` | `h1-stock:farm-worker-waiting` | 농장 작업자 대기 공간 | WI-WORLD-01, WorkerBriefing, ShiftHandoff, Spatial.WorkerAccessible, Spatial.NpcWorkArea, Farm, Town |
| `IdeaInventory` | `h1-stock:hub-cold-storage` | Hub 저온 보관 공간 | ColdStorage, TemperatureExcursion, ColdChainRelease, Spatial.Storage, Spatial.CargoAccessible, Spatial.TemperatureControlled, City |
| `IdeaInventory` | `h1-stock:hub-quarantine` | Hub 검역·격리 공간 | CargoQuarantine, QualityHold, ReleaseOrReject, Spatial.CargoAccessible, Spatial.WorkerAccessible, Spatial.ExclusiveOccupancy, City |
| `IdeaInventory` | `h1-stock:hub-returns` | Hub 반품 처리 공간 | ReturnReceiving, ReturnInspection, RestockOrDispose, Spatial.CargoAccessible, Spatial.WorkerAccessible, Spatial.InspectionWorkArea, City |
| `IdeaInventory` | `h1-stock:nature-lookout` | 자연 전망·관찰 공간 | WI-WORLD-05, LandscapeObservation, ThreatObservation, Spatial.Traversable, Spatial.ObservationArea, Nature |
| `IdeaInventory` | `h1-stock:nature-shelter` | 자연 임시 대피 공간 | WI-WORLD-07, TemporaryShelter, WeatherWait, Recovery, Spatial.Traversable, Spatial.RestArea, Spatial.WeatherShelter, Nature |
| `IdeaInventory` | `h1-stock:town-cleanup-transfer` | 생활권 정화·폐기 인계 공간 | ContaminatedWasteTransfer, MarketCleanup, ServiceVehicleHandoff, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.VehicleAccessible, Spatial.WasteHandlingArea, City, Town |
| `IdeaInventory` | `h1-stock:town-contamination-quarantine` | 생활권 오염 재고 격리 공간 | MarketStockQuarantine, RecallHold, ReleaseOrDispose, Spatial.WorkerAccessible, Spatial.CargoAccessible, Spatial.ExclusiveOccupancy, Spatial.TemporaryStorage, City, Town |
| `IdeaInventory` | `h1-stock:town-returns` | 마트 반품 접수 공간 | CustomerReturn, ReturnTriage, ReturnHandoff, Spatial.CustomerAccessible, Spatial.WorkerAccessible, Spatial.InspectionWorkArea, City, Town |
| `IdeaInventory` | `h1-stock:town-staff-rest` | 생활권 직원 휴게 공간 | WI-WORLD-07, WorkerRest, ShiftChange, Spatial.WorkerAccessible, Spatial.RestArea, City, Town |
| `IdeaInventory` | `h1-stock:town-waste` | 생활권 폐기물 처리 공간 | WasteSorting, WasteStorage, WasteCollection, Spatial.WorkerAccessible, Spatial.TemporaryStorage, Spatial.ServiceVehicleAccessible, Town, Transition |

## H2

| 상태 | 고유 식별자 | 이름 | 검색 단서 |
| --- | --- | --- | --- |
| `CandidateForReview` | `h2-candidate:nature-restoration-recovery` | 자연 복원·안전 회복 블록 | h1-stock:nature-restoration-site, h1-stock:nature-safe-recovery-camp, IncidentRouteInput, RetreatRecoveryInput, SafeCoreOutput, RestoredRouteOutput |
| `CandidateForReview` | `h2-candidate:nature-threat-response` | 자연 위협 추적·대피 블록 | h1-stock:nature-threat-watch, h1-stock:nature-incident-trace, h1-stock:nature-emergency-retreat, SafeCoreInput, ThreatBandContinuation, EmergencyExit, RecoveryHandoff |
| `ExploratoryInventory` | `h2-candidate:farm-harvest-throughput` | 농장 집중 수확·집하 블록 | h1-stock:farm-production, h1-stock:farm-harvest-staging, h1-stock:farm-work-yard, ProductionInput, HarvestFlow, ProcessingOutput |
| `ExploratoryInventory` | `h2-candidate:farm-hub-corridor` | Farm–Hub 회랑 블록 | h1-stock:farm-loading-gate, h1-stock:farm-hub-corridor |
| `ExploratoryInventory` | `h2-candidate:farm-incident-containment` | 농장 사건 점검·격리 블록 | h1-stock:farm-exposure-inspection, h1-stock:farm-incident-quarantine, h1-stock:farm-weather-protection, HarvestInput, SafeCargoOutput, RecoveryOutput |
| `ExploratoryInventory` | `h2-candidate:farm-irrigation-service` | 농장 관수·급수 관리 블록 | h1-stock:farm-production, h1-stock:farm-maintenance-yard, h1-stock:farm-weather-protection, FieldInput, WaterServiceRoute, CropCareOutput |
| `ExploratoryInventory` | `h2-candidate:farm-loss-restoration-handoff` | 농장 손실 회복·복원 인계 블록 | h1-stock:farm-incident-quarantine, h1-stock:farm-loss-recovery, h1-stock:farm-restoration-supply, IncidentInput, RecoveredCargoOutput, NatureRestorationOutput |
| `ExploratoryInventory` | `h2-candidate:farm-processing-shipping` | 농장 작업·출하 블록 | h1-stock:farm-work-yard, h1-stock:farm-maintenance-yard, h1-stock:farm-loading-gate |
| `ExploratoryInventory` | `h2-candidate:farm-seed-and-tools` | 종자·농기구 준비 블록 | h1-stock:farm-tool-storage, h1-stock:farm-seed-preparation, ProductionOutput, WorkerAccess |
| `ExploratoryInventory` | `h2-candidate:forest-edge-farm` | 숲 경계 농장 블록 | h1-stock:nature-farm-edge, h1-stock:nature-exploration-buffer, h1-stock:farm-production |
| `ExploratoryInventory` | `h2-candidate:highland-production` | 고지대 생산 블록 | h1-stock:farm-production, h1-stock:nature-farm-edge |
| `ExploratoryInventory` | `h2-candidate:hub-emergency-power` | Hub 비상 전력·보관 유지 블록 | h1-stock:hub-cold-storage, h1-stock:hub-long-term-storage, h1-stock:hub-service-maintenance, StorageInput, EmergencyService, SafeReleaseOutput |
| `ExploratoryInventory` | `h2-candidate:hub-fulfillment` | Hub 피킹·출고준비 작업 블록 | h1-stock:hub-outbound-staging, h1-stock:hub-temporary-staging, StorageInput, PickingLoop, OutboundStagingOutput |
| `ExploratoryInventory` | `h2-candidate:hub-inbound-storage` | Hub 입고·창고 블록 | h1-stock:hub-receiving-storage, h1-stock:hub-service-maintenance |
| `ExploratoryInventory` | `h2-candidate:hub-maintenance-yard` | Hub 차량·시설 정비 블록 | h1-stock:hub-vehicle-yard, h1-stock:hub-service-maintenance, VehicleInput, MaintenanceLoop, OperationsReturn |
| `ExploratoryInventory` | `h2-candidate:hub-outbound-vehicle` | Hub 출고·차량 블록 | h1-stock:hub-outbound-staging, h1-stock:hub-vehicle-yard, h1-stock:hub-market-transfer |
| `ExploratoryInventory` | `h2-candidate:hub-town-corridor` | Hub–Town 회랑 블록 | h1-stock:hub-market-transfer, h1-stock:hub-town-corridor, h1-stock:road-facility-access |
| `ExploratoryInventory` | `h2-candidate:lowrise-residential` | 저층 주거 블록 | h1-stock:town-living-square, h1-stock:town-resident-pickup |
| `ExploratoryInventory` | `h2-candidate:market-life-commerce` | 마트·생활상권 블록 | h1-stock:town-market-receiving, h1-stock:town-market-display, h1-stock:town-order-packing, h1-stock:town-resident-pickup, h1-stock:town-living-square |
| `ExploratoryInventory` | `h2-candidate:nature-defense-ring` | 자연 야간 방어 블록 | h1-stock:nature-threat-watch, h1-stock:nature-shelter, h1-stock:nature-safe-recovery-camp, ThreatInput, DefenseLoop, RecoveryOutput |
| `ExploratoryInventory` | `h2-candidate:nature-encounter-route` | 자연 몬스터 조우·이탈 블록 | h1-stock:nature-threat-watch, h1-stock:nature-incident-trace, h1-stock:nature-emergency-retreat, EncounterInput, ThreatContinuation, RetreatOutput |
| `ExploratoryInventory` | `h2-candidate:nature-home-core` | 자연 안전 생활핵 블록 | h1-stock:nature-safe-recovery-camp, h1-stock:nature-shelter, h1-stock:nature-trailhead, TrailInput, SafeCoreOutput, EmergencyReturn |
| `ExploratoryInventory` | `h2-candidate:nature-town-relief-transition` | Nature–Town 대피·구호 전환 블록 | h1-stock:town-nature-relief, h1-stock:nature-restoration-site, h1-stock:nature-safe-recovery-camp, TownReliefInput, NatureRestorationHandoff, SafeCoreReturn |
| `ExploratoryInventory` | `h2-candidate:nature-trail-shelter` | 자연 탐색·대피 블록 | h1-stock:nature-trailhead, h1-stock:nature-lookout, h1-stock:nature-shelter, RoadAccess, TrailContinuation |
| `ExploratoryInventory` | `h2-candidate:nature-water-buffer` | 산림·수변 완충 블록 | h1-stock:nature-exploration-buffer, h1-stock:nature-farm-edge |
| `ExploratoryInventory` | `h2-candidate:town-contamination-control` | 생활권 오염 점검·정화 블록 | h1-stock:town-contamination-inspection, h1-stock:town-contamination-quarantine, h1-stock:town-cleanup-transfer, MarketStockInput, SafeDisplayOutput, ServiceVehicleOutput |
| `ExploratoryInventory` | `h2-candidate:town-market-receiving` | 마트 후방 입고·검수 블록 | h1-stock:road-facility-access, h1-stock:town-market-receiving, DeliveryAccessInput, ReceivingInspectionLoop, BackroomOutput |
| `ExploratoryInventory` | `h2-candidate:town-order-fulfillment` | 주문 피킹·포장·수령 블록 | h1-stock:town-market-display, h1-stock:town-order-packing, h1-stock:town-resident-pickup, OrderStockInput, PickingPackingLoop, ResidentPickupOutput |
| `ExploratoryInventory` | `h2-candidate:town-recall-relief` | 생활권 회수 안내·자연권 구호 블록 | h1-stock:town-recall-service, h1-stock:town-nature-relief, h1-stock:town-neighborhood-service, ResidentInput, ReturnOutput, NatureReliefOutput |
| `ExploratoryInventory` | `h2-candidate:town-resident-service` | 생활권 주민지원·공동수령 블록 | h1-stock:town-neighborhood-service, h1-stock:town-resident-pickup, h1-stock:town-recall-service, ResidentInput, ServiceLoop, PickupOutput |
| `ExploratoryInventory` | `h2-candidate:town-residential-alley` | 생활권 주거 골목 블록 | h1-stock:town-living-square, h1-stock:town-neighborhood-service, NeighborhoodInput, PedestrianLoop, LocalRoadOutput |
| `IdeaInventory` | `h2-candidate:farm-wash-sort-pack` | 세척·선별·포장 블록 | h1-stock:farm-harvest-staging, h1-stock:farm-washing, h1-stock:farm-sorting, h1-stock:farm-work-yard, HarvestInput, ShippingOutput |
| `IdeaInventory` | `h2-candidate:farm-worker-support` | 농장 작업 지원 블록 | h1-stock:farm-worker-waiting, h1-stock:farm-tool-storage, h1-stock:farm-maintenance-yard, FarmRoadAccess, ProductionAccess |
| `IdeaInventory` | `h2-candidate:hub-longterm-cold-storage` | Hub 장기·저온 보관 블록 | h1-stock:hub-long-term-storage, h1-stock:hub-cold-storage, InboundStorage, PickingOutput |
| `IdeaInventory` | `h2-candidate:hub-quarantine-staging` | Hub 검역·격리 블록 | h1-stock:hub-receiving-storage, h1-stock:hub-quarantine, h1-stock:hub-temporary-staging, HubInboundGate, StorageOutput, RejectOutput |
| `IdeaInventory` | `h2-candidate:hub-returns-processing` | Hub 반품 처리 블록 | h1-stock:hub-returns, h1-stock:hub-quarantine, ReturnInput, RestockOutput, DisposalOutput |
| `IdeaInventory` | `h2-candidate:town-returns-waste` | 생활권 반품·폐기물 블록 | h1-stock:town-returns, h1-stock:town-waste, h1-stock:road-facility-access, CustomerReturnInput, ServiceVehicleOutput |

## H3

| 상태 | 고유 식별자 | 이름 | 검색 단서 |
| --- | --- | --- | --- |
| `CandidateForReview` | `h3-candidate:nature-threat-recovery` | 자연 생활·위협·회복 경관 | h2-candidate:nature-threat-response, h2-candidate:nature-restoration-recovery, SafeCoreAccess, FarmIncidentInput, TownReliefInput, CityHubReturn, RestoredTrailOutput |
| `ExploratoryInventory` | `h3-candidate:farm-hub-logistics` | 농장–물류 거점 연결 경관 | h2-candidate:farm-processing-shipping, h2-candidate:farm-hub-corridor, h2-candidate:hub-inbound-storage, FarmGate, HubInboundGate |
| `ExploratoryInventory` | `h3-candidate:farm-incident-recovery` | 농장 사건 격리·회복 경관 | h2-candidate:farm-incident-containment, h2-candidate:farm-loss-restoration-handoff, ProductionIncidentInput, RecoveredProductionOutput, NatureRestorationHandoff, FarmExternalGate |
| `ExploratoryInventory` | `h3-candidate:farm-seasonal-production-loop` | Farm 계절 생산·출하 순환 경관 | h2-candidate:farm-irrigation-service, h2-candidate:farm-harvest-throughput, h2-candidate:farm-processing-shipping, SeasonInput, ProductionLoop, FarmShippingGate |
| `ExploratoryInventory` | `h3-candidate:highland-farm` | 고지대 농장 경관 | h2-candidate:highland-production, h2-candidate:farm-processing-shipping, h2-candidate:forest-edge-farm, FarmExternalGate |
| `ExploratoryInventory` | `h3-candidate:hub-fulfillment-operations` | City/Hub 보관·피킹·상차 운영 경관 | h2-candidate:hub-longterm-cold-storage, h2-candidate:hub-fulfillment, h2-candidate:hub-outbound-vehicle, StorageCargoInput, FulfillmentLoop, HubLoadingOutput |
| `ExploratoryInventory` | `h3-candidate:hub-maintenance-emergency-loop` | City/Hub 정비·비상운영 회복 경관 | h2-candidate:hub-maintenance-yard, h2-candidate:hub-emergency-power, h2-candidate:hub-outbound-vehicle, IncidentInput, EmergencyOperationsLoop, HubOperationsReturn |
| `ExploratoryInventory` | `h3-candidate:hub-town-logistics` | Hub–Town 연결 경관 | h2-candidate:hub-outbound-vehicle, h2-candidate:hub-town-corridor, h2-candidate:market-life-commerce, HubOutboundGate, TownReceivingGate |
| `ExploratoryInventory` | `h3-candidate:jinbu-hub` | 진부형 물류 Hub 경관 | h2-candidate:hub-inbound-storage, h2-candidate:hub-outbound-vehicle, HubInboundGate, HubOutboundGate |
| `ExploratoryInventory` | `h3-candidate:lowrise-market-town` | 저층 생활·시장 경관 | h2-candidate:lowrise-residential, h2-candidate:market-life-commerce, TownReceivingGate, TownLocalRoad |
| `ExploratoryInventory` | `h3-candidate:nature-exploration-buffer` | Nature 탐색·완충 경관 | h2-candidate:nature-water-buffer, h2-candidate:forest-edge-farm, NatureTrail, FarmEdge |
| `ExploratoryInventory` | `h3-candidate:nature-home-encounter-defense` | Nature 생활핵·조우·방어 폐루프 경관 | h2-candidate:nature-home-core, h2-candidate:nature-encounter-route, h2-candidate:nature-defense-ring, SafeCoreGate, ExplorationOutput, ThreatInput, RecoveryReturn |
| `ExploratoryInventory` | `h3-candidate:nature-town-relief-loop` | Nature–Town 대피·구호 인계 경관 | h2-candidate:town-recall-relief, h2-candidate:nature-town-relief-transition, h2-candidate:nature-restoration-recovery, TownReliefGate, NatureRestorationGate, SafeCoreReturnLoop |
| `ExploratoryInventory` | `h3-candidate:nature-trail-network` | 자연 탐색길·대피망 경관 | h2-candidate:nature-trail-shelter, h2-candidate:nature-water-buffer, TownOrFarmAccess, TrailLoop, EmergencyExit |
| `ExploratoryInventory` | `h3-candidate:town-contamination-relief` | 생활권 오염 통제·구호 경관 | h2-candidate:town-contamination-control, h2-candidate:town-recall-relief, MarketIncidentInput, SafeMarketOutput, ResidentReturnLoop, NatureReliefHandoff, TownExternalGate |
| `ExploratoryInventory` | `h3-candidate:town-market-fulfillment` | Town 마트 입고·주문이행 경관 | h2-candidate:town-market-receiving, h2-candidate:market-life-commerce, h2-candidate:town-order-fulfillment, TownDeliveryInput, MarketFulfillmentLoop, ResidentPickupOutput |
| `ExploratoryInventory` | `h3-candidate:town-resident-service-loop` | Town 주민서비스·공동수령 경관 | h2-candidate:town-residential-alley, h2-candidate:town-resident-service, h2-candidate:lowrise-residential, NeighborhoodGate, ResidentServiceLoop, TownLocalRoad |
| `IdeaInventory` | `h3-candidate:circular-market-town` | 반품·회수 순환형 시장 마을 경관 | h2-candidate:market-life-commerce, h2-candidate:town-returns-waste, TownReceivingGate, ReturnOutput, TownLocalRoad |
| `IdeaInventory` | `h3-candidate:farm-processing-campus` | 농장 생산·후처리 복합 경관 | h2-candidate:highland-production, h2-candidate:farm-seed-and-tools, h2-candidate:farm-wash-sort-pack, h2-candidate:farm-processing-shipping, FarmExternalGate, NatureEdge |
| `IdeaInventory` | `h3-candidate:resilient-logistics-hub` | 품질·보관 대응형 물류 Hub 경관 | h2-candidate:hub-inbound-storage, h2-candidate:hub-quarantine-staging, h2-candidate:hub-longterm-cold-storage, h2-candidate:hub-outbound-vehicle, HubInboundGate, HubOutboundGate, ReturnGate |
