using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ssalddel.Simulation.Persistence;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Server 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "운영 상태와 Simulation 상태의 권위 경계를 유지한다.")]
public sealed class SimulationWorld파생DbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<SimulationWorld파생DbContext>
{
    public SimulationWorld파생DbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__SimulationWorldDerived");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:SimulationWorldDerived is required for design-time creation.");
        var options = new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysql =>
                {
                    mysql.MigrationsAssembly("Ssalddel.Simulation.Persistence");
                    mysql.MigrationsHistoryTable("__EF마이그레이션이력_시뮬레이션월드파생");
                })
            .Options;
        return new SimulationWorld파생DbContext(options);
    }
}
