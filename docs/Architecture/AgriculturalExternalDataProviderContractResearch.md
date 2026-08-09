# 농업 토지·토양 외부 데이터 공급자 계약 조사

## 1. 조사 목적과 단계

이 문서는 실제 credential이나 운영 수집을 먼저 연결하지 않고 공급자의 데이터 의미와 Ssalddel 정규화 계약을 고정하는 P6-A 결과다.

```text
P6-A Provider contract research
  → official dataset and access metadata
  → dimensions / fields / units / missing values
  → spatial and temporal precision
  → license / attribution / limitations
  → provider-independent Ssalddel contracts and fixtures

P6-B Live ingestion
  → credential only when required
  → live response verification
  → private raw storage
  → parser / normalizer
  → DB lineage verification
```

P6-A의 `ApiAvailable`은 documented access capability를 뜻할 뿐 운영 사용 가능성과 uptime을 보장하지 않는다. Source Catalog 등록과 collection 활성화도 별개다.

## 2. 조사 결과 요약

| Source | Dataset | 인증 | 공간·시간 | 접근 | License | P6 상태 |
| --- | --- | --- | --- | --- | --- | --- |
| World Bank WDI | `AG.LND.ARBL.HA` Arable land (hectares) | 없음 | 국가·연간 | Indicators API v2 JSON | 지표 페이지 CC BY 4.0, 원자료 FAO | registration·collector·normalizer fixture 구현, live 안정성 미확정 |
| FAOSTAT | Land Use `RL` | 문서·통계 접근은 인증 없음 | 국가/지역/세계·연간 | Web와 bulk download 후보 | CC BY 4.0 + FAO database additional terms | metadata registration·공통 국가 토지 계약, parser 미구현 |
| ISRIC SoilGrids | SoilGrids 250m properties | WCS 인증 없음, WebDAV anonymous | 250m raster·6개 깊이·rolling model release | WCS 2.0, WebDAV, WMS | CC BY 4.0 | metadata registration·property/coverage 계약, coverage 수집 미구현 |

## 3. World Bank WDI 경지면적

공식 Indicators API v2는 JSON/XML/JSON-stat과 paging·기간·최근값 질의를 지원하고 API key가 필요하지 않다.

선택 지표:

- indicator: `AG.LND.ARBL.HA`
- label: Arable land (hectares)
- publisher/original source: FAO
- normalized metric: `agricultural-land.arable-area`
- normalized unit: `ha`
- spatial precision: `country`
- temporal precision: `annual`
- 국가 응답 code: `countryiso3code`; Ssalddel Region mapping을 별도로 사용
- null observation: normalized fact를 만들지 않고 rejected count로 기록

공식 지표 화면은 2023년을 최신 표시연도로 안내한다. 전 연도 조회는 2026-08-08 확인 과정에서 server error와 timeout이 섞였으므로 P6-B에서는 `mrv=1`로 국가별 최신 비결측 관측 한 건만 요청한다. 2026-08-09 live verification은 HTTP 200 JSON, `sourceid=2`, `lastupdated=2026-07-13`, 2023년 대한민국 `1,456,000 ha`를 확인했다.

P6-B adapter 기본 범위는 `KOR` 하나다. `USA`, `CHN` mapping은 P7 준비로 저장하되 실제 country query 확대는 세 국가의 같은 지표·연도·단위가 확인된 뒤 수행한다.

공식 근거:

- <https://datahelpdesk.worldbank.org/knowledgebase/articles/889392>
- <https://datahelpdesk.worldbank.org/knowledgebase/articles/898581-api-basic-call-structures>
- <https://data.worldbank.org/indicator/AG.LND.ARBL.HA>
- <https://datacatalog.worldbank.org/public-licenses>

## 4. FAOSTAT Land Use

FAO의 2025년 release 설명에 따르면 Land Use domain은 44개 land use·irrigation·agricultural practice category와 5개 indicator를 국가·연도별로 제공하며 global coverage와 annual update를 가진다. 발표 주기는 연 1회이고 2025 release는 6월 20일, 다음 계획은 2026년 6월로 안내됐다.

P6-A에서 고정하는 의미:

- source: `fao-faostat`
- dataset: `land-use-rl`
- spatial precision: country, region 또는 global aggregate
- temporal precision: annual
- provider dimensions 후보: area, item/category, element/measure, year, unit, flag, note
- provider area/item/element code는 Ssalddel stable ID와 metric으로 직접 사용하지 않고 mapping한다.
- `arable land`, `cropland`, `agricultural land`는 같은 용어가 아니므로 item definition을 보존한다.
- land use 통계는 토지 필지 geometry나 토양 상태가 아니다.

정확한 bulk URL, CSV header, code list version, flag/null 표기와 압축파일 hash는 P6-B의 최소 sample download에서 확정한다. 확인 전에는 예상 field를 parser contract로 고정하지 않는다.

FAO database data는 기본 CC BY 4.0과 추가 terms를 적용한다. attribution에는 database/dataset, last update year, access date, URL과 license를 포함하고 FAO endorsement를 암시하지 않는다.

공식 근거:

- <https://www.fao.org/faostat/en/#data/RL>
- <https://www.fao.org/statistics/events/events-detail/land-use.-june-2025-update/en>
- <https://www.fao.org/contact-us/terms/db-terms-of-use/en>
- <https://www.fao.org/statistics/methods-and-standards/natural-resources/en>

## 5. ISRIC SoilGrids

SoilGrids는 세계 토양 속성을 250m grid에서 예측한 raster다. 현장 측정값이나 필지 판정이 아니며 모델 불확실성을 함께 다뤄야 한다.

공간 계약:

- spatial resolution: 250m raster
- CRS: WCS DescribeCoverage 실제 metadata에서 `EPSG:152160`
- depth intervals: `0-5`, `5-15`, `15-30`, `30-60`, `60-100`, `100-200cm`
- statistics: `Q0.05`, `Q0.5`, `Q0.95`, `mean`, `uncertainty`
- coverage ID: `{property}_{depth}cm_{statistic}`
- WCS `GetCapabilities`, `DescribeCoverage`, bounded `GetCoverage`
- WebDAV global VRT/GeoTIFF는 anonymous access지만 property 하나 전체가 매우 크므로 기본 수집 대상으로 삼지 않는다.

2026-08-09 `phh2o` WCS metadata를 credential 없이 확인한 결과 HTTP 200, 30 coverage가 반환됐다. 이는 6개 깊이 × 5개 statistic 조합이며 `phh2o_0-5cm_Q0.5`가 포함됐다. `DescribeCoverage`는 `EPSG:152160`을 반환했다.

현재 공식 문서는 SoilGrids REST API가 문제로 일시 중단되었고 복구 일정을 제시하지 않는다고 명시한다. 따라서 REST adapter는 만들지 않고 WCS metadata·bounded coverage 또는 WebDAV를 후보로 둔다.

정규화 property 계약:

| Code | 의미 | Source mapped unit | divisor | Conventional unit |
| --- | --- | --- | --- | --- |
| `bdod` | bulk density | `cg/cm3` | 100 | `kg/dm3` |
| `cec` | CEC at pH7 | `mmol(c)/kg` | 10 | `cmol(c)/kg` |
| `cfvo` | coarse fragments | `cm3/dm3` | 10 | `vol%` |
| `clay`, `sand`, `silt` | texture fraction | `g/kg` | 10 | `%` |
| `nitrogen` | total nitrogen | `cg/kg` | 100 | `g/kg` |
| `ocd` | organic carbon density | `hg/m3` | 10 | `kg/m3` |
| `ocs` | organic carbon stock | `t/ha` | 10 | `kg/m2` |
| `soc` | soil organic carbon | `dg/kg` | 10 | `g/kg` |
| `phh2o` | pH in water | `pH x 10` | 10 | `pH` |
| `wv0010` | water content at 10kPa | mapped integer unit | 10 | `%` |

Source mapped value·unit·conversion divisor와 conventional value·unit를 모두 보존한다. 그래야 원자료 재현과 표시 단위 변환을 분리할 수 있다.

공식 근거:

- <https://docs.isric.org/globaldata/soilgrids/>
- <https://docs.isric.org/globaldata/soilgrids/SoilGrids_faqs_01.html>
- <https://docs.isric.org/globaldata/soilgrids/SoilGrids_faqs_02.html>
- <https://docs.isric.org/globaldata/soilgrids/wcs.html>
- <https://docs.isric.org/globaldata/soilgrids/WebDav.html>

## 6. Ssalddel 정규화 계약

### 국가 농업 토지

`국가농업토지Data`는 국가 stable ID, metric, value/unit, reference year, annual precision, source/dataset/version, data revision, quality와 limitation을 가진다.

World Bank와 FAOSTAT 값은 source·item definition·revision이 다르므로 같은 국가·연도·단위라는 이유만으로 덮어쓰지 않는다. 같은 의미인지 확인된 뒤 Interpretation에서 비교한다.

### 지역 토양

`지역토양Data`는 region row만 가정하지 않고 bounded area·grid·raster coverage reference를 지원한다. 다음을 반드시 유지한다.

- spatial reference와 precision
- CRS와 grid resolution
- coverage ID
- property metric
- depth start/end cm
- statistic/quantile
- source mapped value/unit
- conversion divisor와 normalized value/unit
- model revision, quality, limitation

전 세계 SoilGrids cell을 일반 DB row로 변환하지 않는다. Raw GeoTIFF/VRT는 object/spatial storage에 두고 DB에는 coverage metadata와 요청·집계 결과만 저장한다.

## 7. P6-A 완료와 P6-B 게이트

P6-A 완료:

- World Bank·FAOSTAT·SoilGrids source metadata 등록
- 국가 토지와 깊이별 토양 provider-independent 계약 작성
- SoilGrids property/unit/depth/statistic catalog와 coverage ID parser 작성
- 시간 정밀도와 source mapped unit conversion 보존
- World Bank KOR fixture collector/normalizer와 ISO3 mapping 테스트

P6-B 시작 조건:

1. source 하나와 bounded collection scope를 명시한다.
2. 실제 endpoint 또는 bulk file version을 다시 확인한다.
3. 필요한 경우에만 server secret reference를 설정한다.
4. live 응답 field/null/error/rate-limit을 fixture와 비교한다.
5. private raw object와 DB lineage를 실제로 확인한다.
6. 성공·실패를 simulation으로 대체하지 않는다.

World Bank P6-B local verification은 명시적 환경변수로만 실행된다. 실제 `mrv=1` 응답을 production 수집 Runtime에 통과시켜 임시 private local object storage에 raw JSON을 저장하고, 테스트용 SQLite에서 `Run → RawSnapshot → NormalizedRecord` 계보와 SHA-256·source/data revision을 검증했다. 운영 DB migration 적용, 운영 object storage, scheduler/admin 실행 진입점은 아직 연결하지 않았다. FAOSTAT와 SoilGrids는 P6-A metadata/contract만 구현했고 collector는 아직 없다.

재현 명령:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File eng/verify-worldbank-agricultural-data.ps1
```
