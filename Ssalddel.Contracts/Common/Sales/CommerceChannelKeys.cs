namespace Ssalddel.Contracts.Common.Sales;

public static class CommerceChannelKeys
{
    public const string SmartStore = "SmartStore";
    public const string Coupang = "Coupang";
    public const string ElevenStreet = "ElevenStreet";
    public const string Shopify = "Shopify";
    public const string Amazon = "Amazon";
    public const string Ebay = "Ebay";
    public const string Walmart = "Walmart";
    public const string Etsy = "Etsy";
    public const string TikTokShop = "TikTokShop";
    public const string Shopee = "Shopee";
    public const string Lazada = "Lazada";
}

public static class CommerceChannelOrderSyncScopes
{
    public const string Domestic = "Domestic";
    public const string Overseas = "Overseas";

    public static readonly IReadOnlyList<string> DomesticChannelTypes =
    [
        CommerceChannelKeys.SmartStore,
        CommerceChannelKeys.Coupang,
        CommerceChannelKeys.ElevenStreet
    ];

    public static readonly IReadOnlyList<string> OverseasChannelTypes =
    [
        CommerceChannelKeys.Shopify,
        CommerceChannelKeys.Amazon,
        CommerceChannelKeys.Ebay,
        CommerceChannelKeys.Walmart,
        CommerceChannelKeys.Etsy,
        CommerceChannelKeys.TikTokShop,
        CommerceChannelKeys.Shopee,
        CommerceChannelKeys.Lazada
    ];

    public static string Resolve(string channelType)
        => channelType.Trim() switch
        {
            CommerceChannelKeys.SmartStore => Domestic,
            CommerceChannelKeys.Coupang => Domestic,
            CommerceChannelKeys.ElevenStreet => Domestic,
            _ => Overseas
        };

    public static IReadOnlyList<string> GetChannelTypes(string syncScope)
        => string.Equals(syncScope, Overseas, StringComparison.OrdinalIgnoreCase)
            ? OverseasChannelTypes
            : DomesticChannelTypes;
}
