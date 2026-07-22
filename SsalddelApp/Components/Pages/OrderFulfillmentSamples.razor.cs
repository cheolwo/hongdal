using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentSamples
{
    private OrderFulfillmentReadViewModel Read => ViewModel.조회;
    private OrderFulfillmentSimulationViewModel Simulation => ViewModel.Simulation;

    protected override Task OnInitializedAsync()
        => ViewModel.새로고침Async();

    private Task RefreshAsync()
        => ViewModel.새로고침Async();

    private Task RunSimulationAsync()
        => ViewModel.Simulation실행Async();
}
