# Urban Logistics Center Primitive Vertical Slice

도심 물류센터의 공유 World, 운송자 Role Perspective, 작업자 NPC와 물류 거점 사이를 이동하는 Truck projection을 한 Scene에 배선하는 sample이다.

## 포함 범위

- 입고 Dock, 분류 Zone과 출고 Dock primitive
- `transport:71`, pickup·dropoff stable-ID Role View socket
- `vehicle-gate`, `loading-bay`, `vehicle-exit` semantic waypoint
- `npc:transport-driver.71` 운송자 NPC와 NavMeshAgent
- `network.logistics-center` → `network.warehouse` 운송 회랑 waypoint
- `truck-projection:cargo:transport-71` TruckView와 cargo VisualRoot
- 차량 접근 → 입고 Dock → 검수·입고 처리 → 보관 위치를 한눈에 보여주는 물류센터 overview
- 건물과 화물의 독립 `VisualRoot`, 네 업무 영역의 상태 material binding
- Role Perspective, NPC movement와 cargo handoff simulated API client
- VContainer composition root
- primitive Scene Builder와 저장 후 wiring validator

먼저 `Zone NPC Movement Sockets` sample을 import한 뒤 이 sample을 import한다. Scene Builder 실행 후 Unity Navigation 도구로 Ground NavMesh를 bake해야 NPC가 실제로 이동한다. 현재 builder는 프로젝트별 Navigation package와 bake 설정을 임의로 변경하지 않는다.

```text
Ssalddel/Samples/Create Urban Logistics Center Primitive Scene
Ssalddel/Samples/Validate Urban Logistics Center Primitive Scene
```

simulation fixture는 operational data가 아니다. 실제 연결 시 LifetimeScope의 simulated API client를 UnityWebRequest adapter로 교체한다. 서버가 `InTransit`으로 판정한 화물 인계만 Truck movement로 투영하며, 도착 후에는 canonical handoff를 다시 조회해 TruckView를 숨긴다. 시설 overview도 같은 handoff 조회 결과만 사용한다. 현재 canonical 상태에는 독립 검수 진행 상태가 없으므로 임의 상태를 만들지 않고, `ReceivingCompleted`일 때 검수 영역을 완료로 표시하고 보관 위치를 활성화한다. NPC나 트럭의 Unity 도착은 animation만 실행하며 상차·하차·입고 완료 endpoint를 호출하지 않는다.

City Pack 도입 시에는 builder와 업무 모델을 바꾸지 않고 `WarehouseBuildingVisualRoot`, 네 영역의 `VisualRoot`, `FacilityCargoVisualRoot` 아래 외형만 prefab 또는 variant로 교체한다.
