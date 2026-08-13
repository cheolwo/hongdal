namespace Ssalddel.Domain.PublicData.Korea;

public static class 공개사업장연결방법Codes
{
    public const string 정확한정규화도로명주소 = "ExactNormalizedRoadAddress";
    public const string 건물도형내좌표 = "PointInBuildingFootprint";
    public const string 수동검토 = "ManualReview";
}

public static class 공개사업장연결상태Codes
{
    public const string 연결됨 = "Matched";
    public const string 복수후보 = "MultipleCandidates";
    public const string 건물후보없음 = "NoBuildingCandidate";
    public const string 주소부족 = "InsufficientAddress";
}

public sealed class 공개인허가사업장Record
{
    public Guid Id { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string SourceDatasetId { get; set; } = string.Empty;
    public string OpenServiceId { get; set; } = string.Empty;
    public string? OpenServiceName { get; set; }
    public string ManagementNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessTypeName { get; set; }
    public string? LicenseCategoryName { get; set; }
    public string? BusinessStatusCode { get; set; }
    public string? BusinessStatusName { get; set; }
    public string? DetailedStatusCode { get; set; }
    public string? DetailedStatusName { get; set; }
    public string? LotAddress { get; set; }
    public string? RoadAddress { get; set; }
    public string? NormalizedRoadAddressKey { get; set; }
    public decimal? SourceCoordinateX { get; set; }
    public decimal? SourceCoordinateY { get; set; }
    public string? SourceCoordinateReferenceSystem { get; set; }
    public DateOnly? LicenseDate { get; set; }
    public DateOnly? ClosureDate { get; set; }
    public DateTimeOffset? SourceLastModifiedAt { get; set; }
    public string SourceRevision { get; set; } = string.Empty;
    public string SourceHashSha256 { get; set; } = string.Empty;
    public long? EvidenceSnapshotId { get; set; }
    public DateTimeOffset ObservedAtUtc { get; set; }
}

public sealed class 공개사업장건축물Assignment
{
    public Guid Id { get; set; }
    public Guid BusinessRecordId { get; set; }
    public 공개인허가사업장Record BusinessRecord { get; set; } = null!;
    public Guid? BuildingRecordId { get; set; }
    public 건축물대장표제부Record? BuildingRecord { get; set; }
    public string AssignmentStatusCode { get; set; } = string.Empty;
    public string? AssignmentMethodCode { get; set; }
    public string ConfidenceCode { get; set; } = string.Empty;
    public int CandidateBuildingCount { get; set; }
    public string RuleRevision { get; set; } = string.Empty;
    public DateTimeOffset EvaluatedAtUtc { get; set; }
}

public sealed class 건축물공개사업장Aggregate
{
    public Guid Id { get; set; }
    public Guid BuildingRecordId { get; set; }
    public 건축물대장표제부Record BuildingRecord { get; set; } = null!;
    public string SourceRevision { get; set; } = string.Empty;
    public int TotalBusinessCount { get; set; }
    public int OpenBusinessCount { get; set; }
    public int SuspendedBusinessCount { get; set; }
    public int ClosedBusinessCount { get; set; }
    public int UnresolvedStatusCount { get; set; }
    public string EvidenceKindCode { get; set; } = 건축물분류EvidenceKindCodes.Derived;
    public string RuleRevision { get; set; } = string.Empty;
    public string AggregateHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
}
