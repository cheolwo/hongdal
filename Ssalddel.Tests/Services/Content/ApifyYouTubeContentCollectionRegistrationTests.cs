using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Extensions;
using Ssalddel.Services.Content;
using Ssalddel.Services.External.YouTube;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Content;

public sealed class ApifyYouTubeContentCollectionRegistrationTests
{
    [Fact]
    public void AddApifyYouTubeContentCollection_두Actor와통합Service를등록한다()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Apify:Enabled"] = "true",
                ["Apify:ApiToken"] = "test-token",
                ["ApifyYouTubeTranscript:ActorId"] = "custom~transcript",
                ["ApifyYouTubeComments:ActorId"] = "custom~comments"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IYouTubeSocialContextVideoSource, EmptyVideoSource>();

        services.AddApifyYouTubeContentCollection(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApifyOptions>>().Value;
        Assert.Contains("custom~transcript", options.AllowedActorIds);
        Assert.Contains("custom~comments", options.AllowedActorIds);
        Assert.Equal(210, options.TimeoutSeconds);
        Assert.NotNull(provider.GetRequiredService<IYouTubeTranscriptSource>());
        Assert.NotNull(provider.GetRequiredService<IYouTubeCommentSource>());
        Assert.NotNull(provider.GetRequiredService<IYouTubeContentCollectionService>());
    }

    private sealed class EmptyVideoSource : IYouTubeSocialContextVideoSource
    {
        public Task<YouTubeSocialContextVideoDto?> GetAsync(
            string videoId,
            CancellationToken cancellationToken)
            => Task.FromResult<YouTubeSocialContextVideoDto?>(null);
    }
}
