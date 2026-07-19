namespace Ssalddel.Contracts.Common.Community;

public sealed class CommunityActivitySignalQuery
{
    public string? AppKey { get; set; }

    public string? CommunityScope { get; set; }

    public string? Tag { get; set; }

    public bool IncludeRead { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public sealed class CommunityActivitySignalListResponse
{
    public IReadOnlyList<CommunityActivitySignalResponse> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

public sealed class CommunityActivitySignalResponse
{
    public string SignalId { get; set; } = string.Empty;

    public string AppKey { get; set; } = string.Empty;

    public string CommunityScope { get; set; } = string.Empty;

    public string ActivityKind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string ActorRoleLabel { get; set; } = string.Empty;

    public IReadOnlyList<string> TopicTags { get; set; } = [];

    public string TimeBucketLabel { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}

public static class CommunityActivityScopes
{
    public const string DriverWork = "DriverWork";

    public const string ShipperTransport = "ShipperTransport";

    public const string WarehouseWork = "WarehouseWork";

    public const string ProductJourney = "ProductJourney";

    public const string SalesCommerce = "SalesCommerce";

    public const string CommunityTrust = "CommunityTrust";
}
