# Unity 산출물 검토 전용 무료 VM 배포

이 배포 단위는 기존 역할별 WebApp VM과 분리된 Unity 조합물 검토 전용 스택이다.

```text
Caddy 정적 Web·TLS
  -> UnityReview 전용 ASP.NET API
  -> MySQL 원장
  -> 불변 이미지 Docker volume
```

무료 대상 VM의 1GB 메모리 한계 때문에 MySQL과 MongoDB를 함께 실행하지 않는다. 역할별 업무 API, 운영 MySQL, 운영 MongoDB, 결제·주문·배차 기능도 포함하지 않는다. Compose 상한은 MySQL 384MB, API 320MB, Caddy 64MB이며 host에는 2GB swap을 준비한다.

로컬 게시 묶음과 서버 비밀값 파일은 `artifacts/local/azure-unity-review-vm/`에 생성하며 Git에 포함하지 않는다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File deploy/azure-unity-review-vm/package.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File deploy/azure-unity-review-vm/prepare-secrets.ps1 `
  -SiteHost ssalddel-unity-review.koreacentral.cloudapp.azure.com
```

관리자 비밀번호 원문은 `Ssalddel.UnityReview.Api`의 .NET User Secrets에만 보관한다. 배포 파일에는 PBKDF2 결과만 기록한다.

VM 생성은 정확히 무료 대상 SKU만 허용한다. 무료 혜택 기간·월 750시간·디스크·공인 IP·전송 비용은 Azure 구독에서 별도로 확인한다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File deploy/azure-unity-review-vm/provision.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File deploy/azure-unity-review-vm/deploy-from-windows.ps1
```

`Good`은 화면상 후보 판단일 뿐 H 승인, Scene 적용, E5 또는 Simulation 성공이 아니다. 운전 중이나 신호 대기 중에는 사용하지 않고 완전히 주차한 뒤 검토한다.
