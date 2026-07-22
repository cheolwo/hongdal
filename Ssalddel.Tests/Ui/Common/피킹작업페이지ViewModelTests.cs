using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 피킹작업페이지ViewModelTests
{
    [Fact]
    public async Task 목록조회는_상세를자동조회하지않는다()
    {
        var service = CreateService();
        var list = new 피킹작업목록ViewModel(service);

        Assert.True(await list.조회Async());

        Assert.Single(list.항목목록);
        Assert.Equal(1, service.ListRequestCount);
        Assert.Empty(service.DetailRequestKeys);
    }

    [Fact]
    public async Task 시작성공뒤_목록없이_같은TaskKey상세만다시조회한다()
    {
        var service = CreateService();
        using var page = CreatePage(service);
        Assert.True(await page.초기화Async("PICK-71"));

        var started = await page.시작후재조회Async();

        Assert.True(started);
        Assert.Equal(["PICK-71", "PICK-71"], service.DetailRequestKeys);
        Assert.Equal(0, service.ListRequestCount);
        Assert.Equal("진행중", page.상세.항목!.Status);
        Assert.Equal("PICK-71", page.상세.조회대상Key);
    }

    [Fact]
    public async Task 완료는_적재대와두확인을요구하고_성공뒤같은TaskKey를재조회한다()
    {
        var service = CreateService(status: "진행중");
        using var page = CreatePage(service);
        Assert.True(await page.초기화Async("PICK-71"));

        Assert.False(await page.완료후재조회Async());
        Assert.Null(service.LastCompleteRequest);

        page.처리.적재대확인코드 = "RACK-A-01";
        page.처리.상품확인 = true;
        page.처리.전체수량확인 = true;
        Assert.True(await page.완료후재조회Async());

        Assert.Equal("RACK-A-01", service.LastCompleteRequest!.RackCode);
        Assert.Equal(["PICK-71", "PICK-71"], service.DetailRequestKeys);
        Assert.Equal("완료", page.상세.항목!.Status);
    }

    private static 피킹작업실행ViewModel CreatePage(FakePickingService service)
        => new(
            new 피킹작업상세ViewModel(service),
            new 피킹작업처리ViewModel(service));

    private static FakePickingService CreateService(string status = "대기")
        => new() { Detail = Detail(status) };

    private static 피킹작업상세응답 Detail(string status)
        => new()
        {
            TaskKey = "PICK-71",
            WarehouseId = 7,
            WarehouseName = "공동 창고",
            ProductName = "공동구매 감자",
            Sku = "POTATO-01",
            Quantity = 12,
            RackCode = "RACK-A-01",
            Status = status,
            CanStart = status == "대기",
            CanComplete = status == "진행중"
        };

    private sealed class FakePickingService : I피킹작업페이지Service
    {
        public int ListRequestCount { get; private set; }
        public List<string> DetailRequestKeys { get; } = [];
        public 피킹작업완료요청? LastCompleteRequest { get; private set; }
        public 피킹작업상세응답? Detail { get; set; }

        public Task<피킹작업목록페이지응답> 목록조회Async(
            피킹작업목록조회요청 request,
            CancellationToken cancellationToken = default)
        {
            ListRequestCount++;
            var items = Detail is null ? [] : new[]
            {
                new 피킹작업목록항목응답
                {
                    TaskKey = Detail.TaskKey,
                    WarehouseId = Detail.WarehouseId,
                    WarehouseName = Detail.WarehouseName,
                    ProductName = Detail.ProductName,
                    Sku = Detail.Sku,
                    Quantity = Detail.Quantity,
                    RackCode = Detail.RackCode,
                    Status = Detail.Status
                }
            };
            return Task.FromResult(new 피킹작업목록페이지응답
            {
                Items = items,
                TotalCount = items.Length,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<피킹작업상세응답?> 상세조회Async(string taskKey, CancellationToken cancellationToken = default)
        {
            DetailRequestKeys.Add(taskKey);
            return Task.FromResult(Detail?.TaskKey == taskKey ? Detail : null);
        }

        public Task<피킹작업결과응답?> 시작Async(string taskKey, CancellationToken cancellationToken = default)
        {
            Detail = 피킹작업페이지ViewModelTests.Detail("진행중");
            return Task.FromResult<피킹작업결과응답?>(new() { TaskKey = taskKey, Status = "진행중", Quantity = 12 });
        }

        public Task<피킹작업결과응답?> 완료Async(
            string taskKey,
            피킹작업완료요청 request,
            CancellationToken cancellationToken = default)
        {
            LastCompleteRequest = request;
            Detail = 피킹작업페이지ViewModelTests.Detail("완료");
            return Task.FromResult<피킹작업결과응답?>(new() { TaskKey = taskKey, Status = "완료", Quantity = 12 });
        }
    }
}
