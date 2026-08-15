# Figma 근거 Simulation World UI 기획 원장

## 목적

Unity 화면을 만들기 전에 `업무 규칙이 누구에게, 어느 업무 단계에서, 어떤 정보·상태·행동으로 보여야 하는가`를 Simulation World 파생 DB에 개정 가능한 기획 원장으로 저장한다. 이 원장은 실제 Canvas 좌표나 Prefab을 저장하지 않는다.

## 확인한 Figma 근거

| Figma node | 확인한 구조 | DB 반영 |
| --- | --- | --- |
| `2427:243` 역할 앱 01~09 서비스 계층 | Community에서 시작해 Orderer·Shipper·Driver·Warehouse·Restaurant 등 역할별로 같은 원장을 읽음 | `역할코드`, 역할별 화면 영역, 업무 규칙 연결 |
| `2308:990` 주문자 화면 계층·이동 지도 | 발견 → 비교 → 참여 → 준비 판단 | `업무단계코드`, 정보 우선순위, 행동 후보 |
| `2177:64` 살뜰 공통 홈 | 둘러보기, 참여 경계, 역할 선택을 분리 | Simulation 한계·근거 항목, 역할 선택은 권한 확정이 아님 |

Figma는 정보 구조와 시각 의도의 근거다. 서버 권한·Command·실제 상태의 권위는 아니다.

## 원장 트리

```text
UI 기획 대장
├─ Figma 설계 근거
├─ 화면 영역 기획
│  ├─ 시설·공간 anchor
│  ├─ 역할
│  └─ 업무 단계
├─ 정보 항목 기획
│  ├─ 상태·다음 단계
│  ├─ 출처·판정 근거
│  ├─ Simulation 한계
│  └─ 확정 후 원장 재조회 상태
├─ 상태 표현 기획
│  └─ Idle / Loading / Ready / Preview / InProgress / Completed / Blocked / Error
├─ 행동 후보 기획
│  └─ 조회 / Preview / Confirm
└─ UI–업무 규칙 연결
   └─ 원본 객체–업무 규칙 연결 / 시설 기능 / 규칙 개정
```

## 한글 물리 테이블

- `시뮬레이션월드_UI기획대장`
- `시뮬레이션월드_UI설계근거`
- `시뮬레이션월드_UI화면영역기획`
- `시뮬레이션월드_UI정보항목기획`
- `시뮬레이션월드_UI상태표현기획`
- `시뮬레이션월드_UI행동후보기획`
- `시뮬레이션월드_UI업무규칙연결`

대장은 적용한 업무 규칙 대장의 개정 번호와 해시를 함께 고정한다. 하위 원장은 같은 UI 기획 개정에 종속되며, 유효성 검사를 통과한 완전한 묶음만 한 트랜잭션으로 저장한다.

`SchemaVersion 2`부터 UI–업무 규칙 연결은 규칙 식별자만 저장하지 않는다. `원본 객체–업무 규칙 연결 고유 식별자`와 `시설 기능 코드`를 같이 저장하며 다음 정합성을 검사한다.

- UI 화면 영역의 시설과 원본 규칙 연결의 시설이 같다.
- UI에 기록한 기능과 원본 규칙 연결의 기능이 같다.
- 규칙 식별자와 개정 번호가 원본 연결과 같다.
- 활성 객체–업무 규칙 연결이 UI에서 누락되거나 두 번 사용되지 않는다.
- 각 화면에는 정보·상태·행동·규칙 연결이 하나 이상 존재한다.
- 확정 행동이 있는 화면에는 미리보기 행동도 존재한다.

## 첫 평창군 화면 영역

- 농장 출하 준비: 화주 역할, 의뢰 단계
- 진부면 물류 거점 입출고: 창고 관리자, 입고·검수·보관 단계
- 화물 의뢰·배차: 화주, 의뢰·견적·배차 단계
- 상차·운송·하차: 기사, 수락·상차·운송·하차 단계
- 마트 상품 발견·비교·주문: 주문자, 발견·비교·참여 단계
- 음식점 주문 수신·준비: 음식점, 수신·준비·픽업 단계

각 영역은 업무 요약·현재 상태·다음 단계·판정 근거·표현 한계·확정 후 원장 재조회 상태를 가진다. 조회·Preview는 Command를 소유하지 않는다. Confirm 후보는 `Preview + 명시적 확인 + 기대 개정 번호 + 서버 Command 키`가 모두 있어야 한다.

## Unity 인계

```text
UI 기획 원장
→ Simulation 상태 사본과 권한 해석
→ `SimulationWorldUIProjection` 공유 계약
→ Unity ScreenModel / PresentationModel
→ World HUD·선택 정보판·업무 상세판
→ 의미 기반 UI Theme/Profile
```

Unity는 이 기획을 그대로 모바일 화면으로 복사하지 않는다. 역할·업무 단계·정보 우선순위는 유지하되 World HUD, 객체 선택 정보판과 과업 상세판으로 재배치한다. Command 성공 후 같은 원장을 재조회해 새 상태를 표시하며, 버튼·animation·NPC 도착만으로 완료하지 않는다.

공유 Projection 계약은 UI 기획 개정·업무 규칙 대장 개정·Session·상태 개정·WorldTick을 함께 운반한다. 정보 항목에는 자료 상태·출처·관측 시각·한계가, 행동에는 활성 여부·차단 사유·Preview/Confirm 조건이, 규칙 근거에는 원본 객체–업무 규칙 연결과 시설 기능이 포함된다.

## 첫 수직 완결 단위: 진부면 물류 거점 검수–적재

첫 구현은 `ui-surface:sim:pyeongchang:hub-operations` 하나만 런타임 관점별 조회 결과로 조립한다. 이 고유 식별자의 `hub` 문자열은 기존 계약 호환을 위해 유지하지만 사용자에게는 `진부면 물류 거점`으로 표시한다.

```text
도착 화물 상태 사본
→ 정보판 GET
→ 입고 검수 Preview 버튼
→ 명시적 Confirm 버튼 + 기대 상태 개정 번호
→ NPC 배정·이동·검수 WorldTick
→ 적재 대기 재고
→ 적재 Preview·명시적 Confirm
→ 적재 NPC 배정·이동·적재 WorldTick
→ 적재 완료 재고
→ 같은 정보판 GET 재조회
```

정보판 조회 경로는 `GET /api/simulation/v1/sessions/{sessionStableId}/world-ui/surfaces/{surfaceStableId}`다. 각 행동은 HTTP 방식, route template, 요청·응답 계약 키, 대상 화물 식별자와 개정, 담당 NPC, 기대 Session 개정, 확정 뒤 재조회 경로를 함께 제공한다. Unity는 이 값을 사용해 요청을 만들며 화물 개정이나 Session 개정을 자체 계산하지 않는다.

- 도착 전에는 Preview·Confirm을 `SimulationWorldUiInboundFreightNotReady`로 비활성화한다.
- 적격 입고 검수 NPC가 없으면 `SimulationWorldUiInboundInspectorNotAvailable`로 비활성화한다.
- Confirm 뒤에는 즉시 `InProgress`를 재조회하고 `Scheduled → Navigating → Working`을 표시한다.
- 검수 완료는 `PutAwayPending`으로 표시하며, 적재 NPC 작업까지 끝나 `PutAwayCompleted`가 된 뒤에만 `Completed`를 표시한다.
- Projection은 `WorkflowCode`, 현재 업무 단계, `ExecutionModeCode`, canonical 행동 코드를 포함해 Unity가 표시 상태와 실행 계약을 혼동하지 않게 한다.
- 모든 결과는 `SimulationOnly`이며 실제 입고·검수·운영 재고를 변경하지 않는다.

Unity 구현은 `SimulationWorldShell`의 `district:logistics` 선택과 연결된 오른쪽 정보판으로 완결했다. Projection은 `figma-maui-warehouse.v1`, `WorldSidePanel`, `Role.Warehouse`, `State.*`, `Information.*`, `Action.*` 의미 키를 제공하며 Unity의 Theme Catalog가 Figma·MAUI 창고 계열 색과 UGUI 표현으로 해석한다. 서버는 색상값, UGUI 계층이나 Synty 자산을 소유하지 않는다.

기본 Scene의 Composition Root는 Simulation 서버 저장소를 사용한다. Preview는 현재 개정을 변경하지 않고, Confirm과 WorldTick 뒤에는 같은 정보판을 다시 조회한다. 네트워크 실패 시 마지막 성공 상태를 stale로 유지하며 fixture로 자동 대체하지 않는다. 결정적 fixture는 테스트와 화면 검증에서만 명시적으로 주입한다.

2026-08-14 기준 서버 HTTP 수직 시험 1/1, Unity EditMode 3/3, 저장 Scene의 실제 UGUI 버튼을 누르는 PlayMode 1/1을 통과했다. 실제 Editor Play Mode에서 검수부터 적재 완료까지 진행한 Game View를 보존했다. 해당 PNG는 fixture 화면 표현 증거이며 실제 Simulation 서버 연결 성공이나 운영 업무 완료의 증거는 아니다.
