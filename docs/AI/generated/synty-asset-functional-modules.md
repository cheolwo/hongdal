# Synty 자산 기능 체계

> `eng/execution-ledgers/synty-asset-functional-modules.json`에서 자동 생성된다. Unity의 전수 Prefab 판정은 EditMode 시험이 검증한다.

- revision: `synty-asset-functional-modules.r3`
- 현재 원본 Prefab 기준 수량: `4211`
- 원본 팩: `13`
- 신규 구매 활용 프로필: `6` (Prefab과 AnimationClip 분리)
- 기능군: `12`
- 사람이 읽는 표현 범위: `3` (실외 표현 / 실내 표현 / 공통 표현)
- 세부 기능군: `41`
- 자산 종류: `152`
- 업무 영역 골격: `4`
- 기존 156개 A/B/C: `LegacyGenerated / 신규 작성 금지 / 읽기 호환`

| 원본 팩 | Prefab | 정책 |
| --- | ---: | --- |
| nature | 227 | AreaAndSharedCandidates |
| farm | 498 | AreaAndSharedCandidates |
| town | 702 | AreaAndSharedCandidates |
| city | 335 | AreaAndSharedCandidates |
| construction | 584 | SharedConstructionStateLayer |
| generic | 495 | SharedBase |
| starter | 58 | PrototypeFallbackOnly |
| animation-base-locomotion | 4 | AnimationSourceNeedsReview |
| animation-emotes-and-taunts | 2 | AnimationSourceNeedsReview |
| animation-sword-combat | 1 | AnimationSourceNeedsReview |
| alpine-mountain | 125 | AreaAndSharedCandidatesNeedsReview |
| dungeon-realms | 1128 | AreaAndSharedCandidatesNeedsReview |
| dwarven-dungeon-map | 52 | AreaAndSharedCandidatesNeedsReview |

## 신규 구매 자산의 WI·H 활용 후보

> 새 팩은 새 Area가 아니다. 아래 연결은 E4 후보 조사 입력이며 실제 채택과 E5 배치를 승인하지 않는다.

| 원본 팩 | 원천 종류 | H Capability 후보 | PlayableLoop 후보 |
| --- | --- | --- | --- |
| animation-base-locomotion | AnimationClip | PlayerMovementArea, NpcMovementArea, TacticalCommandArea | nature-navigation, farm-work-movement, town-hub-actor-movement |
| animation-emotes-and-taunts | AnimationClip | MeditationWorkArea, CommunityInteractionArea, NpcRelationshipArea | player-mind-meditation, community-resonance, npc-cooperation |
| animation-sword-combat | AnimationClip | ThreatEncounterArea, DirectCombatArea, OutpostDefenseArea | nature-twilight-combat, corridor-defense, squad-combat |
| alpine-mountain | Prefab | NatureTravelArea, ShelterSiteChoiceArea, ClimateExposureArea | nature-shelter-foundation, nature-night-day2, nature-exploration |
| dungeon-realms | Prefab | ThreatEncounterArea, RuinsExplorationArea, OutpostDefenseArea, BarracksWorkArea | nature-twilight-combat, corridor-defense, ruins-exploration |
| dwarven-dungeon-map | Prefab | MineExplorationArea, UndergroundCorridorArea, ResourceExtractionArea | mine-exploration, resource-extraction, corridor-defense |

## 사람이 읽는 한국어 분류

> 설계·문서·대장은 한국어 이름을 먼저 사용한다. 괄호 안 영문은 저장과 호환을 위한 Stable Code다.

분류 순서: `범위 → 기능군 → 세부 기능군 → 자산 종류 → 자산 계열 → 실제 Prefab`

### 실외 표현 (`Outdoor`)

- 월드 지면 (`world-surface`)
  - 지형 표면 (`terrain-surface`): 흙 (`soil`), 풀밭 (`grassland`), 암반 (`bedrock`), 모래 (`sand`), 눈밭 (`snowfield`)
  - 물 표면 (`water-surface`): 강 (`river`), 개울 (`stream`), 연못 (`pond`), 배수로 (`drainage-channel`)
  - 지형 요소 (`terrain-feature`): 절벽 (`cliff`), 경사면 (`slope`), 둑 (`embankment`), 제방 (`levee`)
- 자연 식생 (`nature-vegetation`)
  - 나무 (`tree`): 생목 (`living-tree`), 고사목 (`dead-tree`), 그루터기 (`stump`)
  - 관목 (`shrub`): 덤불 (`bush`), 생울타리 (`hedge`)
  - 지표 식생 (`ground-vegetation`): 풀 (`grass`), 꽃 (`flower`), 잡초 (`weed`)
  - 재배 식물 (`cultivated-plant`): 감자 (`potato`), 곡물 (`grain`), 채소 (`vegetable`), 과수 (`fruit-tree`)
  - 자연 잔해 (`natural-debris`): 통나무 (`log`), 나뭇가지 (`branch`), 돌 (`stone`)
- 실외 구조물 (`exterior-structure`)
  - 건물 (`building`): 주택 (`house`), 창고 (`warehouse`), 헛간 (`barn`), 온실 (`greenhouse`), 상점 (`shop`)
  - 건축 부재 (`building-part`): 벽 (`wall`), 지붕 (`roof`), 기둥 (`pillar`), 계단 (`stairs`)
  - 경계 시설 (`boundary-facility`): 울타리 (`fence`), 담장 (`boundary-wall`), 차단시설 (`barrier`)
  - 실외 시설 (`outdoor-facility`): 쉼터 (`shelter`), 작업장 (`worksite`), 하역장 (`loading-yard`), 승강장 (`platform`)
- 실외 기능 소품 (`exterior-functional-prop`)
  - 보관 (`outdoor-storage`): 상자 (`box`), 팔레트 (`pallet`), 보관대 (`storage-rack`)
  - 작업 (`outdoor-work`): 작업대 (`workbench`), 양동이 (`bucket`), 작업 장비 (`work-equipment`)
  - 생활·서비스 (`living-service`): 쓰레기통 (`trash-bin`), 우편함 (`mailbox`), 공공설비 (`public-utility`)
  - 안내 (`guidance`): 표지판 (`signboard`), 광고판 (`billboard`), 위치표식 (`location-marker`)
- 도로·통행망 (`world-network`)
  - 보행망 (`pedestrian-network`): 보행로 (`footpath`), 인도 (`sidewalk`), 계단길 (`stair-path`)
  - 차량망 (`vehicle-network`): 일반도로 (`road`), 농로 (`farm-road`), 작업도로 (`work-road`)
  - 횡단 시설 (`crossing-facility`): 다리 (`bridge`), 여울 (`ford`), 횡단로 (`crossing`)
  - 도로 부속 (`road-accessory`): 교통표지 (`traffic-sign`), 가드레일 (`guardrail`), 도로표식 (`road-marking`)
- 영역 전이 (`world-transition`)
  - 출입 (`access`): 문 (`door`), 대문 (`gate`), 출입구 (`access-point`)
  - 영역 연결 (`area-connection`): 입구 (`entrance`), 출구 (`exit`), 연결 회랑 (`connection-corridor`)
  - 높이 전이 (`height-transition`): 계단 (`stairs`), 경사로 (`ramp`), 사다리 (`ladder`)

### 실내 표현 (`Interior`)

- 실내 구조 (`interior-structure`)
  - 실내 구조 (`interior-structure-part`): 바닥 (`floor`), 벽 (`wall`), 천장 (`ceiling`), 문 (`door`), 창문 (`window`), 칸막이 (`partition`), 계단 (`stairs`)
- 실내 설비 (`interior-fixture`)
  - 휴식 설비 (`rest-fixture`): 침대 (`bed`), 소파 (`sofa`), 의자 (`chair`)
  - 보관 설비 (`storage-fixture`): 선반 (`shelf`), 장 (`cabinet`), 보관대 (`storage-rack`), 사물함 (`locker`)
  - 작업 설비 (`work-fixture`): 책상 (`desk`), 작업대 (`workbench`), 검수대 (`inspection-table`), 계산대 (`checkout-counter`)
  - 조리 설비 (`cooking-fixture`): 싱크대 (`sink`), 조리대 (`countertop`), 조리기구 (`cooking-equipment`)
  - 위생 설비 (`sanitary-fixture`): 세면대 (`washbasin`), 변기 (`toilet`), 욕조 (`bathtub`)
  - 물류 설비 (`logistics-fixture`): 팔레트 선반 (`pallet-rack`), 포장대 (`packing-table`), 분류대 (`sorting-table`), 집하대 (`collection-table`)
- 실내 소품 (`interior-loose-item`)
  - 용기·포장 (`container-packaging`): 상자 (`box`), 바구니 (`basket`), 병 (`bottle`), 포대 (`sack`)
  - 개별 소품 (`loose-item-kind`): 식품 (`food`), 공구 (`tool`), 문서 (`document`), 상품 (`product`), 생활용품 (`household-item`), 장식품 (`decoration`)

### 공통 표현 (`Shared`)

- 건설·복구 표현 (`construction-state`)
  - 건설·복구 상태 (`construction-recovery-state`): 계획 상태 (`planned`), 건설 중 (`under-construction`), 완성 상태 (`complete`), 파손 상태 (`damaged`), 수리 중 (`under-repair`), 복구 중 (`recovering`), 철거 상태 (`demolished`)
- 인물·차량·도구 (`actor-vehicle-tool`)
  - 인물 (`person`): 플레이어 (`player`), 작업자 (`worker`), 주민 (`resident`), 상인 (`merchant`), 적대 인물 (`hostile-person`)
  - 차량 (`vehicle`): 승용 (`passenger-vehicle`), 화물 (`cargo-vehicle`), 농업 (`agricultural-vehicle`), 건설 (`construction-vehicle`)
  - 손도구 (`hand-tool`): 채집·수확 (`gather-harvest-tool`), 건설 (`construction-tool`), 수리 (`repair-tool`), 검사 (`inspection-tool`)
  - 장비 (`equipment`): 이동 장비 (`mobility-equipment`), 동력 장비 (`powered-equipment`)
- 세계 피드백 효과 (`world-feedback-fx`)
  - 환경 효과 (`environment-effect`): 비 (`rain`), 눈 (`snow`), 안개 (`fog`), 바람 (`wind`)
  - 작업 효과 (`work-effect`): 먼지 (`dust`), 불꽃 (`spark`), 파편 (`debris`)
  - 상태 효과 (`state-effect`): 연기 (`smoke`), 불 (`fire`), 수증기 (`steam`), 젖음 (`wetness`)
  - 상호작용 피드백 (`interaction-feedback`): 선택 (`selection`), 성공 (`success`), 경고 (`warning`), 차단 (`blocked`)
