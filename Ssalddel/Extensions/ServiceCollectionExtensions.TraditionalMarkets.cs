using Ssalddel.Infrastructure.Persistence.TraditionalMarkets;
using Ssalddel.Services.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddTraditionalMarketModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<TraditionalMarketDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.MigrationsAssembly("Ssalddel.Infrastructure");
                    mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_TraditionalMarkets");
                    mysqlOptions.EnableRetryOnFailure();
                }));

        services.AddHttpClient<ITraditionalMarketPublicDataClient, TraditionalMarketPublicDataClient>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PublicDataOptions>>().Value;
                var baseUrl = options.TraditionalMarket.BaseUrl.TrimEnd('/') + "/";
                client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            });
        services.AddScoped<ITraditionalMarketCatalogService, TraditionalMarketCatalogService>();
        services.AddScoped<ITraditionalMarketLogisticsHubService, TraditionalMarketLogisticsHubService>();
        services.AddScoped<I전통시장생활권협의Service, 전통시장생활권협의Service>();
        return services;
    }
}
