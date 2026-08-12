using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Server;

public sealed class DisabledSimulation공유공공데이터Reader
    : ISimulation공유공공데이터조회Port
{
    public const string ErrorCode = "SimulationSharedPublicDataDisabled";

    public Task<Simulation공유공공데이터조회결과> Kamis가격관측조회Async(
        string? itemName,
        int limit,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException(ErrorCode);
}
