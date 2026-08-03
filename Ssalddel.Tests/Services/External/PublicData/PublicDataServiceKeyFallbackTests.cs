using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ssalddel.Extensions;
using 살뜰.Services.External.Nts;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class PublicDataServiceKeyFallbackTests
{
    [Fact]
    public void AddSsalddelOptions_국세청Client가공통DataGoKrKey를사용한다()
    {
        const string sharedKey = "shared-data-go-kr-key";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicData:DataGoKrServiceKey"] = sharedKey
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSsalddelOptions(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider
            .GetRequiredService<IOptions<NtsBusinessRegistrationOptions>>()
            .Value;

        Assert.Equal(sharedKey, options.ServiceKey);
    }

    [Fact]
    public void AddSsalddelOptions_국세청전용Key가있으면공통Key로덮지않는다()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicData:DataGoKrServiceKey"] = "shared-key",
                ["NtsBusinessRegistration:ServiceKey"] = "nts-specific-key"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSsalddelOptions(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider
            .GetRequiredService<IOptions<NtsBusinessRegistrationOptions>>()
            .Value;

        Assert.Equal("nts-specific-key", options.ServiceKey);
    }
}
