using Ssalddel.Filters;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelPresentation(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<SsalddelApiVersionFeatureFilter>();
        });
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSignalR();
        services.AddOpenApi();
        services.AddDataProtection();
        return services;
    }
}
