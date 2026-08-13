using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorld파생원장저장결과
{
    public bool Inserted { get; set; }
    public string BuildStableId { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public int SourceCount { get; set; }
    public int NodeCount { get; set; }
    public int RelationCount { get; set; }
    public int BuildingPlacementCount { get; set; }
    public int GraphicsPlanCount { get; set; }
    public int UnityTransformProfileCount { get; set; }
    public int UnityTileManifestCount { get; set; }
    public int UnityArtifactCount { get; set; }
    public int VisualPlacementCount { get; set; }
}

public interface ISimulationWorld파생원장Store
{
    Task<SimulationWorld파생원장저장결과> 저장Async(
        SimulationWorld파생원장 ledger,
        CancellationToken cancellationToken);
}
}
