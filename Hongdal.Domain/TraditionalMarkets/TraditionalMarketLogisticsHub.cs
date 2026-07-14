using Hongdal.Contracts.Common.TraditionalMarkets;

namespace Hongdal.Domain.TraditionalMarkets;

public sealed class TraditionalMarketLogisticsHub
{
    public string MarketCode { get; set; } = string.Empty;
    public string Status { get; set; } = TraditionalMarketLogisticsHubStatuses.Candidate;
    public string OperatorOrganizationName { get; set; } = string.Empty;
    public decimal ServiceRadiusKm { get; set; }
    public int DailyGroupPurchaseCapacity { get; set; }
    public bool SupportsBulkReceiving { get; set; }
    public bool SupportsSorting { get; set; }
    public bool SupportsResidentPickup { get; set; }
    public bool SupportsLastMileDelivery { get; set; }
    public bool SupportsRefrigeratedStorage { get; set; }
    public bool SupportsFrozenStorage { get; set; }
    public string ReceivingWindow { get; set; } = string.Empty;
    public string PickupWindow { get; set; } = string.Empty;
    public string OperatingNotes { get; set; } = string.Empty;
    public bool HasOperatorConsent { get; set; }
    public DateTime? OperatorConsentedAtUtc { get; set; }
    public DateTime? SiteVerifiedAtUtc { get; set; }
    public string SiteVerifiedByUserId { get; set; } = string.Empty;
    public string StatusReason { get; set; } = string.Empty;
    public string UpdatedByUserId { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime StatusChangedAtUtc { get; set; }
}

public static class TraditionalMarketLogisticsHubPolicy
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedTransitions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [TraditionalMarketLogisticsHubStatuses.Candidate] = Set(
                TraditionalMarketLogisticsHubStatuses.UnderReview,
                TraditionalMarketLogisticsHubStatuses.Closed),
            [TraditionalMarketLogisticsHubStatuses.UnderReview] = Set(
                TraditionalMarketLogisticsHubStatuses.Candidate,
                TraditionalMarketLogisticsHubStatuses.Pilot,
                TraditionalMarketLogisticsHubStatuses.Closed),
            [TraditionalMarketLogisticsHubStatuses.Pilot] = Set(
                TraditionalMarketLogisticsHubStatuses.Active,
                TraditionalMarketLogisticsHubStatuses.Paused,
                TraditionalMarketLogisticsHubStatuses.UnderReview),
            [TraditionalMarketLogisticsHubStatuses.Active] = Set(
                TraditionalMarketLogisticsHubStatuses.Paused),
            [TraditionalMarketLogisticsHubStatuses.Paused] = Set(
                TraditionalMarketLogisticsHubStatuses.Pilot,
                TraditionalMarketLogisticsHubStatuses.Active,
                TraditionalMarketLogisticsHubStatuses.Closed),
            [TraditionalMarketLogisticsHubStatuses.Closed] = Set(
                TraditionalMarketLogisticsHubStatuses.Candidate)
        };

    public static bool CanTransition(string currentStatus, string targetStatus)
        => AllowedTransitions.TryGetValue(currentStatus, out var targets)
           && targets.Contains(targetStatus);

    public static string? GetReadinessError(TraditionalMarketLogisticsHub hub)
    {
        if (string.IsNullOrWhiteSpace(hub.OperatorOrganizationName))
        {
            return "거점 운영주체가 지정되어야 합니다.";
        }

        if (!hub.HasOperatorConsent)
        {
            return "상인회 또는 운영주체의 동의가 필요합니다.";
        }

        if (!hub.SiteVerifiedAtUtc.HasValue)
        {
            return "거점 현장 확인이 필요합니다.";
        }

        if (!hub.SupportsBulkReceiving)
        {
            return "공동구매 물품의 묶음 입고가 가능해야 합니다.";
        }

        if (!hub.SupportsSorting)
        {
            return "입고 물품의 검수·분류가 가능해야 합니다.";
        }

        if (!hub.SupportsResidentPickup && !hub.SupportsLastMileDelivery)
        {
            return "주민 수령 또는 근거리 배송 중 하나 이상을 지원해야 합니다.";
        }

        if (hub.DailyGroupPurchaseCapacity <= 0)
        {
            return "일일 공동구매 처리 용량이 1건 이상이어야 합니다.";
        }

        if (hub.ServiceRadiusKm <= 0)
        {
            return "생활권 서비스 반경이 0보다 커야 합니다.";
        }

        return null;
    }

    private static IReadOnlySet<string> Set(params string[] values)
        => new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}
