# Ssalddel 커뮤니티 공통 module 작업 지침

이 project는 `Common` 제품 범위에 포함되는 커뮤니티 규칙 module이다. 저장소 루트 `AGENTS.md`와 함께 아래 경계를 적용한다.

## 책임

- 게시글, 참여, 공동 원장과 인연 형성에 필요한 저장소 독립 판정·정책·contract를 제공한다.
- 여러 앱이 같은 커뮤니티 의미를 재사용할 수 있게 하되 API, UI, DB와 외부 효과를 직접 소유하지 않는다.
- 기술 역할은 영어, 업무·도메인 의미는 한국어로 쓴다.

## 의존 방향

- 이 project는 `Ssalddel.Contracts`에만 의존한다.
- 공개 HTTP 경계는 `Ssalddel/Controllers/Common`, 영속 상태 전이는 `Ssalddel/Services/Community`, 공유 DTO는 `Ssalddel.Contracts/Common/Community`에 둔다.
- `Ssalddel`, `Ssalddel.Ui.Common`, Entity Framework Core, MongoDB driver를 참조하지 않는다.
- 판정 결과는 후보와 검증 결과만 반환하고 저장·상태 전이·Event 발행은 server UseCase가 수행한다.

## 검증

- 새 공개 module은 `SsalddelCommunityV0Module` metadata와 책임·경계를 명시한다.
- `SsalddelCommunityV0ModuleMetadataTests`로 module catalog와 의존성 방향을 확인한다.
