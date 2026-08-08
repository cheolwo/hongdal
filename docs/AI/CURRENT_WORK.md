# Ssalddel Current Work

> GPT Chat과 Codex가 다음 작업을 이어받기 위한 최신 snapshot이다. 완료 이력을 계속 쌓는 일지가 아니며, 사실이 바뀌면 기존 항목을 현재 상태로 갱신한다. 장기 결정은 [DECISIONS.md](DECISIONS.md), 전체 맥락은 [공용 프로젝트 컨텍스트](../ProjectOverview/GptProjectContext.md)를 따른다.

## Snapshot

- 기준일: 2026-08-08
- 현재 작업 축: Unity World Projection 기반과 AI 공용 기억 체계
- 제품 공개 기본값: 0.0 커뮤니티·공공데이터
- Unity 개발 범위: 제품 버전 순서에 종속되지 않는 전체 Ssalddel 도메인

## 현재 목표

실제 서버 persistence를 출발점으로 객체별 read-only vertical slice를 우선순위대로 확장한다.

```text
authorized warehouse snapshot
  → Unity Repository / UseCase
  → 창고ZoneController
  → Pallet / Crate / Dock / Picker View
  → warehouse NPC presentation
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

## 검증 상태

| 검증 | 상태 | 근거 또는 제한 |
| --- | --- | --- |
| `Ssalddel.Unity.Tests` | 76/76 통과 | 2026-08-08 headless .NET test; 공공 관측·커뮤니티 광장·창고 World·운송 회랑 mapping과 증분 갱신·실패 유지 정책 포함 |
| 커뮤니티 시장 광장 server tests | 4/4 통과 | 공개 aggregate mapping·정보 최소화·고정 route·하위 조회 실패 전파 |
| 창고 World server tests | 4/4 통과 | 권한 조회 결합·정보 최소화·관리자 route·잘못된 창고 ID 차단 |
| 농사로·작물 서버 targeted tests | 9/9 통과 | Nongsaro module 6개 + CropReference typed projection 3개 |
| 도심 물류센터·창고 인계 server tests | 20/20 통과 | Role/NPC/화물 인계·JSON wire projection 18개 + 기존 운송 원장 상호작용 2개 |
| 관련 Unity core build | 통과 | scoped Fast validation |
| 문서 link·diff | 통과 | 상대 link 검사와 `git diff --check` |
| 전체 Task build | 통과 | `Ssalddel.v0.0.slnx` |
| 전체 Task tests | 비관련 실패 4건 | 4,399/4,403 통과; Unity 변경과 별도인 기존 Web/API metadata·CSS 실패 |
| Unity EditMode | 미실행 | 현재 요청은 코드·headless 계약 검증 범위 |
| Unity PlayMode | 미실행 | 현재 요청 범위에서 제외 |
| built player | 미실행 | Windows·Android runtime 미검증 |
| 실제 Unity Scene | 현재 체크아웃에서 미검증 | 사용자 보고 P2 runtime 소스 위치 확인 필요 |
| Urban Market sample compile | 통과 | 임시 Unity 6 project에서 package·sample script compile 확인 |
| Urban Market scene wiring | 통과 | Editor builder 생성 후 별도 scene reload에서 View wiring·3상품 fixture 확인 |
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
| Warehouse World sample compile | 통과 | Unity 6에서 authenticated HTTP client, VContainer, 팔레트·작업·NPC View compile 확인 |
| Warehouse World scene wiring | 통과 | Editor builder 생성 후 scene reload에서 semantic waypoint·NavMeshAgent socket·Controller·LifetimeScope 확인 |

## 현재 작업

P0의 저장소 내 구현 가능 범위와 P1 공공데이터 정보관, P2 커뮤니티·시장 광장, P3 창고·재고, P4 운송 World 연결의 server API→Unity primitive 코드 경로가 완료됐다. 실제 제품 Unity 프로젝트 runtime이 준비되기 전 다음 코드 우선순위는 P5 도심마트 operational aggregate와 주문자 관점이다.

## 다음 구현 후보

1. 실제 제품 Unity project에서 로그인 session token 공급과 API origin 설정
2. TransportNetwork·Warehouse NavMesh bake·Animator Controller·operational runtime 확인
3. 실제 공개 세계지도 API로 Public Data Hall runtime marker 확인
4. 실제 공개 커뮤니티 광장 API로 게시판·활동·원장 primitive runtime 확인
5. 실제 창고 World API로 팔레트·작업·picker/dock NPC runtime 확인
6. 도심마트 operational aggregate와 주문자 관점
7. 주거공동체 공동수령 Role Perspective
8. Farm·Plot·Cultivation·Sensor canonical server contract와 생산자 관점
9. 협동조합·공동원장 공간 뒤 Synty 최소 팩 검증

## 미해결

- 현재 서버에 Farm·Sensor·Crop canonical Entity가 없어 schema와 API contract 결정 필요
- Role Perspective server aggregate는 현재 기사/도심 물류센터 한 개뿐이며 생산자·주문자와 다른 Zone은 미구현
- NPC route, Zone Scene 배선과 compile은 검증됐지만 실제 Unity NavMesh bake, Animator Controller와 이동 재생은 미검증
- 사용자 보고 P2 runtime과 현재 `Ssalddel.Unity` core의 결합 방식 확인 필요
- Unity project를 monorepo에 둘지 별도 repository로 둘지 ADR 필요
- 실제 제품 Unity project의 presentation assembly와 application composition root는 미확인
- 도심마트 sample은 구현됐지만 실제 제품 Unity project에 import되지 않음
- Synty asset 미도입; 구매·license·URP·모바일 성능 미검증
- 전체 test suite의 비관련 실패 4건이 남아 있어 전체 green 상태는 아님

## 다음 작업 종료 시 갱신할 항목

- 현재 목표와 현재 작업
- 최근 완료 중 여전히 인계에 필요한 항목
- 실행한 build·test·runtime 검증과 정확한 결과
- 새로 확인되거나 해소된 미해결 항목
- 다음 구현 후보의 순서

결정을 변경한 작업이라면 이 파일만 수정하지 말고 [DECISIONS.md](DECISIONS.md)에 대체 결정을 함께 기록한다.
