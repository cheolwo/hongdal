using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 대한민국법정동행정구역원장승격Tests
{
    [Fact]
    public async Task 현행법정동은_계층과공식Code근거를유지해_행정구역원장으로승격한다()
    {
        await using var ingestionDb = CreateIngestionDb();
        await using var geographyDb = CreateGeographyDb();
        ingestionDb.NormalizedRecords.AddRange(
            Record("1100000000", "서울특별시", "province", null),
            Record("1111000000", "서울특별시 종로구", "city-county-district", "1100000000"),
            Record("1111010100", "서울특별시 종로구 청운동", "town-neighborhood", "1111000000"),
            Record("1111010101", "서울특별시 종로구 청운동 시험리", "village", "1111010100"),
            Record("1111010102", "폐지 예시", "village", "1111010100", "abolished"));
        await ingestionDb.SaveChangesAsync();

        var result = await new 대한민국법정동행정구역원장승격Service(
                ingestionDb,
                geographyDb,
                TimeProvider.System)
            .승격Async();

        Assert.Equal(4, result.현행정규화Record수);
        Assert.Equal(4, result.행정구역추가수);
        Assert.Equal(4, result.CodeAssignment추가수);
        Assert.Equal(0, result.상위구역미확인수);
        var village = await geographyDb.지역농수산Map행정구역들
            .SingleAsync(item => item.PublicRegionKey == "region:kr:bjd:1111010101");
        Assert.Equal(RegionalAgriculturalMapRegionTypeCodes.Village, village.RegionTypeCode);
        var parent = await geographyDb.지역농수산Map행정구역들.FindAsync(village.ParentRegionId);
        Assert.Equal("region:kr:bjd:1111010100", parent!.PublicRegionKey);
        var assignment = await geographyDb.지역농수산Map행정구역CodeAssignments
            .SingleAsync(item => item.ExternalCode == "1111010101");
        Assert.Equal(RegionalAgriculturalMapCodeSchemeCodes.KoreaMoisAdministrative, assignment.SchemeCode);
        Assert.StartsWith("https://www.code.go.kr/", assignment.SourceUrl, StringComparison.Ordinal);
        Assert.DoesNotContain(geographyDb.지역농수산Map행정구역들,
            item => item.PublicRegionKey == "region:kr:bjd:1111010102");
    }

    [Fact]
    public async Task 같은Revision을다시승격해도_행정구역과Code근거가중복되지않는다()
    {
        await using var ingestionDb = CreateIngestionDb();
        await using var geographyDb = CreateGeographyDb();
        ingestionDb.NormalizedRecords.Add(Record("4100000000", "경기도", "province", null));
        await ingestionDb.SaveChangesAsync();
        var service = new 대한민국법정동행정구역원장승격Service(
            ingestionDb,
            geographyDb,
            TimeProvider.System);

        await service.승격Async();
        var second = await service.승격Async();

        Assert.Equal(0, second.행정구역추가수);
        Assert.Equal(1, second.행정구역갱신수);
        Assert.Equal(0, second.CodeAssignment추가수);
        Assert.Equal(1, await geographyDb.지역농수산Map행정구역들.CountAsync());
        Assert.Equal(1, await geographyDb.지역농수산Map행정구역CodeAssignments.CountAsync());
    }

    private static 외부데이터정규화Record Record(
        string code,
        string name,
        string level,
        string? parentCode,
        string status = "active")
    {
        var stableId = $"region:kr:bjd:{code}";
        var parent = parentCode is null ? string.Empty : $"region:kr:bjd:{parentCode}";
        return new 외부데이터정규화Record
        {
            RecordKey = code.PadRight(64, '0'),
            StableId = stableId,
            SourceId = 대한민국법정동CodeDataset.SourceId,
            DatasetId = 대한민국법정동CodeDataset.DatasetId,
            RegionStableId = stableId,
            MetricCode = 대한민국법정동CodeDataset.MetricCode,
            TextValue = name,
            UnitCode = "text",
            EvidenceAsOfUtc = DateTimeOffset.Parse("2026-08-11T00:00:00Z"),
            CollectedAtUtc = DateTimeOffset.Parse("2026-08-11T00:00:00Z"),
            SpatialPrecisionCode = level,
            TemporalPrecisionCode = "collection-time",
            QualityCode = "official-reference",
            DimensionKey = $"code={code}|status={status}|level={level}|parent={parent}",
            DataRevision = "mois-bjd-test-revision",
            SourceVersion = "test",
        };
    }

    private static PublicDataIngestionDbContext CreateIngestionDb()
    {
        var options = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PublicDataIngestionDbContext(options);
    }

    private static SsalddelContext CreateGeographyDb()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
