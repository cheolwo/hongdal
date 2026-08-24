using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldExplorationCodes
    {
        public const string RuleRevision = "world-building-item-rule.pyeongchang-l2.r1";
        public const string DaegwallyeongRegion = "region:kr:administrative:5176038000";
        public const string RegionalFeature = "RegionalFeature";
        public const string UniversalSupply = "UniversalSupply";
        public const string PendingRuleEvaluation = "PendingRuleEvaluation";
        public const string Eligible = "Eligible";
        public const string Ineligible = "Ineligible";
        public const string PlayerInsideBuilding = "PlayerInsideBuilding";
        public const string FarmExplorationActive = "FarmExplorationActive";
        public const string HarvestContextActive = "HarvestContextActive";
        public const string SupplyInspectionActive = "SupplyInspectionActive";
        public const string RegionalEvidenceReviewActive = "RegionalEvidenceReviewActive";
        public const string PublicLicensedBusinessDataMissing =
            "PublicLicensedBusinessDataMissing";
        public const string SimulationScenario = "SimulationScenario";
        public const string LicensedBusinessSource = "LOCALDATA_PUBLIC_LICENSED_BUSINESS";
        public const string TileNotFound = "SimulationWorldStreamTileNotFound";
        public const string AnchorMissing = "SimulationWorldBuildingItemAnchorMissing";
        public const string EnteredBuildingNotFound =
            "SimulationWorldEnteredBuildingNotFound";
        public const string EnteredBuildingStableIdMissing =
            "SimulationWorldEnteredBuildingStableIdMissing";
        public const string ConditionMissing = "SimulationWorldBuildingItemConditionMissing";
        public const string ExpectedRevisionMismatch = "SimulationExpectedRevisionMismatch";
    }

    public static class PyeongchangWorldExplorationFixtureIds
    {
        public const string DaegwallyeongFarmCenterTile = "kr5186:l2:700:1145";
        public const string Barn = "scenario-object:pyeongchang-farm:barn-a";
        public const string Silo = "scenario-object:pyeongchang-farm:silo-a";
        public const string PotatoSample = "produce.potato.sample";
        public const string BasicWaterSupply = "supply.water.basic";
        public const string RegionalEvidenceNote = "guide.regional-evidence-note";
    }

    /// <summary>
    /// L2 타일 안에서 건물과 아이템 후보가 어떤 조건으로 연결되는지 제공한다.
    /// 이 응답은 아이템을 생성하거나 획득 상태를 변경하지 않는다.
    /// </summary>
    public sealed class SimulationWorldBuildingItemRulePackageResponse
    {
        public string TileKey { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string RelationHashSha256 { get; set; } = string.Empty;
        public SimulationWorldRegionExperienceResponse Region { get; set; }
            = new SimulationWorldRegionExperienceResponse();
        public SimulationWorldBuildingItemRelationResponse[] BuildingItemRelations { get; set; }
            = Array.Empty<SimulationWorldBuildingItemRelationResponse>();
        public SimulationWorldObservedPlaceResponse[] ObservedPlaces { get; set; }
            = Array.Empty<SimulationWorldObservedPlaceResponse>();
        public SimulationWorldDataGapResponse[] DataGaps { get; set; }
            = Array.Empty<SimulationWorldDataGapResponse>();
        public bool CreatesItemInstances { get; set; }
        public bool ChangesSimulationState { get; set; }
        public bool PresentationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldBuildingItemRelationResponse
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string AnchorObjectStableId { get; set; } = string.Empty;
        public string AnchorObjectTypeCode { get; set; } = string.Empty;
        public string AnchorSocketCode { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string ItemKindCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string EvidenceKorean { get; set; } = string.Empty;
        public string[] RequiredConditionCodes { get; set; } = Array.Empty<string>();
        public double SocketOffsetXMeters { get; set; }
        public double SocketOffsetYMeters { get; set; }
        public string InitialStateCode { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationWorldRegionExperienceResponse
    {
        public string RegionStableId { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string RegionRoleCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string RegionalContextLabel { get; set; } = string.Empty;
        public string LimitationKorean { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldObservedPlaceResponse
    {
        public string PlaceStableId { get; set; } = string.Empty;
        public string PublicDisplayName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public bool PublicDisplayAllowed { get; set; }
    }

    public sealed class SimulationWorldDataGapResponse
    {
        public string GapCode { get; set; } = string.Empty;
        public string KoreanMessage { get; set; } = string.Empty;
        public string RequiredSourceKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldBuildingItemEligibilityPreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string EnteredBuildingStableId { get; set; } = string.Empty;
        public string[] ActiveSimulationConditionCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorldBuildingItemEligibilityPreviewResponse
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public long ObservedWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public SimulationWorldBuildingItemEvaluationResponse[] Evaluations { get; set; }
            = Array.Empty<SimulationWorldBuildingItemEvaluationResponse>();
        public bool HasEligibleCandidate { get; set; }
        public bool StateChanged { get; set; }
        public bool SimulationOnly { get; set; }
    }

    public sealed class SimulationWorldBuildingItemEvaluationResponse
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string AnchorObjectStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string EligibilityStateCode { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public string[] MissingConditionCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }
}
