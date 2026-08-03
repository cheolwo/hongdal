using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 공공데이터포털활용ApiModuleCatalogTests
{
    [Fact]
    public void GetCatalog_활용중33개Api를10개업무Module로분리한다()
    {
        var catalog = CreateCatalog();

        var result = catalog.GetCatalog(new 공공데이터포털활용ApiModuleQuery());
        var apis = result.Items.SelectMany(item => item.Apis).ToArray();

        Assert.Equal(new DateOnly(2026, 8, 3), result.VerifiedOn);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(33, apis.Length);
        Assert.Equal(33, apis.Select(api => api.DataId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(apis, api => Assert.StartsWith("uddi:", api.DataId, StringComparison.Ordinal));
    }

    [Fact]
    public void GetCatalog_기존Client가있는Api와CatalogOnlyApi를구분한다()
    {
        var catalog = CreateCatalog("configured-without-exposing-value");

        var result = catalog.GetCatalog(new 공공데이터포털활용ApiModuleQuery());

        var apartment = Assert.Single(result.Items, item => item.Key == "molit-apartment-reference");
        Assert.Equal(공공데이터포털활용ApiModuleCoverageCodes.Full, apartment.CoverageCode);
        Assert.Equal(10, apartment.Apis.Count);
        Assert.Equal(10, apartment.Apis.Count(api => api.ImplementationStatusCode == PublicDataApiImplementationStatusCodes.Connected));

        var fisheries = Assert.Single(result.Items, item => item.Key == "mof-fisheries-distribution-reference");
        Assert.Equal(공공데이터포털활용ApiModuleCoverageCodes.Full, fisheries.CoverageCode);
        Assert.All(fisheries.Apis, api => Assert.Equal(
            PublicDataApiImplementationStatusCodes.Connected,
            api.ImplementationStatusCode));

        var customs = Assert.Single(result.Items, item => item.Key == "customs-country-trade-statistics");
        Assert.Equal(공공데이터포털활용ApiModuleCoverageCodes.Full, customs.CoverageCode);
        Assert.Equal("HsCountryTradeUnitPriceLookupService", Assert.Single(customs.Apis).ClientType);

        var tourism = Assert.Single(result.Items, item => item.Key == "tourapi-regional-culture");
        Assert.Equal(공공데이터포털활용ApiModuleCoverageCodes.Full, tourism.CoverageCode);
        Assert.Equal("국문관광정보공공데이터Client", Assert.Single(tourism.Apis).ClientType);

        var comparison = Assert.Single(result.Items, item => item.Key == "online-price-kosis-comparison");
        Assert.Equal(공공데이터포털활용ApiModuleCoverageCodes.Full, comparison.CoverageCode);
        Assert.Equal(3, comparison.Apis.Count);
        Assert.All(comparison.Apis, api => Assert.Equal(
            PublicDataApiImplementationStatusCodes.Connected,
            api.ImplementationStatusCode));
    }

    [Fact]
    public void GetCatalog_활용계정식별자와인증키값을반환하지않는다()
    {
        const string configuredSecret = "secret-must-not-be-serialized";
        var catalog = CreateCatalog(configuredSecret);

        var json = JsonSerializer.Serialize(catalog.GetCatalog(new 공공데이터포털활용ApiModuleQuery()));

        Assert.DoesNotContain(configuredSecret, json, StringComparison.Ordinal);
        Assert.DoesNotContain("122538592", json, StringComparison.Ordinal);
        Assert.DoesNotContain("71591330", json, StringComparison.Ordinal);
    }

    private static 공공데이터포털활용ApiModuleCatalog CreateCatalog(string? serviceKey = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicData:DataGoKrServiceKey"] = serviceKey,
                ["NtsBusinessRegistration:ServiceKey"] = serviceKey
            })
            .Build();

        return new 공공데이터포털활용ApiModuleCatalog(
            new PublicDataApiMetadataCatalog(configuration));
    }
}
