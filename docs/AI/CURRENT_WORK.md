# Ssalddel Current Work

> GPT Chat과 Codex가 다음 작업을 이어받기 위한 최신 snapshot이다. 완료 이력을 계속 쌓는 일지가 아니며, 사실이 바뀌면 기존 항목을 현재 상태로 갱신한다. 장기 결정은 [DECISIONS.md](DECISIONS.md), 전체 맥락은 [공용 프로젝트 컨텍스트](../ProjectOverview/GptProjectContext.md)를 따른다.

## Snapshot

- 기준일: 2026-08-10
- 현재 작업 축: CP0 Showcase 기준선·CP1 공통 계약·CMP2 실측·CMP3 도로/Gate·CMP4 최소 거점·CMP4-A 공용 animation·CMP5 세 Region/Hub Journey·ART1 도로변·지형 굴곡 1차·NMR4 야간 약탈 Presentation Slice·ART0~ART4 Farm Hero·TOD0~TOD1 시간 Presentation 구현 → 지형 blending/모바일 profiling 또는 TCS1 24개 contact sheet
- 제품 공개 기본값: 0.0 커뮤니티·공공데이터
- Unity 개발 범위: 제품 버전 순서에 종속되지 않는 전체 Ssalddel 도메인

## 현재 목표

기존 `Ssalddel` 운영 서버와 별도 Simulation 경계의 권위를 유지한 채 CP0에서 저장된 Showcase와 Farm 24개 Composition이 남아 있음을 확인하고 테스트·캡처·profiling 기준선을 복구했다. CP1에서는 기존 Farm asset을 다시 저장하지 않고 공통 Composition descriptor·connector·socket·pack/source/detail/journey code, A/B/C signature validator와 Farm adapter를 additive로 구현했다. ANIM0 inventory는 실제 Synty clip/controller 부재, Humanoid rig 5개, Town missing controller 8개와 FX 수를 코드로 검출한다. CMP2에서는 세 Pack source 42개의 실제 좌표와 5m grid·Farm adapter offset을 고정했고, CMP3에서는 도로 12개와 Region/Hub Gate 10개를 A형 prefab으로 생성해 사람·차량·농기계·화물 connector를 분리했다. CMP4에서는 실제 감자 6×6 필지·타운 기본주택·시티 공동주택 가로형·지역 물류허브 Dock A형 각 1종을 만들고 CMP3 connector에 연결했다. CMP4-A/ANIM1에서는 공용 Idle/Walk 계약·catalog·adapter를 추가했고 ANIM2는 세 Pack 대표 actor의 route 이동과 procedural fallback 기준선까지만 닫았다. CMP5에서는 세 Region·Hub와 사람 Journey 2개, Farm·Town origin 화물 2개를 조립했다. 기존 감자 cargo는 Hub 보관에서 멈추고 명시적 allocation이 있는 Town sample cargo만 City outbound로 움직인다. ART0~ART3 1차 미술 패스에서는 Region별 색면, 연속 도로, 환경 군집, 공통 태양·ambient와 overview 카메라를 적용하고 가는 `DataRoute_*`와 숨긴 World Text로 데이터 표현을 정리했다. 세 Region 간격 패스에서는 기존 X/Z 좌표를 6.8배 확장해 Farm–Town과 Hub–City를 약 286~292m, Hub 연결 구간을 약 342~362m로 벌리고, 구간 성격에 맞춘 풍차·우물·급수탑·버스 정류장·벤치·가로등·물류 Station 등 18개 이상 조형물과 World/Region 초점 카메라를 추가했다. Farm Hero Slice는 기존 multi-region Showcase를 바꾸지 않고 Farm 전용 Scene에 97개 Presentation wrapper, 농장 초기 포커스, 따뜻한 soft-shadow·fog·절제된 후처리를 적용했다. ART4에서는 59개 작물·해바라기에 위상 분산 저진폭 흔들림을 적용하고 작업 트랙터 한 대를 짧은 Presentation 경로로 순환시켰다. TOD0~TOD1에서는 Dawn·Morning·Midday·Afternoon·GoldenDusk·Night 여섯 미술 앵커와 자정 연속 보간을 추가하고 태양·soft shadow·Ambient·Fog·카메라 배경·surface tint/brightness를 Presentation으로 적용했다. TCS0는 Farm 8 family×A/B/C 24개 catalog entry의 시간 반응 renderer/material slot과 semantic surface 종류를 측정한다. 같은 Game View의 여섯 시간대 PNG와 집중 EditMode 12/12, Farm/카메라 회귀 8/8 통과를 남겼다. 실제 Synty clip 리타기팅, 24개 contact sheet, Town/City 표면 inventory, 야간 emissive, 데이터 상태 미술과 모바일 profiling은 아직 완료되지 않았다. 환경 object와 NPC·차량·animation·시간 Presenter는 업무 stable ID나 상태 확정 권위를 소유하지 않는다.

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
- [Unity 외부 Reference Pattern 선별 도입 제안](../Architecture/UnityExternalReferencePatternAdoptionProposal.md)에 다섯 외부 프로젝트의 채택·비채택 범위, `SyntyCharacterVisualAdapter`·`NpcMovementPresenter`·`SyntyVehicleVisualAdapter`, spatial mapping, source/license inventory와 snapshot-driven Sandbox Gate를 정의
- [Unity World Visual Art Direction·ART Pass 제안](../Architecture/UnityVisualArtDirectionAndPassProposal.md)에 현재 CMP5 Game View의 시각 한계, `ART0~ART7` 순서, Region별 구도·색·조명·분위기·움직임·데이터 미술·카메라/성능 Gate와 같은 카메라 Before/After 증거 기준을 정의
- [Unity World 시간대별 미술·색감 전환 제안](../Architecture/UnityWorldTimeOfDayVisualDirectionProposal.md)에 Dawn·Day·GoldenHour·Night 네 앵커, 연속 보간, 고정·preview·Simulation·운영 관측 시간 source 경계, Region별 인공조명, 데이터 색 보호, 모바일 비용과 `TOD0~TOD6` 구현·검증 Gate를 정의
- [Unity City·Town·Farm Composition Set 시간 순차 시각 변화 제안](../Architecture/UnityCompositionSetTimeSequenceVisualProposal.md)에 현재 구현된 Farm 8종×A/B/C 24개를 첫 기준으로 여섯 시각의 그림자→밝기→표면·텍스처 반응→emissive 적용 순서, semantic surface binding, 원본 material 비수정, contact sheet와 `TCS0~TCS6` 확장 Gate를 정의
- Unity `FarmHeroShowcase`에 TOD0~TOD1을 구현해 여섯 시간 앵커와 연속 보간, 태양·그림자·Ambient·Fog·카메라 배경·비파괴 `MaterialPropertyBlock` 표면 반응을 적용하고 같은 Game View의 시간대별 PNG 6장을 남김. TCS0에서는 Farm 24개 Composition의 시간 반응 material slot과 semantic surface inventory를 측정했으며 집중 EditMode 12/12를 통과함
- Unity `ThreeRegionHubJourney`에 ART0~ART3 1차 미술 패스를 적용해 Farm·Town·Hub·City 색면, 연속 도로 5개, Synty 환경 군집, 따뜻한 soft-shadow 태양과 top-down 카메라를 표준화하고 1600×900 결과 PNG 및 전용 EditMode 10/10 검증을 남김
- Unity `ThreeRegionHubJourney`의 X/Z 배치를 6.8배 확장해 주요 인접 구간을 약 286~292m로 벌리고, Farm–Town·Farm–Hub·Town–Hub·Town–City·Hub–City 성격에 맞춘 Synty 조형물 18개 이상과 World/Region 카메라 초점을 추가함. 전체/Hub Play Mode PNG와 전용·카메라 EditMode 15/15 검증을 남김
- [Unity 야간 몬스터 화물트럭 약탈 Simulation 제안](../Architecture/UnityNightMonsterCargoRaidSimulationProposal.md)의 첫 vertical slice를 `ThreeRegionHubJourney`에 구현함. 5개 도로변에 독립 주택 10채·수목 View 30개 이상, Farm/Town/Hub/City 성격의 로우폴리 지형 굴곡 14개를 추가함. Hub–City 도로에서 19:45 blue-hour에 Synty skeleton 3명이 출모하고 2명이 상자를 운반한다. `SourceMode=Simulation`이며 약탈 전후 화물 stage·source lineage는 불변이다. 전용 EditMode 13/13과 terrain/roadside/day/night Play Mode PNG 3장을 남겼으며, 실제 화물 결과·운영 신고·배차·결제는 여전히 미구현·비활성이다
- Unity `FarmHeroShowcase`를 별도 Scene으로 추가해 농가·필지·사일로·도로·수목을 Farm 전용 3/4 구도로 재구성하고, 97개 Presentation wrapper와 따뜻한 조명·절제된 fog/후처리를 적용함. ART4에서 59개 작물·해바라기 저진폭 흔들림과 작업 트랙터 Presentation 경로를 추가하고 1600×900 Play Mode PNG 및 전용·회귀 EditMode 12/12 검증을 남김
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
- 관리자 Perspective는 모든 진열 상태의 `NeedCode`·차단 사유·허용 interaction·SourcePlan을 보존하며, 근거 Data가 없는 업무 우선순위 점수나 queue와 `곧 품절` 추론을 만들지 않도록 결정
- UM3R-A 구현: `도심마트재고가용성WorldState`에서 원천 재고별 OnHand·모든 비종료 작업 Allocated·Available과 할당 task lineage를 계산
- 다른 진열대의 작업 점유량도 현재 후보에서 차감하고 할당 합이 원천 수량을 넘으면 `InventoryOversubscribed`로 preview를 차단
- UM3R-B 구현: 명시적 작업 allocation Data·typed World node와 legacy 단일 source 정규화 경로 추가
- 여러 후방 위치의 Available 수량을 deterministic SourcePlan으로 배분하고 합계가 후보 수량과 일치할 때만 preview 허용
- UM3R-C 검증: 다중 원천, 완료·해제 할당 제외, 명시적 allocation 우선, 수량 합계·단위·원천·중복 Stable ID 거부
- UM4 재정비: `마트관리자PerspectiveInterpreter`가 모든 진열 상태를 Stable ID 결정적 순서로 보존하고 `NeedCode`, 차단 사유, 허용 interaction, rule revision·source lineage와 focus 관계를 전달
- UM4 재정비: `PriorityScore`, priority reason, 30초 ManagerSummary와 priority queue를 제거하고 shelf·task·detail·source-plan 독립 surface만 `도심마트PresentationProjector`에서 생성
- UM4 재정비: Stable ID 순서는 업무 우선순위가 아니며 판매속도·기한·SLA 같은 authoritative Data가 생기기 전에는 품절 예상시간·매출 영향·긴급도를 추론하지 않음
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

- Synty City Pack을 실제 Unity 프로젝트 원본 폴더에 import하고, 전용 builder가 canonical primitive sample 뒤 `VisualRoot`만 교체하도록 구성
- 도심마트 저장 Scene에 상점·공동주택·대표·관리자·책상·진열대를, 물류센터 저장 Scene에 시설·차량·pallet·상자를 배치하고 두 builder validation·Game View·Play Mode를 확인
- City Pack 캐릭터의 Humanoid Avatar와 `Synty/Generic_Basic` URP shader를 검증하고, Play Mode에서 발견한 마트 `PresentationContext` VContainer 등록 누락을 보완
- City Pack에는 농장 토양·작물과 실제 AnimationClip·AnimatorController가 없음을 확인해 FARM-2~FARM-5와 walk/work animation은 별도 asset-neutral 후속으로 유지
- Unity Scene·prefab·material·camera·UI 변경은 최종 Game View PNG를 다시 캡처하고 관련 코드·Scene·변경 기록과 같은 맥락의 커밋에 포함하도록 시각 증거 원칙을 보강
- [입체 탑다운 City·Farm World 구성 제안](../Architecture/UnityCityFarmPackWorldCompositionProposal.md)을 POLYGON Farm Showcase 네 장의 목표 화면에 맞춰 개정: 살아 있는 농촌 경관, 전경·핵심 공간·중경·배경, 환경 농지 안의 실제 감자 6×6, Produce Stand/Farm Yard, Semi-Urban Transition과 강화된 WORLD-2 시각 완료 기준을 명시하고 기존 VisualRoot·stable ID·Simulation 권위 경계는 유지
- product Unity project에 감자밭 두렁·혼합 작물밭·헛간 작업마당·농기계 대기장·농산물 직판장·수확물 집하장·농로 교차로·수목 완충지 8종×A/B/C의 `농장풍경CompositionSet` 24개를 생성하고, 실제 Synty nested prefab·footprint·한국어 세트명·상태 socket을 catalog와 저장 preview Scene으로 연결
- [Unity World 구현 현황과 우선순위](../Architecture/UnityWorldImplementationPriority.md)의 현재 실행열을 P0 사전 조사부터 P6 WORLD-5 증거 Gate, P7 FARM-2 폐루프와 후속 P8~P9로 분리하고 각 단계의 재사용 대상과 완료 Gate를 고정
- [City·Farm World P0 기준선과 Asset Inventory](../Architecture/UnityCityFarmWorldP0Inventory.md)에 실제 Unity 6000.5.6f1 project·열린 물류센터 Scene·Farm 498/City 335 prefab·최소 allowlist·PC/Mobile URP와 PC SSAO Renderer Feature·Console 기준선을 기록
- [POLYGON Farm 식품 Asset·HS·가격 연결 조사](../Architecture/UnityPolygonFarmFoodAssetHsPriceCrosswalk.md)에 식별 가능한 29개 식품 품목군·177개 prefab을 현재 `FoodPriceCrosswalkCatalog`와 대조하고, 직접 10개·대표가격 2개·추가 판정 17개 및 가격 조사 품목의 시각 공백을 분리 기록; Unity·서버 구현은 변경하지 않음
- [Unity Farm 상품·가격 카드 상호작용 흐름](../Architecture/UnityFarmProductPriceCardInteractionFlow.md)에 배치 Object 선택→상품 stable ID→HS mapping→국내/국가별 가격 조회→Concept Card Deck 흐름, direct·representative·candidate gate, loading·partial·stale·오류 상태와 FPC0~FPC3 구현 Gate를 정의; 현재 Farm 가격 adapter·Scene wiring은 미구현
- [POLYGON City 반복 배치 Composition Set 조사](../Architecture/UnityPolygonCityCompositionSetResearch.md)에 실제 335개 prefab을 건물 76·환경 65·소품 174·차량 9·캐릭터 9·FX 2개로 분류하고, 첫 12종×A/B/C와 후속 6종의 한국어 세트명·source family·socket·반복 규칙·Farm→City 연결·구현 전 Gate를 정의; Unity 구현은 수행하지 않음
- [City 주거단지·십자형 도로 Modular Composition 설계](../Architecture/UnityCityResidentialRoadModularCompositionDesign.md)에 실제 5m Road·Sidewalk cell과 약 5m Apartment module을 기준으로 도로 6종·공동주택 8종×A/B/C, connector graph, 십자형 생활권·저층 주거 가로·공동수령 recipe와 RR0~RR4 Gate를 정의; City Pack에는 단독주택 family가 없음을 명시하고 구현은 수행하지 않음
- [Farm 시설하우스·밭·논 단지 Modular Composition 설계](../Architecture/UnityFarmGreenhouseFieldPaddyModularCompositionDesign.md)에 실제 Greenhouse 2종·Dirt Row·Vege Row·농로 9종·관수 asset을 근거로 농로 6종·시설하우스 6종·밭 8종과 단지 recipe·socket·GF0~GF5 Gate를 정의; Rice·담수면·논둑·농수로 전용 asset이 없어 논은 Blockout으로 분리하고 구현은 수행하지 않음
- 실제 product Unity project에 import된 POLYGON Town 1.9.1의 702 prefab·25 material을 확인하고 [Town 반복 배치 Composition Set 조사](../Architecture/UnityPolygonTownCompositionSetResearch.md)에 5m 도로 grid, 도로 6종·생활권 12종×A/B/C, House interior 후보·privacy·TOWN0~TOWN4 Gate를 정의; Unity 구현은 수행하지 않음
- [Farm·Town·City 혼합 Composition 조화 설계](../Architecture/UnityFarmTownCityCompositionHarmonyDesign.md)에 Farm 생산·Town 저밀도 생활·City 고밀도 유통 책임, Farm↔Town 6종·Town↔City 6종·세 Pack 관통 4개 recipe, dominant/support/accent·도로·높이·palette·cargo·NPC·MIX0~MIX4 Gate를 정의; 혼합 prefab·Scene은 미구현
- [Farm·Town·City Composition 통합 구현 순서](../Architecture/UnityCompositionSetIntegratedImplementationSequence.md)에 이미 구현된 WORLD-0~5·FARM-2·농장풍경 24개와 문서 상태의 Composition·가격 Card를 구분하고, CP0 기준선 복구→CP1 공통 계약·실측·도로/Gate→CP2 최소 Region·Hub·공용 locomotion·Journey→CP3 감자 가격 카드와 FARM-3→CP4 검증된 subset 확장→CP5 논·Interior·최종 품질 순서를 고정; 이번 작업은 문서화만 수행
- [Farm·Town·City 3개 독립 Region Map 구성 설계](../Architecture/UnityFarmTownCityThreeRegionMapLayoutDesign.md)에 Farm 북서·City 북동·Town 남서와 Town·City 사이 Regional Logistics Hub, 세 Region의 독립 확장 root·Gate, Farm↔Town·Town↔City 사람 route와 Farm/Town→Hub→City 화물 route, Stateful Journey와 Ambient Traffic 분리, Follow와 Scene 분리 Gate를 정의; 구현은 수행하지 않음
- [Farm·Town·City 지역 물류허브 Map·Flow 설계](../Architecture/UnityFarmTownCityRegionalLogisticsHubDesign.md)에 여러 Farm·Town origin의 inbound Gate·Dock·검수·보관·분류·outbound allocation, Hub→City 재출하, passenger/freight 동선 분리와 직송 예외 Gate를 정의하고 D-034로 고정; 구현은 수행하지 않음
- [Synty Animation·FX 재사용과 리타기팅 설계](../Architecture/UnitySyntyAnimationReuseAndRetargetDesign.md)에 실제 Synty `.anim`·controller·embedded clip 부재, 세 Pack Humanoid rig, Town character 8개의 missing controller 참조와 Farm 11·City 2·Generic 17 ParticleSystem을 기록하고 `SyntyProvided→Retargeted→Procedural→Fallback`, ANIM0~ANIM6과 CMP4-A 위치를 정의; Unity 구현은 수행하지 않음
- WORLD-0 구현: product Unity project에 asset-neutral `DioramaCameraStateMachine`, `DioramaTopDownCameraRig`, World/Zone/Object focus, pan·zoom·90도 회전과 명시적 foreground cutaway, 저장하지 않는 primitive builder와 EditMode test를 추가
- WORLD-0 runtime 확인: unsaved prototype에서 Overview/Farm/Logistics/Market focus와 Market 90도 회전을 캡처한 뒤 기존 물류센터 Scene으로 복귀; 제품 Scene·vendor asset·URP 설정은 저장·수정하지 않음
- WORLD-1 구현: product Unity project의 별도 `CityFarmMacroWorldBlockout` Scene에 Farm Production·Farm Yard·Transport·Logistics·Market·Residential 6개 Presentation Zone, 5개 route와 World/Zone focus anchor를 저장하고 canonical Zone code와 Presentation subzone code를 분리
- WORLD-1 저장 보정: Unity 직렬화를 위해 Zone/Route `MonoBehaviour`를 타입명과 일치하는 파일로 분리하고, 저장 Scene 재로드 test에서 6개 Zone·5개 route·camera reference를 검증
- WORLD-1 Game View: 전용 primitive blockout material로 Text 없는 생산→수령 흐름과 Farm 6×6·Logistics cutaway·Market·Residential occlusion을 확인하고 [시각 변경 기록](../Changes/2026-08-09-unity-city-farm-world-blockout.md)에 Overview/Farm/Logistics/Market PNG를 보존
- WORLD-2 Catalog: vendor 이름을 포함하지 않는 Farm·Urban·Transition VisualKey 21개와 prefab·position/rotation/scale 보정만 보유하는 `WorldVisualCatalog`, `WorldVisualInstanceView/VisualRoot` wrapper를 추가
- WORLD-2 Synty Scene: WORLD-1을 보존한 별도 `CityFarmSyntyWorldPrototype` Scene에 Dirt Row·감자 S/M/L·Barn·Silo·Farmer·Tractor·Potato Box와 City Station·Shop·Apartment·Van·Pallet·Box·Shelf·Desk·road를 실제 prefab reference로 연결
- WORLD-2 URP 경계: 기존 PC/Mobile RP Asset과 Renderer/SSAO를 변경하지 않고 Color Adjustments·Neutral Tonemapping·낮은 Bloom만 가진 전용 Global Volume profile과 camera post-processing을 Scene에 연결
- WORLD-2 Game View: 공통 3/4 Perspective와 조명 아래 Overview/Farm/Logistics/Market을 재캡처하고 [시각 변경 기록](../Changes/2026-08-09-unity-city-farm-world-2.md)에 보존
- WORLD-3 기존 View 연결: 별도 `CityFarmBusinessViewIntegration` Scene에 기존 Farm 36 Tile·Logistics facility·Urban Market shelf/Concept Card·Residential pickup View를 연결하고 stable ID와 선택 callback을 Scene 재로드 뒤에도 복구
- WORLD-3 fallback 경계: `WorldPresentationFallbackView`가 Synty `VisualRoot`와 primitive 외형만 전환하며 Farm tile·Market shelf·Residential pickup 업무 View와 stable ID를 wrapper에 유지
- WORLD-3 material 보정: Market/Residential Sample의 상태 색을 Edit Mode 임시 material 생성 대신 `MaterialPropertyBlock`으로 적용하고 원본 vendor material은 수정하지 않음
- WORLD-3 Game View: Overview/Farm/Logistics/Market을 재캡처하고 [시각 변경 기록](../Changes/2026-08-09-unity-city-farm-world-3.md)에 보존
- WORLD-4 cargo 계약: 기존 `CargoWarehouseHandoffSnapshot`을 prefab 중립 `CargoJourneyPresentationModel`로 투영하고 origin·product·cargo·handoff·transport task·inbound task 6개 source lineage를 보존
- WORLD-4 Scene: WORLD-3을 보존한 별도 `CityFarmCargoJourney` Scene에서 같은 `cargo:transport-71`을 Farm Yard potato box·Transport cargo box·Logistics pallet·Market backroom 예정 box에 연결
- WORLD-4 권위 경계: `ArrivedAtWarehouse`의 현재 Zone은 Urban Logistics로 두고 Market은 실제 도착 근거가 없으므로 `Planned`로 유지하며, 낮은 revision·cargo identity 변경을 View에서 차단
- WORLD-4 Game View: Overview/Farm Yard/Logistics/Market을 재캡처하고 [시각 변경 기록](../Changes/2026-08-09-unity-city-farm-world-4.md)에 보존
- WORLD-5 품질 Scene: WORLD-4를 보존한 별도 `CityFarmVisualQualityGate` Scene에 Game View 비교로 선택한 Zone distance 26과 screen-space camera HUD를 저장
- WORLD-5 가독성: 멀리서 읽히지 않던 3D `TextMesh` evidence를 최종 Gate Scene에서 숨기고, 기존 Cargo Journey만 읽는 Farm Yard→Transport→Logistics→Market 상태 bar와 Presentation 비권위 문구로 대체
- WORLD-5 reference Gate: 모든 `WorldVisualInstanceView`의 catalog/prefab 연결, vendor prefab source, material/shader, missing MonoBehaviour script를 Scene 재로드 뒤 검사
- WORLD-5 PC/Android 분리: 현재 PC RP Asset의 render scale 1.0·2048 shadow·4 cascade·SSAO와 Mobile RP Asset의 0.8·1024 shadow·1 cascade·SSAO 없음 차이를 읽기 전용으로 기록하고, Android Player 실측 전 추가 축소 수치는 확정하지 않음
- WORLD-5 Game View: Overview/Farm/Logistics/Market 최종 PNG를 [시각 변경 기록](../Changes/2026-08-09-unity-city-farm-world-5.md)에 보존하고 Visual 확장 중단 조건을 적용

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
| UrbanMarket UM3R 재설계 문서 | 통과 | 전역 allocation·다중 원천 SourcePlan을 관리자 Perspective보다 먼저 검증하도록 순서 보강; docs-only Fast에서 build·test 생략, `git diff --check` 통과, log `20260809-101425` |
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
| UrbanMarket UM5-B Scene·Game View | City Pack builder·Play Mode 통과 | manager primitive builder를 기반으로 저장 Scene을 생성하고 View 교체 뒤 Runtime 시작과 Console error 0건 확인 |
| UrbanMarket UM5-B scoped Fast / Task | Fast·Task build 통과, Task 비관련 실패 | Fast log `20260809-120408`; Task에서 v0.0 build 통과 후 4,465/4,472 통과와 기존 metadata/naming/CSS 7건 실패, log `20260809-120420` |
| UrbanMarket manager Queue 제거 targeted | 15/15 통과 | 평면 ShelfState Perspective, queue 없는 surface change set, focus·last-success 회귀; TRX `artifacts/local/validation/urban-market-manager-no-queue/urban-market-manager-no-queue.trx` |
| Unity core manager Queue 제거 | 216/216 통과 | `Ssalddel.Unity` 전체 headless 회귀; TRX `artifacts/local/validation/urban-market-manager-no-queue-full/unity-manager-no-queue-full.trx` |
| UrbanMarket manager Queue 제거 scoped Fast | 통과 | 관련 Unity core·test project build와 `git diff --check`; log `20260809-151049` |
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
| Concept Card CC3-A asset readiness | Unity EditMode 3/3 통과 | 실제 imported sample에서 대표 NPC·7장 카드·선택·NavMeshData·Mecanim parameter 검증; Scene 저장 없음, XML `artifacts/local/validation/urban-market-cc3-preasset/urban-market-cc3-editmode.xml` |
| Concept Card CC3-A Unity core | 217/217 통과 | engine-independent 전체 회귀, TRX `artifacts/local/validation/urban-market-cc3-preasset/unity-core/unity-cc3-preasset.trx` |
| Logistics facility overview core | 집중 6/6, Unity core 222/222 통과 | canonical cargo handoff를 차량 접근·입고 Dock·검수·보관 4영역에 투영하며 알 수 없는 상태와 가상 handoff 생성을 거부; TRX `artifacts/local/validation/logistics-facility-overview/` |
| Logistics facility overview Unity EditMode | 3/3 통과 | imported sample의 건물·4영역·화물 `VisualRoot` 회귀와 City Pack 저장 Scene·Game View를 함께 확인 |
| Farm soil tile FARM-0~FARM-1 core | 신규 6/6, 기존 Farm 포함 집중 10/10, Unity core 228/228 통과 | 6×6 좌표·stable ID·Simulation 경계, 중복·누락·재배 참조 불일치, 선택·color token 검증; TRX `artifacts/local/validation/farm-soil-tile/` |
| Farm soil tile FARM-0~FARM-1 Unity EditMode | 3/3 통과 | 실제 imported Farm sample에서 36개 cell, 무선택 초기 상태, stable-ID 선택 상세와 Selected material을 임시 Scene으로 검증; 저장된 Scene·Game View는 미실행 |
| 농장 풍경 Composition Library | 집중 5/5, 기본 Unity EditMode 32/32 통과 | 8종×A/B/C 24 prefab, 실제 Synty nested source, 실제감자밭·농부·차량·농기계·화물·상호작용 socket, Simulation/Operational 권위 부재와 저장 preview Scene을 검증 |
| POLYGON Farm 식품 Asset·HS·가격 조사 | 통과 | 실제 prefab 파일명 29개 품목군·177개와 현재 HS·KAMIS code catalog를 대조; docs-only Fast `git diff --check` 통과, log `20260809-212630`; live 가격·Unity runtime은 범위 밖 |
| Unity Farm 상품·가격 카드 흐름 | 통과 | 기존 선택·Concept Card·국내/국가별 가격 계약을 재사용하는 FPC0~FPC3 설계와 직접·대표·후보·오류 Gate 문서화; docs-only Fast 통과, log `20260809-213144`; adapter·Scene·runtime은 미구현 |
| POLYGON City Composition Set 조사 | 통과 | 실제 City prefab 335개 재집계, 첫 12종×A/B/C와 후속 6종의 한국어 세트·source·socket·반복 규칙 문서화; docs-only Fast 통과, log `20260809-213731`; Unity asset·Scene은 미구현 |
| City 주거단지·십자형 도로 Modular 설계 | 통과 | 실제 collider 기준 5m cell, 도로 6종·주거 block 8종과 십자형 생활권 recipe·connector graph·단독주택 asset 공백 문서화; docs-only Fast 통과, log `20260809-220737`; RR0~RR4 구현은 미수행 |
| Farm 시설하우스·밭·논 단지 Modular 설계 | 통과 | Greenhouse 2종·Dirt/Vege Row·농로 9종·관수 source를 대조해 농로 6종·시설하우스 6종·밭 8종과 논 Blockout Gate 문서화; docs-only Fast 통과, log `20260809-221337`; GF0~GF5 구현은 미수행 |
| POLYGON Town Composition Set 조사 | 통과 | 실제 Town prefab 702개·material 25개를 재집계하고 5m 도로 6종·생활권 12종×A/B/C와 선택적 내부 세트를 문서화; docs-only Fast 통과, log `20260809-222620`; TOWN0~TOWN4 구현은 미수행 |
| Farm·Town·City 혼합 Composition 조화 설계 | 통과 | Pack별 생산·저밀도 생활·고밀도 유통 책임과 Farm↔Town 6종·Town↔City 6종·세 Pack 관통 4개 recipe를 문서화; docs-only Fast 통과, log `20260809-222620`; MIX0~MIX4 구현은 미수행 |
| Farm·Town·City Composition 통합 구현 순서 | 통과 | 이미 구현된 WORLD·FARM-2·농장풍경 24개와 미구현 Composition·가격 Card를 분리하고 CMP0~CMP11, 첫날 CMP0~CMP3 권장 절단선과 FARM-3 복귀 조건을 문서화; docs-only Fast 통과, log `20260809-223153`; Unity 구현은 미수행 |
| Farm·Town·City 3개 독립 Region Map 설계 | 통과 | Farm 북서·City 북동·Town 남서의 독립 Region과 Town·City 사이 Hub, passenger/freight Gate·Route, 사람·Cargo Stateful Journey와 ambient traffic, 독립 root·additive Scene 후속 Gate를 문서화; D-033은 세 Region, D-034는 Hub freight topology로 구체화; Unity 구현은 미수행 |
| Farm·Town·City 지역 물류허브 설계 | 통과 | 여러 Farm·Town origin→Hub inbound→검수·보관·allocation→Hub→City outbound, passenger/freight 분리, split·merge 보존식과 직송 예외를 문서화하고 D-034로 고정; docs-only Fast 통과, log `20260809-230430`; Unity 구현은 미수행 |
| Composition 우선순위·Synty animation 보강 | 통과 | CP0~CP5 실행 등급, CMP4-A, ANIM0~ANIM6, 실제 clip/controller 부재·Humanoid retarget·Town missing controller·FX inventory를 문서화하고 D-035로 고정; 9개 문서 link 0건 누락·trailing whitespace 0건, docs-only Fast 통과, log `20260809-232128`; Unity 구현은 미수행 |
| Composition CP0 기준선 복구 | 통과, CLI 종료 crash 1회 | 저장 Showcase 전용 4/4·기존 전체 64/64, Pipeline validation·Console Error 0·Scene dirty false, 대표 PNG 4종과 351 instance·370 renderer·370 shadow caster·7 transparent·Animator/Particle 0; CLI 결과 저장 뒤 Unity InputSystem/TextCore native crash는 열린 Editor 재검증으로 분리 |
| Composition CMP1·ANIM0 | 집중 8/8, 전체 72/72 통과 | 공통 descriptor·connector·socket·source/detail/journey·A/B/C signature validator, 기존 Farm 24개 adapter, Synty clip/controller·Humanoid·missing controller·FX inventory 검사; 새 Town·City·Hub prefab은 아직 생성하지 않음 |
| Composition CMP2 source 실측 | 집중 4/4, 전체 76/76 통과 | 세 Pack source 42개 측정, Town·City 5m grid 확인, Farm Dirt Road 11.9106m·10m 접속 오차 1.9106m·adapter offset `(0, 0, -0.9553)`, LODGroup 0·공통 shader `Synty/Generic_Basic`; Town House 문 방향 12개는 결합 mesh 한계로 `unknown` 유지 |
| Composition CMP3 도로·Gate A형 | 집중 6/6, 전체 82/82 통과 | 세 Pack 도로 12개·Gate 10개, 사람·차량 경계 4쌍·화물 3쌍, builder 2회 `22 → 22`, 90도 회전·tile 중첩·Farm offset·nested prefab 검사, Console Error 0·Preview dirty false·Game View 확인 |
| Composition CMP4 최소 거점 A형 | 집중 6/6, 전체 88/88 통과 | 실제 감자 6×6 필지·타운 기본주택·시티 공동주택 가로형·지역 물류허브 Dock 4종, CMP3 도로/Gate 방향·노선 접속, source/설계 출입구·회전반경·occlusion·상태 socket·권위 부재 검사, builder `4 → 4 → 4`, Preview dirty false·Game View 확인 |
| Composition CMP4-A·ANIM1/ANIM2 fallback | 집중 6/6, 전체 94/94 통과 | 공용 Idle/Walk key·intent·source kind·catalog·adapter, Farm/Town/City 대표 Humanoid route follower, root motion 비활성, 실제 clip/controller 0·Town missing controller 8 진단, procedural fallback Play Mode Game View 확인; 실제 clip retarget은 미완료 |
| Composition CMP5 세 Region·Hub Journey | 집중 7/7, 전체 101/101 통과 | Farm·Town·City·Hub A형 anchor 4개, Gate 10개, 사람 Journey 2개, Farm/Town 화물 2개; 기존 감자 cargo·6 lineage Hub 보관, allocation 있는 Town cargo만 City outbound, 위치 tick의 업무 상태 비변경과 Play Mode Overview 확인 |
| WORLD-0 Diorama camera 집중 | 4/4 통과 | focus level, pitch·zoom clamp, 90도 회전, Perspective rig와 명시적 foreground cutaway |
| WORLD-0 Unity EditMode 전체 | 29/29 통과 | product 20, Farm 3, Logistics 3, Market 3; 최종 recompile 성공과 Console error 0 |
| WORLD-0 primitive Game View | 확인 | Overview/Farm/Logistics/Market focus와 Market 90도 회전 raw capture `C:\Users\user\ssalddel\artifacts\WORLD-0\`; Scene 저장 없음 |
| WORLD-1 공급망 계약·Scene 집중 | 4/4 통과 | 6개 순차 Presentation Zone, Farm canonical 공유, 중복·단절 거부, 저장 Scene의 Zone·route·camera reference 재로드 |
| WORLD-1 Unity EditMode 전체 | 33/33 통과 | product 24, Farm 3, Logistics 3, Market 3; WORLD-0과 기존 Operational 실패 비대체 회귀 포함 |
| WORLD-1 Game View·기본 수량 | 확인 | Overview/Farm/Logistics/Market 대표 PNG, renderer 69·Animator 0·FX 0; 최종 recompile up-to-date, Console error 0, Scene dirty false |
| WORLD-2 Catalog·저장 Scene 집중 | 3/3 통과 | vendor-neutral key, allowlist 21종 prefab·shader, 저장 Scene의 catalog wrapper·vendor prefab connection·Global Volume reload |
| WORLD-2 Unity EditMode 전체 | 36/36 통과 | product 27, Farm 3, Logistics 3, Market 3; WORLD-0~WORLD-1과 Operational 실패 비대체 회귀 포함 |
| WORLD-2 Game View·기본 수량 | 확인 | Overview/Farm/Logistics/Market 대표 PNG, renderer 142·Animator 1·FX 0; 최종 recompile up-to-date, Console error 0, Scene dirty false |
| WORLD-3 기존 업무 View·fallback 집중 | 5/5 통과 | 저장 Scene 재로드, Farm stable-ID 선택, Market shelf/Card 선택, primitive fallback, Simulation Tick·LifetimeScope 부재 |
| WORLD-3 Unity EditMode 전체 | 41/41 통과 | WORLD-0~WORLD-2와 Farm·Logistics·Market Sample 회귀 포함 |
| WORLD-3 Game View·기본 수량 | 확인 | Overview/Farm/Logistics/Market 대표 PNG, active renderer 200·Animator 1·FX 0·fallback socket 41; 최종 recompile 성공, Console error 0, Scene dirty false |
| WORLD-4 cargo journey 계약 | 4/4 통과 | 네 Zone의 동일 cargo identity, handoff 상태별 현재 Zone, Market 도착 비발명, 명시적 origin/product stable source 검증 |
| WORLD-4 Scene 집중 / Unity EditMode 전체 | 6/6·47/47 통과 | Scene 재로드, 4 anchor·6 source lineage, Market Planned, revision·identity, primitive fallback, Simulation authority 부재와 전체 회귀 |
| WORLD-4 Game View·기본 수량 | 확인 | Overview/Farm Yard/Logistics/Market PNG, active MeshRenderer 211·Animator 1·ParticleSystem 0·cargo anchor 4·fallback 44; Editor 순간값 draw call 59·set pass 14·triangle 15,162·vertex 28,000, Console error 0 |
| WORLD-5 품질 Gate 집중 / Unity EditMode 전체 | 5/5·52/52 통과 | 저장 Scene 재로드, HUD 4단계, Zone distance 26, 3D text 억제, shader·vendor prefab·missing script, 범위 확장·업무 권위 부재와 전체 회귀 |
| WORLD-5 Game View·기본 수량 | 확인 | Overview/Farm/Logistics/Market 1600×900 PNG, active MeshRenderer 191·Animator 1·ParticleSystem 0·VisualInstance 106·cargo anchor 4·fallback 44, camera far clip 300 |
| WORLD-5 Editor profiling | 제한적 확인 | 4개 focus 모두 draw call 59·set pass 14·triangle 15,162·vertex 28,000; CPU frame 순간값 0.71~6.32ms, GPU timing 0으로 미수집. Editor/Pipeline 상태이므로 Player FPS·메모리 기준 아님 |
| WORLD-5 PC/Mobile URP 기준선 | 읽기 전용 확인 | PC render scale 1.0, shadow 50/2048, 4 cascade, soft High, SSAO 1개; Mobile 0.8, 50/1024, 1 cascade, soft Medium, SSAO 없음. 기존 Asset 수정 없음 |
| FARM-2 core / Farm View / Unity EditMode 전체 | 10/10·6/6·55/55 통과 | Preview·Confirm 원본 불변, forged/stale command 거부, explicit Tick만 revision 2 `Tilled`, 6×6 stable-ID reconcile와 primitive Dirt Row 형상 검증 |
| FARM-2 저장 Scene·Game View | 확인 | `FarmTillingVerticalSlice` validator와 선택·Preview·Confirm·적용 1600×900 PNG, 최종 직접 조회 `tick:2:Tilled:row=(1.05, 0.34, 0.76)` |
| Synty City Pack market integration | builder validation·Play Mode 통과 | 저장 Scene, Humanoid Avatar, View/Animator socket, URP shader, manager Runtime 시작과 Console error 0건 확인; Game View `Assets/artifacts/citypack-market-playmode-final.png` |
| Synty City Pack logistics integration | builder validation·Play Mode 통과 | 저장 Scene, facility/vehicle/cargo VisualRoot, URP shader와 Console error 0건 확인; Game View `Assets/artifacts/citypack-logistics-playmode-final.png` |
| City Pack 관련 Unity 회귀 | core 228/228·마트 3/3·물류센터 3/3 통과 | core TRX `artifacts/local/validation/citypack-adoption/citypack-core.trx`; imported sample은 열린 Unity 6000.5.6f1 Editor의 Pipeline Test Runner; Android 성능은 미실행 |
| City Pack scoped Fast | 통과 | Unity core build와 `git diff --check`; 최종 log `artifacts/local/validation/20260809-163642` |

## 현재 작업

운영 서버를 변경하거나 Unity core를 이동하지 않고 `Ssalddel.Simulation.Contracts`, `Ssalddel.Simulation.Domain`, `Ssalddel.Simulation.Server`를 별도 dependency island로 추가했다. 첫 API는 session 생성·조회·Tick 진행이며 client request·command 멱등성, expected revision, scenario data revision, seed와 rule revision을 보존한다. API는 기본 비활성이고 `SsalddelExecution:Mode=Simulation`에서만 시작한다. 현재 저장소는 process-local in-memory이므로 개발 fixture 외의 save 권위로 사용하지 않는다.

Unity의 P0~P7·DIP5R 공통 기반과 외부·공공 데이터 P6-B 결과는 유지한다. DIP6 도심마트 UM0~UM4에 이어 UM5에서 `도심마트ManagerRuntime`과 별도 manager surface sample을 추가했다. Runtime은 manager role이 승인된 `AuthorizedUserWorld` Data context만 받고, Data refresh 뒤 Shared World에 남아 있는 선택만 focus로 유지하며, shelf·task·source-plan·detail을 각각 stable-ID change set으로 반환한다. 업무 우선순위 점수·summary·queue는 만들지 않는다. refresh 실패는 마지막 성공 Presentation과 선택을 유지하고 session·World·authorization 경계 변경은 둘 다 폐기한다. View는 Projector가 결정한 문구·색 token·상자 수만 적용한다.

RG1~RG4, RG4-NPC-A/B와 SC0~SC5는 집단 수요·대표 identity·4주 Engine·Perspective·두 Zone 방문·공급 위험/Presentation과 Unity mapper/applicator/View source까지 구현했다. CC1~CC2의 공통 계약·Projector·대표 7-card adapter에 이어 CC3-A에서 `ConceptCardView`·deck·4종 visual skin, 대표 선택, manager desk, 임시 NavMesh와 Mecanim parameter를 연결했다. 도심 물류센터 sample에는 한 handoff 조회로 Truck과 시설 overview를 함께 만들고 차량 접근·입고 Dock·검수·보관을 보여주는 View를 추가했다. 첫 Engine은 검수·입고·진열 판매가능 전환 작업을 하나의 명시적 Tick capacity로 합산하며, 별도 직원 task·진열 surface는 SC7에서 UM4와 연결한다. 실제 Unity project에는 City Pack 원본을 유지한 채 두 전용 builder와 저장 Scene을 추가했고, 마트·물류센터 Game View와 Play Mode를 확인했다. 구매 asset은 View에만 머물며 canonical Data·stable ID·Command 경계는 기존 Ssalddel 코드가 소유한다.

`도심마트: 계약이 진열대를 만든다`는 지역 기본 수요와 기존 같이 주문 집단 수요를 source별로 보존한다. 공공 인구·세대 사실은 잠재수요 Interpretation까지만 제공하고, 비구속 `GroupIntentDemand`는 공급 문의 신호로만 표시한다. 주민별 기존 개별 주문을 합산한 `GroupOrder`만 `GroupConfirmedDemand`가 된다. Operational 연결은 RG5~RG7+SC9에서 기존 authorized Projection·대표 capability·ResidentialPickup·공급중개 UseCase를 통과할 때만 수행한다.

World 실행열은 WORLD-5에서 시각 확장을 중단했고 FARM-2 밭갈이 폐루프까지 완료했다. 기존 6×6 snapshot·validator·Projector·stable-ID View를 재사용해 선택→Preview→명시적 Confirm→Simulation Tick→revision 2 새 Snapshot→Reconcile→`Tilled`를 연결했다. Preview와 Confirm은 revision 1 snapshot을 변경하지 않고 Tick만 새 snapshot을 반환한다. 타일 선택, NPC 도착, animation·FX 완료는 Confirm이나 Tick을 자동 발생시키지 않는다. 별도 `FarmTillingVerticalSlice` Scene과 선택·Preview·Confirm·적용 Game View를 저장했으며 다음 Gate는 FARM-3 농부 작업 Presentation이다.

## 다음 구현 후보

1. CMP6: 감자 상품·가격 카드 한 품목을 Farm·Hub·City anchor에 연결
2. FARM-3: 감자 카드 수직 슬라이스 뒤 농부 작업 Presentation 한 종류로 복귀
3. 검증된 Idle/Walk clip을 확보하면 ANIM2 retarget 품질 Gate를 별도 통과하고 procedural fallback을 교체

## 미해결

- Farm canonical schema와 API contract는 추가됐지만 migration은 실제 DB에 적용하지 않았고 sensor ingestion·판정 rule 실행 경로와 운영 seed가 없음
- Farm Pack의 감자 S/M/L·감자 상자·Dirt Row·Humanoid 농부와 농기계를 확인하고 6×6 Tile View·VisualRoot·primitive fallback과 Farm Game View를 연결했다. 밭갈이 Confirm/Tick 폐루프는 완료했지만 실제 Synty Dirt Row 교체, SeedLot·파종·생육과 농부 작업 animation은 아직 없음
- Role Perspective server aggregate는 기사/도심 물류센터·공동수령·생산자 농장까지 확장됐지만 협동조합과 다른 Zone은 미구현
- Farm·Town·City character FBX의 Humanoid rig는 확인했지만 Synty standalone·embedded AnimationClip은 없고 Town character prefab 8개는 대응 asset 없는 controller GUID를 참조한다. missing-reference validator는 구현했으며 실제 walk/work 재생과 공용 리타기팅은 미구현
- Warehouse W1 operational API와 UnityWebRequest refresh는 검증됐지만 선택·highlight 클릭 상호작용의 실제 Game View 확인은 미검증
- 로컬 DB의 기존 계정 password hash가 현재 개발 설정과 일치하지 않고 기존 암호화 데이터의 Data Protection key도 현재 key ring에 없어 정상 개발 로그인과 startup seed는 별도 환경 정리가 필요함
- 사용자 보고 P2 runtime과 현재 `Ssalddel.Unity` core의 결합 방식 확인 필요
- Unity project를 monorepo에 둘지 별도 repository로 둘지 ADR 필요
- 실제 제품 Unity project의 전체 presentation assembly와 application composition root는 미확인이나, 도심마트 sample은 `C:\Users\user\ssalddel`에 import해 VContainer 조립과 EditMode compile을 확인함
- 도심마트 `api/v1/orderer/mart/products`는 주문자용 공개 판매 가능 수량만 제공하며 관리자용 진열대·위치별 재고·진열 보충 작업·직원 배정 canonical source와 authorized API는 없음
- 도심마트 UM5 manager Runtime·surface applicator·selection·last-success와 City Pack manager Scene 생성·Game View·Play Mode 시작은 검증했지만 실제 클릭 선택과 이동 완료 전 과정은 미검증
- 도심마트 UM3R은 명시적 allocation과 다중 원천 SourcePlan을 구현했지만 operational server에는 canonical 진열대·위치별 재고·작업·allocation Projection이 아직 없음
- Simulation 서버 기반은 구현했지만 process-local in-memory store이며 인증·사용자별 session scope·영속 save/replay와 Unity HTTP repository는 미구현
- 공급 계약 경영 Simulation은 SC0~SC5와 RG1~RG4·RG4-NPC-A/B의 공급/집단 graph·Engine·Perspective·Presentation·NPC route/visit·Unity binding과 City Pack 저장 Scene까지 연결했지만 SC6 Confirm/Tick과 SC7 운영 결과 폐루프는 미구현
- Concept Card는 CC3-A View·visual skin·대표 선택에 City Pack 대표/관리자 `VisualRoot`와 저장 Scene·Game View를 연결했지만 실제 카드 클릭별 스냅샷 검증은 미실행
- 물류센터 overview는 City Pack 시설·차량·화물 `VisualRoot`, 저장 Scene·Game View와 imported sample EditMode를 검증했지만 독립 검수 진행 canonical 상태와 Play Mode role action은 아직 없음
- 공동주택 집단의 기존 서버 재사용 설계는 완료했지만 마트 관리자 privacy-safe 집계 Projection, 대표의 특정 집단 마트 문의 capability, group-order source revision Projection은 아직 없음
- 주민자치 대표의 City Pack Humanoid Avatar·wrapper socket·저장 Scene·Game View와 마트 Play Mode 시작은 검증했지만 실제 walk clip과 방문 왕복 animation은 미검증
- 현재 `ResidentialPickup`은 `residential-pickup:{출고예정Id}`와 canonical unloading task를 제공하지만 별도 pickup-point canonical reference는 직접 보존하지 않아 RG6 검토가 필요함
- 지역 인구·잠재수요·Demand Scenario→기본 Simulation 주문 handoff는 fixture로 구현했지만 실제 공공 Data provider와 운영 주문 Projection은 아직 없음
- 기존 플랫폼 공급중개 원장은 단가·최소/최대 발주수량과 문자열 정산·반품 조건을 보존하지만 첫 Playable의 납품 주기·lead time·결제 일수·품질 수명 canonical 필드는 없으므로 SC9 전까지 운영 계약에서 추정하지 않음
- Synty City·Farm Pack 구매·import와 공통 `Synty/Generic_Basic` PC URP shader는 확인했고 City 첫 Scene 교체까지 검증했지만 Farm 제품 Scene, license/seat 운영 기록, Android material·draw call·메모리는 미검증
- 전체 test suite의 비관련 실패 7건이 남아 있어 전체 green 상태는 아님
- 지역 인구·수요 Layer는 제안만 작성됐고 공급자 key·이용 조건·지역 geometry·운영 집계 policy·API·Unity 코드는 아직 미구현
- DIP3~DIP5 core는 headless와 열린 Unity Editor package import를 검증했고 DIP4 `Samples~/UrbanLogisticsCenter`는 overview와 City Pack 저장 Scene·Game View까지 확인했다. DIP5 `Samples~/PublicDataHall`·`Samples~/CommunityMarketSquare` 별도 sample assembly·Scene refresh는 재검증하지 않았다. DIP6 도심마트 UM0~UM5는 manager City Pack Scene·Play Mode까지 확인했으며 SC6 이후 Confirm/Tick 폐루프는 미구현
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
