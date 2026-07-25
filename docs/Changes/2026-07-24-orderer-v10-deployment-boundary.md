# 주문자 1.0 배포 경계 정리

| 항목 | 내용 |
| --- | --- |
| 날짜 | 2026-07-24 |
| 커밋 | 미커밋 |
| 변경 축 | 1.0 Simulation 배포 프로필, 공통 health rollback, CI 배포 산출물 |
| 화면 변경 | 화면 없음 |
| 시각 증거 | 해당 없음 |

## 변경

- Azure 기본 Compose에서 1.0 수요와 1.5 이후 이행 기능을 기본 비활성화했다.
- `compose.orderer-v10.override.yaml`에서 커뮤니티·연습·비구속 수요만 활성화했다.
- 1.0과 1.5 배포가 같은 `deploy-preview-profile.sh`의 health 확인과 rollback을
  사용하도록 중복을 제거했다.
- `Release readiness` workflow가 바로 전달 가능한 1.0 서버 image·Web·Compose·script
  묶음을 생성하도록 했다.
- 배포 전 확인, 배포 후 smoke test, rollback과 남은 staging gate를
  [1.0 배포 준비 절차](../Versions/v1.0/deployment-runbook.md)에 기록했다.

## Controller 경계

1.0 핵심 `공동구매자동집단화Controller`는 주문자 공통 Controller 경계를 사용하며
공개 모집 조회, 배치 미리보기, 본인 비구속 수요 저장·철회를 한 UseCase 경계로
유지한다. 1.5 이후 Controller 코드는 삭제하지 않고 기능 플래그로 차단한다.

## 검증

- `OrdererV10DeploymentBoundaryTests`
- `공동구매자동집단화ControllerTests`
- `eng/validate-changes.ps1 -Level Fast`
- `eng/validate-changes.ps1 -Level Task`

실제 원격 CI와 staging 복구 rehearsal은 별도 실행 증적이 필요하다.
