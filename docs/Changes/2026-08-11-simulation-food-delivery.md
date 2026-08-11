# Simulation 음식배달 조리·픽업·전달·수령 확인 원장

## 범위

- `SIM-FOOD-DELIVERY-1`
- 가상 음식 주문 Preview·Confirm
- 조리·픽업 준비·기사 후보·픽업·전달의 WorldTick 전이
- 전달 뒤 별도 주문자 수령 확인

## 결과

운영 음식 주문의 상태 의미를 재사용하되 운영 API와 저장소를 호출하지 않는 별도 Simulation 원장을 구성했다.

```text
주문 Preview
  → Confirm: 주문대기 + 생애주기 Task
  → WorldTick: 조리중
  → WorldTick: 픽업대기
  → WorldTick: 기사배정 후보
  → WorldTick: 픽업완료
  → WorldTick: 전달완료
  → 주문자 수령 Preview·Confirm
  → WorldTick: 수령확인
```

실제 기사·주소·GPS·결제·운영 주문 쓰기·실시간 알림은 생성하지 않는다. `기사배정`은 가상 후보 상태이고 `전달완료`만으로 주문자 수령을 자동 확정하지 않는다. 주문·메뉴·시설·배송권·주문자 stable ID, 수량, 기간, 전체 상태 이력과 source lineage는 저장·재생 해시에 포함된다.

## 검증

- Simulation 음식배달 집중 테스트: 6/6 통과
- 공통 업무 규칙 parity 테스트: 10/10 통과
- Simulation 전체 회귀 테스트: 190/190 통과
- scoped Fast: `git diff --check`, `Ssalddel.v0.0.slnx` build 통과
- scoped Task: build 통과, 기존 비관련 route·metadata·CSS 기대 7건으로 전체 4,501/4,508 통과
- 화면 변화: 없음
- Unity runtime 검증: 대상 아님
- 운영 API 호출·실제 주문·기사 배정·결제: 수행하지 않음
- commit·push: 수행하지 않음
