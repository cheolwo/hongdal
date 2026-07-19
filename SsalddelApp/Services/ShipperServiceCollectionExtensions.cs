using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SsalddelApp.Services;

public static class ShipperServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddShipperOptionsModule(configuration);
        services.AddShipperPlatformModule();
        services.AddShipperTransportModule();
        services.AddShipperWarehouseModule();
        services.AddShipperSalesModule();
        services.AddShipperCustomsModule();
        services.AddShipperExternalApiModule();
        return services;
    }
}
