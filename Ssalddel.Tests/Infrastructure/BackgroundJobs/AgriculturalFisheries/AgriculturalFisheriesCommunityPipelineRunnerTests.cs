using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;
using Ssalddel.Infrastructure.BackgroundJobs.Community;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Ssalddel.Services.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Infrastructure.BackgroundJobs.AgriculturalFisheries;

public sealed class AgriculturalFisheriesCommunityPipelineRunnerTests
{
    [Fact]
    public async Task KamisDaily_CollectsBeforePublishingVerifiedBrief()
    {
        var operations = new List<string>();
        var pipeline = CreatePipeline(
            CommunityAutomatedPostSourceKeys.KamisPriceBrief,
            operations,
            publishCommunityPriceBriefs: true);

        await pipeline.RunKamisDailyAsync(CancellationToken.None);

        Assert.Equal(
            new[] { "collect-kamis-daily", "build-kamis-price-brief", "publish-kamis-price-brief" },
            operations);
    }

    [Fact]
    public async Task UsdaMonthly_CollectsBeforePublishingVerifiedBrief()
    {
        var operations = new List<string>();
        var pipeline = CreatePipeline(
            CommunityAutomatedPostSourceKeys.UsdaNassPriceBrief,
            operations,
            publishCommunityPriceBriefs: true);

        await pipeline.RunUsdaMonthlyAsync(CancellationToken.None);

        Assert.Equal(
            new[]
            {
                "collect-usda-monthly",
                "build-usda-nass-price-brief",
                "publish-usda-nass-price-brief"
            },
            operations);
    }

    [Fact]
    public async Task PublicationDisabled_StillCollectsButDoesNotWritePost()
    {
        var operations = new List<string>();
        var pipeline = CreatePipeline(
            CommunityAutomatedPostSourceKeys.KamisPriceBrief,
            operations,
            publishCommunityPriceBriefs: false);

        await pipeline.RunKamisDailyAsync(CancellationToken.None);

        Assert.Equal(new[] { "collect-kamis-daily" }, operations);
    }

    private static AgriculturalFisheriesCommunityPipelineRunner CreatePipeline(
        string sourceKey,
        List<string> operations,
        bool publishCommunityPriceBriefs)
    {
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero));
        var batchOptions = new AgriculturalFisheriesBatchOptions
        {
            TimeZoneId = "Asia/Seoul",
            PublishCommunityPriceBriefs = publishCommunityPriceBriefs
        };
        var editorialOptions = new CommunityEditorialBatchOptions
        {
            TimeZoneId = "Asia/Seoul",
            KamisPriceBriefEnabled = true,
            UsdaNassPriceBriefEnabled = true
        };
        var collectionRunner = new AgriculturalFisheriesBatchRunner(
            new RecordingKamisArchiveService(operations),
            new RecordingUsdaArchiveService(operations),
            Options.Create(batchOptions),
            timeProvider,
            NullLogger<AgriculturalFisheriesBatchRunner>.Instance);
        var editorialRunner = new CommunityEditorialBatchRunner(
            [new RecordingSource(sourceKey, operations)],
            new RecordingPublisher(operations),
            Options.Create(editorialOptions),
            timeProvider,
            NullLogger<CommunityEditorialBatchRunner>.Instance);
        return new AgriculturalFisheriesCommunityPipelineRunner(
            collectionRunner,
            editorialRunner,
            Options.Create(batchOptions),
            Options.Create(editorialOptions));
    }

    private sealed class RecordingKamisArchiveService(List<string> operations)
        : IKamisPriceArchiveService
    {
        public Task<KamisPriceArchiveResult> CollectDailyPricesAsync(
            DateOnly requestedDate,
            CancellationToken cancellationToken = default)
        {
            operations.Add("collect-kamis-daily");
            return Task.FromResult(new KamisPriceArchiveResult(1, 1, 1, 0, 0, requestedDate));
        }

        public Task<KamisPriceArchiveResult> CollectPeriodPricesAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new KamisPriceArchiveResult(1, 0, 0, 0, 0, endDate));

        public Task<KamisPriceArchiveResult> CollectMonthlyPricesAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new KamisPriceArchiveResult(1, 0, 0, 0, 0, endDate));
    }

    private sealed class RecordingUsdaArchiveService(List<string> operations)
        : IUsdaNassPriceArchiveService
    {
        public Task<UsdaNassPriceArchiveResult> CollectRecentMonthlyPricesAsync(
            int yearFrom,
            CancellationToken cancellationToken = default)
        {
            operations.Add("collect-usda-monthly");
            return Task.FromResult(new UsdaNassPriceArchiveResult(1, 1, 1, 0, 0, null));
        }
    }

    private sealed class RecordingSource(string sourceKey, List<string> operations)
        : ICommunityAutomatedPostSource
    {
        public string SourceKey { get; } = sourceKey;

        public Task<CommunityAutomatedPostDraft?> BuildAsync(
            DateOnly publicationDate,
            TimeZoneInfo timeZone,
            CancellationToken cancellationToken = default)
        {
            operations.Add($"build-{SourceKey}");
            return Task.FromResult<CommunityAutomatedPostDraft?>(new CommunityAutomatedPostDraft(
                SourceKey,
                publicationDate.ToString("yyyyMMdd"),
                "정보·시세",
                "공공가격",
                "자동 정보",
                "자동 가격정보",
                "검증된 공공가격",
                "살뜰 정보봇"));
        }
    }

    private sealed class RecordingPublisher(List<string> operations)
        : ICommunityAutomatedPostPublisher
    {
        public Task<CommunityAutomatedPostPublishResult> PublishIfMissingAsync(
            CommunityAutomatedPostDraft draft,
            CancellationToken cancellationToken = default)
        {
            operations.Add($"publish-{draft.SourceKey}");
            return Task.FromResult(new CommunityAutomatedPostPublishResult(1, true));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
