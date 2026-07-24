using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class CommunityBoardPageViewModelTests
{
    [Fact]
    public async Task LoadAsync_ResolvesLegacyBoardNameToCanonicalBoardQuery()
    {
        CommunityBoardPostQuery? capturedQuery = null;
        var viewModel = CreateViewModel(
            loadPosts: (query, _) =>
            {
                capturedQuery = query;
                return Task.FromResult(new PlatformCommunityPostListResponse
                {
                    TotalCount = 101,
                    Page = query.Page,
                    PageSize = query.PageSize
                });
            });

        await viewModel.LoadAsync(new CommunityBoardPageRequest(
            BoardKey: null,
            LegacyBoardName: "자유·생활",
            WorkflowTag: "생활 협업",
            RoleTag: "이웃",
            Page: 0));

        Assert.NotNull(capturedQuery);
        Assert.Equal(CommunityBoardKeys.FreeLife, capturedQuery.BoardKey);
        Assert.Null(capturedQuery.LegacyCategory);
        Assert.Equal("생활 협업", capturedQuery.WorkflowTag);
        Assert.Equal("이웃", capturedQuery.RoleTag);
        Assert.Equal(1, capturedQuery.Page);
        Assert.Equal(50, capturedQuery.PageSize);
        Assert.Equal("자유·생활", viewModel.CurrentBoardName);
        Assert.Equal(3, viewModel.TotalPages);
    }

    [Fact]
    public async Task LoadAsync_PreservesUnknownLegacyCategoryForServerCompatibility()
    {
        CommunityBoardPostQuery? capturedQuery = null;
        var viewModel = CreateViewModel(
            loadPosts: (query, _) =>
            {
                capturedQuery = query;
                return Task.FromResult(new PlatformCommunityPostListResponse());
            });

        await viewModel.LoadAsync(new CommunityBoardPageRequest(
            BoardKey: null,
            LegacyBoardName: "옛 게시판",
            WorkflowTag: null,
            RoleTag: null,
            Page: 1));

        Assert.NotNull(capturedQuery);
        Assert.Null(capturedQuery.BoardKey);
        Assert.Equal("옛 게시판", capturedQuery.LegacyCategory);
        Assert.Null(viewModel.CurrentBoard);
        Assert.Equal("전체 글", viewModel.CurrentBoardName);
    }

    [Fact]
    public async Task FiltersAndSearch_OnlyChangeVisiblePosts()
    {
        var posts = new[]
        {
            Post(1, "운영 공지", "점검 안내", "운영자", isPinned: true),
            Post(2, "인기 글", "함께 읽기", "추천 이웃", isTrending: true),
            Post(3, "도움 요청", "창고 경험을 나눕니다", "현장 이웃", recommendations: 5),
            Post(4, "일상", "산책 이야기", "동네 이웃")
        };
        var viewModel = CreateViewModel(
            loadPosts: (_, _) => Task.FromResult(new PlatformCommunityPostListResponse
            {
                Items = posts,
                TotalCount = posts.Length,
                Page = 1,
                PageSize = 50
            }));
        await viewModel.LoadAsync(new CommunityBoardPageRequest(null, null, null, null, 1));

        viewModel.SelectListFilter("공지");
        Assert.Equal(1, Assert.Single(viewModel.VisiblePosts).Id);

        viewModel.SelectListFilter("추천글");
        Assert.Equal([2L, 3L], viewModel.VisiblePosts.Select(post => post.Id));

        viewModel.SelectListFilter("전체글");
        viewModel.SetSearchText("현장 이웃");
        Assert.Equal(3, Assert.Single(viewModel.VisiblePosts).Id);
    }

    [Fact]
    public async Task LoadAsync_RestoresSearchFilterAndCardViewFromRouteContext()
    {
        var viewModel = CreateViewModel(
            loadPosts: (_, _) => Task.FromResult(new PlatformCommunityPostListResponse()));

        await viewModel.LoadAsync(new CommunityBoardPageRequest(
            null,
            null,
            null,
            null,
            1,
            SearchText: "  창고 경험  ",
            ListFilter: "추천글",
            ViewMode: CommunityBoardNavigationContext.CardViewMode));

        Assert.Equal("창고 경험", viewModel.SearchText);
        Assert.Equal("추천글", viewModel.SelectedFilter);
        Assert.Equal(CommunityPostViewMode.Cards, viewModel.ViewMode);
    }

    [Fact]
    public async Task LoadAsync_WhenServerFails_KeepsCoreBoardNavigationAndErrorState()
    {
        var postsCalled = false;
        var viewModel = new CommunityBoardPageViewModel(
            _ => throw new HttpRequestException("offline"),
            (_, _) =>
            {
                postsCalled = true;
                return Task.FromResult(new PlatformCommunityPostListResponse());
            });

        await viewModel.LoadAsync(new CommunityBoardPageRequest(
            CommunityBoardKeys.Vow,
            null,
            null,
            null,
            1));

        Assert.False(viewModel.IsLoading);
        Assert.False(postsCalled);
        Assert.Equal(CommunityBoardCatalog.PublicBoards.Count, viewModel.BoardSummaries.Count);
        Assert.Equal("서원", viewModel.CurrentBoardName);
        Assert.Equal("네트워크 상태를 확인한 뒤 다시 시도해 주세요.", viewModel.ErrorMessage);
        Assert.Empty(viewModel.VisiblePosts);
    }

    [Fact]
    public async Task 업무게시판_주기성필터는_서버범위와_표시글을_함께_좁힌다()
    {
        CommunityBoardPostQuery? capturedQuery = null;
        var periodic = Post(
            10,
            "정기 가격 근거",
            "서버가 수집한 정기 자료",
            "살뜰 시스템",
            isPeriodic: true);
        var viewModel = CreateViewModel(
            loadPosts: (query, _) =>
            {
                capturedQuery = query;
                return Task.FromResult(new PlatformCommunityPostListResponse
                {
                    Items = [periodic],
                    TotalCount = 1,
                    Page = 1,
                    PageSize = 50
                });
            });

        await viewModel.LoadAsync(new CommunityBoardPageRequest(
            CommunityActivityBoardKeys.FoundationEvidence,
            null,
            null,
            null,
            1,
            ListFilter: CommunityPeriodicPostTopicCatalog.PeriodicListFilter));

        Assert.NotNull(capturedQuery);
        Assert.Equal(
            CommunityPeriodicPostVisibilityModes.Only,
            capturedQuery.PeriodicVisibility);
        Assert.True(Assert.Single(viewModel.VisiblePosts).IsPeriodic);
        Assert.Equal(
            CommunityPeriodicPostTopicCatalog.PeriodicListFilter,
            viewModel.SelectedFilter);
    }

    [Fact]
    public async Task 일반게시판은_주기성_딥링크를_전체글로_보정한다()
    {
        CommunityBoardPostQuery? capturedQuery = null;
        var viewModel = CreateViewModel(
            loadPosts: (query, _) =>
            {
                capturedQuery = query;
                return Task.FromResult(new PlatformCommunityPostListResponse());
            });

        await viewModel.LoadAsync(new CommunityBoardPageRequest(
            CommunityBoardKeys.FreeLife,
            null,
            null,
            null,
            1,
            ListFilter: CommunityPeriodicPostTopicCatalog.PeriodicListFilter));

        Assert.NotNull(capturedQuery);
        Assert.Equal(
            CommunityPeriodicPostVisibilityModes.All,
            capturedQuery.PeriodicVisibility);
        Assert.Equal("전체글", viewModel.SelectedFilter);
    }

    private static CommunityBoardPageViewModel CreateViewModel(
        Func<CommunityBoardPostQuery, CancellationToken, Task<PlatformCommunityPostListResponse>> loadPosts)
        => new(
            _ => Task.FromResult<IReadOnlyList<CommunityBoardSummaryResponse>>([]),
            loadPosts);

    private static PlatformCommunityPostResponse Post(
        long id,
        string title,
        string body,
        string nickname,
        bool isPinned = false,
        bool isTrending = false,
        int recommendations = 0,
        bool isPeriodic = false)
        => new()
        {
            Id = id,
            Title = title,
            Body = body,
            Nickname = nickname,
            IsOperatorPinned = isPinned,
            IsTrending = isTrending,
            RecommendationCount = recommendations,
            IsPeriodic = isPeriodic,
            TopicClassificationCode = isPeriodic
                ? CommunityPostTopicClassificationCodes.Periodic
                : CommunityPostTopicClassificationCodes.General,
            TopicClassificationName = CommunityPostTopicClassificationCodes.DisplayName(
                isPeriodic
                    ? CommunityPostTopicClassificationCodes.Periodic
                    : CommunityPostTopicClassificationCodes.General)
        };
}
