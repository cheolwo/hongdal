# PlayableLoop Synty ?쒗쁽 紐⑤뱢 ?곹깭

> `eng/execution-ledgers/playable-loop-synty-expression-modules.json`? `eng/execution-ledgers/playable-loops.json`?먯꽌 ?먮룞 ?앹꽦?쒕떎. 吏곸젒 ?섏젙?섏? ?딅뒗??

- ???revision: `playable-loop-synty-expression-modules.r1`
- ?먮（??紐⑤뱢: `4`
- 怨듭쑀 紐⑤뱢: `4`
- ?쒗쁽 ?щ’: `23`
- ?ъ슜 ?먯궛 怨꾩뿴: `31`
- 湲곗〈 A/B/C 湲곗? 臾몃쾿: `LegacyGenerated / ?좉퇋 ?앹꽦 湲덉?`

| ?먮（??| 紐⑤뱢 | WI | ?щ’ | 怨듭쑀 紐⑤뱢 |
| --- | --- | ---: | ---: | --- |
| `playable-loop:nature-shelter-foundation.v1` | `synty-loop:nature-shelter-foundation.v1` | 10 | 10 | synty-shared:nature-ground.v1, synty-shared:construction-progress.v1 |
| `playable-loop:nature-twilight-return.v1` | `synty-loop:nature-twilight-return.v1` | 2 | 3 | synty-shared:nature-ground.v1, synty-shared:nature-atmosphere.v1 |
| `playable-loop:nature-night-day2.v1` | `synty-loop:nature-night-day2.v1` | 3 | 8 | synty-shared:shelter-interior.v1, synty-shared:nature-atmosphere.v1, synty-shared:construction-progress.v1 |
| `playable-loop:nature-workbench-foundation.v1` | `synty-loop:nature-workbench-foundation.v1` | 2 | 2 | synty-shared:construction-progress.v1 |

## ???ъ슜 ?뺤콉

| ??| ?뺤콉 | 湲곕낯 ??븷 |
| --- | --- | --- |
| `nature` | `ProductionPrimary` | spatial-base, ambient-detail, feedback-fx |
| `farm` | `ProductionPrimary` | functional-anchor, interior-fixture, interior-loose-item |
| `town` | `ProductionPrimary` | interior-fixture, interior-loose-item, functional-anchor |
| `city` | `ProductionPrimary` | spatial-base, functional-anchor, ambient-detail |
| `construction` | `SharedStateLayer` | state-overlay, functional-anchor, interior-loose-item |
| `generic` | `SharedBasePendingInventory` | spatial-base, functional-anchor |
| `starter` | `PrototypeFallbackOnly` |  |
