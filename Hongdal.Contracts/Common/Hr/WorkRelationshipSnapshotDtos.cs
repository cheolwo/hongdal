namespace Hongdal.Contracts.Common.Hr;

public sealed class WorkRelationshipSnapshotRecordRequest
{
    public string WorkDomain { get; set; } = string.Empty;
    public string WorkProcess { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
    public string RelatedEntityType { get; set; } = string.Empty;
    public string RelatedEntityId { get; set; } = string.Empty;
    public string RelatedDisplayLabel { get; set; } = string.Empty;
    public string? CounterpartyUserId { get; set; }
    public string? CounterpartyRoleCode { get; set; }
    public string PrivacyLevel { get; set; } = "ActorVisibleAnonymized";
    public string Memo { get; set; } = string.Empty;
}

public sealed class WorkRelationshipSnapshotResponse
{
    public Guid Id { get; set; }
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
    public string? CounterpartyAnonymousLabel { get; set; }
    public string? CounterpartyRoleCode { get; set; }
    public string PrivacyLevel { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}

public sealed class WorkRelationshipSnapshotListResponse
{
    public IReadOnlyList<WorkRelationshipSnapshotResponse> Items { get; set; } = [];
}

public static class WorkRelationshipDomains
{
    public const string Warehouse = "Warehouse";
    public const string Dispatch = "Dispatch";
    public const string Customs = "Customs";
    public const string Commerce = "Commerce";
}

public static class WorkRelationshipProcesses
{
    public const string Inbound = "Inbound";
    public const string Inventory = "Inventory";
    public const string Packing = "Packing";
    public const string Dispatch = "Dispatch";
    public const string DriverAssignment = "DriverAssignment";
    public const string CustomsDelegation = "CustomsDelegation";
}
