# 커뮤니티 게시글 Gmail 알림

## 목적

공개 게시글 저장이 완료되면 운영자에게 Gmail 알림을 보낸다. 수동 게시글, 예약 발행, 자동 발행, 원장 완료 게시글이 공통으로 발행하는 `커뮤니티게시글등록됨Event`를 진입점으로 사용한다.

## 처리 흐름

1. 게시글 트랜잭션이 완료된 뒤 MediatR 이벤트가 발행된다.
2. 이벤트 핸들러가 게시글 ID를 메모리 대기열에 등록한다.
3. 백그라운드 Worker가 공개·비삭제 게시글인지 다시 확인한다.
4. `smtp.gmail.com:587`과 TLS를 사용해 Gmail로 알림을 보낸다.
5. 일시적 실패는 설정된 횟수만큼 재시도한다.

메일에는 게시글 본문을 넣지 않는다. 신고 게시글은 제목과 작성자도 비식별 처리한다.

## 로컬 설정

Google 계정에서 2단계 인증을 켠 뒤 16자리 앱 비밀번호를 발급한다. 일반 Google 계정 비밀번호를 저장하면 안 된다.

```powershell
dotnet user-secrets --project Ssalddel set "CommunityPostEmailNotifications:Enabled" "true"
dotnet user-secrets --project Ssalddel set "CommunityPostEmailNotifications:Gmail:UserName" "sender@gmail.com"
dotnet user-secrets --project Ssalddel set "CommunityPostEmailNotifications:Gmail:AppPassword" "16자리앱비밀번호"
dotnet user-secrets --project Ssalddel set "CommunityPostEmailNotifications:RecipientEmail" "recipient@example.com"
dotnet user-secrets --project Ssalddel set "CommunityPostEmailNotifications:PublicBaseUrl" "https://example.com"
```

`RecipientEmail`이 비어 있으면 `IdentitySeed:BootstrapAdmin:Email`, 그 값도 비어 있으면 Gmail 사용자 주소를 수신자로 사용한다.

Docker 배포에서는 `.env`에 `SSALDDEL_COMMUNITY_POST_EMAIL_ENABLED`, `SSALDDEL_COMMUNITY_POST_EMAIL_RECIPIENT`, `SSALDDEL_GMAIL_USER_NAME`, `SSALDDEL_GMAIL_APP_PASSWORD`를 설정한다.

## 운영 주의사항

- 기본값은 `Enabled=false`다.
- 앱 비밀번호는 User Secrets, 환경 변수, 비밀 저장소 중 하나로만 주입한다.
- 현재 대기열은 단일 서버 프로세스 안에서 동작한다. 프로세스가 종료되기 직전의 미발송 항목까지 보장해야 하는 운영 단계에서는 DB outbox로 교체한다.
