using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public sealed class 세계상호작용실행Context
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string InitiatorStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string[] SourceReferenceIds { get; set; } = Array.Empty<string>();
        public string[] FocusEvidenceReferenceIds { get; set; }
            = Array.Empty<string>();
        public Simulation집중판정ResultSnapshot? 집중판정결과 { get; set; }
        public string TimeReferenceId { get; set; } = string.Empty;
        public string PlayableLoopStableId { get; set; } = string.Empty;
        public string AuthorityLocationCode { get; set; } = string.Empty;
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.RequiredMissing;
        public string[] SpatialEvidenceReferenceIds { get; set; }
            = Array.Empty<string>();
        public string[] TaskOrEffectReferenceIds { get; set; } = Array.Empty<string>();
        public string[] ResultStateCodes { get; set; } = Array.Empty<string>();
        public string[] SuccessorOrReturnCodes { get; set; } = Array.Empty<string>();
        public string PrimaryOutcomeCode { get; set; } = "CommandAccepted";
        public string 결과분류Code { get; set; }
            = Simulation행위결과분류Codes.성공;
        public string[] 변화의미Codes { get; set; } = Array.Empty<string>();
        public string SpatialRevision { get; set; } = string.Empty;
    }

    public interface I세계상호작용실행Pipeline
    {
        void RecordPreview(세계상호작용실행Context context,
            long authorityRevision, bool canConfirm,
            IEnumerable<string>? blockReasonCodes = null);

        T ExecutePlayerDriven<T>(경영SimulationSessionAggregate aggregate,
            세계상호작용실행Context context, Func<T> authorityConfirm);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E5,
        "신뢰 Player Adapter의 WI Confirm을 E4 문맥·E5 발현·Save/Replay 기록으로 결속한다.",
        Boundary = "클라이언트 입력으로 발생원을 선택하지 않고 기존 Domain 차단 규칙을 우회하지 않는다.")]
    public sealed class 세계상호작용실행Pipeline : I세계상호작용실행Pipeline
    {
        private const string TraceRevision = "playable-loop-engine-trace.r3";
        private readonly SimulationWorldInteractionMaturityService maturity = new();
        private readonly ISimulationPlayableLoopEngineTraceSink traceSink;

        public 세계상호작용실행Pipeline(
            ISimulationPlayableLoopEngineTraceSink? traceSink = null)
        {
            this.traceSink = traceSink
                ?? NullSimulationPlayableLoopEngineTraceSink.Instance;
        }

        public void RecordPreview(세계상호작용실행Context context,
            long authorityRevision, bool canConfirm,
            IEnumerable<string>? blockReasonCodes = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (!TraceEnabled(context)) return;
            var reasons = Normalize(blockReasonCodes);
            traceSink.Record(CreateTrace(context,
                SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
                SimulationEngineInteractionComponentKinds.Orchestration,
                SimulationEngineInteractionPhaseCodes.Preview,
                canConfirm
                    ? SimulationEngineInteractionStatusCodes.Executed
                    : SimulationEngineInteractionStatusCodes.Blocked,
                authorityRevision, authorityRevision,
                SimulationPlayableLoopEngineTraceHash.Compute(
                    context.WorldInteractionId, context.CommandId,
                    authorityRevision),
                SimulationPlayableLoopEngineTraceHash.Compute(
                    canConfirm, string.Join(",", reasons)),
                reasons.FirstOrDefault() ?? string.Empty));
        }

        public T ExecutePlayerDriven<T>(경영SimulationSessionAggregate aggregate,
            세계상호작용실행Context context, Func<T> authorityConfirm)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var definition = 세계상호작용실행DefinitionCatalog.Get(
                context.WorldInteractionId);
            var boundContexts = new List<string>
            {
                SimulationWorldInteractionContextCodes.Initiator,
                SimulationWorldInteractionContextCodes.Actor,
                SimulationWorldInteractionContextCodes.Target,
                SimulationWorldInteractionContextCodes.DataResource,
                SimulationWorldInteractionContextCodes.Time,
            };
            if (context.SpatialEvidenceStateCode ==
                SimulationWorldInteractionSpatialEvidenceCodes.Bound)
                boundContexts.Add(SimulationWorldInteractionContextCodes.Spatial);
            var e4 = maturity.ReviewE4(
                new SimulationWorldInteractionE4ContextReviewRequest
                {
                    Definition = definition,
                    BoundTriggerSourceCodes = new[]
                    {
                        SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                    },
                    BoundContextCodes = boundContexts.ToArray(),
                    SpatialEvidenceStateCode = context.SpatialEvidenceStateCode,
                });

            var invocation = SimulationWorldInteractionMaturityService.FromPlayer(
                context.WorldInteractionId, context.InitiatorStableId,
                context.ActorStableId, context.PlayableLoopStableId);
            invocation.CommandId = context.CommandId;
            invocation.TargetStableId = context.TargetStableId;
            invocation.SourceReferenceIds = Normalize(context.SourceReferenceIds);
            invocation.TimeReferenceId = context.TimeReferenceId;
            invocation.SpatialEvidenceStateCode = context.SpatialEvidenceStateCode;
            invocation.SpatialEvidenceReferenceIds =
                Normalize(context.SpatialEvidenceReferenceIds);

            var beforeAuthorityRevision = aggregate.Revision;
            if (TraceEnabled(context))
            {
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionComponentKinds.Orchestration,
                    SimulationEngineInteractionPhaseCodes.Confirm,
                    SimulationEngineInteractionStatusCodes.Executed,
                    beforeAuthorityRevision, beforeAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        context.CommandId, beforeAuthorityRevision,
                        context.TargetStableId), string.Empty, string.Empty));
                var focusReferences = Normalize(
                    context.FocusEvidenceReferenceIds);
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionComponentKinds.Orchestration,
                    SimulationEngineInteractionPhaseCodes.FocusEvidenceCollect,
                    focusReferences.Length > 0
                        ? SimulationEngineInteractionStatusCodes.Executed
                        : SimulationEngineInteractionStatusCodes.NotApplicable,
                    beforeAuthorityRevision, beforeAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        context.CommandId, context.WorldInteractionId),
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        string.Join(",", focusReferences)),
                    focusReferences.Length > 0
                        ? string.Empty : "FocusProfileNotConfigured"));
            }

            Simulation세계상호작용실행Result<T> execution;
            try
            {
                execution = aggregate.ExecuteWorldInteraction(invocation,
                    authorityConfirm,
                    (beforeRevision, afterRevision) =>
                    {
                        var e5 = maturity.ReviewE5(
                            new SimulationWorldInteractionE5ManifestationReviewRequest
                            {
                                Definition = definition,
                                E4StateCode = e4.StateCode,
                                Invocation = invocation,
                                AuthorityTransitionRecorded =
                                    afterRevision > beforeRevision,
                                TaskOrEffectRecorded =
                                    context.TaskOrEffectReferenceIds.Length > 0,
                                ResultStateRecorded =
                                    context.ResultStateCodes.Length > 0,
                                SuccessorOrReturnPathRecorded =
                                    context.SuccessorOrReturnCodes.Length > 0,
                                SpatialEvidenceStateCode =
                                    context.SpatialEvidenceStateCode,
                            });
                        var manifestation =
                            new SimulationWorldInteractionManifestationRecord
                        {
                            WorldInteractionId = context.WorldInteractionId,
                            OriginCommandId = context.CommandId,
                            BeforeWorldRevision = beforeRevision,
                            AfterWorldRevision = afterRevision,
                            StateCode = e5.StateCode,
                            TaskOrEffectReferenceIds =
                                Normalize(context.TaskOrEffectReferenceIds),
                            ResultStateCodes = Normalize(context.ResultStateCodes),
                            SuccessorOrReturnCodes =
                                Normalize(context.SuccessorOrReturnCodes),
                            SpatialEvidenceStateCode =
                                context.SpatialEvidenceStateCode,
                            SpatialEvidenceReferenceIds =
                                Normalize(context.SpatialEvidenceReferenceIds),
                            MissingEvidenceCodes = e5.MissingEvidenceCodes,
                        };
                        var effectReferences = Normalize(
                            context.TaskOrEffectReferenceIds);
                        return new Simulation세계상호작용권위증거Bundle
                        {
                            기존E5발현기록 = manifestation,
                            집중판정결과 = context.집중판정결과,
                            행위발현기록 = new Simulation행위발현Record
                            {
                                WorldStableId = aggregate.TerritoryStableId,
                                SessionStableId = aggregate.SessionStableId,
                                PlayableLoopStableId =
                                    context.PlayableLoopStableId,
                                WorldInteractionId = context.WorldInteractionId,
                                CommandId = context.CommandId,
                                TriggerSourceCode =
                                    SimulationWorldInteractionTriggerSourceCodes
                                        .PlayerDriven,
                                InitiatorStableId = context.InitiatorStableId,
                                ActorStableId = context.ActorStableId,
                                ActorKindCode = "Player",
                                TargetStableIds = Normalize(new[]
                                {
                                    context.TargetStableId,
                                }),
                                OutcomeStableId = "outcome:" + context.CommandId,
                                PrimaryOutcomeCode = string.IsNullOrWhiteSpace(
                                    context.PrimaryOutcomeCode)
                                    ? "CommandAccepted"
                                    : context.PrimaryOutcomeCode.Trim(),
                                결과분류Code = context.결과분류Code,
                                TaskStableId = effectReferences.FirstOrDefault(
                                    value => value.StartsWith("task:",
                                        StringComparison.Ordinal))
                                    ?? string.Empty,
                                EffectBatchStableId =
                                    "effect-batch:" + context.CommandId,
                                EffectReceiptStableIds = effectReferences
                                    .Where(value => value.StartsWith("effect:",
                                        StringComparison.Ordinal)).ToArray(),
                                변화의미Codes = Normalize(context.변화의미Codes),
                                영향공간StableIds = Normalize(
                                    context.SpatialEvidenceReferenceIds),
                                SourceReferenceIds = Normalize(
                                    context.SourceReferenceIds.Concat(
                                        context.FocusEvidenceReferenceIds)),
                                BeforeWorldRevision = beforeRevision,
                                AfterWorldRevision = afterRevision,
                                AppliedWorldTick = aggregate.CurrentTick,
                                RuleRevision = aggregate.RuleRevision,
                                SpatialRevision = context.SpatialRevision,
                                DataRevision = aggregate.ScenarioDataRevision,
                            },
                        };
                    });
            }
            catch (Exception exception)
            {
                if (TraceEnabled(context))
                    traceSink.Record(CreateTrace(context,
                        SimulationEngineInteractionComponentCodes.AuthorityCore,
                        SimulationEngineInteractionComponentKinds.Authority,
                        SimulationEngineInteractionPhaseCodes.AuthorityCommit,
                        SimulationEngineInteractionStatusCodes.Blocked,
                        beforeAuthorityRevision, aggregate.Revision,
                        SimulationPlayableLoopEngineTraceHash.Compute(
                            context.CommandId, beforeAuthorityRevision),
                        string.Empty, exception.GetType().Name));
                throw;
            }

            var afterAuthorityRevision = aggregate.Revision;
            if (TraceEnabled(context))
            {
                var status = execution.Reused
                    ? SimulationEngineInteractionStatusCodes.Reused
                    : SimulationEngineInteractionStatusCodes.Executed;
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes.AuthorityCore,
                    SimulationEngineInteractionComponentKinds.Authority,
                    SimulationEngineInteractionPhaseCodes.AuthorityCommit,
                    status, beforeAuthorityRevision, afterAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        context.CommandId, beforeAuthorityRevision),
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        context.CommandId, afterAuthorityRevision),
                    status == SimulationEngineInteractionStatusCodes.Reused
                        ? "IdempotentCommandReused" : string.Empty));
                var actionStatus = execution.행위발현기록 == null
                                   || string.IsNullOrWhiteSpace(
                                       execution.행위발현기록.기록HashSha256)
                    ? SimulationEngineInteractionStatusCodes.Blocked
                    : status;
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes.ActionJournal,
                    SimulationEngineInteractionComponentKinds.Authority,
                    SimulationEngineInteractionPhaseCodes.ActionRecordAppend,
                    actionStatus, beforeAuthorityRevision,
                    afterAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        context.CommandId, beforeAuthorityRevision),
                    execution.행위발현기록?.기록HashSha256 ?? string.Empty,
                    actionStatus == SimulationEngineInteractionStatusCodes.Blocked
                        ? "ActionRecordMissing" : string.Empty));
                var progressionStatus = execution.분야성장적용.상태Code switch
                {
                    Simulation분야성장적용상태Codes.Applied =>
                        SimulationEngineInteractionStatusCodes.Executed,
                    Simulation분야성장적용상태Codes.Reused =>
                        SimulationEngineInteractionStatusCodes.Reused,
                    _ => SimulationEngineInteractionStatusCodes.NotApplicable,
                };
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes
                        .PlayerDomainProgression,
                    SimulationEngineInteractionComponentKinds.Authority,
                    SimulationEngineInteractionPhaseCodes.PlayerProgressionApply,
                    progressionStatus, afterAuthorityRevision,
                    afterAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        execution.행위발현기록?.행위기록StableId),
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        execution.분야성장적용.AfterProfileRevision),
                    execution.분야성장적용.사유Code));
                var meditationStatus = execution.명상성장적용.상태Code switch
                {
                    Simulation집중판정Codes.Applied =>
                        SimulationEngineInteractionStatusCodes.Executed,
                    Simulation집중판정Codes.Reused =>
                        SimulationEngineInteractionStatusCodes.Reused,
                    _ => SimulationEngineInteractionStatusCodes.NotApplicable,
                };
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes
                        .PlayerMeditationProgression,
                    SimulationEngineInteractionComponentKinds.Authority,
                    SimulationEngineInteractionPhaseCodes
                        .MeditationProgressionApply,
                    meditationStatus, afterAuthorityRevision,
                    afterAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        execution.행위발현기록?.행위기록StableId),
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        execution.명상성장적용.AfterProfileRevision,
                        execution.명상성장적용.ContributionStableId),
                    execution.명상성장적용.사유Code));
                traceSink.Record(CreateTrace(context,
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionComponentKinds.Orchestration,
                    SimulationEngineInteractionPhaseCodes.ReturnProjection,
                    status, afterAuthorityRevision, afterAuthorityRevision,
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        context.CommandId, afterAuthorityRevision),
                    SimulationPlayableLoopEngineTraceHash.Compute(
                        string.Join(",", Normalize(
                            context.SuccessorOrReturnCodes))), string.Empty));
            }
            return execution.AuthorityResult;
        }

        private static bool TraceEnabled(세계상호작용실행Context context)
            => !string.IsNullOrWhiteSpace(context.PlayableLoopStableId)
               && !string.IsNullOrWhiteSpace(context.WorldInteractionId)
               && !string.IsNullOrWhiteSpace(context.CommandId);

        private static SimulationPlayableLoopEngineTraceEntry CreateTrace(
            세계상호작용실행Context context, string componentCode,
            string componentKindCode, string phaseCode, string statusCode,
            long beforeRevision, long afterRevision, string inputHash,
            string outputHash, string reasonCode) => new()
        {
            PlayableLoopStableId = context.PlayableLoopStableId,
            WorldInteractionId = context.WorldInteractionId,
            CommandId = context.CommandId,
            AuthorityLocationCode = context.AuthorityLocationCode,
            ComponentCode = componentCode,
            ComponentKindCode = componentKindCode,
            ComponentRevision = TraceRevision,
            PhaseCode = phaseCode,
            InputHashSha256 = inputHash,
            OutputHashSha256 = outputHash,
            StatusCode = statusCode,
            BeforeAuthorityRevision = beforeRevision,
            AfterAuthorityRevision = afterRevision,
            ReasonCode = reasonCode,
        };

        private static string[] Normalize(IEnumerable<string>? values)
            => (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
    }

    public static class 세계상호작용실행DefinitionCatalog
    {
        private static readonly Dictionary<string,
            SimulationWorldInteractionDefinitionContext> Definitions =
                세계상호작용ExecutionHeadCatalog.All.ToDictionary(
                    value => value.WorldInteractionId,
                    value => new SimulationWorldInteractionDefinitionContext
                    {
                        WorldInteractionId = value.WorldInteractionId,
                        AllowedTriggerSourceCodes = value.WorldInteractionId ==
                            "WI-NATURE-01"
                            ? new[]
                            {
                                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                                SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
                                SimulationWorldInteractionTriggerSourceCodes.WorldDerived,
                            }
                            : new[]
                            {
                                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                                SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
                            },
                        RequiredContextCodes = new[]
                        {
                            SimulationWorldInteractionContextCodes.Initiator,
                            SimulationWorldInteractionContextCodes.Actor,
                            SimulationWorldInteractionContextCodes.Target,
                            SimulationWorldInteractionContextCodes.DataResource,
                            SimulationWorldInteractionContextCodes.Time,
                            SimulationWorldInteractionContextCodes.Spatial,
                        },
                        SpatialApplicabilityCode =
                            SimulationWorldInteractionSpatialEvidenceCodes.Required,
                    }, StringComparer.Ordinal);

        public static SimulationWorldInteractionDefinitionContext Get(string id)
        {
            if (!Definitions.TryGetValue(id?.Trim() ?? string.Empty, out var value))
                throw new InvalidOperationException(
                    "WorldInteractionExecutionDefinitionMissing");
            return new SimulationWorldInteractionDefinitionContext
            {
                WorldInteractionId = value.WorldInteractionId,
                AllowedTriggerSourceCodes = value.AllowedTriggerSourceCodes.ToArray(),
                RequiredContextCodes = value.RequiredContextCodes.ToArray(),
                SpatialApplicabilityCode = value.SpatialApplicabilityCode,
            };
        }
    }
}
