# Unity POLYGON Farm 식품 Asset·HS·가격 연결 조사

## 1. 목적과 범위

- 기준일: 2026-08-09
- 조사 대상: `C:\Users\user\ssalddel\Assets\Synty\PolygonFarm\Prefabs`
- 비교 대상:
  - `FoodPriceCrosswalkCatalog`: 현재 운영 코드가 사용하는 HS prefix와 KAMIS 국내 가격 품목 연결표
  - `OfficialFoodIngredientHsCandidateMatcher`: 식품명에서 검토할 HS 후보와 추가 확인 항목
- 이번 결과: 문서 조사만 수행하며 Unity prefab·Scene·catalog와 서버 코드는 변경하지 않는다.

이 문서의 목적은 POLYGON Farm의 외형을 HS 분류나 가격 자료의 근거로 사용하는 것이 아니다. 이미 조사하고 있는 농식품을 World에서 표현할 수 있는지 확인하고, 표현 가능 여부와 데이터 연결의 확정 수준을 분리하는 데 있다.

## 2. 결론

POLYGON Farm에는 이름으로 식별 가능한 식품 품목군 29개, 이에 속하는 prefab 177개가 있다. 이 수치는 재배 단계·수확물·묶음·상자·과수·품목 표지처럼 같은 품목을 다르게 표현하는 variant를 포함한다. `Fruit`, `Veges` 두 개의 범용 표지는 품목별 177개에서 제외했다.

현재 `FoodPriceCrosswalkCatalog`와 연결 가능한 범위는 다음과 같다.

| 판정 | 품목군 수 | 의미 |
| --- | ---: | --- |
| 직접 연결 | 10 | asset 품목명과 현재 HS·KAMIS 가격 품목의 의미가 일치한다. 다만 실제 상품의 상태·품종에 따른 최종 HS 판정은 별도다. |
| 대표가격 연결 | 2 | Pumpkin과 Squash를 현재 `070993 호박` 대표가격으로 함께 볼 수 있지만 동일 상품으로 확정하지 않는다. |
| 후보·추가 판정 필요 | 17 | asset은 있으나 현재 가격 연결이 없거나, 품종·물리적 상태가 불명확하거나, 비슷한 가격 품목과 동일하다고 볼 수 없다. |

따라서 Farm Pack만으로도 감자·토마토·양파·오이·상추·브로콜리·딸기·사과·배·복숭아와 호박류의 생산·수확·출하 장면을 만들 수 있다. 반면 쌀·고구마·마늘·생강·참깨·땅콩·포도·감귤 등 현재 가격 조사표의 주요 품목은 전용 asset이 없어 별도의 시각 대체 원칙이나 추가 asset이 필요하다.

### 2.1 canonical 60품목 실제 catalog 반영

2026년 KAMIS 대조에서 기존 감자와 HS·AMS 후보가 확인된 59개를 합친 canonical 60품목을 Unity `FarmProductVisualCatalog`에 모두 등록했다. 서버에는 prefab 정보가 없으며 Unity에서만 `CanonicalProductStableId → farm.product.* VisualKey → POLYGON Farm prefab`을 해석한다.

| Unity 판정 | 수 | 품목 |
| --- | ---: | --- |
| `Direct` | 18 | 감자, 콩, 양배추, 상추, 수박, 오이, 토마토, 딸기, 당근, 양파, 브로콜리, 사과, 배, 복숭아, 바나나, 오렌지, 레몬, 체리 |
| `Representative` | 10 | 배추·얼갈이배추·알배기배추→Cabbage, 호박→Pumpkin, 풋고추·붉은고추→Chilli, 피망·파프리카→Pepper, 방울토마토→Tomato, 감귤→Orange |
| `Unmapped` | 32 | 쌀, 고구마, 시금치, 갓, 참외, 무, 열무, 피마늘, 파, 생강, 멜론, 깐마늘, 참깨, 땅콩, 버섯 3종, 호두, 아몬드, 포도, 단감, 참다래, 파인애플, 망고, 아보카도, 수산물 7종 |

`Direct`는 Farm Pack 파일명이 같은 품목군이라는 시각 판정이고 HS·가격 관계의 Confirmed를 뜻하지 않는다. `Representative`는 카드와 진단 화면에서 대표 표현임을 유지해야 하며 동일 품종·상품·재고로 합치지 않는다. `Unmapped` 항목은 catalog에 빠진 것이 아니라 prefab과 VisualKey를 비워 둔 명시적 상태다.

## 3. 바로 연결 가능한 품목

`직접`은 현재 저장소의 가격 연결표와 asset 품목명이 맞는다는 뜻이다. 관세 신고용 HS 확정이나 특정 prefab이 실제 상품 상태를 증명한다는 뜻은 아니다.

| Farm 품목군 | prefab 수 | 현재 HS prefix | 국내 가격 품목 | 연결 수준 | 표현 가능 단계 |
| --- | ---: | --- | --- | --- | --- |
| 감자 `Potato` | 8 | `0701` | 감자 | 직접 | 재배 S/M/L, 낱개·묶음, 상자, 표지 |
| 브로콜리 `Broccoli` | 4 | `070410` | 브로콜리 | 직접 | 수확물 크기 variant |
| 상추 `Lettuce` | 3 | `070511`, `070519` | 상추 | 직접 | 수확물 크기 variant |
| 오이 `Cucumber` | 7 | `070700` | 오이 | 직접 | 낱개·묶음, 상자, 표지 |
| 토마토 `Tomato` | 6 | `070200` | 토마토 | 직접 | 재배 S/M/L, 낱개·묶음 |
| 딸기 `Strawberry` | 7 | `081010` | 딸기 | 직접 | 재배 S/M/L, 낱개·묶음, 표지 |
| 양파 `Onion` | 6 | `070310` | 양파 | 직접 | 낱개·묶음, 상자 |
| 사과 `Apple` | 7 | `080810` | 사과 | 직접 | 어린 나무·성목, 낱개·묶음, 상자, 표지 |
| 배 `Pear` | 4 | `080830` | 배 | 직접 | 어린 나무·성목, 낱개·묶음 |
| 복숭아 `Peach` | 6 | `080930` | 복숭아 | 직접 | 어린 나무·성목, 낱개·묶음, 상자, 표지 |
| 호박 `Pumpkin` | 13 | `070993` | 호박 | 대표가격 | 일반·Italian·White와 크기 variant, 표지 |
| 스쿼시 `Squash` | 9 | `070993` | 호박 | 대표가격 | Butternut·Delicata와 크기 variant, 표지 |

`Pumpkin`과 `Squash`에는 현재 가격 연결표의 “애호박·쥬키니·단호박을 포함한 대표가격”을 표시할 수 있다. 다만 서로 다른 품종을 하나의 실물 상품이나 재고로 합치지 않고, 가격 카드에도 `대표가격`임을 드러내야 한다.

## 4. Asset은 있으나 추가 판정이 필요한 품목

| Farm 품목군 | prefab 수 | 저장소의 HS 후보 | 현재 가격 연결 | 보류 이유 |
| --- | ---: | --- | --- | --- |
| 살구 `Apricot` | 5 | 명시 profile 없음 | 없음 | 과수·수확물은 있으나 현재 후보·가격표가 없다. |
| 아스파라거스 `Asparagus` | 5 | 명시 profile 없음 | 없음 | 신선품 여부와 세부 분류 검토가 필요하다. |
| 바나나 `Banana` | 4 | `0803` | 없음 | 플랜틴 여부와 신선·건조 상태가 필요하다. |
| 콩 `Bean` | 5 | 확정 후보 없음 | `071333 흰콩`과 직접 연결 금지 | asset만으로 흰콩 여부와 신선·건조 상태를 알 수 없다. |
| 비트 `Beetroot` | 5 | 명시 profile 없음 | 없음 | 품종과 상품 상태를 먼저 정해야 한다. |
| 양배추 `Cabbage` | 3 | `0704` | `070490 배추`와 직접 연결 금지 | 일반 양배추 외형을 배추 가격으로 간주할 수 없다. |
| 당근 `Carrot` | 6 | `070610` | 없음 | HS 후보는 있으나 KAMIS 가격 연결표가 없다. |
| 체리 `Cherry` | 5 | 명시 profile 없음 | 없음 | 과수·수확물·표지는 있으나 현재 후보·가격표가 없다. |
| 고추 `Chilli` | 6 | `070960`, `090421`, `090422` | 없음 | 신선·건조·분쇄 상태에 따라 후보가 달라진다. |
| 옥수수 `Corn` | 7 | `1005` | 없음 | 종자·곡물·스위트콘 구분을 외형만으로 확정할 수 없다. |
| 가지 `Eggplant` | 6 | 명시 profile 없음 | 없음 | 재배·수확·상자는 있으나 현재 후보·가격표가 없다. |
| 레몬 `Lemon` | 4 | 명시 profile 없음 | 없음 | 과수·수확물은 있으나 현재 후보·가격표가 없다. |
| 오렌지 `Orange` | 5 | 명시 profile 없음 | `080521 감귤`과 직접 연결 금지 | Orange asset을 감귤 품목으로 대체하지 않는다. |
| 피망·파프리카 계열 `Pepper` | 6 | `070960`, `090421`, `090422` | 없음 | 품종과 신선·건조·분쇄 상태가 필요하다. |
| 자두 `Plum` | 6 | 명시 profile 없음 | 없음 | 과수·수확물·상자는 있으나 현재 후보·가격표가 없다. |
| 수박 `Watermelon` | 4 | 명시 profile 없음 | 없음 | 현재 후보·가격표가 없다. |
| 밀 `Wheat` | 15 | `1001` | 없음 | 종자용·듀럼밀 여부가 필요하며 재배체와 곡물 상품을 구분해야 한다. |

이 표의 HS 후보는 `OfficialFoodIngredientHsCandidateMatcher`가 검색 시작점을 제공하는 품목에만 적었다. `명시 profile 없음`은 해당 품목이 HS에 속하지 않는다는 뜻이 아니라, 현재 저장소의 식품명 후보표에서 근거를 찾지 못했다는 뜻이다. 임의의 HS6를 새로 단정하지 않는다.

## 5. 기존 가격 조사 품목 중 Farm Pack 대응 공백

### 5.1 식물성 품목

| 현재 가격 연결 품목 | Farm Pack 상태 | 처리 원칙 |
| --- | --- | --- |
| 쌀 `1006` | 전용 asset 없음 | Wheat나 범용 작물열로 쌀이라고 표시하지 않는다. |
| 흰콩 `071333` | `Bean`은 있으나 동일성 불명 | generic bean은 분위기용으로만 쓰고 상품 연결은 보류한다. |
| 고구마 `071420` | 전용 asset 없음 | Potato를 색만 바꾸어 고구마 stable ID에 연결하지 않는다. |
| 배추 `070490` | `Cabbage`는 있으나 동일성 불명 | generic cabbage를 배추 가격과 자동 연결하지 않는다. |
| 시금치 `070970` | 전용 asset 없음 | Lettuce로 대체하지 않는다. |
| 마늘 `070320` | 전용 asset 없음 | Onion으로 대체하지 않는다. |
| 생강 `091011` | 전용 asset 없음 | root vegetable 범용 외형으로 확정 표시하지 않는다. |
| 참깨 `120740` | 전용 asset 없음 | Wheat나 generic crop으로 상품을 확정하지 않는다. |
| 땅콩 `120241`, `120242` | 전용 asset 없음 | generic bean으로 대체하지 않는다. |
| 포도 `080610` | 전용 asset 없음 | 다른 과수나 묶음 fruit로 대체하지 않는다. |
| 감귤 `080521` | `Orange`는 있으나 동일성 불명 | Orange는 환경 표현 가능, 감귤 가격 연결은 보류한다. |
| 단감 `081070` | 전용 asset 없음 | 다른 과수로 대체하지 않는다. |
| 참다래 `081050` | 전용 asset 없음 | 다른 과수로 대체하지 않는다. |

논의 사각 필지·논길·농수로 자리를 먼저 구성하는 경우에도 [Unity Farm 시설하우스·밭·논 단지 Modular Composition 설계](UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md)의 `논 단지 Blockout` 경계를 사용한다. 벼·담수면·논둑·농수로 전용 Visual과 실제 ProductStableId가 준비되기 전에는 쌀 HS·가격 카드를 연결하지 않는다.

### 5.2 축산·수산 품목

Farm Pack에서는 `SM_Prop_Chicken_Coop_01`, `SM_Prop_Chicken_Coop_Cage_01` 두 닭장과 말 장애물 세 개만 이름으로 확인됐다. 닭·계란·소·돼지·우유·치즈 및 수산물의 상품 prefab은 확인되지 않았다. 그러므로 닭장은 농장 환경으로 사용할 수 있지만 닭고기 `0207`이나 계란 `0407` 가격·재고의 VisualRoot로 사용하지 않는다.

쇠고기·돼지고기·닭고기·계란과 고등어·오징어·굴·홍합·전복·가리비·꽃게 등 현재 가격 연결표의 축산·수산 품목은 POLYGON Farm만으로 상품을 직접 표현할 수 없다.

## 6. World 연결 원칙

식품 asset 연결은 다음 네 식별자를 하나로 합치지 않는다.

```text
FarmVisualKey       화면에 보이는 재배체·수확물·상자
ProductStableId     Simulation 또는 서버가 가리키는 품목
HsCodeCandidate     검토 중인 분류 후보
PriceObservation    출처·시각·단위·시장 단계가 있는 가격 관측
```

- 과수·밭 작물은 생산 공간 표현이고, 수확물·상자는 거래 가능한 상품 표현이다.
- 같은 `ProductStableId`가 재배체·낱개·상자에 투영될 수 있지만 각 prefab이 상태나 수량을 소유하지 않는다.
- asset 이름이 같아도 신선·냉장·냉동·건조·분말·종자·가공품의 HS를 자동 확정하지 않는다.
- 국내 가격은 현재 `FoodPriceCrosswalkCatalog`에 존재하는 연결만 사용한다. 비슷해 보이는 KAMIS 품목으로 자동 대체하지 않는다.
- 가격 카드에는 출처, 기준 시각, 단위, 통화, 시장 단계, 직접/대표 연결 여부를 함께 표시한다.
- `후보·추가 판정 필요` 품목은 World에 환경 object로 배치할 수 있지만 HS·가격 overlay는 숨기거나 `연결 검토 필요`로 표시한다.

배치된 재배체·수확물·상자 선택부터 상품·국내 가격·연결 근거·국가별 가격 Concept Card를 여는 상세 흐름은 [Unity Farm 상품·가격 카드 상호작용 흐름](UnityFarmProductPriceCardInteractionFlow.md)을 따른다.

## 7. 반복 배치 세트에 적용할 수 있는 범위

현재 구현된 농장 풍경 Composition Library에는 다음 방식으로 반영할 수 있다. 이 절은 후속 구현 지침이며 이번 조사에서 prefab이나 catalog를 변경하지 않는다.

1. `실제 데이터 세트`: 감자·토마토·양파·오이·상추·브로콜리·딸기·사과·배·복숭아 중심으로 구성한다.
2. `대표가격 세트`: 호박·스쿼시는 외형상 분리하되 가격 overlay에 대표가격임을 표시한다.
3. `환경 작물 세트`: 밀·옥수수·고추·당근·과수류 등 가격 연결이 없는 asset은 경관 밀도를 만들되 stable ID와 가격 카드를 부여하지 않는다.
4. `출하 세트`: 품목별 box prefab이 있는 감자·사과·살구·당근·오이·가지·양파·복숭아·자두를 Farm Yard나 Produce Stand에 사용한다. 직접 가격 연결이 없는 상자는 환경 화물과 실제 cargo를 구분한다.
5. `공백 품목 세트`: 쌀·고구마·마늘 등 전용 asset이 없는 품목은 잘못된 대체 prefab을 고르지 않고 generic placeholder 또는 추가 asset 검토 대상으로 남긴다.

## 8. 근거와 제한

- prefab 수는 2026-08-09에 실제 Unity project의 `.prefab` 파일명을 읽어 집계했다.
- vendor prefab 내용과 mesh가 표현하는 식물종을 외부 식물학 자료로 검증하지 않았다. 판정은 Synty가 부여한 파일명 기준이다.
- 현재 가격 연결은 `Ssalddel/Services/External/PublicData/FoodPriceCrosswalkCatalog.cs`의 코드 기준이다.
- HS 후보는 `Ssalddel/Services/FoodCulture/OfficialFoodIngredientHsMappingService.cs`의 검색 profile 기준이다.
- HS6는 공통 분류 후보이며 실제 HSK10은 품종·가공·상태·용도와 신고 시점의 공식 품목분류를 다시 확인해야 한다.
- 새로운 실시간 KAMIS·관세청 호출, 가격 갱신, Unity runtime·Game View 검증은 이번 문서 조사 범위에 포함하지 않았다.
