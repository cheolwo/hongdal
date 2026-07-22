using Microsoft.AspNetCore.Components;
using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentPickingTask
{
    [Parameter]
    public long TaskId { get; set; }

    private OrderFulfillmentReadViewModel Read => ViewModel.조회;
    private OrderFulfillmentPickingViewModel Picking => ViewModel.피킹;

    protected override Task OnParametersSetAsync()
        => RefreshAndSelectAsync();

    private async Task RefreshAndSelectAsync()
    {
        Picking.선택작업Id = TaskId;
        await ViewModel.새로고침Async();
    }

    private Task ScanPickingAsync()
        => ViewModel.피킹스캔Async();

    private Task HoldPickingAsync()
        => ViewModel.피킹보류Async();

    private Task CancelPickingAsync()
        => ViewModel.피킹취소Async();
}
