# 공간·Simulation 규칙 객체 표현 결합 원장

## 한 문장 요약

공공데이터에서 만든 공간 객체와 아직 발전 중인 Simulation 규칙을 곧바로 Prefab에 연결하지 않고, 개정 가능한 규칙 Metadata와 `객체표현결합규칙`에서 만나게 한 뒤 그 이유를 불변 `객체표현해석결과`로 저장한다.

```text
공간 실행본·공간 규칙 Metadata
            +
Simulation 상태·Simulation 규칙 Metadata
            ↓
      객체 표현 결합 규칙
            ↓
      객체 표현 해석 실행
       ├─ 기본구성키
       └─ 동적표현의도묶음키
            ↓
Synty 경관 Job + Runtime URP 표현 Pipeline
```

## 규칙이 아직 확정되지 않았을 때

규칙 상태는 `Draft / Active / Retired`로 구분한다. `Draft` Simulation 규칙과 이를 참조하는 `Draft` 결합 규칙은 DB에 축적할 수 있지만 실제 해석에서는 선택하지 않는다. 대신 활성 공간 규칙만 참조하는 결합 규칙이 기본 외형을 만든다.

예를 들어 관측된 물류 창고는 상차 Simulation 규칙이 미정이어도 `composition.logistics-warehouse.observed.v1` 기본 구성을 받을 수 있다. 상차 규칙이 검증되어 `Active`가 되면 더 높은 우선순위의 결합 규칙이 `intent-bundle.warehouse.loading-active.v1`을 추가한다. 공간 실행본은 다시 만들지 않는다.

## 물리 테이블

| 테이블 | 책임 |
| --- | --- |
| `시뮬레이션월드_객체표현규칙대장` | 대장 개정과 전체 규칙 hash |
| `시뮬레이션월드_공간규칙Metadata` | 토지피복·경사·수계·도로·건물 용도·Area 역할 같은 공간 조건 |
| `시뮬레이션월드_Simulation규칙Metadata` | 운송·상차·대기·혼잡 같은 상태 조건과 확정 단계 |
| `시뮬레이션월드_객체표현결합규칙` | 두 규칙을 객체 의미·범위·우선순위·표현 키로 연결하는 중심표 |
| `시뮬레이션월드_객체표현해석실행` | 공간 실행 hash, 선택적 Simulation 세션·개정·WorldTick, 입출력 hash |
| `시뮬레이션월드_객체표현해석결과` | 객체별 적용 규칙과 기본 구성·동적 의도·미충족 처리·근거 |

`Metadata`는 규칙의 설명과 버전을 보존하고, 해석 결과는 특정 공간 실행과 상태에서 어떤 규칙이 실제 선택되었는지를 보존한다. 같은 개정 번호나 해석 실행 식별자에 다른 hash를 덮어쓰면 거부한다.

## 중심 결합 규칙

`SimulationWorld객체표현결합규칙`은 다음을 가진다.

```text
StableId + Revision + StatusCode
ObjectSemanticCode + ScopeCode
SpatialRuleStableId + SpatialRuleRevision
SimulationRuleStableId? + SimulationRuleRevision?
SimulationRuleRequired
MinimumEvidenceKindCode
DefaultCompositionKey
DynamicIntentBundleKey?
UnmetRuleHandlingCode
Priority
PresentationOnly = true
```

`DefaultCompositionKey`와 `DynamicIntentBundleKey`는 의미 기반 키다. `Assets/...`, `.prefab` 같은 Unity 경로는 검증에서 거부한다. 실제 Synty Prefab과 URP 수치는 Unity 구성 대장이 마지막에 해석한다.

## 실행 순서

1. 공공데이터 공간 Pipeline이 공간 실행 ID·출력 SHA-256과 node를 만든다.
   평창군 첫 적용에서는 공유 DB 전체를 보존하면서 건물 용도 Category별 대표 건물을 하나씩만 표현 node로 투영한다. 각 node의 `대표원본건수`는 이 대표가 대신 보여 주는 같은 종류의 원본 규모다.
2. 공간 판정기가 node마다 일치한 공간 규칙 ID를 제출한다.
3. Simulation 상태가 있으면 상태 Projector가 일치한 Simulation 규칙 ID와 세션 개정·WorldTick을 제출한다.
4. `SimulationWorld객체표현해석JobShell`이 공간 실행 hash와 실제 node 존재를 검증한다.
5. 활성 결합 규칙 중 우선순위가 가장 높은 규칙을 선택한다. 동률은 고유 식별자 순으로 결정한다.
6. 객체별 선택 이유와 표현 키를 불변 해석 실행본으로 저장한다.
7. 기본 구성 키는 Synty 경관 Job에, 동적 의도 묶음 키는 Runtime 렌더링 의도 Pipeline에 전달한다.

현재 첫 검증 Fixture는 `관측 물류 창고 + 선택적 상차 상태`다. 실제 평창군 37,383개 건물의 공간 규칙 판정과 실제 Simulation 세션 연결은 후속 작업이며, 아직 이 Fixture가 현실 창고 업무 규칙을 확정한 것은 아니다.

추가 최소 Demo는 종류별 대표 건물에 고정 seed로 `Idle / Operating / Loading / Maintenance` 중 하나를 배정한다. 이 값은 실제 회사 활동 관측이 아니라 `ScenarioFixtureBuildingActivity`이며, 건물 종류 공간 규칙과 결합해 `composition.building.<종류>.representative.v1`과 동적 표현 의도 묶음 키를 만든다.

## 권위와 안전 경계

- 공간 규칙은 공공데이터 원본을 수정하지 않는다.
- 객체 표현 해석은 Simulation 상태·WorldTick·개정을 변경하지 않는다.
- 초안 규칙은 저장할 수 있지만 활성 표현을 만들지 않는다.
- 해석 결과와 Synty·URP 출력은 모두 `PresentationOnly=true`다.
- 애니메이션이나 Particle 완료는 업무 완료가 아니다.
- Prefab·Material을 바꿔도 공간·Simulation 규칙 식별자와 해석 계보는 유지된다.
