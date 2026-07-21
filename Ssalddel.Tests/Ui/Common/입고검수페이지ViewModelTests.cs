using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 입고검수페이지ViewModelTests
{
    [Fact]
    public async Task 초기화는_서버목록을읽되_첫검수대상을자동선택하지않는다()
    {
        var service = CreateService();
        using var page = CreatePage(service);

        var loaded = await page.초기화Async();

        Assert.True(loaded);
        Assert.Single(page.목록.항목목록);
        Assert.Null(page.상세.조회대상Id);
        Assert.Null(page.상세.항목);
        Assert.Empty(service.DetailRequestIds);
        Assert.Equal(PageViewModel상태.준비됨, page.상태);
    }

    [Fact]
    public async Task 주소의Id는_다른항목으로대체하지않고_정확한상세만조회한다()
    {
        var service = CreateService();
        using var page = CreatePage(service);

        var loaded = await page.초기화Async(71);

        Assert.True(loaded);
        Assert.Equal([71L], service.DetailRequestIds);
        Assert.Equal(71, page.상세.항목!.InboundItemId);
        Assert.Equal(12, page.작성.검수수량);
    }

    [Fact]
    public async Task 초기목록조회실패는_초기화완료로숨기지않고_새로고침으로복구한다()
    {
        var service = CreateService();
        service.ListError = new InvalidOperationException("목록 서버 오류");
        using var page = CreatePage(service);

        var loaded = await page.초기화Async();

        Assert.False(loaded);
        Assert.False(page.초기화됨);
        Assert.Equal(PageViewModel상태.실패, page.상태);
        Assert.Contains("목록 서버 오류", page.오류메시지);

        service.ListError = null;
        Assert.True(await page.새로고침Async());
        Assert.Equal(PageViewModel상태.준비됨, page.상태);
    }

    [Fact]
    public async Task 주소의정확한Id가없으면_페이지실패상태를유지한다()
    {
        var service = CreateService();
        using var page = CreatePage(service);

        var loaded = await page.초기화Async(999);

        Assert.False(loaded);
        Assert.False(page.초기화됨);
        Assert.Equal(PageViewModel상태.실패, page.상태);
        Assert.Equal([999L], service.DetailRequestIds);
        Assert.Contains("찾을 수 없", page.오류메시지);

        Assert.True(await page.경로대상변경Async(null));
        Assert.Equal(PageViewModel상태.준비됨, page.상태);
        Assert.Null(page.상세.조회대상Id);
    }

    [Fact]
    public async Task 검수저장은_확인항목을검증하고_성공뒤같은Id상세와목록을다시조회한다()
    {
        var service = CreateService();
        using var page = CreatePage(service);
        Assert.True(await page.초기화Async(71));
        page.작성.검수수량 = 12;
        page.작성.불량수량 = 2;
        page.작성.검수메모 = "박스 눌림 2개 분리";
        page.작성.수량대조확인 = true;
        page.작성.포장파손확인 = true;
        page.작성.품질기한확인 = true;
        page.작성.보관조건확인 = true;

        var saved = await page.검수후재조회Async();

        Assert.True(saved);
        Assert.Equal(71, service.LastInspectionId);
        Assert.Equal(2, service.LastInspectionRequest!.불량수량);
        Assert.Equal([71L, 71L], service.DetailRequestIds);
        Assert.Equal(2, service.ListRequestCount);
        Assert.False(page.상세.항목!.CanInspect);
        Assert.Equal("검수완료-불량포함", page.상세.항목.InventoryStatus);
    }

    [Fact]
    public async Task 검수저장은_확인항목이빠지면_Command를호출하지않는다()
    {
        var service = CreateService();
        using var page = CreatePage(service);
        Assert.True(await page.초기화Async(71));
        page.작성.수량대조확인 = true;
        page.작성.포장파손확인 = true;
        page.작성.품질기한확인 = true;

        var saved = await page.검수후재조회Async();

        Assert.False(saved);
        Assert.Null(service.LastInspectionRequest);
        Assert.Contains("네 가지", page.작성.오류메시지);
    }

    private static 입고검수PageViewModel CreatePage(Fake입고검수페이지Service service)
        => new(
            new 입고검수대상목록ViewModel(service),
            new 입고검수대상상세ViewModel(service),
            new 입고검수작성ViewModel(service));

    private static Fake입고검수페이지Service CreateService()
        => new()
        {
            Detail = new 입고검수대상상세응답
            {
                InboundItemId = 71,
                InboundId = 41,
                WarehouseId = 7,
                WarehouseName = "공동 창고",
                ProductName = "감자",
                Sku = "POTATO-01",
                ReceivedQuantity = 12,
                AvailableQuantity = 12,
                InventoryStatus = "보관중",
                StorageCondition = "냉장",
                CanInspect = true
            }
        };

    private sealed class Fake입고검수페이지Service : I입고검수페이지Service
    {
        public int ListRequestCount { get; private set; }
        public List<long> DetailRequestIds { get; } = [];
        public long? LastInspectionId { get; private set; }
        public 입고검수요청? LastInspectionRequest { get; private set; }
        public 입고검수대상상세응답? Detail { get; set; }
        public Exception? ListError { get; set; }

        public Task<입고검수대상페이지응답> 목록조회Async(
            입고검수대상목록조회요청 request,
            CancellationToken cancellationToken = default)
        {
            ListRequestCount++;
            if (ListError is not null)
            {
                throw ListError;
            }

            입고검수대상목록항목응답[] items = Detail is null
                ? []
                : new[]
                {
                    new 입고검수대상목록항목응답
                    {
                        InboundItemId = Detail.InboundItemId,
                        InboundId = Detail.InboundId,
                        WarehouseId = Detail.WarehouseId,
                        WarehouseName = Detail.WarehouseName,
                        ProductName = Detail.ProductName,
                        Sku = Detail.Sku,
                        ReceivedQuantity = Detail.ReceivedQuantity,
                        DefectiveQuantity = Detail.DefectiveQuantity,
                        InventoryStatus = Detail.InventoryStatus,
                        CanInspect = Detail.CanInspect
                    }
                };
            if (request.InspectionStatus == 입고검수조회상태코드.대기)
            {
                items = items.Where(item => item.CanInspect).ToArray();
            }

            return Task.FromResult(new 입고검수대상페이지응답
            {
                Items = items,
                TotalCount = items.Length,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<입고검수대상상세응답?> 상세조회Async(
            long inboundItemId,
            CancellationToken cancellationToken = default)
        {
            DetailRequestIds.Add(inboundItemId);
            return Task.FromResult(Detail?.InboundItemId == inboundItemId ? Detail : null);
        }

        public Task<창고작업결과응답?> 검수Async(
            long inboundItemId,
            입고검수요청 request,
            CancellationToken cancellationToken = default)
        {
            LastInspectionId = inboundItemId;
            LastInspectionRequest = request;
            if (Detail?.InboundItemId == inboundItemId)
            {
                Detail = new 입고검수대상상세응답
                {
                    InboundItemId = Detail.InboundItemId,
                    InboundId = Detail.InboundId,
                    WarehouseId = Detail.WarehouseId,
                    WarehouseName = Detail.WarehouseName,
                    ProductName = Detail.ProductName,
                    Sku = Detail.Sku,
                    ReceivedQuantity = request.검수수량,
                    AvailableQuantity = request.검수수량 - request.불량수량,
                    DefectiveQuantity = request.불량수량,
                    InventoryStatus = request.불량수량 > 0 ? "검수완료-불량포함" : "검수완료",
                    StorageCondition = Detail.StorageCondition,
                    CanInspect = false,
                    InspectionMemo = request.검수메모
                };
            }

            return Task.FromResult<창고작업결과응답?>(new 창고작업결과응답
            {
                입고상품Id = inboundItemId,
                상태 = Detail?.InventoryStatus ?? string.Empty,
                가용수량 = Detail?.AvailableQuantity ?? 0,
                불량수량 = Detail?.DefectiveQuantity ?? 0
            });
        }
    }
}
