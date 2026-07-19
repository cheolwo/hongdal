# Azure 저비용 미리보기 배포

이 구성은 커뮤니티 0.0을 가장 작은 실용 Azure 리소스로 확인하기 위한 미리보기 환경이다. 목표 사양은 `Standard_B1ms` Linux VM 한 대이며, Caddy, ASP.NET Core, MySQL, MongoDB를 Docker Compose로 실행하고 Redis 대신 메모리 상태 저장소를 사용한다. 2026-07-19 배포에서는 한국 중부와 인접 지역의 B1ms 용량을 확보하지 못해, 실제로 생성 가능한 최소 후보였던 `Standard_B2als_v2`를 사용했다.

## 경계

- `SsalddelExecution:Mode=Simulation`을 유지한다.
- MySQL과 MongoDB 포트는 VM 외부에 공개하지 않는다.
- Caddy만 80·443 포트를 공개하고 WebApp 정적 파일과 API를 같은 출처로 제공한다.
- 비밀값은 VM의 `/opt/ssalddel/.env`에만 저장하고 Git에 넣지 않는다.
- B1ms 2GB는 기능 확인용 목표 최소 사양이고 현재 미리보기 VM은 B2als_v2 4GB다. 이후 B1ms 용량이 생기면 메모리 사용량을 확인한 뒤 축소할 수 있다.
- 단일 VM이므로 VM 장애 시 웹·API·DB가 함께 중단된다. 관리형 DB, 백업, 모니터링을 갖춘 운영 구성으로 보지 않는다.

## 배포 순서

1. `Ssalddel` 이미지를 `ssalddel-server:azure-preview`로 빌드한다.
2. `Ssalddel.WebApp`을 Release/Production으로 게시한다.
3. VM에 `deploy/azure-vm`, WebApp 게시 파일과 Docker 이미지를 전송한다.
4. MySQL과 MongoDB를 먼저 시작한다.
5. 같은 서버 이미지로 `--initialize-database`를 한 번 실행한다.
6. 앱과 Caddy를 시작하고 공개 HTTPS URL에서 `/health/live`, `/health/ready`, 게시판과 글쓰기를 확인한다.

배포 후에는 Azure Portal의 비용 분석과 무료 크레딧 잔액을 확인한다. 미리보기가 필요 없으면 VM을 중지하는 것만으로는 OS 디스크와 공인 IP 비용이 남을 수 있으므로, 보존할 데이터가 없다면 리소스 그룹 전체 삭제를 검토한다.
