# Synty Web 조합물 검토 폐루프 계획

## 목적과 안전 경계

Unity에서 만든 Synty 공간 조합물을 네 시점 PNG로 고정하고, 서버가 이미지 위치와 검토 이력을 관리하며, 휴대폰 Web에서 사람이 후보 판단을 남기는 시각 검토 폐루프를 만든다.

이 기능은 배달 운행 중 신호 대기나 정체 시간을 이용하기 위한 기능이 아니다. **합법적인 장소에 완전히 주차하고 운전을 종료한 뒤에만** 사용한다. Web 화면은 `안전하게 주차했습니다` 확인 전 판단 입력을 잠근다.

휴대폰의 `좋음`은 `ReviewedCandidate`다. 다음 사실을 만들지 않는다.

- `ApprovedForSceneApply`
- H1·H2·H3·H4 승인
- E4·E5·E6·E7 증거
- Scene 적용
- Simulation 상태나 운영 업무 결과

## 2026-08-19 구현 상태

| 단계 | 상태 | 현재 증거 |
| --- | --- | --- |
| P0 검토 상태·v2 계약 | 구현 | 부모 촬영 묶음, 조립 입력 hash, Rendering Profile hash, 예상 revision과 재촬영 경합 검사 |
| P1 이미지 저장 폐루프 | 구현·실환경 미검증 | PNG 검증·서버 재인코딩, 불변 object 저장, 업로드 영수증 Mongo 원장, 원본/저장 hash 분리 |
| P2 전용 WebApp 1카드 | 구현·실기기 미검증 | 일반 업무 WebApp과 분리된 관리자 앱, 4시점 전환, 확대 보기, 판단·문제·메모, 이력, 공개 이미지 경고, 주차 잠금 |
| P3 Unity 1카드 | 구현·로컬 촬영 검증 | 임시 전용 Capture Stage, 다섯 팩 실제 Prefab, 1600×900 PNG 네 장, v2 업로드·batch 등록 client |
| P4 재촬영 | 구현·실HTTP 미검증 | `NeedsRevision` 조회, 부모 bundle·조립 입력·expected revision 대조, 새 bundle 등록 |
| P5 발전소 확대 | 후속 | 회복·위협 × A/B/C의 실제 조립 6개와 Normal·Intensified 표현 결합 |
| P6 모바일 운영 검증 | 후속 | 12카드·48이미지 Blob/Mongo 등록과 실제 휴대폰 검토 |

현재 검토 화면은 별도 `Ssalddel.Web.UnityReviewApp`의 `/`, `/reviews/compositions`, 호환 주소 `/world/review/compositions`에서 열며 서버관리자 로그인이 필요하다. 일반 `Ssalddel.WebApp`과 역할별 WebApp에는 이 화면과 검토 Client를 포함하지 않는다. Unity 로컬 촬영은 실제 PNG를 생성했지만 Azure Blob·MongoDB·Web까지의 실HTTP 왕복과 실제 휴대폰 렌더는 아직 실행 증거가 아니다.

## 전체 흐름

```text
조합 입력과 Rendering Profile
        ↓
Unity 전용 Capture Stage
        ↓
4시점 PNG + SourceCompositionHash + CaptureBundleHash
        ↓
서버 PNG 검증·재인코딩
        ↓
불변 Blob object ── ContainerName + ObjectName + StoredImageSha256
        ↓
CaptureUploadReceipt Mongo 원장
        ↓
Review Batch v2
        ↓
Web 사람 검토
  ├─ Good → ReviewedCandidate
  ├─ CompareCandidate
  ├─ OnHold
  └─ NeedsRevision
          ↓
     Unity 재촬영 조회
          ↓ ParentCaptureBundleHash + ExpectedRevision
     새 CaptureBundle
          ↓
     ReadyForReview ───────────────────────────────↺
```

Unity는 Blob URL을 결정하지 않는다. PNG와 고유 식별자·hash만 서버에 보내고, 서버가 `CaptureUploadId`와 저장 위치 영수증을 발급한다. Review Batch v2는 임의 외부 URL이 아니라 이 영수증을 참조한다.

## v1과 v2 계약

`synty-composition-review-batch.v1`은 기존 URL 기반 batch를 읽기 위한 호환 계약으로 유지한다. 새 Unity 촬영은 `synty-composition-review-batch.v2`만 쓴다.

v2 한 검토 항목의 핵심 값은 다음과 같다.

| 값 | 의미 |
| --- | --- |
| `ReviewItemStableId` | 같은 사람이 이어서 판단할 검토 항목 |
| `ExpectedRevision` | 서버 원장의 낡은 덮어쓰기를 막는 예상 개정 |
| `CompositionInputHash` | Unity 업로드의 `SourceCompositionHash`와 같은 조립 입력 지문 |
| `PlanHash` | 기준 공간 조립 계획 지문 |
| `RenderingProfileHash` | 해상도·카메라·조명·상태 표현 입력 지문 |
| `ParentCaptureBundleHash` | 재촬영 대상이 된 직전 촬영 묶음, 최초 촬영은 빈 값 |
| `CaptureBundleHash` | 네 PNG와 조립·표현·부모 계보를 봉인한 새 촬영 묶음 |
| `CaptureUploadId` | 서버가 검증하고 저장한 이미지 한 장의 불변 영수증 |

최초 등록은 `ExpectedRevision=0`, 빈 `ParentCaptureBundleHash`를 요구한다. 재촬영은 다음 값이 모두 현재 원장과 맞아야 `NeedsRevision → ReadyForReview`로 돌아간다.

1. `ExpectedRevision`
2. `ParentCaptureBundleHash`
3. `CompositionInputHash`
4. 새 `CaptureBundleHash`

원본 조립 입력이 바뀌었으면 재촬영으로 기존 판단을 되살리지 않는다. 기존 정책대로 `Stale` 갱신과 새 사람 검토가 먼저다. 늦게 도착한 예전 재촬영은 `409` 충돌로 거부한다.

## 촬영 업로드와 저장 권위

### 관리자 API

| 용도 | HTTP | 경로 |
| --- | --- | --- |
| 검토함 조회 | `GET` | `/api/v1/platform/world-composition-reviews` |
| PNG 업로드·영수증 발급 | `POST multipart/form-data` | `/api/v1/platform/world-composition-reviews/capture-uploads` |
| v1/v2 batch 등록 | `POST` | `/api/v1/platform/world-composition-reviews/batches` |
| Web 판단 기록 | `POST` | `/api/v1/platform/world-composition-reviews/items/{reviewItemStableId}/decisions` |

모든 경로는 `서버관리자전용` 정책을 요구한다.

### PNG 검증과 재인코딩

서버는 업로드를 그대로 공개 저장하지 않는다.

1. 최대 12MiB와 `image/png`를 확인한다.
2. PNG signature, decode 가능 여부와 최대 4096×4096 크기를 확인한다.
3. Unity가 보낸 `ImageSha256`와 실제 수신 bytes의 SHA-256을 대조한다.
4. SkiaSharp로 픽셀을 읽고 새 PNG로 재인코딩해 ancillary metadata를 제거한다.
5. 재인코딩 결과의 SHA-256을 다시 계산한다.

영수증은 두 hash를 분리한다.

- `UploadedSourceSha256`: Unity가 전송한 원본 PNG bytes
- `StoredImageSha256`: 서버 재인코딩 뒤 실제 Blob bytes

Review Batch의 시점별 `ImageSha256`은 최종 저장 이미지인 `StoredImageSha256`을 사용한다.

### 불변 Blob 위치

MongoDB의 권위 저장값은 `ImageUrl` 문자열 하나가 아니다.

- `StorageProviderCode`
- `ContainerName`
- `ObjectName`
- `StoredImageSha256`
- `ContentType`, `ContentLength`, `ETag`

`ImageUrl`은 Web 표시를 위한 조회 결과다. 저장소 hostname이나 공개 방식이 바뀌어도 위치 원장을 다시 쓰지 않고 URL projection을 교체할 수 있다.

Object name은 batch·검토 항목·bundle의 비가역 hash 조각과 `CaptureUploadId`로 만든다. 사용자 이름, 주소, 주문·배송 식별자, 로컬 경로를 넣지 않는다. 같은 영수증은 `create-if-absent`로 멱등 처리하며 기존 object를 덮어쓰지 않는다.

Blob metadata에는 다음 최소값만 둔다.

- `captureUploadStableId`
- `imageSha256`
- `createdAtUtc`

### Blob과 Mongo 부분 실패

Blob과 MongoDB는 하나의 트랜잭션이 아니다. 첫 버전은 다음 순서를 고정한다.

1. PNG 완전 검증·재인코딩
2. 불변 `ObjectName`과 `CaptureUploadId` 결정
3. Blob `create-if-absent`
4. `CaptureUploadReceipt` Mongo 저장

3번 성공 뒤 4번이 실패하면 Blob orphan을 허용한다. 재시도는 같은 불변 object를 덮어쓰지 않고 기존 object를 확인한 뒤 Mongo 영수증 저장을 다시 시도한다. 현재 자동 orphan 삭제 Job은 구현하지 않는다.

## 공개 이미지 경계

현재 검토 이미지는 공개 읽기 Blob을 전제로 한다.

> 검토 화면은 관리자 전용이지만 검토 이미지 객체는 공개 읽기 방식으로 저장됩니다. 이미지 URL을 가진 사람은 로그인 없이 이미지를 열 수 있습니다.

추측하기 어려운 object name은 접근 제어가 아니다. 따라서 촬영 PNG 픽셀에 다음 정보를 렌더링하지 않는다.

- 사용자 이름·관리자 정보
- 서버 주소·인증 상태
- Session ID·주문 ID
- 주소·배송 위치
- access token·secret 일부
- Console·debug overlay
- 로컬 파일 경로

향후 민감 정보가 필요해지면 `Private Blob + 서버 proxy` 또는 짧은 SAS URL로 전환한다. Mongo가 URL 대신 저장 위치를 보존하므로 검토 원장 구조는 유지할 수 있다.

## Unity 전용 Capture Stage

### 조립과 Scene 경계

첫 1카드는 `회복 발전소 A · Normal`이다. 실제 다섯 팩에서 다음 역할을 가져온다.

| 팩 | 1카드 대표 재료 | 역할 |
| --- | --- | --- |
| Construction | 대형 발전기·급수탑 | 중심 설비와 복구 기능층 |
| Nature | 버드나무·꽃밭 | 회복 분위기 |
| Farm | 풍차·소형 사일로 | 생산·전력 보조 |
| Town | 벤치·야외등 | 휴식·생활 흔적 |
| City | PowerBox·전력 케이블 | 기반시설 연결 |

기존 `SimulationWorldShell`, `WI공간모판검토실`, 기존 Builder와 Build Settings를 수정하지 않는다. Unity Editor 도구는 저장하지 않는 임시 Additive Scene을 만들고 촬영 뒤 원래 활성 Scene으로 복귀한다.

촬영 Stage의 불변 조건은 다음과 같다.

- 조합물 Renderer 전체를 전용 layer 31에 둔다.
- `CaptureCamera`의 culling mask는 전용 layer 하나뿐이다.
- 조합물 root 아래 일반 `Canvas`와 Camera를 허용하지 않는다.
- 애플리케이션 HUD·서버 상태·Console을 캡처하지 않는다.
- 검토용 제목·시점명은 PNG 픽셀이 아니라 Web 카드에 표시한다.
- 정면 3/4, 후면 3/4, 좌측면, 상부 사선 네 시점을 1600×900 PNG로 만든다.
- 네 이미지 hash가 모두 달라야 하며 같은 빈 frame 반복을 실패로 처리한다.

### Editor 메뉴

| 메뉴 | 용도 |
| --- | --- |
| `Ssalddel/Synty Web 검토/01 ... 4시점 로컬 촬영` | 로컬 PNG와 manifest 생성, 단축키 `F10` |
| `.../02 ... 최초 업로드 및 등록` | 네 PNG 업로드 영수증을 받은 뒤 v2 batch 최초 등록 |
| `.../03 NeedsRevision ... 재촬영 및 등록` | 현재 부모 bundle·revision·조립 hash를 조회한 뒤 재촬영 |

로컬 결과는 Unity 프로젝트의 `artifacts/local/synty-web-review/<UTC>/`에 둔다. `capture-manifest.json`은 로컬 검증 자료이며 공개 Blob에 올리지 않는다.

Unity를 시작하기 전에 관리자 단기 access token을 환경 변수로 전달한다. source, Editor asset, manifest와 로그에는 값을 저장하지 않는다.

```text
SSALDDEL_UNITY_ADMIN_ACCESS_TOKEN
SSALDDEL_OPERATIONAL_API_BASE_URL   # 생략 시 https://localhost:7117/
```

개발 인증서 경고를 우회하는 certificate handler는 두지 않는다. 로컬 HTTPS 인증서는 운영체제와 Unity가 정상 신뢰하도록 구성한다.

## Web 검토

### 전용 WebApp 경계

Unity 촬영 산출물은 커뮤니티·주문·운송 등 일반 업무 화면과 배포 주기, 공개 이미지 정책, 관리자 권한이 다르므로 물리 프로젝트를 분리한다.

| 구분 | 경로 | 책임 |
| --- | --- | --- |
| Unity 산출물 검토 | `Ssalddel.Web.UnityReviewApp/` | 촬영 이미지, 검토 이력, 후보 판단, 오프라인 판단 대기열 |
| 일반 통합 Web | `Ssalddel.WebApp/` | 커뮤니티·공공데이터·업무 화면 |
| 역할별 Web | `Ssalddel.Web.*App/` | 01~05 역할별 업무 화면 |

전용 앱은 `Ssalddel.Contracts`와 공통 인증 토큰 구조만 참조하고 `Ssalddel.WebApp` 프로젝트·페이지·레이아웃·서비스를 참조하지 않는다. 인증 localStorage와 검토 오프라인 대기열도 `ssalddel.unity-review.*` 키를 사용해 일반 WebApp 브라우저 상태와 분리한다. 검토 API 요청에는 전용 앱이 복구한 Bearer token을 명시적으로 붙이며, 로그인 성공 뒤에도 `서버관리자` 역할이 없으면 토큰을 즉시 지운다.

전용 앱 내부도 다음 책임으로 나눈다.

| 구성 | 책임 | 금지 경계 |
| --- | --- | --- |
| `Synty공간조립Web검토Page.razor` | 모바일 표시와 사용자 입력 binding | API 호출·localStorage 직접 접근·상태 전이 계산 |
| page code-behind | 로그인 복구·로그아웃·초기화 생명주기 | 검토 목록·판단 대기열 소유 |
| `Synty공간조립검토Workspace` | 카드 선택·4시점 전환·판단·재전송 화면 상태 | HTTP 구현·브라우저 저장 API 직접 접근 |
| `ISynty공간조립모바일검토Client` 구현 | Bearer API 조회·판단 전송 | 오프라인 데이터 영속 |
| `Synty공간조립오프라인검토Store` | 전용 localStorage 대기열 조회·추가·삭제 | 서버 성공 상태의 권위 판단 |

통합 서버는 검토·촬영 UseCase와 Mongo·메모리 `Stores`를 분리한다. 무료 VM 미리보기는 같은 상태 전이·PNG 검증 소스를 `Ssalddel.UnityReview.Core`로 재사용하되 별도 `Ssalddel.UnityReview.Api`와 MySQL 원장 adapter를 사용한다. Unity는 공개 `Synty공간조립Web검토CapturePipeline` 진입점을 유지한 채 orchestration, Capture Stage·카메라, API client, 전송 model을 파일로 나눈다. 배포 adapter가 달라도 API route, JSON field, Stable ID, 불변 업로드 영수증과 `Good ≠ 승인` 의미는 바꾸지 않는다.

전용 개발 솔루션은 `Ssalddel.UnityReview.slnx`다. 일반 제품 배포 대상인 `Ssalddel.v3.5.slnx`와 역할별 `Ssalddel.RoleWebApps.slnx`에는 자동 포함하지 않는다. 로컬 기본 주소는 `https://localhost:7286`, 서버 API 기본 주소는 `https://localhost:7117`이며 다음처럼 실행한다.

```powershell
dotnet run --project Ssalddel.Web.UnityReviewApp/Ssalddel.Web.UnityReviewApp.csproj
```

### 무료 VM 독립 배포

Unity 검토 앱은 기존 역할별 WebApp VM의 `/unity-review/` 하위 경로를 사용하지 않고 별도 무료 대상 VM과 hostname으로 이동한다. `deploy/azure-unity-review-vm/`이 다음 최소 Docker 스택을 소유한다.

| 컨테이너·볼륨 | 책임 | 무료 VM 제한 |
| --- | --- | --- |
| Caddy | 전용 WebAssembly 정적 파일, HTTPS, API 역방향 전달, 공개 이미지 읽기 | 64MB 상한 |
| `Ssalddel.UnityReview.Api` | 전용 관리자 로그인, PNG 검증·재인코딩, 검토 상태 전이 | 320MB 상한 |
| MySQL 8.4 | 촬영 영수증·검토 snapshot·개정 번호 | 384MB 상한 |
| `review_images` | hash 기반 불변 PNG | API 쓰기·Caddy 읽기 전용 |

MongoDB와 역할별 업무 API·DB는 이 VM에 넣지 않는다. 관리자는 전용 PBKDF2 비밀번호와 JWT를 사용하며 비밀번호 원문은 API 프로젝트의 .NET User Secrets에만 둔다. 서버 `.env`에는 PBKDF2 결과, JWT signing key와 MySQL 비밀값만 두고 Git에 포함하지 않는다. VM은 2GB swap을 준비하지만 Compose 메모리 상한을 먼저 적용한다.

미리보기 adapter는 `ContainerName + ObjectName + StoredImageSha256`을 MySQL에 저장하고 Caddy가 `/local-storage/`를 공개 읽기로 투영한다. URL은 권위값이 아니며 향후 Azure Blob·Mongo adapter로 돌아가도 Stable ID와 영수증 계약은 유지한다. 로컬 volume은 단일 VM 장애에 대한 내구 저장소가 아니므로 운영 승인 증거나 유일한 백업으로 사용하지 않는다.

화면은 한 카드에서 네 시점을 전환하고 원본 크기 lightbox로 확대한다. 최신 판단 이력, 원장 revision, 팩 활용 비율, 조립·Rendering hash를 함께 표시한다.

| 화면 | 결정 코드 | 서버 검토 상태 | 의미 |
| --- | --- | --- | --- |
| 좋음 | `Good` | `ReviewedCandidate` | PC에서 다시 볼 유망 후보 |
| 수정 필요 | `NeedsRevision` | `NeedsRevision` | 문제·메모를 남기고 Unity 재촬영 대기 |
| 보류 | `OnHold` | `OnHold` | 현재 판단하지 않음 |
| 비교 후보 | `CompareCandidate` | `CompareCandidate` | 다른 A/B/C 또는 상태와 나란히 비교 |

각 판단은 다음 최소값만 전송한다.

- `reviewItemStableId`
- `expectedRevision`
- `idempotencyKey`
- `decisionCode`
- 선택적인 `issueCodes`와 500자 이하 `note`

`NeedsRevision`에는 문제 꼬리표나 메모가 하나 이상 필요하다. Unity 표현은 업무 결과나 발전소 수치를 계산해서 보내지 않는다.

HTTP 연결 자체가 실패하면 판단 요청을 브라우저 `localStorage`에 임시 저장한다. `401`·`403`은 로그인·권한 문제로 표시하고, `409`는 자동 덮어쓰지 않는다. 현재 전체 화면을 오프라인 제공하는 서비스 워커 PWA는 아니다.

## 12카드와 48이미지 확대

공간 배치와 상태 표현을 분리한다.

```text
Base spatial composition
              │
      ┌───────┴───────┐
     회복             위협
      │                │
   A  B  C          A  B  C
   │  │  │          │  │  │
 Normal / Intensified
```

- A/B/C: 같은 바닥 면적·출입구·외부 연결점·핵심 소켓을 유지하는 공간 조립 변형
- Normal/Intensified: 조명·연기·식생·손상·차단물의 Rendering Profile 변형

`2개 발전소 × 3개 공간 변형 × 2개 상태 표현 = 12개 검토 카드`이고, 카드당 네 시점이면 48개 이미지다.

12개 빈 촬영 초안은 다음 명령으로 만든다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File eng/world-seedbeds/synty-mobile-review/new-nature-plant-review-batch.ps1
```

기본 출력은 `artifacts/local/synty-mobile-review/nature-plant-review-batch.v2.json`이다. `ExpectedRevision=0`, 빈 부모 bundle과 빈 `captures`를 가진 `WaitingForCapture` 설계 초안이며 Unity·Game View·Blob 증거가 아니다.

실제 확대 순서는 다음과 같다.

1. 회복 A Normal 1카드의 전용 VM 이미지 volume·MySQL·Web·재촬영 실HTTP 폐루프를 통과한다.
2. 회복 A의 상태 표현을 Normal/Intensified로 분리한다.
3. 회복 B/C로 공간 불변 조건을 검증한다.
4. 위협 A를 같은 계약으로 연결한다.
5. 위협 B/C와 두 상태를 확장한다.
6. 실제 12카드·48이미지를 휴대폰에서 검토한다.

## 검증과 증거 수준

| 검증 | 완료 조건 |
| --- | --- |
| 서버 집중 시험 | v1 호환, v2 receipt, 재인코딩, 두 hash, 멱등 object, 부모 bundle, revision 충돌, `Stale`, `Good ≠ 승인` |
| Web 프로젝트 분리·컴파일 | 전용 앱만 검토 화면·Client를 소유하고 일반 WebApp에는 관련 route·DI가 없으며 두 프로젝트가 각각 컴파일 |
| Unity 코드 컴파일 | 전용 Stage, 실제 다섯 팩 경로, 업로드 client, 재촬영 조회가 오류 없이 import |
| Unity 로컬 촬영 | 네 개 1600×900 PNG, 서로 다른 hash, 원래 Scene 복귀, 민감 UI 없음 |
| 실서버 왕복 | PNG 업로드 → 불변 저장 위치 → MySQL receipt → batch → Web 조회 |
| 재촬영 왕복 | Web `NeedsRevision` → Unity queue → 새 bundle → `ReadyForReview` |
| 실제 휴대폰 | 로그인, 주차 잠금, 세로 가독성, 터치, 이미지 확대, 오프라인·재연결 |

현재 전용 Docker 스택은 로컬에서 MySQL schema, 전용 관리자 로그인, 무인증 401, PNG 4개 불변 업로드, 12개 batch, `ReadyForReview → ReviewedCandidate`, API 재시작 뒤 원장 복원과 이미지 hash를 통과했다. 이 로컬 시험은 임시 Unity Game View 한 장을 네 영수증에 재사용한 전송 기준작이므로 실제 H 조합물 4시점 증거가 아니다. Azure 구독은 `Warned / FreeTrial / spendingLimit On`으로 쓰기가 차단되어 별도 VM·공개 HTTPS·휴대폰 브라우저는 아직 미검증이다.
