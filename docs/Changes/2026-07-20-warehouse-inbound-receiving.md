# 창고 입고상품 수령과 현장 반입 요청

커밋 상태: 미커밋

## 변경 기록

| 변경 축 | 화면 변경 여부 | 결과 |
| --- | --- | --- |
| 정확한 입고예정 조회 | 화면 변경 | 선택 창고에서 상품 바코드/SKU와 정확히 일치하는 `입고예정`만 서버 조회하고 첫 항목·부분 일치 sample 대체를 제거 |
| 현장 반입 요청 | 화면 변경 | 일치 원장이 없고 사용자가 경계 안내에 동의한 경우에만 멱등 요청을 저장한 뒤 같은 입고 ID를 재조회 |
| 단일 책임 분리 | 화면·구조 변경 | 창고 목록, 정확 검색, 작성·동의, 상세, 페이지 조정과 host 인증/capability를 독립 ViewModel·adapter로 분리 |
| API·영속 흐름 | 간접 확인 | 기존 `입고요청` 원장 확장, exact SKU query, 멱등 unique index, migration과 Controller API → UseCase → service 연결 |
| HR 권한 조회 | 간접 확인 | .NET 10/EF에서 배열 `Contains`가 `ReadOnlySpan`으로 해석되던 경로를 SQL 변환 가능한 목록으로 고정 |

## 책임과 실행 경계

- 공용 workspace는 렌더링과 사용자 event 전달만 담당하고 서버 저장 판단을 소유하지 않는다.
- 페이지 조정 ViewModel은 창고 목록, exact search, 작성, detail 책임의 실행 순서만 조정한다.
- WarehouseManagerApp과 WebApp host는 인증, capability와 URL의 `inboundId` 동기화만 맡는다.
- 서버는 접근 가능한 창고, 현재 상태, exact SKU, 안내 동의·버전과 멱등 요청 ID를 다시 검증한다.
- 저장 결과는 기존 `입고요청`의 `입고예정` 한 건이다. 이 페이지에서는 입고 완료, 검수, 적재 위치, 재고, 보관 책임, 계약, 운송, 결제와 정산을 만들지 않는다.
- 주소, 전화번호, 계좌, 결제정보와 증빙 원본은 계약과 화면에 포함하지 않는다.

## 실제 화면

### 데스크톱

![통합 WebApp 입고상품 수령 데스크톱](../assets/changes/2026-07-20-inbound-receiving/warehouse-inbound-receiving-desktop.png)

### 모바일 390px

<img src="../assets/changes/2026-07-20-inbound-receiving/warehouse-inbound-receiving-mobile.png" alt="통합 WebApp 입고상품 수령 모바일" width="390">

두 캡처는 실제 통합 WebApp과 local API에서 합성 상품 바코드로 현장 반입 요청을 등록하고 같은 입고 ID를 재조회한 결과다. 캡처 뒤 대상 ID와 SKU를 다시 확인하고 정식 서버 취소 API로 검증 요청을 정리했다. 실제 개인정보, 주소, 연락처, 계좌, 결제 식별자와 증빙 원본은 포함하지 않았다.

## 브라우저 검증

- 개발 계정 로그인과 `Warehouse.Manager` 역할 판정 성공
- 개발 창고 목록과 exact 예정 SKU `V1-DEV-LIVING-BOX` 한 건 조회 성공
- 선택한 기존 입고 ID 2를 URL과 상세에서 동일하게 재조회
- 불일치 SKU에서만 현장 입고 요청 폼 노출
- 필수 입력과 안내 동의 전 저장 비활성, 입력 직후 활성화 확인
- 합성 요청 저장 뒤 URL이 `?inboundId=3`으로 바뀌고 ID 3, `입고예정`, `Unplanned` 상세 재조회
- `/warehouse/work/inbound/products`와 `/work/inbound/products` 모두 동일 ID 표시
- desktop과 390px mobile에서 겹침·가로 잘림 없이 렌더링
- 새 브라우저 탭의 console 경고·오류 0건

## 검증

- `Ssalddel.Tests` 전체 테스트 1,799개 통과
- `Ssalddel`, `Ssalddel.Ui.Common`, `Ssalddel.WebApp`, `WarehouseManagerApp`, `SsalddelAdminApp` build 경고 0개·오류 0개
- exact SKU, 권한, 입력 검증, 멱등 재시도, 같은 ID 재조회와 재고 0건 유지 테스트 포함
- `git diff --check` 통과
