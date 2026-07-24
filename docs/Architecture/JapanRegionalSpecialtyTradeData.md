# 일본 지역 특산물·수출입 데이터

## 목적

일본 47개 도도부현의 향토요리·GI 산품·생산 통계·수출 준비 산지와 일본 및 한국의 통관 실적을 서로 다른 근거 원장으로 수집한다. 지역 특산물 탐색을 공동구매 수요 후보로 연결할 수 있지만, 자동 주문·계약·수입 신고·업체 선정은 수행하지 않는다.

## 지역과 실적의 분리

| 관계 | 의미 | 허용하지 않는 해석 |
| --- | --- | --- |
| `JP-01`~`JP-47` 제조업소 소재 | 식약처 해외제조업소 지역명·주소에서 확인한 도도부현 | 원재료 재배·어획지나 법정 원산지 |
| MAFF 향토요리 전승지역 | 향토요리 원문에 명시된 도도부현·지역 | 현재 생산량이나 구매 가능 물량 |
| MAFF GI 생산지역 | 등록 명세에 명시된 보호·생산 범위 | 수출 실적이나 모든 동명 상품의 산지 |
| MAFF 수출산지·사업계획 | 인증·계획에 명시된 산지와 운영 주체 | 실제 수출 완료나 거래 가능 업체 |
| 일본 세관 | 품목·상대국·세관별 통관 실적 | 세관이 위치한 도도부현의 생산 실적 |
| 한국 관세청 | 일본 국가·HS별 한국 수입 실적 | 일본 도도부현별 수출 실적 |

제조업소 지역 분류는 JIS/ISO 숫자 순서에 맞춘 `JP-01`부터 `JP-47`까지의 안정 코드를 쓴다. 국가만 일본으로 확인되고 도도부현 근거가 없으면 `JP-OTHER-UNCLASSIFIED`로 보존하며 도시명이나 통관항만으로 주변 현을 추정하지 않는다.

## 원천과 API key

| 원천 | 자동 수집 | API key | 설정 또는 비고 |
| --- | --- | --- | --- |
| [MAFF うちの郷土料理](https://www.maff.go.jp/e/policies/market/k_ryouri/) | HTML | 불필요 | 기존 `MaffRegionalCuisineRemoteSource` 재사용 |
| [MAFF GI 등록산품](https://www.maff.go.jp/j/shokusan/gi_act/register/index.html) | HTML·PDF | 불필요 | 등록번호·생산지역·상태·기준일 보존 |
| [MAFF 플래그십 수출산지](https://www.maff.go.jp/j/shokusan/export/gfp/240807.html) | HTML·PDF | 불필요 | 인증을 실적이나 계약 가능 상태로 해석하지 않음 |
| [일본 재무성 무역통계](https://www.customs.go.jp/toukei/info/tsdl_e.htm) | CSV·e-Stat download | 다운로드는 불필요 | 월·확보 시점·확정/정정 상태 보존 |
| [e-Stat API v3](https://www.e-stat.go.jp/api/api-info/api-spec) | REST JSON/XML/CSV | `appId` 필요 | `PublicData:Japan:EStat:AppId` 후보. secret 저장소 또는 환경변수 사용 |
| [RESAS API](https://opendata.resas-portal.go.jp/docs/api/v1/agriculture/sales/forLine.html) | REST JSON | API key 필요 | `PublicData:Japan:Resas:ApiKey` 후보. 제공연도 최신성 확인 |
| [한국 관세청 품목별 국가별 수출입실적](https://www.data.go.kr/data/15100475/openapi.do) | REST XML | 공공데이터포털 service key 필요 | 기존 `PublicData:CustomsTradeStatistics:ServiceKey` 또는 `PublicData:DataGoKrServiceKey` |

키는 tracked `appsettings.json`, source, 로그에 넣지 않는다. 개발자 secret, 배포 환경변수 또는 관리형 secret store에만 설정한다. 키가 없는 원천은 수집 상태를 `설정 필요`로 남기고 샘플이나 이전 값을 최신 성공값처럼 대체하지 않는다.

## 연결 키와 단위

1. 특산물은 원천별 등록번호·페이지 ID를 stable source ID로 보존한다.
2. 지역은 `JP-01`~`JP-47`, 필요하면 일본 시정촌 코드를 별도 저장한다.
3. 품목은 국제 공통 HS 6단위를 연결축으로 삼고 일본 9단위와 한국 HSK 10단위를 원문 그대로 보존한다.
4. 통관량·금액은 월, 통화, 중량 단위, 수출·수입 방향, 상대국, 정정 상태를 함께 저장한다.
5. 품목명만으로 HS를 확정하지 않고 후보와 사람의 검토 상태를 분리한다.

## 구현 상태와 다음 단계

- 기존 MAFF 향토요리 수집기는 key 없이 도도부현·전승지역·주요 재료를 수집한다.
- 식약처 일본 제조업소는 47개 도도부현 분류와 `JP-OTHER-UNCLASSIFIED` 보존을 지원한다.
- 원천 카탈로그는 key 필요 여부, 지역 의미와 갱신 원칙을 코드에 고정한다.
- 다음 세로 조각은 MAFF GI 등록산품의 등록번호·생산지역 수집과 원문 snapshot 저장이다.
- 이후 검토된 GI/특산물-HS 연결에만 일본·한국 월별 통관 실적을 결합한다.
