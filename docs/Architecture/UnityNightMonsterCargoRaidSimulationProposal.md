# Unity 야간 몬스터 화물트럭 약탈 Simulation 제안

## 1. 제안 목적과 상태

Farm·Town·Regional Logistics Hub·City 사이를 약 300m 규모로 벌린 현재 World는 지역의 독립성과 이동 거리는 잘 드러나지만, 긴 도로와 골목의 체험 밀도는 아직 낮다. 이 구간을 낮에는 평온한 생활·물류 통로, 저녁과 밤에는 몬스터가 숨어 있다가 화물트럭을 습격하는 긴장감 있는 Simulation 무대로 전환한다.

목표는 현실 물류에 범죄를 덧씌우는 것이 아니라 다음과 같은 stylized game scenario를 만드는 것이다.

> 저녁이 되면 길가 조형물과 골목이 은신처로 바뀌고, 몬스터가 화물트럭을 추적해 상자를 탈취한 뒤 새벽 전에 퇴각한다.

- 기준일: 2026-08-10
- 상태: 구현 전 제안
- 이번 문서에서는 Unity 코드·prefab·Scene을 변경하지 않는다.
- 몬스터·약탈·화물 손실은 `Simulation` 전용이다.
- 실제 운영 배송·재고·기사·위치 데이터에는 적용하지 않는다.

## 2. 핵심 원칙

1. **운영 물류와 분리한다.** 실제 배송차량이 습격당하거나 화물이 손실된 것처럼 표시하지 않는다.
2. **시간만으로 결과를 만들지 않는다.** 밤은 출몰 가능 조건이며, 실제 조우는 Simulation snapshot과 seed가 결정한다.
3. **Navigation은 연출 수단이다.** NavMesh와 충돌 판정이 약탈 성공 여부를 결정하지 않고 이미 결정된 encounter phase를 표현한다.
4. **가족 친화적 Synty 톤을 유지한다.** 피·신체 훼손·사실적 폭력을 배제하고 상자 탈취, 길막, 경적, 도주 중심으로 표현한다.
5. **300m 구간마다 성격을 다르게 한다.** 같은 몬스터를 균일하게 뿌리지 않고 지역 경관과 이동 목적에 맞는 매복 방식을 사용한다.
6. **낮의 평온함을 보존한다.** 낮에는 몬스터를 숨기고 은신처가 일반 조형물·폐자재·수목처럼 읽히게 한다.
7. **결과는 재현 가능해야 한다.** 같은 scenario revision, seed와 Tick은 같은 출몰 위치·대상·탈취 결과를 만든다.

## 3. 권위와 데이터 흐름

### 3.1 허용 모드

| 모드 | 몬스터 표시 | 화물 결과 | 용도 |
| --- | --- | --- | --- |
| `FixedPreview` | 고정 phase 미리보기 | 없음 | 미술·animation 제작 |
| `Simulation` | Simulation snapshot에 따라 표시 | Simulation 원장에만 기록 | 게임 session·save·replay |
| `Operational` | 기본 비활성 | 절대 변경하지 않음 | 실제 물류 World Projection |

운영 데이터를 배경으로 Simulation overlay를 시험해야 한다면 화면에 `Simulation Overlay`를 명시하고, 운영 화물 수량·배송 상태·재고 revision과 완전히 분리된 복제 Presentation만 사용한다.

### 3.2 제안 흐름

```text
SimulationClock + ScenarioSeed
        +
TruckJourneySimulationSnapshot
        +
NightEncounterCorridorProfile
        ↓
NightEncounterSimulationEngine
        ↓
NightEncounterSimulationSnapshot
        ↓
NightEncounterInterpreter
        ↓
NightMonsterRaidPresentationModel
        ↓
Monster / Truck / Cargo / FX Presenter
        ↓
SyntyMonsterVisualAdapter + 기존 Vehicle/Cargo View
```

금지할 흐름은 다음과 같다.

```text
NavMesh 도착 또는 attack animation 완료
        ✕
실제 배송 실패·실재고 감소·운영 Command 확정
```

## 4. 시간대 출몰 규칙

기존 시간대 Presentation의 `GoldenDusk`와 `Night`를 그대로 사용하되 출몰 상태는 별도 Simulation 규칙으로 둔다.

| 시각 | 상태 | 화면 연출 |
| --- | --- | --- |
| 17:30~19:00 | `Dormant` | 길가 조형물·골목은 평범하게 유지 |
| 19:00~19:30 | `Warning` | 까마귀·먼지·흔들리는 표지·희미한 눈빛 같은 징후 |
| 19:30~21:00 | `Emerging` | 은신처에서 소규모 정찰 몬스터 출현 |
| 21:00~02:30 | `Active` | 매복·추적·화물 탈취 encounter 허용 |
| 02:30~04:00 | `Fading` | 신규 encounter 감소, 기존 무리 퇴각 우선 |
| 04:00~04:30 | `Retreat` | 몬스터가 은신처로 복귀하고 흔적만 남음 |
| 04:30 이후 | `Despawned` | 도로·골목을 낮 상태로 복구 |

정확한 출몰 여부는 `DateTime.Now`나 Unity frame time이 아니라 Simulation session의 timezone·Tick·seed·rule revision으로 결정한다. 시간 scrubber는 Presentation 미리보기만 수행하며 Simulation 결과를 만들지 않는다.

## 5. 구간별 몬스터와 조형물 연출

### 5.1 Farm → Town: 밭길 잠복형

- 낮 조형물: 풍차, 우물, 건초더미, 허수아비, 목책
- 밤 은신처: 건초더미 뒤, 옥수수밭 가장자리, 폐수레 옆
- 몬스터 성격: 작은 고블린·들짐승형 정찰대
- 습격 방식: 트럭 뒤를 따라붙어 느슨하게 묶인 상자 한두 개를 빼앗음
- 색과 FX: 낮은 황갈색 안개, 반딧불과 구분되는 짧은 눈빛, 흙먼지

### 5.2 Farm → Hub: 바위·급수탑 길막형

- 낮 조형물: 급수탑, 바위 군집, 창고 표지, 큰 수목
- 밤 은신처: 바위 뒤와 급수탑 하부
- 몬스터 성격: 체구가 큰 골렘·트롤형
- 습격 방식: 도로에 장애물을 밀어 트럭을 감속시키고 작은 몬스터가 화물을 탈취
- 색과 FX: 묵직한 발자국 먼지, 도로 진동, 낮은 주황 warning light

### 5.3 Town → Hub: 골목 협공형

- 낮 조형물: 버스 정류장, 피크닉 테이블, 화분, 골목 간판
- 밤 은신처: 건물 모서리, 정류장 뒤, 좁은 서비스 골목
- 몬스터 성격: 빠른 고블린 운반대
- 습격 방식: 앞뒤 골목에서 동시에 나와 잠시 길을 막고 상자를 릴레이로 운반
- 색과 FX: 깜빡이는 가로등, 쓰러지는 쓰레기통, 빠른 발소리

### 5.4 Town → City: 지붕·고가 감시형

- 낮 조형물: 공원 벤치, 가로수, 버스 정류장, 저층 상가
- 밤 은신처: 옥상 가장자리, 간판 뒤, 고가 구조물
- 몬스터 성격: 그림자·박쥐형 감시자와 지상 탈취대
- 습격 방식: 감시자가 트럭 도착을 알리고 지상 무리가 짧게 추적
- 색과 FX: 건물 실루엣, 이동하는 그림자, 짧은 보라색 신호

### 5.5 Hub → City: 산업 폐자재 약탈형

- 낮 조형물: 물류 Station, 가로등, 팔레트, 컨테이너, 안전 cone
- 밤 은신처: 컨테이너 사이, 폐팔레트 더미, Dock 바깥 서비스 도로
- 몬스터 성격: 조직적인 scavenger 무리
- 습격 방식: 트럭을 Dock 진입 전에 포위하고 팔레트 단위 화물을 나누어 운반
- 색과 FX: 적색 Dock beacon, 금속 타격음, 지게차 경고음과 구분되는 몬스터 신호

### 5.6 도로변 완충 Composition

약 300m 구간을 넓은 잔디 바닥과 도로만으로 채우지 않는다. 각 corridor에는 수목·조형물·독립 주택이 모인 작은 `Roadside Cluster`를 두어 낮에는 생활감과 거리의 리듬을 만들고, 밤에는 경계·은신·조명·피난 landmark로 재사용한다.

```text
Region 출구
  → 출발부 수목·표지 군집
  → 독립 주택 또는 작은 시설
  → 중간 휴게·조형물 군집
  → 야간 encounter pocket
  → 도착 Region 예고 수목·표지
  → Region 입구
```

한 corridor의 1차 배치 기준은 다음과 같다.

| 요소 | 권장 수량 | 배치 기준 |
| --- | --- | --- |
| 수목 군집 | 3~5개, 전체 12~24그루 | 한 줄로 세우지 않고 45~80m 간격의 비대칭 군집으로 구성 |
| 독립 주택·소형 시설 | 1~3채 | 도로에서 12~25m 물리고 진입로·마당을 함께 배치 |
| 조형물·생활 landmark | 2~4개 | 우물·풍차·정류장·쉼터·기념석·급수탑처럼 구간 성격을 표시 |
| 작은 휴게·분기 공간 | 1~2개 | 트럭 회차·NPC 대기·카메라 focus가 가능한 pocket 제공 |
| 야간 은신처 socket | 2~3개 | 낮에는 건초·바위·수목·폐팔레트로 자연스럽게 숨김 |

집을 도로 양쪽에 연속 배치하면 Farm·Town·City가 다시 하나의 붙은 도시처럼 보인다. 따라서 건물 사이에는 최소 한 개 이상의 열린 경관 구간을 남기고, 교차로·Gate 전방 20m와 주요 카메라 시선축은 비워 둔다.

#### 구간별 주택·시설 성격

| Corridor | 주택·시설 | 주변 수목·조형물 | 밤의 역할 |
| --- | --- | --- | --- |
| Farm → Town | 외딴 농가, 작은 직판장, 작업 헛간 | 과수·목책·우물·풍차 | 농가의 따뜻한 창문은 안전 landmark, 헛간 뒤는 출몰 socket |
| Farm → Hub | 관리인 숙소, 농기계 정비 shed | 큰 수목·급수탑·바위·방향 표지 | 작업등 바깥의 바위 pocket에서 길막 무리 출현 |
| Town → Hub | 도로변 단독주택, 작은 식당·상점 | 가로수·정류장·화분·피크닉 공간 | 골목과 뒷마당이 협공 경로, 밝은 현관은 비전투 안전 구역 |
| Town → City | 외곽 연립주택, 주유·휴게 시설 | 도시형 가로수·벤치·간판·작은 기념물 | 옥상 감시와 서비스 골목 추적, 휴게소 조명은 경고 지점 |
| Hub → City | 경비 숙소, 운송업체 사무동, 정비소 | 컨테이너·팔레트·가로등·산업 조형물 | 정비소 뒤 폐자재 pocket이 scavenger 은신처 |

도로변 집은 실제 세대수·주민 위치·영업 상태를 뜻하지 않는 Presentation Composition이다. authorized aggregate 또는 별도 Simulation snapshot이 없으면 사람 수, 거주 여부와 영업 여부를 추론하지 않는다.

#### 낮과 밤의 이중 활용

| 낮 | 저녁·밤 |
| --- | --- |
| 나무가 Region 사이의 긴 빈 공간을 분절 | 수목 그림자가 몬스터 silhouette와 접근 방향을 형성 |
| 집·헛간·상점이 이동 구간의 생활감을 제공 | 일부 창문·현관등이 안전 landmark와 명도 대비를 제공 |
| 우물·벤치·정류장·기념물이 구간 정체성을 설명 | 같은 조형물이 경고 징후·은신처·카메라 focus가 됨 |
| 작은 진입로와 휴게 pocket이 NPC·차량 이동을 자연스럽게 연결 | encounter가 본선 전체를 막지 않고 pocket 주변에서 전개 |

구성 데이터는 `RoadsideClusterProfile`과 `RoadsideCompositionSetView` 같은 Presentation 설정으로 둔다. 집·나무 prefab 이름과 몬스터 spawn 위치를 운영 Domain 또는 실제 주소 계약에 넣지 않는다.

몬스터 유형은 실제 지역 주민·직업·국적·경제 상태를 암시하는 상징으로 사용하지 않는다. 전부 명확한 판타지 생물과 Simulation 역할로 표현한다.

## 6. Encounter 상태 구조

### 6.1 몬스터 무리

```text
Dormant
  ↓ Night window + encounter selected
Emerging
  ↓ truck enters approach radius
Stalking
  ↓ intercept tick
Blocking / Chasing
  ↓ outcome decided by Simulation
Looting
  ↓ assigned cargo visual acquired
Escaping
  ↓ hideout reached or dawn retreat
Resolved
  ↓ presentation cleanup
Despawned
```

### 6.2 화물트럭

```text
Cruising
  ↓ encounter warning
Alerted
  ↓ route obstruction
Slowing
  ↓ simulation outcome
Stopped / Escaping
  ↓ encounter resolved
ResumingRoute
```

`MonsterBrain`, `NavMeshAgent`, `Animator`는 이 상태를 결정하지 않는다. Simulation snapshot이 phase·target·outcome을 제공하고 Presentation이 목적지, 속도 parameter와 animation만 적용한다.

## 7. 제안 계약

```text
NightEncounterSimulationSnapshot
├─ SessionId / ScenarioId
├─ Revision / RuleRevision / Tick
├─ SourceMode = Simulation
├─ SimulatedLocalTime / Timezone
├─ DeterministicSeed
├─ CorridorStableId
├─ EncounterStableId
├─ TargetTruckJourneyStableId
├─ MonsterGroupStableId
├─ PhaseCode
├─ SpawnAnchorStableIds[]
├─ SimulatedCargoOutcome
└─ SourceLineage[]
```

`SimulatedCargoOutcome`은 최소한 다음을 구분한다.

- `NoLoss`
- `VisualScareOnly`
- `SimulatedCargoTaken`
- `TruckEscaped`
- `EncounterAbortedAtDawn`

화면의 상자 수는 이 결과에서 투영한다. 몬스터가 상자 prefab에 닿았다는 이유로 수량을 차감하지 않는다.

## 8. Presentation 구성

```text
NightRaidPresentationRoot
├─ CorridorWarningPresenter
├─ MonsterGroupPresenter
│  ├─ MonsterActorView[]
│  └─ SyntyMonsterVisualAdapter
├─ TruckEncounterPresenter
├─ CargoRaidPresenter
│  ├─ AttachedCargoViews
│  ├─ DroppedCargoViews
│  └─ CarriedCargoViews
├─ HideoutAndLandmarkPresenter
└─ NightRaidFxPresenter
```

- 외부 monster prefab 이름은 catalog의 `VisualKey` 뒤에 둔다.
- `SyntyGoblin`, `SyntyTroll` 같은 이름을 Domain·Simulation 계약으로 만들지 않는다.
- 몬스터 asset을 아직 구매하지 않았다면 capsule·기존 humanoid silhouette placeholder로 먼저 검증한다.
- 원본 prefab·material·AnimatorController는 수정하지 않고 project wrapper와 adapter를 사용한다.

## 9. 첫 Vertical Slice

첫 구현은 `Hub → City` 약 286m 구간 하나로 제한한다.

```text
GoldenDusk 시작
  → 가로등과 Dock beacon 점등
  → 경비 숙소·정비소·도로변 수목을 지나 Truck 이동
  → Truck 1대 Hub 출발
  → 컨테이너 뒤 Monster 3마리 Emerging
  → 2마리 길막 + 1마리 후방 접근
  → Truck 감속·정지
  → 상자 2개를 시각적으로 탈취
  → Monster가 폐팔레트 은신처로 도주
  → Truck 운행 재개
  → Dawn에서 흔적과 Monster 소멸
```

첫 Slice에 포함하지 않는다.

- 전투·무기·체력·사망
- 플레이어 직접 운전
- 실시간 물리 기반 화물 수량 판정
- 실제 운영 차량·기사·재고 연동
- 여러 encounter의 동시 발생
- 확률 기반 live-service 보상·결제

## 10. 단계적 구현 트랙

| 단계 | 구현 범위 | 완료 Gate |
| --- | --- | --- |
| `NMR0` 경계·기준선 | Simulation 전용 정책, Hub→City 낮/밤 고정 카메라, corridor anchor inventory | 운영 모드에서 몬스터·손실이 절대 나타나지 않음 |
| `NMR1` 계약·결정성 | encounter snapshot, phase, target, cargo outcome, seed·revision validator | 같은 seed·Tick의 결과가 동일하고 잘못된 lineage를 거부 |
| `NMR2` 도로변·은신처 Composition | 다섯 구간의 수목 군집·독립 주택·생활 landmark와 spawn/hideout/warning socket | 낮 화면에서 생활 경관으로 읽히고 Region 경계·도로·카메라를 막지 않음 |
| `NMR3` Monster adapter | placeholder 3종, Idle·Emerge·Run·Carry·Retreat animation adapter | 외부 prefab 교체가 Simulation 계약에 영향 없음 |
| `NMR4` 첫 습격 연출 | Hub→City Truck 1대, Monster 3마리, 길막·탈취·도주 | snapshot phase와 화면이 일치하고 NavMesh가 결과를 확정하지 않음 |
| `NMR5` 화물 결과 | attached·dropped·carried cargo projection과 Simulation event ledger | 화면 상자 합계가 snapshot outcome과 일치 |
| `NMR6` 구간 확장 | Farm–Town·Farm–Hub·Town–Hub·Town–City 패턴 추가 | 구간별 silhouette·행동·색·소리가 구분됨 |
| `NMR7` 카메라·성능 | encounter focus, overview marker, LOD·pooling·Android tier | Play Mode PNG·영상, 모바일 budget과 회귀 테스트 기록 |

## 11. 카메라와 UX

- World overview에서는 encounter 위치만 절제된 달 모양·발자국 marker로 표시한다.
- Truck이 warning 반경에 들어오면 카메라가 자동 이동하지 않고 사용자가 선택할 수 있는 `Encounter Focus`를 제공한다.
- 확대 카메라는 트럭, 길막 무리, 화물과 도주 방향이 한 화면에 들어오게 한다.
- 밤에도 도로 edge, Truck silhouette, Monster silhouette와 Cargo 상태가 서로 겹치지 않아야 한다.
- 몬스터 출몰을 원하지 않는 사용자를 위해 Simulation scenario 또는 접근성 설정에서 야간 위협을 비활성화한다.

## 12. 성능 기준

첫 모바일 기준은 다음과 같이 제한한다.

- 한 encounter 활성 Monster: 3~5개
- World 전체 동시 활성 Monster: 최대 12개
- 화면 밖 무리는 animation·NavMesh를 중지하고 snapshot phase만 유지
- 반복 Monster·Cargo·FX는 object pool 사용
- 추가 shadow caster는 중요 Monster 1~2개만 허용하거나 blob shadow 사용
- 가로등은 emissive·light proxy를 우선하고 실시간 shadow light를 늘리지 않음
- 원거리 은신처·몬스터는 LOD 또는 silhouette proxy 사용

## 13. 검증 계획

### 자동 검증

- Operational mode에서 encounter snapshot 적용 거부
- 시간창 이전·이후 신규 encounter 생성 거부
- 같은 seed·scenario revision·Tick의 결정성
- encounter·truck journey·corridor·spawn anchor 참조 무결성
- cargo outcome 수량이 target Simulation cargo를 초과하지 않음
- phase 전이 역행과 중복 resolved event 거부
- animation·NavMesh·collision callback에서 Simulation Tick·운영 Command를 호출하지 않음
- Synty 원본 prefab·material이 dirty하지 않음

### 시각 검증

같은 Hub→City 카메라에서 다음 Game View를 남긴다.

1. 낮의 평온한 도로
2. GoldenDusk warning
3. Monster emerging
4. Cargo looting
5. Dawn retreat

추가로 overview와 encounter focus를 각각 캡처하고 Truck·Monster·Cargo가 모바일 해상도에서도 구분되는지 확인한다.

## 14. 완료 기준

첫 야간 습격 Slice는 다음 조건을 모두 만족할 때 완료다.

1. 저녁 전에는 몬스터가 보이지 않고 조형물이 정상 경관으로 읽힌다.
2. Simulation clock과 seed로 같은 습격을 replay할 수 있다.
3. 몬스터 3마리의 매복→길막→상자 탈취→도주가 한 화면에서 이해된다.
4. Truck은 encounter 뒤 route를 재개한다.
5. 새벽에는 몬스터가 퇴각하고 잔여 FX가 정리된다.
6. 화물 결과는 Simulation snapshot과 일치하고 GameObject 충돌로 결정되지 않는다.
7. Operational mode의 실제 배송·재고·기사 상태는 변경되지 않는다.
8. Play Mode Game View 5장과 집중·회귀 테스트, 모바일 성능 결과가 남는다.

## 15. 권장 다음 작업

바로 여러 구간에 몬스터를 뿌리기보다 `NMR0 → NMR1 → NMR2` 순서로 Hub→City corridor의 시간창·snapshot·은신처 socket을 먼저 고정한다. 이후 placeholder Monster 3마리로 `NMR3~NMR4`를 연결해 장면의 재미와 가독성을 확인하고, 성공했을 때만 실제 monster asset과 나머지 네 구간으로 확장하는 것이 적절하다.
