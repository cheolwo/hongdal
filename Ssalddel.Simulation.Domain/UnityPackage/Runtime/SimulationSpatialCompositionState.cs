using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly SimulationSpatialCompositionEngine
            spatialCompositionEngine = new SimulationSpatialCompositionEngine();
        private SimulationSpatialCompositionStateSnapshot?
            spatialCompositionState;

        private bool HasSpatialCompositionState =>
            spatialCompositionState != null;

        private bool IsSpatialCompositionEnabled => string.Equals(
            SpatialCompositionRuleRevision,
            SimulationSpatialCompositionCodes.RuleRevision,
            StringComparison.Ordinal);

        private void InitializeSpatialComposition()
        {
            if (!IsSpatialCompositionEnabled)
                return;
            spatialCompositionState = spatialCompositionEngine.Evaluate(
                PyeongchangHubSpatialCompositionFixture.CreateRequest(
                    CurrentTick, Revision, false));
        }

        private void EvaluateSpatialComposition()
        {
            if (!IsSpatialCompositionEnabled)
                return;
            spatialCompositionState = spatialCompositionEngine.Evaluate(
                PyeongchangHubSpatialCompositionFixture.CreateRequest(
                    CurrentTick, Revision + 1L, true,
                    spatialCompositionState));
        }

        public SimulationSpatialCompositionStateSnapshot GetSpatialComposition(
            string areaCode)
        {
            if (!string.Equals((areaCode ?? string.Empty).Trim(),
                    PyeongchangHubSpatialCompositionCodes.AreaCode,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationSpatialCompositionAreaCodeInvalid");
            lock (gate)
            {
                if (spatialCompositionState == null)
                    throw new SimulationNotFoundException(
                        "SimulationSpatialCompositionNotEnabled");
                return SimulationSpatialCompositionSnapshots.Clone(
                    spatialCompositionState);
            }
        }

        internal void RestoreSpatialCompositionState(
            SimulationSpatialCompositionStateSnapshot? value)
        {
            if (value == null) return;
            if (!string.Equals(value.GraphHashSha256,
                    SimulationSpatialCompositionEngine.ComputeGraphHash(value),
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationSpatialCompositionGraphHashMismatch");
            spatialCompositionState =
                SimulationSpatialCompositionSnapshots.Clone(value);
        }
    }
}
