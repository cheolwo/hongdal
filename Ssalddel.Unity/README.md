# Ssalddel Unity Simulation Data

살뜰 Unity 농업·유통·경영 시뮬레이션의 데이터 우선 코어다. Unity scene이나 `UnityEngine`에 의존하지 않는 로컬 UPM package이며 `netstandard2.1`로도 빌드된다.

## 현재 구현

- Server DTO와 공유 assembly를 참조하지 않는 Unity API model
- API model을 game model로 바꾸는 명시적 Mapper
- stable ID, schema, 단위, provenance, 품목 mapping과 package hash 검증
- `Live`, `Cached`, `Fixture`, `Invalid`, `Failed`를 구분하는 `DataManager`
- 같은 scenario package와 Command로 같은 결과를 만드는 농업 simulation engine
- 성장, 수분, 생산비, 수확과 일반판매·공동판매 비교
- 실제 주문·참여·서버 원장을 만들지 않는 `SIMULATED` 경계
- 대표 Web route 18개를 8개 World Zone과 object·panel·action·Web handoff로 분류하는 `PageWorldProjectionCatalog`
- stable ID와 revision으로 world object의 추가·갱신·제거·유지를 계산하는 reconciler
- 연구 주장·제품 해석·Unity 표현·한계를 분리하는 근거 card model과 validator
- 같은 Sensor 상태를 외부 장비 상태로 표현하는 sensor projection
- 운영 Command에 명시적 확인과 canonical state 재조회를 요구하는 interaction contract
- 도심마트 3개 진열대용 ScreenModel, 명시적 simulated UseCase와 validator
- `Samples~/UrbanMarket`의 SceneController, View socket과 primitive scene builder
- 전통시장·공개 물류거점 ScreenModel, 위치 정밀도·출처·공개 상태 validator
- `Samples~/TraditionalMarketHub`의 시장 건물·물류거점 View socket과 primitive scene builder
- 농사로 작목기술 주분류 API를 받는 작물 기준정보 ApiModel·Mapper·Repository port·UseCase
- 같은 World Object에 생산자·주문자·운송자 관점을 겹치는 Role Perspective ApiModel·Mapper·Repository·UseCase·applicator
- Zone별 semantic waypoint와 operational/simulation 경계를 가진 NPC 이동 snapshot·route catalog·applicator
- `Samples~/NpcMovement`의 NavMeshAgent·Animator Presentation socket
- `Samples~/UrbanLogisticsCenter`의 Role target·waypoint·운송자 NPC primitive builder와 VContainer scope
- 운송원장과 연계 입고요청을 결합하는 `InTransit → ArrivedAtWarehouse → ReceivingCompleted` 화물 인계 workflow
- 거점 간 운송 NPC, 창고 운송자·입고작업자 NPC와 화물 VisualRoot를 Zone별로 적용하는 Presentation socket
- Role Perspective·NPC·창고 인계 API용 `UnityWebRequest` operational adapter와 VContainer runtime mode 전환
- 공개 세계지도 관측의 layer·출처·위치 정밀도·freshness를 보존하는 Public Data Hall Repository·UseCase
- stable ID marker 증분 갱신과 최초 실패·갱신 실패 정책을 적용한 Public Data Hall primitive sample
- 공개 게시판·게시글 요약·비식별 활동 신호·권한 적용 원장 요약용 Community Market Square Repository·UseCase
- 광장 World Item stable-ID 증분 갱신과 마지막 성공 유지 정책을 적용한 primitive sample
- 권한 필터된 재고·적재·피킹을 결합하는 Warehouse World ApiModel·Mapper·Repository·UseCase
- 팔레트·작업 표식과 DockWorker·Picker NavMeshAgent socket을 가진 authorized Warehouse World sample

## 구조

```text
Runtime/
  ApiModels/    Unity가 HTTP JSON을 받는 transport model
  Mapping/      ApiModel → GameModel 변환과 호환성 판정
  Data/         catalog, provenance, validation, DataManager
  Simulation/   Command, Event, state와 결정적 계산
  WorldProjection/ page-to-world catalog, world snapshot과 stable-ID reconcile
  Evidence/     연구 근거, 제품 해석, 시각 번역과 한계
  Sensors/      센서 상태와 외부 장비 projection
  Interactions/ preview·확인·서버 Command·canonical 재조회 계약
  UrbanMarket/  도심마트 ScreenModel, simulated UseCase와 validation
  TraditionalMarkets/ 전통시장·공개 물류거점 ScreenModel과 validation
  Crops/        작물 기준정보 ApiModel·Mapper·Repository·UseCase
  Perspectives/ 서버 승인 역할별 object 강조·허용 interaction과 Role View 적용 계약
  Npcs/         NPC 이동 ApiModel·Mapper, Zone route catalog와 stable-ID 적용 계약
  Community/    공개 커뮤니티 시장 광장 ApiModel·Mapper·Repository·UseCase와 reconcile
  Warehouse/    권한 적용 재고·적재·피킹·NPC ApiModel·Mapper·Repository·UseCase와 reconcile
Samples~/UrbanMarket/
  Runtime/      SceneController와 마트·진열대·가격·재고 View socket
  Editor/       primitive scene 생성과 wiring 검사
Samples~/TraditionalMarketHub/
  Runtime/      SceneController와 시장 건물·물류거점 View socket
  Editor/       primitive scene 생성과 wiring 검사
Samples~/NpcMovement/
  Runtime/      semantic waypoint, NavMeshAgent·Animator View와 Zone Controller socket
Samples~/UrbanLogisticsCenter/
  Runtime/      Role/NPC Repository 조율 Controller, Role target·interaction·waypoint View와 LifetimeScope
  Editor/       도심 물류센터 primitive scene 생성과 wiring 검사
Samples~/PublicDataHall/
  Runtime/      공개 관측 HTTP client, Repository 조율 Controller와 stable-ID marker View
  Editor/       공공데이터 정보관 primitive scene 생성과 wiring 검사
Samples~/CommunityMarketSquare/
  Runtime/      공개 광장 HTTP client, VContainer, Controller와 게시판·게시글·활동·원장 Item View
  Editor/       커뮤니티 시장 광장 primitive scene 생성과 wiring 검사
Samples~/WarehouseWorld/
  Runtime/      인증 HTTP client, VContainer, 팔레트·작업·DockWorker·Picker View
  Editor/       창고 primitive scene과 semantic waypoint socket 생성·wiring 검사
```

`Ssalddel.Unity.Data.asmdef`는 `noEngineReferences=true`이므로 데이터 코어가 GameObject나 scene에 의존하지 않는다. Unity project를 만들면 이 폴더를 local package로 추가하고 별도 presentation assembly가 이 package를 참조한다.

`Perspectives`는 역할별 Scene을 만들지 않는다. 서버가 인증 session과 실제 역할 할당을 검증해 반환한 `RolePerspectiveApiModel`을 Unity snapshot으로 변환하고, Zone이 등록한 stable-ID 대상의 Role View socket만 갱신한다. World View는 그대로 유지되며 `AllowedInteractions`에 없는 행동을 클라이언트가 생성하지 않는다.

첫 operational endpoint는 `GET api/v1/driver/world/zones/urban-logistics-center/perspective`다. 인증된 기사의 현재 배정 운송만 `Transporter` 관점으로 반환하며 주소·연락처·운임은 포함하지 않는다. 같은 기준의 `/npc-movement` endpoint가 운송 상태를 semantic 물류센터 route로 반환한다. Unity에는 Repository·UseCase, Zone Controller와 Role/NPC View socket까지 구현되어 있고 실제 `UnityWebRequest` adapter는 다음 단계다.

NPC 이동은 서버의 업무 상태를 Unity 좌표로 직접 전달하지 않는다. `RouteCode`, `CurrentWaypointKey`, `DestinationWaypointKey`를 Zone layout의 Transform에 매핑한다. 운영 snapshot에는 `CanonicalTaskStableId`가 필수이고 simulation snapshot은 canonical task를 주장할 수 없다. NavMesh 도착은 animation만 실행하며 서버 업무 완료를 만들지 않는다.

창고 화물 인계 endpoint는 `GET api/v1/driver/world/workflows/warehouse-handoff`다. 현재 기사 운송번호와 `입고요청.운송의뢰Id`가 연결된 경우에만 운송 NPC와 창고 입고작업자 NPC movement를 반환한다. `운송중`에는 거점 간 route, `하차지도착`에는 두 NPC의 입고 Dock 집결, `입고완료`에는 운송자 퇴장과 창고 작업자의 보관 구역 이동을 표현한다. 실제 하차와 입고 완료는 기존 server Command가 수행한다.

`OperationalWorldApiClients`는 실제 API base URL과 runtime session token을 사용한다. token은 `RuntimeSessionAccessTokenProvider.SetAccessToken`으로 로그인 결과를 메모리에만 전달하며 Scene·Prefab·config에 serialize하지 않는다. 404는 선택적 NPC/인계 snapshot 없음으로 처리하고, 인증 오류·timeout·잘못된 JSON은 simulation fallback 없이 오류로 전달한다.

Presentation sample은 VContainer 1.18.0을 composition root로 사용한다. Git dependency는 package 내 `package.json`이 아니라 실제 Unity project의 `Packages/manifest.json`에 추가한다.

```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0"
```

전체 프로젝트 구조와 P2 결합 경계는 [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)를 따르고, API Client·Repository·UseCase·Presenter·SceneController·View·Prefab·Inspector의 기준은 [Unity 클라이언트 계층 구조 설계](../docs/Architecture/UnityClientLayeredArchitecture.md)를 따른다.

## 검증

```powershell
dotnet build Ssalddel.Unity/Ssalddel.Unity.csproj
dotnet test Ssalddel.Unity.Tests/Ssalddel.Unity.Tests.csproj
```

golden fixture는 `Ssalddel.Unity.Tests/Fixtures/potato-basic-kr-001.v1.json`에 있다. 이 값은 KAMIS나 기상청의 실제 관측값이 아니라 실제 contract 형태를 검증하는 교육용 `Fixture`다.

현재 저장소에는 실제 Unity project가 없다. `Samples~` 아래 항목은 local package에서 import하는 presentation sample이다. Urban Market, Traditional Market Hub, Urban Logistics Center, Public Data Hall, Community Market Square와 Warehouse World sample은 임시 Unity 6 project에서 script compile, primitive scene 생성과 scene reload 후 wiring 검사를 확인했다. 화물 인계 World router, View socket과 operational HTTP adapter도 Unity 6 script compile을 확인했다. core headless test는 72/72 통과했다. NavMesh bake·Animator Controller, 실제 인증 session과 PlayMode 검증은 수행 범위에서 제외했다.
