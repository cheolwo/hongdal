using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldInteractionTriggerSourceCodes
    {
        public const string DataDriven = "DataDriven";
        public const string PlayerDriven = "PlayerDriven";
        public const string NpcDriven = "NpcDriven";
        public const string WorldDerived = "WorldDerived";

        public static string[] All { get; } =
        {
            DataDriven, PlayerDriven, NpcDriven, WorldDerived,
        };

        public static bool IsKnown(string value)
            => Array.IndexOf(All, value) >= 0;
    }

    public static class SimulationWorldInteractionContextCodes
    {
        public const string Initiator = "Initiator";
        public const string Actor = "Actor";
        public const string Target = "Target";
        public const string DataResource = "DataResource";
        public const string Time = "Time";
        public const string Spatial = "Spatial";
    }

    public static class SimulationWorldInteractionSpatialEvidenceCodes
    {
        public const string Required = "Required";
        public const string NotApplicable = "NotApplicable";
        public const string Bound = "Bound";
        public const string RequiredMissing = "RequiredMissing";
    }

    public static class SimulationWorldInteractionMaturityStateCodes
    {
        public const string ContextUnbound = "ContextUnbound";
        public const string ContextPartiallyBound = "ContextPartiallyBound";
        public const string ContextBound = "ContextBound";
        public const string ManifestationMissing = "ManifestationMissing";
        public const string ManifestationPartial = "ManifestationPartial";
        public const string Manifested = "Manifested";
    }

    /// <summary>
    /// WI 정의가 허용하는 발생원과 E4에서 결속할 실행 문맥이다.
    /// 공간은 필요한 WI에만 Required로 선언한다.
    /// </summary>
    public sealed class SimulationWorldInteractionDefinitionContext
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string[] AllowedTriggerSourceCodes { get; set; } = Array.Empty<string>();
        public string[] RequiredContextCodes { get; set; } = Array.Empty<string>();
        public string SpatialApplicabilityCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
    }

    public sealed class SimulationWorldInteractionE4ContextReviewRequest
    {
        public SimulationWorldInteractionDefinitionContext Definition { get; set; }
            = new SimulationWorldInteractionDefinitionContext();
        public string[] BoundTriggerSourceCodes { get; set; } = Array.Empty<string>();
        public string[] BoundContextCodes { get; set; } = Array.Empty<string>();
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
    }

    public sealed class SimulationWorldInteractionE4ContextReviewResult
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string StateCode { get; set; } =
            SimulationWorldInteractionMaturityStateCodes.ContextUnbound;
        public string[] AllowedTriggerSourceCodes { get; set; } = Array.Empty<string>();
        public string[] BoundTriggerSourceCodes { get; set; } = Array.Empty<string>();
        public string[] MissingContextCodes { get; set; } = Array.Empty<string>();
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
    }

    /// <summary>
    /// 실행 인스턴스의 발생원 기록이다. Player/NPC/Data/World Factory가 생성하며
    /// 클라이언트가 임의의 발생원 코드를 전달하는 Command DTO로 사용하지 않는다.
    /// </summary>
    public sealed class SimulationWorldInteractionInvocationRecord
    {
        public string PayloadCode { get; set; } = "WorldInteractionInvocation.v1";
        public string WorldInteractionId { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string InitiatorStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string[] SourceReferenceIds { get; set; } = Array.Empty<string>();
        public string TimeReferenceId { get; set; } = string.Empty;
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
        public string[] SpatialEvidenceReferenceIds { get; set; }
            = Array.Empty<string>();
    }

    /// <summary>
    /// Confirm으로 시작된 WI가 권위 Revision과 Task·Effect·결과·복귀 경로에
    /// 얼마나 도달했는지 보존하는 불변 Save/Replay 기록이다.
    /// </summary>
    public sealed class SimulationWorldInteractionManifestationRecord
    {
        public string PayloadCode { get; set; } = "WorldInteractionManifestation.v1";
        public string WorldInteractionId { get; set; } = string.Empty;
        public string OriginCommandId { get; set; } = string.Empty;
        public long BeforeWorldRevision { get; set; }
        public long AfterWorldRevision { get; set; }
        public string StateCode { get; set; }
            = SimulationWorldInteractionMaturityStateCodes.ManifestationMissing;
        public string[] TaskOrEffectReferenceIds { get; set; } = Array.Empty<string>();
        public string[] ResultStateCodes { get; set; } = Array.Empty<string>();
        public string[] SuccessorOrReturnCodes { get; set; } = Array.Empty<string>();
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
        public string[] SpatialEvidenceReferenceIds { get; set; }
            = Array.Empty<string>();
        public string[] MissingEvidenceCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorldInteractionE5ManifestationReviewRequest
    {
        public SimulationWorldInteractionDefinitionContext Definition { get; set; }
            = new SimulationWorldInteractionDefinitionContext();
        public string E4StateCode { get; set; } =
            SimulationWorldInteractionMaturityStateCodes.ContextUnbound;
        public SimulationWorldInteractionInvocationRecord? Invocation { get; set; }
        public bool AuthorityTransitionRecorded { get; set; }
        public bool TaskOrEffectRecorded { get; set; }
        public bool ResultStateRecorded { get; set; }
        public bool SuccessorOrReturnPathRecorded { get; set; }
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
    }

    public sealed class SimulationWorldInteractionE5ManifestationReviewResult
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string StateCode { get; set; } =
            SimulationWorldInteractionMaturityStateCodes.ManifestationMissing;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public string[] MissingEvidenceCodes { get; set; } = Array.Empty<string>();
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
    }
}
