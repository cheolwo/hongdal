using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E4,
        "WI 발생원·주체·대상·자료·자원·시간과 선택적 공간 문맥을 판정한다.",
        Boundary = "E4 문맥 판정과 E5 발현 판정을 제공하지만 상위 E 증거를 대신하지 않는다.")]
    public sealed class SimulationWorldInteractionMaturityService
    {
        public SimulationWorldInteractionE4ContextReviewResult ReviewE4(
            SimulationWorldInteractionE4ContextReviewRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            var definition = ValidateDefinition(request.Definition);
            var boundTriggers = Normalize(request.BoundTriggerSourceCodes);
            if (boundTriggers.Any(value =>
                    !definition.AllowedTriggerSourceCodes.Contains(value,
                        StringComparer.Ordinal)))
                throw new InvalidOperationException(
                    "WorldInteractionTriggerSourceNotAllowed");

            var boundContexts = Normalize(request.BoundContextCodes);
            var missing = definition.RequiredContextCodes
                .Where(value => !boundContexts.Contains(value, StringComparer.Ordinal))
                .ToList();
            if (boundTriggers.Length == 0)
                missing.Add("TriggerSource");

            ValidateSpatialEvidence(definition.SpatialApplicabilityCode,
                request.SpatialEvidenceStateCode);
            if (definition.SpatialApplicabilityCode ==
                    SimulationWorldInteractionSpatialEvidenceCodes.Required
                && request.SpatialEvidenceStateCode !=
                    SimulationWorldInteractionSpatialEvidenceCodes.Bound)
                missing.Add(SimulationWorldInteractionContextCodes.Spatial);

            var state = missing.Count == 0
                ? SimulationWorldInteractionMaturityStateCodes.ContextBound
                : boundTriggers.Length == 0 && boundContexts.Length == 0
                    ? SimulationWorldInteractionMaturityStateCodes.ContextUnbound
                    : SimulationWorldInteractionMaturityStateCodes.ContextPartiallyBound;
            return new SimulationWorldInteractionE4ContextReviewResult
            {
                WorldInteractionId = definition.WorldInteractionId,
                WorldInteractionName = Simulation세계상호작용이름Catalog
                    .한국어기능명(definition.WorldInteractionId),
                WorldInteractionDisplayName = Simulation세계상호작용이름Catalog
                    .한국어표시명(definition.WorldInteractionId),
                ResponsibilityKindCode = Simulation세계상호작용이름Catalog
                    .책임종류코드(definition.WorldInteractionId),
                PrimaryOutcomeCode = Simulation세계상호작용이름Catalog
                    .주요결과코드(definition.WorldInteractionId),
                SingleResponsibilityAssessmentCode =
                    Simulation세계상호작용이름Catalog
                        .단일책임판정코드(definition.WorldInteractionId),
                음양분류Code = Simulation세계상호작용이름Catalog
                    .음양분류Code(definition.WorldInteractionId),
                음양판정방식Code = Simulation세계상호작용이름Catalog
                    .음양판정방식Code(definition.WorldInteractionId),
                실행우선순위 = SimulationWI실행우선순위Catalog
                    .Find(definition.WorldInteractionId),
                StateCode = state,
                AllowedTriggerSourceCodes = definition.AllowedTriggerSourceCodes,
                BoundTriggerSourceCodes = boundTriggers,
                MissingContextCodes = Normalize(missing),
                SpatialEvidenceStateCode = request.SpatialEvidenceStateCode,
            };
        }

        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E5,
            "WI가 허용 발생원에서 권위 상태 전이와 결과·후속 경로로 발현됐는지 판정한다.",
            Boundary = "공간 조립만으로 E5를 완료하지 않으며 E7 실제 플레이를 주장하지 않는다.")]
        public SimulationWorldInteractionE5ManifestationReviewResult ReviewE5(
            SimulationWorldInteractionE5ManifestationReviewRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            var definition = ValidateDefinition(request.Definition);
            ValidateSpatialEvidence(definition.SpatialApplicabilityCode,
                request.SpatialEvidenceStateCode);
            var missing = new List<string>();
            if (request.E4StateCode !=
                SimulationWorldInteractionMaturityStateCodes.ContextBound)
                missing.Add("E4ContextBound");
            if (request.Invocation is null)
                missing.Add("Invocation");
            else
            {
                ValidateInvocation(request.Invocation, definition);
            }
            if (!request.AuthorityTransitionRecorded)
                missing.Add("AuthorityTransition");
            if (!request.TaskOrEffectRecorded)
                missing.Add("TaskOrEffect");
            if (!request.ResultStateRecorded)
                missing.Add("ResultState");
            if (!request.SuccessorOrReturnPathRecorded)
                missing.Add("SuccessorOrReturnPath");
            if (definition.SpatialApplicabilityCode ==
                    SimulationWorldInteractionSpatialEvidenceCodes.Required
                && request.SpatialEvidenceStateCode !=
                    SimulationWorldInteractionSpatialEvidenceCodes.Bound)
                missing.Add("SpatialEvidence");

            var evidenceCount = 5 - missing.Count(value => value is
                "Invocation" or "AuthorityTransition" or "TaskOrEffect" or
                "ResultState" or "SuccessorOrReturnPath");
            var state = missing.Count == 0
                ? SimulationWorldInteractionMaturityStateCodes.Manifested
                : evidenceCount <= 0
                    ? SimulationWorldInteractionMaturityStateCodes.ManifestationMissing
                    : SimulationWorldInteractionMaturityStateCodes.ManifestationPartial;
            return new SimulationWorldInteractionE5ManifestationReviewResult
            {
                WorldInteractionId = definition.WorldInteractionId,
                WorldInteractionName = Simulation세계상호작용이름Catalog
                    .한국어기능명(definition.WorldInteractionId),
                WorldInteractionDisplayName = Simulation세계상호작용이름Catalog
                    .한국어표시명(definition.WorldInteractionId),
                ResponsibilityKindCode = Simulation세계상호작용이름Catalog
                    .책임종류코드(definition.WorldInteractionId),
                PrimaryOutcomeCode = Simulation세계상호작용이름Catalog
                    .주요결과코드(definition.WorldInteractionId),
                SingleResponsibilityAssessmentCode =
                    Simulation세계상호작용이름Catalog
                        .단일책임판정코드(definition.WorldInteractionId),
                음양주체분류 = request.Invocation?.음양주체분류
                    ?? new SimulationWI음양주체분류Snapshot(),
                실행우선순위 = SimulationWI실행우선순위Catalog
                    .Find(definition.WorldInteractionId),
                StateCode = state,
                TriggerSourceCode = request.Invocation?.TriggerSourceCode
                    ?? string.Empty,
                MissingEvidenceCodes = Normalize(missing),
                SpatialEvidenceStateCode = request.SpatialEvidenceStateCode,
            };
        }

        public static SimulationWorldInteractionInvocationRecord FromPlayer(
            string worldInteractionId, string playerStableId, string actorStableId,
            string playableLoopStableId = "")
            => CreateInvocation(worldInteractionId,
                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                playerStableId, actorStableId,
                SimulationWI수행주체Codes.PlayerActor,
                playableLoopStableId, Array.Empty<string>());

        public static SimulationWorldInteractionInvocationRecord FromNpc(
            string worldInteractionId, string npcStableId, string actorStableId,
            string playableLoopStableId = "")
            => CreateInvocation(worldInteractionId,
                SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
                npcStableId, actorStableId,
                SimulationWI수행주체Codes.NpcActor,
                playableLoopStableId, Array.Empty<string>());

        public static SimulationWorldInteractionInvocationRecord
            FromPlayerDelegatedToNpc(
                string worldInteractionId, string playerStableId,
                string npcActorStableId, string playableLoopStableId = "")
            => CreateInvocation(worldInteractionId,
                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                playerStableId, npcActorStableId,
                SimulationWI수행주체Codes.NpcActor,
                playableLoopStableId, Array.Empty<string>());

        public static SimulationWorldInteractionInvocationRecord FromData(
            string worldInteractionId, string evaluatorStableId,
            params string[] sourceReferenceIds)
            => CreateInvocation(worldInteractionId,
                SimulationWorldInteractionTriggerSourceCodes.DataDriven,
                evaluatorStableId, string.Empty,
                SimulationWI수행주체Codes.NotApplicable,
                string.Empty, sourceReferenceIds);

        public static SimulationWorldInteractionInvocationRecord FromWorldDerived(
            string worldInteractionId, string evaluatorStableId,
            params string[] sourceReferenceIds)
            => CreateInvocation(worldInteractionId,
                SimulationWorldInteractionTriggerSourceCodes.WorldDerived,
                evaluatorStableId, string.Empty,
                SimulationWI수행주체Codes.NotApplicable,
                string.Empty, sourceReferenceIds);

        private static SimulationWorldInteractionInvocationRecord CreateInvocation(
            string worldInteractionId, string triggerSourceCode,
            string initiatorStableId, string actorStableId,
            string actorCode, string playableLoopStableId,
            IEnumerable<string> sourceReferenceIds)
        {
            RequireStableId(worldInteractionId, "WorldInteractionIdInvalid");
            RequireStableId(initiatorStableId,
                "WorldInteractionInitiatorStableIdInvalid");
            return new SimulationWorldInteractionInvocationRecord
            {
                PayloadCode = "WorldInteractionInvocation.v2",
                WorldInteractionId = worldInteractionId.Trim(),
                TriggerSourceCode = triggerSourceCode,
                InitiatorStableId = initiatorStableId.Trim(),
                ActorStableId = actorStableId?.Trim() ?? string.Empty,
                SourceReferenceIds = Normalize(sourceReferenceIds),
                음양주체분류 = SimulationWI음양주체사분면Rules.Resolve(
                    worldInteractionId, actorCode, triggerSourceCode,
                    playableLoopStableId),
            };
        }

        private static SimulationWorldInteractionDefinitionContext ValidateDefinition(
            SimulationWorldInteractionDefinitionContext? definition)
        {
            if (definition is null)
                throw new InvalidOperationException("WorldInteractionDefinitionMissing");
            RequireStableId(definition.WorldInteractionId,
                "WorldInteractionIdInvalid");
            definition.AllowedTriggerSourceCodes = Normalize(
                definition.AllowedTriggerSourceCodes);
            definition.RequiredContextCodes = Normalize(
                definition.RequiredContextCodes);
            if (definition.AllowedTriggerSourceCodes.Length == 0
                || definition.AllowedTriggerSourceCodes.Any(value =>
                    !SimulationWorldInteractionTriggerSourceCodes.IsKnown(value)))
                throw new InvalidOperationException(
                    "WorldInteractionAllowedTriggerSourcesInvalid");
            if (definition.SpatialApplicabilityCode is not
                (SimulationWorldInteractionSpatialEvidenceCodes.Required or
                 SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable))
                throw new InvalidOperationException(
                    "WorldInteractionSpatialApplicabilityInvalid");
            return definition;
        }

        private static void ValidateInvocation(
            SimulationWorldInteractionInvocationRecord invocation,
            SimulationWorldInteractionDefinitionContext definition)
        {
            if (!string.Equals(invocation.WorldInteractionId,
                    definition.WorldInteractionId, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "WorldInteractionInvocationIdMismatch");
            if (!definition.AllowedTriggerSourceCodes.Contains(
                    invocation.TriggerSourceCode, StringComparer.Ordinal))
                throw new InvalidOperationException(
                    "WorldInteractionTriggerSourceNotAllowed");
            RequireStableId(invocation.InitiatorStableId,
                "WorldInteractionInitiatorStableIdInvalid");
            if (string.Equals(invocation.PayloadCode,
                    "WorldInteractionInvocation.v2", StringComparison.Ordinal)
                && !SimulationWI음양주체사분면Rules.Matches(
                    invocation.WorldInteractionId,
                    invocation.TriggerSourceCode,
                    invocation.음양주체분류))
                throw new InvalidOperationException(
                    "WorldInteractionPolaritySnapshotInvalid");
        }

        private static void ValidateSpatialEvidence(
            string applicabilityCode, string evidenceStateCode)
        {
            if (applicabilityCode ==
                    SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable
                && evidenceStateCode !=
                    SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable)
                throw new InvalidOperationException(
                    "WorldInteractionSpatialEvidenceMustBeNotApplicable");
            if (applicabilityCode ==
                    SimulationWorldInteractionSpatialEvidenceCodes.Required
                && evidenceStateCode is not
                    (SimulationWorldInteractionSpatialEvidenceCodes.Bound or
                     SimulationWorldInteractionSpatialEvidenceCodes.RequiredMissing))
                throw new InvalidOperationException(
                    "WorldInteractionSpatialEvidenceStateInvalid");
        }

        private static string[] Normalize(IEnumerable<string>? values)
            => (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorCode);
        }
    }
}
