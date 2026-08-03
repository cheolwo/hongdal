using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Controllers.Common;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Domain.Geography;
using Ssalddel.Extensions;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class RegionalAgriculturalMapMarkerQueryTests
{
    [Fact]
    public void AMS기본지도집계에필요한CoveringIndex를_보존한다()
    {
        using var priceDb = CreatePriceDb();
        var entity = priceDb.Model.FindEntityType(typeof(UsdaAms시장가격관측));

        Assert.NotNull(entity);
        var indexNames = entity!.GetIndexes()
            .Select(index => index.GetDatabaseName())
            .ToArray();

        Assert.Contains("IX_agri_ams_map_market_lookup", indexNames);
        Assert.Contains("IX_agri_ams_map_shipping_lookup", indexNames);
    }

    [Fact]
    public async Task MAFRA산지는_검토된교차표와기준점이있을때만_산지Marker로반환한다()
    {
        await using var geographyDb = CreateGeographyDb();
        await using var priceDb = CreatePriceDb();
        var region = Region("kr-11", "KR", "서울특별시");
        geographyDb.지역농수산Map행정구역들.Add(region);
        geographyDb.지역농수산Map행정구역Boundaries.Add(Boundary(region));
        geographyDb.지역농수산Map지역Crosswalks.Add(new 지역농수산Map지역Crosswalk
        {
            SourceSchemeCode = RegionalAgriculturalMapCodeSchemeCodes.KoreaMafraOrigin,
            SourceCode = "110000",
            SourceNameRaw = "서울특별시",
            SourceVintage = "2024",
            TargetRegion = region,
            MatchMethodCode = "OfficialCode",
            ConfidenceCode = RegionalAgriculturalMapConfidenceCodes.OfficialCodeCrosswalk,
            ReviewedAtUtc = Utc(2026, 7, 31),
            EvidenceUrl = "https://example.test/mafra-origin",
            CreatedAtUtc = Utc(2026, 7, 31),
            UpdatedAtUtc = Utc(2026, 7, 31)
        });
        await geographyDb.SaveChangesAsync();

        priceDb.DomesticAuctionPriceObservations.AddRange(
            DomesticObservation("a", "110000", "서울특별시", new(2026, 7, 30)),
            DomesticObservation("b", "110000", "서울특별시", new(2026, 7, 31)),
            DomesticObservation("c", "999999", "지역 미확인", new(2026, 7, 31)));
        await priceDb.SaveChangesAsync();

        var response = await CreateUseCase(geographyDb, priceDb).조회Async(
            new RegionalAgriculturalMapMarkerQuery
            {
                CountryCode = " kr "
            });

        var marker = Assert.Single(response.Items);
        Assert.Equal("KR", response.CountryCode);
        Assert.Equal(RegionalAgriculturalMapRelationTypeCodes.ConfirmedOrigin, marker.RelationTypeCode);
        Assert.Equal("kr-11", marker.RegionKey);
        Assert.Equal(2, marker.ObservationCount);
        Assert.Equal(1, response.UnresolvedObservationCount);
        Assert.Equal(0, response.MissingAnchorRegionCount);
        Assert.Equal("KR-SGIS-HADM", marker.AnchorSourceKey);
        Assert.Equal(RegionalAgriculturalMapConfidenceCodes.OfficialCodeCrosswalk,
            Assert.Single(marker.Sources).CrosswalkConfidenceCode);
    }

    [Fact]
    public async Task AMS시장관측과ShippingPoint는_같은주에서도_별도Marker로유지한다()
    {
        await using var geographyDb = CreateGeographyDb();
        await using var priceDb = CreatePriceDb();
        var california = Region("us-ca", "US", "California");
        geographyDb.지역농수산Map행정구역들.Add(california);
        geographyDb.지역농수산Map행정구역Boundaries.Add(Boundary(
            california,
            "US-CENSUS-TIGER",
            36.7783m,
            -119.4179m));
        geographyDb.지역농수산Map행정구역CodeAssignments.Add(
            new 지역농수산Map행정구역CodeAssignment
            {
                Region = california,
                SchemeCode = RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState,
                ExternalCode = "CA",
                SourceVintage = "2025",
                SourceUrl = "https://example.test/usps",
                VerifiedAtUtc = Utc(2026, 7, 31),
                CreatedAtUtc = Utc(2026, 7, 31),
                UpdatedAtUtc = Utc(2026, 7, 31)
            });
        geographyDb.지역농수산Map지역Crosswalks.Add(new 지역농수산Map지역Crosswalk
        {
            SourceSchemeCode = RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesAmsShippingDistrict,
            SourceCode = "CENTRAL CALIFORNIA",
            SourceNameRaw = "CENTRAL CALIFORNIA",
            SourceVintage = "2025",
            TargetRegion = california,
            MatchMethodCode = "ReviewedDistrict",
            ConfidenceCode = RegionalAgriculturalMapConfidenceCodes.CuratedCrosswalk,
            ReviewedAtUtc = Utc(2026, 7, 31),
            EvidenceUrl = "https://example.test/ams-district",
            CreatedAtUtc = Utc(2026, 7, 31),
            UpdatedAtUtc = Utc(2026, 7, 31)
        });
        await geographyDb.SaveChangesAsync();

        priceDb.UsdaAmsMarketPriceObservations.AddRange(
            AmsObservation("terminal", "Terminal", "CA", "Los Angeles", string.Empty),
            AmsObservation("shipping", "Shipping Point", "CA", "Fresno", "Central   California"));
        await priceDb.SaveChangesAsync();

        var response = await CreateUseCase(geographyDb, priceDb).조회Async(
            new RegionalAgriculturalMapMarkerQuery
            {
                CountryCode = "US",
                ProductName = "APPLES"
            });

        Assert.Equal(2, response.TotalMarkerCount);
        Assert.Contains(response.Items, item =>
            item.RelationTypeCode == RegionalAgriculturalMapRelationTypeCodes.MarketObservation
            && item.ObservationCount == 1);
        Assert.Contains(response.Items, item =>
            item.RelationTypeCode
                == RegionalAgriculturalMapRelationTypeCodes.ShippingPointOrPortOfEntry
            && item.ObservationCount == 1);
        Assert.Equal(0, response.UnresolvedObservationCount);
    }

    [Fact]
    public async Task 코드는연결됐지만_검증된기준점이없으면_Marker를만들지않는다()
    {
        await using var geographyDb = CreateGeographyDb();
        await using var priceDb = CreatePriceDb();
        var region = Region("us-wa", "US", "Washington");
        geographyDb.지역농수산Map행정구역들.Add(region);
        geographyDb.지역농수산Map행정구역CodeAssignments.Add(
            new 지역농수산Map행정구역CodeAssignment
            {
                Region = region,
                SchemeCode = RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState,
                ExternalCode = "WA",
                SourceVintage = "2025",
                SourceUrl = "https://example.test/usps",
                VerifiedAtUtc = Utc(2026, 7, 31),
                CreatedAtUtc = Utc(2026, 7, 31),
                UpdatedAtUtc = Utc(2026, 7, 31)
            });
        await geographyDb.SaveChangesAsync();
        priceDb.UsdaAmsMarketPriceObservations.Add(
            AmsObservation("terminal-wa", "Terminal", "WA", "Seattle", string.Empty));
        await priceDb.SaveChangesAsync();

        var response = await CreateUseCase(geographyDb, priceDb).조회Async(
            new RegionalAgriculturalMapMarkerQuery
            {
                CountryCode = "US",
                RelationTypeCode = RegionalAgriculturalMapRelationTypeCodes.MarketObservation
            });

        Assert.Empty(response.Items);
        Assert.Equal(0, response.UnresolvedObservationCount);
        Assert.Equal(1, response.MissingAnchorRegionCount);
    }

    [Fact]
    public async Task 국가에맞지않는관계유형은_명시적으로거절한다()
    {
        await using var geographyDb = CreateGeographyDb();
        await using var priceDb = CreatePriceDb();
        var useCase = CreateUseCase(geographyDb, priceDb);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => useCase.조회Async(
            new RegionalAgriculturalMapMarkerQuery
            {
                CountryCode = "KR",
                RelationTypeCode = RegionalAgriculturalMapRelationTypeCodes.MarketObservation
            }));

        Assert.Contains("지원하지 않는", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void 같은기준연도의_하나의원천코드는_하나의검토된지역만가진다()
    {
        using var geographyDb = CreateGeographyDb();
        var entity = geographyDb.Model.FindEntityType(typeof(지역농수산Map지역Crosswalk));

        Assert.NotNull(entity);
        var uniqueIndex = Assert.Single(entity!.GetIndexes(), index =>
            index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual(
                ["SourceSchemeCode", "SourceCode", "SourceVintage"]));
        Assert.DoesNotContain(
            uniqueIndex.Properties,
            property => property.Name == "TargetRegionId");
    }

    [Fact]
    public void 공개Controller는_커뮤니티MarkerRoute를_읽기전용으로노출한다()
    {
        var controller = typeof(지역농수산MapController);

        Assert.NotNull(controller.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            "api/v1/community/regional-map/markers",
            controller.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Null(
            controller.GetMethod(nameof(지역농수산MapController.목록조회))
                ?.GetCustomAttribute<HttpGetAttribute>()
                ?.Template);
    }

    [Fact]
    public void UseCase는_가격Reader와지역Resolver만조율하고_두DbContext를직접알지않는다()
    {
        var constructor = Assert.Single(typeof(지역농수산MapMarker조회UseCase).GetConstructors());

        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(지역농수산Map가격관측Reader),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(지역농수산Map지역Resolver),
                parameter.ParameterType));

        var flow = SsalddelCodeMetadataReader.ReadFeature(
                SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
                typeof(RegionalAgriculturalMapMarkerDto).Assembly,
                typeof(지역농수산MapMarker조회UseCase).Assembly)
            .Where(item => item.ComponentType == typeof(RegionalAgriculturalMapMarkerDto)
                           || item.ComponentType == typeof(지역농수산Map가격관측Reader)
                           || item.ComponentType == typeof(지역농수산Map지역Resolver)
                           || item.ComponentType == typeof(지역농수산MapMarker조회UseCase)
                           || item.ComponentType == typeof(지역농수산MapController))
            .ToArray();

        Assert.Equal(new[] { 10, 20, 25, 30, 40 }, flow.Select(item => item.FlowOrder));

        var services = new ServiceCollection();
        services.AddAgriculturalFisheriesInformationModule();
        Assert.Contains(services, item =>
            item.ServiceType == typeof(지역농수산Map가격관측Reader)
            && item.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, item =>
            item.ServiceType == typeof(지역농수산Map지역Resolver)
            && item.Lifetime == ServiceLifetime.Scoped);
    }

    private static 지역농수산MapMarker조회UseCase CreateUseCase(
        SsalddelContext geographyDb,
        AgriculturalFisheriesDbContext priceDb)
        => new(
            new 지역농수산Map가격관측Reader(priceDb),
            new 지역농수산Map지역Resolver(geographyDb));

    private static SsalddelContext CreateGeographyDb()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"regional-map-geography-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static AgriculturalFisheriesDbContext CreatePriceDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase($"regional-map-price-{Guid.NewGuid():N}")
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static 지역농수산Map행정구역 Region(
        string key,
        string countryCode,
        string name)
        => new()
        {
            Id = Guid.NewGuid(),
            PublicRegionKey = key,
            CountryCode = countryCode,
            RegionTypeCode = RegionalAgriculturalMapRegionTypeCodes.StateProvince,
            DisplayNameKo = name,
            DisplayNameEn = name,
            DisplayNameLocal = name,
            CreatedAtUtc = Utc(2026, 7, 31),
            UpdatedAtUtc = Utc(2026, 7, 31)
        };

    private static 지역농수산Map행정구역Boundary Boundary(
        지역농수산Map행정구역 region,
        string sourceCode = "KR-SGIS-HADM",
        decimal latitude = 37.5665m,
        decimal longitude = 126.9780m)
        => new()
        {
            Region = region,
            BoundarySourceCode = sourceCode,
            BoundaryVintage = "2025",
            GeometryReference = "object://verified-boundary",
            AnchorLatitude = latitude,
            AnchorLongitude = longitude,
            SimplificationLevel = 0,
            SourceUrl = "https://example.test/official-boundary",
            VerifiedAtUtc = Utc(2026, 7, 31),
            CreatedAtUtc = Utc(2026, 7, 31),
            UpdatedAtUtc = Utc(2026, 7, 31)
        };

    private static 국내농산물경락가격관측 DomesticObservation(
        string recordKey,
        string originCode,
        string originName,
        DateOnly settlementDate)
        => new()
        {
            FirstCollectionRunId = 1,
            RecordKey = recordKey,
            SourceKey = "mafra-wholesale-market-settlement",
            SettlementDate = settlementDate,
            ItemName = "사과",
            OriginCode = originCode,
            OriginName = originName,
            LastSeenAtUtc = Utc(2026, 7, 31)
        };

    private static UsdaAms시장가격관측 AmsObservation(
        string recordKey,
        string marketType,
        string state,
        string marketName,
        string district)
        => new()
        {
            FirstCollectionRunId = 1,
            RecordKey = recordKey,
            SourceKey = "usda-ams-market-news",
            MarketType = marketType,
            MarketLocationState = state,
            MarketLocationName = marketName,
            District = district,
            Commodity = "APPLES",
            ReportBeginDate = new DateOnly(2026, 7, 1),
            ReportEndDate = new DateOnly(2026, 7, 31),
            LastSeenAtUtc = Utc(2026, 7, 31)
        };

    private static DateTime Utc(int year, int month, int day)
        => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
