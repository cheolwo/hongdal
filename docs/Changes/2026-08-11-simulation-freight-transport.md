# Simulation 화물운송 의뢰·배차·상하차·인수 원장

## 범위

- `SIM-FREIGHT-TRANSPORT-1`
- 기존 `LOGISTICS-MOVEMENT-1` Cargo 이동과 화물운송 업무 상태 결합
- 차량 용량 검증, 인수 확인, save/replay

## 결과

감자 Cargo 300kg의 Lot 계보와 원재고 예약은 기존 물류 이동이 계속 소유한다. 새 화물운송 원장은 같은 Cargo에 가상 운송 의뢰, 배차 후보, 400kg 차량과 상태 전이 원인을 연결한다.

```text
운송 Preview — 원장 무변경
  → Confirm
  → 배차대기 → 매칭중 → 배차확정
  → WorldTick
  → 상차지도착 → 상차완료 → 운송중
  → 목적지 WorldTick
  → 하차지도착
  → 인수 Preview·Confirm
  → WorldTick
  → 인수완료
```

차량 용량이 화물보다 작으면 Preview가 차단된다. 목적지 도착이나 차량 표현만으로 인수완료가 되지 않는다. 실제 기사 배정, GPS, 운임 정산과 알림은 Simulation 제외 효과로 유지한다.

## 검증

- 화물운송·물류이동·저장재생 집중 테스트: 21/21 통과
- Simulation 전체 회귀 테스트: 178/178 통과
- scoped Fast: `git diff --check`, `Ssalddel.v0.0.slnx` build 통과
- scoped Task: build 통과, 전체 4,505개 중 기존 비관련 route·metadata·CSS 기대 7건 실패
- 화면 변화: 없음
- Unity runtime 검증: 대상 아님
- 운영 API 호출·실제 기사 배정·결제·정산: 수행하지 않음
- commit·push: 수행하지 않음
