using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationEngineInteractionComponentCodes
    {
        public const string WorldInteractionPipeline = "WI.ExecutionPipeline";
        public const string AuthorityCore = "Simulation.AuthorityCore";
        public const string LhSurface = "LH.Surface";
        public const string SkyPresentation = "Sky.Presentation";
        public const string ExteriorPlacement = "Placement.Exterior";
        public const string InteriorPlacement = "Placement.Interior";
        public const string WorldPresentation = "World.Presentation";
    }

    public static class SimulationEngineInteractionComponentKinds
    {
        public const string Authority = "Authority";
        public const string Orchestration = "Orchestration";
        public const string Presentation = "Presentation";
    }

    public static class SimulationEngineInteractionPhaseCodes
    {
        public const string Preview = "Preview";
        public const string Confirm = "Confirm";
        public const string AuthorityCommit = "AuthorityCommit";
        public const string SurfacePreparation = "SurfacePreparation";
        public const string AtmosphereProjection = "AtmosphereProjection";
        public const string ExteriorPlacement = "ExteriorPlacement";
        public const string InteriorPlacement = "InteriorPlacement";
        public const string ReturnProjection = "ReturnProjection";
    }

    public static class SimulationEngineInteractionStatusCodes
    {
        public const string Executed = "Executed";
        public const string Reused = "Reused";
        public const string NotApplicable = "NotApplicable";
        public const string Blocked = "Blocked";
    }

    public sealed class SimulationPlayableLoopEngineTraceContext
    {
        public string PlayableLoopStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string AuthorityLocationCode { get; set; } = string.Empty;
        public long AuthorityRevision { get; set; }
    }

    /// <summary>
    /// 한 WI 명령이 권위 Core와 표현 엔진을 통과한 사실을 설명하는 비권위 기록이다.
    /// Save/Replay canonical hash 또는 게임 규칙 입력으로 사용하지 않는다.
    /// </summary>
    public sealed class SimulationPlayableLoopEngineTraceEntry
    {
        public string PlayableLoopStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string AuthorityLocationCode { get; set; } = string.Empty;
        public string ComponentCode { get; set; } = string.Empty;
        public string ComponentKindCode { get; set; } = string.Empty;
        public string ComponentRevision { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string InputHashSha256 { get; set; } = string.Empty;
        public string OutputHashSha256 { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public long BeforeAuthorityRevision { get; set; }
        public long AfterAuthorityRevision { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
    }

    public sealed class SimulationPlayableLoopEngineRequirement
    {
        public string ComponentCode { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public bool AllowsReused { get; set; } = true;
        public bool AllowsNotApplicable { get; set; }
    }

    public sealed class SimulationPlayableLoopEngineValidationProfile
    {
        public string ProfileRevision { get; set; } = string.Empty;
        public string PlayableLoopStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public SimulationPlayableLoopEngineRequirement[] Requirements { get; set; }
            = Array.Empty<SimulationPlayableLoopEngineRequirement>();
    }

    public sealed class SimulationPlayableLoopEngineValidationSnapshot
    {
        public string ProfileRevision { get; set; } = string.Empty;
        public string PlayableLoopStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string EarliestReopenEvidenceStageCode { get; set; } = string.Empty;
        public string[] FailureCodes { get; set; } = Array.Empty<string>();
        public SimulationPlayableLoopEngineTraceEntry[] TraceEntries { get; set; }
            = Array.Empty<SimulationPlayableLoopEngineTraceEntry>();
    }

    public interface ISimulationPlayableLoopEngineTraceSink
    {
        void Record(SimulationPlayableLoopEngineTraceEntry entry);

        SimulationPlayableLoopEngineTraceEntry[] Snapshot(
            string playableLoopStableId, string worldInteractionId,
            string commandId);
    }
}
