using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Ssalddel.Services.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddOfficialNewsRss(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OfficialNewsRssOptions>(
            configuration.GetSection(OfficialNewsRssOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<공식뉴스RssConditionalCache>();
        services.AddHttpClient<공식뉴스RssClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<OfficialNewsRssOptions>>()
                .Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 60));
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                string.IsNullOrWhiteSpace(options.UserAgent)
                    ? "SsalddelOfficialNewsResearch/1.0"
                    : options.UserAgent.Trim());
        });

        foreach (var feed in 공식뉴스RssFeedCatalog.All)
        {
            services.AddScoped<ICommunityInformationCandidateSource>(serviceProvider =>
                new 공식뉴스RssCandidateSource(
                    feed,
                    serviceProvider.GetRequiredService<공식뉴스RssClient>(),
                    serviceProvider.GetRequiredService<IOptions<OfficialNewsRssOptions>>(),
                    serviceProvider.GetRequiredService<TimeProvider>()));
        }

        return services;
    }
}
