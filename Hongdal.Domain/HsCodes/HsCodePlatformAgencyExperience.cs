namespace Hongdal.Domain.HsCodes;

public sealed class HsCodePlatformAgencyExperience
{
    public long Id { get; set; }

    public string HsCode { get; set; } = string.Empty;

    public string AgencyType { get; set; } = string.Empty;

    public string CountryRoute { get; set; } = string.Empty;

    public string CaseStatus { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string RequiredDocumentsJson { get; set; } = "[]";

    public string ContributorUserId { get; set; } = string.Empty;

    public bool ContributorConsented { get; set; }

    public bool IsPaidDetail { get; set; }

    public decimal PaidAccessPrice { get; set; }

    public decimal ContributorRewardRate { get; set; }

    public string DisclosurePolicy { get; set; } = string.Empty;

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
