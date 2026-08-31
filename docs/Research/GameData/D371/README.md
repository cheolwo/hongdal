# D371/Q402 현실 시장 비교 통찰 — 제한 조사

- 판본: `game-data-research.market-insight.d371.r1`, 2026-08-30.
- 상태: **기존 코드 재고·공식 안내 3건 확인 완료 / 데이터 수집·반입·재노출 허용 미확정 / 개발 인수 대기**.
- [자료 목록과 참조 hash](evidence.r1.json), [산출물 hash](manifest.r1.json).
- 이전 [첫 Farm 조사](../first-farm.r1.md), [D367 사전 검토](../trade-opportunity-next.r1.md)와 기존 제출 manifest는 수정하지 않는다. 개발 인수·통합 결과도 변경하지 않는다.

## 기준과 결론

공유 저장소 `C:/Users/user/source/repos/Hongdal`의 생존경제 문답 내용 r6/Q402 및 D371을 읽기 전용으로 확인했다. 문답 SHA-256 `4E04E897DF469C795BE55B7EC1C87F4C6771AE96DCF4EC56978608FAE264D419`는 전달값과 일치한다. 공유 HEAD `712c4a08349bcda0b7c4b0489bfb8ad2a1e7087a`, 배정 HEAD `b0c1c8469664ae6cce9e272f93650d6eba796804`. 현행 AGENTS·docs/AGENTS hash는 첫 묶음 기준과 동일했다.

**재사용할 기반은 있으나 Amazon–Alibaba 동일상품·총비용·현실 수익을 검증한 연결은 이번 조사에서 확인하지 못했다.** 출품 준비, 외부 상품 관측, 운영 상품 식별, 실내 참고 카탈로그를 서로 구분해야 한다. 기존 기능이나 과거 호출 성공은 지금 수집·저장·공개해도 된다는 허가가 아니다.

## 기존 코드 재고

아래 경로는 공유 저장소 기준이며 개별 hash는 evidence 파일에 있다. 실제 설정값·키·DB·과거 원 응답은 열지 않았다.

| 후보 | 정적 확인 결과 | 재사용 한계 |
| --- | --- | --- |
| `docs/Architecture/AmazonExportReadiness.md`, `Ssalddel.Contracts/Common/Sales/AmazonExportReadinessPlanner.cs` | 출품 초안과 수출 이행 준비를 나누고 marketplace/seller/productType/배송/반품/통화·수수료 등의 확인 항목 반환 | SP-API 데이터 수집기나 상품 가격차 분석기가 아님. 문서의 특정 FBA 품목 허용 규칙은 이번에 현행 검증하지 않아 재인증하지 않음 |
| `SsalddelApp/Services/Commerce/Amazon/AmazonSpApiProductPayloadBuilder.cs` | 내부 SKU·상품명·가격·이미지에서 출품 payload 초안 작성 | `RequiredFieldsPending`; marketplaceId/sellerId/productType/schema/condition/fulfillmentAvailability 매핑 필요. 실제 등록/동일상품 확인 아님 |
| `Ssalddel/Services/Content/Amazon상품참고자료Service.cs`, `AmazonProductUrlPolicy.cs`, `Ssalddel.Contracts/Common/Content/AmazonProductResearchDtos.cs` | ASIN·입력 ASIN·마켓플레이스 국가·원문URL·관측 UTC·nullable 가격/배송비·통화·속성·Pending 상태 | 마켓플레이스 국가는 제조국/판매자 소재지/출하국 아님. 입력·반환 ASIN을 모두 보존하지만 그 자체가 동일 변형/묶음 검증은 아님 |
| `Ssalddel/Services/External/Apify/ApifyAmazonProductClient.cs`, `docs/Architecture/ApifyAmazonProductResearch.md` | 외부 Actor 경로, 상품 1개 입력, seller/offer·변형 가격 추가 수집 제한, 비용 상한 | 공식 Amazon API가 아님. 2026-07-17 호출 기록은 과거 이력. 이번 호출·가격 검증 0회이며 비용 상한이 무료를 뜻하지 않음 |
| `Ssalddel/Controllers/Admin/07_콘텐츠/Amazon상품참고자료Controller.cs` | 서버관리자전용 POST preview | 호출하면 외부 조회로 이어질 수 있어 미실행 |
| `Ssalddel/Services/External/Apify/ApifyInteriorProductObservation.cs`, `Ssalddel/Services/Options/ApifyInteriorProductsOptions.cs` | Amazon·Alibaba 정규화기, 외부ID/제목/URL/시각/raw hash/revision. 전역·원천 Enabled와 TermsReviewStatus=Approved 검사, 별도 승인 ReferenceOnly 카탈로그 | 실내 용도 참고 경계. 정규화 결과에는 가격·MOQ·규격 비교가 없고 Approved는 상품 동일성·새 용도 재노출 허가가 아님. 저장 구현이 있다는 사실은 보관권/보관기한 준수를 입증하지 않음 |
| `Ssalddel.Domain/판매/상품식별코드맵.cs`, `채널출품.cs` | 내부 상품↔Barcode/QR, 상품↔채널계정/채널상품번호 | 코드 유형은 Barcode/QR이며 GTIN 검증·제조사 모델·동일/유사 검토 상태가 보이지 않음. 새 채널상품번호는 `SalesChannelService`에서 LIST-임시값일 수 있음 |
| `Ssalddel/Services/LogisticsProcessing/SalesOrders/SalesChannelOrderSyncService.cs` | 계정·소유자 안에서 SKU 또는 채널상품번호로 주문 품목 연결 | 운영 주문 매핑이며 서로 다른 플랫폼 상품의 동등성 판정이 아님 |
| `Ssalddel.Contracts/Common/Sales/ProductPageDtos.cs`, `Ssalddel/Services/Sales/판매페이지Service.cs` | 판매 초안 MOQ·통화·원산지·출고지·외부 관측 참조 | MOQ 기본값 1은 Alibaba 판매자의 실제 최소 수량 증거가 아님. 외부 관측을 내부 판매 조건으로 자동 확정하지 않음 |
| `Ssalddel/Services/Content/AmazonAssociatesLinkBuilder.cs` | 추적 ID를 붙인 제휴 링크 초안 경로 | 단순 출처 링크와 광고/제휴 링크를 분리. Q402는 수익모델·제휴 가입·계정 연결 승인 아님 |

새 카탈로그/DB/수집기는 만들지 않았다. 기존 식품 품목 대응·RealityContext 동결 경계는 이전 재고를 참조하되, 식품 수준 관계나 게임 동결값을 상업 SKU 동일성 또는 최신 현실 시세로 사용하지 않는다.

## 공식 무료 안내 3건과 권리 검토

열람일은 2026-08-30 KST. 안내 문서만 읽었으며 상품/견적/이미지/리뷰/원문 파일 표본은 0건이다. 원문을 로컬 파일로 확보하지 않아 원자료 hash는 모두 null이다. 아래는 표시 내용을 조사한 결과이며 개별 계약의 법률 검토가 아니다.

| ID / 기관·자료 | 직접 확인 범위 | 접근·보관·재노출 판정 |
| --- | --- | --- |
| D371-S01 / Amazon / [SP-API Registration Overview](https://developer-docs.amazon/sp-api/docs/sp-api-registration-overview) | 개발자 등록 후 앱 등록, 공개 앱 판매자/벤더 OAuth 승인·Appstore 등록, 비공개 판매자 앱 Professional 계정 요건, AUP/DPP/계약 검토 필요 | 안내는 로그인 없이 열람. 실제 데이터는 익명 무료 공개 API로 간주하지 않음. 현재 계정/역할/토큰/요금은 확인하지 않음. 등록 가능성이 가격 데이터 공개 재배포 허용을 뜻하지 않음 |
| D371-S02 / Amazon / [SP-API 정책 개정 안내](https://developer.amazonservices.com/policy-update-for-sp-api) | 2026-08-25 시행, 저장 암호화·필요 시 30일 이내 정보 삭제·API 보안 및 제한 우회 금지 확대 안내, 이후 계속 사용 시 개정 정책 수락 문구 | 개정 요약만 확인. **모든 데이터를 30일 보관해도 된다는 규칙으로 해석하지 않음.** 실제 데이터 종류·이용 목적별 보관/삭제 사유·공개/재판매 권한은 AUP/DPP/계약 원문과 승인 범위 추가 검토 필요. API 사용/약관 수락 없음 |
| D371-S03 / Alibaba.com / [Terms of Use](https://rule.alibaba.com/rule/detail/2041.htm) | 브라우저에 표시된 Part A: 2026-03-11 갱신, 03-17 시행본. 3.2 콘텐츠 복제/다운로드/재게시/상업적 이용과 서면 허락 없는 체계적 수집 제한. 7.4 거래 당사자의 배송·반품·보증·비용·세금 등 조건 책임 | 상품 자료 수집·보관·재노출 **보류**. 수작업 체계적 수집도 허용으로 추정하지 않음. 특정 공식 API/제휴 허가·보관 기간·판매자 콘텐츠 재사용 허락은 미확인. 판매자에게 견적 요청/연락하지 않음 |

Alibaba는 web 추출에서 메뉴만 나왔으나 동일 공식 페이지를 브라우저로 연 뒤 본문을 확인했다. 페이지 상단의 2024-06-17 표시는 본문 Part A 시행일과 별도로 기록한다. Part B 이전 버전을 현행으로 혼합하지 않았다. 검색에 나타난 Alibaba.co.de/1688/Accio 및 Amazon Shipping/결제 서비스 조건은 대상이 달라 근거로 채택하지 않았다. 회원 로그인·가입·동의·상품 클릭·다운로드를 하지 않았다.

SP-API 안내의 보안 조건을 Apify 상품 크롤링에 대한 허락으로 옮길 수 없다. Amazon 소매 페이지·Apify·제휴 프로그램별 수집/저장/이미지 링크/공개 표시 조건은 이번 3자료로 해결되지 않았다. 원격 이미지 URL만 보관해도 재노출 권리가 자동 확보되는 것은 아니다. 따라서 현재 후보는 **권리 검토 대기**로 반환한다.

## 동일상품과 유사상품: 비교 입력 후보

다음은 Q402를 근거로 한 검토표이며 새 계약/schema 구현이 아니다. 동일상품 후보라도 판매 조건은 별도로 비교한다. 외형/제목 유사도·같은 HS·같은 ASIN 문자열 하나만으로 최종 동일성을 승인하지 않는다.

| 판단 축 | 필요한 근거 | 기존 재고와 부족분 |
| --- | --- | --- |
| 상품 정체성 | 제조사·브랜드·모델/MPN·검증 가능한 GTIN, 플랫폼+마켓플레이스+상품ID, 출처/검토자/시각 | ASIN·외부ID·브랜드/속성은 후보. 플랫폼 간 crosswalk·제조사 확인·검토 이력 미확보 |
| 변형·구성 | 용량/중량·개수·포장단위·크기·재질·규격·전압/플러그·색상·포함품·품질/인증 | 해당하는 차원만 명시. 일반 속성 배열을 검증된 규격으로 간주하지 않음. 묶음/단품·샘플/OEM/정품·보증 차이 미확보 |
| 동일/유사/미확정 | 동일 핵심 규격을 뒷받침하는 증거; 유사 시 일치/차이 항목; 누락 시 미확정 | 현재 두 플랫폼의 실제 상품 쌍 없음. 동일/유사 판정 0건 |
| 제안·MOQ | 판매자·offer/견적 식별자, 최소수량·수량별 가격·샘플/양산 구분·단위·통화·유효기간 | 자체 판매 초안 MOQ=1을 외부 관측에 채워 넣지 않음. 소매 1개 표시가와 도매 구간 최저가 단순 차감 금지 |
| 배송 문맥 | 배송 목적지·출하지·거래조건·운송수단·리드타임·중량/부피·통관 책임 | marketplace 국가는 목적지/원산지/출하지와 다름. Amazon 관측 배송비 필드만으로 국제 착지가 완성되지 않음 |
| 총비용·판매 가능성 | 플랫폼/결제 수수료·환율 기준일·세금/통관·내륙/국제운송·보험·보관·반품/불량·수요/판매 자격 | 준비 여부 Boolean은 견적 금액이 아님. 미확인 비용은 0이 아니라 미확인. 구매절감과 재판매이익 목적 분리 |
| 시간·계보 | 관측시각/시세 기준시각/견적 만료·원문 출처·허용 범위·변환판본 | 게임 가격·확률·보험·동결 이력을 현실 수익 근거로 역산하지 않음 |

## 개발에 반환할 결정과 검증 상한

1. 기존 참고 DTO·식별맵·TermsReviewStatus/ReferenceOnly를 재사용 후보로만 검토한다. 운영 주문 매핑/실내 분류를 동일상품 판정으로 바꾸지 않는다.
2. 상품 수집 전에 Q402의 사용 목적(구매 절감/재판매 참고), 대상 플랫폼/시장·품목, 데이터 접근 경로, 원문 링크와 자체 요약/이미지/가격 재표시 범위를 먼저 결정한다. 권리 미확정 범위는 유지한다.
3. 보관 기한·삭제/갱신·라이선스 증빙·검토 이력과 실제 동일성 증거가 없는 동안 상업 공개/게임 소비를 승인하지 않는다. 상품 쌍 한 건 조사도 허용 경로가 정해진 뒤 별도 범위로 받는다.
4. 새 실행 WI는 미결속. Q399/Q402에만 조사 후보로 연결한다. 아래 마감 재확인에 따라 선택형 현실 참고 화면과 진입 위치는 방향 확정이다. 게임 보상과 광고/실구매 연결을 만들지 않는다.

이번 결과는 데이터 계약·공백 조사다. 실제 가격차·절감액·수익성·배송 견적·실제 상품 존재/판매 가능성·현행 API 작동·계정/DB·게임 구현을 검증하지 않았다. 빌드/Runtime/Save/E/Scene 증거가 아니다. 키/유료 API/Actor 실행·크롤링·상품 원자료 저장·계정연결·약관동의·구매·공유 저장소 수정·commit/push는 없다.

문서 링크·JSON/ID·신규 파일 공백·개별 참조 및 산출물 hash와 기존 첫 묶음 보존을 검사하고 결과를 `artifacts/local/game-data-research/d371-validation.json`에 둔다. 기존 worktree에 현행 Fast 스크립트가 없으므로 공유 경로 밖에 쓰지 않는 제한 검증을 사용한다. 결과는 개발 `01a02198-8b2a-7491-ac93-366b30ff474c`에 먼저 인계하고 개발 검토가 기획으로 돌아간다. 추가 조사 자동 착수 없음.

### 마감 시 기획 갱신 대조

착수 시 전달받은 r6/hash 일치를 확인한 뒤 조사 중 문답이 r8로 갱신됐다. 마감 시 직접 다시 읽은 r8 SHA-256은 `AA25AF041E0F9E0DA2A67C79E4F5598F48D7CDEDB803539FBA7E5C5725727FFF`다. D372는 선택형 별도 현실 참고 열람, D373은 상품 상세·거래 결과 상세의 진입 항목을 확정했다. 이를 미승인으로 되돌리지 않으며 세부 화면/링크 방식·상품 범위·자료 권리는 여전히 미정이다. 이번 조사는 이 상태 설명만 갱신했고 UI/수집/코드를 구현하지 않았다. evidence의 최초 기대 hash와 나중에 캡처한 참조 hash를 같은 시점으로 오인하지 않는다.
