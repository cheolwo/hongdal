using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationWorldExplorationService
    {
        private readonly SimulationWorldStreamingService streaming;

        public SimulationWorldExplorationService(SimulationWorldStreamingService streamingService)
            => streaming = streamingService ?? throw new ArgumentNullException(nameof(streamingService));

        public SimulationWorldBuildingItemRulePackageResponse GetBuildingItemRules(string tileKey)
        {
            var context = LoadRuleContext(tileKey);
            return new SimulationWorldBuildingItemRulePackageResponse
            {
                TileKey = tileKey,
                RuleRevision = SimulationWorldExplorationCodes.RuleRevision,
                RelationHashSha256 = SimulationWorldBuildingItemRuleCatalog.HashRelations(
                    tileKey,
                    context.Relations),
                Region = CreateRegion(),
                BuildingItemRelations = context.Relations,
                ObservedPlaces = Array.Empty<SimulationWorldObservedPlaceResponse>(),
                DataGaps = CreateDataGaps(),
                CreatesItemInstances = false,
                ChangesSimulationState = false,
                PresentationOnly = true,
                IsOperationalState = false,
            };
        }

        public SimulationWorldBuildingItemEligibilityPreviewResponse PreviewEligibility(
            string sessionStableId,
            string tileKey,
            SimulationWorldBuildingItemEligibilityPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var context = LoadRuleContext(tileKey);
            if (!string.IsNullOrWhiteSpace(request.EnteredBuildingStableId)
                && !context.Buildings.Any(value => string.Equals(
                    value.ObjectStableId,
                    request.EnteredBuildingStableId.Trim(),
                    StringComparison.Ordinal)))
            {
                throw new SimulationNotFoundException(
                    SimulationWorldExplorationCodes.EnteredBuildingNotFound);
            }
            if (!streaming.TryGetActivities(tileKey, out var activities))
                throw new SimulationNotFoundException(SimulationWorldExplorationCodes.TileNotFound);

            return SimulationWorldBuildingItemRuleCatalog.Evaluate(
                sessionStableId,
                tileKey,
                activities.ActivityRevision,
                request,
                context.Relations);
        }

        private TileRuleContext LoadRuleContext(string tileKey)
        {
            if (!streaming.TryGetObjects(tileKey, out var objects))
                throw new SimulationNotFoundException(SimulationWorldExplorationCodes.TileNotFound);
            return new TileRuleContext
            {
                Buildings = objects.Objects,
                Relations = SimulationWorldBuildingItemRuleCatalog.CreateRelations(
                    tileKey,
                    objects.Objects),
            };
        }

        private static SimulationWorldRegionExperienceResponse CreateRegion()
            => new SimulationWorldRegionExperienceResponse
            {
                RegionStableId = SimulationWorldExplorationCodes.DaegwallyeongRegion,
                KoreanName = "평창군 대관령면",
                RegionRoleCode = "Farm",
                EvidenceKindCode = SimulationWorldStreamCodes.Scenario,
                RegionalContextLabel = "고랭지 농업 경관",
                LimitationKorean = "법정동 경계 Geometry가 아니라 대관령 Farm 시나리오 범위입니다.",
            };

        private static SimulationWorldDataGapResponse[] CreateDataGaps()
            => new[]
            {
                new SimulationWorldDataGapResponse
                {
                    GapCode = SimulationWorldExplorationCodes.PublicLicensedBusinessDataMissing,
                    KoreanMessage = "공개 인허가 사업체 원본이 아직 적재되지 않아 실제 상호명을 연결하지 않습니다.",
                    RequiredSourceKindCode = SimulationWorldExplorationCodes.LicensedBusinessSource,
                },
            };

        private sealed class TileRuleContext
        {
            public SimulationWorldTileObjectPlacementResponse[] Buildings { get; set; }
                = Array.Empty<SimulationWorldTileObjectPlacementResponse>();
            public SimulationWorldBuildingItemRelationResponse[] Relations { get; set; }
                = Array.Empty<SimulationWorldBuildingItemRelationResponse>();
        }
    }
}
