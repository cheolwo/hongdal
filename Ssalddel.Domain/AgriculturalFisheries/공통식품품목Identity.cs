namespace Ssalddel.Domain.AgriculturalFisheries;

public sealed class 공통식품품목Identity
{
    public long Id { get; set; }

    public string CanonicalProductStableId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Revision { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<공통식품품목Code관계> CodeRelations { get; set; } =
        new List<공통식품품목Code관계>();
}

public sealed class 공통식품품목Code관계
{
    public long Id { get; set; }

    public long ProductIdentityId { get; set; }

    public 공통식품품목Identity? ProductIdentity { get; set; }

    public string RelationStableId { get; set; } = string.Empty;

    public string SourceKey { get; set; } = string.Empty;

    public string CodeScheme { get; set; } = string.Empty;

    public string? ExternalCode { get; set; }

    public string? ParentCode { get; set; }

    public string Label { get; set; } = string.Empty;

    public string RelationStatusCode { get; set; } = string.Empty;

    public string MatchQualityCode { get; set; } = string.Empty;

    public string EvidenceNote { get; set; } = string.Empty;

    public int Revision { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<공통식품품목Code관계검토이력> ReviewHistory { get; set; } =
        new List<공통식품품목Code관계검토이력>();
}

public sealed class 공통식품품목Code관계검토이력
{
    public long Id { get; set; }

    public long CodeRelationId { get; set; }

    public 공통식품품목Code관계? CodeRelation { get; set; }

    public int Revision { get; set; }

    public string RelationStatusCode { get; set; } = string.Empty;

    public string? ExternalCode { get; set; }

    public string ReviewActionCode { get; set; } = string.Empty;

    public string ReviewReason { get; set; } = string.Empty;

    public string ReviewedBySubjectId { get; set; } = string.Empty;

    public DateTime ReviewedAtUtc { get; set; }
}
