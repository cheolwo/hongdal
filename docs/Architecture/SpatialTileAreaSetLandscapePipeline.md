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

현재 오프라인 실행기가 실제로 절단·집계하는 공간 원본은 WorldCover다. DEM의 출처·CRS·NoData·높이 metadata와 계약은 연결했지만, Unity 연속 Mesh는 아직 기존 `ScenarioTerrainPreview`이다. DEM 표본·경사·수계·공유 경계 정점을 산출하는 단계가 연결되기 전에는 Scene Mesh를 `PhysicalElevation` 결과로 보고하지 않는다.

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

## 첫 세로 단위

`area-set:sim:pyeongchang:farm-hub-town.v1`은 대관령면 Farm, 진부면 Hub, 평창읍 Town과 두 `ScenarioRoute`를 참조한다. 공식 도로 공간자료가 연결되기 전까지 회랑을 실제 도로로 주장하지 않는다. Unity는 `SimulationWorldShell` 한 Scene에서 카메라 거리에 따라 L0/L1/L2 표현만 전환하며 서버나 Simulation 상태를 변경하지 않는다.

첫 완결 단위는 `completion-area:sim:pyeongchang:daegwallyeong-farm.v1`이다. EPSG:5186 범위는 `(350000, 572000)–(351000, 573000)`이고 다음 네 L2 타일을 가진다.

```text
kr5186:l2:700:1145 | kr5186:l2:701:1145
kr5186:l2:700:1144 | kr5186:l2:701:1144
```

서버 파생 Pipeline은 전체 Manifest에 네 타일이 모두 있으면 이 네 L2 타일과 상위 `kr5186:l1:175:286`, `kr5186:l0:43:71`만 선택한다. 파생 DB에는 `LandscapeCompletionArea` node, Farm 포함 관계, 네 `SpatialTile` node와 포함 관계를 SchemaVersion 2의 기존 node·relation 구조로 저장하므로 새 물리 표는 필요하지 않다. Unity `WorldBuildManifest`는 같은 네 타일에 대해 `elevation`, `land-cover`, `placement-mask` 계약 12개와 결정적 완결 영역 hash를 만든다.

완결 여부는 `원자료 → 물리 공간 → 공간 의미 → Scenario 규칙 → 경관 계획 → UI 계획 → Unity Runtime → 최종 검증` 여덟 수직 관문으로 기록한다. 현재 원자료·Scenario·경관 계획 계약은 준비됐지만 실제 DEM 지형·배치 마스크는 `WaitingForSpatialArtifact`, UI·Unity Runtime·최종 화면 검증은 `RequiresEditorEvidence`다. 따라서 계약·정적 컴파일 통과를 완성된 Game View로 표현하지 않는다.

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
```

Recipe와 Manifest는 좌표계, 타일 크기, 활성·준비 반경, Halo, Layer 상태와 결정적 SHA-256을 제공한다. 활동 관점별 조회 결과는 표현용 읽기 사본이며 Unity 이동으로 `WorldTick`이나 Session 개정을 진행하지 않는다.

지역 조회는 타일 요청보다 앞에서 공유 공공데이터 DB의 법정동·행정동 Assignment, 관할 교차 관계와 행정동별 건물 Category 집계를 파생 DB에 고정한 결과를 읽는다. 경계 geometry가 없으면 `WaitingForRegionGeometry`로 남기며, 지역을 임의 타일이나 Farm·Hub·Town 역할에 끼워 맞추지 않는다. 후속 타일 조립기는 `IntersectsSpatialTile` 관계가 검증된 뒤에만 이 지역 Projection을 타일별 건물 표현 후보와 결합한다.

현재 첫 수직 단위는 타일 생명주기와 서버 계약까지 구현됐다. 실제 DEM·토지피복·배치 마스크 런타임 산출물은 아직 없으므로 세 Layer 모두 `WaitingForSpatialArtifact`다. Unity Fixture는 한 시점의 81개 준비 타일 경계와 상태판만 만들고 Terrain Mesh·Collider·가짜 높이를 만들지 않는다. 따라서 기존 `ScenarioTerrainPreview` 위에서 동적 경계를 확인할 수 있지만 이를 실제 DEM 기반 지형의 완료 증거로 사용하지 않는다.

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
