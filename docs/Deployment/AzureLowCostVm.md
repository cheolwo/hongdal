# Azure 저비용 미리보기 배포

이 구성은 커뮤니티 0.0을 가장 작은 실용 Azure 리소스로 확인하기 위한 미리보기 환경이다. 목표 사양은 `Standard_B1ms` Linux VM 한 대이며, Caddy, ASP.NET Core, MySQL, MongoDB를 Docker Compose로 실행하고 Redis 대신 메모리 상태 저장소를 사용한다. 2026-07-19 배포에서는 한국 중부와 인접 지역의 B1ms 용량을 확보하지 못해, 실제로 생성 가능한 최소 후보였던 `Standard_B2als_v2`를 사용했다.

## 경계

- `SsalddelExecution:Mode=Simulation`을 유지한다.
- MySQL과 MongoDB 포트는 VM 외부에 공개하지 않는다.
- Caddy만 80·443 포트를 공개하고 WebApp 정적 파일과 API를 같은 출처로 제공한다.
- 게시글 공개 이미지는 같은 `Korea Central`의 Blob `community-public`, POD·운송 증빙·음성은 비공개 `platform-private`에 저장한다.
- 앱은 VM Managed Identity와 컨테이너 범위 RBAC로 Blob에 접근하며 연결 문자열과 Storage Account Key를 사용하지 않는다.
- 비밀값은 VM의 `/opt/ssalddel/.env`에만 저장하고 Git에 넣지 않는다.
- Data Protection key ring은 `app_data` 볼륨에 유지하고 `/opt/ssalddel/deploy/azure-vm/secrets/ssalddel-data-protection.pfx` 인증서로 암호화한다. 인증서와 비밀번호도 Git에 넣지 않는다.
- B1ms 2GB는 기능 확인용 목표 최소 사양이고 현재 미리보기 VM은 B2als_v2 4GB다. 이후 B1ms 용량이 생기면 메모리 사용량을 확인한 뒤 축소할 수 있다.
- 단일 VM이므로 VM 장애 시 웹·API·DB가 함께 중단된다. 관리형 DB, 백업, 모니터링을 갖춘 운영 구성으로 보지 않는다.

## 배포 순서

1. `Ssalddel` 이미지를 `ssalddel-server:azure-preview`로 빌드한다.
2. `Ssalddel.WebApp`을 Release/Production으로 게시한다.
3. VM에 `deploy/azure-vm`, WebApp 게시 파일과 Docker 이미지를 전송한다.
4. Storage Account와 공개·비공개 컨테이너를 만들고 VM Managed Identity에 각 컨테이너 범위의 `Storage Blob Data Contributor`를 부여한다.
5. `.env`의 `SSALDDEL_STORAGE_ACCOUNT_NAME`을 설정한다. 키나 연결 문자열은 넣지 않는다.
6. MySQL과 MongoDB를 먼저 시작한다.
7. 같은 서버 이미지로 `--initialize-database`를 한 번 실행한다. 최초 관리자 계정이 없다면 이 실행에만 `SSALDDEL_BOOTSTRAP_ADMIN_ENABLED=true`와 아이디·이메일·임시 강력 비밀번호를 주입하고, 성공 직후 다시 비활성화하고 비밀번호를 제거한다.
8. 앱과 Caddy를 시작하고 공개 HTTPS URL에서 `/health/live`, `/health/ready`, 게시판과 실제 이미지 첨부를 확인한다.

### 주문자 1.0 미리보기

비구속 공동구매 수요와 주문자 집단화까지만 확인할 때는
`compose.orderer-v10.override.yaml`을 두 번째 Compose 파일로 사용한다. 기본
`compose.yaml`은 후속 업무 플래그를 모두 닫으며, 1.0 override만
`GroupPurchaseDemandWorkflow`를 추가로 활성화한다.

GitHub `Release readiness` workflow가 생성한
`orderer-v10-deployment-<run-number>` 산출물에는 서버 image, Web 정적 파일,
Compose와 공통 rollback script가 함께 들어간다. 배포와 확인 절차는
[문화교통 1.0 배포 준비 절차](../Versions/v1.0/deployment-runbook.md)를 따른다.

### 주문자 1.5 미리보기

기존 VM의 비밀값·볼륨·보안 설정을 그대로 유지하면서 주문자 1.5 기능을 점검할 때는
`compose.orderer-v15.override.yaml`을 두 번째 Compose 파일로 함께 지정한다. 이 override는
`GroupPurchaseDemandWorkflow`와 `CustomsAndTradeDataWorkflow`만 활성화하며
`SsalddelExecution:Mode=Simulation`을 다시 고정한다. 계약·결제·신고 제출·포워더 자동
선정·외부 전송을 운영 모드로 바꾸지 않는다. 현재 미리보기 VM이 인증서 기반 Data
Protection 전환 전이라면 override의 `PersonalDataProtection:RequireCertificate=false`는
기존 key ring 호환을 위한 임시 설정이다. 인증서 배치와 기존 key ring 복호화 검증을
마친 뒤에는 기본 구성의 `true`로 복귀해야 한다.

### 국내 운송 2.0 미리보기

화주·기사 운송 페이지를 배포 상태에서 점검할 때는 기존 Compose와
`compose.orderer-v15.override.yaml` 뒤에 `compose.transport-v20.override.yaml`을 추가한다.
2.0 override는 선행 커뮤니티 기반과 `DomesticTransportWorkflow`를 활성화하지만
`SsalddelExecution:Mode=Simulation`을 유지한다.

페이지 책임은 `화주 작성 → 의뢰별 요약·이력·결제 상태·증빙`,
`기사 추천 목록 → 추천 상세 → 수락·거절 판단 → 현재 운송 → 상차 → 하차`로 분리한다.
추천 목록과 상세 화면은 상태를 바꾸지 않으며, 수락·거절과 상·하차 Command는 전용
화면에서만 서버 권한·선행 상태 검증을 거쳐 요청한다. 현재 운송은 조회와 다음 단계
안내만 맡고, 이전 `/driver/transport/proof` 링크도 읽기 전용 단계 선택 화면으로
동작한다. 브라우저 타이머는 만료를 표시할 뿐 자동 거절 Command를 전송하지 않는다.

운영 승인 전에는 자동 배차, 운송 계약 중개, 실결제, 운임 수취, 기사 지급과 외부
운송사 전달을 활성화하지 않는다. 2.0 화면이 공개되더라도 이 경계가 바뀌는 것은 아니다.

Blob 이전이 완료되면 Caddy와 앱에서 `app_community` 볼륨 연결을 제거한다. 롤백 확인 전에는 기존 명명된 볼륨 자체를 즉시 삭제하지 않는다.

배포 후에는 Azure Portal의 비용 분석과 무료 크레딧 잔액을 확인한다. Blob도 저장 용량·요청·외부 전송량에 따라 소액이 발생할 수 있어 완전 무료로 표현하지 않는다. 미리보기가 필요 없으면 VM을 중지하는 것만으로는 OS 디스크와 공인 IP 비용이 남을 수 있으므로, 보존할 데이터가 없다면 리소스 그룹 전체 삭제를 검토한다.
