# H4 지역 청사진 후보집

> 이 후보들은 실제 AreaSet이 아니며 실제 지역 코드·좌표·DataRequirement·LandscapeGraph StableId를 갖지 않는다.

## Farm–Hub–Town 연결권

- 후보: `h4-blueprint:farm-hub-town-region`
- 필수 H3: h3-candidate:highland-farm, h3-candidate:farm-hub-logistics, h3-candidate:jinbu-hub, h3-candidate:hub-town-logistics, h3-candidate:lowrise-market-town
- 선택 H3: h3-candidate:farm-processing-campus, h3-candidate:resilient-logistics-hub, h3-candidate:circular-market-town
- 현실 자료 목적: AdministrativeBoundary, TerrainElevation, LandCoverClassification, RoadNetwork

## 농업 생산·후처리권

- 후보: `h4-blueprint:farm-production-processing-region`
- 필수 H3: h3-candidate:highland-farm, h3-candidate:farm-processing-campus, h3-candidate:farm-incident-recovery
- 선택 H3: 없음
- 현실 자료 목적: TerrainElevation, LandCoverClassification, AgriculturalComposition, RoadNetwork

## 물류 Hub권

- 후보: `h4-blueprint:logistics-hub-region`
- 필수 H3: h3-candidate:jinbu-hub, h3-candidate:resilient-logistics-hub
- 선택 H3: 없음
- 현실 자료 목적: BuildingPlacement, RoadNetwork

## 저층 생활·시장권

- 후보: `h4-blueprint:lowrise-market-region`
- 필수 H3: h3-candidate:lowrise-market-town, h3-candidate:circular-market-town
- 선택 H3: 없음
- 현실 자료 목적: BuildingPlacement, RoadNetwork, MarketContext

## Nature 생활·탐험권

- 후보: `h4-blueprint:nature-home-exploration-region`
- 필수 H3: h3-candidate:nature-threat-recovery, h3-candidate:nature-trail-network
- 선택 H3: h3-candidate:nature-exploration-buffer
- 현실 자료 목적: TerrainElevation, LandCoverClassification, Hydrography, RoadNetwork

## 산림·수계 자연권

- 후보: `h4-blueprint:nature-water-region`
- 필수 H3: h3-candidate:nature-exploration-buffer, h3-candidate:nature-trail-network
- 선택 H3: 없음
- 현실 자료 목적: TerrainElevation, LandCoverClassification, Hydrography
