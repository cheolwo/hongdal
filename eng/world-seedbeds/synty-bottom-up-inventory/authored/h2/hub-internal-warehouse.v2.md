# Hub 내부 입고·보관·출고 작업 블록

@spatial-knowledge h2-candidate:hub-internal-warehouse
@hierarchy H2
@state ApprovedReference
@required-h1 h1-stock:hub-receiving-storage
@required-h1 h1-stock:hub-outbound-staging

## 존재 이유

검증된 입고·검수·보관 H1과 피킹·출고 준비 H1을 한 창고 안에서 연결한다. 배치 문서가 H2를 직접 선언하지 않으며, 두 H1의 운영 상태·능력·용량·연결·배치 계획 hash가 규칙을 모두 통과했을 때 Simulation Core가 H2 인스턴스를 성립시킨다.

## 상향식 성립 조건

- `h1-stock:hub-receiving-storage`와 `h1-stock:hub-outbound-staging`이 모두 운영 가능해야 한다.
- 두 H1은 동일한 승인 `interior-placement-plan.v2`와 배치 계획 hash를 증거로 제공해야 한다.
- 입고 H1의 `Output`이 출고 H1의 `Input`으로 연결되어야 한다.
- 저장 용량은 최소 300 KGM, 출고 작업면은 최소 1 slot이어야 한다.
- 배치 검증 실패, 능력·용량 부족 또는 H1 운영 중단 시 H2는 `Blocked` 또는 `Degraded`가 된다.

## 상위 조립

성립한 H2 인스턴스는 `h2-candidate:hub-outbound-vehicle`과 함께 H3 진부 Hub의 하위 증거가 된다. 차량 H2가 없는 현재 범위에서는 H3는 성립하지 않으며 H4는 `PartiallyReady` 준비도만 가진다.

## 권위 경계

이 문서는 위치 독립 설계 지식이다. Unity 배치나 이 문서 자체가 H2를 성립시키지 않으며, WorldTick에서 Simulation 규칙이 판정한다. H4는 이번 판본에서 AreaSet 인스턴스를 자동 생성하지 않는다.
