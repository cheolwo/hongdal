# H4 지역 청사진 후보집

> 이 후보들은 위치 독립 설계 재고이며 실제 지역 코드·좌표·공공데이터 요구·LandscapeGraph StableId를 갖지 않는다. 실제 배치와 공공데이터 연결은 E5·E6에서 분리한다.

## Farm–Hub–Town 연결권

- 후보: `h4-blueprint:farm-hub-town-region`
- 필수 H3: h3-candidate:highland-farm, h3-candidate:farm-hub-logistics, h3-candidate:jinbu-hub, h3-candidate:hub-town-logistics, h3-candidate:lowrise-market-town
- 선택 H3: h3-candidate:farm-processing-campus, h3-candidate:resilient-logistics-hub, h3-candidate:circular-market-town, h3-candidate:hub-fulfillment-operations, h3-candidate:town-market-fulfillment
- 설계 관계: FarmToHub, HubToTown

## 농업 생산·후처리권

- 후보: `h4-blueprint:farm-production-processing-region`
- 필수 H3: h3-candidate:highland-farm, h3-candidate:farm-processing-campus, h3-candidate:farm-incident-recovery, h3-candidate:farm-seasonal-production-loop
- 선택 H3: 없음
- 설계 관계: ProductionToProcessing, IncidentToRecovery, RecoveryToNature

## 물류 Hub권

- 후보: `h4-blueprint:logistics-hub-region`
- 필수 H3: h3-candidate:jinbu-hub, h3-candidate:resilient-logistics-hub, h3-candidate:hub-maintenance-emergency-loop, h3-candidate:hub-fulfillment-operations
- 선택 H3: 없음
- 설계 관계: InboundToStorage, StorageToOutbound

## 저층 생활·시장권

- 후보: `h4-blueprint:lowrise-market-region`
- 필수 H3: h3-candidate:lowrise-market-town, h3-candidate:circular-market-town, h3-candidate:town-contamination-relief, h3-candidate:town-resident-service-loop, h3-candidate:town-market-fulfillment
- 선택 H3: h3-candidate:nature-town-relief-loop
- 설계 관계: LivingToMarket, MarketReturnLoop, IncidentToRelief, ReliefToNature

## Nature 생활·탐험권

- 후보: `h4-blueprint:nature-home-exploration-region`
- 필수 H3: h3-candidate:nature-threat-recovery, h3-candidate:nature-trail-network, h3-candidate:nature-home-encounter-defense
- 선택 H3: h3-candidate:nature-exploration-buffer, h3-candidate:nature-town-relief-loop
- 설계 관계: SafeCoreToThreatBand, ThreatBandToRecovery, NatureToFarm, NatureToTown, NatureToCityHub

## 산림·수계 자연권

- 후보: `h4-blueprint:nature-water-region`
- 필수 H3: h3-candidate:nature-exploration-buffer, h3-candidate:nature-trail-network
- 선택 H3: 없음
- 설계 관계: NatureContinuity, WaterLandTransition
