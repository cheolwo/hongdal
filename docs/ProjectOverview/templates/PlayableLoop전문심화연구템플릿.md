# [PlayableLoop 이름] [분야] 전문 심화 연구

## 식별과 상태

- 연구 고유 식별자: `study:[playable-loop-name].[building|spatial|placement|animation].r1`
- 대상 PlayableLoop: `playable-loop:[name].v1`
- 대상 WI:
  - `WI-...`
- 분야: `Building | Spatial | Placement | Animation`
- 연구 revision: `[name].[field].study.r1`
- 상태: `Draft`
- 공통 기준 문서:
  - `docs/...`

## 연구 질문과 플레이어 문맥

- 이번에 결정할 질문:
- 플레이어가 이 결정을 체감하는 순간:
- 플레이어 선택·대가·성공·실패·회복과의 관계:

## 현재 재고

- 관련 코드와 계약:
- 관련 H 정의와 Scene:
- 사용 가능한 자산과 보류 자산:
- 기존 시험·Runtime·Game View 증거:
- 확인되지 않은 가정:

## 변경할 수 없는 경계

- Simulation 권위와 WI 책임:
- Save/Replay·LocalProcess/RemoteHost 호환:
- H Capability와 공간 의미:
- 성능·플랫폼·자산 라이선스 경계:

## 대안 비교

| 대안 | 플레이 경험 | 구현·검증 비용 | 위험 | 판정 |
| --- | --- | --- | --- | --- |
| A |  |  |  |  |
| B |  |  |  |  |

## 선택한 기준선

- 선택:
- 선택 이유:
- 측정 가능한 기준:
  - 치수:
  - 거리·시간:
  - 밀도·수량:
  - 접촉점·전이 구간:
- 자산 연결:
- 대체 표현과 보류 사유:

## 분야별 구체 검토

### Building

- 유형·규모·바닥 면적:
- 출입구·내부 구역:
- 건설·손상·복구·성장 상태:
- 카메라·Collider·NavMesh 경계:

### Spatial

- 진입·수행·회복·귀환 동선:
- 시야선·방향 인지·카메라 판독:
- 지형 경사·평탄화 범위:
- 혼잡·간격·이동 시간:

### Placement

- 실내·실외 배치 역할:
- Surface·Slot·Clearance:
- 권위 상태별 배치 변화:
- 결정적 Seed·Plan Hash·fallback:

### Animation

- `WorldInteractionId`와 플레이어 판독 순간:
- `AnimationRole`·`ActionCue`:
- 준비·수행·결과·취소 상태:
- 원천 팩과 주·대체 Clip 후보:
- Clip revision·GUID·fingerprint:
- Rig·Avatar·Retarget 호환:
- root motion 적용·reconcile 정책:
- 프로젝트 Controller·AnimationAdapter 결속:
- 손·도구·대상 접촉점:
- Task 시간·접촉 Window·Audio·FX 동기화:
- 취소·피격·시점 전환 뒤 귀환:
- procedural·정적·UI fallback과 사용 표시:
- Animation Event가 권위 결과를 만들지 않는 검증:

해당하지 않는 분야 절은 `NotApplicable`과 이유를 적는다.

## 재결속 영향

- Logic에서 다시 열 E와 이유:
- Presentation에서 다시 열 E와 이유:
- 변경되는 WI·H·엔진 인계:
- 변경되는 저장·상태 사본:
- 다른 전문 연구와의 의존·충돌:

## 검증과 무효화

- Fixture·자동시험:
- Unity 구조 검증:
- 실제 입력·Play Mode·Game View 검증:
- 사람이 확인할 질문:
- 이 연구를 다시 열 조건:

## 검토와 승인

- 남은 미정:
- 검토자 또는 근거:
- 승인 근거 참조:
- 승인 상태: `Draft`
