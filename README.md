# Hongdal

Hongdal은 .NET 10 기반의 물류/배차 도메인 솔루션이다.

## 현재 정리된 방향

- 관리자, 기사, 화주 역할을 기준으로 컨트롤러를 분리한다.
- 기사 업무 흐름은 `01_Work → 02_Recommendation → 03_Action → 04_Progress → 05_Settings → 06_Settlement → 07_Notification` 순서로 정리한다.
- 화주 결제는 Toss Payments 승인 확인이 끝난 뒤에만 `배차대기`를 생성하는 방식으로 유지한다.
- 공유 계약은 `Hongdal.Contracts` 프로젝트에서 분리해 관리한다.

## DB 구조도

```mermaid
erDiagram
	업체 ||--o{ 화주운송의뢰 : "화주Id 논리참조"
	업체 ||--o{ 결제 : "화주Id 논리참조"

	화주운송의뢰 ||--|| 화물요구조건 : "의뢰Id 공통키"
	화주운송의뢰 ||--o{ 운임구성 : "의뢰Id 참조"
	화주운송의뢰 ||--o{ 결제 : "의뢰Id 참조"
	화주운송의뢰 ||--o| 배차대기 : "의뢰Id 참조"
	화주운송의뢰 ||--o{ 운송이벤트 : "의뢰Id 참조"

	용달기사 ||--o{ 기사근무 : "기사Id 참조"
	용달기사 ||--o{ 기사위치기록 : "기사Id 참조"
	용달기사 ||--o{ 기사월정산 : "기사Id 참조"
	용달기사 ||--o{ 기사배차 : "기사Id 참조"

	배달기사 ||--o{ 기사근무 : "기사Id 참조"
	배달기사 ||--o{ 기사위치기록 : "기사Id 참조"
	배달기사 ||--o{ 기사월정산 : "기사Id 참조"

	차량제원 ||--o{ 운임구성 : "차량별 운임 산정 기준"
	차량제원 ||--o{ 차량단가 : "차량별 단가 기준"

	업체 {
		long Id
		string 업체명
		string 상태
	}

	화주운송의뢰 {
		long Id
		string 의뢰Id
		string 화주Id
		long 운임구성Id
		string 결제상태
		string 배차상태
		string 상태
	}

	화물요구조건 {
		string 의뢰Id
		int 화물길이Mm
		int 화물폭Mm
		int 화물높이Mm
		int 화물무게Kg
	}

	운임구성 {
		long Id
		string 의뢰Id
		decimal 기본운임
		decimal 최종운임
	}

	결제 {
		long Id
		string 결제Id
		string 의뢰Id
		string 화주Id
		int 결제금액
		string 결제상태
	}

	배차대기 {
		long Id
		string 의뢰Id
		string 화주Id
		string 상태
	}

	운송이벤트 {
		long Id
		string 의뢰Id
		string 이벤트타입
	}

	용달기사 {
		long Id
		string 기사Id
		string 기사명
		string 운행상태
	}

	배달기사 {
		long Id
		string 기사Id
		string 기사명
		string 운행상태
	}

	기사근무 {
		long Id
		string 기사Id
		string 시작모드
		DateTime 시작시각
	}

	기사위치기록 {
		long Id
		string 기사Id
		decimal 위도
		decimal 경도
	}

	기사월정산 {
		long Id
		string 기사Id
		int 년도
		int 월
		decimal 이용료
	}

	기사배차 {
		long Id
		long 배차Id
		long 용달기사_id
		long 기사Id
		string 상태
	}

	차량제원 {
		string 차량코드
		string 차량명
		int 최대적재중량Kg
	}

	차량단가 {
		long Id
		string 차량종류
		decimal 기본운임
		decimal Km당단가
	}
```

## 컨트롤러별 API 흐름도

```mermaid
sequenceDiagram
	participant Shipper as 화주/클라이언트
	participant ShipperReq as 화주운송의뢰Controller
	participant Payment as 화주결제Controller
	participant Db as HongdalContext
	participant Geo as GeocodingService
	participant Toss as TossPaymentsService

	Note over Shipper,Payment: 1) 화주 운송의뢰 + 결제
	Shipper->>ShipperReq: GET /api/v1/shipper/requests
	ShipperReq->>ShipperReq: page/pageSize 보정
	ShipperReq->>Db: 화주/상태 조건 적용
	ShipperReq->>Db: 의뢰 목록 조회
	Db-->>ShipperReq: 목록 반환
	ShipperReq->>ShipperReq: DTO 매핑
	ShipperReq-->>Shipper: 목록 응답

	Shipper->>ShipperReq: GET /api/v1/shipper/requests/public
	ShipperReq->>ShipperReq: page/pageSize 보정
	ShipperReq->>Db: 공개 화물 요약 조회
	Db-->>ShipperReq: 요약 목록 반환
	ShipperReq->>ShipperReq: 공개 응답 DTO 변환
	ShipperReq-->>Shipper: 공개 목록 응답

	Shipper->>ShipperReq: POST /api/v1/shipper/requests
	ShipperReq->>ShipperReq: 필수값/중복/결제상태 검증
	ShipperReq->>Geo: 좌표 해석
	Geo-->>ShipperReq: 좌표 반환
	ShipperReq->>Db: 화주운송의뢰 저장
	ShipperReq->>Db: 화물요구조건 Upsert
	Db-->>ShipperReq: 저장 완료
	ShipperReq-->>Shipper: 생성 응답

	Shipper->>ShipperReq: GET /api/v1/shipper/requests/{requestId}
	ShipperReq->>ShipperReq: requestId 검증
	ShipperReq->>Db: 의뢰 단건 조회
	Db-->>ShipperReq: 의뢰 반환
	ShipperReq->>ShipperReq: 응답 DTO 변환
	ShipperReq-->>Shipper: 단건 응답

	Shipper->>ShipperReq: PUT /api/v1/shipper/requests/{requestId}
	ShipperReq->>ShipperReq: requestId/요청 body 검증
	ShipperReq->>Db: 의뢰 조회 및 수정
	ShipperReq->>Db: 화물요구조건 Upsert
	ShipperReq->>Db: 수정 시각 갱신
	Db-->>ShipperReq: 수정 완료
	ShipperReq-->>Shipper: 수정 응답

	Shipper->>ShipperReq: DELETE /api/v1/shipper/requests/{requestId}
	ShipperReq->>ShipperReq: requestId 검증
	ShipperReq->>Db: 의뢰 삭제
	Db-->>ShipperReq: 삭제 완료
	ShipperReq-->>Shipper: 204 No Content

	Shipper->>Payment: GET /api/v1/payments
	Payment->>Db: 결제 목록 조회
	Db-->>Payment: 목록 반환
	Payment->>Payment: 응답 DTO 매핑
	Payment-->>Shipper: 결제 목록 응답

	Shipper->>Payment: GET /api/v1/payments/toss/config
	Payment->>Payment: Toss 설정 읽기
	Payment->>Payment: isConfigured 계산
	Payment-->>Shipper: Toss 환경 응답

	Shipper->>Payment: POST /api/v1/payments/toss/prepare
	Payment->>Payment: request body 검증
	Payment->>Db: 의뢰 조회
	Payment->>Payment: 상차완료/결제상태 검증
	Payment->>Db: 기존 결제대기 조회
	alt 기존 결제대기 존재
		Payment->>Payment: 기존 결제 정보로 응답 구성
	else 신규 결제 생성
		Payment->>Payment: 결제금액 결정
		Payment->>Db: 결제 row 생성
		Payment->>Db: 화주운송의뢰 결제상태=결제대기
		Payment->>Db: 저장 반영
	end
	Db-->>Payment: 준비 완료
	Payment-->>Shipper: 결제 준비 응답

	Shipper->>Payment: POST /api/v1/payments/toss/confirm
	Payment->>Payment: request body 검증
	Payment->>Db: 결제 조회 및 금액 검증
	Payment->>Payment: 이미 결제완료 여부 확인
	Payment->>Toss: confirm 호출
	Toss-->>Payment: 승인 결과
	Payment->>Payment: Toss 응답 성공 여부 확인
	Payment->>Db: 트랜잭션 시작
	Payment->>Db: 결제 완료 저장
	Payment->>Db: 화주운송의뢰 결제완료 / 배차상태=매칭중
	Payment->>Db: 배차대기 생성 여부 확인
	Payment->>Db: 배차대기 없으면 생성
	Db-->>Payment: 승인 저장 완료
	Payment-->>Shipper: 결제 승인 응답
```

```mermaid
sequenceDiagram
	participant Driver as 기사
	participant Rec as 기사배차추천Controller
	participant Dispatch as 배차추천Service
	participant National as NationalDispatchRequestService

	Note over Driver,Rec: 2) 기사 배차 추천
	Driver->>Rec: GET /api/v1/driver/recommendations
	Rec->>Rec: 기사 인증 확인
	Rec->>Rec: 기사Id 추출
	Rec->>Dispatch: 추천 조회
	Dispatch->>Dispatch: 운행상태/추천상태 판단
	Dispatch-->>Rec: 추천 목록
	Rec->>Rec: 응답 정렬/매핑
	Rec-->>Driver: 추천 응답

	Driver->>Rec: GET /api/v1/driver/recommendations/idle
	Rec->>Rec: 기사Id 추출
	Rec->>Dispatch: 비운행중 추천 조회
	Dispatch->>Dispatch: 비운행중 필터 적용
	Dispatch-->>Rec: 비운행중 목록
	Rec-->>Driver: 응답

	Driver->>Rec: GET /api/v1/driver/recommendations/driving
	Rec->>Rec: 기사Id 추출
	Rec->>Dispatch: 운행중 추천 조회
	Dispatch->>Dispatch: 운행중 필터 적용
	Dispatch-->>Rec: 운행중 목록
	Rec-->>Driver: 응답

	Driver->>Rec: GET /api/v1/driver/recommendations/search
	Rec->>Rec: 기사Id 추출
	Rec->>Rec: radiusKm 검증
	Rec->>Dispatch: 위치/반경 기반 검색
	Dispatch->>Dispatch: 거리/권역/적합도 계산
	Dispatch-->>Rec: 검색 결과
	Rec->>Rec: 응답 정렬
	Rec-->>Driver: 검색 응답

	Driver->>Rec: GET /api/v1/driver/recommendations/national
	Rec->>Rec: 기사Id 추출
	Rec->>National: 전국 콜 조회
	National->>National: 전국 의뢰 필터 적용
	National-->>Rec: 전국 의뢰 목록
	Rec->>Rec: 응답 매핑
	Rec-->>Driver: 전국 콜 응답
```

## 컨트롤러 구성

### 관리자

- `관리자대시보드Controller` - 관리자 홈과 전체 현황 진입점
- `배차대기Controller` - 배차 대기 목록과 관리
- `기사운행현황Controller` - 기사 운행 상태 조회
- `배차계획관리Controller` - 배차 계획 수립 및 관리
- `운송이벤트Controller` - 운송 이벤트 이력 관리
- `운송진행관리Controller` - 운송 진행 상태 관리
- `파일POD관리Controller` - 증빙 파일과 POD 관리
- `기사월정산관리Controller` - 기사 월 정산 관리
- `기사관리Controller` - 기사 계정 및 정보 관리
- `업체화주관리Controller` - 업체와 화주 관리
- `운임구성Controller` - 운임 구성 관리
- `차량단가Controller` - 차량 단가 관리

### 공통

- `인증Controller` - 로그인, 인증, 토큰 관련 공통 처리
- `파일업로드Controller` - 파일 업로드 공통 처리

### 기사

- `용달기사Controller` - 기사 프로필과 기본 기사 정보
- `기사운행Controller` - 기사 운행 상태와 운행 흐름
- `용달기사근무Controller` - 기사 근무 시작과 근무 상태
- `기사배차추천Controller` - 배차 추천 목록 조회
- `기사운송의뢰Controller` - 기사에게 전달되는 운송의뢰 조회
- `기사배차액션Controller` - 수락, 거절 등 배차 액션
- `기사예약Controller` - 예약 관련 처리
- `기사설정Controller` - 기사 설정 관리
- `기사정산Controller` - 기사 정산 조회
- `기사알림Controller` - 기사 알림과 통지 처리

### 화주

- `화주운송의뢰Controller` - 화주 운송의뢰 생성과 조회
- `화주결제Controller` - 화주 결제 준비, 승인, 상태 관리
- `수입식품해외제조업소Controller` - 수입식품/해외제조업소 조회

### 기타

- `배달기사월정산Controller` - 배달기사 월 정산

## 주요 프로젝트

- `Hongdal` - 백엔드 API와 도메인, 데이터, 서비스
- `HongdalAdmin` - 관리자 앱
- `DriverApp` - 기사 앱
- `ShipperApp` - 화주 앱
- `Hongdal.Contracts` - 공유 DTO/계약

## 결제 연동 문서

Toss Payments 상세 흐름은 다음 문서를 참고한다.

- `Hongdal/tosspayments-integration-guide.md`

## 메모

- 이 문서는 최근 대화에서 정리된 구조와 흐름을 빠르게 확인하기 위한 간단한 요약 문서다.
- 세부 구현 변경이 생기면 관련 상세 문서도 함께 갱신한다.
