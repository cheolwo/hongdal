namespace Ssalddel.Contracts.Common.Warehouse;

public static class LogisticsProxySiteTypes
{
    public const string DeliveryAgency = "DeliveryAgency";
    public const string UrbanLogisticsCenter = "UrbanLogisticsCenter";
    public const string MarketFulfillment = "MarketFulfillment";
    public const string OverseasCustomsAgency = "OverseasCustomsAgency";

    public static string GetDisplayName(string? siteType)
        => siteType switch
        {
            DeliveryAgency => "배송 대행지",
            UrbanLogisticsCenter => "도심 생활물류센터",
            MarketFulfillment => "마켓 물류 대행지",
            OverseasCustomsAgency => "해외 통관 배송 대행지",
            _ => "배송 대행지"
        };

    public static bool RequiresCustoms(string? siteType)
        => string.Equals(siteType, OverseasCustomsAgency, StringComparison.Ordinal);

    public static bool IsValid(string? siteType)
        => siteType is DeliveryAgency
            or UrbanLogisticsCenter
            or MarketFulfillment
            or OverseasCustomsAgency;

    public static string Normalize(string? siteType)
        => IsValid(siteType) ? siteType! : DeliveryAgency;
}
