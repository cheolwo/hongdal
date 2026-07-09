# DriverApp 화면 문서

[전체 화면 문서](../README.md) / [앱 전체 카탈로그](../../app-page-catalog.md)

이 문서는 DriverApp 에 속한 화면별 README를 모은 색인입니다. 각 화면 문서는 캡처 이미지, 화면 책임, 사용자와 참여자, API/서버 연계, 보안 점검을 별도로 설명합니다.

| 페이지 ID | 라우트 | 분류 | 화면 책임 | 캡처 |
| --- | --- | --- | --- | --- |
| [DriverApp-P00](DriverApp-P00/) | / | 시스템 | 기사 앱 시작 라우트 리다이렉트 | 완료 |
| [DriverApp-P01](DriverApp-P01/) | /login | 시스템 | 로그인 | 완료 |
| [DriverApp-P02](DriverApp-P02/) | /driver/menu | 보조 | 기사 앱 메뉴 | 완료 |
| [DriverApp-P02-1](DriverApp-P02-1/) | /driver/settings/views | 보조 | 기사 앱 화면 노출 설정 | 완료 |
| [DriverApp-P03](DriverApp-P03/) | /driver/reservations | 확장 | 예약 운송 또는 예약 업무 | 완료 |
| [DriverApp-P04](DriverApp-P04/) | /driver/exploration/campaigns | 확장 | 탐색 캠페인/추천 확장 | 완료 |
| [DriverApp-P05](DriverApp-P05/) | /driver/transports/history | 보조 | 운송/배달 이력 조회 | 완료 |
| [DriverApp-P06](DriverApp-P06/) | /driver/work/start | 필수 | 운행 시작, 위치 송신 시작 | 완료 |
| [DriverApp-P06-1](DriverApp-P06-1/) | /driver/work/settings | 보조 | 운행 조건과 선호 설정 | 완료 |
| [DriverApp-P07](DriverApp-P07/) | /driver/home | 필수 | 지도 홈, 추천 배너, 현재 운송 진입 | 완료 |
| [DriverApp-P07-1](DriverApp-P07-1/) | /driver/home/summary | 보조 | 기사 업무 허브/요약 | 완료 |
| [DriverApp-P08](DriverApp-P08/) | /driver/recommendations | 필수 | 추천 목록 | 완료 |
| [DriverApp-P09](DriverApp-P09/) | /driver/recommendations/{의뢰Id} | 필수 | 추천 상세와 판단 정보 | 완료 |
| [DriverApp-P10](DriverApp-P10/) | /driver/recommendations/{의뢰Id}/decision | 필수 | 추천 수락/거절/보류 처리 | 완료 |
| [DriverApp-P11](DriverApp-P11/) | /driver/transports/current | 필수 | 진행 중 운송과 다음 행동 | 완료 |
| [DriverApp-P12](DriverApp-P12/) | /driver/transports/{운송Id:long}/pickup | 필수 | 상차 증빙, 상차 예외 | 완료 |
| [DriverApp-P13](DriverApp-P13/) | /driver/transports/{운송Id:long}/dropoff | 필수 | 하차 증빙, POD, 하차 예외 | 완료 |
| [DriverApp-P14](DriverApp-P14/) | /driver/settlements/current-month | 필수 | 월정산 확인 | 완료 |
| [DriverApp-P14-1](DriverApp-P14-1/) | /driver/settlements/info | 보조 | 이용료/정산 정책 안내 | 완료 |
| [DriverApp-P14-2](DriverApp-P14-2/) | /driver/account/bank | 보조 | 기사 정산 계좌 정보 | 완료 |
| [DriverApp-P15](DriverApp-P15/) | /driver/notifications | 필수 | 알림함 | 완료 |
| [DriverApp-P15-1](DriverApp-P15-1/) | /driver/notifications/settings | 보조 | 알림 수신 설정 | 완료 |
| [DriverApp-P15-2](DriverApp-P15-2/) | /driver/notifications/push | 보조 | 푸시 토큰/권한 설정 | 완료 |