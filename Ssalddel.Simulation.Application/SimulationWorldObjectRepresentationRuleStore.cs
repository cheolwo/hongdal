using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorld객체표현규칙대장저장결과
{
    public bool Inserted { get; set; }
    public string CatalogRevision { get; set; } = string.Empty;
    public string CatalogHashSha256 { get; set; } = string.Empty;
    public int SpatialRuleCount { get; set; }
    public int SimulationRuleCount { get; set; }
    public int BindingRuleCount { get; set; }
}

public sealed class SimulationWorld객체표현해석저장결과
{
    public bool Inserted { get; set; }
    public string InterpretationStableId { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public int ResultCount { get; set; }
}

public interface ISimulationWorld객체표현규칙Store
{
    Task<SimulationWorld객체표현규칙대장저장결과> 규칙대장저장Async(
        SimulationWorld객체표현규칙대장 catalog,
        CancellationToken cancellationToken);

    Task<SimulationWorld객체표현해석저장결과> 해석결과저장Async(
        SimulationWorld객체표현해석원장 ledger,
        CancellationToken cancellationToken);
}

public sealed class SimulationWorld객체표현해석JobShell
{
    public const string SpatialBuildNotFoundCode = "SimulationWorldObjectRepresentationSpatialBuildNotFound";
    public const string SpatialOutputMismatchCode = "SimulationWorldObjectRepresentationSpatialOutputMismatch";
    public const string TargetNodeNotFoundCode = "SimulationWorldObjectRepresentationTargetNodeNotFound";

    private readonly ISimulationWorld공간실행Reader _spatialReader;
    private readonly ISimulationWorld객체표현규칙Store _store;

    public SimulationWorld객체표현해석JobShell(
        ISimulationWorld공간실행Reader spatialReader,
        ISimulationWorld객체표현규칙Store store)
    {
        _spatialReader = spatialReader;
        _store = store;
    }

    public async Task<SimulationWorld객체표현해석저장결과> 실행Async(
        SimulationWorld객체표현해석요청 request,
        SimulationWorld객체표현규칙대장 catalog,
        CancellationToken cancellationToken)
    {
        SimulationWorld객체표현규칙Validator.ValidateRequest(request, catalog);
        var spatial = await _spatialReader.조회Async(request.SpatialBuildStableId, cancellationToken);
        if (spatial == null) throw new InvalidOperationException(SpatialBuildNotFoundCode);
        if (!string.Equals(spatial.OutputHashSha256, request.SpatialOutputHashSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(SpatialOutputMismatchCode);
        var nodeIds = new HashSet<string>(spatial.Nodes.Select(item => item.StableId), StringComparer.Ordinal);
        if (request.Targets.Any(item => !nodeIds.Contains(item.TargetNodeStableId)))
            throw new InvalidOperationException(TargetNodeNotFoundCode);
        var ledger = SimulationWorld객체표현해석기.Interpret(request, catalog);
        return await _store.해석결과저장Async(ledger, cancellationToken);
    }
}
}
