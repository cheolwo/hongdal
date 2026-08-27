# 농가 귀환·작업자 대기 공간

@spatial-knowledge h1-stock:farm-worker-waiting
@hierarchy H1
@state CandidateForReview
@wi WI-WORLD-01
@gameplay WorkerBriefing
@gameplay ShiftHandoff
@gameplay FarmReturn
@gameplay FarmRest
@gameplay NextTaskSelection
@role FarmWorkerWaitingArea
@role FarmReturnPoint
@role FarmRestArea
@capability Spatial.WorkerAccessible
@capability Spatial.NpcWorkArea
@capability Spatial.RestArea
@successor h1-stock:farm-production
@successor h1-stock:farm-work-yard
@connector WorkerAccess
@grammar farm:헛간 작업마당
@grammar town:버스 정류장·보행 쉼터

## 존재 이유

플레이어와 작업자가 농장에 돌아와 쉬고, 작업을 확인하고, 다음 작업 구역으로 출발하는 생활 거점 공간이다.

## 설계 상태

- 재고 상태: `CandidateForReview`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- 휴식은 표현·다음 작업 선택 기능이며 소속 PlayableUnit E7, E8 반복 안정성이나 E9 NPC 생활 조화를 자동으로 만들지 않는다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
