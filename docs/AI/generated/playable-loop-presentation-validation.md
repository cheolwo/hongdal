# PlayableLoop 공통 표현 검증 모듈 상태

> `eng/execution-ledgers/playable-loop-presentation-validation-modules.json`와 `eng/execution-ledgers/playable-loops.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 모듈 대장: `playable-loop-presentation-validation-modules.r4`
- 공통 모듈: `5`
- 조건 모듈: `10`
- 적용 PlayableUnit: `17`

## 공통 관문

| E | 모듈 | 자동화 | 차단 |
| --- | --- | --- | --- |
| E4 | `presentation-binding` 표현 상태·H 기준점 결속 검증 | Automated | Blocking |
| E5 | `visual-source-bounds` 시각 자산·Renderer Bounds 준비 검증 | Automated | Blocking |
| E6 | `player-scale-spacing` Player 대비 크기·간격 검증 | Automated | Blocking |
| E6 | `state-difference-readability` 상태별 표현 차이 검증 | Mixed | Blocking |
| E7 | `actual-camera-input-result-return` 실제 카메라·입력·결과·귀환 검증 | ManualEvidence | Blocking |

## 기능 프로필

| 폐루프 | 기능 | 선택된 모듈 |
| --- | --- | --- |
| `playable-loop:nature-shelter-foundation.v1` | GroundSurface, Building, CameraOcclusion | actual-camera-input-result-return, building-foundation-entry, camera-occlusion, player-scale-spacing, presentation-binding, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:nature-twilight-return.v1` | GroundSurface, Actor, CameraOcclusion | actor-grounding-identification, actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-binding, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:nature-night-day2.v1` | Building, Interior, CameraOcclusion, Atmosphere, DirectionalLighting | actual-camera-input-result-return, atmosphere-authority-binding, building-foundation-entry, camera-occlusion, directional-light-surface-readability, interior-clearance, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds, weather-camera-audio-exposure |
| `playable-loop:nature-workbench-foundation.v1` | GroundSurface, WorkZone, CameraOcclusion | actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-binding, state-difference-readability, surface-clearance, visual-source-bounds, work-zone-readability |
| `playable-loop:nature-field-supply-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-base-reflection.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-building-learning.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-regional-threat-recovery.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:nature-tactical-self-navigation.v1` | GroundSurface, Actor, CameraOcclusion | actor-grounding-identification, actual-camera-input-result-return, camera-occlusion, player-scale-spacing, presentation-binding, state-difference-readability, surface-clearance, visual-source-bounds |
| `playable-loop:farm-crop-cycle.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:farm-pack-store-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:farm-player-placement.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:hub-inbound-putaway.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:hub-outbound-ready-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:town-order-consume-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:town-arcana-context.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
| `playable-loop:city-demand-service-return.v1` | 공통만 적용 | actual-camera-input-result-return, player-scale-spacing, presentation-binding, state-difference-readability, visual-source-bounds |
