# Azure Blob 기반 객체 저장소 전환

## 결과

게시글 첨부가 `IGoogleCloudStorageService`에 직접 의존하던 구조를 공급자 중립적인 `IObjectStorageService`로 변경했다. 운영은 Azure Blob Storage, 로컬 개발은 파일 시스템, 필요 시 Google Cloud Storage를 선택할 수 있다.

공개 게시글 이미지는 `community-public`, POD·운송 증빙·커뮤니티 음성 같은 민감 파일은 `platform-private` 경계를 사용한다. Azure 어댑터는 `DefaultAzureCredential`을 사용하므로 운영 VM에서는 Managed Identity와 컨테이너 범위 RBAC로 접근하며 연결 문자열이나 Storage Account Key를 요구하지 않는다.

## 화면 확인

화면 없음 · 간접 확인. 기존 게시글 첨부 API와 화면 계약은 유지하고 객체 주소와 저장소 구현만 교체했다. 운영 게시글의 기존 이미지 두 장을 Blob으로 이전한 뒤 브라우저에서 상세 화면 렌더링을 확인했다.

## 검증

- 서버 Release build 경고 0개·오류 0개
- 객체 저장소 및 게시글 첨부 관련 테스트 6개 통과
- 공개 이미지 업로드·익명 조회 성공
- 비공개 객체 익명 조회 차단 및 Managed Identity 조회 성공
- 운영 `/health/live`, `/health/ready` 모두 Healthy
- 변경 파일 `git diff --check` 통과

## 운영 경계

Blob Storage는 저장 용량·요청·외부 전송량에 따라 비용이 발생할 수 있다. 기존 VM 로컬 미디어 볼륨은 앱에서 분리했지만 이전 검증과 롤백을 위해 즉시 삭제하지 않는다.
