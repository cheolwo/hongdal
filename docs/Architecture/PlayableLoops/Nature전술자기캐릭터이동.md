# Nature 전술 자기 캐릭터 이동

## 식별과 근거

- 주제 고유 식별자: `topic:nature-tactical-self-navigation.v1`
- PlayableLoop 고유 식별자: `playable-loop:nature-tactical-self-navigation.v1`
- 기획 revision: `nature-tactical-self-navigation.design.r1`
- 원천 기획 문서:
  - `docs/AI/DECISIONS.md` D-116
  - `docs/Architecture/UnityLandscapeRenderingPipeline.md`
  - `docs/Architecture/WorldTick과실시간실행경계.md`

## 플레이어 약속과 재미

- 플레이어가 처한 상황: 높은 사선의 전술 시점에서 세계와 자기 캐릭터를 함께 보고 있다.
- 플레이어가 원하는 것: 자기 캐릭터를 명확히 선택하고 직접 또는 목적지 명령으로 움직인 뒤 필요한 세계 물품에 도달한다.
- 반복해도 재미있어야 하는 핵심 선택: 캐릭터를 직접 움직일지, 카메라를 먼저 이동해 주변을 살필지, 우클릭 목적지와 방향키 직접 이동 중 무엇을 사용할지 선택한다.
- 짧은 플레이어 약속 한 문장: 자기 캐릭터를 클릭 또는 박스 드래그로 선택하고 우클릭·방향키로 이동하며, 자유 시야 뒤 Backspace 두 번으로 캐릭터에 돌아와 도끼를 획득한다.

## 반복 폐루프

`전술 시야 진입 → 자기 캐릭터 식별·선택 → 주변 시야 탐색 → 이동 방식 선택 → 도끼 지점 접근 → WI-NATURE-05 확정 → 도끼 소유 상태 확인 → 다음 행동 선택`

- 진입 상태: `NatureSafeClearingAvailable`, `PlayerReady`, `AxeAvailable`
- 종료 뒤 다시 열리는 선택: 도끼를 장착하거나 다른 위치를 탐색할 수 있다.

## 선택·대가·성공·실패·회복

- 선택지: 클릭 또는 박스 선택, 우클릭 목적지 또는 방향키 직접 이동, 자유 카메라 탐색 또는 즉시 재집중.
- 자원·시간·위험 대가: 표현 이동 자체는 자원과 WorldTick을 소비하지 않지만, 카메라를 멀리 옮기면 캐릭터 위치를 놓칠 수 있다.
- 성공 결과: 자기 캐릭터만 선택되고 도끼 지점에 도달해 기존 권위 WI로 도끼를 획득한다.
- 실패 결과: 빈 선택, 이동 불가 지형, UI·전투·배치 입력 잠금은 이동 명령을 만들지 않는다.
- 실패 뒤 회복 경로: 다시 선택하거나 Backspace를 두 번 눌러 캐릭터에 재집중하고 다른 이동점을 선택한다.

## WI 단일 책임 후보

| 순서 | WI 후보 | 한 번에 바꾸는 권위 상태 | 주체 | 비고 |
| --- | --- | --- | --- | --- |
| 1 | `WI-NATURE-05` | 도끼 인스턴스를 WorldPickup에서 플레이어 Inventory로 이전 | Player | 선택·카메라·표현 이동은 WI가 아니며 이 Confirm 전까지 권위 상태를 바꾸지 않는다. |

## 논리·표현 요구

- 논리적으로 반드시 성립할 상태와 규칙: 도끼 획득 Preview는 무변경이고 Confirm만 소유권·행위 기록·revision을 변경한다.
- 플레이어가 화면과 소리로 식별해야 할 대상: 자기 캐릭터, 선택 사각형, 선택 원, 목적지 표식, 도끼와 획득 결과.
- 결과가 같은 revision임을 보여줄 피드백: 도끼 표현 제거, 인벤토리 소유 상태와 행위 기록 cursor를 같은 권위 revision으로 읽는다.
- 공통 표현 검증 모듈 외 조건 모듈: `Actor`, `GroundSurface`, `CameraOcclusion`.

## H 공간과 자산 요구

- 필요한 H1~H5 능력: `Traversable`, `PlayerAccessible`, `ToolPickupPoint`가 결속된 Nature 안전 빈터 H1.
- 실외·실내 배치 요구: 플레이어와 도끼 사이에 선택 Raycast와 CharacterController 이동을 막는 승인되지 않은 Collider가 없어야 한다.
- Synty 자산 후보와 대체 표현: 기존 Synty 플레이어 VisualRoot와 `SM_GEN_Wep_Axe_01` 표현을 재사용한다.
- Traversal, Collider, NavMesh 요구: 기존 `공간안전이동Gate`, 지형 Collider와 CharacterController를 재사용하며 이번 판본은 NavMesh·온라인 위치 동기화를 추가하지 않는다.

## 저장·권위·외부 경계

- Simulation 권위 상태: `WI-NATURE-05` 도끼 소유권, 행위 기록과 WorldRevision만 권위다.
- Save/Replay에 고정할 값: 기존 `simulation-save.v29`의 도끼 소유·행위 기록을 사용하며 카메라 초점·선택 사각형·표현 위치는 추가 저장하지 않는다.
- LocalProcess/RemoteHost 동등성: 같은 도끼 획득 명령이 같은 결과와 canonical hash를 만들어야 한다. 표현 이동 좌표는 비교 대상이 아니다.
- 외부 Provider 또는 운영 효과 제외: 온라인 참가자 좌표·예측·보간·원격 아바타와 운영 계정 상태를 추가하지 않는다.

## 제외 범위와 승인

- 이번 주제에서 하지 않는 것: 네트워크 위치 동기화, 원격 플레이어 조작, 다중 유닛 명령, NavMesh 경로 탐색, 전투 명령, Farm 작업 확정.
- 검토할 사람 또는 근거: 2026-08-28 사용자 결정과 기존 D-116 표현 권위 경계.
- 승인 근거 참조: `D-273`
- 승인 상태: `Approved`
