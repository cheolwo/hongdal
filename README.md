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

현재 공공데이터 기반 농장 생존 방향은 다음 트리로 정리되어 있습니다.

```text
공공데이터 지형·법정동·건물
└─ Simulation Session
   ├─ 플레이어 직접 노동
   ├─ NPC 위임 노동
   ├─ 농장 방어
   ├─ 좀비·가상 약탈자 사건
   ├─ 회복 가능한 피해
   └─ Save / Replay
      └─ Unity PresentationKey / VisualKey
         ├─ 현재 보유 Synty fallback
         └─ 향후 Apocalypse·Alpine 연결
```

설계·구현 경계와 첫 7일 수직 단위는 [공공데이터 기반 Synty 농장 생존 Simulation 기획과 구현 기준](docs/Architecture/PublicDataSyntyFarmSurvivalGamePlan.md)을 따릅니다.

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
