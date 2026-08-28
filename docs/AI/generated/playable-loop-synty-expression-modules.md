# PlayableLoop Synty 표현 모듈 상태

> `eng/execution-ledgers/playable-loop-synty-expression-modules.json`와 `eng/execution-ledgers/playable-loops.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 대장 revision: `playable-loop-synty-expression-modules.r1`
- 폐루프 모듈: `4`
- 공유 모듈: `4`
- 표현 슬롯: `23`
- 사용 자산 계열: `31`
- 기존 A/B/C 기준 문법: `LegacyGenerated / 신규 생성 금지`

| 폐루프 | 모듈 | WI | 슬롯 | 공유 모듈 |
| --- | --- | ---: | ---: | --- |
| `playable-loop:nature-shelter-foundation.v1` | `synty-loop:nature-shelter-foundation.v1` | 10 | 10 | synty-shared:nature-ground.v1, synty-shared:construction-progress.v1 |
| `playable-loop:nature-twilight-return.v1` | `synty-loop:nature-twilight-return.v1` | 2 | 3 | synty-shared:nature-ground.v1, synty-shared:nature-atmosphere.v1 |
| `playable-loop:nature-night-day2.v1` | `synty-loop:nature-night-day2.v1` | 3 | 8 | synty-shared:shelter-interior.v1, synty-shared:nature-atmosphere.v1, synty-shared:construction-progress.v1 |
| `playable-loop:nature-workbench-foundation.v1` | `synty-loop:nature-workbench-foundation.v1` | 2 | 2 | synty-shared:construction-progress.v1 |

## 팩 사용 정책

| 팩 | 정책 | 기본 역할 |
| --- | --- | --- |
| `nature` | `ProductionPrimary` | spatial-base, ambient-detail, feedback-fx |
| `farm` | `ProductionPrimary` | functional-anchor, interior-fixture, interior-loose-item |
| `town` | `ProductionPrimary` | interior-fixture, interior-loose-item, functional-anchor |
| `city` | `ProductionPrimary` | spatial-base, functional-anchor, ambient-detail |
| `construction` | `SharedStateLayer` | state-overlay, functional-anchor, interior-loose-item |
| `generic` | `SharedBasePendingInventory` | spatial-base, functional-anchor |
| `starter` | `PrototypeFallbackOnly` |  |
