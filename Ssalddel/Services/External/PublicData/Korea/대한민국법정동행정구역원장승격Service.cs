using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.Geography;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Data;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record 대한민국법정동행정구역원장승격Result(
    int 현행정규화Record수,
    int 행정구역추가수,
    int 행정구역갱신수,
    int CodeAssignment추가수,
    int 상위구역미확인수,
    string DataRevision);

/// <summary>
/// 공공데이터 정규화 원장에서 현행 법정동만 지도용 행정구역 원장으로 승격합니다.
/// 폐지 시점은 코드 전체자료에 없으므로 폐지 행을 임의의 유효종료일로 바꾸지 않습니다.
/// </summary>
public sealed class 대한민국법정동행정구역원장승격Service
{
    private const string StableIdPrefix = "region:kr:bjd:";
    private const string OfficialSourceUrl = "https://www.code.go.kr/stdcode/regCodeL.do";
    private readonly PublicDataIngestionDbContext ingestionDb;
    private readonly SsalddelContext geographyDb;
    private readonly TimeProvider timeProvider;

    public 대한민국법정동행정구역원장승격Service(
        PublicDataIngestionDbContext ingestionDb,
        SsalddelContext geographyDb,
        TimeProvider timeProvider)
    {
        this.ingestionDb = ingestionDb ?? throw new ArgumentNullException(nameof(ingestionDb));
        this.geographyDb = geographyDb ?? throw new ArgumentNullException(nameof(geographyDb));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<대한민국법정동행정구역원장승격Result> 승격Async(
        CancellationToken cancellationToken = default)
    {
        var records = await ingestionDb.NormalizedRecords
            .AsNoTracking()
            .Where(item => item.SourceId == 대한민국법정동CodeDataset.SourceId
                           && item.DatasetId == 대한민국법정동CodeDataset.DatasetId
                           && item.MetricCode == 대한민국법정동CodeDataset.MetricCode
                           && item.DimensionKey.Contains("status=active"))
            .OrderBy(item => item.SpatialPrecisionCode)
            .ThenBy(item => item.StableId)
            .ToArrayAsync(cancellationToken);
        if (records.Length == 0)
            throw new InvalidOperationException("KoreaLegalDongActiveRecordsMissing");

        var revisions = records.Select(item => item.DataRevision).Distinct(StringComparer.Ordinal).ToArray();
        if (revisions.Length != 1 || string.IsNullOrWhiteSpace(revisions[0]))
            throw new InvalidOperationException("KoreaLegalDongRevisionAmbiguous");

        var rows = records.Select(Parse).OrderBy(item => item.계층순서).ThenBy(item => item.Code).ToArray();
        var regions = await geographyDb.지역농수산Map행정구역들
            .Where(item => item.PublicRegionKey.StartsWith(StableIdPrefix))
            .ToDictionaryAsync(item => item.PublicRegionKey, StringComparer.Ordinal, cancellationToken);
        var assignments = await geographyDb.지역농수산Map행정구역CodeAssignments
            .Where(item => item.SchemeCode == RegionalAgriculturalMapCodeSchemeCodes.KoreaMoisAdministrative
                           && item.SourceVintage == revisions[0])
            .ToDictionaryAsync(item => item.ExternalCode, StringComparer.Ordinal, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var added = 0;
        var updated = 0;
        var assignmentAdded = 0;
        var missingParents = 0;
        foreach (var row in rows)
        {
            if (!regions.TryGetValue(row.StableId, out var region))
            {
                region = new 지역농수산Map행정구역
                {
                    Id = CreateDeterministicGuid(row.StableId),
                    PublicRegionKey = row.StableId,
                    CountryCode = RegionalAgriculturalMapCountryCodes.Korea,
                    CreatedAtUtc = now,
                };
                geographyDb.지역농수산Map행정구역들.Add(region);
                regions.Add(row.StableId, region);
                added++;
            }
            else
            {
                updated++;
            }

            region.RegionTypeCode = row.RegionTypeCode;
            region.DisplayNameKo = row.전체명;
            region.DisplayNameLocal = row.전체명;
            region.UpdatedAtUtc = now;
            if (row.ParentStableId is null)
            {
                region.ParentRegionId = null;
            }
            else if (regions.TryGetValue(row.ParentStableId, out var parent))
            {
                region.ParentRegionId = parent.Id;
            }
            else
            {
                region.ParentRegionId = null;
                missingParents++;
            }

            if (!assignments.ContainsKey(row.Code))
            {
                var assignment = new 지역농수산Map행정구역CodeAssignment
                {
                    RegionId = region.Id,
                    SchemeCode = RegionalAgriculturalMapCodeSchemeCodes.KoreaMoisAdministrative,
                    ExternalCode = row.Code,
                    SourceVintage = revisions[0],
                    SourceUrl = OfficialSourceUrl,
                    VerifiedAtUtc = now,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };
                geographyDb.지역농수산Map행정구역CodeAssignments.Add(assignment);
                assignments.Add(row.Code, assignment);
                assignmentAdded++;
            }
        }

        await geographyDb.SaveChangesAsync(cancellationToken);
        return new 대한민국법정동행정구역원장승격Result(
            records.Length,
            added,
            updated,
            assignmentAdded,
            missingParents,
            revisions[0]);
    }

    private static 승격Row Parse(Ssalddel.Domain.PublicData.외부데이터정규화Record record)
    {
        if (!record.StableId.StartsWith(StableIdPrefix, StringComparison.Ordinal)
            || record.StableId.Length != StableIdPrefix.Length + 10)
            throw new InvalidOperationException("KoreaLegalDongStableIdInvalid");
        var code = record.StableId[StableIdPrefix.Length..];
        var (regionType, order) = record.SpatialPrecisionCode switch
        {
            "province" => (RegionalAgriculturalMapRegionTypeCodes.StateProvince, 0),
            "city-county-district" => (RegionalAgriculturalMapRegionTypeCodes.CountyMunicipality, 1),
            "town-neighborhood" => (RegionalAgriculturalMapRegionTypeCodes.TownshipNeighborhood, 2),
            "village" => (RegionalAgriculturalMapRegionTypeCodes.Village, 3),
            _ => throw new InvalidOperationException("KoreaLegalDongLevelInvalid"),
        };
        var parent = DimensionValue(record.DimensionKey, "parent");
        return new 승격Row(code, record.StableId, record.TextValue.Trim(), regionType, order,
            string.IsNullOrWhiteSpace(parent) ? null : parent);
    }

    private static string DimensionValue(string dimensionKey, string name)
        => dimensionKey.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.Split('=', 2))
            .Where(parts => parts.Length == 2 && string.Equals(parts[0], name, StringComparison.Ordinal))
            .Select(parts => parts[1])
            .SingleOrDefault() ?? string.Empty;

    private static Guid CreateDeterministicGuid(string stableId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(stableId));
        var value = bytes[..16];
        value[6] = (byte)((value[6] & 0x0F) | 0x80);
        value[8] = (byte)((value[8] & 0x3F) | 0x80);
        return new Guid(value);
    }

    private sealed record 승격Row(
        string Code,
        string StableId,
        string 전체명,
        string RegionTypeCode,
        int 계층순서,
        string? ParentStableId);
}
