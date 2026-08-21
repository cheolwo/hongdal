# Ssalddel Current Work

> GPT Chat과 Codex가 다음 작업을 이어받기 위한 최신 상태판이다. 완료 이력을 누적하지 않으며 장기 결정은 [DECISIONS.md](DECISIONS.md), 세부 설계와 과거 근거는 연결된 Architecture 문서를 따른다.

## 현재 목표

- 서버 권위의 `Preview → Confirm → WorldTick → 재조회`를 기준으로 `Nature↔Farm 수확과 회복의 하루 → Farm→Hub 공급선 → Hub→Town 시장·수령`을 실제 플레이 폐루프로 완성한다.
- 공식 Unity 진입점은 `SimulationWorldShell` 하나로 유지한다. 검토·실험 Scene과 Fixture는 실제 서버 플레이 및 완료 증거와 분리한다.
- 운영 서버, Simulation 상태, Unity 표현의 책임을 분리한다. 운영 자료는 승인·동기화·세션 동결을 거쳐 정보와 제안으로만 사용하며 자동 생산량·수익·사건 규칙을 만들지 않는다.
- 공간은 `H1 작업공간 → H2 블록 → H3 경관 → H4 AreaSet → H5 세계 배치`와 `E0~E8 증거 단계`를 분리해 판정한다. 작성 Scenario E5, 선택형 현실 정합 E6, 실제 플레이 E7을 서로 대신하지 않는다.
- 새 기능 수를 늘리기보다 현재 세로 조각의 서버 책임 경계, Save/Replay, Unity 서버 재조회와 실제 실행 증거를 먼저 닫는다.

## 현재 구현 기준선

- 게임플레이 계획 V7의 감자 수확 Lot 결속 판로 선택, 작은 Farm 창고 건설과 보관 용량 활성화, 플레이어별 심리·시대, Farm→Hub 접근, Hosted 참가자 권한, 협동 건설·보호·철거·복원과 Save/Replay가 구현돼 있다. 세부 단계는 [게임플레이 계획 V7 현행화](../Architecture/게임플레이계획V7현행화.md)를 따른다.
- 서버는 세션 생성·조회·Tick·저장·복원·재생을 생명주기 서비스가, 심리·AreaSet 이동·Hosted·협동·관측을 세계 게임플레이 서비스가 소유한다. 기존 HTTP route와 `경영SimulationSessionService` 공개 표면은 호환 facade로 유지한다.
- Farm 감자 현실자료는 농사로 작업·재해예방, KAMIS 국내 가격, USDA AMS 후보 문맥을 운영 승인 자료에서 Simulation 파생 원장으로 명시적으로 동기화하고 세션 시작 시 불변 상태 사본으로 동결한다. Unity에는 근거·관계 상태와 검토 제안만 제공한다.
- 평창 H5 작성 세계는 Nature·Farm·City/Hub·Town AreaSet 네 개와 물리 회랑 세 개를 `ScenarioLocalMeters`에 결속한다. 이는 작성 Scenario E5 근거이며 현실 지형이나 실제 플레이 완료를 의미하지 않는다.
- 실제 공간 산출물로 부분 조립된 곳은 대관령 Farm 중심 타일 `kr5186:l2:700:1145`이다. 나머지 Farm 타일과 Hub·Town Graph의 실제 Tile 범위·양쪽 연결점은 준비되지 않았으므로 완료 상태로 올리지 않는다.
- Unity 공식 조립은 서버 권위를 기본값으로 사용한다. 정착지 선택·턴 마감·물류 이동은 권위가 없을 때 Fixture를 암묵적으로 만들지 않으며 Fixture는 시험·검토 조립에서만 명시적으로 주입한다. 물류 이동은 서버 세션에서 최초 상태를 조회하되, 서버 소유 Preview 문맥이 아직 없으므로 Unity Fixture 요청으로 우회하지 않고 명시적인 사용 불가 상태로 멈춘다.
- Unity Editor의 `Ssalddel` 제작 메뉴는 `통합 월드`, `검토실`, `공간·경관`, `검증`, `실험·참고 Scene` 다섯 묶음만 노출한다. 실험·Sample Builder 구현은 참고 자산으로 보존하되 공식 메뉴에서는 숨기고, 참조가 없던 `WORLD-0` 카메라 Prototype Builder만 제거했다.
- 공식 `SimulationWorldShell`의 Play Mode 기본 화면은 Player 1명과 하단 `전략 시점 / 1인칭 / 3인칭` 전환만 남긴다. 상태·진단·공간 결속·작업 안내와 월드·fixture 표현은 내부 기능과 오브젝트를 보존한 채 숨긴다. 기본 시점은 Player가 보이는 3인칭이며, Play 후 늦게 생성되는 표현도 같은 정책으로 정리한다. 검토 Scene의 안내 표현은 별도 설정으로 다시 켤 수 있다.
- H·WI·AreaSet의 상세 수량과 생성 규칙은 이 문서에 복제하지 않는다. 현재 기준은 [게임 플레이 단위 주도 WI–H 공동 진척 계획](../Architecture/게임플레이단위주도-WI-H공동진척계획.md), [H1~H5 공간 포함 계층 조사](../Architecture/H1-H5공간포함계층조사.md), [AreaSet 구성 패턴](../Architecture/AreaSet구성패턴.md)을 따른다.

## 최신 검증

- 2026-08-21 Task 검증에서 코드 지도 검사, `Ssalddel.Simulation.slnx` 빌드와 Simulation 전체 시험 `708/708`이 통과했다. 결과는 `artifacts/local/validation/20260821-185211`에 있다.
- 세계 게임플레이 책임 분리와 HTTP route 집중 시험 `12/12`가 통과했다.
- Unity EditMode 어셈블리는 현재 소스 기준 오류 없이 컴파일됐다. 실제 Editor에서 `Ssalddel` 최상위 메뉴가 다섯 묶음만 표시됨을 확인했고, 정적 감사 결과는 등록 126개, 실험·Sample 노출 0개, 중복 0개다.
- 최소 Player 화면은 Unity C# 빌드 오류 0개, 열린 Scene `WORLD-UI-MINIMAL-1` 구조 검증 통과, 실제 Play Mode·Game View 확인까지 완료했다. Player 1명과 하단 시점 전환 세 개만 남고, Play 후 늦게 생성되던 창고·표식·fixture 표현도 숨겨지는 것을 확인했다. 증거는 Unity `Documentation/Changes/2026-08-21-minimal-player-view/minimal-player-game-view.png`이다.
- 실제 Editor의 최근 전체 EditMode 실행은 `329/337` 통과했다. 당시 물류 이동 집중 시험 `8/8`은 통과했고, 실패 8개는 물류 이동 밖의 저장 Scene·공간 카탈로그·턴 마감 항목이다. 이후 서버 모드의 Unity Fixture Preview 차단 시험을 추가했으며 실제 Editor 컴파일은 통과했지만 시험은 다시 실행하지 않았다. 물류 이동 전용 Builder가 공식 `SimulationWorldShell`에 서버 기준 조립 뿌리를 저장했고 열린 Scene 구조 검증도 통과했다.
- 최소 HUD 화면만 Play Mode·Game View에서 확인했다. 전체 플레이 폐루프와 실제 서버 연결은 아직 재검증하지 않았으며, 화면 정리를 서버 플레이 증거로 확대 해석하지 않는다.
- 실제 Provider 호출, 운영 DB 동기화·migration 적용, 운영 배포, commit과 push는 수행하지 않았다.

## 다음 작업 순서

1. 배차 후보와 실제 수확 화물 문맥을 서버 소유 조회 계약으로 만들고, Unity는 고유 식별자와 예상 revision만 보내도록 물류 Preview·Confirm 경계를 닫는다.
2. 전체 EditMode 실패 8개의 현재 원인을 병행 변경과 분리해 정리하고, 정착지·턴 마감까지 서버 기준 저장 Scene 검증을 맞춘다.
3. 실제 Simulation 서버와 저장 DB를 연결해 감자 수확→판로 선택→작은 창고→Farm→Hub 이동을 HTTP·Save/Replay·Play Mode·Game View로 완주한다.
4. Nature↔Farm의 정상 귀환, 후퇴 회복, 원인 해결 뒤 복원 회복을 같은 서버 상태와 저장·재생으로 검증한다.
5. 위 폐루프가 닫힌 뒤 Hub 출고와 Hub→Town 시장·수령으로 확장한다.
6. 기능 경계가 안정된 뒤에만 Session Aggregate 부분 클래스와 호환 facade의 추가 분해를 검토한다.

## 남은 위험과 제한

- Hongdal과 Unity 작업 트리에는 여러 병행 변경이 있다. 관련 없는 변경을 정리하거나 되돌리지 말고, 같은 파일을 수정하기 전에 현재 diff를 다시 확인한다.
- 현재 저장 Scene은 병행 변경 상태이므로 코드 컴파일이나 Builder 설정만으로 실제 Scene 배선 완료를 주장하지 않는다.
- 서버가 실행되지 않은 상태에서는 턴 마감·입고·농장 전투 등 서버 권위 UI가 정상 플레이를 제공하지 않는다. Fixture 성공을 서버 연결 성공으로 대체하지 않는다.
- 물류 이동 Presenter의 암묵 Fixture와 고정 화물 ID·표시 수량은 제거했다. 배차 후보·화물 문맥의 서버 소유 조회 계약은 아직 없으므로 서버 모드 Preview는 `SimulationLogisticsMovementPreviewContextMissing`으로 차단된다.
- 선택형 E6 현실 정합은 대관령 Farm 일부만 준비됐다. 자료가 없는 타일과 Graph는 `WaitingForSpatialArtifact` 또는 `Declared`로 유지한다.
- 운영 자료·API key·원문 전체·필지 식별자는 Simulation·Unity 계약에 노출하지 않는다. 운영 효과와 유상 물류·계약·결제는 별도 승인과 운영 게이트 없이는 활성화하지 않는다.
- 검토용 Azure 배포, 모바일 촬영 검토, 추가 자산 설치와 시각 마감은 현재 플레이 폐루프의 선행 조건이 아니다.
