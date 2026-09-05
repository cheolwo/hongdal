using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationHexagramCampaignStateSnapshot? hexagramCampaignState;

        public SimulationHexagramCampaignStateSnapshot GetHexagramCampaignState()
        {
            lock (gate)
            {
                return CloneHexagramCampaignState(hexagramCampaignState)
                    ?? new SimulationHexagramCampaignStateSnapshot();
            }
        }

        public SimulationHexagramCampaignStateSnapshot BeginHexagramCampaign(
            SimulationHexagramCampaignEnterRequest request,
            string entrySaveStableId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireCampaignCommand(request.CommandId, request.HexagramStableId);
            if (string.IsNullOrWhiteSpace(entrySaveStableId))
                throw new SimulationContractException(
                    "HexagramCampaignEntrySaveStableIdInvalid");
            var wiIds = NormalizeWorldInteractionIds(
                request.LineWorldInteractionIds);
            if (request.StoryStageCount < 1)
                throw new SimulationContractException(
                    "HexagramCampaignStoryStageCountInvalid");

            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                if (hexagramCampaignState != null
                    && string.Equals(hexagramCampaignState.CampaignStateCode,
                        SimulationHexagramCampaignCodes.Active,
                        StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "HexagramCampaignAlreadyActive");

                var permanent = hexagramCampaignState?
                    .PermanentlyUnlockedWorldInteractionIds
                    ?? Array.Empty<string>();
                Revision++;
                hexagramCampaignState = new SimulationHexagramCampaignStateSnapshot
                {
                    CampaignStateCode = SimulationHexagramCampaignCodes.Active,
                    HexagramStableId = request.HexagramStableId.Trim(),
                    CurrentLineOrdinal = 1,
                    StoryStageCount = request.StoryStageCount,
                    AttemptOrdinal = 1,
                    AttemptVariationSeed = CalculateAttemptVariationSeed(
                        ScenarioSeed, request.HexagramStableId.Trim(), 1),
                    EntrySaveStableId = entrySaveStableId.Trim(),
                    EntryWorldRevision = Revision,
                    TemporaryWorldInteractionIds = wiIds,
                    PermanentlyUnlockedWorldInteractionIds = permanent.ToArray(),
                    Events = new[]
                    {
                        CampaignEvent("CampaignEntered", string.Empty, 1, 1),
                    },
                };
                AppendHexagramCampaignTransition();
                return CloneHexagramCampaignState(hexagramCampaignState)!;
            }
        }

        public SimulationHexagramCampaignStateSnapshot CompleteHexagramLine(
            SimulationHexagramCampaignLineCompleteRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireCampaignCommand(request.CommandId, "HEX");
            lock (gate)
            {
                var state = RequireActiveCampaign(request.ExpectedRevision);
                if (request.ExpectedLineOrdinal != state.CurrentLineOrdinal)
                    throw new SimulationConflictException(
                        "HexagramCampaignLineOrdinalMismatch");
                if (state.CurrentLineOrdinal >= state.StoryStageCount)
                    throw new SimulationConflictException(
                        "HexagramCampaignUpperLineRequiresCompletion");
                state.Events = AppendEvent(state.Events,
                    CampaignEvent("LineCompleted", string.Empty,
                        state.AttemptOrdinal, state.CurrentLineOrdinal));
                state.CurrentLineOrdinal++;
                Revision++;
                AppendHexagramCampaignTransition();
                return CloneHexagramCampaignState(state)!;
            }
        }

        public SimulationHexagramCampaignStateSnapshot RecordHexagramSetback(
            SimulationHexagramCampaignSetbackRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireCampaignCommand(request.CommandId, "HEX");
            if (!IsRecoverableReason(request.SetbackReasonCode))
                throw new SimulationContractException(
                    "HexagramCampaignSetbackReasonInvalid");
            lock (gate)
            {
                var state = RequireActiveCampaign(request.ExpectedRevision);
                state.Events = AppendEvent(state.Events,
                    CampaignEvent(
                        SimulationHexagramCampaignCodes.RecoverableSetback,
                        request.SetbackReasonCode.Trim(), state.AttemptOrdinal,
                        state.CurrentLineOrdinal));
                Revision++;
                AppendHexagramCampaignTransition();
                return CloneHexagramCampaignState(state)!;
            }
        }

        public string ValidateHexagramCampaignFailure(
            SimulationHexagramCampaignFailureRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireCampaignCommand(request.CommandId, "HEX");
            lock (gate)
            {
                var state = RequireActiveCampaign(request.ExpectedRevision);
                if (!IsIrrecoverableReason(state.HexagramStableId,
                        request.FailureReasonCode))
                    throw new SimulationContractException(
                        "HexagramCampaignFailureReasonRecoverable");
                return state.EntrySaveStableId;
            }
        }

        public SimulationHexagramCampaignStateSnapshot RestartHexagramCampaign(
            SimulationHexagramCampaignFailureRequest request,
            int nextAttemptOrdinal)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (nextAttemptOrdinal <= 1)
                throw new SimulationContractException(
                    "HexagramCampaignAttemptOrdinalInvalid");
            lock (gate)
            {
                var state = RequireActiveCampaign(Revision);
                if (!IsIrrecoverableReason(state.HexagramStableId,
                        request.FailureReasonCode))
                    throw new SimulationContractException(
                        "HexagramCampaignFailureReasonRecoverable");
                state.AttemptOrdinal = nextAttemptOrdinal;
                state.CurrentLineOrdinal = 1;
                state.AttemptVariationSeed = CalculateAttemptVariationSeed(
                    ScenarioSeed, state.HexagramStableId, nextAttemptOrdinal);
                state.Events = AppendEvent(state.Events,
                    CampaignEvent(SimulationHexagramCampaignCodes.CampaignFailure,
                        request.FailureReasonCode.Trim(), nextAttemptOrdinal, 1));
                Revision++;
                AppendHexagramCampaignTransition();
                return CloneHexagramCampaignState(state)!;
            }
        }

        public SimulationHexagramCampaignStateSnapshot CompleteHexagramCampaign(
            SimulationHexagramCampaignCompleteRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireCampaignCommand(request.CommandId, "HEX");
            lock (gate)
            {
                var state = RequireActiveCampaign(request.ExpectedRevision);
                if (state.CurrentLineOrdinal != state.StoryStageCount)
                    throw new SimulationConflictException(
                        "HexagramCampaignUpperLineNotReached");
                state.PermanentlyUnlockedWorldInteractionIds = state
                    .PermanentlyUnlockedWorldInteractionIds
                    .Concat(state.TemporaryWorldInteractionIds)
                    .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray();
                state.TemporaryWorldInteractionIds = Array.Empty<string>();
                state.CampaignStateCode = SimulationHexagramCampaignCodes.FreeRoam;
                state.Events = AppendEvent(state.Events,
                    CampaignEvent("CampaignCompleted", string.Empty,
                        state.AttemptOrdinal, state.CurrentLineOrdinal));
                Revision++;
                AppendHexagramCampaignTransition();
                return CloneHexagramCampaignState(state)!;
            }
        }

        internal void ReplayHexagramCampaignTransition(
            SimulationHexagramCampaignStateSnapshot state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            lock (gate)
            {
                Revision++;
                hexagramCampaignState = CloneHexagramCampaignState(state);
                AppendHexagramCampaignTransition();
            }
        }

        private void AppendHexagramCampaignTransition()
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes
                    .HexagramCampaignStateTransition,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                HexagramCampaignState = CloneHexagramCampaignState(
                    hexagramCampaignState),
            });

        internal void RestoreHexagramCampaignState(
            SimulationHexagramCampaignStateSnapshot? state)
        {
            lock (gate)
            {
                hexagramCampaignState = CloneHexagramCampaignState(state);
            }
        }

        internal static SimulationHexagramCampaignStateSnapshot?
            CloneHexagramCampaignState(
                SimulationHexagramCampaignStateSnapshot? source)
            => source == null ? null : new SimulationHexagramCampaignStateSnapshot
            {
                SchemaVersion = source.SchemaVersion,
                RuleRevision = source.RuleRevision,
                CampaignStateCode = source.CampaignStateCode,
                HexagramStableId = source.HexagramStableId,
                CurrentLineOrdinal = source.CurrentLineOrdinal,
                StoryStageCount = source.StoryStageCount,
                AttemptOrdinal = source.AttemptOrdinal,
                AttemptVariationSeed = source.AttemptVariationSeed,
                EntrySaveStableId = source.EntrySaveStableId,
                EntryWorldRevision = source.EntryWorldRevision,
                TemporaryWorldInteractionIds = (source
                    .TemporaryWorldInteractionIds ?? Array.Empty<string>())
                    .ToArray(),
                PermanentlyUnlockedWorldInteractionIds = (source
                    .PermanentlyUnlockedWorldInteractionIds
                    ?? Array.Empty<string>()).ToArray(),
                Events = (source.Events
                    ?? Array.Empty<SimulationHexagramCampaignEventSnapshot>())
                    .Select(value => new SimulationHexagramCampaignEventSnapshot
                    {
                        EventCode = value.EventCode,
                        ReasonCode = value.ReasonCode,
                        AttemptOrdinal = value.AttemptOrdinal,
                        LineOrdinal = value.LineOrdinal,
                        WorldTick = value.WorldTick,
                        WorldRevision = value.WorldRevision,
                    }).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private SimulationHexagramCampaignStateSnapshot RequireActiveCampaign(
            long expectedRevision)
        {
            if (expectedRevision != Revision)
                throw new SimulationConflictException(
                    "SimulationExpectedRevisionMismatch");
            if (hexagramCampaignState == null
                || !string.Equals(hexagramCampaignState.CampaignStateCode,
                    SimulationHexagramCampaignCodes.Active,
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "HexagramCampaignNotActive");
            return hexagramCampaignState;
        }

        private SimulationHexagramCampaignEventSnapshot CampaignEvent(
            string eventCode, string reasonCode, int attemptOrdinal,
            int lineOrdinal)
            => new SimulationHexagramCampaignEventSnapshot
            {
                EventCode = eventCode,
                ReasonCode = reasonCode,
                AttemptOrdinal = attemptOrdinal,
                LineOrdinal = lineOrdinal,
                WorldTick = CurrentTick,
                WorldRevision = Revision + 1,
            };

        private static SimulationHexagramCampaignEventSnapshot[] AppendEvent(
            IEnumerable<SimulationHexagramCampaignEventSnapshot> events,
            SimulationHexagramCampaignEventSnapshot next)
            => events.Concat(new[] { next }).ToArray();

        private static string[] NormalizeWorldInteractionIds(string[]? values)
            => (values ?? Array.Empty<string>())
                .Select(value => value?.Trim() ?? string.Empty)
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private static void RequireCampaignCommand(string commandId,
            string stableId)
        {
            if (string.IsNullOrWhiteSpace(commandId)
                || string.IsNullOrWhiteSpace(stableId))
                throw new SimulationContractException(
                    "HexagramCampaignCommandInvalid");
        }

        private static bool IsRecoverableReason(string reasonCode)
            => string.Equals(reasonCode, SimulationHexagramCampaignCodes.Injury,
                    StringComparison.Ordinal)
                || string.Equals(reasonCode, SimulationHexagramCampaignCodes.Delay,
                    StringComparison.Ordinal)
                || string.Equals(reasonCode,
                    SimulationHexagramCampaignCodes.PartialFacilityDamage,
                    StringComparison.Ordinal)
                || string.Equals(reasonCode,
                    SimulationHexagramCampaignCodes.ResourceLoss,
                    StringComparison.Ordinal);

        private static bool IsIrrecoverableReason(string hexagramStableId,
            string reasonCode)
            => string.Equals(hexagramStableId,
                    SimulationHexagramCampaignCodes.ZhunStableId,
                    StringComparison.Ordinal)
                && (string.Equals(reasonCode,
                        SimulationHexagramCampaignCodes.HansLost,
                        StringComparison.Ordinal)
                    || string.Equals(reasonCode,
                        SimulationHexagramCampaignCodes.HansFarmFullyLost,
                        StringComparison.Ordinal));

        private static int CalculateAttemptVariationSeed(int scenarioSeed,
            string hexagramStableId, int attemptOrdinal)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("|",
                scenarioSeed, hexagramStableId, attemptOrdinal,
                SimulationHexagramCampaignCodes.RuleRevision)));
            return BitConverter.ToInt32(bytes, 0) & int.MaxValue;
        }
    }
}
