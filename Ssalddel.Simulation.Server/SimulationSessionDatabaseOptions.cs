namespace Ssalddel.Simulation.Server;

public sealed class SimulationSessionDatabaseOptions
{
    public const string SectionName = "SimulationSessionDatabase";

    public bool Enabled { get; set; }

    public string ConnectionStringName { get; set; } = "SimulationSession";
}
