# 농장 작업마당 공간 모판

- 공간 모판: `wi-spatial-seedbed:farm-work-yard.v1`
- 포함 WI: `WI-FARM-05`, `WI-FARM-06`
- 검토 상태: `ApprovedForSimulation`

집하와 포장은 서로 다른 내부 작업영역을 사용한다. 각 `WorkArea = 1 slot`은 해당 작업을 동시에 한 건 예약하는 업무 용량이며 두 작업이 같은 물리 면적을 뜻하지 않는다.

생산구획에서 들어온 수확물은 집하영역에서 포장영역으로 이동한 뒤 상차영역 방향 연결구로 나간다. 헛간 작업마당과 농산물 집하장 구성은 E5가 Block 조건에 따라 선택한다.
