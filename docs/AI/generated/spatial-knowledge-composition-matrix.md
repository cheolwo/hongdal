# 공간 설계 지식 조합표

## H2 조립법

| H2 | 위상 | 필수 H1 | 선택 H1 |
| --- | --- | --- | --- |
| `h2-candidate:farm-harvest-throughput` 농장 집중 수확·집하 블록 | `ModifiedGrid` | h1-stock:farm-production, h1-stock:farm-harvest-staging, h1-stock:farm-work-yard | h1-stock:farm-worker-waiting |
| `h2-candidate:farm-hub-corridor` Farm–Hub 회랑 블록 | `Linear` | h1-stock:farm-loading-gate, h1-stock:farm-hub-corridor |  |
| `h2-candidate:farm-incident-containment` 농장 사건 점검·격리 블록 | `ModifiedGrid` | h1-stock:farm-exposure-inspection, h1-stock:farm-incident-quarantine, h1-stock:farm-weather-protection | h1-stock:farm-harvest-staging |
| `h2-candidate:farm-irrigation-service` 농장 관수·급수 관리 블록 | `Linear` | h1-stock:farm-production, h1-stock:farm-maintenance-yard, h1-stock:farm-weather-protection | h1-stock:farm-tool-storage |
| `h2-candidate:farm-loss-restoration-handoff` 농장 손실 회복·복원 인계 블록 | `Linear` | h1-stock:farm-incident-quarantine, h1-stock:farm-loss-recovery, h1-stock:farm-restoration-supply | h1-stock:farm-work-yard, h1-stock:nature-farm-edge |
| `h2-candidate:farm-processing-shipping` 농장 작업·출하 블록 | `Linear` | h1-stock:farm-work-yard, h1-stock:farm-maintenance-yard, h1-stock:farm-loading-gate |  |
| `h2-candidate:farm-seed-and-tools` 종자·농기구 준비 블록 | `Cluster` | h1-stock:farm-tool-storage, h1-stock:farm-seed-preparation | h1-stock:farm-worker-waiting |
| `h2-candidate:farm-wash-sort-pack` 세척·선별·포장 블록 | `Linear` | h1-stock:farm-harvest-staging, h1-stock:farm-washing, h1-stock:farm-sorting, h1-stock:farm-work-yard |  |
| `h2-candidate:farm-worker-support` 농장 작업 지원 블록 | `Cluster` | h1-stock:farm-worker-waiting, h1-stock:farm-tool-storage, h1-stock:farm-maintenance-yard |  |
| `h2-candidate:forest-edge-farm` 숲 경계 농장 블록 | `ContourAdaptive` | h1-stock:nature-farm-edge, h1-stock:nature-exploration-buffer, h1-stock:farm-production |  |
| `h2-candidate:highland-production` 고지대 생산 블록 | `ModifiedGrid` | h1-stock:farm-production, h1-stock:nature-farm-edge |  |
| `h2-candidate:hub-emergency-power` Hub 비상 전력·보관 유지 블록 | `Cluster` | h1-stock:hub-cold-storage, h1-stock:hub-long-term-storage, h1-stock:hub-service-maintenance | h1-stock:hub-temporary-staging |
| `h2-candidate:hub-fulfillment` Hub 피킹·출고준비 작업 블록 | `Linear` | h1-stock:hub-outbound-staging, h1-stock:hub-temporary-staging | h1-stock:hub-long-term-storage, h1-stock:hub-vehicle-yard |
| `h2-candidate:hub-inbound-storage` Hub 입고·창고 블록 | `ModifiedGrid` | h1-stock:hub-receiving-storage, h1-stock:hub-service-maintenance |  |
| `h2-candidate:hub-longterm-cold-storage` Hub 장기·저온 보관 블록 | `Grid` | h1-stock:hub-long-term-storage, h1-stock:hub-cold-storage | h1-stock:hub-temporary-staging |
| `h2-candidate:hub-maintenance-yard` Hub 차량·시설 정비 블록 | `ModifiedGrid` | h1-stock:hub-vehicle-yard, h1-stock:hub-service-maintenance | h1-stock:hub-temporary-staging |
| `h2-candidate:hub-outbound-vehicle` Hub 출고·차량 블록 | `Linear` | h1-stock:hub-outbound-staging, h1-stock:hub-vehicle-yard, h1-stock:hub-market-transfer |  |
| `h2-candidate:hub-quarantine-staging` Hub 검역·격리 블록 | `ModifiedGrid` | h1-stock:hub-receiving-storage, h1-stock:hub-quarantine, h1-stock:hub-temporary-staging |  |
| `h2-candidate:hub-returns-processing` Hub 반품 처리 블록 | `ModifiedGrid` | h1-stock:hub-returns, h1-stock:hub-quarantine | h1-stock:hub-temporary-staging |
| `h2-candidate:hub-town-corridor` Hub–Town 회랑 블록 | `Linear` | h1-stock:hub-market-transfer, h1-stock:hub-town-corridor, h1-stock:road-facility-access |  |
| `h2-candidate:lowrise-residential` 저층 주거 블록 | `Grid` | h1-stock:town-living-square, h1-stock:town-resident-pickup |  |
| `h2-candidate:market-life-commerce` 마트·생활상권 블록 | `ModifiedGrid` | h1-stock:town-market-receiving, h1-stock:town-market-display, h1-stock:town-order-packing, h1-stock:town-resident-pickup, h1-stock:town-living-square |  |
| `h2-candidate:nature-defense-ring` 자연 야간 방어 블록 | `Radial` | h1-stock:nature-threat-watch, h1-stock:nature-shelter, h1-stock:nature-safe-recovery-camp | h1-stock:nature-incident-trace |
| `h2-candidate:nature-encounter-route` 자연 몬스터 조우·이탈 블록 | `ContourAdaptive` | h1-stock:nature-threat-watch, h1-stock:nature-incident-trace, h1-stock:nature-emergency-retreat | h1-stock:nature-lookout |
| `h2-candidate:nature-home-core` 자연 안전 생활핵 블록 | `Cluster` | h1-stock:nature-safe-recovery-camp, h1-stock:nature-shelter, h1-stock:nature-trailhead | h1-stock:nature-lookout |
| `h2-candidate:nature-restoration-recovery` 자연 복원·안전 회복 블록 | `Organic` | h1-stock:nature-restoration-site, h1-stock:nature-safe-recovery-camp | h1-stock:nature-exploration-buffer, h1-stock:nature-shelter |
| `h2-candidate:nature-threat-response` 자연 위협 추적·대피 블록 | `ContourAdaptive` | h1-stock:nature-threat-watch, h1-stock:nature-incident-trace, h1-stock:nature-emergency-retreat | h1-stock:nature-lookout, h1-stock:nature-shelter |
| `h2-candidate:nature-town-relief-transition` Nature–Town 대피·구호 전환 블록 | `Linear` | h1-stock:town-nature-relief, h1-stock:nature-restoration-site, h1-stock:nature-safe-recovery-camp | h1-stock:town-recall-service, h1-stock:nature-emergency-retreat |
| `h2-candidate:nature-trail-shelter` 자연 탐색·대피 블록 | `ContourAdaptive` | h1-stock:nature-trailhead, h1-stock:nature-lookout, h1-stock:nature-shelter | h1-stock:nature-exploration-buffer |
| `h2-candidate:nature-water-buffer` 산림·수변 완충 블록 | `Organic` | h1-stock:nature-exploration-buffer, h1-stock:nature-farm-edge |  |
| `h2-candidate:town-contamination-control` 생활권 오염 점검·정화 블록 | `ModifiedGrid` | h1-stock:town-contamination-inspection, h1-stock:town-contamination-quarantine, h1-stock:town-cleanup-transfer | h1-stock:town-market-receiving |
| `h2-candidate:town-market-receiving` 마트 후방 입고·검수 블록 | `Linear` | h1-stock:road-facility-access, h1-stock:town-market-receiving | h1-stock:town-staff-rest, h1-stock:town-market-display |
| `h2-candidate:town-order-fulfillment` 주문 피킹·포장·수령 블록 | `ModifiedGrid` | h1-stock:town-market-display, h1-stock:town-order-packing, h1-stock:town-resident-pickup | h1-stock:town-neighborhood-service |
| `h2-candidate:town-recall-relief` 생활권 회수 안내·자연권 구호 블록 | `Cluster` | h1-stock:town-recall-service, h1-stock:town-nature-relief, h1-stock:town-neighborhood-service | h1-stock:town-resident-pickup, h1-stock:town-living-square |
| `h2-candidate:town-resident-service` 생활권 주민지원·공동수령 블록 | `Cluster` | h1-stock:town-neighborhood-service, h1-stock:town-resident-pickup, h1-stock:town-recall-service | h1-stock:town-living-square |
| `h2-candidate:town-residential-alley` 생활권 주거 골목 블록 | `ModifiedGrid` | h1-stock:town-living-square, h1-stock:town-neighborhood-service | h1-stock:town-staff-rest |
| `h2-candidate:town-returns-waste` 생활권 반품·폐기물 블록 | `Linear` | h1-stock:town-returns, h1-stock:town-waste, h1-stock:road-facility-access | h1-stock:town-staff-rest |

## H3 청사진

| H3 | 위상 | 필수 H2 | 선택 H2 | 외부 연결 역할 |
| --- | --- | --- | --- | --- |
| `h3-candidate:circular-market-town` 반품·회수 순환형 시장 마을 경관 | `ModifiedGrid` | h2-candidate:market-life-commerce, h2-candidate:town-returns-waste | h2-candidate:lowrise-residential | TownReceivingGate, ReturnOutput, TownLocalRoad |
| `h3-candidate:farm-hub-logistics` 농장–물류 거점 연결 경관 | `Linear` | h2-candidate:farm-processing-shipping, h2-candidate:farm-hub-corridor, h2-candidate:hub-inbound-storage |  | FarmGate, HubInboundGate |
| `h3-candidate:farm-incident-recovery` 농장 사건 격리·회복 경관 | `ModifiedGrid` | h2-candidate:farm-incident-containment, h2-candidate:farm-loss-restoration-handoff | h2-candidate:farm-processing-shipping, h2-candidate:forest-edge-farm | ProductionIncidentInput, RecoveredProductionOutput, NatureRestorationHandoff, FarmExternalGate |
| `h3-candidate:farm-processing-campus` 농장 생산·후처리 복합 경관 | `ModifiedGrid` | h2-candidate:highland-production, h2-candidate:farm-seed-and-tools, h2-candidate:farm-wash-sort-pack, h2-candidate:farm-processing-shipping | h2-candidate:farm-worker-support, h2-candidate:forest-edge-farm | FarmExternalGate, NatureEdge |
| `h3-candidate:farm-seasonal-production-loop` Farm 계절 생산·출하 순환 경관 | `ModifiedGrid` | h2-candidate:farm-irrigation-service, h2-candidate:farm-harvest-throughput, h2-candidate:farm-processing-shipping | h2-candidate:farm-worker-support | SeasonInput, ProductionLoop, FarmShippingGate |
| `h3-candidate:highland-farm` 고지대 농장 경관 | `ContourAdaptive` | h2-candidate:highland-production, h2-candidate:farm-processing-shipping, h2-candidate:forest-edge-farm |  | FarmExternalGate |
| `h3-candidate:hub-fulfillment-operations` City/Hub 보관·피킹·상차 운영 경관 | `ModifiedGrid` | h2-candidate:hub-longterm-cold-storage, h2-candidate:hub-fulfillment, h2-candidate:hub-outbound-vehicle | h2-candidate:hub-maintenance-yard | StorageCargoInput, FulfillmentLoop, HubLoadingOutput |
| `h3-candidate:hub-maintenance-emergency-loop` City/Hub 정비·비상운영 회복 경관 | `ModifiedGrid` | h2-candidate:hub-maintenance-yard, h2-candidate:hub-emergency-power, h2-candidate:hub-outbound-vehicle | h2-candidate:hub-longterm-cold-storage | IncidentInput, EmergencyOperationsLoop, HubOperationsReturn |
| `h3-candidate:hub-town-logistics` Hub–Town 연결 경관 | `Linear` | h2-candidate:hub-outbound-vehicle, h2-candidate:hub-town-corridor, h2-candidate:market-life-commerce |  | HubOutboundGate, TownReceivingGate |
| `h3-candidate:jinbu-hub` 진부형 물류 Hub 경관 | `ModifiedGrid` | h2-candidate:hub-inbound-storage, h2-candidate:hub-outbound-vehicle |  | HubInboundGate, HubOutboundGate |
| `h3-candidate:lowrise-market-town` 저층 생활·시장 경관 | `ModifiedGrid` | h2-candidate:lowrise-residential, h2-candidate:market-life-commerce |  | TownReceivingGate, TownLocalRoad |
| `h3-candidate:nature-exploration-buffer` Nature 탐색·완충 경관 | `Organic` | h2-candidate:nature-water-buffer, h2-candidate:forest-edge-farm |  | NatureTrail, FarmEdge |
| `h3-candidate:nature-home-encounter-defense` Nature 생활핵·조우·방어 폐루프 경관 | `Organic` | h2-candidate:nature-home-core, h2-candidate:nature-encounter-route, h2-candidate:nature-defense-ring | h2-candidate:nature-restoration-recovery | SafeCoreGate, ExplorationOutput, ThreatInput, RecoveryReturn |
| `h3-candidate:nature-threat-recovery` 자연 생활·위협·회복 경관 | `Organic` | h2-candidate:nature-threat-response, h2-candidate:nature-restoration-recovery | h2-candidate:nature-trail-shelter, h2-candidate:nature-water-buffer | SafeCoreAccess, FarmIncidentInput, TownReliefInput, CityHubReturn, RestoredTrailOutput |
| `h3-candidate:nature-town-relief-loop` Nature–Town 대피·구호 인계 경관 | `Linear` | h2-candidate:town-recall-relief, h2-candidate:nature-town-relief-transition, h2-candidate:nature-restoration-recovery | h2-candidate:town-resident-service, h2-candidate:nature-home-core | TownReliefGate, NatureRestorationGate, SafeCoreReturnLoop |
| `h3-candidate:nature-trail-network` 자연 탐색길·대피망 경관 | `Organic` | h2-candidate:nature-trail-shelter, h2-candidate:nature-water-buffer | h2-candidate:forest-edge-farm | TownOrFarmAccess, TrailLoop, EmergencyExit |
| `h3-candidate:resilient-logistics-hub` 품질·보관 대응형 물류 Hub 경관 | `ModifiedGrid` | h2-candidate:hub-inbound-storage, h2-candidate:hub-quarantine-staging, h2-candidate:hub-longterm-cold-storage, h2-candidate:hub-outbound-vehicle | h2-candidate:hub-returns-processing | HubInboundGate, HubOutboundGate, ReturnGate |
| `h3-candidate:town-contamination-relief` 생활권 오염 통제·구호 경관 | `ModifiedGrid` | h2-candidate:town-contamination-control, h2-candidate:town-recall-relief | h2-candidate:market-life-commerce, h2-candidate:town-returns-waste | MarketIncidentInput, SafeMarketOutput, ResidentReturnLoop, NatureReliefHandoff, TownExternalGate |
| `h3-candidate:town-market-fulfillment` Town 마트 입고·주문이행 경관 | `ModifiedGrid` | h2-candidate:town-market-receiving, h2-candidate:market-life-commerce, h2-candidate:town-order-fulfillment | h2-candidate:town-resident-service | TownDeliveryInput, MarketFulfillmentLoop, ResidentPickupOutput |
| `h3-candidate:town-resident-service-loop` Town 주민서비스·공동수령 경관 | `ModifiedGrid` | h2-candidate:town-residential-alley, h2-candidate:town-resident-service, h2-candidate:lowrise-residential |  | NeighborhoodGate, ResidentServiceLoop, TownLocalRoad |
