# Farm 경작 세계 발현 E5 공간·배치 연구 r1

## 식별과 상태

- 연구 고유 식별자: `study:farm-crop-cycle:spatial-placement.r1`
- 대상 PlayableLoop: `playable-loop:farm-crop-cycle.v1`
- 대상 WI: `WI-FARM-01`, `WI-FARM-02`, `WI-FARM-03`, `WI-FARM-04`
- 연구 revision: `farm-crop-cycle.spatial-placement-study.r1`
- 상태: `Accepted`
- 재결속 기획: [Farm 경작 세계 발현 E5](Farm경작세계발현E5.md), `farm-crop-cycle.design.r1`
- 검토일·근거: 2026-08-30 저장소·후보 인계·실제 Prefab/meta 파일 조사와 사용자의 E5 계획 구현 승인. 작성 검토는 기획 담당, 소비·통합 검증은 개발 담당이 맡는다.
- 승인 범위: 배치 의도, 기존 수치의 출처, 호환 변환 경계와 시험·측정 기준. **자산의 물리 적합성·실제 Scene·사람 시각·패턴 승인·E5 완료는 미검증**이다.

## 연구 질문과 플레이어 사용 문맥

기존 감자 밭 하나에서 밭갈이·파종·관리·수확을 했을 때 무엇이 바뀌었는지 보이고, 플레이어가 입구·마당·밭 사이를 막힘 없이 오갈 수 있는가? 강변 H2 후보를 실제 셀에 연결하면서 기존 지도·Sky·실외/실내 배치·LH의 책임을 유지할 수 있는가?

이번에는 아름다운 전체 월드의 최종 답이나 여러 패턴 중 최적안을 고르지 않는다. [Q-297~339](PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md)의 첫 강변 H2 골격을 검토 인스턴스로 만들고, 기존 Farm의 네 WI가 그 안에서 발현되는지를 확인한다.

## 현재 코드·문서·시험 재고

| 근거 | 실제 확인 내용 | 아직 증명하지 못한 것 |
| --- | --- | --- |
| [Farm 생산 모판](../../../eng/world-seedbeds/wi-spatial-seedbeds/definitions/farm-production.v1.json) | `production-plot`, WI01~04, WorkArea 1 slot, 통행·파종·관리·수확 능력, 변환 허용 범위 | 실제 Prefab·World 배치·WI 실행 |
| [Farm Domain](../../../Ssalddel.Simulation.Domain/UnityPackage/Runtime/SimulationFarmSurvival.cs) | 예약·Tick·Tilling/Sowing/CropCare/Harvesting 상태 전이, Lot과 기존 Save 명령 기록 | 모든 WI의 공통 Pipeline 완료 기록·둘째 재배 안전성 |
| [Farm Application](../../../Ssalddel.Simulation.Application/RuntimeCore/SimulationFarmSurvivalService.cs) | WI04~06 공통 Pipeline, WI01~03 Aggregate 직접 호출 | 네 WI 전체 같은 문맥 인계, 실제 권위 위치 전달 |
| [Local Runtime](../../../Ssalddel.Simulation.Application/RuntimeCore/LocalSimulationRuntime.cs) | Farm Preview/Confirm Service 호출 | 실제 배치 인스턴스·Anchor·표현 연결 |
| [Farm 공급 시험](../../../Ssalddel.Simulation.Tests/SimulationWorldInteractionFarmSupplyTests.cs) | 01~03 순서 실행, 수확 생산 규칙·공간 예약·Save/Replay 시험 소스 | Hub·Market 등 공급선 문맥을 제거한 단독 시작과 Unity 실행 |
| [최신 통합 검토](../../Reports/전문산출물-개발통합검토-2026-08-30.md) | Farm 산출물16·자산12 hash 일치, 기준선20 중 지침·상태4 drift 보고 | net10 후보의 현행 계약 변환과 실제 실행 |

원 후보 위치는 `C:/Users/user/.codex/worktrees/cba3/Hongdal/spatial-support/farm-h2/`다. `HANDOFF.md`, `baseline.json`, `delivery-manifest.json`, `asset-candidates.json`을 읽었으며 원본을 수정하지 않았다. 독립 후보 ID는 `candidate-pattern:farm-riverside-practical-h2`, 생성 판본은 `farm-riverside-h2.trial.r1`이다. 보고된 Debug/Release 50/50은 독립 검사 실행기 결과이며 이번 문서 작업에서 재실행한 결과가 아니다.

Unity의 `Assets/Ssalddel/Presentation/World/업무영역플레이폐루프Synty표현Modules.cs`에는 `synty-loop:farm-crop-cycle.v1`과 `farm.crop.prepare/plant/grow/harvest` Slot이 있다. 이는 의미 대장이지 실제 배치기가 아니다. 기존 공식 Scene·Player·카메라·지면은 재사용하되 이번 조사는 Scene을 열거나 저장하지 않았다.

## 변경하지 않는 경계

- 실제 상태 권위는 Session Aggregate, 운영 상태는 운영 서버에 있다. Unity·Prefab·배치 후보·Animator는 작물 성장·수확·WorldRevision을 변경하지 않는다.
- 현행 Contracts/Domain/Application은 `netstandard2.1`, C#9다. Unity 소비자를 위해 대상 프레임워크를 net10으로 올리지 않는다.
- Farm 개별 WI·모판·배치 인스턴스·LH 생명주기·E 증거의 고유 식별자와 revision을 구분한다.
- Prefab path/GUID는 Unity 시각 자산 결속에만 둔다. 공통 Core는 기존 의미 키·자산 계열·CompositionKey를 사용한다.
- H2 고정 접근 골격을 유지하되 플레이어를 정해진 순서로 걸어야만 작업 가능한 상태로 만들지 않는다.
- 실내 가구·수로·물의 생성·NPC 생활을 이 후보에 추가하지 않는다. Barn은 외형 랜드마크와 작업마당의 배경이다.

## 비교한 대안과 선택

| 대안 | 장점 | 문제·판정 |
| --- | --- | --- |
| net10 후보 DLL을 Unity에 직접 참조 | 후보 코드 이동이 적음 | 현행 프레임워크·문법·API·불변 자료형 경계 불일치로 사용하지 않음 |
| 후보를 외부 사전 생성 전용으로 고정 | 도구 격리가 쉬움 | 이번 동일 Core 소비·동결 계보 확인을 위한 런타임 변환과 중복 계산 소유가 남으므로 첫 구현 기본안으로 삼지 않음 |
| 필요한 순수 계산만 호환 소스로 이식하고 현행 형식으로 변환 | 현행 소비 빌드·공통 시험·명확한 계보를 유지 | **선택**. 값·역할·동결 배치를 재추첨하지 않고 명시적 Adapter로 변환 |

`ReusedSurfacePlacement.cs`의 장기 복제본을 추가하지 않는다. 현행 표면 계산과 의미를 맞춰 공유 가능한 순수 계산 경계 또는 Adapter를 사용한다. 변환 결과가 현행 배치 계약과 맞지 않으면 잘못된 값을 숨기지 않고 해당 인스턴스를 거부한다.

## 선택한 H 기준선과 측정값 출처

| 수준·요소 | 이번 기준 | 측정·검증 경계 |
| --- | --- | --- |
| H1 생산 밭 | 토양 1개, 기존 감자 생산 Fixture의 물리 면적 100㎡ | H2 후보의 장식 외곽 면적으로 생산량을 다시 계산하지 않는다. 실제 경작 면적과 표시 면적의 대응을 별도로 기록한다. |
| H1 공간 모판 | WorkArea 1 slot, 폭/깊이 최소14/12m·선호28/24m·최대56/48m, 균일 Scale·0/90/180/270도 | 모판의 기존 허용 범위다. 100㎡ 생산 면적과 통행 포함 공간 외곽을 혼동하지 않는다. |
| H2 접근 골격 | 외부 진입 → Barn 작업마당 → 내부 작업로 → 외부 출구, 밭·수원 접근점 | 통행 보호 범위와 주요 H1 접근을 보존하고 실제 Player Collider로 확인한다. |
| 주 하천·자연 여백 | 후보의 단일 하천 보존 마스크·자연 완충 영역 | 배치 금지 문맥이다. 실제 강물 양·흐름·관수 공급을 만들지 않는다. |
| 합성 후보 지형 | 평지·완만한 노이즈·낮은 단차 3표본과 급경사 거부 표본 | 원 후보의 80m 셀과 Bounds는 시험 값이다. 현행 셀 크기·Player 규격·Prefab 치수의 승인 값이 아니다. |
| 노동·성장·생산 | 기존 Preview/Rule 출력 사용; Fixture 종자·물 비용 각각1, 직접 작업1Tick, 기존100㎡×3kg/㎡ 수확 기준 | 기존 코드값의 참조이지 새 게임 조정이 아니다. 환경·손실 계수도 기존 규칙 그대로 적용한다. |

후보의 경사5도·통로12도·통로폭3m 등 `Policy` 값은 독립 시험 조정값으로 보존하고 실제 게임 한계로 승격하지 않는다. 실제 표현 결속 전 개발·공간 담당은 현행 Player 이동 설정, 지지 Collider, Prefab pivot/Scale/활성 Bounds를 읽어 측정 표를 작업 명세에 남긴다. 통행 여부는 실제 이동/충돌로 판정한다. 허용 수치가 기존 설정에 없으면 임의 상수를 공통 계약으로 만들지 않고 그 대상만 연구 피드백으로 반환한다.

## Cell·좌표·H·CompositionKey·hash 변환

1. 승인된 검토 세계의 실제 `CellStableId`, `CellX/CellY`, `OwnerCellStableId`, 원점·축·단위, H1/H2 소유 식별자, 지도 계획 hash와 SourceWorldRevision을 변환 입력으로 받는다. 후보의 `SourceWorldRevision=0`을 실제 Session 값으로 간주하지 않는다.
2. 후보 X/Z·Bottom·Yaw·AABB를 실제 로컬 위치·회전·균일 Scale로 변환한다. 시험 AABB 중심을 Prefab pivot이라고 가정하지 않으며 측정된 pivot과 외곽 범위를 사용한다.
3. H1 생산 구획은 기존 모판의 능력과 같은 실제 대상에 결속한다. H2는 H1 배치의 관계를 소유하고, 상위 AreaSet은 기존 `area-set:sim:pyeongchang:farm-production.v1` 문맥을 유지한다. 합성 Fixture ID를 공용 H 대장에 자동 등록하지 않는다.
4. 기존 `farm:감자밭 두렁:A/B/C` 허용 키와 후보 의미 키는 명시적으로 매핑한다. resolver가 해석하지 못하는 키는 오류·검토 필요로 남기며 이름이 비슷한 프리팹을 임의 선택하지 않는다.
5. `Simulation세계자산배치Plan`을 현행 정규형으로 봉인한다. 원 후보 JSON ResultHash, 변환 revision, 입력 지도/후보 hash, 출력 계획 hash를 각각 계보에 남긴다. 후보 hash를 `AssetPlacementPlanHashSha256`에 그대로 넣지 않는다.
6. [기존 분리 서비스](../../../Ssalddel.Simulation.Application/SimulationWorldAssetPlacementPlanPartitioning.cs)의 `Partition`과 [LH 수명주기](../../../Ssalddel.Simulation.Application/SimulationLhAssetPlanLifecycleService.cs)의 `Prepare/Transition`을 사용한다. 분리하거나 활성화할 때 새 Compose로 위치·Seed·자산을 다시 선택하지 않는다.

변환에 없는 자산·불명확한 소유·잘못된 단위·손상 hash는 거부한다. 변경된 지침 네 파일은 실제 변경 내용을 조율하되 원 후보 기준 hash를 자동 덮어쓰지 않는다.

## Synty 후보와 같은 revision 상태 표현

Unity 기준 루트는 `C:/Users/user/ssalddel/`이다. 다음 경로는 모두 `Assets/Synty/PolygonFarm/Prefabs/` 아래의 실제 파일이며 2026-08-30 Prefab와 `.meta` SHA-256을 읽었다. 파일 존재·hash는 활성 Renderer/Collider·적정 Scale·색상·리타기팅 성공이 아니다.

| 역할·표현 슬롯 | 주 후보 | 대체·fallback 기준 |
| --- | --- | --- |
| Barn 랜드마크 `candidate:farm.red-barn` | `Buildings/SM_Bld_Barn_01.prefab` | `_02`는 비교 후보. 붉은 외관·Bounds·접근을 확인한 판본만 사용한다. 라벨 primitive는 검토용이며 최종 자산 증거 아님. |
| 경작 토양 `farm.crop.prepare` | `Environments/SM_Env_Dirt_Rows_01.prefab` | 기존 흙 지면+상태 Outline/UI는 권위 확인용 대체. 실제 Synty 밭 표현 완료로 계산하지 않는다. |
| Growing `farm.crop.plant/grow` | `Plants/SM_Prop_Plant_Potato_01_S.prefab` | 실제 상태 문구와 승인된 정적 식물 표현을 사용한다. 미검증 Prefab 자동 교체 금지. |
| HarvestReady `farm.crop.grow` | `Plants/SM_Prop_Plant_Potato_01_L.prefab` | 작물 모델의 준비 상태와 별개로 수확 가능 문구는 권위 결과를 읽는다. |
| 수확 Lot `farm.crop.harvest` | `Plants/SM_Prop_Box_Potato_01.prefab` | 상자는 현장 수확량의 대표 표현이며 집하·포장 완료를 뜻하지 않는다. Lot ID·수량·단위는 상태 카드가 표시한다. |
| 수원 접근 배경 `candidate:farm.water-access` | `Props/SM_Prop_Well_01.prefab` | 접근점만 준비하며 관수·채수 WI를 생성하지 않는다. 생육 관리 물은 기존 자원 원장에서 읽는다. |

주·대체 자산을 구분하는 파일 fingerprint는 다음과 같다. 이 값이 바뀌면 해당 후보를 다시 검토한다.

| 파일 | Prefab SHA-256 | meta SHA-256 |
| --- | --- | --- |
| Barn01 | `727ED17B903B0D7BBD1F25A2D7636F377637B77C606FC2EF0A70696B76813C59` | `A0AF0C55AF03190724C7D8CA4153F271D6FE3BB25ADCC2A5C0815A4D1B406D10` |
| Barn02 | `F8E58CD74E67D91714AF4AA4C90029609D646BB7DEE1B0FA75ED7D78949DCB1A` | `286C77F593D6AB80559184296D7653740DB8B147EA616678A2E077AB21D1EBF6` |
| DirtRows01 | `1C9876DE7FE7B6F3800617ECB0B23C2206152D4AF1B7C6065193E7774356E15A` | `8A15007ED17215972969BB5160F8BF65ED27F8E6CE3EB035DE011040E57D7F30` |
| PotatoS | `2D5093E764F6F66C08EC2C862ECDF250745B66694CE5278622083E3C9FD12912` | `EE7CDCB2404C218F97697C60B6E1299A307DDE102B9F52C1E54C1EBFE917BD70` |
| PotatoL | `FC01F89A96545D8FBA023FCAE7BE54F4EAE5330306A46519D52D6F3C945FF627` | `D960A73F1FB4EB2A5A55A3F8045C65B7FF0A889771AABECA8CA1085CDBB98703` |
| PotatoBox | `A128993CF0644A5988A537A0196DC69DCD36619538FC499E01ED7F5A3377C583` | `374ED7092770D129BFC362BAA94068F24CE76EADC5EE3B6AEE29A0498306011B` |
| Well01 | `FAA41D302F4EE21B20BD1A01E596CFE4345A80D08176793F36BD802F53B59B4D` | `4B63C1EE1C6DF12A0B61ADF77D929955EFF62673ADF6382AA68E8F2D9D32A17D` |

`farm.crop.*`는 기존 표현 Slot이고 `candidate:*`는 미승인 후보 의미 키다. 둘을 GUID·공통 VisualKey와 혼동하지 않는다. E4 작업 명세에 실제 시각 자산 대장의 해석 키와 매핑을 결속한 뒤 E5 배치를 진행한다. 자연 풀은 선택 장식으로 생략 가능하고 통행 Collider를 불필요하게 추가하지 않는다.

## 애니메이션 최소 기준선

- 대상은 기존 플레이어 Actor 하나다. 기존 이동·1/3인칭 전환을 유지하고 새 분대나 Farmer Actor를 중복 생성하지 않는다.
- 작업 역할은 기존 `Tilling`, `Sowing`, `CropCare`, `Harvesting`에 대응한다. `ActionCue`는 해당 작업의 준비·진행·완료·취소 상태와 기존 PresentationKey를 사용한다. 밭갈이 표현 키가 없으면 대장 결속에서 명시하며 권위 ActionCode를 바꾸지 않는다.
- 첫 E5의 필수 판독은 선택된 밭, 진행 중 Task, 완료된 토양/작물/Lot, 중단·거부 사유다. 기존 정적 Actor·작업 진행 표시·결과 모델 전환을 최소 대체 표현으로 허용한다. 도구가 지면에 실제 접촉하는 애니메이션 완료는 주장하지 않는다.
- 검 전투 Clip을 농사 동작으로 임의 사용하지 않는다. 신규 Clip·손 접촉 Window·양손 IK·root motion을 이번 필수 범위에 넣지 않는다.
- 기존 동작을 사용하면 Actor/Rig/Avatar/Controller의 현재 판본을 기록하고 중단·완료 후 기존 Idle/이동 복귀를 확인한다. 새로운 뼈 작성자를 덧붙이지 않으며 Task 완료는 AnimationEvent가 아닌 권위 Tick이 결정한다.
- 소리·FX 누락은 사실대로 기록한다. 새 음원 제작·효과음 서비스 호출이나 음향 완료 선언은 하지 않는다.

## Logic·Presentation·H·Save 영향과 의존

Logic E4 문맥과 Pipeline 연결은 실제 상태 기록을 소유하고, Presentation E5는 그 결과의 읽기·배치만 소유한다. 지도·Sky·실외배치·실내배치는 [지도구성과 세계자산배치 분리](../지도구성과세계자산배치분리.md)의 기존 순서를 따른다. Sky는 동결된 기존 Fixture 상태를 사용하며 별도 공공 API를 호출하지 않는다. Barn 실내는 현재 범위에 없으므로 실내 계획은 명시적인 빈/비적용 결과로 통과시키고 배치 엔진을 새로 만들지 않는다.

Save는 실제 토양·재배·작업·Lot과 명령 기록을 보존한다. 표현 세계 정의는 생성 시점의 후보·변환·지도·배치 판본을 고정한다. LH 활성/캐시/해제와 시각 자산 변경이 WorldRevision 또는 수확량을 변경할 수 없다. 별도 전문 담당의 Farm 배치 결과는 개발 통합 담당이 공유 계약 소비·시험을 확인한 후 인수한다.

## 자동시험·실제 화면 수용 기준

1. 현행 소비 프로젝트 빌드가 netstandard2.1/C#9와 Unity 소비 경계를 유지한다. 독립 후보 소스를 복사한 것만으로 통합 완료라고 하지 않는다.
2. 같은 후보·지도·Seed·셀 문맥은 같은 변환 결과와 정규형 hash를 만든다. 입력 순서·문화권, 손상 hash·불명확한 키·축/단위·소유 누락을 시험한다.
3. 기존 평지·완만한 노이즈·낮은 단차 표본을 호환 변환하고 급경사·보존 영역 침범·통로 차단은 이유를 남겨 거부한다. 새 지형 평탄화나 Q-316 전체 적응 탐색으로 우회하지 않는다.
4. Partition·LH Prepare/Activate/Cache/Release 후 위치·Seed·자산·계획 hash·SourceWorldRevision이 유지된다. 반복 활성화가 같은 배치 객체를 중복 생성하지 않는다.
5. 독립 Farm Session에서 01→02→03→04를 실제 Core로 실행하고 Preview 무변경·Confirm 멱등·예약/취소·완료 Effect·행위 기록·현재 권위 위치·다음 선택을 시험한다.
6. 작업 중/수확 후 저장·복원·Replay hash와 결과 재조회를 검증한다. 같은 밭의 둘째 파종을 허용하는 기존 Preview 계약을 보존해 고정 재배 ID·수확 Lot 충돌을 교정하고, 첫 주기의 Lot·작업 계보가 유지되는 두 번째 주기까지 회귀한다. 새 비용·자동 성장·대체 재배 규칙을 만들지 않으며 다음 선택 버튼만 보여 주고 완료로 처리하지 않는다.
7. 실제 Prefab 활성 Renderer·Collider·Bounds·지면 지지·기준점과 Player 접근을 측정한다. [공통 배치 반복 검토 도구](../공통배치반복검토도구.md)의 검사를 재사용하고 자동 보정은 승인된 Wrapper 안에서만 수행한다.
8. canonical `SimulationWorldShell`에서 실제 Farm Session 네 WI 자동 실행과 결과 표시, 저장 후 재진입을 검증한다. 입구·마당·밭·출구 접근, 지원 시점의 충돌·가림, 낮/밤 상태 판독을 확인하고 대표 Game View PNG·Console 결과를 보존한다.

공통 표현 검증 프로필에는 GroundSurface·Object/Actor·상호작용·카메라 조건을 적용하고 실제 대장의 모듈 식별자로 결속한다. 이 문서에 임의 모듈 ID를 추가하지 않는다. 같은 WI·Command·WorldRevision 인계 검사도 별도로 통과해야 한다.

검토 인스턴스의 E5와 패턴 재고의 `ApprovedReference`는 다르다. Q-320의 최소 세 시각 승인 표본 조건은 패턴 재사용 승인 때 유지한다. 이번 실제 배치 하나가 통과해도 전체 H2 패턴을 자동 승인하거나 E6/E7으로 올리지 않는다.

## 무효화·반환 조건

- 기존 Farm ActionCode·비용·시간·성장·수확·취소 규칙을 바꿔야 한다면 관련 Logic E1부터 기획 검토를 다시 연다.
- 후보나 자산 fingerprint·지도/배치 형식·H 능력·좌표/Scale 의미가 바뀌면 그 변환·표현 기준선만 다시 동결한다.
- 실제 측정이 후보 시험 외곽과 다르거나 문·통로·Player 이동을 보장하지 못하면 해당 자산/인스턴스의 Presentation E4를 다시 열고 실제 E5 승격을 보류한다.
- 연구에 없는 공공데이터·다른 영역·새 건물/실내·새 애니메이션이 필수가 되면 개발이 조용히 추가하지 않고 기획으로 반환한다.
- 기존 50/50·파일hash·기획 승인을 현재 실행 증거로 대체하지 않는다. 구현과 검사 결과는 작업 명세·EvidencePackage·개발 통합 승인 기록이 소유한다.
