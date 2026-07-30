namespace Ssalddel.WebApp.Services;

public static class DriverRoutes
{
    public const string Home = "/driver/home";
    public const string WorkStart = "/driver/work/start";
    public const string WorkSettings = "/driver/work/settings";
    public const string Recommendations = "/driver/recommendations";
    public const string CommunityRequests = "/driver/community-requests";
    public const string ExplorationCampaigns = "/driver/exploration/campaigns";
    public const string Reservations = "/driver/reservations";
    public const string DispatchDecisions = "/driver/dispatch-decisions";
    public const string CurrentTransport = "/driver/transports/current";
    public const string TransportHistory = "/driver/transports/history";
    public const string ProofStageSelector = "/driver/transport/proof";
    public const string Settlements = "/driver/settlements/current-month";
    public const string Notifications = "/driver/notifications";
    public const string NotificationSettings = "/driver/notifications/settings";

    public static string RecommendationFor(string requestId)
        => $"{Recommendations}/{Escape(requestId)}";

    public static string DispatchDecisionFor(string requestId)
        => $"{DispatchDecisions}/{Escape(requestId)}";

    public static string CurrentTransportFor(string requestId)
        => $"{CurrentTransport}?acceptedRequestId={Escape(requestId)}";

    public static string PickupFor(long transportId)
        => $"/driver/transports/{transportId}/pickup";

    public static string DropoffFor(long transportId)
        => $"/driver/transports/{transportId}/dropoff";

    public static string ProofFor(long transportId)
        => $"{ProofStageSelector}?transportId={transportId}";

    private static string Escape(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("운송 의뢰 ID가 필요합니다.", nameof(value));
        }

        return Uri.EscapeDataString(value.Trim());
    }
}
