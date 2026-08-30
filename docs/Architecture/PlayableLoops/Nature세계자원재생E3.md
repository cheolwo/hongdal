# Nature 세계 자원 재생 — Logic E3

## 식별과 근거

- PlayableLoop `playable-loop:nature-night-day2.v1`, 주제 `topic:nature-night-day2.v1`.
- revision `nature-resource-regeneration.design.r1`, 상태 `Approved`.
- 승인: 2026-08-30 사용자의 [남은 32개 WI 순차 Logic E3 캠페인](신규32개WI논리E3캠페인.md) 요청.
- WI `WI-WORLD-RESOURCE-REGENERATE`; [문답 Q036~038](PlanningSessions/nature-night-day2.inquiry.r1.md)의 재성장·Tick 권위·토지 용도 제외를 사용한다. Q038은 기존 결정 소비이며 토지 변경 WI를 구현하지 않는다.

## 플레이어 약속과 재미

벌목했던 자연 자리에서는 시간이 지나 자원이 다시 자란다. 적합한 빈 자연 셀에는 새 식물이나 환경 자원이 생길 수 있다. 건설·도로·평탄화 부지에 나무가 임의로 끼어들지 않는다. 화면의 성장 보간은 채집 가능 여부를 바꾸지 않는다.

## 반복 폐루프

이미 소진된 자원·빈 자연 셀 → 권위 Tick → 재성장 또는 정책상 신규 생성 → 채집 가능 상태 조회 → 기존 채집 WI의 다음 선택. 이번에는 자원 재생 부분만 독립 Core로 시험한다.

## 선택·대가·성공·실패·회복

신뢰된 초기 상태에 세계·세션·자동 전이 주체·seed, 자원 Profile, 적합 셀과 기존 노드를 동결한다. Profile은 단계 수·단계 간 Tick·생성 확률 백만분율·Tick당 생성 상한과 Plant/Loose 종류를 명시한다. 시험 Fixture 수치는 출시 밸런스가 아니다.

셀은 이미 승인된 한 자원 자리이며 하나의 노드만 점유한다. 실제 지형의 셀 적합성·좌표·범위 산출은 구현하지 않는다. 자연 외 LandUse는 기존 노드 성장과 신규 생성 모두 제외하며 기존 노드를 삭제하지 않는다. WorldSeed·셀·Profile·Tick의 SHA256에서 후보 우선순위와 확률을 결정하고 고유 식별자로 동률을 정리한다. 기존 노드는 위치·ID를 유지한다.

권위 호출만 연속한 다음 Tick을 적용한다. 누락·역행 Tick, 다른 주체, 낡은 revision과 동일 TransitionId의 다른 입력은 무변경 거부한다. 같은 입력 재전송은 최초 결과를 반환한다. Tick 자체의 소비도 상태 변화이므로 자원 변화가 없어도 평가 기록과 revision을 한 번 남긴다. 일부 후보의 정수 범위 오류는 전체 Tick을 무변경 거부한다. 정상 정책을 새 원장에 명시적으로 준비하거나 호출을 수정해 재시도한다.

## WI 단일 책임 후보

`AutomaticTransition / WorldDerived`이며 플레이어 Confirm 버튼을 요구하지 않는다. `PreviewTick`은 내부 읽기 전용 진단이고 `ApplyTick`은 신뢰된 권위 호출 경계다. 재성장·새 식물·환경 묶음은 자원 가용성 갱신이라는 같은 결과의 Profile이다. 벌목·채집·소비·LandUse 변경은 호출하지 않는다.

## 논리·표현 요구

Logic E1 계약 → E2 결정적 Profile·노드·Tick 규칙 및 Application → E3 무변경·멱등·거부·경합·순서 독립·정수 경계·행위 원장 시험. Presentation E1은 성장 단계와 채집 가능 상태를 별도로 판독해야 한다는 요구만이다. 통합 E1이며 기존 다른 WI의 E3 증거는 보존한다.

## H 공간과 자산 요구

실제 공간 배치는 없다. 신뢰된 셀·정수 좌표 입력만 사용한다. H 경계·식생 Prefab·LH·실외 배치와 성장 보간은 후속 E4 연구·승인으로 분리한다. 지원 worktree를 자동 통합하지 않는다.

## 전문 심화 연구 판정과 재결속

순수 결정적 정책 소비·노드 전이 시험은 `NotRequired`다. 문답에서 요청한 실제 셀 적합성·공간 제외 반경·밀도 및 성장 기간 밸런스·Migration·표현 연구는 미해결이며 이 기획으로 확정하지 않는다. 해당 생산 정책과 실제 셀 생성기는 후속 revision에서 연구 기준선을 결속한다.

## 저장·권위·외부 경계

기존 Nature의 Cycle+3와 Save를 변경하지 않는다. 단일 메모리 원장 수명에서만 멱등성을 보장한다. Session WorldTick·Local/Remote·Save·채집 Adapter는 연결하지 않는다. 플레이어 행위가 아니므로 성장 적용은 `NotApplicable:WorldDerivedResourceRegeneration`이다. 매 Tick의 행위 기록·revision과 자원 변경은 원자적이다.

## 제외 범위와 승인

상한 Logic E3 / Presentation E1 / 통합 E1. 실제 장면·수명주기·오디오·Runtime·저장·공간 적합성 산출·토지 용도 변경·자원 소진·Q340 이후·commit·push는 제외한다. 구현 가능한 정책 소비 규칙과 생산 정책 미정은 별도 기록한다.
