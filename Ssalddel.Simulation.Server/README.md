# Ssalddel Simulation Server

Unity 경영 Simulation의 session, scenario clock와 deterministic command 권위를 기존 운영 서버에서 분리하는 별도 ASP.NET Core host다.

이 서버는 실제 운영 전 게임 세계와 업무 규칙을 반복 검증하는 예행연습 서버 역할을 함께 맡는다. 개발 기본 주소는 `http://localhost:5204`이며, 기존 `Ssalddel` 운영 서버의 개발 주소와 분리한다. 예행연습 결과는 실제 운영 원장으로 자동 승격하지 않는다.

코드 책임은 다음 프로젝트 경계로 분리한다.

- `Ssalddel.Simulation.Domain`: Aggregate, 순수 규칙, 상태 전이와 저장 자료 재현
- `Ssalddel.Simulation.Application`: 세션 조회와 미리보기·확정·저장·복원 업무 조율, 저장소 계약
- `Ssalddel.Simulation.Infrastructure`: DB가 비활성일 때 사용하는 메모리 저장소
- `Ssalddel.Simulation.Persistence`: 공유 공공데이터 DB의 읽기 전용 EF 연결, Simulation World 파생 관계 DB와 Simulation Session 저장 자료 DB 구현
- `Ssalddel.Simulation.Server`: HTTP, 실행 설정과 의존성 조립

Domain은 Application·Infrastructure·Persistence·Server를 참조하지 않는다. Application은 저장 구현이나 EF를 직접 생성하지 않고 저장소 계약과 공공데이터 조회 포트만 정의한다. Server는 운영 `Ssalddel.Infrastructure`를 직접 참조하지 않고 Persistence 연결 모듈만 조립한다.

현재 첫 slice는 다음만 제공한다.

- Simulation session 생성·조회
- expected revision 기반 Tick 진행
- `CommandId` 멱등 재시도
- scenario seed, Data revision과 rule revision 보존
- 세력·영지·정착지 stable ID를 가진 공통 World context
- `1 Tick = 1 Game Day` 규칙의 `WorldTick`, `WorldRevision`, `GameDate`
- 상태를 바꾸지 않는 Decision Preview와 명시적 Confirm
- `Decision → Task → Effect` 인과 원장과 Tick 기반 작업 완료
- 기존 공간 비사용 Session의 `simulation-save.v1` 호환과 공간 상태·예약·취소 계보를 포함하는 `simulation-save.v2`
- snapshot·append-only Command log·SHA-256 replay hash
- snapshot 직접 주입 없이 Command를 재실행하는 restore port
- scenario가 명시한 정착지 District·Facility graph와 재정·노동·창고·시장·비축 snapshot
- outbound 예약 식량을 제외한 Fixture `FoodSecurityDays` 계산
- 수확물의 조합 출하·온라인 직판·수출 대행·비축 판로별 영향 Preview와 Confirm
- 창고 capacity·2% Fixture 감모·FoodEquivalent 근거에 따른 비축·FoodSecurityDays 후보
- Confirm 시 수확 Lot 단일 allocation과 labor·treasury·storage capacity 예약
- Task 완료 Tick의 비용·Simulation 수입·시장 공급·비축 Stock Lot 원자적 반영
- 수확 Lot 중복 배정 차단과 판로 전용 Command save/replay
- Cargo stable ID·HarvestLot·PackageLot lineage를 보존하는 물류 이동 Preview와 Confirm
- 원천 allocation 재고 예약과 공통 WorldTick 기반 출발·진행·도착
- 도착 뒤 Hub 검수 전 `DestinationStockCandidate` 경계와 물류 Command save/replay
- 같은 Cargo에 결합된 화물운송 의뢰·배차 후보·가상 차량 용량과 상차·운송·하차 상태 이력
- 목적지 도착 뒤 별도 Preview·Confirm·WorldTick을 요구하는 화물 인수 완료
- 검수 완료 재고를 적재 대기로 보존하고 별도 Preview·Confirm·NPC WorldTick으로 완료하는 창고 적재
- 진부 Hub 입고검수·창고 적재의 공간 능력·용량 검사, Session 공간 예약과 작업 취소
- 참여자별 명시적 의향·수량·동의를 보존하는 같이주문 모집 결과 Preview와 Confirm
- 목표 충족의 `확정`, 목표 미달의 `모집종료목표미달` WorldTick 전이
- 감자 시장재고 300kg을 기준으로 한 개별주문 20kg Preview·재고/노동 예약·포장 Task·수령준비
- 수령준비 전 주문 취소의 원래 Task/Effect 취소와 재고·노동 예약 반환
- 최대 duration을 넘는 Tick 차단

이 서버는 운영 업무용 `SsalddelContext`를 등록하지 않으며 실제 계약·발주·결제·입고·재고를 만들지 않는다. 다만 운영 서버가 외부에서 수집해 둔 공공데이터는 같은 MySQL DB의 `AgriculturalFisheriesDbContext`와 `PublicDataIngestionDbContext`를 읽기 전용으로 등록해 함께 사용한다. 공공데이터 조회에는 `AsNoTracking`을 적용하고 `SaveChanges`를 차단한다. 수집기, migration, 초기화 작업과 운영 업무 원장은 등록하지 않는다. Simulation session·가상 재고·save는 이 공공데이터 DB에 저장하지 않는다.

공공데이터·공간 규칙·시나리오 역할의 결과는 별도 `SimulationWorldDerived` DB의 `SchemaVersion 2` 공간 실행에 저장한다. Synty·URP 대장과 그래픽·VisualKey 계획은 이 공간 실행의 ID와 출력 SHA-256을 입력으로 받는 별도 Synty 경관 실행에 저장한다. Synty 대장이나 URP 프로필만 바뀌면 공간 실행을 다시 만들지 않는다. 건물과 공개 인허가 사업장 node는 공유 공공데이터의 원본 레코드를 참조하고, 사업장–건물 연결을 실제 입주·소유 사실로 확정하지 않는다. 그래픽 계획은 실제 파일 경로 대신 질감·재질·색조·배경·조명·시간대 키와 그림자·LOD·품질 정책을 보존한다. `SimulationWorldDerivationDatabase:Enabled=true`와 별도 연결 문자열이 있을 때만 저장소가 등록되고 기본 설정에서는 비활성이다. migration은 host 시작 시 자동 적용하지 않는다.

평창군 공간 Pipeline `v5`는 공유 공공데이터 DB의 전체 건축물 원장을 유지하면서 Unity 공간 실행에는 건물 용도 Category별 대표 건물을 하나씩만 저장한다. 대표 node에는 대표군·대표하는 원본 건수·대표 순위 1을 기록하며, 공개 사업장도 선택된 대표 건물과 연결된 항목만 표현 후보가 된다. 이 제한은 원본 삭제나 실제 건물 수 축소가 아니라 Unity 표현 Projection이다.

`--interpret-pyeongchang-building-type-demo --spatial-build=<공간실행>`은 종류별 대표에 고정 seed 시험 상태를 배정하고 공간 규칙·Simulation 규칙·결합 규칙·해석 결과를 저장한다. 시험 상태는 실제 회사 영업이나 작업 관측이 아니며 `ScenarioFixtureBuildingActivity`로 명시된다. 출력의 기본 구성 키와 동적 의도 묶음 키는 후속 Synty·URP Adapter 입력이다.

API는 기본 비활성이다. 승인된 Simulation 환경에서만 `SimulationServer:Enabled=true`로 켜며 `SsalddelExecution:Mode=Simulation`이 아니면 host 시작을 거부한다. 실행 중 aggregate를 가진 session store는 계속 프로세스 수명에 한정된다. 저장 자료는 `SimulationSessionDatabase:Enabled=true`일 때 별도 `SimulationSession` MySQL DB에 보관하며, 꺼져 있으면 기존 in-memory 저장소를 사용한다. 프로세스 재시작 뒤에는 저장 식별자로 DB 자료를 읽고 Command를 재생해 새 활성 Session을 복원한다. 현재 활성 Session 자체를 매 Command마다 DB snapshot으로 덮어쓰지는 않는다.

Session 저장 표는 저장 식별자·Session 식별자·schema·WorldTick·개정·Command 수·재생 SHA-256과 전체 `simulation-save.v1` 또는 `simulation-save.v2` JSON을 함께 보존한다. 공간 정의가 없는 기존 Session은 v1과 기존 재생 hash를 유지하고 공간 상태·예약·취소 계보가 필요한 Session은 v2를 사용한다. 조회 시 열 Metadata와 JSON을 대조하고 Save/Replay 검증을 다시 통과하지 못하면 손상 자료로 거부한다. 같은 저장 식별자와 같은 재생 hash는 멱등 재사용하고 다른 hash는 충돌로 거부한다. migration은 host 시작 시 자동 적용하지 않는다.

공공데이터 공유 조회도 기본 비활성이다. 개발 설정에서는 `SimulationSharedPublicData:Enabled=true`이며 `ConnectionStrings:SharedPublicData`를 먼저 사용하고, 개발 설정에 명시된 `FallbackConnectionStringName=DefaultConnection`이 있을 때만 대체 연결 문자열을 사용한다. 배포 기본 설정에는 대체 이름이 없으므로 `SharedPublicData`가 반드시 필요하다. 배포 환경에서는 같은 DB와 스키마를 가리키되 `SELECT`만 허용한 별도 DB 계정을 사용한다. 연결 문자열은 환경 변수나 서버 측 secret 저장소에서 제공하며 source에 기록하지 않는다. 설정을 켰는데 허용된 연결 문자열이 없으면 host 시작을 거부한다.

로컬에서는 Simulation 서버 전용 User Secrets에 같은 DB를 가리키는 연결 문자열을 등록한다. 운영 서버의 secret 값 자체를 source나 작업 보고에 복사하지 않는다.

```powershell
dotnet user-secrets set "ConnectionStrings:SharedPublicData" "<같은 DB의 읽기 전용 연결 문자열>" --project Ssalddel.Simulation.Server
dotnet user-secrets set "ConnectionStrings:SimulationSession" "<별도 Simulation Session DB 연결 문자열>" --project Ssalddel.Simulation.Server
```

## Docker Compose 실행 구성

운영 `Ssalddel` 서버와 같은 .NET 10 다단계 이미지, 비루트 사용자, 환경 변수의 `Section__Key` 주입과 `live`/`ready` 상태 확인 관례를 사용한다. 다만 운영 업무 DB 연결을 Simulation 컨테이너에 전달하지 않는다. `docker-compose.simulation.yml`은 다음 세 권한을 분리한다.

- `ssalddel_simulation_reader`: 기존 `ssalddel_dev` 공유 공공데이터 DB의 `SELECT`만 허용
- `ssalddel_simulation_world`: 별도 `ssalddel_simulation_world` DB의 공간 파생·규칙·표현 해석 원장만 읽고 씀
- `ssalddel_simulation_session`: 별도 `ssalddel_simulation_session` DB의 Session 저장 자료만 읽고 씀

`eng/docker/simulation.env.example`의 항목을 저장소 루트의 추적되지 않는 `.env`에 옮기고 비밀번호를 교체한다. 로컬 예시 기본 비밀번호를 실제 공유 환경이나 운영 환경에 사용하지 않는다. 계정 이름과 비밀번호는 초기화 Script의 SQL 안전 검사를 위해 영문자·숫자와 `_ . @ % -`만 사용한다.

기존 개발 볼륨처럼 실제 공공데이터 DB 이름이 `ssalddel_dev`와 다르면 `SSALDDEL_SIMULATION_PUBLIC_DATA_DATABASE`에 그 이름을 지정한다. 초기화 Script는 지정된 기존 DB를 새로 만들거나 migration하지 않고 읽기 전용 계정에 `SELECT` 권한만 부여한다.

```powershell
docker compose `
  -f docker-compose.yml `
  -f docker-compose.simulation.yml `
  --profile simulation `
  up -d --build mysql simulation-db-init simulation
```

첫 실행 또는 DB migration이 추가된 뒤에는 일반 API host 시작과 분리된 명시적 명령으로 각 migration을 적용한다. host 시작 자체는 migration을 자동 적용하지 않는다.

```powershell
docker compose `
  -f docker-compose.yml `
  -f docker-compose.simulation.yml `
  --profile simulation `
  run --rm simulation --migrate-simulation-world-database

docker compose `
  -f docker-compose.yml `
  -f docker-compose.simulation.yml `
  --profile simulation `
  run --rm simulation --migrate-simulation-session-database
```

상태 확인은 기존 호환 경로 `/health`와 함께 운영 서버와 동일한 의미를 갖는 두 경로를 제공한다.

- `/health/live`: Simulation 프로세스가 응답하는지 확인
- `/health/ready`: 활성화된 공유 공공데이터 DB, Simulation World 파생 DB와 Simulation Session DB에 연결 가능한지 확인

컨테이너 기본 노출은 `127.0.0.1:5204`이고, 외부 공개·TLS·인증·방화벽 구성은 이 로컬 Compose 범위에 포함하지 않는다. `Container` 환경 설정은 세 DB 연결을 활성화하지만 실제 연결 문자열은 Compose 환경 변수로만 공급한다.

공유 공공데이터에서 평창군 Area·건물·공개 사업장 관계를 공간 실행으로 파생하려면 두 DB 연결을 활성화한 승인된 Simulation 환경에서 다음 명령을 실행한다.

```powershell
dotnet run --project Ssalddel.Simulation.Server -- `
  --build-pyeongchang-world-derived `
  --tile-manifest=<private-pyeongchang-tile-manifest-json-path> `
  --spatial-artifact-manifest=<tracked-center-l2-artifact-manifest-json-path>
```

이 명령은 공유 DB에 쓰지 않는다. 원본 정렬·입력 SHA-256, 공간 관계 생성, 원장 검증·출력 hash와 멱등 저장을 수행한다. 선택적 공간 산출물 manifest가 있으면 DEM·토지피복·배치 마스크의 원본 계보, 형식, 표본 크기와 객체 키를 파생 DB에 함께 저장한다. 건물도형·좌표가 없으면 임의 위치를 생성하지 않고 미배치로 보고하며, 원본이 0건이면 `InsufficientSourceData`와 자료부족 node를 공간 DB에 남긴다.

완료 산출물은 기존 Manifest·Artifact route를 유지하면서 다음 본문 route로 읽는다. 로컬 개발에서는 `SimulationWorldDerivationDatabase:ArtifactRootPath` 아래의 상대 객체 키만 허용하며 파일 길이와 SHA-256 불일치는 `409`로 거부한다.

```text
GET /api/simulation/v1/world-stream/tiles/{tileKey}/artifacts/{layerCode}/content
```

같은 실행은 건축물–법정동·행정동 Assignment와 행정동별 건물 Category 집계를 먼저 지역 Projection으로 가공한다. `GET /api/simulation/v1/world-stream/regions/{regionStableId}`는 최신 파생 실행의 법정동·행정동 관계와 건물 분류 집계를 반환한다. 경계 geometry가 아직 없으면 `WaitingForRegionGeometry`와 빈 타일 목록을 반환하며 임의의 지역–타일 관계를 생성하지 않는다.

공간 실행과 독립된 Synty 경관 계획은 다음 명령으로 Job Shell에 제출한다.

```powershell
dotnet run --project Ssalddel.Simulation.Server -- `
  --build-pyeongchang-synty-landscape `
  --spatial-build=<공간 실행 고유 식별자>
```

Job Shell은 공간 출력 SHA-256, AreaSet, Synty·URP 대장, 경관 규칙, seed와 품질 단계를 별도 fingerprint로 만든다. 현재 공간 산출물에 배치 기준점이 없으면 임의 `VisualKey` 위치를 만들지 않고 `Partial`과 배치 거부 사유를 저장한다. 실제 Prefab·Material·HLOD 결합은 후속 Unity BatchMode 작업자의 책임이다.

WI의 실제 경관 공간 승격은 승인 대장과 공간 폐루프를 검사한 뒤 파생 DB에 저장한다. Graph 개정·해시·Node·후속 경로가 맞지 않으면 Scenario 공간으로 자동 대체하지 않는다.

```powershell
dotnet run --project Ssalddel.Simulation.Server -- --build-pyeongchang-wi-spatial-bindings
```

```text
GET /api/simulation/v1/world-stream/area-sets/{areaSetStableId}/interaction-graph-readiness
```

현재 Simulation 상태의 동적 표현은 정적 Synty 경관 실행과 다시 분리한다. `SimulationRuntimeWorldPresentationService`는 화물운송과 물류 이동 상태 사본을 의미 기반 렌더링 의도로 투영하고, Channel 충돌·수명·공간 Route 표면·PC/Mobile Capability를 합성해 URP·Particle·Animation Profile 키와 결정적 표현 hash를 만든다. 이 계산은 Session·WorldTick을 변경하지 않으며 아직 HTTP route나 Unity URP Adapter로 노출하지 않는다. 세부 구조는 [Simulation 규칙 기반 Runtime 렌더링 의도 Pipeline](../docs/Architecture/SimulationRuntimeRenderingIntentPipeline.md)을 따른다.

공간 node와 Simulation 규칙이 객체 표현으로 만나는 단계는 `SimulationWorld객체표현해석JobShell`이 담당한다. 공간·Simulation 규칙 Metadata와 결합 규칙은 초안·활성·폐기 상태로 개정 관리하고, 실제 적용 결과는 공간 출력 hash와 선택적 Session 개정·WorldTick을 봉인한 불변 해석 실행본으로 저장한다. 규칙이 미정이면 활성 공간 규칙의 기본 구성만 사용한다. 세부 구조는 [공간·Simulation 규칙 객체 표현 결합 원장](../docs/Architecture/SimulationWorldObjectRepresentationRuleLedger.md)을 따른다.

시설 의미·시설 기능·업무 Simulation 규칙·객체 연결·Scenario 규칙 묶음은 다음 명령으로 별도 불변 대장에 집결한다. 규칙 실행 코드는 서버에, 현재 상태는 Session 원장에 유지하며 파생 DB는 개정과 관계 계보만 저장한다. 세부 구조는 [Simulation World 업무 규칙 집결 트리](../docs/Architecture/SimulationWorldBusinessRuleTree.md)를 따른다.

```powershell
dotnet run --project Ssalddel.Simulation.Server -- `
  --assemble-pyeongchang-world-business-rules `
  --spatial-build=<공간 실행 고유 식별자>
```

업무 규칙 대장을 바탕으로 Unity 구현 전 UI 정보 구조를 만들려면 다음 명령을 사용한다. Figma 역할 지도·주문자 흐름·공통 홈 node를 설계 근거로 보존하지만 실제 Canvas 좌표나 Prefab 경로는 저장하지 않는다.

```powershell
dotnet run --project Ssalddel.Simulation.Server -- `
  --plan-pyeongchang-world-ui `
  --business-rule-catalog=pyeongchang-farm-hub-town-business-rules.v3
```

UI 기획 `v3`는 활성 객체–업무 규칙 연결을 기준으로 조립하며 진부 Hub 적재 규칙까지 포함한다. 화면 시설·시설 기능·규칙 개정이 원본 연결과 모두 일치해야 하며, 활성 연결이 UI에서 빠지거나 중복되면 저장하지 않는다. 지역별 UI 조립기는 Job Shell과 분리되어 있으므로 다른 AreaSet은 같은 저장·검증 Pipeline을 재사용하고 별도 조립기만 제공한다.

세부 구조는 [Figma 근거 Simulation World UI 기획 원장](../docs/Architecture/SimulationWorldUiPlanningLedger.md)을 따른다.

공간 DB는 추가로 `Unity공간변환Profile → Unity타일Manifest → Unity산출물`을 축적한다. `--tile-manifest`를 생략하면 Unity 원점이나 기준 표고를 추측하지 않고 `WaitingForTileManifest` Profile만 저장한다. Manifest가 있으면 L0~L2 타일 경계·Halo·fingerprint와 원본 계보를 저장한다. 다만 Manifest에 DEM 기준 표고가 없으면 Profile은 `InsufficientSourceData`이며 Terrain·Mask·HLOD 산출 완료로 간주하지 않는다.

L2 500m Runtime Recipe `r2`의 PC 초기 예산은 `3×3 상세 / 5×5 활성 / 9×9 준비`, 동시 Manifest 로드 4개다. 플레이어가 경계 125m 안에서 해당 방향으로 이동하면 Unity가 준비 중심을 한 타일 앞당긴다. 서버 Fixture의 11×11 `CoverageTileKeys`는 이 이동을 검증할 수 있는 제공 범위이지 한 번에 다운로드하거나 활성화하는 범위가 아니다.

```text
POST /api/simulation/v1/sessions
GET  /api/simulation/v1/sessions/{sessionStableId}
GET  /api/simulation/v1/sessions/{sessionStableId}/world-events?afterWorldRevision={revision}
GET  /api/simulation/v1/public-data/kamis-price-observations?itemName=감자&limit=20
GET  /api/simulation/v1/world-stream/regions/{regionStableId}
GET  /api/simulation/v1/sessions/{sessionStableId}/battles
GET  /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}
POST /api/simulation/v1/sessions/{sessionStableId}/battles/previews
POST /api/simulation/v1/sessions/{sessionStableId}/battles/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/participants/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/deployments/preview
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/deployments/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/support-previews
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/supports/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/battles/{battleStableId}/ticks
POST /api/simulation/v1/sessions/{sessionStableId}/ticks
POST /api/simulation/v1/sessions/{sessionStableId}/decision-previews
POST /api/simulation/v1/sessions/{sessionStableId}/decisions/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/harvest-disposition-impact-previews
POST /api/simulation/v1/sessions/{sessionStableId}/harvest-disposition-impacts/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/logistics-movement-previews
POST /api/simulation/v1/sessions/{sessionStableId}/logistics-movements/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/freight-transport-previews
POST /api/simulation/v1/sessions/{sessionStableId}/freight-transports/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/freight-receipt-previews
POST /api/simulation/v1/sessions/{sessionStableId}/freight-receipts/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/warehouse-put-away-previews
POST /api/simulation/v1/sessions/{sessionStableId}/warehouse-put-aways/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/group-order-previews
POST /api/simulation/v1/sessions/{sessionStableId}/group-orders/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/individual-order-previews
POST /api/simulation/v1/sessions/{sessionStableId}/individual-orders/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/individual-order-cancellation-previews
POST /api/simulation/v1/sessions/{sessionStableId}/individual-order-cancellations/confirm
POST /api/simulation/v1/sessions/{sessionStableId}/saves
POST /api/simulation/v1/sessions/restores
GET  /health
GET  /health/live
GET  /health/ready
```

병렬 전투는 기존 FarmSurvival 위협 사건에서 Preview/Confirm으로 생성한다. 경영 세계의 `WorldTick`과 전투의 `BattleTick`은 독립적으로 진행하고, 완료 결과는 다음 안전한 `WorldTick`에 합류한다. 경영 팀원은 보급 상자나 증원 분대를 지원할 수 있지만 예약 자원을 세계 재고 획득이나 농장 노동에 동시에 사용할 수 없다. 명시적 Session Save는 진행 중 전투·예약·멱등 명령 결과와 전투별 무결성 hash를 기존 저장 JSON에 포함하고, Restore에서 활성 전투를 다시 만든다. 활성 저장소는 여전히 프로세스 내부 메모리이므로 자동 저장·자동 시작 복원·다중 인스턴스 분산 잠금은 아직 지원하지 않는다. 계약·자원 잠금·Unity 표현 경계는 [병렬 경영–전투 인스턴스 구조](../docs/Architecture/SimulationParallelManagementBattleInstances.md)를 따른다.

세계 사건 조회는 Simulation Session이 먼저 확정한 사건을 개정 단위로 내려준다. 처음 조회는 `afterWorldRevision=-1`을 사용하고, 이후에는 응답의 `NextAfterWorldRevision`을 다음 요청에 사용한다. `PresentationKey`는 Unity 구성 대장의 의미 키이며 Prefab·Material 경로나 시뮬레이션 업무 확정 권위가 아니다. 현재 첫 adapter는 생존 타로 기회로, 응답·합의는 기존 `/survival-tarot` Confirm 경로를 사용하고 확정 후 사건 변경을 다시 조회한다.

첫 공공데이터 관점별 조회 결과는 KAMIS 가격 관측이다. 품목, 조사일, 규격 단위, 원화 가격, 결측 여부, 출처와 마지막 확인 시각을 반환하며 원문 JSON과 수집 실행 식별자는 내보내지 않는다. 이는 Simulation 규칙의 입력 근거로 읽을 수 있는 관측 자료이지, 운영 재고나 주문 완료 사실이 아니다.

Preview의 예상값은 카드용 Interpretation이며 원장을 변경하지 않는다. 일반 Decision Confirm은 `Confirmed Decision`, `Scheduled Task`, `Pending Effect`를 분리해 기록한다. 수확 판로 Confirm은 여기에 `HarvestLotAllocation=Reserved`를 더하고 labor·treasury와 비축 선택의 storage capacity를 예약한다. 실제 Tick에서 Task·Effect와 allocation을 완료하면서 비용과 Simulation 수입, 시장 공급 또는 비축 Stock Lot을 한 aggregate lock 안에서 적용하고 예약을 해제한다.

save package는 session 생성 요청, 저장 시점 snapshot과 Confirm/Tick Command log를 함께 보존한다. restore는 새 aggregate에 Command를 순서대로 재실행하고 package 자체 hash와 replay 결과 hash가 모두 일치할 때만 session store에 등록한다. 실패 복원은 활성 session을 만들지 않으며 이미 활성인 같은 session도 덮어쓰지 않는다.

정착지 초기 상태는 선택적 session 생성 입력이다. 제공된 scenario에서만 별도 재정·노동·storage·시장 공급·비축 Lot·주민/주둔군 수요를 투영하며 화면의 상자·NPC·건물 크기로 수량을 만들지 않는다. `TreasuryReserved/Available`, `StorageReserved/Available`, `LaborReserved/Available`은 확정된 미완료 작업의 capacity를 포함한다. `FoodSecurityDays`는 명시적 Fixture 환산 rule revision에 따른 게임 지표이며 실제 영양 처방이 아니다.

수확 판로 영향은 기존 Unity choice code와 결정 stable ID·revision을 보존하되 서버가 비용·노동·기간·예상 수입·시장·식량 후보를 다시 계산한다. 네 판로를 점수화하거나 자동 선택하지 않는다. 같은 HarvestLot은 최초 Confirm 이후 다른 판로로 다시 배정할 수 없다. 조합·수출은 비용과 Simulation 수입을, 직판은 여기에 시장 공급을, 비축은 감모 뒤 Stock Lot과 FoodEquivalent를 완료 Tick에 반영한다. 이는 Simulation 결과이며 실제 계약·판매·수출·정산을 뜻하지 않는다.

물류 이동 Preview는 정착지와 원천 HarvestLot allocation을 검증하되 상태를 바꾸지 않는다. Confirm은 같은 Cargo 300kg을 원천 allocation에 예약하고 공통 Task를 생성하며, WorldTick만 `Reserved → InTransit → ArrivedAtDestination`을 확정한다. 도착 결과는 검수 전 재고 후보이므로 실제 Hub 재고·운송·입고·정산을 만들지 않는다.

화물운송은 이 물류 이동을 복제하지 않고 같은 Cargo에 운송 의뢰와 가상 배차·차량 용량을 결합한다. 이동 Tick은 상차·운송·하차 도착 상태 이력을 남기지만 `ArrivedAtDestination`만으로 인수완료를 만들지 않는다. 별도 인수 Preview·Confirm 뒤 Task 완료 Tick에서만 `인수완료`가 된다. 실제 기사·GPS·운임 정산·알림은 수행하지 않는다.

같이주문 Preview는 각 참여자의 명시적 동의와 의향 수량을 보존하고 목표 충족 여부를 계산하지만 상태를 바꾸지 않는다. 모집 결과 Confirm은 공통 Task를 만들고, 완료 WorldTick에서만 목표 충족 모집을 `확정`, 미달 모집을 `모집종료목표미달`로 전이한다. 같은 참여자의 중복 의향은 차단하고 실제 주문·결제·자동 동의는 수행하지 않는다.

다음 공간 구조 우선순위는 평창군 건물 SHP geometry와 DEM 기준 표고·표본을 처리해 현재 미배치 건물과 타일 Manifest를 실제 Terrain·Mask·HLOD 산출물로 연결하는 것이다. 세션·save의 durable 저장소, 인증된 세션 범위와 시나리오 묶음 검증기는 별도 후속 통과 조건이다.
