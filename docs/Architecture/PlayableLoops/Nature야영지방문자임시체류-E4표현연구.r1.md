# Nature 야영지 방문자 임시 체류 E4 표현 연구

## 식별과 상태

- 연구 고유 식별자: `study:nature-camp-visitor-stay:presentation.r1`
- 대상 PlayableLoop: `playable-loop:nature-camp-visitor-stay.v1`
- 대상 WI: `WI-COMMUNITY-VISITOR-STAY`
- 연구 revision: `nature-camp-visitor-stay.presentation-study.r1`
- 상태: `Accepted`
- 승인 근거: Q-207~Q-209의 확정 결정, 2026-08-30 보유 Synty Prefab·AnimationClip 파일 재고 확인, 그리고 현재 스레드의 연속 구현 승인
- 이 문서의 한계: E4 구현 준비 기준선이다. 실제 Prefab 로드·좌표·Renderer·Collider·Animator·Play Mode·Game View 증거가 아니다.

## 연구 질문과 플레이어 문맥

플레이어가 방문자 카드에서 `결정 대기`, `임시 체류`, `거절`을 구분할 때 어떤 H 기준점과 보유 자산 후보를 사용해야 하는지 정한다. 방문자는 안전 경계 바깥에 방치되거나 생활 중심부에 바로 생성되지 않고, 출입구와 생활 중심부 사이의 완충 위치에서 짧은 마중 표현과 함께 읽혀야 한다.

## 변경하지 않는 권위 경계

- 방문자 상태, 수용 칸, 마음 계보, WorldRevision과 행위 기록은 Simulation이 소유한다.
- 표현 준비 투영은 `Simulation공동체방문자응대CardSnapshot`과 같은 revision만 읽는다.
- Prefab·Animator·H 기준점은 수용·거절을 Confirm하거나 방문자 상태를 추론하지 않는다.
- 임시 체류는 정식 편입이 아니며, 거절은 선악·호감 점수로 변환하지 않는다.

## H 적용 기준선

| 권위 상태 | 위치 독립 H 능력 후보 | 배치 의도 |
| --- | --- | --- |
| `AwaitingDecision` | `Spatial.VisitorWaitingAnchor` | Nature 안전 경계 안쪽이면서 생활 중심부와 출입구 사이의 완충 위치. 주 통행선을 막지 않는다. |
| `TemporaryStay` | `Spatial.GuestRestAnchor` | `h1-stock:nature-shelter`의 손님용 점유 Slot 후보. 기존 플레이어 수면 기준점을 덮어쓰지 않는다. |
| `Rejected` | `Spatial.VisitorDepartureAnchor` | 출입구 방향 이탈 경로의 시작 후보. 거절 직후 강제 순간이동이나 장시간 연출을 요구하지 않는다. |

`h1-stock:nature-shelter`는 현재 `IdeaInventory`이며 실제 지역 권위가 아니다. 이번 연구는 위 세 능력을 후보로 결속할 뿐 H1을 자동 승격하거나 좌표를 확정하지 않는다. E5에서는 이동 표면과 주 통행 폭을 보존하고, Actor 발바닥과 침상 하단 Bounds가 지면 아래로 내려가지 않는지를 공통 표현 검증 모듈로 측정한다.

## 보유 Synty 자산 후보

| 역할 | 주 후보 | 대체 후보 | SHA-256 또는 판본 |
| --- | --- | --- | --- |
| 방문자 Actor | `Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Male_01.prefab` | `Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Female_01.prefab` | `0CCA4FF0779B9D40A106B09F8038360C92BC25C4BEC39A6BE89CDB5C3C465BD1` / `BCA2397C491C4AE6F026390031E48B9603CF5919AECB4019DB50D44373AF2C31` |
| 출입구 대기 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Bench_01.prefab` | `Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Shelter_01.prefab` | `0BA1C30D549163F8491AF089B463C8D9882DCC5B7C66C566BDD8C1D114EDBAF3` / `C88AC76FD4B8FB2178395DC2073B84B67AE485F04649FEFB774B52D4BC84258B` |
| 손님 침상 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Bed_01.prefab` | `Assets/Synty/PolygonTown/Prefabs/Props/SM_Prop_Bed_Single_01.prefab` | `53BC5CAB842052C0E0F30935462708AAF35531974E17B0CA625A586AADE82DEC` / `5222AA75718A8B8184BCA18F2AEE871594887AA0344BF88D4348B18E08D855B0` |
| 출입구 표식 | `Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_SignPost_01.prefab` | `Primitive:VisitorWaitingMarker` | `6FD69A832C14B6D7DDDA63F29E903E8F96BFFB907AD977180B8E52A29B70ADA3` / `fallback.r1` |

자산이 로드되지 않거나 Bounds·Rig 검증에 실패하면 `Primitive.CommunityVisitorMarker`와 기존 방문자 카드만 사용한다. fallback은 권위 기능을 축소하지 않고 E5 시각 증거만 차단한다.

## 상태별 VisualKey와 조립

| 상태 | VisualKey | H1 조립 | H2·H3 확장 관계 |
| --- | --- | --- | --- |
| 결정 대기 | `Community.Visitor.Stay.AwaitingDecision` | 방문자 Actor + 대기 기준점 + 선택적 벤치·표지판 | Q-207의 방문 증가가 실제로 승인될 때만 공동 숙소 H2·방문자 생활 구역 H3 후보로 확장한다. |
| 임시 체류 | `Community.Visitor.Stay.TemporaryStay` | 방문자 Actor + 손님 침상 Slot | 기존 플레이어 침상과 별도 점유로 유지한다. |
| 거절 | `Community.Visitor.Stay.Rejected` | 방문자 Actor + 이탈 기준점 | 장기 공간을 생성하지 않는다. |

## 애니메이션 기준선

- `AnimationRole`: `VisitorArrival`
- `ActionCue`: 결정 대기에서 `Visitor.Waiting.Greet`, 결정 완료 후 `Visitor.State.IdleOrDepart`
- 주 Clip: `Assets/Synty/AnimationEmotesAndTaunts/Animations/Polygon/Masculine/Greet/A_POLY_EMOT_Greet_Wave_Masc.fbx` (`F5CF194EF3DBD0ED8EC0822071566702C02685296A4B4C9D939847021AB61B23`)
- 대체 Clip: `Assets/Synty/AnimationEmotesAndTaunts/Animations/Polygon/Feminine/Greet/A_POLY_EMOT_Greet_Wave_Femn.fbx` (`39F74544554682A24B7CEC6347BA69A18D739FDAFE2033BCD801B05A45E2F2BF`)
- 대기 fallback: `Assets/Synty/AnimationEmotesAndTaunts/Animations/Polygon/Masculine/Base/A_POLY_EMOT_Base_Idle_Masc.fbx` (`E0BFDB66BCFB88BD76DAA7B77054210D100E4AA669AE58447E5C57D6CCEF0`)
- root motion은 E4 후보에서 사용하지 않는다. 실제 이동은 권위 방문자 상태와 이동 Adapter가 E5 이상에서 별도로 결속한다.
- Controller·Avatar·Rig 호환, Clip 길이, 중단·복귀 전이는 실제 대상 Prefab을 로드하는 E5 전에 검증한다. 실패하면 정적 Actor와 카드 피드백을 사용한다.

## 구현·검증 기준

E4 자동시험은 다음을 증명한다.

1. 같은 WorldRevision의 방문자 카드만 한 표현 준비 계획에 들어간다.
2. 상태별 H 능력·VisualKey·자산 후보·ActionCue가 결정적으로 결속된다.
3. 입력 순서가 달라도 방문자 정렬과 계획 hash가 같다.
4. 미승인 상태 Binding은 primitive fallback을 사용한다.
5. 모든 결과는 `PresentationOnly=true`, `MutatesCanonicalState=false`, `CanConfirmAuthority=false`다.

E5 전에는 실제 Prefab GUID·Renderer·Collider·Bounds, Actor Rig·Avatar·Controller, `SimulationWorldShell`의 기준점과 이동 표면을 검증한다. E6 전에는 지면 관통·부유, Player 대비 크기, 통행 방해, 카메라 가림과 상태 차이를 확인한다. E7은 실제 입력·Play Mode·Game View·Console과 결과 재조회를 요구한다.

## 무효화 조건

- 방문자 카드 상태 코드나 수용/거절 권위 의미가 바뀐다.
- Q-207~Q-209의 H 성장·완충 위치·짧은 마중 원칙이 바뀐다.
- 후보 Prefab·Clip의 path 또는 SHA-256이 바뀐다.
- 실제 E5 검증에서 후보의 Rig·Bounds·Collider·시인성이 부적합하다.
- `h1-stock:nature-shelter`가 다른 점유·출입 계약으로 승인되어 손님 Slot을 수용할 수 없게 된다.
