# Azure 저비용 미리보기 배포

이 구성은 커뮤니티 0.0을 가장 작은 실용 Azure 리소스로 확인하기 위한 미리보기 환경이다. 목표 사양은 `Standard_B1ms` Linux VM 한 대이며, Caddy, ASP.NET Core, MySQL, MongoDB를 Docker Compose로 실행하고 Redis 대신 메모리 상태 저장소를 사용한다. 2026-07-19 배포에서는 한국 중부와 인접 지역의 B1ms 용량을 확보하지 못해, 실제로 생성 가능한 최소 후보였던 `Standard_B2als_v2`를 사용했다.

## 경계

- `SsalddelExecution:Mode=Operational`을 사용한다. Toss 비밀키와 각 외부 공급자 자격 증명이 없으면 실패하도록 두며 sample fallback을 사용하지 않는다.
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
2. 정적 역할 선택 포털과 `Ssalddel.Web.CommunityApp`부터
   `Ssalddel.Web.WarehouseApp`까지 01~05 WebApp을 각각 Release/Production으로 게시한다.
3. VM에 `deploy/azure-vm`, WebApp 게시 파일과 Docker 이미지를 전송한다.
4. Storage Account와 공개·비공개 컨테이너를 만들고 VM Managed Identity에 각 컨테이너 범위의 `Storage Blob Data Contributor`를 부여한다.
5. `.env`의 `SSALDDEL_STORAGE_ACCOUNT_NAME`을 설정한다. 키나 연결 문자열은 넣지 않는다.
6. MySQL과 MongoDB를 먼저 시작한다.
7. 같은 서버 이미지로 `--initialize-database`를 한 번 실행한다. 최초 관리자 계정이 없다면 이 실행에만 `SSALDDEL_BOOTSTRAP_ADMIN_ENABLED=true`와 아이디·이메일·임시 강력 비밀번호를 주입하고, 성공 직후 다시 비활성화하고 비밀번호를 제거한다.
8. 앱과 Caddy를 시작하고 공개 HTTPS URL에서 `/health/live`, `/health/ready`, 게시판과 실제 이미지 첨부를 확인한다.

### WebApp만 갱신

서버 API와 DB를 바꾸지 않고 기존 Azure 미리보기의 브라우저 화면만 갱신할 때는
다음 스크립트를 사용한다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File deploy/azure-vm/package-web-preview.ps1
```

이 명령은 `artifacts/local/azure-web-preview/` 아래에 JavaScript 없는 정적 역할 선택
포털과 Release/Production 01~05 역할 WebApp, `preview-build.json`,
`web-preview.tar.gz`와 SHA-256 파일을 만든다. 역할 앱은 `/roles/01/`부터
`/roles/05/`까지 서로 다른 base path를 사용한다. 작업 트리에 WebApp 또는 공유
UI·contract 변경이 있으면 공개 빌드 표시에 `working-tree`를 남긴다. scoped stylesheet
주소에는 release ID를 붙여 기존 방문 세션도 새 화면 CSS를 다시 받게 한다.

묶음과 `deploy/azure-vm/Caddyfile`을 VM으로 전송한 뒤 `deploy-web-preview.sh`에
archive, SHA-256, 배포 루트와 Caddyfile 경로를 전달한다.

```bash
sudo bash deploy-web-preview.sh \
  web-preview.tar.gz \
  <sha256> \
  /opt/ssalddel \
  Caddyfile
```

스크립트는 `/opt/ssalddel/web`을 타임스탬프 백업으로 이동하고 새 WebApp을 원자적으로
교체하며, Caddyfile도 별도 타임스탬프 백업 후 Caddy만 다시 만든다. 공개
`preview-build.json` 확인이 실패하면 새 web을 `web-failed-<timestamp>`로 보존하고
이전 web과 Caddyfile을 복구한다. API, MySQL, MongoDB, 볼륨과 `.env`는 변경하지 않는다.
역할 분리 구조와 route 경계는
[01~05 역할 분리 WebApp](../Architecture/RoleSeparatedWebApps.md)을 따른다.

### Unity 산출물 검토 WebApp 별도 갱신

Unity 촬영 산출물의 H1·H2·H3 후보 검토 화면은 일반 01~05 WebApp 묶음에
포함하지 않는다. `Ssalddel.Web.UnityReviewApp`만 게시하는 전용 명령을 사용한다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File deploy/azure-vm/package-unity-review-preview.ps1
```

산출물은 `artifacts/local/azure-unity-review-preview/` 아래의
`unity-review.tar.gz`, SHA-256 파일과 `unity-review/preview-build.json`이다.
manifest는 `/unity-review/` base path, H1·H2·H3 검토 범위와
`ServerAdministratorCandidateReview` 권한 경계를 기록한다. 운영 설정의 API 주소는
`same-origin`이며 브라우저의 현재 host 루트에 있는 기존 관리자 API를 사용한다.

기존 VM을 재사용해 비용을 억제하되 원격 교체 단위는 분리한다. 다음 스크립트는
`/opt/ssalddel/web/unity-review`만 타임스탬프 백업 후 교체하고 Caddy 설정을 검증한다.
일반 `/roles/01/`~`/roles/05/` 파일과 API·MySQL·MongoDB 볼륨은 바꾸지 않는다.

```bash
sudo bash deploy-unity-review-preview.sh \
  unity-review.tar.gz \
  <sha256> \
  /opt/ssalddel \
  Caddyfile
```

검토 API나 Mongo·Blob 저장 코드가 함께 바뀐 배포는 정적 WebApp 교체 전에 새
`ssalddel-server:azure-preview` 이미지를 적재하고 app health를 확인해야 한다.
정적 배포 스크립트가 서버 이미지를 암묵적으로 바꾸지는 않는다. 공개 주소는
`https://<SSALDDEL_SITE_HOST>/unity-review/`이며 화면은 서버관리자 로그인을 요구한다.
Blob 이미지 object는 현재 공개 읽기이므로 URL 보유자는 로그인 없이 이미지를 열 수
있고, 촬영 PNG에는 개인정보·주문·인증·Console 정보를 넣지 않는다.

현재 비용 통제 운영창은 매일 한국 시간 19:00~23:00이다. 이 시간 밖에서 VM을
수동 기동하거나 운영창을 바꾸는 것은 별도 명시 승인을 받은 뒤 수행한다.

배포 전에는 CLI의 로컬 account 표시만 보지 말고 ARM subscription 상태와 실제 VM
쓰기 가능 여부를 확인한다. Free Trial 크레딧 만료나 spending limit 도달 뒤
subscription이 `Warned` 또는 `ReadOnlyDisabledSubscription`이면 VM 시작·Blob 쓰기와
배포가 차단된다. 이 경우 종량제 전환이나 spending limit 제거를 자동 수행하지 않는다.
유효한 결제 수단과 향후 사용량 청구에 대한 별도 명시 승인을 받은 뒤 Azure Portal에서
구독을 다시 활성화해야 한다. 기준은 Microsoft의
[Azure spending limit](https://learn.microsoft.com/azure/cost-management-billing/manage/spending-limit)과
[무료 계정 비용 방지](https://learn.microsoft.com/azure/cost-management-billing/manage/avoid-charges-free-account)를 따른다.

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
`SsalddelExecution:Mode=Operational`을 사용한다. 계약·결제·신고 제출·포워더 자동
선정·외부 전송은 각 기능 플래그와 공급자 자격 증명을 추가로 통과해야 한다. 현재 미리보기 VM이 인증서 기반 Data
Protection 전환 전이라면 override의 `PersonalDataProtection:RequireCertificate=false`는
기존 key ring 호환을 위한 임시 설정이다. 인증서 배치와 기존 key ring 복호화 검증을
마친 뒤에는 기본 구성의 `true`로 복귀해야 한다.

### 국내 운송 2.0 미리보기

화주·기사 운송 페이지를 배포 상태에서 점검할 때는 기존 Compose와
`compose.orderer-v15.override.yaml` 뒤에 `compose.transport-v20.override.yaml`을 추가한다.
2.0 override는 선행 커뮤니티 기반과 `DomesticTransportWorkflow`를 활성화하고
`SsalddelExecution:Mode=Operational`을 사용한다.

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
