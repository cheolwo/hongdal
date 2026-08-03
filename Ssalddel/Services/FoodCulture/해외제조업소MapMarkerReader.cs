using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Data;

namespace Ssalddel.Services.FoodCulture;

public interface I해외제조업소MapMarkerReader
{
    Task<IReadOnlyList<해외제조업소MapMarker>> 공개Marker조회Async(
        CancellationToken cancellationToken = default);
}

public sealed record 해외제조업소MapMarker(
    string StableRegionKey,
    string CountryCode,
    string CountryName,
    string RegionName,
    double Latitude,
    double Longitude,
    int OrganizationCount,
    int EvidenceCount,
    DateTimeOffset LastObservedAtUtc,
    string AnchorSourceName,
    string AnchorSourceUrl,
    string RegionBoundary);

public sealed class 해외제조업소MapMarkerReader(
    AgriculturalFisheriesDbContext archiveDb,
    SsalddelContext geographyDb) : I해외제조업소MapMarkerReader
{
    private const string MfdsSourceName = "식품의약품안전처 해외제조업소 정보";
    private const string MfdsSourceUrl =
        "https://www.data.go.kr/data/15073967/openapi.do";

    private static readonly IReadOnlyDictionary<string, ChinaRegionAnchor> ChinaAnchors =
        new Dictionary<string, ChinaRegionAnchor>(StringComparer.Ordinal)
        {
            [ChinaImportedFoodManufacturerRegionCodes.LiaoningLiaodong] = new(
                "cn-liaoning",
                40.25,
                122.15),
            [ChinaImportedFoodManufacturerRegionCodes.Shandong] = new(
                "cn-shandong",
                36.6683,
                117.0204),
            [ChinaImportedFoodManufacturerRegionCodes.LowerYangtzeJiangnan] = new(
                "cn-lower-yangtze-jiangnan",
                27.6,
                113.9)
        };

    public async Task<IReadOnlyList<해외제조업소MapMarker>> 공개Marker조회Async(
        CancellationToken cancellationToken = default)
    {
        var aggregates = await archiveDb.OfficialFoodIngredientCompanyEvidence
            .AsNoTracking()
            .Where(item => item.IsCurrent
                           && item.RelationCode
                           == OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer
                           && item.VerificationStatusCode
                           == OfficialFoodIngredientCompanyVerificationStatusCodes.OverseasFacilityMatched
                           && item.ManufacturerRegionConfidence >= 0.9m
                           && item.ManufacturerRegionCode != string.Empty
                           && (item.CountryCode == "US" || item.CountryCode == "CN"))
            .GroupBy(item => new
            {
                item.CountryCode,
                item.CountryName,
                item.ManufacturerRegionCode,
                item.ManufacturerRegionName,
                item.ManufacturerRegionScope
            })
            .Select(group => new RegionAggregate(
                group.Key.CountryCode,
                group.Key.CountryName,
                group.Key.ManufacturerRegionCode,
                group.Key.ManufacturerRegionName,
                group.Key.ManufacturerRegionScope,
                group.Select(item => item.OrganizationKey).Distinct().Count(),
                group.Count(),
                group.Max(item => item.LastObservedAtUtc)))
            .ToArrayAsync(cancellationToken);

        if (aggregates.Length == 0)
        {
            return [];
        }

        var result = new List<해외제조업소MapMarker>(aggregates.Length);
        result.AddRange(await BuildUnitedStatesMarkersAsync(
            aggregates.Where(item => item.CountryCode == "US").ToArray(),
            cancellationToken));
        result.AddRange(BuildChinaMarkers(
            aggregates.Where(item => item.CountryCode == "CN")));
        return result
            .OrderByDescending(item => item.OrganizationCount)
            .ThenBy(item => item.CountryCode, StringComparer.Ordinal)
            .ThenBy(item => item.RegionName, StringComparer.Ordinal)
            .ToArray();
    }

    private async Task<IReadOnlyList<해외제조업소MapMarker>> BuildUnitedStatesMarkersAsync(
        IReadOnlyList<RegionAggregate> aggregates,
        CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0)
        {
            return [];
        }

        var definitions = UnitedStatesImportedFoodManufacturerRegionClassifier.Definitions
            .ToDictionary(item => item.RegionCode, StringComparer.Ordinal);
        var postalCodes = aggregates
            .Select(item => definitions.GetValueOrDefault(item.RegionCode)?.PostalCode)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var assignments = await geographyDb.지역농수산Map행정구역CodeAssignments
            .AsNoTracking()
            .Where(item => item.SchemeCode
                           == RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState
                           && postalCodes.Contains(item.ExternalCode))
            .Include(item => item.Region)
            .ThenInclude(region => region.Boundaries)
            .ToArrayAsync(cancellationToken);
        var assignmentLookup = assignments
            .GroupBy(item => item.ExternalCode.Trim().ToUpperInvariant(), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var result = new List<해외제조업소MapMarker>();

        foreach (var aggregate in aggregates)
        {
            if (!definitions.TryGetValue(aggregate.RegionCode, out var definition)
                || !assignmentLookup.TryGetValue(definition.PostalCode, out var assignment))
            {
                continue;
            }

            var anchor = assignment.Region.Boundaries
                .Where(IsVerifiedAnchor)
                .OrderBy(item => item.SimplificationLevel)
                .ThenByDescending(item => item.VerifiedAtUtc)
                .FirstOrDefault();
            if (anchor is null)
            {
                continue;
            }

            result.Add(new 해외제조업소MapMarker(
                $"us-{Slug(definition.EnglishName)}",
                "US",
                string.IsNullOrWhiteSpace(aggregate.CountryName) ? "미국" : aggregate.CountryName,
                aggregate.RegionName,
                (double)anchor.AnchorLatitude,
                (double)anchor.AnchorLongitude,
                aggregate.OrganizationCount,
                aggregate.EvidenceCount,
                new DateTimeOffset(DateTime.SpecifyKind(
                    aggregate.LastObservedAtUtc,
                    DateTimeKind.Utc)),
                MfdsSourceName,
                MfdsSourceUrl,
                $"{aggregate.RegionScope} 마커는 {anchor.BoundarySourceCode}의 행정권역 대표점이며 개별 제조업소 주소가 아닙니다."));
        }

        return result;
    }

    private static IEnumerable<해외제조업소MapMarker> BuildChinaMarkers(
        IEnumerable<RegionAggregate> aggregates)
    {
        foreach (var aggregate in aggregates)
        {
            if (!ChinaAnchors.TryGetValue(aggregate.RegionCode, out var anchor))
            {
                continue;
            }

            yield return new 해외제조업소MapMarker(
                anchor.StableRegionKey,
                "CN",
                string.IsNullOrWhiteSpace(aggregate.CountryName) ? "중국" : aggregate.CountryName,
                aggregate.RegionName,
                anchor.Latitude,
                anchor.Longitude,
                aggregate.OrganizationCount,
                aggregate.EvidenceCount,
                new DateTimeOffset(DateTime.SpecifyKind(
                    aggregate.LastObservedAtUtc,
                    DateTimeKind.Utc)),
                MfdsSourceName,
                MfdsSourceUrl,
                $"{aggregate.RegionScope} 마커는 기존 지역문화 카탈로그의 권역 대표점이며 개별 제조업소 주소가 아닙니다.");
        }
    }

    private static bool IsVerifiedAnchor(
        Ssalddel.Domain.Geography.지역농수산Map행정구역Boundary boundary)
        => boundary.VerifiedAtUtc != default
           && boundary.AnchorLatitude is >= -90m and <= 90m
           && boundary.AnchorLongitude is >= -180m and <= 180m
           && !string.IsNullOrWhiteSpace(boundary.SourceUrl);

    private static string Slug(string value)
        => value.Trim().ToLowerInvariant().Replace(' ', '-');

    private sealed record RegionAggregate(
        string CountryCode,
        string CountryName,
        string RegionCode,
        string RegionName,
        string RegionScope,
        int OrganizationCount,
        int EvidenceCount,
        DateTime LastObservedAtUtc);

    private sealed record ChinaRegionAnchor(
        string StableRegionKey,
        double Latitude,
        double Longitude);
}
