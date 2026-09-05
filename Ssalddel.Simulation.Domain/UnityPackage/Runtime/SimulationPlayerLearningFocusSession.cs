using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private Simulation학습중점InitialState? learningFocusCreationState;
        private Simulation학습중점State? learningFocusState;
        private string learningFocusInitialPayloadKey = string.Empty;

        public Simulation학습중점StateSnapshot GetLearningFocusState()
        {
            lock (gate) return RequireLearningFocusState().Snapshot();
        }

        public Simulation학습중점ProjectionSnapshot GetLearningFocusProjection()
        {
            lock (gate) return RequireLearningFocusState().Project(CurrentTick);
        }

        public Simulation학습중점PreviewSnapshot PreviewLearningFocusChange(
            Simulation학습중점ChangeRequest request)
        {
            lock (gate)
                return RequireLearningFocusState().Preview(request, CurrentTick);
        }

        public Simulation학습중점StateSnapshot ConfirmLearningFocusChange(
            Simulation학습중점ChangeRequest request)
        {
            lock (gate)
                return RequireLearningFocusState().Confirm(request, CurrentTick);
        }

        private void InitializeLearningFocus(
            Simulation학습중점InitialState? initial)
        {
            learningFocusInitialPayloadKey = BuildLearningFocusInitialPayloadKey(
                initial);
            if (initial == null) return;
            Simulation학습중점State.ValidateInitial(initial);
            if (!string.Equals(initial.SessionStableId, SessionStableId,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusSessionMismatch");
            if (initial.Segments.Any(value =>
                    value.EndWorldTickExclusive > DurationTicks))
                throw new SimulationContractException(
                    "SimulationLearningFocusScheduleExceedsSession");
            learningFocusCreationState = Simulation학습중점State.Clone(initial);
            learningFocusState = new Simulation학습중점State(
                learningFocusCreationState);
        }

        private void AdvanceLearningFocus(int previousTick, int currentTick)
            => learningFocusState?.Advance(previousTick, currentTick);

        private void ApplyLearningFocusToAction(
            Simulation행위발현Record record,
            string playerStableId,
            Simulation플레이어분야Engine playerDomain)
        {
            if (learningFocusState == null) return;
            if (learningFocusState.TryCreateContribution(record,
                    playerStableId, out var request, out var receipt))
            {
                playerDomain.ApplyNpcLearningFocus(request);
                learningFocusState.CommitContribution(receipt);
            }
        }

        private Simulation학습중점State RequireLearningFocusState()
            => learningFocusState ?? throw new SimulationNotFoundException(
                "SimulationLearningFocusStateNotFound");

        private Simulation학습중점StateSnapshot?
            CreateLearningFocusStateSnapshotOrNull()
            => learningFocusState?.Snapshot();

        internal void RestoreLearningFocusState(
            Simulation학습중점InitialState? initial,
            Simulation학습중점StateSnapshot? snapshot)
        {
            if (initial == null || snapshot == null)
                throw new SimulationContractException(
                    "SimulationLearningFocusSaveStateIncomplete");
            Simulation학습중점State.ValidateInitial(initial);
            var restored = Simulation학습중점State.Restore(snapshot);
            if (!string.Equals(initial.SessionStableId,
                    snapshot.SessionStableId, StringComparison.Ordinal)
                || !string.Equals(initial.PlayerStableId,
                    snapshot.PlayerStableId, StringComparison.Ordinal)
                || !string.Equals(initial.RuleRevision,
                    snapshot.RuleRevision, StringComparison.Ordinal)
                || !string.Equals(initial.ScheduleRevision,
                    snapshot.ScheduleRevision, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusSaveStateMismatch");
            learningFocusCreationState = Simulation학습중점State.Clone(initial);
            learningFocusInitialPayloadKey = BuildLearningFocusInitialPayloadKey(
                learningFocusCreationState);
            learningFocusState = restored;
        }

        internal static Simulation학습중점InitialState?
            CloneLearningFocusInitialStateOrNull(
                Simulation학습중점InitialState? source)
            => source == null ? null : Simulation학습중점State.Clone(source);

        internal static Simulation학습중점StateSnapshot?
            CloneLearningFocusStateOrNull(
                Simulation학습중점StateSnapshot? source)
            => source == null ? null : Simulation학습중점State.Clone(source);

        internal static string BuildLearningFocusInitialPayloadKey(
            Simulation학습중점InitialState? initial)
        {
            if (initial == null) return string.Empty;
            return string.Join("~", initial.SessionStableId,
                initial.PlayerStableId, initial.RuleRevision,
                initial.ScheduleRevision,
                string.Join(";", initial.Segments.Select(value => string.Join("|",
                    value.SegmentStableId, value.SolarTermStableId,
                    value.SolarTermRevision, value.PhaseCode,
                    value.StartWorldTickInclusive,
                    value.EndWorldTickExclusive))),
                string.Join(";", initial.Cards.OrderBy(value =>
                        value.CardStableId, StringComparer.Ordinal)
                    .Select(value => value.DefinitionHashSha256)),
                string.Join(",", initial.OwnedCardStableIds.OrderBy(value =>
                    value, StringComparer.Ordinal)), initial.ActiveCardStableId,
                initial.ActiveFromSegmentStableId);
        }
    }
}
