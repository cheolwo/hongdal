# Nature 생활핵·조우·방어 공간 계획

## 목적

첫 네이처 실제 플레이 공간은 `h3-candidate:nature-home-encounter-defense`를 재사용한다. 새 H 재고를 늘리지 않고 기존 H1 10개, Nature 주도 H2 8개, Nature 주도 H3 5개를 감사한 뒤 플레이어가 걷고 돌아올 수 있는 한 H3를 먼저 닫는다.

이 문서는 H의 위치 독립 설계와 Unity의 compact 배치를 구분한다. Unity 배치는 표현 검증이며 실제 지역 좌표, 공공데이터 근거 또는 Simulation 결과의 권위가 아니다.

## 재고 감사 결론

선택한 H3는 다음 기존 재고를 사용한다.

| 계층 | 고유 식별자 | 공간 의미 |
| --- | --- | --- |
| H2 | `h2-candidate:nature-home-core` | 안전 생활핵·보급 거점 |
| H2 | `h2-candidate:nature-encounter-route` | 조우로·이탈로 |
| H2 | `h2-candidate:nature-defense-ring` | 방어환·야영지 |

현재 이론 공장은 세 H2를 배치했지만 기존 관계는 `생활핵 → 조우로 → 방어환` 두 개뿐이었다. 이번 계획은 `방어환 → 생활핵`의 `RecoveryHandoff`를 추가해 경관 이름과 실제 의미 관계를 일치시킨다.

## H3 Node·Edge·Connector

### Node

| Node | H2 | Unity compact 기준점 |
| --- | --- | --- |
| `h2-nature-home-core` | 안전 생활핵·보급 거점 블록 | `(0, 0, 0)` |
| `h2-nature-encounter-route` | 조우로·이탈로 블록 | `(22, 0, 24)` |
| `h2-nature-defense-ring` | 방어환·야영지 블록 | `(45, 0, 0)` |

Unity 좌표는 첫 걷기 검증을 위한 위치 독립 compact 표현값이다. 이론 공장의 Reference bounds나 실제 지역 좌표를 대체하지 않는다.

### Edge

| 이동 | 종류 | 방향 |
| --- | --- | --- |
| 생활핵 → 조우로 | `PlayerTraversal` | 단방향 의미 흐름 |
| 조우로 → 방어환 | `PlayerTraversal` | 단방향 의미 흐름 |
| 방어환 → 생활핵 | `RecoveryHandoff` | 단방향 복귀 흐름 |

물리 경로는 플레이어가 양방향으로 걸을 수 있지만, 의미 흐름은 탐색과 회복의 인과를 보존한다.

### Connector

| 역할 | 결속 위치 | 의미 |
| --- | --- | --- |
| `Ingress` | 생활핵 | H3 진입 |
| `Egress` | 방어환 | 다음 네이처 경관으로 이동 |
| `SafeCoreGate` | 생활핵 | 안전 복귀점 |
| `ExplorationOutput` | 생활핵 | 탐색 출발점 |
| `ThreatInput` | 방어환 외곽 | 위협 진입 방향 |
| `RecoveryReturn` | 생활핵 | 대응 후 복귀점 |

## Nature 팩 표현 범위

Unity는 `Assets/Synty/PolygonNature` 아래의 Terrain, Trees, Rocks, Plants, Props만 사용해 다음을 표현한다.

- Terrain: 세 H2 공터와 연결 지면
- Trees·Plants: 경계와 시야 조절
- Rocks: 조우로와 위협 진입 방향 표시
- Dust·Ground 계열 Terrain: 세 Edge의 숲길
- CampFire·Log·RoadSign: 생활핵·Trailhead의 장소성

몬스터 자산, 피해, 승패와 위협도는 이 공간 조립의 일부가 아니다. 활성 조우는 Simulation 서버가 결정하고 Unity는 `ThreatInput`과 대응 공터를 표현 위치로 사용한다.

## Game View 검토 표면

공식 실행 장면을 건드리지 않고 공간 비율과 시야를 검토하는 비공식 저장 장면을 둔다.

- 저장 장면: `Assets/Ssalddel/Scenes/NatureH3GameViewReview.unity`
- 조감 관점: 세 H2와 왕복 동선의 전체 비율을 확인한다.
- 생활핵 관점: 모닥불·통나무·반딧불로 머무름과 회복의 중심을 읽는다.
- 조우로 관점: 표지·부서진 아치·바위로 탐색 방향과 위험 진입을 읽는다.
- 방어환 관점: 바위 경계·횃불·공터로 대응 공간을 읽는다.
- 하천 관점: 오른쪽 앞에서 왼쪽 뒤로 굽이치는 하천과 안전한 다리·탐험적인 여울을 함께 읽는다.
- 이동 관점: `F3` 3인칭, `F2` 1인칭, `WASD` 이동을 제공한다.

하천은 기존 H3 의미 관계를 바꾸지 않는 위치 독립 표현이다. 생활핵→조우로는 다리, 방어환→생활핵 복귀는 여울로 읽히게 하되 실제 GIS 수계나 E6 근거로 주장하지 않는다. `RiverPresentationRoot`는 물·강변·바위·식생·효과를, `RiverTraversalRoot`는 단순 이동 차단 Collider와 두 횡단 개구부를 소유한다. 물 Mesh 자체의 Collider는 이동 규칙에 사용하지 않는다.

숲은 `OuterForestZone`, `RiverBankZone`과 생활핵·조우로·방어환·다리·여울의 제외 영역으로 결정적으로 배치한다. 나무·관목·바위 수량은 표현 목표이고, 배치 가능 영역·최소 이동 폭·고정 seed가 재생성 권위다. 다리·여울은 최소 4m, 세 H2의 주요 활동 공간은 최소 3m를 비운다.

카메라와 플레이어 이동은 표현 전용이며 서버 명령, `WorldTick`, `WorldRevision` 또는 H/E 성숙도를 변경하지 않는다. 검토 장면은 Build Settings와 canonical `SimulationWorldShell`의 공식 진입점에 편입하지 않는다.

## 완료 관문

- H3가 3 Node·3 Edge로 실제 순환한다.
- 여섯 Connector가 고유 식별자와 배치 기준점을 가진다.
- 모든 visible Prefab이 PolygonNature 원본과 연결된 instance다.
- 재사용 가능한 소형 H3 Prefab을 생성하고 구조를 자동 검증한다.
- 저장된 `SimulationWorldShell` 적용은 기존 Scene이 정상 로드되고 `WorldMapRoot`를 확인한 뒤에만 수행한다. 현재 batch load 오류가 있는 Scene을 재생성하거나 덮어쓰지 않는다.
- 플레이어 이동만으로 `WorldTick`·`WorldRevision`이 변하지 않는다.
- 물 표현과 이동 차단 계층을 분리하고 일반 하천은 차단하되 다리·여울은 통과시킨다.
- 하천·숲 보강은 Scene 결정성, EditMode와 PlayMode 자동 시험까지 검증한다. 기존 조감·생활핵·방어환 PNG는 보존하지만 이번 보강의 신규 Game View 캡처는 요구하지 않는다.
- 이 화면 증거는 위치 독립 H3 표현 검토이며 실제 몬스터·전투·서버 HTTP·공공데이터 E6 또는 canonical Scene 완성을 뜻하지 않는다.
