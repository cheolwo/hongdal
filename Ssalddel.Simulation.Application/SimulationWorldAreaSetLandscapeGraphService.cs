using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public interface ISimulationWorldAreaSetGraphStore
{
    Task ReplaceAreaSetBuildAsync(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        IReadOnlyList<SimulationWorldLandscapeGraphResponse> graphs,
        CancellationToken cancellationToken = default);

    Task<SimulationWorldAreaSetDefinitionResponse?> ReadAreaSetAsync(
        string areaSetStableId,
        CancellationToken cancellationToken = default);

    Task<SimulationWorldLandscapeGraphResponse?> ReadGraphAsync(
        string landscapeGraphStableId,
        CancellationToken cancellationToken = default);

    Task<SimulationWorldLandscapeCompositionTileResponse?> ReadTileFacadeAsync(
        string tileKey,
        CancellationToken cancellationToken = default);
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public sealed class DisabledSimulationWorldAreaSetGraphStore : ISimulationWorldAreaSetGraphStore
{
    public Task ReplaceAreaSetBuildAsync(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        IReadOnlyList<SimulationWorldLandscapeGraphResponse> graphs,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<SimulationWorldAreaSetDefinitionResponse?> ReadAreaSetAsync(
        string areaSetStableId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<SimulationWorldAreaSetDefinitionResponse?>(null);

    public Task<SimulationWorldLandscapeGraphResponse?> ReadGraphAsync(
        string landscapeGraphStableId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<SimulationWorldLandscapeGraphResponse?>(null);

    public Task<SimulationWorldLandscapeCompositionTileResponse?> ReadTileFacadeAsync(
        string tileKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<SimulationWorldLandscapeCompositionTileResponse?>(null);
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public sealed class SimulationWorldAreaSetLandscapeGraphJobShell
{
    private readonly ISimulationWorldAreaSetDefinitionReader _definitionReader;
    private readonly ISimulationWorldLandscapeGrammarCatalogReader _grammarReader;
    private readonly ISimulationWorldLandscapeSkeletonSource _skeletonSource;
    private readonly SimulationWorldLandscapeGraphAssembler _assembler;
    private readonly ISimulationWorldAreaSetGraphStore _store;

    public SimulationWorldAreaSetLandscapeGraphJobShell(
        ISimulationWorldAreaSetDefinitionReader definitionReader,
        ISimulationWorldLandscapeGrammarCatalogReader grammarReader,
        ISimulationWorldLandscapeSkeletonSource skeletonSource,
        SimulationWorldLandscapeGraphAssembler assembler,
        ISimulationWorldAreaSetGraphStore store)
    {
        _definitionReader = definitionReader;
        _grammarReader = grammarReader;
        _skeletonSource = skeletonSource;
        _assembler = assembler;
        _store = store;
    }

    public async Task<IReadOnlyList<SimulationWorldLandscapeGraphResponse>> BuildAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_definitionReader.TryRead(out var definitions, out var definitionError))
            throw new InvalidOperationException(definitionError);
        var grammarAvailable = _grammarReader.TryRead(out var grammar, out var grammarError);
        if (grammarAvailable) grammar.ValidateCanonicalCatalog();

        var graphs = definitions.AreaSet.LandscapeGraphs
            .Select(descriptor => grammarAvailable
                ? BuildGraph(definitions.AreaSet, descriptor, grammar)
                : WaitingGraph(definitions.AreaSet, descriptor,
                    SimulationWorldLandscapeCompositionCodes.WaitingForGrammarManifest,
                    grammarError, string.Empty))
            .ToArray();
        SimulationWorldLandscapeGraphRelationValidator.Apply(definitions.AreaSet, graphs);
        definitions.AreaSet.LandscapeGraphs = graphs.Select(ToDescriptor).ToArray();
        await _store.ReplaceAreaSetBuildAsync(
            definitions.AreaSet, graphs, cancellationToken).ConfigureAwait(false);
        return graphs;
    }

    private SimulationWorldLandscapeGraphResponse BuildGraph(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        SimulationWorldLandscapeGraphDescriptorResponse descriptor,
        SimulationWorldLandscapeGrammarCatalog grammar)
    {
        if (descriptor.TileRefs.Length == 0)
            return WaitingGraph(areaSet, descriptor,
                SimulationWorldLandscapeCompositionCodes.Declared,
                "Graph의 공간 Tile 범위가 아직 선언되지 않았습니다.",
                grammar.CatalogHashSha256);

        var tileResults = new List<SimulationWorldLandscapeCompositionTileResponse>();
        var unresolved = new List<SimulationWorldLandscapeUnresolvedResponse>();
        foreach (var tileKey in descriptor.TileRefs.OrderBy(value => value, StringComparer.Ordinal))
        {
            if (!_skeletonSource.TryCreate(tileKey, out var skeleton, out var missingLayerCode))
            {
                unresolved.Add(new SimulationWorldLandscapeUnresolvedResponse
                {
                    UnresolvedStableId = "unresolved:" + descriptor.LandscapeGraphStableId + ":" + tileKey,
                    ReasonCode = SimulationWorldLandscapeCompositionCodes.WaitingForSpatialArtifact,
                    RequiredSemanticCode = "tile-layer:" + missingLayerCode,
                    EvidenceKindCode = SimulationWorldLandscapeAssemblyEvidenceCodes.Derived,
                    Detail = "필수 공간 Layer가 준비되지 않았습니다: " + tileKey,
                });
                continue;
            }
            skeleton.AreaSetStableId = areaSet.AreaSetStableId;
            tileResults.Add(_assembler.Assemble(skeleton, grammar));
        }

        if (tileResults.Count == 0)
            return WaitingGraph(areaSet, descriptor,
                SimulationWorldLandscapeCompositionCodes.WaitingForSpatialArtifact,
                "Graph가 참조하는 모든 Tile의 필수 공간 Layer가 준비되지 않았습니다.",
                grammar.CatalogHashSha256, unresolved);

        var nodes = tileResults.SelectMany(item => item.Nodes)
            .OrderBy(item => item.NodeStableId, StringComparer.Ordinal).ToArray();
        var nodeIds = nodes.Select(item => item.NodeStableId).ToHashSet(StringComparer.Ordinal);
        var placements = tileResults.SelectMany(item => item.Placements)
            .OrderBy(item => item.PlacementStableId, StringComparer.Ordinal).ToArray();
        var edges = tileResults.SelectMany(item => item.Edges)
            .Where(item => nodeIds.Contains(item.FromNodeStableId) && nodeIds.Contains(item.ToNodeStableId))
            .OrderBy(item => item.EdgeStableId, StringComparer.Ordinal).ToArray();
        var tileRefs = descriptor.TileRefs.ToHashSet(StringComparer.Ordinal);
        var stubs = tileResults.SelectMany(item => item.ExternalConnectorStubs)
            .Where(item => !tileRefs.Contains(item.NeighborTileKey))
            .OrderBy(item => item.StubStableId, StringComparer.Ordinal).ToArray();
        unresolved.AddRange(tileResults.SelectMany(item => item.Unresolved));
        var response = CreateBase(areaSet, descriptor, grammar.CatalogHashSha256);
        response.StatusCode = unresolved.Count == 0
            ? SimulationWorldLandscapeCompositionCodes.Available
            : SimulationWorldLandscapeCompositionCodes.PartialUnresolved;
        response.Nodes = nodes;
        response.Edges = edges;
        response.Placements = placements;
        response.ExternalConnectorStubs = stubs;
        response.Unresolved = unresolved.OrderBy(
            item => item.UnresolvedStableId, StringComparer.Ordinal).ToArray();
        SimulationWorldLandscapeGraphHasher.Finalize(response);
        return response;
    }

    private static SimulationWorldLandscapeGraphResponse WaitingGraph(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        SimulationWorldLandscapeGraphDescriptorResponse descriptor,
        string statusCode,
        string detail,
        string grammarHash,
        IEnumerable<SimulationWorldLandscapeUnresolvedResponse>? existing = null)
    {
        var response = CreateBase(areaSet, descriptor, grammarHash);
        response.StatusCode = statusCode;
        response.Unresolved = (existing ?? Array.Empty<SimulationWorldLandscapeUnresolvedResponse>())
            .Append(new SimulationWorldLandscapeUnresolvedResponse
            {
                UnresolvedStableId = "unresolved:" + descriptor.LandscapeGraphStableId + ":definition",
                ReasonCode = statusCode,
                RequiredSemanticCode = "landscape-graph-spatial-definition",
                EvidenceKindCode = SimulationWorldLandscapeAssemblyEvidenceCodes.Derived,
                Detail = detail,
            })
            .OrderBy(item => item.UnresolvedStableId, StringComparer.Ordinal).ToArray();
        SimulationWorldLandscapeGraphHasher.Finalize(response);
        return response;
    }

    private static SimulationWorldLandscapeGraphResponse CreateBase(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        SimulationWorldLandscapeGraphDescriptorResponse descriptor,
        string grammarHash) => new()
    {
        AreaSetStableId = areaSet.AreaSetStableId,
        LandscapeGraphStableId = descriptor.LandscapeGraphStableId,
        GraphRoleCode = descriptor.GraphRoleCode,
        GraphRevision = descriptor.GraphRevision,
        DefinitionHashSha256 = descriptor.DefinitionHashSha256,
        GrammarRevision = SimulationWorldLandscapeCompositionCodes.GrammarRevision,
        GrammarHashSha256 = grammarHash,
        Bounds = descriptor.Bounds,
        AreaRefs = descriptor.AreaRefs,
        TileRefs = descriptor.TileRefs,
        ScenarioRouteRefs = descriptor.ScenarioRouteRefs,
        PresentationOnly = true,
        IsOperationalState = false,
    };

    private static SimulationWorldLandscapeGraphDescriptorResponse ToDescriptor(
        SimulationWorldLandscapeGraphResponse graph) => new()
    {
        LandscapeGraphStableId = graph.LandscapeGraphStableId,
        GraphRoleCode = graph.GraphRoleCode,
        GraphRevision = graph.GraphRevision,
        DefinitionHashSha256 = graph.DefinitionHashSha256,
        BuildStatusCode = graph.StatusCode,
        GraphHashSha256 = graph.GraphHashSha256,
        Bounds = graph.Bounds,
        AreaRefs = graph.AreaRefs,
        TileRefs = graph.TileRefs,
        ScenarioRouteRefs = graph.ScenarioRouteRefs,
    };
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public sealed class SimulationWorldAreaSetLandscapeGraphService
{
    // D442 비저장 검토 진입. JobShell/AreaSet 원장/실제 World를 호출하지 않는다.
    public static Simulation경관조합검토Result ReviewCandidate(
        SimulationWorldLandscapeGrammarCatalog catalog, Simulation경관조합검토Input input,
        ISimulationFarmH2SurfaceReader? surface) =>
        new Simulation경관조합검토Service().Review(catalog, input, surface);

    private readonly ISimulationWorldAreaSetDefinitionReader _definitionReader;
    private readonly ISimulationWorldAreaSetGraphStore _store;

    public SimulationWorldAreaSetLandscapeGraphService(
        ISimulationWorldAreaSetDefinitionReader definitionReader,
        ISimulationWorldAreaSetGraphStore store)
    {
        _definitionReader = definitionReader;
        _store = store;
    }

    public async Task<SimulationWorldAreaSetDefinitionResponse?> ReadAreaSetAsync(
        string areaSetStableId,
        CancellationToken cancellationToken = default)
    {
        var normalized = PyeongchangAreaSetStableIds.NormalizeAreaSet(areaSetStableId);
        var stored = await _store.ReadAreaSetAsync(normalized, cancellationToken).ConfigureAwait(false);
        if (stored != null) return stored;
        return _definitionReader.TryRead(out var definitions, out _)
               && definitions.AreaSet.AreaSetStableId == normalized
            ? definitions.AreaSet
            : null;
    }

    public Task<SimulationWorldLandscapeGraphResponse?> ReadGraphAsync(
        string graphStableId,
        CancellationToken cancellationToken = default) =>
        _store.ReadGraphAsync(graphStableId, cancellationToken);

    public async Task<SimulationWorldLandscapeGraphIndexResponse?> ReadGraphIndexAsync(
        string areaSetStableId,
        string tileKey,
        int radiusTiles,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseL2(tileKey, out var centerX, out var centerY) || radiusTiles is < 0 or > 12)
            return null;
        var areaSet = await ReadAreaSetAsync(areaSetStableId, cancellationToken).ConfigureAwait(false);
        if (areaSet == null) return null;
        var covered = Enumerable.Range(centerX - radiusTiles, radiusTiles * 2 + 1)
            .SelectMany(x => Enumerable.Range(centerY - radiusTiles, radiusTiles * 2 + 1)
                .Select(y => "kr5186:l2:" + x + ":" + y))
            .OrderBy(value => value, StringComparer.Ordinal).ToArray();
        var coveredSet = covered.ToHashSet(StringComparer.Ordinal);
        return new SimulationWorldLandscapeGraphIndexResponse
        {
            AreaSetStableId = areaSet.AreaSetStableId,
            CenterTileKey = tileKey,
            RadiusTiles = radiusTiles,
            CoveredTileKeys = covered,
            Graphs = areaSet.LandscapeGraphs
                .Where(graph => graph.TileRefs.Any(coveredSet.Contains))
                .OrderBy(graph => graph.LandscapeGraphStableId, StringComparer.Ordinal).ToArray(),
            PresentationOnly = true,
        };
    }

    public Task<SimulationWorldLandscapeCompositionTileResponse?> ReadTileFacadeAsync(
        string tileKey,
        CancellationToken cancellationToken = default) =>
        _store.ReadTileFacadeAsync(tileKey, cancellationToken);

    private static bool TryParseL2(string tileKey, out int x, out int y)
    {
        x = y = 0;
        var parts = (tileKey ?? string.Empty).Split(':');
        return parts.Length == 4 && parts[0] == "kr5186" && parts[1] == "l2"
               && int.TryParse(parts[2], out x) && int.TryParse(parts[3], out y);
    }
}

/// <summary>
/// 서로 다른 Graph는 Node를 직접 참조하지 않고 AreaSet의 Connector pair로만 이어진다.
/// 양쪽 Graph가 모두 생성 가능한 시점에만 실제 stub 호환을 검사한다.
/// </summary>
[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public static class SimulationWorldLandscapeGraphRelationValidator
{
    private const double MaximumConnectorDistanceMeters = 2d;

    public static void Apply(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        IReadOnlyList<SimulationWorldLandscapeGraphResponse> graphs)
    {
        if (areaSet == null) throw new ArgumentNullException(nameof(areaSet));
        if (graphs == null) throw new ArgumentNullException(nameof(graphs));
        var byId = graphs.ToDictionary(
            item => item.LandscapeGraphStableId, StringComparer.Ordinal);
        foreach (var relation in areaSet.GraphRelations.OrderBy(
                     item => item.RelationStableId, StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(relation.FromGraphStableId, out var fromGraph)
                || !byId.TryGetValue(relation.ToGraphStableId, out var toGraph))
                throw new InvalidOperationException(
                    "LandscapeGraphRelationGraphMissing:" + relation.RelationStableId);
            if (!CanValidate(fromGraph) || !CanValidate(toGraph)) continue;

            var from = fromGraph.ExternalConnectorStubs.SingleOrDefault(item =>
                item.StubStableId == relation.ConnectorPair.FromConnectorStableId);
            var to = toGraph.ExternalConnectorStubs.SingleOrDefault(item =>
                item.StubStableId == relation.ConnectorPair.ToConnectorStableId);
            var failure = ValidatePair(relation, from, to);
            if (failure == null) continue;
            AddUnresolved(fromGraph, relation, failure);
            AddUnresolved(toGraph, relation, failure);
            SimulationWorldLandscapeGraphHasher.Finalize(fromGraph);
            SimulationWorldLandscapeGraphHasher.Finalize(toGraph);
        }
    }

    private static bool CanValidate(SimulationWorldLandscapeGraphResponse graph) =>
        graph.StatusCode is SimulationWorldLandscapeCompositionCodes.Available
            or SimulationWorldLandscapeCompositionCodes.PartialUnresolved;

    internal static string? ValidatePair(
        SimulationWorldLandscapeGraphRelationResponse relation,
        SimulationWorldLandscapeExternalConnectorResponse? from,
        SimulationWorldLandscapeExternalConnectorResponse? to)
    {
        if (from == null || to == null) return "양쪽 Connector stub이 모두 준비되지 않았습니다.";
        var expected = relation.ConnectorPair;
        if (from.ConnectorTypeCode != expected.ConnectorTypeCode
            || to.ConnectorTypeCode != expected.ConnectorTypeCode)
            return "Connector 종류가 AreaSet 관계 정의와 다릅니다.";
        if (from.RouteSignature != expected.RouteSignature
            || to.RouteSignature != expected.RouteSignature)
            return "Connector 경로 서명이 AreaSet 관계 정의와 다릅니다.";
        if (!AreOpposite(from.DirectionCode, to.DirectionCode))
            return "Connector 방향이 서로 마주보지 않습니다.";
        var dx = from.WorldEastingMeters - to.WorldEastingMeters;
        var dy = from.WorldNorthingMeters - to.WorldNorthingMeters;
        if (Math.Sqrt(dx * dx + dy * dy) > MaximumConnectorDistanceMeters)
            return "Connector 세계 좌표가 허용 거리보다 멉니다.";
        var widthTolerance = Math.Max(1d, Math.Max(from.WidthMeters, to.WidthMeters) * .2d);
        if (from.WidthMeters <= 0d || to.WidthMeters <= 0d
            || Math.Abs(from.WidthMeters - to.WidthMeters) > widthTolerance)
            return "Connector 폭이 서로 호환되지 않습니다.";
        return null;
    }

    private static bool AreOpposite(string from, string to) =>
        (from == "north" && to == "south")
        || (from == "south" && to == "north")
        || (from == "east" && to == "west")
        || (from == "west" && to == "east");

    private static void AddUnresolved(
        SimulationWorldLandscapeGraphResponse graph,
        SimulationWorldLandscapeGraphRelationResponse relation,
        string detail)
    {
        var stableId = "unresolved:" + graph.LandscapeGraphStableId
                       + ":relation:" + relation.RelationStableId;
        if (graph.Unresolved.Any(item => item.UnresolvedStableId == stableId)) return;
        graph.Unresolved = graph.Unresolved.Append(new SimulationWorldLandscapeUnresolvedResponse
        {
            UnresolvedStableId = stableId,
            ReasonCode = SimulationWorldLandscapeCompositionCodes.GraphConnectorUnresolved,
            RequiredSemanticCode = "graph-connector:" + relation.RelationStableId,
            EvidenceKindCode = SimulationWorldLandscapeAssemblyEvidenceCodes.Derived,
            Detail = detail,
        }).OrderBy(item => item.UnresolvedStableId, StringComparer.Ordinal).ToArray();
        graph.StatusCode = SimulationWorldLandscapeCompositionCodes.PartialUnresolved;
    }

}
}
