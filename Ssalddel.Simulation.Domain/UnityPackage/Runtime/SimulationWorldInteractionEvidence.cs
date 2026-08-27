using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly List<SimulationWorldInteractionManifestationRecord>
            worldInteractionManifestations =
                new List<SimulationWorldInteractionManifestationRecord>();

        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E5,
            "Confirm과 같은 원자 경계에서 WI Invocation과 세계 발현 기록을 결속한다.",
            Boundary = "기록은 실제 권위 명령 결과를 설명하며 공간 조립이나 UI 표현만으로 생성하지 않는다.")]
        public Simulation세계상호작용실행Result<T> ExecuteWorldInteraction<T>(
            SimulationWorldInteractionInvocationRecord invocation,
            Func<T> authorityConfirm,
            Func<long, long, Simulation세계상호작용권위증거Bundle>
                evidenceFactory)
        {
            if (invocation == null) throw new ArgumentNullException(nameof(invocation));
            if (authorityConfirm == null)
                throw new ArgumentNullException(nameof(authorityConfirm));
            if (evidenceFactory == null)
                throw new ArgumentNullException(nameof(evidenceFactory));
            RequireStableId(invocation.CommandId,
                "WorldInteractionInvocationCommandIdInvalid");

            lock (gate)
            {
                var beforeRevision = Revision;
                var result = authorityConfirm();
                var entry = commandLog.LastOrDefault(value => string.Equals(
                    CommandIdOf(value), invocation.CommandId,
                    StringComparison.Ordinal));
                if (entry == null)
                    throw new SimulationConflictException(
                        "WorldInteractionCommandLogEntryMissing");

                var existing = worldInteractionManifestations.SingleOrDefault(value =>
                    string.Equals(value.OriginCommandId, invocation.CommandId,
                        StringComparison.Ordinal));
                if (existing != null)
                {
                    if (!string.Equals(existing.WorldInteractionId,
                            invocation.WorldInteractionId,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "WorldInteractionCommandKindConflict");
                    var previous = FindActionManifestation(invocation.CommandId);
                    return new Simulation세계상호작용실행Result<T>
                    {
                        AuthorityResult = result,
                        행위발현기록 = previous.기록
                            ?? new Simulation행위발현Record(),
                        분야성장적용 = previous.성장적용,
                        명상성장적용 = previous.명상성장적용,
                        Reused = true,
                    };
                }

                var clonedInvocation = CloneWorldInteractionInvocation(invocation);
                entry.WorldInteractionInvocation = clonedInvocation;
                var evidence = evidenceFactory(beforeRevision, Revision)
                    ?? throw new SimulationConflictException(
                        "WorldInteractionManifestationMissing");
                var manifestation = evidence.기존E5발현기록
                    ?? throw new SimulationConflictException(
                        "WorldInteractionManifestationMissing");
                if (!string.Equals(manifestation.WorldInteractionId,
                        invocation.WorldInteractionId, StringComparison.Ordinal)
                    || !string.Equals(manifestation.OriginCommandId,
                        invocation.CommandId, StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "WorldInteractionManifestationLinkInvalid");
                var action = AppendActionManifestationAndProgression(
                    evidence.행위발현기록, evidence.집중판정결과);
                worldInteractionManifestations.Add(
                    CloneWorldInteractionManifestation(manifestation));
                return new Simulation세계상호작용실행Result<T>
                {
                    AuthorityResult = result,
                    행위발현기록 = action.기록,
                    분야성장적용 = action.성장적용,
                    명상성장적용 = action.명상성장적용,
                    Reused = action.재사용,
                };
            }
        }

        internal void RestoreWorldInteractionEvidence(
            SimulationSessionSavePackage package)
        {
            if (!string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V15, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V16, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V17, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V18, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V19, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V20, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V21, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V22, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V23, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V24, StringComparison.Ordinal)
                && !string.Equals(package.SchemaVersion,
                    SimulationSaveSchemaVersions.V25, StringComparison.Ordinal))
                return;
            if (package.WorldInteractionManifestations == null)
                throw new SimulationConflictException(
                    "WorldInteractionManifestationLogMissing");

            lock (gate)
            {
                foreach (var savedEntry in package.CommandLog.Where(value =>
                             value.WorldInteractionInvocation != null))
                {
                    var commandId = savedEntry.WorldInteractionInvocation!.CommandId;
                    var replayedEntry = commandLog.SingleOrDefault(value =>
                        string.Equals(CommandIdOf(value), commandId,
                            StringComparison.Ordinal));
                    if (replayedEntry == null)
                        throw new SimulationConflictException(
                            "WorldInteractionCommandLogEntryMissing");
                    replayedEntry.WorldInteractionInvocation =
                        CloneWorldInteractionInvocation(
                            savedEntry.WorldInteractionInvocation);
                }

                worldInteractionManifestations.Clear();
                foreach (var manifestation in package.WorldInteractionManifestations)
                {
                    var entry = commandLog.SingleOrDefault(value => string.Equals(
                        CommandIdOf(value), manifestation.OriginCommandId,
                        StringComparison.Ordinal));
                    if (entry?.WorldInteractionInvocation == null
                        || !string.Equals(entry.WorldInteractionInvocation.WorldInteractionId,
                            manifestation.WorldInteractionId,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "WorldInteractionManifestationLinkInvalid");
                    worldInteractionManifestations.Add(
                        CloneWorldInteractionManifestation(manifestation));
                }
            }
        }

        private void CompleteWorldInteractionManifestationForTask(
            string taskStableId,
            IEnumerable<string> effectReferenceIds,
            IEnumerable<string> resultStateCodes,
            long completedWorldRevision)
        {
            var manifestation = worldInteractionManifestations
                .LastOrDefault(value => value.TaskOrEffectReferenceIds.Contains(
                    taskStableId, StringComparer.Ordinal));
            CompleteWorldInteractionManifestation(manifestation,
                effectReferenceIds, resultStateCodes, completedWorldRevision);
        }

        private void CompleteLatestWorldInteractionManifestation(
            string worldInteractionId,
            IEnumerable<string> effectReferenceIds,
            IEnumerable<string> resultStateCodes,
            long completedWorldRevision)
        {
            var manifestation = worldInteractionManifestations
                .LastOrDefault(value => string.Equals(value.WorldInteractionId,
                    worldInteractionId, StringComparison.Ordinal)
                    && value.StateCode !=
                    SimulationWorldInteractionMaturityStateCodes.Manifested);
            CompleteWorldInteractionManifestation(manifestation,
                effectReferenceIds, resultStateCodes, completedWorldRevision);
        }

        private static void CompleteWorldInteractionManifestation(
            SimulationWorldInteractionManifestationRecord? manifestation,
            IEnumerable<string> effectReferenceIds,
            IEnumerable<string> resultStateCodes,
            long completedWorldRevision)
        {
            if (manifestation == null) return;
            manifestation.TaskOrEffectReferenceIds = manifestation
                .TaskOrEffectReferenceIds.Concat(effectReferenceIds)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            manifestation.ResultStateCodes = manifestation.ResultStateCodes
                .Concat(resultStateCodes)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            manifestation.AfterWorldRevision = completedWorldRevision;
            manifestation.MissingEvidenceCodes = manifestation.MissingEvidenceCodes
                .Where(value => value != "ResultState")
                .ToArray();
            manifestation.StateCode = manifestation.MissingEvidenceCodes.Length == 0
                ? SimulationWorldInteractionMaturityStateCodes.Manifested
                : SimulationWorldInteractionMaturityStateCodes.ManifestationPartial;
        }

        private static string CommandIdOf(SimulationCommandLogEntrySnapshot entry)
            => entry.DecisionConfirmRequest?.CommandId
                ?? entry.FarmWorkConfirmRequest?.CommandId
                ?? entry.NatureSurvivalActionRequest?.CommandId
                ?? entry.RegionalIncidentResponseConfirmRequest?.CommandId
                ?? string.Empty;

        internal static SimulationWorldInteractionInvocationRecord
            CloneWorldInteractionInvocation(
                SimulationWorldInteractionInvocationRecord source)
            => new SimulationWorldInteractionInvocationRecord
            {
                PayloadCode = source.PayloadCode,
                WorldInteractionId = source.WorldInteractionId,
                TriggerSourceCode = source.TriggerSourceCode,
                CommandId = source.CommandId,
                InitiatorStableId = source.InitiatorStableId,
                ActorStableId = source.ActorStableId,
                TargetStableId = source.TargetStableId,
                SourceReferenceIds = source.SourceReferenceIds.ToArray(),
                TimeReferenceId = source.TimeReferenceId,
                SpatialEvidenceStateCode = source.SpatialEvidenceStateCode,
                SpatialEvidenceReferenceIds =
                    source.SpatialEvidenceReferenceIds.ToArray(),
                음양주체분류 = CloneWI음양주체분류(source.음양주체분류),
            };

        internal static SimulationWI음양주체분류Snapshot
            CloneWI음양주체분류(SimulationWI음양주체분류Snapshot source)
            => source == null
                ? new SimulationWI음양주체분류Snapshot()
                : new SimulationWI음양주체분류Snapshot
                {
                    PayloadCode = source.PayloadCode,
                    음양Code = source.음양Code,
                    수행주체Code = source.수행주체Code,
                    사분면Code = source.사분면Code,
                    사분면기호 = source.사분면기호,
                    판정방식Code = source.판정방식Code,
                    판정RuleRevision = source.판정RuleRevision,
                    판정근거StableId = source.판정근거StableId,
                };

        internal static SimulationWorldInteractionManifestationRecord
            CloneWorldInteractionManifestation(
                SimulationWorldInteractionManifestationRecord source)
            => new SimulationWorldInteractionManifestationRecord
            {
                PayloadCode = source.PayloadCode,
                WorldInteractionId = source.WorldInteractionId,
                OriginCommandId = source.OriginCommandId,
                BeforeWorldRevision = source.BeforeWorldRevision,
                AfterWorldRevision = source.AfterWorldRevision,
                StateCode = source.StateCode,
                TaskOrEffectReferenceIds = source.TaskOrEffectReferenceIds.ToArray(),
                ResultStateCodes = source.ResultStateCodes.ToArray(),
                SuccessorOrReturnCodes = source.SuccessorOrReturnCodes.ToArray(),
                SpatialEvidenceStateCode = source.SpatialEvidenceStateCode,
                SpatialEvidenceReferenceIds =
                    source.SpatialEvidenceReferenceIds.ToArray(),
                MissingEvidenceCodes = source.MissingEvidenceCodes.ToArray(),
            };
    }
}
