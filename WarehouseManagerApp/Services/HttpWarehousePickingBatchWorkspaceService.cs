using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace WarehouseManagerApp.Services;

public sealed class HttpWarehousePickingBatchWorkspaceService(
    I피킹작업페이지Service service) : IWarehousePickingBatchWorkspaceService
{
    public async Task<IReadOnlyList<WarehousePickingWarehouseOption>> GetWarehouseOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await LoadAllAsync(null, cancellationToken);
        return items
            .GroupBy(item => new { item.WarehouseId, item.WarehouseName })
            .Select(group => new WarehousePickingWarehouseOption(
                group.Key.WarehouseId,
                group.Key.WarehouseName,
                피킹포장처리방식.피킹포장분리,
                true,
                false,
                "서버에 배정된 피킹 작업을 기준으로 표시합니다."))
            .OrderBy(item => item.WarehouseName)
            .ToArray();
    }

    public async Task<IReadOnlyList<WarehousePickingTaskItem>> GetAssignedTasksAsync(
        long warehouseId,
        CancellationToken cancellationToken = default)
        => await LoadAllAsync(warehouseId, cancellationToken);

    public async Task<WarehousePickingWarehouseOption> UpdateWarehouseOptionAsync(
        long warehouseId,
        피킹포장처리방식 mode,
        bool isBarcodeRequired,
        CancellationToken cancellationToken = default)
    {
        var option = (await GetWarehouseOptionsAsync(cancellationToken))
            .FirstOrDefault(item => item.WarehouseId == warehouseId)
            ?? throw new InvalidOperationException("접근 가능한 창고의 피킹 작업을 찾지 못했습니다.");

        return option with
        {
            Mode = mode,
            IsBarcodeRequired = isBarcodeRequired,
            OperationMemo = "작업 옵션은 현재 서버 배정 원장을 따릅니다."
        };
    }

    public async Task<WarehouseRackScanResult> ScanRackAsync(
        long warehouseId,
        string rackBarcode,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(rackBarcode);
        var items = (await LoadAllAsync(warehouseId, cancellationToken))
            .Where(item => string.Equals(Normalize(item.RackBarcode), normalized, StringComparison.Ordinal))
            .ToArray();

        return items.Length == 0
            ? new WarehouseRackScanResult(false, "해당 적재대의 서버 피킹 작업을 찾지 못했습니다.", normalized, [])
            : new WarehouseRackScanResult(true, $"{items.Length}개 서버 피킹 작업을 찾았습니다.", normalized, items);
    }

    public async Task<WarehousePickCompleteResult> CompletePickAsync(
        string taskKey,
        string rackBarcode,
        string productBarcode,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var detail = await service.상세조회Async(taskKey, cancellationToken);
        if (detail is null)
        {
            return new WarehousePickCompleteResult(false, "피킹 작업을 찾지 못했습니다.", null);
        }

        var rackMatches = string.Equals(Normalize(detail.RackCode), Normalize(rackBarcode), StringComparison.Ordinal)
                          || string.Equals(Normalize(detail.StorageLocationCode), Normalize(rackBarcode), StringComparison.Ordinal);
        if (!rackMatches)
        {
            return new WarehousePickCompleteResult(false, "스캔한 적재대 위치가 서버 작업과 일치하지 않습니다.", Map(detail));
        }

        if (string.IsNullOrWhiteSpace(productBarcode) || quantity != detail.Quantity)
        {
            return new WarehousePickCompleteResult(false, "상품 확인과 서버 배정 수량 확인이 필요합니다.", Map(detail));
        }

        if (detail.CanStart)
        {
            await service.시작Async(taskKey, cancellationToken);
        }

        await service.완료Async(
            taskKey,
            new 피킹작업완료요청
            {
                RackCode = detail.RackCode,
                ProductConfirmed = true,
                QuantityConfirmed = true
            },
            cancellationToken);

        var refreshed = await service.상세조회Async(taskKey, cancellationToken);
        return new WarehousePickCompleteResult(
            refreshed is not null && !refreshed.CanComplete,
            "서버 피킹 완료 상태를 같은 작업 키로 다시 조회했습니다.",
            refreshed is null ? null : Map(refreshed));
    }

    private async Task<IReadOnlyList<WarehousePickingTaskItem>> LoadAllAsync(
        long? warehouseId,
        CancellationToken cancellationToken)
    {
        var page = await service.목록조회Async(
            new 피킹작업목록조회요청
            {
                WarehouseId = warehouseId,
                Status = 피킹작업조회상태코드.전체,
                Page = 0,
                PageSize = 50
            },
            cancellationToken);

        var result = new List<WarehousePickingTaskItem>(page.Items.Count);
        foreach (var summary in page.Items)
        {
            var detail = await service.상세조회Async(summary.TaskKey, cancellationToken);
            if (detail is not null)
            {
                result.Add(Map(detail));
            }
        }

        return result;
    }

    private static WarehousePickingTaskItem Map(피킹작업상세응답 detail)
        => new()
        {
            TaskKey = detail.TaskKey,
            WarehouseId = detail.WarehouseId,
            WarehouseName = detail.WarehouseName,
            PickerName = detail.WorkerDisplayName,
            WorkerBundleBarcode = detail.BundleBarcode,
            LineKey = detail.LineKey,
            ProductName = detail.ProductName,
            Sku = detail.Sku,
            ProductBarcode = detail.Sku,
            RackBarcode = detail.RackCode,
            RackCode = detail.RackCode,
            QuantityToPick = detail.Quantity,
            PickedQuantity = detail.CompletedAtUtc.HasValue ? detail.Quantity : 0,
            Mode = Enum.TryParse<피킹포장처리방식>(detail.ProcessingMode, true, out var mode)
                ? mode
                : 피킹포장처리방식.피킹포장분리,
            Status = detail.Status,
            NextStep = detail.NextStep
        };

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}
