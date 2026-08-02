# 개발 검증 안정성 가이드

실행 중인 Hongdal 프로세스가 기본 `bin/obj`의 DLL을 잡고 있거나 새 worktree에 `project.assets.json`이 없을 때, 같은 실패를 반복하지 않도록 `eng/validate-changes.ps1`을 표준 검증 진입점으로 사용합니다.

## 기본 사용법

테스트는 별도 artifacts 경로에 패키지를 복원한 뒤, 같은 경로를 사용해 `--no-restore`로 실행됩니다.

```powershell
.\eng\validate-changes.ps1
```

특정 프로젝트를 빌드하거나 테스트 필터를 적용할 수 있습니다.

```powershell
.\eng\validate-changes.ps1 -Action Build -Target .\Hongdal.WebApp\Hongdal.WebApp.csproj
.\eng\validate-changes.ps1 -Target .\Hongdal.Tests\Hongdal.Tests.csproj -Filter 'FullyQualifiedName~HongdalUiCommonServiceCollectionExtensionsTests'
```

각 실행은 기본적으로 `artifacts/validation/<project>/<timestamp>-<process-id>/`를 사용합니다. 따라서 개발 서버가 기본 출력 DLL을 잠가도 검증 출력과 충돌하지 않습니다. 병렬 검증에서는 서로 다른 `-RunId`를 지정합니다.

## restore와 검증 실패의 구분

- 기본 동작은 `dotnet restore`를 먼저 실행합니다. 실패하면 build/test는 시작하지 않으며 패키지 소스·네트워크 문제를 restore 단계로 보고합니다.
- restore 성공 뒤에는 같은 artifacts 경로에서 `dotnet build/test --no-restore`를 실행합니다. 이 단계의 실패는 컴파일·테스트·출력 잠금 문제입니다.
- 테스트는 TRX 결과의 실행 건수를 확인합니다. 필터 오타 등으로 `dotnet test`가 0건을 실행하고 종료 코드 0을 반환해도 성공으로 처리하지 않습니다.
- `-NoRestore`는 이미 같은 `-ArtifactsPath`에 대상 프로젝트와 모든 `ProjectReference`의 assets가 있을 때만 사용합니다. 없으면 누락 프로젝트 이름을 나열하고 즉시 중단합니다.
- `MSB3021` 또는 `MSB3027`이 격리 경로에서도 발생하면 해당 run artifacts를 다른 프로세스가 사용 중인 것입니다. 실행 중인 검증을 확인하거나 새 `-RunId`로 재시도합니다. 기본 `bin/obj` 삭제로 원인을 숨기지 않습니다.

회귀 테스트는 외부 서비스나 데이터베이스 없이 실행됩니다.

```powershell
.\eng\tests\validate-changes.tests.ps1
```

이 도구는 서버 실행, DB 초기화, hosted job, 외부 API 호출을 수행하지 않습니다. 로그에는 대상과 artifacts 경로만 출력하며 키나 연결 문자열을 인자로 받지 않습니다.
