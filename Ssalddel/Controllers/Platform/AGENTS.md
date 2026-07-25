# Ssalddel Platform Controller 작업 지침

이 폴더는 여러 업무에서 재사용되지만 그 자체가 사용자의 공동 업무는 아닌 기술 API 경계다. 저장소 루트와 `Ssalddel/AGENTS.md`를 함께 따른다.

## 포함 기준

- version·Feature bootstrap, client installation, file transport, 외부 callback, localization bootstrap처럼 플랫폼 실행을 지원하는 API를 둔다.
- 단지 여러 앱이 호출한다는 이유만으로 `Common`에 두지 않는다. 사용자가 공동 원장, 참여, 관계, 상품, 운송, 창고처럼 같은 업무 의미를 공유하면 `Controllers/Common`에 둔다.
- route, authorization, rate limit과 외부 contract는 기존 호환성을 유지한다.

## 구현 경계

- Controller는 adapter 역할만 맡고 업무 상태 변경은 UseCase·Command에 위임한다.
- 기술 역할은 영어, 업무·도메인 의미는 한국어로 쓴다.
- Platform 분류가 권한이나 운영 Feature 경계를 대신하지 않는다.
