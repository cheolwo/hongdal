# Ssalddel Current Work

> GPT Chat과 Codex가 다음 작업을 이어받기 위한 최신 상태판이다. 완료 이력을 계속 쌓지 않으며, 이전 작업군은 [2026-08-15 리팩토링 전 상태 요약](archive/current-work/2026-08-15-before-simulation-unity-refactor.md), 장기 결정은 [DECISIONS.md](DECISIONS.md)를 따른다.

## 현재 목표

- Simulation·Unity의 외부 계약을 보존하는 구조 리팩토링 위에서, 경관 산책 중심의 28일 농장 생활과 선택 가능한 계절 방어 수직 단위를 완성한다.
- 순서는 `검증 라우팅 → 서버 책임 경계 → Save/Replay → Unity 읽기 런타임 → 문서`다.
- 운영 서버 권위, Simulation 상태와 Unity 표현의 분리, `SimulationWorldShell` 단일 실행 Scene 원칙을 유지한다.

## 현재 구현 상태

- `eng/validate-changes.ps1`는 제품·Simulation·Unity 변경을 서로 다른 작업 단위로 판정하며 혼합 변경에서는 필요한 solution과 test project를 모두 선택한다. `-PlanOnly`는 선택 결과를 JSON으로 반환하고 별도 routing 검증 스크립트가 네 가지 경로 조합을 확인한다.
- 기존 54개 endpoint가 집중됐던 `경영SimulationSessionsController`는 생명주기, World UI, 턴 결정, 수확·수출, 물류·창고, 주문·소비 Controller로 분리했다. 전체 세션 계열 96개 endpoint의 경로·HTTP 방식은 SHA-256 호환 시험으로 고정했다.
- Application은 공통 `경영SimulationSessionAccessor` 위에 생명주기·턴 결정·수확수출·물류창고·주문소비 Service를 두고, 기존 `경영SimulationSessionService`는 공개 호출 호환 Facade로 유지한다.
- API의 `SimulationContractException`, `SimulationNotFoundException`, `SimulationConflictException`은 공통 Filter에서 기존 400·404·409와 `SimulationErrorResponse`로 변환한다. 기존 Controller 직접 호출 시험을 위한 핵심 호환 메서드는 `NonAction`으로 보존한다.
- Save/Replay의 세션 저장 생성, 명령 재생, 해시 계산, 복제와 전투 부착·검증을 각각 `SimulationSaveReplay`, `SimulationSessionReplay`, `SimulationReplayHasher`, `SimulationSaveReplayCloner`, `SimulationBattleSaveReplay` 파일로 분리했다. 기존 `simulation-save.v1` JSON에 `Battles`가 없어도 빈 목록으로 읽고 기존 해시로 복원하는 특성 시험을 추가했다.
- 전투 인스턴스의 process-local 저장소 구현은 Infrastructure로 이동했고 Application에는 저장소 계약과 업무 Service만 남겼다.
- Unity 공공데이터·커뮤니티·창고 로딩 Coordinator는 공통 `LastSuccessfulLoadRuntime`을 사용한다. 최초 실패와 새로고침 실패를 구분하고 마지막 성공 상태를 유지하면서 기존 공개 상태 문자열을 보존한다.
- `Ssalddel.Unity.slnx`를 추가했으며 Unity 데이터 코어는 계속 `netstandard2.1`이고 `UnityEngine`에 의존하지 않는다.
- 코드 탐색 특성을 무의존 `Ssalddel.CodeMetadata`로 분리하고 기존 `Ssalddel.Contracts`에는 타입 전달을 남겼다. 이 어셈블리를 엔진 비의존 로컬 UPM 패키지로도 연결해 .NET과 Unity가 같은 메타데이터 원본을 읽는다. Simulation·Unity의 8개 기능·34개 핵심 단계를 JSON과 한국어 트리로 결정적으로 생성한다.
- `simulation-farm-combat-input`은 기존 전투 API 경로를 유지한다. Unity는 활성 박자 밖 좌클릭으로 서버 전투 진입을 요청하고, 활성 박자에서는 좌클릭 `Counter`·우클릭 `Guard`만 명령 초안으로 만든다. 카메라 전환 뒤 시점 확정과 박자 시작을 순서대로 전송하고, 같은 Command ID로 일시 오류를 한 번만 재시도하며 개정 충돌은 최신 상태 재조회로 처리한다.
- `SimulationWorldShell` 저장 Scene에는 `전투입력Adapter`와 `농장전투CompositionRoot`가 실제 배선되어 있다. 이 계층은 Simulation 전용 상태만 읽고 피해·등급·전술 효과를 로컬에서 계산하지 않는다.
- Simulation World 파생 원장 저장은 `region-presentation-summary.v1` Profile을 함께 적재한다. AreaSet·법정동·행정동·타일별 L0/L1/L2 요약은 분포 60%·지역 특색 25%·게임 맥락 15%, 분류별 최대 40% 규칙과 결정적 hash를 사용하며 표현하지 않은 원본도 분류 보고서에 남긴다.
- `world-stream`에는 지역·타일 요약과 공개 객체 상세 조회가 추가됐다. 일반 요약에는 상호명이 없고 검증된 건물–공개 사업장 관계의 상세 조회만 공개 상호명·출처·기준일을 반환한다. 대표자·연락처·사업자등록번호는 계약에 없다.
- 새 기본 규칙 `farm-survival.scenic-season.r1`은 1~23일에 위협을 만들지 않고 24일 예고, 27일 자동 방어·직접 전투 선택, 28일 자동 판정과 계절 보고를 기록한다. 기존 `spring-preparation.r1~r3`은 명시적 호환 규칙으로 유지한다.
- 계절 방어 선택은 기존 위협 응답 API를 재사용한다. 선택 가능 ID와 마감 Tick을 상태 사본·세계 사건·Save/Replay 해시에 보존하며, Unity 표현 의도는 직접 전투를 선택해 `AwaitingCombat`이 되기 전까지 적 개체와 전투 HUD를 숨긴다.
- 평창 경관 후단은 `Nature + Farm + Town + City` 네 팩 Profile로 개정했다. 서버 Job은 `legal-dong-scenic-catalog.v2`와 경로가 아닌 Nature 의미 키를 저장하고, Unity는 5개 영역 비중과 Nature 8개 경관 세트 × A/B/C 총 24개 wrapper Prefab을 독립 생성한다. `SimulationWorldShell`의 대관령 L2 산림 전이 구획에는 활엽·침엽·혼효·산 능선·숲 가장자리를 배치하고 물 마스크가 없는 개울 회랑은 제외했다. 사계절 색조·FX는 원본 Material을 변경하지 않으며 늪·무덤·폐허 등은 평상시 숨긴 서버 사건 키 전용 Overlay 5종으로 분리했다.

## 검증 상태

- 리팩토링 전 Simulation 기준선: 551/551 통과
- 검증 routing 자체 시험: 통과
- 새 Unity solution build: 경고 0, 오류 0
- API·턴·세션 집중 시험: 37/37 통과
- 공공데이터·커뮤니티·창고 Unity 로딩 집중 시험: 17/17 통과
- 최신 범위 지정 Task 검증에서 Simulation 558/558, Unity 478/478이 통과했고 `Ssalddel.Simulation.slnx`, `Ssalddel.Unity.slnx`, `Ssalddel.v0.0.slnx` 빌드도 모두 통과했다.
- 제품 전체 시험은 4578/4585가 통과했다. 실패 7건은 직전 기준 실행의 4577/4584와 이름이 모두 동일하며, 이번에 추가한 타입 전달 호환 시험 1건은 통과했다.
- 기존 코드 탐색 집중 시험은 Simulation 3/3, Unity 2/2, 운영 메타데이터 5/5가 통과한 상태를 유지한다.
- 농장 전투 집중 시험은 Unity 데이터 계약 16/16, Simulation 규칙·메타데이터 20/20이 통과했다. Unity 6000.5.6f1 실제 스크립트 재컴파일은 오류 0건이고, 저장 Scene 배선·Simulation 경로·개정 충돌을 다룬 EditMode 3/3이 통과했다.
- 경관 중심 계절 규칙 추가 뒤 Simulation 전체 563/563, Unity 데이터 전체 480/480이 통과했다. 새 집중 범위는 Simulation 농장 생활·전투 22/22, Unity 경관·위협 표현 4/4다.
- 지역 표현 요약 집중 시험 4/4와 Simulation 전체 시험 567/567이 통과했다. `Ssalddel.Simulation.slnx` 빌드는 경고·오류 0으로 통과했다. 범위 지정 Task 검증은 기존 미커밋 메타데이터와 `docs/AI/generated`의 불일치 때문에 코드 지도 검사에서 중단됐으며 이번 작업에서 다른 변경을 섞는 `--write`는 실행하지 않았다.
- 코드 지도 생성 원본에는 이번 작업 이전부터 변경이 남아 있고 `docs/AI/generated`와 일치하지 않는다. 이번 범위 지정 Task 검증은 코드 지도 검사에서 중단됐으며 다른 작업의 생성 결과를 섞지 않기 위해 `--write`를 실행하지 않았다.
- 이번 기능 검증 산출물은 `artifacts/local/validation/20260815-210838/`에 있다. Simulation·Unity solution은 경고·오류 0으로 빌드됐고 관련 전체 시험은 통과했다. `Ssalddel.v0.0.slnx` 직접 빌드는 184초 제한에서 완료 여부를 확인하지 못했다.
- 평창 4팩 경관의 기존 검증은 Simulation 전체 568/568, 새 집중 범위 1/1, Unity EditMode 5/5가 통과했다. 이번 Nature 세분화 뒤 Unity 6000.5.6f1 스크립트 재컴파일 오류 0건, 생성기 오류 0건, Nature 집중 EditMode 7/7과 기존 4팩 Profile 3/3이 통과했다. 24개 기본 조합 Prefab·5개 사건 Overlay·사계절 제어기·저장 Scene 배선을 구조적으로 확인했다. 같은 생성기를 연속 실행했을 때 29개 Prefab과 2개 구성 대장의 SHA-256은 모두 같았고 저장 Scene은 의미 배선 시험은 같지만 Unity 이진 직렬화 해시는 달랐다. 전체 EditMode 일괄 실행은 Pipeline 연결의 30초 제한과 연결 재설정으로 결과를 수신하지 못했다.

## 남은 문제

- 팀 관전의 기존 process-local `InMemory...Store` 공개 타입은 호환 표면으로 남아 있다. 실제 원장 연결이나 다음 호환 개정에서 Infrastructure 이전 여부를 결정해야 한다.
- 해시 계산기는 독립 파일이지만 기능별 정규화 항목이 많다. 다음 규칙 확장 시 기존 정규화 순서를 건드리지 않는 부분 클래스로만 추가 분리한다.
- 35개 부분 클래스의 Session Aggregate 심층 분해는 이번 안정화가 끝난 뒤 별도 작업으로 판단한다.
- 메타데이터 전수 적용은 하지 않았다. 현재 경고 후보는 후속 기능을 실제로 수정할 때 기능 단위로 편입하며, 단순 참조만으로 소유권 특성을 붙이지 않는다.
- 코드 지도 원본과 생성 자료의 불일치는 이번 경관 규칙 변경과 분리해 정리해야 한다. 현재 다른 작업의 미커밋 메타데이터·생성 파일이 포함되어 있어 자동 갱신하지 않았다.
- 전투 입력을 위한 `SimulationWorldShell` 저장 배선은 완료했지만 경관 산책·계절 방어 Game View 수동 조작, 실행 중인 서버와의 실제 HTTP 왕복, 운영 DB migration, commit, push와 배포는 수행하지 않았다. Synty 원본 Prefab과 `.meta` GUID는 변경하지 않았다.
- 지역 표현 요약의 EF 모델·적재·조회 코드는 추가했지만 실제 MySQL migration 생성·적용과 기존 평창 원본을 사용한 재파생 실행은 이번 작업에서 수행하지 않았다. 좌표가 없는 건물은 타일 요약으로 승격하지 않으며 실제 Unity Game View 표현도 후속 검증 범위다.
- 평창 4팩 경관은 현재 ScenarioTerrainPreview 위의 표현 완결 단계다. 실제 DEM mesh·세분류 토지피복 위치 대체, 실제 수계 마스크가 있는 개울 회랑 배치, HLOD bake와 Draw Call·Shadow Caster 실측, Play Mode·Game View 시각 검증은 수행하지 않았다.
