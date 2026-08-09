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
| 활성 Scene | `Assets/Ssalddel/Experiments/SyntyCityPackIntegration/UrbanLogisticsCityPackVerticalSlice.unity` |
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
- `Assets/Ssalddel/Experiments/SyntyCityPackIntegration/UrbanLogisticsCityPackVerticalSlice.unity`
- `Assets/Ssalddel/Experiments/SyntyCityPackIntegration/UrbanMarketCityPackVerticalSlice.unity`

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

이 파일명은 Presentation catalog 입력 후보일 뿐 Data·Simulation·Operational contract나 stable ID가 아니다.

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
