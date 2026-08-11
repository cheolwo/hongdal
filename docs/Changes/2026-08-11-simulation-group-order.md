# Simulation 같이주문 명시적 의향·모집 결과 원장

## 범위

- `SIM-GROUP-ORDER-1`
- 참여자별 명시적 의향과 목표 충족 판정
- 모집 결과 Confirm·WorldTick·save/replay

## 결과

같이주문은 참여자 수와 총수량만 남기지 않고 각 참여자의 의향 stable ID, 참여자 stable ID, 희망수량, 단위, 명시적 동의와 source lineage를 보존한다.

```text
참여자 3명 × 감자 20kg
  → Preview: 60kg·목표 충족·확정대기 후보
  → Confirm: 모집 결과 Task 예약
  → WorldTick: 확정

참여자 2명 × 감자 15kg
  → Preview: 30kg·목표 미달·수요수집중
  → 모집 결과 Confirm
  → WorldTick: 모집종료목표미달
```

동의 없는 의향과 같은 참여자의 중복 의향은 자동 합산하지 않는다. 실제 주문·결제·계약·자동 참여 동의는 만들지 않는다.

## 검증

- Simulation 같이주문 집중 테스트: 6/6 통과
- 공통 업무 규칙 parity 테스트: 10/10 통과
- Simulation 전체 회귀 테스트: 184/184 통과
- scoped Fast·Task: `git diff --check`, `Ssalddel.v0.0.slnx` build, 자동 선택 parity 10/10 통과
- 화면 변화: 없음
- Unity runtime 검증: 대상 아님
- 운영 API 호출·실제 주문·결제: 수행하지 않음
- commit·push: 수행하지 않음
