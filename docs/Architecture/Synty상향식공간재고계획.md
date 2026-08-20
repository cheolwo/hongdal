# Synty 상향식 공간 재고 계획

## 목적

보유한 POLYGON Nature·Farm·Town·City·Construction 팩을 AreaSet 작업이 시작될 때까지 보관만 하지 않고, 위치 독립 공간 의미와 조립 후보로 미리 연구해 재사용 가능한 설계 재고를 확보한다.

상향식 재고는 `내가 가진 표현 재료로 어떤 공간 가능성을 만들 수 있는가`를 탐색한다. 실제 지역 세계의 권위는 계속 AreaSet·LandscapeGraph·공공데이터 근거에 있다.

## 게임 기획이 재고 범위를 통제한다

H를 먼저 축적하되 Synty 자산이나 보기 좋은 공간 조합만으로 카드를 늘리지 않는다. 모든 상호작용 H1은 기존 WI 또는 명시된 예상 플레이에 연결되고, H2~H4는 `Nature 생활·위협·회복`, `Farm 생산·생존`, `Town 생활·시장 안전`, `City/Hub 물류 회복력` 가운데 하나의 게임 기획 묶음에 속해야 한다. 여러 세계를 잇는 Farm–Hub–Town 청사진은 독립 재고 확장축이 아니라 네 묶음을 연결하는 교차 세계 조정안으로만 사용한다.

입고 조건은 다음과 같다.

1. H1은 플레이어가 수행하거나 관찰할 행위, 공간 역할과 앞뒤 연결을 설명한다.
2. 팩 표현 H1은 연결 가능한 상호작용 H1이 있어야 탐색 재고가 된다. 연결이 없으면 `IdeaInventory`로 격리한다.
3. H2는 여러 H1을 실제 계획기에서 한 단위로 놓을 수 있는 물리 블록이고, H3는 여러 H2를 도로·경계·연결구로 묶어 한 단위로 놓을 수 있는 구역 조립안이다. 반복 플레이와 사건 흐름은 이 공간 재고의 주 이름이 아니라 활용 유형과 검증 계보다.
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
| H2 블록 조립법 | 34 | 기존 33개에 Nature 복원 공간과 Town 구호 인계점을 잇는 혼합 대피·구호 전환 블록을 추가했다. |
| H3 경관 청사진 | 18 | 기존 17개에 Town 회수·구호에서 Nature 복원·안전 귀환까지 잇는 혼합 인계 경관을 추가했다. |
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
7. 이론 적격 H 설계를 명시적 H4 세계 의도에 적용해 전용 `area-set:theory:*`와 H3 Node·Edge·Connector, H4 GraphRelation을 결정적으로 조립한다. 이는 이론 E5이며 실제 지역·공공데이터·Runtime 권위가 아니다. 공공데이터 계보는 E6에서만 결속한다.

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

P1은 플레이어가 상시 머무는 Nature의 위협·대피·회복 폐루프를 먼저 닫는다. P2와 P3는 Farm·Town 전문 경관에서 생긴 사건 결과를 P1의 자연권 회복 공간으로 인계한다. 이 순서는 위치 독립 설계 재고 제작 순서다. H2 이론 적격은 필수 H1, 내부 관계, 연결구, 위상, 크기 범위와 결정성의 자동 검사로 판단한다. 사람 검토는 후속 일괄 품질 검토이며 생산을 차단하지 않는다.

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

H1 후보는 공간 역할·능력·업무 용량·내부 관계·외부 연결구와 E3 WI 재실행 증거로 이론 적격을 판정한다.

H2 후보는 필수 H1, 상대 위치, 위상, 내부 관계, 크기 변형과 외부 연결구가 결정적으로 닫히면 `TheoryQualified`로 자동 승격한다.

H3 후보는 필수 이론 적격 H2, Node·Edge·Connector 역할과 경관 내부 흐름이 닫히면 `TheoryQualified`로 자동 승격한다.

H4 후보는 작성된 세계 의도, 필수 H3, 지역 내부 관계와 외부 연결 역할이 닫히면 이론 E5 입력으로 사용할 수 있다.

이론 적격 H 설계를 전용 `area-set:theory:*`에 배치하고 이동 경로를 닫는 작업은 `E5TheoryQualified`다. 그 인스턴스에 필요한 공공데이터를 선별해 계보를 연결하는 작업은 E6이며 실제 서버·Unity Runtime 검증은 E7이다. 이론 E5를 사람 승인, 실제 지역 AreaSet, E6 또는 E7 완료 상태로 자동 대체하지 않는다.

자동 공장은 H2 34개, H3 18개와 Nature·Farm·City/Hub·Town 이론 E5 인스턴스를 반복 생성한다. 팩 내부 H3가 준비되면 AreaSet 후보가 H2를 임시로 직접 소유하지 않고 해당 H3를 통해 하위 블록을 추적한다. Nature–Town 혼합 H3는 두 청사진의 선택 가능한 교차 경관 계보이며 실제 Graph나 지역 E5 인스턴스로 자동 복제하지 않는다. 사람 검토 결과는 별도의 `DeferredBatchReview`로 기록하며 생성 중단 조건이나 자동 승인 근거로 사용하지 않는다.

Prefab 이름, GUID, Material, Scene 경로, GameObject 이름은 어느 승격 관문에서도 공간 StableId나 Simulation 권위가 될 수 없다.

## H2·H3 팩 주도 패턴 이름

H2·H3의 저장·계보 참조는 기존 `h2-candidate:*`, `h3-candidate:*` StableId를 유지한다. AreaSet 구성·Unity 검토·사람 문서에서 구분하는 이름은 [`h-pattern-names.v1.json`](../../eng/world-seedbeds/synty-bottom-up-inventory/h-pattern-names.v1.json)의 별도 패턴 코드를 사용한다.

```text
{NATURE|FARM|CITY|TOWN|MIX}-H{2|3}-{FAMILY}-{SEQUENCE:00}
```

예를 들어 `TOWN-H2-VILLAGE-01`의 주 이름은 이름 대장의 물리 공간 이름인 `저층 주거·생활광장 블록`이고, `타운 빌리지 패턴 01 — 저층 생활광장형`은 보조 게임플레이 활용 유형이다. `TOWN-H3-VILLAGE-01`도 `저층 주거·마트 구역`을 먼저 보여 주고 `타운 빌리지 경관 01 — 저층 생활·시장형`을 보조 정보로 표시한다. 패턴 번호는 서로 다른 공간 조립을 구분하며 기준 경관 문법 A/B/C 표현 변형과 혼합하지 않는다.

단일 팩만 사용하는 `SinglePack`, 주도 팩과 보조 팩을 함께 쓰는 `LeadPackWithSupport`, 팩 경계를 잇는 `CrossPackTransition`을 분리한다. 혼합 회랑은 `MIX`로 이름 붙이고 Construction은 지원 기능층으로만 기록한다. 패턴 이름은 H 승인·E5·Prefab·Simulation 권위를 만들지 않는다.

### 공간 계획기용 배치 계약

H2와 H3의 주 용도는 행동 목록을 분류하는 것이 아니라 공간 계획기에 놓고 이어 붙이는 것이다. 이론 공간 공장은 이름 대장 r6부터 다음 계약을 함께 생성한다.

- H2 `BlockPattern`: 로컬 미터 좌표계의 H1 배치, 내부 이동 관계, 기준 경계, 90도 회전 단위, 크기 변형과 외부 연결 역할을 가진다.
- H3 `LandscapeAssemblyPattern`: H2 배치, 블록 사이 이동 관계, 기준 경계, 구역 형태와 외부 연결 역할을 가진다.
- `spatialDisplayNameKo`: 이름 대장이 선언한 물리 공간 이름이며 목록·계획기에서 먼저 표시한다.
- `gameplayProfileNameKo`: 기존 행동 중심 이름이며 해당 공간에서 어떤 플레이를 수용하는지 설명하는 보조 분류다.
- `spatialFormCode`: `StreetBlock`, `LinearBlock`, `CompoundBlock`, `DistrictAssembly`, `CorridorAssembly`처럼 배치 형식을 나타낸다.
- `referenceBoundsMeters`: 결정적 상대 배치에서 계산한 이론상 기준 경계다. 실제 지역 면적·건물 경계·공공데이터 근거가 아니다.

기존 H2·H3 StableId와 팩·계열 패턴 코드는 유지한다. 따라서 저장·계보 참조를 깨지 않고 공간 계획 화면은 `물리 공간 이름 → 형태·크기·연결구 → 게임플레이 활용 유형` 순으로 재고를 제시할 수 있다.

### 팩별 패턴 생산 순서

H2는 여러 H1을 상대 배치와 내부 동선으로 묶고 입구·출구를 가진 `BlockPattern`이다. H3는 여러 H2와 외부 연결 역할을 묶은 `LandscapeAssemblyPattern`이다. 따라서 새로운 공간 재고는 다음 순서로 늘린다.

1. `PackNativeH2`: Nature·Farm·City·Town 각 팩만으로 구성 가능한 H2 블록 패턴을 먼저 확보한다.
2. `PackNativeH3`: 같은 팩의 H2 블록을 조합하여 팩 내부 H3 경관 패턴을 만든다.
3. `LeadPackWithSupport`: 주도 팩의 의미를 유지하면서 Construction이나 다른 팩을 기능·전환 보조층으로 사용한다.
4. `CrossPackH2`: 두 팩의 경계를 한 블록 안에서 직접 다루는 혼합 H2를 만든다.
5. `CrossPackH3`: 여러 팩의 H2·H3 인계가 닫힌 뒤 혼합 H3 경관으로 확장한다.

기존 혼합 패턴은 호환성과 이미 만든 게임 플레이 계보를 위해 보존한다. 다만 새 재고를 추가할 때는 팩 단독 H2와 팩 내부 H3의 부족분을 먼저 채운다. 이 순서는 표현 자산 생산 순서이며 특정 AreaSet 배치, 실제 Scene, WI E5, 공공데이터 E6 또는 실제 플레이 E7을 자동으로 증명하지 않는다.
