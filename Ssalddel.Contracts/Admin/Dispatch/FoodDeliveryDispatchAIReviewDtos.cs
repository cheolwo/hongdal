namespace Ssalddel.Contracts.Admin.Dispatch;

public sealed class FoodDeliveryDispatchAIReviewWorkspaceDto
{
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Source { get; set; } = "actual";

    public string PrimaryDeliveryScopeKey { get; set; } = string.Empty;

    public string PrimaryDeliveryScopeName { get; set; } = string.Empty;

    public List<string> AdjacentDeliveryScopeKeys { get; set; } = [];

    public List<string> AdjacentDeliveryScopeNames { get; set; } = [];

    public List<FoodDeliveryDispatchAIReviewOrderDto> Orders { get; set; } = [];

    public List<FoodDeliveryDispatchAIReviewDriverDto> Drivers { get; set; } = [];

    public List<FoodDeliveryDispatchAIReviewBundleDto> Bundles { get; set; } = [];

    public List<FoodDeliveryDispatchAIReviewAssignmentDto> Assignments { get; set; } = [];

    public List<string> Notes { get; set; } = [];
}

public sealed class FoodDeliveryDispatchAIReviewOrderDto
{
    public string OrderNo { get; set; } = string.Empty;

    public long RestaurantId { get; set; }

    public string RestaurantName { get; set; } = string.Empty;

    public string MenuSummary { get; set; } = string.Empty;

    public decimal OrderAmount { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public string DispatchStatus { get; set; } = string.Empty;

    public string RestaurantAddress { get; set; } = string.Empty;

    public decimal? RestaurantLatitude { get; set; }

    public decimal? RestaurantLongitude { get; set; }

    public string CustomerAddress { get; set; } = string.Empty;

    public decimal? CustomerLatitude { get; set; }

    public decimal? CustomerLongitude { get; set; }

    public DateTime? PickupReadyAtUtc { get; set; }

    public decimal MaxDeliveryMinutesAfterReady { get; set; } = 42m;

    public string PickupScopeKey { get; set; } = string.Empty;

    public string PickupScopeName { get; set; } = string.Empty;

    public string PickupScopeRole { get; set; } = string.Empty;

    public string DropoffScopeKey { get; set; } = string.Empty;

    public string DropoffScopeName { get; set; } = string.Empty;

    public string DropoffScopeRole { get; set; } = string.Empty;
}

public sealed class FoodDeliveryDispatchAIReviewDriverDto
{
    public string DriverId { get; set; } = string.Empty;

    public string DriverName { get; set; } = string.Empty;

    public string DrivingStatus { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string DeliveryScopeKey { get; set; } = string.Empty;

    public string DeliveryScopeName { get; set; } = string.Empty;

    public string DeliveryScopeRole { get; set; } = string.Empty;

    public int CurrentAcceptedDeliveryCount { get; set; }

    public DateTime? LastLocationReceivedAtUtc { get; set; }
}

public sealed class FoodDeliveryDispatchAIReviewBundleDto
{
    public string BundleKey { get; set; } = string.Empty;

    public string BundleType { get; set; } = string.Empty;

    public List<string> OrderNos { get; set; } = [];

    public int BundleSize { get; set; }

    public bool IsBundleAvailable { get; set; }

    public bool IsAISuggested { get; set; }

    public string? SuggestedDriverId { get; set; }

    public decimal Score { get; set; }

    public decimal? PickupDistanceKm { get; set; }

    public decimal? DropoffDistanceKm { get; set; }

    public decimal? ExpectedRouteDistanceKm { get; set; }

    public List<string> Badges { get; set; } = [];

    public List<string> Warnings { get; set; } = [];

    public List<string> ExclusionReasons { get; set; } = [];

    public string Reason { get; set; } = string.Empty;

    public string BundleDecisionSummary { get; set; } = string.Empty;

    public string DriverAssignmentDecisionSummary { get; set; } = string.Empty;
}

public sealed class FoodDeliveryDispatchAIReviewAssignmentDto
{
    public string OrderNo { get; set; } = string.Empty;

    public string DriverId { get; set; } = string.Empty;

    public int Order { get; set; }

    public decimal Score { get; set; }

    public string Reason { get; set; } = string.Empty;

    public List<string> Badges { get; set; } = [];
}

public sealed class FoodDeliveryDispatchAIReviewDecisionRequest
{
    public string DecisionType { get; set; } = "운영자승인";

    public string BundleKey { get; set; } = string.Empty;

    public List<string> OrderNos { get; set; } = [];

    public string DriverId { get; set; } = string.Empty;

    public string AdminNote { get; set; } = string.Empty;

    public bool ManualBundle { get; set; }

    public bool Accepted { get; set; } = true;
}

public sealed class FoodDeliveryDispatchAIReviewDecisionResponse
{
    public string CaseId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
