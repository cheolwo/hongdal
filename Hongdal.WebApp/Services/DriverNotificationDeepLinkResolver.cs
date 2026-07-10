namespace Hongdal.WebApp.Services;

public static class DriverNotificationDeepLinkResolver
{
    public const string Recommendation = "DispatchRecommendation";
    public const string RecommendationExpired = "DispatchRecommendationExpired";
    public const string DispatchAccepted = "DispatchAccepted";
    public const string PickupReady = "TransportPickupReady";
    public const string DropoffReady = "TransportDropoffReady";
    public const string Settlement = "SettlementReady";
    public const string NotificationSettings = "NotificationSettings";

    public static string ResolveHref(string? payloadType, string? requestId, long? transportId)
    {
        var type = Normalize(payloadType);

        return type switch
        {
            Recommendation => HasValue(requestId)
                ? $"/driver/recommendations/{Uri.EscapeDataString(requestId!.Trim())}"
                : "/driver/recommendations",
            RecommendationExpired => "/driver/recommendations",
            DispatchAccepted => HasValue(requestId)
                ? $"/driver/transports/current?acceptedRequestId={Uri.EscapeDataString(requestId!.Trim())}"
                : "/driver/transports/current",
            PickupReady => transportId is > 0
                ? $"/driver/transports/{transportId.Value}/pickup"
                : "/driver/transports/current",
            DropoffReady => transportId is > 0
                ? $"/driver/transports/{transportId.Value}/dropoff"
                : "/driver/transports/current",
            Settlement => "/driver/settlements/current-month",
            NotificationSettings => "/driver/notifications/settings",
            _ => "/driver/notifications"
        };
    }

    public static string ResolveActionLabel(string? payloadType)
    {
        var type = Normalize(payloadType);

        return type switch
        {
            Recommendation => "추천 상세 열기",
            RecommendationExpired => "추천 목록 확인",
            DispatchAccepted => "현재 운송 확인",
            PickupReady => "상차 처리 열기",
            DropoffReady => "하차 처리 열기",
            Settlement => "정산 확인",
            NotificationSettings => "알림 설정 확인",
            _ => "알림함 보기"
        };
    }

    public static string ResolveDisplayType(string? payloadType)
    {
        var type = Normalize(payloadType);

        return type switch
        {
            Recommendation => "신규 추천",
            RecommendationExpired => "추천 만료",
            DispatchAccepted => "배차 확정",
            PickupReady => "상차 요청",
            DropoffReady => "하차 요청",
            Settlement => "정산",
            NotificationSettings => "알림 설정",
            _ => string.IsNullOrWhiteSpace(payloadType) ? "일반 알림" : payloadType.Trim()
        };
    }

    private static string Normalize(string? payloadType)
    {
        if (string.IsNullOrWhiteSpace(payloadType))
        {
            return string.Empty;
        }

        return payloadType.Trim() switch
        {
            "NewRecommendation" or "Recommendation" or "DispatchRecommendation" => Recommendation,
            "RecommendationExpired" or "DispatchRecommendationExpired" => RecommendationExpired,
            "Accepted" or "DispatchAccepted" or "TransportAssigned" => DispatchAccepted,
            "Pickup" or "PickupReminder" or "TransportPickupReady" or "TransportArrivedPickup" => PickupReady,
            "Dropoff" or "DropoffReminder" or "TransportDropoffReady" or "TransportArrivedDropoff" => DropoffReady,
            "Settlement" or "SettlementReady" or "TransportSettlementDepositReminder" => Settlement,
            "NotificationSettings" or "PushTokenRequired" => NotificationSettings,
            var value => value
        };
    }

    private static bool HasValue(string? value)
        => !string.IsNullOrWhiteSpace(value);
}
