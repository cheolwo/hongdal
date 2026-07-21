using Microsoft.AspNetCore.Components;
using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillment
{
    private OrderFulfillmentReadViewModel Read => ViewModel.조회;
    private OrderFulfillmentSimulationViewModel Simulation => ViewModel.Simulation;
    private OrderFulfillmentRestockPolicyViewModel RestockPolicy => ViewModel.입고알림정책;
    private OrderFulfillmentPickingViewModel Picking => ViewModel.피킹;
    private OrderFulfillmentPackingViewModel Packing => ViewModel.포장;

    protected override async Task OnInitializedAsync()
        => await ViewModel.새로고침Async();

    private async Task RefreshAsync()
        => await ViewModel.새로고침Async();

    private async Task RunSimulationAsync()
        => await ViewModel.Simulation실행Async();

    private async Task UpdateRestockPolicyAsync(OrderFulfillmentRestockPreferenceUpdate request)
        => await ViewModel.입고알림정책저장Async(request);

    private async Task ScanPickingAsync()
        => await ViewModel.피킹스캔Async();

    private async Task HoldPickingAsync()
        => await ViewModel.피킹보류Async();

    private async Task CancelPickingAsync()
        => await ViewModel.피킹취소Async();

    private async Task StartPackingAsync(long taskId)
        => await ViewModel.포장시작Async(taskId);

    private async Task CompletePackingAsync(long taskId)
        => await ViewModel.포장완료Async(taskId);
}
