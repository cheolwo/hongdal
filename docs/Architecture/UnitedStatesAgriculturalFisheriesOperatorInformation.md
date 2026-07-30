# 미국 농어업경영체 정보 공개 원천

## 결론

2026년 7월 29일 기준 미국에는 한국의 농어업경영체 등록정보처럼 농장·양식장·어업인·수산업체를 한 번에 조회하는 전국 단일 공개 명부가 없다.

- USDA NASS 농업총조사와 양식총조사는 지역·품목별 집계만 공개하고 개별 농장·양식장 자료는 공개하지 않는다.
- USDA 프로그램 참여 과정에서 생산자와 토지 소유자가 제출한 운영·보전·토지 지리정보는 `7 U.S.C. 8791`의 제한을 받는다.
- 농업에서는 유기농 인증 운영체, 자발적으로 등재한 Local Food 사업자, FSIS 검사시설처럼 프로그램 목적별 공개 명부가 있다.
- 수산업에서는 NOAA 검사 프로그램 참여시설, FDA 인증 패류 취급업체, 일부 NOAA 지역의 선박·딜러·운영자 허가 요약이 공개된다.
- 상업 어획·양륙의 개별 사업자 자료는 기여자를 식별하지 못하도록 집계하거나 비공개 처리된다.
- 주별 농업·양식·어업 허가는 별도이므로 연방 자료만으로 전국 사업 자격을 판정할 수 없다.

따라서 살뜰은 이를 `미국 농어업경영체 명부` 하나로 합치지 않는다. 공개 목적과 법적 범위를 보존한 `공식 정보 원천 카탈로그`로 관리하고, 사업자 참여는 본인의 동의와 최신 공식 상태 확인을 거쳐 별도로 연결한다.

## 공식 원천 판정

| 원천 | 공개 수준 | 살뜰 활용 | 중요한 한계 |
|---|---|---|---|
| [USDA NASS Census of Agriculture](https://www.nass.usda.gov/AgCensus/) | 전국·주·카운티 집계 | 지역 수요·공급 맥락 | 개별 농장·운영자 비공개 |
| [USDA NASS Census of Aquaculture](https://www.nass.usda.gov/Surveys/Guide_to_NASS_Surveys/Census_of_Aquaculture/) | 전국·주 집계 | 양식 품목·생산 규모 맥락 | 개별 양식장 비공개 |
| [USDA FSA Section 1619 안내](https://www.fsa.usda.gov/Internet/FSA_Notice/app_70.pdf) | 개별 행정기록 공개 제한 | 수집 금지 경계 확인 | 제한된 법정 예외 외 운영·토지 정보 공개 금지 |
| [Organic INTEGRITY](https://www.ams.usda.gov/services/organic-certification/certifiers-inspectors) | 인증 운영체 공개 검색 | 유기농 생산·취급 후보 및 인증 상태 대조 | 일반 영업·수출입 허가가 아님 |
| [USDA Local Food Directories](https://www.ams.usda.gov/services/local-regional/food-directories-listings) | 자발적 사업자 등재 | Farmers Market·CSA·Food Hub·On-Farm Market 탐색 | 완전성·최신성을 보증하지 않음 |
| [USDA FSIS MPI Directory](https://www.fsis.usda.gov/inspection/establishments/meat-poultry-and-egg-product-inspection-directory) | 검사 대상 시설 공개 | 육류·가금·난제품 시설번호와 활동 범주 대조 | 농장·목장 전체 명부가 아님 |
| [NOAA USDC Approved Establishments](https://www.fisheries.noaa.gov/resource/document/us-department-commerce-approved-establishments) | 자발적 검사 프로그램 승인시설 공개 | 수산물 가공·검사 참여시설 확인 | 미등재가 위생 실패를 뜻하지 않음 |
| [FDA ICSSL](https://www.fda.gov/food/federal-state-local-tribal-and-territorial-cooperative-human-food-programs/interstate-certified-shellfish-shippers-list) | 인증 패류 취급업체 공개 | 패류 공급·가공 후보 및 인증번호 확인 | 패류에 한정된 프로그램 명부 |
| [NOAA Greater Atlantic Permit Lookup](https://www.fisheries.noaa.gov/data-tools/public-permit-lookup) | 지역별 허가 요약 공개 | 사용자가 제시한 허가번호의 보조 대조 | 미국 전국 자료가 아니며 최종 적격성 보증이 아님 |
| [NOAA Commercial Landing Data Caveats](https://www.fisheries.noaa.gov/commercial-landing-data-caveats) | 비식별 집계 | 어종·지역별 시장 규모 | 개별 업체·선박을 식별할 수 있는 자료는 비공개 |

이 문서는 공식 원천의 공개 범위와 전산 설계 경계를 기록한 것이며 개별 거래나 허가에 대한 법률의견은 아니다.

## 전산화된 조회 계약

공개 API는 다음 경로다.

```http
GET /api/v1/agricultural-fisheries/us-operator-information-sources
    ?sectorCode=Aquaculture
    &recordTypeCode=CertifiedOperationDirectory
    &publicAccessCode=PublicBusinessDirectory
    &integrationStatusCode=OfficialLookupOnly
    &page=1
    &pageSize=50
```

전산화된 USDA AMS Local Food Directory 사업체 후보는 다음 경로에서
조회한다.

```http
GET /api/v1/agricultural-fisheries/us-operator-profiles
    ?directoryTypeCode=FoodHub
    &stateCode=AR
    &productKey=seafood
    &currentOnly=true
    &page=1
    &pageSize=30
```

관리자 수집 API와 서버 직접 실행 명령은 다음과 같다.

```http
POST /api/v1/admin/content/us-operator-profiles/collections
Content-Type: application/json

{
  "directoryTypes": ["FoodHub", "Csa"]
}
```

```powershell
dotnet run --project Ssalddel -- --collect-usda-ams-public-businesses
```

각 원천은 다음 분류를 유지한다.

- `SectorCode`: 농업, 양식, 자연산 어업, 로컬푸드 유통, 육류·가금·난제품 가공, 수산물 가공·운송
- `RecordTypeCode`: 집계 통계, 인증 운영체, 자발적 사업자, 검사시설, 인증 화주, 허가 요약, 비공개 행정기록
- `PublicAccessCode`: 공개 집계, 공개 사업자 명부, 공개 지역 허가 요약, 제한된 개별 기록
- `AccessModeCode`: API, CSV, 동적 검색, PDF, 웹 디렉터리, 접근 제한
- `IntegrationStatusCode`: 기존 집계 API 연동, 일괄 연동 후보, 공식 조회 전용, 메타데이터만 확보, 수집 금지

응답은 `HasUnifiedPublicOperatorRegistry=false`, `DiscoveryOnly=true`를 고정하고 각 원천에 다음 운영 경계를 함께 제공한다.

- 업체 발견 가능 여부와 프로그램 상태 확인 가능 여부
- 거래 수행 권한을 확인할 수 없다는 표시
- 자동 초대와 자동 업무 배정 금지
- 잠재적 개인정보 포함 여부
- 허용 용도, 금지 용도, 원천별 한계와 검토일

## 개인정보와 참여 연결

공개 웹페이지에 정보가 있다는 사실만으로 살뜰이 개인 이름, 자택 주소, 전화·이메일, 농장 좌표, 선박 위치를 복제해도 된다는 뜻은 아니다.

1. 집계 통계는 원천의 비공개 표식과 단위를 유지한다.
2. 업체 명부는 사업체명·공식 식별번호·도시·주·프로그램 상태처럼 목적에 필요한 최소 필드부터 연동한다.
3. 자연인 이름과 연락처는 공식 업무 연락처인지 확인되지 않으면 저장하지 않는다.
4. 공개 후보를 플랫폼 사용자로 자동 생성하거나 초대하지 않는다.
5. 사업자가 직접 참여 의사를 표시하면 계정 소유 확인과 동의를 거쳐 공식 원천 식별자에 연결한다.
6. 원장 역할 슬롯 배정 전에는 최신 공식 원천, 품목·지역별 허가, 계약 범위를 다시 확인한다.

## 실제 데이터 연동 순서

1. USDA Local Food Directory의 공식 bulk download를 정기 수집하고
   source listing ID 기준으로 멱등 갱신한다. 완료.
2. 사업체 단위 최소 필드와 원천 스냅샷 시각을 저장하는 읽기 전용
   투영을 유지한다. 완료.
3. FSIS CSV의 현재 스키마·이용조건·갱신 식별자를 검증한다.
4. Organic INTEGRITY의 현재 공개 API 계약을 다시 확인한 뒤 인증 상태 조회 어댑터를 분리한다.
5. FDA ICSSL 동적 목록과 NOAA 승인시설 문서는 HTML·PDF 변경 감지와 수동 검토 큐를 먼저 둔다.
6. NOAA Greater Atlantic 허가 CSV는 자연인·주소 필드를 기본 제외하고 지역별 검증 어댑터로 제한한다.
7. 주별 농업·양식·어업 허가는 한 주와 한 업무 흐름을 선택해 별도 원천으로 확장한다.

현재 USDA AMS Local Food Directory의 Agritourism, CSA, Farmers Market,
Food Hub, On-Farm Market을 공식 bulk 원천에서 수집한다. 2026년 7월
29일 확인한 원천 건수는 각각 13,569건, 2,002건, 7,148건, 480건,
4,692건이며 원천이 갱신되면 수치는 달라진다.

저장 필드는 official listing ID, 사업체명, 도시·주, 설립연도,
법인 성격 표기, 취급품목, 소매·도매·생산자 지원·조달 기능,
원천 갱신시각과 마지막 관측시각이다. 상세 주소, 좌표, 담당자명,
전화, 이메일과 원본 JSON은 저장하지 않는다. 목록에서 사라진 항목은
삭제하지 않고 `IsCurrentlyListed=false`로 남겨 수집 이력을 보존한다.

AMS My Market News 가격 행의 시장명, 보고 사무소, 원산지와 이 사업체
후보를 자동 연결하지 않는다. 동일 지역·품목이라는 사실은 해당 업체가
그 가격을 제시했다는 증거가 아니므로, 가격은 지역 의사결정 맥락으로만
별도 조회한다. 다른 9개 공식 원천은 여전히 메타데이터 또는 공식 조회
단계이며 운영 실패를 샘플 업체로 숨기지 않는다.
