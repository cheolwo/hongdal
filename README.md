# 살뜰 (Ssalddel)

살뜰은 **정보 공개형 커뮤니티와 실제 업무 도구**, 그리고 같은 도메인을 플레이 가능한 세계로 검증하는 **Simulation·Unity 게임 프로젝트**를 함께 개발한다. 커뮤니티에서는 사람들이 정보를 비교하고 참여·협력 과정을 투명하게 기록하며, 게임에서는 플레이어가 Nature·Farm·Hub·Town·City에서 직접 선택하고 일하고 살아가면서 세계를 변화시킨다.

## 프로젝트를 한눈에 보기

| 축 | 무엇을 만드는가 | 현재 권위 경계 |
| --- | --- | --- |
| 커뮤니티·업무 | 정보 공개, 참여, 공동 원장과 역할별 WebApp | 서버의 업무 원장과 명시적 동의·권한 |
| Simulation Core | 플레이어·NPC·자원·시설·시간·작업·결과 | Solo의 `LocalProcess`, Hosted의 `RemoteHost`가 같은 규칙 실행 |
| Unity 게임 | 이동·선택·카메라·공간·건물·애니메이션·UI·Audio | canonical `SimulationWorldShell`이 권위 상태를 입력과 표현으로 연결 |

현재 대표 게임 개발은 Nature에서 생존 기반을 만들고 회복 방법을 배우는 플레이부터 시작한다. 플레이어는 도구를 얻고 자원을 모으며 거점·불·건설·Recipe·수면·위협 대응을 선택할 수 있다. Nature 정착은 의무가 아니며 이후 Farm·Hub·Town·City에서 각 영역의 독립적인 생활·업무 폐루프를 선택할 수 있게 확장한다.

Unity GameObject나 화면은 권위 상태를 직접 변경하지 않는다.

```text
플레이어 입력
  → Preview: 가능 여부·비용·위험 확인, 상태 무변경
  → Confirm: 명시적 선택 확정
  → Task / Realtime / WorldTick
  → Effect와 WorldRevision
  → 최신 상태 사본
  → Unity 공간·캐릭터·UI·Audio 표현
```

## 처음 참여하는 사람의 읽기 순서

1. 이 README에서 제품과 게임 개발 구조를 파악한다.
2. [AI 공용 프로젝트 컨텍스트](docs/ProjectOverview/GptProjectContext.md)에서 시스템별 책임과 현재 경계를 읽는다.
3. [확정 결정](docs/AI/DECISIONS.md)과 [현재 작업](docs/AI/CURRENT_WORK.md)에서 최신 기준과 실제 검증 상태를 확인한다.
4. 게임 작업이면 [현재 Codex PlayableLoop Goal](docs/AI/generated/codex-playable-loop-goals.md)에서 활성 폐루프·WI·차단·다음 의존성을 확인한다.
5. [문답 기록 routing](docs/Architecture/PlayableLoops/PlanningSessions/README.md)에서 해당 주제의 질문·답변·남은 미정을 읽는다.
6. 활성 Goal이 참조하는 `Approved` 기획서와 E7 작업 명세만 구현 입력으로 사용한다.
7. 코드 위치는 [Simulation·Unity 코드 지도](docs/AI/generated/simulation-unity-code-map.md)에서 찾고, 완료 여부는 [현재 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md)에서 확인한다.

## 문답에서 개발까지

게임 기획은 한 번에 거대한 명세를 확정하지 않는다. 기획 스레드가 한 번에 핵심 질문 하나를 제시하고, 답변을 문답 기록에 판본화한다. 질문은 특정 성장 체계에만 치우치지 않고 Unity 게임을 완성하는 데 필요한 구성요소를 균형 있게 채우도록 설계한다.

```text
핵심 질문 하나
  → 사용자 답변과 해석 확인
  → 상황·선택·대가·실패·회복·귀환 정리
  → WI·H·권위·표현·저장 영향 기록
  → 3~5개 답변 또는 한 구성요소가 안정되면 합성
  → Approved 기획 revision + hash + 작업 명세
  → 개발 스레드에 좁은 에비던스 상한으로 인계
  → 코드·시험·Runtime 증거 반환
  → 다음 질문 또는 가장 이른 E 단계 재개
```

질문 균형은 단순히 분야별 질문 수를 맞추는 일이 아니다. 플레이어 경험, Simulation 규칙, WI·Task, 조작·카메라, H 공간, 건물·배치·자산, 캐릭터·NPC·애니메이션, UI·Audio·FX, 성장·경제, 외부 자료, Save·온라인, Unity 조립·성능, 시험·Game View·빌드의 공백을 살핀다. 세부 기준은 [PlayableLoop 문답 정밀화 체계](docs/Architecture/PlayableLoop문답정밀화체계.md)를 따른다.

현재 문답은 주제별로 분리돼 있다.

- [Nature 거점·수면·날씨·방어](docs/Architecture/PlayableLoops/PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md)
- [플레이어 내면·명상·계획](docs/Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md)
- [Nature 자원·LandUse·건설](docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md)
- [약초·Recipe·조합 제작](docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md)
- [저장·Load·재진입](docs/Architecture/PlayableLoops/PlanningSessions/저장재진입/save-load-runtime.inquiry.r1.md)

## 게임 개발 체계 용어

| 체계 | 답하는 질문 | 핵심 경계 |
| --- | --- | --- |
| `PlayableLoop` | 플레이어가 어떤 상황에서 선택하고 성공·실패·회복·귀환하는가? | 여러 WI가 다음 선택으로 돌아오는 실제 플레이 단위 |
| `WI` | 세계에서 한 번에 어떤 의미 있는 상태를 바꾸는가? | Preview·Confirm·Task·Effect와 단일 권위 책임 |
| `H1~H5` | 행동 공간부터 세계 배치까지 무엇을 어떻게 포함하는가? | 공간 조립 깊이이며 에비던스 성숙도와 별개 |
| `E1~E10` | 논리와 표현이 실제 증거로 어디까지 검증됐는가? | 통합 E는 Logic·Presentation 중 낮은 단계 |
| `G1~G5` | 각 에비던스 구간을 어떤 관리 체계로 통과시키는가? | G 완료가 E 승격을 자동 의미하지 않음 |
| `EvidencePackage` | 어떤 revision·환경·시험·화면이 무엇을 증명하는가? | 코드·시험·Runtime·Game View·운영 증거를 분리 |

### H 공간 포함 체계

```text
H1 행동·작업 공간
  → H2 여러 H1의 블록
  → H3 여러 H2의 경관과 이동·업무 폐루프
  → H4 여러 H3의 위치 독립 AreaSet 청사진
  → H5 AreaSet 인스턴스와 물리 회랑의 세계 배치
```

플레이어는 H1을 직접 배치·복구·연결할 수 있고 H2·H3의 성장을 유도할 수 있다. 상위 공간의 실제 성립은 필요한 WI·연결·용량·폐루프를 Simulation 규칙이 판정한다. 자세한 정의와 현재 검증 상태는 [H1~H5 공간 포함 계층 조사](docs/Architecture/H1-H5공간포함계층조사.md)를 따른다.

### 에비던스 체계

`E`는 기능 개수나 공간 크기가 아니라 **Evidence, 즉 검증된 증거의 성숙도**를 뜻한다.

| 단계 | 핵심 질문 |
| --- | --- |
| E1 | 플레이어 약속과 권위 계약이 확정됐는가? |
| E2 | 계약을 실행할 Core·Adapter가 준비됐는가? |
| E3 | 선택과 결과가 시험·저장·재생에서 결정적인가? |
| E4 | WI의 주체·자원·시간·H 문맥이 결속됐는가? |
| E5 | 선택과 결과가 권위 세계에서 실제로 발현되는가? |
| E6 | 의미·인과·배치·피드백·귀환 결함이 정제됐는가? |
| E7 | 사람이 저장 Scene에서 실제 입력으로 폐루프를 끝낼 수 있는가? |
| E8 | 같은 E7 폐루프가 Save·Replay·Local/Remote·재진입에서 반복 안정적인가? |
| E9 | 같은 영역의 안정 Core 둘 이상이 논리·표현으로 조화롭고 사람이 승인했는가? |
| E10 | 불변 후보가 제한 운영 관찰을 통과했는가? |

각 PlayableUnit은 E7에서 E1로 영향을 내려 검토하고, 가장 낮은 미완료 의존성을 구현한 뒤 E1에서 E7로 다시 조립한다. 결함이 발견되면 같은 Goal에서 가장 이른 책임 단계로 돌아간다. E8~E10은 E7 뒤의 별도 수평 캠페인이다. 현재 기준은 [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md)다.

### G 관리 체계

| 관리 체계 | 주 구간 | 관리 책임 |
| --- | --- | --- |
| G1 | E1→E6 | 세계 성립, WI·H·결정성·정제 |
| G2 | E6→E7 | 실제 입력·카메라·피드백·Game View |
| G3 | E7→E8 | 반복 결정성·Save/Replay·Local/Remote 안정 |
| G4 | E8→E9 | 여러 Core의 영역 조화와 사람 평가 |
| G5 | E9→E10 | 불변 후보의 제한 운영과 관찰 |

예를 들어 `G2 구현 완료`만으로 E7이 되지 않는다. 실제 저장 Scene, 입력, 권위 결과, Game View와 Console 증거가 같은 후보 revision에서 닫혀야 한다.

<p align="center">
  <a href="https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/">
    <strong>01~05 역할별 상시 체험 포털 열기 →</strong>
  </a>
</p>

## 상시 체험 WebApp

> 현재 개발 진행 중인 공개 체험 환경입니다. 화면, 기능, 데이터와 권한 정책은 계속
> 변경될 수 있으며 실제 계약·결제·배차·정산을 실행하는 운영 서비스가 아닙니다.

| 진입점 | 공개 URL |
| --- | --- |
| 역할 선택 포털 | [통합 체험 시작](https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/) |
| 01 Community | [커뮤니티 WebApp](https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/roles/01/) |
| 02 Orderer | [주문자 WebApp](https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/roles/02/) |
| 03 Shipper | [화주 WebApp](https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/roles/03/) |
| 04 Driver | [기사 WebApp](https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/roles/04/) |
| 05 Warehouse | [창고 WebApp](https://ssalddel-v0-7q4m2k.koreacentral.cloudapp.azure.com/roles/05/) |

<p align="center">
  <a href="https://www.figma.com/design/0KhuQLc1MleUBIQnARC21Z/ssalddle?node-id=0-1">
    <strong>Figma 화면 보기 →</strong>
  </a>
</p>

## 운영 업무 Simulation · Unity

[Ssalddel Unity](https://github.com/cheolwo/unity)는 살뜰의 Nature 생존과 Farm·Hub·Town·City의 독립적인 생활·업무 폐루프를 공간과 상호작용으로 검증하는 Unity 프로젝트입니다. 고유 식별자와 권위 revision을 유지하면서 `Preview → Confirm → Task·Realtime·WorldTick → 최신 상태 재조회` 흐름을 게임 월드에서 표현합니다. 영역 간 운송은 각 영역의 내부 폐루프가 성립한 뒤 선택하는 별도 통합 작업입니다.

<p align="center">
  <a href="https://github.com/cheolwo/unity">
    <img src="https://github.com/cheolwo/unity/raw/refs/heads/main/Documentation/Changes/2026-08-11-harvest-route-multi-lot/harvest-route-multi-lot-selection.png" alt="감자 수확물 판로 선택 Unity Simulation Game View" width="900">
  </a>
</p>

> 현재 Unity 화면은 개발용 Simulation입니다. 실제 판매·결제·배차·수출·정산을 실행하지 않으며, 운영 상태의 최종 권위는 서버에 있습니다. 게임 Simulation Core는 Solo에서 Unity 내부 Local Runtime, Hosted Multiplayer에서만 Simulation 서버가 실행합니다.

세계 구축은 공공데이터나 Synty 팩 이름에서 바로 시작하지 않습니다. 먼저 PlayableLoop가 요구하는 WI와 H 공간 능력을 정하고, 배치·실행 엔진이 이를 결정적인 세계 계획으로 조립합니다. 공공데이터는 필요한 현실 근거에만 결속하고, Synty 자산은 Simulation의 의미를 바꾸지 않는 표현 후보로 사용합니다.

```text
플레이어 약속과 PlayableLoop
  → WI: 행위자·조건·선택·Task·Effect
  → H: 행동 공간·블록·경관·AreaSet·세계 배치
  → LH·Sky·실외 배치·실내 배치 엔진
  → Simulation Core의 권위 상태와 WorldRevision
  → canonical SimulationWorldShell의 입력·공간·캐릭터·UI·Audio 표현
  → Logic·Presentation·통합 EvidencePackage
```

이 과정에서 Synty 원본 팩은 출처이지 게임 영역이나 기능의 권위 분류가 아닙니다. 자산은 지면·식생·실외 구조·실내 설비·도구·건설 상태 같은 기능 역할로 분류한 뒤 WI의 현재 권위 상태를 표현할 때 선택합니다. Prefab이 생성됐다는 사실만으로 자원·건물·NPC 상태가 바뀌지는 않습니다.

공간·WI 결속은 [세계 상호작용 단위 중심 공간·Simulation 통합](docs/Architecture/세계상호작용단위중심공간Simulation통합.md), H 정의와 실제 검증 상태는 [H1~H5 공간 포함 계층 조사](docs/Architecture/H1-H5공간포함계층조사.md), 현재 코드 위치는 [Simulation·Unity 코드 지도](docs/AI/generated/simulation-unity-code-map.md)에서 확인합니다. 계획·코드·시험·Runtime·Game View는 서로 다른 증거이며 하나를 다른 하나의 완료로 대신하지 않습니다.

<p align="center">
  <a href="docs/ProjectOverview/page-docs/">
    <img src="docs/assets/changes/2026-07-20-community-forum-restoration/community-board-desktop.png" alt="살뜰 커뮤니티 게시판 글 목록" width="900">
  </a>
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-24-figma-community-mode-toggle/community-mode-toggle.png" alt="Community 생활 게시판과 업무 게시판 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/orderer-import-3pl.png" alt="Orderer 같이 수입 준비 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/shipper-logistics-contract-p1.png" alt="Shipper 물류대행 계약 검토 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/driver-expiry-reconnect-p1.png" alt="Driver 추천과 재연결 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/warehouse-destination-handoff-p1.png" alt="Warehouse 하차지 확인과 운송 인계 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/restaurant-recovery-p1.png" alt="Restaurant 주문 복구 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/seller-mobile-srp.png" alt="Seller 모바일 화면" width="900">
</p>

<p align="center">
  <img src="docs/assets/changes/2026-07-28-figma-code-convergence/admin-mobile-srp.png" alt="Admin Mobile 화면" width="900">
</p>

## 에비던스 기반 게임 개발 업무 순서

Ssalddel의 Simulation·Unity 작업은 현재 목표와 증거 상태에서 시작해 플레이어가 이해할 수 있는 가장 작은 선택 폐루프를 고릅니다. 한 `PlayableUnit`은 E7 플레이 약속부터 E1 계약까지 하향 분해하고, 가장 낮은 미완료 의존성을 구현한 뒤 E1부터 E7까지 실제 증거를 다시 확인합니다. 완성된 각 E7은 별도 E8 반복 안정성 캠페인으로 검증하고, 같은 영역의 안정 Core 둘 이상은 E9 영역 조화·사람 승인, E10 제한 운영 캠페인으로 검증합니다.

```text
현재 목표와 차단점
  → 플레이어의 상황·선택·재료·결과·다음 선택
  → E7→E1 영향·누락 검토
  → 가장 낮은 미완료 의존성 구현
  → E1→E7 조립·증거 검증
  → 새 영향이면 다시 하향 검토
  → 안정 또는 명시적 차단까지 왕복
  → 필요한 형제 E7들과 E8~E10 수평 캠페인
```

플레이어 중심은 Unity나 플레이어에게 상태 권위를 넘긴다는 뜻이 아닙니다. Simulation Core가 조건·비용·시간·결과와 H 공간 성장을 판정하고 Unity는 입력과 표현을 담당합니다. E7을 먼저 적는 것도 완료 주장이 아니라 플레이어 약속에서 필요한 계약까지 영향을 먼저 보는 작업 순서입니다.

- [문서 안내와 질문별 기준 문서](docs/README.md): 같은 설명을 반복하지 않고 각 질문의 단일 권위를 찾는 진입점
- [프로젝트 불변 개발 골격](docs/Architecture/프로젝트불변개발골격.md): 리팩토링과 기능 개발이 보존할 기준선
- [플레이어 중심 게임 개발 업무 구조](docs/Architecture/플레이어중심게임개발업무구조.md): 모든 단계에 적용하는 플레이어 선택 관점
- [플레이 폐루프와 증거 묶음 개발 체계](docs/Architecture/플레이폐루프와증거묶음개발체계.md): 여러 WI의 폐루프 E 판정, 실제 증거 범위·무효화와 협업 인계
- [플레이 폐루프 엔진 상호작용 검증 체계](docs/Architecture/플레이폐루프엔진상호작용검증체계.md): WI 권위 실행과 LH·Sky·실내외 표현을 같은 명령·Revision으로 묶는 통합 관문
- [플레이 폐루프 완결 로드맵](docs/Architecture/플레이폐루프완결로드맵.md): Nature→Farm→Hub→Town→City의 Core 우선·Extension 후속과 영역·세계 집계 완결 순서
- [게임 개발 업무 순서 기준](docs/Architecture/게임개발업무순서기준.md): 작업 선택부터 다음 판단까지의 실행 순서
- [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md): 단일 폐루프·영역 조화·사람 플레이테스트에서 Logic·Presentation 왕복과 제한 운영의 판정 주체·관문
- [WorldTick과 실시간 실행 경계](docs/Architecture/WorldTick과실시간실행경계.md): Unity 표현 시간·권위 실시간 시계·WorldTick·BattleTick·WorldRevision의 책임 구분
- [E 성숙도 책임 코드 지도](docs/Architecture/SsalddelCodeMetadata.md#e-성숙도-책임-메타데이터): Simulation·Unity 구성 요소를 현재 E1~E10 검토 책임에 연결하고 과거 E8·E9 의미를 판본으로 분리하는 기준
- [WI 성숙도 현재 지도](docs/AI/generated/world-interaction-maturity.md): 전체 WI 48개의 선택 여부, E4 문맥, 조건부 H 근거와 E5 발현 상태
- [현재 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md): 완료·부분 완료·미완료·보류 구분

문서, 코드, 자동 시험, Actual E5, 실제 서버, Play Mode·Game View와 운영 효과는 서로 다른 `EvidencePackage`로 기록합니다. Farm·Hub·Town·City는 각각 `PlayableLoop` 독립 내부 폐루프를 먼저 만들고 영역 간 연결은 양쪽이 준비된 뒤 별도 통합 작업으로 선택합니다. 현재 폐루프와 증거 상태는 [자동 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md)에서 확인합니다.

## 개발 책임과 짧은 작업 흐름

기존 `codex/rename-ssalddel`의 운영·Simulation 혼합 이력은 과거 통합 기준선으로 보존합니다. 새 작업은 실제로 바꾸는 권위 상태를 기준으로 `Operations`, `Simulation`, `Unity` 중 주 책임 하나를 먼저 고르고, 공개 계약·Adapter·호환 변경만 `Integration`으로 분리합니다.

```text
cheolwo/ssalddel
├─ operations/<작업명>   실제 업무 원장
├─ simulation/<작업명>   게임 Session·규칙·Save/Replay
└─ integration/<작업명>  계약·Adapter·호환

cheolwo/unity
└─ unity/<작업명>        SimulationWorldShell·입력·표현
```

Git push는 폴더가 아니라 커밋과 브랜치를 전송하므로 서로 다른 책임을 한 커밋에 섞지 않습니다. 작업 ID는 공유할 수 있지만 각 저장소에서 짧은 브랜치, 책임별 커밋과 검증으로 진행합니다. 세부 기준은 [운영·Simulation·Unity 작업 흐름 분리](docs/Architecture/OperationsSimulationUnity작업흐름분리.md), 기계 판독 기준은 [책임 작업 흐름 원장](eng/work-areas/responsibility-workstreams.json)에서 확인합니다.

## 개발

```powershell
dotnet build Ssalddel.v0.0.slnx
dotnet test Ssalddel.Tests/Ssalddel.Tests.csproj
```

전체 문서의 기준과 분류는 [문서 안내](docs/README.md), 화면과 코드 위치는 [프로젝트 화면 안내](docs/ProjectOverview/README.md)와 [화면 카탈로그](docs/ProjectOverview/app-page-catalog.md)에서 확인할 수 있습니다.
