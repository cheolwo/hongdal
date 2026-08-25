# Hub 피킹·출고 준비 공간 모판

- 공간 모판: `wi-spatial-seedbed:hub-outbound-staging.v1`
- 포함 WI: `WI-HUB-03`, `WI-HUB-04`, `WI-HUB-05`
- 검토 상태: `ApprovedForSimulation`

보관 재고를 NPC가 피킹하고 차량 상차 전 `OutboundReady` 상태로 대기시키는 Hub 내부 공간이다. 피킹 공간과 출고 대기 공간은 각각 한 건의 작업 용량을 가지며, 연결된 화물 인계 회랑은 Player와 손수레가 통과할 수 있어야 한다.

출고 대기 공간은 `Spatial.OutboundStagingArea`, `Spatial.PackingWorkArea`, `Spatial.LoadingWorkArea`, `Spatial.CargoAccessible`, `Spatial.WorkerAccessible` 능력을 함께 제공한다. 이는 포장과 출고 준비를 같은 H1 업무 구역에서 닫기 위한 Scenario 능력이며 실제 시설 인증을 뜻하지 않는다.

`WI-HUB-06` 차량 상차는 외부 연결 Stub일 뿐 이 모판의 완료 조건이 아니다. Synty 소품 배치는 표현이며 공간 능력과 업무 완료 권위는 Simulation의 승인 정의와 WorldTick에 남는다.
