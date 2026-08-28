namespace Ssalddel.Simulation.Server;

public sealed class SimulationIdentityOptions
{
    public const string SectionName = "SimulationIdentity";
    public const string OnlineWorldPolicy = "SimulationOnlineWorld";

    public bool Enabled { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
