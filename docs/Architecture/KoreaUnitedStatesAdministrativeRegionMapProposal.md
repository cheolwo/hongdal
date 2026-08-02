# 한국·미국 행정구역 기반 농수산물 지도 제안

## 문서 상태와 범위

- 상태: 1차 공통 화면 구현, 실제 GIS renderer·경계 원장 후속
- 조사 기준일: 2026-08-01
- 첫 적용 국가: 대한민국 `KR`, 미국 `US`
- 대상 화면: 커뮤니티 시작 세계지도와 지역 문화·농수산물 가격 탐색
- 범위 밖: 주문, 계약, 생산자 추천, 배차, 정확한 농장 위치 공개

이 문서는 한국과 미국의 공식 행정구역 코드와 경계 자료를 농수산물 산지·가격 자료에 연결하고, 지도 마커를 선택해 지역 문화와 가격 근거를 살펴보게 하는 방법을 제안한다.

2026-08-02 기준 1차 기능 slice는 공통 marker API·contract를 재사용해 Web·MAUI 공유 화면에 `KR`/`US` 국가 선택, 산지 원천·출하/선적·시장 관측 레이어, canonical route와 SVG fallback을 연결했다. 커뮤니티 시작 세계지도는 별도 slice에서 런타임 키 주입형 Google Maps renderer를 연결했지만, 이 지역 상세 화면은 아직 실제 GIS renderer, polygon 경계, zoom·clustering과 공식 내부점 원장을 구현하지 않았다. 아래의 행정구역 코드·개인정보·원천 연결 기준을 후속 구현 경계로 유지한다.

## 제안 요약

1. 세계지도에서 국가를 고르는 현재 흐름을 유지하고, 확대 수준에 따라 시·도/주와 시·군·구/county 마커를 단계적으로 표시한다.
2. 행정구역 자체의 식별자와 MAFRA 산지코드, USDA AMS의 원문 지역을 같은 코드로 덮어쓰지 않는다. 공식 코드 간 교차표를 별도 원장으로 둔다.
3. 지도에는 정확한 농장이나 출하자 주소가 아니라 공식 행정구역의 대표 내부점만 사용한다.
4. 마커는 `산지·출하지`, `거래·가격 관측지`, `추정 후보`를 모양과 문구로 구분한다.
5. 마커를 선택하면 하단 시트 또는 우측 패널에서 `지역 이야기`, `농수산물`, `가격`, `유통 관계`, `근거`를 사용자가 골라 본다.
6. 지역 경계 polygon은 기본 화면을 채우는 주 표현이 아니라 선택한 마커의 관할 범위를 이해시키는 보조 강조로 사용한다.

## 현재 저장소와의 연결점

현재 커뮤니티 시작 화면은 개략 세계지도 위에 한국·미국·중국·호주 국가 hotspot을 두고 국가별 문화·가격 화면으로 연결한다. 이 SVG는 국가 단위 탐색과 지도 실패 시 fallback으로는 적합하지만, 실제 위경도에 따라 여러 지역 마커를 배치하고 확대·군집화하기에는 부족하다.

현재 활용 가능한 자산은 다음과 같다.

- 지역 문화 이미지 원장은 `CountryCode`와 `SubdivisionCode`를 보관하며 한국 `KR-11`, 미국 `US-CA`처럼 ISO 3166-2 형태의 광역 코드를 이미 사용한다.
- 국내 경락 원장은 `OriginCode`, `OriginName`과 도매시장 코드를 분리해 보관한다.
- USDA AMS 가격 원장은 `Origin`, `District`와 `MarketLocation`을 분리해 보관한다.
- 국내 지역 가격 비교 조회는 원산지와 도매시장 소재지를 서로 다른 비교 기준으로 집계한다.
- 미국 주소·배달권 설계는 Census State, County, Place, ZCTA를 `GEOID`와 데이터 vintage로 구분한다.

따라서 새 지도는 원천 수집을 다시 만드는 작업보다, 기존 자료를 공식 지역 식별자에 연결하는 읽기 전용 지리 투영을 먼저 만드는 것이 적절하다.

## 공식 코드와 경계 자료 조사

### 공통 광역 코드

[ISO 3166-2](https://www.iso.org/standard/72483.html)는 국가의 주요 subdivision을 표현하는 국제 코드다. 현재 지역 문화 원장의 `KR-11`, `US-CA`와 같은 값은 국가 간 화면·URL·검색의 광역 별칭으로 유지할 수 있다. 그러나 ISO 코드는 한국 시·군·구나 미국 county를 식별하는 세부 행정구역 원장이 아니므로 각 국가의 공식 코드를 함께 보관해야 한다.

### 대한민국

| 구분 | 공식 원천과 코드 | 지도에서의 역할 | 주의점 |
| --- | --- | --- | --- |
| 법정 행정구역 | [행정표준코드관리시스템 법정동 코드](https://www.code.go.kr/stdcode/regCodeL.do), 10자리와 생성·폐지 이력 | 현재·과거 시·도, 시·군·구, 읍·면·동의 기준 행정구역 식별 | 행정동과 법정동은 목적과 경계가 다르다. 코드 변경 이력을 보존해야 한다. |
| 통계 경계 | [SGIS 행정구역경계 API](https://sgis.mods.go.kr/developer/html/newOpenApi/api/dataApi/addressBoundary.html), `adm_cd` 2/5/8자리와 기준연도 | 시·도, 시·군·구, 행정동 GeoJSON 경계와 대표 좌표 | 액세스 토큰과 기준연도가 필요하고 좌표 변환·경계 vintage를 기록해야 한다. |
| 농수축산물 산지 | [MAFRA 농축수산물 산지 코드](https://data.mafra.go.kr/opendata/data/indexOpenDataDetail.do?data_id=20240626000000002475), `CODEID`/`CODENAME` | 경락 자료 `SANCD`/`SANNAME`의 원천 코드 해석 | 행정안전부 법정동 코드가 아니다. 매우 상세한 주소가 포함될 수 있다. |
| 산지코드 구성 | [aT 농수축산물 표준코드 소개](https://at.agromarket.kr/codeInfo/introduce.do), 국내 6자리·수입 800·특정 해양 801 | 국내산, 수입산, 특정 해양 산지의 원천 의미 판정 | 국내 코드는 종전 우편번호 체계를 기반으로 하므로 최신 행정코드와 직접 prefix 결합하지 않는다. |
| 경락 관측 | [전국 공영도매시장 경매원천 정산가격](https://data.mafra.go.kr/opendata/data/indexOpenDataDetail.do?data_id=20240625000000002462) | 산지에서 어느 도매시장·법인으로 들어와 어떤 조건으로 거래됐는지 집계 | 산지와 도매시장 소재지는 서로 다른 지리 관계다. 출하자·생산자 식별정보는 지도에 사용하지 않는다. |

#### 한국에 적용할 판단

- 공개 마커의 기본 세분도는 `시·도 → 시·군·구`로 제한한다. 읍·면·동과 더 세부적인 산지코드는 사용자가 확대하더라도 기본 공개하지 않는다.
- MAFRA `SANCD`는 원문 증거로 보존하고, 공식 산지코드 API에서 `CODENAME`을 확인한 뒤 관측일 당시 유효한 법정 행정구역에 연결한다.
- `CODENAME`이 아파트·리·세부 주소까지 포함하면 공개 투영에서는 시·군·구까지만 남긴다. 정확한 좌표를 지오코딩하지 않는다.
- 경계는 SGIS 자료를 화면용으로 단순화하되, `BoundarySource`, `BoundaryYear`, 원 좌표계와 WGS84 변환 정보를 함께 기록한다.
- SGIS 행정동 코드와 법정동 코드를 동일 코드라고 간주하지 않고 명시적인 교차표로 연결한다.
- MAFRA OpenAPI의 원 요청 주소가 HTTP이므로 브라우저가 직접 호출하지 않는다. 운영 수집은 HTTPS 중계 또는 검토된 공식 다운로드 경로가 마련된 경우에만 켠다.

### 미국

| 구분 | 공식 원천과 코드 | 지도에서의 역할 | 주의점 |
| --- | --- | --- | --- |
| 광역 표시 | ISO 3166-2 `US-CA`, USPS 2자리 주 약어 | 주 이름 표시, AMS 주 약어 별칭 | USPS 약어는 경계 식별자의 유일한 기준으로 사용하지 않는다. |
| 공식 지리 식별자 | [Census ANSI/FIPS 코드](https://www.census.gov/library/reference/code-lists/ansi.2020.html)와 [GEOID 구조](https://www.census.gov/programs-surveys/geography/guidance/geo-identifiers.html) | State 2자리, County 5자리, Place 7자리 등 지역 원장 key | State, County, Place는 서로 다른 지리 유형이다. Place가 여러 county에 걸칠 수 있다. |
| 상세 경계 | [Census TIGER/Line GeoPackage](https://www.census.gov/geographies/mapping-files/time-series/geo/tiger-geopackage-file.html) | 서버 전처리, 정확한 코드·내부점·경계 연결 | 파일이 크므로 사용자 요청 때마다 내려받지 않는다. vintage를 고정한다. |
| 화면용 경계 | [Census Cartographic Boundary Files](https://www.census.gov/geographies/mapping-files/2011/geo/carto-boundary-file.html) | 단순화한 주·county 경계와 확대 표시 | 연도와 해상도별 형상이 다르므로 가격 관측 시점과 화면 기준연도를 표시한다. |
| 필요 시 조회 | [Census TIGERweb State/County 서비스](https://tigerweb.geo.census.gov/arcgis/rest/services/TIGERweb/State_County/MapServer) | 조사·보정용 공식 온라인 지리 조회 | 공개 화면이 외부 서비스 장애에 직접 의존하지 않도록 서버 snapshot을 우선한다. |
| AMS 출하·시장 | [USDA AMS Shipping Point와 Terminal 설명](https://mymarketnews.ams.usda.gov/node/8571) | 출하·선적 district와 terminal 거래 위치를 다른 관계로 표시 | Shipping Point는 생산지 또는 최초 선적항일 수 있고, district는 반드시 Census county가 아니다. |

#### 미국에 적용할 판단

- 주 마커는 ISO/USPS 별칭을 Census State GEOID에 연결한다.
- county 마커는 `STATEFP + COUNTYFP` 5자리 GEOID가 확인된 경우에만 만든다.
- AMS `MarketLocationState`는 주 약어 별칭을 통해 주 GEOID로 안전하게 연결할 수 있지만, 이는 거래·관측 위치다.
- AMS `Origin`과 `District`는 원문을 보존한다. Shipping Point 보고서의 district는 여러 shipper가 포함된 생산·선적 지역일 수 있으므로 이름이 유사하다는 이유만으로 Census county에 강제 연결하지 않는다.
- district가 여러 county를 포함하거나 항만·통관 지점을 뜻하면 주 수준 또는 별도 `ShippingDistrict` 지리로 표시하고, `행정구역 아님`을 명시한다.
- 주·county marker anchor는 TIGER/Line의 공식 내부점이 있으면 이를 우선하고, 단순 polygon 중심점이 바다나 다른 행정구역에 놓이지 않도록 `point-on-surface`를 사용한다.

## 공통 지역 원장 제안

### 행정구역과 외부 코드를 분리한다

`RegionId`는 내부의 변경되지 않는 식별자로 두고, 외부 코드는 기간과 원천을 가진 별도 assignment로 보관한다.

```text
AdministrativeRegion
  RegionId
  CountryCode
  RegionTypeCode
  ParentRegionId?
  DisplayNameKo / DisplayNameEn / DisplayNameLocal
  ValidFrom / ValidTo

RegionCodeAssignment
  RegionId
  SchemeCode              // ISO-3166-2, KR-MOIS-BJD, KR-SGIS-HADM,
                          // KR-MAFRA-ORIGIN, US-CENSUS-GEOID, US-USPS
  ExternalCode
  SourceVintage
  ValidFrom / ValidTo
  SourceUrl / VerifiedAtUtc

RegionBoundary
  RegionId
  BoundarySourceCode
  BoundaryVintage
  GeometryReference
  AnchorLatitude / AnchorLongitude
  SimplificationLevel

RegionCrosswalk
  SourceSchemeCode / SourceCode / SourceNameRaw
  TargetRegionId?
  MatchMethodCode
  ConfidenceCode
  ValidFrom / ValidTo
  ReviewedAtUtc / EvidenceUrl
```

이 구조는 `KR-11`, MAFRA `SANCD`, SGIS `adm_cd`를 한 열에 섞지 않으며 미국 USPS 약어, FIPS와 GEOID도 각각의 의미를 보존한다.

### 가격 관측과 지역의 관계를 명시한다

하나의 가격 관측은 산지와 시장을 동시에 가질 수 있으므로 단일 `RegionCode` 열로 축약하지 않는다.

```text
PriceObservationRegionLink
  ObservationRecordKey
  RegionId?
  RelationTypeCode
    ConfirmedOrigin
    ShippingPointOrPortOfEntry
    MarketObservation
    ReportingOffice
    InferredOriginCandidate
    Unresolved
  SourceLocationCodeRaw / SourceLocationNameRaw
  ResolutionMethodCode
  ConfidenceCode
  Explanation
```

공개 화면 문구는 다음 기준을 사용한다.

| 관계 | 화면 문구 | 마커 표현 |
| --- | --- | --- |
| MAFRA 산지코드가 공식 시·군·구로 연결됨 | `산지 원천에 명시` | 초록 원형 + 잎 아이콘 |
| AMS Shipping Point 또는 port of entry | `출하·선적 지역` | 청록 육각형 + 상자 아이콘 |
| 도매시장·Terminal·Retail 위치 | `거래·가격 관측 지역` | 파랑 사각형 + 시장 아이콘 |
| 품목·계절·인접 시장으로만 추정 | `산지 추정 후보` | 주황 테두리 원 + 물음표 |
| 연결 근거 부족 | 지도에 표시하지 않고 `지역 미확인` 목록에 집계 | 마커 없음 |

색상만으로 의미를 전달하지 않고 모양, 아이콘, 텍스트와 보조 설명을 함께 사용한다.

## 지도 표시와 선택 흐름

### 확대 단계

| 지도 상태 | 표시 단위 | 동작 |
| --- | --- | --- |
| 세계 전체 | 국가별 자료 수와 대표 marker cluster | 국가 선택 시 해당 국가 bounds로 이동 |
| 국가 보기 | 한국 시·도, 미국 주 | marker 수와 문화·가격 자료 존재 여부 표시 |
| 지역 확대 | 한국 시·군·구, 미국 county 또는 확인된 shipping district | 화면 영역과 필터에 해당하는 marker만 조회 |
| marker 선택 | 선택 지역 경계 강조와 상세 panel | 지역 이야기·가격·유통 관계·근거 중 사용자가 선택 |

마커 수가 많은 경우 같은 지역과 관계 유형을 서버에서 먼저 집계하고, 가까운 마커는 화면에서 cluster로 합친다. cluster를 선택하면 확대하거나 포함 지역 목록을 먼저 보여 준다.

### 마커 선택 패널

모바일은 하단 시트, 넓은 화면은 지도 오른쪽 패널을 사용한다.

```text
[경상남도 진주시]  산지 원천에 명시
경락 142건 · 품목 8개 · 최근 관측 2026-07-31

  지역 이야기 | 농수산물 | 가격 | 유통 관계 | 근거

  농수산물
  파프리카 · 호박 · 딸기 ...

  가격
  품목 / 품종 / 등급 / 거래단위 / 기간 / 시장단계 선택

  유통 관계
  진주시 산지 → 서울가락 도매시장

  근거
  MAFRA SANCD 660000 · 정산일 · 마지막 수집시각
```

- `지역 이야기`는 지역 문화 원장의 질문·특산물·근거 경계를 보여 준다.
- `농수산물`은 해당 지역에 연결된 품목을 보여 주되 지역 대표성을 확정하는 표현을 피한다.
- `가격`은 원 거래단위, 통화, 등급, 품종, 시장단계와 기준일을 유지한다.
- `유통 관계`는 확인된 산지와 시장 사이의 집계 경로만 보여 주고 개별 출하자나 생산자 이름은 표시하지 않는다.
- `근거`는 원천명, 원 코드, 정규화 방법, 신뢰도, 관측일, 수집시각과 원문 링크를 제공한다.

### 사용자 선택 상태

지도 선택은 조회 조건일 뿐 가입·알림·상대 선택·주문을 만들지 않는다. URL에는 개인정보 없는 상태만 둔다.

```text
/community/home?country=KR&region=...&layer=origin&product=...
```

정확한 외부 코드가 URL에 노출될 필요가 없으면 내부 공개용 `RegionKey`를 사용하고, 폐지된 지역 key는 replacement 안내와 함께 읽기 전용으로 해석한다.

## 지도 기술 제안

### 화면과 데이터 계약을 분리한다

지도 component는 특정 지도 사업자의 region code를 직접 저장하지 않는다. 서버는 WGS84 marker와 단순화한 선택 경계, source attribution을 반환한다. Web과 MAUI Blazor는 같은 contract를 사용한다.

```text
GET /api/v1/community/regional-map/markers
  ?countryCode=KR
  &bounds=...
  &zoom=...
  &relationType=ConfirmedOrigin|MarketObservation
  &productKey=...

GET /api/v1/community/regional-map/regions/{publicRegionKey}
```

marker 요약 응답에는 다음을 포함한다.

- 공개 지역 key, 국가, 지역 유형과 표시명
- 위도·경도와 좌표 근거
- marker 관계 유형과 신뢰도
- 품목 수, 관측 수와 최근 기준일
- 사용할 수 있는 상세 section 목록
- 경계 기준연도, 가격 출처와 마지막 수집시각

### 렌더러 선택

현재 SVG는 접근 가능한 국가 선택 fallback으로 유지한다. 실제 지역 marker는 확대·군집화·경계 강조가 가능한 동적 renderer adapter로 분리한다.

- 단기 검증: 현재 SVG 아래에 지역 목록을 제공하고, 한 국가 pilot에서 실제 위경도 marker component를 검증한다.
- 장기 권장: MapLibre 계열 또는 동등한 vendor-neutral renderer에 서버가 만든 marker와 단순화 경계를 제공한다.
- 배경 지도 tile과 글꼴·지명·attribution의 라이선스, 한국과 미국에서의 표시 품질, 오프라인·장애 fallback을 검토한 뒤 provider를 확정한다.
- 공식 경계 파일은 basemap 자체가 아니다. 화면용 경계 overlay와 배경 지도를 별도 자산으로 관리한다.

## 개인정보와 표현 경계

- MAFRA 원천의 출하자번호·출하자명·생산자명, 세부 주소는 공개 지도 원장에 복제하지 않는다.
- marker는 공식 행정구역 대표 내부점이며 실제 농장·창고·개인의 위치가 아니다.
- 시·군·구보다 작은 단위는 공개 필요성과 재식별 위험을 별도 검토하기 전 기본 비활성이다.
- AMS Shipping Point가 항만이나 수입품의 최초 선적 지점일 수 있음을 표시하고 생산지로 단정하지 않는다.
- 문화권, 역사권, 행정구역을 같은 polygon으로 취급하지 않는다. 문화권은 별도 설명 범위이고 상품 원산지는 현재 공식 지역 근거를 사용한다.
- 근거가 충돌하거나 관측일에 유효한 지역을 정하지 못하면 `Unresolved`로 남기고 임의의 marker를 만들지 않는다.

## 단계별 구현 제안

### 1단계: 지역 코드 원장과 snapshot

- 한국 법정동 변경 이력, SGIS 시·도/시·군·구 경계, MAFRA 산지코드 snapshot을 분리 수집한다.
- 미국 State/County/Place 코드와 화면용 경계의 한 vintage를 고정한다.
- 외부 key, 출처, 유효기간, 폐지·대체 관계를 검증한다.
- 아직 화면은 바꾸지 않는다.

### 2단계: 산지·시장 교차표

- 최근 국내 경락 자료의 고유 `SANCD`를 MAFRA 산지코드에서 확인하고 시·군·구까지 정규화한다.
- AMS 보고서를 시장 유형별로 나누고 주 단위부터 안전하게 연결한다.
- 자동 연결, 이름 기반 후보, 사람 검토가 필요한 항목을 분리한다.
- 해결률뿐 아니라 잘못 연결될 가능성과 미해결 수를 함께 보고한다.

### 3단계: 읽기 전용 marker 투영과 API

- 지역·품목·기간·시장단계별 집계 marker를 만든다.
- 원천 재수집 뒤 같은 관측이 중복 marker를 만들지 않도록 `ObservationRecordKey + RegionId + RelationTypeCode`를 멱등 key로 사용한다.
- 지도 요청은 원천 API를 실시간 호출하지 않고 저장된 snapshot과 투영을 읽는다.

### 4단계: 한국·미국 pilot 화면

- 한국은 시·도에서 시·군·구 산지·도매시장 marker로 확대한다.
- 미국은 주에서 안전하게 확인된 county 또는 shipping district로 확대한다.
- marker 선택 패널, 키보드 조작, screen reader 설명과 지도 밖 목록 보기를 함께 제공한다.
- 지도 실패 시 현재 국가 hotspot과 지역 카드 탐색을 유지한다.

### 5단계: 문화와 유통 관계 연결

- 기존 지역 문화 원장을 같은 `RegionId`에 연결한다.
- 확인된 산지→시장 관계를 집계 선으로 선택적으로 표시한다. 기본 화면에는 모든 선을 동시에 그리지 않는다.
- 사용자가 선택한 한 품목·기간·시장단계에 대해서만 관계를 보여 주고 구매·계약 기능과 분리한다.

## 구현 시작 전 검증 조건

- 한국 MAFRA 산지코드, 법정동 코드와 SGIS 경계 코드가 서로 다른 체계임을 test fixture로 고정한다.
- MAFRA 세부 산지명이 시·군·구보다 정밀하더라도 공개 marker가 더 상세해지지 않는지 확인한다.
- 미국 State, County, Place와 Shipping District를 같은 계층으로 강제하지 않는지 확인한다.
- 동일한 지역에 산지와 시장 관측이 함께 있을 때 두 관계가 별도 marker 의미로 유지되는지 확인한다.
- 경계와 marker에 source vintage, 관측일, 마지막 수집시각과 신뢰도가 남는지 확인한다.
- 원 거래단위와 시장단계를 유지하고 KAMIS·MAFRA 경락·AMS 자료를 하나의 평균가격으로 합치지 않는지 확인한다.
- 키보드와 지도 밖 목록만으로도 국가→지역→상세 정보 탐색이 가능한지 확인한다.
- 실제 DB 표본 검증 전에는 자동 교차표 해결률이나 전국 표시 가능 범위를 확정 수치로 발표하지 않는다.

## 결정이 더 필요한 항목

1. Web·MAUI 공통 지도 renderer와 basemap tile 라이선스
2. SGIS 경계 snapshot 사용 조건, 인증키 운영과 좌표 변환 방식
3. MAFRA HTTP 원천을 위한 HTTPS 중계 또는 공식 파일 수집 경로
4. 한국 행정구역 개편 시 ISO 별칭, 법정동, SGIS와 산지코드의 갱신 순서
5. AMS Shipping District를 주 marker로 축약할지 별도 비행정구역으로 등록할지에 대한 보고서별 검토 기준
6. 시·군·구/county marker의 최소 관측 수와 소지역 공개 기준

이 여섯 항목을 정한 뒤에는 `지역 코드 원장 → 교차표 → marker API → 화면` 순서로 좁은 세로 slice를 구현한다.
