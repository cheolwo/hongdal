# Nature 생존 생활핵 공간 모판

- 공간 모판: `wi-spatial-seedbed:nature-survival-home.v1`
- 포함 WI: `WI-NATURE-05`, `WI-NATURE-07`, `WI-NATURE-08`, `WI-NATURE-09`, `WI-NATURE-10`, `WI-NATURE-12`
- H 결속: `h1-stock:nature-trailhead`·`h1-stock:nature-shelter` 후보 → `h2-candidate:nature-home-core` → `h3-candidate:nature-home-encounter-defense`
- 검토 상태: `ApprovedForSimulation`

플레이어는 안전 빈터에서 도끼를 얻고, 오두막 위치를 선택하고, 통나무를 사용해 건설한 뒤 실내외를 오간다. 이 선택과 확정은 각각 WI이며 건설 30초 진행은 `Task`, 완공은 `Effect`다.

`Tool`, `BuildingSite`, `WorkArea`, `Material`, `ShelterOccupancy`는 H1 위치 독립 용량 계약이다. 현재 Core가 벌목·건설 취소와 점유 충돌 반환을 모두 구현했다는 뜻은 아니며, 해당 규칙은 E4 후속 구현으로 남긴다. 실제 좌표·H2 배치·H3 이동 폐루프는 E5에서 별도로 검증한다.

`WI-NATURE-12`는 새 장소를 만드는 WI가 아니라 진행 중 작업의 기존 공간·예약 문맥을 이어받아 취소와 반환을 확정한다. 생활핵 모판의 `ActiveWorkReservationContext`는 이 상속 경계를 표현하며 실제 취소 대상 공간은 Runtime의 원 작업 기록으로 결정한다.
