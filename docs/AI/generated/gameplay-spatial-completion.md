# 게임플레이·H 공간·E 증거·완성 단위 대장

> 이 문서는 `eng/world-seedbeds/gameplay-spatial-completion.v1.json`에서 자동 생성된다. 직접 수정하지 않는다.

## 네 축

- **H 구조:** 공간 자원의 종류와 H1→H4 조립 깊이
- **게임플레이 추적:** 공간이 기준 플레이를 직접 또는 간접 지원하는 정도
- **E 증거:** E0→E9 구현·통합 증거 깊이
- **완성 단위:** 사람이 시작부터 다음 날까지 완주하는 마감 상태

이론 공간은 게임플레이 추적·사람 검토를 기다리지 않고 생산한다. `E5TheoryQualified`와 실제 E5 결속·E7 완주는 서로 다른 사실이다.

## 현재 완성 단위

| 기준 플레이 | 현재 상태 | 이론 공간 | 실제 공간 | 목표 | 단계/분기 | H1/H2/H3/H4 | 차단 사유 |
| --- | --- | --- | --- | --- | ---: | --- | --- |
| Nature↔Farm 수확과 회복의 하루 (`reference-play:nature-farm-day.v1`) | `SpatiallyComposed` | `E5TheoryQualified` (2) | `ActualE5Bound` (2) | `PlayableSliceComplete` | 12/3 | 17/12/8/2 | FirstTimePlayerEvidenceMissing, FullNormalAndRecoveryRuntimeEvidenceMissing, SavedScenePlayModeGameViewEvidenceMissing, VisualAudioPerformancePolishEvidenceMissing |

## H 게임플레이 추적

| H 참조 | 계층 | 게임플레이 추적 | 단계 | 기여 |
| --- | --- | --- | --- | --- |
| `h1-stock:farm-exposure-inspection` | `H1` | `DirectAction` | farm-incident-decision |  |
| `h1-stock:farm-harvest-staging` | `H1` | `DirectAction` | collect, harvest, pack | StatePresentation |
| `h1-stock:farm-incident-quarantine` | `H1` | `DirectAction` | farm-incident-decision |  |
| `h1-stock:farm-loss-recovery` | `H1` | `Supporting` | farm-incident-decision | StatePresentation, ThreatReadability, TransitionHandoff |
| `h1-stock:farm-production` | `H1` | `DirectAction` | harvest, plan-harvest |  |
| `h1-stock:farm-restoration-supply` | `H1` | `Supporting` | farm-incident-decision, restore-route | StatePresentation, ThreatReadability, TransitionHandoff |
| `h1-stock:farm-tool-storage` | `H1` | `Supporting` | plan-harvest | SightlineAndLandmark, StatePresentation |
| `h1-stock:farm-work-yard` | `H1` | `DirectAction` | collect, pack, plan-harvest | SightlineAndLandmark, StatePresentation |
| `h1-stock:farm-worker-waiting` | `H1` | `Supporting` | plan-harvest | SightlineAndLandmark, StatePresentation |
| `h1-stock:nature-emergency-retreat` | `H1` | `DirectAction` | retreat |  |
| `h1-stock:nature-exploration-buffer` | `H1` | `Supporting` | travel-to-farm | AtmosphereAndIdentity, TransitionHandoff, TraversalGuidance |
| `h1-stock:nature-farm-edge` | `H1` | `Supporting` | harvest, travel-to-farm | AtmosphereAndIdentity, StatePresentation, TransitionHandoff, TraversalGuidance |
| `h1-stock:nature-incident-trace` | `H1` | `DirectAction` | observe-threat |  |
| `h1-stock:nature-restoration-site` | `H1` | `DirectAction` | restore-route |  |
| `h1-stock:nature-safe-recovery-camp` | `H1` | `DirectAction` | close-day, day-start, recover-party, retreat | BufferAndSafety, SightlineAndLandmark, StatePresentation, TraversalGuidance |
| `h1-stock:nature-threat-watch` | `H1` | `DirectAction` | observe-threat |  |
| `h1-stock:nature-trailhead` | `H1` | `Supporting` | day-start, travel-to-farm | AtmosphereAndIdentity, SightlineAndLandmark, TransitionHandoff, TraversalGuidance |
| `h2-candidate:farm-incident-containment` | `H2` | `SequenceMapped` | farm-incident-decision |  |
| `h2-candidate:farm-loss-restoration-handoff` | `H2` | `SequenceMapped` | farm-incident-decision, restore-route |  |
| `h2-candidate:farm-processing-shipping` | `H2` | `SequenceMapped` | collect, pack, plan-harvest |  |
| `h2-candidate:farm-seed-and-tools` | `H2` | `SequenceMapped` | plan-harvest |  |
| `h2-candidate:farm-wash-sort-pack` | `H2` | `SequenceMapped` | collect, harvest, pack |  |
| `h2-candidate:farm-worker-support` | `H2` | `SequenceMapped` | plan-harvest |  |
| `h2-candidate:forest-edge-farm` | `H2` | `SequenceMapped` | travel-to-farm |  |
| `h2-candidate:highland-production` | `H2` | `SequenceMapped` | harvest, plan-harvest |  |
| `h2-candidate:nature-restoration-recovery` | `H2` | `SequenceMapped` | close-day, day-start, recover-party, restore-route, retreat |  |
| `h2-candidate:nature-threat-response` | `H2` | `SequenceMapped` | observe-threat, retreat |  |
| `h2-candidate:nature-trail-shelter` | `H2` | `SequenceMapped` | day-start, travel-to-farm |  |
| `h2-candidate:nature-water-buffer` | `H2` | `SequenceMapped` | travel-to-farm |  |
| `h3-candidate:farm-incident-recovery` | `H3` | `LoopMapped` | farm-incident-decision, restore-route |  |
| `h3-candidate:farm-processing-campus` | `H3` | `LoopMapped` | collect, harvest, pack, plan-harvest |  |
| `h3-candidate:farm-seasonal-production-loop` | `H3` | `LoopMapped` | plan-harvest |  |
| `h3-candidate:highland-farm` | `H3` | `LoopMapped` | harvest, plan-harvest, travel-to-farm |  |
| `h3-candidate:nature-exploration-buffer` | `H3` | `LoopMapped` | travel-to-farm |  |
| `h3-candidate:nature-home-encounter-defense` | `H3` | `LoopMapped` | day-start |  |
| `h3-candidate:nature-threat-recovery` | `H3` | `LoopMapped` | close-day, day-start, observe-threat, recover-party, restore-route, retreat |  |
| `h3-candidate:nature-trail-network` | `H3` | `LoopMapped` | day-start, travel-to-farm |  |
| `h4-blueprint:farm-production-processing-region` | `H4` | `RegionalCausalityMapped` | collect, farm-incident-decision, harvest, pack, plan-harvest, restore-route, travel-to-farm |  |
| `h4-blueprint:nature-home-exploration-region` | `H4` | `RegionalCausalityMapped` | close-day, day-start, observe-threat, recover-party, restore-route, retreat, travel-to-farm |  |

## WI E 증거

| 기준 플레이 | WI | 구현 | 통합 | E5 직접 배치 | E5 문맥 | E7 플레이 |
| --- | --- | --- | --- | --- | --- | --- |
| `reference-play:nature-farm-day.v1` | 수확 (`WI-FARM-04`) | `E3` | `E6` | 2 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 수확물 집하 (`WI-FARM-05`) | `E3` | `E6` | 2 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 출하 준비·포장 (`WI-FARM-06`) | `E3` | `E6` | 2 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 자연권 위협 관찰 (`WI-NATURE-01`) | `E3` | `E5` | 1 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 자연권 긴급 후퇴 (`WI-NATURE-02`) | `E3` | `E5` | 1 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 자연권 복원 (`WI-NATURE-03`) | `E3` | `E5` | 1 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 파티 회복 (`WI-NATURE-04`) | `E3` | `E5` | 1 | 0 | 0 |
| `reference-play:nature-farm-day.v1` | 지역 발견 (`WI-WORLD-05`) | `E3` | `E5` | 0 | 1 | 0 |

## 경고와 다음 순서

- 아직 기준 플레이가 없는 경고 전용 기획: CityHubLogisticsResilience, TownLivingMarketSafety
- 1. Farm→Hub 공급선 (`reference-play:farm-hub-supply-line.v1`) — `reference-play:nature-farm-day.v1` 완료 뒤 시작
- 2. Hub→Town 시장·수령의 하루 (`reference-play:hub-town-market-day.v1`) — `reference-play:farm-hub-supply-line.v1` 완료 뒤 시작

## 판정 경계

- 지원 경관은 이동·가독성·완충·분위기·상태 표현 기여가 있으면 유지한다.
- 게임플레이 추적 누락은 이론 H2·H3·E5 생산을 막지 않고 게임플레이 우선순위에만 영향을 준다.
- H 승인, 촬영 `Good`, E7과 `PlayableSliceComplete`는 서로 다른 사실이다.
- 카드 조건은 공간 표현 연결점만 기록하며 수치와 효과 권위는 서버에 남긴다.
- Town·Hub 누락은 현재 경고이며 Nature·Farm 기준 플레이 누락만 검증을 차단한다.
