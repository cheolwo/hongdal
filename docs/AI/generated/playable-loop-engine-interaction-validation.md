# PlayableLoop 엔진 상호작용 검증 상태

> `eng/execution-ledgers/playable-loop-engine-interaction-validation.json`와 `eng/execution-ledgers/playable-loops.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 대장 revision: `playable-loop-engine-interaction-validation.r2`
- 성숙도 축: 기존 `Logic`·`Presentation` 유지
- 엔진 상호작용: 두 궤적을 같은 WI·Command·Revision으로 묶는 통합 관문
- Save/Replay canonical hash 포함: `false`

## 구성 요소

| 구성 요소 | 종류 | 권위 변경 | revision |
| --- | --- | --- | --- |
| `WI.ExecutionPipeline` | Orchestration | false | `playable-loop-engine-trace.r1` |
| `Simulation.AuthorityCore` | Authority | true | `simulation-core.current` |
| `LH.Surface` | Presentation | false | `lh-streaming.r1` |
| `Sky.Presentation` | Presentation | false | `sky-engine.r2` |
| `Placement.Exterior` | Presentation | false | `exterior-placement.r1` |
| `Placement.Interior` | Presentation | false | `nature-cabin-presentation.r1` |
| `Lighting.Directional` | Presentation | false | `directional-lighting.natural.r1` |
| `World.Presentation` | Presentation | false | `world-presentation.r1` |

## 첫 적용 프로필

| 폐루프 | WI | 순서 |
| --- | --- | --- |
| `playable-loop:nature-night-day2.v1` | `WI-NATURE-13` 거점 보관 | WI.ExecutionPipeline:Preview → WI.ExecutionPipeline:Confirm → Simulation.AuthorityCore:AuthorityCommit → WI.ExecutionPipeline:ReturnProjection → Placement.Interior:InteriorPlacement → World.Presentation:ReturnProjection |
| `playable-loop:nature-night-day2.v1` | `WI-NATURE-14` 오두막 수면과 새벽 전환 | WI.ExecutionPipeline:Preview → WI.ExecutionPipeline:Confirm → Simulation.AuthorityCore:AuthorityCommit → WI.ExecutionPipeline:ReturnProjection → LH.Surface:SurfacePreparation → Sky.Presentation:AtmosphereProjection → Placement.Exterior:ExteriorPlacement → Placement.Interior:InteriorPlacement → Lighting.Directional:PresentationValidation → World.Presentation:ReturnProjection |
| `playable-loop:nature-night-day2.v1` | `WI-NATURE-15` 다음 확장 계획 선택과 Day2 반환 | WI.ExecutionPipeline:Preview → WI.ExecutionPipeline:Confirm → Simulation.AuthorityCore:AuthorityCommit → WI.ExecutionPipeline:ReturnProjection → Placement.Interior:InteriorPlacement → World.Presentation:ReturnProjection |
