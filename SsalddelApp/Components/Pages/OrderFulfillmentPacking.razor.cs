using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentPacking
{
    private OrderFulfillmentReadViewModel Read => ViewModel.조회;

    protected override Task OnInitializedAsync()
        => ViewModel.새로고침Async();

    private Task RefreshAsync()
        => ViewModel.새로고침Async();
}
