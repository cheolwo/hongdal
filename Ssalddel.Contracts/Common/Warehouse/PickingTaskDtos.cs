namespace Ssalddel.Contracts.Common.Warehouse;

public static class 피킹작업조회상태코드
{
    public const string 대기 = "대기";
    public const string 진행중 = "진행중";
    public const string 완료 = "완료";
    public const string 전체 = "전체";

    public static IReadOnlyList<string> 전체목록 { get; } = [대기, 진행중, 완료, 전체];

    public static string Normalize(string? value)
        => 전체목록.Contains(value?.Trim() ?? string.Empty, StringComparer.Ordinal)
            ? value!.Trim()
            : 대기;
}

public sealed class 피킹작업목록조회요청
{
    public long? WarehouseId { get; set; }

    public string? Search { get; set; }

    public string Status { get; set; } = 피킹작업조회상태코드.대기;

    public int Page { get; set; }

    public int PageSize { get; set; } = 12;
}

/// <summary>피킹 작업 목록에 필요한 최소 정보입니다. 작업자 ID와 연락처는 포함하지 않습니다.</summary>
public sealed class 피킹작업목록항목응답
{
    public string TaskKey { get; set; } = string.Empty;

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string RackCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string WorkerDisplayName { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 피킹작업목록페이지응답
{
    public IReadOnlyList<피킹작업목록항목응답> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public bool HasNextPage => (Page + 1) * PageSize < TotalCount;
}

/// <summary>명시한 피킹 작업 한 건의 현장 처리 정보입니다.</summary>
public sealed class 피킹작업상세응답
{
    public string TaskKey { get; set; } = string.Empty;

    public string ProcessingMode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public long WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string WorkerDisplayName { get; set; } = string.Empty;

    public string OrderReference { get; set; } = string.Empty;

    public string LineKey { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string RackCode { get; set; } = string.Empty;

    public string StorageLocationCode { get; set; } = string.Empty;

    public string BundleBarcode { get; set; } = string.Empty;

    public string AssignmentReason { get; set; } = string.Empty;

    public string NextStep { get; set; } = string.Empty;

    public bool CanStart { get; set; }

    public bool CanComplete { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 피킹작업완료요청
{
    public string RackCode { get; set; } = string.Empty;

    public bool ProductConfirmed { get; set; }

    public bool QuantityConfirmed { get; set; }
}

public sealed class 피킹작업결과응답
{
    public string TaskKey { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string NextStep { get; set; } = string.Empty;

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public bool IdempotentReplay { get; set; }
}
