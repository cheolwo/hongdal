using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class Simulation첫벌목성찰SeedEngine
    {
        public static Simulation첫벌목성찰SeedSnapshot Prepare(
            Simulation첫벌목성찰SeedRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionRequestRequired");
            RequireText(request.PlayerStableId,
                "SimulationFirstLoggingReflectionPlayerRequired");

            if (!request.ActionHistoryComplete)
                return NotReady(request.PlayerStableId,
                    Simulation첫벌목성찰Codes.ActionHistoryIncomplete);

            var firstLogging = (request.ActionRecords
                    ?? Array.Empty<Simulation행위발현Record>())
                .Where(value => value != null
                    && string.Equals(value.WorldInteractionId,
                        Simulation첫벌목성찰Codes.WorldInteractionId,
                        StringComparison.Ordinal)
                    && string.Equals(value.PrimaryOutcomeCode,
                        Simulation첫벌목성찰Codes.HarvestCompleted,
                        StringComparison.Ordinal)
                    && string.Equals(value.결과분류Code,
                        Simulation행위결과분류Codes.성공,
                        StringComparison.Ordinal)
                    && string.Equals(value.TriggerSourceCode,
                        SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                        StringComparison.Ordinal)
                    && string.Equals(value.InitiatorStableId,
                        request.PlayerStableId, StringComparison.Ordinal)
                    && string.Equals(value.ActorStableId,
                        request.PlayerStableId, StringComparison.Ordinal)
                    && string.Equals(value.ActorKindCode, "Player",
                        StringComparison.Ordinal))
                .OrderBy(value => value.AfterWorldRevision)
                .ThenBy(value => value.AppliedWorldTick)
                .ThenBy(value => value.행위기록StableId, StringComparer.Ordinal)
                .FirstOrDefault();

            if (firstLogging == null)
                return NotReady(request.PlayerStableId,
                    Simulation첫벌목성찰Codes.FirstPlayerLoggingRequired);
            ValidateActionRecord(firstLogging);

            var rest = request.SafeRestEvidence;
            if (rest == null
                || !rest.SafeRestConfirmed
                || !string.Equals(rest.PlayerStableId, request.PlayerStableId,
                    StringComparison.Ordinal)
                || !string.Equals(rest.PlaceStableId,
                    Simulation첫벌목성찰Codes.HansHouseSafeRest,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(rest.EvidenceStableId)
                || string.IsNullOrWhiteSpace(rest.RuleRevision))
            {
                return NotReady(request.PlayerStableId,
                    Simulation첫벌목성찰Codes.HansHouseSafeRestRequired,
                    firstLogging);
            }

            if (rest.AppliedWorldRevision < firstLogging.AfterWorldRevision)
                return NotReady(request.PlayerStableId,
                    Simulation첫벌목성찰Codes.SafeRestMustFollowLogging,
                    firstLogging);

            var seedStableId = CalculateSeedStableId(request.PlayerStableId,
                firstLogging, rest);
            var progress = request.PreviousProgress == null
                ? CreateProgress(seedStableId, firstLogging.행위기록StableId,
                    Array.Empty<string>(), false)
                : ValidatePreviousProgress(request.PreviousProgress,
                    seedStableId, firstLogging.행위기록StableId);

            return new Simulation첫벌목성찰SeedSnapshot
            {
                StatusCode = progress.StateCode,
                PlayerStableId = request.PlayerStableId,
                SeedStableId = seedStableId,
                SourceActionRecordStableId = firstLogging.행위기록StableId,
                SourceActionRecordHashSha256 = firstLogging.기록HashSha256,
                SourceAfterWorldRevision = firstLogging.AfterWorldRevision,
                SafeRestEvidenceStableId = rest.EvidenceStableId,
                Progress = progress,
            };
        }

        public static Simulation첫벌목성찰ProgressSnapshot CreateProgress(
            string seedStableId, string sourceActionRecordStableId,
            string[] connectedFragmentCodes, bool interrupted)
        {
            RequireText(seedStableId,
                "SimulationFirstLoggingReflectionSeedRequired");
            RequireText(sourceActionRecordStableId,
                "SimulationFirstLoggingReflectionActionRecordRequired");
            var connected = connectedFragmentCodes ?? Array.Empty<string>();
            var expected = Simulation첫벌목성찰Codes.OrderedFragmentCodes();
            if (connected.Length > expected.Length
                || connected.Where((value, index) => !string.Equals(value,
                    expected[index], StringComparison.Ordinal)).Any())
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionFragmentOrderInvalid");

            return new Simulation첫벌목성찰ProgressSnapshot
            {
                SeedStableId = seedStableId,
                SourceActionRecordStableId = sourceActionRecordStableId,
                ConnectedFragmentCodes = connected.ToArray(),
                StateCode = connected.Length == expected.Length
                    ? Simulation첫벌목성찰Codes.Completed
                    : interrupted
                        ? Simulation첫벌목성찰Codes.Interrupted
                        : Simulation첫벌목성찰Codes.Ready,
            };
        }

        public static Simulation첫벌목성찰RewardPreparationSnapshot
            PrepareReward(Simulation첫벌목성찰RewardPreparationRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionRewardRequestRequired");
            var seed = request.Seed ?? throw new SimulationContractException(
                "SimulationFirstLoggingReflectionSeedRequired");
            var record = request.SourceActionRecord
                ?? throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionActionRecordRequired");
            var profile = request.PlayerDomainProfile
                ?? throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionProfileRequired");
            ValidateActionRecord(record);
            if (!string.Equals(seed.PlayerStableId, profile.PlayerStableId,
                    StringComparison.Ordinal)
                || !string.Equals(seed.SourceActionRecordStableId,
                    record.행위기록StableId, StringComparison.Ordinal)
                || !string.Equals(seed.SourceActionRecordHashSha256,
                    record.기록HashSha256, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionLineageInvalid");

            var completed = CreateProgress(seed.SeedStableId,
                record.행위기록StableId, request.ConnectedFragmentCodes, false);
            if (!string.Equals(completed.StateCode,
                    Simulation첫벌목성찰Codes.Completed,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionIncomplete");

            var existing = (profile.명상기여기록들
                    ?? Array.Empty<Simulation명상숙련기여Snapshot>())
                .FirstOrDefault(value => string.Equals(
                    value.SourceActionRecordStableId,
                    record.행위기록StableId, StringComparison.Ordinal));
            if (existing != null)
            {
                return new Simulation첫벌목성찰RewardPreparationSnapshot
                {
                    SeedStableId = seed.SeedStableId,
                    RewardStatusCode =
                        Simulation첫벌목성찰Codes.RewardAlreadyApplied,
                    ExistingContributionStableId = existing.ContributionStableId,
                };
            }

            var focus = request.ApprovedFocusResult;
            if (focus == null || focus.명상경험증가Milli <= 0)
                return new Simulation첫벌목성찰RewardPreparationSnapshot
                {
                    SeedStableId = seed.SeedStableId,
                    RewardStatusCode = Simulation첫벌목성찰Codes
                        .ApprovedFocusEvidenceRequired,
                };
            if (!string.Equals(focus.SourceActionRecordStableId,
                    record.행위기록StableId, StringComparison.Ordinal)
                || focus.AppliedWorldRevision != record.AfterWorldRevision)
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionFocusLineageInvalid");

            return new Simulation첫벌목성찰RewardPreparationSnapshot
            {
                SeedStableId = seed.SeedStableId,
                RewardStatusCode = Simulation첫벌목성찰Codes.RewardReady,
                MeditationProgressionRequest = new Simulation명상숙련기여Request
                {
                    PlayerStableId = seed.PlayerStableId,
                    행위기록 = record,
                    집중판정결과 = focus,
                },
            };
        }

        private static Simulation첫벌목성찰ProgressSnapshot
            ValidatePreviousProgress(Simulation첫벌목성찰ProgressSnapshot value,
                string seedStableId, string recordStableId)
        {
            if (!string.Equals(value.SeedStableId, seedStableId,
                    StringComparison.Ordinal)
                || !string.Equals(value.SourceActionRecordStableId,
                    recordStableId, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionProgressLineageInvalid");
            return CreateProgress(seedStableId, recordStableId,
                value.ConnectedFragmentCodes,
                string.Equals(value.StateCode,
                    Simulation첫벌목성찰Codes.Interrupted,
                    StringComparison.Ordinal));
        }

        private static Simulation첫벌목성찰SeedSnapshot NotReady(
            string playerStableId, string reasonCode,
            Simulation행위발현Record? record = null)
            => new Simulation첫벌목성찰SeedSnapshot
            {
                PlayerStableId = playerStableId,
                SourceActionRecordStableId = record?.행위기록StableId
                    ?? string.Empty,
                SourceActionRecordHashSha256 = record?.기록HashSha256
                    ?? string.Empty,
                SourceAfterWorldRevision = record?.AfterWorldRevision ?? 0,
                ReasonCodes = new[] { reasonCode },
            };

        private static void ValidateActionRecord(Simulation행위발현Record value)
        {
            if (!string.Equals(value.행위기록StableId,
                    Simulation행위발현Ledger.CalculateRecordStableId(value),
                    StringComparison.Ordinal)
                || !string.Equals(value.기록HashSha256,
                    Simulation행위발현Ledger.CalculateRecordHash(value),
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationFirstLoggingReflectionActionRecordInvalid");
        }

        private static string CalculateSeedStableId(string playerStableId,
            Simulation행위발현Record record,
            Simulation안전휴식근거Snapshot rest)
        {
            var canonical = string.Join("\n", new[]
            {
                Simulation첫벌목성찰Codes.SchemaVersion,
                Simulation첫벌목성찰Codes.RuleRevision,
                playerStableId,
                record.행위기록StableId,
                record.기록HashSha256,
                rest.EvidenceStableId,
                rest.RuleRevision,
            });
            using var sha = SHA256.Create();
            var hash = BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
            return "reflection-seed:first-logging:" + hash.Substring(0, 32);
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
        }
    }
}
