using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Services.Customs;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddSsalddelHsCodeCatalog(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services
            .AddHttpClient<IKcsHskCatalogSource, KcsHskCatalogSource>(client =>
            {
                client.BaseAddress = new Uri("https://unipass.customs.go.kr/clip/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-KCS-HSK-Catalog-Collector/1.0");
                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .RemoveAllLoggers();
        services.AddScoped<IKcsHskCatalogImportService, KcsHskCatalogImportService>();

        return services;
    }
}
