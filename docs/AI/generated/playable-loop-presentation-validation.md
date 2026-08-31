# PlayableLoop 공통 표현 검증 모듈 상태

> `eng/execution-ledgers/playable-loop-presentation-validation-modules.json`와 `eng/execution-ledgers/playable-loops.json`에서 자동 생성된다. 직접 수정하지 않는다.

대장·연결 검증은 Unity 실행이나 E 승격이 아니다. 파일 참조는 구현 위치이며 실제 동작·시험 통과를 뜻하지 않는다. `unity:` 참조는 -UnityProjectRoot를 제공해야 파일 존재를 대조한다. 모듈별 Passed는 기존 EvidencePackage의 대상·단계·판본·파일 hash를 확인하며 실제 품질 평가는 별도다.

- 모듈 대장: `playable-loop-presentation-validation-modules.r12`
- 공통 모듈: `8`
- 조건 모듈: `10`
- 적용 PlayableUnit: `20`

## 공통 관문

| E | 모듈 | 자동화 | 차단 |
| --- | --- | --- | --- |
| E1 | `presentation-requirement-contract` 플레이어 표현 요구·오류·귀환 계약 | Automated | Blocking |
| E2 | `presentation-projection-lifecycle` 상태 투영·표현 연결·수명 코드 | Automated | Blocking |
| E3 | `presentation-automated-regression` 표현 회귀·도구 건전성 검사 | Automated | Blocking |
| E4 | `presentation-binding` 표현 상태·H 기준점·E5 준비 인계 검증 | Automated | Blocking |
| E5 | `visual-source-bounds` 시각 자산·Renderer Bounds 준비 검증 | Automated | Blocking |
| E6 | `player-scale-spacing` Player 대비 크기·간격 검증 | Automated | Blocking |
| E6 | `state-difference-readability` 상태별 표현 차이 검증 | Mixed | Blocking |
| E7 | `actual-camera-input-result-return` 실제 카메라·입력·결과·귀환 검증 | ManualEvidence | Blocking |

## 기능 프로필

| 폐루프 | 기능 | 선택된 모듈 |
| --- | --- | --- |
| `playable-loop:nature-shelter-foundation.v1` | GroundSurface, Building, CameraOcclusion | actual-camera-input-result-return, building-foundation-entry, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:nature-twilight-return.v1` | GroundSurface, Actor, CameraOcclusion | actor-grounding-identification, actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:nature-night-day2.v1` | Building, Interior, CameraOcclusion, Atmosphere, DirectionalLighting | actual-camera-input-result-return, atmosphere-authority-binding, building-foundation-entry, camera-occlusion, directional-light-surface-readability, interior-clearance, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds, weather-camera-audio-exposure |
| `playable-loop:nature-workbench-foundation.v1` | GroundSurface, WorkZone, CameraOcclusion | actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds, work-zone-readability |
| `playable-loop:nature-field-supply-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-basic-herbal-recovery.v1` | GroundSurface, WorkZone, CameraOcclusion | actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds, work-zone-readability |
| `playable-loop:nature-base-reflection.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-building-learning.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-regional-threat-recovery.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-tactical-self-navigation.v1` | GroundSurface, Actor, CameraOcclusion | actor-grounding-identification, actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:farm-crop-cycle.v1` | GroundSurface, WorkZone, CameraOcclusion | actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds, work-zone-readability |
| `playable-loop:farm-pack-store-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:farm-barracks-defense.v1` | GroundSurface, Actor, Building, CameraOcclusion | actor-grounding-identification, actual-camera-input-result-return, building-foundation-entry, camera-occlusion, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:farm-player-placement.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:hub-inbound-putaway.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:hub-outbound-ready-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:town-order-consume-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:town-arcana-context.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:city-demand-service-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-camp-visitor-stay.v1` | GroundSurface, Actor, Interior, CameraOcclusion | actor-grounding-identification, actual-camera-input-result-return, camera-occlusion, interior-clearance, player-scale-spacing, presentation-automated-regression, presentation-binding, presentation-projection-lifecycle, presentation-requirement-contract, state-difference-readability, surface-clearance, visual-source-bounds |

## 단계별 최소 구현 책임

| E / 모듈 | 입력 → 출력 | 재사용 구현 참조 | 시험 참조 |
| --- | --- | --- | --- |
| E1 / `presentation-requirement-contract` | 승인된 플레이어 약속, 표시할 권위 상태·오류·다음 선택 → 표현 계약과 제외 범위 | repo:docs/AI/Presentation단계별최소모듈-2026-08-31.md | repo:eng/tests/playable-loop-presentation-validation.ps1 |
| E2 / `presentation-projection-lifecycle` | 동일 Session 상태 사본, 안정 ID·자료/표현 판본, 기존 Synty 자산·동작 목록과 공통 Adapter의 가벼운 파일 선행 조회 → 불변 표현 준비상태, 추가·변경·제거·거부 판정, 잠정 후보·기존 구현 경로: 실제 적합성 확정이나 전체 팩 실측 선행조건 아님 | repo:Ssalddel.Unity/Runtime/PresentationContracts/Reconciliation/StableIdReconciliation.cs<br>repo:eng/execution-ledgers/playable-loop-synty-expression-modules.json<br>repo:docs/AI/Presentation단계별Synty자산조사-2026-08-31.md | repo:Ssalddel.Unity.Tests/StableIdReconciliationTests.cs<br>repo:eng/tests/presentation-synty-survey.ps1 |
| E3 / `presentation-automated-regression` | 표현 코드, 시험 입력·표본, 기준 판본 → 자동시험 결과, 실패 원인·미실행/실제화면 구분 | repo:eng/common/presentation-module-bindings.ps1 | repo:eng/tests/playable-loop-presentation-validation.ps1<br>repo:Ssalddel.Unity.Tests/StableIdReconciliationTests.cs |
| E4 / `atmosphere-authority-binding` | 세계 대기 프로필, 규칙 revision, 권위 시계 원천, 세계 공통 범위 → 세계 대기 상태·시간 원천 결속 검증 결과와 미해결 사항 |  |  |
| E4 / `presentation-binding` | 권위 상태 사본, H 기준점 또는 사유 있는 NotApplicable, 플레이어 판독 순간, VisualKey, 주·대체·fallback 자산 후보, 배치 의도 또는 사유 있는 NotApplicable, InteractionAnchor 의도, 후보 revision·fingerprint, E5 준비 상태, 대상 WI·역할별 Prefab/재질·Clip/Rig/Avatar 조사와 기존 동일 판본·사용 문맥 근거 → presentationE4Preparation에 주·대체 후보와 외형·크기·pivot·Renderer/Collider·접촉/전환의 필요한 범위 및 미해결 사항 기록, 그대로 재사용 / 연결·설정 보완 / 형상·리깅·동작 가공 필요 / 신규 제작 필요 / 미검사: 근거와 사용조건 확인을 분리, 파일 확인·격리 미리보기·실제 대상 검증을 구별. 동일 자산/판본/문맥 근거 재사용·변경분만 재검증, 비자산 범위는 사유 있는 NotApplicable, D389 연결 사전검사 Ready/Conditional/Blocked 및 대상·후보/상태판본·근거·누락·담당·가장 이른 E. 결과 없음은 미검증, Ready도 적용/E5 완료 아님. 적용 직전 현재 판본으로 재검사 | repo:eng/execution-ledgers/manage-e7-vertical-work-order.ps1<br>repo:docs/Architecture/플레이폐루프Synty표현모듈체계.md<br>repo:docs/AI/Presentation단계별Synty자산조사-2026-08-31.md<br>repo:docs/AI/Presentation-E4-E5연결사전검사-2026-08-31.md<br>repo:Ssalddel.Unity/Runtime/PresentationContracts/표현연결Preflight.cs<br>repo:Ssalddel.Unity/Runtime/Farm/Farm수확표현연결Preflight.cs | repo:eng/tests/presentation-synty-survey.ps1<br>repo:Ssalddel.Unity.Tests/표현연결PreflightTests.cs |
| E5 / `visual-source-bounds` | Prefab 또는 대체 표현, 활성 Renderer, Collider, Bounds → 동결 후보의 권위 상태·World·실제 Prefab/대체 표현 결속 및 Renderer/Collider/Bounds 검증. 파일 존재·E4 조사만으로 E5 통과하지 않음, D389 사전검사는 기존 조회/표시/해제 관측만 읽는다. 건전성 미관측은 Conditional, 확인된 결손은 Blocked. 실제 WorldVisualCatalog 조회·소유 lease 적용/해제·지지/접근은 분야별 실행 증거로 별도 확인, D390: 전체 배치 기준을 소비하는 작은 대상/행동/결과 범위와 부모 폐루프 판정을 분리. 필수 상태·접근·통행·컴포넌트/간섭은 차단 유지, 영향 미확인은 미검증. 무영향 근거가 있는 독립 장식만 비선행이며 부분 증거≠부모 E5/모듈 전체 Passed. 기준 변경은 영향받는 소비자/근거만 재검토; E9는 E8 Core 둘 이상과 기존 조화 관문 유지 | unity:Assets/Ssalddel/Presentation/World/WorldVisualCatalog.cs<br>repo:Ssalddel.Unity/Runtime/PresentationContracts/표현연결Preflight.cs<br>repo:docs/AI/E5개별성립과전체배치책임분리-2026-08-31.md | repo:Ssalddel.Unity.Tests/표현연결PreflightTests.cs |
| E6 / `actor-grounding-identification` | 발바닥 Bounds, 이동 표면, 식별 거리 → Actor 지면 정렬·식별 검증 결과와 미해결 사항 | unity:Assets/Ssalddel/Presentation/World/공용AnimationAdapter.cs |  |
| E6 / `building-foundation-entry` | 건물 발자국, 표면 높이 표본, 기초대, 출입구 연결 → 건물 기초면·출입구 검증 결과와 미해결 사항 |  |  |
| E6 / `camera-occlusion` | 대상 Bounds, 지원 카메라, 전경 가림체 → 카메라 가림·절단 검증 결과와 미해결 사항 |  |  |
| E6 / `directional-light-surface-readability` | 시간대 합성 광원 방향, Mesh World Normal, Lit Shader, 그림자 투사·수신 정책, Game View 그늘 면 가독성 → 방향광·표면 명암 판독 검증 결과와 미해결 사항 |  |  |
| E6 / `fence-continuity-entry-gap` | 울타리 연결점, 도로변 전면, 출입구 간격 → 울타리 연속성과 출입구 간격 검증 결과와 미해결 사항 |  |  |
| E6 / `interior-clearance` | 건물 외피, 소품 Bounds, 보행·업무 통로 → 실내 소품·통로 여유 검증 결과와 미해결 사항 |  |  |
| E6 / `player-scale-spacing` | Player 기준 크기, 객체 Bounds, 배치 통제 ScaleBand → Player 대비 크기·간격 검증 결과와 미해결 사항 |  |  |
| E6 / `state-difference-readability` | 진입·진행·성공·실패·귀환 상태, 상태별 Renderer·FX·안내 → 상태별 표현 차이 검증 결과와 미해결 사항 | repo:Ssalddel.Unity/Runtime/PresentationContracts/Reconciliation/StableIdReconciliation.cs |  |
| E6 / `surface-clearance` | Renderer 하단 Bounds, 승인 지면 높이, 표면 여유 정책 → 배치 객체 표면 여유 검증 결과와 미해결 사항 |  |  |
| E6 / `weather-camera-audio-exposure` | 시간대 합성 조명, 구름·강수·번개, 빗소리·천둥, 실내 차폐 상태 → 날씨·카메라·음향·차폐 표현 검증 결과와 미해결 사항 |  |  |
| E6 / `work-zone-readability` | 작업 구역, 건설·운영 상태, 도구·재료 배치 → 작업 구역 상태 식별 검증 결과와 미해결 사항 | unity:Assets/Ssalddel/Presentation/World/Farm표시범위Lease.cs |  |
| E7 / `actual-camera-input-result-return` | 저장 Scene, 실제 입력 명령열, Game View, 결과 상태 재조회 → 실제 카메라·입력·결과·귀환 검증 결과와 미해결 사항 |  |  |

## 작업 명세의 구현·증거 연결

미연결 과거 명세는 Unverified이며, 기존 E를 변경하지 않는다. 이 표는 명세에 기록된 범위만 다루며 한 WI의 준비를 전체 폐루프 완료로 올리지 않는다.

`playable-loop:nature-shelter-foundation.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-twilight-return.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-night-day2.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-workbench-foundation.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-field-supply-return.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-basic-herbal-recovery.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-base-reflection.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-building-learning.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-regional-threat-recovery.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-tactical-self-navigation.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

### `playable-loop:farm-crop-cycle.v1`

명세: `eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json`; 현재 Logic E3 / Presentation E1 / 통합 E1. 자동 승격 없음.

| E / 필요 모듈 | 검증 상태 | 구현 / 시험 참조 | 증거 ID | 남은 문제·범위 |
| --- | --- | --- | --- | --- |
| E1 / `presentation-requirement-contract` | Unverified | repo:docs/AI/Presentation단계별최소모듈-2026-08-31.md<br>repo:eng/tests/playable-loop-presentation-validation.ps1 |  | D386 승인 연결. 개별 모듈 EvidencePackage 검토/등록 전이며 기존 Farm E를 유지한다. |
| E2 / `presentation-projection-lifecycle` | Unverified | repo:Ssalddel.Unity/Runtime/PresentationContracts/Reconciliation/StableIdReconciliation.cs<br>repo:Ssalddel.Unity/Runtime/Farm/Farm수확상태PresentationPreparation.cs<br>repo:Ssalddel.Unity.Tests/StableIdReconciliationTests.cs<br>repo:Ssalddel.Unity.Tests/Farm수확상태PresentationPreparationTests.cs<br>repo:Ssalddel.Unity.Tests/PresentationRevisionFirstApplyTests.cs | evidence:farm-presentation-minimum-preparation-d386-20260831 | D386 한 재배 단위의 불변 준비·판본 거부 코드와 집중50/전체587 회귀는 검증했다. 자산 조회·실제 객체 수명·UI 연결을 포함한 모듈 전체는 미검증으로 유지한다. |
| E3 / `presentation-automated-regression` | Unverified | repo:eng/common/presentation-module-bindings.ps1<br>repo:Ssalddel.Unity/Runtime/Farm/Farm수확상태PresentationPreparation.cs<br>repo:eng/tests/playable-loop-presentation-validation.ps1<br>repo:Ssalddel.Unity.Tests/StableIdReconciliationTests.cs<br>repo:Ssalddel.Unity.Tests/Farm수확상태PresentationPreparationTests.cs<br>repo:Ssalddel.Unity.Tests/PresentationRevisionFirstApplyTests.cs | evidence:farm-presentation-minimum-preparation-d386-20260831 | D386 관리 연결42사례와 준비 코드 전체587 회귀는 통과했다. 실제 표현 수명·촬영 도구 건전성의 전체 소비 검증은 미실행이므로 모듈 전체 Passed로 바꾸지 않는다. |
| E4 / `presentation-binding` | Unverified | repo:eng/execution-ledgers/manage-e7-vertical-work-order.ps1<br>repo:Ssalddel.Unity/Runtime/PresentationContracts/표현연결Preflight.cs<br>repo:Ssalddel.Unity/Runtime/Farm/Farm수확표현연결Preflight.cs<br>repo:Ssalddel.Unity.Tests/표현연결PreflightTests.cs |  | D388 4후보 파일조사 완료/D389 읽기 전용 연결 사전검사 구현. 실제 상태·관측 미확보 소비는 Conditional/FarmSnapshotMissing_E5Unlinked; E3+null 결손 Fixture는 Blocked. 상태명/정확 family/제품 소비·지지/접근 미연결, 모듈 전체 Unverified/E5 아님. 보고: docs/Reports/Presentation연결사전검사-D389-2026-08-31.md |
| E5 / `visual-source-bounds` | Blocked | unity:Assets/Ssalddel/Presentation/World/WorldVisualCatalog.cs |  | D389 사전검사와 실제 E5 구분: 기존9밭 null/상태 공급·선택 미연결, 새 관측 없음. Fixture 결손 거부가 현재 Scene 수리·실제 배치/입력 성공을 대신하지 않는다. |
| E6 / `player-scale-spacing` | Blocked |  |  | 기존 9밭 null 컴포넌트·상태 공급/선택 연결 미완료. 격리 시험을 실제 배치·입력 성공으로 대체하지 않는다. |
| E6 / `state-difference-readability` | Blocked | repo:Ssalddel.Unity/Runtime/PresentationContracts/Reconciliation/StableIdReconciliation.cs<br>repo:Ssalddel.Unity/Runtime/Farm/Farm수확상태PresentationPreparation.cs<br>repo:Ssalddel.Unity.Tests/Farm수확상태PresentationPreparationTests.cs<br>repo:Ssalddel.Unity.Tests/PresentationRevisionFirstApplyTests.cs |  | 기존 9밭 null 컴포넌트·상태 공급/선택 연결 미완료. 격리 시험을 실제 배치·입력 성공으로 대체하지 않는다. |
| E7 / `actual-camera-input-result-return` | Blocked |  |  | 기존 9밭 null 컴포넌트·상태 공급/선택 연결 미완료. 격리 시험을 실제 배치·입력 성공으로 대체하지 않는다. |
| E6 / `surface-clearance` | Blocked |  |  | 기존 9밭 null 컴포넌트·상태 공급/선택 연결 미완료. 격리 시험을 실제 배치·입력 성공으로 대체하지 않는다. |
| E6 / `work-zone-readability` | Blocked | unity:Assets/Ssalddel/Presentation/World/Farm표시범위Lease.cs |  | 기존 9밭 null 컴포넌트·상태 공급/선택 연결 미완료. 격리 시험을 실제 배치·입력 성공으로 대체하지 않는다. |
| E6 / `camera-occlusion` | Blocked |  |  | 기존 9밭 null 컴포넌트·상태 공급/선택 연결 미완료. 격리 시험을 실제 배치·입력 성공으로 대체하지 않는다. |

`playable-loop:farm-pack-store-return.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:farm-barracks-defense.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:farm-player-placement.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:hub-inbound-putaway.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:hub-outbound-ready-return.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:town-order-consume-return.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:town-arcana-context.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:city-demand-service-return.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.

`playable-loop:nature-camp-visitor-stay.v1`: Unverified — 모듈별 작업 명세 연결 없음. 기존 E 보존.
