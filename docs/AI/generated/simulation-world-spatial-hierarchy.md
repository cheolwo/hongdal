# Simulation World 공간 포함 계층

> 이 문서는 `eng/world-seedbeds/spatial-hierarchy-levels.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 계층 대장 개정: `simulation-world-spatial-hierarchy.r1`
- 증거 단계 개정: `simulation-evidence-stages.r13`
- 축 구분: `E`는 증거 성숙도, `G`는 성숙도를 높이는 관리 체계, `H`는 공간 포함 깊이다.
- 모판 계열: `H1 작업공간 → H2 블록 → H3 경관 → H4 지역`으로 상향 조립하며 재고 상태는 별도 대장에서 관리한다.
- 현재 정의 수: `H1 8 / H2 0 / H3 5 / H4 1`

## 포함 계층

| 계층 | 의미 | 포함 | 현재 정의 | 현재 정책 |
| --- | --- | --- | ---: | --- |
| `H1` | 작업공간 모판 | - | 8 | `Defined` |
| `H2` | 블록 모판 | H1 | 0 | `DesignInventorySeparatedFromE5Instances` |
| `H3` | 경관 모판 | H2 | 5 | `DefinedPartialAssemblyAllowed` |
| `H4` | 지역 모판 | H3 | 1 | `DefinedPartialAssemblyAllowed` |

```text
H4 지역 모판 (AreaSet)
└─ H3 경관 모판 (LandscapeGraph)
   └─ H2 블록 모판 (LandscapeBlock)
      └─ H1 작업공간 모판 (WI 공간 모판) 인스턴스
```

H 코드는 리소스 종류를 분류할 뿐 E 완료 상태를 올리지 않는다. 현재 H4 AreaSet과 H3 Graph가 존재해도 WI의 실행 문맥, 권위 전이·Task/Effect·결과·후속 선택이 닫히지 않으면 E4·E5가 아니다.

## E 증거 단계와의 관계

| 증거 | H 계층 사용 | 완료 의미 |
| --- | --- | --- |
| `E3` | 없음 | WI 행위 계약·코드·자동 시험이 성립한다. |
| `E4` | 공간 적용이 Required인 WI만 H1~H5 참조 | 허용 발생원·주체·대상·자료·자원·시간과 선택적 공간 문맥을 결속한다. |
| `E5` | 공간 WI의 조건부 증거로 H1→H5 조립 사용 | 권위 전이·Task/Effect·결과·후속 선택이 결정적 세계에서 발현된다. 공간 조립만으로 완료되지 않는다. |
| `E6` | E5 WI 폐루프와 필요한 H 결과 사용 | WI·상태 변화와 인과 폐루프를 설명하고 필요한 현실 문맥의 출처·판본·hash·한계를 결속한다. |
| `E7` | E6 결과 사용 | 플레이어가 실제 서버와 저장 Scene에서 폐루프를 수행한다. |
| `E8` | 한 E7 PlayableUnit의 H 경로와 상태 사본 사용 | 같은 폐루프의 반복 결정성·Save 재진입·Local/Remote·실제 입력 안정성을 확인한다. |
| `E9` | 같은 영역의 E8 Core 둘 이상과 H 인계 사용 | 공간·시간·자원·회복·조건부 NPC 연속성의 조화와 사람 승인을 확인한다. |

## 계층에서 제외하는 축

- **Tile L0~L2**: 원자료 절단·캐시·부분 재생성 해상도이며 공간 포함 계층이 아니다.
- **Area**: 법정동·Farm·Hub·Town 의미 범위이며 LandscapeGraph와 N:N으로 참조된다.
- **경관 완결 영역**: 사람의 검토·완결 범위이며 구조적 부모 단위가 아니다.
- **ScenarioRoute**: Graph와 AreaSet이 참조하는 이동 의미이며 포함 계층이 아니다.
- **Synty 상향식 공간 설계 재고**: 팩에서 출발해 축적한 H1~H4 위치 독립 설계 재고다. 사람의 설계 검토로 승인할 수 있지만 실제 AreaSet 공간 조립과 공공데이터 계보, WI의 E5 세계 발현은 각각 별도로 검증한다.

기존 156개 기준 경관 문법 모판은 H 계층이 아니다. H1의 허용 후보와 H2·H3 조립에서 사용하는 공간 문법 어휘다.
