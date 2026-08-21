using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server;

public sealed class DisabledSimulationFarmRealityOperationalReader
    : ISimulationFarmRealityOperationalReader
{
    public Task<SimulationFarmRealityEvidenceBundle> ReadApprovedAsync(
        string areaSetStableId, string canonicalProductStableId,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("SimulationFarmRealityEvidenceDisabled");
}

public sealed class DisabledSimulationFarmRealityEvidenceStore
    : ISimulationFarmRealityEvidenceStore
{
    public Task<SimulationFarmRealityEvidenceSyncResponse> UpsertAsync(
        SimulationFarmRealityEvidenceBundle bundle,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("SimulationFarmRealityEvidenceDisabled");

    public Task<SimulationFarmRealityEvidenceBundle> ReadLatestAsync(
        string areaSetStableId, string canonicalProductStableId,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("SimulationFarmRealityEvidenceNotFound");
}
