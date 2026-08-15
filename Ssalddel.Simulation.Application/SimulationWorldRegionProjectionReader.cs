using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{

public sealed class SimulationWorld지역Projection조회Result
{
    public SimulationWorld지역Projection조회Result(
        bool databaseAvailable,
        SimulationWorldRegionProjectionResponse? projection)
    {
        파생Db사용가능 = databaseAvailable;
        Projection = projection;
    }

    public bool 파생Db사용가능 { get; }
    public SimulationWorldRegionProjectionResponse? Projection { get; }
}

public interface ISimulationWorld지역ProjectionReader
{
    Task<SimulationWorld지역Projection조회Result> 조회Async(
        string regionStableId,
        CancellationToken cancellationToken);
}

public sealed class DisabledSimulationWorld지역ProjectionReader
    : ISimulationWorld지역ProjectionReader
{
    public Task<SimulationWorld지역Projection조회Result> 조회Async(
        string regionStableId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(regionStableId))
            throw new ArgumentException("RegionStableIdMissing", nameof(regionStableId));
        return Task.FromResult(new SimulationWorld지역Projection조회Result(false, null));
    }
}
}
