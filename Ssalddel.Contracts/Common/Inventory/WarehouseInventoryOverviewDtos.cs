namespace Ssalddel.Contracts.Common.Inventory;

public static class 창고재고조회상태코드
{
    public const string 전체 = "전체";
    public const string 가용 = "가용";
    public const string 예약 = "예약";
    public const string 위치미배정 = "위치미배정";

    public static IReadOnlyList<string> 전체목록 { get; } = [전체, 가용, 예약, 위치미배정];

    public static string Normalize(string? value)
        => 전체목록.Contains(value?.Trim() ?? string.Empty, StringComparer.Ordinal)
            ? value!.Trim()
            : 전체;
}

public sealed class 창고재고현황목록조회요청
{
    public long? WarehouseId { get; set; }

    public string? Search { get; set; }

    public string Status { get; set; } = 창고재고조회상태코드.전체;

    public int Page { get; set; }

    public int PageSize { get; set; } = 12;
}

/// <summary>재고 목록에 필요한 창고 운영 정보만 포함하며 사용자 ID와 계약 내용은 포함하지 않습니다.</summary>
public sealed class 창고재고현황목록항목응답
{
    public long InboundItemId { get; set; }

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string OptionName { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public string StorageLocation { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool HasCommunityLedger { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 창고재고현황목록페이지응답
{
    public IReadOnlyList<창고재고현황목록항목응답> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int TotalAvailableQuantity { get; set; }

    public int TotalReservedQuantity { get; set; }

    public int UnassignedLocationCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public bool HasNextPage => (Page + 1) * PageSize < TotalCount;
}

/// <summary>명시한 입고상품 한 건의 재고 근거입니다. 사용자 ID와 계약·정산 내용은 노출하지 않습니다.</summary>
public sealed class 창고재고현황상세응답
{
    public long InboundItemId { get; set; }

    public long InboundId { get; set; }

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string OptionName { get; set; } = string.Empty;

    public int InboundQuantity { get; set; }

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int DefectiveQuantity { get; set; }

    public string StorageLocation { get; set; } = string.Empty;

    public string StorageCondition { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string OrderReference { get; set; } = string.Empty;

    public string InboundFlowType { get; set; } = string.Empty;

    public string InboundPath { get; set; } = string.Empty;

    public string BundleBarcode { get; set; } = string.Empty;

    public string? CommunityLedgerId { get; set; }

    public string? CommunityLedgerTemplateKey { get; set; }

    public string? CommunityLedgerState { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
