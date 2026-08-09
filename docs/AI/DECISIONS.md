# Ssalddel AI Shared Decisions

> GPT Chat과 Codex가 공통으로 따라야 하는 장기 결정을 기록한다. 현재 진행 상황은 [CURRENT_WORK.md](CURRENT_WORK.md)에 둔다. 기존 결정을 바꿀 때는 원문을 삭제하지 않고 상태를 `Superseded`로 바꾼 뒤 대체 결정 ID를 연결한다.

## 상태 코드

- `Accepted`: 현재 적용하는 결정
- `Superseded`: 후속 결정으로 대체됨
- `Deprecated`: 더 이상 새 작업에 적용하지 않지만 호환성 때문에 기록을 유지함

## D-001 Unity 개발 순서는 제품 버전에 종속하지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-08

Unity 구현 순서를 0.0, 0.5, 1.0 같은 제품 릴리스 번호의 순서로 정하지 않는다. 제품 버전은 공개·운영 capability의 게이트로 유지하고, Unity는 전체 도메인에 공통인 데이터·projection·interaction 계약과 검증 가능한 vertical slice의 필요 순서로 개발한다.

## D-002 Unity는 전체 도메인을 World 관점에서 통합한다

- 상태: `Accepted`
- 결정일: 2026-08-08

Unity는 특정 WebApp이나 일부 페이지의 3D 이식본이 아니다. 농장, 공공데이터, 커뮤니티, 공동 원장, 시장, 운송과 창고를 `World`, `Data`, `Object`, `Interaction`, `Simulation` 관점에서 통합하는 World Projection Client로 설계한다.

전체 도메인을 한 번에 구현한다는 뜻은 아니다. 공통 wrapper와 좁은 vertical slice를 반복해 확장한다.

## D-003 운영 상태의 최종 권위는 서버다

- 상태: `Accepted`
- 결정일: 2026-08-08

권한, 공개 범위, 실제 상태, 원장, revision과 운영 Command의 성공 여부는 서버가 결정한다. Unity animation, GameObject 상태, NPC 도착이나 local cache만으로 주문·참여·배차·검수·입출고를 확정하지 않는다.

운영 interaction은 `preview → explicit confirmation → server Command → canonical re-query → presentation update` 순서를 따른다.

## D-004 Simulation과 Operational 상태를 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-08

simulation fixture, sample과 FakePG는 실제 운영 데이터가 아니다. source type, 실행 mode, provenance와 UI 표시에서 operational data와 구분하며 운영 실패를 simulation 성공으로 숨기지 않는다.

실행 효과는 저장소 공통 기준인 `SsalddelExecution:Mode`의 `Simulation`과 `Operational` 경계를 따른다.

## D-005 Sensor는 단일 관측 projection을 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-08

Sensor는 stable ID, revision, source, 측정값·단위, 기준 시각, freshness, 판정 상태와 근거 reference를 보존하는 일반 관측 모델이다. Unity에서는 물리 장비의 상태, 표시등과 material로 표현하며 별도의 두 번째 감각 표현 모델이나 이중 projection 계약을 두지 않는다.

View가 raw 값을 임의로 재판정하지 않고 서버 또는 승인된 rule이 만든 상태를 표시한다.

## D-006 Git 저장소 문서를 AI 공용 기억으로 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-08

GPT Chat 대화 기록이나 Codex 세션을 프로젝트의 유일한 기억으로 사용하지 않는다.

- `GptProjectContext.md`: 제품과 아키텍처의 공용 시작 컨텍스트
- `DECISIONS.md`: 쉽게 바꾸지 않는 장기 결정
- `CURRENT_WORK.md`: 최근 완료, 검증, 현재 작업, 다음 후보와 미해결 항목의 최신 snapshot
- `AGENTS.md`: Codex가 위 문서와 경로별 기준으로 진입하는 작업 규칙

세부 정책은 Architecture와 Version 기준 문서에 유지하고 공용 문서에는 필요한 요약과 link만 둔다.

## D-007 외부 시각 asset은 View wrapper 뒤에 둔다

- 상태: `Accepted`
- 결정일: 2026-08-08

Synty를 포함한 외부 asset은 Presentation 계층의 교체 가능한 시각 리소스다. 원본 Prefab에 Ssalddel 업무 로직을 직접 넣지 않고 `VisualRoot`를 가진 project View wrapper로 감싼다. primitive placeholder로 socket, scale, interaction과 target platform 성능을 먼저 검증한 뒤 구매·도입 범위를 정한다.

## D-008 DbSet과 Unity Controller를 1:1로 대응하지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-08

EF `DbSet`, MongoDB 원장과 외부 관측은 Unity에 표현할 현실 실체와 상태를 찾는 출발점이다. Unity가 Entity나 document를 직접 소비하지 않고, 서버가 권한과 공개 범위에 맞는 aggregate projection API를 제공한다.

Unity UseCase는 사용자 질문과 행동 단위로 만들고 SceneController는 Entity 종류가 아니라 World Zone의 상태와 과업을 기준으로 여러 UseCase를 조율한다. 관계 table, 이력과 Outbox는 독립 GameObject가 아니라 aggregate의 상태·관계·revision 또는 내부 동기화 근거로 사용한다.

## D-009 첫 Presentation vertical slice는 도심마트다

- 상태: `Accepted`
- 결정일: 2026-08-08

첫 실제 Unity Presentation 코드 단위는 도심마트 Zone으로 한다. 마트 전체 업무를 한 번에 구현하지 않고 진열대 3개, 상품상자, 가격표, 재고 상태, 출처·기준시각과 상품 선택 상세 panel까지만 연결한다.

초기 Controller↔View 계약은 `SimulatedFixture` ScreenModel로 검증하고, 이후 같은 `I도심마트조회UseCase` 경계 뒤에 Mapper·Repository와 실제 서버 snapshot을 연결한다. Controller와 View가 DTO 또는 EF Entity를 직접 해석하지 않는다.

## D-010 차량 중심 차고가 아니라 도심 물류센터를 Zone으로 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-08

입고, 분류·검수, 보관, 출고 대기, 상차와 운송 인계를 묶는 상위 공간은 `도심 물류센터` Zone으로 명명한다.

`창고`, `입·출고 Dock`, `분류 Zone`, `상차 Zone`과 `차량 대기 Bay`는 물류센터 내부 또는 연결 object로 구성한다. `차고`는 차량 정비·보관이 독립 과업으로 필요할 때만 추가한다.

## D-011 Unity Presentation composition root는 VContainer를 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-08

VContainer 1.18.0을 채택하고 Zone별 `LifetimeScope`에서 UseCase, validator, View와 SceneController를 조립한다. MonoBehaviour는 `[Inject]` method injection을 사용하고 engine-independent core는 VContainer를 참조하지 않는다.

Controller 내 simulation fallback `new`와 Scene Builder의 수동 `ConfigureView` 배선은 제거한다. Simulation·Operational 구현 선택은 Controller가 아니라 LifetimeScope 등록에서 바꾸며, 향후 공통 API Client·session이 필요할 때 Application Scope → Zone child Scope로 확장한다.

## D-012 World는 공유하고 Role Perspective를 겹친다

- 상태: `Accepted`
- 결정일: 2026-08-08

생산자, 주문자와 운송자마다 별도 Scene이나 Zone을 복제하지 않는다. 농장, 시장, 주거공동체와 도심 물류센터는 같은 stable-ID 기반 World Object를 공유하고, 활성 역할에 따라 강조 정보, 상세 panel과 허용 interaction만 교체한다.

Controller는 두 축으로 분리한다.

- Zone Controller: 장소의 canonical 상태와 object 생명주기를 조율한다.
- Role Experience Controller: 서버가 승인한 역할별 질문, 강조와 행동 가능 범위를 조율한다.

Role Perspective는 클라이언트 UI 테마나 `if role` 기반 권한 필터가 아니다. Unity가 보내는 역할 선택은 조회 요청일 뿐 권한 증명이 아니며, 서버가 인증 session, 실제 역할 할당, 현재 Zone과 업무 배정을 검증한 projection만 반환한다. Unity는 그 snapshot에 포함된 object 강조와 `AllowedInteractions`만 적용하고 누락된 개인정보나 권한을 추론하지 않는다. 운영 Command는 실행 시 서버가 권한과 revision을 다시 검증한다.

## D-013 NPC 이동은 업무 상태의 Presentation이다

- 상태: `Accepted`
- 결정일: 2026-08-08

NPC는 Zone마다 별도 이동 구현을 복제하지 않는다. 공통 `NpcMovementSnapshot`, stable ID, semantic route와 waypoint 계약을 사용하고 각 Zone은 route profile과 Transform 배치만 제공한다. 서버가 일반 업무 DTO에 Unity `Vector3` 좌표를 보내지 않는다.

운영 NPC는 canonical task stable ID와 revision이 있는 서버 projection만 사용하고, simulation NPC는 `SimulatedFixture`로 구분한다. NavMeshAgent 도착과 Animator event는 표현 결과일 뿐 상차, 하차, 피킹, 검수, 배송 또는 주문 상태를 확정하지 않는다. 실제 상태 변경은 사용자 확인과 서버 Command 성공 뒤 canonical snapshot 재조회로만 반영한다.

개인 공간에는 기본적으로 자동 NPC를 두지 않는다. 다른 Zone은 농장 생산자, 마트 주문자·재고 담당, 주거공동체 주문자·분배 담당, 전통시장 상인·운송자, 물류센터 Dock 작업자·운송자, 창고 picker와 공공·협동 공간 안내 역할의 semantic route를 제공한다.

## D-014 농장 운영 aggregate와 공개 작물 기준을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-08

농장 운영의 canonical root는 소유자 경계를 가진 `농장`이며 `농장구획`, `재배작기`, `농업센서`, `농업센서관측`과 `농장작업`을 연결한다. 모든 World object와 task는 저장된 stable ID와 revision을 사용한다.

농사로 등 공개 작물 기준의 stable ID·source key는 `재배작기`가 참조할 수 있지만, 공개 분류가 실제 농장의 재배·생육·생산 상태를 뜻하지 않는다. 센서 판정은 서버가 관측값, 최신성, 판정 규칙 revision, 근거 card와 한계를 함께 저장·투영하며 Unity View는 raw value를 재판정하지 않는다.

운영 생산자 NPC는 `농장작업`의 canonical task stable ID와 semantic waypoint만 표현하고, Unity 도착이나 animation으로 작업 상태를 변경하지 않는다.

## D-015 Unity 심화 개발 단위는 Zone 업무 흐름이다

- 상태: `Accepted`
- 결정일: 2026-08-08

P0~P7의 Controller·primitive Scene·VContainer 배선 이후에는 Controller 수를 늘리는 것을 진척 기준으로 삼지 않는다. 현실 업무 하나를 canonical source, 권한 projection, Unity Snapshot, World object, NPC, interaction과 server 재조회까지 연결하는 Zone vertical slice를 기본 개발 단위로 사용한다.

게임 목표와 보상은 먼저 발명하지 않는다. 실제 업무의 시간, 대기열, 동선, 용량, 선택, 협력과 결과를 정직하게 공간화한 뒤 관찰에서 확인된 제약만 별도 simulation 또는 승인된 운영 보조 경험 후보로 만든다.

첫 심화 대상은 canonical 재고·적재·피킹·출고와 NPC 기반이 가장 준비된 창고 Zone이다. P8 협동조합·공동원장 공간은 앞 Zone 두 곳 이상에서 같은 공동 원장·역할·비용·노동·결정 요구가 확인된 뒤 만든다.

## D-016 Unity 읽기 흐름은 Data·Interpretation·Presentation을 기본으로 한다

- 상태: `Accepted`
- 결정일: 2026-08-08

Unity가 서버와 공공 source에서 받은 값을 World로 표현하는 기본 변환을 `Data → Interpretation → Presentation` 세 단계로 구분한다.

- Data: 허용된 사실, ApiModel·Mapper·Repository·Snapshot, source·단위·기준시각·정밀도와 data revision
- Interpretation: 여러 Snapshot의 관계·공간 의미·상태 분류·derived metric·simulation과 rule lineage
- Presentation: 표현 관점, Presenter·PresentationModel·SceneController·View·GameObject·NPC·UI와 visual revision

서버의 권한 관점인 `Authorized Perspective`와 Unity의 표현 관점인 `Presentation Perspective`를 분리한다. stable ID는 세 단계를 관통하고 Data, Interpretation과 Presentation revision을 서로 덮어쓰지 않는다.

Query Application은 세 단계를 조율하고 Command Application은 `preview → explicit confirmation → server Command → canonical re-query` 폐루프를 담당한다. Command 결과로 client Snapshot이나 View를 운영 성공 상태로 직접 수정하지 않는다.

현재 P0~P7 코드는 일괄 이동하지 않는다. Warehouse W1을 첫 migration pilot으로 삼아 transport mapping, 위치·관계 해석과 View 표현을 분리한 뒤 다음에 변경하는 Zone에 점진 적용한다.

## D-017 WorldState와 identity·runtime 경계를 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-08

D-016의 세 단계는 다음처럼 구체화한다. Data와 Interpretation은 Scene이 아니라 도메인 World State 기준으로 공유하며 Presentation만 Zone·surface·관점·장치에 맞게 분기한다. Interpretation 결과는 `*WorldState`로 명명하고 Unity Scene이나 View 계약과 구분한다.

`SourceStableId`, `WorldStableId`, `PresentationStableId`는 우연히 같은 문자열일 수 있어도 서로 다른 identity다. source→world→presentation lineage를 보존하고, 다대다 관계는 단일 `SourceStableId` 체인이 아니라 typed World relation graph로 표현한다.

조회 취소, initial/refresh 상태와 last-success는 Data나 View가 아니라 Application Runtime이 소유한다. Runtime은 Data 재조회, 기존 Data 재해석, 기존 World State 재투영을 구분하며 authorization scope가 바뀔 때만 기존 authorized cache를 버리고 새로 조회한다. World Presentation과 Runtime Status는 별도 channel로 전달한다.

Presentation은 순수 Projector, surface별 Reconciler, Unity Applicator로 구분한다. Projector는 GameObject를 만들지 않고 Applicator는 Data를 해석하지 않는다. PublicData, Community와 Warehouse의 기존 facade는 호환을 위해 유지하되 DIP5R에서 새 identity·graph·runtime 계약으로 점진 전환한다.

## D-018 비교 가격의 파생값은 단계간 가격차로 표현한다

- 상태: `Accepted`
- 결정일: 2026-08-08

산지·생산자 수취·도매·소매 가격은 품목, 등급, 규격, 지역, 관측기간, 통화와 단위가 비교 가능할 때만 Interpretation에서 결합한다. 원본 가격과 `단계간 가격차`는 별도 필드와 revision으로 보존한다.

운송비, 선별·포장비, 보관비, 수수료, 폐기 위험, 인건비와 실제 수익 자료가 분해되지 않은 가격차를 `유통마진`, `이익` 또는 특정 참여자의 수취액으로 표시하지 않는다. 비용 source가 확보되면 별도의 cost component WorldState와 근거 lineage로 확장한다.

가격 그래프, 가격표, 유통경로 World 표현과 DetailPanel은 같은 유통가격WorldState를 소비하는 Presentation surface다. Chart나 View가 가격차·증감률·수요 상태를 직접 계산하지 않는다.

## D-019 Interpretation은 Shared World와 Perspective 단계로 나눈다

- 상태: `Accepted`
- 결정일: 2026-08-08

역할별 Interpreter가 Data 결합과 공통 계산을 반복하지 않도록 Interpretation 내부를 `Shared World Interpretation`과 `Perspective Interpretation`으로 분리한다. Shared 단계는 상태, typed relation, spatial/route graph, constraint, candidate와 possibility를 역할과 독립적인 `SharedWorldState`로 만든다.

Perspective 단계는 같은 SharedWorldState를 `Role + Intent + Zone + Focus + Operational/Simulation mode` 문맥으로 읽어 `PerspectiveWorldState`를 만든다. Interpretation Perspective는 역할에게 상황이 갖는 의미와 허용된 다음 행동 후보를 다루고, Presentation Perspective는 그 의미를 map, chart, NPC, route와 panel로 표현한다.

Perspective 변경은 서버 authorization을 확대하지 않는다. authorization scope가 같을 때만 기존 SharedWorldState를 재사용하며, scope가 달라지면 이전 authorized cache와 selection을 제거하고 서버에서 새 Data를 조회한다.

## D-020 Data 조회는 Session·World·Authorization scope에 묶는다

- 상태: `Accepted`
- 결정일: 2026-08-08

Data Layer는 Data 종류와 transport만이 아니라 `누구를 위한 어느 World의 어떤 authorization 범위인가`를 나타내는 Data Context와 그 수명을 소유한다. Unity가 임의 UserId·Role로 권한을 만들지 않고 서버가 승인한 불투명 session, World, role·capability와 authorization revision을 입력으로 사용한다.

Data scope는 `Global`, `World`, `AuthorizedUser`, `AuthorizedUserWorld`로 구분한다. Global public cache는 World 전환과 logout에서 재사용할 수 있지만 World·authorized cache, selection과 private WorldState는 해당 context가 바뀌면 폐기한다. 동일 World 안의 표현 관점 변경만 재해석·재투영하고, Session·World·authorization·mode가 바뀌면 contextual Data query부터 다시 실행한다.

동일한 `WorldStableId`가 여러 World에 존재할 수 있으므로 `WorldObjectRef`로 `WorldContextId`와 객체 ID를 함께 참조한다. cache key는 scope·mode·dataset과 필요한 context identity로 만들고 Data revision은 entry에 별도 보존한다.

## D-021 외부·공공 데이터는 서버 수집·정규화 경계를 통과한다

- 상태: `Accepted`
- 결정일: 2026-08-08

Unity는 토양·농업 토지·인구·가격·기후·교통 등 외부 공급자를 직접 호출하지 않는다. 서버가 공급자별 credential과 wire format을 격리하고 원자료를 private storage에 보존한 뒤 source, dataset, 기준시각, 단위, 공간 정밀도, 한계와 revision이 있는 Ssalddel normalized data로 변환한다.

기존 `PublicDataApiMetadataCatalog`, server User Secrets와 private `IObjectStorageService`를 재사용한다. Source 등록과 수집 활성화는 별도 상태이며 수집은 source별 명시 설정 전까지 기본 비활성이다. credential 값은 DB, 계약, 로그와 Unity에 저장하지 않는다.

운영 provider 실패를 fixture나 simulation으로 대체하지 않는다. 동일 raw hash는 기본적으로 다시 정규화하지 않고 수집 Run과 마지막 성공 normalized data를 독립적으로 보존한다. 공급자 고유 지역 code와 DTO는 normalized 경계 밖으로 노출하지 않으며 Unity는 Ssalddel API projection만 조회한다.

## D-022 외부 공급자 단계는 계약 조사와 실제 연결을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-09

외부 데이터 provider 작업은 `P6-A 공급자 계약 조사`와 `P6-B 실제 수집 검증`으로 분리한다. P6-A는 credential 없이 공식 문서·metadata로 dataset, dimension, unit, code, 공간·시간 정밀도, 결측, license와 attribution을 조사하고 provider-independent normalized contract와 fixture를 고정한다.

P6-B에서만 필요한 credential을 server secret으로 설정하고 live call, raw object 저장, parser/normalizer와 DB lineage를 검증한다. fixture test, metadata endpoint 성공이나 Source Catalog 등록을 운영 수집 성공으로 표현하지 않는다. live 응답 field·null·오류·rate-limit은 실제 호출 전까지 확정 상태로 두지 않는다.

SoilGrids 같은 raster source는 전 세계 cell을 일반 DB row로 펼치지 않는다. bounded WCS coverage 또는 object/spatial storage를 사용하고 source mapped unit·conversion factor·depth·quantile·CRS와 model uncertainty를 보존한다.

## D-023 첫 실제 농업 공급자는 World Bank 최신 경지면적 한 건으로 제한한다

- 상태: Accepted
- 결정일: 2026-08-09

P6-B 첫 live source는 credential이 없는 World Bank WDI `AG.LND.ARBL.HA`로 고정한다. 대한민국 `KOR`의 전 연도 시계열 대신 `mrv=1` 최신 비결측 관측 한 건만 bounded collection하고, source의 기본 collection 상태는 계속 비활성으로 둔다.

live 검증은 전용 opt-in 명령에서만 수행하며 실제 응답을 private raw object와 관계형 테스트 DB의 `Run → RawSnapshot → NormalizedRecord` 계보까지 통과시킨다. 이 로컬 검증을 운영 DB migration 적용, 운영 object storage 저장, 정기 scheduler 또는 Unity API 완료로 확대 해석하지 않는다.

## D-024 도심마트 첫 운영 업무는 진열 보충으로 3계층 migration한다

- 상태: Accepted
- 결정일: 2026-08-09

도심마트의 다음 확장은 새 UI·MVVM·asset 추가보다 현재 `ScreenModel → View` 흐름을 `Data Snapshot → Shared World Interpretation → 관리자 Perspective Interpretation → Presentation`으로 분리하는 DIP6 migration을 먼저 수행한다. 첫 업무는 위치별 재고·진열대·진행 작업 관계와 필요·후보·차단 사유를 함께 검증할 수 있는 `진열 보충`으로 제한한다.

현재 `api/v1/orderer/mart/products`의 `판매가능수량`은 주문자용 공개 투영이며 내부 보관재고·진열재고·예약재고 또는 물리 상자 수가 아니다. 이 route는 공개 상품 호환 경로로 유지하고, 마트 관리자 operational World는 별도의 authorized server Projection과 canonical 진열대·위치별 재고·작업·capability가 준비된 뒤 연결한다. 그 전의 관리자 흐름은 명시적 Simulation/read-only proof로만 제공한다.

가능 작업 해석은 후보와 차단 사유를 만들 뿐 권한이나 운영 성공을 확정하지 않는다. 실제 진열 보충은 preview·명시적 확인·server Command·revision 재검증·canonical 재조회로만 반영하고 NPC 도착이나 animation 완료를 서버 작업 완료로 취급하지 않는다. 상세 기준은 [Unity 도심마트 운영자 3계층 재정비 설계](../Architecture/UrbanMarketOperatorDataInterpretationPresentationRedesign.md)를 따른다.

## D-025 도심마트 관리자 우선순위보다 재고 할당 무결성을 먼저 보강한다

- 상태: Accepted
- 결정일: 2026-08-09

UM3의 상품별 후방재고 합산과 진열대별 활성 작업 차감은 같은 상품을 여러 진열대가 공유할 때 재고를 중복 추천할 수 있다. 따라서 UM4 관리자 Perspective보다 UM3R을 먼저 수행해 원천 재고별 `OnHand / Allocated / Available`과 모든 비종료 작업의 전역 할당을 계산한다.

작업의 단일 `SourceInventoryStableId`는 호환 facade로 유지하되 기본 계약은 여러 원천 위치의 수량을 나타내는 명시적 allocation과 `SourcePlan`으로 확장한다. 할당 초과, 알 수 없는 원천, 단위 불일치와 불완전 계획은 실행 후보가 아니라 data-attention 상태다. Operational 예약의 최종 권위는 서버이며 Unity simulation 계산을 canonical allocation으로 표현하지 않는다.

관리자 우선순위는 무결성 검증을 통과한 Shared World만 소비하고 priority reason·rule revision·source lineage를 보존한다. 판매속도나 시간창 Data가 없을 때 `곧 품절`을 추론하지 않으며, 입고·발주·유통기한·재고 차이도 각 canonical Projection이 생긴 뒤 별도 업무 slice로 확장한다.

## D-026 UM5 뒤 도심마트 공급 계약 경영 Simulation을 우선한다

- 상태: Accepted
- 결정일: 2026-08-09

UM0~UM4의 진열 보충 운영은 공급 계약 결정이 만든 하류 결과로 유지한다. UM5에서 Runtime·surface reconcile·selection·last-success 기반을 먼저 닫은 뒤, 감자 한 품목·세 공급처·4주 시나리오의 `SC0~SC7` 공급 계약 경영 Simulation을 도심마트의 다음 playable track으로 우선한다.

서버에 이미 존재하는 `플랫폼공급조건계약 → 공급계약이용등록 → 조직개별공급발주`는 operational canonical 경계다. Simulation 공급처·Offer·계약안·수요·납품·현금 원장은 공통 `SsalddelExecution:Mode=Simulation` 안의 별도 schema·stable ID로 만들며 세 번째 실행 mode를 추가하지 않는다. Simulation 확정이 실제 계약·발주·결제·입고를 생성하지 않고, Operational 연결은 SC9에서 기존 서버 권한, 조직 동의, 개별 발주 확인, expected revision, 멱등 Command와 canonical 재조회를 통과할 때만 수행한다.

공급 계약 관리 Perspective는 `ReviewReplenishment`와 별도 Intent·Interpreter를 사용한다. 4주 결과는 충족률·비용·현금·폐기·작업 부담·공급 집중도와 대응력을 독립 지표로 제공하고 하나의 이익 점수로 합치지 않는다. 수요·판매속도 해석은 명시적 Simulation 수요 시나리오 또는 운영 canonical Data가 있을 때만 수행한다. 상세 기준은 [도심마트 공급 계약 경영 Simulation 설계](../Architecture/UrbanMarketSupplyManagementSimulationDesign.md)를 따른다.

## D-027 운영 서버와 게임 Simulation 서버를 물리 분리한다

- 상태: Accepted
- 결정일: 2026-08-09

기존 `Ssalddel` 서버는 실제 사용자·조직·동의·공급계약·발주·입고·재고·결제·원장의 최종 권위를 계속 소유한다. Unity 경영 게임의 scenario, seed, session, 가상 시간, save·replay와 Simulation 전용 계약 결과는 별도 `Ssalddel.Simulation.Server`가 소유한다. 두 서버는 데이터베이스와 영속 entity를 공유하지 않으며, 공통 의미가 필요할 때도 명시적 contract·stable ID·source lineage를 통해서만 연결한다.

Simulation 서버는 공통 `SsalddelExecution:Mode=Simulation`을 사용하고 기본 설정에서는 API가 비활성이다. Simulation Command는 실제 계약·발주·결제·입고를 만들 수 없고, 운영 서버의 Domain·Infrastructure 또는 Unity assembly를 참조하지 않는다. Unity client는 operational repository와 simulation repository를 분리하며, SC9의 명시적 adapter 전에는 Simulation 상태를 운영 Command payload로 변환하지 않는다.

현재 첫 수직 슬라이스는 별도 Contracts·Domain·Server·Tests와 session 생성·조회·멱등 Tick·expected revision·scenario lineage까지만 포함한다. 영속 저장소, 인증·사용자별 session scope, 시나리오 catalog와 실제 공급계약 Engine은 후속 slice에서 추가한다.

## D-028 공급계약 Simulation 전에 지역 수요와 주문 객체를 명시한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-026을 구체화하며 대체하지 않음

도심마트 첫 Playable은 공급계약 조건만 비교하지 않고 `지역 인구·세대 공공 Data → 잠재수요 Interpretation → 명시적 Demand Scenario → synthetic Simulation 주문 → 주문 재고할당·충족 → 공급계약 판단`의 계보를 갖는다. 인구·세대수는 주문이 아니며 인구 변화율을 주문 변화율로 직접 사용하지 않는다. 상품 선택률, Simulation 점유율, 계절·요일·행사, seed와 rule revision을 수요 시나리오에 명시했을 때만 주문을 생성한다.

Simulation 주문은 Stable ID, 생성 Tick, 기한, 요청·할당·충족·미충족 수량과 상태를 가진 독립 객체다. 주문 할당은 전역 판매 가능 재고를 중복 소비하지 않아야 하며 UM3R의 진열 보충 작업 allocation과 별도 원장을 사용한다. Tick은 `주문 생성 → 현재 재고 1차 할당 → 납품·검수·입고 → 진열 보충 → 재할당 → 충족 마감 → 폐기 → 결제` 순서를 사용한다. 예정 입고는 검수·진열 capacity를 통과하기 전 현재 가용재고로 합산하지 않는다.

`DemandAndOrderBriefingSurface`는 오늘 주문, backlog, 현재 가용재고, 예정 입고, 향후 7일 Simulation 수요와 계약 공급량을 함께 표시한다. 즉시 충족, 입고 후 충족 가능, 기한 내 충족 불가를 분리하고 reason·basis·rule lineage를 보존한다. Simulation 주문 ID는 실제 주문 ID로 승격하지 않으며 Operational 주문은 기존 운영 서버의 권한·집계·억제된 canonical Projection만 사용한다. 상세 기준은 [도심마트 지역 수요·주문 Simulation 설계](../Architecture/UrbanMarketDemandOrderSimulationDesign.md)를 따른다.

## D-029 공동주택 주문자 집단은 기존 공동구매 원장과 개별 주문 집계를 재사용한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-026과 D-028을 구체화하며 대체하지 않음

공동주택은 별도 주문 도메인이 아니라 기존 자동집단의 배송권·생활권·공동수령 context다. 주민 의향은 기존 `individual-demand`, 모집과 합의는 `GroupPurchase`, 주민별 확정은 `Order`, 확정 수량 집계는 `GroupOrder` 원장을 authority로 사용한다. 공동주택 전용 주문자 집단 Entity·대표 원장·주문 원장을 만들지 않는다.

`공동구매자동집단상태코드.확정`은 모집 결과의 후속 원장 인계 승인이지 주민별 주문 확정이 아니다. 도심마트 Simulation의 `GroupConfirmedDemand`는 연결된 유효 개별 주문을 합산한 `group-order` Projection에서만 읽으며, 비구속 의향은 `GroupIntentDemand`로 별도 표시하고 hard demand에 합산하지 않는다.

공동주택 대표는 기존 공동구매 원장의 `공동구매 대표` 역할과 배송권 운영주체 context로 표현하지만 다른 주민의 주문·수량·결제·계약 권한을 얻지 않는다. 대표의 마트 문의 capability는 기존 역할 배정과 revision을 서버가 검증하는 일반화된 경계가 생긴 뒤에만 연결한다. 확정 fulfillment 뒤 공동수령은 기존 `ResidentialPickup`의 출고·운송 stable ID를 재사용한다. 상세 조사와 구현 순서는 [도심마트 공동주택 주문자 집단 통합 설계](../Architecture/UrbanMarketResidentialOrdererGroupIntegrationDesign.md)를 따른다.

## D-030 공동주택 대표의 사회적 context·업무 권한·NPC 표현을 분리한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-013과 D-029를 구체화하며 대체하지 않음

공동주택 대표는 Unity World에서 주민자치 대표, 입주자대표회의 대표 또는 관리사무소 조정자 같은 사회적 context를 가질 수 있다. 이 label은 세계와 이야기의 의미이며 주문·결제·계약 권한의 근거가 아니다. 실제 capability는 기존 `GroupPurchase` 원장의 `공동구매 대표` 역할 배정, authorization decision과 revision 검증에서만 나온다.

첫 Simulation playable에는 `ResidentialCommunityRepresentative` context와 `ResidentialGroupRepresentative` NPC actor를 명시적으로 둔다. 이동은 공통 `NpcMovementSnapshot → semantic route → waypoint → NavMeshAgent/Animator`를 재사용하고, 주거공동체 leg와 마트 상담 leg를 상위 representative visit state로 묶는다. 한 route나 snapshot에 서로 다른 Zone의 waypoint를 섞지 않는다.

대표 NPC의 이동·도착·대화·animation은 집단 수요 확인, 마트 문의, 조건 전달과 공동수령 조율 상태의 Presentation이다. NPC가 `market.manager-desk`에 도착하거나 대화를 마쳐도 주민 확인, 문의 제출, 주문, 계약, 발주 또는 수령 완료가 자동 실행되지 않는다. 첫 playable의 마트 관리자는 플레이어이며 자동 시연에서 점장 NPC를 표시해도 동일 Perspective의 표현일 뿐 별도 권한자가 아니다. 상세 계약과 RG1·RG4-NPC 순서는 [도심마트 공동주택 주문자 집단 통합 설계](../Architecture/UrbanMarketResidentialOrdererGroupIntegrationDesign.md)를 따른다.

## D-031 Unity 업무 학습은 공통 Concept Card Presentation 문법으로 제공한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-016~D-020과 D-030을 구체화하며 대체하지 않음

Unity World에서 낯선 업무 개념을 설명하는 Presentation은 `Concept`, `Status`, `Reason`, `Action` 네 종류의 공통 카드 문법을 사용한다. 개념 정의, 현재 상태, 판단 근거와 가능한 행동은 서로 다른 책임이며 하나의 정보 panel이나 단일 점수로 합치지 않는다.

계층은 `Perspective WorldState → Learning Card Projector → ConceptCardDeckPresentationModel → ConceptCardView → VisualSkinAdapter`로 둔다. Synty INTERFACE 등 외부 UI asset은 교체 가능한 visual skin이며 업무 의미, stable ID, source lineage, revision, capability를 소유하지 않는다. View와 prefab은 서버·Simulation 값을 다시 계산하지 않는다.

Action Card는 권한을 만들거나 즉시 Command를 실행하지 않는다. 카드 클릭, NPC 도착, 대화와 animation은 Presentation Event이며 Operational 행동은 `Preview → 명시적 확인 → 기존 server Command → canonical 재조회`를 유지한다. API 실패를 Simulation fixture로 대체하지 않고 역할·권한이 바뀌면 비공개 deck과 기존 선택을 제거한다.

첫 vertical slice는 공동주택 대표 NPC를 anchor로 의향 수요, 확정 수요, 공동수령, 공급 상태, 부족 근거와 공급 검토 행동을 연결한다. 도심마트 이후 농장, 가격, 물류, 공동구매와 공공데이터도 같은 카드 문법을 사용하되 각 도메인의 Interpretation과 권한 경계를 보존한다. 상세 기준은 [Unity 개념 카드 Presentation 패턴](../Architecture/UnityConceptCardPresentationPattern.md)을 따른다.
