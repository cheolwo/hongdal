# 병렬 경영–전투 인스턴스 구조

## 목적

한 팀원이 전투를 수행하는 동안 다른 팀원은 같은 Simulation 세션의 농장·창고·정착지 경영을 계속한다. 전투는 별도 운영 서버나 별도 영속 세계가 아니라, 서버가 권위를 갖는 Simulation 전용 하위 인스턴스다. Unity는 받은 상태 사본에 따라 경영·전투 지휘·전투 관찰 관점을 표현할 뿐 전투 결과나 자원 이동을 계산하지 않는다.

## 현재 구현 트리

```text
Simulation Session
├─ 세계 진행 — WorldTick / WorldRevision
│  ├─ 농장 노동
│  ├─ 창고·세계 재고
│  └─ 경영 참가자: 전투 중에도 계속 명령 가능
│
└─ BattleInstance — BattleTick / BattleRevision
   ├─ 근거 사건: 기존 FarmSurvival 위협 사건
   ├─ 전투 영역·초기 병력·증원 후보
   ├─ 참가 형태
   │  ├─ Commander: 배치·전투 명령
   │  ├─ DelegatedSquad: 위임 분대 조작
   │  └─ Spectator: 관찰 전용, 세계 상태 조작 불가
   ├─ 경영 측 제한 지원
   │  ├─ SupplyCrate: 세계 재고 자원 예약
   │  └─ ReinforcementSquad: 후보 NPC 예약
   ├─ 결정적 BattleTick 진행·무인 자동 판정
   ├─ 전투 결과·재생 SHA-256 생성
   └─ 다음 안전한 WorldTick
      └─ 결과 Reconcile + 예약 자원 해제
```

`WorldTick`과 `BattleTick`은 같은 숫자나 개정을 공유하지 않는다. 전투 생성 뒤 경영 세계가 진행되어도 전투 개정은 바뀌지 않으며, 전투 명령이 진행되어도 세계 개정은 바뀌지 않는다. 전투 완료 결과는 즉시 세계 상태를 덮어쓰지 않고 `Pending`으로 남았다가 다음 `WorldTick`에 `Applied`로 전환된다.

## 자원 중복 사용 방지

전투 영역·건물·분대·지원 물자는 전투 저장소에서 예약한다. 같은 자원을 다른 활성 전투가 다시 예약할 수 없고, 예약된 보급 상자는 세계 재고 획득에서, 예약된 증원 NPC는 농장 노동에서 `BattleResourceLocked`로 거절한다.

활성 잠금은 Simulation 서버 프로세스의 공유 메모리 저장소를 기준으로 한다. 명시적 Session Save에는 전투 생성 근거·상태·예약·재생 사건·멱등 명령 결과와 전투별 무결성 SHA-256을 함께 넣으며, Session DB가 활성화되어 있으면 기존 `simulation-save.v1` JSON 열에 보존한다. Restore는 세계 Command를 재생하고 전투 기록의 패키지 hash·전투 무결성 hash·자체 재생 hash가 모두 맞을 때 활성 전투와 예약을 다시 만든다. 자동 저장·자동 재시작 복원과 다중 서버 인스턴스 분산 잠금은 아직 보장하지 않는다.

## Unity 표현 경계

```text
서버 BattleInstance 상태 사본
└─ 엔진 독립 표현 변환
   ├─ 비참가 팀원 → Management
   ├─ Commander / Deploying → TacticalThirdPerson
   ├─ Commander / Active → FirstPerson
   ├─ Spectator → Follow, 조작 불가
   └─ Reconciled → Management 복귀
```

경영 참가자의 지원 요청 초안에는 안정 식별자와 예상 개정만 들어간다. Unity가 지원 보너스·승패·자원 수량을 계산해 보내지 않는다. 현재 구현은 `Ssalddel.Unity`의 엔진 독립 표현 계층과 단위 시험까지이며, 실제 Unity 프로젝트 HTTP 저장소·Scene 배선·Play Mode·Game View는 후속 단계다.

## HTTP 경계

```text
GET  /api/simulation/v1/sessions/{sessionStableId}/battles
GET  /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}
POST /api/simulation/v1/sessions/{sessionStableId}/battles/previews
POST /api/simulation/v1/sessions/{sessionStableId}/battles/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/participants/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/deployments/preview
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/deployments/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/support-previews
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/supports/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/ticks
```

모든 경로는 `Simulation` 실행 모드 전용이다. 참가·배치·지원·전투 진행은 `CommandId`와 예상 개정으로 멱등성과 동시성 충돌을 확인한다.

## 현재 완료 범위와 다음 경계

현재 완료한 수직 단위는 전투 생성 Preview/Confirm, 참가 역할, 배치, 보급·증원 지원, 독립 BattleTick, 결정적 자동 판정, 재생 해시, 다음 WorldTick 결과 합류, 자원 잠금, Session Save/Restore와 Unity 표현 모델이다. 전투가 없는 기존 `simulation-save.v1`은 전투 개수 0을 hash 입력에 새로 넣지 않아 이전 해시와 호환된다.

다음 구현은 아래 순서가 적합하다.

1. 전투 결과가 시설 내구도·재고·사기·부상처럼 명시적인 세계 효과로 반영되는 `BattleOutcomeEffect`를 추가한다.
2. 재접속 유예·참가자 이탈·서버 주도 BattleTick 실행기를 추가한다.
3. 활성 전투 자동 저장·시작 시 복원과 다중 인스턴스용 분산 자원 예약을 추가한다.
4. 기존 영웅 전투 반응과 전술 명령을 `BattleStableId` 하위 명령으로 일원화한다.
5. 그 뒤에만 실제 Unity HTTP 연결과 단일 `SimulationWorldShell` 배선을 수행한다.

지금 단계에서는 실제 운영 권한·로그인·조직 HR을 조회하지 않고 Simulation 팀 정책만 사용한다. 실제 물류·재고·인력·시설 상태를 변경하지 않는다.
