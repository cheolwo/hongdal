namespace Hongdal.Contracts.Common.Hr;

public sealed class HrRoleAssignmentResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public string ParticipantCategory { get; set; } = HrParticipantCategoryCodes.InternalProjectOperator;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public string AssignedByUserId { get; set; } = string.Empty;
    public bool WorkScheduleEnabled { get; set; }
    public string TimeZoneId { get; set; } = HrWorkScheduleDefaults.TimeZoneId;
    public IReadOnlyList<DayOfWeek> AllowedDaysOfWeek { get; set; } = [];
    public TimeOnly? WorkStartLocalTime { get; set; }
    public TimeOnly? WorkEndLocalTime { get; set; }
    public bool WorksiteIpRestrictionEnabled { get; set; }
    public IReadOnlyList<string> AllowedWorksiteIpRanges { get; set; } = [];
}

public sealed class HrRoleAssignmentListResponse
{
    public IReadOnlyList<HrRoleAssignmentResponse> Items { get; set; } = [];
}

public sealed class HrRoleAssignmentRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ScopeType { get; set; } = HrScopeTypes.Platform;
    public string ScopeId { get; set; } = HrScopeIds.Global;
    public string ParticipantCategory { get; set; } = HrParticipantCategoryCodes.InternalProjectOperator;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool WorkScheduleEnabled { get; set; }
    public string TimeZoneId { get; set; } = HrWorkScheduleDefaults.TimeZoneId;
    public IReadOnlyList<DayOfWeek> AllowedDaysOfWeek { get; set; } = [];
    public TimeOnly? WorkStartLocalTime { get; set; }
    public TimeOnly? WorkEndLocalTime { get; set; }
    public bool WorksiteIpRestrictionEnabled { get; set; }
    public IReadOnlyList<string> AllowedWorksiteIpRanges { get; set; } = [];
}

public static class HrParticipantCategoryCodes
{
    public const string CounterpartyRepresentative = "CounterpartyRepresentative";
    public const string InternalProjectOperator = "InternalProjectOperator";
    public const string ExternalProfessional = "ExternalProfessional";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            CounterpartyRepresentative => CounterpartyRepresentative,
            ExternalProfessional => ExternalProfessional,
            _ => InternalProjectOperator
        };

    public static string GetDisplayName(string? value)
        => Normalize(value) switch
        {
            CounterpartyRepresentative => "거래 상대/대표",
            ExternalProfessional => "외부 전문 참여자",
            _ => "내부 실무/프로젝트 담당자"
        };
}

public static class HrScopeTypes
{
    public const string Platform = "Platform";
    public const string Warehouse = "Warehouse";
    public const string Immigration = "Immigration";
    public const string PurchasingAgency = "PurchasingAgency";
    public const string ShippingAgency = "ShippingAgency";
}

public static class HrScopeIds
{
    public const string Global = "global";
}

public static class HrWorkScheduleDefaults
{
    public const string TimeZoneId = "Asia/Seoul";
}

public static class HrDetailedRoleCodes
{
    public const string WarehouseManager = "Warehouse.Manager";
    public const string WarehouseInboundOperator = "Warehouse.InboundOperator";
    public const string WarehouseInventoryOperator = "Warehouse.InventoryOperator";
    public const string WarehouseDispatchOperator = "Warehouse.DispatchOperator";
    public const string ImmigrationVisaAgent = "Immigration.VisaAgent";
    public const string PurchasingAgencyOperator = "PurchasingAgency.Operator";
    public const string ShippingAgencyOperator = "ShippingAgency.Operator";
}
