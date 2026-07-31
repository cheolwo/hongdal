using Ssalddel.Contracts.Common.Community;
using Ssalddel.WebApp.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class CommunityDirectoryPageViewModelTests
{
    [Fact]
    public async Task LoadAsync_주요네게시판만_ServerCount와합친다()
    {
        var freeLife = CommunityBoardCatalog.Find(CommunityBoardKeys.FreeLife)!;
        var serverBoards = new CommunityBoardSummaryResponse[]
        {
            Summary(freeLife, 12),
            new()
            {
                BoardKey = "neighbors-garden",
                DisplayName = "이웃 텃밭",
                Description = "동네 텃밭 소식",
                GroupCode = CommunityBoardGroupCodes.PeopleAndInformation,
                GroupDisplayName = "사람과 정보",
                IsCustom = true,
                PostCount = 5
            }
        };
        var viewModel = new CommunityDirectoryPageViewModel(
            _ => Task.FromResult<IReadOnlyList<CommunityBoardSummaryResponse>>(serverBoards));

        await viewModel.LoadAsync();

        var boards = viewModel.VisibleGroups.SelectMany(group => group.Boards).ToArray();
        Assert.False(viewModel.IsLoading);
        Assert.Null(viewModel.StatusMessage);
        Assert.Equal(CommunityBoardCatalog.FeaturedBoards.Count, boards.Length);
        Assert.Equal(12, viewModel.TotalPostCount);
        Assert.DoesNotContain(boards, board => board.BoardKey == "neighbors-garden");
        Assert.Equal(12, Assert.Single(boards, board => board.BoardKey == CommunityBoardKeys.FreeLife).PostCount);
    }

    [Fact]
    public async Task UpdateSearch_FiltersByBoardDescriptionAndGroupName()
    {
        var viewModel = new CommunityDirectoryPageViewModel(
            _ => Task.FromResult<IReadOnlyList<CommunityBoardSummaryResponse>>([]));
        await viewModel.LoadAsync();

        viewModel.UpdateSearch("농수산물");

        var descriptionMatches = viewModel.VisibleGroups
            .SelectMany(group => group.Boards)
            .ToArray();
        var priceBoard = Assert.Single(descriptionMatches);
        Assert.Equal(CommunityBoardKeys.InformationPrices, priceBoard.BoardKey);

        viewModel.UpdateSearch("함께하는 일");

        Assert.Empty(viewModel.VisibleGroups);
    }

    [Fact]
    public async Task UpdateSearch_숨긴업무게시판은_검색으로다시노출하지않는다()
    {
        var viewModel = new CommunityDirectoryPageViewModel(
            _ => Task.FromResult<IReadOnlyList<CommunityBoardSummaryResponse>>([]));
        await viewModel.LoadAsync();

        viewModel.UpdateSearch("운송상차완료됨Event");

        Assert.Empty(viewModel.VisibleGroups);
    }

    [Fact]
    public async Task LoadAsync_WhenServerFails_ShowsCoreBoardsAndRecoverableStatus()
    {
        var viewModel = new CommunityDirectoryPageViewModel(
            _ => throw new HttpRequestException("offline"));

        await viewModel.LoadAsync();

        Assert.False(viewModel.IsLoading);
        Assert.Equal(
            "게시글 수를 불러오지 못했습니다. 기본 게시판을 먼저 표시합니다.",
            viewModel.StatusMessage);
        Assert.Equal(CommunityBoardCatalog.FeaturedBoards.Count, viewModel.VisibleBoardCount);
        Assert.Equal(0, viewModel.TotalPostCount);
    }

    private static CommunityBoardSummaryResponse Summary(
        CommunityBoardDefinition board,
        int postCount)
        => new()
        {
            BoardKey = board.Key,
            DisplayName = board.DisplayName,
            Description = board.Description,
            GroupCode = board.GroupCode,
            GroupDisplayName = board.GroupDisplayName,
            IsUserCreatable = board.IsUserCreatable,
            PostingAccessCode = board.PostingAccessCode,
            PostingAccessDisplayName = board.PostingAccessDisplayName,
            AllowsAnonymousPosting = board.AllowsAnonymousPosting,
            PostCount = postCount
        };
}
