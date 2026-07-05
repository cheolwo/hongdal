namespace Hongdal.Domain.HumanResources;

public sealed class WorkRelationshipSnapshotRecord
{
    public Guid Id { get; set; }

    public string ActorUserId { get; set; } = string.Empty;

    public string ActorAnonymousLabel { get; set; } = string.Empty;

    public string ActorRoleCode { get; set; } = string.Empty;

    public string ActorRoleName { get; set; } = string.Empty;

    public string WorkDomain { get; set; } = string.Empty;

    public string WorkProcess { get; set; } = string.Empty;

    public string ActionCode { get; set; } = string.Empty;

    public string ActionLabel { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public string RelatedEntityId { get; set; } = string.Empty;

    public string RelatedDisplayLabel { get; set; } = string.Empty;

    public string? CounterpartyUserId { get; set; }

    public string? CounterpartyAnonymousLabel { get; set; }

    public string? CounterpartyRoleCode { get; set; }

    public string PrivacyLevel { get; set; } = WorkRelationshipPrivacyLevels.PrivateInternal;

    public string Memo { get; set; } = string.Empty;

    public string AppKey { get; set; } = string.Empty;

    public string TraceId { get; set; } = string.Empty;

    public string ClientIpSnapshot { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class WorkRelationshipPrivacyLevels
{
    public const string PrivateInternal = "PrivateInternal";
    public const string ActorVisibleAnonymized = "ActorVisibleAnonymized";
    public const string ConnectionRequestEligible = "ConnectionRequestEligible";
}
