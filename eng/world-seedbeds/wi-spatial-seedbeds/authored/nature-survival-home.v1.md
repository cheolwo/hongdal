# Nature 생존 생활핵 공간 모판

- 공간 모판: `wi-spatial-seedbed:nature-survival-home.v1`
- 포함 WI: `WI-NATURE-05`, `WI-NATURE-07`, `WI-NATURE-08`, `WI-NATURE-09`, `WI-NATURE-10`, `WI-NATURE-12`, `WI-NATURE-13`, `WI-NATURE-14`, `WI-NATURE-15`, `WI-NATURE-16`, `WI-NATURE-17`, `WI-CON-01`
- H 결속: `h1-stock:nature-trailhead`·`h1-stock:nature-shelter` 후보 → `h2-candidate:nature-home-core` → `h3-candidate:nature-home-encounter-defense`
- 검토 상태: `ApprovedForSimulation`

플레이어는 안전 빈터에서 도끼를 얻고, 오두막 위치를 선택하고, 통나무를 사용해 건설한 뒤 실내외를 오간다. 오두막이 운영 상태가 되면 보관·수면·새벽 계획을 수행하고 선택형 영역 건물을 생활핵 발자국 안에 배치할 수 있다. 운영 작업대에서는 플레이어가 `WI-NATURE-16`으로 직접 제작하거나 정책을 선택해 `WI-NATURE-17`을 Nature 거점 NPC에게 위임하여 다음 원정의 선택형 현장 보급을 제작한다. 직접 제작과 위임 제작은 같은 `CraftingWorkArea`·재료 예약을 공유하며, 시간 진행은 권위 작업 상태를 진행시킬 뿐 별도 WI가 아니다.

`Tool`, `BuildingSite`, `WorkArea`, `Material`, `ShelterOccupancy`, `ShelterStorage`, `ShelterSleep`, `DawnPlanChoice`, `CraftingWorkArea`, `ActiveWorkReservationContext`는 H1 위치 독립 용량·능력 계약이다. 이 승인 모판은 실행 문맥 E4 근거이며 실제 좌표·권위 Task·Effect·귀환과 H2/H3 조립을 자동으로 E5 완료시키지 않는다.

`WI-NATURE-12`는 새 장소를 만드는 WI가 아니라 진행 중 작업의 기존 공간·예약 문맥을 이어받아 취소와 반환을 확정한다. 생활핵 모판의 `ActiveWorkReservationContext`는 이 상속 경계를 표현하며 실제 취소 대상 공간은 Runtime의 원 작업 기록으로 결정한다.

벌목 장소에서 `WI-NATURE-18`로 바닥 통나무를 주운 뒤에는 `timber-pickup-return-input`을 통해 오두막 배치 또는 보관 선택으로 돌아온다. 이 연결구는 통나무 수량을 계산하지 않고, 권위 Inventory Effect가 완료된 뒤 플레이어가 다음 선택 공간으로 이동하는 경로만 표현한다.
