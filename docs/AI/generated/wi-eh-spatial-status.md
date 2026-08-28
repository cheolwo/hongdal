# WI별 E/H 공간 성립 현황

> 이 문서는 E/H 원장·공간 재고·공식 H 정의를 대조해 자동 생성한다. 직접 수정하지 않는다.

- WI: `66개` · E3: `61개`
- E4/H1 실행 성립: `14개`
- E5/H3 실제 공간 결속: `15개`
- H1~H4 설계 후보 계보만 존재: `21개`
- 필수 공간 설계 누락: `5개`
- 공간 비적용: `11개`
- 공식 H 정의: `H1 8 / H2 0 / H3 5 / H4 1`

후보 H2·H3·H4 계보와 Graph binding은 설계 입력이며 E 단계나 실제 배치를 자동 승격하지 않는다.

## 행위자 공통 물품·장착 (`ACTOR`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 물품 획득 · `WI-ACTOR-01` | `E3/E5` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 장착 상태 변경 · `WI-ACTOR-02` | `E3/E5` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 지식 습득 · `WI-ACTOR-03` | `E3/E5` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |

## 메이저 아르카나 (`CARD`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 현재 세계의 메이저 아르카나 활성화 · `WI-CARD-01` | `E3/E4` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` | E4WithoutApprovedH1 |

## 도심 운영 (`CITY`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 도심 서비스 수요 확정 · `WI-CITY-01` | `E1/E1` | `Required` | `-` | `MissingRequired` | `P5` | `BlockedMissingDesign` | RequiredSpatialDesignMissing |
| 도심 서비스용 지역 재고 배정 · `WI-CITY-02` | `E1/E1` | `Required` | `-` | `MissingRequired` | `P5` | `BlockedMissingDesign` | RequiredSpatialDesignMissing |
| 도심 주민 서비스 처리 · `WI-CITY-03` | `E1/E1` | `Required` | `-` | `MissingRequired` | `P5` | `BlockedMissingDesign` | RequiredSpatialDesignMissing |
| 도심 서비스 결과 확인 · `WI-CITY-04` | `E1/E1` | `Required` | `-` | `MissingRequired` | `P5` | `BlockedMissingDesign` | RequiredSpatialDesignMissing |

## 영역 건설 (`CON`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 영역 건물 건설 확정 · `WI-CON-01` | `E3/E7` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |

## 농장 생산 (`FARM`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 경작지 밭갈이 · `WI-FARM-01` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P5` | `ReadyForActualE5Input` |  |
| 경작지 씨앗 파종 · `WI-FARM-02` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P5` | `ReadyForActualE5Input` |  |
| 농작물 생육 관리 · `WI-FARM-03` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P5` | `ReadyForActualE5Input` |  |
| 익은 농작물 수확 · `WI-FARM-04` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P2` | `ReadyForActualE5Input` |  |
| 수확물 집하장 모으기 · `WI-FARM-05` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P2` | `ReadyForActualE5Input` |  |
| 출하 물량 포장 · `WI-FARM-06` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P2` | `ReadyForActualE5Input` |  |

## 물류 거점 창고 (`HUB`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 입고 화물 검수 · `WI-001` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P3` | `ReadyForApprovedH1Input` |  |
| 검수 완료 화물 창고 적재 · `WI-002` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P3` | `ReadyForApprovedH1Input` |  |
| 출고 대상 재고 요청 · `WI-HUB-03` | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 출고 대상 재고 피킹 · `WI-HUB-04` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 피킹 화물 포장 · `WI-HUB-05` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 출고 차량 상차 · `WI-HUB-06` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |

## 영역 간 화물 이동 (`LOG`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 출하 차량 상차 확정 · `WI-LOG-01` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P2` | `ReadyForActualE5Input` |  |
| 농장에서 출발 · `WI-LOG-02` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P5` | `ReadyForActualE5Input` |  |
| 농장에서 물류 거점으로 화물 이동 · `WI-LOG-03` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P5` | `ReadyForApprovedH1Input` |  |
| 물류 거점 도착 화물 하차 · `WI-LOG-04` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P3` | `ReadyForApprovedH1Input` |  |
| 물류 거점 도착 화물 인수 · `WI-LOG-05` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P3` | `ReadyForApprovedH1Input` |  |

## 마트 입고·진열 (`MARKET`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 물류 거점에서 마트로 운송 · `WI-MARKET-01` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 마트 도착 화물 인수 · `WI-MARKET-02` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 마트 입고 상품 검수 · `WI-MARKET-03` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 검수 상품 후방 창고 적재 · `WI-MARKET-04` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |
| 매장 진열대 상품 보충 · `WI-MARKET-05` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P4` | `DesignCandidateOnly` |  |

## 자연 탐사·생활 거점 (`NATURE`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 자연 지역 위험 징후 확인 · `WI-NATURE-01` | `E3/E7` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |
| 안전 거점으로 긴급 후퇴 · `WI-NATURE-02` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P1` | `DesignCandidateOnly` |  |
| 훼손된 자연 경로 복원 · `WI-NATURE-03` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P1` | `DesignCandidateOnly` |  |
| 탐사대 안전 회복 · `WI-NATURE-04` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P1` | `DesignCandidateOnly` |  |
| 벌목 도끼 획득 · `WI-NATURE-05` | `E3/E7` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` | E5PlacementReferenceMissing |
| 나무 벌목 작업 시작 · `WI-NATURE-06` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 오두막을 지을 터 선정 · `WI-NATURE-07` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 오두막 건설 작업 시작 · `WI-NATURE-08` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 오두막 안으로 들어가기 · `WI-NATURE-09` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 오두막 밖으로 나가기 · `WI-NATURE-10` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 황혼 위협 대응 방식 확정 · `WI-NATURE-11` | `E3/E7` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |
| 진행 중 작업 취소 · `WI-NATURE-12` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 획득 자원 거점 보관 · `WI-NATURE-13` | `E3/E7` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |
| 오두막에서 수면·새벽 맞기 · `WI-NATURE-14` | `E3/E5` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |
| 다음 날 거점 확장 계획 선택 · `WI-NATURE-15` | `E3/E6` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |
| 현장 보급 꾸러미 제작 · `WI-NATURE-16` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 현장 보급 제작 업무 위임 · `WI-NATURE-17` | `E3/E4` | `Required` | `H1` | `EstablishedH1` | `P1` | `ReadyForApprovedH1Input` |  |
| 벌목 통나무 줍기 · `WI-NATURE-18` | `E3/E7` | `Required` | `H3` | `EstablishedH3` | `P1` | `ReadyForActualE5Input` |  |

## 주민 주문·소비 (`ORDER`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 주민 주문 확정 · `WI-ORDER-01` | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 주문 상품 재고 예약 · `WI-ORDER-02` | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 주문 상품 피킹 · `WI-ORDER-03` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| 주문 상품 포장 · `WI-ORDER-04` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| 주문 상품 수령 준비 · `WI-ORDER-05` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| 주민 주문 상품 수령 · `WI-ORDER-06` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| 주민 상품 소비 · `WI-ORDER-07` | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |

## 거점 성찰 (`REFLECT`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 승인 자료로 거점 성찰 확정 · `WI-REFLECT-01` | `E3/E3` | `Required` | `-` | `MissingRequired` | `P1` | `BlockedMissingDesign` | RequiredSpatialDesignMissing |

## 업무 검토 (`REVIEW`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| NPC 업무 결과 검토 확정 · `WI-REVIEW-01` | `E2/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |

## 공통 세계 운영 (`WORLD`)

| WI | E | 공간 참여 | 성립 H | 설계 상태 | 우선순위 | LH 인계 | 경고 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| NPC에게 반복 업무 배정 · `WI-WORLD-01` | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| NPC에게 업무 역량 위임 · `WI-WORLD-02` | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 진행 중 세계 업무 취소 · `WI-WORLD-03` | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 손상된 시설 수리 · `WI-WORLD-04` | `E3/E1` | `Required` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` | GraphBindingWithoutApprovedH1 |
| 새로운 지역 발견 · `WI-WORLD-05` | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| 일행 역할 카드 장착 · `WI-WORLD-06` | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |
| 세계 활동 상태 변경 · `WI-WORLD-07` | `E3/E1` | `Contextual` | `-` | `CandidateLineage` | `P5` | `DesignCandidateOnly` |  |
| 하루 운영 턴 마감 · `WI-WORLD-08` | `E3/E1` | `NotRequired` | `-` | `NotApplicable` | `P5` | `NotApplicable` |  |

## P1 기준 플레이 공간 구성

`WI-FARM-04 → WI-FARM-05 → WI-FARM-06 → WI-LOG-01`을 생산구획 → 집하 → 포장 → 상차 공간으로 연결한다.
실행 입력은 `eng/world-seedbeds/wi-spatial-composition-plans/reference-play-01-harvest-shipping.v1.json`에 있다. H 설계와 Scenario 실행은 공공데이터와 독립이다. DEM·토지피복·도로·Block 경계는 현실 정합을 선택할 때 사용하는 E6 후보 목적이며, 미적용 상태는 H 공간이나 Scenario E7을 차단하지 않는다.

## P2 진부 Hub 입고·보관 공간 구성

`WI-LOG-04 → WI-LOG-05 → WI-001 → WI-002`를 하차 공간 → 인수·검수 공간 → 창고 적재 공간으로 연결한다.
실행 입력은 `eng/world-seedbeds/wi-spatial-composition-plans/p2-hub-inbound-storage.v1.json`에 있다. 진부 Hub의 권위 업무 Node와 E5 배치 Block이 없어 지역 인스턴스 후보로 유지한다. 도로·건물·Block 경계는 현실 정합을 선택할 때만 E6 후보 목적이 되며 Scenario 공간 실행을 막지 않는다.

## 확인이 필요한 공백

- `WI-CARD-01` 현재 세계의 메이저 아르카나 활성화: `E4WithoutApprovedH1`
- `WI-CITY-01` 도심 서비스 수요 확정: `RequiredSpatialDesignMissing`
- `WI-CITY-02` 도심 서비스용 지역 재고 배정: `RequiredSpatialDesignMissing`
- `WI-CITY-03` 도심 주민 서비스 처리: `RequiredSpatialDesignMissing`
- `WI-CITY-04` 도심 서비스 결과 확인: `RequiredSpatialDesignMissing`
- `WI-NATURE-05` 벌목 도끼 획득: `E5PlacementReferenceMissing`
- `WI-REFLECT-01` 승인 자료로 거점 성찰 확정: `RequiredSpatialDesignMissing`
- `WI-WORLD-04` 손상된 시설 수리: `GraphBindingWithoutApprovedH1`
