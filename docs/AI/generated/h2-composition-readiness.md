# H2 공간 조합 준비도

> 이 문서는 `eng/world-seedbeds/generated/h2-composition-readiness.v1.json`와 함께 자동 생성된다. 직접 수정하지 않는다.

- H1 인지 부품: `52`
- H2에서 사용하는 H1: `51`
- H2 조합 가능: `37 / 37`
- H2 이론 공간 생산 완료: `37 / 37`
- 작성 조립법 재사용 / 이론 파생 조립법: `6 / 31`
- 사람 검토 때문에 이론 생산이 막힌 H2: `0`
- Unity H2 Root·5시점 근거 등록: `6`
- H2 사람 검토 준비: `6`
- 기준 플레이 H2 추적: `12`
- 엄격 관문 H2 / 추적 누락: `20 / 8`
- 경고 전용 추적 누락: `15`
- 게임플레이 우선 이론 생산 / 사람 검토 준비: `27 / 5`

H1의 재고 상태와 사람 검토 대기 여부는 H2 이론 공간 생산을 직접 막지 않는다. 존재·게임 맥락·공간 역할·표현 근거를 인지한 뒤, 이론 공간 공장의 결정성·연결성 관문을 통과하면 `TheoryQualified`로 생산한다. Unity 검토 자료와 게임플레이 추적은 별도 축으로 남긴다.

| H2 후보 | 이론 생산 | 조립법 출처 | 사람 검토 | 게임플레이 관문 | 추적 | H1 | 이론 차단 | 검토 차단 |
| --- | --- | --- | --- | --- | --- | ---: | --- | --- |
| 농장 집중 수확·집하 블록 (`h2-candidate:farm-harvest-throughput`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Farm–Hub 회랑 블록 (`h2-candidate:farm-hub-corridor`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `NotSelected` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 농장 사건 점검·격리 블록 (`h2-candidate:farm-incident-containment`) | `TheoryQualified` | `AuthoredRecipe` | `ReviewReady` | `Strict` | `SequenceMapped` | 3 | 없음 | 없음 |
| 농장 관수·급수 관리 블록 (`h2-candidate:farm-irrigation-service`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 농장 손실 회복·복원 인계 블록 (`h2-candidate:farm-loss-restoration-handoff`) | `TheoryQualified` | `AuthoredRecipe` | `ReviewReady` | `Strict` | `SequenceMapped` | 3 | 없음 | 없음 |
| 농장 작업·출하 블록 (`h2-candidate:farm-processing-shipping`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 종자·농기구 준비 블록 (`h2-candidate:farm-seed-and-tools`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 세척·선별·포장 블록 (`h2-candidate:farm-wash-sort-pack`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 4 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 농장 작업 지원 블록 (`h2-candidate:farm-worker-support`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 숲 경계 농장 블록 (`h2-candidate:forest-edge-farm`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 고지대 생산 블록 (`h2-candidate:highland-production`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 비상 전력·보관 유지 블록 (`h2-candidate:hub-emergency-power`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 피킹·출고준비 작업 블록 (`h2-candidate:hub-fulfillment`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 입고·창고 블록 (`h2-candidate:hub-inbound-storage`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 장기·저온 보관 블록 (`h2-candidate:hub-longterm-cold-storage`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 차량·시설 정비 블록 (`h2-candidate:hub-maintenance-yard`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 출고·차량 블록 (`h2-candidate:hub-outbound-vehicle`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 검역·격리 블록 (`h2-candidate:hub-quarantine-staging`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub 반품 처리 블록 (`h2-candidate:hub-returns-processing`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| Hub–Town 회랑 블록 (`h2-candidate:hub-town-corridor`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `NotSelected` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 저층 주거 블록 (`h2-candidate:lowrise-residential`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 마트·생활상권 블록 (`h2-candidate:market-life-commerce`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 5 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 자연 야간 방어 블록 (`h2-candidate:nature-defense-ring`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 자연 몬스터 조우·이탈 블록 (`h2-candidate:nature-encounter-route`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 자연 안전 생활핵 블록 (`h2-candidate:nature-home-core`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 자연 복원·안전 회복 블록 (`h2-candidate:nature-restoration-recovery`) | `TheoryQualified` | `AuthoredRecipe` | `ReviewReady` | `Strict` | `SequenceMapped` | 2 | 없음 | 없음 |
| 자연 위협 추적·대피 블록 (`h2-candidate:nature-threat-response`) | `TheoryQualified` | `AuthoredRecipe` | `ReviewReady` | `Strict` | `SequenceMapped` | 3 | 없음 | 없음 |
| Nature–Town 대피·구호 전환 블록 (`h2-candidate:nature-town-relief-transition`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 자연 탐색·대피 블록 (`h2-candidate:nature-trail-shelter`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 산림·수변 완충 블록 (`h2-candidate:nature-water-buffer`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `SequenceMapped` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 생활권 오염 점검·정화 블록 (`h2-candidate:town-contamination-control`) | `TheoryQualified` | `AuthoredRecipe` | `ReviewReady` | `WarningOnly` | `Unlinked` | 3 | 없음 | 없음 |
| 마트 후방 입고·검수 블록 (`h2-candidate:town-market-receiving`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 주문 피킹·포장·수령 블록 (`h2-candidate:town-order-fulfillment`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 생활권 회수 안내·자연권 구호 블록 (`h2-candidate:town-recall-relief`) | `TheoryQualified` | `AuthoredRecipe` | `ReviewReady` | `Strict` | `Unlinked` | 3 | 없음 | 없음 |
| 생활권 주민지원·공동수령 블록 (`h2-candidate:town-resident-service`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `Strict` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 생활권 주거 골목 블록 (`h2-candidate:town-residential-alley`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 2 | 없음 | UnityH2RootAndFiveViewCaptureMissing |
| 생활권 반품·폐기물 블록 (`h2-candidate:town-returns-waste`) | `TheoryQualified` | `DerivedTheoryRecipe` | `AwaitingUnityReviewEvidence` | `WarningOnly` | `Unlinked` | 3 | 없음 | UnityH2RootAndFiveViewCaptureMissing |

## 판정 경계

- `TheoryQualified`: 상대 좌표·위상·관계·연결구를 가진 결정적 위치 독립 H2 이론 공간이다.
- `AuthoredRecipe`와 `DerivedTheoryRecipe`는 출처를 구분한다. 둘 다 같은 이론 품질 관문을 통과해야 한다.
- `ReviewReady`: Unity H2 Root와 표준 5시점 촬영 근거가 등록돼 사람이 사후 검토를 시작할 수 있다.
- 게임플레이 추적은 작업 우선순위를 정하지만 이론 생산을 차단하거나 되돌리지 않는다.
- 어느 상태도 사람의 공식 H2 승인, 실제 지역 E5 배치, E6 공공데이터, E7 Runtime·Play Mode 검증을 뜻하지 않는다.
