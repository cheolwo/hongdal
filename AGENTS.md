# Ssalddel 공통 작업 지침

이 파일은 모든 AI 도구와 스레드가 항상 알아야 하는 공통 경계와 작업 routing만 정의한다. 폴더별 세부 규칙은 가까운 `AGENTS.md`와 기준 문서에서 필요한 부분만 읽는다.

## AI 공용 컨텍스트

GPT Chat과 Codex는 대화 기록이 아니라 저장소 문서를 공용 기억으로 사용한다. 작업을 시작할 때 다음 순서로 현재 상태를 확인한다.

1. [공용 프로젝트 컨텍스트](docs/ProjectOverview/GptProjectContext.md)
2. [확정 결정](docs/AI/DECISIONS.md)
3. [현재 작업](docs/AI/CURRENT_WORK.md)
4. 작업 경로에 가까운 `AGENTS.md`와 관련 Architecture 문서

Unity 개발 순서는 제품 릴리스 버전 순서와 별개다. Unity는 전체 Ssalddel 도메인을 `World`, `Data`, `Object`, `Interaction`, `Simulation` 관점에서 다루되, 실제 구현은 검증 가능한 좁은 vertical slice로 진행한다. 서버는 운영 상태의 최종 권위이며 Unity의 simulation과 operational data를 명확히 구분한다.

의미 있는 구현이나 설계 작업을 마치면 `docs/AI/CURRENT_WORK.md`를 누적 일지가 아닌 최신 snapshot으로 갱신한다. 변경 파일, 검증한 범위, runtime 검증 여부와 남은 문제를 사실대로 기록한다. 장기간 유지할 결정이 바뀌면 `docs/AI/DECISIONS.md`에 새 결정이나 대체 관계를 기록하고 과거 결정을 조용히 덮어쓰지 않는다.

## 우선순위와 적용 범위

1. 시스템/개발자 지침과 현재 스레드의 최신 사용자 요청을 먼저 따른다.
2. 작업 대상 폴더에 더 가까운 `AGENTS.md`가 있으면 함께 읽고 그 범위에서는 가까운 지침을 우선한다.
3. 별도 지시가 없으면 [0.0 커뮤니티·공공데이터 집중 로드맵](docs/Versions/v0.0/focus-roadmap.md)을 기본 우선순위로 삼는다.
4. 문서와 코드가 다르면 실제 route, contract, test, 실행 설정을 확인하고 차이를 알린다. 추측으로 한쪽을 덮지 않는다.
5. 과거 대화나 오래된 선호보다 현재 저장소 문서와 최신 사용자 요청을 우선한다.

## 경로별 routing

| 작업 경로 또는 종류 | 먼저 읽을 지침 | 핵심 기준 |
| --- | --- | --- |
| `Ssalddel/` server·API·Event | `Ssalddel/AGENTS.md` | 업무 실행 책임, Command/Event, metadata |
| `Ssalddel/Controllers/Common/`, `Ssalddel.Community/`, `Ssalddel.Contracts/Common/Community/` | `Ssalddel/AGENTS.md`, `Ssalddel.Community/AGENTS.md` | Common에 포함되는 커뮤니티 API·contract·규칙 경계 |
| `Ssalddel/Controllers/Platform/` | `Ssalddel/Controllers/Platform/AGENTS.md` | 업무 공통과 구분되는 플랫폼 기술 API |
| `Ssalddel.Ui.Common/` 공통 UI | `Ssalddel.Ui.Common/AGENTS.md` | 3단계 navigation, MVVM, render 검증 |
| `Ssalddel.Tests/` test | `Ssalddel.Tests/AGENTS.md` | filter 우선, 영향 project build |
| `docs/` 문서·변경 기록 | `docs/AGENTS.md` | 기준 문서 단일화, link·diff 검증 |
| 여러 project를 통과하는 기능 | `SsalddelCodeMetadataAttribute`, `SsalddelCodeFeatureKeys` 검색 | `FlowOrder`, `Layer`, `Effects`, `Boundary`, `ContractType` |
| 커뮤니티 0.0 | `[SsalddelCommunityV0Module]` 검색 | module catalog와 `0.0-A~E` |
| 기능 slice 작업 | `docs/Architecture/FunctionalWorkAreaPartitioning.md`, `eng/work-areas/<slice>.json` | manifest의 `readFirst`, `sourceRoots`, `excludedRoots`만 먼저 읽고 범위를 넓힐 때 이유를 남김 |
| 지역문화·공공데이터 | `eng/work-areas/regional-culture-public-data.json` | 문화 이미지·지역 key·공식 근거·가격 관측 |
| 댓글·프로필·공개범위 | `eng/work-areas/community-foundation.json` | 0.0 참여·개인정보·신고 보호 |
| 개별 의향·비용 검토 | `eng/work-areas/individual-intent.json` | 0.5 비구속 의향, 결제·발주 금지 |
| 공동구매 | `eng/work-areas/group-purchase.json` | 1.0 별도 동의와 공동 원장 |
| 같이 수입 준비 | `eng/work-areas/trade-readiness.json` | 1.5 전문가 확인과 실행 금지 |

## 시작과 인계

- 시작할 때 `git status --short --branch`를 확인하되, 대규모 결과는 항목 수와 관련 경로만 요약한다.
- 기존 변경은 다른 스레드나 사용자의 작업일 수 있다. 관련 없는 변경을 되돌리거나 정리하거나 함께 stage하지 않는다.
- 같은 파일에 기존 변경이 있으면 patch 직전에 다시 읽고 최소 범위로 합친다. 안전하게 병합할 수 없을 때만 묻는다.
- 검색은 `rg`를 우선하고 `bin`, `obj`, `.vs`, `vendor`, `artifacts`, 생성 산출물과 중첩 worktree를 제외한다.
- 한국어 파일은 UTF-8로 읽고 쓴다.
- 종료할 때 완료한 일, 변경 파일, 실행한 검증, 남은 위험을 짧게 남긴다. 하지 않은 commit·push·배포를 했다고 표현하지 않는다.

## 제품과 운영 경계

- Ssalddel의 중심은 **정보 공개형 커뮤니티**다. 대화와 모집이 공동 원장·다이어그램으로 이어지고 업무 도구는 그 위에 붙는다.
- 현재 기본 공개 기준은 `0.0` 커뮤니티·공공데이터 기반이다. 개발은 `3.5` 전체 제품 범위의 페이지·원장·상태 전이를 세로로 계속 완성할 수 있으며, 현재 요청에 대상 버전이 없을 때의 기본 우선순위만 `0.0`으로 둔다. `0.5` 이후 기능은 준비된 capability부터 릴리즈 게이트에 따라 단계적으로 공개하고, 전용 검증 profile과 운영 요건 없이 기본 노출·외부 효과를 켜지 않는다.
- 최종 배포 목표는 `3.5` 마트·도심 물류다. 현재 완성 단계 `0.0`과 최종 목표를 혼동하지 않으며, 새 페이지는 [전체 로드맵 조화형 페이지 원칙](docs/Architecture/WholeRoadmapPagePrinciple.md)에 따라 `0.0`부터 `3.5`까지의 stable ID, 원장과 역할 인계를 고려한다.
- 유상 화물 추천, 자동 배차, 계약 중개, 운임 수취, 보관, 정산은 허가·제휴·법률·운영 준비 전 기본 비활성이다.
- 실행 효과는 `SsalddelExecution:Mode`의 `Simulation`과 `Operational` 경계로만 통제한다. 별도 실행 모드를 화면이나 API마다 만들지 않는다.
- 개발 검증은 sample data, FakePG, 모의 흐름을 사용한다. 운영 저장이나 API 실패를 sample fallback으로 숨기지 않는다.

제품 경계는 [커뮤니티 0.0 기반 제품 원칙](docs/Architecture/CommunityFoundationV0Policy.md), 개발 철학은 [이웃에서 시작하는 공동행동 개발 철학](docs/Architecture/NeighborCenteredDevelopmentPhilosophy.md)을 따른다.

## 공동행동과 개인정보

- 종교, 국적, 언어, 가족 형태, 경제력은 가입·노출·신뢰 점수·검색 순위·역할 자격의 대리 지표로 쓰지 않는다.
- 관심, 참여, 연락처 공개, 가원장, 실원장, 실행은 명시적 동의와 철회 가능한 상태로 분리한다.
- 절감액과 편익뿐 아니라 비용, 노동, 위험, 담당자와 계산 근거를 함께 드러낸다.
- 지리적 가까움은 수요 집계와 물류 효율 후보로만 사용하고 자동 가입·알림·상대 선택·배차·계약 확정 근거로 쓰지 않는다.
- 효율이 낮은 참여자를 숨기거나 배제하지 않고 모집권·시간창·집결 방식·자격 사업자 참여 같은 대안을 표현한다.
- 업무 로그, 공동 원장, 커뮤니티 이야기, 친구 후보 기록, 친구 요청·수락과 연락처 공개는 서로 분리된 상태로 관리한다. 세부 기준은 [업무 경험에서 친구 요청으로 이어지는 커뮤니티 설계 기준](docs/Architecture/FriendRequestCommunityDesignStandard.md)을 따른다.
- API key와 secret은 source, tracked config, 로그, capture에 넣지 않는다.

## 공통 아키텍처

- 기본 방향은 `화면 -> Controller API -> UseCase/Command -> Domain/Infrastructure -> DB/Event/Outbox`다.
- 영속 상태는 API/UseCase/Command가 권한과 현재 상태를 검증한 뒤 변경한다. engine은 후보·점수·분류를 반환할 뿐 확정하지 않는다.
- MongoDB 원장은 유연한 업무 원본, RDB는 권한·조회·정산·보고·안정 투영을 맡는다.
- Event/Outbox 동기화는 재처리 가능하고 멱등해야 하며 순환 발행을 막는다.
- 여러 앱이 같은 업무 의미로 쓰는 contract는 `Ssalddel.Contracts`, 공통 업무 UI와 workflow는 `Ssalddel.Ui.Common`에 둔다.
- `Common`은 기술 재사용 집합이 아니라 01~05가 함께 수행하는 업무 경계다. 커뮤니티 탐색, 참여, 공동 원장, 친구 요청·수락, 상품과 신뢰 환류처럼 여러 역할이 같은 업무 의미로 사용하는 API·contract·UI를 포함한다.
- version·Feature bootstrap, push installation, file transport, 외부 callback처럼 업무 의미 없이 실행 환경을 지원하는 API는 `Controllers/Platform`에 둔다. 여러 앱이 호출한다는 사실만으로 `Common`에 넣지 않는다.
- 커뮤니티 공개 API는 `Ssalddel/Controllers/Common`, 공유 contract는 `Ssalddel.Contracts/Common/Community`, 저장소와 무관한 커뮤니티 규칙은 `Ssalddel.Community`에 둔다. 물리 project는 의존성 경계를 위해 유지한다.
- 새 Controller, DTO, Entity, abstraction보다 기존 route, UseCase, metadata, contract, value object, shared component를 먼저 재사용한다.
- 개인정보 암복호화는 domain property가 아니라 persistence/infrastructure 경계에서 처리한다.

세부 층위는 [업무 실행 책임 모델](docs/Architecture/BusinessWorkflowResponsibilityModel.md), Event 경계는 [Command/Event 리팩토링 원칙](docs/Architecture/CommandEvent리팩토링원칙.md)을 따른다.

## 코드 명명 언어

- 모든 코드에서 기술 역할은 영어, 업무·도메인 의미는 한국어로 쓴다. `국내공동구매협의Controller`, `생산자후보검색`, `_공공데이터조회UseCase`처럼 `한국어 업무명 + 영어 기술 역할`로 조합한다.
- 상세 용어표, 적용 대상, 외부 표준 예외와 기존 contract 호환 규칙의 단일 기준은 [코드 탐색 메타데이터의 코드 명명 언어](docs/Architecture/SsalddelCodeMetadata.md#코드-명명-언어)다.
- 새 코드와 수정하는 코드에 기준을 적용하되, 기존 이름의 광범위한 변경은 기능 단위로 나누고 공개 API·직렬화·Event·metadata·저장 데이터의 호환성을 검증한다.

## 사람이 읽는 Unity 용어와 출력 언어

- Unity·서버 통합 작업의 문서, README, 주석, 진행·완료 보고와 검증 설명은 [Unity 프로젝트 한국어 중심 용어·출력 지침](docs/AI/UnityKoreanTerminologyGuide.md)을 따른다.
- 프로젝트 개념은 `구성 대장`, `업무 흐름`, `배치 객체`, `고유 식별자`, `데이터 연결`, `상태 사본`, `관점별 조회 결과`, `데이터 계보`, `인계`, `배치 검증 기록`처럼 한국어를 먼저 쓴다.
- Unity·C#·표준 고유 기술명과 기존 클래스명·고유 식별자·API 계약·JSON 필드·저장 값은 유지한다. 처음 한 번 의미 설명이 필요할 때만 영어를 병기한다.
- O0~O6은 코드만 단독 보고하지 않고 후보, 준비, 모판 실행 검증, 실제 World 배치 검증의 의미를 함께 적는다.
- 서버의 최종 사실, 역할별 해석 결과, Unity 표현을 분리해 설명한다. Unity 화면이나 애니메이션을 실제 업무 완료 근거로 표현하지 않는다.

## 구현 단위

- 원장 또는 업무 node 하나를 골라 저장, 상태 전이, Event, API, UI, test까지 세로로 완성한다.
- 기존 naming, DI, repository 경계를 먼저 따르고 abstraction은 실제 중복이나 책임 혼합을 줄일 때만 추가한다.
- 영속 workflow는 contract와 API 경계를 먼저 안정화하고 인증, 조회, 저장, 상태 전이 순으로 연결한다.
- 상태 전이 성공 뒤 client는 같은 원장을 다시 조회해 여러 앱이 같은 상태를 보게 한다.
- 외부 API는 interface, options, typed client, DTO, service 경계를 두고 timeout, cancellation, 오류 응답과 retry 가능성을 고려한다.
- 공공 데이터에는 출처, 기준 시각, 단위, 통화, 지역, 갱신 주기와 제한을 표시한다.

## 검증 단계

상세 output은 `artifacts/local/validation/`에 저장하고 대화에는 성공 여부, 실패 test 이름, 첫 오류만 남긴다.

| 시점 | 기본 명령 | 목적 |
| --- | --- | --- |
| 수정 직후 | `powershell -NoProfile -ExecutionPolicy Bypass -File eng/validate-changes.ps1 -Level Fast` | 직접 영향 project와 발견된 관련 test |
| 작업 완료 전 | 같은 명령의 `-Level Task` | 관련 version slice와 targeted/full fallback test |
| release 또는 명시 요청 | 같은 명령의 `-Level Release` | `0.0`, `0.5`, `1.0`, `1.5` build와 전체 test |
| 문서·지침만 변경 | 같은 script의 `Fast` | `git diff --check`, build/test 생략 |

- 작업 트리에 다른 스레드 변경이 섞여 있으면 `-Paths <이번 작업 파일들>`로 검증 범위를 명시한다.
- shared contract나 shared UI 변경은 소비 client 검증 없이 server build만으로 끝내지 않는다.
- 상태 전이·동기화는 원장 저장, RDB 투영, Event 재처리, 권한, 다른 client 재조회를 확인한다.
- UI는 가능한 경우 local server와 browser로 핵심 경로를 확인한다. 불가능하면 제한과 대체 검증을 적는다.
- test나 build를 실행하지 못하면 이유와 미검증 범위를 보고한다.

## Git과 산출물

- commit과 push는 사용자가 명시적으로 요청한 경우에만 수행한다.
- commit은 feature, refactor, fix, test, docs처럼 되돌릴 수 있는 맥락으로 나누고 다른 스레드 변경을 섞지 않는다.
- stage 전 이번 작업의 `git diff`와 `git status`를 다시 확인한다.
- 화면 변경은 [커밋별 시각 변경 기록](docs/Changes/README.md)에 실제 PNG와 함께 기록한다. Unity Scene·prefab·material·camera·UI처럼 Game View 결과가 달라지는 변경은 최종 상태를 Editor/Pipeline에서 다시 캡처하고, 대표 Game View PNG와 변경 기록을 해당 코드·Scene과 같은 맥락의 커밋에 포함한다. Scene View는 보조 증거일 뿐 Game View를 대신하지 않는다. 화면이 없으면 `화면 없음` 또는 `간접 확인`으로 남긴다.
- 임시 log, browser profile, raw capture, test result는 `artifacts/local/`에 두고 commit하지 않는다.
- 새 worktree는 가능하면 저장소 바깥의 sibling 경로에 만든다. `artifacts/worktrees/`는 탐색·검증 대상에서 제외한다.
- 장기 보존할 대표 화면만 `docs/assets/changes/`로 옮긴다.

## 우선 참고 문서

1. [README](README.md)
2. [0.0 커뮤니티·공공데이터 집중 로드맵](docs/Versions/v0.0/focus-roadmap.md)
3. [커뮤니티 0.0 기반 제품 원칙](docs/Architecture/CommunityFoundationV0Policy.md)
4. [업무 실행 책임 모델](docs/Architecture/BusinessWorkflowResponsibilityModel.md)
5. [첨부 문서 목차](docs/ProjectOverview/00-첨부문서목차.md)
