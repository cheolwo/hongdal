using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorldAreaSet정의Entity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string DefinitionHashSha256 { get; set; } = string.Empty;
    public string DocumentHashSha256 { get; set; } = string.Empty;
    public string DefinitionStatusCode { get; set; } = string.Empty;
    public bool PresentationOnly { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorldAreaSet공간참조Entity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public int AreaSetRevision { get; set; }
    public string ReferenceKindCode { get; set; } = string.Empty;
    public string ReferenceStableId { get; set; } = string.Empty;
    public int ReferenceOrder { get; set; }
}

public sealed class SimulationWorldAreaSetGraph참조Entity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public int AreaSetRevision { get; set; }
    public string LandscapeGraphStableId { get; set; } = string.Empty;
    public int ReferenceOrder { get; set; }
}

public sealed class SimulationWorld경관Graph정의Entity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public string LandscapeGraphStableId { get; set; } = string.Empty;
    public string GraphRoleCode { get; set; } = string.Empty;
    public int GraphRevision { get; set; }
    public string DefinitionHashSha256 { get; set; } = string.Empty;
    public string BuildStatusCode { get; set; } = string.Empty;
    public string GraphHashSha256 { get; set; } = string.Empty;
    public bool HasBounds { get; set; }
    public double MinEastingMeters { get; set; }
    public double MinNorthingMeters { get; set; }
    public double MaxEastingMeters { get; set; }
    public double MaxNorthingMeters { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld경관Graph공간참조Entity
{
    public long Id { get; set; }
    public string LandscapeGraphStableId { get; set; } = string.Empty;
    public int GraphRevision { get; set; }
    public string ReferenceKindCode { get; set; } = string.Empty;
    public string ReferenceStableId { get; set; } = string.Empty;
    public int ReferenceOrder { get; set; }
}

public sealed class SimulationWorld경관GraphTile참조Entity
{
    public long Id { get; set; }
    public string LandscapeGraphStableId { get; set; } = string.Empty;
    public int GraphRevision { get; set; }
    public string TileKey { get; set; } = string.Empty;
    public int ReferenceOrder { get; set; }
}

public sealed class SimulationWorld경관Graph관계Entity
{
    public long Id { get; set; }
    public string AreaSetStableId { get; set; } = string.Empty;
    public int AreaSetRevision { get; set; }
    public string RelationStableId { get; set; } = string.Empty;
    public string FromGraphStableId { get; set; } = string.Empty;
    public string ToGraphStableId { get; set; } = string.Empty;
    public string RelationCode { get; set; } = string.Empty;
    public string FromConnectorStableId { get; set; } = string.Empty;
    public string ToConnectorStableId { get; set; } = string.Empty;
    public string ConnectorTypeCode { get; set; } = string.Empty;
    public string RouteSignature { get; set; } = string.Empty;
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 AreaSet·Graph·통행 문맥을 저장한다.",
    Boundary = "Graph 저장은 WI Invocation·Task·Effect·결과를 대신하지 않는다.")]
public sealed class SimulationWorldAreaSetGraphStore(SimulationWorld파생DbContext dbContext)
    : ISimulationWorldAreaSetGraphStore
{
    private const string AreaRef = "Area";
    private const string ScenarioRouteRef = "ScenarioRoute";
    private const string CompletionAreaRef = "CompletionArea";

    public async Task ReplaceAreaSetBuildAsync(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        IReadOnlyList<SimulationWorldLandscapeGraphResponse> graphs,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        var definition = await dbContext.AreaSetDefinitions.SingleOrDefaultAsync(item =>
                item.AreaSetStableId == areaSet.AreaSetStableId && item.Revision == areaSet.Revision,
            cancellationToken).ConfigureAwait(false);
        if (definition != null && definition.DefinitionHashSha256 != areaSet.DefinitionHashSha256)
            throw new InvalidOperationException("AreaSetRevisionHashConflict");
        if (definition == null)
        {
            definition = new SimulationWorldAreaSet정의Entity
            {
                AreaSetStableId = areaSet.AreaSetStableId,
                Revision = areaSet.Revision,
                Title = areaSet.Title,
                Summary = areaSet.Summary,
                DefinitionHashSha256 = areaSet.DefinitionHashSha256,
                DocumentHashSha256 = areaSet.DocumentHashSha256,
                DefinitionStatusCode = areaSet.DefinitionStatusCode,
                PresentationOnly = areaSet.PresentationOnly,
                StoredAtUtc = DateTimeOffset.UtcNow,
            };
            dbContext.AreaSetDefinitions.Add(definition);
            AddAreaSetRefs(areaSet);
        }
        else
        {
            definition.DocumentHashSha256 = areaSet.DocumentHashSha256;
            definition.DefinitionStatusCode = areaSet.DefinitionStatusCode;
            definition.StoredAtUtc = DateTimeOffset.UtcNow;
            await UpdateAreaSetGraphOrderAsync(areaSet, cancellationToken).ConfigureAwait(false);
        }

        foreach (var graph in graphs)
        {
            var graphDefinition = await dbContext.LandscapeGraphDefinitions.SingleOrDefaultAsync(item =>
                    item.LandscapeGraphStableId == graph.LandscapeGraphStableId
                    && item.GraphRevision == graph.GraphRevision,
                cancellationToken).ConfigureAwait(false);
            if (graphDefinition != null
                && graphDefinition.DefinitionHashSha256 != graph.DefinitionHashSha256)
                throw new InvalidOperationException("LandscapeGraphRevisionHashConflict");
            if (graphDefinition == null)
            {
                graphDefinition = new SimulationWorld경관Graph정의Entity
                {
                    AreaSetStableId = areaSet.AreaSetStableId,
                    LandscapeGraphStableId = graph.LandscapeGraphStableId,
                    GraphRoleCode = graph.GraphRoleCode,
                    GraphRevision = graph.GraphRevision,
                    DefinitionHashSha256 = graph.DefinitionHashSha256,
                    HasBounds = HasBounds(graph.Bounds),
                    MinEastingMeters = graph.Bounds.MinEastingMeters,
                    MinNorthingMeters = graph.Bounds.MinNorthingMeters,
                    MaxEastingMeters = graph.Bounds.MaxEastingMeters,
                    MaxNorthingMeters = graph.Bounds.MaxNorthingMeters,
                };
                dbContext.LandscapeGraphDefinitions.Add(graphDefinition);
                AddGraphRefs(graph);
            }
            graphDefinition.BuildStatusCode = graph.StatusCode;
            graphDefinition.GraphHashSha256 = graph.GraphHashSha256;
            graphDefinition.StoredAtUtc = DateTimeOffset.UtcNow;

            if (!await dbContext.LandscapeAssemblyRuns.AnyAsync(item =>
                    item.GraphBuildStableId == graph.GraphBuildStableId, cancellationToken)
                .ConfigureAwait(false))
                await AddGraphRunAsync(graph, cancellationToken).ConfigureAwait(false);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<SimulationWorldAreaSetDefinitionResponse?> ReadAreaSetAsync(
        string areaSetStableId,
        CancellationToken cancellationToken = default)
    {
        var definition = await dbContext.AreaSetDefinitions.AsNoTracking()
            .Where(item => item.AreaSetStableId == areaSetStableId)
            .OrderByDescending(item => item.Revision).FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (definition == null) return null;
        var spatialRefs = await dbContext.AreaSetSpatialRefs.AsNoTracking()
            .Where(item => item.AreaSetStableId == areaSetStableId
                           && item.AreaSetRevision == definition.Revision)
            .OrderBy(item => item.ReferenceOrder).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var graphRefs = await dbContext.AreaSetGraphRefs.AsNoTracking()
            .Where(item => item.AreaSetStableId == areaSetStableId
                           && item.AreaSetRevision == definition.Revision)
            .OrderBy(item => item.ReferenceOrder).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var graphIds = graphRefs.Select(item => item.LandscapeGraphStableId).ToList();
        var graphDefinitions = await dbContext.LandscapeGraphDefinitions.AsNoTracking()
            .Where(item => graphIds.Contains(item.LandscapeGraphStableId))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var graphSpatialRefs = await dbContext.LandscapeGraphSpatialRefs.AsNoTracking()
            .Where(item => graphIds.Contains(item.LandscapeGraphStableId))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var tileRefs = await dbContext.LandscapeGraphTileRefs.AsNoTracking()
            .Where(item => graphIds.Contains(item.LandscapeGraphStableId))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var relations = await dbContext.LandscapeGraphRelations.AsNoTracking()
            .Where(item => item.AreaSetStableId == areaSetStableId
                           && item.AreaSetRevision == definition.Revision)
            .OrderBy(item => item.RelationStableId).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var descriptors = graphRefs.Select(reference =>
        {
            var item = graphDefinitions.Where(value =>
                    value.LandscapeGraphStableId == reference.LandscapeGraphStableId)
                .OrderByDescending(value => value.GraphRevision).First();
            return new SimulationWorldLandscapeGraphDescriptorResponse
            {
                LandscapeGraphStableId = item.LandscapeGraphStableId,
                GraphRoleCode = item.GraphRoleCode,
                GraphRevision = item.GraphRevision,
                DefinitionHashSha256 = item.DefinitionHashSha256,
                BuildStatusCode = item.BuildStatusCode,
                GraphHashSha256 = item.GraphHashSha256,
                Bounds = Bounds(item),
                AreaRefs = OrderedGraphRefs(graphSpatialRefs, item, AreaRef),
                ScenarioRouteRefs = OrderedGraphRefs(graphSpatialRefs, item, ScenarioRouteRef),
                TileRefs = tileRefs.Where(value =>
                        value.LandscapeGraphStableId == item.LandscapeGraphStableId
                        && value.GraphRevision == item.GraphRevision)
                    .OrderBy(value => value.ReferenceOrder).Select(value => value.TileKey).ToArray(),
            };
        }).ToArray();

        return new SimulationWorldAreaSetDefinitionResponse
        {
            AreaSetStableId = definition.AreaSetStableId,
            Revision = definition.Revision,
            Title = definition.Title,
            Summary = definition.Summary,
            DefinitionHashSha256 = definition.DefinitionHashSha256,
            DocumentHashSha256 = definition.DocumentHashSha256,
            DefinitionStatusCode = definition.DefinitionStatusCode,
            AreaRefs = SpatialRefs(spatialRefs, AreaRef),
            ScenarioRouteRefs = SpatialRefs(spatialRefs, ScenarioRouteRef),
            CompletionAreaRefs = SpatialRefs(spatialRefs, CompletionAreaRef),
            LandscapeGraphs = descriptors,
            GraphRelations = relations.Select(item => new SimulationWorldLandscapeGraphRelationResponse
            {
                RelationStableId = item.RelationStableId,
                FromGraphStableId = item.FromGraphStableId,
                ToGraphStableId = item.ToGraphStableId,
                RelationCode = item.RelationCode,
                ConnectorPair = new SimulationWorldLandscapeConnectorPairResponse
                {
                    FromConnectorStableId = item.FromConnectorStableId,
                    ToConnectorStableId = item.ToConnectorStableId,
                    ConnectorTypeCode = item.ConnectorTypeCode,
                    RouteSignature = item.RouteSignature,
                },
            }).ToArray(),
            PresentationOnly = definition.PresentationOnly,
            IsOperationalState = false,
        };
    }

    public async Task<SimulationWorldLandscapeGraphResponse?> ReadGraphAsync(
        string landscapeGraphStableId,
        CancellationToken cancellationToken = default)
    {
        var run = await dbContext.LandscapeAssemblyRuns.AsNoTracking()
            .Where(item => item.LandscapeGraphStableId == landscapeGraphStableId
                           && item.BuildScopeCode == SimulationWorldLandscapeCompositionCodes.GraphBuildScope)
            .OrderByDescending(item => item.StoredAtUtc).ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return run == null ? null : await MapGraphAsync(run, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SimulationWorldLandscapeCompositionTileResponse?> ReadTileFacadeAsync(
        string tileKey,
        CancellationToken cancellationToken = default)
    {
        var graphIds = (await dbContext.LandscapeGraphTileRefs.AsNoTracking()
            .Where(item => item.TileKey == tileKey)
            .Select(item => item.LandscapeGraphStableId).Distinct()
            .ToArrayAsync(cancellationToken).ConfigureAwait(false)).ToList();
        if (graphIds.Count == 0)
            return await new SimulationWorldLandscapeCompositionStore(dbContext)
                .ReadLatestAsync(tileKey, cancellationToken).ConfigureAwait(false);
        var candidateRuns = await dbContext.LandscapeAssemblyRuns.AsNoTracking()
            .Where(item => graphIds.Contains(item.LandscapeGraphStableId)
                           && item.BuildScopeCode == SimulationWorldLandscapeCompositionCodes.GraphBuildScope)
            .OrderByDescending(item => item.StoredAtUtc).ThenByDescending(item => item.Id)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var runs = candidateRuns.GroupBy(item => item.LandscapeGraphStableId, StringComparer.Ordinal)
            .Select(group => group.First()).ToArray();
        if (runs.Length == 0) return null;
        var graphs = new List<SimulationWorldLandscapeGraphResponse>();
        foreach (var run in runs)
            graphs.Add(await MapGraphAsync(run, cancellationToken).ConfigureAwait(false));
        var placements = graphs.SelectMany(item => item.Placements)
            .Where(item => item.OwnerTileKey == tileKey)
            .GroupBy(item => item.PlacementStableId, StringComparer.Ordinal)
            .Select(group => group.First()).OrderBy(item => item.PlacementStableId, StringComparer.Ordinal)
            .ToArray();
        var nodeIds = placements.Select(item => item.NodeStableId).ToHashSet(StringComparer.Ordinal);
        var nodes = graphs.SelectMany(item => item.Nodes).Where(item => nodeIds.Contains(item.NodeStableId))
            .GroupBy(item => item.NodeStableId, StringComparer.Ordinal).Select(group => group.First()).ToArray();
        var edges = graphs.SelectMany(item => item.Edges)
            .Where(item => nodeIds.Contains(item.FromNodeStableId) && nodeIds.Contains(item.ToNodeStableId))
            .GroupBy(item => item.EdgeStableId, StringComparer.Ordinal).Select(group => group.First()).ToArray();
        var placementIds = placements.Select(item => item.PlacementStableId).ToHashSet(StringComparer.Ordinal);
        var first = graphs[0];
        var response = new SimulationWorldLandscapeCompositionTileResponse
        {
            TileKey = tileKey,
            AreaSetStableId = first.AreaSetStableId,
            GraphBuildStableId = "legacy-tile-view:" + tileKey,
            GrammarRevision = first.GrammarRevision,
            GrammarHashSha256 = first.GrammarHashSha256,
            StatusCode = placements.Length == 0
                ? graphs.Select(item => item.StatusCode).First()
                : graphs.Any(item => item.StatusCode == SimulationWorldLandscapeCompositionCodes.PartialUnresolved)
                    ? SimulationWorldLandscapeCompositionCodes.PartialUnresolved
                    : SimulationWorldLandscapeCompositionCodes.Available,
            Nodes = nodes,
            Edges = edges,
            Placements = placements,
            ExternalConnectorStubs = graphs.SelectMany(item => item.ExternalConnectorStubs)
                .Where(item => placementIds.Contains(item.PlacementStableId)).ToArray(),
            Unresolved = graphs.SelectMany(item => item.Unresolved)
                .GroupBy(item => item.UnresolvedStableId, StringComparer.Ordinal)
                .Select(group => group.First()).ToArray(),
            PresentationOnly = true,
            IsOperationalState = false,
        };
        response.GraphHashSha256 = SimulationWorldLandscapeGraphAssembler.HashCanonical(response);
        response.GraphBuildStableId += ":" + response.GraphHashSha256[..16];
        return response;
    }

    private void AddAreaSetRefs(SimulationWorldAreaSetDefinitionResponse areaSet)
    {
        var order = 0;
        foreach (var item in areaSet.AreaRefs)
            AddAreaSetSpatialRef(areaSet, AreaRef, item, order++);
        order = 0;
        foreach (var item in areaSet.ScenarioRouteRefs)
            AddAreaSetSpatialRef(areaSet, ScenarioRouteRef, item, order++);
        order = 0;
        foreach (var item in areaSet.CompletionAreaRefs)
            AddAreaSetSpatialRef(areaSet, CompletionAreaRef, item, order++);
        dbContext.AreaSetGraphRefs.AddRange(areaSet.LandscapeGraphs.Select((item, index) =>
            new SimulationWorldAreaSetGraph참조Entity
            {
                AreaSetStableId = areaSet.AreaSetStableId,
                AreaSetRevision = areaSet.Revision,
                LandscapeGraphStableId = item.LandscapeGraphStableId,
                ReferenceOrder = index,
            }));
        dbContext.LandscapeGraphRelations.AddRange(areaSet.GraphRelations.Select(item =>
            new SimulationWorld경관Graph관계Entity
            {
                AreaSetStableId = areaSet.AreaSetStableId,
                AreaSetRevision = areaSet.Revision,
                RelationStableId = item.RelationStableId,
                FromGraphStableId = item.FromGraphStableId,
                ToGraphStableId = item.ToGraphStableId,
                RelationCode = item.RelationCode,
                FromConnectorStableId = item.ConnectorPair.FromConnectorStableId,
                ToConnectorStableId = item.ConnectorPair.ToConnectorStableId,
                ConnectorTypeCode = item.ConnectorPair.ConnectorTypeCode,
                RouteSignature = item.ConnectorPair.RouteSignature,
            }));
    }

    private void AddAreaSetSpatialRef(
        SimulationWorldAreaSetDefinitionResponse areaSet, string kind, string stableId, int order) =>
        dbContext.AreaSetSpatialRefs.Add(new SimulationWorldAreaSet공간참조Entity
        {
            AreaSetStableId = areaSet.AreaSetStableId,
            AreaSetRevision = areaSet.Revision,
            ReferenceKindCode = kind,
            ReferenceStableId = stableId,
            ReferenceOrder = order,
        });

    private async Task UpdateAreaSetGraphOrderAsync(
        SimulationWorldAreaSetDefinitionResponse areaSet,
        CancellationToken cancellationToken)
    {
        var storedRefs = await dbContext.AreaSetGraphRefs
            .Where(item => item.AreaSetStableId == areaSet.AreaSetStableId
                           && item.AreaSetRevision == areaSet.Revision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var expectedOrder = areaSet.LandscapeGraphs
            .Select((item, index) => (item.LandscapeGraphStableId, index))
            .ToDictionary(item => item.LandscapeGraphStableId, item => item.index,
                StringComparer.Ordinal);
        if (storedRefs.Length != expectedOrder.Count
            || storedRefs.Any(item => !expectedOrder.ContainsKey(item.LandscapeGraphStableId)))
            throw new InvalidOperationException("AreaSetStoredGraphRefMismatch");
        foreach (var storedRef in storedRefs)
            storedRef.ReferenceOrder = expectedOrder[storedRef.LandscapeGraphStableId];
    }

    private void AddGraphRefs(SimulationWorldLandscapeGraphResponse graph)
    {
        dbContext.LandscapeGraphSpatialRefs.AddRange(graph.AreaRefs.Select((item, index) =>
            new SimulationWorld경관Graph공간참조Entity
            {
                LandscapeGraphStableId = graph.LandscapeGraphStableId,
                GraphRevision = graph.GraphRevision,
                ReferenceKindCode = AreaRef,
                ReferenceStableId = item,
                ReferenceOrder = index,
            }));
        dbContext.LandscapeGraphSpatialRefs.AddRange(graph.ScenarioRouteRefs.Select((item, index) =>
            new SimulationWorld경관Graph공간참조Entity
            {
                LandscapeGraphStableId = graph.LandscapeGraphStableId,
                GraphRevision = graph.GraphRevision,
                ReferenceKindCode = ScenarioRouteRef,
                ReferenceStableId = item,
                ReferenceOrder = index,
            }));
        dbContext.LandscapeGraphTileRefs.AddRange(graph.TileRefs.Select((item, index) =>
            new SimulationWorld경관GraphTile참조Entity
            {
                LandscapeGraphStableId = graph.LandscapeGraphStableId,
                GraphRevision = graph.GraphRevision,
                TileKey = item,
                ReferenceOrder = index,
            }));
    }

    private async Task AddGraphRunAsync(
        SimulationWorldLandscapeGraphResponse graph,
        CancellationToken cancellationToken)
    {
        var run = new SimulationWorld경관조립실행Entity
        {
            GraphBuildStableId = graph.GraphBuildStableId,
            LandscapeGraphStableId = graph.LandscapeGraphStableId,
            BuildScopeCode = SimulationWorldLandscapeCompositionCodes.GraphBuildScope,
            GraphRoleCode = graph.GraphRoleCode,
            GraphRevision = graph.GraphRevision,
            DefinitionHashSha256 = graph.DefinitionHashSha256,
            TileKey = graph.TileRefs.FirstOrDefault() ?? string.Empty,
            AreaSetStableId = graph.AreaSetStableId,
            GrammarRevision = graph.GrammarRevision,
            GrammarHashSha256 = graph.GrammarHashSha256,
            GraphHashSha256 = graph.GraphHashSha256,
            StatusCode = graph.StatusCode,
            PresentationOnly = graph.PresentationOnly,
            StoredAtUtc = DateTimeOffset.UtcNow,
        };
        dbContext.LandscapeAssemblyRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        AddGraphChildren(run.Id, graph);
    }

    private void AddGraphChildren(long runId, SimulationWorldLandscapeGraphResponse graph)
    {
        dbContext.LandscapeNodes.AddRange(graph.Nodes.Select(item => new SimulationWorld경관공간NodeEntity
        {
            RunId = runId, NodeStableId = item.NodeStableId, ParentNodeStableId = item.ParentNodeStableId,
            NodeKindCode = item.NodeKindCode, SemanticCode = item.SemanticCode,
            EvidenceKindCode = item.EvidenceKindCode, CenterEastingMeters = item.CenterEastingMeters,
            CenterNorthingMeters = item.CenterNorthingMeters, WidthMeters = item.WidthMeters,
            DepthMeters = item.DepthMeters,
        }));
        dbContext.LandscapeEdges.AddRange(graph.Edges.Select(item => new SimulationWorld경관공간EdgeEntity
        {
            RunId = runId, EdgeStableId = item.EdgeStableId, FromNodeStableId = item.FromNodeStableId,
            RelationCode = item.RelationCode, ToNodeStableId = item.ToNodeStableId,
            ConnectorTypeCode = item.ConnectorTypeCode, EvidenceKindCode = item.EvidenceKindCode,
        }));
        dbContext.LandscapeEdges.AddRange(graph.ExternalConnectorStubs.Select(item =>
            new SimulationWorld경관공간EdgeEntity
            {
                RunId = runId, EdgeStableId = item.StubStableId, IsExternalStub = true,
                NeighborTileKey = item.NeighborTileKey, PlacementStableId = item.PlacementStableId,
                ConnectorTypeCode = item.ConnectorTypeCode, RouteSignature = item.RouteSignature,
                DirectionCode = item.DirectionCode, EvidenceKindCode = item.EvidenceKindCode,
                WorldEastingMeters = item.WorldEastingMeters,
                WorldNorthingMeters = item.WorldNorthingMeters, WidthMeters = item.WidthMeters,
            }));
        dbContext.LandscapePlacements.AddRange(graph.Placements.Select(item =>
            new SimulationWorld경관모판배치Entity
            {
                RunId = runId, PlacementStableId = item.PlacementStableId, NodeStableId = item.NodeStableId,
                OwnerTileKey = item.OwnerTileKey, CompositionKey = item.CompositionKey,
                TopologyCode = item.TopologyCode, EvidenceKindCode = item.EvidenceKindCode,
                EastingMeters = item.EastingMeters, NorthingMeters = item.NorthingMeters,
                PhysicalElevationMeters = item.PhysicalElevationMeters, RotationDegrees = item.RotationDegrees,
                Mirrored = item.Mirrored, DeterministicSeed = item.DeterministicSeed,
                FootprintWidthMeters = item.FootprintWidthMeters,
                FootprintDepthMeters = item.FootprintDepthMeters, PresentationOnly = item.PresentationOnly,
            }));
        dbContext.LandscapeUnresolved.AddRange(graph.Unresolved.Select(item =>
            new SimulationWorld경관조립미해결Entity
            {
                RunId = runId, UnresolvedStableId = item.UnresolvedStableId,
                NodeStableId = item.NodeStableId, ReasonCode = item.ReasonCode,
                RequiredSemanticCode = item.RequiredSemanticCode,
                EvidenceKindCode = item.EvidenceKindCode, Detail = item.Detail,
            }));
    }

    private async Task<SimulationWorldLandscapeGraphResponse> MapGraphAsync(
        SimulationWorld경관조립실행Entity run,
        CancellationToken cancellationToken)
    {
        var definition = await dbContext.LandscapeGraphDefinitions.AsNoTracking()
            .SingleAsync(item => item.LandscapeGraphStableId == run.LandscapeGraphStableId
                                 && item.GraphRevision == run.GraphRevision, cancellationToken)
            .ConfigureAwait(false);
        var refs = await dbContext.LandscapeGraphSpatialRefs.AsNoTracking()
            .Where(item => item.LandscapeGraphStableId == run.LandscapeGraphStableId
                           && item.GraphRevision == run.GraphRevision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var tiles = await dbContext.LandscapeGraphTileRefs.AsNoTracking()
            .Where(item => item.LandscapeGraphStableId == run.LandscapeGraphStableId
                           && item.GraphRevision == run.GraphRevision)
            .OrderBy(item => item.ReferenceOrder).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var nodes = await dbContext.LandscapeNodes.AsNoTracking().Where(item => item.RunId == run.Id)
            .OrderBy(item => item.NodeStableId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var edges = await dbContext.LandscapeEdges.AsNoTracking().Where(item => item.RunId == run.Id)
            .OrderBy(item => item.EdgeStableId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var placements = await dbContext.LandscapePlacements.AsNoTracking().Where(item => item.RunId == run.Id)
            .OrderBy(item => item.PlacementStableId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var unresolved = await dbContext.LandscapeUnresolved.AsNoTracking().Where(item => item.RunId == run.Id)
            .OrderBy(item => item.UnresolvedStableId).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        return new SimulationWorldLandscapeGraphResponse
        {
            AreaSetStableId = run.AreaSetStableId,
            LandscapeGraphStableId = run.LandscapeGraphStableId,
            GraphBuildStableId = run.GraphBuildStableId,
            GraphRoleCode = run.GraphRoleCode,
            GraphRevision = run.GraphRevision,
            DefinitionHashSha256 = run.DefinitionHashSha256,
            GraphHashSha256 = run.GraphHashSha256,
            GrammarRevision = run.GrammarRevision,
            GrammarHashSha256 = run.GrammarHashSha256,
            StatusCode = run.StatusCode,
            Bounds = Bounds(definition),
            AreaRefs = OrderedGraphRefs(refs, definition, AreaRef),
            ScenarioRouteRefs = OrderedGraphRefs(refs, definition, ScenarioRouteRef),
            TileRefs = tiles.Select(item => item.TileKey).ToArray(),
            Nodes = nodes.Select(MapNode).ToArray(),
            Edges = edges.Where(item => !item.IsExternalStub).Select(MapEdge).ToArray(),
            Placements = placements.Select(MapPlacement).ToArray(),
            ExternalConnectorStubs = edges.Where(item => item.IsExternalStub).Select(MapStub).ToArray(),
            Unresolved = unresolved.Select(MapUnresolved).ToArray(),
            PresentationOnly = run.PresentationOnly,
            IsOperationalState = false,
        };
    }

    private static bool HasBounds(SimulationWorldLandscapeBoundsResponse value) =>
        value.MaxEastingMeters > value.MinEastingMeters
        && value.MaxNorthingMeters > value.MinNorthingMeters;
    private static SimulationWorldLandscapeBoundsResponse Bounds(SimulationWorld경관Graph정의Entity value) =>
        value.HasBounds ? new()
        {
            MinEastingMeters = value.MinEastingMeters, MinNorthingMeters = value.MinNorthingMeters,
            MaxEastingMeters = value.MaxEastingMeters, MaxNorthingMeters = value.MaxNorthingMeters,
        } : new();
    private static string[] SpatialRefs(IEnumerable<SimulationWorldAreaSet공간참조Entity> refs, string kind) =>
        refs.Where(item => item.ReferenceKindCode == kind).OrderBy(item => item.ReferenceOrder)
            .Select(item => item.ReferenceStableId).ToArray();
    private static string[] OrderedGraphRefs(
        IEnumerable<SimulationWorld경관Graph공간참조Entity> refs,
        SimulationWorld경관Graph정의Entity graph,
        string kind) => refs.Where(item => item.LandscapeGraphStableId == graph.LandscapeGraphStableId
                                           && item.GraphRevision == graph.GraphRevision
                                           && item.ReferenceKindCode == kind)
        .OrderBy(item => item.ReferenceOrder).Select(item => item.ReferenceStableId).ToArray();
    private static SimulationWorldLandscapeNodeResponse MapNode(SimulationWorld경관공간NodeEntity item) => new()
    {
        NodeStableId = item.NodeStableId, ParentNodeStableId = item.ParentNodeStableId,
        NodeKindCode = item.NodeKindCode, SemanticCode = item.SemanticCode,
        EvidenceKindCode = item.EvidenceKindCode, CenterEastingMeters = item.CenterEastingMeters,
        CenterNorthingMeters = item.CenterNorthingMeters, WidthMeters = item.WidthMeters,
        DepthMeters = item.DepthMeters,
    };
    private static SimulationWorldLandscapeEdgeResponse MapEdge(SimulationWorld경관공간EdgeEntity item) => new()
    {
        EdgeStableId = item.EdgeStableId, FromNodeStableId = item.FromNodeStableId,
        RelationCode = item.RelationCode, ToNodeStableId = item.ToNodeStableId,
        ConnectorTypeCode = item.ConnectorTypeCode, EvidenceKindCode = item.EvidenceKindCode,
    };
    private static SimulationWorldLandscapePlacementResponse MapPlacement(SimulationWorld경관모판배치Entity item) => new()
    {
        PlacementStableId = item.PlacementStableId, NodeStableId = item.NodeStableId,
        OwnerTileKey = item.OwnerTileKey, CompositionKey = item.CompositionKey,
        TopologyCode = item.TopologyCode, EvidenceKindCode = item.EvidenceKindCode,
        EastingMeters = item.EastingMeters, NorthingMeters = item.NorthingMeters,
        PhysicalElevationMeters = item.PhysicalElevationMeters, RotationDegrees = item.RotationDegrees,
        Mirrored = item.Mirrored, DeterministicSeed = item.DeterministicSeed,
        FootprintWidthMeters = item.FootprintWidthMeters,
        FootprintDepthMeters = item.FootprintDepthMeters, PresentationOnly = item.PresentationOnly,
    };
    private static SimulationWorldLandscapeExternalConnectorResponse MapStub(SimulationWorld경관공간EdgeEntity item) => new()
    {
        StubStableId = item.EdgeStableId, PlacementStableId = item.PlacementStableId,
        NeighborTileKey = item.NeighborTileKey, ConnectorTypeCode = item.ConnectorTypeCode,
        RouteSignature = item.RouteSignature, DirectionCode = item.DirectionCode,
        EvidenceKindCode = item.EvidenceKindCode, WorldEastingMeters = item.WorldEastingMeters,
        WorldNorthingMeters = item.WorldNorthingMeters, WidthMeters = item.WidthMeters,
    };
    private static SimulationWorldLandscapeUnresolvedResponse MapUnresolved(SimulationWorld경관조립미해결Entity item) => new()
    {
        UnresolvedStableId = item.UnresolvedStableId, NodeStableId = item.NodeStableId,
        ReasonCode = item.ReasonCode, RequiredSemanticCode = item.RequiredSemanticCode,
        EvidenceKindCode = item.EvidenceKindCode, Detail = item.Detail,
    };
}

internal abstract class SimulationWorldAreaSetGraphConfiguration
{
    protected static void Id<T>(PropertyBuilder<T> property, string name) => property.HasColumnName(name);
    protected static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length).IsRequired();
}

internal sealed class SimulationWorldAreaSet정의Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorldAreaSet정의Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldAreaSet정의Entity> b)
    {
        b.ToTable("시뮬레이션월드_AreaSet정의"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AreaSetStableId, x.Revision }).IsUnique();
        Id(b.Property(x => x.Id), "식별번호"); Text(b.Property(x => x.AreaSetStableId), "AreaSet고유식별자", 200);
        Id(b.Property(x => x.Revision), "AreaSet개정번호"); Text(b.Property(x => x.Title), "제목", 200);
        Text(b.Property(x => x.Summary), "요약", 1000); Text(b.Property(x => x.DefinitionHashSha256), "실행정의SHA256", 64);
        Text(b.Property(x => x.DocumentHashSha256), "작성문서SHA256", 64); Text(b.Property(x => x.DefinitionStatusCode), "정의상태코드", 64);
        Id(b.Property(x => x.PresentationOnly), "표현전용여부"); Id(b.Property(x => x.StoredAtUtc), "저장시각UTC");
    }
}

internal sealed class SimulationWorldAreaSet공간참조Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorldAreaSet공간참조Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldAreaSet공간참조Entity> b)
    {
        b.ToTable("시뮬레이션월드_AreaSet공간참조"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AreaSetStableId, x.AreaSetRevision, x.ReferenceKindCode, x.ReferenceStableId }).IsUnique();
        Id(b.Property(x => x.Id), "식별번호"); Text(b.Property(x => x.AreaSetStableId), "AreaSet고유식별자", 200);
        Id(b.Property(x => x.AreaSetRevision), "AreaSet개정번호"); Text(b.Property(x => x.ReferenceKindCode), "참조종류코드", 40);
        Text(b.Property(x => x.ReferenceStableId), "공간참조고유식별자", 240); Id(b.Property(x => x.ReferenceOrder), "참조순서");
    }
}

internal sealed class SimulationWorldAreaSetGraph참조Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorldAreaSetGraph참조Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorldAreaSetGraph참조Entity> b)
    {
        b.ToTable("시뮬레이션월드_AreaSetGraph참조"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AreaSetStableId, x.AreaSetRevision, x.LandscapeGraphStableId }).IsUnique();
        Id(b.Property(x => x.Id), "식별번호"); Text(b.Property(x => x.AreaSetStableId), "AreaSet고유식별자", 200);
        Id(b.Property(x => x.AreaSetRevision), "AreaSet개정번호"); Text(b.Property(x => x.LandscapeGraphStableId), "경관Graph고유식별자", 240);
        Id(b.Property(x => x.ReferenceOrder), "참조순서");
    }
}

internal sealed class SimulationWorld경관Graph정의Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorld경관Graph정의Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관Graph정의Entity> b)
    {
        b.ToTable("시뮬레이션월드_경관Graph정의"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.LandscapeGraphStableId, x.GraphRevision }).IsUnique();
        Text(b.Property(x => x.AreaSetStableId), "AreaSet고유식별자", 200); Id(b.Property(x => x.Id), "식별번호");
        Text(b.Property(x => x.LandscapeGraphStableId), "경관Graph고유식별자", 240); Text(b.Property(x => x.GraphRoleCode), "경관Graph역할코드", 80);
        Id(b.Property(x => x.GraphRevision), "경관Graph개정번호"); Text(b.Property(x => x.DefinitionHashSha256), "경관Graph정의SHA256", 64);
        Text(b.Property(x => x.BuildStatusCode), "생성상태코드", 64); Text(b.Property(x => x.GraphHashSha256), "경관GraphSHA256", 64);
        Id(b.Property(x => x.HasBounds), "경계범위보유여부"); Id(b.Property(x => x.MinEastingMeters), "최소동쪽좌표미터");
        Id(b.Property(x => x.MinNorthingMeters), "최소북쪽좌표미터"); Id(b.Property(x => x.MaxEastingMeters), "최대동쪽좌표미터");
        Id(b.Property(x => x.MaxNorthingMeters), "최대북쪽좌표미터"); Id(b.Property(x => x.StoredAtUtc), "저장시각UTC");
    }
}

internal sealed class SimulationWorld경관Graph공간참조Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorld경관Graph공간참조Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관Graph공간참조Entity> b)
    {
        b.ToTable("시뮬레이션월드_경관Graph공간참조"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.LandscapeGraphStableId, x.GraphRevision, x.ReferenceKindCode, x.ReferenceStableId }).IsUnique();
        Id(b.Property(x => x.Id), "식별번호"); Text(b.Property(x => x.LandscapeGraphStableId), "경관Graph고유식별자", 240);
        Id(b.Property(x => x.GraphRevision), "경관Graph개정번호"); Text(b.Property(x => x.ReferenceKindCode), "참조종류코드", 40);
        Text(b.Property(x => x.ReferenceStableId), "공간참조고유식별자", 240); Id(b.Property(x => x.ReferenceOrder), "참조순서");
    }
}

internal sealed class SimulationWorld경관GraphTile참조Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorld경관GraphTile참조Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관GraphTile참조Entity> b)
    {
        b.ToTable("시뮬레이션월드_경관GraphTile참조"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.LandscapeGraphStableId, x.GraphRevision, x.TileKey }).IsUnique(); b.HasIndex(x => x.TileKey);
        Id(b.Property(x => x.Id), "식별번호"); Text(b.Property(x => x.LandscapeGraphStableId), "경관Graph고유식별자", 240);
        Id(b.Property(x => x.GraphRevision), "경관Graph개정번호"); Text(b.Property(x => x.TileKey), "타일키", 120);
        Id(b.Property(x => x.ReferenceOrder), "참조순서");
    }
}

internal sealed class SimulationWorld경관Graph관계Configuration : SimulationWorldAreaSetGraphConfiguration,
    IEntityTypeConfiguration<SimulationWorld경관Graph관계Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관Graph관계Entity> b)
    {
        b.ToTable("시뮬레이션월드_경관Graph관계"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AreaSetStableId, x.AreaSetRevision, x.RelationStableId }).IsUnique();
        Id(b.Property(x => x.Id), "식별번호"); Text(b.Property(x => x.AreaSetStableId), "AreaSet고유식별자", 200);
        Id(b.Property(x => x.AreaSetRevision), "AreaSet개정번호"); Text(b.Property(x => x.RelationStableId), "Graph관계고유식별자", 240);
        Text(b.Property(x => x.FromGraphStableId), "출발경관Graph고유식별자", 240); Text(b.Property(x => x.ToGraphStableId), "도착경관Graph고유식별자", 240);
        Text(b.Property(x => x.RelationCode), "관계코드", 40); Text(b.Property(x => x.FromConnectorStableId), "출발연결지점고유식별자", 300);
        Text(b.Property(x => x.ToConnectorStableId), "도착연결지점고유식별자", 300); Text(b.Property(x => x.ConnectorTypeCode), "연결지점종류코드", 100);
        Text(b.Property(x => x.RouteSignature), "경로서명", 160);
    }
}
