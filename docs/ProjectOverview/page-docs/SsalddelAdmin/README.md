# SsalddelAdmin 화면 문서

[전체 화면 문서](../README.md) / [앱 전체 카탈로그](../../app-page-catalog.md)

이 문서는 SsalddelAdmin 에 속한 화면별 README를 모은 색인입니다. 각 화면 문서는 캡처 이미지, 화면 책임, 사용자와 참여자, API/서버 연계, 보안 점검을 별도로 설명합니다.

| 페이지 ID / 제목 | 라우트 | 분류 | 화면 책임 | 캡처 |
| --- | --- | --- | --- | --- |
| [SsalddelAdmin-P00 - 관리자 홈](SsalddelAdmin-P00/) | / | 시스템 | 관리자 홈 | 완료 |
| [SsalddelAdmin-P00-1 - 관리자 로그인](SsalddelAdmin-P00-1/) | /login | 시스템 | 관리자 로그인 | 완료 |
| [SsalddelAdmin-P00-2 - 오류 화면](SsalddelAdmin-P00-2/) | /Error | 시스템 | 오류 화면 | 완료 |
| [SsalddelAdmin-P16 - 운영 대시보드](SsalddelAdmin-P16/) | /dashboard | 필수 | 운영 대시보드 | 완료 |
| [SsalddelAdmin-P17 - 의뢰 목록](SsalddelAdmin-P17/) | /requests | 필수 | 의뢰 목록 | 완료 |
| [SsalddelAdmin-P18 - 의뢰 상세](SsalddelAdmin-P18/) | /requests/{RequestId} | 필수 | 의뢰 상세 | 완료 |
| [SsalddelAdmin-P19 - 배차대기/추천 잠금 상태](SsalddelAdmin-P19/) | /dispatch/wait | 필수 | 배차대기/추천 잠금 상태 | 완료 |
| [SsalddelAdmin-P20 - 운행 중 기사 현황](SsalddelAdmin-P20/) | /drivers/operating | 필수 | 운행 중 기사 현황 | 완료 |
| [SsalddelAdmin-P21 - 운송 목록](SsalddelAdmin-P21/) | /transports | 필수 | 운송 목록 | 완료 |
| [SsalddelAdmin-P22 - 운송 상세 원장](SsalddelAdmin-P22/) | /transports/{RequestId} | 필수 | 운송 상세 원장 | 완료 |
| [SsalddelAdmin-P22-1 - 운송 이벤트 감사](SsalddelAdmin-P22-1/) | /transports/{RequestId}/events | 필수 | 운송 이벤트 감사 | 완료 |
| [SsalddelAdmin-P22-2 - 운송 증빙/POD](SsalddelAdmin-P22-2/) | /transports/{RequestId}/proofs | 필수 | 운송 증빙/POD | 완료 |
| [SsalddelAdmin-P22-3 - 운송 정산 상세](SsalddelAdmin-P22-3/) | /transports/{RequestId}/settlement | 필수 | 운송 정산 상세 | 완료 |
| [SsalddelAdmin-P23 - 관리자 활동 로그](SsalddelAdmin-P23/) | /activity-logs | 운영 | 관리자 활동 로그 | 완료 |
| [SsalddelAdmin-P24 - 화면/기능 노출 정책](SsalddelAdmin-P24/) | /view-policies | 운영 | 화면/기능 노출 정책 | 완료 |
| [SsalddelAdmin-P25 - 공통 콘텐츠 관리](SsalddelAdmin-P25/) | /common-contents | 운영 | 공통 콘텐츠 관리 | 완료 |
| [SsalddelAdmin-P26 - 결제 목록](SsalddelAdmin-P26/) | /payments | 필수 | 결제 목록 | 완료 |
| [SsalddelAdmin-P26-1 - 정산 목록](SsalddelAdmin-P26-1/) | /settlements | 필수 | 정산 목록 | 완료 |
| [SsalddelAdmin-P27 - 문서 목록](SsalddelAdmin-P27/) | /documents | 필수 | 문서 목록 | 완료 |
| [SsalddelAdmin-P27-1 - 문서 업로드](SsalddelAdmin-P27-1/) | /documents/upload | 필수 | 문서 업로드 | 완료 |
| [SsalddelAdmin-P27-2 - 문서 정책 목록](SsalddelAdmin-P27-2/) | /documents/policies | 필수 | 문서 정책 목록 | 완료 |
| [SsalddelAdmin-P27-3 - 문서 정책 상세](SsalddelAdmin-P27-3/) | /documents/policies/{DocumentCode} | 필수 | 문서 정책 상세 | 완료 |
| [SsalddelAdmin-P27-4 - 문서 조회 로그](SsalddelAdmin-P27-4/) | /documents/logs | 필수 | 문서 조회 로그 | 완료 |
| [SsalddelAdmin-P27-5 - 파일/POD 관리](SsalddelAdmin-P27-5/) | /files/pod | 필수 | 파일/POD 관리 | 완료 |
| [SsalddelAdmin-P28 - 공개 화물/화물 운영 화면](SsalddelAdmin-P28/) | /cargo | 운영 | 공개 화물/화물 운영 화면 | 완료 |
| [SsalddelAdmin-P29 - HS 코드/통관 운영](SsalddelAdmin-P29/) | /customs/hs-codes | 운영 | HS 코드/통관 운영 | 완료 |
| [SsalddelAdmin-P30 - 음식 주문/배달 운영](SsalddelAdmin-P30/) | /food/operations | 운영 | 음식 주문/배달 운영 | 완료 |
| [SsalddelAdmin-P30-1 - 음식점 검색 정책](SsalddelAdmin-P30-1/) | /restaurant-search-policy | 운영 | 음식점 검색 정책 | 완료 |
| [SsalddelAdmin-P31 - 탐색 캠페인 운영](SsalddelAdmin-P31/) | /exploration/campaigns | 운영 | 탐색 캠페인 운영 | 완료 |
| [SsalddelAdmin-P32 - 기사 목록/관리](SsalddelAdmin-P32/) | /drivers | 운영 | 기사 목록/관리 | 완료 |
| [SsalddelAdmin-P32-1 - 차량 관리](SsalddelAdmin-P32-1/) | /vehicle-management | 운영 | 차량 관리 | 완료 |
| [SsalddelAdmin-P33 - 파트너 관리](SsalddelAdmin-P33/) | /partners | 운영 | 파트너 관리 | 완료 |
| [SsalddelAdmin-P34 - 수익/요율 정책](SsalddelAdmin-P34/) | /revenue-policies | 운영 | 수익/요율 정책 | 완료 |
| [SsalddelAdmin-P35 - 보조 기능 설정](SsalddelAdmin-P35/) | /auxiliary-feature-settings | 운영 | 보조 기능 설정 | 완료 |
| [SsalddelAdmin-P36 - 연락처 통합 검색](SsalddelAdmin-P36/) | /contact-search | 운영 | 전화번호 뒤 8자리 기준 인물/역할 통합 조회 | 완료 |
| [SsalddelAdmin-P37 - 국내화물 AI 배차 검토](SsalddelAdmin-P37/) | /dispatch/ai-review | 운영 | 국내화물 AI 배차 검토 | 완료 |
| [SsalddelAdmin-P38 - 음식배달 AI 배차 검토](SsalddelAdmin-P38/) | /dispatch/food-ai-review | 운영 | 음식배달 AI 배차 검토 | 완료 |
| [SsalddelAdmin-P39 - 배차 AI 판단 사례](SsalddelAdmin-P39/) | /dispatch-ai-judgment-cases | 운영 | 배차 AI 판단 사례 | 완료 |
| [SsalddelAdmin-P40 - 개발용 Fake PG/정산 콘솔](SsalddelAdmin-P40/) | /development/fake-payment-settlement | 개발 | 결제 보증과 기사 정산 상태 전이 시뮬레이션 | 캡처 대기 |
| [SsalddelAdmin-P90 - 템플릿/샘플성 날씨 화면](SsalddelAdmin-P90/) | /weather | 시스템 | 템플릿/샘플성 날씨 화면 | 완료 |
| [SsalddelAdmin-P91 - 템플릿/샘플성 카운터 화면](SsalddelAdmin-P91/) | /counter | 시스템 | 템플릿/샘플성 카운터 화면 | 완료 |
| [SsalddelAdmin-P99 - 미발견 페이지](SsalddelAdmin-P99/) | /not-found | 시스템 | 미발견 페이지 | 완료 |
