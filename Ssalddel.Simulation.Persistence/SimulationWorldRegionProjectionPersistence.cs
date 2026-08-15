using Microsoft.EntityFrameworkCore;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld지역ProjectionReader(
    SimulationWorld파생DbContext dbContext) : ISimulationWorld지역ProjectionReader
{
    private const string AggregateNodeKind = "AdministrativeRegionBuildingCategoryAggregate";
    private const string AggregateRelation = "HasBuildingCategoryAggregate";
    private const string RegionCrosswalkRelation = "LegalAdministrativeRegionCrosswalk";
    private const string TileRelation = "IntersectsSpatialTile";

    public async Task<SimulationWorld지역Projection조회Result> 조회Async(
        string regionStableId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionStableId);
        var region = await dbContext.Nodes.AsNoTracking()
            .Where(item => item.SourceRecordStableId == regionStableId
                && (item.NodeKindCode == SimulationWorldRegionProjectionCodes.LegalRegion
                    || item.NodeKindCode == SimulationWorldRegionProjectionCodes.AdministrativeRegion))
            .OrderByDescending(item => item.Run.StoredAtUtc)
            .Select(item => new
            {
                Node = item,
                item.Run.BuildStableId,
                item.Run.OutputHashSha256,
                item.Run.RecipeRevision,
                item.Run.RuleRevision,
                item.Run.GeneratedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (region is null)
            return new SimulationWorld지역Projection조회Result(true, null);

        var runId = region.Node.RunId;
        var relations = await dbContext.Relations.AsNoTracking()
            .Where(item => item.RunId == runId
                && (item.FromNodeStableId == region.Node.StableId
                    || item.ToNodeStableId == region.Node.StableId))
            .ToListAsync(cancellationToken);
        var relatedNodeIds = relations
            .Select(item => item.FromNodeStableId == region.Node.StableId
                ? item.ToNodeStableId
                : item.FromNodeStableId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var runNodes = await dbContext.Nodes.AsNoTracking()
            .Where(item => item.RunId == runId)
            .ToListAsync(cancellationToken);
        var relatedNodes = runNodes
            .Where(item => relatedNodeIds.Contains(item.StableId, StringComparer.Ordinal))
            .ToDictionary(item => item.StableId, StringComparer.Ordinal);

        var aggregateIds = relations
            .Where(item => item.RelationCode == AggregateRelation
                && item.FromNodeStableId == region.Node.StableId)
            .Select(item => item.ToNodeStableId)
            .ToHashSet(StringComparer.Ordinal);
        var categories = relatedNodes.Values
            .Where(item => aggregateIds.Contains(item.StableId)
                && item.NodeKindCode == AggregateNodeKind)
            .OrderBy(item => item.RepresentativeGroupCode, StringComparer.Ordinal)
            .Select(item => new SimulationWorldRegionBuildingCategorySummaryResponse
            {
                CategoryCode = item.RepresentativeGroupCode ?? "unresolved",
                BuildingCount = item.RepresentedRecordCount ?? 0,
                EvidenceKindCode = item.EvidenceKindCode,
                SourceAggregateRecordStableId = item.SourceRecordStableId ?? string.Empty,
            })
            .ToArray();
        var relatedRegions = relations
            .Where(item => item.RelationCode == RegionCrosswalkRelation)
            .Select(item => item.FromNodeStableId == region.Node.StableId
                ? item.ToNodeStableId
                : item.FromNodeStableId)
            .Where(relatedNodes.ContainsKey)
            .Select(item => relatedNodes[item].SourceRecordStableId)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var tileKeys = relations
            .Where(item => item.RelationCode == TileRelation)
            .Select(item => item.FromNodeStableId == region.Node.StableId
                ? item.ToNodeStableId
                : item.FromNodeStableId)
            .Where(relatedNodes.ContainsKey)
            .Select(item => relatedNodes[item].TileKey)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();

        return new SimulationWorld지역Projection조회Result(true,
            new SimulationWorldRegionProjectionResponse
            {
                RegionStableId = regionStableId,
                RegionKindCode = region.Node.NodeKindCode,
                DisplayName = region.Node.DisplayName ?? regionStableId,
                RegionCode = region.Node.RegionCode,
                AreaStableId = region.Node.AreaStableId,
                ProjectionStatusCode = tileKeys.Length > 0
                    ? SimulationWorldRegionProjectionCodes.Ready
                    : SimulationWorldRegionProjectionCodes.WaitingForRegionGeometry,
                BuildStableId = region.BuildStableId,
                BuildOutputHashSha256 = region.OutputHashSha256,
                RecipeRevision = region.RecipeRevision,
                RuleRevision = region.RuleRevision,
                GeneratedAtUtc = region.GeneratedAtUtc,
                RelatedRegionStableIds = relatedRegions,
                TileKeys = tileKeys,
                BuildingCategories = categories,
                PresentationOnly = true,
                IsOperationalState = false,
            });
    }
}
