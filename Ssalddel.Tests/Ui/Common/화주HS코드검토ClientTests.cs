using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 화주HS코드검토ClientTests
{
    [Fact]
    public async Task 목록Client는_검색어를인코딩하고보호된화주경로를호출한다()
    {
        var api = new RecordingJsonApiClient
        {
            Response = new 화주HS코드검토목록응답()
        };
        var client = new 화주HS코드검토Client(api);

        await client.목록조회Async("의자 & 가구", 20, 2, 30);

        Assert.Equal(
            "api/v1/shipper/customs/hs-reviews?page=2&pageSize=30&query=%EC%9D%98%EC%9E%90%20%26%20%EA%B0%80%EA%B5%AC&businessCategory=20",
            api.LastPath);
        Assert.False(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 상세Client는_정확한ReviewId경로에서404만없음으로허용한다()
    {
        var api = new RecordingJsonApiClient { Response = null };
        var client = new 화주HS코드검토Client(api);

        var result = await client.상세조회Async(73);

        Assert.Null(result);
        Assert.Equal("api/v1/shipper/customs/hs-reviews/73", api.LastPath);
        Assert.True(api.LastAllowNotFound);
    }

    [Fact]
    public async Task 접근Service는_서버버전메타데이터의통관기능상태를사용한다()
    {
        var api = new RecordingJsonApiClient
        {
            Response = new VersionFeatureFlagsResponse
            {
                Flags = new Dictionary<string, bool>
                {
                    ["CustomsAndTradeDataWorkflow"] = true
                }
            }
        };
        var service = new 화주HS코드검토접근Service(api);

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
