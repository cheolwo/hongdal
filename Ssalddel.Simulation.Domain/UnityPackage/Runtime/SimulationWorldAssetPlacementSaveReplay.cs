using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationWorldAssetPlacementStateSnapshot?
            worldAssetPlacementState;

        internal void RestoreWorldAssetPlacementState(
            SimulationWorldAssetPlacementStateSnapshot? value)
        {
            if (value != null)
                SimulationWorldAssetPlacementSaveReplay.Validate(value,
                    Revision);
            worldAssetPlacementState = SimulationSaveReplayCloner
                .CloneWorldAssetPlacementState(value);
        }
    }

    internal static class SimulationWorldAssetPlacementSaveReplay
    {
        public static void Validate(
            SimulationWorldAssetPlacementStateSnapshot? state,
            long expectedWorldRevision)
        {
            if (state == null
                || !string.Equals(state.SchemaVersion,
                    Simulation세계자산배치Codes.AssetSchemaVersion,
                    StringComparison.Ordinal)
                || state.SourceWorldRevision != expectedWorldRevision
                || state.MapPlans == null
                || state.ChangeProjections == null
                || state.SpawnDecisionPlans == null
                || state.AssetPlacementPlans == null
                || state.InteriorPlanBodies == null
                || !string.Equals(state.StateHashSha256,
                    Simulation세계자산CanonicalHash.ComputeStateHash(state),
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationWorldAssetPlacementStateInvalid");
        }
    }
}
