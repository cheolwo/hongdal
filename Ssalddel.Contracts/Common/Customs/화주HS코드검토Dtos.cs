namespace Ssalddel.Contracts.Common.Customs;

/// <summary>
/// 화주·판매자가 검토할 수 있는 활성 HS 코드 후보 목록입니다.
/// 전문 분류 확정이나 신고 결과를 표현하지 않습니다.
/// </summary>
public sealed class 화주HS코드검토목록응답
{
    public IReadOnlyList<화주HS코드검토항목응답> Items { get; init; } = [];

    public int TotalCount { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 30;
}

public sealed class 화주HS코드검토항목응답
{
    public long ReviewId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string NormalizedCode { get; init; } = string.Empty;

    public string KoreanName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int Level { get; init; }

    public string LevelLabel { get; init; } = string.Empty;

    public int BusinessCategory { get; init; }

    public string BusinessCategoryLabel { get; init; } = string.Empty;

    public string RiskLevelCode { get; init; } = string.Empty;

    public string RiskLevelLabel { get; init; } = string.Empty;

    public bool BrokerReviewRecommended { get; init; }

    public IReadOnlyList<string> RiskTagLabels { get; init; } = [];

    public int OfficialCaseCount { get; init; }

    public int CustomsAgencyExperienceCount { get; init; }

    public int ImportAgencyExperienceCount { get; init; }

    public 화주HS코드검토출처응답 Source { get; init; } = new();
}

public sealed class 화주HS코드검토상세응답
{
    public 화주HS코드검토항목응답 Item { get; init; } = new();

    public IReadOnlyList<화주HS코드주의태그응답> RiskTags { get; init; } = [];

    public IReadOnlyList<화주HS코드공식분류사례응답> OfficialCases { get; init; } = [];

    public IReadOnlyList<화주HS코드공개대행경험응답> AgencyExperiences { get; init; } = [];

    public IReadOnlyList<string> RequiredDocuments { get; init; } = [];

    public string DecisionBoundary { get; init; } =
        "이 자료는 HS 코드 후보와 확인 항목을 제공하며 관세사의 최종 품목분류, 세율 확정 또는 세관 신고를 대신하지 않습니다.";
}

public sealed class 화주HS코드검토출처응답
{
    public string StandardCode { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public int CodeDigits { get; init; }

    public string Revision { get; init; } = string.Empty;

    public string SourceName { get; init; } = string.Empty;

    public string SourceUrl { get; init; } = string.Empty;

    public DateTime? EffectiveFrom { get; init; }

    public DateTime? EffectiveTo { get; init; }

    public DateTime? ImportedAtUtc { get; init; }
}

public sealed class 화주HS코드주의태그응답
{
    public int TagType { get; init; }

    public string Label { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public string SourceLabel { get; init; } = string.Empty;
}

public sealed class 화주HS코드공식분류사례응답
{
    public long CaseId { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public string SourceType { get; init; } = string.Empty;

    public string SourceReferenceNo { get; init; } = string.Empty;

    public string SourceUrl { get; init; } = string.Empty;

    public string IssuingAuthority { get; init; } = string.Empty;

    public DateTime? DecidedAt { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string GoodsDescription { get; init; } = string.Empty;

    public string DecisionReason { get; init; } = string.Empty;
}

public sealed class 화주HS코드공개대행경험응답
{
    public long ExperienceId { get; init; }

    public string AgencyType { get; init; } = string.Empty;

    public string AgencyTypeLabel { get; init; } = string.Empty;

    public string CountryRoute { get; init; } = string.Empty;

    public string CaseStatus { get; init; } = string.Empty;

    public string RiskLevel { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public IReadOnlyList<string> RequiredDocuments { get; init; } = [];

    public string DisclosurePolicy { get; init; } = string.Empty;

    public DateTime? CompletedAtUtc { get; init; }
}
