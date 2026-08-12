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

## D-032 도심마트 관리자 30초 업무 Queue와 우선순위 점수를 제거한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-025의 재고 할당 무결성 우선 결정은 유지하고 관리자 priority·queue 부분만 대체함

현재 Data에는 판매속도, 업무 기한, SLA, 담당자와 지연시간처럼 관리자 업무의 실제 긴급도를 판정할 근거가 충분하지 않다. 따라서 UM4는 `UrgentActions / PendingActions / InProgress / DataAttention` 30초 queue, `PriorityScore`, priority reason과 manager summary surface를 만들지 않는다.

`마트관리자PerspectiveWorldState`는 Shared World의 모든 진열 상태를 Stable ID 결정적 순서로 보존하고 `NeedCode`, 차단 사유, 허용 interaction, SourcePlan, focus 관계, rule revision과 source lineage만 전달한다. Stable ID 순서는 업무 우선순위가 아니다. 실제 우선순위는 authoritative 운영 Data와 명시적 rule이 추가될 때 별도 Interpretation으로 설계한다.

## D-033 Farm·Town·City를 독립 Presentation Region으로 구성하고 이동망으로 연결한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: 기존 WORLD-1~WORLD-5의 공급망 시각 기반을 확장하며 canonical 업무 Zone 결정을 대체하지 않음

Farm·Town·City를 하나의 선형 Transition에 종속시키지 않고 각각 독립적으로 발전할 수 있는 Macro Presentation Region으로 구성한다. 기본 Map은 Farm 북서, City 북동, Town 남쪽 중앙의 삼각형 배치를 사용하고 Farm↔Town 농촌 생활도로, Town↔City 지역 간선도로, Farm↔City 농산물 화물회랑으로 연결한다. 정확한 좌표·거리와 footprint는 prefab·camera·NavMesh 실측 뒤 확정한다.

각 Region은 내부 Composition, route, focus anchor와 바깥쪽 expansion socket을 독립적으로 소유한다. 서로 마주보는 안쪽 면에는 안정된 Gate signature를 두며 Region Scene은 다른 Region 내부 object를 직접 참조하지 않는다. 초기에는 하나의 Integration Scene에서 분리된 root로 검증하고, Gate·Route 계약이 안정된 뒤 World Shell과 Farm·Town·City additive Scene으로 분리할 수 있다. Region은 Presentation 경계이므로 Town 표시만으로 운영 서버에 새 Town Entity·실제 주민 주소·canonical Zone을 만들지 않는다.

사람·차량 이동은 stable ID·revision·source lineage를 가진 Stateful Journey와 업무 의미가 없는 Ambient Traffic으로 분리한다. 기존 Cargo Journey는 Farm↔City 화물회랑에 재사용하고 대표 주민·농부·배송 actor는 명시된 Gate와 semantic route를 따라 이동한다. NPC·차량의 출발·도착·animation은 주문·계약·입고·검수·수령 또는 농작업 완료를 자동 실행하지 않는다. 세부 기준은 [Farm·Town·City 3개 독립 Region Map 구성 설계](../Architecture/UnityFarmTownCityThreeRegionMapLayoutDesign.md)를 따른다.

## D-034 Town과 City 사이에 다중 origin 지역 물류허브를 둔다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-033의 세 독립 Region 원칙은 유지하고 기본 화물 topology의 Farm↔City 직통 회랑 부분을 대체함

Farm·Town·City는 계속 독립적으로 발전하는 세 주 Region으로 유지한다. 화물망은 Town과 City 사이에 `Regional Logistics Hub`를 두는 hub-and-spoke 구조를 기본으로 한다. 여러 Farm의 농산물과 여러 Town의 지역 집배송 화물은 origin별 Gate·Route로 Hub에 들어오고, 입고·검수·보관·분류와 명시적 outbound allocation을 거친 뒤 City 또는 다른 destination으로 재출하된다. Farm→City 직송은 명시적 계약·긴급·대체 route가 있을 때만 허용하는 예외이며 첫 구현의 기본 경로가 아니다.

Hub는 네 번째 생활권이나 실제 행정구역이 아니라 기존 Urban Logistics Center·Warehouse canonical Projection을 표시하는 공유 운영 Area다. cargo 차량의 Hub 도착은 입고 완료가 아니고 Dock·검수 animation은 검사 통과가 아니며, Hub 출발은 City 입고·마트 재고 반영을 의미하지 않는다. 여러 origin lot의 통합은 ProductStableId, 단위, 품질·보관 호환성, accepted available quantity, allocation과 source lineage를 명시적으로 검증해야 한다.

Town↔City 주민·대표·Bus 이동은 Hub freight yard를 통과하지 않는 passenger route로 분리한다. Farm↔Town 직판·생활 이동도 유지한다. 세부 Map, 다중 origin, inbound·outbound와 직송 예외 기준은 [Farm·Town·City 지역 물류허브 Map·Flow 설계](../Architecture/UnityFarmTownCityRegionalLogisticsHubDesign.md)를 따른다.

## D-035 Synty animation은 실제 source를 검증하고 공용 Presentation adapter로 사용한다

- 상태: Accepted
- 결정일: 2026-08-09
- 관계: D-007 외부 시각 asset 경계와 D-013 NPC 이동 Presentation 원칙을 구체화함

현재 import된 Synty Farm·Town·City character는 Humanoid rig를 제공하지만 standalone·embedded animation clip은 확인되지 않았다. Town character prefab 8개의 Animator Controller 참조도 대응 asset을 찾지 못했으므로 사용 가능한 Synty animation으로 간주하지 않는다. 이후 실제 Synty clip이 확인되면 우선 사용하되, 현재는 검증된 Humanoid clip 리타기팅과 차량·설비의 절차형 동작, 명시적 fallback을 사용한다. `SyntyProvided`, `Retargeted`, `Procedural`, `Fallback` source를 catalog에서 구분하고 vendor prefab은 직접 수정하지 않는다.

Animation은 canonical 또는 Simulation state가 만든 `AnimationIntent`를 표현할 뿐이다. NavMesh·route follower가 Journey 위치를 소유하고 root motion은 기본적으로 끄며, NPC·차량 도착·animation event·FX 완료는 Command·Tick·입고·검수·판매·수령을 확정하지 않는다. 구현은 Map·Gate 실측 뒤 공용 source validator와 Idle/Walk adapter부터 시작하고 Zone별 작업 동작은 해당 vertical slice에서 한 종류씩 추가한다. 세부 기준은 [Synty Animation·FX 재사용과 리타기팅 설계](../Architecture/UnitySyntyAnimationReuseAndRetargetDesign.md)를 따른다.

## D-036 공통 상품 stable ID와 출처별 품목코드를 분리한다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-021의 공공데이터 정규화 경계와 감자생산유통 World를 구체화하며 기존 외부 코드를 대체하지 않음

KAMIS 품목코드, 국제 HS·한국 HSK, USDA AMS `Commodity`, 농사로 품목구분Code는 서로 다른 기관·분류 목적의 코드다. 이들을 같은 값으로 변환하거나 한 외부 코드를 Ssalddel 도메인 authority로 사용하지 않고, 서버 내부 `CanonicalProductStableId` 아래에 출처별 code relation을 둔다.

relation은 `Confirmed`, `Candidate`, `Unlinked`를 구분하고 source key, code scheme, code, 상위 code, label, match quality와 근거를 함께 보존한다. `Candidate`는 검색·표시 후보일 뿐 가격 직접 비교, 세관 신고, 계약, 재고 또는 운영 상태 확정에 사용할 수 없다. 공식 코드 근거가 없으면 이름으로 추정하지 않고 `Unlinked`로 남긴다.

첫 항목은 기존 `product:potato`를 재사용한다. KAMIS 식량작물 `100/152`는 확인된 source relation, HS4 `0701`과 USDA AMS `Potatoes`는 후보 relation이며 농사로 품목구분Code는 공식 근거를 확인할 때까지 미연결이다. `공통식품품목Identity`, `공통식품품목Code관계`, `공통식품품목Code관계검토이력`을 별도 영속 원장으로 두고 공개 API는 이 DB projection만 읽는다. 코드 catalog는 초기 seed와 World 상수의 bootstrap 근거로만 남기며 다품목 추가·관계 승격은 revision과 검토 이력을 함께 기록해야 한다.

기존 공공데이터 전수 대조는 KAMIS 분류·품목코드를 정렬 기준으로 사용할 수 있지만 이를 canonical 상품 생성 권한으로 사용하지 않는다. HS·AMS 후보가 있어도 내부 상품이 없으면 `CandidateOnly`, 후보가 없으면 `Unmapped`, 같은 KAMIS code에 이름 충돌이 있으면 `SourceConflict`로 둔다. Preview는 read-only이며 새 상품과 relation의 생성·승격은 별도 검토 Command에서만 수행한다.

## D-037 다품목 승격과 Farm asset 대응을 별도 검토 축으로 유지한다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-036의 다품목 추가 절차와 D-007의 외부 시각 asset 경계를 구체화함

다품목 확대는 현재 관측을 다시 계산한 `PreviewHash`와 명시적 확인 주체가 일치하는 maintenance Command에서만 수행한다. Ssalddel이 발급하는 새 식별자는 `product:food:{KAMIS분류Code}:{KAMIS품목Code}` 형식을 사용하지만, 이 형식은 생성 시점의 충돌 없는 원천 key를 보존하기 위한 내부 식별 규칙일 뿐 KAMIS가 상품 업무의 최종 권위라는 뜻은 아니다. 재실행은 이미 Confirmed인 KAMIS 관계를 기준으로 멱등이어야 한다.

승격 시 KAMIS 분류·품목 관계만 `Confirmed`로 기록한다. HS와 USDA AMS의 복수 후보는 각 관계를 `Candidate`로 보존하고, 농사로는 공식 품목구분Code crosswalk가 없으면 `Unlinked`로 둔다. 한 품목에 여러 HS·AMS 후보가 존재할 수 있으므로 원장은 source별 단일 관계를 강제하지 않고 relation stable ID와 검토 이력으로 각각 식별한다.

Unity의 Farm asset 대응은 이 source-code 검토와 별도다. 모든 canonical 상품을 `Direct`, `Representative`, `Unmapped` 중 하나로 분류하고 `CanonicalProductStableId → semantic VisualKey → prefab reference` 순서로 연결한다. `Representative`는 유사 외형일 뿐 동일 품종·가격·HS 관계를 뜻하지 않으며, `Unmapped`는 Wheat·Potato·Onion 같은 다른 prefab으로 임의 대체하지 않는다. prefab 이름과 `Assets/` 경로는 서버 DTO·Domain·Simulation에 들어가지 않는다.

## D-038 정착지 경영·분쟁 Simulation은 공통 World와 경제 인과를 먼저 닫는다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-027, D-033, D-036~D-037을 유지하며 Unity 구현 우선순위를 재정렬함

현재 FARM-3, HARVEST-CHOICE-1, COOP-1, DIRECT-1, CARGO-1, JOURNEY-1과 Hub Lot 흐름을 폐기하지 않고 정착지 경제·군량·침공으로 확장한다. 다만 판로 분기나 전투 Scene을 계속 늘리기 전에 `SIM-WORLD-0 → DECISION-WORK-0 → SAVE-REPLAY-0 → SETTLEMENT-CORE-1` 순서로 세력·영지·정착지·공통 세계시간과 Decision·Task·Effect 원장을 먼저 만든다.

영지·군단·침공은 기존 운영 제품에 추가되는 운영 상태가 아니라 `Ssalddel.Simulation.Server`가 소유하는 별도 Simulation World다. 현실 사용자·조직·동의·주문·계약·결제와 DB를 공유하지 않으며 게임 Command가 운영 효과를 만들지 않는다. 현재 `Ssalddel.Unity`의 농장·판로·Cargo engine은 검증된 실행 명세로 보존하되, 장기 World authority는 Simulation.Contracts·Domain·Server로 점진 이관한다.

도메인 의미는 시대 중립적으로 유지하고 온라인 마켓·수출대행·영주·성·상단 같은 용어와 외형은 scenario presentation profile에서 표현한다. 첫 필수 playable은 군단이나 공성전이 아니라 감자 300kg 판로가 정착지 재정·노동·시장 공급·비축과 식량 안전 일수를 실제로 바꾸고 원인 Decision까지 역추적되는 경제 폐루프다. 상세 순서와 Gate는 [Unity 실시간 정착지 경제·영지 경영·분쟁 Simulation 재정렬 제안서](../Architecture/UnityRealtimeTerritoryManagementConflictSimulationProposal.md)를 따른다.

## D-039 Simulation save는 versioned package와 Command replay로 검증한다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-038의 `SAVE-REPLAY-0` 저장·복원 경계를 구체화함

Simulation save는 `simulation-save.v1` schema를 가진 package로 저장한다. package는 session 생성 요청, 저장 시점 snapshot, 순서화된 Confirm/Tick Command log, hash algorithm과 replay hash를 함께 보존한다. snapshot은 빠른 조회와 검증 자료이며 복원 권위로 직접 주입하지 않는다. 새 aggregate에 같은 seed·scenario revision·rule revision과 Command를 재실행하고 package 자체 hash와 replay 결과 hash가 모두 일치할 때만 session store에 등록한다.

Command log는 session 내부 적용 순서를 보존하며 멱등 재시도는 새 항목을 추가하지 않는다. schema 불일치, sequence·payload·hash 변조, replay 결과 위치 불일치와 이미 활성인 session 덮어쓰기는 거부한다. 실패한 restore는 부분 session을 등록하지 않는다.

현재 `InMemorySimulationSessionSaveStore`는 restore port의 기본 개발 adapter일 뿐 process restart를 넘는 durable store가 아니다. 실제 연속 플레이 저장은 별도 외부 adapter, schema migration, 동시성·보존·백업 정책을 통과한 뒤 활성화한다. Simulation save는 운영 서버 DB·계약·주문·결제 원장과 공유하지 않는다.

## D-040 정착지 초기 경제는 scenario 입력과 독립 원장 지표로 구성한다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-038의 `SETTLEMENT-CORE-1`과 D-039 save/replay 입력 경계를 구체화함

정착지 초기 경제는 Unity Scene, 상자·NPC 수, 건물 크기나 운영 서버 상태에서 추정하지 않는다. Simulation session 생성 시 scenario가 명시적으로 제공한 District·Facility graph, 재정, 노동 capacity, storage, 상품별 시장 공급, 비축 StockLot, 주민·주둔군 수요와 source stable ID를 사용한다. 기존 session 생성 호환성을 위해 정착지 입력은 선택적이지만, 제공된 입력은 session 생성 멱등 payload와 save/replay hash에 포함한다.

재정·노동·창고·시장 공급·비축·식량 수요는 하나의 종합 점수로 합치지 않는다. `LaborAvailable = Total - Reserved`, `StorageAvailable = Capacity - Occupied`를 보존하고 비축 Lot 합계는 occupied storage를 초과할 수 없다. Facility는 존재하는 District를 참조하고 비축 Lot은 `Storage` Facility만 참조한다. 상품별 시장 공급과 StockLot stable ID는 중복될 수 없다.

`FoodSecurityDays`는 `판매·수출 예약을 제외한 FoodEquivalent / (PopulationDemandPerTick + GarrisonDemandPerTick)`으로 계산한다. FoodEquivalent unit과 rule revision을 반드시 보존하며 첫 값은 현실 영양 처방이 아닌 Simulation Fixture다. `SETTLEMENT-CORE-1`의 값은 초기 snapshot과 active Task projection까지만 제공하고, 실제 Decision Effect에 따른 재정·노동·재고 변경과 Lot 중복 배정 차단은 `SETTLEMENT-ECONOMY-1`에서 수행한다.

## D-041 수확 판로 영향과 비축은 서버 계산 후보로 먼저 연결한다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-038의 `HARVEST-IMPACT-1 + STORAGE-1`을 구현하고 D-040의 실제 경제 적용 경계를 유지함

기존 Unity 수확 판로의 `CooperativeShipment`, `DirectOnlineSale`, `ExportAgent` choice code와 `harvest-disposition:sim.potato.20260407.r1` stable ID를 변경하지 않는다. Simulation 서버는 같은 HarvestLot·판로 결정 stable ID와 revision을 받아 비용, 노동, 기간, 예상 수입, 시장·비축 영향, 위험과 차단 사유를 `harvest-impact:fixture-r1` 정책으로 다시 계산한다. Unity가 계산한 예상값을 Confirm payload의 권위로 받지 않으며, 판로 추천 점수나 자동 선택도 만들지 않는다.

네 번째 선택 `ReserveStorage`는 2% Fixture 감모, 창고 capacity, 같은 상품의 기존 비축 Lot에서 얻은 FoodEquivalent 환산 근거와 rule revision을 사용해 `ReserveStockLotCandidate`와 `FoodSecurityDaysCandidate`를 만든다. 보관은 즉시 군량이 아니며 실제 StockLot, 창고 점유, 재정, 노동, 시장 공급과 식량 안전 일수는 이 단계에서 변경하지 않는다.

Preview는 어떤 session 원장도 바꾸지 않는다. Confirm은 공통 `Decision → Task → Effect` 원장에 후보만 기록하고 Tick 완료는 Effect record를 `Applied`로 전이할 뿐 정착지 경제 값을 적용하지 않는다. 같은 300kg의 실제 allocation과 중복 배정 차단, 비용·수입·재고 반영은 다음 `SETTLEMENT-ECONOMY-1`에서 하나의 transaction 경계로 구현한다. 현재 HarvestLot 관계는 Simulation import 계약과 source-revision lineage로 보존하며 운영 수확 원장이나 실제 거래를 증명하지 않는다.

## D-042 World Map과 정착지 내부는 같은 Simulation snapshot의 관찰 규모다

- 상태: Accepted
- 결정일: 2026-08-10
- 관계: D-027·D-038~D-041의 Simulation 권위를 유지하며 Unity Presentation의 장기 Scene 골격과 구현 순서를 구체화함

World Map, Settlement Interior, District, Object와 향후 Conflict View는 서로 다른 save나 독립 Simulation이 아니다. 모두 같은 `SimulationSession`, `WorldTick`, `WorldRevision`, `SettlementStableId`, Task·Effect·Stock·Treasury·Labor snapshot을 다른 관찰 규모에서 표현한다. Scene 또는 Presentation root를 전환해도 session·시간·재고를 다시 만들지 않으며 GameObject와 prefab은 canonical state를 소유하지 않는다.

다음 서버 권위 Gate는 계속 `SETTLEMENT-ECONOMY-1`이다. 다만 개별 기능 Scene의 추가가 장기 구조를 고착시키기 전에 `WORLD-SHELL-0 → SETTLEMENT-SCENE-0`을 하나의 읽기 전용 Presentation milestone으로 먼저 수행한다. 두 단계는 WorldMap·Settlement root, 공통 카메라·HUD·stable-ID 선택, 첫 정착지 District socket과 blockout만 만들며 경제 계산, Preview·Confirm, Tick 자동 진행, NPC·차량 기반 완료와 분쟁 기능을 포함하지 않는다. 완료 뒤 시각 확장을 중단하고 `SETTLEMENT-ECONOMY-1`로 복귀한다.

## D-043 수확 판로 Confirm은 capacity 예약이고 Task 완료 Tick은 경제 원장 적용이다

- 상태: 확정
- 결정일: 2026-08-10
- 대체 범위: D-041의 "Confirm/Tick은 후보 Effect만 기록하고 정착지 경제를 변경하지 않는다"는 임시 경계

수확 판로 Preview는 계속 무변경 후보 계산이다. Confirm은 하나의 `HarvestLotStableId`에 하나의 `HarvestLotAllocation`만 허용하고, `Labor`, `Treasury`, 비축의 `Storage` capacity를 예약한다. Confirm 자체는 잔액·시장 공급·비축 Stock Lot을 확정 변경하지 않는다.

해당 Task의 완료 Tick에서 aggregate가 예약을 해제하고 비용과 Simulation 수입, 직판 시장 공급 또는 감모 후 비축 Stock Lot·FoodEquivalent를 같은 lock 안에서 반영한다. Allocation과 Effect는 함께 Applied로 전이한다. 같은 HarvestLot은 Applied 뒤에도 다른 판로에 중복 배정할 수 없다. 수확 판로 Confirm은 일반 DecisionConfirm으로 축약하지 않고 전용 Command payload를 save/replay하여 정책 입력과 lineage를 복원한다.

이 원장은 Simulation 전용이며 실제 판매·계약·수출·입고·정산을 만들지 않는다. Unity의 NPC 도착, 차량 animation, 상자 Renderer 수는 완료와 수량의 권위가 아니다.

## D-044 World navigation은 상위 선택을 보존하고 하위 선택만 해제한다

- 상태: 확정
- 결정일: 2026-08-10

`World Map → Settlement → District → Object`는 하나의 Simulation snapshot을 다른 관찰 규모로 보여주는 Presentation navigation이다. 이동 중 session, Tick, revision과 경제 원장을 재생성하거나 변경하지 않는다.

`Back`은 `Object → District → Settlement → World Map` 순서로 한 단계씩 이동한다. Object에서 Back하면 Object 선택만, District에서 Back하면 District 이하 선택만 해제한다. World Map으로 돌아가도 최근 Settlement 선택은 context로 보존하되, 다음 snapshot에서 해당 stable ID가 사라지거나 session이 바뀌면 기존 D-042 규칙에 따라 유효하지 않은 선택을 해제한다.

카메라 World/Zone/Object focus, 선택 강조와 breadcrumb는 Presentation이다. Collider나 클릭 target은 stable ID를 전달할 뿐 Simulation Command를 발행하지 않으며, 차량·NPC 도착과 Renderer 수는 Task 완료나 수량을 확정하지 않는다.

기존 공공데이터 `WorldBootstrapScene`은 공개지도 surface로 유지하고 Simulation World Shell로 암묵적으로 재사용하지 않는다. 신규 `SimulationWorldShell`은 첫 버전에서 하나의 Scene 안의 `WorldMapRoot`와 `SettlementInteriorRoot`를 전환해 동일 snapshot 보존을 검증한다. District는 Presentation socket이며 새로운 서버 Entity나 전용 manager가 아니다. 상세 hierarchy, 선택 규칙, 합류 순서와 완료 Gate는 [Unity Simulation World Shell·정착지 Scene 기반 재정렬 제안서](../Architecture/UnityWorldShellSettlementSceneFoundationProposal.md)를 따른다.

## D-045 Synty 에셋은 자동 원본 목록과 사람의 연구 기록을 분리해 승격한다

- 상태: 확정
- 결정일: 2026-08-10

Farm·Town·City의 Synty Prefab은 곧바로 업무 의미나 `VisualKey`가 되지 않는다. Editor가 GUID·원본 이름·경로·묶음·분류·Prefab 참조를 `에셋원본Index`로 자동 색인하고, 사람이 관찰한 사실·현실 의미·월드 역할 후보·함께 둘 에셋·연결할 자료 후보·승격 후보를 별도 `에셋연구Catalog`에 기록한다. 자동 재색인은 연구 기록을 덮어쓰지 않는다.

연구 상태는 `미검토 → 관찰됨 → 해석됨 → 장소 검증됨 → 체계 검증됨 → 월드 목록 승격`으로 구분한다. `해석됨`은 후보 의미일 뿐 실제 생산량·재고·운영 상태·공공데이터 연결을 증명하지 않는다. 장소와 작은 Simulation 검증을 통과한 항목만 semantic `VisualKey` 후보로 승격하며, 원본 Prefab 이름과 `Assets/` 경로는 Domain·서버 DTO·Simulation stable ID가 되지 않는다.

`신티에셋연구소`는 한 번에 모든 Prefab을 생성하지 않고 묶음·분류별 12개씩 전시한다. 표본 선택은 카메라 초점·선택 강조·연구 카드만 바꾸며 Command, Tick, 운영 저장을 실행하지 않는다. 첫 연구 항목은 Farm의 `온실 01`이고 `farm.facility.greenhouse`는 아직 승격 후보로만 보존한다.

## D-046 Unity 판로 adapter는 서버 Preview 입력과 후보 Task 의미만 구성한다

- 상태: 확정
- 결정일: 2026-08-10

Unity의 수확 판로는 `CooperativeShipment`, `DirectOnlineSale`, `ExportAgent`, `ReserveStorage` 네 choice와 각각의 `CooperativeIntakeCandidate`, `ProducerPackingCandidate`, `ExportReadinessCandidate`, `ReserveStockLotCandidate` workflow code를 서버 계약과 동일하게 사용한다. adapter는 기존 `HarvestDispositionDecision`·`HarvestLot`의 stable ID, revision, 상품, 수량, 단위와 source lineage를 서버 Preview 입력 형태로 보존한다.

adapter가 제시하는 `task:harvest-impact:{DispositionDecisionStableId}`, `{ChoiceCode}Work`, input Lot과 output candidate는 아직 후보 의미다. Unity는 비용, 노동, 기간, 시설, 예상 수입, 감모, FoodSecurityDays, block reason과 Effect를 계산하지 않는다. 서버가 현재 session과 정책 revision으로 다시 Preview하고 사용자가 명시적으로 Confirm한 뒤에만 authoritative Decision·Task 예약이 생기며, 완료 Tick만 D-043의 정착지 경제 원장을 변경한다.

기존 조합 인수·직판 포장·Cargo 준비 구현은 폐기하지 않고 후속 workflow의 Presentation/fixture 명세로 보존한다. adapter 생성만으로 실제 조합 출하, 상품 게시, 수출, 창고 입고, Cargo, 판매 또는 정산이 발생했다고 표시하지 않는다.

## D-047 Unity 연구 Scene 파일명은 한국어 목적 이름을 사용한다

- 상태: 확정
- 결정일: 2026-08-10

`Assets/Ssalddel/Experiments - 연구` 아래 Scene은 사람이 Project 창에서 바로 목적을 알아볼 수 있도록 장소·흐름·검증 목적을 한국어 파일명으로 표현한다. 예를 들어 `PotatoHubReceivingLifecycle`은 `감자물류거점입고검수흐름`, `UrbanMarketCityPackVerticalSlice`는 `도심마트도시팩적용연구`로 표시한다. 기술 단계명, class, namespace와 외부 contract 식별자는 호환성과 코드 탐색을 위해 필요한 경우 영어를 유지할 수 있지만 Scene 파일명에 그대로 노출할 필요는 없다.

Scene 이름 변경은 Unity `AssetDatabase`를 통해 `.unity`와 `.meta`를 함께 이동하여 GUID를 보존한다. Builder, Test, 문서와 Build Settings 등 경로 소비자는 같은 변경에서 새 경로로 갱신하고, 모든 연구 Scene을 실제로 열어 유효성과 이름 일치를 검증한다.

한국어 Scene 파일명은 개발자 탐색을 돕는 Presentation 이름이다. Domain stable ID, 서버 계약, Simulation 권위와 운영 상태를 대신하지 않는다.

## D-048 정착지 1차 미술은 semantic VisualKey와 고정 Presentation 시간으로 구성한다

- 상태: 확정
- 결정일: 2026-08-10

`SimulationWorldShell`의 Farm, Town, Market, Storage, Logistics, Residential는 기존 District socket과 navigation target을 유지한 채 Farm/Urban/Environment catalog의 semantic `VisualKey`로 Synty prefab을 연결한다. Vendor prefab 이름과 경로는 Domain·서버 DTO·Simulation stable ID가 아니며 `WorldVisualInstanceView`와 `VisualRoot` 뒤에 둔다.

첫 시각 기준은 실제 경제량을 Renderer나 상자 수로 재현하는 것이 아니라 한 Overview에서 생산·저장·운반·판매·생활의 공간 관계를 읽게 하는 것이다. Gate와 Garrison은 후속 기능을 위한 Presentation placeholder이며 존재만으로 방어 capability나 주둔 상태를 만들지 않는다.

시간대는 기존 시간 Presentation을 오후 15:00 고정값으로 재사용한다. 자동 순환은 끄고 Simulation `WorldTick`, `WorldRevision`, `GameDate`, Task 완료를 변경하지 않는다. 향후 Day/Night가 Simulation 시간과 연동되더라도 authoritative snapshot을 입력으로 받아 표현할 뿐 Scene presenter가 시간을 진행시키지 않는다.

## D-049 Unity 정착지 상호작용은 Simulation authority 응답만 reconcile한다

- 상태: 확정
- 결정일: 2026-08-10

HarvestLot action card는 `CooperativeShipment`, `DirectOnlineSale`, `ReserveStorage`, `ExportAgent` 네 choice를 보여주지만 비용·노동·기간·수입·감모·FoodSecurityDays와 Effect를 Unity에서 결정하지 않는다. Production client는 공식 Simulation session 조회, harvest disposition impact Preview·Confirm, Tick API를 expected revision과 함께 호출하고 응답 snapshot만 WorldShell HUD와 카드에 reconcile한다.

Preview는 session revision과 WorldTick을 바꾸지 않아야 한다. Confirm은 allocation과 재정·노동·storage capacity 예약까지만 표시하고, 명시적인 완료 Tick 응답에서만 allocation·Task·Effect Applied와 정착지 경제 변경을 표시한다. UI 버튼, 차량·NPC animation, Renderer와 상자 수는 완료 근거가 아니다.

Game View 검증용 `SimulationFixtureAuthority`는 운영 실패 fallback이 아닌 test double이다. Production HTTP client와 같은 경계를 재현하지만 실제 판매·배송·수출·계약·정산을 만들지 않으며 HUD에 fixture임을 표시한다. 실제 실행 서버 live 호출은 별도 검증 사실로 보고한다.

## D-050 Cargo 이동은 공통 WorldTick Task와 원재고 예약을 함께 보존한다

- 상태: 확정
- 결정일: 2026-08-11

기존 CARGO-1/JOURNEY-1의 Cargo stable ID, HarvestLot·PackageLot lineage와 route는 폐기하지 않고 서버 Simulation session의 물류 이동 계약으로 적응한다. Preview는 후보만 반환하고 Confirm은 원천 HarvestLot allocation 수량을 예약한 뒤 공통 Decision·Task·Effect를 생성한다. 출발·진행·도착은 차량 animation이나 Renderer 위치가 아니라 공통 WorldTick 응답으로만 확정한다.

도착은 destination stock candidate를 만들 뿐 Hub 검수·입고를 자동 확정하지 않는다. 같은 원천 allocation을 다른 Cargo에 중복 배정할 수 없으며 이동 중 Cargo 상태와 예약량은 save/replay hash에 포함한다. Unity fixture authority는 Game View test double이고 운영 실패 fallback이 아니다.

## D-051 Unity C# 이름은 한국어 업무 의미와 영어 기술 역할을 조합한다

- 상태: 확정
- 결정일: 2026-08-11

Unity의 C# 파일명, type, method와 test 이름은 사람이 업무 목적을 바로 이해할 수 있도록 한국어 업무 의미를 우선한다. `물류이동Presenter`, `정착지상호작용AuthorityRepository`, `물류이동Tests`처럼 업무·도메인 의미는 한국어로 쓰고 `MonoBehaviour`, `Presenter`, `Repository`, `Authority`, `Snapshot`, `Preview`, `Confirm`, `Task`, `StableId` 같은 기술 역할·공통 계약 용어는 영어를 유지할 수 있다.

이 규칙은 외부 호환 경계를 깨지 않는다. 서버 route, JSON wire field, 직렬화된 stable ID, 기존 오류 code, vendor asset path와 이미 공개된 계약 식별자는 그대로 보존하거나 명시적 변환 경계를 둔다. Unity asset 파일을 바꿀 때는 `AssetDatabase`로 `.meta` GUID를 유지하고 Scene·prefab 참조, 컴파일과 관련 test를 함께 검증한다.

대규모 일괄 rename은 하지 않는다. 현재 구현하거나 수정하는 vertical slice부터 적용하며, 기존 이름은 소비자와 serialization 영향을 확인한 작은 묶음으로 순차 정리한다. 이름 변경만으로 Simulation 권위, 원장 상태나 Presentation 결과를 변경하지 않는다.

## D-052 운영 API의 업무 규칙은 순수 공통 계층을 거쳐 Simulation에 적용한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-038~D-043의 운영/Simulation 권위 분리와 D-050의 공통 WorldTick 인과를 유지하며 `MARKET-CONSUMPTION-1`의 주문 기반을 구체화함

음식배달·화물운송·개별주문·같이주문의 운영 Controller나 POST API를 Simulation이 직접 호출하지 않는다. 운영 코드에서 확인한 상태 code, 허용 전이, 차단 사유와 수량 보존식을 `Ssalddel.WorkflowRules.Contracts`와 `Ssalddel.WorkflowRules`의 외부 효과 없는 순수 규칙으로 고정하고, 운영과 Simulation adapter가 같은 업무 의미를 각자의 권위 안에서 사용한다. 규칙에는 `SourceCapabilityKey`, source contract revision, rule revision과 source stable ID를 남기며 API route 문자열은 업무 identity로 사용하지 않는다.

실제 결제·실제 기사 배차·GPS·주소·개인 알림·운영 주문/재고 쓰기는 Simulation 제외 효과로 명시한다. 개별주문과 같이주문은 현재 운영 원장의 실제 상태까지만 공통 규칙에 포함하고, 운영에 없는 실행 단계를 완성된 API처럼 만들지 않는다. Simulation 전용 주문은 별도 원장으로 시장 재고와 노동을 예약하고, 공통 `Decision → Task → WorldTick → Effect`와 save/replay를 통해서만 수령 준비 또는 취소·예약 반환 상태로 진행한다.

## D-053 Simulation 화물운송은 Cargo 이동과 업무 상태 원장을 분리해 결합한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-050의 Cargo 이동 권위와 D-052의 공통 업무 규칙 적용 경계를 구체화함

Cargo 수량·Lot 계보·원재고 예약·경로 진행은 기존 `LOGISTICS-MOVEMENT-1`이 계속 소유한다. 화물운송 원장은 같은 Cargo를 참조하면서 운송 의뢰, 배차 후보, 가상 운송 주체·차량 용량과 `배차대기 → 매칭중 → 배차확정 → 상차지도착 → 상차완료 → 운송중 → 하차지도착 → 인수완료` 인과 이력을 별도로 보존한다. 두 원장을 새 Cargo나 별도 WorldTick으로 복제하지 않는다.

차량 animation이나 목적지 도착만으로 인수완료를 확정하지 않는다. 물류 이동이 `ArrivedAtDestination`에 도달하면 화물운송은 `하차지도착`에 머물고, 별도 인수 Preview·Confirm으로 예약된 Task가 완료되는 WorldTick에서만 `인수완료`가 된다. 실제 기사 배정, GPS 위치 쓰기, 운임 정산과 운송사 알림은 Simulation 제외 효과이며, 화물 상태와 전이 이력은 save/replay hash에 포함한다.

## D-054 Simulation 같이주문은 명시적 개별 의향을 보존한 모집 결과 원장이다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-052의 운영/Simulation 규칙 공유와 개별 동의 경계를 같이주문 모집 결과에 적용함

Simulation 같이주문은 참여자 수와 총수량만 저장하는 익명 합계가 아니다. 각 참여자의 의향 stable ID, 참여자 stable ID, 희망수량, 단위, 명시적 참여 동의와 source lineage를 보존하고, 같은 참여자의 중복 의향이나 동의 없는 의향을 자동 합산하지 않는다. 실제 개인정보·주소·결제정보는 포함하지 않는다.

Preview는 운영 `공동구매자동집단화계획기`의 예약결제 없는 목표 판정과 같은 의미로 `수요수집중` 또는 `확정대기` 후보를 계산하지만 원장을 변경하지 않는다. 사용자가 모집 결과를 명시적으로 Confirm하면 Task가 예약되고, 완료 WorldTick에서 목표 충족은 `확정`, 목표 미달은 `모집종료목표미달`로 전이한다. 이 결과는 Simulation 수요 원장이며 실제 주문 생성·결제·계약·자동 참여 동의를 뜻하지 않는다.

## D-055 Simulation 음식배달의 전달과 주문자 수령 확인을 분리한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-052의 운영/Simulation 경계와 공통 상태 규칙을 음식배달 생애주기에 적용함

Simulation 음식배달은 운영 음식 주문 API나 저장소를 호출하지 않고 별도 가상 주문 원장을 만든다. 주문 접수 Confirm 뒤 공통 WorldTick이 `주문대기 → 조리중 → 픽업대기 → 기사배정 → 픽업완료 → 전달완료`를 진행한다. 여기서 기사배정은 익명의 Simulation 후보 상태이며 실제 기사 계정, GPS, 개인 주소, 결제, 운영 주문 쓰기나 실시간 알림을 만들지 않는다.

`전달완료`는 기사 관점의 전달 결과이고 `수령확인`은 주문자의 별도 의사다. 차량이나 NPC animation, 목적지 도착, 전달 Task 완료만으로 수령확인하지 않는다. 전달 완료 상태에서 주문자가 별도 Preview·Confirm한 수령 Task가 다음 WorldTick에 완료될 때만 `수령확인`으로 전이한다. 주문 stable ID, 메뉴, 음식점/목적 시설, 비식별 배송권, 수량, 기간, 전이 이력과 source lineage는 save/replay hash에 포함한다.

## D-056 주민 소비는 주문 이행에서 이미 차감된 시장재고를 다시 차감하지 않는다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-052의 Simulation 개별주문 원장과 D-043의 정착지 경제 수량 보존을 주민 소비까지 확장함

개별주문의 재고예약은 포장·수령준비 Task가 완료되는 WorldTick에 시장재고에서 한 번만 차감된다. 이후 주민의 수령·소비 Confirm은 해당 주문, 소진된 재고예약, 상품, 시장 시설, 주문자와 수량 계보를 검증하지만 시장재고를 다시 줄이지 않는다. 예를 들어 감자 300kg에서 20kg 주문이 이행되면 시장 잔여는 280kg이고, 소비 완료 뒤에도 시장 잔여 280kg과 주민 소비 누계 20kg가 같은 정착지 경제 snapshot에 함께 존재한다.

수령준비 전 주문, 다른 주문자, 소진되지 않은 예약, 주문·예약 수량 불일치와 같은 주문의 중복 소비는 차단한다. 소비 Confirm은 Task 예약까지만 수행하고 완료 WorldTick에서 주문과 소비 원장을 `Consumed`로 전이한다. 품목별 주민 소비 누계는 실제 주민 개인정보나 식품영양 환산치가 아니며, 비축량·`FoodSecurityDays`를 근거 없이 변경하지 않는다.

## D-057 수출 준비 검사는 운영 수출이 아니라 실패를 보존하는 Simulation 후보 원장이다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-044의 수확 판로 단일 배분과 D-052의 운영/Simulation 경계를 외부 교역 준비에 적용함

수출 준비는 `ExportAgent` 판로로 적용 완료된 수확 배분만 원천으로 사용한다. Confirm에서 준비 수량을 예약하고 공통 WorldTick이 포장과 모의 검사를 진행한다. 검사 통과는 실제 수출이나 운송사 인계가 아니라 배송대행지 인계 후보를 만들 수 있는 상태일 뿐이며, 수출신고·통관·무역계약·운송·정산을 확정하지 않는다.

검사 실패는 성공처럼 숨기거나 수확물을 소진하지 않는다. 실패 사유와 재작업 가능 상태를 수출 준비 원장에 남기고 원배분의 예약 수량을 해제한다. 이후 재작업·재검사는 별도 Preview·Confirm으로 명시적으로 선택해야 하며, 차량이나 NPC 연출로 검사 또는 인계를 확정하지 않는다.

## D-058 수출 재작업은 실패 원장을 덮어쓰지 않는 새 검사 시도다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-057의 실패 보존과 명시적 재작업 원칙을 다중 검사 시도 계보로 확장함

수출 준비 검사에 실패하면 기존 준비 원장의 상태·사유·검사 시각을 수정해 성공으로 바꾸지 않는다. 사용자가 재작업을 Preview하고 Confirm하면 새 준비 stable ID를 가진 후속 시도를 만들고 루트 준비, 직전 실패 준비와 시도 번호를 함께 기록한다. 직전 실패 시도는 더 이상 재작업 가능하지 않게 닫아 같은 실패 결과에서 여러 재작업이 동시에 수량을 예약하지 못하게 한다.

후속 시도도 WorldTick에서 재작업과 재검사를 거친다. 통과한 최신 시도만 배송대행지 인계 후보가 되고, 실패하면 해당 시도에 새 실패 사유를 기록하고 원배분 수량을 다시 반환한다. 모든 과거 시도는 save/replay 대상이며 삭제하거나 현재 성공 상태로 합쳐 쓰지 않는다.

## D-059 Cargo 준비 완료는 배송대행지 인계나 차량 출발이 아니다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-057·D-058의 검사 통과 계보와 기존 물류 이동 사이에 명시적인 Cargo 준비 경계를 추가함

검사 통과한 최신 수출 준비 시도만 Cargo 준비 원천이 된다. Cargo 준비는 HarvestLot, 포장 Lot 후보, 상품, 수량, Cargo stable ID, 경로와 출발·목적 시설 입력을 하나의 후보 원장으로 조립한다. 수출 준비에서 이미 예약한 수량을 승계하며 같은 수량을 다시 출고 예약하지 않는다.

Cargo 준비 Task 완료는 `ReadyForHandoff`를 의미할 뿐 배송대행지의 실제 인수, 운송사 계약, 배차, 상차, 차량 출발, 수출신고나 통관을 확정하지 않는다. 따라서 Cargo 준비 완료만으로 기존 물류 이동 원장을 만들거나 WorldTick 이동을 시작하지 않는다. 배송대행지 인계와 이후 물류 이동은 각각 별도 Preview·Confirm을 요구한다.

## D-060 배송대행지 Simulation 인계와 물류 이동 시작을 분리한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-059의 Cargo 준비 경계 뒤에 인계 확인을 추가하고 기존 물류 이동과의 분리를 유지함

배송대행지 인계 Preview는 `ReadyForHandoff` Cargo와 원래 Cargo 준비 목적 시설이 일치할 때만 허용한다. Confirm은 인계 Task를 예약하고 완료 WorldTick에서 Cargo 준비와 인계 원장을 `HandedOffInSimulation`으로 전이한다. 이는 Simulation에서 배송대행지가 Cargo를 넘겨받았다는 기록이며 운영 사업자의 실제 인수 증빙이나 계약이 아니다.

인계 완료만으로 배차, 차량 지정, 상차, 출발 또는 기존 물류 이동 원장을 만들지 않는다. 인계된 Cargo의 기존 300kg 출고 예약은 유지하고 이후 물류 이동 결정이 이를 승계해야 한다. 차량과 NPC 연출은 인계 완료나 이동 시작을 확정하지 않는다.

## D-061 수출 Cargo 물류 이동은 기존 출고 예약을 승계한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-060의 별도 인계 완료를 기존 `LOGISTICS-MOVEMENT-1`의 선택적 source로 연결함

배송대행지 Simulation 인계가 완료된 Cargo는 기존 물류 이동 Preview 입력으로 사용할 수 있다. 인계 stable ID, Cargo, HarvestLot, 포장 Lot, 상품, 배분, 수량·단위와 출발 시설이 모두 일치해야 하며 인계 완료 전이나 불일치 계보는 차단한다.

수출 준비에서 이미 확보한 출고 예약은 물류 이동 Confirm에서 다시 더하지 않는다. 물류 이동은 같은 300kg 예약을 승계하고 명시적 Confirm 뒤 WorldTick에서만 출발·진행·도착한다. 화물운송 binding이 없는 경우 실제 운송사, 차량, 배차, GPS와 운임 정산을 만들지 않으며 목적지 도착도 별도 인수 결정 전에는 재고 확정이 아니다.

## D-062 항만 준비시설 도착과 인수 완료를 분리한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-061의 목적지 도착 후보 뒤에 별도 항만 인수 Decision·Task를 추가함

수출 Cargo는 기존 물류 이동이 `ArrivedAtDestination`이고 배송대행지 인계 계보와 목적 시설이 일치할 때만 항만 인수 Preview 대상이 된다. Confirm은 별도 인수 Task를 예약하고 완료 WorldTick에서만 `ReceivedAtPortStaging`으로 전이한다. 기존 출고 예약 300kg과 HarvestLot·포장 Lot·상품·Cargo 계보는 그대로 유지한다.

항만 준비시설 인수는 실제 수출신고, 공인 검사, 검역, 통관 또는 선적이 아니다. 해당 운영 효과는 만들지 않으며 이후 준비 여부도 별도 Decision으로 다룬다. Scene의 하역 animation이나 NPC 도착은 인수 완료 권위를 갖지 않는다.

## D-063 수출 준비성 검토는 자기 진술형 Simulation 후보로 제한한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-062의 항만 인수 완료 뒤 서류·검사 준비 여부를 별도 검토하되 운영 승인과 분리함

`ReceivedAtPortStaging`인 Cargo만 수출 준비성 검토 Preview 대상이 된다. 검토 입력은 서류 묶음 준비 여부와 후속 검사 준비 여부이며, 이 값은 Simulation 자기 진술이다. Confirm과 완료 WorldTick은 `ReadyCandidate` 또는 누락 코드가 있는 `ActionRequired`를 기록할 뿐 정부·검사기관의 확인을 뜻하지 않는다.

보완 필요 결과는 보존하고 새 stable ID, 부모 검토 stable ID와 증가한 시도 번호로 재검토한다. 준비 후보가 완료된 항만 인수에는 중복 검토를 허용하지 않는다. 어느 결과도 실제 수출신고, 공인 검사, 검역 승인, 통관, 선복 예약 또는 선적 권위를 만들지 않으며 출고 예약 300kg과 전체 Lot 계보를 유지한다.

## D-064 수출 선적 계획은 비교 가능한 추정 후보이며 재정을 바꾸지 않는다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-063의 준비 후보 뒤에 목적 시장과 운송 방식의 선택 단계를 추가함

`ReadyCandidate`로 완료된 준비성 검토만 선적 계획 Preview 원천이 된다. 사용자는 목적 국가·시장과 해상/항공 방식별 예상 매출, 국제물류·취급·기타 비용, 예상 운송 기간과 위험 점수를 비교할 수 있다. 위험 수준은 0~33 낮음, 34~66 중간, 67~100 높음으로 결정적으로 계산한다.

Preview는 비교만 제공하고 하나의 명시적 Confirm 뒤 완료 WorldTick에서만 `PlannedCandidate`를 만든다. 같은 준비성 검토에서 계획을 중복 확정하지 않는다. 금액·기간·위험은 출처를 가진 Simulation 추정치이며 견적이나 수익 보장이 아니다. 계획 단계는 정착지 재정과 출고 예약을 변경하지 않고 실제 운송 예약·수출신고·공식 검사·검역 승인·통관·선적을 생성하지 않는다.

## D-065 수출 실행 결과는 seed 기반으로 숨겨 두고 기존 예상 매출과 정산한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-064의 선택된 계획 후보를 Simulation 결과까지 닫되 운영 실행과 분리함

`PlannedCandidate`만 별도 수출 실행 Preview·Confirm 원천이 된다. Preview는 위험 점수에서 계산한 성공 확률과 성공·손실 재정 후보를 제공하지만 실제 결과 roll은 완료 전까지 공개하지 않는다. Confirm은 최악 손실을 감당할 재정 여력만 예약하고, WorldTick이 시작되면 `InTransit`, 계획 기간이 끝나면 세션 seed와 실행 stable ID의 결정적 roll로 성공 또는 손실을 확정한다.

판로 선택에서 이미 반영한 `ProjectedRevenue`는 결과 정산 시 조정한다. 성공 차액은 `계획 순수익 - 기존 예상 매출`, 손실 차액은 `-계획 총비용 - 기존 예상 매출`이다. 이로써 예상 매출을 중복 반영하지 않는다. 성공은 도착 수량, 손실은 손실 수량을 기록하고 모두 출고 예약을 종료한다. 결과 effect는 Simulation 재정 원장에 한 번만 적용하며 실제 운송 예약·신고·검사·검역·통관·선적·운영 정산 권위를 만들지 않는다.

## D-066 수확물 판로 카드는 기존 원장의 읽기 projection만 사용한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-065까지 분리된 판로별 원장을 하나의 생산자 관점으로 읽되 새 권위를 만들지 않음

HarvestLot 판로 결과는 조합 출하·직판·보관·외부 교역 네 선택지를 고정 순서로 반환한다. 실제 선택된 경로만 기존 allocation과 후속 원장에서 현재 단계, 해결·잔여·시장 공급·비축·도착·손실 수량, 누적 Simulation 재정 효과와 위험 결과를 읽는다. 선택되지 않은 경로는 `NotSelected`이며 실제 결과값을 부여하지 않는다.

조합 출하는 물류나 인수 완료 증거가 없으면 해결 수량을 0으로 유지한다. 직판은 allocation이 시장 공급에 반영된 수량, 보관은 감모 후 생성된 비축 Lot 수량, 외부 교역은 최신 준비·Cargo·인계·항만·준비성·계획·실행 계보를 따른다. 이 projection은 조회 전용이며 session revision, 원장, 재정, save/replay hash를 바꾸지 않고 운영 효과도 만들지 않는다.

## D-067 Unity 판로 결과 카드는 서버 읽기 projection을 한국어로만 표현한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-049의 Authority reconcile 원칙과 D-066의 통합 판로 읽기를 Unity 생산자 카드에 적용함

Unity HarvestLot 카드는 공식 `harvest-route-outcomes` 응답의 네 판로, 선택 여부, 현재 단계, 수량, 재정과 위험 결과를 그대로 받아 한국어 Presentation model로 변환한다. Unity는 해결·잔여·비축·도착·손실 수량이나 누적 재정 효과를 다시 계산하지 않고, server code를 사용자 문구로 대응시키는 일만 한다.

판로 결과 조회는 Decision·Task·Effect 명령 흐름과 분리한다. 읽기 조회가 실패해도 이미 Confirm 또는 WorldTick으로 확정된 session snapshot과 상호작용 phase를 되돌리거나 실패로 바꾸지 않는다. Scene과 버튼은 원장 권위가 아니며 후속 진행은 같은 결과 projection을 다시 조회해 표현한다.

## D-068 Unity 판로 재접속은 session과 결과 목록의 동일 revision을 원자적으로 적용한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-067의 읽기 카드가 장시간 실행과 재접속에서도 stale 상태를 표시하지 않도록 보강함

Unity가 판로 카드를 다시 열 때는 최신 Simulation session snapshot을 읽은 뒤 판로 결과 목록을 조회한다. 두 응답의 session stable ID, WorldRevision과 WorldTick이 모두 일치할 때만 WorldShell snapshot, 현재 결과와 카드 phase를 함께 교체한다. 목록이 stale이거나 현재 allocation이 있는데 결과가 비어 있으면 기존 화면 snapshot을 보존하고 읽기 오류만 기록한다.

첫 playable의 한-Lot 범위에서는 목록이 하나일 때 해당 결과를 현재 HarvestLot 카드에 연결한다. 여러 Lot이 생기면 object stable ID와 HarvestLot stable ID의 명시적 mapping을 추가해야 하며 임의의 첫 항목을 선택하지 않는다. 재접속으로 기존 Preview가 없을 때는 Task 기간이나 Tick 수를 추정하지 않고 진행 버튼을 비활성화한다.

## D-069 Unity 판로 작업 재개는 session Task의 남은 Tick만 사용한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-068에서 재접속 시 비활성화한 진행 기능을 권위 있는 Task 원장으로만 다시 열음

Unity는 판로 allocation에 연결된 `TaskStableId`로 같은 Simulation session snapshot의 Task를 찾는다. allocation이 `Reserved`, Task가 활성 상태이고 `Scheduled` 또는 `InProgress`이며 예정 완료 Tick이 현재 WorldTick보다 뒤일 때만 재개할 수 있다. 남은 기간은 `ExpectedEndTick - WorldTick`으로 계산하고 서버가 반환한 일정과 일치하는지 검증한다.

재접속 전에 보았던 Preview나 화면 메모리에서 기간·비용·효과를 복원하지 않는다. 계속 진행은 현재 session revision과 검증한 남은 Tick을 기존 Tick Command에 전달하며, 모순된 Task snapshot은 최신 판로 목록과 함께 화면에 적용하지 않는다. Unity 애니메이션이나 버튼은 Task 완료 권위를 갖지 않는다.

## D-070 플레이 경영 시간은 명시적 턴 마감으로만 진행한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: 기존 `OneTickOneDay`와 D-003의 Preview·Confirm·revision 원칙을 플레이 시간 경계로 구체화함

플레이 중 경영 행동과 조회는 게임 날짜를 자동으로 진행하지 않는다. 사용자는 현재 경영일의 예약 업무와 선택 카드를 검토하고 `턴 마감 Preview → 명시적 Confirm`을 거쳐야 다음 WorldTick과 다음 게임 날짜로 진행한다. 기존 `/ticks` API는 저장 재생과 기술 호환을 위해 유지하지만 플레이 UI의 기본 시간 진행 경로로 사용하지 않는다.

턴 카드는 0장 또는 허용된 수만 선택할 수 있고 서버의 canonical catalog에 있는 stable ID·revision·종류·효과만 적용한다. 선택 효과는 마감한 날을 소급 변경하지 않고 다음 경영일에만 활성화된다. LLM과 Unity View는 카드의 수치·규칙을 만들지 않는다. 초기 `TURN-0`의 바보·전차 카드는 명시적 Fixture이며 승인 publication이나 실제 문화 자료로 오인하지 않는다. 문화 카드는 같은 종류 계약으로 확장하되 출처·달력·효과 규칙을 확정하기 전에는 임의로 게시하지 않는다.

## D-072 문화 턴 카드는 지역·기간·공식 원천·달력·효과 규칙이 완전할 때만 게시한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-070의 문화 카드 확장 조건을 첫 canonical Fixture 계약으로 구체화함

`Culture` 턴 카드는 stable ID와 표시 문구만으로 catalog에 들어오지 않는다. `RegionKey`, Simulation game date 유효 시작·종료, `CalendarRevision`, `EffectRuleRevision`, source stable ID, HTTPS source URL과 근거 확인 시각이 모두 있어야 하며 하나라도 빠지면 서버와 Unity가 fail-closed로 거부한다.

공식 기관 원천은 특정 지역 행사나 생활양식의 대표성을 자동 증명하지 않는다. 첫 `CULTURE-CARD-0`은 서울의 특정 사실을 주장하지 않는 생활문화 질문 Fixture이며, 실제 행사·계절 문화 카드는 지역별 원문 근거와 사람 검수 publication을 별도로 거쳐야 한다. 카드의 수치와 다음 턴 효과는 서버 effect rule revision만 결정하며 Unity와 LLM은 만들거나 보완하지 않는다.

## D-071 Unity 다중 판로 카드는 object-Lot 명시 mapping으로만 선택한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-068의 한-Lot 임시 자동 선택을 여러 Lot에서도 안전한 선택 계약으로 확장함

Unity는 Scene object stable ID와 HarvestLot stable ID를 서로 다른 identity로 유지하고 일대일 mapping을 통해서만 연결한다. object 이름, 표시 순서, prefab 또는 판로 결과 목록의 첫 항목으로 Lot을 추측하지 않는다. object stable ID 중복과 HarvestLot stable ID 중복은 composition 오류로 차단한다.

Simulation session의 모든 HarvestLot allocation은 Lot별 Task authority로 보존한다. 사용자가 object를 선택하면 mapping된 Lot의 allocation·Task·Effect·남은 Tick과 판로 결과만 현재 카드에 함께 투영한다. mapping 결과나 Lot별 Task가 누락되면 기존 화면 선택을 유지하며 다른 Lot의 Task를 대신 보여주거나 진행하지 않는다.

## D-073 Unity 에셋 현실 관측은 연구 해석과 Simulation에서 분리한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: 신티 에셋 연구소를 공공데이터 탐구 입구로 확장하되 운영 사실과 Presentation의 경계를 고정함

Unity 에셋은 source asset GUID로 별도 현실 관측 Catalog의 후보와 연결한다. 에셋 연구 Catalog는 외형에서 관찰한 사실, 현실 의미와 활용 후보를 설명하고, 현실 관측 Catalog는 공공데이터 source·서버 조회 경로·지역·관측 기간·유통 단계·품종·등급·단위·표본 수·revision을 독립적으로 보존한다. 실제 응답이 없거나 필수 근거가 빠지면 `실제 관측 미수집`으로 표시하며 Simulation Fixture나 에셋 수량으로 보완하지 않는다.

공공 관측은 선택한 에셋과 관련된 현실 현상을 이해하게 할 수 있지만 그 prefab의 실제 중량·품질·원산지·소유자·가격이나 특정 농장의 생산·출하 사실을 증명하지 않는다. 기존 Simulation 값은 비교 자료임을 명시하고 실제 관측과 같은 카드에서 혼동되지 않게 구분한다.

source key와 API path는 관측 출처를 찾는 기술 식별자일 뿐 Domain 권위가 아니다. 제품은 canonical product stable ID와 확인된 source 품목 관계를 유지하고, 운영 확정이나 상태 변경은 기존 서버 Command·원장 경계를 통과한다.

## D-074 KAMIS 대응 작물은 모판에서 연구한 뒤 Farm Scene으로 승격한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-073의 현실 관측 분리를 여러 Farm 작물의 반복 가능한 Scene 승격 절차로 구체화함

Farm Pack 작물은 곧바로 생산 Scene에 배치하지 않고 `KAMIS 작물 모판`에서 source prefab, 한국어 이름, canonical product stable ID, KAMIS 분류·품목 관계와 시각 대응 수준을 먼저 확인한다. 첫 모판은 `FarmProductVisualCatalog`에서 `Direct`인 18종만 포함한다. 비슷한 외형을 빌리는 `Representative`와 전용 prefab이 없는 `Unmapped`는 같은 모판에 섞지 않고 별도 검토 구역에 둔다.

모판에서 실제 Scene으로 승격하려면 source·지역·조사일·유통 단계·품종·등급·단위·revision의 관측 경계, 장소 역할과 작은 Simulation, VisualKey와 object-product stable-ID mapping을 각각 확인해야 한다. Scene의 renderer 수·에셋 크기·배치 위치는 생산량·재고·가격·품질 권위가 아니다. 실제 관측이 없을 때는 `실제 관측 미수집`을 유지하고 Simulation Fixture로 대체하지 않는다.

## D-075 공공 관측 출처표와 에셋 연결표는 분리하고 모판 문맥으로 선택한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-073의 현실 관측 분리와 D-074의 Scene 승격 절차를 토양·농업환경 자료로 확장함

공공 관측의 자료 식별자, 제공기관, 공식 주소, 응답 형식, 공간·시간 기준, 관측 항목, 이용조건, 한계와 확인 기준일은 `공공관측SourceCatalog`에서 출처 단위로 보존한다. source asset GUID와 자료의 관계, 실제 관측 수집 여부, 알 수 있는 것과 없는 것, Simulation 비교는 `에셋공공관측Catalog`에서 별도로 보존한다. 같은 에셋이 KAMIS 모판과 토양 모판처럼 여러 연구 문맥에 참여할 수 있으므로 상세 카드는 현재 선택한 모판의 연결을 우선하고 임의의 첫 연결을 사용하지 않는다.

자료 진행 상태는 `자료 후보 → 메타데이터 확인 → 표본 응답 확인 → 실제 관측 연결`로 구분한다. 메타데이터 확인만으로 실제 값, 특정 필지 상태, 감자 재배 권고나 관수 명령을 만들지 않는다. 서로 다른 `환경·식물·소품` 분류의 에셋도 하나의 연구 질문을 구성하면 같은 모판에 전시할 수 있지만, 실제 Scene 승격은 위치 공개 범위, 표본 응답과 코드 대응, 서버 읽기 경계, 작은 Simulation 검증을 각각 통과해야 한다.

## D-076 농사 생육은 일수 대신 환경 Snapshot의 제한 요인과 스트레스로 진행한다

- 상태: 확정
- 날짜: 2026-08-11
- 관계: D-021의 공공데이터 정규화, D-070의 하루 한 턴, D-073~D-075의 관측·연구·Simulation 분리를 감자 생육 규칙으로 구체화함

작물 요구조건, 토양 기준과 수분 상태, 일별 기온·햇빛·강수를 하나의 `재배환경일일Snapshot`으로 고정한다. 하루 생육점수는 가장 부족한 동적 조건을 제한 요인으로 삼고 토양 적합도를 반영하며, 가뭄·과습·저온·고온·저일사·토양부적합 스트레스는 원인별로 별도 누적한다. 강수량은 생육점수에 직접 더하지 않고 유효강수·관수·증발산·유출·배수를 거친 토양수분으로 판정하며, 일사량과 일조시간을 같은 값으로 취급하지 않는다.

농사로와 기상·토양 API는 현실 기준정보와 관측 source이며 Simulation 상태의 권위가 아니다. 서버가 원문·단위·품질·공간·시간·rule revision을 검증하고 Preview·Confirm·Tick을 계산하며 Unity는 결과만 한국어 카드와 에셋 상태로 표현한다. 관측 결측을 Fixture로 조용히 대체하지 않고, 여러 날 진행도 하루씩 계산한다. 첫 감자 구현은 실제 농업 권고가 아닌 별도 versioned Fixture rule로 검증한 뒤 공식 근거를 사람이 검토해 승격한다.

## D-077 Unity 턴 마감은 Confirm 뒤 canonical session을 다시 조회한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-070의 명시적 턴 마감과 D-072의 문화 카드 출처·효과 규칙을 실제 Unity HTTP 경계로 연결함

Unity의 서버 모드 턴 마감은 서버 context 조회, Preview, 명시적 Confirm을 거친다. Confirm 응답만으로 WorldShell을 바꾸지 않고 같은 session을 다시 조회해 stable ID, revision 증가, 완료 Tick과 다음 턴을 검증한 뒤 canonical snapshot을 적용한다. 네트워크 또는 검증 실패를 Fixture 성공으로 대체하지 않으며 기존 화면 상태를 보존하고 오류를 드러낸다.

개발 편의를 위한 결정적 session 자동 생성은 명시적 Development Simulation 조립부에서만 허용한다. 사용자가 이어서 플레이할 session 선택, 인증과 권한, 영속 저장, 서버 재시작 복구는 별도 경계이며 운영 모드의 자동 생성이나 sample fallback 근거가 아니다.

## D-078 턴 카드는 분야별 모판에서 검증한 뒤 서버 덱으로 승격한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-070의 하루 한 턴, D-072의 문화카드 근거, D-073~D-075의 에셋 모판 승격 방식을 턴 카드 전체에 적용함

철학·학당, 지역문화, 경영사건과 공공관측 카드는 서로 다른 모판에서 출처와 검수 방식을 보존한다. 후보 카드는 `카드 씨앗 → 출처 메타데이터 → 내용·사람 검수 → 효과 규칙 → 모판 화면 → 게시 snapshot → 게임 덱 이식` 단계를 거친다. 모판은 연구 projection이며 카드가 모판에 있다는 사실만으로 게시 승인이나 게임 효과 권위를 갖지 않는다.

개발용 Fixture 카드는 효과 규칙과 화면 계약을 검증할 수 있지만 승인 publication을 대신하지 않는다. 실제 승인 catalog가 비어 있으면 빈 덱이 정상이며 Unity나 LLM이 일반 지식, 다른 지역 자료 또는 임의 수치로 보완하지 않는다. 턴 마감 효과는 C5 게시 snapshot과 C6 서버 덱 이식을 통과한 카드만 canonical session에 적용한다.

## D-079 농사로 작업군·콘텐츠·canonical 상품 관계를 분리한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-036의 출처별 품목코드 분리와 D-076의 사람 검토 후 rule 승격 경계를 농사로 감자 Profile에 적용함

농사로 `farmWorkingPlanNew`의 `kidofcomdtySeCode=210005`는 감자 상품코드가 아니라 `밭농사` 작업군 분류다. 감자는 해당 작업군의 일정 목록에서 제목과 `cntntsNo=30699`로 식별되고 상세 응답이 같은 콘텐츠번호·제목·작업군을 반환할 때만 사람 검토용 Profile 후보로 연결한다. 이 콘텐츠 연결은 기존 `NONGSARO_KIND_OF_COMMODITY` 상품 관계를 Confirmed로 승격하지 않으며 그 관계는 별도 공식 품목 crosswalk가 확인될 때까지 `Unlinked`로 유지한다.

상세·시기 원문의 토양, 물, 기온, 햇빛, 생육 단계와 작형 관련 구간은 `LocatedNeedsReview` 근거로만 보존한다. 원문의 수치·시기·재배 설명은 품종·지역·작형·단위와 적용 범위를 사람이 검토하고 새 rule revision을 승인하기 전까지 `FARM-ENV` 임계값이나 운영 농업 처방으로 변환하지 않는다.

## D-080 기상청 ASOS 일관측은 지점·날짜·원문 단위로 보존한다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: D-021의 공공데이터 정규화와 D-076의 기후·토양·작물 일일 Snapshot 경계를 기상청 일관측에 구체화함

기상청 ASOS 일자료는 `관측소 ID + 관측일`을 stable ID의 기준으로 삼고, 관측소·날짜 한 건만 정확히 선택한다. 원문 응답 SHA-256, 조회시각, 출처, 단위, 품질, 공간 정밀도와 한계를 함께 보존하며, 기온·강수량·일사량·일조시간·상대습도를 서로 대체할 수 없는 별도 관측값으로 다룬다.

원천의 빈 값은 0이나 Fixture로 보완하지 않고 `Incomplete`와 결측 필드 목록으로 드러낸다. 필수 항목 중 하나라도 결측이면 FARM-ENV Simulation 입력 자격을 자동 차단한다. ASOS는 농장 면 전체가 아닌 관측소 지점 자료이므로, 농장과 관측소의 거리는 근거 URL이 있는 좌표 context가 제공된 경우에만 계산하고 그렇지 않으면 미계산으로 남긴다. D-1 이전의 과거 관측만 허용하며 예보나 운영 농업 처방으로 표현하지 않는다.

## D-081 통합 모판·전시관의 Scene 이식 단위는 업무 장면이 아니라 개별 Object다

- 상태: 확정
- 결정일: 2026-08-11
- 관계: EXH-0~5의 업무 계보와 증거를 유지하면서 전시관의 모판 역할과 Scene 이식 단위를 명확히 함

통합 모판·전시관은 완성된 업무 Scene을 전시하거나 Story 전체를 하나의 배치 모듈로 제공하지 않는다. 화물·창고, 주문자 집단·마트, 음식배달 같은 업무 계보는 여러 Object의 관계를 설명하는 `Story`로 보존하고, 실제 Scene에 이식하는 단위는 건물, 시설, 가구, Dock, Gate, 차량, 화물, actor visual과 marker 같은 독립 `Seedbed Object`로 둔다.

업무 record stable ID, 모판 Object stable ID와 Scene Placement stable ID는 서로 다른 identity다. Object prefab은 주문·배차·재고·수령 상태를 소유하지 않으며 서버/Simulation snapshot을 Presenter로 받아 표현한다. prefab path·GUID·좌표는 Unity catalog와 Scene placement가 관리하고 shared/server contract는 semantic role, compatible zone, DataBinding, Gate와 evidence만 보존한다.

Object가 독립 Preview에서 검증됐다는 사실만으로 다른 Scene에 자동 배치하지 않는다. 대상 Scene별 placement stable ID, placement profile revision, Runtime test와 Game View evidence를 가진 명시적 O6 승격 기록이 있어야 하며 한 Scene의 승격은 다른 Scene의 승격을 대신하지 않는다.

## D-082 Simulation 생산·소비는 부호가 아니라 자원 변동 유형과 효과 묶음으로 기록한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-038의 공통 Decision·Task·Effect, D-043의 정착지 경제 수량 보존, D-052의 순수 업무 규칙, D-056의 주민 소비 1회 차감을 일반화함

Simulation의 농장 확장, 생산, 소비, 예약, 이동, 변환, 손실과 용량 변화는 하나의 종합 점수나 부호만으로 기록하지 않는다. 같은 양수라도 생산·예약 해제·외부 유입·용량 증가는 서로 다르고, 같은 음수라도 소비·예약·이동 반출·변환 입력·손실은 서로 다르므로 자원 변동 유형을 명시한다.

한 프로젝트는 현금, 노동, 재고, 저장 용량, 생산 용량, 시간과 위험에 대한 여러 효과선을 하나의 효과 묶음으로 만든다. 각 효과선은 대상 원장, 자원 종류, 품목·Lot 참조, 이전 값, 변화량, 이후 값, 단위, 규칙 개정 번호와 출처를 보존한다. 예약은 전체량을 바꾸지 않고 가용량과 예약량 사이에서 이동하며, 위치 이동은 원천 감소와 대상 증가를, 형태 변환은 입력·출력·부산물·손실을 같은 묶음에서 수량 보존식으로 검증한다.

예상 효과는 Preview용 관점별 조회 결과이고 실제 효과가 아니다. 명시적 Confirm 뒤 Task가 완료되는 WorldTick에서만 효과 묶음을 원자적으로 적용하며, 같은 효과 고유 식별자는 한 번만 적용한다. Unity의 `+`·`-` 표시, 작물 Mesh, 상자 수와 애니메이션은 설명용 표현이며 자원 변화의 근거가 아니다. 실제 수치는 Simulation Fixture와 출처·사람 검토를 거친 규칙 개정을 구분하고 운영 원장에 자동 적용하지 않는다.

## D-083 규칙은 업무·해석·표현·상호작용 계층과 세부 영역으로 분리한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-003의 서버 권위, D-004의 Simulation/Operational 분리, D-049의 Unity 상태 재조회와 D-082의 자원 효과 묶음을 규칙 분류에 적용함

프로젝트 규칙을 하나의 목록으로 관리하지 않고 `업무·Simulation`, `해석`, `표현`, `상호작용` 계층으로 먼저 분리한다. 업무·Simulation 규칙은 생산·소비·운송·창고·시장·시설·시간 영역으로 나누며 서버 원장, Task, 상태 전이와 자원 효과를 소유한다. 해석 규칙은 권한·공개 범위에 맞는 관점별 조회 결과와 표현 모델을 만들지만 기준 원장을 변경하지 않는다.

표현 규칙은 그래픽·Material·LOD·FX, 카메라·초점·전환, 애니메이션·이동 표현, 조명·오디오·UI를 담당한다. 표현 규칙은 서버 상태를 읽어 보여줄 수 있지만 생산량·재고·운송·입고·수령 완료를 만들 수 없다. 상호작용 규칙은 선택, 정보 보기와 Preview·Confirm 요청을 구성하며 서버 응답 전에 업무 성공을 확정하지 않는다.

`Simulation자원효과묶음Snapshot.RuleDomainCode`에는 업무 규칙 영역만 허용한다. `Presentation.Camera` 같은 표현 규칙은 별도 Unity 대장과 계약에서 관리하며 자원 효과 묶음으로 변환하지 않는다. 차량 이동, 카메라 도착, 애니메이션 완료, Renderer·GameObject 상태는 업무 규칙의 완료 근거가 아니다.

## D-084 감자 생산의 첫 기준 단위는 명시적 면적을 가진 단일 Tile 재배 단위다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-076의 환경 생육, D-081의 개별 배치 객체, D-082의 자원 효과 묶음과 D-083의 생산 규칙 영역을 첫 감자 생산 세로 단면으로 연결함

감자 재배체 배치 객체는 실제 한 포기나 밭 전체가 아니라 단일 `TileStableId`에 대응하는 재배 군집을 표현한다. 생산 규칙은 Unity Mesh 수나 Grid 칸 수로 면적을 추정하지 않고, 재배 단위 상태에 물리 면적과 유효 재배 면적 비율을 명시적으로 요구한다. Tile의 현실 기준 면적과 유효 비율의 공식 값은 아직 확정하지 않으며 Fixture와 운영 기준을 구분한다.

수확 후보량은 `단위면적 기준 생산성 × 유효 재배 면적 × 환경 계수 × 투입 계수 × 시설 계수 × 손실 계수`로 계산한다. 첫 규칙은 출처와 한계를 가진 Fixture만 허용하며 관측 자료나 Unity 표현을 검토 없이 생산 규칙으로 승격하지 않는다. 재배 단위가 수확 준비 상태이고 Decision이 확정됐으며 Task가 완료된 경우에만 새 수확 Lot을 위한 대기 중 생산 효과 묶음을 만든다. 실제 원장 반영은 해당 묶음을 WorldTick에서 원자 적용했을 때만 성립한다.

기존 300kg Fixture는 호환 테스트에서 100m²와 3kg/m²를 사용해 재현하지만, 두 수치는 실제 농업 생산성·면적 또는 운영 수확량의 근거가 아니다. 단일 Tile은 첫 계약에서 별도 수확 Lot을 만들며 여러 Tile의 Lot 합산은 별도 규칙으로 남긴다.

## D-085 수요·예약·주문 이행·주민 소비는 서로 다른 자원 단계다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-043의 정착지 수량 보존, D-056의 시장 재고 1회 차감, D-082의 자원 효과 묶음과 D-083의 시장·소비 규칙 영역을 공통 효과로 연결함

수요와 주문 의향은 그 자체로 재고를 바꾸지 않는다. 주문 Confirm은 시장 가용량을 주문별 예약량으로 옮기며 전체 보유량은 유지한다. 주문 이행 Task가 완료되면 예약량을 주민 수령량으로 옮기고 이 시점에 시장 보유량에서 한 번만 제외한다. 주민의 실제 소비는 별도 Confirm과 Task 완료 뒤 주민 수령량을 줄이고 소비 누계를 올린다.

주민 소비 단계는 시장 가용량이나 예약량을 다시 차감하지 않는다. 소비 기록에는 주문 이행 후 시장 잔여 수량을 관측값으로 보존하되, 그 값과 소비 시점 값이 다르거나 추가 시장 차감이 적용됐다고 표시되면 효과 묶음을 만들지 않는다. 따라서 주문 이행과 소비는 시간·상태·원장이 분리되면서도 같은 주문·예약·품목·주민 계보를 유지한다.

시장 예약과 이행 효과는 `Market` 규칙 영역, 주민의 실제 소비 효과는 `Consumption` 규칙 영역에 둔다. 음식이 사라지는 Unity 애니메이션이나 UI 숫자 변화는 소비 완료 근거가 아니며 서버 Simulation의 완료된 Task와 최신 상태 사본을 기준으로 한다.

## D-086 운송은 상차·이동 자원 소비·하차·인수 확인을 분리한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-054의 화물운송 Simulation, D-060의 공통 물류 이동, D-082의 이동·손실 보존과 D-083의 운송 규칙 영역을 공통 자원 효과로 연결함

Cargo 운송은 차량이 움직이는 하나의 애니메이션으로 취급하지 않는다. 상차는 원산지 Cargo를 차량 Cargo로 옮기고, 이동은 서버 WorldTick과 경로 진행 상태를 기준으로 연료·노동·명시적 Cargo 손실을 기록한다. 목적지 도착 뒤 하차는 차량 Cargo를 목적지 대기 Cargo로 옮기며, 별도 인수 Confirm과 Task 완료 뒤에만 인수 Cargo로 전환한다.

차량 용량과 Cargo 단위가 맞지 않으면 상차 효과를 만들지 않는다. 운송 손실은 원천 Cargo 감소와 손실 원장 증가를 같은 보존 묶음에 기록하며 원래 Cargo 전체량 이상을 잃을 수 없다. 도착과 인수는 다른 상태이고, 도착만으로 창고 입고나 재고 반영을 확정하지 않는다.

Unity 차량의 Transform, NavMesh 도착, 바퀴 회전, 카메라 추적과 도착 FX는 운송 상태나 인수 완료의 근거가 아니다. 서버의 물류 이동 상태, 화물운송 상태, 완료 Tick과 인수 Task를 다시 조회한 결과만 업무 상태를 바꿀 수 있다.

## D-087 창고는 인수·검수·적치·보관·피킹·출고와 용량을 함께 기록한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-043의 정착지 저장 용량, D-060의 물류 도착·인수 분리, D-082의 이동·예약·손실 보존과 D-083의 창고 규칙 영역을 공통 자원 효과로 연결함

운송 인수가 완료됐다는 사실만으로 창고 재고가 되지 않는다. 인수 Cargo는 창고 입고 대기와 검수를 거치며, 검수 합격량만 적치할 수 있다. 검수 거부량은 조용히 삭제하지 않고 별도 거부·손실 원장과 출처를 남긴다.

적치는 Cargo 수량 이동과 저장 용량 점유를 같은 효과 묶음에서 처리한다. 가용 용량과 점유 용량의 합은 전체 저장 용량과 같아야 한다. 보관 감모와 출고로 창고 재고가 줄면 같은 양의 점유 용량을 해제한다. 피킹은 재고를 즉시 외부로 내보내지 않고 출고 예약으로 옮기며, 별도 출고 Task 완료 뒤에만 출고 인계 Cargo가 된다.

Shelf의 상자 개수, 지게차 애니메이션, Dock 점유 표시와 창고 카메라는 검수·입고·출고 완료의 근거가 아니다. 서버의 완료된 창고 Task와 최신 재고·용량 상태 사본만 창고 업무 결과를 확정한다.

## D-088 Unity 표현 규칙은 영역별 출력 채널과 구현 상태를 대장으로 관리한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-049의 Unity 상태 재조회, D-081의 배치 객체 이식 단위와 D-083의 표현 규칙 영역을 기존 시각 규칙에 적용함

Unity 표현 규칙은 그래픽, 카메라, 애니메이션, 조명, 오디오, UI의 여섯 영역으로 나누고 각 영역이 사용할 수 있는 출력 채널을 제한한다. 그래픽은 Material·Color·Mesh Variant·LOD·FX, 카메라는 초점·거리·구도·전환, 애니메이션은 Animator 상태·이동 재생·속도, 조명은 Light·Ambient·Fog·시간대 시각, 오디오는 Cue·Volume·공간 혼합, UI는 Label·Icon·Panel·Badge·Progress만 다룬다.

기존 `VisualRuleRevision` 문자열과 클래스명은 변경하지 않고 표현 규칙 대장에 연결한다. 이미 코드에 존재하는 규칙은 `ExistingRuleMapped`, 분류 계약만 준비한 규칙은 `ContractPrepared`로 구분한다. 계약 준비 상태는 Unity Component 연결, Scene 실행 또는 Game View 검증 완료를 의미하지 않는다.

모든 표현 규칙은 기준 원장을 변경하지 않고 업무 완료를 확정하지 않는다고 명시해야 한다. 업무 규칙 영역, 서버 Command 효과, 생산·운송·입고·소비 완료를 표현 출력으로 선언하면 검증에서 차단한다. 카메라 도착, 애니메이션 종료, Renderer·GameObject 변화는 계속 업무 완료 근거가 아니다.

## D-089 통합 전시관의 규칙 실험대는 미리보기와 서버 재조회 결과를 비교한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-049의 Unity 상태 재조회, D-081의 개별 배치 객체 이식, D-083의 규칙 계층과 D-088의 표현 규칙 비권위성을 전시관 실험 흐름에 적용함

통합 전시관의 규칙 실험대는 생산·소비·운송·창고 규칙을 Unity에서 다시 계산하거나 기준 원장을 직접 변경하지 않는다. 서버 기준 상태 사본과 예상 효과를 불러와 이전 값·증감값·예상 이후 값을 보여주고, 명시적 Simulation 확정 뒤에는 서버의 최신 상태 사본을 다시 조회해 예상값과 실제값을 대조한다. 확정 요청만으로 Unity의 기준 상태를 바꾸지 않는다.

표현 규칙 실험은 그래픽·카메라·애니메이션·조명·오디오·UI의 차이를 미리보기로만 보여준다. 표현 효과는 기준 원장 자원 효과를 만들거나 Simulation 확정을 요청할 수 없다. 모든 실험 시나리오는 기존 모판 배치 객체의 고유 식별자를 참조하고 실운영 API를 호출하지 않는다.

규칙 실험대의 공통 상태는 기준 상태 준비, 미리보기 표시, 서버 재조회 대기, 예상값과 재조회값 대조 완료, 실패로 구분한다. 실제 Scene 버튼·Prefab·카메라가 연결되거나 Game View가 확인되기 전에는 코드 계약 검증을 전시관 실행 완료로 보고하지 않는다.

## D-090 Unity 감자 생산 실험대는 서버 효과를 재계산하지 않고 변환한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-084의 감자 생산 Fixture 규칙과 D-089의 전시관 미리보기·서버 재조회 대조 경계를 첫 생산 실험대에 적용함

Unity의 감자 생산 실험대 변환기는 `Ssalddel.Simulation` 구현이나 서버 Entity를 직접 참조하지 않고 서버 API 사본만 소비한다. 유효 재배 면적, 기본 수확량, 환경·투입·시설·손실 계수가 반영된 예상 수확량을 Unity에서 다시 계산하지 않는다. 서버가 제공한 생산 효과선의 이전 값, 증감값, 이후 값과 예상 수확량이 서로 일치하는지만 검증해 모판 기준 상태와 미리보기로 변환한다.

첫 감자 생산 실험은 `rule:potato-production.fixture.v1`, `Production`, `Simulation`, 적용 전 `Pending` 효과만 허용한다. 실운영 모드, 서버 권위가 아닌 상태, 기준 개정 번호 불일치, 수확량과 효과선 불일치, 대장과 다른 규칙 고유 식별자는 거부한다.

Simulation 확정 뒤 최신 상태 사본에는 미리보기에서 사용한 효과 묶음의 적용 기록이 있어야 한다. 적용 기록과 더 최신 개정 번호를 확인한 뒤에만 수확 재고 원장의 실제값을 예상값과 대조한다. API 사본 변환기 검증은 실제 HTTP 연결, Unity Scene 배치 또는 Game View 검증을 대신하지 않는다.

## D-091 다품목 Unity 모판은 서버가 보장하는 연결 깊이를 품목별로 구분한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-081의 개별 배치 객체, D-085의 수요·예약·이행·소비 분리, D-089의 서버 재조회와 D-090의 생산 효과 비재계산 원칙을 감자 외 품목과 운영 API에 확장함

Unity 모판은 서버 공개 상품 API가 반환하는 품목을 개수 제한 없이 투영하되, 모든 품목이 생산 규칙까지 연결됐다고 가정하지 않는다. 현재 감자는 생산 Fixture에서 마트 표현까지 연결된 품목이고, 쌀·양파는 공개 상품·가격·판매 가능 수량과 마트 업무만 연결된 품목이다. 공개 상품에 생산 품목 기준 고유 식별자가 없으면 이름이 같다는 이유로 생산 Lot·Cargo·재고 계보를 자동 연결하지 않는다.

마트 주문 요청은 주문 확정이 아니라 비구속 구매 의향이다. Unity는 인증, 개인정보 동의 증적, 수량, 현재 안내 버전과 명시적 확인을 미리보기에서 점검할 수 있지만 서버가 현재 상품 상태와 권한을 다시 검증하고 기록한다. 등록 응답 뒤 같은 주문 의향 상세를 다시 조회해야 Unity 상태를 갱신한다.

비구속 주문 의향은 재고 차감·예약, 결제, 피킹·포장, 배송 또는 계약을 만들지 않는다. Unity API 사본은 기존 한국어 JSON 필드명, API 경로와 상태 코드를 변경하지 않는다. Client 인터페이스와 결정적 테스트는 실제 UnityWebRequest, 인증 세션, 운영 DB 저장, Scene 또는 Game View 검증을 대신하지 않는다.

## D-092 Unity 운영 API Client는 공통 전송 계층과 인증 경계를 재사용한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-073의 서버 재조회 원칙과 D-091의 다품목·비구속 주문 의향 경계를 실제 Unity 프로젝트 Client에 적용함

도심마트 전용 Client는 별도의 HTTP 전송 구현을 만들지 않고 실제 Unity 프로젝트의 공통 `IUnityApiClient`와 `UnityWebRequestApiClient`를 사용한다. 공개 상품 목록은 인증 없이 조회하고, 개인 주문 의향 등록과 상세 재조회는 인증 토큰을 요구한다. API 경로, 기존 한국어 JSON 필드, 상태 코드는 변경하지 않는다.

운영 HTTP 실패를 Simulation 자료로 대체하지 않는다. Simulation 표본과 운영 서버 자료는 조립 시점부터 명시적으로 분리하며, 운영 Client의 오류는 사용자에게 설명 가능한 실패 상태로 전달한다. Client 집중 테스트와 Editor 재컴파일은 실제 서버 기동, 유효 인증 토큰, 운영 DB 기록, Scene·Play Mode 또는 Game View 검증을 대신하지 않는다.

## D-093 게임 세계 Simulation 서버를 실제 운영 전 예행연습 서버로 사용한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-027의 물리 분리와 D-092의 Unity Client 인증 경계를 유지하면서 상시 서버 운영 구성을 두 서버로 단순화함

`Ssalddel.Simulation.Server`는 게임 세계의 scenario·seed·session·가상 시간·save·replay 권위를 유지하면서 생산·소비·운송·창고·시장 업무 규칙을 실제 운영 전에 반복 검증하는 예행연습 서버 역할을 함께 맡는다. 별도의 세 번째 상시 예행연습용 운영 서버를 기본 구조로 두지 않는다. `Ssalddel`은 WebApp·MAUI와 Unity 운영 기능이 사용하는 실제 운영 서버다.

Unity는 두 서버 주소와 Client 타입을 분리한다. 공개 상품·공공데이터·실제 사용자 의향처럼 운영 API를 읽거나 쓰는 Client는 운영 서버만 사용하고, WorldTick·가상 자원·Simulation Confirm은 예행연습·게임 세계 서버만 사용한다. Simulation 결과와 가상 식별자는 운영 원장으로 자동 승격하지 않으며, 두 서버가 공통 의미를 가져야 할 때는 명시적 계약·고유 식별자·데이터 계보와 호환성 테스트를 사용한다.

예행연습 서버의 검증 근거를 운영 배포 판단에 사용하려면 영속 save·replay, 기준 scenario, 실패·재시도 시험과 운영 계약 호환성 검증을 단계적으로 갖춰야 한다. 운영 배포 직전의 동일 빌드 기동·migration·설정 검증은 별도 일회성 배포 검증으로 남기며 세 번째 상시 권위 서버로 취급하지 않는다.
