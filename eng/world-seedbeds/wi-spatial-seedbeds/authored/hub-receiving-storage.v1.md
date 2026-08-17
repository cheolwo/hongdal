# Hub 입고·보관 공간 모판

- 공간 모판: `wi-spatial-seedbed:hub-receiving-storage.v1`
- 포함 WI: `WI-LOG-04`, `WI-LOG-05`, `WI-001`, `WI-002`
- 검토 상태: `ApprovedForSimulation`

도착 화물은 하차영역에서 검수영역으로 인계되고 검수를 통과한 재고만 보관영역에 적재된다. `WI-LOG-05`는 별도 Confirm API를 만들지 않고 기존 `WI-001` 부모 명령 안의 자동 인수 계보를 유지한다.

검수·하차 작업영역은 각각 한 건의 작업 용량을 갖고 보관영역은 기존 Scenario 규칙과 같은 `10000 KGM`을 제공한다. 물류 Station·상하차 Dock·화물 대기 야드는 경관 구성 후보이며 실제 진부 Hub 시설의 존재 근거가 아니다.
