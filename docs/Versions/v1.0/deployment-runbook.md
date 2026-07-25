# 문화교통 1.0 배포 준비 절차

## 배포 의미

이 절차는 `0.0` 커뮤니티 기반에 `1.0` 비구속 공동구매 수요·주문자 집단화를 추가하는
통제된 `Operational` 검증용이다. 1.0 기능 플래그 범위 밖의 결제, 계약, 수입 신고,
운송 의뢰, 자동 배차, 창고 상태 변경과 판매채널 동기화는 열지 않는다.

배포 프로필은 `deploy/azure-vm/compose.orderer-v10.override.yaml`이다. 이 프로필은
`CommunityTrustWorkflow`, `GroupPurchasePracticeWorkflow`,
`GroupPurchaseDemandWorkflow`만 열고 `1.5` 이후 업무 플래그를 명시적으로 닫는다.

## 배포 전 게이트

1. `Ssalddel.v1.0.slnx`의 Release build와 관련 test가 통과해야 한다.
2. GitHub `Release readiness` workflow가 만든
   `orderer-v10-deployment-<run-number>` 산출물을 사용한다.
3. 산출물에 서버 image, Web 정적 파일, 기본 Compose, 1.0 override와 배포 script가
   모두 있는지 확인한다.
4. VM의 `/opt/ssalddel/.env`와 Data Protection 인증서가 준비되어야 한다.
5. MySQL·MongoDB backup과 복구 위치를 확인하고 현재 Web 폴더와 서버 image를
   되돌릴 수 있어야 한다.
6. `SsalddelExecution:Mode=Operational`과 필수 비밀값을 확인하고, 1.0 범위 밖 기능 플래그가 닫혀 있는지 확인한다.

## 산출물 구성

```text
ssalddel-server.tar
web.tar.gz
compose.yaml
compose.orderer-v10.override.yaml
Caddyfile
deploy-preview-profile.sh
deploy-orderer-v10.sh
```

`web.tar.gz`의 최상위에는 `index.html`이 있어야 한다. 비밀값이나 `.env`는 산출물에
포함하지 않는다.

## 최초 설치

1. `compose.yaml`, `Caddyfile`, 배포 script를 `/opt/ssalddel`에 배치한다.
2. `/opt/ssalddel/.env`를 VM에서만 만들고 권한을 `600`으로 제한한다.
3. MySQL과 MongoDB를 먼저 시작한다.
4. 같은 서버 image로 `--initialize-database`를 한 번 실행한다.
5. 최초 관리자 bootstrap이 필요하면 초기화 실행에만 활성화하고 성공 직후 비밀번호와
   활성 설정을 제거한다.

## 배포

release 산출물을 예를 들어 `/opt/ssalddel/releases/orderer-v10-<run-number>`에 둔 뒤
다음을 실행한다.

```bash
bash /opt/ssalddel/deploy-orderer-v10.sh \
  /opt/ssalddel/releases/orderer-v10-<run-number>
```

공통 배포 script는 다음을 순서대로 수행한다.

1. 현재 서버 image를 rollback tag로 보존한다.
2. 1.0 Compose 구성을 검사한다.
3. 새 Web 파일을 별도 폴더에서 푼다.
4. 서버를 교체하고 `/health/ready`가 정상인지 확인한다.
5. 실패하면 이전 Web과 서버 image를 같은 1.0 프로필로 복구한다.

## 배포 후 확인

1. `/health/live`, `/health/ready`가 정상이어야 한다.
2. 익명 사용자가 공개 공동구매 모집 목록을 조회할 수 있어야 한다.
3. 로그인 사용자가 `Idempotency-Key`로 비구속 수요를 등록하고 같은 키 재시도 시
   같은 stable ID를 받아야 한다.
4. 같은 사용자가 수요를 변경·철회한 뒤 같은 원장을 다시 조회할 수 있어야 한다.
5. 공개 응답에 개인 주소, 연락처, 결제 정보와 내부 사용자 식별자가 없어야 한다.
6. `CustomsAndTradeDataWorkflow`와 운송·창고·판매·배달 API는 기능 비활성 응답을
   반환해야 한다.
7. 서버 log와 배포 증적에 비밀값, 개인정보 또는 원문 인증자료가 없어야 한다.

확인 결과에는 Git commit, workflow run 번호, image ID, 배포 시각, health 결과,
멱등 재시도 결과와 rollback 가능 여부만 기록한다.

## 승격 보류

다음은 별도 승인 전까지 배포 준비 완료로 간주하지 않는다.

- `Operational` 실행 모드
- 실제 결제·계약·신고·운송·창고·판매 동기화
- 자격 사업자 또는 전문 검토자를 자동 선정하는 기능
- backup 복구 리허설과 원격 workflow 녹색 증적이 없는 운영 공개
