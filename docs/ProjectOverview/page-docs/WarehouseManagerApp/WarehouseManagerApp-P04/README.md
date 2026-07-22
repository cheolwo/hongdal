# WarehouseManagerApp-P04 - 피킹 작업

[전체 화면 문서](../../README.md) / [WarehouseManagerApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 실제 화면

Web과 `WarehouseManagerApp`이 공유하는 새 목록·상세·실행 Screen을 실제 Web route에서 desktop·390px로 재검증했습니다.

![피킹 작업 검색과 stable-key 상세 전환을 보여 주는 desktop 목록](../../../../assets/changes/2026-07-22-picking-task-route-srp/picking-task-list-desktop.png)

![적재대·상품·전체 수량 확인을 입력 순서대로 배치한 390px mobile 실행 화면](../../../../assets/changes/2026-07-22-picking-task-route-srp/picking-task-execute-mobile.png)

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | WarehouseManagerApp, Ssalddel.WebApp |
| 페이지 ID / 제목 | WarehouseManagerApp-P04 - 피킹 작업 |
| 라우트 | `/work/picking-batch`, `/work/picking-batch/{TaskKey}`, `/work/picking-batch/{TaskKey}/execute`와 Web legacy alias |
| 공용 화면 | `PickingTaskListScreen`, `PickingTaskDetailScreen`, `PickingTaskExecuteScreen` |
| 앱 host | `WarehouseManagerApp/Components/Pages/PickingBatch*.razor`, `Ssalddel.WebApp/Pages/WarehousePickingBatch*Page.razor` |
| capability | Beta / Simulation / 인증 필요 |
| 캡처 상태 | 공용 Web route desktop·390px 실제 재검증 완료 |

## 한 가지 책임

목록 화면은 검색과 대상 선택, 상세 화면은 한 작업 원장 읽기, 실행 화면은 서버에 이미 배정된 한 피킹 작업의 `대기 → 진행중 → 완료` 상태 전이만 맡습니다. 창고 옵션 편집, 작업자 배정, 부분 수량 계산, 포장, 출고, 기사 인계, 운송, 결제와 정산은 어느 화면의 책임도 아닙니다.

커뮤니티에서 확인된 공동의 필요는 공동 원장·다이어그램을 거쳐 주문 참조와 피킹 작업 Key로 내려옵니다. 화면은 이 식별자를 바꾸지 않고 현장 결과를 저장하며, 후속 원장과 다이어그램이 같은 Key를 이어받을 수 있게 합니다. 따라서 피킹은 별도 제품이 아니라 커뮤니티 여정 위에서 필요할 때 열리는 실행 도구입니다.

## 분리된 책임

- `피킹작업목록ViewModel`: 검색, 상태 조건과 서버 페이징만 관리합니다.
- `피킹작업상세ViewModel`: 사용자가 고른 정확한 `taskKey` 한 건만 다시 조회합니다.
- `피킹작업처리ViewModel`: 시작과 완료 Command, 적재대·상품·전체 수량 확인만 관리합니다.
- `피킹작업실행ViewModel`: Command 성공 뒤 목록 전체를 읽지 않고 같은 Key 상세 한 건만 다시 읽는 순서를 조정합니다.
- Web·WarehouseManager host: 로그인, capability, route parameter와 legacy query 이동만 담당합니다.

## API와 권한

- `GET /api/v1/warehouse-operations/picking-tasks`
- `GET /api/v1/warehouse-operations/picking-tasks/{taskKey}`
- `POST /api/v1/warehouse-operations/picking-tasks/{taskKey}/start`
- `POST /api/v1/warehouse-operations/picking-tasks/{taskKey}/complete`

서버는 직접 담당 작업, 소유한 창고 또는 배정된 창고 작업만 반환하고 범위 밖의 Key는 없는 작업과 같은 404로 처리합니다. 목록·상세에는 사용자 ID, 주소, 연락처, 계좌와 결제 식별자를 포함하지 않습니다.

## 상태 전이와 경계

- 시작은 `대기` 작업만 `진행중`으로 바꾸며 동일 요청은 자연 멱등 결과를 반환합니다.
- 완료는 `진행중` 작업에서 서버 적재대 코드, 상품 확인과 전체 수량 확인을 모두 검증합니다.
- 최초 완료만 감사 기록과 `창고피킹완료됨Event`를 남깁니다.
- 완료 뒤 재고 차감, 포장 완료, 출고, 운송, 결제와 정산을 자동 실행하지 않습니다.
- 첫 목록 항목이나 sample을 자동 선택하지 않고 stable-key route에 명시한 정확한 `taskKey`를 유지합니다.

## 검증

- route builder·안전한 복귀 문맥·legacy query 이동·route별 공용 Screen 조립을 자동 테스트로 고정했습니다.
- 목록·상세 Screen에 Command 입력이 없고 실행 ViewModel이 같은 Key 상세만 재조회하는 경계를 자동 테스트로 확인했습니다.
- 전체 `Ssalddel.Tests` 2,523개와 피킹 대상 테스트 107개가 통과했습니다.
- `Ssalddel.WebApp`, `WarehouseManagerApp`, `SsalddelApp`, `SsalddelAdminApp` 소비 빌드는 경고 0개·오류 0개로 통과했습니다.
- desktop 1270×714와 mobile 390×844 모두 horizontal overflow가 없고, mobile navigation은 2열·58px 터치 높이로 표시됐습니다.
- mobile 실행 화면은 확인 폼을 요약보다 먼저 표시하며 적재대·상품·전체 수량 조건이 모두 충족된 뒤에만 완료 버튼이 활성화됐습니다.
- legacy query와 Web alias가 같은 stable-key 상세로 연결되고 브라우저 warning·error가 없음을 확인했습니다.
- 시각 검증 중 시작·완료 Command는 실행하지 않았습니다.
