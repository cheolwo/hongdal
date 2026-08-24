using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ssalddel.Simulation.Persistence;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Server 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "운영 상태와 Simulation 상태의 권위 경계를 유지한다.")]
public sealed class SimulationSessionDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<SimulationSessionDbContext>
{
    public SimulationSessionDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__SimulationSession");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:SimulationSession is required for design-time creation.");
        var options = new DbContextOptionsBuilder<SimulationSessionDbContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysql =>
                {
                    mysql.MigrationsAssembly("Ssalddel.Simulation.Persistence");
                    mysql.MigrationsHistoryTable(
                        "__EF마이그레이션이력_시뮬레이션세션");
                })
            .Options;
        return new SimulationSessionDbContext(options);
    }
}
