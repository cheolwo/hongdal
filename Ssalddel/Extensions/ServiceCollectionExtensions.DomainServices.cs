using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelDomainServices(this IServiceCollection services)
    {
        services.AddSsalddelHsCodeCatalog();
        services.AddSsalddelFoundationDomainServices();
        services.AddSsalddelCollectiveProcurementDomainServices();
        services.TryAddSingleton(공동구매수요모집Os배치등록계획.빈계획());
        services.AddSingleton<I공동구매수요모집Os배치Catalog, 공동구매수요모집Os배치Catalog>();
        services.AddSsalddelCommunityDomainServices();
        services.AddSsalddelOperationsDomainServices();

        return services;
    }
}
