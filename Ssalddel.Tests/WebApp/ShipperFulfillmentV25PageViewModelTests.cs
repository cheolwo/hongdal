using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.WebApp.Services;
using Ssalddel.WebApp.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class ShipperFulfillmentV25PageViewModelTests
{
    [Fact]
    public async Task 재고에서판매상품등록은_정확한재고만읽고_채널출품없이_비샘플원장만저장한다()
    {
        var warehouse = new FakeWarehouseWorkspaceService(
            new 재고항목응답
            {
                입고상품Id = 5001,
                상품명 = "공동구매 참기름",
                SKU = "SESAME-01",
                가용수량 = 12
            });
        var products = new FakeProductService();
        var viewModel = new ShipperSalesProductCreatePageViewModel(
            warehouse,
            products);

        await viewModel.LoadAsync(5001);

        Assert.NotNull(viewModel.Inventory);
        Assert.Equal("공동구매 참기름", viewModel.ProductName);
        Assert.Equal("SESAME-01", viewModel.SalesSku);
        Assert.Equal(0m, viewModel.Price);

        viewModel.Price = 19000m;
        var created = await viewModel.CreateAsync();

        Assert.True(created);
        Assert.NotNull(products.LastCreateRequest);
        Assert.Equal(5001, products.LastCreateRequest.입고상품Id);
        Assert.Equal(19000m, products.LastCreateRequest.판매가);
        Assert.False(products.LastCreateRequest.샘플데이터여부);
        Assert.Null(products.LastCreateRequest.샘플데이터코드);
        Assert.NotNull(viewModel.CreatedProduct);
    }

    [Fact]
    public async Task 채널출품준비는_상품query만보존하고_채널은사용자가직접선택해야한다()
    {
        var products = new FakeProductService(
            new 판매상품항목응답
            {
                Id = 10,
                대표상품명 = "공동구매 참기름",
                판매SKU = "SESAME-01"
            });
        var accounts = new FakeAccountService(
            new 판매채널계정항목응답
            {
                Id = 7,
                채널종류 = CommerceChannelKeys.SmartStore,
                상점명 = "마을 판매 준비"
            });
        var listings = new FakeListingService();
        var viewModel = new ShipperSalesListingCreatePageViewModel(
            listings,
            products,
            accounts);

        await viewModel.LoadAsync(productId: 10);

        Assert.Equal(10, viewModel.SelectedProductId);
        Assert.Null(viewModel.SelectedAccountId);
        Assert.False(await viewModel.CreateAsync());
        Assert.Null(listings.LastCreateRequest);

        viewModel.SelectedAccountId = 7;
        Assert.True(await viewModel.CreateAsync());
        Assert.Equal(10, listings.LastCreateRequest?.판매상품Id);
        Assert.Equal(7, listings.LastCreateRequest?.판매채널계정Id);
    }

    [Fact]
    public void 판매상품과출품route는_양의stableId만허용한다()
    {
        Assert.Equal(
            "/shipper/sales/products/new?inventoryItemId=5001",
            ShipperRoutes.SalesProductCreateForInventory(5001));
        Assert.Equal(
            "/shipper/sales/listings/new?productId=10",
            ShipperRoutes.SalesListingCreateForProduct(10));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ShipperRoutes.SalesProductCreateForInventory(0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ShipperRoutes.SalesListingCreateForProduct(-1));
    }

    private sealed class FakeWarehouseWorkspaceService(
        params 재고항목응답[] inventory) : IWarehouseWorkspaceService
    {
        public Task<재고목록응답?> GetInventoryAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<재고목록응답?>(
                new 재고목록응답 { Items = inventory });

        public Task<창고목록응답?> GetWarehousesAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<창고요약응답?> CreateWarehouseAsync(
            창고저장요청 payload,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<입고요청목록응답?> GetInboundsAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<입고요청항목응답?> CreateInboundAsync(
            입고요청저장요청 payload,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<입고상품목록응답?> CompleteInboundAsync(
            long inboundId,
            입고완료요청 payload,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeProductService(
        params 판매상품항목응답[] products) : I상품등록Service
    {
        public 판매상품저장요청? LastCreateRequest { get; private set; }

        public Task<IReadOnlyList<판매상품항목응답>> 상품목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<판매상품항목응답>>(products);

        public Task<판매상품항목응답?> 상품생성Async(
            판매상품저장요청 request,
            CancellationToken cancellationToken = default)
        {
            LastCreateRequest = request;
            return Task.FromResult<판매상품항목응답?>(
                new 판매상품항목응답
                {
                    Id = 91,
                    입고상품Id = request.입고상품Id,
                    대표상품명 = request.대표상품명,
                    판매SKU = request.판매SKU,
                    판매가 = request.판매가,
                    상태 = "판매준비",
                    샘플데이터여부 = request.샘플데이터여부,
                    샘플데이터코드 = request.샘플데이터코드
                });
        }

        public Task<판매상품항목응답?> 상품수정Async(
            long productId,
            판매상품저장요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task 상품삭제Async(
            long productId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeAccountService(
        params 판매채널계정항목응답[] accounts) : I판매채널계정읽기Service
    {
        public Task<IReadOnlyList<판매채널계정항목응답>> 계정목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<판매채널계정항목응답>>(accounts);

        public Task<판매채널계정항목응답?> 계정상세조회Async(
            long accountId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                accounts.FirstOrDefault(account => account.Id == accountId));
    }

    private sealed class FakeListingService : I채널출품Service
    {
        public 채널출품저장요청? LastCreateRequest { get; private set; }

        public Task<IReadOnlyList<채널출품항목응답>> 출품목록조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<채널출품항목응답>>([]);

        public Task<채널출품항목응답?> 출품생성Async(
            채널출품저장요청 request,
            CancellationToken cancellationToken = default)
        {
            LastCreateRequest = request;
            return Task.FromResult<채널출품항목응답?>(
                new 채널출품항목응답
                {
                    Id = 101,
                    판매상품Id = request.판매상품Id,
                    판매채널계정Id = request.판매채널계정Id,
                    출품상태 = "출품준비",
                    동기화상태 = "수동동기화대기"
                });
        }

        public Task<채널출품항목응답?> 출품수정Async(
            long listingId,
            채널출품저장요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task 출품삭제Async(
            long listingId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
