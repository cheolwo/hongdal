namespace Ssalddel.Contracts.Common.Mart;

/// <summary>창고 앱과 통합 Web이 공유하는 마트 피킹 List·Detail route입니다.</summary>
public static class MartPickingPageRoutes
{
    public const string AppRoot = "/mart/picking";
    public const string AppDetailRoot = $"{AppRoot}/orders";
    public const string AppDetailTemplate = $"{AppDetailRoot}/{{OrderId:long}}";

    public const string WebRoot = "/warehouse/mart/picking";
    public const string WebDetailRoot = $"{WebRoot}/orders";
    public const string WebDetailTemplate = $"{WebDetailRoot}/{{OrderId:long}}";

    public static string AppDetailFor(long orderId)
        => $"{AppDetailRoot}/{RequireOrderId(orderId)}";

    public static string WebDetailFor(long orderId)
        => $"{WebDetailRoot}/{RequireOrderId(orderId)}";

    private static long RequireOrderId(long orderId)
        => orderId > 0
            ? orderId
            : throw new ArgumentOutOfRangeException(nameof(orderId), "마트 주문 ID는 1 이상이어야 합니다.");
}
