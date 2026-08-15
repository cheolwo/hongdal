using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ssalddel.Simulation.Persistence;

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
