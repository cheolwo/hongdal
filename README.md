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

> 현재 Unity 화면은 개발용 Simulation입니다. 실제 판매·결제·배차·수출·정산을 실행하지 않으며, 운영 상태의 최종 권위는 서버에 있습니다.

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
   ├─ 선택형 E6 현실 결속
   │  └─ 선택한 프로필의 DataRequirement·EvidenceBinding·DerivedArtifact
   └─ Simulation 서버
      └─ Preview → Confirm → WorldTick → 최신 상태 재조회
         └─ Unity SimulationWorldShell 표현
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

## 개발

```powershell
dotnet build Ssalddel.v0.0.slnx
dotnet test Ssalddel.Tests/Ssalddel.Tests.csproj
```

전체 문서의 기준과 분류는 [문서 안내](docs/README.md), 화면과 코드 위치는 [프로젝트 화면 안내](docs/ProjectOverview/README.md)와 [화면 카탈로그](docs/ProjectOverview/app-page-catalog.md)에서 확인할 수 있습니다.
