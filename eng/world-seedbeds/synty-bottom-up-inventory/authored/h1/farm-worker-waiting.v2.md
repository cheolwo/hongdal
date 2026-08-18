# 농장 작업자 대기 공간

@spatial-knowledge h1-stock:farm-worker-waiting
@hierarchy H1
@state IdeaInventory
@wi WI-WORLD-01
@gameplay WorkerBriefing
@gameplay ShiftHandoff
@role FarmWorkerWaitingArea
@capability Spatial.WorkerAccessible
@capability Spatial.NpcWorkArea
@successor h1-stock:farm-production
@successor h1-stock:farm-work-yard
@connector WorkerAccess
@grammar farm:헛간 작업마당
@grammar town:버스 정류장·보행 쉼터

## 존재 이유

배정 전후의 작업자가 모이고 다음 작업 구역으로 이동하는 작은 대기 공간이다.

## 설계 상태

- 재고 상태: `IdeaInventory`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 실제 업무 용량과 연결구 방향은 공식 H1 승격 전에 검토한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
