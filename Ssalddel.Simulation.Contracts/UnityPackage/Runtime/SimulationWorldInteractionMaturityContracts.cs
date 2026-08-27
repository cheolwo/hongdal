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

    public static class SimulationWI음양Codes
    {
        public const string Yang = "Yang";
        public const string Yin = "Yin";
        public const string Contextual = "Contextual";
        public const string NotApplicable = "NotApplicable";
        public const string Unclassified = "Unclassified";
    }

    public static class SimulationWI수행주체Codes
    {
        public const string PlayerActor = "PlayerActor";
        public const string NpcActor = "NpcActor";
        public const string NotApplicable = "NotApplicable";

        public static bool IsActor(string value)
            => string.Equals(value, PlayerActor, StringComparison.Ordinal)
               || string.Equals(value, NpcActor, StringComparison.Ordinal);
    }

    public static class SimulationWI사분면Codes
    {
        public const string YangPlayer = "YangPlayer";
        public const string YangNpc = "YangNpc";
        public const string YinPlayer = "YinPlayer";
        public const string YinNpc = "YinNpc";
        public const string NotApplicable = "NotApplicable";
        public const string Unclassified = "Unclassified";
    }

    public sealed class SimulationWI음양주체분류Snapshot
    {
        public string PayloadCode { get; set; } = "WorldInteractionPolarity.v1";
        public string 음양Code { get; set; } = string.Empty;
        public string 수행주체Code { get; set; } = string.Empty;
        public string 사분면Code { get; set; } = string.Empty;
        public string 사분면기호 { get; set; } = string.Empty;
        public string 판정방식Code { get; set; } = string.Empty;
        public string 판정RuleRevision { get; set; } = string.Empty;
        public string 판정근거StableId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 생성 대장의 행동 목적과 신뢰 Actor 결속을 조합한다. TriggerSource는
    /// Actor 부호를 결정하지 않으며 클라이언트 입력으로 호출하지 않는다.
    /// </summary>
    public static class SimulationWI음양주체사분면Rules
    {
        public const string RuleRevision =
            "world-interaction-polarity-quadrants.r1";

        public static SimulationWI음양주체분류Snapshot Resolve(
            string worldInteractionId,
            string 수행주체Code,
            string triggerSourceCode = "",
            string playableLoopStableId = "")
        {
            var definition = Simulation세계상호작용이름Catalog.Find(
                worldInteractionId);
            if (definition == null)
                return CreateUnclassified(수행주체Code,
                    playableLoopStableId, string.Empty);

            if (string.Equals(definition.음양분류Code,
                    SimulationWI음양Codes.NotApplicable,
                    StringComparison.Ordinal)
                || string.Equals(수행주체Code,
                    SimulationWI수행주체Codes.NotApplicable,
                    StringComparison.Ordinal))
                return CreateNotApplicable(definition.음양판정방식Code,
                    playableLoopStableId);

            if (!SimulationWI수행주체Codes.IsActor(수행주체Code))
                return CreateUnclassified(수행주체Code,
                    playableLoopStableId, definition.음양판정방식Code);

            var polarity = definition.음양분류Code;
            if (string.Equals(polarity, SimulationWI음양Codes.Contextual,
                    StringComparison.Ordinal))
            {
                polarity = Simulation세계상호작용이름Catalog.문맥음양Code(
                    worldInteractionId, playableLoopStableId);
                if (string.IsNullOrWhiteSpace(polarity))
                    return CreateUnclassified(수행주체Code,
                        playableLoopStableId, definition.음양판정방식Code);
            }

            var player = string.Equals(수행주체Code,
                SimulationWI수행주체Codes.PlayerActor,
                StringComparison.Ordinal);
            var quadrant = string.Equals(polarity, SimulationWI음양Codes.Yang,
                StringComparison.Ordinal)
                ? player ? SimulationWI사분면Codes.YangPlayer
                    : SimulationWI사분면Codes.YangNpc
                : player ? SimulationWI사분면Codes.YinPlayer
                    : SimulationWI사분면Codes.YinNpc;
            var symbol = quadrant == SimulationWI사분면Codes.YangPlayer ? "++"
                : quadrant == SimulationWI사분면Codes.YangNpc ? "+-"
                : quadrant == SimulationWI사분면Codes.YinPlayer ? "-+" : "--";
            return new SimulationWI음양주체분류Snapshot
            {
                음양Code = polarity,
                수행주체Code = 수행주체Code,
                사분면Code = quadrant,
                사분면기호 = symbol,
                판정방식Code = definition.음양판정방식Code,
                판정RuleRevision = RuleRevision,
                판정근거StableId = playableLoopStableId?.Trim() ?? string.Empty,
            };
        }

        public static bool Matches(
            string worldInteractionId,
            string triggerSourceCode,
            SimulationWI음양주체분류Snapshot snapshot)
        {
            if (snapshot == null
                || !string.Equals(snapshot.PayloadCode,
                    "WorldInteractionPolarity.v1", StringComparison.Ordinal)
                || !string.Equals(snapshot.판정RuleRevision, RuleRevision,
                    StringComparison.Ordinal))
                return false;
            var expected = Resolve(worldInteractionId, snapshot.수행주체Code,
                triggerSourceCode, snapshot.판정근거StableId);
            return string.Equals(expected.음양Code, snapshot.음양Code,
                       StringComparison.Ordinal)
                   && string.Equals(expected.사분면Code, snapshot.사분면Code,
                       StringComparison.Ordinal)
                   && string.Equals(expected.사분면기호, snapshot.사분면기호,
                       StringComparison.Ordinal)
                   && string.Equals(expected.판정방식Code,
                       snapshot.판정방식Code, StringComparison.Ordinal);
        }

        private static SimulationWI음양주체분류Snapshot CreateNotApplicable(
            string decisionCode, string contextStableId)
            => new SimulationWI음양주체분류Snapshot
            {
                음양Code = SimulationWI음양Codes.NotApplicable,
                수행주체Code = SimulationWI수행주체Codes.NotApplicable,
                사분면Code = SimulationWI사분면Codes.NotApplicable,
                판정방식Code = decisionCode,
                판정RuleRevision = RuleRevision,
                판정근거StableId = contextStableId?.Trim() ?? string.Empty,
            };

        private static SimulationWI음양주체분류Snapshot CreateUnclassified(
            string actorCode, string contextStableId, string decisionCode)
            => new SimulationWI음양주체분류Snapshot
            {
                음양Code = SimulationWI음양Codes.Unclassified,
                수행주체Code = actorCode?.Trim() ?? string.Empty,
                사분면Code = SimulationWI사분면Codes.Unclassified,
                판정방식Code = decisionCode,
                판정RuleRevision = RuleRevision,
                판정근거StableId = contextStableId?.Trim() ?? string.Empty,
            };
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
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public string 음양분류Code { get; set; } = string.Empty;
        public string 음양판정방식Code { get; set; } = string.Empty;
        public SimulationWI실행우선순위Definition? 실행우선순위 { get; set; }
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
        public SimulationWI음양주체분류Snapshot 음양주체분류 { get; set; }
            = new SimulationWI음양주체분류Snapshot();
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
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public SimulationWI음양주체분류Snapshot 음양주체분류 { get; set; }
            = new SimulationWI음양주체분류Snapshot();
        public SimulationWI실행우선순위Definition? 실행우선순위 { get; set; }
        public string StateCode { get; set; } =
            SimulationWorldInteractionMaturityStateCodes.ManifestationMissing;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public string[] MissingEvidenceCodes { get; set; } = Array.Empty<string>();
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable;
    }
}
