using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 판매채널계정읽기ClientTests
{
    [Fact]
    public async Task 목록Client는_404를빈목록으로숨기지않는보호경로를사용한다()
    {
        var api = new RecordingJsonApiClient
        {
            Response = new 판매채널계정목록응답
            {
                Items = [new 판매채널계정항목응답 { Id = 7 }]
            }
        };
        var client = new 판매채널Client(api);

        var result = await client.계정목록조회Async();

        Assert.Equal(7, Assert.Single(result).Id);
        Assert.Equal("api/v1/sales-channels/accounts", api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한AccountId경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 판매채널Client(api);

        var result = await client.계정상세조회Async(73);

        Assert.Null(result);
        Assert.Equal("api/v1/sales-channels/accounts/73", api.LastPath);
        Assert.True(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 접근Service는_서버메타데이터의판매채널기능상태를사용한다()
    {
        var api = new RecordingJsonApiClient
        {
            Response = new VersionFeatureFlagsResponse
            {
                Flags = new Dictionary<string, bool>
                {
                    ["SalesChannelFulfillmentWorkflow"] = true
                }
            }
        };
        var service = new 판매채널페이지접근Service(api);

        var enabled = await service.기능활성여부Async();

        Assert.True(enabled);
        Assert.Equal("api/v1/version-feature-flags", api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public object? Response { get; init; }
        public string? LastPath { get; private set; }
        public bool LastAllowNotFound { get; private set; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            LastPath = path;
            LastAllowNotFound = allowNotFound;
            return Task.FromResult((TResponse?)Response);
        }

        public Task<TResponse?> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TResponse?> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            bool allowNotFound = false,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync(
            HttpMethod method,
            string path,
            string operationName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendAsync<TRequest>(
            HttpMethod method,
            string path,
            TRequest request,
            string operationName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
