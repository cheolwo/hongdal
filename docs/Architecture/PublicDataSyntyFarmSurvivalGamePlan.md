# 공공데이터 기반 Synty 경관 생활·농장 생존 Simulation 기획과 구현 기준

## 목표

공공데이터가 제공하는 평창의 지형·법정동·건물·도로 사실 위에, 실제 사람이나 사업체와 무관한 `SimulationScenario` 생활·생존 규칙을 겹친다. 기본 플레이는 1인칭 경관 산책, 지역 발견과 가벼운 농장 생활이며 전투는 28일 계절 장 끝의 선택 가능한 방어 사건으로 제한한다. Synty 자산은 이 상태를 표현하지만 사건·노동·재고를 확정하지 않는다.

```text
공공데이터 사실
├─ DEM·경사·수계
├─ 법정동·행정동·L2 500m 타일
├─ 건물·도로·공개 사업체
└─ 출처·기준일·근거 수준
   ↓ 공간 닻으로만 사용
Simulation Session
├─ 28일 계절 장
├─ 1~23일 경관 산책·지역 발견·선택 농사
├─ 24~26일 계절 방어 예고와 준비
├─ 27일 자동 방어·직접 전투 선택
├─ 28일 회복 가능한 결과와 계절 보고
├─ 플레이어 직접 노동
├─ NPC 위임 노동과 공동 노동력 예약
├─ 기존 r1~r3 전투 시나리오 호환
└─ Save / Replay / 세계 사건 개정
   ↓ 의미 기반 표현 자료
Unity SimulationWorldShell
├─ 1인칭 이동·직접 작업
├─ 전술 시점 노동·방어 배치
├─ PresentationKey / VisualKey
├─ 현재 보유 Synty fallback
└─ 향후 구매 Synty 팩 교체
```

## 권위와 개인정보 경계

- 공공데이터는 지형·지역·건물·사업체의 공개 사실만 제공한다.
- 실제 상호명, 주민, 사업자나 실제 건물을 감염자·약탈자·전리품·공격 대상으로 분류하지 않는다.
- 좀비, 약탈자, 아이템, 피해와 회복은 `SimulationOnly=true`, `IsOperationalState=false`인 가상 오버레이다.
- 클라이언트는 노동 대상·배우·행동·배치 방식 또는 사건·선택 고유 식별자와 예상 개정만 보낸다. 비용·피해·성공 여부는 서버가 규칙 개정과 seed로 계산한다.
- Unity의 Prefab·animation·FX는 표현일 뿐 업무나 생존 결과의 완료 증거가 아니다.

## 기본 경관 장과 계절 방어

새 Session의 기본값은 `farm-survival.scenic-season.r1`이다. 기존 `farm-survival.spring-preparation.r1~r3`은 저장·재생과 명시적 전투 시나리오를 위해 그대로 유지한다.

| 장의 날 | 기본 플레이 | 서버 상태 |
| --- | --- | --- |
| 1~23일 | 경관 산책, L2·L1 발견, 건물 정보 확인, 선택 농사 | 위협 없음, `ScenicExploration` |
| 24일 | 계절 방어 예고 | `Warning`, 응답 불가 |
| 25~26일 | 울타리·창고·조명·경계 준비 | 시설 준비도 누적 |
| 27일 | 자동 방어 또는 직접 전투 선택 | `AwaitingDefenseChoice` |
| 28일 | 선택 또는 미응답 자동 판정과 계절 보고 | `Resolved`, 회복 가능한 결과 |

- 자동 방어는 기존 시설 준비도 점수를 사용한다.
- 직접 전투를 선택한 경우에만 `AwaitingCombat`으로 전환하며 기존 1인칭 방어·카운터 판정을 재사용한다.
- 27일에 선택하지 않으면 28일에 `AutomaticDefense`가 기록되어 Session 진행이 멈추지 않는다.
- Unity는 `AwaitingCombat` 전까지 적 개체와 전투 HUD를 만들지 않고 경관·지역 정보 표현을 유지한다.
- 약탈자 사건은 기본 경관 장에서 생성하지 않으며 기존 규칙 또는 별도 탐험 시나리오에서만 사용한다.

## 기존 전투 규칙 호환: 첫 7일 수직 단위

다음 흐름은 기존 `farm-survival.spring-preparation.r1~r3`을 명시적으로 선택했을 때만 적용한다. 한 Tick은 하루이며 첫 구현의 `대관령면 봄 준비` 첫 주를 서버 원장부터 Unity 의미 키까지 관통한다.

| 날 | 플레이 내용 | 서버 권위 결과 | 주요 화면 의미 키 |
| --- | --- | --- | --- |
| 1일 | 플레이어 밭 1칸 직접 경운, NPC 밭 1칸 위임 | 체력 비용, NPC 공동 노동력 예약, 완료 Tick | `survival.day-farm`, `survival.tactical-labor` |
| 2일 | 울타리·창고 잠금·조명·경계 중 우선순위 결정 | 재료 소비, 방어 준비도 | `survival.tactical-labor` |
| 3일 | 농장 자급 식량 부족 시 외부 탐색 타로 | 안전 거점 합의와 다음 Tick 보정 | `survival.external-expedition` |
| 4일 | 인접 L2 탐색과 건물 내부 조건형 획득 | 서버 Preview·Confirm 재고 변화 | 기존 탐색·건물 정보판 의미 키 |
| 5일 | 3개체 좀비 경고와 접근 | `Warning` 뒤 r2에서 `AwaitingCombat` 전환 | `survival.zombie-warning`, `survival.combat.ready` |
| 6일 | 가상 약탈자 사절의 교환·거절·속임수 | 선택 ID를 기준으로 서버가 결과 계산 | `survival.raider-approach` |
| 7일 | 피해 확인과 수리 계획 | 복구 가능한 피해량과 주간 보고 | `survival.damage-assessment` |

현재 구현은 1·5·6·7일 규칙의 원장과 사건을 직접 포함하고, 3·4일은 기존 생존 타로·건물 내부 아이템 계약을 재사용한다. 2일의 방어 행동 계약도 포함되지만 실제 Game View 조작 UI 배선은 후속 작업이다.

## 농장 노동과 방어 규칙

`SimulationFarmSurvivalInitialStateRequest`가 지역·Area·L2 타일·농장 건물과 배우·밭 타일·방어 시설의 초기 상태를 선언한다.

- 플레이어 직접 노동은 공동 노동력을 예약하지 않고 체력을 소비하며 기본 1 Tick이 걸린다.
- NPC 위임 노동은 Settlement 공동 노동력을 예약하고 기본 2 Tick이 걸린다. 완료 후 예약을 반환한다.
- 같은 배우는 동시에 하나의 농장 작업만 수행한다.
- 울타리 수리, 창고 잠금, 조명 준비, 경계 근무는 허용되는 방어 대상과만 결합한다.
- 방어 재료·체력·공동 노동력이 부족하면 Preview에 차단 이유를 반환하고 Confirm을 거부한다.
- 위협 손실은 보급품 감소, 시설 피해, 부상으로 한정하며 영구 사망은 만들지 않는다. 피해량은 수리 작업으로 줄일 수 있다.

API는 다음과 같다.

```text
GET  /api/simulation/v1/sessions/{sessionStableId}/farm-survival
POST /api/simulation/v1/sessions/{sessionStableId}/farm-survival/work/preview
POST /api/simulation/v1/sessions/{sessionStableId}/farm-survival/work/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/farm-survival/threat-responses/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/farm-survival/combat/perspective/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/farm-survival/combat/beats/start
POST /api/simulation/v1/sessions/{sessionStableId}/farm-survival/combat/beats/{beatStableId}/react
GET  /api/simulation/v1/sessions/{sessionStableId}/world-events?afterWorldRevision=...
```

농장 작업과 위협 대응 Command는 Session Save/Replay 로그와 SHA-256 재생 해시에 들어간다. 같은 초기 상태·seed·Command 순서는 같은 노동 완료, 위협 결과와 세계 사건을 복원한다.

## 직접 전투 박자와 시점 이점

기존 `farm-survival.spring-preparation.r1`은 좀비 위협을 방어 준비도만으로 자동 판정한다. 직접 입력 수직 단위는 `farm-survival.spring-preparation.r2`에서만 활성화하며 한 번에 한 공격 박자만 처리한다.

```text
서버: 좀비 사건 AwaitingCombat
├─ 시점 확정 Command
│  ├─ FirstPersonPrecision
│  └─ ThirdPersonAwareness
├─ 전투 박자 시작 Command
│  └─ 공격 유형·충돌 시각·허용 구간 확정
└─ 반응 확정 Command
   ├─ Guard 또는 Counter
   └─ Unity가 관측한 반응 경과 ms
      ↓
서버: 등급·피해·방어 점수·경직·사건 결과 확정
```

충돌 기준은 전조 시작 뒤 1,000ms다. 1인칭 일반 방어/카운터 허용 구간은 각각 ±320/±200ms, 전술 3인칭은 ±220/±130ms다. 완벽 방어/카운터 구간은 시점과 무관하게 ±70/±45ms다. 완벽 방어는 피해 0·점수 1, 일반 방어 성공은 피해 3·점수 1, 완벽 카운터는 피해 0·점수 2·경직, 일반 카운터 성공은 피해 0·점수 1·경직으로 계산한다. 너무 빠름·늦음·미반응은 피해 10·점수 0이다. 방어 준비도와 반응 점수의 합이 2 이상이면 농장 방어에 성공한다.

Unity는 전투 시작 시 1인칭을 권장하고 기존 곡선 카메라 전환을 사용한다. 활성 박자가 없고 `AwaitingCombat` 교전이 있을 때 좌클릭은 개별 좀비 피해를 로컬에서 만들지 않고 전투 진입만 요청한다. 카메라 전환이 끝난 뒤 시점 확정과 박자 시작 Command를 차례로 보내며, 활성 박자 중에는 시점 변경·이동·농장 경영 입력을 잠그고 좌클릭 카운터, 우클릭 방어만 받는다. UI 위 포인터 입력은 전투로 전달하지 않는다. 전술 3인칭을 선택한 박자는 위협·동료·시설 인식 정보를 넓게 표시한다. Unity 명령 초안에는 판정 등급·피해·점수 필드가 없으며, 실제 HTTP 전송 뒤 반환된 상태 사본이 결과 표현의 유일한 근거다. 일시 오류 재시도는 같은 Command ID로 한 번만 수행하고, 개정 충돌은 클라이언트가 기대 개정을 고쳐 재전송하지 않고 최신 상태를 다시 조회한다.

`farm-survival.spring-preparation.r3`은 이 판정을 영웅의 전술 기회로 연결한다. `r1`과 `r2`의 기존 결과는 바꾸지 않는다.

```text
1인칭 Guard OnTime/Perfect   → Rally 품질 1/2
1인칭 Counter OnTime/Perfect → Breakthrough 품질 1/2
                              ↓ 다음 명령창까지만 유효
3인칭 전술 전환 제안
├─ 전진 공격 + Breakthrough
├─ 대형 사수 + Rally
└─ 전술 후퇴
   ↓ Confirm 뒤 다음 WorldTick
서버: 전선 위치·분대 전투력·회복 가능한 부상·시설 피해 확정
```

반응 성공 여부와 무관하게 명령창은 열리지만 성공한 행동만 전술 기회를 만든다. 기회를 만든 영웅만 해당 전선에서 사용할 수 있으며 다른 팀원은 관찰만 할 수 있다. 명령하지 않으면 다음 WorldTick에 기회가 만료되고 보너스 없는 대형 사수가 적용된다. 전술 후퇴는 NPC 부상과 전투력 손실을 피하는 대신 전선 후퇴와 시설·물자 피해를 확정한다. Unity는 전환을 강제하지 않고 안내만 표시하며 Preview·Confirm에는 명령창·전선·영웅·명령·기회 식별자와 기대 개정만 보낸다.

### 전술 분대 이동 표현

확정된 전술 결과는 별도의 이동 표현 frame을 거쳐 분대 경관에 적용한다.

```text
서버의 최신 전술 명령·판정·분대 상태 사본
└─ FarmTacticalMovementPresentationMapper
   ├─ 전진 공격 → 쐐기 대형 → 전방 기준점 → 달리기
   ├─ 대형 사수 → 선형 대형 → 외곽 기준점 → 경계
   └─ 전술 후퇴 → 종대 → 농장 안쪽 기준점 → 달리기
      ↓
분대 기준점 NavMeshAgent 1개
└─ 결정적 로컬 슬롯 최대 6개
   └─ Synty VisualRoot + 공용 animation adapter
```

화면 인원은 서버 `MemberCount`를 최대 6명까지 표시하고, 서버가 제공한 구성원 고유 식별자를 animation 위상 seed로 사용한다. 같은 구성원에게 새 명령 결과가 와도 위상을 다시 시작하지 않는다. 경로는 Synty 지형 mesh와 분리된 전술 전용 NavMesh 바닥에서 계산한다. 캐릭터마다 별도 agent를 두지 않으므로 충돌 회피가 대형을 흩뜨리지 않으며, 분대원은 기준점 아래의 선형·쐐기·종대 슬롯으로 완만하게 보간한다. 이동 중에는 진행 방향을 보고 정지하면 진영별 대치 방향으로 돌아오며, 양쪽 기준점은 캐릭터 폭보다 넓은 안전 간격을 유지한다. 현재 보유 팩에 실제 동작 clip이 없으므로 대기·달리기·경계·경직은 Humanoid wrapper의 절차형 fallback을 사용한다. 이후 animation pack을 구매하면 공용 구성 대장 연결만 교체한다.

## 팀 공동 역할 카드와 자유로운 활동 전환

사건을 해결하는 생존 타로와, 플레이어가 장착하는 역할 카드는 구분한다.

```text
팀 공동 역할 카드함
├─ 탐험 기술 카드 사본
├─ 농사 기술 카드 사본
└─ 물류 기술 카드 사본
   ↓ 서버 개정 확인 후 원격 장착 가능
구성원 장착 칸
   ↓ 활동 시작 동안 카드 잠금
현재 활동 배정
├─ Exploration
├─ FarmWork
└─ Logistics
   ↓ 서버가 현재 역할 투영
Unity 정보판·캐릭터 표현
```

- 역할 카드는 물리 아이템이 아니므로 팀원이 다른 L2 타일에 있어도 교체할 수 있다.
- 카드 사본 하나는 한 명의 한 장착 칸에만 존재하며 복제하지 않는다.
- 활동 중인 카드는 종료 전까지 옮길 수 없다.
- 한 배우는 동시에 하나의 활동만 수행한다.
- 탐험가·농부 같은 표시는 현재 활동 역할이며 영구 직업이나 서버 권한이 아니다.
- 팀 구성원·정책 개정이 바뀌면 기존 카드 상태를 그대로 사용하지 않고 새 정책과 다시 맞춘다.

API는 공동 카드함 조회, 원격 장착, 활동 시작·종료로 나뉜다.

```text
GET  /api/simulation/v1/sessions/{sessionStableId}/team-role-cards?actorStableId=...
POST /api/simulation/v1/sessions/{sessionStableId}/team-role-cards/equip
POST /api/simulation/v1/sessions/{sessionStableId}/team-role-cards/activities/start
POST /api/simulation/v1/sessions/{sessionStableId}/team-role-cards/activities/end
```

Scenario 조립부가 Session 생성 요청에 초기 카드함을 주입한다. 카드 장착·활동 시작·활동 종료는 Session aggregate의 개정과 Command 로그를 함께 올리며 저장 자료의 SHA-256 해시와 재현 대상에 포함된다. 저장 자료를 복원하면 카드 장착, 잠금, 활성 활동과 현재 역할이 같은 상태로 돌아온다.

파생 DB의 `pyeongchang-farm-hub-town-business-rules.v3`에는 카드 장착·활동 시작·활동 종료의 정적 규칙 정의만 저장한다. 현재 카드 사본의 소유·장착·활동 상태는 공간 파생 사실이 아니므로 파생 DB에 복제하지 않는다. 기본 Session 저장 구현은 아직 process-local이며 Save package를 물리 저장소에 보관하는 adapter와 실제 공동 팀 원장은 후속 작업이다.

## Unity와 Synty 구성 대장

Unity 패키지의 `FarmSurvivalVisualIntentMapper`는 서버 상태를 `VisualKey`로만 바꾼다. 자산 경로나 Prefab 이름은 계약에 저장하지 않는다.

```text
서버 ThreatType / 상태 / 개체 수
└─ FarmSurvivalVisualIntentMapper
   ├─ threat.zombie.stylized
   │  ├─ 현재 fallback: character.threat.skeleton
   │  └─ 선호 팩: POLYGON Apocalypse
   ├─ threat.raider.stylized
   │  ├─ 현재 fallback: character.generic.lowpoly
   │  └─ 선호 팩: POLYGON Apocalypse
   ├─ survival.defense.prepared
   ├─ survival.defense.damaged
   └─ survival.damage.recoverable
```

평창 경관의 현재 기본 조합은 `POLYGON Nature + Farm + Town + City` 네 팩이다. 공공데이터와 Simulation은 팩을 선택하지 않고 토지피복·시설 의미·영역 역할과 표현 키만 제공한다. Unity의 독립 경관 생성기가 마지막 단계에서 다음 비중을 해석한다.

| 영역 | Nature | Farm | Town | City |
| --- | ---: | ---: | ---: | ---: |
| World Overview | 85% | 6% | 5% | 4% |
| 대관령 Farm | 48% | 44% | 6% | 2% |
| Farm–Hub 회랑 | 35% | 30% | 20% | 15% |
| 진부 Hub | 20% | 10% | 25% | 45% |
| 평창읍 Town | 20% | 5% | 55% | 20% |

비중은 실제 토지 면적이나 Prefab 개수를 뜻하지 않는다. 세계 좌표 기반 seed와 배치 슬롯에 적용하는 경관 선택 가중치이며, 토지피복 허용 범위·경사·수계 제외가 먼저다. Nature는 산림·수변·바위·원경의 바탕, Farm은 생산 시설, Town은 생활권, City는 물류 거점의 강조층으로 사용한다.

Nature 구성 대장은 활엽수림·침엽수림·혼효림·수변 완충지·바위 절개지에 산 능선·숲 가장자리·개울 회랑을 더한 여덟 세트와 각각의 A/B/C 변형을 가진다. 총 24개 생성 Prefab은 `EnvironmentRoot / OcclusionRoot / DetailRoot / FxRoot` 구조로 원본 Prefab을 감싸며 `PresentationOnly=true`와 HLOD 가능 여부를 보존한다. 각 항목은 자연 역할, 허용 토지피복, 물 마스크 필요 여부, 원본 경사 범위, 계절·분위기, 움직임 정책, Shader 기능, 표시 거리, GPU 예산 단계와 실제 Triangle·Material Slot·Collider·Particle·Animator·LOD 측정값을 가진다. 산 능선은 DEM을 대체하지 않는 원경 실루엣이라 배치 지점의 최소 경사를 강제하지 않으며, 개울 회랑과 수변 완충지는 실제 물 마스크가 있을 때만 배치한다.

평상시 Nature는 조용한 탐색 배경이다. 봄·여름·가을·겨울 표현은 Synty 원본 Material을 수정하지 않고 `MaterialPropertyBlock` 색조와 계절별 FX 의미 키로 전환한다. 늪·무덤·해골·폐허·동굴은 기본 경관에서 제외하고 기존 서버 `PresentationKey` 다섯 종류에 대응하는 사건 전용 Overlay로 분리한다. `survival.scenic-exploration` 상태에서는 Overlay가 하나도 활성화되지 않으며, 서버 사건 상태 사본을 받은 경우에만 정확히 대응하는 하나를 표시한다. Synty 원본과 `.meta` GUID는 수정하지 않는다.

구매·적용 순서는 다음으로 유지한다.

1. `Animation Base Locomotion`: 1인칭·3인칭 캐릭터 보행·대기·기초 동작의 공통 기반
2. `POLYGON Apocalypse`: 로우폴리 좀비·생존자·방어 시설·폐허 소품
3. `POLYGON Alpine Mountain`: 평창 산악 실루엣·침엽수·바위·설경 보강

현재 코드는 보유한 Nature·Farm·Town·City를 평창 경관 대장 v2에 연결한다. Generic·Starter는 필요한 호환 fallback으로만 남긴다. 향후 Apocalypse·Alpine 또는 animation 팩을 추가해도 공공데이터·Simulation 계약은 바꾸지 않고 구성 대장의 의미 키 연결만 교체한다.

## 시점과 성능

- 1인칭은 WASD·마우스 이동, 직접 노동·탐색·방어 상호작용을 담당한다.
- 전술 시점은 노동자·작업·방어 시설의 선택과 우선순위를 담당한다.
- 시점 전환은 표현 상태이며 WorldTick이나 Session 개정을 만들지 않는다.
- 현재 L2 500m Profile은 `3×3 상세 / 5×5 활성 / 9×9 준비`, 동시 로드 4개를 유지한다.
- 첫 위협 예산은 한 사건 3~5개체, 월드 활성 12개체 이하이며 서버의 `ThreatUnitCount`를 Unity가 임의로 늘리지 않는다.
- 초기 목표는 Windows PC 1080p 60fps다. 실제 Prefab 연결 후 Triangle, Material Slot, Draw Call, Shadow Caster, Collider, Animator와 HLOD를 측정해 Profile 개정을 분리한다.

## 현재 구현 증거와 남은 작업

완료된 범위:

- 기본 `scenic-season.r1`의 1~23일 평온 구간, 24일 예고, 27일 선택, 28일 자동 판정과 계절 보고
- 방어 선택 가능 ID·마감 Tick, 미응답 자동 방어와 Save/Replay 해시 검증
- 경관 규칙에서 직접 전투 선택 전 적 개체·전투 HUD를 숨기는 Unity 표현 의도
- 플레이어/NPC 농장 노동 Preview·Confirm·Tick 완료
- Settlement 공동 노동력 예약·반환
- 농장 방어 준비와 복구 가능한 피해
- 5일 좀비 경고, 6일 서버 판정과 약탈자 선택, 7일 피해 보고
- 개정 기반 세계 사건 Projection
- 농장 Command Save/Replay와 해시 검증
- Unity VisualKey·fallback·선호 팩 구성 대장과 표현 의도 변환
- r2 단일 전투 박자, 시점별 일반 허용 구간, 서버 판정과 WorldTick 만료
- 전투 Command 저장·재생과 농장 방어 규칙/UI 기획 대장 연결
- Unity 전투 표현 투영, 반응 명령 초안과 기존 플레이어 입력 잠금 연결 코드
- Input System의 `Attack`·`Defend` 의미 입력, 기존 Simulation 전투 HTTP 경로 adapter와 마지막 서버 상태 보존
- `SimulationWorldShell`의 전투 입력·서버 조정 Composition Root 저장 배선
- r3 영웅 전술 기회, 전진 공격·대형 사수·전술 후퇴와 다음 WorldTick 판정
- 전술 명령 Save/Replay, 업무 규칙 대장 v6와 전술 명령판 UI 기획 v5
- Unity 전술 시점 전환 제안과 결과 수치가 없는 Preview·Confirm 명령 초안
- Unity 서버 결과 기반 6대6 분대 표시, 선형·쐐기·종대 전환과 NavMesh 이동
- Synty wrapper의 결정적 절차형 대기·달리기·경계·경직 표현
- 평창 5개 영역의 Nature·Farm·Town·City 결정적 구성 비중 Profile
- Nature 8개 경관 세트 × A/B/C, 총 24개 표현 전용 조합 Prefab과 HLOD 후보·렌더링 비용 대장
- 대관령 L2 산림 전이 구획의 활엽·침엽·혼효·산 능선·숲 가장자리 조합과 평창 경관 Visual Catalog v2 연결
- 사계절 `MaterialPropertyBlock`·FX 전환과 평온 탐색에서 비활성인 서버 사건 키 기반 Nature Overlay 5종

아직 완료하지 않은 범위:

- 실행 중인 Simulation 서버를 사용한 실제 HTTP 왕복과 사람이 조작한 타이밍 판정 증거
- 전술 명령판의 실제 uGUI 버튼과 HTTP 응답 adapter
- 밭 경운·NPC 작업·좀비·약탈자 animation 연결
- Synty Apocalypse·Alpine 등 추가 팩 구매와 의미 키 연결
- 경관 산책·계절 방어 5종 Game View와 사람이 직접 조작한 입력 증거
- 멀티플레이 네트워크 동기화

화면이 달라지는 후속 작업의 완료 조건은 평온한 경관 산책, 지역 발견, 계절 방어 예고, 직접 전투 선택, 계절 보고의 Game View 다섯 장과 PlayMode 입력 검증이다. 이번 변경은 계약·도메인·Unity 표현 모델 자동 시험까지만 포함한다.

## Synty 공식 제품 참고

- [Animation Base Locomotion](https://syntystore.com/en-gb/products/animation-base-locomotion)
- [POLYGON Apocalypse](https://syntystore.com/products/polygon-apocalypse-pack)
- [POLYGON Alpine Mountain](https://syntystore.com/products/polygon-alpine-mountain-nature-biomes)
