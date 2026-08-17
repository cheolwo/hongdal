# Simulation·Unity 코드 탐색 트리

> 이 문서는 `SsalddelCodeMetadataAttribute`와 `eng/work-areas/simulation-unity.json`에서 자동 생성된다. 직접 수정하지 않는다.

```text
Simulation·Unity
├─ Simulation 세션 생명주기 [simulation-session-lifecycle]
│  ├─ 010 contract.session-create · Contract · Definition
│  ├─ 020 api.session-lifecycle · Api · Confirm
│  ├─ 030 application.session-lifecycle · Application · Confirm
│  ├─ 040 domain.session-aggregate · Domain · Tick
│  └─ 050 infrastructure.session-store · Infrastructure · Persistence
├─ 병렬 경영-전투 [simulation-parallel-battle]
│  ├─ 010 contract.battle-preview · Contract · Definition
│  ├─ 020 api.battle · Api · Confirm
│  ├─ 030 application.battle · Application · Confirm
│  ├─ 040 domain.battle-state · Domain · Tick
│  ├─ 050 infrastructure.battle-store · Infrastructure · Persistence
│  └─ 060 unity.battle-presentation · ViewModel · Presentation
├─ 1인칭 농장 타이밍 전투 [simulation-farm-combat-input]
│  ├─ 010 contract.farm-combat · Contract · Definition
│  ├─ 020 api.farm-combat · Api · Confirm
│  ├─ 030 application.farm-combat · Application · Confirm
│  ├─ 040 domain.farm-combat · Domain · Tick
│  └─ 050 unity.farm-combat-input · ClientAdapter · Presentation
├─ Simulation 저장-재생 [simulation-save-replay]
│  ├─ 010 contract.save-request · Contract · Definition
│  ├─ 020 api.save-replay · Api · Persistence
│  ├─ 030 application.save-replay · Application · Persistence
│  ├─ 040 domain.save-package · Domain · Persistence
│  └─ 050 infrastructure.save-store · Infrastructure · Persistence
├─ 공공데이터-파생 World [simulation-world-derivation]
│  ├─ 010 domain.derived-world-ledger · Domain · Definition
│  ├─ 016 contract.landscape-composition-tile · Contract · Definition
│  ├─ 017 contract.landscape-graph · Contract · Definition
│  ├─ 020 application.pyeongchang-derivation · Application · Projection
│  ├─ 030 infrastructure.derived-world-store · Infrastructure · Persistence
│  ├─ 042 domain.landscape-graph-assembler · Domain · Projection
│  └─ 044 application.landscape-graph-job · Application · Projection
├─ 독립 Synty 경관 처리 [simulation-synty-landscape]
│  ├─ 010 domain.synty-ledger · Domain · Definition
│  ├─ 030 application.synty-job · Application · Projection
│  └─ 040 infrastructure.synty-store · Infrastructure · Persistence
├─ L2 타일 Streaming [simulation-world-streaming]
│  ├─ 010 contract.stream-recipe · Contract · Definition
│  ├─ 020 api.world-stream · Api · Query
│  ├─ 025 api.world-region-summary · Api · Query
│  └─ 030 application.world-stream · Application · Projection
└─ Unity 마지막 성공 상태 로딩 [unity-resilient-world-load]
   ├─ 010 client.last-successful-runtime · ClientAdapter · Presentation
   ├─ 020 client.community-load · ClientAdapter · Query
   ├─ 020 client.public-data-load · ClientAdapter · Query
   └─ 020 client.warehouse-load · ClientAdapter · Query
```

기능 하나만 보려면 `dotnet run --project eng/Ssalddel.CodeMap -- --feature <기능키>`를 사용한다.

## Simulation 세션 생명주기 (`simulation-session-lifecycle`)

- **010 contract.session-create** — [경영SimulationSession생성Request](../../../Ssalddel.Simulation.Contracts/경영SimulationSessionContracts.cs) · Simulation 세션 생성 입력과 초기 World 문맥을 정의한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: 운영 업무 생성 계약이 아니라 결정적 Simulation 세션 입력 계약이다.
- **020 api.session-lifecycle** — [경영SimulationSessionsController](../../../Ssalddel.Simulation.Server/Controllers/경영SimulationSessionsController.cs) · 세션 생성·조회·Tick HTTP 경계를 제공한다.
  - 계층/단계: `Api / Confirm`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: Simulation 실행 모드에서만 조립되며 오류 계약과 기존 route를 보존한다.
- **030 application.session-lifecycle** — [경영SimulationSession생명주기Service](../../../Ssalddel.Simulation.Application/경영SimulationSession생명주기Service.cs) · 세션 생성·조회·Tick·저장·복원을 조율한다.
  - 계층/단계: `Application / Confirm`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 실제 업무 상태를 만들지 않으며 기대 개정과 저장 자료 무결성을 통과한 Simulation 상태만 변경한다.
- **040 domain.session-aggregate** — [경영SimulationSessionAggregate](../../../Ssalddel.Simulation.Domain/경영SimulationSession.cs) · 결정적 세션 상태와 개정·Tick 상태 전이를 소유한다.
  - 계층/단계: `Domain / Tick`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: Aggregate 상태 전이는 Simulation 전용이며 운영 계약·재고·결제를 변경하지 않는다.
- **050 infrastructure.session-store** — [InMemory경영SimulationSessionStore](../../../Ssalddel.Simulation.Infrastructure/InMemorySimulationStores.cs) · 활성 Simulation 세션을 프로세스 수명 동안 보관한다.
  - 계층/단계: `Infrastructure / Persistence`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: 프로세스 내부 저장소이며 durable 저장이나 다중 인스턴스 동기화를 보장하지 않는다.

## 병렬 경영-전투 (`simulation-parallel-battle`)

선행 기능: `simulation-session-lifecycle`

- **010 contract.battle-preview** — [SimulationBattleCreatePreviewRequest](../../../Ssalddel.Simulation.Contracts/SimulationBattleInstanceContracts.cs) · 병렬 전투 생성 미리보기의 서버 입력을 정의한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: 클라이언트는 전투 결과나 수치 보정을 확정하지 않고 예상 World 개정과 안정 ID만 보낸다.
- **020 api.battle** — [SimulationBattlesController](../../../Ssalddel.Simulation.Server/Controllers/SimulationBattlesController.cs) · 병렬 전투 조회·Preview·Confirm·진행 HTTP 경계를 제공한다.
  - 계층/단계: `Api / Confirm`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: 클라이언트가 보낸 안정 ID와 예상 개정만 받아 서버 규칙으로 전투 상태를 확정한다.
- **030 application.battle** — [SimulationBattleInstanceService](../../../Ssalddel.Simulation.Application/SimulationBattleInstanceService.cs) · 전투 Preview·Confirm·진행과 경영 World 합류를 조율한다.
  - 계층/단계: `Application / Confirm`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: 전투 Tick과 경영 WorldTick을 분리하고 완료 결과만 안전한 WorldTick에 합류시킨다.
- **040 domain.battle-state** — [SimulationBattleInstanceState](../../../Ssalddel.Simulation.Domain/SimulationBattleInstances.cs) · 독립 BattleTick·참가·배치·지원·결과 상태 전이를 소유한다.
  - 계층/단계: `Domain / Tick`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: 전투 상태는 SimulationOnly이며 운영 자원이나 실제 인력을 잠그지 않는다.
- **050 infrastructure.battle-store** — [InMemorySimulationBattleInstanceStore](../../../Ssalddel.Simulation.Infrastructure/InMemorySimulationBattleInstanceStore.cs) · 활성 전투와 Simulation 자원 예약을 프로세스 수명 동안 보관한다.
  - 계층/단계: `Infrastructure / Persistence`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: 실제 창고 재고나 인력을 잠그지 않는 process-local Simulation 저장소다.
- **060 unity.battle-presentation** — [BattlePresentationMapper](../../../Ssalddel.Unity/Runtime/Battles/SimulationBattleInstancePresentationModels.cs) · 서버 전투 Snapshot을 경영·전투 카메라와 정보판 표현 상태로 투영한다.
  - 계층/단계: `ViewModel / Presentation`
  - 읽기/쓰기: `SimulationState → None`
  - 부수효과: `None`
  - 경계: 전투 결과·World 상태를 계산하거나 변경하지 않는 순수 Unity 표현 변환이다.

## 1인칭 농장 타이밍 전투 (`simulation-farm-combat-input`)

선행 기능: `simulation-session-lifecycle`

- **010 contract.farm-combat** — [SimulationCombatPerspectiveConfirmRequest](../../../Ssalddel.Simulation.Contracts/SimulationFarmCombatContracts.cs) · 전투 시점·박자·반응 입력의 서버 계약을 정의한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: Unity는 안정 식별자·예상 개정·행동·반응 경과 시간만 제출한다.
- **020 api.farm-combat** — [SimulationFarmSurvivalController](../../../Ssalddel.Simulation.Server/Controllers/SimulationFarmSurvivalController.cs) · 전투 시점·박자 시작·반응 확정 HTTP 경계를 제공한다.
  - 계층/단계: `Api / Confirm`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: Simulation 전용 경로이며 운영 서버 권한·원장을 변경하지 않는다.
- **030 application.farm-combat** — [SimulationFarmSurvivalService](../../../Ssalddel.Simulation.Application/SimulationFarmSurvivalService.cs) · 전투 입력을 현재 Simulation Session aggregate에 전달한다.
  - 계층/단계: `Application / Confirm`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: 운영 업무 상태가 아니라 Simulation Session 상태만 변경한다.
- **040 domain.farm-combat** — [경영SimulationSessionAggregate](../../../Ssalddel.Simulation.Domain/SimulationFarmCombat.cs) · 전투 박자·타이밍 등급·피해·전술 기회를 결정적으로 판정한다.
  - 계층/단계: `Domain / Tick`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `StateMutation`
  - 경계: Unity가 제출한 판정 결과를 신뢰하지 않고 Session aggregate가 최종 결과를 확정한다.
- **050 unity.farm-combat-input** — [FarmCombatInputCommandFactory](../../../Ssalddel.Unity/Runtime/Survival/SimulationFarmCombatPresentationModels.cs) · 서버 전투 상태를 공격 진입·방어·반격 명령 초안으로 변환한다.
  - 계층/단계: `ClientAdapter / Presentation`
  - 읽기/쓰기: `SimulationState → ClientPresentation`
  - 부수효과: `UiStateMutation`
  - 경계: 피해·판정 등급·전술 효과를 계산하지 않고 안정 식별자와 입력 시각만 전달한다.

## Simulation 저장-재생 (`simulation-save-replay`)

선행 기능: `simulation-session-lifecycle`, `simulation-parallel-battle`

- **010 contract.save-request** — [SimulationSessionSaveRequest](../../../Ssalddel.Simulation.Contracts/SimulationSaveReplayContracts.cs) · 세션 저장 식별자와 기대 개정을 정의한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: 저장 자료는 Simulation 상태만 포함하며 운영 원장과 공공데이터 원본을 복제하지 않는다.
- **020 api.save-replay** — [경영SimulationSessionsController](../../../Ssalddel.Simulation.Server/Controllers/경영SimulationSessionsController.cs) · 세션 저장·복원 HTTP 경계를 제공한다.
  - 계층/단계: `Api / Persistence`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 저장 식별자와 기대 개정을 서버가 검증하며 운영 서버 저장 API로 전달하지 않는다.
- **030 application.save-replay** — [경영SimulationSession생명주기Service](../../../Ssalddel.Simulation.Application/경영SimulationSession생명주기Service.cs) · 세션 저장·복원과 전투 저장 자료 결합을 조율한다.
  - 계층/단계: `Application / Persistence`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 검증된 simulation-save.v1·v2 자료만 저장·복원하며 활성 세션을 임의로 덮어쓰지 않는다.
- **040 domain.save-package** — [경영SimulationSessionAggregate](../../../Ssalddel.Simulation.Domain/SimulationSaveReplay.cs) · 세션 Snapshot과 Command log를 봉인한 저장 자료로 만든다.
  - 계층/단계: `Domain / Persistence`
  - 읽기/쓰기: `SimulationState → None`
  - 부수효과: `None`
  - 경계: 재생 hash와 schema가 일치하는 Simulation 자료만 만들며 운영 원장을 직렬화하지 않는다.
- **050 infrastructure.save-store** — [SimulationSessionSaveStore](../../../Ssalddel.Simulation.Persistence/SimulationSessionSavePersistence.cs) · 검증된 세션 저장 자료 JSON과 재생 hash를 Simulation 전용 DB에 보관한다.
  - 계층/단계: `Infrastructure / Persistence`
  - 읽기/쓰기: `SimulationState → SimulationState`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 공유 공공데이터 DB가 아니라 별도 SimulationSession DB만 읽고 쓴다.

## 공공데이터-파생 World (`simulation-world-derivation`)

- **010 domain.derived-world-ledger** — [SimulationWorld파생원장](../../../Ssalddel.Simulation.Domain/SimulationWorldDerivation.cs) · 공간 원본 계보·파생 node·관계·배치 계획을 불변 실행 단위로 정의한다.
  - 계층/단계: `Domain / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: 관측·파생·통계배분·시나리오·장식 근거를 분리하며 추정 위치를 실제 사실로 승격하지 않는다.
- **016 contract.landscape-composition-tile** — [SimulationWorldLandscapeCompositionTileResponse](../../../Ssalddel.Simulation.Contracts/SimulationWorldLandscapeCompositionContracts.cs) · 공간 근거로 조립된 경관 Graph와 의미 기반 Composition 배치를 Unity에 전달한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: Prefab 경로·GUID·상품명은 노출하지 않으며, 응답은 표현 계획이지 운영 사실이나 실제 시설 존재의 확정이 아니다.
- **017 contract.landscape-graph** — [SimulationWorldLandscapeGraphResponse](../../../Ssalddel.Simulation.Contracts/SimulationWorldLandscapeCompositionContracts.cs) · 여러 타일과 Area를 참조하는 하나의 경관 Graph를 Unity에 전달한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: Graph는 표현용 공간 구조이며 Unity의 로드 상태나 운영 업무 상태를 확정하지 않는다.
- **020 application.pyeongchang-derivation** — [평창군공간파생Pipeline](../../../Ssalddel.Simulation.Persistence/PyeongchangWorldDerivationPipeline.cs) · 평창군 공공데이터를 읽어 대표 건물·공간 관계·Unity 타일 계획을 결정적으로 조립한다.
  - 계층/단계: `Application / Projection`
  - 읽기/쓰기: `SharedPublicData → DerivedWorld`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 공유 공공데이터는 읽기 전용이며 건물 도형이나 DEM이 없으면 임의 좌표를 생성하지 않는다.
- **030 infrastructure.derived-world-store** — [SimulationWorld파생원장Store](../../../Ssalddel.Simulation.Persistence/SimulationWorldDerivationPersistence.cs) · 파생 World 원장과 입력·출력 hash를 별도 DB에 멱등 저장한다.
  - 계층/단계: `Infrastructure / Persistence`
  - 읽기/쓰기: `DerivedWorld → DerivedWorld`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: SimulationWorldDerived DB만 변경하며 입력 fingerprint가 다른 같은 식별자는 충돌로 거부한다.
- **042 domain.landscape-graph-assembler** — [SimulationWorldLandscapeGraphAssembler](../../../Ssalddel.Simulation.Domain/SimulationWorldLandscapeAssembly.cs) · Macro·Meso 공간 골격을 156개 의미 모판의 연결·반복 문법으로 결정적으로 조립한다.
  - 계층/단계: `Domain / Projection`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: 실제 도로가 없는 연결은 Scenario 근거를 유지하며, Micro 장식 좌표와 Prefab은 Unity wrapper가 결정한다.
- **044 application.landscape-graph-job** — [SimulationWorldLandscapeCompositionJobShell](../../../Ssalddel.Simulation.Application/SimulationWorldLandscapeCompositionService.cs) · 공간 Layer 준비 상태를 확인하고 네 L2 타일의 경관 Graph를 조립·저장한다.
  - 계층/단계: `Application / Projection`
  - 읽기/쓰기: `DerivedWorld → DerivedWorld`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 자료가 없는 타일은 꾸며내지 않고 대기 상태로 저장하며 Unity Prefab을 해석하지 않는다.

## 독립 Synty 경관 처리 (`simulation-synty-landscape`)

선행 기능: `simulation-world-derivation`

- **010 domain.synty-ledger** — [SimulationWorldSynty경관실행원장](../../../Ssalddel.Simulation.Domain/SimulationWorldSyntyLandscape.cs) · 공간 출력과 Synty·URP 대장 개정을 결합한 경관 실행 결과를 정의한다.
  - 계층/단계: `Domain / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: 경관 실행은 표현 계획이며 공간 원본·법정동·Simulation 상태를 변경하지 않는다.
- **030 application.synty-job** — [SimulationWorldSynty경관JobShell](../../../Ssalddel.Simulation.Application/SimulationWorldSyntyLandscapeJobShell.cs) · 공간 실행을 읽어 Synty·URP 경관 계획을 만들고 별도 실행 원장으로 저장한다.
  - 계층/단계: `Application / Projection`
  - 읽기/쓰기: `DerivedWorld → DerivedWorld`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: Synty 원본 경로나 Prefab 이름을 업무 권위로 사용하지 않고 공간 출력 hash를 입력으로 삼는다.
- **040 infrastructure.synty-store** — [SimulationWorldSynty경관Store](../../../Ssalddel.Simulation.Persistence/SimulationWorldSyntyLandscapePersistence.cs) · Synty 경관 실행과 그래픽·배치·거부 결과를 별도 파생 DB에 저장한다.
  - 계층/단계: `Infrastructure / Persistence`
  - 읽기/쓰기: `DerivedWorld → DerivedWorld`
  - 부수효과: `PersistentRead | PersistentWrite`
  - 경계: 공간 실행과 별도 fingerprint를 사용하며 Synty 대장 변경이 공간 원장을 다시 쓰게 하지 않는다.

## L2 타일 Streaming (`simulation-world-streaming`)

선행 기능: `simulation-world-derivation`

- **010 contract.stream-recipe** — [SimulationWorldStreamRecipeResponse](../../../Ssalddel.Simulation.Contracts/SimulationWorldStreamingContracts.cs) · L2 타일 활성·준비 범위와 사전 적재 규칙을 전달한다.
  - 계층/단계: `Contract / Definition`
  - 읽기/쓰기: `None → None`
  - 부수효과: `None`
  - 경계: Recipe는 제공 범위와 로드 정책이며 Unity가 전체 타일을 동시에 생성하라는 명령이 아니다.
- **020 api.world-stream** — [SimulationWorldStreamingController](../../../Ssalddel.Simulation.Server/Controllers/SimulationWorldStreamingController.cs) · 타일 Recipe·Manifest·Layer·객체 Projection 조회 경계를 제공한다.
  - 계층/단계: `Api / Query`
  - 읽기/쓰기: `SimulationState | DerivedWorld → None`
  - 부수효과: `None`
  - 경계: 조회와 eligibility Preview는 타일이나 업무 상태를 생성·확정하지 않는다.
- **025 api.world-region-summary** — [SimulationWorldRegionSummaryController](../../../Ssalddel.Simulation.Server/Controllers/SimulationWorldRegionSummaryController.cs) · 지역·타일별 대표 정보와 가까운 공개 객체의 제한된 상세정보를 제공한다.
  - 계층/단계: `Api / Query`
  - 읽기/쓰기: `DerivedWorld → None`
  - 부수효과: `None`
  - 경계: 요약 응답에는 상호명을 넣지 않고 명시적인 공개 상세 조회에서만 공개 공공데이터 상호명을 반환한다.
- **030 application.world-stream** — [SimulationWorldStreamingService](../../../Ssalddel.Simulation.Application/SimulationWorldStreamingService.cs) · 카메라·플레이어 경계 접근에 필요한 타일 Recipe와 Manifest Projection을 제공한다.
  - 계층/단계: `Application / Projection`
  - 읽기/쓰기: `DerivedWorld → None`
  - 부수효과: `None`
  - 경계: 자료가 없는 DEM·배치 좌표·URL을 꾸며내지 않고 명시된 제공 범위만 투영한다.

## Unity 마지막 성공 상태 로딩 (`unity-resilient-world-load`)

선행 기능: `simulation-world-streaming`

- **010 client.last-successful-runtime** — [LastSuccessfulLoadRuntime`2](../../../Ssalddel.Unity/Runtime/Application/LastSuccessfulLoadRuntime.cs) · 최초 조회와 새로고침을 구분하고 마지막 성공 Snapshot을 유지한다.
  - 계층/단계: `ClientAdapter / Presentation`
  - 읽기/쓰기: `None → ClientPresentation`
  - 부수효과: `UiStateMutation`
  - 경계: 서버 상태를 만들거나 변경하지 않고 실패 시 마지막 성공 표현만 보존한다.
- **020 client.community-load** — [CommunityMarketSquareLoadCoordinator](../../../Ssalddel.Unity/Runtime/Community/CommunityMarketSquareModels.cs) · 커뮤니티 광장 Snapshot 조회와 마지막 성공 상태 조정을 연결한다.
  - 계층/단계: `ClientAdapter / Query`
  - 읽기/쓰기: `OperationalState → ClientPresentation`
  - 부수효과: `NetworkCall | UiStateMutation`
  - 경계: 공개 Projection만 읽고 커뮤니티 원장이나 서버 개정을 변경하지 않는다.
- **020 client.public-data-load** — [PublicDataHallLoadCoordinator](../../../Ssalddel.Unity/Runtime/PublicData/PublicWorldMapModels.cs) · 공공데이터 World Map Snapshot 조회와 마지막 성공 상태 조정을 연결한다.
  - 계층/단계: `ClientAdapter / Query`
  - 읽기/쓰기: `SharedPublicData → ClientPresentation`
  - 부수효과: `NetworkCall | UiStateMutation`
  - 경계: 출처와 자료 상태가 있는 조회 결과만 표현하며 공공데이터 원본을 수정하지 않는다.
- **020 client.warehouse-load** — [WarehouseWorldLoadCoordinator](../../../Ssalddel.Unity/Runtime/Warehouse/WarehouseWorldModels.cs) · 창고 World Snapshot 조회와 마지막 성공 상태 조정을 연결한다.
  - 계층/단계: `ClientAdapter / Query`
  - 읽기/쓰기: `OperationalState → ClientPresentation`
  - 부수효과: `NetworkCall | UiStateMutation`
  - 경계: 권한 적용된 창고 Projection만 표현하며 Unity가 입출고 완료를 확정하지 않는다.

## 진단 요약

- 오류: 0
- 경고: 5
- 일반 공개 타입의 미표기는 경고이며, 필수 단계·권위 위반·오래된 생성 파일만 검증을 차단한다.
