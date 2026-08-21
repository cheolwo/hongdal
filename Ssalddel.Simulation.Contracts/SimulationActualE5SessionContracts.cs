using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationActualE5SessionCodes
    {
        public const string SchemaVersion = "simulation-actual-e5-session.v1";
        public const string EvidenceStage = "E5";
    }

    /// <summary>
    /// 실제 E5 공간과 H5 배치를 고정하여 Simulation 세션을 만드는 서버 입력이다.
    /// E6 현실 근거는 선택 사항이며 이 계약의 선행 조건이 아니다.
    /// </summary>
    public sealed class SimulationActualE5SessionCreateRequest
    {
        public 경영SimulationSession생성Request Session { get; set; } = new();
        public string AreaSetNetworkStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int ExpectedWorldLayoutRevision { get; set; }
        public string ExpectedWorldLayoutHashSha256 { get; set; } = string.Empty;
        public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationActualE5SessionCreateResponse
    {
        public string SchemaVersion { get; set; } = SimulationActualE5SessionCodes.SchemaVersion;
        public string EvidenceStageCode { get; set; } = SimulationActualE5SessionCodes.EvidenceStage;
        public string AreaSetNetworkStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public string PlacementAuthorityCode { get; set; } = SimulationWorldLayoutCodes.ScenarioRelative;
        public string WorldGroundingStateCode { get; set; } = SimulationWorldLayoutCodes.NotApplied;
        public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
        public string RealityContextSnapshotStableId { get; set; } = string.Empty;
        public string RealityContextAvailabilityCode { get; set; } =
            SimulationRealityContextCodes.Unavailable;
        public 경영SimulationSessionSnapshot Session { get; set; } = new();
    }
}
