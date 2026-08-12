# Unity 아르카나 카드와 비즈니스 흐름 통합 제안서

> 이 문서는 기존 홍익학당 학습 카드와 비즈니스 미리보기의 연결을 다루는 선택형 확장 제안이다. 일반 타로를 기본 덱으로 사용하는 지역 경영 게임의 현재 기준은 [Unity 일반 타로 기반 지역 경영 게임 뼈대 기획](UnityTarotManagementGameFoundationPlan.md)을 따른다. 기존 학당 카드의 고유 식별자와 구현은 호환성을 위해 유지하지만 기본 게임 덱의 필수 전제로 사용하지 않는다.

- 기준일: 2026-08-11
- 문서 성격: 구현 제안서
- 대상: Ssalddel Unity World, 별도 Simulation, 저녁 학당 학습 흐름
- 연관 문서:
  - [Unity 정식 상품 Farm-to-Market 생애주기 제안서](UnityCanonicalProductFarmToMarketLifecycleProposal.md)
  - [Unity Concept Card 표현 패턴](UnityConceptCardPresentationPattern.md)
  - [Unity 턴 카드 모판 설계](UnityTurnCardSeedbedDesign.md)
  - [현재 작업 기준선](../AI/CURRENT_WORK.md)

## 1. 제안 요약

아르카나 카드를 임의의 재화 버프나 점술 결과로 붙이지 않는다. 플레이어가 낮 동안 수행한 생산·유통·주문·운송·교역 행위를 저녁에 다시 성찰하고, 다음 날의 판단 방식을 하나씩 습득하는 **학습 규칙 시스템**으로 통합한다.

권장 구조는 다음과 같다.

```text
낮의 Simulation 행동과 결과
  → 서버 권위의 행동 결과 ledger
  → 결정론적 상황 tag 변환
  → 허용 목록 안에서 저녁 콘텐츠 추천
  → 플레이어가 카드·영상·핵심 구절 선택
  → Preview → 성찰 입력 → Confirm
  → 다음 날 내면 규칙 활성화
  → 기존 비즈니스 Preview의 근거·대안·복구 선택지 보강
  → 기존 Confirm과 WorldTick
  → canonical state 재조회
```

핵심 원칙은 **카드가 비즈니스 상태를 직접 바꾸지 않고, 비즈니스 결정을 더 잘 내리게 한다**는 것이다. 카드 규칙은 우선 Preview에만 작용한다. 재고량, 금액, 상품 계보, 검사 결과, 운영 서버 상태는 기존 권위와 보존 법칙을 그대로 따른다.

## 2. 현재 구현 기준선

### 2.1 이미 구현된 기반

현재 코드에는 저녁 학당의 최소 수직 흐름이 있다.

- 낮과 저녁 학습 phase가 구분되어 있다.
- `바보 · 모를 뿐`과 `전차 · 통합된 전진` 콘텐츠 fixture가 있다.
- 같은 날 중복 학습, 빈 성찰, stale 또는 위조된 Preview가 차단된다.
- 학습 결과는 다음 날에만 내면 stat과 규칙으로 적용된다.
- LLM 추천 요청은 낮 행동과 허용된 콘텐츠 ID를 받는다.
- LLM이 허용 목록 밖의 콘텐츠를 만들거나 실제로 없던 행동을 근거로 제시하면 거부한다.
- 전차에는 부정적 수치 효과가 없고 `IntegratedProgress`라는 긍정적 규칙을 부여한다.
- 학습 Tick은 정착지 경제, 상품 source, 재고량을 바꾸지 않는다.

현재 규칙은 두 개다.

| 카드 | 현재 규칙 | 현재 의미 |
|---|---|---|
| 바보 | `BeginnerMind` | 모름을 인정하고 알아차리는 마음 |
| 전차 | `IntegratedProgress` | 충돌하는 힘을 통합해 방향 있게 전진하는 태도 |

### 2.2 현재의 가장 중요한 빈틈

`ActiveRuleCodes`에는 규칙이 저장되지만, 생산·주문·운송·수출 Preview가 이 규칙을 실제로 소비하지 않는다. 따라서 지금은 “카드를 배웠다”는 상태는 남아도 플레이 방식은 달라지지 않는다.

첫 구현 목표는 새 카드 수를 늘리는 일이 아니라 다음 연결을 닫는 것이다.

```text
저녁에 습득한 규칙
  → 다음 날 비즈니스 Preview 보강
  → 플레이어가 달라진 정보를 보고 선택
  → 기존 명령과 보존 법칙으로 실행
```

### 2.3 현재 비즈니스 진행 상황과 접점

현재 Simulation은 개별주문, 화물운송, 같이주문, 음식배달, 시장 소비, 수출 준비, 수출 Cargo 인계와 목적지 이동까지 연결되어 있다. `EXPORT-LOGISTICS-1` 이후 다음 후보는 목적지 항만 준비시설 인수인 `EXPORT-PORT-RECEIVING-1`이다.

따라서 카드 통합의 첫 검증 지점은 새 미니게임이 아니라 **도착한 수출 Cargo 300kg을 항만 준비시설이 인수하는 Preview**가 적합하다. 기존 수량 보존과 계보 검증이 강하고, 모름·통합·정의 같은 카드 의미를 정보 표현으로 검증하기 쉽기 때문이다.

## 3. 카드의 세 가지 역할을 분리한다

하나의 “카드”라는 말이 서로 다른 데이터 권위를 섞지 않도록 역할과 ID를 분리한다.

| 역할 | 용도 | 권위 | 예시 ID |
|---|---|---|---|
| 학습 콘텐츠 카드 | 영상, 핵심 구절, 카드 이미지, 출처를 보여 줌 | 승인된 학습 catalog | `learning:hongik.fool.beginner-mind` |
| 플레이어 습득 카드 | 발견·학습·숙련·마지막 학습일을 기록 | Simulation/player progression | `player-card:major:00` |
| 비즈니스 Concept Card | 상태·이유·가능 행동을 화면에 투영 | canonical state의 projection | `concept:export.port-receiving` |

타로 카드 번호나 명칭을 비즈니스 entity stable ID로 사용하지 않는다. 카드가 감자, HarvestLot, Cargo, 주문, 운송 Task를 소유해서도 안 된다.

## 4. 콘텐츠와 출처 모델

### 4.1 홍익학당 설명과 일반 타로 설명의 분리

같은 카드에 두 설명 계열을 둘 수 있지만 필드와 효과 근거를 분리한다.

- `HongikAcademyMeaning`: 영상 ID, 재생 시점, 자막 핵심 구절, 검수 상태
- `GeneralTarotMeaning`: 일반 정방향·역방향 의미, 출처, 검수 상태
- `EffectBasisCode`: 실제 게임 규칙이 어느 설명을 근거로 했는지 명시

초기 게임 효과는 `EffectBasisCode=HongikAcademy`인 승인 콘텐츠만 사용한다. 일반 타로 의미가 `미입력`이거나 출처 검수가 끝나지 않았다면 설명 영역에도 게시하지 않고 게임 효과 근거로 삼지 않는다.

특히 바보는 “무분별한 마음”을 충동이나 무책임으로 단순 번역하지 않는다. 여기서의 핵심은 **아직 모른다는 사실을 분별로 덮지 않고 그대로 알아차리는 마음**이다. 따라서 바보 규칙은 무작위 성공률이나 위험 행동 보상이 아니라, 누락된 모름과 확인할 질문을 드러내야 한다.

### 4.2 Notion, Blob Storage, 런타임 catalog의 경계

```text
영상·자막 수집
  → 카드별 구간 분리와 핵심 구절 전처리
  → Notion 검수 DB
  → 승인된 revision만 게시
  → 이미지 Blob + 불변 publication snapshot
  → 서버의 학습 catalog
  → Unity read model
```

- Notion은 사람이 검수하는 editorial projection이며 런타임 권위가 아니다.
- Blob Storage는 이미지와 필요한 공개 미디어 파생물의 저장 위치다.
- DB에는 임의 URL 문자열보다 `BlobObjectKey`, content hash, mime type, source license/provenance를 둔다.
- 승인 취소나 내용 수정은 기존 레코드 덮어쓰기가 아니라 새 `ContentRevision`으로 게시한다.
- `Draft`, `미입력`, `음성 대조 필요` 상태는 Unity 배포 catalog에서 제외한다.

## 5. 카드 획득과 저녁 학습 흐름

완전 무작위와 완전 순차 중 하나를 택하기보다 혼합 방식을 권장한다.

### 5.1 대 아르카나는 순차적인 큰 여정

0 바보에서 21 세계까지의 기본 해금은 순서를 가진다. 이는 플레이어가 비즈니스 규칙을 한 번에 받지 않고, 모름 인식에서 시작해 완결과 재시작까지 하나의 학습 여정을 경험하게 한다.

### 5.2 저녁 추천은 상황에 따라 달라진다

이미 해금됐거나 현재 학습 가능한 카드 안에서 낮의 행동 결과에 맞는 후보를 추천한다. 후보가 여러 개면 다음 방식을 함께 사용할 수 있다.

- 결정론적 최우선 후보 1개
- 같은 상황과 관련 있는 선택 후보 1~2개
- 낮은 빈도의 발견 카드 1개

무작위는 **노출 순서**에만 영향을 주고 카드의 의미나 효과 수치를 뒤집지 않는다. 정·역방향을 무작위 긍정·부정 효과로 사용하지 않는다.

### 5.3 최종 선택은 플레이어가 한다

LLM은 “왜 오늘 이 영상을 권하는가”를 설명하지만 효과를 만들지 않는다. 플레이어는 추천된 영상·카드의 핵심 구절을 보고 하나를 선택한다. 선택 전에 효과 Preview를 보여 주고, 성찰을 입력한 뒤 명시적으로 Confirm한다.

## 6. 비즈니스 효과의 안전한 단계

### 단계 1 — 정보·해석 보강

첫 구현 범위다.

- 아직 확인하지 않은 사실 표시
- source lineage와 revision 표시
- 막힌 이유와 필요한 조건 표시
- 서로 다른 처분·주문·운송 대안 비교
- 결정이 영향을 주는 다음 단계 표시

이 단계는 canonical state나 허용 명령을 바꾸지 않는다.

### 단계 2 — 복구·재검토 행동 개방

정보 보강이 안정된 뒤 적용한다.

- 진행 중단 후 재검토
- stale 계획 재작성
- 실패 검사 재작업 Preview
- 의향 철회나 대안 경로 비교
- Confirm 전 체크리스트 재실행

복구 행동도 기존 도메인 규칙이 허용하는 명령만 호출한다.

### 단계 3 — 제한된 Simulation 수치 효과

가장 나중에 별도 결정으로 도입한다. 도입하더라도 effect revision, 범위, 만료, stack policy, 보존 테스트가 필요하다.

허용 가능한 예는 가상 학습 포인트나 Preview 비용처럼 canonical 물류와 분리된 수치다. 다음 값은 카드가 직접 바꾸지 않는다.

- 실제 또는 Simulation 상품·HarvestLot·Cargo 수량
- 원가, 매출, 정산 금액
- 검사 통과 여부
- 주문·운송·통관의 운영 상태
- source provenance와 stable ID

## 7. 대 아르카나와 현재 비즈니스 흐름의 연결 지도

아래 효과는 모두 플레이어에게 유리한 **학습·판단 보조 효과**다. 불리한 정·역방향 수치, 무작위 실패, 상품 수량 증감은 넣지 않는다. 다만 바보와 전차를 제외한 카드의 규칙 code와 효과는 아직 설계 초안이다. 각 카드에 해당하는 홍익학당 자막 구간과 핵심 구절을 검수하고 새 `EffectRevision`을 승인해야 게시할 수 있다.

### 7.1 카드별 간략 효과 초안

| 번호·카드 | 제안 규칙 code | 내면 stat 제안 | 다음 날 비즈니스 Preview 효과 |
|---|---|---|---|
| 0 바보 | `BeginnerMind` | 알아차림 +1 | 미확인 조건과 “아직 모르는 것”, 확인할 질문을 별도 표시한다. |
| 1 마법사 | `ResourceOrchestration` | 의지 +1 | 현재 사용할 수 있는 자원·도구·용량과 빠진 의존성을 한 묶음으로 보여 준다. |
| 2 여사제 | `EvidenceAndIntuition` | 통찰 +1 | 확인된 증거와 아직 검증하지 않은 가설·직관을 서로 다른 영역에 표시한다. |
| 3 여제 | `CultivatingCare` | 양심 +1 | 재배·돌봄 입력이 성장과 품질에 미칠 다음 단계 영향을 미리 설명한다. |
| 4 황제 | `OrderedExecution` | 명료 +1 | 필요한 작업 순서, rule revision, 책임 주체와 선행 조건을 명확히 정렬한다. |
| 5 교황 | `SharedStandard` | 양심 +1 | 협동조합·검사·교육에 적용되는 공동 규칙과 그 출처를 함께 보여 준다. |
| 6 연인 | `ConsciousChoice` | 양심 +1 | 처분·계약·주문 선택지별 약속, 포기하는 가치와 영향받는 주체를 비교한다. |
| 7 전차 | `IntegratedProgress` | 의지 +1 | 출발·인계·이동·도착 milestone과 참여 주체가 같은 목적을 향하는지 점검한다. |
| 8 힘 | `SteadyStrength` | 균형 +1 | 사고·실패·압박 상황에서 즉시 Confirm하기 전에 안전한 복구·재검토 선택을 먼저 제시한다. |
| 9 은둔자 | `LineageReflection` | 통찰 +1 | 상품 source부터 HarvestLot·Cargo·현재 상태까지 전체 계보를 집중 감사한다. |
| 10 운명의 수레바퀴 | `CycleAwareness` | 알아차림 +1 | 현재 날짜·계절·수요 주기에서 이번 결정이 어느 국면에 있는지 보여 준다. |
| 11 정의 | `ConservationJustice` | 양심 +1 | 수량·단위·참여 의향·책임의 보존표를 표시하고 불일치 지점을 강조한다. |
| 12 매달린 사람 | `PerspectivePause` | 통찰 +1 | 도메인이 허용할 때 실행 대신 보류·관점 전환·계획 재작성 Preview를 연다. |
| 13 죽음 | `CleanTransition` | 명료 +1 | 끝내야 하는 계획·lot·상태와 이후 새 상태의 시작 조건을 분리해 보여 준다. |
| 14 절제 | `BalancedFlow` | 균형 +1 | 품질·손실·노동·재고 제약을 한 표에서 비교하고 균형을 깨는 선택을 경고한다. |
| 15 악마 | `AttachmentAwareness` | 알아차림 +1 | 단기 이익 뒤의 lock-in, 숨은 비용, 과도한 의존 관계를 표시한다. |
| 16 탑 | `AssumptionReset` | 명료 +1 | stale revision이나 실패로 무효가 된 가정을 지우고 재구축 순서를 체크리스트로 제시한다. |
| 17 별 | `TraceableHope` | 의지 +1 | 검증된 source lineage와 장기 목표를 연결해 다음에 이어 갈 수 있는 경로를 보여 준다. |
| 18 달 | `UncertaintyAwareness` | 알아차림 +1 | 수요·예측의 불확실성을 확정값으로 가장하지 않고 미확인·범위·추가 evidence로 나눈다. |
| 19 태양 | `VerifiedClarity` | 명료 +1 | 완료된 작업과 성공 근거, 함께 기여한 주체를 명료하게 요약한다. |
| 20 심판 | `ReflectiveReconciliation` | 양심 +1 | 원래 의도와 실제 결과를 대조하고 정정·재작업·마감 후보를 보여 준다. |
| 21 세계 | `LifecycleCompletion` | 균형 +1 | Farm-to-Market 전체 계보와 보존 조건의 완결을 확인하고 다음 학습 여정을 연다. |

### 7.2 획득, 활성화와 중첩 규칙

- **카드 획득**은 영구 수집 상태다. 획득한 카드와 검수된 영상·핵심 구절은 도감에서 다시 볼 수 있다.
- **첫 학습 보상**은 위 표의 내면 stat `+1`을 다음 날 한 번 적용한다. 이 수치는 성찰 성장과 콘텐츠 해금에만 사용하고 재고·가격·검사 성공률을 계산하지 않는다.
- **집중 규칙**은 저녁에 선택한 카드 한 장만 다음 Simulation day 동안 활성화한다. 해당 scope의 Preview는 여러 번 열어 볼 수 있지만 동시에 두 카드 효과를 stack하지 않는다.
- **재학습**은 이미 보유한 카드를 다시 집중 규칙으로 선택하는 행위다. 내면 stat을 반복 지급하지 않고, 다른 비즈니스 문맥에서 같은 원리를 적용한 기록만 남긴다.
- **만료** 시 카드는 사라지지 않고 활성 목록에서 해제되어 도감 보유 상태만 남는다. 다음 저녁에 같은 카드 또는 다른 카드를 다시 선택할 수 있다.

현재 구현은 `ActiveRuleCodes`에 규칙을 계속 누적하고 stat validator도 알아차림과 의지 효과만 허용한다. 따라서 나머지 20장을 데이터만 추가해서는 안 된다. `RuleGrantStableId`, 적용일·만료일, scope, 재학습 여부를 갖는 grant 계약과 여섯 내면 stat의 허용 규칙을 먼저 확장해야 한다.

### 7.3 근거 상태

- 바보와 전차: 현재 홍익학당 source를 연결한 fixture와 다음 날 효과 테스트가 있는 구현 기준선이다.
- 마법사부터 세계까지의 나머지 카드: 이 문서에 effect design만 정의한 상태다.
- 나머지 카드의 stat과 규칙은 홍익학당 자막 시점·핵심 구절·음성 대조를 마친 뒤 카드별로 승인한다.
- 일반 타로 정·역방향 설명은 별도 검수 자료이며 위 효과의 자동 근거가 아니다.

## 8. 오전 행동 ledger와 추천 입력

현재의 범용 tag 다섯 개만으로는 비즈니스 의미가 너무 많이 사라진다. LLM이 자유 문장에서 tag를 추측하게 하지 말고, 각 Simulation UseCase가 결과를 canonical outcome으로 기록한 뒤 결정론적 adapter가 추천 tag로 바꾼다.

예시 tag는 다음과 같다.

- `UnknownSkipped`, `EvidenceChecked`
- `DispositionCompared`, `CompetingForces`
- `CooperativeTermsReviewed`, `DirectSalePromiseCreated`
- `OrderConfirmed`, `GroupTargetMissed`
- `CargoLoaded`, `JourneyStarted`, `HandoffCompleted`
- `InspectionFailed`, `ReworkConfirmed`
- `LossRecorded`, `DemandUncertain`
- `PortReceivingBlocked`, `QuantityConservationChecked`

각 ledger 항목은 최소한 다음을 가진다.

```text
ActionStableId
ActionRevision
SimulationSessionId
WorldTick
BusinessDomainCode
LifecycleStageCode
OutcomeCode
OutcomeTags[]
SourceLineageRefs[]
```

추천 요청에는 이 ledger의 불변 snapshot과 현재 학습 가능한 콘텐츠 allow-list만 전달한다. LLM 응답은 `RecommendedContentStableId`, 실제 ledger의 `CitedActionStableIds`, 짧은 추천 이유만 반환한다.

## 9. 권장 계약

### 9.1 학습 콘텐츠 게시 snapshot

```text
LearningContentStableId
ArcanaStableId
Title
HongikAcademyMeaningRevision
HongikSourceVideoId
HongikSourceStartSeconds
HongikCorePassage
GeneralMeaningRevision?          // 별도 검수 완료 때만
ImageBlobObjectKey
ImageContentHash
ReviewStatus
EffectBasisCode
EffectRevision
GrantedRuleDefinition
PublishedAtUtc
```

### 9.2 내면 규칙 부여 snapshot

```text
RuleGrantStableId
PlayerSimulationId
RuleCode
SourceLearningContentStableId
SourceEffectRevision
EffectiveWorldTick
ExpiresAtWorldTick?
ScopeCodes[]
StackPolicyCode
```

### 9.3 비즈니스 Preview 보강 결과

```text
CanonicalPreviewRevision
AppliedRuleGrantIds[]
EvidenceRows[]
RevealedUnknowns[]
AlternativeSummaries[]
RecoveryIntents[]
Explanations[]
```

보강 결과는 canonical command payload를 몰래 바꾸지 않는다. Confirm 시에는 원본 Preview revision과 적용된 grant ID를 함께 검증해 stale, 위조, 만료된 규칙 사용을 막는다.

## 10. 첫 수직 구현: 항만 준비시설 인수

### 10.1 장면

`EXPORT-LOGISTICS-1`로 수출 Cargo 300kg이 목적지에 도착한 뒤 `EXPORT-PORT-RECEIVING-1` Preview를 연다.

### 10.2 카드가 없을 때

- 기존 canonical 도착 상태와 인수 가능 여부를 표시한다.
- Cargo, HarvestLot, packing lot, allocation, handoff, movement 계보를 검증한다.
- 수량·단위·목적지 불일치가 있으면 기존 규칙대로 차단한다.

### 10.3 바보 규칙이 활성화됐을 때

- “모르는 사실”을 별도 영역에 표시한다.
- 통관 준비, 항만 인수 주체, 보관 조건처럼 아직 Simulation이 확정하지 않은 항목을 완료된 사실처럼 보이지 않게 한다.
- 인수 전에 확인할 질문과 다음 evidence를 제시한다.

### 10.4 전차 규칙이 활성화됐을 때

- Farm 출발부터 수출 준비, Cargo 인계, 이동, 목적지 도착까지 milestone 연결을 한 화면에 보여 준다.
- 서로 다른 힘을 빠르게 밀어붙인다는 뜻이 아니라, 인계 주체·Cargo·목적지·작업 상태가 같은 방향인지 확인한다.
- 속도, 수량, 검사 성공률을 올리지 않는다.

### 10.5 정의 규칙을 추가했을 때

- 최초 300kg, 기존 출고 예약, 이동 Cargo, 목적지 인수 후보의 수량·단위를 나란히 대조한다.
- mismatch가 있으면 위반한 보존 조건과 책임 경계를 강조한다.
- 오류를 자동으로 수정하거나 Confirm을 우회하지 않는다.

세 경우 모두 허용되는 command와 canonical 수량은 같아야 한다. 달라지는 것은 플레이어가 결정을 이해하는 깊이와 선택 전에 볼 수 있는 근거다.

## 11. 우선순위와 구현 순서

현재 구현 상태를 반영한 우선순위는 다음과 같다. 두 fixture의 효과 경계를 먼저 코드로 고정하고, 실제 업무 adapter와 게시 catalog를 차례로 연결한 뒤 카드 수를 늘린다.

| 우선순위 | 구현 단위 | 상태 | 완료 gate |
|---|---|---|---|
| P0 | `CARD-BIZ-1A` 공통 Preview 보강기 | 완료 | 집중 규칙 한 장만 적용하고 바보 unknown·전차 milestone을 투영하며 canonical 300kg·계보·허용 intent 불변 |
| P1 | `CARD-BIZ-1B` 실제 업무 Preview adapter | 완료 | 서버 항만 인수 JSON wire snapshot을 검증해 P0 입력으로 결정론적으로 변환하고 block·운영 경계 누락을 fail-closed 거부 |
| P2 | `CARD-BIZ-0` 게시·출처 계약 | 기술 Gate 완료·실제 게시 0건 | 승인된 홍익학당 revision과 Blob image만 runtime catalog에 게시 |
| P3 | `CARD-BIZ-2` 오전 행동 adapter | 대기 | Simulation 결과에서 LLM 입력용 canonical ledger 생성 |
| P4 | `CARD-BIZ-3` 핵심 여섯 장 | 대기 | 카드별 자막 검수와 effect revision 승인 뒤 정보·복구 효과 추가 |
| P5 | `CARD-BIZ-4` 대 아르카나 22장 | 대기 | 순차 해금·재학습·한 장 집중 규칙과 save/replay 연결 |
| P6 | `CARD-BIZ-5` 서버 추천 | 대기 | provider-neutral LLM allow-list, fallback와 개인정보 경계 검증 |

### `CARD-BIZ-0` — 게시·출처 계약

- 검수된 홍익학당 콘텐츠만 immutable publication snapshot으로 내보낸다.
- Notion review row, Blob image, 영상·자막 근거, effect revision의 계보를 연결한다.
- 일반 타로 의미는 별도 source와 승인 상태를 갖는다.

### `CARD-BIZ-1` — 기존 두 규칙의 첫 소비자

- `BeginnerMind`를 `EXPORT-PORT-RECEIVING-1`과 수확물 처분 Preview의 unknown/evidence 보강에 연결한다.
- `IntegratedProgress`를 물류 이동과 항만 인수 Preview의 milestone 일관성 보강에 연결한다.
- 규칙이 Preview projection만 바꾸고 canonical state는 바꾸지 않음을 테스트한다.
- `CARD-BIZ-1A`에서는 `저녁학당업무Preview보강Projector`를 구현했다. 보유 규칙 중 `FocusedRuleCode` 한 장만 적용하며, 규칙이 없을 때는 보강 정보가 비어 있다.
- 항만 인수 300kg fixture에서 바보는 미확인 질문 두 건만, 전차는 정렬된 이동 milestone만 반환한다. Preview stable ID·revision·상품·수량·단위·source lineage를 복사하고 canonical state와 허용 intent를 바꿀 수 없다는 flag를 고정했다.
- `CARD-BIZ-1B`에서는 실제 서버 `Simulation수출항만인수PreviewSnapshot`의 JSON field와 route를 독립 Unity `ApiModel`에 매핑했다. Unity runtime은 서버 contract assembly를 참조하지 않고 test만 양쪽 contract를 참조해 wire parity를 검증한다.
- 차단 사유가 있는 Preview, `NoCustomsClearance` 등 일곱 운영 경계 중 하나라도 빠진 응답, Cargo·Lot·시설·Task 계보와 300kg·단위가 맞지 않는 응답은 보강 전에 거부한다.
- 실제 HTTP 호출과 Unity Presenter·화면 연결은 아직 수행하지 않았다.

### `CARD-BIZ-2` — 비즈니스 결과 adapter

- 기존 Simulation 결정·작업·효과에서 오전 행동 ledger를 생성한다.
- 현재 수출 인계·물류와 다음 항만 인수부터 시작해 주문·협동조합·직거래로 확장한다.
- LLM은 이 adapter의 결과만 추천 근거로 사용할 수 있다.

### `CARD-BIZ-3` — 핵심 카드 확장

바보와 전차 다음에는 현재 비즈니스 흐름과 접점이 큰 여섯 장을 우선한다.

- 연인: 처분·약속·이해관계 선택
- 은둔자: 계보 감사
- 정의: 수량·단위·의향 보존
- 절제: 품질·손실·노동 균형
- 탑: invalidation과 복구
- 세계: 생애주기 완결

### `CARD-BIZ-4` — 22장 학습 여정

- 대 아르카나 순차 해금과 상황별 재노출을 구현한다.
- 같은 카드를 다른 비즈니스 문맥에서 다시 학습할 수 있게 한다.
- 소 아르카나 56장은 홍익학당 구간 분리와 음성 대조, 일반 의미 출처 검수가 충분히 끝난 뒤 별도 확장한다.

### `CARD-BIZ-5` — 서버 추천과 저장·재생

- provider-neutral LLM 추천 경계를 서버에 둔다.
- 허용 목록 위반 시 결정론적 fallback을 사용한다.
- 카드 습득, 성찰, rule grant, recommendation evidence를 save/replay hash와 migration 정책에 포함한다.

## 12. 테스트와 완료 기준

첫 통합 gate는 다음 조건을 모두 만족해야 한다.

- 카드 규칙이 없는 Preview와 있는 Preview가 같은 canonical source와 300kg을 가리킨다.
- `BeginnerMind`는 unknown과 질문만 추가하고 성공률이나 수량을 바꾸지 않는다.
- `IntegratedProgress`는 milestone과 인계 일관성만 보강한다.
- 만료, stale, 중복, 위조된 rule grant가 거부된다.
- LLM이 allow-list 밖의 카드나 ledger에 없는 행동을 인용하면 fallback으로 전환된다.
- Notion의 Draft 또는 일반 의미 `미입력` 콘텐츠가 게시 catalog에 들어오지 않는다.
- Blob image의 object key와 hash가 없거나 source 검수가 끝나지 않으면 게시가 실패한다.
- save/replay 뒤 같은 오전 ledger에는 같은 결정론적 후보 집합이 생성된다.
- 카드 학습 Tick 전후 정착지 경제와 상품·Cargo 계보가 동일하다.
- operational API, 실제 주문·결제·운송·통관을 호출하지 않는다.

## 13. 사업적 가치와 수익화 경계

이 통합의 가치는 타로 수집 자체보다 복잡한 생산·유통 시뮬레이션을 이해 가능한 학습 경험으로 바꾸는 데 있다.

- 신규 플레이어에게 업무 규칙을 장문의 튜토리얼 대신 상황과 성찰로 가르친다.
- 상품 계보와 수량 보존을 “감사 로그”가 아니라 플레이 의미로 경험하게 한다.
- 농장, 협동조합, 주문, 물류, 수출마다 같은 카드를 다른 맥락에서 재해석할 수 있다.
- 장기적으로 검수된 학습 시즌, 농식품 유통 교육 시나리오, 커뮤니티 협동 과정을 독립 콘텐츠 팩으로 구성할 수 있다.

다만 카드 희귀도 판매, 무작위 성능 뽑기, 불안 심리를 이용한 추천은 이 설계와 맞지 않는다. 유료화가 필요하다면 성능 우위가 아니라 검수된 강의 묶음, 새로운 시뮬레이션 사례, 시각 테마처럼 권위 데이터와 공정성을 침해하지 않는 범위가 적합하다.

## 14. 측정 지표

영상 재생 시간이나 카드 클릭 수만 최적화하지 않는다. 다음 행동 품질을 본다.

- Preview에서 evidence와 source lineage를 실제로 열어 본 비율
- Confirm 전 대안을 비교한 비율
- stale command와 중복 예약의 감소
- 수량·단위 보존 위반을 실행 전에 발견한 비율
- 실패 뒤 복구 Preview를 선택한 비율
- 같은 카드를 새 비즈니스 문맥에서 재학습한 비율
- Farm-to-Market 전체 계보를 끊김 없이 완결한 비율

개인 성찰 텍스트를 심리 프로파일링이나 광고 타기팅에 쓰지 않는다. 기본 저장은 로컬 또는 private 범위로 두고, 서버에는 명시적 동의가 없다면 선택한 콘텐츠 ID와 규칙 code 같은 최소 이벤트만 남긴다.

## 15. 최종 권고

22장 전체 데이터 입력보다 작은 gate를 우선한다. `CARD-BIZ-1A → CARD-BIZ-1B`와 `CARD-BIZ-0` 기술 Gate는 완료됐지만 실제 승인 게시물은 0건이다. 카드 수를 늘리기 전에 [턴 카드 모판](UnityTurnCardSeedbedDesign.md)의 C2 사람 검수와 C5 게시 snapshot을 통과시키고, 이후 `CARD-BIZ-2 → CARD-BIZ-3`으로 진행한다.

1. 홍익학당 근거가 검수된 바보와 전차의 게시 snapshot을 고정하고 Notion·Blob을 runtime 권위와 분리한다.
2. Draft, 미입력, 운영 경계 누락 콘텐츠가 runtime catalog에 들어오지 못하도록 게시 validator를 만든다.
3. 오전 행동 canonical ledger를 만든 뒤 정의 카드를 수량 보존 카드로 추가한다.
4. 실제 HTTP repository와 Unity Presenter 연결은 승인 catalog와 같은 source revision을 소비하게 한다.

이 순서라면 현재 Unity의 저녁 학당 구현을 버리지 않고 살리면서도, 카드가 장식이나 임의 버프로 고립되지 않는다. 플레이어는 낮에 비즈니스를 수행하고, 저녁에 그 의미를 배우고, 다음 날 더 잘 보고 판단하는 방식으로 성장한다.

## 16. 2026-08-11 `CARD-BIZ-0` 진행 갱신

게시·출처 계약의 기술 게이트는 완료했다. VideoSearch의 `hongik-unity-learning-card-publication.v1` publisher는 런타임 승인, 원음 대조, 미해결 quality flag 0건, 영상·자막 근거, Notion editorial stable ID, Blob object·hash·license, `HongikAcademyTranscript` 기반 effect revision이 모두 있어야 snapshot을 만든다. 동일 stable ID와 revision은 덮어쓰지 않는다.

Unity의 `학습카드PublicationAdapter`는 같은 publication hash를 재계산하고, 첫 vertical slice의 `Awareness + BeginnerMind`, `Resolve + IntegratedProgress`만 기존 저녁 학당 콘텐츠로 투영한다. 일반 타로 의미는 별도 source로 남고 게임 효과 근거를 대체하지 못한다.

다만 현재 타로 노트와 직접 인용은 음성 대조·사람 승인 전이다. 따라서 `CARD-BIZ-0` 상태는 **기술 게이트 완료 / 실제 바보·전차 게시 0건**이다. 다음 우선순위는 오전 행동 adapter로 넘어가기 전에 바보·전차 원음 대조와 승인 snapshot 두 건을 고정하는 것이다.
