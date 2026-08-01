using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.Geography;
using 살뜰.Data;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "원천 지역 관측을 검토된 행정구역·경계 기준점에 연결",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "관측 전체 기간에 유효한 코드 근거와 검증된 경계 기준점이 없으면 마커를 만들지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Application,
    "원천 코드를 검토된 지역 교차표와 대표 기준점에 해석",
    FlowOrder = 25,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "지역 원장만 읽으며 원천 코드를 이름·인접 지역으로 임의 추정하지 않습니다.")]
public sealed class 지역농수산Map지역Resolver(SsalddelContext geographyDb)
{
    internal async Task<지역농수산MapProjectionResult> 투영Async(
        IReadOnlyList<지역농수산MapSourceAggregate> sourceAggregates,
        CancellationToken cancellationToken)
    {
        if (sourceAggregates.Count == 0)
        {
            return 지역농수산MapProjectionResult.Empty;
        }

        var schemes = sourceAggregates
            .Select(item => item.CodeScheme)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var codeAssignments = await geographyDb.지역농수산Map행정구역CodeAssignments
            .AsNoTracking()
            .Where(item => schemes.Contains(item.SchemeCode))
            .Include(item => item.Region)
            .ThenInclude(item => item.Boundaries)
            .ToArrayAsync(cancellationToken);
        var crosswalks = await geographyDb.지역농수산Map지역Crosswalks
            .AsNoTracking()
            .Where(item => schemes.Contains(item.SourceSchemeCode))
            .Include(item => item.TargetRegion)
            .ThenInclude(item => item!.Boundaries)
            .ToArrayAsync(cancellationToken);
        var assignmentLookup = codeAssignments.ToLookup(item => new RegionCodeKey(
            item.SchemeCode,
            지역농수산MapCodeNormalizer.Normalize(item.ExternalCode)));
        var crosswalkLookup = crosswalks.ToLookup(item => new RegionCodeKey(
            item.SourceSchemeCode,
            지역농수산MapCodeNormalizer.Normalize(item.SourceCode)));

        var mapped = new List<MappedAggregate>();
        var unresolvedObservationCount = 0;
        var missingAnchorRegionIds = new HashSet<Guid>();
        foreach (var aggregate in sourceAggregates)
        {
            var lookupKey = new RegionCodeKey(aggregate.CodeScheme, aggregate.ExternalCode);
            var match = FindMatch(
                aggregate,
                assignmentLookup[lookupKey],
                crosswalkLookup[lookupKey]);
            if (match is null)
            {
                unresolvedObservationCount += aggregate.ObservationCount;
                continue;
            }

            var boundary = match.Region.Boundaries
                .Where(IsVerifiedBoundary)
                .OrderBy(item => item.SimplificationLevel)
                .ThenByDescending(item => item.VerifiedAtUtc)
                .FirstOrDefault();
            if (boundary is null)
            {
                missingAnchorRegionIds.Add(match.Region.Id);
                continue;
            }

            mapped.Add(new MappedAggregate(aggregate, match, boundary));
        }

        var markers = mapped
            .GroupBy(item => new
            {
                item.Match.Region.Id,
                item.Aggregate.RelationTypeCode
            })
            .Select(group => BuildMarker(group))
            .OrderByDescending(item => item.ObservationCount)
            .ThenBy(item => item.DisplayNameKo, StringComparer.Ordinal)
            .ThenBy(item => item.RelationTypeCode, StringComparer.Ordinal)
            .ToArray();

        return new 지역농수산MapProjectionResult(
            markers,
            unresolvedObservationCount,
            missingAnchorRegionIds.Count);
    }

    private static RegionMatch? FindMatch(
        지역농수산MapSourceAggregate aggregate,
        IEnumerable<지역농수산Map행정구역CodeAssignment> codeAssignments,
        IEnumerable<지역농수산Map지역Crosswalk> crosswalks)
    {
        if (aggregate.ExternalCode.Length == 0)
        {
            return null;
        }

        if (aggregate.CodeScheme == RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState)
        {
            var assignment = codeAssignments
                .Where(item =>
                    AppliesThroughout(
                        item.ValidFrom,
                        item.ValidTo,
                        aggregate.EarliestObservedDate,
                        aggregate.LatestObservedDate)
                    && AppliesThroughout(
                        item.Region.ValidFrom,
                        item.Region.ValidTo,
                        aggregate.EarliestObservedDate,
                        aggregate.LatestObservedDate)
                    && item.VerifiedAtUtc != default)
                .OrderByDescending(item => item.VerifiedAtUtc)
                .FirstOrDefault();
            return assignment is null
                ? null
                : new RegionMatch(
                    assignment.Region,
                    RegionalAgriculturalMapConfidenceCodes.OfficialCodeCrosswalk,
                    assignment.VerifiedAtUtc);
        }

        var crosswalk = crosswalks
            .Where(item =>
                item.TargetRegion is not null
                && item.ReviewedAtUtc.HasValue
                && item.ConfidenceCode.Length > 0
                && AppliesThroughout(
                    item.ValidFrom,
                    item.ValidTo,
                    aggregate.EarliestObservedDate,
                    aggregate.LatestObservedDate)
                && AppliesThroughout(
                    item.TargetRegion.ValidFrom,
                    item.TargetRegion.ValidTo,
                    aggregate.EarliestObservedDate,
                    aggregate.LatestObservedDate))
            .OrderByDescending(item => item.ReviewedAtUtc)
            .FirstOrDefault();
        return crosswalk?.TargetRegion is null
            ? null
            : new RegionMatch(
                crosswalk.TargetRegion,
                crosswalk.ConfidenceCode,
                crosswalk.ReviewedAtUtc!.Value);
    }

    private static RegionalAgriculturalMapMarkerDto BuildMarker<TKey>(
        IGrouping<TKey, MappedAggregate> group)
    {
        var first = group.First();
        var region = first.Match.Region;
        var boundary = first.Boundary;
        var sources = group
            .Select(item => new RegionalAgriculturalMapMarkerSourceDto(
                item.Aggregate.DataSourceKey,
                item.Aggregate.CodeScheme,
                item.Aggregate.ExternalCode,
                item.Aggregate.ExternalName,
                item.Match.ConfidenceCode,
                item.Match.VerifiedAtUtc,
                item.Aggregate.ObservationCount,
                item.Aggregate.EarliestObservedDate,
                item.Aggregate.LatestObservedDate))
            .OrderByDescending(item => item.ObservationCount)
            .ThenBy(item => item.ExternalCode, StringComparer.Ordinal)
            .ToArray();

        return new RegionalAgriculturalMapMarkerDto(
            $"{first.Aggregate.RelationTypeCode}:{region.PublicRegionKey}",
            region.PublicRegionKey,
            region.CountryCode,
            region.RegionTypeCode,
            region.DisplayNameKo,
            region.DisplayNameEn,
            region.DisplayNameLocal,
            boundary.AnchorLatitude,
            boundary.AnchorLongitude,
            boundary.BoundarySourceCode,
            boundary.BoundaryVintage,
            boundary.SourceUrl,
            boundary.VerifiedAtUtc,
            first.Aggregate.RelationTypeCode,
            sources.Sum(item => item.ObservationCount),
            sources.Min(item => item.EarliestObservedDate),
            sources.Max(item => item.LatestObservedDate),
            sources);
    }

    private static bool IsVerifiedBoundary(지역농수산Map행정구역Boundary boundary)
        => boundary.VerifiedAtUtc != default
           && boundary.AnchorLatitude is >= -90m and <= 90m
           && boundary.AnchorLongitude is >= -180m and <= 180m
           && boundary.SourceUrl.Length > 0;

    private static bool AppliesThroughout(
        DateOnly? validFrom,
        DateOnly? validTo,
        DateOnly earliestDate,
        DateOnly latestDate)
        => (!validFrom.HasValue || validFrom.Value <= earliestDate)
           && (!validTo.HasValue || validTo.Value >= latestDate);

    private readonly record struct RegionCodeKey(string SchemeCode, string ExternalCode);

    private sealed record RegionMatch(
        지역농수산Map행정구역 Region,
        string ConfidenceCode,
        DateTime VerifiedAtUtc);

    private sealed record MappedAggregate(
        지역농수산MapSourceAggregate Aggregate,
        RegionMatch Match,
        지역농수산Map행정구역Boundary Boundary);
}
