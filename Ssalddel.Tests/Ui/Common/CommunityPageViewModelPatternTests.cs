using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class CommunityPageViewModelPatternTests
{
    [Fact]
    public async Task 전체피드는_PageViewModel수명주기로_초기화된다()
    {
        var viewModel = new Community전체FeedViewModel((_, pageSize, _) =>
            Task.FromResult(new PlatformCommunityPostListResponse
            {
                Page = 1,
                PageSize = pageSize,
                TotalCount = 1,
                Items =
                [
                    new PlatformCommunityPostResponse
                    {
                        Id = 1,
                        Title = "공개 글",
                        Category = PlatformCommunityPostCategories.General
                    }
                ]
            }));

        var initialized = await viewModel.초기화Async();

        Assert.True(initialized);
        Assert.True(viewModel.초기화됨);
        Assert.Equal(PageViewModel상태.준비됨, viewModel.상태);
        Assert.Single(viewModel.Items);
    }

    [Fact]
    public async Task 게시판디렉터리는_화면모드와검색을_ViewModel이관리한다()
    {
        var viewModel = new CommunityMobileBoardDirectoryViewModel(_ =>
            Task.FromResult<IReadOnlyList<CommunityBoardSummaryResponse>>([]));
        viewModel.Configure("shipper", initialWorkMode: false);

        await viewModel.초기화Async();
        viewModel.ToggleMode();
        viewModel.UpdateSearch("가격");

        Assert.True(viewModel.WorkMode);
        Assert.Equal("가격", viewModel.SearchText);
        Assert.True(viewModel.초기화됨);
    }

    [Fact]
    public async Task 지역문화목록은_국가선택을_PageViewModel이관리한다()
    {
        var viewModel = new 지역문화특산물목록PageViewModel();
        await viewModel.초기화Async();

        viewModel.SelectCountry(RegionalCultureSpecialtyCatalog.UnitedStatesCountryCode);

        Assert.True(viewModel.초기화됨);
        Assert.NotEmpty(viewModel.VisibleRegions);
        Assert.All(
            viewModel.VisibleRegions,
            region => Assert.Equal(
                RegionalCultureSpecialtyCatalog.UnitedStatesCountryCode,
                region.CountryCode,
                ignoreCase: true));
    }

    [Fact]
    public async Task 지역문화상세는_routeKey를_현재지역문맥으로해석한다()
    {
        var viewModel = new 지역문화특산물상세PageViewModel();
        viewModel.Configure("cn-shandong");

        await viewModel.초기화Async();

        Assert.Equal("산둥성", viewModel.Region?.RegionName);
        Assert.Contains("cn-shandong", viewModel.CultureConversationHref);
    }
}
