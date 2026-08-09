# Ssalddel Current Work

> GPT Chat과 Codex가 다음 작업을 이어받기 위한 최신 snapshot이다. 완료 이력을 계속 쌓는 일지가 아니며, 사실이 바뀌면 기존 항목을 현재 상태로 갱신한다. 장기 결정은 [DECISIONS.md](DECISIONS.md), 전체 맥락은 [공용 프로젝트 컨텍스트](../ProjectOverview/GptProjectContext.md)를 따른다.

## Snapshot

- 기준일: 2026-08-09
- 현재 작업 축: CC2 대표 NPC 7-card deck 완료 뒤 CC3 + RG4-NPC-C Unity View·Scene runtime wiring
- 제품 공개 기본값: 0.0 커뮤니티·공공데이터
- Unity 개발 범위: 제품 버전 순서에 종속되지 않는 전체 Ssalddel 도메인

## 현재 목표

기존 `Ssalddel` 운영 서버는 실제 사용자·조직·공동구매·개별 주문·계약·발주·입고·재고·결제의 권위를 유지하고, 별도 `Ssalddel.Simulation.Server`가 게임 session·scenario lineage·seed·가상 Tick·revision을 소유한다. 도심마트 UM5, SC0~SC5와 RG1~RG4·RG4-NPC-A/B code까지 완료됐고 CC2에서 대표 NPC 7-card deck adapter를 구현했다. 다음은 `ConceptCardView`·visual skin을 기존 Unity project sample과 manager desk Scene에 연결하고 NavMesh·Animator와 함께 검증하는 CC3 + RG4-NPC-C다. Simulation 주문·계약 확정은 실제 주문·계약·발주·결제·입고를 만들지 않는다.

```text
Authorized market operation data
  → Market Data Snapshot
  → Shared product / inventory / shelf / task graph
  → Global allocation integrity + multi-source plan
  → Market-manager replenishment Perspective
  → Shelf / Task / Detail Presentation
  → confirmed Command and canonical re-query
```

현재 서버 데이터 흐름은 다음을 우선한다.

```text
External source
  → opt-in server ingestion
  → private raw storage + run metadata
  → normalized public data + source lineage
  → Ssalddel API projection
  → Unity Data → Shared/Perspective Interpretation → Presentation
```

## 현재 방향

- Unity는 특정 Web 버전이나 route를 순서대로 3D 복제하지 않는다.
- 전체 Ssalddel 도메인을 `World`, `Data`, `Object`, `Interaction`, `Simulation`으로 구성한다.
- 실제 구현은 공통 계약과 작은 vertical slice 단위로 진행한다.
- 서버가 실제 운영 상태의 최종 권위를 가진다.
- simulation fixture와 operational data를 schema, 상태와 UI에서 구분한다.
- sensor는 일반 관측 상태와 물리 장비 표현을 연결하는 단일 projection이다.
- 외부 asset보다 placeholder와 View socket 계약을 먼저 검증한다.

## 최근 완료

- `Ssalddel.Unity` engine-independent package의 ApiModels, Mapping, Data와 Simulation 구조 정리
- 대표 route 18개의 `PageWorldProjectionCatalog` 작성
- stable ID와 revision 기반 `WorldProjectionReconciler` 작성
- 연구 근거 card model과 validator 작성
- sensor model을 단일 물리 장비 projection으로 정리
- 운영 interaction의 preview, 명시적 확인과 canonical 재조회 계약 작성
- Unity 계층 구조와 package-local 프로젝트 구조 문서 작성
- GPT Chat과 Codex가 함께 읽는 공용 컨텍스트·결정·현재 작업 문서 체계 구성
- EF DbSet 180개와 MongoDB 물리 collection 27개의 persistence inventory 조사
- DbSet 1개당 Controller를 만들지 않고 aggregate projection과 Zone Controller로 묶는 설계 작성
- 현재 서버에 Farm·Sensor·Crop canonical Entity가 없음을 확인하고 operational 연결 전 contract 필요성을 기록
- 도심마트 ScreenModel, simulated 조회 UseCase와 validation 구현
- `도심마트SceneController`와 마트·진열대·상품상자·가격표·재고·키오스크 View socket 구현
- primitive scene과 Inspector wiring을 생성하는 importable Urban Market sample 작성
- [Unity 도심마트 운영자 3계층 재정비 설계](../Architecture/UrbanMarketOperatorDataInterpretationPresentationRedesign.md)에 현재 공개 route의 한계, 관리자 Projection 경계와 UM0~UM9 migration 순서 확정
- 기존 `WorldReadRuntime`, 세 identity, `WorldGraphIndex`, `StableIdReconciler`를 마트에서도 재사용하고 View에 섞인 색·상자 수·상세 문구 판단을 Presentation Projector로 이동하는 기준 확정
- UM0~UM1 구현: `도심마트공개상품DataSnapshot`, Data Mapper·validator, operational Data Repository와 simulation Data Query 추가
- 공개 Data에 `OrdererPublic` audience, `ProjectedSaleAvailability` 수량 의미와 공개 안내를 강제하고 보관·진열·예약 재고 필드를 만들지 않음
- 기존 `도심마트ApiMapper.Map`, `Simulated도심마트조회UseCase`와 ScreenModel 소비자를 새 Data 경로 뒤 compatibility adapter로 유지
- UM2 구현: 공개 상품 World에는 상품 node만 두고 물리 재고·진열대를 만들지 않으며, 관리자 simulation은 상품·위치·재고·진열대·작업을 기존 `WorldGraphIndex`와 typed relation으로 구성
- UM3 구현: 목표 진열률·rule revision을 입력으로 보충 후보 수량, 입고 필요, 활성 작업 중복, 데이터 불충분과 server capability 차단 사유를 결정하는 순수 C# Interpreter 추가
- 진열 보충 결과는 `CanPreviewRequest` 후보일 뿐 Command나 canonical 재고 변경을 수행하지 않도록 고정
- UM3 계산을 재검토해 같은 상품의 다른 진열대 작업이 점유한 재고를 차감하지 못하는 초과 추천 위험과 단일 `SourceInventoryStableId`의 다중 위치 계획 한계를 확인
- UM4보다 UM3R을 먼저 두고 원천 재고별 `OnHand / Allocated / Available`, 작업 allocation, 다중 원천 SourcePlan과 integrity blocker를 구현하도록 설계 보강
- 관리자 Perspective는 무결성 검증 뒤 `UrgentActions / PendingActions / InProgress / DataAttention` 30초 queue를 만들며, 판매속도 Data 없이 `곧 품절`을 추론하지 않도록 결정
- UM3R-A 구현: `도심마트재고가용성WorldState`에서 원천 재고별 OnHand·모든 비종료 작업 Allocated·Available과 할당 task lineage를 계산
- 다른 진열대의 작업 점유량도 현재 후보에서 차감하고 할당 합이 원천 수량을 넘으면 `InventoryOversubscribed`로 preview를 차단
- UM3R-B 구현: 명시적 작업 allocation Data·typed World node와 legacy 단일 source 정규화 경로 추가
- 여러 후방 위치의 Available 수량을 deterministic SourcePlan으로 배분하고 합계가 후보 수량과 일치할 때만 preview 허용
- UM3R-C 검증: 다중 원천, 완료·해제 할당 제외, 명시적 allocation 우선, 수량 합계·단위·원천·중복 Stable ID 거부
- UM4 구현: `마트관리자PerspectiveInterpreter`가 무결성 오류, 진열 0·입고 검토, 완전한 보충 후보와 활성 작업을 4개 관리자 queue로 분류하고 같은 우선순위에서만 Stable ID tie-breaker 사용
- UM4 구현: priority reason·rule revision·source lineage, focus 관련 World와 허용 interaction intent를 보존하고 판매속도 없이 품절 예상시간·매출 영향 점수를 만들지 않음
- UM4 구현: 30초 ManagerSummary와 priority queue·shelf·task·detail·source-plan 독립 surface를 `도심마트PresentationProjector`에서 생성하고 새 surface 경로의 색·상자 수·문구 판단을 Projector가 소유; 기존 compatibility View 전환은 UM5로 유지
- UM5-B 구현: 기존 공개 상품 Controller/View는 compatibility 경로로 보존하고 별도 manager Controller·surface/shelf View·LifetimeScope가 stable-ID change set과 선택 focus를 적용
- UM5-B 구현: manager primitive builder는 별도 Scene을 대상으로 하며, 실제 Unity 프로젝트에는 sample과 VContainer 1.18.0을 가져와 compile·EditMode를 검증하되 builder 실행·Scene 저장은 하지 않음
- SC0 구현: 공급처·Offer·계약안, 수요 시나리오, synthetic 주문·주문 재고할당을 세 독립 Simulation snapshot으로 추가하고 각 DataRevision·source lineage·mode 경계를 보존
- SC0 검증: 운영 상태 위장, Offer/계약안 공급처·상품·통화·단위 불일치, 명시적 선택률·시장점유율·인구 basis 누락, 주문·allocation 중복·참조·단위·합계 오류를 거부
- SC1-A 구현: 감자·지역 협동조합·대형 도매·Simulation 현물시장의 명시적 가격·최소수량·빈도·lead time fixture와 10개 node·15개 relation 공급 graph 추가
- SC1-A 구현: `Provides / Targets / ProvidedBy / Covers / DerivedFrom`의 허용 node 종류를 검증하고 정·역방향 탐색과 deterministic 순서를 제공; 별도 Simulation Domain graph이며 Unity mapper는 아직 추가하지 않음
- SC1-B 구현: 공개 집계 인구·세대 basis, 잠재수요 band Interpretation과 명시적 상품 선택률·Simulation 점유율·주별 수요 범위를 가진 4주 Demand Scenario builder 추가
- SC1-B 검증: 억제·결측 인구를 0으로 대체하지 않고, 인구 basis 변경은 revision·lineage에는 반영하되 명시적 주별 assumption이 같으면 수요량을 자동 비례 변경하지 않음
- SC1-C 구현: 명시적 7일 주문 건수 pattern, 기대수요 기준 균등 분할과 seeded remainder, 주문 생성·기한 Tick, `BaseScenarioDemand` source를 가진 기본 방문 주문 stream 추가
- SC1-C 검증: 4주 56건·일별 2건과 주별 기대수요 합계 보존, same-seed 결정성, seed 변경 시 합계 불변, lineage·개인정보 부재·rule 오류를 집중 8건으로 고정
- RG1 구현: synthetic 공동주택 주문자 집단에 의향 67명·410kg, 확정 61명·385kg, 공동수령 후보와 대표의 사회적 context·canonical role·NPC/visit stable ID를 분리
- RG2 구현: `OrdererGroup → DemandRequest → Product/PickupPoint` 별도 typed graph와 relation semantic·dangling·duplicate 검증을 추가하고 공급 graph는 상품 stable ID로만 결합
- RG3 구현: 기본 방문·집단 의향·집단 확정을 독립 component와 lineage로 합성하고 `BaseScenarioDemand + GroupConfirmedDemand`만 hard demand로 고정; 전환율 계약은 추가하지 않음
- SC2 구현: 28 Tick 순수 C# Engine이 주문 생성·재고 1차 할당·납품/작업 capacity·재할당·기한 마감·폐기·결제를 deterministic 순서로 실행
- SC2 구현: 초기/입고 재고 보존, hard demand 충족/미충족 보존, 현금 비음수와 미지급금 분리, 운송비 포함 구매비용·공급처별 수량 비중·source lineage를 병렬 결과로 보존
- RG4 구현: 주민은 본인 참여·수령 조건만, 대표는 집계 의향/확정·문의·공동수령 조율만, 마트 관리자는 상품·집계·기간·공급 검토 queue만 받는 별도 Perspective 추가
- RG4 구현: 대표/마트 capability가 없으면 projection·action·dialogue를 제거하고, 문의 초안은 마트에 노출하지 않으며 dialogue의 Command effect를 항상 `None`으로 고정
- RG4-NPC-A 구현: 기존 route catalog에 `residential-group-representative-briefing`과 `market-group-representative-consultation`을 additive하게 추가하고 동일 NPC의 두 Zone snapshot을 `RepresentativeVisitStableId`로 연결
- RG4-NPC-A 구현: 방문 stage와 활성 leg를 검증하며 Simulation movement는 canonical task를 주장하지 않고 arrival action과 visit command effect를 Presentation-only로 고정
- SC3~SC5 headless 구현: 공급 공백·주문 미충족·미지급·작업 capacity·공급처 집중도·폐기를 독립 reason/evidence로 해석하고 자동 발주·계약 확정 Intent를 제외
- SC3~SC5 headless 구현: 주문 브리핑의 현재 재고·예정 입고·즉시 충족·작업 후 잠재 충족·기한 내 불가를 분리하고 관리 Preview·포트폴리오·현금·납품 surface 입력과 revision·lineage를 보존
- RG4-NPC-B 구현: `공동주택대표NpcView`가 semantic waypoint·NavMeshAgent·Animator와 대표 dialogue를 표시하되 도착 event를 외부 Command로 발행하지 않음
- SC5 Unity binding 구현: 서버 계산 `UrbanMarketSupplyManagementApiModel`을 Simulation mode·수요/충족 보존식·비음수 metric·공급처 중복 기준으로 검증하고 낮은 revision 적용을 차단
- Unity coordinator 구현: visit active movement와 inquiry가 일치하는 무효과 dialogue만 NPC/dialogue target에 적용하고 다른 NPC·ServerCommand 효과를 거부
- CC0 설계 완료: Unity 업무 학습을 `Concept / Status / Reason / Action` 네 카드로 분리하고 `Perspective → Projector → Deck PresentationModel → View → VisualSkinAdapter` 경계를 확정
- CC0 설계 완료: 첫 대표 NPC deck을 집단 상태·확정 수요·의향 수요·공동수령·공급 상태·부족 근거·공급 검토 행동 7장으로 고정하고 주민 개인정보와 capability 확대를 금지
- CC0 우선순위 재정렬: 공통 카드 계약·Projector와 대표 deck을 imported sample·Scene wiring보다 먼저 구현하고, Synty 등 외부 asset은 교체 가능한 skin으로 한정
- CC1 구현: 공통 deck/card/evidence/action·source lineage 계약과 `ConceptCardDeckProjector`를 추가하고 typed anchor, mode, source/presentation revision과 deterministic 순서를 보존
- CC1 구현: 역할 권한이 없으면 deck을 반환하지 않고 미승인 Intent는 Action Card에서 제거하며, 권한 필터로 카드가 사라지면 기존 선택도 제거
- CC1 검증: 중복 card stable ID, dangling evidence source, action kind·block reason 불일치와 빈 lineage를 거부하고 공통 계약에 주민 개인정보 필드가 없음을 고정
- CC2 구현: 대표 집단 수요 authorized aggregate와 공급 브리핑의 product·unit·mode·visit·source lineage를 검증하고 공통 Projector에 전달하는 `UrbanMarketResidentialGroupConceptCardAdapter` 추가
- CC2 구현: 대표 NPC anchor에서 집단 상태·확정 수요·의향 수요·공동수령·공급 상태·부족 근거·공급 검토 Action의 일곱 카드를 deterministic stable ID로 생성
- CC2 경계: 집단 확정 385kg, 전체 hard demand 2,105kg과 현재 공급 부족 75kg을 서로 계산하거나 덮어쓰지 않고 각각의 source card에 보존
- CC2 권한: 기존 Perspective의 `ReviewOrdererGroupDemand`, `PreviewSupplyPlan`, `CompareSupplyOffers`만 Preview Action으로 투영하고 `ConfirmAllMembers` 같은 미정의 권한은 노출하지 않음
- 도심마트 공급 계약 경영 Simulation 설계 확정: 기존 공급중개 canonical 원장을 중복하지 않고 UM5-B 기반 위에서 지역 인구→잠재수요→Demand Scenario→synthetic 주문을 포함한 SC0~SC7 playable을 우선하며 SC9 전 실제 주문·계약·발주·결제·입고 연결 금지
- RG0 설계 완료: 기존 `individual-demand → GroupPurchase → Order → GroupOrder`를 공동주택 주문자 집단의 authority로 고정하고, 자동집단 `Confirmed`가 아닌 유효 개별 주문 집계만 hard demand로 사용
- RG0 설계 완료: 공동주택 대표는 기존 `공동구매 대표` 역할과 `ManagementOfficeEntrusted` 운영주체 context를 사용하되 주민별 주문 권한은 얻지 않으며, 공동수령은 확정 fulfillment 뒤 `residential-pickup:{출고예정Id}`로 연결
- 대표 NPC 재설계: 주민자치 대표 등 사회적 label은 World Context로만 사용하고, 업무 권한은 기존 공동구매 역할에 유지하며, Unity에서는 공통 `NpcMovementSnapshot` 기반 `ResidentialGroupRepresentative` actor로 표현
- 대표 NPC 재설계: 주거공동체 briefing leg와 도심마트 consultation leg를 분리하고 `market.manager-desk` 도착·대화·Animator event가 문의·주문·계약 Command를 자동 실행하지 않도록 고정
- Scene Builder가 현재 수정 중인 Scene을 교체하기 전 저장 여부를 확인하고, batch mode에서는 dirty Scene 교체를 거부하도록 보강
- loading·initial error 상태에서 진열대의 `0 KRW` placeholder가 실제 데이터처럼 보이지 않도록 숨김 처리
- 상품 목록·항목 null과 `GeneratedAt`·`EvidenceAsOf` 기본값을 ScreenModel validation에 추가
- 동시 `InitializeAsync()` 호출이 하나의 in-flight Task를 공유하도록 중복 초기화 방지
- `물류차고`를 차량 중심 공간이 아닌 입고·분류·보관·출고·운송 인계 중심의 `도심 물류센터` Zone으로 정정
- 도심마트 다음의 객체별 vertical slice 순서를 전통시장·물류거점 → 공공데이터 정보대 → 커뮤니티 게시판 → 도심 물류센터 → 창고 → 운송 순으로 고정
- 전통시장·공개 물류거점 ScreenModel, simulated UseCase와 validator 구현
- `Pilot/Active` 공개 상태, 검증된 위치 정밀도, 출처·기준시각·revision을 표현하는 시장 건물·물류거점 View socket 구현
- 전통시장 건물, 물류거점, 입고·픽업 Dock과 상세 panel을 생성하는 PrimitiveSceneBuilder 구현
- VContainer 1.18.0을 Unity Presentation composition root로 채택
- 도심마트와 전통시장·물류거점 Controller의 simulation fallback `new`와 수동 `ConfigureView`를 제거하고 Zone `LifetimeScope`·`[Inject]` method injection으로 전환
- 농사로 작목기술 `mainCategoryList`를 출처·기준시각·경계가 보존된 typed `CropReferenceCategoryListResponse`로 변환하는 서버 UseCase·공개 API 구현
- Unity에 server DTO를 공유하지 않는 CropReference ApiModel·Mapper·Repository port·UseCase 구현
- 공유 World를 유지하면서 생산자·주문자·운송자 관점을 stable ID로 적용하는 Role Perspective ApiModel·Mapper·Repository·UseCase·applicator 구현
- 요청 역할과 서버 승인 역할·Zone 일치, 운영 Command 확인·canonical 재조회 경계를 headless test로 고정
- 인증된 기사의 현재 배정 운송을 주소·연락처·운임 없이 반환하는 도심 물류센터 `Transporter` Role Perspective API 구현
- 기존 기사 운송 상태전이 정책을 공용 조회 정책으로 추출하고 가능한 interaction만 projection하도록 연결
- Unity `RoleExperienceCoordinator`가 서버 조회 뒤 동일한 stable-ID Zone 대상에 역할 관점을 적용하도록 연결
- Zone별 semantic route와 운영·simulation 경계를 가진 NPC movement ApiModel·Mapper·applicator 구현
- 농장, 마트, 주거공동체, 전통시장, 도심 물류센터, 창고와 공공·협동 공간의 NPC route catalog 구현; 개인 공간은 자동 NPC 없음으로 고정
- `NavMeshAgent` 이동과 `Animator` 도착 행동만 수행하는 importable NPC Presentation socket sample 구현
- 인증된 기사의 현재 운송 상태를 물류센터 gate·loading bay·exit semantic route로 변환하는 operational NPC movement API 구현
- Unity NPC movement Repository·UseCase를 server API 경계까지 확장
- 도심 물류센터 Role target, interaction panel, waypoint와 운송자 NPC를 조립하는 VContainer primitive sample 구현
- Unity 6 batch compile, primitive scene 생성과 scene reload 배선 검증 완료
- 현재 구현 완료도와 제품 0.0 공개 순서를 분리한 [Unity World 구현 우선순위](../Architecture/UnityWorldImplementationPriority.md) 작성
- `운송원장.운송번호`와 `입고요청.운송의뢰Id`를 연결해 운송중·창고도착·입고완료를 투영하는 화물 인계 API 구현
- 운송 NPC와 창고 입고작업자 NPC를 같은 Dock에 집결시키고 입고완료 후 퇴장·보관 route로 분기하는 workflow 구현
- Unity 화물 인계 Mapper·Repository·UseCase·revision applicator와 World Zone NPC router·화물 View socket 구현
- Role Perspective·NPC movement·창고 화물 인계 API용 `UnityWebRequest` operational adapter 구현
- API base URL·timeout과 serialize되지 않는 runtime session token provider를 VContainer simulation/operational 분기에 연결
- server camelCase·ISO 시각 JSON 호환 test와 Unity 6 adapter compile·Scene token provider 배선 검증 완료
- 공개 세계지도 관측 API용 Unity ApiModel·Mapper·Repository·UseCase 구현
- layer·출처·기준시각·위치 정밀도·freshness·boundary를 보존하고 중복 ID·잘못된 좌표를 거부하도록 검증
- stable ID marker의 생성·갱신·제거와 InitialLoadError·RefreshError 마지막 성공 유지 coordinator 구현
- 공공데이터 정보관 simulated/operational HTTP client, Controller·View·VContainer와 PrimitiveSceneBuilder 구현
- Unity 6에서 Public Data Hall compile, scene 생성과 reload wiring 검증 완료
- 공개 게시판·게시글 요약·비식별 활동 신호·권한 적용 원장 요약을 결합하는 커뮤니티 시장 광장 server aggregate API 구현
- 광장 공개 계약에서 작성자 식별자·연락처·댓글 본문·원장 ID·담당자·실행 행동을 제외하도록 테스트로 고정
- Unity에 별도 Community Market Square ApiModel·Mapper·Repository·UseCase와 stable-ID 증분 reconcile 구현
- 최초 조회 실패는 빈 광장, 갱신 실패는 마지막 성공 Snapshot과 기존 World Item을 유지하도록 coordinator 구현
- simulated/operational HTTP client, VContainer, SceneController와 게시판·게시글·활동·원장 primitive View sample 구현
- Unity 6에서 Community Market Square compile, scene 생성과 reload wiring 검증 완료
- 기존 권한 필터가 적용된 재고·적재·피킹 UseCase를 결합하는 `WarehouseManager` 전용 창고 World Snapshot API 구현
- 작업자 이름·주문 참조·연락처·주소·계약·정산 정보를 제외하고 재고·작업·NPC semantic route만 전달하도록 계약 고정
- Unity Warehouse ApiModel·Mapper·Repository·UseCase, 작업·재고 참조 검증과 stable-ID reconcile 구현
- PutAway 작업은 대응 재고를, DockWorker·Picker NPC는 대응 작업을 참조하도록 validation 추가
- 팔레트·작업 표식, NavMeshAgent 기반 DockWorker·Picker socket과 VContainer Warehouse World sample 구현
- Unity 6에서 Warehouse World compile, primitive scene 생성과 reload wiring 검증 완료
- 기존 화물 인계 API의 `InTransit` 상태를 transport corridor와 TruckView로 투영하는 Unity core 구현
- 도심 물류센터 sample에 물류센터→창고 waypoint, NavMeshAgent truck과 cargo VisualRoot 배선
- 기존 마트 공개 상품 aggregate를 Unity operational ApiModel·Mapper·Repository·UseCase로 연결
- 도심마트 LifetimeScope의 simulation/operational 명시적 분기와 읽기 전용 주문자 상품 관점 구현
- 기존 하차 권한 필터를 재사용하는 개인정보 최소화 Residential Pickup server projection 구현
- 같은 공동수령 object를 주문자 `내 수령 상품`·운송자 `내 하차 대상`으로 표현하는 Unity sample 구현
- 소유자 경계를 가진 `농장` root와 `농장구획`·`재배작기`·`농업센서`·`농업센서관측`·`농장작업` canonical aggregate 및 EF migration 구현
- 공개 농사로 작물 기준 stable ID·source key와 실제 재배 생육 상태를 별도 필드로 유지
- 인증 생산자 본인 농장만 반환하고 위치·주소·연락처·소유자 ID를 제외하는 Farm Producer World API 구현
- 센서 원시값·단위·기준시각과 서버 판정 상태·규칙 revision·근거 card·한계를 함께 투영
- Unity Farm ApiModel·Mapper·Repository·UseCase와 FarmTile·Crop·Sensor stable-ID applicator 구현
- canonical 농장작업의 semantic waypoint를 사용하는 FarmWorker NavMeshAgent socket과 VContainer primitive sample 구현
- Controller 추가보다 현실 업무·권한·Snapshot·Scene object·NPC·interaction·재조회 폐루프를 깊게 연결하는 `Unity Zone 업무 심화 설계` 작성
- 심화 개발 순서를 창고 → 도심 물류센터 → 농장 → 도심마트 → 주거공동체 → 전통시장·물류거점 → 공공데이터 정보관 → 커뮤니티·시장 광장으로 정리
- P8 협동조합·공동원장 공간은 앞 Zone의 실제 공통 원장 요구가 확인된 뒤 만드는 조건부 단계로 정리
- Warehouse W1 위치 catalog 구현: semantic waypoint와 보관 위치 코드를 Scene socket으로 해석하고 빈 값·미지원 값은 `UnassignedArea`로 격리
- 명시적 Stable ID만 따라 재고→적재 작업→DockWorker NPC 및 역방향 선택 관계를 계산하고, SKU 일치만으로 피킹 작업을 재고에 임의 연결하지 않도록 고정
- 창고 primitive에 선택/관계 highlight와 DetailPanel을 추가하고 가용·예약 수량, 위치와 관계 수를 표시하되 재고 투영값을 물리 팔레트 수로 해석하지 않는 경계를 명시
- 기존 Public World Map의 point observation 기반을 유지하면서 행정구역별 인구·세대·실제 수요를 병렬 Snapshot으로 표현하는 [지역 인구·수요 World Layer 제안](../Architecture/RegionalPopulationDemandWorldLayerProposal.md) 작성
- 공공 잠재 수요 기반, 권한이 적용된 실제 운영 수요, 물류 접근성과 Simulation 파생값을 분리하고 소지역 억제·geometry version·개인정보 집계·비구속 후보지 경계를 설계
- Unity 읽기 흐름을 `Data → Interpretation → Presentation`으로 고정하고 Query/Command Application을 별도 조율 축으로 두는 [기준 아키텍처](../Architecture/UnityDataInterpretationPresentationArchitecture.md) 작성
- P0~P7의 ApiModel·Mapper·Repository·Interpreter 후보·Applicator·Controller·View를 세 층에 분류하고 Warehouse W1을 첫 점진 migration pilot으로 지정
- `Authorized Perspective`와 `Presentation Perspective`, `DataRevisionSet`·`InterpretationRevision`·`PresentationRevision`과 근거 lineage 경계를 설계
- DIP1 공통 계약 구현: source별 revision set, quality·limitation code, interpretation lineage와 deterministic interpretation/presentation revision 계산 추가
- DIP2 Warehouse W1 migration pilot 구현: `WarehouseDataMapper/Repository → WarehouseWorldInterpreter → WarehousePresenter → WarehouseWorldView`로 실행 경로 분리
- 기존 `WarehouseWorldMapper`, `IWarehouseWorldRepository`와 Query UseCase constructor는 호환 facade로 유지하고 route·JSON·stable ID·last-success refresh 의미를 보존
- `WarehouseLocationResolver`, `WarehouseRelationResolver`를 View 밖으로 이동하고 DetailText·socket·관계 highlight 입력을 `WarehousePresentationModel`에서 결정
- VContainer가 Data Repository·Interpreter·Query UseCase를 실제 조립하는 Unity EditMode test 추가
- Warehouse W2 server projection 구현: 기사·창고 관점이 공통 handoff builder를 사용하고 Warehouse authorized snapshot에 권한 필터된 `InboundHandoffs`를 additive contract로 포함
- 적재 작업과 handoff를 `inbound-task:{입고요청Id}`로 연결하고 주소·연락처·운임·주문 식별자 없이 차량·화물·Transporter·DockWorker 관계를 제공
- Warehouse inbound handoff Interpreter 구현: `Approach → InboundDock → StorageZone/VehicleExit` 상태별 공간 점유와 canonical relation 선택 강조 입력 생성
- Warehouse primitive에 Approach·StaffEntry·InspectionZone·VehicleExit 소켓과 Cargo·Vehicle visual state를 추가
- DIP3 공통 reconcile 구현: Unity type이 없는 `StableIdReconciler<T>`·policy·change set으로 stable-ID add/update/remove/unchanged, Data revision 역행 거부와 Presentation revision 동일 시 기존 instance 유지를 표준화
- 기존 WorldProjection·Warehouse·PublicData·Community Reconciler를 공통 계산기에 연결하되 공개 facade, feature change set과 initial/refresh last-success 정책은 유지
- DIP4 Role 분리: `AuthorizedRoleProjectionQuery`는 서버 승인 Snapshot만 조회하고 `RolePresentationPerspectiveCoordinator`는 Presenter output을 같은 World target에 적용하도록 실제 도심 물류센터 경로 전환
- DIP4 NPC·Transport 분리: semantic route와 corridor에 input/rule lineage를 추가하고 `NpcMovementPresentationModel`·`TruckMovementPresentationModel`을 Applicator의 새 입력으로 고정
- 기존 `RoleExperienceCoordinator`, Snapshot 기반 NPC·Truck target overload는 P0·P4 밖의 점진 migration 호환 경로로 유지하고 NPC 도착은 animation 입력일 뿐 Command가 아님을 테스트 구조로 유지
- DIP5 PublicData 분리: layer·metric·관측 source facts를 `PublicWorldMapDataSnapshot`에 보존하고 `PublicWorldMapInterpreter → PublicDataHallPresenter`가 공개 marker 의미·lineage·표현 입력을 생성
- DIP5 Community 분리: board·post·activity·ledger facts를 `CommunitySquareDataSnapshot`에 보존하고 `CommunitySquareWorldInterpreter → CommunitySquarePresenter`가 stable-ID World item·lineage·표현 입력을 생성
- PublicDataHall·CommunityMarketSquare sample Controller를 DataFlow coordinator 조회와 Presenter output 소비 방식으로 전환하고 기존 Mapper·Repository·LoadCoordinator는 호환 facade로 유지
- DIP5R 보완 설계: Data=사실, Interpretation=도메인 공유 WorldState, Presentation=Zone별 경험으로 경계를 강화하고 Source/World/Presentation identity, typed graph, Application Runtime status, surface별 reconcile을 확정
- 현재 구현의 visual metadata in Data, Community 평면 Item, Warehouse 단일 SourceStableId chain, identity 혼용과 LoadCoordinator 책임 결합을 명시적 migration 대상으로 기록
- DIP5R-1 구현: `SourceStableId`·`WorldStableId`·`PresentationStableId`와 source→world→presentation identity lineage 추가
- DIP5R-1 typed graph 구현: `WorldRelationKind`, `WorldRelation`, `IWorldNode`, `WorldGraphIndex<TNode>`로 중복·dangling 관계를 거부하고 outgoing/incoming index 제공
- DIP5R-2 Runtime 구현: `WorldReadRuntime`이 RefreshDataAsync·ReinterpretShared·ReinterpretPerspective·Reproject를 구분하고 모든 변환·diff 성공 뒤 last-success를 교체
- authorization scope 변경 시 이전 역할 Data·World·Presentation cache를 제거하고 동일 scope refresh 실패에서만 마지막 성공 표현을 유지하도록 고정
- `ZoneRuntimeStatus`의 loading/ready/refresh/reinterpret/reproject/error와 안전 오류 code를 World Presentation과 분리하고 `SelectionStateStore`가 scope 변경·대상 제거 시 선택을 해제하도록 구현
- Interpretation 재설계: `ISharedWorldInterpreter`와 `IPerspectiveInterpreter`를 분리하고 `SharedWorldState → PerspectiveWorldState → Presentation`을 Runtime 기본 경로로 전환
- `InterpretationPerspectiveContext`에 Role·Intent·Zone·Focus·Operational/Simulation mode를 명시하고 공통 재해석·관점 재해석·표현 재투영을 별도 실행 경로로 고정
- 유통단계 가격 해석 기준 보강: 품목·규격·단위·지역·시점 비교 가능성을 먼저 검증하고 비용 근거 없는 차이를 마진이 아닌 `단계간 가격차`로 제한
- DIP5R-3 PublicData pilot: 공개 Data를 역할 독립 `PublicWorldState`와 `PublicWorldPerspectiveState`로 해석하고 Marker·Legend·Heatmap·Detail을 독립 Presentation surface와 item revision으로 분리
- PublicData wire의 Color·MarkerShape는 호환 Data에만 보존하고 Shared World에서는 제거하며, `PublicDataHallVisualPolicy`가 표현 단계에서 색·형태를 결정하도록 전환
- `PublicDataHallSurfaceRuntimeCoordinator`와 VContainer operational/simulation scope 설정을 sample Controller에 배선하고 기존 DataFlow coordinator·Presenter·View overload는 호환 facade로 유지
- Warehouse selection pilot: 기존 `Kind`·`SourceStableId` 체인을 `WarehouseWorldGraphBuilder`의 `Targets`·`AssignedTo`·`Carries`·`HandoffTo`·`DerivedFrom` 관계로 변환하고 resolver는 typed graph index만 탐색
- Data Context 보강: 서버 승인 `UserSessionContext`·`WorldContext`·`DataAuthorizationContext`를 `WorldDataContext`로 묶고 Global/World/AuthorizedUser/AuthorizedUserWorld query scope를 명시
- `ContextScopedSnapshotCache`와 `WorldDataContextRuntime`이 session·World·authorization·mode 전환에 따라 private cache와 selection을 폐기하고 Global public cache는 유지하도록 구현
- `WorldObjectRef(WorldContextId, WorldStableId)`와 `IContextualWorldDataQuery`를 추가하고 `WorldReadRuntime`에 context-aware refresh overload를 병행 제공
- PublicDataHall Runtime을 `Global` DataScope의 첫 소비자로 전환하고 기존 문자열 authorization scope overload는 호환 facade로 유지
- 외부·공공 데이터 P0 조사에서 기존 `PublicDataApiMetadataCatalog`, server User Secrets, private Object Storage, archive·EF migration 패턴을 재사용 대상으로 분류
- P1 Source Catalog: 기존 API metadata를 API·다운로드·수동 import가 가능한 `ExternalDataSourceDefinition`으로 확장하고 중복·credential 계약 검증 추가
- P2 Credential: configuration/User Secrets 기반 provider, secret redaction과 source별 기본 비활성 collection policy 구현
- P3 Ingestion Runtime: timeout·caller cancellation·bounded retry, typed 오류, 성공·부분·실패·취소 Run과 simulation fallback 금지 구현
- P4 Raw 저장: private object storage, SHA-256·source version·기준/수집시각 metadata, 동일 hash 재정규화 방지와 전용 EF DbContext·migration 구현
- P5 Normalization: Ssalddel RegionStableId, metric·unit·as-of·precision·quality·limitation·dimension·source/data revision과 lineage validation 구현
- [외부·공공 데이터 서버 수집 기반](../Architecture/ExternalPublicDataServerIngestionFoundation.md)에 재사용/확장/신규 분류, 구현 경계와 P6 진입 조건 기록
- P6를 credential 없는 `P6-A 공급자 계약 조사`와 실제 raw·DB 검증인 `P6-B 연결`로 분리
- World Bank WDI `AG.LND.ARBL.HA`, FAOSTAT Land Use와 ISRIC SoilGrids를 공식 metadata 기반 Source Catalog에 등록하되 모두 기본 비활성 유지
- 국가·연간 농업 토지 `국가농업토지Data`와 coverage·CRS·250m grid·깊이·quantile·source mapped unit conversion을 보존하는 `지역토양Data` 계약 구현
- SoilGrids 12개 property 단위·conversion, 6개 깊이와 coverage statistic parser 구현; REST 중단 상태에서는 WCS/WebDAV만 후보로 고정
- World Bank KOR 경지면적 collector·normalizer fixture, explicit ISO3 region mapping과 temporal precision 구현
- [농업 토지·토양 공급자 계약 조사](../Architecture/AgriculturalExternalDataProviderContractResearch.md)에 provider별 확인·미확정 항목과 P6-B gate 기록
- P6-B World Bank 요청을 `KOR + mrv=1`로 bounded하고 raw 기준연도·source version을 보존
- opt-in 전용 live verifier로 실제 응답을 production Runtime, private local object storage와 SQLite Run→Raw→Normalized lineage에 통과
- live verifier가 test 0건을 성공으로 오인하지 않도록 ASCII Trait filter와 TRX executed/passed count 검증 추가

## 검증 상태

| 검증 | 상태 | 근거 또는 제한 |
| --- | --- | --- |
| External Data P1~P5 targeted tests | 15/15 통과 | 실제 기존 catalog 호환, credential redaction, opt-in policy, retry, 실패 경계, raw hash 멱등성, normalization·region·EF upsert |
| External Data scoped Fast / Task | 통과 | `Ssalddel.v3.5.slnx` build, targeted tests와 `git diff --check`; logs `20260808-222846`, `20260808-222955` |
| External Data EF migration model check | 통과 | `PublicDataIngestionDbContext`에 migration 이후 pending model change 없음 |
| Agricultural provider P6-A tests | 78/78 통과 | 최종 `Services.External.PublicData` 범위; P1~P5 회귀, World Bank fixture, FAOSTAT/SoilGrids metadata, SoilGrids unit/depth/coverage와 ISO3 region seed mapping; TRX `agricultural-provider-contract-p6a-final.trx` |
| Agricultural provider P6-A scoped Fast / Task | Fast 통과, Task 비관련 7건 실패 | Fast build·diff 통과 log `20260809-082609`; Task build 통과 후 4,463/4,470 통과, 기존 metadata·route·CSS 계열 7건 실패 log `20260809-082722` |
| SoilGrids WCS metadata live | 통과 | 2026-08-09 credential 없이 `phh2o` GetCapabilities HTTP 200·30 coverage, DescribeCoverage `EPSG:152160` 확인; coverage 본문 저장은 미실행 |
| External Data P6-B final tests | 80/80 통과 | 전체 External PublicData 회귀와 bounded `mrv=1`, 기준연도, 오류 mapping, KOR normalization, live opt-in 기본 비활성 포함; TRX `external-data-p6b-final.trx` |
| World Bank P6-B live lineage | 1/1 통과 | 2026-08-09 실제 2023년 `1,456,000 ha`, source version `wdi:2:lastupdated:2026-07-13`; private raw 물리 파일과 SQLite Run→Raw→Normalized FK 확인; 최종 TRX `world-bank-p6b-live-20260809-090032.trx` |
| World Bank P6-B scoped Fast / Task | 통과 | Fast server·test build와 targeted test log `20260809-090107`; Task `Ssalddel.v0.0.slnx` build와 targeted test log `20260809-090129` |
| 실제 외부 공급자 / 운영 DB / Unity runtime | 로컬 P6-B만 완료 | 실제 provider→임시 private local storage→테스트 SQLite는 검증; 운영 DB migration·운영 object storage·scheduler/admin endpoint·Unity API/runtime은 미실행 |
| `Ssalddel.Unity.Tests` | 140/140 통과 | 2026-08-08 headless .NET test; DIP5R Data Context·scope cache·contextual query, PublicData 독립 surface와 Warehouse typed selection 포함 |
| Farm Producer server tests | 7/7 통과 | canonical EF model, 소유자 필터, 작물 기준 분리, 최신 센서 판정, 개인정보 제외, NPC canonical task와 고정 인증 route |
| P7 scoped Fast / Task | 통과 | `Ssalddel.v3.5.slnx` build, 24개 관련 test filter와 `git diff --check`; logs `20260808-171402`, `20260808-171535` |
| Residential Pickup server tests | 6/6 통과 | 개인정보 최소화 projection 4건 + 역할 고정 route·인증 경계 2건 |
| 커뮤니티 시장 광장 server tests | 4/4 통과 | 공개 aggregate mapping·정보 최소화·고정 route·하위 조회 실패 전파 |
| 창고 World server tests | 4/4 통과 | 권한 조회 결합·정보 최소화·관리자 route·잘못된 창고 ID 차단 |
| 농사로·작물 서버 targeted tests | 9/9 통과 | Nongsaro module 6개 + CropReference typed projection 3개 |
| 도심 물류센터·창고 인계 server tests | 20/20 통과 | Role/NPC/화물 인계·JSON wire projection 18개 + 기존 운송 원장 상호작용 2개 |
| 관련 Unity core build | 통과 | scoped Fast validation |
| 문서 link·diff | 통과 | 상대 link 검사와 `git diff --check` |
| 전체 Task build | 통과 | `Ssalddel.v0.0.slnx` |
| 전체 Task tests | 비관련 실패 7건 | 4,432/4,439 통과; DIP3 영향 프로젝트 build와 104개 Unity test는 통과했으며, 별도 dirty 작업의 신규 역할 Controller metadata 3건과 기존 Web/API metadata·CSS 4건 실패 |
| Unity EditMode | 미실행 | 현재 요청은 코드·headless 계약 검증 범위 |
| Unity PlayMode | 미실행 | 현재 요청 범위에서 제외 |
| built player | 미실행 | Windows·Android runtime 미검증 |
| 실제 Unity Scene | 현재 체크아웃에서 미검증 | 사용자 보고 P2 runtime 소스 위치 확인 필요 |
| Urban Market sample compile | 통과 | 임시 Unity 6 project에서 operational HTTP adapter, VContainer 분기와 sample script compile 확인 |
| Urban Market scene wiring | 통과 | Editor builder 생성 후 별도 scene reload에서 View wiring·3상품 simulation fixture 확인 |
| Residential Pickup sample compile | 통과 | Unity 6 + VContainer에서 simulation/operational client와 역할 전환 sample compile 확인 |
| Residential Pickup scene wiring | 통과 | Editor builder 생성 후 scene reload에서 공동수령 View·역할 switch·runtime token provider wiring 확인 |
| Farm sample compile | 통과 | Unity 6 + VContainer에서 simulation/operational HTTP client, FarmTile·Crop·Sensor·FarmWorker assembly compile 확인 |
| Farm scene wiring | 통과 | Editor builder 생성 후 scene reload에서 View·Controller·LifetimeScope·producer NPC socket 확인 |
| Traditional Market Hub sample compile | 통과 | 임시 Unity 6 project에서 package sample import·script compile 확인 |
| Traditional Market Hub scene wiring | 통과 | Editor builder 생성 후 scene reload에서 View wiring·fixture 확인 |
| VContainer composition | 통과 | Unity 6 + VContainer 1.18.0에서 두 sample compile, LifetimeScope 포함 Scene 생성·reload 확인 |
| Urban Logistics Center sample compile | 통과 | Unity 6 + VContainer에서 Role/NPC/transport corridor sample assembly compile 확인 |
| Urban Logistics Center scene wiring | 통과 | Editor builder 생성 후 scene reload에서 Role target·waypoint·NPC·비활성 TruckView wiring 확인 |
| Cargo Warehouse Handoff sample compile | 통과 | Unity 6에서 World NPC router·화물 View socket compile 확인 |
| Operational World API adapter compile | 통과 | UnityWebRequest, cancellation, 404/error, runtime token과 VContainer 분기 compile 확인 |
| Public Data Hall sample compile | 통과 | Unity 6에서 simulated/operational client, Controller와 marker View compile 확인 |
| Public Data Hall scene wiring | 통과 | Editor builder 생성 후 scene reload에서 marker template·View·Controller·LifetimeScope 확인 |
| Community Market Square sample compile | 통과 | Unity 6에서 simulated/operational client, VContainer, Controller와 stable-ID Item View compile 확인 |
| Community Market Square scene wiring | 통과 | Editor builder 생성 후 scene reload에서 View template·Controller·LifetimeScope 확인 |
| Warehouse World sample compile | 통과 | Unity 6에서 위치 catalog, 선택/관계 highlight, DetailPanel, authenticated HTTP client와 VContainer compile 확인 |
| Warehouse World scene wiring | 통과 | Editor builder 생성 후 scene reload에서 UnassignedArea·DetailPanel·semantic waypoint·NavMeshAgent socket·Controller·LifetimeScope 확인 |
| Warehouse W1 operational refresh | 1/1 통과 | Unity 6 EditMode에서 실제 UnityWebRequest 최초 조회·동일 revision refresh·단절 후 RefreshError와 마지막 성공 snapshot 유지 확인 |
| Warehouse W1 scoped Fast / Task | Fast 통과, Task 비관련 실패 | 최종 Fast log `20260808-192758`; Task build 통과 후 기존과 동일한 비관련 7건 실패, log `20260808-192559` |
| DIP1~DIP2 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260808-200341`; Task에서 4,431/4,438 통과 후 기존과 동일한 metadata·CSS 7건 실패, log `20260808-200351` |
| DIP3 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Unity core 104/104와 Fast 통과, log `20260808-204026`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, log `20260808-204037` |
| DIP4 Unity package core compile | 통과 | 열린 Unity 6000.5.6f1 Editor가 신규 Role/NPC/Transport core 파일을 import하고 assembly reload 완료; `Samples~/UrbanLogisticsCenter` 별도 sample assembly·Scene reload는 미검증 |
| DIP4 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Unity core 107/107와 Fast 통과, log `20260808-205317`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, log `20260808-205332` |
| DIP5 Unity package core compile | 통과 | 열린 Unity 6000.5.6f1 Editor가 신규 PublicData/Community DataFlow·Presentation core 파일을 import하고 assembly reload 완료; 두 `Samples~` 별도 sample assembly·Scene reload는 미검증 |
| DIP5 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Unity core 111/111와 Fast 통과, log `20260808-210901`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, log `20260808-210914` |
| DIP5R-1 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | identity/typed graph 포함 Unity core 117/117와 Fast 통과, log `20260808-211852`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, log `20260808-211902` |
| DIP5R-2 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Shared/Perspective Runtime·Selection 포함 Unity core 126/126와 Fast 통과, 최종 log `20260808-214056`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, 최종 log `20260808-214106` |
| DIP5R-3 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | PublicData 독립 surface·Runtime adapter와 Warehouse typed selection 포함 Unity core 134/134, Fast log `20260808-215742`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, log `20260808-215754` |
| DIP5R Data Context scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Session·World·Authorization·DataScope·cache와 contextual query 포함 Unity core 140/140, Fast log `20260808-220758`; Task build 통과 후 4,432/4,439 통과와 기존 동일 metadata·CSS 7건 실패, log `20260808-220809` |
| Warehouse 3계층 Unity compile·scene | 통과 | Unity 6000.5.6f1 batch compile과 primitive scene 생성·reload wiring 확인 |
| Warehouse 3계층 VContainer EditMode | 1/1 통과 | Data Repository→Interpreter→Query UseCase 실제 resolve·조회 확인 |
| Warehouse W2 server targeted | 27/27 통과 | 운송자 handoff 18건 + 적재·Warehouse snapshot·권한 필터된 입고 handoff 9건 |
| Warehouse W2 Unity compile·scene | 통과 | Unity 6000.5.6f1에서 신규 4개 waypoint, Cargo·Vehicle·NPC socket 생성과 scene reload wiring 확인 |
| Warehouse W2 VContainer EditMode | 2/2 통과 | 기본 Data→Interpretation 조립과 simulated handoff의 차량·화물·Dock canonical relation 확인 |
| UrbanMarket 운영자 3계층 재설계 문서 | 통과 | 상대 link 확인과 docs-only Fast `git diff --check`; build·test·Unity runtime은 설계 작업 범위 밖이라 미실행, log `20260809-092712` |
| UrbanMarket UM0~UM1 targeted | 17/17 통과 | 새 Data 경계 6건과 기존 operational·simulation ScreenModel 회귀 포함; TRX `artifacts/local/validation/urban-market-um01/urban-market-um01.trx` |
| Unity core UM0~UM1 | 146/146 통과 | `Ssalddel.Unity` netstandard build와 전체 headless tests; TRX `artifacts/local/validation/urban-market-um01-full/unity-core-um01.trx` |
| UrbanMarket UM0~UM1 scoped Fast | 통과 | Unity core·test project build와 `git diff --check`; tests는 위 두 전용 실행으로 별도 검증, log `20260809-094320` |
| UrbanMarket UM0~UM3 targeted | 28/28 통과 | 공개/관리자 graph, dangling relation, deterministic revision, 진열 보충 후보·입고 필요·활성 작업·capability·불충분 data 포함; TRX `artifacts/local/validation/urban-market-um03/urban-market-um03.trx` |
| Unity core UM0~UM3 | 157/157 통과 | `Ssalddel.Unity` netstandard build와 전체 headless tests; TRX `artifacts/local/validation/urban-market-um03-full/unity-core-um03.trx` |
| UrbanMarket UM0~UM3 scoped Fast | 통과 | Unity core·test project build와 `git diff --check`; tests는 위 전용 실행으로 별도 검증, log `20260809-095316` |
| UrbanMarket UM3R 재설계 문서 | 통과 | 전역 allocation·다중 원천 SourcePlan·관리자 30초 queue 순서 보강; docs-only Fast에서 build·test 생략, `git diff --check` 통과, log `20260809-101425` |
| UrbanMarket UM3R-A targeted | 9/9 통과 | 다른 진열대 전역 할당 차감, 초과 할당 차단, 물리 재고 없음과 전량 할당 구분 포함; TRX `artifacts/local/validation/urban-market-um3r-a/urban-market-um3r-a.trx` |
| Unity core UM3R-A | 160/160 통과 | `Ssalddel.Unity` netstandard build와 전체 headless tests; TRX `artifacts/local/validation/urban-market-um3r-a-full/unity-core-um3r-a.trx` |
| UrbanMarket UM3R-A scoped Fast | 통과 | Unity core·test project build와 `git diff --check`; tests는 위 전용 실행으로 별도 검증, log `20260809-102503` |
| UrbanMarket UM3R-B/C targeted | 22/22 통과 | 다중 원천 SourcePlan, explicit-over-legacy allocation, 완료·해제 제외, 수량·단위·원천·중복 검증 포함; TRX `artifacts/local/validation/urban-market-um3r-bc/urban-market-um3r-bc.trx` |
| Unity core UM3R-B/C | 168/168 통과 | `Ssalddel.Unity` netstandard build와 전체 headless tests; TRX `artifacts/local/validation/urban-market-um3r-bc-full/unity-core-um3r-bc.trx` |
| UrbanMarket UM3R-B/C scoped Fast | 통과 | Unity core·test project build와 `git diff --check`; tests는 위 전용 실행으로 별도 검증, log `20260809-103624` |
| UrbanMarket UM4 targeted | 37/37 통과 | UM4 신규 9건과 UM0~UM3R Data·Shared·Replenishment 회귀 28건; TRX `artifacts/local/validation/urban-market-um4/urban-market-um4-targeted.trx` |
| Unity core UM4 | 177/177 통과 | `Ssalddel.Unity` netstandard build와 전체 headless tests; TRX `artifacts/local/validation/urban-market-um4-full/unity-core-um4.trx` |
| UrbanMarket UM4 scoped Fast | 통과 | Unity core·test project build와 `git diff --check`; tests는 위 전용 실행으로 별도 검증, log `20260809-104736` |
| Supply Management Simulation redesign docs | 통과 | 신규 설계, D-026, 공급중개·도심마트·Unity 우선순위 link와 `git diff --check`; docs-only Fast log `20260809-111651` |
| Separate Simulation server foundation | 통과 | 별도 Contracts·Domain·Server·Tests, session lineage·expected revision·멱등 Tick·운영/Unity assembly 비의존 11/11 및 전용 solution build |
| Separate Simulation server scoped Fast | 통과 | `git diff --check`와 v0.0 영향 build, log `20260809-112759`; 새 전용 solution build·11/11은 별도 직접 검증 |
| Separate Simulation server Task | 기존 비관련 실패 | v0.0 build 통과, 전체 4,472건 중 4,465 통과·기존 metadata/naming 7건 실패, log `20260809-112821` |
| UrbanMarket UM5-A targeted | 16/16 통과 | manager Runtime 7건과 공통 WorldReadRuntime 9건; context·surface change set·selection·last-success 회귀 |
| Unity core UM5-A | 183/183 통과 | `Ssalddel.Unity` 전체 headless test, TRX `artifacts/local/validation/urban-market-um5/unity-um5.trx` |
| Unity project package import UM5-A | 16/16 통과 | 실제 `C:\Users\user\ssalddel` local UPM import와 EditMode compile·test, XML `artifacts/urban-market-um5-editmode.xml` |
| UrbanMarket UM5-A scoped Fast | 통과 | Unity core·test project build와 `git diff --check`, log `20260809-114658` |
| UrbanMarket UM5-A Task | 기존 비관련 실패 | v0.0 build 통과, 전체 4,472건 중 4,465 통과·기존 metadata/naming 7건 실패, log `20260809-114710` |
| UrbanMarket demand·order redesign docs | 통과 | 수요·주문 기준 문서, D-028, 지역 인구 handoff와 SC0~SC7 재정렬; docs-only Fast log `20260809-115208`, 코드·runtime 구현 아님 |
| UrbanMarket UM5-B headless | 184/184 통과 | manager View 입력 계약을 포함한 `Ssalddel.Unity` 전체 회귀; TRX `artifacts/local/validation/urban-market-um5b/unity-um5b.trx` |
| 실제 Unity project UM5-B core/sample | 각 16/16 통과 | `C:\Users\user\ssalddel` local UPM core와 imported Urban Market sample compile·EditMode; VContainer 1.18.0 구성 누락 보완 |
| UrbanMarket UM5-B Scene·Game View | 미실행 | 별도 manager builder 코드는 추가했으나 Scene 생성·저장·PlayMode는 수행하지 않음 |
| UrbanMarket UM5-B scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260809-120408`; Task에서 v0.0 build 통과 후 4,465/4,472 통과와 기존 metadata/naming/CSS 7건 실패, log `20260809-120420` |
| Supply Management SC0 contracts | 집중 11/11, Simulation 전체 22/22 통과 | 독립 snapshot revision·lineage, 운영 분리, 공급·수요·주문·allocation 무결성; TRX `artifacts/local/validation/urban-market-sc0/sc0-all-final.trx` |
| Supply Management SC0 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260809-120850`; Task에서 v0.0 build 통과 후 4,465/4,472 통과와 기존 7건 실패, log `20260809-120901` |
| Supply Management SC1-A | 집중 7/7, Simulation 전체 29/29 통과 | 감자·3공급처 fixture, 10 node·15 relation, relation semantic·dangling·duplicate·결정성; TRX `artifacts/local/validation/urban-market-sc1a/sc1a-all.trx` |
| Supply Management SC1-A scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260809-121356`; Task에서 v0.0 build 통과 후 4,465/4,472 통과와 기존 7건 실패, log `20260809-121405` |
| Supply Management SC1-B | 집중 8/8, Simulation 전체 37/37 통과 | 공공 basis 최소 계약, 잠재수요 band, 명시적 4주 가정, 비례 추론 방지와 revision·lineage; TRX `artifacts/local/validation/urban-market-sc1b/sc1b-all.trx` |
| Supply Management SC1-B scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260809-121831`; Task에서 v0.0 build 통과 후 4,465/4,472 통과와 기존 7건 실패, log `20260809-121840` |
| Residential Orderer Group RG0 redesign | 문서 전용 Fast 통과 | 기존 공동구매·대표·공동수령·마트 상품·공급중개 ReuseMap, D-029와 RG/SC 재정렬; build/test/runtime 미실행, log `20260809-124003` |
| Residential Representative NPC redesign | 문서 전용 Fast 통과 | 사회적 context·canonical role·NPC Presentation 분리, D-030, RG1/RG4-NPC와 두 Zone route leg; code·Unity runtime 미구현, log `20260809-124625` |
| Supply Management SC1-C | 집중 8/8, Simulation 전체 45/45 통과 | 기본 방문 4주 56건, 일별 2건, 기대수요·seed·기한·lineage·privacy 경계; TRX `artifacts/local/validation/urban-market-sc1c/sc1c-all.trx` |
| Supply Management SC1-C scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260809-125430`; Task에서 v1.5 build 통과 후 4,465/4,472 통과와 기존 metadata/naming 7건 실패, log `20260809-125444` |
| Residential Orderer Group RG1 | 집중 8/8, 당시 Simulation 전체 53/53 통과 | synthetic 집단·대표 role/NPC identity·pickup 후보·privacy·무결성; TRX `artifacts/local/validation/urban-market-rg1/rg1-all.trx` |
| Residential Orderer Group RG2 | 집중 7/7, 당시 Simulation 전체 60/60 통과 | 별도 typed graph, relation semantic·dangling·duplicate·결정성 및 공급 graph 분리; TRX `artifacts/local/validation/urban-market-rg2/rg2-all.trx` |
| Residential Orderer Group RG3 | 집중 8/8, Simulation 전체 68/68 통과 | 기본 1,720kg·의향 410kg·확정 385kg 분리, hard demand 2,105kg, 단위/session/scenario/policy 검증; TRX `artifacts/local/validation/urban-market-rg3/rg3-all.trx` |
| SC1-C + RG1~RG3 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast에서 Simulation Contracts·Domain·Tests build와 diff 통과, log `20260809-130253`; Task에서 v1.5 build 통과 후 4,465/4,472 통과와 기존 metadata/naming 7건 실패, log `20260809-130320` |
| Supply Management SC2 | 집중 9/9, Simulation 전체 77/77 통과 | 28 Tick 순서, 집단 확정 주문, 재고·수요·현금 보존, 작업/storage capacity, 폐기, 운송비·공급처 비중, 결정성; TRX `artifacts/local/validation/urban-market-sc2/sc2-all-final.trx` |
| Supply Management SC2 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast에서 Simulation 3개 project build와 diff 통과, log `20260809-130803`; Task에서 v1.5 build 통과 후 4,465/4,472 통과와 기존 metadata/naming 7건 실패, log `20260809-130816` |
| Residential Orderer Group RG4 | 집중 11/11, Simulation 전체 88/88 통과 | 주민·대표·마트 관리자 projection, capability 제거, 문의 초안 비노출, privacy와 dialogue 무효과; TRX `artifacts/local/validation/urban-market-rg4/rg4-all.trx` |
| Residential Orderer Group RG4 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast에서 Simulation 3개 project build와 diff 통과, log `20260809-131437`; Task에서 v1.5 build 통과 후 4,465/4,472 통과와 기존 metadata/naming 7건 실패, log `20260809-131447` |
| Residential Representative RG4-NPC-A | 집중 8/8, Unity core 전체 192/192 통과 | 기존 route 유지, 두 Zone leg·동일 NPC visit·활성 stage·Simulation/canonical task·무효과 arrival 경계; TRX `artifacts/local/validation/urban-market-rg4-npc/unity-all.trx` |
| Supply Management SC3~SC5 headless | 집중 8/8, Simulation 전체 96/96 통과 | 독립 위험 근거, 비자동 Intent, 재고/입고 분리 브리핑, Preview·포트폴리오·현금·납품 surface; TRX `artifacts/local/validation/urban-market-sc3-sc5/simulation-all.trx` |
| RG4-NPC-A + SC3~SC5 scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast·Task에서 v1.5 build와 diff 통과, Fast log `20260809-132335`; Task 전체 4,465/4,472 통과와 기존 metadata/naming 7건 실패, log `20260809-132349` |
| RG4-NPC-B + SC5 Unity binding headless | 집중 6/6, Unity core 전체 198/198 통과 | Simulation fallback 금지, briefing 보존식, revision 역행, active movement/dialogue와 Command 무효과; TRX `artifacts/local/validation/urban-market-rg4-npcb/unity-all.trx` |
| 실제 Unity project RG4-NPC-B core compile | 통과 | Unity 6000.5.6f1이 local package 신규 core를 재임포트하고 `Exiting batchmode successfully now!`; log `artifacts/local/validation/urban-market-rg4-npcb/unity-compile.log`; package sample 신규 View의 기존 imported copy 갱신·Scene wiring은 미실행 |
| RG4-NPC-B + SC5 Unity binding scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast에서 Unity core/tests build와 diff 통과, log `20260809-133001`; Task v1.5 build 통과 후 4,465/4,472 통과와 기존 metadata/naming 7건 실패, log `20260809-133009` |
| Concept Card CC0 문서 재설계 | 통과 | 네 카드 문법, D-031, 도심마트·대표·구현 우선순위 연결과 docs-only Fast `git diff --check`; code·Unity runtime 미구현, log `20260809-135629` |
| Concept Card CC1 | 집중 9/9, Unity core 전체 207/207 통과 | 네 카드 계약, deterministic revision, mode·lineage, 권한 Action·selection 제거, 무결성·privacy contract; TRX `artifacts/local/validation/concept-card-cc1/` |
| 실제 Unity project CC1 core compile | 통과 | Unity 6000.5.6f1 local package 재임포트·script compile 뒤 `Exiting batchmode successfully now!`; log `artifacts/local/validation/concept-card-cc1/unity-compile.log`; Card View·Scene runtime은 미구현 |
| Concept Card CC1 scoped Fast | 통과 | Unity core/tests project build와 `git diff --check`; log `20260809-140444` |
| Concept Card CC2 | 관련 집중 25/25, Unity core 전체 217/217 통과 | 7-card 순서, 385/2,105/75 source 분리, reason evidence, 권한 Action, mode·visit·unit·privacy 경계; TRX `artifacts/local/validation/concept-card-cc2/` |
| 실제 Unity project CC2 core compile | 통과 | Unity 6000.5.6f1 local package 재임포트·script compile 뒤 `Exiting batchmode successfully now!`; log `artifacts/local/validation/concept-card-cc2/unity-compile.log`; Card View·Scene runtime은 미구현 |
| Concept Card CC2 scoped Fast | 통과 | Unity core/tests project build와 `git diff --check`; log `20260809-141802` |

## 현재 작업

운영 서버를 변경하거나 Unity core를 이동하지 않고 `Ssalddel.Simulation.Contracts`, `Ssalddel.Simulation.Domain`, `Ssalddel.Simulation.Server`를 별도 dependency island로 추가했다. 첫 API는 session 생성·조회·Tick 진행이며 client request·command 멱등성, expected revision, scenario data revision, seed와 rule revision을 보존한다. API는 기본 비활성이고 `SsalddelExecution:Mode=Simulation`에서만 시작한다. 현재 저장소는 process-local in-memory이므로 개발 fixture 외의 save 권위로 사용하지 않는다.

Unity의 P0~P7·DIP5R 공통 기반과 외부·공공 데이터 P6-B 결과는 유지한다. DIP6 도심마트 UM0~UM4에 이어 UM5에서 `도심마트ManagerRuntime`과 별도 manager surface sample을 추가했다. Runtime은 manager role이 승인된 `AuthorizedUserWorld` Data context만 받고, Data refresh 뒤 Shared World에 남아 있는 선택만 focus로 유지하며, summary·queue·shelf·task·source-plan·detail을 각각 stable-ID change set으로 반환한다. refresh 실패는 마지막 성공 Presentation과 선택을 유지하고 session·World·authorization 경계 변경은 둘 다 폐기한다. View는 Projector가 결정한 문구·색 token·상자 수만 적용한다.

RG1~RG4, RG4-NPC-A/B와 SC0~SC5는 집단 수요·대표 identity·4주 Engine·Perspective·두 Zone 방문·공급 위험/Presentation과 Unity mapper/applicator/View source까지 구현했다. CC1~CC2는 공통 Concept Card 계약·Projector와 대표 전용 7-card adapter까지 구현했지만 Card View는 아직 없다. 첫 Engine은 검수·입고·진열 판매가능 전환 작업을 하나의 명시적 Tick capacity로 합산하며, 별도 직원 task·진열 surface는 SC7에서 UM4와 연결한다. 신규 View는 package sample source에 있고 실제 project의 기존 imported sample과 Scene에는 아직 반영하지 않았다.

`도심마트: 계약이 진열대를 만든다`는 지역 기본 수요와 기존 같이 주문 집단 수요를 source별로 보존한다. 공공 인구·세대 사실은 잠재수요 Interpretation까지만 제공하고, 비구속 `GroupIntentDemand`는 공급 문의 신호로만 표시한다. 주민별 기존 개별 주문을 합산한 `GroupOrder`만 `GroupConfirmedDemand`가 된다. Operational 연결은 RG5~RG7+SC9에서 기존 authorized Projection·대표 capability·ResidentialPickup·공급중개 UseCase를 통과할 때만 수행한다.

## 다음 구현 후보

1. CC3 + RG4-NPC-C: ConceptCardView·skin, Unity imported sample 갱신과 대표 View·manager desk·NavMesh/Animator Scene wiring
2. SC6~SC7: Action Card confirm/tick 폐루프와 납품·재고·진열·UM4·대표 결과 전달
3. RG5~RG7 + SC9: 기존 공동구매 Projection·ResidentialPickup·공급 Command 운영 폐루프

## 미해결

- Farm canonical schema와 API contract는 추가됐지만 migration은 실제 DB에 적용하지 않았고 sensor ingestion·판정 rule 실행 경로와 운영 seed가 없음
- Role Perspective server aggregate는 기사/도심 물류센터·공동수령·생산자 농장까지 확장됐지만 협동조합과 다른 Zone은 미구현
- NPC route, Zone Scene 배선과 compile은 검증됐지만 실제 Unity NavMesh bake, Animator Controller와 이동 재생은 미검증
- Warehouse W1 operational API와 UnityWebRequest refresh는 검증됐지만 선택·highlight 클릭 상호작용의 실제 Game View 확인은 미검증
- 로컬 DB의 기존 계정 password hash가 현재 개발 설정과 일치하지 않고 기존 암호화 데이터의 Data Protection key도 현재 key ring에 없어 정상 개발 로그인과 startup seed는 별도 환경 정리가 필요함
- 사용자 보고 P2 runtime과 현재 `Ssalddel.Unity` core의 결합 방식 확인 필요
- Unity project를 monorepo에 둘지 별도 repository로 둘지 ADR 필요
- 실제 제품 Unity project의 전체 presentation assembly와 application composition root는 미확인이나, 도심마트 sample은 `C:\Users\user\ssalddel`에 import해 VContainer 조립과 EditMode compile을 확인함
- 도심마트 `api/v1/orderer/mart/products`는 주문자용 공개 판매 가능 수량만 제공하며 관리자용 진열대·위치별 재고·진열 보충 작업·직원 배정 canonical source와 authorized API는 없음
- 도심마트 UM5 manager Runtime·surface applicator·selection·last-success는 구현·compile됐지만 manager builder Scene 생성·Game View 상호작용은 미검증
- 도심마트 UM3R은 명시적 allocation과 다중 원천 SourcePlan을 구현했지만 operational server에는 canonical 진열대·위치별 재고·작업·allocation Projection이 아직 없음
- Simulation 서버 기반은 구현했지만 process-local in-memory store이며 인증·사용자별 session scope·영속 save/replay와 Unity HTTP repository는 미구현
- 공급 계약 경영 Simulation은 SC0~SC5와 RG1~RG4·RG4-NPC-A/B의 공급/집단 graph·Engine·Perspective·Presentation·NPC route/visit·Unity binding code까지 구현했으며 imported sample·Scene runtime wiring은 아직 미구현
- Concept Card는 CC2 대표 7-card adapter까지 구현했으며 View·visual skin과 Unity Scene runtime은 아직 미구현
- 공동주택 집단의 기존 서버 재사용 설계는 완료했지만 마트 관리자 privacy-safe 집계 Projection, 대표의 특정 집단 마트 문의 capability, group-order source revision Projection은 아직 없음
- 주민자치 대표의 두 route·visit state·`공동주택대표NpcView` source와 대화 coordinator는 구현했지만 기존 imported sample 갱신, Scene/NavMesh/Animator와 실제 Game View는 아직 미구현
- 현재 `ResidentialPickup`은 `residential-pickup:{출고예정Id}`와 canonical unloading task를 제공하지만 별도 pickup-point canonical reference는 직접 보존하지 않아 RG6 검토가 필요함
- 지역 인구·잠재수요·Demand Scenario→기본 Simulation 주문 handoff는 fixture로 구현했지만 실제 공공 Data provider와 운영 주문 Projection은 아직 없음
- 기존 플랫폼 공급중개 원장은 단가·최소/최대 발주수량과 문자열 정산·반품 조건을 보존하지만 첫 Playable의 납품 주기·lead time·결제 일수·품질 수명 canonical 필드는 없으므로 SC9 전까지 운영 계약에서 추정하지 않음
- Synty asset 미도입; 구매·license·URP·모바일 성능 미검증
- 전체 test suite의 비관련 실패 7건이 남아 있어 전체 green 상태는 아님
- 지역 인구·수요 Layer는 제안만 작성됐고 공급자 key·이용 조건·지역 geometry·운영 집계 policy·API·Unity 코드는 아직 미구현
- DIP3~DIP5 core는 headless와 열린 Unity Editor package import를 검증했지만 DIP4 `Samples~/UrbanLogisticsCenter`와 DIP5 `Samples~/PublicDataHall`·`Samples~/CommunityMarketSquare` 별도 sample assembly·Scene refresh는 재검증하지 않았다. DIP6 도심마트 UM0~UM5와 SC0는 headless 및 sample compile 범위까지 완료했으며 manager Scene·Game View와 SC1 이후는 미구현
- DIP5R identity·typed graph와 Shared/Perspective Runtime/Selection 계약은 구현했고 Warehouse selection과 PublicData sample은 새 adapter를 소비하지만 Community adapter와 계층 의존 architecture test는 아직 미구현
- PublicData Marker·Legend·Heatmap·Detail surface 계약과 sample Controller 코드는 구현했지만 Unity Editor sample assembly compile·Scene wiring·Game View는 이번 작업에서 미검증
- Data Context core와 PublicData Global scope pilot은 구현했지만 실제 로그인 bootstrap, 서버의 World authorization 확인 endpoint와 VContainer dynamic SessionScope·WorldScope composition은 아직 미구현
- 유통단계별 비교 가격은 해석 기준만 확정했으며 생산자 수취·도매·소매를 같은 규격·지역·기간으로 제공하는 server Data contract와 실제 Chart surface는 미구현
- DIP2 뒤 실제 운영 서버를 사용한 Warehouse W1 UnityWebRequest refresh test는 재실행하지 않았으며, 이번 검증은 기존 headless 회귀·Unity compile·scene wiring·VContainer 조립 범위임
- Warehouse W2의 실제 서버 기동은 현재 실행 정책에서 background process 시작이 차단되어 재검증하지 못했으며, 활성 handoff가 존재하는 운영 DB JSON과 Game View 점유는 미확인
- World Bank P6-B는 임시 private local storage와 테스트 SQLite까지만 확인했으며 운영 DB migration, 운영 object storage와 scheduler/admin 실행 진입점은 미연결
- 전용 PublicDataIngestion migration은 생성하고 startup 초기화에 연결했지만 실제 운영 DB에는 적용하지 않음
- raw hash 확인 전에 private object upload가 수행되므로 DB 정규화 중복은 막지만 동일 본문의 물리 object 정리는 대용량 공급자 연결 전 보완 후보임
- OAuth token 교환·갱신, GeoTIFF/대형 raster streaming parser와 spatial storage는 실제 P6 공급자 요구가 확인될 때 구현함
- P6-A에서 FAOSTAT의 연간 범위·license·Land Use 의미는 확인했지만 exact bulk URL·CSV header·code/flag/null 표기는 P6-B sample 전까지 미확정
- SoilGrids WCS metadata는 live 확인했지만 GetCoverage GeoTIFF 저장·bounding box 제한·nodata·scale/offset과 raster revision은 아직 검증하지 않음
- World Bank 전 연도 조회는 불안정했으므로 `mrv=1`만 검증했으며, P7 세 국가 동시 요청의 응답 안정성과 비교 가능성은 아직 확인하지 않음

## 다음 작업 종료 시 갱신할 항목

- 현재 목표와 현재 작업
- 최근 완료 중 여전히 인계에 필요한 항목
- 실행한 build·test·runtime 검증과 정확한 결과
- 새로 확인되거나 해소된 미해결 항목
- 다음 구현 후보의 순서

결정을 변경한 작업이라면 이 파일만 수정하지 말고 [DECISIONS.md](DECISIONS.md)에 대체 결정을 함께 기록한다.
