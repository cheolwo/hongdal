using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.FoodCulture;
using Ssalddel.Domain.Geography;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.FoodCulture;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class 해외제조업소MapMarkerReaderTests
{
    [Fact]
    public async Task 공식코드와검증된권역이있는제조업소만_행정권역집계Marker로반환한다()
    {
        await using var archiveDb = CreateArchiveDb();
        await using var geographyDb = CreateGeographyDb();
        var california = new 지역농수산Map행정구역
        {
            Id = Guid.NewGuid(),
            PublicRegionKey = "us-ca",
            CountryCode = "US",
            RegionTypeCode = RegionalAgriculturalMapRegionTypeCodes.StateProvince,
            DisplayNameKo = "캘리포니아주",
            DisplayNameEn = "California",
            DisplayNameLocal = "California",
            CreatedAtUtc = Utc(2026, 8, 3),
            UpdatedAtUtc = Utc(2026, 8, 3)
        };
        geographyDb.지역농수산Map행정구역들.Add(california);
        geographyDb.지역농수산Map행정구역Boundaries.Add(new 지역농수산Map행정구역Boundary
        {
            Region = california,
            BoundarySourceCode = "US-CENSUS-TIGER",
            BoundaryVintage = "2025",
            GeometryReference = "object://verified-state",
            AnchorLatitude = 36.7783m,
            AnchorLongitude = -119.4179m,
            SourceUrl = "https://www.census.gov/geographies/mapping-files/time-series/geo/tiger-line-file.html",
            VerifiedAtUtc = Utc(2026, 8, 3),
            CreatedAtUtc = Utc(2026, 8, 3),
            UpdatedAtUtc = Utc(2026, 8, 3)
        });
        geographyDb.지역농수산Map행정구역CodeAssignments.Add(
            new 지역농수산Map행정구역CodeAssignment
            {
                Region = california,
                SchemeCode = RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState,
                ExternalCode = "CA",
                SourceVintage = "2025",
                SourceUrl = "https://www.census.gov/",
                VerifiedAtUtc = Utc(2026, 8, 3),
                CreatedAtUtc = Utc(2026, 8, 3),
                UpdatedAtUtc = Utc(2026, 8, 3)
            });
        await geographyDb.SaveChangesAsync();

        archiveDb.OfficialFoodIngredientCompanyEvidence.AddRange(
            Evidence(1, "org-a", "US", "미국", "US-CA", "캘리포니아주", 0.95m),
            Evidence(2, "org-a", "US", "미국", "US-CA", "캘리포니아주", 0.95m),
            Evidence(3, "org-b", "US", "미국", "US-CA", "캘리포니아주", 1m),
            Evidence(4, "org-low", "US", "미국", "US-CA", "캘리포니아주", 0.5m),
            Evidence(5, "org-cn", "CN", "중국", "CN-SHANDONG", "산둥성", 0.95m));
        await archiveDb.SaveChangesAsync();

        var markers = await new 해외제조업소MapMarkerReader(
            archiveDb,
            geographyDb).공개Marker조회Async();

        Assert.Equal(2, markers.Count);
        var us = Assert.Single(markers, item => item.StableRegionKey == "us-california");
        Assert.Equal(2, us.OrganizationCount);
        Assert.Equal(3, us.EvidenceCount);
        Assert.Equal(36.7783, us.Latitude, 4);
        Assert.Contains("개별 제조업소 주소가 아닙니다", us.RegionBoundary, StringComparison.Ordinal);
        var china = Assert.Single(markers, item => item.StableRegionKey == "cn-shandong");
        Assert.Equal(1, china.OrganizationCount);
        Assert.StartsWith("https://", china.AnchorSourceUrl, StringComparison.Ordinal);
    }

    private static OfficialFoodIngredientCompanyEvidence Evidence(
        long id,
        string organizationKey,
        string countryCode,
        string countryName,
        string regionCode,
        string regionName,
        decimal confidence)
        => new()
        {
            Id = id,
            IngredientId = id,
            LastResearchRunId = 1,
            CandidateKey = $"candidate-{id}",
            OrganizationKey = organizationKey,
            OrganizationName = $"공개 제조업소 {id}",
            CountryCode = countryCode,
            CountryName = countryName,
            RelationCode = OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer,
            VerificationStatusCode =
                OfficialFoodIngredientCompanyVerificationStatusCodes.OverseasFacilityMatched,
            ManufacturerRegionCode = regionCode,
            ManufacturerRegionName = regionName,
            ManufacturerRegionScope = $"{regionName} 행정권역",
            ManufacturerRegionConfidence = confidence,
            LastObservedAtUtc = Utc(2026, 8, 3),
            IsCurrent = true
        };

    private static AgriculturalFisheriesDbContext CreateArchiveDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase($"overseas-manufacturer-map-{Guid.NewGuid():N}")
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static SsalddelContext CreateGeographyDb()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"overseas-manufacturer-geography-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static DateTime Utc(int year, int month, int day)
        => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
