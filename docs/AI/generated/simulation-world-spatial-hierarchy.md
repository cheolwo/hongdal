# Simulation World 공간 포함 계층

> 이 문서는 `eng/world-seedbeds/spatial-hierarchy-levels.json`에서 자동 생성된다. 직접 수정하지 않는다.

- 계층 대장 개정: `simulation-world-spatial-hierarchy.r1`
- 증거 단계 개정: `simulation-evidence-stages.r4`
- 축 구분: `E`는 증거 깊이, `H`는 공간 포함 깊이다.
- 현재 정의 수: `H1 5 / H2 0 / H3 5 / H4 1`

## 포함 계층

| 계층 | 의미 | 포함 | 현재 정의 | 현재 정책 |
| --- | --- | --- | ---: | --- |
| `H1` | WI 공간 모판 | - | 5 | `Defined` |
| `H2` | LandscapeBlock | H1 | 0 | `ReservedForE5` |
| `H3` | LandscapeGraph | H2 | 5 | `DefinedPartialAssemblyAllowed` |
| `H4` | AreaSet | H3 | 1 | `DefinedPartialAssemblyAllowed` |

```text
H4 AreaSet
└─ H3 LandscapeGraph
   └─ H2 LandscapeBlock
      └─ H1 WI 공간 모판 인스턴스
```

H 코드는 리소스 종류를 분류할 뿐 완료 상태를 올리지 않는다. 현재 H4 AreaSet과 H3 Graph가 존재해도 실제 H2 Block과 연결 폐루프가 없으므로 E5가 아니다.

## E 증거 단계와의 관계

| 증거 | H 계층 사용 | 완료 의미 |
| --- | --- | --- |
| `E3` | 없음 | WI 행위 계약·코드·자동 시험이 성립한다. |
| `E4` | H1 | H1 모판에서 포함된 E3 WI를 다시 실행한다. |
| `E5` | H1→H2→H3→H4 | 실제 Block·Graph·AreaSet 이동 경로가 닫힌다. |
| `E6` | E5 결과 사용 | 공공데이터 원본·파생·출처·hash 계보를 검증한다. |
| `E7` | E6 결과 사용 | 플레이어가 실제 서버와 저장 Scene에서 폐루프를 수행한다. |

## 계층에서 제외하는 축

- **Tile L0~L2**: 원자료 절단·캐시·부분 재생성 해상도이며 공간 포함 계층이 아니다.
- **Area**: 법정동·Farm·Hub·Town 의미 범위이며 LandscapeGraph와 N:N으로 참조된다.
- **경관 완결 영역**: 사람의 검토·완결 범위이며 구조적 부모 단위가 아니다.
- **ScenarioRoute**: Graph와 AreaSet이 참조하는 이동 의미이며 포함 계층이 아니다.

기존 156개 기준 경관 문법 모판은 H 계층이 아니다. H1의 허용 후보와 H2·H3 조립에서 사용하는 공간 문법 어휘다.
