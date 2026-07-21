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
  ├─ 재료·단계·영양·태그 JSON
  ├─ 원문 URL·원문 payload·checksum
  └─ 수집 당시 라이선스·귀속 문구·이미지 정책 snapshot

OfficialFoodRecipeCollectionRun
  └─ 시작·완료·실패, 조회 범위, 신규·갱신·기존 건수
```

동일 출처의 동일 원천 ID를 다시 수집하면 변형을 중복 생성하지 않고 checksum을 비교한다. 이름이 같은 음식은 같은 국가 안에서 대표 음식 후보를 공유할 수 있지만, 자동 병합 결과도 검토 전 상태를 유지한다.

## 관리자 API

| Method | Path | 용도 |
| --- | --- | --- |
| `GET` | `/api/v1/admin/content/official-food-recipes/sources` | 7개 원천의 권리·갱신·자동화 정책 조회 |
| `GET` | `/api/v1/admin/content/official-food-recipes/dishes` | 원천·국가·지역·검토 상태·검색어별 음식 후보 조회 |
| `GET` | `/api/v1/admin/content/official-food-recipes/dishes/{dishKey}/variants` | 음식 후보의 출처별 레시피, 귀속 문구와 freshness 조회 |
| `POST` | `/api/v1/admin/content/official-food-recipes/collections` | 허용된 원천을 페이지·항목 상한 안에서 명시적으로 수집 |

모든 API는 `서버관리자전용`이다. 미국·캐나다·프랑스 메타데이터 전용 원천에 수집 요청을 보내면 서버가 거부한다.

## 설정과 순서별 실행

키는 Git에 추적되는 설정에 넣지 않는다. `appsettings.Local.json`, user secrets 또는 환경 변수의 `PublicData:MfdsCookRecipe:ApiKey`, `PublicData:RdaLocalFood:ApiKey`를 사용한다.

```powershell
dotnet run --project Ssalddel -- --collect-mfds-recipes --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --collect-rda-local-food --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --collect-maff-regional-cuisines --max-pages=1 --max-items=100
dotnet run --project Ssalddel -- --collect-nhs-recipes --max-pages=1 --max-items=100
```

`--max-pages`와 `--max-items`는 최초 소량 검증을 위한 안전 상한이다. 전체 수집 전에는 원천 트래픽 정책, 응답 필드 변화, DB 용량과 이용조건을 다시 확인한다. 운영 scheduler는 아직 켜지 않았으므로 자동 외부 호출은 발생하지 않는다.

## 커뮤니티 검토 후보

DB에 보관된 네 원천은 기존 `CommunityInformationCandidateDto`로 읽을 수 있다. 이 단계에서는 제목, 짧은 설명, 국가·언어, 기준일, 수집일, 원문 링크, 귀속 문구와 제한만 제공하고 이미지 URL은 후보 thumbnail로 내보내지 않는다. 상세 재료와 조리법은 관리자 전용 variant API에서 확인한다.

NHS 자료는 마지막 수집 후 7일이 지나면 DB에서 삭제하지 않되 공통 검토 후보에서 제외한다. 다시 수집해 원문과 checksum을 확인한 뒤에만 freshness가 회복된다.
