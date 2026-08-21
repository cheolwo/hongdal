using Microsoft.Extensions.Options;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Agriculture;
using 살뜰.Services.External.PublicData.WorldBank;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddAgriculturalExternalDataProviders(this IServiceCollection services)
    {
        services.AddOptions<WorldBank경지면적Options>()
            .BindConfiguration(WorldBank경지면적Options.SectionName)
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
                                 && uri.Scheme == Uri.UriSchemeHttps,
                "World Bank base URL must be an absolute HTTPS URL.")
            .Validate(options => options.CountryCodes is { Length: > 0 },
                "At least one World Bank country code is required.")
            .Validate(options => options.MostRecentValues is >= 1 and <= 20,
                "World Bank most recent value limit is invalid.")
            .Validate(options => options.MaxResponseBytes is >= 1024 and <= 50 * 1024 * 1024,
                "World Bank response size limit is invalid.");

        services.AddSingleton<IExternalDataSourceRegistration, WorldBank경지면적SourceRegistration>();
        services.AddSingleton<IExternalDataSourceRegistration, AgriculturalDataResearchSourceRegistration>();
        services.AddSingleton<IExternalDataSourceRegistration, FarmRealityDataSourceRegistration>();
        services.AddHttpClient<WorldBank경지면적Collector>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-ExternalData/1.0");
        });
        services.AddScoped<IExternalDataCollector>(provider =>
            provider.GetRequiredService<WorldBank경지면적Collector>());
        services.AddScoped<IExternalDataNormalizer, WorldBank경지면적Normalizer>();
        return services;
    }
}
