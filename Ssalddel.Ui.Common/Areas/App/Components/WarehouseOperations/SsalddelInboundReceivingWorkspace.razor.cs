using Microsoft.AspNetCore.Components;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.WarehouseOperations;

public partial class SsalddelInboundReceivingWorkspace
{
    [Parameter]
    public long? InitialWarehouseId { get; set; }

    [Parameter]
    public long? InboundId { get; set; }

    [Parameter]
    public string? ExpectedInboundPath { get; set; }

    [Parameter]
    public string? WorkBoardPath { get; set; }

    [Parameter]
    public string? InspectionPath { get; set; }

    [Parameter]
    public EventCallback<long?> OnInboundSelected { get; set; }

    private bool _initialized;
    private long? _loadedWarehouseId;
    private long? _loadedInboundId;

    private bool IsWorkflowReady
        => ViewModel.상태 is not (PageViewModel상태.대기 or PageViewModel상태.불러오는중 or PageViewModel상태.실패)
           && !ViewModel.창고.오류발생
           && !ViewModel.창고.비어있음;

    private string? WorkBoardHref
        => InboundReceivingPresentation.WorkBoardHref(
            WorkBoardPath,
            ViewModel.상세.항목?.Id);

    private string? InspectionHref
        => !string.IsNullOrWhiteSpace(InspectionPath)
           && ViewModel.수령.완료된입고상품Id is > 0
            ? $"{InspectionPath.TrimEnd('/')}?inboundItemId={ViewModel.수령.완료된입고상품Id.Value}"
            : null;

    protected override async Task OnParametersSetAsync()
    {
        var normalizedWarehouseId = NormalizeId(InitialWarehouseId);
        var normalizedInboundId = NormalizeId(InboundId);
        if (!_initialized)
        {
            _initialized = true;
            _loadedWarehouseId = normalizedWarehouseId;
            _loadedInboundId = normalizedInboundId;
            await ViewModel.초기화Async(_loadedWarehouseId, _loadedInboundId);
            return;
        }

        if (normalizedWarehouseId == _loadedWarehouseId
            && normalizedInboundId == _loadedInboundId)
        {
            return;
        }

        _loadedWarehouseId = normalizedWarehouseId;
        _loadedInboundId = normalizedInboundId;
        await ViewModel.경로변경Async(_loadedWarehouseId, _loadedInboundId);
    }

    private async Task RetryWarehousesAsync()
        => await ViewModel.초기화Async(NormalizeId(InitialWarehouseId), _loadedInboundId);

    private async Task WarehouseChangedAsync(long? warehouseId)
    {
        if (!ViewModel.창고선택(warehouseId))
        {
            return;
        }

        await ClearSelectedInboundAsync();
    }

    private async Task ProductBarcodeChangedAsync(string value)
    {
        var hadSelection = ViewModel.상세.항목 is not null;
        ViewModel.상품바코드변경(value);
        if (hadSelection)
        {
            await ClearSelectedInboundAsync();
        }
    }

    private async Task SearchAsync()
        => await ViewModel.검색Async();

    private async Task ClearSearchAsync()
    {
        ViewModel.상품바코드변경(string.Empty);
        await ClearSelectedInboundAsync();
    }

    private void StartUnplannedInbound()
        => ViewModel.현장입고작성시작();

    private void CancelUnplannedInbound()
        => ViewModel.작성.닫기();

    private async Task SaveUnplannedInboundAsync()
    {
        if (!await ViewModel.현장입고등록후조회Async()
            || ViewModel.상세.항목 is not { } item)
        {
            return;
        }

        _loadedInboundId = item.Id;
        await OnInboundSelected.InvokeAsync(item.Id);
    }

    private async Task SelectInboundAsync(long inboundId)
    {
        if (!await ViewModel.입고선택Async(inboundId))
        {
            return;
        }

        _loadedInboundId = inboundId;
        await OnInboundSelected.InvokeAsync(inboundId);
    }

    private Task RecordReceiptAsync()
        => ViewModel.수령기록후재조회Async();

    private async Task ClearSelectedInboundAsync()
    {
        _loadedInboundId = null;
        await OnInboundSelected.InvokeAsync(null);
    }

    private static long? NormalizeId(long? id)
        => id is > 0 ? id : null;
}
