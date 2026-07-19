# Ssalddel 관리자 백오피스 요구사항 정리 (복붙용)

본 문서는 요청하신 10개 관리 기능을 기준으로, **화면/핵심 항목/API 연결/현재 구현상태**를 한 번에 볼 수 있게 정리한 문서입니다.

---

## 1) 대시보드

### 화면 항목
- 오늘 의뢰 수
- 결제대기 수
- 결제완료 수
- 배차대기 수
- 배차확정 수
- 운송중 수
- 완료 수
- 취소/환불 수

### 집계 기준(권장)
- 오늘 의뢰 수: `shipper_requests.created_at`가 오늘인 건수
- 결제대기/결제완료/환불: `shipper_requests.payment_status`
- 배차대기/배차확정: `운송원장.status`와 `운송원장.배차큐단계`
- 운송중/완료: `운송원장.상태` (상태값 체계 통일 필요)
- 취소/환불 수: `결제.payment_status = 환불됨` + 의뢰 상태가 취소인 건(상태값 확장 필요)

### API 후보
- 기존 API 조합 집계 또는 전용 API 추가 필요 (`/api/v1/admin/dashboard` 권장)

### 현재 상태
- 전용 대시보드 API 없음 (신규 개발 필요)

---

## 2) 화주운송의뢰 관리

### 화면 항목
- 의뢰 목록
- 의뢰 상세
- 결제상태 확인
- 배차상태 확인
- 주소/좌표 확인
- 운임 확인
- 취소/환불 처리

### 연결 API
- 목록: `GET /api/v1/shipper/requests`
- 상세: `GET /api/v1/shipper/requests/{requestId}`
- 수정(결제/배차/주소/운임 등): `PUT /api/v1/shipper/requests/{requestId}`
- 삭제: `DELETE /api/v1/shipper/requests/{requestId}`

### 현재 상태
- 목록/상세/수정/삭제 API 존재
- 취소/환불 전용 플로우는 별도 정책/API 정리 필요

---

## 3) 결제 관리

### 화면 항목
- 결제대기
- 결제완료
- 결제실패
- 환불됨
- Toss 응답 JSON 확인
- 의뢰Id 연결 확인

### 연결 API
- 결제 준비: `POST /api/v1/payments/toss/prepare`
- 결제 승인: `POST /api/v1/payments/toss/confirm`

### 데이터 확인 테이블
- `결제` (payment_status, toss_response_json, request_id)

### 현재 상태
- prepare/confirm API 존재
- 결제 목록/상태 필터 조회 API 부재 (신규 API 필요)
- `결제실패` 상태값은 상태값 상수에 명시되어 있지 않음 (정의 필요)

---

## 4) 배차대기 관리

### 화면 항목
- 결제완료 후 배차대기 진입한 콜 목록
- 픽업지/하차지
- 현재 상태
- 수동 배차
- 배차대기 제거/보류

### 연결 API
- 목록: `GET /api/v1/dispatch/wait`
- 단건: `GET /api/v1/dispatch/wait/{id}`
- 생성: `POST /api/v1/dispatch/wait`
- 수정: `PUT /api/v1/dispatch/wait/{id}`
- 삭제: `DELETE /api/v1/dispatch/wait/{id}`
- 기사 배차확정(기사앱): `POST /api/v1/drivers/{driverId}/dispatches/confirm`

### 현재 상태
- CRUD API 존재
- "보류" 상태는 상태값에 미정의 (상태값 확장 필요)

---

## 5) 기사 관리

### 화면 항목
- 기사 목록
- 운행상태
- 현재 위치
- 차량종류
- 주 활동지역
- 기사별 배차 내역
- 기사 월정산

### 연결 API
- 현재 운행 기사: `GET /api/v1/admin/drivers/operating`
  - 쿼리: `운행상태`, `기사명검색어`, `활동지역검색어`
- 기사 근무 시작/예약/조회: `POST /api/v1/drivers/{driverId}/shifts/start`, `POST /api/v1/drivers/{driverId}/shifts/reserve`, `GET /api/v1/drivers/{driverId}/shifts/{id}`
- 기사 월정산(당월): `GET /api/v1/drivers/{driverId}/monthly-settlements/current`

### 현재 상태
- 운행현황 조회 API 있음
- 전체 기사 목록/기사별 배차내역 전용 관리자 API는 부족 (신규 API 권장)

---

## 6) 기사월정산 관리

### 화면 항목
- 기사별 월 배차건수
- 이용료
- 월 5,000원 상한 적용 여부
- 결제완료 여부

### 연결 API
- 당월 조회: `GET /api/v1/drivers/{driverId}/monthly-settlements/current`
- 월 결제완료 처리: `POST /api/v1/drivers/{driverId}/monthly-settlements/{year}/{month}/mark-paid`

### 현재 상태
- 기사 본인 기준 API는 존재
- 관리자 일괄 조회/관리 API는 별도 필요
- 상한(월 5,000원)은 서비스 정책으로 반영되어 있으나 관리자 확인용 필드/설명 컬럼 추가 검토 권장

---

## 7) 운송 진행 관리

### 화면 항목
- 상차대기
- 상차완료
- 운송중
- 하차완료
- 인수완료
- 운송이벤트 로그

### 연결 API
- 운송이벤트 목록/단건/생성/수정/삭제: `GET/POST/PUT/DELETE /api/v1/transport-events...`

### 현재 상태
- 이벤트 CRUD API 존재
- 운송 진행 상태 표준값(상차대기~인수완료)은 `운송원장.상태`와 `운송이벤트.event_type` 표준화 필요

---

## 8) 운임/차량단가 관리

### 화면 항목
- 차량별 기본운임
- Km당단가
- 최소운임
- 야간/우천 할증
- 운임구성 확인

### 연결 API
- 차량단가 CRUD: `/api/v1/vehicle-rates`
- 운임구성 CRUD: `/api/v1/fare-configurations`

### 현재 상태
- 관리자 CRUD API 존재

---

## 9) 업체/화주 관리

### 화면 항목
- 업체 목록
- 화주 계정
- 연락처
- 사업자정보
- 거래 상태

### 연결 데이터
- 업체: `업체` 테이블
- 화주: `shipper_requests.shipper_id` + Identity 계정 연동 필요

### 현재 상태
- 업체/화주 관리자 전용 컨트롤러 미구현 (신규 API 필요)

---

## 10) 파일/POD 관리

### 화면 항목
- 배송완료 사진
- 인수증
- 첨부파일
- 업로드 상태

### 연결 API
- 파일 업로드: `POST /api/v1/files/upload`

### 현재 상태
- 업로드 API는 존재
- 파일 메타 목록/조회/삭제, POD 타입 분류는 미구현

---

# 관리자 앱 화면 라우트 제안 (Blazor Server)

- `/dashboard` : 대시보드
- `/requests` : 화주운송의뢰 목록
- `/requests/{requestId}` : 의뢰 상세
- `/payments` : 결제 관리
- `/dispatch/wait` : 배차대기 관리
- `/drivers` : 기사 관리
- `/drivers/operating` : 기사운행현황 (구현됨)
- `/settlements` : 기사월정산 관리
- `/transports` : 운송 진행 관리
- `/rates/vehicle` : 차량단가 관리
- `/rates/fare-configurations` : 운임구성 관리
- `/partners` : 업체/화주 관리
- `/files/pod` : 파일/POD 관리

---

# 우선순위 제안

## P1 (바로 개발)
1. 대시보드 API + 화면
2. 의뢰 목록/상세 관리 화면
3. 결제 관리 목록 API + 화면
4. 배차대기 관리 화면

## P2
1. 기사 전체관리(목록/배차내역)
2. 기사월정산 관리자 화면
3. 운송 진행 표준 상태 UI

## P3
1. 업체/화주 관리
2. 파일/POD 메타 관리

---

# 상태값/도메인 보강 필요사항

1. 결제상태에 `결제실패` 추가 여부 결정
2. 배차대기 상태에 `보류` 추가 여부 결정
3. 운송 진행 상태(상차대기/상차완료/운송중/하차완료/인수완료) 표준 상수화
4. 취소 상태(의뢰)와 환불 상태(결제) 매핑 규칙 정의
