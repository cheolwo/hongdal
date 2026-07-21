# 각국 정부 공식 음식 레시피 아카이브

## 목적과 경계

각국 정부·공공기관이 제공하는 음식문화와 레시피를 출처·권리·수집 시각과 함께 DB에 보관해 커뮤니티 글의 검토 근거로 사용한다. 수집한 한 건을 곧바로 “그 나라의 대표 음식”으로 확정하지 않는다. 모든 음식은 `Candidate`·`PendingReview`로 시작하고 운영자가 지역성, 대표성, 번역, 원문 최신성과 이용조건을 확인해야 한다.

수집은 게시글, 가원장, 공동주문, 음식 주문, 결제 또는 배달 업무를 만들지 않는다. 이미지 파일은 어느 원천에서도 내려받아 저장하지 않는다.

## 구현 순서와 현재 상태

| 순서 | 원천 | 수집 방식 | DB 저장 범위 | 현재 정책 |
| --- | --- | --- | --- | --- |
| 1 | [식품의약품안전처 식품안전나라 COOKRCP01](https://www.data.go.kr/data/15060073/openapi.do) | JSON Open API | 음식명, 식재료, 최대 20단계 조리법, 저감 팁, 영양값과 단위, 원문 JSON | API key가 있을 때 자동 수집. 이미지 URL만 보관 |
| 2 | [농촌진흥청 농사로 향토 음식](https://www.data.go.kr/data/15101449/openapi.do) | XML Open API 목록·상세 | 지역, 음식 유형, 주·부재료, 조리법, 유래·참고사항, 원문 XML | API key가 있을 때 자동 수집. 권리 표기 충돌 때문에 내부 검토 전용 |
| 3 | [일본 농림수산성 Our Regional Cuisines](https://www.maff.go.jp/e/policies/market/k_ryouri/) | 공식 영문 HTML | 도도부현, 역사·유래, 분량, 재료, 단계, 제공자 설명과 원문 HTML | [MAFF 이용조건](https://www.maff.go.jp/e/use/term_use.html)에 맞춰 출처와 편집 사실 표시. 제3자 사진 비복제 |
| 4 | [NHS Healthier Families recipes](https://www.nhs.uk/healthier-families/recipes/) | 공식 HTML의 JSON-LD와 단계 목록 | 설명, 분량, 재료, 단계, 영양 표기와 원문 HTML | [NHS 이용조건](https://www.nhs.uk/our-policies/terms-and-conditions/)에 따라 복사일 표시. 7일 경과 사본은 검토 후보에서 자동 제외. 이미지 비사용 |
| 5 | [USDA MyPlate Kitchen](https://www.myplate.gov/myplate-kitchen/recipes), [Health Canada Food Guide](https://food-guide.canada.ca/en/recipes/), [프랑스 농업부](https://agriculture.gouv.fr/recettes) | 공식 링크 메타데이터 | 제공기관, 국가·언어, 공식 URL, 이용조건과 제한 | 연방·제3자 원문 또는 상업 재이용 조건이 섞여 있어 항목별 확인 전 원문 수집 차단 |

농사로의 공공데이터포털 상세는 이용허락범위를 `제한 없음`으로 표시하지만, 농사로 자체 Open API 목록은 같은 향토 음식 서비스를 `공공누리 유형3`으로 표시한다. 이 불일치를 숨기지 않고 source policy에 같이 저장하며, 해결 전에는 외부 자동 게시나 이미지 재사용을 허용하지 않는다.

## 저장 모델

```text
OfficialFoodRecipeSource
  ├─ 출처·국가·언어·접근 방식
  ├─ 문서·약관 URL, 라이선스, 텍스트/이미지 정책
  └─ 권리 확인일, 갱신 주기, 자동화 상태

OfficialFoodDish
  ├─ 국가+정규화 음식명 기반 안정 DishKey
  ├─ 지역·분류·요약
  └─ Candidate / PendingReview

OfficialFoodRecipeVariant
  ├─ 출처+원천 ID 기반 안정 RecordKey
  ├─ 원문 재료 JSON, 단계·영양·태그 JSON
  ├─ 원문 URL·원문 payload·checksum
  ├─ 수집 당시 라이선스·귀속 문구·이미지 정책 snapshot
  └─ 재료 parser 버전·색인 시각·구조화 재료 수

OfficialFoodIngredientCategory
  └─ 곡류·채소·수산물·장류 등 18개 분류 코드와 설명

OfficialFoodIngredient
  ├─ 언어+정규화 이름 기반 안정 IngredientKey
  ├─ 표준 표시명·정규화명·분류
  └─ 자동 분류 방식·신뢰도·확정/검토 대기 상태

OfficialFoodIngredientPriceMapping
  ├─ 표준 재료와 국가별 공공가격 품목의 명시적 연결
  ├─ 국가·원천·외부 부류/품목 코드와 매칭 방식·신뢰도
  └─ 자동 매칭/운영자 확정 상태, 근거와 원천 URL

OfficialFoodRecipeIngredient
  ├─ 레시피 변형과 표준 재료의 N:M 사용 관계
  ├─ 주재료·양념장·고명 등 원문 묶음과 표시 순서
  ├─ 원문 항목·원천 재료명·수량·표준 단위·가정 계량
  └─ parser 버전·파싱 신뢰도·검토 필요 여부

OfficialFoodRecipeCollectionRun
  └─ 시작·완료·실패, 조회 범위, 신규·갱신·기존 건수
```

동일 출처의 동일 원천 ID를 다시 수집하면 변형을 중복 생성하지 않고 checksum을 비교한다. 이름이 같은 음식은 같은 국가 안에서 대표 음식 후보를 공유할 수 있지만, 자동 병합 결과도 검토 전 상태를 유지한다.

### 재료 전산화 원칙

- `IngredientsJson`은 원문 증거로 계속 보관하고 구조화 행으로 대체하지 않는다.
- 쉼표와 줄바꿈을 괄호 밖에서만 분리하고, `주재료`, `양념장`, `고명`, `육수` 같은 묶음을 각 사용 행에 남긴다.
- `75g(3/4모)`는 수치 `75`, 표준 단위 `g`, 가정 계량 `3/4모`로 나누며 `약간`과 무수량 재료도 버리지 않는다.
- 언어와 정규화 재료명이 같으면 여러 레시피가 하나의 재료 마스터를 공유한다. 조리 상태나 상품 차이를 성급히 합치지 않도록 `다진 마늘`, `저염간장` 같은 원천 이름은 보존한다.
- 규칙으로 분류하지 못한 재료는 `other`와 `PendingReview`로 명시한다. 낮은 신뢰도를 숨기거나 임의의 식품군으로 확정하지 않는다.
- parser 버전을 레시피 변형과 사용 행에 기록해 규칙이 바뀌면 명시적으로 재색인할 수 있게 한다.
- 재료 조회는 실제 사용 행에서 대표 레시피를 최대 3개까지 `RelatedRecipes`로 반환한다. 승인·대표 상태, freshness와 최근 수집을 우선하되 먼저 서로 다른 음식을 고르고, 음식이 부족할 때만 같은 음식의 다른 실제 레시피 변형으로 채운다.
- 관계가 3개 미만인 재료는 유사 재료나 같은 분류의 레시피를 추정해 채우지 않는다. 원천에서 제거된 레시피와 제외된 음식은 대표 관계에 포함하지 않는다.

### 재료 공공가격 연결 원칙

- 한국은 [KAMIS 가격정보 Open API](https://www.kamis.or.kr/customer/reference/openapi_list.do)에 보관된 전국 도매·소매 관측값을 사용한다. API가 제공하는 식량작물·채소·특용작물·과일·축산·수산 부류와 품목 코드를 유지한다.
- 미국은 [USDA NASS Quick Stats](https://quickstats.nass.usda.gov/api)의 `SURVEY / PRICE RECEIVED / NATIONAL` 관측값을 사용한다. 농작물뿐 아니라 축산물과 NASS 조사 대상 양식 수산물도 수집하되, 생산자가 받은 가격이지 소매가격이 아님을 표시한다.
- `PendingReview`, `other`, 가공식품·장류·조미료 또는 정확한 공공 품목을 확인할 수 없는 재료에는 매핑 행을 만들지 않는다. 분류만으로 유사 품목 가격을 추정하지 않는다.
- 한국은 최신 조사일의 품종·등급 표본을 도매와 소매로 나눠 평균·최소·최대·표본 수를 제공한다. 기존 검토 교차표에 국산 품종 코드가 지정된 품목은 그 코드만 포함한다. 미국은 원문 품목·단위·기준월과 통계를 그대로 제공한다.
- KRW/1kg 유통 조사가격과 USD 원문 단위의 미국 생산자 수취가격은 직접 비교하거나 자동 환산하지 않는다. 각 가격에는 국가, 시장 단계, 기준일, 통화, 단위, 지역, 갱신 시각, 매칭 품질과 주의문을 함께 반환한다.
- 가격은 레시피 수량을 곱한 구매비용이나 판매·주문 견적이 아니다. 원천 관측이 없거나 최신 통합 계열을 고를 수 없으면 매핑이 있더라도 가격을 표시하지 않는다.

## 공개 조회와 관리자 API

| Method | Path | 용도 |
| --- | --- | --- |
| `GET` | `/api/v1/agricultural-fisheries/food-ingredients` | 표준 재료, 출처가 확인된 한국·미국 공공가격, 실제 사용 관계의 대표 레시피 최대 3개 조회 |

공개 조회는 이미 DB에 보관·색인된 자료만 읽는다. 외부 공공 API를 즉시 호출하거나 재료 분류·가격 매핑·레시피 관계를 확정하지 않으며, 정확한 매핑이 없으면 가격을 비워 둔다.

### 관리자 API

| Method | Path | 용도 |
| --- | --- | --- |
| `GET` | `/api/v1/admin/content/official-food-recipes/sources` | 7개 원천의 권리·갱신·자동화 정책 조회 |
| `GET` | `/api/v1/admin/content/official-food-recipes/dishes` | 원천·국가·지역·검토 상태·검색어별 음식 후보 조회 |
| `GET` | `/api/v1/admin/content/official-food-recipes/dishes/{dishKey}/variants` | 음식 후보의 출처별 레시피, 귀속 문구와 freshness 조회 |
| `GET` | `/api/v1/admin/content/official-food-recipes/ingredients/categories` | 18개 재료 분류와 분류별 마스터 수 조회 |
| `GET` | `/api/v1/admin/content/official-food-recipes/ingredients` | 분류·언어·검토 상태·검색어별 표준 재료와 실제 대표 레시피 최대 3개 조회 |
| `POST` | `/api/v1/admin/content/official-food-recipes/ingredients/index` | 기존 원문 재료를 parser 버전에 맞춰 배치 색인 |
| `POST` | `/api/v1/admin/content/official-food-recipes/ingredients/prices/index` | 명확한 재료만 KAMIS·USDA 품목에 매핑하고 실제 가격 가용 건수 집계 |
| `POST` | `/api/v1/admin/content/official-food-recipes/collections` | 허용된 원천을 페이지·항목 상한 안에서 명시적으로 수집 |

관리자 표의 API는 모두 `서버관리자전용`이다. 관리자 `ingredients` 응답과 공개 재료 조회 응답에는 연결 가능한 `PublicPrices`와 실제 사용 관계의 `RelatedRecipes`가 포함되고, 레시피 `variants` 응답의 구조화 재료에는 `PublicPrices`가 포함된다. 미국·캐나다·프랑스 메타데이터 전용 원천에 수집 요청을 보내면 서버가 거부한다.

## 설정과 순서별 실행

키는 Git에 추적되는 설정에 넣지 않는다. `appsettings.Local.json`, user secrets 또는 환경 변수의 `PublicData:MfdsCookRecipe:ApiKey`, `PublicData:RdaLocalFood:ApiKey`를 사용한다.

```powershell
dotnet run --project Ssalddel -- --collect-mfds-recipes --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --collect-rda-local-food --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --collect-maff-regional-cuisines --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --collect-nhs-recipes --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --index-official-food-recipe-ingredients --source=mfds-cookrcp01 --max-items=5000
dotnet run --project Ssalddel -- --collect-kamis-prices --target-date=2026-07-20
dotnet run --project Ssalddel -- --collect-usda-nass-prices --year-from=2025
dotnet run --project Ssalddel -- --index-official-food-ingredient-prices --max-items=5000 --force
```

`--max-pages`와 `--max-items`는 최초 소량 검증을 위한 안전 상한이다. 전체 수집 전에는 원천 트래픽 정책, 응답 필드 변화, DB 용량과 이용조건을 다시 확인한다. 운영 scheduler는 아직 켜지 않았으므로 자동 외부 호출은 발생하지 않는다.

재료 색인은 현재 parser 버전이 없는 레시피만 기본 처리한다. 규칙을 바꾸고 전체를 다시 계산할 때만 `--force`를 붙이며, 운영자가 `Confirmed`로 확정한 재료 분류는 자동 규칙이 덮어쓰지 않는다.

공공가격 매핑도 같은 재료에 중복 행을 만들지 않는다. 자동 매핑 규칙을 바꿨을 때만 `--force`로 다시 계산하며, 운영자가 `Confirmed`로 확정한 매핑은 자동 규칙이 덮어쓰거나 비활성화하지 않는다. 매핑 색인은 외부 API를 호출하지 않고 먼저 보관된 KAMIS·USDA 관측값을 읽는다.

## 커뮤니티 검토 후보

DB에 보관된 네 원천은 기존 `CommunityInformationCandidateDto`로 읽을 수 있다. 이 단계에서는 제목, 짧은 설명, 국가·언어, 기준일, 수집일, 원문 링크, 귀속 문구와 제한만 제공하고 이미지 URL은 후보 thumbnail로 내보내지 않는다. 상세 재료와 조리법은 관리자 전용 variant API에서 확인한다.

NHS 자료는 마지막 수집 후 7일이 지나면 DB에서 삭제하지 않되 공통 검토 후보에서 제외한다. 다시 수집해 원문과 checksum을 확인한 뒤에만 freshness가 회복된다.
