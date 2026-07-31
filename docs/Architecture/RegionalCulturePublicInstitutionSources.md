# 지역문화 공공기관·공식 원천 전산화

## 목적

지역문화 화면과 이미지 생성 프롬프트가 관광 홍보 문구나 일반 검색 결과에만 의존하지 않도록, 한국과 미국의 중앙·지역 정부기관 및 공식 데이터 디렉터리를 공통 지역 key와 연결한다.

이 자료는 기관에 민원을 자동 제출하거나 기관의 보증을 표시하기 위한 것이 아니다. 화면과 작성 도구가 지역 문화의 공식 근거를 찾아가고, 마지막 확인 시각과 한계를 함께 보여 주기 위한 읽기 전용 기반이다.

## 관할 구조

| 국가 | 전산화 계층 | 공통 지역 식별자 | 주의점 |
| --- | --- | --- | --- |
| 한국 | 중앙부처·관계기관 → 시·도 → 시·군·구 → 읍·면·동 행정기관 | 행정안전부 주민등록 행정기관 코드, 법정동·지자체 코드 | 행정동과 법정동은 목적이 다르며 주민센터 실제 정보는 지자체 원천으로 재확인한다. |
| 미국 | 연방기관 → 주기관 → county·municipal·township·special district → 시설·프로그램 | Census GEOID·FIPS·정부 단위 코드, 주 약어 | 한국의 행정복지센터와 일대일 대응하지 않는다. 주마다 지방정부 종류와 문화업무 소유 기관이 다르다. |

## 초기 등록 원천

### 한국

| source key | 기관·원천 | 활용 |
| --- | --- | --- |
| `kr-mcst-regional-culture-policy` | [문화체육관광부 지역문화정책과](https://www.mcst.go.kr/site/s_about/organ/staff/staffGuide001.jsp?pDeptCode=0721000000&pIntro=&pTeamCD=1371746) | 지역문화·생활문화·문화도시와 지역문화기관의 중앙 정책 책임 확인 |
| `kr-regional-culture-promotion-agency` | [지역문화진흥원](https://www.mcst.go.kr/site/s_data/corpNaru/corpView.jsp?pSeq=615) | 지역문화 조사·연구, 기관 협력과 정보 수집·공유 관계 확인 |
| `kr-mois-administrative-agency-jurisdiction` | [행정안전부 주민등록업무 행정기관 및 관할구역](https://www.data.go.kr/data/15095148/fileData.do?recommendDataYn=Y) | 읍·면·동과 출장소 등 행정기관 코드 및 법정동 관할 관계 |
| `kr-national-museum-art-museum-standard-data` | [전국박물관미술관정보표준데이터](https://www.data.go.kr/tcs/dss/selectStdDataDetailView.do?publicDataPk=15017323) | 지역 문화시설, 운영기관과 관리기관 연결 |
| `kr-national-cultural-festival-standard-data` | [전국문화축제표준데이터](https://www.data.go.kr/data/15013104/standard.do) | 지역 축제, 주관·후원기관, 장소와 일정 연결 |
| `kr-khs-national-heritage-portal` | [국가유산청 국가유산포털](https://www.heritage.go.kr/main/) | 국가·시도 지정 문화·자연·무형유산과 관리기관 연결 |

### 미국

| source key | 기관·원천 | 활용 |
| --- | --- | --- |
| `us-nea-state-regional-arts-organizations` | [National Endowment for the Arts 주·지역 예술기관](https://www.arts.gov/state-and-regional-arts-organizations) | 주·관할구역 예술기관과 지역 예술기관 연결 |
| `us-nps-state-historic-preservation-offices` | [National Park Service 주 역사보존실](https://www.nps.gov/subjects/nationalregister/state-historic-preservation-offices.htm) | 주별 역사·고고·건축 자원 보존 책임 기관 연결 |
| `us-census-geographic-information` | [Census 지리정보](https://www.census.gov/data/developers/geography.html)·[Gazetteer 파일](https://www.census.gov/geographies/reference-files/time-series/geo/gazetteer-files.html) | 주·카운티·place의 코드, 명칭과 좌표 정규화 |
| `us-census-government-units` | [Census 정부조직 공개 파일](https://www.census.gov/programs-surveys/gus/data/publicusefiles.html) | 실제 주·지방정부 단위와 주요 기관 식별 |
| `us-usa-gov-local-governments` | [USA.gov 지방정부 디렉터리](https://www.usa.gov/local-governments) | 지방정부 공식 사이트와 문화·공원·도서관 부서 탐색 |
| `us-imls-public-libraries-survey` | [IMLS 공공도서관 조사](https://www.imls.gov/research-evaluation/surveys/public-libraries-survey-pls) | 지역 문화 거점인 공공도서관 시스템과 분관 연결 |

## 서버 계약

`regional_culture_public_institution_sources`는 다음을 보관한다.

- `SourceKey`, 국가와 관할 단계
- 기관·디렉터리·공식 데이터 원천의 구분
- 기관명, 감독기관과 문화 관련 책임
- 대응 가능한 `RegionKeyPattern`과 공통 지리 식별자 체계
- 공식 페이지와 데이터 URL, 기계 판독 여부
- 갱신 주기, 근거 확인 시각과 지역별 재확인 필요 상태
- 일대일 대응이 불가능하거나 최신성을 보장할 수 없는 한계

공개 조회는 `GET /api/v1/community/regional-culture/public-institutions`를 사용하며 `countryCode=KR|US`, `jurisdictionLevelCode`로 필터링한다. 조회는 저장된 근거만 반환하고 외부 사이트를 실시간 호출하지 않는다.

## 다음 확장 단위

1. 한국 행정기관 코드와 지자체 공개자료를 합쳐 실제 행정복지센터·문화부서 주소를 지역 key에 연결한다.
2. 미국 Census Government Units와 USA.gov를 합쳐 county·city별 문화·공원·도서관 담당 부서를 등록한다.
3. 각 기관 원천을 문화 이미지 프롬프트의 `RegionKey`와 연결하고, [지역문화 3D 애니메이션 이미지 순차 생성](RegionalCultureAnimationImageGeneration.md)의 생성 승인 전에 최소 한 개의 지역 기관 근거를 요구한다.
4. 폐지·통합·명칭 변경을 이력으로 남기고 화면에는 마지막 확인 시각과 직접 확인 링크를 함께 표시한다.
