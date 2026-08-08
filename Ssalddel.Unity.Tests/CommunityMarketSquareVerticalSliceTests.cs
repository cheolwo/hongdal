using Ssalddel.Unity.Community;

namespace Ssalddel.Tests.UnityData;

public sealed class CommunityMarketSquareVerticalSliceTests
{
    [Fact]
    public async Task 공개광장Api는_Repository와UseCase를통해_WorldItem으로_투영된다()
    {
        var client = new SequenceClient(Snapshot("revision-1"));
        var useCase = UseCase(client);

        var result = await useCase.실행Async();

        Assert.Equal("community-market-square:public", result.StableId);
        Assert.Equal(4, result.Items.Length);
        Assert.Contains(result.Items, item => item.Kind == "Board" && item.StableId == "community-board:sales-supply");
        Assert.Contains(result.Items, item => item.Kind == "Post" && item.DetailHref == "/community/posts/101");
        Assert.Equal("api/v1/community/world/zones/community-market-square", CommunityMarketSquareApiRoutes.PublicSnapshot);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public void Mapper는_중복StableId를_잘못된Snapshot으로_거부한다()
    {
        var source = Snapshot("revision-1");
        source.Posts = new[] { source.Posts[0], source.Posts[0] };

        var exception = Assert.Throws<InvalidOperationException>(() => new CommunityMarketSquareMapper().Map(source));

        Assert.Equal("DuplicateCommunitySquarePost:community-post:101", exception.Message);
    }

    [Fact]
    public void Mapper는_원장요약이_공개게시글을_참조하도록_제한한다()
    {
        var source = Snapshot("revision-1");
        source.Ledgers[0].SourcePostStableId = "community-post:private";

        var exception = Assert.Throws<InvalidOperationException>(() => new CommunityMarketSquareMapper().Map(source));

        Assert.Equal("CommunitySquareLedgerPostUnknown:community-ledger-summary:post-101", exception.Message);
    }

    [Fact]
    public async Task 최초실패는_빈상태이고_갱신실패는_마지막성공Snapshot을_유지한다()
    {
        var initial = new CommunityMarketSquareLoadCoordinator(
            UseCase(new SequenceClient(new InvalidOperationException("offline"))),
            new CommunityMarketSquareReconciler());
        var initialFailure = await initial.LoadAsync();

        var successful = Snapshot("revision-1");
        var coordinator = new CommunityMarketSquareLoadCoordinator(
            UseCase(new SequenceClient(successful, new InvalidOperationException("refresh-offline"))),
            new CommunityMarketSquareReconciler());
        var success = await coordinator.LoadAsync();
        var refreshFailure = await coordinator.LoadAsync();

        Assert.Equal(CommunityMarketSquareLoadStateCodes.InitialLoadError, initialFailure.StateCode);
        Assert.Null(initialFailure.Snapshot);
        Assert.Equal(CommunityMarketSquareLoadStateCodes.Success, success.StateCode);
        Assert.Equal(CommunityMarketSquareLoadStateCodes.RefreshError, refreshFailure.StateCode);
        Assert.Same(success.Snapshot, refreshFailure.Snapshot);
    }

    [Fact]
    public async Task 성공갱신은_StableId기준으로_추가갱신제거를_계산한다()
    {
        var first = Snapshot("revision-1");
        var second = Snapshot("revision-2");
        second.Boards = Array.Empty<CommunitySquareBoardApiModel>();
        second.Posts[0].Title = "갱신된 공동 수요";
        second.ActivitySignals = second.ActivitySignals.Append(new CommunitySquareActivityApiModel
        {
            StableId = "community-activity:signal-2",
            CommunityScope = "CommunityTrust",
            ActivityKind = "Participation",
            Title = "새 참여 신호",
            Summary = "비식별 집계",
            AggregationCount = 2,
            OccurredAtUtc = DateTimeOffset.Parse("2026-08-08T02:00:00Z"),
        }).ToArray();
        var coordinator = new CommunityMarketSquareLoadCoordinator(
            UseCase(new SequenceClient(first, second)), new CommunityMarketSquareReconciler());

        await coordinator.LoadAsync();
        var refreshed = await coordinator.LoadAsync();

        Assert.Single(refreshed.Changes!.Added);
        Assert.Single(refreshed.Changes.Updated);
        Assert.Single(refreshed.Changes.Removed);
        Assert.Equal(2, refreshed.Changes.Unchanged.Length);
    }

    private static CommunityMarketSquareQueryUseCase UseCase(ICommunityMarketSquareApiClient client)
        => new(new CommunityMarketSquareApiRepository(client, new CommunityMarketSquareMapper()));

    private static CommunityMarketSquareSnapshotApiModel Snapshot(string revision)
        => new()
        {
            StableId = "community-market-square:public",
            Revision = revision,
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            Boards = new[]
            {
                new CommunitySquareBoardApiModel
                {
                    StableId = "community-board:sales-supply", BoardKey = "sales-supply",
                    DisplayName = "판매·공급", Description = "공개 이야기", PostingAccessCode = "authenticated", PostCount = 1,
                },
            },
            Posts = new[]
            {
                new CommunitySquarePostApiModel
                {
                    StableId = "community-post:101", PostId = 101, Category = "판매·공급",
                    Title = "감자 공동 수요", Excerpt = "공개 요약", PublishedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                    DetailHref = "/community/posts/101", CommentCount = 2,
                },
            },
            ActivitySignals = new[]
            {
                new CommunitySquareActivityApiModel
                {
                    StableId = "community-activity:signal-1", CommunityScope = "CommunityTrust",
                    ActivityKind = "InterestGathering", Title = "수요가 모이고 있습니다", Summary = "비식별 집계",
                    AggregationCount = 4, OccurredAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            Ledgers = new[]
            {
                new CommunitySquareLedgerApiModel
                {
                    StableId = "community-ledger-summary:post-101", SourcePostStableId = "community-post:101",
                    TemplateName = "공동행동 준비", Title = "감자 수요 확인", State = "관심모집", CurrentStage = "수요확인",
                    DetailAvailable = true, DetailHref = "/community/posts/101",
                },
            },
        };

    private sealed class SequenceClient : ICommunityMarketSquareApiClient
    {
        private readonly Queue<object> responses;
        public SequenceClient(params object[] responses) => this.responses = new Queue<object>(responses);
        public int CallCount { get; private set; }
        public Task<CommunityMarketSquareSnapshotApiModel> GetPublicSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            var response = responses.Dequeue();
            return response is Exception exception
                ? Task.FromException<CommunityMarketSquareSnapshotApiModel>(exception)
                : Task.FromResult((CommunityMarketSquareSnapshotApiModel)response);
        }
    }
}
