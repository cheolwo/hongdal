using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Services.Content;
using Ssalddel.Services.External.Apify.YouTube;
using Ssalddel.Services.External.YouTube;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApifyYouTubeContentCollection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApifyYouTubeTranscriptResearch(configuration);
        services.Configure<ApifyYouTubeCommentsOptions>(
            configuration.GetSection(ApifyYouTubeCommentsOptions.SectionName));

        var commentsOptions = configuration
            .GetSection(ApifyYouTubeCommentsOptions.SectionName)
            .Get<ApifyYouTubeCommentsOptions>()
            ?? new ApifyYouTubeCommentsOptions();
        var transcriptOptions = configuration
            .GetSection(ApifyYouTubeTranscriptOptions.SectionName)
            .Get<ApifyYouTubeTranscriptOptions>()
            ?? new ApifyYouTubeTranscriptOptions();
        var longestActorTimeout = Math.Max(
            Math.Clamp(commentsOptions.ActorTimeoutSeconds, 30, 270),
            Math.Clamp(transcriptOptions.ActorTimeoutSeconds, 30, 270));
        var requiredHttpTimeout = longestActorTimeout + 30;
        services.PostConfigure<ApifyOptions>(options =>
        {
            options.AllowedActorIds = (options.AllowedActorIds ?? [])
                .Append(commentsOptions.ActorId)
                .Where(actorId => !string.IsNullOrWhiteSpace(actorId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            options.TimeoutSeconds = Math.Max(options.TimeoutSeconds, requiredHttpTimeout);
        });

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IYouTubeCommentSource, ApifyYouTubeCommentSource>();
        services.AddScoped<IYouTubeContentCollectionService, YouTubeContentCollectionService>();
        return services;
    }
}
