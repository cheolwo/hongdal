using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData.Agriculture;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Agriculture;
using 살뜰.Services.External.PublicData.WorldBank;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class AgriculturalDataProviderContractTests
{
    [Fact]
    public void ResearchSources_AreMetadataOnlyAndDisabled()
    {
        var sources = new AgriculturalDataResearchSourceRegistration().GetDefinitions();

        Assert.Equal(2, sources.Count);
        Assert.All(sources, source =>
        {
            Assert.False(source.DefaultCollectionEnabled);
            Assert.False(source.RequiresCredential);
            Assert.NotEqual(string.Empty, source.License);
            Assert.NotEqual(string.Empty, source.SpatialResolution);
            Assert.NotEqual(string.Empty, source.TemporalResolution);
        });
        Assert.Equal(
            ExternalDataAccessMethod.DownloadFile,
            sources.Single(source => source.SourceId == "fao-faostat").AccessMethod);
        Assert.Equal(
            ExternalDataAccessMethod.OgcWcs,
            sources.Single(source => source.SourceId == "isric-soilgrids").AccessMethod);
    }

    [Fact]
    public void SourceCatalog_CanCombineWorldBankFaostatAndSoilGridsWithoutCollision()
    {
        var catalog = new ExternalDataSourceCatalog(
            new EmptyApiCatalog(),
            [
                new WorldBank경지면적SourceRegistration(),
                new AgriculturalDataResearchSourceRegistration(),
            ]);

        Assert.Equal(3, catalog.GetCatalog().Items.Count);
        Assert.Equal(
            "Annual",
            catalog.GetRequired("fao-faostat", "land-use-rl").TemporalResolution);
        Assert.Equal(
            "250m raster grid; requested bounded coverage",
            catalog.GetRequired("isric-soilgrids", "soilgrids-250m-properties").SpatialResolution);
    }

    [Theory]
    [InlineData("phh2o_0-5cm_Q0.5", "soil.ph-water", 0, 5, "median", 10)]
    [InlineData("nitrogen_5-15cm_Q0.05", "soil.total-nitrogen", 5, 15, "quantile-0.05", 100)]
    [InlineData("soc_100-200cm_mean", "soil.organic-carbon", 100, 200, "mean", 10)]
    public void SoilGridsCoverageId_MapsPropertyDepthStatisticAndConversion(
        string coverageId,
        string metric,
        decimal depthStart,
        decimal depthEnd,
        string statistic,
        decimal divisor)
    {
        var parsed = SoilGridsMetadataCatalog.TryParseCoverageId(coverageId, out var definition);

        Assert.True(parsed);
        Assert.NotNull(definition);
        Assert.Equal(metric, definition.Property.MetricCode);
        Assert.Equal(depthStart, definition.DepthStartCm);
        Assert.Equal(depthEnd, definition.DepthEndCm);
        Assert.Equal(statistic, definition.StatisticCode);
        Assert.Equal(divisor, definition.Property.ConversionDivisor);
    }

    [Theory]
    [InlineData("phh2o_0-10cm_Q0.5")]
    [InlineData("unknown_0-5cm_Q0.5")]
    [InlineData("phh2o_0-5cm_Q0.7")]
    [InlineData("")]
    public void SoilGridsCoverageId_RejectsUnknownContract(string coverageId)
        => Assert.False(SoilGridsMetadataCatalog.TryParseCoverageId(coverageId, out _));

    [Fact]
    public void SoilContract_PreservesMappedValueAndConventionalConversion()
    {
        var property = SoilGridsMetadataCatalog.Properties["phh2o"];
        var fact = new 지역토양Data
        {
            SpatialReferenceId = "area:kr:test-bounds",
            SpatialPrecisionCode = "grid-250m",
            CoverageId = "phh2o_0-5cm_Q0.5",
            MetricCode = property.MetricCode,
            Depth = new 토양DepthInterval(0, 5),
            StatisticCode = "median",
            SourceMappedValue = 54,
            SourceMappedUnitCode = property.MappedUnitCode,
            ConversionDivisor = property.ConversionDivisor,
            Value = 54 / property.ConversionDivisor,
            UnitCode = property.ConventionalUnitCode,
        };

        Assert.True(fact.Depth!.IsValid);
        Assert.Equal(5.4m, fact.Value);
        Assert.Equal("ph", fact.UnitCode);
        Assert.Equal(54m, fact.SourceMappedValue);
    }

    [Fact]
    public async Task PublicDataDb_SeedsProviderCountryCodesAsExplicitMappings()
    {
        var options = new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        await using var db = new PublicDataIngestionDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var mappings = await db.RegionMappings
            .Where(item => item.SourceId == WorldBank경지면적Dataset.SourceId)
            .OrderBy(item => item.ExternalRegionCode)
            .ToArrayAsync();

        Assert.Equal(3, mappings.Length);
        Assert.Contains(mappings, item => item.ExternalRegionCode == "KOR" && item.RegionStableId == "country:kr");
        Assert.Contains(mappings, item => item.ExternalRegionCode == "USA" && item.RegionStableId == "country:us");
        Assert.Contains(mappings, item => item.ExternalRegionCode == "CHN" && item.RegionStableId == "country:cn");
    }

    private sealed class EmptyApiCatalog : IPublicDataApiMetadataCatalog
    {
        public PublicDataApiMetadataResponse GetCatalog(PublicDataApiMetadataQuery query) => new();
    }
}
