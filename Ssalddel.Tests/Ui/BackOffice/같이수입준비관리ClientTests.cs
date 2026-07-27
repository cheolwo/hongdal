using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.BackOffice.Services;

namespace Ssalddel.Tests.Ui.BackOffice;

public sealed class 같이수입준비관리ClientTests
{
    [Fact]
    public async Task 작업대목록은_확정검토와확정집단을합치고_중복을제거한다()
    {
        var api = new RecordingApiClient();
        api.GetResponses[$"api/v1/orderer/group-purchase-auto-groups?currentStatus={공동구매자동집단상태코드.확정대기}"] =
            new 공동구매자동집단요약응답[]
            {
                Group("ready", 공동구매자동집단상태코드.확정대기, new DateTime(2026, 7, 23, 1, 0, 0, DateTimeKind.Utc)),
                Group("duplicate", 공동구매자동집단상태코드.확정대기, new DateTime(2026, 7, 23, 2, 0, 0, DateTimeKind.Utc))
            };
        api.GetResponses[$"api/v1/orderer/group-purchase-auto-groups?currentStatus={공동구매자동집단상태코드.확정}"] =
            new 공동구매자동집단요약응답[]
            {
                Group("confirmed", 공동구매자동집단상태코드.확정, new DateTime(2026, 7, 23, 3, 0, 0, DateTimeKind.Utc)),
                Group("duplicate", 공동구매자동집단상태코드.확정, new DateTime(2026, 7, 23, 1, 0, 0, DateTimeKind.Utc))
            };
        var client = new 같이수입준비관리Client(api);

        var result = await client.작업대목록조회Async();

        Assert.Equal(["duplicate", "ready", "confirmed"], result.Select(item => item.자동집단Id).ToArray());
        Assert.Equal(2, api.GetCalls.Count);
        Assert.All(api.GetCalls, call => Assert.False(call.AllowNotFound));
    }

    [Fact]
    public async Task 승인과원장저장은_같은집단경로와멱등헤더를사용한다()
    {
        var api = new RecordingApiClient
        {
            SendResponse = new 공동구매수요모집인계승인응답(),
            HeaderSendResponse = new 같이수입준비원장응답()
        };
        var client = new 같이수입준비관리Client(api);
        var approval = new 공동구매수요모집인계승인요청
        {
            요청멱등키 = "handoff-key",
            승인사유 = "수요 근거 확인"
        };
        var save = new 같이수입준비원장저장요청 { 요청멱등키 = "readiness-key" };

        await client.인계승인Async("group/1", approval);
        await client.저장Async("group/1", save);

        Assert.Equal(HttpMethod.Post, api.HeaderCalls[0].Method);
        Assert.Equal(
            "api/v1/admin/orderer/group-purchase-demand-os/groups/group%2F1/handoff-approval",
            api.HeaderCalls[0].Path);
        Assert.Equal("handoff-key", api.HeaderCalls[0].Headers["Idempotency-Key"]);
        Assert.Equal(HttpMethod.Put, api.HeaderCalls[1].Method);
        Assert.Equal(
            "api/v1/admin/orderer/group-purchase-demand-os/groups/group%2F1/trade-readiness",
            api.HeaderCalls[1].Path);
        Assert.Equal("readiness-key", api.HeaderCalls[1].Headers["Idempotency-Key"]);
    }

    [Fact]
    public async Task Os조회_작업실행_전문검토인계는_준비원장하위경로와멱등헤더를사용한다()
    {
        const string osPath = "api/v1/admin/orderer/group-purchase-demand-os/groups/group%2F1/trade-readiness/os";
        var response = new 같이수입준비Os상태응답();
        var api = new RecordingApiClient
        {
            SendResponse = response,
            HeaderSendResponse = response
        };
        api.GetResponses[osPath] = response;
        var client = new 같이수입준비관리Client(api);

        await client.준비Os상태조회Async("group/1");
        await client.준비Os작업실행Async("group/1", new 같이수입준비Os작업실행요청
        {
            요청멱등키 = "os-run-key"
        });
        await client.전문검토인계Async("group/1", new 같이수입준비Os전문검토인계요청
        {
            요청멱등키 = "review-handoff-key"
        });

        Assert.Contains(api.GetCalls, call => call.Path == osPath && call.AllowNotFound);
        Assert.Equal(
            $"{osPath}/workloads/run",
            api.HeaderCalls[0].Path);
        Assert.Equal("os-run-key", api.HeaderCalls[0].Headers["Idempotency-Key"]);
        Assert.Equal(
            $"{osPath}/qualified-review-handoff",
            api.HeaderCalls[1].Path);
        Assert.Equal("review-handoff-key", api.HeaderCalls[1].Headers["Idempotency-Key"]);
    }

    private static 공동구매자동집단요약응답 Group(string id, string state, DateTime updatedAt)
        => new()
        {
            자동집단Id = id,
            상품키 = $"product-{id}",
            상품명 = id,
            현재상태 = state,
            수정시각Utc = updatedAt
        };

    private sealed class RecordingApiClient : ISsalddelJsonApiClient
    {
        public Dictionary<string, object?> GetResponses { get; } = new(StringComparer.Ordinal);
        public List<(string Path, bool AllowNotFound)> GetCalls { get; } = [];
        public List<(HttpMethod Method, string Path, IReadOnlyDictionary<string, string> Headers)> HeaderCalls { get; } = [];
        public object? SendResponse { get; set; }
        public object? HeaderSendResponse { get; set; }

        public Task<TResponse?> GetAsync<TResponse>(string path, string operationName, bool allowNotFound = true, CancellationToken cancellationToken = default)
        {
            GetCalls.Add((path, allowNotFound));
            GetResponses.TryGetValue(path, out var response);
            return Task.FromResult(response is null ? default : (TResponse)response);
        }

        public Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string path, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
            => Task.FromResult(SendResponse is null ? default : (TResponse)SendResponse);

        public Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest request, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
            => Task.FromResult(SendResponse is null ? default : (TResponse)SendResponse);

        public Task<TResponse?> SendWithHeadersAsync<TResponse>(HttpMethod method, string path, IReadOnlyDictionary<string, string> headers, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
        {
            HeaderCalls.Add((method, path, headers));
            return Task.FromResult(HeaderSendResponse is null ? default : (TResponse)HeaderSendResponse);
        }

        public Task<TResponse?> SendWithHeadersAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest request, IReadOnlyDictionary<string, string> headers, string operationName, bool allowNotFound = false, CancellationToken cancellationToken = default)
        {
            HeaderCalls.Add((method, path, headers));
            var response = HeaderCalls.Count == 1 ? SendResponse : HeaderSendResponse;
            return Task.FromResult(response is null ? default : (TResponse)response);
        }

        public Task SendAsync(HttpMethod method, string path, string operationName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SendAsync<TRequest>(HttpMethod method, string path, TRequest request, string operationName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
