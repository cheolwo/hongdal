using Ssalddel.Services.External.Apify.SocialMedia;
using Ssalddel.Services.External.Free.SocialMedia;
using Microsoft.Extensions.DependencyInjection.Extensions;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddFreeSocialMediaResearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FreeSocialMediaOptions>(
            configuration.GetSection(FreeSocialMediaOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpClient<RedditRssPublicContentSource>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FreeSocialMediaOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 60));
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                string.IsNullOrWhiteSpace(options.UserAgent)
                    ? "SsalddelPublicFeedResearch/1.0"
                    : options.UserAgent.Trim());
        });
        services.AddScoped<ISocialMediaPublicContentSource>(sp =>
            sp.GetRequiredService<RedditRssPublicContentSource>());
        return services;
    }
}
