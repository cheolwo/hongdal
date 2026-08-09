# Warehouse World primitive

권한 필터가 적용된 재고·적재·피킹·입고 인계 snapshot을 팔레트, 차량, 화물, 작업 표식, Transporter, DockWorker와 Picker NPC로 투영한다.

- Operational 모드는 `WarehouseManager` 인증 token을 메모리 provider로만 전달한다.
- 작업자 이름, 주문 참조, 연락처, 주소와 계약·정산 정보는 Unity 계약에 포함하지 않는다.
- semantic waypoint를 Scene Transform으로 변환하며 NPC 도착은 서버 작업을 완료하지 않는다.
- `inbound-task:{입고요청Id}`를 차량·화물·운송자·입고작업자의 공통 관계로 사용하고 SKU나 위치 문자열로 관계를 추측하지 않는다.
- 운송 중은 Approach, 도착은 InboundDock, 입고 완료는 StorageZone과 VehicleExit 점유로 구분한다.
- 갱신 실패 시 마지막 성공 snapshot과 기존 object를 유지한다.
- `Ssalddel/Samples/Create Warehouse World Primitive`에서 primitive Scene을 생성한다.

읽기 흐름은 다음과 같다.

```text
WarehouseWorldSnapshotApiModel
  → WarehouseDataMapper / IWarehouseDataRepository
  → WarehouseDataSnapshot
  → WarehouseWorldInterpreter + WarehouseInboundHandoffInterpreter + interpretation lineage
  → WarehouseWorldSnapshot
  → WarehousePresenter / WarehousePresentationSnapshot
  → WarehouseWorldSceneController / WarehouseWorldView
```

기존 `WarehouseWorldMapper`와 `IWarehouseWorldRepository` 기반 조립은 호환 facade로 남기고, VContainer 기본 경로는 Data Repository와 Interpreter를 Query UseCase에 주입한다. 위치·stable-ID 관계·상세 문자열은 View가 아니라 Resolver와 Presenter가 결정한다.

`WarehouseWorldDataFlowCompositionTests`는 VContainer가 기본 3계층 경로와 W2 차량·화물·Dock relation을 실제로 resolve하는지 EditMode에서 확인한다.

## Operational refresh 검증

`Tests/Editor/WarehouseWorldOperationalRefreshTests.cs`는 Unity Test Framework가 설치된 Editor에서 다음을 확인한다.

- 실제 `UnityWebRequest` 최초 조회와 동일 snapshot refresh
- stable ID 기준 추가·변경·제거가 없는 refresh
- 연결 단절 시 `RefreshError`와 마지막 성공 snapshot 유지

로컬 검증은 서버를 `Simulation + DevelopmentReadOnly`, database initialization 비활성, `WarehouseFulfillmentWorkflow` 임시 활성 상태로 실행한 뒤 저장소 루트에서 다음 스크립트를 사용한다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File eng/verify-warehouse-unity-operational.ps1
```

스크립트는 서버의 로컬 설정과 개발 DB를 읽어 메모리에서만 짧은 진단 토큰을 만들고, Unity에는 실제 토큰 대신 일회성 localhost proxy를 전달한다. 운영·CI 인증 수단으로 사용하지 않으며 token·password를 파일, 명령행, Unity 설정이나 로그에 기록하지 않는다.
