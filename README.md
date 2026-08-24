# 살뜰 (Ssalddel)

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

[Ssalddel Unity](https://github.com/cheolwo/unity)는 농장·수확·물류 거점·판로·도시로 이어지는 살뜰의 운영 업무를 공간과 상호작용으로 검증하는 Unity 프로젝트입니다. 생산물 stable ID와 서버 revision을 유지하면서 `Preview → Confirm → WorldTick → 최신 상태 재조회` 흐름을 게임 월드에서 표현합니다.

<p align="center">
  <a href="https://github.com/cheolwo/unity">
    <img src="https://github.com/cheolwo/unity/raw/refs/heads/main/Documentation/Changes/2026-08-11-harvest-route-multi-lot/harvest-route-multi-lot-selection.png" alt="감자 수확물 판로 선택 Unity Simulation Game View" width="900">
  </a>
</p>

> 현재 Unity 화면은 개발용 Simulation입니다. 실제 판매·결제·배차·수출·정산을 실행하지 않으며, 운영 상태의 최종 권위는 서버에 있습니다. 게임 Simulation Core는 Solo에서 Unity 내부 Local Runtime, Hosted Multiplayer에서만 Simulation 서버가 실행합니다.

현재 세계 구축은 공공데이터에서 출발하지 않습니다. 게임 플레이와 세계 의도에서 필요한 WI와 H 공간을 상향식으로 조립하고, AreaSet이 요구한 현실 근거만 E6에서 연결합니다.

```text
게임 기획과 세계 의도
├─ 플레이어 경험
│  ├─ Nature 체류·탐험·위협·회복
│  ├─ Farm 생산·수확·출하
│  ├─ City/Hub 물류·검수·보관
│  └─ Town 시장·생활·소비
├─ WI 세계 상호작용 단위
│  └─ 행위자·시작 조건·공간 요구·예약·Task·Effect
└─ 상향식 H 공간 설계 재고
   ├─ 기준 경관 문법 52개 의미군 × A/B/C = 156개 표현 변형
   ├─ H1 작업공간 모판
   ├─ H2 블록 모판
   ├─ H3 경관 모판
   └─ H4 지역 모판
      ↓
이론 공간 생산 공장
├─ H2 TheoryQualified 24개
├─ H3 TheoryQualified 13개
└─ E5TheoryQualified AreaSet 4개
   ↓
AreaSet 세계 설계
└─ LandscapeGraph 공간 조립
   ├─ Node·Edge·외부 Connector
   ├─ 공간 역할·공간 능력·업무 용량
   ├─ E6 플레이 전 정제·선택형 현실 결속
   │  └─ 선택한 프로필의 DataRequirement·EvidenceBinding·DerivedArtifact
   └─ Simulation Core
      └─ Preview → Confirm → WorldTick → 최신 상태 재조회
         ├─ Solo: Unity 내부 Local Runtime
         └─ Hosted: Simulation 서버 Host
            ↓
         Unity SimulationWorldShell 표현
            └─ E7 실제 플레이·Save / Replay 검증
```

`E5TheoryQualified`는 사람 검토 없이 이론상 공간 구조와 연결이 닫힌 상태이며 실제 지역·공공데이터·Unity Runtime 증거는 아닙니다. 공공데이터 기반 경관과 계절 방어의 상세 기준은 [공공데이터 기반 Synty 경관 생활·농장 생존 Simulation 기획과 구현 기준](docs/Architecture/PublicDataSyntyFarmSurvivalGamePlan.md)을 따르고, Unity의 상세 구조는 [Ssalddel Unity README](https://github.com/cheolwo/unity#한눈에-보는-상향식-세계-구축-구조)에서 확인합니다.

계획·코드·DB·Runtime·Game View를 같은 완료로 섞지 않기 위해, 현재 남은 작업과 증거 단계는 [Simulation·Unity 미완료 실행 트리](docs/AI/generated/simulation-unity-execution-tree.md)에서 한눈에 확인합니다. 이 트리는 `eng/execution-ledgers/simulation-unity.json`에서 자동 생성되며 첫 종단 완결 대상은 대관령 중앙 L2 `kr5186:l2:700:1145`입니다.

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

## 게임 개발 업무 순서

Ssalddel의 Simulation·Unity 작업은 현재 목표와 증거 상태에서 시작해 플레이어가 이해할 수 있는 가장 작은 선택 폐루프를 고릅니다. 그 작업을 E9 목표부터 E1 계약까지 하향 분해하고, 가장 낮은 미완료 의존성을 구현한 뒤 E1부터 E9까지 실제 증거를 다시 확인합니다.

```text
현재 목표와 차단점
  → 플레이어의 상황·선택·재료·결과·다음 선택
  → E9→E1 영향·누락 검토
  → 가장 낮은 미완료 의존성 구현
  → E1→E9 조립·증거 검증
  → 새 영향이면 다시 하향 검토
  → 안정 또는 명시적 차단까지 왕복
```

플레이어 중심은 Unity나 플레이어에게 상태 권위를 넘긴다는 뜻이 아닙니다. Simulation Core가 조건·비용·시간·결과와 H 공간 성장을 판정하고 Unity는 입력과 표현을 담당합니다. E9를 먼저 적는 것도 완료 주장이 아니라 영향과 누락을 먼저 보는 작업 순서입니다.

- [문서 안내와 질문별 기준 문서](docs/README.md): 같은 설명을 반복하지 않고 각 질문의 단일 권위를 찾는 진입점
- [프로젝트 불변 개발 골격](docs/Architecture/프로젝트불변개발골격.md): 리팩토링과 기능 개발이 보존할 기준선
- [플레이어 중심 게임 개발 업무 구조](docs/Architecture/플레이어중심게임개발업무구조.md): 모든 단계에 적용하는 플레이어 선택 관점
- [게임 개발 업무 순서 기준](docs/Architecture/게임개발업무순서기준.md): 작업 선택부터 다음 판단까지의 실행 순서
- [E9↔E1 반복 왕복 구현 체계](docs/Architecture/E9하향식수직구현체계.md): 하향 영향 검토와 상향 조립·검증을 안정 상태까지 반복하는 절차
- [E 성숙도 책임 코드 지도](docs/Architecture/SsalddelCodeMetadata.md#e-성숙도-책임-메타데이터): Simulation·Unity 구성 요소를 E1~E9 검토 책임에 연결하고, E1~E3은 사람용 하위 모듈로 다시 묶어 탐색하며 무사유 누락을 차단하는 기준
- [WI 성숙도 현재 지도](docs/AI/generated/world-interaction-maturity.md): 전체 WI 48개의 선택 여부, E4 문맥, 조건부 H 근거와 E5 발현 상태
- [현재 완료 원장](docs/AI/authority-maps/07_CURRENT_COMPLETION_LEDGER.md): 완료·부분 완료·미완료·보류 구분

문서, 코드, 자동 시험, Actual E5, 실제 서버, Play Mode·Game View와 운영 효과는 서로 다른 증거로 기록합니다. Farm·Hub·Town·City는 독립 내부 폐루프를 먼저 만들고 영역 간 연결은 양쪽이 준비된 뒤 별도 통합 작업으로 선택합니다.

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
