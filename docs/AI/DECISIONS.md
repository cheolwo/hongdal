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
