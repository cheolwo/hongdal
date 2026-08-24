using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 현실 자료는 게임 규칙의 직접 입력이 아니라 세션 시작 시 동결되는 의미 문맥이다.
    /// Proposal은 Incident 또는 Effect가 아니며 Unity는 플레이어용 투영만 사용한다.
    /// </summary>
    public static class SimulationRealityContextCodes
    {
        public const string SchemaVersion = "simulation-reality-context.v1";

        public const string Available = "Available";
        public const string PartiallyAvailable = "PartiallyAvailable";
        public const string Unavailable = "Unavailable";

        public const string Current = "Current";
        public const string Stale = "Stale";
        public const string Unknown = "Unknown";

        public const string Valid = "Valid";
        public const string Incomplete = "Incomplete";

        public const string WetWorkContext = "WetWorkContext";
        public const string ColdStressContext = "ColdStressContext";
        public const string CropHealthAttentionContext = "CropHealthAttentionContext";
        public const string MarketPressureContext = "MarketPressureContext";
        public const string DryForestContext = "DryForestContext";

        public const string InspectDrainage = "InspectDrainage";
        public const string ReviewFieldWorkTiming = "ReviewFieldWorkTiming";
        public const string ProtectColdSensitiveWork = "ProtectColdSensitiveWork";
        public const string InspectCropHealth = "InspectCropHealth";
        public const string ReviewShipmentTiming = "ReviewShipmentTiming";
        public const string ReviewNatureFireReadiness = "ReviewNatureFireReadiness";
    }

    public sealed class SimulationRealityContextSnapshot
    {
        public string SchemaVersion { get; set; } = SimulationRealityContextCodes.SchemaVersion;
        public string ContextSnapshotStableId { get; set; } = string.Empty;
        public string ProfileStableId { get; set; } = string.Empty;
        public int ProfileRevision { get; set; }
        public string SignalRuleRevision { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public DateTimeOffset FrozenAtUtc { get; set; }
        public string AvailabilityCode { get; set; } = SimulationRealityContextCodes.Unavailable;
        public string InputHashSha256 { get; set; } = string.Empty;
        public SimulationRealitySourceEvidenceSnapshot[] SourceEvidence { get; set; } =
            Array.Empty<SimulationRealitySourceEvidenceSnapshot>();
        public SimulationRealitySemanticSignalSnapshot[] SemanticSignals { get; set; } =
            Array.Empty<SimulationRealitySemanticSignalSnapshot>();
        public bool ChangesSimulationRules { get; set; }
        public bool MovesSpatialDefinitions { get; set; }
        public bool CreatesIncidentOrEffect { get; set; }
    }

    public sealed class SimulationRealitySourceEvidenceSnapshot
    {
        public string SourceEvidenceStableId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string AvailabilityCode { get; set; } = SimulationRealityContextCodes.Unavailable;
        public string QualityCode { get; set; } = SimulationRealityContextCodes.Unavailable;
        public string FreshnessCode { get; set; } = SimulationRealityContextCodes.Unknown;
        public DateTimeOffset? ObservedAtUtc { get; set; }
        public DateTimeOffset? RetrievedAtUtc { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string[] UnitCodes { get; set; } = Array.Empty<string>();
        public string SourceHashSha256 { get; set; } = string.Empty;
        public string LicenseCode { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationRealitySemanticSignalSnapshot
    {
        public string SignalStableId { get; set; } = string.Empty;
        public string SignalCode { get; set; } = string.Empty;
        public string SignalRuleRevision { get; set; } = string.Empty;
        public string[] H3StableIds { get; set; } = Array.Empty<string>();
        public string[] AdvisoryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceEvidenceStableIds { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Unity가 사용하는 간접 표현이다. 원 관측값·API 응답·비밀 값·필지 식별자를 포함하지 않는다.
    /// </summary>
    public sealed class SimulationRealityContextPlayerProjectionResponse
    {
        public string ContextSnapshotStableId { get; set; } = string.Empty;
        public string AvailabilityCode { get; set; } = SimulationRealityContextCodes.Unavailable;
        public DateTimeOffset FrozenAtUtc { get; set; }
        public SimulationRealityPhenomenonProjection[] Phenomena { get; set; } =
            Array.Empty<SimulationRealityPhenomenonProjection>();
        public SimulationRealitySourceInformationProjection[] SourceInformation { get; set; } =
            Array.Empty<SimulationRealitySourceInformationProjection>();
        public bool SourceDetailsIncluded { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationRealityPhenomenonProjection
    {
        public string PhenomenonStableId { get; set; } = string.Empty;
        public string PhenomenonCode { get; set; } = string.Empty;
        public string TitleKorean { get; set; } = string.Empty;
        public string SummaryKorean { get; set; } = string.Empty;
        public string[] H3StableIds { get; set; } = Array.Empty<string>();
        public string[] AdvisoryCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationRealitySourceInformationProjection
    {
        public string InformationStableId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTimeOffset? ReferenceTimeUtc { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
        public string[] LimitationSummariesKorean { get; set; } = Array.Empty<string>();
    }
}
