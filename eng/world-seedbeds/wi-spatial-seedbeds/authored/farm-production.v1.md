# 농장 생산 공간 모판

- 공간 모판: `wi-spatial-seedbed:farm-production.v1`
- 포함 WI: `WI-FARM-01`, `WI-FARM-02`, `WI-FARM-03`, `WI-FARM-04`
- 검토 상태: `ApprovedForSimulation`

밭갈기·파종·재배 관리·수확은 같은 생산구획을 공유한다. `WorkArea = 1 slot`은 물리 면적이 아니라 이 생산구획에서 동시에 한 건의 농장 작업을 예약할 수 있다는 Simulation 용량이다.

수확 결과는 `harvest-handoff` 연결구로 작업마당에 넘긴다. 연결구는 방향 의미만 가지며 실제 지역의 위치·도로·회전은 E5에서 결정한다. 감자밭 두렁 A/B/C는 허용 가능한 경관 구성 후보이고 Simulation 능력의 근거가 아니다.
