namespace Ssalddel.Domain.PublicData.Korea;

public static class 건축물용도CategoryCodes
{
    public const string Residential = "residential";
    public const string Agriculture = "agriculture";
    public const string LogisticsStorage = "logistics-storage";
    public const string Commercial = "commercial";
    public const string BusinessOffice = "business-office";
    public const string PublicCommunity = "public-community";
    public const string Industrial = "industrial";
    public const string EducationResearch = "education-research";
    public const string MedicalWelfare = "medical-welfare";
    public const string CultureTourism = "culture-tourism";
    public const string Transport = "transport";
    public const string UtilityInfrastructure = "utility-infrastructure";
    public const string Religious = "religious";
    public const string Other = "other";
    public const string Unresolved = "unresolved";
}

public static class 건축물분류EvidenceKindCodes
{
    public const string Observed = "Observed";
    public const string Derived = "Derived";
    public const string ManualReview = "ManualReview";
    public const string Unresolved = "Unresolved";
}

public sealed class 건축물용도CategoryDefinition
{
    public string CategoryCode { get; set; } = string.Empty;
    public string DisplayNameKo { get; set; } = string.Empty;
    public string DescriptionKo { get; set; } = string.Empty;
    public string WorldRoleCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool PresentationEligible { get; set; }
}

public sealed class 건축물대장표제부Record
{
    public Guid Id { get; set; }
    public string RegisterManagementPk { get; set; } = string.Empty;
    public string RegisterKindCode { get; set; } = string.Empty;
    public string? RegisterTypeCode { get; set; }
    public string SigunguCode { get; set; } = string.Empty;
    public string LegalDongCode { get; set; } = string.Empty;
    public string? LandLot { get; set; }
    public string? RoadAddress { get; set; }
    public string? NormalizedRoadAddressKey { get; set; }
    public string? BuildingName { get; set; }
    public string? DongName { get; set; }
    public string? MainPurposeCode { get; set; }
    public string? MainPurposeName { get; set; }
    public string? StructureCode { get; set; }
    public string? StructureName { get; set; }
    public decimal? BuildingAreaSquareMeters { get; set; }
    public decimal? TotalFloorAreaSquareMeters { get; set; }
    public decimal? SiteAreaSquareMeters { get; set; }
    public decimal? OfficialBuildingCoveragePercent { get; set; }
    public decimal? OfficialFloorAreaRatioPercent { get; set; }
    public decimal? HeightMeters { get; set; }
    public int? AboveGroundFloorCount { get; set; }
    public int? UndergroundFloorCount { get; set; }
    public DateOnly? ApprovalDate { get; set; }
    public string SourceRevision { get; set; } = string.Empty;
    public long EvidenceSnapshotId { get; set; }
    public DateTimeOffset ObservedAtUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
}

public sealed class 건축물행정구역Assignment
{
    public Guid Id { get; set; }
    public Guid BuildingRecordId { get; set; }
    public 건축물대장표제부Record BuildingRecord { get; set; } = null!;
    public string LegalRegionStableId { get; set; } = string.Empty;
    public string? AdministrativeRegionStableId { get; set; }
    public string AssignmentMethodCode { get; set; } = string.Empty;
    public string ConfidenceCode { get; set; } = string.Empty;
    public string SourceVintage { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public DateTimeOffset ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
}

public sealed class 건축물용도CategoryAssignment
{
    public Guid Id { get; set; }
    public Guid BuildingRecordId { get; set; }
    public 건축물대장표제부Record BuildingRecord { get; set; } = null!;
    public string CategoryCode { get; set; } = string.Empty;
    public 건축물용도CategoryDefinition Category { get; set; } = null!;
    public bool IsPrimary { get; set; } = true;
    public string AssignmentMethodCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string? SourceMainPurposeCode { get; set; }
    public string? SourceMainPurposeName { get; set; }
    public DateTimeOffset ClassifiedAtUtc { get; set; }
}

public sealed class 행정동건축물CategoryAggregate
{
    public Guid Id { get; set; }
    public string AdministrativeRegionStableId { get; set; } = string.Empty;
    public string SourceVintage { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public 건축물용도CategoryDefinition Category { get; set; } = null!;
    public long BuildingCount { get; set; }
    public decimal BuildingAreaSquareMeters { get; set; }
    public decimal TotalFloorAreaSquareMeters { get; set; }
    public long NamedBuildingCount { get; set; }
    public long GeometryLinkedCount { get; set; }
    public long UnresolvedBuildingCount { get; set; }
    public string EvidenceKindCode { get; set; } = 건축물분류EvidenceKindCodes.Derived;
    public string RuleRevision { get; set; } = string.Empty;
    public string AggregateHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
}
