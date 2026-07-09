# ShipperApp 화면 문서

[전체 화면 문서](../README.md) / [앱 전체 카탈로그](../../app-page-catalog.md)

이 문서는 ShipperApp 에 속한 화면별 README를 모은 색인입니다. 각 화면 문서는 캡처 이미지, 화면 책임, 사용자와 참여자, API/서버 연계, 보안 점검을 별도로 설명합니다.

| 페이지 ID | 라우트 | 분류 | 화면 책임 | 캡처 |
| --- | --- | --- | --- | --- |
| [ShipperApp-P01](ShipperApp-P01/) | /`, `/shipper | 필수 | 화주 업무 홈, 운송 의뢰/상태/창고/판매 업무 진입 | 완료 |
| [ShipperApp-P01-1](ShipperApp-P01-1/) | /shipper/settings/profile | 보조 | 화주 프로필과 운영 프로필 설정 | 완료 |
| [ShipperApp-P01-2](ShipperApp-P01-2/) | /shipper/settings/views | 보조 | 화주 앱 메뉴/화면 노출 설정 | 완료 |
| [ShipperApp-P01-3](ShipperApp-P01-3/) | /shipper/public-cargo | 확장 | 공개 화물 또는 공개 의뢰 확인 | 완료 |
| [ShipperApp-P01-4](ShipperApp-P01-4/) | /shipper/exploration/inbox | 확장 | 탐색/제안성 업무 수신함 | 완료 |
| [ShipperApp-P02](ShipperApp-P02/) | /shipper/request | 필수 | 운송 의뢰 작성 | 완료 |
| [ShipperApp-P02-1](ShipperApp-P02-1/) | /shipper/request/bulk | 보조 | 운송 의뢰 대량 등록 | 완료 |
| [ShipperApp-P02-2](ShipperApp-P02-2/) | /dispatch/address-form | 보조 | 배차 주소 입력/검증 폼 | 완료 |
| [ShipperApp-P03](ShipperApp-P03/) | /shipper/request/{RequestId} | 필수 | 의뢰 상세, 결제/배차/상차/하차/정산 타임라인 | 완료 |
| [ShipperApp-P04](ShipperApp-P04/) | /shipper/inbound/dashboard | 확장 | 화주 입고 업무 대시보드 | 완료 |
| [ShipperApp-P04-1](ShipperApp-P04-1/) | /shipper/inbound/requests | 확장 | 입고 요청 목록과 처리 | 완료 |
| [ShipperApp-P05](ShipperApp-P05/) | /shipper/warehouse/workspace | 확장 | 화주 관점 창고 업무 허브 | 완료 |
| [ShipperApp-P05-1](ShipperApp-P05-1/) | /shipper/warehouse/inventory | 확장 | 창고 재고 조회 | 완료 |
| [ShipperApp-P05-2](ShipperApp-P05-2/) | /shipper/warehouse/scan | 확장 | 창고 스캔 작업 | 완료 |
| [ShipperApp-P05-3](ShipperApp-P05-3/) | /shipper/warehouse/work/{ProcessCode} | 확장 | 창고 프로세스별 작업 시작 | 완료 |
| [ShipperApp-P06](ShipperApp-P06/) | /shipper/sales/channels | 확장 | 판매채널 연결/관리 | 완료 |
| [ShipperApp-P06-1](ShipperApp-P06-1/) | /shipper/sales/listings | 확장 | 상품 등록/리스팅 | 완료 |
| [ShipperApp-P06-2](ShipperApp-P06-2/) | /shipper/sales/orders | 확장 | 판매 주문 이행/출고 연결 | 완료 |
| [ShipperApp-P07](ShipperApp-P07/) | /shipper/international/fcl-lcl | 확장 | FCL/LCL 해외 물류 계획 | 완료 |
| [ShipperApp-P07-1](ShipperApp-P07-1/) | /shipper/customs/hs-reviews | 확장 | HS 코드/통관 검토 | 완료 |
| [ShipperApp-P08](ShipperApp-P08/) | /shipper/reconsignment/orders | 확장 | 재위탁/재운송 주문 | 완료 |
| [ShipperApp-P90](ShipperApp-P90/) | /weather | 시스템 | 템플릿/샘플성 날씨 화면 | 완료 |
| [ShipperApp-P91](ShipperApp-P91/) | /counter | 시스템 | 템플릿/샘플성 카운터 화면 | 완료 |
| [ShipperApp-P99](ShipperApp-P99/) | /not-found | 시스템 | 미발견 페이지 | 완료 |