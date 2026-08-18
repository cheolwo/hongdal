# Hub 검역·격리 블록

@spatial-knowledge h2-candidate:hub-quarantine-staging
@hierarchy H2
@state IdeaInventory
@required-h1 h1-stock:hub-receiving-storage
@required-h1 h1-stock:hub-quarantine
@required-h1 h1-stock:hub-temporary-staging
@connector HubInboundGate
@connector StorageOutput
@connector RejectOutput

## 존재 이유

입고 검수와 격리, 임시 적치를 일반 보관 흐름과 분리하는 품질 관리 레시피다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 실제 Block 경계와 배치 방향은 현실 근거 적용 단계에서 결정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
