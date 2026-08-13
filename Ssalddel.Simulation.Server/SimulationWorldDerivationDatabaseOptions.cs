namespace Ssalddel.Simulation.Server;

public sealed class SimulationWorldDerivationDatabaseOptions
{
    public const string SectionName = "SimulationWorldDerivationDatabase";

    public bool Enabled { get; set; }

    public string ConnectionStringName { get; set; } = "SimulationWorldDerived";
}
