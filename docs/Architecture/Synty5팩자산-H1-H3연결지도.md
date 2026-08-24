# Synty 5팩 자산과 H1~H3 연결 지도

## 목적과 권위 경계

이 문서는 Unity에 설치된 POLYGON Nature·Farm·Town·City·Construction 다섯 팩의 전수 기술 재고를 사람이 읽을 수 있게 요약하고, 각 자산 분류가 현재 어떤 H1 작업공간·H2 블록·H3 경관을 **표현하는 데 관련되는지** 연결한다.

```text
Synty Prefab·의미 자산군
  → H 공간의 표현 후보
  → 조립 Prefab·VisualRoot
  → 서버 상태의 시각 투영
```

Prefab 하나가 H1인 것은 아니다. Prefab 이름·경로·GUID·GameObject는 H StableId, 공간 능력, WI 또는 Simulation 상태를 만들지 않는다. H 권위는 [`catalog.v3.json`](../../eng/world-seedbeds/synty-bottom-up-inventory/catalog.v3.json)의 위치 독립 공간 지식과 Actual E5 배치에 있고, Unity 자산은 그 결과를 표현한다.

## 전수 재고

Unity `synty-pack-inventory.v2` 기술 대장은 2026-08-19 기준 2,346개 Prefab을 1,499개 의미 자산군으로 분류한다. 자동 분류 2,345개, 사람 검토 대기 1개다. 유료 원본 파일명·경로·GUID 전체 목록은 Unity 내부 Catalog에만 유지한다.

| 팩 | Prefab | 주된 의미 | H 관계 방식 |
| --- | ---: | --- | --- |
| Nature | 227 | 지형·수계·바위·식생·탐색 분위기 | Nature H1 표현과 H2·H3 장소 골격 |
| Farm | 498 | 생산·작물·농장 시설·작업·집하 | Farm H1 표현과 생산·후처리 H2·H3 |
| Town | 702 | 저층 주거·상점·생활 서비스·주민 물품 | Town H1 표현과 생활·시장 H2·H3 |
| City | 335 | 고밀도 건축·도로·물류·기반시설 | Hub·City 업무 H1 표현과 물류 H2·H3 |
| Construction | 584 | 골조·비계·도구·차단·공사·복구 | 독립 H가 아닌 모든 영역의 상태·기능 지원층 |
| 합계 | 2,346 | 1,499개 의미 자산군 | 모두 `PresentationOnly` |

## 분류를 H에 연결하는 규칙

| 자산 분류 | H1에서의 관계 | H2에서의 관계 | H3에서의 관계 |
| --- | --- | --- | --- |
| Terrain·Buildings·Environments·Plants·Rocks·Trees | 장소 골격과 경계 표현 | 여러 H1의 면·도로·건축 덩어리 연결 | 경관 실루엣·밀도·완충대 구성 |
| Props·Generic | 작업대·저장·표지·생활 단서 | 내부 작업 순서와 구역 읽기 | 구역의 기능 차이와 상태 계보 표시 |
| Characters | H1을 사용하는 행위자 표현 | 블록 안 역할 분포 표현 | 경관 생활 밀도 표현 |
| Vehicles | 진입·상차·이동 상태 표현 | 화물로·서비스로 표현 | H3 외부 연결구와 이동 투영 |
| Items·Tools | 예약·작업 단계의 시각 단서 | 공사·수리·포장 흐름 표현 | 경관 상태의 세부 단서 |
| FX | 서버가 확정한 물·연기·먼지·가동 상태 표현 | 블록 상태 채널 | 경관 분위기·위험도 표현 |
| ManualReview | 자동 배치 금지 | 관계 없음 | 관계 없음 |

Characters·Vehicles·Items·Tools·FX는 H의 구성 사실이 아니라 투영 채널이다. 이들이 존재해도 작업자 자격, 재고, 이동 경로, 피해 또는 완료가 확정되지 않는다.

## Nature 팩 — 심리 영역의 장소 골격

### 들어 있는 자산

| 분류 | 수량 | 의미 자산군 예시 |
| --- | ---: | --- |
| FX | 24 | 구름·안개·수계·분위기 상태 |
| ManualReview | 1 | `Misc` 자동 정규화 실패 항목, 사람 검토 전 사용 금지 |
| Plants | 42 | 풀·양치·갈대·꽃·덤불 |
| Props | 31 | 자연 길목·흔적·소형 환경 단서 |
| Rocks | 30 | 바위·돌무더기·절개 경계 |
| Terrain | 33 | 산·하천·수변·지면 조각 |
| Trees | 66 | 활엽·침엽·고사목·그루터기·수풀 |
| 합계 | 227 | 131개 의미 자산군 |

### H1 관련성

- 팩 표현 H1: `개울-회랑`, `고지대-노출지`, `바위-절개지`, `산-능선`, `산길-바위-길목`, `수변-완충지`, `숲-가장자리`, `숲-빈터-고사목`, `초지-야생화`, `침엽수림-군집`, `혼효림-군집`, `활엽수림-군집`.
- 상호작용 H1: `h1-stock:nature-trailhead`, `nature-lookout`, `nature-shelter`, `nature-threat-watch`, `nature-incident-trace`, `nature-emergency-retreat`, `nature-restoration-site`, `nature-safe-recovery-camp`, `nature-exploration-buffer`, `nature-farm-edge`.
- Terrain·Trees·Rocks·Plants는 장소 골격, Props는 흔적·관찰·대피 단서, FX는 위협·회복 상태 표현으로 사용한다.

### H2·H3 관련성

| H2 블록 | 관련 H3 경관 |
| --- | --- |
| `h2-candidate:nature-home-core` | `h3-candidate:nature-home-encounter-defense` |
| `nature-encounter-route`, `nature-defense-ring` | `nature-home-encounter-defense` |
| `nature-threat-response`, `nature-restoration-recovery` | `nature-threat-recovery` |
| `nature-trail-shelter`, `nature-water-buffer` | `nature-trail-network`, `nature-exploration-buffer` |
| `nature-town-relief-transition` | `nature-town-relief-loop` |

Nature 자산은 Farm 사고의 직접 원인을 해결하는 표현으로 사용하지 않는다. 심리적 완충·탐색·위협 관찰·후퇴·복귀 공간을 표현한다.

## Farm 팩 — 생산·후처리 업무 영역

### 들어 있는 자산

| 분류 | 수량 | 의미 자산군 예시 |
| --- | ---: | --- |
| Buildings | 17 | 농가·헛간·온실·저장시설 |
| Characters | 14 | 농장 작업자 외형과 부착 표현 |
| Environments | 67 | 농로·울타리·지면·경계 |
| FX | 11 | 물·먼지·작업 상태 |
| Generic | 39 | 공통 농장 조립 부품 |
| Plants | 173 | 감자·곡물·채소·과수·꽃과 성장 변형 |
| Props | 166 | 농기구·상자·건초·급수·전력·작업 소품 |
| Vehicles | 11 | 농업·운반 차량 표현 |
| 합계 | 498 | 363개 의미 자산군 |

### H1 관련성

- 팩 표현 H1: `감자밭-두렁`, `과수원-블록`, `논-필지-농수로-표현`, `농산물-집하-직판장`, `시설하우스-단동`, `시설하우스-병렬단지`, `헛간-작업마당`, `혼합-작물밭`.
- 생산 H1: `h1-stock:farm-production`, `farm-seed-preparation`, `farm-tool-storage`, `farm-worker-waiting`.
- 수확·후처리 H1: `farm-harvest-staging`, `farm-washing`, `farm-sorting`, `farm-work-yard`, `farm-loading-gate`.
- 사건·회복 H1: `farm-exposure-inspection`, `farm-incident-quarantine`, `farm-weather-protection`, `farm-loss-recovery`, `farm-maintenance-yard`, `farm-restoration-supply`.
- 연결 H1: `farm-hub-corridor`, `nature-farm-edge`.

### H2·H3 관련성

| H2 블록 | 관련 H3 경관 |
| --- | --- |
| `highland-production`, `forest-edge-farm` | `h3-candidate:highland-farm` |
| `farm-worker-support`, `farm-seed-and-tools`, `farm-wash-sort-pack`, `farm-processing-shipping` | `farm-processing-campus` |
| `farm-harvest-throughput`, `farm-irrigation-service` | `farm-seasonal-production-loop` |
| `farm-incident-containment`, `farm-loss-restoration-handoff` | `farm-incident-recovery` |
| `farm-hub-corridor`, `farm-processing-shipping` | `farm-hub-logistics` |

Plants·Terrain만 배치한 밭은 H1이 아니다. 밭갈기·파종·재배·수확 WI, 작업 용량, 접근로와 다음 H1 인계가 함께 있어야 `farm-production`을 표현할 수 있다.

## Town 팩 — 저층 생활·시장 업무 영역

### 들어 있는 자산

| 분류 | 수량 | 의미 자산군 예시 |
| --- | ---: | --- |
| Buildings | 143 | 저층 주택·상점·생활시설·건물 조립부 |
| Characters | 9 | 주민·서비스 역할 외형 |
| Environments | 97 | 도로·보도·정원·담장·생활 경계 |
| Generic | 33 | 공통 생활 조립 부품 |
| Items | 72 | 생활·상점·주거 휴대물과 소품 |
| Props | 340 | 가구·표지·상점·정원·생활 서비스 단서 |
| Vehicles | 8 | 주민·서비스 이동 표현 |
| 합계 | 702 | 435개 의미 자산군 |

### H1 관련성

- 팩 표현 H1: `근린-놀이터`, `버스-정류장-보행-쉼터`, `생활-공공광장`, `읍내-상점-전면`, `저층-주택-블록`, `정원-담장-경계`.
- 생활·시장 H1: `h1-stock:town-living-square`, `h1-stock:town-market-receiving`, `h1-stock:town-market-display`, `h1-stock:town-order-packing`, `h1-stock:town-resident-pickup`, `h1-stock:town-neighborhood-service`, `h1-stock:town-staff-rest`.
- 반품·안전 H1: `town-returns`, `town-waste`, `town-contamination-inspection`, `town-contamination-quarantine`, `town-recall-service`, `town-cleanup-transfer`, `town-nature-relief`.

### H2·H3 관련성

| H2 블록 | 관련 H3 경관 |
| --- | --- |
| `lowrise-residential`, `town-residential-alley` | `h3-candidate:lowrise-market-town` |
| `market-life-commerce`, `town-market-receiving`, `town-order-fulfillment` | `h3-candidate:town-market-fulfillment` |
| `town-resident-service` | `town-resident-service-loop` |
| `town-returns-waste` | `circular-market-town` |
| `town-contamination-control`, `town-recall-relief` | `town-contamination-relief` |
| `nature-town-relief-transition` | `nature-town-relief-loop` |

Town의 건물·가구 수는 주민 수요·재고·주문을 만들지 않는다. 시장 WI와 주민 수령·반품 상태는 서버가 소유한다.

## City 팩 — 고밀도 생활·Hub 물류 업무 영역

### 들어 있는 자산

| 분류 | 수량 | 의미 자산군 예시 |
| --- | ---: | --- |
| Buildings | 76 | 공동주택·상업·사무·Station·도시 건축 |
| Characters | 9 | 도시 주민·업무 역할 외형 |
| Environments | 65 | 도로·보도·수변·도시 경계 |
| FX | 2 | 도시 상태·분위기 표현 |
| Props | 174 | 선반·상자·표지·전력·배관·교통 소품 |
| Vehicles | 9 | 도시·물류 차량 표현 |
| 합계 | 335 | 221개 의미 자산군 |

### H1 관련성

- 팩 표현 H1: `공동주택-생활마당`, `도심-마트-앞마당`, `먹거리-상점-골목`, `물류-station-진입부`, `상하차-dock`, `화물-대기-야드`.
- Hub H1: `h1-stock:hub-receiving-storage`, `hub-temporary-staging`, `hub-quarantine`, `hub-cold-storage`, `hub-long-term-storage`, `hub-outbound-staging`, `hub-vehicle-yard`, `hub-service-maintenance`, `hub-returns`, `hub-market-transfer`.
- 연결 H1: `hub-town-corridor`, `road-facility-access`, `farm-hub-corridor`.

### H2·H3 관련성

| H2 블록 | 관련 H3 경관 |
| --- | --- |
| `hub-inbound-storage`, `hub-quarantine-staging`, `hub-longterm-cold-storage` | `h3-candidate:resilient-logistics-hub`, `jinbu-hub` |
| `hub-fulfillment`, `hub-outbound-vehicle` | `h3-candidate:hub-fulfillment-operations` |
| `hub-maintenance-yard`, `hub-emergency-power` | `hub-maintenance-emergency-loop` |
| `hub-returns-processing` | `resilient-logistics-hub` |
| `hub-town-corridor` | `hub-town-logistics` |
| Farm 쪽 출하 블록 + `farm-hub-corridor` + Hub 입고 블록 | `farm-hub-logistics` |

City 팩을 사용해도 Hub와 City의 업무 목적·상태·완료 증거는 합치지 않는다. 기존 `CityHub` 안정 식별자는 호환용이며 새 설계 판단에서는 독립 영역으로 분리한다.

## Construction 팩 — 공사·격리·복구 공통 상태층

### 들어 있는 자산

| 분류 | 수량 | 의미 자산군 예시 |
| --- | ---: | --- |
| Buildings | 74 | 골조·벽·기둥·임시 시설·설비 |
| Characters | 44 | 공사·정비 작업자 외형 |
| Environments | 36 | 구덩이·토사·작업 지면·공사 경계 |
| Generic | 19 | 공통 조립 부품 |
| Items | 49 | 자재·부품·계획·소모품 표현 |
| Props | 300 | 비계·바리케이드·발전기·탱크·적치·안전 소품 |
| Tools | 39 | 수리·조립·절단·도장 도구 |
| Vehicles | 23 | 공사·운반·정비 차량 |
| 합계 | 584 | 349개 의미 자산군 |

Construction은 `h1-expression:construction:*`, 독립 H2 또는 독립 H3를 만들지 않는다. 아래 상태층으로 기존 Nature·Farm·Town·Hub H에 결합한다.

| 상태층 | H1 적용 | H2 적용 | H3 적용 |
| --- | --- | --- | --- |
| 정상 운영 | 완성 시설·안전 표지·급수·전력 소켓 | 블록 핵심 기능이 읽히는 설비 배치 | 정상 경관 기준 표현 |
| 점검·정비 | 도구·작업대·이동 조명 | `farm-maintenance-yard`, `hub-maintenance-yard` 등 정비 흐름 | `farm-incident-recovery`, `hub-maintenance-emergency-loop` |
| 공사 진행 | 골조·비계·자재·공사 차량 | 생산·주거·물류 블록의 공사 상태 | H3 경관의 제한 구역과 우회 표현 |
| 손상·격리 | 노출 구조·구덩이·바리케이드·경고 | `farm-incident-containment`, `town-contamination-control`, `hub-quarantine-staging` | 영역별 사건·격리 경관 |
| 복구·재가동 | 발전기·용수·보강·정리 적치 | `nature-restoration-recovery`, `farm-loss-restoration-handoff`, `hub-emergency-power` | 회복·복구 H3의 재가동 표현 |

상태층이 Collider·NavMesh·업무 용량·필수 이동로를 바꾸면 단순 시각 상태가 아니다. 이 경우 관련 H1/H2의 새 revision과 E5 회귀 검토가 필요하다.

## H 조립 시 사용 순서

1. 플레이 목적과 WI에서 필요한 상호작용 H1을 선택한다.
2. 해당 팩의 표현 H1과 공간 골격 자산군을 고른다.
3. H1의 능력·용량·접근로·외부 연결구를 먼저 배치한다.
4. H2의 내부 의미 관계와 동선을 닫는다.
5. H3의 블록 간 관계·외부 연결 역할·귀환 경로를 닫는다.
6. Construction 상태층과 Character·Vehicle·Item·FX 투영을 마지막에 결합한다.
7. 원본 Prefab이 아니라 조립 Prefab·`VisualRoot`에서 참조하고, 저장 Scene·Play Mode·Game View 증거는 별도로 남긴다.

## 현재 판정

| 항목 | 상태 |
| --- | --- |
| 5팩 설치·전수 스캔 | 완료: 2,346개 |
| 정규화 분류·활용 트랙 | 완료: 2,345 자동, 1 수동 검토 |
| 팩별 H1~H3 관련성 문서화 | 완료: 이 문서 |
| 개별 의미 자산군의 H 조립 승인 | 미완료: 모판별 사람 검토 필요 |
| Actual E5 배치·Unity Runtime 활용 | 이 문서로 증명하지 않음 |

## 연결 문서

- [Synty 상향식 공간 재고 계획](Synty상향식공간재고계획.md)
- [심리·업무 영역 Synty 5팩 공간 조립 계획](심리업무영역Synty공간조립계획.md)
- [H1~H5 공간 포함 계층 조사](H1-H5공간포함계층조사.md)
- [AreaSet 구성 패턴](AreaSet구성패턴.md)
- [현재 H1~H5 선택 트리](../AI/authority-maps/03_H1_H5_CURRENT_TREE.md)
