namespace Ssalddel.Contracts.Common.Hr;

public static class HrRoleReviewSourceCodes
{
    public const string RoleApplication = "RoleApplication";
    public const string RoleAssignment = "RoleAssignment";

    public static bool IsKnown(string? value)
        => string.Equals(value, RoleApplication, StringComparison.Ordinal)
           || string.Equals(value, RoleAssignment, StringComparison.Ordinal);

    public static string GetDisplayName(string? value)
        => value switch
        {
            RoleApplication => "역할 지원 원장",
            RoleAssignment => "역할 배정 원장",
            _ => "알 수 없는 원장"
        };
}

public static class HrRoleReviewStatusCodes
{
    public const string Submitted = HrRoleApplicationStatusCodes.Submitted;
    public const string Withdrawn = HrRoleApplicationStatusCodes.Withdrawn;
    public const string Assigned = "Assigned";
    public const string Revoked = "Revoked";

    public static bool IsKnown(string? value)
        => string.Equals(value, Submitted, StringComparison.Ordinal)
           || string.Equals(value, Withdrawn, StringComparison.Ordinal)
           || string.Equals(value, Assigned, StringComparison.Ordinal)
           || string.Equals(value, Revoked, StringComparison.Ordinal);

    public static string GetDisplayName(string? value)
        => value switch
        {
            Submitted => "검토 대기",
            Withdrawn => "철회됨",
            Assigned => "배정됨",
            Revoked => "해제됨",
            _ => "알 수 없는 상태"
        };
}

public sealed class HrRoleReviewListRequest
{
    public string Search { get; set; } = string.Empty;

    public string SourceCode { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public string ParticipantCategory { get; set; } = string.Empty;

    public string ScopeType { get; set; } = string.Empty;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public sealed class HrRoleReviewSummaryResponse
{
    public Guid ReviewId { get; set; }

    public string SourceCode { get; set; } = HrRoleReviewSourceCodes.RoleAssignment;

    public string SourceName { get; set; } = HrRoleReviewSourceCodes.GetDisplayName(HrRoleReviewSourceCodes.RoleAssignment);

    public string ParticipantDisplayName { get; set; } = string.Empty;

    public string ParticipantUserName { get; set; } = string.Empty;

    public string ParticipantCategory { get; set; } = string.Empty;

    public string ParticipantCategoryName { get; set; } = string.Empty;

    public string ScopeType { get; set; } = string.Empty;

    public string ScopeId { get; set; } = string.Empty;

    public string RoleCode { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string StatusCode { get; set; } = HrRoleReviewStatusCodes.Assigned;

    public string StatusName { get; set; } = HrRoleReviewStatusCodes.GetDisplayName(HrRoleReviewStatusCodes.Assigned);

    public bool WorkScheduleEnabled { get; set; }

    public bool WorksiteIpRestrictionEnabled { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class HrRoleReviewListResponse
{
    public IReadOnlyList<HrRoleReviewSummaryResponse> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public sealed class HrRoleReviewDetailResponse
{
    public Guid ReviewId { get; set; }

    public string SourceCode { get; set; } = HrRoleReviewSourceCodes.RoleAssignment;

    public string SourceName { get; set; } = HrRoleReviewSourceCodes.GetDisplayName(HrRoleReviewSourceCodes.RoleAssignment);

    public string ParticipantDisplayName { get; set; } = string.Empty;

    public string ParticipantUserName { get; set; } = string.Empty;

    public string ParticipantCategory { get; set; } = string.Empty;

    public string ParticipantCategoryName { get; set; } = string.Empty;

    public string ScopeType { get; set; } = string.Empty;

    public string ScopeId { get; set; } = string.Empty;

    public string RoleCode { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string StatusCode { get; set; } = HrRoleReviewStatusCodes.Assigned;

    public string StatusName { get; set; } = HrRoleReviewStatusCodes.GetDisplayName(HrRoleReviewStatusCodes.Assigned);

    public string RecordedByDisplayName { get; set; } = string.Empty;

    public bool WorkScheduleEnabled { get; set; }

    public string TimeZoneId { get; set; } = HrWorkScheduleDefaults.TimeZoneId;

    public IReadOnlyList<DayOfWeek> AllowedDaysOfWeek { get; set; } = [];

    public TimeOnly? WorkStartLocalTime { get; set; }

    public TimeOnly? WorkEndLocalTime { get; set; }

    public bool WorksiteIpRestrictionEnabled { get; set; }

    public int AllowedWorksiteIpRangeCount { get; set; }

    public bool ConfirmedVoluntaryApplication { get; set; }

    public bool ConfirmedNoRoleOrEmploymentGuarantee { get; set; }

    public bool ConfirmedReviewDataUse { get; set; }

    public string ConsentVersion { get; set; } = string.Empty;

    public DateTime? WithdrawnAtUtc { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
