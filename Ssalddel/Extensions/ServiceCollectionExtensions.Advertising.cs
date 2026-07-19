using Ssalddel.Services.Advertising;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoleAdvertisingIntegration(this IServiceCollection services)
    {
        services.AddSingleton<IRoleAdvertisingAudienceCatalog, RoleAdvertisingAudienceCatalog>();
        services.AddSingleton<IRoleAdvertisingPlatformAdapter, MetaRoleAdvertisingPlatformAdapter>();
        services.AddSingleton<IRoleAdvertisingPlatformAdapter, GoogleAdsRoleAdvertisingPlatformAdapter>();
        services.AddSingleton<IRoleAdvertisingPlatformAdapter, LinkedInRoleAdvertisingPlatformAdapter>();
        services.AddSingleton<IRoleAdvertisingPlatformAdapter, NaverSearchAdsRoleAdvertisingPlatformAdapter>();
        services.AddScoped<IRoleAdvertisingCampaignPlanner, RoleAdvertisingCampaignPlanner>();

        return services;
    }
}
