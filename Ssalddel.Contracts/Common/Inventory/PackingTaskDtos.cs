namespace Ssalddel.Contracts.Common.Inventory;

public static class 포장작업조회상태코드
{
    public const string 대기 = "대기";
    public const string 완료 = "완료";
    public const string 전체 = "전체";
    public static IReadOnlyList<string> 전체목록 { get; } = [대기, 완료, 전체];
    public static string Normalize(string? value)
        => 전체목록.Contains(value?.Trim() ?? string.Empty, StringComparer.Ordinal) ? value!.Trim() : 대기;
}

public static class 포장유형코드
{
    public const string 일반포장 = "일반포장";
    public const string 냉장포장 = "냉장포장";
    public const string 완충포장 = "완충포장";
    public static IReadOnlyList<string> 전체목록 { get; } = [일반포장, 냉장포장, 완충포장];
    public static bool IsValid(string? value) => 전체목록.Contains(value?.Trim() ?? string.Empty, StringComparer.Ordinal);
}

public sealed class 포장작업목록조회요청
{
    public long? WarehouseId { get; set; }
    public string? Search { get; set; }
    public string Status { get; set; } = 포장작업조회상태코드.대기;
    public int Page { get; set; }
    public int PageSize { get; set; } = 12;
}

/// <summary>포장 작업 목록에 필요한 최소 정보이며 사용자 ID와 계약·정산 내용은 포함하지 않습니다.</summary>
public sealed class 포장작업목록항목응답
{
    public long InboundItemId { get; set; }
    public long WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public string InventoryStatus { get; set; } = string.Empty;
    public string StorageLocation { get; set; } = string.Empty;
    public bool CanPack { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 포장작업목록페이지응답
{
    public IReadOnlyList<포장작업목록항목응답> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => (Page + 1) * PageSize < TotalCount;
}

/// <summary>명시한 입고상품 한 건의 적재 근거와 포장 상태입니다.</summary>
public sealed class 포장작업상세응답
{
    public long InboundItemId { get; set; }
    public long InboundId { get; set; }
    public long WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int DefectiveQuantity { get; set; }
    public string InventoryStatus { get; set; } = string.Empty;
    public string StorageLocation { get; set; } = string.Empty;
    public string StorageCondition { get; set; } = string.Empty;
    public string OrderReference { get; set; } = string.Empty;
    public DateTime? PutAwayAtUtc { get; set; }
    public string PutAwayMemo { get; set; } = string.Empty;
    public DateTime? PackedAtUtc { get; set; }
    public string PackingMemo { get; set; } = string.Empty;
    public string PackingType { get; set; } = string.Empty;
    public bool CanPack { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 포장작업완료요청
{
    public int PackagingQuantity { get; set; }
    public string PackagingType { get; set; } = 포장유형코드.일반포장;
    public string Memo { get; set; } = string.Empty;
    public bool InventoryConfirmed { get; set; }
    public bool PackageLabelConfirmed { get; set; }
}

public sealed class 포장작업결과응답
{
    public long InboundItemId { get; set; }
    public string InventoryStatus { get; set; } = string.Empty;
    public int PackagingQuantity { get; set; }
    public string PackagingType { get; set; } = string.Empty;
    public string NextStep { get; set; } = string.Empty;
    public DateTime PackedAtUtc { get; set; }
    public bool IdempotentReplay { get; set; }
}
