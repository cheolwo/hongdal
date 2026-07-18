using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Content;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class CommunityInformationReviewPageViewModelTests
{
    [Fact]
    public async Task Initialize_LoadsSourcesAndCandidatesThroughSelectedFilters()
    {
        var client = new RecordingClient(
            [Source(CommunityInformationSourceKeys.KamisPriceObservations)],
            [KamisCandidate()]);
        using var viewModel = CreateViewModel(client);
        viewModel.CountryCode = "KR";
        viewModel.SearchText = "사과";

        await viewModel.InitializeAsync();

        Assert.Single(viewModel.Sources);
        Assert.Single(viewModel.Candidates);
        Assert.Equal("KR", client.LastQuery?.CountryCode);
        Assert.Equal("사과", client.LastQuery?.SearchText);
        Assert.Equal(100, client.LastQuery?.Take);
    }

    [Fact]
    public void KamisCandidate_PreparesEditableInformationPostDraftWithSourceBoundary()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        var candidate = KamisCandidate();

        var prepared = viewModel.PrepareDraft(candidate, "운영자 홍길동");

        Assert.True(prepared);
        Assert.True(viewModel.Composer.IsOpen);
        Assert.True(viewModel.Composer.IsSettingsOpen);
        Assert.Equal(CommunityBoardCatalog.InformationPrices.DisplayName, viewModel.Composer.Draft.Category);
        Assert.Equal("운영자 정보 공유", viewModel.Composer.Draft.RoleTag);
        Assert.StartsWith("[공공자료]", viewModel.Composer.Draft.Title);
        Assert.Contains("자료 기준일: 2026-07-17", viewModel.Composer.Draft.Body);
        Assert.Contains("표시 기준: KRW · 10개", viewModel.Composer.Draft.Body);
        Assert.Contains("판매 권고", viewModel.Composer.Draft.Body);
        Assert.Equal(candidate.OriginalUrl, viewModel.Composer.Draft.SharedLinkUrl);
        Assert.Equal(string.Empty, viewModel.Composer.Draft.Password);
    }

    [Fact]
    public void ExistingDraft_IsNotReplacedUntilOperatorConfirms()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        viewModel.Composer.Draft.Title = "작성 중인 글";
        var video = VideoCandidate();

        var prepared = viewModel.PrepareDraft(video, "관리자");

        Assert.False(prepared);
        Assert.True(viewModel.HasDraftConflict);
        Assert.Equal("작성 중인 글", viewModel.Composer.Draft.Title);

        viewModel.ReplaceDraft("관리자");

        Assert.False(viewModel.HasDraftConflict);
        Assert.Equal(CommunityBoardCatalog.Food.DisplayName, viewModel.Composer.Draft.Category);
        Assert.StartsWith("[영상 공유]", viewModel.Composer.Draft.Title);
        Assert.Contains("제작자가 작성한 정보", viewModel.Composer.Draft.Body);
    }

    private static CommunityInformationReviewPageViewModel CreateViewModel(
        ICommunityInformationReviewClient client)
    {
        var composer = new CommunityPostComposerViewModel(
            new PlatformCommunityService(new HttpClient(), null!),
            new InMemoryDraftStore());
        return new CommunityInformationReviewPageViewModel(client, composer);
    }

    private static CommunityInformationSourceDto Source(string sourceKey)
        => new(
            sourceKey,
            CommunityInformationSourceTypes.PublicData,
            "provider",
            "source",
            CommunityInformationCollectionModes.ScheduledArchive,
            "daily",
            "review",
            "https://example.com/docs",
            true);

    private static CommunityInformationCandidateDto KamisCandidate()
        => new(
            "kamis:apple",
            CommunityInformationSourceKeys.KamisPriceObservations,
            CommunityInformationSourceTypes.PublicData,
            "KAMIS 농산물 유통정보",
            "사과 (후지 · 상품)",
            "소매 · 과일류 · 25,000원/10개",
            "https://www.kamis.or.kr/service/price/xml.do",
            null,
            null,
            new DateOnly(2026, 7, 17),
            new DateTime(2026, 7, 18, 1, 0, 0, DateTimeKind.Utc),
            "KR",
            "ko",
            "KRW",
            "10개",
            CommunityInformationReviewStates.OfficialObservation,
            ["농수산물", "과일류"],
            "KAMIS Open API에서 수집한 관측값입니다.",
            "전체 시장 평균이나 판매 권고가 아닙니다.");

    private static CommunityInformationCandidateDto VideoCandidate()
        => new(
            "youtube:food",
            CommunityInformationSourceKeys.YouTubeChannelVideos,
            CommunityInformationSourceTypes.Video,
            "음식 채널",
            "새로운 사과 요리",
            "사과를 활용한 공개 영상입니다.",
            "https://www.youtube.com/watch?v=food",
            "https://i.ytimg.com/vi/food/hqdefault.jpg",
            new DateTime(2026, 7, 18, 1, 0, 0, DateTimeKind.Utc),
            new DateOnly(2026, 7, 18),
            new DateTime(2026, 7, 18, 2, 0, 0, DateTimeKind.Utc),
            "KR",
            "ko",
            null,
            null,
            CommunityInformationReviewStates.PendingReview,
            ["음식", "CookingIngredient"],
            "YouTube 공개 메타데이터입니다.",
            "제목과 설명은 영상 제작자가 작성한 정보입니다.");

    private sealed class RecordingClient(
        IReadOnlyList<CommunityInformationSourceDto> sources,
        IReadOnlyList<CommunityInformationCandidateDto> candidates) : ICommunityInformationReviewClient
    {
        public CommunityInformationCollectionQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<CommunityInformationSourceDto>> GetSourcesAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(sources);

        public Task<CommunityInformationCollectionResponse> GetCandidatesAsync(
            CommunityInformationCollectionQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(new CommunityInformationCollectionResponse(
                DateTime.UtcNow,
                sources,
                candidates,
                []));
        }
    }

    private sealed class InMemoryDraftStore : ICommunityPostComposerDraftStore
    {
        public Task<CommunityPostComposerSnapshot?> LoadAsync(
            string appKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult<CommunityPostComposerSnapshot?>(null);

        public Task SaveAsync(
            string appKey,
            CommunityPostComposerSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ClearAsync(
            string appKey,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
