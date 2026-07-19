namespace Ssalddel.Contracts.Common.PlatformProfit;

public sealed class PlatformRevenueEntryRequest
{
    public string RevenueSource { get; set; } = PlatformRevenueSourceCodes.TransportRecommendationCommission;
    public string SourceReferenceType { get; set; } = string.Empty;
    public string SourceReferenceId { get; set; } = string.Empty;
    public string PayerUserId { get; set; } = string.Empty;
    public string RelatedParticipantUserId { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal PlatformRevenueAmount { get; set; }
    public string CurrencyCode { get; set; } = "KRW";
    public DateTime OccurredAtUtc { get; set; }
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformRevenueEntryResponse
{
    public Guid Id { get; set; }
    public string RevenueSource { get; set; } = string.Empty;
    public string SourceReferenceType { get; set; } = string.Empty;
    public string SourceReferenceId { get; set; } = string.Empty;
    public string PayerUserId { get; set; } = string.Empty;
    public string RelatedParticipantUserId { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal PlatformRevenueAmount { get; set; }
    public string CurrencyCode { get; set; } = "KRW";
    public DateTime OccurredAtUtc { get; set; }
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformProfitReturnPolicyRequest
{
    public string PolicyName { get; set; } = string.Empty;
    public string TargetParticipantCategory { get; set; } = ProfitReturnParticipantCategoryCodes.Driver;
    public decimal ReturnRatePercent { get; set; }
    public decimal CompanyReserveAmount { get; set; }
    public decimal MinimumProfitThreshold { get; set; }
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformProfitReturnPolicyResponse
{
    public Guid Id { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public string TargetParticipantCategory { get; set; } = ProfitReturnParticipantCategoryCodes.Driver;
    public decimal ReturnRatePercent { get; set; }
    public decimal CompanyReserveAmount { get; set; }
    public decimal MinimumProfitThreshold { get; set; }
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public bool IsActive { get; set; }
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformProfitReturnParticipantShareRequest
{
    public string ParticipantUserId { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public decimal Weight { get; set; } = 1m;
}

public sealed class PlatformProfitReturnScheduleCreateRequest
{
    public Guid PolicyId { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public DateOnly ScheduledPaymentDate { get; set; }
    public decimal OperatingCostAmount { get; set; }
    public IReadOnlyList<PlatformProfitReturnParticipantShareRequest> Participants { get; set; } = [];
}

public sealed class PlatformProfitReturnPlanResponse
{
    public Guid PolicyId { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public decimal TotalPlatformRevenueAmount { get; set; }
    public decimal OperatingCostAmount { get; set; }
    public decimal EstimatedProfitAmount { get; set; }
    public decimal ReturnPoolAmount { get; set; }
    public IReadOnlyList<PlatformProfitReturnScheduleResponse> Schedules { get; set; } = [];
}

public sealed class PlatformProfitReturnScheduleResponse
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string ParticipantUserId { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantCategory { get; set; } = ProfitReturnParticipantCategoryCodes.Driver;
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public DateOnly ScheduledPaymentDate { get; set; }
    public decimal TotalPlatformRevenueAmount { get; set; }
    public decimal OperatingCostAmount { get; set; }
    public decimal EstimatedProfitAmount { get; set; }
    public decimal ReturnPoolAmount { get; set; }
    public decimal ParticipantWeight { get; set; }
    public decimal PlannedReturnAmount { get; set; }
    public string Status { get; set; } = ProfitReturnScheduleStatuses.Planned;
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformProfitReturnScheduleListResponse
{
    public IReadOnlyList<PlatformProfitReturnScheduleResponse> Items { get; set; } = [];
}

public static class PlatformRevenueSourceCodes
{
    public const string TransportRecommendationCommission = "TransportRecommendationCommission";
    public const string DriverUsageFee = "DriverUsageFee";
    public const string WarehouseSalesCommission = "WarehouseSalesCommission";
    public const string FoodDeliveryCommission = "FoodDeliveryCommission";
    public const string LogisticsAgencyFee = "LogisticsAgencyFee";
    public const string PlatformSubscription = "PlatformSubscription";
}

public static class ProfitReturnParticipantCategoryCodes
{
    public const string Driver = "Driver";
    public const string DeliveryRider = "DeliveryRider";
    public const string WarehouseWorker = "WarehouseWorker";
    public const string RestaurantPartner = "RestaurantPartner";
    public const string PlatformContributor = "PlatformContributor";
}

public static class ProfitReturnScheduleStatuses
{
    public const string Planned = "Planned";
    public const string Approved = "Approved";
    public const string PaymentRequested = "PaymentRequested";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
}
