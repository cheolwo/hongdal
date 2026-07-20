using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 입고상품수령페이지ViewModelTests
{
    [Fact]
    public async Task 입고예정검색은_창고와Sku를전달하고_완전일치항목만남긴다()
    {
        var client = new ScenarioJsonApiClient
        {
            Responder = (_, path, responseType, _) => responseType == typeof(입고요청페이지응답)
                ? new 입고요청페이지응답
                {
                    Items =
                    [
                        new 입고요청항목응답 { Id = 11, 예정SKU = "SKU:FIELD-001" },
                        new 입고요청항목응답 { Id = 12, 예정SKU = "SKU:FIELD-001-X" }
                    ],
                    TotalCount = 2
                }
                : throw new InvalidOperationException($"예상하지 않은 요청: {path}")
        };
        var viewModel = new 입고예정상품검색ViewModel(new 입출고작업Service(client))
        {
            상품바코드 = " sku:field-001 "
        };

        var succeeded = await viewModel.검색Async(17);

        Assert.True(succeeded);
        Assert.Equal(11, Assert.Single(viewModel.후보목록).Id);
        Assert.Contains("warehouseId=17", client.Last.Path);
        Assert.Contains("status=%EC%9E%85%EA%B3%A0%EC%98%88%EC%A0%95", client.Last.Path);
        Assert.Contains("sku=SKU%3AFIELD-001", client.Last.Path);
    }

    [Fact]
    public async Task 현장입고작성은_안내확인전에는전송하지않고_같은요청Id를사용한다()
    {
        var client = new ScenarioJsonApiClient
        {
            Responder = (_, _, responseType, _) => responseType == typeof(입고요청항목응답)
                ? new 입고요청항목응답 { Id = 71 }
                : null
        };
        var viewModel = new 현장입고요청작성ViewModel(new 입출고작업Service(client));
        viewModel.새요청준비("SKU:FIELD-001");
        viewModel.상품명 = "현장 상품";
        viewModel.공급처명 = "현장 공급처";
        var requestId = viewModel.클라이언트요청Id;

        var rejected = await viewModel.등록Async(17);

        viewModel.임시입고안내확인 = true;
        var saved = await viewModel.등록Async(17);
        var request = Assert.IsType<현장입고요청등록요청>(client.Last.Request);

        Assert.False(rejected);
        Assert.True(saved);
        Assert.Single(client.Requests);
        Assert.Equal(requestId, request.클라이언트요청Id);
        Assert.True(request.임시입고안내확인);
        Assert.Equal(현장입고요청안내.현재버전, request.안내버전);
        Assert.Equal("api/v1/warehouse-operations/inbounds/unplanned-requests", client.Last.Path);
    }

    [Fact]
    public async Task 페이지는_현장요청저장뒤_반환된같은Id를다시조회한다()
    {
        var persisted = new 입고요청항목응답
        {
            Id = 88,
            창고Id = 17,
            예정상품명 = "현장 상품",
            예정SKU = "SKU:FIELD-001",
            예정수량 = 3,
            상태 = 입고상태코드.예정,
            입고흐름유형 = 입고흐름유형코드.현장임시입고
        };
        var client = new ScenarioJsonApiClient
        {
            Responder = (method, path, responseType, _) =>
            {
                if (responseType == typeof(창고목록응답))
                {
                    return new 창고목록응답
                    {
                        Items = [new 창고요약응답 { Id = 17, 창고명 = "공동 창고", IsActive = true }]
                    };
                }

                if (method == HttpMethod.Post
                    && path == "api/v1/warehouse-operations/inbounds/unplanned-requests")
                {
                    return new 입고요청항목응답 { Id = 88, 창고Id = 17 };
                }

                if (method == HttpMethod.Get
                    && path == "api/v1/warehouse-operations/inbounds/88")
                {
                    return persisted;
                }

                throw new InvalidOperationException($"예상하지 않은 요청: {method} {path}");
            }
        };
        var service = new 입출고작업Service(client);
        using var page = new 입고상품수령PageViewModel(
            new 입고상품수령창고ViewModel(service),
            new 입고예정상품검색ViewModel(service),
            new 현장입고요청작성ViewModel(service),
            new 입고상품수령상세ViewModel(service));
        Assert.True(await page.초기화Async(17));
        page.현장입고작성시작();
        page.작성.상품바코드 = "SKU:FIELD-001";
        page.작성.입고묶음바코드 = "BND:FIELD-001";
        page.작성.상품명 = "현장 상품";
        page.작성.공급처명 = "현장 공급처";
        page.작성.입고수량 = 3;
        page.작성.임시입고안내확인 = true;

        var succeeded = await page.현장입고등록후조회Async();

        Assert.True(succeeded);
        Assert.Same(persisted, page.상세.항목);
        Assert.False(page.작성.폼표시);
        Assert.Equal(
            [
                "api/v1/warehouse-operations/warehouses",
                "api/v1/warehouse-operations/inbounds/unplanned-requests",
                "api/v1/warehouse-operations/inbounds/88"
            ],
            client.Requests.Select(item => item.Path));
        Assert.DoesNotContain(client.Requests, item => item.Path.Contains("inventory", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(client.Requests, item => item.Path.Contains("complete", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task 페이지는_저장뒤같은Id를찾지못하면_완료로처리하지않는다()
    {
        var client = new ScenarioJsonApiClient
        {
            Responder = (method, path, responseType, _) =>
            {
                if (responseType == typeof(창고목록응답))
                {
                    return new 창고목록응답
                    {
                        Items = [new 창고요약응답 { Id = 17, 창고명 = "공동 창고", IsActive = true }]
                    };
                }

                if (method == HttpMethod.Post
                    && path == "api/v1/warehouse-operations/inbounds/unplanned-requests")
                {
                    return new 입고요청항목응답 { Id = 88, 창고Id = 17 };
                }

                if (method == HttpMethod.Get
                    && path == "api/v1/warehouse-operations/inbounds/88")
                {
                    return null;
                }

                throw new InvalidOperationException($"예상하지 않은 요청: {method} {path}");
            }
        };
        var service = new 입출고작업Service(client);
        using var page = new 입고상품수령PageViewModel(
            new 입고상품수령창고ViewModel(service),
            new 입고예정상품검색ViewModel(service),
            new 현장입고요청작성ViewModel(service),
            new 입고상품수령상세ViewModel(service));
        Assert.True(await page.초기화Async(17));
        page.현장입고작성시작();
        page.작성.상품바코드 = "SKU:FIELD-001";
        page.작성.입고묶음바코드 = "BND:FIELD-001";
        page.작성.상품명 = "현장 상품";
        page.작성.공급처명 = "현장 공급처";
        page.작성.임시입고안내확인 = true;

        var succeeded = await page.현장입고등록후조회Async();

        Assert.False(succeeded);
        Assert.True(page.작성.폼표시);
        Assert.True(page.상세.대상없음);
        Assert.Equal(88, page.작성.등록응답?.Id);
    }

    private sealed record RecordedRequest(HttpMethod Method, string Path, object? Request);

    private sealed class ScenarioJsonApiClient : ISsalddelJsonApiClient
    {
        public Func<HttpMethod, string, Type, object?, object?> Responder { get; init; }
            = (_, _, _, _) => null;

        public List<RecordedRequest> Requests { get; } = [];
        public RecordedRequest Last => Requests[^1];

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
            => Respond<TResponse>(HttpMethod.Get, path, null);

        public Task<TResponse?> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => Respond<TResponse>(method, path, null);

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => Respond<TResponse>(method, path, request);

        public Task SendAsync(
            HttpMethod method,
            string path,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            Record(method, path, null, typeof(object));
            return Task.CompletedTask;
        }

        public Task SendAsync<TRequest>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            Record(method, path, request, typeof(object));
            return Task.CompletedTask;
        }

        private Task<TResponse?> Respond<TResponse>(HttpMethod method, string path, object? request)
        {
            var response = Record(method, path, request, typeof(TResponse));
            return Task.FromResult(response is null ? default : (TResponse)response);
        }

        private object? Record(HttpMethod method, string path, object? request, Type responseType)
        {
            Requests.Add(new RecordedRequest(method, path, request));
            return Responder(method, path, responseType, request);
        }
    }
}
