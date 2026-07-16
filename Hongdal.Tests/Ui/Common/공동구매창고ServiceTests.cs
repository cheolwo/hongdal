using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Tests.Ui.Common;

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

    private sealed class RecordingJsonApiClient : IHongdalJsonApiClient
    {
        public object? Response { get; set; }
        public string? LastPath { get; private set; }
        public HttpMethod? LastMethod { get; private set; }
        public object? LastRequest { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            Record(HttpMethod.Get, path, null);
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

        private void Record(HttpMethod method, string path, object? request)
        {
            LastMethod = method;
            LastPath = path;
            LastRequest = request;
        }
    }
}
