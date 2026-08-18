# Ssalddel Current Work

> GPT Chat과 Codex가 다음 작업을 이어받기 위한 최신 상태판이다. 완료 이력을 계속 쌓지 않으며, 이전 작업군은 [2026-08-15 리팩토링 전 상태 요약](archive/current-work/2026-08-15-before-simulation-unity-refactor.md), 장기 결정은 [DECISIONS.md](DECISIONS.md)를 따른다.

## 현재 목표

- Nature 팩 중심 생활·탐험 공간을 주인공의 상시 체류 세계로 두고 Farm·Town·City/Hub 팩 중심 전문 경관 세 곳을 연결한다. 탐험·전투·경영의 기본 시점은 전술 3인칭으로 통일하고, Nature의 안전 생활핵·경계 완충대·조우 위험대를 분리한다. 실제 적 개체는 `POLYGON Apocalypse` 설치 전까지 표현하지 않으며 Generic 대체 자산을 사용하지 않는다.
- Farm·Town·City/Hub 사건의 잘못된 해결과 기한 초과를 경로별 Nature 위협으로 전파한다. 원인 WI 해결과 자연권 전투 승리만 압력을 줄이며, 일반 취소·미리보기 차단·시간 경과는 위협을 만들거나 자동 회복시키지 않는다.
- 공간과 Simulation을 `WI` 세계 상호작용 단위로 종단 연결한다. `E`는 증거 깊이, `H1 작업공간 모판 → H2 블록 모판 → H3 경관 모판 → H4 지역 모판`은 상향 조립 공간 자원 종류와 포함 깊이로 분리한다. 기본 구현 완료는 E3이며, E4 H1 모판 실행 검증 → E5 H1~H4 실제 이동 경관 폐루프 → E6 WI 필수 공공데이터 계보 → E7 실제 플레이 폐루프로 승격한다. 개발 순서는 `P0 핵심 인과선 → P1 세계 공통 규칙 → P2 원장·문서 → P3 전체 회귀`로 나눈다.
- Simulation·Unity의 외부 계약을 보존하는 구조 리팩토링 위에서, 경관 산책 중심의 28일 농장 생활과 선택 가능한 계절 방어 수직 단위를 완성한다.
- 순서는 `검증 라우팅 → 서버 책임 경계 → Save/Replay → Unity 읽기 런타임 → 문서`다.
- 운영 서버 권위, Simulation 상태와 Unity 표현의 분리, `SimulationWorldShell` 단일 실행 Scene 원칙을 유지한다.

## 현재 구현 상태

- 지역 사건 세 종류(수확물 노출, 시장 재고 오염, Hub 화물 적체), 안전/불안전 선택, 후속 WI 관찰, 기한 초과, 세 자연권 경로의 압력·경고·조우를 Session 상태로 추가했다. 전용 세계 사건 Preview/Confirm API는 서버가 선택 규칙과 예상 심각도를 결정하며 멱등 명령과 예상 개정을 유지한다. 자연권 전투 승리는 해당 경로의 가장 오래된 원인을 한 단계만 줄이고 같은 결과를 재적용하지 않는다. `simulation-save.v4`가 선택·승리 계보를 재생하며 v1~v3 읽기 경계를 유지한다. 새 집중 시험 5/5, 관련 농장·물류·창고·전투·저장·세계 사건 회귀 58/58, HTTP 호환 경계 10/10이 통과했고 Task 범위 전체는 Simulation 627/627·Unity 486/486이다. 저장 Scene·실제 Apocalypse Prefab·Play Mode·Game View는 수행하지 않았다.
- `simulation-world-immersive-layout.v1` 기계 대장과 결정적 생성기를 추가했다. Nature 상시 생활권과 Farm·Town·City/Hub 전문 경관 3곳, 안전 생활핵·경계 완충대·조우 위험대, 세 연결구, H2·H3·WI 참조와 팩 비중을 검증한다. 공통 런타임은 Apocalypse 자산 준비 상태, Generic 대체 금지, 위험대·Simulation 전용 조우 의도 관문과 원자적 경관 전환을 제공한다. Unity에는 Nature를 유지하면서 전문 경관 하나만 활성화하고 플레이어 표현 위치만 옮기는 `몰입경관InstanceController`를 추가했다. 경관 대장 통합 검증 8/8, .NET 정책 시험 9/9, Unity EditMode 전환 시험 2/2와 재컴파일 오류 0을 확인했다. 저장 Scene 배선·Play Mode·Game View와 실제 Apocalypse 표현은 수행하지 않았다.
- `OPEN-WORLD-1`로 기존 500m L2 자료 Streaming 위에 125m L3 `LH 엔진`을 연결했다. 현재 `SimulationWorldShell`의 기본은 서버 접속이 필요 없는 싱글 플레이 로컬 생성이다. `로컬공간LHWorldEngine`이 플레이어 위치·로컬 월드 시드·생성기 버전·로컬 날짜로 승인 H4 안의 상세 3×3·이동 준비 5×5·선행 자료 9×9를 계산하고, H1~H4 계보, 수확 기준작 네 고정 거점, 셀당 3~5개 주변 경관, 인접 셀 공유 경계, 28일 사계절 표현 해시를 반환한다. Unity는 경계 25% 전에 다음 창을 요청하고 최신 epoch만 조립하며 충돌·연결 준비가 끝난 셀만 이동에 연다. 서버 Profile·Preview는 멀티플레이·원격 검증·저장 동기화용 선택 경로로 보존한다. Save/Replay v3는 기본 월드 대신 seed·generator·H4 경계와 Delta만 봉인한다. 실제 도로·경계 근거가 없는 H2는 계속 `IdeaInventory` 후보이고 로컬 절차 생성은 H2 정의나 E5 증거가 아니다.
- 음식·화물 공통 `Npc물류RouteFollower`와 주문 결속형 경로 체크포인트를 추가했다. `법정동화물운송View`는 저장 Scene GUID를 보존하는 화물 호환 구성 요소이며, 배차 Confirm 뒤 Farm 포장장→농촌 회랑→Hub 입구→하역장 네 지점을 자동 이동한다. 각 체크포인트는 경로·Cargo/주문·NPC·차량·순번·예상 개정이 일치할 때만 기존 물류 권위를 한 단계 진행하고 중복·역순·다른 차량 입력을 거부한다. LH 엔진은 플레이어를 주 관심점으로 유지하면서 NPC 다음 경로 셀을 보조 관심점으로 선행 준비하며 `NpcNavigation`·충돌·연결이 준비되지 않으면 NPC를 정지시킨다. 화물 배차 정책과 계약은 `UnityPackage` 로컬 UPM 원본을 통해 .NET과 Unity가 같은 `화물배차후보선정Policy`를 사용한다. Unity EditMode 물류 7/7·LH 11/11, 저장 Scene 화물 NPC 자동 이동 PlayMode 1/1, .NET 공유 정책 3/3과 Console 오류 0을 확인했다.
- 첫 실제 기준작 `수확과 출하의 날`을 추가했다. 서버의 ‘오늘 작업 계획’은 여러 작업 초안을 한 번에 Preview하고 모든 항목을 선검증한 뒤 한 개정으로 원자 Confirm하며, 차단 시 예약·작업·개정을 전혀 바꾸지 않고 Command를 Save/Replay한다. Unity `SimulationWorldShell`은 직접/NPC 선택, 계획 Preview/Confirm, 현장 Tick, 300kg 수확 Lot, 집하, 포장 Lot 출하 준비를 같은 서버 canonical 상태 흐름으로 표시한다. 실제 Simulation 서버와 저장 Scene의 Play Mode에서 NPC 위임 → 300kg Preview → 수확 → 집하 → 포장 → 미완료 업무 0건 턴 마감 → 다음 날을 확인했고, 포장 완료 때 턴 마감 Context를 최신 개정으로 다시 읽도록 연결했다. Windows 물리 키 전송은 새 Input System에 도달하지 않아 키 동작은 Play Mode `Keyboard.current` 상태 주입으로 검증했으므로 사람 손 조작 증거와는 구분한다. H2는 도로·경계 원본이 없어 `WaitingForRoadBoundaryEvidence` 후보만 추가했으며 실제 H2·E5로 승격하지 않았다.
- 기계 원본 `eng/execution-ledgers/world-interactions.json`에 농장·물류·Hub·마트·주문·세계 공통 37개 행위를 등록하고 한국어 [세계 상호작용 단위 대장](generated/world-interaction-catalog.md)을 결정적으로 생성한다. 26개 명령, 10개 부모 작업 자동 전이, 1개 공유 판정 정책을 구분한다.
- 공간 포함 계층은 별도 기계 대장 `simulation-world-spatial-hierarchy.r1`로 관리한다. 현재 정의는 H1 작업공간 모판 5개, H2 블록 모판 0개, H3 경관 모판 5개, H4 지역 모판 1개이며 H3·H4의 존재를 E5 완료로 오판하지 않도록 검증한다. 기준 경관 문법 156개와 Tile·Area·경관 완결 영역·ScenarioRoute는 H 계층 밖의 어휘·참조 축이다.
- 공통 `H 공간 구성 재고` 대장은 현재 상향식 설계 지식과 실제 H 정의를 입력으로 H1 설계 재고/정의 68/5, H2 19/0(범용 조립법 18개와 실제 지역 후보 1개), H3 10/5, H4 지역 청사진 후보/정의 5/1을 결정적으로 대조한다. H는 자원 종류, 후보·승인·배정·배치는 재고 상태, E는 증거 깊이로 분리하며 H4 후보를 평창 Farm–Hub–Town 실제 AreaSet 정의와 혼동하지 않는다.
- 기준 경관 문법에서 H1~H4를 상향 유도하는 설계 지식 저장소 v3를 구현했다. `catalog.v1`·`v2`와 기존 StableId를 보존하면서 Nature 12·Farm 8·Town 6·City 6의 팩 단독 표현 H1 32장을 행동 공간 H1 36장과 분리하고, Network 12종은 H2 18개 조립법에, Transition 8종은 H3 10개 청사진에 연결했다. 위치 독립 H4 지역 청사진 5개를 추가하고 52개 의미군·156개 변형 전체 계보, 파일별 SHA-256, 금지 권위 필드를 검증한다. 조회 도구는 WI·능력·팩·문법·위상에서 행동 H1→표현 H1→H2→H3→H4 후보를 양방향 제안하지만 공식 H 정의나 E4·E5 증거를 만들지 않는다. v1~v3 호환 재고·H 계층·실행 원장을 포함한 WI 공간 통합 검증 6/6이 통과했으며 새 지식의 Unity Prefab 조립·Scene·Play Mode는 수행하지 않았다.
- WI별 E/H 성립 상태를 별도 정책과 결정적 생성 결과로 대조한다. 현재 37개는 모두 E3이고, 13개는 승인 H1에서 E4 실행 성립, 17개는 H1~H4 설계 후보 계보만 존재, 1개(`WI-ORDER-04`)는 필수 공간 설계 누락, 6개는 공간 비적용이다. `WI-WORLD-04`의 Graph binding과 공식 H1 불일치, H2 정의가 0인데 남은 E5 배치 참조를 경고로 차단한다. P1 기준 플레이는 수확→집하→포장→상차를 생산구획·집하·포장·상차 공간 및 연결구로 구성하고, P2는 Hub 하차→인수→검수→적재를 하차·검수·보관 공간으로 연결한다. 두 계획 모두 위치 독립 후보이며 실제 도로·Block 경계와 Graph Node 전에는 H2·E5로 승격하지 않는다.
- 37개 항목의 기본 구현 상태는 계속 E3다. P0 집중 시험 22건과 P1 세계 공통 집중 시험 57건을 유지하며, H1 모판 E4 시험 6건을 더한 Simulation 전체 회귀 611건이 통과했다.
- 첫 13개 WI를 농장 생산, 농장 작업마당, 농장 상차·Gate, Farm–Hub 화물회랑, Hub 입고·보관의 다섯 H1 WI 공간 모판으로 묶었다. 실행 권위 JSON과 사람이 읽는 Markdown을 분리하고 Compiler가 WI E3 상태·능력 충족·내부 관계·연결구 쌍·기준 경관 문법 후보·금지 위치/Unity 자산 필드와 정의·문서·대장 SHA-256을 검증한다.
- H1 WI 공간 모판은 AreaSet·경관 그래프·Tile·절대좌표·실제 도로·Prefab·GUID·Material·Scene 경로를 갖지 않는다. 별도 `LandscapePattern` 계약을 만들지 않고 기존 156개 기준 경관 문법의 `compositionKey`를 허용 후보로 재사용한다.
- 다섯 H1 모판에서 기존 Simulation 공간 Provider가 소비하는 9개 `Scenario` 공간 정의를 결정적으로 만들고, 13개 WI의 Preview·Confirm·Task·Tick·Effect와 300kg 공급선 Save/Replay를 다시 통과했다. 이 시험 통과가 E4 증거이며 실제 평창 공간이나 공공데이터 근거가 아니다.
- 대관령 Farm의 기존 Graph Node 5·Edge 3·외부 연결점 0·미해결 3과 `spatial-capabilities.v1.json` 폐루프 3/9는 E4 완료 증거가 아니라 E5 배치 후보 근거로 재분류했다. Graph 해시 불일치 또는 미해결 공간을 Scenario로 자동 대체하지 않는 경계는 유지한다.
- `WI-FARM-01~06`, `WI-LOG-01~05`, `WI-001~002` 공급선은 재배 면적 100m²와 Fixture 생산 규칙에서 300kg 수확 Lot·포장 Lot·Cargo·출하 배분을 만들고, 상차 공간·Farm–Hub 회랑·하차 공간을 역할별로 판정해 진부 Hub 검수와 창고 사용 중 용량 300kg까지 같은 계보로 연결한다.
- 화물 이동은 출발지 상차·운송 회랑·목적지 하차의 세 공간 역할을 가진다. 출발 Tick에 상차 작업 영역 예약을 반환하고 도착 Tick에 하차 작업 영역 예약을 반환한다. 클라이언트는 역할별 선택적 선호 공간만 제출하며, 능력·용량·기간은 서버 규칙이 정하고 부적합한 선호 공간을 자동 대체하지 않는다.
- `WI-001` 입고 검수는 화물·작업자 접근과 검수 작업 영역, `WI-002` 적재는 보관·화물·작업자 접근과 적재 작업 영역을 요구한다.
- Session 상태 사본은 공간 정의·현재 점유·예약을 포함한다. 미리보기는 상태를 바꾸지 않고 확정은 기존 자원과 공간을 함께 예약하며, 완료 시 작업 영역을 해제하고 보관 예약을 실제 사용 중 용량으로 전환한다. 예정·차단 작업 취소는 같은 작업 계보의 공간 예약·NPC 배정·임시 검수 재고만 되돌린다.
- 13개 첫 공급선 WI의 통합 증거는 E4이며, E5 실제 배치 전 시험 인스턴스의 근거 종류는 계속 `Scenario`다. 공간 상태·역할별 예약·취소 명령이 있는 Session은 `simulation-save.v2`로 저장하고 역할 공간이 없는 기존 요청과 v1 재생 해시는 유지한다.
- 작업 취소 HTTP 경로와 Unity 입고 정보판 연결을 추가했다. Unity는 미리보기에서 선택된 공간과 “시나리오 공간 근거”를 표시하고 공간 정의·능력·용량·접근·예약 충돌을 한국어로 보여주지만 완료를 확정하지 않는다.
- 2026-08-17 E4 집중 시험 16건, Simulation 전체 시험 611건과 `Ssalddel.Simulation.slnx` 빌드가 경고·오류 없이 통과했다. 세계 상호작용·실행 원장 검증과 결정적 문서 재생성도 통과했다. 이어 Unity에는 원본 E4 JSON을 SHA-256 영수증과 함께 읽는 `WI공간모판VisualCatalog`와 전용 `WI공간모판검토실` Scene을 추가했다. 5개 H1 모판·9개 내부 공간의 전체 흐름, 모판별 대표 상세와 27개 고유 경관 후보 비교를 제공하며 H2~H4 실제 공간 조립이나 E5 증거·운영 상태는 만들지 않는다. 파생 DB migration과 실제 Graph 조립은 여전히 수행하지 않았다.
- WI 공간 작업의 반복 속도를 위해 세 생성기는 동일 내용이면 출력 파일을 다시 쓰지 않는 공통 원자적 writer를 사용한다. 세계 상호작용·H 공간 계층·Simulation–Unity 실행 대장 시험은 `eng/tests/wi-spatial-validation.ps1` 한 진입점에서 같은 PowerShell process로 실행하며, 해당 범위만 읽는 `eng/work-areas/wi-spatial-evidence.json`을 추가했다.
- Unity H1 검토실 생성기는 원본 경로·해시, JSON mirror 동기화, Visual Catalog 조립, UI 조립 책임을 별도 파일로 분리했다. 중심 `WI공간모판검토실Builder.cs`는 911줄에서 387줄로 줄었고, `빠른 원본·Catalog 새로고침`은 입력이 같을 때 JSON·Catalog·Scene 수정 시각을 바꾸지 않는다. 저장 Scene 전체 생성 메뉴와 고유 식별자·해시·화면 계약은 유지한다.
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
- 미완료 작업은 `eng/execution-ledgers/evidence-stages.json`의 공통 E0~E7 정의와 `eng/execution-ledgers/simulation-unity.json`의 실행 상태로 관리하고 한국어 실행 트리를 자동 생성한다. 첫 E7 종단 대상은 대관령 중앙 L2 `kr5186:l2:700:1145`다.
- 중앙 L2와 Halo 60m에서 Copernicus DEM 63×63, WorldCover·배치 마스크 62×62 산출물을 결정적으로 만들었다. 로컬 파생 DB migration을 적용하고 평창 건축물 37,383건·대표군 15개·Unity 산출물 3건으로 재파생했으며 같은 입력 재실행은 새 행을 만들지 않았다.
- `world-stream`은 최신 완료 산출물의 원본 hash·좌표계·해상도·NoData·수직 기준·형식·표본 크기를 DB에서 읽는다. 로컬 HTTP 검증에서 세 Layer가 `Available`이었고 elevation 본문 SHA-256이 manifest와 일치했다.
- Unity 서버 repository는 바이너리 본문을 다시 hash 검증한다. Streaming Controller는 상세 범위의 검증된 DEM만 Halo를 제외한 51×51 표현용 Mesh로 만들며 높이 과장은 Renderer에만 적용하고 Collider는 만들지 않는다.
- 새 기본 규칙 `farm-survival.scenic-season.r1`은 1~23일에 위협을 만들지 않고 24일 예고, 27일 자동 방어·직접 전투 선택, 28일 자동 판정과 계절 보고를 기록한다. 기존 `spring-preparation.r1~r3`은 명시적 호환 규칙으로 유지한다.
- 계절 방어 선택은 기존 위협 응답 API를 재사용한다. 선택 가능 ID와 마감 Tick을 상태 사본·세계 사건·Save/Replay 해시에 보존하며, Unity 표현 의도는 직접 전투를 선택해 `AwaitingCombat`이 되기 전까지 적 개체와 전투 HUD를 숨긴다.
- 평창 경관 후단은 `Nature + Farm + Town + City` 네 팩 Profile로 개정했다. 서버 Job은 `legal-dong-scenic-catalog.v2`와 경로가 아닌 Nature 의미 키를 저장하고, Unity는 5개 영역 비중과 Nature 8개 경관 세트 × A/B/C 총 24개 wrapper Prefab을 독립 생성한다. `SimulationWorldShell`의 대관령 L2 산림 전이 구획에는 활엽·침엽·혼효·산 능선·숲 가장자리를 배치하고 물 마스크가 없는 개울 회랑은 제외했다. 사계절 색조·FX는 원본 Material을 변경하지 않으며 늪·무덤·폐허 등은 평상시 숨긴 서버 사건 키 전용 Overlay 5종으로 분리했다.
- 평창 정적 경관은 Scene 직접 배치 전에 사람이 읽는 Markdown 기획서, 기본 JSON과 사람 보정 JSON을 거친다. Farm·Town·City 보유 Prefab 1,535건은 기술 대장으로 전수 등록하되 배치 승격과 분리하고, Nature·Farm·Town·City 팩별 기준 문서와 개별·묶음 SHA-256을 기획서 승인 입력으로 봉인한다. 의미 구성 대장은 Farm 24개·Town 18개·City 18개의 60개 세트와 Nature 24개를 `CompositionKey`로 해석한다. 현재 기본 계획은 8개 말단 배치 대상에서 감자 작물을 12개 밭고랑 타일마다 두 개씩 중앙 정렬하며, 보정 계획은 플레이어 시작점 통행구를 위해 울타리 하나를 비활성화하고 풍차·숲 가장자리를 이격해 활성 배치 127건으로 만든다. 계획 검증은 실제 Prefab Renderer 경계를 측정해 작물이 개별 밭고랑 밖에 있거나 방향이 어긋나면 Scene 적용을 막는다. 맑은 늦은 오전 렌더링 Profile v2와 Nature 그림자 정책은 계획·검토·Staging 영수증의 해시 계약으로 전달된다. `WORLD-PLAN-3`은 현재 Scene에 빠진 8개 Anchor를 기존 타일·회랑·경관 루트에 멱등 복구하고, 검증된 8개 생성 Root를 적용한 뒤 이전 생성 방식의 중복 정적 경관만 정리한다. 창고 상호작용용 관측 Fixture와 서버·Simulation 객체는 이 정리 대상이 아니다.
- 정적 배치 위에 연속 공간 조립용 canonical 경관 문법 대장을 추가했다. Nature 36·Farm 24·Town 18·City/Hub 18·Network 36·Transition 24, 총 52개 의미군 × A/B/C = 156개다. 각 항목은 위상·사방 경계·연결 지점·반복·인접·확장·세계 좌표 seed와 렌더링 비용을 갖는다. 서버용 안전 Manifest에는 유료 Prefab 경로·GUID가 없고 Unity 대장과 SHA-256이 일치한다.
- Simulation 서버는 공간 Layer가 준비된 타일만 `LandscapeSkeleton → LandscapeGraph`로 결정 조립한다. 새 파생 DB 다섯 테이블은 Node·Edge·Composition 배치·미해결을 저장하고 `GET /api/simulation/v1/world-stream/tiles/{tileKey}/landscape-compositions`로 표현 전용 결과를 조회한다. 공식 도로가 없는 농로는 `Scenario` 근거와 인접 타일 연결 Stub을 유지한다.
- Unity `SimulationWorldShell`의 동적 타일 Controller에는 경관 문법 대장이 실제 배선됐다. 상세 3×3 범위에서 서버 Graph와 대장 revision/hash·참조를 검증하고 비활성 staging root를 완성한 뒤 `LandscapeCompositionRoot`를 원자적으로 교체한다. Micro detail은 각 wrapper의 결정적 생성기 interface로 seed와 개정을 전달하며 Unity가 서버 업무 상태를 변경하지 않는다.

## 검증 상태

- LH 서버 집중 시험은 계약·결정성·인접 경계·H4 거부·28일 계절·Save/Replay·HTTP Revision 충돌과 기존 Streaming/저장을 합쳐 24/24 통과했다. 범위 지정 Fast 검증은 코드 지도 갱신 뒤 build·targeted test까지 통과했다. Task 검증은 build와 코드 지도는 통과했지만 전체 시험 621/622에서 기존 수정 중인 `SimulationFarmSurvivalController`가 Session 경로 두 개를 추가한 반면 호환 Manifest 기대값이 101인 상태라 `SimulationServerHttpBoundaryTests.세션_API_경로와_HTTP방식은_호환기준을_유지한다` 1건이 실패했다. LH Preview 경로는 `world-stream/lh` 아래라 이 Session 경로 집계에 포함되지 않는다. Unity 6000.5.6f1 재컴파일 오류는 0건이며 최신 epoch·2.125km 원점 이동 회귀를 포함한 LH EditMode 7/7, 기존 L2 Streaming 10/10, Nature 계절 8/8, 저장 Scene 자유 이동 PlayMode 2/2가 통과했다. Editor에서 `SimulationWorldShell`의 플레이어, 125m L3 Root, Synty 대장, 상태 UI, 서버 권위 Presenter와 `OfficialRegionProjectionRoot` 원점 이동 참조를 확인했다. 이번 변경의 별도 Game View 캡처와 실제 실행 서버 HTTP 왕복은 수행하지 않았다.
- WI E/H 상태 생성기는 P1·P2 공간 구성 계획을 함께 검증하며 반복 Write와 Check에서 동일 JSON·Markdown hash와 수정 시각을 유지한다. LH 공간 서비스는 내장된 E/H 상태와 P1 계획을 읽는 공급자를 통해 승인 H1·WI·표현 문법을 검증하고 H binding과 배치 후보를 결정적으로 만든다. P2는 실제 진부 Hub Node가 없으므로 계획 검증까지만 연결했다. WI 공간 통합 PowerShell 검증 7/7과 LH 공간 서비스·H1 모판 집중 .NET 시험 12/12가 통과했다. 이번 변경은 Unity Editor·Scene·Play Mode·Game View를 실행하지 않았다.
- E4 WI 공간 모판 Unity 검토실은 Unity 6000.5.6f1에서 생성·저장 Scene 재개방 검증을 통과했고 집중 EditMode 5/5가 통과했다. 실제 Play Mode에서 전체 개요 1장, 모판별 대표 5장, 후보 비교 5장 등 Game View 11장을 1600×900으로 확인했다. E/H 분리 뒤에는 전체 개요를 다시 캡처해 `증거 E4 · 공간 계층 H1`과 `실제 H2 Block이 아님` 경계를 확인했다. Console 오류는 0건이며 기존 nullable·폐기 예정 API 경고는 남아 있다. `SimulationWorldShell`, 기존 통합 모판 Scene, 서버 API·Domain 계약은 수정하지 않았다.
- 반복 작업 리팩터링 뒤 WI 공간 통합 PowerShell 검증 3/3은 내부 실행 약 0.95초에 통과했고 연속 생성에서 hash와 파일 수정 시각이 모두 유지됐다. Unity 재컴파일은 오류 없이 완료됐고 H1 검토실 EditMode는 빠른 no-op 새로고침 시험을 포함해 6/6 통과했다. 리팩터링 후 저장 Scene을 전체 재생성·재개방했으며 Play Mode·Game View는 화면 변경 작업이 아니므로 다시 실행하지 않았다.
- 공통 E0~E7 대장과 두 실행 원장의 E7 목표선 검증을 추가했다. 세계 상호작용 대장 37개와 Simulation–Unity 실행 대장 16개는 각각 두 번 생성해 동일 hash를 확인했고 최신 생성 문서 검사도 통과했다.
- 중앙 L2 산출물 생성 결정성 시험, 실행 대장 생성·검증 시험과 Simulation 공간 집중 시험 28/28이 통과했다. Simulation Server 빌드는 경고·오류 0이며 로컬 MySQL 증분 migration과 실제 평창 재파생·멱등 재실행을 확인했다.
- 실행 중인 로컬 `world-stream`에서 elevation·land-cover·placement-mask가 모두 `Available`이고 elevation 바이너리 SHA-256이 `B1A2FACB6D7E0E77493F8D2F23FBC452BDCBDDB907B0CCFAD5798EDAE263AC64`로 일치했다.
- 열린 Unity 6000.5.6f1 Editor 재컴파일은 오류 0, 공간 Streaming EditMode 시험은 10/10 통과했다. 실제 서버 산출물의 저장 Scene Play Mode·Game View 확인과 land-cover·placement-mask 기반 Synty 배치는 아직 미검증이다.

- 리팩토링 전 Simulation 기준선: 551/551 통과
- 검증 routing 자체 시험: 통과
- 새 Unity solution build: 경고 0, 오류 0
- API·턴·세션 집중 시험: 37/37 통과
- 공공데이터·커뮤니티·창고 Unity 로딩 집중 시험: 17/17 통과
- 최신 전체 Task 검증(`artifacts/local/validation/20260817-122625/`)에서 `Ssalddel.Simulation.slnx`, `Ssalddel.Unity.slnx`, `Ssalddel.v0.0.slnx` 빌드가 모두 통과했고 Simulation 605/605, Unity 480/480이 통과했다.
- 제품 전체 시험은 4578/4585가 통과했다. 실패 7건은 직전 기준 실행의 4577/4584와 이름이 모두 동일하며, 이번에 추가한 타입 전달 호환 시험 1건은 통과했다.
- 기존 코드 탐색 집중 시험은 Simulation 3/3, Unity 2/2, 운영 메타데이터 5/5가 통과한 상태를 유지한다.
- 농장 전투 집중 시험은 Unity 데이터 계약 16/16, Simulation 규칙·메타데이터 20/20이 통과했다. Unity 6000.5.6f1 실제 스크립트 재컴파일은 오류 0건이고, 저장 Scene 배선·Simulation 경로·개정 충돌을 다룬 EditMode 3/3이 통과했다.
- 경관 중심 계절 규칙 추가 뒤 Simulation 전체 563/563, Unity 데이터 전체 480/480이 통과했다. 새 집중 범위는 Simulation 농장 생활·전투 22/22, Unity 경관·위협 표현 4/4다.
- 지역 표현 요약 집중 시험 4/4의 기존 증거를 유지한다. 현재 코드 지도는 실제 메타데이터와 다시 일치하며 Fast·Task 검증에서 통과했다.
- 이번 기능 검증 산출물은 `artifacts/local/validation/20260815-210838/`에 있다. Simulation·Unity solution은 경고·오류 0으로 빌드됐고 관련 전체 시험은 통과했다. `Ssalddel.v0.0.slnx` 직접 빌드는 184초 제한에서 완료 여부를 확인하지 못했다.
- 평창 4팩 경관의 기존 검증은 Simulation 전체 568/568, 새 집중 범위 1/1, Unity EditMode 5/5가 통과했다. 이번 Nature 세분화 뒤 Unity 6000.5.6f1 스크립트 재컴파일 오류 0건, 생성기 오류 0건, Nature 집중 EditMode 7/7과 기존 4팩 Profile 3/3이 통과했다. 24개 기본 조합 Prefab·5개 사건 Overlay·사계절 제어기·저장 Scene 배선을 구조적으로 확인했다. 같은 생성기를 연속 실행했을 때 29개 Prefab과 2개 구성 대장의 SHA-256은 모두 같았고 저장 Scene은 의미 배선 시험은 같지만 Unity 이진 직렬화 해시는 달랐다. 전체 EditMode 일괄 실행은 Pipeline 연결의 30초 제한과 연결 재설정으로 결과를 수신하지 못했다.
- 네 팩 정적 경관 계획·검토 Pipeline은 Unity 6000.5.6f1에서 승인 입력 일치를 확인하고 `WORLD-PLAN-3`까지 실행했다. 현재 검토 상태는 `ApprovedForSceneApply`, `CanStage=true`, `CanApply=true`다. 보정 뒤 활성 배치 127건·Anchor 8/8·생성 Root 8/8·유효 배치 View 127/127을 저장 Scene에서 확인했고, 이전 정적 경관 중복 164개는 제거했으며 관측 창고 Fixture 하나는 보존했다. 감자 작물 24개는 개별 밭고랑 Renderer 안에 모두 포함되고 회전은 밭고랑과 같은 8도로 정렬됐다. 최종 성능 합계는 삼각형 212,151/222,412, Material Slot·Draw Call 378/408, Shadow Caster 238/260, Collider 194/213, Animator 5/7이다. 경고 6건은 성능 예산 80% 접근 5건과 `ScenarioPreview` 높이 근거 1건이다. 정적 경관 Pipeline 15/15와 창고 아이템 5/5가 통과했다.
- 저장된 `SimulationWorldShell`의 플레이어·카메라와 창고 Target·Controller 배선을 복구했다. Unity 6000.5.6f1 DX12 실제 Play Mode에서 1인칭과 전술 3인칭 버튼 전환, 농가·풍차·농지 경관, 감자 상자 Pallet 시선 감지를 확인했다. 통합 월드 3/3, 전투 입력 3/3, 턴 마감 5/5, 농장 경영 시점 3/3과 PlayMode `F2 → W 유지 → F3` 2/2가 통과했다. `UnifiedWorldModeWiringMissing`, 전투 실패 처리 `NullReferenceException`, 창고 `Scene 배선 오류`는 재발하지 않았다. 실행 중인 서버가 없어 턴 마감·입고 UI·농장 전투 세션 오류 3건은 남고, 로컬 오디오 출력 장치의 FMOD 전환 오류 1건을 별도로 관찰했다.
- `SimulationWorldShell`의 1인칭 WASD와 3인칭 전술 이동 범위를 대관령 Farm 전용 `X 10.5~31.5 / Z 2.5~22`에서 평창 통합 지도 `X -30.5~30.5 / Z -22.5~22.5`로 넓혔다. 연속 지형 Collider와 공간 Streaming 안전 Gate는 유지하며 8개 법정동 경계가 새 범위 안에 있음을 저장 Scene에서 검증했다. Unity 재컴파일 오류 0, Profile 1/1·통합 Scene 3/3·실제 키보드 `W` PlayMode 1/1이 통과했고, 별도 실제 Play Mode에서 `W` 2초 입력으로 `(15.400, 1.742, 10.250) → (15.400, 1.720, 12.105)` 이동과 `MovementBlockedByStreaming=false`를 확인했다. Game View 증거는 Unity 저장소의 `artifacts/local/validation/world-traversal-1/after-wasd.png`에 보존했다.
- `SimulationWorldShell`의 1인칭↔전술 3인칭 전환은 모드 설정 전에 활성 카메라의 시작 자세를 캡처하고 목표 자세도 전환 시작 시 고정한다. 두 방향은 같은 이차 곡선 제어점과 같은 시간 완화 함수를 역방향으로 사용하며 위치·회전·시야각을 함께 보간한다. Unity 재컴파일 오류 0, 농장 경영 시점 EditMode 4/4, 저장 Scene 왕복 곡선 PlayMode 1/1과 기존 `F2 → W → F3` 입력 PlayMode 1/1이 통과했다. 실제 Play Mode의 양방향 50% 지점은 위치 `(12.5955, 10.1389, 6.3899)`, 회전 `(26.9281, 15.9532, 355.4879)`, 시야각 `58`로 일치했고 `WorldTick`·개정 번호는 변하지 않았다.
- 경관 공간 문법 집중 검증은 Simulation 계약·결정성·반복 상한·자료 대기·파생 DB 재조회·Manifest 변조 차단과 기존 world-stream 경계를 합쳐 22/22 통과했다. `Ssalddel.Simulation.slnx` 빌드는 경고·오류 0이다. Unity 6000.5.6f1 재컴파일 오류 0, 안전 Manifest hash 일치·staging 원자 교체·hash 불일치 차단·서버 조회 경로 EditMode 4/4와 기존 공간 Streaming 10/10이 통과했고 저장 Scene의 `공간TileStreamingController.landscapeCompositionCatalog` 배선을 Editor에서 확인했다. 이번 변경의 Play Mode·Game View는 실행하지 않았다.
- 경관 문법 마이그레이션을 로컬 `ssalddel_simulation_world`에 적용했다. 최신 AreaSet Graph 조립 기준 중앙 `kr5186:l2:700:1145`은 Node 5·Edge 3·배치 5이고 Graph 전체의 외부 연결점은 0, Graph SHA-256은 `9c3b09c7fc59bd98a4a0102f6e08a1538e050aa2fbc3d2d0fc0becdb459ecc84`다. 동쪽 연결점은 자료가 없는 `701:1145`에 속해 생성되지 않았으며 문서 값으로 꾸며내지 않는다.
- `area-set:sim:pyeongchang:farm-hub-town.v1`을 JSON 실행 정의와 사람이 작성한 Markdown으로 분리하고 compiler가 참조·schema·금지된 Prefab/GUID 경로와 두 SHA-256을 검증하도록 했다. AreaSet은 대관령 Farm·두 회랑·진부 Hub·평창 Town의 다섯 `LandscapeGraph`와 네 Connector 관계를 묶으며, Graph는 Area·Tile을 N:N으로 참조한다.
- 파생 DB에 AreaSet 정의·참조·Graph 정의·공간 참조·Tile 참조·Graph 관계를 추가하고 로컬 migration을 적용했다. Graph 조립 Job, 연결 쌍 검증, 결정적 Graph hash, AreaSet·Graph index·Graph 조회 API와 기존 Tile API 호환 투영을 연결했다. 로컬 HTTP에서 AreaSet 1개·Graph 5개·관계 4개, 대관령 Farm `PartialUnresolved`·Tile 4개·배치 5개를 확인했다.
- Unity에는 AreaSet/Graph 응답 검증, 플레이어별 `Unloaded / Declared / Prepared / Active / Cached` 전이, 서버 저장소와 Graph 단위 비활성 staging·원자 교체 조립기를 추가했다. 이번 범위는 C# 컴파일까지만 검증했으며 새 EditMode 시험, 저장 Scene 배선, Play Mode와 Game View는 현재 요청에 따라 실행하지 않았다.

## 남은 문제

- Town·City/Hub 사건 발생 연결은 실제 시장 입고·화물 도착 작업 경로에 연결했지만, 이번 자동 종단 기준작은 Farm 수확물 노출 흐름이다. Town·Hub의 실제 WI 연속 실행과 병렬 Battle 서비스가 자연권 승리를 되돌려 쓰는 전체 HTTP 기준작은 후속 집중 시험이 필요하다.
- 몰입 경관 대장과 전환 Controller는 아직 `SimulationWorldShell` 저장 Scene에 연결되지 않았다. Apocalypse 자산 설치·검토 전에는 조우 위험대의 적 개체를 만들지 않으며, 설치 후에도 실제 Prefab 연결·NavMesh·전투 조우 표현은 별도 Scene 적용과 Play Mode·Game View 증거가 필요하다. 현재 Farm·Town·City/Hub 인스턴스는 H2·H3·WI 기반 설계 참조이지 실제 AreaSet·Graph·E5 공간 권위가 아니다.
- 저장 Scene에 실제 연결된 자동 운행 기준작은 현재 화물 1건이다. 음식배달은 같은 추종기·경로 계약의 `FoodDelivery` 종류와 검증까지 준비됐지만 음식점 픽업·고객 전달 배치 객체, 조리 시간창·품질 감소·묶음 배차 UI와 PlayMode 기준작은 후속이다. 이번 경로는 `ScenarioProcedural` 표현 자료이며 실제 도로 기반 H2·GraphRelation·E5 증거나 창고 입고 완료 권위를 만들지 않는다.
- 로컬 LH 엔진은 월드 시드·계절·H4 경계·기준 거점·연결을 직접 계산하지만 현재 저장 UI에서 새 시드 입력, 날짜 진행에 따른 로컬 달력 갱신, Delta 저장·불러오기, 생성기 버전 migration과 선택적 서버 충돌 해결 화면은 아직 연결하지 않았다.
- LH 공간 서비스의 대관령 Farm H1·WI 연결은 E/H 상태와 P1 구성 계획을 읽는 공급자로 전환했지만, 셀 내부 상대 배치와 연결구 생성 규칙 일부는 여전히 기준작 고정 로직이다. P2 진부 Hub 구성은 후보 계획과 검증까지만 있으며 실제 Hub Graph Node·H2 Block·LH 셀 조합에는 아직 연결되지 않았다. 후보 H2는 플레이어 이동 권위를 주지 않으며 승인 H1만 상호작용 입력으로 사용할 수 있다.
- `OPEN-WORLD-1`은 승인 H4 안의 125m L3 선행 생성, 셀 충돌·연결 준비, 캐시와 L3 단위 원점 재배치까지 닫았다. 실제 도로·경계 근거의 H2 Block, Farm→Hub 양쪽 GraphRelation 연결점, 실제 DEM 기반 L3 지형 Collider, NPC NavMesh 능력 준비와 H4 밖 다음 AreaSet 전환은 남아 있다. 따라서 절차형 주변 경관과 Fixture 이동 시험을 Farm→Hub 실제 보행이나 E5 증거로 해석하지 않는다.
- 대관령 Farm 2×2의 나머지 세 L2 공간 산출물과 작업마당·상차영역·Farm Gate, Farm–Hub·진부 Hub Graph가 없다. E5는 실제 도로 Network·Junction으로 H2 Block을 만든 뒤 승인된 H1 모판 인스턴스를 배치하고 양쪽 연결구를 H3 Node·Edge와 H4 GraphRelation에 결속해야 한다.
- `SimulationWorldShell.unity`의 표현 전용 플레이어 이동 범위는 평창 통합 지도 전체로 넓혔지만 이것은 E4 모판 승인이나 E5 실제 Graph 배치 증거를 승격하지 않는다. WI-FARM-01~03의 기존 Graph 근거는 계속 E5 배치 후보이며, 실제 지형 산출물이 없는 타일을 자료 완료로 간주하지 않는다.
- E5 경관에 WI별 필수 공공데이터 원본·파생·출처·해시 계보를 연결하는 E6과 실제 서버·Session DB·저장 Scene에서 사람이 조작하는 E7도 남아 있다.
- 실행 중인 실제 Simulation 서버·Session DB와 Unity 저장 Scene을 사용한 감자 생산→Hub 보관 HTTP·Play Mode·Game View 종단 검증은 남아 있다. 현재 첫 공급선의 완료 증거는 다섯 공간 모판에서 13개 E3 행위와 Save/Replay를 다시 통과한 E4까지다.
- 팀 관전의 기존 process-local `InMemory...Store` 공개 타입은 호환 표면으로 남아 있다. 실제 원장 연결이나 다음 호환 개정에서 Infrastructure 이전 여부를 결정해야 한다.
- 해시 계산기는 독립 파일이지만 기능별 정규화 항목이 많다. 다음 규칙 확장 시 기존 정규화 순서를 건드리지 않는 부분 클래스로만 추가 분리한다.
- 35개 부분 클래스의 Session Aggregate 심층 분해는 이번 안정화가 끝난 뒤 별도 작업으로 판단한다.
- 메타데이터 전수 적용은 하지 않았다. 현재 경고 후보는 후속 기능을 실제로 수정할 때 기능 단위로 편입하며, 단순 참조만으로 소유권 특성을 붙이지 않는다.
- 전투 입력을 위한 `SimulationWorldShell` 저장 배선과 경관 Game View 검증은 완료했지만, 실행 중인 서버와의 실제 HTTP 왕복과 운영 DB migration은 수행하지 않았다. Unity 변경은 맥락별 로컬 commit으로 분리했으며 push와 배포는 수행하지 않았다. Synty 원본 Prefab과 `.meta` GUID는 변경하지 않았다.
- 지역 표현 요약의 EF 모델·적재·조회 코드는 추가했지만 실제 MySQL migration 생성·적용과 기존 평창 원본을 사용한 재파생 실행은 이번 작업에서 수행하지 않았다. 좌표가 없는 건물은 타일 요약으로 승격하지 않으며 실제 Unity Game View 표현도 후속 검증 범위다.
- 평창 4팩 경관은 현재 `ScenarioTerrainPreview` 위의 표현 완결 단계다. 실제 DEM mesh·세분류 토지피복 위치 대체, 실제 수계 마스크가 있는 개울 회랑 배치, HLOD bake와 Draw Call·Shadow Caster 실측은 아직 수행하지 않았다.
- 저장 Scene의 계획 적용·플레이어·창고 배선과 Game View 확인은 완료했다. 남은 실제 실행 오류는 서버가 떠 있지 않아 발생하는 턴 마감·입고 UI·농장 전투 세션 연결 3건이며, live HTTP 왕복과 서버 기준 원장 재조회는 서버 실행 범위에서 별도로 검증해야 한다. FMOD 출력 장치 오류는 Unity 경관 로직과 분리해 로컬 오디오 장치 설정에서 확인해야 한다.
- 첫 2×2 L2 범위 중 실제 공간 산출물이 확인된 것은 중앙 `kr5186:l2:700:1145` 하나다. 새 경관 문법 MySQL 마이그레이션·네 타일 Job·중앙 타일 HTTP 조회는 완료했지만 나머지 세 타일은 실제로 `WaitingForSpatialArtifact`다. 세 타일의 DEM·토지피복·배치 마스크 확보와 실제 서버 연결 Play Mode·Game View 검증이 남았다.
- 다섯 AreaSet Graph 중 실제 공간 산출물로 부분 조립된 것은 대관령 Farm뿐이다. 나머지 네 Graph의 Tile 범위와 실제 양쪽 Connector가 준비되기 전에는 `Declared`를 완료 상태로 올리지 않는다. 새 Unity Graph 시험과 Scene 연결은 의도적으로 미실행이며, 다음 화면 검증 요청 전까지 코드 계약과 파생 DB 경계를 기준으로 유지한다.
