# WI별 E/H 공간 성립 현황

> 이 문서는 E/H 원장·공간 재고·공식 H 정의를 대조해 자동 생성한다. 직접 수정하지 않는다.

- WI: `37개` · E3: `37개`
- E4/H1 실행 성립: `13개`
- H1~H4 설계 후보 계보만 존재: `17개`
- 필수 공간 설계 누락: `1개`
- 공간 비적용: `6개`
- 공식 H 정의: `H1 5 / H2 0 / H3 5 / H4 1`

후보 H2·H3·H4 계보와 Graph binding은 설계 입력이며 E 단계나 실제 배치를 자동 승격하지 않는다.

## FARM

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WI-FARM-01` 밭갈기 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P4` | `ReadyForApprovedH1Input` | E5PlacementReferenceWithoutH2Definition |
| `WI-FARM-02` 파종 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P4` | `ReadyForApprovedH1Input` | E5PlacementReferenceWithoutH2Definition |
| `WI-FARM-03` 관수·재배 관리 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P4` | `ReadyForApprovedH1Input` | E5PlacementReferenceWithoutH2Definition |
| `WI-FARM-04` 수확 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| `WI-FARM-05` 수확물 집하 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| `WI-FARM-06` 출하 준비·포장 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |

## HUB

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WI-001` 진부 Hub 입고검수 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P2` | `ReadyForApprovedH1Input` |  |
| `WI-002` 진부 Hub 창고 적재 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P2` | `ReadyForApprovedH1Input` |  |
| `WI-HUB-03` 출고 요청 | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-HUB-04` 피킹 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-HUB-05` 출고 준비 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-HUB-06` Hub 차량 상차 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |

## LOG

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WI-LOG-01` 차량 상차 확정 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| `WI-LOG-02` Farm 출발 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P4` | `ReadyForApprovedH1Input` |  |
| `WI-LOG-03` Farm→Hub 화물 이동 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P4` | `ReadyForApprovedH1Input` |  |
| `WI-LOG-04` Hub 하차 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P2` | `ReadyForApprovedH1Input` |  |
| `WI-LOG-05` Hub 인수 | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P2` | `ReadyForApprovedH1Input` |  |

## MARKET

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WI-MARKET-01` Hub→마트 운송 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-MARKET-02` 마트 하차·인수 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-MARKET-03` 마트 입고검수 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-MARKET-04` 마트 후방 적재 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |
| `WI-MARKET-05` 진열 보충 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P3` | `DesignCandidateOnly` |  |

## ORDER

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WI-ORDER-01` 주문 확정 | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P4` | `NotApplicable` |  |
| `WI-ORDER-02` 주문 재고 예약 | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P4` | `NotApplicable` |  |
| `WI-ORDER-03` 주문 피킹 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| `WI-ORDER-04` 주문 포장 | `E3/E1` | `Required` | `-` | `MissingRequired` | `P4` | `BlockedMissingDesign` | RequiredSpatialDesignMissing |
| `WI-ORDER-05` 수령 준비 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| `WI-ORDER-06` 주민 수령 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| `WI-ORDER-07` 주민 소비 | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |

## WORLD

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WI-WORLD-01` NPC 작업 배정 | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| `WI-WORLD-02` NPC 역량 위임 | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P4` | `NotApplicable` |  |
| `WI-WORLD-03` 작업 취소 | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P4` | `NotApplicable` |  |
| `WI-WORLD-04` 시설 수리 | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` | GraphBindingWithoutApprovedH1 |
| `WI-WORLD-05` 지역 발견 | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| `WI-WORLD-06` 역할 카드 장착 | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P4` | `NotApplicable` |  |
| `WI-WORLD-07` 활동 시작·종료 | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| `WI-WORLD-08` 턴 마감 | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P4` | `NotApplicable` |  |

## P1 기준 플레이 공간 구성

`WI-FARM-04 → WI-FARM-05 → WI-FARM-06 → WI-LOG-01`을 생산구획 → 집하 → 포장 → 상차 공간으로 연결한다.
실행 입력은 `eng/world-seedbeds/wi-spatial-composition-plans/reference-play-01-harvest-shipping.v1.json`에 있으며 실제 도로·Block 경계가 없으므로 H2·E5로 승격하지 않는다.

## P2 진부 Hub 입고·보관 공간 구성

`WI-LOG-04 → WI-LOG-05 → WI-001 → WI-002`를 하차 공간 → 인수·검수 공간 → 창고 적재 공간으로 연결한다.
실행 입력은 `eng/world-seedbeds/wi-spatial-composition-plans/p2-hub-inbound-storage.v1.json`에 있으며 진부 Hub의 실제 업무 Node와 H2 Block이 없으므로 후보 설계로 유지한다.

## 확인이 필요한 공백

- `WI-FARM-01` 밭갈기: `E5PlacementReferenceWithoutH2Definition`
- `WI-FARM-02` 파종: `E5PlacementReferenceWithoutH2Definition`
- `WI-FARM-03` 관수·재배 관리: `E5PlacementReferenceWithoutH2Definition`
- `WI-ORDER-04` 주문 포장: `RequiredSpatialDesignMissing`
- `WI-WORLD-04` 시설 수리: `GraphBindingWithoutApprovedH1`
