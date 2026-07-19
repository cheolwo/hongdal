namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelDomainServices(this IServiceCollection services)
    {
        services.AddSsalddelFoundationDomainServices();
        services.AddSsalddelCollectiveProcurementDomainServices();
        services.AddSsalddelCommunityDomainServices();
        services.AddSsalddelOperationsDomainServices();

        return services;
    }
}
