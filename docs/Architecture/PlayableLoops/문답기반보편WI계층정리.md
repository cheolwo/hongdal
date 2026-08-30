# 문답 기반 보편 WI 계층 정리

- 식별: `wi-hierarchy-review.r1`, 2026-08-30.
- 상태: `ReviewReference`. 사용자가 요청한 현황 정리와 보편화 제안이며 신규 실행 승인 대장이 아니다.
- 근거: [WI 대장](../../../eng/execution-ledgers/world-interactions.json) `simulation-world-interactions.r43`, [기존 등록 관계](../../../eng/execution-ledgers/world-interaction-registration-relations.json) `world-interaction-registration-relations.r2`, [문답 검색](../../../eng/planning-inquiries/README.md).
- 문답 411개 검색 항목/774개 발췌/20개 입력 파일을 기계적으로 확인했다. 411은 전역 Q 번호만의 수가 아니라 의미 ID 등을 포함한 색인 건수다. 주제별 현재 기준과 대표 연결을 읽었으며, 모든 과거 답변의 상충·승인 범위를 개별 재심사한 것은 아니다.
- 원장 105개 WI를 아래 부록에 정확히 한 번씩 배치했다. 부록의 20개 탐색 묶음은 이 문서의 읽기 보조이며 **20개의 새 보편 WI 등록이나 단일 실행 책임 승인이 아니다**.
- 변경 범위: 문서와 문서 탐색 링크. 기존 ID·Command·Save·E·실행 원장·정식 관계는 보존한다.

## 1. 조사 결론

기존에도 18개 비실행 WI군과 5개 명시적 특화 관계가 있다. 그 18개 군에 연결된 고유 WI는 56/105개다. 나머지 49개는 **해당 등록 관계 파일에서 미분류**이며 기능 미구현이나 다른 메타데이터 부재를 뜻하지 않는다. 분류가 부분적인 이유를 먼저 확인하고 전면 교체보다 보완한다.

기존 관계 파일의 기준 WI revision은 r39이고 실제 WI 대장은 r43이다. 참조한 ID는 현행에 존재하지만 관계 파일이 최신 WI 전체를 포괄한다고 해석하지 않는다. 실행 종류는 Command 92개, AutomaticTransition 12개, SharedPolicy 1개다. 자연 재생·NPC 자동 피킹을 플레이어가 매번 버튼으로 실행하는 보편 행위로 바꾸지 않는다.

문답 검색 상태는 Confirmed 349 / ConfirmedDirection 9 / Incorporated 34 / Asked 9 / Deferred 5 / NeedsSourceRecovery 3 / Superseded 2다. **Q272~274의 소실 본문은 추론하지 않으며 HB-01의 9개 미승인 추천도 확정 근거에 섞지 않는다.** 현재 상태는 검색을 다시 실행해 확인한다.

## 2. 네 가지 관계를 구분한다

| 관계 | 뜻 | 예 |
| --- | --- | --- |
| 비실행 WI군 | 의미를 묶는 폴더. 자체 비용/Confirm/보상 없음 | 건설 활동 → 청사진 배치·재료 투입·시공·해체 |
| 보편 실행 WI → 대상별 특화 | 같은 핵심 행위에 대상 규칙 적용. 기존 자식 ID는 호환 보존 | 물품 획득 → 도끼 획득 / 장착 → 도구·의복 슬롯 |
| 절차 조합 | 서로 다른 결과 책임을 이어 실행. 부모·자식 상속이 아님 | 채집 → 배합 → 가열 → 달임 완료 → 섭취 |
| 횡단 적용 | 여러 행위에 붙는 승인된 성장/권한/저장 규칙 | 집중 근거·명상 숙련·행위 기록·멱등성 |

[명상 WI군](../전행위몰입과명상숙련행위원장통합체계.md)은 기존처럼 비실행 횡단 축이다. 모든 행동을 명상 명령으로 감싸 다시 실행하거나 새 WI에 회복 보상을 자동 지급하지 않는다. 현재 성장 결속의 증거 범위는 해당 원장/코드에서 별도로 확인한다.

공통 전송 엔진을 쓴다고 인벤토리 줍기·거래 인수·건설 재료 투입을 동일한 권위 행위로 합치지 않는다. 소유권·예약·대가·완료 의미가 다르면 별도 WI를 유지한다. H1~H3는 공간 의미 계층, E는 증거 성숙도이며 WI군의 깊이와 별개다.

## 3. 사람이 읽는 보편 행위 지도

| 보편 의미 | 기존 WI 또는 재사용 출발점 | 하위 적용 사례 / 문답 근거 | 현재 판정 |
| --- | --- | --- | --- |
| 획득 | WI-ACTOR-01 | 도끼·통나무 줍기, 약초 채집 | 도끼/약초 특화 관계 등록됨. 벌목·수확은 원천 노드 변경까지 있으므로 단순 줍기와 구분 |
| 장착 상태 변경 | WI-ACTOR-02 | 도구 장착·의복 착용·해제 | 기존 보편 WI 재사용 검토. 역할 카드 WI-WORLD-06과 슬롯/효과 계약을 무조건 통합하지 않음 |
| 조사·지식 습득 | WI-NATURE-TRACE-INVESTIGATE, WI-ACTOR-03 | 흔적 조사·처방 읽기·분야 지식 | 조사와 지식 원장 확정은 다른 단계. 단순 카드 열람은 권위 WI가 아닐 수 있음 |
| 계획 설정 | WI-ACTOR-PLAN-SET | 다음날 확장 계획 WI-NATURE-15 | 기존 특화 관계. 계획 설정과 실제 건설 비용/효과 구분 |
| 섭취 | WI-ACTOR-CONSUME | 약초차·식량·후반 포션, 주민 소비 WI-ORDER-07 | 공통 소비 및 대상 효과. 물량·효과·재복용은 승인된 프로필만 사용 |
| 쉬기·수면·명상 | WI-NATURE-14 및 체력/마나 기획 | 오두막 수면·제자리 휴식·명상 | 회복이라는 목적은 공통이나 별도 행위/수치다. 자연 회복은 시간 규칙이지 매초 새 명령이 아님 |
| 열원 관리 | WI-HEAT-SOURCE-STATE-CHANGE | 점화·연료 보충·소화 | 기존 한 WI의 작업 코드. 대상마다 새 WI를 만들 필요 없음 |
| 데우기 | Q371 / D354 | 물 데우기·식은 차 데우기 | 방향 확인, **정식 ID 미등록**. 열원 관리와 분리하고 달이기에서 공통 가열 처리를 조합 검토 |
| 제작 | WI-CRAFT-BREW, WI-NATURE-16 | 약초 달이기·보급 꾸러미 제작·후반 술식 | 제작군 아래 서로 다른 처방. 술식은 후속 후보이며 데우기만으로 포션 생성 금지 |
| 내용물 옮기기 | WI-FARM-WATER-TRANSFER, Q347~348·364~367 | 물 뜨기·차 옮겨 담기·용기 비우기 | 액체 전송 공통 계산 후보. 농업 용수 WI를 개인 물병 명령으로 무조건 전용하지 않음 |
| 건설·수리 | WI-CON-* 및 WI-WORLD-04 | 오두막·농장 시설·초소·방호 구간 | 비실행 건설군 + 단일 책임 WI의 절차 조합 |
| 경작·관리·수확 | WI-FARM-01~04 및 토양/용수 WI | 밭갈이·파종·토양 개선·생육 관리·수확 | 농업군이 하나의 거대 경작 명령이 되지 않도록 유지 |
| 보관·운반·검수 | WI-NATURE-13, WI-001/002, Town/Hub/Market WI | 거점 보관·인수·검수·피킹·포장·운송 | 공통 처리 재사용 가능, 각 소유권/완료 책임과 자동 발생원은 유지 |
| 위임·협력·배정 | WI-WORLD-01/02, WI-SQUAD-ASSIGN 등 | 농사 보조·초소 배정·탐사 파견 | 공동체 편입·고용·실제 업무 배정은 서로 다른 확정 |
| 대응·전투·귀환 | WI-NATURE-11, WI-COMBAT-*, WI-FARM-DEFENSE-* | 직접 전투·분대 명령·방어 결과·귀환 | 조종 모드와 실제 전투 결과를 구분 |
| 취소 | WI-NATURE-12, WI-WORLD-03 | 제작 취소·건설 취소 | 공통 취소 요청 후보. 작업별 보존/환불 정책은 유지하고 임시 중단과 구분 |
| 주문·거래·상환 | WI-ORDER-01, WI-TOWN-STOCK-REPLENISH 등 / Q356~359 | 소비 주문·재고 보충·NPC 외상·상환 | 주문 2종의 원장 유지. 신용/상환은 방향 기획이며 이번에 실행 WI로 등록하지 않음 |

옷 장착·도구 장착처럼 대상만 다른 경우는 하나의 WI와 슬롯/물품 프로필을 우선한다. 반대로 거래·고용·시공처럼 결과 원장과 대가가 다른 것은 비실행 군으로만 묶고 명령을 합치지 않는다.

## 4. 약초 폐루프 적용 예

```text
비실행: 제작·생존 활동
  ├─ 획득: 약초 채집
  ├─ 내용물 전송 후보: 수원 → 냄비
  ├─ 열원 관리: 점화 / 연료 보충 / 소화
  ├─ 데우기 후보
  │    ├─ 물 데우기
  │    └─ 식은 차 데우기
  ├─ 달이기: 배합 + 가열 조건 + 처방 완료
  ├─ 내용물 전송 후보: 냄비 → 컵/병
  └─ 섭취: 차 음용 → 개인 상태 변화 → 탐험/휴식
```

이는 정해진 이동 루트나 매번 반드시 반복해야 하는 전체 순서가 아니다. 이미 물·불·완성 차가 있다면 해당 현재 상태에서 가능한 행동을 선택한다. 물을 데운다고 차·약효·정수 결과가 자동 생성되지는 않는다. 부모·자식 양쪽에서 Confirm·재료 소비·ActionRecord를 두 번 만들지 않는다.

## 5. 기존 관계의 검토 주의점

- `wi-family:threat-core-clear`에 열원 상태 변경이 연결돼 있다. 이는 열원이 위협 대응에 참여하는 문맥 관계로 읽을 수 있으나 **불 켜기=위협 핵 제거**라는 행위 특화로 해석하면 안 된다. 기존 등록은 수정하지 않고 개발 검토 사항으로 남긴다.
- `casualty-response` 군의 멤버는 0개다. 분류 항목 존재를 사상자 치료·보충 기능 구현으로 보고하지 않는다.
- 물품 소비→주민 소비 등 일부 관계가 RegistrationOnly다. 타입 계층이나 실제 부모 코드 호출이 구현됐다는 뜻이 아니다.
- 기능이 같은 이름이라도 플레이어 직접/자동/NPC 정책과 권한·결과가 다를 수 있다.
- LH/배치 엔진의 자동 조립, Sky 날씨 공급, 로컬 LLM 대사 생성, UI 창 열기, View 캡처, Save/Replay 기술 처리는 분류 편의를 위해 새 플레이어 WI로 만들지 않는다. 권위 변화가 필요한 사용자 확정만 별도 검토한다.

## 6. 문답 주제와 기존 추출 연결

아래는 기존 구현 범위 원장의 **주제 단위 추출 연결 사본**이다. 질문 범위 내 모든 질문이 나열된 모든 WI를 승인한 것으로 해석하지 않는다. 최신 Q340~377과 체력/마나 의미 ID는 각 원문 및 검색에서 별도 보완하며, 현재 E나 차단은 이 표에 복제하지 않는다.

| 주제 원문 | 기존 Q 범위 | 기존 주제의 WI 연결 |
| --- | --- | --- |
| [Nature 거점·수면·날씨·방어](PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md) | 1-5,23-35,132,141,149,153,156 | `WI-ACTOR-02`、`WI-NATURE-11`、`WI-NATURE-15` |
| [플레이어 내면·명상·계획·공동체 마음](PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md) | 6-22,40-44,65-67,122-125,196-198 | `WI-REFLECT-01`、`WI-CARD-01` |
| [Nature 자원·LandUse·건설](PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md) | 36-39,51-60,134 | `WI-CON-01`、`WI-NATURE-16` |
| [약초·Recipe·조합 제작](PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md) | 45-50,61-64,68-71,131,133,142,150,157,269-296 | `WI-ACTOR-03`、`WI-NATURE-HERB-GATHER`、`WI-CRAFT-BREW`、`WI-ACTOR-CONSUME` |
| [저장·Load·재진입](PlanningSessions/저장재진입/save-load-runtime.inquiry.r1.md) | 72-76,139-140 | 소비 WI에서 연결할 공통 모듈/미결속 |
| [Farm 건물·공간·배치·협력](PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) | 77-121,126-130,136,143-144,146-147,155 | `WI-ACTOR-02`、`WI-ACTOR-03`、`WI-FARM-01`、`WI-FARM-02`、`WI-FARM-03`、`WI-CON-01`、`WI-WORLD-01`、`WI-WORLD-02`、`WI-WORLD-03`、`WI-WORLD-04`、`WI-WORLD-05` |
| [Town 주문·입고·회랑 안전화](PlanningSessions/Town주문수령/town-order-pickup.inquiry.r1.md) | 135,137-138,145,148,151-152,154,158-160 | `WI-ORDER-06`、`WI-WORLD-01`、`WI-WORLD-02`、`WI-WORLD-04` |
| [지역 오행·몬스터·개척·회랑 전술](PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md) | 161-195 | `WI-ACTOR-03`、`WI-NATURE-01`、`WI-NATURE-03`、`WI-NATURE-04`、`WI-CON-01`、`WI-WORLD-01`、`WI-WORLD-02`、`WI-WORLD-06`、`WI-WORLD-07` |
| [공동체 편입·손님·원격 응대](PlanningSessions/공동체편입방문/community-membership-visitor.inquiry.r1.md) | 199-219 | `WI-CON-01`、`WI-COMMUNITY-VISITOR-STAY` |
| [시스템 보조 건물 배치](PlanningSessions/배치엔진보조/building-placement-assistance.inquiry.r1.md) | 220-222 | 소비 WI에서 연결할 공통 모듈/미결속 |
| [Farm 병영·방위·분대 운영](PlanningSessions/Farm병영방위/farm-barracks-defense.inquiry.r1.md) | 223-239 | `WI-FARM-DEFENSE-MOBILIZE`、`WI-FARM-DEFENSE-RESOLVE`、`WI-FARM-DEFENSE-RETURN` |
| [Hub 수요·분배·출고 준비](PlanningSessions/Hub수요분배/hub-demand-allocation.inquiry.r1.md) | 240-250 | `WI-HUB-03` |
| [생존경제·생산·소비·비축](PlanningSessions/생존경제/survival-economy.inquiry.r1.md) | 251-266 | 소비 WI에서 연결할 공통 모듈/미결속 |
| [Solo 업무 위임·예외](PlanningSessions/솔로업무위임/solo-work-delegation.inquiry.r1.md) | 267-268 | `WI-FARM-04` |
| [Farm 경관·H 패턴 재고·LH 조립](PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md) | 297-339 | 소비 WI에서 연결할 공통 모듈/미결속 |

후속 연결: Q340~351은 약초 입력·용기·작업·효과, Q352~355는 술식/관심/관련 NPC, Q356~359는 신용/상환, Q360~367은 차의 중단/회복/휴대/소비, Q368~377은 HB-01 검토 묶음이다. 후속 전체를 확정으로 묶지 않고 개별 recordStatus를 따른다.

## 7. 다음 개발에 넘길 정리 순서

1. 기존 18군·5특화의 의미를 보존하고 아래 49개 미분류 항목의 관계를 검토한다. 이 문서의 탐색 묶음 이름을 새 실행 ID로 일괄 등록하지 않는다.
2. 먼저 기존 보편 WI 재사용으로 닫을 수 있는 장착·획득·섭취·계획을 확인한다. 기존 자식 ID/저장 호환은 유지한다.
3. 약초 활성 폐루프에서 필요한 데우기·액체 전송의 독립 결과/프로필 경계를 확정한다. Q371 보편화 방향과 HB-01 미승인 수치/행동은 구분한다.
4. 관련 기획·작업 명세·연구·작성 경로를 결속한 뒤 개발 통합 담당이 좁게 구현한다. 체계 정리 때문에 이미 승인된 다른 개발을 멈추지 않는다.
5. 검증은 비용/효과/행위 기록 1회, Preview 무변경, 멱등 Confirm, 하위 프로필별 권한/실패 차이, 기존 Save·Local/Remote 호환 및 필요한 표현 증거를 포함한다.
6. 상위군의 존재를 하위 전체 E로 전파하지 않는다. 실제 Logic/Presentation 중 낮은 값을 통합 E로 유지한다.

## 부록 A. 기존 비실행 군 18개

멤버 수는 중복 소속을 포함한 각 군의 수다. 고유 WI 합계는 56개다.

| 기존 ID | 제목 | 멤버 수 |
| --- | --- | --- |
| `wi-family:consumption` | 물품 섭취 | 2 |
| `wi-family:planning` | 개인 계획 | 2 |
| `wi-family:combat` | 전투 지휘 | 2 |
| `wi-family:community` | 공동체 관계 | 9 |
| `wi-family:construction` | 건설 활동 | 8 |
| `wi-family:crafting` | 조합 제작 | 1 |
| `wi-family:expedition` | 탐사 활동 | 2 |
| `wi-family:farming` | 농업 활동 | 7 |
| `wi-family:heat-source` | 열원 관리 | 1 |
| `wi-family:logistics` | 물품 공급·보관 | 13 |
| `wi-family:economy` | 생존 경제 | 2 |
| `wi-family:gathering` | 자원 획득 | 4 |
| `wi-family:regeneration` | 세계 자원 재생 | 1 |
| `wi-family:casualty-response` | 사상자 대응 | 0 |
| `wi-family:land-improvement` | 농지 조성 | 1 |
| `wi-family:threat-core-clear` | 위협 핵 정리 | 1 |
| `wi-family:route-safety` | 회랑 안전화 | 3 |
| `wi-family:noncombat-response` | 비전투 위협 대응 | 2 |

## 부록 B. 기존 명시 특화 관계 5개

| 기존 부모 WI | 기존 자식 WI | 등록 상태 |
| --- | --- | --- |
| `WI-ACTOR-CONSUME` | `WI-ORDER-07` | RegistrationOnly |
| `WI-ACTOR-01` | `WI-NATURE-05` | Existing |
| `WI-ACTOR-01` | `WI-NATURE-HERB-GATHER` | RegistrationOnly |
| `WI-ACTOR-PLAN-SET` | `WI-NATURE-15` | RegistrationOnly |
| `WI-CON-BLUEPRINT-PLACE` | `WI-NATURE-07` | RegistrationOnly |

## 부록 C. 기존 WI 105개 탐색 배치

아래 묶음은 검토 편의를 위한 **제안 분류**다. 기존 WI군 칸만 등록 관계 원장의 사실을 옮겼다. 소속 없음은 이 파일에서의 공백이지 코드 미구현 판정이 아니다. 표의 실행 종류는 기존 대장을 그대로 보존한다.

### 획득·수집

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-ACTOR-01` | 물품 획득 | Command | 자원 획득 |
| `WI-NATURE-05` | 벌목 도끼 획득 | Command | 자원 획득 |
| `WI-NATURE-18` | 벌목 통나무 줍기 | Command | 자원 획득 |
| `WI-NATURE-HERB-GATHER` | 약초 채집 | Command | 자원 획득 |
| `WI-NATURE-06` | 나무 벌목 작업 시작 | Command | 이 관계 대장에서는 미분류 |
| `WI-FARM-04` | 익은 농작물 수확 | Command | 농업 활동 |

### 장착

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-ACTOR-02` | 장착 상태 변경 | Command | 이 관계 대장에서는 미분류 |
| `WI-WORLD-06` | 일행 역할 카드 장착 | Command | 이 관계 대장에서는 미분류 |

### 조사·학습·결과 확인

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-ACTOR-03` | 지식 습득 | Command | 이 관계 대장에서는 미분류 |
| `WI-NATURE-01` | 자연 지역 위험 징후 확인 | Command | 이 관계 대장에서는 미분류 |
| `WI-NATURE-TRACE-INVESTIGATE` | 자연 흔적 조사 | Command | 탐사 활동 / 비전투 위협 대응 |
| `WI-WORLD-05` | 새로운 지역 발견 | Command | 이 관계 대장에서는 미분류 |
| `WI-CITY-04` | 도심 서비스 결과 확인 | Command | 이 관계 대장에서는 미분류 |
| `WI-REVIEW-01` | NPC 업무 결과 검토 확정 | Command | 이 관계 대장에서는 미분류 |

### 계획·운영 정책

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-ACTOR-PLAN-SET` | 개인 계획 설정 | Command | 개인 계획 |
| `WI-NATURE-15` | 다음 날 거점 확장 계획 선택 | Command | 개인 계획 |
| `WI-SURVIVAL-RATION-POLICY-SET` | 생존 배급 정책 설정 | Command | 생존 경제 |
| `WI-WORLD-07` | 세계 활동 상태 변경 | Command | 이 관계 대장에서는 미분류 |
| `WI-WORLD-08` | 하루 운영 턴 마감 | Command | 이 관계 대장에서는 미분류 |

### 섭취·휴식·회복

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-ACTOR-CONSUME` | 물품 섭취 | Command | 물품 섭취 |
| `WI-ORDER-07` | 주민 상품 소비 | Command | 물품 섭취 |
| `WI-NATURE-14` | 오두막에서 수면·새벽 맞기 | Command | 이 관계 대장에서는 미분류 |
| `WI-NATURE-04` | 탐사대 안전 회복 | Command | 이 관계 대장에서는 미분류 |

### 성찰·내면 표현

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-REFLECT-01` | 승인 자료로 거점 성찰 확정 | Command | 이 관계 대장에서는 미분류 |
| `WI-CARD-01` | 현재 세계의 메이저 아르카나 활성화 | Command | 이 관계 대장에서는 미분류 |

### 재배·토지 관리

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-FARM-01` | 경작지 밭갈이 | Command | 농업 활동 / 농지 조성 |
| `WI-FARM-02` | 경작지 씨앗 파종 | Command | 농업 활동 |
| `WI-FARM-03` | 농작물 생육 관리 | Command | 농업 활동 |
| `WI-FARM-FIELD-BOUNDARY-CONFIRM` | 밭 경계 확정 | Command | 농업 활동 |
| `WI-FARM-SOIL-AMEND` | 토양 개량 | Command | 농업 활동 |
| `WI-FARM-WATER-TRANSFER` | 농업 용수 이송 | Command | 농업 활동 |

### 열원 관리

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-HEAT-SOURCE-STATE-CHANGE` | 열원 상태 변경 | Command | 열원 관리 / 위협 핵 정리 |

### 제작·포장

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-CRAFT-BREW` | 배합물 달이기 | Command | 조합 제작 |
| `WI-NATURE-16` | 현장 보급 꾸러미 제작 | Command | 이 관계 대장에서는 미분류 |
| `WI-FARM-06` | 출하 물량 포장 | Command | 이 관계 대장에서는 미분류 |
| `WI-HUB-05` | 피킹 화물 포장 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-ORDER-04` | 주문 상품 포장 | AutomaticTransition | 이 관계 대장에서는 미분류 |

### 건설·수리·해체

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-CON-01` | 영역 건물 건설 확정 | Command | 건설 활동 |
| `WI-CON-BLUEPRINT-PLACE` | 건설 청사진 배치 | Command | 건설 활동 |
| `WI-CON-MATERIAL-DEPOSIT` | 건설 재료 투입 | Command | 건설 활동 |
| `WI-CON-WORK-CONTRIBUTE` | 건설 시공 기여 | Command | 건설 활동 |
| `WI-CON-DEMOLISH` | 건설물 해체 | Command | 건설 활동 |
| `WI-NATURE-07` | 오두막을 지을 터 선정 | Command | 건설 활동 |
| `WI-NATURE-08` | 오두막 건설 작업 시작 | Command | 건설 활동 |
| `WI-NATURE-03` | 훼손된 자연 경로 복원 | Command | 회랑 안전화 |
| `WI-WORLD-04` | 손상된 시설 수리 | Command | 건설 활동 / 회랑 안전화 |

### 보관·피킹·진열

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-NATURE-13` | 획득 자원 거점 보관 | Command | 이 관계 대장에서는 미분류 |
| `WI-FARM-05` | 수확물 집하장 모으기 | Command | 이 관계 대장에서는 미분류 |
| `WI-002` | 검수 완료 화물 창고 적재 | Command | 물품 공급·보관 |
| `WI-TOWN-STOCK-PUTAWAY` | Town 후방 재고 적재 | Command | 물품 공급·보관 |
| `WI-MARKET-04` | 검수 상품 후방 창고 적재 | Command | 물품 공급·보관 |
| `WI-MARKET-05` | 매장 진열대 상품 보충 | Command | 물품 공급·보관 |
| `WI-HUB-04` | 출고 대상 재고 피킹 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-ORDER-03` | 주문 상품 피킹 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-ORDER-05` | 주문 상품 수령 준비 | AutomaticTransition | 이 관계 대장에서는 미분류 |

### 운송·상하차

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-LOG-01` | 출하 차량 상차 확정 | Command | 이 관계 대장에서는 미분류 |
| `WI-LOG-02` | 농장에서 출발 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-LOG-03` | 농장에서 물류 거점으로 화물 이동 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-LOG-04` | 물류 거점 도착 화물 하차 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-LOG-05` | 물류 거점 도착 화물 인수 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-HUB-06` | 출고 차량 상차 | Command | 이 관계 대장에서는 미분류 |
| `WI-MARKET-01` | 물류 거점에서 마트로 운송 | Command | 이 관계 대장에서는 미분류 |
| `WI-TOWN-SUPPLY-DISPATCH` | Town 공급 운송 출발 확정 | Command | 물품 공급·보관 |

### 주문·할당·서비스

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-HUB-03` | 출고 대상 재고 요청 | Command | 이 관계 대장에서는 미분류 |
| `WI-HUB-DEMAND-ALLOCATE` | Hub 수요 재고 할당 | Command | 물품 공급·보관 |
| `WI-HUB-SUPPLY-TASK-ACCEPT` | Hub 조달 과제 수락 | Command | 물품 공급·보관 |
| `WI-ORDER-01` | 주민 주문 확정 | Command | 이 관계 대장에서는 미분류 |
| `WI-ORDER-02` | 주문 상품 재고 예약 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-TOWN-STOCK-REPLENISH` | Town 재고 보충 주문 | Command | 물품 공급·보관 |
| `WI-CITY-01` | 도심 서비스 수요 확정 | Command | 이 관계 대장에서는 미분류 |
| `WI-CITY-02` | 도심 서비스용 지역 재고 배정 | AutomaticTransition | 이 관계 대장에서는 미분류 |
| `WI-CITY-03` | 도심 주민 서비스 처리 | Command | 이 관계 대장에서는 미분류 |
| `WI-INVENTORY-BELOW-RESERVE-SALE-CONFIRM` | 목표 비축 미달 판매 확정 | Command | 생존 경제 |

### 검수·인수·수령

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-001` | 입고 화물 검수 | Command | 물품 공급·보관 |
| `WI-TOWN-DELIVERY-INSPECT` | Town 납품 검수 | Command | 물품 공급·보관 |
| `WI-TOWN-DELIVERY-RECEIVE` | Town 납품 인수 | Command | 물품 공급·보관 |
| `WI-MARKET-02` | 마트 도착 화물 인수 | Command | 물품 공급·보관 |
| `WI-MARKET-03` | 마트 입고 상품 검수 | Command | 물품 공급·보관 |
| `WI-ORDER-06` | 주민 주문 상품 수령 | Command | 이 관계 대장에서는 미분류 |

### 관계·참여·권한

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-COMMUNITY-COOPERATION-PROPOSE` | 공동체 협력 제안 | Command | 공동체 관계 |
| `WI-COMMUNITY-ENTRANCE-POLICY-SET` | 공동체 출입 정책 설정 | Command | 공동체 관계 |
| `WI-COMMUNITY-HIRE` | NPC 고용 확정 | Command | 공동체 관계 |
| `WI-COMMUNITY-MEMBERSHIP-CONFIRM` | 공동체 정식 편입 확정 | Command | 공동체 관계 |
| `WI-COMMUNITY-REMOTE-RESPONSE` | 원격 응대 지시 확정 | Command | 공동체 관계 |
| `WI-COMMUNITY-SUPPORT-MISSION-JOIN` | 공동 지원 임무 참여 | Command | 공동체 관계 |
| `WI-COMMUNITY-VISITOR-STAY` | 방문자 임시 체류 결정 | Command | 공동체 관계 |
| `WI-GUEST-PERMISSION-SET` | 손님 활동 권한 설정 | Command | 공동체 관계 |

### 업무 배정·위임・보급

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-WORLD-01` | NPC에게 반복 업무 배정 | SharedPolicy | 이 관계 대장에서는 미분류 |
| `WI-WORLD-02` | NPC에게 업무 역량 위임 | Command | 공동체 관계 |
| `WI-NATURE-17` | 현장 보급 제작 업무 위임 | Command | 이 관계 대장에서는 미분류 |
| `WI-EXPEDITION-DISPATCH` | 탐사 임무 파견 | Command | 탐사 활동 / 회랑 안전화 |
| `WI-SQUAD-ASSIGN` | 경비 초소 분대 배정 | Command | 이 관계 대장에서는 미분류 |
| `WI-SQUAD-SUPPLY` | 경비 분대 식량·장비 보급 | Command | 이 관계 대장에서는 미분류 |

### 전투 대응·지휘·귀환

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-COMBAT-DIRECT-CONTROL-SET` | 직접 전투 조종 전환 | Command | 전투 지휘 |
| `WI-COMBAT-TACTICAL-COMMAND` | 분대 전술 명령 확정 | Command | 전투 지휘 |
| `WI-FARM-DEFENSE-MOBILIZE` | 방위 분대 소집 | Command | 이 관계 대장에서는 미분류 |
| `WI-FARM-DEFENSE-RESOLVE` | Farm 방어 성공 결과 발현 | Command | 이 관계 대장에서는 미분류 |
| `WI-FARM-DEFENSE-RETURN` | Farm 방위 분대 초소 귀환 인계 | Command | 이 관계 대장에서는 미분류 |
| `WI-NATURE-11` | 황혼 위협 대응 방식 확정 | Command | 이 관계 대장에서는 미분류 |

### 이동·출입·후퇴

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-NATURE-02` | 안전 거점으로 긴급 후퇴 | Command | 비전투 위협 대응 |
| `WI-NATURE-09` | 오두막 안으로 들어가기 | Command | 이 관계 대장에서는 미분류 |
| `WI-NATURE-10` | 오두막 밖으로 나가기 | Command | 이 관계 대장에서는 미분류 |

### 취소

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-NATURE-12` | 진행 중 작업 취소 | Command | 이 관계 대장에서는 미분류 |
| `WI-WORLD-03` | 진행 중 세계 업무 취소 | Command | 이 관계 대장에서는 미분류 |

### 세계 자동 재생

| 기존 WI | 현재 제목 | 실행 종류 | 기존 등록 관계 대장의 WI군 |
| --- | --- | --- | --- |
| `WI-WORLD-RESOURCE-REGENERATE` | 세계 자원 재생 | AutomaticTransition | 세계 자원 재생 |

## 검증과 한계

- WI 대장 105개와 부록 C의 ID 집합/개수 일치, 중복 0을 검사한다.
- 기존 가족 멤버/특화 양 끝의 ID가 현행 WI에 있는지 확인한다.
- 문답 검색 신선도 Validate를 수행했다. 원문 소실 3건은 복원하지 않는다.
- 게임 C#·Runtime·Save·Scene·Game View는 이번 조사에서 실행/변경하지 않았다. 등록 현황 정리를 구현 성숙도 조사로 확대하지 않는다.

