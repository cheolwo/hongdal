using Hongdal.Application.Operations;
using Hongdal.Contracts.Common.Operations;
using Hongdal.Extensions;
using Hongdal.Services.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Tests.Services.Operations;

public sealed class OperatingMarketServiceModuleTests
{
    [Fact]
    public void Registration_DefaultsToKoreaModule()
    {
        var services = RegisterServices(marketCode: null);

        AssertMarketModule<KoreaOperatingMarketServiceModule,
            KoreaRoadAddressLookupAdapter,
            KoreaOperatingMarketFreightWorkflowPolicy>(
            services,
            OperatingMarketCodes.Korea);
    }

    [Fact]
    public void Registration_SelectsUnitedStatesModuleOnly()
    {
        var services = RegisterServices(OperatingMarketCodes.UnitedStates);

        AssertMarketModule<UnitedStatesOperatingMarketServiceModule,
            UnitedStatesAddressLookupAdapter,
            UnitedStatesOperatingMarketFreightWorkflowPolicy>(
            services,
            OperatingMarketCodes.UnitedStates);
    }

    [Fact]
    public void Registration_ReadsVerifiedPartnerFromServerConfiguration()
    {
        var services = RegisterServices(
            OperatingMarketCodes.UnitedStates,
            " broker-partner-1 ");

        var deploymentDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IOperatingMarketDeployment));
        var deployment = Assert.IsType<OperatingMarketDeployment>(
            deploymentDescriptor.ImplementationInstance);

        Assert.Equal("broker-partner-1", deployment.VerifiedLicensedBrokerPartnerId);
    }

    [Fact]
    public void Registration_RejectsUnsupportedMarket()
    {
        var configuration = CreateConfiguration("JP");
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddHongdalOperatingMarketServices(configuration));

        Assert.Contains("must be KR or US", exception.Message);
    }

    private static ServiceCollection RegisterServices(
        string? marketCode,
        string? verifiedLicensedBrokerPartnerId = null)
    {
        var services = new ServiceCollection();
        services.AddHongdalOperatingMarketServices(CreateConfiguration(
            marketCode,
            verifiedLicensedBrokerPartnerId));
        return services;
    }

    private static IConfiguration CreateConfiguration(
        string? marketCode,
        string? verifiedLicensedBrokerPartnerId = null)
    {
        var values = new Dictionary<string, string?>();
        if (marketCode is not null)
        {
            values[$"{OperatingMarketDeploymentOptions.SectionName}:MarketCode"] = marketCode;
        }

        if (verifiedLicensedBrokerPartnerId is not null)
        {
            values[$"{OperatingMarketDeploymentOptions.SectionName}:" +
                   nameof(OperatingMarketDeploymentOptions.VerifiedLicensedBrokerPartnerId)] =
                verifiedLicensedBrokerPartnerId;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static void AssertMarketModule<TModule, TAddressAdapter, TFreightPolicy>(
        IServiceCollection services,
        string expectedMarketCode)
    {
        var deploymentDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IOperatingMarketDeployment));
        var deployment = Assert.IsType<OperatingMarketDeployment>(
            deploymentDescriptor.ImplementationInstance);
        Assert.Equal(expectedMarketCode, deployment.MarketCode);

        var moduleDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IOperatingMarketServiceModule));
        Assert.IsType<TModule>(moduleDescriptor.ImplementationInstance);

        var addressDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IOperatingMarketAddressLookupAdapter));
        Assert.Equal(typeof(TAddressAdapter), addressDescriptor.ImplementationType);

        var policyDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IOperatingMarketFreightWorkflowPolicy));
        Assert.Equal(typeof(TFreightPolicy), policyDescriptor.ImplementationType);

        var contextDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IOperatingMarketContextAccessor));
        Assert.Equal(
            typeof(DeploymentOperatingMarketContextAccessor),
            contextDescriptor.ImplementationType);
    }
}
