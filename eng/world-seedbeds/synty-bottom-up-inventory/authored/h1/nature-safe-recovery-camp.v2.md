# 자연권 안전 회복 야영지

@spatial-knowledge h1-stock:nature-safe-recovery-camp
@hierarchy H1
@state ExploratoryInventory
@gameplay PartyRecovery
@gameplay ThreatDebrief
@gameplay NextActionPreparation
@role NatureSafeRecoveryCampArea
@capability Spatial.Traversable
@capability Spatial.RestArea
@capability Spatial.SafeCore
@capability Spatial.NpcWorkArea
@predecessor h1-stock:nature-emergency-retreat
@predecessor h1-stock:nature-restoration-site
@successor h1-stock:nature-trailhead
@successor h1-stock:nature-threat-watch
@connector RetreatInput
@connector RecoveryInput
@connector SafeCoreOutput
@grammar nature:숲 빈터·고사목
@grammar nature:초지·야생화

## 존재 이유

위협 관찰·후퇴·복구 뒤 플레이어와 동료가 다음 행동을 준비하는 안전 생활핵의 회복 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
