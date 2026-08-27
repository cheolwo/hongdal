# Simulation NPC 조직·업무 행동 규칙

## 목적

이 구조는 운영 사용자의 로그인 세션이나 실제 HR 권한을 확인하는 기능이 아니다. Simulation 안의 NPC가 조직, 역량, 위임과 업무 정책에 따라 결정적으로 행동하도록 만드는 가상 규칙이다.

첫 수직 단위는 `진부면 물류 거점 입고검수 → 재고 적재`다. 화물이 물류 거점에 도착하면 입고검수 작업이 생성되고, Simulation이 적격 NPC를 배정해 `배정 → 이동 → 작업 → 완료`를 `WorldTick`으로 진행한다. 검수 완료 뒤의 `보관 가능(StorageEligible)`은 적재 대기이며, 같은 재고 ID를 대상으로 적재 NPC 작업이 끝나야 `적재 완료(PutAwayCompleted)`가 된다.

운영 유래 WI의 플레이어·NPC 통제 분류와 첫 Hub 내부 출고 준비 세로 조각은 [NPC 루틴 WI 통제 정책](NPC루틴WI통제정책.md)이 소유한다. 이 문서는 NPC 조직·역량·배정 규칙을 소유하고, 통제 정책을 중복 정의하지 않는다.

## 권위와 표현 경계

```text
Simulation 시나리오
└─ NPC 조직·행위자·역량·위임·업무 정책
   └─ 결정적 작업 배정
      └─ WorldTick 행동 단계
         ├─ 업무 기록·시설 재고 상태  ← Simulation 권위
         └─ NPC 업무 행동 Projection
            └─ Unity 이동·Idle/Walk     ← PresentationOnly
```

- 운영 서버의 사용자, 로그인, 조직 소속, HR 권한과 API 인가를 복제하지 않는다.
- NPC 고유 식별자는 `actor:sim:*`, 조직은 `organization:sim:*`처럼 Simulation 전용으로 둔다.
- Unity의 캐릭터 Prefab, 위치 보간과 애니메이션은 업무 완료를 확정하지 않는다.
- Unity는 서버 상태 사본의 `WorldTick`, `Revision`, 행동 단계와 상호작용 지점만 읽는다.

## 핵심 계약

| 계약 | 책임 |
| --- | --- |
| `SimulationNpcOrganizationSnapshot` | 시나리오 조직과 시설 범위 |
| `SimulationNpcActorSnapshot` | NPC의 조직·시설·기술·활성 상태 |
| `SimulationNpcCapabilityGrantSnapshot` | 누가 어떤 역량을 행사·위임할 수 있는지 |
| `SimulationNpcWorkPolicySnapshot` | 행동 코드별 자동 배정·이동·작업 시간·우선순위 |
| `SimulationNpcTaskAssignmentSnapshot` | 작업과 담당 NPC, 행동 단계, 일정 |
| `SimulationNpcWorkRecordSnapshot` | 실제 Simulation 안에서 완료된 업무 기록 |
| `SimulationNpcActionProjection` | Unity가 읽는 표현 전용 행동 상태 사본 |
| `SimulationNpcFacilityInventorySnapshot` | 검수 대기·적재 대기·적재 완료 시설 재고 상태 |

업무 정책 변경은 `POST /api/simulation/v1/sessions/{sessionStableId}/npc-policies`에서 명시적 Command로 처리한다. 같은 `CommandId` 재시도는 멱등하고, 다른 내용의 재사용은 거부한다.

## 배정과 위임 규칙

1. 업무의 조직·시설·필요 역량 범위를 만족하는 활성 NPC만 후보가 된다.
2. 선호 담당자, 현재 작업량, 기술 수준, NPC 고유 식별자 순으로 결정적으로 정렬한다.
3. 적격자가 부족하고 자동 위임이 허용되면, 초기 시나리오에서 위임 권한을 받은 관리자가 같은 조직·시설의 NPC 한 명에게 필요한 역량을 위임한다.
4. 자동 위임으로 생긴 권한은 재위임할 수 없다.
5. 자동화가 꺼졌거나 적격자가 없으면 작업을 없애지 않고 `Blocked`로 남긴다.
6. 정책이나 역량 조건이 바뀐 다음 Tick에 차단된 작업을 다시 평가한다.

사용자는 NPC 하나를 직접 조종해 완료를 만들지 않고, 자동 배정·자동 위임·선호 담당자·우선순위 같은 정책을 바꾸는 방식으로 개입한다.

## 첫 Fixture

```text
진부 물류 조직
├─ Hub 관리자
│  └─ 인력 위임 가능
├─ 입고 담당자
│  └─ 입고검수 역량 보유
└─ 물류 보조
   ├─ 적재 역량과 초기 부여 보유
   └─ 입고검수는 적체 조건에서만 동적 위임

도착 화물 1
└─ 입고 담당자 배정 → 이동 1 Tick → 검수 2 Tick → 적재 대기
   └─ 물류 보조 배정 → 이동 1 Tick → 적재 2 Tick → 적재 완료

도착 화물 2
└─ 업무 적체 조건 충족 → 관리자가 물류 보조에게 역량 위임 → 병렬 배정
```

Fixture는 실제 진부면 사업장 인력이나 직무 관측이 아니라 `Scenario` 근거다.

## 저장·재생과 재현성

- 조직, 행위자, 역량, 위임, 정책, 배정, 업무 기록과 시설 재고를 Session 상태 사본과 save hash에 포함한다.
- 정책 변경 Command도 replay log에 포함한다.
- 같은 초기 상태, seed, 정책, Command와 Tick 순서는 같은 담당자·단계·hash를 만든다.
- 기존 save에 NPC 상태가 없으면 빈 확장으로 취급해 이전 hash 호환을 유지한다.

## 다음 확장 순서

1. 진부 Hub 내부 출고 준비(`WI-HUB-03~05`, 차량 상차 제외)
2. Farm 수확·포장
3. 음식점 조리·픽업 인계
4. 마트 입고·진열
5. 운송 기사 상차·이동·하차

각 확장은 같은 조직·역량·정책·행동 단계 계약을 재사용하고, 실제 운영 권한이나 외부 효과가 필요해지는 시점에만 별도 운영 Command 경계를 추가한다.
