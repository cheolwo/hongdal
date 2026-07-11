namespace Hongdal.Contracts.Admin.Dispatch;

public sealed class DomesticCargoDispatchAIReviewWorkspaceDto
{
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Source { get; set; } = "actual";

    public List<DomesticCargoDispatchAIReviewRequestDto> Requests { get; set; } = [];

    public List<DomesticCargoDispatchAIReviewDriverDto> Drivers { get; set; } = [];

    public List<DomesticCargoDispatchAIReviewBundleDto> Bundles { get; set; } = [];

    public List<DomesticCargoDispatchAIReviewAssignmentDto> Assignments { get; set; } = [];

    public List<string> Notes { get; set; } = [];
}

public sealed class DomesticCargoDispatchAIReviewRequestDto
{
    public long QueueId { get; set; }

    public string RequestId { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string CargoType { get; set; } = string.Empty;

    public string PickupAddress { get; set; } = string.Empty;

    public decimal? PickupLatitude { get; set; }

    public decimal? PickupLongitude { get; set; }

    public string DropoffAddress { get; set; } = string.Empty;

    public decimal? DropoffLatitude { get; set; }

    public decimal? DropoffLongitude { get; set; }

    public string DeliveryScopeKey { get; set; } = string.Empty;

    public string DeliveryScopeName { get; set; } = string.Empty;

    public decimal? Fare { get; set; }

    public DateTime? PickupWindowEndUtc { get; set; }
}

public sealed class DomesticCargoDispatchAIReviewDriverDto
{
    public string DriverId { get; set; } = string.Empty;

    public string DriverName { get; set; } = string.Empty;

    public string VehicleType { get; set; } = string.Empty;

    public string DrivingStatus { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string DeliveryScopeKey { get; set; } = string.Empty;

    public string DeliveryScopeName { get; set; } = string.Empty;

    public int CurrentAcceptedTransportCount { get; set; }

    public DateTime? LastLocationReceivedAtUtc { get; set; }
}

public sealed class DomesticCargoDispatchAIReviewBundleDto
{
    public string BundleKey { get; set; } = string.Empty;

    public string BundleType { get; set; } = string.Empty;

    public List<string> RequestIds { get; set; } = [];

    public int BundleSize { get; set; }

    public bool IsBundleAvailable { get; set; }

    public bool IsAISuggested { get; set; }

    public string? SuggestedDriverId { get; set; }

    public decimal Score { get; set; }

    public decimal? ExpectedFare { get; set; }

    public decimal? ExpectedCost { get; set; }

    public decimal? ExpectedProfit { get; set; }

    public decimal? ExpectedProfitPerRequest { get; set; }

    public List<string> Badges { get; set; } = [];

    public List<string> Warnings { get; set; } = [];

    public List<string> ExclusionReasons { get; set; } = [];

    public string Reason { get; set; } = string.Empty;
}

public sealed class DomesticCargoDispatchAIReviewAssignmentDto
{
    public string RequestId { get; set; } = string.Empty;

    public string DriverId { get; set; } = string.Empty;

    public int Order { get; set; }

    public decimal Score { get; set; }

    public decimal? ExpectedCost { get; set; }

    public decimal? ExpectedFare { get; set; }

    public decimal? ExpectedProfit { get; set; }

    public string Reason { get; set; } = string.Empty;

    public List<string> Badges { get; set; } = [];
}

public sealed class DomesticCargoDispatchAIReviewDecisionRequest
{
    public string DecisionType { get; set; } = "운영자승인";

    public string BundleKey { get; set; } = string.Empty;

    public List<string> RequestIds { get; set; } = [];

    public string DriverId { get; set; } = string.Empty;

    public string AdminNote { get; set; } = string.Empty;

    public bool ManualBundle { get; set; }

    public bool Accepted { get; set; } = true;
}

public sealed class DomesticCargoDispatchAIReviewDecisionResponse
{
    public string CaseId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
