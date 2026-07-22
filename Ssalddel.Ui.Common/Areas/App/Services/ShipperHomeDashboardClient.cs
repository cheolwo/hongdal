using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public static class ShipperHomeFeatureKeys
{
    public const string DomesticTransport = "DomesticTransportWorkflow";
    public const string WarehouseFulfillment = "WarehouseFulfillmentWorkflow";
    public const string SalesChannelFulfillment = "SalesChannelFulfillmentWorkflow";
    public const string CustomsAndTradeData = "CustomsAndTradeDataWorkflow";
}

public sealed record ShipperHomeRequestSummary(
    string RequestId,
    string RequestStatus,
    string PaymentStatus,
    string DispatchStatus,
    DateTime CreatedAtUtc);

public sealed record ShipperHomeDashboardSnapshot(
    bool IsAuthenticated,
    string IdentityLabel,
    bool FeatureMetadataAvailable,
    IReadOnlyDictionary<string, bool> FeatureFlags,
    ShipperHomeRequestSummary? LatestRequest,
    int ActiveRequestCount,
    int PendingInboundCount,
    int ActionableInventoryCount,
    IReadOnlyList<string> Warnings)
{
    public static ShipperHomeDashboardSnapshot Empty { get; } = new(
        false,
        "방문자",
        false,
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
        null,
        0,
        0,
        0,
        []);

    public bool IsFeatureEnabled(string featureKey)
        => FeatureFlags.Any(pair =>
            string.Equals(pair.Key, featureKey, StringComparison.OrdinalIgnoreCase)
            && pair.Value);
}

public interface IShipperHomeDashboardClient
{
    Task<ShipperHomeDashboardSnapshot> LoadAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 화주 허브에 필요한 읽기 전용 요약과 기능 플래그만 조회합니다.
/// 비활성 workflow의 업무 API는 호출하지 않습니다.
/// </summary>
public sealed class ShipperHomeDashboardClient(
    ISsalddelJsonApiClient apiClient,
    ISsalddel현재사용자Context currentUserContext)
    : IShipperHomeDashboardClient
{
    private const string FeatureMetadataPath = "api/v1/version-feature-flags";
    private const string InboundsPath = "api/v1/warehouse-operations/inbounds";
    private const string InventoryPath = "api/v1/warehouse-operations/inventory";

    public async Task<ShipperHomeDashboardSnapshot> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        var user = currentUserContext.현재사용자;
        var identityLabel = string.IsNullOrWhiteSpace(user.UserName)
            ? user.인증됨 ? "로그인 사용자" : "방문자"
            : user.UserName.Trim();
        var warnings = new List<string>();

        VersionFeatureFlagsResponse? metadata;
        try
        {
            metadata = await apiClient.GetAsync<VersionFeatureFlagsResponse>(
                FeatureMetadataPath,
                "화주 허브 기능 상태 조회",
                allowNotFound: false,
                cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            warnings.Add("업무 기능 상태를 확인하지 못해 1.0 이후 도구를 안전하게 비활성으로 표시합니다.");
            return EmptyFor(user.인증됨, identityLabel, warnings);
        }

        if (metadata is null)
        {
            warnings.Add("업무 기능 상태 응답이 비어 있어 1.0 이후 도구를 안전하게 비활성으로 표시합니다.");
            return EmptyFor(user.인증됨, identityLabel, warnings);
        }

        var flags = new Dictionary<string, bool>(metadata.Flags, StringComparer.OrdinalIgnoreCase);
        if (!user.인증됨 || string.IsNullOrWhiteSpace(user.UserId))
        {
            return new(
                false,
                identityLabel,
                true,
                flags,
                null,
                0,
                0,
                0,
                warnings);
        }

        var requests = await LoadRequestsAsync(user.UserId, flags, warnings, cancellationToken);
        var (inbounds, inventory) = await LoadWarehouseAsync(flags, warnings, cancellationToken);
        var latestRequest = requests
            .OrderByDescending(item => item.생성일시)
            .Select(ToSummary)
            .FirstOrDefault();

        return new(
            true,
            identityLabel,
            true,
            flags,
            latestRequest,
            requests.Count(IsActiveRequest),
            inbounds.Count(item => string.Equals(item.상태, "입고예정", StringComparison.OrdinalIgnoreCase)),
            inventory.Count(item => item.가용수량 > 0),
            warnings);
    }

    private async Task<IReadOnlyList<화주운송의뢰응답>> LoadRequestsAsync(
        string userId,
        IReadOnlyDictionary<string, bool> flags,
        ICollection<string> warnings,
        CancellationToken cancellationToken)
    {
        if (!IsEnabled(flags, ShipperHomeFeatureKeys.DomesticTransport))
        {
            return [];
        }

        try
        {
            return await apiClient.GetAsync<IReadOnlyList<화주운송의뢰응답>>(
                       $"api/v1/shipper/requests?shipperId={Uri.EscapeDataString(userId.Trim())}",
                       "화주 허브 운송 요약 조회",
                       allowNotFound: false,
                       cancellationToken)
                   ?? [];
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            warnings.Add("운송 의뢰 요약을 불러오지 못했습니다. 운송 업무 화면에서 다시 시도해 주세요.");
            return [];
        }
    }

    private async Task<(IReadOnlyList<입고요청항목응답> Inbounds, IReadOnlyList<재고항목응답> Inventory)> LoadWarehouseAsync(
        IReadOnlyDictionary<string, bool> flags,
        ICollection<string> warnings,
        CancellationToken cancellationToken)
    {
        if (!IsEnabled(flags, ShipperHomeFeatureKeys.WarehouseFulfillment))
        {
            return ([], []);
        }

        try
        {
            var inboundsTask = apiClient.GetAsync<입고요청목록응답>(
                InboundsPath,
                "화주 허브 입고 요약 조회",
                allowNotFound: false,
                cancellationToken);
            var inventoryTask = apiClient.GetAsync<재고목록응답>(
                InventoryPath,
                "화주 허브 재고 요약 조회",
                allowNotFound: false,
                cancellationToken);
            await Task.WhenAll(inboundsTask, inventoryTask);
            return ((await inboundsTask)?.Items ?? [], (await inventoryTask)?.Items ?? []);
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            warnings.Add("입고·재고 요약을 불러오지 못했습니다. 창고 업무 화면에서 다시 시도해 주세요.");
            return ([], []);
        }
    }

    private static ShipperHomeDashboardSnapshot EmptyFor(
        bool isAuthenticated,
        string identityLabel,
        IReadOnlyList<string> warnings)
        => ShipperHomeDashboardSnapshot.Empty with
        {
            IsAuthenticated = isAuthenticated,
            IdentityLabel = identityLabel,
            Warnings = warnings
        };

    private static bool IsEnabled(IReadOnlyDictionary<string, bool> flags, string featureKey)
        => flags.Any(pair =>
            string.Equals(pair.Key, featureKey, StringComparison.OrdinalIgnoreCase)
            && pair.Value);

    private static bool IsActiveRequest(화주운송의뢰응답 item)
        => !Matches(item.의뢰상태, "취소", "환불")
           && !Matches(item.배차상태, "배송완료", "하차완료", "완료");

    private static ShipperHomeRequestSummary ToSummary(화주운송의뢰응답 item)
        => new(
            item.의뢰Id,
            item.의뢰상태,
            item.결제상태,
            item.배차상태,
            item.생성일시);

    private static bool Matches(string? value, params string[] keywords)
        => !string.IsNullOrWhiteSpace(value)
           && keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
