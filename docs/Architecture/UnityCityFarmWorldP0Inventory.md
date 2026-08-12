# Unity City·Farm World P0 기준선과 Asset Inventory

## 1. 범위와 확인 시각

- 기준일: 2026-08-09
- Unity project: `C:\Users\user\ssalddel`
- Unity: 6000.5.6f1
- Render Pipeline: URP 17.5.0
- Pipeline: `com.unity.pipeline` 0.4.0-exp.1
- 확인 방법: repository 파일 조사와 연결된 Unity Editor/Pipeline의 read-only 상태 조회

이 문서는 [Unity World 구현 현황과 우선순위](UnityWorldImplementationPriority.md)의 P0 결과다. Vendor asset, 기존 Scene과 사용자의 dirty working tree는 수정하지 않았다.

## 2. 현재 Editor와 Scene 기준선

| 항목 | 확인 결과 |
| --- | --- |
| Editor 상태 | ready, compile·domain reload 없음, Play Mode stopped |
| 활성 Scene | `Assets/Ssalddel/Experiments - 연구/SyntyCityPackIntegration/도심물류센터도시팩적용연구.unity` |
| Scene dirty | false |
| Console error | 0 |
| 현재 Camera | `Main Camera`의 고정 Transform, 전용 camera rig 없음 |
| 현재 업무 배선 | 물류센터 Controller·LifetimeScope·NPC waypoint·Truck·CargoVisualRoot·시설 4영역 VisualRoot |

현재 `SyntyPlazaOrbitCamera`는 Community Plaza 실험용 자유 orbit camera다. WORLD-0의 90도 단계 회전·World/Zone/Object Focus 규칙과 다르므로 확장하지 않는다. `WorldBootstrapSceneBuilder`의 고정 Camera도 공개 World Map bootstrap용이므로 그대로 유지한다.

## 3. 재사용할 저장 Scene과 코드

확인된 제품·실험 Scene:

- `Assets/Ssalddel/Scenes/WorldBootstrapScene.unity`
- `Assets/Ssalddel/Scenes/UrbanLogisticsCenterPrimitive.unity`
- `Assets/Ssalddel/Scenes/UrbanMarketManagerPrimitive.unity`
- `Assets/Ssalddel/Experiments - 연구/SyntyCityPackIntegration/도심물류센터도시팩적용연구.unity`
- `Assets/Ssalddel/Experiments - 연구/SyntyCityPackIntegration/도심마트도시팩적용연구.unity`

재사용 대상:

- Farm 6×6 `FarmSoilTileCellView`·`FarmSoilTileGridView`·Projector
- `TransportCorridorTruckView`와 `CargoVisualRoot`
- `LogisticsFacilityOverviewView`의 건물·차량 접근·입고 Dock·검수·보관 VisualRoot
- 도심마트 manager surface·Concept Card·대표 NPC
- Residential pickup SceneController·View·LifetimeScope
- City Pack builder가 이미 선택한 Shop 05, Apartment 01, Station 03, Desk 01, Shelf 01, Van 01, Pallet 01, Cardboard Box 01

신규 구현 후보는 통합 Diorama camera/focus, Macro World Zone anchor, 실제 inventory 기반 Presentation catalog와 cross-zone cargo lineage presentation으로 제한한다.

## 4. Synty inventory

| Pack | Prefab | Material | Demo/Overview 처리 |
| --- | ---: | ---: | --- |
| POLYGON Farm | 498 | 24 | asset·lighting 분석에만 사용하고 제품 Scene으로 복사하지 않음 |
| POLYGON City | 335 | 25 | asset·lighting 분석에만 사용하고 제품 Scene으로 복사하지 않음 |
| POLYGON Town | 702 | 25 | asset·lighting 분석과 Composition 후보 조사에만 사용하고 제품 Scene으로 복사하지 않음 |

WORLD-0~WORLD-2 첫 allowlist 후보:

### Farm

- Dirt: `SM_Env_Dirt_01`
- Dirt Row: `SM_Env_Dirt_Rows_01`
- Potato: `SM_Prop_Plant_Potato_01_S/M/L`
- Potato cargo: `SM_Prop_Box_Potato_01`
- Farmer: `SM_Chr_Farmer_Male_01`
- Barn: `SM_Bld_Barn_01`
- Silo: `SM_Bld_Silo_01`
- Produce Stand: `SM_Bld_ProduceStand_01`
- Tractor: `SM_Veh_Tractor_01`
- Rural road: `SM_Env_Road_Dirt_Straight_01`

### City·Logistics·Market·Residential

- Road: `SM_Env_Road_01`
- Logistics facility: 기존 builder의 `SM_Bld_Station_03`
- Vehicle: `SM_Veh_Car_Van_01`
- Pallet: `SM_Prop_Pallet_01`
- Cargo box: `SM_Prop_CardboardBox_01`
- Market: 기존 builder의 `SM_Bld_Shop_05`
- Shelf: `SM_Prop_ShopInterior_Shelf_01`
- Manager desk: `SM_Prop_ShopInterior_Desk_01`
- Residential: `SM_Bld_Apartment_01`

### Town·Low-density Residential·Transition

- House: `SM_Bld_House_Preset_01`
- Garage house: `SM_Bld_House_Preset_Garage_01`
- Shop: `SM_Bld_Shop_01`
- Road: `SM_Env_Road_01`
- Sidewalk: `SM_Env_Sidewalk_Straight_01`
- Driveway: `SM_Env_Driveway_01`
- Delivery vehicle: `SM_Veh_Truck_Delivery_01`
- Pickup: `SM_Veh_Pickup_01`
- Playground: `SM_Prop_Playground_01`
- Garden: `SM_Env_Garden_Straight_01`

이 파일명은 Presentation catalog 입력 후보일 뿐 Data·Simulation·Operational contract나 stable ID가 아니다.

Farm Pack 식품 prefab과 현재 HS·KAMIS 가격 연결표의 대응 여부는 [Unity POLYGON Farm 식품 Asset·HS·가격 연결 조사](UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)에 별도로 정리했다. asset 이름이 비슷하다는 이유만으로 상품 HS나 가격을 확정하지 않는다.

Town Pack 702개 prefab의 단독주택·도로·생활상권 Composition 후보는 [Unity POLYGON Town 반복 배치 Composition Set 조사](UnityPolygonTownCompositionSetResearch.md), Farm→Town→City 혼합 기준은 [Unity Farm·Town·City 혼합 Composition 조화 설계](UnityFarmTownCityCompositionHarmonyDesign.md)에 정리했다. 두 문서 모두 설계 상태이며 Town·혼합 prefab과 Scene은 아직 생성하지 않았다. 이미 구현한 기반과 이 미구현 범위를 합친 실제 착수 순서는 [Farm·Town·City Composition 통합 구현 순서](UnityCompositionSetIntegratedImplementationSequence.md)를 따른다.

### Animation·Avatar·FX 실측

`Assets/Synty` 전체를 파일과 FBX `.meta` 기준으로 추가 조사했다.

| 항목 | 확인 결과 |
| --- | --- |
| Synty `.anim`·`.controller`·`.overrideController` | 모두 0개 |
| Farm·Town·City·Generic·Starter character FBX | 모두 Humanoid `animationType: 3`, `clipAnimations: []`, `importAnimation: 0` |
| Town character prefab | 8개가 대응 asset을 찾지 못한 controller GUID를 참조 |
| 실제 ParticleSystem prefab | Farm 11개, City 2개, Generic 17개 |

따라서 현재 Pack은 사용할 Humanoid 외형·Avatar와 FX는 제공하지만 걷기·밭갈이·파종·수확·하역 clip을 제공한다고 확인할 수 없다. 구현에서는 실제 Synty 제공 source, 검증된 Humanoid 리타기팅, 절차형 차량·설비 동작과 fallback을 구분한다. 상세 source 정책과 `ANIM0~ANIM6` Gate는 [Synty Animation·FX 재사용과 리타기팅 설계](UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다.

### 반복 배치용 농장 풍경 Composition Library

실제 product Unity project `C:\Users\user\ssalddel`에는 단일 allowlist 위에 다음 재사용 계층을 구현했다.

| 항목 | 구현 상태 |
| --- | --- |
| 풍경 종류 | 감자밭 두렁, 혼합 작물밭, 헛간 작업마당, 농기계 대기장, 농산물 직판장, 수확물 집하장, 농로 교차로, 수목 완충지 8종 |
| 변형 | 각 A/B/C, 총 24개 prefab |
| source | POLYGON Farm 실제 nested prefab 83종 |
| catalog | `농장풍경CompositionCatalog.asset` |
| 생성기 | `농장풍경CompositionSetBuilder` |
| 상태 경계 | 실제감자밭·농부·차량·농기계·화물·상호작용 socket만 제공하고 상태나 stable ID는 소유하지 않음 |
| preview | `농장풍경조합모음미리보기.unity`, 24개 prefab과 Perspective camera 저장 |

생성 경로:

- source: `Assets/Ssalddel/Presentation/World/농장풍경Composition*.cs`
- builder: `Assets/Ssalddel/Editor/농장풍경CompositionSetBuilder.cs`
- prefab: `Assets/Ssalddel/Experiments - 연구/CityFarmWorld/CompositionSets/Farm/`
- catalog: `Assets/Ssalddel/Experiments - 연구/CityFarmWorld/Catalogs/농장풍경CompositionCatalog.asset`

이 library preview는 반복 가능한 조합과 원본 prefab 연결을 검증하는 편집용 배열이다. 최종 Farm Game View의 구도·밀도·scale 완료 증거는 아니며 실제 Zone 배치 뒤 다시 캡처한다.

### 반복 배치용 도시 풍경 Composition 후보

POLYGON City 335개 prefab을 건물 76·환경 65·소품 174·차량 9·캐릭터 9·FX 2개로 다시 분류하고, 첫 도시 세트 12종×A/B/C와 후속 6종 후보를 [Unity POLYGON City 반복 배치 Composition Set 조사](UnityPolygonCityCompositionSetResearch.md)에 정리했다.

이는 문서 조사 결과다. `도시풍경CompositionSet` prefab, catalog, builder와 preview Scene은 아직 생성하지 않았다.

## 5. URP와 Quality 기준선

| 항목 | PC | Mobile |
| --- | --- | --- |
| RP Asset | `PC_RPAsset` | `Mobile_RPAsset` |
| Renderer | `PC_Renderer` | `Mobile_Renderer` |
| Render Scale | 1.0 | 0.8 |
| Main Light Shadow | 사용 | 사용 |
| Additional Light Shadow | 사용 | 미사용 |
| Shadow Distance | 50 | 50 |
| Renderer Feature | SSAO, intensity 0.4 | 없음 |

현재 Editor quality는 `PC`, Graphics default pipeline도 `PC_RPAsset`이다. SSAO는 Volume override가 아니라 PC Renderer Feature이며 Mobile Renderer에는 없다.

`DefaultVolumeProfile.asset`에는 Color Adjustments·Tonemapping·Bloom 외에 `CopyPasteTestComponent*`, `TestVolume` 같은 test component가 섞여 있다. WORLD-2에서 이 asset을 제품 World profile로 재사용하거나 정리하지 않고, 필요한 override만 가진 Ssalddel 전용 profile을 별도로 만드는 후보로 남긴다. 기존 PC/Mobile RP Asset과 Renderer를 P0에서 수정하지 않는다.

## 6. P0 완료와 남은 Gate

확인 완료:

- 기존 Camera·World·SceneController·LifetimeScope·View·VisualRoot 조사
- City/Farm 실제 prefab inventory와 최소 allowlist 후보
- PC/Mobile URP·Renderer 분리와 SSAO 위치 확인
- 열린 Scene dirty 없음과 Console error 0 확인
- Unity EditMode test 25개 discovery

다음 WORLD-0에서는 Scene·vendor asset을 수정하지 않고 다음을 먼저 구현한다.

1. asset-neutral camera state와 World/Zone/Object focus
2. 90도 단계 회전, 제한된 pan·zoom과 pitch/FOV 범위
3. Unity `DioramaTopDownCameraRig` adapter
4. EditMode test와 compile·Console 검증

실제 Scene 저장, 카메라 최종 수치 확정과 Game View 비교는 생성 코드와 테스트가 통과한 다음 수행한다.

## 7. WORLD-0 구현 결과

P0 뒤 `C:\Users\user\ssalddel`에 다음 asset-neutral Presentation 코드를 추가했다.

- `Assets/Ssalddel/Runtime/World/DioramaCameraModels.cs`
- `Assets/Ssalddel/Presentation/World/DioramaTopDownCameraRig.cs`
- `Assets/Ssalddel/Presentation/World/DioramaForegroundOcclusionController.cs`
- `Assets/Ssalddel/Editor/DioramaCameraPrototypeBuilder.cs`
- `Assets/Ssalddel/Tests/EditMode/DioramaCameraTests.cs`

확인 결과:

- Perspective 3/4 pitch 45~55도 제한
- World/Zone/Object Focus별 직렬화 가능한 후보 거리와 FOV
- 지면 pan, 제한 zoom, Q/E 90도 단계 회전
- `DioramaOcclusionView`로 명시한 foreground만 cutaway
- 저장하지 않은 primitive Scene에서 Overview/Farm/Logistics/Market와 90도 회전 Game View 확인
- camera 집중 test 4/4 통과
- Unity EditMode 전체 29/29 통과: 제품 EditMode 20, Farm 3, Logistics 3, Market 3
- 최종 recompile 성공, Console error 0

Pipeline test runner 호출 종료 과정에서 runner 내부 `TaskCompletionSource.SetResult` 중복 callback 예외가 일시적으로 기록됐지만 네 assembly 결과는 모두 통과했다. Console을 비운 뒤 재compile했을 때 재발하지 않았고 최종 오류는 0건이다.

중간 PNG는 `C:\Users\user\ssalddel\artifacts\WORLD-0\`에만 두었다. 제품 Scene과 vendor prefab/material, URP Asset·Renderer, Quality 설정은 수정하지 않았다.
