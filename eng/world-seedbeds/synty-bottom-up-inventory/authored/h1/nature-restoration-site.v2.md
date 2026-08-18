# 자연권 정화·복구 작업 공간

@spatial-knowledge h1-stock:nature-restoration-site
@wi WI-NATURE-03
@hierarchy H1
@state CandidateForReview
@gameplay NatureRestoration
@gameplay ContaminationCleanup
@gameplay RouteRecovery
@role NatureRestorationWorkArea
@capability Spatial.WorkerAccessible
@capability Spatial.RestorationWorkArea
@capability Spatial.CargoAccessible
@capacity RestorationWorkArea
@capacity RestorationMaterialStaging
@predecessor h1-stock:nature-incident-trace
@successor h1-stock:nature-safe-recovery-camp
@successor h1-stock:nature-exploration-buffer
@connector CauseRouteInput
@connector MaterialInput
@connector RecoveredRouteOutput
@grammar nature:수변 완충지
@grammar nature:숲 가장자리

## 존재 이유

지역 사건의 원인을 해결한 뒤 남은 자연권 압력을 낮추기 위한 정화·복구 작업 공간이다.

## 설계 상태

- 재고 상태: `CandidateForReview`
- 공간 계층: `H1`
- 실제 지역 권위: 없음

## 미해결

- E2에서 해결된 원인 계보와 복원 자재 예약 단위를 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
