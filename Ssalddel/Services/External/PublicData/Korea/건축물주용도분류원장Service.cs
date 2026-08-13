using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record 건축물분류원장Result(
    int BuildingCount,
    int InsertedAssignmentCount,
    int ExistingAssignmentCount,
    int AggregateCount);

public sealed class 건축물주용도분류원장Service
{
    private readonly PublicDataIngestionDbContext _db;

    public 건축물주용도분류원장Service(PublicDataIngestionDbContext db)
    {
        _db = db;
    }

    public async Task<건축물분류원장Result> ClassifyAndAggregateAsync(
        string sourceRevision,
        string sourceVintage,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceVintage);

        var buildings = await _db.BuildingRegisterTitles
            .Where(item => item.SourceRevision == sourceRevision && item.ValidToUtc == null)
            .OrderBy(item => item.RegisterManagementPk)
            .ToListAsync(cancellationToken);
        var buildingIds = buildings.Select(item => item.Id).ToList();
        var existingBuildingIds = await _db.BuildingCategoryAssignments
            .Where(item => buildingIds.Contains(item.BuildingRecordId)
                && item.RuleRevision == 건축물주용도분류Engine.RuleRevision
                && item.IsPrimary)
            .Select(item => item.BuildingRecordId)
            .ToListAsync(cancellationToken);
        var existingSet = existingBuildingIds.ToHashSet();

        var inserted = 0;
        foreach (var building in buildings.Where(item => !existingSet.Contains(item.Id)))
        {
            var categoryCode = 건축물주용도분류Engine.Classify(building.MainPurposeName);
            _db.BuildingCategoryAssignments.Add(new 건축물용도CategoryAssignment
            {
                Id = 공공데이터원장식별자.결정적Guid(
                    $"category|{building.Id:N}|{건축물주용도분류Engine.RuleRevision}"),
                BuildingRecordId = building.Id,
                CategoryCode = categoryCode,
                IsPrimary = true,
                AssignmentMethodCode = "OfficialMainPurposeNameRule",
                EvidenceKindCode = categoryCode == 건축물용도CategoryCodes.Unresolved
                    ? 건축물분류EvidenceKindCodes.Unresolved
                    : 건축물분류EvidenceKindCodes.Derived,
                RuleRevision = 건축물주용도분류Engine.RuleRevision,
                SourceMainPurposeCode = building.MainPurposeCode,
                SourceMainPurposeName = building.MainPurposeName,
                ClassifiedAtUtc = DateTimeOffset.UtcNow,
            });
            inserted++;
        }

        if (inserted > 0)
            await _db.SaveChangesAsync(cancellationToken);

        var aggregateRows = await (
                from region in _db.BuildingRegionAssignments
                join building in _db.BuildingRegisterTitles
                    on region.BuildingRecordId equals building.Id
                join category in _db.BuildingCategoryAssignments
                    on building.Id equals category.BuildingRecordId
                where region.AdministrativeRegionStableId != null
                    && region.SourceVintage == sourceVintage
                    && region.ValidToUtc == null
                    && building.SourceRevision == sourceRevision
                    && building.ValidToUtc == null
                    && category.RuleRevision == 건축물주용도분류Engine.RuleRevision
                    && category.IsPrimary
                select new
                {
                    region.AdministrativeRegionStableId,
                    category.CategoryCode,
                    building.BuildingAreaSquareMeters,
                    building.TotalFloorAreaSquareMeters,
                    building.BuildingName,
                })
            .ToListAsync(cancellationToken);

        var currentAggregates = await _db.AdministrativeBuildingCategoryAggregates
            .Where(item => item.SourceVintage == sourceVintage
                && item.RuleRevision == 건축물주용도분류Engine.RuleRevision)
            .ToDictionaryAsync(
                item => $"{item.AdministrativeRegionStableId}|{item.CategoryCode}",
                cancellationToken);

        var generatedKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in aggregateRows.GroupBy(item => new
                 {
                     item.AdministrativeRegionStableId,
                     item.CategoryCode,
                 }))
        {
            var regionStableId = group.Key.AdministrativeRegionStableId!;
            var key = $"{regionStableId}|{group.Key.CategoryCode}";
            generatedKeys.Add(key);
            if (!currentAggregates.TryGetValue(key, out var aggregate))
            {
                aggregate = new 행정동건축물CategoryAggregate
                {
                    Id = 공공데이터원장식별자.결정적Guid(
                        $"aggregate|{regionStableId}|{sourceVintage}|{group.Key.CategoryCode}|{건축물주용도분류Engine.RuleRevision}"),
                    AdministrativeRegionStableId = regionStableId,
                    SourceVintage = sourceVintage,
                    CategoryCode = group.Key.CategoryCode,
                    RuleRevision = 건축물주용도분류Engine.RuleRevision,
                };
                _db.AdministrativeBuildingCategoryAggregates.Add(aggregate);
            }

            aggregate.BuildingCount = group.LongCount();
            aggregate.BuildingAreaSquareMeters = group.Sum(item => item.BuildingAreaSquareMeters ?? 0m);
            aggregate.TotalFloorAreaSquareMeters = group.Sum(item => item.TotalFloorAreaSquareMeters ?? 0m);
            aggregate.NamedBuildingCount = group.LongCount(item => !string.IsNullOrWhiteSpace(item.BuildingName));
            aggregate.GeometryLinkedCount = 0;
            aggregate.UnresolvedBuildingCount = group.Key.CategoryCode == 건축물용도CategoryCodes.Unresolved
                ? aggregate.BuildingCount
                : 0;
            aggregate.EvidenceKindCode = 건축물분류EvidenceKindCodes.Derived;
            aggregate.AggregateHashSha256 = 공공데이터원장식별자.Sha256(
                $"{regionStableId}|{sourceVintage}|{group.Key.CategoryCode}|{aggregate.BuildingCount}|" +
                $"{aggregate.BuildingAreaSquareMeters:F4}|{aggregate.TotalFloorAreaSquareMeters:F4}|" +
                $"{aggregate.NamedBuildingCount}|{aggregate.GeometryLinkedCount}|{aggregate.UnresolvedBuildingCount}|" +
                건축물주용도분류Engine.RuleRevision);
            aggregate.GeneratedAtUtc = DateTimeOffset.UtcNow;
        }

        var staleAggregates = currentAggregates
            .Where(item => !generatedKeys.Contains(item.Key))
            .Select(item => item.Value)
            .ToArray();
        _db.AdministrativeBuildingCategoryAggregates.RemoveRange(staleAggregates);
        await _db.SaveChangesAsync(cancellationToken);

        return new 건축물분류원장Result(
            buildings.Count,
            inserted,
            existingSet.Count,
            generatedKeys.Count);
    }

}
