using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentSamples
{
    protected override Task OnInitializedAsync()
        => ViewModel.새로고침Async();

    private Task RefreshAsync()
        => ViewModel.새로고침Async();

    private Task<bool> SyncDomesticAsync()
        => ViewModel.동기화Async(Ssalddel.Contracts.Common.Sales.CommerceChannelOrderSyncScopes.Domestic);
}
