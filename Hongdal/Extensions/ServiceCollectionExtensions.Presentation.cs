namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSignalR();
        services.AddOpenApi();
        services.AddDataProtection();
        return services;
    }
}
