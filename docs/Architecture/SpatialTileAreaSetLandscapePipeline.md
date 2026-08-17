# 공간 Tile·Area·AreaSet 경관 생성 파이프라인

## 목적과 권위 경계

공간 원본은 배치 가능 위치를, 환경부 면적 통계는 행정구역 전체의 경관 구성 목표를, Simulation은 Farm·Town·Hub 역할과 구체 작물을 결정한다. Synty Prefab과 높이 과장은 `PresentationOnly`이며 법정동·공간 관측·업무 완료를 변경하지 않는다.

```text
원본 snapshot → EPSG:5186 고정 Tile → 공간 Layer
→ LandAllocationResult → LandscapeCompositionPlan
→ 행정동·법정동 지역 Projection → 경관 완결 영역 → Area
→ 건물·공개 사업장 관계 → 건물 배치 계획
→ 그래픽 표현 계획 → ScenarioRoute → AreaSet
→ 배치·성능 검증 → 마지막 시각 자산 연결 → VisualRoot·Unity 산출물
```

고정 격자는 L0 8km, L1 2km, L2 500m이고 식별자는 `kr5186:l{level}:{x}:{y}`다. 생성 범위는 L0 300m, L1 150m, L2 60m Halo를 포함하고 최종 산출물은 가운데 핵심 범위만 사용한다. 결정적 seed는 타일 내부 순번이 아니라 EPSG:5186 세계 좌표와 의미 key로 계산한다.

`Tile`은 절단·재생성·캐시 단위이고, 사람이 미술·시뮬레이션·UI까지 완결 여부를 판단하는 단위는 `경관 완결 영역`이다. 첫 규격은 인접한 L2 500m 타일 2×2를 묶은 1km×1km이며, 전체 평창군을 먼저 생성하지 않고 이 범위의 L0 1개·L1 1개·L2 4개만 첫 공간 실행 입력으로 선택한다. `Area`는 법정동·Farm·Hub·Town 의미 범위, `AreaSet`은 여러 Area·회랑·완결 영역을 묶는 시나리오 범위다.

## 원본과 표고

모든 원본은 출처, 기준일, CRS, 수평 해상도, NoData, SHA-256을 가진다. DEM은 높이 단위와 수직 기준을 추가로 기록한다.

- `PhysicalElevation`: 경사, 수계, 건물·경관 배치 가능 여부에만 사용한다.
- `VisualElevation`: Renderer의 높이 과장과 기준 offset에만 사용한다.

현재 기본 표고는 Copernicus GLO-30 30m이고 VWorld·국토지리정보원 90m DEM은 국내 공식 비교 자료다. 토지피복 위치는 ESA WorldCover 2021 10m를 사용한다.

현재 오프라인 실행기는 대관령 중앙 L2 `kr5186:l2:700:1145`와 Halo 60m에 대해 Copernicus DEM과 WorldCover를 함께 절단한다. `height-f32-v1`은 63×63 물리 표고, `landcover-u8-v1`과 `placement-mask-u8-v1`은 각각 62×62 의미·배치 bit 표본이며 manifest에 원본·산출물 SHA-256, CRS, 해상도, NoData와 아직 확인되지 않은 DEM 수직 기준 `Unverified`를 기록한다. Unity는 Halo를 제외한 중앙 500m의 51×51 표본만 표현용 Mesh로 만들고, 높이 과장은 Renderer에만 적용한다. 이 Mesh는 현재 Collider나 배치 판정 권위를 갖지 않는다.

세 산출물은 로컬 `SimulationWorldDerived` DB의 한국어 열에 계보·형식·표본 크기와 함께 저장되고 `world-stream` Manifest에서 `Available`로 투영된다. 바이너리 본문 API는 객체 키가 설정한 산출물 루트를 벗어나지 않는지 확인하고 DB의 길이·SHA-256과 일치할 때만 응답한다. 자료가 없는 이웃 타일은 계속 `WaitingForSpatialArtifact`다.

2026-08-13에는 VWorld 90m DEM ZIP, VWorld 법정동 경계 ZIP, ESA WorldCover 평창군 TIFF, Copernicus DEM 평창군 TIFF, 환경부 토지피복 통계 CSV, 평창군 타일 Manifest JSON의 원본 6종을 공유 공공데이터 DB의 raw snapshot으로 등록했다. 환경부 CSV에서 평창군 7개 연도·294개 면적 값을 `km2`, 기준연도, 지역 고유 식별자와 `AreaStatisticWithoutGeometry` 제한으로 정규화했다. 같은 파일을 다시 등록했을 때 새 snapshot과 수치 행을 만들지 않는 멱등성도 확인했다. 원본 파일은 `artifacts/local/public-spatial/`의 비공개·Git 제외 경로에 유지한다.

## 통계 배분과 의미 신뢰 수준

환경부 2024 평창군 합계는 `1,464.2839㎢`다. 하천 `8.7735㎢`와 호소 `0.4307㎢`의 수계 합은 `9.2042㎢`이며 `23.6943㎢`는 기타 나지다.

의미 신뢰 수준은 `Observed`, `Derived`, `StatisticallyAllocated`, `Scenario`, `Decorative`로 구분한다. WorldCover 후보 마스크에 환경부 총량을 나눈 논·밭·시설재배지·과수원과 산림 수종은 세분류 SHP가 확보되기 전까지 `StatisticallyAllocated`다. 감자밭은 대관령 Farm의 `Scenario`다.

후보 면적이 목표보다 적으면 새 공간을 꾸며내지 않고 `UnresolvedTargetArea`로 남긴다. 면적 배분은 실제 면적 산출물이고, Synty 개체 수·군집은 별도 `LandscapeCompositionPlan`에서 `sqrt(면적 비율)`, 희소 유형 최소 노출과 단일 유형 40% 상한을 적용한다.

## 중간 검증 관문

1. 원자료 metadata와 hash
2. `PhysicalElevation`/`VisualElevation` 분리
3. 의미 신뢰 수준
4. Halo와 세계 좌표 seed
5. 면적 배분/경관 계획 분리
6. 시각 자산의 배치 능력
7. 대관령면 1km 경관 완결 영역 안의 L2 500m 수작업 Reference Tile 비교
8. Triangle·Material Slot·Draw Call·Shadow Caster·Collider·Animator 성능 예산

위 8단계는 자산 연결 전에 수행하는 중간 검증이다. `final-visual-asset-binding`은 그 뒤에 오는 파이프라인의 마지막 단계다. 서버와 공간 DB는 의미 기반 `VisualKey`까지만 결정하고, Unity가 현재 선택된 시각 자산 대장에서 토지피복·영역 역할·원본 경사·LOD·성능 조건을 통과한 항목만 실제 Prefab으로 해석한다. 현재 대장은 보유 Synty 팩을 사용하지만, 원본 Prefab 이름이나 경로는 공간·건물·Simulation 고유 식별자에 들어가지 않는다. 하나라도 연결이 거부되면 불완전 Unity 산출물을 저장하지 않고 거부 건수와 원인을 배치 검증 기록으로 남긴다.

시각 자산 대장은 허용 토지피복·역할·경사, footprint·여백, collision 정책, LOD, 군집·회전 가능 여부와 예상 렌더링 비용을 가진다. Overview와 Region 단계는 Cluster/HLOD 대상이다.

### JSON 경관 배치 계획과 Scene 적용 관문

정적 경관은 생성기가 Scene에 즉시 쓰지 않고, 사람이 읽는 Markdown 기획서와 기계가 검증하는 JSON 계획을 중간 산출물로 거친다. 첫 평창 계획은 현재 Fixture에서 생성하지만 이후 서버도 같은 JSON schema를 내려줄 수 있다. 계약에는 `VisualKey`·`CompositionKey`, 세계 좌표, 회전·크기, 토지피복·영역 역할·경사·수계·계절·분위기 조건, 표현 전용 근거, 성능 예산과 대상 컨테이너만 저장한다. Synty Prefab 경로·GUID와 원본 이름은 저장하지 않는다.

```text
사람이 읽는 경관 배치 기획서.md
├─ 영역별 주역·필수·금지 요소와 자료 한계
└─ 기획서 고유 식별자·개정·내용 hash

기본 배치 계획 JSON
├─ 서버·Fixture가 결정한 137개 의미 배치
└─ 계획 hash와 항목별 hash

사람 보정 JSON
├─ Add / Modify / Disable
└─ 예상 기본 계획 hash + 예상 항목 hash

병합·검증
├─ 대장 revision·의미 key·토지피복·역할·경사·수계 조건
├─ 컨테이너 범위·겹침·LOD별 렌더링 예산
└─ 오류·경고·hash가 담긴 배치 검증 기록

Unity 준비 산출물
├─ 8개 말단 배치 대상별 Staging Prefab
└─ 배치 영수증 + VisualRoot/Composition 해석 결과

사람 검토 승인 기록
└─ 기획서·기본·보정·병합 계획 hash 봉인

명시적 Scene 적용
└─ 승인 hash와 Anchor를 다시 확인한 뒤 StaticSceneryGeneratedRoot만 교체
```

현재 8개 말단 배치 대상은 대관령 Farm 4개 L2 타일, Farm–Hub 회랑, 진부 Hub, Hub–Town 회랑, 평창 Town이다. 상위 `L4_L7_Synty경관_PresentationOnly`은 이들을 묶는 Scene 계층 부모이며 자체 배치·Staging을 갖지 않는다. 전용 Unity 검토창은 기본 계획을 읽기 전용으로 표시하고 이동·회전·크기·의미 키 변경을 보정 JSON의 `Add/Modify/Disable`로만 저장한다. 2D 배치도와 대상 목록, 배치 속성, 검증 문제와 성능 예산을 함께 보여주며 활성 Scene을 수정하지 않는다.

`WORLD-PLAN-2`는 검토 승인 전에도 Staging을 허용한다. 검토 완료 또는 Scene 적용 승인은 기획서·기본 계획·보정 계획·병합 계획의 SHA-256을 별도 승인 기록에 봉인한다. 이후 어느 입력이라도 바뀌면 상태를 `Stale`로 계산하고 `WORLD-PLAN-3`을 차단한다. Scene 적용은 오류 0건, `ApprovedForSceneApply`, 네 hash 일치를 모두 만족할 때만 수행하며 저장 실패 시 기존 정적 경관 Root를 보존한다. 계절 사건 Overlay, 플레이어·NPC·차량, 카메라와 UI는 정적 계획에서 제외하고 각자의 Simulation 상태 사본과 런타임 표현 Pipeline을 유지한다.

## 문서 중심 AreaSet과 여러 LandscapeGraph

`AreaSet`은 하나의 거대한 경관 Graph가 아니라 지역 세계의 의미와 시나리오 범위를 설명하고 여러 독립 Graph를 묶는 상위 컨테이너다. `LandscapeGraph`는 실제 공간 조립·검증·부분 재생성·의미 기반 스트리밍 단위이고, `Tile`은 공간 Layer 산출물과 캐시 단위다. `Area`는 법정동 또는 Farm·Hub·Town 같은 의미 범위이므로 `Area = LandscapeGraph`를 1:1로 고정하지 않는다. 한 Graph는 여러 Area·Tile을 참조할 수 있고, 큰 Area도 여러 Graph로 나눌 수 있다.

```text
World
└─ AreaSet : 지역 세계 정의서
   ├─ authored/area-set.md       사람의 의도·근거·미해결 설명
   ├─ area-set.json              실행 권위·고유 식별자·참조·관계
   ├─ generated/status.md        DB 실행 상태의 자동 산출물
   │
   ├─ LandscapeGraph : 대관령 Farm
   │  └─ AreaRefs[] + TileRefs[] + Node/Edge/Placement
   ├─ LandscapeGraph : Farm–Hub 회랑
   ├─ LandscapeGraph : 진부 Hub
   ├─ LandscapeGraph : Hub–Town 회랑
   └─ LandscapeGraph : 평창 Town

      GraphRelations[]
      └─ ExternalConnectorStub ↔ ConnectorPair ↔ ExternalConnectorStub
```

JSON만 실행 권위를 가진다. Markdown의 `@areaset`, `@area`, `@landscape-graph` 참조는 compiler가 JSON과 정확히 일치하는지 검증하고, 사람이 작성한 문서 SHA-256과 실행 정의 SHA-256을 따로 기록한다. `generated/area-set-status.md`는 파생 DB의 최신 실행 상태로 다시 만들며 사람이 직접 수정하지 않는다. Unity나 Simulation이 Markdown을 제각각 해석하지 않고 compiler가 만든 단일 `AreaSetDefinition`만 소비한다.

### E 증거 단계와 H 공간 포함 계층

`E`는 검증 깊이이고 `H`는 공간 구조의 포함 깊이다. 두 축을 같은 단계 번호처럼 사용하지 않는다.

```text
H4 AreaSet
└─ H3 LandscapeGraph
   └─ H2 LandscapeBlock
      └─ H1 WI 공간 모판 인스턴스
```

- `E4`는 H1 모판에서 포함된 E3 WI를 다시 실행한 증거다.
- `E5`는 H1을 실제 H2에 배치하고 H2→H3→H4 이동 경로를 닫은 증거다.
- `E6`는 E5 경관에 공공데이터 계보를 연결하고 `E7`은 플레이어 이용 폐루프를 검증한다.

H 코드는 리소스 종류를 분류할 뿐 완료 상태를 올리지 않는다. 현재 H3·H4 정의가 존재해도 실제 H2 Block과 연결 폐루프가 없으면 E5가 아니다. 기존 156개 기준 경관 문법 모판은 H 계층이 아니라 H1의 허용 후보와 H2·H3 조립에 쓰는 공간 문법 어휘다. Tile L0~L2, Area, 경관 완결 영역, ScenarioRoute도 각각 기술 해상도·의미 범위·검토 범위·이동 의미 참조이므로 H 계층에 넣지 않는다.

Graph 내부 Node는 다른 Graph의 Node를 직접 참조하지 않는다. Graph 사이 연결은 AreaSet의 `GraphRelation`과 양쪽 `ExternalConnectorStub`의 식별자·종류·방향·폭·좌표·Route 서명을 비교해 검증한다. 양쪽 Graph가 조립 가능한 상태인데 연결이 맞지 않으면 임의 연결을 만들지 않고 두 Graph를 `PartialUnresolved`로 남긴다. 한 Graph를 다시 만들 때 이웃 Graph 전체를 무효화하지 않는 것이 이 경계의 목적이다.

서버의 `Declared / Available / PartialUnresolved`는 공간자료와 조립 결과의 상태다. Unity의 플레이어별 `Unloaded / Declared / Prepared / Active / Cached`는 같은 Graph를 언제 메모리에 보관하고 표시할지 나타내는 로컬 스트리밍 상태이며 서버 상태를 변경하지 않는다. Unity는 Graph 하나의 모든 타일 조각을 비활성 staging root에 조립·검증한 후 Graph root 단위로 교체한다.

```text
GET /api/simulation/v1/world-stream/area-sets/{areaSetStableId}
GET /api/simulation/v1/world-stream/area-sets/{areaSetStableId}/landscape-graphs
GET /api/simulation/v1/world-stream/landscape-graphs/{landscapeGraphStableId}
GET /api/simulation/v1/world-stream/tiles/{tileKey}/landscape-composition
```

마지막 Tile API는 기존 Unity 소비자를 위한 호환 조회다. 한 Recipe 개정 동안 Graph에서 해당 Tile 소유 Node·Edge·Placement만 투영하며, 새 Graph 계약을 다시 Tile 권위로 축소하지 않는다.

## 모판을 연속 공간으로 만드는 공간 문법

`CompositionKey`는 완성 Scene이 아니라 경관을 만드는 어휘다. 서버는 Prefab 좌표를 무작정 나열하지 않고 `공간 골격 → 영역 채우기 → 연결망 생성 → 경계 봉합 → 반복 변형`을 거쳐 Macro·Meso `LandscapeGraph`를 만든다. Unity wrapper는 이 Graph와 세계 좌표 기반 seed를 받아 Micro 장식, LOD·HLOD와 Renderer 세부 구성을 맡는다.

```text
공간 원본·Area·ScenarioRoute
└─ LandscapeSkeleton
   ├─ 면형 영역: 숲·밭·초지·주택지
   ├─ 선형 연결망: 농로·타운도로·도시도로
   └─ 의미 경계: Nature–Farm·Farm–Town·Farm–Hub 등
      ↓
   LandscapeGraph
   ├─ Node: Macro·Meso 공간 의미와 근거 수준
   ├─ Edge: 포함·인접·연결·전환 관계
   ├─ Placement: CompositionKey·세계 좌표·회전·seed
   ├─ ExternalConnectorStub: 인접 타일 인계
   └─ Unresolved: 자료·연결·호환 부족
      ↓
   Unity LandscapeCompositionRoot
   ├─ 면형모판
   ├─ 선형모판
   ├─ 결절모판
   ├─ 경계봉합모판
   ├─ 거점모판
   └─ 세부모판
```

경관 문법의 공개 대장은 아래 52개 의미 모판군 × A/B/C 세 변형, 정확히 156개다. 원본 팩별 기술 대장과 기존 도로·Gate 대장은 제작 근거로 더 많은 항목을 가질 수 있지만 서버와 런타임이 소비하는 canonical 대장은 이 156개뿐이다.

| 계열 | 의미 모판군 | A/B/C 포함 항목 수 |
| --- | ---: | ---: |
| Nature | 12 | 36 |
| Farm | 8 | 24 |
| Town | 6 | 18 |
| City·Hub | 6 | 18 |
| Network | 농촌·타운·도시의 직선·곡선·T·십자 12 | 36 |
| Transition | Nature–Farm, Farm–Town, Town–City, Farm–Hub, Town–Hub, Hub–City, Water–Land, Road–BuildingFront 8 | 24 |
| 합계 | 52 | 156 |

각 항목은 `Area / Linear / Junction / Transition / Landmark / Detail` 위상, 사방 `EdgeProfile`, 위치·방향·폭을 가진 `Connector`, 연속 반복 상한과 최근 변형 감점, 허용·선호·금지 이웃, 타일·연쇄·종료 가능 여부, 내부 detail 생성기 개정과 세계 좌표 seed 규약을 가진다. `EdgeProfile`은 맞닿는 면의 성격이고 `Connector`는 길·수로·보행처럼 실제로 이어지는 지점이므로 합치지 않는다.

Unity는 의미 대장에서 유료 Prefab을 해석하지만 서버로 내보내는 안전 Manifest에는 Prefab 경로·원본 이름·GUID를 넣지 않는다. `CatalogRevision`과 안전 필드 SHA-256이 서버 응답과 로컬 대장에서 모두 일치해야 조립한다. 새 경관은 비활성 staging root에서 전부 검증한 후 `LandscapeCompositionRoot`와 원자적으로 교체한다. hash나 Node·Edge·Placement 참조가 틀리면 기존 root를 유지한다.

파생 DB는 공간 원본과 Synty 해석 영수증 사이에 다음 중립 테이블을 둔다.

```text
시뮬레이션월드_경관조립실행
├─ 시뮬레이션월드_경관공간Node
├─ 시뮬레이션월드_경관공간Edge
├─ 시뮬레이션월드_경관모판배치
└─ 시뮬레이션월드_경관조립미해결
```

이 테이블은 `CompositionKey`까지만 저장하고 Synty 상품 정보는 저장하지 않는다. 실제 도로 자료가 없는 첫 농로 연결은 `Scenario` 근거와 외부 연결 Stub으로 남기며 관측 도로로 승격하지 않는다.

## 첫 세로 단위

`area-set:sim:pyeongchang:farm-hub-town.v1`은 대관령면 Farm, 진부면 Hub, 평창읍 Town과 두 `ScenarioRoute`를 참조한다. 공식 도로 공간자료가 연결되기 전까지 회랑을 실제 도로로 주장하지 않는다. Unity는 `SimulationWorldShell` 한 Scene에서 카메라 거리에 따라 L0/L1/L2 표현만 전환하며 서버나 Simulation 상태를 변경하지 않는다.

첫 완결 단위는 `completion-area:sim:pyeongchang:daegwallyeong-farm.v1`이다. EPSG:5186 범위는 `(350000, 572000)–(351000, 573000)`이고 다음 네 L2 타일을 가진다.

```text
kr5186:l2:700:1145 | kr5186:l2:701:1145
kr5186:l2:700:1144 | kr5186:l2:701:1144
```

서버 파생 Pipeline은 전체 Manifest에 네 타일이 모두 있으면 이 네 L2 타일과 상위 `kr5186:l1:175:286`, `kr5186:l0:43:71`만 선택한다. `LandscapeCompletionArea`, Farm과 네 `SpatialTile`의 원본 공간 관계는 SchemaVersion 2의 기존 node·relation 구조에 유지한다. 그 위에서 반복 생성·외부 연결·부분 미해결을 조회하기 위한 경관 Graph만 위의 다섯 중립 테이블에 별도 저장한다. Unity `WorldBuildManifest`는 같은 네 타일에 대해 `elevation`, `land-cover`, `placement-mask` 계약 12개와 결정적 완결 영역 hash를 만든다.

완결 여부는 `원자료 → 물리 공간 → 공간 의미 → Scenario 규칙 → 경관 계획 → UI 계획 → Unity Runtime → 최종 검증` 여덟 수직 관문으로 기록한다. 중앙 타일 `kr5186:l2:700:1145`은 DEM·WorldCover·배치 마스크가 준비됐고 나머지 세 타일은 아직 `WaitingForSpatialArtifact`다. 경관 문법 계약·DB·API·Unity 원자적 조립 코드는 준비됐지만 네 타일 실제 조립과 최종 Game View는 아직 완료 증거가 아니다.

각 Area는 타일 레이어 결과뿐 아니라 공유 공공데이터 DB의 건축물대장·GIS 건물도형과 공개 지방행정 인허가 사업장 관점별 조회 결과를 읽는다. 관측 건물은 도형 또는 대표점에 배치하고, 도형이 없는 건물은 임의 좌표에 놓지 않는다. 자료 부족을 보완하는 대표 건물은 `AreaComposition`, Farm·Hub·Town 역할 건물은 `Scenario` 근거로 분리한다. 공개 사업장명과 업종은 간판·상점 계열 시각 후보의 근거가 될 수 있지만 실제 입주 확정이나 운영 업무 완료를 뜻하지 않는다.

## 1인칭 런타임 타일 스트리밍

오프라인 공간 Pipeline의 L2 500m 타일은 Unity 런타임에서도 같은 `kr5186:l2:{x}:{y}` 식별자를 사용한다. 첫 Recipe의 스트리밍 창은 공식 엔진 자료와 현재 수직 단위의 자료 상태를 바탕으로 `상세 3×3 / 활성 5×5 / 준비 9×9`로 둔다. 대관령 Farm Fixture가 선언하는 전체 검증 범위는 경계 선행 이동 한 칸을 포함하는 11×11이며, 한 시점에 121개 Manifest를 모두 요청하지 않는다.

```text
플레이어 위치
├─ 3×3 상세 창   : 시야 안 건물·Synty·업무 객체 승격 후보
├─ 5×5 활성 창   : 충돌·상호작용에 사용할 수 있는 검증 완료 표현
└─ 9×9 준비 창   : 다음 이동을 위한 Manifest·산출물 사전 준비
   └─ 창 밖 Slot : 표현 Root를 풀로 반환해 재사용
```

### 스트리밍 범위 조사와 초기 예산

공식 문서에는 모든 프로젝트에 맞는 고정 타일 개수가 없다. Unreal World Partition은 플레이어나 별도 Streaming Source의 위치, Grid Cell Size와 Loading Range로 로드 범위를 정하고 Loaded와 Activated 상태 및 HLOD를 분리한다. Unity Addressables는 비동기 로드와 참조 횟수에 맞춘 해제를 요구하고, Terrain 인접 연결은 이웃 타일의 LOD 경계를 맞춘다. Cesium 3D Tiles는 시야·화면 오차를 기준으로 우선순위를 정하며 선행 형제 타일이 이동을 부드럽게 하지만 더 많은 메모리를 사용하고, 동시 타일 로드 수를 별도 제한한다.

- [Unity Addressables 에셋 로드](https://docs.unity3d.com/kr/Packages/com.unity.addressables%401.21/manual/load-assets.html)
- [Unity Addressables 메모리 관리](https://docs.unity3d.com/kr/Packages/com.unity.addressables%401.21/manual/MemoryManagement.html)
- [Unity Terrain.SetNeighbors](https://docs.unity3d.com/2023.2/Documentation/ScriptReference/Terrain.SetNeighbors.html)
- [Unreal Engine World Partition](https://dev.epicgames.com/documentation/unreal-engine/world-partition-in-unreal-engine)
- [Cesium 3D Tiles 선택·동시 로드](https://cesium.com/learn/cesium-native/ref-doc/selection-algorithm-details.html)
- [Cesium for Unity 선행 타일 설정](https://cesium.com/learn/cesium-unity/ref-doc/classCesiumForUnity_1_1Cesium3DTileset.html)

500m L2에서 10×10 활성은 25㎢, 25×25 준비는 156.25㎢이므로 현재 실제 지형·건물 상세 예산으로는 과하다. 반대로 기존 3×3 활성·5×5 준비는 보행보다 빠른 차량과 경계 선행 준비를 확장하기에 좁다. 초기 균형값은 다음과 같다.

| 상태 | 범위 | 한 변 | 면적 | 실제 의미 |
| --- | ---: | ---: | ---: | --- |
| 상세 | 3×3, 9개 | 1.5km | 2.25㎢ | 카메라 시야에 따른 객체 상세 승격 후보 |
| 활성 | 5×5, 25개 | 2.5km | 6.25㎢ | 지형·충돌·상호작용 상주 범위 |
| 준비 | 9×9, 81개 | 4.5km | 20.25㎢ | Manifest와 다음 산출물 준비 범위 |
| Fixture 제공 범위 | 11×11, 121개 | 5.5km | 30.25㎢ | 한 칸 앞당긴 9×9 창의 검증 범위이며 동시 로드 범위가 아님 |

동시 타일 로드는 PC 첫 예산 4개로 제한한다. 플레이어가 현재 타일 경계까지 타일 폭의 25%, 즉 실제 125m 안으로 접근하고 이동 방향도 그 경계를 향하면 준비 창 중심을 한 타일 앞당긴다. 경계를 넘으면 기존 81개를 다시 받지 않고 새 가장자리 9개만 요청하며 나머지 Slot은 재사용한다. 활성 객체 Projection은 앞당긴 중심의 5×5까지만 준비하고 실제 Prefab 상세화는 계속 카메라 절두체와 거리 예산이 결정한다.

이 값은 영구 상수가 아니라 `RecipeRevision`에 봉인된 PC 초기 Profile이다. 실제 Terrain·Addressables 산출물이 연결되면 프레임 시간, 최고 메모리, 타일 요청 p95, 경계 대기 시간과 캐시 적중률을 측정해 Mobile·PC·고속 차량 Profile을 분리한다.

시뮬레이션 서버의 읽기 경계는 다음과 같다.

```text
GET /api/simulation/v1/world-stream/recipes/{recipeId}
GET /api/simulation/v1/world-stream/regions/{regionStableId}
GET /api/simulation/v1/world-stream/tiles/{tileKey}/manifest
GET /api/simulation/v1/world-stream/tiles/{tileKey}/artifacts/{layerCode}
GET /api/simulation/v1/world-stream/tiles/{tileKey}/activities
GET /api/simulation/v1/world-stream/tiles/{tileKey}/objects
GET /api/simulation/v1/world-stream/tiles/{tileKey}/landscape-compositions
```

Recipe와 Manifest는 좌표계, 타일 크기, 활성·준비 반경, Halo, Layer 상태와 결정적 SHA-256을 제공한다. 활동 관점별 조회 결과는 표현용 읽기 사본이며 Unity 이동으로 `WorldTick`이나 Session 개정을 진행하지 않는다.

지역 조회는 타일 요청보다 앞에서 공유 공공데이터 DB의 법정동·행정동 Assignment, 관할 교차 관계와 행정동별 건물 Category 집계를 파생 DB에 고정한 결과를 읽는다. 경계 geometry가 없으면 `WaitingForRegionGeometry`로 남기며, 지역을 임의 타일이나 Farm·Hub·Town 역할에 끼워 맞추지 않는다. 후속 타일 조립기는 `IntersectsSpatialTile` 관계가 검증된 뒤에만 이 지역 Projection을 타일별 건물 표현 후보와 결합한다.

현재 첫 수직 단위는 타일 생명주기, 중앙 타일의 실제 DEM·토지피복·배치 마스크 산출물, 경관 Graph 계약·저장·조회와 Unity 조립 경계까지 구현됐다. 로컬 MySQL Job에서 중앙 타일은 Node 5·Edge 3·Composition 배치 5·인접 타일 연결 Stub 1개로 저장됐고 같은 입력 재실행의 Graph SHA-256이 일치했다. 나머지 세 L2 타일은 필수 Layer가 없어 `WaitingForSpatialArtifact`로 저장하며 가짜 경관을 만들지 않는다. Unity Fixture도 자료 대기 응답만 제공한다. 따라서 중앙 타일 산출물과 코드·시험 검증을 네 타일 실제 경관 또는 Game View 완료 증거로 사용하지 않는다.

### 이동하면서 새 공간을 마주하는 런타임 구조

수평 타일 창과 타일 하나의 수직 생성 절차를 분리한다. 수평 창은 안전하게 이동할 범위와 미리 준비할 범위를 정하고, 카메라 시야는 그 안에서 렌더링 우선순위만 정한다.

```text
사용자 이동·카메라 시야
├─ 수평 범위 관리
│  ├─ 현재 L2 타일 산정
│  ├─ 3×3 상세 창
│  ├─ 5×5 활성 창
│  ├─ 9×9 준비 창
│  └─ 창 밖 TileRoot·VisualRoot 풀 반환
│
├─ 타일별 수직 자료 처리
│  ├─ Recipe·Manifest 조회
│  ├─ 원본·Layer hash와 자료 상태 검증
│  ├─ DEM·토지피복·배치 마스크 조립 또는 자료 대기
│  ├─ 안전 지면·Collider 판정
│  └─ 타일별 건물 관점별 조회 결과 수신
│
└─ 시야 기반 건물 표현
   ├─ 카메라 절두체 안           실제 시야
   ├─ 화면 여백·이동 방향 안     예측 시야
   ├─ Declared → ProxyActive     저비용 실루엣
   ├─ ProxyActive → DetailActive VisualKey 기반 Synty 상세
   └─ 시야 밖 유예 → HiddenCached 재사용 대기
```

첫 건물 자료는 대관령 Farm 시각 검증을 위한 결정적 `Scenario` 5개다. Barn·Silo·Farmhouse·Greenhouse·ProduceStand의 타일, 로컬 오프셋, 회전, footprint와 `VisualKey`를 제공하지만 실제 관측 건물이나 운영 시설로 노출하지 않는다. 프록시와 상세 Prefab의 Collider는 끄고 같은 배치 객체를 중복 생성하지 않는다.

이동 Gate는 다음 위치의 타일이 준비 창에 있고 지면 충돌이 확인됐는지를 먼저 본다. 서버 자료 모드에서는 안전 기반 Layer도 준비돼야 하며, 그렇지 않으면 경계에서 기다린다. Fixture에서 기존 Scenario 지면 Collider를 허용하는 것은 Play Mode 검증용 예외다. 런타임 진단 트리는 활성·준비 타일, 자료 대기, 실제·예측 시야, 프록시·상세·캐시 수와 `WorldTick`을 함께 보여준다.

실제 산출물이 연결되면 `다운로드 → SHA-256 검증 → Halo 포함 조립 → 활성 전환`을 같은 Slot 상태 전이에 추가한다. 원점 이동, Addressables/HLOD와 다중 사용자 위치 동기화는 9×9 준비 창·5×5 활성 창의 공간 정확도와 성능을 검증한 뒤 별도 단계로 확장한다.
