# Unity World 구현 현황과 우선순위

## 1. 목적

이 문서는 서버 상태를 Unity World로 투영하는 현재 구현을 한곳에 모으고, 다음 작업을 **기술 의존성**, **현재 slice 완결 비용**, **제품 공개 우선순위**, **권한·개인정보 위험**을 기준으로 정렬한다.

P0~P7의 기본 배선 이후 각 공간의 현실 업무·상태 변화·NPC·interaction을 깊게 구현하는 순서와 첫 창고 slice는 [Unity Zone 업무 심화 설계](UnityZoneDomainDeepeningDesign.md)를 따른다.

새 데이터 종류를 더 연결하기 전 읽기 변환은 [Unity Data·Interpretation·Presentation 기준 아키텍처](UnityDataInterpretationPresentationArchitecture.md)에 따라 분리한다. 현재 W1을 첫 migration pilot으로 삼고 기존 route·JSON·stable ID·refresh 동작을 보존한 뒤 W2로 진행한다.

사용자가 World에서 낯선 업무 의미를 학습하는 공통 진입 문법은 [Unity 개념 카드 Presentation 패턴](UnityConceptCardPresentationPattern.md)을 따른다. Scene asset 연결 전에 `Concept / Status / Reason / Action` 계약과 Projector를 먼저 안정화한다.

장소와 역할은 별도 Scene 복제 기준이 아니다.

```text
Canonical server state
  → 권한별 Projection API
  → Unity Repository / UseCase
  → Zone Controller + Role Experience
  → World View / Role View / Detail View
  → NPC movement presentation
```

Zone Controller는 장소의 현재 상태를 조율하고, Role Experience는 같은 장소에서 현재 사용자에게 허용된 대상과 행동을 강조한다. NPC는 서버 상태를 설명하는 시각적 투영이며 도착만으로 주문·배차·상차 같은 운영 Command를 발생시키지 않는다.

## 2. 현재 구현 기준선

| 영역 | 현재 상태 | 다음 완결 조건 |
| --- | --- | --- |
| 공통 데이터 계층 | stable ID, revision, provenance, Repository, UseCase, reconcile 구현 | 실제 제품 Unity 프로젝트의 HTTP adapter와 composition root 연결 |
| 도심마트 | UM0~UM5, SC0~SC5와 RG0~RG4·RG4-NPC-A/B code, CC0~CC2 완료 | CC3 + RG4-NPC-C Card View·Scene runtime wiring |
| 전통시장·공개 물류거점 | 공개 위치·출처 기반 simulated slice와 primitive builder 구현 | 기존 공개 세계지도 aggregate와 연결 |
| Role Perspective | 생산자·주문자·운송자 공통 계약과 applicator 구현 | 생산자·주문자 server aggregate 추가 |
| 도심 물류센터 운송자 | 인증된 현재 배정 운송 기반 server Role/NPC API 구현 | 실제 UnityWebRequest adapter, NavMesh bake와 runtime 확인 |
| NPC | Zone route catalog, NavMeshAgent·Animator socket, stable-ID 적용 구현 | Zone별 canonical API 확대와 실제 Animator Controller |
| 작물 | 농사로 기준정보 server→Unity read-only 흐름 구현 | Farm·Plot·Cultivation·Sensor canonical 운영 contract |
| Synty | 미도입 | placeholder 계약과 성능 기준 확정 후 최소 팩 검증 |

## 3. 현재 활성 개발 순서

현재 즉시 실행 순서는 cross-zone P0~P9 목록과 별도로 다음처럼 고정한다.

| 순서 | 작업 | 이유 |
| --- | --- | --- |
| 완료 | SS0~SS1 별도 Simulation 서버 기반 | 운영 서버 비침범, session·seed·revision·멱등 Tick 경계 |
| 완료 | UM5-B manager surface applicator·sample wiring | 실제 Unity 프로젝트 sample compile·EditMode 16/16; Scene·Game View는 별도 검증 |
| 완료 | SC0 공급·수요·주문 Data 계약 | 독립 revision·lineage와 운영 경계·참조·단위·합계 검증 |
| 완료 | SC1-A 감자·3공급처 fixture | 10 node·15 relation의 deterministic 공급 graph |
| 완료 | SC1-B 인구→잠재수요→Demand Scenario | 공공 basis와 명시적 4주 수요 가정 계보 분리 |
| 완료 | RG0 기존 서버 재사용 조사 | individual-demand·GroupPurchase·Order·GroupOrder·ResidentialPickup·공급중개 경계 |
| 완료 | SC1-C 기본 방문 Demand→Order Stream | 4주 56건, 기대수요 합계·same-seed·기한·lineage 보존 |
| 완료 | RG1~RG3 공동주택 fixture·대표 NPC identity·typed graph·수요 합성 | 사회적 context와 권한 분리, 의향과 group-order 확정 분리 |
| 완료 | SC2 4주 주문·공급 Engine | 합성 수요의 할당·충족·재고·현금·폐기·노동 인과관계 |
| 완료 | RG4 역할별 Perspective | 주민 본인·대표 집계·마트 공급 검토 projection과 inquiry/dialogue 권한 경계 |
| 완료 | RG4-NPC-A + SC3~SC5 headless | 두 Zone route leg·visit state, 공급 위험과 주문/계약 surface 입력 모델 |
| 코드 완료 | RG4-NPC-B + SC5 Unity binding | 대표 View, dialogue coordinator, 서버 계산 surface mapper/applicator와 package core compile |
| 완료 | CC0 Concept Card 방향 확정 | 네 카드 책임, 공통 계층, asset 중립성과 첫 대표 NPC deck |
| 완료 | CC1 카드 계약·Projector | 공통 identity·revision·mode·lineage 검증과 미승인 Action 제거 |
| 완료 | CC2 대표 NPC 7-card deck | RG4·SC3~SC5 값을 재계산 없이 source별 Presentation으로 투영 |
| 1 | CC3 + RG4-NPC-C Unity runtime wiring | imported sample 갱신, 카드 View·skin, Scene·NavMesh·Animator·manager desk 검증 |
| 2 | SC6~SC7 confirm·tick·UM4·대표 결과 전달 | Action Card와 Command 분리, 10분 playable과 하류 결과 완결 |
| 3 | RG5~RG7 + SC9 Operational | 기존 공동구매 Projection·ResidentialPickup·공급 Command 폐루프 |

세부 계약과 완료 기준은 [도심마트 공급 계약 경영 Simulation 설계](UrbanMarketSupplyManagementSimulationDesign.md)와 [도심마트 공동주택 주문자 집단 통합 설계](UrbanMarketResidentialOrdererGroupIntegrationDesign.md)를 따른다. 이 순서는 제품 0.0 기본 공개 순서를 바꾸거나 실제 계약·발주·결제를 활성화하지 않는다.

## 4. 통합 구현 우선순위

### P0. 현재 도심 물류센터 slice를 실제 연결까지 닫기

이미 server Role/NPC projection과 Unity Repository·UseCase, primitive Zone 배선이 있으므로 가장 적은 추가 비용으로 전체 아키텍처를 검증할 수 있다.

1. Role Perspective와 NPC movement용 `UnityWebRequest` adapter를 작성한다.
2. API base URL, 인증과 cancellation을 application `LifetimeScope`에 등록한다.
3. server JSON과 Unity ApiModel의 직렬화 호환 test를 추가한다.
4. 물류센터 NavMesh를 bake하고 기본 Animator Controller를 연결한다.
5. canonical 상태 재조회 시 같은 NPC stable ID가 새 route로 증분 갱신되는지 확인한다.

2026-08-08 현재 1~3의 코드 기반은 구현됐다. 실제 제품 Unity 프로젝트가 저장소에 없어 login session 공급과 4~5의 runtime 확인은 남아 있다. operational adapter는 인증 실패나 API 실패를 simulated fixture로 대체하지 않는다.

완료 기준은 실제 서버의 배정 운송 하나가 물류센터 Role target과 NPC 이동으로 함께 표시되고, NPC 도착이 운영 상태를 변경하지 않는 것이다.

현재 코드에는 이 slice의 다음 연결도 포함되어 있다.

```text
운송중
  → 운송 NPC: 물류센터 거점 → 창고 거점
하차지도착 + 입고 운송중
  → 운송 NPC + 창고 입고작업자 NPC: warehouse.inbound-dock 집결
입고완료
  → 운송 NPC: warehouse.vehicle-exit
  → 입고작업자 NPC: warehouse.storage-zone
```

화물은 `cargo:transport-{id}`, 업무는 `transport-task:{id}`와 `inbound-task:{id}`로 분리해 추적한다.

### P1. 공공데이터 정보관

제품의 기본 공개 범위인 0.0과 가장 잘 맞고 개인정보 위험이 낮다. 기존 `community/world-map/observations` aggregate와 123개 마커 검증 경험을 재사용한다.

지역 인구·실제 수요·물류 접근성을 별도 Layer로 확장하는 계약과 순서는 [지역 인구·수요 World Layer 제안](RegionalPopulationDemandWorldLayerProposal.md)을 따른다. 인구통계와 운영 주문을 같은 의미로 합치지 않고, 운영 수요는 권한과 개인정보 집계를 통과한 별도 Projection으로 둔다.

- 공개 관측 ScreenModel과 Repository adapter
- marker freshness·source·기준시각 표현
- 정보키오스크와 상세 근거 panel
- initial load/refresh failure 및 stable-ID reconcile

2026-08-08 현재 Unity ApiModel·Mapper·Repository·UseCase, stable-ID reconcile, 마지막 성공 유지 정책과 primitive 정보관 View까지 구현됐다. 실제 제품 Unity 프로젝트에서 공개 API marker 수와 위치 표현을 확인하는 runtime 검증이 남아 있다.

### P2. 커뮤니티·시장 광장

공개 정보가 공동행동으로 이어지는 제품 중심 공간이다. 게시판 글을 개별 3D 객체로 대량 복제하지 않고 게시판·원장 보드·상세 panel로 압축한다.

- 공개 게시판 projection
- 커뮤니티 활동 공개 범위
- 관심·참여·실행 상태의 명시적 분리
- 민감 상세와 실행은 Web handoff 또는 확인된 server Command

2026-08-08 현재 기존 공개 게시판·게시글·비식별 활동 신호를 결합하는 server aggregate와 Unity ApiModel·Mapper·Repository·UseCase, stable-ID reconcile, primitive 광장 View까지 구현됐다. 공개 계약에는 작성자 식별자·연락처·댓글 본문·원장 ID·담당자·실행 행동이 포함되지 않는다. 실제 제품 Unity 프로젝트에서 operational API 표시를 확인하는 runtime 검증은 남아 있다.

### P3. 창고·재고

서버에 canonical Entity와 업무 상태가 존재하고 도심 물류센터의 Dock·팔레트·NPC 구조를 가장 많이 재사용할 수 있다.

- authorized warehouse snapshot
- 팔레트·상품상자·입고·피킹·출고 View
- picker와 dock worker NPC
- 재고수량, 작업상태, viewer scope의 서버 판정

2026-08-08 현재 기존 `재고현황UseCase`, `적재작업UseCase`, `피킹작업UseCase`의 권한 필터 결과를 결합하는 `WarehouseManager` 전용 server aggregate와 Unity ApiModel·Mapper·Repository·UseCase가 구현됐다. 재고·작업·NPC 참조 무결성, stable-ID reconcile, 마지막 성공 유지 정책과 팔레트·작업 표식·DockWorker·Picker primitive socket까지 연결했다. 실제 제품 Unity 프로젝트의 NavMesh bake와 operational API runtime 검증은 남아 있다.

### P4. 운송 World 연결

물류센터 안의 NPC에서 농장·창고·마트·공동수령지 사이의 노드 이동으로 확장한다. 초기에는 실제 도로 시뮬레이션 대신 server가 결정한 출발·목적지와 semantic route만 표시한다.

- transport corridor node와 TruckView
- 운송자 다음 작업·상차·하차 Role View
- canonical 재조회와 stale 처리
- 주소·연락처는 배정 업무에 필요한 서버 projection 범위만 사용

2026-08-08 현재 기존 `warehouse-handoff` API의 `InTransit` 상태와 Transporter movement를 재사용해 `TransportCorridorSnapshot`으로 투영한다. `TruckMovementApplicator`는 truck stable ID와 revision을 검사하고, 도심 물류센터 sample은 `network.logistics-center`에서 `network.warehouse`로 향하는 NavMeshAgent TruckView와 cargo VisualRoot를 제공한다. 도착·입고 완료는 Unity가 확정하지 않으며 서버 handoff 재조회 결과가 운송중이 아니면 TruckView를 숨긴다. 임시 Unity 6 프로젝트에서 script compile, primitive scene 생성과 scene reload 후 truck wiring을 확인했다. 실제 제품 Unity 프로젝트의 NavMesh bake와 operational runtime 검증은 남아 있다.

### P5. 도심마트 operational + 주문자 관점

현재 primitive View를 살리되 상품·가격·재고를 각각 독립 API로 조립하지 않고 공개 가능한 마트 aggregate를 먼저 정의한다.

- 마트 상품·가격·재고·출처 snapshot
- 주문자 Role target과 상세 panel
- 주문 생성은 확인 panel → server UseCase → canonical 재조회

2026-08-08 현재 기존 공개 aggregate `GET api/v1/orderer/mart/products`를 Unity ApiModel·Mapper·Repository·operational UseCase로 연결했다. 서버가 공개한 판매가·판매 가능 수량·재고 기준시각을 그대로 사용하고 내부 창고 원문을 추론하지 않는다. 주문자 관점은 판매 가능 상품 탐색과 읽기 전용 상세 panel까지만 허용하며 주문 Command는 포함하지 않는다. VContainer에서 simulation/operational 모드를 명시적으로 선택하고 operational 실패를 fixture로 대체하지 않는다. 임시 Unity 6 프로젝트에서 operational adapter compile, primitive scene 생성과 reload wiring을 확인했으며 실제 API runtime 표시는 남아 있다.

### P6. 주거공동체 수령

주문자와 운송자가 같은 장소를 다르게 보는 첫 개인정보 민감 slice다. 다른 세대 정보, 상세 주소와 연락처가 client 필터에 의존하지 않도록 server projection을 먼저 고정한다.

- 공동수령지 World View
- 주문자 본인 수령 상태
- 운송자의 현재 배정 하차 대상
- 배송 증빙과 완료 Command의 권한·revision 검증

2026-08-08 현재 기존 `unloading-perspectives`의 주문자 본인·운송담당자 배정 관계 필터를 재사용하고, Unity에는 공동수령 point·상품 요약·상태·canonical task만 제공하는 `ResidentialPickupPerspective`를 추가했다. 주문자와 운송자는 역할 선택 파라미터가 없는 별도 인증 route를 사용한다. 주소·상세주소·연락처·사용자 ID·주문번호·결제·계약 정보는 계약에서 제외했다. Unity는 같은 point를 `내 수령 상품` 또는 `내 하차 대상`으로 표현하며 현재 interaction은 읽기 전용이다. 임시 Unity 6 프로젝트에서 operational adapter compile, 역할 전환 socket, primitive scene 생성과 reload wiring을 확인했다. 실제 API runtime과 수령 확인·하차 완료 Command는 남아 있다.

공동주택 같이 주문 playable에서는 P6 앞단에 `ResidentialGroupRepresentative` NPC를 추가한다. 이는 주민자치 대표 등 사회적 context의 표현이며 기존 `공동구매 대표` 역할 검증을 대체하지 않는다. 현재 route catalog에는 해당 actor가 없으므로 RG4-NPC에서 다음 두 leg를 additive하게 추가한다.

- 주거공동체: `community office → community board → departure point`
- 도심마트: `market entrance → manager desk → exit`

두 Zone을 하나의 `NpcMovementSnapshot` route로 합치지 않고 representative visit state가 leg를 연결한다. 도착·대화·Animator event는 문의·주문·계약·발주·수령 Command를 실행하지 않는다.

### P7. 농장·생산자 관점

시각적 중요도는 높지만 현재 서버에 Farm·Plot·Cultivation·Sensor canonical 운영 모델이 없으므로 추정 DTO를 먼저 만들지 않는다.

1. canonical aggregate와 stable ID를 결정한다.
2. 작물 기준정보와 실제 재배 상태를 분리한다.
3. Farm/Sensor API, Repository와 생산자 ScreenModel을 연결한다.
4. FarmTile, CropView, SensorView와 생산자 NPC를 적용한다.

canonical contract 전에는 명확히 표시된 simulation vertical slice만 허용한다.

2026-08-08 현재 `농장` root 아래 `농장구획`, `재배작기`, `농업센서`, `농업센서관측`과 `농장작업` canonical aggregate 및 EF migration을 추가했다. 생산자 API는 인증 사용자가 소유한 농장만 반환하며 공개 작물 기준 ID·출처와 실제 생육 상태를 분리한다. 센서는 원시값·단위·기준시각과 서버 판정 상태·규칙 revision·근거 card·한계를 함께 제공하고 위치·주소·소유자 ID는 제외한다. Unity에는 Repository·UseCase, FarmTile·Crop·Sensor View, canonical 농장작업을 참조하는 생산자 NavMeshAgent socket과 VContainer primitive sample을 연결했다. 임시 Unity 6 프로젝트에서 compile, scene 생성과 reload wiring을 확인했다. 실제 sensor ingestion, 운영 DB migration 적용, 인증 API runtime, NavMesh bake와 Animator Controller는 남아 있다.

### P8. 협동조합·공동원장 공간

커뮤니티에서 형성된 공동행동을 원장 보드와 회의 테이블로 투영한다. 참여 의향, 가원장, 실원장과 실행 권한을 한 상태로 합치지 않는다.

### P9. Synty 교체와 성능 최적화

모든 주요 View는 `VisualRoot`, `Animator`, `Renderer`, 장착점 같은 socket을 유지한다. 최소 환경·캐릭터 팩으로 URP, 스케일, Windows·Android 성능을 검증한 뒤 placeholder 외형만 교체한다.

## 5. 병렬로 보지 말아야 할 세 순서

- **개발 완결 순서**는 P0 도심 물류센터가 먼저다. 이미 구현된 server→Unity→NPC 흐름을 실제 adapter와 runtime까지 닫아 공통 패턴을 증명한다.
- **현재 도심마트 playable 순서**는 SC1-C → RG1~RG3 → SC2 → RG4+SC3~SC5 → SC6~SC7이다. Operational은 그 뒤 RG5~RG7+SC9에서 기존 원장을 재사용한다.
- **제품 공개 순서**는 P1 공공데이터 정보관과 P2 커뮤니티 광장이 먼저다. 현재 0.0 공개 범위와 제품 중심에 맞기 때문이다.

따라서 물류센터를 먼저 기술적으로 완결한다고 해서 3.5 기능을 기본 공개하거나 유상 운송·자동 배차를 활성화하는 것은 아니다.

## 6. 공통 완료 기준

- server가 권한·공개 범위·canonical 상태의 최종 권위다.
- Unity API model은 server DTO assembly를 직접 공유하지 않는다.
- stable ID, revision, source, 기준시각과 operational/simulation 구분이 보존된다.
- Controller는 DTO, NavMesh 좌표와 개별 Renderer를 직접 해석하지 않는다.
- Role View는 server가 허용한 대상만 강조한다.
- NPC 이동과 animation은 Presentation이며 운영 Command가 아니다.
- 상태 전이는 명시적 확인과 server 검증 뒤 같은 aggregate를 재조회한다.
- placeholder에서 계약과 runtime을 확인한 뒤 외부 asset을 도입한다.
