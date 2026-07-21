using System.Text.Encodings.Web;
using System.Text.Json;
using Ssalddel.Contracts.Common.Sales;
using SsalddelApp.Services;
using SsalddelApp.Services.Commerce;

namespace SsalddelApp.Components.Pages;

public static class ProductListingPresentation
{
    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
    private static readonly IReadOnlyDictionary<string, string> ChannelNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [CommerceChannelKeys.SmartStore] = "네이버 스마트스토어",
            [CommerceChannelKeys.Coupang] = "쿠팡 Wing",
            [CommerceChannelKeys.Shopify] = "Shopify",
            [CommerceChannelKeys.Amazon] = "Amazon",
            [CommerceChannelKeys.Ebay] = "eBay",
            [CommerceChannelKeys.Walmart] = "Walmart Marketplace",
            [CommerceChannelKeys.Etsy] = "Etsy",
            [CommerceChannelKeys.TikTokShop] = "TikTok Shop",
            [CommerceChannelKeys.Shopee] = "Shopee",
            [CommerceChannelKeys.Lazada] = "Lazada",
            [CommerceChannelKeys.ElevenStreet] = "11번가"
        };

    public static string Money(decimal value) => $"{value:N0}원";

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value;

    public static string Payload(CommerceChannelListingPreparation preparation)
        => preparation.PayloadDraft?.ToJsonString(PayloadOptions) ?? "payload 매퍼가 아직 준비되지 않았습니다.";

    public static string ChannelName(string? channelType)
    {
        var key = channelType?.Trim();
        return !string.IsNullOrWhiteSpace(key) && ChannelNames.TryGetValue(key, out var displayName)
            ? displayName
            : ValueOrDash(channelType);
    }

    public static MudBlazor.Color StatusColor(string? status)
        => status == SalesStatusCodes.ProductActive ? MudBlazor.Color.Success : MudBlazor.Color.Default;

    public static MudBlazor.Color SyncColor(string? status)
        => status switch
        {
            SalesStatusCodes.SyncNormal => MudBlazor.Color.Success,
            SalesStatusCodes.SyncReady => MudBlazor.Color.Info,
            SalesStatusCodes.SyncPending => MudBlazor.Color.Warning,
            SalesStatusCodes.SyncManual => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Default
        };
}
