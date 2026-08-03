using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Services.Content;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongikAcademyContentMapModule(
        this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<I홍익학당철학영상MapLayer조회UseCase,
            홍익학당철학영상MapLayer조회UseCase>();
        return services;
    }
}
