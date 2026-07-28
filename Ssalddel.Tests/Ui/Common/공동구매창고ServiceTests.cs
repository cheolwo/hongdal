using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 공동구매창고ServiceTests
{
    [Fact]
    public async Task 입고목록조회_Controller경로의응답항목을반환한다()
    {
        var item = new 입고요청항목응답 { Id = 17 };
        var client = new RecordingJsonApiClient
        {
            Response = new 입고요청목록응답 { Items = [item] }
        };
        var service = new 공동구매창고Service(client);

        var result = await service.입고목록조회Async();

        Assert.Same(item, Assert.Single(result));
        Assert.Equal(HttpMethod.Get, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inbounds", client.LastPath);
    }

    [Fact]
    public async Task 입고서버목록조회는_검색정렬페이지조건을Query경로에전달한다()
    {
        var client = new RecordingJsonApiClient
        {
            Response = new 입고요청페이지응답 { TotalCount = 31, Page = 2, PageSize = 25 }
        };
        var service = new 입출고작업Service(client);

        var result = await service.입고목록조회Async(new 입고요청목록조회요청
        {
            Page = 2,
            PageSize = 25,
            Search = "공급처 A",
            SortBy = nameof(입고요청항목응답.예정도착일),
            SortDescending = false,
            WarehouseId = 17,
            Status = "입고예정",
            Sku = "SKU:FIELD-001"
        });

        Assert.Equal(31, result.TotalCount);
        Assert.Equal(HttpMethod.Get, client.LastMethod);
        Assert.Contains("api/v1/warehouse-operations/inbounds/query?", client.LastPath);
        Assert.Contains("page=2", client.LastPath);
        Assert.Contains("pageSize=25", client.LastPath);
        Assert.Contains("search=%EA%B3%B5%EA%B8%89%EC%B2%98%20A", client.LastPath);
        Assert.Contains("sortBy=%EC%98%88%EC%A0%95%EB%8F%84%EC%B0%A9%EC%9D%BC", client.LastPath);
        Assert.Contains("sortDescending=false", client.LastPath);
        Assert.Contains("warehouseId=17", client.LastPath);
        Assert.Contains("status=%EC%9E%85%EA%B3%A0%EC%98%88%EC%A0%95", client.LastPath);
        Assert.Contains("sku=SKU%3AFIELD-001", client.LastPath);
    }

    [Fact]
    public async Task 현장입고요청은_전용Controller경로와요청Id를그대로사용한다()
    {
        var request = new 현장입고요청등록요청
        {
            클라이언트요청Id = Guid.NewGuid(),
            창고Id = 17,
            상품바코드 = "SKU:FIELD-001",
            입고묶음바코드 = "BND:FIELD-001",
            상품명 = "현장 상품",
            공급처명 = "현장 공급처",
            입고수량 = 4,
            보관조건 = 현장입고보관조건.냉장,
            현장입고사유 = "계약 연결 전 현장 반입",
            임시입고안내확인 = true,
            안내버전 = 현장입고요청안내.현재버전
        };
        var client = new RecordingJsonApiClient
        {
            Response = new 입고요청항목응답 { Id = 71 }
        };
        var service = new 입출고작업Service(client);

        var result = await service.현장입고요청생성Async(request);

        Assert.Equal(71, result?.Id);
        Assert.Equal(HttpMethod.Post, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inbounds/unplanned-requests", client.LastPath);
        Assert.Same(request, client.LastRequest);
    }

    [Fact]
    public async Task 입고상세조회는_같은입고번호의Get경로를사용한다()
    {
        var item = new 입고요청항목응답 { Id = 29, 상태 = 입고상태코드.운송중 };
        var client = new RecordingJsonApiClient { Response = item };
        var service = new 입출고작업Service(client);

        var result = await service.입고상세조회Async(29);

        Assert.Same(item, result);
        Assert.Equal(HttpMethod.Get, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inbounds/29", client.LastPath);
        Assert.True(client.LastAllowNotFound);
    }

    [Fact]
    public async Task 입고상세조회ViewModel은_404응답을오류가아닌대상없음으로분리한다()
    {
        var client = new RecordingJsonApiClient();
        var viewModel = new 입고상세조회ViewModel(new 입출고작업Service(client));
        viewModel.조회대상설정(404);

        var succeeded = await viewModel.조회Async();

        Assert.True(succeeded);
        Assert.True(viewModel.대상없음);
        Assert.Null(viewModel.항목);
        Assert.Null(viewModel.오류메시지);
    }

    [Fact]
    public async Task 입고예정조회ViewModel은_상태조건을입고예정으로고정한다()
    {
        var client = new RecordingJsonApiClient
        {
            Response = new 입고요청페이지응답
            {
                Items = [new 입고요청항목응답 { Id = 7, 상태 = 입고상태코드.예정 }],
                TotalCount = 1
            }
        };
        var state = new 입출고화면상태ViewModel();
        state.창고목록적용([new() { Id = 17, 기본창고여부 = true }]);
        using var ledger = new 입고원장ViewModel(new 입출고원장상태ViewModel());
        using var query = new 입고조회ViewModel(new 입출고작업Service(client), state, ledger);
        var expectedQuery = new 입고예정조회ViewModel(query);

        var succeeded = await expectedQuery.조회Async(new 목록조회요청
        {
            검색어 = "SUP-01",
            필터조건 =
            [
                new 목록필터조건(
                    nameof(입고요청항목응답.상태),
                    "Equal",
                    입고상태코드.완료)
            ]
        });

        Assert.True(succeeded);
        Assert.Equal(1, expectedQuery.결과.전체건수);
        Assert.Contains("search=SUP-01", client.LastPath);
        Assert.Contains("warehouseId=17", client.LastPath);
        Assert.Contains("status=%EC%9E%85%EA%B3%A0%EC%98%88%EC%A0%95", client.LastPath);
        Assert.DoesNotContain("status=%EC%9E%85%EA%B3%A0%EC%99%84%EB%A3%8C", client.LastPath);
    }

    [Fact]
    public async Task 입고완료_입고번호와요청을Controller에전달한다()
    {
        var request = new 입고완료요청
        {
            Items = [new 입고상품저장요청 { 상품명 = "감자", 입고수량 = 10 }]
        };
        var client = new RecordingJsonApiClient
        {
            Response = new 입고상품목록응답()
        };
        var service = new 공동구매창고Service(client);

        await service.입고완료Async(23, request);

        Assert.Equal(HttpMethod.Post, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inbounds/23/complete", client.LastPath);
        Assert.Same(request, client.LastRequest);
    }

    [Fact]
    public async Task 입고수정과취소는_같은입고리소스의PutDelete경로를사용한다()
    {
        var request = new 입고요청저장요청 { 창고Id = 3, 공급처명 = "생산자" };
        var client = new RecordingJsonApiClient
        {
            Response = new 입고요청항목응답 { Id = 37 }
        };
        var service = new 공동구매창고Service(client);

        await service.입고요청수정Async(37, request);

        Assert.Equal(HttpMethod.Put, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inbounds/37", client.LastPath);
        Assert.Same(request, client.LastRequest);

        await service.입고요청취소Async(37);

        Assert.Equal(HttpMethod.Delete, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inbounds/37", client.LastPath);
        Assert.Null(client.LastRequest);
    }

    [Fact]
    public async Task 출고포장_입고상품번호를재고경로에전달한다()
    {
        var request = new 포장작업요청 { 포장수량 = 4 };
        var client = new RecordingJsonApiClient
        {
            Response = new 창고작업결과응답 { 입고상품Id = 31 }
        };
        var service = new 공동구매창고Service(client);

        await service.포장작업Async(31, request);

        Assert.Equal("api/v1/warehouse-operations/inventory/31/pack", client.LastPath);
        Assert.Same(request, client.LastRequest);
    }

    [Fact]
    public async Task 운송인계_재위탁운송Controller경로를사용한다()
    {
        var request = new 재고운송의뢰생성요청 { 입고상품Id = 31, 요청수량 = 4 };
        var client = new RecordingJsonApiClient
        {
            Response = new 화주운송의뢰응답 { 의뢰Id = "shipping-1" }
        };
        var service = new 공동구매창고Service(client);

        var result = await service.운송인계Async(request);

        Assert.Equal("shipping-1", result?.의뢰Id);
        Assert.Equal("api/v1/warehouse-operations/inventory/reconsignment", client.LastPath);
        Assert.Same(request, client.LastRequest);
    }

    [Fact]
    public async Task 입고검수대상목록은_전용필터와서버페이징경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 입고검수대상페이지응답() };
        var service = new 입고검수페이지Service(client);

        await service.목록조회Async(new 입고검수대상목록조회요청
        {
            WarehouseId = 7,
            Search = "감자 10kg",
            InspectionStatus = 입고검수조회상태코드.완료,
            Page = 2,
            PageSize = 20
        });

        Assert.Equal(HttpMethod.Get, client.LastMethod);
        Assert.Contains("api/v1/warehouse-operations/inventory/inspection-targets?", client.LastPath);
        Assert.Contains("inspectionStatus=%EC%99%84%EB%A3%8C", client.LastPath);
        Assert.Contains("search=%EA%B0%90%EC%9E%90%2010kg", client.LastPath);
        Assert.Contains("warehouseId=7", client.LastPath);
        Assert.Contains("page=2", client.LastPath);
        Assert.False(client.LastAllowNotFound);
    }

    [Fact]
    public async Task 입고검수상세는_명시한입고상품Id전용경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 입고검수대상상세응답 { InboundItemId = 71 } };
        var service = new 입고검수페이지Service(client);

        var result = await service.상세조회Async(71);

        Assert.Equal(71, result!.InboundItemId);
        Assert.Equal("api/v1/warehouse-operations/inventory/71/inspection-target", client.LastPath);
        Assert.True(client.LastAllowNotFound);
    }

    [Fact]
    public async Task 입고검수저장은_기존검수Command경로를재사용한다()
    {
        var request = new 입고검수요청 { 검수수량 = 12, 불량수량 = 1 };
        var client = new RecordingJsonApiClient { Response = new 창고작업결과응답 { 입고상품Id = 71 } };
        var service = new 입고검수페이지Service(client);

        await service.검수Async(71, request);

        Assert.Equal(HttpMethod.Post, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/inventory/71/inspect", client.LastPath);
        Assert.Same(request, client.LastRequest);
    }

    [Fact]
    public async Task 피킹작업목록은_상태검색창고와서버페이징경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 피킹작업목록페이지응답() };
        var service = new 피킹작업페이지Service(client);

        await service.목록조회Async(new 피킹작업목록조회요청
        {
            WarehouseId = 7,
            Search = "감자 묶음",
            Status = 피킹작업조회상태코드.진행중,
            Page = 2,
            PageSize = 20
        });

        Assert.Equal(HttpMethod.Get, client.LastMethod);
        Assert.Contains("api/v1/warehouse-operations/picking-tasks?", client.LastPath);
        Assert.Contains("status=%EC%A7%84%ED%96%89%EC%A4%91", client.LastPath);
        Assert.Contains("search=%EA%B0%90%EC%9E%90%20%EB%AC%B6%EC%9D%8C", client.LastPath);
        Assert.Contains("warehouseId=7", client.LastPath);
        Assert.Contains("page=2", client.LastPath);
        Assert.False(client.LastAllowNotFound);
    }

    [Fact]
    public async Task 재고현황목록과상세는_별도최소조회경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 창고재고현황목록페이지응답() };
        var service = new 재고현황페이지Service(client);

        await service.목록조회Async(new 창고재고현황목록조회요청
        {
            WarehouseId = 7,
            Search = "감자 묶음",
            Status = 창고재고조회상태코드.예약,
            Page = 2,
            PageSize = 20
        });

        Assert.Contains("api/v1/warehouse-operations/inventory-overview?", client.LastPath);
        Assert.Contains("status=%EC%98%88%EC%95%BD", client.LastPath);
        Assert.Contains("warehouseId=7", client.LastPath);
        Assert.Contains("page=2", client.LastPath);
        Assert.False(client.LastAllowNotFound);

        client.Response = new 창고재고현황상세응답 { InboundItemId = 71 };
        await service.상세조회Async(71);
        Assert.Equal("api/v1/warehouse-operations/inventory-overview/71", client.LastPath);
        Assert.True(client.LastAllowNotFound);
    }

    [Fact]
    public async Task 적재작업은_목록상세완료전용경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 적재작업목록페이지응답() };
        var service = new 적재작업페이지Service(client);
        await service.목록조회Async(new 적재작업목록조회요청 { WarehouseId=7, Search="감자", Status=적재작업조회상태코드.완료, Page=2 });
        Assert.Contains("api/v1/warehouse-operations/put-away-tasks?", client.LastPath);
        Assert.Contains("warehouseId=7", client.LastPath); Assert.Contains("page=2", client.LastPath); Assert.False(client.LastAllowNotFound);
        client.Response = new 적재작업상세응답 { InboundItemId=71 }; await service.상세조회Async(71);
        Assert.Equal("api/v1/warehouse-operations/put-away-tasks/71", client.LastPath); Assert.True(client.LastAllowNotFound);
        var request = new 적재작업완료요청 { StorageLocation="A-01" }; client.Response = new 적재작업결과응답 { InboundItemId=71 };
        await service.완료Async(71,request); Assert.Equal(HttpMethod.Post,client.LastMethod); Assert.Equal("api/v1/warehouse-operations/put-away-tasks/71/complete",client.LastPath); Assert.Same(request,client.LastRequest);
    }

    [Fact]
    public async Task 포장작업은_목록상세완료전용경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 포장작업목록페이지응답() };
        var service = new 포장작업페이지Service(client);
        await service.목록조회Async(new 포장작업목록조회요청 { WarehouseId=7, Search="감자", Status=포장작업조회상태코드.완료, Page=2 });
        Assert.Contains("api/v1/warehouse-operations/packing-tasks?", client.LastPath);
        Assert.Contains("warehouseId=7", client.LastPath); Assert.Contains("page=2", client.LastPath); Assert.False(client.LastAllowNotFound);
        client.Response = new 포장작업상세응답 { InboundItemId=71 }; await service.상세조회Async(71);
        Assert.Equal("api/v1/warehouse-operations/packing-tasks/71", client.LastPath); Assert.True(client.LastAllowNotFound);
        var request = new 포장작업완료요청 { PackagingQuantity=9 }; client.Response = new 포장작업결과응답 { InboundItemId=71 };
        await service.완료Async(71,request); Assert.Equal(HttpMethod.Post,client.LastMethod); Assert.Equal("api/v1/warehouse-operations/packing-tasks/71/complete",client.LastPath); Assert.Same(request,client.LastRequest);
    }

    [Fact]
    public async Task 출고인계준비는_목록상세완료전용경로를사용한다()
    {
        var client=new RecordingJsonApiClient{Response=new 출고인계준비목록페이지응답()};var service=new 출고인계준비페이지Service(client);
        await service.목록조회Async(new 출고인계준비목록조회요청{WarehouseId=7,Search="감자",Status=출고인계준비조회상태코드.완료,Page=2});
        Assert.Contains("api/v1/warehouse-operations/outbound-handoff-tasks?",client.LastPath);Assert.Contains("warehouseId=7",client.LastPath);Assert.Contains("page=2",client.LastPath);Assert.False(client.LastAllowNotFound);
        client.Response=new 출고인계준비상세응답{InboundItemId=71};await service.상세조회Async(71);Assert.Equal("api/v1/warehouse-operations/outbound-handoff-tasks/71",client.LastPath);Assert.True(client.LastAllowNotFound);
        var request=new 출고인계준비완료요청{HandoffQuantity=9};client.Response=new 출고인계준비결과응답{InboundItemId=71};await service.완료Async(71,request);Assert.Equal(HttpMethod.Post,client.LastMethod);Assert.Equal("api/v1/warehouse-operations/outbound-handoff-tasks/71/complete",client.LastPath);Assert.Same(request,client.LastRequest);
    }

    [Fact]
    public async Task 출고예정검토는_목록상세와기사확인인계완료경로를사용한다()
    {
        var client=new RecordingJsonApiClient{Response=new 출고예정검토목록페이지응답()};var service=new 출고예정검토페이지Service(client);
        await service.목록조회Async(new 출고예정검토목록조회요청{WarehouseId=7,Search="감자",Status=출고예정검토조회상태코드.운송연결,Page=2});
        Assert.Equal(HttpMethod.Get,client.LastMethod);Assert.Contains("api/v1/warehouse-operations/outbound-plan-reviews?",client.LastPath);Assert.Contains("warehouseId=7",client.LastPath);Assert.Contains("page=2",client.LastPath);Assert.False(client.LastAllowNotFound);
        client.Response=new 출고예정검토상세응답{OutboundPlanId=31};var result=await service.상세조회Async(31);
        Assert.Equal(31,result!.OutboundPlanId);Assert.Equal(HttpMethod.Get,client.LastMethod);Assert.Equal("api/v1/warehouse-operations/outbound-plan-reviews/31",client.LastPath);Assert.True(client.LastAllowNotFound);
        var request=new 출고운송인계완료요청{DriverIdentityConfirmed=true,VehicleConfirmed=true,CargoReleasedConfirmed=true};
        client.Response=new 출고운송인계완료응답{OutboundPlanId=31};var completed=await service.인계완료Async(31,request);
        Assert.Equal(31,completed.OutboundPlanId);Assert.Equal(HttpMethod.Post,client.LastMethod);Assert.Equal("api/v1/warehouse-operations/outbound-plan-reviews/31/handoff-complete",client.LastPath);Assert.Same(request,client.LastRequest);
    }

    [Fact]
    public async Task 피킹작업상세와시작은_명시한TaskKey전용경로를사용한다()
    {
        var client = new RecordingJsonApiClient { Response = new 피킹작업상세응답 { TaskKey = "PICK A/1" } };
        var service = new 피킹작업페이지Service(client);

        await service.상세조회Async("PICK A/1");

        Assert.Equal("api/v1/warehouse-operations/picking-tasks/PICK%20A%2F1", client.LastPath);
        Assert.True(client.LastAllowNotFound);

        client.Response = new 피킹작업결과응답 { TaskKey = "PICK A/1", Status = "진행중" };
        await service.시작Async("PICK A/1");

        Assert.Equal(HttpMethod.Post, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/picking-tasks/PICK%20A%2F1/start", client.LastPath);
        Assert.Null(client.LastRequest);
    }

    [Fact]
    public async Task 피킹작업완료는_확인요청을같은TaskKeyCommand경로로전달한다()
    {
        var request = new 피킹작업완료요청
        {
            RackCode = "A-01-03",
            ProductConfirmed = true,
            QuantityConfirmed = true
        };
        var client = new RecordingJsonApiClient { Response = new 피킹작업결과응답 { TaskKey = "PICK-71" } };
        var service = new 피킹작업페이지Service(client);

        await service.완료Async("PICK-71", request);

        Assert.Equal(HttpMethod.Post, client.LastMethod);
        Assert.Equal("api/v1/warehouse-operations/picking-tasks/PICK-71/complete", client.LastPath);
        Assert.Same(request, client.LastRequest);
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public object? Response { get; set; }
        public string? LastPath { get; private set; }
        public HttpMethod? LastMethod { get; private set; }
        public object? LastRequest { get; private set; }
        public bool? LastAllowNotFound { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            Record(HttpMethod.Get, path, null, allowNotFound);
            return Task.FromResult(Response is null ? default : (TResponse)Response);
        }

        public Task<TResponse?> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
        {
            Record(method, path, null);
            return Task.FromResult(Response is null ? default : (TResponse)Response);
        }

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
        {
            Record(method, path, request);
            return Task.FromResult(Response is null ? default : (TResponse)Response);
        }

        public Task SendAsync(
            HttpMethod method,
            string path,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            Record(method, path, null);
            return Task.CompletedTask;
        }

        public Task SendAsync<TRequest>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            Record(method, path, request);
            return Task.CompletedTask;
        }

        private void Record(HttpMethod method, string path, object? request, bool? allowNotFound = null)
        {
            LastMethod = method;
            LastPath = path;
            LastRequest = request;
            LastAllowNotFound = allowNotFound;
        }
    }
}
