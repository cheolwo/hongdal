# 팩별 H1 표현 탐색 카드

| 팩 | 카드 | 기준 문법 A/B/C | 연결 행동 H1 |
| --- | --- | --- | --- |
| `City` | `h1-expression:city:공동주택-생활마당` 공동주택 생활마당 — City 단독 표현 | city:공동주택 생활마당:A, city:공동주택 생활마당:B, city:공동주택 생활마당:C | h1-stock:town-staff-rest |
| `City` | `h1-expression:city:도심-마트-앞마당` 도심 마트 앞마당 — City 단독 표현 | city:도심 마트 앞마당:A, city:도심 마트 앞마당:B, city:도심 마트 앞마당:C | h1-stock:town-market-receiving, h1-stock:town-returns |
| `City` | `h1-expression:city:먹거리-상점-골목` 먹거리 상점 골목 — City 단독 표현 | city:먹거리 상점 골목:A, city:먹거리 상점 골목:B, city:먹거리 상점 골목:C | h1-stock:town-market-display |
| `City` | `h1-expression:city:물류-station-진입부` 물류 Station 진입부 — City 단독 표현 | city:물류 Station 진입부:A, city:물류 Station 진입부:B, city:물류 Station 진입부:C | h1-stock:hub-cold-storage, h1-stock:hub-long-term-storage, h1-stock:hub-market-transfer, h1-stock:hub-receiving-storage |
| `City` | `h1-expression:city:상하차-dock` 상하차 Dock — City 단독 표현 | city:상하차 Dock:A, city:상하차 Dock:B, city:상하차 Dock:C | h1-stock:hub-outbound-staging, h1-stock:hub-quarantine, h1-stock:hub-receiving-storage, h1-stock:hub-returns, h1-stock:hub-temporary-staging, h1-stock:hub-vehicle-yard |
| `City` | `h1-expression:city:화물-대기-야드` 화물 대기 야드 — City 단독 표현 | city:화물 대기 야드:A, city:화물 대기 야드:B, city:화물 대기 야드:C | h1-stock:hub-cold-storage, h1-stock:hub-long-term-storage, h1-stock:hub-outbound-staging, h1-stock:hub-quarantine, h1-stock:hub-receiving-storage, h1-stock:hub-returns, h1-stock:hub-service-maintenance, h1-stock:hub-temporary-staging, h1-stock:hub-vehicle-yard |
| `Farm` | `h1-expression:farm:감자밭-두렁` 감자밭 두렁 — Farm 단독 표현 | farm:감자밭 두렁:A, farm:감자밭 두렁:B, farm:감자밭 두렁:C | h1-stock:farm-production |
| `Farm` | `h1-expression:farm:과수원-블록` 과수원 블록 — Farm 단독 표현 | farm:과수원 블록:A, farm:과수원 블록:B, farm:과수원 블록:C |  |
| `Farm` | `h1-expression:farm:논-필지-농수로-표현` 논 필지·농수로 표현 — Farm 단독 표현 | farm:논 필지·농수로 표현:A, farm:논 필지·농수로 표현:B, farm:논 필지·농수로 표현:C |  |
| `Farm` | `h1-expression:farm:농산물-집하-직판장` 농산물 집하·직판장 — Farm 단독 표현 | farm:농산물 집하·직판장:A, farm:농산물 집하·직판장:B, farm:농산물 집하·직판장:C | h1-stock:farm-exposure-inspection, h1-stock:farm-harvest-staging, h1-stock:farm-incident-quarantine, h1-stock:farm-loss-recovery, h1-stock:farm-restoration-supply, h1-stock:farm-sorting, h1-stock:farm-work-yard |
| `Farm` | `h1-expression:farm:시설하우스-단동` 시설하우스 단동 — Farm 단독 표현 | farm:시설하우스 단동:A, farm:시설하우스 단동:B, farm:시설하우스 단동:C | h1-stock:farm-maintenance-yard, h1-stock:farm-seed-preparation, h1-stock:farm-tool-storage, h1-stock:farm-weather-protection |
| `Farm` | `h1-expression:farm:시설하우스-병렬단지` 시설하우스 병렬단지 — Farm 단독 표현 | farm:시설하우스 병렬단지:A, farm:시설하우스 병렬단지:B, farm:시설하우스 병렬단지:C |  |
| `Farm` | `h1-expression:farm:헛간-작업마당` 헛간 작업마당 — Farm 단독 표현 | farm:헛간 작업마당:A, farm:헛간 작업마당:B, farm:헛간 작업마당:C | h1-stock:farm-exposure-inspection, h1-stock:farm-harvest-staging, h1-stock:farm-incident-quarantine, h1-stock:farm-loading-gate, h1-stock:farm-loss-recovery, h1-stock:farm-maintenance-yard, h1-stock:farm-seed-preparation, h1-stock:farm-sorting, h1-stock:farm-tool-storage, h1-stock:farm-washing, h1-stock:farm-weather-protection, h1-stock:farm-work-yard, h1-stock:farm-worker-waiting |
| `Farm` | `h1-expression:farm:혼합-작물밭` 혼합 작물밭 — Farm 단독 표현 | farm:혼합 작물밭:A, farm:혼합 작물밭:B, farm:혼합 작물밭:C | h1-stock:farm-production |
| `Nature` | `h1-expression:nature:개울-회랑` 개울 회랑 — Nature 단독 표현 | nature:개울 회랑:A, nature:개울 회랑:B, nature:개울 회랑:C |  |
| `Nature` | `h1-expression:nature:고지대-노출지` 고지대 노출지 — Nature 단독 표현 | nature:고지대 노출지:A, nature:고지대 노출지:B, nature:고지대 노출지:C | h1-stock:nature-lookout, h1-stock:nature-threat-watch |
| `Nature` | `h1-expression:nature:바위-절개지` 바위 절개지 — Nature 단독 표현 | nature:바위 절개지:A, nature:바위 절개지:B, nature:바위 절개지:C |  |
| `Nature` | `h1-expression:nature:산-능선` 산 능선 — Nature 단독 표현 | nature:산 능선:A, nature:산 능선:B, nature:산 능선:C | h1-stock:nature-lookout, h1-stock:nature-threat-watch |
| `Nature` | `h1-expression:nature:산길-바위-길목` 산길·바위 길목 — Nature 단독 표현 | nature:산길·바위 길목:A, nature:산길·바위 길목:B, nature:산길·바위 길목:C | h1-stock:nature-emergency-retreat, h1-stock:nature-exploration-buffer, h1-stock:nature-incident-trace, h1-stock:nature-shelter, h1-stock:nature-trailhead |
| `Nature` | `h1-expression:nature:수변-완충지` 수변 완충지 — Nature 단독 표현 | nature:수변 완충지:A, nature:수변 완충지:B, nature:수변 완충지:C | h1-stock:farm-washing, h1-stock:nature-restoration-site |
| `Nature` | `h1-expression:nature:숲-가장자리` 숲 가장자리 — Nature 단독 표현 | nature:숲 가장자리:A, nature:숲 가장자리:B, nature:숲 가장자리:C | h1-stock:farm-restoration-supply, h1-stock:nature-emergency-retreat, h1-stock:nature-farm-edge, h1-stock:nature-restoration-site, h1-stock:nature-trailhead |
| `Nature` | `h1-expression:nature:숲-빈터-고사목` 숲 빈터·고사목 — Nature 단독 표현 | nature:숲 빈터·고사목:A, nature:숲 빈터·고사목:B, nature:숲 빈터·고사목:C | h1-stock:nature-exploration-buffer, h1-stock:nature-incident-trace, h1-stock:nature-safe-recovery-camp, h1-stock:nature-shelter |
| `Nature` | `h1-expression:nature:초지-야생화` 초지·야생화 — Nature 단독 표현 | nature:초지·야생화:A, nature:초지·야생화:B, nature:초지·야생화:C | h1-stock:nature-safe-recovery-camp |
| `Nature` | `h1-expression:nature:침엽수림-군집` 침엽수림 군집 — Nature 단독 표현 | nature:침엽수림 군집:A, nature:침엽수림 군집:B, nature:침엽수림 군집:C |  |
| `Nature` | `h1-expression:nature:혼효림-군집` 혼효림 군집 — Nature 단독 표현 | nature:혼효림 군집:A, nature:혼효림 군집:B, nature:혼효림 군집:C |  |
| `Nature` | `h1-expression:nature:활엽수림-군집` 활엽수림 군집 — Nature 단독 표현 | nature:활엽수림 군집:A, nature:활엽수림 군집:B, nature:활엽수림 군집:C |  |
| `Town` | `h1-expression:town:근린-놀이터` 근린 놀이터 — Town 단독 표현 | town:근린 놀이터:A, town:근린 놀이터:B, town:근린 놀이터:C | h1-stock:town-living-square |
| `Town` | `h1-expression:town:버스-정류장-보행-쉼터` 버스 정류장·보행 쉼터 — Town 단독 표현 | town:버스 정류장·보행 쉼터:A, town:버스 정류장·보행 쉼터:B, town:버스 정류장·보행 쉼터:C | h1-stock:farm-worker-waiting, h1-stock:town-resident-pickup |
| `Town` | `h1-expression:town:생활-공공광장` 생활 공공광장 — Town 단독 표현 | town:생활 공공광장:A, town:생활 공공광장:B, town:생활 공공광장:C | h1-stock:town-living-square, h1-stock:town-neighborhood-service, h1-stock:town-staff-rest |
| `Town` | `h1-expression:town:읍내-상점-전면` 읍내 상점 전면 — Town 단독 표현 | town:읍내 상점 전면:A, town:읍내 상점 전면:B, town:읍내 상점 전면:C | h1-stock:town-market-display, h1-stock:town-neighborhood-service, h1-stock:town-resident-pickup, h1-stock:town-returns |
| `Town` | `h1-expression:town:저층-주택-블록` 저층 주택 블록 — Town 단독 표현 | town:저층 주택 블록:A, town:저층 주택 블록:B, town:저층 주택 블록:C |  |
| `Town` | `h1-expression:town:정원-담장-경계` 정원·담장 경계 — Town 단독 표현 | town:정원·담장 경계:A, town:정원·담장 경계:B, town:정원·담장 경계:C | h1-stock:town-waste |
