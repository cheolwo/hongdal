# Simulation·Unity 미완료 실행 트리

> 이 문서는 `eng/execution-ledgers/simulation-unity.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 원장 개정: `simulation-unity-incomplete-execution.r3`
- 증거 단계 개정: `simulation-evidence-stages.r7`
- 마지막 확인일: `2026-08-17`
- 첫 실행축: `TRACK-DAEGWALLYEONG-L2-REAL-DATA`
- 중심 타일: `kr5186:l2:700:1145`

## 상태 요약

| 상태 | 수 |
| --- | ---: |
| 미착수 | 2 |
| 진행 중 | 14 |
| 차단 | 0 |
| 완료 | 0 |
| 대체됨 | 0 |

## 의존 실행 트리

```text
대관령 L2 E7 실제 플레이 종단 완결
├─ 공간 원자료: GEO-LEGAL-01 → GEO-DEM-01 → GEO-LANDCOVER-01
├─ 건물 관계: GEO-LEGAL-01 → DATA-BUILDING-01
├─ 파생 DB: DB-REGION-SUMMARY-01
├─ 공간 산출물: ART-TILE-01
├─ 서버 전송: API-STREAM-01
└─ Unity 실제 타일: UNITY-REAL-TILE-01
   ├─ live Simulation: SIM-LIVE-HTTP-01
   ├─ URP 표현: RENDER-URP-01 → PERF-HLOD-01
   ├─ 영속화: PERSIST-SIM-01
   ├─ 역할 UI: UI-FIGMA-01
   └─ 전국 확장: EXPAND-NATIONWIDE-01
```

## Simulation 서버

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `API-STREAM-01` 파생 DB 기반 실제 타일 Streaming | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | Unity 저장 Scene을 서버 기준 모드로 전환해 실제 Manifest·본문을 한 차례 종단 로드한다. |

## Unity 공간 산출물

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `ART-TILE-01` 중심 L2 Terrain·Mask 산출물 생성 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | SimulationWorldShell의 실제 서버 모드에서 중심 DEM Mesh를 로드하고 기존 Preview와 시각적으로 비교한다. |

## 파생 DB

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `DB-REGION-SUMMARY-01` 지역 표현 요약 migration과 평창 재파생 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 중심 타일 지역 표현 요약 응답을 Unity 정보판과 Synty 배치 예산에 연결한다. |

## 공간 원자료

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `GEO-DEM-01` DEM PhysicalElevation과 수직 기준 연결 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 수직 기준 공식 근거를 보완하고 실제 서버 산출물을 Unity 저장 Scene에서 로드해 지형 연속성과 시점 이동을 확인한다. |
| `GEO-LANDCOVER-01` 토지피복·수계·배치 마스크 연결 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 검증된 mask bit를 Synty 경관 계획기의 허용 토지피복·경사·수계 판정 입력으로 연결한다. |
| `GEO-LEGAL-01` 실제 법정동 경계와 건물 geometry 연결 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 중심 L2 타일과 교차하는 법정동 경계 및 좌표가 확인된 건물 footprint를 EPSG:5186로 절단한다. |

## Unity Runtime

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `UNITY-REAL-TILE-01` SimulationWorldShell 실제 중심 타일 로딩 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | SimulationWorldShell을 서버 기준 모드로 실행해 실제 DEM Mesh를 배치하고 mask를 Synty 배치 제한에 연결한다. |

## 공공데이터 파생

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `DATA-BUILDING-01` 행정동·법정동 건물과 공개 사업장 관계 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 중심 L2 타일의 좌표가 확인된 건물만 법정동·행정동 관계와 함께 지역 표현 요약 입력으로 승격한다. |

## Simulation 실행

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `SIM-LIVE-HTTP-01` 턴·전투·창고 live HTTP와 서버 상태 재조회 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 로컬 Simulation 서버를 실행해 Preview·Confirm·재조회 왕복을 Unity 저장 Scene에서 검증한다. |
| `PERSIST-SIM-01` Session·전투·관전 영속 저장과 재시작 복원 | 진행 중 | `E2 코드 준비` → `E7 실제 플레이 폐루프` | 기존 Save JSON 호환을 유지하는 durable Store를 연결하고 재시작 복원 시험을 추가한다. |

## 세계 상호작용

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `WI-001` 진부 Hub 입고 검수 공간–Simulation 종단 연결 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 진부 Hub 경관 그래프에 검수 공간 Node를 조립하고 승인된 공간 능력 연결로 Scenario 공간 공급자를 승격한다. |
| `WI-002` 진부 Hub 창고 적재 공간–Simulation 종단 연결 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 실제 진부 Hub 창고 Node와 검토된 보관 용량을 연결한 뒤 동일 WI를 재생하고 Unity 정보판의 경관 그래프 근거 표시를 확인한다. |

## 렌더링

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `RENDER-URP-01` Simulation Runtime 렌더링 의도 HTTP·URP Adapter | 진행 중 | `E2 코드 준비` → `E7 실제 플레이 폐루프` | 서버가 결정한 표현 Profile key를 조회하는 API와 Unity MaterialPropertyBlock·FX Adapter를 연결한다. |
| `PERF-HLOD-01` HLOD bake와 실제 렌더링 비용 측정 | 미착수 | `E1 계약·결정 완료` → `E7 실제 플레이 폐루프` | 실제 중심 타일을 기준으로 HLOD 대표 Renderer를 bake하고 Profiler 비용을 기록한다. |

## Unity UI

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `UI-FIGMA-01` Figma·MAUI 근거 Unity 역할 UI 확장 | 진행 중 | `E3 자동 시험 통과` → `E7 실제 플레이 폐루프` | 현재 Figma node와 MAUI route를 다시 확인하고 다음 역할 화면 하나를 Preview·Confirm·서버 재조회까지 완결한다. |

## 전국 확장

| ID | 상태 | 현재→목표 | 다음 실행 |
| --- | --- | --- | --- |
| `EXPAND-NATIONWIDE-01` 전국 행정동·법정동·건물·사업장 확장 | 미착수 | `E1 계약·결정 완료` → `E7 실제 플레이 폐루프` | 대관령 중심 L2가 E7에 도달한 뒤 두 번째 시군구를 같은 Recipe로 재현한다. |

## 승격 규칙

- 계획 문구나 코드 존재만으로 완료 처리하지 않는다.
- E4는 WI 공간 모판, E5는 권위 경관 조립, E6는 AreaSet 정밀 몰입·현실 문맥 결속, E7은 실제 플레이 폐루프다. GIS 결속은 E6 안의 독립 선택 축이다.
- DEM·도로는 공통 필수 자료가 아니다. 선택한 현실 결속 프로필이 요구할 때만 E6 준비도와 완료 판정에 참여한다.
- 실제 DB 적용, HTTP 왕복, Play Mode, Game View, commit과 push는 서로 다른 증거다.
- `Done`은 목표 증거 단계와 검증 자료가 모두 있을 때만 허용한다.
- 원자료가 부족하면 Fixture로 숨기지 않고 `Blocked` 또는 `InProgress`와 차단 사유를 유지한다.
