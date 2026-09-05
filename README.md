# Mirror (거울)

Mirror(거울)는 **정보 공개형 커뮤니티와 실제 업무 도구**, 그리고 같은 도메인을 플레이 가능한 세계로 검증하는 **Simulation·Unity 게임 프로젝트**를 함께 개발한다. 커뮤니티에서는 사람들이 정보를 비교하고 참여·협력 과정을 투명하게 기록하며, 게임에서는 플레이어가 Nature·Farm·Hub·Town·City에서 직접 선택하고 일하고 살아가면서 세계를 변화시킨다.

> 2026-08-31부터 프로젝트 표시 이름을 Ssalddel(살뜰)에서 **Mirror(거울)**로 변경했다. GitHub 저장소는 [cheolwo/mirror](https://github.com/cheolwo/mirror)다. 아래의 `Ssalddel.*`, `Assets/Ssalddel`, 기존 브랜치·로컬 경로·저장 식별자는 실행 호환성을 위해 유지한 내부 이름이다. 별도 Unity 저장소 `cheolwo/unity`와 운영 서비스 주소는 변경하지 않았다. 변경 범위는 [D384](docs/AI/DECISIONS.md#d-384-프로젝트-표시명과-github-저장소를-mirror거울로-변경한다)를 따른다.

## 프로젝트를 한눈에 보기

| 축 | 무엇을 만드는가 | 현재 권위 경계 |
| --- | --- | --- |
| 커뮤니티·업무 | 정보 공개, 참여, 공동 원장과 역할별 WebApp | 서버의 업무 원장과 명시적 동의·권한 |
| Simulation Core | 플레이어·NPC·자원·시설·시간·작업·결과 | Solo의 `LocalProcess`, Hosted의 `RemoteHost`가 같은 규칙 실행 |
| Unity 게임 | 이동·선택·카메라·공간·건물·애니메이션·UI·Audio | canonical `SimulationWorldShell`이 권위 상태를 입력과 표현으로 연결 |

현행 게임 개발의 중심 흐름은 다음과 같다. 기획 문서, Graph Map, 배치 맵은 서로 연결되지만 같은 산출물이 아니며, 어느 하나의 작성만으로 실제 Unity 배치나 에비던스 승격을 선언하지 않는다.

```text
주체 기반 확립(Actor·직접 대상의 권위·역할·저장 정체성)
  → WI 하나의 Goal과 직접 행동·대상·결과
  → 직접 결과 뒤 파생 작용 0~2 hop
  → 필요한 경우에만 PlayableLoop 반복 검증 묶음
  → 적용 가능한 행동의 오행 관계와 명상·회복 해석 경계
  → 영향 분야 판정
      ├─ 비공간: 필요한 논리·데이터·UI·Audio·서사 절차만 수행
      └─ 공간: Graph Map(L1·L2·L3)
               → 배치 맵(H 구성·상대 위치·통행·시야)
               → Synty 후보와 필요한 경우에만 Blender 보완
  → 적용 분야의 Presentation E4 후보 동결
  → 개발 작업 명세와 실제 E5 권위 세계·공간 표현 결속
  → E6 정제·E7 실제 입력 폐루프
```

다음은 **책임과 기획 범위를 보여주는 구조도**이며 모든 가지가 구현 완료됐다는 뜻은 아니다. 최신 완료·차단은 [개발 통합 상태판](docs/AI/개발통합상태판.md)에서 확인한다.

```text
Mirror 거울
├─ 커뮤니티·실제 업무
│  ├─ 정보 공개·참여·협력·공동 원장
│  ├─ 역할별 도구: Community / Orderer / Shipper / Driver / Warehouse
│  └─ 운영 서버: 권한·명시적 동의·상태 전이·저장·Event
├─ 게임 Simulation — 같은 Core, 실제 업무와 별도 상태
│  ├─ 실행: Solo LocalProcess / Hosted RemoteHost
│  ├─ 규칙: Actor·NPC·자원·시간·작업·성장·경제
│  ├─ 플레이 영역 — 각자 독립 폐루프, 영역 간 연결은 별도
│  │  ├─ Nature: 탐험·채집·생존·회복
│  │  ├─ Farm: 경작·수확·보관·위임
│  │  ├─ Hub: 물류 집적·입출고·수요 연결
│  │  ├─ Town: 생활·가공·교류
│  │  └─ City: 도시 업무·시장·금융 관련 기획
│  └─ 권위 기록: Command·ActionRecord·Revision·Save/Replay
├─ Unity — 하나의 canonical SimulationWorldShell
│  ├─ 입력: 1인칭·3인칭·관리 시점·선택·UI
│  ├─ 공간: 배치 계획 소비·LH 셀 수명·객체 조립
│  ├─ 표현: Synty 자산·캐릭터 동작·Sky·Audio·FX
│  └─ 검증: 실제 입력·Game View·통행·재진입
└─ 현실 자료와 게임의 선택형 연결
   ├─ 원천: 공공 자료·향후 외부 상품 자료 후보
   ├─ 중간 해석: 출처·단위·상품 대응·권리·운영자 검토
   └─ 사용자: 관심이 있을 때 현실 자료 살펴보기
      ※ 게임 진행 필수 아님 / 실제 수집·게시·구매 자동 실행 아님
```

현재 대표 표본은 Nature의 생존·회복, 한스의 숲 경계 Farm 생활, 현실의 창고·상하차·운송을 비추는 Hub 물류다. 이들은 구현 순서가 아니라 독립 폐루프이며, Nature 정착이나 Farm 출하를 Hub·Town·City의 필수 선행 상태로 만들지 않는다. 플레이어는 직접 행동하거나 NPC에게 권한과 자원을 위임할 수 있지만, 실제 결과는 같은 Simulation 권위 규칙과 행위 기록으로 판정한다.

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
4. [기획 목차](docs/AI/PLANNING.md)에서 이야기·인물·사건의 현행 기획을 찾는다. 괘·효는 [스토리 영감과 플레이 진행 분리](docs/Architecture/스토리영감과플레이진행분리.md)에 따라 선택적으로 참고하며 사건 수·제작 순서·플레이 순서를 강제하지 않는다.
5. 게임 작업이면 [현재 Codex PlayableLoop Goal](docs/AI/generated/codex-playable-loop-goals.md)에서 활성 폐루프·WI·차단·다음 의존성을 확인한다.
6. [전체 문답 정리 상태판](docs/Architecture/PlayableLoops/PlanningSessions/문답정리상태판.md)과 [문답 기록 routing](docs/Architecture/PlayableLoops/PlanningSessions/README.md)에서 해당 주제의 질문·답변·남은 미정을 읽는다.
7. 활성 Goal이 참조하는 `Approved` 기획서와 E7 작업 명세만 구현 입력으로 사용한다.
8. 코드 위치는 [Simulation·Unity 코드 지도](docs/AI/generated/simulation-unity-code-map.md)에서 찾고, 완료 여부는 [현재 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md)에서 확인한다.

## 문답에서 개발까지

게임 기획은 한 번에 거대한 명세를 확정하지 않는다. D398부터 기본은 **지금·여기·나·너·이렇게의 상황에서 핵심 질문 하나를 깊게 검토하는 방식**이다. 기존 답변의 중복을 확인하고 추천·대가를 제시한 뒤 답변에 따라 다음 선택으로 이어간다. 묶음 질문은 사용자가 요청할 때만 사용하며 과거 묶음 승인은 보존한다. 추천은 승인 전까지 제안이고, 질문은 특정 성장 체계에만 치우치지 않도록 실제 플레이의 공백을 기준으로 고른다.

`이렇게`에는 플레이어가 무엇을 어떻게 하는지 적는다. 다른 유효한 선택을 지우지 않으며, 승인된 기획은 `docs/AI/Planning/<분야>/<PLAN-ID>/README.md`를 향해 단계적으로 정본화한다. 권위 상태를 바꾸는 역할 객체의 행동은 적용 가능한 오행 관계를 E1~E4에서 설명해야 하지만, 오행을 다섯 칸 모두 채우거나 분류만으로 E5를 통과시키지 않는다.

```text
기존 문답 검색·미정/중복 확인
  → 지금·여기·나·너·이렇게의 핵심 질문 하나 + 추천·대가
  → 사용자 답변·승인 또는 수정
  → 결과·다음 선택·실패·회복·귀환 정리
  → 주체·WI·오행·H·권위·표현·저장 영향 기록
  → 준비된 주체와 WI 하나의 직접 결과를 결속하고 필요한 경우에만 PlayableLoop 연결
  → Approved 기획 revision + hash
  → 공간 영향이 확인된 경우에만 Graph Map L1·L2·L3와 별도 배치 맵 영향 판정
  → E4 후보·자산·가공 필요·fallback과 작업 명세 결속
  → 개발 스레드에 좁은 에비던스 상한으로 인계
  → 코드·시험·Runtime 증거 반환
  → 다음 질문 또는 가장 이른 E 단계 재개
```

질문 균형은 단순히 분야별 질문 수를 맞추는 일이 아니다. 플레이어 경험, Simulation 규칙, WI·Task, 조작·카메라, H 공간, 건물·배치·자산, 캐릭터·NPC·애니메이션, UI·Audio·FX, 성장·경제, 외부 자료, Save·온라인, Unity 조립·성능, 시험·Game View·빌드의 공백을 살핀다. 세부 기준은 [PlayableLoop 문답 정밀화 체계](docs/Architecture/PlayableLoop문답정밀화체계.md)를 따른다.

현재 문답은 주제별로 분리돼 있다.

Q 번호는 질문의 식별자이고, 개발 Queue는 승인·선행 의존성·담당 경로를 가진 작업 목록이다. Q 번호를 순회해 WI를 찾되 질문 한 건을 코드 한 개나 E5 완료 한 건으로 세지 않는다. 기존 Q001~339와 Q340~403 및 의미 후속의 조회 연결은 [전체 문답 E5 세계 통합 계획](docs/AI/전체문답-E5세계통합계획-2026-08-30.md)과 기존 구현 원장에서 관리한다.

- [Q-001~339 전체 문답 정리 상태판](docs/Architecture/PlayableLoops/PlanningSessions/문답정리상태판.md)
- [질문별 기획·구현·시험·Runtime·Evidence 점검 원장](docs/AI/generated/playable-loop-inquiry-implementation-scope.md)
- [Nature 거점·수면·날씨·방어](docs/Architecture/PlayableLoops/PlanningSessions/Nature거점수면/nature-shelter-sleep.inquiry.r1.md)
- [플레이어 내면·명상·계획](docs/Architecture/PlayableLoops/PlanningSessions/플레이어내면명상/player-mind-meditation.inquiry.r1.md)
- [Nature 자원·LandUse·건설](docs/Architecture/PlayableLoops/PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md)
- [약초·Recipe·조합 제작](docs/Architecture/PlayableLoops/PlanningSessions/약초Recipe제작/herbal-recipe-crafting.inquiry.r1.md)
- [저장·Load·재진입](docs/Architecture/PlayableLoops/PlanningSessions/저장재진입/save-load-runtime.inquiry.r1.md)

## 게임 개발 체계 용어

| 체계 | 답하는 질문 | 핵심 경계 |
| --- | --- | --- |
| `Subject` | 누가 또는 무엇이 상태를 가지고 상호작용하는가? | 역할·권위·초기 해석·Save/Replay 정체성을 먼저 준비하며 Runtime 인스턴스 증거는 아님 |
| `WI Goal` | 두 주체 사이에서 한 번에 어떤 의미 있는 상태를 바꾸는가? | Goal 하나가 WI 하나와 직접 결과를 소유 |
| `PlayableLoop` | 여러 WI의 성공·실패·회복·귀환을 함께 반복 검증해야 하는가? | 필요한 경우에만 연결하는 선택적 검증 묶음 |
| `오행 관계` | 주체가 무엇을 어떤 작용으로 대하는가? | WI 탐색 메타데이터이며 능력치·보상·E 승격을 자동 결정하지 않음 |
| `H1~H5` | 행동 공간부터 세계 배치까지 무엇을 어떻게 포함하는가? | 공간 조립 깊이이며 에비던스 성숙도와 별개 |
| `Graph Map` | 플레이 관계와 공간·코드 의존성이 어떻게 연결되는가? | L1 플레이 관계, L2 배치 제약, L3 코드·컴포넌트 결속 |
| `배치 맵` | 실제 배치 전에 무엇을 어디에 어떤 제약으로 놓을 것인가? | H 구성·상대 위치·통행·시야·자산 후보를 판본화하며 Scene이 아님 |
| `E1~E10` | 논리와 표현이 실제 증거로 어디까지 검증됐는가? | 통합 E는 Logic·Presentation 중 낮은 단계 |
| `G1~G5` | 각 에비던스 구간을 어떤 관리 체계로 통과시키는가? | G 완료가 E 승격을 자동 의미하지 않음 |
| `EvidencePackage` | 어떤 revision·환경·시험·화면이 무엇을 증명하는가? | 코드·시험·Runtime·Game View·운영 증거를 분리 |

### H 공간 포함 체계

```text
H5 세계 배치 — AreaSet 인스턴스·물리 회랑
└─ H4 위치 독립 AreaSet 청사진을 적용한 영역
   └─ H3 경관·이동·업무 폐루프
      └─ H2 여러 행동 공간의 블록
         └─ H1 행동·작업 공간
```

플레이어는 H1을 직접 배치·복구·연결할 수 있고 H2·H3의 성장을 유도할 수 있다. 상위 공간의 실제 성립은 필요한 WI·연결·용량·폐루프를 Simulation 규칙이 판정한다. 자세한 정의와 현재 검증 상태는 [H1~H5 공간 포함 계층 조사](docs/Architecture/H1-H5공간포함계층조사.md)를 따른다.

### 에비던스 체계

`E`는 기능 개수나 공간 크기가 아니라 **Evidence, 즉 검증된 증거의 성숙도**를 뜻한다.

#### 변경 전후 안내 — 과거 문서를 읽는 경우

2026-08-31 확인 기준, 현재 증거 모델은 `horizontal-dual-cycle-evidence.r3`다. 과거 `legacy-change-adaptive.r10`과 같은 E 번호를 사용하더라도 의미와 판정 대상이 다르므로 문서의 모델 판본을 먼저 확인한다.

| 항목 | 과거 체계·기록 | 현재 체계에서 읽는 방법 |
| --- | --- | --- |
| E8 | NPC 생활세계 | **개별 플레이 폐루프 반복 안정성**. NPC 생활 연속성은 관련 E9 조화 검증 항목으로 다룸 |
| E9 | 변화 적응 | **여러 안정 폐루프의 영역 조화·사람 승인**. 변경 영향·Migration·호환·회귀는 모든 단계의 교차 책임 |
| 수직 작업 명세 | 과거 `.e9-work-order.json` 및 E9 하향식 자료 | 새 PlayableUnit·Goal은 **E7까지**. E8 안정성·E9 영역 조화·E10 제한 운영은 별도 캠페인 |
| 기존 증거 | 과거 모델에서 기록된 E8/E9 결과 | 이력·읽기 호환으로 보존. 현재 E8/E9로 자동 변환하거나 합산하지 않음 |

E1~E7은 아래 표의 **현행 정의**로 읽는다. 특히 E4의 자산 후보·배치 의도 조사는 E5 실제 배치·상태 발현과 다르고, E3 시험 통과는 E7 실제 플레이 완료를 대신하지 않는다. 각 WI Goal은 `Logic`과 `Presentation`을 따로 평가하며 통합 E는 두 값 중 낮은 단계다. 여러 WI의 반복 폐쇄성을 함께 검증해야 할 때만 PlayableLoop를 추가한다. 예를 들어 논리 E5·표현 E3이면 통합은 E3이다. 이는 설명용 예시이며 현재 프로젝트 달성 수치가 아니다.

기준 정의는 [현행 설계와 호환성 안내](docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md), 기계 판독 값은 [현재 단계 원장](eng/execution-ledgers/evidence-stages.json), 이전 정의는 [과거 r10 원장](eng/execution-ledgers/compatibility/evidence-stages.legacy-r10.json)에서 확인한다. **체계 설명이 갱신됐다는 사실은 기능의 E 승격을 뜻하지 않는다.** 실제 달성·차단은 [Goal 상태판](docs/AI/generated/codex-playable-loop-goals.md)과 [개발 통합 상태판](docs/AI/개발통합상태판.md)을 따른다.

#### 현재 단계 요약

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

각 WI Goal은 주체 기반을 먼저 확인하고 E7에서 E1로 영향을 내려 검토한 뒤, 가장 낮은 미완료 의존성을 구현하고 E1에서 E7로 다시 조립한다. 결함이 발견되면 같은 Goal에서 가장 이른 책임 단계로 돌아간다. E8~E10은 E7 뒤의 별도 수평 캠페인이다. 현재 기준은 [주체·상호작용 중심 개발 체계](docs/Architecture/주체상호작용중심개발체계.md)와 [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md)다.

표현 제작도 별도 단계가 아니라 같은 Presentation E1~E7 안에서 관리한다. E1 요구 → E2 코드 → E3 자동시험 → E4 자산·필요 가공/배치 준비 → E5 실제 상태·장면 → E6 품질 정제 → E7 실제 입력·결과·귀환이다. [표현 최소 모듈 기준](docs/Architecture/플레이폐루프논리시각이중순환체계.md#단계별-최소-구현-책임-d-386)과 [필요 모듈·구현·증거 연결 상태판](docs/AI/generated/playable-loop-presentation-validation.md)에서 확인한다. 이전 검증 목록 중심 대장에 구현/시험 참조와 미연결 상태를 추가했으며, 과거 E와 게임 Logic·Save는 변경하지 않는다.

E6까지의 주력 작업은 작은 WI의 기획을 Graph Map L1 플레이 관계·L2 배치 제약·L3 코드 결속으로 검토하고, 배치 맵과 표현 후보를 준비한 뒤 E5 연결·E6 정제를 확인한다. 레벨은 H/E 단계가 아니며 실제 완료는 해당 증거로 판정한다. [Graph Map 인계 기준](docs/Architecture/GraphMap기획인계순환체계.md)을 따른다.

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

[Mirror Unity](https://github.com/cheolwo/unity)는 Mirror의 Nature 생존과 Farm·Hub·Town·City의 독립적인 생활·업무 폐루프를 공간과 상호작용으로 검증하는 Unity 프로젝트입니다. 고유 식별자와 권위 revision을 유지하면서 `Preview → Confirm → Task·Realtime·WorldTick → 최신 상태 재조회` 흐름을 게임 월드에서 표현합니다. 영역 간 운송은 각 영역의 내부 폐루프가 성립한 뒤 선택하는 별도 통합 작업입니다.

<p align="center">
  <a href="https://github.com/cheolwo/unity">
    <img src="https://github.com/cheolwo/unity/raw/refs/heads/main/Documentation/Changes/2026-08-11-harvest-route-multi-lot/harvest-route-multi-lot-selection.png" alt="감자 수확물 판로 선택 Unity Simulation Game View" width="900">
  </a>
</p>

> 현재 Unity 화면은 개발용 Simulation입니다. 실제 판매·결제·배차·수출·정산을 실행하지 않으며, 운영 상태의 최종 권위는 서버에 있습니다. 게임 Simulation Core는 Solo에서 Unity 내부 Local Runtime, Hosted Multiplayer에서만 Simulation 서버가 실행합니다.

세계 구축은 준비된 주체 사이의 WI와 필요한 H 공간 능력에서 시작한다. 배치·실행 엔진은 이를 결정적인 세계 계획으로 조립하고, 공공데이터는 현실 근거, Synty 자산은 표현 후보로 사용한다.

```text
공간 실행 파이프라인 — 접근·갱신·이탈·재진입을 함께 조율
├─ Simulation: WI·Task·Effect·권위 상태와 WorldRevision
├─ 배치 엔진: 기능·점유·통행·경관 계획과 검증
├─ LH: 셀 준비·활성·캐시·해제·재진입
├─ Unity 조립: 실제 Prefab·Renderer·Collider·입력 연결
└─ 표현·검증: Sky·캐릭터·UI·Audio → EvidencePackage
```

이 트리는 책임 분류이며 모든 엔진을 매번 직렬 호출한다는 뜻이 아니다. [D364 공간 실행 조율 기준](docs/Architecture/지도구성과세계자산배치분리.md)에 따라 하나의 요청 수명을 관리하되 권위·배치 계획·셀 수명·실제 조립을 분리한다. 화면에서 객체를 해제해도 작물·재고·NPC의 권위 상태를 삭제하지 않는다.

이 과정에서 Synty 원본 팩은 출처이지 게임 영역이나 기능의 권위 분류가 아닙니다. 자산은 지면·식생·실외 구조·실내 설비·도구·건설 상태 같은 기능 역할로 분류한 뒤 WI의 현재 권위 상태를 표현할 때 선택합니다. Prefab이 생성됐다는 사실만으로 자원·건물·NPC 상태가 바뀌지는 않습니다.

현재 13팩의 환경·건물·소품 Prefab과 이동·감정·검 전투 Animation Clip은 서로 다른 원천 대장으로 관리합니다. 구체적인 WI·H 결속, Rig·Avatar·Controller 호환, fallback과 E4→E5 검증 기준은 [플레이 폐루프 Synty 표현 모듈 체계](docs/Architecture/플레이폐루프Synty표현모듈체계.md)를 따릅니다.

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

Mirror의 Simulation·Unity 작업은 현재 목표와 증거 상태에서 시작해 플레이어가 이해할 수 있는 가장 작은 선택 폐루프를 고릅니다. 한 `PlayableUnit`은 E7 플레이 약속부터 E1 계약까지 하향 분해하고, 가장 낮은 미완료 의존성을 구현한 뒤 E1부터 E7까지 실제 증거를 다시 확인합니다. 완성된 각 E7은 별도 E8 반복 안정성 캠페인으로 검증하고, 같은 영역의 안정 Core 둘 이상은 E9 영역 조화·사람 승인, E10 제한 운영 캠페인으로 검증합니다.

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
- [플레이 폐루프 완결 로드맵](docs/Architecture/플레이폐루프완결로드맵.md): 영역별 Core 우선·Extension 후속과 영역·세계 집계. 영역 나열을 필수 이동 경로나 선행 의존성으로 해석하지 않음
- [WI 괘성 분류 체계](docs/Architecture/WI괘성분류체계.md): 주체·행동·대상·보조의 오행 관계와 E5 진입 전 역할 객체 관문
- [Graph Map 기획 인계 순환 체계](docs/Architecture/GraphMap기획인계순환체계.md): 승인 기획을 L1·L2·L3와 별도 배치 맵 영향으로 구조화하는 책임 경계
- [Presentation E4 후보 풀](docs/AI/Planning/표현/PLAN-PRESENTATION-E4-POOL-001/README.md): 여러 표현 후보의 자산·가공·배치 준비와 E5 선택 경계
- [게임 개발 업무 순서 기준](docs/Architecture/게임개발업무순서기준.md): 작업 선택부터 다음 판단까지의 실행 순서
- [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](docs/Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md): 단일 폐루프·영역 조화·사람 플레이테스트에서 Logic·Presentation 왕복과 제한 운영의 판정 주체·관문
- [WorldTick과 실시간 실행 경계](docs/Architecture/WorldTick과실시간실행경계.md): Unity 표현 시간·권위 실시간 시계·WorldTick·BattleTick·WorldRevision의 책임 구분
- [E 성숙도 책임 코드 지도](docs/Architecture/SsalddelCodeMetadata.md#e-성숙도-책임-메타데이터): Simulation·Unity 구성 요소를 현재 E1~E10 검토 책임에 연결하고 과거 E8·E9 의미를 판본으로 분리하는 기준
- [WI 성숙도 현재 지도](docs/AI/generated/world-interaction-maturity.md): 등록 WI의 선택 여부, E4 문맥, 조건부 H 근거와 E5 발현 상태. 개수·상태는 생성 원장 기준
- [현재 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md): 완료·부분 완료·미완료·보류 구분

문서, 코드, 자동 시험, Actual E5, 실제 서버, Play Mode·Game View와 운영 효과는 서로 다른 `EvidencePackage`로 기록합니다. Farm·Hub·Town·City는 각각 `PlayableLoop` 독립 내부 폐루프를 먼저 만들고 영역 간 연결은 양쪽이 준비된 뒤 별도 통합 작업으로 선택합니다. 현재 폐루프와 증거 상태는 [자동 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md)에서 확인합니다.

## 개발 책임과 짧은 작업 흐름

```text
기획 — 사용자 문답·확정 결정·연구 검토
└─ 개발 — 승인 작업 Queue·공유 계약·경로 소유·최종 통합
   ├─ Simulation / 운영 담당: 각각의 권위 규칙·API·저장·시험
   ├─ 월드·공간·배치: 배치·통행·실제 입력·Game View 캡처
   ├─ 애니메이션: 보유 동작 대조·리그·접촉·Blender 전문 산출물
   └─ 자료 조사: 원천·출처·권리·단위·게임 대응 조사
      각 전문 결과 → 개발 검토·통합 → 기획에 실제 결과/미완료 반환
```

이는 작업 인계 구조이지 앱의 물리적 부모·자식 스레드 구조가 아니다. 독립 경로는 병렬 진행하되 같은 파일·공유 계약과 Unity Editor는 소유를 조율한다. 담당 수나 고정 WIP 1개 제한으로 독립 작업을 막지 않는다. [Goal 운영 체계](docs/Architecture/CodexPlayableLoopGoal운영체계.md)를 따른다.

### 최근 변경을 찾는 곳

커밋된 기능에는 한스 농장 울타리 복원, 이데아 맵·NPC 학습 중점, 캠페인 진행·실패 복원이 포함된다. 수뢰둔·산수몽 기획은 이 이야기들의 참고 맥락이며 전체 게임의 고정 진행표가 아니다. 코드·자동시험 범위와 남은 Unity 표현은 [현재 작업](docs/AI/CURRENT_WORK.md)에서 확인한다.

```text
개발 통합 상태판 — 실제 성과와 남은 연결의 최신 기준
├─ 기획: PLAN 정본·원자 E1·문답과 미정
├─ 구조: WI·오행·H·Graph Map L1·L2·L3
├─ 배치·표현: 배치 맵·Synty/가공 후보·Presentation E4
├─ 구현: Operations·Simulation·Unity의 독립 책임
├─ 통합: 같은 Goal·Session·객체·Revision의 인계
└─ 증거: 시험·Runtime·E5·Game View·Save/Replay의 분리 판정
```

위 가지는 진행 중인 작업을 찾는 분류다. 코드·시험이 있어도 Session/Save·실제 입력·Game View가 남을 수 있다. [개발 통합 상태판](docs/AI/개발통합상태판.md), [전체 문답 E5 계획](docs/AI/전체문답-E5세계통합계획-2026-08-30.md), [애니메이션·리깅 계획](docs/Architecture/PlayableLoops/WI애니메이션-리깅정밀화계획.md)에서 구체 근거와 승인 범위를 확인한다. 후자의 신규 제작 순위는 검토안이며 구현 완료 목록이 아니다.

### 저장소·문서·코드 구조

아래는 주요 실제 경로만 남긴 탐색 트리다. 세부 파일은 [생성 코드 지도](docs/AI/generated/simulation-unity-code-map.md)로 찾는다.

```text
Hongdal/                         cheolwo/mirror 저장소
├─ README.md                     전체 구조의 진입점
├─ docs/
│  ├─ ProjectOverview/           프로젝트·화면 안내
│  ├─ Architecture/              기준 설계
│  │  └─ PlayableLoops/           주제 기획·전문 연구
│  │     └─ PlanningSessions/     과거 문답·주제별 참조 기록
│  ├─ AI/                        DECISIONS·CURRENT_WORK·개발 통합 상태판
│  │  ├─ PLANNING.md             현행 기획 탐색·판본·관계
│  │  ├─ Planning/               이야기·사건별 PLAN 정본
│  │  │  └─ 스토리/              한스 농장·학습·캠페인, 괘·효 참고 이력
│  │  └─ generated/              원장에서 생성한 Goal·WI·코드 지도
│  ├─ Reports/                   검토·구현·검증 결과
│  └─ Research/GameData/         현실 자료 조사·개발 인수 기록
├─ eng/
│  ├─ execution-ledgers/         실행·증거 원장과 관리 도구
│  │  └─ work-orders/            E7 작업 명세·승인/연구 결속
│  ├─ planning-inquiries/        문답 검색 원천·도구
│  └─ work-areas/                책임·탐색 범위
├─ Ssalddel/                     운영 서버
├─ Ssalddel.Contracts/           공통 업무 계약
├─ Ssalddel.Ui.Common/           공통 업무 UI
├─ Ssalddel.Simulation.Contracts/ 게임 계약
├─ Ssalddel.Simulation.Domain/    게임 규칙
├─ Ssalddel.Simulation.Application/ 실행·Session·Adapter
├─ Ssalddel.Simulation.Infrastructure/ 기반 연결
├─ Ssalddel.Simulation.Persistence/ 저장 기반
├─ Ssalddel.Simulation.Server/    Hosted 실행
└─ Ssalddel.Simulation.Tests/     Simulation 시험

ssalddel/                        별도 cheolwo/unity 저장소
├─ Assets/Ssalddel/              프로젝트 입력·조립·표현·시험
├─ Assets/Synty/                 보유 원본 자산·애니메이션
├─ Packages/                    패키지 의존성
└─ ProjectSettings/             Unity 프로젝트 설정
```

원장·기획의 존재는 해당 저장/실행 기능의 완성을 보증하지 않는다. 생성 문서를 직접 수정하지 않으며 원장/도구에서 갱신한다.

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

### 커밋을 읽고 묶는 기준

커밋 수가 많을 때는 단순히 날짜나 파일 수로 합치지 않고 되돌릴 수 있는 책임 단위로 묶어 읽는다. 원장과 그 원장에서 생성한 문서는 같은 맥락에 두되, 기획 의미·Graph Map·배치/표현·Simulation·운영 자료·통합 검증은 서로 분리한다.

| 권장 묶음 | 함께 둘 내용 | 분리할 내용 |
| --- | --- | --- |
| 기획 정본 | `PLAN-*` 본문, `PLANNING.md`, 직접 계보 | 코드 구현, Unity 실행 증거 |
| 구조·Graph Map | 노드·엣지·제약, 인계 원장과 생성 결과 | Scene·Prefab 실제 배치 |
| 배치·Presentation | 배치 맵, 자산 후보, 가공/fallback, E4 검사 | 권위 게임 규칙 |
| Simulation | 계약·도메인·Session·Save/Replay·시험 | 운영 DB와 외부 자료 수집 |
| Operations·자료 | 서버 업무, 검토된 공공자료, 영속 투영 | 게임 효과 자동 활성화 |
| 통합·검증 | Adapter, 책임 지도, 상태 snapshot, 검증 보고 | 독립 기능의 큰 소스 변경 |

이미 공유된 커밋은 책임별 변경으로 읽는다. 게시할 때는 README와 연결 문서를 같은 커밋 묶음에 포함하고 `eng/tests/readme-navigation.ps1 -Revision HEAD`로 커밋 내부의 탐색 경로를 확인한다.

## 개발

```powershell
dotnet build Ssalddel.v0.0.slnx
dotnet test Ssalddel.Tests/Ssalddel.Tests.csproj
```

전체 문서의 기준과 분류는 [문서 안내](docs/README.md), 화면과 코드 위치는 [프로젝트 화면 안내](docs/ProjectOverview/README.md)와 [화면 카탈로그](docs/ProjectOverview/app-page-catalog.md)에서 확인할 수 있습니다.
