using Ssalddel.Contracts.Common.Warehouse;

namespace WarehouseManagerApp.Services;

public interface IWarehousePickingBatchWorkspaceService
{
    Task<IReadOnlyList<WarehousePickingWarehouseOption>> GetWarehouseOptionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WarehousePickingTaskItem>> GetAssignedTasksAsync(long warehouseId, CancellationToken cancellationToken = default);

    Task<WarehousePickingWarehouseOption> UpdateWarehouseOptionAsync(long warehouseId, 피킹포장처리방식 mode, bool isBarcodeRequired, CancellationToken cancellationToken = default);

    Task<WarehouseRackScanResult> ScanRackAsync(long warehouseId, string rackBarcode, CancellationToken cancellationToken = default);

    Task<WarehousePickCompleteResult> CompletePickAsync(string taskKey, string rackBarcode, string productBarcode, int quantity, CancellationToken cancellationToken = default);
}

public sealed record WarehousePickingWarehouseOption(
    long WarehouseId,
    string WarehouseName,
    피킹포장처리방식 Mode,
    bool IsBarcodeRequired,
    bool IsSsalddelMartUrbanWarehouse,
    string OperationMemo);

public sealed class WarehousePickingTaskItem
{
    public string TaskKey { get; init; } = string.Empty;

    public long WarehouseId { get; init; }

    public string WarehouseName { get; init; } = string.Empty;

    public string PickerName { get; init; } = string.Empty;

    public string PickerPhoneLast8 { get; init; } = string.Empty;

    public string WorkerBundleBarcode { get; init; } = string.Empty;

    public string? PackerName { get; init; }

    public string LineKey { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public string ProductBarcode { get; init; } = string.Empty;

    public string RackBarcode { get; init; } = string.Empty;

    public string RackCode { get; init; } = string.Empty;

    public int QuantityToPick { get; init; }

    public int PickedQuantity { get; set; }

    public 피킹포장처리방식 Mode { get; init; }

    public string Status { get; set; } = "피킹대기";

    public string NextStep { get; set; } = "피킹 필요";

    public int RemainingQuantity => Math.Max(0, QuantityToPick - PickedQuantity);

    public bool IsCompleted => RemainingQuantity == 0;
}

public sealed record WarehouseRackScanResult(
    bool IsSuccess,
    string Message,
    string RackBarcode,
    IReadOnlyList<WarehousePickingTaskItem> Items);

public sealed record WarehousePickCompleteResult(
    bool IsSuccess,
    string Message,
    WarehousePickingTaskItem? Task);

public sealed class SampleWarehousePickingBatchWorkspaceService : IWarehousePickingBatchWorkspaceService
{
    private readonly List<WarehousePickingWarehouseOption> _options =
    [
        new(10, "송파 상온 창고", 피킹포장처리방식.피킹포장분리, true, false, "일반 출고 창고는 피킹과 포장을 분리해 운영합니다."),
        new(20, "알뜰살뜰 마트 도심 창고", 피킹포장처리방식.피킹포장통합, true, true, "장소가 협소한 도심 창고라 피킹 작업자가 포장까지 이어서 처리합니다.")
    ];

    private readonly List<WarehousePickingTaskItem> _tasks =
    [
        new()
        {
            TaskKey = "PICK-10-001",
            WarehouseId = 10,
            WarehouseName = "송파 상온 창고",
            PickerName = "피킹 담당자 A",
            PickerPhoneLast8 = "77773333",
            WorkerBundleBarcode = "77773333",
            PackerName = "포장 담당자 B",
            LineKey = "NAVER-20260708-001-01",
            ProductName = "수입 삼겹살 3kg 묶음",
            Sku = "PORK-3KG",
            ProductBarcode = "BAR-PORK-3KG",
            RackBarcode = "LOC-A-01-02",
            RackCode = "A-01-02",
            QuantityToPick = 5,
            Mode = 피킹포장처리방식.피킹포장분리,
            NextStep = "포장 작업자에게 인계"
        },
        new()
        {
            TaskKey = "PICK-10-002",
            WarehouseId = 10,
            WarehouseName = "송파 상온 창고",
            PickerName = "피킹 담당자 A",
            PickerPhoneLast8 = "77773333",
            WorkerBundleBarcode = "77773333",
            PackerName = "포장 담당자 B",
            LineKey = "COUPANG-20260708-014-01",
            ProductName = "공동구매 생활용품 박스",
            Sku = "LIFE-BOX",
            ProductBarcode = "BAR-LIFE-BOX",
            RackBarcode = "LOC-A-01-02",
            RackCode = "A-01-02",
            QuantityToPick = 3,
            Mode = 피킹포장처리방식.피킹포장분리,
            NextStep = "포장 작업자에게 인계"
        },
        new()
        {
            TaskKey = "PICK-20-001",
            WarehouseId = 20,
            WarehouseName = "알뜰살뜰 마트 도심 창고",
            PickerName = "도심피킹",
            PickerPhoneLast8 = "77773333",
            WorkerBundleBarcode = "77773333",
            PackerName = "도심피킹",
            LineKey = "MART-20260708-021-01",
            ProductName = "냉장 밀키트",
            Sku = "COLD-MEALKIT",
            ProductBarcode = "BAR-COLD-MEAL",
            RackBarcode = "LOC-C-02-01",
            RackCode = "C-02-01",
            QuantityToPick = 4,
            Mode = 피킹포장처리방식.피킹포장통합,
            NextStep = "같은 작업자가 포장까지 이어서 처리"
        }
    ];

    public Task<IReadOnlyList<WarehousePickingWarehouseOption>> GetWarehouseOptionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<WarehousePickingWarehouseOption>>(_options.ToArray());

    public Task<IReadOnlyList<WarehousePickingTaskItem>> GetAssignedTasksAsync(long warehouseId, CancellationToken cancellationToken = default)
    {
        var result = _tasks
            .Where(x => x.WarehouseId == warehouseId)
            .OrderBy(x => x.RackCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<WarehousePickingTaskItem>>(result);
    }

    public Task<WarehousePickingWarehouseOption> UpdateWarehouseOptionAsync(
        long warehouseId,
        피킹포장처리방식 mode,
        bool isBarcodeRequired,
        CancellationToken cancellationToken = default)
    {
        var index = _options.FindIndex(x => x.WarehouseId == warehouseId);
        if (index < 0)
        {
            throw new InvalidOperationException("창고 옵션을 찾지 못했습니다.");
        }

        var current = _options[index];
        var next = current with { Mode = mode, IsBarcodeRequired = isBarcodeRequired };
        _options[index] = next;

        foreach (var task in _tasks.Where(x => x.WarehouseId == warehouseId && !x.IsCompleted))
        {
            task.Status = "피킹대기";
            task.NextStep = ResolveNextStep(mode);
        }

        return Task.FromResult(next);
    }

    public Task<WarehouseRackScanResult> ScanRackAsync(long warehouseId, string rackBarcode, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(rackBarcode);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Task.FromResult(new WarehouseRackScanResult(false, "적재대 위치 바코드를 입력해야 합니다.", string.Empty, []));
        }

        var items = _tasks
            .Where(x => x.WarehouseId == warehouseId)
            .Where(x => !x.IsCompleted)
            .Where(x => string.Equals(Normalize(x.RackBarcode), normalized, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return items.Length == 0
            ? Task.FromResult(new WarehouseRackScanResult(false, "해당 적재대에 할당된 피킹 상품이 없습니다.", normalized, []))
            : Task.FromResult(new WarehouseRackScanResult(true, $"{items.Length}개 피킹 상품을 찾았습니다.", normalized, items));
    }

    public Task<WarehousePickCompleteResult> CompletePickAsync(
        string taskKey,
        string rackBarcode,
        string productBarcode,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var task = _tasks.FirstOrDefault(x => string.Equals(x.TaskKey, taskKey, StringComparison.OrdinalIgnoreCase));
        if (task is null)
        {
            return Task.FromResult(new WarehousePickCompleteResult(false, "피킹 작업을 찾지 못했습니다.", null));
        }

        if (!string.Equals(Normalize(task.RackBarcode), Normalize(rackBarcode), StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new WarehousePickCompleteResult(false, "스캔한 적재대 위치가 피킹 작업과 일치하지 않습니다.", task));
        }

        var option = _options.FirstOrDefault(x => x.WarehouseId == task.WarehouseId);
        if (option?.IsBarcodeRequired != false
            && !string.Equals(Normalize(task.ProductBarcode), Normalize(productBarcode), StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new WarehousePickCompleteResult(false, "상품 바코드가 피킹 작업의 상품과 일치하지 않습니다.", task));
        }

        if (quantity <= 0)
        {
            return Task.FromResult(new WarehousePickCompleteResult(false, "피킹 수량은 1개 이상이어야 합니다.", task));
        }

        if (quantity > task.RemainingQuantity)
        {
            return Task.FromResult(new WarehousePickCompleteResult(false, "남은 피킹 수량보다 많이 입력할 수 없습니다.", task));
        }

        task.PickedQuantity += quantity;
        task.Status = task.IsCompleted ? "피킹완료" : "피킹중";
        if (task.IsCompleted)
        {
            task.NextStep = ResolveNextStep(task.Mode);
        }

        var message = task.IsCompleted
            ? $"피킹 완료. 작업자 묶음 바코드 {task.WorkerBundleBarcode} 아래에 물량을 묶었습니다."
            : $"피킹 수량을 반영했습니다. 작업자 묶음 바코드 {task.WorkerBundleBarcode} 아래에 누적됩니다.";

        return Task.FromResult(new WarehousePickCompleteResult(true, message, task));
    }

    private static string ResolveNextStep(피킹포장처리방식 mode)
        => mode switch
        {
            피킹포장처리방식.피킹포장통합 => "같은 작업자가 포장까지 이어서 처리",
            피킹포장처리방식.피킹포장분리 => "포장 작업자에게 인계",
            _ => "피킹 완료"
        };

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();
}
