# Blazor/Maui 공통화 1차 적용 (2026-07-02)

## 목표
- Blazor/MudBlazor 중심 UI 구조를 유지하면서 공통 UI 자산을 재사용 가능한 형태로 분리한다.
- `HongdalAdmin`(Blazor Web)과 `RestaurantDeskApp`(MAUI Blazor Hybrid)에서 동일한 루트 Provider/테마 구성을 사용한다.
- 이후 2차 리팩터링(공통 페이지/공통 서비스 확장)을 위한 기반을 만든다.

## 이번 단계에서 적용한 항목

### 1) 공통 프로젝트 추가
- `Hongdal.BackOffice.Ui` (Razor Class Library)
  - 공통 MudBlazor Provider 컴포넌트 추가
	- `Components/BackOfficeProviders.razor`
  - 공통 MudBlazor 테마 추가
	- `Theme/BackOfficeTheme.cs`
  - 공통 배차업무유형 칩 컴포넌트 추가
	- `Components/Dispatch/DispatchBusinessTypeChip.razor`
- `Hongdal.BackOffice.Client` (Class Library)
  - 공통 API 옵션 뼈대
	- `Configuration/BackOfficeApiOptions.cs`
  - 공통 진단 클래스 뼈대
	- `Diagnostics/ApiClientDiagnostics.cs`

### 2) 앱별 연결
- `HongdalAdmin`
  - 공통 프로젝트 참조 추가
	- `Hongdal.BackOffice.Ui`
	- `Hongdal.BackOffice.Client`
  - 루트 Provider를 공통 컴포넌트로 교체
	- `Components/App.razor`
	  - 기존 `MudThemeProvider/MudPopoverProvider/MudDialogProvider/MudSnackbarProvider` 제거
	  - `BackOfficeProviders` 적용
  - 공통 UI 네임스페이스 import 추가
	- `Components/_Imports.razor`

- `RestaurantDeskApp`
  - 공통 프로젝트 참조 추가
	- `Hongdal.BackOffice.Ui`
	- `Hongdal.BackOffice.Client`
  - 루트 Provider를 공통 컴포넌트로 교체
	- `Components/Routes.razor`
	  - 기존 Mud Provider 4종 제거
	  - `BackOfficeProviders` 적용
  - 공통 UI 네임스페이스 import 추가
	- `Components/_Imports.razor`

## 검증
- 솔루션 전체 빌드 성공 확인.

## 현재 상태 요약
- `RestaurantDeskApp`은 기존처럼 MAUI Blazor Hybrid 유지.
- `HongdalAdmin`은 기존처럼 Blazor Web 유지.
- 두 앱의 루트 MudBlazor Provider/테마 구성이 공통 컴포넌트로 통합됨.
- 공통 클라이언트/진단 계층은 뼈대 수준으로 준비됨.

## 다음(2차) 권장 작업
1. `HongdalAdmin`와 `RestaurantDeskApp` 페이지 중 중복 UI를 `Hongdal.BackOffice.Ui`로 단계적 이관
2. 공통 메뉴/레이아웃(네비, 헤더, 상태 배지) 통합
3. `Hongdal.BackOffice.Client`에 공통 HttpClient 팩토리/인증 핸들러 추가
4. 음식점 주문 실시간 알림(SignalR) 컴포넌트와 `I주문알림Service` 연결 확대
