using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentRestockPolicy
{
    private OrderFulfillmentReadViewModel Read => ViewModel.조회;
    private OrderFulfillmentRestockPolicyViewModel RestockPolicy => ViewModel.입고알림정책;

    protected override Task OnInitializedAsync()
        => ViewModel.새로고침Async();

    private Task RefreshAsync()
        => ViewModel.새로고침Async();

    private Task UpdateRestockPolicyAsync(OrderFulfillmentRestockPreferenceUpdate request)
        => ViewModel.입고알림정책저장Async(request);
}
