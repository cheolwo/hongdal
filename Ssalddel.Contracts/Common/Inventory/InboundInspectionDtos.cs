namespace Ssalddel.Contracts.Common.Inventory;

/// <summary>입고 검수 대상 목록에서 사용할 서버 필터 코드입니다.</summary>
public static class 입고검수조회상태코드
{
    public const string 대기 = "대기";
    public const string 완료 = "완료";
    public const string 전체 = "전체";

    public static IReadOnlyList<string> 전체목록 { get; } = [대기, 완료, 전체];

    public static string Normalize(string? value)
        => 전체목록.Contains(value?.Trim() ?? string.Empty, StringComparer.Ordinal)
            ? value!.Trim()
            : 대기;
}

/// <summary>로그인 사용자가 접근할 수 있는 입고 검수 대상만 조회하는 조건입니다.</summary>
public sealed class 입고검수대상목록조회요청
{
    public long? WarehouseId { get; set; }

    public string? Search { get; set; }

    public string InspectionStatus { get; set; } = 입고검수조회상태코드.대기;

    public int Page { get; set; }

    public int PageSize { get; set; } = 12;
}

/// <summary>검수 대상 목록에 필요한 최소 정보입니다. 사용자 식별자와 계약·정산 정보는 포함하지 않습니다.</summary>
public sealed class 입고검수대상목록항목응답
{
    public long InboundItemId { get; set; }

    public long InboundId { get; set; }

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int ReceivedQuantity { get; set; }

    public int DefectiveQuantity { get; set; }

    public string InventoryStatus { get; set; } = string.Empty;

    public bool CanInspect { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 입고검수대상페이지응답
{
    public IReadOnlyList<입고검수대상목록항목응답> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public bool HasNextPage => (Page + 1) * PageSize < TotalCount;
}

/// <summary>명시한 한 입고상품의 검수 화면에 필요한 정보입니다.</summary>
public sealed class 입고검수대상상세응답
{
    public long InboundItemId { get; set; }

    public long InboundId { get; set; }

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string OptionName { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public int ReceivedQuantity { get; set; }

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int DefectiveQuantity { get; set; }

    public string InventoryStatus { get; set; } = string.Empty;

    public string StorageLocation { get; set; } = string.Empty;

    public string StorageCondition { get; set; } = string.Empty;

    public bool CanInspect { get; set; }

    public DateTime? ReceivedAtUtc { get; set; }

    public DateTime? InspectedAtUtc { get; set; }

    public string InspectionMemo { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }
}
