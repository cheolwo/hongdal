using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 판매채널계정페이지ViewModelTests
{
    [Fact]
    public async Task 접근상태는_기능로그인역할을각각분리해판정한다()
    {
        var anonymous = new 판매채널페이지접근ViewModel(
            new StubAccessService(true),
            new StubCurrentUserContext(현재사용자Snapshot.익명));
        var wrongRole = new 판매채널페이지접근ViewModel(
            new StubAccessService(true),
            new StubCurrentUserContext(new 현재사용자Snapshot("orderer-1", "주문자", ["주문자"])));
        var disabled = new 판매채널페이지접근ViewModel(
            new StubAccessService(false),
            new StubCurrentUserContext(new 현재사용자Snapshot("seller-1", "판매자", ["판매자"])));

        Assert.True(await anonymous.확인Async());
        Assert.True(anonymous.로그인필요);
        Assert.True(await wrongRole.확인Async());
        Assert.True(wrongRole.역할없음);
        Assert.True(await disabled.확인Async());
        Assert.True(disabled.기능비활성);
    }

    [Fact]
    public async Task 목록ViewModel은_서버목록을검색과채널필터로만좁힌다()
    {
        var service = new StubReadService
        {
            Items =
            [
                new 판매채널계정항목응답 { Id = 1, 채널종류 = CommerceChannelKeys.SmartStore, 상점명 = "동네 식품", 연결상태 = "준비" },
                new 판매채널계정항목응답 { Id = 2, 채널종류 = CommerceChannelKeys.Coupang, 상점명 = "공구 생활", 연결상태 = "연결" }
            ]
        };
        var viewModel = new 판매채널계정목록PageViewModel(service);

        Assert.True(await viewModel.조회Async());
        viewModel.채널종류 = CommerceChannelKeys.Coupang;
        viewModel.검색어 = "생활";

        Assert.Equal(2, Assert.Single(viewModel.표시목록).Id);
        Assert.False(viewModel.계정없음);
        Assert.False(viewModel.검색결과없음);
    }

    [Fact]
    public async Task 상세ViewModel은_요청한AccountId가없어도_첫계정으로대체하지않는다()
    {
        var service = new StubReadService { Detail = null };
        var viewModel = new 판매채널계정상세PageViewModel(service);

        var succeeded = await viewModel.조회Async(77);

        Assert.True(succeeded);
        Assert.Equal(77, service.LastDetailId);
        Assert.Equal(77, viewModel.요청AccountId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.계정);
    }

    [Fact]
    public async Task 연결ViewModel은_선택한채널의자격증명을서버요청에포함한다()
    {
        var service = new StubAccountService();
        var viewModel = new 판매채널계정연결준비ViewModel(service)
        {
            채널종류 = CommerceChannelKeys.Shopify,
            상점명 = "  해외 준비 상점  "
        };
        viewModel.인증값설정("shopDomain", "my-shop.myshopify.com");
        viewModel.인증값설정("adminAccessToken", "shpat_secret");

        var succeeded = await viewModel.등록Async();

        Assert.True(succeeded);
        Assert.NotNull(service.LastRequest);
        Assert.Equal(CommerceChannelKeys.Shopify, service.LastRequest.채널종류);
        Assert.Equal("해외 준비 상점", service.LastRequest.상점명);
        Assert.Equal("my-shop.myshopify.com", service.LastRequest.인증정보["shopDomain"]);
        Assert.Equal("shpat_secret", service.LastRequest.인증정보["adminAccessToken"]);
        Assert.Equal(91, viewModel.등록된계정?.Id);
    }

    private sealed record StubAccessService(bool Enabled) : I판매채널페이지접근Service
    {
        public Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
            => Task.FromResult(Enabled);
    }

    private sealed record StubCurrentUserContext(현재사용자Snapshot Current) : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 => Current;
    }

    private sealed class StubReadService : I판매채널계정읽기Service
    {
        public IReadOnlyList<판매채널계정항목응답> Items { get; init; } = [];
        public 판매채널계정항목응답? Detail { get; init; }
        public long? LastDetailId { get; private set; }

        public Task<IReadOnlyList<판매채널계정항목응답>> 계정목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items);

        public Task<판매채널계정항목응답?> 계정상세조회Async(
            long accountId,
            CancellationToken cancellationToken = default)
        {
            LastDetailId = accountId;
            return Task.FromResult(Detail);
        }
    }

    private sealed class StubAccountService : I판매채널계정Service
    {
        public 판매채널계정저장요청? LastRequest { get; private set; }

        public Task<IReadOnlyList<판매채널계정항목응답>> 계정목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<판매채널계정항목응답>>([]);

        public Task<판매채널계정항목응답?> 계정상세조회Async(
            long accountId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<판매채널계정항목응답?>(null);

        public Task<판매채널계정항목응답?> 계정생성Async(
            판매채널계정저장요청 request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult<판매채널계정항목응답?>(new 판매채널계정항목응답
            {
                Id = 91,
                채널종류 = request.채널종류,
                상점명 = request.상점명,
                연결상태 = "준비"
            });
        }

        public Task<판매채널계정항목응답?> 계정수정Async(
            long accountId,
            판매채널계정저장요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task 계정삭제Async(long accountId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
