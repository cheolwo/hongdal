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

## 현실 품목과 시각 자산의 대응 — D425 (2026-08-31)

농수산물 등의 외부 코드를 Prefab 경로와 직접 같은 식별자로 사용하지 않는다. 기존
`CanonicalProductStableId`와 출처별 코드 관계를 보존하고, **품목·표현 역할/상태 →
검토된 표현 후보 → 기존 VisualKey/자산 계열 → Unity 자산 목록의 실제 Prefab**으로 연결한다.
상세 데이터/개발 범위는 [D425 품목·시각 자산 대응 계획](../AI/농수산품목-시각자산대응-기획과개발인계-2026-08-31.md)을 따른다.

코드가 같은 품목을 뜻하는지에 대한 검토와, 자산이 그 품목을 얼마나 잘 나타내는지에 대한
검토는 별개다. ‘품목 외형 적합 / 범주 대체 / 부적합 / 미검토·미확보’를 구분하고 주 후보와
승인된 대체 후보를 기록한다. 특정 종·품종·등급까지 식별 가능한 외형이라는 뜻은 별도 근거가
있을 때만 사용한다. 이름이 비슷하거나 모양이 같다는 이유로 외부 코드 대응을 확정하지 않는다.

한 품목의 생육·수확·포장·조리·UI 이미지 역할을 구분하며, 같은 범용 자산을 여러 품목에
대체 표현으로 쓸 때도 실제 품목 이름/상태는 권위 데이터를 읽는다. 모델 개수·크기로 재고량이나
거래 단위를 추정하지 않는다. 외형 후보가 없으면 기존 명시적 미확보/대체 정책으로 반환하며
검사기의 등록 키 제약을 풀어 임의 경로를 로드하지 않는다.

서버는 품목·표현 대응과 검토 이력을 관리하고 Unity는 기존 자산 목록에서 실제 참조를 해결한다.
Prefab/GUID·판본/fingerprint·미리보기 근거는 표현 측 기록이다. 공급사 원본/패키지를 운영 API에
업로드하거나 재배포하는 승인이 아니다. 자료조사 담당은 품목 의미, 공간 담당은 실제 외형/자산,
개발은 계약·저장/조회·결정적 선택을 맡는다. 후보 등록과 실제 World 배치·입력/E5는 구분한다.

## 개체 종류와 개별 기록의 표현 대응 — D426 (2026-08-31)

[D426 개체·레코드 대응 확장](../AI/개체레코드-시각자산대응-확장과개발인계-2026-08-31.md)은 D425의
품목 중심 범위를 창고·마트·농장 등으로 넓힌다. 같은 상태·목적에서 유효한 개별 승인 표현을
우선하고, 없으면 승인된 종류 기본 표현과 대체 사유를 반환한다. 접근 거부·자료 조회 실패를
기본형으로 숨기지 않으며 승인된 표현도 없으면 미연결로 남긴다.

DbSet/Entity는 조사 출발점이며 모든 행을 World 객체로 만들지 않는다. 기존 서버 관점별 조회와
공간 실체·상태·정보창·Web 인계·내부 자료 분류를 유지한다. 공통 대응 기능과 분야별 읽기 연결을
재사용하며 실제 자산 해결과 배치는 기존 자산 대장·배치 검증을 거친다. 기본형 승인·대응 저장은
개별 대상의 정확 외형 검증이나 실제 Unity 연결·E5 완료가 아니다. 상세 범위는 D426 원문을 따른다.

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

## 13팩 전수 기능군 대장

팩 이름은 자산 출처이고 게임 기능 모듈은 아니다. Unity의
`Synty전체자산ModuleCatalog`은 기존 Nature·Farm·Town·City·Construction·Generic·Starter와
추가된 Base Locomotion·Emotes and Taunts·Sword Combat·Alpine Mountain·Dungeon Realms·
Dwarven Dungeon Map까지 13팩의 Prefab `4,211`개를 다음 12개 기능군으로 분류한다.

- 월드 지면, 자연 식생, 실외 구조물, 실외 기능 소품
- 도로·통행망, 영역 전이, 건설·복구 상태
- 실내 구조, 실내 설비, 실내 소품
- 인물·차량·도구, 세계 피드백 효과

한 Prefab은 여러 기능군에 속할 수 있다. 자동 분류 결과는 배치 승인이 아니며
`production-ready`, `needs-review`, `shared-base`, `prototype-fallback`,
`reserved-for-future-loop`, `excluded` 수명주기로 따로 관리한다. 기능 모듈이 없으면
반드시 보류 이유가 있어야 한다.

Construction은 AreaSet이 아니라 모든 영역이 사용할 수 있는 건설·복구 상태 계층이다.
Generic은 공유 기반, Starter는 prototype fallback으로만 취급한다. 새로 구매한 팩도
새 AreaSet을 만들지 않으며 최초 상태는 모두 `needs-review`다.

호환 기능군과 팩 정책의 관리 기준은
`eng/execution-ledgers/synty-asset-functional-modules.json`, Unity 실제
대장은 `Synty전체자산ModuleCatalog.asset`, 공개 수량 요약은 Unity 프로젝트의
`Documentation/Generated/Synty전체자산ModuleCatalog.md`다.

## 프리팹과 애니메이션 원천의 분리

프리팹 기능 대장과 애니메이션 원천 대장은 같은 자산 사용 체계에 속하지만 같은 항목을
세지 않는다. 애니메이션 팩에 포함된 예제 Prefab 수만으로 동작 가용성을 판단하지 않는다.

| 원천 | 현재 확인량 | 주 용도 | 대장이 증명하지 않는 것 |
| --- | ---: | --- | --- |
| 전체 Prefab | `4,211` | 지면·식생·건물·소품·Actor·FX 후보 | 실제 H 배치와 WI 발현 |
| Base Locomotion Clip | `823` | 플레이어·NPC·분대 이동 | 선택 캐릭터와 Avatar 호환, 조작감 |
| Emotes and Taunts Clip | `280` | 명상·관계·협력·상태 피드백 | 광복기·회복 같은 권위 결과 |
| Sword Combat Clip | `363` | 직접 전투·초소 방어·분대 전투 | 타격 판정·피해·전투 결과 |
| Animator Controller | `176` | 공급자 예제와 상태 연결 참고 | 프로젝트 공식 Controller 채택 |

Clip 수는 실제 Unity `AssetDatabase`에서 읽은 원천 재고다. Clip이 있다는 사실은
`SyntyProvided` 표현 후보가 존재한다는 뜻일 뿐 특정 Character·Avatar·Controller에서
정상 재생된다는 증거가 아니다. 기존 Town Character의 해소되지 않은 Controller 참조처럼
공급자 Prefab 내부 결함이 있으면 원본 Prefab을 직접 고치지 않고 프로젝트 Wrapper와
명시적 Controller/Avatar 결속 또는 procedural fallback을 사용한다.

## 신규 6팩 활용 기준

| 팩 | 우선 표현 역할 | 결속할 WI/H 후보 | 첫 채택 전에 확인할 것 |
| --- | --- | --- | --- |
| Base Locomotion | Idle·Walk·Run·방향 전환 | 플레이어 이동, NPC 작업 이동, 전술 명령 공간 | Humanoid Avatar, root motion 정책, 이동 속도와 보폭, 중단·재진입 |
| Emotes and Taunts | 명상·정신 차림·협력·관계 신호 | 명상 작업 공간, 공동체 상호작용, NPC 관계 공간 | 동작 의미, 길이, 취소 가능 시점, UI·Audio 대체 신호 |
| Sword Combat | 준비·공격·피격·회복 | 위협 조우, 직접 전투, 초소 방어 | 공격 Window, ToolContact, 피해 권위와 분리, 카메라·입력 복귀 |
| Alpine Mountain | 산악 지면·식생·기후 노출·거점 | Nature 여행, 오두막 후보지, 기후 노출 공간 | 경사·Bounds·통행·기존 Nature와의 VisualKey 중복 |
| Dungeon Realms | 몬스터·폐허·초소·병영·마법 FX | 황혼 위협, 회랑 방어, 폐허 탐사 | 사람형/비사람형 Rig, Collider, 속성 신호, 과도한 던전 문맥 |
| Dwarven Dungeon Map | 광산·지하 회랑·자원 채취 | 광산 탐사, 지하 통행, 채취 작업 공간 | 모듈 연결부, 출입구, NavMesh, 광원, 지상 H와의 전이 |

`Dungeon Realms`가 있다는 이유만으로 Dungeon Area를 만들지 않고, `Alpine Mountain`이
있다는 이유만으로 기존 Nature를 교체하지 않는다. PlayableLoop가 요구하는 WI 순간과 H
능력이 먼저 존재하고, 그 표현 후보로 팩을 선택한다.

## WI·H에서 자산을 채택하는 절차

환경·소품 WI는 다음 순서를 따른다.

```text
PlayableLoop의 판독 순간
→ WI 권위 상태와 결과
→ 필요한 H1 기능 공간과 H2·H3 문맥
→ PlacementRole과 VisualKey
→ 기능 모듈
→ 자산 계열·Prefab 후보
→ fingerprint 동결
→ E5 실제 배치·Renderer·Collider·Bounds 검증
```

Actor 동작이 필요한 WI는 중간에 애니메이션 원천 절차를 추가한다.

```text
WI 순간
→ AnimationRole (locomotion / social-emote / meditation-feedback / sword-combat)
→ ActionCue와 권위 상태
→ Clip 후보와 대체 후보
→ Character Rig·Avatar·root motion 호환
→ 프로젝트 AnimationAdapter·Controller 결속
→ Clip revision·fingerprint 동결
→ 실제 입력·전이·중단·귀환 검증
```

Presentation E4 준비 기록에는 최소한 다음을 남긴다.

- 플레이어가 상태를 읽어야 하는 순간과 `WorldInteractionId`
- 필요한 H1 Capability와 H2·H3 문맥, 비공간이면 사유 있는 `NotApplicable`
- `VisualKey`, `PlacementRole`, `AnimationRole`, `ActionCue`
- 주 후보·대체 후보·procedural fallback과 각 원천 팩
- Prefab 또는 Clip GUID/fingerprint와 후보 revision
- Character Rig·Avatar·root motion·Controller 호환 판정
- Collider·Bounds·통로·접촉 Window·중단·귀환 의도
- `Ready / Conditional / Blocked`와 열린 결함

공간 배치가 필요한 경우 이 기록은 [Graph Map 기획 인계 순환 체계의 배치 맵 Synty 표현 조사 태그](GraphMap기획인계순환체계.md#배치-맵의-synty-표현-조사-태그)와 같은 후보를 참조한다. Synty 대장의 기능 분류를 배치 맵에 복제하지 않고 안정 코드·자산 계열·Prefab GUID·fingerprint만 연결한다. Graph Map은 필요한 시각 역할을, 배치 맵은 후보와 상대 배치 적합성을, Presentation E4 대장은 WI 판독 순간과 후보 동결을 각각 소유한다.

애니메이션이 필요 없는 정적 WI에 억지로 AnimationRole을 만들지 않는다. 반대로 이동·전투·
도구 접촉·수면 기상처럼 동작이 플레이 결과의 판독과 입력 복귀에 영향을 주면 단순
`Actor Prefab 준비됨`으로 대체하지 않는다.

## 애니메이션 권위와 fallback

- Clip·Animator·Avatar·IK·Animation Event는 모두 표현 계층이다.
- 피해, 수확, 건설 기여, 회복, 재고 이동은 Simulation의 Confirm·Task·Effect가 결정한다.
- Animation Event는 권위 결과를 만들지 않고 이미 승인된 접촉 Window와 피드백만 표시한다.
- root motion을 쓰더라도 권위 위치·Navigation 결과와의 reconcile 정책을 먼저 정한다.
- Clip 호환이 확인되지 않으면 `SyntyProvided`로 선언하지 않고 `Conditional` 또는 `Blocked`로 둔다.
- procedural fallback은 기능 부재를 숨기지 않으며 Game View 증거에 fallback 사용을 표시한다.
- 공급자 Controller를 공식 Controller로 자동 채택하거나 공급자 Prefab에 게임 규칙을 붙이지 않는다.

첫 적용 순서는 `Base Locomotion → 현재 플레이어 이동`, `Sword Combat → Nature 황혼 전투`,
`Emotes and Taunts → 명상·협력 피드백`이다. 환경 팩은 해당 WI가 Presentation E4 후보를
요구할 때 실제 Prefab을 좁혀 사용한다.

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
- 애니메이션은 실제 대상 Rig에서 시작·전이·중단·귀환하며 권위 결과를 중복 발생시키지 않는다.
- 실제 E7은 canonical `SimulationWorldShell`의 입력·Game View·귀환 증거로만 판정한다.
