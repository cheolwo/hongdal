# Hongdal

Hongdal은 .NET 10 기반의 물류/배차 도메인 솔루션이다.

## 프로젝트 요약

- 관리자, 기사, 화주 역할을 기준으로 기능을 나눈다.
- 화주 결제는 Toss Payments 승인 이후에만 배차 대기 데이터를 생성한다.
- 기사 관련 기능은 업무 흐름에 맞춰 분리해서 관리한다.
-  `Hongdal.Contracts` 프로젝트에서 관리한다.

## 주요 프로젝트

- `Hongdal` - 백엔드 API와 도메인, 데이터, 서비스
- `HongdalAdmin` - 관리자 앱
- `DriverApp` - 기사 앱
- `ShipperApp` - 화주 앱
- `Hongdal.Contracts` - 공유 DTO/계약

## 참고 문서

- `Hongdal/tosspayments-integration-guide.md` - Toss 결제 연동 상세 문서

## 기사 흐름

```mermaid
flowchart TD
	A[기사 홈] --> B[운행시작]
	A --> C[추천콜 목록]
	C --> D[운송의뢰 상세]
	D --> E{수락/거절}
	E -->|수락| F[진행중 운송]
	E -->|거절| C
	F --> G[상차지 도착]
	G --> H[상차 완료]
	H --> I[하차지 도착]
	I --> J[운송 완료]
	A --> K[예약]
	A --> L[내정보]
	L --> M[정산]
	L --> N[알림설정]
```

## 프로젝트 구조

```mermaid
flowchart TB
	subgraph Solution[Hongdal 솔루션]
		direction TB

		subgraph Core[백엔드]
			Hongdal[Hongdal\nASP.NET Core API]
			Contracts[Hongdal.Contracts\n공유 DTO / 계약]
			Hongdal --> Contracts
		end

		subgraph Apps[클라이언트]
			Admin[HongdalAdmin\nBlazor 관리자]
			Driver[DriverApp\n기사 앱]
			Shipper[ShipperApp\n화주 앱]
		end

		subgraph Infra[주요 인프라/기능]
			Db[(MySQL / EF Core)]
			Redis[(Redis)]
			Mongo[(MongoDB)]
			Hub[SignalR Hub]
			Services[Services / Infrastructure\n비즈니스 로직 / 저장소 / 보안]
		end

		Admin -->|API 호출| Hongdal
		Driver -->|API 호출| Hongdal
		Shipper -->|API 호출| Hongdal

		Hongdal --> Services
		Hongdal --> Db
		Hongdal --> Redis
		Hongdal --> Mongo
		Hongdal --> Hub
		Services --> Db
		Services --> Redis
		Services --> Mongo
	end
```

## 기사 홈 상태 흐름

```mermaid
flowchart TD
	A[기사 홈 진입] --> B[홈 API 호출<br/>GET /api/v1/driver/home]
	B --> C[응답 수신]

	C --> D{진행중 운송 있음?}
	D -->|예| E[중앙 CTA<br/>진행중 운송 보기]
	D -->|아니오| F{운행중인가?}

	F -->|아니오| G[중앙 CTA<br/>운행시작]
	F -->|예| H{추천콜 있음?}

	H -->|예| I[중앙 CTA<br/>추천콜 보기]
	H -->|아니오| J{오늘 예약 임박?}

	J -->|예| K[중앙 CTA<br/>예약 확인]
	J -->|아니오| L{알림 오류?}

	L -->|예| M[중앙 CTA<br/>알림 설정 확인]
	L -->|아니오| N{정산 확인 필요?}

	N -->|예| O[중앙 CTA<br/>정산 확인]
	N -->|아니오| P[중앙 CTA<br/>추천콜 새로고침]

	E --> Q[오늘 할 일 카드 목록]
	G --> Q
	I --> Q
	K --> Q
	M --> Q
	O --> Q
	P --> Q

	Q --> R[진행중 운송 카드]
	Q --> S[추천콜 카드]
	Q --> T[오늘 예약 카드]
	Q --> U[알림 확인 카드]
	Q --> V[정산 확인 카드]

	R --> W[진행중 운송 화면]
	S --> X[추천콜 목록 화면]
	T --> Y[예약 화면]
	U --> Z[알림설정 화면]
	V --> AA[정산 화면]

	W --> AB[상차지 도착]
	AB --> AC[상차 완료]
	AC --> AD[하차지 도착]
	AD --> AE[운송 완료]
```

## 메모

이 문서는 프로젝트를 파악하기 위한 요약용 문서다.
상세한 흐름이나 구조는 별도 문서에서 관리한다.
