# 공공데이터 기반 Synty 농장 생존 Simulation 기획과 구현 기준

## 목표

공공데이터가 제공하는 평창의 지형·법정동·건물·도로 사실 위에, 실제 사람이나 사업체와 무관한 `SimulationScenario` 생존 규칙을 겹친다. 플레이어는 1인칭으로 농장을 일구고 탐색·방어하며, 전술 시점에서는 NPC 노동과 방어 우선순위를 배치한다. Synty 자산은 이 상태를 표현하지만 사건·노동·재고를 확정하지 않는다.

```text
공공데이터 사실
├─ DEM·경사·수계
├─ 법정동·행정동·L2 500m 타일
├─ 건물·도로·공개 사업체
└─ 출처·기준일·근거 수준
   ↓ 공간 닻으로만 사용
Simulation Session
├─ 28일 계절 장
├─ 플레이어 직접 노동
├─ NPC 위임 노동과 공동 노동력 예약
├─ 농장 방어 준비
├─ 좀비 환경 압력
├─ 가상 약탈자 세력 선택 사건
├─ 회복 가능한 손실·수리
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

## 28일 계절 장과 첫 7일 수직 단위

한 Tick은 하루이며 28 Tick을 한 계절 장으로 본다. 첫 구현은 `대관령면 봄 준비`의 첫 주를 서버 원장부터 Unity 의미 키까지 관통한다.

| 날 | 플레이 내용 | 서버 권위 결과 | 주요 화면 의미 키 |
| --- | --- | --- | --- |
| 1일 | 플레이어 밭 1칸 직접 경운, NPC 밭 1칸 위임 | 체력 비용, NPC 공동 노동력 예약, 완료 Tick | `survival.day-farm`, `survival.tactical-labor` |
| 2일 | 울타리·창고 잠금·조명·경계 중 우선순위 결정 | 재료 소비, 방어 준비도 | `survival.tactical-labor` |
| 3일 | 농장 자급 식량 부족 시 외부 탐색 타로 | 안전 거점 합의와 다음 Tick 보정 | `survival.external-expedition` |
| 4일 | 인접 L2 탐색과 건물 내부 조건형 획득 | 서버 Preview·Confirm 재고 변화 | 기존 탐색·건물 정보판 의미 키 |
| 5일 | 3개체 좀비 경고와 접근 | `Warning` 세계 사건, 이후 방어 점수 판정 | `survival.zombie-warning` |
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
GET  /api/simulation/v1/sessions/{sessionStableId}/world-events?afterWorldRevision=...
```

농장 작업과 위협 대응 Command는 Session Save/Replay 로그와 SHA-256 재생 해시에 들어간다. 같은 초기 상태·seed·Command 순서는 같은 노동 완료, 위협 결과와 세계 사건을 복원한다.

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

구매·적용 순서는 다음으로 유지한다.

1. `Animation Base Locomotion`: 1인칭·3인칭 캐릭터 보행·대기·기초 동작의 공통 기반
2. `POLYGON Apocalypse`: 로우폴리 좀비·생존자·방어 시설·폐허 소품
3. `POLYGON Alpine Mountain`: 평창 산악 실루엣·침엽수·바위·설경 보강

현재 코드는 팩을 구매하거나 원본을 수정하지 않는다. 보유한 Farm·Town·City·Generic·Starter 자산으로 fallback을 유지해 기능을 먼저 검증하고, 팩이 추가되면 구성 대장의 연결만 바꾼다.

## 시점과 성능

- 1인칭은 WASD·마우스 이동, 직접 노동·탐색·방어 상호작용을 담당한다.
- 전술 시점은 노동자·작업·방어 시설의 선택과 우선순위를 담당한다.
- 시점 전환은 표현 상태이며 WorldTick이나 Session 개정을 만들지 않는다.
- 현재 L2 500m Profile은 `3×3 상세 / 5×5 활성 / 9×9 준비`, 동시 로드 4개를 유지한다.
- 첫 위협 예산은 한 사건 3~5개체, 월드 활성 12개체 이하이며 서버의 `ThreatUnitCount`를 Unity가 임의로 늘리지 않는다.
- 초기 목표는 Windows PC 1080p 60fps다. 실제 Prefab 연결 후 Triangle, Material Slot, Draw Call, Shadow Caster, Collider, Animator와 HLOD를 측정해 Profile 개정을 분리한다.

## 현재 구현 증거와 남은 작업

완료된 범위:

- 플레이어/NPC 농장 노동 Preview·Confirm·Tick 완료
- Settlement 공동 노동력 예약·반환
- 농장 방어 준비와 복구 가능한 피해
- 5일 좀비 경고, 6일 서버 판정과 약탈자 선택, 7일 피해 보고
- 개정 기반 세계 사건 Projection
- 농장 Command Save/Replay와 해시 검증
- Unity VisualKey·fallback·선호 팩 구성 대장과 표현 의도 변환

아직 완료하지 않은 범위:

- 실제 HTTP adapter와 `SimulationWorldShell` 정보판·버튼 배선
- 밭 경운·NPC 작업·좀비·약탈자 animation 연결
- Synty Apocalypse·Alpine 구매와 Prefab 연결
- 실제 Scene 배치, PlayMode 상호작용, 5종 Game View 증거
- 멀티플레이 네트워크 동기화와 28일 전체 계절 사건

화면이 달라지는 후속 작업의 완료 조건은 낮 농장, 전술 노동 배치, 해질녘 방어 준비, 밤 좀비 접근, 새벽 피해 확인의 Game View 다섯 장과 PlayMode 입력 검증이다.

## Synty 공식 제품 참고

- [Animation Base Locomotion](https://syntystore.com/en-gb/products/animation-base-locomotion)
- [POLYGON Apocalypse](https://syntystore.com/products/polygon-apocalypse-pack)
- [POLYGON Alpine Mountain](https://syntystore.com/products/polygon-alpine-mountain-nature-biomes)
