using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class PublicDataApiMetadataCatalogTests
{
    [Fact]
    public void GetCatalog_수입식품서버모듈의공식계약을제공한다()
    {
        var catalog = new PublicDataApiMetadataCatalog();

        var result = catalog.GetCatalog(new PublicDataApiMetadataQuery
        {
            Domain = "ImportedFood"
        });

        Assert.Equal(3, result.Items.Count);
        Assert.Contains(result.Items, item => item.Key == "mfds-imported-food-product-db");
        Assert.Contains(result.Items, item => item.Key == "mfds-imported-food-overseas-manufacturer");

        var label = Assert.Single(result.Items, item => item.Key == "mfds-imported-food-korean-label");
        Assert.Contains("IRDNT_NM", label.MainResponseFields);
        Assert.Contains(label.UsageNotes, note => note.Contains("확정값이 아니라 후보", StringComparison.Ordinal));

        var product = Assert.Single(result.Items, item => item.Key == "mfds-imported-food-product-db");
        Assert.Contains(product.UsageNotes, note => note.Contains("HSK 코드가 아니므로", StringComparison.Ordinal));
    }

    [Fact]
    public void GetCatalog_실제Client와키설정경계를비밀값없이제공한다()
    {
        const string configuredSecret = "do-not-expose-this-value";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicData:DataGoKrServiceKey"] = configuredSecret,
                ["NtsBusinessRegistration:ServiceKey"] = string.Empty
            })
            .Build();
        var catalog = new PublicDataApiMetadataCatalog(configuration);

        var connected = catalog.GetCatalog(new PublicDataApiMetadataQuery
        {
            ImplementationStatusCode = PublicDataApiImplementationStatusCodes.Connected
        });
        var apartment = Assert.Single(connected.Items, item => item.Key == "kapt-apartment-complex-list");
        Assert.True(apartment.IsServiceKeyConfigured);
        Assert.Equal("ApartmentComplexLookupService", apartment.ClientType);
        Assert.Contains("PublicData:DataGoKrServiceKey", apartment.ConfigurationPaths);
        Assert.Contains("sample fallback", apartment.ErrorPolicy, StringComparison.Ordinal);
        Assert.Contains("자동 재시도 없음", apartment.RetryPolicy, StringComparison.Ordinal);
        Assert.Contains(connected.Items, item => item.Key == "nts-business-registration-status");
        Assert.DoesNotContain(configuredSecret, JsonSerializer.Serialize(connected), StringComparison.Ordinal);
    }

    [Fact]
    public void GetCatalog_키없는파일Source와추가공공데이터Client를구분한다()
    {
        var catalog = new PublicDataApiMetadataCatalog(new ConfigurationBuilder().Build());

        var result = catalog.GetCatalog(new PublicDataApiMetadataQuery());

        var fishingAreas = Assert.Single(result.Items, item => item.Key == "mof-fishing-area-file");
        Assert.Equal(PublicDataApiImplementationStatusCodes.Connected, fishingAreas.ImplementationStatusCode);
        Assert.False(fishingAreas.RequiresServiceKey);
        Assert.False(fishingAreas.IsServiceKeyConfigured);
        Assert.Contains(result.Items, item => item.Key == "fish-cooperative-general-statistics");
        Assert.Contains(result.Items, item => item.Key == "kapt-apartment-management-fees");
        Assert.Contains(result.Items, item => item.Key == "nts-business-registration-status");
    }
}
