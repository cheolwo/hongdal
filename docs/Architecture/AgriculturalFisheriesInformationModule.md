# 농수산물 정보 모듈

## 목적

농수산물 시장과 물류업을 충분히 이해하기 전에 거래나 화물 주선부터 시작하지 않는다. 먼저 공공데이터의 출처, 조사 기준, 품목 코드, 가격 단위와 한계를 정리해 사용자에게 읽기 전용 정보로 제공하고, 필요한 경우 준비 상태·질문·확인 기록만 협업 원장에 남겨 실제 조회 품질과 현장 이해를 축적한다.

현재 단계 코드는 `InformationFoundation`이며 다음 원칙을 따른다.

- 시장 데이터 기능은 공개 정보 조회와 비교만 제공한다.
- 협업 쓰기는 준비 상태, 증빙 메타데이터, 질문·이의와 참여자 확인 기록으로 한정한다.
- 주문, 계약, 기사 배정, 운임 중개, 수수료 정산을 만들지 않는다.
- 가격은 출처, 최신 조사일, 단위, 품목 매칭 품질과 원산지 판정 수준을 함께 표시한다.
- 향후 주선 기능은 이 모듈에 덧붙이지 않고 별도 운영 모듈로 분리한다.

## 모듈 경계

`Hongdal.Services.AgriculturalFisheries.Information`이 정보 활용 규칙을 담당한다.

- `FoodPriceCrosswalkCatalog`: HS 코드와 aT 조사품목 사이의 검토된 연결표
- `AtDomesticFoodPriceLookupService`: aT 일별 가격 API 어댑터
- `I미국농수산가격공급자`: 미국 공식 가격 원천을 교체·추가하기 위한 공급자 경계
- `UsdaNassQuickStats가격공급자`: USDA NASS Quick Stats 농산물·양식 수산물 통계 어댑터
- `미국농수산가격조회Service`: 출처 선택, 요청 정규화, 단기 캐시와 장애 격리
- `AgriculturalFisheriesInformationService`: 출처 안내, 지원 품목 검색, 국내가격 조회와 주의사항
- `AgriculturalFisheriesInformationController`: 공개 읽기 전용 API
- `FoodPriceComparisonService`: 정보 모듈의 국내가격 결과를 소비하고 관세청 수입 통계와 비교

의존성 등록은 `AddAgriculturalFisheriesInformationModule()`에 모아 일반 도메인 서비스와 HTTP 클라이언트 등록부에 흩어지지 않게 한다.

## 현재 데이터 원천

| 원천 | 현재 용도 | 해석할 때 주의할 점 |
|---|---|---|
| 한국농수산식품유통공사(aT) 일별 도·소매 가격정보 | 농축수산물 국내 중도매·소매 가격 | 품질·등급·산지·포장 차이가 있으며 모든 국내시장 표본이 국산 확정값은 아님 |
| 관세청 품목별 국가별 수출입실적 | HS 코드·국가별 CIF 통계단가 비교 | 실제 매입가, 국내 도착원가, 운송 견적이나 소비자가격이 아님 |
| 미국 농무부 농업통계청(USDA NASS) Quick Stats | 미국 농작물·축산물·양식 수산물의 가격·판매 집계 통계 | 미국 공식 품목명·단위·조사 프로그램을 유지하며 국내 aT 가격과 동일한 시장 단계로 간주하지 않음 |

미국 농어업경영체 정보는 전국 단일 공개 명부로 취급하지 않는다. 개별 농장·양식장·어업 자료의 비공개 경계와 인증·검사·자발적 등재·지역 허가 목적별 공개 원천은 [미국 농어업경영체 정보 공개 원천](UnitedStatesAgriculturalFisheriesOperatorInformation.md)에 구분해 전산화한다.

인증키가 없을 때도 모듈 개요와 지원 품목은 조회할 수 있다. 개요 응답은 각 원천의 `Ready` 또는 `NeedsServiceKey` 상태를 표시하며 인증키 값 자체는 노출하지 않는다.

## 공개 API

| 용도 | 메서드와 경로 | 상태 변경 |
|---|---|---|
| 모듈 단계·출처·준비도 조회 | `GET /api/v1/agricultural-fisheries` | 없음 |
| 지원 품목 검색 | `GET /api/v1/agricultural-fisheries/items` | 없음 |
| HS 품목의 국내가격 조회 | `GET /api/v1/agricultural-fisheries/items/{hsCode}/domestic-price` | 없음 |
| 미국 농수산물 가격·판매 통계 조회 | `GET /api/v1/agricultural-fisheries/us-prices` | 없음 |
| 미국 농어업경영체 정보 원천 조회 | `GET /api/v1/agricultural-fisheries/us-operator-information-sources` | 없음 |
| 한국 육류 수입 준비 절차도 조회 | `GET /api/v1/agricultural-fisheries/import-readiness/diagram` | 없음 |

국내가격 조회는 지원하지 않는 HS 코드에 `MappingRequired`, 잘못된 요청에 `InvalidRequest`, 외부 자료를 얻지 못한 경우 `DataUnavailable`을 반환한다. 연결되지 않은 품목을 임의의 유사 품목으로 자동 대체하지 않는다.

## 미국 가격 공급자

첫 미국 공급자는 [USDA NASS Quick Stats API](https://quickstats.nass.usda.gov/api)다. 일반 농산물뿐 아니라 [Catfish Production](https://www.nass.usda.gov/Surveys/Guide_to_NASS_Surveys/Catfish_Production/)과 [Census of Aquaculture](https://www.nass.usda.gov/Surveys/Guide_to_NASS_Surveys/Census_of_Aquaculture/)에 포함되는 양식 수산물의 가격·판매 통계도 같은 공급자 경계로 조회한다.

```http
GET /api/v1/agricultural-fisheries/us-prices
    ?commodity=CATFISH
    &statisticCategory=PRICE%20RECEIVED
    &program=SURVEY
    &aggregationLevel=NATIONAL
    &domain=TOTAL
    &yearFrom=2023
    &yearTo=2026
```

- `commodity`는 `CATFISH`, `TROUT`, `CORN`처럼 NASS의 공식 영문 품목명을 사용한다.
- `program`은 `SURVEY` 또는 `CENSUS`이며, 기본 통계 구분은 `PRICE RECEIVED`다.
- 한 요청의 기간은 최대 20년, 반환 항목은 최대 500건으로 제한한다.
- NASS의 비공개·극소량 표시인 `(D)`, `(Z)` 등은 원문을 보존하고 숫자로 변환하지 않는다.
- 정상 결과는 1시간, 자료 없음은 15분 동안 메모리에 캐시한다.
- NASS 요구 문구인 `This product uses the NASS API but is not endorsed or certified by NASS.`를 응답 주의사항에 포함한다.

키는 추적되지 않는 `appsettings.Local.json` 또는 user secrets의 `PublicData:UsdaNassQuickStats:ApiKey`에 둔다. 키가 없으면 서버 시작은 정상적으로 완료되고 조회 API만 `NotConfigured`와 HTTP 503을 반환한다. 키 자체나 외부 오류 본문은 공개 응답과 로그에 남기지 않는다.

상용 수산물 양륙 가격의 후속 원천으로 [NOAA Fisheries 상업 어업 자료](https://www.fisheries.noaa.gov/topic/resources-fishing/commercial-fishing)를 검토한다. 현재는 안정적인 공식 REST 계약을 확인하지 않은 상태이므로 화면 조회 도구를 스크래핑하지 않고, 공급자 인터페이스만 확장 가능하게 유지한다.

## 육류 수입 준비도 협업 작업공간

소고기·돼지고기 수입은 한국 수입업자, 해외 수출자·작업장, 수출국 정부기관, 농림축산검역본부, 식품의약품안전처와 관세청의 확인이 이어진다. 이 흐름을 단순 체크리스트가 아니라 선행 관계가 있는 13개 노드의 다이어그램으로 제공한다.

```text
제품·HS 확정
  ├─ 국가·품목 적격성 ─┐
  ├─ 해외 작업장 적격성 ├─ 수출 증명서 준비 ─┐
  ├─ 한국 수입자 등록 ───────────────────────┤
  └─ 표시·상업서류 정합성 ──────────────────┤
                                             ▼
                                  선적 전 양측 공동 확인
                                             ▼
                                  선적·콜드체인 기록
                                      ├─ QIA 검역 결과 ─┐
                                      └─ MFDS 검사 결과 ├─ 세관 통관 결과
                                                        ▼
                                              표시·이력·국내 반출 준비
```

작업공간은 기존 `community_ledgers` Mongo 원장을 저장 기반으로 재사용한다. 원장 템플릿 키는 `meat-import-readiness`이며 각 절차 단계는 원장 블록, 현재 상태는 다이어그램 노드 데이터로 함께 저장된다. 별도 거래 테이블이나 수입 실행 엔진을 만들지 않는다.

### 커뮤니티 중심 진입 원칙

수입 전용 앱이나 해외 사용자 전용 커뮤니티를 따로 만들지 않는다. 국내외 사용자는 같은 커뮤니티에서 보통 게시글을 쓰고 읽으며, 게시글에 육류 제품 신호와 수입·수출·검역·통관 같은 국경 간 거래 신호가 함께 있을 때만 정보 협업 후보를 보여 준다.

- 후보 조회는 읽기 전용이며 게시글·원장·업무를 자동으로 만들지 않는다.
- 게시글 작성자가 `ConfirmExplicitStart`와 `ConfirmInformationOnly`를 모두 확인해야 준비도 원장을 만든다.
- 시작한 원장은 결정적 ID로 게시글과 연결되어 재시도해도 중복 생성하지 않는다.
- 이미 다른 원장이 연결된 게시글에는 덮어쓰지 않고 충돌을 반환한다.
- 한국 측 또는 해외 측 어느 쪽이든 게시글 작성자로 시작할 수 있으며, `InitiatorSideCode`는 업무상 참여자 측을 나타낼 뿐 앱 종류나 표시 언어를 결정하지 않는다.
- 준비도 원장은 `CommunityTrustWorkflow`에 속한다. 향후 공동수입 실행용 `GroupPurchaseImportWorkflow`와 자동으로 연결하지 않는다.

표시 언어는 국가·역할·국내외 운영 프로필에서 추론하지 않는다. 클라이언트가 사용자 설정의 `displayLanguage`를 전달하면 서버는 같은 제안 코드, 같은 권한, 같은 원장 템플릿과 같은 API 경로를 유지한 채 제목·설명·질문 문구만 현지화한다. 현재 커뮤니티 제안 응답은 `ko-KR`과 `en-US`를 지원하며 알 수 없는 값은 `ko-KR`로 정규화한다.

각 단계는 다음 협업 기록을 가진다.

- `NotStarted`, `InProgress`, `EvidenceSubmitted`, `ParticipantChecked`, `OfficialResultRecorded`, `Blocked` 등의 상태
- 파일 원문 대신 문서번호, 발급기관, 발급·만료일, 권한 통제 문서 위치를 담는 증빙 메타데이터
- 질문, 답변, 메모와 진행을 막는 이의 및 해결 기록
- 상태 변경 이력과 낙관적 동시성 `Revision`
- 한국 측과 해외 측의 선적 전 공동 확인

정부기관 확인이 필요한 노드는 참여자의 `ParticipantChecked`만으로 완료되지 않는다. 공식 결과의 참조번호 또는 증빙 메타데이터를 남긴 `OfficialResultRecorded`가 필요하다. 이 값 역시 참여자가 결과를 기록했다는 뜻이며 홍달이 승인이나 진위를 보증하는 것은 아니다.

선적 전 공동 확인은 한국 측과 해외 측의 확인이 모두 있어야 완료된다. 선행 단계에 미해결 차단 이의가 있으면 확인할 수 없고, 이미 확인한 뒤 선행 상태·증빙·이의가 바뀌면 기존 확인을 자동으로 지운다.

### 협업 API

절차도는 공개 정보이지만 개별 작업공간은 로그인한 참여자만 조회·변경한다.

| 용도 | 메서드와 경로 |
|---|---|
| 게시글의 선택적 정보 협업 후보 조회 | `GET /api/v1/community/posts/{postId}/opportunities?displayLanguage=ko-KR` |
| 작성자가 게시글에서 준비도 원장 시작 | `POST /api/v1/community/posts/{postId}/opportunities/meat-import-readiness/start` |
| 내 작업공간 목록 | `GET /api/v1/agricultural-fisheries/import-readiness/cases/mine` |
| 작업공간 직접 생성(기존 API 호환) | `POST /api/v1/agricultural-fisheries/import-readiness/cases` |
| 참여 작업공간 조회 | `GET /api/v1/agricultural-fisheries/import-readiness/cases/{caseId}` |
| 단계 상태 기록 | `PUT /api/v1/agricultural-fisheries/import-readiness/cases/{caseId}/steps/{stepCode}/status` |
| 증빙 메타데이터 추가 | `POST /api/v1/agricultural-fisheries/import-readiness/cases/{caseId}/steps/{stepCode}/evidences` |
| 질문·답변·이의 추가 | `POST /api/v1/agricultural-fisheries/import-readiness/cases/{caseId}/steps/{stepCode}/discussions` |
| 차단 이의 해결 기록 | `POST /api/v1/agricultural-fisheries/import-readiness/cases/{caseId}/steps/{stepCode}/discussions/{discussionId}/resolve` |
| 양측 확인 | `POST /api/v1/agricultural-fisheries/import-readiness/cases/{caseId}/steps/{stepCode}/acknowledgements` |

각 응답은 기존 다이어그램 공동작업 SignalR 방에서 사용할 `CollaborationRoomId`를 함께 제공한다. 해외 상대방의 홍달 사용자 ID를 아직 모르면 `PendingAccountLink` 참여자로 생성되며, 실제 계정 초대·연결 UI와 별도 초대 토큰 흐름은 후속 범위다.

국가·품목 수입 가능성, 해외 작업장 상태와 검역증명서 서식은 고정된 허용값으로 저장하지 않는다. 농림축산검역본부, 식품의약품안전처 수입식품정보마루, 국가법령정보센터와 관세청의 공식 원문 링크를 제공하고 `LiveRecheckRequired=true`로 선적 직전 재확인을 요구한다.

## 다음 데이터 축적 순서

1. USDA Local Food API와 FSIS CSV를 개인정보 최소화 규칙과 함께 검증한다.
2. NOAA 수산물 양륙·생산 자료의 안정적인 공식 제공 방식과 NASS 품목 코드 연결을 검증한다.
3. 축산물 등급·도매가격과 aT 조사 가격의 역할을 구분한다.
4. 소비자 체감가격과 온라인 가격은 조사 기준 및 수집 허용 범위를 먼저 정한다.
5. 지역·시장·품질·등급·포장단위별 누락률, 갱신 지연과 매칭 정확도를 시계열로 기록한다.
6. 정보 이용자와 현장 종사자의 질문·오류 제보를 품목 연결표 개선 근거로 축적한다.

## 주선업 단계로 넘어가기 위한 조건

정보가 많아졌다는 이유만으로 주선 기능을 자동 활성화하지 않는다. 최소한 다음 조건을 별도로 검토한다.

- 화주, 기사, 주선사, 시장 운영자의 실제 업무 흐름과 책임 경계를 인터뷰로 확인
- 화물자동차 운수사업 관련 등록·허가, 약관, 보험, 정산과 개인정보 요건의 전문가 검토
- 취소, 사고, 과적, 품질 훼손, 분쟁의 증빙과 책임 절차 설계
- 정보 제공 결과와 거래 추천의 이해상충 및 설명 책임 검토
- 운영 모듈의 별도 권한, 감사기록, 기능 플래그와 중단 절차 마련

이 조건을 통과하기 전에는 `IsBrokerageEnabled=false`를 유지한다.
