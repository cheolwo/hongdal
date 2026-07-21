using MudBlazor;
using Ssalddel.Contracts.Common.Sales;
using SsalddelApp.Services.Commerce.Orders;

namespace SsalddelApp.Components.Pages;

public static class OrderFulfillmentPresentation
{
    public static string ChannelName(string? channelType)
        => channelType?.Trim() switch
        {
            CommerceChannelKeys.SmartStore => "네이버 스마트스토어",
            CommerceChannelKeys.Coupang => "쿠팡 Wing",
            CommerceChannelKeys.ElevenStreet => "11번가",
            CommerceChannelKeys.Shopify => "Shopify",
            CommerceChannelKeys.Amazon => "Amazon",
            CommerceChannelKeys.Ebay => "eBay",
            _ => ValueOrDash(channelType)
        };

    public static string ScopeName(string? scope)
        => scope switch
        {
            CommerceOrderScopeCodes.Domestic => "국내",
            CommerceOrderScopeCodes.International => "해외",
            _ => "구분 확인"
        };

    public static MudBlazor.Color StatusColor(string? status)
        => status switch
        {
            WarehouseOutboundNotificationStatusCodes.Ready => MudBlazor.Color.Success,
            WarehouseOutboundNotificationStatusCodes.Blocked => MudBlazor.Color.Warning,
            WarehouseOutboundNotificationStatusCodes.Packed => MudBlazor.Color.Info,
            WarehouseOutboundNotificationStatusCodes.Packing => MudBlazor.Color.Info,
            WarehouseOutboundNotificationStatusCodes.Picking => MudBlazor.Color.Primary,
            _ => MudBlazor.Color.Default
        };

    public static string FormatPickPlan(WarehouseOutboundNotification notification)
    {
        if (notification.PickPlan is null || notification.PickPlan.Instructions.Count == 0)
        {
            return "피킹 경로 없음";
        }

        var bins = string.Join(", ", notification.PickPlan.Instructions.Select(item => $"{item.BinCode} {item.PickQuantity:N0}개"));
        return notification.PickPlan.IsComplete
            ? bins
            : $"{bins} · 부족 {notification.PickPlan.ShortageQuantity:N0}개";
    }

    public static string DateLabel(DateTime value)
        => value == default ? "—" : value.ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
}
