using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 판매페이지공개상품SeedTests
{
    [Fact]
    public void 공개상품근거는_개인원장값없이판매페이지Query로전달된다()
    {
        var seed = CreateSeed();

        var uri = seed.ToNavigationUri("/shipper/sales/pages/new");

        Assert.StartsWith("/shipper/sales/pages/new?", uri, StringComparison.Ordinal);
        Assert.Contains("sourceProductId=41", uri, StringComparison.Ordinal);
        Assert.Contains("completedLedgerVerified=true", uri, StringComparison.Ordinal);
        Assert.Contains("reviewCount=3", uri, StringComparison.Ordinal);
        Assert.DoesNotContain("ledgerId", uri, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ledger-private", uri, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("participant", uri, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task 근거적용은_설명만채우고가격원산지재고를확정하거나자동저장하지않는다()
    {
        var client = new RecordingSalesPageClient();
        var viewModel = new 판매페이지작성ViewModel(client);

        viewModel.공개상품근거적용(CreateSeed());

        Assert.Equal("제철 감자 10kg", viewModel.초안.상품명);
        Assert.Equal(41, viewModel.초안.원본공개상품Id);
        Assert.Contains("완료 구매 원장 확인", viewModel.초안.상세설명);
        Assert.Contains("공개 후기 3건", viewModel.초안.상세설명);
        Assert.Null(viewModel.초안.판매가);
        Assert.Null(viewModel.초안.원산지표시);
        Assert.Null(viewModel.초안.출고지표시);
        Assert.Equal(0, client.CreateCallCount);

        viewModel.초안.판매자표시명 = "동네 협동조합";
        viewModel.초안.판매가 = 21_000m;
        Assert.True(await viewModel.초안생성Async());
        Assert.Equal(1, client.CreateCallCount);
        Assert.Equal(21_000m, client.LastCreateRequest!.판매가);
    }

    private static 판매페이지공개상품Seed CreateSeed()
        => new(
            41,
            "제철 감자 10kg",
            "농산물",
            "함께 구매한 감자입니다.",
            "상자",
            19_000m,
            true,
            3,
            new DateTime(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc));

    private sealed class RecordingSalesPageClient : I판매페이지Client
    {
        public int CreateCallCount { get; private set; }
        public 판매페이지초안생성요청? LastCreateRequest { get; private set; }

        public Task<IReadOnlyList<판매페이지초안응답>> 초안목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<판매페이지초안응답>>([]);

        public Task<판매페이지초안응답?> 초안조회Async(
            string pageId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<판매페이지초안응답?>(null);

        public Task<판매페이지초안응답?> 초안생성Async(
            판매페이지초안생성요청 request,
            CancellationToken cancellationToken = default)
        {
            CreateCallCount++;
            LastCreateRequest = request;
            return Task.FromResult<판매페이지초안응답?>(new 판매페이지초안응답
            {
                페이지Id = "sales-page-1",
                상품명 = request.상품명,
                판매자표시명 = request.판매자표시명,
                한줄소개 = request.한줄소개,
                상세설명 = request.상세설명,
                판매가 = request.판매가,
                개별주문허용 = request.개별주문허용,
                공동주문허용 = request.공동주문허용,
                최소주문수량 = request.최소주문수량
            });
        }

        public Task<판매페이지초안응답?> 초안수정Async(
            string pageId,
            판매페이지초안수정요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
