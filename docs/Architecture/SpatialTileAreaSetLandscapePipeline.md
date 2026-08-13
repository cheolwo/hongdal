# 공간 Tile·Area·AreaSet 경관 생성 파이프라인

## 목적과 권위 경계

공간 원본은 배치 가능 위치를, 환경부 면적 통계는 행정구역 전체의 경관 구성 목표를, Simulation은 Farm·Town·Hub 역할과 구체 작물을 결정한다. Synty Prefab과 높이 과장은 `PresentationOnly`이며 법정동·공간 관측·업무 완료를 변경하지 않는다.

```text
원본 snapshot → EPSG:5186 고정 Tile → 공간 Layer
→ LandAllocationResult → LandscapeCompositionPlan
→ Area → 건물·공개 사업장 관계 → 건물 배치 계획
→ 그래픽 표현 계획 → ScenarioRoute → AreaSet
→ 배치·성능 검증 → 마지막 시각 자산 연결 → VisualRoot·Unity 산출물
```

고정 격자는 L0 8km, L1 2km, L2 500m이고 식별자는 `kr5186:l{level}:{x}:{y}`다. 생성 범위는 L0 300m, L1 150m, L2 60m Halo를 포함하고 최종 산출물은 가운데 핵심 범위만 사용한다. 결정적 seed는 타일 내부 순번이 아니라 EPSG:5186 세계 좌표와 의미 key로 계산한다.

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
7. 대관령면 L2 500m 수작업 Reference Tile 비교
8. Triangle·Material Slot·Draw Call·Shadow Caster·Collider·Animator 성능 예산

위 8단계는 자산 연결 전에 수행하는 중간 검증이다. `final-visual-asset-binding`은 그 뒤에 오는 파이프라인의 마지막 단계다. 서버와 공간 DB는 의미 기반 `VisualKey`까지만 결정하고, Unity가 현재 선택된 시각 자산 대장에서 토지피복·영역 역할·원본 경사·LOD·성능 조건을 통과한 항목만 실제 Prefab으로 해석한다. 현재 대장은 보유 Synty 팩을 사용하지만, 원본 Prefab 이름이나 경로는 공간·건물·Simulation 고유 식별자에 들어가지 않는다. 하나라도 연결이 거부되면 불완전 Unity 산출물을 저장하지 않고 거부 건수와 원인을 배치 검증 기록으로 남긴다.

시각 자산 대장은 허용 토지피복·역할·경사, footprint·여백, collision 정책, LOD, 군집·회전 가능 여부와 예상 렌더링 비용을 가진다. Overview와 Region 단계는 Cluster/HLOD 대상이다.

## 첫 세로 단위

`area-set:sim:pyeongchang:farm-hub-town.v1`은 대관령면 Farm, 진부면 Hub, 평창읍 Town과 두 `ScenarioRoute`를 참조한다. 공식 도로 공간자료가 연결되기 전까지 회랑을 실제 도로로 주장하지 않는다. Unity는 `SimulationWorldShell` 한 Scene에서 카메라 거리에 따라 L0/L1/L2 표현만 전환하며 서버나 Simulation 상태를 변경하지 않는다.

각 Area는 타일 레이어 결과뿐 아니라 공유 공공데이터 DB의 건축물대장·GIS 건물도형과 공개 지방행정 인허가 사업장 관점별 조회 결과를 읽는다. 관측 건물은 도형 또는 대표점에 배치하고, 도형이 없는 건물은 임의 좌표에 놓지 않는다. 자료 부족을 보완하는 대표 건물은 `AreaComposition`, Farm·Hub·Town 역할 건물은 `Scenario` 근거로 분리한다. 공개 사업장명과 업종은 간판·상점 계열 시각 후보의 근거가 될 수 있지만 실제 입주 확정이나 운영 업무 완료를 뜻하지 않는다.
