# 01~05 역할 분리 WebApp

## 목적

상시 체험 포털에서 모든 역할 페이지를 하나의 Blazor WebAssembly 앱에 포함하면 첫 진입 때
현재 역할과 무관한 화면 코드까지 함께 내려받는다. 이를 피하기 위해 포털과 역할 앱을 다음
여섯 개의 독립 배포 단위로 나눈다.

| 공개 경로 | 배포 단위 | 기본 화면 |
| --- | --- | --- |
| `/` | 정적 역할 선택 포털 | 01~05 앱 선택 |
| `/roles/01/` | `Ssalddel.Web.CommunityApp` | 커뮤니티 |
| `/roles/02/` | `Ssalddel.Web.OrdererApp` | 주문자 |
| `/roles/03/` | `Ssalddel.Web.ShipperApp` | 화주 |
| `/roles/04/` | `Ssalddel.Web.DriverApp` | 기사 |
| `/roles/05/` | `Ssalddel.Web.WarehouseApp` | 창고 |

루트 포털은 JavaScript와 WebAssembly를 실행하지 않는다. 사용자가 역할을 선택한 뒤에만
해당 역할 앱의 Blazor 런타임과 화면 코드를 내려받는다. 앱을 바꾸면 현재 앱의 화면 코드는
브라우저 문서와 함께 내려가고 선택한 앱을 새로 시작한다.

## 공유 경계

- 다섯 앱은 `eng/web-role-app/`의 공통 부트스트랩, 레이아웃, 역할 전환기를 사용한다.
- API는 역할 경로가 아니라 같은 출처의 `/api`를 사용하므로 서버 계약과 원장은 분리하지 않는다.
- 로그인 세션, 권한, 공동 원장과 상태 전이는 기존 서버가 계속 단일 기준이다.
- 역할별 프로젝트는 자기 업무 페이지와 필요한 공용 화면만 컴파일한다.
- `/roles/{code}/...` 하위 주소는 Caddy의 SPA fallback으로 같은 역할 앱에 되돌린다.
- 기존 `/community`, `/orderer`, `/shipper`, `/driver`, `/warehouse` 계열 주소는
  대응 역할 앱으로 영구 이동해 저장된 링크를 보존한다.

## 배포와 검증

역할 앱만 묶어 확인할 때는 다음 solution을 빌드한다.

```powershell
dotnet build Ssalddel.RoleWebApps.slnx
```

Azure 미리보기 묶음은 `deploy/azure-vm/package-web-preview.ps1`로 만든다. 산출물의
`preview-build.json`은 `webAppMode: RoleSeparated`와 `01`~`05` 역할 목록을 기록한다.
배포 스크립트는 웹 루트와 Caddy 설정을 각각 백업한 뒤 Caddy만 다시 만들며 API와 DB는
재시작하지 않는다.

실제 확인에서는 루트 포털의 script 수가 0인지, 01~05 기본 화면이 모두 렌더링되는지,
역할별 하위 주소 직접 진입과 기존 주소 이동이 정상인지 별도로 확인한다.

## 한계

각 역할 앱도 공용 UI, 인증, HTTP client와 Blazor 런타임은 자체적으로 포함한다. 따라서
역할 앱을 동시에 여러 탭에서 열면 런타임도 탭마다 존재한다. 이 구조의 목표는 단일 탭의
첫 진입과 역할 전환 시 불필요한 역할 화면 코드를 배제하는 것이며, 서버 원장이나 업무
계약을 앱별로 복제하는 것이 아니다.
