# Client Apps Screen Map

클라이언트 앱 화면이 늘어나면서, 앞으로는 앱 단위와 화면 단위로 개발 범위를 잡는다. 이 문서는 현재 화면 지도를 기준으로 다음 작업을 고르기 위한 정리 문서다.

## OrdererApp

주문자 앱이다. 일반 사용자 주문 경험의 중심이다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| 주문자 홈 | `/` | 음식, 홈달마트, 화물·공산품 주문 진입 |
| 음식 주문 홈 | `/food` | 음식점 주문과 홈달마트 주문 구분 |
| 음식점 주문 | `/food/restaurants` | 반경 km 기준 음식점 조회, 메뉴 진입 |
| 홈달마트 주문 | `/food/mart` | 창고 재고 기반 마트 상품 주문 |
| 화물·공산품 주문 | `/cargo` | 화물 운송 의뢰, 공산품 주문, 견적 |
| 주문 내역 | `/orders` | 음식점, 홈달마트, 화물 주문 상태 통합 조회 |

다음 개발 방향:

- 음식점 주문 반경 정책을 Admin 설정과 연결
- 음식점 메뉴/장바구니/결제 연결
- 홈달마트 재고 조회와 장바구니 연결
- 화물 주문을 기존 화주 운송 의뢰 흐름과 연결

## WarehouseManagerApp

창고 작업자 앱이다. 입고, 포장, 스캔, 홈달마트 도심 물류 작업을 다룬다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| 창고 홈 | `/` | 창고 작업 빠른 실행 |
| 작업자 확인 | `/work/{ProcessCode}` | 휴대폰 뒤 8자리로 작업자/역할 확인 |
| 작업대 확인 | `/work/{ProcessCode}/workbench` | 입고/포장 작업대 바코드 확인 |
| 입고 상품 확인 | `/work/inbound/products` | 상품 바코드 조회, 예정/실제 수량 확인, 입고 확인 |
| 입고 검수 작업 | `/work/inbound/inspection` | 입고 확인 이후 검수 |
| 현장 작업 보드 | `/work-board` | 공정별 대기 작업 보드 |
| 스캔 스테이션 | `/scan` | 바코드 스캔 공통 처리 |
| 홈달마트 홈 | `/mart` | 도심 마트 물류 작업 진입 |
| 홈달마트 작업자 확인 | `/mart/work/{ProcessCode}` | 홈달마트 공정 작업자 확인 |
| 홈달마트 작업 보드 | `/mart/work-board` | 입고/보충/피킹/포장/배달 픽업 보드 |

다음 개발 방향:

- 입고 상품 확인 서비스를 서버 API 구현체로 교체
- 입고 확인 후 검수 작업 항목 자동 생성
- 작업대 확인 기록을 필수 업무 로그로 저장
- 포장 작업대 이후 포장 대상 바코드/라벨 흐름 추가

## DriverApp

기사 앱이다. 운행, 배차, 상차/하차, 정산 축이다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| 기사 홈 | `/driver/home` | 지도 기반 기사 홈 |
| 운행 시작 | `/driver/work/start` | 운행 시작/배차 수신 진입 |
| 추천 목록 | `/driver/recommendations` | 추천 운송/배차 목록 |
| 추천 상세 | `/driver/recommendations/{의뢰Id}` | 배차 건 상세 |
| 배차 처리 | `/driver/recommendations/{의뢰Id}/decision` | 수락/거절 |
| 진행중 운송 | `/driver/transports/current` | 현재 운송 |
| 상차 | `/driver/transports/{운송Id}/pickup` | 상차 완료와 증빙 |
| 하차 | `/driver/transports/{운송Id}/dropoff` | 하차 완료와 증빙/결제 |
| 배달 내역 | `/driver/transports/history` | 과거 운송 |
| 계좌 정보 | `/driver/account/bank` | 정산 계좌 |
| 월정산 | `/driver/settlements/current-month` | 정산 |
| 알림/푸시/설정 | `/driver/notifications...` | 알림 설정과 알림함 |
| 화면 설정 | `/driver/settings/views` | 화면 노출 정책 |

다음 개발 방향:

- 상차/하차 사진 업로드를 필수 증빙 API로 고정
- 배차 수락/거절 이후 후속 이벤트 정리
- 기사 홈 지도와 운행 시작/배차 수신 흐름 연결 강화

## ShipperApp

화주, 판매자, 운송 의뢰자 앱이다. 기능 폭이 넓으므로 화면 그룹 단위 관리가 필요하다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| 홈 | `/`, `/shipper` | 화주 앱 홈 |
| 화물운송의뢰 등록 | `/shipper/request` | 단건 운송 의뢰 |
| CSV 일괄등록 | `/shipper/request/bulk` | 대량 의뢰 |
| 상차/하차 주소 입력 | `/dispatch/address-form` | 주소 입력 공통 |
| 입고 대시보드 | `/shipper/inbound/dashboard` | 입고 요약 |
| 입고 현황 | `/shipper/inbound/requests` | 입고 요청 관리 |
| 창고 운영 허브 | `/shipper/warehouse/workspace` | 창고 관련 업무 진입 |
| 창고 작업 시작 | `/shipper/warehouse/work/{ProcessCode}` | 창고 작업자 확인 흐름 |
| 창고 스캔 | `/shipper/warehouse/scan` | 스캔 |
| 재고 목록 | `/shipper/warehouse/inventory` | 재고 조회 |
| 재위탁 운송 | `/shipper/reconsignment/orders` | 재고 기반 재위탁 |
| 판매채널 연결 | `/shipper/sales/channels` | 네이버/쿠팡 등 |
| 출품 관리 | `/shipper/sales/listings` | 상품 출품 |
| 주문 출고 알림 | `/shipper/sales/orders` | 마켓 주문 출고 |
| HS 코드 검토 | `/shipper/customs/hs-reviews` | 통관/HS 검토 |
| 공개 화물 | `/shipper/public-cargo` | 공개 화물 조회 |
| 화면/프로필 설정 | `/shipper/settings/...` | 사용자 설정 |

다음 개발 방향:

- 운송 의뢰, 판매/입고/재고, 통관/HS 화면 그룹 분리
- ShipperApp 비대화 방지용 화면 정책 정리
- 창고 작업 화면은 WarehouseManagerApp과 공통/전용 경계를 명확히 분리

## RestaurantDeskApp

음식점 운영 앱이다. 주문 수신과 조리 상태 변경 쪽으로 역할을 좁히는 것이 좋다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| 주문 대시보드 | `/` | 음식점 주문 처리 홈 |
| 가까운 음식점 흐름 | `/restaurants/nearby` | 근거리 음식점 노출 흐름 |
| 인기 음식점 흐름 | `/restaurants/popular` | 인기 음식점 노출 |
| 리뷰 운영 | `/reviews/moderation` | 리뷰 관리 |
| 주소 입력 | `/dispatch/address-form` | 배달 주소 입력 |

다음 개발 방향:

- OrdererApp 음식점 주문과 주문 수신 흐름 연결
- 조리 시작/조리 완료/라이더 픽업 상태 전환 추가

## CustomsBrokerApp

관세사 전용 앱이다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| HS 코드 보정 홈 | `/` | 관세사 HS 코드 검토/보정 |
| NotFound | `/not-found` | 없음 처리 |

다음 개발 방향:

- HS 코드 보정 요청 목록
- 요청 상세
- 보정 저장/검토 완료

## HongdalAdmin

운영자 앱이다. 기능 구현보다 정책 조정과 운영 조회가 중심이다.

| 화면 | 라우트 | 역할 |
|---|---|---|
| 홈 | `/` | 관리자 홈 |
| 로그인 | `/login` | 운영자 로그인 |
| 업무 안내판 | `/dashboard` | 운영 대시보드 |
| 유입/배차대기 | `/dispatch/wait` | 배차 대기 관리 |
| 의뢰 관리 | `/requests` | 운송 의뢰 관리 |
| 결제 관리 | `/payments` | 결제 |
| 운송 진행 | `/transports` | 운송 현황 |
| 정산 관리 | `/settlements` | 정산 |
| 기사 관리 | `/drivers` | 기사 관리 |
| 기사 운행 현황 | `/drivers/operating` | 운행 상태 |
| 업체/화주 관리 | `/partners` | 파트너 관리 |
| 공개 화물정보 | `/cargo` | 공개 화물 |
| 파일/POD | `/files/pod` | 증빙 파일 |
| 공통콘텐츠 관리 | `/common-contents` | 공통 콘텐츠 |
| 사용자 행위 로그 | `/activity-logs` | 감사/로그 |
| 화면 정책 | `/view-policies` | 앱 화면 노출 정책 |
| 부가 기능 설정 | `/auxiliary-feature-settings` | 부가 기능 전역/사용자 설정 |
| HS 코드 운영 | `/customs/hs-codes` | HS 코드 관리 |
| 음식 운영 | `/food/operations` | 음식 운영 |
| 구독료 운영 정책 | `/revenue-policies` | 구독료/비용 정책 |
| 문서 관리 | `/documents` | 문서 |
| 차량 관리 | `/vehicle-management` | 차량 |

다음 개발 방향:

- 음식점 주문 반경 정책 Admin 설정화
- 부가 기능 설정의 event/service 확장
- 화면 정책과 앱별 홈/메뉴 노출 연결 강화
