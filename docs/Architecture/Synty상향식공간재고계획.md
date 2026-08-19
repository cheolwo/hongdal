# Synty 상향식 공간 재고 계획

## 목적

보유한 POLYGON Nature·Farm·Town·City·Construction 팩을 AreaSet 작업이 시작될 때까지 보관만 하지 않고, 위치 독립 공간 의미와 조립 후보로 미리 연구해 재사용 가능한 설계 재고를 확보한다.

상향식 재고는 `내가 가진 표현 재료로 어떤 공간 가능성을 만들 수 있는가`를 탐색한다. 실제 지역 세계의 권위는 계속 AreaSet·LandscapeGraph·공공데이터 근거에 있다.

## 게임 기획이 재고 범위를 통제한다

H를 먼저 축적하되 Synty 자산이나 보기 좋은 공간 조합만으로 카드를 늘리지 않는다. 모든 상호작용 H1은 기존 WI 또는 명시된 예상 플레이에 연결되고, H2~H4는 `Nature 생활·위협·회복`, `Farm 생산·생존`, `Town 생활·시장 안전`, `City/Hub 물류 회복력` 가운데 하나의 게임 기획 묶음에 속해야 한다. 여러 세계를 잇는 Farm–Hub–Town 청사진은 독립 재고 확장축이 아니라 네 묶음을 연결하는 교차 세계 조정안으로만 사용한다.

입고 조건은 다음과 같다.

1. H1은 플레이어가 수행하거나 관찰할 행위, 공간 역할과 앞뒤 연결을 설명한다.
2. 팩 표현 H1은 연결 가능한 상호작용 H1이 있어야 탐색 재고가 된다. 연결이 없으면 `IdeaInventory`로 격리한다.
3. H2는 여러 H1이 만드는 반복 가능한 플레이 흐름, H3는 여러 H2가 만드는 사건·이동 경관, H4는 그 경험을 묶는 지역 세계 의도를 가져야 한다.
4. 게임 기획이나 WI 계보가 끊긴 카드는 자동 삭제하지 않고 아이디어 재고로 강등한다. 공식 H 승격과 E 단계 입력에는 사용하지 않는다.
5. H가 공간을 제안한 뒤 WI별 E 부족분을 계산한다. H 정의 안에 E6 공공데이터를 넣지 않는다.

기계 기준과 최신 감사·우선순위는 [`gameplay-led-h-policy.v1.json`](../../eng/world-seedbeds/synty-bottom-up-inventory/gameplay-led-h-policy.v1.json)과 [게임 기획 주도 H 공간 재고](../AI/generated/gameplay-led-h-inventory.md)를 따른다.

## 두 종류의 재고

- **승인 재고**: E3 세계 상호작용을 품고 시험 공간에서 다시 검증된 공식 H1 WI 공간 모판이다.
- **탐색 재고**: Synty 조합에서 발견한 H1 검토 후보, H2 블록 후보, H3 조립 후보다. 공간 역할·관계·연결구 설계 검토 없이 공식 H 정의로 자동 승격하지 않는다.

현재 설계 지식 재고는 다음과 같다.

| 분류 | 수량 | 의미 |
| --- | ---: | --- |
| H1 행동 공간 카드 | 52 | 승인 참조 5개와 Nature 위협·회복 5개, Farm 사건 대응 5개, Town 사건 대응 및 주문 포장 공간을 포함한 WI·능력 중심 장소 지식 |
| H1 팩 단독 표현 카드 | 32 | Nature 12·Farm 8·Town 6·City 6 의미군과 A/B/C 변형 |
| H2 블록 조립법 | 24 | H1을 위상과 연결구로 묶는 재사용 레시피. 이 가운데 사건 대응 H1에서 유도한 Nature 2·Farm 2·Town 2개를 P1~P3 우선순위로 관리한다. |
| H3 경관 청사진 | 13 | Farm·Hub·Town·회랑·Nature 경관 유형. Nature 생활·위협·회복, Farm 사건 격리·회복, Town 오염 통제·구호 경관을 포함한다. |
| H4 지역 청사진 후보 | 6 | 실제 AreaSet이 아닌 위치 독립 세계 구성 후보. Nature 생활·탐험권을 포함한다. |

초기 `catalog.v1.json`과 항목별 `catalog.v2.json`은 호환 입력으로 보존한다. 현재 `catalog.v3.json`은 기존 StableId를 유지하면서 행동 공간 H1과 팩 단독 표현 H1을 구분하고, 검토된 조립법의 문법→H1→H2→H3→H4 계보와 파일 SHA-256을 봉인한다. 팩 단독 H1과 H4 후보는 각각 `definitions/h1-expression/`, `definitions/h4/`와 대응 Markdown으로 결정적으로 생성한다.

## 지식 카드 상태와 조회

- `IdeaInventory`: 재사용 가능성을 기록한 아이디어다.
- `ExploratoryInventory`: WI 또는 예상 게임 행위, 공간 역할·능력·연결 관계를 갖춘 탐색 지식이다.
- `CandidateForReview`: 공식 H 정의 승격을 사람이 검토할 수 있는 후보다.
- `ApprovedReference`: 기존 공식 H 정의를 가리키는 승인 참조다.

WI가 아직 없어도 예상 게임 행위를 명시하면 아이디어·탐색 재고로 등록할 수 있다. 존재하지 않는 WI 식별자를 꾸며내지는 않는다. `query-spatial-design-knowledge.ps1`은 WI·예상 행위·공간 능력·팩·위상을 입력받아 H1→H2→H3 조합 후보와 미충족 조건을 제안하지만 상태를 승인하거나 AreaSet·경관 그래프 권위를 만들지 않는다.

## 관리 명령

```powershell
# v2 호환 원본 검증
pwsh -NoProfile -File eng/world-seedbeds/manage-spatial-design-knowledge.ps1 -Mode Check

# v3 상향 유도 원장·팩 카드·H4 청사진 검증/갱신
pwsh -NoProfile -File eng/world-seedbeds/manage-spatial-design-knowledge-v3.ps1 -Mode Check
pwsh -NoProfile -File eng/world-seedbeds/manage-spatial-design-knowledge-v3.ps1 -Mode Write

# 게임 기획 소속·고아 재고·H/E 우선순위 검증
pwsh -NoProfile -File eng/world-seedbeds/manage-gameplay-led-h-inventory.ps1 -Mode Check

# 수확·집하·포장 WI에 맞는 Farm/Nature 조합 후보 조회
pwsh -NoProfile -File eng/world-seedbeds/query-spatial-design-knowledge.ps1 `
  -WiIds WI-FARM-04,WI-FARM-05,WI-FARM-06 `
  -PackCodes Farm,Nature
```

기존 v2 행동 공간 지식을 수정할 때는 같은 항목의 JSON 실행 계약과 Markdown 설명·지시문을 함께 바꾼다. v3 팩 표현 H1·H4 카드와 `docs/AI/generated/` 결과는 기준 문법·검토 조립법에서 파생되므로 직접 수정하지 않는다.

## 제작 순서

1. 기존 승인 H1 다섯 개를 상향식 재고에서 참조한다.
2. 37개 E3 세계 상호작용과 예상 게임 행위 가운데 공간을 직접 요구하는 행위를 묶어 H1 아이디어·탐색·검토 후보를 만든다.
3. 각 H1 의미에는 기존 156개 기준 경관 문법에서 A/B/C 후보를 연결한다.
4. 여러 H1을 `Grid`, `ModifiedGrid`, `Linear`, `ContourAdaptive`, `Organic` 위상으로 묶어 H2 후보를 만든다.
5. 여러 H2 후보와 외부 연결 역할을 조합해 H3 청사진을 만든다.
6. 여러 H3와 세계 주제를 묶어 실제 지역 권위가 없는 H4 청사진 후보를 만든다.
7. 승인 H 설계를 실제 AreaSet에 적용하는 E5에서 지역 인스턴스의 도로·경계와 H3 Node·Edge·Connector, H4 GraphRelation을 조립한다. 공공데이터 계보는 E6에서만 결속한다.

## H1에서 H2로 올리는 현재 우선순위

기계 기준은 [`h2-composition-priorities.v1.json`](../../eng/world-seedbeds/synty-bottom-up-inventory/h2-composition-priorities.v1.json)이다. 현재 H1의 선행·후속 관계와 연결구를 분석해 다음 여섯 조합을 먼저 설계 재고로 관리한다.

| 우선순위 | H2 후보 | 필수 H1 흐름 |
| --- | --- | --- |
| P1 | 자연 위협 추적·대피 블록 | 위협 감시 → 사건 흔적 추적 → 긴급 후퇴 |
| P1 | 자연 복원·안전 회복 블록 | 복원 작업 → 안전 회복 → 탐색 재개 |
| P2 | 농장 사건 점검·격리 블록 | 외부 노출 점검 → 사건 격리 → 기상 보호 |
| P2 | 농장 손실 회복·복원 인계 블록 | 격리 인계 → 손실 회복 → 자연권 복원 물자 인계 |
| P3 | 생활권 오염 점검·정화 블록 | 오염 점검 → 격리 → 정화 폐기 인계 |
| P3 | 생활권 회수 안내·자연권 구호 블록 | 주민 회수 안내 → 생활 서비스 → 자연권 구호 인계 |

P1은 플레이어가 상시 머무는 Nature의 위협·대피·회복 폐루프를 먼저 닫는다. P2와 P3는 Farm·Town 전문 경관에서 생긴 사건 결과를 P1의 자연권 회복 공간으로 인계한다. 이 순서는 위치 독립 설계 재고 제작 순서다. 공식 H2 승격은 필수 H1, 내부 관계, 연결구, 위상과 크기 범위의 설계 검토로 판단하고 실제 지역 배치는 E5에서 별도로 판단한다.

P1~P3 여섯 후보는 [`h2-composition-recipes.v1.json`](../../eng/world-seedbeds/synty-bottom-up-inventory/h2-composition-recipes.v1.json)의 로컬 H1 배치·내부 관계·외부 연결구로 구현한다. 결정적 생성 결과는 [P1~P3 H2 조립안](../AI/generated/h2-composition-plans.md)에서 확인한다. 좌표계 `LocalMeters`는 재사용 조립안 내부의 상대 위치일 뿐 실제 지역 좌표가 아니며, 결과 상태 `DesignCandidateOnly / ReadyForPlanningReview`는 H2 설계 검토 대기 상태다. 실제 AreaSet 배치와 E5 증거를 뜻하지 않는다.

## 팩의 기본 역할

- Nature: 지형 골격, 숲 경계, 수변, 완충, 탐색 배경
- Farm: 생산구획, 작업마당, 집하, 포장, 농장 시설
- Town: 저층 주거, 생활도로, 소형 상권, 주민 활동
- City: Hub, 창고, 물류, 마트, 상하차 강조
- Construction: 독립 AreaSet이 아니라 골조, 공사, 복구, 격리와 상태 전환을 다른 팩 공간에 입히는 공통 조립 재료층
- Network·Transition: 팩 사이의 도로, Gate, 건물 전면, 수변 전환

Construction을 포함한 다섯 팩의 조립 규칙과 심리 영역의 두 발전소 상세안은 [심리·업무 영역 Synty 5팩 공간 조립 계획](심리업무영역Synty공간조립계획.md)을 따른다. 이 계획은 대표 Prefab을 표현 후보로만 사용하며 기존 H 대장·Stable ID·Simulation 권위를 자동 변경하지 않는다.

다섯 팩 활용 확장의 첫 구현 순서는 H 카드 추가가 아니라 기술 대장 확장이다. 기존 Farm·Town·City 1,535개 항목의 고유 식별자를 유지하면서 Nature 227개와 Construction 584개를 더해 2,346개 전부에 정규화 분류, 의미 자산군, 활용 트랙과 최소 한 계획 적용 영역 또는 보류 사유를 기록한다. Construction 자산군은 H1 팩 표현 카드로 자동 승격하지 않고 정상 운영·점검·공사·손상 격리·복구 재가동의 공통 상태층 후보로 분류한다.

2026-08-19 현재 `synty-pack-inventory.v2` 기술 대장을 생성해 위 범위를 구현했다. 2,346개는 1,499개 의미 자산군, 자동 분류 2,345개, 사람 검토 대기 1개로 나뉜다. Vehicle 51개는 `Vehicles`·`vehicle`로 분류하고 Nature `Misc` 1개만 사람 검토에 남긴다. 기존 3팩 `inventoryId` 재료는 유지하고 원본 폴더명이 다른 `Environment(s)`는 별도 정규화 필드로 흡수한다. 이 대장은 표현 재료 재고이며 H 승인·AreaSet 사실·Simulation 상태를 만들지 않는다.

## 승격 관문

H 계층은 위치 독립 공간 설계의 조립 깊이만 나타낸다. 공공데이터 목적·출처·좌표계·원본·파생 hash는 H1~H4 정의에 넣지 않고 E6에서 선택한 WI와 E5 경관 인스턴스에만 연결한다.

H1 검토 후보는 공간 역할·능력·업무 용량·내부 관계·외부 연결구와 E3 WI 재실행 증거로 설계 승인한다.

H2 후보는 필수 H1, 상대 위치, 위상, 내부 관계, 크기 변형과 외부 연결구가 결정적으로 닫히면 설계 승인한다.

H3 후보는 필수 H2, Node·Edge·Connector 역할과 경관 내부 흐름이 닫히면 설계 승인한다.

H4 후보는 사람의 세계 의도, 필수 H3, 지역 내부 관계와 외부 연결 역할이 닫히면 설계 승인한다.

승인 H 설계를 특정 AreaSet에 배치하고 이동 경로를 닫는 작업은 E5다. 그 인스턴스에 필요한 공공데이터를 선별해 계보를 연결하는 작업은 E6다. H4 설계 후보를 실제 AreaSet이나 E6 완료 상태로 자동 대체하지 않는다.

Prefab 이름, GUID, Material, Scene 경로, GameObject 이름은 어느 승격 관문에서도 공간 StableId나 Simulation 권위가 될 수 없다.
