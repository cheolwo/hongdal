using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Services.Shipper;

public sealed class ShipperHomeDashboardClientTests
{
    [Fact]
    public async Task 익명사용자는_기능상태만읽고_개인원장API를호출하지않는다()
    {
        var api = new RecordingJsonApiClient();
        api.Responses["api/v1/version-feature-flags"] = Metadata(
            (ShipperHomeFeatureKeys.DomesticTransport, true),
            (ShipperHomeFeatureKeys.WarehouseFulfillment, true));
        var client = new ShipperHomeDashboardClient(
            api,
            new Test현재사용자Context(현재사용자Snapshot.익명));

        var snapshot = await client.LoadAsync();

        Assert.False(snapshot.IsAuthenticated);
        Assert.True(snapshot.FeatureMetadataAvailable);
        Assert.True(snapshot.IsFeatureEnabled(ShipperHomeFeatureKeys.DomesticTransport));
        Assert.Single(api.Paths);
        Assert.Equal("api/v1/version-feature-flags", api.Paths[0]);
    }

    [Fact]
    public async Task 비활성workflow는_로그인상태에서도_업무API를호출하지않는다()
    {
        var api = new RecordingJsonApiClient();
        api.Responses["api/v1/version-feature-flags"] = Metadata(
            (ShipperHomeFeatureKeys.DomesticTransport, false),
            (ShipperHomeFeatureKeys.WarehouseFulfillment, false));
        var client = new ShipperHomeDashboardClient(
            api,
            AuthenticatedContext());

        var snapshot = await client.LoadAsync();

        Assert.True(snapshot.IsAuthenticated);
        Assert.True(snapshot.FeatureMetadataAvailable);
        Assert.Equal(0, snapshot.ActiveRequestCount);
        Assert.Equal(0, snapshot.PendingInboundCount);
        Assert.Equal(0, snapshot.ActionableInventoryCount);
        Assert.Single(api.Paths);
    }

    [Fact]
    public async Task 활성workflow만_읽기요약을조회하고_최근원장과건수를계산한다()
    {
        var api = new RecordingJsonApiClient();
        api.Responses["api/v1/version-feature-flags"] = Metadata(
            (ShipperHomeFeatureKeys.DomesticTransport, true),
            (ShipperHomeFeatureKeys.WarehouseFulfillment, true),
            (ShipperHomeFeatureKeys.SalesChannelFulfillment, false),
            (ShipperHomeFeatureKeys.CustomsAndTradeData, false));
        api.Responses["api/v1/shipper/requests?shipperId=user-17"] =
            new 화주운송의뢰응답[]
            {
                new()
                {
                    의뢰Id = "REQ-OLD",
                    의뢰상태 = "운송중",
                    결제상태 = "결제완료",
                    배차상태 = "운송중",
                    생성일시 = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    의뢰Id = "REQ-LATEST",
                    의뢰상태 = "완료",
                    결제상태 = "결제완료",
                    배차상태 = "배송완료",
                    생성일시 = new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc)
                }
            };
        api.Responses["api/v1/warehouse-operations/inbounds"] = new 입고요청목록응답
        {
            Items =
            [
                new 입고요청항목응답 { 상태 = "입고예정" },
                new 입고요청항목응답 { 상태 = "입고완료" }
            ]
        };
        api.Responses["api/v1/warehouse-operations/inventory"] = new 재고목록응답
        {
            Items =
            [
                new 재고항목응답 { 가용수량 = 3 },
                new 재고항목응답 { 가용수량 = 0 }
            ]
        };
        var client = new ShipperHomeDashboardClient(api, AuthenticatedContext());

        var snapshot = await client.LoadAsync();

        Assert.Equal("REQ-LATEST", snapshot.LatestRequest?.RequestId);
        Assert.Equal(1, snapshot.ActiveRequestCount);
        Assert.Equal(1, snapshot.PendingInboundCount);
        Assert.Equal(1, snapshot.ActionableInventoryCount);
        Assert.Contains("api/v1/shipper/requests?shipperId=user-17", api.Paths);
        Assert.Contains("api/v1/warehouse-operations/inbounds", api.Paths);
        Assert.Contains("api/v1/warehouse-operations/inventory", api.Paths);
        Assert.DoesNotContain(api.Paths, path => path.Contains("sales", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task 기능상태조회실패는_안전한비활성과재시도안내로변환한다()
    {
        var api = new RecordingJsonApiClient { FailurePath = "api/v1/version-feature-flags" };
        var client = new ShipperHomeDashboardClient(api, AuthenticatedContext());

        var snapshot = await client.LoadAsync();

        Assert.False(snapshot.FeatureMetadataAvailable);
        Assert.False(snapshot.IsFeatureEnabled(ShipperHomeFeatureKeys.DomesticTransport));
        Assert.Contains(snapshot.Warnings, warning => warning.Contains("비활성", StringComparison.Ordinal));
        Assert.Single(api.Paths);
    }

    [Fact]
    public async Task ViewModel은_조회실패를_error상태로보존하고_로딩을해제한다()
    {
        var viewModel = new ShipperHomePageViewModel(new ThrowingDashboardClient());

        await viewModel.LoadAsync();

        Assert.False(viewModel.IsLoading);
        Assert.False(viewModel.HasLoaded);
        Assert.Contains("불러오지 못했습니다", viewModel.ErrorMessage);
    }

    private static VersionFeatureFlagsResponse Metadata(params (string Key, bool Enabled)[] flags)
        => new()
        {
            Flags = flags.ToDictionary(item => item.Key, item => item.Enabled, StringComparer.OrdinalIgnoreCase)
        };

    private static Test현재사용자Context AuthenticatedContext()
        => new(new 현재사용자Snapshot("user-17", "화주17", ["Shipper"]));

    private sealed class Test현재사용자Context(현재사용자Snapshot snapshot)
        : ISsalddel현재사용자Context
    {
        public 현재사용자Snapshot 현재사용자 { get; } = snapshot;
    }

    private sealed class ThrowingDashboardClient : IShipperHomeDashboardClient
    {
        public Task<ShipperHomeDashboardSnapshot> LoadAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("test failure");
    }

    private sealed class RecordingJsonApiClient : ISsalddelJsonApiClient
    {
        public Dictionary<string, object> Responses { get; } = new(StringComparer.Ordinal);
        public List<string> Paths { get; } = [];
        public string? FailurePath { get; init; }

        public Task<TResponse?> GetAsync<TResponse>(
            string path,
            string operationName,
            bool allowNotFound = true,
            CancellationToken cancellationToken = default)
        {
            Paths.Add(path);
            if (string.Equals(path, FailurePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("test failure");
            }

            return Task.FromResult(
                Responses.TryGetValue(path, out var response)
                    ? (TResponse?)response
                    : default);
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
