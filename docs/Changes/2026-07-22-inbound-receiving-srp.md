# 창고 입고상품 수령 화면 단일책임 분리

커밋 상태: 실제 렌더링 재확인 대기

## 변경 기록

| 변경 축 | 화면 변경 여부 | 결과 |
| --- | --- | --- |
| 조립 shell | 구조 변경 | `SsalddelInboundReceivingWorkspace.razor`를 438줄에서 43줄로 줄이고 경로 parameter와 업무 event 조립만 유지 |
| 페이지 상태 | 화면 유지 | loading, 창고 조회 error·retry와 빈 창고 상태를 전용 컴포넌트로 분리 |
| 정확 검색·후보 | 화면 유지 | 창고·바코드 입력과 서버가 반환한 exact SKU 후보 표시를 각각 분리 |
| 현장 반입 요청 | 반응형 보강 | 입력·안내 동의·저장 event를 분리하고 처리 중 입력 잠금과 44px 동작 영역을 적용 |
| 저장 결과 | 화면 유지 | 선택·저장 뒤 같은 입고 ID를 재조회한 결과와 후속 작업 보드 이동만 표시 |
| 서버 경계 | 간접 확인 | 권한 밖 ID 404, 사용자별 멱등 요청 ID, 다른 내용 재시도 거부와 재고 미생성 테스트 유지 |

## 책임과 실행 경계

- root component는 화면 영역과 event만 조립하고 입력 setter, 반복 렌더링과 표시 형식을 소유하지 않는다.
- `입고상품수령PageViewModel`은 창고 선택, exact search, 현장 요청 저장과 같은 ID 재조회 순서만 조정한다.
- 서버는 접근 가능한 창고, 현재 상태, exact SKU, 안내 동의·버전과 멱등 요청 ID를 다시 검증한다.
- 저장 결과는 `입고예정` 원장 한 건이며 입고 완료, 검수, 적재, 재고, 계약, 운송, 결제와 정산을 실행하지 않는다.
- 화면과 API 계약에는 주소, 연락처, 계좌, 결제 식별자와 증빙 원본을 추가하지 않았다.

## 실제 화면

변경 전 실제 동작 기준 화면은 [2026-07-20 입고상품 수령 기록](2026-07-20-warehouse-inbound-receiving.md)에 있다. 변경 후 로컬 WebApp과 API는 실행했지만 ChatGPT Chrome Extension 연결이 되지 않아 새 코드 기준 PNG 재캡처와 desktop·390px 육안 검증은 아직 완료하지 않았다. 이 항목을 완료하기 전에는 새 리팩터링 commit을 만들지 않는다.

## 자동 검증

- `InboundReceivingWorkspaceCompositionTests`, `입고상품수령페이지ViewModelTests`, `WarehouseOperationDetailTests` 44개 통과
- `dotnet build Ssalddel.WebApp/Ssalddel.WebApp.csproj --no-restore` 경고 0개·오류 0개
- `dotnet build SsalddelApp/SsalddelApp.csproj -f net10.0-windows10.0.19041.0 --no-restore` 경고 0개·오류 0개
- `dotnet build WarehouseManagerApp/WarehouseManagerApp.csproj -f net10.0-windows10.0.19041.0 --no-restore` 경고 0개·오류 0개
