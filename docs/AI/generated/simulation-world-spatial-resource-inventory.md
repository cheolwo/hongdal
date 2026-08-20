# Simulation World H 공간 구성 재고

> 이 문서는 `eng/world-seedbeds/spatial-resource-inventory/catalog.v1.json`에서 결정적으로 생성된다. 직접 수정하지 않는다.

- 재고 개정: `simulation-world-spatial-resource-inventory.r8`
- 계열 의미: 모판은 H1 하나의 이름이 아니라 H1~H4를 상향 조립하는 공간 구성 자원 계열이다.
- 축 구분: H는 공간 자원 종류, 재고 상태는 후보·승인·배정·배치, E는 구현·통합 증거 깊이다.

## 공간 구성 재고

| 계층 | 사람 중심 명칭 | 기술 자원 | 설계 재고 | 현재 정의 |
| --- | --- | --- | ---: | ---: |
| `H1` | 작업공간 모판 | `WiSpatialSeedbed` | 84 | 5 |
| `H2` | 블록 모판 | `LandscapeBlock` | 35 | 0 |
| `H3` | 경관 모판 | `LandscapeGraph` | 18 | 5 |
| `H4` | 지역 모판 | `AreaSet` | 6 | 1 |

```text
H1 작업공간 모판 재고
  → H2 블록 모판 재고
    → H3 경관 모판 재고
      → H4 지역 모판 재고
```

## 정의 재고와 배치 재고

- 설계 재고는 위치 독립적인 후보·승인 참조다. Unity Prefab이나 절대좌표가 공간 권위를 갖지 않는다.
- 현재 정의는 각 H 기술 자원의 실제 버전 관리 정의다. H3·H4 정의가 있어도 실제 H2가 없으면 E5가 아니다.
- 배치 상태 `Unallocated / Allocated / Placed`는 정의 상태와 별도이며 아직 이 대장에서 실제 배치 수량을 꾸며내지 않는다.
- 상위 재고는 하위 재고의 정확한 revision과 결정적 hash를 참조하고 사람 검토 없이는 권위 정의로 자동 승격되지 않는다.

## 호환 경계

기존 WI 공간 모판, LandscapeBlock, LandscapeGraph, AreaSet의 stable ID·schema·공개 계약은 유지한다. H 코드는 이 공통 재고 대장에서만 계산하며 기존 실행 JSON과 저장 상태에 중복 저장하지 않는다. Unity 배치 객체 모판과 규칙 실험 모판은 각각 표현·시험 adapter로 남고 H 공간 자원으로 자동 편입되지 않는다.
