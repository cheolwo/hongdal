# 대한민국 행정구역별 건물 데이터 원장

## 목적

법정동·행정동과 건물을 먼저 출처가 있는 DB 원장으로 구성한 뒤, Unity의 `Area`와 Synty 경관이 이 원장의 읽기 전용 관점별 조회 결과를 사용하게 한다.

`어느 행정동에 어떤 건물이 있는가`는 하나의 원본이 직접 제공하는 사실이 아니다.

```text
행정안전부 행정기관·관할 법정동 코드
  → 행정동과 법정동의 기준일별 관계

국토교통부 건축HUB 건축물대장
  → 건물의 대장 PK·주소·주용도·구조·층수·높이·면적

국토교통부·VWorld GIS건물통합정보
  → 건물 footprint·대표점·공간 위치
```

행정동과 법정동은 같은 식별자가 아니다. 주소의 법정동코드만 보고 행정동을 영구 속성으로 복사하지 않고, 기준일이 있는 관할 관계 또는 행정동 경계와 건물 대표점의 공간 포함 판정으로 연결한다.

## 확인한 공식 자료

| 자료 | 권위 | 현재 확인 상태 | 갱신·제한 |
| --- | --- | --- | --- |
| 행정안전부 `jscode20260301.zip` | 행정기관코드, 법정동코드, `KiKmix` 관할 관계 | 원본 확보, SHA-256 `8AF8C1F122D67D43518F58B37AEA6EEA7986F2809062F24E2E03465F21AE7A08` | 2026-03-01 시행본. 변경 시행본마다 새 snapshot으로 수집 |
| 국토교통부 건축HUB 건축물대장정보 | 기본개요·총괄표제부·표제부·층별개요 등 | 명세 확인. 현재 공통 key 호출은 HTTP 403, `SERVICE_KEY_IS_NOT_REGISTERED_ERROR(30)` | 월간, 개발계정 10,000건. 건축HUB 전환 뒤 PK 변경에 유의 |
| 국토교통부 GIS건물통합정보 | 건물 도형과 건축물대장 융합 속성 | VWorld `건물통합정보_마스터` 평창군 2026-08 원본 확보·DBF 37,383행 적재 | EPSG:5186. SHP 도형의 DB geometry 투영은 후속 단계 |
| 주소기반산업지원서비스 건물DB·전자지도 | 건물관리번호·도로명주소·건물 도형 보조 연결 | 결합 경로 확인, 원본 미확보 | 주소관리번호와 건물관리번호가 1:N일 수 있음 |

현재 로컬 원본은 `artifacts/local/public-spatial/administrative-codes/20260301/`와 `artifacts/local/public-spatial/source/20260813/`에 보관한다. raw 원본과 인증키는 Git에 포함하지 않는다.

## 전국 행정구역 원장 적재 결과

2026-08-13에 행정안전부 `jscode20260301.zip` 전체를 `PublicDataIngestionDbContext`에 실제 적재했다.

| 항목 | 적재 결과 |
| --- | ---: |
| 행정기관(행정동) 정규화 record | 3,922 |
| 행정기관–법정동 관할 관계 record | 21,817 |
| 합계 | 25,739 |
| 최초 적재 | 추가 25,739, 거부 0 |
| 동일 원본 강제 재처리 | 기존 25,739, 추가·갱신·거부 0 |
| Data revision | `mois-hjd-bjd-20260301-8af8c1f122d67d43518f` |

원본 ZIP은 private raw storage와 raw snapshot metadata로 보존하고 정규화 record는 행정기관과 관할 관계를 별도 metric으로 저장한다. 수집 source는 기본 비활성이고 명시적으로 활성화한 command에서만 실행했다.

## 평창군 관할 관계 표본

행정안전부 `KiKmix.20260301`에서 평창군 코드 `51760` 범위를 실제로 읽은 결과는 98관계다.

| 행정기관 | 행정기관코드 | 관할 법정 읍·면·리 관계 수 |
| --- | --- | ---: |
| 평창읍 | `5176025000` | 32 |
| 미탄면 | `5176031000` | 10 |
| 방림면 | `5176032000` | 4 |
| 방림면계촌출장소 | `5176032500` | 1 |
| 대화면 | `5176033000` | 6 |
| 봉평면 | `5176034000` | 10 |
| 용평면 | `5176035000` | 9 |
| 진부면 | `5176036000` | 19 |
| 대관령면 | `5176038000` | 7 |

첫 AreaSet과 직접 관계되는 법정리는 대관령면의 유천리·병내리·차항리·횡계리·수하리·용산리, 진부면의 하진부리부터 호명리까지 18개 리, 평창읍의 상리부터 뇌운리까지 31개 리다. 읍·면 자기 자신을 나타내는 법정 코드도 `KiKmix` 관계 수에 포함한다.

## DB 적재 층위

### 1. 원본 수집 원장

기존 `PublicDataIngestionDbContext`를 재사용한다.

- `public_data_ingestion_runs`: 제공처·dataset·시작/종료·성공/차단 상태
- `public_data_raw_snapshots`: 원본 SHA-256·크기·기준일·비공개 저장 위치
- 건축HUB 권한이 없을 때도 `PermissionMissing`과 제공처 오류 code를 기록하고 빈 성공으로 만들지 않는다.

### 2. 행정구역 기준 원장

기존 `regional_agricultural_map_regions`와 code assignment를 확장해 법정동과 행정기관을 별도 scheme으로 보존한다.

```text
KR-MOIS-BJD     법정동 코드
KR-MOIS-HJD     행정기관(행정동) 코드
KR-MOIS-HJD-BJD 기준일별 행정기관–관할 법정동 관계
```

`KiKmix` 관계는 `administrative_legal_jurisdictions`에 행정구역 ID, 법정구역 ID, 기준일, 유효기간, 원본 snapshot과 상태를 저장한다. 동일 법정동이 여러 행정동과 연결될 수 있으므로 단일 행정동 열로 덮어쓰지 않는다.

### 3. 건축물대장 표제부

```text
building_register_titles
- id / register_management_pk
- register_kind_code / register_type_code
- sigungu_code / legal_dong_code / land_lot
- road_address / building_name / dong_name
- main_purpose_code / main_purpose_name
- structure_code / structure_name
- roof_code / roof_name
- building_area_m2 / total_floor_area_m2
- height_m / above_ground_floor_count / underground_floor_count
- household_count / family_count / approval_date
- source_revision / evidence_snapshot_id
- observed_at_utc / valid_to_utc
```

소유자, 상세 호수, 주택가격과 개인 연락처는 World 구축용 기본 projection에 적재하지 않는다. 이름이 비어 있어도 주용도와 규모가 있으면 유효한 건물이다.

### 4. 건물 공간 도형

```text
building_footprints
- id / source_feature_id
- source_crs / normalized_crs
- geometry_object_reference
- centroid_easting / centroid_northing
- footprint_area_m2
- source_revision / source_hash / evidence_snapshot_id
```

대형 geometry는 object storage의 GeoPackage·FlatGeobuf 또는 타일 산출물로 보존하고, DB에는 검색용 bounding box·대표점·면적과 원본 참조를 둔다. MySQL 공간형을 쓰더라도 원본 파일 hash는 별도로 유지한다.

### 5. 서로 다른 건물 ID의 연결

건축물대장 PK, VWorld feature ID와 도로명주소 건물관리번호를 같은 값으로 가정하지 않는다.

```text
building_identity_links
- source_kind / source_id
- target_kind / target_id
- match_method / confidence_code
- rule_revision / evidence_snapshot_id
```

연결 근거는 `OfficialCrosswalk`, `ExactSourceKey`, `SpatialOverlap`, `AddressAndName`, `ManualReview`로 구분한다. 공간 중첩이나 주소 추정 연결을 공식 PK 연결처럼 노출하지 않는다.

### 6. 지역별 건물 배정 결과

```text
building_region_assignments
- building_id
- legal_region_id / administrative_region_id
- assignment_method / confidence_code
- source_vintage / valid_from / valid_to
```

우선순위는 다음과 같다.

1. 건축물대장 공식 법정동코드로 법정동 배정
2. 행정동 경계와 건물 대표점의 `PointInPolygon`으로 행정동 배정
3. 행정동 경계가 없고 `KiKmix`가 1:1일 때만 `OfficialJurisdictionCrosswalk`
4. 하나의 법정동이 여러 행정동에 속하면 행정동을 미확정으로 두고 공간 경계를 기다림

### 7. Unity용 집계와 표현 계획

Unity에는 개별 원문 행이 아니라 다음 읽기 전용 projection을 제공한다.

```text
RegionBuildingComposition
- RegionStableId / SourceVintage
- ObservedBuildingCount
- MainPurposeAreaBuckets
- HeightAndFloorBuckets
- FootprintDensity
- UnresolvedGeometryCount
- EvidenceKind / SourceHash
```

건물 수와 용도·면적은 Synty Prefab을 같은 개수로 생성하는 명령이 아니다. `Observed` footprint와 높이·용도 bucket은 배치 가능 영역과 경관 밀도의 근거이고, `VisualKey` 선택과 LOD/HLOD는 Presentation 계획에서 별도로 결정한다.

### 8. 건축물 주용도 분류 원장

공식 건축물대장의 `main_purpose_code`와 `main_purpose_name`은 원문 그대로 보존한다. 게임이나 검색에 쓰기 위한 상위 분류는 원문을 덮어쓰지 않고 규칙 개정 번호가 있는 별도 파생 원장으로 기록한다.

```text
public_building_category_catalog
  → 주거 / 농업 / 물류·창고 / 상업·생활 / 업무 / 공공·공동체
  → 산업 / 교육·연구 / 의료·복지 / 문화·관광 / 교통 / 기반시설
  → 종교 / 기타 / 미분류

public_building_register_titles
  → 건축물대장 공식 주용도와 규모

public_building_region_assignments
  → 법정동 사실과 행정동 배정 근거·신뢰 수준

public_building_category_assignments
  → 공식 주용도 원문 + 파생 Category + rule revision

public_administrative_building_category_aggregates
  → 행정동·기준시점·Category별 건물 수와 면적 집계
```

첫 규칙 `kr-building-main-purpose-v1`은 건물 이름이나 Synty Prefab 이름을 추측 근거로 사용하지 않고 공식 주용도명만 분류한다. 주용도가 없으면 `unresolved`, 주용도는 있으나 규칙에 없으면 `other`로 남긴다. 분류 결과의 근거 수준은 `Derived`이며, 향후 공식 코드표를 연결하더라도 원문과 과거 규칙 결과를 보존한다.

2026-08-13에 `PublicDataIngestionDbContext` migration을 로컬 MySQL에 적용하고 15개 Category 정의를 실제 적재했다. 이어 VWorld 평창군 `건물통합정보_마스터` DBF 37,383행을 원본 snapshot과 함께 적재하고 PNU 법정동·용도 분류를 생성했다. 행정동은 PNU만으로 추정하지 않으며 관할 관계나 정확한 공간 포함 검증 전에는 미확정으로 둔다.

### 9. 건축물 형태와 Synty 시각 구성 계획

건축물 형태 계약은 사람이 읽는 한국어 의미를 우선한다. 외부 원본 필드와 DB의 안정적인 기술 열 이름은 유지하지만 코드와 문서에서는 다음과 같이 표현한다.

| 한국어 항목 | 의미 |
| --- | --- |
| 관측 지상층수 | 건축물대장에 기록된 공식 지상층수 |
| 추정 지상층수 | 공식 층수가 없을 때 높이와 용도별 추정 층고로 계산한 값 |
| 표현 지상층수 | Synty Base·Middle·Roof 조합에 사용할 층수 |
| 공식 건폐율 | 원본 건축물대장에 제공된 건폐율 |
| 공식 용적률 | 원본 건축물대장에 제공된 용적률 |
| 단순 건폐 비율 | `건축면적 ÷ 대지면적 × 100`; 공식 건폐율이 아님 |
| 단순 연면적 대지 비율 | `연면적 ÷ 대지면적 × 100`; 법정 용적률이 아님 |
| 건물 바닥면적 등급 | small·medium·large·very-large 표현 구간 |
| 높이 등급 | lowrise·mid-lowrise·midrise·highrise 표현 구간 |
| 밀도 등급 | 공식 용적률 우선, 없으면 단순 연면적 대지 비율을 사용한 표현 구간 |
| 근거 종류 | 관측값 우선·일부 추정·자료 부족 |

`건축물형태Profile`은 공간·원본 근거에서 생성하고 `건축물시각구성계획`은 그 Profile을 Synty 조합 명령으로 번역한다. 7층 건물은 `Base 1 + Middle 5 + Roof 1`처럼 계획하며 실제 Prefab 경로나 이름을 저장하지 않고 `시각FamilyCode`만 사용한다.

```text
건축물대장 공식값
→ 건축물형태Profile
→ 건축물시각구성계획(PresentationOnly)
→ VisualKey/Catalog
→ Synty Base·Middle·Roof Prefab
```

용도별 추정 층고는 `kr-building-massing-v1` 규칙에 기록한다. 물류 창고 5m, 농업·산업시설 4.5m, 상업·업무시설 3.6m, 그 밖의 기본값 3m이며 공식 층수가 있을 때는 사용하지 않는다. 이 값은 시각 표현을 위한 파생 규칙이지 건축물대장 사실이 아니다.

DB에는 `public_building_massing_profiles`와 `public_building_visual_composition_plans`를 추가했다. VWorld 평창군 37,383행에 대해 같은 원본·규칙에서 결정적으로 생성하며, 계획의 층수와 Synty 키는 표현 결과일 뿐 공식 건축물 정보나 실제 입주 사실을 바꾸지 않는다.

### 10. 공개 상호명과 건물 연결 원장

건물에 연결되는 주어는 개인이나 대표자가 아니라 공개 인허가 사업장이다. 행정안전부 지방행정인허가데이터개방의 전체 기초분에서 공개되는 사업장명·업종·업태·영업상태·주소·좌표·인허가일을 수집한다. 대표자명·전화번호·사업자등록번호는 원본에 존재하더라도 기본 정규화 원장에 투영하지 않는다.

```text
지방행정인허가 전체 기초분 CSV
→ public_licensed_business_records
→ 정규화 도로명주소 또는 건물도형 포함 판정
→ public_business_building_assignments
→ public_building_business_aggregates
```

`public_licensed_business_records`는 공개 상호명, 개방서비스, 관리번호, 업종·업태, 영업상태, 주소·공급처 좌표, 인허가·폐업·최종수정 시각과 원본 SHA-256을 보존한다. 지방행정인허가 자료는 인허가 대상 업종만 포함하므로 대한민국 모든 사업체의 전수명부로 표현하지 않는다.

첫 주소 연결 규칙 `kr-public-business-building-match-v1`은 다음과 같이 보수적으로 동작한다.

1. 도로명주소에서 괄호 안 법정동·건물명과 층·호 상세를 분리한 정규화 key를 만든다.
2. 같은 원본 개정의 현행 건축물대장 표제부 중 정규화 주소가 정확히 같은 후보를 찾는다.
3. 후보가 하나일 때만 `ExactNormalizedRoadAddress / DerivedHigh`로 연결한다.
4. 후보가 둘 이상이면 `MultipleCandidates`, 없으면 `NoBuildingCandidate`, 주소가 없으면 `InsufficientAddress`로 남긴다.
5. 주소 연결은 공개 사업장 주소와 건물 후보의 파생 연결이지 현재 실제 입주나 소유 관계의 공식 확인이 아니다.

전체분 파일을 확보하면 다음 명령으로 SHA-256 계보와 함께 적재할 수 있다.

```powershell
dotnet run --project Ssalddel -- `
  --import-localdata-businesses `
  --file=<private-csv-path> `
  --source-revision=<localdata-full-vintage> `
  --encoding=utf-8
```

건축물대장 원본 개정도 확보된 경우 `--building-source-revision=<revision>`을 추가하면 정확한 주소의 단일 후보만 연결하고 건물별 정상·휴업·폐업 수를 집계한다. 상호명 목록은 사업장 원장과 연결 원장을 조회해 만들며 집계 table에 JSON으로 중복 저장하지 않는다.

2026-08-13 현재 LOCALDATA 전체분 다운로드 화면은 터미널과 앱 내 브라우저에서 모두 연결 오류가 발생했다. 따라서 schema, import·연결·집계 Service와 migration은 적용했지만 실제 사업장·건물 연결·집계 행은 모두 0건이다. 연결 실패를 빈 성공이나 전국 조사 완료로 기록하지 않는다.

## 첫 적재 단위

행정구역 기준 원장은 전국을 적재했다. 건물 원장의 첫 검증 단위는 `평창군 51760`으로 제한하고, 연결률과 개인정보 제외를 검증한 뒤 전국 파일 적재로 확장한다.

1. 전국 행정기관 3,922건과 관할 관계 21,817건을 원본 hash와 함께 적재 완료
2. VWorld GIS건물통합정보 평창군 DBF 37,383행과 SHP 원본 수집·속성 적재 완료
3. SHP footprint를 EPSG:5186 geometry로 투영하고 법정동 경계 포함 관계 검증
4. 건축HUB `getBrTitleInfo` 또는 전국 건축물대장 표제부 대용량 파일을 확보해 VWorld 속성과 교차 검증
5. 주용도별 건물 수와 연면적, 이름 있음/없음, 도형 있음/없음, 행정동 미확정 수 보고
6. 검증이 끝난 projection만 `pyeongchang-farm-hub-town-v1` AreaSet이 조회

## 완료 조건

- 같은 원본 hash와 규칙 개정 번호에서 동일한 정규화 key와 집계 hash가 생성된다.
- 법정동과 행정동 code를 한 열이나 한 식별자로 합치지 않는다.
- 건축HUB PK 변경 전후 연결 근거를 보존한다.
- 이름 없는 건물을 누락하지 않고 주용도·규모 기준으로 집계한다.
- 원본 건물 도형이 없는 건물은 임의 좌표에 배치하지 않는다.
- 여러 행정동 후보가 있는 건물은 `UnresolvedAdministrativeRegion`으로 남긴다.
- 폐쇄말소대장은 현행 건물과 분리하며 기본 Game View에 활성 건물처럼 노출하지 않는다.
- 개인·소유권·상세 호수·주택가격은 World projection에서 제외한다.
- Synty 교체나 Unity LOD 전환이 건물 DB와 지역 배정 결과를 변경하지 않는다.

## 현재 차단과 다음 실행

전국 행정기관–법정동 관계 25,739건과 VWorld 평창군 건물 속성 37,383건은 DB 적재를 완료했다. VWorld ZIP의 SHA-256은 `4E99D3EC5EB176D6249809762875774E460722822FFFE749881680F6A82C9485`이며, 같은 UFID가 둘인 원본 행은 `UFID + SGG_OID` 기반 고유 식별자로 충돌 없이 보존한다. 현재 남은 핵심은 SHP 도형 투영, 행정동 공간 배정, 건축HUB 교차 검증과 LOCALDATA 공개 사업장 적재다. 건축HUB 공통 인증키의 HTTP 403은 여전히 별도 활용승인 문제이며 VWorld 적재 성공으로 해소된 것으로 간주하지 않는다.
