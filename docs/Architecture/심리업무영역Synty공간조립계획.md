# 심리·업무 영역 Synty 5팩 공간 조립 계획

## 문서 상태와 목적

| 항목 | 값 |
| --- | --- |
| 상태 | `TechnicalInventoryImplemented` · 발전소 조립은 `DesignPlanOnly` |
| 주력 자산 | POLYGON Nature·Farm·Town·City·Construction |
| 상세 설계 | 심리 영역의 회복 발전소·위협 발전소 |
| 후속 로드맵 | Farm·Town·City/Hub 업무 영역 |
| 구현됨 | 5팩 2,346개 전수 기술 대장과 의미 자산군·활용 트랙·계획 영역 분류 |
| 비포함 | 발전소 H 기계 대장 등록, Unity 조립 Prefab·Scene, Simulation 수치·저장 계약 |

이 문서는 보유한 다섯 Synty 팩을 조합해 H1 작업공간, H2 블록, H3 경관으로 발전시키는 공간 조립 기준이다. 세계 의미와 업무 결과 인과는 [업무 사건과 심리 영역 발전소 영향 규칙](지역사건-자연권위협-규칙.md), H 계층과 승격 관문은 [Synty 상향식 공간 재고 계획](Synty상향식공간재고계획.md)을 따른다.

Construction은 다섯 번째 AreaSet이나 독립 세계가 아니다. 구조물·공사·복구·격리·전환 상태를 Nature·Farm·Town·City/Hub에 입히는 **공통 조립 재료층**이다. 실제 업무 결과와 발전소 상태는 서버가 확정하고 Unity는 그 상태를 시설·환경·효과로 표현한다.

## 설치 자산 기준

2026-08-18 로컬 Unity 프로젝트 `Assets/Synty` 아래의 `.prefab`을 다시 센 결과다. 수량은 구매 증빙이나 기계 판독 공간 대장이 아니라 이번 조립 계획의 가용 표현 재료 확인값이다.

| 팩 | 설치 경로 | Prefab 수 | 주된 공간 역할 |
| --- | --- | ---: | --- |
| Nature | `Assets/Synty/PolygonNature` | 227 | 지형·식생·물·완충·심리 대비 |
| Farm | `Assets/Synty/PolygonFarm` | 498 | 생산·저장·용수·전력·복구 물자 |
| Town | `Assets/Synty/PolygonTown` | 702 | 생활·휴식·정원·주민 서비스 |
| City | `Assets/Synty/PolygonCity` | 335 | 전력·배관·경고·통제·물류 기반시설 |
| Construction | `Assets/Synty/PolygonConstruction` | 584 | 골조·발전 설비·공사·수리·격리 |
| 합계 |  | 2,346 | 중복 없는 설치 Prefab 파일 수 |

`PolygonGeneric`과 `PolygonStarter`는 이번 다섯 주력 팩에 포함하지 않는다. 다른 팩 Prefab이 참조하는 필수 의존성이 있을 때만 사용하며, 공간의 정체성이나 누락 자산을 대신하는 근거로 삼지 않는다.

2026-08-19 Unity의 `SyntyPackAssetInventoryCatalog`를 `synty-pack-inventory.v2`로 생성했다. 2,346개 전부를 1,499개 의미 자산군으로 묶었고 자동 분류 2,345개와 사람 검토 대기 1개를 분리했다. Vehicle 51개도 정식 `Vehicles` 분류와 `vehicle` 활용 트랙을 가지며, 남은 검토 대상은 Nature `Misc` 1개다. 이 기술 대장 구현은 보유 표현 재료를 빠짐없이 찾고 분류한 증거이며, 발전소 H 승인이나 실제 Scene 활용 증거는 아니다.

### 팩·원본 분류별 전수 기준

| 팩 | 원본 분류별 Prefab 수 |
| --- | --- |
| Nature | FX 24 · Misc 1 · Plants 42 · Props 31 · Rocks 30 · Terrain 33 · Trees 66 |
| Farm | Buildings 17 · Characters 14 · Environments 67 · FX 11 · Generic 39 · Plants 173 · Props 166 · Vehicles 11 |
| Town | Buildings 143 · Characters 9 · Environment 97 · Generic 33 · Items 72 · Props 340 · Vehicles 8 |
| City | Buildings 76 · Characters 9 · Environments 65 · FX 2 · Props 174 · Vehicles 9 |
| Construction | Buildings 74 · Characters 44 · Environments 36 · Generic 19 · Items 49 · Props 300 · Tools 39 · Vehicles 23 |

원본 폴더의 `Environment`와 `Environments`, `Misc`처럼 팩마다 다른 이름은 경로와 원본 지문을 보존하면서 의미 대장에서 각각 `Environments`, `ManualReview`로 정규화한다. 기존 Farm·Town·City 기술 대장의 `inventoryId`는 바꾸지 않고, 정규화 분류와 자산군을 호환 필드로 추가한다.

### 최대 활용의 완료 기준

`최대한 활용`은 2,346개를 같은 Scene에 모두 배치하거나 모든 변형을 억지로 노출하는 뜻이 아니다. 다음 세 지표를 독립적으로 기록한다.

| 지표 | 완료 조건 | 완료로 보지 않는 것 |
| --- | --- | --- |
| 원본 스캔 범위 | 2,346개 모두 고유 원본 지문과 기술 수치를 가짐 | 설치 폴더 수만 센 상태 |
| 자산군 분류 범위 | 모든 Prefab이 의미 자산군·주 활용 트랙·최소 한 적용 영역 또는 보류 사유를 가짐 | 파일명만 나열한 목록 |
| 실제 검증 범위 | 계획된 자산군의 대표 Prefab이 모판·Runtime 단계별 증거를 가짐 | 문서 후보나 Catalog 등록만 된 상태 |

중복 변형, 현재 기능에 맞지 않는 Character, 성능 예산을 넘는 조합도 삭제하지 않는다. `보류` 또는 `제외`와 사유를 남기면 전수 분류에는 포함하되 실제 활용 완료율에는 포함하지 않는다.

## 공통 조립 문법

### 팩별 책임

| 조립 층 | 담당 팩 | 판단 질문 |
| --- | --- | --- |
| 장소 골격 | Nature·Farm·Town·City 중 하나 | 이 H1은 숲, 생산지, 생활지, 물류지 중 어디에 속하는가? |
| 기능 골격 | Construction | 무엇을 짓고, 점검하고, 고치고, 격리하는 공간인가? |
| 생활·업무 단서 | 나머지 보조 팩 1~2개 | 누가 이곳을 쓰며 앞뒤 공간과 무엇을 주고받는가? |
| 상태 표현 | 조명·연기·식생·손상·차단물 | 서버가 확정한 상태를 어떤 채널로 보여주는가? |

한 H1에는 `주도 팩 1개 + Construction 기능층 + 보조 팩 1~2개`를 기본으로 사용한다. 다섯 팩을 모든 공간에 같은 비율로 넣지 않는다. 다섯 팩의 활용도는 한 공간의 물체 수가 아니라 전체 H1~H3 목록에서 각 팩이 분명한 역할을 맡는지로 판단한다.

### 모든 자산 분류를 포함하는 활용 트랙

| 활용 트랙 | 원본 분류 | 조립 책임 | 권위 경계 |
| --- | --- | --- | --- |
| 공간 골격 | Terrain·Buildings·Environment(s)·Plants·Rocks·Trees | 지면·건축·식생·외곽 실루엣 | 실제 지형·건물·토지피복 근거를 대체하지 않음 |
| 기능·생활 소품 | Props·Generic | 작업, 생활, 저장, 경고와 상태 단서 | 소품 존재만으로 업무 능력이나 완료를 만들지 않음 |
| 행위자 | Characters | 역할별 NPC·작업자·주민 표현 | Character Prefab이 역할 자격·작업 성공을 결정하지 않음 |
| 이동수단 | Vehicles | 농업·서비스·물류 이동 표현 | 서버가 확정한 경로와 상태 사본만 재생함 |
| 작업 도구·휴대물 | Items·Tools | 예약 물자·작업 단계·손 도구 표현 | 도구 표시가 예약·소비·재고 변경을 확정하지 않음 |
| 상태·분위기 | FX | 물·연기·먼지·불꽃·작업 효과 | Collider와 업무 판정을 갖지 않는 표현 전용 효과 |
| 수동 검토 | Misc와 자동 정규화 실패 항목 | 의미·성능·의존성 사람 판정 | 검토 전 자동 배치 금지 |

각 Prefab은 기술 대장에서 하나의 의미 자산군과 주 활용 트랙을 갖고 필요한 보조 트랙을 추가할 수 있다. Character·Vehicle·Item·Tool·FX도 전수 대장에는 포함하지만 정적 경관 자동 배치에는 섞지 않는다.

### 의미 자산군 대장

개별 연번·개폐·크기·손상·색상 변형은 원본별 지문을 유지하면서 공통 의미 자산군으로 묶는다. 자동 정규화는 후보를 만들 뿐 사람 승인을 대신하지 않는다.

| 항목 | 의미 |
| --- | --- |
| 자산군 고유 식별자 | 팩·정규화 분류·정규화 이름으로 만든 표현 전용 식별자 |
| 주·보조 활용 트랙 | 공간, 소품, 행위자, 차량, 도구, FX 중 실제 사용 방식 |
| 계획 적용 영역 | NatureHome·Farm·Town·CityHub 중 최소 한 곳 |
| 조립 연결 | H1/H2 후보 또는 Construction 상태층. 공간 권위 참조가 아님 |
| 기술 조건 | Renderer 경계, Triangle, Material Slot, Collider, Animator, Particle, LOD |
| 검토 상태 | `AutoClassified`, `NeedsHumanReview`, `Planned`, `SeedbedVerified`, `RuntimeVerified`, `Reserved`, `Excluded` |
| 보류·제외 사유 | 중복 변형, 의존성 전용, 성능 초과, 분위기 불일치, 현재 capability 부재 등 |

유료 원본 파일명·경로·GUID 전체 목록은 Unity 내부 Catalog에만 둔다. 사람이 읽는 공개 요약은 팩·정규화 분류·활용 트랙·자산군 수와 검토 상태만 제공한다.

### 배치 순서

1. 기존 H1의 역할, 필수 능력, 작업 용량과 외부 연결구를 먼저 고정한다.
2. 주도 팩으로 바닥·식생·건물 전면 등 장소 골격을 만든다.
3. Construction으로 핵심 설비, 작업대, 안전 경계와 공사 중 상태를 만든다.
4. 보조 팩으로 생활·생산·물류 계보를 읽을 수 있는 단서만 추가한다.
5. 주 이동로, 비상로, 화물 접근로와 시야선을 장식물보다 먼저 검사한다.
6. 상태 표현은 별도 `VisualRoot` 아래에 두고 원본 Synty Prefab에는 업무 로직을 넣지 않는다.

Prefab 이름·GUID·Material·Scene 경로·GameObject 이름은 공간 Stable ID, Simulation 상태, 운영 Command의 근거가 될 수 없다. 원본 Prefab을 수정하지 않고 조립 Prefab 또는 표현용 하위 객체에서 참조한다.

### Construction 공통 상태층

Construction은 새 팩 표현 H1이나 156개 기준 경관 문법의 일곱 번째 계열로 추가하지 않는다. 각 A/B/C 공간 조립물 위에 다음 상태층을 독립적으로 결합한다.

| 상태층 | 표현 목적 | 대표 Construction 자산군 |
| --- | --- | --- |
| 정상 운영 | 완성된 시설과 안전한 접근 상태 | 완성 구조물·전력·급수·안전 표지 |
| 점검·정비 | 고장 전 점검과 소규모 수리 | Tool·Item·이동 조명·PowerBox·작업대 |
| 공사 진행 | 신설·증설·구조 변경 | 골조·비계·크레인·공사 차량·자재 |
| 손상·격리 | 위험 통제와 접근 차단 | 노출 철근·구덩이·바리케이드·콘·경고 설비 |
| 복구·재가동 | 수리 완료와 운영 복귀 준비 | 발전기·용수 설비·보강재·정리된 적치·시험 가동 FX |

상태층은 바닥 면적, H1 소켓, 외부 연결구, 업무 용량과 필수 이동로를 변경하지 않는다. Collider·NavMesh·접근 권한이 달라져야 하면 단순 상태 표현이 아니라 별도 H 설계 변경으로 검토한다. A/B/C는 공간 배치 변형이고 위 다섯 상태층은 서버 확정 상태의 표현 선택이므로 서로 대체하지 않는다.

## H1에서 H4까지의 발전소 구조

| 깊이 | 회복 계열 | 위협 계열 | 호환 원칙 |
| --- | --- | --- | --- |
| H1 새 기획 후보 | `h1-action:nature-recovery-plant-core.r1` 회복 발전 동력핵 | `h1-action:nature-threat-plant-core.r1` 위협 발전 집속핵 | 문서 후보이며 아직 기계 대장·WI에 등록하지 않음 |
| H1 기존 재사용 | `h1-stock:nature-restoration-site`, `h1-stock:nature-safe-recovery-camp` | `h1-stock:nature-threat-watch`, `h1-stock:nature-incident-trace`, `h1-stock:nature-emergency-retreat` | 기존 WI와 계획 용량을 유지 |
| H2 기존 지식 | `h2-candidate:nature-restoration-recovery` | `h2-candidate:nature-threat-response` | 기존 조립법과 외부 연결구를 유지 |
| H2 조립법 | `h2-composition:nature-restoration-recovery.r1` | `h2-composition:nature-threat-response.r1` | 새 동력핵·집속핵은 후속 대장 개정 전까지 조립 소켓 후보 |
| H3 | `h3-candidate:nature-threat-recovery` 안의 회복 발전소 | 같은 H3 안의 위협 발전소 | 두 H2와 완충 지형·복귀 동선을 하나의 경관으로 묶음 |
| H4·AreaSet | `NatureHome` 심리 영역 | `NatureHome` 심리 영역 | 기존 식별자·팩 코드·API·저장 계약을 변경하지 않음 |

두 발전소는 단일 장식물이 아니라 여러 H1을 묶은 H2 복합 공간이다. 새 H1 후보는 발전 설비 자체를 점검·관찰하는 중심 기능을 나타내고, 기존 H1은 복원·회복·관찰·흔적 조사·후퇴를 담당한다. 실제 H1 승격 전에는 존재하지 않는 WI를 만들거나 기존 H2 정의가 갱신됐다고 기록하지 않는다.

## 회복 발전소 H2 조립안

기존 기준 크기 `220m × 180m LocalMeters`와 `IncidentRouteInput`, `RetreatRecoveryInput`, `SafeCoreOutput`, `RestoredRouteOutput` 연결구를 유지한다. 이 크기는 위치 독립 설계값이며 실제 지역 면적이 아니다.

### 공간 순서

```text
사건·후퇴 입력
→ 복원 작업장
→ 회복 발전 동력핵
→ 안전 회복 야영지
→ 안전 생활핵 또는 복원된 탐험로
```

- 동력핵은 복원 작업장과 안전 회복 야영지 사이에서 두 공간이 모두 보이는 위치에 둔다.
- 화물 접근은 복원 자재 적치까지만 들어오고, 파티 휴식 동선과 교차하지 않게 분리한다.
- 안전 회복 야영지에서는 굴뚝·구덩이 같은 위협 실루엣이 주 시야를 점유하지 않게 한다.
- 물·전력·수리 흔적은 회복이 저절로 발생하는 것이 아니라 유지되는 시설임을 보여준다.

### 대표 Prefab 후보

| 팩 | 실제 설치 예시 | 쓰임 |
| --- | --- | --- |
| Construction | `SM_Prop_Generator_Large_01`, `SM_Bld_WaterTower_01`, `SM_Bld_WaterTank_01`, `SM_Prop_PowerBoxes_01`, `SM_Bld_Portable_Office_01`, `SM_Prop_Scaffold_Preset_01` | 동력핵·용수·제어·수리·관리 |
| Nature | `SM_Tree_Willow_Medium_01`, `SM_Plant_FlowerPatch_01`, `SM_Plant_Grass_01`, `SM_Rock_Rounded_01` | 살아 있는 식생·완충·안정된 외곽 |
| Farm | `SM_Prop_Windmill_01`, `SM_Bld_Silo_Small_01`, `SM_Prop_Power_Pole_01`, `SM_Prop_Power_Lines_01`, `SM_Prop_PalletCrate_01` | 분산 전력·저장·복구 물자 인계 |
| Town | `SM_Prop_ParkBench_01`, `SM_Prop_Outdoor_Light_01`, `SM_Env_Gardenbox_Single_01`, `SM_Bld_GardenShed_01` | 휴식·야간 안전·생활 관리 |
| City | `SM_Prop_Power_Cables_01`, `SM_Prop_PowerBox_01`, `SM_Prop_Pipe_Preset_01`, `SM_Prop_Planter_01` | 전력·배관의 연결성과 정돈된 설비 표현 |

회복 강도는 공식 상태 코드가 확정되기 전까지 밝기, 물 흐름, 가동 설비 수, 수리 완료 흔적과 식생 활력이라는 독립 표현 채널로만 계획한다. 이 표현이 업무 성공이나 `WorldTick` 변화를 확정하지 않는다.

## 위협 발전소 H2 조립안

기존 기준 크기 `240m × 200m LocalMeters`와 `SafeCoreInput`, `ThreatBandContinuation`, `EmergencyExit`, `RecoveryHandoff` 연결구를 유지한다.

### 공간 순서

```text
안전 생활핵 입력
→ 위협 감시 지점
→ 위협 발전 집속핵
→ 사건 흔적 조사 구역
→ 긴급 후퇴 또는 회복 발전소 인계
```

- 집속핵은 멀리서 식별되는 굴뚝·노출 골조를 사용하되 플레이어의 필수 이동로 위에 두지 않는다.
- 감시 지점에서는 집속핵, 사건 흔적, 긴급 출구가 서로 다른 방향으로 읽혀야 한다.
- 바리케이드와 철근은 위험 경계를 만들지만 `EmergencyExit`와 `RecoveryHandoff`를 막지 않는다.
- 잔해·쓰레기·노출 배관은 원인 경로를 추적할 수 있게 군집화하고 무작위 장식처럼 흩뿌리지 않는다.

### 대표 Prefab 후보

| 팩 | 실제 설치 예시 | 쓰임 |
| --- | --- | --- |
| Construction | `SM_Bld_SmokeStack_01`, `SM_Bld_ConcreteFrame_Wall_01`, `SM_Bld_ConcreteRebar_Wall_01`, `SM_Prop_Barricade_Concrete_01`, `SM_Env_Dirt_Hole_01`, `SM_Prop_Generator_01`, `SM_Prop_Light_Portable_01` | 집속핵·노출 골조·격리·불안정 작업장 |
| Nature | `SM_Tree_Dead_01`, `SM_Tree_Stump_01`, `SM_Rock_Pile_01` | 고사·단절·훼손된 완충대 |
| Farm | `SM_Prop_Fence_Wire_01`, `SM_Prop_Power_Pole_01` | 차단된 생산 경계와 불안정 전력 인계 |
| Town | `SM_Prop_Drain_01`, `SM_Bld_House_DrainPipe_01`, `SM_Prop_TrashBag_01` | 생활권에서 유입된 오염·방치 단서 |
| City | `SM_Prop_Cone_01`, `SM_Prop_Barrier_01`, `SM_Prop_Trashbin_01`, `SM_Prop_Pipe_Preset_01` | 통제·우회·물류 및 기반시설 고장 단서 |

위협 강도는 공식 상태 코드가 확정되기 전까지 연기·먼지, 경고등, 차단물 밀도, 노출 철근, 잔해와 고사 식생이라는 표현 채널로만 계획한다. 심리 영역의 전투·복구 연출은 업무 영역의 원인 사건을 해결하지 않는다.

## A/B/C 공간 변형

A/B/C는 시간 흐름이나 회복·위협 단계가 아니라 **같은 기능을 수행하는 공간 배치 변형**이다. 같은 발전소 계열 안에서 바닥 면적, 외부 연결구 위치·방향, 핵심 H1 소켓과 이동 가능 폭을 유지한다.

| 변형 | 조립 목표 | 허용 변화 | 고정 항목 |
| --- | --- | --- | --- |
| A 명료형 | 첫 검토와 플레이 동선을 가장 쉽게 읽음 | 장식 최소화, 단일 주 순환로 | 면적·연결구·핵심 소켓 |
| B 작업확장형 | 공사·수리·화물 적치 과정을 강조 | 보조 작업로와 적치 구역 추가 | A의 진입·출구와 작업 용량 |
| C 서사밀집형 | 반복 방문 시 공간 기억과 사건 흔적을 강화 | 높이·밀도·보조 소품과 시야 가림 조정 | 비상로·시야 확인점·외부 인계 |

회복 출력과 위협 압력의 변화는 A/B/C로 나타내지 않는다. 각 변형 안에서 동일한 상태 표현 소켓을 켜고 끌 수 있어야 하며, 상태 표현 때문에 Collider·NavMesh·상호작용 위치가 달라지지 않게 한다.

## 업무 영역 후속 조립 로드맵

| 순서 | 업무 영역 | 재사용 H2 | Construction 적용 | 심리 영역 인계 |
| --- | --- | --- | --- | --- |
| P2 | Farm | `farm-incident-containment`, `farm-loss-restoration-handoff` | 손상 생산시설 점검, 격리 울타리, 관개·전력 수리, 복구 물자 적치 | 해결 결과와 복원 물자를 회복 발전소 입력 후보로 전달 |
| P3 | Town | `town-contamination-control`, `town-recall-relief` | 시장 차단선, 배수·정화 작업, 주민 안내·구호 임시 거점 | 오염 처리 결과와 주민 구호 결과를 원인별로 전달 |
| P4 | City/Hub | 후속 물류 회복력 H2 후보 | 도로 공사, 하역 우회, 창고·전력·배관 정비, 서비스 차량 대기 | 적체·정비 결과를 해당 경로의 발전소 입력 후보로 전달 |

업무 사건은 먼저 발생 영역의 직접 흔적과 후속 WI를 가진다. 서버가 확정한 결과 계보만 심리 영역으로 전달하며 미리보기 차단과 일반 취소는 발전소 입력으로 표현하지 않는다. 업무 영역별 Construction 조립은 기존 H2 연결구와 업무 용량을 보존한 뒤 별도 문서·대장에서 구체화한다.

## 제작·승격 순서

1. 기존 Farm·Town·City 1,535개 기술 대장의 고유 식별자를 보존하면서 Nature·Construction을 더한 2,346개 전수 기술 대장을 만든다.
2. 모든 항목에 정규화 분류, 의미 자산군, 주 활용 트랙, 계획 적용 영역과 검토 상태를 계산하고 수동 검토 대기 항목을 분리한다.
3. 회복 동력핵과 위협 집속핵을 `ExploratoryInventory` H1 후보로 검토한다.
4. 각 H1의 예상 플레이, 능력, 작업 용량과 기존 H2 내 상대 소켓을 확정한다.
5. 회복·위협 H2의 A/B/C 모판을 같은 면적·연결구로 조립하고 동선을 비교한다.
6. 두 H2와 완충 지형·복귀로를 `nature-threat-recovery` H3 안에서 닫는다.
7. 사람 검토 뒤에만 기계 판독 H 대장과 생성 규칙을 개정한다.
8. 승인된 설계를 실제 `NatureHome` AreaSet에 놓는 작업은 E5로, 필요한 공공데이터 계보는 E6로 별도 처리한다.
9. Unity 조립 시 원본 Prefab을 보존하고 저장 Scene 배선·Play Mode·Game View를 별도 증거로 남긴다.

## 단계별 검증 관문

| 단계 | 필수 증거 | 아직 성립하지 않는 것 |
| --- | --- | --- |
| 문서 기준 | 5팩 역할·전수 수량·분류 규칙·두 발전소·업무 영역 로드맵, link와 `git diff --check` | Catalog 생성, Unity 컴파일, Scene 배치 |
| 기술 대장 | 2,346개·5팩 수량·고유 ID·원본 지문·정규화 분류·활용 트랙·자산군 검증 | H 승인, 실제 조립, Game View |
| 모판 검증 | 대표 자산군 조립, A/B/C와 상태층 불변 조건, EditMode | 실제 AreaSet 배치와 E5 |
| Runtime 검증 | 저장 Scene 배선, Play Mode 입력, Game View·Console | 서버 원장 완료나 운영 효과 |
| E5~E7 | 실제 H 이동 폐루프, 공공데이터 계보, 서버 재조회와 플레이 폐루프 | 문서·표현만으로 자동 승격 없음 |

## 완료 판정

현재 단계의 완료는 다섯 팩 역할과 2,346개 기술 대장·전수 분류가 구현되고, 두 발전소의 H1→H2→H3 관계, 대표 설치 Prefab, 연결구 보존, A/B/C·Construction 상태층 불변 조건과 업무 영역 후속 순서가 서로 모순 없이 기록되는 것이다. 기술 대장만으로 새 H1·H2가 등록되거나 E4~E7, Unity Scene, Simulation 상태가 성립하지 않는다.
