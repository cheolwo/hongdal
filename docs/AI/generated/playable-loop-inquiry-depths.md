# PlayableLoop Inquiry Depth and Evidence Readiness

- Catalog revision: `playable-loop-inquiry-depth-catalog.r2`
- Evidence model: `horizontal-dual-cycle-evidence.r3`
- Inquiry depth forecasts implementation readiness and never promotes actual Evidence.

## Question branch revision

- Schema: `playable-loop-question-branch.v1`
- Revision format: `{questionStableId}.r{positiveInteger}`
- One branch revision contains one core question. An answer or a meaningful context/depth change creates a new revision instead of overwriting the previous one.
- A deeper question references its parent revision. Same-depth refinement may also create a child revision. Question revision never promotes Evidence automatically.
- Decision statuses: `Asked / ConfirmedDirection / Confirmed / Deferred / Superseded`
- Graph impact: assess the direct ring, explicit one-hop neighbors, and causally justified two-hop propagation. Effects beyond two hops become a follow-up question revision.
- Higher H changes are derived from child H and edge changes; unknown graph effects remain unknown and never become confirmed automatically.
- Placement Map impact: Graph Map keeps gameplay meaning; a separate versioned Placement Map keeps repeated H instances, relative arrangement, and placement constraints. Relative plans never prove Unity World placement.

| Depth | Question scope | Logic readiness | Presentation readiness | Horizontal campaign readiness | Evidence still required |
| --- | --- | --- | --- | --- | --- |
| `D1` 목적과 플레이어 약속 | 존재 목적, 플레이어 약속, 첫 적용 영역과 역할 | `E1` | `E1` | `-` | 승인 기획 revision·hash와 E1 작업 명세 검증 |
| `D2` 폐루프와 기본 계약 | 핵심 폐루프, 조작, 기본 규칙과 첫 임무 | `E2` | `E2` | `-` | 실제 Contract·Application·Unity 투영 코드와 경계 시험 |
| `D3` 손익과 회복 규칙 | 비용, 실패, 부상, 회복, 성장과 손익 | `E3~E4` | `E3` | `-` | 결정성·거부·회복 시험과 LocalProcess·RemoteHost 동등성 |
| `D4` 공간과 표현 조립 | 건물, H 공간, 배치, Synty 자산, UI와 애니메이션 | `E5` | `E4~E5` | `-` | 후보 fingerprint, H 결속, 실제 Prefab·World 배치·Renderer·Collider·상태 발현 |
| `D5` 실행 안정성과 영역 조화 | Save/Replay, NPC 연속성, 멀티플레이, 영역 통합과 Runtime 완료 조건 | `E6~E7` | `E6~E7` | `E8~E9` | 실제 입력·Play Mode·Game View·Save 재진입·반복 안정성·사람 승인 |
