using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class InboundRequestPageViewModelTests
{
    [Fact]
    public async Task 신규신청은_저장응답Id를같은Adapter에서다시조회한다()
    {
        var service = new FakeWarehouseWorkspaceService();
        var viewModel = new InboundRequestPageViewModel(service);
        await viewModel.LoadCreateAsync(new InboundRequestNavigationContext
        {
            WarehouseId = 5,
            SupplierName = "공동주문 공급처",
            OrderReference = "ORDER-5"
        });

        var inboundId = await viewModel.CreateInboundAsync();

        Assert.Equal(77, inboundId);
        Assert.Equal([77L], service.DetailRequests);
        Assert.Equal(77, viewModel.State.Current?.Id);
        Assert.True(viewModel.State.Created);
        Assert.Contains("같은 ID", viewModel.State.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 입고완료는_Command응답뒤같은Id를재조회한다()
    {
        var service = new FakeWarehouseWorkspaceService();
        service.Inbounds.Add(new 입고요청항목응답
        {
            Id = 41,
            창고Id = 5,
            공급처명 = "샘플 공급처",
            상태 = 입고상태코드.예정,
            예정상품명 = "공동구매 채소",
            예정수량 = 12
        });
        var viewModel = new InboundRequestPageViewModel(service);
        await viewModel.LoadCompletionAsync(41);
        viewModel.CompletionDraft.StorageLocation = "A-01";

        var completed = await viewModel.CompleteInboundAsync(41);

        Assert.True(completed);
        Assert.Equal([41L], service.CompletedRequests);
        Assert.Equal([41L, 41L], service.DetailRequests);
        Assert.Equal(입고상태코드.완료, viewModel.State.Current?.상태);
        Assert.Single(viewModel.State.CompletionItems);
        Assert.False(viewModel.CanComplete);
    }

    [Fact]
    public async Task 완료수량검증실패는_Command를호출하지않는다()
    {
        var service = new FakeWarehouseWorkspaceService();
        service.Inbounds.Add(new 입고요청항목응답
        {
            Id = 42,
            창고Id = 5,
            공급처명 = "샘플 공급처",
            상태 = 입고상태코드.예정
        });
        var viewModel = new InboundRequestPageViewModel(service);
        await viewModel.LoadCompletionAsync(42);
        viewModel.CompletionDraft.ItemName = "상품";
        viewModel.CompletionDraft.Quantity = 2;
        viewModel.CompletionDraft.DefectQuantity = 3;

        var completed = await viewModel.CompleteInboundAsync(42);

        Assert.False(completed);
        Assert.Empty(service.CompletedRequests);
        Assert.Equal(InboundRequestPageMessageTone.Warning, viewModel.State.MessageTone);
    }

    [Fact]
    public async Task 창고등록은_입고신청Command와분리된다()
    {
        var service = new FakeWarehouseWorkspaceService();
        var viewModel = new InboundRequestPageViewModel(service);
        viewModel.ApplyWarehouseContext(new InboundRequestNavigationContext
        {
            WarehouseName = "생활물류센터",
            WarehouseAddress = "서울시 샘플로 1",
            ProxyType = LogisticsProxySiteTypes.UrbanLogisticsCenter
        });

        var warehouseId = await viewModel.CreateWarehouseAsync();

        Assert.Equal(21, warehouseId);
        Assert.Equal(1, service.WarehouseCreateCount);
        Assert.Equal(0, service.InboundCreateCount);
    }

    [Fact]
    public async Task 실제창고Id없는다이어그램후보는_임의의첫창고를선택하지않는다()
    {
        var service = new FakeWarehouseWorkspaceService();
        var viewModel = new InboundRequestPageViewModel(service);

        await viewModel.LoadCreateAsync(new InboundRequestNavigationContext
        {
            Source = "diagram-warehouse-proxy",
            WarehouseName = "아직 등록되지 않은 후보"
        });

        Assert.Null(viewModel.CreateDraft.WarehouseId);
        Assert.Equal(InboundRequestPageMessageTone.Warning, viewModel.State.MessageTone);
    }

    [Theory]
    [InlineData(입고흐름유형코드.현장임시입고)]
    [InlineData(입고흐름유형코드.주문자동입고예정)]
    public async Task 전용Workflow흐름은_일반신청Command로우회하지않는다(string flowType)
    {
        var service = new FakeWarehouseWorkspaceService();
        var viewModel = new InboundRequestPageViewModel(service);
        await viewModel.LoadCreateAsync(new InboundRequestNavigationContext
        {
            WarehouseId = 5,
            SupplierName = "공급처"
        });
        viewModel.CreateDraft.FlowType = flowType;

        var inboundId = await viewModel.CreateInboundAsync();

        Assert.Null(inboundId);
        Assert.Equal(0, service.InboundCreateCount);
        Assert.Equal(InboundRequestPageMessageTone.Warning, viewModel.State.MessageTone);
    }

    private sealed class FakeWarehouseWorkspaceService : IWarehouseWorkspaceService
    {
        public List<입고요청항목응답> Inbounds { get; } = [];
        public List<long> DetailRequests { get; } = [];
        public List<long> CompletedRequests { get; } = [];
        public int WarehouseCreateCount { get; private set; }
        public int InboundCreateCount { get; private set; }

        public Task<창고목록응답?> GetWarehousesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<창고목록응답?>(new()
            {
                Items =
                [
                    new 창고요약응답
                    {
                        Id = 5,
                        창고명 = "샘플 창고",
                        주소 = "서울시 샘플로 1",
                        IsActive = true
                    }
                ]
            });

        public Task<창고요약응답?> CreateWarehouseAsync(
            창고저장요청 payload,
            CancellationToken cancellationToken = default)
        {
            WarehouseCreateCount++;
            return Task.FromResult<창고요약응답?>(new()
            {
                Id = 21,
                창고명 = payload.창고명,
                주소 = payload.주소,
                물류대행지분류 = payload.물류대행지분류,
                IsActive = true
            });
        }

        public Task<입고요청목록응답?> GetInboundsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<입고요청목록응답?>(new() { Items = Inbounds.ToArray() });

        public Task<입고요청항목응답?> GetInboundAsync(long inboundId, CancellationToken cancellationToken = default)
        {
            DetailRequests.Add(inboundId);
            return Task.FromResult(Inbounds.FirstOrDefault(item => item.Id == inboundId));
        }

        public Task<입고요청항목응답?> CreateInboundAsync(
            입고요청저장요청 payload,
            CancellationToken cancellationToken = default)
        {
            InboundCreateCount++;
            var item = new 입고요청항목응답
            {
                Id = 77,
                창고Id = payload.창고Id,
                공급처명 = payload.공급처명,
                원주문참조번호 = payload.원주문참조번호,
                상태 = 입고상태코드.예정,
                계약정보 = payload.계약정보
            };
            Inbounds.Add(item);
            return Task.FromResult<입고요청항목응답?>(item);
        }

        public Task<입고상품목록응답?> CompleteInboundAsync(
            long inboundId,
            입고완료요청 payload,
            CancellationToken cancellationToken = default)
        {
            CompletedRequests.Add(inboundId);
            var inbound = Inbounds.Single(item => item.Id == inboundId);
            inbound.상태 = 입고상태코드.완료;
            inbound.입고완료일시 = DateTime.UtcNow;
            var item = payload.Items.Single();
            return Task.FromResult<입고상품목록응답?>(new()
            {
                Items =
                [
                    new 입고상품항목응답
                    {
                        Id = 501,
                        입고요청Id = inboundId,
                        창고Id = inbound.창고Id,
                        상품명 = item.상품명,
                        SKU = item.SKU,
                        입고수량 = item.입고수량,
                        불량수량 = item.불량수량,
                        가용수량 = item.입고수량 - item.불량수량,
                        보관위치 = item.보관위치,
                        상태 = "보관중"
                    }
                ]
            });
        }

        public Task<재고목록응답?> GetInventoryAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<재고목록응답?>(new());
    }
}
