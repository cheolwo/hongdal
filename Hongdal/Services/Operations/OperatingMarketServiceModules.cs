using Hongdal.Contracts.Common.Operations;

namespace Hongdal.Services.Operations;

public interface IOperatingMarketServiceModule
{
    string MarketCode { get; }

    void AddServices(IServiceCollection services);
}

public sealed class KoreaOperatingMarketServiceModule : IOperatingMarketServiceModule
{
    public string MarketCode => OperatingMarketCodes.Korea;

    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IOperatingMarketAddressLookupAdapter, KoreaRoadAddressLookupAdapter>();
        services.AddScoped<IOperatingMarketFreightWorkflowPolicy>(serviceProvider =>
            new KoreaOperatingMarketFreightWorkflowPolicy(
                serviceProvider.GetRequiredService<
                    IOperatingMarketFreightServiceProviderRegistry>(),
                serviceProvider.GetRequiredService<TimeProvider>()));
    }
}

public sealed class UnitedStatesOperatingMarketServiceModule : IOperatingMarketServiceModule
{
    public string MarketCode => OperatingMarketCodes.UnitedStates;

    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IOperatingMarketAddressLookupAdapter,
            UnitedStatesAddressLookupAdapter>();
        services.AddScoped<IOperatingMarketFreightWorkflowPolicy>(serviceProvider =>
            new UnitedStatesOperatingMarketFreightWorkflowPolicy(
                serviceProvider.GetRequiredService<
                    IOperatingMarketFreightServiceProviderRegistry>(),
                serviceProvider.GetRequiredService<TimeProvider>()));
    }
}
