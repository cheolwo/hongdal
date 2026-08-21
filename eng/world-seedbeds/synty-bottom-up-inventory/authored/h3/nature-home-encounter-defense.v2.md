# Nature 생활핵·조우·방어 폐루프 경관

@spatial-knowledge h3-candidate:nature-home-encounter-defense
@hierarchy H3
@state ExploratoryInventory
@required-h2 h2-candidate:nature-home-core
@required-h2 h2-candidate:nature-encounter-route
@required-h2 h2-candidate:nature-defense-ring
@optional-h2 h2-candidate:nature-restoration-recovery
@connector SafeCoreGate
@connector ExplorationOutput
@connector ThreatInput
@connector RecoveryReturn

## 존재 이유

안전 생활핵에서 탐색을 시작해 몬스터 조우와 야간 방어를 거친 뒤 회복 공간으로 돌아오는 Nature 단독 경관이다.

## 공간 폐루프

```text
자연 안전 생활핵 블록
  → 자연 몬스터 조우·이탈 블록
  → 자연 야간 방어 블록
  → 자연 안전 생활핵 블록
```

- 첫 두 이동은 `PlayerTraversal`이다.
- 방어환에서 생활핵으로 돌아오는 이동은 `RecoveryHandoff`다.
- `SafeCoreGate`는 복귀 안전점, `ExplorationOutput`은 탐색 출발점, `ThreatInput`은 외부 위협 진입점, `RecoveryReturn`은 생활핵 복귀점이다.
- 세 H2의 상대 배치와 연결 의미만 정의하며 실제 지역 좌표나 Unity 자산 경로는 포함하지 않는다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H3`
- 실제 지역 권위: 없음

## 미해결

- 실제 AreaSet과 공공데이터 근거를 적용하기 전까지 조립 후보로 유지한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
