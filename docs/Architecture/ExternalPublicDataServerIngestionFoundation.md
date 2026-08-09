# 외부·공공 데이터 서버 수집 기반

## 1. 목적

Ssalddel의 토양·농업 토지·인구·가격·기후·교통 데이터는 Unity가 공급자에게 직접 요청하지 않는다. 서버가 공급자별 인증과 wire format을 격리하고 원자료, 출처, 기준시각, 단위, 공간 정밀도와 revision을 보존한 뒤 Ssalddel 공통 데이터로 정규화한다.

```text
External source
  → server source catalog / credential boundary
  → opt-in ingestion runtime
  → private raw object + DB metadata
  → normalized public data
  → Ssalddel API projection
  → Unity Data
  → Shared World Interpretation
  → Perspective Interpretation
  → Presentation
```

이 문서는 특정 토양 API 구현서가 아니라 공급자가 추가되어도 같은 수집·추적·정규화 경계를 재사용하기 위한 서버 기반의 기준 문서다.

## 2. P0 조사 결과

### 재사용

| 기존 기반 | 사용 방식 |
| --- | --- |
| `PublicDataApiMetadataCatalog` | 기존 API 목록을 새 Source Catalog의 초기 등록 정보로 변환한다. 별도 중복 카탈로그를 만들지 않는다. |
| `IConfiguration`과 server User Secrets | 자격증명 reference를 해석한다. secret 값은 계약·DB·로그에 저장하지 않는다. |
| `IObjectStorageService` private storage | 원자료 본문을 저장한다. DB에는 hash와 object 위치 metadata만 저장한다. |
| KAMIS·USDA·농사로 archive 패턴 | source/as-of/revision, 명시적 실패와 sample fallback 금지 원칙을 유지한다. |
| 별도 EF DbContext·migration history 패턴 | 대량 수집 원장을 주 업무 DbContext에서 분리한다. |
| Unity Data Context와 revision/reconcile | 추후 서버 projection의 `DataRevision`을 Interpretation·Presentation revision으로 연결한다. |

### 확장

- 기존 API 전용 metadata를 API·다운로드 파일·수동 import·object-storage drop을 표현하는 `ExternalDataSourceDefinition`으로 확장한다.
- 기존 server configuration을 source별 credential reference와 opt-in collection policy로 확장한다.
- 기존 private object storage에 external raw object naming과 hash 계산을 추가한다.
- 기존 EF 저장 패턴에 수집 Run, raw metadata, normalized record와 외부 지역 code mapping을 추가한다.

### 신규

- `IExternalDataCredentialProvider`
- `IExternalDataCollector`, `IExternalDataNormalizer`, `IExternalDataRawStorage`
- `IExternalDataIngestionRuntime`
- `I외부데이터수집Store`, `I외부지역MappingStore`
- `RegionStableIdRules`, normalized record key와 dedicated `PublicDataIngestionDbContext`

## 3. 구현 범위와 현재 상태

| 단계 | 현재 상태 | 구현 내용 |
| --- | --- | --- |
| P1 Source Catalog | 구현 | 기존 API catalog adapter, 파일·수동 등록 확장점, credential·중복 검증 |
| P2 Credential | 구현 | configuration/User Secrets provider, secret redaction, source별 기본 비활성 policy |
| P3 Ingestion Runtime | 구현 | timeout, caller cancellation, 제한된 retry, 오류 code, 성공·부분·실패·취소 Run |
| P4 Raw와 Run | 구현 | private object storage, SHA-256, raw metadata, 수집 Run, 동일 hash 재정규화 방지, EF migration |
| P5 Normalization | 구현 | region/metric/unit/as-of/precision/quality/limitation/source version/data revision, lineage validation |
| P6-A 공급자 계약 조사 | 구현 | World Bank·FAOSTAT·SoilGrids 공식 metadata, 국가 토지·깊이별 토양 계약과 fixture 검증 |
| P6-B 실제 농업 공급자 1개 | 로컬 검증 완료 | World Bank KOR `mrv=1` 실응답을 private local raw storage와 테스트용 SQLite Run→Raw→Normalized 계보까지 검증; 운영 DB·scheduler는 미연결 |
| P7 국가 3개 비교 | 미착수 | P6 계약과 지표가 실제로 비교 가능한지 확인한 뒤 확장한다. |
| P8 이후 Server API·Unity | 미착수 | provider DTO를 노출하지 않는 projection부터 별도 vertical slice로 진행한다. |

모든 source는 명시 설정 전까지 비활성이다. World Bank P6-B 검증도 일반 test나 server startup에서 호출하지 않고 `SSALDDEL_RUN_WORLD_BANK_LIVE=1`을 설정하는 전용 검증 명령에서만 실행한다.

## 4. Source Catalog

Source는 최소한 다음 경계를 명시한다.

- `SourceId`, `DatasetId`, 공식 기관과 URL
- API·파일·수동 import 등 접근 방식
- credential 종류와 server configuration reference
- 형식, 공간·시간 해상도, 갱신 주기
- license, 재배포 가능 여부, attribution과 이용 제한
- 마지막 확인일과 collection 기본값

기존 `PublicDataApiMetadataCatalog` 항목은 자동으로 source 정의로 변환한다. 파일이나 수동 import 공급자는 `IExternalDataSourceRegistration` 구현으로 추가한다. 같은 `SourceId + DatasetId` 중복과 잘못된 credential 정의는 startup에서 거부한다.

Source catalog 등록은 실제 수집 허용을 뜻하지 않는다. `ExternalData:Sources:{SourceId}:Enabled=true`가 명시되어야 Runtime이 실행된다.

## 5. Secret 경계

`ConfigurationExternalDataCredentialProvider`는 등록된 configuration reference를 순서대로 확인한다. 운영 secret은 환경변수나 server User Secrets에 저장하고 값은 Unity, API contract, DB, source, log와 exception message로 전달하지 않는다.

지원 계약은 `None`, `ApiKeyHeader`, `ApiKeyQuery`, `BearerToken`, `OAuth`다. OAuth token 교환과 갱신 adapter는 아직 구현하지 않았으며 P6 공급자가 요구할 때 별도 server adapter로 구현한다.

## 6. 수집 Runtime

```text
request validation
  → registered source lookup
  → explicit collection policy
  → Run start
  → server credential resolution
  → one collector
  → timeout / bounded retry
  → raw private storage + hash
  → same-hash check
  → one normalizer
  → lineage validation
  → normalized upsert
  → Run complete
```

Runtime은 공급자 응답을 fixture로 대체하지 않는다. credential 누락, 401/403/404/429, timeout, invalid payload와 adapter 누락을 별도 안전 code로 기록한다. caller cancellation은 Run을 `Cancelled`로 마감한 뒤 호출자에게 다시 전달한다.

retry는 collector가 retry 가능 오류로 명시하거나 Runtime timeout이 발생한 경우에만 요청의 최대 횟수 안에서 수행한다. 최대 시도 횟수는 5회다. provider별 backoff·quota 정책은 P6 adapter가 오류의 `RetryAfter`와 retry 가능 여부로 전달한다.

## 7. Raw와 저장 구조

원자료 본문은 private object storage에 저장한다. SHA-256, 길이, content type, 원래 파일명, source version, 수집·기준시각과 object 위치만 DB에 저장한다.

Dedicated DB 구조:

- `public_data_ingestion_runs`
- `public_data_raw_snapshots`
- `public_data_normalized_records`
- `public_data_region_mappings`

별도 `PublicDataIngestionDbContext`와 `__EFMigrationsHistory_PublicDataIngestion`을 사용한다. 앱의 명시적 DB 초기화 흐름에서 이 context의 migration도 적용한다.

동일 `SourceId + DatasetId + ContentHash` 원자료가 이미 있으면 기본적으로 normalizer와 normalized upsert를 반복하지 않는다. `ForceReprocess`는 정규화 규칙 변경처럼 원자료를 다시 처리해야 할 때만 사용한다. 현재 저장 adapter는 hash 확인 전에 private object upload를 수행하므로 동일 본문의 DB 재처리는 막지만, 물리 object 중복 제거는 대용량 P6 adapter의 후속 개선 대상이다.

## 8. Normalized Data와 Geography

공급자 고유 DTO·지역 code는 normalized 경계 밖으로 나오지 않는다. normalized record는 다음을 보존한다.

- stable ID와 deterministic record key
- source·dataset·raw snapshot lineage
- Ssalddel `RegionStableId`
- metric, numeric/text value, unit
- evidence as-of와 collected-at
- spatial precision, quality, limitation과 dimension
- source version과 data revision

`RegionStableId`는 `country`, `region`, `point`, `grid`, `area`, `raster` scope를 구분한다. 외부 code는 `외부지역CodeMapping`을 통해 변환하고 그 code를 World identity로 직접 사용하지 않는다.

normalized record key는 다음 논리 key의 SHA-256이다.

```text
SourceId + DatasetId + RegionStableId + Metric + EvidenceAsOf + Dimension
```

같은 batch의 중복 key, 잘못된 region, source/dataset lineage 불일치, 누락된 unit/as-of/revision은 저장 전 거부한다. 원자료 숫자를 `좋음`, `부족`, `수요 높음`으로 판단하는 규칙은 서버 또는 Unity Interpretation 단계에서 별도 rule lineage와 함께 수행하며 raw/normalized fact에 섞지 않는다.

## 9. Scope와 Unity 연결 경계

공공 토양·인구·가격·기후는 주로 Global/Public 데이터다. 주문·농장·창고·운송은 World 또는 Authorized scope다. public cache와 authorized cache는 공유하지 않는다.

P8 이후 API는 provider 이름이나 provider wire DTO가 아니라 Ssalddel normalized projection을 반환한다. Unity는 다시 별도 ApiModel과 Mapper를 사용한다.

```text
Server normalized projection
  → Unity ApiModel
  → DataSnapshot (fact)
  → SharedWorldState (meaning)
  → PerspectiveWorldState (role + intent + zone)
  → chart / map / heatmap / legend / world / panel
```

Unity Presentation은 pH, 경지면적이나 가격 원자료를 직접 판정하지 않는다. authorization이 바뀌면 Data부터 재조회하고, 동일 authorized data 안의 관점 변경만 SharedWorldState를 재사용한다.

## 10. P6 진입 조건과 다음 우선순위

P6는 [농업 토지·토양 공급자 계약 조사](AgriculturalExternalDataProviderContractResearch.md)에 따라 P6-A 계약 조사와 P6-B 실제 연결을 분리한다. P6-A에서는 provider 수가 여러 개여도 metadata만 등록할 수 있지만, P6-B에서는 공급자를 여러 개 붙이지 않고 농업 토지 또는 토양의 공식 source 하나만 고른다.

1. 현재 공식 제공 여부와 endpoint/download 위치
2. credential·가입·승인 필요 여부
3. license, 재배포와 attribution 조건
4. 지표 정의·단위·깊이·공간 및 시간 해상도
5. paging, 파일 크기, rate limit과 갱신 주기
6. 원자료 sample과 checksum
7. 한국·미국·중국 비교에 실제로 동일한 지표인지 여부

그다음 순서는 `source registration → collector → raw parser → normalizer → focused live call → stored lineage verification`이다. live call, 운영 DB 저장, API projection, Unity runtime proof는 각각 별도 검증 사실로 보고한다.

## 11. 검증 기준

- source 등록과 credential 계약을 분리한다.
- collection은 기본 비활성이다.
- secret 값은 log·DB·contract에 남지 않는다.
- provider 실패는 simulation으로 대체되지 않는다.
- 같은 raw hash는 기본적으로 다시 정규화하지 않는다.
- source, unit, as-of, precision, limitation과 revision이 normalized record까지 유지된다.
- region code mapping과 stable ID가 공급자 code와 분리된다.
- migration 생성과 server build를 검증한다.
- live provider call, 실제 DB migration과 Unity runtime은 실행했을 때만 별도 완료로 기록한다.
