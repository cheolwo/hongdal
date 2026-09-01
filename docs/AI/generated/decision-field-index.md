# 결정 분야별 전수 색인

> 기준 원문: [DECISIONS.md](../DECISIONS.md). 이 문서는 생성 산출물이며 직접 수정하지 않는다.

## 읽는 법

- `D-###`는 전역 작성 이력 번호다.
- `D-{주분야}-{세부주제}-{순번}`은 분야별 결정 ID다. 마지막 숫자는 같은 주분야·세부주제 안의 누적 결정 수다.
- `TitleReviewed`는 제목과 주변 결정군을 전수 대조한 1차 분류이고, `BodyReviewed`는 본문까지 현재 기획에서 직접 재검토한 분류다.
- 분야별 결정 수는 구현 완료나 E 성숙도가 아니라 기획 결정의 분류 개수다.

## 전수성 점검

- 결정 제목: **486개**
- 고유 전역 번호: **485개** / 최댓값 `D-489`
- 비어 있는 전역 번호: `D-422, D-423, D-424, D-425`
- 중복 전역 번호: `D-096`
- 주분야: **11개**, 세부주제: **63개**

## 주분야 요약

| 주분야 | 세부주제 수 | 결정 수 | 본문 재검토 | 제목 1차 분류 |
| --- | ---: | ---: | ---: | ---: |
| 아키텍처 (`ARCHITECTURE`) | 4 | 46 | 0 | 46 |
| 데이터·공공자료 (`DATA`) | 4 | 36 | 0 | 36 |
| 증거·성숙도 (`EVIDENCE`) | 5 | 51 | 0 | 51 |
| 게임플레이 (`GAMEPLAY`) | 19 | 159 | 5 | 154 |
| 상호작용·UI (`INTERACTION`) | 2 | 12 | 2 | 10 |
| 운영·개발 운영 (`OPERATIONS`) | 2 | 10 | 0 | 10 |
| 기획·업무 방법 (`PLANNING`) | 11 | 39 | 4 | 35 |
| 표현·시각 (`PRESENTATION`) | 5 | 24 | 3 | 21 |
| Simulation 규칙·권위 (`SIMULATION`) | 3 | 48 | 0 | 48 |
| 스토리 (`STORY`) | 3 | 12 | 2 | 10 |
| 월드·공간 (`WORLD`) | 5 | 49 | 0 | 49 |

## 세부주제 요약

| 주분야 | 세부주제 | 결정 수 | 본문 재검토 | 전역 범위 |
| --- | --- | ---: | ---: | --- |
| 아키텍처 | `OPERATIONS-SIMULATION` | 6 | 0 | `D-027` ~ `D-032` |
| 상호작용·UI | `QUEST` | 10 | 0 | `D-455` ~ `D-464` |
| 운영·개발 운영 | `GIT-COMMIT` | 1 | 0 | `D-448` ~ `D-448` |
| 운영·개발 운영 | `OVERNIGHT-VISUAL-DEV` | 9 | 0 | `D-375` ~ `D-383` |
| 기획·업무 방법 | `CODE-NAMING` | 1 | 0 | `D-051` ~ `D-051` |
| 기획·업무 방법 | `DECISION-NAMING` | 1 | 1 | `D-486` ~ `D-486` |
| 기획·업무 방법 | `DECISION-WI-RELATION` | 1 | 1 | `D-487` ~ `D-487` |
| 기획·업무 방법 | `DISCOVERY-PLAN` | 4 | 0 | `D-465` ~ `D-468` |
| 기획·업무 방법 | `EVIDENCE-GOVERNANCE` | 2 | 0 | `D-472` ~ `D-473` |
| 기획·업무 방법 | `GOAL-INQUIRY-HANDOFF` | 18 | 0 | `D-269` ~ `D-286` |
| 기획·업무 방법 | `GRAPH-MAP-DEVELOPMENT-HANDOFF` | 1 | 1 | `D-489` ~ `D-489` |
| 기획·업무 방법 | `GRAPH-MAP-HANDOFF` | 1 | 1 | `D-488` ~ `D-488` |
| 기획·업무 방법 | `INQUIRY-SEARCH` | 1 | 0 | `D-325` ~ `D-325` |
| 기획·업무 방법 | `PLAYER-CENTERED-INQUIRY` | 8 | 0 | `D-391` ~ `D-398` |
| 상호작용·UI | `COMBAT-COMMAND` | 2 | 2 | `D-483` ~ `D-484` |
| 기획·업무 방법 | `PROJECT-IDENTITY` | 1 | 0 | `D-384` ~ `D-384` |
| 표현·시각 | `COMBAT-RISK` | 3 | 3 | `D-480` ~ `D-482` |
| 표현·시각 | `HERBAL-PROP` | 1 | 0 | `D-385` ~ `D-385` |
| 표현·시각 | `WORLD-CAMERA-STREAMING` | 13 | 0 | `D-096` ~ `D-125` |
| 표현·시각 | `WORLD-REGION-ASSET` | 3 | 0 | `D-033` ~ `D-035` |
| Simulation 규칙·권위 | `LOGISTICS-TRADE` | 20 | 0 | `D-052` ~ `D-071` |
| Simulation 규칙·권위 | `SETTLEMENT-ECONOMY` | 13 | 0 | `D-038` ~ `D-050` |
| Simulation 규칙·권위 | `WORLD-OBJECT-RULES` | 15 | 0 | `D-081` ~ `D-095` |
| 스토리 | `FIRST-DISCOVERY` | 8 | 0 | `D-408` ~ `D-415` |
| 스토리 | `MAIN-STORY` | 2 | 0 | `D-470` ~ `D-471` |
| 스토리 | `YODONG` | 2 | 2 | `D-474` ~ `D-475` |
| 월드·공간 | `GRAPH-MAP-E6` | 6 | 0 | `D-442` ~ `D-447` |
| 월드·공간 | `H-LH-ASSET-COMPOSITION` | 24 | 0 | `D-177` ~ `D-200` |
| 월드·공간 | `LANDSCAPE-PLACEMENT-LH` | 3 | 0 | `D-362` ~ `D-364` |
| 표현·시각 | `ANIMATION-WORKFLOW` | 4 | 0 | `D-358` ~ `D-361` |
| 월드·공간 | `SPATIAL-DATA-PRESENTATION` | 14 | 0 | `D-100` ~ `D-113` |
| 게임플레이 | `YODONG` | 4 | 4 | `D-476` ~ `D-479` |
| 게임플레이 | `TRADE-REALITY` | 8 | 0 | `D-367` ~ `D-374` |
| 아키텍처 | `PLAYABLE-DEVELOPMENT` | 17 | 0 | `D-214` ~ `D-230` |
| 아키텍처 | `REFACTOR-DATA-ASSET` | 6 | 0 | `D-139` ~ `D-144` |
| 아키텍처 | `UNITY-WORLD-FOUNDATION` | 17 | 0 | `D-001` ~ `D-017` |
| 데이터·공공자료 | `GAME-OBJECT-ASSET-DB` | 16 | 0 | `D-426` ~ `D-441` |
| 데이터·공공자료 | `MARKET-SUPPLY` | 9 | 0 | `D-018` ~ `D-026` |
| 데이터·공공자료 | `PRODUCT-ASSET-IDENTITY` | 2 | 0 | `D-036` ~ `D-037` |
| 데이터·공공자료 | `TIME-PUBLIC-DATA` | 9 | 0 | `D-072` ~ `D-080` |
| 증거·성숙도 | `E5-CONTEXT` | 1 | 0 | `D-469` ~ `D-469` |
| 증거·성숙도 | `PRESENTATION-E4-E5` | 5 | 0 | `D-386` ~ `D-390` |
| 증거·성숙도 | `PRESENTATION-INTEGRATION` | 19 | 0 | `D-250` ~ `D-268` |
| 증거·성숙도 | `SPATIAL-MATURITY` | 16 | 0 | `D-145` ~ `D-160` |
| 증거·성숙도 | `WORLD-PLAY-LOOPS` | 10 | 0 | `D-201` ~ `D-210` |
| 게임플레이 | `BATTLE-PREPARATION` | 1 | 1 | `D-485` ~ `D-485` |
| 게임플레이 | `WI-LOOP-PROGRESSION` | 19 | 0 | `D-231` ~ `D-249` |
| 게임플레이 | `CONSTRUCTION-CANCEL` | 5 | 0 | `D-320` ~ `D-324` |
| 게임플레이 | `FARM-DELEGATION` | 1 | 0 | `D-357` ~ `D-357` |
| 게임플레이 | `FOCUS-RESEARCH` | 2 | 0 | `D-365` ~ `D-366` |
| 게임플레이 | `HERBAL-CONTENT` | 11 | 0 | `D-346` ~ `D-356` |
| 게임플레이 | `HERBAL-TEA` | 8 | 0 | `D-326` ~ `D-333` |
| 게임플레이 | `HUB-REALITY-LOGISTICS` | 6 | 0 | `D-449` ~ `D-454` |
| 게임플레이 | `IDEA-NPC-INQUIRY` | 6 | 0 | `D-334` ~ `D-339` |
| 게임플레이 | `NATURE-THREAT-RECOVERY` | 16 | 0 | `D-161` ~ `D-176` |
| 게임플레이 | `PERSPECTIVE-FOCUS` | 6 | 0 | `D-416` ~ `D-421` |
| 게임플레이 | `PLAYER-RECOVERY-RESOURCES` | 33 | 0 | `D-287` ~ `D-319` |
| 게임플레이 | `SEASON-TECH-TREE` | 7 | 0 | `D-399` ~ `D-405` |
| 게임플레이 | `TAROT` | 4 | 0 | `D-096` ~ `D-099` |
| 게임플레이 | `TAROT-REALITY-SPATIAL` | 3 | 0 | `D-211` ~ `D-213` |
| 게임플레이 | `TEAM-COMBAT` | 13 | 0 | `D-126` ~ `D-138` |
| 게임플레이 | `CREDIT-MULTIPLAYER` | 6 | 0 | `D-340` ~ `D-345` |
| 월드·공간 | `WORLDMAP-PROPOSAL` | 2 | 0 | `D-406` ~ `D-407` |

## 전체 결정 대응표

| 전역 번호 | 분야별 ID | 결정 제목 | 검토 깊이 |
| --- | --- | --- | --- |
| [D-001](../DECISIONS.md#d-001-unity-개발-순서는-제품-버전에-종속하지-않는다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-001` | Unity 개발 순서는 제품 버전에 종속하지 않는다 | `TitleReviewed` |
| [D-002](../DECISIONS.md#d-002-unity는-전체-도메인을-world-관점에서-통합한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-002` | Unity는 전체 도메인을 World 관점에서 통합한다 | `TitleReviewed` |
| [D-003](../DECISIONS.md#d-003-운영-상태의-최종-권위는-서버다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-003` | 운영 상태의 최종 권위는 서버다 | `TitleReviewed` |
| [D-004](../DECISIONS.md#d-004-simulation과-operational-상태를-분리한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-004` | Simulation과 Operational 상태를 분리한다 | `TitleReviewed` |
| [D-005](../DECISIONS.md#d-005-sensor는-단일-관측-projection을-사용한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-005` | Sensor는 단일 관측 projection을 사용한다 | `TitleReviewed` |
| [D-006](../DECISIONS.md#d-006-git-저장소-문서를-ai-공용-기억으로-사용한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-006` | Git 저장소 문서를 AI 공용 기억으로 사용한다 | `TitleReviewed` |
| [D-007](../DECISIONS.md#d-007-외부-시각-asset은-view-wrapper-뒤에-둔다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-007` | 외부 시각 asset은 View wrapper 뒤에 둔다 | `TitleReviewed` |
| [D-008](../DECISIONS.md#d-008-dbset과-unity-controller를-11로-대응하지-않는다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-008` | DbSet과 Unity Controller를 1:1로 대응하지 않는다 | `TitleReviewed` |
| [D-009](../DECISIONS.md#d-009-첫-presentation-vertical-slice는-도심마트다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-009` | 첫 Presentation vertical slice는 도심마트다 | `TitleReviewed` |
| [D-010](../DECISIONS.md#d-010-차량-중심-차고가-아니라-도심-물류센터를-zone으로-사용한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-010` | 차량 중심 차고가 아니라 도심 물류센터를 Zone으로 사용한다 | `TitleReviewed` |
| [D-011](../DECISIONS.md#d-011-unity-presentation-composition-root는-vcontainer를-사용한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-011` | Unity Presentation composition root는 VContainer를 사용한다 | `TitleReviewed` |
| [D-012](../DECISIONS.md#d-012-world는-공유하고-role-perspective를-겹친다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-012` | World는 공유하고 Role Perspective를 겹친다 | `TitleReviewed` |
| [D-013](../DECISIONS.md#d-013-npc-이동은-업무-상태의-presentation이다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-013` | NPC 이동은 업무 상태의 Presentation이다 | `TitleReviewed` |
| [D-014](../DECISIONS.md#d-014-농장-운영-aggregate와-공개-작물-기준을-분리한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-014` | 농장 운영 aggregate와 공개 작물 기준을 분리한다 | `TitleReviewed` |
| [D-015](../DECISIONS.md#d-015-unity-심화-개발-단위는-zone-업무-흐름이다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-015` | Unity 심화 개발 단위는 Zone 업무 흐름이다 | `TitleReviewed` |
| [D-016](../DECISIONS.md#d-016-unity-읽기-흐름은-datainterpretationpresentation을-기본으로-한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-016` | Unity 읽기 흐름은 Data·Interpretation·Presentation을 기본으로 한다 | `TitleReviewed` |
| [D-017](../DECISIONS.md#d-017-worldstate와-identityruntime-경계를-분리한다) | `D-ARCHITECTURE-UNITY-WORLD-FOUNDATION-017` | WorldState와 identity·runtime 경계를 분리한다 | `TitleReviewed` |
| [D-018](../DECISIONS.md#d-018-비교-가격의-파생값은-단계간-가격차로-표현한다) | `D-DATA-MARKET-SUPPLY-001` | 비교 가격의 파생값은 단계간 가격차로 표현한다 | `TitleReviewed` |
| [D-019](../DECISIONS.md#d-019-interpretation은-shared-world와-perspective-단계로-나눈다) | `D-DATA-MARKET-SUPPLY-002` | Interpretation은 Shared World와 Perspective 단계로 나눈다 | `TitleReviewed` |
| [D-020](../DECISIONS.md#d-020-data-조회는-sessionworldauthorization-scope에-묶는다) | `D-DATA-MARKET-SUPPLY-003` | Data 조회는 Session·World·Authorization scope에 묶는다 | `TitleReviewed` |
| [D-021](../DECISIONS.md#d-021-외부공공-데이터는-서버-수집정규화-경계를-통과한다) | `D-DATA-MARKET-SUPPLY-004` | 외부·공공 데이터는 서버 수집·정규화 경계를 통과한다 | `TitleReviewed` |
| [D-022](../DECISIONS.md#d-022-외부-공급자-단계는-계약-조사와-실제-연결을-분리한다) | `D-DATA-MARKET-SUPPLY-005` | 외부 공급자 단계는 계약 조사와 실제 연결을 분리한다 | `TitleReviewed` |
| [D-023](../DECISIONS.md#d-023-첫-실제-농업-공급자는-world-bank-최신-경지면적-한-건으로-제한한다) | `D-DATA-MARKET-SUPPLY-006` | 첫 실제 농업 공급자는 World Bank 최신 경지면적 한 건으로 제한한다 | `TitleReviewed` |
| [D-024](../DECISIONS.md#d-024-도심마트-첫-운영-업무는-진열-보충으로-3계층-migration한다) | `D-DATA-MARKET-SUPPLY-007` | 도심마트 첫 운영 업무는 진열 보충으로 3계층 migration한다 | `TitleReviewed` |
| [D-025](../DECISIONS.md#d-025-도심마트-관리자-우선순위보다-재고-할당-무결성을-먼저-보강한다) | `D-DATA-MARKET-SUPPLY-008` | 도심마트 관리자 우선순위보다 재고 할당 무결성을 먼저 보강한다 | `TitleReviewed` |
| [D-026](../DECISIONS.md#d-026-um5-뒤-도심마트-공급-계약-경영-simulation을-우선한다) | `D-DATA-MARKET-SUPPLY-009` | UM5 뒤 도심마트 공급 계약 경영 Simulation을 우선한다 | `TitleReviewed` |
| [D-027](../DECISIONS.md#d-027-운영-서버와-게임-simulation-서버를-물리-분리한다) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-001` | 운영 서버와 게임 Simulation 서버를 물리 분리한다 | `TitleReviewed` |
| [D-028](../DECISIONS.md#d-028-공급계약-simulation-전에-지역-수요와-주문-객체를-명시한다) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-002` | 공급계약 Simulation 전에 지역 수요와 주문 객체를 명시한다 | `TitleReviewed` |
| [D-029](../DECISIONS.md#d-029-공동주택-주문자-집단은-기존-공동구매-원장과-개별-주문-집계를-재사용한다) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-003` | 공동주택 주문자 집단은 기존 공동구매 원장과 개별 주문 집계를 재사용한다 | `TitleReviewed` |
| [D-030](../DECISIONS.md#d-030-공동주택-대표의-사회적-context업무-권한npc-표현을-분리한다) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-004` | 공동주택 대표의 사회적 context·업무 권한·NPC 표현을 분리한다 | `TitleReviewed` |
| [D-031](../DECISIONS.md#d-031-unity-업무-학습은-공통-concept-card-presentation-문법으로-제공한다) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-005` | Unity 업무 학습은 공통 Concept Card Presentation 문법으로 제공한다 | `TitleReviewed` |
| [D-032](../DECISIONS.md#d-032-도심마트-관리자-30초-업무-queue와-우선순위-점수를-제거한다) | `D-ARCHITECTURE-OPERATIONS-SIMULATION-006` | 도심마트 관리자 30초 업무 Queue와 우선순위 점수를 제거한다 | `TitleReviewed` |
| [D-033](../DECISIONS.md#d-033-farmtowncity를-독립-presentation-region으로-구성하고-이동망으로-연결한다) | `D-PRESENTATION-WORLD-REGION-ASSET-001` | Farm·Town·City를 독립 Presentation Region으로 구성하고 이동망으로 연결한다 | `TitleReviewed` |
| [D-034](../DECISIONS.md#d-034-town과-city-사이에-다중-origin-지역-물류허브를-둔다) | `D-PRESENTATION-WORLD-REGION-ASSET-002` | Town과 City 사이에 다중 origin 지역 물류허브를 둔다 | `TitleReviewed` |
| [D-035](../DECISIONS.md#d-035-synty-animation은-실제-source를-검증하고-공용-presentation-adapter로-사용한다) | `D-PRESENTATION-WORLD-REGION-ASSET-003` | Synty animation은 실제 source를 검증하고 공용 Presentation adapter로 사용한다 | `TitleReviewed` |
| [D-036](../DECISIONS.md#d-036-공통-상품-stable-id와-출처별-품목코드를-분리한다) | `D-DATA-PRODUCT-ASSET-IDENTITY-001` | 공통 상품 stable ID와 출처별 품목코드를 분리한다 | `TitleReviewed` |
| [D-037](../DECISIONS.md#d-037-다품목-승격과-farm-asset-대응을-별도-검토-축으로-유지한다) | `D-DATA-PRODUCT-ASSET-IDENTITY-002` | 다품목 승격과 Farm asset 대응을 별도 검토 축으로 유지한다 | `TitleReviewed` |
| [D-038](../DECISIONS.md#d-038-정착지-경영분쟁-simulation은-공통-world와-경제-인과를-먼저-닫는다) | `D-SIMULATION-SETTLEMENT-ECONOMY-001` | 정착지 경영·분쟁 Simulation은 공통 World와 경제 인과를 먼저 닫는다 | `TitleReviewed` |
| [D-039](../DECISIONS.md#d-039-simulation-save는-versioned-package와-command-replay로-검증한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-002` | Simulation save는 versioned package와 Command replay로 검증한다 | `TitleReviewed` |
| [D-040](../DECISIONS.md#d-040-정착지-초기-경제는-scenario-입력과-독립-원장-지표로-구성한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-003` | 정착지 초기 경제는 scenario 입력과 독립 원장 지표로 구성한다 | `TitleReviewed` |
| [D-041](../DECISIONS.md#d-041-수확-판로-영향과-비축은-서버-계산-후보로-먼저-연결한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-004` | 수확 판로 영향과 비축은 서버 계산 후보로 먼저 연결한다 | `TitleReviewed` |
| [D-042](../DECISIONS.md#d-042-world-map과-정착지-내부는-같은-simulation-snapshot의-관찰-규모다) | `D-SIMULATION-SETTLEMENT-ECONOMY-005` | World Map과 정착지 내부는 같은 Simulation snapshot의 관찰 규모다 | `TitleReviewed` |
| [D-043](../DECISIONS.md#d-043-수확-판로-confirm은-capacity-예약이고-task-완료-tick은-경제-원장-적용이다) | `D-SIMULATION-SETTLEMENT-ECONOMY-006` | 수확 판로 Confirm은 capacity 예약이고 Task 완료 Tick은 경제 원장 적용이다 | `TitleReviewed` |
| [D-044](../DECISIONS.md#d-044-world-navigation은-상위-선택을-보존하고-하위-선택만-해제한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-007` | World navigation은 상위 선택을 보존하고 하위 선택만 해제한다 | `TitleReviewed` |
| [D-045](../DECISIONS.md#d-045-synty-에셋은-자동-원본-목록과-사람의-연구-기록을-분리해-승격한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-008` | Synty 에셋은 자동 원본 목록과 사람의 연구 기록을 분리해 승격한다 | `TitleReviewed` |
| [D-046](../DECISIONS.md#d-046-unity-판로-adapter는-서버-preview-입력과-후보-task-의미만-구성한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-009` | Unity 판로 adapter는 서버 Preview 입력과 후보 Task 의미만 구성한다 | `TitleReviewed` |
| [D-047](../DECISIONS.md#d-047-unity-연구-scene-파일명은-한국어-목적-이름을-사용한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-010` | Unity 연구 Scene 파일명은 한국어 목적 이름을 사용한다 | `TitleReviewed` |
| [D-048](../DECISIONS.md#d-048-정착지-1차-미술은-semantic-visualkey와-고정-presentation-시간으로-구성한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-011` | 정착지 1차 미술은 semantic VisualKey와 고정 Presentation 시간으로 구성한다 | `TitleReviewed` |
| [D-049](../DECISIONS.md#d-049-unity-정착지-상호작용은-simulation-authority-응답만-reconcile한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-012` | Unity 정착지 상호작용은 Simulation authority 응답만 reconcile한다 | `TitleReviewed` |
| [D-050](../DECISIONS.md#d-050-cargo-이동은-공통-worldtick-task와-원재고-예약을-함께-보존한다) | `D-SIMULATION-SETTLEMENT-ECONOMY-013` | Cargo 이동은 공통 WorldTick Task와 원재고 예약을 함께 보존한다 | `TitleReviewed` |
| [D-051](../DECISIONS.md#d-051-unity-c-이름은-한국어-업무-의미와-영어-기술-역할을-조합한다) | `D-PLANNING-CODE-NAMING-001` | Unity C# 이름은 한국어 업무 의미와 영어 기술 역할을 조합한다 | `TitleReviewed` |
| [D-052](../DECISIONS.md#d-052-운영-api의-업무-규칙은-순수-공통-계층을-거쳐-simulation에-적용한다) | `D-SIMULATION-LOGISTICS-TRADE-001` | 운영 API의 업무 규칙은 순수 공통 계층을 거쳐 Simulation에 적용한다 | `TitleReviewed` |
| [D-053](../DECISIONS.md#d-053-simulation-화물운송은-cargo-이동과-업무-상태-원장을-분리해-결합한다) | `D-SIMULATION-LOGISTICS-TRADE-002` | Simulation 화물운송은 Cargo 이동과 업무 상태 원장을 분리해 결합한다 | `TitleReviewed` |
| [D-054](../DECISIONS.md#d-054-simulation-같이주문은-명시적-개별-의향을-보존한-모집-결과-원장이다) | `D-SIMULATION-LOGISTICS-TRADE-003` | Simulation 같이주문은 명시적 개별 의향을 보존한 모집 결과 원장이다 | `TitleReviewed` |
| [D-055](../DECISIONS.md#d-055-simulation-음식배달의-전달과-주문자-수령-확인을-분리한다) | `D-SIMULATION-LOGISTICS-TRADE-004` | Simulation 음식배달의 전달과 주문자 수령 확인을 분리한다 | `TitleReviewed` |
| [D-056](../DECISIONS.md#d-056-주민-소비는-주문-이행에서-이미-차감된-시장재고를-다시-차감하지-않는다) | `D-SIMULATION-LOGISTICS-TRADE-005` | 주민 소비는 주문 이행에서 이미 차감된 시장재고를 다시 차감하지 않는다 | `TitleReviewed` |
| [D-057](../DECISIONS.md#d-057-수출-준비-검사는-운영-수출이-아니라-실패를-보존하는-simulation-후보-원장이다) | `D-SIMULATION-LOGISTICS-TRADE-006` | 수출 준비 검사는 운영 수출이 아니라 실패를 보존하는 Simulation 후보 원장이다 | `TitleReviewed` |
| [D-058](../DECISIONS.md#d-058-수출-재작업은-실패-원장을-덮어쓰지-않는-새-검사-시도다) | `D-SIMULATION-LOGISTICS-TRADE-007` | 수출 재작업은 실패 원장을 덮어쓰지 않는 새 검사 시도다 | `TitleReviewed` |
| [D-059](../DECISIONS.md#d-059-cargo-준비-완료는-배송대행지-인계나-차량-출발이-아니다) | `D-SIMULATION-LOGISTICS-TRADE-008` | Cargo 준비 완료는 배송대행지 인계나 차량 출발이 아니다 | `TitleReviewed` |
| [D-060](../DECISIONS.md#d-060-배송대행지-simulation-인계와-물류-이동-시작을-분리한다) | `D-SIMULATION-LOGISTICS-TRADE-009` | 배송대행지 Simulation 인계와 물류 이동 시작을 분리한다 | `TitleReviewed` |
| [D-061](../DECISIONS.md#d-061-수출-cargo-물류-이동은-기존-출고-예약을-승계한다) | `D-SIMULATION-LOGISTICS-TRADE-010` | 수출 Cargo 물류 이동은 기존 출고 예약을 승계한다 | `TitleReviewed` |
| [D-062](../DECISIONS.md#d-062-항만-준비시설-도착과-인수-완료를-분리한다) | `D-SIMULATION-LOGISTICS-TRADE-011` | 항만 준비시설 도착과 인수 완료를 분리한다 | `TitleReviewed` |
| [D-063](../DECISIONS.md#d-063-수출-준비성-검토는-자기-진술형-simulation-후보로-제한한다) | `D-SIMULATION-LOGISTICS-TRADE-012` | 수출 준비성 검토는 자기 진술형 Simulation 후보로 제한한다 | `TitleReviewed` |
| [D-064](../DECISIONS.md#d-064-수출-선적-계획은-비교-가능한-추정-후보이며-재정을-바꾸지-않는다) | `D-SIMULATION-LOGISTICS-TRADE-013` | 수출 선적 계획은 비교 가능한 추정 후보이며 재정을 바꾸지 않는다 | `TitleReviewed` |
| [D-065](../DECISIONS.md#d-065-수출-실행-결과는-seed-기반으로-숨겨-두고-기존-예상-매출과-정산한다) | `D-SIMULATION-LOGISTICS-TRADE-014` | 수출 실행 결과는 seed 기반으로 숨겨 두고 기존 예상 매출과 정산한다 | `TitleReviewed` |
| [D-066](../DECISIONS.md#d-066-수확물-판로-카드는-기존-원장의-읽기-projection만-사용한다) | `D-SIMULATION-LOGISTICS-TRADE-015` | 수확물 판로 카드는 기존 원장의 읽기 projection만 사용한다 | `TitleReviewed` |
| [D-067](../DECISIONS.md#d-067-unity-판로-결과-카드는-서버-읽기-projection을-한국어로만-표현한다) | `D-SIMULATION-LOGISTICS-TRADE-016` | Unity 판로 결과 카드는 서버 읽기 projection을 한국어로만 표현한다 | `TitleReviewed` |
| [D-068](../DECISIONS.md#d-068-unity-판로-재접속은-session과-결과-목록의-동일-revision을-원자적으로-적용한다) | `D-SIMULATION-LOGISTICS-TRADE-017` | Unity 판로 재접속은 session과 결과 목록의 동일 revision을 원자적으로 적용한다 | `TitleReviewed` |
| [D-069](../DECISIONS.md#d-069-unity-판로-작업-재개는-session-task의-남은-tick만-사용한다) | `D-SIMULATION-LOGISTICS-TRADE-018` | Unity 판로 작업 재개는 session Task의 남은 Tick만 사용한다 | `TitleReviewed` |
| [D-070](../DECISIONS.md#d-070-플레이-경영-시간은-명시적-턴-마감으로만-진행한다) | `D-SIMULATION-LOGISTICS-TRADE-019` | 플레이 경영 시간은 명시적 턴 마감으로만 진행한다 | `TitleReviewed` |
| [D-071](../DECISIONS.md#d-071-unity-다중-판로-카드는-object-lot-명시-mapping으로만-선택한다) | `D-SIMULATION-LOGISTICS-TRADE-020` | Unity 다중 판로 카드는 object-Lot 명시 mapping으로만 선택한다 | `TitleReviewed` |
| [D-072](../DECISIONS.md#d-072-문화-턴-카드는-지역기간공식-원천달력효과-규칙이-완전할-때만-게시한다) | `D-DATA-TIME-PUBLIC-DATA-001` | 문화 턴 카드는 지역·기간·공식 원천·달력·효과 규칙이 완전할 때만 게시한다 | `TitleReviewed` |
| [D-073](../DECISIONS.md#d-073-unity-에셋-현실-관측은-연구-해석과-simulation에서-분리한다) | `D-DATA-TIME-PUBLIC-DATA-002` | Unity 에셋 현실 관측은 연구 해석과 Simulation에서 분리한다 | `TitleReviewed` |
| [D-074](../DECISIONS.md#d-074-kamis-대응-작물은-모판에서-연구한-뒤-farm-scene으로-승격한다) | `D-DATA-TIME-PUBLIC-DATA-003` | KAMIS 대응 작물은 모판에서 연구한 뒤 Farm Scene으로 승격한다 | `TitleReviewed` |
| [D-075](../DECISIONS.md#d-075-공공-관측-출처표와-에셋-연결표는-분리하고-모판-문맥으로-선택한다) | `D-DATA-TIME-PUBLIC-DATA-004` | 공공 관측 출처표와 에셋 연결표는 분리하고 모판 문맥으로 선택한다 | `TitleReviewed` |
| [D-076](../DECISIONS.md#d-076-농사-생육은-일수-대신-환경-snapshot의-제한-요인과-스트레스로-진행한다) | `D-DATA-TIME-PUBLIC-DATA-005` | 농사 생육은 일수 대신 환경 Snapshot의 제한 요인과 스트레스로 진행한다 | `TitleReviewed` |
| [D-077](../DECISIONS.md#d-077-unity-턴-마감은-confirm-뒤-canonical-session을-다시-조회한다) | `D-DATA-TIME-PUBLIC-DATA-006` | Unity 턴 마감은 Confirm 뒤 canonical session을 다시 조회한다 | `TitleReviewed` |
| [D-078](../DECISIONS.md#d-078-턴-카드는-분야별-모판에서-검증한-뒤-서버-덱으로-승격한다) | `D-DATA-TIME-PUBLIC-DATA-007` | 턴 카드는 분야별 모판에서 검증한 뒤 서버 덱으로 승격한다 | `TitleReviewed` |
| [D-079](../DECISIONS.md#d-079-농사로-작업군콘텐츠canonical-상품-관계를-분리한다) | `D-DATA-TIME-PUBLIC-DATA-008` | 농사로 작업군·콘텐츠·canonical 상품 관계를 분리한다 | `TitleReviewed` |
| [D-080](../DECISIONS.md#d-080-기상청-asos-일관측은-지점날짜원문-단위로-보존한다) | `D-DATA-TIME-PUBLIC-DATA-009` | 기상청 ASOS 일관측은 지점·날짜·원문 단위로 보존한다 | `TitleReviewed` |
| [D-081](../DECISIONS.md#d-081-통합-모판전시관의-scene-이식-단위는-업무-장면이-아니라-개별-object다) | `D-SIMULATION-WORLD-OBJECT-RULES-001` | 통합 모판·전시관의 Scene 이식 단위는 업무 장면이 아니라 개별 Object다 | `TitleReviewed` |
| [D-082](../DECISIONS.md#d-082-simulation-생산소비는-부호가-아니라-자원-변동-유형과-효과-묶음으로-기록한다) | `D-SIMULATION-WORLD-OBJECT-RULES-002` | Simulation 생산·소비는 부호가 아니라 자원 변동 유형과 효과 묶음으로 기록한다 | `TitleReviewed` |
| [D-083](../DECISIONS.md#d-083-규칙은-업무해석표현상호작용-계층과-세부-영역으로-분리한다) | `D-SIMULATION-WORLD-OBJECT-RULES-003` | 규칙은 업무·해석·표현·상호작용 계층과 세부 영역으로 분리한다 | `TitleReviewed` |
| [D-084](../DECISIONS.md#d-084-감자-생산의-첫-기준-단위는-명시적-면적을-가진-단일-tile-재배-단위다) | `D-SIMULATION-WORLD-OBJECT-RULES-004` | 감자 생산의 첫 기준 단위는 명시적 면적을 가진 단일 Tile 재배 단위다 | `TitleReviewed` |
| [D-085](../DECISIONS.md#d-085-수요예약주문-이행주민-소비는-서로-다른-자원-단계다) | `D-SIMULATION-WORLD-OBJECT-RULES-005` | 수요·예약·주문 이행·주민 소비는 서로 다른 자원 단계다 | `TitleReviewed` |
| [D-086](../DECISIONS.md#d-086-운송은-상차이동-자원-소비하차인수-확인을-분리한다) | `D-SIMULATION-WORLD-OBJECT-RULES-006` | 운송은 상차·이동 자원 소비·하차·인수 확인을 분리한다 | `TitleReviewed` |
| [D-087](../DECISIONS.md#d-087-창고는-인수검수적치보관피킹출고와-용량을-함께-기록한다) | `D-SIMULATION-WORLD-OBJECT-RULES-007` | 창고는 인수·검수·적치·보관·피킹·출고와 용량을 함께 기록한다 | `TitleReviewed` |
| [D-088](../DECISIONS.md#d-088-unity-표현-규칙은-영역별-출력-채널과-구현-상태를-대장으로-관리한다) | `D-SIMULATION-WORLD-OBJECT-RULES-008` | Unity 표현 규칙은 영역별 출력 채널과 구현 상태를 대장으로 관리한다 | `TitleReviewed` |
| [D-089](../DECISIONS.md#d-089-통합-전시관의-규칙-실험대는-미리보기와-서버-재조회-결과를-비교한다) | `D-SIMULATION-WORLD-OBJECT-RULES-009` | 통합 전시관의 규칙 실험대는 미리보기와 서버 재조회 결과를 비교한다 | `TitleReviewed` |
| [D-090](../DECISIONS.md#d-090-unity-감자-생산-실험대는-서버-효과를-재계산하지-않고-변환한다) | `D-SIMULATION-WORLD-OBJECT-RULES-010` | Unity 감자 생산 실험대는 서버 효과를 재계산하지 않고 변환한다 | `TitleReviewed` |
| [D-091](../DECISIONS.md#d-091-다품목-unity-모판은-서버가-보장하는-연결-깊이를-품목별로-구분한다) | `D-SIMULATION-WORLD-OBJECT-RULES-011` | 다품목 Unity 모판은 서버가 보장하는 연결 깊이를 품목별로 구분한다 | `TitleReviewed` |
| [D-092](../DECISIONS.md#d-092-unity-운영-api-client는-공통-전송-계층과-인증-경계를-재사용한다) | `D-SIMULATION-WORLD-OBJECT-RULES-012` | Unity 운영 API Client는 공통 전송 계층과 인증 경계를 재사용한다 | `TitleReviewed` |
| [D-093](../DECISIONS.md#d-093-게임-세계-simulation-서버를-실제-운영-전-예행연습-서버로-사용한다) | `D-SIMULATION-WORLD-OBJECT-RULES-013` | 게임 세계 Simulation 서버를 실제 운영 전 예행연습 서버로 사용한다 | `TitleReviewed` |
| [D-094](../DECISIONS.md#d-094-simulation-서버는-수집된-공공데이터-db를-읽기-전용으로-공유한다) | `D-SIMULATION-WORLD-OBJECT-RULES-014` | Simulation 서버는 수집된 공공데이터 DB를 읽기 전용으로 공유한다 | `TitleReviewed` |
| [D-095](../DECISIONS.md#d-095-운영자-전용-재고-shelf는-주소-지정-가능한-피킹-위치-단위다) | `D-SIMULATION-WORLD-OBJECT-RULES-015` | 운영자 전용 재고 Shelf는 주소 지정 가능한 피킹 위치 단위다 | `TitleReviewed` |
| [D-096#1](../DECISIONS.md#d-096-simulationworldshell의-플레이어-카메라는-presentation-전용-입력-모듈이다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-001` | SimulationWorldShell의 플레이어 카메라는 Presentation 전용 입력 모듈이다 | `TitleReviewed` |
| [D-096#2](../DECISIONS.md#d-096-일반-타로를-경영-게임의-기본-덱으로-두고-학당-카드는-선택형-확장으로-분리한다) | `D-GAMEPLAY-TAROT-001` | 일반 타로를 경영 게임의 기본 덱으로 두고 학당 카드는 선택형 확장으로 분리한다 | `TitleReviewed` |
| [D-097](../DECISIONS.md#d-097-타로-규칙은-기존-업무-규칙에-보정선을-제공하는-상위-시뮬레이션-규칙이다) | `D-GAMEPLAY-TAROT-002` | 타로 규칙은 기존 업무 규칙에 보정선을 제공하는 상위 시뮬레이션 규칙이다 | `TitleReviewed` |
| [D-098](../DECISIONS.md#d-098-일반-타로-뽑기는-seed턴덱-개정-번호선택-이력으로-결정한다) | `D-GAMEPLAY-TAROT-003` | 일반 타로 뽑기는 seed·턴·덱 개정 번호·선택 이력으로 결정한다 | `TitleReviewed` |
| [D-099](../DECISIONS.md#d-099-타로-객체-관계와-현재-강조-상태를-분리한다) | `D-GAMEPLAY-TAROT-004` | 타로 객체 관계와 현재 강조 상태를 분리한다 | `TitleReviewed` |
| [D-100](../DECISIONS.md#d-100-공간-world는-고정-tileareaareaset과-통계-구성-대장으로-반복-생성한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-001` | 공간 World는 고정 Tile·Area·AreaSet과 통계 구성 대장으로 반복 생성한다 | `TitleReviewed` |
| [D-101](../DECISIONS.md#d-101-행정구역별-건물은-출처별-db-원장을-먼저-구축하고-world에-투영한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-002` | 행정구역별 건물은 출처별 DB 원장을 먼저 구축하고 World에 투영한다 | `TitleReviewed` |
| [D-102](../DECISIONS.md#d-102-건축물-공식-주용도와-상위-경관-category를-분리한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-003` | 건축물 공식 주용도와 상위 경관 Category를 분리한다 | `TitleReviewed` |
| [D-103](../DECISIONS.md#d-103-건축물-형태의-공식값단순-계산값synty-표현값을-분리한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-004` | 건축물 형태의 공식값·단순 계산값·Synty 표현값을 분리한다 | `TitleReviewed` |
| [D-104](../DECISIONS.md#d-104-건물-안의-상호는-공개-인허가-사업장과-보수적인-주소-연결로-표현한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-005` | 건물 안의 상호는 공개 인허가 사업장과 보수적인 주소 연결로 표현한다 | `TitleReviewed` |
| [D-105](../DECISIONS.md#d-105-공유-공공데이터-db와-simulation-world-파생-관계-db를-분리한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-006` | 공유 공공데이터 DB와 Simulation World 파생 관계 DB를 분리한다 | `TitleReviewed` |
| [D-106](../DECISIONS.md#d-106-unity-공간-실행과-synty-경관-실행을-독립-파이프라인으로-분리한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-007` | Unity 공간 실행과 Synty 경관 실행을 독립 파이프라인으로 분리한다 | `TitleReviewed` |
| [D-107](../DECISIONS.md#d-107-simulation-상태는-의미-기반-렌더링-의도를-거쳐-urp-표현으로-번역한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-008` | Simulation 상태는 의미 기반 렌더링 의도를 거쳐 URP 표현으로 번역한다 | `TitleReviewed` |
| [D-108](../DECISIONS.md#d-108-공간-규칙과-simulation-규칙은-개정-가능한-객체-표현-결합-원장에서-만난다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-009` | 공간 규칙과 Simulation 규칙은 개정 가능한 객체 표현 결합 원장에서 만난다 | `TitleReviewed` |
| [D-109](../DECISIONS.md#d-109-평창군-unity-공간-표현은-전체-원장을-보존하고-건물-종류별-하나로-축약한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-010` | 평창군 Unity 공간 표현은 전체 원장을 보존하고 건물 종류별 하나로 축약한다 | `TitleReviewed` |
| [D-110](../DECISIONS.md#d-110-simulation-서버는-운영-서버의-컨테이너-관례를-따르되-db-권한과-migration을-분리한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-011` | Simulation 서버는 운영 서버의 컨테이너 관례를 따르되 DB 권한과 migration을 분리한다 | `TitleReviewed` |
| [D-111](../DECISIONS.md#d-111-simulation-world-파생-db는-업무-규칙의-관계와-계보를-집결한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-012` | Simulation World 파생 DB는 업무 규칙의 관계와 계보를 집결한다 | `TitleReviewed` |
| [D-112](../DECISIONS.md#d-112-unity-ui-구현-전-figma-근거-ui-기획-원장을-둔다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-013` | Unity UI 구현 전 Figma 근거 UI 기획 원장을 둔다 | `TitleReviewed` |
| [D-113](../DECISIONS.md#d-113-ui는-규칙-식별자가-아니라-객체업무-규칙-연결을-통해-조립한다) | `D-WORLD-SPATIAL-DATA-PRESENTATION-014` | UI는 규칙 식별자가 아니라 객체–업무 규칙 연결을 통해 조립한다 | `TitleReviewed` |
| [D-114](../DECISIONS.md#d-114-tile과-경관-완결-영역의-책임을-분리한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-002` | Tile과 경관 완결 영역의 책임을 분리한다 | `TitleReviewed` |
| [D-115](../DECISIONS.md#d-115-경관-품질은-synty-연결-뒤-독립-rendering-profile로-일괄-적용한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-003` | 경관 품질은 Synty 연결 뒤 독립 Rendering Profile로 일괄 적용한다 | `TitleReviewed` |
| [D-116](../DECISIONS.md#d-116-플레이어-경관-탐색은-simulation-권위와-분리한-표현-전용-입력이다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-004` | 플레이어 경관 탐색은 Simulation 권위와 분리한 표현 전용 입력이다 | `TitleReviewed` |
| [D-117](../DECISIONS.md#d-117-npc-직업권한은-운영-인증이-아니라-simulation-조직역량위임-규칙으로-실행한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-005` | NPC 직업·권한은 운영 인증이 아니라 Simulation 조직·역량·위임 규칙으로 실행한다 | `TitleReviewed` |
| [D-118](../DECISIONS.md#d-118-ui-행동은-실행-가능한-호출-계약과-확정-뒤-재조회를-함께-제공한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-006` | UI 행동은 실행 가능한 호출 계약과 확정 뒤 재조회를 함께 제공한다 | `TitleReviewed` |
| [D-119](../DECISIONS.md#d-119-입고-검수-완료와-적재-완료를-다른-상태행동으로-관리한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-007` | 입고 검수 완료와 적재 완료를 다른 상태·행동으로 관리한다 | `TitleReviewed` |
| [D-120](../DECISIONS.md#d-120-figmamauiunity는-디자인-의미를-공유하고-렌더러-구현은-분리한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-008` | Figma·MAUI·Unity는 디자인 의미를 공유하고 렌더러 구현은 분리한다 | `TitleReviewed` |
| [D-121](../DECISIONS.md#d-121-unity-최종-실행은-simulationworldshell-하나에-통합한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-009` | Unity 최종 실행은 SimulationWorldShell 하나에 통합한다 | `TitleReviewed` |
| [D-122](../DECISIONS.md#d-122-1인칭-월드는-고정-l2-타일-창과-자료-상태를-따라-동적으로-준비한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-010` | 1인칭 월드는 고정 L2 타일 창과 자료 상태를 따라 동적으로 준비한다 | `TitleReviewed` |
| [D-123](../DECISIONS.md#d-123-타일-안전-창과-카메라-시야-기반-표현-우선순위를-분리한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-011` | 타일 안전 창과 카메라 시야 기반 표현 우선순위를 분리한다 | `TitleReviewed` |
| [D-124](../DECISIONS.md#d-124-월드-api는-행정동법정동-파생-projection을-먼저-읽는다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-012` | 월드 API는 행정동·법정동 파생 Projection을 먼저 읽는다 | `TitleReviewed` |
| [D-125](../DECISIONS.md#d-125-l2-runtime은-상세-33활성-55준비-99의-예산형-창을-사용한다) | `D-PRESENTATION-WORLD-CAMERA-STREAMING-013` | L2 Runtime은 상세 3×3·활성 5×5·준비 9×9의 예산형 창을 사용한다 | `TitleReviewed` |
| [D-126](../DECISIONS.md#d-126-생존-타로는-안전-거점-전원-합의-뒤-다음-tick에만-적용한다) | `D-GAMEPLAY-TEAM-COMBAT-001` | 생존 타로는 안전 거점 전원 합의 뒤 다음 Tick에만 적용한다 | `TitleReviewed` |
| [D-127](../DECISIONS.md#d-127-세계-사건은-서버-원장에-먼저-확정하고-unity는-개정-기반-표현-자료만-읽는다) | `D-GAMEPLAY-TEAM-COMBAT-002` | 세계 사건은 서버 원장에 먼저 확정하고 Unity는 개정 기반 표현 자료만 읽는다 | `TitleReviewed` |
| [D-128](../DECISIONS.md#d-128-농장-생존은-플레이어npc-노동과-회복-가능한-위협을-같은-session-원장에-둔다) | `D-GAMEPLAY-TEAM-COMBAT-003` | 농장 생존은 플레이어·NPC 노동과 회복 가능한 위협을 같은 Session 원장에 둔다 | `TitleReviewed` |
| [D-129](../DECISIONS.md#d-129-같은-simulation-팀은-별도-요청-없이-서로-관찰하되-조작-권한을-공유하지-않는다) | `D-GAMEPLAY-TEAM-COMBAT-004` | 같은 Simulation 팀은 별도 요청 없이 서로 관찰하되 조작 권한을 공유하지 않는다 | `TitleReviewed` |
| [D-130](../DECISIONS.md#d-130-simulation-역할은-고정-직업이-아니라-팀-공동-카드와-현재-활동에서-파생한다) | `D-GAMEPLAY-TEAM-COMBAT-005` | Simulation 역할은 고정 직업이 아니라 팀 공동 카드와 현재 활동에서 파생한다 | `TitleReviewed` |
| [D-131](../DECISIONS.md#d-131-역할-카드-규칙-정의와-현재-장착-상태의-db-책임을-분리한다) | `D-GAMEPLAY-TEAM-COMBAT-006` | 역할 카드 규칙 정의와 현재 장착 상태의 DB 책임을 분리한다 | `TitleReviewed` |
| [D-132](../DECISIONS.md#d-132-simulation-session-저장-자료는-별도-db에-보존하고-command-재생으로-복원한다) | `D-GAMEPLAY-TEAM-COMBAT-007` | Simulation Session 저장 자료는 별도 DB에 보존하고 Command 재생으로 복원한다 | `TitleReviewed` |
| [D-133](../DECISIONS.md#d-133-농사영역-발견-보상은-개인-수집-카드-원장으로-분리한다) | `D-GAMEPLAY-TEAM-COMBAT-008` | 농사·영역 발견 보상은 개인 수집 카드 원장으로 분리한다 | `TitleReviewed` |
| [D-134](../DECISIONS.md#d-134-활동별-기본-시점은-편의-정책이며-사용자의-허용된-수동-전환을-막지-않는다) | `D-GAMEPLAY-TEAM-COMBAT-009` | 활동별 기본 시점은 편의 정책이며 사용자의 허용된 수동 전환을 막지 않는다 | `TitleReviewed` |
| [D-135](../DECISIONS.md#d-135-1인칭과-3인칭은-전환-전용-카메라로-연속-보간한다) | `D-GAMEPLAY-TEAM-COMBAT-010` | 1인칭과 3인칭은 전환 전용 카메라로 연속 보간한다 | `TitleReviewed` |
| [D-136](../DECISIONS.md#d-136-전투는-서버-권위-단일-박자로-판정하고-시점별-이점은-일반-허용-구간에만-둔다) | `D-GAMEPLAY-TEAM-COMBAT-011` | 전투는 서버 권위 단일 박자로 판정하고 시점별 이점은 일반 허용 구간에만 둔다 | `TitleReviewed` |
| [D-137](../DECISIONS.md#d-137-1인칭-영웅-성과는-한-명령창-동안만-주변-전선의-전술-기회가-된다) | `D-GAMEPLAY-TEAM-COMBAT-012` | 1인칭 영웅 성과는 한 명령창 동안만 주변 전선의 전술 기회가 된다 | `TitleReviewed` |
| [D-138](../DECISIONS.md#d-138-분대-이동은-서버-판정의-교체-가능한-표현이며-기준점과-대형-슬롯을-분리한다) | `D-GAMEPLAY-TEAM-COMBAT-013` | 분대 이동은 서버 판정의 교체 가능한 표현이며 기준점과 대형 슬롯을 분리한다 | `TitleReviewed` |
| [D-139](../DECISIONS.md#d-139-simulationunity-구조-리팩토링은-외부-계약을-보존한-채-검증-경계부터-진행한다) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-001` | Simulation·Unity 구조 리팩토링은 외부 계약을 보존한 채 검증 경계부터 진행한다 | `TitleReviewed` |
| [D-140](../DECISIONS.md#d-140-코드-탐색-특성이-원본이고-생성-코드-지도는-검증되는-파생-자료다) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-002` | 코드 탐색 특성이 원본이고 생성 코드 지도는 검증되는 파생 자료다 | `TitleReviewed` |
| [D-141](../DECISIONS.md#d-141-1인칭-전투-마우스-입력은-전투-진입과-서버-판정-반응으로-분리한다) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-003` | 1인칭 전투 마우스 입력은 전투 진입과 서버 판정 반응으로 분리한다 | `TitleReviewed` |
| [D-142](../DECISIONS.md#d-142-기본-생존-장은-경관-산책-중심이며-직접-전투는-계절-방어의-선택-경로다) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-004` | 기본 생존 장은 경관 산책 중심이며 직접 전투는 계절 방어의 선택 경로다 | `TitleReviewed` |
| [D-143](../DECISIONS.md#d-143-지역-공공데이터는-원본을-보존하고-lod별-대표-요약과-상세-조회로-나눈다) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-005` | 지역 공공데이터는 원본을 보존하고 LOD별 대표 요약과 상세 조회로 나눈다 | `TitleReviewed` |
| [D-144](../DECISIONS.md#d-144-synty-팩은-기술-대장팩별-기준의미-구성검토-계획을-거쳐-scene에-적용한다) | `D-ARCHITECTURE-REFACTOR-DATA-ASSET-006` | Synty 팩은 기술 대장·팩별 기준·의미 구성·검토 계획을 거쳐 Scene에 적용한다 | `TitleReviewed` |
| [D-145](../DECISIONS.md#d-145-미완료-작업은-증거-단계-원장으로-관리하고-중앙-l2-실자료부터-종단-완결한다) | `D-EVIDENCE-SPATIAL-MATURITY-001` | 미완료 작업은 증거 단계 원장으로 관리하고 중앙 L2 실자료부터 종단 완결한다 | `TitleReviewed` |
| [D-146](../DECISIONS.md#d-146-areaset은-문서-중심-상위-컨테이너이고-landscapegraph는-독립-조립스트리밍-단위다) | `D-EVIDENCE-SPATIAL-MATURITY-002` | AreaSet은 문서 중심 상위 컨테이너이고 LandscapeGraph는 독립 조립·스트리밍 단위다 | `TitleReviewed` |
| [D-147](../DECISIONS.md#d-147-공간과-simulation은-세계-상호작용-단위로-종단-연결한다) | `D-EVIDENCE-SPATIAL-MATURITY-003` | 공간과 Simulation은 세계 상호작용 단위로 종단 연결한다 | `TitleReviewed` |
| [D-148](../DECISIONS.md#d-148-세계-상호작용-단위의-기본-구현-완료는-e3이고-실세계-승격은-별도로-관리한다) | `D-EVIDENCE-SPATIAL-MATURITY-004` | 세계 상호작용 단위의 기본 구현 완료는 E3이고 실세계 승격은 별도로 관리한다 | `TitleReviewed` |
| [D-149](../DECISIONS.md#d-149-wi-e3-승격은-핵심-인과선공통-규칙문서전체-회귀-순으로-나눈다) | `D-EVIDENCE-SPATIAL-MATURITY-005` | WI E3 승격은 핵심 인과선·공통 규칙·문서·전체 회귀 순으로 나눈다 | `TitleReviewed` |
| [D-150](../DECISIONS.md#d-150-e4-승격은-wi-공간-폐루프와-graph-계보를-기준으로-개별-판정한다) | `D-EVIDENCE-SPATIAL-MATURITY-006` | E4 승격은 WI 공간 폐루프와 Graph 계보를 기준으로 개별 판정한다 | `TitleReviewed` |
| [D-151](../DECISIONS.md#d-151-e4e7은-장소경관공공데이터실제-플레이로-분리한다) | `D-EVIDENCE-SPATIAL-MATURITY-007` | E4~E7은 장소·경관·공공데이터·실제 플레이로 분리한다 | `TitleReviewed` |
| [D-152](../DECISIONS.md#d-152-e4는-wi-공간-모판이고-e5는-실제-지역-경관-조립이다) | `D-EVIDENCE-SPATIAL-MATURITY-008` | E4는 WI 공간 모판이고 E5는 실제 지역 경관 조립이다 | `TitleReviewed` |
| [D-153](../DECISIONS.md#d-153-e-증거-단계와-h-공간-포함-계층을-분리한다) | `D-EVIDENCE-SPATIAL-MATURITY-009` | E 증거 단계와 H 공간 포함 계층을 분리한다 | `TitleReviewed` |
| [D-154](../DECISIONS.md#d-154-모판을-h1h4-상향-조립-공간-구성-재고로-확장한다) | `D-EVIDENCE-SPATIAL-MATURITY-010` | 모판을 H1~H4 상향 조립 공간 구성 재고로 확장한다 | `TitleReviewed` |
| [D-155](../DECISIONS.md#d-155-synty-상향식-공간-재고는-공식-h-계층과-분리해-축적한다) | `D-EVIDENCE-SPATIAL-MATURITY-011` | Synty 상향식 공간 재고는 공식 H 계층과 분리해 축적한다 | `TitleReviewed` |
| [D-156](../DECISIONS.md#d-156-기준-경관-문법은-검토된-조립법으로-h1h4-설계-후보를-유도한다) | `D-EVIDENCE-SPATIAL-MATURITY-012` | 기준 경관 문법은 검토된 조립법으로 H1~H4 설계 후보를 유도한다 | `TitleReviewed` |
| [D-157](../DECISIONS.md#d-157-오픈-월드는-고정-좌표-경계가-아니라-h4-의도와-h3h2-streaming-coverage로-연다) | `D-EVIDENCE-SPATIAL-MATURITY-013` | 오픈 월드는 고정 좌표 경계가 아니라 H4 의도와 H3·H2 Streaming Coverage로 연다 | `TitleReviewed` |
| [D-158](../DECISIONS.md#d-158-lh-엔진은-l-해상도와-h-의미-권위를-직교시키고-승인-h4-안에서-결정적으로-선행-생성한다) | `D-EVIDENCE-SPATIAL-MATURITY-014` | LH 엔진은 L 해상도와 H 의미 권위를 직교시키고 승인 H4 안에서 결정적으로 선행 생성한다 | `TitleReviewed` |
| [D-159](../DECISIONS.md#d-159-wi별-eh-성립-상태는-후보-계보와-실행-증거를-분리해-lh-인계-입력으로-생성한다) | `D-EVIDENCE-SPATIAL-MATURITY-015` | WI별 E/H 성립 상태는 후보 계보와 실행 증거를 분리해 LH 인계 입력으로 생성한다 | `TitleReviewed` |
| [D-160](../DECISIONS.md#d-160-싱글-플레이-lh-지도-생성은-로컬-엔진을-기본-권위로-하고-서버-연결은-선택-동기화로-둔다) | `D-EVIDENCE-SPATIAL-MATURITY-016` | 싱글 플레이 LH 지도 생성은 로컬 엔진을 기본 권위로 하고 서버 연결은 선택 동기화로 둔다 | `TitleReviewed` |
| [D-161](../DECISIONS.md#d-161-음식화물-배달은-npc-경로-이동을-기본-수행-방식으로-사용한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-001` | 음식·화물 배달은 NPC 경로 이동을 기본 수행 방식으로 사용한다 | `TitleReviewed` |
| [D-162](../DECISIONS.md#d-162-nature-생활권은-주인공의-상시-체류-세계이고-farmtowncityhub는-전문-경관-인스턴스다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-002` | Nature 생활권은 주인공의 상시 체류 세계이고 Farm·Town·City/Hub는 전문 경관 인스턴스다 | `TitleReviewed` |
| [D-163](../DECISIONS.md#d-163-전문-경관의-미해결-사건은-경로별-자연권-위협으로-전파한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-003` | 전문 경관의 미해결 사건은 경로별 자연권 위협으로 전파한다 | `TitleReviewed` |
| [D-164](../DECISIONS.md#d-164-상향식-공간-재고는-nature-위협회복-카드부터-작은-묶음으로-확장한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-004` | 상향식 공간 재고는 Nature 위협·회복 카드부터 작은 묶음으로 확장한다 | `TitleReviewed` |
| [D-165](../DECISIONS.md#d-165-사건-대응-h1은-nature에서-시작해-farm과-town으로-이어지는-h2-우선순위로-조립한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-005` | 사건 대응 H1은 Nature에서 시작해 Farm과 Town으로 이어지는 H2 우선순위로 조립한다 | `TitleReviewed` |
| [D-166](../DECISIONS.md#d-166-naturefarmtowncityhub는-각각-독립-areaset-후보로-상향-조립한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-006` | Nature·Farm·Town·City/Hub는 각각 독립 AreaSet 후보로 상향 조립한다 | `TitleReviewed` |
| [D-167](../DECISIONS.md#d-167-farm-areaset-후보는-생산-흐름과-사건-격리회복-흐름을-함께-포함한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-007` | Farm AreaSet 후보는 생산 흐름과 사건 격리·회복 흐름을 함께 포함한다 | `TitleReviewed` |
| [D-168](../DECISIONS.md#d-168-town-areaset-후보는-시장-생활-흐름과-오염-통제주민-구호-흐름을-함께-포함한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-008` | Town AreaSet 후보는 시장 생활 흐름과 오염 통제·주민 구호 흐름을 함께 포함한다 | `TitleReviewed` |
| [D-169](../DECISIONS.md#d-169-h1h4는-위치-독립-공간-설계-계층이며-공공데이터-결속은-e6에서만-수행한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-009` | H1~H4는 위치 독립 공간 설계 계층이며 공공데이터 결속은 E6에서만 수행한다 | `TitleReviewed` |
| [D-170](../DECISIONS.md#d-170-게임-기획-묶음이-h-재고-범위를-통제하고-h에서-wi의-e-부족분을-유도한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-010` | 게임 기획 묶음이 H 재고 범위를 통제하고 H에서 WI의 E 부족분을 유도한다 | `TitleReviewed` |
| [D-171](../DECISIONS.md#d-171-nature-위협-대응의-예상-플레이-네-동사를-독립-e1-wi-계약으로-고정한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-011` | Nature 위협 대응의 예상 플레이 네 동사를 독립 E1 WI 계약으로 고정한다 | `TitleReviewed` |
| [D-172](../DECISIONS.md#d-172-nature-h-설계는-반복-플레이-폐루프와-계획-용량을-먼저-봉인한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-012` | Nature H 설계는 반복 플레이 폐루프와 계획 용량을 먼저 봉인한다 | `TitleReviewed` |
| [D-173](../DECISIONS.md#d-173-자연권-위협-관찰은-기존-결정작업효과-원장을-재사용해-e3로-완결한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-013` | 자연권 위협 관찰은 기존 결정·작업·효과 원장을 재사용해 E3로 완결한다 | `TitleReviewed` |
| [D-174](../DECISIONS.md#d-174-자연권-긴급-후퇴는-선행-위협-근거와-경로-예약으로-e3를-닫는다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-014` | 자연권 긴급 후퇴는 선행 위협 근거와 경로 예약으로 E3를 닫는다 | `TitleReviewed` |
| [D-175](../DECISIONS.md#d-175-자연권-복원은-관찰된-원인-전체-해결-후에만-자재를-소비한다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-015` | 자연권 복원은 관찰된 원인 전체 해결 후에만 자재를 소비한다 | `TitleReviewed` |
| [D-176](../DECISIONS.md#d-176-파티-회복은-후퇴-또는-복원-효과-후-탐색을-재개하는-e3-행위다) | `D-GAMEPLAY-NATURE-THREAT-RECOVERY-016` | 파티 회복은 후퇴 또는 복원 효과 후 탐색을 재개하는 E3 행위다 | `TitleReviewed` |
| [D-177](../DECISIONS.md#d-177-nature-팩-중심-상시-체류-세계를-심리-영역으로-정의하고-두-발전소-인과를-둔다) | `D-WORLD-H-LH-ASSET-COMPOSITION-001` | Nature 팩 중심 상시 체류 세계를 심리 영역으로 정의하고 두 발전소 인과를 둔다 | `TitleReviewed` |
| [D-178](../DECISIONS.md#d-178-construction-팩은-공통-조립층이며-두-발전소는-기존-nature-h2h3를-확장한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-002` | Construction 팩은 공통 조립층이며 두 발전소는 기존 Nature H2·H3를 확장한다 | `TitleReviewed` |
| [D-179](../DECISIONS.md#d-179-다섯-synty-팩은-h-승격-전에-전수-기술-대장과-의미-자산군으로-관리한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-003` | 다섯 Synty 팩은 H 승격 전에 전수 기술 대장과 의미 자산군으로 관리한다 | `TitleReviewed` |
| [D-180](../DECISIONS.md#d-180-휴대폰-공간-조립-검토는-주차-후-후보-선별이며-최종-scene-승인이-아니다) | `D-WORLD-H-LH-ASSET-COMPOSITION-004` | 휴대폰 공간 조립 검토는 주차 후 후보 선별이며 최종 Scene 승인이 아니다 | `TitleReviewed` |
| [D-181](../DECISIONS.md#d-181-synty-web-검토-v2는-불변-촬영-영수증과-부모-bundle-계보로-재촬영을-닫는다) | `D-WORLD-H-LH-ASSET-COMPOSITION-005` | Synty Web 검토 v2는 불변 촬영 영수증과 부모 bundle 계보로 재촬영을 닫는다 | `TitleReviewed` |
| [D-182](../DECISIONS.md#d-182-unity-산출물-검토-webapp은-일반-업무-webapp과-물리-프로젝트를-분리한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-006` | Unity 산출물 검토 WebApp은 일반 업무 WebApp과 물리 프로젝트를 분리한다 | `TitleReviewed` |
| [D-183](../DECISIONS.md#d-183-synty-검토-폐루프는-저장화면-상태전송촬영-조립-책임을-분리한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-007` | Synty 검토 폐루프는 저장·화면 상태·전송·촬영 조립 책임을 분리한다 | `TitleReviewed` |
| [D-184](../DECISIONS.md#d-184-unity-산출물-검토-앱은-기존-azure-vm에서-별도-경로배포-묶음으로-운영한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-008` | Unity 산출물 검토 앱은 기존 Azure VM에서 별도 경로·배포 묶음으로 운영한다 | `TitleReviewed` |
| [D-185](../DECISIONS.md#d-185-h1h4-unity-조합물은-선택-root-촬영-영수증으로-모바일-검토하되-공간-권위를-만들지-않는다) | `D-WORLD-H-LH-ASSET-COMPOSITION-009` | H1~H4 Unity 조합물은 선택 Root 촬영 영수증으로 모바일 검토하되 공간 권위를 만들지 않는다 | `TitleReviewed` |
| [D-186](../DECISIONS.md#d-186-unity-산출물-검토는-역할별-vm과-분리한-무료-대상-vm의-최소-docker-스택으로-운영한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-010` | Unity 산출물 검토는 역할별 VM과 분리한 무료 대상 VM의 최소 Docker 스택으로 운영한다 | `TitleReviewed` |
| [D-187](../DECISIONS.md#d-187-h1은-인지-부품이고-h2는-첫-공간-조합-판단-단위다) | `D-WORLD-H-LH-ASSET-COMPOSITION-011` | H1은 인지 부품이고 H2는 첫 공간 조합 판단 단위다 | `TitleReviewed` |
| [D-188](../DECISIONS.md#d-188-h-조립게임플레이-추적e-증거완주-상태는-독립-축으로-관리한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-012` | H 조립·게임플레이 추적·E 증거·완주 상태는 독립 축으로 관리한다 | `TitleReviewed` |
| [D-189](../DECISIONS.md#d-189-h2h3와-이론-e5-공간-생산은-사람-검토를-차단-관문으로-사용하지-않는다) | `D-WORLD-H-LH-ASSET-COMPOSITION-013` | H2·H3와 이론 E5 공간 생산은 사람 검토를 차단 관문으로 사용하지 않는다 | `TitleReviewed` |
| [D-190](../DECISIONS.md#d-190-이론-공간-공급과-실제-플레이-공간-완성-사이에-독립-완료-상태를-둔다) | `D-WORLD-H-LH-ASSET-COMPOSITION-014` | 이론 공간 공급과 실제 플레이 공간 완성 사이에 독립 완료 상태를 둔다 | `TitleReviewed` |
| [D-191](../DECISIONS.md#d-191-h2h3는-stableid를-보존하고-팩-주도-패턴-이름을-별도로-가진다) | `D-WORLD-H-LH-ASSET-COMPOSITION-015` | H2·H3는 StableId를 보존하고 팩 주도 패턴 이름을 별도로 가진다 | `TitleReviewed` |
| [D-192](../DECISIONS.md#d-192-팩-단독-h2는-팩-내부-h3보다-먼저-게임-기획-areaset에-대기-결속한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-016` | 팩 단독 H2는 팩 내부 H3보다 먼저 게임 기획 AreaSet에 대기 결속한다 | `TitleReviewed` |
| [D-193](../DECISIONS.md#d-193-팩-내부-h3가-준비되면-areaset의-임시-h2-직접-참조를-h3-계보로-대체한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-017` | 팩 내부 H3가 준비되면 AreaSet의 임시 H2 직접 참조를 H3 계보로 대체한다 | `TitleReviewed` |
| [D-194](../DECISIONS.md#d-194-naturetown-혼합-경관은-선택-계보로-만들고-실제-e5-배치를-자동-생성하지-않는다) | `D-WORLD-H-LH-ASSET-COMPOSITION-018` | Nature–Town 혼합 경관은 선택 계보로 만들고 실제 E5 배치를 자동 생성하지 않는다 | `TitleReviewed` |
| [D-195](../DECISIONS.md#d-195-h2는-배치-가능한-물리-블록이고-h3는-배치-가능한-구역-조립안이다) | `D-WORLD-H-LH-ASSET-COMPOSITION-019` | H2는 배치 가능한 물리 블록이고 H3는 배치 가능한 구역 조립안이다 | `TitleReviewed` |
| [D-196](../DECISIONS.md#d-196-실제-e5는-네-전용-areaset과-하나의-network로-결속하고-모든-이론-h3의-처리를-명시한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-020` | 실제 E5는 네 전용 AreaSet과 하나의 Network로 결속하고 모든 이론 H3의 처리를 명시한다 | `TitleReviewed` |
| [D-197](../DECISIONS.md#d-197-지역-위협회복과-카드-효과는-서버-권위-v5-인과-원장으로-계산한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-021` | 지역 위협·회복과 카드 효과는 서버 권위 v5 인과 원장으로 계산한다 | `TitleReviewed` |
| [D-198](../DECISIONS.md#d-198-h-공간-공장은-모든-계층에서-명시적-연결점-의미와-방향성-흐름을-재귀-검증한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-022` | H 공간 공장은 모든 계층에서 명시적 연결점 의미와 방향성 흐름을 재귀 검증한다 | `TitleReviewed` |
| [D-199](../DECISIONS.md#d-199-lh는-스트리밍-범위와-셀-내용을-분리하고-l과-h를-조회-관계로만-연결한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-023` | LH는 스트리밍 범위와 셀 내용을 분리하고 L과 H를 조회 관계로만 연결한다 | `TitleReviewed` |
| [D-200](../DECISIONS.md#d-200-h2h3-재고는-팩별-수량이-아니라-게임플레이-공간-수요로-증산한다) | `D-WORLD-H-LH-ASSET-COMPOSITION-024` | H2·H3 재고는 팩별 수량이 아니라 게임플레이 공간 수요로 증산한다 | `TitleReviewed` |
| [D-201](../DECISIONS.md#d-201-e8은-npc-생활세계의-자율-행동-폐루프를-검증한다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-001` | E8은 NPC 생활세계의 자율 행동 폐루프를 검증한다 | `TitleReviewed` |
| [D-202](../DECISIONS.md#d-202-h5는-권위-상대-공간이며-e6는-선택형-현실-결속이다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-002` | H5는 권위 상대 공간이며 E6는 선택형 현실 결속이다 | `TitleReviewed` |
| [D-203](../DECISIONS.md#d-203-dem도로는-공통-필수-자료가-아니라-현실-결속-프로필의-선택-요구다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-003` | DEM·도로는 공통 필수 자료가 아니라 현실 결속 프로필의 선택 요구다 | `TitleReviewed` |
| [D-204](../DECISIONS.md#d-204-전투-맵은-h5의-확대가-아니라-지역-문맥에서-결정적으로-파생한-독립-공간이다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-004` | 전투 맵은 H5의 확대가 아니라 지역 문맥에서 결정적으로 파생한 독립 공간이다 | `TitleReviewed` |
| [D-205](../DECISIONS.md#d-205-h5-통합-생활세계는-정적-장소와-session-가변-시설을-결합한-wi-폐루프로-구현한다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-005` | H5 통합 생활세계는 정적 장소와 Session 가변 시설을 결합한 WI 폐루프로 구현한다 | `TitleReviewed` |
| [D-206](../DECISIONS.md#d-206-소규모-현장-전투와-대규모-파생-전장은-같은-서버-전투-원장을-사용한다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-006` | 소규모 현장 전투와 대규모 파생 전장은 같은 서버 전투 원장을 사용한다 | `TitleReviewed` |
| [D-207](../DECISIONS.md#d-207-e6는-areaset-정밀-몰입-성숙도이며-gis-결속은-독립-선택-축이다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-007` | E6는 AreaSet 정밀 몰입 성숙도이며 GIS 결속은 독립 선택 축이다 | `TitleReviewed` |
| [D-208](../DECISIONS.md#d-208-카드-서랍은-의미-투영을-통합하되-원장권위실행-책임을-통합하지-않는다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-008` | 카드 서랍은 의미 투영을 통합하되 원장·권위·실행 책임을 통합하지 않는다 | `TitleReviewed` |
| [D-209](../DECISIONS.md#d-209-첫-e7-플레이-폐루프는-네이처-탐험접근-조우현장-대응탐험-복귀다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-009` | 첫 E7 플레이 폐루프는 네이처 탐험·접근 조우·현장 대응·탐험 복귀다 | `TitleReviewed` |
| [D-210](../DECISIONS.md#d-210-첫-네이처-실제-공간은-기존-생활핵조우방어-h3를-완전한-복귀-폐루프로-조립한다) | `D-EVIDENCE-WORLD-PLAY-LOOPS-010` | 첫 네이처 실제 공간은 기존 생활핵·조우·방어 H3를 완전한 복귀 폐루프로 조립한다 | `TitleReviewed` |
| [D-211](../DECISIONS.md#d-211-바보는-항상-활성인-타로-여정-루트이고-현재-메이저-아르카나는-그-아래의-가변-문맥이다) | `D-GAMEPLAY-TAROT-REALITY-SPATIAL-001` | 바보는 항상 활성인 타로 여정 루트이고 현재 메이저 아르카나는 그 아래의 가변 문맥이다 | `TitleReviewed` |
| [D-212](../DECISIONS.md#d-212-e6-현실-자료는-세션-시작-상태-사본으로-동결하고-unity에는-게임-현상을-우선-표현한다) | `D-GAMEPLAY-TAROT-REALITY-SPATIAL-002` | E6 현실 자료는 세션 시작 상태 사본으로 동결하고 Unity에는 게임 현상을 우선 표현한다 | `TitleReviewed` |
| [D-213](../DECISIONS.md#d-213-areaset-구성-패턴은-h2h3-재고를-역할-슬롯으로-조립하는-위치-독립-제작-계약이다) | `D-GAMEPLAY-TAROT-REALITY-SPATIAL-003` | AreaSet 구성 패턴은 H2·H3 재고를 역할 슬롯으로 조립하는 위치 독립 제작 계약이다 | `TitleReviewed` |
| [D-214](../DECISIONS.md#d-214-farmhubcity-독립-우선-경로-연결-후속) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-001` | Farm·Hub·City 독립 우선, 경로 연결 후속 | `TitleReviewed` |
| [D-215](../DECISIONS.md#d-215-e는-증거-성숙도이고-g는-다음-e로-올리는-관리-체계다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-002` | E는 증거 성숙도이고 G는 다음 E로 올리는 관리 체계다 | `TitleReviewed` |
| [D-216](../DECISIONS.md#d-216-권위-지도-묶음은-플레이-목적부터-실행-증거까지-잇는-navigation-기준이다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-003` | 권위 지도 묶음은 플레이 목적부터 실행 증거까지 잇는 navigation 기준이다 | `TitleReviewed` |
| [D-217](../DECISIONS.md#d-217-배치-통제-계층은-h-공간-의미와-분리하고-player-실측-크기를-기준으로-한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-004` | 배치 통제 계층은 H 공간 의미와 분리하고 Player 실측 크기를 기준으로 한다 | `TitleReviewed` |
| [D-218](../DECISIONS.md#d-218-게임-개발은-현재-목표에서-증거와-다음-판단까지-같은-업무-순서를-사용한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-005` | 게임 개발은 현재 목표에서 증거와 다음 판단까지 같은 업무 순서를 사용한다 | `TitleReviewed` |
| [D-219](../DECISIONS.md#d-219-nature는-생존-생활거점을-1차-플레이로-두고-심리-회복을-그-결과에-결합한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-006` | Nature는 생존 생활거점을 1차 플레이로 두고 심리 회복을 그 결과에 결합한다 | `TitleReviewed` |
| [D-220](../DECISIONS.md#d-220-worldtick권위-실시간표현-실시간battletick을-분리한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-007` | WorldTick·권위 실시간·표현 실시간·BattleTick을 분리한다 | `TitleReviewed` |
| [D-221](../DECISIONS.md#d-221-simulation-core는-게임-세계이고-server는-hosted-host다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-008` | Simulation Core는 게임 세계이고 Server는 Hosted Host다 | `TitleReviewed` |
| [D-222](../DECISIONS.md#d-222-의미-있는-게임-작업은-e9-목표부터-하향-분해하고-e1부터-상향-검증한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-009` | 의미 있는 게임 작업은 E9 목표부터 하향 분해하고 E1부터 상향 검증한다 | `TitleReviewed` |
| [D-223](../DECISIONS.md#d-223-e6는-e5-세계를-e7-전에-정제하는-관문이다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-010` | E6는 E5 세계를 E7 전에 정제하는 관문이다 | `TitleReviewed` |
| [D-224](../DECISIONS.md#d-224-기존-통합-이력은-보존하고-새-작업은-운영simulationunity-책임으로-분리한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-011` | 기존 통합 이력은 보존하고 새 작업은 운영·Simulation·Unity 책임으로 분리한다 | `TitleReviewed` |
| [D-225](../DECISIONS.md#d-225-게임-작업은-플레이어-선택-폐루프를-ewih-전-단계의-공통-관점으로-사용한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-012` | 게임 작업은 플레이어 선택 폐루프를 E·WI·H 전 단계의 공통 관점으로 사용한다 | `TitleReviewed` |
| [D-226](../DECISIONS.md#d-226-지역-사건nature-위협전투-결과지역-발전을-독립-모듈로-잇는다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-013` | 지역 사건·Nature 위협·전투 결과·지역 발전을 독립 모듈로 잇는다 | `TitleReviewed` |
| [D-227](../DECISIONS.md#d-227-최근-개발-철학의-반복-경계를-프로젝트-불변-골격으로-고정한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-014` | 최근 개발 철학의 반복 경계를 프로젝트 불변 골격으로 고정한다 | `TitleReviewed` |
| [D-228](../DECISIONS.md#d-228-게임-개발-기준-문서는-질문별-단일-책임을-갖고-대체-경로는-호환-안내로-남긴다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-015` | 게임 개발 기준 문서는 질문별 단일 책임을 갖고 대체 경로는 호환 안내로 남긴다 | `TitleReviewed` |
| [D-229](../DECISIONS.md#d-229-e9와-e1-사이는-한-번-통과하는-파이프라인이-아니라-안정될-때까지-반복-왕복한다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-016` | E9와 E1 사이는 한 번 통과하는 파이프라인이 아니라 안정될 때까지 반복 왕복한다 | `TitleReviewed` |
| [D-230](../DECISIONS.md#d-230-게임-코드에는-e-증거-상태가-아니라-e-검토-책임을-메타데이터로-남긴다) | `D-ARCHITECTURE-PLAYABLE-DEVELOPMENT-017` | 게임 코드에는 E 증거 상태가 아니라 E 검토 책임을 메타데이터로 남긴다 | `TitleReviewed` |
| [D-231](../DECISIONS.md#d-231-플레이어가-확정하는-nature-생존-행동은-정식-wi로-관리한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-001` | 플레이어가 확정하는 Nature 생존 행동은 정식 WI로 관리한다 | `TitleReviewed` |
| [D-232](../DECISIONS.md#d-232-e-성숙도의-주어는-wi이며-공간은-조건부-발현-증거다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-002` | E 성숙도의 주어는 WI이며 공간은 조건부 발현 증거다 | `TitleReviewed` |
| [D-233](../DECISIONS.md#d-233-진행-작업-취소는-예약을-반환하는-독립-wi이며-원래-공간-문맥을-이어받는다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-003` | 진행 작업 취소는 예약을 반환하는 독립 WI이며 원래 공간 문맥을 이어받는다 | `TitleReviewed` |
| [D-234](../DECISIONS.md#d-234-playableloop와-evidencepackage를-wihe-사이의-공식-연결-객체로-사용한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-004` | PlayableLoop와 EvidencePackage를 WI·H·E 사이의 공식 연결 객체로 사용한다 | `TitleReviewed` |
| [D-235](../DECISIONS.md#d-235-메이저-아르카나-방향은-카드가-아니라-활성화-인스턴스에-귀속한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-005` | 메이저 아르카나 방향은 카드가 아니라 활성화 인스턴스에 귀속한다 | `TitleReviewed` |
| [D-236](../DECISIONS.md#d-236-marketplace-상품-관측은-item-효과-근거이고-synty는-범주형-외형이다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-006` | Marketplace 상품 관측은 Item 효과 근거이고 Synty는 범주형 외형이다 | `TitleReviewed` |
| [D-237](../DECISIONS.md#d-237-현장-전투-참여-방식은-관찰-운영과-직접-개입을-같은-권위-원장에서-잠근다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-007` | 현장 전투 참여 방식은 관찰 운영과 직접 개입을 같은 권위 원장에서 잠근다 | `TitleReviewed` |
| [D-238](../DECISIONS.md#d-238-영역-건물-발전은-다섯-독립-누적-트리로-관리하고-nature부터-실제-구현한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-008` | 영역 건물 발전은 다섯 독립 누적 트리로 관리하고 Nature부터 실제 구현한다 | `TitleReviewed` |
| [D-239](../DECISIONS.md#d-239-플레이-폐루프는-coreextension-자식과-영역세계-집계로-완결-판정한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-009` | 플레이 폐루프는 Core·Extension 자식과 영역·세계 집계로 완결 판정한다 | `TitleReviewed` |
| [D-240](../DECISIONS.md#d-240-플레이어-활동은-역할이-아니라-현장-원정영역-운영영역-제조의-선택-가능한-세-갈래로-분류한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-010` | 플레이어 활동은 역할이 아니라 현장 원정·영역 운영·영역 제조의 선택 가능한 세 갈래로 분류한다 | `TitleReviewed` |
| [D-241](../DECISIONS.md#d-241-운영-유래-반복-wi는-npc-루틴이고-플레이어는-정책과-예외를-통제한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-011` | 운영 유래 반복 WI는 NPC 루틴이고 플레이어는 정책과 예외를 통제한다 | `TitleReviewed` |
| [D-242](../DECISIONS.md#d-242-배치-통제의-주-성립-축은-h1에서-h4-준비도로-올라간다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-012` | 배치 통제의 주 성립 축은 H1에서 H4 준비도로 올라간다 | `TitleReviewed` |
| [D-243](../DECISIONS.md#d-243-발산과-수렴은-수치-균형이-아니라-양방향-플레이-인계로-조화를-판정한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-013` | 발산과 수렴은 수치 균형이 아니라 양방향 플레이 인계로 조화를 판정한다 | `TitleReviewed` |
| [D-244](../DECISIONS.md#d-244-wi는-한국어-기능명을-먼저-표시하고-안정-고유-식별자는-보조-표기로-유지한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-014` | WI는 한국어 기능명을 먼저 표시하고 안정 고유 식별자는 보조 표기로 유지한다 | `TitleReviewed` |
| [D-245](../DECISIONS.md#d-245-wi는-한-의도와-하나의-주요-권위-결과만-소유하고-절차는-별도-흐름이-조립한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-015` | WI는 한 의도와 하나의 주요 권위 결과만 소유하고 절차는 별도 흐름이 조립한다 | `TitleReviewed` |
| [D-246](../DECISIONS.md#d-246-wi-음양-사분면은-행동-목적과-실제-수행-주체를-직교-결합한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-016` | WI 음양 사분면은 행동 목적과 실제 수행 주체를 직교 결합한다 | `TitleReviewed` |
| [D-247](../DECISIONS.md#d-247-배치-결과는-플레이어-감각-표현축에서-교차-검증한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-017` | 배치 결과는 플레이어 감각 표현축에서 교차 검증한다 | `TitleReviewed` |
| [D-248](../DECISIONS.md#d-248-codex-장기-goal은-playableunit-하나를-소유하고-wi-wip-1로-진행한다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-018` | Codex 장기 Goal은 PlayableUnit 하나를 소유하고 WI WIP 1로 진행한다 | `TitleReviewed` |
| [D-249](../DECISIONS.md#d-249-거점-성찰은-승인-자료를-읽는-플레이어-선택이며-시청-보상이-아니다) | `D-GAMEPLAY-WI-LOOP-PROGRESSION-019` | 거점 성찰은 승인 자료를 읽는 플레이어 선택이며 시청 보상이 아니다 | `TitleReviewed` |
| [D-250](../DECISIONS.md#d-250-플레이-폐루프의-논리와-표현-성숙도를-분리하고-낮은-단계로-통합-판정한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-001` | 플레이 폐루프의 논리와 표현 성숙도를 분리하고 낮은 단계로 통합 판정한다 | `TitleReviewed` |
| [D-251](../DECISIONS.md#d-251-표현-e4e7-승격은-공통-검증-모듈과-기능별-조건-모듈을-통과한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-002` | 표현 E4~E7 승격은 공통 검증 모듈과 기능별 조건 모듈을 통과한다 | `TitleReviewed` |
| [D-252](../DECISIONS.md#d-252-lh의-335599는-고정-계층이-아니라-기본-동적-맵-창-프로필이다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-003` | LH의 3×3·5×5·9×9는 고정 계층이 아니라 기본 동적 맵 창 프로필이다 | `TitleReviewed` |
| [D-253](../DECISIONS.md#d-253-동적-셀-활성화-전에-객체의-표면간격가시-하단을-별도-관문으로-검증한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-004` | 동적 셀 활성화 전에 객체의 표면·간격·가시 하단을 별도 관문으로 검증한다 | `TitleReviewed` |
| [D-254](../DECISIONS.md#d-254-sky-engine은-세계-공통-대기-권위와-카메라-전역-표현을-잇는다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-005` | Sky Engine은 세계 공통 대기 권위와 카메라 전역 표현을 잇는다 | `TitleReviewed` |
| [D-255](../DECISIONS.md#d-255-lh-불규칙-지형은-교체-가능한-표면-상태-사본으로-조립한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-006` | LH 불규칙 지형은 교체 가능한 표면 상태 사본으로 조립한다 | `TitleReviewed` |
| [D-256](../DECISIONS.md#d-256-lh는-지면셀을-준비하고-sky-뒤-실외실내-배치엔진이-표현을-조립한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-007` | LH는 지면·셀을 준비하고 Sky 뒤 실외·실내 배치엔진이 표현을 조립한다 | `TitleReviewed` |
| [D-257](../DECISIONS.md#d-257-엔진-상호작용은-logicpresentation을-같은-wi-명령으로-묶는-통합-관문이다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-008` | 엔진 상호작용은 Logic·Presentation을 같은 WI 명령으로 묶는 통합 관문이다 | `TitleReviewed` |
| [D-258](../DECISIONS.md#d-258-synty-표현은-abc-완전성이-아니라-playableunit의-플레이-순간으로-모듈화한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-009` | Synty 표현은 A/B/C 완전성이 아니라 PlayableUnit의 플레이 순간으로 모듈화한다 | `TitleReviewed` |
| [D-259](../DECISIONS.md#d-259-synty-팩-출처와-게임-기능-모듈을-분리한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-010` | Synty 팩 출처와 게임 기능 모듈을 분리한다 | `TitleReviewed` |
| [D-260](../DECISIONS.md#d-260-playableunit-수직-성숙도는-e7에서-끝나고-e8e10은-수평-증거로-판정한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-011` | PlayableUnit 수직 성숙도는 E7에서 끝나고 E8~E10은 수평 증거로 판정한다 | `TitleReviewed` |
| [D-261](../DECISIONS.md#d-261-물품-획득과-장착을-보편-wi로-분리하고-능력은-장착-상태에서-파생한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-012` | 물품 획득과 장착을 보편 WI로 분리하고 능력은 장착 상태에서 파생한다 | `TitleReviewed` |
| [D-262](../DECISIONS.md#d-262-자연-방향광은-urp-lit와-표현-검증-기록으로-강화한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-013` | 자연 방향광은 URP Lit와 표현 검증 기록으로 강화한다 | `TitleReviewed` |
| [D-263](../DECISIONS.md#d-263-synty-자산-설계-분류는-한국어를-먼저-쓰고-stable-code를-보존한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-014` | Synty 자산 설계 분류는 한국어를 먼저 쓰고 Stable Code를 보존한다 | `TitleReviewed` |
| [D-264](../DECISIONS.md#d-264-e1e9는-판정-주체를-바꾸되-logicpresentation-왕복을-유지한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-015` | E1~E9는 판정 주체를 바꾸되 Logic·Presentation 왕복을 유지한다 | `TitleReviewed` |
| [D-265](../DECISIONS.md#d-265-e8-조화-묶음은-소유-areaaggregate의-core-일부를-선택할-수-있다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-016` | E8 조화 묶음은 소유 AreaAggregate의 Core 일부를 선택할 수 있다 | `TitleReviewed` |
| [D-266](../DECISIONS.md#d-266-e8은-개별-폐루프-안정-e9는-영역-조화와-사람-승인으로-판정한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-017` | E8은 개별 폐루프 안정, E9는 영역 조화와 사람 승인으로 판정한다 | `TitleReviewed` |
| [D-267](../DECISIONS.md#d-267-권위-행위-기록은-엔진과-분리하고-분야-성장은-효과-계보에서-파생한다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-018` | 권위 행위 기록은 엔진과 분리하고 분야 성장은 효과 계보에서 파생한다 | `TitleReviewed` |
| [D-268](../DECISIONS.md#d-268-행위-파이프라인은-e-logicpresentation과-e8e10의-공통-통합-관문이다) | `D-EVIDENCE-PRESENTATION-INTEGRATION-019` | 행위 파이프라인은 E Logic·Presentation과 E8~E10의 공통 통합 관문이다 | `TitleReviewed` |
| [D-269](../DECISIONS.md#d-269-주제-기획-승인은-새-playableloop-goal보다-앞선다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-001` | 주제 기획 승인은 새 PlayableLoop Goal보다 앞선다 | `TitleReviewed` |
| [D-270](../DECISIONS.md#d-270-집중-판정은-wi-task에-종속하고-명상은-횡단-성장축으로-둔다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-002` | 집중 판정은 WI Task에 종속하고 명상은 횡단 성장축으로 둔다 | `TitleReviewed` |
| [D-271](../DECISIONS.md#d-271-모든-플레이어-wi를-집중-profile로-분류하고-명상-성장은-행위-원장-계보에-결속한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-003` | 모든 플레이어 WI를 집중 Profile로 분류하고 명상 성장은 행위 원장 계보에 결속한다 | `TitleReviewed` |
| [D-272](../DECISIONS.md#d-272-solo를-유지하면서-공식-지속-세계와-서버-권위-비공개-협동방을-분리한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-004` | Solo를 유지하면서 공식 지속 세계와 서버 권위 비공개 협동방을 분리한다 | `TitleReviewed` |
| [D-273](../DECISIONS.md#d-273-전술-시점은-자기-캐릭터-선택이동과-카메라-탐색-입력을-분리한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-005` | 전술 시점은 자기 캐릭터 선택·이동과 카메라 탐색 입력을 분리한다 | `TitleReviewed` |
| [D-274](../DECISIONS.md#d-274-다섯-영역-위치를-h5-상대좌표로-고정하고-city는-예약-상태로-분리한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-006` | 다섯 영역 위치를 H5 상대좌표로 고정하고 City는 예약 상태로 분리한다 | `TitleReviewed` |
| [D-275](../DECISIONS.md#d-275-플레이-폐루프-기획과-개발은-승인-기획서를-저장소-인계면으로-사용한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-007` | 플레이 폐루프 기획과 개발은 승인 기획서를 저장소 인계면으로 사용한다 | `TitleReviewed` |
| [D-276](../DECISIONS.md#d-276-구체-설계는-전문-심화-연구로-분기한-뒤-playableloop에-재결속한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-008` | 구체 설계는 전문 심화 연구로 분기한 뒤 PlayableLoop에 재결속한다 | `TitleReviewed` |
| [D-277](../DECISIONS.md#d-277-짧은-정차대기-문답을-playableloop-기획의-공식-draft-절차로-사용한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-009` | 짧은 정차·대기 문답을 PlayableLoop 기획의 공식 Draft 절차로 사용한다 | `TitleReviewed` |
| [D-278](../DECISIONS.md#d-278-명상은-실행-wi가-아닌-비실행-상위-wi군으로-구체-플레이어-행위를-묶는다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-010` | 명상은 실행 WI가 아닌 비실행 상위 WI군으로 구체 플레이어 행위를 묶는다 | `TitleReviewed` |
| [D-279](../DECISIONS.md#d-279-farm-작업-참여는-초기-solo-가능성과-후속-협력-성장을-공통-비실행-정책으로-연결한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-011` | Farm 작업 참여는 초기 Solo 가능성과 후속 협력 성장을 공통 비실행 정책으로 연결한다 | `TitleReviewed` |
| [D-280](../DECISIONS.md#d-280-presentation-e4는-적용-가능한-자산배치-후보를-e5-준비-인계로-남긴다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-012` | Presentation E4는 적용 가능한 자산·배치 후보를 E5 준비 인계로 남긴다 | `TitleReviewed` |
| [D-281](../DECISIONS.md#d-281-문답-질문은-주제깊이전체-번호를-함께-표시하고-깊이별-evidence-준비-전망을-제공한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-013` | 문답 질문은 주제·깊이·전체 번호를 함께 표시하고 깊이별 Evidence 준비 전망을 제공한다 | `TitleReviewed` |
| [D-282](../DECISIONS.md#d-282-신규-synty-환경애니메이션-팩은-wih-표현-원천으로-분리-결속한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-014` | 신규 Synty 환경·애니메이션 팩은 WI·H 표현 원천으로 분리 결속한다 | `TitleReviewed` |
| [D-283](../DECISIONS.md#d-283-wi-후보-등록은-상위-분류특화결과-투영과-실행-행동을-구별한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-015` | WI 후보 등록은 상위 분류·특화·결과 투영과 실행 행동을 구별한다 | `TitleReviewed` |
| [D-284](../DECISIONS.md#d-284-goalwi-고정-wip-상한을-없애고-실제-의존성과-변경-소유권으로-병렬-개발한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-016` | Goal·WI 고정 WIP 상한을 없애고 실제 의존성과 변경 소유권으로 병렬 개발한다 | `TitleReviewed` |
| [D-285](../DECISIONS.md#d-285-전문-작업-결과는-개발에서-통합검증한-뒤-기획으로-반환한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-017` | 전문 작업 결과는 개발에서 통합·검증한 뒤 기획으로 반환한다 | `TitleReviewed` |
| [D-286](../DECISIONS.md#d-286-기존-wi를-session배치저장에-연결하는-e5-전달을-우선한다) | `D-PLANNING-GOAL-INQUIRY-HANDOFF-018` | 기존 WI를 Session·배치·저장에 연결하는 E5 전달을 우선한다 | `TitleReviewed` |
| [D-287](../DECISIONS.md#d-287-행동-체력은-자연휴식물품으로-회복하고-성장에-따라-최대치를-늘린다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-001` | 행동 체력은 자연·휴식·물품으로 회복하고 성장에 따라 최대치를 늘린다 | `TitleReviewed` |
| [D-288](../DECISIONS.md#d-288-자연-체력-회복은-걷기대기-중-허용하고-노동질주전투-중-중단한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-002` | 자연 체력 회복은 걷기·대기 중 허용하고 노동·질주·전투 중 중단한다 | `TitleReviewed` |
| [D-289](../DECISIONS.md#d-289-제자리-휴식은-시설-없이-허용하고-안전한-오두막은-회복-효율을-높인다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-003` | 제자리 휴식은 시설 없이 허용하고 안전한 오두막은 회복 효율을 높인다 | `TitleReviewed` |
| [D-290](../DECISIONS.md#d-290-휴식은-이동작업피격-시-즉시-중단하고-이미-회복한-체력은-유지한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-004` | 휴식은 이동·작업·피격 시 즉시 중단하고 이미 회복한 체력은 유지한다 | `TitleReviewed` |
| [D-291](../DECISIONS.md#d-291-체력-회복-속도는-세-단계로-구분하고-farm-반복-시험으로-조정한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-005` | 체력 회복 속도는 세 단계로 구분하고 Farm 반복 시험으로 조정한다 | `TitleReviewed` |
| [D-292](../DECISIONS.md#d-292-휴식은-버튼-한-번으로-시작하고-유지-입력을-요구하지-않는다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-006` | 휴식은 버튼 한 번으로 시작하고 유지 입력을 요구하지 않는다 | `TitleReviewed` |
| [D-293](../DECISIONS.md#d-293-전투-중-휴식-시작을-금지하고-전투-종료-후-다시-허용한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-007` | 전투 중 휴식 시작을 금지하고 전투 종료 후 다시 허용한다 | `TitleReviewed` |
| [D-294](../DECISIONS.md#d-294-위협-접근은-오두막-회복-우대만-해제하고-실제-전투피격은-휴식을-종료한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-008` | 위협 접근은 오두막 회복 우대만 해제하고 실제 전투·피격은 휴식을 종료한다 | `TitleReviewed` |
| [D-295](../DECISIONS.md#d-295-자연-회복은-걷기대기-조건이-되면-별도-지연-없이-재개한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-009` | 자연 회복은 걷기·대기 조건이 되면 별도 지연 없이 재개한다 | `TitleReviewed` |
| [D-296](../DECISIONS.md#d-296-불은-휴식의-필수가-아니라-추가-회복-요소로-둔다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-010` | 불은 휴식의 필수가 아니라 추가 회복 요소로 둔다 | `TitleReviewed` |
| [D-297](../DECISIONS.md#d-297-야외-모닥불-곁의-휴식에도-불의-추가-회복을-적용한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-011` | 야외 모닥불 곁의 휴식에도 불의 추가 회복을 적용한다 | `TitleReviewed` |
| [D-298](../DECISIONS.md#d-298-view-캡처는-월드공간배치-담당이-실행하고-개발이-통합한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-012` | View 캡처는 월드·공간·배치 담당이 실행하고 개발이 통합한다 | `TitleReviewed` |
| [D-299](../DECISIONS.md#d-299-여러-불의-회복-효과는-합산하지-않고-가장-효과적인-하나만-적용한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-013` | 여러 불의 회복 효과는 합산하지 않고 가장 효과적인 하나만 적용한다 | `TitleReviewed` |
| [D-300](../DECISIONS.md#d-300-막힌-벽-너머의-열원은-휴식-추가-회복에-적용하지-않는다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-014` | 막힌 벽 너머의 열원은 휴식 추가 회복에 적용하지 않는다 | `TitleReviewed` |
| [D-301](../DECISIONS.md#d-301-휴식-중-불-효과와-벽-차단-이유를-작은-아이콘짧은-문구로-안내한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-015` | 휴식 중 불 효과와 벽 차단 이유를 작은 아이콘·짧은 문구로 안내한다 | `TitleReviewed` |
| [D-302](../DECISIONS.md#d-302-행동-체력이-가득-차면-휴식을-끝내고-다음-행동은-플레이어가-선택한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-016` | 행동 체력이 가득 차면 휴식을 끝내고 다음 행동은 플레이어가 선택한다 | `TitleReviewed` |
| [D-303](../DECISIONS.md#d-303-포션-한-개를-소비하면-행동-체력을-즉시-일정량-회복한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-017` | 포션 한 개를 소비하면 행동 체력을 즉시 일정량 회복한다 | `TitleReviewed` |
| [D-304](../DECISIONS.md#d-304-행동-체력-포션은-전투-중-사용을-허용하고-재사용-대기를-둔다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-018` | 행동 체력 포션은 전투 중 사용을 허용하고 재사용 대기를 둔다 | `TitleReviewed` |
| [D-305](../DECISIONS.md#d-305-체력이-가득-차면-포션-소비를-막고-부족할-때만-최대치까지-회복한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-019` | 체력이 가득 차면 포션 소비를 막고 부족할 때만 최대치까지 회복한다 | `TitleReviewed` |
| [D-306](../DECISIONS.md#d-306-포션-획득은-탐험제작거래의-선택-경로이며-고정-순서를-강제하지-않는다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-020` | 포션 획득은 탐험·제작·거래의 선택 경로이며 고정 순서를 강제하지 않는다 | `TitleReviewed` |
| [D-307](../DECISIONS.md#d-307-선택한-실제-활동으로-행동-체력이-성장하며-활동-다양성을-의무화하지-않는다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-021` | 선택한 실제 활동으로 행동 체력이 성장하며 활동 다양성을 의무화하지 않는다 | `TitleReviewed` |
| [D-308](../DECISIONS.md#d-308-성장으로-최대-체력이-늘어나면-현재-체력도-증가분만큼-보충한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-022` | 성장으로 최대 체력이 늘어나면 현재 체력도 증가분만큼 보충한다 | `TitleReviewed` |
| [D-309](../DECISIONS.md#d-309-활동-경험에-따른-최대치-자동-성장에-체력과-마나를-포함한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-023` | 활동 경험에 따른 최대치 자동 성장에 체력과 마나를 포함한다 | `TitleReviewed` |
| [D-310](../DECISIONS.md#d-310-마나-최대치는-명상과-집중한-일상-행동으로도-성장한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-024` | 마나 최대치는 명상과 집중한 일상 행동으로도 성장한다 | `TitleReviewed` |
| [D-311](../DECISIONS.md#d-311-마나-성장-시-최대치-증가분만-현재-마나에-보충한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-025` | 마나 성장 시 최대치 증가분만 현재 마나에 보충한다 | `TitleReviewed` |
| [D-312](../DECISIONS.md#d-312-마나는-자연-회복되고-명상-중에는-더-빠르게-회복된다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-026` | 마나는 자연 회복되고 명상 중에는 더 빠르게 회복된다 | `TitleReviewed` |
| [D-313](../DECISIONS.md#d-313-전투-중-마나-자연-회복은-유지하고-명상-회복은-제한한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-027` | 전투 중 마나 자연 회복은 유지하고 명상 회복은 제한한다 | `TitleReviewed` |
| [D-314](../DECISIONS.md#d-314-명상은-버튼-한-번으로-시작하고-행동피격으로-종료한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-028` | 명상은 버튼 한 번으로 시작하고 행동·피격으로 종료한다 | `TitleReviewed` |
| [D-315](../DECISIONS.md#d-315-마나가-가득-차도-명상은-유지하고-짧게-알린다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-029` | 마나가 가득 차도 명상은 유지하고 짧게 알린다 | `TitleReviewed` |
| [D-316](../DECISIONS.md#d-316-명상-중-같은-버튼으로-명상을-끝낸다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-030` | 명상 중 같은 버튼으로 명상을 끝낸다 | `TitleReviewed` |
| [D-317](../DECISIONS.md#d-317-회복-상태는-작게-표시하고-속도와-효과는-상세에서-보여준다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-031` | 회복 상태는 작게 표시하고 속도와 효과는 상세에서 보여준다 | `TitleReviewed` |
| [D-318](../DECISIONS.md#d-318-행동-자원이-부족하면-부족량과-회복-방법을-안내한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-032` | 행동 자원이 부족하면 부족량과 회복 방법을 안내한다 | `TitleReviewed` |
| [D-319](../DECISIONS.md#d-319-행동-체력-자연-회복을-독립된-첫-실행-범위로-승인한다) | `D-GAMEPLAY-PLAYER-RECOVERY-RESOURCES-033` | 행동 체력 자연 회복을 독립된 첫 실행 범위로 승인한다 | `TitleReviewed` |
| [D-320](../DECISIONS.md#d-320-farm-건물-실측-크기를-수용하도록-부지를-넓힌다) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-001` | Farm 건물 실측 크기를 수용하도록 부지를 넓힌다 | `TitleReviewed` |
| [D-321](../DECISIONS.md#d-321-건설-취소-재료-반환을-난이도별로-구분한다) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-002` | 건설 취소 재료 반환을 난이도별로 구분한다 | `TitleReviewed` |
| [D-322](../DECISIONS.md#d-322-노말-건설-취소는-약85-재료-반환-방향으로-둔다) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-003` | 노말 건설 취소는 약85% 재료 반환 방향으로 둔다 | `TitleReviewed` |
| [D-323](../DECISIONS.md#d-323-미사용-재료와-실제-시공-사용분의-취소-반환을-구분한다) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-004` | 미사용 재료와 실제 시공 사용분의 취소 반환을 구분한다 | `TitleReviewed` |
| [D-324](../DECISIONS.md#d-324-건설-취소-확정-전에-보존회수손실-재료를-나눠-안내한다) | `D-GAMEPLAY-CONSTRUCTION-CANCEL-005` | 건설 취소 확정 전에 보존·회수·손실 재료를 나눠 안내한다 | `TitleReviewed` |
| [D-325](../DECISIONS.md#d-325-문답-원문을-보존하고-파일-기반-검색-색인으로-재개한다) | `D-PLANNING-INQUIRY-SEARCH-001` | 문답 원문을 보존하고 파일 기반 검색 색인으로 재개한다 | `TitleReviewed` |
| [D-326](../DECISIONS.md#d-326-첫-약초차-반복은-기존-냄비로-물을-운반하고-컵으로-마신다) | `D-GAMEPLAY-HERBAL-TEA-001` | 첫 약초차 반복은 기존 냄비로 물을 운반하고 컵으로 마신다 | `TitleReviewed` |
| [D-327](../DECISIONS.md#d-327-물-확보량은-고정-제작-1회분이-아니라-용기별-용량을-따른다) | `D-GAMEPLAY-HERBAL-TEA-002` | 물 확보량은 고정 제작 1회분이 아니라 용기별 용량을 따른다 | `TitleReviewed` |
| [D-328](../DECISIONS.md#d-328-물-보관-가능-여부와-용량을-구분한다) | `D-GAMEPLAY-HERBAL-TEA-003` | 물 보관 가능 여부와 용량을 구분한다 | `TitleReviewed` |
| [D-329](../DECISIONS.md#d-329-달이기-중-다른-행동을-허용하고-약초-포션의-수요-공급을-후속-확장한다) | `D-GAMEPLAY-HERBAL-TEA-004` | 달이기 중 다른 행동을 허용하고 약초 포션의 수요 공급을 후속 확장한다 | `TitleReviewed` |
| [D-330](../DECISIONS.md#d-330-자기-돌봄이-타인의-이로움으로-확장되는-자리이타를-기획-방향으로-둔다) | `D-GAMEPLAY-HERBAL-TEA-005` | 자기 돌봄이 타인의 이로움으로 확장되는 자리이타를 기획 방향으로 둔다 | `TitleReviewed` |
| [D-331](../DECISIONS.md#d-331-처방별-연속-가열돌봄-차이를-후반-전문-제작으로-확장한다) | `D-GAMEPLAY-HERBAL-TEA-006` | 처방별 연속 가열·돌봄 차이를 후반 전문 제작으로 확장한다 | `TitleReviewed` |
| [D-332](../DECISIONS.md#d-332-첫-약초차의-따뜻함-감정날숨-표현과-자유-행동을-유지한다) | `D-GAMEPLAY-HERBAL-TEA-007` | 첫 약초차의 따뜻함 감정·날숨 표현과 자유 행동을 유지한다 | `TitleReviewed` |
| [D-333](../DECISIONS.md#d-333-약초차탕에-마법-술식을-추가하는-물약-제작을-구분한다) | `D-GAMEPLAY-HERBAL-TEA-008` | 약초차·탕에 마법 술식을 추가하는 물약 제작을 구분한다 | `TitleReviewed` |
| [D-334](../DECISIONS.md#d-334-관심명상에서-얻는-이데아의-편린을-npc-인연-가능성과-연결한다) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-001` | 관심·명상에서 얻는 이데아의 편린을 NPC 인연 가능성과 연결한다 | `TitleReviewed` |
| [D-335](../DECISIONS.md#d-335-상태창형-ui에서-관심-분야-카드를-선택한다) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-002` | 상태창형 UI에서 관심 분야 카드를 선택한다 | `TitleReviewed` |
| [D-336](../DECISIONS.md#d-336-관심행동-기록을-근거로-방문-npc-후보를-결정한다) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-003` | 관심·행동 기록을 근거로 방문 NPC 후보를 결정한다 | `TitleReviewed` |
| [D-337](../DECISIONS.md#d-337-호출당-외부-비용-없는-로컬-llmrag-대화-지원-기반-설치를-승인한다) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-004` | 호출당 외부 비용 없는 로컬 LLM/RAG 대화 지원 기반 설치를 승인한다 | `TitleReviewed` |
| [D-338](../DECISIONS.md#d-338-방문-npc는-분야-화두를-꺼내고-관련-거래-기회를-제공한다) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-005` | 방문 NPC는 분야 화두를 꺼내고 관련 거래 기회를 제공한다 | `TitleReviewed` |
| [D-339](../DECISIONS.md#d-339-기획-답변을-기록한-뒤-다음-핵심-질문을-이어서-제시한다) | `D-GAMEPLAY-IDEA-NPC-INQUIRY-006` | 기획 답변을 기록한 뒤 다음 핵심 질문을 이어서 제시한다 | `TitleReviewed` |
| [D-340](../DECISIONS.md#d-340-npc-물품-외상을-도시-은행계좌-상환-경로와-연결한다) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-001` | NPC 물품 외상을 도시 은행·계좌 상환 경로와 연결한다 | `TitleReviewed` |
| [D-341](../DECISIONS.md#d-341-연체-영향을-유예계좌-정지신용상단-인지로-단계화한다) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-002` | 연체 영향을 유예·계좌 정지·신용·상단 인지로 단계화한다 | `TitleReviewed` |
| [D-342](../DECISIONS.md#d-342-계좌-거래-정지-중에도-입금채무-상환을-허용한다) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-003` | 계좌 거래 정지 중에도 입금·채무 상환을 허용한다 | `TitleReviewed` |
| [D-343](../DECISIONS.md#d-343-연체-해소-후-계좌-제한을-풀고-신용은-점진-회복한다) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-004` | 연체 해소 후 계좌 제한을 풀고 신용은 점진 회복한다 | `TitleReviewed` |
| [D-344](../DECISIONS.md#d-344-첫-약초차는-불-꺼짐-중-재료진척을-보존한다) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-005` | 첫 약초차는 불 꺼짐 중 재료·진척을 보존한다 | `TitleReviewed` |
| [D-345](../DECISIONS.md#d-345-후반-개척-지역의-선택형-멀티플레이에-거래-신용을-연계한다) | `D-GAMEPLAY-CREDIT-MULTIPLAYER-006` | 후반 개척 지역의 선택형 멀티플레이에 거래 신용을 연계한다 | `TitleReviewed` |
| [D-346](../DECISIONS.md#d-346-첫-약초차의-체온-회복과-질병-관련-효과를-시간적으로-분리한다) | `D-GAMEPLAY-HERBAL-CONTENT-001` | 첫 약초차의 체온 회복과 질병 관련 효과를 시간적으로 분리한다 | `TitleReviewed` |
| [D-347](../DECISIONS.md#d-347-같은-첫-약초차의-지속-효과는-중첩하지-않고-시간을-갱신한다) | `D-GAMEPLAY-HERBAL-CONTENT-002` | 같은 첫 약초차의 지속 효과는 중첩하지 않고 시간을 갱신한다 | `TitleReviewed` |
| [D-348](../DECISIONS.md#d-348-식은-첫-약초차의-질병-효과를-유지하고-재가열을-허용한다) | `D-GAMEPLAY-HERBAL-CONTENT-003` | 식은 첫 약초차의 질병 효과를 유지하고 재가열을 허용한다 | `TitleReviewed` |
| [D-349](../DECISIONS.md#d-349-마개-달린-휴대-용기에-약초차를-담아-탐험에-가져간다) | `D-GAMEPLAY-HERBAL-CONTENT-004` | 마개 달린 휴대 용기에 약초차를 담아 탐험에 가져간다 | `TitleReviewed` |
| [D-350](../DECISIONS.md#d-350-약초차-음용-후-빈-휴대-용기를-유지해-재사용한다) | `D-GAMEPLAY-HERBAL-CONTENT-005` | 약초차 음용 후 빈 휴대 용기를 유지해 재사용한다 | `TitleReviewed` |
| [D-351](../DECISIONS.md#d-351-다른-종류의-차로-교체하기-전에-기존-내용물을-비운다) | `D-GAMEPLAY-HERBAL-CONTENT-006` | 다른 종류의 차로 교체하기 전에 기존 내용물을 비운다 | `TitleReviewed` |
| [D-352](../DECISIONS.md#d-352-한-번-음용할-때-1회분만-소비하고-나머지를-보존한다) | `D-GAMEPLAY-HERBAL-CONTENT-007` | 한 번 음용할 때 1회분만 소비하고 나머지를 보존한다 | `TitleReviewed` |
| [D-353](../DECISIONS.md#d-353-질문-약-10개와-추천-답안을-묶어-검토승인한-뒤-개발에-인계한다) | `D-GAMEPLAY-HERBAL-CONTENT-008` | 질문 약 10개와 추천 답안을 묶어 검토·승인한 뒤 개발에 인계한다 | `TitleReviewed` |
| [D-354](../DECISIONS.md#d-354-보편-데우기-wi-아래-물식은-차-데우기를-적용-사례로-둔다) | `D-GAMEPLAY-HERBAL-CONTENT-009` | 보편 데우기 WI 아래 물·식은 차 데우기를 적용 사례로 둔다 | `TitleReviewed` |
| [D-355](../DECISIONS.md#d-355-기존-문답과-wi를-보편-행위적용-사례조합-관계로-정리한다) | `D-GAMEPLAY-HERBAL-CONTENT-010` | 기존 문답과 WI를 보편 행위·적용 사례·조합 관계로 정리한다 | `TitleReviewed` |
| [D-356](../DECISIONS.md#d-356-hb-01의-q368377-추천안을-전체-승인한다) | `D-GAMEPLAY-HERBAL-CONTENT-011` | HB-01의 Q368~377 추천안을 전체 승인한다 | `TitleReviewed` |
| [D-357](../DECISIONS.md#d-357-fb-01-농사-생활위임의-수정-답변과-나머지-추천안을-승인한다) | `D-GAMEPLAY-FARM-DELEGATION-001` | FB-01 농사 생활·위임의 수정 답변과 나머지 추천안을 승인한다 | `TitleReviewed` |
| [D-358](../DECISIONS.md#d-358-blender-제작은-애니메이션-전문-담당-플레이-기획은-기획-스레드에-둔다) | `D-PRESENTATION-ANIMATION-WORKFLOW-001` | Blender 제작은 애니메이션 전문 담당, 플레이 기획은 기획 스레드에 둔다 | `TitleReviewed` |
| [D-359](../DECISIONS.md#d-359-구매한-synty-캐릭터의-도끼-벌목-동작-하나를-제작한다) | `D-PRESENTATION-ANIMATION-WORKFLOW-002` | 구매한 Synty 캐릭터의 도끼 벌목 동작 하나를 제작한다 | `TitleReviewed` |
| [D-360](../DECISIONS.md#d-360-nature-기초-폐루프의-기획-문서-연결을-복원한다) | `D-PRESENTATION-ANIMATION-WORKFLOW-003` | Nature 기초 폐루프의 기획 문서 연결을 복원한다 | `TitleReviewed` |
| [D-361](../DECISIONS.md#d-361-기존-wi와-보유-애니메이션을-대조해-부족한-표현부터-제작한다) | `D-PRESENTATION-ANIMATION-WORKFLOW-004` | 기존 WI와 보유 애니메이션을 대조해 부족한 표현부터 제작한다 | `TitleReviewed` |
| [D-362](../DECISIONS.md#d-362-ls-01-경관-구성-q387396-추천안을-전체-채택한다) | `D-WORLD-LANDSCAPE-PLACEMENT-LH-001` | LS-01 경관 구성 Q387~396 추천안을 전체 채택한다 | `TitleReviewed` |
| [D-363](../DECISIONS.md#d-363-배치-엔진-고도화를-우선하고-lh는-공간-실행을-지원한다) | `D-WORLD-LANDSCAPE-PLACEMENT-LH-002` | 배치 엔진 고도화를 우선하고 LH는 공간 실행을 지원한다 | `TitleReviewed` |
| [D-364](../DECISIONS.md#d-364-simulation배치lh를-하나의-공간-실행-파이프라인에서-조율한다) | `D-WORLD-LANDSCAPE-PLACEMENT-LH-003` | Simulation·배치·LH를 하나의 공간 실행 파이프라인에서 조율한다 | `TitleReviewed` |
| [D-365](../DECISIONS.md#d-365-첫-농사-목표-시간과-선택형-집중-타이밍) | `D-GAMEPLAY-FOCUS-RESEARCH-001` | 첫 농사 목표 시간과 선택형 집중 타이밍 | `TitleReviewed` |
| [D-366](../DECISIONS.md#d-366-선택형-집중-실패-무손실과-자료-조사-전문-담당) | `D-GAMEPLAY-FOCUS-RESEARCH-002` | 선택형 집중 실패 무손실과 자료 조사 전문 담당 | `TitleReviewed` |
| [D-367](../DECISIONS.md#d-367-기존-가격-자료의-해석을-통한-게임-교역-기회) | `D-GAMEPLAY-TRADE-REALITY-001` | 기존 가격 자료의 해석을 통한 게임 교역 기회 | `TitleReviewed` |
| [D-368](../DECISIONS.md#d-368-교역-위험을-상단-보험과-직접-경로-개입으로-다룬다) | `D-GAMEPLAY-TRADE-REALITY-002` | 교역 위험을 상단 보험과 직접 경로 개입으로 다룬다 | `TitleReviewed` |
| [D-369](../DECISIONS.md#d-369-첫-보험-보장-범위와-현실에-닿는-운영-경험) | `D-GAMEPLAY-TRADE-REALITY-003` | 첫 보험 보장 범위와 현실에 닿는 운영 경험 | `TitleReviewed` |
| [D-370](../DECISIONS.md#d-370-작업-결과는-짧게-요약하고-상세는-선택해서-펼친다) | `D-GAMEPLAY-TRADE-REALITY-004` | 작업 결과는 짧게 요약하고 상세는 선택해서 펼친다 | `TitleReviewed` |
| [D-371](../DECISIONS.md#d-371-현실-자료에-연결된-게임-경험을-시장-통찰로-확장한다) | `D-GAMEPLAY-TRADE-REALITY-005` | 현실 자료에 연결된 게임 경험을 시장 통찰로 확장한다 | `TitleReviewed` |
| [D-372](../DECISIONS.md#d-372-게임의-재미를-우선하고-현실-자료는-선택해서-열람한다) | `D-GAMEPLAY-TRADE-REALITY-006` | 게임의 재미를 우선하고 현실 자료는 선택해서 열람한다 | `TitleReviewed` |
| [D-373](../DECISIONS.md#d-373-현실-자료-열람은-상품거래-결과-상세에서-진입한다) | `D-GAMEPLAY-TRADE-REALITY-007` | 현실 자료 열람은 상품·거래 결과 상세에서 진입한다 | `TitleReviewed` |
| [D-374](../DECISIONS.md#d-374-게임-상품을-먼저-만들고-현실-자료의-제공은-운영자가-검토한다) | `D-GAMEPLAY-TRADE-REALITY-008` | 게임 상품을 먼저 만들고 현실 자료의 제공은 운영자가 검토한다 | `TitleReviewed` |
| [D-375](../DECISIONS.md#d-375-승인된-개발을-야간-반복-검증으로-이어간다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-001` | 승인된 개발을 야간 반복 검증으로 이어간다 | `TitleReviewed` |
| [D-376](../DECISIONS.md#d-376-야간-보완에-월드-경계와-배경-연속성을-포함한다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-002` | 야간 보완에 월드 경계와 배경 연속성을 포함한다 | `TitleReviewed` |
| [D-377](../DECISIONS.md#d-377-전체-문답을-e5-세계-통합-queue로-정리해-가능한-묶음을-완성한다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-003` | 전체 문답을 E5 세계 통합 Queue로 정리해 가능한 묶음을 완성한다 | `TitleReviewed` |
| [D-378](../DECISIONS.md#d-378-화면의-부유-흰색-객체와-네모난-면-겹침을-정리한다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-004` | 화면의 부유 흰색 객체와 네모난 면 겹침을 정리한다 | `TitleReviewed` |
| [D-379](../DECISIONS.md#d-379-도끼-접촉-타격음과-완료-후-나무-넘어짐을-보완한다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-005` | 도끼 접촉 타격음과 완료 후 나무 넘어짐을 보완한다 | `TitleReviewed` |
| [D-380](../DECISIONS.md#d-380-보유-자산과-확정-기획을-대조해-애니메이션리깅-정밀화-계획을-먼저-만든다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-006` | 보유 자산과 확정 기획을 대조해 애니메이션·리깅 정밀화 계획을 먼저 만든다 | `TitleReviewed` |
| [D-381](../DECISIONS.md#d-381-벌목-개발이-장기-정체되면-농장약초의-준비된-플레이-구간으로-전환한다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-007` | 벌목 개발이 장기 정체되면 농장·약초의 준비된 플레이 구간으로 전환한다 | `TitleReviewed` |
| [D-382](../DECISIONS.md#d-382-야간-목표를-여러-분야의-시각-진척-비교로-넓힌다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-008` | 야간 목표를 여러 분야의 시각 진척 비교로 넓힌다 | `TitleReviewed` |
| [D-383](../DECISIONS.md#d-383-애니메이션-담당을-동작-품질과-시각-성과-중심으로-운영한다) | `D-OPERATIONS-OVERNIGHT-VISUAL-DEV-009` | 애니메이션 담당을 동작 품질과 시각 성과 중심으로 운영한다 | `TitleReviewed` |
| [D-384](../DECISIONS.md#d-384-프로젝트-표시명과-github-저장소를-mirror거울로-변경한다) | `D-PLANNING-PROJECT-IDENTITY-001` | 프로젝트 표시명과 GitHub 저장소를 Mirror거울로 변경한다 | `TitleReviewed` |
| [D-385](../DECISIONS.md#d-385-좁은-목의-병-대신-조리-냄비로-읽히는-보유-prefab을-적용한다) | `D-PRESENTATION-HERBAL-PROP-001` | 좁은 목의 병 대신 조리 냄비로 읽히는 보유 Prefab을 적용한다 | `TitleReviewed` |
| [D-386](../DECISIONS.md#d-386-presentation-e단계에-최소-공통-모듈과-실제-구현증거를-연결한다) | `D-EVIDENCE-PRESENTATION-E4-E5-001` | Presentation E단계에 최소 공통 모듈과 실제 구현·증거를 연결한다 | `TitleReviewed` |
| [D-387](../DECISIONS.md#d-387-보유-synty-자산-조사를-presentation-e4의-명시적-준비-과정으로-둔다) | `D-EVIDENCE-PRESENTATION-E4-E5-002` | 보유 Synty 자산 조사를 Presentation E4의 명시적 준비 과정으로 둔다 | `TitleReviewed` |
| [D-388](../DECISIONS.md#d-388-논리가-선행한-기존-기능에도-자산-조사와-표현-준비를-적용한다) | `D-EVIDENCE-PRESENTATION-E4-E5-003` | 논리가 선행한 기존 기능에도 자산 조사와 표현 준비를 적용한다 | `TitleReviewed` |
| [D-389](../DECISIONS.md#d-389-presentation-e4e5의-연결-사전검사와-인계를-공통화한다) | `D-EVIDENCE-PRESENTATION-E4-E5-004` | Presentation E4→E5의 연결 사전검사와 인계를 공통화한다 | `TitleReviewed` |
| [D-390](../DECISIONS.md#d-390-전체-배치-기준과-개별-e5-성립후속-조화의-책임을-분리한다) | `D-EVIDENCE-PRESENTATION-E4-E5-005` | 전체 배치 기준과 개별 E5 성립·후속 조화의 책임을 분리한다 | `TitleReviewed` |
| [D-391](../DECISIONS.md#d-391-시간공간플레이어대상-관점으로-기존-기획과-wi를-정리한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-001` | 시간·공간·플레이어·대상 관점으로 기존 기획과 WI를 정리한다 | `TitleReviewed` |
| [D-392](../DECISIONS.md#d-392-네-관점과-wi-순환으로-전체-기획-이관을-우선한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-002` | 네 관점과 WI 순환으로 전체 기획 이관을 우선한다 | `TitleReviewed` |
| [D-393](../DECISIONS.md#d-393-네-관점에-skylh배치플레이어-상태대상-시스템을-연결한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-003` | 네 관점에 Sky·LH/배치·플레이어 상태·대상 시스템을 연결한다 | `TitleReviewed` |
| [D-394](../DECISIONS.md#d-394-기획의-공통-안내-말을-지금여기나너이렇게로-연결한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-004` | 기획의 공통 안내 말을 지금·여기·나·너·이렇게로 연결한다 | `TitleReviewed` |
| [D-395](../DECISIONS.md#d-395-같은-기획-패턴을-문서코드자산unity-검증과-결과-기록으로-연결한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-005` | 같은 기획 패턴을 문서·코드·자산·Unity 검증과 결과 기록으로 연결한다 | `TitleReviewed` |
| [D-396](../DECISIONS.md#d-396-wi-전체를-중심으로-e4까지-문서코드synty-준비를-진행한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-006` | WI 전체를 중심으로 E4까지 문서·코드·Synty 준비를 진행한다 | `TitleReviewed` |
| [D-397](../DECISIONS.md#d-397-실제-문답도-지금여기나너이렇게의-상황과-선택으로-이어간다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-007` | 실제 문답도 지금·여기·나·너·이렇게의 상황과 선택으로 이어간다 | `TitleReviewed` |
| [D-398](../DECISIONS.md#d-398-한-회차에-질문-하나로-상황과-선택을-깊게-탐구한다) | `D-PLANNING-PLAYER-CENTERED-INQUIRY-008` | 한 회차에 질문 하나로 상황과 선택을 깊게 탐구한다 | `TitleReviewed` |
| [D-399](../DECISIONS.md#d-399-시간성을-절기-중심으로-읽고-농사계절-상품물류에-연결한다) | `D-GAMEPLAY-SEASON-TECH-TREE-001` | 시간성을 절기 중심으로 읽고 농사·계절 상품·물류에 연결한다 | `TitleReviewed` |
| [D-400](../DECISIONS.md#d-400-계절의-야생-변화와-재배-시설-대응을-기획에-연결한다) | `D-GAMEPLAY-SEASON-TECH-TREE-002` | 계절의 야생 변화와 재배 시설 대응을 기획에 연결한다 | `TitleReviewed` |
| [D-401](../DECISIONS.md#d-401-재배-추위-대응을-난방과-마법진까지-연결한다) | `D-GAMEPLAY-SEASON-TECH-TREE-003` | 재배 추위 대응을 난방과 마법진까지 연결한다 | `TitleReviewed` |
| [D-402](../DECISIONS.md#d-402-발전-수단을-기능-군집과-테크트리로-관리하고-정보를-점진-공개한다) | `D-GAMEPLAY-SEASON-TECH-TREE-004` | 발전 수단을 기능 군집과 테크트리로 관리하고 정보를 점진 공개한다 | `TitleReviewed` |
| [D-403](../DECISIONS.md#d-403-한국-24절기를-기준으로-농수산물-제철-자료를-조사연결한다) | `D-GAMEPLAY-SEASON-TECH-TREE-005` | 한국 24절기를 기준으로 농수산물 제철 자료를 조사·연결한다 | `TitleReviewed` |
| [D-404](../DECISIONS.md#d-404-자료조사에서-로그인-등-접근-조치가-필요하면-먼저-보고한다) | `D-GAMEPLAY-SEASON-TECH-TREE-006` | 자료조사에서 로그인 등 접근 조치가 필요하면 먼저 보고한다 | `TitleReviewed` |
| [D-405](../DECISIONS.md#d-405-절기별-산숲-경관을-월드맵배치lh-협력으로-표현한다) | `D-GAMEPLAY-SEASON-TECH-TREE-007` | 절기별 산·숲 경관을 월드맵·배치·LH 협력으로 표현한다 | `TitleReviewed` |
| [D-406](../DECISIONS.md#d-406-초기-산재-표시를-정리하고-월드맵-기반실제-보행을-우선한다) | `D-WORLD-WORLDMAP-PROPOSAL-001` | 초기 산재 표시를 정리하고 월드맵 기반·실제 보행을 우선한다 | `TitleReviewed` |
| [D-407](../DECISIONS.md#d-407-네-업무영역을-지형지물로-구분하는-월드맵-제안을-작성한다) | `D-WORLD-WORLDMAP-PROPOSAL-002` | 네 업무영역을 지형지물로 구분하는 월드맵 제안을 작성한다 | `TitleReviewed` |
| [D-408](../DECISIONS.md#d-408-발견형-구도를-첫-인과-기록으로-남기고-기존-문답을-연결한다) | `D-STORY-FIRST-DISCOVERY-001` | 발견형 구도를 첫 인과 기록으로 남기고 기존 문답을 연결한다 | `TitleReviewed` |
| [D-409](../DECISIONS.md#d-409-날씨별-발견-난도를-확정하고-전투-활용은-후속으로-분리한다) | `D-STORY-FIRST-DISCOVERY-002` | 날씨별 발견 난도를 확정하고 전투 활용은 후속으로 분리한다 | `TitleReviewed` |
| [D-410](../DECISIONS.md#d-410-잔여-문답을-현재-관점으로-전량-대조하고-리팩토링한다) | `D-STORY-FIRST-DISCOVERY-003` | 잔여 문답을 현재 관점으로 전량 대조하고 리팩토링한다 | `TitleReviewed` |
| [D-411](../DECISIONS.md#d-411-숲-가장자리와-농장-외곽부터-플레이-경험을-구체화한다) | `D-STORY-FIRST-DISCOVERY-004` | 숲 가장자리와 농장 외곽부터 플레이 경험을 구체화한다 | `TitleReviewed` |
| [D-412](../DECISIONS.md#d-412-춘분을-기획-기준으로-삼고-보유-경관-자산을-먼저-조사한다) | `D-STORY-FIRST-DISCOVERY-005` | 춘분을 기획 기준으로 삼고 보유 경관 자산을 먼저 조사한다 | `TitleReviewed` |
| [D-413](../DECISIONS.md#d-413-춘분-무렵의-약초와-농작물-자료를-경관-기획과-병행-조사한다) | `D-STORY-FIRST-DISCOVERY-006` | 춘분 무렵의 약초와 농작물 자료를 경관 기획과 병행 조사한다 | `TitleReviewed` |
| [D-414](../DECISIONS.md#d-414-북부-춘분의-굶주린-농장-발견과-상호-도움눈과-침엽수-배경을-기획한다) | `D-STORY-FIRST-DISCOVERY-007` | 북부 춘분의 굶주린 농장 발견과 상호 도움·눈과 침엽수 배경을 기획한다 | `TitleReviewed` |
| [D-415](../DECISIONS.md#d-415-눈-없는-춘분의-봄으로-전환하고-부담-없는-식사-협력과-가방-필요를-반영한다) | `D-STORY-FIRST-DISCOVERY-008` | 눈 없는 춘분의 봄으로 전환하고 부담 없는 식사 협력과 가방 필요를 반영한다 | `TitleReviewed` |
| [D-416](../DECISIONS.md#d-416-여러-영역의-선택형-플레이를-함께-설계하고-승인된-개발을-병행한다) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-001` | 여러 영역의 선택형 플레이를 함께 설계하고 승인된 개발을 병행한다 | `TitleReviewed` |
| [D-417](../DECISIONS.md#d-417-직접-탐험의-1인칭과-넓은-운영-시점현실-업무-보조를-연결한다) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-002` | 직접 탐험의 1인칭과 넓은 운영 시점·현실 업무 보조를 연결한다 | `TitleReviewed` |
| [D-418](../DECISIONS.md#d-418-wi를-시점의-스케일별-공통특화전용으로-구분하고-독립-발전을-관리한다) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-003` | WI를 시점의 스케일별 공통·특화·전용으로 구분하고 독립 발전을 관리한다 | `TitleReviewed` |
| [D-419](../DECISIONS.md#d-419-1인칭-직접-타이밍-수행을-선택형-추가-효과에-특화한다) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-004` | 1인칭 직접 타이밍 수행을 선택형 추가 효과에 특화한다 | `TitleReviewed` |
| [D-420](../DECISIONS.md#d-420-집중-타이밍은-플레이어가-선택한-작업에서만-제시한다) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-005` | 집중 타이밍은 플레이어가 선택한 작업에서만 제시한다 | `TitleReviewed` |
| [D-421](../DECISIONS.md#d-421-반복-동작마다-집중에-재도전하고-성공으로-작업-효율을-높인다) | `D-GAMEPLAY-PERSPECTIVE-FOCUS-006` | 반복 동작마다 집중에 재도전하고 성공으로 작업 효율을 높인다 | `TitleReviewed` |
| [D-426](../DECISIONS.md#d-426-개체-종류와-개별-기록의-시각-자산-대응을-여러-분야에-구현한다) | `D-DATA-GAME-OBJECT-ASSET-DB-001` | 개체 종류와 개별 기록의 시각 자산 대응을 여러 분야에 구현한다 | `TitleReviewed` |
| [D-427](../DECISIONS.md#d-427-농장-건물과-밭은-이격정렬하고-상점-진열대는-실내에-둔다) | `D-DATA-GAME-OBJECT-ASSET-DB-002` | 농장 건물과 밭은 이격·정렬하고 상점 진열대는 실내에 둔다 | `TitleReviewed` |
| [D-428](../DECISIONS.md#d-428-npc-양조-공방에서-배우고-국내-현실-양조장-자료를-선택형으로-연결한다) | `D-DATA-GAME-OBJECT-ASSET-DB-003` | NPC 양조 공방에서 배우고 국내 현실 양조장 자료를 선택형으로 연결한다 | `TitleReviewed` |
| [D-429](../DECISIONS.md#d-429-공공자료-조사는-기존-서버-수집과-docker-mysql-축적까지를-기본으로-한다) | `D-DATA-GAME-OBJECT-ASSET-DB-004` | 공공자료 조사는 기존 서버 수집과 Docker MySQL 축적까지를 기본으로 한다 | `TitleReviewed` |
| [D-430](../DECISIONS.md#d-430-기존-dbset-개체레코드와-kamis-코드의-실제-시각-대응을-먼저-재검증한다) | `D-DATA-GAME-OBJECT-ASSET-DB-005` | 기존 DbSet 개체·레코드와 KAMIS 코드의 실제 시각 대응을 먼저 재검증한다 | `TitleReviewed` |
| [D-431](../DECISIONS.md#d-431-synty-자산-목록과-개체-대응-관계를-mysql에-먼저-구축한다) | `D-DATA-GAME-OBJECT-ASSET-DB-006` | Synty 자산 목록과 개체 대응 관계를 MySQL에 먼저 구축한다 | `TitleReviewed` |
| [D-432](../DECISIONS.md#d-432-개체별-자산-할당을-실제-이미지로-비교검토할-수-있게-한다) | `D-DATA-GAME-OBJECT-ASSET-DB-007` | 개체별 자산 할당을 실제 이미지로 비교·검토할 수 있게 한다 | `TitleReviewed` |
| [D-433](../DECISIONS.md#d-433-프리팹-이미지-전담-조사를-만들고-azure-비공개-보관웹-열람으로-연결한다) | `D-DATA-GAME-OBJECT-ASSET-DB-008` | 프리팹 이미지 전담 조사를 만들고 Azure 비공개 보관·웹 열람으로 연결한다 | `TitleReviewed` |
| [D-434](../DECISIONS.md#d-434-게임-객체를-역할별-여러-자산으로-구성하여-mysql-관계로-관리한다) | `D-DATA-GAME-OBJECT-ASSET-DB-009` | 게임 객체를 역할별 여러 자산으로 구성하여 MySQL 관계로 관리한다 | `TitleReviewed` |
| [D-435](../DECISIONS.md#d-435-wi의-플레이-관점에서-게임-객체를-추출하여-기존-mysql-정의와-연결한다) | `D-DATA-GAME-OBJECT-ASSET-DB-010` | WI의 플레이 관점에서 게임 객체를 추출하여 기존 MySQL 정의와 연결한다 | `TitleReviewed` |
| [D-436](../DECISIONS.md#d-436-새-문답-확대보다-기존-요청을-사용자-확인-가능한-결과로-우선-마무리한다) | `D-DATA-GAME-OBJECT-ASSET-DB-011` | 새 문답 확대보다 기존 요청을 사용자 확인 가능한 결과로 우선 마무리한다 | `TitleReviewed` |
| [D-437](../DECISIONS.md#d-437-보유-synty-팩-전체의-자산-목록을-조사하여-mysql에-축적한다) | `D-DATA-GAME-OBJECT-ASSET-DB-012` | 보유 Synty 팩 전체의 자산 목록을 조사하여 MySQL에 축적한다 | `TitleReviewed` |
| [D-438](../DECISIONS.md#d-438-wi-전수-객체-조사를-쉬고-있는-자료-담당에-분담하고-개발이-db-등록을-통합한다) | `D-DATA-GAME-OBJECT-ASSET-DB-013` | WI 전수 객체 조사를 쉬고 있는 자료 담당에 분담하고 개발이 DB 등록을 통합한다 | `TitleReviewed` |
| [D-439](../DECISIONS.md#d-439-표준-기획-문서의-판단을-로컬-저장-기능으로-mysql-관계에-연결한다) | `D-DATA-GAME-OBJECT-ASSET-DB-014` | 표준 기획 문서의 판단을 로컬 저장 기능으로 MySQL 관계에 연결한다 | `TitleReviewed` |
| [D-440](../DECISIONS.md#d-440-근거가-충분한-자산을-codex가-역할별-자동-할당하고-개발-작업이-구현한다) | `D-DATA-GAME-OBJECT-ASSET-DB-015` | 근거가 충분한 자산을 Codex가 역할별 자동 할당하고 개발 작업이 구현한다 | `TitleReviewed` |
| [D-441](../DECISIONS.md#d-441-기존-synty-기능-분류를-재사용하여-db-검색과-자동-할당을-보완한다) | `D-DATA-GAME-OBJECT-ASSET-DB-016` | 기존 Synty 기능 분류를 재사용하여 DB 검색과 자동 할당을 보완한다 | `TitleReviewed` |
| [D-442](../DECISIONS.md#d-442-실제-월드-배치-전에-패턴-조합과-배치-규칙을-정밀화하고-격리-미리보기로-검토한다) | `D-WORLD-GRAPH-MAP-E6-001` | 실제 월드 배치 전에 패턴 조합과 배치 규칙을 정밀화하고 격리 미리보기로 검토한다 | `TitleReviewed` |
| [D-443](../DECISIONS.md#d-443-기존-노드엣지-구조를-재사용해-패턴-관계와-연결-의도를-검사한다) | `D-WORLD-GRAPH-MAP-E6-002` | 기존 노드·엣지 구조를 재사용해 패턴 관계와 연결 의도를 검사한다 | `TitleReviewed` |
| [D-444](../DECISIONS.md#d-444-그래프-맵을-기획서의-지금여기나너이렇게와-연결하여-검토한다) | `D-WORLD-GRAPH-MAP-E6-003` | 그래프 맵을 기획서의 지금·여기·나·너·이렇게와 연결하여 검토한다 | `TitleReviewed` |
| [D-445](../DECISIONS.md#d-445-그래프-맵의-플레이-관계와-세부-배치-규칙을-두-상세-수준으로-문서화한다) | `D-WORLD-GRAPH-MAP-E6-004` | 그래프 맵의 플레이 관계와 세부 배치 규칙을 두 상세 수준으로 문서화한다 | `TitleReviewed` |
| [D-446](../DECISIONS.md#d-446-그래프-맵-기반-정밀화를-e5-준비입증의-공식-주력-절차로-채택한다) | `D-WORLD-GRAPH-MAP-E6-005` | 그래프 맵 기반 정밀화를 E5 준비·입증의 공식 주력 절차로 채택한다 | `TitleReviewed` |
| [D-447](../DECISIONS.md#d-447-공식-준비입증-절차를-e6-정제와-필요한-현실-근거-결속까지-확장한다) | `D-WORLD-GRAPH-MAP-E6-006` | 공식 준비·입증 절차를 E6 정제와 필요한 현실 근거 결속까지 확장한다 | `TitleReviewed` |
| [D-448](../DECISIONS.md#d-448-누적-변경을-정합화하고-소유가-확인된-맥락별로-커밋한다) | `D-OPERATIONS-GIT-COMMIT-001` | 누적 변경을 정합화하고 소유가 확인된 맥락별로 커밋한다 | `TitleReviewed` |
| [D-449](../DECISIONS.md#d-449-허브를-발견하는-3인칭-도입과-광역-노드-관찰을-기획한다) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-001` | 허브를 발견하는 3인칭 도입과 광역 노드 관찰을 기획한다 | `TitleReviewed` |
| [D-450](../DECISIONS.md#d-450-허브를-데이터-통합과-공통-개발-검증의-중심-사례로-활용한다) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-002` | 허브를 데이터 통합과 공통 개발 검증의 중심 사례로 활용한다 | `TitleReviewed` |
| [D-451](../DECISIONS.md#d-451-허브를-화물차기사-npc가-오가는-물류센터로-표현하고-창고-상세를-두-시점에서-함께-읽는다) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-003` | 허브를 화물차·기사 NPC가 오가는 물류센터로 표현하고 창고 상세를 두 시점에서 함께 읽는다 | `TitleReviewed` |
| [D-452](../DECISIONS.md#d-452-현실-물류-자료를-게임용-허브-물류-사본으로-변환하고-선택형-상세에서-대응-근거를-보여준다) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-004` | 현실 물류 자료를 게임용 허브 물류 사본으로 변환하고 선택형 상세에서 대응 근거를 보여준다 | `TitleReviewed` |
| [D-453](../DECISIONS.md#d-453-첫-현실-연계-물류-신호는-입출고-추세불균형품목군-비중의-상대-단계로-둔다) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-005` | 첫 현실 연계 물류 신호는 입출고 추세·불균형·품목군 비중의 상대 단계로 둔다 | `TitleReviewed` |
| [D-454](../DECISIONS.md#d-454-플레이어가-접촉한-게임-데이터의-현실-대응-보고서를-선택적으로-생성통지할-수-있게-한다) | `D-GAMEPLAY-HUB-REALITY-LOGISTICS-006` | 플레이어가 접촉한 게임 데이터의 현실 대응 보고서를 선택적으로 생성·통지할 수 있게 한다 | `TitleReviewed` |
| [D-455](../DECISIONS.md#d-455-허브-설명과-도움-요청은-물음표느낌표-표시를-사용자가-선택할-때만-연다) | `D-INTERACTION-QUEST-001` | 허브 설명과 도움 요청은 물음표·느낌표 표시를 사용자가 선택할 때만 연다 | `TitleReviewed` |
| [D-456](../DECISIONS.md#d-456-물음표는-설명-느낌표는-도움-요청으로-구분하고-함께-있으면-통합-표시를-사용한다) | `D-INTERACTION-QUEST-002` | 물음표는 설명, 느낌표는 도움 요청으로 구분하고 함께 있으면 통합 표시를 사용한다 | `TitleReviewed` |
| [D-457](../DECISIONS.md#d-457-색과-보조표식이-다른-물음표로-정보퀘스트-획득을-구분하고-느낌표는-완료-확인에-사용한다) | `D-INTERACTION-QUEST-003` | 색과 보조표식이 다른 물음표로 정보·퀘스트 획득을 구분하고 느낌표는 완료 확인에 사용한다 | `TitleReviewed` |
| [D-458](../DECISIONS.md#d-458-퀘스트-정산-뒤-world-완료-표시는-제거하고-기록과-실제-후속-의뢰-표시를-분리한다) | `D-INTERACTION-QUEST-004` | 퀘스트 정산 뒤 World 완료 표시는 제거하고 기록과 실제 후속 의뢰 표시를 분리한다 | `TitleReviewed` |
| [D-459](../DECISIONS.md#d-459-의미-있는-금색-의뢰-완료는-개인-회복-기여를-만들고-검증된-공동체-기여로-집계한다) | `D-INTERACTION-QUEST-005` | 의미 있는 금색 의뢰 완료는 개인 회복 기여를 만들고 검증된 공동체 기여로 집계한다 | `TitleReviewed` |
| [D-460](../DECISIONS.md#d-460-개인-회복은-공동체-회복의-기본층에-반영하고-공공-문제-해결은-더-강한-기여층으로-둔다) | `D-INTERACTION-QUEST-006` | 개인 회복은 공동체 회복의 기본층에 반영하고 공공 문제 해결은 더 강한 기여층으로 둔다 | `TitleReviewed` |
| [D-461](../DECISIONS.md#d-461-공동체-회복-기본층은-실제-소속체류관측-구성원만-집계한다) | `D-INTERACTION-QUEST-007` | 공동체 회복 기본층은 실제 소속·체류·관측 구성원만 집계한다 | `TitleReviewed` |
| [D-462](../DECISIONS.md#d-462-파란-물음표는-일반-퀘스트-금색-물음표는-메인-퀘스트로-두고-일반-경험으로-메인을-준비한다) | `D-INTERACTION-QUEST-008` | 파란 물음표는 일반 퀘스트, 금색 물음표는 메인 퀘스트로 두고 일반 경험으로 메인을 준비한다 | `TitleReviewed` |
| [D-463](../DECISIONS.md#d-463-퀘스트에서-게임-문제-해결과-대응-현실-자료-이해를-함께-제공한다) | `D-INTERACTION-QUEST-009` | 퀘스트에서 게임 문제 해결과 대응 현실 자료 이해를 함께 제공한다 | `TitleReviewed` |
| [D-464](../DECISIONS.md#d-464-동적-퀘스트는-그래프-호환-뼈대를-남기고-현재-퀘스트-문답을-마감해-개발에-인계한다) | `D-INTERACTION-QUEST-010` | 동적 퀘스트는 그래프 호환 뼈대를 남기고 현재 퀘스트 문답을 마감해 개발에 인계한다 | `TitleReviewed` |
| [D-465](../DECISIONS.md#d-465-발견-사실은-자동-기록하고-계획-채택은-플레이어가-직접-선택한다) | `D-PLANNING-DISCOVERY-PLAN-001` | 발견 사실은 자동 기록하고 계획 채택은 플레이어가 직접 선택한다 | `TitleReviewed` |
| [D-466](../DECISIONS.md#d-466-현재-실행-가능한-발견만-계획-후보로-추천한다) | `D-PLANNING-DISCOVERY-PLAN-002` | 현재 실행 가능한 발견만 계획 후보로 추천한다 | `TitleReviewed` |
| [D-467](../DECISIONS.md#d-467-활성-계획과-가까운-실행-가능-후보를-먼저-보여준다) | `D-PLANNING-DISCOVERY-PLAN-003` | 활성 계획과 가까운 실행 가능 후보를 먼저 보여준다 | `TitleReviewed` |
| [D-468](../DECISIONS.md#d-468-관심-분야는-관련-이데아와-실행-기회를-더-쉽게-알아차리게-한다) | `D-PLANNING-DISCOVERY-PLAN-004` | 관심 분야는 관련 이데아와 실행 기회를 더 쉽게 알아차리게 한다 | `TitleReviewed` |
| [D-469](../DECISIONS.md#d-469-e5는-대상-wi에-적용되는-e1e4-전체-맥락을-소비해-실제-결속한다) | `D-EVIDENCE-E5-CONTEXT-001` | E5는 대상 WI에 적용되는 E1~E4 전체 맥락을 소비해 실제 결속한다 | `TitleReviewed` |
| [D-470](../DECISIONS.md#d-470-메인-스토리는-지금여기나너이렇게-하위-기획과-wi를-의미로-결속한다) | `D-STORY-MAIN-STORY-001` | 메인 스토리는 지금·여기·나·너·이렇게 하위 기획과 WI를 의미로 결속한다 | `TitleReviewed` |
| [D-471](../DECISIONS.md#d-471-흑막상인-원작을-메인-스토리-기준으로-삼고-원문-확인-전-각색을-확정하지-않는다) | `D-STORY-MAIN-STORY-002` | 흑막상인 원작을 메인 스토리 기준으로 삼고 원문 확인 전 각색을 확정하지 않는다 | `TitleReviewed` |
| [D-472](../DECISIONS.md#d-472-기획-스레드는-개발-목표를-소유하거나-중간-완료를-기다리지-않는다) | `D-PLANNING-EVIDENCE-GOVERNANCE-001` | 기획 스레드는 개발 목표를 소유하거나 중간 완료를 기다리지 않는다 | `TitleReviewed` |
| [D-473](../DECISIONS.md#d-473-상위-e는-같은-후보의-모든-하위-e를-누적-소비해-성립한다) | `D-PLANNING-EVIDENCE-GOVERNANCE-002` | 상위 E는 같은 후보의 모든 하위 E를 누적 소비해 성립한다 | `TitleReviewed` |
| [D-474](../DECISIONS.md#d-474-요동성-방어를-메인-스토리의-첫-장기-목표로-둔다) | `D-STORY-YODONG-001` | 요동성 방어를 메인 스토리의 첫 장기 목표로 둔다 | `BodyReviewed` |
| [D-475](../DECISIONS.md#d-475-요동성-첫-장의-위협인과결과복구-기준선을-닫는다) | `D-STORY-YODONG-002` | 요동성 첫 장의 위협·인과·결과·복구 기준선을 닫는다 | `BodyReviewed` |
| [D-476](../DECISIONS.md#d-476-요동성-방어의-기본은-내정전투-혼합-대응이다) | `D-GAMEPLAY-YODONG-001` | 요동성 방어의 기본은 내정·전투 혼합 대응이다 | `BodyReviewed` |
| [D-477](../DECISIONS.md#d-477-플레이-유형을-먼저-고르지-않고-실제-활동-기록이-요동성-방비를-바꾼다) | `D-GAMEPLAY-YODONG-002` | 플레이 유형을 먼저 고르지 않고 실제 활동 기록이 요동성 방비를 바꾼다 | `BodyReviewed` |
| [D-478](../DECISIONS.md#d-478-첫-방어의-군내-역할은-평소-키운-역량에서-주로-정한다) | `D-GAMEPLAY-YODONG-003` | 첫 방어의 군내 역할은 평소 키운 역량에서 주로 정한다 | `BodyReviewed` |
| [D-479](../DECISIONS.md#d-479-추천-역할과-다른-행동의-한계는-실제-능력-데이터와-결과로-드러낸다) | `D-GAMEPLAY-YODONG-004` | 추천 역할과 다른 행동의 한계는 실제 능력 데이터와 결과로 드러낸다 | `BodyReviewed` |
| [D-480](../DECISIONS.md#d-480-몬스터-이름-색으로-현재-조건의-상대-위험을-보여-준다) | `D-PRESENTATION-COMBAT-RISK-001` | 몬스터 이름 색으로 현재 조건의 상대 위험을 보여 준다 | `BodyReviewed` |
| [D-481](../DECISIONS.md#d-481-개인과-분대의-상대-위험을-구분하고-실제-보급-기여를-분대에-반영한다) | `D-PRESENTATION-COMBAT-RISK-002` | 개인과 분대의 상대 위험을 구분하고 실제 보급 기여를 분대에 반영한다 | `BodyReviewed` |
| [D-482](../DECISIONS.md#d-482-전투-중-실제-상태-변화에-따라-개인분대-위험-색을-갱신한다) | `D-PRESENTATION-COMBAT-RISK-003` | 전투 중 실제 상태 변화에 따라 개인·분대 위험 색을 갱신한다 | `BodyReviewed` |
| [D-483](../DECISIONS.md#d-483-위험-색은-대응을-추천하지-않고-실행-가능한-전술-선택만-연다) | `D-INTERACTION-COMBAT-COMMAND-001` | 위험 색은 대응을 추천하지 않고 실행 가능한 전술 선택만 연다 | `BodyReviewed` |
| [D-484](../DECISIONS.md#d-484-빠른-전술-메뉴에는-현재-실행-가능한-명령만-표시한다) | `D-INTERACTION-COMBAT-COMMAND-002` | 빠른 전술 메뉴에는 현재 실행 가능한 명령만 표시한다 | `BodyReviewed` |
| [D-485](../DECISIONS.md#d-485-전투-전에-후퇴증원보급-선택의-실제-조건을-마련한다) | `D-GAMEPLAY-BATTLE-PREPARATION-001` | 전투 전에 후퇴·증원·보급 선택의 실제 조건을 마련한다 | `BodyReviewed` |
| [D-486](../DECISIONS.md#d-486-전역-이력-번호와-분야별-결정-id를-함께-사용한다) | `D-PLANNING-DECISION-NAMING-001` | 전역 이력 번호와 분야별 결정 ID를 함께 사용한다 | `BodyReviewed` |
| [D-487](../DECISIONS.md#d-487-결정-분야-분류와-공식-wi-대장을-양방향-관계-색인으로-연결한다) | `D-PLANNING-DECISION-WI-RELATION-001` | 결정 분야 분류와 공식 WI 대장을 양방향 관계 색인으로 연결한다 | `BodyReviewed` |
| [D-488](../DECISIONS.md#d-488-승인-기획을-graph-map-작업에-판본-인계하고-최종-결과만-기획에-반환한다) | `D-PLANNING-GRAPH-MAP-HANDOFF-001` | 승인 기획을 Graph Map 작업에 판본 인계하고 최종 결과만 기획에 반환한다 | `BodyReviewed` |
| [D-489](../DECISIONS.md#d-489-graph-map의-작은-구현-후보를-기존-goalwi작업-명세에-결속해-개발로-인계한다) | `D-PLANNING-GRAPH-MAP-DEVELOPMENT-HANDOFF-001` | Graph Map의 작은 구현 후보를 기존 Goal·WI·작업 명세에 결속해 개발로 인계한다 | `BodyReviewed` |
