using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Community;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도뉴스후보UseCaseTests
{
    [Fact]
    public async Task SourceKey없이는_외부후보조회를하지않고_언론사Feed상태만반환한다()
    {
        var collection = new FakeCollectionService();
        var useCase = CreateUseCase(collection);

        var response = await useCase.조회Async(
            "news-publisher:kr-yonhap",
            null,
            8);

        Assert.NotNull(response);
        Assert.Equal("연합뉴스", response.PublisherName);
        Assert.Equal(
            커뮤니티세계지도뉴스Feed상태Codes.OfficialPublicFeedUnverified,
            response.PublisherFeedStatus.Code);
        Assert.Equal(3, response.RelatedOfficialSources.Count);
        Assert.Null(response.SelectedSourceKey);
        Assert.Empty(response.Items);
        Assert.False(response.CreatesPost);
        Assert.Equal(0, collection.ReadCount);
        Assert.Contains("연합뉴스 기사가 아니라", response.RelationNotice);
    }

    [Fact]
    public async Task 명시선택한Source는_Rss를재호출하지않고_검토원장의승인후보만조회한다()
    {
        var collection = new FakeCollectionService();
        var store = new InMemory공식뉴스검토원장Store();
        var ledger = new 공식뉴스검토원장Service(collection, store, TimeProvider.System);
        await ledger.결정기록Async(
            "candidate-1",
            new 공식뉴스검토결정Request
            {
                SourceKey = CommunityInformationSourceKeys.MfdsPressReleases,
                DecisionCode = CommunityInformationReviewStates.Approved,
                IdempotencyKey = "approve-1"
            },
            "admin-1",
            "운영자");
        var useCase = new 커뮤니티세계지도뉴스후보UseCase(
            collection,
            ledger,
            TimeProvider.System);

        var response = await useCase.조회Async(
            "news-publisher:kr-yonhap",
            CommunityInformationSourceKeys.MfdsPressReleases,
            99);

        Assert.NotNull(response);
        Assert.Equal(1, collection.ReadCount);
        Assert.Equal(CommunityInformationSourceKeys.MfdsPressReleases, collection.LastQuery?.SourceKey);
        Assert.Equal(CommunityInformationReviewStates.PendingReview, collection.LastQuery?.ReviewState);
        Assert.Equal(100, collection.LastQuery?.Take);
        Assert.Single(response.Items);
        Assert.Equal(CommunityInformationReviewStates.Approved, response.Items[0].ReviewState);
        Assert.False(response.CreatesPost);
    }

    [Fact]
    public async Task 마커에연결되지않은SourceKey는_외부조회전에거부한다()
    {
        var collection = new FakeCollectionService();
        var useCase = CreateUseCase(collection);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => useCase.조회Async(
            "news-publisher:kr-yonhap",
            CommunityInformationSourceKeys.UsdaNassPriceObservations,
            8));

        Assert.Contains("연결된 공식뉴스 RSS", exception.Message);
        Assert.Equal(0, collection.ReadCount);
    }

    [Theory]
    [InlineData("news-publisher:us-associated-press", 커뮤니티세계지도뉴스Feed상태Codes.LicensedApiOnly)]
    [InlineData("news-publisher:au-abc-news", 커뮤니티세계지도뉴스Feed상태Codes.Discontinued)]
    [InlineData("news-publisher:cn-xinhua", 커뮤니티세계지도뉴스Feed상태Codes.OfficialPublicFeedUnverified)]
    public async Task 국가별언론사Feed상태를_수집성공으로가장하지않는다(
        string stableId,
        string expectedStatus)
    {
        var useCase = new 커뮤니티세계지도뉴스후보UseCase(
            new FakeCollectionService(),
            new 공식뉴스검토원장Service(
                new FakeCollectionService(),
                new InMemory공식뉴스검토원장Store(),
                TimeProvider.System),
            TimeProvider.System);

        var response = await useCase.조회Async(stableId, null, 8);

        Assert.NotNull(response);
        Assert.Equal(expectedStatus, response.PublisherFeedStatus.Code);
        Assert.Empty(response.RelatedOfficialSources);
        Assert.Empty(response.Items);
    }

    private static 커뮤니티세계지도뉴스후보UseCase CreateUseCase(
        FakeCollectionService collection)
        => new(
            collection,
            new 공식뉴스검토원장Service(
                collection,
                new InMemory공식뉴스검토원장Store(),
                TimeProvider.System),
            TimeProvider.System);

    private sealed class FakeCollectionService : ICommunityInformationCollectionService
    {
        public int ReadCount { get; private set; }

        public CommunityInformationCollectionQuery? LastQuery { get; private set; }

        public IReadOnlyList<CommunityInformationSourceDto> GetSources()
            =>
            [
                Source(CommunityInformationSourceKeys.MafraPressReleases, "농림축산식품부 보도자료"),
                Source(CommunityInformationSourceKeys.MafraExplanations, "농림축산식품부 설명자료"),
                Source(CommunityInformationSourceKeys.MfdsPressReleases, "식품의약품안전처 보도자료"),
                Source(CommunityInformationSourceKeys.UsdaNassPriceObservations, "USDA 가격")
            ];

        public Task<CommunityInformationCollectionResponse> ReadAsync(
            CommunityInformationCollectionQuery query,
            CancellationToken cancellationToken = default)
        {
            ReadCount++;
            LastQuery = query;
            var item = new CommunityInformationCandidateDto(
                "candidate-1",
                query.SourceKey!,
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
                new DateTime(2026, 8, 4, 1, 0, 0, DateTimeKind.Utc),
                GetSources(),
                [item],
                []));
        }

        private static CommunityInformationSourceDto Source(string key, string displayName)
            => new(
                key,
                CommunityInformationSourceTypes.OfficialNews,
                displayName,
                displayName,
                CommunityInformationCollectionModes.OnDemandOfficialNewsQuery,
                "명시 조회",
                "자동 게시 없음",
                "https://example.test/source",
                true);
    }
}
