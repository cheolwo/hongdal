# Docker Compose 로컬 실행

## 1. 비밀 설정 준비

`docker-compose.env.example`을 `.env`로 복사한 뒤 비밀번호, JWT, ISMS-P 키, KAMIS와 USDA 키를 실제 값으로 바꾼다. `.env`와 `appsettings.Local.json`은 Git 및 Docker 빌드 컨텍스트에서 제외된다.

기존 MySQL·MongoDB 볼륨을 이미 만든 경우에는 최초 생성 때 사용한 비밀번호를 계속 사용해야 한다. `.env`의 비밀번호와 연결 문자열의 비밀번호가 기존 볼륨과 다르면 컨테이너 환경변수만 바꿔도 DB 계정 비밀번호는 바뀌지 않는다.

## 2. 이미지 빌드

```powershell
docker compose --progress plain build app
```

Dockerfile은 복원 레이어를 캐시한 뒤 `dotnet publish` 한 번으로 컴파일한다. 완료 후 이미지 ID와 생성 시각을 확인한다.

```powershell
docker image inspect hongdal-app --format 'Id={{.Id}} Created={{.Created}}'
```

## 3. 데이터베이스 초기화

웹 서버가 시작할 때마다 여러 인스턴스에서 마이그레이션하지 않도록 기본값은 `DatabaseInitialization__RunAtStartup=false`다. 이미지 배포 전에 같은 이미지로 초기화 명령을 한 번 실행한다.

```powershell
docker compose up -d mysql redis mongo
docker compose run --rm --no-deps app --initialize-database
```

초기화 명령이 실패하면 웹 서버를 시작하지 말고 누락된 EF Core 마이그레이션이나 연결 정보를 먼저 수정한다.

## 4. 서버 실행과 확인

```powershell
docker compose up -d app
docker compose ps
curl.exe --fail http://localhost:8080/health/live
curl.exe --fail http://localhost:8080/health/ready
```

- `/health/live`: ASP.NET Core 프로세스가 요청을 처리할 수 있는지 확인한다.
- `/health/ready`: 기본·전통시장·농수산 MySQL DbContext의 연결과 미적용 마이그레이션, Redis, MongoDB를 확인한다.
- 농수산물 배치는 `HONGDAL_AGRICULTURAL_FISHERIES_BATCH_ENABLED=true`일 때만 등록된다.

`docker compose ps`의 app 상태가 `healthy`가 아니면 다음 순서로 확인한다.

```powershell
docker compose logs --tail 200 app
docker compose config
```

`docker compose config` 출력에는 비밀값이 포함될 수 있으므로 외부에 그대로 공유하지 않는다.
