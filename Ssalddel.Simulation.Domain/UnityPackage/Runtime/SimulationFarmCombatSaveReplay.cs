using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private void AppendCombatPerspectiveConfirmCommand(
            SimulationCombatPerspectiveConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.CombatPerspectiveConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                CombatPerspectiveConfirmRequest =
                    SimulationSaveReplayCloner.CloneCombatPerspectiveConfirmRequest(request),
            });

        private void AppendCombatBeatStartCommand(
            SimulationCombatBeatStartRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.CombatBeatStart,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                CombatBeatStartRequest =
                    SimulationSaveReplayCloner.CloneCombatBeatStartRequest(request),
            });

        private void AppendCombatReactionConfirmCommand(
            SimulationCombatReactionConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.CombatReactionConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                CombatReactionConfirmRequest =
                    SimulationSaveReplayCloner.CloneCombatReactionConfirmRequest(request),
            });

        private void AppendTacticalOrderConfirmCommand(
            SimulationTacticalOrderConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TacticalOrderConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TacticalOrderConfirmRequest =
                    SimulationSaveReplayCloner.CloneTacticalOrderConfirmRequest(request),
            });
    }
}
