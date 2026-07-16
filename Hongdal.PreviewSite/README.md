# Hongdal Preview Site

Hongdal의 역할별 업무 화면을 로그인이나 API 서버 없이 둘러보는 읽기 전용 체험 사이트입니다.

`scripts/build.ps1`은 `src`와 Cloudflare Worker 진입점을 Sites 배포 형식인 `dist`로 조립합니다. 공개 경로는 모두 클라이언트 라우팅과 Worker의 HTML fallback을 함께 사용합니다.

