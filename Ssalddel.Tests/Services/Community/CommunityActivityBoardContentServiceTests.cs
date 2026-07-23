using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityActivityBoardContentServiceTests
{
    [Fact]
    public async Task EnsureAnnouncementsAsync_CreatesOnePinnedNoticePerActivityBoard()
    {
        var publisher = new RecordingPublisher();
        var service = new CommunityActivityBoardContentService(
            publisher,
            new FixedTimeProvider());

        var first = await service.EnsureAnnouncementsAsync();
        var second = await service.EnsureAnnouncementsAsync();

        Assert.Equal(CommunityActivityBoardCatalog.All.Count, first.AttemptedCount);
        Assert.Equal(CommunityActivityBoardCatalog.All.Count, first.CreatedCount);
        Assert.Equal(CommunityActivityBoardCatalog.All.Count, second.AttemptedCount);
        Assert.Equal(0, second.CreatedCount);
        Assert.All(
            publisher.Drafts.Take(CommunityActivityBoardCatalog.All.Count),
            draft =>
            {
                Assert.StartsWith("[게시판 안내]", draft.Title);
                Assert.True(draft.IsOperatorPinned);
                Assert.False(draft.EnqueueDerivedWork);
                Assert.False(draft.PublishCreatedEvent);
            });

        foreach (var definition in CommunityActivityBoardCatalog.All)
        {
            var draft = Assert.Single(
                publisher.Drafts.Take(CommunityActivityBoardCatalog.All.Count),
                item => item.Category == definition.Board.DisplayName);
            Assert.Contains(definition.SourceName, draft.Body);
            Assert.Contains(
                CommunityActivityBoardCatalog.SurfaceMappingBoundary,
                draft.Body);
        }
    }

    [Fact]
    public async Task SeedTestActivityPostsAsync_CreatesClearlyMarkedIdempotentTestRows()
    {
        var publisher = new RecordingPublisher();
        var service = new CommunityActivityBoardContentService(
            publisher,
            new FixedTimeProvider());
        var expectedCount = CommunityActivityBoardCatalog.All.Count * 2;

        var first = await service.SeedTestActivityPostsAsync(
            "Observation Scenario #1",
            postsPerBoard: 2);
        var second = await service.SeedTestActivityPostsAsync(
            "Observation Scenario #1",
            postsPerBoard: 2);

        Assert.Equal(expectedCount, first.AttemptedCount);
        Assert.Equal(expectedCount, first.CreatedCount);
        Assert.Equal(0, second.CreatedCount);
        Assert.All(
            publisher.Drafts.Take(expectedCount),
            draft =>
            {
                Assert.StartsWith("[테스트 데이터]", draft.Title);
                Assert.Contains("실제 주문, 계약, 결제", draft.Body);
                Assert.Contains(
                    CommunityActivityBoardCatalog.SurfaceMappingBoundary,
                    draft.Body);
                Assert.Contains("observationscenario1", draft.PeriodKey);
                Assert.False(draft.IsOperatorPinned);
                Assert.False(draft.EnqueueDerivedWork);
                Assert.False(draft.PublishCreatedEvent);
            });
    }

    [Theory]
    [InlineData("Development", true, true)]
    [InlineData("Testing", true, true)]
    [InlineData("Production", true, false)]
    [InlineData("Development", false, false)]
    public void TestSeedPolicy_AllowsOnlyExplicitDevelopmentOrTesting(
        string environmentName,
        bool enabled,
        bool expected)
        => Assert.Equal(
            expected,
            CommunityActivityBoardContentWorker.CanSeedTestActivityPosts(
                environmentName,
                enabled));

    private sealed class RecordingPublisher : ICommunityAutomatedPostPublisher
    {
        private readonly HashSet<string> _publishedKeys = new(StringComparer.Ordinal);

        public List<CommunityAutomatedPostDraft> Drafts { get; } = [];

        public Task<CommunityAutomatedPostPublishResult> PublishIfMissingAsync(
            CommunityAutomatedPostDraft draft,
            CancellationToken cancellationToken = default)
        {
            Drafts.Add(draft);
            var created = _publishedKeys.Add(draft.SystemAuthorKey);
            return Task.FromResult(new CommunityAutomatedPostPublishResult(
                _publishedKeys.Count,
                created));
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(2026, 7, 23, 12, 30, 0, TimeSpan.Zero);
    }
}
