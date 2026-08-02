# Google Maps 브라우저 키 배포 보안

## 적용 범위

이 문서는 `Ssalddel.WebApp`의 `/community/home`에서 Maps JavaScript API를 로드하는 전용 브라우저 키만 다룬다. 브라우저 키는 최종 사용자에게 전달되는 값이므로 비밀로 숨기는 방식이 아니라 **전용 키 분리, HTTP referrer 제한, API 제한, quota·사용량 감시**로 보호한다.

`GoogleMaps:UnifiedApiKey`, 서버용 Geocoding·Routes 키, Android 앱 키를 Web 지도에 재사용하지 않는다.

## Google Cloud 필수 제한

환경별로 별도 프로젝트 또는 최소한 별도 키를 사용하고 다음 제한을 모두 설정한다.

1. Application restrictions를 `Websites`로 설정한다.
2. 운영 키에는 실제 HTTPS origin의 referrer만 허용한다. 개발용 `localhost`는 운영 키에 넣지 않고 별도 개발 키로 분리한다.
3. API restrictions를 `Maps JavaScript API` 하나로 제한한다.
4. 일별 quota와 예산 알림을 설정하고 예상하지 못한 origin·사용량 증가를 감시한다.
5. 노출 또는 오용 징후가 있으면 새 제한 키를 발급해 배포한 뒤 기존 키를 폐기한다.

Google Cloud Console의 제한 변경은 실제 운영 상태 변경이며, 해당 프로젝트와 허용 origin을 확인한 운영자가 수행한다.

## 배포 산출물 주입

저장소의 `Ssalddel.WebApp/wwwroot/runtime-config.js`는 항상 빈 설정만 유지한다. `dotnet publish`가 끝난 뒤 CI/CD 비밀 저장소의 전용 키를 publish 산출물에만 주입한다.

```powershell
$env:SSALDDEL_GOOGLE_MAPS_BROWSER_API_KEY = '<CI secret에서 제공>'
powershell -NoProfile -ExecutionPolicy Bypass -File eng/inject-web-runtime-config.ps1 `
  -PublishRoot artifacts/publish/Ssalddel.WebApp `
  -AllowedOrigins 'https://map.example.com'
Remove-Item Env:SSALDDEL_GOOGLE_MAPS_BROWSER_API_KEY
```

명령줄 인수에 실제 키를 직접 적지 않는다. 로컬 개발에서는 전용 `GoogleMaps:BrowserApiKey` user-secret을 선택적으로 읽을 수 있다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File eng/inject-web-runtime-config.ps1 `
  -PublishRoot artifacts/publish/Ssalddel.WebApp `
  -AllowedOrigins 'http://localhost:5238' `
  -AllowLoopback `
  -UserSecretsProject Ssalddel/Ssalddel.csproj
```

주입 스크립트는 다음 경우 실패한다.

- 전용 키가 없거나 Google 브라우저 키 형식이 아닌 경우
- 비 HTTPS 외부 origin, path·query·fragment가 포함된 origin인 경우
- 명시적인 `-AllowLoopback` 없이 loopback origin을 넣거나 개발·운영 origin을 한 키에 섞은 경우
- publish 산출물이 아니라 tracked WebApp source에 쓰려고 하는 경우

## Web 실행 경계

- `runtime-config.js`는 Blazor boot보다 먼저 로드한다.
- 주입 시 publish된 `index.html`에 배포별 cache token을 붙여 키 회전 뒤 이전 runtime 설정 재사용을 줄인다.
- 로더는 현재 origin이 배포 allowlist와 정확히 일치하는지 확인한 뒤 키를 소비한다.
- 외부 origin은 HTTPS만 허용한다. loopback HTTP는 로컬 개발에서만 허용한다.
- 키를 읽은 뒤 전역 runtime 값을 삭제하고 동적 Google loader script를 DOM에서 제거한다. 과거 meta-key fallback은 허용하지 않는다. 이는 화면 잔존을 줄일 뿐 키를 비밀로 만들지는 않는다.
- origin 불일치, 미설정 또는 Google 로드 실패 시 Google API를 호출하지 않고 기존 SVG 지도로 전환한다.
- 배포 서버는 `runtime-config.js`에 `Cache-Control: no-store`를 적용하고 서비스 워커 precache 대상에서 제외한다.

## 배포 전 검증

1. `git grep`으로 tracked source에 실제 `AIza...` 키가 없는지 확인한다.
2. 허용 origin에서는 Google tile과 Data marker가 표시되고 인증 오류가 없는지 확인한다.
3. 허용되지 않은 HTTPS origin과 비 HTTPS 외부 origin에서는 `blocked-origin` fallback이 표시되는지 확인한다.
4. Google Cloud Metrics에서 호출 API가 Maps JavaScript API로 한정되는지 확인한다.
5. 브라우저 키와 서버·Android 키가 서로 다른 credential인지 확인한다.

빌드·구성 테스트는 주입 경계를 확인하지만 Google Cloud Console의 실제 referrer/API 제한 상태를 증명하지 않는다. 운영 전에는 Console 설정과 실제 허용·차단 origin을 별도로 검증한다.
