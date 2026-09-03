# Hub 입고·검수·적치

## 식별과 근거

- 주제 고유 식별자: `topic:hub-inbound-putaway.v1`
- PlayableLoop 고유 식별자: `playable-loop:hub-inbound-putaway.v1`
- 기획 revision: `hub-inbound-putaway.design.r1`
- 원천 기획 문서:
  - `docs/AI/Planning/시스템/PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001/README.md`
  - `docs/Architecture/UnityServerStateToWorldProjectionDesign.md`
- 마지막으로 반영한 기획 revision: `operations-unity-transfer.r1`
- 남은 승인 차단 미정: 정확한 `VisualKey`, Synty 후보, H2 내부 연결구·도달 가능성, 실제 배치·입력

## 플레이어 약속과 재미

- 플레이어가 처한 상황: Hub 자체 입고 Fixture가 도착했고 검수·적치 작업을 선택할 수 있다.
- 플레이어가 원하는 것: 화물 상태를 확인하고 허용된 위치에 적치해 다음 창고 작업을 연다.
- 반복해도 재미있어야 하는 핵심 선택: 화물 상태와 용량을 보고 검수·적치 순서를 고르며, 실패하면 다른 위치 또는 작업을 다시 선택한다.
- 짧은 플레이어 약속 한 문장: 독립 입고 화물을 검수하고 적치한 뒤 다음 Hub 작업 선택으로 돌아간다.

## 반복 폐루프

`HubIndependentInboundFixtureReady → WI-001 검수 → WI-002 적치 → HubPutAwayCompleted → HubWorkChoiceAvailable`

- 진입 상태: `HubIndependentInboundFixtureReady`
- 종료 뒤 다시 열리는 선택: `HubWorkChoiceAvailable`
- 다른 영역 의존성: Farm·City에서 온 화물이나 실제 운송 완료를 진입 조건으로 요구하지 않는다.

## 선택·대가·성공·실패·회복

- 선택지: 검수 대상 선택, 검수 결과에 따른 보류, 허용 보관 위치 선택, 적치 재시도
- 자원·시간·위험 대가: 작업 시간, 사용 가능한 보관 용량, 손상·불일치 화물의 보류 비용
- 성공 결과: `HubPutAwayCompleted`와 같은 revision의 재고·작업 상태 사본
- 실패 결과: 검사 불일치, 용량 부족, 위치 미할당, 오래된 revision 거부
- 실패 뒤 회복 경로: 상태를 다시 조회하고 보류 또는 다른 허용 위치를 선택해 `HubWorkChoiceAvailable`로 돌아간다.

## WI 단일 책임 후보

| 순서 | WI 후보 | 한 번에 바꾸는 권위 상태 | 주체 | 비고 |
| --- | --- | --- | --- | --- |
| 1 | `WI-001` | 입고 화물의 검수 상태 | Player/NPC | 검수와 적치를 한 Command로 합치지 않는다. |
| 2 | `WI-002` | 검수된 화물의 보관 위치·적치 상태 | Player/NPC | 허용 용량·위치와 같은 revision을 검사한다. |

## 논리·표현 요구

- 논리적으로 반드시 성립할 상태와 규칙: Hub 독립 Fixture, 권한, 현재 revision, 검수 상태, 허용 보관 위치와 용량
- 플레이어가 화면과 소리로 식별해야 할 대상: 입고 화물, 검수 접점, 보관 위치, 보류·완료 상태
- 결과가 같은 revision임을 보여줄 피드백: 서버 Snapshot을 canonical 재조회해 작업·재고·NPC 표현을 함께 갱신
- 공통 표현 검증 모듈 외 조건 모듈: H2 내부 도달 가능성, 화물 Bounds, `InteractionAnchor`, 작업 접점의 가림·충돌
- 기존 소비 경로: `WarehouseWorldApiRepository → WarehouseWorldInterpreter → WarehousePresenter`

## H 공간과 자산 요구

- 필요한 H1~H5 능력:
  - H1 `h1-stock:hub-receiving-storage`
  - H2 `h2-candidate:hub-inbound-storage`
  - 상위 조립 후보 `h2-candidate:hub-internal-warehouse`
  - 능력 `ReceivingWorkArea`, `InspectionWorkArea`, `StorageCapacity`
- 실외·실내 배치 요구: 입고 접점과 검수·보관 구획의 통행이 이어져야 하며 작업자·화물 동선을 막지 않는다.
- Synty 자산 후보와 대체 표현: Presentation E4에서 주·대체·fallback을 별도 동결한다. 현재 미선정이다.
- Traversal, Collider, NavMesh 요구: 실제 후보의 Renderer·Collider·Bounds와 접근 가능한 `InteractionAnchor`를 E5에서 검증한다.

## 전문 심화 연구 판정과 재결속

| 분야 | 필요성 | 연구 문서 참조 또는 NotRequired 사유 | 상태 | 기획서 반영 항목 |
| --- | --- | --- | --- | --- |
| 건물 | `Required` | Hub 입고·보관 H1/H2 후보의 실제 구조 조사 필요 | `Planned` | 실내외 경계·출입구·천장·하역 높이 |
| 공간 | `Required` | H2 연결구·도달 가능성 조사 필요 | `Planned` | 입고→검수→적치 동선 |
| 배치 | `Required` | 화물·작업자·보관 랙 간격과 Anchor 조사 필요 | `Planned` | 가림·충돌·회전·대기 구획 |
| 애니메이션 | `Required` | 검수·운반·적치 ActionCue와 fallback 조사 필요 | `Planned` | Actor 역할·중단·귀환 |

- `requiredDetailStudyRefs`의 모든 `Required` 연구가 `Accepted`인지: 아니오. E5 실행 전 결속해야 한다.
- 연구 결과로 다시 연 Logic E와 이유: 현재 없음. 권위 상태 결손이 발견되면 가장 이른 Logic E를 다시 연다.
- 연구 결과로 다시 연 Presentation E와 이유: 현재 E1. E4 후보 동결 전 실제 후보와 H 계약이 필요하다.
- 연구끼리 충돌한 사항과 기획 판단: 현재 없음.
- 개발 인계에 고정할 측정값·자산 fallback·검증법: 연구 수용 뒤 같은 revision의 작업 명세에서 고정한다.

## 저장·권위·외부 경계

- Simulation 권위 상태: Hub 독립 입고 Fixture의 검수·적치 상태
- 서버 읽기 Projection: `api/v1/warehouse-operations/world/zones/warehouse`
- Save/Replay에 고정할 값: 화물 안정 ID, 작업 상태, 보관 위치, revision과 선택 결과
- LocalProcess/RemoteHost 동등성: 같은 Simulation Core와 frozen Fixture를 사용하며 운영 DB를 Local fallback으로 대체하지 않는다.
- 외부 Provider 또는 운영 효과 제외: 실제 운송·정산·계약·운영 창고 변경은 이번 폐루프의 기본 Fixture에서 제외한다.

## 제외 범위와 승인

- 이번 주제에서 하지 않는 것: 새 공식 Scene, 자동 H 생성, Farm→Hub 화물 선행, 실제 운영 Command, Synty 자동 채택, E5 자동 승격
- 검토할 사람 또는 근거: 운영 서버→Unity 선별 이관 정책, 기존 창고 Snapshot 계약, H 안정 ID와 실제 공간 전문 연구
- 승인 근거 참조: `docs/AI/Planning/시스템/PLAN-ARCH-OPERATIONS-UNITY-TRANSFER-001/README.md`
- 승인 상태: `Approved`
- 현재 구현 상한: 기획 관문과 전수 대장. Presentation은 E1이며 E4 준비가 다음 목표다.
