# 플레이 폐루프 Synty 표현 모듈 체계

## 기준

Synty 자산의 사용 단위는 52개 의미군의 A/B/C 변형이 아니라 `PlayableUnit`이다.
각 폐루프는 플레이어가 경험하는 진입·선택·진행·성공·실패와 회복·귀환을 기준으로
필요한 실내외 표현 역할과 자산 계열을 선언한다.

```text
PlayableLoop
└─ WI와 권위 상태
   └─ 플레이 순간
      └─ 배치 역할
         └─ Synty 기능군
            └─ 세부 기능군
               └─ 자산 종류
                  └─ Synty 자산 계열
                     └─ 결정적 Prefab 선택
```

이 계층은 표현 전용이다. 자산 선택과 GameObject 생성은 `WorldRevision`, WI 결과,
Simulation 저장 상태 또는 H 공간 의미를 변경하지 않는다.

## 모듈 경계

`eng/execution-ledgers/playable-loop-synty-expression-modules.json`이 폐루프와 Synty
자산 계열 연결의 기준 대장이다. 모듈은 다음을 가진다.

- `loopStableId`와 포함 WI
- 폐루프가 요구하는 H 공간 능력
- 진입·선택·진행·성공·실패 회복·귀환의 표현 슬롯
- 실외 기반, 기능 객체, 상태 덧입힘, 실내 설비·소품, Actor·FX 역할
- Prefab 경로가 아닌 `assetFamilyId` 후보
- 공유 환경·건설 상태·실내·대기 모듈 참조

Unity는 같은 대장의 의미를 `플레이폐루프Synty표현Module`과
`플레이폐루프Synty표현Resolver`로 소비한다. 후보 수를 세 개로 강제하지 않는다.
적합한 후보가 하나뿐이면 하나를 사용하고 후보가 없으면 억지로 대체하지 않고
검증 차단 또는 명시적 보류로 남긴다.

## 사람이 읽는 자산 기능 분류

Synty 분류는 게임 세계의 권위 분류가 아니라 표현 자산을 찾고 검토하기 위한 체계다.
설계·문서·생성 요약에서는 한국어 이름을 먼저 쓰고, 저장·폐루프 연결에서는 기존 영문
Stable Code를 유지한다.

```text
Synty 자산 기능 체계
├─ 실외 표현
├─ 실내 표현
└─ 공통 표현

범위 → 기능군 → 세부 기능군 → 자산 종류 → 자산 계열 → 실제 Prefab
```

예를 들어 `실내 표현 → 실내 설비 → 보관 설비 → 선반 →
synty-family:town:props:shelf → 실제 Prefab`으로 읽는다. `Interior`,
`interior-fixture`, `storage-fixture`, `shelf`는 저장과 호환을 위한 Stable Code이며
사람에게 먼저 노출하는 명칭은 각각 `실내 표현`, `실내 설비`, `보관 설비`, `선반`이다.

전체 한국어 트리의 단일 원본은
`eng/execution-ledgers/synty-asset-human-taxonomy.json`이다. 이 JSON의 새 분류 필드는
`범위Code`, `범위이름`, `기능군Code`, `기능군이름`, `세부기능군Code`,
`세부기능군이름`, `자산종류Code`, `자산종류이름`처럼 한국어로 쓴다. 값으로 쓰는
Stable Code와 기존 `moduleCode`·`assetFamilyId`·Prefab GUID는 바꾸지 않는다.

## 7팩 전수 기능군 대장

팩 이름은 자산 출처이고 게임 기능 모듈은 아니다. Unity의
`Synty전체자산ModuleCatalog`은 Nature·Farm·Town·City·Construction·Generic·Starter
7팩의 Prefab `2,899`개를 다음 12개 기능군으로 분류한다.

- 월드 지면, 자연 식생, 실외 구조물, 실외 기능 소품
- 도로·통행망, 영역 전이, 건설·복구 상태
- 실내 구조, 실내 설비, 실내 소품
- 인물·차량·도구, 세계 피드백 효과

한 Prefab은 여러 기능군에 속할 수 있다. 자동 분류 결과는 배치 승인이 아니며
`production-ready`, `needs-review`, `shared-base`, `prototype-fallback`,
`reserved-for-future-loop`, `excluded` 수명주기로 따로 관리한다. 기능 모듈이 없으면
반드시 보류 이유가 있어야 한다.

Construction은 AreaSet이 아니라 모든 영역이 사용할 수 있는 건설·복구 상태 계층이다.
Generic은 공유 기반, Starter는 prototype fallback으로만 취급한다. 현재 자동 전수
결과는 `174`개를 명시적 보류로 남겼고 나머지는 하나 이상의 기능 모듈을 가진다.

호환 기능군과 팩 정책의 관리 기준은
`eng/execution-ledgers/synty-asset-functional-modules.json`, Unity 실제
대장은 `Synty전체자산ModuleCatalog.asset`, 공개 수량 요약은 Unity 프로젝트의
`Documentation/Generated/Synty전체자산ModuleCatalog.md`다.

## 기존 156개 A/B/C의 상태

기존 52개 의미군 × A/B/C는 `LegacyGenerated`다.

- 신규 작업의 기준이나 완전성 지표로 사용하지 않는다.
- 신규 A/B/C 생성과 세 변형 강제를 중단한다.
- 기존 Scene·모판·저장 호환을 읽는 Legacy 입력으로만 유지한다.
- 연결구, 반복 제한, Bounds, 경사, 통행처럼 실제 검증 가치가 있는 규칙은 새 모듈과
  배치 검증 규칙으로 옮긴다.
- 활성 폐루프·공식 Scene·WI 모판의 참조가 0이고 호환 시험이 통과한 생성 Prefab만
  마지막에 제거한다.

`CompositionKey`가 존재한다는 사실은 E5 공간 발현이나 E7 플레이 증거가 아니다.

## 첫 적용

첫 모듈은 Nature 핵심 PlayableUnit 네 개다.

1. 도끼·벌목·오두막 기초
2. 황혼 위협·대응·귀환
3. 보관·수면·새벽·Day2 계획
4. 작업대 건설·취소·다음 선택

Construction 팩은 독립 AreaSet이 아니라 건설 중·취소·복구 상태를 보여 주는 공유
상태 계층이다. Generic은 별도 전수 대장 편입 후 공통 골격 후보로 사용할 수 있고,
Starter는 제품 표현에 자동 채택하지 않는 시험용 대체 자산으로 유지한다.

## 결정성과 검증

Prefab 후보 선택은 다음 입력만 사용한다.

```text
WorldSeed
+ PlacementStableId
+ ModuleRevision
+ AssetModuleRevision
+ SlotStableId
+ AuthorityStateCode
```

후보 자산 계열과 계열 내부 Prefab은 Stable ID로 정렬한다. Unity Instance ID, 현재
시간, 배열 입력 순서에는 의존하지 않는다.

완료 기준은 자산 수가 아니라 다음과 같다.

- 모든 WI가 하나 이상의 표현 슬롯으로 추적된다.
- 상태가 다르면 필요한 표현 차이를 읽을 수 있다.
- 같은 입력은 같은 자산 계열과 Prefab을 선택한다.
- Bounds·지면·건물 출입구·실내 통로 검증을 통과한다.
- 표현 전후 Simulation canonical hash와 `WorldRevision`이 같다.
- 실제 E7은 canonical `SimulationWorldShell`의 입력·Game View·귀환 증거로만 판정한다.
