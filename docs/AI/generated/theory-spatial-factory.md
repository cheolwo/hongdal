# 이론 기반 H2·H3·E5 공간 생산 결과

사람 검토를 생산 관문으로 사용하지 않고 결정적 공간 이론 규칙으로 반복 생성한 결과다.

- H1 입력: 52
- H2 이론 적격: 34 (수기 조립법 6, 자동 유도 28)
- H3 이론 적격: 18
- 이론 E5 AreaSet 인스턴스: 4
- 패턴 이름 대장: `simulation-world-h-pattern-naming.r6`
- 사람 검토: `DeferredBatchReview` · 생산 비차단

| 우선 | 이론 AreaSet | 게임 기획 | H3 패턴 | E5 상태 |
| ---: | --- | --- | --- | --- |
| 1 | `area-set:theory:nature-home-exploration-region` | `NatureHomeThreatRecovery` | NATURE-H3-REGION-02, NATURE-H3-REGION-03, NATURE-H3-REGION-04 | `E5TheoryQualified` |
| 2 | `area-set:theory:farm-production-processing-region` | `FarmProductionSurvival` | FARM-H3-REGION-01, FARM-H3-REGION-02, FARM-H3-REGION-03, FARM-H3-REGION-04 | `E5TheoryQualified` |
| 3 | `area-set:theory:logistics-hub-region` | `CityHubLogisticsResilience` | CITY-H3-HUB-01, CITY-H3-HUB-02, CITY-H3-HUB-03 | `E5TheoryQualified` |
| 4 | `area-set:theory:lowrise-market-region` | `TownLivingMarketSafety` | TOWN-H3-VILLAGE-01, TOWN-H3-VILLAGE-02, TOWN-H3-VILLAGE-03, TOWN-H3-VILLAGE-04 | `E5TheoryQualified` |

## H2 팩 주도 패턴

| 패턴 코드 | 배치 공간 이름 | 게임플레이 활용 유형 | 공간 형태 | 기준 크기 | 기존 StableId |
| --- | --- | --- | --- | --- | --- |
| `CITY-H2-INBOUND-01` | Hub 입고장·창고 전면 블록 | 시티 허브 입고 패턴 01 — 검수·창고형 | `StreetBlock` | 160m × 80m | `h2-candidate:hub-inbound-storage` |
| `CITY-H2-MAINTENANCE-01` | Hub 정비고·차량 마당 블록 | 시티 허브 정비 패턴 01 — 차량·시설 복구형 | `StreetBlock` | 160m × 80m | `h2-candidate:hub-maintenance-yard` |
| `CITY-H2-OUTBOUND-01` | Hub 출고장·차량 대기 블록 | 시티 허브 출고 패턴 01 — 피킹·차량형 | `LinearBlock` | 240m × 80m | `h2-candidate:hub-outbound-vehicle` |
| `CITY-H2-POWER-01` | Hub 비상전력실·보관동 블록 | 시티 허브 비상 패턴 01 — 전력·보관 유지형 | `CompoundBlock` | 200m × 218.56m | `h2-candidate:hub-emergency-power` |
| `CITY-H2-QUARANTINE-01` | Hub 검역장·격리창고 블록 | 시티 허브 격리 패턴 01 — 검역·임시적치형 | `StreetBlock` | 160m × 160m | `h2-candidate:hub-quarantine-staging` |
| `CITY-H2-RETURN-01` | Hub 반품장·격리 적치 블록 | 시티 허브 반품 패턴 01 — 회수·격리형 | `StreetBlock` | 160m × 80m | `h2-candidate:hub-returns-processing` |
| `CITY-H2-STORAGE-01` | Hub 장기·저온 창고 블록 | 시티 허브 보관 패턴 01 — 장기·저온형 | `StreetBlock` | 160m × 80m | `h2-candidate:hub-longterm-cold-storage` |
| `FARM-H2-HARVEST-01` | 생산구획·집하마당 블록 | 팜 수확 패턴 01 — 집중 집하형 | `StreetBlock` | 160m × 160m | `h2-candidate:farm-harvest-throughput` |
| `FARM-H2-INCIDENT-01` | 점검마당·격리구획 블록 | 팜 사건대응 패턴 01 — 점검·격리형 | `StreetBlock` | 194m × 160m | `h2-candidate:farm-incident-containment` |
| `FARM-H2-IRRIGATION-01` | 농수로·급수시설 블록 | 팜 관수 패턴 01 — 농수로·급수형 | `LinearBlock` | 240m × 80m | `h2-candidate:farm-irrigation-service` |
| `FARM-H2-PREP-01` | 종자창고·농기구고 블록 | 팜 준비 패턴 01 — 종자·농기구형 | `CompoundBlock` | 240m × 80m | `h2-candidate:farm-seed-and-tools` |
| `FARM-H2-PROCESSING-01` | 세척장·선별장·포장장 블록 | 팜 후처리 패턴 01 — 세척·선별·포장형 | `LinearBlock` | 320m × 80m | `h2-candidate:farm-wash-sort-pack` |
| `FARM-H2-PRODUCTION-01` | 고지대 경작지 블록 | 팜 생산 패턴 01 — 고지대 경작형 | `StreetBlock` | 160m × 80m | `h2-candidate:highland-production` |
| `FARM-H2-RECOVERY-01` | 복구 작업장·복원 인계장 블록 | 팜 손실회복 패턴 01 — 복원 인계형 | `LinearBlock` | 240m × 128m | `h2-candidate:farm-loss-restoration-handoff` |
| `FARM-H2-SHIPPING-01` | 작업마당·출하장 블록 | 팜 출하 패턴 01 — 작업마당·상차형 | `LinearBlock` | 240m × 80m | `h2-candidate:farm-processing-shipping` |
| `FARM-H2-SUPPORT-01` | 작업자 대기소·정비고 블록 | 팜 작업지원 패턴 01 — 대기·정비형 | `CompoundBlock` | 200m × 218.56m | `h2-candidate:farm-worker-support` |
| `MIX-H2-FARM-HUB-01` | 농장 출구–Hub 진입 회랑 블록 | 팜–허브 회랑 패턴 01 — 농산물 운송형 | `LinearBlock` | 160m × 80m | `h2-candidate:farm-hub-corridor` |
| `MIX-H2-HUB-TOWN-01` | Hub 출구–Town 입고 회랑 블록 | 허브–타운 회랑 패턴 01 — 시장 배송형 | `LinearBlock` | 240m × 80m | `h2-candidate:hub-town-corridor` |
| `MIX-H2-NATURE-FARM-01` | 숲 가장자리 경작지 블록 | 네이처–팜 전환 패턴 01 — 숲 경계 생산형 | `TerrainAdaptiveBlock` | 240m × 136m | `h2-candidate:forest-edge-farm` |
| `MIX-H2-NATURE-TOWN-01` | 자연권 출구–생활권 구호 인계 블록 | 네이처–타운 전환 패턴 01 — 생활권 대피·구호형 | `LinearBlock` | 240m × 80m | `h2-candidate:nature-town-relief-transition` |
| `NATURE-H2-BUFFER-01` | 산림·수변 완충지 블록 | 네이처 완충 패턴 01 — 산림·수변형 | `TerrainAdaptiveBlock` | 160m × 136m | `h2-candidate:nature-water-buffer` |
| `NATURE-H2-DEFENSE-01` | 방어환·야영지 블록 | 네이처 방어 패턴 01 — 야간 방어형 | `RingBlock` | 200m × 218.56m | `h2-candidate:nature-defense-ring` |
| `NATURE-H2-ENCOUNTER-01` | 조우로·이탈로 블록 | 네이처 조우 패턴 01 — 몬스터 접근·이탈형 | `TerrainAdaptiveBlock` | 240m × 136m | `h2-candidate:nature-encounter-route` |
| `NATURE-H2-HOME-01` | 안전 생활핵·보급 거점 블록 | 네이처 생활 패턴 01 — 안전 생활핵형 | `CompoundBlock` | 200m × 218.56m | `h2-candidate:nature-home-core` |
| `NATURE-H2-RECOVERY-01` | 복원 작업지·회복 쉼터 블록 | 네이처 회복 패턴 01 — 경로복원·안전회복형 | `TerrainAdaptiveBlock` | 172m × 144m | `h2-candidate:nature-restoration-recovery` |
| `NATURE-H2-THREAT-01` | 위험 흔적길·긴급 대피로 블록 | 네이처 위협 패턴 01 — 추적·긴급후퇴형 | `TerrainAdaptiveBlock` | 214m × 186m | `h2-candidate:nature-threat-response` |
| `NATURE-H2-TRAIL-01` | 탐색로·대피 쉼터 블록 | 네이처 탐색 패턴 01 — 탐색·대피형 | `TerrainAdaptiveBlock` | 240m × 136m | `h2-candidate:nature-trail-shelter` |
| `TOWN-H2-CIRCULAR-01` | 반품 집하장·폐기물 처리장 블록 | 타운 순환 패턴 01 — 반품·폐기물형 | `LinearBlock` | 240m × 80m | `h2-candidate:town-returns-waste` |
| `TOWN-H2-RELIEF-01` | 주민 안내소·구호 물자 인계장 블록 | 타운 구호 패턴 01 — 회수안내·주민지원형 | `CompoundBlock` | 218m × 148m | `h2-candidate:town-recall-relief` |
| `TOWN-H2-SAFETY-01` | 오염 점검장·격리구획 블록 | 타운 안전 패턴 01 — 오염점검·격리형 | `StreetBlock` | 218m × 154m | `h2-candidate:town-contamination-control` |
| `TOWN-H2-SERVICE-01` | 주민지원소·공동수령장 블록 | 타운 서비스 패턴 01 — 주민지원·공동수령형 | `CompoundBlock` | 200m × 218.56m | `h2-candidate:town-resident-service` |
| `TOWN-H2-VILLAGE-01` | 저층 주거·생활광장 블록 | 타운 빌리지 패턴 01 — 저층 생활광장형 | `StreetBlock` | 160m × 80m | `h2-candidate:lowrise-residential` |
| `TOWN-H2-VILLAGE-02` | 마트·생활상가 블록 | 타운 빌리지 패턴 02 — 생활상권형 | `StreetBlock` | 240m × 160m | `h2-candidate:market-life-commerce` |
| `TOWN-H2-VILLAGE-03` | 저층 주거 골목 블록 | 타운 빌리지 패턴 03 — 주거골목형 | `StreetBlock` | 160m × 80m | `h2-candidate:town-residential-alley` |

## H3 팩 주도 패턴

| 패턴 코드 | 배치 구역 이름 | 게임플레이 활용 유형 | 구역 형태 | 포함 H2 패턴 | 기존 StableId |
| --- | --- | --- | --- | --- | --- |
| `CITY-H3-HUB-01` | Hub 입고장·창고·출고장 구역 | 시티 허브 경관 01 — 입고·보관·출고형 | `DistrictAssembly` | CITY-H2-INBOUND-01, CITY-H2-OUTBOUND-01 | `h3-candidate:jinbu-hub` |
| `CITY-H3-HUB-02` | Hub 검역장·저온창고 구역 | 시티 허브 경관 02 — 품질·격리·저온대응형 | `DistrictAssembly` | CITY-H2-INBOUND-01, CITY-H2-QUARANTINE-01, CITY-H2-STORAGE-01, CITY-H2-OUTBOUND-01 | `h3-candidate:resilient-logistics-hub` |
| `CITY-H3-HUB-03` | Hub 정비고·비상전력 구역 | 시티 허브 경관 03 — 정비·비상운영 회복형 | `DistrictAssembly` | CITY-H2-MAINTENANCE-01, CITY-H2-POWER-01, CITY-H2-OUTBOUND-01 | `h3-candidate:hub-maintenance-emergency-loop` |
| `FARM-H3-REGION-01` | 고지대 경작지·숲경계 구역 | 팜 경관 01 — 고지대 농장형 | `LandscapeDistrictAssembly` | FARM-H2-PRODUCTION-01, FARM-H2-SHIPPING-01, MIX-H2-NATURE-FARM-01 | `h3-candidate:highland-farm` |
| `FARM-H3-REGION-02` | 생산구획·후처리장 구역 | 팜 경관 02 — 생산·후처리 캠퍼스형 | `DistrictAssembly` | FARM-H2-PRODUCTION-01, FARM-H2-PREP-01, FARM-H2-PROCESSING-01, FARM-H2-SHIPPING-01 | `h3-candidate:farm-processing-campus` |
| `FARM-H3-REGION-03` | 격리구획·복구 작업장 구역 | 팜 경관 03 — 사건격리·손실회복형 | `DistrictAssembly` | FARM-H2-INCIDENT-01, FARM-H2-RECOVERY-01 | `h3-candidate:farm-incident-recovery` |
| `FARM-H3-REGION-04` | 관수 경작지·출하장 구역 | 팜 경관 04 — 계절 생산·출하 순환형 | `DistrictAssembly` | FARM-H2-IRRIGATION-01, FARM-H2-HARVEST-01, FARM-H2-SHIPPING-01 | `h3-candidate:farm-seasonal-production-loop` |
| `MIX-H3-FARM-HUB-01` | 농장 출하문–Hub 입고문 회랑 | 팜–허브 연결 경관 01 — 생산물류형 | `CorridorAssembly` | MIX-H2-FARM-HUB-01 | `h3-candidate:farm-hub-logistics` |
| `MIX-H3-HUB-TOWN-01` | Hub 출고문–Town 입고문 회랑 | 허브–타운 연결 경관 01 — 시장배송형 | `CorridorAssembly` | MIX-H2-HUB-TOWN-01 | `h3-candidate:hub-town-logistics` |
| `MIX-H3-NATURE-TOWN-01` | Nature 대피문–Town 구호문 회랑 | 네이처–타운 연결 경관 01 — 대피·구호 인계형 | `CorridorAssembly` | TOWN-H2-RELIEF-01, MIX-H2-NATURE-TOWN-01, NATURE-H2-RECOVERY-01 | `h3-candidate:nature-town-relief-loop` |
| `NATURE-H3-REGION-01` | 탐색로·산림완충지 구역 | 네이처 경관 01 — 탐색·완충형 | `LandscapeDistrictAssembly` | NATURE-H2-BUFFER-01, MIX-H2-NATURE-FARM-01 | `h3-candidate:nature-exploration-buffer` |
| `NATURE-H3-REGION-02` | 생활핵·대피로·복원쉼터 구역 | 네이처 경관 02 — 위협·회복형 | `LandscapeDistrictAssembly` | NATURE-H2-THREAT-01, NATURE-H2-RECOVERY-01 | `h3-candidate:nature-threat-recovery` |
| `NATURE-H3-REGION-03` | 탐색로·대피망 구역 | 네이처 경관 03 — 탐색길·대피망형 | `LandscapeDistrictAssembly` | NATURE-H2-TRAIL-01, NATURE-H2-BUFFER-01 | `h3-candidate:nature-trail-network` |
| `NATURE-H3-REGION-04` | 생활핵·조우로·방어환 구역 | 네이처 경관 04 — 생활핵·조우·방어 폐루프형 | `LandscapeDistrictAssembly` | NATURE-H2-HOME-01, NATURE-H2-ENCOUNTER-01, NATURE-H2-DEFENSE-01 | `h3-candidate:nature-home-encounter-defense` |
| `TOWN-H3-VILLAGE-01` | 저층 주거·마트 구역 | 타운 빌리지 경관 01 — 저층 생활·시장형 | `DistrictAssembly` | TOWN-H2-VILLAGE-01, TOWN-H2-VILLAGE-02 | `h3-candidate:lowrise-market-town` |
| `TOWN-H3-VILLAGE-02` | 시장·반품집하 구역 | 타운 빌리지 경관 02 — 반품·회수 순환시장형 | `DistrictAssembly` | TOWN-H2-VILLAGE-02, TOWN-H2-CIRCULAR-01 | `h3-candidate:circular-market-town` |
| `TOWN-H3-VILLAGE-03` | 오염격리·주민구호 구역 | 타운 빌리지 경관 03 — 오염통제·주민구호형 | `DistrictAssembly` | TOWN-H2-SAFETY-01, TOWN-H2-RELIEF-01 | `h3-candidate:town-contamination-relief` |
| `TOWN-H3-VILLAGE-04` | 주거골목·주민지원 구역 | 타운 빌리지 경관 04 — 주민서비스·공동수령형 | `DistrictAssembly` | TOWN-H2-VILLAGE-03, TOWN-H2-SERVICE-01, TOWN-H2-VILLAGE-01 | `h3-candidate:town-resident-service-loop` |

## 증거 경계

- `E5TheoryQualified`는 명시적 세계 의도와 H3를 가진 특정 Theory AreaSet 공간 인스턴스다.
- 사람 승인, 공공데이터 결속, Unity Runtime 또는 실제 플레이를 주장하지 않는다.
- 공공데이터는 E6, 실제 서버·저장 Scene 플레이는 E7에서 별도로 검증한다.

## 다음 패턴 확장 대기열

생산 순서는 팩 단독 H2 → 팩 내부 H3 → 주도·보조 팩 조합 → 혼합 H2 → 혼합 H3다.

| 우선 | 예약 패턴 코드 | 한국어 이름 | 단계 | 조합 | 게임 플레이 목적 |
| --- | --- | --- | --- | --- | --- |
