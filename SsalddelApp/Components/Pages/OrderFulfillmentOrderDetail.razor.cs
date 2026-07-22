using Microsoft.AspNetCore.Components;
using Ssalddel.Contracts.Common.Sales;
using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentOrderDetail
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public string OrderKey { get; set; } = string.Empty;

    private OrderFulfillmentReadViewModel Read => ViewModel.조회;
    private bool InvalidOrderKey { get; set; }
    private string? ChannelType { get; set; }
    private string? ChannelOrderNo { get; set; }
    private string ReturnPath
        => FulfillmentOrderNavigationContext.Parse(Navigation.Uri)
            .ResolveReturnPath();

    protected override Task OnParametersSetAsync()
    {
        ChannelType = null;
        ChannelOrderNo = null;
        InvalidOrderKey = !OrderFulfillmentSimulationPageRoutes.TryDecodeOrderKey(
            OrderKey,
            out var channelType,
            out var channelOrderNo);
        if (!InvalidOrderKey)
        {
            ChannelType = channelType;
            ChannelOrderNo = channelOrderNo;
        }

        return RefreshAndSelectAsync();
    }

    private async Task RefreshAndSelectAsync()
    {
        if (!await ViewModel.새로고침Async()
            || InvalidOrderKey
            || ChannelType is null
            || ChannelOrderNo is null)
        {
            return;
        }

        Read.주문선택(ChannelType, ChannelOrderNo);
    }
}
