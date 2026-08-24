# 기능 작업영역 분할 기준

## 목적

프로젝트 수와 기능이 늘어도 개발자와 Codex가 매 작업마다 전체 저장소를 읽지 않도록 제품 흐름 다섯 영역과 Simulation·Unity 공통 영역으로 나눈다. 이 분할은 먼저 **읽기·검색·검증·커밋 경계**로 적용하고, 프로젝트 간 의존성이 안정된 뒤에만 물리 assembly 또는 별도 repository 분리를 검토한다.

## 작업영역

| 작업영역 | 제품 단계 | 소유 의미 | 기본 solution |
| --- | --- | --- | --- |
| `community-foundation` | 0.0 | 게시글·댓글·첨부·프로필 공개범위·신고 보호·공동 원장 입구 | `Ssalddel.v0.0.slnx` |
| `regional-culture-public-data` | 0.0 | 지역문화·지역 key·생성 이미지·공식 식품/가격/통계 근거 | `Ssalddel.v0.0.slnx` |
| `individual-intent` | 0.5 | 비용 미리보기·수정/철회 가능한 개인 의향 | `Ssalddel.v0.5.slnx` |
| `group-purchase` | 1.0 | 별도 참여 동의·집계·공동구매 원장 | `Ssalddel.v1.0.slnx` |
| `trade-readiness` | 1.5 | 공급자·HS/HTS·검역·표시·포워더 인계 전 준비 | `Ssalddel.v1.5.slnx` |
| `simulation-unity` | 공통 | Simulation 세션·파생 World·Streaming과 Unity 데이터 코어 | `Ssalddel.Simulation.slnx`, `Ssalddel.Unity.slnx` |

각 작업영역의 기계 판독 범위는 `eng/work-areas/*.json`을 단일 기준으로 사용한다.

## 제품 기능 영역과 시스템 책임 흐름

위 표는 커뮤니티·공공데이터·개별 의향처럼 **어떤 제품 기능을 개발하는가**를 나눈다. 운영·Simulation·Unity 구분은 **어느 실행 책임이 상태를 소유하는가**를 나누는 별도 축이다. 작업을 시작할 때 제품 기능 영역 하나와 시스템 책임 흐름 하나를 함께 고른다.

| 책임 흐름 | 기본 branch prefix | 상태 소유 기준 |
| --- | --- | --- |
| `operations` | `operations/` | 실제 사용자·권한·동의·업무 원장 |
| `simulation` | `simulation/` | 게임 Session·가상 시간·WorldTick·Save/Replay |
| `unity` | `unity/` | 입력·공간·UI와 권위 상태 사본 표현 |
| `integration` | `integration/` | 공개 계약·Adapter·호환 검증이며 독립 상태를 소유하지 않음 |

예를 들어 Farm 현실자료 작업은 제품 기능상 `regional-culture-public-data`이면서 책임상 원자료 승인·운영 저장은 `operations`, 세션 동결 상태는 `simulation`, 계약·파생 경계는 `integration`, 표시만 바꾸는 작업은 `unity`다. 여러 책임을 통과하는 하나의 기능도 커밋과 검증을 책임별로 나눈다.

`codex/rename-ssalddel`의 기존 혼합 이력은 과거 통합 기준선으로 보존하고 새 일반 목적 작업을 계속 누적하지 않는다. 상세 기준과 기계 원장은 [운영·Simulation·Unity 작업 흐름 분리](OperationsSimulationUnity작업흐름분리.md), `eng/work-areas/responsibility-workstreams.json`을 따른다.

## Codex 읽기 규칙

1. 요청의 주 제품 기능 영역과 시스템 책임 흐름을 하나씩 고른다. 단일 책임 작업은 제품 기능 영역을 생략할 수 있지만 책임 흐름은 생략하지 않는다.
2. 해당 manifest의 `readFirst`만 먼저 읽는다.
3. 검색은 `sourceRoots` 안에서 시작하고 `excludedRoots`는 제외한다.
4. 다른 작업영역 contract가 필요한 경우 공개 route·DTO·metadata부터 읽고 내부 구현 전체로 바로 확장하지 않는다.
5. 둘 이상의 제품 기능 영역 또는 시스템 책임을 변경하면 커밋을 영역·책임별로 나누고, 공유 contract 변경은 생산자와 소비 영역 test를 별도로 실행한다.
6. 전체 repository 검색은 이름 충돌, DI 조립, migration snapshot, 공개 route 호환처럼 전역 확인이 필요한 경우에만 수행한다.
7. `simulation-unity`에서는 생성 코드 지도의 기능 트리를 먼저 읽고, `StepKey` 순서로 핵심 타입만 연 뒤 세부 구현으로 확장한다.

## 물리 분리 판단

다음 조건을 모두 만족할 때 assembly 분리를 시작한다.

- 한 작업영역의 public contract와 소유 DB/Event가 문서와 test로 고정되어 있다.
- 다른 영역은 해당 영역의 내부 Entity·UseCase를 직접 참조하지 않고 contract/API만 사용한다.
- 공통 UI가 영역별 component namespace와 DI module로 분리되어 있다.
- migration 소유 DbContext를 분리해도 한 상태 전이가 여러 DbContext transaction에 암묵적으로 기대지 않는다.
- 현재 version solution에서 독립 build/test가 통과한다.

처음 추출할 후보는 `Ssalddel.RegionalCulture.Contracts`, `Ssalddel.RegionalCulture.Application`, `Ssalddel.RegionalCulture.Ui`다. 지역문화는 주문 실행과 분리된 읽기 중심 0.0 기능이어서 경계를 검증하기 쉽다. 댓글과 공동 원장은 기존 영속 관계가 넓으므로 두 번째 이후에 분리한다.

## 금지하는 분할

- 폴더 수를 줄이기 위한 성급한 별도 repository 분리
- 같은 Entity를 여러 영역에서 각각 복제
- `Ssalddel.Ui.Common`의 단순 기술 widget까지 제품 영역별로 중복
- 0.0 화면에서 0.5 이후 서비스가 없으면 시작조차 못 하는 의존
- 기능영역 분리를 이유로 동의·개인정보·실행 경계를 합치는 것

## 현재 작업 적용

지역 허브와 상품 근거는 `regional-culture-public-data`, 댓글 국가 공개와 프로필 기본값은 `community-foundation`, 비용 미리보기는 `individual-intent`로 커밋한다. 공동구매·같이 수입 연결은 route 문맥만 앞 단계에 둘 수 있지만 상태 생성은 각각 `group-purchase`, `trade-readiness` 영역의 명시적 동의 뒤에서만 수행한다.
