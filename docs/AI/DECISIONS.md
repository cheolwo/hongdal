# Ssalddel AI Shared Decisions

> GPT Chat과 Codex가 공통으로 따라야 하는 장기 결정을 기록한다. 현재 진행 상황은 [CURRENT_WORK.md](CURRENT_WORK.md)에 둔다. 기존 결정을 바꿀 때는 원문을 삭제하지 않고 상태를 `Superseded`로 바꾼 뒤 대체 결정 ID를 연결한다.

## 상태 코드

- `Accepted`: 현재 적용하는 결정
- `Superseded`: 후속 결정으로 대체됨
- `Deprecated`: 더 이상 새 작업에 적용하지 않지만 호환성 때문에 기록을 유지함

## 빠른 색인

| 결정 범위 | 중심 주제 |
| --- | --- |
| D-001~D-026 | Unity World 기본 경계, 데이터·관점·표현 분리 |
| D-027~D-050 | 운영 서버·Simulation 서버 분리, 공공데이터와 상품·공간 근거 |
| D-051~D-085 | World Projection, 규칙·UI 대장, Synty·URP 표현 파이프라인 |
| D-086~D-113 | 타일·영역·업무 객체와 서버 파생 DB |
| D-114~D-125 | `SimulationWorldShell`, 1·3인칭, 시야 기반 L2 스트리밍 |
| D-126~D-138 | 생존·카드·팀 관전·전투·전술 분대 |
| D-139 이후 | 구조 안정화와 후속 리팩토링 결정 |

기존 문서가 참조하는 결정 제목 앵커는 유지한다. 상태를 바꾸거나 대체할 때에는 기존 본문을 삭제하지 않고 후속 결정 번호를 연결한다.

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

## D-094 Simulation 서버는 수집된 공공데이터 DB를 읽기 전용으로 공유한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-027의 두 서버 물리 분리와 D-093의 게임 세계·예행연습 서버 역할에서 공공데이터 저장소만 공유하는 예외를 명시함

운영 서버가 외부 제공처에서 수집한 공공데이터는 Simulation 규칙의 공통 입력 근거이므로 `Ssalddel.Simulation.Server`도 같은 MySQL DB와 농수산 공공데이터 스키마를 조회한다. 자료를 복제한 별도 Simulation 공공데이터 DB를 기본 구조로 두지 않는다.

공유 범위는 공공데이터 전용 `AgriculturalFisheriesDbContext`의 읽기뿐이다. Application은 조회 포트를 정의하고 `Ssalddel.Simulation.Persistence`가 EF 조회를 구현하며 Server는 이 전용 연결 모듈만 조립한다. Server가 운영 `Ssalddel.Infrastructure`나 `Ssalddel.Domain`을 직접 참조하지 않도록 유지한다. 운영 업무용 `SsalddelContext`, 공공데이터 수집기, migration과 초기화 작업은 등록하지 않으며 조회 추적과 `SaveChanges`를 차단한다. 배포 환경에서는 같은 DB를 가리키는 별도 읽기 전용 계정을 사용해 응용 코드 밖에서도 쓰기를 막는다.

Simulation session, WorldTick, 가상 재고, save와 replay는 공공데이터 DB에 저장하지 않는다. 공공데이터 관측은 생산·시장 규칙의 입력 근거가 될 수 있지만 실제 주문·재고·계약 완료 사실이나 Simulation 결과를 뜻하지 않는다. Unity 표현 또한 공공데이터의 출처·기준 시각·단위·결측 여부를 유지해야 하며 화면 변화만으로 운영 사실을 만들지 않는다.

## D-095 운영자 전용 재고 Shelf는 주소 지정 가능한 피킹 위치 단위다

- 상태: `Accepted`
- 결정일: 2026-08-12

Unity의 운영자 전용 재고 Shelf는 창고 전체나 상품 한 종류가 아니라 운영자가 주소로 접근할 수 있는 적재·피킹 위치 한 곳을 표현한다. 같은 위치의 여러 재고 항목은 한 Shelf 상태로 묶을 수 있으며, 한 상품이 여러 위치에 나뉘면 여러 Shelf로 표현한다.

Shelf 수량은 창고 기준 원장의 상태 사본에서 읽고 Unity가 재계산하거나 변경하지 않는다. 위치가 없는 재고는 임의 Shelf로 만들지 않으며 관련 피킹 작업은 `LocationUnmapped`로 남긴다. 실제 피킹 완료는 Unity 이동이나 애니메이션이 아니라 서버 명령 뒤 기준 원장 재조회로 확인한다.

## D-096 SimulationWorldShell의 플레이어 카메라는 Presentation 전용 입력 모듈이다

- 상태: `Accepted`
- 결정일: 2026-08-12

`SimulationWorldShell`의 실제 사용자 탐색은 Scene View 편집 카메라가 아니라 `PlayerCameraRig/CameraPivot/Main Camera` 계층과 Input System 기반 전략 카메라가 담당한다. 이동·회전·Zoom과 배치 객체 초점은 Presentation 상태이며 서버 원장, Simulation 상태, `WorldTick`, 상태 버전이나 업무 완료를 바꾸지 않는다.

자유 탐색과 배치 객체 초점은 명시적으로 구분한다. 배치 객체 선택과 `ESC` 복귀는 별도 단계로 연결하고 UI 위의 포인터 입력은 World 선택으로 전달하지 않는다.

## D-096 일반 타로를 경영 게임의 기본 덱으로 두고 학당 카드는 선택형 확장으로 분리한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-070의 명시적 턴 마감, D-078의 카드 모판 승격과 기존 학당 카드 구현을 일반 타로 기반 지역 경영 게임으로 확장함

`살뜰 아르카나`의 기본 덱은 특정 학당 해석을 전제로 하지 않는 일반 타로 원형과 게임용 경영 해석으로 구성한다. 기존 `learning:hongik.*` 카드는 고유 식별자와 저장 의미를 유지하는 선택형 학당 콘텐츠로 남기며, 같은 타로 원형의 일반 게임 카드는 별도 고유 식별자·해석 개정 번호·효과 규칙 개정 번호를 사용한다.

타로 원형, 게임용 해석, 시나리오 덱, 개별 카드 제안, 경영 대응과 배치 객체 반응 관점별 조회 결과를 분리한다. 카드 선택은 다음 날 적용할 조건과 대응을 열 뿐 생산·운송·입고·재고·판매를 직접 확정하지 않는다. 실제 업무 변화는 기존 시뮬레이션 업무의 미리보기·명시적 확정·기준 상태 사본 재조회로만 적용한다.

첫 뼈대는 여제·전차·정의·절제 네 장, 결정론적 3장 제안, 한 장과 대응 선택, O6 실제 World 배치 검증 완료 객체 7개의 영향 미리보기까지만 닫는다. 지역 신뢰와 환경·시설 상태를 임의의 단일 점수로 만들거나 타로 78장 전체와 최종 밸런스를 먼저 확정하지 않는다.

## D-097 타로 규칙은 기존 업무 규칙에 보정선을 제공하는 상위 시뮬레이션 규칙이다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-082의 자원 효과 묶음, D-083의 규칙 계층과 D-096의 일반 타로 기본 덱을 실제 시뮬레이션 계산 순서로 구체화함

활성 타로 카드의 방향과 경영 대응은 생산·소비·운송·창고·시장·시설·시간 규칙보다 상위에서 적용할 하위 규칙과 플러스·마이너스 보정선을 결정한다. 각 보정선은 대상 연결 지점, 계산 방식, 값, 단위, 허용 범위, 활성 턴, 원천 카드와 상위 규칙 개정 번호를 보존한다. 정방향과 역방향은 같은 값의 부호를 자동 반전하지 않고 각각 독립된 기회·부담 보정 묶음으로 관리한다.

기존 하위 업무 규칙은 허용하는 보정 연결 지점과 범위를 명시하고, 타로 보정을 소비해 기준 후보와 다른 최종 후보를 계산한다. 등록되지 않은 연결 지점, 단위 불일치, 호환되지 않는 규칙 개정 번호와 허용 범위 초과는 실패 상태로 닫는다. 카드가 활성화되지 않았을 때는 기존 규칙 결과와 완전히 같아야 한다.

타로 상위 규칙은 수량 보존, 음수 재고 금지, 차량·창고 용량, 단위 일치, 권한, 현재 상태 전이와 Simulation·Operational 분리를 우회할 수 없다. 이 불변 안전 조건은 보정 적용 뒤에도 다시 검증한다. 미리보기는 보정 전 기준 후보·보정선·보정 후 최종 후보를 함께 반환하고 기준 원장을 바꾸지 않으며, 실제 효과는 기존 Confirm·Task·WorldTick과 기준 상태 사본 재조회 경계에서만 적용한다.

## D-098 일반 타로 뽑기는 seed·턴·덱 개정 번호·선택 이력으로 결정한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-070의 명시적 턴 마감, D-096의 일반 타로 기본 덱과 D-097의 상위 규칙 적용 출처를 결정적 제안 계약으로 구체화함

일반 타로의 첫 시작 덱은 여제·전차·정의·절제 각 3복사본, 총 12칸으로 구성한다. 카드 원형의 고유 식별자, 덱 안의 카드 복사본 고유 식별자와 특정 턴의 제안 고유 식별자를 분리한다. 같은 카드 종류가 한 번의 제안에 중복되더라도 복사본과 제안 식별자는 충돌하지 않는다.

시뮬레이션 서버는 시나리오 seed, 턴 번호, 덱 개정 번호와 이전 타로 제안 선택 이력으로 세 장과 각 정·역방향을 결정한다. Unity 프레임 순서, 클라이언트 시각이나 클라이언트 난수는 결과 권위가 아니다. 턴 마감 요청은 서버가 제안한 고유 식별자·카드 고유 식별자·방향이 모두 일치해야 하며, 선택된 복사본·제안·방향은 턴 기록과 save·replay hash에 보존한다.

기존 학당·문화 카드 목록은 호환 경계로 유지하고 일반 타로 제안 묶음과 구분한다. 카드 선택은 다음 턴의 상위 규칙 조건을 활성화할 뿐 하위 업무 원장을 직접 변경하지 않는다.

## D-099 타로 객체 관계와 현재 강조 상태를 분리한다

- 상태: 확정
- 결정일: 2026-08-12
- 관계: D-078의 모판 승격, D-096의 첫 O6 객체 범위와 D-097의 타로 상위 규칙 경계를 객체 반응 미리보기로 구체화함

타로 카드와 배치 객체의 영향 가능 관계는 개정 번호가 있는 서버 구성 대장으로 관리한다. 첫 구성 대장 `integrated-seedbed:o6.r1`은 O6 실제 World 배치 검증을 마친 감자 수확 상자, 농장 출하 상자, 배송 차량, 공용 화물 Pallet, Hub 입고 Gate, 도심마트와 집단수요 Cart Table 일곱 객체만 포함한다. Unity의 이름 검색, 태그 추측이나 화면 배치 순서는 이 관계의 권위가 아니다.

현재 강조 상태는 정적 관계와 별도로 현재 Simulation 세션의 수확 배정, 화물운송, 물류 이동, 마트 공급과 공동주문 상태에서 계산한다. 카드와 연결되어 있어도 현재 관련 상태가 없으면 영향 가능 객체로는 반환하되 강조하지 않는다. 근거가 된 상태 고유 식별자와 상태 부재 사유를 함께 반환해 같은 서버 상태에서 재현할 수 있게 한다.

객체 반응 미리보기는 카드 제안과 현재 상태를 읽는 후보 조회이며 세션 개정 번호, 기준 원장, 카드 선택 기록이나 운영 원장을 변경하지 않는다. 객체의 색 변화, 차량 이동 또는 Gate 표현은 생산·운송·입고·재고 업무의 확정 근거가 아니며 실제 변화는 기존 Confirm·Task·WorldTick 뒤 기준 상태 사본 재조회로만 확인한다.

## D-100 공간 World는 고정 Tile·Area·AreaSet과 통계 구성 대장으로 반복 생성한다

- 상태: `Accepted`
- 결정일: 2026-08-13

EPSG:5186 고정 격자의 L0 8km, L1 2km, L2 500m Tile을 공간 Layer 처리와 cache 단위로 사용한다. 법정동과 Farm·Town·Hub는 Tile을 참조하는 Area, 이들과 회랑은 AreaSet으로 구성한다. 기존 Synty `Composition` 세트는 Area가 아니라 Area 안의 교체 가능한 경관 조각이다.

DEM·WorldCover·법정동 경계는 배치 가능 위치, 환경부 세분류 면적 통계는 행정구역 전체의 구성 목표로만 사용한다. 세분류 공간 SHP가 없는 논·밭·수종 배치는 `StatisticallyAllocated`이며 실제 위치로 표현하지 않는다. 후보 면적이 목표보다 적으면 새로 만들지 않고 `UnresolvedTargetArea`로 남긴다.

`PhysicalElevation`은 경사·수계·배치 판정, `VisualElevation`은 Renderer 높이 과장에만 사용한다. 면적 배분 결과와 Synty 경관 개체 계획을 별도 산출물로 유지하고, Halo·세계 좌표 seed·수작업 Reference Tile·실제 렌더링 비용을 중간 검증한다. Unity 표현과 LOD 전환은 공간 사실이나 Simulation·운영 상태를 변경하지 않는다.

## D-101 행정구역별 건물은 출처별 DB 원장을 먼저 구축하고 World에 투영한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-100의 Area 경관 입력을 실제 건물 자료로 확장함

행정동·법정동·건물을 Unity Scene이나 Synty Prefab에서 먼저 정의하지 않는다. 행정안전부 행정기관–관할 법정동 관계, 건축HUB 건축물대장 속성, VWorld GIS건물통합정보 도형을 서로 다른 원본과 기준일로 DB화하고 검증된 관점별 조회 결과만 World가 읽는다.

법정동코드와 행정동코드는 같은 식별자가 아니며 기준일별 다대다 관할 관계를 유지한다. 건축물대장 PK, VWorld feature ID와 도로명주소 건물관리번호도 하나의 값으로 합치지 않고 근거·연결 방식·신뢰 수준을 가진 identity link로 연결한다. 행정동 경계가 있으면 건물 대표점의 공간 포함 판정을 우선하고, 관할 관계가 1:N이면 법정동코드만으로 행정동을 확정하지 않는다.

World 기본 projection은 소유자·상세 호수·주택가격·개인 연락처를 포함하지 않는다. 건물 이름이 비어 있어도 주용도·구조·층수·높이·면적·footprint 근거로 집계하며, 원본 도형이 없거나 지역 연결이 모호하면 임의 배치하지 않고 미해결 상태로 남긴다. Synty Prefab과 LOD/HLOD는 건물 원장의 표현 계획일 뿐 건물 사실이나 행정구역 배정을 변경하지 않는다.

## D-102 건축물 공식 주용도와 상위 경관 Category를 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-101의 건물 원장을 행정동별 검색·집계와 D-100의 경관 입력으로 구체화함

건축물대장의 공식 주용도 코드·이름은 관측 원문으로 보존하고, 주거·농업·물류·상업 같은 상위 Category는 별도 규칙 결과로 저장한다. Category는 건물 이름이나 Prefab 이름으로 추정하지 않으며 분류 방법, 근거 수준, 규칙 개정 번호와 분류 시각을 기록한다. 공식 주용도가 없으면 `unresolved`, 공식 주용도는 있으나 현재 규칙이 다루지 않으면 `other`로 남긴다.

행정동별 구성은 건물 원문을 직접 덮어쓰는 열이 아니라 기준시점별 집계 projection으로 생성한다. 집계에는 건물 수, 건축면적, 연면적, 이름·도형 연결·미해결 수와 재현 가능한 hash를 남긴다. Synty `WorldRoleCode`는 표현 후보를 좁히는 값일 뿐 실제 시설의 업무 역할, 물류 Hub 자격이나 Simulation 상태를 확정하지 않는다.

## D-103 건축물 형태의 공식값·단순 계산값·Synty 표현값을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-102의 건물 Category를 층수·밀도 기반 시각 구성으로 확장함

건축물대장의 공식 지상층수·건폐율·용적률은 관측값으로 보존한다. 공식 값이 없을 때 높이와 용도별 층고로 구한 층수는 `추정 지상층수`, 건축면적과 연면적을 대지면적으로 나눈 값은 `단순 건폐 비율`과 `단순 연면적 대지 비율`로 부르며 법정 건폐율·용적률로 노출하지 않는다.

`건축물형태Profile`은 공식값과 파생값·근거 종류·규칙 개정 번호·hash를 가진다. `건축물시각구성계획`은 표현 층수, 중간층 반복 수, 대지 점유·주변 여백·LOD 등급과 의미 기반 시각 Family만 가지며 항상 표현 전용이다. Synty Prefab 경로나 원본 이름은 업무·공간 원장에 저장하지 않고 `VisualKey/Catalog`에서 해석한다.

## D-104 건물 안의 상호는 공개 인허가 사업장과 보수적인 주소 연결로 표현한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-101의 건물 원장과 D-102의 용도 Category에 공개 사업장 구성을 연결함

건물 안의 사람이나 사업자를 추정하지 않고 행정안전부 지방행정인허가 자료에 공개된 사업장을 별도 원장으로 수집한다. 정규화 원장에는 사업장명, 인허가 업종·업태, 영업상태, 공개 주소·좌표, 인허가·폐업·최종수정 시각과 원본 계보를 저장한다. 대표자명·전화번호·사업자등록번호는 건물 World 구성 목적의 기본 projection에 저장하지 않는다.

사업장 주소와 건물 주소가 정확히 일치하고 건물 후보가 하나일 때만 파생 연결한다. 후보가 여러 개이거나 주소·건물 자료가 부족하면 미확정으로 남긴다. 주소 연결은 현재 입주·소유·임대 관계의 공식 증거가 아니며 공개 인허가 자료도 모든 사업체의 전수명부가 아니다. Unity는 검증된 공개 상호명이나 업종 밀도를 표현할 수 있지만, 이를 영업 자격·재고·공급 의사·물류 Hub 역할이나 실제 사람의 활동으로 확정하지 않는다.

## D-105 공유 공공데이터 DB와 Simulation World 파생 관계 DB를 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-100의 World 생성, D-101~D-104의 행정구역·건물·사업장 원장을 Simulation 저장 경계로 구체화함

운영 서버의 공공데이터 수집기가 공식 원본과 정규화 결과를 공유 공공데이터 DB에 저장한다. 운영 서버와 Simulation 서버는 이 DB를 함께 읽되 Simulation 서버는 SELECT 전용 연결만 사용하고 수집·migration·`SaveChanges`를 수행하지 않는다.

Simulation 서버가 공공데이터·공간 규칙·시나리오·Synty 구성 대장을 결합해 만든 node, 관계와 시각 배치 계획은 별도 `SimulationWorldDerived` DB에 불변 파생 실행본으로 저장한다. 각 실행본은 원본 DB·자료 개정·SHA-256, Recipe·규칙·VisualCatalog 개정, seed, 입력 fingerprint와 출력 hash를 보존한다. 같은 입력과 결과는 재사용하고 같은 실행 식별자에 다른 내용은 거부한다.

파생 DB의 `VisualKey`와 배치 계획은 표현 전용이며 Prefab 경로·원본 GUID를 저장하지 않는다. 파생 관계는 운영 계약·재고·배차·사업장 입주나 행정구역 공식 사실을 확정하지 않고 운영 DB와 공유 공공데이터 DB로 역승격되지 않는다.

새 파생 DB의 물리 표·열·외래키·인덱스와 migration 이력 표는 한국어 업무 의미를 먼저 사용한다. 다만 C# 속성명, 설정 키, API·JSON 계약, `SHA256`·`UTC`·`DB`·좌표축 같은 표준 기술 표기는 호환성과 의미 보존을 위해 유지한다. 이미 운영 중인 업무 DB와 공유 공공데이터 DB의 물리 스키마는 이 결정으로 일괄 변경하지 않는다.

영역별 건물 표현은 공유 공공데이터 DB의 건축물·공개 인허가 사업장을 원본 레코드 단위로 참조하고, `영역 포함 건물`, `건물 연결 공개 사업장`, `건물 배치 계획`을 서로 분리한다. 관측 도형·관측 대표점·영역 통계 구성·시나리오 배치 근거를 구분하며, 사업장–건물 주소 연결은 실제 입주나 소유 관계로 승격하지 않는다. 공개 상호·업종·영업상태는 표현 후보에 사용할 수 있지만 대표자·연락처·사업자등록번호는 파생 World 원장에 투영하지 않는다.

그래픽 표현 계획은 건물 배치와 다시 분리한다. 파생 DB에는 실제 자산 파일 경로 대신 질감 세트·재질 변형·색조·배경·조명·시간대의 의미 키와 그림자·LOD·품질 정책을 저장한다. Unity 구성 대장이 키를 실제 자산으로 해석하며 원본 Synty Prefab·Material을 수정하지 않는다. 그래픽 계획은 항상 표현 전용이고 공간·건물·인허가·업무 사실을 바꾸지 않는다.

## D-106 Unity 공간 실행과 Synty 경관 실행을 독립 파이프라인으로 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-105의 단일 결합 실행본 중 Synty·그래픽 결합 부분을 대체하고, 공유 공공데이터 DB와 공간 DB 분리는 유지함

공공데이터에서 Unity 공간 DB로 가는 파이프라인은 원본 계보, Tile·Area·AreaSet, 공간 관계, Unity 좌표 변환, Terrain·Mask와 배치 기준점까지만 생성한다. 새 `SchemaVersion 2` 공간 실행에는 `VisualCatalogRevision`, 그래픽 표현 계획과 `VisualKey` 시각 배치를 저장하지 않으며 공간 fingerprint에도 포함하지 않는다. 기존 `SchemaVersion 1` 결합 실행과 표는 읽기 호환을 위해 보존한다.

Synty 경관 처리는 별도 `Synty경관JobShell`이 담당한다. Job은 공간 실행 고유 식별자와 공간 출력 SHA-256, Tile·Area·AreaSet 범위, 경관 규칙, Synty 구성 대장, URP 표현 대장, seed, 대상 플랫폼과 품질 단계를 입력으로 봉인한다. 공간 출력 hash나 AreaSet이 저장본과 다르면 실행을 거부하며 Synty 실행은 공간 원장을 수정하거나 역갱신할 수 없다.

Synty 실행은 그래픽 표현 계획, 의미 기반 `VisualKey` 배치와 배치 거부 사유를 별도 불변 원장에 저장한다. Terrain·Mask·기준점이 없으면 `(0,0,0)`이나 임의 위치를 만들지 않고 `UnitySpatialArtifactMissing`으로 보류한다. Prefab·Material·Shader Graph·HLOD처럼 Unity AssetDatabase가 필요한 결합은 후속 Unity BatchMode 작업자가 수행하며, 원본 Synty Prefab·Material·`.meta` GUID는 계속 수정하지 않는다.

## D-107 Simulation 상태는 의미 기반 렌더링 의도를 거쳐 URP 표현으로 번역한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-106의 정적 Synty 경관 실행 위에 현재 Simulation 상태의 동적 표현을 합성함

Simulation Domain은 `Bloom`, Shader 값, Material 경로 같은 URP 구현을 결정하지 않는다. 확정된 Simulation 상태 사본을 별도 Projector가 `Simulation렌더링의도`로 번역하고, 표현 합성 규칙과 버전이 있는 URP 표현 대장이 플랫폼 Capability에 맞는 의미 기반 Profile 키를 선택한다. Unity 구성 대장이 이 키를 실제 `MaterialPropertyBlock`, Volume, Renderer Feature, Particle과 Animation에 연결한다.

렌더링 의도는 `Environment / Surface / Lighting / ObjectState / Attention / Fx / Animation` Channel, World·AreaSet·Area·Tile·Route·Facility·Object 범위, 우선순위, 수명, 근거, 원본 상태·Session 개정 번호를 보존한다. 같은 대상과 Channel의 충돌은 높은 우선순위, 동일 우선순위는 고유 식별자 순으로 해결하고 억제 근거를 남긴다. 기간과 개정 수명을 지난 의도는 제거하며 확인한 `OneShot` 의도는 재조회에서 반복하지 않는다.

Runtime 표현 상태 사본은 공간 실행과 Synty 시각 실행의 ID·출력 SHA-256, Simulation·World 개정, 렌더링 규칙·URP 대장·Capability 개정과 합성 결과로 결정적 hash를 만든다. 도로 표면이나 공간 근거가 없으면 흙길 먼지 같은 효과를 꾸며내지 않고 Fallback 사유를 남긴다. 표현 Pipeline은 Simulation 상태를 수정하지 않고 운영 상태를 입력으로 받지 않으며 Animation·Particle·URP 효과 완료는 업무 완료나 Command·WorldTick을 발생시키지 않는다.

## D-108 공간 규칙과 Simulation 규칙은 개정 가능한 객체 표현 결합 원장에서 만난다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-106의 공간·Synty 분리와 D-107의 Runtime 표현 사이에 객체별 해석 계보를 추가함

공간 node를 Synty Prefab이나 URP 수치에 직접 연결하지 않는다. 공간 사실을 판정하는 `공간규칙Metadata`, Simulation 상태를 설명하는 `Simulation규칙Metadata`, 두 규칙을 객체 의미·범위·우선순위·기본 구성 키·동적 표현 의도 묶음 키로 연결하는 `객체표현결합규칙`을 별도 대장으로 관리한다.

규칙 상태는 `Draft / Active / Retired`로 구분한다. 아직 확정되지 않은 Simulation 규칙은 초안으로 축적할 수 있지만 활성 해석에는 참여하지 않는다. Simulation 규칙이 없거나 초안인 동안에는 활성 공간 규칙만 참조하는 결합 규칙이 기본 외형을 제공할 수 있다. 이후 Simulation 규칙을 활성화해도 공간 실행본을 수정하지 않고 새로운 규칙 대장 개정과 해석 실행본을 만든다.

객체 표현 해석 실행은 공간 실행 ID·출력 SHA-256, 선택적 Simulation 세션·개정·WorldTick, 규칙 대장 개정, 입력 fingerprint와 출력 hash를 봉인한다. 객체별 결과에는 적용한 공간·Simulation·결합 규칙, 기본 구성 키, 동적 표현 의도 묶음 키, 미충족 처리와 근거를 저장한다. 결과는 항상 표현 전용이며 같은 대장 개정 또는 해석 실행 식별자에 다른 내용을 덮어쓰지 않는다. 의미 키에는 Prefab·Material 경로를 저장하지 않고 Unity 구성 대장이 마지막에 해석한다.

## D-109 평창군 Unity 공간 표현은 전체 원장을 보존하고 건물 종류별 하나로 축약한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-105의 공유 공공데이터 원장과 D-108의 객체 표현 결합 사이에 제한된 대표 Projection을 추가함

평창군 건축물·공개 사업장 원본은 공유 공공데이터 DB에 전수 보존한다. Unity 공간 실행과 객체 표현 규칙 편집에는 모든 건물을 그대로 복제하지 않고 건물 용도 Category마다 대표 건물 node 하나만 사용한다. 이는 원본 삭제·통계 축소·공식 건물 통합이 아니라 화면과 규칙 실험을 위한 표현 Projection이다.

대표군은 건물 용도 Category로 만든다. 각 종류에서 이름·면적·층수·높이·주소가 상대적으로 충실한 건물을 먼저 선택하고 같은 조건이면 고정 seed hash로 정한다. 같은 원본·규칙·seed에서는 입력 순서와 무관하게 같은 대표를 선택한다. 대표 node는 `대표군코드`, `대표원본건수`, `대표순위=1`을 보존하며 대표원본건수의 합은 전체 후보 수와 일치해야 한다.

공개 인허가 사업장은 선택된 대표 건물과 검증된 주소·도형 관계가 있는 항목만 표현 후보로 가져온다. 대표 건물이나 Synty Prefab 하나를 실제 회사 한 곳, 입주 관계 또는 회사 수와 동일하게 해석하지 않는다. 사용자가 상세 원본을 조회할 때는 대표 node가 아니라 공유 공공데이터 원본 레코드와 계보를 사용한다.

첫 화면 검증을 위해 종류별 대표에는 고정 seed로 `Idle / Operating / Loading / Maintenance` 중 하나의 시험 상태를 배정할 수 있다. 이 규칙은 `ScenarioFixtureBuildingActivity`이며 실제 회사 영업·작업 상태나 공공데이터 관측으로 승격하지 않는다. 건물 종류 공간 규칙과 시험 Simulation 규칙이 결합한 결과만 기본 구성 키와 동적 표현 의도 묶음 키로 전달한다.

## D-110 Simulation 서버는 운영 서버의 컨테이너 관례를 따르되 DB 권한과 migration을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-105의 공유 공공데이터 DB와 Simulation World 파생 DB 경계를 배포 구성으로 구체화함

Simulation 서버는 운영 `Ssalddel` 서버와 같은 .NET 10 다단계 이미지, 비루트 실행, `Section__Key` 환경 변수 주입, `live`와 `ready` 상태 확인 관례를 사용한다. 실행 모드는 항상 `SsalddelExecution:Mode=Simulation`이어야 하며 기본 포트와 컨테이너 이름은 운영 API와 분리한다.

운영 업무 DB의 일반 연결 문자열을 Simulation 컨테이너에 재사용하지 않는다. 공유 공공데이터 DB에는 `SELECT`만 허용한 전용 계정을 사용하고, 파생 관계·규칙·표현 해석 실행본은 별도 Simulation World DB의 전용 쓰기 계정으로 저장한다. 개발용 fallback 연결은 Container 환경에서 사용하지 않으며 실제 secret은 source·Compose 기본값·로그에 기록하지 않는다.

상태 확인에서 `live`는 프로세스 응답만, `ready`는 설정으로 활성화된 두 DB의 연결 가능성을 검사한다. DB schema migration은 API host 시작 시 자동 실행하지 않는다. 별도 명령이나 배포 단계에서 명시적으로 적용해 읽기 전용 공공데이터 계정으로 migration을 시도하거나 여러 host가 동시에 schema를 변경하지 않도록 한다.

## D-111 Simulation World 파생 DB는 업무 규칙의 관계와 계보를 집결한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-108의 객체 표현 결합 전에 시설 의미와 실행 가능한 업무 규칙의 연결을 명시함

공간 node에 창고·마트·음식점 같은 업무 의미를 곧바로 고정하지 않는다. `시설 의미 → 시설 기능 → 업무 Simulation 규칙 → 객체–규칙 연결 → Scenario 규칙 묶음`을 독립된 개정 대장으로 저장하고, 각 시설 의미가 공간 node와 어떤 근거로 연결되었는지 보존한다. 첫 평창군 Farm·Hub·Mart·Restaurant는 실제 영업 관측이 아니라 `Scenario` 근거다.

파생 DB는 규칙 식별자·개정, 영역, 상태, Engine 키, 입력·출력 계약, 결정적 실행 여부와 Parameter를 저장하지만 규칙 실행 코드를 복제하지 않는다. 현재 주문·재고·화물·WorldTick은 Session 원장이 소유하며, 규칙 대장과 Synty·URP 표현 완료는 업무 완료나 운영 효과를 만들지 않는다.

같은 규칙 대장 개정은 공간 실행 ID·공간 출력 SHA-256·규칙 대장 SHA-256이 같을 때만 재사용한다. 시설·기능·규칙·연결·Scenario 항목은 상위 대장에 외래키로 묶고, 참조하지 않는 기능이나 규칙, 중복 식별자, 운영 규칙 혼입은 저장 전에 거부한다. 객체 표현 결합은 이 대장을 읽어 의미 기반 구성·렌더링 의도를 만들고 실제 Prefab·Material 결합은 독립 Synty·URP 파이프라인이 마지막에 수행한다.

## D-112 Unity UI 구현 전 Figma 근거 UI 기획 원장을 둔다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-111 업무 규칙 대장과 Unity ScreenModel 사이의 기획 경계를 추가함

기존 Figma의 01~09 역할 서비스 계층, 주문자의 발견→비교→참여→준비 판단, 공통 홈의 둘러보기·참여 경계·역할 선택을 UI 정보 구조의 설계 근거로 사용한다. Figma 파일·node 식별자와 확인한 구조를 계보로 저장하되 Figma를 서버 권한이나 업무 상태의 권위로 사용하지 않는다.

UI 기획 대장은 시설·역할·업무 단계별 화면 영역, 정보 항목, 상태 표현, 행동 후보와 업무 규칙 연결을 저장한다. 픽셀 좌표·Prefab·Material·색상 수치는 저장하지 않고 Unity가 World HUD·선택 정보판·업무 상세판으로 다시 구성한다. Confirm 후보는 Preview·명시적 확인·기대 개정 번호·서버 Command 키를 모두 요구하고, Command 성공 뒤 같은 원장을 재조회해야 한다.

## D-113 UI는 규칙 식별자가 아니라 객체–업무 규칙 연결을 통해 조립한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-111의 객체–업무 규칙 연결과 D-112의 UI 기획을 하나의 검증 가능한 계보로 연결함

UI 화면이 규칙 식별자를 별도 대응표로만 참조하면 규칙의 시설이나 시설 기능이 바뀌어도 잘못된 화면 연결이 남을 수 있다. `SchemaVersion 2` UI 기획은 원본 객체–업무 규칙 연결 고유 식별자와 시설 기능 코드를 함께 저장하고, 화면 시설·기능·규칙 개정의 일치를 검증한다. 활성 원본 연결은 UI 기획에서 빠지거나 중복될 수 없다.

지역별 UI 구성 차이는 Application의 UI 기획 조립기가 담당한다. Job Shell은 업무 규칙 대장 읽기, 조립기 호출과 불변 원장 저장만 조율하며 평창군 화면 정의를 직접 소유하지 않는다. Unity는 이 원장을 ScreenModel로 투영하지만 현재 Session 상태·권한·Command 성공 여부를 UI 기획 DB에서 만들지 않는다.

## D-114 Tile과 경관 완결 영역의 책임을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-108 공간–Simulation 규칙 결합과 D-111 업무 규칙 집결 전에 반복 가능한 공간 완결 범위를 고정함

L0 8km·L1 2km·L2 500m Tile은 공간 원본 절단, fingerprint, 캐시와 부분 재생성의 기술 단위다. 미술·Simulation 규칙·UI·Unity Runtime을 사람이 함께 검수하는 단위는 인접 L2 타일 2×2를 묶은 1km×1km `경관 완결 영역`으로 둔다. Area는 법정동·Farm·Hub·Town 의미 범위이고 AreaSet은 완결 영역·Area·회랑을 참조하는 시나리오 묶음이므로 서로 대체하지 않는다.

첫 완결 영역은 대관령 Farm으로 고정한다. 전체 평창군 타일을 먼저 산출하지 않고 해당 L2 네 타일과 필요한 L1·L0 상위 타일만 처리한다. 완결 상태는 원자료, 물리 공간, 공간 의미, Scenario 규칙, 경관 계획, UI 계획, Unity Runtime, 최종 검증 관문을 모두 기록하며 자료 대기나 Editor 증거 대기를 완료로 승격하지 않는다. 같은 원본·규칙·seed·타일 범위는 같은 완결 영역 hash를 생성한다.

## D-115 경관 품질은 Synty 연결 뒤 독립 Rendering Profile로 일괄 적용한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-106의 독립 Synty Pipeline과 D-114의 경관 완결 영역 뒤에 Unity 표현 품질 관문을 추가함

공간 사실과 Simulation 규칙은 URP의 조명·안개·색보정 수치를 직접 소유하지 않는다. 영역 역할, 시간대, 계절과 날씨 같은 의미 코드를 `RenderingProfile` 고유 식별자로 해석한 뒤 Unity의 독립 `경관 품질 후처리` 단계가 태양, 환경광, 그림자, 하늘, 대기 원근, 색보정, Bloom, Vignette와 카메라 모드를 일괄 적용한다.

## D-116 플레이어 경관 탐색은 Simulation 권위와 분리한 표현 전용 입력이다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-096 플레이어 카메라 원칙과 D-115 경관 품질 Profile을 Farm 완결 영역의 직접 보행으로 확장한다.

Unity 플레이어는 의미 기반 플레이어 루트가 `CharacterController`, Synty `VisualRoot`와 1인칭 시선 회전축을 소유하고, 영역의 표현 계층은 독립 RTS 전술 카메라 회전축을 소유한다. F2 1인칭의 WASD·방향키·Shift·마우스 시선과 F3 RTS 전술 화면의 유닛 선택·지형 우클릭 목적지 이동·WASD 화면 이동·휠 줌은 공간을 관찰하는 `PresentationOnly` 입력이다. F1은 기존 전략 화면으로 복귀한다. 어느 조작도 서버 상태, `WorldTick`, 상태 개정 번호나 업무 완료를 변경하지 않는다. Synty 캐릭터 Prefab과 동작은 교체 가능한 표현이고 플레이어 고유 식별자나 업무 권위를 갖지 않는다.

RTS 전술 3인칭은 캐릭터 어깨를 따라가는 근접 추적 카메라가 아니라 높은 사선에서 영역·건물·유닛을 함께 읽는 독립 초점 카메라다. 전술 초점은 WASD·방향키로 이동하고 휠로 확대·축소하며 F로 선택 유닛에 재집중한다. 선택 원과 목적지 표식은 `VisualRoot` 바깥의 표현 계층에 둔다. 우클릭 목적지는 지형 충돌 결과와 완결 영역 이동 경계로 제한하고 현재 단계에서는 로컬 직선 이동만 수행한다. 실제 도로 경로 탐색, 화물 배차, 기사 업무 이동이나 서버 Command로 승격할 때는 별도 권위 계약과 확인 절차를 추가한다.

같은 공간·Synty 배치에 Overview, Region, Task와 1인칭 카메라를 함께 제공할 수 있지만 카메라 이동과 화면 효과는 항상 `PresentationOnly`다. 1인칭 이동은 WorldTick, 상태 개정, 주문·생산·운송 완료를 바꾸지 않는다. Synty 원본 Prefab·Material을 수정하지 않고 Volume Profile, wrapper와 카메라 설정을 교체해 지역별 미감을 조정한다.

첫 Profile은 `rendering-profile:sim:pyeongchang:rural-clear-day.v1`이며 대관령 Farm 1km 완결 영역에서 검증한다. 다른 Farm·Town·Hub는 같은 처리기를 재사용하고 Profile만 교체한다. 실제 날씨나 시간대와 연동할 때도 서버는 의미 코드와 근거만 제공하며 Unity가 구체적인 URP 수치를 해석한다.

## D-117 NPC 직업·권한은 운영 인증이 아니라 Simulation 조직·역량·위임 규칙으로 실행한다

- 상태: `Accepted`
- 결정일: 2026-08-13
- 관계: D-111의 업무 규칙 집결과 D-116의 표현 전용 캐릭터 조작 사이에 NPC 업무 실행 권위를 추가함

현재 NPC 행동은 운영 사용자의 로그인 세션, 실제 HR 조직, API 역할 Annotation이나 운영 시설 권한을 조회하지 않는다. Simulation Session이 시나리오 조직, NPC 행위자, 기술, 역량 부여, 관리자 위임과 업무 정책을 소유하고 같은 seed·입력·WorldTick에서 결정적으로 담당자와 행동 단계를 계산한다.

첫 세로 단위는 진부면 물류 거점의 입고검수다. 도착 화물의 검수 작업은 `배정 → 이동 → 작업 → 완료`를 거쳐야 보관 가능한 재고가 되며, 자동화 중지나 적격자 부재는 작업 삭제가 아니라 `Blocked` 상태로 남긴다. 시나리오 관리자는 같은 조직·시설 범위 안에서만 역량을 위임할 수 있고 자동 위임으로 받은 역량은 다시 위임할 수 없다.

사용자 개입은 NPC를 직접 조종해 업무 완료를 만드는 방식이 아니라 자동 배정, 자동 위임, 우선순위와 선호 담당자 같은 Simulation 정책을 변경하는 방식으로 둔다. Unity는 `Npc업무행동Projection`을 따라 캐릭터 위치와 Idle/Walk를 표현할 뿐 WorldTick, 작업 완료, 재고 상태나 권한을 변경하지 않는다.

## D-118 UI 행동은 실행 가능한 호출 계약과 확정 뒤 재조회를 함께 제공한다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-112 UI 기획과 D-113 객체–업무 규칙 연결을 첫 런타임 수직 단위로 구체화함

UI 기획 DB는 화면에 무엇을 보여줄지 정의하지만 현재 Session 상태나 버튼 활성 여부를 저장하지 않는다. Application의 관점별 조회 서비스가 같은 Session 원장에서 상태·WorldTick·업무 대상·규칙 근거를 읽어 `SimulationWorldUIProjection`을 생성한다. 따라서 UI 상태를 위한 별도 쓰기 원장이나 migration을 만들지 않는다.

실행 가능한 UI 행동은 기능 키와 표시 이름만 전달하지 않는다. HTTP 방식, route template, 요청·응답 계약 키, 대상 고유 식별자와 대상 개정, 기대 Session 개정, 행위자, 확정 뒤 canonical 재조회 route를 함께 전달한다. 클라이언트는 가격·수량·개정이나 성공 상태를 자체 계산하지 않고 Preview 요청, 명시적 Confirm, 같은 정보판 재조회 순서를 따른다.

첫 적용은 진부면 물류 거점 입고 검수다. 도착 가능한 화물과 적격 NPC가 있을 때만 Preview·Confirm을 활성화하고, Confirm 뒤 NPC 작업과 화물 인수가 실제 Simulation `WorldTick`에서 완료되어 보관 가능 재고가 생긴 뒤에만 정보판을 `Completed`로 표시한다. Unity 버튼과 animation은 이 상태를 표현하지만 독립적으로 완료를 만들지 않는다.

## D-119 입고 검수 완료와 적재 완료를 다른 상태·행동으로 관리한다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-117의 NPC 검수 실행과 D-118의 정보판 완료 판정을 적재까지 확장하며, D-118의 검수 후 즉시 `Completed` 판정을 대체함

공통 입고 흐름은 `입고예정 → 검수대기 → 적재대기 → 적재완료`를 사용한다. Simulation의 `StorageEligible`은 검수를 통과했으나 아직 적재하지 않은 `적재대기` 상태로 해석하고, 같은 재고 고유 식별자와 개정을 참조하는 별도 적재 Preview·Confirm·NPC 작업이 완료된 뒤에만 `적재완료` 상태로 전이한다.

운영 MAUI는 기존 운영 API에 수령 기록을 저장하고 정확한 입고 요청·입고상품 ID를 재조회하는 경계를 유지한다. Simulation은 같은 canonical 업무 코드를 사용하되 운영 로그인·HR·운영 재고를 변경하지 않는 독립 Session 원장으로 실행한다. `SimulationWorldUIProjection`은 업무 코드·현재 단계·실행 모드·canonical 행동 코드를 함께 전달하며, Unity는 이 상태를 표현할 뿐 검수나 적재 완료를 독자적으로 만들지 않는다.

## D-120 Figma·MAUI·Unity는 디자인 의미를 공유하고 렌더러 구현은 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-112의 Figma 근거, D-118의 실행 계약, D-119의 검수–적재 흐름을 Unity 실제 정보판으로 구체화함

Figma와 MAUI에서 확인한 역할 색, 상태 배지, 정보 우선순위, Preview·Confirm 문법은 `DesignProfileRevision`, Layout Profile과 역할·상태·정보·행동의 의미 키로 공유한다. 서버는 RGB, 글꼴, UGUI 계층이나 Prefab 경로를 내려보내지 않고 현재 상태와 표현 의도만 제공한다. MAUI와 Unity는 같은 의미를 각 실행 환경에 맞게 렌더링하며 디자인 개정이 맞지 않으면 Unity는 중립 표현과 경고를 사용한다.

Unity 기본 Scene은 Simulation 서버를 권위로 사용한다. 클라이언트는 Projection이 허용한 검수·적재 Preview, 명시적 Confirm과 WorldTick만 호출하고 확정 뒤 같은 정보판을 다시 조회한다. 마지막 성공 상태와 stale 표시는 유지하지만 네트워크 실패를 fixture 성공으로 바꾸지 않는다. 결정적 fixture는 EditMode·PlayMode와 화면 증거 생성에만 명시적으로 사용하며 운영 API·운영 DB나 실제 HR 권한을 호출하지 않는다.

## D-121 Unity 최종 실행은 SimulationWorldShell 하나에 통합한다

- 상태: 확정
- 날짜: 2026-08-14
- 관계: D-115의 농장 플레이어 시점과 D-120의 진부 입고 정보판을 하나의 실행 맥락으로 통합함

Unity의 월드 개요, 대관령 Farm 1인칭, RTS형 농장 전술 시점과 진부 Hub 입고 정보판은 `SimulationWorldShell` 하나에서 전환한다. Build Settings에서 활성화하는 최종 Play Scene도 이 Scene 하나로 제한한다. `WorldBootstrapScene`과 기능별 연구 Scene은 삭제하거나 자동 합성하지 않고 관찰·회귀 검증 자료로 보존하며 독립 게임 진입점으로 사용하지 않는다.

화면 전환 막대는 카메라 교체와 무관하게 유지되는 Overlay Canvas로 구성한다. 모드 전환은 WorldRoot의 가시성, 카메라 초점과 정보판 문맥만 바꾸는 `PresentationOnly` 동작이며 서버 상태, `WorldTick`, 상태 개정 번호나 업무 완료를 변경하지 않는다. 새로운 기능은 연구 Scene에서 근거를 확인한 뒤 같은 `SimulationWorldShell`에 좁게 통합하고 별도의 최종 Play Scene을 추가하지 않는다.

## D-122 1인칭 월드는 고정 L2 타일 창과 자료 상태를 따라 동적으로 준비한다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-114의 기술 타일과 D-116의 1인칭 탐색을 D-121 단일 Play Scene의 런타임 생명주기로 연결함

대관령 Farm의 첫 동적 공간은 EPSG:5186 L2 500m 타일을 원본·캐시 식별 단위로 사용한다. 플레이어가 있는 중심 타일 주변 `3×3`은 활성 창, `5×5`는 미리 준비하는 창으로 유지하며 타일을 벗어나면 더 이상 필요하지 않은 표현 루트는 파괴하지 않고 풀로 돌려 재사용한다. 첫 검증 범위는 중심 `kr5186:l2:700:1145`의 5×5이고 같은 Recipe·원본·규칙 개정은 같은 타일 hash를 제공한다.

시뮬레이션 서버는 Recipe, Tile Manifest, Layer 산출물 상태와 활동 관점별 조회 결과를 읽기 API로 제공한다. Unity는 이 계약을 읽어 표현 생명주기만 바꾸며 `WorldTick`, Session 개정, 주문·생산·운송 완료를 확정하지 않는다. 실제 DEM·토지피복·배치 마스크 산출물이 없으면 URL·hash·높이 Mesh를 꾸며내지 않고 `WaitingForSpatialArtifact`를 표시한다. 개발 Fixture도 같은 결손 상태와 `PresentationOnly` 경계만 표현하며 실제 공간 증거가 아니다.

첫 단계는 Addressables나 전국 스트리밍을 도입하지 않는다. 실제 산출물 저장·다운로드·검증이 연결되기 전까지 동적 경계는 Collider 없는 상태 표시이고 기존 `ScenarioTerrainPreview`를 물리 DEM으로 승격하지 않는다. 장거리 확장 시 원점 이동과 다중 사용자 위치 전송은 이 타일 식별자를 재사용하되 별도 권위·성능 검증 뒤 활성화한다.

## D-123 타일 안전 창과 카메라 시야 기반 표현 우선순위를 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-122의 고정 타일 생명주기에 1인칭 시야·건물 표현 승격과 이동 안전 경계를 추가함

플레이어 주변 `3×3` 활성·`5×5` 준비 창은 이동 안전과 자료 가용성을 위한 방사형 범위로 유지한다. 카메라 절두체, 화면 가장자리 여백과 짧은 이동 예측은 이 창 안에서 무엇을 먼저 표현할지만 결정한다. 시야 밖 타일을 즉시 버리거나 카메라가 보지 않는다는 이유로 안전 지면을 해제하지 않는다.

타일별 건물 조회 결과는 `Declared → ProxyQueued/ProxyActive → DetailQueued/DetailActive → HiddenCached` 상태를 갖는다. 먼 거리·예측 시야에는 충돌 없는 단순 프록시를 먼저 배치하고, 실제 시야와 상세 거리 안에 들어오면 의미 기반 `VisualKey`를 Synty Prefab으로 해석한다. 화면 밖으로 나간 표현은 유예 시간 뒤 비활성 캐시에 두며 다시 보일 때 재사용한다. 이 상태 전이는 모두 `PresentationOnly`이고 공공데이터 관측, 업무 완료, `WorldTick`이나 Session 개정을 만들지 않는다.

실제 DEM·배치 마스크가 준비되지 않은 서버 모드에서는 다음 위치의 타일이 추적 중이어도 안전 기반 Layer와 지면 충돌이 모두 확인될 때만 이동한다. 명시적 개발 Fixture는 기존 `ScenarioTerrainPreview` Collider를 이동 검증용으로 사용할 수 있지만 이를 물리 DEM 완료로 승격하지 않는다. 타일·시야·프록시·상세·캐시·안전 판정은 런타임 진단 트리로 함께 표시한다.

## D-124 월드 API는 행정동·법정동 파생 Projection을 먼저 읽는다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-101·D-102의 행정구역·건물 원장과 D-114·D-122의 타일 Pipeline 사이에 서버 가공 경계를 추가함

Simulation 서버는 월드 요청마다 공유 공공데이터 원문을 직접 조인하지 않는다. 공간 파생 실행이 건축물–법정동·행정동 Assignment, 법정동–행정동 관할 관계와 행정동별 건물 Category 집계를 먼저 불변 node·relation으로 저장하고, 월드 API는 최신 완료 실행의 지역 Projection을 읽는다.

법정동과 행정동은 서로 다른 고유 식별자와 근거를 유지한다. 경계 geometry와 지역–타일 교차 관계가 없으면 타일을 추정하지 않고 `WaitingForRegionGeometry`를 반환한다. 이 Projection은 Unity 공간 표현을 위한 파생 읽기 결과이며 운영 시설 자격, 주문·재고·운송 상태나 Simulation 업무 완료를 확정하지 않는다.

## D-125 L2 Runtime은 상세 3×3·활성 5×5·준비 9×9의 예산형 창을 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-14
- 관계: D-122·D-123의 고정 타일 창을 공식 엔진 조사와 경계 선행 준비로 대체함

500m L2의 첫 PC Profile은 3×3 상세 후보, 5×5 활성 지형·상호작용, 9×9 Manifest·산출물 준비 창을 사용한다. 대관령 Fixture의 11×11 제공 범위는 한 칸 앞당긴 9×9 준비 창을 검증하기 위한 범위이며 동시 다운로드나 활성 범위가 아니다. 한 시점의 동시 타일 로드는 4개로 제한한다.

플레이어가 타일 경계까지 타일 폭의 25% 안으로 접근하고 이동 방향이 그 경계를 향하면 준비 중심을 한 타일 앞당긴다. 경계를 넘을 때 기존 Slot과 검증된 Manifest를 재사용하고 새 가장자리만 요청한다. 최고 상세 Prefab은 상세 창 전체가 아니라 카메라 절두체·거리·프레임 예산을 통과한 객체만 승격한다.

이 숫자는 공식 엔진의 고정 권장값을 복사한 것이 아니라 스트리밍 원점, Loading Range, HLOD, 비동기 참조 관리와 동시 타일 로드 제한 원칙을 현재 L2 크기에 적용한 초기값이다. 실제 지형 산출물이 연결되면 프레임 시간·메모리·요청 p95·경계 대기·캐시 적중률을 근거로 플랫폼·이동수단별 Recipe를 새 개정으로 분리한다.

## D-126 생존 타로는 안전 거점 전원 합의 뒤 다음 Tick에만 적용한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-096의 타로 카드 조건·대응 경계와 D-097의 상위 규칙 보정선을 1인칭 생존 Session에 적용함

공공데이터는 지형·건물·도로·공개 사업체의 공간 사실을 제공하며 생존 위기, 아이템 재고와 타로 효과를 만들지 않는다. 생존 타로는 `SimulationScenario` 초기 규칙과 같은 Session 원장 안에서만 실행하며 운영 재고나 공공데이터 원본을 변경하지 않는다.

기회는 한 Tick을 하루로 보는 기본 3일 주기와 첫 식량 비축 `2 인일 이하` 위기에서 발생하고, 미해결 기회는 한 건만 유지한다. Scenario가 농장 건물 범위를 지정하면 월드 전체 식량과 별도로 농장 건물 컨테이너 및 농장 안 참여자의 소지 식량을 합산한다. 이 농장 자급분이 기본 `2 인일 이하`면 농장 밖 보급·탐색이 필요하다는 `ExternalExpeditionRequired` 기회를 일반 식량 위기보다 먼저 한 번 생성한다. 농장 밖에 충분한 식량이 있어도 이 판정은 유지하며, 같은 부족을 일반 식량 위기로 중복 생성하지 않는다. 농장 범위를 지정하지 않은 기존 Scenario는 종전 전체 식량 위기 규칙을 유지한다. 같은 seed·Tick·deck 개정과 선택 이력은 같은 3장 제안을 만든다. 응답은 Scenario가 지정한 안전 건물에 참여자 전원이 함께 있을 때만 허용하며, 전원이 동일 제안을 선택해야 확정할 수 있다.

클라이언트는 `CommandId`, `ExpectedRevision`, 기회 고유 식별자, 참여자 고유 식별자와 제안 고유 식별자만 보낸다. 카드, 방향, 수치와 기회·부담 보정선은 서버가 결정한다. 확정한 보정선은 확정 Tick의 재고·이동·생산을 즉시 바꾸지 않고 다음 Tick 한 번만 활성화하며, 재고 생성·용량 초과·접근 정책 우회나 운영 효과를 만들 수 없다. 응답·합의·아이템 획득은 모두 Save/Replay 명령 로그와 해시에 포함한다. Unity는 이 상태를 정보판·효과로 표현할 수 있지만 화면이나 animation으로 합의·생존 결과를 독자적으로 확정하지 않는다.

## D-127 세계 사건은 서버 원장에 먼저 확정하고 Unity는 개정 기반 표현 자료만 읽는다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-126의 생존 타로 기회를 첫 세계 사건 adapter로 일반화함

Simulation Session이 사건 발생 조건, 선택지, 참여자 응답과 확정 결과를 먼저 계산한다. 사건은 안정적 고유 식별자, 사건 개정, 마지막으로 변경된 세계 개정, 발생·노출 Tick, 건물·타일·지역 닻과 규칙 개정을 가진다. 클라이언트는 마지막으로 반영한 세계 개정 이후의 변경만 요청하고 서버가 반환한 다음 개정을 커서로 저장한다.

Unity에는 Prefab·Material·Synty 원본 경로를 내려주지 않고 `PresentationKey`와 사건 의미·상태·선택지·공간 닻을 내려준다. Unity는 다운로드한 결과를 정보판·사운드·파티클·환경 효과로 표현하되, 사건 발생·합의·보정선을 다시 계산하지 않는다. `SimulationOnly=true`, `IsOperationalState=false`, `PresentationOnly=true`의 경계를 어긴 응답은 수신 계약에서 거부한다.

생존 타로는 첫 adapter이며 사건 원장 자체를 독립 화면 상태로 중복 저장하지 않는다. 현재 Save/Replay는 세션 생성과 응답·합의 Command를 다시 실행해 같은 사건 고유 식별자·개정·결과를 재생한다. 다른 사건 원천을 추가할 때도 재생 가능한 규칙 입력과 Command 계보를 먼저 정의한다.

## D-128 농장 생존은 플레이어·NPC 노동과 회복 가능한 위협을 같은 Session 원장에 둔다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-122·D-125의 L2 공간 창, D-126의 생존 타로, D-127의 세계 사건 Projection을 첫 주 농장 생존 흐름으로 결합함

공공데이터의 지형·법정동·건물·사업체는 농장 생존 Scenario의 공간 닻과 배치 제약으로만 사용한다. 실제 사람·사업체·건물을 감염자, 약탈자, 전리품이나 공격 대상으로 해석하지 않는다. 좀비는 환경 압력, 약탈자는 가상 세력이며 모두 `SimulationOnly=true`, `IsOperationalState=false`인 오버레이다.

플레이어 직접 노동과 NPC 위임 노동은 같은 농장 상태를 변경하지만 비용 계약은 분리한다. 플레이어 노동은 개인 체력을, NPC 위임은 Settlement 공동 노동력을 예약하며 완료 뒤 반환한다. 클라이언트는 배우·대상·행동·배치 방식과 예상 개정만 보내고 소요 시간·노동력·체력·재료 비용은 서버가 정한다.

위협 결과는 영구 사망 대신 부상, 보급품 손실, 시설 피해와 수리 필요량으로 남긴다. 서버가 seed와 방어 준비도에 따라 결과를 계산하고 Save/Replay Command 계보와 세계 사건 개정으로 재현한다. Unity는 서버의 위협 종류·상태·개체 수·`PresentationKey`를 `VisualKey`로 해석할 뿐 개체 수나 성공 결과를 독자적으로 계산하지 않는다. 유료 Synty 팩은 원본 경로나 구매 여부를 업무 계약에 넣지 않고 현재 fallback과 선호 SourcePack을 가진 구성 대장에서 마지막에 연결한다.

## D-129 같은 Simulation 팀은 별도 요청 없이 서로 관찰하되 조작 권한을 공유하지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-116의 표현 전용 플레이어 카메라와 D-122·D-125의 L2 타일 창을 협동 Session 관찰에 확장함

같은 Simulation 팀에 가입한 행위자는 팀 정책이 허용한 1인칭 시선 또는 따라보기 카메라를 별도 건별 동의 요청 없이 사용할 수 있다. 팀 가입 시 상호 관찰 가능성을 안내하고 관찰 중에는 대상 화면에 관찰자 표시를 유지한다. 다른 팀 행위자에게는 이 자동 허용을 적용하지 않는다.

관찰 권한은 서버의 팀 원장 또는 그 읽기 Projection이 같은 Session·팀·구성원·정책 개정을 확인한 결과로만 부여한다. Unity가 구성원 목록이나 팀 정책을 제출해 권한을 만들 수 없다. 관찰 결과는 대상 이동·시선·상호작용 Command, 재고, `WorldTick`, Session 개정이나 운영 상태를 변경하지 않으며 `CanControlTarget=false`, `MoveObserverActor=false`, `ChangesWorldState=false`를 유지한다.

관찰은 권한 미리보기와 별도의 짧은 수명 관찰 Session으로 관리한다. 시작 뒤에도 각 공개 위치 상태 사본을 조회할 때 현재 팀 구성원·정책 개정을 다시 검사하며, 팀 이탈·정책 개정·명시적 종료·로컬 위험이 발생하면 관찰 표현을 종료한다. 공개 상태 사본에는 대상의 L2 타일, 타일 내부 평면 오프셋, 표고 참고값, 카메라 높이, 시선 회전과 이동 의도만 포함하고 재고·개인 UI·대화 내용은 포함하지 않는다. 대상에게는 활성 관찰자 수를 표시하되 이 목록으로 조작 권한을 만들지 않는다. 위치 입력은 HTTP 공개 쓰기 API가 아니라 이후 Netcode 또는 전용 위치 수집기가 구현할 내부 저장 경계로 둔다.

Unity는 로컬 플레이어 입력과 관찰 카메라를 분리한다. 관찰 중 로컬 입력 Controller를 일시 정지하고 상대의 공개 가능한 카메라 기준점 또는 따라보기 기준점만 읽는다. 관찰자가 위험해지면 로컬 시점으로 즉시 복귀한다. 먼 팀원을 관찰할 때는 서버가 허용한 대상 타일을 별도 표현 초점으로 사용할 수 있지만 로컬 행위자를 순간이동시키거나 두 지역을 모두 최고 상세로 유지하지 않는다. L2 타일 내부 평면 거리는 World의 500m→Unity 타일 압축률로 변환하고 사람의 카메라 높이는 압축하지 않는다. 실제 다중 사용자 위치 전송, 팀 영속 원장과 Netcode 연결은 별도 어댑터로 검증한다.

## D-130 Simulation 역할은 고정 직업이 아니라 팀 공동 카드와 현재 활동에서 파생한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-126의 사건 해결용 생존 타로와 D-129의 협동 팀 원장을 분리해 역할 전환 능력으로 확장함

생존 타로 카드는 사건 발생·응답·합의 결과를 나타내므로 개인 장착 카드로 전용하지 않는다. 별도의 팀 역할 카드함이 탐험·농사·물류 같은 활동 능력 카드 사본을 보유하며, 구성원은 물리적으로 떨어져 있어도 같은 팀 정책 개정 안에서 카드를 다른 구성원의 장착 칸으로 옮길 수 있다. 역할 카드는 실제 물품 재고가 아니므로 공간 근접이나 아이템 순간이동으로 해석하지 않는다.

캐릭터의 역할은 영구 직업이 아니다. 서버가 `현재 장착 카드 + 활성 활동 배정`을 조합해 현재 역할을 투영하며, 활동이 끝나면 고정 직업으로 남지 않는다. 한 카드 사본은 한 시점에 한 장착 칸에만 존재한다. 누군가 탐험·농사 등 해당 카드를 사용하는 활동을 시작하면 카드와 그 배우의 활동을 잠그고, 종료 전에는 다른 구성원에게 옮기거나 같은 배우가 다른 활동을 동시에 시작할 수 없다.

카드 장착과 활동 시작·종료는 요청 식별자, 카드함 예상 개정과 팀 정책 예상 개정을 서버가 검사한다. Unity는 역할을 독자적으로 계산하거나 카드 사본을 복제하지 않고 서버 상태 사본을 표현한다. 카드 상태는 별도 process-local 카드함이 아니라 Simulation Session aggregate의 선택적 상태이며, 장착·활동 Command와 상태는 Save/Replay 로그·해시에 포함한다.

## D-131 역할 카드 규칙 정의와 현재 장착 상태의 DB 책임을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-130의 팀 공동 역할 카드를 Simulation World 파생 DB와 Session 저장 경계에 배치함

카드 장착·활동 시작·활동 종료의 안정적 규칙 식별자, Engine과 입출력 계약은 `Simulation World 업무 규칙 대장`의 새 불변 개정에 저장할 수 있다. 이 대장은 어떤 Scenario가 어떤 규칙 개정을 사용했는지 재현하는 정적 관계·계보이며 플레이 중 상태의 권위가 아니다.

카드 사본의 현재 장착 대상, 잠금, 활성 활동과 파생 역할은 Simulation Session aggregate가 소유한다. 이 상태와 세 Command는 Session 저장 자료와 재생 해시에 포함하고, 공간 파생 DB에는 복제하지 않는다. 따라서 공간·Synty 파이프라인을 다시 생성해도 팀 장착 상태가 바뀌지 않으며, 카드 규칙 대장을 교체해도 이미 저장된 Session은 생성 요청과 Command 계보로 같은 상태를 재현한다.

## D-132 Simulation Session 저장 자료는 별도 DB에 보존하고 Command 재생으로 복원한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-131의 가변 역할 카드 상태와 기존 `simulation-save.v1` 저장·재현 경계를 물리 저장소에 연결함

공유 공공데이터 DB와 Simulation World 파생 관계 DB에는 Simulation Session 저장 자료를 넣지 않는다. 별도 `Simulation Session DB`가 저장 식별자, Session 식별자, schema, WorldTick, World 개정, Command 수, 재생 SHA-256과 전체 저장 자료 JSON을 보존한다. 운영 업무 DB 상태나 실제 재고로 승격하지 않는다.

현재 활성 aggregate는 계속 프로세스 메모리에 두고 모든 Command마다 DB snapshot으로 덮어쓰지 않는다. 명시적 Save가 불변 저장 자료를 만들고, 프로세스 재시작 뒤 Restore가 그 자료의 Metadata와 JSON을 대조한 다음 기존 Command를 재생해 새 활성 aggregate를 만든다. 같은 저장 식별자와 같은 hash는 멱등 재사용하며 다른 hash는 충돌로 거부한다. JSON·Metadata·재생 hash 또는 Command 결과가 일치하지 않으면 손상 자료로 거부한다.

영속 저장은 `SimulationSessionDatabase:Enabled=true`일 때만 활성화하고 기본 개발·시험 호환을 위해 비활성 상태에서는 기존 메모리 저장소를 사용한다. DB migration은 API host 시작 시 자동 적용하지 않고 별도 명시적 명령으로만 적용한다.

## D-133 농사·영역 발견 보상은 개인 수집 카드 원장으로 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-125의 L2 Runtime, D-128의 농장 노동, D-130·D-131의 팀 역할 카드와 D-132의 Session 저장 경계를 결합함

농사·탐험 보상 카드는 팀 활동 능력을 부여하는 역할 카드나 사건 합의를 보정하는 생존 타로가 아니다. 첫 개정은 수치 효과와 기능 해금이 없는 개인 수집·표현 카드로 유지하고, 획득한 카드 사본은 같은 Simulation 팀 구성원에게 거리와 무관하게 양도할 수 있다. 양도는 소유권만 바꾸며 물리 아이템 이동이나 복제를 만들지 않는다.

농사 보상은 플레이어가 직접 수행한 밭갈기가 `WorldTick`에서 실제 완료된 첫 사건에만 판정한다. NPC 위임, 반복 완료, 방어·의료 작업은 제외한다. 탐험은 서버가 기억한 현재 L2에서 8방향 인접 L2로 이동했음을 확인한 뒤 팀 최초 L2와 그 상위 L1 발견을 각각 한 번 기록한다. 시작 L2와 상위 L1은 이미 밝혀진 것으로 보며 시작 보상을 주지 않는다. 현재 HTTP 기반 첫 개정은 권위 있는 실시간 위치 검증을 대신하지 않고 서버가 기억한 이전 타일·인접성·Fixture 제공 범위만 검사한다.

기본 확률은 직접 농사 완료 20%, 새 L2 15%, 새 L1 40%이며 같은 계열의 활성 역할은 10%p를 더한다. 행위자·농사/탐험 계열별로 5회 연속 실패하면 다음 적격 판정을 보장하고, 성공 시 해당 실패 수를 초기화한다. 판정은 Scenario seed, 보상 규칙 개정, 사건 고유 식별자, 행위자, 유발 종류와 시도 순번을 SHA-256에 넣어 결정하며 클라이언트 요청 식별자·원하는 카드·seed·확률을 입력받지 않는다. 자료집 용량 부족으로 건너뛴 사건은 실패 수에 포함하지 않는다.

보상 성공은 정확한 카드를 즉시 정하지 않고 소유자 전용·무기한 미개봉 기회를 만든다. 별도 뽑기 Command가 팀이 아직 보유하지 않은 같은 계열 정의 중 하나를 결정하며 미개봉 기회도 자료집 용량을 예약한다. 발견 원장, 실패 보정, 판정 기록, 미개봉 기회, 카드 사본과 양도 이력은 Session 상태와 Command 재생 해시에 포함하고 Simulation World 공간 파생 DB에는 복제하지 않는다. Unity에는 `PresentationKey`만 전달할 수 있으며 Prefab 경로나 게임 화면이 보상·소유권을 확정하지 않는다.

## D-134 활동별 기본 시점은 편의 정책이며 사용자의 허용된 수동 전환을 막지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-116의 플레이어 카메라, D-122·D-125의 L2 탐험과 D-128의 농장 생존 작업을 입력 방식에 맞게 결합함

농장 경영은 영역·농지·시설·인부와 여러 작업을 한눈에 비교해야 하므로 RTS형 전술 3인칭을 기본 시점으로 사용한다. 탐험은 직접 이동·근접 상호작용·시야 기반 공간 준비가 중심이므로 1인칭을 기본 시점으로 사용한다. 이는 강제 직업이나 서버 권한이 아니라 Unity의 표현·입력 편의 정책이며, 농장에서도 1인칭으로 걷고 탐험에서도 허용된 전술 3인칭으로 전환할 수 있다.

전술 3인칭의 이점은 단순히 카메라를 멀리 두는 것으로 끝내지 않는다. 농지·시설·인부 다중 선택, 영역 상태 겹쳐보기, 작업 초안과 일괄 계획처럼 넓은 시야에 맞는 도구를 제공한다. 첫 수직 단위는 농지 10개와 수확 마당 선택, 밭갈기·파종·관수·수확 종류 선택과 우클릭 위치 기반 작업 초안까지다. 초안은 아직 서버 `Preview`가 아니며 `RequiresExplicitConfirm=true`, `ChangesWorldState=false`, `PresentationOnly=true`를 유지한다.

1인칭은 같은 플레이어와 Session·재고·역할 카드·농장 상태를 공유하고 카메라와 입력만 바꾼다. 시점 전환, 선택 강조, 작업 초안과 카메라 이동은 `WorldTick`, Session 개정, 생산량, 노동력, 카드 보상이나 업무 완료를 변경하지 않는다. 실제 농장 작업은 기존 Simulation Command의 Preview와 명시적 Confirm을 통과한 뒤 서버 상태 사본을 다시 조회해야 확정된다.

## D-135 1인칭과 3인칭은 전환 전용 카메라로 연속 보간한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 관계: D-134의 활동별 기본 시점과 수동 전환을 시각적으로 연결함

1인칭과 전술 3인칭 사이의 전환은 카메라 활성 상태를 한 프레임에 교체하지 않는다. 현재 활성 카메라의 세계 위치·회전·화각을 전환 전용 카메라가 이어받고 목표 카메라까지 짧은 이차 곡선으로 위치를 이동하며, 회전과 화각은 완만한 ease-in-out으로 보간한다. 캐릭터 Renderer는 카메라가 충분히 멀어지거나 가까워진 중간 지점에서 전환해 머리 내부나 몸체가 갑자기 화면을 가리는 문제를 줄인다.

전환 중에는 1인칭 직접 이동, 전술 선택·이동과 농장 경영 입력을 모두 잠그고 완료 뒤 목표 시점의 입력만 활성화한다. 전환 도중 반대 시점을 다시 선택하면 화면을 처음 위치로 순간이동시키지 않고 현재 전환 카메라 위치를 새 시작점으로 사용한다. 이 과정은 Unity 표현 전용이며 `WorldTick`, Session 개정, 업무 상태, 이동 결과나 작업 확정을 변경하지 않는다.

## D-136 전투는 서버 권위 단일 박자로 판정하고 시점별 이점은 일반 허용 구간에만 둔다

- 상태: 채택
- 날짜: 2026-08-15
- 결정: 농장 직접 전투는 한 번에 하나의 `CombatBeat`만 활성화한다. Unity는 서버가 확정한 공격 유형·충돌 시각·허용 구간을 표시하고 `BeatStableId`, 배우, 방어/카운터 행동과 반응 경과 ms만 제출한다. 판정 등급, 피해, 방어 점수, 경직과 사건 결과는 Session aggregate가 확정한다. 1인칭은 일반 방어·카운터 허용 구간을 넓히고 집중 전조를 제공하며, 전술 3인칭은 표준 구간을 유지하는 대신 위협·동료·시설 상황 인식을 제공한다. 완벽 판정 구간은 양 시점에서 동일하다. 활성 박자 동안 시점 변경과 다른 플레이어 입력은 잠그고, 미반응은 다음 WorldTick에서 결정적으로 만료한다.
- 이유: 1인칭 몰입의 조작 이점을 주면서도 완벽 판정의 경쟁 기준은 동일하게 유지하고, 네트워크 지연에 민감한 피해 결과를 Unity 로컬 시간이나 프레임에 맡기지 않기 위해서다.
- 호환: 기존 `farm-survival.spring-preparation.r1`의 자동 좀비 판정은 유지하고 직접 전투는 `r2`에서만 활성화한다. 전투 Command와 상태 사본은 Save/Replay와 해시에 포함하며 실제 사람·사업체·건물에 전투 의미를 부여하지 않는다.
- 관계: D-127의 서버 권위 세계 사건, D-128의 농장 생존 Session, D-134의 활동별 시점 정책과 D-135의 연속 카메라 전환을 전투 입력까지 확장함

## D-137 1인칭 영웅 성과는 한 명령창 동안만 주변 전선의 전술 기회가 된다

- 상태: 채택
- 날짜: 2026-08-15
- 결정: `farm-survival.spring-preparation.r3`에서 Guard 성공은 `Rally`, Counter 성공은 `Breakthrough` 전술 기회를 만든다. OnTime은 품질 1, Perfect는 품질 2이며 기회를 만든 영웅만 해당 농장 방어 전선의 바로 다음 명령창에서 사용할 수 있다. 전진 공격·대형 사수·전술 후퇴 Confirm은 다음 WorldTick에서 전선 위치, 분대 전투력, 회복 가능한 NPC 부상과 시설·물자 피해로 판정한다. 미확정 명령창은 기회를 만료시키고 보너스 없는 대형 사수를 자동 적용한다.
- 이유: 1인칭 숙련이 3인칭에서 무조건적인 전장 보너스가 아니라 선택 가능한 제한 자원이 되게 하고, 카메라·animation이나 클라이언트 계산이 승패를 확정하지 못하게 하기 위해서다.
- 호환: `r1` 자동 판정과 `r2` 반응 즉시 판정은 유지한다. 영구 NPC 사망, 여러 전선 RTS, 카드 공유와 영구 능력 성장은 첫 개정에 포함하지 않는다. Unity는 전술 시점 전환을 강제하지 않고 제안하며 실제 결과는 서버 상태 사본을 다시 받아 표현한다.
- 관계: D-136의 서버 권위 CombatBeat 결과를 D-134·D-135의 전술 3인칭·연속 카메라 전환과 연결함

## D-138 분대 이동은 서버 판정의 교체 가능한 표현이며 기준점과 대형 슬롯을 분리한다

- 상태: 채택
- 날짜: 2026-08-15
- 결정: Unity는 최신 확정 전술 판정의 명령 종류·전선 위치·분대 인원·전투력을 표현 frame으로 바꾸되 피해·승패·이동 결과를 계산하지 않는다. 한 분대는 경로 탐색을 담당하는 기준점 `NavMeshAgent` 하나와 최대 6개의 결정적 로컬 대형 슬롯으로 구성한다. 전진 공격은 쐐기, 대형 사수는 선형, 전술 후퇴는 종대를 사용하며 분대원은 기준점을 따라 슬롯으로 보간한다. Synty 캐릭터와 동작은 `VisualKey`·wrapper·공용 animation adapter를 거쳐 연결하고 root motion은 사용하지 않는다.
- 이유: 여러 개의 독립 agent가 서로 밀어내며 대형을 깨는 문제를 줄이고, 서버 권위 결과와 경로·대형·캐릭터·동작 자산을 서로 교체 가능하게 유지하기 위해서다.
- 호환: 첫 수직 단위는 아군 6명·적군 6명 표시 예산과 평면 농장 전술 바닥만 지원한다. 서버 `MemberCount`가 작으면 표시 인원도 줄이며 6명을 넘는 인원은 집계 상태로 남긴다. 영구 사망, ragdoll, 여러 전선 동시 경로, 외부 animation pack, 네트워크 위치 동기화는 포함하지 않는다. Unity의 이동·경직·도착은 `WorldTick`, 상태 개정이나 업무 완료의 근거가 아니다.
- 관계: D-137의 서버 전술 판정 사본을 D-118의 Synty 역할 표현, D-134의 전술 3인칭과 `SimulationWorldShell` 최종 Scene에 연결함

## D-139 Simulation·Unity 구조 리팩토링은 외부 계약을 보존한 채 검증 경계부터 진행한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 결정: 기능 확장이 누적된 Simulation·Unity는 짧은 기능 동결 기간에 `검증 라우팅 → API·Application 경계 → Save/Replay → Unity 읽기 런타임 → 문서 상태판` 순으로 정리한다. 기존 API 경로·HTTP 방식·JSON 필드·안정 식별자·저장 해시·DB 구조와 Unity 공개 모델을 바꾸지 않으며, 기존 Facade와 상태 코드는 호환 표면으로 유지한다.
- 이유: 잘못된 solution과 test project를 실행하는 검증 상태에서 거대 Controller·Service·저장 재생 파일을 먼저 분해하면 거짓 성공이나 호환 회귀를 발견하기 어렵기 때문이다.
- 범위: `SimulationWorldShell`, Prefab, Synty 원본, `.meta` GUID와 Game View는 구조 리팩토링에서 변경하지 않는다. 35개 부분 클래스로 구성된 Session Aggregate의 심층 분해는 Save/Replay 결합과 호환 특성 시험을 먼저 정리한 뒤 별도 결정으로 다룬다.
- 관계: D-003·D-004의 서버 권위 경계, D-016의 Unity 읽기 흐름, D-027의 서버 물리 분리와 D-132의 Save/Replay 결정을 구조 안정화 절차에 적용함

## D-140 코드 탐색 특성이 원본이고 생성 코드 지도는 검증되는 파생 자료다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 결정: 운영·Simulation·Unity가 함께 쓰는 코드 탐색 계약은 무의존 `Ssalddel.CodeMetadata`에 둔다. 소스의 `SsalddelCodeMetadataAttribute`와 작업영역 manifest를 원본으로 삼고, Codex와 사람이 읽는 JSON·한국어 트리는 결정적으로 생성한다. 기능 안의 `StepKey`, 단계 의존성, 실행 단계, 읽기·쓰기 데이터 권위와 부수효과를 기록한다.
- 검증: 필수 단계 누락, 중복·순환·역전 흐름, Simulation·Unity의 운영 상태 쓰기, 공유 공공데이터 쓰기, Unity의 표현 외 상태 쓰기와 오래된 생성 파일은 실패시킨다. 전체 공개 타입 미표기는 기능 확장을 막지 않고 프로젝트별 경고로 남긴다.
- 호환: 기존 namespace·특성 생성자·기능 키·descriptor 순서를 유지하고 `Ssalddel.Contracts`에는 타입 전달을 둔다. 메타데이터는 권한·트랜잭션·업무 규칙·DB 원장 또는 Unity 표현의 실행 권위가 아니다.
- 관계: D-003·D-004의 권위 분리, D-016의 Unity 데이터 흐름과 D-139의 구조 리팩토링 검증 경계를 코드 탐색에도 적용함

`Ssalddel.CodeMetadata`는 .NET 프로젝트 참조뿐 아니라 엔진 비의존 로컬 UPM 패키지로도 제공한다. Unity 패키지의 `Ssalddel.Unity.Data`는 이 어셈블리를 참조하고, .NET build 산출물은 패키지 바깥 `artifacts/local/dotnet/Ssalddel.CodeMetadata`에 둬 같은 이름의 사전 컴파일 DLL이 중복 수입되지 않게 한다.

## D-141 1인칭 전투 마우스 입력은 전투 진입과 서버 판정 반응으로 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 결정: 활성 전투 박자가 없고 서버 상태에 `AwaitingCombat` 교전이 있을 때 좌클릭은 피해를 만들지 않고 전투 진입을 요청한다. 기존 곡선 카메라 전환이 끝난 뒤 시점 확정과 박자 시작 Command를 보낸다. 활성 박자에서는 좌클릭을 `Counter`, 우클릭을 `Guard`로 해석하며 UI 위 입력은 소비하고 전투 Command로 만들지 않는다.
- 권위: Unity는 배우·교전·박자·행동과 관측 경과 시간만 보낸다. 등급·피해·방어 점수·경직·전술 기회는 기존 Simulation Session 규칙이 확정하며 응답 상태 사본을 다시 받아 표현한다. 일시 오류는 같은 Command ID로 한 번만 재시도하고 개정 충돌에서는 기대 개정을 클라이언트가 바꿔 재전송하지 않는다.
- 표현: 전투 진입과 활성 박자 동안 플레이어 이동을 잠그고 의미 기반 Input System `Attack`·`Defend`를 사용한다. Synty animation과 HUD는 서버 결과의 표현이며 업무나 전투 결과의 권위를 갖지 않는다.
- 관계: D-135의 연속 카메라 전환, D-136의 단일 박자 서버 판정과 D-137의 영웅 전술 기회를 실제 Unity 입력·HTTP 경계에 연결함

## D-142 기본 생존 장은 경관 산책 중심이며 직접 전투는 계절 방어의 선택 경로다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 결정: 새 농장 생존 Session의 기본 규칙은 `farm-survival.scenic-season.r1`이다. 1~23일은 공공데이터 기반 경관 산책·지역 발견·선택 농사를 유지하고, 24일 예고와 25~26일 준비 뒤 27일에 자동 방어 또는 직접 전투를 선택한다. 미응답은 28일 자동 방어로 확정하며 약탈자 사건은 기본 장에서 생성하지 않는다.
- 전투 경계: 자동 방어는 기존 시설 준비도만 사용한다. 직접 전투를 선택한 경우에만 `AwaitingCombat`과 1인칭 방어·카운터 입력을 열며, 성공은 방어를 보완하지만 참가 자체가 필수 성공 조건은 아니다. 결과는 영구 사망이나 농장 소멸 없이 회복 가능한 시설 피해·물자 손실·부상으로 제한한다.
- 호환: 기존 `farm-survival.spring-preparation.r1~r3`의 일정·판정·Save/Replay 해시는 명시적 규칙으로 보존한다. Unity는 새 규칙의 예고·선택 단계에서 적 개체와 전투 HUD를 노출하지 않고 `AwaitingCombat` 뒤에만 전투 표현을 연다.
- 관계: D-126~D-138의 생존·전투 기능을 삭제하지 않고 D-116의 경관 탐색과 D-122~D-125의 L2 스트리밍을 기본 플레이 순환으로 승격함

## D-143 지역 공공데이터는 원본을 보존하고 LOD별 대표 요약과 상세 조회로 나눈다

- 상태: `Accepted`
- 결정일: 2026-08-15
- 결정: 공유 공공데이터와 공간 파생 node는 전량 보존하고 Unity에는 `region-presentation-summary.v1`이 만든 L0 8개·L1 32개·L2 120개 표현 슬롯만 전달한다. 기본 슬롯은 분포 대표 60%, 지역 특색 25%, 게임 맥락 15%이며 한 분류의 최대 점유율은 40%다. 표현하지 않은 원본은 삭제하지 않고 분류별 `화면생략대표원본수`로 보존한다.
- 공개 경계: 타일·지역 요약에는 실제 상호명을 넣지 않는다. 검증된 건물–공개 사업장 관계가 있고 사용자가 가까이에서 명시적으로 상호작용할 때만 별도 공개 상세 조회가 상호명·분류·출처·기준일을 제공하며 대표자·연락처·사업자등록번호는 제공하지 않는다.
- 권위와 호환: 지역 요약은 Simulation World 파생 DB의 표현 입력이며 운영 사실이나 Session 상태를 확정하지 않는다. 기본 요약 hash와 현재 게임 상황 오버레이 개정을 분리하고 Synty·URP는 의미 기반 `VisualKey`만 해석한다. 평창은 일반 Engine에 넘기는 첫 Recipe이며 일반 Selector와 요약 Engine에 지역 코드를 고정하지 않는다.
- 관계: D-016의 서버 상태 투영, D-032의 파생 DB, D-122~D-125의 L2 스트리밍과 D-140의 코드 탐색·권위 경계를 지역 정보 축약에 적용함

## D-144 Synty 팩은 기술 대장·팩별 기준·의미 구성·검토 계획을 거쳐 Scene에 적용한다

- 상태: `Accepted`
- 결정일: 2026-08-16
- 결정: PolygonNature·Farm·Town·City 원본을 Scene에 바로 배치하지 않는다. 보유 Prefab은 먼저 표현 비용과 분류를 담은 기술 대장으로 전수 등록하고, 팩별 Markdown 기준과 사람이 검토한 의미 구성 대장이 `VisualKey`·`CompositionKey`로 승격한다. AreaSet 계획은 구획별 로컬 저작 경계와 세계 Anchor 변환, 토지피복·역할·경사·수계 조건, 기획서와 네 기준 문서의 SHA-256을 함께 고정한다.
- 적용 관문: 기본 JSON과 사람 보정 JSON을 병합해 검증한 뒤 구획별 Staging Prefab만 생성한다. 저장 Scene 적용은 별도 승인 기록이 기획서 bundle·기본·보정·병합 hash와 모두 일치할 때만 허용한다. 기술 대장 등록, Staging 생성, Scene 적용과 Game View 검증은 서로 다른 증거 단계다.
- 권위와 호환: Synty 원본 Prefab과 Material, `.meta` GUID는 수정하지 않는다. 유료 원본 파일명·경로·GUID는 공개 문서나 업무·공간 계약에 저장하지 않고, 팩 교체가 공공 공간자료·Simulation 상태·업무 완료를 변경하지 않게 한다.
- 관계: D-116의 공공데이터 경관, D-118의 Synty 역할 표현, D-122~D-125의 타일 스트리밍, D-143의 지역 요약을 문서 우선 배치 검토 절차로 연결함

## D-145 미완료 작업은 증거 단계 원장으로 관리하고 중앙 L2 실자료부터 종단 완결한다

- 상태: `Accepted`
- 결정일: 2026-08-16
- 결정: 계획, 코드, 자동 시험, 로컬 DB·자료 적용, Runtime 확인과 원자료부터 Unity까지의 종단 확인을 E0~E6으로 분리한다. `eng/execution-ledgers/simulation-unity.json`을 원본으로 삼고 한국어 실행 트리는 결정적으로 생성한다. 첫 종단 완결 대상은 대관령 중앙 L2 `kr5186:l2:700:1145`이며 전국 확장은 이 타일의 원자료·파생 DB·HTTP·Unity 검증 뒤 진행한다.
- 공간 산출물: Copernicus DEM과 ESA WorldCover 원본은 private storage에 두고 Git에는 원본 hash·CRS·해상도·NoData·수직 기준·산출물 hash·형식·표본 크기를 가진 manifest만 둔다. `PhysicalElevation`은 배치 판정 근거이고 Unity 높이 과장은 표현 전용이다. 확인되지 않은 수직 기준이나 세분류 위치는 꾸며내지 않는다.
- 전달 경계: 파생 DB는 산출물 계보와 객체 키를 저장하고 서버는 루트 경로 이탈, 바이트 길이와 SHA-256을 검증해 본문을 제공한다. Unity는 다시 hash를 확인하고 상세 범위에서만 Mesh를 만든다. 이 표현 Mesh는 Collider나 Simulation 권위를 갖지 않는다.
- 완료 판정: 코드나 Fixture 시험만으로 E6 또는 완료로 올리지 않는다. 실제 저장 Scene의 서버 연결·Play Mode·Game View, mask 기반 Synty 배치와 경계 연속성이 남아 있는 동안 관련 항목은 진행 중으로 유지한다.
- 관계: D-122~D-125의 L2 스트리밍, D-139의 구조 검증, D-140의 생성 코드 지도와 D-143~D-144의 지역 요약·Synty 적용 관문을 실자료 실행 순서로 묶음

## D-146 AreaSet은 문서 중심 상위 컨테이너이고 LandscapeGraph는 독립 조립·스트리밍 단위다

- 상태: `Accepted`
- 결정일: 2026-08-16
- 결정: `AreaSet`은 Area·ScenarioRoute·여러 `LandscapeGraph`와 Graph 관계를 묶는 지역 세계 정의서다. `LandscapeGraph`는 하나 이상의 Area·Tile을 참조하는 독립 공간 조립·검증·부분 재생성·스트리밍 단위이고, `Tile`은 공간 Layer 산출물과 캐시 단위다. Area와 Graph를 1:1로 고정하지 않는다.
- 문서 권위: 사람이 작성한 Markdown은 의미·근거·미해결을 설명하고 JSON만 실행 권위를 가진다. compiler는 Markdown 참조와 JSON 참조가 정확히 같은지 검증하고 두 SHA-256을 분리한다. DB 실행 상태 문서는 결정적으로 생성하며 직접 수정하지 않는다.
- 연결 경계: Graph 내부 Node는 다른 Graph의 Node를 직접 참조하지 않는다. Graph 간 연결은 AreaSet의 `GraphRelation`과 양쪽 `ExternalConnectorStub` 쌍으로만 검증한다. 연결 불일치는 꾸며내지 않고 `GraphConnectorUnresolved`로 기록한다.
- Runtime 경계: 서버의 Graph 빌드 상태와 Unity의 플레이어별 `Prepared / Active / Cached` 상태를 분리한다. 기존 Tile API는 한 Recipe 개정 동안 Graph 결과를 Tile별로 투영하는 호환 조회이며 새 공간 권위를 갖지 않는다. 업무·공간 계약에는 Synty Prefab 경로와 `.meta` GUID를 넣지 않는다.
- 관계: D-116의 공공데이터 경관, D-122~D-125의 Tile 스트리밍, D-144의 문서 우선 Synty 적용과 D-145의 증거 단계 원장을 AreaSet 상위 구조로 연결함

## D-147 공간과 Simulation은 세계 상호작용 단위로 종단 연결한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: 공간 기반 시설과 Simulation 기반 시설을 각각 먼저 일반화하지 않고, 실제 세계 행위 하나를 결정–작업–효과와 공간 요구–능력–예약–상태 변화까지 종단 완결하는 `WI` 작업 단위로 구현한다. `WI`는 새 Domain 엔티티나 공개 계약이 아니라 설계·실행·검증 원장의 상위 식별자다. 첫 단위는 진부 Hub 입고 검수와 창고 적재다.
- 책임 경계: Simulation ActionCode가 필요한 공간 능력·용량·기간을 서버에서 정하고, 공간 정의는 가능한 활동과 기본 용량을 제공한다. 가변 점유·예약은 별도 공간 Simulation이나 개정을 만들지 않고 Session 상태와 기존 `WorldRevision`에 포함한다. 클라이언트는 선택적 선호 공간 식별자만 제출할 수 있고 Unity·Synty 표현은 업무 완료 권위를 갖지 않는다.
- 근거 관문: 실제 진부 Hub 경관 노드가 준비되기 전에는 `Scenario` 공간 근거로 규칙을 검증하되 공공데이터 근거처럼 표현하지 않는다. 이후 승인된 공간 능력 연결을 통해 같은 WI를 경관 그래프 노드로 승격하며 Simulation 규칙과 기존 API를 교체하지 않는다.
- 호환: 공간이 없는 기존 Session과 `simulation-save.v1` 재생 해시를 보존한다. 공간 상태·예약·취소 계보가 필요한 Session은 `simulation-save.v2`를 사용한다. 취소는 작업 계보에 연결된 예약·배정·임시 상태만 되돌린다.
- 관계: D-127의 서버 권위 세계 사건, D-132의 저장·재생, D-139의 호환 리팩토링, D-145의 증거 단계와 D-146의 AreaSet·경관 그래프 책임을 실제 업무 상호작용으로 연결함

## D-148 세계 상호작용 단위의 기본 구현 완료는 E3이고 실세계 승격은 별도로 관리한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: 세계 상호작용 단위는 행위 계약·서버 코드·자동 시험·시험 HTTP 호스트·저장 재생을 포함한 `E3`를 기본 구현 완료로 본다. 실제 공간 DB·경관 그래프·저장 Scene·실행 중 서버·Unity Play Mode·Game View는 구현 완료와 분리한 통합 승격 `E4~E6`으로 관리한다. Unity가 없는 E3 완료를 미완료로 낮추지 않고, 반대로 E3를 실제 공간·화면 종단 증거로 과장하지 않는다.
- 실행 원장: `eng/execution-ledgers/world-interactions.json`을 세계 행위 대장의 기계 원본으로 사용하고 한국어 대장을 결정적으로 생성한다. 항목은 사람이 확정하는 명령, 부모 작업 안의 자동 전이, 여러 행위가 공유하는 판정 정책을 구분한다. 공간 능력·예약 같은 공통 계약은 실제 두 개 이상의 세계 행위에서 반복된 뒤에만 추출한다.
- 첫 인과선: 결정적 300kg 감자 수확에서 집하·포장·상차·Farm→Hub 이동·하차·인수·입고 검수·창고 적재까지를 첫 E3 공급선으로 삼는다. 운송 공간은 상차 지점·운송 회랑·하차 지점을 역할별로 분리하며, 출발 시 상차 예약을 반환하고 도착 시 하차 예약을 반환한다. 이 첫 인과선의 공간 근거는 `Scenario`이므로 공공데이터나 실제 경관 그래프 근거로 표현하지 않는다.
- 호환: 클라이언트는 역할별 선호 공간 고유 식별자만 보낼 수 있고 필요한 능력·용량·기간은 서버 규칙이 정한다. 기존 단일 공간 작업, 공개 API 경로·JSON 이름·상태 코드·저장 자료를 유지하며 역할별 공간 정보가 없는 기존 요청의 재생 해시를 바꾸지 않는다.
- 관계: D-132의 저장·재생, D-139의 호환 리팩토링, D-145의 증거 단계와 D-147의 세계 상호작용 구현 단위를 기본 개발 완료선과 실세계 승격선으로 분리함

## D-149 WI E3 승격은 핵심 인과선·공통 규칙·문서·전체 회귀 순으로 나눈다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: 37개 WI를 한 번에 동일 깊이로 검증하지 않는다. `P0`는 농장 생산→Hub→마트→주문·수령·소비 인과선, `P1`은 NPC 배정·취소·시설 수리·지역 발견·역할·활동·턴 마감, `P2`는 원장·문서 일치, `P3`는 전체 회귀·빌드로 구분한다.
- 완료 판정: 상위 우선순위의 집중 시험이 통과하기 전에 하위 단계의 문서 정리나 전체 회귀를 먼저 실행하지 않는다. E3는 자동 시험 증거를 요구하지만 E4~E6 실제 공간·Unity 증거를 포함하지 않는다.
- 관계: D-145의 증거 단계, D-147의 WI 구현 단위와 D-148의 E3 기본 완료선을 지속 가능한 작업 순서로 구체화함

## D-150 E4 승격은 WI 공간 폐루프와 Graph 계보를 기준으로 개별 판정한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: 경관 Graph 전체가 완성되지 않아도 WI가 사용하는 시작 공간, 능력, 용량, 선행·후속 Edge 또는 외부 연결점과 결과 인계가 모두 닫히고 사용 경로가 미해결 구간을 통과하지 않으면 해당 WI만 E4 승격 후보가 된다. 이 `WI 공간 폐루프`는 새 증거 단계나 Domain 엔티티가 아니라 승격 검사 규칙이다.
- 의미와 근거: 공간 역할은 장소가 무엇인지, 공간 능력은 그 장소에서 무엇이 가능한지를 나타낸다. `1 slot`은 물리 면적이 아니라 해당 WI를 동시에 한 건 수행할 수 있는 Simulation 작업 용량이며 `Scenario` 또는 `ReviewedDesign` 근거를 실제 면적·공공데이터 근거처럼 표현하지 않는다.
- 공급자 경계: 경관 Graph 공간을 요청한 WI는 Graph 개정·해시·Node·연결이 없거나 달라지면 차단하고 Scenario 공간으로 자동 대체하지 않는다. 동일 ActionCode·Preview·Confirm·Task·Effect를 유지하고 공간 정의 해결 결과의 계보만 Graph 근거로 승격한다.
- 완료 판정: 계획·대장·자동 검증은 `E4 준비 완료`, Graph·파생 DB 적용은 `E4 진행 중`이며 저장 Scene에 Graph 식별자·개정·해시를 반영해야 공식 E4다. Play Mode·Game View는 E5 이후다.
- 관계: D-145의 증거 단계, D-146의 Graph 부분 조립, D-147의 공간–Simulation 접점과 D-148의 구현·통합 증거 분리를 개별 WI 승격 규칙으로 구체화함

## D-151 E4~E7은 장소·경관·공공데이터·실제 플레이로 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: E3 구현 완료 뒤의 통합 승격을 `E4 실제 의미 공간 귀속 → E5 이동 가능한 경관 조직 → E6 WI 필수 공공데이터 경관 연결 → E7 실제 플레이 폐루프`로 분리한다. 공통 단계 정의는 `eng/execution-ledgers/evidence-stages.json`을 단일 기준으로 사용하고 Simulation–Unity 실행 원장과 37개 WI 원장은 모두 E7을 통합 목표로 삼는다.
- E4·E5: E4는 WI가 실제 `LandscapeGraph` Node의 역할·능력·용량과 필수 공간 전이에 연결된 상태다. E5는 도로·교차로·Gate가 만든 Block에 공간 역할과 `LandscapePattern`을 배정하고 WI 공간 사이의 결정적 이동 경로를 닫은 상태다. 저장 Scene이나 사람 조작 여부만으로 E4·E5를 판정하지 않는다.
- E6: 선정 WI의 공간 판정에 필요한 최소 공공데이터만 E5 경관에 연결한다. 출처·기준일·좌표계·원본과 파생 hash·근거 수준·한계를 보존하며, 관계없는 자료의 부재는 승격을 막지 않는다. `Scenario`, `ReviewedDesign`, 통계 배분과 관측 위치를 서로 바꾸어 표현하지 않는다.
- E7: 사람이 실제 Simulation 서버와 저장된 `SimulationWorldShell`에서 이동·선택·Preview·Confirm·Tick·재조회를 수행하고 Game View와 Console로 확인해야 한다. 자동 WI는 부모 명령 뒤의 자동 전이를 관찰한다. Unity는 입력과 표현만 담당하며 서버가 성공·효과·개정을 확정한다. 패키징 빌드·배포·운영 공개는 E7의 기본 완료 조건이 아니다.
- 이관: E0~E3 증거는 유지한다. 이전 정의의 E4·E5 값은 새 의미로 자동 승계하지 않고 최대 E3으로 보수적으로 재분류한 뒤 새 관문 증거로 다시 승격한다. D-145의 E0~E6 종단선, D-148의 E4~E6 통합선과 D-150의 저장 Scene 중심 E4 완료 판정은 이 결정으로 구체화·대체한다.

## D-152 E4는 WI 공간 모판이고 E5는 실제 지역 경관 조립이다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: `E4`는 하나 이상의 E3 WI를 공간 역할·능력·업무 용량·내부 관계·외부 연결구와 결합한 위치 독립적이고 재사용 가능한 `WI 공간 모판` 완료선이다. E4는 E3를 대체하지 않고 포함하며, 모판 정의로 만든 `Scenario` 공간에서 동일한 Preview·Confirm·Task·Tick·Effect·Save/Replay를 다시 통과해야 한다.
- E4 경계: E4 실행 권위는 버전 관리 JSON이고 사람이 읽는 Markdown은 의미·근거·한계를 설명한다. 모판은 허용된 경관 문법 `compositionKey` 후보와 회전·크기 제약을 가질 수 있지만 `AreaSet`, `LandscapeGraph`, Tile, 절대좌표, 실제 도로, Prefab·GUID·Material·Scene 경로를 가질 수 없다. `1 slot`은 물리 면적이 아니라 해당 작업을 동시에 한 건 예약할 수 있다는 Simulation 업무 용량이다.
- E5 경계: `E5`는 실제 AreaSet·LandscapeGraph에서 도로 Network와 Junction이 만든 Block에 승인된 E4 모판 인스턴스를 배치하고, 추상 연결구를 실제 Node·Edge·GraphRelation에 결속하여 이동 가능한 지역 경관을 닫는 단계다. 기존 대관령 Farm의 공간 능력 연결과 Graph 폐루프 판정은 폐기하지 않고 E5 배치 후보 증거로 이관한다. 실제 Graph 모드에서 미해결 공간을 Scenario로 자동 대체하지 않는다.
- 계약과 책임: WI 공간 모판은 새 운영 Domain 엔티티나 공개 API·저장 원장이 아니다. 기존 Unity 배치 객체 모판과 위치 독립성·결정적 해시·검토 승인 원칙은 공유하지만, WI 공간 모판 계약과 Unity 표현 모판 계약은 서로 분리한다. 새로운 `LandscapePattern` 계약을 만들지 않고 기존 156개 기준 경관 문법을 허용 후보로 재사용한다.
- 후속 단계: E6는 E5 경관에 필요한 공공데이터 원본·파생·출처·해시 계보를 연결하고, E7은 실제 서버와 저장된 `SimulationWorldShell`에서 플레이어가 이동·선택·Preview·Confirm·Tick·재조회를 수행해 서버 상태와 Game View를 함께 확인한다.
- 호환과 이관: 37개 WI의 E3 구현 증거, ActionCode, API, 상태 코드, Save/Replay 형식과 기존 Fixture 해시는 유지한다. D-151의 E6·E7 정의는 유지하고 E4·E5 정의만 이 결정으로 대체한다. D-150의 Graph 공간 폐루프는 E4 완료 조건이 아니라 E5 후보 검사로 재분류한다.

## D-153 E 증거 단계와 H 공간 포함 계층을 분리한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: `E0~E7`은 구현·통합 증거의 깊이만 나타내고, 공간 구조의 포함 깊이는 `H1 WI 공간 모판 → H2 LandscapeBlock → H3 LandscapeGraph → H4 AreaSet`으로 별도 분류한다. 기존에 검토한 `S4~S7` 표기는 같은 숫자의 E 단계와 의미가 충돌하므로 사용하지 않는다.
- 증거와 계층의 관계: E4는 H1 모판 안에서 E3 WI를 다시 실행한 증거다. E5는 승인된 H1 인스턴스를 실제 H2에 배치하고 H2→H3→H4의 도로·연결 지점·GraphRelation·이동 경로를 닫은 증거다. E6는 E5 결과의 공공데이터 계보를, E7은 실제 서버와 저장 Scene에서의 플레이어 폐루프를 검증한다.
- 상태 경계: H 코드는 리소스 종류와 포함 관계를 나타낼 뿐 완료·승격 상태가 아니다. 현재 H3 Graph 다섯 개와 H4 AreaSet 하나가 정의되어 있어도 실제 H2 Block이 없으므로 E5가 아니다. H2는 실제 도로와 경계 근거가 준비될 때까지 정의 수 0인 예약 단계로 유지한다.
- 제외 축: 기준 경관 문법 156개는 H1의 허용 후보와 H2·H3 조립에서 사용하는 공간 문법 어휘이며 H 계층이 아니다. Tile L0~L2는 원자료 처리 해상도, Area는 의미 범위, 경관 완결 영역은 사람 검토 범위, ScenarioRoute는 이동 의미 참조이므로 H 계층에 넣지 않는다.
- 호환: H 분류는 별도 기계 대장에서 리소스 종류와 기준 정의를 대조하여 계산한다. 기존 WI 공간 모판·LandscapeGraph·AreaSet 실행 JSON, 정의 SHA-256, 공개 HTTP 계약, 파생 DB schema와 저장 상태에는 H 필드를 중복 저장하지 않는다.

## D-154 모판을 H1~H4 상향 조립 공간 구성 재고로 확장한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: `모판`은 H1만의 기술 자원명이 아니라 `H1 작업공간 모판 → H2 블록 모판 → H3 경관 모판 → H4 지역 모판`으로 상향 조립되는 공간 구성 자원 계열이다. 기술 계약과 stable ID에서는 기존 `WiSpatialSeedbed`, `LandscapeBlock`, `LandscapeGraph`, `AreaSet` 명칭을 유지한다.
- 재고 경계: 위치 독립적인 후보·승인 참조인 `설계 재고`, 버전 관리되는 실제 H 자원인 `정의 재고`, 특정 부모 공간에 배정·배치된 `배치 재고`를 분리한다. 현재 확인할 수 없는 실제 배치 수량을 꾸며내지 않으며 H3·H4 정의가 있어도 실제 H2와 이동 폐루프가 없으면 E5가 아니다.
- 상향 조립: 상위 재고는 하위 재고의 정확한 revision과 결정적 hash를 참조하고 순환 포함을 금지한다. 후보가 정의 권위를 얻으려면 현실 공간 근거와 사람 검토가 필요하며 Synty 자산이나 Unity Scene은 자동 승격 권위를 갖지 않는다.
- 축 분리: H는 공간 자원 종류, `CandidateForReview / ApprovedReference / DefinedPartialAssemblyReference`는 설계 상태, `Unallocated / Allocated / Placed`는 배치 상태, E0~E7은 구현·통합 증거 깊이다. 어느 상태도 H 숫자나 E 단계에서 자동 유도하지 않는다.
- 호환과 제외: 기존 schema version, 공개 계약, 저장 상태와 stable ID는 변경하지 않고 H 코드는 공통 공간 구성 재고 대장에서만 계산한다. Unity 배치 객체 모판과 규칙 실험 모판은 각각 표현·시험 adapter로 유지하며 H 공간 자원으로 자동 편입하지 않는다.
- 관계: D-152의 H1 실행 모판 경계와 D-153의 E/H 축 분리를 유지하면서, D-153에서 H1에 한정했던 사람 중심 `모판` 용어를 H1~H4 전체 공간 자원 계열과 재고 책임으로 확장함

## D-155 Synty 상향식 공간 재고는 공식 H 계층과 분리해 축적한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: 보유한 Nature·Farm·Town·City 팩을 AreaSet 적용 전에도 연구할 수 있도록 `H1 공간 설계 카드 → H2 블록 조립법 → H3 경관 청사진`을 별도 상향식 설계 지식으로 축적한다. 초기 `catalog.v1`과 항목별 `catalog.v2`는 호환 입력으로 보존하며 현재 v3 수량과 문법 유도 관계는 D-156에서 확장한다.
- 승인 경계: `IdeaInventory → ExploratoryInventory → CandidateForReview → ApprovedReference`를 설계 지식 상태로 사용한다. WI가 아직 없으면 예상 게임 행위를 명시할 수 있지만 존재하지 않는 WI 식별자를 만들지 않는다. 기존 `ApprovedForSimulation` H1 다섯 개만 공식 H1 정의 수에 포함하며 사람 승인이나 실제 도로·경계·지형 근거 없이 후보를 공식 H1·H2·H3 또는 E4·E5 증거로 자동 승격하지 않는다.
- 표현 경계: 상향식 재고는 기존 156개 기준 경관 문법의 A/B/C 후보를 참조한다. Synty Prefab·GUID·Material·Scene 경로는 Unity 표현 대장에서만 연결하며 공간 StableId, Simulation 상태 또는 운영 권위를 만들지 않는다.
- 결정성: 기계 원본과 사람이 읽는 Markdown의 지시문 일치, 파일별 SHA-256, 팩 Prefab 수량, E3 WI 존재, 승인 H1 참조, 경관 문법 키, H1→H2→H3 참조와 금지 필드를 검증한다. 조회 도구는 WI·예상 행위·공간 능력·팩·위상으로 조합 후보와 공백을 제안하지만 승인·배치·AreaSet·경관 그래프 권위를 갖지 않는다.
- 관계: D-144의 Synty 표현 후단, D-147의 WI 공간 상호작용, D-152의 H1~H4 포함 계층을 보존하면서 AreaSet 하향식 기획과 별개로 표현 탐색형 설계 지식을 미리 축적함

## D-156 기준 경관 문법은 검토된 조립법으로 H1~H4 설계 후보를 유도한다

- 상태: `Accepted`
- 결정일: 2026-08-17
- 결정: 기준 경관 문법 52개 의미군·A/B/C 156개 변형을 H 계층에 직접 편입하지 않고 검토된 조립법의 입력으로 사용한다. Nature·Farm·Town·City 32개 의미군은 팩 단독 표현 H1 32장, Network 12개는 H2 블록 골격, Transition 8개는 H3 연결·전환 입력으로 사용하며 여러 H3와 세계 주제로 위치 독립 H4 지역 청사진 후보 5개를 유도한다.
- 재고와 호환: `catalog.v3`는 기존 행동 공간 H1 36개와 팩 표현 H1 32개를 `InteractionSpace / PackExpression`으로 분리하고 기존 v2 StableId·H2 18개·H3 10개 참조를 유지한다. 공통 재고는 H1 68/5, H2 19/0, H3 10/5, H4 5/1의 설계/정의 수를 별도로 보고한다.
- 결정성: `grammar-derivation-recipes.v1.json`에 명시된 조립법만 실행하고 문법·하위 지식·조립법 hash로 입력 지문을 만든다. 52개 의미군과 156개 변형의 계보, A/B/C 완전성, Markdown·JSON·파일 hash와 반복 생성 무변경을 자동 검증한다.
- 권위 경계: 팩 표현 H1에서 WI·능력·업무 용량을 자동 추론하지 않는다. H4 청사진은 실제 AreaSet이 아니며 실제 지역 코드·좌표·DataRequirement·LandscapeGraph StableId를 갖지 않는다. 사람의 세계 의도와 현실 근거 없이 후보를 공식 H 정의나 E 증거로 자동 승격하거나 Scenario로 대체하지 않는다.
- 관계: D-153의 문법/H 계층 분리, D-154의 H1~H4 상향 조립, D-155의 Synty 탐색 재고를 유지하면서 문법에서 H 설계 후보로 이어지는 결정적 계보를 추가함

## D-157 오픈 월드는 고정 좌표 경계가 아니라 H4 의도와 H3·H2 Streaming Coverage로 연다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `SimulationWorldShell`은 계속 하나의 영속 실행 Scene으로 유지하고, 플레이어를 하나의 평창 고정 사각형 안에 가두지 않는다. 이동 가능 범위는 현재 준비된 Tile·H3 Graph·H2 Block의 `Streaming Coverage`에서 파생하며, 카메라와 플레이어는 이 동적 범위를 표현 경계로 사용한다.
- 안전 경계: `OPEN-WORLD-0`에서는 기존 `공간TileStreamingController`의 추적 Tile 범위를 이동·전술 카메라의 동적 Coverage로 사용한다. `공간안전이동Gate`는 목적지가 추적 범위 안이고 지면 충돌 근거가 있을 때만 진입을 허용한다. Streaming이 없거나 초기화되지 않은 실험 Scene은 기존 `플레이어경관Profile` 사각형을 fallback으로 유지한다.
- 상향 조립: H4 AreaSet은 어떤 H3 Graph를 연결해 하나의 지역 경험으로 열지 정한다. 플레이어 접근에 따라 H3는 `Declared → Prepared → Active → Cached/Unloaded`로 전이하고, H3 내부에서는 실제 도로·경계 근거의 H2 Block과 승인된 H1 모판 인스턴스만 상호작용 공간을 제공한다. Tile은 지형·표현 자료의 Streaming 단위이며 H 계층을 대체하지 않는다.
- 권위 경계: 이동·카메라·Graph 가시화는 Unity 표현 상태이며 `WorldTick`, 서버 개정, 업무 완료를 바꾸지 않는다. 새 지역 발견, 여행 시작, 물류 이동 같은 게임 효과는 별도 Preview·Confirm과 서버 Command가 확정한다. 미해결 Graph나 지형은 Scenario 공간으로 자동 대체하지 않는다.
- 완료 판정: 고정 사각형 제거만으로 오픈 월드, H2, E5 또는 Farm→Hub 폐루프를 완료했다고 판정하지 않는다. 실제 완료에는 GraphRelation 양쪽 연결점, 지형 Collider, H2 이동 경로, H1 상호작용 귀속, 저장 Scene Play Mode 이동과 필요한 서버 재조회 증거가 함께 필요하다.
- 관계: D-153의 E/H 축 분리, D-154의 H1~H4 공간 구성 재고, `SimulationWorldShell` 단일 실행 Scene 원칙을 유지하면서 오픈 월드의 런타임 경계를 동적 Streaming Coverage로 구체화함

## D-158 LH 엔진은 L 해상도와 H 의미 권위를 직교시키고 승인 H4 안에서 결정적으로 선행 생성한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `LH 엔진`에서 `L0 8km / L1 2km / L2 500m / L3 125m`는 생성·적재 해상도이고 `H4 AreaSet / H3 LandscapeGraph / H2 LandscapeBlock / H1 작업공간`은 공간 의미·포함·권위 계층이다. 기본 대응은 L0→H4, L1→H3, L2→H2, L3→H1이지만 두 축을 같은 완료 상태로 취급하지 않는다. 플레이어 주변 창은 L3 기준 상세 3×3, 이동 준비 5×5, 선행 자료 9×9이며 셀 경계 25% 전에 이동 방향의 다음 창을 요청한다.
- 생성과 권위: 서버는 `WorldSeed + GeneratorVersion + L3 Cell + 생성 Layer`로 기본 배치와 인접 연결 경계를 결정하고, 고정 농장 Anchor와 절차형 주변 경관 후보를 함께 반환한다. 생성 범위는 승인된 H4 경계 안으로 제한하고 H3·H1 승인 참조를 보존한다. 실제 도로·경계 근거가 없는 H2는 계속 `IdeaInventory` 후보이며 절차 생성 성공으로 H2 정의·E5 증거를 만들지 않는다.
- 실행 준비: 셀은 `Requested → DependenciesReady → GeneratedDataReady → VisualPrepared → PlayerTraversalReady → Active/Cached/Released`로 관리하고 지형 시각, 충돌, 연결, H1 상호작용, NPC 경로, 계절 표현 능력을 분리한다. 최신 요청 epoch만 메인 스레드의 제한된 조립 예산 안에서 적용하며, 셀 Root와 로컬 Synty 대장을 재사용한다. 첫 개정은 Addressables나 제3자 절차 생성 패키지를 필수로 하지 않는다.
- 계절과 저장: 계절은 서버 날짜를 기준으로 봄·여름·가을·겨울 각 28일이며 기본 생성 hash와 계절 표현 hash를 분리한다. Save/Replay는 생성된 전체 월드를 저장하지 않고 `WorldSeed`, `GeneratorVersion`, H4 경계 revision/hash와 발견·상태 변경·배치·제거 Delta만 저장한다. Unity는 공유 계절 Material 변형과 전역 shader 값으로 표현하며 원본 Synty Material과 Simulation 자원 원장을 변경하지 않는다.
- 관계: D-132의 Save/Replay, D-153의 E/H 축 분리, D-157의 Streaming Coverage와 `SimulationWorldShell` 단일 실행 Scene 원칙을 125m 플레이 셀 생성·적재 계약으로 구체화함

## D-159 WI별 E/H 성립 상태는 후보 계보와 실행 증거를 분리해 LH 인계 입력으로 생성한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 37개 WI의 E 단계, 공간 참여 여부, 공식 H1 실행 성립, H1~H4 설계 후보 계보, Graph binding과 LH 인계 가능 상태를 `world-interactions`, 공식 H1 대장, 상향식 설계 재고와 공간 능력 대장에서 결정적으로 합성한다. 이 결과는 새 업무 Domain 엔티티나 운영 상태가 아니라 설계·검증·공간 생성 인계 대장이다.
- 판정: 승인 H1에서 다시 실행된 13개만 `EstablishedH1`, 설계 지식만 연결된 WI는 `CandidateLineage`, 필수 공간인데 H1 설계가 없으면 `MissingRequired`, 공간을 요구하지 않는 명령·정책은 `NotApplicable`로 기록한다. 후보 H2·H3·H4, Graph binding 또는 E5 참조가 존재해도 실제 H2 정의와 이동 폐루프가 없으면 E5나 성립 H 단계를 올리지 않는다.
- 우선순위: P1은 수확→집하→포장→상차, P2는 Hub 입고·검수·보관, P3는 Hub 출고·마트, 나머지는 P4로 관리한다. P1 공간 구성은 절대좌표 없이 승인 H1·능력·업무 용량·내부 관계·외부 연결구와 LH 인계 제약을 선언하고, 실제 도로·Block 경계와 양쪽 Graph 연결이 생길 때까지 H2 후보로 유지한다.
- LH 경계: LH 엔진에는 승인 H1만 상호작용 입력으로 넘기고 설계 후보는 Preview 제안에만 사용할 수 있다. 후보 H2는 플레이어 통과 권위를 주지 않으며 Graph 요청 실패를 Scenario로 대체하지 않는다. Synty 표현 H1과 기준 경관 문법은 배치 후보를 제공하지만 WI·능력·용량을 자동 확정하지 않는다.
- 관계: D-147의 WI 종단 단위, D-152의 E4/H1·E5/H2~H4, D-154의 공간 재고, D-156의 문법 유도 계보와 D-158의 LH 선행 생성을 연결하는 기계 인계 경계를 추가함

## D-160 싱글 플레이 LH 지도 생성은 로컬 엔진을 기본 권위로 하고 서버 연결은 선택 동기화로 둔다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `SimulationWorldShell`의 기본 실행은 서버 접속 없이 Unity의 `로컬공간LHWorldEngine`이 플레이어 L3 위치, 로컬 월드 시드, 생성기 버전과 로컬 달력을 입력으로 주변 지도를 결정적으로 계산한다. 플레이어 주변 상세 3×3·이동 준비 5×5·선행 자료 9×9와 경계 25% 선행 요청은 그대로 유지하며, 상태 화면은 로컬 싱글 플레이 생성 여부를 명시한다.
- 생성 범위: 로컬 엔진은 승인 H4 범위 안에서 H4·H3 승인 참조, H2 `IdeaInventory`, 승인 H1 상호작용 참조를 분리하고, 수확 기준작의 네 고정 거점과 셀마다 3~5개 절차 배치, 동서남북 공유 경계 해시를 계산한다. 같은 시드·생성기 버전·셀은 같은 기본 배치를 만들고 계절은 별도 표현 해시에만 반영한다. H4 밖 셀은 생성하지 않고 미지원 범위로 반환한다.
- 서버 선택 경계: D-158의 LH 계약과 서버 구현은 멀티플레이, 원격 검증, 저장 동기화의 선택 경로로 보존하되 싱글 플레이 시작을 막지 않는다. 이 결정은 D-158의 “서버가 기본 생성 권위” 부분과 “서버 날짜가 기본 계절 기준” 부분을 대체한다. 로컬 Simulation 지도 생성은 주문·계약·입출고·결제 등 실운영 원장의 권위를 얻지 않으며, 실운영 효과는 계속 서버의 권한·개정·Command·Event 경계를 거친다.
- 저장과 호환: 저장 전체 지형 대신 로컬 월드 시드, 생성기 버전, 로컬 날짜, H4 경계 개정·해시와 Delta를 보존한다. 나중에 서버 동기화를 켤 때도 같은 입력 계약과 기본 배치 해시를 비교하고, 생성기 버전 불일치나 충돌을 조용히 덮어쓰지 않는다.
- 관계: D-132의 Save/Replay, D-157의 동적 Streaming Coverage, D-158의 LH 직교 계층·선행 생성 계약을 유지하면서 싱글 플레이 기본 실행 권위를 로컬로 전환함

## D-161 음식·화물 배달은 NPC 경로 이동을 기본 수행 방식으로 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 싱글 플레이 음식·화물 배달의 기본 수행자는 확정된 배차 NPC다. 플레이어는 후보 비교·우선순위·Confirm을 담당하고 차량 직접 운전이나 운전자 인계는 첫 범위에 포함하지 않는다. 음식과 화물은 같은 경로·Waypoint·체크포인트 계약을 사용하고 운송 종류와 이동 Profile만 분리한다.
- 진행 권위: NPC Transform 도착이나 animation 종료만으로 운송·전달·입고를 완료하지 않는다. 경로 StableId, Cargo 또는 주문 StableId, NPC, 차량, 순번과 예상 개정이 일치하는 체크포인트만 로컬 Simulation 진행을 만들며 도착 뒤 인수·검수·입고는 별도 업무 상태로 유지한다. 기존 서버 `I물류이동AuthorityClient`와 운영 권위 경계는 보존한다.
- LH 인계: 플레이어 위치가 Streaming의 주 관심점이고 활성 NPC의 다음 경로 셀은 보조 관심점이다. 다음 셀의 충돌·연결·`NpcNavigation` 능력이 준비되지 않으면 NPC는 경계에서 정지하고 준비 뒤 같은 구간에서 재개한다. 메모리나 준비 실패를 순간이동으로 숨기지 않는다.
- 공간 증거: 로컬에서 생성한 배달 경로는 `ScenarioProcedural` 표현 자료다. 실제 도로·경계 원본과 승인 H2, H3 Graph 연결이 없으면 NPC 완주 성공을 H2 정의, E5 경관 폐루프 또는 실운영 배달 증거로 승격하지 않는다.
- 관계: D-086의 도착·인수 분리, D-157의 Streaming Coverage, D-158의 셀 능력 분리와 D-160의 로컬 싱글 플레이 기본 경계를 NPC 물류 이동에 적용함

## D-162 Nature 생활권은 주인공의 상시 체류 세계이고 Farm·Town·City/Hub는 전문 경관 인스턴스다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 플레이어는 Nature 팩을 주축으로 한 생활·탐험 공간에 상시 체류하고, Farm·Town·City/Hub 팩을 주축으로 한 세 전문 경관을 필요에 따라 오가며 각 장소의 사건과 경영 업무를 3인칭 시점에서 관리한다. Nature 생활권은 전문 경관 전환 중에도 사라지지 않으며 한 번에 하나의 전문 경관만 활성화한다. 기본 시점은 탐험·전투·경영 모두 `TacticalThirdPerson`이고 1인칭은 허용되는 표현 선택으로 남긴다.
- 공간 구조: Nature는 `안전 생활핵 → 경계 완충대 → 조우 위험대` 순서의 위험 구역을 가지며, Farm·Town·City/Hub는 각각 Nature 생활권의 승인된 연결구로 진입한다. 이 배치는 H1~H4 후보와 WI를 재사용하는 위치 독립 설계 재고이며 실제 AreaSet·Graph·공공데이터·E5 증거를 자동 생성하지 않는다. 인스턴스 전환은 표현 위치와 활성 경관만 바꾸며 `WorldTick`, 개정 번호와 Simulation 업무 상태를 변경하지 않는다.
- 자산 관문: 실제 적 개체 표현은 `POLYGON Apocalypse` 설치와 검토가 확인된 경우에만 위험대의 Simulation 전용 조우 의도를 표시한다. 해당 팩이 없으면 상태를 `WaitingForApocalypseAssetPack`으로 유지하고 Generic Skeleton·다른 Synty Prefab·절차 Primitive로 자동 대체하지 않는다. 안전 생활핵·경계 완충대, 실운영 상태 또는 지원하지 않는 표현 키에서는 적 개체를 표시하지 않는다.
- 실행 경계: 첫 구현은 기계 경관 대장, 결정적 검증, 엔진 비의존 위험·전환 정책과 Unity 표현 전용 Controller까지만 제공한다. 저장 `SimulationWorldShell` 배선, 실제 몬스터 Prefab, Play Mode와 Game View는 자산 준비와 별도 Scene 적용 검토 뒤 수행한다. Unity Transform·Animator·NavMesh는 사건 완료 권위를 갖지 않는다.
- 관계: D-142의 경관 산책과 선택 전투, D-155~D-156의 Synty 상향식 설계 재고, D-157의 단일 World Streaming, D-160의 로컬 싱글 플레이 기본 권위와 `SimulationWorldShell` 단일 실행 Scene을 플레이어 중심 경관 구조로 구체화함

## D-163 전문 경관의 미해결 사건은 경로별 자연권 위협으로 전파한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: Farm·Town·City/Hub에서 확정된 안전하지 않은 선택과 사건 기한 초과만 `NatureToFarm`, `NatureToTown`, `NatureToCityHub` 경로의 자연권 위협 원인이 된다. 미리보기 차단과 일반 작업 취소는 포함하지 않는다. 해당 경로 남은 심각도 두 배에 전체 남은 심각도의 3분의 1 내림값을 더하고 12로 제한하며, 경로 자체 원인이 남아 있고 압력이 4 이상일 때만 최대 5개의 적 조우를 만든다.
- 인과와 회복: 사건 선택은 후속 WI와 분리하지 않는다. 안전 선택은 Farm 집하·포장, Town 검수·후방 적재·진열, Hub 입고 검수·창고 적재를 요구한다. 원인 WI 완료는 해당 사건을 완전히 해결하고, 자연권 전투 승리는 같은 경로의 가장 오래된 미해결 사건 심각도를 한 단계만 줄인다. 수동 방치 없이 시간이 흐른다는 이유로 자동 회복하지 않으며 기존 계절 농장 위협을 다시 입력하지 않는다.
- 권위와 호환: 선택·심각도·압력·적 수는 Session과 `WorldRevision` 안에서 서버가 확정한다. 클라이언트는 사건 선택 고유 식별자와 예상 개정·명령 고유 식별자만 제출한다. 지역 사건 명령과 자연권 승리 계보는 `simulation-save.v4`에 기록하고 v1~v3 읽기 호환을 유지한다.
- 표현 경계: Apocalypse 자산이 없어도 압력 경고는 유지하지만 적 개체는 만들지 않고 Generic 대체를 금지한다. Unity는 서버 상태를 표현할 뿐 사건 해결·압력 감소·전투 승리를 역추론하지 않는다. 상세 규칙은 [지역 사건과 자연권 위협 연결 규칙](../Architecture/지역사건-자연권위협-규칙.md)을 따른다.
- 관계: D-127의 서버 권위 세계 사건, D-147의 WI 공간 상호작용, D-161의 NPC 물류, D-162의 Nature 상시 생활권과 전문 경관 구조를 실패 결과의 세계 인과로 연결함

## D-164 상향식 공간 재고는 Nature 위협·회복 카드부터 작은 묶음으로 확장한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 상향식 재고 확장은 한 번에 전체 H 계층을 늘리지 않고 `Nature 위협·회복 → Farm 사건 대응 → Town → City/Hub → H2·H3 조립` 순서로 진행한다. 한 검토 묶음은 5~10장으로 제한한다.
- 첫 묶음: 위협 관찰, 사건 흔적 조사, 긴급 후퇴, 정화·복구, 안전 회복 야영지의 H1 탐색 카드 5장을 추가한다. 기존 Nature 기준 경관 문법과 연결하지만 WI·공간 능력·업무 용량·실제 좌표·LandscapeGraph·Unity Scene 권위를 자동 획득하지 않는다.
- 승격 경계: 실제 지역 사건 규칙과 연결할 새 WI가 아직 없으면 `anticipatedGameplayCodes`로 가능성만 기록하고, 사람 검토와 실행 계약 없이 공식 H1이나 E4 이상으로 승격하지 않는다.
- 관계: D-155~D-156의 공간 재고·문법 유도 원칙과 D-162~D-163의 Nature 생활권·위협 인과를 상향식 재고 확장 순서로 구체화함

## D-165 사건 대응 H1은 Nature에서 시작해 Farm과 Town으로 이어지는 H2 우선순위로 조립한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 기존 H2에 아직 포함되지 않은 사건 대응 H1은 `P1 Nature 위협 추적·대피와 복원·안전 회복 → P2 Farm 점검·격리와 손실 회복·복원 인계 → P3 Town 오염 통제와 회수 안내·구호` 순서의 여섯 H2 후보로 조립한다. 우선순위는 플레이어의 상시 체류 공간인 Nature의 위협·회복 폐루프를 먼저 만들고 전문 경관 사건의 결과를 그 폐루프에 인계하는 순서다.
- 조립 기준: H2 후보는 필수 H1 사이의 선행·후속 관계, 공유 위상, 입출력 연결구와 다음 H2로 이어지는 폐루프 출구를 가져야 한다. 같은 필수 H1 조합을 기존 H2가 이미 수용하면 중복 후보를 만들지 않는다. Network 기준 경관 문법은 조립 입력으로 연결하지만 H1의 WI·능력·용량을 자동 추론하지 않는다.
- 승격 경계: 여섯 항목은 위치 독립 설계 재고이며 공식 H2 정의가 아니다. 실제 도로·경계로 결정된 Block 면, 승인 H1 배치, 연결구 폐루프, Graph revision·hash와 공간 근거 검토가 없으면 자동 승격하거나 Scenario로 대체하지 않는다.
- 관계: D-155~D-156의 문법 유도와 H 계층, D-162~D-164의 Nature 중심 세계·사건 인과·재고 확장 순서를 H2 조립 실행 순서로 구체화함

## D-166 Nature·Farm·Town·City/Hub는 각각 독립 AreaSet 후보로 상향 조립한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 네 팩 중심 경관을 하나의 AreaSet에 평면적으로 넣지 않는다. Nature 생활·탐험, Farm 생산·생존, Town 생활·시장, City/Hub 물류를 각각 `H1 → H2 → H3 → H4 AreaSet 후보`로 독립 성장시키고, 상위 구성 대장은 후보 사이의 이동·물류 관계만 관리한다. 팩은 주축 표현 정보이며 AreaSet의 공간 권위가 아니다.
- 우선순위: P1 Nature는 상시 생활 세계이므로 위협 추적·대피 H2와 복원·안전 회복 H2를 자연 생활·위협·회복 H3로 묶고 Nature 생활·탐험 H4 후보에 연결한다. 이후 P2 Farm, P3 Town, P4 City/Hub 순서로 같은 상향 조립을 반복한다.
- 관계 분리: Nature↔Farm·Town·City/Hub는 플레이어 이동 관계다. Farm→City/Hub→Town은 Nature 생활권을 지름길로 쓰지 않는 별도 화물 관계다. 후보 연결은 실제 GraphRelation이 아니며 양쪽 AreaSet의 승인 Connector가 준비되기 전에는 이동 폐루프나 E5 증거로 승격하지 않는다.
- 권위 경계: H4 후보는 실제 AreaSet이 아니다. 실제 지역 세계가 되려면 사람의 세계 의도, 지역 범위, DataRequirement, H3 LandscapeGraph와 GraphRelation 승인이 필요하며 누락 근거를 Scenario로 대체하지 않는다.
- 관계: D-147의 AreaSet-first 공간 권위, D-155~D-156의 H1~H4 상향식 재고, D-162의 Nature 상시 생활권과 전문 경관 구조, D-165의 H2 조립 우선순위를 다중 AreaSet 구성으로 구체화함

## D-167 Farm AreaSet 후보는 생산 흐름과 사건 격리·회복 흐름을 함께 포함한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: P2 Farm은 생산·후처리 경관만 가진 AreaSet 후보로 끝내지 않는다. 노출 점검·사건 격리·기상 보호를 묶은 H2와 손실 회복·자연권 복원 물자 인계를 묶은 H2를 Farm 사건 격리·회복 H3로 조립하고, 고지대 생산 H3·생산 후처리 H3와 함께 Farm 생산·생존 H4 후보의 필수 경관으로 둔다.
- 인계: Farm의 `NatureRestorationOutput`은 Nature 복원·안전 회복 H2의 사건 입력 후보와 의미상 대응한다. 그러나 양쪽 실제 AreaSet의 승인된 외부 연결점과 GraphRelation이 없으면 자동 연결하거나 E5 이동 폐루프로 간주하지 않는다.
- 공간 경계: H2 조립 좌표는 `LocalMeters` 상대 설계값이다. 실제 도로·Block 경계·AreaSet 범위·공공데이터 근거가 없으므로 공식 H2나 실제 Farm AreaSet으로 승격하지 않는다.
- 관계: D-163의 Farm 사건→Nature 위협 인과, D-165의 P2 H2 우선순위, D-166의 팩별 독립 AreaSet 조립 원칙을 Farm 생산·생존 경관으로 구체화함

## D-168 Town AreaSet 후보는 시장 생활 흐름과 오염 통제·주민 구호 흐름을 함께 포함한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: P3 Town은 저층 생활·시장과 반품·회수 경관만 가진 AreaSet 후보로 끝내지 않는다. 재고 오염 점검·격리·정화 폐기 인계를 묶은 H2와 주민 회수 안내·근린 서비스·자연권 구호 물자 인계를 묶은 H2를 생활권 오염 통제·구호 H3로 조립하고, 저층 생활·시장 H3와 반품·회수 순환 H3와 함께 Town 생활·시장 H4 후보의 필수 경관으로 둔다.
- 인계: Town의 `NatureReliefOutput`은 Nature 복원·안전 회복 H2의 사건 입력 후보와 의미상 대응한다. 실제 Town·Nature AreaSet 양쪽 외부 연결점과 GraphRelation이 승인되기 전에는 자동 연결하거나 E5 이동 폐루프로 간주하지 않는다.
- 공간 경계: P3 H2 조립 좌표는 `LocalMeters` 상대 설계값이며 실제 도로·건물·Block 경계와 시장 자료 근거가 아니다. 공식 H2·H3나 실제 Town AreaSet으로 자동 승격하지 않고 Scenario로 대체하지 않는다.
- 관계: D-163의 Town 사건→Nature 위협 인과, D-165의 P3 H2 우선순위, D-166의 팩별 독립 AreaSet 조립 원칙을 Town 생활·시장 경관으로 구체화함

## D-169 H1~H4는 위치 독립 공간 설계 계층이며 공공데이터 결속은 E6에서만 수행한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `H1 작업공간 → H2 블록 → H3 경관 → H4 지역 세계 청사진`은 공간 설계의 조립 깊이만 나타낸다. H 정의에는 실제 지역 좌표, `DataRequirement`, 공공데이터 목적·출처·좌표계·원본·파생 해시나 근거 계보를 넣지 않는다.
- 설계 승격: H1은 역할·능력·용량·연결구, H2는 승인 H1·상대 배치·위상·내부 관계·외부 연결구, H3는 승인 H2와 Node·Edge·Connector 역할, H4는 세계 의도·승인 H3·내부 관계·외부 연결 역할을 검토해 승인한다. 실제 도로·건물·Block 경계나 공공데이터 부재는 H 설계 승격 차단 사유가 아니다.
- E단계 경계: 승인 H 설계를 특정 AreaSet·경관 그래프에 배치하고 사람이 설계한 도로·Gate·이동 경로를 닫는 것은 E5다. 선정 WI의 공간 판정에 필요한 공공데이터만 그 E5 인스턴스에 연결하고 출처·기준일·좌표계·해시·한계를 보존하는 것은 E6다. E6 자료가 H 정의로 역류하거나 H 설계 후보가 실제 AreaSet으로 자동 승격되지 않는다.
- 대체 관계: D-153·D-155·D-156의 위치 독립 설계 원칙을 강화한다. D-152·D-165·D-166·D-167·D-168 중 실제 도로·경계·공공데이터를 공식 H 정의의 선행 조건으로 둔 문구는 이 결정으로 대체한다. 실제 AreaSet 인스턴스의 E5 이동 폐루프와 E6 공공데이터 계보 관문은 유지한다.

## D-170 게임 기획 묶음이 H 재고 범위를 통제하고 H에서 WI의 E 부족분을 유도한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: 상향식 공간 설계는 H를 먼저 축적하되 모든 상호작용 H1을 기존 WI 또는 명시된 예상 플레이에 연결하고, H2~H4를 `Nature 생활·위협·회복`, `Farm 생산·생존`, `Town 생활·시장 안전`, `City/Hub 물류 회복력` 가운데 하나의 게임 기획 묶음에 귀속한다. Synty 팩이나 경관 문법만으로 맥락 없는 공식 H를 늘리지 않는다.
- 재고 처리: 상호작용 H1과 연결되지 않은 팩 표현 H1은 삭제하지 않고 `IdeaInventory`로 격리해 표현 연구 자료로만 유지한다. 기존 WI나 예상 플레이, 하위 H 계보 또는 게임 기획 소속이 없는 항목은 공식 승격과 E 단계 입력에서 제외한다. 자동 삭제와 자동 승격은 모두 금지한다.
- WI 관계: H가 플레이 공간과 연결 흐름을 제안하면 WI별 공간 요구와 현재 E 상태를 대조해 부족한 계약·시험·배치·공공데이터 작업을 만든다. 공간 비의존 WI에 H를 강제하지 않는다. `WI-ORDER-04 주문 포장`처럼 필수 공간이 빠진 경우에는 해당 게임 기획 안에서 H1을 먼저 보완한 뒤 E4 이상을 진행한다.
- 우선순위: H 확장은 `H-P0 맥락·필수 공간 누락 정리 → H-P1 Nature → H-P2 Farm → H-P3 Town → H-P4 City/Hub` 순서다. WI 증거는 `E-P1 Farm 수확·출하 E5 → E-P2 Hub 입고·보관 E5 → E-P3 Town 주문·수령 E4 → E-P4 Nature 사건 WI E1 → E-P5 명시적으로 선정한 WI만 E6` 순서로 진행한다.
- 관계: D-155·D-156의 상향식 재고와 문법 유도, D-162의 Nature 중심 세계 구조, D-169의 H/E·공공데이터 분리를 게임 기획 주도 재고 입고 관문과 두 개의 우선순위 대기열로 구체화한다.

## D-171 Nature 위협 대응의 예상 플레이 네 동사를 독립 E1 WI 계약으로 고정한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: Nature 상시 생활권의 `위협 관찰 → 긴급 후퇴 또는 원인 해결 뒤 복원 → 파티 회복`을 `WI-NATURE-01~04`로 등록한다. 관찰은 상태를 읽고, 후퇴는 안전 생활핵으로 이동하며, 복원은 해결된 원인 계보가 있는 경로만 다루고, 회복은 파티 준비 상태만 바꾼다.
- 금지 경계: 관찰·후퇴·파티 회복은 지역 사건 심각도나 자연권 압력을 낮추지 않는다. 복원도 원인이 해결되지 않은 사건을 우회 해결하거나 다른 경로를 자동 회복하지 않는다. Unity 이동·애니메이션·전투 연출은 어느 효과도 확정하지 않는다.
- H 연결: 다섯 Nature 상호작용 H1 후보와 위협 대응·복원 회복 H2, 자연 생활·위협·회복 H3에 실제 WI 식별자를 연결한다. 현재 WI 계약은 E1이고 H는 설계 후보이므로 Preview/Confirm·Task·저장 재생 시험 전에는 E3, 공식 H1 또는 E5로 승격하지 않는다.
- 호환: 기존 지역 사건·자연권 압력 계산, 세계 사건 API와 `simulation-save.v4`를 교체하지 않는다. 후속 E2~E3 구현은 기존 상태를 입력으로 재사용하고 새 명령의 멱등성·예상 개정·원인 계보를 별도로 검증한다.
- 관계: D-163의 지역 사건 압력, D-164~D-170의 Nature 우선 H 재고와 H 중심 E 보완 순서를 실제 WI 계약으로 연결함

## D-172 Nature H 설계는 반복 플레이 폐루프와 계획 용량을 먼저 봉인한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: Nature 상시 생활권의 기준 플레이를 `탐험 → 위협 관찰·흔적 조사 → 긴급 후퇴 또는 원인 해결 뒤 복원 → 안전 회복 → 탐험 복귀`로 고정한다. 다섯 H1 작업공간 후보, 두 H2 블록 후보와 하나의 H3 경관 후보는 이 반복 플레이와 양방향으로 추적될 때 `CandidateForReview`까지 승격할 수 있다.
- 용량 의미: H1의 `slot`, `route`, `party`, `trace`, `cargo-lot` 수량은 실제 면적·도로 폭·공공데이터 실측값이 아니라 동시 수행과 조립 충돌을 검토하기 위한 위치 독립 계획 용량이다. 공식 Simulation 용량은 WI E2~E3 규칙과 시험에서 별도로 확정한다.
- 연결 관문: 후퇴 분기는 위협 대응 H2의 `RecoveryHandoff`에서 복원·회복 H2의 `RetreatRecoveryInput`으로, 복원 분기는 `ThreatBandContinuation`에서 `IncidentRouteInput`으로 이어진다. 회복 뒤에는 안전 생활핵과 탐험 출구로 복귀해야 하며 어느 분기도 미해결 연결구를 통과하지 않는다.
- 증거 경계: 위 설계는 `DesignCandidateOnly / E1`이다. H 후보가 존재한다는 이유로 WI Preview·Confirm·Task·Effect·Save/Replay, 공식 H1, E5 지역 배치나 E6 공공데이터 결속을 성립한 것으로 보지 않는다.
- 관계: D-169의 H/E·공공데이터 분리, D-170의 게임 기획 주도 H 재고, D-171의 Nature E1 WI를 실제 반복 플레이와 공간 조립 검토 단위로 닫음

## D-173 자연권 위협 관찰은 기존 결정·작업·효과 원장을 재사용해 E3로 완결한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `WI-NATURE-01`은 전용 Preview·Confirm HTTP 계약에서 관찰 대상 자연권 경로와 현재 개정을 받고, 서버가 기존 범용 `Decision → Task → Effect` 요청으로 변환한다. Confirm은 관찰 공간 한 자리를 예약하고 한 Tick 뒤 `NatureThreatObserved` 효과와 원인 사건 계보를 기록한다.
- 비변경 경계: 관찰 효과의 압력 변화량은 0이다. 관찰은 지역 사건 상태·남은 심각도·자연권 압력·조우 상태를 변경하지 않으며 후퇴·복원 선택을 자동 확정하지 않는다.
- 공간과 증거: E3 자동 시험은 `Scenario` 근거의 관찰 공간을 사용한다. 이 성공은 H1 후보 승인, 실제 Nature AreaSet 배치, E5 이동 폐루프나 E6 공공데이터 근거를 만들지 않는다. H 계보는 계속 `CandidateForReview`다.
- 호환: 새 명령 종류를 Save schema에 추가하지 않고 기존 결정 Confirm 명령과 공간 예약을 `simulation-save.v4`로 재생한다. 동일 명령 재전송, 낮은 개정, 잘못된 경로, 공간 부재와 HTTP 경로 Manifest를 자동 시험으로 고정한다.
- 관계: D-171의 네 Nature WI 중 첫 동사를 D-172의 H 반복 플레이 설계 위에서 E3까지 구현함

## D-174 자연권 긴급 후퇴는 선행 위협 근거와 경로 예약으로 E3를 닫는다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `WI-NATURE-02`는 같은 자연권 경로에 적용된 `NatureThreatObserved` 효과 또는 활성 조우가 있을 때만 Preview를 통과한다. Confirm은 `EmergencyAccess + PlayerEscapeRoute + SafeCore`를 가진 후퇴 공간의 파티 용량을 예약하고, 한 Tick 뒤 `PartyRetreatedToSafeCore` 효과를 적용하며 예약을 반환한다.
- 비해결 경계: 후퇴는 지역 사건 상태, 남은 심각도, 자연권 압력과 조우 승패를 변경하지 않는다. 다음 `WI-NATURE-04` 회복을 열 수 있는 파티 상태 인계만 기록한다.
- 공간과 증거: E3 시험은 `Scenario` 근거의 후퇴 경로 한 파티 용량을 사용한다. 이 결과는 Nature H1 승인, AreaSet 배치, E5 이동 폐루프나 E6 공공데이터 근거가 아니다.
- 호환: 새 Save schema를 만들지 않고 기존 결정 Confirm·공간 예약·작업 취소·Save/Replay를 재사용한다. 전용 HTTP Preview·Confirm 경로와 멱등성·payload 충돌·낮은 개정·공간 부재를 자동 시험으로 고정한다.
- 관계: D-171의 두 번째 Nature WI를 D-173 관찰 효과의 후속 행위로 E3까지 구현함

## D-175 자연권 복원은 관찰된 원인 전체 해결 후에만 자재를 소비한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `WI-NATURE-03`은 같은 경로의 `NatureThreatObserved` 효과가 가리킨 원인 사건이 하나 이상 있고, 그 전체가 `Resolved / RemainingSeverity=0`이며 현재 경로의 남은 심각도도 0일 때만 Preview를 통과한다.
- 예약과 소비: Confirm은 `WorkerAccessible + CargoAccessible + RestorationWorkArea`를 가진 공간의 작업 자리와 복원 자재 한 롯을 원자적으로 예약한다. 취소 시 둘 다 반환하고, 완료 시 작업 자리는 반환하며 자재는 소비한다.
- 비해결 경계: `NatureRouteRestored`는 원인이 이미 해결된 경로에서 복구 행위가 완료됐음을 기록한다. 이 효과가 지역 사건 심각도, 자연권 압력, 다른 경로를 추가로 감소시키지 않는다.
- 공간과 증거: E3의 복원 공간과 자재 용량은 `Scenario`다. 이 성공은 Nature H1 승인, 실제 AreaSet 배치, E5 이동 폐루프나 E6 공공데이터 계보를 만들지 않는다.
- 호환: 기존 결정 Confirm·공간 예약·Save/Replay를 재사용하고, 공간 용량에 복원 자재라는 소모성 종류만 추가한다. 전용 Preview·Confirm HTTP와 미관찰·원인 미해결·공간 부재·취소·Save/Replay를 자동 시험으로 고정한다.
- 관계: D-171의 세 번째 Nature WI를 D-173 관찰 계보와 D-163 지역 사건 해결 규칙 위에서 E3까지 구현함

## D-176 파티 회복은 후퇴 또는 복원 효과 후 탐색을 재개하는 E3 행위다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 결정: `WI-NATURE-04`는 같은 자연권 경로의 `PartyRetreatedToSafeCore` 또는 `NatureRouteRestored` 적용 효과가 있을 때만 Preview를 통과한다. Confirm은 안전 생활핵의 파티 휴식 용량과 회복 보급 한 롯을 예약한다.
- 예약과 효과: 취소 시 휴식 용량·보급을 모두 반환하고, 완료 시 휴식 용량은 반환하며 보급은 소비한다. `PartyRecovered`는 파티 준비 상태와 다음 `Explore` 행위만 연다.
- 비해결 경계: 회복은 지역 사건, 남은 심각도, 자연권 압력, 조우 상태를 변경하지 않는다. Unity의 휴식 애니메이션이 회복 완료를 확정하지 않는다.
- 공간과 증거: E3의 안전 회복 야영지·휴식 용량·회복 보급은 `Scenario` 근거다. 이 성공은 Nature H1 승인, AreaSet 배치, E5 공간 폐루프나 E6 공공데이터 계보가 아니다.
- 완결 범위: 이 결정으로 현재 원장의 41개 WI 전체가 E3에 도달한다. 다음 단계는 자동 H 승격이 아니라 Nature 반복 플레이의 H1·H2·H3 사람 검토와 E5 후보 설계다.
- 관계: D-171의 네 번째 Nature WI를 D-174 후퇴·D-175 복원 효과의 공통 후속 행위로 E3까지 구현함

## D-177 Nature 팩 중심 상시 체류 세계를 심리 영역으로 정의하고 두 발전소 인과를 둔다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 용어: 플레이어가 상시 체류하는 Nature 팩 중심 세계의 게임 의미는 `심리 영역`으로, Farm·Town·City/Hub의 독립 AreaSet 묶음은 Simulation 문맥의 `업무 영역`으로 부른다. 심리 영역은 외부 세계 전체가 마음에서 비롯된다는 형이상학적 설명이 아니라 업무 결과가 플레이어의 회복력과 위협 상태에 미친 심리적 영향을 명시적으로 공간화한 세계다.
- 발전소: 심리 영역에는 `회복 발전소`와 `위협 발전소`를 각각 하나씩 둔다. 좋은 업무 결과는 회복 발전소를 강화하고 위협 발전소를 약화하며, 잘못된 결과와 기한 초과는 회복 발전소를 약화하고 위협 발전소를 강화한다. 두 상태는 같은 결과 계보에서 함께 변하지만 한 수치로 합치지 않는다.
- 인과 경계: 확정된 업무 결과는 발생 업무 영역의 직접 영향과 심리 영역의 발전소 변화를 함께 남긴다. 미리보기 차단과 일반 취소는 입력이 아니다. 심리 영역의 행동은 확산과 피해를 완충할 수 있지만 업무 직접 영향을 해결하거나 원인 사건을 종료하지 않으며, 원인 해결은 해당 업무 영역의 후속 WI가 담당한다.
- 되먹임: 두 발전소의 상태는 여러 업무 영역으로 다시 영향을 보낼 수 있으나 원인 경로와 업무 종류별 대상을 보존한다. 모든 영역에 같은 전역 수치를 일괄 적용하지 않으며 수치·임계값·업무별 효과표는 후속 Simulation 수직 조각에서 확정한다.
- 호환과 이행: `NatureHome`, `ProfessionalWorld`, Nature·Farm·Town·City/Hub 팩 코드, AreaSet·WI·API·저장 식별자는 바꾸지 않는다. D-162의 공간·시점·자산 관문은 유지하면서 사람이 읽는 세계 의미를 이 결정으로 구체화한다. D-163의 경로별 위협 계보는 유지하되 자연권 전투가 업무 사건 심각도를 직접 줄이는 현재 동작은 심리 완충과 업무 원인 해결을 분리하는 후속 구현에서 대체한다.
- 증거: 이번 결정은 용어와 인과 기획만 확정한다. 회복 발전소 상태, 두 발전소의 동시 변화, 업무 영역 되먹임, 저장 계약과 Unity 배치는 아직 구현·시험하지 않았다.
- 기준 문서: [업무 사건과 심리 영역 발전소 영향 규칙](../Architecture/지역사건-자연권위협-규칙.md)

## D-178 Construction 팩은 공통 조립층이며 두 발전소는 기존 Nature H2·H3를 확장한다

- 상태: `Accepted`
- 결정일: 2026-08-18
- 팩 역할: POLYGON Construction은 독립 AreaSet이나 다섯 번째 업무 영역이 아니다. Nature·Farm·Town·City/Hub의 구조물·공사·복구·격리·전환 상태를 만드는 공통 조립 재료층이다. 한 H1은 주도 팩 하나, Construction 기능층과 보조 팩 1~2개를 기본으로 하며 모든 공간에 다섯 팩을 같은 비율로 넣지 않는다.
- 발전소 H: `회복 발전 동력핵`과 `위협 발전 집속핵`을 예상 플레이가 있는 H1 문서 후보로 둔다. 회복 발전소는 기존 `nature-restoration-recovery`, 위협 발전소는 기존 `nature-threat-response` H2를 확장하고 둘은 기존 `nature-threat-recovery` H3 안에서 완충 지형과 복귀 동선으로 연결한다. 기존 `NatureHome`, H2·H3 Stable ID, AreaSet·WI·API·저장 식별자는 바꾸지 않는다.
- 변형과 권위: A/B/C는 같은 면적·연결구·핵심 소켓을 유지하는 공간 배치 변형이며 발전소의 시간·강도 상태가 아니다. Prefab 이름·경로는 교체 가능한 표현 참조이고 서버가 확정하는 업무 결과·발전소 상태나 Simulation 권위를 만들지 않는다.
- 증거: 이번 범위는 다섯 팩 2,346개와 Construction 584개 Prefab 설치 여부를 확인한 `DesignPlanOnly` 문서다. 새 H1의 기계 대장 등록, Unity 조립 Prefab·Scene, Play Mode·Game View와 Simulation 수치·저장 계약은 구현하지 않았다.
- 기준 문서: [심리·업무 영역 Synty 5팩 공간 조립 계획](../Architecture/심리업무영역Synty공간조립계획.md)
- 관계: D-155~D-156의 Synty 상향식 재고와 H 계층, D-165의 H2 조립 우선순위, D-170의 게임 기획 관문, D-177의 심리 영역·두 발전소 인과를 보유 자산 조립 규칙으로 구체화한다.

## D-179 다섯 Synty 팩은 H 승격 전에 전수 기술 대장과 의미 자산군으로 관리한다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 전수 범위: Nature 227·Farm 498·Town 702·City 335·Construction 584개, 합계 2,346개 Prefab을 기술 대장에 등록한다. Character·Vehicle·Item·Tool·FX도 누락하지 않되 정적 경관 자동 배치 대상과는 분리한다.
- 호환과 분류: 기존 Farm·Town·City 1,535개 `inventoryId` 산식은 유지한다. 팩마다 다른 원본 `Environment(s)` 경로는 새 정규화 분류로 흡수하고, 각 항목에 의미 자산군, 주 활용 트랙, 검토 상태와 계획 적용 영역을 둔다. Construction은 네 영역의 공통 상태층 후보이며 독립 H1로 자동 승격하지 않는다.
- 승격 경계: 전수 등록과 자동 분류는 보유 표현 재료를 찾은 증거다. 사람 검토 대기 항목과 대표 조립물은 별도로 검증하며 기술 대장만으로 H 승인, E4~E7, AreaSet 배치나 Simulation 상태가 성립하지 않는다.
- 구현 증거: `synty-pack-inventory.v2` 대장에 2,346개·1,499개 의미 자산군을 생성했고 Vehicle 51개를 포함한 자동 분류 2,345개와 Nature `Misc` 사람 검토 대기 1개를 분리했다. Editor·EditMode 시험 어셈블리는 오류 0개로 컴파일했고 집중 Unity EditMode 시험 4/4가 통과했다. 조립 Prefab·Scene, Play Mode·Game View는 완료 증거에 포함하지 않는다.
- 기준 문서: [심리·업무 영역 Synty 5팩 공간 조립 계획](../Architecture/심리업무영역Synty공간조립계획.md), [Synty 상향식 공간 재고 계획](../Architecture/Synty상향식공간재고계획.md)
- 관계: D-178의 Construction 공통 조립층과 발전소 설계를 바꾸지 않고, 그 설계 전에 수행할 표현 재료 전수 관리 규칙을 구현 수준으로 고정한다.

## D-180 휴대폰 공간 조립 검토는 주차 후 후보 선별이며 최종 Scene 승인이 아니다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 안전 경계: 모바일 검토는 합법적인 장소에 완전히 주차하고 운전을 종료한 뒤에만 사용한다. 신호 대기·정체 중 사용하지 않으며 화면은 `안전하게 주차했습니다` 확인 전 판단 입력을 잠근다.
- 권위와 상태: Unity 촬영 batch는 조합·계획·Rendering Profile·이미지 hash를 서버 검토 원장에 등록한다. 휴대폰은 Stable ID, 예상 개정, 멱등 키, 판단 코드와 선택적 문제·메모만 보내며 `Good`은 `ReviewedCandidate`다. 어떤 모바일 판단도 `ApprovedForSceneApply`, Simulation 상태나 운영 업무 결과를 만들지 않는다.
- 변경과 오프라인: 같은 조합물의 입력 또는 촬영 snapshot이 바뀌면 기존 판단을 `Stale`로 만들어 재검토한다. 통신 실패 요청은 휴대폰에 임시 보관하지만 `409` 개정 충돌을 자동 덮어쓰지 않는다.
- 첫 범위: 회복·위협 발전소 × A/B/C × 기본·강화 상태의 12개 카드를 첫 batch로 사용한다. 서버 Mongo 원장·관리자 API·모바일 Web 화면·오프라인 요청 대기열과 촬영 전 batch 생성기를 구현하며, Unity 실제 촬영·업로드와 PC 최종 승인은 후속 검증으로 남긴다.
- 기준 문서: [Synty 모바일 조합물 검토 계획](../Architecture/Synty모바일조합물검토계획.md)
- 관계: D-178~D-179의 다섯 팩 조립·전수 기술 대장을 사람이 짧게 비교 검토하는 인계 절차로 확장하며 기존 정적 경관 배치 승인 영수증의 hash 권위를 유지한다.

## D-181 Synty Web 검토 v2는 불변 촬영 영수증과 부모 bundle 계보로 재촬영을 닫는다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 계약과 경합: 새 Unity 촬영은 `synty-composition-review-batch.v2`를 사용하고 `ExpectedRevision`, `CompositionInputHash`, `RenderingProfileHash`, `ParentCaptureBundleHash`, `CaptureBundleHash`를 함께 보낸다. `NeedsRevision` 재촬영은 현재 원장의 부모 bundle·조립 입력·예상 개정이 모두 맞을 때만 `ReadyForReview`로 돌아가며, 원본 변경은 `Stale`, 늦은 재촬영은 `409`로 처리한다. URL 기반 v1은 읽기 호환으로 유지한다.
- 이미지 권위: Unity는 Blob URL을 결정하지 않고 PNG를 서버에 보낸다. 서버는 signature·크기·원본 hash를 검증하고 PNG를 재인코딩한 뒤 `UploadedSourceSha256`과 `StoredImageSha256`을 분리한다. `CaptureUploadId` 영수증이 `StorageProviderCode + ContainerName + ObjectName + hash`를 보존하며 Review Batch v2는 이 영수증만 참조한다.
- 저장 실패: Blob은 불변 object의 `create-if-absent`, Mongo는 촬영 영수증 원장으로 나눈다. Blob 성공 뒤 Mongo 실패는 orphan을 허용하고 같은 영수증 재시도로 원장 저장을 회복한다. 기존 object를 덮어쓰거나 실패 시 URL을 권위값으로 대체하지 않는다.
- 공개와 개인정보: 현재 검토 이미지는 공개 읽기 object다. URL 비밀성은 접근 제어가 아니므로 Capture Camera는 전용 layer만 렌더링하고 사용자·주소·주문·서버·인증·Console·token·로컬 경로를 PNG에 넣지 않는다. Web은 관리자 화면과 공개 이미지 접근 경계가 다름을 명시한다.
- 이행 순서: 12카드·48이미지를 먼저 만들지 않고 회복 발전소 A Normal 1카드에서 Capture Stage → 업로드 영수증 → Web 판단 → `NeedsRevision` 재촬영 폐루프를 먼저 닫는다. 현재 코드와 로컬 네 시점 PNG까지 구현했으며 Azure Blob·Mongo·관리자 HTTP·실제 휴대폰 왕복은 별도 운영 검증이다.
- 기준 문서: [Synty Web 조합물 검토 폐루프 계획](../Architecture/Synty모바일조합물검토계획.md)
- 관계: D-180의 주차 후 후보 선별과 `Good ≠ 승인`을 유지하면서 촬영 저장·재촬영 경합·공개 이미지 개인정보 경계를 구현 계약으로 구체화한다.

## D-182 Unity 산출물 검토 WebApp은 일반 업무 WebApp과 물리 프로젝트를 분리한다

- 상태: Accepted
- 결정: Unity 촬영 이미지·검토 이력·후보 판단은 `Ssalddel.Web.UnityReviewApp`이 소유한다. 일반 `Ssalddel.WebApp`과 01~05 역할별 WebApp은 Unity 검토 route, page, Client와 오프라인 대기열을 포함하지 않는다.
- 인증: 전용 앱은 기존 서버 로그인 API와 계약을 사용하되 별도 `ssalddel.unity-review.auth.v1` 저장 키를 쓴다. 검토 API에는 Bearer token을 명시적으로 전송하고 `서버관리자` 역할이 아니면 전용 앱 토큰을 보존하지 않는다.
- 배포: 전용 앱은 `Ssalddel.UnityReview.slnx`로 독립 빌드한다. 일반 제품 릴리스와 역할 WebApp에 자동 포함하지 않으며, 배포·hostname·공개 Blob 정책·민감 화면 검증을 별도 게이트로 관리한다.
- 공유 경계: 서버 API·`Ssalddel.Contracts`·공통 인증 토큰 구조만 재사용한다. 일반 WebApp 프로젝트·레이아웃·서비스에 대한 참조는 금지한다.
- 관계: D-181의 공개 이미지·관리자 검토 경계를 배포 가능한 클라이언트 경계로 구체화하며 `Good ≠ 승인`, 서버 원장 권위와 Unity 표현 전용 원칙은 유지한다.

## D-183 Synty 검토 폐루프는 저장·화면 상태·전송·촬영 조립 책임을 분리한다

- 상태: Accepted
- 결정일: 2026-08-19
- 서버: 검토·촬영 UseCase는 상태 전이와 PNG 검증을 소유하고 Mongo·메모리 원장 구현, 영수증 record와 저장 구현은 별도 `Stores` 파일이 소유한다. Blob 위치와 Mongo 영수증의 권위, 기존 API route·JSON 계약·Stable ID는 바꾸지 않는다.
- 전용 WebApp: Razor page는 표시만, code-behind는 인증 생명주기만, `Workspace`는 화면 상태와 판단 전이만 맡는다. HTTP Client는 Bearer API 통신만 맡고 브라우저 `localStorage` 대기열은 전용 오프라인 Store가 맡는다. 일반 `Ssalddel.WebApp`으로 책임을 되돌리지 않는다.
- Unity: 공개 메뉴와 `Synty공간조립Web검토CapturePipeline` 진입점은 유지하고 orchestration, 전용 Capture Stage·카메라, 서버 API client, 전송 model을 파일 단위로 나눈다. 원본 Synty Prefab·저장 Scene·촬영 Stable ID와 v2 계약은 유지한다.
- 검증 경계: 구조 분리는 서버·Web 집중 시험, Unity Editor 어셈블리 빌드와 그래픽 장치를 유지한 EditMode 촬영 시험으로 확인한다. 이는 Azure·Mongo·관리자 HTTP·휴대폰 실기기 왕복 증거를 대신하지 않는다.
- 관계: D-181의 서버 권위·촬영 계보와 D-182의 물리 Web 프로젝트 분리를 내부 책임 경계까지 구체화한다.

## D-184 Unity 산출물 검토 앱은 기존 Azure VM에서 별도 경로·배포 묶음으로 운영한다

- 상태: Accepted
- 결정일: 2026-08-19
- 배포 단위: `Ssalddel.Web.UnityReviewApp`은 일반 01~05 WebApp 게시 묶음과 분리한 `unity-review.tar.gz`로 게시하고 `/opt/ssalddel/web/unity-review`만 원자 교체한다. 일반 역할 앱 파일을 Unity 검토 배포에 다시 게시하거나 함께 교체하지 않는다.
- 공개 경로와 API: 추가 유료 host를 만들지 않고 기존 Azure 미리보기 VM의 `/unity-review/`를 사용한다. Production 앱은 같은 host 루트의 관리자 API를 호출하며 Caddy는 `/unity-review/*`만 전용 정적 루트로 보낸다. 물리 프로젝트·인증 저장 키·배포 묶음의 분리는 유지한다.
- 서버와 데이터: 검토 API가 바뀐 경우 서버 이미지는 별도로 갱신하되 기존 MySQL·MongoDB·Data Protection·업로드 볼륨과 VM 비밀값을 보존한다. 촬영 이미지는 기존 Azure Blob 공개 container의 불변 `world-composition-reviews/` prefix를 사용하고 Mongo 영수증 권위를 유지한다.
- 운영 경계: 화면은 H1·H2·H3 고유 식별자와 촬영 묶음을 보여 주지만 `Good`은 후보 판단일 뿐 H 승인·Scene 적용·E5·Simulation 완료가 아니다. Azure VM의 현재 비용 통제 운영창은 한국 시간 19:00~23:00이며 시간 밖 수동 기동은 별도 명시 승인을 요구한다.
- 관계: D-182의 물리 WebApp 분리를 저비용 Azure 미리보기의 실제 경로와 원격 교체 단위까지 구체화한다.

## D-185 H1~H4 Unity 조합물은 선택 Root 촬영 영수증으로 모바일 검토하되 공간 권위를 만들지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 결정: Unity 편집기에서 H1~H4 조합물의 최상위 Root와 H 계보를 명시하고, 저장 장면을 변경하지 않는 임시 장면에서 단계별 표준 시점 PNG를 만든 뒤 서버 업로드 영수증과 `synty-composition-review-batch.v3` 검토 묶음을 등록한다. 사용자는 전용 Web 검토함을 휴대폰으로 열어 후보 판단을 남긴다.
- 촬영 Profile: H1 장소는 4시점, H2 블록은 5시점, H3 경관은 6시점, H4 지역 청사진은 4시점이다. 서버는 주 검토 대상과 H 계보, Profile, 시점 수, 조립 입력·Rendering Profile·부모 묶음·예상 개정 및 개별 업로드 영수증을 함께 검증한다.
- 권위 경계: 촬영 이미지, 자동 계산한 팩 사용 비율과 모바일 `Good`은 표현 검토 증거와 `ReviewedCandidate`만 만든다. 공식 H 승인, WI의 E단계, AreaSet·경관 그래프·공공데이터 근거, Scene 적용이나 Simulation 상태를 만들지 않는다.
- 보안 경계: Unity는 관리자 토큰을 파일·manifest·로그에 저장하지 않고 환경 변수에서 읽는다. 서버가 PNG를 검증·재인코딩하고 불변 저장 위치 영수증을 발급하며 Unity는 Blob 위치를 결정하지 않는다.
- 관계: D-180의 주차 후 모바일 후보 선별, D-181의 불변 업로드·재촬영 계보, D-183의 촬영·전송 책임 분리를 H1~H4 일반 조합물로 확장함

## D-186 Unity 산출물 검토는 역할별 VM과 분리한 무료 대상 VM의 최소 Docker 스택으로 운영한다

- 상태: Accepted
- 결정일: 2026-08-19
- 대체 관계: D-184의 기존 Azure VM `/unity-review/` 공동 운영 결정을 대체한다. 기존 배포 묶음은 비상용 자료로만 남기며 새 검토 앱은 역할별 WebApp VM의 기동 시간·Caddy·MySQL·MongoDB·비밀값을 공유하지 않는다.
- 실행 경계: 별도 `Standard_B2ats_v2` 또는 같은 무료 대상 SKU에 Caddy, `Ssalddel.UnityReview.Api`, MySQL 8.4만 Docker Compose로 실행한다. MongoDB와 통합 `Ssalddel` 서버는 넣지 않으며 Compose 메모리 상한 64MB·320MB·384MB와 host 2GB swap을 적용한다.
- 코드 경계: `Ssalddel.UnityReview.Core`가 검토 상태 전이와 PNG 검증 소스만 재사용하고 전용 API는 통합 서버 프로젝트를 참조하지 않는다. 전용 solution·hostname·관리자 JWT·MySQL 원장·이미지 volume·배포 archive와 SSH key를 역할별 WebApp에서 분리한다.
- 저장 경계: 미리보기 이미지는 hash 기반 불변 Docker volume, 촬영 영수증과 검토 snapshot은 MySQL에 저장한다. 공개 URL은 Caddy 투영값일 뿐 권위가 아니고 `ContainerName + ObjectName + StoredImageSha256`이 위치·무결성 기준이다. 단일 VM volume은 운영 백업이나 H·E 승인 증거가 아니다.
- 비용 경계: 무료 대상 VM 혜택의 기간·월별 시간과 디스크·공인 IP·전송 부대 비용은 별도로 확인한다. 구독이 쓰기 가능한 상태가 아니면 provisioning을 중단하며 무료 크레딧 반복 생성을 전제로 하지 않는다.
- 관계: D-180의 주차 후 후보 선별, D-181의 불변 영수증·부모 bundle, D-182의 물리 WebApp 분리와 D-185의 H1~H4 촬영 Profile을 유지한다.

## D-187 H1은 인지 부품이고 H2는 첫 공간 조합 판단 단위다

- 상태: `Accepted`
- 결정일: 2026-08-19
- H1 역할: H1 설계 재고는 재사용 가능한 공간 부품이 존재하고 그 의미·표현 후보·게임 기획 맥락을 식별하는 단계다. H2 조합 입력이 되기 위해 모든 H1이 먼저 공식 Simulation 공간 능력·업무 용량 승인을 받을 필요는 없다. WI가 실제로 사용하는 H1의 강한 계약은 해당 WI의 E단계에서 별도로 검증한다.
- H2 역할: 사용자가 공간 구성을 처음 판단하는 기본 단위는 H2 블록 모판이다. 필수 H1 두 개 이상, 정확한 하위 revision·hash, 결정적인 상대 배치, 위상, 내부 도달 관계, 외부 연결구, 크기·금지 조합, 촬영 가능한 Unity Root와 표준 5시점 촬영 기록을 갖춘 뒤 H2 조합물 자체를 사람이 검토한다.
- 상태 분리: `H1 인지 부품 → H2 조합 가능 → H2 검토 가능 → H2 승인`을 구분한다. H2 조합 가능은 공식 승인이나 자동 승격이 아니며 Unity 촬영과 모바일 `Good`도 `ReviewedCandidate`만 만든다. 실제 AreaSet 배치·경관 이동 폐루프는 E5, 공공데이터 계보는 E6에서 별도로 검증한다.
- 재고 운영: H1의 `knowledgeStateCode` 자체를 H2 조합 차단기로 사용하지 않는다. 정의가 존재하고 WI 또는 예상 게임 플레이, 공간 역할, Synty 팩 또는 기준 문법 표현 근거가 각각 하나 이상 식별되면 `IdeaInventory`도 인지 부품으로 사용할 수 있다. 이 맥락 신호가 없는 고아 아이디어만 계속 격리한다. H2 후보 24개는 이 인지 조건을 기준으로 조합 가능성을 먼저 감사하고, 세부 조립법과 Unity Root가 준비된 항목부터 우선 촬영한다.
- 관계: D-170의 게임 기획 주도 H 재고, D-172의 계획 용량, D-180의 모바일 후보 선별과 D-185의 H1~H4 촬영 Profile을 유지하면서 H1 과승인 병목을 제거하고 H2를 실제 공간 판단 표면으로 재정의한다.

## D-188 H 조립·게임플레이 추적·E 증거·완주 상태는 독립 축으로 관리한다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 네 축: H1 인지 부품·H2 블록·H3 `LandscapeGraph`급 경관·H4 `AreaSet`급 지역 설계는 공간 조립 깊이다. `GameplayTraceState`는 `Unlinked`, 지원 경관, 직접 행위, H2 순서, H3 폐루프, H4 지역 인과를 별도로 기록한다. E0~E7은 증거 깊이이고 `PlayableSliceState`는 사람이 실제로 완주할 수 있는 마감 상태다. 어느 한 축도 다른 축을 자동 승격하지 않는다.
- 첫 엄격 관문: `NatureHomeThreatRecovery`와 `FarmProductionSurvival`은 `reference-play:nature-farm-day.v1`에서 H 추적 누락을 차단한다. Town 생활·시장 안전과 City/Hub 물류 회복력의 기준 플레이 누락은 경고만 남기며 기존 H 후보·사람 검토를 폐기하거나 막지 않는다.
- 카드 경계: 첫 기준 플레이에는 `Normal / Opportunity / Threat / Recovery` 조건 슬롯과 영향을 받는 단계·H1 표현 연결점만 둔다. 구체 카드 ID·수치·비용·확률·플러스·마이너스 효과는 서버 턴 규칙이 확정하며 H 대장과 Unity가 계산하지 않는다.
- 완성 판정: Nature↔Farm 기준 플레이는 설계·H 추적이 있어도 실제 E5 `LandscapeGraph`·`AreaSet` 결속, 정상·실패·회복 Runtime 완주, 처음 접한 플레이어의 20~40분 관찰, canonical `SimulationWorldShell` 저장 배선·Play Mode·Game View와 시각·음향·성능 마감 증거가 모두 있기 전에는 `Planned`를 유지한다. H 승인, 촬영 `Good`, 개별 E3·E4 성공만으로 `PlayableSliceComplete`를 선언하지 않는다.
- 관계: D-170의 게임 기획 주도 H 범위, D-172의 Nature 반복 폐루프, D-187의 H1/H2 공간 판단 분리를 보존하면서 실제 게임 완성을 반복해서 판정할 수 있는 별도 관문을 추가한다.

## D-189 H2·H3와 이론 E5 공간 생산은 사람 검토를 차단 관문으로 사용하지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 결정: 게임 플레이 맥락, 필수 하위 H 인지, 결정적 상대 배치, 최소 간격, 연결 그래프, 입구·출구, 세계 의도와 GraphRelation 폐루프를 자동 검사해 `TheoryQualified` H2·H3와 `E5TheoryQualified` Theory AreaSet 인스턴스를 반복 생산한다. 사람의 이미지 검토는 `DeferredBatchReview`로 남기며 생산을 멈추지 않는다.
- E5 경계: 이론 공장은 H4 후보를 그대로 AreaSet으로 가장하지 않고 별도 `area-set:theory:*` 고유 식별자, H3 Graph 인스턴스, 관계와 결정성 hash를 만든다. 이 산출물은 E5 공간 조립 계보를 가지지만 `EvidenceKind=TheoryGenerated`, `humanReviewed=false`, `publicDataBound=false`, `runtimeValidated=false`를 반드시 함께 기록한다.
- 사람 검토: 촬영·휴대폰 판단은 나중 개정의 미감·가독성 개선 입력이다. 검토가 없다는 사실을 숨기거나 자동 사람 승인으로 기록하지 않는다. D-180·D-185의 모바일 검토 경로는 선택적 품질 개선 수단으로 유지한다.
- 후속 관문: H2·H3·이론 E5 생산은 E6 공공데이터 계보와 E7 실제 서버·저장 Scene 플레이를 완료했다고 주장하지 않는다. 실제 지역 근거가 필요한 WI만 E6에서 연결하고 실제 플레이 완주는 E7에서 별도로 검증한다.
- 관계: D-187의 “H2가 첫 사람 판단 단위” 가운데 사람 판단의 생산 차단 역할을 대체한다. H1 인지 부품, H/E 독립 축과 자동 사람 승인 금지는 유지한다.

## D-190 이론 공간 공급과 실제 플레이 공간 완성 사이에 독립 완료 상태를 둔다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 상태 사슬: `PlayableSliceState`는 `Planned → TheorySpatiallyComposed → SpatiallyComposed → FunctionallyClosed → ExperienceValidated → PlayableSliceComplete` 순서를 사용한다. `TheorySpatiallyComposed`는 추적된 H2·H3와 H4 세계 의도가 결정적인 전용 `area-set:theory:*`에 결속되어 다음 실제 배치 작업의 입력이 준비됐음을 뜻한다.
- 이론 관문: 이 상태에는 `TheoryQualified` H2·H3, `E5TheoryQualified`, 별도 고유 식별자와 결정성 hash가 필요하다. 사람 검토, 게임플레이 추적, 공공데이터와 Runtime은 이론 공간 생산을 차단하지 않는다. 엄격 게임플레이 추적은 Nature·Farm 우선 작업 선정을 막을 수 있지만 이미 적격인 이론 H를 폐기하거나 강등하지 않는다.
- 실제 관문: `SpatiallyComposed`는 승인된 H가 실제 지역 근거의 `LandscapeGraph`·`AreaSet`과 양쪽 연결구에 결속된 실제 E5 이동 폐루프가 있을 때만 부여한다. 이론 E5는 실제 E5, E6 공공데이터, E7 서버·저장 Scene Runtime, Play Mode·Game View를 대신하지 않는다.
- 현재 판정: `reference-play:nature-farm-day.v1`은 Nature·Farm 이론 AreaSet 두 개까지 닫혀 `TheorySpatiallyComposed`다. `ActualE5BindingMissing`, Runtime 분기 완주, 처음 접한 플레이어 관찰, canonical Scene 실행 증거, 시각·음향·성능 마감이 남으므로 실제 게임 완성은 아니다.
- 관계: D-188의 네 독립 축과 최종 완료 기준을 유지하되, 이론 공간 공장을 도입한 D-189에 맞춰 D-188의 `Planned` 고정 판정을 이 중간 상태 모델로 대체한다.

## D-191 H2·H3는 StableId를 보존하고 팩 주도 패턴 이름을 별도로 가진다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 이름 계약: 기존 `h2-candidate:*`, `h3-candidate:*` StableId는 저장·계보·생성기 참조용으로 보존한다. 사람이 AreaSet 재고를 구분하는 이름은 `{주도 팩}-H{단계}-{패턴 계열}-{일련번호}` 형식의 별도 `patternCode`와 한국어 `displayNameKo`로 관리한다.
- 팩 경계: 단일 팩 `SinglePack`, 주도 팩과 보조 팩의 `LeadPackWithSupport`, 팩 사이 전환인 `CrossPackTransition`을 구분한다. 혼합 회랑은 어느 한 팩의 단독 자원으로 가장하지 않고 `MIX` 계열을 사용하며 Construction은 독립 AreaSet이 아닌 지원 기능층으로만 기록한다.
- 변형 경계: 패턴 일련번호는 서로 다른 H2·H3 공간 조립을 구분한다. 기준 경관 문법의 A/B/C 표현 변형과 같은 값이 아니며 패턴 코드가 Prefab·GUID·Scene·Simulation 권위를 만들지 않는다.
- 생산 연결: 이론 공간 공장은 패턴 이름 대장 전체를 검증하고 H3 Node에 하위 H2 패턴, 이론 AreaSet Graph 인스턴스에 H3 패턴을 투영한다. 대장 누락·중복·형식 오류·StableId 불일치가 있으면 생산을 중단한다.
- 자원 종류: H2는 여러 H1의 상대 배치·내부 동선·입구·출구를 봉인한 `BlockPattern`이고, H3는 여러 H2와 외부 연결 역할을 묶은 `LandscapeAssemblyPattern`이다. H2·H3를 단순 번호 목록이 아니라 팩·계열·공간 역할이 드러나는 패턴 재고로 관리한다.
- 확장 순서: 첫째 Nature·Farm·City·Town 각 팩만으로 구성 가능한 `SinglePack` H2 블록을 고르게 확보한다. 둘째 같은 팩의 H2를 그 팩 내부 H3로 묶는다. 셋째 주도 팩에 Construction이나 다른 팩을 보조층으로 붙이는 `LeadPackWithSupport`를 확장한다. 넷째 팩 경계를 직접 조립하는 혼합 H2와 혼합 H3를 차례로 만든다. 기존 혼합 패턴은 보존하지만 새 단독 팩 재고보다 먼저 증식시키지 않는다. 예약 이름은 실제 H 정의나 E5 증거가 아니다.
- 관계: D-170의 게임 기획 주도 H 재고, D-189의 비차단 이론 생산과 D-190의 이론 공간 완료 상태를 유지하면서 AreaSet에 채울 H2·H3 재고의 식별성과 확장 순서를 고정한다.

## D-192 팩 단독 H2는 팩 내부 H3보다 먼저 게임 기획 AreaSet에 대기 결속한다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 결정: 팩 단독 H2는 H3가 아직 정의되지 않았다는 이유로 고아 재고로 격리하지 않는다. Nature·Farm·City/Hub·Town 게임 기획 AreaSet 후보가 `stagedPackNativeH2Refs`로 직접 소유하고, 다음 팩 내부 H3 생산의 입력으로 사용한다.
- 현재 재고: Nature 생활핵·조우·방어 3개, Farm 관수·집중수확 2개, City/Hub 정비·비상전력 2개, Town 주거골목·주민서비스 2개를 추가해 H2를 24개에서 33개로 확장했다. 아홉 블록은 모두 단일 주도 팩 `SinglePack`, 위치 독립 상대 조립, 최소 두 H1, Network 문법, 입구·출구를 가지며 `TheoryQualified`다.
- E 경계: H2 생성과 이론 적격은 WI 플레이 순서 추적을 기다리지 않는다. Nature·Farm의 엄격 추적 누락과 City·Town의 경고형 추적 누락은 후속 E 보완 대상으로 표시하며 H3·AreaSet·실제 플레이 완성을 자동 주장하지 않는다.
- 관계: D-189의 사람 검토 비차단 이론 생산, D-191의 팩 주도 패턴 이름과 생산 순서를 첫 팩 단독 H2 묶음으로 실행한다.

## D-193 팩 내부 H3가 준비되면 AreaSet의 임시 H2 직접 참조를 H3 계보로 대체한다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 결정: 같은 팩의 H2를 조합한 H3가 `TheoryQualified`가 되면 게임 기획 AreaSet 후보의 `stagedPackNativeH2Refs`를 제거하고 해당 H3를 `requiredH3Refs`에 결속한다. H2는 H3 정의의 하위 계보로 계속 추적되며 고유 식별자와 해시는 유지한다.
- 현재 재고: Nature 생활핵·조우·방어 폐루프, Farm 계절 생산 폐루프, City/Hub 정비·비상 대응 폐루프, Town 주민 서비스 폐루프를 추가해 H3를 13개에서 17개로 확장했다. 팩 주도 H2·H3 패턴은 50개이며 다음 생산 대기열은 Nature–Town 혼합 H2·H3 두 개다.
- 게임플레이 추적: 엄격 기준 플레이가 사용하는 새 Nature·Farm H3는 플레이 단계에서 명시적으로 추적한다. AreaSet이 H3를 요구한다는 사실만으로 플레이 계보를 자동 보완하거나 E단계를 승격하지 않는다.
- E 경계: 새 H3와 이론 AreaSet 재조립은 위치 독립 `TheoryQualified`·`E5TheoryQualified` 증거다. 실제 지역 Graph 결속, 공공데이터, Unity Scene·Runtime 또는 사람 검토 완료를 뜻하지 않는다.
- 관계: D-192의 H2 대기 결속을 완료된 팩 내부 H3에 한해 종료하고, D-188~D-191의 H·게임플레이·E 독립 축과 StableId 보존 원칙을 유지한다.

## D-194 Nature–Town 혼합 경관은 선택 계보로 만들고 실제 E5 배치를 자동 생성하지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 공간 구성: `MIX-H2-NATURE-TOWN-01`은 Town 구호 물자 인계점과 Nature 복원 작업·안전 회복 공간을 연결한다. `MIX-H3-NATURE-TOWN-01`은 Town 회수·구호 블록, 이 전환 블록, Nature 복원·회복 블록을 `Town 구호 → Nature 복원 → 안전 생활핵 귀환` 흐름으로 묶는다.
- 문법 경계: 기준 156 문법에 존재하지 않는 Nature–Town 전환어를 새로 꾸미지 않는다. H2는 타운·농촌 곡선 도로, H3는 건물 전면·수변 전환 문법을 조합하고 혼합 의미는 H 계보와 연결구가 담당한다.
- AreaSet 경계: 혼합 H3는 Nature와 Town H4 청사진 양쪽의 선택 가능한 교차 경관 계보다. 같은 Graph를 두 이론 AreaSet에 자동 복제하지 않으며 실제 소유 Graph, 양쪽 연결점과 GraphRelation은 후속 E5에서 결정한다.
- E 경계: H2 34개·H3 18개의 `TheoryQualified`와 패턴 대기열 0개는 위치 독립 설계 재고 완성을 뜻한다. 새 경관의 WI 추적·실제 지역 E5·공공데이터 E6·Unity Runtime을 완료했다고 주장하지 않는다.
- 관계: D-191의 P4·P5 혼합 생산 순서를 실행하고 D-188~D-190의 H·게임플레이·E 독립 관문을 유지한다.

## D-195 H2는 배치 가능한 물리 블록이고 H3는 배치 가능한 구역 조립안이다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 주 이름: H2·H3 목록과 공간 계획기에서는 이름 대장의 `spatialDisplayNameKo` 물리 공간 이름을 먼저 표시한다. 기존 `탐색·대피형`, `집중 집하형`, `순환시장형` 같은 행동 중심 이름은 삭제하지 않고 `gameplayProfileNameKo` 보조 활용 유형으로 내린다. StableId와 팩·계열 `patternCode`는 바꾸지 않는다.
- H2 계약: `BlockPattern`은 여러 H1의 로컬 상대 배치, 내부 이동 관계, 기준 경계, 허용 회전, 크기 변형과 외부 연결 역할을 봉인해 도시·농장·자연권 계획기에 한 단위로 놓을 수 있어야 한다.
- H3 계약: `LandscapeAssemblyPattern`은 여러 H2의 상대 배치, 블록 사이 이동 관계, 기준 경계, 구역 형태와 외부 연결 역할을 봉인해 지구·캠퍼스·회랑 단위로 놓을 수 있어야 한다.
- 게임플레이 경계: WI와 사건 흐름은 H2·H3의 존재 이유와 검증 계보지만 공간 자원 자체의 주 이름이나 자원 종류를 대신하지 않는다. 같은 물리 블록은 능력·용량 관문을 만족하는 여러 WI가 재사용할 수 있다.
- 근거 경계: `LocalMeters` 기준 경계와 `spatialFormCode`는 위치 독립 이론 배치 계약이다. 실제 지역 좌표·도로·건물·공공데이터 E6, Unity Scene·Runtime E7 또는 사람 승인을 증명하지 않는다.
- 관계: D-187의 H2 첫 공간 판단 단위와 D-191의 팩 주도 식별자를 유지하면서, 행동 중심 표시 때문에 H2가 블록처럼 읽히지 않던 문제를 공간 계획기용 자원 계약으로 보완한다.

## D-196 실제 E5는 네 전용 AreaSet과 하나의 Network로 결속하고 모든 이론 H3의 처리를 명시한다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 공간 소유권: Nature·Farm·City/Hub·Town은 각각 독립 `area-set:sim:*`을 소유하고, AreaSet 내부 H3는 내부 `LandscapeGraph`, 영역 사이 H3는 `AreaSetNetwork` 소유 경로 Graph로 둔다. 기존 `area-set:sim:pyeongchang:farm-hub-town.v1`은 공개 계약 호환 facade로 보존하며 새 실제 공간의 소유권으로 재사용하지 않는다.
- 현재 결속: 내부 Graph 14개와 Network 경로 Graph 3개를 AreaSet 4개·Network 1개·방향 관계 8개로 결속한다. 현재 이론 H3 18개 가운데 17개는 실제 E5 소유자를 갖고 `h3-candidate:nature-town-relief-loop` 1개는 정책에 명시적으로 보류한다. 이후 이론 H3도 승격 또는 보류 중 하나로 분류되지 않으면 생성을 실패시킨다.
- WI 결속: 전체 WI 41개를 직접 공간 결속 30개, AreaSet 문맥 결속 5개, 비공간 6개로 상호 배타적으로 분류한다. 기준 플레이의 Nature·Farm WI는 이 실제 대장을 근거로 유효 통합 단계를 E5로 계산하고 `reference-play:nature-farm-day.v1`을 `SpatiallyComposed`로 올린다.
- 좌표·증거 경계: 이번 실제 E5는 사람이 승인한 게임 기획을 `ScenarioLocalMeters`에 결정적으로 작성한 공간 결속이다. 공공데이터·실제 도로·DEM 계보 E6, 실행 중 서버·Session DB·사람 조작 E7, 운영 상태나 사람 시각 승인을 뜻하지 않는다.
- Unity 적재: canonical `SimulationWorldShell` 하나에서 Nature는 상시 유지하고 Farm·City/Hub·Town은 선택된 업무 영역만 적재한다. Network 경로 Graph는 전환에 필요한 경로만 준비·캐시하며 별도 공식 Scene을 만들지 않는다.
- 관계: D-190의 `ActualE5BindingMissing` 현재 판정을 대체해 `SpatiallyComposed`로 올리되, `FunctionallyClosed` 이후 관문은 그대로 유지한다. D-194의 Nature–Town 혼합 H3 보류와 D-188의 H·게임플레이·E·완주 독립 축을 유지한다.

## D-197 지역 위협·회복과 카드 효과는 서버 권위 v5 인과 원장으로 계산한다

- 상태: `Accepted`
- 결정일: 2026-08-19
- 인과 원장: Simulation Session은 원인 경로별 `Threat`와 `Recovery`를 함께 보존한다. 확정된 안전 업무 결과와 Nature 복원·파티 회복은 회복을 높이고 위협을 낮추며, 불안전 선택과 기한 초과는 반대로 적용한다. 미리보기 차단과 일반 취소는 인과 입력이 아니다.
- 카드 경계: 정방향의 유리한 카드는 회복 쪽, 역방향의 불리한 카드는 위협 쪽 변화로 확정한다. 다음 날 `Normal / Opportunity / Threat / Recovery`, 사건 심각도와 경로 영향은 서버가 계산하며 Unity는 결과 코드와 원장을 표시할 뿐 수치나 플러스·마이너스를 다시 계산하지 않는다.
- 저장 호환: `simulation-save.v5`는 인과 상태와 변화 계보를 저장·재생한다. v1~v4 저장·재생은 당시 의미를 보존하기 위해 새 인과 규칙을 적용하지 않으며 기존 결과 hash 호환을 유지한다.
- 표현 경계: Unity HUD는 서버 상태 사본을 읽어 위협·회복과 다음 날 결과를 보여 준다. 저장 Scene 배선과 EditMode 시험은 E7 사람 조작, Play Mode·Game View 또는 업무 완료의 증거가 아니다.
- 관계: D-177의 심리 영역·업무 영역 되먹임을 실행 규칙으로 구체화하고, D-196의 실제 E5 공간 소유권과 독립적으로 서버 상태 권위를 유지한다.

## D-198 H 공간 공장은 모든 계층에서 명시적 연결점 의미와 방향성 흐름을 재귀 검증한다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 재귀 계약: H2는 H1 배치와 H1 관계, H3는 H2 배치와 H2 관계, 이론 AreaSet은 H3 Graph 배치와 H3 관계, Theory World는 AreaSet 배치와 AreaSet 관계로 구성한다. 배열 순서에 따른 암묵 이동선은 사용하지 않고 `h2RelationRecipes`, `h3RelationRecipes`, `areaSetRelationRecipes`, `worldRelationRecipe`를 실행 입력으로 사용한다.
- 연결 의미: 연결점은 원본 하위 공간·연결점 계보, `Input / Output / Bidirectional` 방향과 복수 이동 종류를 가진다. 관계는 `Directed / Bidirectional`, 이동 종류와 호환 규칙을 명시한다. 관계 고유 식별자는 양쪽 공간·연결점·관계 의미에서 계산하고 정규 정렬 뒤 hash를 생성한다.
- 적격 판정: 하위 공간 두 개 이상과 무방향 연결 성분을 구조 관문으로 검사하고, 연결점 존재·방향·이동 종류·호환 규칙·필수 흐름의 방향성 도달을 의미 관문으로 검사한다. H3는 H2 두 개 이상이어야 하며 기존 Farm–Hub·Hub–Town 단일 회랑 H3는 출하/출고 블록·회랑·입고 블록의 세 H2로 보정한다.
- 실패 분리: 알 수 없는 고유 식별자·방향·호환 규칙 같은 잘못된 계약은 생성을 실패시킨다. 필요한 연결점이나 흐름이 아직 없는 정상적인 설계 미완료는 `SemanticRelationUnresolved`로 남긴다. `SpatialInventoryGap`만 새 H 후보의 근거로 사용하고 `EvidenceGap`은 기존 공간의 WI·시험·계보 보완 대상으로 처리한다.
- 세계 흐름: Nature와 Farm·Town·City/Hub 사이는 양방향 플레이어 이동으로, Farm→City/Hub→Town은 단방향 화물 흐름으로 검증한다. `TheoryWorldQualified`는 위치 독립 의미 폐쇄일 뿐 사람 승인, 실제 지역, 공공데이터 E6, 서버·Unity Runtime E7 권위를 주장하지 않는다.
- 관계: D-189의 사람 검토 비차단 생산, D-195의 배치 가능한 H2·H3 정의와 D-196의 실제 E5 소유권을 유지하면서, 단순 배열 연결을 의미 기반 공간 계획으로 대체한다.

## D-199 LH는 스트리밍 범위와 셀 내용을 분리하고 L과 H를 조회 관계로만 연결한다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 책임 분리: LH의 스트리밍 범위 계산기는 플레이어·NPC 관심점에서 필요한 L3 셀과 상세·활성·선행 준비 우선순위만 계산한다. 셀 내용 공급자는 그 셀에 H 계보·공간 배치·연결구·능력 근거·현재 상태를 결합한다. 조정 서비스는 두 책임을 순서대로 호출하며 어느 구현도 상대 책임을 다시 계산하지 않는다.
- L/H 관계: L0~L3는 실행 해상도이고 H1~H4는 의미 구조다. L0→H4, L1→H3, L2→H2, L3→H1은 주 조회 기본값이지 등식·소유권·완료 조건이 아니다. 한 셀은 여러 H 단계를 함께 조회할 수 있고 하나의 H 공간도 여러 셀에 걸칠 수 있다.
- 공급자 경계: 기존 절차 생성은 시험용 셀 내용 공급자로 보존한다. 후속 실제 세계 공급자는 E5 실제 공간, E6 현실 공간자료와 서버 시뮬레이션 현재 상태를 결합한다. 실제 세계 조회 실패나 증거 부족을 시나리오 절차생성으로 조용히 대체하지 않는다.
- 호환: 기존 `DefaultHLevelCode`와 Preview route·안정 식별자·hash는 유지하고 `PrimaryHQueryLevelCode`와 `ContentSourceCode`를 추가한다. 문서와 화면은 한국어 역할명을 먼저 쓰고 실제 코드 이름은 첫 언급에만 병기한다.
- 완료 경계: 이 분리는 LH 엔진 구현과 시나리오 시험의 구조 증거다. E5·E6 실제 공급, Unity 셀 조립, 연결구 통과, 서버 확인 지점 상태 변경, 세계 개정 번호에 따른 재계획이 폐루프로 검증되기 전에는 LH E7 실제 실행 검증 완료로 승격하지 않는다.
- 관계: D-158의 L/H 직교성과 창 크기를 유지하면서 “기본 대응”을 주 조회 관계로 엄밀히 하고, D-160의 로컬 시험 경로와 D-196의 실제 E5 공간 소유권을 교체 가능한 공급자 경계로 연결한다.

## D-200 H2·H3 재고는 팩별 수량이 아니라 게임플레이 공간 수요로 증산한다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 수요 원장: H2·H3 신규 생산과 기존 재고 개정은 `gameplay-h-inventory-demands.v1`에서 게임 기획, 관련 WI, 재사용 H, 신규 H와 반드시 닫혀야 하는 흐름을 먼저 선언한다. `SpatialInventoryGap`만 새 H 생성 근거가 되며 `EvidenceGap`은 기존 공간의 WI·시험·Runtime 증거 보완으로 보낸다.
- 첫 증산: City/Hub의 보관→피킹→출고준비→상차와 Town의 배송→입고·검수→후방재고→진열→피킹·포장→주민 수령을 우선한다. 기존 H1을 재사용해 H2 3개와 H3 2개를 추가하고 기존 H2 34개·H3 18개의 StableId를 보존한다.
- 자격 경계: 신규 H는 위치 독립 `TheoryQualified`까지 자동 생산할 수 있으나 WI의 E단계, 실제 E5, 공공데이터 E6와 서버·Unity E7을 자동 승격하지 않는다. 이론 AreaSet에는 신규 H3를 포함하되 실제 E5 정책에서는 명시적으로 보류한다.
- 관계: D-170의 게임 기획 주도 재고, D-188의 H·게임플레이·E·완주 독립 축, D-198의 재귀 의미 관계 검증을 유지하면서 균등 재고 목표보다 실제 플레이 인과선의 역할 분리와 의미 폐쇄를 우선한다.

## D-201 E8은 NPC 생활세계의 자율 행동 폐루프를 검증한다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 단계 정의: E8은 선정된 E7 세계에서 NPC가 지속되는 정체성·역할·생활 상태로 다음 목표를 결정하고, 실제 H 공간과 경관 그래프 경로를 선택해 공간·자원을 예약하고 WI를 수행한 결과가 다음 행동에 반영되는 연속 폐루프를 검증하는 단계다.
- 최소 관문: NPC 두 명 이상, 업무 WI 한 종류 이상, 비업무 생활 이동 한 종류 이상, 공유 공간·자원 경쟁 또는 인계 한 건 이상, Effect가 후속 선택을 바꾸는 연속 행동 두 회 이상, Save/Replay 동일성과 실제 서버·저장 `SimulationWorldShell`의 Play Mode·Game View 증거를 요구한다.
- 이동 의미: `NpcTraversal`은 출근·대기·휴식·시장 방문·대피·순찰 같은 일반 생활 이동, `WorkTraversal`은 확정된 WI·Task를 위한 업무 이동, `CargoLogistics`는 화물 이동으로 구분한다. 경로 부재를 순간이동·임의 좌표·Scenario fallback으로 숨기지 않는다.
- 권위 경계: Simulation 서버가 NPC 목표·행동·예약·완료를 결정하고 Unity는 그 결과를 표현한다. NavMesh 도착, Animator Event와 NPC 수는 E8 증거가 아니며 실제 계약·결제·배차·발주·정산을 NPC 자율 행동으로 실행하지 않는다.
- 적용 범위: E8은 모든 WI의 공통 목표가 아니라 `AreaSet + H 경로 + WI 묶음 + NPC 역할군`별 선택 증거다. 이번 결정은 정의와 완료 관문만 확정하며 기존 E0~E7 실행 원장·API·Simulation·Unity 구현을 변경하지 않는다.
- 상세 기준: [E8 NPC 생활세계 폐루프 정의](../Architecture/E8-NPC생활세계폐루프정의.md)를 따른다.
- 관계: D-013의 NPC 이동 Presentation 경계, D-117의 Simulation NPC 조직·역량·정책 권위, D-188의 독립 증거 축과 D-196의 실제 E5 공간 소유권을 유지한다.

## D-202 H5는 권위 상대 공간이며 E6는 선택형 현실 결속이다

- 상태: `Accepted`
- 결정일: 2026-08-20
- H4/H5 경계: H4 청사진은 재사용 공간 의미이고 AreaSet은 특정 세계 문맥의 H4 지역 인스턴스다. H5 `WorldLayout`은 H4 인스턴스와 물리 H3 회랑의 상대 배치를 소유하고, 이동 가능성과 경로 선택은 기존 `AreaSetNetwork`가 계속 소유한다.
- 좌표 계약: H5 직접 자식만 H5 루트 로컬 좌표 `ScenarioLocalMeters`를 사용하고 H4 이하 자식은 바로 위 부모 기준 `ParentLocalMeters`를 사용한다. 최종 위치는 부모 회전을 포함해 변환 합성하며 임의 Scale을 허용하지 않는다.
- E6 선택성: E6는 모든 H5의 필수 단계가 아니다. 세계 배치 정의, 현실 결속 적용 상태, 현실 자료 준비도를 분리하고 첫 세계는 `Optional / ScenarioRelative / NotApplied / Partial`로 둔다. E6 부재는 LH를 절차생성으로 후퇴시키는 근거가 아니다.
- 불변성: E6와 Floating Origin은 H5 이하 상대 X/Z와 배치 hash를 바꾸지 않는다. 실제 자료 정합을 위해 지역·회랑·연결 지점을 옮겨야 하면 E6 수정이 아니라 새 H5 revision을 만든다.
- 저장 계보: LH 저장 상태는 H5 고유 식별자·revision·배치 hash, 배치 권위, 현실 결속 상태와 적용된 경우의 현실 근거 hash를 보존한다. E6 준비도는 권위 상태가 아니다.
- 상세 기준: [H5 세계 배치와 선택형 E6 현실 결속](../Architecture/H5세계배치와선택형E6결속.md)을 따른다.
- 관계: D-196의 실제 E5 AreaSet/Network 소유권과 D-199의 LH 공급자 분리를 유지하면서 H5 세계 배치와 선택형 E6 투영 경계를 추가한다.

## D-203 DEM·도로는 공통 필수 자료가 아니라 현실 결속 프로필의 선택 요구다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 결정: H1~H5 공간 설계, WI Simulation, Scenario 상대 공간과 E7 실제 플레이는 DEM·도로·건물·블록 경계의 존재를 공통 선행 조건으로 요구하지 않는다. 해당 자료는 E6 현실 결속을 적용하기로 선택한 프로필이 그 목적에 필요하다고 선언할 때만 요구한다.
- 정책 판정: `NotRequired`와 `Optional / NotApplied`는 자료 부재로 차단하지 않는다. `Required` 또는 `Optional / Applied`는 선택된 프로필 안에서만 자료 요구·준비도·오염 전파를 판정하며, 누락은 해당 현실 결속 목표를 막되 H5 권위 배치나 Scenario 실행을 무효화하지 않는다.
- 계약 경계: 위치 독립 H와 WI 공간 계획에는 `requiredEvidencePurposeCodes`를 두지 않는다. 현실 자료 후보는 별도 `realityGrounding`에 정책·적용 상태·목표 완료 필요 여부·후보 목적·한계를 기록한다. DEM이나 도로를 다른 공급자로 묵시 대체하거나 `Required` 요청을 Scenario로 후퇴시키지 않는다.
- 기존 결정 정리: D-152의 `E6 WI 필수 공공데이터` 표현을 모든 WI에 적용되는 보편 의무로 읽지 않는다. D-202의 선택형 E6를 구체화하며, 특정 지리 파생 프로필 내부의 필수 Layer 판정은 그 프로필에만 유지한다.
- 기준 문서: [H5 세계 배치와 선택형 E6 현실 결속](../Architecture/H5세계배치와선택형E6결속.md)을 따른다.

## D-207 E6는 AreaSet 정밀 몰입 성숙도이며 GIS 결속은 독립 선택 축이다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 단계 정의: E6는 E5로 성립한 AreaSet의 H3·H2·H1·WI·상태 변화를 질문 행렬로 정밀 조사하고, 모든 H3와 교차 H3 인과 폐루프를 근거와 함께 설명할 수 있는 성숙도다. E5 합격 상태를 덮어쓰지 않고 `SpatialMaturity`, `ImmersionMaturity`, `FreshnessState`, `GroundingStatus`를 독립적으로 보존한다.
- 첫 적용: Farm AreaSet의 농가·생산·후처리, 고지대 농장, 계절 생산·출하, 사고 격리·회복 네 H3와 수확→후처리→출하 및 사고→복구→생산 복귀 폐루프를 첫 `RequiredBeforeE7` 대상으로 삼는다.
- 현실 문맥: 농사로 작물·재해 문맥과 KAMIS 시장 관측 계약은 출처·판본·hash·제한을 가진 설명 근거다. USDA AMS와 GIS는 선택 근거다. 어떤 공공자료도 H5 좌표, 생산량·수익성, WI 규칙을 자동 변경하지 않으며 라이브 공급자 호출 없이 계약 참조만으로 판정한 사실을 명시한다.
- 최신성: AreaSet/H5/H3/H2/H1/WI/질문/정책/근거/생성기 입력 hash가 달라지면 결과는 `Stale`이며 E7 시작 관문을 닫는다. `ImmersionQualified + Stale`을 허용해 과거 합격 사실과 현재 재검사 필요를 함께 표현한다.
- E7 경계: E6 합격은 E7 검증을 시작할 수 있다는 뜻일 뿐 실제 서버 HTTP, 저장 Scene, Play Mode, Game View 또는 E7 완료 증거가 아니다. GIS `NotApplied`만으로 E7을 차단하지 않는다.
- 관계: D-202의 H5 권위 상대 공간과 D-203의 선택형 GIS 자료 요구를 유지하면서, 과거의 E6를 GIS 적용 여부 하나로만 해석하지 않고 AreaSet 이해 성숙도로 확장한다.

## D-204 전투 맵은 H5의 확대가 아니라 지역 문맥에서 결정적으로 파생한 독립 공간이다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 공간 경계: H5 생활세계는 전투 장소의 계보·주변 H 공간·연결 지점·경로·접근 방향을 제공한다. 전투 맵은 이 사실을 `BattleLocalMeters`에서 전술적으로 재구성한 별도 Simulation 공간이며 좌표를 H5로 역투영하거나 Floating Origin 보정을 권위값으로 저장하지 않는다.
- 지역 유효성: 전역 `WorldRevision` 불일치는 재검증 계기일 뿐 자동 거부 사유가 아니다. 현재 1km 지역 문맥 해시와 전장 파생 입력 해시가 미리보기와 같으면 확정할 수 있고, 전체 H5 배치 hash는 계보로만 보존하며 전장 Seed에 넣지 않는다.
- 생성 순서: 문맥 추출 뒤 보존 등급·원본 대상·경로·관계·문맥 경계 포털을 난수 없이 먼저 확정하고, 그 제약 집합 hash 뒤에만 Seed를 계산한다. 첫 Profile은 1km 문맥에서 500m `FarmPerimeter500` 또는 `NatureField500` 전장을 만든다.
- 전투 권위: 부대 시작 사본에는 배우 상태·역할·역량과 카드의 플러스·마이너스 수정치, 전투 규칙 개정을 함께 봉인한다. 참여 배우와 충돌 가능한 세계 대상은 전투 동안 예약하고 100ms BattleTick과 고수준 명령은 서버가 처리한다.
- 결과 합류: 전술 피해값을 H5 수치로 복사하지 않고 시설 피해·관문 피해·배우 부상·목표 확보/상실 의미 효과로 집계한다. `전투 + 원본 대상 + 효과 코드 + 효과 고유 식별자` 멱등 키로 다음 안전한 WorldTick에 한 번만 적용한다.
- Unity 경계: canonical `SimulationWorldShell` 안에서 서버 전장 계획을 별도 Root로 조립하되 전장 생성·판정·세계 결과를 Unity가 다시 계산하지 않는다. 코드·EditMode 증거와 실제 Play Mode·Game View 전환 증거는 구분한다.
- 관계: D-197의 카드·Simulation 서버 권위, D-202의 H5 상대 공간 불변성과 D-199의 실행/표현 분리를 유지하면서 경영세계→전투→경영세계 폐루프를 추가한다.

## D-205 H5 통합 생활세계는 정적 장소와 Session 가변 시설을 결합한 WI 폐루프로 구현한다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 공통 뼈대: 농사·제조·물류·건설·군사·전투·복구는 별도 게임 모드나 선행 범용 엔진이 아니라 같은 Session의 `Preview → Confirm → 예약 → Task → Effect`를 공유하는 수직 WI 묶음으로 구현한다. 앞선 Effect가 다음 WI의 가능성을 실제로 열거나 닫아야 한다.
- 공간·시설 경계: H5·H1은 장소와 능력 수용 조건을 제공하고 병영·창고·제조소는 그 장소를 점유하는 `RuntimeFacility`다. 기존 시설은 Scenario seed에서 결정적으로 생성하며 플레이어 병영은 건설 확정 때 `Planned`로 생성되어 건설 중과 운영 상태를 같은 Project와 원자적으로 전이한다.
- 상태 권위: 시설 피해 Effect는 불변 기록이고 적용 대기 항목·적용 영수증·현재 능력 제한을 분리한다. 수리는 확정 시 고정한 제한만 해결하며 시설 능력은 정의와 남은 활성 제한에서 계산한다. Actor 모집·훈련·편성·전투는 실제 Actor 점유와 부상 가용성을 공유한다.
- 전투 결속: `BattleRelevantOverlayHash`는 Session 전역값이 아니라 전투 범위별 파생값이다. `BattleWorldContextHash`는 정적 지역 문맥·조우 범위·공격/방어 문맥·관련 Runtime 투영과 규칙 버전으로 계산하고 `SourceWorldRevision`은 추출 메타데이터로만 둔다.
- 결정성과 저장: 제조 Job·건설 Project·Formation은 각 WI 사이를 잇는 지속 객체이며 시작 시 규칙 revision/hash와 계산된 입출력·기간을 동결한다. Lot 소비·출력 생성·자재 소비·피해 적용은 명령 멱등성과 적용 영수증으로 정확히 한 번 수행하고 `simulation-save.v6`에서 재생한다.
- 자원 규칙: 운송 상자는 감자 포장에 소비되는 자원이지 일반 화물 운송의 공통 필수조건이 아니다. Hub 제조→Farm 운송→병영 건설·감자 포장→민병대 편성→전투 피해→부품 운송·수리→출하 복구를 첫 기준 폐루프로 삼는다.
- 증거 경계: 기준 구현 목표는 위치 독립 Scenario E3다. 이 계약만으로 실제 E5 공간 결속, 선택형 E6 현실 자료, Unity E7 Runtime 또는 NPC E8 자율성을 주장하지 않는다.
- 상세 기준: [H5 통합 생활세계 기준 구현 계약](../Architecture/H5통합생활세계기준구현계약.md)을 따른다.

## D-206 소규모 현장 전투와 대규모 파생 전장은 같은 서버 전투 원장을 사용한다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 규모 정책: 적 1~3명은 즉석 교전, 적 4~5명·정예·동료 참여는 현장 전투, 그보다 큰 무리·대규모 습격·강제 사건은 H5 전투 문맥 기반 독립 전장으로 판정한다. 판정과 전환은 서버 권위이며 Unity가 적 수만으로 다시 계산하지 않는다.
- 공간·시간 경계: 현장 전투는 현재 H5/LH 공간을 유지하고 전투 참여 대상 주변의 상세·활성 셀을 고정한다. 전투 중 생활세계 `WorldTick`은 멈추지만 전투 원장은 100ms `CombatTick`으로 진행한다. 독립 전장은 D-204의 `BattleLocalMeters` 파생 공간을 그대로 사용한다.
- 단일 원장: 두 공간 방식은 배우 체력·부상·공격·방어·카드 수정치·예약·의미 효과·Save/Replay를 공유한다. 현장 전투가 확대되면 남은 체력·기력과 참여자를 새 부대 시작 사본에 봉인하고, 독립 전장 결과는 기존 멱등 세계 효과 규칙으로 H5에 합류한다.
- 조작 방식: 카메라 이름을 권위값으로 사용하지 않고 서버가 한 번에 하나의 `DirectAction` 또는 `TacticalCommand`를 확정한다. 1인칭은 직접 이동·기본 공격·방어/반격·회피·역할 카드 기술을 사용한다. 전술 3인칭은 대상 선택·접근/퇴각·대형 유지와 부대 기본 공격 지휘만 사용하고 개인 회피·반격·역할 기술을 실행하지 않는다. 시점 전환이 끝난 뒤 해당 조작 방식이 서버에 확정되기 전에는 새 전투 입력을 보내지 않는다.
- 카드 분리: `Direct*` 카드 수정치는 1인칭 개인 방어와 역할 기술에만 적용한다. 정찰 범위·대형 응집·농장 방어 준비·추격·보급·전개 같은 전술 수정치는 3인칭 부대 기본 공격, 대형 방어와 명령 간격에 적용한다. 카드 사본·플러스/마이너스·규칙 개정은 같은 전투 시작 사본과 Save/Replay에 봉인하며 Unity가 수치를 다시 계산하지 않는다.
- 이전 결정과 관계: D-136의 서버 권위 단일 판정 원칙은 유지한다. D-136·D-137의 기존 좌·우클릭 고정 전투 입력은 현장 전투에서 이 결정의 시점별 번역으로 대체하며, 시점 전환 자체가 WorldTick이나 전투 결과를 바꾸지 않는 D-134·D-135 경계는 유지한다.
- 증거 경계: 코드·단위시험·EditMode·PlayMode 자동 시험은 실제 수동 조작과 Game View 증거를 대신하지 않는다.

## D-208 카드 서랍은 의미 투영을 통합하되 원장·권위·실행 책임을 통합하지 않는다

- 상태: `Accepted`
- 결정일: 2026-08-20
- 의미 층: `Meta / Context / Action / Knowledge / Research`는 소유·상속·권한 트리가 아니라 세계를 해석하는 의미 층이다. 타로는 가장 넓은 문맥을 제안하지만 행동 허용이나 Effect를 직접 확정하지 않는다.
- 타로 경계: `FrameSet`은 턴·일·계절·지역·사건 프레임을 향후 함께 담을 수 있게 하되 첫 버전은 턴 프레임 하나만 허용한다. `ContextProposalCode`는 사건 평가 입력이며 `IncidentStableId`나 `EffectStableId`가 아니다. `NoIncident`는 정상 결과다.
- 관련성 경계: `Relevant / Recommended / Warned / Contrasted / AvailabilityExplained / BlockExplained`는 Unity 강조와 서버 설명 관계다. 관계 자체는 가용성을 바꾸지 않으며 실제 허용·차단은 각 도메인 규칙이 확정한다.
- 전투 편성: 카메라 시점이 아닌 `DirectAction`/직접 전투와 `TacticalCommand`/전술 지휘를 권위 조작 방식으로 사용한다. 두 조작 방식의 주력·지원 편성은 독립하고, BattleInstance 생성 시 카드 복사본·소스 개정·적용 조작 방식·효과·규칙 개정을 동결한다. 전투 중 원본 편성 변경은 현재 전투에 영향을 주지 않는다.
- Unity 경계: canonical `SimulationWorldShell`의 `C` 카드 서랍은 계열별 조회 결과와 행동 route만 통합한다. 턴 마감·팀 역할·전투·정보·연구 모판의 기존 소유자가 명령을 계속 처리하며 Unity는 타로 수치, 전투 수정치, 행동 허용을 재계산하지 않는다.
- 호환성: 기존 턴 카드 필드와 연구 Scene은 호환 영역으로 보존하되, 새 실행 코드는 문자열 접두사로 직접/전술 효과를 추론하지 않는다.
