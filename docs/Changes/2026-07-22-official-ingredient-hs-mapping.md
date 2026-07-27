# 공식 음식 재료 HS 후보 연결

## 목적

식품의약품안전처 등 공식 음식 데이터에서 전산화한 재료를 기존 HS 카탈로그와 연결해 같이수입·수출 검토의 시작점을 제공합니다.

이 연결은 신고용 세번을 자동 확정하지 않습니다. 같은 재료도 품종, 생물종, 신선·냉장·냉동·건조·분말 상태, 성분 함량, 가공공정, 포장과 용도에 따라 품목분류가 달라질 수 있습니다.

## 국가별 코드 경계

- 국제 HS는 6자리까지 공통 기반입니다.
- 한국은 HS를 10자리 HSK로 세분하며 한국 수출신고 후보로 취급합니다.
- 미국 수입은 미국 HTS 10자리 후보를 별도로 확인합니다.
- 한국 HSK와 미국 HTS는 6자리 이후가 같다고 가정하지 않습니다.
- 실제 신고 전에는 관세율표의 주·호·소호, 품목분류 결정사례와 관세사 또는 관세당국의 사전심사를 확인합니다.

공식 참고:

- 관세청 관세법령정보포털: <https://unipass.customs.go.kr/clip/index.do>
- 관세청 품목분류 사전심사: <https://www.customs.go.kr/ftaportalkor/cm/cntnts/cntntsView.do?cntntsId=8441&mi=15087>
- WCO HS 협약: <https://www.wcoomd.org/en/topics/nomenclature/instrument-and-tools/hs_convention.aspx>
- USITC HTS: <https://hts.usitc.gov/>
- 미국 CBP Binding Ruling: <https://www.help.cbp.gov/s/article/Article-1106?language=en_US>

## 저장 모델

`food_official_ingredient_hs_mappings`는 농수산 전용 DB에 다음 스냅샷을 저장합니다.

- 재료 ID와 외부 HS 카탈로그·항목 ID
- 국가, 표준, 개정판, 코드 자릿수와 효력일
- HS 코드, 품명, 설명과 공식 출처
- 매칭 방식, 품질, 신뢰도와 근거
- `Candidate`, `Confirmed`, `Rejected`, `Superseded` 검토 상태
- 코드 확정 전에 필요한 상품 상세정보
- 전문 검토 필요 여부와 마지막 대조 시각

주 DB의 HS 카탈로그와 농수산 DB 사이에는 교차 DB 외래 키를 만들지 않습니다. 대신 카탈로그 버전과 출처를 스냅샷으로 보존합니다.

자동 생성 결과는 항상 `Candidate`이며 `IsDeclarationReady=false`입니다. 사람이 거절한 후보는 강제 재생성에서도 자동 복구하지 않습니다.

## 후보 생성

후보 생성기는 다음 순서로 보수적으로 대조합니다.

1. 활성 HS 카탈로그와 활성 항목만 읽습니다.
2. 식품 업무 분류 또는 식품 관련 류 01~24와 식용 소금 2501 범위만 검색합니다.
3. 재료명과 카탈로그 품명의 정확 일치 후보를 찾습니다.
4. 주요 재료는 검토용 HS 군 접두어로 후보를 좁힙니다.
5. 나머지는 품명·설명·검색어의 문자열 포함 후보만 제시합니다.
6. 카탈로그 버전별 후보 수를 제한하고 모든 결과를 검토 대상으로 저장합니다.

## 관세청 HSK 카탈로그 적재

관세청 관세법령정보포털(CLIP)의 해당 연도 관세율표에서 식품 관련 제01~24류와 식용 소금 제2501호를 읽어 주 DB의 `hs_code_catalog_versions`, `hs_code_entries`에 저장합니다.

```powershell
dotnet run --project Ssalddel -- --import-kcs-hsk-catalog --year=2026 --chapters=01-24,25 --request-delay-ms=150 --force
```

- `--chapters`는 `01-03,10,25`와 같은 범위를 지원하며 제25류에서는 제2501호만 수집합니다.
- 원문 수집이 모두 성공하고 10자리 HSK가 확인된 뒤 한 번의 원자적 저장으로 활성 버전을 교체합니다.
- 같은 연도·범위가 이미 활성화된 경우 `--force`가 없으면 재수집하지 않습니다.
- 부분 장 갱신은 요청한 장에만 적용되며 다른 활성 장을 비활성화하지 않습니다.
- 표시명 컬럼 한도를 넘는 원문은 표시명만 500자로 축약하고 결합 설명에는 최대 4,000자까지 보존합니다.
- 원문: <https://unipass.customs.go.kr/clip/hsinfosrch/openULS0201002Q.do?cntyCd=KR>

2026-07-22 개발 DB 검증 결과는 활성 카탈로그 1개, 전체 항목 2,727개입니다. 장 항목 25개를 제외한 후보 검색 대상은 2,702개입니다.

## 실행

카탈로그 적재 후 전체 활성 카탈로그를 기준으로 최대 5,000개 재료를 연결합니다.

```powershell
dotnet run --project Ssalddel -- --index-official-food-ingredient-hs-codes --max-items=5000 --force
```

2026-07-22 개발 DB에서 한국 HSK만 대조한 결과는 재료 1,903개 중 700개 재료, 후보 3,159개가 연결되었습니다. 나머지 1,203개는 상품 상태·가공도 등 추가 정보 또는 재료별 검토 규칙이 필요합니다.

한국과 미국 카탈로그만 대조합니다.

```powershell
dotnet run --project Ssalddel -- --index-official-food-ingredient-hs-codes --countries=KR,US --max-items=5000 --force
```

## 공개 조회

```http
GET /api/v1/agricultural-fisheries/food-ingredients/hs-codes?ingredientKey={ingredientKey}&ingredientName={ingredientName}
```

선택 입력:

- `countryCode`: 특정 국가 카탈로그만 조회
- `refresh=true`: 현재 활성 카탈로그와 다시 대조

음식 재료 탐색 화면에서는 사용자가 `HS 후보 확인`을 눌렀을 때만 조회합니다. 후보 카드에는 국가별 사용 목적, 카탈로그 개정판, 매칭 근거, 필요한 상품 정보와 공식 원문을 함께 표시합니다.
