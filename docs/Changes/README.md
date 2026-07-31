# 살뜰 커밋별 시각 변경 기록

이 폴더는 커밋별 화면 변화를 실제 렌더링 캡처와 함께 추적합니다. 서버·DB·문서만 바뀐 커밋도 누락하지 않고 `화면 없음` 또는 `간접 확인`으로 표시합니다.

## 날짜별 기록

| 날짜 | 기록 | 주요 화면 변화 |
| --- | --- | --- |
| 2026-07-31 | [커뮤니티 게시판 네 탭 집중화](2026-07-31-community-board-tabs-simplification.md) | 직접 확인 — 커뮤니티 홈·글 목록·게시판 모음·글쓰기에서 서원, 자유·생활, 지역 문화, 농수산물 가격 네 게시판만 상시 노출하고 기존 게시판 key와 글은 호환 조회용으로 유지 |
| 2026-07-30 | [지역문화 3D 애니메이션 이미지 순차 생성](2026-07-30-regional-culture-animation-generation.md) | 화면 없음 — 98개 지역에 10개 생활 장면 목표를 두고, 공식 근거·고정관념 검토 승인 뒤 한 번에 1장씩 생성하는 서버 진행률·중복 방지·비용 제한 흐름과 서울 스타일 샘플을 검증 |
| 2026-07-30 | [KAMIS·USDA AMS 핵심 분석과 Azure 데이터 이관](2026-07-30-kamis-ams-core-analysis-azure-migration.md) | 화면 없음 — 96개 KAMIS 품목과 AMS·HS·FCL 근거를 분석하고 113만 AMS 관측을 포함한 원장을 Azure로 이관한 뒤 시장 단계 필터·최신일 인덱스와 공개 API 성능을 검증 |
| 2026-07-30 | [Figma 역할 화면 우선순위 정렬](2026-07-30-role-priority-figma-alignment.md) | 직접 확인 — 01 커뮤니티를 핵심 게시판 홈과 전체 운영 디렉터리로 분리하고, 02 KAMIS·FCL 목표, 04 문의·탐색·예약, 05 입고·예외·이력·설정·통관 화면을 역할별 모바일 흐름으로 정렬 |
| 2026-07-30 | [01~05 역할 분리 WebApp](2026-07-30-role-separated-webapps.md) | 직접 확인 — JavaScript 없는 선택 포털과 Community·Orderer·Shipper·Driver·Warehouse 5개 WebApp을 독립 배포하고 역할 홈·하위 route·기존 주소 이동을 Azure에서 확인 |
| 2026-07-30 | [Azure 상시 체험 포털](2026-07-30-azure-preview-portal.md) | 직접 확인 — 기존 Azure VM·Caddy를 재사용하고 Figma `01 Community → 02 Orderer → 03 Shipper`를 WebApp 역할 포털로 구성, 미국 현지 구매자 화면을 제외하고 국내 KAMIS·같이 주문·같이 수입·화주 흐름을 연결 |
| 2026-07-28 | [음식 주문 운영 추적](2026-07-28-admin-food-order-operations-trace.md) | 모바일 직접 확인·결과 카드 간접 확인 — 주문번호로 음식 주문·배차·추천·운송·Outbox 상관관계와 복구 필요 상태를 조회하고 주소·연락처·payload를 제외 |
| 2026-07-28 | [기사 전달과 주문자 음식 수령 확인 분리](2026-07-28-orderer-food-receipt-confirmation.md) | 간접 확인 — 기사 전달 완료와 주문자 수령 확인을 별도 상태로 저장하고, 주문자 상세에서 두 단계를 표시한 뒤 소유권·멱등 요청을 검증해 같은 주문번호를 재조회 |
| 2026-07-28 | [음식점 주문 거절·조리·픽업 준비 상태 전이](2026-07-28-restaurant-order-progress.md) | 로그인 화면 직접 확인·업무 화면 간접 확인 — WebView와 Windows 창에서 로그인 렌더를 재확인했고, 거절 사유·조리시간 변경·픽업 준비 완료는 서버 수신함 원장 기반 멱등 상태 전이로 연결 |
| 2026-07-28 | [주문자 공개 메뉴 기반 음식 주문 제출](2026-07-28-orderer-food-order-submission.md) | Windows 앱 직접 실행 실패·간접 확인 — 공개 메뉴 수량·수령 정보·현장결제 메모를 등록 API에 연결하고 서버 가격 재계산·멱등 ID·주문번호 상세 이동을 추가했으나 Hybrid WebView 흰 화면으로 실제 작성 화면은 미검증 |
| 2026-07-28 | [문서 생명주기와 관리자 관제](2026-07-28-document-lifecycle-admin.md) | 직접 확인 — 원천 원장 Revision·문서 분류·생명주기·SHA-256·접근 정책을 390px 관리자 화면에 표시하고 발행 완료→전달 완료 전이를 실제 메모리 검증 모드에서 확인 |
| 2026-07-28 | [음식점 주문 수신함 재시작·재연결 복구](2026-07-28-restaurant-order-recovery.md) | 직접 확인 — 별도 E2E DB와 실제 주문자·음식점·음식 배달 기사 계정으로 주문 등록부터 전달 완료까지 닫고, RestaurantDeskApp 수신함과 FDriverApp 완료 후 빈 업무공간을 Windows에서 렌더 확인 |
| 2026-07-28 | [창고 출고예정의 실제 운송의뢰·기사 인계](2026-07-28-warehouse-transport-handoff.md) | 기존 Windows MAUI 메뉴 확인·신규 카드 간접 확인 — 실제 하차지·일정·차량 조건 저장 뒤 기사 수락·등록 차량·할당수량·예약재고를 검증해 출고를 한 번만 완료하고 같은 의뢰 ID를 재조회 |
| 2026-07-28 | [Figma 서버·클라이언트 수렴](2026-07-28-figma-code-convergence.md) | 실제 Figma 확인 — `01~09` 역할 계층, 독립 Seller·Admin Mobile, 주문자 통관·온도별 3PL·별도 동의, 음식 주문·화주-기사 폐쇄 루프와 Shipper 계약, Driver 만료·재연결, Warehouse 하차지·인계, Restaurant 재시작 복구를 지표·상태·CTA·화살표가 있는 모바일 화면으로 시각 보완 |
| 2026-07-28 | [주문자 해외 구매 목적과 통관·3PL 안내](2026-07-28-orderer-import-purpose-customs.md) | 간접 확인 — 개인 소비와 사업·판매 목적, 150달러 기준, 식품 목록통관 배제·일반 수입신고를 구분하고 과세·판매·냉장 조건에서 3PL 비교 화면과 포워더 인계로 연결 |
| 2026-07-28 | [물류대행 계약 비용 검토안](2026-07-28-logistics-service-contract-preview.md) | 간접 확인 — 누구나 계약별 화주가 되어 입고·검수·적재·보관·피킹·포장·출고 요율과 예상 비용을 비교하되 양측 서명 전에는 실행되지 않는 모바일 검토 흐름 |
| 2026-07-28 | [화주·용달기사 운송 폐쇄 루프 보완](2026-07-28-shipper-driver-closed-loop.md) | 간접 확인 — 배정 기사·위치 신선도, 인증 화주·기사·관리자 실시간 갱신, 30초 보완 조회, Push Outbox 수신자와 안전한 설치 등록 경계를 연결 |
| 2026-07-28 | [판매자 집단 수요 탐색](2026-07-28-seller-clustered-demand.md) | 간접 확인 — 판매자가 이미 참여한 원장뿐 아니라 상품·배송권·거래 단위별로 모이고 있는 주문자 수요를 비식별 집계로 확인하고 판매 조건 초안 준비로 이어지는 모바일 흐름 |
| 2026-07-28 | [해외 판매자의 한국 수입식품 준비](2026-07-28-seller-korea-imported-food-readiness.md) | 간접 확인 — 해외 판매자·실제 제조시설·한국 수입자를 분리하고 식약처 등록 정보·수출국 증빙·특수 정부 절차·준비도 차단 사유를 SellerApp과 서버 원장에 연결 |
| 2026-07-28 | [판매자 외부 채널 API 자격증명 보안 입력](2026-07-28-seller-channel-api-credentials.md) | 간접 확인 — SmartStore·Coupang·Shopify·Amazon별 요구 필드를 SellerApp에 표시하고, API 키·토큰은 보호 전송 후 서버 전용 Data Protection으로 암호화하며 조회에는 마스킹 상태만 반환 |
| 2026-07-28 | [독립 판매자 앱의 서버 원장 기반 업무 흐름](2026-07-28-seller-app-foundation.md) | 간접 확인 — 화주 앱의 로컬 판매 Simulation과 분리한 `SellerApp`을 추가하고, 판매자 로그인·KR/US 운영시장·판매 페이지·채널·재고·상품·출품·영속 주문과 30초 보완 조회를 서버 원장 기반으로 연결 |
| 2026-07-28 | [모바일 관리자 운영 개요와 운송 관제](2026-07-28-admin-mobile-control.md) | 간접 확인 — 기존 Admin MAUI 앱의 기본 화면을 실제 관리자 대시보드로 전환하고, 30초 자동 조회·토큰 갱신·401 단일 재시도와 실제 운송·운행 기사 읽기 관제를 연결 |
| 2026-07-27 | [음식·화물 공통 운송 실행 프로필](2026-07-27-transport-execution-profile.md) | 간접 확인 — 공통 운송 실행 원장은 유지하면서 음식점 픽업·고객 전달, 마트 상품 픽업·주문 전달, 화물 상차·하차 등 유형별 행동 용어와 필수 상세 경계를 DriverApp·FDriverApp에 연결 |
| 2026-07-27 | [FDriver 추천 응답성과 주소 공개 경계](2026-07-27-fdriver-recommendation-ux.md) | 간접 확인 — 실시간 배차 수신과 30초 보조 조회, 로그인 토큰 자동 갱신, 추천 만료·거절·다건 배달 선택, 수락 뒤 실제 수령자 정보 표시와 수락 전 주소·좌표 축약을 연결 |
| 2026-07-27 | [음식 주문 배차의 MAUI 앱 인계 흐름](2026-07-27-maui-food-delivery-handoff.md) | 음식점 수락의 배차 인계, 기사 제안 수락·픽업·전달 전용 `04.08A`, 주문자 배차 예외 안내를 실제 서버 상태로 연결하고 Windows MAUI API 미연결 상태를 실제 렌더로 확인 |
| 2026-07-27 | [Figma 02A2 주문자 여정의 실제 MAUI 전환](2026-07-27-orderer-figma-02a2-maui.md) | `재료 상세 → 25kg 활용 → 주문 방식 비교 → 배송권 → 같이 주문 → 공급자·긴급 수확`을 독립 Route와 화살표형 화면 지도로 연결하고, 주문자 셸을 파란색으로 통일한 Windows MAUI 실제 렌더 |
| 2026-07-26 | [주문자 같이 주문 전 레시피 활용 판단](2026-07-26-orderer-recipe-guidance.md) | 기존 공식 음식·식재료 레시피 DB를 재사용해 주문자가 자신이 받을 양으로 만들 수 있는 음식·재료량·공식 원문을 확인하고, 자동 전환 없이 개별 주문 계속과 같이 주문 검토를 선택하는 Figma `02.01B`와 읽기 전용 API 추가 |
| 2026-07-26 | [기사 운송대금 지급 승인·Outbox](2026-07-26-driver-payout-approval-outbox.md) | 화면 없음 — 완료 운송·화주 수납·확인 계좌·지급 예정 금액을 관리자가 재검증하고 멱등 승인·비민감 Outbox·재시도 감사를 저장하되 Simulation은 송금 없이 검증하고 Operational은 Provider 미구성으로 차단 |
| 2026-07-26 | [기사 운송대금 지급 준비 API](2026-07-26-driver-payout-preparation-api.md) | 화면 없음 — 화주 최종운임과 기사 지급 예정 운임을 분리 저장하고, 월별 완료 운송의 화주 수납·현장수금·정산계좌 확인 조건을 기사 지급 완료와 구분해 조회하는 읽기 전용 API 추가 |
| 2026-07-26 | [기사 정산계좌 서버 API](2026-07-26-driver-settlement-account-api.md) | 화면 없음 — 기사 본인 정산계좌 조회·등록·철회, 개인정보 동의, 예금주명·계좌번호 암호화 저장, 응답 마스킹, 비민감 감사 로그와 MySQL 마이그레이션 추가 |
| 2026-07-26 | [Driver 콜 범위·알림·푸시 설정](2026-07-26-driver-call-notification-settings.md) | 서버의 전국 콜 여부, 여섯 알림 옵션, 푸시 토큰 등록·해제를 실제 사용자 설정 화면으로 연결하고 자동 배차와 토큰 원문 노출을 막은 Figma `04.20` 추가 |
| 2026-07-26 | [Driver 운행 예약 생성·상세·취소](2026-07-26-driver-reservation-api-flow.md) | 미래 시작시각·시작 위치 검증을 거친 예약 생성, 동일 예약 ID 상세 재조회, 시작 전 본인 예약 취소와 목록 재조회를 실제 모바일 흐름으로 연결한 Figma `04.19` 추가 |
| 2026-07-26 | [Driver API 실제 계약 기반 추천 흐름](2026-07-26-driver-api-ready-recommendation-flow.md) | 추천 응답에 실제로 존재하는 거리·비용·수익·추천점수·차량 적합 정보만 표시하고, 상세 조회·수락 재검증·현재 운송 재조회·오류 재시도·배차 전후 개인정보 공개 경계를 실제 모바일 화면으로 연결한 Figma `04.18` 추가 |
| 2026-07-26 | [일반 운송 기사 상차·하차 조건 카드](2026-07-26-driver-freight-condition-cards.md) | 일반 기사 운송 추천카드를 왼쪽 상차 조건·오른쪽 하차 조건의 고정 2열로 정리하고 독차·혼적, 당상·당착·익착, 차량 조건, 인수증과 지급·비용을 한 카드에서 비교하는 Figma `04.17` 추가 |
| 2026-07-26 | [일반 운송 기사 화주 의뢰 인계 화면](2026-07-26-driver-freight-request-handoff.md) | 화주의 운송 의뢰가 일반 기사에게 추천 후보로 도착한 뒤 거리·비용·기한·증빙 조건을 직접 확인하고, 서버 재검증을 거쳐 배차확정·현재 운송·상차 행동으로 이어지는 Figma `04.16` 추가 |
| 2026-07-26 | [음식 주문 ID 중심 역할 화면 상태 변화](2026-07-26-food-order-cross-app-state.md) | 같은 음식 주문번호의 등록·음식점 수락·기사 배정·픽업·전달 상태가 바뀔 때 주문자·음식점·기사·원장 화면이 무엇을 다시 조회해 표시하는지 한 화면에서 비교하는 Figma 상태 확인 UI 추가 |
| 2026-07-26 | [음식점 데스크 새 음식 주문 접수](2026-07-26-restaurant-desktop-order-intake.md) | 주문자의 음식 주문이 들어오면 음식점 데스크톱에 새 주문 알림·접수 대기열·음식 상세·요청사항·조리 예상시간을 함께 보여 주고 명시적 수락 뒤 조리중·전표·배차 준비로 넘기는 화면 추가 |
| 2026-07-26 | [주문자 인코텀즈 그림 도움말](2026-07-26-orderer-incoterms-help.md) | 공급 조건 옆 물음표에서 FOB·CIF·DDP의 비용·위험·보험 구간을 그림으로 비교하고, CIF의 비용 도착항·위험 선적항 차이를 두 막대로 보여 주는 Figma `02G`와 읽기 전용 도움말 API 추가 |
| 2026-07-26 | [주문자 상품·HS/HTS·공식 수출입 통계 단가 판단](2026-07-26-orderer-trade-unit-price.md) | 내부 상품코드와 HS·국가 세번을 분리 연결하고 관세청의 CIF 수입·FOB 수출 통계 단가를 USD/kg 가중평균으로 보여준 뒤 개별 주문과 같이 주문의 실제 견적·물류비·대기시간을 나란히 판단하는 Figma `02.09A~02.09C` 추가 |
| 2026-07-26 | [주문자 미국 물류 역할·권한 선택](2026-07-26-orderer-us-logistics-authority.md) | 미국 시장에서 motor carrier·property broker·freight forwarder, FMC OFF·NVOCC, CBP customs broker, TSA indirect air carrier와 3PL 시설 근거를 역할별로 확인하고 조건을 충족한 후보만 우선협상 투표로 넘기는 Figma `02.06E~02.06G` 추가 |
| 2026-07-26 | [주문자 공급자·수입 물류대행·3PL 후보 투표](2026-07-26-orderer-partner-candidate-voting.md) | 농업경영체·제조·수입 공급자, 포워더·통관 범위, 국내 3PL 후보를 근거·견적·자격·용량으로 비교하고 우선협상 순위를 투표한 뒤 이의·결의·별도 서명으로 넘기는 Figma `02.06B~02.06D` 추가 |
| 2026-07-26 | [주문자 같이 주문 투표·수령 확인·내 입고 품목 선택](2026-07-26-orderer-vote-receipt-inbound.md) | 서버의 같이 주문 투표와 개인 가상 창고·입고예정원장을 주문자 화면으로 잇고, 수령 뒤 직접 사용·내 입고 품목·보류·문제 처리를 명시적으로 선택하는 Figma `02.06A`, `02.15`, `02.16` 추가 |
| 2026-07-26 | [생산자 긴급 수확 요청과 주문자 집단의 비구속 검토](2026-07-26-urgent-harvest-connection.md) | 폐기 위험 농산물의 수확 기한·생산자 보호 단가·최소 인수 물량·수확 및 인수 책임을 공개하고 자동 구매·가격 인하 없이 주문자 집단이 검토하는 Figma `02.07~02.08`과 서버 적합성 계약 추가 |
| 2026-07-26 | [배송권에서 같이 주문 참여 결정까지](2026-07-26-delivery-scope-together-order.md) | 한국 행정구역과 미국 Census 지역을 분리한 배송권 확인, 같은 생활권의 문화 상품·인근 음식점 같이 주문 목록, 공개 상세·비용 비교·비구속 참여 판단을 잇는 Figma `02.02~02.04`와 공개 상세 API 추가 |
| 2026-07-26 | [농업경영체·해외 제조업체와 주문자의 구독 관계](2026-07-26-supplier-membership.md) | 주문자 개인·배송권 집단의 무료 관심 구독과 유료 멤버십을 분리하고, 공급자 근거·월 구독료·주문 할인·순 혜택을 비교하는 Figma `02.05~02.06`과 실행 없는 서버 미리보기 계약 추가 |
| 2026-07-26 | [주문자 개별 주문·같이 주문 비용·시간 비교](2026-07-26-order-mode-comparison.md) | 문화·인근 매장 상품의 같은 수량을 `개별 주문`과 `같이 주문`으로 나란히 비교하고 총비용·예상 절감액·추가 대기시간·모집 진척·별도 동의 경계를 표시하는 Figma `02.01` 화면과 읽기 전용 서버 비교 API 추가 |
| 2026-07-25 | [01~05 Operational API 세로 슬라이스](2026-07-25-role-app-operational-api-slices.md) | 간접 확인 — 주문자 Catalog, 화주 판매채널 동기화, 기사 알림함 읽음 상태, 창고 작업 진입·입고·피킹을 서버 원장 기본 경로로 전환 |
| 2026-07-25 | [커뮤니티 전체 피드 미디어와 반응형 동영상 재생](2026-07-25-community-feed-media-autoplay.md) | 간접 확인 — 이미지 grid와 화면 중앙의 동영상 하나만 음소거 자동 재생하는 전체 피드, MP4·WebM 작성·저장 경계를 추가하고 관련 test 29개와 Windows MAUI build 통과 |
| 2026-07-25 | [주문자 과일·채소 가격 탐색 흐름](2026-07-25-orderer-produce-price-journey.md) | 기존 공개 가격 비교 Route를 유지하면서 `02 Orderer`에 `가격 탐색 → 재료 선택 → 내 원함 → 함께 주문` 흐름과 `02.02A` 화면을 추가 |
| 2026-07-25 | [USDA 미국 국내 농가가격 비교](2026-07-25-usda-us-domestic-price-comparison.md) | USDA NASS 농가 수취가격을 oz·lb·대표 1개 기준으로 환산하고 원문 단위·지역·기간·한계를 함께 표시하는 미국 가격 비교 화면 추가 |
| 2026-07-25 | [국내 농산물 공영도매시장 경락가격 서버 모듈](2026-07-25-domestic-agricultural-auction-price-module.md) | 화면 없음 — KAMIS 조사 가격과 구분되는 공영도매시장 경매 정산가격의 비식별 live 조회·멱등 archive·전일 수집 배치 추가 |
| 2026-07-25 | [커뮤니티 전체 피드 카드 탐색](2026-07-25-community-all-feed.md) | 게시판별 탐색을 유지하면서 공개 글을 서버 순서대로 이어 보는 전체 피드 카드 탐색을 추가하고 MAUI 실제 렌더·Figma `01A.13` 화면을 함께 확인 |
| 2026-07-25 | [업무 관계의 친구 요청 용어와 설계 기준](2026-07-25-friend-request-terminology.md) | 간접 확인 — 관계 표시를 친구 후보·친구 요청·친구 수락으로 분리하고 활성 코드 명칭과 설계 기준을 통일, Windows build·UI 조립 test 통과 |
| 2026-07-25 | [과일·채소 국가·지역별 가격 비교](2026-07-25-produce-regional-price-comparison.md) | 사과 단일 품목 화면을 과일·채소, 품목, 국가, 지역 순으로 좁히고 같은 중량·접속자 통화로 지역 관측값을 비교하는 MAUI·Figma 화면으로 확장 |
| 2026-07-24 | [다국어 커뮤니티 기반과 일본어 표시](2026-07-24-globalization-foundation.md) | 표시 언어를 카탈로그로 관리하고 `/ja/community`, 일본어 언어 선택·국가 추천·게시글 번역 대상을 연결하며 미번역 공개 문구는 영어로 대체 |
| 2026-07-24 | [커뮤니티App·주문자App 개별주문 흐름 정렬](2026-07-24-community-orderer-harmony.md) | SsalddelApp을 공개 커뮤니티에서 시작하고 `0.0 둘러보기 → 0.5 내 개별주문 → 1.0 함께 주문` 흐름, 별도 공동후보 동의, 같은 원장의 커뮤니티 조회·주문자 업무 관리를 연결 |
| 2026-07-24 | [사과 한 개 동일 중량·현지 통화 가격 비교](2026-07-24-apple-unit-price-comparison.md) | 한국·미국·중국 사과 관측값을 같은 중량으로 환산하고 한국어는 원화, 중국어는 위안, 그 밖은 달러를 기본 표시하는 정보 전용 MAUI·Figma 화면 추가 |
| 2026-07-24 | [Controller 업무 용어 한국어화](2026-07-24-korean-domain-controller-naming.md) | 화면 없음 — 주문자·기사·화주·창고·공통·관리자 Controller의 업무 용어를 한국어화하고 기술 접미사와 기존 Route·EndpointKey는 유지 |
| 2026-07-24 | [주문자 1.0 배포 경계 정리](2026-07-24-orderer-v10-deployment-boundary.md) | 화면 없음 — 1.0 비구속 수요만 여는 Simulation 프로필, 공통 health rollback과 CI 배포 산출물 구성 |
| 2026-07-24 | [API 버전 중심 분류를 업무 의미 중심으로 전환](2026-07-24-api-business-classification.md) | 화면 없음. 제품 버전은 도입 이력으로 분리하고 01~05 Controller를 업무 영역·사용자·업무 동작·Feature 경계로 조회하도록 리팩토링 |
| 2026-07-24 | [업무 앱 로그에서 친구 요청으로 이어지는 장](2026-07-24-community-work-relationships.md) | 02~05에서 생기는 업무 접점을 01 커뮤니티에서 양쪽 당사자가 다시 확인하고 명시적으로 친구 요청을 보내는 Route 추가, 현재 실제 자동 기록인 기사·화주 배차 친구 후보로 검증 |
| 2026-07-24 | [MAUI 01 탐색형·02~05 목적형 역할 홈](2026-07-24-maui-role-purpose-navigation.md) | 기존의 깔끔한 역할별 디자인을 유지하면서 01은 정보 둘러보기, 02~05는 시작·진행·확인·완료 근거의 실제 업무 Route 중심으로 재구성하고 Windows MAUI 5개 화면 확인 |
| 2026-07-24 | [MAUI Warehouse 05 Figma 근접 구현](2026-07-24-maui-warehouse-figma-05.md) | 별도 창고 관리자 MAUI 앱에 Figma 05의 주황색 모바일 Shell과 `05.01~05.20` 입고·검수·재고·피킹·포장·출고 책임을 적용하고 홈·입고·작업·출고 흐름을 실제 Windows에서 확인 |
| 2026-07-24 | [MAUI Driver 04 Figma 근접 구현](2026-07-24-maui-driver-figma-04.md) | 별도 기사 MAUI 앱에 Figma 04의 청록색 모바일 Shell과 `04.01~04.15` 기사 업무 책임을 적용하고 홈·추천·운송·정산 및 기존 네이티브 지도 왕복을 실제 Windows에서 확인 |
| 2026-07-24 | [MAUI Shipper 03 Figma 근접 구현](2026-07-24-maui-shipper-figma-03.md) | 통합 MAUI 앱의 화주 영역에 Figma 03의 밝은 모바일 Shell과 `03.01~03.18` 운송·입고·통관·판매·창고 책임을 적용하고 실제 Windows에서 홈·의뢰·입고·판매 흐름을 확인 |
| 2026-07-24 | [MAUI 공공데이터 전용 게시판](2026-07-24-maui-public-data-boards.md) | 통합 MAUI 커뮤니티에 KAMIS·MFDS·USDA·관세청 원천 게시판 카드와 drawer 바로가기를 추가하고 같은 canonical 게시판·`주기성` route를 실제 Windows에서 확인 |
| 2026-07-24 | [MAUI Orderer 02 Figma 근접 구현](2026-07-24-maui-orderer-figma-02.md) | 별도 주문자 MAUI 앱에 Figma 02의 보라색 모바일 Shell, `02.01~02.14` 업무 책임, 홈·재료·원함·원장 내비게이션을 적용하고 기존 로그인·원장 흐름을 실제 Windows에서 확인 |
| 2026-07-24 | [0.0 커뮤니티 지역 문화·특산물 탐색](2026-07-24-community-regional-culture.md) | 미국의 주와 중국의 현재 성·역사문화권을 구분해 문화 질문·대표 특산물·근거 확인 경계를 보여 주고 커뮤니티 이야기로 연결, desktop·390px 실제 확인 |
| 2026-07-24 | [원천별 주기성 데이터 전용 게시판](2026-07-24-periodic-data-source-boards.md) | KAMIS·MFDS·USDA·관세청 수입단가를 단일 저장 게시판으로 분리하고 관련 게시판은 대표 안내로 `주기성` 목록에 연결, desktop·390px 실제 확인 |
| 2026-07-24 | [MAUI Community 01 Figma 근접 구현](2026-07-24-maui-community-figma-01.md) | 통합 MAUI 앱의 게시판 모음을 Figma 01의 밝은 모바일 Shell, 생활·업무 토글, 업무 묶음, 하단 내비게이션과 FAB로 구현하고 실제 Windows 렌더 확인 |
| 2026-07-24 | [업무 게시판 주기성 주제분류](2026-07-24-work-board-periodic-topic-filter.md) | 업무단위 게시판에서 서버 정기 자료를 전체글·일반글·주기성으로 포함·제외·전용 조회하고 desktop·390px 실제 확인 |
| 2026-07-24 | [게시판별 공공데이터·조사 관계 카탈로그](2026-07-24-community-board-public-data-relations.md) | 화면 없음 — 전체 게시판에 구현·필요 시 조회·배치 준비·연계 예정 원천과 자동 발행·검토·참고 전용 경계를 연결 |
| 2026-07-24 | [정보·시세 수입식품 중국·미국 분류](2026-07-24-information-prices-country-filters.md) | `정보·시세`에서 전체 정보·중국·미국을 전환하고 선택 국가를 URL과 적용 필터에 복원, desktop·390px 실제 확인 |
| 2026-07-24 | [미국 수입식품 제조업소 주별 근거 누적](2026-07-24-us-imported-food-states.md) | 화면 없음 — 식약처 미국 기록을 50개 주·D.C.·미국령·미분류로 누적하고 월 1회 `정보·시세` 시스템 글로 멱등 게시 |
| 2026-07-24 | [중국 수입식품 제조업소 권역 근거 누적](2026-07-24-china-imported-food-regions.md) | 화면 없음 — 식약처 권역 근거를 재료별 RDB 원장에 누적하고 월 1회 `정보·시세` 시스템 글로 멱등 게시 |
| 2026-07-24 | [Figma 커뮤·업무 모드 토글](2026-07-24-figma-community-mode-toggle.md) | Community 우측 상단에 `업무` OFF/ON 스위치를 추가해 커뮤모드의 생활 게시판과 업무모드의 업무 게시판 상태를 한 section에서 비교 |
| 2026-07-24 | [Figma Community 생활·업무 게시판 통합](2026-07-24-figma-community-board-consolidation.md) | `01D` 업무단위 게시판을 `01A`에 통합하고 12개 화면을 생활 게시판 6개·업무 게시판 6개로 재배치해 게시판 모음을 두 부류로 단순화 |
| 2026-07-24 | [Figma 01~05 역할 레이어와 업무단위 게시판](2026-07-24-figma-role-layer-milestone.md) | Figma의 Community·Orderer·Shipper·Driver·Warehouse 화면을 역할별 페이지로 분리하고, 커뮤니티에 6개 업무단위 게시판 화면을 추가한 설계 성과를 실제 PNG로 기록 |
| 2026-07-24 | [업무단위 간괘 게시판 산맥 확정](2026-07-24-community-board-mountains.md) | 간접 확인 — 버전별 일곱 산을 16개 업무단위 산으로 재분류하고 Command·Event·페이지 및 공개 투영 여부를 카드에서 점검, 간괘 `☶` 유지 |
| 2026-07-24 | [3.5 알뜰살뜰 마트 페이지 단일책임 정리](2026-07-24-mart-v35-srp.md) | 간접 확인 — 피킹 주문 목록·stable-ID 상세를 Web·창고 앱 공용 Screen과 독립 Route로 분리하고, 샘플 작업 보드를 실제 서버 원장 허브로 전환하며 3.5 Simulation 배포 override를 추가 |
| 2026-07-23 | [커뮤니티 내비게이션 최소 노출](2026-07-23-community-navigation-minimal.md) | 커뮤니티 공용 메뉴를 공개 커뮤니티·내 정보·내 글로 제한하고 개인 보조 메뉴도 내 정보·내 글만 표시하되 기존 route와 화면 구현은 보존, desktop·390px 실제 확인 |
| 2026-07-23 | [3.0 음식 주문·배달 페이지 단일책임 정리](2026-07-23-food-delivery-v30-srp.md) | 간접 확인 — 음식점 운영 홈에서 실시간 수신함과 정확한 주문번호 상세·수락을 분리하고 샘플 fallback을 제거했으며, 주문자·음식점·배달기사 Route capability와 Simulation 배포 override를 정렬 |
| 2026-07-23 | [2.5 Web 창고·판매 이행 단일책임 정리](2026-07-23-fulfillment-v25-web-srp.md) | 간접 확인 — 화주 재고에서 판매상품 등록을 분리하고 판매상품·채널 출품 목록/생성 Route, 실제 서버 API 연결, 샘플 없는 창고 작업 허브를 추가 |
| 2026-07-23 | [저장 없는 공동구매 체험 모드](2026-07-23-group-purchase-practice-mode.md) | 로그인 없이 명시적으로 표시된 가상 이웃과 집단화를 연습하고, 실제 수요는 별도 화면에서 다시 확인하는 Web·Orderer 공용 체험 화면 추가 |
| 2026-07-23 | [주문자 중심 1.0→1.5 공동구매 흐름](2026-07-23-orderer-individual-first-v1-5.md) | 여러 재료를 개별 원함으로 저장하고 내 원함·내 공동 진행·같이 수입 준비·개별주문 원장을 독립 Route로 조회하는 주문자 흐름을 실제 Windows App에서 확인 |
| 2026-07-22 | [공동구매 수요·모집 OS 분리](2026-07-22-group-purchase-demand-os.md) | 화면 없음 — 1.0 수요 변경·마감·Aging·검토 큐·사람 승인 인계를 실제 원장 조율 OS로 구현하고 1.5 외부 실행은 분리 |
| 2026-07-22 | [음식·재료 탐색에서 비구속 수요 등록까지](2026-07-22-food-ingredient-nonbinding-demand.md) | 공개 재료 근거를 유지하는 독립 수요 Route와 Web·모바일 공용 Screen을 추가하고 로그인 저장·철회, 주문·결제·운송 비실행 경계를 desktop·390px에서 실제 확인 |
| 2026-07-22 | [Warehouse 플랫폼 Home·커뮤니티 Route 단일책임 분리](2026-07-22-warehouse-platform-home-srp.md) | 공용 `CommunityWorkspaceScreen` 기반 Home에는 사방괘·업무 허브만 남기고 게시판 목록·개설 신청·글쓰기·상세·원장·다이어그램을 독립 Route로 분리, desktop·561px 실제 확인 |
| 2026-07-22 | [공동구매 비구속 수요 수명주기](2026-07-22-group-purchase-demand-lifecycle.md) | 화면 없음 — 익명 집단 탐색과 인증된 멱등 수요 저장·변경·철회, 소유권 검증, 철회 수요 집계 제외를 서버에 구현 |
| 2026-07-22 | [공동구매 우선 제품 버전 재정렬](2026-07-22-group-purchase-first-version-roadmap.md) | 화면 구조 변경 없음 — 1.0 공동구매, 1.5 공급·무역 준비, 2.0 운송, 2.5 창고·판매 이행으로 코드·문서 메타데이터 정렬 |
| 2026-07-22 | [Azure Blob 기반 객체 저장소 전환](2026-07-22-azure-object-storage.md) | 화면 없음·간접 확인 — 공개 게시글 이미지와 비공개 증빙·음성의 저장 경계를 분리하고 기존 첨부 UI를 유지한 채 Azure Managed Identity 기반 Blob 저장으로 전환 |
| 2026-07-22 | [사방괘 기본 목적지 Navigation 계약 정렬](2026-07-22-bagua-navigation-contract.md) | 화면 없음·간접 확인 — 공용 사방괘의 판매·창고·운송·합의 링크를 Web·모바일 공통 route 계약과 실제 제공 Route Page로 정렬 |
| 2026-07-22 | [다이어그램 노드 앱별 Navigation Adapter](2026-07-22-diagram-node-navigation-adapter.md) | Web·메인·창고·기사·주문 앱이 실제 제공 화면만 열고 미지원 node는 비활성 안내와 원장 문맥을 유지하도록 분리, 실제 MAUI Windows 확인 |
| 2026-07-22 | [Workspace 앱별 Navigation Capability](2026-07-22-workspace-navigation-capability.md) | 공용 workspace URL을 host capability로 분리하고 창고 앱에서 지원 업무는 `열기`, 주문·기사 전용 업무는 `현재 앱 미지원`으로 표시, 실제 MAUI Windows 확인 |
| 2026-07-22 | [화주 홈 Route 의미·단일책임 정렬](2026-07-22-shipper-home-route-srp.md) | Web·모바일 `/shipper`를 읽기 전용 업무 요약·기능별 진입 공용 Screen으로 통합하고 1.0 이후 기능을 로그인·서버 flag 뒤에 유지, desktop·좁은 viewport 실제 확인 |
| 2026-07-22 | [개인 공간·꾸미기 Route 의미·단일책임 정렬](2026-07-22-community-decoration-route-srp.md) | Web 개인 관리를 상점에서 분리하고 Web·모바일 상점·stable-key 상품·FakePG checkout을 공용 Screen과 canonical route로 통합, desktop·390px 실제 확인 |
| 2026-07-22 | [판매 주문 이행 목표별 Route 단일책임 분리](2026-07-22-order-fulfillment-route-srp.md) | 로컬 복합 탭을 상태 변경 없는 허브와 주문·재고·피킹·포장·정책 route로 분리하고 stable key Action·상단 스크롤 복원, desktop·501px 실제 확인 |
| 2026-07-22 | [판매 주문 조회 Route 의미·단일책임 정렬](2026-07-22-sales-order-mobile-route-srp.md) | Web·모바일 영속 주문 목록·stable-ID 상세를 공용 읽기 Screen으로 통합하고 로컬 이행 Simulation을 별도 route로 분리, desktop·390px 실제 확인 |
| 2026-07-22 | [마트 공개 상품 Route·공용 Screen 단일책임 분리](2026-07-22-mart-product-route-srp.md) | 목록·stable-ID 상세·후기·비구속 주문 요청을 Web·주문자 앱 공용 Screen과 독립 route로 분리하고 desktop·390px 실제 확인 |
| 2026-07-22 | [피킹 작업 Route·공용 Screen 단일책임 분리](2026-07-22-picking-task-route-srp.md) | 목록·stable-key 상세·피킹 실행을 Web·창고 앱 공용 Screen과 독립 route로 분리하고 같은 Key 재조회, desktop·390px 실제 확인 |
| 2026-07-22 | [입고 검수 Route·공용 Screen 단일책임 분리](2026-07-22-inbound-inspection-route-srp.md) | 목록·stable-ID 상세·검수 실행을 Web·창고 앱 공용 Screen과 독립 route로 분리하고 Command 뒤 같은 ID 재조회, desktop·390px 실제 확인 |
| 2026-07-22 | [입고 요청 Route·공용 Screen 단일책임 분리](2026-07-22-inbound-request-route-srp.md) | 목록·신규 신청·stable-ID 상세·입고 완료·창고 등록을 Web·모바일 공용 Screen과 독립 route로 분리하고 Command 뒤 같은 ID 재조회, desktop·390px 실제 확인 |
| 2026-07-22 | [운송 의뢰 상세 Route·공용 Screen 단일책임 분리](2026-07-22-shipper-request-detail-srp.md) | Web·모바일 요약·진행 이력·결제·증빙을 같은 request ID·서버 원장을 사용하는 공용 Screen과 독립 route로 분리, desktop·390px 실제 확인 |
| 2026-07-22 | [운송 의뢰 작성 Route·공용 Screen 단일책임 분리](2026-07-22-shipper-request-authoring-srp.md) | Web adaptive 조립과 Web·모바일 화물·운송·절차·최종 확인 route가 같은 draft·validation·다이어그램 복귀 문맥을 사용하도록 통합, desktop·390px 실제 확인 |
| 2026-07-22 | [커뮤니티 목록·다이어그램 복귀 문맥 통합](2026-07-22-community-return-context.md) | 게시판 검색·필터·보기·focus와 다이어그램 원장·node·zoom·출발 page를 Web·모바일 공용 URL 계약으로 복원하고 3단계 업무 화면 왕복을 desktop·390px에서 실제 확인 |
| 2026-07-22 | [다이어그램 Route·공용 Screen 단일책임 분리](2026-07-22-diagram-route-screen-srp.md) | Web·모바일 `/diagram`을 공용 Screen으로 통합하고 desktop sidebar·mobile bottom sheet, 선택 node·zoom·filter·출발 page 복원을 실제 확인 |
| 2026-07-22 | [공동구매 Route·공용 Screen 단일책임 분리](2026-07-22-group-purchase-route-screen-srp.md) | 목록·개설·stable-ID 상세와 참여·협의·이의·결의·서명·이행 화면을 Web·모바일 공용 Screen으로 분리, desktop·390px 실제 확인 |
| 2026-07-22 | [커뮤니티 Route·공용 Screen 단일책임 분리](2026-07-22-community-route-screen-srp.md) | 작업공간·게시판 관리·원장 초안·글쓰기·추천 목록·추천 상세·영속 글 상세를 독립 Route Page와 공용 Screen으로 분리, desktop·390px 핵심 route 확인 |
| 2026-07-22 | [창고 입고상품 수령 화면 단일책임 분리](2026-07-22-inbound-receiving-srp.md) | 438줄 화면을 43줄 shell과 상태·검색·후보·현장 요청·정확한 저장 결과 책임으로 분리, 실제 재캡처 대기 |
| 2026-07-22 | [구매 근거를 잇는 상품 상세·판매 초안 UI](2026-07-22-purchase-evidence-ui.md) | 완료 원장·공개 후기 근거를 마트 상품 상세와 판매 초안에 비식별로 표시하고 실제 판매 초안 desktop 렌더링 검증 |
| 2026-07-22 | [공식 재료·가격·레시피에서 공동구매 초안까지](2026-07-22-official-ingredient-journey.md) | 실제 KAMIS 가격과 식약처 관련 레시피를 재료별로 표시하고 비식별 근거만 공동구매 초안으로 연결, desktop 렌더링 검증 |
| 2026-07-22 | [도심 생활물류센터 창고 Profile 계층](2026-07-22-urban-logistics-profile.md) | 간접 확인 — 일반 창고 기능을 상속하는 도심 생활물류센터와 마트·공동주택 파생 Profile 조립 |
| 2026-07-22 | [커뮤니티 WebApp 페이지 상태 책임 분리](2026-07-22-community-page-state-srp.md) | 화면 없음 — 게시판 조회·검색·필터 상태와 업무 공간 route 문맥을 Razor 페이지에서 분리 |
| 2026-07-22 | [주문자 음식 주문 내역 단일책임 분리](2026-07-22-orderer-food-order-srp.md) | 320줄 화면을 40줄 shell과 접근·인증·검색·목록·정확한 상세 책임으로 분리하고 실제 OrdererApp desktop·390px mobile 로그인 경계 검증 |
| 2026-07-22 | [판매상품 출품 페이지 단일책임 분리](2026-07-22-product-listings-srp.md) | 209줄 화면을 47줄 shell과 조회·정확한 선택·payload 검토·로컬 Simulation 생성 책임으로 분리하고 실제 MAUI desktop·390px mobile 작업 흐름 검증 |
| 2026-07-22 | [판매 주문 이행 페이지 단일책임 분리](2026-07-22-order-fulfillment-srp.md) | 631줄 화면을 61줄 shell과 조회·Simulation·재고·피킹·포장·알림 정책 책임으로 분리하고 실제 MAUI desktop·390px mobile 작업 흐름 검증 |
| 2026-07-21 | [주문자 마트 주문 요청 단일책임 분리](2026-07-21-orderer-mart-order-request-srp.md) | 260줄 화면을 54줄 shell과 접근·선택·상품·인증·작성·저장 영수증 책임으로 분리하고 로그인·저장·동일 ID 재조회 desktop 검증 |
| 2026-07-21 | [주문자 음식점 탐색 단일책임 분리](2026-07-21-orderer-restaurant-srp.md) | 303줄 화면을 35줄 shell과 접근·검색·결과·정확한 상세·표현 책임으로 분리하고 desktop 실제 조회·상세 전환 검증 |
| 2026-07-21 | [커뮤니티 홈 단일책임 화면 조립](2026-07-21-community-home-srp.md) | 736줄 홈을 313줄 route/mode shell과 머리글·공개 feed·다이어그램·생활 원장·업무 workspace 책임으로 분리, 화면 유지·간접 확인 |
| 2026-07-21 | [커뮤니티 글 목록 단일책임 분리](2026-07-21-community-post-list-srp.md) | 546줄 글 목록을 97줄 조립 shell과 도구막대·표·카드·검색·표현 규칙으로 분리, 화면 유지·간접 확인 |
| 2026-07-21 | [주문자 앱 판매채널 계정 상세조회 계약 복구](2026-07-21-shipper-sales-account-detail-contract.md) | 화면 없음 — MAUI 판매채널 adapter의 정확한 계정 ID 조회 계약 누락을 보완해 Windows 배포 빌드 복구 |
| 2026-07-21 | [커뮤니티 글쓰기 단일책임 분리](2026-07-21-community-post-composer-srp.md) | 794줄 글쓰기를 215줄 조립 shell과 머리글·상태·본문·판매·첨부·문맥·설정 책임으로 분리, 화면 유지·간접 확인 |
| 2026-07-21 | [기사 상차 처리 페이지 단일책임 분리](2026-07-21-driver-transport-pickup-srp.md) | 436줄 route 페이지를 67줄 조립 shell과 운송 요약·이동·상차 사진·인수증·상차 예외 책임으로 분리하고 desktop·390px mobile 검증 |
| 2026-07-21 | [기사 통합 운송 증빙 페이지 단일책임 분리](2026-07-21-driver-transport-proof-srp.md) | 518줄 route 페이지를 59줄 조립 shell과 대상·상차·하차·예외 workflow 및 입력 컴포넌트로 분리하고 desktop·390px mobile 검증 |
| 2026-07-21 | [기사 현재 운송 페이지 단일책임 분리](2026-07-21-driver-current-transport-srp.md) | 541줄 route 페이지를 74줄 조립 shell과 운송 workflow ViewModel, 개요·타임라인·현장 전환·이동 컴포넌트로 분리하고 desktop·390px mobile 검증 |
| 2026-07-21 | [공동 원장 다이어그램 상세 단일책임 조립](2026-07-21-community-ledger-detail-srp.md) | 740줄 상세 컴포넌트를 157줄 shell과 canvas·inspector·실시간 session·presentation 책임으로 분리하고 선택 전환과 desktop·390px mobile 검증 |
| 2026-07-21 | [공동 원장 블록 업무 실행 책임 분리](2026-07-21-community-ledger-node-action-srp.md) | 원장 상세에서 증빙 검증·업로드·운송 상태 Command를 독립 컴포넌트와 ViewModel로 분리하고 desktop·390px mobile 검증 |
| 2026-07-21 | [개인 커뮤니티 페이지 단일책임 분리](2026-07-21-community-personal-srp.md) | 576줄 다중 책임 route 페이지를 얇은 조립 셸과 활동·알림·꾸미기 ViewModel, 일곱 section 컴포넌트로 분리하고 desktop·390px mobile 검증 |
| 2026-07-21 | [구매 근거를 잇는 상품 상세·판매 초안 서버 흐름](2026-07-21-purchase-evidence-product-detail.md) | 화면 없음 — 완료 원장과 공개 후기를 비식별 투영하고, 로그인 후기 작성과 서버 재검증 판매 초안 근거를 연결 |
| 2026-07-21 | [공식 재료별 대표 레시피 관계](2026-07-21-official-ingredient-related-recipes.md) | 화면 없음 — 각 재료에 실제 대표 레시피를 최대 3개 연결하고 저장된 재료·공공가격을 공개 읽기 API로 제공 |
| 2026-07-21 | [농수축산물 가격 수집 후 커뮤니티 발행 파이프라인](2026-07-21-agricultural-price-publication-pipeline.md) | 화면 없음 — KAMIS·USDA 수집 성공 뒤 검증된 가격 요약을 중복 없이 시스템 글로 저장 |
| 2026-07-21 | [공식 레시피 재료의 한국·미국 공공가격 연결](2026-07-21-official-recipe-ingredient-public-prices.md) | 화면 없음 — 명확한 재료만 KAMIS 도·소매가격과 USDA NASS 생산자 수취가격에 연결하고 국가·통화·단위·시장 단계를 분리 표시 |
| 2026-07-21 | [공식 레시피 재료 전산화](2026-07-21-official-recipe-ingredient-index.md) | 화면 없음 — 원문 재료를 보존하면서 18개 분류, 표준 재료 마스터, 레시피별 수량·단위·묶음·파싱 신뢰도 행으로 전산화 |
| 2026-07-21 | [각국 정부 공식 음식 레시피 아카이브](2026-07-21-official-government-food-recipes.md) | 화면 없음 — 식약처·농촌진흥청·일본 MAFF·영국 NHS 레시피를 권리 snapshot과 함께 검토 DB에 보관하고 미국·캐나다·프랑스는 메타데이터 전용으로 차단 |
| 2026-07-21 | [커뮤니티 홈 ViewModel 책임 조립 분리](2026-07-21-community-home-viewmodel-composition.md) | 간접 확인 — 공개 게시판과 명시적으로 여는 연결 도구의 수명·책임을 분리하고 기존 화면 계약 유지 |
| 2026-07-21 | [공동주문 페이지 단일책임 분리](2026-07-21-group-order-page-srp.md) | 1,158줄 라우트 컴포넌트를 61줄 조립 화면과 선적·상품·원가·배송권 수요·지급 표시 책임으로 분리하고 기존 상품 전환·수요 등록·자동집단 재조회 흐름 유지 |
| 2026-07-21 | [국가별 배송권을 잇는 공동주문 수요 원장](2026-07-21-group-order-delivery-scope.md) | 한국 도로명주소·미국 Census 주소를 공개 모집권으로 정규화하고 사용자가 범위를 확인한 뒤, 로그인 수요 등록과 같은 상품·배송권 자동집단 서버 재조회까지 잇는 1차 수직 흐름 |
| 2026-07-21 | [DbContext 경계와 입고 수명주기 리팩터링](2026-07-21-data-model-and-inbound-lifecycle.md) | 간접 확인 — 전용 Context 소유권과 aggregate 관계 검증, 입고 완료·검수 transaction과 정확한 멱등성, 수령·검수 페이지 실패·취소 상태 정합화 |
| 2026-07-20 | [공개 커뮤니티 국가·언어 분리 진입](2026-07-20-community-locale-entry.md) | `/ko/community`·`/en/community`, 계정·쿠키·브라우저·신뢰 국가 추천 우선순위, 상시 언어 전환과 시스템 UI 번역, 게시글 원문 유지 |
| 2026-07-20 | [출고예정에서 이어지는 운송의뢰 로컬 초안](2026-07-20-warehouse-transport-request-draft.md) | 정확한 출고예정의 하차지·희망 일정·차량 조건 입력과 교차검증, 개인정보 입력 안내, 서버 저장·예약·운송 생성 없는 Simulation 경계 |
| 2026-07-20 | [전통 게시판 기본형과 게시글 상세 이동](2026-07-20-community-classic-board-and-detail.md) | `/community`를 밀도 높은 전통 게시판 기본형으로 정돈하고, 글 제목 선택 시 전용 상세 화면으로 이동하며 출발한 게시판 문맥으로 돌아가는 흐름을 desktop·390px mobile에서 검증 |
| 2026-07-20 | [출고예정 원장의 운송 전 읽기 검토](2026-07-20-warehouse-outbound-plan-review.md) | 준비된 출고예정의 정확한 상세, 포장 이력·수량·출발 창고 확인, 하차지·희망 일정·운송의뢰 입력 필요 분리, 변경 Command 없는 읽기 전용 경계 |
| 2026-07-20 | [포장 완료 재고를 잇는 출고 인계 준비](2026-07-20-warehouse-outbound-handoff.md) | 포장 완료 재고의 정확한 상세, 출고예정 원장 준비와 같은 ID 재조회, 동일 수량 멱등 처리, 재고 예약·운송의뢰·배차 분리 |
| 2026-07-20 | [적재 완료 재고를 잇는 창고 포장 작업](2026-07-20-warehouse-packing-task.md) | 적재 완료 재고의 정확한 상세, 전체 가용수량·표찰 확인 뒤 포장 완료, 같은 ID 재조회와 동일 요청 멱등 처리, 재고 차감·출고·운송 분리 |
| 2026-07-20 | [검수 결과를 잇는 창고 적재 작업](2026-07-20-warehouse-put-away-task.md) | 검수 완료 재고의 정확한 상세, 두 현장 확인 뒤 위치 확정, 같은 ID 재조회와 동일 위치 멱등 처리, 위치 이동·출고·운송 분리 |
| 2026-07-20 | [창고 범위의 읽기 전용 재고 현황](2026-07-20-warehouse-inventory-overview.md) | 창고 소유·배정 범위의 최소 재고 목록과 서버 집계, 선택한 정확한 입고상품의 주문·원장 근거, 사용자 ID·계약·정산 비노출과 읽기 전용 경계 |
| 2026-07-20 | [게시판형 커뮤니티 첫 화면 복구](2026-07-20-community-forum-restoration.md) | `/community`를 게시판 분류와 밀도 높은 글 목록이 함께 보이는 대표 화면으로 복구하고 게시판 → 글 목록 → 글쓰기 전용 경로 이동 및 API 장애 중 목록 골격 유지, desktop·390px mobile 검증 |
| 2026-07-20 | [커뮤니티 공개 흐름과 README 집중화](2026-07-20-community-release-flow.md) | 게시판 홈·목록의 복구 상태와 글쓰기·상세 문맥을 보강하고 README를 커뮤니티 대표 화면·배포 URL·저비용 Azure 미리보기 안내 중심으로 축약, desktop·390px mobile 검증 |
| 2026-07-20 | [커뮤니티 문맥을 잇는 창고 피킹 작업](2026-07-20-warehouse-picking-task.md) | 공동의 필요에서 이어진 주문 참조·작업 Key, 접근 가능한 영속 피킹 목록·정확한 상세, 시작/완료와 같은 Key 재조회, 후속 실행 경계의 실제 desktop 검증 |
| 2026-07-20 | [창고 입고 검수와 같은 ID 재조회](2026-07-20-warehouse-inbound-inspection.md) | 접근 가능한 서버 검수대상 목록·정확한 상세, 네 가지 현장 확인, 멱등 수량 검수와 같은 ID 재조회, 실제 desktop 렌더링 검증 |
| 2026-07-20 | [창고 입고상품 수령과 현장 반입 요청](2026-07-20-warehouse-inbound-receiving.md) | 정확한 예정 SKU 조회, 불일치 때만 여는 명시적 현장 반입 요청, 멱등 저장과 같은 입고 ID 재조회, desktop·390px mobile 검증 |
| 2026-07-20 | [통합 베타 업무 페이지 연결](2026-07-20-integrated-beta-pages.md) | 개인정보 동의, 화물·창고, 공동구매·통관·판매, 마트 주문·피킹, 인사 역할 지원·검토를 공용 책임 경계로 연결하고 익명 마트 공개 조회를 실제 렌더링 확인 |
| 2026-07-20 | [주문자 음식점 탐색과 음식 주문 내역](2026-07-20-orderer-food-pages.md) | 공개 행정권역·반경을 직접 선택하는 서버 저장 음식점/메뉴 탐색, 사용자 소유권을 검증하는 음식 주문 내역, 익명 공개 탐색과 로그인 보호 정보의 분리, OrdererApp AppBar 겹침 보정 |
| 2026-07-19 | [한국·미국 화물 운송 주선업 진입비용 메모](2026-07-19-freight-broker-entry-costs.md) | 화면 없음 — 한국 허가권 개인 간 양도·양수 거래비용 약 6천만 원과 미국 FMCSA 재정보증 USD 75,000을 서로 다른 비용 성격으로 README에 명시 |
| 2026-07-19 | [일반 사용자 공통 글쓰기와 커뮤니티 컴파일 모듈](2026-07-19-community-common-writing-module.md) | 화면 변경 없음·간접 확인 — WebApp과 Admin이 같은 공통 작성기를 사용하고, 글쓰기만 선택 등록할 수 있는 DI 경계 및 Contracts-only 서버 커뮤니티 모듈 추가 |
| 2026-07-19 | [커뮤니티 글쓰기 저장·복구 신뢰성](2026-07-19-community-authoring-reliability.md) | 제목·본문 자동 임시저장과 새로고침 복구, 비밀번호 제외, 2단계 초안 비우기, 서버 검증 정합화, 첨부 부분 실패 안내 및 WebApp 기동 DI 경계 보완 |
| 2026-07-19 | [커뮤니티 글쓰기 이미지 생성](2026-07-19-community-authoring-image.md) | 글을 최대 5개 연속 문맥으로 나눠 각각의 이미지 프롬프트·생성 상태·미리보기·첨부 선택을 관리하고 글 저장 뒤 문맥 순서대로 사진을 연결하는 관리자 도구 |
| 2026-07-19 | [커뮤니티 글쓰기 LLM 근거 초안](2026-07-19-community-authoring-ai.md) | 기존 수집 자료와 명시적으로 선택한 YouTube·SNS 조회를 서버 allowlist 안에서 실행하고, 출처·비용·확인 질문이 있는 검토 전용 초안을 현재 글과 다이어그램에 각각 명시 적용하는 관리자 도구 |
| 2026-07-19 | [미국·호주 수입식품 통관 규정 카탈로그](2026-07-19-us-au-imported-food-compliance.md) | 화면 없음 — CBP·FDA·USDA 및 ABF·DAFF·FSANZ 규정을 단계·품목·역할·증빙·공식 근거로 분리한 정보용 카탈로그 |
| 2026-07-19 | [서원 여정 글쓰기 템플릿](2026-07-19-community-vow-journey-template.md) | 서원의 출발점을 여정·자료·참여자 모집으로 나누고 문제·사람·다이어그램·가원장·근거·상호 이익·다음 행동·운영 경계를 한 초안으로 조립하는 데스크톱·모바일 글쓰기 도구 |
| 2026-07-19 | [글쓰기 기간 통계와 근거 그래프](2026-07-19-community-period-statistics.md) | 달력 범위로 수집 자료를 다시 조회해 건수·수치 평균을 최대 12개 구간으로 집계하고 출처·기준일·단위·한계를 포함한 기존 근거 그래프로 가져오는 데스크톱·모바일 글쓰기 도구 |
| 2026-07-19 | [서원 중심 자료조사와 공동행동 글쓰기 작업공간](2026-07-19-community-authoring-workspace.md) | YouTube·SNS 자료를 한 작업공간으로 모으고 서원·예약 발행·수입 다이어그램·Win-Win 점검·출처와 한계를 포함한 근거 그래프로 글을 조립하는 Admin 및 공통 글쓰기 흐름 |
| 2026-07-18 | [한국·미국·호주 농수산물 가격 비교](2026-07-18-agricultural-fisheries-price-comparison.md) | 한국 aT 실제 단가, 미국 USDA NASS 생산자 가격, 호주 ABS 소비자가격지수를 원문 단위와 조사 단계로 구분하고 부분 API 실패에도 국가별 결과를 유지하는 데스크톱·모바일 비교 작업공간 |
| 2026-07-18 | [Admin 페이지 운영 카탈로그](2026-07-18-admin-page-catalog.md) | 현재 0.0 핵심 route 37개를 앱·업무영역·실행 경계·검토·화면 검증 기준으로 조회하고 서버관리자 검토 메타데이터를 관리하는 데스크톱·모바일 작업공간 |
| 2026-07-18 | [음식점 국내·수입 식재료 공급 요청](2026-07-18-restaurant-ingredient-supply.md) | 음식점이 국내 농수산물 산지 공급과 수입 공동공급을 나눠 요청하고, 현재 단가와 물류·부대비용을 합친 예상 도착단가 후보를 직접 비교·선택한 뒤 Simulation 초안으로 저장 |
| 2026-07-18 | [국내 농수산물 산지 직입고와 전통시장 배분](2026-07-18-domestic-market-supply.md) | 국내 공동 출하를 전통시장 직입고로 연결하고, 가정 예약과 시장 조리 가게 식재료 공급을 보호·분리해 공급자·포장·운송·입고 역할, 가게 공급 6단계와 전체 10단계를 표시 |
| 2026-07-18 | [전통시장별 꾸미기 팩과 디자이너 참여](2026-07-18-traditional-market-decoration-pack.md) | 시장 게시판·장날·상품·수령 표식을 한 팩으로 연결하고, 디자이너 초안이 플랫폼 검토와 상인회 승인을 모두 거친 뒤 해당 시장에만 적용되도록 구성 |
| 2026-07-18 | [공동구매 장날과 품목별 전통시장 참여](2026-07-18-community-market-day.md) | 상인회 합의와 청과·채소 상인의 직접 참여를 거쳐 특정일 입고·예약수령·현장판매가 이어지며, 공동구매 예약 재고와 주민 공개판매 재고를 분리해 표시 |
| 2026-07-18 | [수입육 전통시장 2차 가공과 생활권 배송](2026-07-18-traditional-market-imported-meat.md) | 해외 승인시설 도축·1차 가공과 한국 검역·통관을 전통시장 역할에서 분리하고, 허가된 2차 가공·포장·이력관리·생활권 냉장배송의 10단계와 지역 사업자 보상을 표시 |
| 2026-07-18 | [미국 농어업경영체 정보 공개 원천](../Architecture/UnitedStatesAgriculturalFisheriesOperatorInformation.md) | 화면 없음 — 단일 공개 명부가 없는 미국의 집계·인증·검사·자발적 등재·지역 허가·비공개 행정기록 10개 원천을 구분하고 자동 초대·업무 배정을 금지한 조회 API 추가 |
| 2026-07-18 | [미국 구매자 같이 수입 배송 여정](2026-07-18-us-buyer-collective-import.md) | 미국 구매자 수요·가원장·역할 구성부터 중국 공장 출발 전 전처리, 보세·통관·풀필먼트·참여자 주소 배송까지 11단계를 별도 계약 경계와 함께 표시 |
| 2026-07-18 | [미국 3PL 업체 문의 준비](../Architecture/UnitedStatesThirdPartyLogisticsProviderDirectory.md#업체-문의-초안-준비) | 화면 없음 — 보세-주소 후보 10개의 공식 문의 채널과 영문 초안을 관리자 전용 API로 준비하고, 발신자·실제 주소·수신거부·업체별 승인 없이는 차단하며 자동 발송은 비활성 |
| 2026-07-18 | [미국 3PL 후보 디렉터리](../Architecture/UnitedStatesThirdPartyLogisticsProviderDirectory.md) | 화면 없음 — 범용 23개 업체와 보세-주소 역할 후보를 전산화하고, 미국 같이 수입 가원장에 보세시설·in-bond·풀필먼트·참여자 주소 배송 슬롯 및 기존 역할 참여 API를 연결 |
| 2026-07-18 | [이웃에서 시작하는 공동행동 개발 철학](../Architecture/NeighborCenteredDevelopmentPhilosophy.md) | 화면 없음 — 이웃 사랑과 수신·제가·치국을 같은 실천 원리로 보고 자발적 참여·책임·공정한 공동 기록의 개발 기준으로 정리 |
| 2026-07-18 | [관리자 자료 검토와 커뮤니티 글쓰기](2026-07-18-admin-information-review.md) | YouTube·KAMIS 수집 후보를 출처 기준과 함께 검토하고 기존 커뮤니티 글쓰기 초안으로 넘기는 Admin 앱 작업공간 |
| 2026-07-18 | [게시판별 익명 작성과 특색 닉네임](2026-07-18-community-anonymous-posting.md) | 게시판마다 비로그인·로그인·운영자 작성 조건을 표시하고 방문자에게 게시판 특색형 익명 닉네임 자동 발급 |
| 2026-07-18 | [커뮤니티 읽기 테마 패키지](2026-07-18-community-reading-theme.md) | 홈 테마 패키지를 게시판 홈·글 목록·글 본문에 일괄 적용하고 브라우저 재접속 시 선택 복원 |
| 2026-07-18 | [업무 계층형 동적 게시판 주제](2026-07-18-dynamic-community-workflow-topics.md) | 간접 확인 — 창고·주문·판매·운송 4개 업무와 8개 세부 주제를 API·ViewModel 조립 경계로 제공 |
| 2026-07-18 | [사용자 개설 커뮤니티 게시판 승인 경계](2026-07-18-user-created-community-boards.md) | 화면 없음 — 기존 신청·승인 화면을 재사용하고 로그인 신청·관리자 검토·승인 게시판 글쓰기 경계를 보강 |
| 2026-07-18 | [출처 기반 커뮤니티 자동 편집 배치](2026-07-18-community-editorial-batch.md) | 화면 없음 — KAMIS 가격 정보·시스템 성찰문·비식별 원장 활동 요약을 자동 게시하는 서버 배치 |
| 2026-07-17 | [목적별 커뮤니티 게시판과 안전센터](2026-07-17-community-board-taxonomy.md) | 역할별 게시판 중복을 목적별 9개 게시판과 역할·업무 필터로 정리하고 신고·분쟁을 별도 안전센터로 분리 |
| 2026-07-17 | [맥락별 기능·배포 변경](2026-07-17-contextual-commit-log.md) | 커뮤니티 게시판·공동행동, 판매 페이지, 기사 운행 프로필, Preview Site와 역할별 물류 ViewModel |
| 2026-07-13 | [원장 중심 통합 살뜰 앱과 실시간 베스트](2026-07-13-ssalddel-visual-change-log.md) | 역할 기반 통합 홈, 살뜰 생활 게시판, 태극 중심 사방 이동판, 모바일 원장 다이어그램, 꾸미기 FakePG, 원장별 웹 팔레트, 실시간 베스트 |
| 2026-07-12 | [커뮤니티 원장 다이어그램과 FakePG](2026-07-12-ssalddel-visual-change-log.md) | 다이어그램 작업대, 살뜰 1.0 상태 톤, 마트·창고 흐름, FakePG 정산 콘솔, 대화방 |

## 기록 원칙

1. 날짜별 문서의 표에는 커밋 해시, 변경 축, 화면 변경 여부, 시각 증거를 기록합니다.
2. 화면 변경이 있으면 실제 렌더링 PNG를 첨부하고, 문서 자산은 `docs/assets/changes/<날짜>-<주제>`에 둡니다.
3. 검증 중간 산출물, 브라우저 프로필과 캐시는 `artifacts/`에만 두고 Git에 포함하지 않습니다.
4. API·DB 중심 커밋은 화면을 억지로 만들지 않고 `화면 없음`을 명시한 뒤 가장 가까운 후속 UI를 연결합니다.
5. 실제 사용자 정보, 주소, 연락처, 결제·계좌 정보와 운송 증빙 원본은 캡처에 포함하지 않습니다.
