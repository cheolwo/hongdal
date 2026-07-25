# Ssalddel 서버 작업 지침

이 폴더에서는 저장소 루트 `AGENTS.md`와 함께 아래 서버 경계를 적용한다.

## 호출과 상태 변경

- 기본 호출 방향은 `Controller API -> UseCase/Command -> Domain/Infrastructure -> DB/Event/Outbox`다.
- OS는 원장과 규칙을 읽어 순서·정책·handoff를 조율한다. 엔진은 후보·점수·분류를 반환할 뿐 영속 상태를 확정하지 않는다.
- 상태 전이는 서버가 권한과 현재 상태를 검증하고, client는 성공 응답 또는 server event 뒤 같은 원장을 다시 조회한다.
- 새 Controller나 DTO보다 기존 route, UseCase, metadata, contract를 먼저 재사용한다.
- 하나의 EventHandler는 하나의 후속 관심사만 맡긴다. 원본과 반드시 함께 성공해야 하는 일은 같은 transaction, 재시도 가능한 알림·투영·감사·추천은 분리한다.

## Common과 커뮤니티

- `Controllers/Common`은 여러 역할 앱이 같은 의미로 수행하는 공동 업무 API다. 기술적으로 재사용된다는 이유만으로 배치하지 않는다.
- 커뮤니티 탐색, 참여, 공동 원장, 친구 요청·수락, 상품과 업무 신뢰 환류처럼 역할을 넘어 이어지는 업무를 포함한다.
- 공개 커뮤니티 Controller는 `Ssalddel.Controllers.Common` 경계에 두고 `SsalddelCommunityV0Module`, API 업무 분류와 기존 route contract를 유지한다.
- 공유 DTO와 catalog는 `Ssalddel.Contracts/Common/Community`, DB·UI와 무관한 판정·정책은 `Ssalddel.Community`, 영속 workflow는 `Services/Community`의 UseCase에 둔다.
- Admin의 커뮤니티 작성·운영 기능은 권한과 운영 책임이 다르므로 `Controllers/Admin`에 유지하고 Common 공개 API와 합치지 않는다.
- version·Feature bootstrap, push installation, file transport, 외부 callback과 localization bootstrap은 `Controllers/Platform`에 둔다.

## 영속성과 외부 경계

- MongoDB 원장은 원장 블록·다이어그램·표시 옵션의 업무 원본이고, RDB는 권한·조회·정산·보고·안정 투영을 맡는다.
- 원장과 RDB의 양방향 동기화는 재처리 가능하고 멱등해야 하며 순환 Event 발행을 막는다.
- 개인정보 암복호화는 domain property가 아니라 persistence/infrastructure 경계에서 처리한다.
- 외부 API는 interface, options, typed client, DTO, service로 나누고 timeout, cancellation, 오류 응답과 retry 가능성을 고려한다.
- 운영 저장이나 API 실패를 sample fallback으로 숨기지 않는다.

## 탐색과 검증

- 여러 프로젝트를 통과하면 `SsalddelCodeMetadataAttribute`, `SsalddelCodeFeatureKeys`부터 검색한다.
- 커뮤니티 0.0은 `[SsalddelCommunityV0Module]`과 `0.0-A~E` catalog를 먼저 확인한다.
- 상태 전이 변경은 저장, RDB 투영, Event 재처리, 권한, 다른 client의 재조회를 검증한다.
- 수정 직후에는 `powershell -NoProfile -ExecutionPolicy Bypass -File eng/validate-changes.ps1 -Level Fast`, 완료 전에는 같은 명령의 `-Level Task`를 우선 사용한다.

세부 층위는 `docs/Architecture/HIOPSLayerModel.md`, Event 경계는 `docs/Architecture/CommandEvent리팩토링원칙.md`, metadata 규약은 `docs/Architecture/SsalddelCodeMetadata.md`를 따른다.
