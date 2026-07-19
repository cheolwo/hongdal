namespace Ssalddel.Contracts.Common.Hr;

public sealed class HrParticipationBenefitRecordResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string SourceType { get; set; } = HrParticipationSourceTypes.OfflineMeeting;
    public string SourceId { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
    public string ParticipationStatus { get; set; } = HrParticipationStatuses.Confirmed;
    public string BenefitStatus { get; set; } = HrParticipationBenefitStatuses.Granted;
    public long? BenefitPolicyId { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public string BenefitDescription { get; set; } = string.Empty;
    public decimal? BenefitAmount { get; set; }
    public string CurrencyCode { get; set; } = "KRW";
    public DateTimeOffset ParticipatedAtUtc { get; set; }
    public DateTimeOffset? GrantedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public DateTimeOffset RecordedAtUtc { get; set; }
    public string Memo { get; set; } = string.Empty;
}

public sealed class HrParticipationBenefitRecordListResponse
{
    public IReadOnlyList<HrParticipationBenefitRecordResponse> Items { get; set; } = [];
}

public sealed class HrParticipationBenefitTransferRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string SourceType { get; set; } = HrParticipationSourceTypes.OfflineMeeting;
    public string SourceId { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
    public string ParticipationStatus { get; set; } = HrParticipationStatuses.Confirmed;
    public long? BenefitPolicyId { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public string BenefitDescription { get; set; } = string.Empty;
    public decimal? BenefitAmount { get; set; }
    public string CurrencyCode { get; set; } = "KRW";
    public DateTimeOffset? ParticipatedAtUtc { get; set; }
    public DateTimeOffset? GrantedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
}

public static class HrParticipationSourceTypes
{
    public const string EducationCourse = "EducationCourse";
    public const string OfflineMeeting = "OfflineMeeting";
    public const string PlatformCommunityEvent = "PlatformCommunityEvent";
}

public static class HrParticipationStatuses
{
    public const string Registered = "Registered";
    public const string Attended = "Attended";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
}

public static class HrParticipationBenefitStatuses
{
    public const string Pending = "Pending";
    public const string Granted = "Granted";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}
