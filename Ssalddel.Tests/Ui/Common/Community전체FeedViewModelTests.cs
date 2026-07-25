using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class Community전체FeedViewModelTests
{
    [Fact]
    public async Task 전체피드는_서버순서를유지하며다음page를중복없이붙인다()
    {
        var requestedPages = new List<int>();
        var viewModel = new Community전체FeedViewModel((page, pageSize, _) =>
        {
            requestedPages.Add(page);
            Assert.Equal(Community전체FeedViewModel.PageSize, pageSize);
            return Task.FromResult(new PlatformCommunityPostListResponse
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = 3,
                Items = page switch
                {
                    1 => [Post(3), Post(2)],
                    2 => [Post(2), Post(1)],
                    _ => []
                }
            });
        });

        await viewModel.LoadAsync();
        Assert.Equal([3L, 2L], viewModel.Items.Select(item => item.Id));
        Assert.True(viewModel.HasMore);

        await viewModel.LoadMoreAsync();

        Assert.Equal([1, 2], requestedPages);
        Assert.Equal([3L, 2L, 1L], viewModel.Items.Select(item => item.Id));
        Assert.False(viewModel.HasMore);
    }

    [Fact]
    public async Task 다음page오류는_이미읽은글을유지한다()
    {
        var viewModel = new Community전체FeedViewModel((page, _, _) =>
            page == 1
                ? Task.FromResult(new PlatformCommunityPostListResponse
                {
                    Page = 1,
                    PageSize = Community전체FeedViewModel.PageSize,
                    TotalCount = 2,
                    Items = [Post(2)]
                })
                : throw new HttpRequestException("network"));

        await viewModel.LoadAsync();
        await viewModel.LoadMoreAsync();

        Assert.Equal([2L], viewModel.Items.Select(item => item.Id));
        Assert.Contains("이미 불러온 글", viewModel.ErrorMessage);
        Assert.True(viewModel.HasMore);
    }

    [Fact]
    public async Task 첫조회오류는_빈상태와재시도문구를제공한다()
    {
        var viewModel = new Community전체FeedViewModel((_, _, _) =>
            throw new HttpRequestException("network"));

        await viewModel.LoadAsync();

        Assert.Empty(viewModel.Items);
        Assert.False(viewModel.IsInitialLoading);
        Assert.False(viewModel.HasMore);
        Assert.Contains("게시판 보기는 계속", viewModel.ErrorMessage);
    }

    private static PlatformCommunityPostResponse Post(long id)
        => new()
        {
            Id = id,
            Title = $"글 {id}",
            Category = PlatformCommunityPostCategories.General,
            CreatedAtUtc = DateTime.UtcNow
        };
}
