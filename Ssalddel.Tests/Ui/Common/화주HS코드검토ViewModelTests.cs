using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 화주HS코드검토ViewModelTests
{
    [Fact]
    public async Task 접근상태는_기능활성후로그인여부를별도로판정한다()
    {
        var anonymous = new 화주HS코드검토접근ViewModel(
            new StubAccessService(true),
            new StubCurrentUserContext(현재사용자Snapshot.익명));
        var authenticated = new 화주HS코드검토접근ViewModel(
            new StubAccessService(false),
            new StubCurrentUserContext(new 현재사용자Snapshot("shipper-1", "화주", ["화주"])));

        Assert.True(await anonymous.확인Async());
        Assert.True(anonymous.로그인필요);
        Assert.True(await authenticated.확인Async());
        Assert.True(authenticated.기능비활성);
    }

    [Fact]
    public async Task 목록ViewModel은_입력조건과페이지를그대로Client에전달한다()
    {
        var client = new StubReviewClient
        {
            ListResponse = new 화주HS코드검토목록응답
            {
                Items = [new 화주HS코드검토항목응답 { ReviewId = 42, Code = "9401.69" }],
                TotalCount = 61,
                Page = 2,
                PageSize = 30
            }
        };
        var viewModel = new 화주HS코드검토목록ViewModel(client)
        {
            검색어 = "의자 & 가구",
            업무분류 = 20
        };

        var succeeded = await viewModel.페이지조회Async(2);

        Assert.True(succeeded);
        Assert.Equal(("의자 & 가구", 20, 2, 30), client.LastListRequest);
        Assert.Equal(42, Assert.Single(viewModel.검토목록).ReviewId);
        Assert.Equal(3, viewModel.총페이지수);
        Assert.False(viewModel.비어있음);
    }

    [Fact]
    public async Task 상세ViewModel은_요청한Id가없어도_다른항목으로대체하지않는다()
    {
        var client = new StubReviewClient { DetailResponse = null };
        var viewModel = new 화주HS코드검토상세ViewModel(client);

        var succeeded = await viewModel.조회Async(77);

        Assert.True(succeeded);
        Assert.Equal(77, client.LastDetailId);
        Assert.Equal(77, viewModel.요청ReviewId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    private sealed record StubAccessService(bool Enabled) : I화주HS코드검토접근Service
    {
        public Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
            => Task.FromResult(Enabled);
    }

    private sealed record StubCurrentUserContext(현재사용자Snapshot Current) : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 => Current;
    }

    private sealed class StubReviewClient : I화주HS코드검토Client
    {
        public 화주HS코드검토목록응답 ListResponse { get; init; } = new();
        public 화주HS코드검토상세응답? DetailResponse { get; init; }
        public (string? Query, int? Category, int Page, int PageSize)? LastListRequest { get; private set; }
        public long? LastDetailId { get; private set; }

        public Task<화주HS코드검토목록응답> 목록조회Async(
            string? query,
            int? businessCategory,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            LastListRequest = (query, businessCategory, page, pageSize);
            return Task.FromResult(ListResponse);
        }

        public Task<화주HS코드검토상세응답?> 상세조회Async(
            long reviewId,
            CancellationToken cancellationToken = default)
        {
            LastDetailId = reviewId;
            return Task.FromResult(DetailResponse);
        }
    }
}
