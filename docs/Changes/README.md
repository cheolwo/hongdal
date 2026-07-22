# 살뜰 커밋별 시각 변경 기록

이 폴더는 커밋별 화면 변화를 실제 렌더링 캡처와 함께 추적합니다. 서버·DB·문서만 바뀐 커밋도 누락하지 않고 `화면 없음` 또는 `간접 확인`으로 표시합니다.

## 날짜별 기록

| 날짜 | 기록 | 주요 화면 변화 |
| --- | --- | --- |
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
| 2026-07-19 | [커뮤니티 글쓰기 이미지 생성](2026-07-19-community-authoring-image.md) | 글을 최대 5개 연속 문맥으로 나눠 각각의 Kie.ai 프롬프트·생성 상태·미리보기·첨부 선택을 관리하고 글 저장 뒤 문맥 순서대로 사진을 연결하는 관리자 도구 |
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
| 2026-07-18 | [미국 구매자 공동수입 배송 여정](2026-07-18-us-buyer-collective-import.md) | 미국 구매자 수요·가원장·역할 구성부터 중국 공장 출발 전 전처리, 보세·통관·풀필먼트·참여자 주소 배송까지 11단계를 별도 계약 경계와 함께 표시 |
| 2026-07-18 | [미국 3PL 업체 문의 준비](../Architecture/UnitedStatesThirdPartyLogisticsProviderDirectory.md#업체-문의-초안-준비) | 화면 없음 — 보세-주소 후보 10개의 공식 문의 채널과 영문 초안을 관리자 전용 API로 준비하고, 발신자·실제 주소·수신거부·업체별 승인 없이는 차단하며 자동 발송은 비활성 |
| 2026-07-18 | [미국 3PL 후보 디렉터리](../Architecture/UnitedStatesThirdPartyLogisticsProviderDirectory.md) | 화면 없음 — 범용 23개 업체와 보세-주소 역할 후보를 전산화하고, 미국 공동수입 가원장에 보세시설·in-bond·풀필먼트·참여자 주소 배송 슬롯 및 기존 역할 참여 API를 연결 |
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
