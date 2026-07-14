using Hongdal.Application.Operations;
using Hongdal.Contracts.Common.Operations;
using Hongdal.Services.Operations;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalOperatingMarketServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var configuredMarketCode = configuration[
            $"{OperatingMarketDeploymentOptions.SectionName}:{nameof(OperatingMarketDeploymentOptions.MarketCode)}"];
        configuredMarketCode = string.IsNullOrWhiteSpace(configuredMarketCode)
            ? OperatingMarketCodes.Korea
            : configuredMarketCode;

        if (!OperatingMarketCodes.TryNormalize(configuredMarketCode, out var marketCode))
        {
            throw new InvalidOperationException(
                $"{OperatingMarketDeploymentOptions.SectionName}:MarketCode must be KR or US.");
        }

        var verifiedLicensedBrokerPartnerId = configuration[
            $"{OperatingMarketDeploymentOptions.SectionName}:" +
            nameof(OperatingMarketDeploymentOptions.VerifiedLicensedBrokerPartnerId)];
        var deployment = new OperatingMarketDeployment(
            marketCode,
            verifiedLicensedBrokerPartnerId);
        IOperatingMarketServiceModule module = marketCode switch
        {
            OperatingMarketCodes.Korea => new KoreaOperatingMarketServiceModule(),
            OperatingMarketCodes.UnitedStates => new UnitedStatesOperatingMarketServiceModule(),
            _ => throw new InvalidOperationException(
                $"No service module exists for operating market {marketCode}.")
        };

        services.AddSingleton<IOperatingMarketDeployment>(deployment);
        services.AddSingleton<IOperatingMarketServiceModule>(module);
        services.AddScoped<IOperatingMarketContextAccessor,
            DeploymentOperatingMarketContextAccessor>();
        services.AddScoped<IOperatingMarketAddressLookupService,
            OperatingMarketAddressLookupService>();
        module.AddServices(services);

        return services;
    }
}
