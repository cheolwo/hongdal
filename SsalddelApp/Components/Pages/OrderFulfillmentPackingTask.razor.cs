using Microsoft.AspNetCore.Components;
using SsalddelApp.Services.Warehouse.Fulfillment;
using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentPackingTask
{
    [Parameter]
    public long TaskId { get; set; }

    private OrderFulfillmentReadViewModel Read => ViewModel.조회;
    private OrderFulfillmentPackingViewModel Packing => ViewModel.포장;
    private WarehousePackingTask? SelectedTask
        => Read.스냅샷.포장작업.FirstOrDefault(item => item.Id == TaskId);

    protected override Task OnParametersSetAsync()
        => ViewModel.새로고침Async();

    private Task RefreshAsync()
        => ViewModel.새로고침Async();

    private Task StartPackingAsync(long taskId)
        => ViewModel.포장시작Async(taskId);

    private Task CompletePackingAsync(long taskId)
        => ViewModel.포장완료Async(taskId);
}
