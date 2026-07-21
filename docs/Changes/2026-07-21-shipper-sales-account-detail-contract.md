# 주문자 앱 판매채널 계정 상세조회 계약 복구

## 변경

- 공용 `I판매채널계정읽기Service`가 요구하는 `계정상세조회Async`를 MAUI 주문자 앱의 `ShipperSalesService`에 구현했다.
- 인메모리 저장소의 정확한 계정 ID 조회를 재사용하고, 조회 전에 cancellation을 확인한다.
- 계정이 없으면 공용 API client와 같은 의미로 `null`을 반환한다.

## 원인

판매채널 계정 읽기 계약에 정확한 ID 상세조회가 추가됐지만, Web API client만 갱신되고 MAUI 인메모리 adapter가 누락돼 `SsalddelApp` Windows 빌드가 `CS0535`로 실패했다.

## 화면 변화

화면 없음 — 주문자 앱의 판매채널 계정 상세 화면이 사용하는 읽기 계약과 배포 빌드를 복구한 adapter 수정이다.

## 검증

- clean worktree `SsalddelApp` `net10.0-windows10.0.19041.0` build 경고 0개·오류 0개
- 공용 UI와 WebApp build 경고 0개·오류 0개
