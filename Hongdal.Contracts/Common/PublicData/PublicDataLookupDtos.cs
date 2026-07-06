namespace Hongdal.Contracts.Common.PublicData;

public sealed class PublicDataLookupResponse<TItem>
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int? TotalCount { get; init; }

    public IReadOnlyList<TItem> Items { get; init; } = [];
}

public sealed class RoadAddressSearchRequest
{
    public string Keyword { get; init; } = string.Empty;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed class RoadAddressItem
{
    public string RoadAddress { get; init; } = string.Empty;

    public string JibunAddress { get; init; } = string.Empty;

    public string ZipCode { get; init; } = string.Empty;

    public string AdministrativeCode { get; init; } = string.Empty;

    public string RoadNameManagementNo { get; init; } = string.Empty;

    public string BuildingManagementNo { get; init; } = string.Empty;

    public string? RelatedJibun { get; init; }

    public string? EnglishAddress { get; init; }
}

public sealed class ApartmentComplexSearchRequest
{
    public string? SidoCode { get; init; }

    public string? SigunguCode { get; init; }

    public string? EupmyeondongCode { get; init; }

    public string? RoadName { get; init; }

    public string? Keyword { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed class ApartmentComplexItem
{
    public string ComplexCode { get; init; } = string.Empty;

    public string ComplexName { get; init; } = string.Empty;

    public string? Sido { get; init; }

    public string? Sigungu { get; init; }

    public string? Eupmyeondong { get; init; }

    public string? RoadAddress { get; init; }

    public string? LegalDongAddress { get; init; }
}

public sealed class ApartmentComplexBasicRequest
{
    public string ComplexCode { get; init; } = string.Empty;
}

public sealed class ApartmentComplexBasicItem
{
    public string ComplexCode { get; init; } = string.Empty;

    public string ComplexName { get; init; } = string.Empty;

    public int? HouseholdCount { get; init; }

    public int? BuildingCount { get; init; }

    public string? ManagementType { get; init; }

    public string? HeatingType { get; init; }

    public string? ApprovalDate { get; init; }

    public string? RoadAddress { get; init; }

    public string? LegalDongAddress { get; init; }
}
