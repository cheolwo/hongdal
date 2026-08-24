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

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
public interface ISimulationWorld파생원장Store
{
    Task<SimulationWorld파생원장저장결과> 저장Async(
        SimulationWorld파생원장 ledger,
        CancellationToken cancellationToken);
}
}
