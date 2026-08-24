using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarmRealityEvidenceRoutes
    {
        public const string Base = "/api/simulation/v1/reality-evidence/farm-potato";
        public const string Sync = Base + "/sync";
    }

    public static class SimulationFarmRealityEvidenceCodes
    {
        public const string SchemaVersion = "simulation-farm-reality-evidence.v1";
        public const string FarmAreaSetStableId =
            "area-set:sim:pyeongchang:farm-production.v1";
        public const string PotatoProductStableId = "product:potato";
        public const string RealityContextProfileStableId =
            "reality-context-profile:sim:pyeongchang:farm-production.v1";
        public const string Confirmed = "Confirmed";
        public const string Candidate = "Candidate";
        public const string Unlinked = "Unlinked";
    }

    public sealed class SimulationFarmRealityEvidenceSyncRequest
    {
        public string AreaSetStableId { get; set; } =
            SimulationFarmRealityEvidenceCodes.FarmAreaSetStableId;
        public string CanonicalProductStableId { get; set; } =
            SimulationFarmRealityEvidenceCodes.PotatoProductStableId;
    }

    public sealed class SimulationFarmRealityEvidenceSyncResponse
    {
        public bool Inserted { get; set; }
        public string EvidenceRevision { get; set; } = string.Empty;
        public string InputHashSha256 { get; set; } = string.Empty;
        public int SourceCount { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationFarmRealityEvidence,
        SsalddelCodeLayer.Contract,
        "감자 Farm Area의 승인된 농사로·KAMIS·USDA AMS 현실 근거 묶음을 전달한다.",
        StepKey = "contract.farm-reality-evidence",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "원 단위와 관계 상태를 보존하며 가격 차이·수익·사건·공간 배치를 계산하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationFarmRealityEvidenceBundle
    {
        public string SchemaVersion { get; set; } =
            SimulationFarmRealityEvidenceCodes.SchemaVersion;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string ProductDisplayName { get; set; } = string.Empty;
        public string ProductIdentityRevision { get; set; } = string.Empty;
        public string EvidenceRevision { get; set; } = string.Empty;
        public string InputHashSha256 { get; set; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; set; }
        public SimulationFarmRealitySourceEvidence[] Sources { get; set; } =
            Array.Empty<SimulationFarmRealitySourceEvidence>();
        public bool ChangesSimulationRules { get; set; }
        public bool MovesSpatialDefinitions { get; set; }
        public bool CreatesIncidentOrEffect { get; set; }
    }

    public sealed class SimulationFarmRealitySourceEvidence
    {
        public string SourceEvidenceStableId { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string DatasetId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string CodeScheme { get; set; } = string.Empty;
        public string ExternalCode { get; set; } = string.Empty;
        public string RelationStatusCode { get; set; } = string.Empty;
        public string AvailabilityCode { get; set; } = string.Empty;
        public string QualityCode { get; set; } = string.Empty;
        public DateTimeOffset? ObservedAtUtc { get; set; }
        public DateTimeOffset? RetrievedAtUtc { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string[] UnitCodes { get; set; } = Array.Empty<string>();
        public int MaxAgeHours { get; set; }
        public string SourceHashSha256 { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
        public string[] AdvisoryCodes { get; set; } = Array.Empty<string>();
    }
}
