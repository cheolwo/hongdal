# 역경 효사 기획 H 통합 참조 색인

> 효사 기획이 참조한 H1~H4를 한곳에서 찾기 위한 자동 생성 색인이다. 새 H를 만들거나 채택·배치·Evidence를 승격하지 않는다.

| 계층 | 대상 | 역할 | 해소 상태 | 조합 근거 |
| --- | --- | --- | --- | --- |
<a id="ref-h-h1-h1-stock-farm-fence-edge"></a>
| `H1` | `h1-stock:farm-fence-edge` | `FarmFenceDefenseChokepoint` | `ExistingReused` | 없음 |
<a id="ref-h-h1-h1-stock-farm-production"></a>
| `H1` | `h1-stock:farm-production` | `FarmProductionObjective`, `FarmProductionPlotContext`, `ManagedPotatoCultivationPlot`, `PotatoCultivationPlotAuthorityBoundary` | `CandidateNeedsReview`, `ExistingReused` | 없음 |
<a id="ref-h-h1-h1-stock-farm-residential-home"></a>
| `H1` | `h1-stock:farm-residential-home` | `FarmhouseDefenseObjective`, `FarmLivingHomeAuthorityBoundary`, `FarmLivingHomeWithGuestAnchors`, `FullyRepairedFarmLivingHome` | `ExistingReused` | 없음 |
<a id="ref-h-h1-h1-stock-farm-tool-storage"></a>
| `H1` | `h1-stock:farm-tool-storage` | `FarmToolStorage` | `CandidateNeedsReview` | 없음 |
<a id="ref-h-h1-h1-stock-nature-emergency-retreat"></a>
| `H1` | `h1-stock:nature-emergency-retreat` | `EmergencyRetreat` | `ExistingReused` | 없음 |
<a id="ref-h-h1-h1-stock-nature-incident-trace"></a>
| `H1` | `h1-stock:nature-incident-trace` | `IncidentTrace` | `ExistingReused` | 없음 |
<a id="ref-h-h2-h2-candidate-farm-boundary-defense-recovery"></a>
| `H2` | `h2-candidate:farm-boundary-defense-recovery` | `FarmBoundaryDefenseRecovery` | `ExistingReused` | `h1-stock:farm-fence-edge`<br>`h1-stock:farm-residential-home`<br>`h1-stock:nature-emergency-retreat`<br>`h1-stock:nature-incident-trace` |
<a id="ref-h-h2-h2-candidate-forest-edge-living-farm"></a>
| `H2` | `h2-candidate:forest-edge-living-farm` | `ForestEdgeFarm` | `ExistingReused` | `h1-stock:farm-fence-edge`<br>`h1-stock:farm-production`<br>`h1-stock:farm-residential-home` |
<a id="ref-h-h2-h2-candidate-nature-threat-response"></a>
| `H2` | `h2-candidate:nature-threat-response` | `ThreatResponse` | `ExistingReused` | `h1-stock:nature-emergency-retreat`<br>`h1-stock:nature-incident-trace` |
<a id="ref-h-h3-h3-candidate-forest-edge-living-farm-campaign"></a>
| `H3` | `h3-candidate:forest-edge-living-farm-campaign` | `ForestEdgeLivingFarmCampaign` | `ExistingReused` | `h2-candidate:farm-boundary-defense-recovery`<br>`h2-candidate:forest-edge-living-farm` |

- 후보·표현 배당 참고: [H1 Synty 표현 배당](h1-synty-representation-assignments.md)
