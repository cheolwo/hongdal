using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ssalddel.Simulation.Persistence;

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
