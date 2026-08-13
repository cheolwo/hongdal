using Microsoft.EntityFrameworkCore;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace Ssalddel.Simulation.Persistence;

public interface ISimulationDatabaseReadinessProbe
{
    string 데이터베이스이름 { get; }

    Task<bool> 연결가능Async(CancellationToken cancellationToken);
}

internal sealed class SimulationSharedPublicDataReadinessProbe(
    PublicDataIngestionDbContext dbContext) : ISimulationDatabaseReadinessProbe
{
    public string 데이터베이스이름 => "공유 공공데이터 DB";

    public Task<bool> 연결가능Async(CancellationToken cancellationToken) =>
        dbContext.Database.CanConnectAsync(cancellationToken);
}

internal sealed class SimulationWorldDerivedReadinessProbe(
    SimulationWorld파생DbContext dbContext) : ISimulationDatabaseReadinessProbe
{
    public string 데이터베이스이름 => "Simulation World 파생 DB";

    public Task<bool> 연결가능Async(CancellationToken cancellationToken) =>
        dbContext.Database.CanConnectAsync(cancellationToken);
}
