# Hub 임시 적치 공간

@spatial-knowledge h1-stock:hub-temporary-staging
@hierarchy H1
@state ExploratoryInventory
@wi WI-001
@wi WI-HUB-04
@wi WI-HUB-05
@gameplay InboundStaging
@gameplay OutboundStaging
@role HubTemporaryStagingArea
@capability Spatial.CargoAccessible
@capability Spatial.WorkerAccessible
@capability Spatial.TemporaryStorage
@predecessor h1-stock:hub-receiving-storage
@successor h1-stock:hub-outbound-staging
@successor h1-stock:hub-long-term-storage
@connector InboundHandoff
@connector StorageHandoff
@connector OutboundHandoff
@grammar city:화물 대기 야드
@grammar city:상하차 Dock

## 존재 이유

입고·피킹·출고 사이에서 화물을 짧게 대기시키는 공유 적치 공간이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
