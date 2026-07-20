namespace Ssalddel.Contracts.Common.Inventory;

public static class 출고인계준비조회상태코드
{
    public const string 대기 = "대기";
    public const string 완료 = "완료";
    public const string 전체 = "전체";
    public static IReadOnlyList<string> 전체목록 { get; } = [대기, 완료, 전체];
    public static string Normalize(string? value)
        => 전체목록.Contains(value?.Trim() ?? string.Empty, StringComparer.Ordinal) ? value!.Trim() : 대기;
}

public sealed class 출고인계준비목록조회요청
{
    public long? WarehouseId { get; set; }
    public string? Search { get; set; }
    public string Status { get; set; } = 출고인계준비조회상태코드.대기;
    public int Page { get; set; }
    public int PageSize { get; set; } = 12;
}

/// <summary>출고 인계 준비 목록에 필요한 최소 정보이며 사용자 ID와 연락처·계약·정산 내용은 포함하지 않습니다.</summary>
public sealed class 출고인계준비목록항목응답
{
    public long InboundItemId { get; set; }
    public long? OutboundPlanId { get; set; }
    public long WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int HandoffQuantity { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public string PackagingType { get; set; } = string.Empty;
    public bool IsHandoffReady { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 출고인계준비목록페이지응답
{
    public IReadOnlyList<출고인계준비목록항목응답> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => (Page + 1) * PageSize < TotalCount;
}

/// <summary>명시한 포장 재고 한 건의 포장 근거와 출고예정 원장 상태입니다.</summary>
public sealed class 출고인계준비상세응답
{
    public long InboundItemId { get; set; }
    public long InboundId { get; set; }
    public long? OutboundPlanId { get; set; }
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
    public string PackagingType { get; set; } = string.Empty;
    public DateTime? PackedAtUtc { get; set; }
    public string PackingMemo { get; set; } = string.Empty;
    public string OutboundStatus { get; set; } = string.Empty;
    public DateTime? HandoffReadyAtUtc { get; set; }
    public string HandoffMemo { get; set; } = string.Empty;
    public bool CanConfirmHandoff { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 출고인계준비완료요청
{
    public int HandoffQuantity { get; set; }
    public string Memo { get; set; } = string.Empty;
    public bool PackageSealConfirmed { get; set; }
    public bool TransportConditionsConfirmed { get; set; }
}

public sealed class 출고인계준비결과응답
{
    public long InboundItemId { get; set; }
    public long OutboundPlanId { get; set; }
    public string OutboundStatus { get; set; } = string.Empty;
    public int HandoffQuantity { get; set; }
    public string NextStep { get; set; } = string.Empty;
    public DateTime HandoffReadyAtUtc { get; set; }
    public bool IdempotentReplay { get; set; }
}
