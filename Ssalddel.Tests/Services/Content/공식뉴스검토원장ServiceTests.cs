using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 공식뉴스검토원장ServiceTests
{
    [Fact]
    public async Task 승인결정은_후보Snapshot과검토이력을_원장에저장한다()
    {
        var store = new InMemory공식뉴스검토원장Store();
        var service = CreateService(store);

        var result = await service.결정기록Async(
            "candidate-1",
            Request(CommunityInformationReviewStates.Approved, "decision-1"),
            "admin-1",
            "운영자");
        var reloaded = await store.조회Async("candidate-1");

        Assert.Equal(CommunityInformationReviewStates.Approved, result.ReviewState);
        Assert.Equal(CommunityInformationReviewStates.Approved, result.Candidate.ReviewState);
        Assert.Equal(1, result.Revision);
        Assert.Single(result.History);
        Assert.NotNull(reloaded);
        Assert.Equal(CommunityInformationReviewStates.Approved, reloaded.ReviewState);
        Assert.DoesNotContain("admin-1", result.History.Select(item => item.ReviewerDisplayName));
        Assert.Contains("admin-1", reloaded.History.Select(item => item.ReviewerId));
    }

    [Fact]
    public async Task 같은IdempotencyKey재시도는_revision과이력을늘리지않는다()
    {
        var service = CreateService(new InMemory공식뉴스검토원장Store());
        var request = Request(CommunityInformationReviewStates.Approved, "decision-1");

        var first = await service.결정기록Async(
            "candidate-1", request, "admin-1", "운영자");
        var second = await service.결정기록Async(
            "candidate-1", request, "admin-1", "운영자");

        Assert.Equal(first.Revision, second.Revision);
        Assert.Single(second.History);
    }

    [Fact]
    public async Task 제외전이는_expectedRevision과함께_감사이력을추가한다()
    {
        var service = CreateService(new InMemory공식뉴스검토원장Store());
        var approved = await service.결정기록Async(
            "candidate-1",
            Request(CommunityInformationReviewStates.Approved, "decision-1"),
            "admin-1",
            "운영자");
        var exclude = Request(CommunityInformationReviewStates.Excluded, "decision-2");
        exclude.ExpectedRevision = approved.Revision;

        var excluded = await service.결정기록Async(
            "candidate-1",
            exclude,
            "admin-2",
            "검토자");

        Assert.Equal(2, excluded.Revision);
        Assert.Equal(CommunityInformationReviewStates.Excluded, excluded.ReviewState);
        Assert.Equal(2, excluded.History.Count);
        Assert.Equal("검토자", excluded.History[^1].ReviewerDisplayName);
    }

    [Fact]
    public async Task 오래된Revision의결정은_후보재조회전에충돌로거부한다()
    {
        var source = new FakeCollectionService();
        var service = new 공식뉴스검토원장Service(
            source,
            new InMemory공식뉴스검토원장Store(),
            TimeProvider.System);
        await service.결정기록Async(
            "candidate-1",
            Request(CommunityInformationReviewStates.Approved, "decision-1"),
            "admin-1",
            "운영자");
        var stale = Request(CommunityInformationReviewStates.Excluded, "decision-2");
        stale.ExpectedRevision = 0;

        await Assert.ThrowsAsync<공식뉴스검토원장ConcurrencyException>(() =>
            service.결정기록Async("candidate-1", stale, "admin-2", "검토자"));

        Assert.Equal(1, source.ReadCount);
    }

    [Fact]
    public async Task 승인목록은_제외된후보를공개조회에서분리한다()
    {
        var service = CreateService(new InMemory공식뉴스검토원장Store());
        var approved = await service.결정기록Async(
            "candidate-1",
            Request(CommunityInformationReviewStates.Approved, "decision-1"),
            "admin-1",
            "운영자");
        var exclude = Request(CommunityInformationReviewStates.Excluded, "decision-2");
        exclude.ExpectedRevision = approved.Revision;
        await service.결정기록Async(
            "candidate-1",
            exclude,
            "admin-1",
            "운영자");

        var publicItems = await service.목록Async(
            [CommunityInformationSourceKeys.MfdsPressReleases],
            CommunityInformationReviewStates.Approved,
            20);

        Assert.Empty(publicItems);
    }

    private static 공식뉴스검토원장Service CreateService(I공식뉴스검토원장Store store)
        => new(new FakeCollectionService(), store, TimeProvider.System);

    private static 공식뉴스검토결정Request Request(string decisionCode, string key)
        => new()
        {
            SourceKey = CommunityInformationSourceKeys.MfdsPressReleases,
            DecisionCode = decisionCode,
            DecisionNote = "공식 원문과 제목을 확인했습니다.",
            IdempotencyKey = key
        };

    private sealed class FakeCollectionService : ICommunityInformationCollectionService
    {
        public int ReadCount { get; private set; }

        public IReadOnlyList<CommunityInformationSourceDto> GetSources()
            =>
            [
                new(
                    CommunityInformationSourceKeys.MfdsPressReleases,
                    CommunityInformationSourceTypes.OfficialNews,
                    "식품의약품안전처",
                    "식품의약품안전처 보도자료",
                    CommunityInformationCollectionModes.OnDemandOfficialNewsQuery,
                    "명시 조회",
                    "검토 후 공개",
                    "https://www.mfds.go.kr/www/rss/list.do",
                    true)
            ];

        public Task<CommunityInformationCollectionResponse> ReadAsync(
            CommunityInformationCollectionQuery query,
            CancellationToken cancellationToken = default)
        {
            ReadCount++;
            var candidate = new CommunityInformationCandidateDto(
                "candidate-1",
                CommunityInformationSourceKeys.MfdsPressReleases,
                CommunityInformationSourceTypes.OfficialNews,
                "식품의약품안전처",
                "식품 안전 공식자료",
                "검토 전 요약",
                "https://www.mfds.go.kr/article/1",
                null,
                new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
                new DateOnly(2026, 8, 4),
                new DateTime(2026, 8, 4, 1, 0, 0, DateTimeKind.Utc),
                "KR",
                "ko",
                null,
                null,
                CommunityInformationReviewStates.PendingReview,
                ["공식뉴스"],
                "공식 RSS 제목·요약",
                "운영자 검토 전");
            return Task.FromResult(new CommunityInformationCollectionResponse(
                DateTime.UtcNow,
                GetSources(),
                [candidate],
                []));
        }
    }
}
