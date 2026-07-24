using MudBlazor;

namespace Ssalddel.WebApp.Services;

public static class DriverNotificationPresentation
{
    public static string ResolveActionIcon(string? payloadType)
    {
        var type = DriverNotificationDeepLinkResolver.ResolveDisplayType(payloadType);
        return type switch
        {
            "신규 추천" => Icons.Material.Filled.Route,
            "추천 만료" => Icons.Material.Filled.ListAlt,
            "배차 확정" => Icons.Material.Filled.LocalShipping,
            "상차 요청" => Icons.Material.Filled.FileUpload,
            "하차 요청" => Icons.Material.Filled.TaskAlt,
            "정산" => Icons.Material.Filled.ReceiptLong,
            "알림 설정" => Icons.Material.Filled.Settings,
            _ => Icons.Material.Filled.Notifications
        };
    }

    public static string BuildPayloadSummary(
        string? payloadType,
        string? requestId,
        long? transportId)
    {
        var parts = new List<string> { $"type={payloadType ?? "-"}" };
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            parts.Add($"requestId={requestId.Trim()}");
        }

        if (transportId is > 0)
        {
            parts.Add($"transportId={transportId}");
        }

        return string.Join(" · ", parts);
    }
}
